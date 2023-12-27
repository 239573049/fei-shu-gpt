namespace FeiShuGpt.Dto;

public class SendMessageInput(string content, string msg_type, string receive_id)
{
    public string content { get; set; } = content;

    public string msg_type { get; set; } = msg_type;

    public string receive_id { get; set; } = receive_id;
}