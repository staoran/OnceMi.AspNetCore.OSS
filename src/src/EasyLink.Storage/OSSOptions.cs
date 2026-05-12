using System;

namespace EasyLink.Storage
{
    /// <summary>
    /// 对象存储 provider 类型。
    /// </summary>
    public enum StorageProvider
    {
        /// <summary>
        /// 无效 provider。
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Minio 或 S3 兼容对象存储。
        /// </summary>
        Minio = 1,

        /// <summary>
        /// 阿里云 OSS。
        /// </summary>
        Aliyun = 2,

        /// <summary>
        /// 腾讯云 COS。
        /// </summary>
        QCloud = 3,

        /// <summary>
        /// 七牛 Kodo。
        /// </summary>
        Qiniu = 4,

        /// <summary>
        /// 华为 OBS。
        /// </summary>
        HuaweiCloud = 5,

        /// <summary>
        /// 百度 BOS。
        /// </summary>
        BaiduCloud = 6,

        /// <summary>
        /// 天翼云 OOS 经典版。
        /// </summary>
        Ctyun = 7,

        /// <summary>
        /// 阿里云 OSS V2 SDK。
        /// </summary>
        AliyunV2 = 8
    }

    /// <summary>
    /// 对象存储服务配置。
    /// </summary>
    public class OSSOptions
    {
        /// <summary>
        /// 对象存储 provider。
        /// </summary>
        public StorageProvider Provider { get; set; }

        /// <summary>
        /// 服务 endpoint。
        /// </summary>
        /// <remarks>
        /// 腾讯云 COS 中此配置项表示 AppId。
        /// </remarks>
        public string Endpoint { get; set; }

        /// <summary>
        /// AccessKey
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// SecretKey
        /// </summary>
        public string SecretKey { get; set; }

        private string _region = "us-east-1";

        /// <summary>
        /// 存储区域。
        /// </summary>
        public string Region
        {
            get
            {
                return _region;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _region = "us-east-1";
                }
                else
                {
                    _region = value;
                }
            }
        }

        /// <summary>
        /// 是否启用 HTTPS。
        /// </summary>
        public bool IsEnableHttps { get; set; } = true;

        /// <summary>
        /// 是否启用签名 URL 缓存。未注册自定义 <see cref="ICacheProvider"/> 时，默认使用内存缓存。
        /// </summary>
        public bool IsEnableCache { get; set; } = false;
    }
}
