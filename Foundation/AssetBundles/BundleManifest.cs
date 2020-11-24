using System;
using System.Collections.Generic;
using Foundation.Utils.OperationUtils;
using UnityEngine;

namespace Foundation.AssetBundles
{
    [Serializable]
    public class BundleManifest
    {
        // Manifest MD5 Hash
        public string Hash;
        // Maps bundle name to bundle object
        public Dictionary<string, Bundle> BundlesMap = new Dictionary<string, Bundle>();
        // Maps all assets to their corresponding bundles (based on unique asset names)
        public Dictionary<string, Asset> AssetsMap = new Dictionary<string, Asset>();
        
        /// <summary>
        /// Syncs the given (old) manifest to this (new) manifest
        /// </summary>
        /// <param name="oldManifest"></param>
        public void SyncManifest(BundleManifest oldManifest)
        {
            // Clear all embedded / cached flags when downloading a remote manifest
            foreach (Bundle bundle in BundlesMap.Values)
            {
                bundle.State = Bundle.States.RequireDownload;
            }

            // Sync embedded bundles / cached bundles from older manifest
            // i.e. the embedded bundles will always remain the same as the original manifest that was built with this client
            // additionally, a cached bundle can be synced between manifests
            if (oldManifest != null)
            {
                foreach (KeyValuePair<string,Bundle> pair in oldManifest.BundlesMap)
                {
                    // If a bundle is already cached / embedded, sync it to the new manifest
                    if (BundlesMap.TryGetValue(pair.Key, out Bundle bundle))
                    {
                        bundle.State = pair.Value.State;
                    }
                }
            }
            
        }
        
        #region Dependencies Helpers methods
        
        public Result<AssetBundleDependencies> GetAssetDependencies(string assetName, bool requireDownloadOnly = false)
        {
            
            Result<AssetBundleDependencies> result = new Result<AssetBundleDependencies>();
            
            // Get containing bundle
            Result<Bundle> bundleResult = GetContainingBundle(assetName);
            result += bundleResult;
            if (!bundleResult.Success)
            {
                return result;
            }
            
            // Get all bundles dependencies requiring download including this bundle 
            Bundle bundle = bundleResult.Data;
            HashSet<string> dependencies = GetAllDependencies(bundle, requireDownloadOnly);
            if ((bundle.State == Bundle.States.RequireDownload || !requireDownloadOnly) && !dependencies.Contains(bundle.BundleName))
            {
                dependencies.Add(bundle.BundleName);
            }

            // Prepare result object...
            AssetBundleDependencies assetDependencies = new AssetBundleDependencies(bundle.BundleName, dependencies);
            result.Data = assetDependencies;
            return result;
        }
        
        public HashSet<string> GetAllDependencies(Bundle bundle, bool requireDownloadOnly)
        {
            HashSet<string> dependencies = new HashSet<string>();
            GetAllDependencies(bundle, dependencies, requireDownloadOnly);
            return dependencies;
        }

        private void GetAllDependencies(Bundle bundle, HashSet<string> dependencies, bool requireDownloadOnly)
        {
            for (int i = 0; i < bundle.DirectDependencies.Length; i++)
            {
                string dependencyName = bundle.DirectDependencies[i];
                // If dependency was already added, ignore it.
                if(dependencies.Contains(dependencyName)) continue;
                
                if (BundlesMap.TryGetValue(dependencyName, out Bundle dependency))
                {
                    if (dependency.State == Bundle.States.RequireDownload || !requireDownloadOnly)
                    {
                        dependencies.Add(dependencyName);
                    }
                    
                    // Collect all dependencies recursively
                    GetAllDependencies(dependency, dependencies, requireDownloadOnly);
                }
            }
        }
        
        #endregion
        
        #region Assets Helper methods
        
        public Result<Bundle> GetContainingBundle(string assetName)
        {
            Result<Bundle> result = new Result<Bundle>();
            if (!AssetsMap.TryGetValue(assetName, out Asset asset))
            {
                result.SetFailure($"Asset does not exists in manifest: {assetName}");
                return result;
            }

            if (BundlesMap.TryGetValue(asset.BundleName, out Bundle bundle))
            {
                result.Data = bundle;
            }
            else
            {
                result.SetFailure($"Bundle does not exists in manifest: {asset.BundleName}");
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns true if the containing bundle is cached
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool IsAssetCached(string assetName)
        {
            bool cached = false;
            if (AssetsMap.TryGetValue(assetName, out Asset asset))
            {
                cached = IsBundleCached(asset.BundleName);
            }
            return cached;
        }
        
        #endregion
        
        #region Bundles Helper methods
        
        /// <summary>
        /// Returns true if the bundle is cached
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public bool IsBundleCached(string bundleName)
        {
            bool cached = false;
            if (BundlesMap.TryGetValue(bundleName, out Bundle bundle))
            {
                cached = bundle.State == Bundle.States.Cached;
            }
            return cached;
        }

        public Result<Bundle> GetBundle(string bundleName)
        {
            Result<Bundle> result = new Result<Bundle>();
            if(!BundlesMap.TryGetValue(bundleName, out Bundle bundle))
            {
                result.SetFailure($"Bundle map doesn't contain bundle {bundleName}");                
            }
            result.Data = bundle;
            return result;
        }
        
        /// <summary>
        /// Adds a new bundle to the manifest
        /// Should be used only from asset build pipeline when generating a new manifest
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="bundleAssetNames"></param>
        /// <returns></returns>
        public Result AddBundle(Bundle bundle, string[] bundleAssetNames)
        {
            Result result = new Result();
            if (!Application.isEditor)
            {
                result.SetFailure("Bundles cannot be added at run-time!");
                return result;
            }
            
            if (BundlesMap.ContainsKey(bundle.BundleName))
            {
                result.SetFailure("Bundle manifest already contains bundle: " + bundle.BundleName);
                return result;
            }

            BundlesMap.Add(bundle.BundleName, bundle);
            foreach (string assetFullName in bundleAssetNames)
            {
                string[] assetPathComponents = assetFullName.Split('.');
                if (assetPathComponents.Length != 2)
                {
                    result.SetFailure($"Bundle asset names must contain the file name & extension: {assetFullName}");
                    continue;
                }
                string assetName = assetPathComponents[0];
                string assetExtension = assetPathComponents[1];
                if (AssetsMap.ContainsKey(assetName))
                {
                    Asset asset = AssetsMap[assetName];
                    string duplicateAssetNameBundle = asset.BundleName;
                    result.SetFailure($"Asset {assetName} already exists in bundle: " + duplicateAssetNameBundle);
                    continue;
                }
                
                Asset newAsset = new Asset(assetName, bundle.BundleName, assetExtension);
                AssetsMap.Add(assetName, newAsset);
            }

            return result;
        }
        
        #endregion

    }
}
