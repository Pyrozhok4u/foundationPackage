using FoundationEditor.Utils.Editor.BuildUtils;
using UnityEditor;
using Environment = Foundation.ConfigurationResolver.EnvironmentConfig.Environment;

namespace FoundationEditor.BuildPipelineService.Editor.ClientBuild
{
    public class ClientBuilderEditor : UnityEditor.Editor
    {
        
        [MenuItem("Foundation/Build/Android")]
        public static void BuildAndroid()
        {
            RunBuild(BuildTarget.Android);
        }
        
        [MenuItem("Foundation/Build/iOS")]
        public static void BuildIOS()
        {
            RunBuild(BuildTarget.iOS);
        }
        
        [MenuItem("Foundation/Build/WebGL")]
        public static void BuildWebGL()
        {
            RunBuild(BuildTarget.WebGL);
        }

        [MenuItem("Foundation/Build/Selected Platform")]
        public static void BuildSelectedPlatform()
        {
            RunBuild(EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("Foundation/Update scripting symbols")]
        public static void RunBuilders()
        {
            RunBuild(EditorUserBuildSettings.activeBuildTarget, false);
        }
        
        private static void RunBuild(BuildTarget buildTarget, bool buildPlayer = true)
        {
            BuildConfig buildConfig = new BuildConfig
            (
                BuildService.BuildType.Client, buildTarget, Environment.Dev, true,
                0, EditorBuildUtils.GetClientLocalBuildPath(buildTarget), buildPlayer
            );

            BuildService buildService = new BuildService();
            buildService.Build(buildConfig);
        }
    }
}
