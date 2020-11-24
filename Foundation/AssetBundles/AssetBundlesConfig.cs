using System;
using System.Collections.Generic;
using Foundation.ConfigurationResolver;
using Foundation.Logger;
using UnityEngine;

namespace Foundation.AssetBundles
{
    public class AssetBundlesConfig : BaseConfig
    {
        public string CloudStorageBaseUrl = "https://unity-foundation.s3.amazonaws.com/";
        public string ManifestVersionFileName = "ManifestVersion.json";
        public int MaxParallelBundleRequests = 3;

        public List<string> EmbeddedBundles;

        [HideInInspector] public BundlesManifestVersion EmbeddedManifestVersion;

        public bool IsBundleEmbedded(string bundleName)
        {
            bool isEmbedded = false;
            for (int i = 0; i < EmbeddedBundles.Count; i++)
            {
                if (EmbeddedBundles[i].Equals(bundleName, StringComparison.OrdinalIgnoreCase))
                {
                    isEmbedded = true;
                    break;
                }
            }

            return isEmbedded;
        }
    }
}
