using System;

namespace Foundation.AssetBundles
{
    [Serializable]
    public class Bundle
    {
        public enum States { RequireDownload, Cached, Embedded, UnityEditor }

        public States State = States.RequireDownload;
        public string BundleName;
        public string[] DirectDependencies;

        public Bundle()
        {
            DirectDependencies = new string[0];
        }
        
        public Bundle(string bundleName, string[] dependencies, States state)
        {
            BundleName = bundleName;
            State = state;
            DirectDependencies = dependencies ?? new string[0];
        }

    }
}
