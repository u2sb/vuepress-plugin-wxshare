using Microsoft.AspNetCore.Mvc;
using Wx.Share.Models.Wx;

namespace Wx.Share.Controllers;

public class WxShareController : Controller
{
    public IActionResult Index(SharePage sharePage)
    {
        if (Request.Headers.TryGetValue("User-Agent", out var userAgent) &&
            userAgent.ToString().ToLower().IndexOf("micromessenger", StringComparison.Ordinal) > -1)
            return View("Wx", sharePage);

        return View(sharePage);
    }
}