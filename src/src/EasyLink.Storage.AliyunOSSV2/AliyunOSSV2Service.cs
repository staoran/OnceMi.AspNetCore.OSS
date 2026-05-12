using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSS = AlibabaCloud.OSS.V2;
using OSSModels = AlibabaCloud.OSS.V2.Models;

namespace EasyLink.Storage
{
    /// <summary>
    /// 基于 AlibabaCloud.OSS.V2 的阿里云 OSS provider 实现。
    /// </summary>
    public class AliyunOSSV2Service : BaseOSSService, IAliyunOSSV2Service, IDisposable
    {
        private const string PrivateAcl = "private";
        private const string PublicReadAcl = "public-read";
        private const string PublicReadWriteAcl = "public-read-write";
        private const string DefaultAcl = "default";

        private readonly OSS.Client _client;

        /// <inheritdoc />
        public OSS.Client Context => _client;

        public AliyunOSSV2Service(ICacheProvider cache, OSSOptions options)
            : base(cache, options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), "The OSSOptions can not null");
            }

            _client = CreateClient(options);
        }

        #region 存储桶

        public async Task<List<Bucket>> ListBucketsAsync()
        {
            var buckets = new List<Bucket>();
            string marker = null;
            OSSModels.ListBucketsResult page;

            do
            {
                page = await _client.ListBucketsAsync(new OSSModels.ListBucketsRequest
                {
                    Marker = marker,
                    MaxKeys = 100
                });

                if (page?.Buckets != null)
                {
                    foreach (var item in page.Buckets)
                    {
                        buckets.Add(new Bucket
                        {
                            Location = item.Location ?? item.Region,
                            Name = item.Name,
                            Owner = page.Owner == null
                                ? null
                                : new Owner
                                {
                                    Id = page.Owner.Id,
                                    Name = page.Owner.DisplayName
                                },
                            CreationDate = item.CreationDate?.ToString("yyyy-MM-dd HH:mm:ss")
                        });
                    }
                }

                marker = page?.NextMarker;
            } while (page?.IsTruncated == true);

            return buckets;
        }

        public Task<bool> BucketExistsAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            return _client.IsBucketExistAsync(bucketName);
        }

        public async Task<bool> CreateBucketAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            if (await BucketExistsAsync(bucketName))
            {
                throw new BucketExistException($"Bucket '{bucketName}' already exists.");
            }

            var result = await _client.PutBucketAsync(new OSSModels.PutBucketRequest
            {
                Bucket = bucketName,
                Acl = PrivateAcl,
                CreateBucketConfiguration = new OSSModels.CreateBucketConfiguration
                {
                    DataRedundancyType = "LRS"
                }
            });

            return IsSuccessStatusCode(result?.StatusCode);
        }

        public async Task<bool> RemoveBucketAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            var result = await _client.DeleteBucketAsync(new OSSModels.DeleteBucketRequest
            {
                Bucket = bucketName
            });

            return IsSuccessStatusCode(result?.StatusCode);
        }

        public async Task<string> GetBucketLocationAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            var result = await _client.GetBucketLocationAsync(new OSSModels.GetBucketLocationRequest
            {
                Bucket = bucketName
            }, cancellationToken: cancellationToken);

            return result?.LocationConstraint;
        }

        public async Task<string> GetBucketEndpointAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            var result = await _client.GetBucketInfoAsync(new OSSModels.GetBucketInfoRequest
            {
                Bucket = bucketName
            }, cancellationToken: cancellationToken);

            var bucket = result?.BucketInfo;
            if (bucket == null || string.IsNullOrEmpty(bucket.Name) || string.IsNullOrEmpty(bucket.ExtranetEndpoint))
            {
                return string.Empty;
            }

            string endpoint = StripScheme(bucket.ExtranetEndpoint);
            return $"{(Options.IsEnableHttps ? "https" : "http")}://{bucket.Name}.{endpoint}";
        }

        public async Task<bool> SetBucketAclAsync(string bucketName, AccessMode mode)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            var result = await _client.PutBucketAclAsync(new OSSModels.PutBucketAclRequest
            {
                Bucket = bucketName,
                Acl = ToBucketAcl(mode)
            });

            return IsSuccessStatusCode(result?.StatusCode);
        }

        public async Task<AccessMode> GetBucketAclAsync(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            var result = await _client.GetBucketAclAsync(new OSSModels.GetBucketAclRequest
            {
                Bucket = bucketName
            });

            return FromAcl(result?.AccessControlPolicy?.AccessControlList?.Grant);
        }

        #endregion

        #region 对象

        public async Task GetObjectAsync(string bucketName, string objectName, Action<Stream> callback, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            objectName = FormatObjectName(objectName);
            var result = await _client.GetObjectAsync(new OSSModels.GetObjectRequest
            {
                Bucket = bucketName,
                Key = objectName
            }, cancellationToken: cancellationToken);

            if (result?.Body == null)
            {
                throw new Exception($"Get object '{objectName}' from bucket '{bucketName}' returned empty stream.");
            }

            using (result.Body)
            {
                callback(result.Body);
            }
        }

        public async Task GetObjectAsync(string bucketName, string objectName, string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            string fullPath = Path.GetFullPath(fileName);
            string parentPath = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(parentPath) && !Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }

            objectName = FormatObjectName(objectName);
            var result = await _client.GetObjectAsync(new OSSModels.GetObjectRequest
            {
                Bucket = bucketName,
                Key = objectName
            }, cancellationToken: cancellationToken);

            if (result?.Body == null)
            {
                throw new Exception($"Get object '{objectName}' from bucket '{bucketName}' returned empty stream.");
            }

            using (result.Body)
            using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
            {
                await result.Body.CopyToAsync(fileStream, 81920, cancellationToken);
            }
        }

        public async Task<List<Item>> ListObjectsAsync(string bucketName, string prefix = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            var items = new List<Item>();
            string continuationToken = null;
            OSSModels.ListObjectsV2Result page;

            do
            {
                page = await _client.ListObjectsV2Async(new OSSModels.ListObjectsV2Request
                {
                    Bucket = bucketName,
                    Prefix = prefix,
                    MaxKeys = 100,
                    ContinuationToken = continuationToken
                });

                if (page?.Contents != null)
                {
                    foreach (var item in page.Contents)
                    {
                        items.Add(new Item
                        {
                            Key = item.Key,
                            LastModified = item.LastModified?.ToString("yyyy-MM-dd HH:mm:ss"),
                            ETag = item.ETag,
                            Size = ToUnsignedSize(item.Size),
                            BucketName = bucketName,
                            IsDir = !string.IsNullOrWhiteSpace(item.Key) && item.Key[item.Key.Length - 1] == '/',
                            LastModifiedDateTime = item.LastModified
                        });
                    }
                }

                continuationToken = page?.NextContinuationToken;
            } while (page?.IsTruncated == true);

            return items;
        }

        public Task<bool> ObjectsExistsAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            objectName = FormatObjectName(objectName);
            return _client.IsObjectExistAsync(bucketName, objectName, null);
        }

        public Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            return PresignedObjectAsync(bucketName
                , objectName
                , expiresInt
                , PresignedObjectType.Get
                , async (bucket, key, expires) =>
                {
                    key = FormatObjectName(key);
                    AccessMode accessMode = await GetObjectAclAsync(bucket, key);
                    if (accessMode == AccessMode.PublicRead || accessMode == AccessMode.PublicReadWrite)
                    {
                        string bucketUrl = await GetBucketEndpointAsync(bucket);
                        if (!string.IsNullOrEmpty(bucketUrl))
                        {
                            return $"{bucketUrl}{(key.StartsWith("/") ? string.Empty : "/")}{key}";
                        }
                    }

                    var result = _client.Presign(new OSSModels.GetObjectRequest
                    {
                        Bucket = bucket,
                        Key = key
                    }, DateTime.Now.AddSeconds(expires));

                    if (result == null || string.IsNullOrEmpty(result.Url))
                    {
                        throw new Exception("Generate get presigned uri failed");
                    }

                    return result.Url;
                });
        }

        public Task<string> PresignedPutObjectAsync(string bucketName, string objectName, int expiresInt)
        {
            return PresignedObjectAsync(bucketName
                , objectName
                , expiresInt
                , PresignedObjectType.Put
                , (bucket, key, expires) =>
                {
                    key = FormatObjectName(key);
                    var result = _client.Presign(new OSSModels.PutObjectRequest
                    {
                        Bucket = bucket,
                        Key = key
                    }, DateTime.Now.AddSeconds(expires));

                    if (result == null || string.IsNullOrEmpty(result.Url))
                    {
                        throw new Exception("Generate put presigned uri failed");
                    }

                    return Task.FromResult(result.Url);
                });
        }

        public async Task<bool> PutObjectAsync(string bucketName, string objectName, Stream data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            objectName = FormatObjectName(objectName);
            var result = await _client.PutObjectAsync(new OSSModels.PutObjectRequest
            {
                Bucket = bucketName,
                Key = objectName,
                Body = data
            }, cancellationToken: cancellationToken);

            return IsSuccessStatusCode(result?.StatusCode);
        }

        public async Task<bool> PutObjectAsync(string bucketName, string objectName, string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (!File.Exists(filePath))
            {
                throw new Exception("Upload file is not exist.");
            }

            objectName = FormatObjectName(objectName);
            var result = await _client.PutObjectFromFileAsync(new OSSModels.PutObjectRequest
            {
                Bucket = bucketName,
                Key = objectName
            }, filePath, cancellationToken: cancellationToken);

            return IsSuccessStatusCode(result?.StatusCode);
        }

        public async Task<bool> CopyObjectAsync(string bucketName, string objectName, string destBucketName = null, string destObjectName = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            objectName = FormatObjectName(objectName);
            if (string.IsNullOrEmpty(destBucketName))
            {
                destBucketName = bucketName;
            }
            destObjectName = string.IsNullOrEmpty(destObjectName) ? objectName : FormatObjectName(destObjectName);

            var result = await _client.CopyObjectAsync(new OSSModels.CopyObjectRequest
            {
                Bucket = destBucketName,
                Key = destObjectName,
                SourceBucket = bucketName,
                SourceKey = objectName
            });

            return IsSuccessStatusCode(result?.StatusCode);
        }

        public async Task<bool> RemoveObjectAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            objectName = FormatObjectName(objectName);
            var result = await _client.DeleteObjectAsync(new OSSModels.DeleteObjectRequest
            {
                Bucket = bucketName,
                Key = objectName
            });

            return IsSuccessStatusCode(result?.StatusCode);
        }

        public async Task<bool> RemoveObjectAsync(string bucketName, List<string> objectNames)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }
            if (objectNames == null || objectNames.Count == 0)
            {
                throw new ArgumentNullException(nameof(objectNames));
            }

            var deleteObjects = objectNames
                .Select(name => new OSSModels.DeleteObject { Key = FormatObjectName(name) })
                .ToList();

            var result = await _client.DeleteMultipleObjectsAsync(new OSSModels.DeleteMultipleObjectsRequest
            {
                Bucket = bucketName,
                Objects = deleteObjects,
                Quiet = false
            });

            if (!IsSuccessStatusCode(result?.StatusCode))
            {
                return false;
            }

            if (result.DeletedObjects != null && result.DeletedObjects.Count != deleteObjects.Count)
            {
                throw new Exception("Some file delete failed.");
            }

            return true;
        }

        public async Task<ItemMeta> GetObjectMetadataAsync(string bucketName, string objectName, string versionID = null, string matchEtag = null, DateTime? modifiedSince = null)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            objectName = FormatObjectName(objectName);
            var result = await _client.HeadObjectAsync(new OSSModels.HeadObjectRequest
            {
                Bucket = bucketName,
                Key = objectName,
                VersionId = versionID,
                IfMatch = matchEtag,
                IfModifiedSince = modifiedSince?.ToUniversalTime().ToString("R")
            });

            return new ItemMeta
            {
                ObjectName = objectName,
                ContentType = result.ContentType,
                Size = result.ContentLength.GetValueOrDefault(),
                LastModified = ParseOssDate(result.LastModified),
                ETag = result.ETag,
                IsEnableHttps = Options.IsEnableHttps,
                MetaData = result.Metadata == null
                    ? new Dictionary<string, string>()
                    : new Dictionary<string, string>(result.Metadata)
            };
        }

        public async Task<bool> SetObjectAclAsync(string bucketName, string objectName, AccessMode mode)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            objectName = FormatObjectName(objectName);
            if (!await ObjectsExistsAsync(bucketName, objectName))
            {
                throw new Exception($"Object '{objectName}' not in bucket '{bucketName}'.");
            }

            var result = await _client.PutObjectAclAsync(new OSSModels.PutObjectAclRequest
            {
                Bucket = bucketName,
                Key = objectName,
                Acl = ToObjectAcl(mode)
            });

            return IsSuccessStatusCode(result?.StatusCode);
        }

        public async Task<AccessMode> GetObjectAclAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            objectName = FormatObjectName(objectName);
            if (!await ObjectsExistsAsync(bucketName, objectName))
            {
                throw new Exception($"Object '{objectName}' not in bucket '{bucketName}'.");
            }

            var result = await _client.GetObjectAclAsync(new OSSModels.GetObjectAclRequest
            {
                Bucket = bucketName,
                Key = objectName
            });

            AccessMode mode = FromAcl(result?.Acl ?? result?.AccessControlPolicy?.AccessControlList?.Grant);
            if (mode == AccessMode.Default)
            {
                return await GetBucketAclAsync(bucketName);
            }

            return mode;
        }

        public async Task<AccessMode> RemoveObjectAclAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            objectName = FormatObjectName(objectName);
            if (!await SetObjectAclAsync(bucketName, objectName, AccessMode.Default))
            {
                throw new Exception("Save new policy info failed when remove object acl.");
            }

            return await GetObjectAclAsync(bucketName, objectName);
        }

        #endregion

        /// <summary>
        /// 释放底层 OSS V2 客户端资源。
        /// </summary>
        public void Dispose()
        {
            _client?.Dispose();
        }

        /// <summary>
        /// 根据当前配置创建阿里云 OSS V2 客户端。
        /// </summary>
        private static OSS.Client CreateClient(OSSOptions options)
        {
            var config = OSS.Configuration.LoadDefault();
            config.CredentialsProvider = new OSS.Credentials.StaticCredentialsProvider(options.AccessKey, options.SecretKey);
            config.Region = options.Region;
            config.Endpoint = StripScheme(options.Endpoint);
            config.DisableSsl = !options.IsEnableHttps;
            return new OSS.Client(config);
        }

        /// <summary>
        /// 判断响应状态码是否表示成功。
        /// </summary>
        internal static bool IsSuccessStatusCode(int? statusCode)
        {
            return statusCode.HasValue && statusCode.Value >= 200 && statusCode.Value < 300;
        }

        /// <summary>
        /// 将带符号长度转换为无符号长度，负值按 0 处理。
        /// </summary>
        internal static ulong ToUnsignedSize(long? size)
        {
            long value = size.GetValueOrDefault();
            return value <= 0 ? 0 : (ulong)value;
        }

        /// <summary>
        /// 将统一访问模式转换为 OSS 存储桶 ACL。
        /// </summary>
        internal static string ToBucketAcl(AccessMode mode)
        {
            return mode switch
            {
                AccessMode.Private => PrivateAcl,
                AccessMode.PublicRead => PublicReadAcl,
                AccessMode.PublicReadWrite => PublicReadWriteAcl,
                _ => PrivateAcl,
            };
        }

        /// <summary>
        /// 将统一访问模式转换为 OSS 对象 ACL。
        /// </summary>
        internal static string ToObjectAcl(AccessMode mode)
        {
            return mode switch
            {
                AccessMode.Default => DefaultAcl,
                AccessMode.Private => PrivateAcl,
                AccessMode.PublicRead => PublicReadAcl,
                AccessMode.PublicReadWrite => PublicReadWriteAcl,
                _ => DefaultAcl,
            };
        }

        /// <summary>
        /// 将 OSS 返回的 ACL 字符串转换为统一访问模式。
        /// </summary>
        internal static AccessMode FromAcl(string acl)
        {
            string normalized = NormalizeAcl(acl);
            return normalized switch
            {
                "private" => AccessMode.Private,
                "publicread" => AccessMode.PublicRead,
                "publicreadwrite" => AccessMode.PublicReadWrite,
                "default" => AccessMode.Default,
                _ => AccessMode.Default,
            };
        }

        /// <summary>
        /// 规范化 ACL 字符串，去掉分隔符并统一为小写。
        /// </summary>
        internal static string NormalizeAcl(string acl)
        {
            return string.IsNullOrEmpty(acl)
                ? string.Empty
                : acl.Replace("-", string.Empty).Replace("_", string.Empty).ToLowerInvariant();
        }

        /// <summary>
        /// 解析 OSS 返回的日期字符串。
        /// </summary>
        internal static DateTime ParseOssDate(string value)
        {
            if (DateTime.TryParse(value, out DateTime result))
            {
                return result;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// 去掉 endpoint 中的协议前缀，只保留主机名和端口。
        /// </summary>
        internal static string StripScheme(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
            {
                return endpoint;
            }

            if (Uri.TryCreate(endpoint, UriKind.Absolute, out Uri uri))
            {
                string port = uri.IsDefaultPort ? string.Empty : $":{uri.Port}";
                return $"{uri.Host}{port}";
            }

            return endpoint.TrimEnd('/');
        }
    }
}
