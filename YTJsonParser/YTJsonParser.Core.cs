using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Sets;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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
    /// <param name="liveChatOptions">LiveChatStreamOptions，僅 dataType 為 LiveChat 時使用</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task&lt;InitialData&gt;</returns>
    private async Task<InitialData> GetYTConfigDataAsync(
        string videoIDorChannelID,
        EnumSet.DataType dataType = EnumSet.DataType.LiveChat,
        LiveChatStreamOptions? liveChatOptions = null,
        CancellationToken cancellationToken = default)
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
                // 2026/8 更新：無論直播中或重播，一律使用 popout 版本的聊天室頁面。
                // 該頁面回傳的 ytInitialData 結構（contents.liveChatRenderer）在直播／重播下一致，
                // 不再需要分別拉取 /watch 或 /live_chat 頁面。
                url = $"{StringSet.Origin}/live_chat?is_popout=1&v={videoIDorChannelID}";
                break;
            case EnumSet.DataType.Community:
                // 2026/8 修正：改用 /posts（YouTube 已將分頁網址由 /community 更名，見下方 GetCommunityTab
                // 的說明）。實測發現對相當比例的頻道（例如 Kurzgesagt、米妃Tobi 等訂閱數大、頻道存在已久
                // 的頻道，5 個抽測頻道裡有 2 個）直接請求 /community 會回傳「沒有貼文」的空狀態訊息，即使
                // 該頻道其實有貼文；改用 /posts 在同一批抽測頻道裡全數正常運作，沒有任何一個失敗。
                url = $"{StringSet.Origin}/channel/{videoIDorChannelID}/posts";

                initialData.YTConfigData.InitPage = url;
                break;
        }

        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, url);

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

        LogMessages.Debug(_logger, nameof(GetYTConfigDataAsync), GetRedactedRequestSummary(httpRequestMessage));

        HttpResponseMessage httpResponseMessage;

        try
        {
            httpResponseMessage = await SharedHttpClient!.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogMessages.Error(_logger, nameof(GetYTConfigDataAsync), $"發送請求失敗：{ex.GetExceptionMessage()}");

            return initialData;
        }

        using (httpResponseMessage)
        {
            LogMessages.Debug(_logger, nameof(GetYTConfigDataAsync), httpResponseMessage.ToString());

            string htmlContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                if (string.IsNullOrEmpty(htmlContent))
                {
                    LogMessages.Error(_logger, nameof(GetYTConfigDataAsync), "變數 \"htmlContent\" 為空白或是 null！");

                    return initialData;
                }

                HtmlParser htmlParser = new();
                IHtmlDocument htmlDocument = htmlParser.ParseDocument(htmlContent);
                IHtmlCollection<IElement> scriptElements = htmlDocument.QuerySelectorAll("script");
                IElement? elementYtCfg = scriptElements
                    .FirstOrDefault(n => n.InnerHtml.Contains("ytcfg.set({"));

                if (elementYtCfg == null)
                {
                    LogMessages.Error(_logger, nameof(GetYTConfigDataAsync), "變數 \"elementYtCfg\" 為 null！");

                    return initialData;
                }

                string jsonYtCfg = elementYtCfg.InnerHtml;

                switch (dataType)
                {
                    default:
                    case EnumSet.DataType.LiveChat:
                        {
                            // popout 頁面（直播／重播皆同）一律採此格式擷取。
                            //
                            // 2026/8 修正舊版 TODO（2023/6/13，原本考慮參考 yt_chat.go／chat-downloader 的
                            // 做法）：原本用 Replace("ytcfg.set(", "") 加上 LastIndexOf("});") 裁切字串尾端，
                            // 跟 IsVideoStreamingAsync 修正 ytInitialPlayerResponse 時遇到的問題是同一類——
                            // 一旦 ytcfg 物件內任何欄位值剛好含有字面上的 "});"（例如某個巢狀字串），
                            // LastIndexOf 就會找到錯誤的位置而截斷失敗。已改用同一份程式碼裡已經驗證過的
                            // ExtractBalancedJsonObject（括號配對），不受內容影響，也讓兩處的擷取邏輯一致。
                            jsonYtCfg = ExtractBalancedJsonObject(jsonYtCfg);

                            break;
                        }
                    case EnumSet.DataType.Community:
                        // 使用正則表達式抓取 ytcfg.set({ ... }) 括號內的 JSON。
                        Match match = RegexYtCfgCommunity().Match(jsonYtCfg);

                        if (match.Success)
                        {
                            jsonYtCfg = match.Groups[1].Value;
                        }
                        else
                        {
                            // 如果正則失敗，至少也要檢查 IndexOf 是否抓到有效值。
                            int start = jsonYtCfg.IndexOf('{');
                            int end = jsonYtCfg.LastIndexOf('}');

                            if (start != -1 && end != -1)
                            {
                                jsonYtCfg = jsonYtCfg.Substring(start, end - start + 1);
                            }
                            else
                            {
                                LogMessages.Error(_logger, nameof(GetYTConfigDataAsync), "無法解析 ytcfg JSON 結構。");

                                return initialData;
                            }
                        }

                        break;
                }

                JsonElement jeYtCfg;

                try
                {
                    jeYtCfg = JsonSerializer.Deserialize<JsonElement>(jsonYtCfg);
                }
                catch (JsonException ex)
                {
                    LogMessages.Error(_logger, nameof(GetYTConfigDataAsync), $"解析 ytcfg JSON 失敗：{ex.GetExceptionMessage()}");

                    return initialData;
                }

                initialData.YTConfigData = ParseYtCfg(jeYtCfg);

                // 套用設定的語系。
                if (hasRegionData)
                {
                    initialData.YTConfigData.Gl = regionData?.Gl;
                    initialData.YTConfigData.Hl = regionData?.Hl;
                    initialData.YTConfigData.TimeZone = regionData?.TimeZone;
                }

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    LogMessages.Trace(
                        _logger,
                        nameof(GetYTConfigDataAsync),
                        RedactJsonProperty(jeYtCfg.GetRawText(), "ID_TOKEN", "SESSION_INDEX", "DATASYNC_ID", "DELEGATED_SESSION_ID"));
                }

                IElement? elementYtInitialData = dataType switch
                {
                    EnumSet.DataType.Community => scriptElements.FirstOrDefault(n => n.InnerHtml.Contains("var ytInitialData =")),
                    _ => scriptElements.FirstOrDefault(n => n.InnerHtml.Contains("window[\"ytInitialData\"] =")),
                };

                if (elementYtInitialData == null)
                {
                    LogMessages.Error(_logger, nameof(GetYTConfigDataAsync), "變數 \"elementYtInitialData\" 為 null！");

                    return initialData;
                }

                string jsonYtInitialData = dataType switch
                {
                    EnumSet.DataType.Community => elementYtInitialData.InnerHtml.Replace("var ytInitialData = ", string.Empty),
                    _ => elementYtInitialData.InnerHtml.Replace("window[\"ytInitialData\"] = ", string.Empty),
                };

                if (jsonYtInitialData.EndsWith(';'))
                {
                    jsonYtInitialData = jsonYtInitialData[0..^1];
                }

                JsonElement jeYtInitialData;

                try
                {
                    jeYtInitialData = JsonSerializer.Deserialize<JsonElement>(jsonYtInitialData);
                }
                catch (JsonException ex)
                {
                    LogMessages.Error(_logger, nameof(GetYTConfigDataAsync), $"解析 ytInitialData JSON 失敗：{ex.GetExceptionMessage()}");

                    return initialData;
                }

                switch (dataType)
                {
                    default:
                    case EnumSet.DataType.LiveChat:
                        {
                            // 2026/8 更新：popout 頁面（/live_chat?is_popout=1&v=）在絕大多數直播／重播下，
                            // 皆直接於 contents.liveChatRenderer 提供 continuation 與初始訊息，不需要額外的
                            // 中繼請求。但實測發現並非所有重播都適用：部分影片（例如聊天室在直播期間曾被
                            // 限制過的新人「初配信」）popout 頁面會回傳 contents.messageRenderer
                            // 「聊天室已停用」的假象，真正的聊天室其實還在，只是網頁版把它做成「需要按
                            // 『顯示聊天重播』重新載入」的狀態——continuation 只出現在 /watch 頁面內嵌的
                            // liveChatRenderer.continuations[0].reloadContinuationData，且後續必須改打
                            // get_live_chat_replay（而非一般輪詢用的 get_live_chat）。下方在 popout 頁面
                            // 拿不到 continuation 時，才 fallback 改讀 /watch 頁面，避免每次都多一次請求。
                            string[] continuationData = ParseStreamingContinuation(jeYtInitialData, liveChatOptions ?? new LiveChatStreamOptions());

                            initialData.YTConfigData.Continuation = continuationData[0];
                            initialData.Messages = ParseActions(jeYtInitialData);

                            if (string.IsNullOrEmpty(initialData.YTConfigData.Continuation))
                            {
                                LogMessages.Error(_logger, nameof(GetYTConfigDataAsync), "無法從 ytInitialData 取得 continuation 權杖，嘗試改用重新載入流程。");

                                string reloadContinuation = await GetReplayReloadContinuationAsync(videoIDorChannelID, cancellationToken).ConfigureAwait(false);

                                if (!string.IsNullOrEmpty(reloadContinuation))
                                {
                                    initialData.YTConfigData.Continuation = reloadContinuation;
                                    initialData.YTConfigData.IsReplayReload = true;
                                }
                                else
                                {
                                    LogMessages.Error(_logger, nameof(GetYTConfigDataAsync), "重新載入流程同樣無法取得 continuation 權杖，該影片的聊天室可能真的已被關閉。");
                                }
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
                LogMessages.HttpError(_logger, nameof(GetYTConfigDataAsync), httpResponseMessage.StatusCode.ToString(), htmlContent);
            }
        }

        return initialData;
    }

    /// <summary>
    /// 取得聊天室重播「重新載入」流程所需的 continuation
    /// <para>2026/8 新增：popout 聊天室頁面（/live_chat?is_popout=1）對部分重播影片會回傳
    /// contents.messageRenderer「聊天室已停用」的假象，實際聊天室仍存在，只是網頁版把它做成
    /// 需要按下「顯示聊天重播」才會重新載入的狀態。真正的 continuation 只出現在一般影片頁面
    /// （/watch）內嵌的 contents.twoColumnWatchNextResults.conversationBar.liveChatRenderer
    /// .continuations[0].reloadContinuationData，取得後須改打 get_live_chat_replay 端點
    /// （GetJsonElementAsync 依 YTConfigData.IsReplayReload 切換），而不是一般輪詢用的
    /// get_live_chat——已實測驗證：對「已停用」假象的重播影片，這條路徑能正常取得訊息。</para>
    /// </summary>
    /// <param name="videoID">字串，YouTube 影片的 ID 值</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task&lt;string&gt;，取得失敗時回傳空字串</returns>
    private async Task<string> GetReplayReloadContinuationAsync(string videoID, CancellationToken cancellationToken)
    {
        string url = $"{StringSet.Origin}/watch?v={videoID}";

        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, url);

        if (!string.IsNullOrEmpty(SharedCookies))
        {
            SetHttpRequestMessageHeader(httpRequestMessage);
        }

        LogMessages.Debug(_logger, nameof(GetReplayReloadContinuationAsync), GetRedactedRequestSummary(httpRequestMessage));

        HttpResponseMessage httpResponseMessage;

        try
        {
            httpResponseMessage = await SharedHttpClient!.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogMessages.Error(_logger, nameof(GetReplayReloadContinuationAsync), $"發送請求失敗：{ex.GetExceptionMessage()}");

            return string.Empty;
        }

        using (httpResponseMessage)
        {
            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                LogMessages.HttpError(
                    _logger,
                    nameof(GetReplayReloadContinuationAsync),
                    httpResponseMessage.StatusCode.ToString(),
                    await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));

                return string.Empty;
            }

            string htmlContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(htmlContent))
            {
                return string.Empty;
            }

            HtmlParser htmlParser = new();
            IHtmlDocument htmlDocument = htmlParser.ParseDocument(htmlContent);

            // /watch 頁面內嵌 ytInitialData 的方式跟 popout 頁面不同（少了 window[...] = 這層包裝），
            // 跟 Community 分頁的 var ytInitialData = 是同一種格式。
            IElement? elementYtInitialData = htmlDocument
                .QuerySelectorAll("script")
                .FirstOrDefault(n => n.InnerHtml.Contains("var ytInitialData ="));

            if (elementYtInitialData == null)
            {
                return string.Empty;
            }

            string jsonYtInitialData = elementYtInitialData.InnerHtml.Replace("var ytInitialData = ", string.Empty);

            if (jsonYtInitialData.EndsWith(';'))
            {
                jsonYtInitialData = jsonYtInitialData[0..^1];
            }

            JsonElement jeYtInitialData;

            try
            {
                jeYtInitialData = JsonSerializer.Deserialize<JsonElement>(jsonYtInitialData);
            }
            catch (JsonException ex)
            {
                LogMessages.Error(_logger, nameof(GetReplayReloadContinuationAsync), $"解析 ytInitialData JSON 失敗：{ex.GetExceptionMessage()}");

                return string.Empty;
            }

            JsonElement? liveChatRenderer = jeYtInitialData
                .Get("contents")
                ?.Get("twoColumnWatchNextResults")
                ?.Get("conversationBar")
                ?.Get("liveChatRenderer");

            JsonElement? firstContinuation = liveChatRenderer
                ?.Get("continuations")
                ?.ToArrayEnumerator()
                ?.Get(0);

            return firstContinuation
                ?.Get("reloadContinuationData")
                ?.Get("continuation")
                ?.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// 取得 JsonElement
    /// </summary>
    /// <param name="ytConfigData">YTConfigData</param>
    /// <param name="dataType">EnumSet.DataType，預設直為 DataType.LiveChat</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task&lt;JsonElement&gt;</returns>
    private async Task<JsonElement> GetJsonElementAsync(
        YTConfigData ytConfigData,
        EnumSet.DataType dataType = EnumSet.DataType.LiveChat,
        CancellationToken cancellationToken = default)
    {
        JsonElement jsonElement = new();

        // 2026/8 修正：get_live_chat_replay 並非「一律回傳 400」——先前的結論是拿一般輪詢用的
        // invalidationContinuationData 權杖去打這個端點才會 400，該端點真正吃的是
        // GetReplayReloadContinuationAsync 取得的 reloadContinuationData／回應內建的
        // liveChatReplayContinuationData 權杖。ytConfigData.IsReplayReload 只有在那條 fallback
        // 路徑被觸發時才會是 true，一般情況（popout 頁面就能取得 continuation）仍然使用
        // get_live_chat，行為不變。
        string methodName = ytConfigData?.IsReplayReload == true ? "get_live_chat_replay" : "get_live_chat";

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

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                LogMessages.Trace(_logger, nameof(GetJsonElementAsync), RedactJsonProperty(jsonContent, "visitorData"));
            }

            // 對 HTTP 429（限速）與暫時性網路例外（例如 Wi-Fi 瞬斷、DNS 短暫解析失敗）都做有限次數的重試，
            // 共用同一組嘗試次數預算：429 遵循伺服器回應的 Retry-After（沒有就用保守預設值），網路例外用
            // 遞增間隔。這是為了避免短暫的網路不穩直接讓長達數小時的擷取整場中止——重試前這裡完全沒有這層
            // 保護，任何非 429 的網路例外都會讓這次輪詢直接放棄，串流因此自然結束。重試次數用盡後才照原本
            // 邏輯記錄並回傳預設值，不做無限重試／指數退避。
            const int maxAttempts = 4;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                using HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, url);

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

                httpRequestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                LogMessages.Debug(_logger, nameof(GetJsonElementAsync), GetRedactedRequestSummary(httpRequestMessage));

                HttpResponseMessage httpResponseMessage;

                try
                {
                    httpResponseMessage = await SharedHttpClient!.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    if (attempt >= maxAttempts)
                    {
                        LogMessages.Error(
                            _logger,
                            nameof(GetJsonElementAsync),
                            $"發送請求失敗，已重試 {maxAttempts - 1} 次仍失敗，放棄這次輪詢：{ex.GetExceptionMessage()}");

                        return jsonElement;
                    }

                    TimeSpan networkRetryDelay = TimeSpan.FromSeconds(Math.Min(5 * attempt, 20));

                    LogMessages.Warning(
                        _logger,
                        nameof(GetJsonElementAsync),
                        $"發送請求失敗，將於 {networkRetryDelay.TotalSeconds:0} 秒後重試（第 {attempt}/{maxAttempts - 1} 次）：{ex.GetExceptionMessage()}");

                    if (!await DelayOrBreakAsync((int)networkRetryDelay.TotalMilliseconds, cancellationToken).ConfigureAwait(false))
                    {
                        return jsonElement;
                    }

                    continue;
                }

                using (httpResponseMessage)
                {
                    LogMessages.Debug(_logger, nameof(GetJsonElementAsync), httpResponseMessage.ToString());

                    if (httpResponseMessage.StatusCode == HttpStatusCode.TooManyRequests && attempt < maxAttempts)
                    {
                        TimeSpan retryAfter = TimeSpan.FromSeconds(10);

                        RetryConditionHeaderValue? retryAfterHeader = httpResponseMessage.Headers.RetryAfter;

                        if (retryAfterHeader?.Delta is { } delta)
                        {
                            retryAfter = delta;
                        }
                        else if (retryAfterHeader?.Date is { } date && date - DateTimeOffset.Now is { } untilDate && untilDate > TimeSpan.Zero)
                        {
                            retryAfter = untilDate;
                        }

                        LogMessages.Warning(
                            _logger,
                            nameof(GetJsonElementAsync),
                            $"收到 HTTP 429（Too Many Requests），將於 {retryAfter.TotalSeconds:0} 秒後重試一次。");

                        if (!await DelayOrBreakAsync((int)retryAfter.TotalMilliseconds, cancellationToken).ConfigureAwait(false))
                        {
                            return jsonElement;
                        }

                        continue;
                    }

                    string? receivedJsonContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                    if (string.IsNullOrEmpty(receivedJsonContent))
                    {
                        LogMessages.Error(_logger, nameof(GetJsonElementAsync), "變數 \"receivedJsonContent\" 為空白或是 null！");

                        return jsonElement;
                    }

                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        try
                        {
                            jsonElement = JsonSerializer.Deserialize<JsonElement>(receivedJsonContent);
                        }
                        catch (JsonException ex)
                        {
                            LogMessages.Error(_logger, nameof(GetJsonElementAsync), $"解析回應 JSON 失敗：{ex.GetExceptionMessage()}");
                        }
                    }
                    else
                    {
                        LogMessages.HttpError(
                            _logger,
                            nameof(GetJsonElementAsync),
                            httpResponseMessage.StatusCode.ToString(),
                            receivedJsonContent);
                    }

                    return jsonElement;
                }
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

    /// <summary>
    /// 正規表示式（社群頁面的 ytcfg.set({ ... }) 括號內的 JSON）
    /// </summary>
    /// <returns>Regex</returns>
    [GeneratedRegex(@"ytcfg\.set\s*\(\s*({.*?})\s*\)\s*;")]
    private static partial Regex RegexYtCfgCommunity();
}
