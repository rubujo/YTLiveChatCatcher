using NLog;
using System.Text;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>
/// HttpClient 工具
/// </summary>
public class HttpClientUtil
{
    /// <summary>
    /// NLog 的 Logger
    /// </summary>
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 取得 HttpClient
    /// </summary>
    /// <param name="httpClientFactory">IHttpClientFactory</param>
    /// <param name="userAgent">字串，使用者代理字串，預設值為空白</param>
    /// <returns>HttpClient</returns>
    public static HttpClient GetHttpClient(
        IHttpClientFactory httpClientFactory,
        string userAgent = "")
    {
        HttpClient httpClient = httpClientFactory.CreateClient();

        bool canTryParseAdd = SetUserAgent(httpClient, userAgent);

        // 當可以設定使用者代理字串時，才設定 Client Hints。
        if (canTryParseAdd)
        {
            // 設定 Client Hints。
            ClientHintsUtil.SetClientHints(httpClient);
        }

        LogHeaders(httpClient);

        return httpClient;
    }

    /// <summary>
    /// 更新 HttpClient
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="userAgent">字串，使用者代理字串，預設值為空白</param>
    /// <returns>HttpClient</returns>
    public static void UpdateHttpClient(HttpClient? httpClient, string userAgent = "")
    {
        bool canTryParseAdd = SetUserAgent(httpClient, userAgent);

        // 當可以設定使用者代理字串時，才設定 Client Hints。
        if (canTryParseAdd)
        {
            // 設定 Client Hints。
            ClientHintsUtil.SetClientHints(httpClient);
        }

        LogHeaders(httpClient);
    }

    /// <summary>
    /// 為 HttpClient 設定指定的使用者代理字串
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="userAgent">字串，使用者代理字串</param>
    /// <returns>布林值</returns>
    public static bool SetUserAgent(HttpClient? httpClient, string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return false;
        }

        httpClient?.DefaultRequestHeaders.UserAgent.Clear();

        return httpClient?.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent) ?? false;
    }

    /// <summary>
    /// 記錄 HttpClient 的標頭資訊
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    public static void LogHeaders(HttpClient? httpClient)
    {
        if (httpClient == null)
        {
            _logger.Error("[HttpClientUtil.LogHeaders()]　{ErrorMessage}", "變數 \"httpClient\"為 null！");

            return;
        }

        StringBuilder stringBuilder = new();

        foreach (KeyValuePair<string, IEnumerable<string>> defaultRequestHeader in httpClient.DefaultRequestHeaders)
        {
            string value = string.Join(",", defaultRequestHeader.Value);

            stringBuilder.AppendLine($"{defaultRequestHeader.Key}：{value}");
        }

        _logger.Debug("本次連線使用的請求標頭：{Message}", $"{Environment.NewLine}{stringBuilder}");
    }
}