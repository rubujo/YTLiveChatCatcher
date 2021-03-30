namespace YTLiveChatCatcher.Models;

/// <summary>
/// 徽章資料
/// </summary>
public class BadgeData
{
    /// <summary>
    /// 工具提示
    /// </summary>
    public string? Tooltip { get; set; }

    /// <summary>
    /// 標籤
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// 圖示類型
    /// </summary>
    public string? IconType { get; set; }

    /// <summary>
    /// 影像檔的網址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 影像檔的 Image
    /// </summary>
    public Image? Image { get; set; }

    /// <summary>
    /// 影像檔的格式
    /// </summary>
    public string? Format { get; set; }
}