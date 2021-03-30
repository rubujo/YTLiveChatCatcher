namespace YTLiveChatCatcher.Models;

/// <summary>
/// 必要資料類別
/// </summary>
public class YTConfig
{
    /// <summary>
    /// 初始頁面
    /// </summary>
    public string? InitPage { get; set; }

    /// <summary>
    /// API 金鑰
    /// </summary>
    public string? APIKey { get; set; }

    /// <summary>
    /// Continuation
    /// </summary>
    public string? Continuation { get; set; }

    /// <summary>
    /// visitorData
    /// </summary>
    public string? VisitorData { get; set; }

    /// <summary>
    /// clientName
    /// </summary>
    public string? ClientName { get; set; }

    /// <summary>
    /// clientVersion
    /// </summary>
    public string? ClientVersion { get; set; }

    /// <summary>
    /// ID_TOKEN
    /// </summary>
    public string? ID_TOKEN { get; set; }

    /// <summary>
    /// DATASYNC_ID
    /// </summary>
    public string? DATASYNC_ID { get; set; }

    /// <summary>
    /// DELEGATED_SESSION_ID
    /// </summary>
    public string? DELEGATED_SESSION_ID { get; set; }

    /// <summary>
    /// SESSION_INDEX
    /// </summary>
    public string? SESSION_INDEX { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT_CLIENT_NAME
    /// </summary>
    public string? INNERTUBE_CONTEXT_CLIENT_NAME { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT_CLIENT_VERSION
    /// </summary>
    public string? INNERTUBE_CONTEXT_CLIENT_VERSION { get; set; }

    /// <summary>
    /// INNERTUBE_CLIENT_VERSION
    /// </summary>
    public string? INNERTUBE_CLIENT_VERSION { get; set; }
}