namespace FeiShuGpt;

public class OpenAiHttpClientHandler : HttpClientHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 如果未配置端点则使用默认
        if (!OpenAiOptions.Endpoint.IsNullOrWhiteSpace())
        {
            var uriBuilder = new UriBuilder(OpenAiOptions.Endpoint.TrimEnd("/") + request.RequestUri.LocalPath);
            request.RequestUri = uriBuilder.Uri;
        }
        return base.SendAsync(request, cancellationToken);
    }
}