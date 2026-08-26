using System.Text.Json;
using Microsoft.Extensions.Logging;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Models.Community;
using Rubujo.YouTube.Utility.Models.LiveChat;
using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// YTJsonParser 的解析社群 JSON 資料的方法
/// </summary>
public partial class YTJsonParser
{
    /// <summary>
    /// 取得初始的社群貼文
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="ytConfigData">YTConfigData</param>
    /// <returns>List&lt;PostData&gt;</returns>
    private List<PostData> GetInitialPosts(
        JsonElement? jsonElement,
        YTConfigData? ytConfigData)
    {
        List<PostData> postDatas = [];

        JsonElement? communityTab = GetCommunityTab(jsonElement: jsonElement);
        JsonElement? contents = GetTabContents(jsonElement: communityTab);
        JsonElement.ArrayEnumerator? contentsArray = contents?.ToArrayEnumerator();

        if (!contentsArray.HasValue)
        {
            return postDatas;
        }

        // 理論上只會有一筆。
        foreach (JsonElement content in contentsArray)
        {
            JsonElement? itemSectionRendererContents =
                GetItemSectionRendererContents(jsonElement: content);
            JsonElement.ArrayEnumerator? itemSectionRendererContentsArray =
                itemSectionRendererContents
                ?.ToArrayEnumerator();

            // 設定上一個梯次貼文用的 continuation。
            SetContinuation(
                arrayEnumerator: itemSectionRendererContentsArray,
                ytConfigData: ytConfigData);

            if (!itemSectionRendererContentsArray.HasValue)
            {
                continue;
            }

            // 取得並解析最新的貼文資料。
            IEnumerable<JsonElement>? backstagePostThreadRenderers =
                itemSectionRendererContentsArray
                ?.Where(n => n.Get("backstagePostThreadRenderer") != null);

            if (backstagePostThreadRenderers == null)
            {
                continue;
            }

            foreach (JsonElement? backstagePostThreadRenderer in
                backstagePostThreadRenderers.Select(v => (JsonElement?)v))
            {
                JsonElement? backstagePostRenderer = GetBackstagePostRenderer(
                    jsonElement: backstagePostThreadRenderer);

                if (backstagePostRenderer == null)
                {
                    continue;
                }

                string postId = GetPostID(jsonElement: backstagePostRenderer);
                JsonElement? sharedPostRenderer = GetSharedPostRenderer(jsonElement: backstagePostThreadRenderer);

                postDatas.Add(new PostData()
                {
                    PostID = postId,
                    Url = $"{StringSet.Origin}/post/{postId}",
                    AuthorText = GetAuthorText(jsonElement: backstagePostRenderer),
                    AuthorThumbnailUrl = GetAuthorThumbnailUrl(jsonElement: backstagePostRenderer),
                    ContentTexts = GetContentText(jsonElement: backstagePostRenderer),
                    PublishedTimeText = GetPublishedTimeText(jsonElement: backstagePostRenderer),
                    VoteCount = GetVoteCount(jsonElement: backstagePostRenderer, simpleText: false),
                    Attachments = GetBackstageAttachment(jsonElement: backstagePostRenderer),
                    IsSponsorsOnly = IsSponsorsOnly(jsonElement: backstagePostRenderer),
                    IsRepost = sharedPostRenderer.HasValue,
                    RepostedByAuthorText = GetSharedPostRepostedByAuthorText(jsonElement: sharedPostRenderer),
                    RepostCaptionTexts = GetSharedPostCaptionTexts(jsonElement: sharedPostRenderer),
                });
            }
        }

        return postDatas;
    }

    /// <summary>
    /// 取得先前的社群貼文
    /// </summary>
    /// <param name="ytConfigData">YTConfigData</param>
    /// <returns>Task&lt;List&lt;PostData&gt;&gt;</returns>
    private async Task<List<PostData>> GetEarlierPostsAsync(
        YTConfigData ytConfigData,
        CancellationToken cancellationToken = default)
    {
        List<PostData> postDatas = [];

        JsonElement jsonElement = await GetJsonElementAsync(
            ytConfigData: ytConfigData,
            EnumSet.DataType.Community,
            cancellationToken);

        JsonElement.ArrayEnumerator? onResponseReceivedEndpointsArray =
            GetOnResponseReceivedEndpointsArray(jsonElement: jsonElement);

        if (!onResponseReceivedEndpointsArray.HasValue)
        {
            return postDatas;
        }

        // 理論上只會有一筆。
        foreach (JsonElement onResponseReceivedEndpoint in onResponseReceivedEndpointsArray)
        {
            JsonElement? continuationItems = GetAppendContinuationItemsActionContinuationItems(
                    jsonElement: onResponseReceivedEndpoint);
            JsonElement.ArrayEnumerator? continuationItemsArray = continuationItems
                ?.ToArrayEnumerator();

            // 設定上一個梯次貼文用的 continuation。
            SetContinuation(
                arrayEnumerator: continuationItemsArray,
                ytConfigData: ytConfigData);

            if (!continuationItemsArray.HasValue)
            {
                continue;
            }

            // 取得並解析貼文資料。
            IEnumerable<JsonElement>? backstagePostThreadRenderers =
                continuationItemsArray
                ?.Where(n => n.Get("backstagePostThreadRenderer") != null);

            if (backstagePostThreadRenderers == null)
            {
                continue;
            }

            foreach (JsonElement? backstagePostThreadRenderer in
                backstagePostThreadRenderers.Select(v => (JsonElement?)v))
            {
                JsonElement? backstagePostRenderer = GetBackstagePostRenderer(
                    jsonElement: backstagePostThreadRenderer);

                if (backstagePostRenderer == null)
                {
                    continue;
                }

                string postId = GetPostID(jsonElement: backstagePostRenderer);
                JsonElement? sharedPostRenderer = GetSharedPostRenderer(jsonElement: backstagePostThreadRenderer);

                postDatas.Add(new PostData()
                {
                    PostID = postId,
                    Url = $"{StringSet.Origin}/post/{postId}",
                    AuthorText = GetAuthorText(jsonElement: backstagePostRenderer),
                    AuthorThumbnailUrl = GetAuthorThumbnailUrl(jsonElement: backstagePostRenderer),
                    ContentTexts = GetContentText(jsonElement: backstagePostRenderer),
                    PublishedTimeText = GetPublishedTimeText(jsonElement: backstagePostRenderer),
                    VoteCount = GetVoteCount(jsonElement: backstagePostRenderer, simpleText: false),
                    Attachments = GetBackstageAttachment(jsonElement: backstagePostRenderer),
                    IsSponsorsOnly = IsSponsorsOnly(jsonElement: backstagePostRenderer),
                    IsRepost = sharedPostRenderer.HasValue,
                    RepostedByAuthorText = GetSharedPostRepostedByAuthorText(jsonElement: sharedPostRenderer),
                    RepostCaptionTexts = GetSharedPostCaptionTexts(jsonElement: sharedPostRenderer)
                });
            }
        }

        return postDatas;
    }

    /// <summary>
    /// 設定 YTConfigData 的 Continuation
    /// </summary>
    /// <param name="arrayEnumerator">JsonElement.ArrayEnumerator</param>
    /// <param name="ytConfigData">YTConfigData</param>
    private static void SetContinuation(
        JsonElement.ArrayEnumerator? arrayEnumerator,
        YTConfigData? ytConfigData)
    {
        if (ytConfigData != null)
        {
            if (arrayEnumerator != null)
            {
                JsonElement? continuationItemRenderer = arrayEnumerator
                    ?.FirstOrDefault(n => n.Get("continuationItemRenderer") != null);

                ytConfigData.Continuation = GetToken(continuationItemRenderer);
            }
            else
            {
                ytConfigData.Continuation = null;
            }
        }
    }

    /// <summary>
    /// 取得 Tabs
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement</returns>
    private static JsonElement? GetTabs(JsonElement? jsonElement)
    {
        return jsonElement?.Get("contents")
            ?.Get("twoColumnBrowseResultsRenderer")
            ?.Get("tabs");
    }

    /// <summary>
    /// 取得社群的 tab
    /// <para>2026/8 更新：直接以 /community 網址請求時，YouTube 現在只會在 tabs 內回傳「單一、已選取」的
    /// 分頁（不再像過去一樣把所有分頁都塞進同一份回應，也不再穩定提供可比對 "/community" 的 tabRenderer.endpoint.
    /// commandMetadata.webCommandMetadata.url）。已實測驗證：無論頻道是否提供 tabIdentifier，tabs 陣列內
    /// 只會有這一個分頁，因此改以「已選取」的分頁為優先，找不到時才退回原本的網址比對（保留舊格式的相容性）。</para>
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement</returns>
    private static JsonElement? GetCommunityTab(JsonElement? jsonElement)
    {
        JsonElement? tabs = GetTabs(jsonElement);

        if (tabs == null || !tabs.HasValue || tabs.Value.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        JsonElement[] tabArray = [.. tabs.Value.EnumerateArray()];

        // 優先找「已選取」的分頁（目前實測的唯一可靠依據）。
        foreach (JsonElement tab in tabArray)
        {
            JsonElement? selected = tab.Get("tabRenderer")?.Get("selected");

            if (selected.HasValue && selected.Value.ValueKind == JsonValueKind.True)
            {
                return tab;
            }
        }

        // 退回舊格式：比對網址是否包含 "/community"。
        foreach (JsonElement tab in tabArray)
        {
            JsonElement? url = tab.Get("tabRenderer")
                ?.Get("endpoint")
                ?.Get("commandMetadata")
                ?.Get("webCommandMetadata")
                ?.Get("url");

            if (url.HasValue && url.Value.GetRawText().Contains("/community"))
            {
                return tab;
            }
        }

        // 最後手段：若整份回應僅有單一分頁，直接視為社群分頁。
        return tabArray.Length == 1 ? tabArray[0] : null;
    }

    /// <summary>
    /// 取得 tab 的 contents
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement</returns>
    private static JsonElement? GetTabContents(JsonElement? jsonElement)
    {
        return jsonElement?.Get("tabRenderer")
            ?.Get("content")
            ?.Get("sectionListRenderer")
            ?.Get("contents");
    }

    /// <summary>
    /// 取得 itemSectionRenderer 的 contents
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement</returns>
    private static JsonElement? GetItemSectionRendererContents(JsonElement? jsonElement)
    {
        return jsonElement?.Get("itemSectionRenderer")
            ?.Get("contents");
    }

    /// <summary>
    /// 取得 onResponseReceivedEndpoints 的陣列
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement.ArrayEnumerator</returns>
    private static JsonElement.ArrayEnumerator? GetOnResponseReceivedEndpointsArray(JsonElement? jsonElement)
    {
        return jsonElement?.Get("onResponseReceivedEndpoints")
                ?.ToArrayEnumerator();
    }

    /// <summary>
    /// 取得 appendContinuationItemsAction 的 continuationItems
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement</returns>
    private static JsonElement? GetAppendContinuationItemsActionContinuationItems(JsonElement? jsonElement)
    {
        return jsonElement?.Get("appendContinuationItemsAction")
            ?.Get("continuationItems");
    }

    /// <summary>
    /// 取得 backstagePostRenderer
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement</returns>
    private static JsonElement? GetBackstagePostRenderer(JsonElement? jsonElement)
    {
        JsonElement? post = jsonElement?.Get("backstagePostThreadRenderer")?.Get("post");

        JsonElement? backstagePostRenderer = post?.Get("backstagePostRenderer");

        if (backstagePostRenderer.HasValue)
        {
            return backstagePostRenderer;
        }

        // 轉發貼文（2026/8 新增）：post 底下是 sharedPostRenderer 而非 backstagePostRenderer，
        // 實際內容（作者／文字／附件）在 sharedPostRenderer.originalPost.backstagePostRenderer 裡面，
        // 跟一般貼文是同一份 Renderer 結構，因此可以沿用同一套解析邏輯。轉發本身的中繼資料
        // （轉發者、轉發時附加的文字）由 GetSharedPostRenderer 另外取得。
        return post?.Get("sharedPostRenderer")
            ?.Get("originalPost")
            ?.Get("backstagePostRenderer");
    }

    /// <summary>
    /// 取得 sharedPostRenderer（轉發貼文本身的中繼資料，不是被轉發的原始貼文內容）
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement，若不是轉發貼文則為 null</returns>
    private static JsonElement? GetSharedPostRenderer(JsonElement? jsonElement)
    {
        return jsonElement?.Get("backstagePostThreadRenderer")
            ?.Get("post")
            ?.Get("sharedPostRenderer");
    }

    /// <summary>
    /// 判斷 backstagePostRenderer 是否為頻道會員專屬
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>布林值</returns>
    private static bool IsSponsorsOnly(JsonElement? jsonElement)
    {
        JsonElement? sponsorsOnlyBadge = jsonElement?.Get("sponsorsOnlyBadge");

        return sponsorsOnlyBadge != null;
    }

    /// <summary>
    /// 取得 backstagePostRenderer 的 postId
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetPostID(JsonElement? jsonElement)
    {
        JsonElement? postId = jsonElement?.Get("postId");

        if (postId.HasValue)
        {
            return postId.Value.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    /// 取得 backstagePostRenderer 的 authorText 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetAuthorText(JsonElement? jsonElement)
    {
        JsonElement.ArrayEnumerator? runs = jsonElement?.Get("authorText")
            ?.Get("runs")
            ?.ToArrayEnumerator();

        if (runs != null)
        {
            foreach (JsonElement run in runs)
            {
                RunsData? runsData = GetRuns(jsonElement: run);

                if (runsData != null)
                {
                    return runsData.Text ?? string.Empty;
                }
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 取得 backstagePostRenderer 的 authorThumbnail 的網址字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetAuthorThumbnailUrl(JsonElement? jsonElement)
    {
        JsonElement.ArrayEnumerator? thumbnails = jsonElement?.Get("authorThumbnail")
            ?.Get("thumbnails")
            ?.ToArrayEnumerator();

        if (thumbnails != null)
        {
            // 32*32, 48*48, 76*76
            return GetThumbnailUrl(arrayEnumerator: thumbnails, width: 76);
        }

        return string.Empty;
    }

    /// <summary>
    /// 取得 backstagePostRenderer 的 contentText
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static List<RunsData> GetContentText(JsonElement? jsonElement)
    {
        List<RunsData> runsDatas = [];

        JsonElement.ArrayEnumerator? runs = jsonElement?.Get("contentText")
            ?.Get("runs")
            ?.ToArrayEnumerator();

        if (runs != null)
        {
            foreach (JsonElement run in runs)
            {
                RunsData? runsData = GetRuns(jsonElement: run);

                if (runsData != null)
                {
                    runsDatas.Add(runsData);
                }
            }
        }

        return runsDatas;
    }

    /// <summary>
    /// 取得 sharedPostRenderer 轉發時附加的文字（<c>content</c> 欄位，跟 <see cref="GetContentText"/>
    /// 解析的 <c>contentText</c> 是不同欄位名稱但同樣的 runs 結構）
    /// </summary>
    /// <param name="jsonElement">JsonElement（sharedPostRenderer）</param>
    /// <returns>List&lt;RunsData&gt;，沒有附加文字時為 null</returns>
    private static List<RunsData>? GetSharedPostCaptionTexts(JsonElement? jsonElement)
    {
        List<RunsData> runsDatas = [];

        JsonElement.ArrayEnumerator? runs = jsonElement?.Get("content")
            ?.Get("runs")
            ?.ToArrayEnumerator();

        if (runs != null)
        {
            foreach (JsonElement run in runs)
            {
                RunsData? runsData = GetRuns(jsonElement: run);

                if (runsData != null)
                {
                    runsDatas.Add(runsData);
                }
            }
        }

        return runsDatas.Count > 0 ? runsDatas : null;
    }

    /// <summary>
    /// 取得 sharedPostRenderer 內執行轉發者的名稱（<c>displayName</c> 欄位）
    /// </summary>
    /// <param name="jsonElement">JsonElement（sharedPostRenderer）</param>
    /// <returns>字串</returns>
    private static string? GetSharedPostRepostedByAuthorText(JsonElement? jsonElement)
    {
        JsonElement.ArrayEnumerator? runs = jsonElement?.Get("displayName")
            ?.Get("runs")
            ?.ToArrayEnumerator();

        return runs?.FirstOrDefault().Get("text")?.GetString();
    }

    /// <summary>
    /// 取得 backstagePostRenderer 的 publishedTimeText 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetPublishedTimeText(JsonElement? jsonElement)
    {
        JsonElement.ArrayEnumerator? runs = jsonElement?.Get("publishedTimeText")
            ?.Get("runs")
            ?.ToArrayEnumerator();

        if (runs != null)
        {
            foreach (JsonElement run in runs)
            {
                RunsData? runsData = GetRuns(jsonElement: run);

                if (runsData != null)
                {
                    return runsData.Text ?? string.Empty;
                }
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 取得 backstagePostRenderer 的 voteCount 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="simpleText">布林值，是否取得 simpleText 的文字內容，預設值為 false</param>
    /// <returns>字串</returns>
    private static string GetVoteCount(JsonElement? jsonElement, bool simpleText = false)
    {
        JsonElement? voteCount = jsonElement?.Get("voteCount");
        JsonElement? element = simpleText ?
            voteCount?.Get("simpleText") :
            voteCount?.Get("accessibility")
                ?.Get("accessibilityData")
                ?.Get("label");

        return element?.GetString() ?? string.Empty;
    }

    /// <summary>
    /// 取得 backstagePostRenderer 的 backstageAttachment
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>List&lt;Attachment&gt;</returns>
    private List<AttachmentData> GetBackstageAttachment(JsonElement? jsonElement)
    {
        List<AttachmentData> attachmentDatas = [];

        JsonElement? backstageAttachment = jsonElement?.Get("backstageAttachment");
        JsonElement? postMultiImageRenderer = backstageAttachment?.Get("postMultiImageRenderer");
        JsonElement? backstageImageRenderer = backstageAttachment?.Get("backstageImageRenderer");
        JsonElement? videoRenderer = backstageAttachment?.Get("videoRenderer");
        JsonElement? pollRenderer = backstageAttachment?.Get("pollRenderer");
        JsonElement? quizRenderer = backstageAttachment?.Get("quizRenderer");

        // 有多張圖片附件。
        if (postMultiImageRenderer != null)
        {
            JsonElement.ArrayEnumerator? images = postMultiImageRenderer?.Get("images")
                ?.ToArrayEnumerator();

            if (images != null)
            {
                foreach (JsonElement image in images)
                {
                    string url = GetBackstageImageRendererThumbnailUrl(jsonElement: image);

                    if (!string.IsNullOrEmpty(url))
                    {
                        attachmentDatas.Add(new AttachmentData()
                        {
                            Url = url
                        });
                    }
                }
            }
        }

        // 有單一圖片附件。
        if (backstageImageRenderer != null)
        {
            string url = GetBackstageImageRendererThumbnailUrl(jsonElement: backstageAttachment);

            if (!string.IsNullOrEmpty(url))
            {
                attachmentDatas.Add(new AttachmentData()
                {
                    Url = url
                });
            }
        }

        // 有影片附件。
        if (videoRenderer != null)
        {
            VideoData? videoData = GetVideoData(jsonElement: videoRenderer);

            if (videoData != null)
            {
                attachmentDatas.Add(new AttachmentData()
                {
                    Url = videoData.ThumbnailUrl,
                    IsVideo = true,
                    VideoData = videoData
                });
            }
        }

        // 有投票附件。
        if (pollRenderer != null)
        {
            PollData? pollData = GetPollData(jsonElement: pollRenderer);

            if (pollData != null)
            {
                attachmentDatas.Add(new AttachmentData()
                {
                    IsPoll = true,
                    PollData = pollData
                });
            }
        }

        // 有測驗附件（2026/8 新增，YouTube 社群貼文的測驗功能）。
        if (quizRenderer != null)
        {
            PollData? quizData = GetQuizData(jsonElement: quizRenderer);

            if (quizData != null)
            {
                attachmentDatas.Add(new AttachmentData()
                {
                    IsQuiz = true,
                    PollData = quizData
                });
            }
        }

        // 完全陌生的附件類型（例如轉發貼文）目前會被靜默忽略，這裡補上診斷用的 Trace 記錄，
        // 避免 YouTube 未來新增的附件類型在毫無記錄的情況下遺失資料。
        if (backstageAttachment.HasValue &&
            postMultiImageRenderer == null &&
            backstageImageRenderer == null &&
            videoRenderer == null &&
            pollRenderer == null &&
            quizRenderer == null &&
            _logger.IsEnabled(LogLevel.Trace))
        {
            LogMessages.Trace(_logger, "GetBackstageAttachment -> 尚未支援的附件類型", backstageAttachment.Value.GetRawText());
        }

        return attachmentDatas;
    }

    /// <summary>
    /// 取得 backstageAttachment 的 videoRenderer
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>VideoData</returns>
    private static VideoData? GetVideoData(JsonElement? jsonElement)
    {
        string? url = GetVideoRendererVideoUrl(jsonElement: jsonElement);

        if (string.IsNullOrEmpty(url))
        {
            return null;
        }

        return new VideoData()
        {
            ID = GetVideoRendererVideoID(jsonElement: jsonElement),
            Url = url,
            ThumbnailUrl = GetVideoRendererThumbnailUrl(jsonElement: jsonElement),
            Title = GetVideoRendererTitle(jsonElement: jsonElement),
            DescriptionSnippet = GetVideoRendererDescriptionSnippet(jsonElement: jsonElement),
            PublishedTimeText = GetVideoRendererPublishedTimeText(jsonElement: jsonElement),
            LengthText = GetVideoRendererLengthText(jsonElement: jsonElement, simpleText: false),
            ViewCountText = GetVideoRendererViewCountText(jsonElement: jsonElement),
            OwnerText = GetVideoRendererOwnerText(jsonElement: jsonElement)
        };
    }

    /// <summary>
    /// 取得 videoRenderer 的 videoId 字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string? GetVideoRendererVideoID(JsonElement? jsonElement)
    {
        return jsonElement?.Get("videoId")?.GetString();
    }

    /// <summary>
    /// 取得 videoRenderer 的影片網址字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string? GetVideoRendererVideoUrl(JsonElement? jsonElement)
    {
        string? url = jsonElement?.Get("navigationEndpoint")
            ?.Get("commandMetadata")
            ?.Get("webCommandMetadata")
            ?.Get("url")
            ?.GetString();

        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }

        return $"{StringSet.Origin}{url}";
    }

    /// <summary>
    /// 取得 videoRenderer 的 thumbnail 的網址字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string? GetVideoRendererThumbnailUrl(JsonElement? jsonElement)
    {
        JsonElement.ArrayEnumerator? thumbnails = jsonElement?.Get("thumbnail")
            ?.Get("thumbnails")
            ?.ToArrayEnumerator();

        return GetThumbnailUrl(arrayEnumerator: thumbnails, width: 0);
    }

    /// <summary>
    /// 取得 videoRenderer 的 title 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string? GetVideoRendererTitle(JsonElement? jsonElement)
    {
        JsonElement.ArrayEnumerator? runs = jsonElement?.Get("title")
            ?.Get("runs")
            ?.ToArrayEnumerator();

        if (runs != null)
        {
            // 理論上只會有一筆。
            foreach (JsonElement run in runs)
            {
                RunsData? runsData = GetRuns(jsonElement: run);

                if (runsData != null)
                {
                    return runsData.Text;
                }
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 取得 videoRenderer 的 descriptionSnippet 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string? GetVideoRendererDescriptionSnippet(JsonElement? jsonElement)
    {
        JsonElement.ArrayEnumerator? runs = jsonElement?.Get("descriptionSnippet")
            ?.Get("runs")
            ?.ToArrayEnumerator();

        if (runs != null)
        {
            // 理論上只會有一筆。
            foreach (JsonElement run in runs)
            {
                RunsData? runsData = GetRuns(jsonElement: run);

                if (runsData != null)
                {
                    return runsData.Text;
                }
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 取得 videoRenderer 的 publishedTimeText 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string? GetVideoRendererPublishedTimeText(JsonElement? jsonElement)
    {
        return jsonElement?.Get("publishedTimeText")?.Get("simpleText")?.GetString();
    }

    /// <summary>
    /// 取得 videoRenderer 的 lengthText 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="simpleText">布林值，是否取得 simpleText 的文字內容，預設值為 false</param>
    /// <returns>字串</returns>
    private static string GetVideoRendererLengthText(JsonElement? jsonElement, bool simpleText = false)
    {
        JsonElement? lengthText = jsonElement?.Get("lengthText");
        JsonElement? element = simpleText ?
            lengthText?.Get("simpleText") :
            lengthText?.Get("accessibility")
                ?.Get("accessibilityData")
                ?.Get("label");

        return element?.GetString() ?? string.Empty;
    }

    /// <summary>
    /// 取得 videoRenderer 的 viewCountText 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string? GetVideoRendererViewCountText(JsonElement? jsonElement)
    {
        return jsonElement?.Get("viewCountText")?.Get("simpleText")?.GetString();
    }

    /// <summary>
    /// 取得 videoRenderer 的 ownerText 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string? GetVideoRendererOwnerText(JsonElement? jsonElement)
    {
        JsonElement.ArrayEnumerator? runs = jsonElement?.Get("ownerText")
            ?.Get("runs")
            ?.ToArrayEnumerator();

        if (runs != null)
        {
            // 理論上只會有一筆。
            foreach (JsonElement run in runs)
            {
                RunsData? runsData = GetRuns(run);

                if (runsData != null)
                {
                    return runsData.Text;
                }
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 取得 backstageAttachment 的 pollRenderer
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>VideoData</returns>
    private static PollData? GetPollData(JsonElement? jsonElement)
    {
        return new PollData()
        {
            ChoiceDatas = GetPollRendererChoices(jsonElement: jsonElement),
            TotalVotes = GetPollRendererTotalVotes(jsonElement: jsonElement)
        };
    }

    /// <summary>
    /// 取得 quizRenderer 的資料（2026/8 新增，YouTube 社群貼文的測驗功能）
    /// <para>問題文字沿用貼文本身既有的 <see cref="GetContentText"/>（跟一般文字貼文同一個欄位，
    /// quizRenderer 本身不含問題文字），這裡只處理選項與正確答案。跟 <see cref="GetPollData"/>
    /// 共用 <see cref="PollData"/> 模型，是因為兩者資料形狀相同（選項文字＋總作答人數），
    /// 差別只在於 quiz 的選項多了 <see cref="ChoiceData.IsCorrect"/>、少了 numVotes／votePercentage
    /// （2026/8 實測真實貼文樣本確認：quiz 選項在作答前不會透露各選項的即時票數分布）。</para>
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>PollData</returns>
    private static PollData? GetQuizData(JsonElement? jsonElement)
    {
        return new PollData()
        {
            ChoiceDatas = GetQuizRendererChoices(jsonElement: jsonElement),
            TotalVotes = GetPollRendererTotalVotes(jsonElement: jsonElement)
        };
    }

    /// <summary>
    /// 取得 quizRenderer 的 choices
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>List&lt;ChoiceData&gt;</returns>
    private static List<ChoiceData>? GetQuizRendererChoices(JsonElement? jsonElement)
    {
        List<ChoiceData> choiceDatas = [];

        JsonElement.ArrayEnumerator? choices = jsonElement?.Get("choices")?.ToArrayEnumerator();

        if (choices != null)
        {
            foreach (JsonElement choice in choices)
            {
                string? text = GetChoicesText(jsonElement: choice);

                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                choiceDatas.Add(new ChoiceData()
                {
                    Text = text,
                    IsCorrect = choice.Get("isCorrect")?.GetBoolean()
                });
            }
        }

        return choiceDatas.Count > 0 ? choiceDatas : null;
    }

    /// <summary>
    /// 取得 pollRenderer 的 totalVotes 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string? GetPollRendererTotalVotes(JsonElement? jsonElement)
    {
        return jsonElement?.Get("totalVotes")?.Get("simpleText")?.GetString();
    }

    /// <summary>
    /// 取得 pollRenderer 的 choices
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>List&lt;ChoiceData&gt;</returns>
    private static List<ChoiceData>? GetPollRendererChoices(JsonElement? jsonElement)
    {
        List<ChoiceData> choiceDatas = [];

        JsonElement.ArrayEnumerator? choices = jsonElement?.Get("choices")?.ToArrayEnumerator();

        if (choices != null)
        {
            foreach (JsonElement choice in choices)
            {
                string? text = GetChoicesText(jsonElement: choice);

                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                choiceDatas.Add(new ChoiceData()
                {
                    Text = text,
                    ImageUrl = GetChoicesImage(jsonElement: choice),
                    NumVotes = GetChoicesNumVotes(jsonElement: choice),
                    VotePercentage = GetChoicesVotePercentage(jsonElement: choice)
                });
            }
        }

        return choiceDatas.Count > 0 ? choiceDatas : null;
    }

    /// <summary>
    /// 取得 pollRenderer 的 choices 的每個項目的 text 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string? GetChoicesText(JsonElement? jsonElement)
    {
        string? runText = string.Empty;

        JsonElement.ArrayEnumerator? runs = jsonElement?.Get("text")
            ?.Get("runs")
            ?.ToArrayEnumerator();

        if (runs != null)
        {
            // 理論上只會有一筆。
            foreach (JsonElement run in runs)
            {
                RunsData? runsData = GetRuns(run);

                if (runsData != null)
                {
                    runText += $"{runsData.Text} ";
                }
            }
        }

        return string.IsNullOrEmpty(runText) ? null : runText.TrimEnd();
    }

    /// <summary>
    /// 取得 pollRenderer 的 choices 的每個項目的 numVotes 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string? GetChoicesNumVotes(JsonElement? jsonElement)
    {
        // 要登入後才看的到 numVotes。
        return jsonElement?.Get("numVotes")?.GetString();
    }

    /// <summary>
    /// 取得 pollRenderer 的 choices 的每個項目的 votePercentage 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string? GetChoicesVotePercentage(JsonElement? jsonElement)
    {
        // 要登入後才看的到 votePercentage。
        if (jsonElement?.TryGetProperty(
                propertyName: "votePercentage",
                value: out JsonElement votePercentage) == true)
        {
            return votePercentage.Get("simpleText")?.GetString();
        }

        // 未登入只能取 votePercentageIfNotSelected。
        return jsonElement?.Get("votePercentageIfNotSelected")?.Get("simpleText")?.GetString();
    }

    /// <summary>
    /// 取得 pollRenderer 的 choices 的每個項目的 image 的 thumbnail 的網址字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string? GetChoicesImage(JsonElement? jsonElement)
    {
        JsonElement.ArrayEnumerator? thumbnails = jsonElement?.Get("image")
            ?.Get("thumbnails")
            ?.ToArrayEnumerator();

        return GetThumbnailUrl(arrayEnumerator: thumbnails, width: 0);
    }

    /// <summary>
    /// 取得 runs 的內容
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>RunsData</returns>
    private static RunsData? GetRuns(JsonElement? jsonElement)
    {
        JsonElement? text = jsonElement?.Get("text");

        string value = text?.GetString() ?? string.Empty;

        // 針對被縮略的網址進行替換。
        if (value.StartsWith("http") && value.EndsWith("..."))
        {
            value = GetUrl(jsonElement);
        }

        return text == null ?
            null :
            new RunsData()
            {
                Text = value,
                Url = GetUrl(jsonElement),
                IsLink = IsLink(jsonElement)
            };
    }

    /// <summary>
    /// 取得 run 的網址字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetUrl(JsonElement? jsonElement)
    {
        JsonElement? url = jsonElement?.Get("navigationEndpoint")
            ?.Get("commandMetadata")
            ?.Get("webCommandMetadata")
            ?.Get("url");

        if (url != null)
        {
            string? value = url.Value.GetString();

            if (!string.IsNullOrEmpty(value) && value.StartsWith('/'))
            {
                value = $"{StringSet.Origin}{value}";
            }

            return value ?? string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    /// 判斷 run 是否為網址
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>布林值</returns>
    private static bool IsLink(JsonElement? jsonElement)
    {
        JsonElement? navigationEndpoint = jsonElement?.Get("navigationEndpoint");

        return navigationEndpoint != null;
    }

    /// <summary>
    /// 取得 backstageImageRenderer 的 thumbnail 的網址字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetBackstageImageRendererThumbnailUrl(JsonElement? jsonElement)
    {
        JsonElement.ArrayEnumerator? thumbnails = jsonElement?.Get("backstageImageRenderer")
            ?.Get("image")
            ?.Get("thumbnails")
            ?.ToArrayEnumerator();

        return GetThumbnailUrl(arrayEnumerator: thumbnails, width: 0);
    }

    /// <summary>
    /// 取得 thumbnail 的網址字串
    /// </summary>
    /// <param name="arrayEnumerator">JsonElement.ArrayEnumerator</param>
    /// <param name="width">數值，寬度，預設值為 0</param>
    /// <returns>字串</returns>
    private static string GetThumbnailUrl(
        JsonElement.ArrayEnumerator? arrayEnumerator,
        int width = 0)
    {
        string value = string.Empty;

        // 當為 width 為 0 時，自動取最後一個項目，通常為最大張圖。
        JsonElement? thumbnail = width == 0 || width == -1 ?
            arrayEnumerator?.LastOrDefault() :
            arrayEnumerator?.FirstOrDefault(n => n.Get("width") != null &&
                n.Get("width")?.GetInt32() == width);

        JsonElement? url = thumbnail?.Get("url");

        if (url != null)
        {
            value = url.Value.GetString() ?? string.Empty;
        }

        if (value.StartsWith("//"))
        {
            value = $"https:{value}";
        }

        // 用以取得完整未裁切的圖片。
        if (value.Contains("-c-fcrop64="))
        {
            string[] tempArray = value.Split("-c-fcrop64=");

            value = tempArray[0];
        }

        // 將 "=s***" 替換成 "=s0" 可以取得原圖。
        if (width == -1)
        {
            if (value.Contains("=s"))
            {
                string[] tempArray = value.Split("=s");

                value = tempArray[0] + "=s0";
            }
        }

        return value;
    }

    /// <summary>
    /// 取得 continuationItemRenderer 的 token 字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetToken(JsonElement? jsonElement)
    {
        JsonElement? token = jsonElement?.Get("continuationItemRenderer")
            ?.Get("continuationEndpoint")
            ?.Get("continuationCommand")
            ?.Get("token");

        if (token.HasValue)
        {
            return token.Value.GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}