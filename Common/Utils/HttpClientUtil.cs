﻿using Microsoft.Extensions.Logging;
using System.Text;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>
/// HttpClient 工具
/// </summary>
public class HttpClientUtil
{
    /// <summary>
    /// 取得 HttpClient
    /// </summary>
    /// <param name="httpClientFactory">IHttpClientFactory</param>
    /// <param name="logger">Microsoft.Extensions.Logging.ILogger</param>
    /// <param name="userAgent">字串，使用者代理字串</param>
    /// <returns>HttpClient</returns>
    public static HttpClient GetHttpClient(
        IHttpClientFactory httpClientFactory,
        ILogger logger,
        string userAgent = "")
    {
        HttpClient outputHttpClient = httpClientFactory.CreateClient();

        if (!string.IsNullOrEmpty(userAgent))
        {
            bool result = SetUserAgent(outputHttpClient, userAgent);

            if (result)
            {
                // 設定 Client Hints。
                ClientHintsUtil.SetClientHints(outputHttpClient);

                StringBuilder stringBuilder = new();

                foreach (KeyValuePair<string, IEnumerable<string>> requestHeader in
                    outputHttpClient.DefaultRequestHeaders)
                {
                    string value = string.Join(",", requestHeader.Value);

                    stringBuilder.AppendLine($"{requestHeader.Key}：{value}");
                }

                logger.LogInformation(
                    "本次連線使用的請求標頭：{NewLine}{RequestHeader}",
                    Environment.NewLine,
                    stringBuilder.ToString());
            }
        }

        return outputHttpClient;
    }

    /// <summary>
    /// 為 HttpClient 設定指定的使用者代理字串
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="userAgent">字串，使用者代理字串</param>
    /// <returns>布林值</returns>
    public static bool SetUserAgent(HttpClient httpClient, string userAgent)
    {
        bool result = httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);

        if (result)
        {
            httpClient.DefaultRequestHeaders.UserAgent.Clear();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        }

        return result;
    }

    /// <summary>
    /// 檢查使用者代理字串
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="userAgent">字串，使用者代理字串</param>
    /// <returns>布林值</returns>
    public static bool CheckUserAgent(HttpClient httpClient, string userAgent)
    {
        return httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
    }
}