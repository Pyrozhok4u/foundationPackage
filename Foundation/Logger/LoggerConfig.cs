using Foundation.ConfigurationResolver;
using UnityEngine;

namespace Foundation.Logger
{
    public class LoggerConfig : BaseConfig
    {
        public const string EnableLogsSymbols = "ENABLE_LOGS";
        
        [SerializeField] public bool IsErrorsEnabled;
    }
}

