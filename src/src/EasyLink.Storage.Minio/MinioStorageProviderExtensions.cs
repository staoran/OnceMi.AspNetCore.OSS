using Microsoft.Extensions.DependencyInjection;

namespace EasyLink.Storage
{
    public static class MinioStorageProviderExtensions
    {
        public static IServiceCollection AddMinioStorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(StorageProvider.Minio, (cache, options) => new MinioOSSService(cache, options));
        }
    }
}
