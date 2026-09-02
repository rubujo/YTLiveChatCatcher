using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>
/// 以 Windows DPAPI（CurrentUser scope）加密儲存本應用程式自己取得的 YouTube 登入 Cookie
/// <para>只有同一台機器、同一個 Windows 使用者才能解密還原，不會被複製到其他機器上使用。</para>
/// <para>注意：這裡是加密「自己這個程式要存的東西」，跟解密其他應用程式（例如瀏覽器）已加密的資料庫是相反的方向。</para>
/// </summary>
[SupportedOSPlatform("windows")]
public static class SecureCookieStore
{
    /// <summary>
    /// 加密後的 Cookie 檔案路徑
    /// </summary>
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "YTLiveChatCatcher",
        "cookie.dat");

    /// <summary>
    /// 是否已儲存記住的登入資料
    /// </summary>
    /// <returns>布林值</returns>
    public static bool Exists()
    {
        return File.Exists(FilePath);
    }

    /// <summary>
    /// 加密並儲存 Cookie 字串
    /// </summary>
    /// <param name="cookies">字串，Cookie 內容</param>
    public static void Save(string cookies)
    {
        string? directory = Path.GetDirectoryName(FilePath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        byte[] plainBytes = Encoding.UTF8.GetBytes(cookies);

        try
        {
            byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);

            File.WriteAllBytes(FilePath, encryptedBytes);
        }
        finally
        {
            // 降低 Cookie 明文位元組在受控堆積裡停留的時間；原始 string 仍由 .NET 管理，
            // 但這份為了 DPAPI 額外建立的可變緩衝區可以在使用後立即清除。
            CryptographicOperations.ZeroMemory(plainBytes);
        }
    }

    /// <summary>
    /// 讀取並解密先前儲存的 Cookie 字串
    /// </summary>
    /// <returns>字串，找不到或無法解密時為 null</returns>
    public static string? Load()
    {
        if (!File.Exists(FilePath))
        {
            return null;
        }

        try
        {
            byte[] encryptedBytes = File.ReadAllBytes(FilePath);
            byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (CryptographicException)
        {
            // 可能是在別台機器或別的 Windows 使用者帳號下產生的檔案，無法解密，視為不存在並清除。
            Clear();

            return null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 2026/9 修正：原本只攔截 CryptographicException，但 File.ReadAllBytes 若遇到檔案被
            // 其他程式鎖住或權限異常，會拋出 IOException／UnauthorizedAccessException，不在原本
            // 那個 catch 的保護範圍內，會一路未攔截地往上冒到啟動流程的通用 try/catch，讓啟動流程
            // 提早中止、後續步驟（CheckCaptureRecovery、頭像快取清理、版本檢查）都不會執行。
            // 這類情境是暫時性的讀取失敗，跟「密文本身已經損毀」完全不同——不應該比照
            // CryptographicException 直接刪除檔案（檔案內容可能完全正常，只是這次讀不到），
            // 單純視為這次讀取失敗、保留檔案供下次重試即可。
            return null;
        }
    }

    /// <summary>
    /// 清除已儲存的 Cookie 檔案
    /// </summary>
    public static void Clear()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}
