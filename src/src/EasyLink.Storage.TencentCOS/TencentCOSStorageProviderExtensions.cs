using Microsoft.Extensions.DependencyInjection;

namespace EasyLink.Storage
{
    /// <summary>
    /// Extension methods for registering the Tencent COS provider.
    /// </summary>
    public static class TencentCOSStorageProviderExtensions
    {
        /// <summary>
        /// Registers the Tencent COS provider for EasyLink.Storage.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddTencentCOSStorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(StorageProvider.QCloud, (cache, options) => new QCloudOSSService(cache, options));
        }
    }
}
