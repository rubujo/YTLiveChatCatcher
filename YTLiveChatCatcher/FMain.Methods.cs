﻿using Color = System.Drawing.Color;
using HorizontalAlignment = System.Windows.Forms.HorizontalAlignment;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
using NLog;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Drawing.Chart.Style;
using OfficeOpenXml.Style;
using OfficeOpenXml.Style.XmlAccess;
using Rubujo.YouTube.Utility.Events;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Sets;
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
                Name = "BackgroundColor",
                Text = "背景顏色",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 6
            },
            new()
            {
                Name = "TimestampText",
                Text = "時間標記文字",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 7
            },
            new()
            {
                Name = "AuthorPhotoUrl",
                Text = "頭像網址",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 8
            },
            new()
            {
                Name = "AuthorExternalChannelID",
                Text = "外部頻道 ID",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 9
            },
            new()
            {
                Name = "MessageID",
                Text = "訊息 ID 值",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 10
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

                if (type == Rubujo.YouTube.Utility.Sets.StringSet.ChatGeneral ||
                    type == Rubujo.YouTube.Utility.Sets.StringSet.ChatSuperChat ||
                    type == Rubujo.YouTube.Utility.Sets.StringSet.ChatSuperSticker)
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

        bool isStreaiming = false;

        RBtnStreaming.InvokeIfRequired(() =>
        {
            isStreaiming = RBtnStreaming.Checked;
        });

        DialogResult dialogResult = saveFileDialog.ShowDialog();

        if (dialogResult == DialogResult.OK)
        {
            if (!string.IsNullOrEmpty(saveFileDialog.FileName))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        if (listView.Name == LVLiveChatList.Name)
                        {
                            RunLongTask();
                        }

                        using FileStream fileStream = (FileStream)saveFileDialog.OpenFile();

                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        using ExcelPackage package = new();

                        double[] widthSet = [5.0, 20.0, 24.0, 50.0, 14.0, 27.0, 16.0, 20.0, 20.0, 20.0, 20.0, 0.0];

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

                        for (int i = 0; i < listView.Columns.Count; i++)
                        {
                            ColumnHeader header = listView.Columns[i];

                            ExcelRange range = worksheet1.Cells[1, i + 2];

                            range.StyleName = "HeaderStyle";
                            range.Style.Fill.SetBackground(Color.BlanchedAlmond);
                            range.Value = header.Text;
                        }

                        // 設定篩選。
                        worksheet1.Cells[1, 2, 1, 9].AutoFilter = true;

                        #endregion

                        #region 建置內容

                        int startIdx1 = 2;

                        listView.InvokeIfRequired(() =>
                        {
                            IEnumerable<ListViewItem> dataSet = listView.GetListViewItems()
                                .Where(n => n.SubItems[5].Text != StringSet.AppName &&
                                    n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.YouTube);

                            foreach (ListViewItem listViewItem in dataSet)
                            {
                                ExcelRange firstRange = worksheet1.Cells[startIdx1, 1];

                                firstRange.StyleName = "ContentStyle";
                                firstRange.Value = string.Empty;
                                firstRange.Style.Fill.SetBackground(listViewItem.BackColor);

                                if (CBExportAuthorPhoto.Checked)
                                {
                                    Image? image = listView.SmallImageList?.Images[listViewItem.ImageKey];

                                    if (image != null)
                                    {
                                        // 將 image 轉換成 stream。
                                        Stream? imageStream = image.ToStream(ImageFormat.Png);

                                        if (imageStream != null)
                                        {
                                            // 2021/4/1
                                            // 對名稱加料，以免造成例外。
                                            // 還沒找到方法可以重複利用 ExcelPicture。
                                            ExcelPicture picture = worksheet1.Drawings
                                                .AddPicture($"row{startIdx1}_{listViewItem.ImageKey}",
                                                imageStream);

                                            int zeroBasedRow = startIdx1 - 1;

                                            picture.SetPosition(zeroBasedRow, 0, 0, 0);

                                            imageStream.Close();
                                            imageStream.Dispose();
                                            imageStream = null;
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
                                    excelRange.Style.WrapText = true;

                                    if (j == 8)
                                    {
                                        if (!string.IsNullOrEmpty(listViewSubItem.Text))
                                        {
                                            excelRange.Hyperlink = new Uri(listViewSubItem.Text, UriKind.Absolute);
                                        }
                                    }
                                }

                                startIdx1++;
                            }
                        });

                        // 只有 LVLiveChatList 可以匯出統計資訊，
                        // 因為統計資訊的資料不是從 Excel 的內容直接產生的。
                        if (listView.Name == LVLiveChatList.Name)
                        {
                            #region 統計資訊

                            int summaryIdx = 1;

                            ExcelRange summaryHeaderRange = worksheet1.Cells[summaryIdx, 14, summaryIdx, 15];

                            summaryHeaderRange.Merge = true;
                            summaryHeaderRange.StyleName = "HeaderStyle";
                            summaryHeaderRange.Style.Fill.SetBackground(Color.BlanchedAlmond);
                            summaryHeaderRange.Value = "統計資訊";

                            summaryIdx++;

                            List<string> arrayFormula =
                            [
                                $"SUM(COUNTIF(G:G,{{\"{Rubujo.YouTube.Utility.Sets.StringSet.ChatGeneral}\", " +
                                $"\"{Rubujo.YouTube.Utility.Sets.StringSet.ChatSuperChat}\"," +
                                $"\"{Rubujo.YouTube.Utility.Sets.StringSet.ChatSuperSticker}\"}}))&\" 個\"",
                                $"COUNTIF(G:G,\"{Rubujo.YouTube.Utility.Sets.StringSet.ChatSuperChat}\")&\" 個\"",
                                $"COUNTIF(G:G,\"{Rubujo.YouTube.Utility.Sets.StringSet.ChatSuperSticker}\")&\" 個\"",
                                $"COUNTIF(G:G,\"{Rubujo.YouTube.Utility.Sets.StringSet.ChatJoinMember}\")&\" 個\"",
                                $"COUNTIF(G:G,\"{Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberUpgrade}\")&\" 個\"",
                                $"COUNTIF(G:G,\"{Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberMilestone}\")&\" 個\"",
                                $"COUNTIF(G:G,\"{Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberGift}\")&\" 個\"",
                                $"COUNTIF(G:G,\"{Rubujo.YouTube.Utility.Sets.StringSet.ChatReceivedMemberGift}\")&\" 個\""
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
                                        LTempIncome.Text,
                                }
                            ];

                            string tpLTempIncome = SharedTooltip?.GetToolTip(LTempIncome) ?? string.Empty;

                            if (!string.IsNullOrEmpty(tpLTempIncome))
                            {
                                arraySummaryInfo.Add(tpLTempIncome);
                            }

                            // 設定預設寬度。
                            worksheet1.Column(14).Width = 15.0;

                            char[] separators2 = ['：'];

                            for (int i = 0; i < arraySummaryInfo.Count; i++)
                            {
                                string[] arrayInfo = arraySummaryInfo[i].Split(separators2,
                                    StringSplitOptions.RemoveEmptyEntries);

                                ExcelRange summaryTitleRange = worksheet1.Cells[summaryIdx, 14];

                                summaryTitleRange.StyleName = "HeaderStyle";
                                summaryTitleRange.Style.Font.Bold = false;
                                summaryTitleRange.Value = arrayInfo[0];
                                // 2022-05-30 改為使用固定寬度。
                                //summaryTitleRange.AutoFitColumns();

                                ExcelRange summaryContentRange = worksheet1.Cells[summaryIdx, 15];

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

                        #endregion

                        #region 時間熱點

                        listView.InvokeIfRequired(() =>
                        {
                            // 參考 1：https://stackoverflow.com/a/687347
                            // 參考 2：https://stackoverflow.com/a/687370

                            // 排除在影片開始前的時間點。
                            Dictionary<string, int> sourceList = listView
                                .GetListViewItems()
                                .Where(n => n.SubItems[5].Text != StringSet.AppName &&
                                    n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.YouTube &&
                                    // 2022-10-25 因不容易轉換成影片對應時間點，故而直接排除
                                    // Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberGift、
                                    // Rubujo.YouTube.Utility.Sets.StringSet.ChatReceivedMemberGift 等類型的資料，
                                    // 以免在時間熱點活頁簿內出現奇怪的時間點。
                                    n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberGift &&
                                    n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.ChatReceivedMemberGift &&
                                    !string.IsNullOrEmpty(n.SubItems[7].Text) &&
                                    !n.SubItems[7].Text.Contains('-'))
                                .Select(n => n.SubItems[7].Text.Length > 3 ?
                                    n.SubItems[7].Text[0..^3] :
                                    n.SubItems[7].Text)
                                .GroupBy(n => n)
                                .Select(n => new { Timestamp = n.Key, Count = n.Count() })
                                .ToDictionary(n => n.Timestamp, n => n.Count);

                            if (sourceList.Count > 0)
                            {
                                string sheetName = isStreaiming ? StringSet.SheetName2 : StringSet.SheetName3;

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
                                // 2021-12-11 還沒找到 EPPlus 調整標籤間距的方法，故使用 eTextVerticalType.Vertical。
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
                        });

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

                                IImage? image = emojiData.Image;

                                if (image != null)
                                {
                                    // 將 image 轉換成 stream。
                                    Stream? imageStream = null;

                                    try
                                    {
                                        imageStream = image.AsStream();
                                    }
                                    catch (Exception ex)
                                    {
                                        WriteLog($"無法將自定義表情符號「{emojiData.Label}」轉換成 Stream。");
                                        WriteLog($"自定義表情符號的網址：{emojiData.Url}");
                                        WriteLog($"發生錯誤：{ex}");
                                    }

                                    if (imageStream != null)
                                    {
                                        // 2021-04-01
                                        // 對名稱加料，以免造成例外。
                                        // 還沒找到方法可以重複利用 ExcelPicture。
                                        ExcelPicture picture = worksheet3.Drawings
                                            .AddPicture($"row{startIdx2}_{emojiData.ID}",
                                            imageStream);

                                        int zeroBasedRow = startIdx2 - 1;

                                        picture.SetPosition(zeroBasedRow, 0, 0, 0);
                                        // 欄寬為 0.98cm。
                                        // 1% : 0.0127cm
                                        // 77% : 98cm
                                        picture.SetSize(77);

                                        imageStream.Close();
                                        imageStream.Dispose();
                                        imageStream = null;
                                    }
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

                                IImage? image = badgeData.Image;

                                if (image != null)
                                {
                                    // 將 image 轉換成 stream。
                                    Stream? imageStream = null;

                                    try
                                    {
                                        imageStream = image.AsStream();
                                    }
                                    catch (Exception ex)
                                    {
                                        WriteLog($"無法將會員徽章「{badgeData.Label}」轉換成 Stream。");
                                        WriteLog($"會員徽章的網址：{badgeData.Url}");
                                        WriteLog($"發生錯誤：{ex}");
                                    }

                                    if (imageStream != null)
                                    {
                                        // 2021-04-01
                                        // 對名稱加料，以免造成例外。
                                        // 還沒找到方法可以重複利用 ExcelPicture。
                                        ExcelPicture picture = worksheet4.Drawings
                                            .AddPicture($"row{startIdx3}_{badgeData.Label}",
                                            imageStream);

                                        int zeroBasedRow = startIdx3 - 1;

                                        picture.SetPosition(zeroBasedRow, 0, 0, 0);

                                        imageStream.Close();
                                        imageStream.Dispose();
                                        imageStream = null;
                                    }
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

                                IImage? image = stickerData.Image;

                                if (image != null)
                                {
                                    // 將 image 轉換成 stream。
                                    Stream? imageStream = null;

                                    try
                                    {
                                        imageStream = image.AsStream();
                                    }
                                    catch (Exception ex)
                                    {
                                        WriteLog($"無法將超級貼圖「{stickerData.Label}」轉換成 Stream。");
                                        WriteLog($"超級貼圖的網址：{stickerData.Url}");
                                        WriteLog($"發生錯誤：{ex}");
                                    }

                                    if (imageStream != null)
                                    {
                                        // 2021-04-01
                                        // 對名稱加料，以免造成例外。
                                        // 還沒找到方法可以重複利用 ExcelPicture。
                                        ExcelPicture picture = worksheet5.Drawings
                                            .AddPicture($"row{startIdx4}_{stickerData.Label}",
                                            imageStream);

                                        int zeroBasedRow = startIdx4 - 1;

                                        picture.SetPosition(zeroBasedRow, 0, 0, 0);
                                        // 欄寬為 0.98cm。
                                        // 20% : 2.7cm -> 1% : 0.135cm
                                        // 7.25% : 0.98cm
                                        picture.SetSize(7);

                                        imageStream.Close();
                                        imageStream.Dispose();
                                        imageStream = null;
                                    }
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
                        workbook.Properties.Subject = fileTitle;
                        workbook.Properties.Category = StringSet.SheetName1;
                        workbook.Properties.Keywords = $"{Rubujo.YouTube.Utility.Sets.StringSet.YouTube}, {StringSet.SheetName1}";
                        workbook.Properties.Author = $"{StringSet.AppName} {version}";
                        workbook.Properties.Comments = comments;
                        workbook.Properties.Company = string.Empty;

                        package.SaveAs(fileStream);
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"發生錯誤：{ex}");
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
                        string channelUrl = SharedLiveChatCatcher
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

        TBInterval.InvokeIfRequired(() =>
        {
            // 預設 5 秒。
            TBInterval.Text = "5";
        });

        // 從 Settings 中讀取值。
        IsStreaming = Properties.Settings.Default.IsStreaming;

        RBtnStreaming.InvokeIfRequired(() =>
        {
            RBtnStreaming.Checked = Properties.Settings.Default.IsStreaming;
        });

        RBtnReplay.InvokeIfRequired(() =>
        {
            RBtnReplay.Checked = !Properties.Settings.Default.IsStreaming;
        });

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

        CBLoadCookies.InvokeIfRequired(() =>
        {
            // 載入載入 Cookies 設定值。
            CBLoadCookies.Checked = Properties.Settings.Default.LoadCookies;
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
                            string errorMessage = await stickerData.SetImage(SharedHttpClient, FetchLargePicture);

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
                                string errorMessage = await emojiData.SetImage(SharedHttpClient, FetchLargePicture);

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
                            string errorMessage = await badgeData.SetImage(SharedHttpClient, FetchLargePicture);

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
                    rendererData.AuthorName != Rubujo.YouTube.Utility.Sets.StringSet.NoAuthorName) ?
                    rendererData.AuthorName :
                    string.Empty;
                string authorBages = (rendererData.AuthorBadges != null &&
                    rendererData.AuthorBadges != Rubujo.YouTube.Utility.Sets.StringSet.NoAuthorBadges) ?
                    rendererData.AuthorBadges :
                    string.Empty;
                string authorPhotoUrl = (rendererData.AuthorPhotoUrl != null &&
                    rendererData.AuthorPhotoUrl != Rubujo.YouTube.Utility.Sets.StringSet.NoAuthorPhotoUrl) ?
                    rendererData.AuthorPhotoUrl :
                    string.Empty;
                string messageContent = (rendererData.MessageContent != null &&
                    rendererData.MessageContent != Rubujo.YouTube.Utility.Sets.StringSet.NoMessageContent) ?
                    rendererData.MessageContent :
                    string.Empty;
                string purchaseAmountText = (rendererData.PurchaseAmountText != null &&
                    rendererData.PurchaseAmountText != Rubujo.YouTube.Utility.Sets.StringSet.NoPurchaseAmountText) ?
                    rendererData.PurchaseAmountText :
                    string.Empty;
                string timestampUsec = rendererData.TimestampUsec ?? string.Empty;
                string type = rendererData.Type ?? string.Empty;
                string backgroundColor = (rendererData.BackgroundColor != null &&
                    rendererData.BackgroundColor != Rubujo.YouTube.Utility.Sets.StringSet.NoBackgroundColor) ?
                    rendererData.BackgroundColor :
                    string.Empty;
                // 直播不會有，只有重播才會有。
                string timestampText = (rendererData.TimestampText != null &&
                    rendererData.TimestampText != Rubujo.YouTube.Utility.Sets.StringSet.NoTimestampText) ?
                    rendererData.TimestampText :
                    string.Empty;
                string authorExternalChannelID = (rendererData.AuthorExternalChannelID != null &&
                    rendererData.AuthorExternalChannelID != Rubujo.YouTube.Utility.Sets.StringSet.NoAuthorExternalChannelID) ?
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

                if (!string.IsNullOrEmpty(backgroundColor))
                {
                    foreach (ListViewItem.ListViewSubItem item in lvItem.SubItems)
                    {
                        item.BackColor = ColorTranslator.FromHtml(backgroundColor);
                    }
                }

                if (type == Rubujo.YouTube.Utility.Sets.StringSet.ChatJoinMember ||
                    type == Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberUpgrade ||
                    type == Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberMilestone ||
                    type == Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberGift ||
                    type == Rubujo.YouTube.Utility.Sets.StringSet.ChatReceivedMemberGift)
                {
                    foreach (ListViewItem.ListViewSubItem item in lvItem.SubItems)
                    {
                        item.ForeColor = Color.White;
                        item.BackColor = Color.Green;
                    }
                }

                if (type == Rubujo.YouTube.Utility.Sets.StringSet.ChatRedirect ||
                    type == Rubujo.YouTube.Utility.Sets.StringSet.ChatPinned)
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
            WriteLog($"發生錯誤：{ex}");
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
                RunLongTask();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using ExcelPackage package = new(filePath);

                string comments = package.Workbook.Properties.Comments;

                if (!string.IsNullOrEmpty(comments))
                {
                    if (Uri.IsWellFormedUriString(comments, UriKind.Absolute))
                    {
                        TBVideoID.InvokeIfRequired(() =>
                        {
                            TBVideoID.Text = comments;
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
                    string authorPhotoUrl = sheet1.Cells[rowIdx1, 10].Text;
                    string messageContent = sheet1.Cells[rowIdx1, 4].Text;
                    string purchaseAmmount = sheet1.Cells[rowIdx1, 5].Text;
                    string timestampUsec = sheet1.Cells[rowIdx1, 6].Text;
                    string type = sheet1.Cells[rowIdx1, 7].Text;
                    string backgroundColor = sheet1.Cells[rowIdx1, 8].Text;
                    string timestampText = sheet1.Cells[rowIdx1, 9].Text;
                    string authorExternalChannelID = sheet1.Cells[rowIdx1, 11].Text;
                    string id = sheet1.Cells[rowIdx1, 12].Text;

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

                    if (!string.IsNullOrEmpty(backgroundColor))
                    {
                        foreach (ListViewItem.ListViewSubItem item in lvItem.SubItems)
                        {
                            item.BackColor = ColorTranslator.FromHtml(backgroundColor);
                        }
                    }

                    if (type == Rubujo.YouTube.Utility.Sets.StringSet.ChatJoinMember ||
                       type == Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberUpgrade ||
                       type == Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberMilestone ||
                       type == Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberGift ||
                       type == Rubujo.YouTube.Utility.Sets.StringSet.ChatReceivedMemberGift)
                    {
                        foreach (ListViewItem.ListViewSubItem item in lvItem.SubItems)
                        {
                            item.ForeColor = Color.White;
                            item.BackColor = Color.Green;
                        }
                    }

                    if (type == Rubujo.YouTube.Utility.Sets.StringSet.ChatRedirect ||
                        type == Rubujo.YouTube.Utility.Sets.StringSet.ChatPinned)
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
                    LVLiveChatList.Items.AddRange(listTempItem.ToArray());
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
                                    string errorMessage = await emojiData.SetImage(SharedHttpClient, FetchLargePicture);

                                    if (!string.IsNullOrEmpty(errorMessage))
                                    {
                                        WriteLog(errorMessage);
                                    };

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
                                string errorMessage = await badgeData.SetImage(SharedHttpClient, FetchLargePicture);

                                if (!string.IsNullOrEmpty(errorMessage))
                                {
                                    WriteLog(errorMessage);
                                };

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
                                string errorMessage = await stickerData.SetImage(SharedHttpClient, FetchLargePicture);

                                if (!string.IsNullOrEmpty(errorMessage))
                                {
                                    WriteLog(errorMessage);
                                };

                                SharedStickers.Add(stickerData);
                            }
                        }

                        rowIdx4++;
                    }

                    WriteLog($"已匯入 {SharedStickers.Count} 個超級貼圖資料。");
                }

                #endregion
            }
            catch (Exception ex)
            {
                WriteLog($"發生錯誤：{ex}");
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
                (n.SubItems[5].Text == Rubujo.YouTube.Utility.Sets.StringSet.ChatSuperChat ||
                n.SubItems[5].Text == Rubujo.YouTube.Utility.Sets.StringSet.ChatSuperSticker) &&
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
                n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.ChatJoinMember &&
                n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberUpgrade &&
                n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberMilestone &&
                n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberGift &&
                n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.ChatReceivedMemberGift &&
                n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.ChatRedirect &&
                n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.ChatPinned)
                .Count();

            LChatCount.Text = $"留言數量：{count} 個";
        });

        LSuperChatCount.InvokeIfRequired(() =>
        {
            int count = dataSet.Where(n => n.SubItems[5].Text == Rubujo.YouTube.Utility.Sets.StringSet.ChatSuperChat).Count();

            LSuperChatCount.Text = $"{Rubujo.YouTube.Utility.Sets.StringSet.ChatSuperChat}：{count} 個";
        });

        LSuperStickerCount.InvokeIfRequired(() =>
        {
            int count = dataSet.Where(n => n.SubItems[5].Text == Rubujo.YouTube.Utility.Sets.StringSet.ChatSuperSticker).Count();

            LSuperStickerCount.Text = $"{Rubujo.YouTube.Utility.Sets.StringSet.ChatSuperSticker}：{count} 個";
        });

        LMemberJoinCount.InvokeIfRequired(() =>
        {
            int joinCount = dataSet.Where(n => n.SubItems[5].Text == Rubujo.YouTube.Utility.Sets.StringSet.ChatJoinMember).Count();
            int upgradeCount = dataSet.Where(n => n.SubItems[5].Text == Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberUpgrade).Count();
            int milestoneCount = dataSet.Where(n => n.SubItems[5].Text == Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberMilestone).Count();
            int giftCount = dataSet.Where(n => n.SubItems[5].Text == Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberGift).Count();
            int receivedGiftCount = dataSet.Where(n => n.SubItems[5].Text == Rubujo.YouTube.Utility.Sets.StringSet.ChatReceivedMemberGift).Count();

            LMemberJoinCount.Text = $"{Rubujo.YouTube.Utility.Sets.StringSet.ChatJoinMember}：{joinCount} 位";

            string tooltip = $"{Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberUpgrade}：{upgradeCount} 位、" +
                $"{Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberMilestone}：{milestoneCount} 位、" +
                $"{Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberGift}：{giftCount} 位、" +
                $"{Rubujo.YouTube.Utility.Sets.StringSet.ChatReceivedMemberGift}：{receivedGiftCount} 位";

            SharedTooltip.SetToolTip(LMemberJoinCount, tooltip);
        });

        LMemberInRoomCount.InvokeIfRequired(() =>
        {
            int count = dataSet.Where(n => n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.ChatJoinMember &&
                n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberUpgrade &&
                n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.ChatMemberMilestone &&
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
        RBtnStreaming.InvokeIfRequired(() =>
        {
            RBtnStreaming.Enabled = enable;
        });

        RBtnReplay.InvokeIfRequired(() =>
        {
            RBtnReplay.Enabled = enable;
        });

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

        CBRandomInterval.InvokeIfRequired(() =>
        {
            CBRandomInterval.Enabled = enable;

            TBInterval.InvokeIfRequired(() =>
            {
                TBInterval.Enabled = !CBRandomInterval.Checked;

                if (!enable)
                {
                    TBInterval.Enabled = false;
                }
            });
        });

        TBUserAgent.InvokeIfRequired(() =>
        {
            TBUserAgent.Enabled = enable;
        });


        CBLoadCookies.InvokeIfRequired(() =>
        {
            CBLoadCookies.Enabled = enable;
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
    /// 設定使用 Coookies
    /// </summary>
    private void SetUseCookies()
    {
        bool enable = false;

        CBLoadCookies.InvokeIfRequired(() =>
        {
            enable = CBLoadCookies.Checked;
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
        SharedLiveChatCatcher.UseCookies(
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

        SharedLiveChatCatcher.Init(
            httpClient: httpClient,
            isStreaming: IsStreaming);
        SharedLiveChatCatcher.FetchLargePicture(true);
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