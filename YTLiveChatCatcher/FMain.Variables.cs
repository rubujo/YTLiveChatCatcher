using LiveChatCatcher;
using LiveChatCatcher.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace YTLiveChatCatcher;

// 阻擋設計工具。
partial class DesignerBlocker { };

/// <summary>
/// FMain 變數
/// </summary>
public partial class FMain
{
    private bool IsStreaming = false;
    /// <summary>
    /// 是否取得大張的影像檔
    /// </summary>
    private bool FetchLargePicture = true;
    private Catcher SharedCatcher = new();

    public readonly ILogger<FMain> _logger;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ToolTip SharedTooltip = new();
    private readonly List<StickerData> SharedStickers = [];
    private readonly List<EmojiData> SharedCustomEmojis = [];
    private readonly List<BadgeData> SharedBadges = [];

    [GeneratedRegex("(?:(http|https):\\/\\/(?:www\\.)?youtu\\.?be(?:\\.com)?\\/(?:embed\\/|watch\\?v=|\\?v=|v\\/|e\\/|[^\\[]+\\/|watch.*v=)?)")]
    private static partial Regex RegexYouTubeUrl();
}