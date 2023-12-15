using System.Text.RegularExpressions;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// LiveChatCatcher 的變數
/// </summary>
public partial class LiveChatCatcher
{
    /// <summary>
    /// 共用的 Task
    /// </summary>
    private static Task? SharedTask;

    /// <summary>
    /// 共用的 CancellationTokenSource
    /// </summary>
    private static CancellationTokenSource? SharedCancellationTokenSource;

    /// <summary>
    /// 共用的 HttpClient
    /// </summary>
    private static HttpClient? SharedHttpClient;

    /// <summary>
    /// 共用的 Cookies 字串
    /// </summary>
    private static string? SharedCookies;

    /// <summary>
    /// 共用的布林值（是否為直播）
    /// </summary>
    private static bool SharedIsStreaming = false;

    /// <summary>
    /// 共用的布林值（是否獲取大張圖片）
    /// </summary>
    private static bool SharedIsFetchLargePicture;

    /// <summary>
    /// 共用的逾時毫秒值
    /// </summary>
    private static int SharedTimeoutMs;

    /// <summary>
    /// 正規表示式（取得 YouTube 影片的 ID）
    /// </summary>
    /// <returns>Regex</returns>
    [GeneratedRegex("v=(.+)")]
    private static partial Regex RegexVideoID();

    /// <summary>
    /// 正規表示式（YouTube 影片的網址）
    /// <para>來源：https://stackoverflow.com/a/15219045</para>
    /// <para>原作者：rvalvik</para>
    /// <para>原授權：CC BY-SA 3.0</para>
    /// <para>CC BY-SA 3.0：https://creativecommons.org/licenses/by-sa/3.0/</para>
    /// </summary>
    /// <returns>Regex</returns>
    [GeneratedRegex("(?:(http|https):\\/\\/(?:www\\.)?youtu\\.?be(?:\\.com)?\\/(?:embed\\/|watch\\?v=|\\?v=|v\\/|e\\/|[^\\[]+\\/|watch.*v=)?)")]
    private static partial Regex RegexYouTubeUrl();
}