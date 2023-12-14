using Microsoft.Extensions.Logging;
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Models;
using System.Text.RegularExpressions;

namespace YTLiveChatCatcher;

// 阻擋設計工具。
partial class DesignerBlocker { };

/// <summary>
/// FMain 的變數
/// </summary>
public partial class FMain
{
    /// <summary>
    /// 是否為直播
    /// </summary>
    private bool IsStreaming = false;

    /// <summary>
    /// 是否取得大張的影像檔
    /// </summary>
    private bool FetchLargePicture = true;

    /// <summary>
    /// 共用的 LiveChatCatcher
    /// </summary>
    private LiveChatCatcher SharedLiveChatCatcher = new();

    /// <summary>
    /// 共用的 HttpClient
    /// </summary>
    private HttpClient? SharedHttpClient;

    /// <summary>
    /// 共用的 ILogger&lt;FMain&gt;
    /// </summary>
    public readonly ILogger<FMain> SharedLogger;

    /// <summary>
    /// 共用的 IHttpClientFactory
    /// </summary>
    private readonly IHttpClientFactory SharedHttpClientFactory;

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