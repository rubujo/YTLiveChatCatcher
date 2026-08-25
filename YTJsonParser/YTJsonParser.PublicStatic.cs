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
    /// <returns>Task&lt;byte[]&gt;</returns>
    public async Task<byte[]?> GetImageBytes(string? url)
    {
        if (string.IsNullOrEmpty(url) || SharedHttpClient == null)
        {
            return null;
        }

        byte[] imageBytes = await BetterCacheManager.GetCachableData(url, async () =>
        {
            try
            {
                using HttpResponseMessage httpResponseMessage = await SharedHttpClient.GetAsync(url);

                return await httpResponseMessage.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetExceptionMessage());

                return [];
            }
        }, 10);

        return imageBytes.Length == 0 ? null : imageBytes;
    }
}
