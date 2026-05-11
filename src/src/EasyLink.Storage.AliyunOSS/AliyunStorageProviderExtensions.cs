using Microsoft.Extensions.DependencyInjection;

namespace EasyLink.Storage
{
    public static class AliyunStorageProviderExtensions
    {
        public static IServiceCollection AddAliyunStorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(StorageProvider.Aliyun, (cache, options) => new AliyunOSSService(cache, options));
        }
    }
}
