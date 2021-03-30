namespace YTLiveChatCatcher.Models;

/// <summary>
/// message 的資料
/// </summary>
public class MessageData
{
    /// <summary>
    /// 文字
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 列表：Emoji 資料
    /// </summary>
    public List<EmojiData>? Emojis { get; set; }
}