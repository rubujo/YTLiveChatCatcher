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
    /// <para>來源：https://stackoverflow.com/a/13287224</para>
    /// <para>原作者：Greg Beech</para>
    /// <para>原授權：CC BY-SA 3.0</para>
    /// <para>CC BY-SA 3.0：https://creativecommons.org/licenses/by-sa/3.0/</para>
    /// </summary>
    /// <param name="httpRequestMessage">HttpRequestMessage</param>
    /// <param name="ytConfigData">YTConfigData</param>
    private static void SetHttpRequestMessageHeader(
        HttpRequestMessage httpRequestMessage,
        YTConfigData? ytConfigData = null)
    {
        if (!string.IsNullOrEmpty(SharedCookies))
        {
            httpRequestMessage.Headers.Add("Cookie", SharedCookies);

            string[] SharedCookiesArray = SharedCookies.Split(
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
            httpRequestMessage.Headers.Add("X-Goog-Visitor-Id", ytConfigData.VisitorData);
            httpRequestMessage.Headers.Add("X-Youtube-Client-Name", ytConfigData.InnetrubeContextClientName.ToString());
            httpRequestMessage.Headers.Add("X-Youtube-Client-Version", ytConfigData.InnetrubeClientVersion);

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

        StringBuilder builder = new();

        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }

        return builder.ToString();
    }
}