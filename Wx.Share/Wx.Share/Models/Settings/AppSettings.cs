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
    ///     跨域设置
    /// </summary>
    public string[] WithOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     白名单
    /// </summary>
    public string[] WhiteListDomains { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     数据库设置
    /// </summary>
    public DataBase DataBase { get; set; } = new();

    /// <summary>
    ///     微信SDK
    /// </summary>
    public WxSdk WxSdk { get; set; } = new();
}

/// <summary>
///     数据库相关设置
/// </summary>
public class DataBase
{
    public string Directory { get; set; } = "DataBase";
    public string CachingDb { get; set; } = "KLiveCaching.db";
}

/// <summary>
///     微信SDK配置
/// </summary>
public class WxSdk
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
}