namespace FeiShuGpt.Dto;

[Index("uk_session_id", nameof(SessionId), false)]
public class MessageDto
{
    [Column(IsIdentity = true)]
    public Guid Id { get; set; }

    public string SessionId { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Column(StringLength =-1)]
    public string Content { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }

    public GptRole Role { get; set; }

}