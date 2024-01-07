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
}