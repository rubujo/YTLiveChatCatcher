using NLog;
using System.Text.Json;
using YTApi.Extensions;
using YTApi.Models;
using YTLiveChatCatcher.Common.Sets;

namespace YTApi;

/// <summary>
/// YouTube 聊天室 JSON 解析器
/// <para>參考 1：https://takikomiprogramming.hateblo.jp/entry/2020/07/21/114851</para>
/// <para>參考 2：https://yasulab-pg.com/%E3%80%90python%E3%80%91youtube-live%E3%81%AE%E3%82%A2%E3%83%BC%E3%82%AB%E3%82%A4%E3%83%96%E3%81%8B%E3%82%89%E3%83%81%E3%83%A3%E3%83%83%E3%83%88%E3%82%92%E5%8F%96%E5%BE%97%E3%81%99%E3%82%8B/</para>
/// </summary>
public partial class JsonParser
{
    /// <summary>
    /// 解析 ytcfg 
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>YTConfigData</returns>
    public static YTConfigData ParseYtCfg(JsonElement? jsonElement)
    {
        bool useDelegatedSessionID = false;

        YTConfigData ytConfigData = new();

        if (jsonElement.HasValue)
        {
            JsonElement? jejeInnertubeApiKey = jsonElement?.Get("INNERTUBE_API_KEY");
            JsonElement? jeIDToken = jsonElement?.Get("ID_TOKEN");
            JsonElement? jeSessionIndex = jsonElement?.Get("SESSION_INDEX");
            JsonElement? jeInnertubeContextClientName = jsonElement?.Get("INNERTUBE_CONTEXT_CLIENT_NAME");
            JsonElement? jeInnertubeContextClientVersion = jsonElement?.Get("INNERTUBE_CONTEXT_CLIENT_VERSION");
            JsonElement? jeInnertubeClientVersion = jsonElement?.Get("INNERTUBE_CLIENT_VERSION");
            JsonElement? jeDataSyncID = jsonElement?.Get("DATASYNC_ID");
            JsonElement? jeDelegatedSessionID = jsonElement?.Get("DELEGATED_SESSION_ID");
            JsonElement? jeInnertubeContext = jsonElement?.Get("INNERTUBE_CONTEXT");
            JsonElement? jeClient = jeInnertubeContext?.Get("client");
            JsonElement? jeBrowserName = jeClient?.Get("browserName");
            JsonElement? jeBrowserVersion = jeClient?.Get("browserVersion");
            JsonElement? jeClientFormFactor = jeClient?.Get("clientFormFactor");
            JsonElement? jeClientName = jeClient?.Get("clientName");
            JsonElement? jeClientVersion = jeClient?.Get("clientVersion");
            JsonElement? jeDeviceMake = jeClient?.Get("deviceMake");
            JsonElement? jeDeviceModel = jeClient?.Get("deviceModel");
            JsonElement? jeGl = jeClient?.Get("gl");
            JsonElement? jeHl = jeClient?.Get("hl");
            JsonElement? jeOriginalUrl = jeClient?.Get("originalUrl");
            JsonElement? jeOsName = jeClient?.Get("osName");
            JsonElement? jeOsVersion = jeClient?.Get("osVersion");
            JsonElement? jePlatform = jeClient?.Get("platform");
            JsonElement? jeRemoteHost = jeClient?.Get("remoteHost");
            JsonElement? jeUserAgent = jeClient?.Get("userAgent");
            JsonElement? jeVisitorData = jeClient?.Get("visitorData");

            ytConfigData.APIKey = jejeInnertubeApiKey?.GetString();
            ytConfigData.IDToken = jeIDToken?.GetString();
            ytConfigData.SessionIndex = jeSessionIndex?.GetString();
            ytConfigData.InnetrubeContextClientName = jeInnertubeContextClientName?.GetInt32() ?? 0;
            ytConfigData.InnetrubeContextClientVersion = jeInnertubeContextClientVersion?.GetString();
            ytConfigData.InnetrubeClientVersion = jeInnertubeClientVersion?.GetString();
            ytConfigData.DataSyncID = jeDataSyncID?.GetString();
            ytConfigData.DelegatedSessionID = jeDelegatedSessionID?.GetString();
            ytConfigData.BrowserName = jeBrowserName?.GetString();
            ytConfigData.BrowserVersion = jeBrowserVersion?.GetString();
            ytConfigData.ClientFormFactor = jeClientFormFactor?.GetString();
            ytConfigData.ClientName = jeClientName?.GetString();
            ytConfigData.ClientVersion = jeClientVersion?.GetString();
            ytConfigData.DeviceMake = jeDeviceMake?.GetString();
            ytConfigData.DeviceModel = jeDeviceModel?.GetString();
            ytConfigData.Gl = jeGl?.GetString();
            ytConfigData.Hl = jeHl?.GetString();
            ytConfigData.OriginalUrl = jeOriginalUrl?.GetString();
            ytConfigData.OsName = jeOsName?.GetString();
            ytConfigData.OsVersion = jeOsVersion?.GetString();
            ytConfigData.Platform = jePlatform?.GetString();
            ytConfigData.RemoteHost = jeRemoteHost?.GetString();
            ytConfigData.UserAgent = jeUserAgent?.GetString();
            ytConfigData.VisitorData = jeVisitorData?.GetString();

            // 參考：https://github.com/xenova/chat-downloader/blob/master/chat_downloader/sites/youtube.py#L1629
            string[]? arrayDataSyncID = ytConfigData.DataSyncID
                ?.Split("||".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (arrayDataSyncID?.Length >= 2 && !string.IsNullOrEmpty(arrayDataSyncID[1]))
            {
                ytConfigData.DataSyncID = arrayDataSyncID[0];
            }
            else
            {
                useDelegatedSessionID = true;
            }

            if (useDelegatedSessionID)
            {
                ytConfigData.DataSyncID = ytConfigData.DelegatedSessionID;
            }
        }

        return ytConfigData;
    }

    /// <summary>
    /// 解析第一次的 continuation
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串陣列</returns>
    public static string[] ParseFirstTimeContinuation(JsonElement? jsonElement)
    {
        string[] output = new string[2];

        if (jsonElement.HasValue)
        {
            JsonElement.ArrayEnumerator? continuations = jsonElement?.Get("contents")
                ?.Get("liveChatRenderer")
                ?.Get("continuations")
                ?.ToArrayEnumerator();

            if (continuations.HasValue)
            {
                foreach (JsonElement singleContinuation in continuations)
                {
                    #region invalidationContinuationData

                    JsonElement? invalidationContinuationData = singleContinuation.Get("invalidationContinuationData");

                    if (invalidationContinuationData.HasValue)
                    {
                        JsonElement? continuation = invalidationContinuationData.Value.Get("continuation");

                        if (continuation.HasValue)
                        {
                            output[0] = continuation.Value.ToString();
                        }

                        JsonElement? timeoutMs = invalidationContinuationData.Value.Get("timeoutMs");

                        if (timeoutMs.HasValue)
                        {
                            output[1] = timeoutMs.Value.ToString();
                        }

                        break;
                    }

                    #endregion

                    #region timedContinuationData

                    JsonElement? timedContinuationData = singleContinuation.Get("timedContinuationData");

                    if (timedContinuationData.HasValue)
                    {
                        JsonElement? continuation = timedContinuationData.Value.Get("continuation");

                        if (continuation.HasValue)
                        {
                            output[0] = continuation.Value.ToString();
                        }

                        JsonElement? timeoutMs = timedContinuationData.Value.Get("timeoutMs");

                        if (timeoutMs.HasValue)
                        {
                            output[1] = timeoutMs.Value.ToString();
                        }

                        break;
                    }

                    #endregion

                    #region liveChatReplayContinuationData

                    JsonElement? liveChatReplayContinuationData = singleContinuation.Get("liveChatReplayContinuationData");

                    if (liveChatReplayContinuationData.HasValue)
                    {
                        JsonElement? continuation = liveChatReplayContinuationData.Value.Get("continuation");

                        if (continuation.HasValue)
                        {
                            output[0] = continuation.Value.ToString();

                            // 沒有 "timeoutMs"。
                            output[1] = string.Empty;
                        }

                        JsonElement? _ = liveChatReplayContinuationData.Value.Get("timeUntilLastMessageMsec");

                        break;
                    }

                    #endregion

                    #region playerSeekContinuationData

                    JsonElement? playerSeekContinuationData = singleContinuation.Get("playerSeekContinuationData");

                    if (playerSeekContinuationData.HasValue)
                    {
                        // 略過不進行任何的處理。

                        _logger.Debug("方法：GetFirstTimeContinuation() -> playerSeekContinuationData -> 略過不處理的內容：{Message}",
                             $"{Environment.NewLine}{playerSeekContinuationData.Value.GetRawText()}{Environment.NewLine}");
                    }

                    #endregion

                    // 尚未支援的內容。
                    _logger.Debug("方法：GetFirstTimeContinuation() -> 尚未支援的內容：{Message}",
                        $"{Environment.NewLine}{singleContinuation.GetRawText()}{Environment.NewLine}");
                }
            }
        }

        return output;
    }

    /// <summary>
    /// 解析 continuation
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串陣列</returns>
    public static string[] ParseContinuation(JsonElement? jsonElement)
    {
        string[] output = new string[2];

        if (jsonElement.HasValue)
        {
            JsonElement.ArrayEnumerator? continuations = jsonElement?.Get("continuationContents")
                ?.Get("liveChatContinuation")
                ?.Get("continuations")
                ?.ToArrayEnumerator();

            if (continuations.HasValue)
            {
                foreach (JsonElement singleContinuation in continuations)
                {
                    #region invalidationContinuationData

                    JsonElement? invalidationContinuationData = singleContinuation.Get("invalidationContinuationData");

                    if (invalidationContinuationData.HasValue)
                    {
                        JsonElement? continuation = invalidationContinuationData.Value.Get("continuation");

                        if (continuation.HasValue)
                        {
                            output[0] = continuation.Value.ToString();
                        }

                        JsonElement? timeoutMs = invalidationContinuationData.Value.Get("timeoutMs");

                        if (timeoutMs.HasValue)
                        {
                            output[1] = timeoutMs.Value.ToString();
                        }

                        break;
                    }

                    #endregion

                    #region timedContinuationData

                    JsonElement? timedContinuationData = singleContinuation.Get("timedContinuationData");

                    if (timedContinuationData.HasValue)
                    {
                        JsonElement? continuation = timedContinuationData.Value.Get("continuation");

                        if (continuation.HasValue)
                        {
                            output[0] = continuation.Value.ToString();
                        }

                        JsonElement? timeoutMs = timedContinuationData.Value.Get("timeoutMs");

                        if (timeoutMs.HasValue)
                        {
                            output[1] = timeoutMs.Value.ToString();
                        }

                        break;
                    }

                    #endregion

                    #region liveChatReplayContinuationData

                    JsonElement? liveChatReplayContinuationData = singleContinuation.Get("liveChatReplayContinuationData");

                    if (liveChatReplayContinuationData.HasValue)
                    {
                        JsonElement? continuation = liveChatReplayContinuationData.Value.Get("continuation");

                        if (continuation.HasValue)
                        {
                            output[0] = continuation.Value.ToString();

                            // 沒有 "timeoutMs"。
                            output[1] = string.Empty;
                        }

                        JsonElement? _ = liveChatReplayContinuationData.Value.Get("timeUntilLastMessageMsec");

                        break;
                    }

                    #endregion

                    #region playerSeekContinuationData

                    JsonElement? playerSeekContinuationData = singleContinuation.Get("playerSeekContinuationData");

                    if (playerSeekContinuationData.HasValue)
                    {
                        // 略過不進行任何的處理。

                        _logger.Debug("方法：GetContinuation() -> playerSeekContinuationData -> 略過不處理的內容：{Message}",
                             $"{Environment.NewLine}{playerSeekContinuationData.Value.GetRawText()}{Environment.NewLine}");
                    }

                    #endregion

                    // 尚未支援的內容。
                    _logger.Debug("方法：GetContinuation() -> 尚未支援的內容：{Message}",
                        $"{Environment.NewLine}{singleContinuation.GetRawText()}{Environment.NewLine}");
                }
            }
        }

        return output;
    }

    /// <summary>
    /// 解析重播時的 continuation
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    public static string ParseReplayContinuation(JsonElement? jsonElement)
    {
        string output = string.Empty;

        if (jsonElement.HasValue)
        {
            JsonElement? liveChatRenderer = jsonElement?.Get("contents")
                ?.Get("twoColumnWatchNextResults")
                ?.Get("conversationBar")
                ?.Get("liveChatRenderer");

            if (liveChatRenderer.HasValue)
            {
                JsonElement.ArrayEnumerator? subMenuItems = liveChatRenderer?.Get("header")
                    ?.Get("liveChatHeaderRenderer")
                    ?.Get("viewSelector")
                    ?.Get("sortFilterSubMenuRenderer")
                    ?.Get("subMenuItems")
                    ?.ToArrayEnumerator();

                if (subMenuItems.HasValue)
                {
                    foreach (JsonElement subMenuItem in subMenuItems)
                    {
                        JsonElement? title = subMenuItem.Get("title");

                        // "StringSet.TitleHotReplay" 與 "StringSet.TitleReplay" 的結構是一樣的。
                        if (title.HasValue && title.Value.GetString() == StringSet.TitleReplay)
                        {
                            JsonElement? continuation = subMenuItem.Get("continuation")
                                ?.Get("reloadContinuationData")
                                ?.Get("continuation");

                            if (continuation.HasValue)
                            {
                                output = continuation.Value.GetString() ?? string.Empty;

                                break;
                            }
                        }
                    }
                }

                // 當從 "subMenuItem" 取不到 "continuation" 時，
                // 才使用此處的 "continuation"。
                if (string.IsNullOrEmpty(output))
                {
                    JsonElement.ArrayEnumerator? continuations = liveChatRenderer
                        ?.Get("continuations")
                        ?.ToArrayEnumerator();

                    if (continuations.HasValue)
                    {
                        foreach (JsonElement singleContinuation in continuations)
                        {
                            // 用此 "continuation" 可能無法抓到全部的訊息。
                            JsonElement? continuation = singleContinuation
                                .Get("reloadContinuationData")
                                ?.Get("continuation");

                            if (continuation.HasValue)
                            {
                                output = continuation.Value.GetString() ?? string.Empty;

                                break;
                            }
                        }
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// 解析 action 的內容
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>List&lt;RendererData&gt;</returns>
    public static List<RendererData> ParseActions(JsonElement? jsonElement, bool isLarge = true)
    {
        List<RendererData> output = new();

        if (jsonElement.HasValue)
        {
            JsonElement.ArrayEnumerator? actions = jsonElement.Value.Get("continuationContents")
                ?.Get("liveChatContinuation")
                ?.Get("actions")
                ?.ToArrayEnumerator();

            if (!actions.HasValue)
            {
                // 若是直播中的影片時，剛載入頁面就影片聊天室的內容，這些資料也需要處理。
                actions = jsonElement.Value.Get("contents")
                    ?.Get("liveChatRenderer")
                    ?.Get("actions")
                    ?.ToArrayEnumerator();
            }

            if (actions.HasValue)
            {
                foreach (JsonElement singleAction in actions)
                {
                    // TODO: 2023-05-29 測試如何解析 addBannerToLiveChatCommand。
                    _logger.Debug("singleAction");
                    _logger.Debug(singleAction);

                    JsonElement? item = singleAction.Get("addChatItemAction")?.Get("item");

                    if (item.HasValue)
                    {
                        output.AddRange(ParseRenderer(item.Value, isLarge));
                    }

                    // TODO: 2023-05-29 未測試，不確定是否有效。。
                    JsonElement? singleBannerRenderer = singleAction
                        .Get("addBannerToLiveChatCommand")
                        ?.Get("bannerRenderer");

                    if (singleBannerRenderer.HasValue)
                    {
                        output.AddRange(ParseRenderer(singleBannerRenderer.Value, isLarge));
                    }

                    JsonElement? videoOffsetTimeMsec = singleAction
                        .Get("addChatItemAction")
                        ?.Get("videoOffsetTimeMsec");

                    string videoOffsetTimeText = GetVideoOffsetTimeMsec(videoOffsetTimeMsec);

                    JsonElement.ArrayEnumerator? replayActions = singleAction
                        .Get("replayChatItemAction")
                        ?.Get("actions")
                        ?.ToArrayEnumerator();

                    if (replayActions.HasValue)
                    {
                        foreach (JsonElement replayAction in replayActions)
                        {
                            JsonElement? replayItem = replayAction.Get("addChatItemAction")?.Get("item");

                            if (replayItem.HasValue)
                            {
                                List<RendererData> rendererDatas = ParseRenderer(replayItem.Value, isLarge);

                                foreach (RendererData rendererData in rendererDatas)
                                {
                                    if (string.IsNullOrEmpty(rendererData.TimestampText) &&
                                        string.IsNullOrEmpty(rendererData.TimestampUsec))
                                    {
                                        rendererData.TimestampText = videoOffsetTimeText;
                                    }
                                }

                                output.AddRange(rendererDatas);
                            }

                            JsonElement? replayBannerRenderer = replayAction
                                .Get("addBannerToLiveChatCommand")
                                ?.Get("bannerRenderer");

                            if (replayBannerRenderer.HasValue)
                            {
                                List<RendererData> rendererDatas = ParseRenderer(replayBannerRenderer.Value, isLarge);

                                foreach (RendererData rendererData in rendererDatas)
                                {
                                    if ((string.IsNullOrEmpty(rendererData.TimestampText) &&
                                        string.IsNullOrEmpty(rendererData.TimestampUsec)) ||
                                        rendererData.TimestampText == StringSet.NoTimestampText)
                                    {
                                        rendererData.TimestampText = videoOffsetTimeText;
                                    }
                                }

                                output.AddRange(rendererDatas);
                            }
                        }
                    }

                    // TODO: 2023-05-29 用於取得 replaceChatItemAction 使用。
                    JsonElement? replaceAction = singleAction
                        .Get("replaceChatItemAction");

                    if (replaceAction.HasValue)
                    {
                        _logger.Debug("replaceChatItemAction");
                        _logger.Debug(replaceAction);
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// 解析 *Renderer 的內容
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>List&lt;RendererData&gt;</returns>
    public static List<RendererData> ParseRenderer(JsonElement jsonElement, bool isLarge = true)
    {
        List<RendererData> output = new();

        // 參考：https://github.com/xenova/chat-downloader/blob/master/chat_downloader/sites/youtube.py#L969
        if (jsonElement.TryGetProperty(
            "liveChatTextMessageRenderer",
            out JsonElement liveChatTextMessageRenderer))
        {
            SetRendererData(
                logger: _logger,
                dataSet: output,
                jsonElement: liveChatTextMessageRenderer,
                rendererName: "liveChatTextMessageRenderer",
                isLarge: isLarge);
        }
        else if (jsonElement.TryGetProperty(
            "liveChatPaidMessageRenderer",
            out JsonElement liveChatPaidMessageRenderer))
        {
            SetRendererData(
                logger: _logger,
                dataSet: output,
                jsonElement: liveChatPaidMessageRenderer,
                rendererName: "liveChatPaidMessageRenderer",
                isLarge: isLarge);
        }
        else if (jsonElement.TryGetProperty(
            "liveChatPaidStickerRenderer",
            out JsonElement liveChatPaidStickerRenderer))
        {
            SetRendererData(
                logger: _logger,
                dataSet: output,
                jsonElement: liveChatPaidStickerRenderer,
                rendererName: "liveChatPaidStickerRenderer",
                isLarge: isLarge);
        }
        else if (jsonElement.TryGetProperty(
            "liveChatMembershipItemRenderer",
            out JsonElement liveChatMembershipItemRenderer))
        {
            SetRendererData(
                logger: _logger,
                dataSet: output,
                jsonElement: liveChatMembershipItemRenderer,
                rendererName: "liveChatMembershipItemRenderer",
                isLarge: isLarge);
        }
        else if (jsonElement.TryGetProperty(
            "liveChatViewerEngagementMessageRenderer",
            out JsonElement liveChatViewerEngagementMessageRenderer))
        {
            SetRendererData(
                logger: _logger,
                dataSet: output,
                jsonElement: liveChatViewerEngagementMessageRenderer,
                rendererName: "liveChatViewerEngagementMessageRenderer",
                isLarge: isLarge);
        }
        else if (jsonElement.TryGetProperty(
            "liveChatModeChangeMessageRenderer",
            out JsonElement liveChatModeChangeMessageRenderer))
        {
            SetRendererData(
                logger: _logger,
                dataSet: output,
                jsonElement: liveChatModeChangeMessageRenderer,
                rendererName: "liveChatModeChangeMessageRenderer",
                isLarge: isLarge);
        }
        else if (jsonElement.TryGetProperty(
            "liveChatSponsorshipsGiftPurchaseAnnouncementRenderer",
            out JsonElement liveChatSponsorshipsGiftPurchaseAnnouncementRenderer))
        {
            SetRendererData(
                logger: _logger,
                dataSet: output,
                jsonElement: liveChatSponsorshipsGiftPurchaseAnnouncementRenderer,
                rendererName: "liveChatSponsorshipsGiftPurchaseAnnouncementRenderer",
                isLarge: isLarge);
        }
        else if (jsonElement.TryGetProperty(
            "liveChatSponsorshipsGiftRedemptionAnnouncementRenderer",
            out JsonElement liveChatSponsorshipsGiftRedemptionAnnouncementRenderer))
        {
            SetRendererData(
                logger: _logger,
                dataSet: output,
                jsonElement: liveChatSponsorshipsGiftRedemptionAnnouncementRenderer,
                rendererName: "liveChatSponsorshipsGiftRedemptionAnnouncementRenderer",
                isLarge: isLarge);
        }
        else if (jsonElement.TryGetProperty(
            "liveChatBannerRenderer",
            out JsonElement liveChatBannerRenderer))
        {
            _logger.Debug("liveChatBannerRenderer");
            _logger.Debug(liveChatBannerRenderer);

            // TODO: 2023-05-29 有插入時間順序的問題。
            if (liveChatBannerRenderer.TryGetProperty(
                "header",
                out JsonElement header))
            {
                _logger.Debug("liveChatBannerRenderer -> header");
                _logger.Debug(header);

                if (header.TryGetProperty(
                    "liveChatBannerHeaderRenderer",
                    out JsonElement liveChatBannerHeaderRenderer))
                {
                    SetRendererData(
                        logger: _logger,
                        dataSet: output,
                        jsonElement: liveChatBannerHeaderRenderer,
                        rendererName: "liveChatBannerHeaderRenderer",
                        customRendererName: "liveChatBannerRenderer -> header -> liveChatBannerHeaderRenderer",
                        isLarge: isLarge);
                }
            }

            if (liveChatBannerRenderer.TryGetProperty(
                "contents",
                out JsonElement contents))
            {
                _logger.Debug("liveChatBannerRenderer -> contents");
                _logger.Debug(contents);

                if (contents.TryGetProperty(
                    "liveChatTextMessageRenderer",
                    out JsonElement liveChatTextMessageRenderer1))
                {
                    SetRendererData(
                       logger: _logger,
                       dataSet: output,
                       jsonElement: liveChatTextMessageRenderer1,
                       rendererName: "liveChatTextMessageRenderer",
                       customRendererName: "liveChatBannerRenderer -> contents -> liveChatTextMessageRenderer",
                       isLarge: isLarge);
                }
                else if (contents.TryGetProperty(
                    "liveChatBannerRedirectRenderer",
                    out JsonElement liveChatBannerRedirectRenderer))
                {
                    SetRendererData(
                        _logger,
                        output,
                        liveChatBannerRedirectRenderer,
                        "liveChatBannerRedirectRenderer",
                        "liveChatBannerRenderer -> contents -> liveChatBannerRedirectRenderer",
                        isLarge);
                }
            }
        }
        else if (jsonElement.TryGetProperty("liveChatTickerPaidMessageItemRenderer", out _) ||
            jsonElement.TryGetProperty("liveChatTickerPaidStickerItemRenderer", out _) ||
            jsonElement.TryGetProperty("liveChatTickerSponsorItemRenderer", out _) ||
            jsonElement.TryGetProperty("liveChatPlaceholderItemRenderer", out _) ||
            jsonElement.TryGetProperty("liveChatDonationAnnouncementRenderer", out _) ||
            jsonElement.TryGetProperty("liveChatPurchasedProductMessageRenderer", out _) ||
            jsonElement.TryGetProperty("liveChatLegacyPaidMessageRenderer", out _) ||
            jsonElement.TryGetProperty("liveChatModerationMessageRenderer", out _) ||
            jsonElement.TryGetProperty("liveChatAutoModMessageRenderer", out _))
        {
            // 略過進不行任何處理。
            // 參考：https://taiyakisun.hatenablog.com/entry/2020/10/13/223443

            _logger.Debug("方法：GetRenderer() -> 略過不處理的內容：{Message}",
                $"{Environment.NewLine}{jsonElement.GetRawText()}{Environment.NewLine}");
        }
        else
        {
            _logger.Debug("方法：GetRenderer() -> 尚未支援的內容：{Message}",
                $"{Environment.NewLine}{jsonElement.GetRawText()}{Environment.NewLine}");
        }

        return output;
    }

    /// <summary>
    /// 解析 authorBadges
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>AuthorBadgesData</returns>
    public static AuthorBadgesData ParseAuthorBadges(JsonElement jsonElement, bool isLarge = true)
    {
        AuthorBadgesData output = new();

        JsonElement.ArrayEnumerator? authorBadges = jsonElement
            .Get("authorBadges")
            ?.ToArrayEnumerator();

        if (authorBadges.HasValue)
        {
            List<BadgeData> tempBadges = new();

            foreach (JsonElement singleAuthorBadge in authorBadges)
            {
                BadgeData badgeData = new();

                // 自定義預覽圖。
                JsonElement? customThumbnail = singleAuthorBadge
                    .Get("liveChatAuthorBadgeRenderer")
                    ?.Get("customThumbnail");

                if (customThumbnail.HasValue)
                {
                    badgeData.Url = GetThumbnailUrl(customThumbnail, isLarge);
                }

                // 圖示類型。
                JsonElement? iconType = singleAuthorBadge.Get("liveChatAuthorBadgeRenderer")
                    ?.Get("icon")
                    ?.Get("iconType");

                if (iconType.HasValue)
                {
                    badgeData.IconType = iconType.Value.GetString();
                }

                // 工具提示。
                JsonElement? tooltip = singleAuthorBadge.Get("liveChatAuthorBadgeRenderer")
                    ?.Get("tooltip");

                if (tooltip.HasValue)
                {
                    badgeData.Tooltip = tooltip.Value.GetString();
                }

                // 標籤。
                JsonElement? label = singleAuthorBadge.Get("liveChatAuthorBadgeRenderer")
                    ?.Get("accessibility")
                    ?.Get("accessibilityData")
                    ?.Get("label");

                if (label.HasValue)
                {
                    badgeData.Label = label.Value.GetString();
                }

                tempBadges.Add(badgeData);
            }

            output.Text = tempBadges.GetBadgeName();
            output.Badges = tempBadges;
        }

        return output;
    }

    /// <summary>
    /// 解析 Message 資料
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>MessageData</returns>
    public static MessageData ParseMessageData(JsonElement jsonElement, bool isLarge = true)
    {
        MessageData output = new();

        string tempText = string.Empty;
        string? tempTextColor = string.Empty,
            tempFontFace = string.Empty;

        bool isBold = false;

        List<StickerData> tempStickers = new();
        List<EmojiData> tempEmojis = new();

        JsonElement? headerPrimaryText = jsonElement.Get("headerPrimaryText");

        if (headerPrimaryText.HasValue)
        {
            RunsData runsData = ParseRunData(headerPrimaryText.Value, isLarge);

            tempText += $" [{runsData.Text}] ";

            isBold = runsData.Bold ?? false;
            tempTextColor = runsData.TextColor;
            tempFontFace = runsData.FontFace;

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        // "headerSubtext" 的 "simpleText"。
        JsonElement? headerSubtext = jsonElement.Get("headerSubtext");

        if (headerSubtext.HasValue)
        {
            // "headerSubtext" 的 "simpleText"。
            JsonElement? simpleText = jsonElement.Get("headerSubtext")
                ?.Get("simpleText");

            if (simpleText.HasValue)
            {
                // 手動在前後補一個空白跟 []。
                tempText += $" [{simpleText.Value}] ";
            }

            RunsData runsData = ParseRunData(headerSubtext.Value, isLarge);

            tempText += $" {runsData.Text} ";

            isBold = runsData.Bold ?? false;
            tempTextColor = runsData.TextColor;
            tempFontFace = runsData.FontFace;

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        JsonElement? primaryText = jsonElement.Get("primaryText");

        if (primaryText.HasValue)
        {
            RunsData runsData = ParseRunData(primaryText.Value, isLarge);

            tempText += runsData.Text;

            isBold = runsData.Bold ?? false;
            tempTextColor = runsData.TextColor;
            tempFontFace = runsData.FontFace;

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        JsonElement? text = jsonElement.Get("text");

        if (text.HasValue)
        {
            RunsData runsData = ParseRunData(text.Value, isLarge);

            tempText += runsData.Text;

            isBold = runsData.Bold ?? false;
            tempTextColor = runsData.TextColor;
            tempFontFace = runsData.FontFace;

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        JsonElement? subtext = jsonElement.Get("subtext");

        if (subtext.HasValue)
        {
            RunsData runsData = ParseRunData(subtext.Value, isLarge);

            tempText += runsData.Text;

            isBold = runsData.Bold ?? false;
            tempTextColor = runsData.TextColor;
            tempFontFace = runsData.FontFace;

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        JsonElement? sticker = jsonElement.Get("sticker");

        if (sticker.HasValue)
        {
            if (!string.IsNullOrEmpty(sticker?.ToString()))
            {
                StickerData stickerData = new();

                // "sticker" 的 "label"。
                JsonElement? label = sticker
                    ?.Get("accessibility")
                    ?.Get("accessibilityData")
                    ?.Get("label");

                if (label.HasValue)
                {
                    tempText += $":{label?.GetString()}:" ?? string.Empty;
                }

                // 是第一次購買超級留言或貼圖才會有。
                JsonElement? content = jsonElement.Get("lowerBumper")
                    ?.Get("liveChatItemBumperViewModel")
                    ?.Get("content")
                    ?.Get("bumperUserEduContentViewModel")
                    ?.Get("text")
                    ?.Get("content");

                if (content.HasValue)
                {
                    // 手動在前後補一個空白跟 []。
                    tempText += $" [{content?.GetString()}] " ?? string.Empty;
                }

                stickerData.ID = label.HasValue ? label?.GetString() : string.Empty;
                stickerData.Url = GetThumbnailUrl(sticker, isLarge);
                stickerData.Text = label.HasValue ? $":{label?.GetString()}:" : string.Empty;
                stickerData.Label = label.HasValue ? label?.GetString() : string.Empty;

                _logger.Debug($"方法：GetRunData() -> sticker -> 除錯用的內容：" +
                    $"{Environment.NewLine}{sticker?.GetRawText()}{Environment.NewLine}");

                tempStickers.Add(stickerData);
            }
        }

        // "purchaseAmountText" 的 "simpleText"。
        JsonElement? purchaseAmountText = jsonElement.Get("purchaseAmountText")
            ?.Get("simpleText");

        if (purchaseAmountText.HasValue)
        {
            // 手動在前後補一個空白跟 []。
            tempText += $" [{purchaseAmountText.Value}] ";
        }

        JsonElement? message = jsonElement.Get("message");

        if (message.HasValue)
        {
            RunsData runsData = ParseRunData(message.Value, isLarge);

            tempText += runsData.Text;

            isBold = runsData.Bold ?? false;
            tempTextColor = runsData.TextColor;
            tempFontFace = runsData.FontFace;

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        JsonElement? bannerMessage = jsonElement.Get("bannerMessage");

        if (bannerMessage.HasValue)
        {
            RunsData runsData = ParseRunData(bannerMessage.Value, isLarge);

            tempText += runsData.Text;

            isBold = runsData.Bold ?? false;
            tempTextColor = runsData.TextColor;
            tempFontFace = runsData.FontFace;

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        _logger.Debug($"方法：GetMessage() -> 除錯用的內容：" +
            $"{Environment.NewLine}{jsonElement.GetRawText()}{Environment.NewLine}");

        if (string.IsNullOrEmpty(tempText))
        {
            tempText = StringSet.NoMessageContent;
        }

        output.Text = tempText;
        output.Bold = isBold;
        output.TextColor = tempTextColor;
        output.FontFace = tempFontFace;
        output.Stickers = tempStickers;
        output.Emojis = tempEmojis;

        return output;
    }

    /// <summary>
    /// 解析 runs 資料
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>RunsData</returns>
    public static RunsData ParseRunData(JsonElement jsonElement, bool isLarge = true)
    {
        RunsData output = new();

        JsonElement.ArrayEnumerator? runs = jsonElement
            .Get("runs")
            ?.ToArrayEnumerator();

        if (runs.HasValue)
        {
            string tempText = string.Empty,
                tempTextColor = string.Empty,
                tempFontFace = string.Empty;

            bool isBold = false;

            List<EmojiData> tempEmojis = new();

            foreach (JsonElement singleRun in runs)
            {
                JsonElement? text = singleRun.Get("text");

                if (text.HasValue)
                {
                    tempText += text?.GetString();
                }

                JsonElement? bold = singleRun.Get("bold");

                if (text.HasValue)
                {
                    isBold = bold?.GetBoolean() ?? false;
                }

                JsonElement? textColor = singleRun.Get("textColor");

                if (textColor.HasValue)
                {
                    tempTextColor += GetColorHexCode(textColor.Value.GetInt64());
                }

                JsonElement? fontFace = singleRun.Get("fontFace");

                if (fontFace.HasValue)
                {
                    tempFontFace += fontFace?.GetString();
                }

                JsonElement? emoji = singleRun.Get("emoji");

                if (emoji.HasValue)
                {
                    if (!string.IsNullOrEmpty(emoji?.ToString()))
                    {
                        EmojiData emojiData = new();

                        JsonElement? emojiId = emoji?.Get("emojiId");

                        emojiData.ID = emojiId.HasValue ? emojiId?.GetString() : string.Empty;

                        // "image" 的 "thumbnails"。
                        JsonElement? image = emoji?.Get("image");

                        emojiData.Url = GetThumbnailUrl(image, isLarge);

                        // "image" 的 "label"。
                        JsonElement? label = image
                            ?.Get("accessibility")
                            ?.Get("accessibilityData")
                            ?.Get("label");

                        if (label.HasValue)
                        {
                            // 仿 "shortcuts" 以利人工辨識。
                            tempText += $" :{label?.GetString()}: ";
                        }

                        // 取 "shortcuts" 的第一個值。
                        JsonElement.ArrayEnumerator? shortcuts = emoji
                            ?.Get("shortcuts")
                            ?.ToArrayEnumerator();

                        if (shortcuts?.Any() == true)
                        {
                            // 只取第一個。
                            emojiData.Text = $" {shortcuts?.ElementAtOrDefault(0).GetString()} ";
                        }

                        // 2023-08-17 因為部分 "emoji" 的 "label" 也是 "emoji" 本身，所以改回取 "shortcuts" 的值。
                        //stickerData.Text = label.HasValue ? $":{label?.GetString()}:" : string.Empty;
                        emojiData.Label = label.HasValue ? label?.GetString() : string.Empty;

                        JsonElement? isCustomEmoji = emoji?.Get("isCustomEmoji");

                        if (isCustomEmoji.HasValue)
                        {
                            emojiData.IsCustomEmoji = isCustomEmoji?.GetBoolean() ?? false;
                        }

                        _logger.Debug($"方法：GetRunData() -> emoji -> 除錯用的內容：" +
                            $"{Environment.NewLine}{emoji?.GetRawText()}{Environment.NewLine}");

                        tempEmojis.Add(emojiData);
                    }
                }

                _logger.Debug($"方法：GetRunData() -> 除錯用的內容：" +
                    $"{Environment.NewLine}{singleRun.GetRawText()}{Environment.NewLine}");
            }

            output.Text = tempText;
            output.Bold = isBold;
            output.TextColor = tempTextColor;
            output.FontFace = tempFontFace;
            output.Emojis = tempEmojis;
        }

        return output;
    }

    /// <summary>
    /// 設定 RendererData
    /// </summary>
    /// <param name="logger">NLog 的 Logger</param>
    /// <param name="dataSet">List&lt;RendererData&gt;</param>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="rendererName">字串，*Renderer 的名稱，預設值為空白</param>
    /// <param name="customRendererName">字串，自定義 *Renderer 的名稱，預設值為空白</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    public static void SetRendererData(
        Logger logger,
        List<RendererData> dataSet,
        JsonElement jsonElement,
        string rendererName = "",
        string customRendererName = "",
        bool isLarge = true)
    {
        if (!string.IsNullOrEmpty(customRendererName))
        {
            logger.Debug(customRendererName);
        }
        else
        {
            logger.Debug(rendererName);
        }

        logger.Debug(jsonElement);

        AuthorBadgesData authorBadgesData = ParseAuthorBadges(jsonElement, isLarge);
        MessageData messageData = ParseMessageData(jsonElement, isLarge);

        string id = GetID(jsonElement),
            type = GetRendererDataType(rendererName),
            timestampUsec = GetTimestampUsec(jsonElement),
            authorName = GetAuthorName(jsonElement),
            authorPhoto = GetAuthorPhoto(jsonElement, isLarge),
            authorBadges = authorBadgesData.Text ?? StringSet.NoAuthorBadges,
            message = messageData.Text ?? StringSet.NoMessageContent,
            purchaseAmountText = GetPurchaseAmountText(jsonElement),
            forgroundColor = messageData?.TextColor ?? string.Empty,
            backgroundColor = GetBackgroundColor(jsonElement),
            timestampText = GetTimestampText(jsonElement),
            authorExternalChannelID = GetAuthorExternalChannelId(jsonElement);

        #region 處理特例

        if (type == StringSet.YouTube)
        {
            authorName = $"[{type}]";
        }

        if (rendererName == "liveChatMembershipItemRenderer")
        {
            // 此處 message 為 headerSubtext。
            if (message.Contains(StringSet.MemberUpgrade))
            {
                type = StringSet.ChatMemberUpgrade;
            }
            else if (message.Contains(StringSet.MemberMilestone))
            {
                type = StringSet.ChatMemberMilestone;
            }
            else
            {
                // 不進行任何處理。
            }
        }
        else if (rendererName == "liveChatSponsorshipsGiftPurchaseAnnouncementRenderer")
        {
            JsonElement? liveChatSponsorshipsHeaderRenderer = jsonElement
                .Get("header")
                ?.Get("liveChatSponsorshipsHeaderRenderer");

            if (liveChatSponsorshipsHeaderRenderer.HasValue)
            {
                authorBadgesData = ParseAuthorBadges(liveChatSponsorshipsHeaderRenderer.Value, isLarge);
                messageData = ParseMessageData(liveChatSponsorshipsHeaderRenderer.Value, isLarge);

                authorName = GetAuthorName(liveChatSponsorshipsHeaderRenderer.Value);
                authorPhoto = GetAuthorPhoto(liveChatSponsorshipsHeaderRenderer.Value, isLarge);
                authorBadges = authorBadgesData.Text ?? StringSet.NoAuthorBadges;
                // 此處 message 為 primaryText。
                message = messageData.Text ?? StringSet.NoMessageContent;
            }
        }
        else
        {
            // 不進行任何處理。
        }

        #endregion

        dataSet.Add(new RendererData()
        {
            ID = id,
            Type = type,
            TimestampUsec = timestampUsec,
            AuthorName = authorName,
            AuthorBadges = authorBadges,
            AuthorPhotoUrl = authorPhoto,
            MessageContent = message,
            PurchaseAmountText = purchaseAmountText,
            ForegroundColor = forgroundColor,
            BackgroundColor = backgroundColor,
            TimestampText = timestampText,
            AuthorExternalChannelID = authorExternalChannelID,
            Stickers = messageData?.Stickers,
            Emojis = messageData?.Emojis,
            Badges = authorBadgesData?.Badges
        });
    }
}