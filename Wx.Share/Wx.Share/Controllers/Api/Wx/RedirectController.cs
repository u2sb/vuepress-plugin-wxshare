using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Wx.Share.Models.Settings;
using Wx.Share.Utils.Dao;

namespace Wx.Share.Controllers.Api.Wx;

[Route("/api/wx/")]
[ApiController]
public class RedirectController : Controller
{
  private readonly AppSettings _appSettings;
  private readonly SharePageDao _sharePageDao;

  public RedirectController(AppSettings appSettings, SharePageDao dao)
  {
    _appSettings = appSettings;
    _sharePageDao = dao;
  }

  [HttpGet("redirect/{id}")]
  public async Task<ActionResult> RedirectUrlById(string id)
  {
    var page = await _sharePageDao.GetUrlAsync(id);
    var url = page?.Url;

    if (!string.IsNullOrWhiteSpace(url))
    {
      var uri = new Uri(url);
      if (_appSettings.WhiteListDomains.Any(a => Regex.IsMatch(uri.Host, a))) return Redirect(url);
    }

    return NotFound();
  }
}