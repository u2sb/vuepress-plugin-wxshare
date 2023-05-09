using LiteDB.Async;
using Wx.Share.Models.Caching;
using Wx.Share.Models.Settings;

namespace Wx.Share.Utils.Caching;

public class CachingDbContext
{
  private readonly AppSettings _appSettings;

  public CachingDbContext(AppSettings appSettings)
  {
    _appSettings = appSettings;

    Database = new LiteDatabaseAsync(Path.Combine(appSettings.DataBase.Directory, appSettings.DataBase.CachingDb));

    StringCaching = Database.GetCollection<StringCachingTable>("StringCaching");
    StringCaching.EnsureIndexAsync(x => x.Key);

    WxSignPackageCaching = Database.GetCollection<WxSignPackageCachingTable>("WxSignPackageCaching");
    WxSignPackageCaching.EnsureIndexAsync(x => x.UrlHash);
  }

  public ILiteCollectionAsync<StringCachingTable> StringCaching { get; }
  public ILiteCollectionAsync<WxSignPackageCachingTable> WxSignPackageCaching { get; }

  public LiteDatabaseAsync Database { get; }
}