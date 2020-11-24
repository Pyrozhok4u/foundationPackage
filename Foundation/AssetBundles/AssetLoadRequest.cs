using System;
using System.Collections.Generic;
using Foundation.Logger;
using Foundation.ServicesResolver;
using Foundation.Utils.OperationUtils;
using UnityEngine;
using Object = UnityEngine.Object;
using BundlesMode = Foundation.AssetBundles.AssetBundlesService.BundlesMode;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Foundation.AssetBundles
{
    internal class AssetLoadRequest<T> : IAssetLoadRequest where T : UnityEngine.Object
    {
        
        public string AssetName { get; private set; }
        public string FallbackAssetName { get; }
        public HashSet<string> MissingBundles { get; private set; }

        private Action<Result<T>> _onComplete;
        private AssetBundlesService _assetBundlesService;
        private BundleManifest _bundleManifest;
        private Asset _asset;
        private BundlesMode _bundlesMode;
        
        public AssetLoadRequest(string assetName, string fallbackAssetName, AssetBundlesService assetBundlesService, Action<Result<T>> onComplete)
        {
            AssetName = assetName;
            FallbackAssetName = fallbackAssetName;
            _onComplete = onComplete;
            _assetBundlesService = assetBundlesService;
        }
        
        public void Dispose()
        {
            _onComplete = null;
            _assetBundlesService = null;
            _bundleManifest = null;
        }
        
        public void Initialize(Asset asset, HashSet<string> missingBundles, BundlesMode bundlesMode = BundlesMode.Remote)
        {
            _asset = asset;
            _bundlesMode = bundlesMode;
            MissingBundles = missingBundles;

            // If all bundles are already available, start asset load immediately...
            if (MissingBundles.Count == 0) { StartAssetLoad(); }
        }

        public void SetAssetName(string name)
        {
            AssetName = name;
        }

        public void FailRequest(string reason)
        {
            Result<T> result = new Result<T>();
            result.SetFailure(reason);
            _onComplete?.Invoke(result);
        }

        private void StartAssetLoad()
        {
            if (_bundlesMode == BundlesMode.Remote)
            {
                LoadAsset();
            }
            else
            {
                LoadLocalAsset();
            }
        }

        private void LoadAsset()
        {
            Result<AssetBundle> result = _assetBundlesService.GetLoadedBundle(_asset.BundleName);
            if (!result.Success)
            {
                _onComplete.Invoke(result as Result<T>);
                return;
            }

            AssetBundle bundle = result.Data;
            AssetBundleRequest assetAsyncRequest = bundle.LoadAssetAsync(AssetName);
            assetAsyncRequest.completed += OnAssetLoaded;
        }

        private void LoadLocalAsset()
        {
            #if UNITY_EDITOR
            
            string assetPath = "Assets/AssetBundles/";
            string[] folders = _asset.BundleName.Split('_');
            foreach (string folder in folders)
            {
                assetPath += folder + "/";
            }
            assetPath += AssetName + "." + _asset.Extension;
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            Result<T> result = InstantiateObject(asset);
            _onComplete?.Invoke(result);
            
            #endif
        }
        
        private void OnAssetLoaded(AsyncOperation operation)
        {
            AssetBundleRequest request = operation as AssetBundleRequest;
            Result<T> result = InstantiateObject(request?.asset);
            _onComplete?.Invoke(result);
        }

        private Result<T> InstantiateObject(Object loadedObject)
        {
            Result<T> result = new Result<T>();
            Type type = typeof(T);
            T asset = null;
            bool isMonoComponent = type.IsSubclassOf(AssetBundlesService.ComponentType);
            bool isGameObject = type == AssetBundlesService.GameObjectType;
            if (isMonoComponent || isGameObject)
            {
                this.Log($"Try loading game object with component: {type.Name}");
                GameObject o = loadedObject as GameObject;
                if (o != null)
                {
                    GameObject gameObject = GameObject.Instantiate(o);
                    if (isGameObject)
                    {
                        asset = gameObject as T;
                    }
                    else
                    {
                        asset = gameObject.GetComponent<T>();
                    }
                }
            }
            else
            {
                this.Log($"Try loading direct type: {type.Name}");
                asset = loadedObject as T;
            }
            
            if (asset == null)
            {
                result.SetFailure($"Failed loading asset {AssetName} from bundle {_asset.BundleName}");
            }

            result.Data = asset;
            return result;
        }

        public void OnBundleReady(BundleLoadRequest bundleLoadRequest)
        {
            this.Log($"Asset {AssetName} received missing dependency {bundleLoadRequest.Bundle.BundleName}");
            
            // Un-register from callback...
            bundleLoadRequest.OnBundleReady -= OnBundleReady;
            
            // Remove bundle & check if more bundles are missing
            MissingBundles.Remove(bundleLoadRequest.Bundle.BundleName);
            if (MissingBundles.Count > 0) { return; }
            
            // After all bundles were loaded, try loading actual asset
            this.Log($"All required bundles are ready for asset: {AssetName}");
            StartAssetLoad();
        }
    }
}
