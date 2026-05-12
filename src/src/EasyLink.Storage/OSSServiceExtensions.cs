using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace EasyLink.Storage
{
    /// <summary>
    /// Service collection extensions for registering EasyLink.Storage services and providers.
    /// </summary>
    public static class OSSServiceExtensions
    {
        /// <summary>
        /// Registers a provider factory for the specified storage provider.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="provider">The provider type handled by the factory.</param>
        /// <param name="factory">The factory that creates the provider service.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddStorageProvider(this IServiceCollection services, StorageProvider provider, StorageProviderFactory factory)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            services.Configure<StorageProviderRegistry>(registry => registry.Register(provider, factory));
            return services;
        }

        /// <summary>
        /// Registers a provider factory that can resolve additional services from the application service provider.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="provider">The provider type handled by the factory.</param>
        /// <param name="factory">The factory that creates the provider service.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddStorageProvider(this IServiceCollection services, StorageProvider provider, ServiceStorageProviderFactory factory)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            services.Configure<StorageProviderRegistry>(registry => registry.Register(provider, factory));
            return services;
        }

        /// <summary>
        /// Adds the default named storage service from a configuration section.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="key">The configuration section key.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddStorageService(this IServiceCollection services, string key)
        {
            return services.AddStorageService(DefaultOptionName.Name, key);
        }

        /// <summary>
        /// Adds a named storage service from a configuration section.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="name">The storage service name.</param>
        /// <param name="key">The configuration section key.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddStorageService(this IServiceCollection services, string name, string key)
        {
            return services.AddOSSService(name, key);
        }

        /// <summary>
        /// Adds the default named storage service using code-based options.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="option">The options configuration action.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddStorageService(this IServiceCollection services, Action<OSSOptions> option)
        {
            return services.AddOSSService(option);
        }

        /// <summary>
        /// Adds a named storage service using code-based options.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="name">The storage service name.</param>
        /// <param name="option">The options configuration action.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddStorageService(this IServiceCollection services, string name, Action<OSSOptions> option)
        {
            return services.AddOSSService(name, option);
        }

        /// <summary>
        /// Adds the default named OSS service from a configuration section.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="key">The configuration section key.</param>
        /// <returns></returns>
        public static IServiceCollection AddOSSService(this IServiceCollection services, string key)
        {
            return services.AddOSSService(DefaultOptionName.Name, key);
        }

        /// <summary>
        /// Adds a named OSS service from a configuration section.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name">The OSS service name.</param>
        /// <param name="key">The configuration section key.</param>
        /// <returns></returns>
        public static IServiceCollection AddOSSService(this IServiceCollection services, string name, string key)
        {
            using (ServiceProvider provider = services.BuildServiceProvider())
            {
                IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
                if (configuration == null)
                {
                    throw new ArgumentNullException(nameof(IConfiguration));
                }
                IConfigurationSection section = configuration.GetSection(key);
                if (!section.Exists())
                {
                    throw new Exception($"Config file not exist '{key}' section.");
                }
                OSSOptions options = section.Get<OSSOptions>();
                if (options == null)
                {
                    throw new Exception($"Get OSS option from config file failed.");
                }
                return services.AddOSSService(name, o =>
                 {
                     o.AccessKey = options.AccessKey;
                     o.Endpoint = options.Endpoint;
                     o.IsEnableCache = options.IsEnableCache;
                     o.IsEnableHttps = options.IsEnableHttps;
                     o.Provider = options.Provider;
                     o.Region = options.Region;
                     o.SecretKey = options.SecretKey;
                 });
            }
        }

        /// <summary>
        /// Adds the default named OSS service using code-based options.
        /// </summary>
        public static IServiceCollection AddOSSService(this IServiceCollection services, Action<OSSOptions> option)
        {
            return services.AddOSSService(DefaultOptionName.Name, option);
        }

        /// <summary>
        /// Adds a named OSS service using code-based options.
        /// </summary>
        public static IServiceCollection AddOSSService(this IServiceCollection services, string name, Action<OSSOptions> option)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = DefaultOptionName.Name;
            }
            services.Configure(name, option);
            // The factory only needs to be registered once.
            if (!services.Any(p => p.ServiceType == typeof(IOSSServiceFactory)))
            {
                // Use the default memory cache provider when the application has not registered one.
                if (!services.Any(p => p.ServiceType == typeof(ICacheProvider)))
                {
                    services.AddMemoryCache();
                    services.TryAddSingleton<ICacheProvider, MemoryCacheProvider>();
                }
                services.TryAddSingleton<IOSSServiceFactory, OSSServiceFactory>();
            }
            //
            services.TryAddScoped(sp => sp.GetRequiredService<IOSSServiceFactory>().Create(name));
            return services;
        }
    }
}
