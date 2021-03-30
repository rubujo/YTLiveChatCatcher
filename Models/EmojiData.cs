namespace YTLiveChatCatcher.Models;

/// <summary>
/// Emoji 資料
/// </summary>
public class EmojiData
{
    /// <summary>
    /// Emoji 的 ID 值
    /// </summary>
    public string? ID { get; set; }

    /// <summary>
    /// 影像檔的網址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 文字
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 標籤
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// 是否為自定義表情符號
    /// </summary>
    public bool IsCustomEmoji { get; set; }

    /// <summary>
    /// 影像檔的 Image
    /// </summary>
    public Image? Image { get; set; }

    /// <summary>
    /// 影像檔的格式
    /// </summary>
    public string? Format { get; set; }
}