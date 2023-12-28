namespace FeiShuGpt.Services;

public class ChatService(ChatAiService chatAiService)
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
        if (input.header.event_type == "im.message.receive_v1")
        {
            var eventId = input.header.event_id; // 事件id
            var messageId = input._event.message.message_id; // 消息id
            var chatId = input._event.message.chat_id; // 群聊id
            var senderId = input._event.sender.sender_id.user_id; // 发送人id
            var sessionId = input._event.sender.sender_id.open_id; // 发送人openid

            // 对于同一个事件，只处理一次
            if (await FreeSql.Select<EventDto>().AnyAsync(x => x.EventId == eventId))
            {
                Logger.LogInformation("跳过重复事件");
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
                    await chatAiService.SendMessages(messageId, "暂不支持其他类型的提问");
                    Logger.LogWarning("暂不支持其他类型的提问");
                    await context.Response.WriteAsJsonAsync(new
                    {
                        code = 0,
                    });
                    return;
                }

                // 是文本消息，直接回复
                var userInput = JsonSerializer.Deserialize<UserInput>(input._event.message.content);
                await chatAiService.TextMessageAsync(userInput, sessionId);
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
                await chatAiService.TextMessageAsync(userInput, chatId, "chat_id");
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
}