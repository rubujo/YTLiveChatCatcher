using Rubujo.YouTube.Utility.Models.Community;
using Rubujo.YouTube.Utility.Models.LiveChat;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>
/// 社群貼文匯出用的純計算邏輯，跟 WinForms 完全脫鉤，方便單元測試覆蓋。
/// </summary>
public static class CommunityPostExportUtil
{
    /// <summary>
    /// 把一份 <see cref="RunsData"/> 清單串接成單一段純文字（<see cref="PostData.ContentTexts"/>／
    /// <see cref="PostData.RepostCaptionTexts"/> 都是這種形狀）。
    /// </summary>
    /// <param name="runs">List&lt;RunsData&gt;，可為 null</param>
    /// <returns>字串，找不到任何文字時回傳空字串</returns>
    public static string FlattenRuns(List<RunsData>? runs)
    {
        if (runs == null || runs.Count == 0)
        {
            return string.Empty;
        }

        return string.Concat(runs.Select(n => n.Text));
    }

    /// <summary>
    /// 依附件內容組出一段簡短摘要文字（例如「圖片 x2、投票」），用於社群貼文主要分頁的附件摘要欄位。
    /// </summary>
    /// <param name="attachments">List&lt;AttachmentData&gt;，可為 null</param>
    /// <returns>字串，沒有附件時回傳空字串</returns>
    public static string SummarizeAttachmentTypes(List<AttachmentData>? attachments)
    {
        if (attachments == null || attachments.Count == 0)
        {
            return string.Empty;
        }

        int videoCount = attachments.Count(n => n.IsVideo);
        int quizCount = attachments.Count(n => n.IsQuiz);
        int pollCount = attachments.Count(n => n.IsPoll && !n.IsQuiz);
        int imageCount = attachments.Count(n => !n.IsVideo && !n.IsPoll);

        List<string> parts = [];

        if (imageCount > 0)
        {
            parts.Add(imageCount > 1 ? $"圖片 x{imageCount}" : "圖片");
        }

        if (videoCount > 0)
        {
            parts.Add(videoCount > 1 ? $"影片 x{videoCount}" : "影片");
        }

        if (pollCount > 0)
        {
            parts.Add("投票");
        }

        if (quizCount > 0)
        {
            parts.Add("測驗");
        }

        return string.Join("、", parts);
    }
}
