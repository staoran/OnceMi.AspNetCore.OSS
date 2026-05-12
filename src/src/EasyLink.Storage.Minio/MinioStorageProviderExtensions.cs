using Microsoft.Extensions.DependencyInjection;

namespace EasyLink.Storage
{
    /// <summary>
    /// Extension methods for registering the Minio provider.
    /// </summary>
    public static class MinioStorageProviderExtensions
    {
        /// <summary>
        /// Registers the Minio provider for EasyLink.Storage.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddMinioStorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(StorageProvider.Minio, (cache, options) => new MinioOSSService(cache, options));
        }
    }
}
