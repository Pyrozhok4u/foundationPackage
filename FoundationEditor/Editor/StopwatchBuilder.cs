using Foundation.ConfigurationResolver;
using Foundation.Utils.StopwatchUtils;
using FoundationEditor.BuildPipelineService.Editor.ClientBuild;

namespace FoundationEditor.Utils.StopwatchUtils.Editor
{
    public class StopwatchBuilder : BaseBuilder
    {
        internal override PrebuildResult Prebuild()
        {
            PrebuildResult result = new PrebuildResult();
            StopwatchConfig config = ConfigResolver.GetConfig<StopwatchConfig>();
            if (config.IsEnabled) { result.AddDefineSymbol(StopwatchConfig.StopwatchSymbol); }

            return result;
        }

        internal override void PostBuild() { }

        internal override BaseConfig GetConfig()
        {
            return ConfigResolver.GetConfig<StopwatchConfig>();
        }
    }
}
