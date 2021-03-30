using System.Drawing.Imaging;

namespace YTLiveChatCatcher.Extensions;

/// <summary>
/// Image 的擴充方法
/// </summary>
public static class ImageExtension
{
    /// <summary>
    /// 將 Image 轉換成 Stream
    /// <para>來源：https://stackoverflow.com/a/1668493 </para>
    /// </summary>
    /// <param name="image">Image</param>
    /// <param name="format">ImageFormat</param>
    /// <returns>Stream</returns>
    public static Stream ToStream(this Image image, ImageFormat format)
    {
        MemoryStream memoryStream = new();

        using (Bitmap bitmap = new(image))
        {
            bitmap.Save(memoryStream, format);

            memoryStream.Position = 0;
        }

        return memoryStream;
    }

    /// <summary>
    /// 將 Image 轉換成 Stream
    /// </summary>
    /// <param name="image">Image</param>
    /// <returns>Stream</returns>
    public static Stream ToStream(this Image image)
    {
        MemoryStream memoryStream = new();

        using (Bitmap bitmap = new(image))
        {
            bitmap.Save(memoryStream, image.RawFormat);

            memoryStream.Position = 0;
        }

        return memoryStream;
    }
}