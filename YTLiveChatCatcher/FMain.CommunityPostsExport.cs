using Color = System.Drawing.Color;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Style.XmlAccess;
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Models.Community;
using StringSet = YTLiveChatCatcher.Common.Sets.StringSet;
using YTLiveChatCatcher.Common;
using YTLiveChatCatcher.Common.Utils;

namespace YTLiveChatCatcher;

// 阻擋設計工具。
partial class DesignerBlocker { };

/// <summary>
/// FMain 的社群貼文匯出功能
/// </summary>
public partial class FMain
{
    /// <summary>
    /// 執行社群貼文匯出任務
    /// </summary>
    /// <param name="channelUrlOrID">字串，YouTube 頻道網址或是 ID 值</param>
    /// <param name="saveFileDialog">SaveFileDialog</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task</returns>
    public Task DoExportCommunityPostsTask(
        string channelUrlOrID,
        SaveFileDialog saveFileDialog,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            List<PostData> allPosts = [];

            await foreach (IReadOnlyList<PostData> batch in SharedYTJsonParser.StreamCommunityPostsAsync(
                channelUrlOrID,
                new CommunityPostStreamOptions { FetchWholeCommunityPosts = true },
                cancellationToken))
            {
                allPosts.AddRange(batch);

                WriteLog($"已擷取 {allPosts.Count} 篇社群貼文...");
            }

            if (allPosts.Count == 0)
            {
                WriteLog("找不到任何社群貼文，未產生檔案。");

                return;
            }

            using Stream stream = saveFileDialog.OpenFile();

            ExcelPackage.License.SetNonCommercialOrganization(StringSet.NonCommercialOrganization);

            using ExcelPackage package = new();

            ExcelWorkbook workbook = package.Workbook;

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

            BuildCommunityPostsSheet(workbook, allPosts);
            BuildCommunityPostImagesSheet(workbook, allPosts);
            BuildCommunityPostVideosSheet(workbook, allPosts);
            BuildCommunityPostPollsSheet(workbook, allPosts);

            string version = CustomFunction.GetAppVersion(),
                fileTitle = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);

            workbook.Properties.Title = fileTitle;
            workbook.Properties.Category = StringSet.SheetName7;
            workbook.Properties.Keywords = $"{Rubujo.YouTube.Utility.Sets.StringSet.YouTube}, {StringSet.SheetName7}";
            workbook.Properties.Author = $"{StringSet.AppName} {version}";

            package.SaveAs(stream);

            WriteLog($"社群貼文匯出完成，共 {allPosts.Count} 篇。");
        });
    }

    /// <summary>
    /// 建置「社群貼文」主要分頁
    /// </summary>
    /// <param name="workbook">ExcelWorkbook</param>
    /// <param name="posts">List&lt;PostData&gt;</param>
    private static void BuildCommunityPostsSheet(ExcelWorkbook workbook, List<PostData> posts)
    {
        ExcelWorksheet worksheet = workbook.Worksheets.Add(StringSet.SheetName7);

        worksheet.DefaultRowHeight = 28;
        worksheet.Column(1).Width = 5.0;
        worksheet.Column(3).Width = 60.0;
        worksheet.Column(9).Width = 40.0;

        string[] headers =
        [
            "縮圖", "作者", "內容", "發布時間", "投票數",
            "會員限定", "轉發", "轉發者", "轉發文字", "附件摘要", "貼文網址", "貼文 ID",
        ];

        for (int i = 0; i < headers.Length; i++)
        {
            ExcelRange range = worksheet.Cells[1, i + 1];

            range.StyleName = "HeaderStyle";
            range.Style.Fill.SetBackground(Color.BlanchedAlmond);
            range.Value = headers[i];
        }

        worksheet.Cells[1, 1, 1, headers.Length].AutoFilter = true;

        // 貼文 ID 只是供「貼文圖片」／「貼文影片」／「投票與測驗」分頁對照用的技術欄位，不需要顯示給使用者看。
        worksheet.Column(12).Hidden = true;

        int rowIdx = 2;

        // 一次匯出通常是同一個頻道的貼文，AuthorThumbnailUrl 在絕大多數列都完全相同
        // （只有轉發自其他頻道的貼文例外）。每一列各自寫一次 IMAGE() 公式會讓 Excel 開啟時對
        // 同一張頭像圖重複發送請求；同一個網址第二次以後改用儲存格參照公式指回第一次出現的那一格。
        Dictionary<string, string> firstThumbnailCellAddressByUrl = new(StringComparer.Ordinal);

        foreach (PostData post in posts)
        {
            ExcelRange thumbnailRange = worksheet.Cells[rowIdx, 1];

            thumbnailRange.StyleName = "ContentStyle";

            if (!string.IsNullOrEmpty(post.AuthorThumbnailUrl))
            {
                if (firstThumbnailCellAddressByUrl.TryGetValue(post.AuthorThumbnailUrl, out string? firstCellAddress))
                {
                    thumbnailRange.Formula = firstCellAddress;
                }
                else
                {
                    thumbnailRange.Formula = $"IMAGE(\"{post.AuthorThumbnailUrl}\")";

                    firstThumbnailCellAddressByUrl[post.AuthorThumbnailUrl] = thumbnailRange.Address;
                }
            }

            ExcelRange authorRange = worksheet.Cells[rowIdx, 2];

            authorRange.StyleName = "ContentStyle";
            authorRange.Value = post.AuthorText ?? string.Empty;

            ExcelRange contentRange = worksheet.Cells[rowIdx, 3];

            contentRange.StyleName = "ContentStyle";
            contentRange.Style.WrapText = true;
            contentRange.Value = CommunityPostExportUtil.FlattenRuns(post.ContentTexts);

            ExcelRange publishedTimeRange = worksheet.Cells[rowIdx, 4];

            publishedTimeRange.StyleName = "ContentStyle";
            publishedTimeRange.Value = post.PublishedTimeText ?? string.Empty;

            ExcelRange voteCountRange = worksheet.Cells[rowIdx, 5];

            voteCountRange.StyleName = "ContentStyle";
            voteCountRange.Value = post.VoteCount ?? string.Empty;

            ExcelRange isSponsorsOnlyRange = worksheet.Cells[rowIdx, 6];

            isSponsorsOnlyRange.StyleName = "ContentStyle";
            isSponsorsOnlyRange.Value = post.IsSponsorsOnly ? "是" : "否";

            ExcelRange isRepostRange = worksheet.Cells[rowIdx, 7];

            isRepostRange.StyleName = "ContentStyle";
            isRepostRange.Value = post.IsRepost ? "是" : "否";

            ExcelRange repostedByRange = worksheet.Cells[rowIdx, 8];

            repostedByRange.StyleName = "ContentStyle";
            repostedByRange.Value = post.IsRepost ? post.RepostedByAuthorText ?? string.Empty : string.Empty;

            ExcelRange repostCaptionRange = worksheet.Cells[rowIdx, 9];

            repostCaptionRange.StyleName = "ContentStyle";
            repostCaptionRange.Style.WrapText = true;
            repostCaptionRange.Value = post.IsRepost ?
                CommunityPostExportUtil.FlattenRuns(post.RepostCaptionTexts) :
                string.Empty;

            ExcelRange attachmentSummaryRange = worksheet.Cells[rowIdx, 10];

            attachmentSummaryRange.StyleName = "ContentStyle";
            attachmentSummaryRange.Value = CommunityPostExportUtil.SummarizeAttachmentTypes(post.Attachments);

            ExcelRange urlRange = worksheet.Cells[rowIdx, 11];

            urlRange.StyleName = "ContentStyle";
            urlRange.Value = post.Url ?? string.Empty;

            if (!string.IsNullOrEmpty(post.Url) && Uri.IsWellFormedUriString(post.Url, UriKind.Absolute))
            {
                urlRange.Hyperlink = new Uri(post.Url, UriKind.Absolute);
            }

            ExcelRange postIdRange = worksheet.Cells[rowIdx, 12];

            postIdRange.StyleName = "ContentStyle";
            postIdRange.Value = post.PostID ?? string.Empty;

            rowIdx++;
        }

        // 作者／發布時間／投票數／會員限定／轉發／轉發者／附件摘要這幾欄一開始沒有設定寬度，
        // 會停在 Excel 預設寬度（約 8.43 字元），標題或內容稍長就會被截斷。內容（3）／轉發文字（9）
        // 已經用 WrapText + 固定寬度處理，AutoFit 對這兩欄會失效（見 DoExportTask 內同類修正的說明），
        // 這裡不動；縮圖（1）／貼文 ID（12，隱藏）已有固定寬度，也不動。
        // 貼文網址（11）先前也是固定寬度（30），但實際網址（尤其含編碼參數的網址）常遠超過 30 字元，
        // 沒有 WrapText 時會整段溢出、視覺上像是把後面一大片空白欄位也「佔用」了，改成 AutoFit
        // 依實際內容自動加寬，上限拉高到 80 字元以涵蓋大多數常見網址長度。
        int[] autoFitColumnIndexes = [2, 4, 5, 6, 7, 8, 10];

        worksheet.Column(11).AutoFit(20.0, 80.0);

        foreach (int columnIndex in autoFitColumnIndexes)
        {
            worksheet.Column(columnIndex).AutoFit(8.0, 30.0);
        }

        // 2026/8 修正：刻意不呼叫 worksheet.Calculate(...)。EPPlus 官方文件證實 Calculate() 對含有
        // IMAGE() 公式的儲存格，會由 EPPlus 自己發送 HTTP 請求把圖片下載下來、內嵌成真正的圖片物件
        // 寫進檔案（不是單純把公式字串留給 Excel 自己評估）。實測匯出一份含數百篇貼文、每篇都有
        // 縮圖 IMAGE() 公式的檔案，EPPlus 自己的批次下載機制大量逾時／被 Google CDN 限流，超過一半的
        // 縮圖最終變成 #VALUE! 錯誤，而不是正確顯示圖片；EPPlus 官方文件對這種規模的批次下載沒有任何
        // 說明或建議上限，屬於未處理的失敗模式，不是已知限制。Calculate() 對 IMAGE() 公式而言純粹是
        // EPPlus 自己「順便先算好、內嵌預覽」的選用功能，不呼叫也完全不影響檔案本身的正確性——
        // workbook.xml 已經設定 fullCalcOnLoad="1"，使用者用真正的 Excel（365，具備雲端連線能力）
        // 開啟檔案時，Excel 自己就會正確重新計算並顯示所有公式（含 IMAGE()），用的是遠比 EPPlus
        // 自製下載器更可靠的官方雲端基礎設施，也完全不會把圖片內嵌進 EPPlus 產生的檔案裡（避免檔案
        // 肥大，符合當初改用 IMAGE() 公式而不是直接內嵌圖片的初衷）。
    }

    /// <summary>
    /// 建置「貼文圖片」分頁（只收沒有 IsVideo／IsPoll 的附件）
    /// </summary>
    /// <param name="workbook">ExcelWorkbook</param>
    /// <param name="posts">List&lt;PostData&gt;</param>
    private static void BuildCommunityPostImagesSheet(ExcelWorkbook workbook, List<PostData> posts)
    {
        var images = posts
            .Where(n => n.Attachments != null)
            .SelectMany(n => n.Attachments!.Select(a => (Post: n, Attachment: a)))
            .Where(n => !n.Attachment.IsVideo && !n.Attachment.IsPoll)
            .ToList();

        if (images.Count == 0)
        {
            return;
        }

        ExcelWorksheet worksheet = workbook.Worksheets.Add(StringSet.SheetName8);

        worksheet.DefaultRowHeight = 28;
        worksheet.Column(2).Width = 5.0;

        string[] headers = ["貼文 ID", "縮圖", "網址"];

        for (int i = 0; i < headers.Length; i++)
        {
            ExcelRange range = worksheet.Cells[1, i + 1];

            range.StyleName = "HeaderStyle";
            range.Style.Fill.SetBackground(Color.BlanchedAlmond);
            range.Value = headers[i];
        }

        worksheet.Column(1).Hidden = true;

        int rowIdx = 2;

        foreach ((PostData post, AttachmentData attachment) in images)
        {
            ExcelRange postIdRange = worksheet.Cells[rowIdx, 1];

            postIdRange.StyleName = "ContentStyle";
            postIdRange.Value = post.PostID ?? string.Empty;

            ExcelRange thumbnailRange = worksheet.Cells[rowIdx, 2];

            thumbnailRange.StyleName = "ContentStyle";

            if (!string.IsNullOrEmpty(attachment.Url))
            {
                thumbnailRange.Formula = $"IMAGE(\"{attachment.Url}\")";
            }

            ExcelRange urlRange = worksheet.Cells[rowIdx, 3];

            urlRange.StyleName = "ContentStyle";
            urlRange.Value = attachment.Url ?? string.Empty;

            if (!string.IsNullOrEmpty(attachment.Url) && Uri.IsWellFormedUriString(attachment.Url, UriKind.Absolute))
            {
                urlRange.Hyperlink = new Uri(attachment.Url, UriKind.Absolute);
            }

            rowIdx++;
        }

        // 網址（3）原本是固定寬度（40），實際網址常遠超過 40 字元，沒有 WrapText 時會整段溢出到
        // 後面一大片空白欄位，改成 AutoFit 依實際內容自動加寬。
        worksheet.Column(3).AutoFit(20.0, 80.0);

        // 2026/8 修正：刻意不呼叫 worksheet.Calculate(...)。EPPlus 官方文件證實 Calculate() 對含有
        // IMAGE() 公式的儲存格，會由 EPPlus 自己發送 HTTP 請求把圖片下載下來、內嵌成真正的圖片物件
        // 寫進檔案（不是單純把公式字串留給 Excel 自己評估）。實測匯出一份含數百篇貼文、每篇都有
        // 縮圖 IMAGE() 公式的檔案，EPPlus 自己的批次下載機制大量逾時／被 Google CDN 限流，超過一半的
        // 縮圖最終變成 #VALUE! 錯誤，而不是正確顯示圖片；EPPlus 官方文件對這種規模的批次下載沒有任何
        // 說明或建議上限，屬於未處理的失敗模式，不是已知限制。Calculate() 對 IMAGE() 公式而言純粹是
        // EPPlus 自己「順便先算好、內嵌預覽」的選用功能，不呼叫也完全不影響檔案本身的正確性——
        // workbook.xml 已經設定 fullCalcOnLoad="1"，使用者用真正的 Excel（365，具備雲端連線能力）
        // 開啟檔案時，Excel 自己就會正確重新計算並顯示所有公式（含 IMAGE()），用的是遠比 EPPlus
        // 自製下載器更可靠的官方雲端基礎設施，也完全不會把圖片內嵌進 EPPlus 產生的檔案裡（避免檔案
        // 肥大，符合當初改用 IMAGE() 公式而不是直接內嵌圖片的初衷）。
    }

    /// <summary>
    /// 建置「貼文影片」分頁（只收 IsVideo 的附件）
    /// </summary>
    /// <param name="workbook">ExcelWorkbook</param>
    /// <param name="posts">List&lt;PostData&gt;</param>
    private static void BuildCommunityPostVideosSheet(ExcelWorkbook workbook, List<PostData> posts)
    {
        var videos = posts
            .Where(n => n.Attachments != null)
            .SelectMany(n => n.Attachments!.Select(a => (Post: n, Attachment: a)))
            .Where(n => n.Attachment.IsVideo)
            .ToList();

        if (videos.Count == 0)
        {
            return;
        }

        ExcelWorksheet worksheet = workbook.Worksheets.Add(StringSet.SheetName9);

        worksheet.DefaultRowHeight = 28;
        worksheet.Column(2).Width = 5.0;
        worksheet.Column(3).Width = 40.0;

        string[] headers = ["貼文 ID", "縮圖", "標題", "網址", "發布時間", "長度", "觀看次數", "頻道"];

        for (int i = 0; i < headers.Length; i++)
        {
            ExcelRange range = worksheet.Cells[1, i + 1];

            range.StyleName = "HeaderStyle";
            range.Style.Fill.SetBackground(Color.BlanchedAlmond);
            range.Value = headers[i];
        }

        worksheet.Column(1).Hidden = true;

        int rowIdx = 2;

        foreach ((PostData post, AttachmentData attachment) in videos)
        {
            VideoData? videoData = attachment.VideoData;

            ExcelRange postIdRange = worksheet.Cells[rowIdx, 1];

            postIdRange.StyleName = "ContentStyle";
            postIdRange.Value = post.PostID ?? string.Empty;

            ExcelRange thumbnailRange = worksheet.Cells[rowIdx, 2];

            thumbnailRange.StyleName = "ContentStyle";

            if (!string.IsNullOrEmpty(videoData?.ThumbnailUrl))
            {
                thumbnailRange.Formula = $"IMAGE(\"{videoData.ThumbnailUrl}\")";
            }

            ExcelRange titleRange = worksheet.Cells[rowIdx, 3];

            titleRange.StyleName = "ContentStyle";
            titleRange.Style.WrapText = true;
            titleRange.Value = videoData?.Title ?? string.Empty;

            ExcelRange urlRange = worksheet.Cells[rowIdx, 4];

            urlRange.StyleName = "ContentStyle";
            urlRange.Value = videoData?.Url ?? string.Empty;

            if (!string.IsNullOrEmpty(videoData?.Url) && Uri.IsWellFormedUriString(videoData.Url, UriKind.Absolute))
            {
                urlRange.Hyperlink = new Uri(videoData.Url, UriKind.Absolute);
            }

            ExcelRange publishedTimeRange = worksheet.Cells[rowIdx, 5];

            publishedTimeRange.StyleName = "ContentStyle";
            publishedTimeRange.Value = videoData?.PublishedTimeText ?? string.Empty;

            ExcelRange lengthRange = worksheet.Cells[rowIdx, 6];

            lengthRange.StyleName = "ContentStyle";
            lengthRange.Value = videoData?.LengthText ?? string.Empty;

            ExcelRange viewCountRange = worksheet.Cells[rowIdx, 7];

            viewCountRange.StyleName = "ContentStyle";
            viewCountRange.Value = videoData?.ViewCountText ?? string.Empty;

            ExcelRange ownerRange = worksheet.Cells[rowIdx, 8];

            ownerRange.StyleName = "ContentStyle";
            ownerRange.Value = videoData?.OwnerText ?? string.Empty;

            rowIdx++;
        }

        // 發布時間／長度／觀看次數／頻道這幾欄一開始沒有設定寬度，理由同「社群貼文」分頁的對應修正。
        int[] autoFitColumnIndexes = [5, 6, 7, 8];

        foreach (int columnIndex in autoFitColumnIndexes)
        {
            worksheet.Column(columnIndex).AutoFit(8.0, 30.0);
        }

        // 網址（4）原本是固定寬度（30），實際網址常遠超過 30 字元，沒有 WrapText 時會整段溢出到
        // 後面一大片空白欄位，改成 AutoFit（上限拉高到 80，跟其他分頁的網址欄一致）。
        worksheet.Column(4).AutoFit(20.0, 80.0);

        // 2026/8 修正：刻意不呼叫 worksheet.Calculate(...)。EPPlus 官方文件證實 Calculate() 對含有
        // IMAGE() 公式的儲存格，會由 EPPlus 自己發送 HTTP 請求把圖片下載下來、內嵌成真正的圖片物件
        // 寫進檔案（不是單純把公式字串留給 Excel 自己評估）。實測匯出一份含數百篇貼文、每篇都有
        // 縮圖 IMAGE() 公式的檔案，EPPlus 自己的批次下載機制大量逾時／被 Google CDN 限流，超過一半的
        // 縮圖最終變成 #VALUE! 錯誤，而不是正確顯示圖片；EPPlus 官方文件對這種規模的批次下載沒有任何
        // 說明或建議上限，屬於未處理的失敗模式，不是已知限制。Calculate() 對 IMAGE() 公式而言純粹是
        // EPPlus 自己「順便先算好、內嵌預覽」的選用功能，不呼叫也完全不影響檔案本身的正確性——
        // workbook.xml 已經設定 fullCalcOnLoad="1"，使用者用真正的 Excel（365，具備雲端連線能力）
        // 開啟檔案時，Excel 自己就會正確重新計算並顯示所有公式（含 IMAGE()），用的是遠比 EPPlus
        // 自製下載器更可靠的官方雲端基礎設施，也完全不會把圖片內嵌進 EPPlus 產生的檔案裡（避免檔案
        // 肥大，符合當初改用 IMAGE() 公式而不是直接內嵌圖片的初衷）。
    }

    /// <summary>
    /// 建置「投票與測驗」分頁（只收 IsPoll 的附件，IsQuiz 一併算在內）
    /// </summary>
    /// <param name="workbook">ExcelWorkbook</param>
    /// <param name="posts">List&lt;PostData&gt;</param>
    private static void BuildCommunityPostPollsSheet(ExcelWorkbook workbook, List<PostData> posts)
    {
        var polls = posts
            .Where(n => n.Attachments != null)
            .SelectMany(n => n.Attachments!.Select(a => (Post: n, Attachment: a)))
            .Where(n => n.Attachment.IsPoll)
            .ToList();

        if (polls.Count == 0)
        {
            return;
        }

        ExcelWorksheet worksheet = workbook.Worksheets.Add(StringSet.SheetName10);

        worksheet.DefaultRowHeight = 28;
        worksheet.Column(3).Width = 5.0;
        worksheet.Column(2).Width = 30.0;

        string[] headers =
        [
            "貼文 ID", "選項文字", "選項圖片", "是否為測驗", "得票數", "得票率", "是否為正確答案", "該貼文總票數",
        ];

        for (int i = 0; i < headers.Length; i++)
        {
            ExcelRange range = worksheet.Cells[1, i + 1];

            range.StyleName = "HeaderStyle";
            range.Style.Fill.SetBackground(Color.BlanchedAlmond);
            range.Value = headers[i];
        }

        worksheet.Column(1).Hidden = true;

        int rowIdx = 2;

        foreach ((PostData post, AttachmentData attachment) in polls)
        {
            IEnumerable<ChoiceData> choices = attachment.PollData?.ChoiceDatas ?? [];

            foreach (ChoiceData choice in choices)
            {
                ExcelRange postIdRange = worksheet.Cells[rowIdx, 1];

                postIdRange.StyleName = "ContentStyle";
                postIdRange.Value = post.PostID ?? string.Empty;

                ExcelRange choiceTextRange = worksheet.Cells[rowIdx, 2];

                choiceTextRange.StyleName = "ContentStyle";
                choiceTextRange.Style.WrapText = true;
                choiceTextRange.Value = choice.Text ?? string.Empty;

                ExcelRange choiceImageRange = worksheet.Cells[rowIdx, 3];

                choiceImageRange.StyleName = "ContentStyle";

                if (!string.IsNullOrEmpty(choice.ImageUrl))
                {
                    choiceImageRange.Formula = $"IMAGE(\"{choice.ImageUrl}\")";
                }

                ExcelRange isQuizRange = worksheet.Cells[rowIdx, 4];

                isQuizRange.StyleName = "ContentStyle";
                isQuizRange.Value = attachment.IsQuiz ? "是" : "否";

                ExcelRange numVotesRange = worksheet.Cells[rowIdx, 5];

                numVotesRange.StyleName = "ContentStyle";
                numVotesRange.Value = choice.NumVotes ?? string.Empty;

                ExcelRange votePercentageRange = worksheet.Cells[rowIdx, 6];

                votePercentageRange.StyleName = "ContentStyle";
                votePercentageRange.Value = choice.VotePercentage ?? string.Empty;

                ExcelRange isCorrectRange = worksheet.Cells[rowIdx, 7];

                isCorrectRange.StyleName = "ContentStyle";
                isCorrectRange.Value = choice.IsCorrect.HasValue ? (choice.IsCorrect.Value ? "是" : "否") : string.Empty;

                ExcelRange totalVotesRange = worksheet.Cells[rowIdx, 8];

                totalVotesRange.StyleName = "ContentStyle";
                totalVotesRange.Value = attachment.PollData?.TotalVotes ?? string.Empty;

                rowIdx++;
            }
        }

        // 是否為測驗／得票數／得票率／是否為正確答案／該貼文總票數這幾欄一開始沒有設定寬度，
        // 理由同「社群貼文」分頁的對應修正。「是否為正確答案」標題本身就有 7 個字，下限拉高到 14，
        // 避免連標題本身都放不下。
        worksheet.Column(4).AutoFit(8.0, 20.0);
        worksheet.Column(5).AutoFit(8.0, 20.0);
        worksheet.Column(6).AutoFit(8.0, 20.0);
        worksheet.Column(7).AutoFit(14.0, 20.0);
        worksheet.Column(8).AutoFit(8.0, 20.0);

        // 2026/8 修正：刻意不呼叫 worksheet.Calculate(...)。EPPlus 官方文件證實 Calculate() 對含有
        // IMAGE() 公式的儲存格，會由 EPPlus 自己發送 HTTP 請求把圖片下載下來、內嵌成真正的圖片物件
        // 寫進檔案（不是單純把公式字串留給 Excel 自己評估）。實測匯出一份含數百篇貼文、每篇都有
        // 縮圖 IMAGE() 公式的檔案，EPPlus 自己的批次下載機制大量逾時／被 Google CDN 限流，超過一半的
        // 縮圖最終變成 #VALUE! 錯誤，而不是正確顯示圖片；EPPlus 官方文件對這種規模的批次下載沒有任何
        // 說明或建議上限，屬於未處理的失敗模式，不是已知限制。Calculate() 對 IMAGE() 公式而言純粹是
        // EPPlus 自己「順便先算好、內嵌預覽」的選用功能，不呼叫也完全不影響檔案本身的正確性——
        // workbook.xml 已經設定 fullCalcOnLoad="1"，使用者用真正的 Excel（365，具備雲端連線能力）
        // 開啟檔案時，Excel 自己就會正確重新計算並顯示所有公式（含 IMAGE()），用的是遠比 EPPlus
        // 自製下載器更可靠的官方雲端基礎設施，也完全不會把圖片內嵌進 EPPlus 產生的檔案裡（避免檔案
        // 肥大，符合當初改用 IMAGE() 公式而不是直接內嵌圖片的初衷）。
    }
}
