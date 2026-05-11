using Microsoft.Extensions.DependencyInjection;

namespace EasyLink.Storage
{
    public static class HuaweiOBSStorageProviderExtensions
    {
        public static IServiceCollection AddHuaweiOBSStorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(StorageProvider.HuaweiCloud, (cache, options) => new HaweiOSSService(cache, options));
        }
    }
}
