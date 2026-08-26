using System.Text.Json;
using Microsoft.Extensions.Logging;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Models.LiveChat;
using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// YTJsonParser 的解析即時聊天 JSON 資料的方法
/// <para>參考 1：https://takikomiprogramming.hateblo.jp/entry/2020/07/21/114851</para>
/// <para>參考 2：https://yasulab-pg.com/%E3%80%90python%E3%80%91youtube-live%E3%81%AE%E3%82%A2%E3%83%BC%E3%82%AB%E3%82%A4%E3%83%96%E3%81%8B%E3%82%89%E3%83%81%E3%83%A3%E3%83%83%E3%83%88%E3%82%92%E5%8F%96%E5%BE%97%E3%81%99%E3%82%8B/</para>
/// </summary>
public partial class YTJsonParser
{
    /// <summary>
    /// 解析 ytcfg 
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>YTConfigData</returns>
    private static YTConfigData ParseYtCfg(JsonElement? jsonElement)
    {
        YTConfigData ytConfigData = new();

        YtCfgDto? ytCfgDto = jsonElement?.Deserialize<YtCfgDto>();

        if (ytCfgDto == null)
        {
            return ytConfigData;
        }

        YtCfgClientDto? client = ytCfgDto.InnertubeContext?.Client;

        ytConfigData.APIKey = ytCfgDto.InnertubeApiKey;
        ytConfigData.IDToken = ytCfgDto.IdToken;
        ytConfigData.SessionIndex = ytCfgDto.SessionIndex;
        ytConfigData.InnertubeContextClientName = ytCfgDto.InnertubeContextClientName;
        ytConfigData.InnertubeContextClientVersion = ytCfgDto.InnertubeContextClientVersion;
        ytConfigData.InnertubeClientVersion = ytCfgDto.InnertubeClientVersion;
        ytConfigData.DataSyncID = ytCfgDto.DataSyncId;
        ytConfigData.DelegatedSessionID = ytCfgDto.DelegatedSessionId;
        ytConfigData.BrowserName = client?.BrowserName;
        ytConfigData.BrowserVersion = client?.BrowserVersion;
        ytConfigData.ClientFormFactor = client?.ClientFormFactor;
        ytConfigData.ClientName = client?.ClientName;
        ytConfigData.ClientVersion = client?.ClientVersion;
        ytConfigData.DeviceMake = client?.DeviceMake;
        ytConfigData.DeviceModel = client?.DeviceModel;
        ytConfigData.Gl = client?.Gl;
        ytConfigData.Hl = client?.Hl;
        ytConfigData.OriginalUrl = client?.OriginalUrl;
        ytConfigData.OsName = client?.OsName;
        ytConfigData.OsVersion = client?.OsVersion;
        ytConfigData.Platform = client?.Platform;
        ytConfigData.RemoteHost = client?.RemoteHost;
        ytConfigData.UserAgent = client?.UserAgent;
        ytConfigData.VisitorData = client?.VisitorData;

        // 參考：https://github.com/xenova/chat-downloader/blob/master/chat_downloader/sites/youtube.py#L1629
        string[]? arrayDataSyncID = ytConfigData.DataSyncID
            ?.Split("||".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        if (arrayDataSyncID?.Length >= 2 && !string.IsNullOrEmpty(arrayDataSyncID[1]))
        {
            ytConfigData.DataSyncID = arrayDataSyncID[0];
        }
        else
        {
            ytConfigData.DataSyncID = ytConfigData.DelegatedSessionID;
        }

        return ytConfigData;
    }

    /// <summary>
    /// 解析直播時的 continuation
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="options">LiveChatStreamOptions</param>
    /// <returns>string[]</returns>
    private string[] ParseStreamingContinuation(JsonElement? jsonElement, LiveChatStreamOptions options)
    {
        string[] output = new string[2];

        if (jsonElement.HasValue)
        {
            JsonElement? liveChatRenderer = jsonElement
                ?.Get("contents")
                ?.Get("liveChatRenderer");

            if (liveChatRenderer.HasValue)
            {
                output[0] = ParseSubMenuItemsContinuation(liveChatRenderer, options);

                // Fallback 機制。
                JsonElement.ArrayEnumerator? continuations = liveChatRenderer
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
                            if (string.IsNullOrEmpty(output[0]))
                            {
                                JsonElement? continuation = invalidationContinuationData.Value.Get("continuation");

                                if (continuation.HasValue)
                                {
                                    output[0] = continuation.Value.ToString();
                                }
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
                            if (string.IsNullOrEmpty(output[0]))
                            {
                                JsonElement? continuation = timedContinuationData.Value.Get("continuation");

                                if (continuation.HasValue)
                                {
                                    output[0] = continuation.Value.ToString();
                                }
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
                            if (string.IsNullOrEmpty(output[0]))
                            {
                                JsonElement? continuation = liveChatReplayContinuationData.Value.Get("continuation");

                                if (continuation.HasValue)
                                {
                                    output[0] = continuation.Value.ToString();
                                }
                            }

                            // 沒有 "timeoutMs"。
                            output[1] = string.Empty;

                            JsonElement? _ = liveChatReplayContinuationData.Value.Get("timeUntilLastMessageMsec");

                            break;
                        }

                        #endregion

                        #region playerSeekContinuationData

                        JsonElement? playerSeekContinuationData = singleContinuation.Get("playerSeekContinuationData");

                        if (playerSeekContinuationData.HasValue)
                        {
                            // 略過不進行任何的處理。
                            LogMessages.Trace(_logger, "ParseStreamingContinuation -> playerSeekContinuationData", playerSeekContinuationData.Value.GetRawText());
                        }

                        #endregion

                        LogMessages.UnsupportedContentEncountered(_logger, "ParseStreamingContinuation -> 尚未支援的內容", singleContinuation.GetRawText());
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// 解析 subMenuItems 下的 continuation
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="options">LiveChatStreamOptions</param>
    /// <returns>字串</returns>
    private string ParseSubMenuItemsContinuation(JsonElement? jsonElement, LiveChatStreamOptions options)
    {
        string output = string.Empty;

        if (jsonElement.HasValue)
        {
            JsonElement.ArrayEnumerator? subMenuItems = jsonElement
                ?.Get("header")
                ?.Get("liveChatHeaderRenderer")
                ?.Get("viewSelector")
                ?.Get("sortFilterSubMenuRenderer")
                ?.Get("subMenuItems")
                ?.ToArrayEnumerator();

            if (subMenuItems.HasValue)
            {
                // 通常都是預設已選擇熱門的即時聊天。
                // 照索引順序的話：
                // 0：熱門
                // 1：全部

                // 當 CustomLiveChatType 不為 null 或空白時，則使用使用者自行帶入的值。
                if (!string.IsNullOrEmpty(options.CustomLiveChatType))
                {
                    LogMessages.SubMenuCustomTitle(_logger, options.CustomLiveChatType);

                    foreach (JsonElement subMenuItem in subMenuItems)
                    {
                        JsonElement? title = subMenuItem.Get("title");

                        LogMessages.SubMenuTitle(_logger, title?.GetString());

                        if (title.HasValue && title?.GetString() == options.CustomLiveChatType)
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
                else
                {
                    JsonElement subMenuItem = subMenuItems
                        .Value
                        .ElementAtOrDefault(options.LiveChatType.ToInt32());

                    JsonElement? title = subMenuItem.Get("title");

                    if (title.HasValue)
                    {
                        JsonElement? continuation = subMenuItem.Get("continuation")
                            ?.Get("reloadContinuationData")
                            ?.Get("continuation");

                        if (continuation.HasValue)
                        {
                            output = continuation.Value.GetString() ?? string.Empty;
                        }
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// 解析 continuation
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>string[]</returns>
    private string[] ParseContinuation(JsonElement? jsonElement)
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
                        }

                        JsonElement? timeUntilLastMessageMsec = liveChatReplayContinuationData.Value.Get("timeUntilLastMessageMsec");

                        if (timeUntilLastMessageMsec.HasValue)
                        {
                            output[1] = timeUntilLastMessageMsec.Value.ToString();
                        }

                        break;
                    }

                    #endregion

                    #region playerSeekContinuationData

                    JsonElement? playerSeekContinuationData = singleContinuation.Get("playerSeekContinuationData");

                    if (playerSeekContinuationData.HasValue)
                    {
                        // 略過不進行任何的處理。
                        LogMessages.Trace(_logger, "ParseContinuation -> playerSeekContinuationData", playerSeekContinuationData.Value.GetRawText());
                    }

                    #endregion

                    // 尚未支援的內容。
                    LogMessages.UnsupportedContentEncountered(_logger, "ParseContinuation -> 尚未支援的內容", singleContinuation.GetRawText());
                }
            }
        }

        return output;
    }

    /// <summary>
    /// 解析 action 的內容
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>List&lt;RendererData&gt;</returns>
    private List<RendererData> ParseActions(JsonElement? jsonElement)
    {
        List<RendererData> output = [];

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
                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        LogMessages.Trace(_logger, "ParseActions -> singleAction", singleAction.GetRawText());
                    }

                    JsonElement? item = singleAction.Get("addChatItemAction")?.Get("item");

                    if (item.HasValue)
                    {
                        output.AddRange(ParseRenderer(item.Value));
                    }

                    // 2026/8 已對真實直播驗證過，詳見 ParseRenderer 內 liveChatBannerRenderer 分支的說明。
                    JsonElement? singleBannerRenderer = singleAction
                        .Get("addBannerToLiveChatCommand")
                        ?.Get("bannerRenderer");

                    if (singleBannerRenderer.HasValue)
                    {
                        output.AddRange(ParseRenderer(singleBannerRenderer.Value));
                    }

                    ParseNonMessageAction(output, singleAction);

                    JsonElement? videoOffsetTimeMsec = singleAction
                        .Get("addChatItemAction")
                        ?.Get("videoOffsetTimeMsec");

                    string videoOffsetTimeText = GetVideoOffsetTimeMsec(videoOffsetTimeMsec) ?? string.Empty;

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
                                List<RendererData> rendererDatas = ParseRenderer(replayItem.Value);

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
                                List<RendererData> rendererDatas = ParseRenderer(replayBannerRenderer.Value);

                                foreach (RendererData rendererData in rendererDatas)
                                {
                                    if ((string.IsNullOrEmpty(rendererData.TimestampText) &&
                                        string.IsNullOrEmpty(rendererData.TimestampUsec)) ||
                                        rendererData.TimestampText == KeySet.NoTimestampText)
                                    {
                                        rendererData.TimestampText = videoOffsetTimeText;
                                    }
                                }

                                output.AddRange(rendererDatas);
                            }

                            ParseNonMessageAction(output, replayAction);
                        }
                    }

                    // 訊息內容被取代／修改（例如超級留言／超級貼圖淡出時，會被替換成較小的顯示樣式）。
                    JsonElement? replaceAction = singleAction
                        .Get("replaceChatItemAction");

                    if (replaceAction.HasValue)
                    {
                        output.AddRange(ParseReplaceChatItemAction(replaceAction.Value));
                    }
                }
            }

            output.AddRange(ParseFrameworkUpdates(jsonElement.Value));
        }

        return output;
    }

    /// <summary>
    /// 解析 <c>frameworkUpdates.entityBatchUpdate.mutations</c>（YouTube 的通用「實體」即時更新機制）
    /// <para>目前只處理 <c>replyCountEntity</c>（付費類訊息的回覆討論串人數，見 <see cref="GetReplyCountEntityKey"/>）
    /// ——這是目前唯一觀察到、承載了實際可用資料的實體種類。同一個機制底下還有 <c>engagementToolbarStateEntityPayload</c>
    /// （超級留言的愛心按鈕狀態）與 <c>emojiFountainDataEntity</c>（直播間的表情雨特效），
    /// 前者實測過真實資料後只有一個不透明的 key、沒有任何可判讀的狀態欄位，後者是整場直播共用、
    /// 不屬於任何一則訊息的環境特效資料，兩者皆刻意不處理。</para>
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>List&lt;RendererData&gt;</returns>
    private List<RendererData> ParseFrameworkUpdates(JsonElement jsonElement)
    {
        List<RendererData> output = [];

        JsonElement.ArrayEnumerator? mutations = jsonElement.Get("frameworkUpdates")
            ?.Get("entityBatchUpdate")
            ?.Get("mutations")
            ?.ToArrayEnumerator();

        if (!mutations.HasValue)
        {
            return output;
        }

        foreach (JsonElement mutation in mutations)
        {
            JsonElement? replyCountEntity = mutation.Get("payload")?.Get("replyCountEntity");

            if (!replyCountEntity.HasValue)
            {
                continue;
            }

            string entityKey = mutation.Get("entityKey")?.GetString() ?? string.Empty;
            string? replyCountNumber = replyCountEntity.Value.Get("replyCountNumber")?.GetString();

            output.Add(new RendererData()
            {
                ID = entityKey,
                Type = GetLocalizeString(KeySet.ChatReplyCountUpdate),
                ReplyCount = replyCountNumber
            });
        }

        return output;
    }

    /// <summary>
    /// 目前所有已知的頂層 action 鍵值名稱（部分在 <see cref="ParseActions"/> 呼叫端已檢查，
    /// 部分在這裡檢查），用於 <see cref="ParseNonMessageAction"/> 判斷是否為完全陌生的 action 類型。
    /// </summary>
    private static readonly string[] KnownActionKeys =
    [
        "addChatItemAction",
        "addBannerToLiveChatCommand",
        "removeChatItemAction",
        "removeChatItemByAuthorAction",
        "showLiveChatActionPanelAction",
        "replayChatItemAction",
        "replaceChatItemAction",
        "updateLiveChatPollAction"
    ];

    /// <summary>
    /// 解析非訊息類型的 action（留言刪除、使用者封鎖、投票、投票結果更新）
    /// </summary>
    /// <param name="output">List&lt;RendererData&gt;</param>
    /// <param name="singleAction">JsonElement</param>
    private void ParseNonMessageAction(List<RendererData> output, JsonElement singleAction)
    {
        JsonElement? removeChatItemAction = singleAction.Get("removeChatItemAction");

        if (removeChatItemAction.HasValue)
        {
            output.Add(ParseRemoveChatItemAction(removeChatItemAction.Value));
        }

        JsonElement? removeChatItemByAuthorAction = singleAction.Get("removeChatItemByAuthorAction");

        if (removeChatItemByAuthorAction.HasValue)
        {
            output.Add(ParseRemoveChatItemByAuthorAction(removeChatItemByAuthorAction.Value));
        }

        JsonElement? pollRenderer = singleAction
            .Get("showLiveChatActionPanelAction")
            ?.Get("panelToShow")
            ?.Get("liveChatActionPanelRenderer")
            ?.Get("contents")
            ?.Get("pollRenderer");

        if (pollRenderer.HasValue)
        {
            output.Add(ParsePollRenderer(pollRenderer.Value));
        }

        JsonElement? pollToUpdate = singleAction
            .Get("updateLiveChatPollAction")
            ?.Get("pollToUpdate")
            ?.Get("pollRenderer");

        if (pollToUpdate.HasValue)
        {
            output.Add(ParseUpdateLiveChatPollAction(pollToUpdate.Value));
        }

        // 完全陌生的 action 類型（不屬於任何已知鍵值）目前會被靜默忽略，這裡補上診斷用的 Trace 記錄，
        // 避免 YouTube 未來新增的 action 類型在毫無記錄的情況下遺失資料。
        if (!KnownActionKeys.Any(key => singleAction.TryGetProperty(key, out _)) && _logger.IsEnabled(LogLevel.Trace))
        {
            LogMessages.UnsupportedContentEncountered(_logger, "ParseNonMessageAction -> 尚未支援的 action 類型", singleAction.GetRawText());
        }
    }

    /// <summary>
    /// 解析 replaceChatItemAction（既有留言的內容被取代／修改，例如超級留言／超級貼圖淡出後改為較小的顯示樣式）
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>List&lt;RendererData&gt;</returns>
    private List<RendererData> ParseReplaceChatItemAction(JsonElement jsonElement)
    {
        string targetItemId = jsonElement.Get("targetItemId")?.GetString() ?? string.Empty;

        JsonElement? replacementItem = jsonElement.Get("replacementItem");

        if (!replacementItem.HasValue)
        {
            return [];
        }

        List<RendererData> rendererDatas = ParseRenderer(replacementItem.Value);

        foreach (RendererData rendererData in rendererDatas)
        {
            // 被取代後的內容通常仍會帶有相同的 id，這裡僅在缺漏時補上，讓呼叫端能以 ID 對應到原留言。
            if (string.IsNullOrEmpty(rendererData.ID) && !string.IsNullOrEmpty(targetItemId))
            {
                rendererData.ID = targetItemId;
            }
        }

        return rendererDatas;
    }

    /// <summary>
    /// 解析 removeChatItemAction（留言被刪除）
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>RendererData</returns>
    private RendererData ParseRemoveChatItemAction(JsonElement jsonElement)
    {
        string targetItemId = jsonElement.Get("targetItemId")?.GetString() ?? string.Empty;

        return new RendererData()
        {
            ID = targetItemId,
            Type = GetLocalizeString(KeySet.ChatMessageDeleted),
            AuthorName = $"[{GetLocalizeString(StringSet.YouTube)}]",
            AuthorBadges = KeySet.NoAuthorBadges,
            AuthorPhotoUrl = KeySet.NoAuthorPhotoUrl,
            MessageContent = string.IsNullOrEmpty(targetItemId) ? KeySet.NoMessageContent : targetItemId,
            PurchaseAmountText = KeySet.NoPurchaseAmountText,
            ForegroundColor = KeySet.NoForegroundColor,
            BackgroundColor = KeySet.NoBackgroundColor,
            TimestampText = KeySet.NoTimestampText,
            AuthorExternalChannelID = KeySet.NoAuthorExternalChannelID
        };
    }

    /// <summary>
    /// 解析 removeChatItemByAuthorAction（使用者被封鎖，其留言全數移除）
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>RendererData</returns>
    private RendererData ParseRemoveChatItemByAuthorAction(JsonElement jsonElement)
    {
        string externalChannelId = jsonElement.Get("externalChannelId")?.GetString() ?? string.Empty;

        return new RendererData()
        {
            ID = string.Empty,
            Type = GetLocalizeString(KeySet.ChatUserBanned),
            AuthorName = $"[{GetLocalizeString(StringSet.YouTube)}]",
            AuthorBadges = KeySet.NoAuthorBadges,
            AuthorPhotoUrl = KeySet.NoAuthorPhotoUrl,
            MessageContent = string.IsNullOrEmpty(externalChannelId) ? KeySet.NoMessageContent : externalChannelId,
            PurchaseAmountText = KeySet.NoPurchaseAmountText,
            ForegroundColor = KeySet.NoForegroundColor,
            BackgroundColor = KeySet.NoBackgroundColor,
            TimestampText = KeySet.NoTimestampText,
            AuthorExternalChannelID = string.IsNullOrEmpty(externalChannelId) ?
                KeySet.NoAuthorExternalChannelID :
                externalChannelId
        };
    }

    /// <summary>
    /// 解析 pollRenderer（創作者投票）
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>RendererData</returns>
    private RendererData ParsePollRenderer(JsonElement jsonElement)
    {
        string pollId = jsonElement.Get("liveChatPollId")?.GetString() ?? string.Empty;

        RunsData questionRunsData = ParseRunData(
            jsonElement.Get("header")
                ?.Get("pollHeaderRenderer")
                ?.Get("pollQuestion") ?? default);

        List<string> choiceTexts = [];

        JsonElement.ArrayEnumerator? choices = jsonElement.Get("choices")?.ToArrayEnumerator();

        if (choices.HasValue)
        {
            foreach (JsonElement choice in choices)
            {
                RunsData choiceRunsData = ParseRunData(choice.Get("text") ?? default);

                if (!string.IsNullOrEmpty(choiceRunsData.Text))
                {
                    choiceTexts.Add(choiceRunsData.Text);
                }
            }
        }

        string message = !string.IsNullOrEmpty(questionRunsData.Text) ?
            $"{questionRunsData.Text}：{string.Join("、", choiceTexts)}" :
            string.Join("、", choiceTexts);

        return new RendererData()
        {
            ID = pollId,
            Type = GetLocalizeString(KeySet.ChatPoll),
            AuthorName = $"[{GetLocalizeString(StringSet.YouTube)}]",
            AuthorBadges = KeySet.NoAuthorBadges,
            AuthorPhotoUrl = KeySet.NoAuthorPhotoUrl,
            MessageContent = string.IsNullOrEmpty(message) ? KeySet.NoMessageContent : message,
            PurchaseAmountText = KeySet.NoPurchaseAmountText,
            ForegroundColor = KeySet.NoForegroundColor,
            BackgroundColor = KeySet.NoBackgroundColor,
            TimestampText = KeySet.NoTimestampText,
            AuthorExternalChannelID = KeySet.NoAuthorExternalChannelID
        };
    }

    /// <summary>
    /// 解析 updateLiveChatPollAction（創作者投票的即時得票率更新）
    /// <para>投票建立時（<see cref="ParsePollRenderer"/>）只會有問題與選項文字，沒有任何票數／得票率；
    /// 得票率是 YouTube 另外透過這個獨立的 action（而非 <see cref="ParseFrameworkUpdates"/> 的通用實體更新機制）
    /// 即時推送，`liveChatPollId` 與建立時相同，可用 <see cref="RendererData.ID"/> 對照回原本的投票。</para>
    /// </summary>
    /// <param name="jsonElement">JsonElement（<c>updateLiveChatPollAction.pollToUpdate.pollRenderer</c>）</param>
    /// <returns>RendererData</returns>
    private RendererData ParseUpdateLiveChatPollAction(JsonElement jsonElement)
    {
        string pollId = jsonElement.Get("liveChatPollId")?.GetString() ?? string.Empty;

        RunsData voteCountRunsData = ParseRunData(
            jsonElement.Get("header")
                ?.Get("pollHeaderRenderer")
                ?.Get("metadataText") ?? default);

        List<string> choiceResults = [];

        JsonElement.ArrayEnumerator? choices = jsonElement.Get("choices")?.ToArrayEnumerator();

        if (choices.HasValue)
        {
            foreach (JsonElement choice in choices)
            {
                RunsData choiceRunsData = ParseRunData(choice.Get("text") ?? default);
                string? votePercentage = choice.Get("votePercentage")?.Get("simpleText")?.GetString();

                if (!string.IsNullOrEmpty(choiceRunsData.Text) && !string.IsNullOrEmpty(votePercentage))
                {
                    choiceResults.Add($"{choiceRunsData.Text}：{votePercentage}");
                }
            }
        }

        string message = !string.IsNullOrEmpty(voteCountRunsData.Text) ?
            $"{string.Join("、", choiceResults)}（{voteCountRunsData.Text}）" :
            string.Join("、", choiceResults);

        return new RendererData()
        {
            ID = pollId,
            Type = GetLocalizeString(KeySet.ChatPollUpdate),
            AuthorName = $"[{GetLocalizeString(StringSet.YouTube)}]",
            AuthorBadges = KeySet.NoAuthorBadges,
            AuthorPhotoUrl = KeySet.NoAuthorPhotoUrl,
            MessageContent = string.IsNullOrEmpty(message) ? KeySet.NoMessageContent : message,
            PurchaseAmountText = KeySet.NoPurchaseAmountText,
            ForegroundColor = KeySet.NoForegroundColor,
            BackgroundColor = KeySet.NoBackgroundColor,
            TimestampText = KeySet.NoTimestampText,
            AuthorExternalChannelID = KeySet.NoAuthorExternalChannelID
        };
    }

    /// <summary>
    /// 解析 giftMessageViewModel（新版 ViewModel 結構的贈禮訊息）
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>RendererData</returns>
    private RendererData ParseGiftMessageViewModel(JsonElement jsonElement)
    {
        string id = jsonElement.Get("id")?.GetString() ?? string.Empty;

        string authorName = jsonElement.Get("authorName")?.Get("content")?.GetString() ?? KeySet.NoAuthorName;

        string messageText = jsonElement.Get("text")?.Get("content")?.GetString() ?? string.Empty;

        string authorPhotoUrl = jsonElement
            .Get("authorAvatar")
            ?.Get("avatarViewModel")
            ?.Get("image")
            ?.Get("sources")
            ?.ToArrayEnumerator()
            ?.LastOrDefault()
            .Get("url")
            ?.GetString() ?? KeySet.NoAuthorPhotoUrl;

        return new RendererData()
        {
            ID = id,
            Type = GetLocalizeString(KeySet.ChatGift),
            AuthorName = authorName,
            AuthorPhotoUrl = authorPhotoUrl,
            AuthorBadges = KeySet.NoAuthorBadges,
            MessageContent = string.IsNullOrEmpty(messageText) ? KeySet.NoMessageContent : messageText,
            PurchaseAmountText = KeySet.NoPurchaseAmountText,
            ForegroundColor = KeySet.NoForegroundColor,
            BackgroundColor = KeySet.NoBackgroundColor,
            TimestampText = KeySet.NoTimestampText,
            AuthorExternalChannelID = KeySet.NoAuthorExternalChannelID
        };
    }

    /// <summary>
    /// 解析 ticker（跑馬燈）項目，取出其內嵌的完整原始 *Renderer 內容
    /// </summary>
    /// <param name="output">List&lt;RendererData&gt;</param>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="innerRendererName">字串，跑馬燈內嵌的原始 *Renderer 名稱</param>
    private void ParseTickerRenderer(List<RendererData> output, JsonElement jsonElement, string innerRendererName)
    {
        JsonElement? innerRenderer = jsonElement
            .Get("showItemEndpoint")
            ?.Get("showLiveChatItemEndpoint")
            ?.Get("renderer")
            ?.Get(innerRendererName);

        if (innerRenderer.HasValue)
        {
            SetRendererData(
                dataSet: output,
                jsonElement: innerRenderer.Value,
                rendererName: innerRendererName,
                customRendererName: $"[ticker] {innerRendererName}");
        }
    }

    /// <summary>
    /// 解析 *Renderer 的內容
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>List&lt;RendererData&gt;</returns>
    private List<RendererData> ParseRenderer(JsonElement jsonElement)
    {
        List<RendererData> output = [];

        // 參考：https://github.com/xenova/chat-downloader/blob/master/chat_downloader/sites/youtube.py#L969
        if (jsonElement.TryGetProperty(
            "liveChatTextMessageRenderer",
            out JsonElement liveChatTextMessageRenderer))
        {
            SetRendererData(
                dataSet: output,
                jsonElement: liveChatTextMessageRenderer,
                rendererName: "liveChatTextMessageRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatPaidMessageRenderer",
            out JsonElement liveChatPaidMessageRenderer))
        {
            SetRendererData(
                dataSet: output,
                jsonElement: liveChatPaidMessageRenderer,
                rendererName: "liveChatPaidMessageRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatPaidStickerRenderer",
            out JsonElement liveChatPaidStickerRenderer))
        {
            SetRendererData(
                dataSet: output,
                jsonElement: liveChatPaidStickerRenderer,
                rendererName: "liveChatPaidStickerRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatMembershipItemRenderer",
            out JsonElement liveChatMembershipItemRenderer))
        {
            SetRendererData(
                dataSet: output,
                jsonElement: liveChatMembershipItemRenderer,
                rendererName: "liveChatMembershipItemRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatViewerEngagementMessageRenderer",
            out JsonElement liveChatViewerEngagementMessageRenderer))
        {
            SetRendererData(
                dataSet: output,
                jsonElement: liveChatViewerEngagementMessageRenderer,
                rendererName: "liveChatViewerEngagementMessageRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatModeChangeMessageRenderer",
            out JsonElement liveChatModeChangeMessageRenderer))
        {
            SetRendererData(
                dataSet: output,
                jsonElement: liveChatModeChangeMessageRenderer,
                rendererName: "liveChatModeChangeMessageRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatSponsorshipsGiftPurchaseAnnouncementRenderer",
            out JsonElement liveChatSponsorshipsGiftPurchaseAnnouncementRenderer))
        {
            SetRendererData(
                dataSet: output,
                jsonElement: liveChatSponsorshipsGiftPurchaseAnnouncementRenderer,
                rendererName: "liveChatSponsorshipsGiftPurchaseAnnouncementRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatSponsorshipsGiftRedemptionAnnouncementRenderer",
            out JsonElement liveChatSponsorshipsGiftRedemptionAnnouncementRenderer))
        {
            SetRendererData(
                dataSet: output,
                jsonElement: liveChatSponsorshipsGiftRedemptionAnnouncementRenderer,
                rendererName: "liveChatSponsorshipsGiftRedemptionAnnouncementRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatBannerRenderer",
            out JsonElement liveChatBannerRenderer))
        {
            LogMessages.Trace(_logger, "ParseRenderer -> liveChatBannerRenderer", liveChatBannerRenderer.GetRawText());

            // 2026/8 已對真實直播的置頂橫幅（addBannerToLiveChatCommand -> bannerRenderer）驗證過，
            // 底下的 header/contents 解析路徑與真實 JSON 結構一致。
            //
            // 2026/8 已釐清舊版「有插入時間順序的問題」TODO 的真正原因（用真實直播的原始 JSON 逐欄位比對過）：
            // 這個 action 一律會產生「header」與「contents」兩筆各自獨立的 RendererData：
            // - header（liveChatBannerHeaderRenderer）：Type 為「置頂留言」，內容是「由 @xxx 置頂」這則
            //   通知本身，沒有 id／timestampUsec（YouTube 原始 JSON 就沒有這兩個欄位，不是解析遺漏）。
            // - contents（通常是 liveChatTextMessageRenderer）：Type 為「一般」，帶的是被置頂的那則訊息
            //   本身「原始」的 id／timestampUsec——如果那則訊息先前已經以一般留言的身分出現過，這裡就會是
            //   同一個 id 再次出現（語意上是「重新展示」而非新訊息），且 timestampUsec 反映的是「原本發送
            //   的時間」，不是「現在被置頂的時間」，但這筆資料在輸出序列裡的位置是「現在」。
            // 這不是解析錯誤，兩筆資料的內容都與 YouTube 原始 JSON 完全吻合；只是呼叫端如果單純依賴
            // 「輸出順序＝時間順序」的假設，會在這個情境下看到一則帶著舊時間戳記的訊息出現在批次尾端。
            // 需要嚴格時間排序的呼叫端請自行以 TimestampUsec 排序，並用 ID 去重（正常的 addChatItemAction
            // 也可能因為輪詢重疊而重複收到同一個 id，去重本來就是呼叫端該做的事）。
            if (liveChatBannerRenderer.TryGetProperty(
                "header",
                out JsonElement header))
            {
                LogMessages.Trace(_logger, "ParseRenderer -> liveChatBannerRenderer -> header", header.GetRawText());

                if (header.TryGetProperty(
                    "liveChatBannerHeaderRenderer",
                    out JsonElement liveChatBannerHeaderRenderer))
                {
                    SetRendererData(
                        dataSet: output,
                        jsonElement: liveChatBannerHeaderRenderer,
                        rendererName: "liveChatBannerHeaderRenderer",
                        customRendererName: "liveChatBannerRenderer -> header -> liveChatBannerHeaderRenderer");
                }
            }

            if (liveChatBannerRenderer.TryGetProperty(
                "contents",
                out JsonElement contents))
            {
                LogMessages.Trace(_logger, "ParseRenderer -> liveChatBannerRenderer -> contents", contents.GetRawText());

                if (contents.TryGetProperty(
                    "liveChatTextMessageRenderer",
                    out JsonElement liveChatTextMessageRenderer1))
                {
                    SetRendererData(
                       dataSet: output,
                       jsonElement: liveChatTextMessageRenderer1,
                       rendererName: "liveChatTextMessageRenderer",
                       customRendererName: "liveChatBannerRenderer -> contents -> liveChatTextMessageRenderer");
                }
                else if (contents.TryGetProperty(
                    "liveChatBannerRedirectRenderer",
                    out JsonElement liveChatBannerRedirectRenderer))
                {
                    SetRendererData(
                        output,
                        liveChatBannerRedirectRenderer,
                        "liveChatBannerRedirectRenderer",
                        "liveChatBannerRenderer -> contents -> liveChatBannerRedirectRenderer");
                }
            }
        }
        else if (jsonElement.TryGetProperty(
            "giftMessageViewModel",
            out JsonElement giftMessageViewModel))
        {
            output.Add(ParseGiftMessageViewModel(giftMessageViewModel));
        }
        else if (jsonElement.TryGetProperty(
            "liveChatTickerPaidMessageItemRenderer",
            out JsonElement liveChatTickerPaidMessageItemRenderer))
        {
            ParseTickerRenderer(output, liveChatTickerPaidMessageItemRenderer, "liveChatPaidMessageRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatTickerPaidStickerItemRenderer",
            out JsonElement liveChatTickerPaidStickerItemRenderer))
        {
            ParseTickerRenderer(output, liveChatTickerPaidStickerItemRenderer, "liveChatPaidStickerRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatTickerSponsorItemRenderer",
            out JsonElement liveChatTickerSponsorItemRenderer))
        {
            ParseTickerRenderer(output, liveChatTickerSponsorItemRenderer, "liveChatSponsorshipsGiftPurchaseAnnouncementRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatDonationAnnouncementRenderer",
            out JsonElement liveChatDonationAnnouncementRenderer))
        {
            SetRendererData(
                dataSet: output,
                jsonElement: liveChatDonationAnnouncementRenderer,
                rendererName: "liveChatDonationAnnouncementRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatPurchasedProductMessageRenderer",
            out JsonElement liveChatPurchasedProductMessageRenderer))
        {
            SetRendererData(
                dataSet: output,
                jsonElement: liveChatPurchasedProductMessageRenderer,
                rendererName: "liveChatPurchasedProductMessageRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatLegacyPaidMessageRenderer",
            out JsonElement liveChatLegacyPaidMessageRenderer))
        {
            SetRendererData(
                dataSet: output,
                jsonElement: liveChatLegacyPaidMessageRenderer,
                rendererName: "liveChatLegacyPaidMessageRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatModerationMessageRenderer",
            out JsonElement liveChatModerationMessageRenderer))
        {
            SetRendererData(
                dataSet: output,
                jsonElement: liveChatModerationMessageRenderer,
                rendererName: "liveChatModerationMessageRenderer");
        }
        else if (jsonElement.TryGetProperty(
            "liveChatAutoModMessageRenderer",
            out JsonElement liveChatAutoModMessageRenderer))
        {
            SetRendererData(
                dataSet: output,
                jsonElement: liveChatAutoModMessageRenderer,
                rendererName: "liveChatAutoModMessageRenderer");
        }
        else if (jsonElement.TryGetProperty("liveChatPlaceholderItemRenderer", out _))
        {
            // liveChatPlaceholderItemRenderer 僅為 UI 佔位用途，無實際內容，略過不處理。
            LogMessages.Trace(_logger, "ParseRenderer -> 略過不處理的內容", jsonElement.GetRawText());
        }
        else
        {
            LogMessages.UnsupportedContentEncountered(_logger, "ParseRenderer -> 尚未支援的內容", jsonElement.GetRawText());
        }

        return output;
    }

    /// <summary>
    /// 解析 authorBadges
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>AuthorBadgesData</returns>
    private AuthorBadgesData ParseAuthorBadges(JsonElement jsonElement)
    {
        AuthorBadgesData output = new();

        JsonElement.ArrayEnumerator? authorBadges = jsonElement
            .Get("authorBadges")
            ?.ToArrayEnumerator();

        if (authorBadges.HasValue)
        {
            List<BadgeData> tempBadges = [];

            foreach (JsonElement singleAuthorBadge in authorBadges)
            {
                BadgeData badgeData = new();

                // 自定義預覽圖。
                JsonElement? customThumbnail = singleAuthorBadge
                    .Get("liveChatAuthorBadgeRenderer")
                    ?.Get("customThumbnail");

                if (customThumbnail.HasValue)
                {
                    badgeData.Url = GetThumbnailUrl(customThumbnail.Value);
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
    /// <returns>MessageData</returns>
    private MessageData ParseMessageData(JsonElement jsonElement)
    {
        MessageData output = new();

        string tempText = string.Empty;
        string? tempTextColor = string.Empty,
            tempFontFace = string.Empty;

        bool isBold = false;

        List<StickerData> tempStickers = [];
        List<EmojiData> tempEmojis = [];

        JsonElement? headerPrimaryText = jsonElement.Get("headerPrimaryText");

        if (headerPrimaryText.HasValue)
        {
            RunsData runsData = ParseRunData(headerPrimaryText.Value);

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

            RunsData runsData = ParseRunData(headerSubtext.Value);

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
            RunsData runsData = ParseRunData(primaryText.Value);

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
            RunsData runsData = ParseRunData(text.Value);

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
            RunsData runsData = ParseRunData(subtext.Value);

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
                    tempText += $":{label?.GetString()}:";
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
                    tempText += $" [{content?.GetString()}] ";
                }

                stickerData.ID = label.HasValue ? label?.GetString() : string.Empty;
                stickerData.Url = GetThumbnailUrl(sticker);
                stickerData.Text = label.HasValue ? $":{label?.GetString()}:" : string.Empty;
                stickerData.Label = label.HasValue ? label?.GetString() : string.Empty;

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    LogMessages.Trace(_logger, "ParseMessageData -> sticker", sticker?.GetRawText() ?? string.Empty);
                }

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
            RunsData runsData = ParseRunData(message.Value);

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
            RunsData runsData = ParseRunData(bannerMessage.Value);

            tempText += runsData.Text;

            isBold = runsData.Bold ?? false;
            tempTextColor = runsData.TextColor;
            tempFontFace = runsData.FontFace;

            if (runsData.Emojis != null)
            {
                tempEmojis.AddRange(runsData.Emojis);
            }
        }

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogMessages.Trace(_logger, "ParseMessageData", jsonElement.GetRawText());
        }

        if (string.IsNullOrEmpty(tempText))
        {
            tempText = KeySet.NoMessageContent;
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
    /// <returns>RunsData</returns>
    private RunsData ParseRunData(JsonElement jsonElement)
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

            List<EmojiData> tempEmojis = [];

            foreach (JsonElement singleRun in runs)
            {
                JsonElement? text = singleRun.Get("text");

                if (text.HasValue)
                {
                    tempText += text?.GetString();
                }

                JsonElement? bold = singleRun.Get("bold");

                if (bold.HasValue)
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

                        emojiData.Url = GetThumbnailUrl(image);

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

                        // 2023/8/17 因為部分 "emoji" 的 "label" 也是 "emoji" 本身，所以改回取 "shortcuts" 的值。
                        //stickerData.Text = label.HasValue ? $":{label?.GetString()}:" : string.Empty;
                        emojiData.Label = label.HasValue ? label?.GetString() : string.Empty;

                        JsonElement? isCustomEmoji = emoji?.Get("isCustomEmoji");

                        if (isCustomEmoji.HasValue)
                        {
                            emojiData.IsCustomEmoji = isCustomEmoji?.GetBoolean() ?? false;
                        }

                        if (_logger.IsEnabled(LogLevel.Trace))
                        {
                            LogMessages.Trace(_logger, "ParseRunData -> emoji", emoji?.GetRawText() ?? string.Empty);
                        }

                        tempEmojis.Add(emojiData);
                    }
                }

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    LogMessages.Trace(_logger, "ParseRunData", singleRun.GetRawText());
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
    /// <param name="dataSet">List&lt;RendererData&gt;</param>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="rendererName">字串，*Renderer 的名稱，預設值為空白</param>
    /// <param name="customRendererName">字串，自定義 *Renderer 的名稱，預設值為空白</param>
    private void SetRendererData(
        List<RendererData> dataSet,
        JsonElement jsonElement,
        string rendererName = "",
        string customRendererName = "")
    {
        string effectiveRendererName = !string.IsNullOrEmpty(customRendererName) ? customRendererName : rendererName;

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            LogMessages.Trace(_logger, $"SetRendererData -> {effectiveRendererName}", jsonElement.GetRawText());
        }

        AuthorBadgesData authorBadgesData = ParseAuthorBadges(jsonElement);
        MessageData messageData = ParseMessageData(jsonElement);

        string id = GetID(jsonElement),
            type = GetRendererDataType(rendererName),
            timestampUsec = GetTimestampUsec(jsonElement),
            authorName = GetAuthorName(jsonElement),
            authorPhoto = GetAuthorPhoto(jsonElement),
            authorBadges = authorBadgesData.Text ?? KeySet.NoAuthorBadges,
            message = messageData.Text ?? KeySet.NoMessageContent,
            purchaseAmountText = GetPurchaseAmountText(jsonElement),
            forgroundColor = messageData?.TextColor ?? KeySet.NoForegroundColor,
            backgroundColor = GetBackgroundColor(jsonElement),
            timestampText = GetTimestampText(jsonElement),
            authorExternalChannelID = GetAuthorExternalChannelId(jsonElement);

        string? headerBackgroundColor = GetHeaderBackgroundColor(jsonElement);
        string? leaderboardRank = GetLeaderboardRank(jsonElement);
        string? replyCountEntityKey = GetReplyCountEntityKey(jsonElement);

        #region 處理特例

        if (type == StringSet.YouTube)
        {
            authorName = $"[{type}]";
        }

        if (rendererName == "liveChatMembershipItemRenderer")
        {
            // 此處 message 為 headerSubtext，依據 message 是否帶有關鍵字來更新 type。
            if (message.Contains(
                GetLocalizeString(KeySet.MemberUpgrade),
                StringComparison.InvariantCultureIgnoreCase))
            {
                type = GetLocalizeString(KeySet.ChatMemberUpgrade);
            }
            else if (message.Contains(
                GetLocalizeString(KeySet.MemberMilestone),
                StringComparison.InvariantCultureIgnoreCase))
            {
                type = GetLocalizeString(KeySet.ChatMemberMilestone);
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
                authorBadgesData = ParseAuthorBadges(liveChatSponsorshipsHeaderRenderer.Value);
                messageData = ParseMessageData(liveChatSponsorshipsHeaderRenderer.Value);

                authorName = GetAuthorName(liveChatSponsorshipsHeaderRenderer.Value);
                authorPhoto = GetAuthorPhoto(liveChatSponsorshipsHeaderRenderer.Value);
                authorBadges = authorBadgesData.Text ?? KeySet.NoAuthorBadges;
                // 此處 message 為 primaryText。
                message = messageData.Text ?? KeySet.NoMessageContent;
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
            HeaderBackgroundColor = headerBackgroundColor,
            LeaderboardRank = leaderboardRank,
            ReplyCountEntityKey = replyCountEntityKey,
            TimestampText = timestampText,
            AuthorExternalChannelID = authorExternalChannelID,
            Stickers = messageData?.Stickers,
            Emojis = messageData?.Emojis,
            Badges = authorBadgesData?.Badges
        });
    }

    /// <summary>
    /// 取得 id
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetID(JsonElement? jsonElement)
    {
        string output = string.Empty;

        JsonElement? id = jsonElement?.Get("id");

        if (id.HasValue)
        {
            output = id.Value.GetString() ?? string.Empty;
        }

        return output;
    }

    /// <summary>
    /// 取得 authorName
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetAuthorName(JsonElement? jsonElement)
    {
        string output = string.Empty;

        JsonElement? simpleText = jsonElement
            ?.Get("authorName")
            ?.Get("simpleText");

        if (simpleText.HasValue)
        {
            output = simpleText.Value.GetString() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(output))
        {
            output = KeySet.NoAuthorName;
        }

        return output;
    }

    /// <summary>
    /// 取得 authorPhoto
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private string GetAuthorPhoto(JsonElement? jsonElement)
    {
        string output = string.Empty;

        JsonElement? authorPhoto = jsonElement?.Get("authorPhoto");

        if (authorPhoto.HasValue)
        {
            output = GetThumbnailUrl(authorPhoto.Value);
        }

        if (string.IsNullOrEmpty(output))
        {
            output = KeySet.NoAuthorPhotoUrl;
        }

        return output;
    }

    /// <summary>
    /// 取得 authorExternalChannelId
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetAuthorExternalChannelId(JsonElement? jsonElement)
    {
        string output = string.Empty;

        JsonElement? authorExternalChannelId = jsonElement?.Get("authorExternalChannelId");

        if (authorExternalChannelId.HasValue)
        {
            output = authorExternalChannelId.Value.GetString() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(output))
        {
            output = KeySet.NoAuthorExternalChannelID;
        }

        return output;
    }

    /// <summary>
    /// 取得 timestampUsec
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private string GetTimestampUsec(JsonElement? jsonElement)
    {
        string output = string.Empty;

        JsonElement? timestampUsec = jsonElement?.Get("timestampUsec");

        if (timestampUsec.HasValue &&
            long.TryParse(timestampUsec.Value.GetString(), out long rawTimestamp))
        {
            // 將 Microseconds 轉換成 Miliseconds。
            long timestamp = rawTimestamp / 1000L;

            // 2026/8 已實測驗證：對 DictRegion 目前收錄的全部 65 種語系逐一呼叫
            // DateTimeOffset.ToString(CultureInfo) 皆能正確轉換（zh-CN／zh-TW／ja／ko 等皆已確認格式正確、
            // 無例外拋出），不會有部分語系轉換失敗或格式錯亂的情況。
            bool hasRegionData = DictionarySet.GetRegionDictionary()
                .TryGetValue(
                    SharedDisplayLanguage,
                    out RegionData? regionData);

            output = hasRegionData ?
                DateTimeOffset.FromUnixTimeMilliseconds(timestamp)
                    .LocalDateTime
                    .ToString(regionData?.GetCultureInfo()) :
                DateTimeOffset.FromUnixTimeMilliseconds(timestamp)
                    .LocalDateTime
                    .ToString();
        }

        return output;
    }

    /// <summary>
    /// 取得 timestampText
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetTimestampText(JsonElement? jsonElement)
    {
        string output = string.Empty;

        JsonElement? simpleText = jsonElement
            ?.Get("timestampText")
            ?.Get("simpleText");

        if (simpleText.HasValue)
        {
            output = simpleText.Value.GetString() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(output))
        {
            output = KeySet.NoTimestampText;
        }

        return output;
    }

    /// <summary>
    /// 取得 purchaseAmountText
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetPurchaseAmountText(JsonElement? jsonElement)
    {
        string output = string.Empty;

        JsonElement? simpleText = jsonElement
            ?.Get("purchaseAmountText")
            ?.Get("simpleText");

        if (simpleText.HasValue)
        {
            output = simpleText.Value.GetString() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(output))
        {
            return KeySet.NoPurchaseAmountText;
        }

        return output;
    }

    /// <summary>
    /// 取得背景顏色
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetBackgroundColor(JsonElement? jsonElement)
    {
        string output = string.Empty;

        JsonElement? backgroundColor = jsonElement?.Get("backgroundColor");

        if (backgroundColor.HasValue)
        {
            output = GetColorHexCode(backgroundColor.Value.GetInt64());
        }

        JsonElement? bodyBackgroundColor = jsonElement?.Get("bodyBackgroundColor");

        if (bodyBackgroundColor.HasValue)
        {
            output = GetColorHexCode(bodyBackgroundColor.Value.GetInt64());
        }

        if (string.IsNullOrEmpty(output))
        {
            output = KeySet.NoBackgroundColor;
        }

        return output;
    }

    /// <summary>
    /// 取得標頭背景顏色（僅付費類 Renderer，例如超級留言／超級貼圖才會有）
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串，找不到時為 null（代表此訊息類型不適用，非資料缺漏）</returns>
    private static string? GetHeaderBackgroundColor(JsonElement? jsonElement)
    {
        JsonElement? headerBackgroundColor = jsonElement?.Get("headerBackgroundColor");

        return headerBackgroundColor.HasValue ?
            GetColorHexCode(headerBackgroundColor.Value.GetInt64()) :
            null;
    }

    /// <summary>
    /// 取得排行榜徽章的名次文字（例如 "#1"，掛在超級留言／超級貼圖上的排行榜皇冠徽章）
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串，找不到時為 null（代表此訊息沒有排行榜徽章，非資料缺漏）</returns>
    private static string? GetLeaderboardRank(JsonElement? jsonElement)
    {
        JsonElement? title = jsonElement?.Get("leaderboardBadge")
            ?.Get("buttonViewModel")
            ?.Get("title");

        return title?.GetString();
    }

    /// <summary>
    /// 取得回覆數更新事件的關聯鍵值（僅付費類 Renderer，例如超級留言／超級貼圖才會有回覆按鈕）
    /// <para>YouTube 把回覆數當成獨立的「實體」（entity），不會直接內嵌在訊息 JSON 裡，
    /// 而是透過這個 key 對照到後續某一批回應內 <c>frameworkUpdates.entityBatchUpdate.mutations</c>
    /// 裡面同一個 key 的 <c>replyCountEntity</c>（見 <see cref="ParseFrameworkUpdates"/>）。</para>
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串，找不到時為 null（代表此訊息類型不適用，非資料缺漏）</returns>
    private static string? GetReplyCountEntityKey(JsonElement? jsonElement)
    {
        JsonElement? replyCountEntityKey = jsonElement?.Get("replyButton")
            ?.Get("pdgReplyButtonViewModel")
            ?.Get("replyCountEntityKey");

        return replyCountEntityKey?.GetString();
    }

    /// <summary>
    /// 取得 videoOffsetTimeMsec
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private static string GetVideoOffsetTimeMsec(JsonElement? jsonElement)
    {
        string output = string.Empty;

        JsonElement? videoOffsetTimeMsec = jsonElement?.Get("videoOffsetTimeMsec");

        if (videoOffsetTimeMsec.HasValue)
        {
            long milliseconds = videoOffsetTimeMsec.Value.GetInt64();

            output = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).ToString("HH:mm:ss");
        }

        return output;
    }

    /// <summary>
    /// 取得預覽圖網址
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    private string GetThumbnailUrl(JsonElement? jsonElement)
    {
        string output = string.Empty;

        JsonElement.ArrayEnumerator? thumbnails = jsonElement
            ?.Get("thumbnails")
            ?.ToArrayEnumerator();

        if (thumbnails.HasValue && thumbnails?.Any() == true)
        {
            int index = SharedIsFetchLargePicture ? 1 : 0;

            if (thumbnails?.Count() == 1)
            {
                index = 0;
            }

            // badge -> 0：16x16、1：32x32
            // image -> 0：24x24、1：48x48
            // authorPhoto -> 0：32x32、1：64x64
            // sticker -> 0：72x72、1：144x144
            JsonElement? url = thumbnails?.Get(index)?.Get("url");

            if (url.HasValue)
            {
                output = url?.GetString() ?? string.Empty;

                // 貼圖的網址會沒有 Protocol，需要手動再補上。
                if (!string.IsNullOrEmpty(output) && output.StartsWith("//"))
                {
                    output = $"https:{output}";

                    // 移除尾端的 =s144-rwa 以取得非 WebP 格式的圖檔網址。
                    // 疑似 System.Drawing 的 Image 不支援動畫的 WebP。 
                    string[] tempArray = output.Split("=");

                    // 當陣列數量大於 1 時才執行後續的操作。
                    if (tempArray.Length > 1)
                    {
                        // 取得 "=" 之前的網址部分。
                        output = tempArray[0];
                    }
                }
            }
        }

        return output;
    }
}