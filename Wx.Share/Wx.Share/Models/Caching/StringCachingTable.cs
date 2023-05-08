using System.ComponentModel.DataAnnotations;
using LiteDB;

namespace Wx.Share.Models.Caching;

public class StringCachingTable
{
  [Key] public ObjectId _id { get; set; } = ObjectId.NewObjectId();

  public string Key { get; set; } = null!;

  public string Value { get; set; } = null!;

  public DateTime DateTime { get; set; } = DateTime.UtcNow;
}