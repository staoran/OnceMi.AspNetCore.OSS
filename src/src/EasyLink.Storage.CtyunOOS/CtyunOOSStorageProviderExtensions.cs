using Microsoft.Extensions.DependencyInjection;

namespace EasyLink.Storage
{
    /// <summary>
    /// Extension methods for registering the Ctyun OOS provider.
    /// </summary>
    public static class CtyunOOSStorageProviderExtensions
    {
        /// <summary>
        /// Registers the Ctyun OOS provider for EasyLink.Storage.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddCtyunOOSStorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(StorageProvider.Ctyun, (cache, options) => new CtyunOSSService(cache, options));
        }
    }
}
