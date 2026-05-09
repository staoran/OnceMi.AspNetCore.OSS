using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.OSS.Tests
{
    [TestClass]
    public class OSSOptionsTests
    {
        [TestMethod]
        public void Region_UsesDefaultValue_WhenNotSet()
        {
            OSSOptions options = new OSSOptions();

            Assert.AreEqual("us-east-1", options.Region);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void Region_ResetToDefault_WhenAssignedNullOrEmpty(string region)
        {
            OSSOptions options = new OSSOptions
            {
                Region = "cn-east-1"
            };

            options.Region = region;

            Assert.AreEqual("us-east-1", options.Region);
        }
    }

    [TestClass]
    public class BaseOSSServiceTests
    {
        [TestMethod]
        public async Task RemovePresignedUrlCache_WhenCacheEnabled_RemovesGetAndPutCacheEntries()
        {
            RecordingCacheProvider cache = new RecordingCacheProvider();
            TestOSSService service = CreateService(cache, isEnableCache: true);

            await service.RemovePresignedUrlCache("bucket", "/folder/file.txt");

            Assert.AreEqual(2, cache.RemovedKeys.Count);
            CollectionAssert.AllItemsAreUnique(cache.RemovedKeys);
            Assert.IsTrue(cache.RemovedKeys.TrueForAll(key => !string.IsNullOrWhiteSpace(key)));
        }

        [TestMethod]
        public async Task RemovePresignedUrlCache_WhenCacheDisabled_DoesNotRemoveCacheEntries()
        {
            RecordingCacheProvider cache = new RecordingCacheProvider();
            TestOSSService service = CreateService(cache, isEnableCache: false);

            await service.RemovePresignedUrlCache("bucket", "/folder/file.txt");

            Assert.AreEqual(0, cache.RemovedKeys.Count);
        }

        [TestMethod]
        public async Task RemovePresignedUrlCache_WhenObjectNameInvalid_ThrowsArgumentNullException()
        {
            TestOSSService service = CreateService(new RecordingCacheProvider(), isEnableCache: true);

            try
            {
                await service.RemovePresignedUrlCache("bucket", "/");
                Assert.Fail("Expected ArgumentNullException.");
            }
            catch (ArgumentNullException)
            {
            }
        }

        private static TestOSSService CreateService(ICacheProvider cache, bool isEnableCache)
        {
            return new TestOSSService(
                cache,
                new OSSOptions
                {
                    Provider = OSSProvider.Minio,
                    IsEnableCache = isEnableCache
                });
        }

        private sealed class TestOSSService : BaseOSSService
        {
            public TestOSSService(ICacheProvider cache, OSSOptions options)
                : base(cache, options)
            {
            }
        }

        private sealed class RecordingCacheProvider : ICacheProvider
        {
            public List<string> RemovedKeys { get; } = new List<string>();

            public T Get<T>(string key) where T : class
            {
                return null;
            }

            public void Remove(string key)
            {
                RemovedKeys.Add(key);
            }

            public void Set<T>(string key, T value, TimeSpan ts) where T : class
            {
            }
        }
    }
}
