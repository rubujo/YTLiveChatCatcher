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
    /// <para>當 <see cref="IsQuiz"/> 為 true 時，這裡放的是測驗貼文的選項與作答人數
    /// （<see cref="ChoiceData.IsCorrect"/> 會標示正確答案），沿用同一個欄位是因為兩者資料形狀相同，
    /// 沒有另外新增 QuizData 模型的必要。</para>
    /// </summary>
    [JsonPropertyName("pollData")]
    public PollData? PollData { get; set; }

    /// <summary>
    /// 是否為測驗貼文（2026/8 新增，YouTube 社群貼文的測驗功能，choices 內會標示正確答案）
    /// </summary>
    [JsonPropertyName("isQuiz")]
    public bool IsQuiz { get; set; } = false;
}