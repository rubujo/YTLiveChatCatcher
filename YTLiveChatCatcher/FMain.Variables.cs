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
}