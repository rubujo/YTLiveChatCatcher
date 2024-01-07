using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Sets;
using Rubujo.YouTube.Utility.Utils;
using System.Net;
using System.Runtime.Versioning;
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
    /// <returns>字串</returns>
    public string GetLatestStreamingVideoID(string channelID)
    {
        string videoID = string.Empty,
               //url = $"{StringSet.Origin}/embed/live_stream?channel={channelID}",
               // 2023/12/15 改用其它方式取得最新直播的影片。
               url = $"{StringSet.Origin}/channel/{channelID}/live";

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
                "[YTJsonParser.GetLatestStreamingVideoID()] 發生錯誤，變數 \"htmlContent\" 為空白或是 null！");

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
    /// 透過 YouTube 影片的 ID 值取得該影片的標題
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
                "[YTJsonParser.GetVideoTitle()] 發生錯誤，變數 \"htmlContent\" 為空白或是 null！");

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
    /// 檢查影片是否正在直播中
    /// </summary>
    /// <param name="videoID">字串，影片 ID</param>
    /// <returns>布林值</returns>
    public bool IsVideoStreaming(string videoID)
    {
        string url = $"{StringSet.Origin}/live_chat?v={videoID}";

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, url);

        if (!string.IsNullOrEmpty(SharedCookies))
        {
            SetHttpRequestMessageHeader(httpRequestMessage);
        }

        HttpResponseMessage? httpResponseMessage = SharedHttpClient?.SendAsync(httpRequestMessage)
            .GetAwaiter()
            .GetResult();

        string? htmlContent = httpResponseMessage?.Content.ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();

        if (htmlContent == null)
        {
            RaiseOnLogOutput(
                EnumSet.LogType.Error,
                "[YTJsonParser.CheckVideoIsStreamingOrNot()] 發生錯誤，變數 \"htmlContent\" 為 null！");

            return false;
        }

        if (httpResponseMessage?.StatusCode == HttpStatusCode.OK)
        {
            HtmlParser htmlParser = new();
            IHtmlDocument htmlDocument = htmlParser.ParseDocument(htmlContent);
            IHtmlCollection<IElement> scriptElements = htmlDocument.QuerySelectorAll("script");
            IElement targetScriptElement = scriptElements
                .FirstOrDefault(n => n.InnerHtml.Contains("window[\"ytInitialData\"] = "))!;

            string scriptContent = targetScriptElement.InnerHtml.Replace("window[\"ytInitialData\"] = ", string.Empty);

            if (scriptContent.EndsWith(';'))
            {
                scriptContent = scriptContent[0..^1];
            }

            #region 非直播中影片的範例資料

            /*
            "contents": {
                "messageRenderer": {
                    "text": {
                        "runs": [
                            {
                                "text": "這部直播影片的聊天室已停用。"
                            }
                        ]
                    },
                    "trackingParams": "CAEQljsiEwj4t4bixZCDAxUZYA8CHZS-A8s="
                }
            },
            */

            #endregion

            JsonElement jeRoot = JsonSerializer.Deserialize<JsonElement>(scriptContent);
            JsonElement? jeContents = jeRoot.Get("contents");
            JsonElement? jeMessageRenderer = jeContents?.Get("messageRenderer");

            return !jeMessageRenderer.HasValue;
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

        return false;
    }

    /// <summary>
    /// 取得 YouTube 網站的 Cookie
    /// </summary>
    /// <param name="browserType">WebBrowserUtil.BrowserType，預設值為 WebBrowserUtil.BrowserType.GoogleChrome</param>
    /// <param name="profileFolderName">字串，設定檔資料夾名稱，預設值為空白</param>
    /// <returns>字串</returns>
    [SupportedOSPlatform("windows")]
    public string GetYouTubeCookie(
        WebBrowserUtil.BrowserType browserType = WebBrowserUtil.BrowserType.GoogleChrome,
        string profileFolderName = "")
    {
        return GetCookie(
            browserType: browserType,
            profileFolderName: profileFolderName,
            hostKey: ".youtube.com");
    }

    /// <summary>
    /// 取得 Cookie
    /// </summary>
    /// <param name="browserType">WebBrowserUtil.BrowserType，預設值為 WebBrowserUtil.BrowserType.GoogleChrome</param>
    /// <param name="profileFolderName">字串，設定檔資料夾名稱，預設值為空白</param>
    /// <param name="hostKey">字串，主機鍵值，預設值為空白</param>
    /// <returns>字串</returns>
    [SupportedOSPlatform("windows")]
    public string GetCookie(
        WebBrowserUtil.BrowserType browserType = WebBrowserUtil.BrowserType.GoogleChrome,
        string profileFolderName = "",
        string? hostKey = null)
    {
        List<WebBrowserUtil.CookieData> listCookie = WebBrowserUtil
            .GetCookies(
                browserType,
                profileFolderName,
                hostKey);

        if (listCookie.Count <= 0)
        {
            RaiseOnLogOutput(
                EnumSet.LogType.Error,
                $"[YTJsonParser.GetCookie()] 主機鍵值 \"{hostKey}\" 找不到 Cookie。");
        }

        string errorMessage = WebBrowserUtil.GetErrorMessage();

        if (!string.IsNullOrEmpty(errorMessage))
        {
            RaiseOnLogOutput(
                EnumSet.LogType.Error,
                errorMessage);
        }

        return string.Join(";", listCookie.Select(n => $"{n.Name}={n.Value}"));
    }
}