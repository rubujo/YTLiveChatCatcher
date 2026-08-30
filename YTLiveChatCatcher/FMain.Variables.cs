using Microsoft.Extensions.Logging;
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Models.LiveChat;

namespace YTLiveChatCatcher;

// 阻擋設計工具。
partial class DesignerBlocker { };

/// <summary>
/// FMain 的變數
/// </summary>
public partial class FMain
{
    /// <summary>
    /// 共用的 HttpClient
    /// </summary>
    private HttpClient? SharedHttpClient;

    /// <summary>
    /// 共用的 ILogger&lt;FMain&gt;
    /// </summary>
    private readonly ILogger<FMain> SharedLogger;

    /// <summary>
    /// 共用的 ILogger&lt;YTJsonParser&gt;
    /// </summary>
    private readonly ILogger<YTJsonParser> SharedYTJsonParserLogger;

    /// <summary>
    /// 共用的 IHttpClientFactory
    /// </summary>
    private readonly IHttpClientFactory SharedHttpClientFactory;

    /// <summary>
    /// 共用的 YTJsonParser
    /// <para>於 InitLiveChatCather() 內建立（需要等待 SharedHttpClient 就緒），並非在此處以欄位初始設定式建立。</para>
    /// </summary>
    private YTJsonParser SharedYTJsonParser = null!;

    /// <summary>
    /// 共用的 CancellationTokenSource（用於取消目前的即時聊天擷取工作）
    /// </summary>
    private CancellationTokenSource? SharedFetchCancellationTokenSource;

    /// <summary>
    /// 共用的 ToolTip
    /// </summary>
    private readonly ToolTip SharedTooltip = new();

    /// <summary>
    /// 共用的 List&lt;StickerData&gt;
    /// </summary>
    private readonly List<StickerData> SharedStickers = [];

    /// <summary>
    /// 共用的 List&lt;EmojiData&gt;
    /// </summary>
    private readonly List<EmojiData> SharedCustomEmojis = [];

    /// <summary>
    /// 共用的 List&lt;BadgeData&gt;
    /// </summary>
    private readonly List<BadgeData> SharedBadges = [];

    /// <summary>
    /// <see cref="FMain.FMain_Load"/> 內把 <c>LVLiveChatList</c> 設成 <c>VirtualMode = true</c> 之後，
    /// 這是它唯一的真實資料來源——VirtualMode 下 <c>ListView.Items</c> 集合完全禁止存取（讀寫都會丟
    /// <see cref="InvalidOperationException"/>），必須自己維護這份背景清單，透過
    /// <c>RetrieveVirtualItem</c> 事件（<see cref="LVLiveChatList_RetrieveVirtualItem"/>）供應資料，
    /// 並在異動筆數後同步更新 <c>LVLiveChatList.VirtualListSize</c>。
    /// </summary>
    private readonly List<ListViewItem> SharedListViewItems = [];

    /// <summary>
    /// 依訊息 ID（<see cref="RendererData.ID"/>）索引現有的 ListViewItem。
    /// <para>用於「留言已被刪除」／「投票結果更新」等以 ID 關聯回原始留言的事件，
    /// 讓這類事件能 O(1) 找到對應列並就地更新，而不是被誤判成新留言加入清單。
    /// 在 <see cref="BtnClear_Click"/> 清空聊天室時務必一併清空，避免記憶體洩漏與關聯到已不存在的列。</para>
    /// </summary>
    private readonly Dictionary<string, ListViewItem> SharedItemsByMessageID = [];

    /// <summary>
    /// 依 <see cref="RendererData.ReplyCountEntityKey"/> 索引現有的 ListViewItem。
    /// <para>回覆數更新事件的關聯鍵值跟訊息 ID 是不同的命名空間，因此需要獨立索引；
    /// 語意與清空時機同 <see cref="SharedItemsByMessageID"/>。</para>
    /// </summary>
    private readonly Dictionary<string, ListViewItem> SharedItemsByReplyCountEntityKey = [];

    /// <summary>
    /// 依 <see cref="RendererData.AuthorExternalChannelID"/> 索引該使用者目前所有的 ListViewItem。
    /// <para>用於「使用者已被封鎖」事件一次找出該使用者所有留言並標記，而不是逐列線性掃描整個 ListView；
    /// 語意與清空時機同 <see cref="SharedItemsByMessageID"/>。</para>
    /// </summary>
    private readonly Dictionary<string, List<ListViewItem>> SharedItemsByAuthorChannelID = [];

    #region 累加式統計計數器（2026/8 新增，取代 UpdateSummaryInfo 內原本每批次都要重新掃描整個 ListView 的做法）

    // 以下欄位只應該由 RegisterNewListViewItemStats 更新（每新增一列呼叫一次），
    // 以及 BtnClear_Click 清空聊天室時重設；UpdateSummaryInfo 只負責讀取，不應該再重新計算。
    // 若未來又要新增一種會影響統計的訊息類型，記得同時更新 RegisterNewListViewItemStats。

    /// <summary>累加：留言數量（不含系統／會員事件／導向／置頂等類型）</summary>
    private int SharedChatCount = 0;

    /// <summary>累加：超級留言數量</summary>
    private int SharedSuperChatCount = 0;

    /// <summary>累加：超級貼圖數量</summary>
    private int SharedSuperStickerCount = 0;

    /// <summary>累加：加入會員人數</summary>
    private int SharedMemberJoinCount = 0;

    /// <summary>累加：會員升級人數</summary>
    private int SharedMemberUpgradeCount = 0;

    /// <summary>累加：會員里程碑人數</summary>
    private int SharedMemberMilestoneCount = 0;

    /// <summary>累加：贈送會員人數</summary>
    private int SharedMemberGiftCount = 0;

    /// <summary>累加：接收會員贈送人數</summary>
    private int SharedReceivedMemberGiftCount = 0;

    /// <summary>
    /// 累加：依貨幣符號分類的超級留言／貼圖原始金額加總（key 為貨幣符號，例如 "NT$"、"US$"）。
    /// <para>刻意不做匯率換算、也不合併成單一數字——不同貨幣直接相加沒有意義，
    /// 且沒有不會過期的匯率來源可用，見 <see cref="RegisterNewListViewItemStats"/> 內的
    /// <see cref="TryParsePurchaseAmount"/>。</para>
    /// </summary>
    private readonly Dictionary<string, double> SharedIncomeByCurrency = new(StringComparer.Ordinal);

    /// <summary>
    /// 目前聊天室內、具有會員徽章且訊息類型不是加入／升級／里程碑事件本身的不重複作者名稱集合
    /// （對應「會員人數」統計）。
    /// </summary>
    private readonly HashSet<string> SharedMemberInRoomAuthors = new(StringComparer.Ordinal);

    /// <summary>
    /// 目前聊天室內、排除系統訊息與會員相關事件後的不重複作者名稱集合（對應「留言人數」統計）。
    /// </summary>
    private readonly HashSet<string> SharedDistinctAuthors = new(StringComparer.Ordinal);

    #endregion
}