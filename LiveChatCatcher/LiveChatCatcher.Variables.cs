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
    private static bool SharedIsStreaming;

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
}