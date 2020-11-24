using System;
using System.Collections.Generic;
using System.Diagnostics;
using Foundation.Logger;
using UnityEngine;

namespace Foundation.ConfigurationResolver
{
    public class ConfigResolver
    {
        public string EnvironmentConfigFolder { get; private set; }
        
        public ConfigResolver()
        {
            EnvironmentConfig environmentConfig = EnvironmentConfig.LoadConfig();
            SetEnvironment(environmentConfig.GetEnvironment());
        }

        public T GetConfig<T>() where T : BaseConfig
        {
            return LoadConfig(typeof(T)) as T;
        }

        public BaseConfig LoadConfig(Type type)
        {
            BaseConfig config = null;
            string configPath = EnvironmentConfigFolder + type.Name;
            try
            {
                config = Resources.Load(configPath) as BaseConfig;
                //this.Log("Get config for type: " + type.Name + " success: " + (config != null));
            }
            catch (Exception e)
            {
                this.LogError("Failed loading config: " + configPath);
                this.LogException(e);
            }
            
            return config;
        }

        private void SetEnvironment(EnvironmentConfig.Environment environment)
        {
            EnvironmentConfigFolder = environment + "/";
            this.Log("Initialize config resolver with environment path: " + EnvironmentConfigFolder);
        }
    }
}
