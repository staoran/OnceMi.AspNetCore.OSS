using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyLink.Storage
{
    /// <summary>
    /// Extension methods for registering the Huawei OBS provider.
    /// </summary>
    public static class HuaweiOBSStorageProviderExtensions
    {
        /// <summary>
        /// Registers the Huawei OBS provider for EasyLink.Storage.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddHuaweiOBSStorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(
                StorageProvider.HuaweiCloud,
                (serviceProvider, cache, options) =>
                {
                    ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                    return new HaweiOSSService(cache, options, loggerFactory);
                });
        }
    }
}
