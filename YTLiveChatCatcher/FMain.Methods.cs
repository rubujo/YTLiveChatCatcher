using Color = System.Drawing.Color;
using HorizontalAlignment = System.Windows.Forms.HorizontalAlignment;
using Microsoft.Extensions.Logging;
using NLog;
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Sets;
using Rubujo.YouTube.Utility.Utils;
using Size = System.Drawing.Size;
using StringSet = YTLiveChatCatcher.Common.Sets.StringSet;
using System.Runtime.Versioning;
using System.Text;
using YTLiveChatCatcher.Common;
using YTLiveChatCatcher.Common.Utils;
using YTLiveChatCatcher.Extensions;
using Rubujo.YouTube.Utility.Models.LiveChat;
using OfficeOpenXml.Drawing.Chart.Style;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style.XmlAccess;
using OfficeOpenXml.Style;
using OfficeOpenXml;

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
        // 2026/8 修正：可見欄位（Width > 0）加總過去曾經超過 LVLiveChatList 控制項本身的寬度
        // （Designer.cs 的 Size.Width），導致最右側幾欄被控制項邊界擠壓、標題文字看起來截斷／重疊。
        // 這裡刻意讓可見欄位加總比控制項寬度少 30px 以上的緩衝：(1) 直播進行中訊息一多，ListView
        // 會出現垂直捲軸，捲軸本身會吃掉一截水平空間（尤其在非 100% DPI 縮放下更寬）；(2) 本應用程式
        // 用 AutoScaleMode.Font 讓整個表單跟著系統 DPI 縮放，每個欄位寬度是各自獨立四捨五入換算，
        // 加總後未必剛好等於控制項寬度的換算結果，抓死剛好相等在非 100% 縮放（例如 125%）下容易再度
        // 溢出。新增或調整欄位寬度時，維持這個緩衝，不要讓加總逼近控制項寬度。
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
                Width = 85,
                DisplayIndex = 1
            },
            new()
            {
                Name = "Message",
                Text = "訊息",
                TextAlign = HorizontalAlignment.Left,
                Width = 275,
                DisplayIndex = 2
            },
            new()
            {
                Name = "PurchaseAmount",
                Text = "金額",
                TextAlign = HorizontalAlignment.Left,
                Width = 68,
                DisplayIndex = 3
            },
            new()
            {
                Name = "TimestampUsec",
                Text = "時間",
                TextAlign = HorizontalAlignment.Center,
                Width = 125,
                DisplayIndex = 4
            },
            new()
            {
                Name = "Type",
                Text = "類型",
                TextAlign = HorizontalAlignment.Center,
                Width = 85,
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
            },
            new()
            {
                Name = "LeaderboardRank",
                Text = "排行榜",
                TextAlign = HorizontalAlignment.Center,
                Width = 95,
                DisplayIndex = 12
            },
            new()
            {
                Name = "ReplyCount",
                Text = "回覆數",
                TextAlign = HorizontalAlignment.Center,
                Width = 65,
                DisplayIndex = 13
            },
            new()
            {
                Name = "HeaderBackgroundColor",
                Text = "標頭背景顏色",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示（跟 ForegroundColor／BackgroundColor 一樣，僅供樣式套用與匯出使用）。
                Width = 0,
                DisplayIndex = 14
            },
            new()
            {
                Name = "ReplyCountEntityKey",
                Text = "回覆數關聯鍵值",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示，純粹用於回覆數更新事件的關聯查找。
                Width = 0,
                DisplayIndex = 15
            }
        ];

        listview.Columns.AddRange(columnHeaders);

        // 2026/8 修正：原本用 Depth32Bit，但 WinForms ImageList 在 32-bit 色深下對 alpha 通道的處理
        // 已知有瑕疵（.NET 8 起 32-bit 才是預設值，之前一直是 8-bit）——即使來源圖片（頭像固定是
        // JPEG，本身沒有 alpha 通道）沒有透明度需求，內部格式轉換仍可能把 alpha 處理成 0（全透明），
        // 造成圖片實際上加入成功、SmallImageList.Images 也確實有這筆資料，畫面上卻完全看不到（已用
        // 診斷紀錄逐步排除下載失敗／未加入 ImageList／找不到列這幾種可能，才查到這個已知限制）。
        // 頭像不需要透明背景，改用 Depth24Bit 完全避開 alpha 通道處理，不受這個限制影響。
        ImageList imageList = new()
        {
            ImageSize = new Size(32, 32),
            ColorDepth = ColorDepth.Depth24Bit
        };

        listview.SmallImageList = imageList;
    }

    /// <summary>
    /// LVLiveChatList 的 VirtualMode 資料供應：VirtualMode 下 ListView 不自己保存項目，
    /// 每次要顯示／重繪某一列時都會透過這個事件跟 <see cref="SharedListViewItems"/> 要資料。
    /// </summary>
    private void LVLiveChatList_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e)
    {
        e.Item = SharedListViewItems[e.ItemIndex];
    }

    // 對應 InitListView 的 columnHeaders 陣列索引，每個可見欄位各自的上限寬度；HasIcon 只有
    // 「作者名稱」是 true（該欄位左側還要留給頭像圖示的空間）。
    //
    // 2026/8 修正：原本這裡不含「訊息」欄位（索引 2），改成由其他欄位變寬時「跟訊息欄搶位子」
    // （從訊息欄扣掉等量寬度），把總欄寬預算鎖死在 InitListView 開頭那段安全緩衝的範圍內——
    // 但訊息內容本身才是使用者最需要看清楚的部分，不應該為了遷就這個預算被截斷。改成訊息欄
    // 也一併列入自動加寬，且每一欄都獨立依內容加寬，不再互相搶位子；加總後若超出 ListView
    // 目前的可視寬度，讓 WinForms 自然出現水平捲軸即可——這是使用者能理解、可以主動捲動看到
    // 完整內容的標準行為，比把內容硬截斷成「...」永久看不到更好。
    private static readonly (int ColumnIndex, int MaxWidth, bool HasIcon)[] AutoFitColumnRules =
    [
        (0, 200, true),   // 作者名稱
        (1, 130, false),  // 徽章
        (2, 500, false),  // 訊息
        (3, 110, false),  // 金額
        (4, 210, false),  // 時間（TimestampUsec，實際內容是已轉成當地語系格式的日期時間字串，長度變化很大）
        (5, 120, false),  // 類型
        (12, 140, false), // 排行榜
        (13, 100, false), // 回覆數
    ];

    private const int AutoFitAuthorNameIconWidth = 32;
    private const int AutoFitCellPadding = 16;

    /// <summary>
    /// 依這一批新加入的內容動態調整各欄位寬度，讓內容不再被省略號截斷。
    /// <para>每一欄都獨立依內容加寬（互不影響彼此），只會變寬、不會縮回去（避免捲動到不同批次時欄寬
    /// 忽大忽小）；加總後若超出 ListView 目前的可視寬度，交給 WinForms 自然出現水平捲軸。
    /// LVLiveChatList／LVFilteredList 共用同一份 InitListView 建立的欄位配置，這裡也一併共用，
    /// 兩邊都要呼叫。</para>
    /// </summary>
    /// <param name="listView">ListView，LVLiveChatList 或 LVFilteredList</param>
    /// <param name="newItems">List&lt;ListViewItem&gt;，這一批新增的項目</param>
    public static void AutoFitListViewColumns(ListView listView, List<ListViewItem> newItems)
    {
        if (newItems.Count == 0)
        {
            return;
        }

        using Graphics graphics = listView.CreateGraphics();

        foreach ((int columnIndex, int maxWidth, bool hasIcon) in AutoFitColumnRules)
        {
            int requiredWidth = listView.Columns[columnIndex].Width;

            // 已到上限的欄位不需要在之後每一批資料重新量測。大量匯入時，這可避免對數萬列
            // 重複呼叫昂貴的 GDI 文字量測，且不改變任何可見結果。
            if (requiredWidth >= maxWidth)
            {
                continue;
            }

            foreach (ListViewItem item in newItems)
            {
                // 第 0 欄（作者名稱）用的是 ListViewItem.Text 本身，其餘欄位才是對應索引的 SubItems。
                string text = columnIndex == 0 ? item.Text : item.SubItems[columnIndex].Text;
                int measuredWidth = TextRenderer.MeasureText(graphics, text, listView.Font).Width + AutoFitCellPadding;

                if (hasIcon)
                {
                    measuredWidth += AutoFitAuthorNameIconWidth;
                }

                requiredWidth = Math.Max(requiredWidth, measuredWidth);

                if (requiredWidth >= maxWidth)
                {
                    break;
                }
            }

            listView.Columns[columnIndex].Width = Math.Min(requiredWidth, maxWidth);
        }
    }

    /// <summary>
    /// 最短間隔（毫秒）：兩次 <see cref="InvalidateLiveChatListThrottled"/> 之間至少要間隔這麼久，
    /// 才會真的呼叫 <c>LVLiveChatList.Invalidate()</c>。對人眼而言遠低於能感知到延遲的門檻，
    /// 但足以避免重播密集批次期間短時間內觸發大量重複的整可視範圍重繪。
    /// </summary>
    private const int LiveChatListInvalidateThrottleMs = 250;

    /// <summary>
    /// 節流版的 <c>LVLiveChatList.Invalidate()</c>：每個批次的 EndUpdate() 之後都要補一次 Invalidate()
    /// 撿回頭像下載完成時被 BeginUpdate 視窗吃掉的重繪（見 AGENTS.md 的說明），但重播密集批次期間
    /// 可能在數秒內連續呼叫數十次，這裡限制最短間隔，超過門檻才真的觸發重繪。
    /// <para>這裡只節流「per-batch 的補償重繪」這個情境，不能拿來取代整場擷取真正停止時
    /// （<see cref="BtnStop_Click"/>）的那次 Invalidate()——那次是保證流程結束後只會觸發一次的
    /// 最終收尾動作，不在密集迴圈裡，沒有節流的必要，也不應該被節流影響到而漏掉。</para>
    /// </summary>
    private void InvalidateLiveChatListThrottled()
    {
        DateTime now = DateTime.UtcNow;

        if ((now - SharedLastLiveChatListInvalidateUtc).TotalMilliseconds < LiveChatListInvalidateThrottleMs)
        {
            return;
        }

        SharedLastLiveChatListInvalidateUtc = now;

        LVLiveChatList.Invalidate();
    }

    /// <summary>
    /// 組出安全的 IMAGE() 公式字串。
    /// <para>2026/9 修正：原本 8 個匯出用的呼叫點（聊天記錄的頭像／自定義表情符號／會員徽章／
    /// 超級貼圖，以及社群貼文的縮圖／圖片附件／影片縮圖／投票選項圖片）都各自用
    /// <c>$"IMAGE(\"{url}\")"</c> 這種簡單字串插值組公式，網址一旦包含雙引號就會把公式字串截斷，
    /// 後面的內容被當成公式的一部分解析（甚至可能被拼接成非預期的公式片段）。這裡統一跳脫
    /// 網址中的雙引號成 Excel 公式語法認得的 <c>""</c>，並讓 8 個呼叫點共用同一份邏輯，
    /// 之後如果還要調整公式組法，只需要改這一個地方。</para>
    /// </summary>
    /// <param name="url">字串，圖片網址</param>
    /// <returns>字串，例如 <c>IMAGE("https://...")</c></returns>
    private static string BuildImageFormula(string url)
    {
        return $"IMAGE(\"{url.Replace("\"", "\"\"")}\")";
    }

    /// <summary>
    /// 通知 LVLiveChatList 重繪指定的既有列。
    /// <para>VirtualMode 下修改 <see cref="ListViewItem"/> 的 SubItems／顏色／字型不會自動觸發重繪
    /// （非 VirtualMode 才會），就地更新既有列的內容之後都要呼叫這個方法，否則畫面要等使用者
    /// 捲動或縮放視窗才會反映最新內容。</para>
    /// </summary>
    /// <param name="lvItem">ListViewItem</param>
    private void RedrawListViewItem(ListViewItem lvItem)
    {
        int index = SharedListViewItems.IndexOf(lvItem);

        if (index >= 0)
        {
            LVLiveChatList.RedrawItems(index, index, false);
        }
    }

    /// <summary>
    /// 使用 Text-To-Speech 說話
    /// </summary>
    /// <param name="listView">ListView</param>
    [SupportedOSPlatform("windows7.0")]
    public void TtsSpeak(ListView listView)
    {
        if (OperatingSystem.IsWindows())
        {
            listView.InvokeIfRequired(() =>
            {
                // VirtualMode 下 listView.SelectedItems 禁止存取，改用 SelectedIndices 為底的
                // GetSelectedListViewItems()（ListViewExtension.cs）取代，兩種模式都適用。
                // 點擊 ListView 空白處（沒有任何列）不會有任何選取項目，這裡務必略過，
                // 不能直接假設一定有選取——原本的 selectedItems[^1] 在這種情況下也會拋例外，
                // 這裡順手補上防呆，而不是延續同樣的風險。
                ListViewItem? listViewItem = listView.GetSelectedListViewItems().LastOrDefault();

                if (listViewItem == null)
                {
                    return;
                }

                string type = listViewItem.SubItems[5].Text;
                string authorName = listViewItem.SubItems[0].Text;
                string message = listViewItem.SubItems[2].Text;

                string speakText = string.Empty;

                if (type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatGeneral) ||
                    type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatSuperChat) ||
                    type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatSuperSticker))
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
    /// <param name="listAllData">List&lt;ListViewItem&gt;</param>
    /// <param name="saveFileDialog">SaveFileDialog</param>
    /// <param name="videoID">字串，影片的 ID 值</param>
    /// <returns>Task</returns>
    public Task DoExportTask(
        ListView listView,
        List<ListViewItem> listAllData,
        SaveFileDialog saveFileDialog,
        string videoID)
    {
        return Task.Run(async () =>
        {
            using Stream stream = saveFileDialog.OpenFile();

            ListView.ColumnHeaderCollection columnHeaderCollection = LVLiveChatList.Columns;

            ExcelPackage.License.SetNonCommercialOrganization(StringSet.NonCommercialOrganization);

            using ExcelPackage package = new();

            double[] widthSet = [5.0, 20.0, 24.0, 50.0, 14.0, 27.0, 16.0, 20.0, 20.0, 20.0, 20.0, 20.0, 0.0, 14.0, 10.0, 0.0, 0.0];

            ExcelWorkbook workbook = package.Workbook;
            ExcelWorksheet worksheet1 = workbook.Worksheets.Add(StringSet.SheetName1);

            worksheet1.DefaultRowHeight = 28;

            // 欄位寬度設定。
            for (int i = 0; i < widthSet.Length; i++)
            {
                worksheet1.Column(i + 1).Width = widthSet[i];
            }

            #region 建置風格

            ExcelNamedStyleXml headerStyle = workbook.Styles.CreateNamedStyle("HeaderStyle");

            headerStyle.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            headerStyle.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            headerStyle.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            headerStyle.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            headerStyle.Style.Font.Name = "微軟正黑體";
            headerStyle.Style.Font.Bold = true;
            headerStyle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerStyle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            headerStyle.Style.WrapText = false;

            ExcelNamedStyleXml contentStyle = workbook.Styles.CreateNamedStyle("ContentStyle");

            contentStyle.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            contentStyle.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            contentStyle.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            contentStyle.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            contentStyle.Style.Font.Name = "微軟正黑體";
            contentStyle.Style.Font.Bold = false;
            contentStyle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            contentStyle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            contentStyle.Style.WrapText = false;

            #endregion

            #region 建置標題

            ExcelRange headerFirstRange = worksheet1.Cells[1, 1];

            headerFirstRange.StyleName = "HeaderStyle";
            headerFirstRange.Value = "頭像";
            headerFirstRange.Style.Fill.SetBackground(Color.BlanchedAlmond);

            for (int i = 0; i < columnHeaderCollection.Count; i++)
            {
                ColumnHeader header = columnHeaderCollection[i];

                ExcelRange range = worksheet1.Cells[1, i + 2];

                range.StyleName = "HeaderStyle";
                range.Style.Fill.SetBackground(Color.BlanchedAlmond);
                range.Value = header.Text;
            }

            // 設定篩選。
            worksheet1.Cells[1, 2, 1, 10].AutoFilter = true;

            #endregion

            #region 建置內容

            int startIdx1 = 2;

            // 2026/8 補上刪除／封鎖／回覆數更新／投票結果更新這四種類型的排除：正常情況下這幾種事件
            // 現在會在 DoProcessMessages 就地處理掉，不會再變成 ListView 裡的獨立列可以匯出；
            // 這裡補上排除純粹是防禦性處理，避免舊版（修正前）留下的資料被匯出。
            IEnumerable<ListViewItem> dataSet = listAllData
                .Where(n => n.SubItems[5].Text != StringSet.AppName &&
                    n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.YouTube &&
                    !ChatStatsCalculator.NonMessageEventExclusionKeys
                        .Select(SharedYTJsonParser.GetLocalizeString)
                        .Contains(n.SubItems[5].Text));

            // 同一位使用者在同一場直播常常留言好幾十則，頭像網址完全相同；每一列都各自寫一次
            // IMAGE() 公式，Excel 開啟時就會對同一張圖片重複發送好幾十次請求。這裡記錄「這個網址
            // 第一次出現時寫在哪一格」，同一個網址第二次以後只用儲存格參照公式（例如 "A5"）指回
            // 那一格，讓 Excel 重複使用同一次計算結果，不會為了同一張圖片重複對外請求。
            Dictionary<string, string> firstImageCellAddressByUrl = new(StringComparer.Ordinal);

            foreach (ListViewItem listViewItem in dataSet)
            {
                ExcelRange firstRange = worksheet1.Cells[startIdx1, 1];

                firstRange.StyleName = "ContentStyle";
                firstRange.Value = string.Empty;
                firstRange.Style.Fill.SetBackground(listViewItem.BackColor);

                if (CBExportAuthorPhoto.Checked)
                {
                    string authorPhotoUrl = listViewItem.SubItems[9].Text;

                    if (!string.IsNullOrEmpty(authorPhotoUrl))
                    {
                        if (firstImageCellAddressByUrl.TryGetValue(authorPhotoUrl, out string? firstCellAddress))
                        {
                            firstRange.Formula = firstCellAddress;
                        }
                        else
                        {
                            firstRange.Formula = BuildImageFormula(authorPhotoUrl);

                            firstImageCellAddressByUrl[authorPhotoUrl] = firstRange.Address;
                        }
                    }
                }

                for (int j = 0; j < listViewItem.SubItems.Count; j++)
                {
                    ListViewItem.ListViewSubItem listViewSubItem = listViewItem.SubItems[j];

                    ExcelRange excelRange = worksheet1.Cells[startIdx1, j + 2];

                    excelRange.StyleName = "ContentStyle";
                    excelRange.Value = listViewSubItem.Text;
                    excelRange.Style.Font.Color.SetColor(listViewItem.SubItems[j].ForeColor);
                    excelRange.Style.Fill.SetBackground(listViewItem.BackColor);

                    // 2026/8 修正：原本不分欄位一律 WrapText = true，實測發現這會讓下方新增的 AutoFit
                    // 完全失效——EPPlus 對 WrapText = true 的儲存格不計入自動寬度計算（用一支獨立的
                    // 測試專案分別驗證過 Column.AutoFit() 與 ExcelRange.AutoFitColumns() 兩種呼叫方式，
                    // 兩者都有這個限制），導致所有欄位的寬度永遠停在 widthSet 的初始值，即使標題或內容
                    // 明顯需要更寬也不會跟著調整。只有「訊息」（j == 2）是長度變化很大的自由輸入文字，
                    // 真的需要 WrapText 讓長訊息換行顯示；其餘都是長度相對固定的單一值欄位，改成讓
                    // AutoFit 依實際內容決定寬度，不要整批套用 WrapText。
                    if (j == 2)
                    {
                        excelRange.Style.WrapText = true;
                    }

                    if (j == 9)
                    {
                        // 2026/9 修正：比照 FMain.CommunityPostsExport.cs 既有的做法，先用
                        // Uri.IsWellFormedUriString 檢查過才建構 Uri——new Uri(text, UriKind.Absolute)
                        // 的建構子對非「well-formed」的字串（例如網址中含未編碼的空白）會丟
                        // UriFormatException，成千上萬列資料中只要有一筆網址格式異常，就會讓迴圈
                        // 中斷、整份匯出直接失敗，而不是那一格沒有超連結、其餘正常匯出。
                        if (!string.IsNullOrEmpty(listViewSubItem.Text) &&
                            Uri.IsWellFormedUriString(listViewSubItem.Text, UriKind.Absolute))
                        {
                            excelRange.Hyperlink = new Uri(listViewSubItem.Text, UriKind.Absolute);
                        }
                    }
                }

                startIdx1++;
            }

            // 只有 LVLiveChatList 可以匯出統計資訊，
            // 因為統計資訊的資料不是從 Excel 的內容直接產生的。
            if (listView.Name == LVLiveChatList.Name)
            {
                #region 統計資訊

                int summaryIdx = 1;

                ExcelRange summaryHeaderRange = worksheet1.Cells[summaryIdx, 15, summaryIdx, 16];

                summaryHeaderRange.Merge = true;
                summaryHeaderRange.StyleName = "HeaderStyle";
                summaryHeaderRange.Style.Fill.SetBackground(Color.BlanchedAlmond);
                summaryHeaderRange.Value = "統計資訊";

                summaryIdx++;

                // 「留言數量」刻意用排除法（跟 ChatStatsCalculator.Classify 的 CountsAsChatMessage 邏輯一致，
                // 且直接共用同一份 ChatMessageExclusionKeys 清單，不是各自維護一份），不是列舉「一般留言／
                // 超級留言／超級貼圖」這種包含法——包含法只要之後又新增一種聊天類訊息（例如捐款、版主訊息、
                // 投票建立），忘記同步更新這裡就會被漏算，這是這次修正前的實際狀況：舊版公式只認得上述三種
                // 類型，ChatPoll／ChatGift／ChatDonation／ChatModeration 這些同樣算「留言」的類型會被排除法
                // 算進 SharedChatCount（畫面上的即時數字），卻不會被這個公式算進去，造成匯出的 Excel 跟畫面上
                // 的數字對不起來。改成跟 ChatStatsCalculator 共用同一份排除清單後，兩邊不可能再各自漂移。
                string chatCountExclusionList = string.Join(",",
                    new[] { Rubujo.YouTube.Utility.Sets.StringSet.YouTube }
                        .Concat(ChatStatsCalculator.ChatMessageExclusionKeys.Select(SharedYTJsonParser.GetLocalizeString))
                        .Select(n => $"\"{n}\""));

                List<string> arrayFormula =
                [
                    $"(COUNTA(G2:G1048576)-SUM(COUNTIF(G2:G1048576,{{{chatCountExclusionList}}})))&\" 個\"",
                    $"COUNTIF(G:G,\"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatSuperChat)}\")&\" 個\"",
                    $"COUNTIF(G:G,\"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatSuperSticker)}\")&\" 個\"",
                    $"COUNTIF(G:G,\"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatJoinMember)}\")&\" 個\"",
                    $"COUNTIF(G:G,\"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatMemberUpgrade)}\")&\" 個\"",
                    $"COUNTIF(G:G,\"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatMemberMilestone)}\")&\" 個\"",
                    $"COUNTIF(G:G,\"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatMemberGift)}\")&\" 個\"",
                    $"COUNTIF(G:G,\"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatReceivedMemberGift)}\")&\" 個\""
                ];

                char[] separators1 = ['、'];

                string[] tpLMemberJoinCounts = SharedTooltip?.GetToolTip(LMemberJoinCount)
                    ?.Split(separators1, StringSplitOptions.RemoveEmptyEntries) ?? [];

                List<string> arraySummaryInfo =
                [
                    LChatCount.Text,
                    LSuperChatCount.Text,
                    LSuperStickerCount.Text,
                    LMemberJoinCount.Text,
                    .. tpLMemberJoinCounts,
                    .. new List<string>
                    {
                        LMemberInRoomCount.Text,
                        LAuthorCount.Text,
                        LTempIncome.Text
                    }
                ];

                string tpLTempIncome = SharedTooltip?.GetToolTip(LTempIncome) ?? string.Empty;

                if (!string.IsNullOrEmpty(tpLTempIncome))
                {
                    arraySummaryInfo.Add(tpLTempIncome);
                }

                // 設定預設寬度。
                worksheet1.Column(15).Width = 15.0;

                char[] separators2 = ['：'];

                for (int i = 0; i < arraySummaryInfo.Count; i++)
                {
                    string[] arrayInfo = arraySummaryInfo[i].Split(separators2,
                        StringSplitOptions.RemoveEmptyEntries);

                    ExcelRange summaryTitleRange = worksheet1.Cells[summaryIdx, 15];

                    summaryTitleRange.StyleName = "HeaderStyle";
                    summaryTitleRange.Style.Font.Bold = false;
                    summaryTitleRange.Value = arrayInfo[0];
                    // 2022/5/30 改為使用固定寬度。
                    //summaryTitleRange.AutoFitColumns();

                    ExcelRange summaryContentRange = worksheet1.Cells[summaryIdx, 16];

                    summaryContentRange.StyleName = "ContentStyle";
                    summaryContentRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    if (i <= arrayFormula.Count - 1)
                    {
                        summaryContentRange.Formula = arrayFormula[i];
                    }
                    else
                    {
                        summaryContentRange.Value = arrayInfo[1];
                    }

                    summaryContentRange.AutoFitColumns();

                    summaryIdx++;
                }


                #endregion
            }

            // 2026/8 修正：這張分頁原本只在寫入內容前套用 widthSet 的固定寬度，寫完內容後完全沒有
            // 依實際內容／標題文字自動調整，跟 worksheet2～worksheet5（時間熱點／自定義表情符號／會員
            // 徽章／超級貼圖）寫完內容後都會呼叫 AutoFit() 的既有慣例不一致——例如「頭像網址」
            // （AuthorPhotoUrl）／「外部頻道 ID」欄位遇到較長的網址／ID 時，固定寬度沒有跟著調整。
            // 這裡補上 AutoFit，但刻意排除三種欄位：
            // (1) 第 1 欄「頭像」是 IMAGE() 公式欄，AutoFit 會依公式文字長度而非圖片視覺寬度計算，數字沒有意義；
            // (2) 第 15 欄（回覆數）同時也是右側「統計資訊」區塊的標籤欄，2022/5/30 已經刻意改回固定寬度
            //     （見上方 summaryTitleRange 旁的註解），對這欄重新套用 AutoFit 會跟那次的決定衝突；
            // (3) widthSet 內原本就設為 0 的欄位（訊息 ID 值／標頭背景顏色／回覆數關聯鍵值）是刻意隱藏的
            //     技術欄位，不應該被 AutoFit 撐開變成可見欄位。
            // 用 AutoFit(最小寬度, 最大寬度) 而不是無上限的 AutoFit()：「頭像網址」這類自由輸入文字欄位，
            // 只要整批資料裡出現一筆異常長的內容（例如含追蹤參數的長網址），無上限的 AutoFit 會讓那一欄
            // 被單一離群值撐到不合理的寬度。下限用原本 widthSet 設定的寬度（不會比修正前更窄），
            // 上限抓 60 字元寬度。
            //
            // 這個 AutoFit 之所以能生效，前提是上面內容迴圈裡已經把 WrapText = true 限縮成只有
            // 「訊息」欄位才設定——用一支獨立的測試專案實測驗證過：EPPlus 的 Column.AutoFit() 與
            // ExcelRange.AutoFitColumns() 兩種呼叫方式，只要目標儲存格的 WrapText 為 true，
            // 就完全不會被納入自動寬度計算，欄寬永遠停在呼叫前的值。這應該就是 2022/5/30 那次改回固定
            // 寬度的真正原因（當時很可能是在全部欄位都設 WrapText = true 的狀態下試過 AutoFit，
            // 因為沒有效果才放棄），而不是 AutoFit 本身不適合這張分頁；worksheet2～worksheet5
            // 的內容從來沒有設定過 WrapText，AutoFit 才會一直正常運作。
            for (int i = 2; i <= widthSet.Length; i++)
            {
                if (i == 15)
                {
                    continue;
                }

                if (widthSet[i - 1] > 0)
                {
                    worksheet1.Column(i).AutoFit(widthSet[i - 1], 60.0);
                }
            }

            // 2026/8 修正：這裡原本會呼叫 worksheet1.Calculate(...)。EPPlus 官方文件證實 Calculate()
            // 對含有 IMAGE() 公式的儲存格，會由 EPPlus 自己發送 HTTP 請求把圖片下載下來、內嵌成真正的
            // 圖片物件寫進檔案（不是單純把公式字串留給 Excel 自己評估）。實測匯出一份含數百則訊息、
            // 每則都有頭像 IMAGE() 公式的檔案，EPPlus 自己的批次下載機制大量逾時／被 Google CDN 限流，
            // 超過一半的頭像最終變成 #VALUE! 錯誤，而不是正確顯示圖片；EPPlus 官方文件對這種規模的
            // 批次下載沒有任何說明或建議上限，屬於未處理的失敗模式，不是已知限制。
            // Calculate() 對 IMAGE() 公式而言純粹是 EPPlus 自己「順便先算好、內嵌預覽」的選用功能，
            // 不呼叫也完全不影響檔案本身的正確性——workbook.xml 已經設定 fullCalcOnLoad="1"，
            // 使用者用真正的 Excel（365，具備雲端連線能力）開啟檔案時，Excel 自己就會正確重新計算並
            // 顯示所有公式（含 IMAGE()），用的是遠比 EPPlus 自製下載器更可靠的官方雲端基礎設施，
            // 也完全不會把圖片內嵌進 EPPlus 產生的檔案裡（避免檔案肥大，符合當初改用 IMAGE() 公式
            // 而不是直接內嵌圖片的初衷）。因此這裡刻意不再呼叫 Calculate()，讓 IMAGE() 公式維持原始
            // 未計算狀態，交給使用者端的 Excel 自己處理。

            #endregion

            #region 時間熱點

            // 參考 1：https://stackoverflow.com/a/687347
            // 參考 2：https://stackoverflow.com/a/687370

            // 排除在影片開始前的時間點。
            Dictionary<string, int> sourceList = listAllData
                .Where(n => n.SubItems[5].Text != StringSet.AppName &&
                    n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.YouTube &&
                    // 2022/10/25 因不容易轉換成影片對應時間點，故而直接排除
                    // LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberGift)、
                    // LiveChatCatcher.GetLocalizeString(KeySet.ChatReceivedMemberGift) 等類型的資料，
                    // 以免在時間熱點活頁簿內出現奇怪的時間點。
                    n.SubItems[5].Text != SharedYTJsonParser.GetLocalizeString(KeySet.ChatMemberGift) &&
                    n.SubItems[5].Text != SharedYTJsonParser.GetLocalizeString(KeySet.ChatReceivedMemberGift) &&
                    // 2026/9 修正：刪除／封鎖／回覆數更新／投票結果更新的排除改用共用清單
                    // ChatStatsCalculator.NonMessageEventExclusionKeys（理由同上方的內容分頁排除），
                    // 避免這裡跟內容分頁的排除清單各自維護、日後新增同類型事件時漏改其中一處。
                    !ChatStatsCalculator.NonMessageEventExclusionKeys
                        .Select(SharedYTJsonParser.GetLocalizeString)
                        .Contains(n.SubItems[5].Text) &&
                    !string.IsNullOrEmpty(n.SubItems[8].Text) &&
                    !n.SubItems[8].Text.Contains('-'))
                .Select(n => n.SubItems[8].Text.Length > 3 ?
                    n.SubItems[8].Text[0..^3] :
                    n.SubItems[8].Text)
                .GroupBy(n => n)
                .Select(n => new { Timestamp = n.Key, Count = n.Count() })
                .ToDictionary(n => n.Timestamp, n => n.Count);

            if (sourceList.Count > 0)
            {
                bool isStreaming = await SharedYTJsonParser.IsVideoStreamingAsync(videoID);

                string sheetName = isStreaming ?
                    StringSet.SheetName2 :
                    StringSet.SheetName3;

                ExcelWorksheet worksheet2 = workbook.Worksheets.Add(sheetName);

                #region 建置標題

                ExcelRange headerFirstRange2 = worksheet2.Cells[1, 1];

                headerFirstRange2.StyleName = "HeaderStyle";
                headerFirstRange2.Value = "影片的分鐘值";
                headerFirstRange2.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange3 = worksheet2.Cells[1, 2];

                headerFirstRange3.StyleName = "HeaderStyle";
                headerFirstRange3.Value = "留言數";
                headerFirstRange3.Style.Fill.SetBackground(Color.BlanchedAlmond);

                #endregion

                int startIdx2 = 2;

                foreach (KeyValuePair<string, int> item in sourceList)
                {
                    ExcelRange range1 = worksheet2.Cells[startIdx2, 1];

                    range1.StyleName = "ContentStyle";
                    range1.Value = item.Key;

                    ExcelRange range2 = worksheet2.Cells[startIdx2, 2];

                    range2.StyleName = "ContentStyle";
                    range2.Value = item.Value;

                    startIdx2++;
                }

                // 2026/9 修正：終止條件原本用 worksheet2.Cells.Count()（反映整張工作表已使用的
                // 儲存格數），但迴圈實際只處理 widthSet.Length 以內的欄位（內層 if 已經證明這點），
                // 資料量大時這個終止條件會讓迴圈跑動次數遠超過「只是要跑十幾欄」所需的規模。
                // 直接把迴圈上限換成 widthSet.Length，語意不變，不再需要內層判斷。
                for (int i = 1; i < widthSet.Length; i++)
                {
                    ExcelColumn column = worksheet2.Column(i);

                    column.AutoFit();
                }

                ExcelLineChart excelLineChart = worksheet2
                    .Drawings
                    .AddLineChart("LineChart", eLineChartType.Line);

                excelLineChart.StyleManager.SetChartStyle(ePresetChartStyle.LineChartStyle1);

                excelLineChart.Legend.Font.ComplexFont = "微軟正黑體";
                excelLineChart.Legend.Font.EastAsianFont = "微軟正黑體";
                excelLineChart.Legend.Font.LatinFont = "微軟正黑體";

                excelLineChart.Title.Text = sheetName;
                excelLineChart.Title.Font.ComplexFont = "微軟正黑體";
                excelLineChart.Title.Font.EastAsianFont = "微軟正黑體";
                excelLineChart.Title.Font.LatinFont = "微軟正黑體";

                excelLineChart.XAxis.Title.Text = worksheet2.Cells[1, 1].Text;
                excelLineChart.XAxis.Title.Font.ComplexFont = "微軟正黑體";
                excelLineChart.XAxis.Title.Font.EastAsianFont = "微軟正黑體";
                excelLineChart.XAxis.Title.Font.LatinFont = "微軟正黑體";
                excelLineChart.XAxis.Font.ComplexFont = "微軟正黑體";
                excelLineChart.XAxis.Font.EastAsianFont = "微軟正黑體";
                excelLineChart.XAxis.Font.LatinFont = "微軟正黑體";
                // 2021/12/11 還沒找到 EPPlus 調整標籤間距的方法，故使用 eTextVerticalType.Vertical。
                excelLineChart.XAxis.TextBody.VerticalText = eTextVerticalType.Vertical;

                excelLineChart.YAxis.Title.Text = worksheet2.Cells[1, 2].Text;
                excelLineChart.YAxis.Title.TextVertical = eTextVerticalType.EastAsianVertical;
                excelLineChart.YAxis.Title.Font.ComplexFont = "微軟正黑體";
                excelLineChart.YAxis.Title.Font.EastAsianFont = "微軟正黑體";
                excelLineChart.YAxis.Title.Font.LatinFont = "微軟正黑體";
                excelLineChart.YAxis.Font.ComplexFont = "微軟正黑體";
                excelLineChart.YAxis.Font.EastAsianFont = "微軟正黑體";
                excelLineChart.YAxis.Font.LatinFont = "微軟正黑體";

                excelLineChart.SetPosition(1, 0, 3, 0);

                int lastRowIdx = sourceList.Count + 1;

                ExcelChartSerie excelChartSerie = excelLineChart.Series.Add(
                    // Y 軸。
                    worksheet2.Cells[2, 2, lastRowIdx, 2],
                    // X 軸。
                    worksheet2.Cells[2, 1, lastRowIdx, 1]);

                excelChartSerie.Header = StringSet.SheetName1;
            }

            #endregion

            #region 自定義表情符號

            if (SharedCustomEmojis.Any(n => n.IsCustomEmoji))
            {
                string sheetName = StringSet.SheetName4;

                ExcelWorksheet worksheet3 = workbook.Worksheets.Add(sheetName);

                worksheet3.DefaultRowHeight = 28;
                worksheet3.Column(1).Width = 5.0;

                #region 建置標題

                ExcelRange headerFirstRange4 = worksheet3.Cells[1, 1];

                headerFirstRange4.StyleName = "HeaderStyle";
                headerFirstRange4.Value = "影像";
                headerFirstRange4.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange5 = worksheet3.Cells[1, 2];

                headerFirstRange5.StyleName = "HeaderStyle";
                headerFirstRange5.Value = "文字";
                headerFirstRange5.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange6 = worksheet3.Cells[1, 3];

                headerFirstRange6.StyleName = "HeaderStyle";
                headerFirstRange6.Value = "格式";
                headerFirstRange6.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange7 = worksheet3.Cells[1, 4];

                headerFirstRange7.StyleName = "HeaderStyle";
                headerFirstRange7.Value = "ID 值";
                headerFirstRange7.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange8 = worksheet3.Cells[1, 5];

                headerFirstRange8.StyleName = "HeaderStyle";
                headerFirstRange8.Value = "網址";
                headerFirstRange8.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange9 = worksheet3.Cells[1, 6];

                headerFirstRange9.StyleName = "HeaderStyle";
                headerFirstRange9.Value = "標籤";
                headerFirstRange9.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange10 = worksheet3.Cells[1, 7];

                headerFirstRange10.StyleName = "HeaderStyle";
                headerFirstRange10.Value = "自定義表情符號";
                headerFirstRange10.Style.Fill.SetBackground(Color.BlanchedAlmond);

                #endregion

                int startIdx2 = 2;

                foreach (EmojiData emojiData in SharedCustomEmojis.Where(n => n.IsCustomEmoji))
                {
                    ExcelRange range2 = worksheet3.Cells[startIdx2, 1];

                    range2.StyleName = "ContentStyle";

                    if (!string.IsNullOrEmpty(emojiData.Url))
                    {
                        range2.Formula = BuildImageFormula(emojiData.Url);
                    }

                    ExcelRange range3 = worksheet3.Cells[startIdx2, 2];

                    range3.StyleName = "ContentStyle";
                    range3.Value = emojiData.Text;

                    ExcelRange range4 = worksheet3.Cells[startIdx2, 3];

                    range4.StyleName = "ContentStyle";
                    range4.Value = emojiData.Format;

                    ExcelRange range5 = worksheet3.Cells[startIdx2, 4];

                    range5.StyleName = "ContentStyle";
                    range5.Value = emojiData.ID;

                    ExcelRange range6 = worksheet3.Cells[startIdx2, 5];

                    range6.StyleName = "ContentStyle";
                    range6.Value = emojiData.Url;

                    if (!string.IsNullOrEmpty(emojiData.Url) &&
                        Uri.IsWellFormedUriString(emojiData.Url, UriKind.Absolute))
                    {
                        range6.Hyperlink = new Uri(emojiData.Url, UriKind.Absolute);
                    }

                    ExcelRange range7 = worksheet3.Cells[startIdx2, 6];

                    range7.StyleName = "ContentStyle";
                    range7.Value = emojiData.Label;

                    ExcelRange range8 = worksheet3.Cells[startIdx2, 7];

                    range8.StyleName = "ContentStyle";
                    range8.Value = emojiData.IsCustomEmoji;

                    startIdx2++;
                }

                // 理由同 worksheet2 上方的說明：終止條件改用 widthSet.Length，不再用
                // worksheet3.Cells.Count()，語意不變。
                for (int i = 2; i < widthSet.Length; i++)
                {
                    ExcelColumn column = worksheet3.Column(i);

                    column.AutoFit();
                }

                // 2026/8 修正：刻意不呼叫 worksheet3.Calculate(...)，理由同 worksheet1 上方的詳細說明——
                // EPPlus 自己對 IMAGE() 公式的批次下載機制不可靠，交給使用者端的 Excel 自己計算即可。
            }

            #endregion

            #region 會員徽章

            if (SharedBadges.Any(n => n.Label != null && n.Label.Contains(StringSet.Member)))
            {
                string sheetName = StringSet.SheetName5;

                ExcelWorksheet worksheet4 = workbook.Worksheets.Add(sheetName);

                worksheet4.DefaultRowHeight = 28;
                worksheet4.Column(1).Width = 5.0;

                #region 建置標題

                ExcelRange headerFirstRange11 = worksheet4.Cells[1, 1];

                headerFirstRange11.StyleName = "HeaderStyle";
                headerFirstRange11.Value = "影像";
                headerFirstRange11.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange12 = worksheet4.Cells[1, 2];

                headerFirstRange12.StyleName = "HeaderStyle";
                headerFirstRange12.Value = "標籤";
                headerFirstRange12.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange13 = worksheet4.Cells[1, 3];

                headerFirstRange13.StyleName = "HeaderStyle";
                headerFirstRange13.Value = "格式";
                headerFirstRange13.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange14 = worksheet4.Cells[1, 4];

                headerFirstRange14.StyleName = "HeaderStyle";
                headerFirstRange14.Value = "工具提示";
                headerFirstRange14.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange15 = worksheet4.Cells[1, 5];

                headerFirstRange15.StyleName = "HeaderStyle";
                headerFirstRange15.Value = "網址";
                headerFirstRange15.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange16 = worksheet4.Cells[1, 6];

                headerFirstRange16.StyleName = "HeaderStyle";
                headerFirstRange16.Value = "圖示類型";
                headerFirstRange16.Style.Fill.SetBackground(Color.BlanchedAlmond);

                #endregion

                int startIdx3 = 2;

                foreach (BadgeData badgeData in SharedBadges.Where(n => n.Label != null && n.Label.Contains(StringSet.Member)))
                {
                    ExcelRange range9 = worksheet4.Cells[startIdx3, 1];

                    range9.StyleName = "ContentStyle";
                    
                    if (!string.IsNullOrEmpty(badgeData.Url))
                    {
                        range9.Formula = BuildImageFormula(badgeData.Url);
                    }

                    ExcelRange range10 = worksheet4.Cells[startIdx3, 2];

                    range10.StyleName = "ContentStyle";
                    range10.Value = badgeData.Label;

                    ExcelRange range11 = worksheet4.Cells[startIdx3, 3];

                    range11.StyleName = "ContentStyle";
                    range11.Value = badgeData.Format;

                    ExcelRange range12 = worksheet4.Cells[startIdx3, 4];

                    range12.StyleName = "ContentStyle";
                    range12.Value = badgeData.Tooltip;

                    ExcelRange range13 = worksheet4.Cells[startIdx3, 5];

                    range13.StyleName = "ContentStyle";
                    range13.Value = badgeData.Url;

                    if (!string.IsNullOrEmpty(badgeData.Url) &&
                        Uri.IsWellFormedUriString(badgeData.Url, UriKind.Absolute))
                    {
                        range13.Hyperlink = new Uri(badgeData.Url, UriKind.Absolute);
                    }

                    ExcelRange range14 = worksheet4.Cells[startIdx3, 6];

                    range14.StyleName = "ContentStyle";
                    range14.Value = badgeData.IconType;

                    startIdx3++;
                }

                // 理由同 worksheet2 上方的說明：終止條件改用 widthSet.Length，不再用
                // worksheet4.Cells.Count()，語意不變。
                for (int i = 2; i < widthSet.Length; i++)
                {
                    ExcelColumn column = worksheet4.Column(i);

                    column.AutoFit();
                }

                // 2026/8 修正：刻意不呼叫 worksheet4.Calculate(...)，理由同 worksheet1 上方的詳細說明——
                // EPPlus 自己對 IMAGE() 公式的批次下載機制不可靠，交給使用者端的 Excel 自己計算即可。
            }

            #endregion

            #region 超級貼圖

            if (SharedStickers.Count != 0)
            {
                string sheetName = StringSet.SheetName6;

                ExcelWorksheet worksheet5 = workbook.Worksheets.Add(sheetName);

                worksheet5.DefaultRowHeight = 28;
                // 5:1.12cm -> 1:0.224cm
                worksheet5.Column(1).Width = 5.0;

                #region 建置標題

                ExcelRange headerFirstRange17 = worksheet5.Cells[1, 1];

                headerFirstRange17.StyleName = "HeaderStyle";
                headerFirstRange17.Value = "影像";
                headerFirstRange17.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange18 = worksheet5.Cells[1, 2];

                headerFirstRange18.StyleName = "HeaderStyle";
                headerFirstRange18.Value = "標籤";
                headerFirstRange18.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange19 = worksheet5.Cells[1, 3];

                headerFirstRange19.StyleName = "HeaderStyle";
                headerFirstRange19.Value = "格式";
                headerFirstRange19.Style.Fill.SetBackground(Color.BlanchedAlmond);

                ExcelRange headerFirstRange20 = worksheet5.Cells[1, 4];

                headerFirstRange20.StyleName = "HeaderStyle";
                headerFirstRange20.Value = "網址";
                headerFirstRange20.Style.Fill.SetBackground(Color.BlanchedAlmond);

                #endregion

                int startIdx4 = 2;

                foreach (StickerData stickerData in SharedStickers)
                {
                    ExcelRange range15 = worksheet5.Cells[startIdx4, 1];

                    range15.StyleName = "ContentStyle";
                    
                    if (!string.IsNullOrEmpty(stickerData.Url))
                    {
                        range15.Formula = BuildImageFormula(stickerData.Url);
                    }

                    ExcelRange range10 = worksheet5.Cells[startIdx4, 2];

                    range10.StyleName = "ContentStyle";
                    range10.Value = stickerData.Label;

                    ExcelRange range11 = worksheet5.Cells[startIdx4, 3];

                    range11.StyleName = "ContentStyle";
                    range11.Value = stickerData.Format;

                    ExcelRange range12 = worksheet5.Cells[startIdx4, 4];

                    range12.StyleName = "ContentStyle";
                    range12.Value = stickerData.Url;

                    if (!string.IsNullOrEmpty(stickerData.Url) &&
                        Uri.IsWellFormedUriString(stickerData.Url, UriKind.Absolute))
                    {
                        range12.Hyperlink = new Uri(stickerData.Url, UriKind.Absolute);
                    }

                    startIdx4++;
                }

                // 理由同 worksheet2 上方的說明：終止條件改用 widthSet.Length，不再用
                // worksheet5.Cells.Count()，語意不變。
                for (int i = 2; i < widthSet.Length; i++)
                {
                    ExcelColumn column = worksheet5.Column(i);

                    column.AutoFit();
                }

                // 2026/8 修正：刻意不呼叫 worksheet5.Calculate(...)，理由同 worksheet1 上方的詳細說明——
                // EPPlus 自己對 IMAGE() 公式的批次下載機制不可靠，交給使用者端的 Excel 自己計算即可。
            }

            #endregion

            string version = CustomFunction.GetAppVersion(),
                fileTitle = Path.GetFileNameWithoutExtension(saveFileDialog.FileName),
                comments = string.Empty;

            if (!string.IsNullOrEmpty(videoID))
            {
                comments = $"https://www.youtube.com/watch?v={videoID}";
            }

            workbook.Properties.Title = fileTitle;
            workbook.Properties.Subject = comments;
            workbook.Properties.Category = StringSet.SheetName1;
            workbook.Properties.Keywords = $"{Rubujo.YouTube.Utility.Sets.StringSet.YouTube}, {StringSet.SheetName1}";
            workbook.Properties.Author = $"{StringSet.AppName} {version}";

            package.SaveAs(stream);

            // 只有完整匯出 LVLiveChatList（不是篩選後的搜尋結果子集）才代表這場擷取的原始資料已經
            // 安全落地，可以清除當機復原記錄；篩選子集匯出不代表使用者已經拿到完整資料的備份。
            if (listView.Name == LVLiveChatList.Name)
            {
                CaptureRecoveryStore.Clear();
                CaptureSessionStore.Clear();
                SharedCaptureSessionManifest = null;
                SharedResumeContinuation = null;
            }
        });
    }

    /// <summary>
    /// 複製至剪貼簿
    /// </summary>
    /// <param name="listView">ListView</param>
    public void CopyToClipboard(ListView listView)
    {
        listView.InvokeIfRequired(() =>
        {
            // VirtualMode 下 listView.SelectedItems 禁止存取，改用 SelectedIndices 為底的
            // GetSelectedListViewItems()（ListViewExtension.cs）取代，兩種模式都適用。
            IEnumerable<ListViewItem> selectedItems = listView.GetSelectedListViewItems();

            // 2026/9 修正：改用 StringBuilder 取代字串 += 逐次串接。VirtualMode 下
            // GetSelectedListViewItems() 理論上可以選取到全部列（例如 Ctrl+A），長時間直播累積
            // 數萬筆資料時，字串 += 每次都要配置一份新字串複本，對這麼多列全選後複製會有明顯延遲。
            StringBuilder copiedContentBuilder = new();

            foreach (ListViewItem listViewItem in selectedItems)
            {
                StringBuilder tempContentBuilder = new();

                int count = 0;

                foreach (ListViewItem.ListViewSubItem listViewSubItem in listViewItem.SubItems)
                {
                    string currentContent = listViewSubItem.Text;

                    tempContentBuilder.Append(currentContent);

                    if (count != listViewItem.SubItems.Count - 1)
                    {
                        if (!string.IsNullOrEmpty(currentContent))
                        {
                            tempContentBuilder.Append(StringSet.Splitter);
                        }
                    }

                    count++;
                }

                copiedContentBuilder.Append(tempContentBuilder).Append(Environment.NewLine);
            }

            string copiedContent = copiedContentBuilder.ToString();

            // 沒有選取任何列時 copiedContent 會是空字串——Clipboard.SetText 對空字串會直接拋
            // ArgumentException（跟 null 一樣不允許），這裡先擋掉，避免雙擊 ListView 空白處炸掉。
            if (string.IsNullOrEmpty(copiedContent))
            {
                return;
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
                if (focusedItem.SubItems.Count >= 11)
                {
                    string authorExternalChannelId = focusedItem.SubItems[10].Text;

                    if (!string.IsNullOrEmpty(authorExternalChannelId))
                    {
                        string channelUrl = YouTubeUrlUtil.GetYouTubeChannelUrl(authorExternalChannelId);

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
    /// 寫紀錄
    /// <para>2026/8 修正：原本是 async void 搭配 Task.Run + InvokeIfRequired，從 UI 執行緒呼叫時
    /// 會多繞一趟執行緒集區再繞回來，且 async void 本身若拋出例外無法被呼叫端 catch 到。
    /// InvokeIfRequired 本身就已經處理好「目前是否在 UI 執行緒上」的判斷（是的話直接執行，
    /// 不是的話才呼叫 Control.Invoke 切換），不需要外面再包一層 Task.Run。</para>
    /// </summary>
    /// <param name="message">字串，訊息內容</param>
    public void WriteLog(string message)
    {
        TBLog.InvokeIfRequired(() =>
        {
            TBLog.AppendText($"[{DateTime.Now}]：{message}{Environment.NewLine}");
        });
    }

    /// <summary>
    /// 取得 SharedLogger
    /// </summary>
    /// <returns>ILogger&lt;FMain&gt;</returns>
    public ILogger<FMain> GetSharedLogger()
    {
        return SharedLogger;
    }

    /// <summary>
    /// 取得 SharedYTJsonParser
    /// </summary>
    /// <returns>YTJsonParser</returns>
    public YTJsonParser GetSharedYTJsonParser()
    {
        return SharedYTJsonParser;
    }

    /// <summary>
    /// 取得 SharedListViewItems
    /// <para>LVLiveChatList 是 VirtualMode，Items 集合禁止存取，FSearch 要讀取完整聊天記錄
    /// 只能透過這個方法，不能再用 <c>LVLiveChatList.GetListViewItems()</c>。</para>
    /// </summary>
    /// <returns>IReadOnlyList&lt;ListViewItem&gt;</returns>
    public IReadOnlyList<ListViewItem> GetSharedListViewItems() => [.. SharedListViewItems];

    public IReadOnlyList<RendererData> GetRawMessagesSnapshot() => [.. SharedRawRendererData];

    public CaptureSessionManifest? GetCaptureSessionSnapshot() => SharedCaptureSessionManifest;

    public IReadOnlyList<string> GetSanitizedRawResponsesSnapshot() => [.. SharedSanitizedRawResponses];

    public decimal GetRevenueEstimateRate() => SharedRevenueEstimateRate;

    public void SetRevenueEstimateRate(decimal rate)
    {
        SharedRevenueEstimateRate = Math.Clamp(rate, 0m, 1m);
        RevenueEstimateSettings.SaveRate(SharedRevenueEstimateRate);
        UpdateSummaryInfo();
    }

    public async Task<int> ImportRawMessagesAsync(IReadOnlyList<RendererData> messages)
    {
        IReadOnlyList<RendererData> newMessages = SharedCaptureMessageDeduplicator.FilterNew(messages);

        const int batchSize = 1_000;

        for (int offset = 0; offset < newMessages.Count; offset += batchSize)
        {
            int count = Math.Min(batchSize, newMessages.Count - offset);
            RendererData[] batch = new RendererData[count];

            for (int index = 0; index < count; index++)
            {
                batch[index] = newMessages[offset + index];
            }

            DoProcessMessages(batch, updateListView: false);
            await Task.Yield();
        }

        if (newMessages.Count > 0)
        {
            // 大量匯入期間只建立資料與索引，最後才一次更新 VirtualListSize、捲動與重繪。
            // 否則每一批都 EnsureVisible/Invalidate，會留下大量原生 ListView 繪製工作，讓匯入
            // 對話框已完成後主視窗仍長時間沒有回應。
            LVLiveChatList.BeginUpdate();
            AutoFitListViewColumns(LVLiveChatList, ListSamplingUtil.CreateEvenlySpaced(SharedListViewItems, 512));
            LVLiveChatList.VirtualListSize = SharedListViewItems.Count;
            LVLiveChatList.EndUpdate();
            LVLiveChatList.Invalidate();
            UpdateSummaryInfo();
        }

        return newMessages.Count;
    }

    public void ConfigureStreamingJsonLines(string? path)
    {
        SharedStreamingJsonlPath = path;
    }

    private void TryAppendStreamingJsonLines(IReadOnlyList<RendererData> messages)
    {
        if (string.IsNullOrWhiteSpace(SharedStreamingJsonlPath))
        {
            return;
        }

        try
        {
            ChatDataTools.AppendJsonLines(SharedStreamingJsonlPath, messages);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            SharedLogger.LogWarning(ex, "持續寫入 JSONL 失敗，已停止同步寫入。Path={Path}", SharedStreamingJsonlPath);
            SharedStreamingJsonlPath = null;
            WriteLog("持續寫入 JSONL 失敗，已停止同步寫入；聊天室擷取仍會繼續。");
        }
    }

    /// <summary>
    /// 取得 TBVideoID
    /// </summary>
    /// <returns>TextBox</returns>
    public TextBox GetTBVideoID()
    {
        return TBVideoID;
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
        SharedTooltip.SetToolTip(TBInterval, "顯示目前擷取聊天室內容的輪詢間隔秒數，由系統依 YouTube 回應自動調整，唯讀。");
        SharedTooltip.SetToolTip(TBUserAgent, "已提供預設值，一般情況下不需要修改。若擷取聊天室內容持續失敗，可嘗試點選右方「搜尋使用者代理字串」按鈕更新為較新的版本。");
        SharedTooltip.SetToolTip(TBSecChUa, "已提供預設值，一般情況下不需要修改，通常與上方的使用者代理字串搭配更新。");
        SharedTooltip.SetToolTip(BtnCookieLogin, "登入 YouTube 帳號後，可取得您已加入會員之頻道的會員專屬直播聊天室內容。一般公開直播不需要登入。");

        // 設定控制項的狀態。
        SetControlsState(true);
    }

    /// <summary>
    /// 執行處裡訊息
    /// </summary>
    /// <param name="messages">IReadOnlyList&lt;RendererData&gt;</param>
    private void DoProcessMessages(IReadOnlyList<RendererData> messages, bool updateListView = true)
    {
        try
        {
            SharedRawRendererData.AddRange(messages);
            List<ListViewItem> listTempItem = [];

            foreach (RendererData rendererData in messages)
            {
                if (rendererData.Stickers != null)
                {
                    foreach (StickerData stickerData in rendererData.Stickers)
                    {
                        string stickerKey = stickerData.ID ?? stickerData.Url ?? string.Empty;

                        if (string.IsNullOrEmpty(stickerKey) || SharedStickerKeys.Add(stickerKey))
                        {
                            // 2025/4/15 改用新的方式下載圖片。
                            //string errorMessage = await stickerData.SetImage(
                            //    SharedHttpClient,
                            //    SharedYTJsonParser.FetchLargePicture());

                            //if (!string.IsNullOrEmpty(errorMessage))
                            //{
                            //    WriteLog(errorMessage);
                            //}

                            SharedStickers.Add(stickerData);
                        }
                    }
                }

                if (rendererData.Emojis != null)
                {
                    foreach (EmojiData emojiData in rendererData.Emojis)
                    {
                        // 只處理自定義表情符號的資料。
                        if (emojiData.IsCustomEmoji &&
                            !string.IsNullOrEmpty(emojiData.ID) &&
                            SharedCustomEmojiIds.Add(emojiData.ID))
                        {
                            // 2025/4/15 改用新的方式下載圖片。
                            //string errorMessage = await emojiData.SetImage(
                            //    SharedHttpClient,
                            //    SharedYTJsonParser.FetchLargePicture());

                            //if (!string.IsNullOrEmpty(errorMessage))
                            //{
                            //    WriteLog(errorMessage);
                            //}

                            SharedCustomEmojis.Add(emojiData);
                        }
                    }
                }

                if (rendererData.Badges != null)
                {
                    foreach (BadgeData badgeData in rendererData.Badges)
                    {
                        // 只處理會員徽章的資料。
                        if (badgeData.Label != null &&
                            badgeData.Label.Contains(StringSet.Member) &&
                            SharedBadgeLabels.Add(badgeData.Label))
                        {
                            // 2025/4/15 改用新的方式下載圖片。
                            //string errorMessage = await badgeData.SetImage(
                            //    SharedHttpClient,
                            //    SharedYTJsonParser.FetchLargePicture());

                            //if (!string.IsNullOrEmpty(errorMessage))
                            //{
                            //    WriteLog(errorMessage);
                            //}

                            SharedBadges.Add(badgeData);
                        }
                    }
                }

                string id = rendererData.ID ?? string.Empty;
                string type = rendererData.Type ?? string.Empty;

                // 這幾種類型是「以 ID 關聯回既有列」的更新／刪除事件，本身不是新留言，
                // 找到對應列後就地標記／更新，然後繼續處理下一筆，不要再往下組成新的 ListViewItem。
                // （2026/8 修正：先前這幾種事件會被誤判成新留言，變成畫面上一列列的原始 ID／頻道 ID 字串垃圾列，
                // 且會虛灌「留言數量」／「留言人數」統計；Excel 匯出也會原封不動地把這些垃圾列匯出。）
                if (type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatMessageDeleted))
                {
                    ApplyMessageDeletedMarker(id);

                    continue;
                }

                if (type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatUserBanned))
                {
                    ApplyUserBannedMarker(rendererData.AuthorExternalChannelID);

                    continue;
                }

                if (type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatReplyCountUpdate))
                {
                    ApplyReplyCountUpdate(id, rendererData.ReplyCount);

                    continue;
                }

                if (type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatPollUpdate))
                {
                    ApplyPollResultUpdate(id, rendererData.MessageContent);

                    continue;
                }

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
                string leaderboardRank = rendererData.LeaderboardRank ?? string.Empty;
                string replyCount = rendererData.ReplyCount ?? string.Empty;
                string headerBackgroundColor = rendererData.HeaderBackgroundColor ?? string.Empty;
                string replyCountEntityKey = rendererData.ReplyCountEntityKey ?? string.Empty;

                if (string.IsNullOrEmpty(timestampText))
                {
                    // 改為使用發送訊息的時間。
                    if (DateTime.TryParse(timestampUsec, out DateTime dateTime))
                    {
                        timestampText = dateTime.ToString("HH:mm:ss");
                    }
                }

                // 優先以訊息 ID 判斷是否已存在（跨批次也能正確判斷），
                // 只有在沒有 ID 值可用時才退回舊版「作者名稱＋時間戳記」的判斷方式。
                //
                // 這個 id 已經存在時，不能直接略過：既有可能是真的重複資料（輪詢間隔重疊造成同一則訊息
                // 收到兩次），也有可能是 replaceChatItemAction（例如超級留言／貼圖淡出後改為較小樣式，
                // 內容與顏色都可能改變）——後者若整個略過，新的內容會被靜默丟棄；若當成新列加入，
                // 又會變成一列看起來重複的資料。兩種情況都應該「更新既有列」才對，因此改用就地更新
                // （對真正重複的資料而言，用相同的值覆寫一次是無害的）。
                if (!string.IsNullOrEmpty(id) &&
                    SharedItemsByMessageID.TryGetValue(id, out ListViewItem? existingItemForId))
                {
                    ApplyExistingListViewItemUpdate(
                        existingItemForId,
                        authorBages,
                        messageContent,
                        purchaseAmountText,
                        foregroundColor,
                        backgroundColor,
                        headerBackgroundColor,
                        leaderboardRank,
                        replyCount);

                    RedrawListViewItem(existingItemForId);

                    continue;
                }

                if (string.IsNullOrEmpty(id) &&
                    listTempItem.Any(n => n.Text == authorName && n.SubItems[4].Text == timestampUsec))
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
                    purchaseAmountText,
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
                    // 只變更「標頭」相關欄位的背景色（作者名稱／徽章／金額／時間），呈現跟真實 YouTube
                    // 超級留言一樣的雙色設計（標頭一色、內文另一色），訊息本文維持 backgroundColor。
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
                    // 2026/9 修正：原本用作者「顯示名稱」當頭像快取／ImageList 的 key，但顯示名稱不保證
                    // 唯一——只要同一批次或同一次執行期間出現兩個顯示名稱相同、實際是不同頻道的使用者，
                    // 後出現的那位會直接沿用前一位的頭像（imageUrl 參數被完全忽略），因為
                    // ImageList.ContainsKey(key)／BetterCacheManager 的快取都會命中。改用頻道 ID
                    // （YouTube 保證唯一，不會像顯示名稱一樣重複）當 key，找不到頻道 ID 時才退回顯示名稱
                    // （例如某些系統類型的訊息可能沒有頻道 ID）。
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

                                // 2026/8 修正（真正的根本原因）：VirtualMode 下透過 RetrieveVirtualItem 供應的
                                // 項目，用字串鍵值的 ImageKey 是已知不可靠的做法——即使 ImageList.Images 裡
                                // 確實有這個 key（用暫時性的診斷紀錄逐步確認過下載／加入 ImageList／
                                // SharedListViewItems 索引查找全部正確執行），圖示還是不會顯示，這是
                                // WinForms VirtualMode 的既有限制，跟資料是否正確、重繪時機是否正確都無關。
                                // 必須改用整數索引的 ImageIndex 才能正常運作。下載是非同步的，建立
                                // ListViewItem 當下還不知道這張圖片最後會落在 ImageList 的哪個索引，
                                // 要等這裡下載完成（或確認先前已經快取過）之後，用 IndexOfKey 查出實際
                                // 索引再指定；ImageIndex／ImageKey 兩者互斥，指定 ImageIndex 會自動清掉
                                // 先前可能設過的 ImageKey，不需要另外清除。
                                int imageIndex = LVLiveChatList.SmallImageList.Images.IndexOfKey(imgKey);

                                if (imageIndex >= 0)
                                {
                                    lvItem.ImageIndex = imageIndex;
                                }

                                // VirtualMode 下 SmallImageList.Images 多出一張圖片、或 ImageIndex 被改變，
                                // 都不會自動觸發這一列重繪（非 VirtualMode 才會）。這裡不直接呼叫
                                // RedrawListViewItem(lvItem)：實測過 RedrawItems 在批次密集時很容易撞上
                                // 下一個批次自己的 BeginUpdate 視窗而被吃掉、不會補跑（微軟官方文件已記載
                                // 這個限制）。呼叫端已經會在每個批次自己的 EndUpdate() 之後、以及整場
                                // 擷取結束時各補一次節流過的 Invalidate()（見 DoProcessMessages／
                                // LoadXLSX／BtnStop_Click），但下載本身是背景中各自獨立完成的非同步工作，
                                // 仍可能晚於「最後一次」那些呼叫點才真正完成（例如整場擷取已經停止、
                                // BtnStop_Click 的收尾 Invalidate() 都執行完了，這裡才姍姍來遲）——那種情況下
                                // 沒有任何後續事件會再觸發重繪，頭像就會永久空白。這裡額外主動補一次節流過的
                                // Invalidate()，跟呼叫端的時機互為保險，兩邊都命中節流窗口內時只會真的重繪一次。
                                InvalidateLiveChatListThrottled();
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());
                        }
                    });
                }

                // 登記進關聯用的索引，讓之後同一批次或後續批次的刪除／封鎖／回覆數更新／
                // 投票結果更新事件能 O(1) 找到這一列（而不是線性掃描整個 ListView）。
                if (!string.IsNullOrEmpty(id))
                {
                    SharedItemsByMessageID[id] = lvItem;
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

                RegisterNewListViewItemStats(type, authorBages, authorName, purchaseAmountText);

                listTempItem.Add(lvItem);
            }

            // 這裡刻意不再用 Task.Run 把實際插入 ListView 的動作丟到背景執行緒：DoProcessMessages
            // 本身就是透過 TBUserAgent.InvokeAsyncIfRequired(() => DoProcessMessages(batch), ...) 呼叫的
            // （見 FMain.cs 的擷取迴圈），呼叫當下就已經在 UI 執行緒上，不需要再轉一手。
            // 更重要的是正確性：舊版用 Task.Run 讓這個方法在還沒真正把這批資料插入 ListView 之前就先
            // return，外層迴圈以為這批「已處理完成」就繼續去抓下一批——執行緒集區不保證先排的工作先跑，
            // 如果下一批的 Task.Run 剛好比這一批先執行，聊天室畫面上（以及匯出檔案裡）的訊息順序就會
            // 跟實際收到的順序不一致。改成直接呼叫，讓 InvokeAsyncIfRequired 的 await 真正等到這批資料
            // 完整插入畫面後才算完成，才能保證批次之間嚴格照收到的順序處理。
            SharedListViewItems.AddRange(listTempItem);

            if (updateListView)
            {
                LVLiveChatList.BeginUpdate();
                AutoFitListViewColumns(LVLiveChatList, listTempItem);
                LVLiveChatList.VirtualListSize = SharedListViewItems.Count;

                if (SharedListViewItems.Count > 0)
                {
                    // VirtualMode 下捲動可見範圍要用 ListView 層級、以索引為準的多載，
                    // 不是 ListViewItem.EnsureVisible() 這個實例版；且一定要先更新完 VirtualListSize
                    // 才能呼叫，否則索引可能還沒被 ListView 視為有效範圍。
                    LVLiveChatList.EnsureVisible(SharedListViewItems.Count - 1);
                }

                LVLiveChatList.EndUpdate();

                // 保證落在沒有任何 BeginUpdate 視窗的時間點，強制重繪目前可視範圍，撿回前面批次因為
                // RedrawItems 撞上 BeginUpdate 視窗而被吃掉的頭像重繪（見上方頭像下載完成處的說明）。
                // 重播密集批次可能短時間內連續呼叫這裡，改用節流版避免短時間內觸發大量重複的重繪。
                InvalidateLiveChatListThrottled();

                UpdateSummaryInfo();
            }
        }
        catch (Exception ex)
        {
            WriteLog($"發生錯誤：{ex.GetExceptionMessage()}");
        }
    }

    /// <summary>
    /// 就地更新一筆已存在的既有列（同一個訊息 ID 再次出現時使用）：可能是輪詢間隔重疊造成的真重複資料
    /// （用相同的值覆寫一次無害），也可能是 replaceChatItemAction（例如超級留言／貼圖淡出後改為較小樣式）。
    /// 只更新視覺上可能因此改變的欄位（訊息內容、金額、顏色、排行榜名次、回覆數），
    /// 不重新套用會員／置頂等以 Type 分類的樣式（一則訊息被 replace 後通常不會變成其它訊息類型）。
    /// </summary>
    private static void ApplyExistingListViewItemUpdate(
        ListViewItem lvItem,
        string authorBadges,
        string messageContent,
        string purchaseAmountText,
        string foregroundColor,
        string backgroundColor,
        string headerBackgroundColor,
        string leaderboardRank,
        string replyCount)
    {
        lvItem.SubItems[1].Text = authorBadges;
        lvItem.SubItems[2].Text = messageContent;
        lvItem.SubItems[3].Text = purchaseAmountText;
        lvItem.SubItems[12].Text = leaderboardRank;
        lvItem.SubItems[13].Text = replyCount;

        if (ChatColorUtil.TryParse(foregroundColor, out Color parsedForegroundColor))
        {
            lvItem.SubItems[2].ForeColor = parsedForegroundColor;
        }

        if (ChatColorUtil.TryParse(backgroundColor, out Color parsedBackgroundColor))
        {
            foreach (ListViewItem.ListViewSubItem subItem in lvItem.SubItems)
            {
                subItem.BackColor = parsedBackgroundColor;
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
    }

    /// <summary>
    /// 累加式更新統計計數器，取代 UpdateSummaryInfo 內原本每批次都要重新掃描整個 ListView 的做法
    /// ——長時間直播累積上千則訊息後，每批次都重新掃描一次全部歷史資料是 O(n²)。
    /// <para>只應該在真正新增一列時呼叫一次（<see cref="DoProcessMessages"/>／<see cref="LoadXLSX"/> 各呼叫一處），
    /// 就地更新既有列（<see cref="ApplyExistingListViewItemUpdate"/>、刪除／封鎖標記）不會、也不應該呼叫這個方法，
    /// 否則會讓同一則訊息被重複計算。實際的分類判斷邏輯在 <see cref="ChatStatsCalculator.Classify"/>
    /// （跟 WinForms 脫鉤、有單元測試覆蓋，見 YTLiveChatCatcher.Tests），這裡只負責依分類結果套用狀態變更。</para>
    /// </summary>
    /// <param name="type">字串，訊息類型（已在地化）</param>
    /// <param name="authorBages">字串，作者徽章文字</param>
    /// <param name="authorName">字串，作者名稱</param>
    /// <param name="purchaseAmountText">字串，購買金額文字</param>
    private void RegisterNewListViewItemStats(
        string type,
        string authorBages,
        string authorName,
        string purchaseAmountText)
    {
        MessageStatsClassification classification = ChatStatsCalculator.Classify(SharedYTJsonParser, type, authorBages);

        if (classification.IsSuperChat)
        {
            SharedSuperChatCount++;
        }

        if (classification.IsSuperSticker)
        {
            SharedSuperStickerCount++;
        }

        if (classification.IsSuperChat || classification.IsSuperSticker)
        {
            if (ChatStatsCalculator.TryParsePurchaseAmount(purchaseAmountText, out string currencySymbol, out decimal amount))
            {
                SharedIncomeByCurrency[currencySymbol] = SharedIncomeByCurrency.GetValueOrDefault(currencySymbol) + amount;
            }
            else
            {
                WriteLog($"無法辨識的金額格式，未計入收益統計：「{purchaseAmountText}」。");
            }
        }

        if (classification.IsJoinMember)
        {
            SharedMemberJoinCount++;
        }

        if (classification.IsMemberUpgrade)
        {
            SharedMemberUpgradeCount++;
        }

        if (classification.IsMemberMilestone)
        {
            SharedMemberMilestoneCount++;
        }

        if (classification.IsMemberGift)
        {
            SharedMemberGiftCount++;
        }

        if (classification.IsReceivedMemberGift)
        {
            SharedReceivedMemberGiftCount++;
        }

        if (classification.CountsAsChatMessage)
        {
            SharedChatCount++;
        }

        if (classification.CountsAsMemberInRoom)
        {
            SharedMemberInRoomAuthors.Add(authorName);
        }

        if (classification.CountsAsDistinctAuthor)
        {
            SharedDistinctAuthors.Add(authorName);
        }
    }

    /// <summary>
    /// 套用「留言已被刪除」事件：透過 <see cref="SharedItemsByMessageID"/> 找到對應訊息 ID 的既有列並標記，
    /// 而不是把這個事件本身當成一則新留言加入清單。找不到對應列時（例如該訊息是在這次擷取開始前發送的）
    /// 靜默略過，不做任何事。
    /// </summary>
    /// <param name="targetItemId">字串，被刪除訊息的 ID</param>
    private void ApplyMessageDeletedMarker(string? targetItemId)
    {
        if (string.IsNullOrEmpty(targetItemId) ||
            !SharedItemsByMessageID.TryGetValue(targetItemId, out ListViewItem? lvItem))
        {
            return;
        }

        MarkListViewItemAsRemoved(lvItem, "〔已刪除〕");

        RedrawListViewItem(lvItem);
    }

    /// <summary>
    /// 套用「使用者已被封鎖」事件：透過 <see cref="SharedItemsByAuthorChannelID"/> 一次找出該使用者
    /// 目前所有留言的既有列並逐一標記。
    /// </summary>
    /// <param name="externalChannelId">字串，被封鎖使用者的外部頻道 ID</param>
    private void ApplyUserBannedMarker(string? externalChannelId)
    {
        if (string.IsNullOrEmpty(externalChannelId) ||
            !SharedItemsByAuthorChannelID.TryGetValue(externalChannelId, out List<ListViewItem>? lvItems))
        {
            return;
        }

        foreach (ListViewItem lvItem in lvItems)
        {
            MarkListViewItemAsRemoved(lvItem, "〔使用者已被封鎖〕");

            RedrawListViewItem(lvItem);
        }
    }

    /// <summary>
    /// 幫既有列加上「已刪除／已封鎖」的視覺標記，保留原始內容（不移除該列）供封存與匯出使用。
    /// 標記文字直接寫進訊息內容欄位本身（而不是只靠字型樣式），是因為 Excel 匯出目前只會轉存
    /// 前景／背景顏色，不會轉存刪除線字型樣式，純靠字型會讓這個資訊在匯出檔案裡遺失。
    /// </summary>
    /// <param name="lvItem">ListViewItem</param>
    /// <param name="marker">字串，標記文字</param>
    private static void MarkListViewItemAsRemoved(ListViewItem lvItem, string marker)
    {
        ListViewItem.ListViewSubItem messageSubItem = lvItem.SubItems[2];

        if (!messageSubItem.Text.StartsWith(marker, StringComparison.Ordinal))
        {
            messageSubItem.Text = $"{marker}{messageSubItem.Text}";
        }

        foreach (ListViewItem.ListViewSubItem subItem in lvItem.SubItems)
        {
            // 2026/9 修正：這個方法理論上可能對同一列被呼叫兩次（例如一則留言先被刪除，
            // 之後其作者又被封鎖），第二次呼叫時 subItem.Font 已經是第一次呼叫建立的 Font
            // 執行個體，直接覆蓋掉、不 Dispose 舊的會造成 GDI 資源小洩漏。這裡的 subItem.Font
            // 只有可能是 null（尚未被設定過，繼承 lvItem.Font，不能 Dispose）或是這個方法自己
            // 先前建立的 Font（可以安全 Dispose），先記住舊值，指派新值之後再 Dispose 舊的。
            Font? previousFont = subItem.Font;

            subItem.Font = new Font(previousFont ?? lvItem.Font, FontStyle.Strikeout);
            subItem.ForeColor = Color.Gray;

            previousFont?.Dispose();
        }
    }

    /// <summary>
    /// 套用「回覆數更新」事件：透過 <see cref="SharedItemsByReplyCountEntityKey"/> 找到對應的既有列
    /// （通常是超級留言／超級貼圖），更新其回覆數欄位。
    /// </summary>
    /// <param name="entityKey">字串，回覆數更新事件的關聯鍵值（對應原始訊息的 ReplyCountEntityKey）</param>
    /// <param name="replyCount">字串，最新的回覆數</param>
    private void ApplyReplyCountUpdate(string? entityKey, string? replyCount)
    {
        if (string.IsNullOrEmpty(entityKey) ||
            !SharedItemsByReplyCountEntityKey.TryGetValue(entityKey, out ListViewItem? lvItem))
        {
            return;
        }

        lvItem.SubItems[13].Text = replyCount ?? string.Empty;

        RedrawListViewItem(lvItem);
    }

    /// <summary>
    /// 套用「投票結果更新」事件：透過 <see cref="SharedItemsByMessageID"/> 找到對應投票 ID 的既有列
    /// （投票建立時的 ID 沿用同一個 liveChatPollId），更新其訊息內容為最新的得票結果文字。
    /// </summary>
    /// <param name="pollId">字串，投票 ID</param>
    /// <param name="messageContent">字串，最新的得票結果文字</param>
    private void ApplyPollResultUpdate(string? pollId, string? messageContent)
    {
        if (string.IsNullOrEmpty(pollId) ||
            !SharedItemsByMessageID.TryGetValue(pollId, out ListViewItem? lvItem))
        {
            return;
        }

        lvItem.SubItems[2].Text = messageContent ?? string.Empty;

        RedrawListViewItem(lvItem);
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

        BtnExportCommunityPosts.InvokeIfRequired(() =>
        {
            BtnExportCommunityPosts.Enabled = false;
        });

        PBProgress.InvokeIfRequired(() =>
        {
            PBProgress.Style = ProgressBarStyle.Marquee;
        });

        this.InvokeIfRequired(() =>
        {
            UseWaitCursor = true;
        });
    }

    /// <summary>
    /// 終止匯入／匯出任務
    /// </summary>
    /// <param name="isImport">布林值，判斷是否為匯入，預設值為 false</param>
    private void TerminateLongTask(bool isImport = false)
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

        BtnExportCommunityPosts.InvokeIfRequired(() =>
        {
            BtnExportCommunityPosts.Enabled = true;
        });

        PBProgress.InvokeIfRequired(() =>
        {
            PBProgress.Style = ProgressBarStyle.Blocks;
        });

        this.InvokeIfRequired(() =>
        {
            UseWaitCursor = false;
        });

        string taskWord = isImport ? "匯入" : "匯出";

        WriteLog($"*.xlsx {taskWord}作業完成。");
    }

    /// <summary>
    /// 更新統計資訊
    /// <para>2026/8 改為直接讀取 <see cref="RegisterNewListViewItemStats"/> 維護的累加式計數器，
    /// 不再每次都重新掃描整個 <see cref="LVLiveChatList"/>——長時間直播累積上千則訊息後，
    /// 舊版每批次都要重新掃描一次全部歷史資料（且同一批次內還要掃好幾次算不同統計項目），
    /// 是隨訊息數量成長的 O(n²) 效能問題；改成單純讀取欄位後，這個方法本身是 O(1)。</para>
    /// </summary>
    private void UpdateSummaryInfo()
    {
        TBLog.InvokeIfRequired(() =>
        {
            // 依貨幣符號分別加總、分別顯示，不做匯率換算——不同貨幣的原始金額不能直接相加或比較。
            // 此比例只是可設定的粗略估算；實際結算仍會受稅務、退款、平台與地區規則影響。
            string rawBreakdown = SharedIncomeByCurrency.Count > 0 ?
                string.Join("、", SharedIncomeByCurrency.Select(n => $"{n.Key}{n.Value}")) :
                "0";

            string actualBreakdown = SharedIncomeByCurrency.Count > 0 ?
                string.Join("、", SharedIncomeByCurrency.Select(n =>
                    $"{n.Key}{Math.Round(n.Value * SharedRevenueEstimateRate, 0, MidpointRounding.AwayFromZero)}")) :
                "0";

            LTempIncome.InvokeIfRequired(() =>
            {
                LTempIncome.Text = $"粗估收益（{SharedRevenueEstimateRate:P0}）：{actualBreakdown}";

                SharedTooltip.SetToolTip(LTempIncome, $"原始累積金額：{rawBreakdown}；此比例僅為粗略估算，可於資料工具調整。");
            });

            string message = $"目前累積金額：{rawBreakdown}（粗估收益 {SharedRevenueEstimateRate:P0}：{actualBreakdown}）{Environment.NewLine}" +
                "※此比例僅為粗略估算；依貨幣符號分別加總，不做匯率換算。";

            WriteLog(message);
        });

        LChatCount.InvokeIfRequired(() =>
        {
            LChatCount.Text = $"留言數量：{SharedChatCount} 個";
        });

        LSuperChatCount.InvokeIfRequired(() =>
        {
            LSuperChatCount.Text = $"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatSuperChat)}：{SharedSuperChatCount} 個";
        });

        LSuperStickerCount.InvokeIfRequired(() =>
        {
            LSuperStickerCount.Text = $"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatSuperSticker)}：{SharedSuperStickerCount} 個";
        });

        LMemberJoinCount.InvokeIfRequired(() =>
        {
            LMemberJoinCount.Text = $"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatJoinMember)}：{SharedMemberJoinCount} 位";

            string tooltip = $"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatMemberUpgrade)}：{SharedMemberUpgradeCount} 位、" +
                $"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatMemberMilestone)}：{SharedMemberMilestoneCount} 位、" +
                $"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatMemberGift)}：{SharedMemberGiftCount} 位、" +
                $"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatReceivedMemberGift)}：{SharedReceivedMemberGiftCount} 位";

            SharedTooltip.SetToolTip(LMemberJoinCount, tooltip);
        });

        LMemberInRoomCount.InvokeIfRequired(() =>
        {
            LMemberInRoomCount.Text = $"會員人數：{SharedMemberInRoomAuthors.Count} 位";
        });

        LAuthorCount.InvokeIfRequired(() =>
        {
            LAuthorCount.Text = $"留言人數：{SharedDistinctAuthors.Count} 位";
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

        BtnCookieLogin.InvokeIfRequired(() =>
        {
            BtnCookieLogin.Enabled = enable;
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

        SharedYTJsonParser = new YTJsonParser(
            new YTJsonParserOptions()
            {
                HttpClient = httpClient,
                FetchLargePicture = true,
                DisplayLanguage = EnumSet.DisplayLanguage.Chinese_Traditional,
            },
            new DiagnosticForwardingLogger(
                SharedYTJsonParserLogger,
                message => WriteLog($"⚠ 偵測到 YouTube 回應內含目前尚未支援的內容，這批資料可能沒有被完整解析（詳見 Logs/log.txt）：{message}")));

        // 若使用者先前在登入視窗勾選「記住我」，載入以 DPAPI 加密儲存的 Cookie。
        string? rememberedCookies = SecureCookieStore.Load();

        if (!string.IsNullOrEmpty(rememberedCookies))
        {
            SharedYTJsonParser.Cookies = rememberedCookies;
        }

        UpdateCookieStatus();
    }

    /// <summary>
    /// 檢查是否有上次未正常結束的擷取記錄（<see cref="CaptureRecoveryStore"/>），有的話詢問使用者是否要載入。
    /// <para>「未正常結束」代表應用程式在擷取過程中當機、被強制關閉，或使用者忘記匯出就直接關閉視窗——
    /// 這幾種情況下，畫面上累積的資料原本會直接消失，因為它們只存在記憶體裡。</para>
    /// </summary>
    private void CheckCaptureRecovery()
    {
        if (!CaptureRecoveryStore.Exists())
        {
            return;
        }

        DialogResult result = MessageBox.Show(
            "偵測到上次擷取記錄尚未正常結束（可能是應用程式當機、被強制關閉，或忘記匯出就關閉），是否要載入這些資料？",
            Text,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
        {
            CaptureRecoveryStore.Clear();
            CaptureSessionStore.Clear();

            return;
        }

        List<List<RendererData>> recoveredBatches = CaptureRecoveryStore.LoadBatches();
        CaptureSessionManifest? manifest = CaptureSessionStore.Load();

        foreach (List<RendererData> batch in recoveredBatches)
        {
            IReadOnlyList<RendererData> newMessages = SharedCaptureMessageDeduplicator.FilterNew(batch);

            if (newMessages.Count > 0)
            {
                DoProcessMessages(newMessages);
            }
        }

        // 刻意不在載入後清除記錄檔——這樣即使載入回來後還沒來得及匯出就又當機一次，
        // 這批資料依然留在復原記錄裡，下次啟動還是問得到。記錄檔只在成功匯出或手動清空聊天室時才清除。
        WriteLog($"已從當機復原記錄載入 {recoveredBatches.Sum(n => n.Count)} 筆資料（共 {recoveredBatches.Count} 個批次）。");

        if (manifest is { IsDataComplete: false } && !string.IsNullOrEmpty(manifest.LastContinuation))
        {
            SharedCaptureSessionManifest = manifest;
            SharedResumeContinuation = manifest.LastContinuation;
            TBVideoID.Text = manifest.VideoId;
            WriteLog("已載入上次的續傳狀態；按下「開始」後會嘗試從中斷點繼續。continuation 可能已過期，完成後請確認資料完整性標示。");
        }
    }

    /// <summary>
    /// 保存擷取 session；manifest 寫入失敗不應中止聊天室擷取。
    /// </summary>
    private void TrySaveCaptureSession(CaptureSessionManifest manifest)
    {
        try
        {
            CaptureSessionStore.Save(manifest);
        }
        catch (Exception ex)
        {
            SharedLogger.LogWarning(ex, "無法保存擷取 session manifest。");
        }
    }

    /// <summary>
    /// 更新目前登入用 Cookie 的狀態顯示
    /// </summary>
    private void UpdateCookieStatus()
    {
        LCookieStatus.InvokeIfRequired(() =>
        {
            LCookieStatus.Text = string.IsNullOrEmpty(SharedYTJsonParser.Cookies) ?
                "尚未登入。" :
                SecureCookieStore.Exists() ?
                    "已登入（記住我）。" :
                    "已登入（本次執行有效）。";
        });
    }

}
