using Foundation.ConfigurationResolver;
using FoundationEditor.Utils.Editor.BuildUtils;
using UnityEditor;

namespace FoundationEditor.BuildPipelineService.Editor.ClientBuild.WebGL
{
    public class WebglBaseBuilder : BaseBuilder
    {
        internal override PrebuildResult Prebuild()
        {
            PrebuildResult result = new PrebuildResult();

            if (BuildConfig.BuildTarget != BuildTarget.WebGL) { return result; }

            // On webgl always ignore debug
            SetBuildPath();
            return result;
        }

        internal override void PostBuild()
        {
            // TODO: Upload webgl to S3...
        }

        internal override BaseConfig GetConfig()
        {
            return ConfigResolver.GetConfig<WebglBaseConfig>();
        }

        private void SetBuildPath()
        {
            // On batch mode build, don't modify the build path
            if(BuildConfig.IsBatchMode) { return; }

            // Add build path
            BuildConfig.BuildPath = EditorBuildUtils.GetClientLocalBuildPath(BuildConfig.BuildTarget);
            // Add build folder name (i.e. <product>_<version>_<environment>
            BuildConfig.BuildPath += EditorBuildUtils.GetBuildFileName(BuildConfig.Environment) + "/";
            // Always over-write the same folder...
            EditorBuildUtils.CreateFolder(BuildConfig.BuildPath, true);
        }
    }
}
