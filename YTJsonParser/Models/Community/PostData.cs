using Rubujo.YouTube.Utility.Models.LiveChat;
using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models.Community;

/// <summary>
/// 貼文資料類別
/// </summary>
public class PostData
{
    /// <summary>
    /// 貼文 ID
    /// </summary>
    [JsonPropertyName("postId")]
    public string? PostID { get; set; }

    /// <summary>
    /// 貼文網址
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 作者文字
    /// </summary>
    [JsonPropertyName("authorText")]
    public string? AuthorText { get; set; }

    /// <summary>
    /// 作者頭像網址
    /// </summary>
    [JsonPropertyName("authorThumbnailUrl")]
    public string? AuthorThumbnailUrl { get; set; }

    /// <summary>
    /// 作者頭像資料統一資源標識符
    /// </summary>
    [JsonPropertyName("authorThumbnailDataUri")]
    public string? AuthorThumbnailDataUri { get; set; }

    /// <summary>
    /// 內容文字
    /// </summary>
    [JsonPropertyName("contentTexts")]
    public List<RunsData>? ContentTexts { get; set; }

    /// <summary>
    /// 發布時間文字
    /// </summary>
    [JsonPropertyName("publishedTimeText")]
    public string? PublishedTimeText { get; set; }

    /// <summary>
    /// 投票次數
    /// </summary>
    [JsonPropertyName("voteCount")]
    public string? VoteCount { get; set; }

    /// <summary>
    /// 附件
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<AttachmentData>? Attachments { get; set; }

    /// <summary>
    /// 是否為頻道會員專屬
    /// </summary>
    [JsonPropertyName("isSponsorsOnly")]
    public bool IsSponsorsOnly { get; set; } = false;

    /// <summary>
    /// 已勾選
    /// </summary>
    [JsonIgnore]
    public bool IsChecked { get; set; } = false;
}