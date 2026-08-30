using Microsoft.Maui.Graphics;

namespace Rubujo.YouTube.Utility.Extensions;

/// <summary>
/// Stream 擴充方法
/// </summary>
public static class StreamExtension
{
    /// <summary>
    /// 轉換成 byte[]
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <returns>byte[]</returns>
    public static byte[] ToByteArray(this Stream stream)
    {
        using BinaryReader binaryReader = new(stream);

        long remaining = stream.Length - stream.Position;

        return remaining > 0 && remaining <= int.MaxValue ?
            binaryReader.ReadBytes((int)remaining) :
            [];
    }

    /// <summary>
    /// 依照各圖片格式官方規格定義的檔頭 magic number 判斷 Stream 的圖片格式
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <returns>ImageFormat?，判斷不出來或串流過短時為 null</returns>
    public static ImageFormat? GetImageFormat(this Stream stream)
    {
        byte[] bytes = stream.ToByteArray();

        if (StartsWith(bytes, [0x42, 0x4D]))
        {
            return ImageFormat.Bmp;
        }

        if (StartsWith(bytes, "GIF87a"u8) || StartsWith(bytes, "GIF89a"u8))
        {
            return ImageFormat.Gif;
        }

        if (StartsWith(bytes, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
        {
            return ImageFormat.Png;
        }

        // TIFF 小端序（"II*\0"）／大端序（"MM\0*"）皆有效。
        if (StartsWith(bytes, [0x49, 0x49, 0x2A, 0x00]) || StartsWith(bytes, [0x4D, 0x4D, 0x00, 0x2A]))
        {
            return ImageFormat.Tiff;
        }

        // JPEG 的第 4 個位元組會依實際內容（JFIF、Exif、無 APPn 區段等）而不同，
        // 只有前 3 個位元組（SOI 標記＋下一個標記的起始位元組）是所有 JPEG 共通的。
        if (StartsWith(bytes, [0xFF, 0xD8, 0xFF]))
        {
            return ImageFormat.Jpeg;
        }

        return null;
    }

    /// <summary>
    /// 判斷位元組陣列開頭是否符合指定的簽章
    /// </summary>
    /// <param name="bytes">byte[]</param>
    /// <param name="signature">ReadOnlySpan&lt;byte&gt;</param>
    /// <returns>布林值</returns>
    private static bool StartsWith(byte[] bytes, ReadOnlySpan<byte> signature)
    {
        return bytes.Length >= signature.Length && bytes.AsSpan(0, signature.Length).SequenceEqual(signature);
    }
}
