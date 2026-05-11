using Microsoft.Extensions.DependencyInjection;

namespace EasyLink.Storage
{
    public static class TencentCOSStorageProviderExtensions
    {
        public static IServiceCollection AddTencentCOSStorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(StorageProvider.QCloud, (cache, options) => new QCloudOSSService(cache, options));
        }
    }
}
