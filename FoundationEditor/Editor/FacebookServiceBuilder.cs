using System.Collections.Generic;
using Facebook.Unity.Settings;
using Foundation.ConfigurationResolver;
using Foundation.Facebook.Editor;
using FoundationEditor.BuildPipelineService.Editor.ClientBuild;

namespace FoundationEditor.Facebook.Editor
{
    public class FacebookServiceBuilder : BaseBuilder
    {
        internal override PrebuildResult Prebuild()
        {
            PrebuildResult result = new PrebuildResult();
            FacebookConfig config = ConfigResolver.GetConfig<FacebookConfig>();
            if (string.IsNullOrEmpty(config.AppID))
            {
                result.SetFailure("Facebook app ID doesn't exists!");
            }
            FacebookSettings.AppIds = new List<string> { config.AppID };
            FacebookSettings.Logging = config.Logging;
            FacebookSettings.AutoLogAppEventsEnabled = config.AutoLogAppEventsEnabled;
            return result;
        }

        internal override void PostBuild()
        {
            
        }

        internal override BaseConfig GetConfig()
        {
            return ConfigResolver.GetConfig<FacebookConfig>();
        }
    }
}
