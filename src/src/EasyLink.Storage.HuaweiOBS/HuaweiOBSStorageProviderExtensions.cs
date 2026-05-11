using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyLink.Storage
{
    public static class HuaweiOBSStorageProviderExtensions
    {
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
