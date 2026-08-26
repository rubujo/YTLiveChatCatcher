using GetCachable;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Sets;
using Rubujo.YouTube.Utility.Utils;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// YTJsonParser 的公開方法（狀態存取）
/// </summary>
public partial class YTJsonParser
{
    /// <summary>
    /// 目前使用中的 Cookie
    /// <para>與其他選項不同，Cookie 通常需要在同一實例的生命週期內隨使用者操作（例如切換瀏覽器、重新登入）動態更新，
    /// 因此刻意設計成可變屬性，而非併入 <see cref="YTJsonParserOptions"/> 這種建構後不可變的設定。</para>
    /// </summary>
    public string? Cookies
    {
        get => SharedCookies;
        set => SharedCookies = value;
    }

    /// <summary>
    /// 是否獲取大張圖片
    /// </summary>
    public bool FetchLargePicture => SharedIsFetchLargePicture;

    /// <summary>
    /// 顯示語言
    /// </summary>
    public EnumSet.DisplayLanguage DisplayLanguage => SharedDisplayLanguage;

    /// <summary>
    /// 取得本地化字串
    /// </summary>
    /// <param name="key">字串，鍵值</param>
    /// <returns>字串</returns>
    public string GetLocalizeString(string key)
    {
        return LangUtil.GetLocalizeString(SharedDisplayLanguage, key);
    }

    /// <summary>
    /// 取得圖片的 byte[]
    /// </summary>
    /// <param name="url">字串，圖片的網址</param>
    /// <param name="cancellationToken">CancellationToken，預設值為 default</param>
    /// <returns>Task&lt;byte[]?&gt;，找不到或下載失敗時為 null</returns>
    public async Task<byte[]?> GetImageBytes(string? url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(url) || SharedHttpClient == null)
        {
            return null;
        }

        try
        {
            // 這裡刻意不在快取的 callback 內攔截例外——BetterCacheManager 在 callback 拋出例外時
            // 會自動移除該筆快取再重新拋出，避免下載失敗被誤記成「成功」快取 10 分鐘，
            // 導致網路短暫斷線恢復後還要等快取過期才會重新嘗試下載。
            byte[] imageBytes = await BetterCacheManager.GetCachableData(url, async () =>
            {
                using HttpResponseMessage httpResponseMessage = await SharedHttpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

                return await httpResponseMessage.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            }, 10).ConfigureAwait(false);

            return imageBytes.Length == 0 ? null : imageBytes;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogMessages.Error(_logger, nameof(GetImageBytes), ex.GetExceptionMessage());

            return null;
        }
    }
}
