using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Rubujo.YouTube.Utility.Sets;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using YTLiveChatCatcher.Common.Utils;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// LiveChatCatcher 的公開方法
/// </summary>
public partial class LiveChatCatcher
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
    public string? GetCookies()
    {
        return SharedCookies;
    }

    /// <summary>
    /// 取得使用 Cookies
    /// </summary>
    /// <returns>布林值</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public bool UseCookies()
    {
        return !string.IsNullOrEmpty(SharedCookies);
    }

    /// <summary>
    /// 設定使用 Cookies
    /// </summary>
    /// <param name="enable">布林值，啟用，預設值為 false</param>
    /// <param name="browserType">WebBrowserUtil.BrowserType，預設值為 WebBrowserUtil.BrowserType.GoogleChrome</param>
    /// <param name="profileFolderName">字串，設定檔資料夾名稱，預設值為空白</param>
    [SupportedOSPlatform("windows")]
    public void UseCookies(
        bool enable = false,
        WebBrowserUtil.BrowserType browserType = WebBrowserUtil.BrowserType.GoogleChrome,
        string profileFolderName = "")
    {
        if (enable)
        {
            SharedCookies = GetYouTubeCookies(browserType, profileFolderName);
        }
        else
        {
            SharedCookies = string.Empty;
        }
    }

    /// <summary>
    /// 取得是否為直播
    /// </summary>
    /// <returns>布林值</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public bool IsStreaming()
    {
        return SharedIsStreaming;
    }

    /// <summary>
    /// 設定是否為直播
    /// </summary>
    /// <param name="value">布林值</param>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public void IsStreaming(bool value)
    {
        SharedIsStreaming = value;
    }

    /// <summary>
    /// 取得是否獲取大張圖片
    /// </summary>
    /// <returns>布林值</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public bool FetchLargePicture()
    {
        return SharedIsFetchLargePicture;
    }

    /// <summary>
    /// 設定是否獲取大張圖片
    /// </summary>
    /// <param name="value">布林值</param>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public void FetchLargePicture(bool value)
    {
        SharedIsFetchLargePicture = value;
    }

    /// <summary>
    /// 取得逾時毫秒值
    /// </summary>
    /// <returns>數值</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public int TimeoutMs()
    {
        return SharedTimeoutMs;
    }

    /// <summary>
    /// 設定逾時毫秒值
    /// <para>※極其不建議將值設太低，尤其是有呼叫 LiveChatCatcher.UseCookies() 方法時。</para>
    /// </summary>
    /// <param name="value">數值，值</param>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public void TimeoutMs(int value)
    {
        SharedTimeoutMs = value;
    }

    /// <summary>
    /// 從 YouTube 頻道自定義網址取得頻道 ID 值
    /// </summary>
    /// <param name="channelUrl">字串，YouTube 頻道的網址</param>
    /// <returns>字串</returns>
    public async Task<string> GetYouTubeChannelID(string channelUrl)
    {
        string channelID = string.Empty;

        if (channelUrl.Contains($"{StringSet.Origin}/channel/"))
        {
            // 頻道網址。
            channelID = channelUrl.Replace($"{StringSet.Origin}/channel/", string.Empty);
        }
        else if (channelUrl.Contains($"{StringSet.Origin}/c/"))
        {
            // 自訂網址。
            channelID = await ParseYouTubeChannelID(channelUrl);
        }
        else if (channelUrl.Contains($"{StringSet.Origin}/user/"))
        {
            // 舊有使用者名稱網址。
            channelID = await ParseYouTubeChannelID(channelUrl);
        }
        else if (channelUrl.Contains('@'))
        {
            // 帳號代碼網址。
            channelID = await ParseYouTubeChannelID(channelUrl);
        }

        if (string.IsNullOrEmpty(channelID))
        {
            channelID = channelUrl;
        }

        return channelID;
    }

    /// <summary>
    /// 解析 YouTube 頻道的 ID 值
    /// </summary>
    /// <param name="channelUrl">字串，YouTube 頻道的網址</param>
    /// <returns>Task&lt;string&gt;</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public async Task<string> ParseYouTubeChannelID(string channelUrl)
    {
        string channelID = string.Empty;

        IConfiguration configuration = Configuration.Default.WithDefaultLoader();
        IBrowsingContext browsingContext = BrowsingContext.New(configuration);
        IDocument document = await browsingContext.OpenAsync(channelUrl);
        IElement? element = document?.Body?.Children
            .FirstOrDefault(n => n.LocalName == "meta" &&
                n.GetAttribute("property") == "og:url");

        if (element != null)
        {
            channelID = element.GetAttribute("content") ?? string.Empty;
        }

        if (!string.IsNullOrEmpty(channelID))
        {
            channelID = channelID.Replace($"{StringSet.Origin}/channel/", string.Empty);
        }

        return channelID;
    }

    /// <summary>
    /// 從 YouTube 影片的網址取得影片的 ID 值
    /// <para>來源：https://stackoverflow.com/a/15219045</para>
    /// <para>原作者：rvalvik</para>
    /// <para>原授權：CC BY-SA 3.0</para>
    /// <para>CC BY-SA 3.0：https://creativecommons.org/licenses/by-sa/3.0/</para>
    /// </summary>
    /// <param name="videoUrl">字串，影片的網址</param>
    /// <returns>字串</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public string GetYouTubeVideoID(string videoUrl)
    {
        Regex regex = RegexYouTubeUrl();

        string videoID = regex.Replace(videoUrl, string.Empty);

        if (videoID.Contains("&list="))
        {
            string[] tempArray = videoID.Split("&list=");

            videoID = tempArray[0];
        }

        if (string.IsNullOrEmpty(videoID))
        {
            videoID = videoUrl;
        }

        return videoID;
    }

    /// <summary>
    /// 透過頻道的 ID 取得該頻道最新的直播影片的影片 ID 值
    /// </summary>
    /// <param name="channelID">字串，頻道的 ID 值</param>
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
                "[LiveChatCatcher.GetLatestStreamingVideoID()] 發生錯誤，變數 \"htmlContent\" 為空白或是 null！");

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
    /// 透過影片的 ID 值取得該影片的標題
    /// </summary>
    /// <param name="videoID">字串，影片 ID 值</param>
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
                "[LiveChatCatcher.GetVideoTitle()] 發生錯誤，變數 \"htmlContent\" 為空白或是 null！");

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

    /// <summary>
    /// 取得 YouTube 頻道網址
    /// </summary>
    /// <param name="channelID">字串，頻道的 ID</param>
    /// <returns>字串</returns>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public string GetYouTubeChannelUrl(string channelID)
    {
        return $"{StringSet.Origin}/channel/{channelID}";
    }

    /// <summary>
    /// 取得 YouTube 的 Coookies
    /// </summary>
    /// <param name="browserType">WebBrowserUtil.BrowserType，預設值為 WebBrowserUtil.BrowserType.GoogleChrome</param>
    /// <param name="profileFolderName">字串，設定檔資料夾名稱，預設值為空白</param>
    /// <returns>字串</returns>
    [SupportedOSPlatform("windows")]
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public string GetYouTubeCookies(
        WebBrowserUtil.BrowserType browserType = WebBrowserUtil.BrowserType.GoogleChrome,
        string profileFolderName = "")
    {
        List<WebBrowserUtil.CookieData> cookies = WebBrowserUtil
            .GetCookies(
                browserType,
                profileFolderName,
                ".youtube.com");

        return string.Join(";", cookies.Select(n => $"{n.Name}={n.Value}"));
    }

    /// <summary>
    /// 取得特定 Host 的 Coookies
    /// </summary>
    /// <param name="browserType">WebBrowserUtil.BrowserType，預設值為 WebBrowserUtil.BrowserType.GoogleChrome</param>
    /// <param name="profileFolderName">字串，設定檔資料夾名稱，預設值為空白</param>
    /// <param name="host">字串，目標 Host 的字串值，預設值為空白</param>
    /// <returns>字串</returns>
    [SupportedOSPlatform("windows")]
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public string GetHostCookies(
        WebBrowserUtil.BrowserType browserType = WebBrowserUtil.BrowserType.GoogleChrome,
        string profileFolderName = "",
        string host = "")
    {
        List<WebBrowserUtil.CookieData> cookies = WebBrowserUtil
            .GetCookies(
                browserType,
                profileFolderName,
                host);

        return string.Join(";", cookies.Select(n => $"{n.Name}={n.Value}"));
    }
}