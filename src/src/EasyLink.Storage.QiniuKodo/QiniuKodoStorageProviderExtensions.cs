using Microsoft.Extensions.DependencyInjection;

namespace EasyLink.Storage
{
    /// <summary>
    /// Extension methods for registering the Qiniu Kodo provider.
    /// </summary>
    public static class QiniuKodoStorageProviderExtensions
    {
        /// <summary>
        /// Registers the Qiniu Kodo provider for EasyLink.Storage.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddQiniuKodoStorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(StorageProvider.Qiniu, (cache, options) => new QiniuOSSService(cache, options));
        }
    }
}
