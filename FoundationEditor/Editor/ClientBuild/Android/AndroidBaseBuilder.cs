using Foundation.ClientService;
using Foundation.ConfigurationResolver;
using Foundation.Utils.OperationUtils;
using FoundationEditor.Utils.Editor.BuildUtils;
using UnityEditor;

namespace FoundationEditor.BuildPipelineService.Editor.ClientBuild.Android
{
    public class AndroidBaseBuilder : BaseBuilder
    {
        internal override PrebuildResult Prebuild() 
        {
            PrebuildResult result = new PrebuildResult();
            
            if (BuildConfig.BuildTarget != BuildTarget.Android) { return result; }
            
            AndroidBaseConfig config = ConfigResolver.GetConfig<AndroidBaseConfig>();

            SetVersionCode(result);

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = config.KeyStore;
            PlayerSettings.Android.keystorePass = config.KeyStorePass;
            PlayerSettings.Android.keyaliasName = config.KeyStoreAliasName;
            PlayerSettings.Android.keyaliasPass = config.KeyStoreAliasPass;
            PlayerSettings.Android.minSdkVersion = config.MinSDKVersion;
            
            EditorUserBuildSettings.buildAppBundle = config.BuildAppBundle;
            
            SetBuildPath();

            return result;
        }

        internal override void PostBuild()
        {
            if (BuildConfig.BuildTarget != BuildTarget.Android) { return; }
            
            // TODO: Add post build logic such as rename apk, upload some where etc...
        }

        internal override BaseConfig GetConfig()
        {
            return ConfigResolver.GetConfig<AndroidBaseConfig>();
        }

        private void SetVersionCode(Result result)
        {
            Result<int> versionCodeResult = ClientVersionUtils.GetVersionCode();
            if (versionCodeResult.Success)
            {
                PlayerSettings.Android.bundleVersionCode = versionCodeResult.Data;
            }
            
            result.AddResult(versionCodeResult);
        }

        private void SetBuildPath()
        {
            // On batch mode build, don't modify the build path
            if(BuildConfig.IsBatchMode) { return; }
            
            // Add build path
            BuildConfig.BuildPath = EditorBuildUtils.GetClientLocalBuildPath(BuildConfig.BuildTarget);
            EditorBuildUtils.CreateFolder(BuildConfig.BuildPath, false);
            // Add build file name (i.e. <product>_<version>_<environment>
            BuildConfig.BuildPath += EditorBuildUtils.GetBuildFileName(BuildConfig.Environment);
            // Finally, add the apk extension name
            BuildConfig.BuildPath += ".apk";
        }
    }
}
