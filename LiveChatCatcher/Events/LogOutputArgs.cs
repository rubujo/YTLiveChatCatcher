using LiveChatCatcher.Sets;

namespace LiveChatCatcher.Events;

/// <summary>
/// 紀錄事件參數
/// </summary>
/// <param name="logType">EnumSet.LogType，紀錄類型</param>
/// <param name="message">字串，參數</param>
public class LogOutputArgs(EnumSet.LogType logType, string message) : EventArgs
{
    /// <summary>
    /// 紀錄類型
    /// </summary>
    public EnumSet.LogType LogType { get; } = logType;

    /// <summary>
    /// 訊息
    /// </summary>
    public string Message { get; } = message;
}