using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyLink.Storage
{
    public delegate IOSSService StorageProviderFactory(ICacheProvider cache, OSSOptions options);
    public delegate IOSSService ServiceStorageProviderFactory(IServiceProvider serviceProvider, ICacheProvider cache, OSSOptions options);

    public class StorageProviderRegistry
    {
        private readonly Dictionary<StorageProvider, ServiceStorageProviderFactory> factories = new Dictionary<StorageProvider, ServiceStorageProviderFactory>();

        public void Register(StorageProvider provider, StorageProviderFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            Register(provider, (serviceProvider, cache, options) => factory(cache, options));
        }

        public void Register(StorageProvider provider, ServiceStorageProviderFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            factories[provider] = factory;
        }

        public bool TryGetFactory(StorageProvider provider, out ServiceStorageProviderFactory factory)
        {
            return factories.TryGetValue(provider, out factory);
        }

        public string DescribeRegisteredProviders()
        {
            if (factories.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(", ", factories.Keys.OrderBy(p => p));
        }
    }
}
