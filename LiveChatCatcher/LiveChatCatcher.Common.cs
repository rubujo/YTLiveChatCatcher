using System.Drawing;
using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// LiveChatCatcher 的公用方法
/// </summary>
public partial class LiveChatCatcher
{
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