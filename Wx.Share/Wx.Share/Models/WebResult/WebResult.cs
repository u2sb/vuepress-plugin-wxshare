namespace Wx.Share.Models.WebResult;

public class WebResult<T>
{
    public WebResult()
    {
    }

    public WebResult(int code)
    {
        Code = code;
    }

    public WebResult(T data) : this(0)
    {
        Data = data;
    }

    public int Code { get; set; } = 0;

    public T? Data { get; set; }
}

public class WebResult : WebResult<dynamic>
{
    public WebResult()
    {
    }

    public WebResult(int code) : base(code)
    {
    }
}