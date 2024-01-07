using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models.Community;

/// <summary>
/// 附件資料類別
/// </summary>
public class AttachmentData
{
    /// <summary>
    /// 網址
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 資料統一資源標識符
    /// </summary>
    [JsonPropertyName("dataUri")]
    public string? DataUri { get; set; }

    /// <summary>
    /// 是否為影片
    /// </summary>
    [JsonPropertyName("isVideo")]
    public bool IsVideo { get; set; } = false;

    /// <summary>
    /// 影片資料
    /// </summary>
    [JsonPropertyName("videoData")]
    public VideoData? VideoData { get; set; }

    /// <summary>
    /// 是否為投票
    /// </summary>
    [JsonPropertyName("isPoll")]
    public bool IsPoll { get; set; } = false;

    /// <summary>
    /// 投票資料
    /// </summary>
    [JsonPropertyName("pollData")]
    public PollData? PollData { get; set; }
}