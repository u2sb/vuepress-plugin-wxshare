using Microsoft.AspNetCore.Mvc;
using Wx.Share.Models.Settings;
using Wx.Share.Models.WebResult;
using Wx.Share.Models.Wx;
using Wx.Share.Utils.Wx;

namespace Wx.Share.Controllers.Api.Wx;

[Route("api/wx/share")]
[ApiController]
public class WxShareController : ControllerBase
{
    private readonly AppSettings _appSettings;
    private readonly WxJsSdk _wxSdk;

    public WxShareController(AppSettings appSettings, WxJsSdk wxSdk)
    {
        _wxSdk = wxSdk;
        _appSettings = appSettings;
    }

    [HttpGet("signature")]
    public async Task<WebResult<SignPackage?>> Signature(string? url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            var uri = new Uri(url);
            if (_appSettings.WhiteListDomains.Any(a => a == uri.Host))
            {
                var a = await _wxSdk.GetSignPackageAsync(url);
                if (a != null) return new WebResult<SignPackage?>(a);
            }
        }

        return new WebResult<SignPackage?>(1);
    }

    [HttpGet("redirect")]
    public ActionResult RedirectUrl(string? url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            var uri = new Uri(url);
            if (_appSettings.WhiteListDomains.Any(a => a == uri.Host)) return Redirect(url);
        }

        return NotFound();
    }
}