using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models;

/// <summary>
/// runs 資料
/// </summary>
public class RunsData
{
    /// <summary>
    /// 文字
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 是否為粗體
    /// </summary>
    [JsonPropertyName("bold")]
    public bool? Bold { get; set; }

    /// <summary>
    /// 文字顏色
    /// </summary>
    [JsonPropertyName("textColor")]
    public string? TextColor { get; set; }

    /// <summary>
    /// 字型
    /// </summary>
    [JsonPropertyName("fontFace")]
    public string? FontFace { get; set; }

    /// <summary>
    /// 列表：Emoji 資料
    /// </summary>
    [JsonPropertyName("emojis")]
    public List<EmojiData>? Emojis { get; set; }
}