using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Models.Community;
using Xunit;

namespace Rubujo.YouTube.Utility.Tests;

public class CommunityPostStreamingTests
{
    private static string ReadFixture(string fileName) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName));

    [Fact]
    public async Task StreamCommunityPostsAsync_2026新版單一分頁結構_能正確找到社群分頁並解析貼文與附件()
    {
        // 對應這次工作階段實測發現的問題：YouTube 現在對 /community 請求只回傳單一、已選取的分頁，
        // 不再穩定提供可比對 "/community" 的 tabRenderer.endpoint.commandMetadata.webCommandMetadata.url，
        // 舊版 GetCommunityTab（僅靠網址比對）會找不到分頁，導致完全抓不到任何貼文。
        string html = ReadFixture("community_page.html");

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Get, "/community", html);

        using HttpClient httpClient = new(handler);
        using YTJsonParser ytJsonParser = new(new YTJsonParserOptions { HttpClient = httpClient });

        List<PostData> allPosts = [];

        await foreach (IReadOnlyList<PostData> batch in ytJsonParser.StreamCommunityPostsAsync(
            "TEST_CHANNEL_ID",
            options: new CommunityPostStreamOptions { FetchWholeCommunityPosts = false },
            cancellationToken: TestContext.Current.CancellationToken))
        {
            allPosts.AddRange(batch);
        }

        Assert.Equal(2, allPosts.Count);

        PostData postWithImage = Assert.Single(allPosts, p => p.PostID == "post-1");

        Assert.Contains(postWithImage.ContentTexts ?? [], t => t.Text != null && t.Text.Contains("附帶一張圖片"));
        Assert.NotNull(postWithImage.Attachments);
        Assert.Single(postWithImage.Attachments!);
        Assert.Equal("https://example.com/post1.jpg", postWithImage.Attachments![0].Url);

        PostData textOnlyPost = Assert.Single(allPosts, p => p.PostID == "post-2");

        Assert.True(textOnlyPost.Attachments == null || textOnlyPost.Attachments.Count == 0);
    }
}
