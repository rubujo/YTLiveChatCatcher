using Rubujo.YouTube.Utility.Models.LiveChat;

namespace Rubujo.YouTube.Utility.Events;

/// <summary>
/// 即時聊天資料獲取事件參數
/// </summary>
/// <param name="dataSet">List&lt;RendererData&gt;</param>
public class FecthLiveChatDataArgs(List<RendererData> dataSet) : EventArgs
{
    /// <summary>
    /// List&lt;RendererData&gt;
    /// </summary>
    public List<RendererData> Data { get; set; } = dataSet;
}