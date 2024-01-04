using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Net;
using System.Text;
using System.Text.Json;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Sets;
using Rubujo.YouTube.Utility.Extensions;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// YTJsonParser 的核心方法
/// <para>參考 1：https://takikomiprogramming.hateblo.jp/entry/2020/07/21/114851</para>
/// <para>參考 2：https://yasulab-pg.com/%E3%80%90python%E3%80%91youtube-live%E3%81%AE%E3%82%A2%E3%83%BC%E3%82%AB%E3%82%A4%E3%83%96%E3%81%8B%E3%82%89%E3%83%81%E3%83%A3%E3%83%83%E3%83%88%E3%82%92%E5%8F%96%E5%BE%97%E3%81%99%E3%82%8B/</para>
/// </summary>
public partial class YTJsonParser
{
    /// <summary>
    /// 取得 ytcfg 資料
    /// </summary>
    /// <param name="videoIDorChannelID">字串，影片 ID 或是頻道 ID</param>
    /// <param name="dataType">EnumSet.DataType，預設直為 DataType.LiveChat</param>
    /// <returns>InitialData</returns>
    private InitialData GetYTConfigData(
        string videoIDorChannelID,
        EnumSet.DataType dataType = EnumSet.DataType.LiveChat)
    {
        InitialData initialData = new()
        {
            YTConfigData = new()
        };

        string url;

        switch (dataType)
        {
            default:
            case EnumSet.DataType.LiveChat:
                url = SharedIsStreaming ?
                    $"{StringSet.Origin}/live_chat?v={videoIDorChannelID}" :
                    $"{StringSet.Origin}/watch?v={videoIDorChannelID}";
                break;
            case EnumSet.DataType.Community:
                url = $"{StringSet.Origin}/channel/{videoIDorChannelID}/community";

                initialData.YTConfigData.InitPage = url;
                break;
        }

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, url);

        if (!string.IsNullOrEmpty(SharedCookies))
        {
            SetHttpRequestMessageHeader(httpRequestMessage);
        }

        bool hasRegionData = DictionarySet.GetRegionDictionary()
            .TryGetValue(
                SharedDisplayLanguage,
                out RegionData? regionData);

        // 套用設定的語系。
        if (hasRegionData)
        {
            httpRequestMessage.Headers.AcceptLanguage.Clear();
            httpRequestMessage.Headers.AcceptLanguage.TryParseAdd(regionData?.AcceptLanguage);
        }

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
                    "[YTJsonParser.GetYTConfigData()] 發生錯誤，變數 \"htmlContent\" 為空白或是 null！");

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
                    "[YTJsonParser.GetYTConfigData()] 發生錯誤，變數 \"elementYtCfg\" 為 null！");

                return initialData;
            }

            // TODO: 2023/6/13 考慮是否待修改。
            // 可以參考 1：https://github.com/abhinavxd/youtube-live-chat-downloader/blob/v1.0.5/yt_chat.go#L140
            // 可以參考 2：https://github.com/xenova/chat-downloader/blob/master/chat_downloader/sites/youtube.py#L443
            string jsonYtCfg = elementYtCfg.InnerHtml;

            switch (dataType)
            {
                default:
                case EnumSet.DataType.LiveChat:
                    {
                        if (SharedIsStreaming)
                        {
                            jsonYtCfg = jsonYtCfg.Replace("ytcfg.set(", string.Empty);

                            int endTokenIndex = jsonYtCfg.LastIndexOf("});");

                            // 要補回最後一個 "}"。
                            jsonYtCfg = jsonYtCfg[..(endTokenIndex + 1)];
                        }
                        else
                        {
                            int startTokenIndex1 = jsonYtCfg.IndexOf("ytcfg.set({"),
                                endTokenIndex1 = jsonYtCfg.IndexOf("});");

                            // 只擷取要的部分。
                            jsonYtCfg = jsonYtCfg.Substring(startTokenIndex1, endTokenIndex1);
                            jsonYtCfg = jsonYtCfg.Replace("ytcfg.set(", string.Empty);

                            // 重新再在找一次。
                            endTokenIndex1 = jsonYtCfg.LastIndexOf("});");

                            // 要補回最後一個 "}"。
                            jsonYtCfg = jsonYtCfg[..(endTokenIndex1 + 1)];
                        }

                        break;
                    }
                case EnumSet.DataType.Community:
                    int startTokenIndex2 = jsonYtCfg.IndexOf("ytcfg.set({"),
                        endTokenIndex2 = jsonYtCfg.IndexOf("});");

                    // 只擷取要的部分。
                    jsonYtCfg = jsonYtCfg.Substring(startTokenIndex2, endTokenIndex2);
                    jsonYtCfg = jsonYtCfg.Replace("ytcfg.set(", string.Empty);

                    // 重新再在找一次。
                    endTokenIndex2 = jsonYtCfg.LastIndexOf("});");

                    // 要補回最後一個 "}"。
                    jsonYtCfg = jsonYtCfg[..(endTokenIndex2 + 1)];

                    break;
            }

            JsonElement jeYtCfg = JsonSerializer.Deserialize<JsonElement>(jsonYtCfg);

            initialData.YTConfigData = ParseYtCfg(jeYtCfg);

            // 套用設定的語系。
            if (hasRegionData)
            {
                initialData.YTConfigData.Gl = regionData?.Gl;
                initialData.YTConfigData.Hl = regionData?.Hl;
                initialData.YTConfigData.TimeZone = regionData?.TimeZone;
            }

            RaiseOnLogOutput(EnumSet.LogType.Debug, jeYtCfg.GetRawText());

            IElement elementYtInitialData = dataType switch
            {
                EnumSet.DataType.Community => scriptElements.FirstOrDefault(n => n.InnerHtml.Contains("var ytInitialData ="))!,
                _ => SharedIsStreaming ?
                    scriptElements.FirstOrDefault(n => n.InnerHtml.Contains("window[\"ytInitialData\"] ="))! :
                    scriptElements.FirstOrDefault(n => n.InnerHtml.Contains("var ytInitialData ="))!,
            };

            string jsonYtInitialData = dataType switch
            {
                EnumSet.DataType.Community => elementYtInitialData.InnerHtml.Replace("var ytInitialData = ", string.Empty),
                _ => SharedIsStreaming ?
                    elementYtInitialData.InnerHtml.Replace("window[\"ytInitialData\"] = ", string.Empty) :
                    elementYtInitialData.InnerHtml.Replace("var ytInitialData = ", string.Empty),
            };

            if (jsonYtInitialData.EndsWith(';'))
            {
                jsonYtInitialData = jsonYtInitialData[0..^1];
            }

            JsonElement jeYtInitialData = JsonSerializer.Deserialize<JsonElement>(jsonYtInitialData);

            switch (dataType)
            {
                default:
                case EnumSet.DataType.LiveChat:
                    initialData.YTConfigData.Continuation = SharedIsStreaming ?
                        ParseStreamingContinuation(jeYtInitialData)[0] :
                        ParseReplayContinuation(jeYtInitialData);

                    // 若是直播中的影片時，剛載入頁面就影片聊天室的內容，這些資料也需要處理。
                    if (SharedIsStreaming)
                    {
                        initialData.Messages = ParseActions(jeYtInitialData);
                    }

                    break;
                case EnumSet.DataType.Community:
                    initialData.Posts = GetInitialPosts(jeYtInitialData, initialData.YTConfigData);

                    break;
            }
        }
        else
        {
            string errorMessage = $"[{DateTime.Now}]（{typeof(HttpClient).Name}）：" +
                $"連線發生錯誤，錯誤碼：{httpResponseMessage?.StatusCode} " +
                $"{(httpResponseMessage != null ?
                    $"({(int)(httpResponseMessage.StatusCode)})" :
                    string.Empty)}{Environment.NewLine}" +
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
    /// <param name="dataType">EnumSet.DataType，預設直為 DataType.LiveChat</param>
    /// <returns>JsonElement</returns>
    private JsonElement GetJsonElement(
        YTConfigData ytConfigData,
        EnumSet.DataType dataType = EnumSet.DataType.LiveChat)
    {
        JsonElement jsonElement = new();

        string methodName = SharedIsStreaming ?
            "get_live_chat" :
            "get_live_chat_replay";

        string url = dataType switch
        {
            EnumSet.DataType.Community => $"{StringSet.Origin}/youtubei/v1/browse?key={ytConfigData?.APIKey}",
            _ => $"{StringSet.Origin}/youtubei/v1/live_chat/{methodName}?key={ytConfigData.APIKey}",
        };

        // 當 ytConfigData.Continuation 為 null 或空值時，則表示已經抓取完成。
        if (!string.IsNullOrEmpty(ytConfigData?.Continuation))
        {
            // 當沒有時才指定，後續不更新。
            if (string.IsNullOrEmpty(ytConfigData.InitPage))
            {
                switch (dataType)
                {
                    default:
                    case EnumSet.DataType.LiveChat:
                        string apiType = methodName.Replace("get_", string.Empty);

                        ytConfigData.InitPage = $"{StringSet.Origin}/{apiType}/?continuation={ytConfigData.Continuation}";

                        break;
                    case EnumSet.DataType.Community:
                        // 不進行任何的操作，理論上在第一次獲取資料時就會被設定好。
                        break;
                }
            }

            bool hasRegionData = DictionarySet.GetRegionDictionary()
                .TryGetValue(
                    SharedDisplayLanguage,
                    out RegionData? regionData);

            // 套用設定的語系。
            if (hasRegionData)
            {
                ytConfigData.Gl = regionData?.Gl;
                ytConfigData.Hl = regionData?.Hl;
                ytConfigData.TimeZone = regionData?.TimeZone;
            }

            string jsonContent = GetRequestPayloadData(ytConfigData);

            RaiseOnLogOutput(EnumSet.LogType.Debug, jsonContent);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, url);

            if (!string.IsNullOrEmpty(SharedCookies))
            {
                SetHttpRequestMessageHeader(httpRequestMessage, ytConfigData);
            }

            // 套用設定的語系。
            if (hasRegionData)
            {
                httpRequestMessage.Headers.AcceptLanguage.Clear();
                httpRequestMessage.Headers.AcceptLanguage.TryParseAdd(regionData?.AcceptLanguage);
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
                    "[YTJsonParser.GetJsonElement()] 發生錯誤，變數 \"receivedJsonContent\" 為空白或是 null！");

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
                    $"{(httpResponseMessage != null ?
                        $"({(int)(httpResponseMessage.StatusCode)})" :
                        string.Empty)}{Environment.NewLine}" +
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
    /// 設定 YTConfigData 的 Continuation
    /// </summary>
    /// <param name="arrayEnumerator">JsonElement.ArrayEnumerator</param>
    /// <param name="ytConfigData">YTConfig</param>
    private void SetContinuation(
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
                    BrowserVersion = ytConfigData.BrowserVersion,
                    ClientFormFactor = ytConfigData.ClientFormFactor,
                    ClientName = ytConfigData.ClientName,
                    ClientVersion = ytConfigData.ClientVersion,
                    DeviceMake = ytConfigData.DeviceMake,
                    DeviceModel = ytConfigData.DeviceModel,
                    Gl = ytConfigData.Gl,
                    Hl = ytConfigData.Hl,
                    OriginalUrl = ytConfigData.OriginalUrl,
                    OsName = ytConfigData.OsName,
                    OsVersion = ytConfigData.OsVersion,
                    Platform = ytConfigData.Platform,
                    RemoteHost = ytConfigData.RemoteHost,
                    UserAgent = ytConfigData.UserAgent,
                    VisitorData = ytConfigData.VisitorData,
                    TimeZone = ytConfigData.TimeZone,
                }
            },
            Continuation = ytConfigData.Continuation
        };

        return JsonSerializer.Serialize(requestPayloadData);
    }
}