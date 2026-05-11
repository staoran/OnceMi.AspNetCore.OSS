using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace EasyLink.Storage
{
    public static class OSSServiceExtensions
    {
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

        public static IServiceCollection AddStorageService(this IServiceCollection services, string key)
        {
            return services.AddStorageService(DefaultOptionName.Name, key);
        }

        public static IServiceCollection AddStorageService(this IServiceCollection services, string name, string key)
        {
            return services.AddOSSService(name, key);
        }

        public static IServiceCollection AddStorageService(this IServiceCollection services, Action<OSSOptions> option)
        {
            return services.AddOSSService(option);
        }

        public static IServiceCollection AddStorageService(this IServiceCollection services, string name, Action<OSSOptions> option)
        {
            return services.AddOSSService(name, option);
        }

        /// <summary>
        /// �������ļ��м���Ĭ������
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IServiceCollection AddOSSService(this IServiceCollection services, string key)
        {
            return services.AddOSSService(DefaultOptionName.Name, key);
        }

        /// <summary>
        /// �������ļ��м���
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="configuration"></param>
        /// <param name="key"></param>
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
        /// ����Ĭ������
        /// </summary>
        public static IServiceCollection AddOSSService(this IServiceCollection services, Action<OSSOptions> option)
        {
            return services.AddOSSService(DefaultOptionName.Name, option);
        }

        /// <summary>
        /// ������������
        /// </summary>
        public static IServiceCollection AddOSSService(this IServiceCollection services, string name, Action<OSSOptions> option)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = DefaultOptionName.Name;
            }
            services.Configure(name, option);
            //����IOSSServiceFactoryֻ��Ҫע��һ��
            if (!services.Any(p => p.ServiceType == typeof(IOSSServiceFactory)))
            {
                //���δע��ICacheProvider��Ĭ��ע��MemoryCacheProvider
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
