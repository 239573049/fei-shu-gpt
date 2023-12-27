using System.Linq;

namespace FeiShuGpt.Services;

public class ChatService(FeiShuCaller feiShuCaller, Kernel kernel, OpenAIChatCompletionService openAiChatCompletion)
    : ApplicationService<ChatService>
{
    /// <summary>
    /// 处理飞书开放平台的事件回调
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    public async Task CreateAsync(ChatInput input, HttpContext context)
    {
        if (!input.encrypt.IsNullOrWhiteSpace())
        {
            Logger.LogWarning("user enable encrypt key");

            await context.Response.WriteAsJsonAsync(new
            {
                code = 1,
                message = new
                {
                    zh_CN = "你配置了 Encrypt Key，请关闭该功能。",
                    en_US = "You have open Encrypt Key Feature, please close it.",
                }
            });
            return;
        }

        // 处理飞书开放平台的服务端校验
        if (input.type == "url_verification")
        {
            Logger.LogWarning("deal url_verification");
            await context.Response.WriteAsJsonAsync(new
            {
                input.challenge,
            });
            return;
        }

        // 处理飞书开放平台的事件回调
        if ((input.header.event_type == "im.message.receive_v1"))
        {
            var eventId = input.header.event_id;
            var messageId = input._event.message.message_id;
            var chatId = input._event.message.chat_id;
            // var senderId = input._event.sender.sender_id.user_id;
            var sessionId = input._event.sender.sender_id.open_id;

            // 对于同一个事件，只处理一次
            if (await FreeSql.Select<EventDto>().AnyAsync(x => x.EventId == eventId))
            {
                Logger.LogWarning("skip repeat event");
                await context.Response.WriteAsJsonAsync(new
                {
                    code = 0,
                });
                return;
            }

            await FreeSql.Insert(new EventDto(eventId)).ExecuteAffrowsAsync();

            // 私聊直接回复
            if (input._event.message.chat_type == "p2p")
            {
                // 不是文本消息，不处理
                if (input._event.message.message_type != "text")
                {
                    await Reply(messageId, "暂不支持其他类型的提问");
                    Logger.LogWarning("skip and Reply not support");
                    await context.Response.WriteAsJsonAsync(new
                    {
                        code = 0,
                    });
                    return;
                }

                // 是文本消息，直接回复
                var userInput = JsonSerializer.Deserialize<UserInput>(input._event.message.content);
                await HandleReply(userInput, sessionId);
                await context.Response.WriteAsJsonAsync(new
                {
                    code = 0,
                });
                return;
            }

            // 群聊，需要 @ 机器人
            if (input._event.message.chat_type == "group")
            {
                // 这是日常群沟通，不用管
                if (input._event.message.mentions.Length == 0)
                {
                    Logger.LogWarning("not process message without mention");
                    await context.Response.WriteAsJsonAsync(new
                    {
                        code = 0,
                    });
                    return;
                }

                // 没有 mention 机器人，则退出。
                if (input._event.message.mentions[0].name != "TokenAI")
                {
                    Logger.LogWarning("bot name not equal first mention name ");
                    await context.Response.WriteAsJsonAsync(new
                    {
                        code = 0,
                    });
                    return;
                }

                var userInput = JsonSerializer.Deserialize<UserInput>(input._event.message.content);
                await HandleReply(userInput, chatId, "chat_id");
                await context.Response.WriteAsJsonAsync(new
                {
                    code = 0,
                });
                return;
            }
        }

        Logger.LogWarning("return without other log");
        await context.Response.WriteAsJsonAsync(new
        {
            code = 2,
        });
    }

    async Task HandleReply(UserInput userInput, string sessionId, string receive_id_type = "open_id")
    {
        var question = userInput.text.Replace("@_user_1", "");
        Logger.LogWarning("question: " + question);
        var action = question.Trim();

        var prompt = await BuildConversation(sessionId, question);

        var history = new ChatHistory();
        history.AddRange(prompt);
        var response = await openAiChatCompletion.GetChatMessageContentAsync(history);

        await FreeSql.Insert(new MessageDto()
        {
            Content = question,
            CreatedTime = DateTime.Now,
            Id = Guid.NewGuid(),
            Role = GptRoleHelper.GetGptRole(AuthorRole.User),
            SessionId = sessionId,
            Type = "text",
        }).ExecuteAffrowsAsync();

        await Reply(sessionId, response.Content, receive_id_type);
    }


    // 根据sessionId构造用户会话
    async ValueTask<List<ChatMessageContent>> BuildConversation(string sessionId, string question)
    {
        int pageSize = 2;

        var result = await FreeSql.Select<MessageDto>().Where(x => x.SessionId == sessionId)
            .OrderByDescending(x => x.CreatedTime)
            .Page(1, pageSize)
            .ToListAsync();

        var prompt = result.OrderBy(x => x.CreatedTime).Select(value =>

            new ChatMessageContent(GptRoleHelper.GetAuthorRole(value.Role), value.Content)
        ).ToList();

        prompt.Add(new ChatMessageContent(AuthorRole.User, question));
        return prompt;
    }

    private async Task Reply(string sessionId, string message, string receive_id_type = "open_id")
    {
        try
        {
            await feiShuCaller.SendMessages(
                new SendMessageInput(JsonSerializer.Serialize(new
                {
                    text = message,
                }, Constant.JsonSerializerOptions), "text",
                    sessionId), receive_id_type);
        }
        catch (Exception e)
        {
            Logger.LogError("send message to feishu error", e, sessionId);
        }
    }
}