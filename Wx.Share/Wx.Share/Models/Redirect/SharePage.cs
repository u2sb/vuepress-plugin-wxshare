using System.Text.Json.Serialization;

namespace Wx.Share.Models.Redirect;

public class SharePage
{
  [JsonPropertyName("url")] public string Url { get; set; } = null!;

  [JsonPropertyName("title")] public string? Title { get; set; }

  [JsonPropertyName("desc")] public string? Desc { get; set; }

  [JsonPropertyName("imgUrl")] public string? ImgUrl { get; set; }

  [JsonPropertyName("hash")] public string? Hash { get; set; }
}