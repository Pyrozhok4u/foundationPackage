using System.Collections.Generic;

namespace Foundation.AssetBundles
{
    public class AssetBundleDependencies
    {
        public readonly string ContainingBundle;
        public readonly HashSet<string> Dependencies;
        
        public AssetBundleDependencies(string containingBundle, HashSet<string> dependencies)
        {
            ContainingBundle = containingBundle;
            Dependencies = dependencies;
        }
    }
}