using NLog;
using System.Text.Json;
using YTLiveChatCatcher.Extensions;
using YTLiveChatCatcher.Models;

namespace YTLiveChatCatcher.Common.YTLiveChat;

/// <summary>
/// YouTube 聊天室 JSON 解析器
/// <para>參考 1：https://takikomiprogramming.hateblo.jp/entry/2020/07/21/114851</para>
/// <para>參考 2：https://yasulab-pg.com/%E3%80%90python%E3%80%91youtube-live%E3%81%AE%E3%82%A2%E3%83%BC%E3%82%AB%E3%82%A4%E3%83%96%E3%81%8B%E3%82%89%E3%83%81%E3%83%A3%E3%83%83%E3%83%88%E3%82%92%E5%8F%96%E5%BE%97%E3%81%99%E3%82%8B/</para>
/// </summary>
public class JsonParser
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 取得 Continuation
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <returns>字串陣列</returns>
    public static string[] GetContinuation(JsonElement? element)
    {
        string[] output = new string[2];

        if (element.HasValue)
        {
            JsonElement? continuations = element.Value.Get("continuationContents")
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

                        if (Properties.Settings.Default.EnableDebug)
                        {
                            _logger.Debug("方法：GetContinuation() -> playerSeekContinuationData -> 略過不處理的內容：{Message}",
                                $"{Environment.NewLine}{playerSeekContinuationData.Value.GetRawText()}{Environment.NewLine}");
                        }
                    }

                    #endregion

                    if (Properties.Settings.Default.EnableDebug)
                    {
                        // 尚未支援的內容。
                        _logger.Debug($"方法：GetContinuation() -> 尚未支援的內容：" +
                            $"{Environment.NewLine}{singleContinuation.GetRawText()}{Environment.NewLine}");
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// 取得重播的 Continuation
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <returns>字串</returns>
    public static string GetReplayContinuation(JsonElement element)
    {
        string output = string.Empty;

        JsonElement? liveChatRenderer = element.Get("contents")
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
    /// 取得 Action 的內容
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>List&lt;RendererData&gt;</returns>
    public static List<RendererData> GetActions(JsonElement? element, bool isLarge = true)
    {
        List<RendererData> output = new();

        if (element.HasValue)
        {
            JsonElement? actions = element.Value.Get("continuationContents")
                ?.Get("liveChatContinuation")
                ?.Get("actions");

            if (actions.HasValue &&
                actions.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement singleAction in actions.Value.EnumerateArray())
                {
                    JsonElement? item = singleAction.Get("addChatItemAction")?.Get("item");

                    if (item.HasValue)
                    {
                        output.AddRange(GetRenderer(item.Value, isLarge));
                    }

                    JsonElement? replayActions = singleAction.Get("replayChatItemAction")?.Get("actions");

                    if (replayActions.HasValue &&
                        replayActions.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement replayAction in replayActions.Value.EnumerateArray())
                        {
                            JsonElement? replayItem = replayAction.Get("addChatItemAction")?.Get("item");

                            if (replayItem.HasValue)
                            {
                                output.AddRange(GetRenderer(replayItem.Value, isLarge));
                            }
                        }
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// 取得 *Renderer 的內容
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>List&lt;RendererData&gt;</returns>
    private static List<RendererData> GetRenderer(JsonElement element, bool isLarge = true)
    {
        List<RendererData> output = new();

        // 參考：https://github.com/xenova/chat-downloader/blob/657e56eeec4ebe5af28de66b4d3653dbb796c8c1/chat_downloader/sites/youtube.py#L926
        if (element.TryGetProperty("liveChatTextMessageRenderer", out JsonElement liveChatTextMessageRenderer))
        {
            if (Properties.Settings.Default.EnableDebug)
            {
                _logger.Debug("liveChatTextMessageRenderer");
                _logger.Debug(liveChatTextMessageRenderer);
            }

            AuthorBadgesData authorBadgesData = GetAuthorBadges(liveChatTextMessageRenderer, isLarge);
            MessageData messageData = GetMessage(liveChatTextMessageRenderer, isLarge);

            string timestampUsec = GetTimestampUsec(liveChatTextMessageRenderer);
            string authorName = GetAuthorName(liveChatTextMessageRenderer);
            string authorPhoto = GetAuthorPhoto(liveChatTextMessageRenderer, isLarge);
            string authorBadges = authorBadgesData.Text ?? StringSet.NoAuthorBadges;
            string message = messageData.Text ?? StringSet.NoMessageContent;
            string purchaseAmountText = StringSet.NoPurchaseAmountText;
            string backgroundColor = StringSet.NoBackgroundColor;
            string timestampText = GetTimestampText(liveChatTextMessageRenderer);
            string authorExternalChannelID = GetAuthorExternalChannelId(liveChatTextMessageRenderer);

            output.Add(new RendererData()
            {
                Type = StringSet.ChatGeneral,
                TimestampUsec = timestampUsec,
                AuthorName = authorName,
                AuthorBadges = authorBadges,
                AuthorPhotoUrl = authorPhoto,
                MessageContent = message,
                PurchaseAmountText = purchaseAmountText,
                BackgroundColor = backgroundColor,
                TimestampText = timestampText,
                AuthorExternalChannelID = authorExternalChannelID,
                Emojis = messageData.Emojis ?? null,
                Badges = authorBadgesData.Badges ?? null
            });
        }
        else if (element.TryGetProperty("liveChatPaidMessageRenderer", out JsonElement liveChatPaidMessageRenderer))
        {
            if (Properties.Settings.Default.EnableDebug)
            {
                _logger.Debug("liveChatPaidMessageRenderer");
                _logger.Debug(liveChatPaidMessageRenderer);
            }

            AuthorBadgesData authorBadgesData = GetAuthorBadges(liveChatPaidMessageRenderer, isLarge);
            MessageData messageData = GetMessage(liveChatPaidMessageRenderer, isLarge);

            string timestampUsec = GetTimestampUsec(liveChatPaidMessageRenderer);
            string authorName = GetAuthorName(liveChatPaidMessageRenderer);
            string authorPhoto = GetAuthorPhoto(liveChatPaidMessageRenderer, isLarge);
            string authorBadges = authorBadgesData.Text ?? StringSet.NoAuthorBadges;
            string message = messageData.Text ?? StringSet.NoMessageContent;
            string purchaseAmountText = GetPurchaseAmountText(liveChatPaidMessageRenderer);
            string backgroundColor = GetBackgroundColor(liveChatPaidMessageRenderer);
            string timestampText = GetTimestampText(liveChatPaidMessageRenderer);
            string authorExternalChannelID = GetAuthorExternalChannelId(liveChatPaidMessageRenderer);

            output.Add(new RendererData()
            {
                Type = StringSet.ChatSuperChat,
                TimestampUsec = timestampUsec,
                AuthorName = authorName,
                AuthorBadges = authorBadges,
                AuthorPhotoUrl = authorPhoto,
                MessageContent = message,
                PurchaseAmountText = purchaseAmountText,
                BackgroundColor = backgroundColor,
                TimestampText = timestampText,
                AuthorExternalChannelID = authorExternalChannelID,
                Emojis = messageData.Emojis ?? null,
                Badges = authorBadgesData.Badges ?? null
            });
        }
        else if (element.TryGetProperty("liveChatPaidStickerRenderer", out JsonElement liveChatPaidStickerRenderer))
        {
            if (Properties.Settings.Default.EnableDebug)
            {
                _logger.Debug("liveChatPaidStickerRenderer");
                _logger.Debug(liveChatPaidStickerRenderer);
            }

            AuthorBadgesData authorBadgesData = GetAuthorBadges(liveChatPaidStickerRenderer, isLarge);
            MessageData messageData = GetMessage(liveChatPaidStickerRenderer, isLarge);

            string timestampUsec = GetTimestampUsec(liveChatPaidStickerRenderer);
            string authorName = GetAuthorName(liveChatPaidStickerRenderer);
            string authorPhoto = GetAuthorPhoto(liveChatPaidStickerRenderer, isLarge);
            string authorBadges = authorBadgesData.Text ?? StringSet.NoAuthorBadges;
            string message = messageData.Text ?? StringSet.NoMessageContent;
            string purchaseAmountText = GetPurchaseAmountText(liveChatPaidStickerRenderer);
            string backgroundColor = GetBackgroundColor(liveChatPaidStickerRenderer);
            string timestampText = GetTimestampText(liveChatPaidStickerRenderer);
            string authorExternalChannelID = GetAuthorExternalChannelId(liveChatPaidStickerRenderer);

            output.Add(new RendererData()
            {
                Type = StringSet.ChatSuperSticker,
                TimestampUsec = timestampUsec,
                AuthorName = authorName,
                AuthorBadges = authorBadges,
                AuthorPhotoUrl = authorPhoto,
                MessageContent = message,
                PurchaseAmountText = purchaseAmountText,
                BackgroundColor = backgroundColor,
                TimestampText = timestampText,
                AuthorExternalChannelID = authorExternalChannelID,
                Emojis = messageData.Emojis ?? null,
                Badges = authorBadgesData.Badges ?? null
            });
        }
        else if (element.TryGetProperty("liveChatMembershipItemRenderer", out JsonElement liveChatMembershipItemRenderer))
        {
            if (Properties.Settings.Default.EnableDebug)
            {
                _logger.Debug("liveChatMembershipItemRenderer");
                _logger.Debug(liveChatMembershipItemRenderer);
            }

            AuthorBadgesData authorBadgesData = GetAuthorBadges(liveChatMembershipItemRenderer, isLarge);
            MessageData messageData = GetMessage(liveChatMembershipItemRenderer, isLarge);

            string timestampUsec = GetTimestampUsec(liveChatMembershipItemRenderer);
            string authorName = GetAuthorName(liveChatMembershipItemRenderer);
            string authorPhoto = GetAuthorPhoto(liveChatMembershipItemRenderer, isLarge);
            string authorBadges = authorBadgesData.Text ?? StringSet.NoAuthorBadges;
            string headerSubtext = messageData.Text ?? StringSet.NoMessageContent;
            string purchaseAmountText = StringSet.NoPurchaseAmountText;
            string backgroundColor = StringSet.NoBackgroundColor;
            string timestampText = GetTimestampText(liveChatMembershipItemRenderer);
            string authorExternalChannelID = GetAuthorExternalChannelId(liveChatMembershipItemRenderer);

            string type;

            if (headerSubtext.Contains(StringSet.MemberUpgrade))
            {
                type = StringSet.ChatMemberUpgrade;
            }
            else if (headerSubtext.Contains(StringSet.MemberMilestone))
            {
                type = StringSet.ChatMemberMilestone;
            }
            else
            {
                type = StringSet.ChatJoinMember;
            }

            output.Add(new RendererData()
            {
                Type = type,
                TimestampUsec = timestampUsec,
                AuthorName = authorName,
                AuthorBadges = authorBadges,
                AuthorPhotoUrl = authorPhoto,
                MessageContent = headerSubtext,
                PurchaseAmountText = purchaseAmountText,
                BackgroundColor = backgroundColor,
                TimestampText = timestampText,
                AuthorExternalChannelID = authorExternalChannelID,
                Emojis = messageData.Emojis ?? null,
                Badges = authorBadgesData.Badges ?? null
            });
        }
        else if (element.TryGetProperty("liveChatViewerEngagementMessageRenderer", out JsonElement liveChatViewerEngagementMessageRenderer))
        {
            if (Properties.Settings.Default.EnableDebug)
            {
                _logger.Debug("liveChatViewerEngagementMessageRenderer");
                _logger.Debug(liveChatViewerEngagementMessageRenderer);
            }

            AuthorBadgesData authorBadgesData = GetAuthorBadges(liveChatViewerEngagementMessageRenderer, isLarge);
            MessageData messageData = GetMessage(liveChatViewerEngagementMessageRenderer, isLarge);

            string timestampUsec = GetTimestampUsec(liveChatViewerEngagementMessageRenderer);
            string authorName = $"[{StringSet.YouTube}]";
            string authorPhoto = StringSet.NoAuthorPhotoUrl;
            string authorBadges = authorBadgesData.Text ?? StringSet.NoAuthorBadges;
            string message = messageData.Text ?? StringSet.NoMessageContent;
            string purchaseAmountText = StringSet.NoPurchaseAmountText;
            string backgroundColor = StringSet.NoBackgroundColor;
            string timestampText = GetTimestampText(liveChatViewerEngagementMessageRenderer);
            string authorExternalChannelID = StringSet.NoAuthorExternalChannelID;

            output.Add(new RendererData()
            {
                Type = StringSet.YouTube,
                TimestampUsec = timestampUsec,
                AuthorName = authorName,
                AuthorBadges = authorBadges,
                AuthorPhotoUrl = authorPhoto,
                MessageContent = message,
                PurchaseAmountText = purchaseAmountText,
                BackgroundColor = backgroundColor,
                TimestampText = timestampText,
                AuthorExternalChannelID = authorExternalChannelID,
                Emojis = messageData.Emojis ?? null,
                Badges = authorBadgesData.Badges ?? null
            });
        }
        else if (element.TryGetProperty("liveChatModeChangeMessageRenderer", out JsonElement liveChatModeChangeMessageRenderer))
        {
            if (Properties.Settings.Default.EnableDebug)
            {
                _logger.Debug("liveChatModeChangeMessageRenderer");
                _logger.Debug(liveChatModeChangeMessageRenderer);
            }

            AuthorBadgesData authorBadgesData = GetAuthorBadges(liveChatModeChangeMessageRenderer, isLarge);
            MessageData messageData = GetMessage(liveChatModeChangeMessageRenderer, isLarge);

            string timestampUsec = GetTimestampUsec(liveChatModeChangeMessageRenderer);
            string authorName = $"[{StringSet.YouTube}]";
            string authorPhoto = StringSet.NoAuthorPhotoUrl;
            string authorBadges = authorBadgesData.Text ?? StringSet.NoAuthorBadges;
            string message = messageData.Text ?? StringSet.NoMessageContent;
            string purchaseAmountText = StringSet.NoPurchaseAmountText;
            string backgroundColor = StringSet.NoBackgroundColor;
            string timestampText = GetTimestampText(liveChatModeChangeMessageRenderer);
            string authorExternalChannelID = StringSet.NoAuthorExternalChannelID;

            output.Add(new RendererData()
            {
                Type = StringSet.YouTube,
                TimestampUsec = timestampUsec,
                AuthorName = authorName,
                AuthorBadges = authorBadges,
                AuthorPhotoUrl = authorPhoto,
                MessageContent = message,
                PurchaseAmountText = purchaseAmountText,
                BackgroundColor = backgroundColor,
                TimestampText = timestampText,
                AuthorExternalChannelID = authorExternalChannelID,
                Emojis = messageData.Emojis ?? null,
                Badges = authorBadgesData.Badges ?? null
            });
        }
        else if (element.TryGetProperty("liveChatSponsorshipsGiftPurchaseAnnouncementRenderer", out JsonElement liveChatSponsorshipsGiftPurchaseAnnouncementRenderer))
        {
            if (Properties.Settings.Default.EnableDebug)
            {
                _logger.Debug("liveChatSponsorshipsGiftPurchaseAnnouncementRenderer");
                _logger.Debug(liveChatSponsorshipsGiftPurchaseAnnouncementRenderer);
            }

            AuthorBadgesData? authorBadgesData = null;
            MessageData? messageData = null;

            string authorName = StringSet.NoAuthorName;
            string authorPhoto = StringSet.NoAuthorPhotoUrl;
            string authorBadges = StringSet.NoAuthorBadges;
            string primaryText = string.Empty;

            JsonElement? liveChatSponsorshipsHeaderRenderer = liveChatSponsorshipsGiftPurchaseAnnouncementRenderer
                .Get("header")
                ?.Get("liveChatSponsorshipsHeaderRenderer");

            if (liveChatSponsorshipsHeaderRenderer.HasValue)
            {
                authorBadgesData = GetAuthorBadges(liveChatSponsorshipsHeaderRenderer.Value, isLarge);
                messageData = GetMessage(liveChatSponsorshipsHeaderRenderer.Value, isLarge);

                authorName = GetAuthorName(liveChatSponsorshipsHeaderRenderer.Value);
                authorPhoto = GetAuthorPhoto(liveChatSponsorshipsHeaderRenderer.Value, isLarge);
                authorBadges = authorBadgesData.Text ?? StringSet.NoAuthorBadges;
                primaryText = messageData.Text ?? StringSet.NoMessageContent;
            }

            string timestampUsec = GetTimestampUsec(liveChatSponsorshipsGiftPurchaseAnnouncementRenderer);
            string purchaseAmountText = StringSet.NoPurchaseAmountText;
            string backgroundColor = StringSet.NoBackgroundColor;
            string timestampText = StringSet.NoTimestampText;
            string authorExternalChannelID = GetAuthorExternalChannelId(liveChatSponsorshipsGiftPurchaseAnnouncementRenderer);

            output.Add(new RendererData()
            {
                Type = StringSet.ChatMemberGift,
                TimestampUsec = timestampUsec,
                AuthorName = authorName,
                AuthorBadges = authorBadges,
                AuthorPhotoUrl = authorPhoto,
                MessageContent = primaryText,
                PurchaseAmountText = purchaseAmountText,
                BackgroundColor = backgroundColor,
                TimestampText = timestampText,
                AuthorExternalChannelID = authorExternalChannelID,
                Emojis = messageData?.Emojis ?? null,
                Badges = authorBadgesData?.Badges ?? null
            });
        }
        else if (element.TryGetProperty("liveChatSponsorshipsGiftRedemptionAnnouncementRenderer", out JsonElement liveChatSponsorshipsGiftRedemptionAnnouncementRenderer))
        {
            if (Properties.Settings.Default.EnableDebug)
            {
                _logger.Debug("liveChatSponsorshipsGiftRedemptionAnnouncementRenderer");
                _logger.Debug(liveChatSponsorshipsGiftRedemptionAnnouncementRenderer);
            }

            AuthorBadgesData authorBadgesData = GetAuthorBadges(liveChatSponsorshipsGiftRedemptionAnnouncementRenderer, isLarge);
            MessageData messageData = GetMessage(liveChatSponsorshipsGiftRedemptionAnnouncementRenderer, isLarge);

            string timestampUsec = GetTimestampUsec(liveChatSponsorshipsGiftRedemptionAnnouncementRenderer);
            string authorName = GetAuthorName(liveChatSponsorshipsGiftRedemptionAnnouncementRenderer);
            string authorPhoto = GetAuthorPhoto(liveChatSponsorshipsGiftRedemptionAnnouncementRenderer, isLarge);
            string authorBadges = authorBadgesData.Text ?? StringSet.NoAuthorBadges;
            string message = messageData.Text ?? StringSet.NoMessageContent;
            string purchaseAmountText = StringSet.NoPurchaseAmountText;
            string backgroundColor = StringSet.NoBackgroundColor;
            string timestampText = StringSet.NoTimestampText;
            string authorExternalChannelID = GetAuthorExternalChannelId(liveChatSponsorshipsGiftRedemptionAnnouncementRenderer);

            output.Add(new RendererData()
            {
                Type = StringSet.ChatReceivedMemberGift,
                TimestampUsec = timestampUsec,
                AuthorName = authorName,
                AuthorBadges = authorBadges,
                AuthorPhotoUrl = authorPhoto,
                MessageContent = message,
                PurchaseAmountText = purchaseAmountText,
                BackgroundColor = backgroundColor,
                TimestampText = timestampText,
                AuthorExternalChannelID = authorExternalChannelID,
                Emojis = messageData.Emojis ?? null,
                Badges = authorBadgesData.Badges ?? null
            });
        }
        else if (element.TryGetProperty("liveChatTickerPaidMessageItemRenderer", out _) ||
            element.TryGetProperty("liveChatTickerPaidStickerItemRenderer", out _) ||
            element.TryGetProperty("liveChatTickerSponsorItemRenderer", out _) ||
            element.TryGetProperty("liveChatPlaceholderItemRenderer", out _) ||
            element.TryGetProperty("liveChatDonationAnnouncementRenderer", out _) ||
            element.TryGetProperty("liveChatPurchasedProductMessageRenderer", out _) ||
            element.TryGetProperty("liveChatLegacyPaidMessageRenderer", out _) ||
            element.TryGetProperty("liveChatModerationMessageRenderer", out _) ||
            element.TryGetProperty("liveChatAutoModMessageRenderer", out _))
        {
            // 參考：https://taiyakisun.hatenablog.com/entry/2020/10/13/223443
            // 略過進不行任何處理。

            if (Properties.Settings.Default.EnableDebug)
            {
                _logger.Debug("方法：GetRenderer() -> 略過不處理的內容：{Message}",
                    $"{Environment.NewLine}{element.GetRawText()}{Environment.NewLine}");
            }
        }
        else
        {
            if (Properties.Settings.Default.EnableDebug)
            {
                _logger.Debug("方法：GetRenderer() -> 尚未支援的內容：{Message}",
                    $"{Environment.NewLine}{element.GetRawText()}{Environment.NewLine}");
            }
        }

        return output;
    }

    /// <summary>
    /// 取得 authorName
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetAuthorName(JsonElement element)
    {
        string output = string.Empty;

        JsonElement? simpleText = element.Get("authorName")?.Get("simpleText");

        if (simpleText.HasValue)
        {
            output = simpleText.Value.GetString() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(output))
        {
            output = StringSet.NoAuthorName;
        }

        return output;
    }

    /// <summary>
    /// 取得 authorPhoto
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>字串</returns>
    private static string GetAuthorPhoto(JsonElement element, bool isLarge = true)
    {
        string output = string.Empty;

        JsonElement? thumbnails = element.Get("authorPhoto")?.Get("thumbnails");

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

                // 0：32x32、1：64x64
                JsonElement? url = thumbnails.Value[valIndex].Get("url");

                if (url.HasValue)
                {
                    output = url.Value.GetString() ?? string.Empty;
                }
            }
        }

        return output;
    }

    /// <summary>
    /// 取得 authorExternalChannelId
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetAuthorExternalChannelId(JsonElement element)
    {
        string output = string.Empty;

        JsonElement? authorExternalChannelId = element.Get("authorExternalChannelId");

        if (authorExternalChannelId.HasValue)
        {
            output = authorExternalChannelId.Value.GetString() ?? string.Empty;
        }

        return output;
    }

    /// <summary>
    /// 取得 authorBadges
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>AuthorBadgesData</returns>
    private static AuthorBadgesData GetAuthorBadges(JsonElement element, bool isLarge = true)
    {
        AuthorBadgesData output = new();

        JsonElement? authorBadges = element.Get("authorBadges");

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
    /// 取得 timestampUsec
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetTimestampUsec(JsonElement element)
    {
        string output = string.Empty;

        JsonElement? timestampUsec = element.Get("timestampUsec");

        if (timestampUsec.HasValue)
        {
            // 將 Microseconds 轉換成 Miliseconds。
            long timestamp = Convert.ToInt64(timestampUsec.Value.GetString()) / 1000L;

            output = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime.ToString();
        }

        return output;
    }

    /// <summary>
    /// 取得 timestampText
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetTimestampText(JsonElement element)
    {
        string output = string.Empty;

        JsonElement? simpleText = element.Get("timestampText")?.Get("simpleText");

        if (simpleText.HasValue)
        {
            output = simpleText.Value.GetString() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(output))
        {
            output = StringSet.NoTimestampText;
        }

        return output;
    }

    /// <summary>
    /// 取得 purchaseAmountText
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetPurchaseAmountText(JsonElement element)
    {
        string output = string.Empty;

        JsonElement? simpleText = element.Get("purchaseAmountText")
            ?.Get("simpleText");

        if (simpleText.HasValue)
        {
            output = simpleText.Value.GetString() ?? string.Empty;
        }

        return output;
    }

    /// <summary>
    /// 取得背景顏色
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetBackgroundColor(JsonElement element)
    {
        string output = string.Empty;

        JsonElement? backgroundColor = element.Get("backgroundColor");

        if (backgroundColor.HasValue)
        {
            output = GetColorHexCode(backgroundColor.Value.GetInt64());
        }

        JsonElement? bodyBackgroundColor = element.Get("bodyBackgroundColor");

        if (bodyBackgroundColor.HasValue)
        {
            output = GetColorHexCode(bodyBackgroundColor.Value.GetInt64());
        }

        if (string.IsNullOrEmpty(output))
        {
            output = StringSet.NoBackgroundColor;
        }

        return output;
    }

    /// <summary>
    /// 取得 message
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>MessageData</returns>
    private static MessageData GetMessage(JsonElement element, bool isLarge = true)
    {
        MessageData output = new();

        string tempText = string.Empty;

        List<EmojiData> tempEmojis = new();

        JsonElement? headerPrimaryText = element.Get("headerPrimaryText");

        if (headerPrimaryText.HasValue)
        {
            RunsData runsData = GetRuns(headerPrimaryText.Value, isLarge);

            tempText += $" [{runsData.Text}] ";

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        // "headerSubtext" 的 "simpleText"。
        JsonElement? headerSubtext = element.Get("headerSubtext");

        if (headerSubtext.HasValue)
        {
            // "headerSubtext" 的 "simpleText"。
            JsonElement? simpleText = element.Get("headerSubtext")
                ?.Get("simpleText");

            if (simpleText.HasValue)
            {
                // 手動在前後補一個空白跟 []。
                tempText += $" [{simpleText.Value}] ";
            }

            RunsData runsData = GetRuns(headerSubtext.Value, isLarge);

            tempText += $" {runsData.Text} ";

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        JsonElement? primaryText = element.Get("primaryText");

        if (primaryText.HasValue)
        {
            RunsData runsData = GetRuns(primaryText.Value, isLarge);

            tempText += runsData.Text;

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        JsonElement? text = element.Get("text");

        if (text.HasValue)
        {
            RunsData runsData = GetRuns(text.Value, isLarge);

            tempText += runsData.Text;

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        JsonElement? subtext = element.Get("subtext");

        if (subtext.HasValue)
        {
            RunsData runsData = GetRuns(subtext.Value, isLarge);

            tempText += runsData.Text;

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        // "sticker" 的 "label"。
        JsonElement? label = element.Get("sticker")
            ?.Get("accessibility")
            ?.Get("accessibilityData")
            ?.Get("label");

        if (label.HasValue)
        {
            tempText += label.Value.GetString();
        }

        // "purchaseAmountText" 的 "simpleText"。
        JsonElement? purchaseAmountText = element.Get("purchaseAmountText")
            ?.Get("simpleText");

        if (purchaseAmountText.HasValue)
        {
            // 手動在前後補一個空白跟 []。
            tempText += $" [{purchaseAmountText.Value}] ";
        }

        JsonElement? message = element.Get("message");

        if (message.HasValue)
        {
            RunsData runsData = GetRuns(message.Value, isLarge);

            tempText += runsData.Text;

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        if (Properties.Settings.Default.EnableDebug)
        {
            _logger.Debug($"方法：GetMessage() -> 除錯用的內容：" +
                $"{Environment.NewLine}{element.GetRawText()}{Environment.NewLine}");
        }

        if (string.IsNullOrEmpty(tempText))
        {
            tempText = StringSet.NoMessageContent;
        }

        output.Text = tempText;
        output.Emojis = tempEmojis;

        return output;
    }

    /// <summary>
    /// 取得 runs
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>RunsData</returns>
    private static RunsData GetRuns(JsonElement element, bool isLarge = true)
    {
        RunsData output = new();

        JsonElement? runs = element.Get("runs");

        if (runs.HasValue &&
            runs.Value.ValueKind == JsonValueKind.Array)
        {
            string tempText = string.Empty;

            List<EmojiData> tempEmojis = new();

            foreach (JsonElement singleRun in runs.Value.EnumerateArray())
            {
                JsonElement? text = singleRun.Get("text");

                if (text.HasValue)
                {
                    tempText += text.Value.GetString();
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

                        if (Properties.Settings.Default.EnableDebug)
                        {
                            _logger.Debug($"方法：GetRuns() -> emoji -> 除錯用的內容：" +
                                $"{Environment.NewLine}{emoji.Value.GetRawText()}{Environment.NewLine}");
                        }

                        tempEmojis.Add(emojiData);
                    }
                }

                if (Properties.Settings.Default.EnableDebug)
                {
                    _logger.Debug($"方法：GetRuns() -> 除錯用的內容：" +
                        $"{Environment.NewLine}{singleRun.GetRawText()}{Environment.NewLine}");
                }
            }

            output.Text = tempText;
            output.Emojis = tempEmojis;
        }

        return output;
    }

    /// <summary>
    /// 取得 Hex 色碼
    /// </summary>
    /// <param name="value">Int64</param>
    /// <returns>字串</returns>
    private static string GetColorHexCode(long value)
    {
        string hex = string.Format("{0:X}", value);

        int integer = Convert.ToInt32(hex, 16);

        return ColorTranslator.ToHtml(Color.FromArgb(integer));
    }
}