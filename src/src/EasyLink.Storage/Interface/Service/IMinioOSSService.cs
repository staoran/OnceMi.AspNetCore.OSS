using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyLink.Storage
{
    /// <summary>
    /// Minio provider-specific operations.
    /// </summary>
    public interface IMinioOSSService : IOSSService
    {
        /// <summary>
        /// Removes an incomplete multipart upload.
        /// </summary>
        /// <param name="bucketName">The bucket name.</param>
        /// <param name="objectName">The object name.</param>
        /// <returns>True when the operation succeeds.</returns>
        Task<bool> RemoveIncompleteUploadAsync(string bucketName, string objectName);

        /// <summary>
        /// Lists incomplete multipart uploads in a bucket.
        /// </summary>
        /// <param name="bucketName">The bucket name.</param>
        /// <returns>The incomplete upload list.</returns>
        Task<List<ItemUploadInfo>> ListIncompleteUploads(string bucketName);

        /// <summary>
        /// Gets the bucket policy.
        /// </summary>
        /// <param name="bucketName">The bucket name.</param>
        /// <returns>The bucket policy.</returns>
        Task<PolicyInfo> GetPolicyAsync(string bucketName);

        /// <summary>
        /// Sets the bucket policy statements.
        /// </summary>
        /// <param name="bucketName">The bucket name.</param>
        /// <param name="statements">The policy statements.</param>
        /// <returns>True when the operation succeeds.</returns>
        Task<bool> SetPolicyAsync(string bucketName, List<StatementItem> statements);

        /// <summary>
        /// Removes the bucket policy.
        /// </summary>
        /// <param name="bucketName">The bucket name.</param>
        /// <returns>True when the operation succeeds.</returns>
        Task<bool> RemovePolicyAsync(string bucketName);

        /// <summary>
        /// Checks whether the bucket policy contains the specified statement.
        /// </summary>
        /// <param name="bucketName">The bucket name.</param>
        /// <param name="statement">The policy statement.</param>
        /// <returns>True when the statement exists.</returns>
        Task<bool> PolicyExistsAsync(string bucketName, StatementItem statement);
    }
}
