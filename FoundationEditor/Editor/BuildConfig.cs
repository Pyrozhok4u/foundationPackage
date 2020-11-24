using FoundationEditor.Utils.Editor;
using FoundationEditor.Utils.Editor.BuildUtils;
using UnityEditor;
using Environment = Foundation.ConfigurationResolver.EnvironmentConfig.Environment;

namespace FoundationEditor.BuildPipelineService.Editor
{
    public class BuildConfig
    {
        public readonly BuildTarget BuildTarget;
        public readonly BuildTargetGroup BuildTargetGroup;
        public readonly BuildService.BuildType BuildType;
        public readonly Environment Environment;
        public readonly int BuildId;
        public readonly bool BuildPlayer;
        public readonly bool IsBatchMode;
        
        public bool Debug;
        public string BuildPath;

        public BuildConfig(BuildService.BuildType buildType, BuildTarget buildTarget,
            Environment environment, bool debug, int buildId, string buildPath, bool buildPlayer)
        {
            BuildType = buildType;
            BuildTarget = buildTarget;
            Environment = environment;
            Debug = debug;
            BuildId = buildId;
            BuildPath = buildPath;
            BuildPlayer = buildPlayer;
            IsBatchMode = EditorBuildUtils.IsBatchMode;
            BuildTargetGroup = buildTarget.ToTargetGroup();
        }
    }
}
