using System;
using Foundation.AssetBundles;
using Foundation.ConfigurationResolver;
using Foundation.Network;

namespace Foundation.ServicesResolver
{
    public abstract class BaseService : IService
    {

        protected ServiceResolver ServiceResolver { get; private set; }
        protected ConfigResolver ConfigResolver { get; private set; }

        // Quick accessors to core services..
        protected IAssetBundlesService AssetBundlesService => ServiceResolver?.Resolve<IAssetBundlesService>();
        protected ISocketService SocketService => ServiceResolver?.Resolve<ISocketService>();
        protected IHttpService HTTPService => ServiceResolver?.Resolve<IHttpService>();
        protected ITimerService TimerService => ServiceResolver?.Resolve<ITimerService>();

        private bool _isInitialized;

        public void Init(ServiceResolver serviceResolver, ConfigResolver configResolver)
        {
            if (_isInitialized) return;
            
            ConfigResolver = configResolver;
            ServiceResolver = serviceResolver;

            Initialize();
            
            // Finally, set initialized flag to true...
            _isInitialized = true;
        }

        /// <summary>
        /// Quick access to service config
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T GetConfig<T>() where T : BaseConfig
        {
            return ConfigResolver.GetConfig<T>();
        }
        
        #region Abstract methods
        
        protected abstract void Initialize();
        public abstract void Dispose();

        #endregion

    }
}
