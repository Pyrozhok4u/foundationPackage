using Foundation.ConfigurationResolver;
using Foundation.MonoUtils;
using FoundationEditor.BuildPipelineService.Editor.ClientBuild;

namespace FoundationEditor.MonoUtils.Editor
{
    public class MonoServiceBuilder : BaseBuilder
    {
        private const string LateUpdateSymbol = "ENABLE_LATE_UPDATE";
        private const string FixedUpdateSymbol = "ENABLE_FIXED_UPDATE";
        
        internal override PrebuildResult Prebuild()
        {
            PrebuildResult result = new PrebuildResult();
            MonoConfig config = ConfigResolver.GetConfig<MonoConfig>();
            
            // Enable / disable late update
            if (config.IsEnabled && config.LateUpdateEnabled) { result.AddDefineSymbol(LateUpdateSymbol); }
            
            // Enable / disable fixded update
            if (config.IsEnabled && config.FixedUpdateEnabled) { result.AddDefineSymbol(FixedUpdateSymbol); }

            return result;
        }

        internal override void PostBuild()
        {
            
        }

        internal override BaseConfig GetConfig()
        {
            return ConfigResolver.GetConfig<MonoConfig>();
        }
    }
}
