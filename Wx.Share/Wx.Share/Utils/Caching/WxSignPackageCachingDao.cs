using LiteDB;
using LiteDB.Async;
using Wx.Share.Models.Caching;
using Wx.Share.Models.Wx;

namespace Wx.Share.Utils.Caching;

public class WxSignPackageCachingDao
{
  private readonly ILiteCollectionAsync<WxSignPackageCachingTable> _wxSignPackageCaching;

  public WxSignPackageCachingDao(CachingDbContext context)
  {
    _wxSignPackageCaching = context.WxSignPackageCaching;
  }

  public async Task<WxSignPackageCachingTable> GetOrSetSignPackageAsync(string urlHash,
    Func<Task<SignPackage?>> factory,
    TimeSpan timeSpan)
  {
    var a = await _wxSignPackageCaching.FindOneAsync(x => x.UrlHash == urlHash);

    if (a != null && DateTime.UtcNow - a.DateTime < timeSpan) return a;

    var value = await factory.Invoke();
    if (!string.IsNullOrWhiteSpace(value?.Url))
    {
      var p = new WxSignPackageCachingTable(value)
      {
        _id = a?._id ?? ObjectId.NewObjectId()
      };

      await _wxSignPackageCaching.UpsertAsync(p);

      return p;
    }

    return new WxSignPackageCachingTable(value);
  }

  public async Task<WxSignPackageCachingTable> SetSignPackageAsync(string urlHash, Func<Task<SignPackage?>> factory)
  {
    return await GetOrSetSignPackageAsync(urlHash, factory, TimeSpan.Zero);
  }
}