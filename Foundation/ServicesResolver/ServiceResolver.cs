using System;
using System.Collections.Generic;
using Foundation.ConfigurationResolver;
using Foundation.Logger;
using Foundation.Utils.ReflectionUtils;

namespace Foundation.ServicesResolver
{
    public class ServiceResolver
    {
        private Dictionary<Type, IService> _services;
        private ConfigResolver _configResolver;
        
        #region Initialization
        
        public ServiceResolver(ConfigResolver configResolver)
        {
            _services = new Dictionary<Type, IService>();
            _configResolver = configResolver;
        }
        
        #endregion

        #region Get / Add Services
        
        /// <summary>
        /// Add the given service into the resolver collection if it doesn't exists
        /// </summary>
        /// <param name="service"></param>
        public void Inject<T>(T service) where T : class, IService
        {
            Type serviceType = service.GetType(); // remove
            try
            {
                _services.Add(typeof(T), service);
                service.Init(this, _configResolver);
            }
            catch (Exception e)
            {
                this.LogError("Exception trying to add service: " + serviceType.Name + "\nMessage: " + e.Message);
            }
        }

        /// <summary>
        /// Returns the given service by type or null if it doesn't exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>() where T : class, IService
        {
            bool serviceExists = _services.TryGetValue(typeof(T), out IService iService);
            T service = iService as T;
            if (service == null)
            {
                this.LogError($"Failed resolving service {typeof(T)}: " +
                              $"service exists: {serviceExists} " +
                              $"IService exists: {iService != null}");
            }
            return service;
        }

        #endregion

        #region Dispose Services
        
        /// <summary>
        /// Dispose & remove the given service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void DisposeService<T>() where T : class, IService
        {
            T service = Resolve<T>();
            if (service == null) return;

            service.Dispose();
            _services.Remove(typeof(T));
        }
        
        /// <summary>
        /// Dispose & removes all services
        /// </summary>
        public void DisposeAllServices()
        {
            _configResolver = null;
            
            if (_services != null)
            {
                foreach (IService service in _services.Values)
                {
                    service.Dispose();
                }
                _services.Clear();
            }
            
        }
        
        #endregion
        
    }
}
