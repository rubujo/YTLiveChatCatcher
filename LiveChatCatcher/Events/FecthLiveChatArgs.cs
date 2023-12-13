using Rubujo.YouTube.Utility.Models;

namespace Rubujo.YouTube.Utility.Events;

/// <summary>
/// 即時聊天獲取事件參數
/// </summary>
/// <param name="dataSet">List&lt;RendererData&gt;</param>
public class FecthLiveChatArgs(List<RendererData> dataSet) : EventArgs
{
    /// <summary>
    /// List&lt;RendererData&gt;
    /// </summary>
    public List<RendererData> Data { get; set; } = dataSet;
}