using GetCachable;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Drawing.Chart.Style;
using OfficeOpenXml.Style;
using OfficeOpenXml.Style.XmlAccess;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.Versioning;
using YTApi;
using YTApi.Models;
using YTLiveChatCatcher.Common;
using YTLiveChatCatcher.Common.Sets;
using YTLiveChatCatcher.Common.Utils;
using YTLiveChatCatcher.Extensions;

namespace YTLiveChatCatcher;

// 阻擋設計工具。
partial class DesignerBlocker { };

/// <summary>
/// FMain 方法
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
        {
            new ColumnHeader()
            {
                Name = "AuthorName",
                Text = "作者名稱",
                TextAlign = HorizontalAlignment.Left,
                Width = 140,
                DisplayIndex = 0
            },
            new ColumnHeader()
            {
                Name = "AuthorBages",
                Text = "徽章",
                TextAlign = HorizontalAlignment.Left,
                Width = 100,
                DisplayIndex = 1
            },
            new ColumnHeader()
            {
                Name = "Message",
                Text = "訊息",
                TextAlign = HorizontalAlignment.Left,
                Width = 320,
                DisplayIndex = 2
            },
            new ColumnHeader()
            {
                Name = "PurchaseAmount",
                Text = "金額",
                TextAlign = HorizontalAlignment.Left,
                Width = 80,
                DisplayIndex = 3
            },
            new ColumnHeader()
            {
                Name = "TimestampUsec",
                Text = "時間",
                TextAlign = HorizontalAlignment.Center,
                Width = 150,
                DisplayIndex = 4
            },
            new ColumnHeader()
            {
                Name = "Type",
                Text = "類型",
                TextAlign = HorizontalAlignment.Center,
                Width = 100,
                DisplayIndex = 5
            },
            new ColumnHeader()
            {
                Name = "BackgroundColor",
                Text = "背景顏色",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 6
            },
            new ColumnHeader()
            {
                Name = "TimestampText",
                Text = "時間標記文字",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 7
            },
            new ColumnHeader()
            {
                Name = "AuthorPhotoUrl",
                Text = "頭像網址",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 8
            },
            new ColumnHeader()
            {
                Name = "AuthorExternalChannelID",
                Text = "外部頻道 ID",
                TextAlign = HorizontalAlignment.Center,
                // 設成 0，預設不直接顯示。
                Width = 0,
                DisplayIndex = 9
            }
        };

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

                if (type == StringSet.ChatGeneral ||
                    type == StringSet.ChatSuperChat ||
                    type == StringSet.ChatSuperSticker)
                {
                    speakText = $"{authorName}說{message}";
                }
                else
                {
                    if (type != StringSet.YouTube)
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
        if (listView.Items.Count > 0)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "Excel 活頁簿|*.xlsx",
                Title = "儲存檔案",
                FileName = $"{StringSet.SheetName1}_{DateTime.Now:yyyyMMdd}"
            };

            string userAgent = string.Empty;

            TBUserAgent.InvokeIfRequired(() =>
            {
                userAgent = TBUserAgent.Text;
            });

            // 取得 HttpClient。
            using HttpClient httpClient = HttpClientUtil.GetHttpClient(
                _httpClientFactory,
                userAgent);

            string videoID = string.Empty;

            TBVideoID.InvokeIfRequired(() =>
            {
                videoID = TBVideoID.Text.Trim();
            });

            // 取得影片的標題。
            string videoTitle = LiveChatFunction.GetVideoTitle(
                httpClient,
                videoID,
                GetCookies(),
                TBLog);

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

                            double[] widthSet = { 5.0, 20.0, 24.0, 50.0, 14.0, 27.0, 16.0, 20.0, 20.0, 20.0, 20.0 };

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
                                        n.SubItems[5].Text != StringSet.YouTube);

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
                                                // 2021-04-01
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

                                ExcelRange summaryHeaderRange = worksheet1.Cells[summaryIdx, 13, summaryIdx, 14];

                                summaryHeaderRange.Merge = true;
                                summaryHeaderRange.StyleName = "HeaderStyle";
                                summaryHeaderRange.Style.Fill.SetBackground(Color.BlanchedAlmond);
                                summaryHeaderRange.Value = "統計資訊";

                                summaryIdx++;

                                string[] tpLMemberJoinCounts = SharedTooltip.GetToolTip(LMemberJoinCount)
                                    .Split(new char[] { '、' }, StringSplitOptions.RemoveEmptyEntries);

                                List<string> arraySummaryInfo = new()
                                {
                                    LChatCount.Text,
                                    LSuperChatCount.Text,
                                    LSuperStickerCount.Text,
                                    LMemberJoinCount.Text,
                                };

                                foreach (string item in tpLMemberJoinCounts)
                                {
                                    arraySummaryInfo.Add(item);
                                }

                                arraySummaryInfo.AddRange(new List<string>
                                {
                                    LMemberInRoomCount.Text,
                                    LAuthorCount.Text,
                                    LTempIncome.Text,
                                });

                                string tpLTempIncome = SharedTooltip.GetToolTip(LTempIncome);

                                if (!string.IsNullOrEmpty(tpLTempIncome))
                                {
                                    arraySummaryInfo.Add(tpLTempIncome);
                                }

                                // 設定預設寬度。
                                worksheet1.Column(13).Width = 15.0;

                                for (int i = 0; i < arraySummaryInfo.Count; i++)
                                {
                                    string[] arrayInfo = arraySummaryInfo[i].Split(new char[] { '：' },
                                        StringSplitOptions.RemoveEmptyEntries);

                                    ExcelRange summaryTitleRange = worksheet1.Cells[summaryIdx, 13];

                                    summaryTitleRange.StyleName = "HeaderStyle";
                                    summaryTitleRange.Style.Font.Bold = false;
                                    summaryTitleRange.Value = arrayInfo[0];
                                    // 2022-05-30 改為使用固定寬度。
                                    //summaryTitleRange.AutoFitColumns();

                                    ExcelRange summaryContentRange = worksheet1.Cells[summaryIdx, 14];

                                    summaryContentRange.StyleName = "ContentStyle";
                                    summaryContentRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                    summaryContentRange.Value = arrayInfo[1];
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
                                        n.SubItems[5].Text != StringSet.YouTube &&
                                        // 2022-10-25 因不容易轉換成影片對應時間點，故而直接排除
                                        // StringSet.ChatMemberGift、StringSet.ChatReceivedMemberGift 等類型的資料，
                                        // 以免在時間熱點活頁簿內出現奇怪的時間點。
                                        n.SubItems[5].Text != StringSet.ChatMemberGift &&
                                        n.SubItems[5].Text != StringSet.ChatReceivedMemberGift &&
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
                                    headerFirstRange2.Value = "分鐘";
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

                                    // TODO: 2022-07-10 需要再觀察圖表沒有在指定位置的情況。
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

                                    Image? image = emojiData.Image;

                                    if (image != null)
                                    {
                                        // 將 image 轉換成 stream。
                                        Stream? imageStream = null;

                                        try
                                        {
                                            imageStream = image.ToStream();
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

                                    Image? image = badgeData.Image;

                                    if (image != null)
                                    {
                                        // 將 image 轉換成 stream。
                                        Stream? imageStream = null;

                                        try
                                        {
                                            imageStream = image.ToStream();
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

                            string fileTitle = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                            string comments = string.Empty;

                            if (!string.IsNullOrEmpty(videoID))
                            {
                                comments = $"https://www.youtube.com/watch?v={videoID}";
                            }

                            workbook.Properties.Title = fileTitle;
                            workbook.Properties.Subject = fileTitle;
                            workbook.Properties.Category = string.Empty;
                            workbook.Properties.Keywords = string.Empty;
                            workbook.Properties.Author = StringSet.AppName;
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
                        TerminateLongTask(isImport: false);
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
        else
        {
            MessageBox.Show(
                "匯出失敗，請先確認聊天室內容是否有資料。",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
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
                        string channelUrl = $"{StringSet.Origin}/channel/{authorExternalChannelId}";

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

        LVersion.InvokeIfRequired(() =>
        {
            Version? version = Assembly.GetExecutingAssembly().GetName().Version;

            string verText = version != null ? $"v{version}" : "無";

            // 設定版本號顯示。
            LVersion.Text = $"版本號：{verText}";
        });

        CBEnableDebug.InvokeIfRequired(() =>
        {
            // 載入啟用輸出錯誤資訊的設定值。
            CBEnableDebug.Checked = Properties.Settings.Default.EnableDebug;
        });

        // 設定提示。
        SharedTooltip.SetToolTip(TBInterval, "僅在未勾選「隨機間隔秒數」以及為「重播」時會生效。");

        // 設定控制項的狀態。
        SetControlsState(true);
    }

    /// <summary>
    /// 初始化 SharedTimer
    /// </summary>
    private void InitTimer()
    {
        SharedTimer = new();
        SharedTimer.Tick += (object? sender, EventArgs e) =>
        {
            if (SharedTimer.Enabled)
            {
                WriteLog($"每 {SharedTimer.Interval / 1000} 秒滴嗒一次！");

                int interval = 0;

                CBRandomInterval.InvokeIfRequired(() =>
                {
                    if (CBRandomInterval.Checked)
                    {
                        interval = CustomFunction.GetRandomInterval();

                        SharedTimer.Interval = interval;
                    }
                    else
                    {
                        // 在 "直播" 時遵守 YouTube 的 "timeoutMs" 限制。
                        if (IsStreaming && SharedTimeoutMs != 0)
                        {
                            SharedTimer.Interval = SharedTimeoutMs;
                        }

                        interval = SharedTimer.Interval;
                    }
                });

                TBInterval.InvokeIfRequired(() =>
                {
                    TBInterval.Text = (interval / 1000).ToString();
                });

                GetLiveChatData();
            }
        };
    }

    /// <summary>
    /// 取得影片聊天室的內容
    /// </summary>
    private void GetLiveChatData()
    {
        SharedCancellationTokenSource = new();
        SharedCancellationToken = SharedCancellationTokenSource.Token;

        SharedTask = Task.Run(() =>
        {
            try
            {
                if (SharedCancellationToken.HasValue &&
                    !SharedCancellationToken.Value.IsCancellationRequested)
                {
                    if (SharedYTConfig != null)
                    {
                        string userAgent = string.Empty;

                        TBUserAgent.InvokeIfRequired(() =>
                        {
                            userAgent = TBUserAgent.Text;
                        });

                        // 取得 HttpClient。
                        using HttpClient httpClient = HttpClientUtil.GetHttpClient(
                            _httpClientFactory,
                            userAgent);

                        SharedJsonElement = LiveChatFunction.GetJsonElement(
                            httpClient,
                            SharedYTConfig,
                            IsStreaming,
                            GetCookies(),
                            TBLog);

                        // 判斷是否有取得有效的內容。
                        if (!string.IsNullOrEmpty(SharedJsonElement.ToString()))
                        {
                            // 2021-12-10
                            // 可能會因為 BtnStop_Click() 造成 SharedYTConfig 為 null。
                            // 再次檢查 SharedYTConfig 是否為 null。
                            if (SharedYTConfig != null)
                            {
                                // 0：continuation、1：timeoutMs。
                                string[] continuationData = JsonParser.GetContinuation(SharedJsonElement);

                                // 更換 Continuation。
                                SharedYTConfig.Continuation = continuationData[0];

                                if (int.TryParse(continuationData[1], out int timeoutMs))
                                {
                                    SharedTimeoutMs = timeoutMs;

                                    WriteLog($"接收到的 timeoutMs：{timeoutMs}");
                                }
                            }

                            List<ListViewItem> listTempItem = new();

                            bool isLarge = true;

                            List<RendererData> messages = JsonParser.GetActions(SharedJsonElement, isLarge);

                            foreach (RendererData rendererData in messages)
                            {
                                if (rendererData.Emojis != null)
                                {
                                    foreach (EmojiData emojiData in rendererData.Emojis)
                                    {
                                        // 只處理自定義表情符號的資料。
                                        if (!SharedCustomEmojis.Any(n => n.ID == emojiData.ID) &&
                                            emojiData.IsCustomEmoji)
                                        {
                                            if (!string.IsNullOrEmpty(emojiData.Url))
                                            {
                                                // 以 emojiData.ID 為鍵值，將 Image 暫存 10 分鐘。
                                                Image? image = BetterCacheManager.GetCachableData(emojiData.ID!, () =>
                                                {
                                                    try
                                                    {
                                                        // 取得 HttpClient。
                                                        using HttpClient httpClient = HttpClientUtil
                                                            .GetHttpClient(_httpClientFactory, userAgent);

                                                        byte[] bytes = httpClient.GetByteArrayAsync(emojiData.Url).Result;

                                                        using MemoryStream memoryStream = new(bytes);

                                                        return Image.FromStream(memoryStream);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        WriteLog($"無法下載自定義表情符號「{emojiData.Label}」。");
                                                        WriteLog($"自定義表情符號的網址：{emojiData.Url}");
                                                        WriteLog($"發生錯誤：{e.Message}");

                                                        // 當 isLarge 的值為 true 時，建立一個 48x48 的白色 Bitmap。
                                                        Bitmap bitmap = isLarge ? new(24, 24) : new(48, 48);

                                                        using (Graphics graphics = Graphics.FromImage(bitmap))
                                                        {
                                                            graphics.Clear(Color.FromKnownColor(KnownColor.White));
                                                        }

                                                        return bitmap;
                                                    }
                                                }, 10);

                                                emojiData.Image = image;
                                                emojiData.Format = image.RawFormat.ToString();
                                            }

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
                                            !SharedBadges.Any(n => n.Label == badgeData.Label) &&
                                            badgeData.Label.Contains(StringSet.Member))
                                        {
                                            if (!string.IsNullOrEmpty(badgeData.Url))
                                            {
                                                // 以 badgeData.Label 為鍵值，將 Image 暫存 10 分鐘。
                                                Image? image = BetterCacheManager.GetCachableData(badgeData.Label!, () =>
                                                {
                                                    try
                                                    {
                                                        // 取得 HttpClient。
                                                        using HttpClient httpClient = HttpClientUtil
                                                            .GetHttpClient(_httpClientFactory, userAgent);

                                                        byte[] bytes = httpClient.GetByteArrayAsync(badgeData.Url).Result;

                                                        using MemoryStream memoryStream = new(bytes);

                                                        return Image.FromStream(memoryStream);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        WriteLog($"無法下載會員徽章「{badgeData.Label}」。");
                                                        WriteLog($"會員徽章的網址：{badgeData.Url}");
                                                        WriteLog($"發生錯誤：{e.Message}");

                                                        // 當 isLarge 的值為 true 時，建立一個 32x32 的白色 Bitmap。
                                                        Bitmap bitmap = isLarge ? new(16, 16) : new(32, 32);

                                                        using (Graphics graphics = Graphics.FromImage(bitmap))
                                                        {
                                                            graphics.Clear(Color.FromKnownColor(KnownColor.White));
                                                        }

                                                        return bitmap;
                                                    }
                                                }, 10);

                                                badgeData.Image = image;
                                                badgeData.Format = image.RawFormat.ToString();
                                            }

                                            SharedBadges.Add(badgeData);
                                        }
                                    }
                                }

                                string authorName = (rendererData.AuthorName != null &&
                                    rendererData.AuthorName != StringSet.NoAuthorName) ?
                                    rendererData.AuthorName :
                                    string.Empty;

                                string authorBages = (rendererData.AuthorBadges != null &&
                                    rendererData.AuthorBadges != StringSet.NoAuthorBadges) ?
                                    rendererData.AuthorBadges :
                                    string.Empty;

                                string authorPhotoUrl = (rendererData.AuthorPhotoUrl != null &&
                                    rendererData.AuthorPhotoUrl != StringSet.NoAuthorPhotoUrl) ?
                                    rendererData.AuthorPhotoUrl :
                                    string.Empty;

                                string messageContent = (rendererData.MessageContent != null &&
                                    rendererData.MessageContent != StringSet.NoMessageContent) ?
                                    rendererData.MessageContent :
                                    string.Empty;

                                string purchaseAmountText = (rendererData.PurchaseAmountText != null &&
                                    rendererData.PurchaseAmountText != StringSet.NoPurchaseAmountText) ?
                                    rendererData.PurchaseAmountText :
                                    string.Empty;

                                string timestampUsec = rendererData.TimestampUsec ?? string.Empty;

                                string type = rendererData.Type ?? string.Empty;

                                string backgroundColor = (rendererData.BackgroundColor != null &&
                                    rendererData.BackgroundColor != StringSet.NoBackgroundColor) ?
                                    rendererData.BackgroundColor :
                                    string.Empty;

                                // 直播不會有，只有重播才會有。
                                string timestampText = (rendererData.TimestampText != null &&
                                    rendererData.TimestampText != StringSet.NoTimestampText) ?
                                    rendererData.TimestampText :
                                    string.Empty;

                                string authorExternalChannelID = (rendererData.AuthorExternalChannelID != null &&
                                    rendererData.AuthorExternalChannelID != StringSet.NoAuthorExternalChannelID) ?
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

                                string[] subItemContents = new string[9];

                                subItemContents[0] = authorBages;
                                subItemContents[1] = messageContent;
                                subItemContents[2] = purchaseAmountText;
                                subItemContents[3] = timestampUsec;
                                subItemContents[4] = type;
                                subItemContents[5] = backgroundColor;
                                subItemContents[6] = timestampText;
                                subItemContents[7] = authorPhotoUrl;
                                subItemContents[8] = authorExternalChannelID;

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

                                lvItem.SubItems.AddRange(subItemContents);

                                if (authorName == $"[{StringSet.YouTube}]" ||
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

                                if (type == StringSet.ChatJoinMember ||
                                    type == StringSet.ChatMemberUpgrade ||
                                    type == StringSet.ChatMemberMilestone ||
                                    type == StringSet.ChatMemberGift ||
                                    type == StringSet.ChatReceivedMemberGift)
                                {
                                    foreach (ListViewItem.ListViewSubItem item in lvItem.SubItems)
                                    {
                                        item.ForeColor = Color.White;
                                        item.BackColor = Color.Green;
                                    }
                                }

                                if (!string.IsNullOrEmpty(authorPhotoUrl))
                                {
                                    LVLiveChatList.InvokeIfRequired(() =>
                                    {
                                        string imgKey = authorName;

                                        if (LVLiveChatList.SmallImageList != null &&
                                            !LVLiveChatList.SmallImageList.Images.ContainsKey(imgKey))
                                        {
                                            // 以 imgKey 為鍵值，將 Image 暫存 10 分鐘。
                                            Image? image = BetterCacheManager.GetCachableData(imgKey, () =>
                                            {
                                                try
                                                {
                                                    // 取得 HttpClient。
                                                    using HttpClient httpClient = HttpClientUtil
                                                        .GetHttpClient(_httpClientFactory, userAgent);

                                                    byte[] bytes = httpClient.GetByteArrayAsync(authorPhotoUrl).Result;

                                                    using MemoryStream memoryStream = new(bytes);

                                                    return Image.FromStream(memoryStream);
                                                }
                                                catch (Exception e)
                                                {
                                                    WriteLog($"發生錯誤：{e.Message}");
                                                    WriteLog($"無法下載「{imgKey}」的頭像。");
                                                    WriteLog($"頭像的網址：{authorPhotoUrl}");

                                                    // 當 isLarge 的值為 true 時，建立一個 64x64 的白色 Bitmap。
                                                    Bitmap bitmap = isLarge ? new(64, 64) : new(32, 32);

                                                    using (Graphics graphics = Graphics.FromImage(bitmap))
                                                    {
                                                        graphics.Clear(Color.FromKnownColor(KnownColor.White));
                                                    }

                                                    return bitmap;
                                                }
                                            }, 10);

                                            LVLiveChatList.SmallImageList.Images.Add(imgKey, image);

                                            image.Dispose();
                                            image = null;
                                        }

                                        lvItem.ImageKey = imgKey;
                                    });
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
                        else
                        {
                            WriteLog("無法取得聊天室的內容。");

                            // 手動觸發 BtnStop 的 Click 事件。
                            BtnStop_Click(null, new EventArgs());
                        }
                    }
                    else
                    {
                        // 手動觸發 BtnStop 的 Click 事件。
                        BtnStop_Click(null, new EventArgs());
                    }
                }
                else
                {
                    SharedTask = null;
                    SharedCancellationToken = null;
                    SharedCancellationTokenSource = new();
                }
            }
            catch (Exception ex)
            {
                WriteLog($"發生錯誤：{ex}");

                // 手動觸發 BtnStop 的 Click 事件。
                BtnStop_Click(null, new EventArgs());
            }
        }, SharedCancellationTokenSource.Token);
    }

    /// <summary>
    /// 執行匯入資料
    /// </summary>
    /// <param name="filePath">字串，欲匯入檔案的路徑</param>
    private void DoImportData(string filePath)
    {
        Task.Run(() =>
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

                string userAgent = string.Empty;

                TBUserAgent.InvokeIfRequired(() =>
                {
                    userAgent = TBUserAgent.Text;
                });

                #region 聊天室記錄

                ExcelWorksheet? sheet1 = package.Workbook.Worksheets
                    .FirstOrDefault(n => n.Name == StringSet.SheetName1);

                if (sheet1 != null)
                {
                    List<ListViewItem> listTempItem = new();

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

                        // 當 "type" 為 null 或空值時，直接進入下一個。
                        if (string.IsNullOrEmpty(type))
                        {
                            continue;
                        }

                        ListViewItem lvItem = new(authorName)
                        {
                            UseItemStyleForSubItems = false
                        };

                        string[] subItemContents = new string[9];

                        subItemContents[0] = authorBages;
                        subItemContents[1] = messageContent;
                        subItemContents[2] = purchaseAmmount;
                        subItemContents[3] = timestampUsec;
                        subItemContents[4] = type;
                        subItemContents[5] = backgroundColor;
                        subItemContents[6] = timestampText;
                        subItemContents[7] = authorPhotoUrl;
                        subItemContents[8] = authorExternalChannelID;

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

                        lvItem.SubItems.AddRange(subItemContents);

                        if (authorName == $"[{StringSet.YouTube}]" ||
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

                        if (type == StringSet.ChatJoinMember ||
                           type == StringSet.ChatMemberUpgrade ||
                           type == StringSet.ChatMemberMilestone ||
                           type == StringSet.ChatMemberGift ||
                           type == StringSet.ChatReceivedMemberGift)
                        {
                            foreach (ListViewItem.ListViewSubItem item in lvItem.SubItems)
                            {
                                item.ForeColor = Color.White;
                                item.BackColor = Color.Green;
                            }
                        }

                        if (!string.IsNullOrEmpty(authorPhotoUrl))
                        {
                            LVLiveChatList.InvokeIfRequired(() =>
                            {
                                string imgKey = authorName;

                                if (LVLiveChatList.SmallImageList != null &&
                                    !LVLiveChatList.SmallImageList.Images.ContainsKey(imgKey))
                                {
                                    // 以 imgKey 為鍵值，將 Image 暫存 10 分鐘。
                                    Image? image = BetterCacheManager.GetCachableData(imgKey, () =>
                                    {
                                        try
                                        {
                                            // 取得 HttpClient。
                                            using HttpClient httpClient = HttpClientUtil
                                                .GetHttpClient(_httpClientFactory, userAgent);

                                            byte[] bytes = httpClient.GetByteArrayAsync(authorPhotoUrl).Result;

                                            using MemoryStream memoryStream = new(bytes);

                                            return Image.FromStream(memoryStream);
                                        }
                                        catch (Exception e)
                                        {
                                            WriteLog($"發生錯誤：{e.Message}");
                                            WriteLog($"無法下載「{imgKey}」的頭像。");
                                            WriteLog($"頭像的網址：{authorPhotoUrl}");

                                            // 建立一個 64x64 的白色 Bitmap。
                                            Bitmap bitmap = new(64, 64);

                                            using (Graphics graphics = Graphics.FromImage(bitmap))
                                            {
                                                graphics.Clear(Color.FromKnownColor(KnownColor.White));
                                            }

                                            return bitmap;
                                        }
                                    }, 10);

                                    LVLiveChatList.SmallImageList.Images.Add(imgKey, image);

                                    image.Dispose();
                                    image = null;
                                }

                                lvItem.ImageKey = imgKey;
                            });
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
                }
                else
                {
                    MessageBox.Show(
                        "匯入失敗，請選擇有效檔案。",
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

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

                            if (!SharedCustomEmojis.Any(n => n.ID == emojiData.ID) &&
                                emojiData.IsCustomEmoji)
                            {
                                if (!string.IsNullOrEmpty(emojiData.Url))
                                {
                                    // 以 emojiData.ID 為鍵值，將 Image 暫存 10 分鐘。
                                    Image? image = BetterCacheManager.GetCachableData(emojiData.ID, () =>
                                    {
                                        try
                                        {
                                            // 取得 HttpClient。
                                            using HttpClient httpClient = HttpClientUtil
                                                .GetHttpClient(_httpClientFactory, userAgent);

                                            byte[] bytes = httpClient.GetByteArrayAsync(emojiData.Url).Result;

                                            using MemoryStream memoryStream = new(bytes);

                                            return Image.FromStream(memoryStream);
                                        }
                                        catch (Exception e)
                                        {
                                            WriteLog($"無法下載自定義表情符號「{emojiData.Label}」。");
                                            WriteLog($"自定義表情符號的網址：{emojiData.Url}");
                                            WriteLog($"發生錯誤：{e.Message}");

                                            // 建立一個 48x48 的白色 Bitmap。
                                            Bitmap bitmap = new(48, 48);

                                            using (Graphics graphics = Graphics.FromImage(bitmap))
                                            {
                                                graphics.Clear(Color.FromKnownColor(KnownColor.White));
                                            }

                                            return bitmap;
                                        }
                                    }, 10);

                                    emojiData.Image = image;
                                    emojiData.Format = image.RawFormat.ToString();
                                }

                                SharedCustomEmojis.Add(emojiData);
                            }
                        }

                        rowIdx2++;
                    }

                    WriteLog($"已匯入 {SharedCustomEmojis.Count} 個自定義表情符號資料。");
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
                                if (!string.IsNullOrEmpty(badgeData.Url))
                                {
                                    // 以 badgeData.Label 為鍵值，將 Image 暫存 10 分鐘。
                                    Image? image = BetterCacheManager.GetCachableData(badgeData.Label, () =>
                                    {
                                        try
                                        {
                                            // 取得 HttpClient。
                                            using HttpClient httpClient = HttpClientUtil
                                                .GetHttpClient(_httpClientFactory, userAgent);

                                            byte[] bytes = httpClient.GetByteArrayAsync(badgeData.Url).Result;

                                            using MemoryStream memoryStream = new(bytes);

                                            return Image.FromStream(memoryStream);
                                        }
                                        catch (Exception e)
                                        {
                                            WriteLog($"無法下載會員徽章「{badgeData.Label}」。");
                                            WriteLog($"會員徽章的網址：{badgeData.Url}");
                                            WriteLog($"發生錯誤：{e.Message}");

                                            // 建立一個 32x32 的白色 Bitmap。
                                            Bitmap bitmap = new(32, 32);

                                            using (Graphics graphics = Graphics.FromImage(bitmap))
                                            {
                                                graphics.Clear(Color.FromKnownColor(KnownColor.White));
                                            }

                                            return bitmap;
                                        }
                                    }, 10);

                                    badgeData.Image = image;
                                    badgeData.Format = image.RawFormat.ToString();
                                }

                                SharedBadges.Add(badgeData);
                            }
                        }

                        rowIdx3++;
                    }

                    WriteLog($"已匯入 {SharedBadges.Count} 個會員徽章資料。");
                }

                #endregion
            }
            catch (Exception ex)
            {
                WriteLog($"發生錯誤：{ex}");
            }
        }).ContinueWith(task =>
        {
            TerminateLongTask(isImport: true);
        });
    }

    /// <summary>
    /// 執行匯入/匯出任務
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
    /// 終止匯入/匯出任務
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
    /// </summary>
    private void UpdateSummaryInfo()
    {
        IEnumerable<ListViewItem> dataSet = LVLiveChatList.Items.Cast<ListViewItem>();

        TBLog.InvokeIfRequired(() =>
        {
            IEnumerable<ListViewItem> tempDataSet = dataSet.Where(n =>
                (n.SubItems[5].Text == StringSet.ChatSuperChat ||
                n.SubItems[5].Text == StringSet.ChatSuperSticker) &&
                n.SubItems[3].Text.StartsWith("$"));

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
            int count = dataSet.Where(n => n.SubItems[5].Text != StringSet.YouTube &&
                n.SubItems[5].Text != StringSet.ChatJoinMember &&
                n.SubItems[5].Text != StringSet.ChatMemberUpgrade &&
                n.SubItems[5].Text != StringSet.ChatMemberMilestone &&
                n.SubItems[5].Text != StringSet.ChatMemberGift &&
                n.SubItems[5].Text != StringSet.ChatReceivedMemberGift)
                .Count();

            LChatCount.Text = $"留言數量：{count} 個";
        });

        LSuperChatCount.InvokeIfRequired(() =>
        {
            int count = dataSet.Where(n => n.SubItems[5].Text == StringSet.ChatSuperChat).Count();

            LSuperChatCount.Text = $"{StringSet.ChatSuperChat}：{count} 個";
        });

        LSuperStickerCount.InvokeIfRequired(() =>
        {
            int count = dataSet.Where(n => n.SubItems[5].Text == StringSet.ChatSuperSticker).Count();

            LSuperStickerCount.Text = $"{StringSet.ChatSuperSticker}：{count} 個";
        });

        LMemberJoinCount.InvokeIfRequired(() =>
        {
            int joinCount = dataSet.Where(n => n.SubItems[5].Text == StringSet.ChatJoinMember).Count();
            int upgradeCount = dataSet.Where(n => n.SubItems[5].Text == StringSet.ChatMemberUpgrade).Count();
            int milestoneCount = dataSet.Where(n => n.SubItems[5].Text == StringSet.ChatMemberMilestone).Count();
            int giftCount = dataSet.Where(n => n.SubItems[5].Text == StringSet.ChatMemberGift).Count();
            int receivedGiftCount = dataSet.Where(n => n.SubItems[5].Text == StringSet.ChatReceivedMemberGift).Count();

            LMemberJoinCount.Text = $"{StringSet.ChatJoinMember}：{joinCount} 位";

            string tooltip = $"{StringSet.ChatMemberUpgrade}：{upgradeCount} 位、" +
                $"{StringSet.ChatMemberMilestone}：{milestoneCount} 位、" +
                $"{StringSet.ChatMemberGift}：{giftCount} 位、" +
                $"{StringSet.ChatReceivedMemberGift}：{receivedGiftCount} 位";

            SharedTooltip.SetToolTip(LMemberJoinCount, tooltip);
        });

        LMemberInRoomCount.InvokeIfRequired(() =>
        {
            int count = dataSet.Where(n => n.SubItems[5].Text != StringSet.ChatJoinMember &&
                n.SubItems[5].Text != StringSet.ChatMemberUpgrade &&
                n.SubItems[5].Text != StringSet.ChatMemberMilestone &&
                n.SubItems[1].Text.Contains(StringSet.Member))
                .Select(n => n.SubItems[0].Text)
                .Distinct()
                .Count();

            LMemberInRoomCount.Text = $"會員人數：{count} 位";
        });

        LAuthorCount.InvokeIfRequired(() =>
        {
            int count = dataSet.Where(n => n.SubItems[5].Text != StringSet.YouTube &&
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

        // 2022-05-30 暫時先不要鎖。
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

        BtnImport.InvokeIfRequired(() =>
        {
            BtnImport.Enabled = enable;
        });
    }

    /// <summary>
    /// 取得 Coookies
    /// </summary>
    /// <returns>字串</returns>
    private string GetCookies()
    {
        string cookiesStr = string.Empty;

        CBLoadCookies.InvokeIfRequired(() =>
        {
            if (CBLoadCookies.Checked)
            {
                string profileFolderName = string.Empty;

                TBProfileFolderName.InvokeIfRequired(() =>
                {
                    profileFolderName = TBProfileFolderName.Text;
                });

                CBBrowser.InvokeIfRequired(() =>
                {
                    List<BrowserManager.Cookie> cookies = CBBrowser.SelectedItem.ToString() switch
                    {
                        "Brave" => BrowserManager.GetCookies(BrowserManager.Browser.Brave, profileFolderName, ".youtube.com"),
                        "Google Chrome" => BrowserManager.GetCookies(BrowserManager.Browser.GoogleChrome, profileFolderName, ".youtube.com"),
                        "Chromium" => BrowserManager.GetCookies(BrowserManager.Browser.Chromium, profileFolderName, ".youtube.com"),
                        "Microsoft Edge" => BrowserManager.GetCookies(BrowserManager.Browser.MicrosoftEdge, profileFolderName, ".youtube.com"),
                        "Opera" => BrowserManager.GetCookies(BrowserManager.Browser.Opera, profileFolderName, ".youtube.com"),
                        "Opera GX" => BrowserManager.GetCookies(BrowserManager.Browser.OperaGX, profileFolderName, ".youtube.com"),
                        "Vivaldi" => BrowserManager.GetCookies(BrowserManager.Browser.Vivaldi, profileFolderName, ".youtube.com"),
                        "Mozilla Firefox" => BrowserManager.GetCookies(BrowserManager.Browser.MozillaFirefox, profileFolderName, ".youtube.com"),
                        _ => BrowserManager.GetCookies(BrowserManager.Browser.GoogleChrome, profileFolderName, ".youtube.com")
                    };

                    cookiesStr = string.Join(";", cookies.Select(n => $"{n.Name}={n.Value}"));
                });
            }
        });

        return cookiesStr;
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
    /// 檢查應用程式的版本
    /// </summary>
    private async void CheckAppVersion()
    {
        // 取得 HttpClient。
        using HttpClient httpClient = HttpClientUtil.GetHttpClient(
            _httpClientFactory);

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