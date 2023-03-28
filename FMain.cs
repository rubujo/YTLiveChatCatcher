using Microsoft.Extensions.Logging;
using NLog;
using System.Text.RegularExpressions;
using YTApi;
using YTLiveChatCatcher.Common;
using YTLiveChatCatcher.Common.Sets;
using YTLiveChatCatcher.Common.Utils;
using YTLiveChatCatcher.Extensions;

namespace YTLiveChatCatcher;

public partial class FMain : Form
{
    public FMain(IHttpClientFactory httpClientFactory, ILogger<FMain> logger)
    {
        InitializeComponent();

        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private void FMain_Load(object sender, EventArgs e)
    {
        try
        {
            InitControls();
            InitListView(LVLiveChatList);
            InitTimer();
            CheckAppVersion();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"發生錯誤：{ex}",
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
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"發生錯誤：{ex}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void TBChannelID_TextChanged(object sender, EventArgs e)
    {
        TBChannelID.InvokeIfRequired(async () =>
        {
            string tempValue = TBChannelID.Text.Trim();

            if (tempValue.Contains($"{StringSet.Origin}/channel/"))
            {
                tempValue = tempValue.Replace($"{StringSet.Origin}/channel/", string.Empty);
            }
            else if (tempValue.Contains($"{StringSet.Origin}/c/"))
            {
                tempValue = await LiveChatFunction.GetYtChIdByYtChCustomUrl(tempValue) ?? string.Empty;
            }
            else if (tempValue.Contains('@'))
            {
                tempValue = await LiveChatFunction.GetYtChIdByYtChCustomUrl(tempValue) ?? string.Empty;
            }

            TBChannelID.Text = tempValue;
        });
    }

    private void TBVideoID_TextChanged(object sender, EventArgs e)
    {
        TBVideoID.InvokeIfRequired(() =>
        {
            string tempValue = TBVideoID.Text.Trim();

            // 參考：https://stackoverflow.com/a/15219045
            Regex regex = RegexYouTubeUrl();

            tempValue = regex.Replace(tempValue, string.Empty);

            if (tempValue.Contains("&list="))
            {
                string[] tempArray = tempValue.Split("&list=");

                tempValue = tempArray[0];
            }

            TBVideoID.Text = tempValue;
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
            MessageBox.Show(
                $"發生錯誤：{ex}",
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

    private void CBRandomInterval_CheckedChanged(object sender, EventArgs e)
    {
        CBRandomInterval.InvokeIfRequired(() =>
        {
            TBInterval.InvokeIfRequired(() =>
            {
                if (CBRandomInterval.Checked)
                {
                    int interval = CustomFunction.GetRandomInterval();

                    TBInterval.Text = (interval / 1000).ToString();
                };

                TBInterval.Enabled = !CBRandomInterval.Checked;
            });
        });
    }

    private void RBtnStreaming_CheckedChanged(object sender, EventArgs e)
    {
        RBtnStreaming.InvokeIfRequired(() =>
        {
            IsStreaming = RBtnStreaming.Checked;

            if (IsStreaming != Properties.Settings.Default.IsStreaming)
            {
                Properties.Settings.Default.IsStreaming = IsStreaming;
                Properties.Settings.Default.Save();
            }
        });
    }

    private void RBtnReplay_CheckedChanged(object sender, EventArgs e)
    {
        RBtnReplay.InvokeIfRequired(() =>
        {
            IsStreaming = !RBtnReplay.Checked;

            if (IsStreaming != Properties.Settings.Default.IsStreaming)
            {
                Properties.Settings.Default.IsStreaming = IsStreaming;
                Properties.Settings.Default.Save();
            }
        });
    }

    private void BtnStart_Click(object sender, EventArgs e)
    {
        try
        {
            string videoID = TBVideoID.Text.Trim();

            if (string.IsNullOrEmpty(videoID))
            {
                TBVideoID.InvokeIfRequired(() =>
                {
                    string userAgent = string.Empty;

                    TBUserAgent.InvokeIfRequired(() =>
                    {
                        userAgent = TBUserAgent.Text;
                    });

                    // 取得 HttpClient。
                    using HttpClient httpClient = HttpClientUtil.GetHttpClient(
                        _httpClientFactory,
                        _logger,
                        userAgent);

                    TBVideoID.Text = LiveChatFunction.GetLatestStreamingVideoID(
                        httpClient,
                        TBChannelID.Text.Trim(),
                        GetCookies(),
                        TBLog);
                });

                videoID = TBVideoID.Text.Trim();

                if (!string.IsNullOrEmpty(videoID))
                {
                    WriteLog($"透過頻道 ID 取得的影片 ID：{videoID}");
                }
                else
                {
                    WriteLog("透過頻道 ID 取得影片 ID 失敗。");
                }
            }

            if (!string.IsNullOrEmpty(videoID))
            {
                CBRandomInterval.InvokeIfRequired(() =>
                {
                    bool isIntervalSet = false;

                    if (CBRandomInterval.Checked)
                    {
                        SharedTimer!.Interval = CustomFunction.GetRandomInterval();

                        isIntervalSet = true;
                    }
                    else
                    {
                        if (int.TryParse(TBInterval.Text.Trim(), out int interval))
                        {
                            if (interval >= 3)
                            {
                                SharedTimer!.Interval = interval * 1000;

                                isIntervalSet = true;
                            }
                            else
                            {
                                isIntervalSet = false;

                                MessageBox.Show(
                                    "目前的間隔秒數太低，請調高；最低不可以低於 3 秒。",
                                    Text,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                            }
                        }
                        else
                        {
                            isIntervalSet = false;

                            MessageBox.Show(
                                "請在間隔欄位輸入數值。",
                                Text,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    }

                    if (isIntervalSet)
                    {
                        // 設定控制項的狀態。
                        SetControlsState(false);

                        string userAgent = string.Empty;

                        TBUserAgent.InvokeIfRequired(() =>
                        {
                            userAgent = TBUserAgent.Text;
                        });

                        // 取得 HttpClient。
                        using HttpClient httpClient = HttpClientUtil.GetHttpClient(
                            _httpClientFactory,
                            _logger,
                            userAgent);

                        SharedYTConfig = LiveChatFunction.GetYTConfig(
                            httpClient,
                            videoID,
                            IsStreaming,
                            GetCookies(),
                            TBLog);

                        WriteLog("開始取得聊天室的內容。");

                        // 執行第一次取得影片聊天室的內容。
                        GetLiveChatData();

                        // 啟動 SharedTimer。
                        SharedTimer!.Start();
                    }
                });
            }
            else
            {
                MessageBox.Show(
                    "請輸入頻道 ID 或是影片 ID。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            BtnStop_Click(null, new EventArgs());

            MessageBox.Show(
                $"發生錯誤：{ex}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void BtnStop_Click(object? sender, EventArgs e)
    {
        try
        {
            if (SharedTask?.Status != TaskStatus.RanToCompletion ||
                SharedTask?.Status != TaskStatus.Canceled)
            {
                SharedCancellationTokenSource?.Cancel();
            }

            // 設定控制項的狀態。
            SetControlsState(true);

            // 停止 SharedTimer。
            SharedTimer?.Stop();

            // 清理全域變數。
            SharedJsonElement = new();

            SharedYTConfig = null;

            SharedTimeoutMs = 0;

            WriteLog("已停止取得聊天室的內容。");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"發生錯誤：{ex}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void BtnExport_Click(object sender, EventArgs e)
    {
        try
        {
            if (CBExportAuthorPhoto.Checked)
            {
                DialogResult dialogResult = MessageBox.Show(
                    "注意，啟用匯出頭像會花費大量的時間，如您欲繼續作業請按「確定」按鈕。",
                    Text,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.OK)
                {
                    await DoExportTask(LVLiveChatList);
                }
            }
            else
            {
                await DoExportTask(LVLiveChatList);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"發生錯誤：{ex}",
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
            MessageBox.Show(
                $"發生錯誤：{ex}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void CBExportAuthorPhoto_CheckedChanged(object sender, EventArgs e)
    {
        CBExportAuthorPhoto.InvokeIfRequired(() =>
        {
            if (CBExportAuthorPhoto.Checked != Properties.Settings.Default.ExportAuthorPhoto)
            {
                Properties.Settings.Default.ExportAuthorPhoto = CBExportAuthorPhoto.Checked;
                Properties.Settings.Default.Save();
            }
        });
    }

    private void CBEnableTTS_CheckedChanged(object sender, EventArgs e)
    {
        CBEnableTTS.InvokeIfRequired(() =>
        {
            if (CBEnableTTS.Checked != Properties.Settings.Default.EnableTTS)
            {
                Properties.Settings.Default.EnableTTS = CBEnableTTS.Checked;
                Properties.Settings.Default.Save();
            }
        });
    }

    private void TBUserAgent_TextChanged(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(TBUserAgent.Text))
        {
            string userAgent = string.Empty;

            TBUserAgent.InvokeIfRequired(() =>
            {
                userAgent = TBUserAgent.Text;
            });

            // 取得 HttpClient。
            using HttpClient httpClient = HttpClientUtil
                .GetHttpClient(_httpClientFactory, _logger, userAgent);

            if (HttpClientUtil.CheckUserAgent(httpClient, TBUserAgent.Text))
            {
                if (TBUserAgent.Text != Properties.Settings.Default.UserAgent)
                {
                    Properties.Settings.Default.UserAgent = TBUserAgent.Text;
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                MessageBox.Show(
                    "請輸入有效的使用者代理字串。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        else
        {
            MessageBox.Show(
                "請輸入使用者代理字串。",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void BtnSearchUserAgent_Click(object sender, EventArgs e)
    {
        try
        {
            // 藉由 Google 搜尋預設的網頁瀏覽器的使用者代理資訊。
            //CustomFunction.OpenBrowser("https://www.google.com/search?q=My+User+Agent");

            // 2023-03-01 改為使用 WhatIsMyBrowser.com 網站查詢。
            CustomFunction.OpenBrowser("https://www.whatismybrowser.com/detect/what-is-my-user-agent/");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"發生錯誤：{ex}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void CBLoadCookies_CheckedChanged(object sender, EventArgs e)
    {
        CBLoadCookies.InvokeIfRequired(() =>
        {
            if (CBLoadCookies.Checked != Properties.Settings.Default.LoadCookies)
            {
                Properties.Settings.Default.LoadCookies = CBLoadCookies.Checked;
                Properties.Settings.Default.Save();
            }
        });
    }

    private void CBBrowser_SelectedIndexChanged(object sender, EventArgs e)
    {
        CBBrowser.InvokeIfRequired(() =>
        {
            if (CBBrowser.SelectedIndex != Properties.Settings.Default.BrowserItemIndex)
            {
                Properties.Settings.Default.BrowserItemIndex = CBBrowser.SelectedIndex;
                Properties.Settings.Default.Save();
            }
        });
    }

    private void TBProfileFolderName_TextChanged(object sender, EventArgs e)
    {
        TBProfileFolderName.InvokeIfRequired(() =>
        {
            if (TBProfileFolderName.Text != Properties.Settings.Default.ProfileFolderName)
            {
                string path = $@"C:\Users\{Environment.UserName}\AppData\Local\" +
                    $@"{CBBrowser.SelectedItem?.ToString()!.Replace(" ", "\\")}" +
                    $@"\User Data\{TBProfileFolderName.Text}\";

                if (Directory.Exists(path))
                {
                    Properties.Settings.Default.ProfileFolderName = TBProfileFolderName.Text;
                    Properties.Settings.Default.Save();

                    WriteLog($"設定檔資料夾名稱設定成功。{Environment.NewLine}路徑：{path}");
                }
                else
                {
                    WriteLog($"請輸入有效的設定檔資料夾名稱。{Environment.NewLine}路徑：{path}");
                }
            }
        });
    }

    private void BtnImport_Click(object sender, EventArgs e)
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

            if (!string.IsNullOrEmpty(filePath))
            {
                if (File.Exists(filePath))
                {
                    DoImportData(filePath);
                }
                else
                {
                    MessageBox.Show(
                        "請確認選擇的檔案已存在。",
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show(
                    "請選擇有效的檔案。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
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
                        DoImportData(filePath);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"發生錯誤：{ex}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
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
            MessageBox.Show(
                $"發生錯誤：{ex}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void CBEnableDebug_CheckedChanged(object sender, EventArgs e)
    {
        CBEnableDebug.InvokeIfRequired(() =>
        {
            if (CBEnableDebug.Checked != Properties.Settings.Default.EnableDebug)
            {
                Properties.Settings.Default.EnableDebug = CBEnableDebug.Checked;
                Properties.Settings.Default.Save();
            }
        });
    }
}