using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models;

/// <summary>
/// *Renderer 資料
/// </summary>
public class RendererData
{
    /// <summary>
    /// ID 值
    /// </summary>
    [JsonPropertyName("id")]
    public string? ID { get; set; }

    /// <summary>
    /// 類型
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// 時間標記（Unix 秒數）
    /// </summary>
    [JsonPropertyName("timestampUsec")]
    public string? TimestampUsec { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    [JsonPropertyName("authorName")]
    public string? AuthorName { get; set; }

    /// <summary>
    /// 使用者相片影像檔網址
    /// </summary>
    [JsonPropertyName("authorPhotoUrl")]
    public string? AuthorPhotoUrl { get; set; }

    /// <summary>
    /// 使用者徽章（文字）
    /// </summary>
    [JsonPropertyName("authorBadges")]
    public string? AuthorBadges { get; set; }

    /// <summary>
    /// 訊息內容
    /// </summary>
    [JsonPropertyName("messageContent")]
    public string? MessageContent { get; set; }

    /// <summary>
    /// 購買金額（文字格式）
    /// </summary>
    [JsonPropertyName("purchaseAmountText")]
    public string? PurchaseAmountText { get; set; }

    /// <summary>
    /// 前景顏色（Hex 色碼）
    /// </summary>
    [JsonPropertyName("foregroundColor")]
    public string? ForegroundColor { get; set; }

    /// <summary>
    /// 背景顏色（Hex 色碼）
    /// </summary>
    [JsonPropertyName("backgroundColor")]
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// 時間標記（文字格式）
    /// </summary>
    [JsonPropertyName("timestampText")]
    public string? TimestampText { get; set; }

    /// <summary>
    /// 使用者外部頻道 ID
    /// </summary>
    [JsonPropertyName("authorExternalChannelID")]
    public string? AuthorExternalChannelID { get; set; }

    /// <summary>
    /// 列表：Sticker 資料
    /// </summary>
    [JsonPropertyName("stickers")]
    public List<StickerData>? Stickers { get; set; }

    /// <summary>
    /// 列表：Emoji 資料
    /// </summary>
    [JsonPropertyName("emojis")]
    public List<EmojiData>? Emojis { get; set; }

    /// <summary>
    /// 列表：徽章資料
    /// </summary>
    [JsonPropertyName("badges")]
    public List<BadgeData>? Badges { get; set; }
}