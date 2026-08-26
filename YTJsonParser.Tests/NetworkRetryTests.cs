using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Models.LiveChat;
using Xunit;

namespace Rubujo.YouTube.Utility.Tests;

/// <summary>
/// 驗證 GetJsonElementAsync（YTJsonParser.Core.cs）對暫時性失敗的重試行為，
/// 對應這次新增的「非 429 網路例外也要重試」修正，避免短暫網路不穩讓長時間擷取直接中止。
/// </summary>
public class NetworkRetryTests
{
    private static string ReadFixture(string fileName) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName));

    private static HttpResponseMessage JsonResponse(string content) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(content, Encoding.UTF8, "application/json")
    };

    [Fact]
    public async Task StreamLiveChatDataAsync_輪詢時遇到一次網路例外_會重試並拿到後續資料()
    {
        string popoutHtml = ReadFixture("live_popout_active.html");
        string pollResponseJson = ReadFixture("get_live_chat_response.json");

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Get, "/live_chat?is_popout=1", popoutHtml)
            .WhenSequence(
                "/youtubei/v1/live_chat/get_live_chat",
                // 第一次呼叫：模擬暫時性網路例外（例如 Wi-Fi 瞬斷）。
                () => throw new HttpRequestException("模擬的暫時性網路例外"),
                // 第二次呼叫（重試後）：正常回應。
                () => JsonResponse(pollResponseJson));

        using HttpClient httpClient = new(handler);
        using YTJsonParser ytJsonParser = new(new YTJsonParserOptions { HttpClient = httpClient });

        List<RendererData> allMessages = [];
        int batchCount = 0;

        using CancellationTokenSource cts = new();
        // 網路例外重試的第一次延遲是 5 秒（見 YTJsonParser.Core.cs 的 Math.Min(5 * attempt, 20)），
        // 這裡給足夠寬裕的逾時，避免在較慢的 CI 環境誤判成掛住。
        cts.CancelAfter(TimeSpan.FromSeconds(30));

        await foreach (IReadOnlyList<RendererData> batch in ytJsonParser.StreamLiveChatDataAsync(
            "TEST_VIDEO_ID",
            options: new LiveChatStreamOptions { ForceIntervalMs = 0 },
            cancellationToken: cts.Token))
        {
            allMessages.AddRange(batch);
            batchCount++;

            if (batchCount >= 2)
            {
                await cts.CancelAsync();
            }
        }

        // 第一批來自初始頁面（不受輪詢重試影響），第二批是重試後的輪詢回應——
        // 代表網路例外沒有讓串流直接放棄，而是重試後成功拿到資料。
        Assert.Equal(2, batchCount);
        Assert.Contains(allMessages, m => m.ID == "msg-poll-1" && m.MessageContent == "一般留言測試");
    }

    [Fact]
    public async Task StreamLiveChatDataAsync_輪詢時遇到HTTP429_會依RetryAfter等待後重試()
    {
        string popoutHtml = ReadFixture("live_popout_active.html");
        string pollResponseJson = ReadFixture("get_live_chat_response.json");

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Get, "/live_chat?is_popout=1", popoutHtml)
            .WhenSequence(
                "/youtubei/v1/live_chat/get_live_chat",
                () =>
                {
                    HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);
                    response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(1));

                    return response;
                },
                () => JsonResponse(pollResponseJson));

        using HttpClient httpClient = new(handler);
        using YTJsonParser ytJsonParser = new(new YTJsonParserOptions { HttpClient = httpClient });

        List<RendererData> allMessages = [];
        int batchCount = 0;

        using CancellationTokenSource cts = new();
        cts.CancelAfter(TimeSpan.FromSeconds(15));

        await foreach (IReadOnlyList<RendererData> batch in ytJsonParser.StreamLiveChatDataAsync(
            "TEST_VIDEO_ID",
            options: new LiveChatStreamOptions { ForceIntervalMs = 0 },
            cancellationToken: cts.Token))
        {
            allMessages.AddRange(batch);
            batchCount++;

            if (batchCount >= 2)
            {
                await cts.CancelAsync();
            }
        }

        Assert.Equal(2, batchCount);
        Assert.Contains(allMessages, m => m.ID == "msg-poll-1" && m.MessageContent == "一般留言測試");
    }
}
