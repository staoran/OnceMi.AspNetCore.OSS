using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyLink.Storage
{
    /// <summary>
    /// Creates an object storage service for a provider.
    /// </summary>
    /// <param name="cache">The cache provider.</param>
    /// <param name="options">The storage options.</param>
    /// <returns>The created object storage service.</returns>
    public delegate IOSSService StorageProviderFactory(ICacheProvider cache, OSSOptions options);

    /// <summary>
    /// Creates an object storage service for a provider with access to the application service provider.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    /// <param name="cache">The cache provider.</param>
    /// <param name="options">The storage options.</param>
    /// <returns>The created object storage service.</returns>
    public delegate IOSSService ServiceStorageProviderFactory(IServiceProvider serviceProvider, ICacheProvider cache, OSSOptions options);

    /// <summary>
    /// Stores registered provider factories used by <see cref="IOSSServiceFactory"/>.
    /// </summary>
    public class StorageProviderRegistry
    {
        private readonly Dictionary<StorageProvider, ServiceStorageProviderFactory> factories = new Dictionary<StorageProvider, ServiceStorageProviderFactory>();

        /// <summary>
        /// Registers a provider factory.
        /// </summary>
        /// <param name="provider">The storage provider.</param>
        /// <param name="factory">The provider factory.</param>
        public void Register(StorageProvider provider, StorageProviderFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            Register(provider, (serviceProvider, cache, options) => factory(cache, options));
        }

        /// <summary>
        /// Registers a provider factory that can resolve additional services.
        /// </summary>
        /// <param name="provider">The storage provider.</param>
        /// <param name="factory">The provider factory.</param>
        public void Register(StorageProvider provider, ServiceStorageProviderFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            factories[provider] = factory;
        }

        /// <summary>
        /// Attempts to get the registered factory for a provider.
        /// </summary>
        /// <param name="provider">The storage provider.</param>
        /// <param name="factory">The registered provider factory.</param>
        /// <returns>True when a factory is registered.</returns>
        public bool TryGetFactory(StorageProvider provider, out ServiceStorageProviderFactory factory)
        {
            return factories.TryGetValue(provider, out factory);
        }

        /// <summary>
        /// Describes the currently registered providers.
        /// </summary>
        /// <returns>A comma-separated provider list.</returns>
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
