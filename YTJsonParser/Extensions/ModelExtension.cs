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
    /// <returns>Task</returns>
    public static async Task SetDataUri(this PostData postData)
    {
        if (string.IsNullOrEmpty(postData.AuthorThumbnailUrl))
        {
            return;
        }

        byte[]? imageBytes = await YTJsonParser.GetImageBytes(postData.AuthorThumbnailUrl);

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
    /// <returns>Task</returns>
    public static async Task SetDataUri(this AttachmentData attachmentData)
    {
        if (attachmentData.IsVideo)
        {
            if (string.IsNullOrEmpty(attachmentData.VideoData?.ThumbnailUrl))
            {
                return;
            }

            byte[]? imageBytes = await YTJsonParser.GetImageBytes(attachmentData.VideoData?.ThumbnailUrl);

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

            attachmentData.PollData?.ChoiceDatas?.ForEach(async (ChoiceData choiceData) =>
            {
                byte[]? imageBytes = await YTJsonParser.GetImageBytes(choiceData.ImageUrl);

                if (imageBytes == null)
                {
                    return;
                }

                choiceData.ImageDataUri = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
            });
        }
        else
        {
            if (string.IsNullOrEmpty(attachmentData.Url))
            {
                return;
            }

            byte[]? imageBytes = await YTJsonParser.GetImageBytes(attachmentData.Url);

            if (imageBytes == null)
            {
                return;
            }

            attachmentData.DataUri = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
        }
    }
}