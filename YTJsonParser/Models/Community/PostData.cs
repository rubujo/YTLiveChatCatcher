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
    /// 是否為轉發貼文（2026/8 新增，YouTube 社群貼文的「在 YouTube 上轉發」功能）
    /// <para>為 true 時，<see cref="AuthorText"/>／<see cref="ContentTexts"/>／<see cref="Attachments"/> 等欄位
    /// 皆取自「被轉發的原始貼文」本身，跟一般貼文的語意一致；轉發本身的資訊（誰轉發、轉發時附加的文字）
    /// 另外存在 <see cref="RepostedByAuthorText"/>／<see cref="RepostCaptionTexts"/>。</para>
    /// </summary>
    [JsonPropertyName("isRepost")]
    public bool IsRepost { get; set; } = false;

    /// <summary>
    /// 執行轉發者的名稱（僅 <see cref="IsRepost"/> 為 true 時才會有值）
    /// <para>2026/8 實測樣本為頻道轉發自己先前的貼文，這個欄位跟原始作者剛好相同，
    /// 尚未用跨頻道轉發樣本驗證過這裡取到的是否一定是「轉發者」而非原始作者的重複欄位，
    /// 依據 YouTube 轉發 UI 的慣例（先顯示轉發者、再嵌入原始貼文）推斷為轉發者。</para>
    /// </summary>
    [JsonPropertyName("repostedByAuthorText")]
    public string? RepostedByAuthorText { get; set; }

    /// <summary>
    /// 轉發時附加的文字（僅 <see cref="IsRepost"/> 為 true 時才會有值；沒有附加文字時為 null）
    /// </summary>
    [JsonPropertyName("repostCaptionTexts")]
    public List<RunsData>? RepostCaptionTexts { get; set; }

    /// <summary>
    /// 已勾選
    /// </summary>
    [JsonIgnore]
    public bool IsChecked { get; set; } = false;
}