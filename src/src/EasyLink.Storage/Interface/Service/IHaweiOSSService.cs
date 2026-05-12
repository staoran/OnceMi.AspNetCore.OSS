
using OBS.Model;
using EasyLink.Storage.Models.Huawei;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyLink.Storage
{
    /// <summary>
    /// Huawei OBS provider-specific operations.
    /// </summary>
    public interface IHaweiOSSService : IOSSService
    {
        /// <summary>
        /// 获取存储桶容量信息。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns>存储桶容量信息。</returns>
        Task<BucketStorageInfo> GetBucketStorageInfoAsync(string bucketName);

        /// <summary>
        /// 设置存储桶存储类型。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="type">存储类型。</param>
        /// <returns>如果设置成功，则为 true；否则为 false。</returns>
        /// <remarks>
        /// 标准存储(StorageClassEnum.Standard) 标准存储拥有低访问时延和较高的吞吐量，适用于有大量热点对象（平均一个月多次）或小对象（<1MB），且需要频繁访问数据的业务场景。
        /// 低频访问存储(StorageClassEnum.Warm) 低频访问存储适用于不频繁访问（平均一年少于12次）但在需要时也要求能够快速访问数据的业务场景。
        /// 归档存储(StorageClassEnum.Cold) 归档存储适用于很少访问（平均一年访问一次）数据的业务场景。
        /// </remarks>
        Task<bool> SetBucketStoragePolicyAsync(string bucketName, StorageClassEnum type);

        /// <summary>
        /// 获取存储桶存储类型。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns>存储桶存储类型。</returns>
        /// <exception cref="System.Exception">无法获取存储类型时抛出。</exception>
        /// <remarks>
        /// 标准存储(StorageClassEnum.Standard) 标准存储拥有低访问时延和较高的吞吐量，适用于有大量热点对象（平均一个月多次）或小对象（<1MB），且需要频繁访问数据的业务场景。
        /// 低频访问存储(StorageClassEnum.Warm) 低频访问存储适用于不频繁访问（平均一年少于12次）但在需要时也要求能够快速访问数据的业务场景。
        /// 归档存储(StorageClassEnum.Cold) 归档存储适用于很少访问（平均一年访问一次）数据的业务场景。
        /// </remarks>
        Task<StorageClassEnum> GetBucketStoragePolicyAsync(string bucketName);
    }
}
