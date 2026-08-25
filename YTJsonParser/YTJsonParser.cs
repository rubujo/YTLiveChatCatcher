using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Models.Community;
using Rubujo.YouTube.Utility.Models.LiveChat;
using Rubujo.YouTube.Utility.Sets;
using Rubujo.YouTube.Utility.Utils;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// YTJsonParser
/// </summary>
public partial class YTJsonParser : IDisposable
{
    /// <summary>
    /// 未設定強制間隔毫秒值時，輪詢間隔的安全下限（毫秒）。
    /// <para>避免因回應內容解析失敗等異常狀況，導致間隔值意外變成 0 而對 YouTube 形成近乎無間隔的高頻輪詢。</para>
    /// </summary>
    private const int MinimumIntervalMs = 1000;

    /// <summary>
    /// 建構 YTJsonParser
    /// </summary>
    /// <param name="options">YTJsonParserOptions，預設值為 null（使用預設設定）</param>
    /// <param name="logger">ILogger&lt;YTJsonParser&gt;，預設值為 null（不記錄）</param>
    public YTJsonParser(YTJsonParserOptions? options = null, ILogger<YTJsonParser>? logger = null)
    {
        options ??= new YTJsonParserOptions();

        _logger = logger ?? NullLogger<YTJsonParser>.Instance;

        SharedCookies = options.Cookies ?? string.Empty;
        SharedIsFetchLargePicture = options.FetchLargePicture;
        SharedDisplayLanguage = options.DisplayLanguage;

        // 當未指定 HttpClient 時，自動建立並記錄該 HttpClient 為本實例所擁有。
        OwnsHttpClient = options.HttpClient == null;
        SharedHttpClient = options.HttpClient ?? CreateHttpClient();
    }

    /// <summary>
    /// 釋放本實例自動建立的 HttpClient（透過建構子傳入的 HttpClient 則不會被釋放）
    /// </summary>
    private void DisposeOwnedHttpClient()
    {
        if (OwnsHttpClient)
        {
            SharedHttpClient?.Dispose();
        }

        OwnsHttpClient = false;
    }

    /// <summary>
    /// 釋放資源
    /// </summary>
    public void Dispose()
    {
        DisposeOwnedHttpClient();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 串流獲取即時聊天資料，直到列舉結束或 <paramref name="cancellationToken"/> 被取消為止
    /// </summary>
    /// <param name="videoUrlOrID">字串，YouTube 影片網址或是 ID 值</param>
    /// <param name="options">LiveChatStreamOptions，預設值為 null（使用預設設定）</param>
    /// <param name="intervalProgress">IProgress&lt;int&gt;，每次輪詢間隔更新時回報目前的間隔毫秒值，預設值為 null</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>IAsyncEnumerable&lt;IReadOnlyList&lt;RendererData&gt;&gt;，每次列舉為一次輪詢取得的批次訊息</returns>
    public async IAsyncEnumerable<IReadOnlyList<RendererData>> StreamLiveChatDataAsync(
        string videoUrlOrID,
        LiveChatStreamOptions? options = null,
        IProgress<int>? intervalProgress = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new LiveChatStreamOptions();

        string videoID = YouTubeUrlUtil.GetYouTubeVideoID(videoUrlOrID);

        InitialData initialData = await GetYTConfigDataAsync(videoID, EnumSet.DataType.LiveChat, options, cancellationToken);

        YTConfigData? ytConfigData = initialData.YTConfigData;

        if (ytConfigData == null)
        {
            LogMessages.YtConfigDataIsNull(_logger, nameof(StreamLiveChatDataAsync));

            yield break;
        }

        // 處理初始頁面內的聊天室內容。
        if (initialData.Messages != null && initialData.Messages.Count > 0)
        {
            yield return initialData.Messages;
        }

        int intervalMs = MinimumIntervalMs;

        // 持續取得即時聊天資料。
        while (!cancellationToken.IsCancellationRequested)
        {
            JsonElement jsonElement = await GetJsonElementAsync(ytConfigData, EnumSet.DataType.LiveChat, cancellationToken);

            if (string.IsNullOrEmpty(jsonElement.ToString()))
            {
                break;
            }

            // 0：continuation、1：timeoutMs 或 timeUntilLastMessageMsec。
            string[] continuationData = ParseContinuation(jsonElement);

            ytConfigData.Continuation = continuationData[0];

            int.TryParse(continuationData[1], out int parsedIntervalMs);

            intervalMs = GetEffectiveIntervalMs(parsedIntervalMs, options.ForceIntervalMs);

            intervalProgress?.Report(intervalMs);

            List<RendererData> messages = ParseActions(jsonElement);

            if (messages.Count > 0)
            {
                yield return messages;
            }

            if (!await DelayOrBreakAsync(intervalMs, cancellationToken))
            {
                break;
            }
        }
    }

    /// <summary>
    /// 串流獲取社群貼文資料，直到列舉結束或 <paramref name="cancellationToken"/> 被取消為止
    /// </summary>
    /// <param name="channelUrlOrID">字串，YouTube 頻道網址或是 ID 值</param>
    /// <param name="options">CommunityPostStreamOptions，預設值為 null（使用預設設定）</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>IAsyncEnumerable&lt;IReadOnlyList&lt;PostData&gt;&gt;，每次列舉為一次輪詢取得的批次貼文</returns>
    public async IAsyncEnumerable<IReadOnlyList<PostData>> StreamCommunityPostsAsync(
        string channelUrlOrID,
        CommunityPostStreamOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new CommunityPostStreamOptions();

        string channelID = await YouTubeUrlUtil.GetYouTubeChannelID(channelUrlOrID, cancellationToken);

        InitialData initialData = await GetYTConfigDataAsync(
            channelID,
            EnumSet.DataType.Community,
            cancellationToken: cancellationToken);

        if (initialData.Posts == null || initialData.Posts.Count == 0)
        {
            LogMessages.NoInitialPosts(_logger);
        }

        YTConfigData? ytConfigData = initialData.YTConfigData;

        if (ytConfigData == null)
        {
            LogMessages.YtConfigDataIsNull(_logger, nameof(StreamCommunityPostsAsync));

            yield break;
        }

        if (initialData.Posts != null && initialData.Posts.Count > 0)
        {
            yield return initialData.Posts;
        }

        if (!options.FetchWholeCommunityPosts)
        {
            yield break;
        }

        // 持續取得社群資料。
        while (!cancellationToken.IsCancellationRequested &&
            !string.IsNullOrEmpty(ytConfigData.Continuation))
        {
            List<PostData> posts = await GetEarlierPostsAsync(ytConfigData, cancellationToken);

            if (posts.Count > 0)
            {
                yield return posts;
            }

            if (!await DelayOrBreakAsync(GetEffectiveIntervalMs(0, options.ForceIntervalMs), cancellationToken))
            {
                break;
            }
        }
    }

    /// <summary>
    /// 計算輪詢的有效間隔毫秒值
    /// </summary>
    /// <param name="parsedIntervalMs">數值，從 YouTube 回應解析出的間隔值</param>
    /// <param name="forceIntervalMs">數值，強制間隔毫秒值，未設定時為 null</param>
    /// <returns>數值</returns>
    private static int GetEffectiveIntervalMs(int parsedIntervalMs, int? forceIntervalMs) =>
        forceIntervalMs is >= 0 ? forceIntervalMs.Value : Math.Max(parsedIntervalMs, MinimumIntervalMs);

    /// <summary>
    /// 等待指定的毫秒數，若被取消則回傳 false
    /// </summary>
    /// <param name="delayMs">數值，等待毫秒數</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task&lt;bool&gt;</returns>
    private static async Task<bool> DelayOrBreakAsync(int delayMs, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delayMs, cancellationToken);

            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
