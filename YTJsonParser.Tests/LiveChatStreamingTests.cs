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
