using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.S3;
using Foundation;
using Foundation.AssetBundles;
using FoundationEditor.AWSUtils.Editor;
using Foundation.ClientService;
using Foundation.ConfigurationResolver;
using Foundation.Logger;
using Foundation.ServicesResolver;
using Foundation.Utils.Extensions;
using Foundation.Utils.OperationUtils;
using Foundation.Utils.SerializerUtils;
using FoundationEditor.Utils.Editor;
using FoundationEditor.Utils.Editor.BuildUtils;
using FoundationEditor.Utils.MD5.Editor;
using UnityEditor;
using UnityEngine;
using Directory = System.IO.Directory;

namespace FoundationEditor.BuildPipelineService.Editor.AssetsBuild
{
    public class AssetBundlesBuilder : IBuilder
    {

        private const string ManifestFileExtension = ".manifest";
        private const string LocalAssetBundlesFolder = "LocalAssetBundles";
        
        private BuildConfig _buildConfig;
        private AssetBundlesConfig _config;
        private ServiceResolver _serviceResolver;
        private ConfigResolver _configResolver;
        private S3Service _s3Service;
        
        private BuildAssetBundleOptions _bundleOptions = BuildAssetBundleOptions.StrictMode;
        private PlatformType _platformType = PlatformType.Unknown;

        private string _localBundlesFolder;
        private string _cloudStoragePath;
        private BundleManifest _bundleManifest;
        private BundleManifest _lastBundleManifest;
        private BundlesManifestVersion _manifestVersion;
        private ISerializer _serializer;

        public void Build(BuildConfig buildConfig, ServiceResolver serviceResolver, ConfigResolver configResolver, Action<Result> onComplete)
        {
            _buildConfig = buildConfig;
            _serviceResolver = serviceResolver;
            
            _serializer = new UnitySerializer();
            _configResolver = configResolver;
            _config = configResolver.GetConfig<AssetBundlesConfig>();
            _s3Service = _serviceResolver.Resolve<S3BatchModeService>();

            _platformType = buildConfig.BuildTarget.ToPlatformType();
            
            string clientVersionFolder = $"{ClientVersionUtils.Version}{Path.DirectorySeparatorChar}";
            string platformFolder = $"{_platformType}{Path.DirectorySeparatorChar}";
            _localBundlesFolder = Path.Combine(LocalAssetBundlesFolder, platformFolder);
            _cloudStoragePath = Path.Combine("AssetBundles", Path.Combine(platformFolder, clientVersionFolder));
            
            StartBuild(onComplete);
        }

        private void StartBuild(Action<Result> onComplete)
        {
            OperationsQueue.
            Do(ValidateSettings).
            Then<AssetBundleManifest>(BuildAssetBundles).
            Then<AssetBundleManifest>(BuildBundleManifest).
            Then(EmbedManifestVersion).
            Then(EmbedAssetsToClientBuild).
            Then(DownloadManifestVersion).
            Then(DownloadCurrentBundleManifest).
            Then(UpdateManifestVersion).
            Then(UploadBundles).
            Then(SaveConfigFile).
            Finally(onComplete).
            Run();
        }

        private void SaveConfigFile(Action<Result> operationComplete)
        {
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();
            operationComplete.Invoke(Result.Successful);
        }

        #region Build Assets & initial settings validation
        
        private void BuildAssetBundles(Action<Result<AssetBundleManifest>> operationComplete)
        {
            // Start Unity's Asset build pipeline & verify folder exists and cleared
            EditorBuildUtils.CreateFolder(_localBundlesFolder, true);
            AssetBundleManifest unityManifest = BuildPipeline.BuildAssetBundles(_localBundlesFolder, _bundleOptions, _buildConfig.BuildTarget);
            // config can be destroyed after the "build asset" command when Unity re-compiles so we need to re-load it
            _config = _configResolver.GetConfig<AssetBundlesConfig>();
            
            // Create result object & invoke callback..
            Result<AssetBundleManifest> result = new Result<AssetBundleManifest>() { Data = unityManifest};
            result += ValidateUnityManifest(unityManifest);
            operationComplete.Invoke(result);
        }
        
        private Result ValidateUnityManifest(AssetBundleManifest manifest)
        {
            Result result = new Result();
            if (manifest == null)
            {
                result.SetFailure("Build asset bundles failed (manifest is null)");
                return result;
            }
            
            string[] bundles = manifest.GetAllAssetBundles();
            if (bundles == null || bundles.Length == 0)
            {
                result.SetFailure("Project contains no bundles to build");
            }

            return result;
        }
        
        private void ValidateSettings(Action<Result> operationComplete)
        {
            Result result = new Result();
            if (_platformType == PlatformType.All || _platformType == PlatformType.Unknown)
            {
                result.SetFailure($"Platform type is not supported: {_platformType}");
            }

            if (!(_s3Service is S3BatchModeService))
            {
                result.SetFailure("Asset Bundle Builder must receive a S3BatchModeService dependency in service resolver!");
            }

            operationComplete.Invoke(result);
        }
        
        #endregion
        
        #region Build Hash based Manifest & Bundles
        
        private void BuildBundleManifest(AssetBundleManifest unityManifest, Action<Result> operationComplete)
        {
            Result result = new Result();
            _bundleManifest = new BundleManifest();

            // Hash bundles
            result += BuildHashedBundles(_bundleManifest, unityManifest);
            
            // Hash unity's manifest
            Result<string> hashManifestResult = HashUnityManifest();
            _bundleManifest.Hash = hashManifestResult.Data;
            result += hashManifestResult;

            // Finally, save serialize our bundle manifest and save it as a file..
            string manifestPath = Path.Combine(_localBundlesFolder, BundleManifestUtils.GetBundleManifestHashedFileName(_bundleManifest.Hash));
            Result serializeResult = BundleManifestUtils.SerializeBundleManifest(_bundleManifest, manifestPath);
            result += serializeResult;
            
            operationComplete.Invoke(result);
        }
        
        private Result BuildHashedBundles(BundleManifest bundleManifest, AssetBundleManifest unityManifest)
        {
            // Iterate over all bundles - do not stop immediately on errors to get aggregated data on all bundles
            Result result = new Result();
            string[] bundleNames = unityManifest.GetAllAssetBundles();
            for (int i = 0; i < bundleNames.Length; i++)
            {
                string bundleName = bundleNames[i];
                result += BuildHashedBundle(bundleName, bundleManifest, unityManifest);
            }

            return result;
        }

        private Result BuildHashedBundle(string bundleName, BundleManifest bundleManifest, AssetBundleManifest unityManifest)
        {
            Result result = new Result();
            Hash128 bundleHash = unityManifest.GetAssetBundleHash(bundleName);
            if (!bundleHash.isValid)
            {
                // Check hash only for current bundle (dependencies will be checked later when iterating over them)
                result.SetFailure("Hash128 is not valid for bundle: " + bundleName + ": " + bundleHash);
            }
            // Rename actual bundle file with the hashed name
            string hashedBundleName = RenameBundleWithHash(bundleName, bundleHash.ToString());
                
            // Get all direct bundle dependencies (otherwise it can potentially highly increase manifest size)
            string[] dependencies = unityManifest.GetDirectDependencies(bundleName);
            string[] hashedDependencies = GetHashedBundleDependencies(dependencies, unityManifest);
                
            // Get all files in bundles
            string[] assetsPath = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
            string[] assetNames = new string[assetsPath.Length];
            for (int i = 0; i < assetsPath.Length; i++)
            {
                assetNames[i] = Path.GetFileName(assetsPath[i]);
            }
            
            bool isEmbedded = _config.IsBundleEmbedded(bundleName);
            Bundle.States bundleState = isEmbedded ? Bundle.States.Embedded : Bundle.States.RequireDownload;
            Bundle bundle = new Bundle(hashedBundleName, hashedDependencies, bundleState);
            // Finally, add the bundle to manifest
            result += bundleManifest.AddBundle(bundle, assetNames);
            return result;
        }
        
        private string[] GetHashedBundleDependencies(string[] dependencies, AssetBundleManifest unityManifest)
        {
            string[] hashedDependencies = new string[dependencies.Length];
            for (int j = 0; j < dependencies.Length; j++)
            {
                string dependency = dependencies[j];
                Hash128 dependencyHash = unityManifest.GetAssetBundleHash(dependency);
                string hashedName = BundleManifestUtils.GetHashedName(dependency, dependencyHash.ToString());
                hashedDependencies[j] = hashedName;
            }
            return hashedDependencies;
        }

        private Result<string> HashUnityManifest()
        {
            Result<string> result = new Result<string>();
            // Create MD5 hash based on Unity's manifest file
            string unityManifestFilePath = GetUnityManifestFilePath();
            string md5Hash = MD5Utils.CreateMD5Hash(unityManifestFilePath);
            bool hashValid = MD5Utils.VerifyMD5Hash(unityManifestFilePath, md5Hash);
            if (hashValid)
            {
                result.Data = md5Hash;
            }
            else
            {
                result.SetFailure($"Unity Manifest MD5 Hash is not valid: {md5Hash}");
            }
            RenameBundleWithHash(GetUnityManifestFileName(), md5Hash);
            
            this.Log($"Manifest Name: {unityManifestFilePath} hash: {md5Hash} is valid: {hashValid}");
            return result;
        }
        
        private void EmbedManifestVersion(Action<Result> operationComplete)
        {
            BundlesManifestVersion bundlesManifestVersion = new BundlesManifestVersion();
            bundlesManifestVersion.ManifestHash = _bundleManifest.Hash;
            bundlesManifestVersion.BuildId = _buildConfig.BuildId;
            _config.EmbeddedManifestVersion = bundlesManifestVersion;
            operationComplete.Invoke(Result.Successful);
        }
        
        #endregion

        #region Manifest Version (Download & Update)
        
        private void DownloadCurrentBundleManifest(Action<Result> operationComplete)
        {
            string manifestName = BundleManifestUtils.GetBundleManifestHashedFileName(_manifestVersion.ManifestHash);
            string url = Path.Combine(_cloudStoragePath, manifestName);
            _s3Service.DownloadBytes(url, delegate(Result<byte[]> downloadResult)
            {
                if (downloadResult.Success)
                {
                    Result<BundleManifest> deserializeResult = BundleManifestUtils.DeSerializeBundleManifest(downloadResult.Data);
                    _lastBundleManifest = deserializeResult.Data;
                }

                // Always send a successful result even if download or deserialization failed
                // In case of failure in this step flow can proceed normally (for example in the first time we build bundles)
                operationComplete.Invoke(Result.Successful);
            });
        }

        private void DownloadManifestVersion(Action<Result> operationComplete)
        {
            string finalManifestPath =  _cloudStoragePath + _config.ManifestVersionFileName;
            _s3Service.DownloadFile(finalManifestPath, delegate(Result<string> result)
            {
                string manifestContent = result.Success ? result.Data : string.Empty;
                OnManifestVersionDownloaded(manifestContent, operationComplete);
            });
        }

        private void OnManifestVersionDownloaded(string manifestContent, Action<Result> operationComplete)
        {
            Result result = new Result();
            if (string.IsNullOrEmpty(manifestContent))
            {
                this.Log("No manifest version found for platform / version - create new manifest version!");
                _manifestVersion = new BundlesManifestVersion()
                {
                    ManifestHash = ClientVersionUtils.Version,
                    BuildId = 0
                };
            }
            else
            {
                _manifestVersion = _serializer.DecodeJson<BundlesManifestVersion>(manifestContent);
                this.Log("Successfully decoded manifest version: " + (_manifestVersion != null));
            }

            operationComplete.Invoke(result);
        }

        private void UpdateManifestVersion(Action<Result> operationComplete)
        {
            Result result = new Result();
            if (_manifestVersion.BuildId >= _buildConfig.BuildId)
            {
                if (_buildConfig.Environment == EnvironmentConfig.Environment.Prod)
                {
                    result.SetFailure($"A manifest with higher build number already exists: {_manifestVersion.BuildId} current build ID: {_buildConfig.BuildId}");
                    operationComplete.Invoke(result);
                    return;
                }
            }

            // Update manifest version with current build number & hash
            _manifestVersion.BuildId = _buildConfig.BuildId;
            _manifestVersion.ManifestHash = _bundleManifest.Hash;

            // Serialize & upload manifest version to S3.
            string json = _serializer.EncodeJson(_manifestVersion);
            _s3Service.UploadFileFromMemory(json, _config.ManifestVersionFileName, _cloudStoragePath, S3CannedACL.PublicRead, delegate(Result uploadResult)
            {
                operationComplete.Invoke(uploadResult);
            });
        }

        #endregion

        #region Upload Bundles & Hashed Manifest

        private void UploadBundles(Action<Result> operationComplete)
        {
            List<string> allFiles = Directory.GetFiles(_localBundlesFolder).ToList();
            List<string> files = new List<string>();
            foreach (string file in allFiles)
            {
                string extension = Path.GetExtension(file);
                if(extension == ManifestFileExtension) { continue; }

                string bundleName = Path.GetFileNameWithoutExtension(file);
                bool uploadBundle = true;
                if (_lastBundleManifest != null)
                {
                    Result result = _lastBundleManifest.GetBundle(bundleName);
                    // If bundle already exists in previous manifest, no need to re-upload it...
                    if (result.Success) { uploadBundle = false; }
                }

                if (uploadBundle)
                {
                    this.Log($"Preparing to upload bundle: {file}");
                    files.Add(file);
                }
            }
            _s3Service.UploadFiles(files, _cloudStoragePath, S3CannedACL.PublicRead, delegate(Result result)
            {
                this.Log("Finish uploading all files to S3 - success: " + result.Success);
                operationComplete.Invoke(result);
            });
        }
        
        #endregion

        private void EmbedAssetsToClientBuild(Action<Result> operationComplete)
        {
            if (_buildConfig.BuildType == BuildService.BuildType.Assets && _buildConfig.IsBatchMode)
            {
                this.Log("Skipping embedding bundles in Assets Batch mode build!");
                operationComplete.Invoke(Result.Successful);
                return;
            }

            // Get embedding folder path & verify folder exists
            string embeddedFilesFolderPath = Path.Combine(Application.streamingAssetsPath, AssetBundlesService.EmbeddedAssetsFolderName);
            EditorBuildUtils.CreateFolder(embeddedFilesFolderPath, true);
            
            // Embed bundles & manifest
            EmbedBundles(embeddedFilesFolderPath);
            EmbedManifest(embeddedFilesFolderPath);

            operationComplete.Invoke(Result.Successful);
        }
        
        private void EmbedManifest(string embeddedFilesFolderPath)
        {
            string manifestName = BundleManifestUtils.GetBundleManifestHashedFileName(_bundleManifest.Hash);
            string unityManifestFilePath = Path.Combine(_localBundlesFolder, manifestName);
            string embedManifestPath = Path.Combine(embeddedFilesFolderPath, manifestName);
            this.Log($"Embed manifest from {unityManifestFilePath} to {embedManifestPath}");
            File.Copy(unityManifestFilePath, embedManifestPath);
        }
        
        private void EmbedBundles(string embeddedFilesFolderPath)
        {
            string localBuildPath = _localBundlesFolder;
            foreach (Bundle bundle in _bundleManifest.BundlesMap.Values)
            {
                if(bundle.State != Bundle.States.Embedded) { continue; }

                string bundlePath = Path.Combine(localBuildPath, bundle.BundleName);
                string embedBundlePath = Path.Combine(embeddedFilesFolderPath, bundle.BundleName);
                File.Copy(bundlePath, embedBundlePath);
            }
        }

        #region Helper Methods
        
        private string GetUnityManifestFilePath()
        {
            return Path.Combine(_localBundlesFolder, GetUnityManifestFileName());
        }

        /// <summary>
        /// Get Unity's bundle manifest file name
        /// It's named by Unity as the containing folder name...
        /// </summary>
        /// <returns></returns>
        private string GetUnityManifestFileName()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(_localBundlesFolder.TrimEnd(Path.DirectorySeparatorChar));
            return directoryInfo.Name + ManifestFileExtension;
        }
        
        /// <summary>
        /// Renames the bundle file to [name]~[hash].[extension] and returns the hashed name
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="bundleHash"></param>
        /// <returns></returns>
        public string RenameBundleWithHash(string bundleName, string bundleHash)
        {
            string bundleLocalPath = Path.Combine(_localBundlesFolder, bundleName);
            FileInfo fileInfo = new FileInfo(bundleLocalPath);
            string hashedName = Path.GetFileNameWithoutExtension(bundleLocalPath) + BundleManifestUtils.HashSeparator + bundleHash + fileInfo.Extension;
            fileInfo.Rename(hashedName);
            return hashedName;
        }
        
        #endregion
    }
}
