using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models.Community;

/// <summary>
/// 影片資料類別
/// </summary>
public class VideoData
{
    /// <summary>
    /// 影片 ID
    /// </summary>
    [JsonPropertyName("id")]
    public string? ID { get; set; }

    /// <summary>
    /// 影片網址
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 縮略圖網址
    /// </summary>
    [JsonPropertyName("thumbnailUrl")]
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// 標題
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [JsonPropertyName("descriptionSnippet")]
    public string? DescriptionSnippet { get; set; }

    /// <summary>
    /// 發布時間文字
    /// </summary>
    [JsonPropertyName("publishedTimeText")]
    public string? PublishedTimeText { get; set; }

    /// <summary>
    /// 長度文字
    /// </summary>
    [JsonPropertyName("lengthText")]
    public string? LengthText { get; set; }

    /// <summary>
    /// 觀看次數文字
    /// </summary>
    [JsonPropertyName("viewCountText")]
    public string? ViewCountText { get; set; }

    /// <summary>
    /// 所有者文字
    /// </summary>
    [JsonPropertyName("ownerText")]
    public string? OwnerText { get; set; }
}