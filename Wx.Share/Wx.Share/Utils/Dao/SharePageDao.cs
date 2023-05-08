using LiteDB.Async;
using Wx.Share.Models.Redirect;
using Wx.Share.Utils.Encrypt;

namespace Wx.Share.Utils.Dao;

public class SharePageDao
{
  private readonly ILiteCollectionAsync<SharePageTable> _sharePage;

  public SharePageDao(MainDbContext context)
  {
    _sharePage = context.SharePage;
  }

  public async Task<SharePage?> GetUrlAsync(string urlHash)
  {
    var a = await _sharePage.FindOneAsync(x => x.Hash == urlHash);

    return a;
  }

  public async Task<string?> SetPageAsync(SharePage sharePage)
  {
    sharePage.Hash = Md5.Generate(sharePage.Url + sharePage.Title + sharePage.Desc + sharePage.ImgUrl);

    if (await _sharePage.ExistsAsync(x => x.Hash == sharePage.Hash)) return sharePage.Hash;

    await _sharePage.InsertAsync(new SharePageTable(sharePage));

    return sharePage.Hash;
  }
}