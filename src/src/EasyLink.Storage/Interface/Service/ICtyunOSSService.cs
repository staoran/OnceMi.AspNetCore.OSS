using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLink.Storage
{
    /// <summary>
    /// Ctyun OOS provider-specific operations.
    /// </summary>
    public interface ICtyunOSSService : IOSSService
    {
        /// <summary>
        /// 获取存储桶地域。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns>存储桶地域。</returns>
        Task<string> GetBucketLocationAsync(string bucketName);

    }
}
