using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Foundation.Logger;
using Foundation.Network;
using Foundation.ServicesResolver;
using Foundation.Utils.OperationUtils;
using UnityEngine;
using ISerializer = Foundation.Utils.SerializerUtils.ISerializer;
using BundlesMode = Foundation.AssetBundles.AssetBundlesService.BundlesMode;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Foundation.AssetBundles
{
    internal class BundleManifestInitializer
    {
        
        private const string CachedManifestVersionKey = "CachedManifestVersion";
        private const string CachedBundleManifestKey = "CachedBundleManifest";
        
        private readonly IAssetBundlesService _assetBundlesService;
        private readonly IHttpService _downloadService;
        private readonly ISerializer _serializer;
        private readonly AssetBundlesConfig _config;
        private BundlesManifestVersion _manifestVersion;
        private BundlesManifestVersion _cachedManifestVersion;
        private BundleManifest _bundleManifest;

        private Action<Result<BundleManifest>> _onManifestReady;
        
        // Used only for faster simulation in Editor that by-pass building bundles every time.
        private BundlesMode _bundlesMode;
        
        internal BundleManifestInitializer(ServiceResolver serviceResolver, ISerializer serializer, AssetBundlesConfig config, BundlesMode bundlesMode)
        {
            _downloadService = serviceResolver.Resolve<IHttpService>();
            _assetBundlesService = serviceResolver.Resolve<IAssetBundlesService>();
            _serializer = serializer;
            _config = config;
            _bundlesMode = bundlesMode;
        }

        internal void Initialize(Action<Result<BundleManifest>> onManifestReady)
        {
            _onManifestReady = onManifestReady;

            if (_bundlesMode == BundlesMode.Remote)
            {
                OperationsQueue.
                Do(DownloadManifestVersion).
                And(LoadCachedManifestVersion).
                And(LoadCachedBundleManifest).
                Then(UpdateManifest).
                Finally(OnManifestVersionReady).
                Run("Initialize Manifest");
            }
            else
            {
                CreateManifest();
            }
        }
        
        #region Download & Load Manifest
        
        private void DownloadManifestVersion(Action<Result> operationComplete)
        {
            string url = Path.Combine(_config.CloudStorageBaseUrl, _assetBundlesService.CloudStoragePath, _config.ManifestVersionFileName);
            this.Log($"Download manifest version: {url}");

            _downloadService.GetBytes(url,delegate(APIResponse<byte[]> response)
            {
                this.Log($"Finished downloading manifest version: {url}");
                string json =  Encoding.UTF8.GetString(response.Data, 0, response.Data.Length);
                _manifestVersion = _serializer.DecodeJson<BundlesManifestVersion>(json);
                operationComplete.Invoke(response);
            });
        }

        private void LoadCachedManifestVersion(Action<Result> operationComplete)
        {
            string data = PlayerPrefs.GetString(CachedManifestVersionKey);
            _cachedManifestVersion = null;
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    _cachedManifestVersion = _serializer.DecodeJson<BundlesManifestVersion>(data);
                    this.Log("Successfully loaded & serialized cached manifest");
                }
                catch (Exception e)
                {
                    this.LogException(e);
                }
            }

            if (_cachedManifestVersion == null)
            {
                this.Log("Couldn't find cached manifest version - create manifest version based on embedded manifest version");
                _cachedManifestVersion = _config.EmbeddedManifestVersion;
                PlayerPrefs.SetString(CachedManifestVersionKey, _serializer.EncodeJson(_cachedManifestVersion));
            }
            
            this.Log($"Loaded cached manifest: {_cachedManifestVersion.ManifestHash} build number: {_cachedManifestVersion.BuildId}");
            
            operationComplete.Invoke(Result.Successful);
        }

        private void LoadCachedBundleManifest(Action<Result> operationComplete)
        {
            // First try load a cached bundle manifest
            Result<BundleManifest> bundleManifestResult;
            if (PlayerPrefs.HasKey(CachedBundleManifestKey))
            {
                string base64 = PlayerPrefs.GetString(CachedBundleManifestKey);
                if (!string.IsNullOrEmpty(base64))
                {
                    byte[] bytes = Convert.FromBase64String(base64);
                    bundleManifestResult = BundleManifestUtils.DeSerializeBundleManifest(bytes);
                    this.LogAssertion($"Deserialize cached bundle manifest success: {bundleManifestResult.Success}", bundleManifestResult.Success);
                    if (bundleManifestResult.Success)
                    {
                        _bundleManifest = bundleManifestResult.Data;
                        operationComplete.Invoke(Result.Successful);
                        return;
                    }
                }
            }
            
            #if UNITY_WEBGL
            // For webgl, just skip this step
            operationComplete.Invoke(Result.Successful);
            #else
            // Secondly, try loading the embedded bundle manifest
            string bundleManifestFileName = BundleManifestUtils.GetBundleManifestHashedFileName(_cachedManifestVersion.ManifestHash, true);
            string path = Path.Combine(_assetBundlesService.EmbeddedAssetsFolderPath, bundleManifestFileName);
            bundleManifestResult = BundleManifestUtils.DeSerializeBundleManifest(File.ReadAllBytes(path));
            this.LogAssertion($"Deserialize embedded bundle manifest success: {bundleManifestResult.Success}", bundleManifestResult.Success);

            _bundleManifest = bundleManifestResult.Data;
            operationComplete.Invoke(bundleManifestResult);
            #endif
        }
        
        private void UpdateManifest(Action<Result> operationComplete)
        {
            this.Log("Update manifest - start!");
            // If manifest is already up to date, just skip this step
            if (_manifestVersion == _cachedManifestVersion && _bundleManifest != null)
            {
                this.Log("Manifest is up to date & loaded from cache!");
                operationComplete.Invoke(Result.Successful);
                return;
            }
            
            this.Log($"Download new manifest: {_manifestVersion.ManifestHash} build number: {_manifestVersion.BuildId}");
            
            OperationsQueue.
            Do(DownloadBundleManifest).
            Then(CacheBundleManifest).
            Then(CacheManifestVersion).
            Finally(operationComplete).
            Run("Update Manifest");
        }

        private void DownloadBundleManifest(Action<Result> operationComplete)
        {
            string urlPrefix = Path.Combine(_config.CloudStorageBaseUrl, _assetBundlesService.CloudStoragePath);
            string url = Path.Combine(urlPrefix, BundleManifestUtils.GetBundleManifestHashedFileName(_manifestVersion.ManifestHash));
            this.Log($"Download new bundle manifest: {url}");
            _downloadService.GetBytes(url, delegate(APIResponse<byte[]> response)
            {
                Result result = new Result();
                byte[] bytes = response.Data;
                if (!response.Success)
                {
                    result.SetFailure("Failed downloading bundle manifest");
                    operationComplete.Invoke(result);
                    return;
                }
                
                Result<BundleManifest> deserializeManifestResult = BundleManifestUtils.DeSerializeBundleManifest(bytes);
                result += deserializeManifestResult;

                if (deserializeManifestResult.Success)
                {
                    BundleManifest oldManifest = _bundleManifest;
                    _bundleManifest = deserializeManifestResult.Data;
                    _bundleManifest.SyncManifest(oldManifest);
                    
                    PlayerPrefs.SetString(CachedBundleManifestKey, Convert.ToBase64String(bytes));
                }
                else
                {
                    result.SetFailure("Bundle manifest de-serialization failed (got null result)");
                }
                
                operationComplete.Invoke(result);
            });
        }

        private void CacheBundleManifest(Action<Result> operationComplete)
        {
            // Save new manifest to disk
            Directory.CreateDirectory(_assetBundlesService.CachedFolderPath);
            string manifestPath = Path.Combine(_assetBundlesService.CachedFolderPath, BundleManifestUtils.GetBundleManifestHashedFileName(_bundleManifest.Hash));
            Result result = BundleManifestUtils.SerializeBundleManifest(_bundleManifest, manifestPath);
            operationComplete.Invoke(result);
        }

        private void CacheManifestVersion(Action<Result> operationComplete)
        {
            // Save cached manifest version
            PlayerPrefs.SetString(CachedManifestVersionKey, _serializer.EncodeJson(_manifestVersion));
            operationComplete.Invoke(Result.Successful);
        }

        private void OnManifestVersionReady(Result operationResult)
        {
            // Save player prefs after initialization complete
            PlayerPrefs.Save();
            
            Result<BundleManifest> result = new Result<BundleManifest>();
            result += operationResult;
            result.Data = _bundleManifest;
            
            _onManifestReady?.Invoke(result);
        }
        
        #endregion
        
        #region Bundles Local Mode - Unity Simulation
        
        private void CreateManifest()
        {
            #if UNITY_EDITOR

            Stopwatch stopwatch = Stopwatch.StartNew();
            Result<BundleManifest> result = new Result<BundleManifest>();
            BundleManifest bundleManifest = new BundleManifest();
            string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();

            foreach (string bundleName in bundleNames)
            {
                string[] directDependencies = AssetDatabase.GetAssetBundleDependencies(bundleName, false);

                // Get all files in bundles
                string[] assetsPath = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                string[] assetNames = new string[assetsPath.Length];
                for (int i = 0; i < assetsPath.Length; i++)
                {
                    string assetPath = assetsPath[i];
                    string assetName = Path.GetFileName(assetPath);
                    assetNames[i] = assetName;
                    if (!assetPath.StartsWith("Assets/AssetBundles/"))
                    {
                        result.SetFailure($"Asset {assetName} must be located under the assets bundle root folder!");
                    }
                }
                
                Bundle bundle = new Bundle(bundleName, directDependencies, Bundle.States.UnityEditor);
                bundleManifest.AddBundle(bundle, assetNames);
            }

            result.Data = bundleManifest;
            this.Log($"Creating local manifest took {stopwatch.ElapsedMilliseconds} ms!");
            stopwatch.Stop();
            _onManifestReady.Invoke(result);
            
            #endif
        }
        
        #endregion
    }
}
