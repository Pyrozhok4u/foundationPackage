using Foundation.ConfigurationResolver;
using Foundation.DebugUtils.ErrorAlertService;
using FoundationEditor.BuildPipelineService.Editor.ClientBuild;

namespace FoundationEditor.DebugUtils.ErrorAlertService.Editor
{
    public class AlertBuilder : BaseBuilder
    {
        internal override PrebuildResult Prebuild()
        {
            PrebuildResult result = new PrebuildResult();
            ErrorAlertConfig config = ConfigResolver.GetConfig<ErrorAlertConfig>();
            // Add alert define symbol only if enabled
            if (config.IsEnabled) { result.AddDefineSymbol(ErrorAlertConfig.EnableAlertsSymbols); }
            return result;
        }

        internal override void PostBuild() { }

        internal override BaseConfig GetConfig()
        {
            return ConfigResolver.GetConfig<ErrorAlertConfig>();
        }
    }
}
