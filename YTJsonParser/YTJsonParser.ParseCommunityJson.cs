using System.Text.Json;
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
                });
            }
        }

        return postDatas;
    }

    /// <summary>
    /// 取得先前的社群貼文
    /// </summary>
    /// <param name="ytConfigData">YTConfigData</param>
    /// <returns>List&lt;PostData&gt;</returns>
    private List<PostData> GetEarlierPosts(YTConfigData ytConfigData)
    {
        List<PostData> postDatas = [];

        JsonElement jsonElement = GetJsonElement(
            ytConfigData: ytConfigData,
            EnumSet.DataType.Community);

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
                    IsSponsorsOnly = IsSponsorsOnly(jsonElement: backstagePostRenderer)
                });
            }
        }

        return postDatas;
    }

    /// <summary>
    /// 取得 Tabs
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement</returns>
    private JsonElement? GetTabs(JsonElement? jsonElement)
    {
        return jsonElement?.Get("contents")
            ?.Get("twoColumnBrowseResultsRenderer")
            ?.Get("tabs");
    }

    /// <summary>
    /// 取得社群的 tab
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement</returns>
    private JsonElement? GetCommunityTab(JsonElement? jsonElement)
    {
        JsonElement? tabs = GetTabs(jsonElement);

        if (tabs != null && tabs.HasValue && tabs.Value.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement tab in tabs.Value.EnumerateArray())
            {
                JsonElement? url = tab.Get("tabRenderer")
                    ?.Get("endpoint")
                    ?.Get("commandMetadata")
                    ?.Get("webCommandMetadata")
                    ?.Get("url");

                if (url != null && url.HasValue && url.Value.GetRawText().Contains("/community"))
                {
                    return tab;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 取得 tab 的 contents
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement</returns>
    private JsonElement? GetTabContents(JsonElement? jsonElement)
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
    private JsonElement? GetItemSectionRendererContents(JsonElement? jsonElement)
    {
        return jsonElement?.Get("itemSectionRenderer")
            ?.Get("contents");
    }

    /// <summary>
    /// 取得 onResponseReceivedEndpoints 的陣列
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement.ArrayEnumerator</returns>
    private JsonElement.ArrayEnumerator? GetOnResponseReceivedEndpointsArray(JsonElement? jsonElement)
    {
        return jsonElement?.Get("onResponseReceivedEndpoints")
                ?.ToArrayEnumerator();
    }

    /// <summary>
    /// 取得 appendContinuationItemsAction 的 continuationItems
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement</returns>
    private JsonElement? GetAppendContinuationItemsActionContinuationItems(JsonElement? jsonElement)
    {
        return jsonElement?.Get("appendContinuationItemsAction")
            ?.Get("continuationItems");
    }

    /// <summary>
    /// 取得 backstagePostRenderer
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement</returns>
    private JsonElement? GetBackstagePostRenderer(JsonElement? jsonElement)
    {
        return jsonElement?.Get("backstagePostThreadRenderer")
            ?.Get("post")
            ?.Get("backstagePostRenderer");
    }

    /// <summary>
    /// 判斷 backstagePostRenderer 是否為頻道會員專屬
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>布林值</returns>
    private bool IsSponsorsOnly(JsonElement? jsonElement)
    {
        JsonElement? sponsorsOnlyBadge = jsonElement?.Get("sponsorsOnlyBadge");

        return sponsorsOnlyBadge != null;
    }

    /// <summary>
    /// 取得 backstagePostRenderer 的 postId
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private string GetPostID(JsonElement? jsonElement)
    {
        JsonElement? postId = jsonElement?.Get("postId");

        if (postId != null && postId.HasValue)
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
    private string GetAuthorText(JsonElement? jsonElement)
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
    private string GetAuthorThumbnailUrl(JsonElement? jsonElement)
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
    private List<RunsData> GetContentText(JsonElement? jsonElement)
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
    /// 取得 backstagePostRenderer 的 publishedTimeText 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private string GetPublishedTimeText(JsonElement? jsonElement)
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
    private string GetVoteCount(JsonElement? jsonElement, bool simpleText = false)
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

        return attachmentDatas;
    }

    /// <summary>
    /// 取得 backstageAttachment 的 videoRenderer
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>VideoData</returns>
    private VideoData? GetVideoData(JsonElement? jsonElement)
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
    private string? GetVideoRendererVideoID(JsonElement? jsonElement)
    {
        return jsonElement?.Get("videoId")?.GetString();
    }

    /// <summary>
    /// 取得 videoRenderer 的影片網址字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private string? GetVideoRendererVideoUrl(JsonElement? jsonElement)
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
    private string? GetVideoRendererThumbnailUrl(JsonElement? jsonElement)
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
    private string? GetVideoRendererTitle(JsonElement? jsonElement)
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
    private string? GetVideoRendererDescriptionSnippet(JsonElement? jsonElement)
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
    private string? GetVideoRendererPublishedTimeText(JsonElement? jsonElement)
    {
        return jsonElement?.Get("publishedTimeText")?.Get("simpleText")?.GetString();
    }

    /// <summary>
    /// 取得 videoRenderer 的 lengthText 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="simpleText">布林值，是否取得 simpleText 的文字內容，預設值為 false</param>
    /// <returns>字串</returns>
    private string GetVideoRendererLengthText(JsonElement? jsonElement, bool simpleText = false)
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
    private string? GetVideoRendererViewCountText(JsonElement? jsonElement)
    {
        return jsonElement?.Get("viewCountText")?.Get("simpleText")?.GetString();
    }

    /// <summary>
    /// 取得 videoRenderer 的 ownerText 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private string? GetVideoRendererOwnerText(JsonElement? jsonElement)
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
    private PollData? GetPollData(JsonElement? jsonElement)
    {
        return new PollData()
        {
            ChoiceDatas = GetPollRendererChoices(jsonElement: jsonElement),
            TotalVotes = GetPollRendererTotalVotes(jsonElement: jsonElement)
        };
    }

    /// <summary>
    /// 取得 pollRenderer 的 totalVotes 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private string? GetPollRendererTotalVotes(JsonElement? jsonElement)
    {
        return jsonElement?.Get("totalVotes")?.Get("simpleText")?.GetString();
    }

    /// <summary>
    /// 取得 pollRenderer 的 choices
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>List&lt;ChoiceData&gt;</returns>
    private List<ChoiceData>? GetPollRendererChoices(JsonElement? jsonElement)
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
    private string? GetChoicesText(JsonElement? jsonElement)
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
                RunsData? RunsData = GetRuns(run);

                if (RunsData != null)
                {
                    runText += $"{RunsData.Text} ";
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
    private string? GetChoicesNumVotes(JsonElement? jsonElement)
    {
        // 要登入後才看的到 numVotes。
        return jsonElement?.Get("numVotes")?.GetString();
    }

    /// <summary>
    /// 取得 pollRenderer 的 choices 的每個項目的 votePercentage 的字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private string? GetChoicesVotePercentage(JsonElement? jsonElement)
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
    private string? GetChoicesImage(JsonElement? jsonElement)
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
    private RunsData? GetRuns(JsonElement? jsonElement)
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
    private string GetUrl(JsonElement? jsonElement)
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
    private bool IsLink(JsonElement? jsonElement)
    {
        JsonElement? navigationEndpoint = jsonElement?.Get("navigationEndpoint");

        return navigationEndpoint != null;
    }

    /// <summary>
    /// 取得 backstageImageRenderer 的 thumbnail 的網址字串
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private string GetBackstageImageRendererThumbnailUrl(JsonElement? jsonElement)
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
    private string GetThumbnailUrl(
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
    private string GetToken(JsonElement? jsonElement)
    {
        JsonElement? token = jsonElement?.Get("continuationItemRenderer")
            ?.Get("continuationEndpoint")
            ?.Get("continuationCommand")
            ?.Get("token");

        if (token != null && token.HasValue)
        {
            return token.Value.GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}