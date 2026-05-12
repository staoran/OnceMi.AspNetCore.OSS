using FreeRedis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using EasyLink.Storage;
using Sample.AspNetCore.Mvc.CacheProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.AspNetCore.Mvc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //使用 Redis 替换默认缓存实现
            var client = new RedisClient("127.0.0.1:6379,password=,ConnectTimeout=3000,defaultdatabase=0");
            services.TryAddSingleton<RedisClient>(client);
            services.TryAddSingleton<ICacheProvider, RedisCacheProvider>();

            services.AddMinioStorageProvider();
            services.AddAliyunStorageProvider();
            services.AddTencentCOSStorageProvider();
            services.AddQiniuKodoStorageProvider();
            services.AddHuaweiOBSStorageProvider();
            services.AddBaiduBOSStorageProvider();
            services.AddCtyunOOSStorageProvider();

            //default minio
            //添加默认对象存储配置信息
            services.AddStorageService(option =>
            {
                option.Provider = StorageProvider.Minio;
                option.Endpoint = "oss.oncemi.com:9000";  //不需要包含协议
                option.AccessKey = "root";
                option.SecretKey = "Q*************************f";
                option.IsEnableHttps = true;
                option.IsEnableCache = true;
            });

            //aliyun oss
            //添加名称为 'aliyunoss' 的对象存储配置信息
            services.AddStorageService("aliyunoss", option =>
            {
                option.Provider = StorageProvider.Aliyun;
                option.Endpoint = "oss-cn-hangzhou.aliyuncs.com";
                option.AccessKey = "L*********************U";
                option.SecretKey = "D**************************M";
                option.IsEnableHttps = true;
                option.IsEnableCache = true;
            });

            //qcloud oss
            //从配置文件加载节点为 'OSSProvider' 的配置信息
            services.AddStorageService("QCloud", "OSSProvider");

            //qiniu oss
            //添加名称为 'qiuniu' 的对象存储配置信息
            services.AddStorageService("qiuniu", option =>
            {
                option.Provider = StorageProvider.Qiniu;
                option.Region = "CN_East";  //支持 CN_East/CN_South/CN_North/US_North/Asia_South
                option.AccessKey = "B****************************L";
                option.SecretKey = "Z*************************************g";
                option.IsEnableHttps = true;
                option.IsEnableCache = true;
            });

            //华为 OBS
            //添加名称为 'huaweiobs' 的对象存储配置信息
            //Endpoint 查询：https://developer.huaweicloud.com/endpoint?OBS
            services.AddStorageService("huaweiobs", option =>
            {
                option.Provider = StorageProvider.HuaweiCloud;
                option.Endpoint = "obs.cn-southwest-2.myhuaweicloud.com"; //不需要包含协议
                option.Region = "cn-southwest-2";
                option.AccessKey = "R********************6";
                option.SecretKey = "5*************************************c";
                option.IsEnableHttps = true;
                option.IsEnableCache = true;
            });

            //百度 BOS
            //添加名称为 'baidubos' 的对象存储配置信息
            //Endpoint 查询：https://cloud.baidu.com/doc/BOS/s/8jwvyqdar
            services.AddStorageService("baidubos", option =>
            {
                option.Provider = StorageProvider.BaiduCloud;
                option.Endpoint = "https://su.bcebos.com"; //需要包含协议
                option.AccessKey = "A********************O";
                option.SecretKey = "d********************d";
                option.IsEnableHttps = true;
                option.IsEnableCache = true;
            });

            //天翼云 OOS
            //添加名称为 'ctyunoos' 的对象存储配置信息
            //Endpoint 查询：https://www.ctyun.cn/document/10026693/10027878
            services.AddStorageService("ctyunoos", option =>
            {
                option.Provider = StorageProvider.Ctyun;
                option.Endpoint = "oos-sdqd.ctyunapi.cn"; //不需要包含协议
                option.AccessKey = "6********************6";
                option.SecretKey = "c********************5";
                option.IsEnableHttps = true;
                option.IsEnableCache = true;
            });
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
