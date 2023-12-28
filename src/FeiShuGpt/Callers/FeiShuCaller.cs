using FeiShuGpt.Callers.Middlewares;

namespace FeiShuGpt.Callers;

public class FeiShuCaller(ICaller caller, ILogger<FeiShuCaller> logger) : HttpClientCallerBase
{
    protected override string BaseAddress { get; set; }

    public async ValueTask SendMessages(SendMessageInput input, string receive_id_type = "open_id")
    {
        var result =
            await caller.PostAsync("https://open.feishu.cn/open-apis/im/v1/messages?receive_id_type=" + receive_id_type,
                input, false);

        var response = await result.Content.ReadFromJsonAsync<FeiShuResult>();

        if (response.code == 0)
        {
            return;
        }

        logger.LogError("发送错误：" + response.msg);
    }

    protected override void UseHttpClientPost(MasaHttpClientBuilder masaHttpClientBuilder)
    {
        masaHttpClientBuilder.AddMiddleware<TokenCallerMiddleware>();
    }
}