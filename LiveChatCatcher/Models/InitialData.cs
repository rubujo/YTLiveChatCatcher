using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models;

/// <summary>
/// 初始資料
/// </summary>
public class InitialData
{
    /// <summary>
    /// ytcfg 資料
    /// </summary>
    [JsonPropertyName("ytConfigData")]
    public YTConfigData? YTConfigData { get; set; }

    /// <summary>
    /// 影片聊天室的內容
    /// </summary>
    [JsonPropertyName("messages")]
    public List<RendererData>? Messages { get; set; }
}