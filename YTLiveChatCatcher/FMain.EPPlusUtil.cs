using Color = System.Drawing.Color;
using OfficeOpenXml;
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Sets;
using StringSet = YTLiveChatCatcher.Common.Sets.StringSet;
using YTLiveChatCatcher.Extensions;
using Rubujo.YouTube.Utility.Models.LiveChat;
using System.Collections.Generic;

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
        return Task.Run(() =>
        {
            ExcelPackage.License.SetNonCommercialOrganization(StringSet.NonCommercialOrganization);

            using ExcelPackage package = new(filePath);

            string subject = package.Workbook.Properties.Subject;

            if (!string.IsNullOrEmpty(subject))
            {
                if (Uri.IsWellFormedUriString(subject, UriKind.Absolute))
                {
                    TBVideoID.InvokeIfRequired(() =>
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

            List<ListViewItem> listTempItem = [];

            int rowIdx1 = 2;

            for (int i = rowIdx1; i <= sheet1.Rows.EndRow; i++)
            {
                string authorName = sheet1.Cells[rowIdx1, 2].Text;
                string authorBages = sheet1.Cells[rowIdx1, 3].Text;
                string authorPhotoUrl = sheet1.Cells[rowIdx1, 11].Text;
                string messageContent = sheet1.Cells[rowIdx1, 4].Text;
                string purchaseAmmount = sheet1.Cells[rowIdx1, 5].Text;
                string timestampUsec = sheet1.Cells[rowIdx1, 6].Text;
                string type = sheet1.Cells[rowIdx1, 7].Text;
                string foregroundColor = sheet1.Cells[rowIdx1, 8].Text;
                string backgroundColor = sheet1.Cells[rowIdx1, 9].Text;
                string timestampText = sheet1.Cells[rowIdx1, 10].Text;
                string authorExternalChannelID = sheet1.Cells[rowIdx1, 12].Text;
                string id = sheet1.Cells[rowIdx1, 13].Text;

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

                if (!string.IsNullOrEmpty(foregroundColor))
                {
                    for (int j = 0; j < lvItem.SubItems.Count; j++)
                    {
                        // 只變更訊息欄位的前景色。
                        if (j == 2)
                        {
                            ListViewItem.ListViewSubItem item = lvItem.SubItems[j];

                            item.ForeColor = ColorTranslator.FromHtml(foregroundColor);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(backgroundColor))
                {
                    foreach (ListViewItem.ListViewSubItem item in lvItem.SubItems)
                    {
                        item.BackColor = ColorTranslator.FromHtml(backgroundColor);
                    }
                }

                if (type == YTJsonParser.GetLocalizeString(KeySet.ChatJoinMember) ||
                   type == YTJsonParser.GetLocalizeString(KeySet.ChatMemberUpgrade) ||
                   type == YTJsonParser.GetLocalizeString(KeySet.ChatMemberMilestone) ||
                   type == YTJsonParser.GetLocalizeString(KeySet.ChatMemberGift) ||
                   type == YTJsonParser.GetLocalizeString(KeySet.ChatReceivedMemberGift))
                {
                    foreach (ListViewItem.ListViewSubItem item in lvItem.SubItems)
                    {
                        item.ForeColor = Color.White;
                        item.BackColor = Color.Green;
                    }
                }

                if (type == YTJsonParser.GetLocalizeString(KeySet.ChatRedirect) ||
                    type == YTJsonParser.GetLocalizeString(KeySet.ChatPinned))
                {
                    foreach (ListViewItem.ListViewSubItem item in lvItem.SubItems)
                    {
                        item.ForeColor = Color.White;
                        item.BackColor = ColorTranslator.FromHtml("#203d6c");
                    }
                }

                if (!string.IsNullOrEmpty(authorPhotoUrl))
                {
                    string imgKey = authorName;

                    LVLiveChatList.InvokeIfRequired(async () =>
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
                        }
                    });

                    lvItem.ImageKey = imgKey;
                }

                // 先過濾以避免加入到重複的資料。
                if (!listTempItem.Any(n => n.Text == authorName &&
                    n.SubItems[4].Text == timestampUsec))
                {
                    listTempItem.Add(lvItem);
                }

                rowIdx1++;
            }

            LVLiveChatList.InvokeIfRequired(() =>
            {
                LVLiveChatList.BeginUpdate();
                LVLiveChatList.Items.AddRange([.. listTempItem]);
                LVLiveChatList.Items[^1].EnsureVisible();
                LVLiveChatList.EndUpdate();
            });

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
                                //    YTJsonParser.FetchLargePicture());

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
                            //        YTJsonParser.FetchLargePicture());

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
                            //        YTJsonParser.FetchLargePicture());

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