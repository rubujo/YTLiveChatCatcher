using Microsoft.Extensions.Logging;
using NLog;
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Models.LiveChat;
using Rubujo.YouTube.Utility.Utils;
using YTLiveChatCatcher.Common;
using YTLiveChatCatcher.Common.Sets;
using YTLiveChatCatcher.Common.Utils;
using YTLiveChatCatcher.Extensions;

namespace YTLiveChatCatcher;

/// <summary>
/// FMain
/// </summary>
public partial class FMain : Form
{
    public FMain(
        IHttpClientFactory httpClientFactory,
        ILogger<FMain> logger,
        ILogger<YTJsonParser> ytJsonParserLogger)
    {
        InitializeComponent();

        SharedHttpClientFactory = httpClientFactory;
        SharedLogger = logger;
        SharedYTJsonParserLogger = ytJsonParserLogger;
    }

    private void FMain_Load(object sender, EventArgs e)
    {
        try
        {
            InitHttpCleint();
            InitControls();

            #region 更新 SharedHttpClient 的標頭資訊

            string userAgent = string.Empty;

            TBUserAgent.InvokeIfRequired(() =>
            {
                userAgent = TBUserAgent.Text;
            });

            // 更新 SharedHttpClient 的標頭資訊。
            HttpClientUtil.UpdateHttpClient(SharedHttpClient, userAgent);

            #endregion;

            InitListView(LVLiveChatList);
            InitLiveChatCather(SharedHttpClient);

            CheckAppVersion(SharedHttpClient);
        }
        catch (Exception ex)
        {
            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void FMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        try
        {
            LogManager.Shutdown();

            // 取消尚在執行中的擷取工作，並釋放 SharedYTJsonParser。
            SharedFetchCancellationTokenSource?.Cancel();
            SharedYTJsonParser?.Dispose();

            // 釋放以及清除 SharedHttpClient。
            SharedHttpClient?.Dispose();
            SharedHttpClient = null;
        }
        catch (Exception ex)
        {
            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void TBChannelID_TextChanged(object sender, EventArgs e)
    {
        // 此事件必定於 UI 執行緒觸發，不需要透過 InvokeIfRequired 轉送
        // （InvokeIfRequired 吃的是 void 委派，若傳入 async lambda 會變成 async void，
        // 第一個 await 之後拋出的例外無法被這裡的 try/catch 攔截）。
        TextBox? textBox = (TextBox?)sender;

        if (textBox == null)
        {
            return;
        }

        try
        {
            textBox.Text = await YouTubeUrlUtil.GetYouTubeChannelID(textBox.Text.Trim());
        }
        catch (Exception ex)
        {
            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());
        }
    }

    private void TBVideoID_TextChanged(object sender, EventArgs e)
    {
        TextBox? textBox = (TextBox?)sender;

        if (textBox == null)
        {
            return;
        }

        textBox.InvokeIfRequired(() =>
        {
            textBox.Text = YouTubeUrlUtil.GetYouTubeVideoID(textBox.Text.Trim());
        });
    }

    private void BtnOpenVideoUrl_Click(object sender, EventArgs e)
    {
        try
        {
            string videoID = string.Empty;

            TBVideoID.InvokeIfRequired(() =>
            {
                videoID = TBVideoID.Text;
            });

            if (string.IsNullOrEmpty(videoID))
            {
                MessageBox.Show(
                    "無影片 ID，無法開啟網址。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            CustomFunction.OpenBrowser($"https://www.youtube.com/watch?v={videoID}");
        }
        catch (Exception ex)
        {
            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void TBInterval_KeyPress(object sender, KeyPressEventArgs e)
    {
        // 參考：https://nevinyrral.pixnet.net/blog/post/27551930
        if ((e.KeyChar < 48 | e.KeyChar > 57) & e.KeyChar != 8)
        {
            e.Handled = true;
        }
    }

    private async void BtnStart_Click(object sender, EventArgs e)
    {
        // 此事件必定於 UI 執行緒觸發，不需要透過 InvokeIfRequired 轉送
        // （InvokeIfRequired 吃的是 void 委派，若傳入 async lambda 會變成 async void，
        // 第一個 await 之後拋出的例外無法被下方的 try/catch 攔截）。

        // 立即停用開始按鈕：避免在下方 GetLatestStreamingVideoIDAsync（僅在未填影片 ID 時才 await）
        // 完成前，使用者快速連點造成本方法被重入，進而讓兩個背景輪詢迴圈同時搶用
        // SharedFetchCancellationTokenSource。
        BtnStart.Enabled = false;

        try
        {
            string videoID = TBVideoID.Text;

            if (string.IsNullOrEmpty(videoID))
            {
                videoID = await SharedYTJsonParser.GetLatestStreamingVideoIDAsync(TBChannelID.Text.Trim());

                if (!string.IsNullOrEmpty(videoID))
                {
                    WriteLog($"透過頻道 ID 取得的影片 ID：{videoID}");
                }
                else
                {
                    WriteLog("透過頻道 ID 取得影片 ID 失敗。");
                }

                TBVideoID.Text = videoID;
            }

            if (string.IsNullOrEmpty(videoID))
            {
                MessageBox.Show(
                    "請輸入頻道 ID 或是影片 ID。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                BtnStop_Click(null, new EventArgs());

                return;
            }

            // 設定控制項的狀態。
            SetControlsState(false);

            StartFetchLiveChatData(videoID);

            WriteLog("開始取得聊天室的內容。");
        }
        catch (Exception ex)
        {
            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());

            BtnStop_Click(null, new EventArgs());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 以背景 Task 搭配 IAsyncEnumerable 持續串流獲取即時聊天資料，直到被取消或發生例外為止
    /// </summary>
    /// <param name="videoID">字串，YouTube 影片的 ID 值</param>
    private void StartFetchLiveChatData(string videoID)
    {
        SharedFetchCancellationTokenSource?.Cancel();

        // 局部保留這次專屬的 CancellationTokenSource 參照，
        // 讓下方背景工作結束時只 Dispose 自己這一份，
        // 不會誤 Dispose 掉之後某次重新開始擷取所建立的新實例。
        CancellationTokenSource fetchCancellationTokenSource = new();

        SharedFetchCancellationTokenSource = fetchCancellationTokenSource;

        CancellationToken cancellationToken = fetchCancellationTokenSource.Token;

        Progress<int> intervalProgress = new(intervalMs =>
        {
            TBInterval.InvokeIfRequired(() =>
            {
                string seconds = (intervalMs / 1000).ToString();

                if (seconds != TBInterval.Text)
                {
                    TBInterval.Text = seconds;
                }
            });
        });

        _ = Task.Run(async () =>
        {
            try
            {
                await foreach (IReadOnlyList<RendererData> batch in SharedYTJsonParser.StreamLiveChatDataAsync(
                    videoID,
                    intervalProgress: intervalProgress,
                    cancellationToken: cancellationToken))
                {
                    TBUserAgent.InvokeIfRequired(() => DoProcessMessages(batch));
                }
            }
            catch (OperationCanceledException)
            {
                // 使用者主動停止，不視為錯誤。
            }
            catch (Exception ex)
            {
                SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());

                TBUserAgent.InvokeIfRequired(() => WriteLog(ex.GetExceptionMessage()));
            }
            finally
            {
                TBUserAgent.InvokeIfRequired(() => BtnStop_Click(this, new EventArgs()));

                fetchCancellationTokenSource.Dispose();
            }
        });
    }

    private void BtnStop_Click(object? sender, EventArgs e)
    {
        try
        {
            SharedFetchCancellationTokenSource?.Cancel();

            // 設定控制項的狀態。
            SetControlsState(true);

            TBInterval.InvokeIfRequired(() =>
            {
                // 清除間隔欄位的內容。
                TBInterval.Text = string.Empty;
            });

            WriteLog("已停止取得聊天室的內容。");
        }
        catch (Exception ex)
        {
            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void BtnExport_Click(object sender, EventArgs e)
    {
        try
        {
            if (LVLiveChatList.Items.Count <= 0)
            {
                MessageBox.Show(
                  "匯出失敗，請先確認聊天室內容是否有資料。",
                  Text,
                  MessageBoxButtons.OK,
                  MessageBoxIcon.Error);

                return;
            }

            if (CBExportAuthorPhoto.Checked)
            {
                DialogResult dialogResult1 = MessageBox.Show(
                    "注意，啟用匯出頭像會花費大量的時間，如您欲繼續作業請按「確定」按鈕。",
                    Text,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (dialogResult1 != DialogResult.OK)
                {
                    return;
                }
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
            string videoTitle = await SharedYTJsonParser.GetVideoTitleAsync(videoID);

            if (!string.IsNullOrEmpty(videoTitle))
            {
                string optFileName = $"{videoTitle}_{saveFileDialog.FileName}";
                string cleanedFileName = CustomFunction.RemoveInvalidFilePathCharacters(optFileName, "_");

                saveFileDialog.FileName = cleanedFileName;
            }

            DialogResult dialogResult2 = saveFileDialog.ShowDialog();

            if (dialogResult2 != DialogResult.OK)
            {
                return;
            }

            if (string.IsNullOrEmpty(saveFileDialog.FileName))
            {
                MessageBox.Show(
                    "請選擇有效的檔案名稱。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            List<ListViewItem> listAllData = [.. LVLiveChatList.GetListViewItems()];

            RunLongTask();

            await DoExportTask(
                    LVLiveChatList,
                    listAllData,
                    saveFileDialog,
                    videoID)
                .ContinueWith(_ =>
                {
                    TerminateLongTask(isImport: false);
                });
        }
        catch (Exception ex)
        {
            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());

            TerminateLongTask(isImport: false);

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void BtnClear_Click(object sender, EventArgs e)
    {
        try
        {
            LVLiveChatList.InvokeIfRequired(() =>
            {
                LVLiveChatList.Items.Clear();
            });

            // 清除刪除／封鎖／回覆數更新／投票結果更新事件用的關聯索引，
            // 避免殘留對已經被清空的 ListViewItem 的參照（記憶體洩漏），
            // 也避免下一場直播的事件誤關聯到這一場已清空的舊資料。
            SharedItemsByMessageID.Clear();
            SharedItemsByReplyCountEntityKey.Clear();
            SharedItemsByAuthorChannelID.Clear();

            // 重設累加式統計計數器（務必在呼叫 UpdateSummaryInfo() 之前重設，
            // 否則畫面上的統計文字會先短暫顯示清空前的舊數字）。
            SharedChatCount = 0;
            SharedSuperChatCount = 0;
            SharedSuperStickerCount = 0;
            SharedMemberJoinCount = 0;
            SharedMemberUpgradeCount = 0;
            SharedMemberMilestoneCount = 0;
            SharedMemberGiftCount = 0;
            SharedReceivedMemberGiftCount = 0;
            SharedTotalIncome = 0.0;
            SharedMemberInRoomAuthors.Clear();
            SharedDistinctAuthors.Clear();

            UpdateSummaryInfo();

            TBLog.InvokeIfRequired(TBLog.Clear);

            // 清除 SharedCustomEmojis。
            SharedCustomEmojis.Clear();

            // 清除 SharedBadges。
            SharedBadges.Clear();
        }
        catch (Exception ex)
        {
            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void CBExportAuthorPhoto_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox? checkBox = (CheckBox?)sender;

        if (checkBox == null)
        {
            return;
        }

        checkBox.InvokeIfRequired(() =>
        {
            if (checkBox.Checked != Properties.Settings.Default.ExportAuthorPhoto)
            {
                Properties.Settings.Default.ExportAuthorPhoto = checkBox.Checked;
                Properties.Settings.Default.Save();
            }
        });
    }

    private void CBEnableTTS_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox? checkBox = (CheckBox?)sender;

        if (checkBox == null)
        {
            return;
        }

        checkBox.InvokeIfRequired(() =>
        {
            if (checkBox.Checked != Properties.Settings.Default.EnableTTS)
            {
                Properties.Settings.Default.EnableTTS = checkBox.Checked;
                Properties.Settings.Default.Save();
            }
        });
    }

    private void TBUserAgent_TextChanged(object sender, EventArgs e)
    {
        TextBox? textBox = (TextBox?)sender;

        if (textBox == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(textBox.Text))
        {
            MessageBox.Show(
                "請輸入使用者代理字串。",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (!HttpClientUtil.SetUserAgent(SharedHttpClient, textBox.Text))
        {
            MessageBox.Show(
                "請輸入有效的使用者代理字串。",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (textBox.Text != Properties.Settings.Default.UserAgent)
        {
            Properties.Settings.Default.UserAgent = textBox.Text;
            Properties.Settings.Default.Save();
        }

        // 更新 SharedHttpClient 的標頭資訊。
        HttpClientUtil.UpdateHttpClient(SharedHttpClient, textBox.Text);
    }

    private void BtnSearchUserAgent_Click(object sender, EventArgs e)
    {
        try
        {
            // 藉由 Google 搜尋預設的網頁瀏覽器的使用者代理資訊。
            CustomFunction.OpenBrowser("https://www.google.com/search?q=My+User-Agent");
        }
        catch (Exception ex)
        {
            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 開啟應用程式專屬的 WebView2 登入視窗，取得聊天室擷取用的 Cookie
    /// </summary>
    private void BtnCookieLogin_Click(object sender, EventArgs e)
    {
        try
        {
            using FCookieLogin fCookieLogin = new(this);

            if (fCookieLogin.ShowDialog(this) == DialogResult.OK &&
                fCookieLogin.ResultCookies != null)
            {
                SharedYTJsonParser.Cookies = fCookieLogin.ResultCookies;

                UpdateCookieStatus();

                WriteLog(!string.IsNullOrEmpty(fCookieLogin.ResultCookies) ?
                    "已更新登入用的 Cookie。" :
                    "已清除登入用的 Cookie。");
            }
        }
        catch (Exception ex)
        {
            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void TBSecChUa_TextChanged(object sender, EventArgs e)
    {
        TextBox? textBox = (TextBox?)sender;

        if (textBox == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(textBox.Text))
        {
            MessageBox.Show(
                "請輸入 Sec-CH-UA。",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (textBox.Text != Properties.Settings.Default.SecChUa)
        {
            Properties.Settings.Default.SecChUa = textBox.Text;
            Properties.Settings.Default.Save();
        }

        #region 更新 SharedHttpClient 的標頭資訊

        string userAgent = string.Empty;

        TBUserAgent.InvokeIfRequired(() =>
        {
            userAgent = TBUserAgent.Text;
        });

        // 更新 SharedHttpClient 的標頭資訊。
        HttpClientUtil.UpdateHttpClient(SharedHttpClient, userAgent);

        #endregion
    }

    private void BtnSearch_Click(object sender, EventArgs e)
    {
        if (LVLiveChatList.Items.Count > 0)
        {
            FSearch FSearch = new(this);

            FSearch.Show();
        }
        else
        {
            MessageBox.Show(
                "請確認聊天室內容列表是否有資料。",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void BtnImport_Click(object sender, EventArgs e)
    {
        try
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Excel 活頁簿|*.xlsx",
                Title = "匯入檔案"
            };

            DialogResult dialogResult = openFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                if (string.IsNullOrEmpty(filePath))
                {
                    MessageBox.Show(
                        "請選擇有效的檔案。",
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                if (!File.Exists(filePath))
                {
                    MessageBox.Show(
                        "請確認選擇的檔案已存在。",
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                RunLongTask();

                LoadXLSX(filePath: filePath)
                    .ContinueWith(_ =>
                    {
                        TerminateLongTask(isImport: true);
                    });
            }
        }
        catch (Exception ex)
        {
            WriteLog($"發生錯誤：{ex.GetExceptionMessage()}");

            TerminateLongTask(isImport: true);
        }
    }

    private void LVLiveChatList_MouseClick(object sender, MouseEventArgs e)
    {
        switch (e.Button)
        {
            case MouseButtons.Left:
                TtsSpeak(LVLiveChatList);
                break;
            case MouseButtons.Right:
                OpenYTChannelUrl(LVLiveChatList, e);
                break;
            default:
                break;
        }
    }

    private void LVLiveChatList_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        switch (e.Button)
        {
            case MouseButtons.Left:
                CopyToClipboard(LVLiveChatList);
                break;
            default:
                break;
        }
    }

    private void LVLiveChatList_DragDrop(object sender, DragEventArgs e)
    {
        try
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                List<string>? fileList = ((string[]?)e.Data.GetData(DataFormats.FileDrop))
                     ?.Where(n => Path.GetExtension(n) == ".xlsx")
                     .ToList();

                if (fileList != null)
                {
                    if (fileList.Count == 0)
                    {
                        MessageBox.Show(
                            "請選擇有效的 Excel 檔案。",
                            Text,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }
                    else if (fileList.Count > 1)
                    {
                        MessageBox.Show(
                            "一次僅能匯入一個 Excel 檔案。",
                            Text,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }

                    foreach (string filePath in fileList)
                    {
                        RunLongTask();

                        LoadXLSX(filePath: filePath)
                            .ContinueWith(_ =>
                            {
                                TerminateLongTask(isImport: true);
                            });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());

            WriteLog($"發生錯誤：{ex.GetExceptionMessage()}");

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            TerminateLongTask(isImport: true);
        }
    }

    private void LVLiveChatList_DragEnter(object sender, DragEventArgs e)
    {
        try
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        catch (Exception ex)
        {
            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void CBEnableDebug_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox? checkBox = (CheckBox?)sender;

        if (checkBox == null)
        {
            return;
        }

        checkBox.InvokeIfRequired(() =>
        {
            if (checkBox.Checked != Properties.Settings.Default.EnableDebug)
            {
                Properties.Settings.Default.EnableDebug = checkBox.Checked;
                Properties.Settings.Default.Save();
            }
        });

        if (Properties.Settings.Default.EnableDebug)
        {
            LogManager.ResumeLogging();
        }
        else
        {
            LogManager.SuspendLogging();
        }
    }
}