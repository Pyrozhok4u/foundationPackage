using System;
using Foundation;
using UnityEditor;

using Foundation.ConfigurationResolver;
using Foundation.Logger;
using Foundation.ServicesResolver;
using Foundation.Utils.CommandLineUtils;

using Foundation.Utils.OperationUtils;
using FoundationEditor.AWSUtils.Editor;
using FoundationEditor.BuildPipelineService.Editor.AssetsBuild;
using FoundationEditor.BuildPipelineService.Editor.ClientBuild;
using FoundationEditor.ConfigurationResolver.Editor;
using FoundationEditor.Utils.Editor;
using FoundationEditor.Utils.Editor.BuildUtils;
using Environment = Foundation.ConfigurationResolver.EnvironmentConfig.Environment;

namespace FoundationEditor.BuildPipelineService.Editor
{
    public class BuildService
    {
        public enum BuildType { Client, Assets}
        public enum BuildState { Succeeded = 0, Failed = 101, Cancelled = 102, Unknown = 103 }

        private ServiceResolver _serviceResolver;

        #region Start Build in Batch Mode (from command line)

        /// <summary>
        /// Start a build from command line or shell script
        /// </summary>
        public static void Build()
        {
            Debugger.Log(null, "Start a batch mode build!");
            if (!EditorBuildUtils.IsBatchMode)
            {
                Debugger.LogError(null, "Build() can only be called in batch mode! Ignoring call...");
                return;
            }

            Result<BuildConfig> result = GetBuildConfigFromCommandLine();
            if (!result.Success)
            {
                Debugger.LogError(null, "Failed running build from command line: " + result);
                EditorApplication.Exit((int) BuildState.Failed);
            }
            BuildService buildService = new BuildService();
            buildService.Build(result.Data);
        }

        /// <summary>
        /// Start a build from command line or shell script
        /// </summary>
        private static Result<BuildConfig> GetBuildConfigFromCommandLine()
        {
            Result<BuildConfig> result = new Result<BuildConfig>();
            CommandLineArgsUtils commandLineArgsUtils = new CommandLineArgsUtils();

            // Try initializing command line utils
            result += commandLineArgsUtils.Initialize();
            if (!result.Success) { return result; }

            try
            {
                // Try parse command line arguments for the build
                Environment environment = commandLineArgsUtils.GetEnumValue<Environment>("environment");
                BuildTarget buildTarget = commandLineArgsUtils.GetEnumValue<BuildTarget>("buildTarget");
                BuildType buildType = commandLineArgsUtils.GetEnumValue<BuildType>("buildType");
                bool debug = commandLineArgsUtils.GetBoolValue("debug");
                bool webglDebug = commandLineArgsUtils.GetBoolValue("webglDebug");
                int buildId = commandLineArgsUtils.GetIntNumber("buildId");
                string buildPath = commandLineArgsUtils.GetStringValue("buildPath");
                if (buildTarget == BuildTarget.WebGL)
                {
                    debug = webglDebug;
                }
                BuildConfig buildConfig = new BuildConfig(buildType, buildTarget, environment, debug, buildId, buildPath, true);
                result.Data = buildConfig;
            }
            catch (Exception e)
            {
                result.SetFailure("Error parsing command line arguments for the build:" + e.Message);
            }

            return result;
        }

        #endregion

        #region Run Builds API

        /// <summary>
        /// Start a build with the given build config
        /// Can be called directly from editor to run local builds
        /// </summary>
        /// <param name="buildConfig"></param>
        public void Build(BuildConfig buildConfig)
        {
            // Update selected environment & create config resolver for the environment...
            ConfigResolverEditorService.SetEnvironment(buildConfig.Environment);
            ConfigResolver configResolver = new ConfigResolver();

            _serviceResolver = new ServiceResolver(configResolver);
            _serviceResolver.Inject(new S3BatchModeService());

            Result result = ValidateBuildConfig(buildConfig);
            if (!result.Success)
            {
                OnBuildComplete(result);
                return;
            }

            IBuilder builder;
            switch (buildConfig.BuildType)
            {
                case BuildType.Client:
                    builder = new ClientBuilder();
                    break;
                case BuildType.Assets:
                    builder = new AssetBundlesBuilder();
                    break;
                default:
                    OnBuildComplete(new Result(false, "Unknown build type: " + buildConfig.BuildType));
                    return;
            }
            builder.Build(buildConfig, _serviceResolver, configResolver, OnBuildComplete);
        }

        #endregion

        #region Helper methods

        private Result ValidateBuildConfig(BuildConfig buildConfig)
        {
            Result result = new Result();
            if (buildConfig.BuildTarget.ToPlatformType() == PlatformType.Unknown)
            {
                result.SetFailure("Build config contains unknown target platform");
            }

            result.AddMessage("`Build config valid: " + result.Success);
            return result;
        }

        public void OnBuildComplete(Result result)
        {
            this.Log("Finished build: " + result);
            // Dispose all build related services...
            _serviceResolver.DisposeAllServices();

            // Exit application with code only when actually running in batch mode
            BuildState state = result.Success ? BuildState.Succeeded : BuildState.Failed;
            if (EditorBuildUtils.IsBatchMode) { EditorApplication.Exit((int) state); }
            else { EditorUtility.DisplayDialog("Build finished", result.ToString(), "Ok"); }
        }

        #endregion
    }

    internal interface IBuilder
    {
        void Build(BuildConfig buildConfig, ServiceResolver serviceResolver, ConfigResolver configResolver, Action<Result> onComplete);
    }
}
