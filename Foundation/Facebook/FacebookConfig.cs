using Foundation.ConfigurationResolver;
using UnityEngine;

namespace Foundation.Facebook.Editor
{
    public class FacebookConfig : BaseConfig
    {
        public bool AutoInitialization;
        public string AppID;
        public bool Logging;
        public bool AutoLogAppEventsEnabled;
    }
}
