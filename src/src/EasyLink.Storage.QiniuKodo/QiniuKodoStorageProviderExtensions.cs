using Microsoft.Extensions.DependencyInjection;

namespace EasyLink.Storage
{
    public static class QiniuKodoStorageProviderExtensions
    {
        public static IServiceCollection AddQiniuKodoStorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(StorageProvider.Qiniu, (cache, options) => new QiniuOSSService(cache, options));
        }
    }
}
