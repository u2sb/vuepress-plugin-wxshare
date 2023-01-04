using System.Text.Json.Serialization;

namespace Wx.Share.Models.Wx;

public class SignPackage
{
    [JsonPropertyName("appId")] public string? AppId { get; set; }
    [JsonPropertyName("nonceStr")] public string? NonceStr { get; set; }
    [JsonPropertyName("timestamp")] public long Timestamp { get; set; }
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("signature")] public string? Signature { get; set; }
}