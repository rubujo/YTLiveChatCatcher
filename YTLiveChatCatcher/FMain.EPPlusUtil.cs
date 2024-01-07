using Color = System.Drawing.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using Microsoft.Maui.Graphics;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Drawing.Chart.Style;
using OfficeOpenXml.Style.XmlAccess;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Sets;
using StringSet = YTLiveChatCatcher.Common.Sets.StringSet;
using YTLiveChatCatcher.Common;
using YTLiveChatCatcher.Extensions;
using Rubujo.YouTube.Utility.Models.LiveChat;

namespace YTLiveChatCatcher;

// 阻擋設計工具。
partial class DesignerBlocker { };

/// <summary>
/// FMain 的 EPPlus 工具
/// </summary>
public partial class FMain
{
    /// <summary>
    /// 建立 *.xlsx 檔案
    /// </summary>
    /// <param name="control1">SaveFileDialog</param>
    /// <param name="control2">ListView</param>
    /// <param name="videoID">字串，影片的 ID 值</param>
    public void CreateXLSX(
        SaveFileDialog control1,
        ListView control2,
        string videoID)
    {
        if (control2.Name == LVLiveChatList.Name)
        {
            RunLongTask();
        }

        using FileStream fileStream = (FileStream)control1.OpenFile();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using ExcelPackage package = new();

        double[] widthSet = [5.0, 20.0, 24.0, 50.0, 14.0, 27.0, 16.0, 20.0, 20.0, 20.0, 20.0, 20.0, 0.0];

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

        for (int i = 0; i < control2.Columns.Count; i++)
        {
            ColumnHeader header = control2.Columns[i];

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

        control2.InvokeIfRequired(() =>
        {
            IEnumerable<ListViewItem> dataSet = control2.GetListViewItems()
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
                    Image? image = control2.SmallImageList?.Images[listViewItem.ImageKey];

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
        });

        // 只有 LVLiveChatList 可以匯出統計資訊，
        // 因為統計資訊的資料不是從 Excel 的內容直接產生的。
        if (control2.Name == LVLiveChatList.Name)
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
                $"SUM(COUNTIF(G:G,{{\"{YTJsonParser.GetLocalizeString(KeySet.ChatGeneral)}\", " +
                $"\"{YTJsonParser.GetLocalizeString(KeySet.ChatSuperChat)}\"," +
                $"\"{YTJsonParser.GetLocalizeString(KeySet.ChatSuperSticker)}\"}}))&\" 個\"",
                $"COUNTIF(G:G,\"{YTJsonParser.GetLocalizeString(KeySet.ChatSuperChat)}\")&\" 個\"",
                $"COUNTIF(G:G,\"{YTJsonParser.GetLocalizeString(KeySet.ChatSuperSticker)}\")&\" 個\"",
                $"COUNTIF(G:G,\"{YTJsonParser.GetLocalizeString(KeySet.ChatJoinMember)}\")&\" 個\"",
                $"COUNTIF(G:G,\"{YTJsonParser.GetLocalizeString(KeySet.ChatMemberUpgrade)}\")&\" 個\"",
                $"COUNTIF(G:G,\"{YTJsonParser.GetLocalizeString(KeySet.ChatMemberMilestone)}\")&\" 個\"",
                $"COUNTIF(G:G,\"{YTJsonParser.GetLocalizeString(KeySet.ChatMemberGift)}\")&\" 個\"",
                $"COUNTIF(G:G,\"{YTJsonParser.GetLocalizeString(KeySet.ChatReceivedMemberGift)}\")&\" 個\""
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
                // 2022-05-30 改為使用固定寬度。
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

        #endregion

        #region 時間熱點

        control2.InvokeIfRequired(() =>
        {
            // 參考 1：https://stackoverflow.com/a/687347
            // 參考 2：https://stackoverflow.com/a/687370

            // 排除在影片開始前的時間點。
            Dictionary<string, int> sourceList = control2
                .GetListViewItems()
                .Where(n => n.SubItems[5].Text != StringSet.AppName &&
                    n.SubItems[5].Text != Rubujo.YouTube.Utility.Sets.StringSet.YouTube &&
                    // 2022/10/25 因不容易轉換成影片對應時間點，故而直接排除
                    // LiveChatCatcher.GetLocalizeString(KeySet.ChatMemberGift)、
                    // LiveChatCatcher.GetLocalizeString(KeySet.ChatReceivedMemberGift) 等類型的資料，
                    // 以免在時間熱點活頁簿內出現奇怪的時間點。
                    n.SubItems[5].Text != YTJsonParser.GetLocalizeString(KeySet.ChatMemberGift) &&
                    n.SubItems[5].Text != YTJsonParser.GetLocalizeString(KeySet.ChatReceivedMemberGift) &&
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
                string sheetName = YTJsonParser.IsStreaming() ?
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
                        WriteLog($"發生錯誤：{ex.GetExceptionMessage()}");
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
                        WriteLog($"發生錯誤：{ex.GetExceptionMessage()}");
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
                        WriteLog($"發生錯誤：{ex.GetExceptionMessage()}");
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
            fileTitle = Path.GetFileNameWithoutExtension(control1.FileName),
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
        workbook.Properties.Company = "DD們的避難所";

        package.SaveAs(fileStream);
    }

    /// <summary>
    /// 載入 *.xlsx 檔案
    /// </summary>
    /// <param name="filePath">字串，*.xlsx 檔案的路徑</param>
    /// <returns>Task</returns>
    public async Task LoadXLSX(string filePath)
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
                            string errorMessage = await emojiData.SetImage(
                                SharedHttpClient,
                                YTJsonParser.FetchLargePicture());

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
                        string errorMessage = await badgeData.SetImage(
                            SharedHttpClient,
                            YTJsonParser.FetchLargePicture());

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
                        string errorMessage = await stickerData.SetImage(
                            SharedHttpClient,
                            YTJsonParser.FetchLargePicture());

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
}