using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models.Community;

/// <summary>
/// 選擇資料類別
/// </summary>
public class ChoiceData
{
    /// <summary>
    /// 文字
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 圖片網址
    /// </summary>
    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 圖片的資料統一資源標識符
    /// </summary>
    [JsonPropertyName("imageDataUri")]
    public string? ImageDataUri { get; set; }

    /// <summary>
    /// 投票數
    /// </summary>
    [JsonPropertyName("numVotes")]
    public string? NumVotes { get; set; }

    /// <summary>
    /// 投票率
    /// </summary>
    [JsonPropertyName("votePercentage")]
    public string? VotePercentage { get; set; }

    /// <summary>
    /// 是否為測驗貼文（<see cref="AttachmentData.IsQuiz"/>）的正確答案
    /// <para>僅測驗貼文的選項才會有這個欄位；一般投票沒有正確答案的概念，此欄位為 null，代表不適用而非資料缺漏。</para>
    /// </summary>
    [JsonPropertyName("isCorrect")]
    public bool? IsCorrect { get; set; }
}