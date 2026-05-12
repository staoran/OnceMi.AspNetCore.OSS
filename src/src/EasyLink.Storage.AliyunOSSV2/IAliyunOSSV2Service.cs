using System.Threading;
using System.Threading.Tasks;
using OSS = AlibabaCloud.OSS.V2;

namespace EasyLink.Storage
{
    /// <summary>
    /// 阿里云 OSS V2 provider 的扩展能力。
    /// </summary>
    public interface IAliyunOSSV2Service : IOSSService
    {
        /// <summary>
        /// 获取底层的 AlibabaCloud.OSS.V2 客户端。
        /// </summary>
        OSS.Client Context { get; }

        /// <summary>
        /// 获取存储桶所在地域。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>存储桶地域。</returns>
        Task<string> GetBucketLocationAsync(string bucketName, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取存储桶公网访问地址。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>存储桶公网访问地址。</returns>
        Task<string> GetBucketEndpointAsync(string bucketName, CancellationToken cancellationToken = default);
    }
}
