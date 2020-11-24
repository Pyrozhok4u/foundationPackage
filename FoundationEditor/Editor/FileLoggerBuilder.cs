using Foundation.ConfigurationResolver;
using Foundation.FileLogger;
using FoundationEditor.BuildPipelineService.Editor.ClientBuild;

namespace FoundationEditor.FileLogger.Editor
{
    public class FileLoggerBuilder : BaseBuilder
    {
        internal override PrebuildResult Prebuild()
        {
            PrebuildResult result = new PrebuildResult();
            FileLoggerConfig config = ConfigResolver.GetConfig<FileLoggerConfig>();
            // Add alert define symbol only if enabled
            if (config.IsEnabled) { result.AddDefineSymbol(FileLoggerConfig.EnableLoggerSymbols); }
            return result;
        }

        internal override void PostBuild() { }

        internal override BaseConfig GetConfig()
        {
            return ConfigResolver.GetConfig<FileLoggerConfig>();
        }
    }
}
