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
        worksheet.Column(11).Width = 30.0;

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

        foreach (PostData post in posts)
        {
            ExcelRange thumbnailRange = worksheet.Cells[rowIdx, 1];

            thumbnailRange.StyleName = "ContentStyle";

            if (!string.IsNullOrEmpty(post.AuthorThumbnailUrl))
            {
                thumbnailRange.Formula = $"IMAGE(\"{post.AuthorThumbnailUrl}\")";
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

        worksheet.Calculate(n => n.AlwaysRefreshImageFunction = false);
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
        worksheet.Column(3).Width = 40.0;

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

        worksheet.Calculate(n => n.AlwaysRefreshImageFunction = false);
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
        worksheet.Column(4).Width = 30.0;

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

        worksheet.Calculate(n => n.AlwaysRefreshImageFunction = false);
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

        worksheet.Calculate(n => n.AlwaysRefreshImageFunction = false);
    }
}
