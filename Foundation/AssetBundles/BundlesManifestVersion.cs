using System;

namespace Foundation.AssetBundles
{
    [Serializable]
    public class BundlesManifestVersion
    {
        public string ManifestHash;
        public int BuildId;

        public static bool operator ==(BundlesManifestVersion a, BundlesManifestVersion b)
        {
            // Handle null checking
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                // If both are null - will return true, otherwise false
                return ReferenceEquals(a, b);
            }
            // Actually compare the instances...
            return a.ManifestHash == b.ManifestHash && a.BuildId == b.BuildId;
        }

        public static bool operator !=(BundlesManifestVersion a, BundlesManifestVersion b)
        {
            return !(a == b);
        }
    }
}
