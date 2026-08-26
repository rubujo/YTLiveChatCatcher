using Microsoft.Extensions.Logging;
using Rubujo.YouTube.Utility;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>
/// 包裝真正的 <see cref="ILogger{YTJsonParser}"/>，行為完全不變（一樣依既有 NLog 設定寫進檔案／主控台），
/// 但額外攔截 <see cref="LogMessages.UnsupportedContentEncountered"/>（EventId 15：遇到尚未支援的內容/類型）
/// 這一個特定事件，轉送給呼叫端指定的回呼。
/// <para>刻意不是「把 YTJsonParser 所有內部記錄都轉送到 UI」——那個決定（見 AGENTS.md「已知的行為變化」）維持
/// 不變，避免洗版；只有這一個對正在盯著畫面的使用者有即時意義的事件（代表 YouTube 可能又新增了本函式庫不
/// 認得的元素，這批資料可能沒有被完整解析）才值得在擷取當下就讓使用者看到，而不是事後才想到要去挖記錄檔。</para>
/// </summary>
/// <param name="innerLogger">實際負責記錄的 ILogger&lt;YTJsonParser&gt;（例如 DI 容器提供、接到 NLog 的那個）</param>
/// <param name="onUnsupportedContentEncountered">攔截到 EventId 15 時要呼叫的回呼，帶入已格式化的訊息文字</param>
public sealed class DiagnosticForwardingLogger(
    ILogger<YTJsonParser> innerLogger,
    Action<string> onUnsupportedContentEncountered) : ILogger<YTJsonParser>
{
    private const int UnsupportedContentEventId = 15;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
        innerLogger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => innerLogger.IsEnabled(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        innerLogger.Log(logLevel, eventId, state, exception, formatter);

        if (eventId.Id == UnsupportedContentEventId)
        {
            onUnsupportedContentEncountered(formatter(state, exception));
        }
    }
}
