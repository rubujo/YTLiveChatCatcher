using System.Text.Json;
using YTApi.Extensions;
using YTLiveChatCatcher.Common.Sets;

namespace YTApi;

/// <summary>
/// JsonParser 的方法
/// </summary>
public partial class JsonParser
{
    /// <summary>
    /// 取得 id
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    public static string GetID(JsonElement? jsonElement)
    {
        string output = string.Empty;

        if (jsonElement.HasValue)
        {
            JsonElement? id = jsonElement?.Get("id");

            if (id.HasValue)
            {
                output = id.Value.GetString() ?? string.Empty;
            }
        }

        return output;
    }

    /// <summary>
    /// 取得 authorName
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    public static string GetAuthorName(JsonElement? jsonElement)
    {
        string output = string.Empty;

        if (jsonElement.HasValue)
        {
            JsonElement? simpleText = jsonElement
                ?.Get("authorName")
                ?.Get("simpleText");

            if (simpleText.HasValue)
            {
                output = simpleText.Value.GetString() ?? string.Empty;
            }
        }

        if (string.IsNullOrEmpty(output))
        {
            output = StringSet.NoAuthorName;
        }

        return output;
    }

    /// <summary>
    /// 取得 authorPhoto
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>字串</returns>
    public static string GetAuthorPhoto(JsonElement? jsonElement, bool isLarge = true)
    {
        string output = string.Empty;

        if (jsonElement.HasValue)
        {
            JsonElement? authorPhoto = jsonElement?.Get("authorPhoto");

            output = GetThumbnailUrl(authorPhoto, isLarge);
        }

        if (string.IsNullOrEmpty(output))
        {
            output = StringSet.NoAuthorPhotoUrl;
        }

        return output;
    }

    /// <summary>
    /// 取得 authorExternalChannelId
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    public static string GetAuthorExternalChannelId(JsonElement? jsonElement)
    {
        string output = string.Empty;

        if (jsonElement.HasValue)
        {
            JsonElement? authorExternalChannelId = jsonElement?.Get("authorExternalChannelId");

            if (authorExternalChannelId.HasValue)
            {
                output = authorExternalChannelId.Value.GetString() ?? string.Empty;
            }
        }

        if (string.IsNullOrEmpty(output))
        {
            output = StringSet.NoAuthorExternalChannelID;
        }

        return output;
    }

    /// <summary>
    /// 取得 timestampUsec
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    public static string GetTimestampUsec(JsonElement? jsonElement)
    {
        string output = string.Empty;

        if (jsonElement.HasValue)
        {
            JsonElement? timestampUsec = jsonElement?.Get("timestampUsec");

            if (timestampUsec.HasValue)
            {
                // 將 Microseconds 轉換成 Miliseconds。
                long timestamp = Convert.ToInt64(timestampUsec.Value.GetString()) / 1000L;

                output = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime.ToString();
            }
        }

        return output;
    }

    /// <summary>
    /// 取得 timestampText
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    public static string GetTimestampText(JsonElement? jsonElement)
    {
        string output = string.Empty;

        if (jsonElement.HasValue)
        {
            JsonElement? simpleText = jsonElement
                ?.Get("timestampText")
                ?.Get("simpleText");

            if (simpleText.HasValue)
            {
                output = simpleText.Value.GetString() ?? string.Empty;
            }
        }

        if (string.IsNullOrEmpty(output))
        {
            output = StringSet.NoTimestampText;
        }

        return output;
    }

    /// <summary>
    /// 取得 purchaseAmountText
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    public static string GetPurchaseAmountText(JsonElement? jsonElement)
    {
        string output = string.Empty;

        if (jsonElement.HasValue)
        {
            JsonElement? simpleText = jsonElement
                ?.Get("purchaseAmountText")
                ?.Get("simpleText");

            if (simpleText.HasValue)
            {
                output = simpleText.Value.GetString() ?? string.Empty;
            }
        }

        if (string.IsNullOrEmpty(output))
        {
            return StringSet.NoPurchaseAmountText;
        }

        return output;
    }

    /// <summary>
    /// 取得背景顏色
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    public static string GetBackgroundColor(JsonElement? jsonElement)
    {
        string output = string.Empty;

        if (jsonElement.HasValue)
        {
            JsonElement? backgroundColor = jsonElement?.Get("backgroundColor");

            if (backgroundColor.HasValue)
            {
                output = GetColorHexCode(backgroundColor.Value.GetInt64());
            }

            JsonElement? bodyBackgroundColor = jsonElement?.Get("bodyBackgroundColor");

            if (bodyBackgroundColor.HasValue)
            {
                output = GetColorHexCode(bodyBackgroundColor.Value.GetInt64());
            }
        }

        if (string.IsNullOrEmpty(output))
        {
            output = StringSet.NoBackgroundColor;
        }

        return output;
    }

    /// <summary>
    /// 取得 videoOffsetTimeMsec
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>字串</returns>
    public static string GetVideoOffsetTimeMsec(JsonElement? jsonElement)
    {
        string output = string.Empty;

        if (jsonElement.HasValue)
        {
            JsonElement? videoOffsetTimeMsec = jsonElement?.Get("videoOffsetTimeMsec");

            if (videoOffsetTimeMsec.HasValue)
            {
                long milliseconds = videoOffsetTimeMsec.Value.GetInt64();

                output = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).ToString("HH:mm:ss");
            }
        }

        return output;
    }

    /// <summary>
    /// 取得 Hex 色碼
    /// </summary>
    /// <param name="value">Int64</param>
    /// <returns>字串</returns>
    public static string GetColorHexCode(long value)
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
    public static string GetRendererDataType(string rendererName)
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

    /// <summary>
    /// 取得預覽圖網址
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="isLarge">布林值，是否取得大張的影像檔，預設值為 true</param>
    /// <returns>字串</returns>
    public static string GetThumbnailUrl(JsonElement? jsonElement, bool isLarge = true)
    {
        string output = string.Empty;

        if (jsonElement.HasValue)
        {
            JsonElement.ArrayEnumerator? thumbnails = jsonElement
                ?.Get("thumbnails")
                ?.ToArrayEnumerator();

            if (thumbnails.HasValue && thumbnails?.Any() == true)
            {
                int index = isLarge ? 1 : 0;

                if (thumbnails?.Count() == 1)
                {
                    index = 0;
                }

                // badge -> 0：16x16、1：32x32
                // image -> 0：24x24、1：48x48
                // authorPhoto -> 0：32x32、1：64x64
                // sticker -> 0：72x72、1：144x144
                JsonElement? url = thumbnails?.Get(index)?.Get("url");

                if (url.HasValue)
                {
                    output = url?.GetString() ?? string.Empty;

                    // 貼圖的網址會沒有 Protocol，需要手動再補上。
                    if (!string.IsNullOrEmpty(output) && output.StartsWith("//"))
                    {
                        output = $"https:{output}";

                        // 移除尾端的 =s144-rwa 以取得非 WebP 格式的圖檔網址。
                        // 疑似 System.Drawing 的 Image 不支援動畫的 WebP。 
                        string[] tempArray = output.Split("=");

                        // 當陣列數量大於 1 時才執行後續的操作。
                        if (tempArray.Length > 1)
                        {
                            // 取得 "=" 之前的網址部分。
                            output = tempArray[0];
                        }
                    }
                }
            }
        }

        return output;
    }
}