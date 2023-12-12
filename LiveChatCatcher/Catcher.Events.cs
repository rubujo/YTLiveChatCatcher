using LiveChatCatcher.Events;
using LiveChatCatcher.Models;
using LiveChatCatcher.Sets;

namespace LiveChatCatcher;

/// <summary>
/// Catcher 的事件
/// </summary>
public partial class Catcher
{
    /// <summary>
    /// 紀錄輸出事件
    /// </summary>
    public event EventHandler<LogOutputArgs>? OnLogOutput;

    /// <summary>
    /// 即時聊天獲取事件
    /// </summary>
    public event EventHandler<FecthLiveChatArgs>? OnFecthLiveChat;

    /// <summary>
    /// 執行狀態更新事件
    /// </summary>
    public event EventHandler<RunningStatusArgs>? OnRunningStatusUpdate;

    /// <summary>
    /// 引發紀錄輸出事件
    /// </summary>
    /// <param name="logType">EnumSet.LogType，紀錄類型</param>
    /// <param name="message">字串，訊息</param>
    private void RaiseOnLogOutput(EnumSet.LogType logType, string message) =>
        OnLogOutput?.Invoke(this, new LogOutputArgs(logType, message));

    /// <summary>
    /// 引發即時聊天獲取事件
    /// </summary>
    /// <param name="dataSet">List&lt;RendererData&gt;</param>
    private void RaiseOnFecthLiveChat(List<RendererData> dataSet) =>
        OnFecthLiveChat?.Invoke(this, new FecthLiveChatArgs(dataSet));

    /// <summary>
    /// 引發執行狀態更新事件
    /// </summary>
    /// <param name="runningStatus">RunningStatusArgs，執行狀態</param>
    private void RaiseOnRunningStatusUpdate(EnumSet.RunningStatus runningStatus) =>
        OnRunningStatusUpdate?.Invoke(this, new RunningStatusArgs(runningStatus));
}