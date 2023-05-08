using System.ComponentModel.DataAnnotations;
using LiteDB;
using Wx.Share.Models.Wx;
using Wx.Share.Utils.Encrypt;

namespace Wx.Share.Models.Caching;

public class WxSignPackageCachingTable : SignPackage
{
  public WxSignPackageCachingTable()
  {
  }

  public WxSignPackageCachingTable(SignPackage? signPackage)
  {
    AppId = signPackage.AppId;
    NonceStr = signPackage.NonceStr;
    Timestamp = signPackage.Timestamp;
    Url = signPackage.Url;
    Signature = signPackage.Signature;
    UrlHash = Md5.Generate(Url);
  }

  [Key] public ObjectId _id { get; set; } = ObjectId.NewObjectId();

  public string UrlHash { get; set; } = string.Empty;

  public DateTime DateTime { get; set; } = DateTime.UtcNow;
}