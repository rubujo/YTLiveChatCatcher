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
        // 對應這次工作階段實測發現的問題：YouTube 現在對社群分頁只回傳單一、已選取的分頁，
        // 不再穩定提供可比對舊網址格式的 tabRenderer.endpoint.commandMetadata.webCommandMetadata.url，
        // 舊版 GetCommunityTab（僅靠網址比對）會找不到分頁，導致完全抓不到任何貼文。
        // 另外，2026/8 也修正了實際請求的網址：YouTube 已將分頁網址由 /community 更名為 /posts，
        // 部分頻道（尤其訂閱數大、頻道存在已久的頻道）直接請求 /community 會回傳空狀態，改用 /posts 才正常。
        string html = ReadFixture("community_page.html");

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Get, "/posts", html);

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

        Assert.Equal(4, allPosts.Count);

        PostData postWithImage = Assert.Single(allPosts, p => p.PostID == "post-1");

        Assert.Contains(postWithImage.ContentTexts ?? [], t => t.Text != null && t.Text.Contains("附帶一張圖片"));
        Assert.NotNull(postWithImage.Attachments);
        Assert.Single(postWithImage.Attachments!);
        Assert.Equal("https://example.com/post1.jpg", postWithImage.Attachments![0].Url);

        PostData textOnlyPost = Assert.Single(allPosts, p => p.PostID == "post-2");

        Assert.True(textOnlyPost.Attachments == null || textOnlyPost.Attachments.Count == 0);

        // quizRenderer（2026/8 新增，YouTube 社群貼文的測驗功能）：問題文字沿用一般貼文的 ContentTexts，
        // 附件本身只有選項與正確答案／總作答人數。
        PostData quizPost = Assert.Single(allPosts, p => p.PostID == "post-3");

        Assert.NotNull(quizPost.Attachments);
        AttachmentData quizAttachment = Assert.Single(quizPost.Attachments!);
        Assert.True(quizAttachment.IsQuiz);
        Assert.NotNull(quizAttachment.PollData);
        Assert.Equal("999 人已回答", quizAttachment.PollData!.TotalVotes);
        Assert.Equal(2, quizAttachment.PollData.ChoiceDatas?.Count);
        Assert.Contains(quizAttachment.PollData.ChoiceDatas!, c => c.Text == "選項A" && c.IsCorrect == false);
        Assert.Contains(quizAttachment.PollData.ChoiceDatas!, c => c.Text == "選項B" && c.IsCorrect == true);

        // sharedPostRenderer（2026/8 新增，YouTube 社群貼文的「在 YouTube 上轉發」功能）：post 底下是
        // sharedPostRenderer 而非 backstagePostRenderer，實際內容（作者／文字／附件）取自巢狀的
        // originalPost.backstagePostRenderer，轉發本身的中繼資料（轉發者、附加文字）另外解析。
        PostData repostPost = Assert.Single(allPosts, p => p.PostID == "post-4");

        Assert.True(repostPost.IsRepost);
        Assert.Equal("轉發的頻道", repostPost.RepostedByAuthorText);
        Assert.Contains(repostPost.RepostCaptionTexts ?? [], t => t.Text == "轉發時附加的說明文字");
        Assert.Equal("原始作者頻道", repostPost.AuthorText);
        Assert.Contains(repostPost.ContentTexts ?? [], t => t.Text != null && t.Text.Contains("被轉發的原始貼文內容"));
        Assert.NotNull(repostPost.Attachments);
        Assert.Single(repostPost.Attachments!);
        Assert.Equal("https://example.com/post4.jpg", repostPost.Attachments![0].Url);
    }

    [Fact]
    public async Task StreamCommunityPostsAsync_續傳請求回應無法解析時應清空Continuation並結束串流_不能無限重試()
    {
        // 對應實測發現的卡死問題：GetEarlierPostsAsync 曾經在「onResponseReceivedEndpoints 這個欄位
        // 不存在」（不論是因為 GetJsonElementAsync 重試耗盡後放棄，還是回應結構不符預期）時，直接
        // return 而完全不呼叫 SetContinuation，導致 ytConfigData.Continuation 停留在舊值，
        // StreamCommunityPostsAsync 外層 while 迴圈的結束條件永遠不成立，變成每隔
        // MinimumIntervalMs（1 秒）就重送同一個已經沒用的 continuation token，永遠不會結束、也不會報錯，
        // 使用者只會看到匯出進度卡住不動。這裡模擬「續傳請求回應缺少 onResponseReceivedEndpoints」
        // （用一個空物件 "{}" 代表），驗證修正後的行為：只送出一次續傳請求就正常結束列舉，不會無限重試。
        string html = ReadFixture("community_page_with_continuation.html");

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Get, "/posts", html)
            .When(HttpMethod.Post, "/browse", "{}");

        using HttpClient httpClient = new(handler);
        using YTJsonParser ytJsonParser = new(new YTJsonParserOptions { HttpClient = httpClient });

        // 用一個較短的逾時保護測試本身：若修正失效（迴圈真的無限重試下去），
        // 這裡會在逾時後強制取消，讓測試明確失敗（逾時／取消），而不是整個測試執行緒被卡死。
        using CancellationTokenSource timeoutCts = new(TimeSpan.FromSeconds(10));
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            timeoutCts.Token,
            TestContext.Current.CancellationToken);

        List<PostData> allPosts = [];

        await foreach (IReadOnlyList<PostData> batch in ytJsonParser.StreamCommunityPostsAsync(
            "TEST_CHANNEL_ID",
            options: new CommunityPostStreamOptions { FetchWholeCommunityPosts = true },
            cancellationToken: linkedCts.Token))
        {
            allPosts.AddRange(batch);
        }

        Assert.False(timeoutCts.IsCancellationRequested, "串流在逾時內沒有自然結束，代表 Continuation 沒有被正確清空，續傳迴圈仍在無限重試。");

        PostData post = Assert.Single(allPosts);

        Assert.Equal("post-1", post.PostID);

        int browseRequestCount = handler.Requests.Count(r => (r.RequestUri?.ToString() ?? string.Empty).Contains("/browse"));

        Assert.Equal(1, browseRequestCount);
    }
}
