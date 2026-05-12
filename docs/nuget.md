
# EasyLink.Storage
ASP.NET Core 对象存储扩展包族。核心包是 `EasyLink.Storage`，各 provider 包按需安装。主库目标框架为 .NET Standard 2.1，示例和测试项目以 .NET 10 作为当前构建基线。

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
2. 腾讯云`PutObjectAsync`流式上传接口，有非常低的概率会抛“储存桶不存在的异常”，应该是腾讯云自身的原因，具体原因未知。

## 构建与发布准备
- 当前仓库使用.NET 10 SDK执行`restore`、`build`、`test`和`pack`。
- GitHub Actions 的主入口是`.github/workflows/ci.yml`：自动触发只响应`v*.*.*` tag；手动运行时默认构建、测试、打包勾选的核心包和 provider 包，勾选`publish_to_nuget`时发布 NuGet 包。
- `.github/workflows/publish-nuget.yml`只保留为手动备用发布入口，可勾选需要发布的包，不再响应 tag，避免重复推包。
- Trusted Publishing需要在NuGet.org中配置仓库 owner、repository、workflow 文件名和可选 environment；本仓库不使用长期`NUGET_API_KEY`作为主发布路径。
- `NUGET_USER`可通过 GitHub repository 或 environment variable 覆盖；未配置时 workflow 使用`taoran`作为 NuGet.org username。

## 如何使用
1、安装核心包和所需 provider 包。
Cmd install：
```shell
dotnet add package EasyLink.Storage
dotnet add package EasyLink.Storage.Minio
```
Nuget： [![](https://img.shields.io/nuget/v/EasyLink.Storage.svg)](https://www.nuget.org/packages/EasyLink.Storage)

2、在`Startup.cs`中配置
You need to configure OSSService in your Startup.cs：

```csharp
//default minio
//添加默认对象储存配置信息
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

//aliyun oss
//添加名称为‘aliyunoss’的OSS对象储存配置信息
services.AddStorageService("aliyunoss", option =>
 {
     option.Provider = StorageProvider.Aliyun;
     option.Endpoint = "oss-cn-hangzhou.aliyuncs.com";
     option.AccessKey = "L*******************U";
     option.SecretKey = "5*******************************T";
     option.IsEnableCache = true;
 });

//qcloud oss
//也可以从配置文件中加载节点为‘OSSProvider’的配置信息
services.AddTencentCOSStorageProvider();
services.AddStorageService("QCloud", "OSSProvider");
```

可注入多个OSSService，不同的Service用名称来区分。需要注意的是，腾讯云COS中配置节点Endpoint表示AppId。

appsettings.json配置文件实例：
```csharp
{
  "OSSProvider": {
    "Provider": "QCloud", //枚举值支持：Minio/Aliyun/QCloud
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
/// 获取IOSSServiceFactory，根据名称创建对应的OSS服务
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
| Provider  | 枚举  | OSS提供者  |  Minio | 允许值：Minio/Aliyun/QCloud/Qiniu/HuaweiCloud/BaiduCloud/Ctyun |
| Endpoint  | string  | 节点  | oss-cn-hangzhou.aliyuncs.com  |  在腾讯云OSS中表示AppId  |
| AccessKey  | string  | AccessKey  | F...............s  |    |
| SecretKey  | string  | SecretKey  | v...............d  |    |
| Region  | string  | 地域  | ap-chengdu  |    |
| IsEnableHttps  | bool  | 是否启用HTTPS  |  true  |  建议启用  |
| IsEnableCache  | bool  | 是否启用缓存  |  true  |  启用后将缓存签名URL，以减少请求次数  |

## Dependencies

1. Aliyun.OSS.SDK.NetCore
2. BceSdkDotNetCore
3. Microsoft.Extensions.Caching.Memory
4. Microsoft.Extensions.Configuration.Binder
5. Microsoft.Extensions.DependencyInjection
6. Microsoft.Extensions.Options
7. Tencent.QCloud.Cos.Sdk
8. Minio
9. Qiniu

## To do list
1. 修改签名URL过期策略为滑动过期策略
2. 文件分页加载
