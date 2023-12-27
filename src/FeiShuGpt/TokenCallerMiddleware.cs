namespace FeiShuGpt;

public class TokenCallerMiddleware(IHttpClientFactory httpClientFactory) : ICallerMiddleware
{
    private DateTime? _lastTime;

    public async Task HandleAsync(MasaHttpContext masaHttpContext, CallerHandlerDelegate next,
        CancellationToken cancellationToken = new())
    {
        await RefreshTokenAsync(masaHttpContext.RequestMessage);
        await next();
    }

    private async ValueTask RefreshTokenAsync(HttpRequestMessage requestMessage)
    {
        if (requestMessage.Headers.Contains("Authorization"))
        {
            // 如果LastTime大于1.5小时，刷新token
            if (_lastTime != null && DateTime.Now - _lastTime < TimeSpan.FromHours(1.5))
            {
                return;
            }

            requestMessage.Headers.Remove("Authorization");
        }

        var client = httpClientFactory.CreateClient(nameof(RefreshTokenAsync));
        var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://open.feishu.cn/open-apis/auth/v3/app_access_token/internal");
        
        request.Content = new StringContent(JsonSerializer.Serialize(new
        {
            app_id = FeiShuOptions.AppId,
            app_secret = FeiShuOptions.AppSecret,
        }), Encoding.UTF8, "application/json");
        var response = await client.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<FeiShuToken>(result);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.TenantAccessToken);
        _lastTime = DateTime.Now;
    }
}

public class FeiShuToken
{
    [JsonPropertyName("tenant_access_token")]
    public string TenantAccessToken { get; set; }

    [JsonPropertyName("user_access_token")]
    public string UserAccessToken { get; set; }

    [JsonPropertyName("expire")] public int Expire { get; set; }
}