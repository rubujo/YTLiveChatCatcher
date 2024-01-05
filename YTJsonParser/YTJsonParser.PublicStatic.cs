using AngleSharp;
using AngleSharp.Dom;
using GetCachable;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Sets;
using Rubujo.YouTube.Utility.Utils;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// YTJsonParser 的公開靜態方法
/// </summary>
public partial class YTJsonParser
{
    /// <summary>
    /// 停止
    /// </summary>
    public static void Stop()
    {
        SharedCancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// 取得 Task
    /// </summary>
    /// <returns>Task</returns>
    public static Task? GetTask()
    {
        return SharedTask;
    }

    /// <summary>
    /// 取得 CancellationTokenSource
    /// </summary>
    /// <returns>CancellationTokenSource</returns>
    public static CancellationTokenSource? GetCancellationTokenSource()
    {
        return SharedCancellationTokenSource;
    }

    /// <summary>
    /// 取得 HttpClient
    /// </summary>
    /// <returns>HttpClient</returns>
    public static HttpClient? GetHttpClient()
    {
        return SharedHttpClient;
    }

    /// <summary>
    /// 設定 HttpClient
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    public static void SetHttpClient(HttpClient httpClient)
    {
        SharedHttpClient = httpClient;
    }

    /// <summary>
    /// 取得使用的 Cookie
    /// </summary>
    /// <returns>字串，Cookies</returns>
    public static string? GetUsedCookie()
    {
        return SharedCookies;
    }

    /// <summary>
    /// 取得使用 Cookie
    /// </summary>
    /// <returns>布林值</returns>
    public static bool UseCookie()
    {
        return !string.IsNullOrEmpty(SharedCookies);
    }

    /// <summary>
    /// 設定使用 Cookie
    /// </summary>
    /// <param name="enable">布林值，啟用，預設值為 false</param>
    /// <param name="browserType">WebBrowserUtil.BrowserType，預設值為 WebBrowserUtil.BrowserType.GoogleChrome</param>
    /// <param name="profileFolderName">字串，設定檔資料夾名稱，預設值為空白</param>
    [SupportedOSPlatform("windows")]
    public void UseCookie(
        bool enable = false,
        WebBrowserUtil.BrowserType browserType = WebBrowserUtil.BrowserType.GoogleChrome,
        string profileFolderName = "")
    {
        if (enable)
        {
            SharedCookies = GetYouTubeCookie(browserType, profileFolderName);

            if (!string.IsNullOrEmpty(SharedCookies))
            {
                RaiseOnLogOutput(EnumSet.LogType.Info, "已啟用使用 Cookie。");
            }
            else
            {
                SharedCookies = string.Empty;

                RaiseOnLogOutput(EnumSet.LogType.Info, "Cookie 取得失敗，已關閉使用 Cookie。");
            }
        }
        else
        {
            SharedCookies = string.Empty;

            RaiseOnLogOutput(EnumSet.LogType.Info, "已關閉使用 Cookie。");
        }
    }

    /// <summary>
    /// 取得是否為直播
    /// </summary>
    /// <returns>布林值</returns>
    public static bool IsStreaming()
    {
        return SharedIsStreaming;
    }

    /// <summary>
    /// 取得是否獲取大張圖片
    /// </summary>
    /// <returns>布林值</returns>
    public static bool FetchLargePicture()
    {
        return SharedIsFetchLargePicture;
    }

    /// <summary>
    /// 設定是否獲取大張圖片
    /// </summary>
    /// <param name="value">布林值</param>
    public static void FetchLargePicture(bool value)
    {
        SharedIsFetchLargePicture = value;
    }

    /// <summary>
    /// 取得是否獲全部的社群貼文
    /// </summary>
    /// <returns>布林值</returns>
    public static bool FetchWholeCommunityPosts()
    {
        return SharedFetchWholeCommunityPosts;
    }

    /// <summary>
    /// 設定是否獲全部的社群貼文
    /// </summary>
    /// <param name="value">布林值</param>
    public static void FetchWholeCommunityPosts(bool value)
    {
        SharedFetchWholeCommunityPosts = value;
    }

    /// <summary>
    /// 取得間隔毫秒值
    /// </summary>
    /// <returns>數值</returns>
    public static int IntervalMs()
    {
        return SharedForceIntervalMs >= 0 ?
            SharedForceIntervalMs :
            SharedIntervalMs;
    }

    /// <summary>
    /// 設定間隔毫秒值
    /// </summary>
    /// <param name="value">數值，值</param>
    public static void IntervalMs(int value)
    {
        SharedIntervalMs = value;
    }

    /// <summary>
    /// 設定強制間隔毫秒值
    /// <para>※當值為 -1 時，則使用間隔毫秒值</para>
    /// </summary>
    /// <param name="value">數值，值，預設值為 -1</param>
    public static void ForceIntervalMs(int value = -1)
    {
        if (value < -1)
        {
            value = -1;
        }

        SharedForceIntervalMs = value;
    }

    /// <summary>
    /// 是否強制間隔毫秒值
    /// </summary>
    /// <returns>布林值</returns>
    public static bool IsForceIntervalMs()
    {
        return SharedForceIntervalMs >= 0;
    }

    /// <summary>
    /// 取得顯示語言
    /// </summary>
    /// <returns>EnumSet.DisplayLanguag</returns>
    public static EnumSet.DisplayLanguage DisplayLanguage()
    {
        return SharedDisplayLanguage;
    }

    /// <summary>
    /// 設定顯示語言
    /// </summary>
    /// <param name="value">EnumSet.DisplayLanguage，值</param>
    public static void DisplayLanguage(EnumSet.DisplayLanguage value)
    {
        SharedDisplayLanguage = value;
    }

    /// <summary>
    /// 取得即時聊天類型
    /// </summary>
    /// <returns>EnumSet.LiveChatType</returns>
    public static EnumSet.LiveChatType LiveChatType()
    {
        return SharedLiveChatType;
    }

    /// <summary>
    /// 設定即時聊天類型
    /// </summary>
    /// <param name="value">EnumSet.LiveChatType，值</param>
    public static void LiveChatType(EnumSet.LiveChatType value)
    {
        SharedLiveChatType = value;
    }

    /// <summary>
    /// 取得自定義即時聊天類型（title）
    /// </summary>
    /// <returns>字串</returns>
    public static string? CustomLiveChatType()
    {
        return SharedCustomLiveChatType;
    }

    /// <summary>
    /// 設定自定義即時聊天類型（title）
    /// </summary>
    /// <param name="value">字串，值</param>
    public static void CustomLiveChatType(string value)
    {
        SharedCustomLiveChatType = value;
    }

    /// <summary>
    /// 從 YouTube 頻道網址取得頻道 ID 值
    /// </summary>
    /// <param name="channelUrl">字串，YouTube 頻道的網址</param>
    /// <returns>字串</returns>
    public static async Task<string> GetYouTubeChannelID(string channelUrl)
    {
        string channelID = string.Empty;

        if (channelUrl.Contains($"{StringSet.Origin}/channel/"))
        {
            // 頻道網址。
            channelID = channelUrl.Replace($"{StringSet.Origin}/channel/", string.Empty);
        }
        else if (channelUrl.Contains($"{StringSet.Origin}/c/"))
        {
            // 自訂網址。
            channelID = await ParseYouTubeChannelID(channelUrl);
        }
        else if (channelUrl.Contains($"{StringSet.Origin}/user/"))
        {
            // 舊有使用者名稱網址。
            channelID = await ParseYouTubeChannelID(channelUrl);
        }
        else if (channelUrl.Contains('@'))
        {
            // 帳號代碼網址。
            channelID = await ParseYouTubeChannelID(channelUrl);
        }

        if (string.IsNullOrEmpty(channelID))
        {
            channelID = channelUrl;
        }

        return channelID;
    }

    /// <summary>
    /// 解析 YouTube 頻道網址取得頻道 ID 值
    /// </summary>
    /// <param name="channelUrl">字串，YouTube 頻道的網址</param>
    /// <returns>Task&lt;string&gt;</returns>
    public static async Task<string> ParseYouTubeChannelID(string channelUrl)
    {
        string channelID = string.Empty;

        IConfiguration configuration = Configuration.Default.WithDefaultLoader();
        IBrowsingContext browsingContext = BrowsingContext.New(configuration);
        IDocument document = await browsingContext.OpenAsync(channelUrl);
        IElement? element = document?.Body?.Children
            .FirstOrDefault(n => n.LocalName == "meta" &&
                n.GetAttribute("property") == "og:url");

        if (element != null)
        {
            channelID = element.GetAttribute("content") ?? string.Empty;
        }

        if (!string.IsNullOrEmpty(channelID))
        {
            channelID = channelID.Replace($"{StringSet.Origin}/channel/", string.Empty);
        }

        return channelID;
    }

    /// <summary>
    /// 從 YouTube 影片的網址取得影片的 ID 值
    /// <para>來源：https://stackoverflow.com/a/15219045</para>
    /// <para>原作者：rvalvik</para>
    /// <para>原授權：CC BY-SA 3.0</para>
    /// <para>CC BY-SA 3.0：https://creativecommons.org/licenses/by-sa/3.0/</para>
    /// </summary>
    /// <param name="videoUrl">字串，影片的網址</param>
    /// <returns>字串</returns>
    public static string GetYouTubeVideoID(string videoUrl)
    {
        Regex regex = RegexYouTubeUrl();

        string videoID = regex.Replace(videoUrl, string.Empty);

        if (videoID.Contains("&list="))
        {
            string[] tempArray = videoID.Split("&list=");

            videoID = tempArray[0];
        }

        if (string.IsNullOrEmpty(videoID))
        {
            videoID = videoUrl;
        }

        return videoID;
    }

    /// <summary>
    /// 取得 YouTube 頻道網址
    /// </summary>
    /// <param name="channelID">字串，頻道的 ID</param>
    /// <returns>字串</returns>
    public static string GetYouTubeChannelUrl(string channelID)
    {
        return $"{StringSet.Origin}/channel/{channelID}";
    }

    /// <summary>
    /// 取得本地化字串
    /// </summary>
    /// <param name="key">字串，鍵值</param>
    /// <returns>字串</returns>
    public static string GetLocalizeString(string key)
    {
        return LangUtil.GetLocalizeString(SharedDisplayLanguage, key);
    }

    /// <summary>
    /// 取得圖片的 byte[]
    /// </summary>
    /// <param name="url">字串，圖片的網址</param>
    /// <returns>Task&lt;byte[]&gt;</returns>
    public static async Task<byte[]?> GetImageBytes(string? url)
    {
        if (string.IsNullOrEmpty(url) || SharedHttpClient == null)
        {
            return null;
        }

        byte[]? imageBytes = await BetterCacheManager.GetCachableData(url, async () =>
        {
            try
            {
                using HttpResponseMessage httpResponseMessage = await SharedHttpClient.GetAsync(url);

                return await httpResponseMessage.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetExceptionMessage());

                return null;
            }
        }, 10);

        return imageBytes;
    }
}