using UnityEditor;
using Environment = Foundation.ConfigurationResolver.EnvironmentConfig.Environment;

namespace FoundationEditor.BuildPipelineService.Editor.AssetsBuild
{
	public class AssetBundlesBuilderEditor : UnityEditor.Editor
	{
		[MenuItem("Foundation/Build Bundles/Android")]
		public static void BuildAndroid()
		{
			RunBuild(BuildTarget.Android);
		}

		[MenuItem("Foundation/Build Bundles/iOS")]
		public static void BuildIOS()
		{
			RunBuild(BuildTarget.iOS);
		}

		[MenuItem("Foundation/Build Bundles/WebGL")]
		public static void BuildWebGL()
		{
			RunBuild(BuildTarget.WebGL);
		}

		[MenuItem("Foundation/Build Bundles/Selected Platform")]
		public static void BuildSelectedPlatform()
		{
			RunBuild(EditorUserBuildSettings.activeBuildTarget);
		}

		public static void RunBuild(BuildTarget buildTarget)
		{
			BuildConfig buildConfig = new BuildConfig
			(
				BuildService.BuildType.Assets, buildTarget, Environment.Dev, true,
				0, string.Empty, false
			);

			BuildService buildService = new BuildService();
			buildService.Build(buildConfig);
		}
	}
}
