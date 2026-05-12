using Microsoft.Extensions.DependencyInjection;

namespace EasyLink.Storage
{
    /// <summary>
    /// Extension methods for registering the Baidu BOS provider.
    /// </summary>
    public static class BaiduBOSStorageProviderExtensions
    {
        /// <summary>
        /// Registers the Baidu BOS provider for EasyLink.Storage.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddBaiduBOSStorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(StorageProvider.BaiduCloud, (cache, options) => new BaiduOSSService(cache, options));
        }
    }
}
