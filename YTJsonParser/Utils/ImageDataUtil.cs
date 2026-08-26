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
    /// <param name="cancellationToken">CancellationToken，預設值為 default</param>
    /// <returns>Task&lt;(IImage Image, string ErrorMessage)&gt;</returns>
    internal static async Task<(IImage Image, string ErrorMessage)> DownloadOrPlaceholderAsync(
        HttpClient httpClient,
        string cacheKey,
        string? displayIdentifier,
        string url,
        bool isFetchLargePicture,
        string entityDisplayName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 這裡刻意不在快取的 callback 內攔截例外——BetterCacheManager 在 callback 拋出例外時
            // 會自動移除該筆快取再重新拋出，避免下載失敗被誤記成「成功」快取 10 分鐘，
            // 導致網路短暫斷線恢復後還要等快取過期才會重新嘗試下載。
            IImage image = await BetterCacheManager.GetCachableData(cacheKey, async () =>
            {
                byte[] bytes = await httpClient.GetByteArrayAsync(url, cancellationToken);

                using MemoryStream memoryStream = new(bytes);

                return PlatformImage.FromStream(memoryStream);
            }, 10);

            return (image, string.Empty);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            string errorMessage = $"無法下載{entityDisplayName}「{displayIdentifier}」。{Environment.NewLine}" +
                $"{entityDisplayName}的網址：{url}{Environment.NewLine}" +
                $"發生錯誤：{ex.GetExceptionMessage()}{Environment.NewLine}";

            return (CreatePlaceholderImage(isFetchLargePicture), errorMessage);
        }
    }

    /// <summary>
    /// 建立指定尺寸的白色佔位圖
    /// </summary>
    /// <param name="isFetchLargePicture">布林值，true 建立 48x48、false 建立 24x24</param>
    /// <returns>IImage</returns>
    private static IImage CreatePlaceholderImage(bool isFetchLargePicture)
    {
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
}
