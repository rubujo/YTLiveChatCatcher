using GetCachable;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Skia;
using Rubujo.YouTube.Utility.Extensions;

namespace Rubujo.YouTube.Utility.Models;

/// <summary>
/// 徽章資料
/// </summary>
public class BadgeData
{
    /// <summary>
    /// 工具提示
    /// </summary>
    public string? Tooltip { get; set; }

    /// <summary>
    /// 標籤
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// 圖示類型
    /// </summary>
    public string? IconType { get; set; }

    /// <summary>
    /// 影像檔的網址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 影像檔的格式
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// 影像
    /// </summary>
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
            errorMessage = "[BadgeData.SetImage()] 變數 \"httpClient\" 為 null！";

            return errorMessage;
        }

        if (string.IsNullOrEmpty(Label) || string.IsNullOrEmpty(Url))
        {
            errorMessage = "[BadgeData.SetImage()] 變數 \"Label\" 或 \"Url\" 為 null 或空白！";

            return errorMessage;
        }

        // 以 Label 為鍵值，將 IImage 暫存 10 分鐘。
        IImage image = await BetterCacheManager.GetCachableData(Label, async () =>
        {
            try
            {
                byte[] bytes = await httpClient.GetByteArrayAsync(Url);

                using MemoryStream memoryStream = new(bytes);

                return PlatformImage.FromStream(memoryStream);
            }
            catch (Exception ex)
            {
                errorMessage = $"無法下載會員徽章「{Label}」。{Environment.NewLine}" +
                    $"會員徽章的網址：{Url}{Environment.NewLine}" +
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