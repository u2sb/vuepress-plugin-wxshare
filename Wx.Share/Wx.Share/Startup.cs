using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using EasyCaching.LiteDB;
using FluentScheduler;
using LiteDB;
using Microsoft.AspNetCore.HttpOverrides;
using RestSharp;
using Wx.Share.Models.Settings;
using Wx.Share.Utils.Caching;
using Wx.Share.Utils.Wx;

namespace Wx.Share;

public class Startup
{
    private readonly AppSettings _appSettings;
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
        _appSettings = configuration.Get<AppSettings>()!;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        if (!Directory.Exists(_appSettings!.DataBase.Directory))
            Directory.CreateDirectory(_appSettings.DataBase.Directory);

        services.AddControllersWithViews().AddJsonOptions(opt =>
        {
            opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opt.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        });

        services.AddRazorPages().AddJsonOptions(opt =>
        {
            opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opt.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        });

        // 转接头，代理
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        //配置跨域
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(b => b
                // .SetIsOriginAllowedToAllowWildcardSubdomains()
                .WithOrigins(_appSettings.WithOrigins.ToArray())
                .WithMethods("GET", "POST", "OPTIONS")
                .AllowAnyHeader()
                .AllowCredentials());
        });

        // 缓存
        services.AddEasyCaching(options =>
        {
            options.UseLiteDB(config =>
            {
                config.DBConfig = new LiteDBDBOptions
                {
                    ConnectionType = ConnectionType.Direct,
                    FilePath = _appSettings.DataBase.Directory,
                    FileName = _appSettings.DataBase.CachingDb
                };
            }, ConstantTable.DefaultCachingDb);
        });

        services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
        services.AddSingleton(_appSettings);
        services.AddSingleton(new RestClient(new HttpClient()));
        services.AddScoped<DefaultCaching>();
        services.AddScoped<WxJsSdk>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, WxJsSdk wxJsSdk)
    {
        app.UseHttpsRedirection();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseForwardedHeaders();
        app.UseRouting();
        app.UseCors();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute("default",
                "{controller=Home}/{action=Index}/{id?}");
        });

        JobManager.Initialize();
        JobManager.AddJob(
            async () =>
            {
                var a = 0;
                while (!await wxJsSdk.RefreshAccessTokenAsync())
                {
                    a++;
                    if (a > 3) break;
                }

                var b = 0;
                while (!await wxJsSdk.RefreshJsApiTicketAsync())
                {
                    b++;
                    if (b > 3) break;
                }
            },
            s => s.ToRunEvery(1).Hours()
        );
    }
}