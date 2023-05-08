using System.Security.Cryptography;
using System.Text;

namespace Wx.Share.Utils.Encrypt;

public class Sha1
{
  public static string Generate(string str)
  {
    if (string.IsNullOrWhiteSpace(str)) return string.Empty;

    using var sha1 = SHA1.Create();
    var bytesSha1In = Encoding.UTF8.GetBytes(str);
    var bytesSha1Out = sha1.ComputeHash(bytesSha1In);
    sha1.Clear();

    var strSha1Out = BitConverter.ToString(bytesSha1Out);
    strSha1Out = strSha1Out.Replace("-", "");
    return strSha1Out;
  }
}