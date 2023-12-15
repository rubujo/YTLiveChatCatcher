using GetCachable;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Skia;
using Rubujo.YouTube.Utility.Extensions;
using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models;

/// <summary>
/// Emoji 資料
/// </summary>
public class EmojiData
{
    /// <summary>
    /// Emoji 的 ID 值
    /// </summary>
    [JsonPropertyName("id")]
    public string? ID { get; set; }

    /// <summary>
    /// 影像檔的網址
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 文字
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 標籤
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// 是否為自定義表情符號
    /// </summary>
    [JsonPropertyName("isCustomEmoji")]
    public bool IsCustomEmoji { get; set; }

    /// <summary>
    /// 影像檔的格式
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// 影像
    /// </summary>
    [JsonIgnore]
    public IImage? Image { get; set; }

    /// <summary>
    /// 設定影像
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="isFetchLargePicture">布林值，是否獲取大張圖片</param>
    /// <returns>Task&lt;string&gt;</returns>
    public async Task<string> SetImage(HttpClient? httpClient, bool isFetchLargePicture)
    {
        string errorMessage = string.Empty;

        if (httpClient == null)
        {
            errorMessage = "[EmojiData.SetImage()] 變數 \"httpClient\" 為 null！";

            return errorMessage;
        }

        if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(Url))
        {
            errorMessage = "[EmojiData.SetImage()] 變數 \"ID\" 或 \"Url\" 為 null 或空白！";

            return errorMessage;
        }

        // 以 ID 為鍵值，將 IImage 暫存 10 分鐘。
        IImage image = await BetterCacheManager.GetCachableData(ID, async () =>
        {
            try
            {
                byte[] bytes = await httpClient.GetByteArrayAsync(Url);

                using MemoryStream memoryStream = new(bytes);

                return PlatformImage.FromStream(memoryStream);
            }
            catch (Exception ex)
            {
                errorMessage = $"無法下載自定義表情符號「「{Label}」。{Environment.NewLine}" +
                    $"自定義表情符號「的網址：{Url}{Environment.NewLine}" +
                    $"發生錯誤：{ex.GetExceptionMessage()}{Environment.NewLine}";

                // 當 isFetchLargePicture 的值為 true 時，建立一個 48x48 的白色 SkiaBitmapExportContext。
                SkiaBitmapExportContext skiaBitmapExportContext = isFetchLargePicture ?
                    new(width: 24, height: 24, displayScale: 1.0f) :
                    new(width: 48, height: 48, displayScale: 1.0f);

                ICanvas canvas = skiaBitmapExportContext.Canvas;

                Rect rect = new(
                    x: 0,
                    y: 0,
                    width: skiaBitmapExportContext.Width,
                    height: skiaBitmapExportContext.Height);

                canvas.FillColor = Color.FromArgb(Colors.White.ToHex());
                canvas.FillRectangle(rect);

                using MemoryStream memoryStream = new();

                skiaBitmapExportContext.WriteToStream(memoryStream);

                return PlatformImage.FromStream(memoryStream);
            }
        }, 10);

        Image = image;
        Format = image.AsStream().GetImageFormat().ToString();

        return errorMessage;
    }
}