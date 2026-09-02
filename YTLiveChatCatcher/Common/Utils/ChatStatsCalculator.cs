using System.Globalization;
using System.Text.RegularExpressions;
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Sets;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>
/// 依訊息類型／徽章判斷這則訊息該如何影響統計數字的結果。
/// </summary>
/// <param name="IsSuperChat">是否為超級留言</param>
/// <param name="IsSuperSticker">是否為超級貼圖</param>
/// <param name="IsJoinMember">是否為加入會員事件</param>
/// <param name="IsMemberUpgrade">是否為會員升級事件</param>
/// <param name="IsMemberMilestone">是否為會員里程碑事件</param>
/// <param name="IsMemberGift">是否為贈送會員事件</param>
/// <param name="IsReceivedMemberGift">是否為接收會員贈送事件</param>
/// <param name="CountsAsChatMessage">是否應計入「留言數量」統計</param>
/// <param name="CountsAsMemberInRoom">是否應計入「會員人數」統計（依作者名稱去重）</param>
/// <param name="CountsAsDistinctAuthor">是否應計入「留言人數」統計（依作者名稱去重）</param>
public readonly record struct MessageStatsClassification(
    bool IsSuperChat,
    bool IsSuperSticker,
    bool IsJoinMember,
    bool IsMemberUpgrade,
    bool IsMemberMilestone,
    bool IsMemberGift,
    bool IsReceivedMemberGift,
    bool CountsAsChatMessage,
    bool CountsAsMemberInRoom,
    bool CountsAsDistinctAuthor);

/// <summary>
/// 聊天室統計計算邏輯。
/// <para>從 <c>FMain.Methods.cs</c> 的 <c>RegisterNewListViewItemStats</c> 抽出、跟 WinForms 完全脫鉤的純計算部分，
/// 讓這些判斷可以在不啟動 STA 訊息迴圈、不建立 <see cref="System.Windows.Forms.Form"/> 的情況下被單元測試覆蓋——
/// 這學期兩個真實統計錯誤（幣值加總、匯出留言數量公式）都不是被測試抓到的，就是因為這類邏輯原本整包
/// 寫死在跟 UI 控制項綁死的方法裡，沒有任何自動化機制會發現算錯。</para>
/// </summary>
public static partial class ChatStatsCalculator
{
    /// <summary>
    /// 「留言數量」統計要排除的訊息類型（不含 YouTube 系統訊息，那個用 <see cref="StringSet.YouTube"/> 直接比對）。
    /// <para>刻意集中成一份清單，讓 <see cref="Classify"/> 跟 Excel 匯出的「留言數量」公式
    /// （<c>FMain.Methods.cs</c> 的 <c>DoExportTask</c>）共用同一份排除依據，不要各自維護一份清單
    /// ——這正是先前那個匯出公式漏算捐款／版主訊息／投票建立的根本原因。</para>
    /// </summary>
    public static readonly string[] ChatMessageExclusionKeys =
    [
        KeySet.ChatJoinMember,
        KeySet.ChatMemberUpgrade,
        KeySet.ChatMemberMilestone,
        KeySet.ChatMemberGift,
        KeySet.ChatReceivedMemberGift,
        KeySet.ChatRedirect,
        KeySet.ChatPinned
    ];

    /// <summary>
    /// 「這是關聯回既有列的事件，不是獨立訊息」要排除的訊息類型（留言已被刪除／使用者已被封鎖／
    /// 回覆數更新／投票結果更新）。
    /// <para>2026/9 集中成一份清單：聊天記錄匯出的內容分頁、時間熱點分頁原本各自手刻一份幾乎相同
    /// 的排除清單（<c>FMain.Methods.cs</c> 的 <c>DoExportTask</c>），時間熱點分頁另外還會排除
    /// 會員贈送／收到贈送會員這兩種類型（理由是不容易轉換成影片對應時間點，是刻意的差異，
    /// 不在這份共用清單裡），但兩處共同的這 4 種類型如果之後要新增同類型事件，很容易忘記
    /// 同步更新其中一處。</para>
    /// </summary>
    public static readonly string[] NonMessageEventExclusionKeys =
    [
        KeySet.ChatMessageDeleted,
        KeySet.ChatUserBanned,
        KeySet.ChatReplyCountUpdate,
        KeySet.ChatPollUpdate
    ];

    /// <summary>
    /// 依訊息類型與作者徽章文字，判斷這則訊息該如何影響統計數字。
    /// </summary>
    /// <param name="ytJsonParser">YTJsonParser，用於解析目前顯示語言下的本地化類型字串</param>
    /// <param name="type">字串，訊息類型（已在地化）</param>
    /// <param name="authorBadges">字串，作者徽章文字</param>
    /// <returns>MessageStatsClassification</returns>
    public static MessageStatsClassification Classify(
        YTJsonParser ytJsonParser,
        string type,
        string authorBadges)
    {
        bool isJoinMember = type == ytJsonParser.GetLocalizeString(KeySet.ChatJoinMember);
        bool isMemberUpgrade = type == ytJsonParser.GetLocalizeString(KeySet.ChatMemberUpgrade);
        bool isMemberMilestone = type == ytJsonParser.GetLocalizeString(KeySet.ChatMemberMilestone);
        bool isMemberGift = type == ytJsonParser.GetLocalizeString(KeySet.ChatMemberGift);
        bool isReceivedMemberGift = type == ytJsonParser.GetLocalizeString(KeySet.ChatReceivedMemberGift);
        bool isRedirect = type == ytJsonParser.GetLocalizeString(KeySet.ChatRedirect);
        bool isPinned = type == ytJsonParser.GetLocalizeString(KeySet.ChatPinned);
        bool isSuperChat = type == ytJsonParser.GetLocalizeString(KeySet.ChatSuperChat);
        bool isSuperSticker = type == ytJsonParser.GetLocalizeString(KeySet.ChatSuperSticker);
        bool isYouTubeSystem = type == Rubujo.YouTube.Utility.Sets.StringSet.YouTube;

        bool countsAsChatMessage = !isYouTubeSystem &&
            !isJoinMember && !isMemberUpgrade && !isMemberMilestone &&
            !isMemberGift && !isReceivedMemberGift &&
            !isRedirect && !isPinned;

        bool countsAsMemberInRoom = !isJoinMember && !isMemberUpgrade && !isMemberMilestone &&
            authorBadges.Contains(Sets.StringSet.Member);

        bool countsAsDistinctAuthor = !isYouTubeSystem && !type.Contains(Sets.StringSet.Member);

        return new MessageStatsClassification(
            IsSuperChat: isSuperChat,
            IsSuperSticker: isSuperSticker,
            IsJoinMember: isJoinMember,
            IsMemberUpgrade: isMemberUpgrade,
            IsMemberMilestone: isMemberMilestone,
            IsMemberGift: isMemberGift,
            IsReceivedMemberGift: isReceivedMemberGift,
            CountsAsChatMessage: countsAsChatMessage,
            CountsAsMemberInRoom: countsAsMemberInRoom,
            CountsAsDistinctAuthor: countsAsDistinctAuthor);
    }

    /// <summary>
    /// 嘗試解析超級留言／貼圖的金額文字（例如 "NT$100"、"US$10.00"、"¥1,000"）為貨幣符號與數字金額。
    /// <para>可能出現任何貨幣符號（NT$、US$、HK$、¥ 等），不能假設一律是新臺幣或美金，也不能只認裸 "$" 開頭
    /// ——這是舊版 <c>purchaseAmountText.StartsWith('$')</c> 的錯誤假設，會讓 "NT$100" 這種帶國別字首的
    /// 金額完全被忽略，不同貨幣也不能直接相加當同一個數字看待。</para>
    /// <para><b>貨幣符號的字首格式取決於發送請求時的 <c>hl</c>／<c>gl</c>（對應 <see cref="EnumSet.DisplayLanguage"/>），
    /// 不是只取決於實際交易貨幣</b>——曾直接對同一筆真實新臺幣超級留言（同一個訊息 ID）分別用
    /// <c>hl=zh-TW</c> 與 <c>hl=en</c> 發請求驗證過：前者回傳裸 <c>"$15.00"</c>（沒有任何字首），
    /// 後者回傳 <c>"NT$15.00"</c>。<see cref="YTLiveChatCatcher.FMain"/> 固定使用
    /// <see cref="EnumSet.DisplayLanguage.Chinese_Traditional"/>（<c>hl=zh-TW</c>），因此裸 <c>"$"</c>
    /// 在這個應用程式看到的實際上幾乎必然是新臺幣，不是美金——這裡刻意把裸 <c>"$"</c> 正規化成
    /// <c>"NT$"</c>，避免同一種貨幣（新臺幣）因為 YouTube 偶爾省略字首而被拆成兩個獨立的統計項目。
    /// 如果未來 <c>DisplayLanguage</c> 改成非正體中文，這個正規化規則需要一併重新檢視。</para>
    /// </summary>
    /// <param name="purchaseAmountText">字串，購買金額文字</param>
    /// <param name="currencySymbol">out 字串，貨幣符號（例如 "NT$"）</param>
    /// <param name="amount">out decimal，數字金額</param>
    /// <returns>布林值，是否成功解析出金額</returns>
    public static bool TryParsePurchaseAmount(string purchaseAmountText, out string currencySymbol, out decimal amount)
    {
        Match match = PurchaseAmountRegex().Match(purchaseAmountText);

        if (!match.Success)
        {
            currencySymbol = string.Empty;
            amount = 0;

            return false;
        }

        string symbol = match.Groups["symbol"].Value.Trim();

        // 在本應用程式固定使用的 zh-TW 請求語系下，YouTube 對新臺幣金額有時會省略 "NT" 字首、
        // 只回傳裸 "$"，見上方文件註解的實測紀錄；正規化成 "NT$"，避免同一種貨幣被拆成兩個統計項目。
        currencySymbol = symbol == "$" ? "NT$" : symbol;

        // 2026/9 修正：正規表示式擷取出的金額字串固定是英式格式（逗號千分位、句點小數點，
        // 見上方文件註解），但 decimal.TryParse 沒指定 NumberStyles／CultureInfo 時會用呼叫端執行緒
        // 的 CurrentCulture 判讀，受 Windows 地區設定影響——若使用者的地區設定把逗號當小數點、
        // 句點當千分位（常見於多數歐洲地區設定），千元以上的金額會解析失敗，收益統計因此失準。
        // 固定用 InvariantCulture 判讀，不受執行環境的地區設定影響。
        return decimal.TryParse(
            match.Groups["amount"].Value,
            NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out amount);
    }

    [GeneratedRegex(@"^(?<symbol>[^\d]*)(?<amount>[\d,]+(?:\.\d+)?)")]
    private static partial Regex PurchaseAmountRegex();
}
