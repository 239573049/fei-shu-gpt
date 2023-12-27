namespace FeiShuGpt.Options;

public class OpenAiOptions
{
    /// <summary>
    /// OpenAI API Key
    /// </summary>
    public static string Key { get; set; }

    /// <summary>
    /// 端点为空则使用默认
    /// </summary>
    public static string Endpoint { get; set; }

    /// <summary>
    /// 使用AI模型
    /// </summary>
    public static string Model { get; set; } = "gpt-3.5-turbo-1106";

    /// <summary>
    /// 最大历史记录
    /// </summary>
    public static int MaxHistory { get; set; } = 3;
}