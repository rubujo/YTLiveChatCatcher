using Microsoft.Extensions.Logging;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// YTJsonParser 內部使用的 source-generated 記錄訊息
/// <para>大部分呼叫點使用通用範本（<see cref="Error"/>／<see cref="Warning"/>／<see cref="Debug"/>／<see cref="Trace"/>），
/// 少數語意明確、值得結構化的事件則有專屬方法。</para>
/// </summary>
internal static partial class LogMessages
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "[{Context}] {Message}")]
    public static partial void Error(ILogger logger, string context, string message);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "[{Context}] {Message}")]
    public static partial void Warning(ILogger logger, string context, string message);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "{Message}")]
    public static partial void Info(ILogger logger, string message);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "[{Context}] {Message}")]
    public static partial void Debug(ILogger logger, string context, string message);

    [LoggerMessage(EventId = 5, Level = LogLevel.Trace, Message = "[{Context}] {Content}")]
    public static partial void Trace(ILogger logger, string context, string content);

    [LoggerMessage(EventId = 10, Level = LogLevel.Error, Message = "[{MethodName}] 變數 \"ytConfigData\" 是 null！")]
    public static partial void YtConfigDataIsNull(ILogger logger, string methodName);

    [LoggerMessage(EventId = 11, Level = LogLevel.Information,
        Message = "未抓取到任何初始貼文（頻道本身沒有社群貼文屬於正常情況；若確定該頻道有貼文卻持續出現本訊息，才需要檢查 Parse 邏輯）。")]
    public static partial void NoInitialPosts(ILogger logger);

    [LoggerMessage(EventId = 12, Level = LogLevel.Debug, Message = "[ParseSubMenuItemsContinuation] CustomLiveChatType：{CustomTitle}")]
    public static partial void SubMenuCustomTitle(ILogger logger, string customTitle);

    [LoggerMessage(EventId = 13, Level = LogLevel.Debug, Message = "[ParseSubMenuItemsContinuation] title：{Title}")]
    public static partial void SubMenuTitle(ILogger logger, string? title);

    [LoggerMessage(EventId = 14, Level = LogLevel.Error,
        Message = "[{Context}] 連線發生錯誤，錯誤碼：{StatusCode}\n接收到的內容：\n{Content}")]
    public static partial void HttpError(ILogger logger, string context, string? statusCode, string content);
}
