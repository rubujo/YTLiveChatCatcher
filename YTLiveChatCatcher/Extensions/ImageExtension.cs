using System.Drawing.Imaging;

namespace YTLiveChatCatcher.Extensions;

/// <summary>
/// Image 的擴充方法
/// </summary>
public static class ImageExtension
{
    /// <summary>
    /// 將 Image 轉換成 Stream
    /// </summary>
    /// <param name="image">Image</param>
    /// <param name="format">ImageFormat</param>
    /// <returns>Stream，游標已歸零，呼叫端可以直接從頭讀取</returns>
    public static Stream ToStream(this Image image, ImageFormat format)
    {
        MemoryStream memoryStream = new();

        image.Save(memoryStream, format);

        memoryStream.Position = 0;

        return memoryStream;
    }

    /// <summary>
    /// 將 Image 轉換成 Stream，使用原圖本身的格式
    /// </summary>
    /// <param name="image">Image</param>
    /// <returns>Stream，游標已歸零，呼叫端可以直接從頭讀取</returns>
    public static Stream ToStream(this Image image)
    {
        return image.ToStream(image.RawFormat);
    }
}