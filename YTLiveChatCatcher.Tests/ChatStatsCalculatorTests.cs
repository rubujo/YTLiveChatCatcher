using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Sets;
using YTLiveChatCatcher.Common.Utils;
using Xunit;

namespace YTLiveChatCatcher.Tests;

public class ChatStatsCalculatorTests
{
    // Chinese_Traditional 是 YTLiveChatCatcher 應用程式本身明確指定的顯示語言（FMain.Methods.cs），
    // 這裡用同一個語言建構，讓測試貼近應用程式實際執行時的本地化字串。
    private static readonly YTJsonParser YtJsonParser = new(
        new YTJsonParserOptions { DisplayLanguage = EnumSet.DisplayLanguage.Chinese_Traditional });

    public static TheoryData<string, string, double> ValidPurchaseAmounts => new()
    {
        // 對應這次真正修正的 bug：主要受眾使用的新臺幣格式，之前完全沒被辨識出來。
        { "NT$100", "NT$", 100 },
        { "NT$1,234.50", "NT$", 1234.5 },
        { "$10.00", "$", 10 },
        { "US$5", "US$", 5 },
        { "HK$50", "HK$", 50 },
        { "A$20", "A$", 20 },
        { "¥1,000", "¥", 1000 },
    };

    [Theory]
    [MemberData(nameof(ValidPurchaseAmounts))]
    public void TryParsePurchaseAmount_各種真實格式_正確拆出貨幣符號與金額(
        string purchaseAmountText, string expectedCurrencySymbol, double expectedAmount)
    {
        bool success = ChatStatsCalculator.TryParsePurchaseAmount(
            purchaseAmountText, out string currencySymbol, out double amount);

        Assert.True(success);
        Assert.Equal(expectedCurrencySymbol, currencySymbol);
        Assert.Equal(expectedAmount, amount);
    }

    [Fact]
    public void TryParsePurchaseAmount_完全無法辨識的格式_回傳false()
    {
        bool success = ChatStatsCalculator.TryParsePurchaseAmount("abc", out _, out _);

        Assert.False(success);
    }

    [Theory]
    [InlineData(KeySet.ChatGeneral, true)]
    [InlineData(KeySet.ChatSuperChat, true)]
    [InlineData(KeySet.ChatSuperSticker, true)]
    // 對應第二個真正修正的 bug：這三種類型先前被匯出用的 Excel 公式（包含法）漏算，
    // 但 SharedChatCount（排除法）本來就有正確算進去——這裡直接驗證 Classify 對這三種
    // 類型的判斷結果，確保「留言數量」的分類邏輯不會重蹈同一個覆轍。
    [InlineData(KeySet.ChatDonation, true)]
    [InlineData(KeySet.ChatModeration, true)]
    [InlineData(KeySet.ChatPoll, true)]
    [InlineData(KeySet.ChatGift, true)]
    [InlineData(KeySet.ChatJoinMember, false)]
    [InlineData(KeySet.ChatMemberUpgrade, false)]
    [InlineData(KeySet.ChatMemberMilestone, false)]
    [InlineData(KeySet.ChatMemberGift, false)]
    [InlineData(KeySet.ChatReceivedMemberGift, false)]
    [InlineData(KeySet.ChatRedirect, false)]
    [InlineData(KeySet.ChatPinned, false)]
    public void Classify_CountsAsChatMessage_只排除會員事件與導向置頂(string typeKey, bool expectedCountsAsChatMessage)
    {
        string type = YtJsonParser.GetLocalizeString(typeKey);

        MessageStatsClassification classification = ChatStatsCalculator.Classify(YtJsonParser, type, authorBadges: string.Empty);

        Assert.Equal(expectedCountsAsChatMessage, classification.CountsAsChatMessage);
    }

    [Fact]
    public void Classify_YouTube系統訊息_不計入留言數量也不計入留言人數()
    {
        MessageStatsClassification classification = ChatStatsCalculator.Classify(
            YtJsonParser, StringSet.YouTube, authorBadges: string.Empty);

        Assert.False(classification.CountsAsChatMessage);
        Assert.False(classification.CountsAsDistinctAuthor);
    }

    [Fact]
    public void Classify_超級留言且帶有會員徽章_計入會員人數統計()
    {
        string type = YtJsonParser.GetLocalizeString(KeySet.ChatSuperChat);

        MessageStatsClassification classification = ChatStatsCalculator.Classify(
            YtJsonParser, type, authorBadges: "會員（1 年）");

        Assert.True(classification.CountsAsMemberInRoom);
    }

    [Theory]
    [InlineData(KeySet.ChatJoinMember)]
    [InlineData(KeySet.ChatMemberUpgrade)]
    [InlineData(KeySet.ChatMemberMilestone)]
    public void Classify_會員加入升級里程碑事件本身_即使帶會員徽章也不計入會員人數統計(string typeKey)
    {
        string type = YtJsonParser.GetLocalizeString(typeKey);

        MessageStatsClassification classification = ChatStatsCalculator.Classify(
            YtJsonParser, type, authorBadges: "會員（1 年）");

        Assert.False(classification.CountsAsMemberInRoom);
    }

    [Fact]
    public void Classify_超級留言_IsSuperChat為true且CountsAsChatMessage為true()
    {
        string type = YtJsonParser.GetLocalizeString(KeySet.ChatSuperChat);

        MessageStatsClassification classification = ChatStatsCalculator.Classify(YtJsonParser, type, authorBadges: string.Empty);

        Assert.True(classification.IsSuperChat);
        Assert.False(classification.IsSuperSticker);
        Assert.True(classification.CountsAsChatMessage);
    }
}
