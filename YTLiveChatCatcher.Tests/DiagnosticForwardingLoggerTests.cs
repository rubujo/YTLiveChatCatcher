using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Rubujo.YouTube.Utility;
using YTLiveChatCatcher.Common.Utils;
using Xunit;

namespace YTLiveChatCatcher.Tests;

public class DiagnosticForwardingLoggerTests
{
    // EventId 15 對應 YTJsonParser 的 LogMessages.UnsupportedContentEncountered——
    // LogMessages 是 internal，測試專案刻意不透過 InternalsVisibleTo 存取（跟 YTJsonParser.Tests
    // 的既有慣例一致，見 AGENTS.md），這裡直接呼叫 ILogger.Log 這個公開介面方法本身來驗證攔截邏輯，
    // 不依賴 YTJsonParser 內部實作。
    private const int UnsupportedContentEventId = 15;

    private static void Log(ILogger<YTJsonParser> logger, int eventId, string message) =>
        logger.Log(LogLevel.Debug, new EventId(eventId), message, null, (state, _) => state);

    [Fact]
    public void Log_EventId為15時_會呼叫回呼且訊息內容正確()
    {
        List<string> capturedMessages = [];

        DiagnosticForwardingLogger logger = new(
            NullLogger<YTJsonParser>.Instance,
            onUnsupportedContentEncountered: capturedMessages.Add);

        Log(logger, UnsupportedContentEventId, "[ParseNonMessageAction -> 尚未支援的 action 類型] {\"someBrandNewUnknownAction\":{}}");

        string captured = Assert.Single(capturedMessages);
        Assert.Contains("someBrandNewUnknownAction", captured);
    }

    [Theory]
    [InlineData(1)]  // Error
    [InlineData(2)]  // Warning
    [InlineData(3)]  // Info
    [InlineData(4)]  // Debug（通用範本）
    [InlineData(10)] // YtConfigDataIsNull
    public void Log_EventId不是15時_不會呼叫回呼(int otherEventId)
    {
        List<string> capturedMessages = [];

        DiagnosticForwardingLogger logger = new(
            NullLogger<YTJsonParser>.Instance,
            onUnsupportedContentEncountered: capturedMessages.Add);

        Log(logger, otherEventId, "不應該被轉送的一般記錄訊息");

        Assert.Empty(capturedMessages);
    }

    [Fact]
    public void Log_一律轉送給內層Logger_不論EventId是否為15()
    {
        RecordingLogger innerLogger = new();

        DiagnosticForwardingLogger logger = new(innerLogger, onUnsupportedContentEncountered: _ => { });

        Log(logger, UnsupportedContentEventId, "訊息A");
        Log(logger, 1, "訊息B");

        Assert.Equal(2, innerLogger.LoggedMessages.Count);
        Assert.Contains("訊息A", innerLogger.LoggedMessages);
        Assert.Contains("訊息B", innerLogger.LoggedMessages);
    }

    /// <summary>
    /// 用來驗證 DiagnosticForwardingLogger 是否真的把每一筆記錄都轉送給內層 Logger 的最小假實作。
    /// </summary>
    private sealed class RecordingLogger : ILogger<YTJsonParser>
    {
        public List<string> LoggedMessages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            LoggedMessages.Add(formatter(state, exception));
    }
}
