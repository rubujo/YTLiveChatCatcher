using System.Text.Json;
using System.Text.Json.Serialization;
using Rubujo.YouTube.Utility.Models.Community;
using Rubujo.YouTube.Utility.Models.LiveChat;

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
    /// 即時聊天資料
    /// </summary>
    [JsonPropertyName("messages")]
    public List<RendererData>? Messages { get; set; }

    /// <summary>
    /// 社群貼文資料
    /// </summary>
    [JsonPropertyName("posts")]
    public List<PostData>? Posts { get; set; }
}