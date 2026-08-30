using Microsoft.Maui.Graphics;
using Rubujo.YouTube.Utility.Extensions;
using Xunit;

namespace Rubujo.YouTube.Utility.Tests;

/// <summary>
/// 驗證 StreamExtension 的行為規格：
/// <para>ToByteArray：把 Stream 剩餘（從目前 Position 到 Length）的內容讀成 byte[]。</para>
/// <para>GetImageFormat：依照各圖片格式官方規格定義的檔頭 magic number 判斷格式，
/// 判斷不出來或串流過短時回傳 null。</para>
/// </summary>
public class StreamExtensionTests
{
    private static MemoryStream CreateStream(byte[] bytes) => new(bytes);

    [Fact]
    public void ToByteArray_讀出串流剩餘的全部內容()
    {
        byte[] source = [1, 2, 3, 4, 5];

        using MemoryStream stream = CreateStream(source);

        byte[] result = stream.ToByteArray();

        Assert.Equal(source, result);
    }

    [Fact]
    public void ToByteArray_從目前Position開始讀取_不含已經讀過的部分()
    {
        byte[] source = [1, 2, 3, 4, 5];

        using MemoryStream stream = CreateStream(source);

        stream.Position = 2;

        byte[] result = stream.ToByteArray();

        Assert.Equal(new byte[] { 3, 4, 5 }, result);
    }

    [Fact]
    public void ToByteArray_空串流回傳空陣列()
    {
        using MemoryStream stream = CreateStream([]);

        byte[] result = stream.ToByteArray();

        Assert.Empty(result);
    }

    [Fact]
    public void GetImageFormat_BMP檔頭辨識為Bmp()
    {
        using MemoryStream stream = CreateStream([0x42, 0x4D, 0, 0, 0, 0]);

        Assert.Equal(ImageFormat.Bmp, stream.GetImageFormat());
    }

    [Theory]
    [InlineData(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 })] // GIF87a
    [InlineData(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 })] // GIF89a
    public void GetImageFormat_GIF檔頭辨識為Gif(byte[] header)
    {
        using MemoryStream stream = CreateStream(header);

        Assert.Equal(ImageFormat.Gif, stream.GetImageFormat());
    }

    [Fact]
    public void GetImageFormat_PNG檔頭辨識為Png()
    {
        using MemoryStream stream = CreateStream([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

        Assert.Equal(ImageFormat.Png, stream.GetImageFormat());
    }

    [Fact]
    public void GetImageFormat_TIFF小端序檔頭辨識為Tiff()
    {
        using MemoryStream stream = CreateStream([0x49, 0x49, 0x2A, 0x00]);

        Assert.Equal(ImageFormat.Tiff, stream.GetImageFormat());
    }

    [Fact]
    public void GetImageFormat_TIFF大端序檔頭辨識為Tiff()
    {
        // 原始版本沒有涵蓋大端序 TIFF（"MM"），依官方規格補上。
        using MemoryStream stream = CreateStream([0x4D, 0x4D, 0x00, 0x2A]);

        Assert.Equal(ImageFormat.Tiff, stream.GetImageFormat());
    }

    [Theory]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 })] // JFIF
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 })] // Exif
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xDB })] // 不含 APPn 區段的精簡 JPEG
    public void GetImageFormat_JPEG各種變體檔頭都辨識為Jpeg(byte[] header)
    {
        using MemoryStream stream = CreateStream(header);

        Assert.Equal(ImageFormat.Jpeg, stream.GetImageFormat());
    }

    [Fact]
    public void GetImageFormat_無法辨識的檔頭回傳null()
    {
        using MemoryStream stream = CreateStream([0, 1, 2, 3, 4, 5, 6, 7]);

        Assert.Null(stream.GetImageFormat());
    }

    [Fact]
    public void GetImageFormat_串流過短時回傳null不拋例外()
    {
        using MemoryStream stream = CreateStream([0x42]);

        Assert.Null(stream.GetImageFormat());
    }

    [Fact]
    public void GetImageFormat_空串流回傳null不拋例外()
    {
        using MemoryStream stream = CreateStream([]);

        Assert.Null(stream.GetImageFormat());
    }
}
