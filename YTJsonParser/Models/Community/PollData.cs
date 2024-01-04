using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models.Community;

/// <summary>
/// 投票資料類別
/// </summary>
public class PollData
{
    /// <summary>
    /// 選擇資料
    /// </summary>
    [JsonPropertyName("choiceDatas")]
    public List<ChoiceData>? ChoiceDatas { get; set; }

    /// <summary>
    /// 總投票數
    /// </summary>
    [JsonPropertyName("totalVotes")]
    public string? TotalVotes { get; set; }
}