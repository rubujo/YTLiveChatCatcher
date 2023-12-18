using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Net;
using System.Text;
using System.Text.Json;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// LiveChatCatcher 的核心方法
/// <para>參考 1：https://takikomiprogramming.hateblo.jp/entry/2020/07/21/114851</para>
/// <para>參考 2：https://yasulab-pg.com/%E3%80%90python%E3%80%91youtube-live%E3%81%AE%E3%82%A2%E3%83%BC%E3%82%AB%E3%82%A4%E3%83%96%E3%81%8B%E3%82%89%E3%83%81%E3%83%A3%E3%83%83%E3%83%88%E3%82%92%E5%8F%96%E5%BE%97%E3%81%99%E3%82%8B/</para>
/// </summary>
public partial class LiveChatCatcher
{
    /// <summary>
    /// 取得 ytcfg 資料
    /// </summary>
    /// <param name="videoID">字串，影片 ID</param>
    /// <returns>InitialData</returns>
    private InitialData GetYTConfigData(string videoID)
    {
        InitialData initialData = new()
        {
            YTConfigData = new()
        };

        string url = SharedIsStreaming ? $"{StringSet.Origin}/live_chat?v={videoID}" :
            $"{StringSet.Origin}/watch?v={videoID}";

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, url);

        if (!string.IsNullOrEmpty(SharedCookies))
        {
            SetHttpRequestMessageHeader(httpRequestMessage);
        }

        // TODO: 2023/12/18 測試語系。
        httpRequestMessage.Headers.AcceptLanguage.Clear();
        httpRequestMessage.Headers.AcceptLanguage.TryParseAdd("ja-JP,ja;q=0.9");

        RaiseOnLogOutput(EnumSet.LogType.Debug, httpRequestMessage.ToString());

        HttpResponseMessage? httpResponseMessage = SharedHttpClient?.SendAsync(httpRequestMessage)
            .GetAwaiter()
            .GetResult();

        if (httpResponseMessage != null)
        {
            RaiseOnLogOutput(EnumSet.LogType.Debug, httpResponseMessage.ToString());
        }

        string? htmlContent = httpResponseMessage?.Content.ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();

        if (httpResponseMessage?.StatusCode == HttpStatusCode.OK)
        {
            if (string.IsNullOrEmpty(htmlContent))
            {
                RaiseOnLogOutput(
                    EnumSet.LogType.Error,
                    "[LiveChatCatcher.GetYTConfigData()] 發生錯誤，變數 \"htmlContent\" 為空白或是 null！");

                return initialData;
            }

            HtmlParser htmlParser = new();
            IHtmlDocument htmlDocument = htmlParser.ParseDocument(htmlContent);
            IHtmlCollection<IElement> scriptElements = htmlDocument.QuerySelectorAll("script");
            IElement elementYtCfg = scriptElements
                .FirstOrDefault(n => n.InnerHtml.Contains("ytcfg.set({"))!;

            if (elementYtCfg == null)
            {
                RaiseOnLogOutput(
                    EnumSet.LogType.Error,
                    "[LiveChatCatcher.GetYTConfigData()] 發生錯誤，變數 \"elementYtCfg\" 為 null！");

                return initialData;
            }

            // TODO: 2023/6/13 考慮是否待修改。
            // 可以參考 1：https://github.com/abhinavxd/youtube-live-chat-downloader/blob/v1.0.5/yt_chat.go#L140
            // 可以參考 2：https://github.com/xenova/chat-downloader/blob/master/chat_downloader/sites/youtube.py#L443
            string jsonYtCfg = elementYtCfg.InnerHtml;

            if (SharedIsStreaming)
            {
                jsonYtCfg = jsonYtCfg.Replace("ytcfg.set(", string.Empty);

                int endTokenIndex = jsonYtCfg.LastIndexOf("});");

                // 要補回最後一個 "}"。
                jsonYtCfg = jsonYtCfg[..(endTokenIndex + 1)];
            }
            else
            {
                int startTokenIndex = jsonYtCfg.IndexOf("ytcfg.set({"),
                    endTokenIndex = jsonYtCfg.IndexOf("});");

                // 只擷取要的部分。
                jsonYtCfg = jsonYtCfg.Substring(startTokenIndex, endTokenIndex);
                jsonYtCfg = jsonYtCfg.Replace("ytcfg.set(", string.Empty);

                // 重新再在找一次。
                endTokenIndex = jsonYtCfg.LastIndexOf("});");

                // 要補回最後一個 "}"。
                jsonYtCfg = jsonYtCfg[..(endTokenIndex + 1)];
            }

            JsonElement jeYtCfg = JsonSerializer.Deserialize<JsonElement>(jsonYtCfg);

            initialData.YTConfigData = ParseYtCfg(jeYtCfg);

            RaiseOnLogOutput(EnumSet.LogType.Debug, jeYtCfg.GetRawText());

            IElement elementYtInitialData = SharedIsStreaming ?
                scriptElements.FirstOrDefault(n => n.InnerHtml.Contains("window[\"ytInitialData\"] ="))! :
                scriptElements.FirstOrDefault(n => n.InnerHtml.Contains("var ytInitialData ="))!;

            string jsonYtInitialData = SharedIsStreaming ?
                elementYtInitialData.InnerHtml.Replace("window[\"ytInitialData\"] = ", string.Empty) :
                elementYtInitialData.InnerHtml.Replace("var ytInitialData = ", string.Empty);

            if (jsonYtInitialData.EndsWith(';'))
            {
                jsonYtInitialData = jsonYtInitialData[0..^1];
            }

            JsonElement jeYtInitialData = JsonSerializer.Deserialize<JsonElement>(jsonYtInitialData);

            initialData.YTConfigData.Continuation = SharedIsStreaming ?
                ParseFirstTimeContinuation(jeYtInitialData)[0] :
                ParseReplayContinuation(jeYtInitialData);

            // 若是直播中的影片時，剛載入頁面就影片聊天室的內容，這些資料也需要處理。
            if (SharedIsStreaming)
            {
                initialData.Messages = ParseActions(jeYtInitialData);
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

        return initialData;
    }

    /// <summary>
    /// 取得 JsonElement
    /// </summary>
    /// <param name="ytConfigData">YTConfigData</param>
    /// <returns>JsonElement</returns>
    private JsonElement GetJsonElement(YTConfigData ytConfigData)
    {
        JsonElement jsonElement = new();

        string methodName = SharedIsStreaming ? "get_live_chat" : "get_live_chat_replay";
        string url = $"{StringSet.Origin}/youtubei/v1/live_chat/{methodName}?key={ytConfigData.APIKey}";

        // 當 ytConfigData.Continuation 為 null 或空值時，則表示已經抓取完成。
        if (!string.IsNullOrEmpty(ytConfigData.Continuation))
        {
            // 當沒有時才指定，後續不更新。
            if (string.IsNullOrEmpty(ytConfigData.InitPage))
            {
                string apiType = methodName.Replace("get_", string.Empty);

                ytConfigData.InitPage = $"{StringSet.Origin}/{apiType}/?continuation={ytConfigData.Continuation}";
            }

            string jsonContent = GetRequestPayloadData(ytConfigData);

            RaiseOnLogOutput(EnumSet.LogType.Debug, jsonContent);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, url);

            if (!string.IsNullOrEmpty(SharedCookies))
            {
                SetHttpRequestMessageHeader(httpRequestMessage, ytConfigData);
            }

            HttpContent httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            httpRequestMessage.Content = httpContent;

            RaiseOnLogOutput(EnumSet.LogType.Debug, httpRequestMessage.ToString());

            HttpResponseMessage? httpResponseMessage = SharedHttpClient?.SendAsync(httpRequestMessage)
                .GetAwaiter()
                .GetResult();

            if (httpResponseMessage != null)
            {
                RaiseOnLogOutput(EnumSet.LogType.Debug, httpResponseMessage.ToString());
            }

            string? receivedJsonContent = httpResponseMessage?.Content.ReadAsStringAsync()
                .GetAwaiter()
                .GetResult();

            if (string.IsNullOrEmpty(receivedJsonContent))
            {
                RaiseOnLogOutput(
                    EnumSet.LogType.Error,
                    "[LiveChatCatcher.GetJsonElement()] 發生錯誤，變數 \"receivedJsonContent\" 為空白或是 null！");

                return jsonElement;
            }

            if (httpResponseMessage?.StatusCode == HttpStatusCode.OK)
            {
                jsonElement = JsonSerializer.Deserialize<JsonElement>(receivedJsonContent);
            }
            else
            {
                string errorMessage = $"[{DateTime.Now}]（{typeof(HttpClient).Name}）：" +
                    $"連線發生錯誤，錯誤碼：{httpResponseMessage?.StatusCode} " +
                    $"{(httpResponseMessage != null ? $"({(int)(httpResponseMessage.StatusCode)})" : string.Empty)}{Environment.NewLine}" +
                    $"接收到的內容：{Environment.NewLine}" +
                    $"{receivedJsonContent}{Environment.NewLine}";

                RaiseOnLogOutput(
                    EnumSet.LogType.Error,
                    errorMessage);
            }
        }

        return jsonElement;
    }

    /// <summary>
    /// 取得要求裝載資料
    /// </summary>
    /// <param name="ytConfigData">YTConfigData</param>
    /// <returns>字串</returns>
    private string GetRequestPayloadData(YTConfigData ytConfigData)
    {
        // 參考：https://github.com/xenova/chat-downloader/blob/master/chat_downloader/sites/youtube.py#L1764
        // 參考：https://github.com/abhinavxd/youtube-live-chat-downloader/blob/main/yt_chat.go

        // ※內容是精簡過的。
        RequestPayloadData requestPayloadData = new()
        {
            Context = new()
            {
                Client = new()
                {
                    BrowserName = ytConfigData.BrowserName,
                    BrowserVersion = ytConfigData.BrowserVersion ,
                    ClientFormFactor = ytConfigData.ClientFormFactor,
                    ClientName = ytConfigData.ClientName,
                    ClientVersion = ytConfigData.ClientVersion,
                    DeviceMake = ytConfigData.DeviceMake,
                    DeviceModel = ytConfigData.DeviceModel,
                    Gl = ytConfigData.Gl ?? "TW",
                    Hl = ytConfigData.Hl ?? "zh-TW",
                    OriginalUrl = ytConfigData.OriginalUrl,
                    OsName = ytConfigData.OsName,
                    OsVersion = ytConfigData.OsVersion,
                    Platform = ytConfigData.Platform,
                    RemoteHost = ytConfigData.RemoteHost,
                    UserAgent = ytConfigData.UserAgent,
                    VisitorData = ytConfigData.VisitorData,
                    TimeZone = "Asia/Taipei"
                }
            },
            Continuation = ytConfigData.Continuation
        };

        return JsonSerializer.Serialize(requestPayloadData);
    }
}