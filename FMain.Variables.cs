using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;
using YTLiveChatCatcher.Models;

namespace YTLiveChatCatcher;

// 阻擋設計工具。
partial class DesignerBlocker { };

public partial class FMain
{
    private bool IsStreaming = false;
    private int SharedTimeoutMs = 0;
    private System.Windows.Forms.Timer? SharedTimer;
    private JsonElement? SharedJsonElement = new();
    private YTConfig? SharedYTConfig = null;
    private Task? SharedTask = null;
    private CancellationToken? SharedCancellationToken = null;
    private CancellationTokenSource? SharedCancellationTokenSource = null;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FMain> _logger;
    private readonly ToolTip SharedTooltip = new();
    private readonly List<EmojiData> SharedCustomEmojis = new();
    private readonly List<BadgeData> SharedBadges = new();

    [GeneratedRegex("(?:(http|https):\\/\\/(?:www\\.)?youtu\\.?be(?:\\.com)?\\/(?:embed\\/|watch\\?v=|\\?v=|v\\/|e\\/|[^\\[]+\\/|watch.*v=)?)")]
    private static partial Regex RegexYouTubeUrl();
}