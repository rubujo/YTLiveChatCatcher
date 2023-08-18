namespace YTApi.Models;

/// <summary>
/// ytcfg 資料
/// </summary>
public class YTConfigData
{
    /// <summary>
    /// 初始頁面
    /// </summary>
    public string? InitPage { get; set; }

    /// <summary>
    /// Continuation
    /// </summary>
    public string? Continuation { get; set; }

    /// <summary>
    /// API 金鑰
    /// </summary>
    public string? APIKey { get; set; }

    /// <summary>
    /// ID_TOKEN
    /// </summary>
    public string? IDToken { get; set; }

    /// <summary>
    /// SESSION_INDEX
    /// </summary>
    public string? SessionIndex { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT_CLIENT_NAME
    /// </summary>
    public int InnetrubeContextClientName { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT_CLIENT_VERSION
    /// </summary>
    public string? InnetrubeContextClientVersion { get; set; }

    /// <summary>
    /// INNERTUBE_CLIENT_VERSION
    /// </summary>
    public string? InnetrubeClientVersion { get; set; }

    /// <summary>
    /// DATASYNC_ID
    /// </summary>
    public string? DataSyncID { get; set; }

    /// <summary>
    /// DELEGATED_SESSION_ID
    /// </summary>
    public string? DelegatedSessionID { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.browserName
    /// </summary>
    public string? BrowserName { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.browserVersion
    /// </summary>
    public string? BrowserVersion { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.clientFormFactor
    /// </summary>
    public string? ClientFormFactor { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.clientName
    /// </summary>
    public string? ClientName { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.clientVersion
    /// </summary>
    public string? ClientVersion { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.deviceMake
    /// </summary>
    public string? DeviceMake { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.deviceModel
    /// </summary>
    public string? DeviceModel { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.gl
    /// </summary>
    public string? Gl { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.hl
    /// </summary>
    public string? Hl { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.originalUrl
    /// </summary>
    public string? OriginalUrl { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.osName
    /// </summary>
    public string? OsName { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.osVersion
    /// </summary>
    public string? OsVersion { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.platform
    /// </summary>
    public string? Platform { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.remoteHost
    /// </summary>
    public string? RemoteHost { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.userAgent
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.visitorData
    /// </summary>
    public string? VisitorData { get; set; }
}