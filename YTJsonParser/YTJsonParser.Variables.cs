using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// YTJsonParser 的變數
/// </summary>
public partial class YTJsonParser
{
    /// <summary>
    /// 本實例的記錄器
    /// </summary>
    private readonly ILogger<YTJsonParser> _logger;

    /// <summary>
    /// 本實例的 HttpClient
    /// </summary>
    private HttpClient? SharedHttpClient = null;

    /// <summary>
    /// 本實例的 HttpClient 是否為自動建立（用於判斷 Dispose 時機）
    /// </summary>
    private bool OwnsHttpClient = false;

    /// <summary>
    /// 本實例的 Cookies 字串
    /// </summary>
    private string? SharedCookies = string.Empty;

    /// <summary>
    /// 本實例的布林值（是否獲取大張圖片）
    /// </summary>
    private bool SharedIsFetchLargePicture = true;

    /// <summary>
    /// 本實例的顯示語言
    /// <para>預設值為 EnumSet.DisplayLanguage.Chinese_Traditional</para>
    /// </summary>
    private EnumSet.DisplayLanguage SharedDisplayLanguage = EnumSet.DisplayLanguage.Chinese_Traditional;

    /// <summary>
    /// 正規表示式（取得 YouTube 影片的 ID）
    /// </summary>
    /// <returns>Regex</returns>
    [GeneratedRegex("v=(.+)")]
    private static partial Regex RegexVideoID();
}
