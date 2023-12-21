using Color = System.Drawing.Color;
using HorizontalAlignment = System.Windows.Forms.HorizontalAlignment;
using Microsoft.Extensions.Logging;
using NLog;
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Events;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Sets;
using Rubujo.YouTube.Utility.Utils;
using Size = System.Drawing.Size;
using StringSet = YTLiveChatCatcher.Common.Sets.StringSet;
using System.Runtime.Versioning;
using YTLiveChatCatcher.Common;
using YTLiveChatCatcher.Common.Utils;
using YTLiveChatCatcher.Extensions;

namespace YTLiveChatCatcher;

// 阻擋設計工具。
partial class DesignerBlocker { };

/// <summary>
/// FMain 的方法
/// </summary>
public partial class FMain
{
    /// <summary>
    /// 初始化 ListView
    /// </summary>
    /// <param name="listview">ListView</param>
    public static void InitListView(ListView listview)
    {
        ColumnHeader[] columnHeaders =
        [
            new()
            {
                Name = "AuthorName",
                Text = "作者名稱",
                TextAlign = HorizontalAlignment.Left,
                Width = 140,
                DisplayIndex = 0
            },
            new()
            {
                Name = "AuthorBages",
                Text = "徽章",
                TextAlign = HorizontalAlignment.Left,
                Width = 100,
                DisplayIndex = 1
            },
            new()
            {
                Name = "Message",
                Text = "訊息",
                TextAlign = HorizontalAlignment.Left,
                Width = 320,
                DisplayIndex = 2
            },
            new()
            {
                Name = "PurchaseAmount",
                Text = "金額",
                TextAlign = HorizontalAlignment.Left,
                Width = 80,
                DisplayIndex = 3
            },
            new()
            {
                Name = "TimestampUsec",
                Text = "時間",
                TextAlign = HorizontalAlignment.Center,
                Width = 150,
                DisplayIndex = 4
            },
            new()
            {
                Name = "Type",
                Text = "類型",
                TextAlign = HorizontalAlignment.Center,
                Width = 100,
                DisplayIndex = 5
            },
            new()
            {
                Name = "ForegroundColor",
                Text = "前景顏色",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 6
            },
            new()
            {
                Name = "BackgroundColor",
                Text = "背景顏色",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 7
            },
            new()
            {
                Name = "TimestampText",
                Text = "時間標記文字",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 8
            },
            new()
            {
                Name = "AuthorPhotoUrl",
                Text = "頭像網址",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 9
            },
            new()
            {
                Name = "AuthorExternalChannelID",
                Text = "外部頻道 ID",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 10
            },
            new()
            {
                Name = "MessageID",
                Text = "訊息 ID 值",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 11
            }
        ];

        listview.Columns.AddRange(columnHeaders);

        ImageList imageList = new()
        {
            ImageSize = new Size(32, 32),
            ColorDepth = ColorDepth.Depth32Bit
        };

        listview.SmallImageList = imageList;
    }

    /// <summary>
    /// 使用 Text-To-Speech 說話
    /// </summary>
    /// <param name="listView">ListView</param>
    [SupportedOSPlatform("windows7.0")]
    public static void TtsSpeak(ListView listView)
    {
        if (OperatingSystem.IsWindows())
        {
            listView.InvokeIfRequired(() =>
            {
                ListView.SelectedListViewItemCollection selectedItems = listView.SelectedItems;
                ListViewItem listViewItem = selectedItems[^1];

                string type = listViewItem.SubItems[5].Text;
                string authorName = listViewItem.SubItems[0].Text;
                string message = listViewItem.SubItems[2].Text;

                string speakText = string.Empty;

                if (type == LiveChatCatcher.GetLocalizeString(KeySet.ChatGeneral) ||
                    type == LiveChatCatcher.GetLocalizeString(KeySet.ChatSuperChat) ||
                    type == LiveChatCatcher.GetLocalizeString(KeySet.ChatSuperSticker))
                {
                    speakText = $"{authorName}說{message}";
                }
                else
                {
                    if (type != Rubujo.YouTube.Utility.Sets.StringSet.YouTube)
                    {
                        speakText = $"{authorName}";
                    }
                }

                CustomFunction.SpeechText(speakText);
            });
        }
    }

    /// <summary>
    /// 執行匯出任務
    /// </summary>
    /// <param name="listView">ListView</param>
    /// <returns>Task</returns>
    public async Task DoExportTask(ListView listView)
    {
        if (listView.Items.Count <= 0)
        {
            MessageBox.Show(
              "匯出失敗，請先確認聊天室內容是否有資料。",
              Text,
              MessageBoxButtons.OK,
              MessageBoxIcon.Error);

            return;
        }

        SaveFileDialog saveFileDialog = new()
        {
            Filter = "Excel 活頁簿|*.xlsx",
            Title = "儲存檔案",
            FileName = $"{StringSet.SheetName1}_{DateTime.Now:yyyyMMdd}"
        };

        string videoID = string.Empty;

        TBVideoID.InvokeIfRequired(() =>
        {
            videoID = TBVideoID.Text.Trim();
        });

        // 取得影片的標題。
        string videoTitle = SharedLiveChatCatcher.GetVideoTitle(videoID);

        if (!string.IsNullOrEmpty(videoTitle))
        {
            string optFileName = $"{videoTitle}_{saveFileDialog.FileName}";
            string cleanedFileName = CustomFunction.RemoveInvalidFilePathCharacters(optFileName, "_");

            saveFileDialog.FileName = cleanedFileName;
        }

        DialogResult dialogResult = saveFileDialog.ShowDialog();

        if (dialogResult == DialogResult.OK)
        {
            if (!string.IsNullOrEmpty(saveFileDialog.FileName))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        CreateXLSX(
                            control1: saveFileDialog,
                            control2: listView,
                            videoID: videoID);
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"發生錯誤：{ex.GetExceptionMessage()}");
                    }
                }).ContinueWith(task =>
                {
                    TerminateLongTask(listView, isImport: false);
                });
            }
            else
            {
                MessageBox.Show(
                    "請選擇有效的檔案名稱。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }

    /// <summary>
    /// 複製至剪貼簿
    /// </summary>
    /// <param name="listView">ListView</param>
    public void CopyToClipboard(ListView listView)
    {
        listView.InvokeIfRequired(() =>
        {
            ListView.SelectedListViewItemCollection selectedItems = listView.SelectedItems;

            string copiedContent = string.Empty;

            foreach (ListViewItem listViewItem in selectedItems)
            {
                string tempContent = string.Empty;

                int count = 0;

                foreach (ListViewItem.ListViewSubItem listViewSubItem in listViewItem.SubItems)
                {
                    string currentContent = listViewSubItem.Text;

                    tempContent += currentContent;

                    if (count != listViewItem.SubItems.Count - 1)
                    {
                        if (!string.IsNullOrEmpty(currentContent))
                        {
                            tempContent += StringSet.Splitter;
                        }
                    }

                    count++;
                }

                copiedContent += $"{tempContent}{Environment.NewLine}";
            }

            Clipboard.SetText(copiedContent);

            WriteLog("已將選擇的內容複製至剪貼簿。");
        });
    }

    /// <summary>
    /// 開啟 YouTube 頻道網址
    /// </summary>
    /// <param name="listView">ListView</param>
    /// <param name="e">MouseEventArgs</param>
    public void OpenYTChannelUrl(ListView listView, MouseEventArgs e)
    {
        listView.InvokeIfRequired(() =>
        {
            ListViewItem? focusedItem = listView.FocusedItem;

            if (focusedItem != null && focusedItem.Bounds.Contains(e.Location))
            {
                if (focusedItem.SubItems.Count >= 10)
                {
                    string authorExternalChannelId = focusedItem.SubItems[9].Text;

                    if (!string.IsNullOrEmpty(authorExternalChannelId))
                    {
                        string channelUrl = LiveChatCatcher
                            .GetYouTubeChannelUrl(authorExternalChannelId);

                        CustomFunction.OpenBrowser(channelUrl);
                    }
                    else
                    {
                        WriteLog("找不到頻道 ID，無法開啟頻道網址。");
                    }
                }
                else
                {
                    WriteLog("找不到頻道 ID，無法開啟頻道網址。");
                }
            }
        });
    }

    /// <summary>
    /// 初始化使用者控制項
    /// </summary>
    private void InitControls()
    {
        this.InvokeIfRequired(() =>
        {
            Text = StringSet.AppName;
            Icon = Properties.Resources.app_icon;
            ActiveControl = TBVideoID;
        });

        bool enableDebug = Properties.Settings.Default.EnableDebug;

        if (enableDebug)
        {
            LogManager.ResumeLogging();
        }
        else
        {
            LogManager.SuspendLogging();
        }

        // 2023/12/21 暫時先取消預設值。
        //TBInterval.InvokeIfRequired(() =>
        //{
        //    // 預設 3 秒。
        //    TBInterval.Text = "3";
        //});

        BtnStop.InvokeIfRequired(() =>
        {
            // 預設禁用停止按鈕。
            BtnStop.Enabled = false;
        });

        CBExportAuthorPhoto.InvokeIfRequired(() =>
        {
            // 載入啟用匯出頭像設定值。
            CBExportAuthorPhoto.Checked = Properties.Settings.Default.ExportAuthorPhoto;
        });

        CBEnableTTS.InvokeIfRequired(() =>
        {
            // 載入啟用文字轉語音設定值。
            CBEnableTTS.Checked = Properties.Settings.Default.EnableTTS;
        });

        CBLoadCookie.InvokeIfRequired(() =>
        {
            // 載入載入 Cookies 設定值。
            CBLoadCookie.Checked = Properties.Settings.Default.LoadCookie;
        });

        CBBrowser.InvokeIfRequired(() =>
        {
            // 載入網頁瀏覽器選項的索引值。
            CBBrowser.SelectedIndex = Properties.Settings.Default.BrowserItemIndex;
        });

        TBProfileFolderName.InvokeIfRequired(() =>
        {
            // 載入設定檔資料夾名稱的設定值。
            TBProfileFolderName.Text = Properties.Settings.Default.ProfileFolderName;
        });

        TBUserAgent.InvokeIfRequired(() =>
        {
            // 載入使用者代理字串。
            TBUserAgent.Text = Properties.Settings.Default.UserAgent;
        });

        TBSecChUa.InvokeIfRequired(() =>
        {
            // 載入 Sec-CH-UA。
            TBSecChUa.Text = Properties.Settings.Default.SecChUa;
        });

        LVersion.InvokeIfRequired(() =>
        {
            string version = CustomFunction.GetAppVersion();

            string verText = !string.IsNullOrEmpty(version) ? version : "無";

            // 設定版本號顯示。
            LVersion.Text = $"版本號：{verText}";
        });

        CBEnableDebug.InvokeIfRequired(() =>
        {
            // 載入啟用輸出錯誤資訊的設定值。
            CBEnableDebug.Checked = enableDebug;
        });

        // 設定提示。
        SharedTooltip.SetToolTip(TBInterval, "僅在未勾選「隨機間隔秒數」以及為「重播」時會生效。");

        // 設定控制項的狀態。
        SetControlsState(true);
    }

    /// <summary>
    /// 執行處裡訊息
    /// </summary>
    /// <param name="messages">List&lt;RendererData&gt;</param>
    private async void DoProcessMessages(List<RendererData> messages)
    {
        try
        {
            List<ListViewItem> listTempItem = [];

            foreach (RendererData rendererData in messages)
            {
                if (rendererData.Stickers != null)
                {
                    foreach (StickerData stickerData in rendererData.Stickers)
                    {
                        if (!SharedStickers.Any(n => n.ID == stickerData.ID))
                        {
                            string errorMessage = await stickerData.SetImage(
                                SharedHttpClient,
                                LiveChatCatcher.FetchLargePicture());

                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                WriteLog(errorMessage);
                            };

                            SharedStickers.Add(stickerData);
                        }
                    }
                }

                if (rendererData.Emojis != null)
                {
                    foreach (EmojiData emojiData in rendererData.Emojis)
                    {
                        // 只處理自定義表情符號的資料。
                        if (!SharedCustomEmojis.Any(n => n.ID == emojiData.ID))
                        {
                            if (emojiData.IsCustomEmoji)
                            {
                                string errorMessage = await emojiData.SetImage(
                                    SharedHttpClient,
                                    LiveChatCatcher.FetchLargePicture());

                                if (!string.IsNullOrEmpty(errorMessage))
                                {
                                    WriteLog(errorMessage);
                                };

                                SharedCustomEmojis.Add(emojiData);
                            }
                        }
                    }
                }

                if (rendererData.Badges != null)
                {
                    foreach (BadgeData badgeData in rendererData.Badges)
                    {
                        // 只處理會員徽章的資料。
                        if (badgeData.Label != null &&
                            !SharedBadges.Any(n => n.Label == badgeData.Label) &&
                            badgeData.Label.Contains(StringSet.Member))
                        {
                            string errorMessage = await badgeData.SetImage(
                                SharedHttpClient,
                                LiveChatCatcher.FetchLargePicture());

                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                WriteLog(errorMessage);
                            };

                            SharedBadges.Add(badgeData);
                        }
                    }
                }

                string id = rendererData.ID ?? string.Empty;
                string authorName = (rendererData.AuthorName != null &&
                    rendererData.AuthorName != KeySet.NoAuthorName) ?
                    rendererData.AuthorName :
                    string.Empty;
                string authorBages = (rendererData.AuthorBadges != null &&
                    rendererData.AuthorBadges != KeySet.NoAuthorBadges) ?
                    rendererData.AuthorBadges :
                    string.Empty;
                string authorPhotoUrl = (rendererData.AuthorPhotoUrl != null &&
                    rendererData.AuthorPhotoUrl != KeySet.NoAuthorPhotoUrl) ?
                    rendererData.AuthorPhotoUrl :
                    string.Empty;
                string messageContent = (rendererData.MessageContent != null &&
                    rendererData.MessageContent != KeySet.NoMessageContent) ?
                    rendererData.MessageContent :
                    string.Empty;
                string purchaseAmountText = (rendererData.PurchaseAmountText != null &&
                    rendererData.PurchaseAmountText != KeySet.NoPurchaseAmountText) ?
                    rendererData.PurchaseAmountText :
                    string.Empty;
                string timestampUsec = rendererData.TimestampUsec ?? string.Empty;
                string type = rendererData.Type ?? string.Empty;
                string foregroundColor = (rendererData.ForegroundColor != null &&
                    rendererData.ForegroundColor != KeySet.NoForegroundColor) ?
                    rendererData.ForegroundColor :
                    string.Empty;
                string backgroundColor = (rendererData.BackgroundColor != null &&
                    rendererData.BackgroundColor != KeySet.NoBackgroundColor) ?
                    rendererData.BackgroundColor :
                    string.Empty;
                // 直播不會有，只有重播才會有。
                string timestampText = (rendererData.TimestampText != null &&
                    rendererData.TimestampText != KeySet.NoTimestampText) ?
                    rendererData.TimestampText :
                    string.Empty;
                string authorExternalChannelID = (rendererData.AuthorExternalChannelID != null &&
                    rendererData.AuthorExternalChannelID != KeySet.NoAuthorExternalChannelID) ?
                    rendererData.AuthorExternalChannelID :
                    string.Empty;

                if (string.IsNullOrEmpty(timestampText))
                {
                    // 改為使用發送訊息的時間。
                    if (DateTime.TryParse(timestampUsec, out DateTime dateTime))
                    {
                        timestampText = dateTime.ToString("HH:mm:ss");
                    }
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
                    purchaseAmountText,
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

                if (type == LiveChatCatcher.GetLocalizeString(KeySet.ChatJoinMember) ||
                    type == LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberUpgrade) ||
                    type == LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberMilestone) ||
                    type == LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberGift) ||
                    type == LiveChatCatcher.GetLocalizeString(KeySet.ChatReceivedMemberGift))
                {
                    foreach (ListViewItem.ListViewSubItem item in lvItem.SubItems)
                    {
                        item.ForeColor = Color.White;
                        item.BackColor = Color.Green;
                    }
                }

                if (type == LiveChatCatcher.GetLocalizeString(KeySet.ChatRedirect) ||
                    type == LiveChatCatcher.GetLocalizeString(KeySet.ChatPinned))
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
                if (!listTempItem.Any(n => n.Text == authorName && n.SubItems[4].Text == timestampUsec))
                {
                    listTempItem.Add(lvItem);
                }
            }

            LVLiveChatList.InvokeIfRequired(() =>
            {
                LVLiveChatList.BeginUpdate();
                LVLiveChatList.Items.AddRange(listTempItem.ToArray());

                if (LVLiveChatList.Items.Count > 0)
                {
                    LVLiveChatList.Items[^1].EnsureVisible();
                }

                LVLiveChatList.EndUpdate();
            });

            UpdateSummaryInfo();
        }
        catch (Exception ex)
        {
            WriteLog($"發生錯誤：{ex.GetExceptionMessage()}");
        }
    }

    /// <summary>
    /// 執行匯入資料
    /// </summary>
    /// <param name="filePath">字串，欲匯入檔案的路徑</param>
    private void DoImportData(string filePath)
    {
        Task.Run(async () =>
        {
            try
            {
                await LoadXLSX(filePath: filePath);
            }
            catch (Exception ex)
            {
                WriteLog($"發生錯誤：{ex.GetExceptionMessage()}");
            }
        }).ContinueWith(task =>
        {
            TerminateLongTask(listView: LVLiveChatList, isImport: true);
        });
    }

    /// <summary>
    /// 執行匯入／匯出任務
    /// </summary>
    private void RunLongTask()
    {
        CBExportAuthorPhoto.InvokeIfRequired(() =>
        {
            CBExportAuthorPhoto.Enabled = false;
        });

        BtnStart.InvokeIfRequired(() =>
        {
            BtnStart.Enabled = false;
        });

        BtnExport.InvokeIfRequired(() =>
        {
            BtnExport.Enabled = false;
        });

        BtnClear.InvokeIfRequired(() =>
        {
            BtnClear.Enabled = false;
        });

        TBUserAgent.InvokeIfRequired(() =>
        {
            TBUserAgent.Enabled = false;
        });

        TBSecChUa.InvokeIfRequired(() =>
        {
            TBSecChUa.Enabled = false;
        });

        BtnImport.InvokeIfRequired(() =>
        {
            BtnImport.Enabled = false;
        });

        PBProgress.InvokeIfRequired(() =>
        {
            PBProgress.Style = ProgressBarStyle.Marquee;
        });
    }

    /// <summary>
    /// 終止匯入／匯出任務
    /// </summary>
    /// <param name="listView">ListView，預設值為 null</param>
    /// <param name="isImport">布林值，判斷是否為匯入，預設值為 false</param>
    private void TerminateLongTask(ListView? listView = null, bool isImport = false)
    {
        // 判斷 listView 是不是 LVLiveChatList。
        if (listView?.Name == LVLiveChatList.Name)
        {
            CBExportAuthorPhoto.InvokeIfRequired(() =>
            {
                CBExportAuthorPhoto.Enabled = true;
            });

            BtnStart.InvokeIfRequired(() =>
            {
                BtnStart.Enabled = true;
            });

            BtnExport.InvokeIfRequired(() =>
            {
                BtnExport.Enabled = true;
            });

            BtnClear.InvokeIfRequired(() =>
            {
                BtnClear.Enabled = true;
            });

            TBUserAgent.InvokeIfRequired(() =>
            {
                TBUserAgent.Enabled = true;
            });

            TBSecChUa.InvokeIfRequired(() =>
            {
                TBSecChUa.Enabled = true;
            });

            BtnImport.InvokeIfRequired(() =>
            {
                BtnImport.Enabled = true;
            });

            PBProgress.InvokeIfRequired(() =>
            {
                PBProgress.Style = ProgressBarStyle.Blocks;
            });
        }

        string taskWord = isImport ? "匯入" : "匯出";

        WriteLog($"*.xlsx {taskWord}作業完成。");
    }

    /// <summary>
    /// 更新統計資訊
    /// </summary>
    private void UpdateSummaryInfo()
    {
        IEnumerable<ListViewItem> dataSet = LVLiveChatList.Items.Cast<ListViewItem>();

        TBLog.InvokeIfRequired(() =>
        {
            IEnumerable<ListViewItem> tempDataSet = dataSet.Where(n =>
                (n.SubItems[5].Text == LiveChatCatcher.GetLocalizeString(KeySet.ChatSuperChat) ||
                n.SubItems[5].Text == LiveChatCatcher.GetLocalizeString(KeySet.ChatSuperSticker)) &&
                n.SubItems[3].Text.StartsWith('$'));

            double totalIncome = 0.0;

            foreach (ListViewItem lvItem in tempDataSet)
            {
                // 只會取新臺幣的資料。
                string currentItemIncomeValue = lvItem.SubItems[3].Text.Replace("$", string.Empty);

                totalIncome += double.TryParse(currentItemIncomeValue, out double currentItemIncomeActualValue) ?
                    currentItemIncomeActualValue : 0;
            }

            // YouTube 會抽取 30% 收益。
            double actualIncome = Math.Round(totalIncome * 0.70, 0, MidpointRounding.AwayFromZero);

            LTempIncome.InvokeIfRequired(() =>
            {
                LTempIncome.Text = $"預計收益：共 {actualIncome} 元整";

                SharedTooltip.SetToolTip(LTempIncome, $"累積收益：共 {totalIncome} 元整");
            });

            string message = $"目前累積收益：共 {totalIncome} 元整" +
                $"（實收：{actualIncome} 元整）{Environment.NewLine}" +
                "※僅計算超級留言／貼圖的新臺幣收益，其他幣種一律不納入計算。";

            WriteLog(message);
        });

        LChatCount.InvokeIfRequired(() =>
        {
            int count = dataSet.Where(n => n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.YouTube &&
                n.SubItems[5].Text != LiveChatCatcher.GetLocalizeString(KeySet.ChatJoinMember) &&
                n.SubItems[5].Text != LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberUpgrade) &&
                n.SubItems[5].Text != LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberMilestone) &&
                n.SubItems[5].Text != LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberGift) &&
                n.SubItems[5].Text != LiveChatCatcher.GetLocalizeString(KeySet.ChatReceivedMemberGift) &&
                n.SubItems[5].Text != LiveChatCatcher.GetLocalizeString(KeySet.ChatRedirect) &&
                n.SubItems[5].Text != LiveChatCatcher.GetLocalizeString(KeySet.ChatPinned))
                .Count();

            LChatCount.Text = $"留言數量：{count} 個";
        });

        LSuperChatCount.InvokeIfRequired(() =>
        {
            int count = dataSet.Where(n => n.SubItems[5].Text == LiveChatCatcher.GetLocalizeString(KeySet.ChatSuperChat)).Count();

            LSuperChatCount.Text = $"{LiveChatCatcher.GetLocalizeString(KeySet.ChatSuperChat)}：{count} 個";
        });

        LSuperStickerCount.InvokeIfRequired(() =>
        {
            int count = dataSet.Where(n => n.SubItems[5].Text == LiveChatCatcher.GetLocalizeString(KeySet.ChatSuperSticker)).Count();

            LSuperStickerCount.Text = $"{LiveChatCatcher.GetLocalizeString(KeySet.ChatSuperSticker)}：{count} 個";
        });

        LMemberJoinCount.InvokeIfRequired(() =>
        {
            int joinCount = dataSet.Where(n => n.SubItems[5].Text == LiveChatCatcher.GetLocalizeString(KeySet.ChatJoinMember)).Count();
            int upgradeCount = dataSet.Where(n => n.SubItems[5].Text == LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberUpgrade)).Count();
            int milestoneCount = dataSet.Where(n => n.SubItems[5].Text == LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberMilestone)).Count();
            int giftCount = dataSet.Where(n => n.SubItems[5].Text == LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberGift)).Count();
            int receivedGiftCount = dataSet.Where(n => n.SubItems[5].Text == LiveChatCatcher.GetLocalizeString(KeySet.ChatReceivedMemberGift)).Count();

            LMemberJoinCount.Text = $"{LiveChatCatcher.GetLocalizeString(KeySet.ChatJoinMember)}：{joinCount} 位";

            string tooltip = $"{LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberUpgrade)}：{upgradeCount} 位、" +
                $"{LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberMilestone)}：{milestoneCount} 位、" +
                $"{LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberGift)}：{giftCount} 位、" +
                $"{LiveChatCatcher.GetLocalizeString(KeySet.ChatReceivedMemberGift)}：{receivedGiftCount} 位";

            SharedTooltip.SetToolTip(LMemberJoinCount, tooltip);
        });

        LMemberInRoomCount.InvokeIfRequired(() =>
        {
            int count = dataSet.Where(n => n.SubItems[5].Text != LiveChatCatcher.GetLocalizeString(KeySet.ChatJoinMember) &&
                n.SubItems[5].Text != LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberUpgrade) &&
                n.SubItems[5].Text != LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberMilestone) &&
                n.SubItems[1].Text.Contains(StringSet.Member))
                .Select(n => n.SubItems[0].Text)
                .Distinct()
                .Count();

            LMemberInRoomCount.Text = $"會員人數：{count} 位";
        });

        LAuthorCount.InvokeIfRequired(() =>
        {
            int count = dataSet.Where(n => n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.YouTube &&
                !n.SubItems[5].Text.Contains(StringSet.Member))
                .Select(n => n.SubItems[0].Text)
                .Distinct()
                .Count();

            LAuthorCount.Text = $"留言人數：{count} 位";
        });
    }

    /// <summary>
    /// 設定控制項的狀態。
    /// </summary>
    /// <param name="enable">布林值，預設值為 true</param>
    private void SetControlsState(bool enable = true)
    {
        // 2022/5/30 暫時先不要鎖。
        /*
        BtnOpenVideoUrl.InvokeIfRequired(() =>
        {
            BtnOpenVideoUrl.Enabled = !enable;
        });
        */

        BtnStart.InvokeIfRequired(() =>
        {
            BtnStart.Enabled = enable;
        });

        BtnStop.InvokeIfRequired(() =>
        {
            BtnStop.Enabled = !enable;
        });

        BtnExport.InvokeIfRequired(() =>
        {
            BtnExport.Enabled = enable;
        });

        BtnClear.InvokeIfRequired(() =>
        {
            BtnClear.Enabled = enable;
        });

        TBChannelID.InvokeIfRequired(() =>
        {
            TBChannelID.Enabled = enable;
        });

        TBVideoID.InvokeIfRequired(() =>
        {
            TBVideoID.Enabled = enable;
        });

        TBUserAgent.InvokeIfRequired(() =>
        {
            TBUserAgent.Enabled = enable;
        });

        // 2023/12/21 暫時取消控制。
        /*
        TBInterval.InvokeIfRequired(() =>
        {
            TBInterval.Enabled = enable;
        });
        */

        CBLoadCookie.InvokeIfRequired(() =>
        {
            CBLoadCookie.Enabled = enable;
        });

        CBBrowser.InvokeIfRequired(() =>
        {
            CBBrowser.Enabled = enable;
        });

        TBProfileFolderName.InvokeIfRequired(() =>
        {
            TBProfileFolderName.Enabled = enable;
        });

        TBSecChUa.InvokeIfRequired(() =>
        {
            TBSecChUa.Enabled = enable;
        });

        BtnImport.InvokeIfRequired(() =>
        {
            BtnImport.Enabled = enable;
        });
    }

    /// <summary>
    /// 設定使用 Cookie
    /// </summary>
    private void SetUseCookie()
    {
        bool enable = false;

        CBLoadCookie.InvokeIfRequired(() =>
        {
            enable = CBLoadCookie.Checked;
        });

        WebBrowserUtil.BrowserType browserType =
            WebBrowserUtil.BrowserType.GoogleChrome;

        CBBrowser.InvokeIfRequired(() =>
        {
            browserType = CBBrowser.SelectedItem?.ToString() switch
            {
                "Brave" => WebBrowserUtil.BrowserType.Brave,
                "Brave Beta" => WebBrowserUtil.BrowserType.BraveBeta,
                "Brave Nightly" => WebBrowserUtil.BrowserType.BraveNightly,
                "Google Chrome" => WebBrowserUtil.BrowserType.GoogleChrome,
                "Google Chrome Beta" => WebBrowserUtil.BrowserType.GoogleChromeBeta,
                "Google Chrome Canary" => WebBrowserUtil.BrowserType.GoogleChromeCanary,
                "Chromium" => WebBrowserUtil.BrowserType.Chromium,
                "Microsoft Edge" => WebBrowserUtil.BrowserType.MicrosoftEdge,
                "Microsoft Edge Insider Beta" => WebBrowserUtil.BrowserType.MicrosoftEdgeInsiderBeta,
                "Microsoft Edge Insider Dev" => WebBrowserUtil.BrowserType.MicrosoftEdgeInsiderDev,
                "Microsoft Edge Insider Canary" => WebBrowserUtil.BrowserType.MicrosoftEdgeInsiderCanary,
                "Opera" => WebBrowserUtil.BrowserType.Opera,
                "Opera Beta" => WebBrowserUtil.BrowserType.OperaBeta,
                "Opera Developer" => WebBrowserUtil.BrowserType.OperaDeveloper,
                "Opera GX" => WebBrowserUtil.BrowserType.OperaGX,
                "Opera Crypto" => WebBrowserUtil.BrowserType.OperaCrypto,
                "Vivaldi" => WebBrowserUtil.BrowserType.Vivaldi,
                "Mozilla Firefox" => WebBrowserUtil.BrowserType.MozillaFirefox,
                _ => WebBrowserUtil.BrowserType.GoogleChrome
            };
        });

        string profileFolderName = string.Empty;

        TBProfileFolderName.InvokeIfRequired(() =>
        {
            profileFolderName = TBProfileFolderName.Text;
        });

        // 設定 LiveChatCatcher 是否使用 Cookies。
        SharedLiveChatCatcher.UseCookie(
            enable: enable,
            browserType: browserType,
            profileFolderName: profileFolderName);
    }

    /// <summary>
    /// 寫紀錄
    /// </summary>
    /// <param name="message">字串，訊息內容</param>
    private async void WriteLog(string message)
    {
        await Task.Run(() =>
        {
            TBLog.InvokeIfRequired(() =>
            {
                TBLog.AppendText($"[{DateTime.Now}]：{message}{Environment.NewLine}");
            });
        });
    }

    /// <summary>
    /// 初始化 HttpCleint
    /// </summary>
    private void InitHttpCleint()
    {
        string userAgent = string.Empty;

        TBUserAgent.InvokeIfRequired(() =>
        {
            userAgent = TBUserAgent.Text;
        });

        // 取得 HttpClient。
        SharedHttpClient = HttpClientUtil.GetHttpClient(
            SharedHttpClientFactory,
            userAgent);
    }

    /// <summary>
    /// 初始化 LiveChatCather
    /// </summary>
    private void InitLiveChatCather(HttpClient? httpClient)
    {
        if (httpClient == null)
        {
            WriteLog("[InitLiveChatCather()] 變數 \"httpClient\" 為 null！");

            return;
        }

        SharedLiveChatCatcher.Init(httpClient: httpClient);

        LiveChatCatcher.FetchLargePicture(true);
        LiveChatCatcher.DisplayLanguage(EnumSet.DisplayLanguage.Chinese_Traditional);

        SharedLiveChatCatcher.OnFecthLiveChat += (object? sender, FecthLiveChatArgs e) =>
        {
            TBUserAgent.InvokeIfRequired(() =>
            {
                DoProcessMessages(e.Data);
            });
        };
        SharedLiveChatCatcher.OnRunningStatusUpdate += (object? sender, RunningStatusArgs e) =>
        {
            EnumSet.RunningStatus runningStatus = e.RunningStatus;

            switch (runningStatus)
            {
                default:
                case EnumSet.RunningStatus.Running:
                    break;
                case EnumSet.RunningStatus.Stopped:
                    BtnStop_Click(this, new EventArgs());

                    break;
                case EnumSet.RunningStatus.ErrorOccured:
                    BtnStop_Click(this, new EventArgs());

                    break;
            }
        };
        SharedLiveChatCatcher.OnLogOutput += (object? sender, LogOutputArgs e) =>
        {
            EnumSet.LogType logType = e.LogType;

            switch (logType)
            {
                case EnumSet.LogType.Info:
                    SharedLogger.LogInformation("{Message}", e.Message);

                    WriteLog(e.Message);

                    // 更新間隔欄位的值。
                    if (e.Message.Contains("接收到的間隔毫秒值："))
                    {
                        if (int.TryParse(
                            e.Message.Replace("接收到的間隔毫秒值：", string.Empty),
                            out int newInterval))
                        {
                            newInterval /= 1000;

                            TBInterval.InvokeIfRequired(() =>
                            {
                                if (newInterval.ToString() != TBInterval.Text)
                                {
                                    TBInterval.Text = newInterval.ToString();
                                }
                            });
                        }
                    }

                    break;
                case EnumSet.LogType.Warn:
                    SharedLogger.LogWarning("{WarningMessage}", e.Message);

                    WriteLog(e.Message);

                    break;
                case EnumSet.LogType.Error:
                    SharedLogger.LogError("{ErrorMessage}", e.Message);

                    WriteLog(e.Message);

                    break;
                case EnumSet.LogType.Debug:
                    SharedLogger.LogDebug("{DebugMessage}", e.Message);

                    break;
                default:
                    break;
            }
        };

        CBLoadCookie.InvokeIfRequired(() =>
        {
            if (CBLoadCookie.Checked)
            {
                // 設定使用 Cookie
                SetUseCookie();
            }
        });
    }

    /// <summary>
    /// 檢查應用程式的版本
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    private async void CheckAppVersion(HttpClient? httpClient)
    {
        if (httpClient == null)
        {
            WriteLog("[CheckAppVersion()] 變數 \"httpClient\" 為 null！");

            return;
        }

        UpdateNotifier.CheckResult checkResult = await UpdateNotifier.CheckVersion(httpClient);

        if (!string.IsNullOrEmpty(checkResult.MessageText))
        {
            WriteLog(checkResult.MessageText);
        }

        if (checkResult.HasNewVersion &&
            !string.IsNullOrEmpty(checkResult.DownloadUrl))
        {
            DialogResult dialogResult = MessageBox.Show($"您是否要下載新版本 v{checkResult.VersionText}？",
                Text,
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question);

            if (dialogResult == DialogResult.OK)
            {
                CustomFunction.OpenBrowser(checkResult.DownloadUrl);
            }
        }

        if (checkResult.NetVersionIsOdler &&
            !string.IsNullOrEmpty(checkResult.DownloadUrl))
        {
            DialogResult dialogResult = MessageBox.Show($"您是否要下載舊版本 v{checkResult.VersionText}？",
                Text,
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question);

            if (dialogResult == DialogResult.OK)
            {
                CustomFunction.OpenBrowser(checkResult.DownloadUrl);
            }
        }
    }
}