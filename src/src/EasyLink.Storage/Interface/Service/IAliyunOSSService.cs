using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLink.Storage
{
    /// <summary>
    /// Aliyun OSS provider-specific operations.
    /// </summary>
    public interface IAliyunOSSService : IOSSService
    {
        /// <summary>
        /// 获取存储桶地域。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns>存储桶地域。</returns>
        Task<string> GetBucketLocationAsync(string bucketName);

        /// <summary>
        /// 设置存储桶跨域访问规则。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="rules">跨域访问规则。</param>
        /// <returns>如果设置成功，则为 true；否则为 false。</returns>
        Task<bool> SetBucketCorsRequestAsync(string bucketName, List<BucketCorsRule> rules);

        /// <summary>
        /// 获取存储桶外部访问 URL。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns>存储桶外部访问 URL。</returns>
        Task<string> GetBucketEndpointAsync(string bucketName);
    }
}
