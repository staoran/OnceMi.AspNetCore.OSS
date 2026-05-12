using Microsoft.Extensions.DependencyInjection;

namespace EasyLink.Storage
{
    /// <summary>
    /// 阿里云 OSS V2 provider 的注册扩展。
    /// </summary>
    public static class AliyunOSSV2StorageProviderExtensions
    {
        /// <summary>
        /// 注册阿里云 OSS V2 provider。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <returns>当前服务集合。</returns>
        public static IServiceCollection AddAliyunOSSV2StorageProvider(this IServiceCollection services)
        {
            return services.AddStorageProvider(StorageProvider.AliyunV2, (cache, options) => new AliyunOSSV2Service(cache, options));
        }
    }
}
