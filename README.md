
# EasyLink.Storage
.NET 对象存储扩展包族。核心包是 EasyLink.Storage，各厂商实现拆为独立 provider 包，支持 Minio、阿里云 OSS、腾讯云 COS、七牛 Kodo、华为 OBS、百度 BOS、天翼 OOS 经典版。主库目标框架为 .NET Standard 2.1，示例和测试项目以 .NET 10 作为当前构建基线。

## 迁移提示
- 旧包 `OnceMi.AspNetCore.OSS` 迁移到新包族 `EasyLink.Storage.*`。
- 核心包安装：`EasyLink.Storage`
- provider 按需安装：`EasyLink.Storage.Minio`、`EasyLink.Storage.AliyunOSS`、`EasyLink.Storage.TencentCOS`、`EasyLink.Storage.QiniuKodo`、`EasyLink.Storage.HuaweiOBS`、`EasyLink.Storage.BaiduBOS`、`EasyLink.Storage.CtyunOOS`

## 各厂家相关SDK文档
- Minio: [点此查看](https://docs.min.io/docs/dotnet-client-api-reference.html "点此查看")
- Aliyun: [点此查看](https://help.aliyun.com/document_detail/32085.html "点此查看")
- QCloud: [点此查看](https://cloud.tencent.com/document/product/436/32819 "点此查看")
- 七牛云: [点此查看](https://developer.qiniu.com/kodo/1237/csharp "点此查看")
- HuaweiOBS：[点此查看](https://support.huaweicloud.com/sdk-dotnet-devg-obs/obs_25_0001.html "点此查看")
- 百度云BOS：[点此查看](https://cloud.baidu.com/doc/BOS/s/ejwvys1ju "点此查看")
- 天翼云OOS经典版：[点此查看](https://www.ctyun.cn/document/10026693 "点此查看")

## 已知问题
1. Minio通过Nginx发反向代理后直接通过域名（不加端口）调用存在问题，应该是Minio本身问题，有兴趣的可以自行测试研究，具体信息我已经发布在Issue中。
2. ~~腾讯云 `PutObjectAsync` 流式上传接口，有非常低的概率会抛“存储桶不存在的异常”，应该是腾讯云自身的原因，具体原因未知。~~ PS：最近没有复现了

## 构建与发布准备
- 当前仓库使用 .NET 10 SDK 执行 `restore`、`build`、`test` 和 `pack`。
- `.github/workflows/ci.yml` 是主入口：推送 `v*.*.*` tag 时构建、测试、打包并发布全部包；手动运行时可选择要打包和发布的核心包或 provider 包。
- `.github/workflows/publish-nuget.yml` 是手动备用发布入口，可按包选择发布，避免 tag 发布时重复推包。
- Trusted Publishing 需要在 NuGet.org 中配置仓库 owner、repository、workflow 文件名和可选 environment；本仓库不使用长期 `NUGET_API_KEY` 作为主发布路径。

## 如何使用
1、安装核心包和所需 provider 包。
Cmd install：
```shell
dotnet add package EasyLink.Storage
dotnet add package EasyLink.Storage.Minio
```
NuGet： [![](https://img.shields.io/nuget/v/EasyLink.Storage.svg)](https://www.nuget.org/packages/EasyLink.Storage)

2、在 `Startup.cs` 中注册 provider 并配置 StorageService：

```csharp
services.AddMinioStorageProvider();
services.AddStorageService(option =>
{
    option.Provider = StorageProvider.Minio;
    option.Endpoint = "oss.example.com:9000";
    option.AccessKey = "Q*************9";
    option.SecretKey = "A**************************Q";
    option.IsEnableHttps = true;
    option.IsEnableCache = true;
});

services.AddStorageService("aliyunoss", option =>
 {
     option.Provider = StorageProvider.Aliyun;
     option.Endpoint = "oss-cn-hangzhou.aliyuncs.com";
     option.AccessKey = "L*******************U";
     option.SecretKey = "5*******************************T";
     option.IsEnableCache = true;
 });

services.AddTencentCOSStorageProvider();
services.AddStorageService("QCloud", "OSSProvider");
```

可注册多个命名 StorageService，不同的服务用名称来区分。需要注意的是，腾讯云 COS 中配置节点 `Endpoint` 表示 AppId。

appsettings.json配置文件实例：
```csharp
{
  "OSSProvider": {
    "Provider": "QCloud", //枚举值支持：Minio/Aliyun/QCloud/Qiniu/HuaweiCloud/BaiduCloud/Ctyun
    "Endpoint": "你的AppId", //腾讯云中表示AppId
    "Region": "ap-chengdu",  //地域
    "AccessKey": "A****************************z",
    "SecretKey": "g6I***************la",
    "IsEnableCache": true  //是否启用缓存，推荐开启
  }
}

```

3、使用Demo

```csharp
/// <summary>
/// 使用默认的配置文件
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IOSSService _OSSService;
    private readonly string _bucketName = "default-dev";

    public HomeController(ILogger<HomeController> logger
        , IOSSService OSSService)
    {
        _logger = logger;
        _OSSService = OSSService;
    }
}
```

```csharp
/// <summary>
/// 获取 IOSSServiceFactory，根据名称创建对应的对象存储服务
/// </summary>
public class QCloudController : Controller
{
    private readonly ILogger<QCloudController> _logger;
    private readonly IOSSService _OSSService;
    private readonly string _bucketName = "default-dev";

    public QCloudController(ILogger<QCloudController> logger
        , IOSSServiceFactory ossServiceFactory)
    {
        _logger = logger;
        _OSSService = ossServiceFactory.Create("QCloud");
    }
}
```
列出bucket中的全部文件

```csharp
public async Task<IActionResult> ListBuckets()
{
    try
    {
        var result = await _OSSService.ListBucketsAsync();
        return Json(result);
    }
    catch (Exception ex)
    {
        return Content(ex.Message);
    }
}
```

### 配置参数

|  名称 |  类型  | 说明  | 案例  |  备注 |
| :------------ |:------------ | :------------ | :------------ | :------------ |
| Provider  | 枚举  | 对象存储提供者  |  Minio | 允许值：Minio/Aliyun/QCloud/Qiniu/HuaweiCloud/BaiduCloud/Ctyun |
| Endpoint  | string  | 节点  | oss-cn-hangzhou.aliyuncs.com  |  在腾讯云OSS中表示AppId  |
| AccessKey  | string  | AccessKey  | F...............s  |    |
| SecretKey  | string  | SecretKey  | v...............d  |    |
| Region  | string  | 地域  | ap-chengdu  |    |
| IsEnableHttps  | bool  | 是否启用HTTPS  |  true  |  建议启用  |
| IsEnableCache  | bool  | 是否启用缓存  |  true  |  启用后将缓存签名 URL，以减少请求次数  |

#### Endpoint查询
| Provider  | Endpoint  | Remark  |
| ------------ | ------------ | ------------ |
| Minio  | -  | 默认或自建Minio Endpoint  |
| Aliyun  | https://help.aliyun.com/document_detail/31837.html  | -  |
| QCloud  | -  | 腾讯云没有Endpoint，此配置项表示AppId  |
| Qiniu  | https://developer.qiniu.com/kodo/4088/s3-access-domainname  | -  |
| HuaweiCloud  | https://support.huaweicloud.com/productdesc-obs/obs_03_0152.html  | -  |
| BaiduCloud  | https://cloud.baidu.com/doc/BOS/s/8jwvyqdar  | -  |
| Ctyun  | https://www.ctyun.cn/document/10026693/10027878  | -  |


### API参考

##### BucketExistsAsync
`Task<bool> BucketExistsAsync(string bucketName);`

判断该存储桶是否存在。

##### CreateBucketAsync
`Task<bool> CreateBucketAsync(string bucketName);`

创建一个存储桶。如果当前存储桶存在，将抛出异常 `BucketExistException`。

##### ListBucketsAsync
`Task<List<Bucket>> ListBucketsAsync();`

列出当前账号下允许访问的所有存储桶。

##### RemoveBucketAsync
`Task<bool> RemoveBucketAsync(string bucketName);`

移除当前存储桶。移除存储桶之前，请先移除存储桶中所有的对象和对象碎片文件。

##### SetBucketAclAsync
`Task<bool> SetBucketAclAsync(string bucketName, AccessMode mode);`

设置存储桶的外部访问权限，支持的权限有：私有、公共读、公共读写。返回设置结果（True or False）。

##### GetBucketAclAsync
`Task<AccessMode> GetBucketAclAsync(string bucketName);`

获取存储桶的外部访问权限。

##### ObjectsExistsAsync
`Task<bool> ObjectsExistsAsync(string bucketName, string objectName);`

获取指定存储桶中指定对象是否存在。

##### ListObjectsAsync
`Task<List<Item>> ListObjectsAsync(string bucketName, string prefix = null);`

列出当前存储桶所有文件。如果存储桶中文件较多，可能需要较长的执行时间，因此推荐填写 `prefix` 参数。`prefix` 会根据文件名称进行前缀匹配，例如输入 `abc`，则列出全部 `abc` 开头的文件或目录。

##### GetObjectAsync
获取文件的数据流。
Method 1:

`Task GetObjectAsync(string bucketName, string objectName, Action<Stream> callback, CancellationToken cancellationToken = default);`

Example
```csharp
try
{
    await _OSSService.GetObjectAsync(_bucketName, "1.jpg", (stream) =>
    {
        using (FileStream fs = new FileStream("1.jpg", FileMode.Create, FileAccess.Write))
        {
            stream.CopyTo(fs);
            fs.Close();
        }
    });
    return Json("OK");
}
catch (Exception ex)
{
    throw ex;
}
```

Method 2:

`Task GetObjectAsync(string bucketName, string objectName, string fileName, CancellationToken cancellationToken = default);`

Example
```csharp
try
{
    await _OSSService.GetObjectAsync(_bucketName, "1.jpg", "C:\\Temp\\1.jpg");
    return Json("OK");
}
catch (Exception ex)
{
    throw ex;
}
```

##### PutObjectAsync

上传文件。支持流式上传和上传本地文件。腾讯云 COS 不支持直接流式上传，为了兼容接口，采用先将流加载到内存中再上传。

Method 1(流式上传):

`Task<bool> PutObjectAsync(string bucketName, string objectName, Stream data, CancellationToken cancellationToken = default(CancellationToken));`

Example
```csharp
try
{
    byte[] bs = System.IO.File.ReadAllBytes(@"C:\Users\sysru\Desktop\PHOTO-1.jpg");
    using (MemoryStream filestream = new MemoryStream(bs))
    {
        await _OSSService.PutObjectAsync(_bucketName, "PHOTO-1.jpg", filestream);
    }

    return Json("OK");
}
catch (Exception ex)
{
    throw;
}
```

Method 2(上传本地文件):
`Task<bool> PutObjectAsync(string bucketName, string objectName, string filePath, CancellationToken cancellationToken = default);`

Example
```csharp
try
{
    await _OSSService.PutObjectAsync(_bucketName, "PHOTO-1.jpg", @"C:\Users\sysru\Desktop\PHOTO-1.jpg");
    return Json("OK");
}
catch (Exception ex)
{
    throw;
}
```

##### GetObjectMetadataAsync
```csharp
Task<ItemMeta> GetObjectMetadataAsync(string bucketName
    , string objectName
    , string versionID = null
    , string matchEtag = null
    , DateTime? modifiedSince = null);
```

获取对象的元数据，或根据 VersionId 获取对象元数据。需要注意的是，阿里云 OSS 和腾讯云 COS 不支持 `matchEtag` 和 `modifiedSince` 参数。

##### CopyObjectAsync
`Task<bool> CopyObjectAsync(string bucketName, string objectName, string destBucketName, string destObjectName = null);`

在存储桶之间复制对象。

##### RemoveObjectAsync
`Task<bool> RemoveObjectAsync(string bucketName, string objectName);`

删除存储桶中指定对象。

`Task<bool> RemoveObjectAsync(string bucketName, List<string> objectNames);`

删除存储桶中多个对象。

##### RemovePresignedUrlCache
`Task RemovePresignedUrlCache(string bucketName, string objectName);`

清除对象生成的签名 URL 缓存。在未开启签名 URL 缓存的情况下，此功能无效。

##### PresignedGetObjectAsync
`Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt);`

生成一个给 HTTP GET 请求使用的预签名 URL。浏览器或移动端客户端可以用这个 URL 进行下载，即使其所在的存储桶是私有的。这个预签名 URL 可以设置一个失效时间，且不能超过 7 天。
如果该对象拥有公共读权限或该对象继承了存储桶的公共读权限，将生成永久下载链接。
如果 `Option` 参数中设置 `IsEnableCache` 为 `true`，将会在有效时间中缓存生成的签名链接，同时也推荐开启此功能，将大大降低请求的频率。

##### PresignedPutObjectAsync
`Task<string> PresignedPutObjectAsync(string bucketName, string objectName, int expiresInt);`

生成一个给 HTTP PUT 请求使用的预签名 URL。浏览器或移动端客户端可以用这个 URL 进行上传，即使其所在的存储桶是私有的。这个预签名 URL 可以设置一个失效时间，且不能超过 7 天。
如果 `Option` 参数中设置 `IsEnableCache` 为 `true`，将会在有效时间中缓存生成的签名链接，同时也推荐开启此功能，将大大降低请求的频率。
注意：七牛云、天翼云对象存储不支持此操作！

##### SetObjectAclAsync
`Task<bool> SetObjectAclAsync(string bucketName, string objectName, AccessMode mode);`

设置对象的访问权限，默认文件的访问权限继承存储桶设置。可以单独通过此 API 为对象设置访问权限。
注意：七牛云、百度云、天翼云对象存储不支持此操作！

##### GetObjectAclAsync
`Task<AccessMode> GetObjectAclAsync(string bucketName, string objectName);`

获取对象的访问权限。如果该权限继承自存储桶，获取的可能是存储桶对当前对象的访问权限。
注意：七牛云、百度云、天翼云对象存储不支持此操作！

##### RemoveObjectAclAsync
`Task<AccessMode> RemoveObjectAclAsync(string bucketName, string objectName);`

清除该对象的访问权限或将其恢复至继承权限。
注意：七牛云、天翼云对象存储不支持此操作！

### 替换内部缓存提供器

如果启用了缓存来缓存签名 URL，可以提高单个文件的签名 URL 请求效率。由于 1.1.3 之前版本使用的是 MemoryCache，有三个问题：
1、不支持分布式，只能单机缓存
2、大量占用应用服务器内存
3、应用重启之后，之前的缓存丢失

从1.1.3开始，提供了一个ICacheProvider接口。用户可以自己实现此接口，替换掉内部的MemoryCache，比如使用Redis。
下面是代码：
```csharp
class RedisCacheProvider : ICacheProvider
{
    private readonly RedisClient _cache;

    public RedisCacheProvider(RedisClient cache)
    {
        this._cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public T Get<T>(string key) where T : class
    {
        string val = _cache.Get(key);
        if (string.IsNullOrEmpty(val))
        {
            return default(T);
        }
        return JsonUtil.DeserializeStringToObject<T>(val);
    }

    public void Remove(string key)
    {
        _cache.Del(key);
    }

    public void Set<T>(string key, T value, TimeSpan ts) where T : class
    {
        string stringVal = JsonUtil.SerializeToString(value);
        _cache.Set(key, stringVal, ts);
    }
}

//构建Redis Client
var client = new RedisClient("127.0.0.1:6379,password=,ConnectTimeout=3000,defaultdatabase=0");
services.TryAddSingleton<RedisClient>(client);
//注入ICacheProvider，一定要在AddStorageService之前注入
services.TryAddSingleton<ICacheProvider, RedisCacheProvider>();
```

## 包与依赖

- `EasyLink.Storage`：核心抽象、DI 注册、配置模型、公共模型、缓存抽象和默认内存缓存，不直接依赖任何厂商 SDK。
- `EasyLink.Storage.Minio`：Minio / S3 兼容对象存储 provider，依赖 `Minio`。
- `EasyLink.Storage.AliyunOSS`：阿里云 OSS provider，依赖 `Aliyun.OSS.SDK.NetCore`。
- `EasyLink.Storage.TencentCOS`：腾讯云 COS provider，依赖 `Tencent.QCloud.Cos.Sdk`。
- `EasyLink.Storage.QiniuKodo`：七牛 Kodo provider，依赖 `Qiniu`。
- `EasyLink.Storage.HuaweiOBS`：华为 OBS provider，使用仓库内嵌 OBS SDK 适配代码。
- `EasyLink.Storage.BaiduBOS`：百度 BOS provider，依赖 `BceSdkDotNetCore`。
- `EasyLink.Storage.CtyunOOS`：天翼云 OOS 经典版 provider，使用仓库内置 HTTP/签名适配代码。

## To do list
~~1. 修改签名 URL 过期策略为滑动过期策略~~
2. 文件分页加载
3. 文件分片上传
