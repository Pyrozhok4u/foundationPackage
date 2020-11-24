using System;

namespace Foundation.AssetBundles
{
    [Serializable]
    public struct Asset
    {
        public readonly string BundleName;
        public readonly string AssetName;
        public readonly string Extension;
    
        public Asset(string assetName, string bundleName, string extension)
        {
            AssetName = assetName;
            BundleName = bundleName;
            Extension = extension;
        }
    }
}
