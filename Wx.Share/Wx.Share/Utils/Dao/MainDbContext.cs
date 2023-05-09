using LiteDB.Async;
using Wx.Share.Models.Redirect;
using Wx.Share.Models.Settings;

namespace Wx.Share.Utils.Dao;

public class MainDbContext
{
  private readonly AppSettings _appSettings;

  public MainDbContext(AppSettings appSettings)
  {
    _appSettings = appSettings;

    Database = new LiteDatabaseAsync(Path.Combine(appSettings.DataBase.Directory, appSettings.DataBase.MainDb));
    SharePage = Database.GetCollection<SharePageTable>("Redirect");
    SharePage.EnsureIndexAsync(x => x.Hash);
  }

  public ILiteCollectionAsync<SharePageTable> SharePage { get; }


  public LiteDatabaseAsync Database { get; }
}