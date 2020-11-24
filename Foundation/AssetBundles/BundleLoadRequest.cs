using System;
using System.IO;
using Foundation.Logger;
using Foundation.Network;
using Foundation.ServicesResolver;
using UnityEngine;

namespace Foundation.AssetBundles
{
    internal class BundleLoadRequest : IDisposable
    {
        public Bundle Bundle;
        public AssetBundle UnityAssetBundle;

        private readonly string _url;
        private readonly string _filePath;
        private readonly IAssetBundlesService _assetBundlesService;
        private IHttpService _downloader;
        private ServiceResolver _serviceResolver;
        
        private event Action<BundleLoadRequest> OnBundleLoaded;
        public event Action<BundleLoadRequest> OnBundleReady;

        #region Life Cycle
        
        public BundleLoadRequest(Bundle bundle, string url, ServiceResolver serviceResolver, Action<BundleLoadRequest> onBundleLoaded)
        {
            Bundle = bundle;
            
            _url = url;
            _serviceResolver = serviceResolver;
            _downloader = serviceResolver.Resolve<IHttpService>();
            _assetBundlesService = serviceResolver.Resolve<IAssetBundlesService>();

            OnBundleLoaded = onBundleLoaded;

            _filePath = Path.Combine(_assetBundlesService.CachedFolderPath, bundle.BundleName);
        }
        
        public void Initialize()
        {
            switch (Bundle.State)
            {
                case Bundle.States.Cached:
                    LoadBundleFromFile(Path.Combine(_assetBundlesService.CachedFolderPath, Bundle.BundleName));
                    break;
                case Bundle.States.Embedded:
                    LoadBundleFromFile(Path.Combine(_assetBundlesService.EmbeddedAssetsFolderPath, Bundle.BundleName));
                    break;
                case Bundle.States.UnityEditor:
                    OnBundleLoadingComplete();
                    break;
                case Bundle.States.RequireDownload:
                    DownloadBundle();
                    break;
            }
        }
        
        private void OnBundleLoadingComplete()
        {
            bool success = UnityAssetBundle != null;
            if(Bundle.State == Bundle.States.UnityEditor) { success = true; }
            
            this.LogAssertion($"Loaded bundle {Bundle.BundleName} successfully: {success}", success);
            // First invoke the bundle loaded event which is required by asset service
            OnBundleLoaded?.Invoke(this);
            // Than invoke bundle ready event which is used by the asset requests
            OnBundleReady?.Invoke(this);
        }
        
        public void Dispose()
        {
            Bundle = null;
            UnityAssetBundle = null;
            OnBundleLoaded = null;
            OnBundleReady = null;
            _downloader = null;
            _serviceResolver = null;
        }
        
        #endregion

        #region Download Bundle

        private void DownloadBundle()
        {
            _downloader.GetBytes(_url, OnDownloadComplete);
        }

        private void OnDownloadComplete(APIResponse<byte[]> response)
        {
            if (!response.Success)
            {
                OnBundleLoadingComplete();
                return;
            }
            
            // Load asset bundle
            AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromMemoryAsync(response.Data);
            assetRequest.completed += delegate(AsyncOperation operation)
            {
                AssetBundleCreateRequest request = operation as AssetBundleCreateRequest;
                UnityAssetBundle = request?.assetBundle;
                OnBundleLoadingComplete();
            };
            
            // Cache (save) to disk only on mobile (will be ignored for webgl)
            SaveFileAsyncOperation.SaveAsync(response.Data, _filePath, _serviceResolver);
        }

        #endregion

        private void LoadBundleFromFile(string filePath)
        {
            this.Log($"Try load bundle: {Bundle.BundleName} from: {filePath}");
            AssetBundleCreateRequest asyncRequest = AssetBundle.LoadFromFileAsync(filePath);
            asyncRequest.completed += delegate(AsyncOperation operation)
            {
                AssetBundleCreateRequest request = operation as AssetBundleCreateRequest;
                UnityAssetBundle = request?.assetBundle;
                OnBundleLoadingComplete();
            };
        }
    }
}
