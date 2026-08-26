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
            },
            new()
            {
                Name = "LeaderboardRank",
                Text = "排行榜名次",
                TextAlign = HorizontalAlignment.Center,
                Width = 80,
                DisplayIndex = 12
            },
            new()
            {
                Name = "ReplyCount",
                Text = "回覆數",
                TextAlign = HorizontalAlignment.Center,
                Width = 60,
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
    public void TtsSpeak(ListView listView)
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
        return Task.Run(() =>
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
                    n.SubItems[5].Text != SharedYTJsonParser.GetLocalizeString(KeySet.ChatMessageDeleted) &&
                    n.SubItems[5].Text != SharedYTJsonParser.GetLocalizeString(KeySet.ChatUserBanned) &&
                    n.SubItems[5].Text != SharedYTJsonParser.GetLocalizeString(KeySet.ChatReplyCountUpdate) &&
                    n.SubItems[5].Text != SharedYTJsonParser.GetLocalizeString(KeySet.ChatPollUpdate));

            foreach (ListViewItem listViewItem in dataSet)
            {
                ExcelRange firstRange = worksheet1.Cells[startIdx1, 1];

                firstRange.StyleName = "ContentStyle";
                firstRange.Value = string.Empty;
                firstRange.Style.Fill.SetBackground(listViewItem.BackColor);

                if (CBExportAuthorPhoto.Checked)
                {
                    if (!string.IsNullOrEmpty(listViewItem.SubItems[9].Text))
                    {
                        firstRange.Formula = $"IMAGE(\"{listViewItem.SubItems[9].Text}\")";
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
                    excelRange.Style.WrapText = true;

                    if (j == 9)
                    {
                        if (!string.IsNullOrEmpty(listViewSubItem.Text))
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

                List<string> arrayFormula =
                [
                    $"SUM(COUNTIF(G:G,{{\"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatGeneral)}\", " +
                    $"\"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatSuperChat)}\"," +
                    $"\"{SharedYTJsonParser.GetLocalizeString(KeySet.ChatSuperSticker)}\"}}))&\" 個\"",
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

            worksheet1.Calculate(n => n.AlwaysRefreshImageFunction = false);

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
                    // 2026/8 補上刪除／封鎖／回覆數更新／投票結果更新的排除，理由同上方的內容分頁排除。
                    n.SubItems[5].Text != SharedYTJsonParser.GetLocalizeString(KeySet.ChatMessageDeleted) &&
                    n.SubItems[5].Text != SharedYTJsonParser.GetLocalizeString(KeySet.ChatUserBanned) &&
                    n.SubItems[5].Text != SharedYTJsonParser.GetLocalizeString(KeySet.ChatReplyCountUpdate) &&
                    n.SubItems[5].Text != SharedYTJsonParser.GetLocalizeString(KeySet.ChatPollUpdate) &&
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
                bool isStreaming = SharedYTJsonParser.IsVideoStreamingAsync(videoID).GetAwaiter().GetResult();

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

                for (int i = 1; i <= worksheet2.Cells.Count(); i++)
                {
                    if (i < widthSet.Length)
                    {
                        ExcelColumn column = worksheet2.Column(i);

                        column.AutoFit();
                    }
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
                        range2.Formula = $"IMAGE(\"{emojiData.Url}\")";
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

                    if (!string.IsNullOrEmpty(emojiData.Url))
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

                for (int i = 2; i <= worksheet3.Cells.Count(); i++)
                {
                    if (i < widthSet.Length)
                    {
                        ExcelColumn column = worksheet3.Column(i);

                        column.AutoFit();
                    }
                }

                worksheet3.Calculate(n => n.AlwaysRefreshImageFunction = false);
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
                        range9.Formula = $"IMAGE(\"{badgeData.Url}\")";
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

                    if (!string.IsNullOrEmpty(badgeData.Url))
                    {
                        range13.Hyperlink = new Uri(badgeData.Url, UriKind.Absolute);
                    }

                    ExcelRange range14 = worksheet4.Cells[startIdx3, 6];

                    range14.StyleName = "ContentStyle";
                    range14.Value = badgeData.IconType;

                    startIdx3++;
                }

                for (int i = 2; i <= worksheet4.Cells.Count(); i++)
                {
                    if (i < widthSet.Length)
                    {
                        ExcelColumn column = worksheet4.Column(i);

                        column.AutoFit();
                    }
                }

                worksheet4.Calculate(n => n.AlwaysRefreshImageFunction = false);
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
                        range15.Formula = $"IMAGE(\"{stickerData.Url}\")";
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

                    if (!string.IsNullOrEmpty(stickerData.Url))
                    {
                        range12.Hyperlink = new Uri(stickerData.Url, UriKind.Absolute);
                    }

                    startIdx4++;
                }

                for (int i = 2; i <= worksheet5.Cells.Count(); i++)
                {
                    if (i < widthSet.Length)
                    {
                        ExcelColumn column = worksheet5.Column(i);

                        column.AutoFit();
                    }
                }

                worksheet5.Calculate(n => n.AlwaysRefreshImageFunction = false);
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
    /// </summary>
    /// <param name="message">字串，訊息內容</param>
    public async void WriteLog(string message)
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
        SharedTooltip.SetToolTip(TBInterval, "僅在未勾選「隨機間隔秒數」以及為「重播」時會生效。");

        // 設定控制項的狀態。
        SetControlsState(true);
    }

    /// <summary>
    /// 執行處裡訊息
    /// </summary>
    /// <param name="messages">IReadOnlyList&lt;RendererData&gt;</param>
    private void DoProcessMessages(IReadOnlyList<RendererData> messages)
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
                        if (!SharedCustomEmojis.Any(n => n.ID == emojiData.ID))
                        {
                            if (emojiData.IsCustomEmoji)
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

                if (!string.IsNullOrEmpty(headerBackgroundColor))
                {
                    // 只變更「標頭」相關欄位的背景色（作者名稱／徽章／金額／時間），呈現跟真實 YouTube
                    // 超級留言一樣的雙色設計（標頭一色、內文另一色），訊息本文維持 backgroundColor。
                    Color headerColor = ColorTranslator.FromHtml(headerBackgroundColor);
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
                    string imgKey = authorName;

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
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.LogError("{ErrorMessage}", ex.GetExceptionMessage());
                        }
                    });

                    lvItem.ImageKey = imgKey;
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

            Task.Run(() =>
            {
                LVLiveChatList.InvokeIfRequired(() =>
                {
                    LVLiveChatList.BeginUpdate();
                    LVLiveChatList.Items.AddRange([.. listTempItem]);

                    if (LVLiveChatList.Items.Count > 0)
                    {
                        LVLiveChatList.Items[^1].EnsureVisible();
                    }

                    LVLiveChatList.EndUpdate();
                });

                UpdateSummaryInfo();
            });
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

        if (!string.IsNullOrEmpty(foregroundColor))
        {
            lvItem.SubItems[2].ForeColor = ColorTranslator.FromHtml(foregroundColor);
        }

        if (!string.IsNullOrEmpty(backgroundColor))
        {
            foreach (ListViewItem.ListViewSubItem subItem in lvItem.SubItems)
            {
                subItem.BackColor = ColorTranslator.FromHtml(backgroundColor);
            }
        }

        if (!string.IsNullOrEmpty(headerBackgroundColor))
        {
            Color headerColor = ColorTranslator.FromHtml(headerBackgroundColor);
            int[] headerSubItemIndexes = [0, 1, 3, 4];

            foreach (int headerSubItemIndex in headerSubItemIndexes)
            {
                lvItem.SubItems[headerSubItemIndex].BackColor = headerColor;
            }
        }
    }

    /// <summary>
    /// 累加式更新統計計數器（2026/8 新增，取代 UpdateSummaryInfo 內原本每批次都要重新掃描整個 ListView
    /// 的做法——長時間直播累積上千則訊息後，每批次都重新掃描一次全部歷史資料是 O(n²)）。
    /// <para>只應該在真正新增一列時呼叫一次（<see cref="DoProcessMessages"/>／<see cref="LoadXLSX"/> 各呼叫一處），
    /// 就地更新既有列（<see cref="ApplyExistingListViewItemUpdate"/>、刪除／封鎖標記）不會、也不應該呼叫這個方法，
    /// 否則會讓同一則訊息被重複計算。條件務必跟 <see cref="UpdateSummaryInfo"/> 過去的篩選邏輯保持一致。</para>
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
        bool isJoinMember = type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatJoinMember);
        bool isMemberUpgrade = type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatMemberUpgrade);
        bool isMemberMilestone = type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatMemberMilestone);
        bool isMemberGift = type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatMemberGift);
        bool isReceivedMemberGift = type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatReceivedMemberGift);
        bool isRedirect = type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatRedirect);
        bool isPinned = type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatPinned);
        bool isSuperChat = type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatSuperChat);
        bool isSuperSticker = type == SharedYTJsonParser.GetLocalizeString(KeySet.ChatSuperSticker);
        bool isYouTubeSystem = type == Rubujo.YouTube.Utility.Sets.StringSet.YouTube;

        if (isSuperChat)
        {
            SharedSuperChatCount++;
        }

        if (isSuperSticker)
        {
            SharedSuperStickerCount++;
        }

        if ((isSuperChat || isSuperSticker) && purchaseAmountText.StartsWith('$'))
        {
            string amountText = purchaseAmountText.Replace("$", string.Empty);

            SharedTotalIncome += double.TryParse(amountText, out double amount) ? amount : 0;
        }

        if (isJoinMember)
        {
            SharedMemberJoinCount++;
        }

        if (isMemberUpgrade)
        {
            SharedMemberUpgradeCount++;
        }

        if (isMemberMilestone)
        {
            SharedMemberMilestoneCount++;
        }

        if (isMemberGift)
        {
            SharedMemberGiftCount++;
        }

        if (isReceivedMemberGift)
        {
            SharedReceivedMemberGiftCount++;
        }

        // 對應舊版 LChatCount 的排除條件（系統訊息／五種會員事件／導向／置頂）。
        if (!isYouTubeSystem &&
            !isJoinMember && !isMemberUpgrade && !isMemberMilestone &&
            !isMemberGift && !isReceivedMemberGift &&
            !isRedirect && !isPinned)
        {
            SharedChatCount++;
        }

        // 對應舊版 LMemberInRoomCount 的排除條件（加入／升級／里程碑事件本身不算「在場會員」，
        // 但這幾種事件之外、帶有會員徽章的一般留言／超級留言／貼圖才算）。
        if (!isJoinMember && !isMemberUpgrade && !isMemberMilestone &&
            authorBages.Contains(StringSet.Member))
        {
            SharedMemberInRoomAuthors.Add(authorName);
        }

        // 對應舊版 LAuthorCount 的排除條件（系統訊息、以及類型文字含有「會員」關鍵字的五種會員事件）。
        if (!isYouTubeSystem && !type.Contains(StringSet.Member))
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
            subItem.Font = new Font(subItem.Font ?? lvItem.Font, FontStyle.Strikeout);
            subItem.ForeColor = Color.Gray;
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

        PBProgress.InvokeIfRequired(() =>
        {
            PBProgress.Style = ProgressBarStyle.Blocks;
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
            // YouTube 會抽取 30% 收益。
            double actualIncome = Math.Round(SharedTotalIncome * 0.70, 0, MidpointRounding.AwayFromZero);

            LTempIncome.InvokeIfRequired(() =>
            {
                LTempIncome.Text = $"預計收益：共 {actualIncome} 元整";

                SharedTooltip.SetToolTip(LTempIncome, $"累積收益：共 {SharedTotalIncome} 元整");
            });

            string message = $"目前累積收益：共 {SharedTotalIncome} 元整" +
                $"（實收：{actualIncome} 元整）{Environment.NewLine}" +
                "※僅計算超級留言／貼圖的新臺幣收益，其它幣種一律不納入計算。";

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
            SharedYTJsonParserLogger);

        // 若使用者先前在登入視窗勾選「記住我」，載入以 DPAPI 加密儲存的 Cookie。
        string? rememberedCookies = SecureCookieStore.Load();

        if (!string.IsNullOrEmpty(rememberedCookies))
        {
            SharedYTJsonParser.Cookies = rememberedCookies;
        }

        UpdateCookieStatus();
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