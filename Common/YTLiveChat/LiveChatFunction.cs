using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using NLog;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using YTLiveChatCatcher.Extensions;
using YTLiveChatCatcher.Models;

namespace YTLiveChatCatcher.Common.YTLiveChat;

/// <summary>
/// YouTube 聊天室函式
/// </summary>
public class LiveChatFunction
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 透過頻道的 ID 取得該頻道最新的直播影片的影片 ID
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="channelID">字串，頻道的 ID</param>
    /// <param name="cookies">字串，Cookies</param>
    /// <param name="control">TextBox</param>
    /// <returns>字串</returns>
    public static string GetLatestStreamingVideoID(
        HttpClient httpClient,
        string channelID,
        string cookies,
        TextBox control)
    {
        string videoID = string.Empty,
               url = $"{StringSet.Origin}/embed/live_stream?channel={channelID}";

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, url);

        if (!string.IsNullOrEmpty(cookies))
        {
            SetHttpRequestMessageHeader(httpRequestMessage, cookies);
        }

        HttpResponseMessage httpResponseMessage = httpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();

        string htmlContent = httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

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

                    MatchCollection matches = Regex.Matches(hrefStr, "v=(.+)");

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
            control.InvokeIfRequired(() =>
            {
                string errorMessage = $"[{DateTime.Now}]（{typeof(HttpClient).Name}）：" +
                    $"連線發生錯誤，錯誤碼：{httpResponseMessage.StatusCode} " +
                    $"({(int)httpResponseMessage.StatusCode}){Environment.NewLine}" +
                    $"接收到的內容：{Environment.NewLine}" +
                    $"{htmlContent}{Environment.NewLine}";

                control.AppendText(errorMessage);
            });
        }

        return videoID;
    }

    /// <summary>
    /// 透過影片的 ID 取得該影片的標題
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="videoID">字串，影片 ID</param>
    /// <param name="cookies">字串，Cookies</param>
    /// <param name="control">TextBox</param>
    /// <returns>字串</returns>
    public static string GetVideoTitle(
        HttpClient httpClient,
        string videoID,
        string cookies,
        TextBox control)
    {
        string output = string.Empty,
               url = $"{StringSet.Origin}/watch?v={videoID}";

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, url);

        if (!string.IsNullOrEmpty(cookies))
        {
            SetHttpRequestMessageHeader(httpRequestMessage, cookies);
        }

        HttpResponseMessage httpResponseMessage = httpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();

        string htmlContent = httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
        {
            HtmlParser htmlParser = new();
            IHtmlDocument htmlDocument = htmlParser.ParseDocument(htmlContent);
            IElement titleElement = htmlDocument.QuerySelector("title")!;

            output = titleElement.InnerHtml;
        }
        else
        {
            control.InvokeIfRequired(() =>
            {
                string errorMessage = $"[{DateTime.Now}]（{typeof(HttpClient).Name}）：" +
                    $"連線發生錯誤，錯誤碼：{httpResponseMessage.StatusCode} " +
                    $"({(int)httpResponseMessage.StatusCode}){Environment.NewLine}" +
                    $"接收到的內容：{Environment.NewLine}" +
                    $"{htmlContent}{Environment.NewLine}";

                control.AppendText(errorMessage);
            });
        }

        return output;
    }

    /// <summary>
    /// 取得 YTConfig
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="videoID">字串，影片 ID</param>
    /// <param name="isStreaming">布林值，用於判斷是否為直播中的影片</param>
    /// <param name="cookies">字串，Cookies</param>
    /// <param name="control">TextBox</param>
    /// <returns>YTConfig</returns>
    public static YTConfig GetYTConfig(
        HttpClient httpClient,
        string videoID,
        bool isStreaming,
        string cookies,
        TextBox control)
    {
        YTConfig ytConfig = new();

        string url = isStreaming ? $"{StringSet.Origin}/live_chat?v={videoID}" :
            $"{StringSet.Origin}/watch?v={videoID}";

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, url);

        if (!string.IsNullOrEmpty(cookies))
        {
            SetHttpRequestMessageHeader(httpRequestMessage, cookies);
        }

        HttpResponseMessage httpResponseMessage = httpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();

        string htmlContent = httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
        {
            MatchCollection collection1 = Regex.Matches(htmlContent, "INNERTUBE_API_KEY\":\"(.+?)\",");

            foreach (Match match in collection1.Cast<Match>())
            {
                if (match.Success && match.Groups.Count >= 2)
                {
                    ytConfig.APIKey = match.Groups[1].Captures[0].Value;
                }
            }

            MatchCollection collection2 = Regex.Matches(htmlContent, "continuation\":\"(.+?)\",");

            foreach (Match match in collection2.Cast<Match>())
            {
                if (match.Success && match.Groups.Count >= 2)
                {
                    ytConfig.Continuation = match.Groups[1].Captures[0].Value;

                    break;
                }
            }

            MatchCollection collection3 = Regex.Matches(htmlContent, "visitorData\":\"(.+?)\",");

            foreach (Match match in collection3.Cast<Match>())
            {
                if (match.Success && match.Groups.Count >= 2)
                {
                    ytConfig.VisitorData = match.Groups[1].Captures[0].Value;

                    break;
                }
            }

            MatchCollection collection4 = Regex.Matches(htmlContent, "clientName\":\"(.+?)\",");

            foreach (Match match in collection4.Cast<Match>())
            {
                if (match.Success && match.Groups.Count >= 2)
                {
                    string clientName = match.Groups[1].Captures[0].Value;

                    if (clientName == "WEB")
                    {
                        ytConfig.ClientName = clientName;

                        break;
                    }
                }
            }

            MatchCollection collection5 = Regex.Matches(htmlContent, "clientVersion\":\"(.+?)\",");

            foreach (Match match in collection5.Cast<Match>())
            {
                if (match.Success && match.Groups.Count >= 2)
                {
                    ytConfig.ClientVersion = match.Groups[1].Captures[0].Value;

                    break;
                }
            }

            MatchCollection collection6 = Regex.Matches(htmlContent, "ID_TOKEN\"(.+?)\",");

            foreach (Match match in collection6.Cast<Match>())
            {
                if (match.Success && match.Groups.Count >= 2)
                {
                    ytConfig.ID_TOKEN = match.Groups[1].Captures[0].Value;

                    break;
                }
            }

            MatchCollection collection7 = Regex.Matches(htmlContent, "SESSION_INDEX\":\"(.*?)\"");

            foreach (Match match in collection7.Cast<Match>())
            {
                if (match.Success && match.Groups.Count >= 2)
                {
                    ytConfig.SESSION_INDEX = match.Groups[1].Captures[0].Value;

                    break;
                }
            }

            MatchCollection collection8 = Regex.Matches(htmlContent, "INNERTUBE_CONTEXT_CLIENT_NAME\":(.*?),");

            foreach (Match match in collection8.Cast<Match>())
            {
                if (match.Success && match.Groups.Count >= 2)
                {
                    ytConfig.INNERTUBE_CONTEXT_CLIENT_NAME = match.Groups[1].Captures[0].Value;

                    break;
                }
            }

            MatchCollection collection9 = Regex.Matches(htmlContent, "INNERTUBE_CONTEXT_CLIENT_VERSION\":\"(.*?)\"");

            foreach (Match match in collection9.Cast<Match>())
            {
                if (match.Success && match.Groups.Count >= 2)
                {
                    ytConfig.INNERTUBE_CONTEXT_CLIENT_VERSION = match.Groups[1].Captures[0].Value;

                    break;
                }
            }

            MatchCollection collection10 = Regex.Matches(htmlContent, "INNERTUBE_CLIENT_VERSION\":\"(.*?)\"");

            foreach (Match match in collection10.Cast<Match>())
            {
                if (match.Success && match.Groups.Count >= 2)
                {
                    ytConfig.INNERTUBE_CLIENT_VERSION = match.Groups[1].Captures[0].Value;

                    break;
                }
            }

            MatchCollection collection11 = Regex.Matches(htmlContent, "DATASYNC_ID\":\"(.*?)\"");

            foreach (Match match in collection11.Cast<Match>())
            {
                if (match.Success && match.Groups.Count >= 2)
                {
                    string dataSyncID = match.Groups[1].Captures[0].Value;

                    string[] tempArray = dataSyncID
                        .Split("||".ToCharArray(),
                        StringSplitOptions.RemoveEmptyEntries);

                    if (tempArray.Length > 0)
                    {
                        ytConfig.DATASYNC_ID = tempArray[0];
                    }

                    break;
                }
            }

            // TODO: 2022-05-19 尚未確認到是否有此參數。
            // 參考：https://github.com/xenova/chat-downloader/blob/ff9ddb1f840fa06d0cc3976badf75c1fffebd003/chat_downloader/sites/youtube.py#L1537
            MatchCollection collection12 = Regex.Matches(htmlContent, "DELEGATED_SESSION_ID\":\"(.*?)\"");

            foreach (Match match in collection12.Cast<Match>())
            {
                if (match.Success && match.Groups.Count >= 2)
                {
                    ytConfig.DELEGATED_SESSION_ID = match.Groups[1].Captures[0].Value;

                    break;
                }
            }

            if (!isStreaming)
            {
                HtmlParser htmlParser = new();
                IHtmlDocument htmlDocument = htmlParser.ParseDocument(htmlContent);
                IHtmlCollection<IElement> scriptElements = htmlDocument.QuerySelectorAll("script");
                IElement scriptElement = scriptElements
                    .FirstOrDefault(n => n.InnerHtml.Contains("var ytInitialData ="))!;

                string json = scriptElement.InnerHtml
                    .Replace("var ytInitialData = ", "");

                if (json.EndsWith(";"))
                {
                    json = json[0..^1];
                }

                JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

                ytConfig.Continuation = JsonParser.GetReplayContinuation(jsonElement);
            }
        }
        else
        {
            control.InvokeIfRequired(() =>
            {
                string errorMessage = $"[{DateTime.Now}]（{typeof(HttpClient).Name}）：" +
                    $"連線發生錯誤，錯誤碼：{httpResponseMessage.StatusCode} " +
                    $"({(int)httpResponseMessage.StatusCode}){Environment.NewLine}" +
                    $"接收到的內容：{Environment.NewLine}" +
                    $"{htmlContent}{Environment.NewLine}";

                control.AppendText(errorMessage);
            });
        }

        return ytConfig;
    }

    /// <summary>
    /// 取得 JsonElement
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="ytConfig">YTConfig</param>
    /// <param name="isStreaming">布林值，用於判斷是否為直播中的影片</param>
    /// <param name="cookies">字串，Cookies</param>
    /// <param name="control">TextBox</param>
    /// <returns>JsonElement</returns>
    public static JsonElement GetJsonElement(
        HttpClient httpClient,
        YTConfig ytConfig,
        bool isStreaming,
        string cookies,
        TextBox control)
    {
        JsonElement outputJsonElement = new();

        string methodName = isStreaming ? "get_live_chat" : "get_live_chat_replay";
        string url = $"{StringSet.Origin}/youtubei/v1/live_chat/{methodName}?key={ytConfig.APIKey}";

        // 當 ytConfig.Continuation 為 null 或空值時，則表示已經抓取完成。
        if (!string.IsNullOrEmpty(ytConfig.Continuation))
        {
            // 當沒有時才指定，後續不更新。
            if (string.IsNullOrEmpty(ytConfig.InitPage))
            {
                string apiType = methodName.Replace("get_", string.Empty);

                ytConfig.InitPage = $"{StringSet.Origin}/{apiType}/?continuation={ytConfig.Continuation}";
            }

            string inputJsonContent = GetRequestPayload(ytConfig, httpClient.DefaultRequestHeaders.UserAgent.ToString());

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, url);

            if (!string.IsNullOrEmpty(cookies))
            {
                SetHttpRequestMessageHeader(httpRequestMessage, cookies, ytConfig);
            }

            HttpContent content = new StringContent(inputJsonContent, Encoding.UTF8, "application/jsonElement");

            httpRequestMessage.Content = content;

            HttpResponseMessage httpResponseMessage = httpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();

            string receivedJsonContent = httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (Properties.Settings.Default.EnableDebug)
            {
                _logger.Debug(httpRequestMessage);
                _logger.Debug(inputJsonContent);
                _logger.Debug(httpResponseMessage);
                _logger.Debug(receivedJsonContent);
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                outputJsonElement = JsonSerializer.Deserialize<JsonElement>(receivedJsonContent);
            }
            else
            {
                control.InvokeIfRequired(() =>
                {
                    string errorMessage = $"[{DateTime.Now}]（{typeof(HttpClient).Name}）：" +
                        $"連線發生錯誤，錯誤碼：{httpResponseMessage.StatusCode} " +
                        $"({(int)httpResponseMessage.StatusCode}){Environment.NewLine}" +
                        $"接收到的內容：{Environment.NewLine}" +
                        $"{receivedJsonContent}{Environment.NewLine}";

                    control.AppendText(errorMessage);
                });
            }
        }

        return outputJsonElement;
    }

    /// <summary>
    /// 取得要求裝載
    /// </summary>
    /// <param name="ytConfig">YTConfig</param>
    /// <param name="userAgent">字串，使用者代理字串</param>
    /// <returns>字串</returns>
    private static string GetRequestPayload(
        YTConfig ytConfig,
        string userAgent)
    {
        // 2022-05-19 尚未測試會員限定直播是否需要 "clickTracking" 參數。
        // 2022-05-29 已確認不需要 "clickTracking" 參數。
        // 參考：https://github.com/xenova/chat-downloader/blob/ff9ddb1f840fa06d0cc3976badf75c1fffebd003/chat_downloader/sites/youtube.py#L1664
        // 參考：https://github.com/abhinavxd/youtube-live-chat-downloader/blob/main/yt_chat.go

        // 內容是精簡過的。
        return JsonSerializer.Serialize(new RequestPayload()
        {
            Context = new()
            {
                Client = new()
                {
                    // 語系會影響取得的內容，強制使用 zh-TW, TW。
                    Hl = "zh-TW",
                    Gl = "TW",
                    DeviceMake = string.Empty,
                    DeviceModel = string.Empty,
                    VisitorData = ytConfig.VisitorData,
                    UserAgent = userAgent,
                    ClientName = ytConfig.ClientName,
                    ClientVersion = ytConfig.ClientVersion,
                    OsName = "Windows",
                    OsVersion = "10.0",
                    Platform = "DESKTOP",
                    ClientFormFactor = "UNKNOWN_FORM_FACTOR",
                    TimeZone = "Asia/Taipei"
                }
            },
            Continuation = ytConfig.Continuation
        });
    }

    /// <summary>
    /// 設定 HttpRequestMessage 的標頭
    /// <para>參考：https://stackoverflow.com/questions/12373738/how-do-i-set-a-cookie-on-httpclients-httprequestmessage/13287224#13287224 </para>
    /// </summary>
    /// <param name="request">HttpRequestMessage</param>
    /// <param name="cookies">字串，Cookies</param>
    /// <param name="ytConfig">YTConfig</param>
    private static void SetHttpRequestMessageHeader(
        HttpRequestMessage request,
        string cookies,
        YTConfig? ytConfig = null)
    {
        request.Headers.Add("Cookie", cookies);

        string[] cookieSet = cookies.Split(
            new char[] { ';' },
            StringSplitOptions.RemoveEmptyEntries);

        string? sapiSid = cookieSet.FirstOrDefault(n => n.Contains("SAPISID"));

        if (!string.IsNullOrEmpty(sapiSid))
        {
            string[] tempArray = sapiSid.Split(
                new char[] { '=' },
                StringSplitOptions.RemoveEmptyEntries);

            if (tempArray.Length == 2)
            {
                request.Headers.Add(
                    "authorization",
                    $"SAPISIDHASH {GetSapiSidHash(tempArray[1], StringSet.Origin)}");
            }
        }

        if (ytConfig != null)
        {
            string xGoogAuthuser = "0",
                xGoogPageId = string.Empty;

            if (!string.IsNullOrEmpty(ytConfig.DATASYNC_ID))
            {
                xGoogPageId = ytConfig.DATASYNC_ID;
            }

            if (string.IsNullOrEmpty(xGoogPageId) &&
                !string.IsNullOrEmpty(ytConfig.DELEGATED_SESSION_ID))
            {
                xGoogPageId = ytConfig.DELEGATED_SESSION_ID;
            }

            if (!string.IsNullOrEmpty(xGoogPageId))
            {
                request.Headers.Add("x-goog-pageid", xGoogPageId);
            }

            if (!string.IsNullOrEmpty(ytConfig.ID_TOKEN))
            {
                request.Headers.Add("x-youtube-identity-token", ytConfig.ID_TOKEN);
            }

            if (!string.IsNullOrEmpty(ytConfig.SESSION_INDEX))
            {
                xGoogAuthuser = ytConfig.SESSION_INDEX;
            }

            request.Headers.Add("x-goog-authuser", xGoogAuthuser);
            request.Headers.Add("x-goog-visitor-id", ytConfig.VisitorData);
            request.Headers.Add("x-youtube-client-name", ytConfig.INNERTUBE_CONTEXT_CLIENT_NAME);
            request.Headers.Add("x-youtube-client-version", ytConfig.INNERTUBE_CLIENT_VERSION);

            if (!string.IsNullOrEmpty(ytConfig.InitPage))
            {
                request.Headers.Add("referer", ytConfig.InitPage);
            }
        }

        request.Headers.Add("origin", StringSet.Origin);
        request.Headers.Add("x-origin", StringSet.Origin);
    }

    /// <summary>
    /// 取得 SAPISIDHASH 字串
    /// </summary>
    /// <param name="sapiSid">字串，SAPISID</param>
    /// <param name="origin">字串，origin</param>
    /// <returns>字串</returns>
    private static string GetSapiSidHash(string sapiSid, string origin)
    {
        long unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

        return $"{unixTimestamp}_{GetSHA1Hash($"{unixTimestamp} {sapiSid} {origin}")}";
    }

    /// <summary>
    /// 取得 SHA-1 雜湊 
    /// </summary>
    /// <param name="value">字串，值</param>
    /// <returns>字串</returns>
    private static string GetSHA1Hash(string value)
    {
        using SHA1 sha1 = SHA1.Create();

        byte[] bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(value));

        StringBuilder builder = new();

        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }

        return builder.ToString();
    }
}