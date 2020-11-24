using System;
using System.Collections.Generic;
using BundlesMode = Foundation.AssetBundles.AssetBundlesService.BundlesMode;

namespace Foundation.AssetBundles
{
    internal interface IAssetLoadRequest : IDisposable
    {
        string AssetName { get; }
        string FallbackAssetName { get; }

        void Initialize(Asset asset, HashSet<string> missingBundles, BundlesMode bundlesMode = BundlesMode.Remote);

        void SetAssetName(string name);

        void FailRequest(string reason);

        void OnBundleReady(BundleLoadRequest bundleLoadRequest);

    }
}
