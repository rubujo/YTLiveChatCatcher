using LiveChatCatcher.Sets;

namespace LiveChatCatcher.Events;

/// <summary>
/// 執行狀態事件參數
/// </summary>
/// <param name="runningStatus">字串，訊息</param>
public class RunningStatusArgs(EnumSet.RunningStatus runningStatus) : EventArgs
{
    /// <summary>
    /// 執行狀態
    /// </summary>
    public EnumSet.RunningStatus RunningStatus { get; } = runningStatus;
}