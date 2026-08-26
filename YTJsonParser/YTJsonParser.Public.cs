using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Sets;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// YTJsonParser 的公開方法
/// </summary>
public partial class YTJsonParser
{
    /// <summary>
    /// 透過 YouTube 頻道的 ID 值取得該頻道最新的直播影片的影片 ID 值
    /// </summary>
    /// <param name="channelID">字串，頻道的 ID 值</param>
    /// <param name="cancellationToken">CancellationToken，預設值為 default</param>
    /// <returns>Task&lt;string&gt;</returns>
    public async Task<string> GetLatestStreamingVideoIDAsync(string channelID, CancellationToken cancellationToken = default)
    {
        string videoID = string.Empty,
               url = $"{StringSet.Origin}/channel/{channelID}/live";

        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, url);

        SetHttpRequestMessageHeader(httpRequestMessage);

        HttpResponseMessage httpResponseMessage;

        try
        {
            httpResponseMessage = await SharedHttpClient!.SendAsync(httpRequestMessage, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogMessages.Error(_logger, nameof(GetLatestStreamingVideoIDAsync), $"發送請求失敗：{ex.GetExceptionMessage()}");

            return videoID;
        }

        using (httpResponseMessage)
        {
            string htmlContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrEmpty(htmlContent))
            {
                LogMessages.Error(_logger, nameof(GetLatestStreamingVideoIDAsync), "變數 \"htmlContent\" 為空白或是 null！");

                return videoID;
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                HtmlParser htmlParser = new();
                IHtmlDocument htmlDocument = htmlParser.ParseDocument(htmlContent);
                IHtmlCollection<IElement> linkElements = htmlDocument.QuerySelectorAll("link");

                foreach (IElement element in linkElements)
                {
                    // 取得該頁面的標準網址。
                    if (element.GetAttribute("rel") == "canonical")
                    {
                        string hrefStr = element.GetAttribute("href")!;

                        MatchCollection matches = RegexVideoID().Matches(hrefStr);

                        foreach (Match match in matches.Cast<Match>())
                        {
                            if (match.Success && match.Groups.Count >= 2)
                            {
                                // 取得 "v=" 之後的內容。
                                videoID = match.Groups[1].Captures[0].Value;
                            }
                        }
                    }
                }
            }
            else
            {
                LogMessages.HttpError(
                    _logger,
                    nameof(GetLatestStreamingVideoIDAsync),
                    httpResponseMessage.StatusCode.ToString(),
                    htmlContent);
            }

            return videoID;
        }
    }

    /// <summary>
    /// 透過 YouTube 影片的 ID 值取得該影片的標題
    /// </summary>
    /// <param name="videoID">字串，影片 ID 值</param>
    /// <param name="cancellationToken">CancellationToken，預設值為 default</param>
    /// <returns>Task&lt;string&gt;</returns>
    public async Task<string> GetVideoTitleAsync(string videoID, CancellationToken cancellationToken = default)
    {
        string videoTitle = string.Empty,
               url = $"{StringSet.Origin}/watch?v={videoID}";

        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, url);

        SetHttpRequestMessageHeader(httpRequestMessage);

        HttpResponseMessage httpResponseMessage;

        try
        {
            httpResponseMessage = await SharedHttpClient!.SendAsync(httpRequestMessage, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogMessages.Error(_logger, nameof(GetVideoTitleAsync), $"發送請求失敗：{ex.GetExceptionMessage()}");

            return videoTitle;
        }

        using (httpResponseMessage)
        {
            string htmlContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrEmpty(htmlContent))
            {
                LogMessages.Error(_logger, nameof(GetVideoTitleAsync), "變數 \"htmlContent\" 為空白或是 null！");

                return videoTitle;
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                HtmlParser htmlParser = new();
                IHtmlDocument htmlDocument = htmlParser.ParseDocument(htmlContent);
                IElement titleElement = htmlDocument.QuerySelector("title")!;

                videoTitle = titleElement.InnerHtml;
            }
            else
            {
                LogMessages.HttpError(
                    _logger,
                    nameof(GetVideoTitleAsync),
                    httpResponseMessage.StatusCode.ToString(),
                    htmlContent);
            }

            return videoTitle;
        }
    }

    /// <summary>
    /// 檢查影片是否「目前正在直播中」
    /// <para>2026/8 更新：改為解析 /watch 頁面內 ytInitialPlayerResponse 的
    /// microformat.playerMicroformatRenderer.liveBroadcastDetails.isLiveNow（並以
    /// videoDetails.isLive 為備援）。實測驗證：即使影片曾經是直播（isLiveContent 為 true），
    /// 只要直播已結束，isLiveNow 就會是 false 並且會多出 endTimestamp 欄位；
    /// 舊版做法（檢查聊天室頁面是否顯示「聊天室已停用」）無法區分「正在直播」與
    /// 「已結束但聊天室仍開放的重播」，兩者都會誤判為 true。</para>
    /// </summary>
    /// <param name="videoID">字串，影片 ID</param>
    /// <param name="cancellationToken">CancellationToken，預設值為 default</param>
    /// <returns>Task&lt;bool&gt;</returns>
    public async Task<bool> IsVideoStreamingAsync(string videoID, CancellationToken cancellationToken = default)
    {
        string url = $"{StringSet.Origin}/watch?v={videoID}";

        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, url);

        if (!string.IsNullOrEmpty(SharedCookies))
        {
            SetHttpRequestMessageHeader(httpRequestMessage);
        }

        HttpResponseMessage httpResponseMessage;

        try
        {
            httpResponseMessage = await SharedHttpClient!.SendAsync(httpRequestMessage, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogMessages.Error(_logger, nameof(IsVideoStreamingAsync), $"發送請求失敗：{ex.GetExceptionMessage()}");

            return false;
        }

        using (httpResponseMessage)
        {
            string htmlContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrEmpty(htmlContent))
            {
                LogMessages.Error(_logger, nameof(IsVideoStreamingAsync), "變數 \"htmlContent\" 為空白或是 null！");

                return false;
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                HtmlParser htmlParser = new();
                IHtmlDocument htmlDocument = htmlParser.ParseDocument(htmlContent);
                IHtmlCollection<IElement> scriptElements = htmlDocument.QuerySelectorAll("script");
                IElement? targetScriptElement = scriptElements
                    .FirstOrDefault(n => n.InnerHtml.Contains("var ytInitialPlayerResponse = "));

                if (targetScriptElement == null)
                {
                    LogMessages.Error(_logger, nameof(IsVideoStreamingAsync), "找不到 \"ytInitialPlayerResponse\"！");

                    return false;
                }

                // ytInitialPlayerResponse 內可能含有巢狀的大型字串（例如 SVG 圖示），
                // 單純裁切最後一個 ";" 並不可靠，改用括號配對找出完整且獨立的 JSON 物件。
                string scriptContent = ExtractBalancedJsonObject(targetScriptElement.InnerHtml);

                if (string.IsNullOrEmpty(scriptContent))
                {
                    LogMessages.Error(_logger, nameof(IsVideoStreamingAsync), "無法從 \"ytInitialPlayerResponse\" 取出完整的 JSON 物件！");

                    return false;
                }

                JsonElement jeRoot;

                try
                {
                    jeRoot = JsonSerializer.Deserialize<JsonElement>(scriptContent);
                }
                catch (JsonException ex)
                {
                    LogMessages.Error(_logger, nameof(IsVideoStreamingAsync), $"解析 ytInitialPlayerResponse JSON 失敗：{ex.GetExceptionMessage()}");

                    return false;
                }

                JsonElement? isLiveNow = jeRoot
                    .Get("microformat")
                    ?.Get("playerMicroformatRenderer")
                    ?.Get("liveBroadcastDetails")
                    ?.Get("isLiveNow");

                if (isLiveNow.HasValue)
                {
                    return isLiveNow.Value.GetBoolean();
                }

                // 備援：部分影片可能沒有 microformat.liveBroadcastDetails，改用 videoDetails.isLive。
                JsonElement? isLive = jeRoot.Get("videoDetails")?.Get("isLive");

                return isLive?.GetBoolean() ?? false;
            }
            else
            {
                LogMessages.HttpError(
                    _logger,
                    nameof(IsVideoStreamingAsync),
                    httpResponseMessage.StatusCode.ToString(),
                    htmlContent);
            }

            return false;
        }
    }

    /// <summary>
    /// 從文字內找出第一個完整、括號配對正確的 JSON 物件（用於從內嵌 &lt;script&gt; 內容中，
    /// 安全地取出 JSON 賦值語句右側的物件，即使物件內含有巢狀大型字串也不受影響）
    /// </summary>
    /// <param name="text">字串</param>
    /// <returns>字串，找不到時回傳空字串</returns>
    private static string ExtractBalancedJsonObject(string text)
    {
        int start = text.IndexOf('{');

        if (start == -1)
        {
            return string.Empty;
        }

        int depth = 0;
        bool inString = false;
        bool isEscaped = false;

        for (int i = start; i < text.Length; i++)
        {
            char c = text[i];

            if (inString)
            {
                if (isEscaped)
                {
                    isEscaped = false;
                }
                else if (c == '\\')
                {
                    isEscaped = true;
                }
                else if (c == '"')
                {
                    inString = false;
                }
            }
            else if (c == '"')
            {
                inString = true;
            }
            else if (c == '{')
            {
                depth++;
            }
            else if (c == '}')
            {
                depth--;

                if (depth == 0)
                {
                    return text[start..(i + 1)];
                }
            }
        }

        return string.Empty;
    }
}
