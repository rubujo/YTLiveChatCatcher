using System.Security.Cryptography;
using System.Text;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>
/// 頭像圖片的落地檔案快取，讓下載過的頭像可以跨應用程式重啟／跨不同場次直播重複使用。
/// <para>記憶體內的 <c>ImageList</c>／<see cref="GetCachable.BetterCacheManager"/> 只在同一次應用程式
/// 執行期間有效，關閉應用程式後就會全部清空；這個類別把下載到的原始圖片位元組另外存一份到本機檔案，
/// 下次不論是重新啟動應用程式，或抓取同一位留言者又出現的另一場直播，都能先查這份快取，省下重複的
/// 網路下載。</para>
/// <para>快取鍵值用圖片網址本身的雜湊值，不是作者名稱或頻道 ID——YouTube 的頭像網址在使用者更換大頭貼
/// 後會變成新的網址，用網址當鍵值可以讓快取自然跟著失效／更新，不會顯示已經過期的舊頭像。</para>
/// </summary>
public static class AvatarDiskCache
{
    private static readonly string CacheDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "YTLiveChatCatcher",
        "AvatarCache");

    /// <summary>
    /// 快取檔案的存活時間：超過這個天數沒有被讀取／寫入更新過，視為過期。
    /// 頭像網址本身在使用者更換大頭貼時就會變成新網址（見類別說明），這裡的天數純粹是為了避免
    /// 只出現過一次的留言者的頭像檔案永遠留在磁碟上，不是為了處理「頭像內容過期」這件事。
    /// </summary>
    private static readonly TimeSpan Expiration = TimeSpan.FromDays(30);

    /// <summary>
    /// 依圖片網址計算快取檔案的完整路徑
    /// </summary>
    /// <param name="imageUrl">字串，圖片的網址</param>
    /// <returns>字串</returns>
    public static string GetCacheFilePath(string imageUrl)
    {
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(imageUrl)));

        return Path.Combine(CacheDirectory, $"{hash}.cache");
    }

    /// <summary>
    /// 嘗試從落地快取讀取圖片的原始位元組
    /// </summary>
    /// <param name="imageUrl">字串，圖片的網址</param>
    /// <returns>byte[]?，快取不存在、已過期或讀取失敗時回傳 null</returns>
    public static byte[]? TryRead(string imageUrl)
    {
        try
        {
            string filePath = GetCacheFilePath(imageUrl);

            if (!File.Exists(filePath))
            {
                return null;
            }

            if (DateTime.UtcNow - File.GetLastWriteTimeUtc(filePath) > Expiration)
            {
                return null;
            }

            return File.ReadAllBytes(filePath);
        }
        catch
        {
            // 落地快取純粹是效能優化，讀取失敗（例如檔案剛好被其他行程鎖住）時當成快取沒命中即可，
            // 讓呼叫端退回正常的網路下載，不應該讓這裡的例外中斷整個頭像載入流程。
            return null;
        }
    }

    /// <summary>
    /// 把下載到的圖片原始位元組寫入落地快取
    /// </summary>
    /// <param name="imageUrl">字串，圖片的網址</param>
    /// <param name="bytes">byte[]，圖片的原始位元組</param>
    public static void Write(string imageUrl, byte[] bytes)
    {
        try
        {
            string filePath = GetCacheFilePath(imageUrl);
            string? directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(filePath, bytes);
        }
        catch
        {
            // 寫入快取失敗（例如磁碟空間不足、權限問題）不應該讓這次的頭像下載被當成失敗處理——
            // 圖片本身已經下載成功，只是這次沒能順便存一份快取，下次一樣會重新下載，不影響正確性。
        }
    }

    /// <summary>
    /// 清除已過期的快取檔案。建議在應用程式啟動時呼叫一次即可，不需要每次讀寫快取都掃描整個目錄。
    /// </summary>
    public static void PruneExpiredEntries()
    {
        try
        {
            if (!Directory.Exists(CacheDirectory))
            {
                return;
            }

            foreach (string filePath in Directory.EnumerateFiles(CacheDirectory))
            {
                if (DateTime.UtcNow - File.GetLastWriteTimeUtc(filePath) > Expiration)
                {
                    File.Delete(filePath);
                }
            }
        }
        catch
        {
            // 清理過期快取純粹是磁碟空間維護，失敗（例如檔案剛好被鎖住）不影響應用程式其餘功能，
            // 靜默略過即可，下次啟動再試一次。
        }
    }
}
