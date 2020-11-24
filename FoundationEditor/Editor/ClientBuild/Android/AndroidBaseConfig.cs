using Foundation.ConfigurationResolver;
using UnityEditor;
using UnityEngine;

namespace FoundationEditor.BuildPipelineService.Editor.ClientBuild.Android
{
    public class AndroidBaseConfig : BaseConfig
    {
        public string KeyStore;
        public string KeyStorePass;
        public string KeyStoreAliasName;
        public string KeyStoreAliasPass;
        public bool BuildAppBundle;
        public AndroidSdkVersions MinSDKVersion;
    }
}
