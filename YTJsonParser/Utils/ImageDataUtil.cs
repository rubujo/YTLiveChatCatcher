using GetCachable;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Skia;
using Rubujo.YouTube.Utility.Extensions;

namespace Rubujo.YouTube.Utility.Utils;

/// <summary>
/// 影像下載工具
/// <para>供 BadgeData／EmojiData／StickerData 的 SetImage 共用，避免三份幾乎相同的下載／快取／
/// 失敗時佔位圖邏輯各自維護一份而逐漸產生分歧（先前就是這樣分歧出一個佔位圖尺寸寫反、一個錯誤訊息字串損毀的問題）。</para>
/// </summary>
internal static class ImageDataUtil
{
    /// <summary>
    /// 下載影像並快取，失敗時改回傳一張白色佔位圖
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="cacheKey">字串，快取鍵值</param>
    /// <param name="displayIdentifier">字串，用於錯誤訊息內辨識該筆資料的顯示用文字，可為 null</param>
    /// <param name="url">字串，影像檔的網址</param>
    /// <param name="isFetchLargePicture">布林值，是否獲取大張圖片（決定失敗時佔位圖的尺寸）</param>
    /// <param name="entityDisplayName">字串，用於錯誤訊息內的實體名稱（例如「會員徽章」）</param>
    /// <returns>Task&lt;(IImage Image, string ErrorMessage)&gt;</returns>
    internal static async Task<(IImage Image, string ErrorMessage)> DownloadOrPlaceholderAsync(
        HttpClient httpClient,
        string cacheKey,
        string? displayIdentifier,
        string url,
        bool isFetchLargePicture,
        string entityDisplayName)
    {
        string errorMessage = string.Empty;

        IImage image = await BetterCacheManager.GetCachableData(cacheKey, async () =>
        {
            try
            {
                byte[] bytes = await httpClient.GetByteArrayAsync(url);

                using MemoryStream memoryStream = new(bytes);

                return PlatformImage.FromStream(memoryStream);
            }
            catch (Exception ex)
            {
                errorMessage = $"無法下載{entityDisplayName}「{displayIdentifier}」。{Environment.NewLine}" +
                    $"{entityDisplayName}的網址：{url}{Environment.NewLine}" +
                    $"發生錯誤：{ex.GetExceptionMessage()}{Environment.NewLine}";

                // isFetchLargePicture 為 true 時建立 48x48、否則建立 24x24 的白色佔位圖。
                int placeholderSize = isFetchLargePicture ? 48 : 24;

                SkiaBitmapExportContext skiaBitmapExportContext = new(
                    width: placeholderSize,
                    height: placeholderSize,
                    displayScale: 1.0f);

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

        return (image, errorMessage);
    }
}
