namespace Rubujo.YouTube.Utility.Sets;

/// <summary>
/// 列舉組
/// </summary>
public class EnumSet
{
    /// <summary>
    /// 紀錄類型
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// 資訊
        /// </summary>
        Info,
        /// <summary>
        /// 警告
        /// </summary>
        Warn,
        /// <summary>
        /// 錯誤
        /// </summary>
        Error,
        /// <summary>
        /// 除錯
        /// </summary>
        Debug
    }

    /// <summary>
    /// 執行狀態
    /// </summary>
    public enum RunningStatus
    {
        /// <summary>
        /// 執行中
        /// </summary>
        Running,
        /// <summary>
        /// 發生錯誤
        /// </summary>
        ErrorOccured,
        /// <summary>
        /// 已停止
        /// </summary>
        Stopped
    }
}