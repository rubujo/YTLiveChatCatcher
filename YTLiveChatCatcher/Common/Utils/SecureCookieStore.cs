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
        byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);

        File.WriteAllBytes(FilePath, encryptedBytes);
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
