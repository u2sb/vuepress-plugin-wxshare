namespace Wx.Share.Models.Settings;

public class AppSettings
{
  public AppSettings()
  {
  }

  public AppSettings(IConfiguration configuration)
  {
    configuration.Bind(this);
  }

  /// <summary>
  ///   UnixSocket
  /// </summary>
  public string UnixSocket { get; set; } = string.Empty;

  /// <summary>
  ///   Port
  /// </summary>
  public int Port { get; set; } = 3567;

  /// <summary>
  ///   跨域设置
  /// </summary>
  public string[] WithOrigins { get; set; } = Array.Empty<string>();

  /// <summary>
  ///   白名单
  /// </summary>
  public string[] WhiteListDomains { get; set; } = Array.Empty<string>();

  /// <summary>
  ///   数据库设置
  /// </summary>
  public DataBase DataBase { get; set; } = new();

  /// <summary>
  ///   微信SDK
  /// </summary>
  public WxSdk WxSdk { get; set; } = new();
}

/// <summary>
///   数据库相关设置
/// </summary>
public class DataBase
{
  /// <summary>
  ///   数据库目录
  /// </summary>
  public string Directory { get; set; } = "DataBase";

  /// <summary>
  ///   缓存数据库
  /// </summary>
  public string CachingDb { get; set; } = "caching.sb";

  /// <summary>
  ///   数据库名称
  /// </summary>
  public string MainDb { get; set; } = "da.sb";
}

/// <summary>
///   微信SDK配置
/// </summary>
public class WxSdk
{
  public string AppId { get; set; } = string.Empty;
  public string AppSecret { get; set; } = string.Empty;
}