namespace Wx.Share.Models.SbWebResult;

public class SbWebResult
{
  /// <summary>
  ///   返回代码
  ///   0 正确;
  /// </summary>
  public int Code { get; set; }

  /// <summary>
  ///   数据
  /// </summary>
  public object? Data { get; set; }
}

public class SbWebResult<T> : SbWebResult
{
  public SbWebResult(int code)
  {
    Code = code;
  }

  public SbWebResult(T data)
  {
    Code = 0;
    Data = data;
  }

  public new T? Data { get; set; }
}