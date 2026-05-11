using System;

namespace EasyLink.Storage
{
    public enum StorageProvider
    {
        /// <summary>
        /// ��Ч
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Minio�Խ����󴢴�
        /// </summary>
        Minio = 1,

        /// <summary>
        /// ������OSS
        /// </summary>
        Aliyun = 2,

        /// <summary>
        /// ��Ѷ��OSS
        /// </summary>
        QCloud = 3,

        /// <summary>
        /// ��ţ�� OSS
        /// </summary>
        Qiniu = 4,

        /// <summary>
        /// ��Ϊ�� OBS
        /// </summary>
        HuaweiCloud = 5,

        /// <summary>
        /// �ٶ��� BOS
        /// </summary>
        BaiduCloud = 6,
        /// <summary>
        /// ������ OOS
        /// </summary>
        Ctyun = 7
    }

    public class OSSOptions
    {
        /// <summary>
        /// ö�٣�OOS�ṩ��
        /// </summary>
        public StorageProvider Provider { get; set; }

        /// <summary>
        /// �ڵ�
        /// </summary>
        /// <remarks>
        /// ��Ѷ���б�ʾAppId
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
        /// ����
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
        /// �Ƿ�����HTTPS
        /// </summary>
        public bool IsEnableHttps { get; set; } = true;

        /// <summary>
        /// �Ƿ����û��棬Ĭ�ϻ�����MemeryCache�У���ʹ������ʵ�ֵĻ������Ĭ�ϻ��棩
        /// ��ʹ��֮ǰ��������ǰӦ�õĻ��������ܷ�ס��ǰ����
        /// </summary>
        public bool IsEnableCache { get; set; } = false;
    }
}
