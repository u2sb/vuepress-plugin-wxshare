using System.Security.Cryptography;
using System.Text;

namespace Wx.Share.Utils.Encrypt;

public class Md5
{
  public static string Generate(string str, bool isLong32 = true)
  {
    if (string.IsNullOrWhiteSpace(str)) return string.Empty;

    using var md5 = MD5.Create();
    var bytesMd5In = Encoding.UTF8.GetBytes(str);
    var bytesMd5Out = md5.ComputeHash(bytesMd5In);
    md5.Clear();

    var strMd5Out = isLong32
      ? BitConverter.ToString(bytesMd5Out)
      : BitConverter.ToString(bytesMd5Out, 4, 8);

    strMd5Out = strMd5Out.Replace("-", "");
    return strMd5Out;
  }
}