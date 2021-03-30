namespace YTLiveChatCatcher.Models;

/// <summary>
/// runs 的資料
/// </summary>
public class RunsData
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