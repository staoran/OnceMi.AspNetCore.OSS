using Microsoft.Extensions.Options;
using System;

namespace EasyLink.Storage
{
    public class OSSServiceFactory : IOSSServiceFactory
    {
        private readonly IOptionsMonitor<OSSOptions> optionsMonitor;
        private readonly ICacheProvider _cache;
        private readonly StorageProviderRegistry registry;
        private readonly IServiceProvider serviceProvider;

        public OSSServiceFactory(IOptionsMonitor<OSSOptions> optionsMonitor
            , ICacheProvider provider
            , IOptions<StorageProviderRegistry> registryOptions
            , IServiceProvider serviceProvider)
        {
            this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException();
            this._cache = provider ?? throw new ArgumentNullException(nameof(provider));
            this.registry = registryOptions?.Value ?? throw new ArgumentNullException(nameof(registryOptions));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IOSSService Create()
        {
            return Create(DefaultOptionName.Name);
        }

        public IOSSService Create(string name)
        {
            #region Options validation

            if (string.IsNullOrEmpty(name))
            {
                name = DefaultOptionName.Name;
            }
            var options = optionsMonitor.Get(name);
            if (options == null ||
                (options.Provider == StorageProvider.Invalid
                && string.IsNullOrEmpty(options.Endpoint)
                && string.IsNullOrEmpty(options.SecretKey)
                && string.IsNullOrEmpty(options.AccessKey)))
                throw new ArgumentException($"Cannot get option by name '{name}'.");
            if (options.Provider == StorageProvider.Invalid)
                throw new ArgumentNullException(nameof(options.Provider));
            if (string.IsNullOrEmpty(options.Endpoint) && options.Provider != StorageProvider.Qiniu)
                throw new ArgumentNullException(nameof(options.Endpoint), "When your provider is Minio/QCloud/Aliyun/HuaweiCloud, endpoint can not null.");
            if (string.IsNullOrEmpty(options.SecretKey))
                throw new ArgumentNullException(nameof(options.SecretKey), "SecretKey can not null.");
            if (string.IsNullOrEmpty(options.AccessKey))
                throw new ArgumentNullException(nameof(options.AccessKey), "AccessKey can not null.");
            if ((options.Provider == StorageProvider.Minio
                || options.Provider == StorageProvider.QCloud
                || options.Provider == StorageProvider.Qiniu
                || options.Provider == StorageProvider.HuaweiCloud)
                && string.IsNullOrEmpty(options.Region))
            {
                throw new ArgumentNullException(nameof(options.Region), "When your provider is Minio/QCloud/Qiniu/HuaweiCloud, region can not null.");
            }

            #endregion

            if (!registry.TryGetFactory(options.Provider, out ServiceStorageProviderFactory factory))
            {
                string registered = registry.DescribeRegisteredProviders();
                string packageHint = GetProviderPackageHint(options.Provider);
                string message = $"Provider '{options.Provider}' is not registered. Install '{packageHint}' and call its AddXxxStorageProvider extension.";
                if (!string.IsNullOrEmpty(registered))
                {
                    message += $" Registered providers: {registered}.";
                }

                throw new InvalidOperationException(message);
            }

            return factory(serviceProvider, _cache, options);
        }

        private static string GetProviderPackageHint(StorageProvider provider)
        {
            return provider switch
            {
                StorageProvider.Minio => "EasyLink.Storage.Minio",
                StorageProvider.Aliyun => "EasyLink.Storage.AliyunOSS",
                StorageProvider.QCloud => "EasyLink.Storage.TencentCOS",
                StorageProvider.Qiniu => "EasyLink.Storage.QiniuKodo",
                StorageProvider.HuaweiCloud => "EasyLink.Storage.HuaweiOBS",
                StorageProvider.BaiduCloud => "EasyLink.Storage.BaiduBOS",
                StorageProvider.Ctyun => "EasyLink.Storage.CtyunOOS",
                _ => "the matching provider package",
            };
        }
    }
}
