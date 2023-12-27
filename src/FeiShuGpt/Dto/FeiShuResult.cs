namespace FeiShuGpt.Dto;

public class FeiShuResult : FeiShuResultBase
{
    public object data { get; set; }
}
public class FeiShuResult<T> : FeiShuResultBase
{
    public T data { get; set; }
}
public abstract class FeiShuResultBase
{
    public int code { get; set; }
    public string msg { get; set; }
}
