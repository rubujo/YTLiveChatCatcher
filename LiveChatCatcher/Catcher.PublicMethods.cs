using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using LiveChatCatcher.Sets;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.RegularExpressions;

namespace LiveChatCatcher;

/// <summary>
/// Catcher 的公開方法
/// </summary>
public partial class Catcher
{
    /// <summary>
    /// 取得 Task
    /// </summary>
    /// <returns>Task</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public Task? GetTask()
    {
        return SharedTask;
    }

    /// <summary>
    /// 取得 CancellationTokenSource
    /// </summary>
    /// <returns>CancellationTokenSource</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public CancellationTokenSource? GetCancellationTokenSource()
    {
        return SharedCancellationTokenSource;
    }

    /// <summary>
    /// 取得 HttpClient
    /// </summary>
    /// <returns>HttpClient</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public HttpClient? GetHttpClient()
    {
        return SharedHttpClient;
    }

    /// <summary>
    /// 取得 Cookies
    /// </summary>
    /// <returns>字串，Cookies</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public string? GetCookkise()
    {
        return SharedCookies;
    }

    /// <summary>
    /// 設定 Cookies
    /// </summary>
    /// <param name="value">字串，Cookies/param>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public void SetCookkise(string value)
    {
        SharedCookies = value;
    }

    /// <summary>
    /// 取得 IsStreaming
    /// </summary>
    /// <returns>布林值</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public bool GetIsStreaming()
    {
        return SharedIsStreaming;
    }

    /// <summary>
    /// 設定 IsStreaming
    /// </summary>
    /// <param name="value">布林值</param>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public void SetIsStreaming(bool value)
    {
        SharedIsStreaming = value;
    }

    /// <summary>
    /// 取得 FetchLargePicture
    /// </summary>
    /// <returns>布林值</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public bool GetFetchLargePicture()
    {
        return SharedFetchLargePicture;
    }

    /// <summary>
    /// 設定 FetchLargePicture
    /// </summary>
    /// <param name="value">布林值</param>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public void SetFetchLargePicture(bool value)
    {
        SharedFetchLargePicture = value;
    }

    /// <summary>
    /// 取得 TimeoutMs
    /// </summary>
    /// <returns>數值</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public int GetTimeoutMs()
    {
        return SharedTimeoutMs;
    }

    /// <summary>
    /// 設定 TimeoutMs
    /// </summary>
    /// <param name="value">數值，值</param>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public void SetTimeoutMs(int value)
    {
        SharedTimeoutMs = value;
    }

    /// <summary>
    /// 從 YouTube 頻道自定義網址取得頻道 ID
    /// </summary>
    /// <param name="url">字串，YouTube 頻道自定義網址</param>
    /// <returns>字串</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public async Task<string?> GetYtChIdByYtChCustomUrl(string url)
    {
        string? ytChId = string.Empty;

        IConfiguration configuration = Configuration.Default.WithDefaultLoader();
        IBrowsingContext browsingContext = BrowsingContext.New(configuration);
        IDocument document = await browsingContext.OpenAsync(url);
        IElement? element = document?.Body?.Children
            .FirstOrDefault(n => n.LocalName == "meta" &&
                n.GetAttribute("property") == "og:url");

        if (element != null)
        {
            ytChId = element.GetAttribute("content");
        }

        if (!string.IsNullOrEmpty(ytChId))
        {
            ytChId = ytChId.Replace("https://www.youtube.com/channel/", string.Empty);
        }

        return ytChId;
    }

    /// <summary>
    /// 透過頻道的 ID 取得該頻道最新的直播影片的影片 ID
    /// </summary>
    /// <param name="channelID">字串，頻道的 ID</param>
    /// <returns>字串</returns>
    public string GetLatestStreamingVideoID(string channelID)
    {
        string videoID = string.Empty,
               url = $"{StringSet.Origin}/embed/live_stream?channel={channelID}";

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, url);

        SetHttpRequestMessageHeader(httpRequestMessage);

        HttpResponseMessage? httpResponseMessage = SharedHttpClient?.SendAsync(httpRequestMessage)
            .GetAwaiter()
            .GetResult();

        string? htmlContent = httpResponseMessage?.Content.ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();

        if (string.IsNullOrEmpty(htmlContent))
        {
            RaiseOnLogOutput(
                EnumSet.LogType.Error,
                "[GetLatestStreamingVideoID()] 發生錯誤，變數 \"htmlContent\" 為空白或是 null！");

            return videoID;
        }

        if (httpResponseMessage?.StatusCode == HttpStatusCode.OK)
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
            string errorMessage = $"[{DateTime.Now}]（{typeof(HttpClient).Name}）：" +
                $"連線發生錯誤，錯誤碼：{httpResponseMessage?.StatusCode} " +
                $"{(httpResponseMessage != null ? $"({(int)(httpResponseMessage.StatusCode)})" : string.Empty)}{Environment.NewLine}" +
                $"接收到的內容：{Environment.NewLine}" +
                $"{htmlContent}{Environment.NewLine}";

            RaiseOnLogOutput(
                EnumSet.LogType.Error,
                errorMessage);
        }

        return videoID;
    }

    /// <summary>
    /// 透過影片的 ID 取得該影片的標題
    /// </summary>
    /// <param name="videoID">字串，影片 ID</param>
    /// <param name="cookies">字串，Cookies</param>
    /// <returns>字串</returns>
    public string GetVideoTitle(string videoID)
    {
        string videoTitle = string.Empty,
               url = $"{StringSet.Origin}/watch?v={videoID}";

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, url);

        SetHttpRequestMessageHeader(httpRequestMessage);

        HttpResponseMessage? httpResponseMessage = SharedHttpClient?.SendAsync(httpRequestMessage)
            .GetAwaiter()
            .GetResult();

        string? htmlContent = httpResponseMessage?.Content.ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();

        if (string.IsNullOrEmpty(htmlContent))
        {
            RaiseOnLogOutput(
                EnumSet.LogType.Error,
                "[GetVideoTitle()] 發生錯誤，變數 \"htmlContent\" 為空白或是 null！");

            return videoTitle;
        }

        if (httpResponseMessage?.StatusCode == HttpStatusCode.OK)
        {
            HtmlParser htmlParser = new();
            IHtmlDocument htmlDocument = htmlParser.ParseDocument(htmlContent);
            IElement titleElement = htmlDocument.QuerySelector("title")!;

            videoTitle = titleElement.InnerHtml;
        }
        else
        {
            string errorMessage = $"[{DateTime.Now}]（{typeof(HttpClient).Name}）：" +
                $"連線發生錯誤，錯誤碼：{httpResponseMessage?.StatusCode} " +
                $"{(httpResponseMessage != null ? $"({(int)(httpResponseMessage.StatusCode)})" : string.Empty)}{Environment.NewLine}" +
                $"接收到的內容：{Environment.NewLine}" +
                $"{htmlContent}{Environment.NewLine}";

            RaiseOnLogOutput(
                EnumSet.LogType.Error,
                errorMessage);
        }

        return videoTitle;
    }
}