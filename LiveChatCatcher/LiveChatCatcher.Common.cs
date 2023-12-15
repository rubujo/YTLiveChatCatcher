using System.Drawing;
using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// LiveChatCatcher 的公用方法
/// </summary>
public partial class LiveChatCatcher
{
    /// <summary>
    /// 建立 HttpClient
    /// </summary>
    /// <returns>HttpClient</returns>
    private HttpClient CreateHttpClient()
    {
        HttpClient httpClient = new();

        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/120.0.0.0 Safari/537.36";

        httpClient.DefaultRequestHeaders.UserAgent.Clear();
        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);

        Dictionary<string, string> dictKeyValues = new()
        {
            { "Sec-CH-Prefers-Reduced-Motion", string.Empty },
            { "Sec-CH-UA", "\"Not_A Brand\";v=\"8\", \"Chromium\";v=\"120\", \"Google Chrome\";v=\"120\"" },
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
                if (httpClient.DefaultRequestHeaders.Contains(item.Key) == true)
                {
                    httpClient.DefaultRequestHeaders.Remove(item.Key);
                }

                httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
            }
        }

        return httpClient;
    }

    /// <summary>
    /// 取得 Hex 色碼
    /// </summary>
    /// <param name="value">Int64</param>
    /// <returns>字串</returns>
    private string GetColorHexCode(long value)
    {
        string hex = string.Format("{0:X}", value);

        int integer = Convert.ToInt32(hex, 16);

        return ColorTranslator.ToHtml(Color.FromArgb(integer));
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
            "liveChatTextMessageRenderer" => StringSet.ChatGeneral,
            "liveChatPaidMessageRenderer" => StringSet.ChatSuperChat,
            "liveChatPaidStickerRenderer" => StringSet.ChatSuperSticker,
            "liveChatMembershipItemRenderer" => StringSet.ChatJoinMember,
            "liveChatViewerEngagementMessageRenderer" => StringSet.YouTube,
            "liveChatModeChangeMessageRenderer" => StringSet.YouTube,
            "liveChatSponsorshipsGiftPurchaseAnnouncementRenderer" => StringSet.ChatMemberGift,
            "liveChatSponsorshipsGiftRedemptionAnnouncementRenderer" => StringSet.ChatReceivedMemberGift,
            "liveChatBannerHeaderRenderer" => StringSet.ChatPinned,
            "liveChatBannerRedirectRenderer" => StringSet.ChatRedirect,
            _ => string.Empty
        };
    }
}