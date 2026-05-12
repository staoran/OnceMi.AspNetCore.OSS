
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyLink.Storage
{
    /// <summary>
    /// Qiniu Kodo provider-specific operations.
    /// </summary>
    public interface IQiniuOSSService : IOSSService
    {
        /// <summary>
        /// 获取存储桶信息。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns>存储桶信息。</returns>
        Task<Bucket> GetBucketInfoAsync(string bucketName);

        /// <summary>
        /// 获取存储桶绑定域名。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns>绑定域名列表。</returns>
        Task<List<string>> GetBucketDomainNameAsync(string bucketName);
    }
}
