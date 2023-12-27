namespace FeiShuGpt.Dto;

[Index("uk_event_id", nameof(EventId), true)]
public class EventDto
{
    protected EventDto()
    {
    }

    public EventDto(string eventId)
    {
        EventId = eventId;
        CreatedTime=DateTime.Now;
    }

    public string EventId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}