using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
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
      var appSetting = b.Configuration.Get<AppSettings>();
      var unixSocket = appSetting?.UnixSocket;
      var port = appSetting?.Port ?? 0;
      if (port > 0)
        options.ListenAnyIP(port);

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

    app.Run();
  }
}