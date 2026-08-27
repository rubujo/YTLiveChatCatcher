using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility.Utils;

/// <summary>
/// YouTube 網址解析工具
/// </summary>
public static partial class YouTubeUrlUtil
{
    /// <summary>
    /// 從 YouTube 頻道網址取得頻道 ID 值
    /// </summary>
    /// <param name="channelUrl">字串，YouTube 頻道的網址</param>
    /// <param name="cancellationToken">CancellationToken，預設值為 default</param>
    /// <returns>Task&lt;string&gt;</returns>
    public static async Task<string> GetYouTubeChannelID(string channelUrl, CancellationToken cancellationToken = default)
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
            channelID = await ParseYouTubeChannelID(channelUrl, cancellationToken);
        }
        else if (channelUrl.Contains($"{StringSet.Origin}/user/"))
        {
            // 舊有使用者名稱網址。
            channelID = await ParseYouTubeChannelID(channelUrl, cancellationToken);
        }
        else if (channelUrl.Contains('@'))
        {
            // 帳號代碼網址。
            channelID = await ParseYouTubeChannelID(channelUrl, cancellationToken);
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
    /// <param name="cancellationToken">CancellationToken，預設值為 default</param>
    /// <returns>Task&lt;string&gt;，解析失敗時為空字串</returns>
    public static async Task<string> ParseYouTubeChannelID(string channelUrl, CancellationToken cancellationToken = default)
    {
        string channelID = string.Empty;

        try
        {
            IConfiguration configuration = Configuration.Default.WithDefaultLoader();
            using IBrowsingContext browsingContext = BrowsingContext.New(configuration);
            using IDocument document = await browsingContext.OpenAsync(channelUrl, cancellationToken);

            channelID = GetOgUrlContent(document);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // 此類別為靜態、無 logger 可用，比照同檔案其餘解析方法遇錯靜默回傳空字串的慣例。
            channelID = string.Empty;
        }

        if (!string.IsNullOrEmpty(channelID))
        {
            channelID = channelID.Replace($"{StringSet.Origin}/channel/", string.Empty);
        }

        return channelID;
    }

    /// <summary>
    /// 從已載入的文件取得 og:url 這個 meta 標籤的 content 屬性值
    /// <para>2026/8 修正：實測發現部分頻道（例如 @handle 網址）的頁面現在會把 og:url 這類 SEO meta
    /// 標籤放在 <c>&lt;body&gt;</c> 底下，不是 <c>&lt;head&gt;</c> 的直接子節點（原本假設一定在
    /// <c>&lt;head&gt;</c> 底下）。原本用 <c>document.Head.Children.FirstOrDefault(...)</c> 只找
    /// <c>&lt;head&gt;</c> 的直接子節點，找不到就靜默回傳空字串，讓「自動取得頻道 ID」整個功能看起來
    /// 像壞掉（輸入框內容原封不動）。改用 <see cref="IParentNode.QuerySelector"/> 搜尋整份文件，
    /// 不假設這個 meta 標籤一定在哪個容器底下。</para>
    /// </summary>
    /// <param name="document">IDocument，可為 null</param>
    /// <returns>字串，找不到時為空字串</returns>
    public static string GetOgUrlContent(IDocument? document)
    {
        IElement? element = document?.QuerySelector("meta[property='og:url']");

        return element?.GetAttribute("content") ?? string.Empty;
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

        // 移除任何查詢參數（例如 &list=、&t=、&si= 等分享／追蹤參數），只保留純影片 ID。
        int queryIndex = videoID.IndexOfAny(['&', '?']);

        if (queryIndex >= 0)
        {
            videoID = videoID[..queryIndex];
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
