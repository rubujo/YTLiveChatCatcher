using Microsoft.Extensions.Logging;
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Extensions;
using System.Data;
using YTLiveChatCatcher.Common;
using YTLiveChatCatcher.Common.Sets;
using YTLiveChatCatcher.Extensions;

namespace YTLiveChatCatcher;

public partial class FSearch : Form
{
    /// <summary>
    /// LVFilteredList 是 VirtualMode，這是它唯一的真實資料來源，取代目前透過
    /// <c>LVFilteredList.Items</c> 存取的所有地方（VirtualMode 下該集合完全禁止存取）。
    /// </summary>
    private readonly List<ListViewItem> SharedFilteredListViewItems = [];

    public FSearch(FMain fmain)
    {
        InitializeComponent();

        Icon = Properties.Resources.app_icon;
        Text = $"搜尋 - {fmain.Text}";

        _FMain = fmain;
        _LVLiveChatList = fmain.Controls
            .OfType<ListView>()
            .FirstOrDefault(n => n.Name == "LVLiveChatList")!;
        _CBExportAuthorPhoto = fmain.Controls
            .OfType<CheckBox>()
            .FirstOrDefault(n => n.Name == "CBExportAuthorPhoto")!;
        _BtnSearch = fmain.Controls
            .OfType<Button>()
            .FirstOrDefault(n => n.Name == "BtnSearch")!;
    }

    /// <summary>
    /// LVFilteredList 的 VirtualMode 資料供應：每次要顯示／重繪某一列時都會透過這個事件跟
    /// <see cref="SharedFilteredListViewItems"/> 要資料。
    /// </summary>
    private void LVFilteredList_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e)
    {
        e.Item = SharedFilteredListViewItems[e.ItemIndex];
    }

    private void FSearch_Load(object sender, EventArgs e)
    {
        try
        {
            _BtnSearch.InvokeIfRequired(() =>
            {
                _BtnSearch.Enabled = false;
            });

            FMain.InitListView(LVFilteredList);
        }
        catch (Exception ex)
        {
            _FMain.GetSharedLogger().LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void FSearch_FormClosing(object sender, FormClosingEventArgs e)
    {
        try
        {
            _BtnSearch.InvokeIfRequired(() =>
            {
                _BtnSearch.Enabled = true;
            });
        }
        catch (Exception ex)
        {
            _FMain.GetSharedLogger().LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void BtnSearch_Click(object sender, EventArgs e)
    {
        try
        {
            string keyword = string.Empty;

            TBKeyword.InvokeIfRequired(() =>
            {
                keyword = TBKeyword.Text;
            });

            if (!string.IsNullOrEmpty(keyword))
            {
                LVFilteredList.InvokeIfRequired(() =>
                {
                    // LVLiveChatList 是 VirtualMode，Items 集合禁止存取，改讀 FMain 公開的
                    // GetSharedListViewItems()（見 FMain.Methods.cs）。
                    ListViewItem?[] dataSet = _FMain.GetSharedListViewItems()
                        .Where(n => n.SubItems[0].Text.Contains(keyword) ||
                            n.SubItems[2].Text.Contains(keyword) ||
                            n.SubItems[5].Text.Contains(keyword))
                        .Select(n => n.Clone() as ListViewItem)
                        .Reverse()
                        .ToArray();

                    if (dataSet.Length <= 0)
                    {
                        SharedFilteredListViewItems.Clear();
                        LVFilteredList.VirtualListSize = 0;

                        MessageBox.Show(
                            $"關鍵字「{keyword}」查無資料。",
                            Text,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                    else
                    {
                        LVFilteredList.SmallImageList = _LVLiveChatList.SmallImageList;

                        LVFilteredList.BeginUpdate();
                        SharedFilteredListViewItems.Clear();
                        SharedFilteredListViewItems.AddRange(dataSet!);
                        LVFilteredList.VirtualListSize = SharedFilteredListViewItems.Count;
                        LVFilteredList.EndUpdate();
                    }

                    LChatCount.InvokeIfRequired(() =>
                    {
                        LChatCount.Text = $"留言數量：{SharedFilteredListViewItems.Count} 個";
                    });
                });
            }
            else
            {
                MessageBox.Show(
                    "請確認您有輸入關鍵字。",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            _FMain.GetSharedLogger().LogError("{ErrorMessage}", ex.GetExceptionMessage());

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
            LVFilteredList.InvokeIfRequired(() =>
            {
                TBKeyword.InvokeIfRequired(() =>
                {
                    TBKeyword.Clear();
                });

                SharedFilteredListViewItems.Clear();
                LVFilteredList.VirtualListSize = 0;

                LChatCount.InvokeIfRequired(() =>
                {
                    LChatCount.Text = $"留言數量：{SharedFilteredListViewItems.Count} 個";
                });
            });
        }
        catch (Exception ex)
        {
            _FMain.GetSharedLogger().LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void LVFilteredList_MouseClick(object sender, MouseEventArgs e)
    {
        switch (e.Button)
        {
            case MouseButtons.Left:
                _FMain.TtsSpeak(LVFilteredList);
                break;
            case MouseButtons.Right:
                _FMain.OpenYTChannelUrl(LVFilteredList, e);
                break;
            default:
                break;
        }
    }

    private void LVFilteredList_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        switch (e.Button)
        {
            case MouseButtons.Left:
                _FMain.CopyToClipboard(LVFilteredList);
                break;
            default:
                break;
        }
    }

    private async void BtnExport_Click(object sender, EventArgs e)
    {
        try
        {
            if (SharedFilteredListViewItems.Count <= 0)
            {
                MessageBox.Show(
                  "匯出失敗，請先確認聊天室內容是否有資料。",
                  Text,
                  MessageBoxButtons.OK,
                  MessageBoxIcon.Error);

                return;
            }

            if (_CBExportAuthorPhoto.Checked)
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

            TextBox TBVideoID = _FMain.GetTBVideoID();

            TBVideoID.InvokeIfRequired(() =>
            {
                videoID = TBVideoID.Text.Trim();
            });

            // 取得影片的標題。
            string videoTitle = await _FMain.GetSharedYTJsonParser().GetVideoTitleAsync(videoID);

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

            List<ListViewItem> listAllData = [.. SharedFilteredListViewItems];

            BtnExport.InvokeIfRequired(() =>
            {
                BtnExport.Enabled = false;
            });

            TBKeyword.InvokeIfRequired(() =>
            {
                TBKeyword.Enabled = false;
            });

            BtnSearch.InvokeIfRequired(() =>
            {
                BtnSearch.Enabled = false;
            });

            BtnClear.InvokeIfRequired(() =>
            {
                BtnClear.Enabled = false;
            });

            PBProgress.InvokeIfRequired(() =>
            {
                PBProgress.Style = ProgressBarStyle.Marquee;
            });

            UseWaitCursor = true;

            // 原本用 .ContinueWith(...) 收尾：預設的 ContinueWith 不論前面的 Task 是成功或失敗都會執行，
            // 且回傳的 Task 只反映 ContinueWith 委派本身的結果——這代表 DoExportTask 拋出的例外會被吞掉，
            // 不會被下面的 catch 攔到，使用者只會看到「作業完成」，看不到真正失敗的原因。改用
            // try/finally：控制項的還原邏輯一樣保證會執行（也不用在 catch 裡再複製一份），
            // 但例外現在會正確往外傳給下面的 catch，「作業完成」的記錄也只會在真的成功時才寫入。
            try
            {
                await _FMain.DoExportTask(
                    LVFilteredList,
                    listAllData,
                    saveFileDialog,
                    videoID);

                _FMain.WriteLog($"*.xlsx 匯出作業完成。");
            }
            finally
            {
                BtnExport.InvokeIfRequired(() =>
                {
                    BtnExport.Enabled = true;
                });

                TBKeyword.InvokeIfRequired(() =>
                {
                    TBKeyword.Enabled = true;
                });

                BtnSearch.InvokeIfRequired(() =>
                {
                    BtnSearch.Enabled = true;
                });

                BtnClear.InvokeIfRequired(() =>
                {
                    BtnClear.Enabled = true;
                });

                PBProgress.InvokeIfRequired(() =>
                {
                    PBProgress.Style = ProgressBarStyle.Blocks;
                });

                UseWaitCursor = false;
            }
        }
        catch (Exception ex)
        {
            _FMain.GetSharedLogger().LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}