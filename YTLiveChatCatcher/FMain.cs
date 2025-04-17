﻿using Microsoft.Extensions.Logging;
using NLog;
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Extensions;
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
    public FMain(IHttpClientFactory httpClientFactory, ILogger<FMain> logger)
    {
        InitializeComponent();

        SharedHttpClientFactory = httpClientFactory;
        SharedLogger = logger;
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

    private void TBChannelID_TextChanged(object sender, EventArgs e)
    {
        TextBox? textBox = (TextBox?)sender;

        if (textBox == null)
        {
            return;
        }

        textBox.InvokeIfRequired(async () =>
        {
            textBox.Text = await YTJsonParser
                .GetYouTubeChannelID(textBox.Text.Trim());
        });
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
            textBox.Text = YTJsonParser
                .GetYouTubeVideoID(textBox.Text.Trim());
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

    private void BtnStart_Click(object sender, EventArgs e)
    {
        try
        {
            string videoID = string.Empty;

            TBVideoID.InvokeIfRequired(async () =>
            {
                videoID = TBVideoID.Text;

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

                // 在 LiveChatCatcher 開始前，再設定一次 Cookie。
                SetUseCookie();

                SharedYTJsonParser.StartFetchLiveChatData(videoID);

                WriteLog("開始取得聊天室的內容。");
            });
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

    private void BtnStop_Click(object? sender, EventArgs e)
    {
        try
        {
            YTJsonParser.Stop();

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

    private void CBLoadCookie_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox? checkBox = (CheckBox?)sender;

        if (checkBox == null)
        {
            return;
        }

        checkBox.InvokeIfRequired(() =>
        {
            if (checkBox.Checked != Properties.Settings.Default.LoadCookie)
            {
                Properties.Settings.Default.LoadCookie = checkBox.Checked;
                Properties.Settings.Default.Save();
            }

            SetUseCookie();
        });
    }

    private void CBBrowser_SelectedIndexChanged(object sender, EventArgs e)
    {
        ComboBox? comboBox = (ComboBox?)sender;

        if (comboBox == null)
        {
            return;
        }

        comboBox.InvokeIfRequired(() =>
        {
            if (comboBox.SelectedIndex != Properties.Settings.Default.BrowserItemIndex)
            {
                Properties.Settings.Default.BrowserItemIndex = comboBox.SelectedIndex;
                Properties.Settings.Default.Save();
            }

            SetUseCookie();
        });
    }

    private void TBProfileFolderName_TextChanged(object sender, EventArgs e)
    {
        TextBox? textBox = (TextBox?)sender;

        if (textBox == null)
        {
            return;
        }

        textBox.InvokeIfRequired(() =>
        {
            if (textBox.Text != Properties.Settings.Default.ProfileFolderName)
            {
                string path = $@"C:\Users\{Environment.UserName}\AppData\Local\" +
                    $@"{CBBrowser.SelectedItem?.ToString()!.Replace(" ", "\\")}" +
                    $@"\User Data\{textBox.Text}\";

                if (!Directory.Exists(path))
                {
                    WriteLog($"請輸入有效的設定檔資料夾名稱。{Environment.NewLine}路徑：{path}");

                    return;
                }

                WriteLog($"設定檔資料夾名稱設定成功。{Environment.NewLine}路徑：{path}");

                Properties.Settings.Default.ProfileFolderName = textBox.Text;
                Properties.Settings.Default.Save();

                SetUseCookie();
            }
        });
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