namespace YTApi.Models;

/// <summary>
/// 必要資料類別
/// </summary>
public class YTConfigData
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
    public string? IDToken { get; set; }

    /// <summary>
    /// DATASYNC_ID
    /// </summary>
    public string? DataSyncID { get; set; }

    /// <summary>
    /// DELEGATED_SESSION_ID
    /// </summary>
    public string? DelegatedSessionID { get; set; }

    /// <summary>
    /// SESSION_INDEX
    /// </summary>
    public string? SessionIndex { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT_CLIENT_NAME
    /// </summary>
    public string? InnetrubeContextClientName { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT_CLIENT_VERSION
    /// </summary>
    public string? InnetrubeContextClientVersion { get; set; }

    /// <summary>
    /// INNERTUBE_CLIENT_VERSION
    /// </summary>
    public string? InnetrubeClientVersion { get; set; }
}