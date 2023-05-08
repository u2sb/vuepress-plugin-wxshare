using LiteDB;
using LiteDB.Async;
using Wx.Share.Models.Caching;

namespace Wx.Share.Utils.Caching;

public class StringCachingDao
{
  private readonly ILiteCollectionAsync<StringCachingTable> _stringCaching;

  public StringCachingDao(CachingDbContext context)
  {
    _stringCaching = context.StringCaching;
  }

  /// <summary>
  ///   获取或设置缓存
  /// </summary>
  /// <param name="key"></param>
  /// <param name="factory"></param>
  /// <param name="duration">过期时间</param>
  /// <returns></returns>
  public async Task<string> GetOrSetValueAsync(string key, Func<Task<string>> factory, TimeSpan duration)
  {
    var a = await _stringCaching.FindOneAsync(x => x.Key == key);

    if (a is { Value: not null } && DateTime.UtcNow - a.DateTime < duration) return a.Value;

    var value = await factory.Invoke();
    if (!string.IsNullOrWhiteSpace(value))
      await _stringCaching.UpsertAsync(new StringCachingTable
      {
        _id = a?._id ?? ObjectId.NewObjectId(),
        Key = key,
        Value = value
      });

    return value;
  }

  /// <summary>
  ///   强制刷新缓存
  /// </summary>
  /// <param name="key"></param>
  /// <param name="factory"></param>
  /// <returns></returns>
  public async Task<string> SetValueAsync(string key, Func<Task<string>> factory)
  {
    return await GetOrSetValueAsync(key, factory, TimeSpan.Zero);
  }
}