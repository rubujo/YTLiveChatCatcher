using System.Drawing;
using System.Text.RegularExpressions;
using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// YTJsonParser 的公用方法
/// </summary>
public partial class YTJsonParser
{
    /// <summary>
    /// 建立 HttpClient
    /// </summary>
    /// <returns>HttpClient</returns>
    private static HttpClient CreateHttpClient()
    {
        // 沒有外部注入 HttpClient 時的備援：套用 PooledConnectionLifetime，
        // 避免長時間存活的單一 HttpClient 因底層連線一直重複使用而遇到 DNS 更新不到的問題。
        SocketsHttpHandler socketsHttpHandler = new()
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15)
        };

        HttpClient httpClient = new(socketsHttpHandler);

        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/152.0.0.0 Safari/537.36";

        httpClient.DefaultRequestHeaders.UserAgent.Clear();
        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);

        Dictionary<string, string> dictKeyValues = new()
        {
            { "Sec-CH-Prefers-Reduced-Motion", string.Empty },
            { "Sec-CH-UA", "\"Chromium\";v=\"152\", \"Google Chrome\";v=\"152\", \"Not_A Brand\";v=\"99\"" },
            { "Sec-CH-UA-Arch", string.Empty },
            { "Sec-CH-UA-Bitness",string.Empty },
            { "Sec-CH-UA-Full-Version-List", string.Empty },
            { "Sec-CH-UA-Mobile", "?0" },
            { "Sec-CH-UA-Model", string.Empty },
            { "Sec-CH-UA-Platform", "Windows" },
            { "Sec-CH-UA-Platform-Version", string.Empty },
            { "Sec-Fetch-Site", "same-origin" },
            { "Sec-Fetch-Mode", "same-origin" },
            // 2023/3/28 目前未使用 Sec-Fetch-User。
            //{ "Sec-Fetch-User", "?1" },
            { "Sec-Fetch-Dest", "empty" }
        };

        foreach (KeyValuePair<string, string> item in dictKeyValues)
        {
            if (!string.IsNullOrEmpty(item.Value))
            {
                // 先移除再新增。
                if (httpClient.DefaultRequestHeaders.Contains(item.Key))
                {
                    httpClient.DefaultRequestHeaders.Remove(item.Key);
                }

                httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
            }
        }

        return httpClient;
    }

    /// <summary>
    /// 取得已遮蔽敏感標頭（Cookie、Authorization）的 HttpRequestMessage 摘要，供記錄使用
    /// </summary>
    /// <param name="httpRequestMessage">HttpRequestMessage</param>
    /// <returns>字串</returns>
    private static string GetRedactedRequestSummary(HttpRequestMessage httpRequestMessage)
    {
        IEnumerable<string> headerLines = httpRequestMessage.Headers
            .Select(header => IsSensitiveHeaderName(header.Key) ?
                $"{header.Key}: [REDACTED]" :
                $"{header.Key}: {string.Join(", ", header.Value)}");

        return $"Method: {httpRequestMessage.Method}, RequestUri: '{httpRequestMessage.RequestUri}', " +
            $"Headers:{Environment.NewLine}{{{Environment.NewLine}  " +
            $"{string.Join($"{Environment.NewLine}  ", headerLines)}{Environment.NewLine}}}";
    }

    /// <summary>
    /// 判斷是否為記錄時應遮蔽內容的敏感標頭
    /// </summary>
    /// <param name="headerName">字串，標頭名稱</param>
    /// <returns>布林值</returns>
    private static bool IsSensitiveHeaderName(string headerName) =>
        headerName.Equals("Cookie", StringComparison.OrdinalIgnoreCase) ||
        headerName.Equals("Authorization", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 遮蔽 JSON 字串內指定屬性名稱的值，供記錄使用
    /// <para>用於 Trace 等級記錄 ytcfg／請求 body 等內容前，先遮蔽 ID_TOKEN、SESSION_INDEX、visitorData
    /// 等機密欄位，避免這些值被原封不動地寫進 log 檔案。</para>
    /// </summary>
    /// <param name="json">字串，JSON 內容</param>
    /// <param name="propertyNames">字串陣列，要遮蔽的屬性名稱</param>
    /// <returns>字串</returns>
    private static string RedactJsonProperty(string json, params string[] propertyNames)
    {
        string result = json;

        foreach (string propertyName in propertyNames)
        {
            result = Regex.Replace(
                result,
                $"(\"{Regex.Escape(propertyName)}\"\\s*:\\s*\")(?:[^\"\\\\]|\\\\.)*(\")",
                "$1[REDACTED]$2");
        }

        return result;
    }

    /// <summary>
    /// 取得 Hex 色碼
    /// </summary>
    /// <param name="value">Int64?，null 時視為異常情況</param>
    /// <returns>字串，value 為 null 或數值超出色彩範圍等異常情況時為空字串</returns>
    private static string GetColorHexCode(long? value)
    {
        // 2026/9 修正：參數改成可為 null 的 long?，呼叫端一律先用 JsonElementExtension.GetInt64Safely()
        // 取值（型別不符時回傳 null，不拋例外），這裡收到 null 直接走跟其他異常情況一致的空字串回傳，
        // 不需要呼叫端自己在呼叫這個方法「之前」就先做型別判斷——原本呼叫端是直接寫
        // textColor.Value.GetInt64()，這一步在呼叫進來之前就求值，不在這個方法自己的 try/catch
        // 保護範圍內，只要 YouTube 把顏色欄位的型別從數字改成別的，就會讓例外一路往上炸穿整批
        // ParseActions，而不是被這裡攔下來。
        if (value is not long actualValue)
        {
            return string.Empty;
        }

        try
        {
            string hex = string.Format("{0:X}", actualValue);

            int integer = Convert.ToInt32(hex, 16);

            return ColorTranslator.ToHtml(Color.FromArgb(integer));
        }
        catch (Exception)
        {
            // 此方法為 static，呼叫鏈上有多處同為 static、拿不到 _logger 的情境，
            // 比照同檔案其餘 static 解析輔助方法遇錯靜默回傳空字串的慣例，避免單一則訊息的
            // 顏色值異常就讓整批 ParseActions 中斷。
            return string.Empty;
        }
    }

    /// <summary>
    /// 取得 RendererData 的 Type
    /// </summary>
    /// <param name="rendererName">字串，*Renderer 的名稱</param>
    /// <returns>字串</returns>
    private string GetRendererDataType(string rendererName)
    {
        return rendererName switch
        {
            "liveChatTextMessageRenderer" => GetLocalizeString(KeySet.ChatGeneral),
            "liveChatPaidMessageRenderer" => GetLocalizeString(KeySet.ChatSuperChat),
            "liveChatPaidStickerRenderer" => GetLocalizeString(KeySet.ChatSuperSticker),
            "liveChatMembershipItemRenderer" => GetLocalizeString(KeySet.ChatJoinMember),
            "liveChatViewerEngagementMessageRenderer" => GetLocalizeString(StringSet.YouTube),
            "liveChatModeChangeMessageRenderer" => GetLocalizeString(StringSet.YouTube),
            "liveChatSponsorshipsGiftPurchaseAnnouncementRenderer" => GetLocalizeString(KeySet.ChatMemberGift),
            "liveChatSponsorshipsGiftRedemptionAnnouncementRenderer" => GetLocalizeString(KeySet.ChatReceivedMemberGift),
            "liveChatBannerHeaderRenderer" => GetLocalizeString(KeySet.ChatPinned),
            "liveChatBannerRedirectRenderer" => GetLocalizeString(KeySet.ChatRedirect),
            "giftMessageViewModel" => GetLocalizeString(KeySet.ChatGift),
            "liveChatDonationAnnouncementRenderer" => GetLocalizeString(KeySet.ChatDonation),
            "liveChatPurchasedProductMessageRenderer" => GetLocalizeString(KeySet.ChatDonation),
            "liveChatLegacyPaidMessageRenderer" => GetLocalizeString(KeySet.ChatDonation),
            "liveChatModerationMessageRenderer" => GetLocalizeString(KeySet.ChatModeration),
            "liveChatAutoModMessageRenderer" => GetLocalizeString(KeySet.ChatModeration),
            _ => string.Empty
        };
    }
}