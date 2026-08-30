using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// YTJsonParser 的 YouTube 驗證機制相關方法
/// </summary>
public partial class YTJsonParser
{
    /// <summary>
    /// 設定 HttpRequestMessage 的標頭
    /// </summary>
    /// <param name="httpRequestMessage">HttpRequestMessage</param>
    /// <param name="ytConfigData">YTConfigData</param>
    private void SetHttpRequestMessageHeader(
        HttpRequestMessage httpRequestMessage,
        YTConfigData? ytConfigData = null)
    {
        // 快照到區域變數，避免另一個執行緒中途呼叫 Cookies setter，
        // 導致下面的 Cookie 標頭跟 SAPISIDHASH 是用兩個不同的 Cookie 字串算出來的。
        string? cookiesSnapshot = SharedCookies;

        if (!string.IsNullOrEmpty(cookiesSnapshot))
        {
            httpRequestMessage.Headers.Add("Cookie", cookiesSnapshot);

            string[] SharedCookiesArray = cookiesSnapshot.Split(
                ';',
                StringSplitOptions.RemoveEmptyEntries);

            string? sapiSid = SharedCookiesArray.FirstOrDefault(n => n.Contains("SAPISID"));

            if (!string.IsNullOrEmpty(sapiSid))
            {
                string[] tempArray = sapiSid.Split(
                    '=',
                    StringSplitOptions.RemoveEmptyEntries);

                if (tempArray.Length == 2)
                {
                    httpRequestMessage.Headers.Authorization =
                        new AuthenticationHeaderValue(
                            "SAPISIDHASH",
                            GetSapiSidHash(tempArray[1], StringSet.Origin));
                }
            }
        }

        if (ytConfigData != null)
        {
            string xGoogAuthuser = "0",
                xGoogPageId = string.Empty;

            if (!string.IsNullOrEmpty(ytConfigData.DataSyncID))
            {
                xGoogPageId = ytConfigData.DataSyncID;
            }

            if (string.IsNullOrEmpty(xGoogPageId) &&
                !string.IsNullOrEmpty(ytConfigData.DelegatedSessionID))
            {
                xGoogPageId = ytConfigData.DelegatedSessionID;
            }

            if (!string.IsNullOrEmpty(xGoogPageId))
            {
                httpRequestMessage.Headers.Add("X-Goog-Pageid", xGoogPageId);
            }

            if (!string.IsNullOrEmpty(ytConfigData.IDToken))
            {
                httpRequestMessage.Headers.Add("X-Youtube-Identity-Token", ytConfigData.IDToken);
            }

            if (!string.IsNullOrEmpty(ytConfigData.SessionIndex))
            {
                xGoogAuthuser = ytConfigData.SessionIndex;
            }

            httpRequestMessage.Headers.Add("X-Goog-Authuser", xGoogAuthuser);

            if (!string.IsNullOrEmpty(ytConfigData.VisitorData))
            {
                httpRequestMessage.Headers.Add("X-Goog-Visitor-Id", ytConfigData.VisitorData);
            }
            else
            {
                LogMessages.Warning(_logger, nameof(SetHttpRequestMessageHeader), "變數 \"ytConfigData.VisitorData\" 為 null 或空白，未附加 X-Goog-Visitor-Id 標頭。");
            }

            httpRequestMessage.Headers.Add("X-Youtube-Client-Name", ytConfigData.InnertubeContextClientName.ToString());

            if (!string.IsNullOrEmpty(ytConfigData.InnertubeClientVersion))
            {
                httpRequestMessage.Headers.Add("X-Youtube-Client-Version", ytConfigData.InnertubeClientVersion);
            }
            else
            {
                LogMessages.Warning(_logger, nameof(SetHttpRequestMessageHeader), "變數 \"ytConfigData.InnertubeClientVersion\" 為 null 或空白，未附加 X-Youtube-Client-Version 標頭。");
            }

            if (!string.IsNullOrEmpty(ytConfigData.InitPage))
            {
                httpRequestMessage.Headers.Referrer = new Uri(ytConfigData.InitPage);
            }
        }

        httpRequestMessage.Headers.Add("Origin", StringSet.Origin);
        httpRequestMessage.Headers.Add("X-Origin", StringSet.Origin);
    }

    /// <summary>
    /// 取得 SAPISIDHASH 字串
    /// </summary>
    /// <param name="sapiSid">字串，SAPISID</param>
    /// <param name="origin">字串，origin</param>
    /// <returns>字串</returns>
    private static string GetSapiSidHash(string sapiSid, string origin)
    {
        long unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

        return $"{unixTimestamp}_{GetSHA1Hash($"{unixTimestamp} {sapiSid} {origin}")}";
    }

    /// <summary>
    /// 取得 SHA-1 雜湊 
    /// </summary>
    /// <param name="value">字串，值</param>
    /// <returns>字串</returns>
    private static string GetSHA1Hash(string value)
    {
        byte[] bytes = SHA1.HashData(Encoding.UTF8.GetBytes(value));

        return Convert.ToHexStringLower(bytes);
    }
}