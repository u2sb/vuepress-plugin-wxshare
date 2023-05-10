using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using FluentScheduler;
using Microsoft.AspNetCore.HttpOverrides;
using RestSharp;
using Wx.Share.Models.Settings;
using Wx.Share.Utils.Caching;
using Wx.Share.Utils.Dao;
using Wx.Share.Utils.Wx;

namespace Wx.Share;

public class Program
{
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    // �����ļ�
    var appSettings = builder.Configuration.Get<AppSettings>();

    // ��������
    builder.WebHost.ConfigureKestrel((b, options) =>
    {
      var unixSocket = appSettings?.UnixSocket;
      var port = appSettings?.Port ?? 0;
      if (port > 0)
        options.ListenLocalhost(port);

      if (!string.IsNullOrWhiteSpace(unixSocket))
        options.ListenUnixSocket(unixSocket);
    });


    var services = builder.Services;

    if (!Directory.Exists(appSettings!.DataBase.Directory))
      Directory.CreateDirectory(appSettings.DataBase.Directory);

    // ת��ͷ������
    services.Configure<ForwardedHeadersOptions>(options =>
    {
      options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });

    //���ÿ���
    services.AddCors(options =>
    {
      options.AddDefaultPolicy(b => b
        .SetIsOriginAllowedToAllowWildcardSubdomains()
        .WithOrigins(appSettings.WithOrigins.ToArray())
        .WithMethods("GET", "POST", "OPTIONS")
        .AllowAnyHeader()
        .AllowCredentials());
    });

    // �ս��
    services.AddControllers().AddJsonOptions(opt =>
    {
      opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
      opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
      opt.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    });

    services.AddSingleton(appSettings);
    services.AddSingleton(new RestClient(new HttpClient()));
    services.AddSingleton<CachingDbContext>();
    services.AddSingleton<MainDbContext>();
    services.AddScoped<StringCachingDao>();
    services.AddScoped<WxSignPackageCachingDao>();
    services.AddScoped<SharePageDao>();
    services.AddScoped<WxJsSdk>();


    var app = builder.Build();

    // ����ģʽ�������
    if (app.Environment.IsDevelopment()) app.UseHttpLogging();

    // ��̬�ļ�
    app.UseDefaultFiles();
    app.UseStaticFiles();


    app.UseForwardedHeaders();
    app.UseRouting();
    app.UseCors();

    app.MapControllers();

    var serviceScope = app.Services.CreateScope();
    var s = serviceScope.ServiceProvider;

    if (!string.IsNullOrWhiteSpace(appSettings.WxSdk.AppId) &&
        !string.IsNullOrWhiteSpace(appSettings.WxSdk.AppSecret))
    {
      var wxJsSdk = s.GetRequiredService<WxJsSdk>();

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
        sc => sc.ToRunEvery(6900).Seconds()
      );
    }

    var life = s.GetRequiredService<IHostApplicationLifetime>();
    var cachingDb = s.GetRequiredService<CachingDbContext>();
    var mainDb = s.GetRequiredService<MainDbContext>();

    life.ApplicationStarted.Register(() =>
    {
      // ��ȡ��д��pid�ļ�
      var pid = Process.GetCurrentProcess().Id;
      TextWriter pidWriter = new StreamWriter(appSettings.PidFile);
      pidWriter.Write(pid);
      pidWriter.Flush();
      pidWriter.Close();
    });

    life.ApplicationStopped.Register(() =>
    {
      // ɾ���ļ�
      if (File.Exists(appSettings.PidFile)) File.Delete(appSettings.PidFile);
      if (!string.IsNullOrWhiteSpace(appSettings.UnixSocket) & File.Exists(appSettings.UnixSocket))
        File.Delete(appSettings.UnixSocket);

      cachingDb.Database.Dispose();
      mainDb.Database.Dispose();
    });

    app.Run();
  }
}