using System.Runtime.Versioning;

namespace Rubujo.YouTube.Utility.Utils;

/// <summary>
/// YouTube Cookie 取得工具
/// <para>僅限於 Microsoft Windows 平臺可以使用。</para>
/// </summary>
[SupportedOSPlatform("windows")]
public static class YouTubeCookieUtil
{
    /// <summary>
    /// 取得 YouTube 網站的 Cookie
    /// </summary>
    /// <param name="browserType">WebBrowserUtil.BrowserType，預設值為 WebBrowserUtil.BrowserType.GoogleChrome</param>
    /// <param name="profileFolderName">字串，設定檔資料夾名稱，預設值為空白</param>
    /// <returns>字串</returns>
    public static string GetYouTubeCookie(
        WebBrowserUtil.BrowserType browserType = WebBrowserUtil.BrowserType.GoogleChrome,
        string profileFolderName = "")
    {
        return GetCookie(
            browserType: browserType,
            profileFolderName: profileFolderName,
            hostKey: ".youtube.com");
    }

    /// <summary>
    /// 取得 Cookie
    /// </summary>
    /// <param name="browserType">WebBrowserUtil.BrowserType，預設值為 WebBrowserUtil.BrowserType.GoogleChrome</param>
    /// <param name="profileFolderName">字串，設定檔資料夾名稱，預設值為空白</param>
    /// <param name="hostKey">字串，主機鍵值，預設值為空白</param>
    /// <returns>字串，取得失敗時為空字串（可呼叫 <see cref="WebBrowserUtil.GetErrorMessage"/> 取得原因）</returns>
    public static string GetCookie(
        WebBrowserUtil.BrowserType browserType = WebBrowserUtil.BrowserType.GoogleChrome,
        string profileFolderName = "",
        string? hostKey = null)
    {
        List<WebBrowserUtil.CookieData> listCookie = WebBrowserUtil
            .GetCookies(
                browserType,
                profileFolderName,
                hostKey);

        return string.Join(";", listCookie.Select(n => $"{n.Name}={n.Value}"));
    }
}
