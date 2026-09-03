using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Models.LiveChat;
using Rubujo.YouTube.Utility.Sets;
using Xunit;

namespace Rubujo.YouTube.Utility.Tests;

public class LiveChatStreamingTests
{
    private static string ReadFixture(string fileName) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName));

    [Fact]
    public async Task StreamLiveChatDataAsync_初始批次與輪詢批次_正確分派各種訊息類型()
    {
        string popoutHtml = ReadFixture("live_popout_active.html");
        string pollResponseJson = ReadFixture("get_live_chat_response.json");

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Get, "/live_chat?is_popout=1", popoutHtml)
            .When(HttpMethod.Post, "/youtubei/v1/live_chat/get_live_chat", pollResponseJson);

        using HttpClient httpClient = new(handler);
        using YTJsonParser ytJsonParser = new(new YTJsonParserOptions
        {
            HttpClient = httpClient,
            DisplayLanguage = EnumSet.DisplayLanguage.Chinese_Traditional,
        });

        List<RendererData> allMessages = [];
        int batchCount = 0;

        using CancellationTokenSource cts = new();

        await foreach (IReadOnlyList<RendererData> batch in ytJsonParser.StreamLiveChatDataAsync(
            "TEST_VIDEO_ID",
            options: new LiveChatStreamOptions { ForceIntervalMs = 0 },
            cancellationToken: cts.Token))
        {
            allMessages.AddRange(batch);
            batchCount++;

            // 第二批（輪詢批次）取得後即停止，避免無窮迴圈（fixture 的 continuation 固定不變）。
            if (batchCount >= 2)
            {
                cts.Cancel();
            }
        }

        Assert.Equal(2, batchCount);
        Assert.Contains(allMessages, m => m.ID == "msg-initial-1" && m.MessageContent == "這是初始頁面的一般留言");
        Assert.Contains(allMessages, m => m.ID == "msg-poll-1" && m.MessageContent == "一般留言測試");
        Assert.Contains(allMessages, m => m.ID == "msg-superchat-1" && m.PurchaseAmountText == "NT$100");

        // removeChatItemAction：留言刪除事件應被解析成一筆帶有目標 ID 的資料。
        Assert.Contains(allMessages, m => m.ID == "msg-poll-1" && m.Type == "留言已被刪除");

        // removeChatItemByAuthorAction：使用者封鎖事件。
        Assert.Contains(allMessages, m => m.AuthorExternalChannelID == "UCbanned" && m.Type == "使用者已被封鎖");

        // showLiveChatActionPanelAction -> pollRenderer：投票應被解析並包含問題與選項文字。
        Assert.Contains(allMessages, m => m.Type == "投票" && m.MessageContent != null &&
            m.MessageContent.Contains("測試投票問題") && m.MessageContent.Contains("選項A") && m.MessageContent.Contains("選項B"));

        // 超級留言應帶有回覆數更新事件的關聯鍵值。
        RendererData superChat = Assert.Single(allMessages, m => m.ID == "msg-superchat-1");
        Assert.Equal("reply-entity-key-1", superChat.ReplyCountEntityKey);

        // frameworkUpdates.entityBatchUpdate 內的 replyCountEntity 突變應被解析成一筆獨立的「回覆數更新」資料，
        // 其 ID 借用來存放 entityKey，讓呼叫端可以對照回上面的 ReplyCountEntityKey。
        Assert.Contains(allMessages, m =>
            m.Type == "回覆數更新" && m.ID == "reply-entity-key-1" && m.ReplyCount == "3");

        // 同一個實體更新機制底下的其它酬載類型（例如愛心按鈕狀態）應被忽略，不應該產生任何資料。
        Assert.DoesNotContain(allMessages, m => m.ID == "unrelated-entity-key");

        // updateLiveChatPollAction：投票的即時得票率更新，ID 沿用建立時的 liveChatPollId 以便對照。
        Assert.Contains(allMessages, m => m.Type == "投票結果更新" && m.ID == "poll-1" && m.MessageContent != null &&
            m.MessageContent.Contains("選項A：70%") && m.MessageContent.Contains("選項B：30%") && m.MessageContent.Contains("100 votes"));
    }

    [Fact]
    public async Task StreamLiveChatDataAsync_聊天室已停用時_不拋出例外且回傳空結果()
    {
        string disabledHtml = ReadFixture("live_popout_disabled.html");

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Get, "/live_chat?is_popout=1", disabledHtml);

        using HttpClient httpClient = new(handler);
        using YTJsonParser ytJsonParser = new(new YTJsonParserOptions { HttpClient = httpClient });

        List<RendererData> allMessages = [];

        await foreach (IReadOnlyList<RendererData> batch in ytJsonParser.StreamLiveChatDataAsync(
            "TEST_VIDEO_ID",
            cancellationToken: TestContext.Current.CancellationToken))
        {
            allMessages.AddRange(batch);
        }

        Assert.Empty(allMessages);
    }

    [Fact]
    public async Task StreamLiveChatDataAsync_斷點續傳時_使用保存權杖且在批次消費後才回報checkpoint()
    {
        string popoutHtml = ReadFixture("live_popout_active.html");
        string pollResponseJson = ReadFixture("get_live_chat_response.json");
        string? requestBody = null;

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Get, "/live_chat?is_popout=1", popoutHtml)
            .When(HttpMethod.Post, "/youtubei/v1/live_chat/get_live_chat", request =>
            {
                requestBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
                return pollResponseJson;
            });

        using HttpClient httpClient = new(handler);
        using YTJsonParser ytJsonParser = new(new YTJsonParserOptions { HttpClient = httpClient });
        using CancellationTokenSource cts = new();

        List<string> events = [];
        List<string> rawResponses = [];
        InlineProgressForTest<LiveChatStreamStatus> progress = new(status =>
            events.Add($"checkpoint:{status.Continuation}"));
        InlineProgressForTest<string> rawProgress = new(rawResponses.Add);

        int batchCount = 0;

        await foreach (IReadOnlyList<RendererData> _ in ytJsonParser.StreamLiveChatDataAsync(
            "TEST_VIDEO_ID",
            options: new LiveChatStreamOptions
            {
                ForceIntervalMs = 0,
                ResumeContinuation = "SAVED_CONTINUATION"
            },
            streamStatusProgress: progress,
            rawResponseProgress: rawProgress,
            cancellationToken: cts.Token))
        {
            events.Add($"batch:{++batchCount}");

            if (batchCount == 2)
            {
                cts.Cancel();
            }
        }

        Assert.NotNull(requestBody);
        Assert.Contains("SAVED_CONTINUATION", requestBody);
        Assert.Equal("batch:1", events[0]);
        Assert.Equal("checkpoint:SAVED_CONTINUATION", events[1]);
        Assert.Equal("batch:2", events[2]);
        Assert.StartsWith("checkpoint:", events[3]);
        Assert.Single(rawResponses);
        Assert.Contains("continuationContents", rawResponses[0]);
    }

    [Fact]
    public async Task StreamLiveChatDataAsync_popout回傳已停用假象但watch頁面有重新載入權杖時_改用get_live_chat_replay取得訊息()
    {
        // 對應真實案例：popout 聊天室頁面（/live_chat?is_popout=1）對部分重播影片（例如聊天室
        // 在直播期間曾被限制過的「初配信」）會回傳 contents.messageRenderer「已停用」的假象，
        // 但 /watch 頁面內嵌的 liveChatRenderer 其實還在，只是需要透過 reloadContinuationData
        // 重新載入、且後續必須改打 get_live_chat_replay（而非一般輪詢用的 get_live_chat）。
        string disabledHtml = ReadFixture("live_popout_disabled.html");
        string watchReloadHtml = ReadFixture("watch_page_replay_reload.html");
        string replayResponseJson = ReadFixture("get_live_chat_replay_response.json");

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Get, "/live_chat?is_popout=1", disabledHtml)
            .When(HttpMethod.Get, "/watch?v=", watchReloadHtml)
            .When(HttpMethod.Post, "/youtubei/v1/live_chat/get_live_chat_replay", replayResponseJson);

        using HttpClient httpClient = new(handler);
        using YTJsonParser ytJsonParser = new(new YTJsonParserOptions { HttpClient = httpClient });

        List<RendererData> allMessages = [];
        int batchCount = 0;

        using CancellationTokenSource cts = new();

        await foreach (IReadOnlyList<RendererData> batch in ytJsonParser.StreamLiveChatDataAsync(
            "TEST_VIDEO_ID",
            options: new LiveChatStreamOptions { ForceIntervalMs = 0 },
            cancellationToken: cts.Token))
        {
            allMessages.AddRange(batch);
            batchCount++;

            // fixture 的 continuation 固定不變，取得第一批（僅有的一批真實訊息）後即停止，避免無窮迴圈。
            if (batchCount >= 1)
            {
                cts.Cancel();
            }
        }

        Assert.Single(allMessages);
        Assert.Contains(allMessages, m => m.ID == "msg-replay-1" && m.MessageContent == "這是聊天室重播的訊息");

        // 確認真的有改打 get_live_chat_replay，而不是一般輪詢用的 get_live_chat。
        Assert.Contains(
            handler.Requests,
            r => r.RequestUri!.ToString().Contains("/youtubei/v1/live_chat/get_live_chat_replay"));
        Assert.DoesNotContain(
            handler.Requests,
            r => r.RequestUri!.ToString().Contains("/youtubei/v1/live_chat/get_live_chat?"));
    }

    [Fact]
    public async Task StreamLiveChatDataAsync_請求URL會使用is_popout統一端點()
    {
        string popoutHtml = ReadFixture("live_popout_disabled.html");

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Get, "/live_chat?is_popout=1", popoutHtml);

        using HttpClient httpClient = new(handler);
        using YTJsonParser ytJsonParser = new(new YTJsonParserOptions { HttpClient = httpClient });

        await foreach (IReadOnlyList<RendererData> _ in ytJsonParser.StreamLiveChatDataAsync(
            "TEST_VIDEO_ID",
            cancellationToken: TestContext.Current.CancellationToken))
        {
        }

        Assert.Contains(
            handler.Requests,
            r => r.RequestUri!.ToString() == "https://www.youtube.com/live_chat?is_popout=1&v=TEST_VIDEO_ID");
    }
}

file sealed class InlineProgressForTest<T>(Action<T> handler) : IProgress<T>
{
    public void Report(T value) => handler(value);
}
