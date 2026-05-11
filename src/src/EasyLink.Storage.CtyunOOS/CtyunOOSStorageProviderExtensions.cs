using Microsoft.Extensions.DependencyInjection;

namespace EasyLink.Storage
{
    public static class CtyunOOSStorageProviderExtensions
    {
        public static IServiceCollection AddCtyunOOSStorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(StorageProvider.Ctyun, (cache, options) => new CtyunOSSService(cache, options));
        }
    }
}
