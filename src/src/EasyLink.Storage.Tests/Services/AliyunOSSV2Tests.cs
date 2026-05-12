using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyLink.Storage.Tests
{
    [TestClass]
    public class AliyunOSSV2ProviderTests
    {
        [TestMethod]
        public void AddAliyunOSSV2StorageProvider_RegistersFactoryInRegistry()
        {
            using ServiceProvider provider = new ServiceCollection()
                .AddOptions()
                .AddAliyunOSSV2StorageProvider()
                .BuildServiceProvider();

            StorageProviderRegistry registry = provider.GetRequiredService<IOptions<StorageProviderRegistry>>().Value;

            Assert.IsTrue(registry.TryGetFactory(StorageProvider.AliyunV2, out _));
        }

        [TestMethod]
        public void Create_WhenAliyunV2IsRegistered_ReturnsAliyunOSSV2Service()
        {
            using ServiceProvider provider = CreateServiceProvider(registerAliyunV2: true);
            IOSSServiceFactory factory = provider.GetRequiredService<IOSSServiceFactory>();

            IOSSService service = factory.Create("aliyun-v2");

            try
            {
                Assert.IsInstanceOfType(service, typeof(AliyunOSSV2Service));
                Assert.IsInstanceOfType(service, typeof(IAliyunOSSV2Service));

                IAliyunOSSV2Service aliyunV2Service = (IAliyunOSSV2Service)service;
                Assert.AreEqual(StorageProvider.AliyunV2, aliyunV2Service.Options.Provider);
                Assert.IsNotNull(aliyunV2Service.Context);
            }
            finally
            {
                (service as IDisposable)?.Dispose();
            }
        }

        [TestMethod]
        public void Create_WhenAliyunV2IsNotRegistered_ReportsPackageHint()
        {
            using ServiceProvider provider = CreateServiceProvider(registerAliyunV2: false);
            IOSSServiceFactory factory = provider.GetRequiredService<IOSSServiceFactory>();

            InvalidOperationException exception = AssertThrows<InvalidOperationException>(() => factory.Create("aliyun-v2"));

            StringAssert.Contains(exception.Message, "EasyLink.Storage.AliyunOSSV2");
        }

        [TestMethod]
        [DataRow("Endpoint")]
        [DataRow("Region")]
        public void Create_WhenAliyunV2RequiredSettingMissing_ThrowsArgumentNullException(string missingOption)
        {
            using ServiceProvider provider = CreateServiceProvider(registerAliyunV2: false, options =>
            {
                if (missingOption == "Endpoint")
                {
                    options.Endpoint = null;
                }

                if (missingOption == "Region")
                {
                    SetRegionBackingField(options, null);
                }
            });

            IOSSServiceFactory factory = provider.GetRequiredService<IOSSServiceFactory>();

            ArgumentNullException exception = AssertThrows<ArgumentNullException>(() => factory.Create("aliyun-v2"));

            Assert.AreEqual(missingOption, exception.ParamName);
        }

        private static ServiceProvider CreateServiceProvider(bool registerAliyunV2, Action<OSSOptions> configure = null)
        {
            IServiceCollection services = new ServiceCollection();

            services.AddStorageService("aliyun-v2", options =>
            {
                options.Provider = StorageProvider.AliyunV2;
                options.Endpoint = "oss-cn-hangzhou.aliyuncs.com";
                options.Region = "cn-hangzhou";
                options.AccessKey = "test-ak";
                options.SecretKey = "test-sk";
                options.IsEnableCache = false;
                configure?.Invoke(options);
            });

            if (registerAliyunV2)
            {
                services.AddAliyunOSSV2StorageProvider();
            }

            return services.BuildServiceProvider();
        }

        private static void SetRegionBackingField(OSSOptions options, string value)
        {
            FieldInfo field = typeof(OSSOptions).GetField("_region", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(options, value);
        }

        private static TException AssertThrows<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException exception)
            {
                return exception;
            }

            Assert.Fail($"Expected {typeof(TException).Name}.");
            return null;
        }
    }

    [TestClass]
    public class AliyunOSSV2ServiceHelperTests
    {
        [TestMethod]
        [DataRow(AccessMode.Private, "private")]
        [DataRow(AccessMode.PublicRead, "public-read")]
        [DataRow(AccessMode.PublicReadWrite, "public-read-write")]
        [DataRow(AccessMode.Default, "private")]
        public void ToBucketAcl_MapsAccessModeToOSSValue(AccessMode mode, string expectedAcl)
        {
            Assert.AreEqual(expectedAcl, AliyunOSSV2Service.ToBucketAcl(mode));
        }

        [TestMethod]
        [DataRow(AccessMode.Private, "private")]
        [DataRow(AccessMode.PublicRead, "public-read")]
        [DataRow(AccessMode.PublicReadWrite, "public-read-write")]
        [DataRow(AccessMode.Default, "default")]
        public void ToObjectAcl_MapsAccessModeToOSSValue(AccessMode mode, string expectedAcl)
        {
            Assert.AreEqual(expectedAcl, AliyunOSSV2Service.ToObjectAcl(mode));
        }

        [TestMethod]
        [DataRow("private", AccessMode.Private)]
        [DataRow("public-read", AccessMode.PublicRead)]
        [DataRow("public_read_write", AccessMode.PublicReadWrite)]
        [DataRow("DEFAULT", AccessMode.Default)]
        public void FromAcl_NormalizesAlibabaCloudAclStrings(string acl, AccessMode expectedMode)
        {
            Assert.AreEqual(expectedMode, AliyunOSSV2Service.FromAcl(acl));
        }

        [TestMethod]
        public void StripScheme_RemovesProtocolPrefixAndKeepsPort()
        {
            Assert.AreEqual("oss-cn-hangzhou.aliyuncs.com:9443", AliyunOSSV2Service.StripScheme("https://oss-cn-hangzhou.aliyuncs.com:9443/path"));
        }

        [TestMethod]
        [DataRow(-1L, 0UL)]
        [DataRow(0L, 0UL)]
        [DataRow(1024L, 1024UL)]
        public void ToUnsignedSize_ClampsNegativeValuesToZero(long size, ulong expected)
        {
            Assert.AreEqual(expected, AliyunOSSV2Service.ToUnsignedSize(size));
        }

        [TestMethod]
        public void ParseOssDate_ReturnsMinValueForInvalidInput()
        {
            Assert.AreEqual(DateTime.MinValue, AliyunOSSV2Service.ParseOssDate("not-a-date"));
        }
    }
}
