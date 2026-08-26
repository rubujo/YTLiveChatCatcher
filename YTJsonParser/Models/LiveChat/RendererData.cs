using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models.LiveChat;

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
    /// 標頭背景顏色（Hex 色碼）
    /// <para>僅付費類訊息（例如超級留言／超級貼圖）才會有；一般訊息為 null，代表不適用而非資料缺漏。</para>
    /// </summary>
    [JsonPropertyName("headerBackgroundColor")]
    public string? HeaderBackgroundColor { get; set; }

    /// <summary>
    /// 排行榜徽章的名次文字（例如 "#1"）
    /// <para>僅出現在有排行榜徽章的付費類訊息上；沒有排行榜徽章時為 null，代表不適用而非資料缺漏。</para>
    /// </summary>
    [JsonPropertyName("leaderboardRank")]
    public string? LeaderboardRank { get; set; }

    /// <summary>
    /// 回覆數更新事件的關聯鍵值
    /// <para>僅付費類訊息（例如超級留言／超級貼圖）才會有；一般訊息為 null，代表不適用而非資料缺漏。
    /// 訊息剛送出時尚無回覆，之後若有人回覆，會在後續某一批資料中收到一筆
    /// <see cref="Type"/> 為「回覆數更新」的 <see cref="RendererData"/>，其 <see cref="ID"/>
    /// 會等於這裡的值——呼叫端需自行保留這個對照關係，才能把新的 <see cref="ReplyCount"/> 更新回原始訊息上。</para>
    /// </summary>
    [JsonPropertyName("replyCountEntityKey")]
    public string? ReplyCountEntityKey { get; set; }

    /// <summary>
    /// 回覆數
    /// <para>一般訊息、以及尚未有任何回覆的付費類訊息皆為 null（並非資料缺漏）。
    /// 只有在收到「回覆數更新」事件（<see cref="ReplyCountEntityKey"/> 對照關係）時才會有值。</para>
    /// </summary>
    [JsonPropertyName("replyCount")]
    public string? ReplyCount { get; set; }

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