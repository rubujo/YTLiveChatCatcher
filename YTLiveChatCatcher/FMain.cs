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
            CheckCaptureRecovery();

            // 清理過期的頭像落地快取，丟到背景執行緒跑，不要讓目錄掃描／刪檔的 I/O 拖慢啟動流程；
            // AvatarDiskCache.PruneExpiredEntries() 內部已經自行處理例外，這裡不需要再包 try/catch。
            _ = Task.Run(AvatarDiskCache.PruneExpiredEntries);

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

            // 取消尚在執行中的擷取工作。
            SharedFetchCancellationTokenSource?.Cancel();

            // 2026/9 修正：以前這裡取消後就立刻往下 Dispose SharedHttpClient，但背景擷取工作
            // （StartFetchLiveChatData 內的 Task.Run）是非同步的，取消當下如果剛好正在等待一個
            // HTTP 請求的回應，該請求要等 CancellationToken 真正被觀察到才會結束——如果搶在那之前
            // Dispose 掉 HttpClient，該請求會拋出非 OperationCanceledException 的例外，可能被
            // YTJsonParser 的重試機制誤判成暫時性網路錯誤，對著已經 Dispose 的 HttpClient 重試到
            // 次數耗盡。這裡先有限度地等待背景工作先自然結束（正常情況下取消後應該在很短時間內
            // 就會完成，逾時也不阻塞關閉流程太久），再繼續往下釋放資源。
            try
            {
                SharedFetchTask?.Wait(TimeSpan.FromSeconds(5));
            }
            catch (AggregateException)
            {
                // 背景工作內部已經有自己完整的 try/catch 與記錄，這裡只是要確保等待完成，
                // 不需要重複處理或記錄背景工作本身拋出的例外。
            }

            // 釋放 SharedYTJsonParser。
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

    private async void TBVideoID_TextChanged(object sender, EventArgs e)
    {
        // 此事件必定於 UI 執行緒觸發，不需要透過 InvokeIfRequired 轉送
        // （InvokeIfRequired 吃的是 void 委派，若傳入 async lambda 會變成 async void，
        // 第一個 await 之後拋出的例外無法被這裡的 try/catch 攔截）。
        TextBox? textBox = (TextBox?)sender;

        if (textBox == null)
        {
            return;
        }

        // 2026/9 修正：textBox.Text = videoID; 這行如果讓文字內容真的改變（例如貼上的是完整網址、
        // 正規化成純 ID），會同步觸發巢狀的 TextChanged 事件重入這個方法本身。巢狀呼叫在自己的
        // await 處讓出控制權後，外層呼叫恢復執行時 TBChannelID.Text 通常還是空的（巢狀呼叫的網路
        // 請求還沒回來），導致外層也會再呼叫一次同一個 GetChannelIDFromVideoAsync，對同一次貼上
        // 動作發出兩個並發的 HTTP 請求。用這個旗標讓巢狀呼叫在賦值那一刻直接跳過，不做任何處理。
        if (SharedIsNormalizingVideoID)
        {
            return;
        }

        try
        {
            string videoID = YouTubeUrlUtil.GetYouTubeVideoID(textBox.Text.Trim());

            if (textBox.Text != videoID)
            {
                SharedIsNormalizingVideoID = true;

                try
                {
                    textBox.Text = videoID;
                }
                finally
                {
                    SharedIsNormalizingVideoID = false;
                }
            }

            // 跟 BtnStart_Click「頻道 ID 空時才用影片 ID 反查最新直播影片」的方向相反、互補：
            // 只在頻道 ID 欄位還是空的時候才自動帶入，避免覆蓋使用者已經手動輸入的頻道 ID。
            if (!string.IsNullOrEmpty(videoID) && string.IsNullOrEmpty(TBChannelID.Text.Trim()))
            {
                string channelID = await SharedYTJsonParser.GetChannelIDFromVideoAsync(videoID);

                if (!string.IsNullOrEmpty(channelID))
                {
                    TBChannelID.Text = channelID;
                }
            }
        }
        catch (Exception ex)
        {
            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());
        }
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

        SharedFetchTask = Task.Run(async () =>
        {
            try
            {
                await foreach (IReadOnlyList<RendererData> batch in SharedYTJsonParser.StreamLiveChatDataAsync(
                    videoID,
                    intervalProgress: intervalProgress,
                    cancellationToken: cancellationToken))
                {
                    // 先寫進當機復原記錄再處理成 ListView 項目：即使 DoProcessMessages 或後續流程
                    // 出了問題，這批已經收到的原始資料也已經安全落地在本機檔案裡。
                    CaptureRecoveryStore.AppendBatch(batch);

                    await TBUserAgent.InvokeAsyncIfRequired(() => DoProcessMessages(batch), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // 使用者主動停止，不視為錯誤。
            }
            catch (Exception ex)
            {
                SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());

                // 這裡刻意不帶入 cancellationToken——不論是使用者主動停止還是發生例外，
                // 這則記錄都應該要能寫入，而不是被已取消的 token 連帶擋下來。
                await TBUserAgent.InvokeAsyncIfRequired(() => WriteLog(ex.GetExceptionMessage()));
            }
            finally
            {
                // 只有在 SharedFetchCancellationTokenSource 仍然是「這一份」時才還原 UI／清空共用欄位：
                // 如果使用者在這個背景工作跑到這裡之前，已經按過一次「停止」再按「開始」，
                // SharedFetchCancellationTokenSource 這時已經指向新一輪擷取的新實例，
                // 這裡不該把新那一輪的 UI 狀態當成「已停止」還原掉。
                if (ReferenceEquals(SharedFetchCancellationTokenSource, fetchCancellationTokenSource))
                {
                    // 同上，清理／還原 UI 狀態這件事，不應該因為 cancellationToken 已取消而被跳過。
                    await TBUserAgent.InvokeAsyncIfRequired(() => BtnStop_Click(this, new EventArgs()));

                    // 2026/8 修正：這裡以前沒有把 SharedFetchCancellationTokenSource 清為 null，
                    // 讓它繼續指向即將在下一行被 Dispose 的這個實例。使用者實測「開始 -> 停止 -> 開始」
                    // 會在第二次按「開始」時看到「The CancellationTokenSource has been disposed.」——
                    // StartFetchLiveChatData 開頭防禦性的 `SharedFetchCancellationTokenSource?.Cancel()`
                    // 作用在這個已經被 Dispose 過、但欄位仍非 null 的殘留參照上，直接丟出
                    // ObjectDisposedException。清成 null 後，下一次呼叫該行時 `?.` 會直接短路，不會
                    // 呼叫到已經 Dispose 的執行個體。
                    SharedFetchCancellationTokenSource = null;
                }

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

            // 保證在沒有任何 BeginUpdate 視窗、也不會再有後續批次的時間點強制重繪一次，撿回擷取結束前
            // 最後幾筆還在下載中的頭像（理由同 FMain.Methods.cs 頭像下載完成處的說明）。
            LVLiveChatList.InvokeIfRequired(() =>
            {
                LVLiveChatList.Invalidate();
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
            if (SharedListViewItems.Count <= 0)
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

            List<ListViewItem> listAllData = [.. SharedListViewItems];

            RunLongTask();

            // 原本用 .ContinueWith(...) 收尾：預設的 ContinueWith 不論前面的 Task 是成功或失敗都會執行，
            // 且回傳的 Task 只反映 ContinueWith 委派本身的結果——這代表 DoExportTask 拋出的例外會被吞掉，
            // 不會被下面的 catch 攔到，使用者只會看到「作業完成」，看不到真正失敗的原因。改用
            // try/finally：TerminateLongTask 一樣保證會執行，但例外現在會正確往外傳給下面的 catch。
            try
            {
                await DoExportTask(
                    LVLiveChatList,
                    listAllData,
                    saveFileDialog,
                    videoID);
            }
            finally
            {
                TerminateLongTask(isImport: false);
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

    private async void BtnExportCommunityPosts_Click(object sender, EventArgs e)
    {
        try
        {
            string channelID = string.Empty;

            TBChannelID.InvokeIfRequired(() =>
            {
                channelID = TBChannelID.Text.Trim();
            });

            if (string.IsNullOrEmpty(channelID))
            {
                MessageBox.Show(
                    "匯出失敗，請先輸入頻道 ID。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            string cleanedFileName = CustomFunction.RemoveInvalidFilePathCharacters(
                $"{StringSet.SheetName7}_{channelID}_{DateTime.Now:yyyyMMdd}",
                "_");

            SaveFileDialog saveFileDialog = new()
            {
                Filter = "Excel 活頁簿|*.xlsx",
                Title = "儲存檔案",
                FileName = cleanedFileName
            };

            DialogResult dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult != DialogResult.OK)
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

            RunLongTask();

            // 比照 BtnExport_Click／BtnImport_Click：用 try/finally 而不是 .ContinueWith(...)，
            // 確保 DoExportCommunityPostsTask 拋出的例外會正確往外傳給下面的 catch，
            // 不會被靜默吞掉。
            try
            {
                await DoExportCommunityPostsTask(
                    channelID,
                    saveFileDialog,
                    CancellationToken.None);
            }
            finally
            {
                TerminateLongTask(isImport: false);
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

    private void BtnClear_Click(object sender, EventArgs e)
    {
        try
        {
            LVLiveChatList.InvokeIfRequired(() =>
            {
                SharedListViewItems.Clear();
                LVLiveChatList.VirtualListSize = 0;

                // 頭像圖片快取在清除聊天室之前不會自動釋放，同一次執行期間清除、開始擷取下一場
                // 直播時會無限累積在記憶體裡，這裡一併清空。
                //
                // 2026/9 修正：改成指派一份全新的 ImageList，而不是原地呼叫 Clear()。FSearch
                // 開啟搜尋結果時會直接共用（不是複製）這個 ImageList 物件參照（見 FSearch.cs
                // 的 BtnSearch_Click），如果原地 Clear() 再重新累加下一場直播的新頭像，搜尋視窗裡
                // 已經複製出去的 ListViewItem 的 ImageIndex（整數索引）會全部指向新的、不相關的
                // 圖片——因為 ImageIndex 本身沒有失效機制。改成整個換一份新的 ImageList，讓
                // FSearch 手上還握著的舊物件參照維持原樣、不受這次清空影響，直到使用者在
                // FSearch 重新搜尋（那時會重新指向 LVLiveChatList 目前這一份最新的 ImageList）。
                ImageList? previousImageList = LVLiveChatList.SmallImageList;

                LVLiveChatList.SmallImageList = new ImageList
                {
                    ImageSize = previousImageList?.ImageSize ?? new Size(32, 32),
                    ColorDepth = previousImageList?.ColorDepth ?? ColorDepth.Depth24Bit,
                };
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
            SharedIncomeByCurrency.Clear();
            SharedMemberInRoomAuthors.Clear();
            SharedDistinctAuthors.Clear();

            // 使用者主動清空聊天室，代表明確不需要保留這批資料，一併清除當機復原記錄。
            CaptureRecoveryStore.Clear();

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
        if (SharedListViewItems.Count > 0)
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

    private async void BtnImport_Click(object sender, EventArgs e)
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

                // 原本是完全沒有 await 的 fire-and-forget：LoadXLSX(...).ContinueWith(...) 沒有被等待，
                // 代表 LoadXLSX 拋出的例外根本不會被下面的 catch 攔到（也不會讓應用程式崩潰，就只是
                // 靜默消失），使用者會看到匯入按鈕重新可以按，卻完全不知道匯入其實失敗、原因是什麼。
                // 改成 await + try/finally 後，例外會正確往外傳給下面的 catch。
                try
                {
                    await LoadXLSX(filePath: filePath);
                }
                finally
                {
                    TerminateLongTask(isImport: true);
                }
            }
        }
        catch (Exception ex)
        {
            WriteLog($"發生錯誤：{ex.GetExceptionMessage()}");
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

    private async void LVLiveChatList_DragDrop(object sender, DragEventArgs e)
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

                    // 同一個原因（原本是完全沒有 await 的 fire-and-forget，LoadXLSX 拋出的例外會被
                    // 靜默吞掉，不會被下面的 catch 攔到）改成 await + try/finally，見 BtnImport_Click。
                    foreach (string filePath in fileList)
                    {
                        RunLongTask();

                        try
                        {
                            await LoadXLSX(filePath: filePath);
                        }
                        finally
                        {
                            TerminateLongTask(isImport: true);
                        }
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