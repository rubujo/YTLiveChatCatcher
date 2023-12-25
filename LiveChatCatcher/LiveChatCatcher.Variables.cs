using Rubujo.YouTube.Utility.Sets;
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
    private static Task? SharedTask = null;

    /// <summary>
    /// 共用的 CancellationTokenSource
    /// </summary>
    private static CancellationTokenSource? SharedCancellationTokenSource = null;

    /// <summary>
    /// 共用的 HttpClient
    /// </summary>
    private static HttpClient? SharedHttpClient = null;

    /// <summary>
    /// 共用的 Cookies 字串
    /// </summary>
    private static string? SharedCookies = string.Empty;

    /// <summary>
    /// 共用的布林值（是否為直播）
    /// </summary>
    private static bool SharedIsStreaming = false;

    /// <summary>
    /// 共用的布林值（是否獲取大張圖片）
    /// </summary>
    private static bool SharedIsFetchLargePicture = true;

    /// <summary>
    /// 共用的顯示語言
    /// <para>預設值為 EnumSet.DisplayLanguage.Chinese_Traditional</para>
    /// </summary>
    private static EnumSet.DisplayLanguage SharedDisplayLanguage = EnumSet.DisplayLanguage.Chinese_Traditional;

    /// <summary>
    /// 共用的即時聊天類型
    /// <para>預設值為 EnumSet.LiveChatType.All</para>
    /// </summary>
    private static EnumSet.LiveChatType SharedLiveChatType = EnumSet.LiveChatType.All;

    /// <summary>
    /// 共用的自定義即時聊天類型（title）
    /// </summary>
    private static string? SharedCustomLiveChatType = string.Empty;

    /// <summary>
    /// 共用的間隔毫秒值
    /// </summary>
    private static int SharedIntervalMs = 0;

    /// <summary>
    /// 共用的強制間隔毫秒值
    /// </summary>
    private static int SharedForceIntervalMs = -1;

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