using Microsoft.Extensions.Logging;
using System.Text.Json;
using Xunit;

namespace Rubujo.YouTube.Utility.Tests;

public class UnsupportedActionTests
{
    [Fact]
    public async Task 未知action在Debug開啟Trace關閉時仍留下診斷()
    {
        string response = JsonSerializer.Serialize(new
        {
            continuationContents = new { liveChatContinuation = new
            { actions = new[] { new { unrecognizedTestAction = new { test = true } } } } }
        });
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Get, "/live_chat?is_popout=1", File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "live_popout_active.html")))
            .When(HttpMethod.Post, "/youtubei/v1/live_chat/get_live_chat", response);
        using HttpClient client = new(handler);
        DebugLogger logger = new();
        using YTJsonParser parser = new(new YTJsonParserOptions { HttpClient = client }, logger);
        await foreach (var _ in parser.StreamLiveChatDataAsync("TEST_VIDEO_ID", options: new() { ForceIntervalMs = 0 },
            cancellationToken: TestContext.Current.CancellationToken)) { }
        Assert.Contains(logger.Messages, message => message.Contains("unrecognizedTestAction", StringComparison.Ordinal));
    }

    private sealed class DebugLogger : ILogger<YTJsonParser>
    {
        public List<string> Messages { get; } = [];
        public bool IsEnabled(LogLevel level) => level >= LogLevel.Debug;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error, Func<TState, Exception?, string> formatter)
        { if (id.Id == 15) Messages.Add(formatter(state, error)); }
    }
}
