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
    /// </summary>
    public EnumSet.DisplayLanguage DisplayLanguage { get; init; } = EnumSet.DisplayLanguage.Chinese_Traditional;

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
}

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
