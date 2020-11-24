using System.Collections.Generic;
using Foundation;
using Foundation.ConfigurationResolver;
using Foundation.Logger;
using Foundation.Utils.OperationUtils;
using FoundationEditor.Utils.Editor;

namespace FoundationEditor.BuildPipelineService.Editor.ClientBuild
{
    public abstract class BaseBuilder
    {
        
        protected ClientBuilder ClientBuilder;
        protected ConfigResolver ConfigResolver;
        protected BuildConfig BuildConfig;

        /// <summary>
        /// Initialize the builder
        /// Should be called internally only after instantiating all builders
        /// </summary>
        /// <param name="clientBuilder"></param>
        /// <param name="buildConfig"></param>
        internal void Init(ClientBuilder clientBuilder, BuildConfig buildConfig, ConfigResolver configResolver)
        {
            this.Log("Initialize builder: " + GetType().Name);
            ClientBuilder = clientBuilder;
            BuildConfig = buildConfig;
            ConfigResolver = configResolver;
        }

        /// <summary>
        /// Returns true if the builder is supported for the current platform
        /// </summary>
        /// <returns></returns>
        internal bool IsPlatformSupported()
        {
            BaseConfig config = GetConfig();
            bool isSupported = false;
            PlatformType platformType = BuildConfig.BuildTarget.ToPlatformType();
            if (config.SupportedPlatform == PlatformType.All)
            {
                isSupported = true;
            }
            else if (config.SupportedPlatform != PlatformType.Unknown && config.SupportedPlatform == platformType)
            {
                isSupported = true;
            }
            
            return isSupported;
        }
        
        internal abstract PrebuildResult Prebuild();
        internal abstract void PostBuild();
        internal abstract BaseConfig GetConfig();
    }

    internal class PrebuildResult : Result
    {
        public readonly List<string> DefineSymbols = new List<string>();

        internal void AddDefineSymbol(string symbol)
        {
            if(DefineSymbols.Contains(symbol)) { return; }
            
            DefineSymbols.Add(symbol);
        }
        
        internal void AddDefineSymbol(List<string> symbols)
        {
            foreach (string symbol in symbols)
            {
                AddDefineSymbol(symbol);
            }
        }
    }
}

