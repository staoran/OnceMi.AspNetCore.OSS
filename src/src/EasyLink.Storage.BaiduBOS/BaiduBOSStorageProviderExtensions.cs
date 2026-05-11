using Microsoft.Extensions.DependencyInjection;

namespace EasyLink.Storage
{
    public static class BaiduBOSStorageProviderExtensions
    {
        public static IServiceCollection AddBaiduBOSStorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(StorageProvider.BaiduCloud, (cache, options) => new BaiduOSSService(cache, options));
        }
    }
}
