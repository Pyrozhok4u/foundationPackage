using Foundation.ConfigurationResolver;
using FoundationEditor.Utils.Editor.BuildUtils;
using UnityEditor;

namespace FoundationEditor.BuildPipelineService.Editor.ClientBuild.iOS
{
    // ReSharper disable once InconsistentNaming
    public class iOSBaseBuilder : BaseBuilder
    {
        internal override PrebuildResult Prebuild()
        {
            PrebuildResult result = new PrebuildResult();
            if (BuildConfig.BuildTarget != BuildTarget.iOS) { return result; }

            iOSBaseConfig config = ConfigResolver.GetConfig<iOSBaseConfig>();
            PlayerSettings.iOS.buildNumber = BuildConfig.BuildId.ToString();

            SetBuildPath();

            return result;
        }

        internal override void PostBuild()
        {
            if (BuildConfig.BuildTarget != BuildTarget.iOS) { return; }

            // TODO: Check if any post build logic is required for iOS or next steps happen on dev-ops level only
        }

        internal override BaseConfig GetConfig()
        {
            return ConfigResolver.GetConfig<iOSBaseConfig>();
        }

        private void SetBuildPath()
        {
            // On batch mode build, don't modify the build path
            if(BuildConfig.IsBatchMode) { return; }
            
            BuildConfig.BuildPath = EditorBuildUtils.GetClientLocalBuildPath(BuildConfig.BuildTarget);
            BuildConfig.BuildPath += "XCodeProject/";
            // Always over-write the same folder...
            EditorBuildUtils.CreateFolder(BuildConfig.BuildPath, true);
        }
    }
}
