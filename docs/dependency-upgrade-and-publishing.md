# Dependency Upgrade And NuGet Publishing

本文记录 1 号任务中的 NuGet 依赖升级结果、由升级引发的代码改动，以及后续具备 NuGet 包权限后如何开启自动发包。

更新时间：2026-05-11

## 升级结论

- 主库继续保持 `netstandard2.1`。
- 示例项目和测试项目升级到 `net10.0`。
- 包版本预备为 `1.3.0`。
- `dotnet list package --outdated` 已确认三个项目在当前 NuGet 源下没有可用更新。
- `dotnet list package --vulnerable --include-transitive` 已确认三个项目没有已知易受攻击包。
- 当前阶段只准备发布链路，不真实发布 NuGet 包；fork 仓库没有原 `OnceMi.AspNetCore.OSS` 包的发布权限。

## 依赖升级统计

| 项目 | 包 | 升级前 | 升级后 | 是否最新版 | 代码影响 |
| --- | --- | --- | --- | --- | --- |
| `OnceMi.AspNetCore.OSS` | `Aliyun.OSS.SDK.NetCore` | `2.13.0` | `2.14.1` | 是 | 未发现编译 API 断裂。 |
| `OnceMi.AspNetCore.OSS` | `BceSdkDotNetCore` | `1.0.2.911` | `1.0.6.1123` | 是 | 未发现编译 API 断裂；仍会带出 `log4net` 传递依赖。 |
| `OnceMi.AspNetCore.OSS` | `log4net` | 无直接引用 | `3.3.1` | 是 | 新增显式引用，用于覆盖百度 BOS SDK 带出的旧版 `log4net`，消除漏洞审计项。后续拆包/瘦身阶段再隔离或移除。 |
| `OnceMi.AspNetCore.OSS` | `Minio` | `5.0.0` | `7.0.0` | 是 | 有 API 断裂，已修复。详见下方 Minio 代码改动。 |
| `OnceMi.AspNetCore.OSS` | `Qiniu` | `8.3.1` | `8.7.0` | 是 | 未发现编译 API 断裂。 |
| `OnceMi.AspNetCore.OSS` | `Tencent.QCloud.Cos.Sdk` | `5.4.34` | `5.4.51` | 是 | 未发现编译 API 断裂。 |
| `OnceMi.AspNetCore.OSS` | `Microsoft.Extensions.Caching.Memory` | `7.0.0` | `10.0.7` | 是 | 未发现编译 API 断裂。 |
| `OnceMi.AspNetCore.OSS` | `Microsoft.Extensions.Configuration.Binder` | `7.0.4` | `10.0.7` | 是 | 未发现编译 API 断裂。 |
| `OnceMi.AspNetCore.OSS` | `Microsoft.Extensions.DependencyInjection` | `7.0.0` | `10.0.7` | 是 | 未发现编译 API 断裂。 |
| `OnceMi.AspNetCore.OSS` | `Microsoft.Extensions.Options` | `7.0.1` | `10.0.7` | 是 | 未发现编译 API 断裂。 |
| `Sample.AspNetCore.Mvc` | `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` | `7.0.10` | `10.0.7` | 是 | 示例项目目标框架同步升级到 `net10.0`。 |
| `Sample.AspNetCore.Mvc` | `FreeRedis` | `1.1.5` | `1.5.5` | 是 | 未发现编译 API 断裂。 |
| `EasyLink.Storage.Tests` | `Microsoft.NET.Test.Sdk` | `17.7.0` | `18.5.1` | 是 | 测试项目目标框架同步升级到 `net10.0`。 |
| `EasyLink.Storage.Tests` | `MSTest.TestAdapter` | `3.1.1` | `4.2.2` | 是 | 测试写法做了 MSTest 4 兼容调整。 |
| `EasyLink.Storage.Tests` | `MSTest.TestFramework` | `3.1.1` | `4.2.2` | 是 | 测试写法做了 MSTest 4 兼容调整。 |

未升级但保留的直接引用：

| 项目 | 包 | 当前版本 | 说明 |
| --- | --- | --- | --- |
| `OnceMi.AspNetCore.OSS` | `Microsoft.CSharp` | `4.7.0` | 当前 NuGet 源未提示可升级版本，本阶段保持不动。 |

## 升级造成的代码改动

### Minio 5 到 7

Minio 7 的类型和返回值有变化，主要影响 `src/src/EasyLink.Storage/Services/MinioOSSService.cs`。

已做改动：

- 新增 `using Minio.ApiEndpoints;`
- 新增 `using Minio.DataModel.Args;`
- 新增 `using Minio.DataModel.Result;`
- `_client` 字段从 `MinioClient` 改成 `IMinioClient`。
- `Context` 属性从 `MinioClient` 改成 `IMinioClient`。
- 构造函数中的临时 client 变量从 `MinioClient` 改成 `IMinioClient`，适配 `Build()` 返回值。
- `RemoveObjectsAsync` 返回值从旧 observable 处理方式改为 `IList<DeleteError>`，直接收集删除失败对象。

保留事项：

- `ListIncompleteUploads` 和 `ListObjectsAsync` 目前仍通过 Minio 兼容扩展工作，会产生过时 warning。为了控制 1 号任务范围，本阶段只修编译兼容，不重写对象枚举逻辑；后续拆包或瘦身阶段再处理。

### MSTest 3 到 4

测试项目删除原本必失败的 `MinioOSSServiceTests.BucketExistsAsyncTest`，新增 `OSSCoreTests`。

已做改动：

- 将测试项目目标框架升级到 `net10.0`。
- 使用 MSTest 4 包版本。
- 将 `DataTestMethod` 调整为 `TestMethod + DataRow`。
- 不再使用旧的 `Assert.ThrowsExceptionAsync` 写法，改为显式 `try/catch` 断言异常。
- 新增不依赖真实云账号的 `OSSOptions` 和 `BaseOSSService.RemovePresignedUrlCache` 单元测试，共 6 个。

### .NET 10 示例和测试基线

示例项目和测试项目从 `net7.0` 升级到 `net10.0`，解决 .NET 7 EOL warning。

代码层没有业务逻辑改动，只调整：

- `TargetFramework`
- Razor Runtime Compilation 包版本
- FreeRedis 包版本
- 测试 SDK / MSTest 包版本

### log4net 显式引用

`BceSdkDotNetCore` 仍会带出 `log4net`。升级到百度 BOS SDK 当前版本后，漏洞审计仍会命中旧版 `log4net`，因此主库新增显式 `log4net 3.3.1` 引用，让最终解析版本进入安全版本。

这是阶段性处理：

- 1 号任务目标是让当前单包依赖审计干净。
- 3 号任务再处理 `log4net` 是否应从核心包移除、隔离到百度 provider 包，或替换百度 BOS SDK。

## 自动发包教程

当前仓库已经有两个 workflow：

- `.github/workflows/ci.yml`：主发布入口。自动触发只响应 `v*.*.*` tag；手动运行时会执行构建、测试、打包，选择 `publish_to_nuget=true` 时会继续发布到 NuGet。
- `.github/workflows/publish-nuget.yml`：手动备用发布入口，只保留 `workflow_dispatch`，不再响应 tag，避免 tag 发布时重复推包。

要真正自动发包，需要先拥有目标 NuGet 包的发布权限。如果只是 fork 原仓库源码，没有原 `OnceMi.AspNetCore.OSS` 包权限，应选择新的 `PackageId` 发布，或先获得原包 owner 授权。

### 推荐方案

使用 NuGet Trusted Publishing / OIDC。

优点：

- 不需要长期保存 `NUGET_API_KEY`。
- GitHub Actions 通过 OIDC 向 NuGet.org 换取短期 API key。
- workflow 只需要 `id-token: write` 权限。

### 1. 在 NuGet.org 配置信任发布者

在 NuGet.org 登录拥有发布权限的账号或组织，然后进入 Trusted Publishing 配置。

建议配置：

| 字段 | 建议值 |
| --- | --- |
| Repository owner | `staoran` 或实际 GitHub owner |
| Repository name | `OnceMi.AspNetCore.OSS` 或实际 repo 名 |
| Workflow file | `ci.yml` |
| Environment | `nuget-production`，推荐启用 |
| Package | `EasyLink.Storage`，或你实际拥有权限的包名 |

如果配置了 environment，GitHub workflow 里的 publish job 也必须使用同名 environment。若使用 `.github/workflows/publish-nuget.yml` 作为备用手动入口，需要在 NuGet.org 中额外配置匹配 `publish-nuget.yml` 的 Trusted Publishing policy。

### 2. 发布 workflow

主入口是 `.github/workflows/ci.yml`。核心触发和发布逻辑如下：

```yaml
name: CI

on:
  push:
    tags:
      - "v*.*.*"
  workflow_dispatch:
    inputs:
      publish_to_nuget:
        description: "Publish packages to NuGet after build, test, and pack"
        default: "false"
        type: choice
        options:
          - "false"
          - "true"

permissions:
  contents: read

env:
  DOTNET_VERSION: "10.0.x"
  SOLUTION_PATH: src/EasyLink.Storage.sln
  PACKAGE_PROJECT_PATH: src/src/EasyLink.Storage/EasyLink.Storage.csproj
  PACKAGE_OUTPUT: artifacts/packages
  NUGET_SOURCE: https://api.nuget.org/v3/index.json

jobs:
  build-test-pack:
    # restore/build/test/pack/upload artifact

  publish:
    name: Publish to NuGet
    runs-on: ubuntu-latest
    needs: build-test-pack
    if: github.event_name == 'push' || (github.event_name == 'workflow_dispatch' && inputs.publish_to_nuget == 'true')
    environment: nuget-production
    permissions:
      contents: read
      id-token: write
    env:
      NUGET_USER: ${{ vars.NUGET_USER || 'taoran' }}

    steps:
      - name: Download package artifacts
        uses: actions/download-artifact@v4
        with:
          name: nuget-packages
          path: artifacts/packages

      - name: NuGet login
        uses: NuGet/login@v1
        id: login
        with:
          user: ${{ env.NUGET_USER }}

      - name: Push package
        run: dotnet nuget push "<package>.nupkg" --api-key "${{ steps.login.outputs.NUGET_API_KEY }}" --source "${{ env.NUGET_SOURCE }}" --skip-duplicate
```

实际 workflow 会遍历 `artifacts/packages/*.nupkg` 并逐个推送。`NUGET_USER` 优先读取 GitHub repository 或 environment variable；未配置时兜底使用 `taoran`。

### 3. 配置 GitHub environment

建议在 GitHub 仓库中创建 `nuget-production` environment，并加上人工审批。

推荐配置：

- Required reviewers：至少 1 人。
- Deployment branches and tags：只允许 `v*.*.*` tag 或主分支。
- Environment variables：可配置 `NUGET_USER` 覆盖默认值，值为 NuGet.org username，不是邮箱；它不是 API key。未配置时 workflow 使用 `taoran`。

### 4. 发布版本

发布前确认 `src/src/EasyLink.Storage/EasyLink.Storage.csproj` 中的 `<Version>` 已更新，例如：

```xml
<Version>1.3.0</Version>
```

发布流程：

```powershell
git tag v1.3.0
git push origin v1.3.0
```

tag push 后，`ci.yml` 会自动运行。workflow 会先 restore/build/test/pack，再通过 Trusted Publishing 登录 NuGet.org，最后推送 `.nupkg`。

tag 版本必须与 `.csproj` 中的 `<Version>` 一致。例如 `<Version>1.3.0</Version>` 只能用 `v1.3.0` tag 发布；不一致时 workflow 会失败并阻止发布。

手动发布时，在 GitHub Actions 里运行 `CI` workflow，并把 `publish_to_nuget` 选择为 `true`。这会走同一套 restore/build/test/pack/publish 链路。备用入口是手动运行 `Publish NuGet` workflow。

### 5. 首次发布前检查

首次启用真实发布前，至少检查：

- NuGet.org 上的 Trusted Publishing policy 与 workflow 文件名完全一致；主入口应配置为 `ci.yml`。
- workflow 中的 `environment` 与 NuGet.org policy 中的 environment 一致。
- `NUGET_USER` 未配置时会使用 `taoran`；若实际 NuGet profile name 不同，应在 GitHub repository 或 environment variables 中配置正确值。
- `.nupkg` 内包含 `README.md` 和 `LICENSE`。
- `dotnet list package --vulnerable --include-transitive` 无已知漏洞。
- 包名是你有权限发布的包名。

### 6. 当前仓库现状

当前 `.github/workflows/ci.yml` 是主发布入口：

- 自动触发只响应 `v*.*.*` tag，不响应普通分支 push 或 pull request。
- 自动 tag 触发时执行 restore/build/test/pack，并在 tag 版本匹配包版本后发布 NuGet。
- 上传 package artifacts。
- 手动触发时默认只执行 restore/build/test/pack；选择 `publish_to_nuget=true` 时执行 `dotnet nuget push`。

当前 `.github/workflows/publish-nuget.yml` 是手动备用发布入口。两个发布入口只有在以下条件都满足时才能成功：

- GitHub environment `nuget-production` 存在并通过保护规则。
- `NUGET_USER` 变量已配置，或确认默认 `taoran` 是正确的 NuGet.org username。
- NuGet.org Trusted Publishing policy 与仓库、workflow 文件名、environment 匹配。
- 目标包名是当前 NuGet 账号有权限发布的包。

## 参考资料

- NuGet Trusted Publishing 文档：<https://github.com/nuget/docs.microsoft.com-nuget/blob/main/docs/nuget-org/trusted-publishing.md>
- GitHub Actions workflow syntax：<https://docs.github.com/en/actions>
- log4net 3.3.1 NuGet 包页：<https://www.nuget.org/packages/log4net/3.3.1>
