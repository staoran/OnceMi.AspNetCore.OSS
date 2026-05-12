using Microsoft.Extensions.DependencyInjection;

namespace EasyLink.Storage
{
    /// <summary>
    /// Extension methods for registering the Aliyun OSS provider.
    /// </summary>
    public static class AliyunStorageProviderExtensions
    {
        /// <summary>
        /// Registers the Aliyun OSS provider for EasyLink.Storage.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddAliyunStorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(StorageProvider.Aliyun, (cache, options) => new AliyunOSSService(cache, options));
        }
    }
}
