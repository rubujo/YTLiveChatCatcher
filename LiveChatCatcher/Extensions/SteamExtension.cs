using Microsoft.Maui.Graphics;
using System.Text;

namespace Rubujo.YouTube.Utility.Extensions;

/// <summary>
/// Stream 擴充方法
/// </summary>
public static class SteamExtension
{
    /// <summary>
    /// 轉換成 byte[]
    /// <para>來源：https://code-maze.com/create-byte-array-from-stream-in-csharp</para>
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <returns>byte[]</returns>
    public static byte[] ToByteArray(this Stream stream)
    {
        using BinaryReader binaryReader = new(stream);

        if (int.TryParse(stream.Length.ToString(), out int streamLength))
        {
            return binaryReader.ReadBytes(streamLength);
        }

        return [];
    }

    /// <summary>
    /// 取得 ImageFormat
    /// <para>來源：https://gist.github.com/markcastle/3cc99c8e5756c7e27532900a5f8a2a93</para>
    /// <para>原作者：markcastle</para>
    /// <para>原授權：Copyright 2017 Captive Reality Ltd</para>
    /// <para>Copyright 2017 Captive Reality Ltd：https://gist.github.com/markcastle/3cc99c8e5756c7e27532900a5f8a2a93</para>
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <returns>ImageFormat</returns>
    public static ImageFormat? GetImageFormat(this Stream stream)
    {
        byte[] bytesBMP = Encoding.ASCII.GetBytes("BM");
        byte[] bytesGIF = Encoding.ASCII.GetBytes("GIF");
        byte[] bytesPNG = [137, 80, 78, 71];
        byte[] bytesTIFF = [73, 73, 42];
        byte[] bytesJPEG = [255, 216, 255, 224];

        // 複製前 4 個 byte 到 bytesBuffer。
        byte[] bytesBuffer = new byte[4];

        Buffer.BlockCopy(stream.ToByteArray(), 0, bytesBuffer, 0, bytesBuffer.Length);

        // 檢查 bytesBuffer 的 Sequence。
        if (bytesBMP.SequenceEqual(bytesBuffer.Take(bytesBMP.Length)))
        {
            return ImageFormat.Bmp;
        }
        else if (bytesGIF.SequenceEqual(bytesBuffer.Take(bytesGIF.Length)))
        {
            return ImageFormat.Gif;
        }
        else if (bytesPNG.SequenceEqual(bytesBuffer.Take(bytesPNG.Length)))
        {
            return ImageFormat.Png;
        }
        else if (bytesTIFF.SequenceEqual(bytesBuffer.Take(bytesTIFF.Length)))
        {
            return ImageFormat.Tiff;
        }
        else if (bytesJPEG.SequenceEqual(bytesBuffer.Take(bytesJPEG.Length)))
        {
            return ImageFormat.Jpeg;
        }
        else
        {
            return null;
        }
    }
}