namespace FeiShuGpt;

public static class Constant
{
    public const string APPID = nameof(APPID);
    public const string APPSECRET = nameof(APPSECRET);
    public const string GPTKEY = nameof(GPTKEY);
    public const string MODEL = nameof(MODEL);
    public const string ENDPOINT = nameof(ENDPOINT);
    public const string MAXHISTORY = nameof(MAXHISTORY);
    public const string BOTNAME = nameof(BOTNAME);


    public static JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}