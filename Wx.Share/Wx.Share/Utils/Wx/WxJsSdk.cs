using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;
using RestSharp;
using Wx.Share.Models.Settings;
using Wx.Share.Models.Wx;
using Wx.Share.Utils.Caching;
using Wx.Share.Utils.Encrypt;

namespace Wx.Share.Utils.Wx;

public class WxJsSdk
{
  private const string AccessTokenUrl = "https://api.weixin.qq.com/cgi-bin/token";
  private const string JsApsTicketUrl = "https://api.weixin.qq.com/cgi-bin/ticket/getticket";

  private readonly IRandomizerString _rdString;
  private readonly RestClient _restClient;

  private readonly StringCachingDao _stringCaching;
  private readonly WxSdk _wx;
  private readonly WxSignPackageCachingDao _wxSignPackageCaching;


  public WxJsSdk(AppSettings appSettings, RestClient restClient, StringCachingDao stringCaching,
    WxSignPackageCachingDao wxSignPackageCachingDao)
  {
    _wx = appSettings.WxSdk;
    _restClient = restClient;
    _stringCaching = stringCaching;
    _wxSignPackageCaching = wxSignPackageCachingDao;
    _rdString = _rdString = RandomizerFactory.GetRandomizer(new FieldOptionsText
      { UseNumber = false, UseSpecial = false, Max = 10, Min = 6 });
  }

  /// <summary>
  ///   刷新 AccessToken
  /// </summary>
  /// <returns></returns>
  public async Task<bool> RefreshAccessTokenAsync()
  {
    var a = await _stringCaching.SetValueAsync("WxAccessToken", async () => await GetAccessTokenAsync());

    return !string.IsNullOrWhiteSpace(a);
  }

  /// <summary>
  ///   刷新 JsApiTicket
  /// </summary>
  /// <returns></returns>
  public async Task<bool> RefreshJsApiTicketAsync()
  {
    var a = await _stringCaching.SetValueAsync("WxJsApiTicket", async () => await GetJsApiTicketAsync());
    return !string.IsNullOrWhiteSpace(a);
  }


  /// <summary>
  ///   获取签名
  /// </summary>
  /// <param name="url"></param>
  /// <returns></returns>
  public async Task<SignPackage?> GetSignPackageFromCachingAsync(string url)
  {
    return await _wxSignPackageCaching.GetOrSetSignPackageAsync(Md5.Generate(url),
      async () => await SignPackageAsync(url),
      TimeSpan.FromMinutes(10));
  }

  /// <summary>
  ///   签名
  /// </summary>
  /// <param name="url"></param>
  /// <returns></returns>
  private async Task<SignPackage?> SignPackageAsync(string url)
  {
    var ticket = await GetJsApiTicketFromCachingAsync();
    if (string.IsNullOrWhiteSpace(ticket)) return null;
    var nonceStr = _rdString.Generate();
    var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
    var raw = $"jsapi_ticket={ticket}&noncestr={nonceStr}&timestamp={timestamp}&url={url}";
    var signature = Sha1.Generate(raw);
    return new SignPackage
    {
      AppId = _wx.AppId,
      NonceStr = nonceStr,
      Timestamp = timestamp,
      Url = url,
      Signature = signature
    };
  }

  private async Task<string?> GetAccessTokenFromCachingAsync()
  {
    return await _stringCaching.GetOrSetValueAsync("WxAccessToken", GetAccessTokenAsync,
      TimeSpan.FromSeconds(7000));
  }


  private async Task<string?> GetJsApiTicketFromCachingAsync()
  {
    return await _stringCaching.GetOrSetValueAsync("WxJsApiTicket", GetJsApiTicketAsync,
      TimeSpan.FromSeconds(7000));
  }

  /// <summary>
  ///   获取 AccessToken
  /// </summary>
  /// <returns></returns>
  private async Task<string> GetAccessTokenAsync()
  {
    var a = await _restClient.GetAsync<AccessTokenResult>(
      new RestRequest(AccessTokenUrl)
        .AddQueryParameter("grant_type", "client_credential")
        .AddQueryParameter("appid", _wx.AppId)
        .AddQueryParameter("secret", _wx.AppSecret)
    );

    if (!string.IsNullOrWhiteSpace(a?.AccessToken)) return a.AccessToken;

    return string.Empty;
  }

  /// <summary>
  ///   获取 JsApiTicket
  /// </summary>
  /// <returns></returns>
  private async Task<string> GetJsApiTicketAsync()
  {
    var accessToken = await GetAccessTokenFromCachingAsync();
    if (!string.IsNullOrWhiteSpace(accessToken))
    {
      var a = await _restClient.GetAsync<JsApiTicketResult>(
        new RestRequest(JsApsTicketUrl)
          .AddQueryParameter("access_token", accessToken)
          .AddQueryParameter("type", "jsapi"));

      if (!string.IsNullOrWhiteSpace(a?.Ticket)) return a.Ticket;
    }

    return string.Empty;
  }
}