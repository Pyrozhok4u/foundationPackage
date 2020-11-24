using System.Collections.Generic;
using System.Linq;
using Foundation.ConfigurationResolver;
using Foundation.ServicesResolver;
using UnityEngine;

namespace Foundation.CurrencySystem
{
    public class CurrencyService : BaseService
    {
        private readonly Dictionary<int, CurrencyProvider> _currencyProviders = new Dictionary<int, CurrencyProvider>();

        public CurrencyProvider GetCurrencyProvider(int typeId)
        {
            if (_currencyProviders.TryGetValue(typeId, out CurrencyProvider currencyProvider))
            {
                return currencyProvider;
            }

            return null;
        }

        public static T LoadConfig<T>() where T: CurrencyProviderConfig
        {
            string path =  typeof(T).Name;
            return Resources.Load<T>(path);
        }

        public void AddCurrencyProvider(CurrencyProvider currencyProvider)
        {
            _currencyProviders[currencyProvider.TypeId] = currencyProvider;
        }

        public bool Check(IEnumerable<CurrencyProvider> currencyProviders)
        {
            return currencyProviders.All(currencyProvider => _currencyProviders.ContainsKey(currencyProvider.TypeId));
        }
        
        public bool Check(Dictionary<CurrencyProvider, int> amountsByProviders)
        {
            foreach (var entry in amountsByProviders)
            {
                if (_currencyProviders.TryGetValue(entry.Key.TypeId, out CurrencyProvider provider))
                {
                    if (!provider.Check(entry.Value))
                        return false;
                    
                    continue;
                }
                
                return false;
            }

            return true;
        }

        public override void Dispose()
        {
            _currencyProviders.Clear();
        }
        
        public void SubscribeOnce(CurrencyProvider currencyProvider)
        {
        }
        
        protected override void Initialize()
        {
        }
    }
}
