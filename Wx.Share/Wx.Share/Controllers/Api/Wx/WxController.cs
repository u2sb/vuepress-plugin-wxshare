using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Wx.Share.Models.SbWebResult;
using Wx.Share.Models.Settings;
using Wx.Share.Models.Wx;
using Wx.Share.Utils.Wx;

namespace Wx.Share.Controllers.Api.Wx;

[Route("/api/wx/")]
[ApiController]
public class WxController : ControllerBase
{
  private readonly AppSettings _appSettings;
  private readonly WxJsSdk _wxJsSdk;

  public WxController(WxJsSdk wxJsSdk, AppSettings appSettings)
  {
    _wxJsSdk = wxJsSdk;
    _appSettings = appSettings;
  }

  [HttpPost("signature")]
  public async Task<SbWebResult<SignPackage>> Signature([FromBody] SignatureReq req)
  {
    var url = req.Url;
    if (!string.IsNullOrWhiteSpace(url))
    {
      var uri = new Uri(url);
      if (_appSettings.WhiteListDomains.Any(a => Regex.IsMatch(uri.Host, a)))
      {
        var s = await _wxJsSdk.GetSignPackageFromCachingAsync(url);
        if (s != null)
          return new SbWebResult<SignPackage>(s);
      }
    }

    return new SbWebResult<SignPackage>(1);
  }

  [HttpGet("share/{id}")]
  public ActionResult Share(string id)
  {
    var ua = HttpContext.Request.Headers.UserAgent.ToString().ToLower();

    //判断是否是微信环境
    if (ua.IndexOf("micromessenger", StringComparison.Ordinal) > -1)
      return Redirect($"/wx/wx.html?id={id}");
    return Redirect($"/wx/pc.html?id={id}");
  }

  public class SignatureReq
  {
    public string? Url { get; set; }
  }
}