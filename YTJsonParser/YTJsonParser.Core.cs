using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Sets;
using System.Net;
using System.Text;
using System.Text.Json;

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
    /// <returns>Task&lt;InitialData&gt;</returns>
    private async Task<InitialData> GetYTConfigDataAsync(
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

        if (SharedHttpClient == null)
        {
            Init();
        }

        HttpResponseMessage httpResponseMessage = await SharedHttpClient!.SendAsync(httpRequestMessage);

        RaiseOnLogOutput(EnumSet.LogType.Debug, httpResponseMessage.ToString());

        string htmlContent = await httpResponseMessage.Content.ReadAsStringAsync();

        if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
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
            // 可以參考 1：https://github.com/abhinavxd/youtube-live-chat-downloader/blob/v2.0.3/yt_chat.go#L147
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
                    {
                        // 來源：https://github.com/xenova/chat-downloader/pull/277/files
                        // 2025/11/10 修正：套用 API 變更（2025/10）後的「兩步驟擷取」邏輯。

                        // 步驟一：從 ytInitialData 取得「通用」權杖（無論直播或重播，路徑相同）。
                        JsonElement? genericContinuationToken = jeYtInitialData
                            .Get("contents")
                            ?.Get("twoColumnWatchNextResults")
                            ?.Get("conversationBar")
                            ?.Get("liveChatRenderer")
                            ?.Get("continuations")
                            ?.ToArrayEnumerator()
                            ?.FirstOrDefault()
                            .Get("reloadContinuationData")
                            ?.Get("continuation");

                        string genericToken = genericContinuationToken?.GetString() ?? string.Empty;

                        if (string.IsNullOrEmpty(genericToken))
                        {
                            // 失敗：在 initialData 中找不到 "reloadContinuationData" 通用權杖。
                            //
                            // 【重要備註】：
                            // 如果是「直播」，有時 continuation 會在 *不同* 的路徑。
                            // 您目前的 C# 程式碼有一個 ParseStreamingContinuation 函數，
                            // 它可能包含了備用的直播權杖路徑（例如 invalidationContinuationData）。
                            //
                            // 為了安全起見，我們先執行原有的直播邏輯，如果它失敗了（回傳空值），
                            // 再嘗試新的「兩步驟擷取」。

                            string originalStreamingToken = string.Empty;

                            if (SharedIsStreaming)
                            {
                                originalStreamingToken = ParseStreamingContinuation(jeYtInitialData)[0];
                            }

                            // 如果原始的直播邏輯能取到值，就直接使用（這表示是舊版 API 或另一種直播類型）。
                            if (!string.IsNullOrEmpty(originalStreamingToken))
                            {
                                RaiseOnLogOutput(EnumSet.LogType.Debug, $"[YTJsonParser]（直播）偵測到直接的 streaming continuation，使用舊有邏輯：{originalStreamingToken}");
                                
                                initialData.YTConfigData.Continuation = originalStreamingToken;
                                initialData.Messages = ParseActions(jeYtInitialData);

                                // 跳出 switch case。
                                break; 
                            }

                            // 如果原始邏輯失敗 且 genericToken 也為空，才真正報錯。
                            if (string.IsNullOrEmpty(genericToken))
                            {
                                RaiseOnLogOutput(EnumSet.LogType.Error, $"[YTJsonParser] 步驟一（直播／重播）失敗：在 initialData 中找不到 \"reloadContinuationData\" 通用權杖，且舊的 ParseStreamingContinuation 也失敗。");
                                
                                // 返回空資料。
                                return initialData;
                            }
                        }

                        RaiseOnLogOutput(EnumSet.LogType.Debug, $"[YTJsonParser] 步驟一（直播／重播）成功：取得通用權杖：{genericToken}");

                        // 步驟二：根據「直播」或「重播」狀態，決定中繼網址。
                        string intermediateUrl = SharedIsStreaming ?
                            $"{StringSet.Origin}/live_chat?continuation={genericToken}" :
                            $"{StringSet.Origin}/live_chat_replay?continuation={genericToken}";

                        // 步驟三：使用「通用」權杖，發起中繼 GET 請求。
                        HttpRequestMessage intermediateRequest = new(HttpMethod.Get, intermediateUrl);

                        // 複製原始請求的 Cookies 和 語系設定。
                        if (!string.IsNullOrEmpty(SharedCookies))
                        {
                            SetHttpRequestMessageHeader(intermediateRequest);
                        }

                        if (hasRegionData)
                        {
                            intermediateRequest.Headers.AcceptLanguage.Clear();
                            intermediateRequest.Headers.AcceptLanguage.TryParseAdd(regionData?.AcceptLanguage);
                        }

                        // 步驟四：執行中繼請求：
                        RaiseOnLogOutput(EnumSet.LogType.Debug, $"[YTJsonParser] 步驟二（直播／重播）：發起中繼 GET 請求至 {intermediateUrl}");
                        
                        HttpResponseMessage intermediateResponse = await SharedHttpClient!.SendAsync(intermediateRequest);
                        
                        string intermediateHtml = await intermediateResponse.Content.ReadAsStringAsync();

                        if (intermediateResponse.StatusCode != HttpStatusCode.OK)
                        {
                            RaiseOnLogOutput(EnumSet.LogType.Error, $"[YTJsonParser] 步驟二（直播／重播）失敗：中繼 GET 請求錯誤：{intermediateResponse.StatusCode}{Environment.NewLine}{intermediateHtml}");
                            
                            return initialData;
                        }

                        // 步驟五：解析中繼回應中的「新」 ytInitialData：
                        HtmlParser intermediateParser = new();

                        IHtmlDocument intermediateDocument = intermediateParser.ParseDocument(intermediateHtml);

                        // 根據 Python 程式碼，新資料位於 window["ytInitialData"]。
                        IElement elementYtInitialDataNew = intermediateDocument.QuerySelectorAll("script")
                            .FirstOrDefault(n => n.InnerHtml.Contains("window[\"ytInitialData\"] ="))!;

                        if (elementYtInitialDataNew == null)
                        {
                            RaiseOnLogOutput(EnumSet.LogType.Error, "[YTJsonParser] 步驟三（直播／重播）失敗：在中繼回應中找不到 'window[\"ytInitialData\"]'。");
                            
                            return initialData;
                        }

                        string jsonYtInitialDataNew = elementYtInitialDataNew.InnerHtml.Replace("window[\"ytInitialData\"] = ", string.Empty);

                        if (jsonYtInitialDataNew.EndsWith(';'))
                        {
                            jsonYtInitialDataNew = jsonYtInitialDataNew[0..^1];
                        }

                        // 步驟六：反序列化「新」JSON。
                        JsonElement jeYtInitialDataNew = JsonSerializer.Deserialize<JsonElement>(jsonYtInitialDataNew);

                        // 步驟七：從「新」JSON 中解析 subMenuItems，並取得「真實」權杖。
                        //（注意：這裡的 JSON 結構是 "continuationContents"，與初始頁面的 "contents" 不同）。
                        JsonElement.ArrayEnumerator? subMenuItems = jeYtInitialDataNew
                            .Get("continuationContents")
                            ?.Get("liveChatContinuation")
                            ?.Get("header")
                            ?.Get("liveChatHeaderRenderer")
                            ?.Get("viewSelector")
                            ?.Get("sortFilterSubMenuRenderer")
                            ?.Get("subMenuItems")
                            ?.ToArrayEnumerator();

                        string trueContinuationToken = string.Empty;
                        // 要尋找的標題（"Live chat" 或 "聊天重播"）。
                        string targetTitle = string.Empty;

                        int targetIndex = SharedLiveChatType.ToInt32();

                        // 根據直播／重播狀態，決定要查找的標題或索引。
                        if (SharedIsStreaming)
                        {
                            targetTitle = !string.IsNullOrEmpty(SharedCustomLiveChatType) ?
                                SharedCustomLiveChatType :
                                // 預設使用索引 1（0=熱門重播、1=聊天重播）。
                                subMenuItems?.ElementAtOrDefault(targetIndex).Get("title")?.GetString() ?? string.Empty;
                        }
                        else
                        {
                            // 重播。
                            targetTitle = !string.IsNullOrEmpty(SharedCustomLiveChatType) ?
                                SharedCustomLiveChatType :
                                // 預設使用索引 1（0=熱門重播、1=聊天重播）。
                                subMenuItems?.ElementAtOrDefault(targetIndex).Get("title")?.GetString() ?? string.Empty;
                        }

                        RaiseOnLogOutput(EnumSet.LogType.Debug, $"[YTJsonParser] 步驟四（直播／重播）：搜尋目標標題 \"{targetTitle}\"（來自自定義：\"{SharedCustomLiveChatType}\" 或索引值：{targetIndex}）……");

                        if (subMenuItems.HasValue && !string.IsNullOrEmpty(targetTitle))
                        {
                            foreach (JsonElement subMenuItem in subMenuItems)
                            {
                                string? title = subMenuItem.Get("title")?.GetString();

                                if (title == targetTitle)
                                {
                                    JsonElement? continuation = subMenuItem.Get("continuation")
                                        ?.Get("reloadContinuationData")
                                        ?.Get("continuation");

                                    if (continuation.HasValue)
                                    {
                                        trueContinuationToken = continuation.Value.GetString() ?? string.Empty;
                                        
                                        RaiseOnLogOutput(EnumSet.LogType.Debug, $"[YTJsonParser] 步驟五（直播／重播）成功：找到「真實」權杖（{title}）：{trueContinuationToken}");
                                        
                                        break;
                                    }
                                    else
                                    {
                                        RaiseOnLogOutput(EnumSet.LogType.Warn, $"[YTJsonParser]（直播／重播）警告：找到標題 \"{title}\"，但其 \"continuation.reloadContinuationData.continuation\" 路徑下沒有權杖。");
                                    }
                                }
                            }
                        }
                        else if (!subMenuItems.HasValue)
                        {
                            RaiseOnLogOutput(EnumSet.LogType.Error, "[YTJsonParser] 步驟三（直播／重播）失敗：在中繼回應中找不到 \"subMenuItems\"。");
                        }
                        else
                        {
                            RaiseOnLogOutput(EnumSet.LogType.Error, $"[YTJsonParser] 步驟四（直播／重播）失敗：未能從索引 {targetIndex} 或自定義名稱 \"{SharedCustomLiveChatType}\" 決定目標標題。");
                        }

                        // 步驟八：儲存「真實」權杖。
                        if (string.IsNullOrEmpty(trueContinuationToken))
                        {
                            RaiseOnLogOutput(
                                EnumSet.LogType.Error,
                                $"[YTJsonParser]（直播／重播）最終失敗：未能取得 \"{targetTitle}\" 的「真實」 continuation 權杖。");
                        }

                        initialData.YTConfigData.Continuation = trueContinuationToken;

                        // 【重要】：
                        // 即使是直播，在兩步驟擷取後，初始訊息也可能在「新」的 JSON 中。
                        if (SharedIsStreaming)
                        {
                            // 從「新」的 jeYtInitialDataNew 中解析初始訊息。
                            initialData.Messages = ParseActions(jeYtInitialDataNew);
                        }
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
    /// <returns>Task&lt;JsonElement&gt;</returns>
    private async Task<JsonElement> GetJsonElementAsync(
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

            HttpResponseMessage httpResponseMessage = await SharedHttpClient!.SendAsync(httpRequestMessage);

            RaiseOnLogOutput(EnumSet.LogType.Debug, httpResponseMessage.ToString());

            string? receivedJsonContent = await httpResponseMessage.Content.ReadAsStringAsync();

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
    /// 取得要求裝載資料
    /// </summary>
    /// <param name="ytConfigData">YTConfigData</param>
    /// <returns>字串</returns>
    private static string GetRequestPayloadData(YTConfigData ytConfigData)
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