using Color = System.Drawing.Color;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Sets;
using StringSet = YTLiveChatCatcher.Common.Sets.StringSet;
using YTLiveChatCatcher.Extensions;
using Rubujo.YouTube.Utility.Models.LiveChat;
using System.Collections.Generic;
using YTLiveChatCatcher.Common.Utils;

namespace YTLiveChatCatcher;

// 阻擋設計工具。
partial class DesignerBlocker { };

/// <summary>
/// FMain 的 EPPlus 工具
/// </summary>
public partial class FMain
{
    /// <summary>
    /// 載入 *.xlsx 檔案
    /// </summary>
    /// <param name="filePath">字串，*.xlsx 檔案的路徑</param>
    /// <returns>Task</returns>
    public Task LoadXLSX(string filePath)
    {
        return Task.Run(async () =>
        {
            ExcelPackage.License.SetNonCommercialOrganization(StringSet.NonCommercialOrganization);

            using ExcelPackage package = new(filePath);

            string subject = package.Workbook.Properties.Subject;

            if (!string.IsNullOrEmpty(subject))
            {
                if (Uri.IsWellFormedUriString(subject, UriKind.Absolute))
                {
                    await TBVideoID.InvokeAsyncIfRequired(() =>
                    {
                        TBVideoID.Text = subject;
                    });
                }
            }

            #region 聊天室記錄

            ExcelWorksheet? sheet1 = package.Workbook.Worksheets
                .FirstOrDefault(n => n.Name == StringSet.SheetName1);

            if (sheet1 == null)
            {
                MessageBox.Show(
                    "匯入失敗，請選擇有效檔案。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // 2026/9 修正：itemsSeenWithoutId 讓「沒有 ID 值」的舊格式列改用 O(1) 雜湊查找去重，
            // 取代原本對整份累積清單做 .Any(...) 的 O(n) 線性掃描——舊格式匯出檔案沒有 ID 欄位是
            // 被刻意保留支援的真實情境（見 AGENTS.md），大檔案（例如上萬列）下線性掃描會讓匯入
            // 耗時從數秒暴增到數十秒。
            HashSet<(string AuthorName, string TimestampUsec)> itemsSeenWithoutId = [];

            // 2026/9 修正：AutoFitListViewColumns 原本被整份匯入檔案（可能上千列）一次呼叫，在
            // UI 執行緒同步跑完全部欄寬量測，會讓畫面在匯入大檔案時凍結數秒。改成每累積到一定
            // 筆數就先把這批 flush 進畫面（呼叫模式比照 DoProcessMessages 逐批處理的做法），
            // 讓 UI 有機會在批次之間處理繪製／輸入訊息，不會整段匯入期間完全沒有回應。
            const int FlushBatchSize = 200;

            List<ListViewItem> currentBatch = [];

            async Task FlushCurrentBatchAsync()
            {
                if (currentBatch.Count == 0)
                {
                    return;
                }

                List<ListViewItem> batchToFlush = currentBatch;

                currentBatch = [];

                await LVLiveChatList.InvokeAsyncIfRequired(() =>
                {
                    LVLiveChatList.BeginUpdate();
                    AutoFitListViewColumns(LVLiveChatList, batchToFlush);
                    SharedListViewItems.AddRange(batchToFlush);
                    LVLiveChatList.VirtualListSize = SharedListViewItems.Count;

                    if (SharedListViewItems.Count > 0)
                    {
                        LVLiveChatList.EnsureVisible(SharedListViewItems.Count - 1);
                    }

                    LVLiveChatList.EndUpdate();

                    // 保證落在沒有任何 BeginUpdate 視窗的時間點，強制重繪目前可視範圍，撿回前面
                    // 批次因為 RedrawItems 撞上 BeginUpdate 視窗而被吃掉的頭像重繪（見
                    // DoProcessMessages 頭像下載完成處的說明）。匯入大檔案時批次數量可能很多，
                    // 改用節流版避免短時間內觸發大量重複的重繪。
                    InvalidateLiveChatListThrottled();
                });
            }

            // 2026/9 修正：原本用兩個計數器——for 迴圈的 i 判斷是否結束、獨立的 rowIdx1 定位儲存格，
            // 只在「從不觸發 continue」時兩者才同步遞增。下面「type 為空就 continue」那段會跳過
            // rowIdx1++（原本寫在迴圈最後），導致下一輪用同一個 rowIdx1 再讀一次同一列、再度是空
            // type、再度 continue……i 持續逼近 EndRow 但 rowIdx1 永遠卡住，之後所有列都不會被讀到，
            //且沒有任何錯誤提示，資料被靜默截斷。改成只用單一個 i 當儲存格列索引，不再需要
            // 另一個計數器保持同步。
            for (int i = 2; i <= sheet1.Rows.EndRow; i++)
            {
                string authorName = sheet1.Cells[i, 2].Text;
                string authorBages = sheet1.Cells[i, 3].Text;
                string authorPhotoUrl = sheet1.Cells[i, 11].Text;
                string messageContent = sheet1.Cells[i, 4].Text;
                string purchaseAmmount = sheet1.Cells[i, 5].Text;
                string timestampUsec = sheet1.Cells[i, 6].Text;
                string type = sheet1.Cells[i, 7].Text;
                string foregroundColor = sheet1.Cells[i, 8].Text;
                string backgroundColor = sheet1.Cells[i, 9].Text;
                string timestampText = sheet1.Cells[i, 10].Text;
                string authorExternalChannelID = sheet1.Cells[i, 12].Text;
                string id = sheet1.Cells[i, 13].Text;
                // 2026/8 新增：舊版匯出的 *.xlsx 檔案不會有這幾欄，讀取到空字串是正常情況。
                string leaderboardRank = sheet1.Cells[i, 14].Text;
                string replyCount = sheet1.Cells[i, 15].Text;
                string headerBackgroundColor = sheet1.Cells[i, 16].Text;
                string replyCountEntityKey = sheet1.Cells[i, 17].Text;

                // 當 "type" 為 null 或空值時，直接進入下一個。
                if (string.IsNullOrEmpty(type))
                {
                    continue;
                }

                ListViewItem lvItem = new(authorName)
                {
                    UseItemStyleForSubItems = false
                };

                if (authorBages.Contains(StringSet.BadgeOwner))
                {
                    lvItem.SubItems[0].ForeColor = Color.Orange;
                }
                else if (authorBages.Contains(StringSet.BadgeModerator))
                {
                    lvItem.SubItems[0].ForeColor = Color.Blue;
                }
                else if (authorBages.Contains(StringSet.BadgeValid))
                {
                    lvItem.SubItems[0].ForeColor = Color.Purple;
                }
                else if (authorBages.Contains(StringSet.BadgeMember))
                {
                    lvItem.SubItems[0].ForeColor = Color.Green;
                }
                else
                {
                    lvItem.SubItems[0].ForeColor = Color.Black;
                }

                string[] subItemContents =
                [
                    authorBages,
                    messageContent,
                    purchaseAmmount,
                    timestampUsec,
                    type,
                    foregroundColor,
                    backgroundColor,
                    timestampText,
                    authorPhotoUrl,
                    authorExternalChannelID,
                    id,
                    leaderboardRank,
                    replyCount,
                    headerBackgroundColor,
                    replyCountEntityKey,
                ];

                lvItem.SubItems.AddRange(subItemContents);

                if (authorName == $"[{Rubujo.YouTube.Utility.Sets.StringSet.YouTube}]" ||
                    authorName == $"[{StringSet.AppName}]")
                {
                    foreach (ListViewItem.ListViewSubItem item in lvItem.SubItems)
                    {
                        item.ForeColor = Color.White;
                        item.BackColor = ColorTranslator.FromHtml("#3e3e3e");
                    }
                }

                if (ChatColorUtil.TryParse(foregroundColor, out Color parsedForegroundColor))
                {
                    for (int j = 0; j < lvItem.SubItems.Count; j++)
                    {
                        // 只變更訊息欄位的前景色。
                        if (j == 2)
                        {
                            ListViewItem.ListViewSubItem item = lvItem.SubItems[j];

                            item.ForeColor = parsedForegroundColor;
                        }
                    }
                }

                if (ChatColorUtil.TryParse(backgroundColor, out Color parsedBackgroundColor))
                {
                    foreach (ListViewItem.ListViewSubItem item in lvItem.SubItems)
                    {
                        item.BackColor = parsedBackgroundColor;
                    }
                }

                if (ChatColorUtil.TryParse(headerBackgroundColor, out Color headerColor))
                {
                    int[] headerSubItemIndexes = [0, 1, 3, 4];

                    foreach (int headerSubItemIndex in headerSubItemIndexes)
                    {
                        lvItem.SubItems[headerSubItemIndex].BackColor = headerColor;
                    }
                }

                if (type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatJoinMember) ||
                   type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatMemberUpgrade) ||
                   type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatMemberMilestone) ||
                   type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatMemberGift) ||
                   type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatReceivedMemberGift))
                {
                    foreach (ListViewItem.ListViewSubItem item in lvItem.SubItems)
                    {
                        item.ForeColor = Color.White;
                        item.BackColor = Color.Green;
                    }
                }

                if (type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatRedirect) ||
                    type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatPinned))
                {
                    foreach (ListViewItem.ListViewSubItem item in lvItem.SubItems)
                    {
                        item.ForeColor = Color.White;
                        item.BackColor = ColorTranslator.FromHtml("#203d6c");
                    }
                }

                if (!string.IsNullOrEmpty(authorPhotoUrl))
                {
                    // 理由同 DoProcessMessages 對應的修正：顯示名稱不保證唯一，改用頻道 ID 當 key。
                    string imgKey = !string.IsNullOrEmpty(authorExternalChannelID) ?
                        authorExternalChannelID :
                        authorName;

                    LVLiveChatList.InvokeIfRequired(async () =>
                    {
                        // 這裡是 fire-and-forget 的 async void 委派，
                        // 一定要在內部自己攔截例外，否則會直接讓整個應用程式當掉。
                        try
                        {
                            if (LVLiveChatList.SmallImageList != null)
                            {
                                string errorMessage = await LVLiveChatList.SmallImageList
                                    .Images
                                    .SetAuthorPhoto(
                                        SharedHttpClient,
                                        imgKey,
                                        authorPhotoUrl);

                                if (!string.IsNullOrEmpty(errorMessage))
                                {
                                    WriteLog(errorMessage);
                                }

                                // 2026/8 修正（真正的根本原因，理由同 DoProcessMessages 對應的修正）：
                                // VirtualMode 下用字串鍵值的 ImageKey 是已知不可靠的做法，必須改用整數索引
                                // 的 ImageIndex；下載完成後才知道實際索引，用 IndexOfKey 查出來再指定。
                                int imageIndex = LVLiveChatList.SmallImageList.Images.IndexOfKey(imgKey);

                                if (imageIndex >= 0)
                                {
                                    lvItem.ImageIndex = imageIndex;
                                }

                                // 理由同 DoProcessMessages 對應的修正：下載本身是背景中各自獨立完成的
                                // 非同步工作，可能晚於呼叫端「最後一次」的 Invalidate() 才真正完成，
                                // 額外主動補一次節流過的 Invalidate() 當保險，避免頭像永久空白。
                                InvalidateLiveChatListThrottled();
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());
                        }
                    });
                }

                // 先過濾以避免加入到重複的資料：優先以訊息 ID 判斷，沒有 ID 值時才退回舊版判斷方式，
                // 邏輯與 DoProcessMessages 一致。
                bool isDuplicate = !string.IsNullOrEmpty(id) ?
                    SharedItemsByMessageID.ContainsKey(id) :
                    itemsSeenWithoutId.Contains((authorName, timestampUsec));

                if (!isDuplicate)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        SharedItemsByMessageID[id] = lvItem;
                    }
                    else
                    {
                        itemsSeenWithoutId.Add((authorName, timestampUsec));
                    }

                    if (!string.IsNullOrEmpty(authorExternalChannelID))
                    {
                        if (!SharedItemsByAuthorChannelID.TryGetValue(authorExternalChannelID, out List<ListViewItem>? authorItems))
                        {
                            authorItems = [];

                            SharedItemsByAuthorChannelID[authorExternalChannelID] = authorItems;
                        }

                        authorItems.Add(lvItem);
                    }

                    if (!string.IsNullOrEmpty(replyCountEntityKey))
                    {
                        SharedItemsByReplyCountEntityKey[replyCountEntityKey] = lvItem;
                    }

                    RegisterNewListViewItemStats(type, authorBages, authorName, purchaseAmmount);

                    currentBatch.Add(lvItem);

                    if (currentBatch.Count >= FlushBatchSize)
                    {
                        await FlushCurrentBatchAsync();
                    }
                }
            }

            await FlushCurrentBatchAsync();

            UpdateSummaryInfo();

            #endregion

            #region 自定義表情符號

            ExcelWorksheet? sheet2 = package.Workbook.Worksheets
                .FirstOrDefault(n => n.Name == StringSet.SheetName4);

            if (sheet2 != null)
            {
                int rowIdx2 = 2;

                for (int i = rowIdx2; i <= sheet2.Rows.EndRow; i++)
                {
                    string text = sheet2.Cells[rowIdx2, 2].Text;
                    string format = sheet2.Cells[rowIdx2, 3].Text;
                    string id = sheet2.Cells[rowIdx2, 4].Text;
                    string url = sheet2.Cells[rowIdx2, 5].Text;
                    string label = sheet2.Cells[rowIdx2, 6].Text;
                    bool isCustomEmoji = bool.TryParse(sheet2.Cells[rowIdx2, 7].Value?.ToString(), out bool result) && result;

                    if (!string.IsNullOrEmpty(id))
                    {
                        EmojiData emojiData = new()
                        {
                            ID = id,
                            Text = text,
                            Label = label,
                            Url = url,
                            IsCustomEmoji = isCustomEmoji,
                            Format = format
                        };

                        if (!SharedCustomEmojis.Any(n => n.ID == emojiData.ID))
                        {
                            if (emojiData.IsCustomEmoji)
                            {
                                // 2025/4/17 取消在匯入時下載圖片。
                                //string errorMessage = await emojiData.SetImage(
                                //    SharedHttpClient,
                                //    SharedYTJsonParser.FetchLargePicture());

                                //if (!string.IsNullOrEmpty(errorMessage))
                                //{
                                //    WriteLog(errorMessage);
                                //}

                                SharedCustomEmojis.Add(emojiData);
                            }
                            else
                            {
                                SharedCustomEmojis.Add(emojiData);
                            }
                        }
                    }

                    rowIdx2++;
                }

                WriteLog($"已匯入 {SharedCustomEmojis.Count} 個情符號資料。");
            }

            #endregion

            #region 會員徽章

            ExcelWorksheet? sheet3 = package.Workbook.Worksheets
                .FirstOrDefault(n => n.Name == StringSet.SheetName5);

            if (sheet3 != null)
            {
                int rowIdx3 = 2;

                for (int i = rowIdx3; i <= sheet3.Rows.EndRow; i++)
                {
                    string label = sheet3.Cells[rowIdx3, 2].Text;
                    string format = sheet3.Cells[rowIdx3, 3].Text;
                    string tooltip = sheet3.Cells[rowIdx3, 4].Text;
                    string url = sheet3.Cells[rowIdx3, 5].Text;
                    string iconType = sheet3.Cells[rowIdx3, 6].Text;

                    if (!string.IsNullOrEmpty(label))
                    {
                        BadgeData badgeData = new()
                        {
                            Label = label,
                            Tooltip = tooltip,
                            Url = url,
                            IconType = iconType == string.Empty ? null : iconType,
                            Format = format
                        };

                        if (!SharedBadges.Any(n => n.Label == badgeData.Label) &&
                            badgeData.Label.Contains(StringSet.Member))
                        {
                            // 2025/4/17 取消在匯入時下載圖片。
                            //if (!string.IsNullOrEmpty(badgeData.Url))
                            //{
                            //    string errorMessage = await badgeData.SetImage(
                            //        SharedHttpClient,
                            //        SharedYTJsonParser.FetchLargePicture());

                            //    if (!string.IsNullOrEmpty(errorMessage))
                            //    {
                            //        WriteLog(errorMessage);
                            //    }
                            //}

                            SharedBadges.Add(badgeData);
                        }
                    }

                    rowIdx3++;
                }

                WriteLog($"已匯入 {SharedBadges.Count} 個會員徽章資料。");
            }

            #endregion

            #region 超級貼圖

            ExcelWorksheet? sheet4 = package.Workbook.Worksheets
                .FirstOrDefault(n => n.Name == StringSet.SheetName6);

            if (sheet4 != null)
            {
                int rowIdx4 = 2;

                for (int i = rowIdx4; i <= sheet4.Rows.EndRow; i++)
                {
                    string label = sheet4.Cells[rowIdx4, 2].Text;
                    string format = sheet4.Cells[rowIdx4, 3].Text;
                    string url = sheet4.Cells[rowIdx4, 4].Text;

                    if (!string.IsNullOrEmpty(label))
                    {
                        StickerData stickerData = new()
                        {
                            Label = label,
                            Url = url,
                            Format = format
                        };

                        if (!SharedStickers.Any(n => n.Url == stickerData.Url))
                        {
                            // 2025/4/17 取消在匯入時下載圖片。
                            //if (!string.IsNullOrEmpty(stickerData.Url))
                            //{
                            //    string errorMessage = await stickerData.SetImage(
                            //        SharedHttpClient,
                            //        SharedYTJsonParser.FetchLargePicture());

                            //    if (!string.IsNullOrEmpty(errorMessage))
                            //    {
                            //        WriteLog(errorMessage);
                            //    }
                            //}

                            SharedStickers.Add(stickerData);
                        }
                    }

                    rowIdx4++;
                }

                WriteLog($"已匯入 {SharedStickers.Count} 個超級貼圖資料。");
            }

            #endregion
        });
    }
}
