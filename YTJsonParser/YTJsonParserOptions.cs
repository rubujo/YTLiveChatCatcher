using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// YTJsonParser 實例層級的設定（建構時傳入，建立後不可變）
/// </summary>
public sealed record YTJsonParserOptions
{
    /// <summary>
    /// HttpClient
    /// <para>不指定時，會自動建立一個並由本實例負責釋放。</para>
    /// </summary>
    public HttpClient? HttpClient { get; init; }

    /// <summary>
    /// 顯示語言
    /// <para>不指定時，會依 System.Globalization.CultureInfo.CurrentUICulture 自動判斷；
    /// 找不到對應語言時退回英文。</para>
    /// </summary>
    public EnumSet.DisplayLanguage? DisplayLanguage { get; init; }

    /// <summary>
    /// 是否獲取大張圖片
    /// </summary>
    public bool FetchLargePicture { get; init; } = true;

    /// <summary>
    /// Cookies 字串
    /// <para>本函式庫不提供讀取／解密瀏覽器 Cookie 資料庫的方法，
    /// 請透過官方支援的介面（例如專屬登入視窗＋ CoreWebView2CookieManager，或使用者手動貼上）取得。</para>
    /// </summary>
    public string? Cookies { get; init; }
}

/// <summary>
/// 單次即時聊天串流的設定
/// </summary>
public sealed record LiveChatStreamOptions
{
    /// <summary>
    /// 即時聊天類型
    /// </summary>
    public EnumSet.LiveChatType LiveChatType { get; init; } = EnumSet.LiveChatType.All;

    /// <summary>
    /// 自定義即時聊天類型（title）
    /// <para>有設定時，會自動忽略 <see cref="LiveChatType"/> 的值。</para>
    /// </summary>
    public string? CustomLiveChatType { get; init; }

    /// <summary>
    /// 強制間隔毫秒值
    /// <para>不指定時，改用 YouTube 回應內容解析出的間隔值（並套用安全下限）。</para>
    /// </summary>
    public int? ForceIntervalMs { get; init; }

    /// <summary>
    /// 從先前 session manifest 保存的 continuation 嘗試續傳。
    /// <para>continuation 是 YouTube 的暫時性權杖，可能過期；呼叫端必須把續傳失敗視為資料可能不完整，
    /// 不能把「沒有拋例外」等同於完整取得所有訊息。</para>
    /// </summary>
    public string? ResumeContinuation { get; init; }
}

/// <summary>
/// 即時聊天串流目前的可持久化狀態
/// </summary>
/// <param name="Continuation">下一次請求使用的 continuation</param>
/// <param name="IsReplay">是否使用重播聊天室端點</param>
/// <param name="IntervalMs">YouTube 建議或呼叫端強制的輪詢間隔</param>
public sealed record LiveChatStreamStatus(string? Continuation, bool IsReplay, int IntervalMs);

/// <summary>
/// 單次社群貼文串流的設定
/// </summary>
public sealed record CommunityPostStreamOptions
{
    /// <summary>
    /// 是否要獲取全部的社群貼文
    /// </summary>
    public bool FetchWholeCommunityPosts { get; init; } = true;

    /// <summary>
    /// 強制間隔毫秒值
    /// <para>不指定時，改用 YouTube 回應內容解析出的間隔值（並套用安全下限）。</para>
    /// </summary>
    public int? ForceIntervalMs { get; init; }
}
