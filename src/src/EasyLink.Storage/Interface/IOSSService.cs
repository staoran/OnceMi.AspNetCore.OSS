using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EasyLink.Storage
{
    public interface IOSSService
    {
        /// <summary>
        /// 当前服务使用的对象存储配置。
        /// </summary>
        OSSOptions Options { get; }

        /// <summary>
        /// 检查存储桶是否存在。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns>如果存储桶存在，则为 true；否则为 false。</returns>
        Task<bool> BucketExistsAsync(string bucketName);

        /// <summary>
        /// 创建一个存储桶。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns>如果创建成功，则为 true；否则为 false。</returns>
        Task<bool> CreateBucketAsync(string bucketName);

        /// <summary>
        /// 删除一个存储桶。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns>如果删除成功，则为 true；否则为 false。</returns>
        Task<bool> RemoveBucketAsync(string bucketName);

        /// <summary>
        /// 列出所有的存储桶。
        /// </summary>
        /// <returns>当前账号可访问的存储桶列表。</returns>
        Task<List<Bucket>> ListBucketsAsync();

        /// <summary>
        /// 设置存储桶的访问权限。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="mode">访问权限。</param>
        /// <returns>如果设置成功，则为 true；否则为 false。</returns>
        Task<bool> SetBucketAclAsync(string bucketName, AccessMode mode);

        /// <summary>
        /// 获取存储桶的访问权限。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <returns>存储桶访问权限。</returns>
        Task<AccessMode> GetBucketAclAsync(string bucketName);

        /// <summary>
        /// 检查存储桶中的对象是否存在。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">对象名称。</param>
        /// <returns>如果对象存在，则为 true；否则为 false。</returns>
        Task<bool> ObjectsExistsAsync(string bucketName, string objectName);

        /// <summary>
        /// 列出存储桶里的对象。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="prefix">可选对象名前缀。</param>
        /// <returns>对象列表。</returns>
        Task<List<Item>> ListObjectsAsync(string bucketName, string prefix = null);

        /// <summary>
        /// 返回对象数据的流。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">对象名称。</param>
        /// <param name="callback">处理流的回调函数。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>异步操作任务。</returns>
        Task GetObjectAsync(string bucketName, string objectName, Action<Stream> callback, CancellationToken cancellationToken = default);

        /// <summary>
        /// 下载并将文件保存到本地文件系统。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">对象名称。</param>
        /// <param name="fileName">本地文件路径。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>异步操作任务。</returns>
        Task GetObjectAsync(string bucketName, string objectName, string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// 通过 <see cref="Stream"/> 上传对象。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">对象名称。</param>
        /// <param name="data">要上传的流。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>如果上传成功，则为 true；否则为 false。</returns>
        Task<bool> PutObjectAsync(string bucketName, string objectName, Stream data, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 上传本地文件到对象存储。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">对象名称。</param>
        /// <param name="filePath">要上传的本地文件路径。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>如果上传成功，则为 true；否则为 false。</returns>
        Task<bool> PutObjectAsync(string bucketName, string objectName, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取对象的元数据。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">对象名称。</param>
        /// <param name="versionID">可选版本 ID。</param>
        /// <param name="matchEtag">可选 ETag 匹配条件。</param>
        /// <param name="modifiedSince">可选修改时间条件。</param>
        /// <returns>对象元数据。</returns>
        Task<ItemMeta> GetObjectMetadataAsync(string bucketName
            , string objectName
            , string versionID = null
            , string matchEtag = null
            , DateTime? modifiedSince = null);

        /// <summary>
        /// 将源对象复制到目标存储桶中的目标对象。
        /// </summary>
        /// <param name="bucketName">源存储桶名称。</param>
        /// <param name="objectName">源存储桶中的源对象名称。</param>
        /// <param name="destBucketName">目标存储桶名称。</param>
        /// <param name="destObjectName">目标对象名称；为空时使用源对象名称。</param>
        /// <returns>如果复制成功，则为 true；否则为 false。</returns>
        Task<bool> CopyObjectAsync(string bucketName, string objectName, string destBucketName, string destObjectName = null);

        /// <summary>
        /// 删除一个对象。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">对象名称。</param>
        /// <returns>如果删除成功，则为 true；否则为 false。</returns>
        Task<bool> RemoveObjectAsync(string bucketName, string objectName);

        /// <summary>
        /// 删除多个对象。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectNames">对象名称列表。</param>
        /// <returns>如果删除成功，则为 true；否则为 false。</returns>
        Task<bool> RemoveObjectAsync(string bucketName, List<string> objectNames);

        /// <summary>
        /// 清除对象的预签名 URL 缓存。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">对象名称。</param>
        /// <returns>异步操作任务。</returns>
        Task RemovePresignedUrlCache(string bucketName, string objectName);

        /// <summary>
        /// 生成 HTTP GET 请求使用的预签名 URL。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">对象名称。</param>
        /// <param name="expiresInt">有效期，单位为秒，不得超过 7 天。</param>
        /// <returns>预签名下载 URL。</returns>
        Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt);

        /// <summary>
        /// 生成 HTTP PUT 请求使用的预签名 URL。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">对象名称。</param>
        /// <param name="expiresInt">有效期，单位为秒，不得超过 7 天。</param>
        /// <returns>预签名上传 URL。</returns>
        Task<string> PresignedPutObjectAsync(string bucketName, string objectName, int expiresInt);

        /// <summary>
        /// 设置对象的访问权限。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">对象名称。</param>
        /// <param name="mode">访问权限。</param>
        /// <returns>如果设置成功，则为 true；否则为 false。</returns>
        Task<bool> SetObjectAclAsync(string bucketName, string objectName, AccessMode mode);

        /// <summary>
        /// 获取对象的访问权限。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">对象名称。</param>
        /// <returns>对象访问权限。</returns>
        Task<AccessMode> GetObjectAclAsync(string bucketName, string objectName);

        /// <summary>
        /// 清除对象 ACL，使对象继承存储桶的访问权限。
        /// </summary>
        /// <param name="bucketName">存储桶名称。</param>
        /// <param name="objectName">对象名称。</param>
        /// <returns>清除后的对象访问权限。</returns>
        Task<AccessMode> RemoveObjectAclAsync(string bucketName, string objectName);
    }
}
