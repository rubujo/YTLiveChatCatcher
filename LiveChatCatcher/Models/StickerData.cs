using Microsoft.Maui.Graphics;

namespace LiveChatCatcher.Models;

/// <summary>
/// Sticker 資料
/// </summary>
public class StickerData
{
    /// <summary>
    /// Sticker 的 ID 值
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
    /// 影像檔的格式
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// 影像
    /// </summary>
    public IImage? Image { get; set; }
}