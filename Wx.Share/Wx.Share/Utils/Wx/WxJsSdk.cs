using System.Security.Cryptography;
using System.Text;
using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;
using RestSharp;
using Wx.Share.Models.Settings;
using Wx.Share.Models.Wx;
using Wx.Share.Utils.Caching;

namespace Wx.Share.Utils.Wx;

public class WxJsSdk
{
    private const string AccessTokenUrl = "https://api.weixin.qq.com/cgi-bin/token";
    private const string JsApsTicketUrl = "https://api.weixin.qq.com/cgi-bin/ticket/getticket";
    private readonly DefaultCaching _caching;
    private readonly IRandomizerString _rdString;
    private readonly RestClient _restClient;
    private readonly WxSdk _wx;

    public WxJsSdk(AppSettings appSettings, RestClient restClient, DefaultCaching defaultCaching)
    {
        _wx = appSettings.WxSdk;
        _restClient = restClient;
        _caching = defaultCaching;

        _rdString = RandomizerFactory.GetRandomizer(new FieldOptionsText
            { UseNumber = false, UseSpecial = false, Max = 10, Min = 6 });
    }

    /// <summary>
    ///     获得当前时间戳
    /// </summary>
    /// <returns></returns>
    private long TimeStampNow =>
        (long)(DateTime.UtcNow - TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1, 0, 0, 0, 0))).TotalSeconds;

    /// <summary>
    ///     获取 SignPackage
    /// </summary>
    /// <returns></returns>
    public async Task<SignPackage?> GetSignPackageAsync(string url)
    {
        return await _caching.GetAsync($"sp{url}", async () => await SignPackageAsync(url), TimeSpan.FromMinutes(5));
    }

    /// <summary>
    ///     刷新 AccessToken
    /// </summary>
    /// <returns></returns>
    public async Task<bool> RefreshAccessTokenAsync()
    {
        var a = await _caching.SetAsync("WxAccessToken", async () => await GetAccessTokenAsync(), TimeSpan.FromSeconds(
            3500));

        return !string.IsNullOrWhiteSpace(a);
    }

    /// <summary>
    ///     刷新 JsApiTicket
    /// </summary>
    /// <returns></returns>
    public async Task<bool> RefreshJsApiTicketAsync()
    {
        var a = await _caching.SetAsync("WxJsApiTicket", async () => await GetJsApiTicketAsync(),
            TimeSpan.FromSeconds(3500));
        return !string.IsNullOrWhiteSpace(a);
    }


    private async Task<SignPackage?> SignPackageAsync(string url)
    {
        var ticket = await GetJsApiTicketFromCachingAsync();
        if (string.IsNullOrWhiteSpace(ticket)) return null;
        var nonceStr = _rdString.Generate();
        var timestamp = TimeStampNow;
        var raw = $"jsapi_ticket={ticket}&noncestr={nonceStr}&timestamp={timestamp}&url={url}";
        var signature = Sha1Sign(raw);
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
        return await _caching.GetAsync("WxAccessToken", async () => await GetAccessTokenAsync(), TimeSpan.FromSeconds(
            3500));
    }

    private async Task<string?> GetJsApiTicketFromCachingAsync()
    {
        return await _caching.GetAsync("WxJsApiTicket", async () => await GetJsApiTicketAsync(),
            TimeSpan.FromSeconds(3500));
    }

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

    /// <summary>
    ///     SHA1
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private string Sha1Sign(string data)
    {
        var temp1 = Encoding.UTF8.GetBytes(data);
        var sha = SHA1.Create();
        var temp2 = sha.ComputeHash(temp1);
        sha.Clear();
        // 注意， 不能用这个
        // string output = Convert.ToBase64String(temp2);// 不能直接转换成base64string
        var output = BitConverter.ToString(temp2);
        output = output.Replace("-", "");
        output = output.ToLower();
        return output;
    }
}