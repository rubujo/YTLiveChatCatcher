using System.Net;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>
/// Client Hints 工具
/// </summary>
public class ClientHintsUtil
{
    /// <summary>
    /// Client Hints
    /// <para>來源 1：https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers#client_hints </para>
    /// <para>來源 2：https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers#fetch_metadata_request_headers </para>
    /// </summary>
    private static readonly Dictionary<string, string> KeyValues = new()
    {
        { "Sec-CH-Prefers-Reduced-Motion", string.Empty },
        { "Sec-CH-UA", Properties.Settings.Default.SecChUa },
        { "Sec-CH-UA-Arch", string.Empty },
        { "Sec-CH-UA-Bitness",string.Empty },
        // Deprecated.
        //{ "Sec-CH-UA-Full-Version", string.Empty },
        { "Sec-CH-UA-Full-Version-List", string.Empty },
        { "Sec-CH-UA-Mobile", Properties.Settings.Default.SecChUaMobile },
        { "Sec-CH-UA-Model", string.Empty },
        { "Sec-CH-UA-Platform", Properties.Settings.Default.SecChUaPlatform },
        { "Sec-CH-UA-Platform-Version", string.Empty },
        { "Sec-Fetch-Site", Properties.Settings.Default.SecFetchSite },
        { "Sec-Fetch-Mode", Properties.Settings.Default.SecFetchMode },
        // TODO: 2023-03-28 目前未使用 Sec-Fetch-User。
        //{ "Sec-Fetch-User", Properties.Settings.Default.SecFetchUser },
        { "Sec-Fetch-Dest", Properties.Settings.Default.SecFetchDest }
    };

    /// <summary>
    /// 設定 Client Hints 標頭資訊
    /// </summary>
    /// <param name="webHeaderCollection">HttpClient</param>
    public static void SetClientHints(WebHeaderCollection webHeaderCollection)
    {
        foreach (KeyValuePair<string, string> item in KeyValues)
        {
            if (!string.IsNullOrEmpty(item.Value))
            {
                webHeaderCollection.Add(item.Key, item.Value);
            }
        }
    }

    /// <summary>
    /// 設定 Client Hints 標頭資訊
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    public static void SetClientHints(HttpClient? httpClient)
    {
        foreach (KeyValuePair<string, string> item in KeyValues)
        {
            if (!string.IsNullOrEmpty(item.Value))
            {
                httpClient?.DefaultRequestHeaders.Add(item.Key, item.Value);
            }
        }
    }
}