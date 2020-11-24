using System;
using System.Collections.Generic;
using System.Text;
using Foundation.ConfigurationResolver;
using Foundation.Logger;
using Foundation.ServicesResolver;
using Foundation.Utils.OperationUtils;
using Foundation.Utils.ReflectionUtils;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace FoundationEditor.BuildPipelineService.Editor.ClientBuild
{
    public class ClientBuilder : IBuilder
    {

        private BuildConfig _buildConfig;
        private ConfigResolver _configResolver;
        private List<BaseBuilder> _builders;

        #region Build API & internal methods
        
        /// <summary>
        /// Start a a build
        /// </summary>
        public void Build(BuildConfig buildConfig, ServiceResolver serviceResolver, ConfigResolver configResolver, Action<Result> onComplete)
        {
            _buildConfig = buildConfig;
            _configResolver = configResolver;
            
            OperationsQueue.
            Do(InitializeBuilders).
            Then(Prebuild).
            Then(BuildPlayer).
            Then(PostBuild).
            Finally(onComplete).
            Run();
        }
        
        /// <summary>
        /// Find, creates & initialize all builders using reflection
        /// </summary>
        private void InitializeBuilders(Action<Result> operationComplete)
        {
            _builders = new List<BaseBuilder>();
            List<BaseBuilder> builders = ReflectionUtils.GetDerivedInstancesOfType<BaseBuilder>();
            foreach (BaseBuilder builder in builders)
            {
                builder.Init(this, _buildConfig, _configResolver);
                // Add builder only if it supports the current target platform
                if (!builder.IsPlatformSupported()) { continue; }
                _builders.Add(builder);
            }
            operationComplete.Invoke(new Result());
        }
        
        /// <summary>
        /// Called before Unity's build player command
        /// Should be used to configure stand-alone services according to the environment config
        /// </summary>
        private void Prebuild(Action<Result> operationComplete)
        {
            PrebuildResult prebuildResult = new PrebuildResult();
            foreach (BaseBuilder builder in _builders)
            {
                PrebuildResult result = builder.Prebuild();
                if (!result.Success) { prebuildResult.SetFailure(result.Messages); }
                prebuildResult.AddDefineSymbol(result.DefineSymbols);
            }

            // If failed, trigger callback and return...
            if (!prebuildResult.Success)
            {
                operationComplete.Invoke(prebuildResult);
                return;
            }

            //Set define symbols and wait until compilation is done before continuing...
            SetDefineSymbols(prebuildResult.DefineSymbols);
            operationComplete.Invoke(prebuildResult);
            
            // CompileHandler.WaitForCompile(delegate(Result result)
            // {
            //     this.Log("Prebuild - compiler finished successfully: " + result.Success);
            //     prebuildResult.AddResult(result);
            //     operationComplete.Invoke(prebuildResult);
            // });
        }

        /// <summary>
        /// Called after Unity's build player command
        /// Can be used to upload player, change apk / ipa name etc...
        /// </summary>
        private void PostBuild(Action<Result> operationComplete)
        {
            Result result = new Result();
            foreach (BaseBuilder builder in _builders)
            {
                builder.PostBuild();
            }
            operationComplete.Invoke(result);
        }
        
        /// <summary>
        /// Start building the actual player using Unity's Build pipeline
        /// </summary>
        private void BuildPlayer(Action<Result> operationComplete)
        {
            Result<BuildPlayerOptions> result = new Result<BuildPlayerOptions>();
            if (!_buildConfig.BuildPlayer)
            {
                this.Log("Skip build player in dry run!");
                operationComplete.Invoke(result);
                return;
            }
            
            result = GetBuildPlayerOptions();
            if (!result.Success)
            {
                operationComplete.Invoke(result);
                return;
            }

            try
            {
                this.Log("Start Build Player at path: " + result.Data.locationPathName + " is debug: " + result.Data.options);
                BuildReport buildReport = UnityEditor.BuildPipeline.BuildPlayer(result.Data);
                string summary = GetBuildSummaryLog(buildReport);
                bool success = buildReport.summary.result == BuildResult.Succeeded;
                result.AssertMessage(success, summary);
            }
            catch (Exception e)
            {
                this.LogException(e);
                result.SetFailure("Failed building player - see logs for more info...");
            }
            operationComplete.Invoke(result);
        }
        
        #endregion

        #region Internal Util Methods
        
        /// <summary>
        /// Returns a build player options based on the build config
        /// </summary>
        /// <returns></returns>
        private Result<BuildPlayerOptions> GetBuildPlayerOptions()
        {
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                locationPathName = _buildConfig.BuildPath,
                target = _buildConfig.BuildTarget,
                options = _buildConfig.Debug ? BuildOptions.Development : BuildOptions.None,
                scenes = GetScenesFromBuildSettings()
            };
            Result<BuildPlayerOptions> result = ValidateBuildPlayerOptions(options);
            
            return result;
        }
        
        /// <summary>
        /// Validates the given build player options
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private Result<BuildPlayerOptions> ValidateBuildPlayerOptions(BuildPlayerOptions options)
        {
            Result<BuildPlayerOptions> result = new Result<BuildPlayerOptions>() { Data = options };
            if (options.scenes.Length == 0) { result.SetFailure("Build doesn't contain any scenes!"); }
            
            return result;
        }

        /// <summary>
        /// Returns a string array of all enabled scenes configured in build settings
        /// </summary>
        /// <returns></returns>
        private string[] GetScenesFromBuildSettings()
        {
            List<string> scenes = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
                if (scene.enabled) { scenes.Add(scene.path); }
            }
            return scenes.ToArray();
        }

        /// <summary>
        /// Sets the define symbols in player settings
        /// </summary>
        /// <param name="symbols"></param>
        private void SetDefineSymbols(List<string> symbols)
        {
            // Make sure the symbols are always sorted the same way to avoid redundant re-compilations
            symbols.Sort();
            StringBuilder sb = new StringBuilder();
            char symbolSeparator = ';';
            foreach (string symbol in symbols)
            {
                sb.Append(symbol + symbolSeparator);
            }
            
            this.Log("Set define symbols: " + sb);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildConfig.BuildTargetGroup, sb.ToString());
        }

        /// <summary>
        /// Returns a summary of the given build report
        /// </summary>
        /// <param name="buildReport"></param>
        /// <returns></returns>
        private string GetBuildSummaryLog(BuildReport buildReport)
        {
            BuildSummary buildSummary = buildReport.summary;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Build Summary:");
            sb.AppendLine("Result: " + buildSummary.result);
            sb.AppendLine("Duration: " + buildSummary.totalTime);
            sb.AppendLine("Warnings: " + buildSummary.totalWarnings);
            sb.AppendLine("Errors: " + buildSummary.totalErrors);
            sb.AppendLine("Size: " + buildSummary.totalSize);
            return sb.ToString();
        }
        
        #endregion
    }
}
