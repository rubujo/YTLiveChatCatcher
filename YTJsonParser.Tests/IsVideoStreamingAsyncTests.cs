using Rubujo.YouTube.Utility;
using Xunit;

namespace Rubujo.YouTube.Utility.Tests;

public class IsVideoStreamingAsyncTests
{
    private static string ReadFixture(string fileName) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName));

    [Theory]
    [InlineData("watch_player_response_live.html", true)]
    [InlineData("watch_player_response_ended.html", false)]
    [InlineData("watch_player_response_notlive.html", false)]
    public async Task IsVideoStreamingAsync_依liveBroadcastDetails正確判斷直播狀態(string fixtureFileName, bool expected)
    {
        string html = ReadFixture(fixtureFileName);

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Get, "/watch?v=", html);

        using HttpClient httpClient = new(handler);
        using YTJsonParser ytJsonParser = new(new YTJsonParserOptions { HttpClient = httpClient });

        bool actual = await ytJsonParser.IsVideoStreamingAsync("TEST_VIDEO_ID");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task IsVideoStreamingAsync_已結束直播的fixture含有ytInitialPlayerResponse後方額外語句_仍能正確解析()
    {
        // 對應這次工作階段實測發現的問題：ytInitialPlayerResponse 內可能有巢狀大型字串，
        // 且同一個 <script> 標籤內，後方可能還接著其他語句，單純裁切最後一個 ";" 並不可靠。
        string html = ReadFixture("watch_player_response_ended.html");

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Get, "/watch?v=", html);

        using HttpClient httpClient = new(handler);
        using YTJsonParser ytJsonParser = new(new YTJsonParserOptions { HttpClient = httpClient });

        bool actual = await ytJsonParser.IsVideoStreamingAsync("TEST_VIDEO_ID");

        Assert.False(actual);
    }
}
