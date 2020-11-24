using Foundation.ConfigurationResolver;
using Foundation.Logger;
using FoundationEditor.BuildPipelineService.Editor.ClientBuild;

namespace FoundationEditor.Logger.Editor
{
    public class LoggerBaseBuilder : BaseBuilder
    {
        
        internal override PrebuildResult Prebuild()
        {
            PrebuildResult result = new PrebuildResult();
            LoggerConfig config = ConfigResolver.GetConfig<LoggerConfig>();
            // Add logger define symbol only if enabled
            if (config.IsEnabled) { result.AddDefineSymbol(LoggerConfig.EnableLogsSymbols); }

            return result;
        }

        internal override void PostBuild()
        {
            
        }

        internal override BaseConfig GetConfig()
        {
            return ConfigResolver.GetConfig<LoggerConfig>();
        }
    }
}
