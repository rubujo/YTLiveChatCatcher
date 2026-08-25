using Rubujo.YouTube.Utility.Models.Community;

namespace Rubujo.YouTube.Utility.Extensions;

/// <summary>
/// Model 的擴充方法
/// </summary>
public static class ModelExtension
{
    /// <summary>
    /// 設定 PostData 的資料統一資源標識符
    /// </summary>
    /// <param name="postData">PostData</param>
    /// <param name="ytJsonParser">YTJsonParser</param>
    /// <returns>Task</returns>
    public static async Task SetDataUri(this PostData postData, YTJsonParser ytJsonParser)
    {
        if (string.IsNullOrEmpty(postData.AuthorThumbnailUrl))
        {
            return;
        }

        byte[]? imageBytes = await ytJsonParser.GetImageBytes(postData.AuthorThumbnailUrl);

        if (imageBytes == null)
        {
            return;
        }

        postData.AuthorThumbnailDataUri = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
    }

    /// <summary>
    /// 設定 AttachmentData 的資料統一資源標識符
    /// </summary>
    /// <param name="attachmentData">AttachmentData</param>
    /// <param name="ytJsonParser">YTJsonParser</param>
    /// <returns>Task</returns>
    public static async Task SetDataUri(this AttachmentData attachmentData, YTJsonParser ytJsonParser)
    {
        if (attachmentData.IsVideo)
        {
            if (string.IsNullOrEmpty(attachmentData.VideoData?.ThumbnailUrl))
            {
                return;
            }

            byte[]? imageBytes = await ytJsonParser.GetImageBytes(attachmentData.VideoData?.ThumbnailUrl);

            if (imageBytes == null)
            {
                return;
            }

            attachmentData.DataUri = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
        }
        else if (attachmentData.IsPoll)
        {
            if (attachmentData.PollData?.ChoiceDatas == null)
            {
                return;
            }

            // List<T>.ForEach 吃的是 Action<T>，若直接塞 async lambda 會變成 fire-and-forget（呼叫端等不到
            // 圖片真正下載完成，例外也沒人接），改用 Task.WhenAll 讓呼叫端能確實等到全部選項圖片下載完成。
            await Task.WhenAll(attachmentData.PollData.ChoiceDatas.Select(async choiceData =>
            {
                byte[]? imageBytes = await ytJsonParser.GetImageBytes(choiceData.ImageUrl);

                if (imageBytes != null)
                {
                    choiceData.ImageDataUri = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
                }
            }));
        }
        else
        {
            if (string.IsNullOrEmpty(attachmentData.Url))
            {
                return;
            }

            byte[]? imageBytes = await ytJsonParser.GetImageBytes(attachmentData.Url);

            if (imageBytes == null)
            {
                return;
            }

            attachmentData.DataUri = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
        }
    }
}