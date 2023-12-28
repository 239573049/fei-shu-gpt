namespace FeiShuGpt.Services;

/// <summary>
/// AI对话处理服务
/// </summary>
/// <param name="feiShuCaller"></param>
/// <param name="openAiChatCompletion"></param>
public class ChatAiService(
    FeiShuCaller feiShuCaller,
    IChatCompletionService openAiChatCompletion,
    IFreeSql freeSql,
    ILogger<ChatAiService> logger)
    : ISingletonDependency
{
    public async ValueTask TextMessageAsync(UserInput userInput, string openId, string receiveIdType = "open_id")
    {
        var question = userInput.text.Replace("@_user_1", "");
        var action = question.Trim();

        var prompt = await BuildConversation(openId, question);

        var history = new ChatHistory();
        history.AddRange(prompt);
        var response = await openAiChatCompletion.GetChatMessageContentAsync(history);

        if (response.Content.IsNullOrWhiteSpace())
        {
            await SendMessages(openId, "我不知道你在说什么", receiveIdType);
            return;
        }

        await freeSql.Insert(new MessageDto()
        {
            Content = question,
            CreatedTime = DateTime.Now,
            Id = Guid.NewGuid(),
            Role = GptRoleHelper.GetGptRole(AuthorRole.User),
            SessionId = openId,
            Type = "text",
        }).ExecuteAffrowsAsync();

        await SendMessages(openId, response.Content, receiveIdType);
    }

    /// <summary>
    /// 根据sessionId构造用户会话
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="question"></param>
    /// <returns></returns>
    private async ValueTask<List<ChatMessageContent>> BuildConversation(string sessionId, string question)
    {
        int pageSize = 2;

        var result = await freeSql.Select<MessageDto>().Where(x => x.SessionId == sessionId)
            .OrderByDescending(x => x.CreatedTime)
            .Page(1, pageSize)
            .ToListAsync();

        var prompt = result.OrderBy(x => x.CreatedTime).Select(value =>
            new ChatMessageContent(GptRoleHelper.GetAuthorRole(value.Role), value.Content)
        ).ToList();

        prompt.Add(new ChatMessageContent(AuthorRole.User, question));
        return prompt;
    }

    public async Task SendMessages(string sessionId, string message, string receiveIdType = "open_id")
    {
        try
        {
            await feiShuCaller.SendMessages(
                new SendMessageInput(JsonSerializer.Serialize(new
                    {
                        text = message,
                    }, Constant.JsonSerializerOptions), "text",
                    sessionId), receiveIdType);
        }
        catch (Exception e)
        {
            logger.LogError("send message to feishu error {0} sessionId:{1}", e, sessionId);
        }
    }
}