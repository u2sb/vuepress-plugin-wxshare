using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Wx.Share.Models.Redirect;
using Wx.Share.Models.SbWebResult;
using Wx.Share.Models.Settings;
using Wx.Share.Utils.Dao;

namespace Wx.Share.Controllers.Api.Wx;

[Route("/api/wx/page/")]
[ApiController]
public class PageController : ControllerBase
{
  private readonly AppSettings _appSettings;
  private readonly SharePageDao _sharePageDao;

  public PageController(AppSettings appSettings, SharePageDao dao)
  {
    _appSettings = appSettings;
    _sharePageDao = dao;
  }

  [HttpPost("add")]
  public async Task<SbWebResult<string?>> AddPage([FromBody] SharePage page)
  {
    var url = page.Url;
    if (!string.IsNullOrWhiteSpace(url))
    {
      var uri = new Uri(url);
      if (_appSettings.WhiteListDomains.Any(a => Regex.IsMatch(uri.Host, a)))
      {
        var hash = await _sharePageDao.SetPageAsync(page);
        return new SbWebResult<string?>(hash);
      }
    }

    return new SbWebResult<string?>(1);
  }

  [HttpGet("get/{id}")]
  public async Task<SbWebResult<SharePage>> GetPage(string id)
  {
    var page = await _sharePageDao.GetUrlAsync(id);

    if (page != null) return new SbWebResult<SharePage>(page);

    return new SbWebResult<SharePage>(1);
  }
}