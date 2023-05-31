﻿using NLog;
using System.Text.Json;
using YTApi.Extensions;
using YTApi.Models;
using YTLiveChatCatcher.Common.Sets;
using YTLiveChatCatcher.Properties;

namespace YTApi;

/// <summary>
/// YouTube 聊天室 JSON 解析器
/// <para>參考 1：https://takikomiprogramming.hateblo.jp/entry/2020/07/21/114851</para>
/// <para>參考 2：https://yasulab-pg.com/%E3%80%90python%E3%80%91youtube-live%E3%81%AE%E3%82%A2%E3%83%BC%E3%82%AB%E3%82%A4%E3%83%96%E3%81%8B%E3%82%89%E3%83%81%E3%83%A3%E3%83%83%E3%83%88%E3%82%92%E5%8F%96%E5%BE%97%E3%81%99%E3%82%8B/</para>
/// </summary>
public partial class JsonParser
{
    /// <summary>
    /// 取得 continuation
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串陣列</returns>
    public static string[] GetContinuation(JsonElement? jsonElement)
    {
        string[] output = new string[2];

        if (jsonElement.HasValue)
        {
            JsonElement? continuations = jsonElement.Value.Get("continuationContents")
                ?.Get("liveChatContinuation")
                ?.Get("continuations");

            if (continuations.HasValue &&
                continuations.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement singleContinuation in continuations.Value.EnumerateArray())
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

                        if (Settings.Default.EnableDebug)
                        {
                            _logger.Debug("方法：GetContinuation() -> playerSeekContinuationData -> 略過不處理的內容：{Message}",
                                $"{Environment.NewLine}{playerSeekContinuationData.Value.GetRawText()}{Environment.NewLine}");
                        }
                    }

                    #endregion

                    if (Settings.Default.EnableDebug)
                    {
                        // 尚未支援的內容。
                        _logger.Debug("方法：GetContinuation() -> 尚未支援的內容：{Message}",
                            $"{Environment.NewLine}{singleContinuation.GetRawText()}{Environment.NewLine}");
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// 取得重播時的 continuation
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    public static string GetReplayContinuation(JsonElement jsonElement)
    {
        string output = string.Empty;

        JsonElement? liveChatRenderer = jsonElement.Get("contents")
            ?.Get("twoColumnWatchNextResults")
            ?.Get("conversationBar")
            ?.Get("liveChatRenderer");

        if (liveChatRenderer.HasValue)
        {
            JsonElement? subMenuItems = liveChatRenderer?.Get("header")
                ?.Get("liveChatHeaderRenderer")
                ?.Get("viewSelector")
                ?.Get("sortFilterSubMenuRenderer")
                ?.Get("subMenuItems");

            if (subMenuItems.HasValue &&
                subMenuItems.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement subMenuItem in subMenuItems.Value.EnumerateArray())
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
                JsonElement? continuations = liveChatRenderer?.Get("continuations");

                if (continuations.HasValue &&
                    continuations.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement singleContinuation in continuations.Value.EnumerateArray())
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

        return output;
    }

    /// <summary>
    /// 取得 action 的內容
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>List&lt;RendererData&gt;</returns>
    public static List<RendererData> GetActions(JsonElement? jsonElement, bool isLarge = true)
    {
        List<RendererData> output = new();

        if (jsonElement.HasValue)
        {
            JsonElement? actions = jsonElement.Value.Get("continuationContents")
                ?.Get("liveChatContinuation")
                ?.Get("actions");

            if (actions.HasValue &&
                actions.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement singleAction in actions.Value.EnumerateArray())
                {
                    // TODO: 2023-05-29 測試如何解析 addBannerToLiveChatCommand。
                    _logger.Debug("singleAction");
                    _logger.Debug(singleAction);

                    JsonElement? item = singleAction.Get("addChatItemAction")?.Get("item");

                    if (item.HasValue)
                    {
                        output.AddRange(GetRenderer(item.Value, isLarge));
                    }

                    // TODO: 2023-05-29 未測試，不確定是否有效。。
                    JsonElement? singleBannerRenderer = singleAction.Get("addBannerToLiveChatCommand")?.Get("bannerRenderer");

                    if (singleBannerRenderer.HasValue)
                    {
                        output.AddRange(GetRenderer(singleBannerRenderer.Value, isLarge));
                    }

                    JsonElement? videoOffsetTimeMsec = singleAction.Get("addChatItemAction")?.Get("videoOffsetTimeMsec");

                    string videoOffsetTimeText = GetVideoOffsetTimeMsec(videoOffsetTimeMsec);

                    JsonElement? replayActions = singleAction.Get("replayChatItemAction")?.Get("actions");

                    if (replayActions.HasValue &&
                        replayActions.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement replayAction in replayActions.Value.EnumerateArray())
                        {
                            JsonElement? replayItem = replayAction.Get("addChatItemAction")?.Get("item");

                            if (replayItem.HasValue)
                            {
                                List<RendererData> rendererDatas = GetRenderer(replayItem.Value, isLarge);

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

                            JsonElement? replayBannerRenderer = replayAction.Get("addBannerToLiveChatCommand")?.Get("bannerRenderer");

                            if (replayBannerRenderer.HasValue)
                            {
                                List<RendererData> rendererDatas = GetRenderer(replayBannerRenderer.Value, isLarge);

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
                    JsonElement? replaceAction = singleAction.Get("replaceChatItemAction");

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
    /// 取得 *Renderer 的內容
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>List&lt;RendererData&gt;</returns>
    public static List<RendererData> GetRenderer(JsonElement jsonElement, bool isLarge = true)
    {
        List<RendererData> output = new();

        // 參考：https://github.com/xenova/chat-downloader/blob/657e56eeec4ebe5af28de66b4d3653dbb796c8c1/chat_downloader/sites/youtube.py#L926
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
            if (Settings.Default.EnableDebug)
            {
                _logger.Debug("liveChatBannerRenderer");
                _logger.Debug(liveChatBannerRenderer);
            }

            // TODO: 2023-05-29 疑似有插入時間順序的問題。
            if (liveChatBannerRenderer.TryGetProperty(
                "header",
                out JsonElement header))
            {
                if (Settings.Default.EnableDebug)
                {
                    _logger.Debug("liveChatBannerRenderer -> header");
                    _logger.Debug(header);
                }

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
                if (Settings.Default.EnableDebug)
                {
                    _logger.Debug("liveChatBannerRenderer -> contents");
                    _logger.Debug(contents);
                }

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

            if (Settings.Default.EnableDebug)
            {
                _logger.Debug("方法：GetRenderer() -> 略過不處理的內容：{Message}",
                    $"{Environment.NewLine}{jsonElement.GetRawText()}{Environment.NewLine}");
            }
        }
        else
        {
            if (Settings.Default.EnableDebug)
            {
                _logger.Debug("方法：GetRenderer() -> 尚未支援的內容：{Message}",
                    $"{Environment.NewLine}{jsonElement.GetRawText()}{Environment.NewLine}");
            }
        }

        return output;
    }

    /// <summary>
    /// 取得 authorBadges
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>AuthorBadgesData</returns>
    public static AuthorBadgesData GetAuthorBadges(JsonElement jsonElement, bool isLarge = true)
    {
        AuthorBadgesData output = new();

        JsonElement? authorBadges = jsonElement.Get("authorBadges");

        if (authorBadges.HasValue &&
            authorBadges.Value.ValueKind == JsonValueKind.Array)
        {
            List<BadgeData> tempBadges = new();

            foreach (JsonElement singleAuthorBadge in authorBadges.Value.EnumerateArray())
            {
                BadgeData badgeData = new();

                // 自定義預覽圖。
                JsonElement? customThumbnail = singleAuthorBadge.Get("liveChatAuthorBadgeRenderer")
                    ?.Get("customThumbnail");

                if (customThumbnail.HasValue)
                {
                    JsonElement? thumbnails = customThumbnail.Value.Get("thumbnails");

                    if (thumbnails.HasValue &&
                        thumbnails.Value.ValueKind == JsonValueKind.Array)
                    {
                        if (thumbnails.Value.GetArrayLength() > 0)
                        {
                            int valIndex = isLarge ? 1 : 0;

                            if (thumbnails.Value.GetArrayLength() == 1)
                            {
                                valIndex = 0;
                            }

                            // 0：16x16、1：32x32
                            JsonElement? url = thumbnails.Value[valIndex].Get("url");

                            badgeData.Url = url.HasValue ? url.Value.GetString() : string.Empty;
                        }
                    }
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
    /// 取得 Message
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>MessageData</returns>
    public static MessageData GetMessage(JsonElement jsonElement, bool isLarge = true)
    {
        MessageData output = new();

        string tempText = string.Empty;
        string? tempTextColor = string.Empty,
            tempFontFace = string.Empty;

        bool isBold = false;

        List<EmojiData> tempEmojis = new();

        JsonElement? headerPrimaryText = jsonElement.Get("headerPrimaryText");

        if (headerPrimaryText.HasValue)
        {
            RunsData runsData = GetRuns(headerPrimaryText.Value, isLarge);

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

            RunsData runsData = GetRuns(headerSubtext.Value, isLarge);

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
            RunsData runsData = GetRuns(primaryText.Value, isLarge);

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
            RunsData runsData = GetRuns(text.Value, isLarge);

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
            RunsData runsData = GetRuns(subtext.Value, isLarge);

            tempText += runsData.Text;

            isBold = runsData.Bold ?? false;
            tempTextColor = runsData.TextColor;
            tempFontFace = runsData.FontFace;

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        // "sticker" 的 "label"。
        JsonElement? label = jsonElement.Get("sticker")
            ?.Get("accessibility")
            ?.Get("accessibilityData")
            ?.Get("label");

        if (label.HasValue)
        {
            tempText += label.Value.GetString();
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
            RunsData runsData = GetRuns(message.Value, isLarge);

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
            RunsData runsData = GetRuns(bannerMessage.Value, isLarge);

            tempText += runsData.Text;

            isBold = runsData.Bold ?? false;
            tempTextColor = runsData.TextColor;
            tempFontFace = runsData.FontFace;

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        if (Settings.Default.EnableDebug)
        {
            _logger.Debug($"方法：GetMessage() -> 除錯用的內容：" +
                $"{Environment.NewLine}{jsonElement.GetRawText()}{Environment.NewLine}");
        }

        if (string.IsNullOrEmpty(tempText))
        {
            tempText = StringSet.NoMessageContent;
        }

        output.Text = tempText;
        output.Bold = isBold;
        output.TextColor = tempTextColor;
        output.FontFace = tempFontFace;
        output.Emojis = tempEmojis;

        return output;
    }

    /// <summary>
    /// 取得 runs
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>RunsData</returns>
    public static RunsData GetRuns(JsonElement element, bool isLarge = true)
    {
        RunsData output = new();

        JsonElement? runs = element.Get("runs");

        if (runs.HasValue &&
            runs.Value.ValueKind == JsonValueKind.Array)
        {
            string tempText = string.Empty,
                tempTextColor = string.Empty,
                tempFontFace = string.Empty;

            bool isBold = false;

            List<EmojiData> tempEmojis = new();

            foreach (JsonElement singleRun in runs.Value.EnumerateArray())
            {
                JsonElement? text = singleRun.Get("text");

                if (text.HasValue)
                {
                    tempText += text.Value.GetString();
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
                    tempFontFace += fontFace.Value.GetString();
                }

                JsonElement? emoji = singleRun.Get("emoji");

                if (emoji.HasValue)
                {
                    if (!string.IsNullOrEmpty(emoji.Value.ToString()))
                    {
                        EmojiData emojiData = new();

                        JsonElement? emojiId = emoji.Value.Get("emojiId");

                        emojiData.ID = emojiId.HasValue ? emojiId.Value.GetString() : string.Empty;

                        // 2022-05-18 不再取 "shortcuts" 的第一個值。
                        /*
                        JsonElement? shortcuts = emoji.Value.Get("shortcuts");

                        if (shortcuts.HasValue &&
                            shortcuts.Value.ValueKind == JsonValueKind.Array)
                        {
                            if (shortcuts.Value.GetArrayLength() > 0)
                            {
                                // 只取第一個。
                                tempStr += $" {shortcuts.Value[0].GetString()} ";
                            }
                        }
                        */

                        // "image" 的 "thumbnails"。
                        JsonElement? thumbnails = emoji.Value.Get("image")
                            ?.Get("thumbnails");

                        if (thumbnails.HasValue &&
                            thumbnails.Value.ValueKind == JsonValueKind.Array)
                        {
                            if (thumbnails.Value.GetArrayLength() > 0)
                            {
                                int valIndex = isLarge ? 1 : 0;

                                // 非自定義表情符號的的索引值只有 1。
                                if (thumbnails.Value.GetArrayLength() == 1)
                                {
                                    valIndex = 0;
                                }

                                // 0：24x24、1：48x48
                                JsonElement? url = thumbnails.Value[valIndex].Get("url");

                                emojiData.Url = url.HasValue ? url.Value.GetString() : string.Empty;
                            }
                        }

                        // "image" 的 "label"。
                        JsonElement? label = emoji.Value.Get("image")
                            ?.Get("accessibility")
                            ?.Get("accessibilityData")
                            ?.Get("label");

                        if (label.HasValue)
                        {
                            // 仿 "shortcuts" 以利人工辨識。
                            tempText += $" :{label.Value.GetString()}: ";
                        }

                        emojiData.Text = label.HasValue ? $":{label.Value.GetString()}:" : string.Empty;
                        emojiData.Label = label.HasValue ? label.Value.GetString() : string.Empty;

                        JsonElement? isCustomEmoji = emoji.Value.Get("isCustomEmoji");

                        emojiData.IsCustomEmoji = isCustomEmoji.HasValue && isCustomEmoji.Value.GetBoolean();

                        if (Settings.Default.EnableDebug)
                        {
                            _logger.Debug($"方法：GetRuns() -> emoji -> 除錯用的內容：" +
                                $"{Environment.NewLine}{emoji.Value.GetRawText()}{Environment.NewLine}");
                        }

                        tempEmojis.Add(emojiData);
                    }
                }

                if (Settings.Default.EnableDebug)
                {
                    _logger.Debug($"方法：GetRuns() -> 除錯用的內容：" +
                        $"{Environment.NewLine}{singleRun.GetRawText()}{Environment.NewLine}");
                }
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
        if (Settings.Default.EnableDebug)
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
        }

        AuthorBadgesData authorBadgesData = GetAuthorBadges(jsonElement, isLarge);
        MessageData messageData = GetMessage(jsonElement, isLarge);

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

        // 處理特例。
        if (type == StringSet.YouTube)
        {
            authorName = $"[{type}]";
        }

        if (rendererName == "liveChatMembershipItemRenderer")
        {
            // 此處 message = headerSubtext.
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
                authorBadgesData = GetAuthorBadges(liveChatSponsorshipsHeaderRenderer.Value, isLarge);
                messageData = GetMessage(liveChatSponsorshipsHeaderRenderer.Value, isLarge);

                authorName = GetAuthorName(liveChatSponsorshipsHeaderRenderer.Value);
                authorPhoto = GetAuthorPhoto(liveChatSponsorshipsHeaderRenderer.Value, isLarge);
                authorBadges = authorBadgesData.Text ?? StringSet.NoAuthorBadges;
                // 此處 message = primaryText.
                message = messageData.Text ?? StringSet.NoMessageContent;
            }
        }
        else
        {
            // 不進行任何處理。
        }

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
            Emojis = messageData?.Emojis,
            Badges = authorBadgesData?.Badges
        });
    }
}