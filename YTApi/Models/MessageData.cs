namespace YTApi.Models;

/// <summary>
/// Message 資料
/// </summary>
public class MessageData
{
    /// <summary>
    /// 文字
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 是否為粗體
    /// </summary>
    public bool? Bold { get; set; }

    /// <summary>
    /// 文字顏色
    /// </summary>
    public string? TextColor { get; set; }

    /// <summary>
    /// 字型
    /// </summary>
    public string? FontFace { get; set; }

    /// <summary>
    /// 列表：Sticker 資料
    /// </summary>
    public List<StickerData>? Stickers { get; set; }

    /// <summary>
    /// 列表：Emoji 資料
    /// </summary>
    public List<EmojiData>? Emojis { get; set; }
}