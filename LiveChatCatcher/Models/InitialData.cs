namespace LiveChatCatcher.Models;

/// <summary>
/// 初始資料
/// </summary>
public class InitialData
{
    /// <summary>
    /// ytcfg 資料
    /// </summary>
    public YTConfigData? YTConfigData { get; set; }

    /// <summary>
    /// 影片聊天室的內容
    /// </summary>
    public List<RendererData>? Messages { get; set; }
}