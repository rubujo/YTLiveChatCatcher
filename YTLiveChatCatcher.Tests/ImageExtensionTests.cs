using System.Drawing;
using System.Drawing.Imaging;
using YTLiveChatCatcher.Extensions;
using Xunit;

namespace YTLiveChatCatcher.Tests;

/// <summary>
/// 驗證 ImageExtension.ToStream 的行為規格：把 Image 編碼成指定（或原本）格式的位元組，
/// 寫進一個游標歸零的 Stream，讓呼叫端可以立即從頭讀取。
/// </summary>
public class ImageExtensionTests
{
    private static Bitmap CreateTestBitmap()
    {
        Bitmap bitmap = new(4, 4);

        using Graphics graphics = Graphics.FromImage(bitmap);

        graphics.Clear(Color.Red);

        return bitmap;
    }

    [Fact]
    public void ToStream_指定格式時輸出的Stream游標歸零且可讀取()
    {
        using Bitmap bitmap = CreateTestBitmap();
        using Stream stream = bitmap.ToStream(ImageFormat.Png);

        Assert.Equal(0, stream.Position);
        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void ToStream_指定格式時輸出的內容能還原成相同尺寸的圖片()
    {
        using Bitmap bitmap = CreateTestBitmap();
        using Stream stream = bitmap.ToStream(ImageFormat.Png);
        using Image restored = Image.FromStream(stream);

        Assert.Equal(bitmap.Width, restored.Width);
        Assert.Equal(bitmap.Height, restored.Height);
    }

    [Fact]
    public void ToStream_未指定格式時使用原圖的RawFormat()
    {
        // 直接對一張全新建立的 Bitmap 呼叫 ToStream()（不指定格式）會踩到 GDI+ 本身的已知限制——
        // 全新建立的 Bitmap 的 RawFormat 是 MemoryBmp，Image.Save 對這個格式會直接拋例外，
        // 這不是 ToStream 這個包裝方法的問題，任何直接呼叫 Image.Save(stream, image.RawFormat) 的
        // 程式碼都會遇到同樣的例外。改用「先編碼成 PNG、再讀回來」取得一張具有真實 RawFormat
        // （Png）的 Image，貼近實際使用情境（例如從網路下載或從檔案讀取的圖片）。
        using Bitmap originalBitmap = CreateTestBitmap();
        using Stream pngStream = originalBitmap.ToStream(ImageFormat.Png);
        using Image loadedImage = Image.FromStream(pngStream);

        using Stream stream = loadedImage.ToStream();

        Assert.Equal(0, stream.Position);
        Assert.True(stream.Length > 0);
    }

    [Theory]
    [InlineData(true)] // ImageFormat.Png
    [InlineData(false)] // ImageFormat.Bmp
    public void ToStream_不同格式都能正確編碼(bool usePng)
    {
        using Bitmap bitmap = CreateTestBitmap();

        ImageFormat format = usePng ? ImageFormat.Png : ImageFormat.Bmp;

        using Stream stream = bitmap.ToStream(format);

        Assert.True(stream.Length > 0);
    }
}
