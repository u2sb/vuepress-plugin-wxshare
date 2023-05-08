using LiteDB;

namespace Wx.Share.Models.Redirect;

public class SharePageTable : SharePage
{
  public SharePageTable()
  {
  }

  public SharePageTable(SharePage page)
  {
    Title = page.Title;
    Desc = page.Desc;
    ImgUrl = page.ImgUrl;
    Url = page.Url;
    Hash = page.Hash;
  }

  public ObjectId _id { get; set; } = ObjectId.NewObjectId();
}