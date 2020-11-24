using System;
using System.Collections.Generic;
using System.IO;
using Foundation.ClientService;
using Foundation.DeviceInfoService;
using Foundation.Logger;
using Foundation.ServicesResolver;
using Foundation.Utils.OperationUtils;
using Foundation.Utils.SerializerUtils;
using UnityEngine;
using ISerializer = Foundation.Utils.SerializerUtils.ISerializer;

namespace Foundation.AssetBundles
{
    public class AssetBundlesService : BaseService, IAssetBundlesService
    {
        
        public enum BundlesMode {Local, Remote};
        
        public const string EmbeddedAssetsFolderName = "EmbeddedAssetBundles/";
        public const string CachedAssetsFolderName = "AssetBundles/";
        public const string CloudStorageFolder = "AssetBundles";
        public const string BundlesModeKey = "BundlesMode";
        
        public static readonly Type ComponentType = typeof(Component);
        public static readonly Type GameObjectType = typeof(GameObject);
        
        public string EmbeddedAssetsFolderPath { get; private set; }
        public string CloudStoragePath { get; private set; }
        public string CachedFolderPath { get; private set; }
        
        private AssetBundlesConfig _config;
        private BundleManifest _bundleManifest;
        private ISerializer _serializer;
        private bool _isInitialized;
        private int _maxParallelBundleRequests;

        private List<IAssetLoadRequest> _pendingAssetRequests = new List<IAssetLoadRequest>();
        private Dictionary<string, BundleLoadRequest> _pendingBundleRequests = new Dictionary<string, BundleLoadRequest>();
        private List<BundleLoadRequest> _queuedBundleRequests = new List<BundleLoadRequest>();
        private Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();
        
        // Used only for faster simulation in Editor that by-pass building bundles every time.
        private BundlesMode _bundlesMode = BundlesMode.Remote;
        
        #region Initialization & life-cycle
        
        protected override void Initialize()
        {
            _config = GetConfig<AssetBundlesConfig>();
            _maxParallelBundleRequests = _config.MaxParallelBundleRequests;
            PlatformType platformType = ServiceResolver.Resolve<DeviceService>().CurrentPlatform;
            _serializer = new UnitySerializer();

            // Generate dynamic folders paths & make sure folder exists...
            string clientVersionFolder = $"{ClientVersionUtils.Version}{Path.DirectorySeparatorChar}";
            string platformFolder = $"{platformType}{Path.DirectorySeparatorChar}";
            CloudStoragePath = Path.Combine(CloudStorageFolder, Path.Combine(platformFolder, clientVersionFolder));
            EmbeddedAssetsFolderPath = Path.Combine(Application.streamingAssetsPath, EmbeddedAssetsFolderName);
            CachedFolderPath = Path.Combine(Application.persistentDataPath, CachedAssetsFolderName);
            Directory.CreateDirectory(CachedFolderPath);
            Directory.CreateDirectory(EmbeddedAssetsFolderPath);

            OperationsQueue.
            Do(InitializeManifest).
            Finally(OnFinishedInitialization).
            Run("Initialize Asset Bundles Service");

        }

        private void InitializeManifest(Action<Result<BundleManifest>> operationComplete)
        {
            #if UNITY_EDITOR
            // In unity editor only - check the bundles mode
            _bundlesMode = (BundlesMode)PlayerPrefs.GetInt(BundlesModeKey, (int)BundlesMode.Local);
            #endif
            
            var manifestInitializer = new BundleManifestInitializer(ServiceResolver, _serializer, _config, _bundlesMode);
            manifestInitializer.Initialize(delegate(Result<BundleManifest> result)
            {
                _bundleManifest = result.Data;
                operationComplete.Invoke(result);
            });
        }

        private void OnFinishedInitialization(Result result)
        {
            _isInitialized = result.Success;
            
            // Initialize all pending requests that were received before initialization
            foreach (IAssetLoadRequest request in _pendingAssetRequests)
            {
                InitializeAssetRequest(request);
            }
            _pendingAssetRequests.Clear();
        }

        public override void Dispose()
        {
            _config = null;
            _bundleManifest = null;
            _serializer = null;

            foreach (AssetBundle assetBundle in _loadedBundles.Values)
            {
                // We may want in the future to use assetBundle.Unload(false) instead of keep loaded objects alive...
                if(assetBundle != null) { assetBundle.Unload(true); }
            }
            _loadedBundles.Clear();
            
            foreach (IAssetLoadRequest pendingAssetRequest in _pendingAssetRequests)
            {
                pendingAssetRequest.Dispose();
            }
            _pendingAssetRequests.Clear();

            foreach (BundleLoadRequest request in _pendingBundleRequests.Values)
            {
                request.Dispose();
            }
            _pendingBundleRequests.Clear();

            foreach (BundleLoadRequest request in _queuedBundleRequests)
            {
                request.Dispose();
            }
            _queuedBundleRequests.Clear();
        }
        
        #endregion

        #region Asset load requests

        public void LoadAsset<T>(string asset, string fallbackAsset, Action<Result<T>> onComplete) where T : UnityEngine.Object
        {
            CreateAssetLoadRequest<T>(asset, onComplete, fallbackAsset);
        }

        public void LoadAsset<T>(string asset, Action<Result<T>> onComplete) where T : UnityEngine.Object
        {
            CreateAssetLoadRequest<T>(asset, onComplete);
        }

        private void CreateAssetLoadRequest<T>(string asset, Action<Result<T>> onComplete, string fallbackAsset = null) where T : UnityEngine.Object
        {
            IAssetLoadRequest assetRequest = new AssetLoadRequest<T>(asset, fallbackAsset, this, onComplete);
            if (!_isInitialized)
            {
                _pendingAssetRequests.Add(assetRequest);
                return;
            }

            InitializeAssetRequest(assetRequest);
        }

        private void InitializeAssetRequest(IAssetLoadRequest assetRequest)
        {
            
            Result<AssetBundleDependencies> dependenciesResult = _bundleManifest.GetAssetDependencies(assetRequest.AssetName);
            if (!dependenciesResult.Success)
            {
                this.LogError($"Initialize asset request failed: {dependenciesResult}");
                return;
            }
            
            HashSet<string> allDependencies = dependenciesResult.Data.Dependencies;
            HashSet<string> missingDependencies = new HashSet<string>();
            foreach (string dependency in allDependencies)
            {
                // Skip loaded bundles...
                if(_loadedBundles.ContainsKey(dependency)) { continue; }
                // Add to missing dependency list
                missingDependencies.Add(dependency);
            }

            // If alternative asset name exists, use it instead...
            if(!AssetExists(assetRequest.AssetName)) { assetRequest.SetAssetName(assetRequest.FallbackAssetName); }

            if (!_bundleManifest.AssetsMap.TryGetValue(assetRequest.AssetName, out Asset asset))
            {
                string error = $"Failed to find asset {assetRequest.AssetName} in manifest!";
                this.LogError(error);
                assetRequest.FailRequest(error);
                return;
            }
            
            assetRequest.Initialize(asset, missingDependencies, _bundlesMode);

            // If all bundles already available, asset request will initialize start load immediately.
            // Otherwise, register & wait for the missing bundles
            if (missingDependencies.Count <= 0) return;

            // Create bundle requests for the missing dependencies
            foreach (string missingBundle in missingDependencies)
            {
                // If bundle request already exists - register to it. Otherwise initialize a new bundle request...
                if (!_pendingBundleRequests.TryGetValue(missingBundle, out BundleLoadRequest bundleRequest))
                {
                    bundleRequest = CreateBundleRequest(missingBundle);
                    _queuedBundleRequests.Add(bundleRequest);
                }
                bundleRequest.OnBundleReady += assetRequest.OnBundleReady;
            }

            StartNextBundleRequest();
        }

        private BundleLoadRequest CreateBundleRequest(string bundleName)
        {
            Bundle bundle = _bundleManifest.GetBundle(bundleName).Data;
            string url = Path.Combine(_config.CloudStorageBaseUrl, CloudStoragePath, bundle.BundleName);
            BundleLoadRequest bundleRequest = new BundleLoadRequest(bundle, url, ServiceResolver, OnBundleLoaded);
            return bundleRequest;
        }
        
        private void OnBundleLoaded(BundleLoadRequest bundleRequest)
        {
            Bundle bundle = bundleRequest.Bundle;
            _pendingBundleRequests.Remove(bundleRequest.Bundle.BundleName);
            _loadedBundles.Add(bundle.BundleName, bundleRequest.UnityAssetBundle);
            
            StartNextBundleRequest();
        }

        private void StartNextBundleRequest()
        {
            // Iterate over all pending bundle requests & Initialize the maximum parallel bundle requests allowed...
            for (int i = 0; i < _queuedBundleRequests.Count; i++)
            {
                if (_pendingBundleRequests.Count > _maxParallelBundleRequests)
                {
                    // Max parallel requests reached...
                    break;
                }

                BundleLoadRequest request = _queuedBundleRequests[0];
                _queuedBundleRequests.RemoveAt(0);
                
                _pendingBundleRequests.Add(request.Bundle.BundleName, request);
                request.Initialize();
            }
        }

        #endregion

        #region Public / Internal Helpers

        public bool AssetExists(string assetName)
        {
            if (!_isInitialized)
            {
                this.LogError($"Asset exists cannot be called before asset bundle service is initialized - asset name {assetName}");
                return false;
            }

            bool exists = false;
            if (!string.IsNullOrEmpty(assetName))
            {
                exists = _bundleManifest.AssetsMap.ContainsKey(assetName);
            }
            return exists;
        }

        internal Result<AssetBundle> GetLoadedBundle(string bundleName)
        {
            Result<AssetBundle> result = new Result<AssetBundle>();
            if (!_loadedBundles.TryGetValue(bundleName, out result.Data))
            {
                result.SetFailure($"Bundle {bundleName} is not loaded!");
            }
            return result;
        }
        
        #endregion
        
    }
}
