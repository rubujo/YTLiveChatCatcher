using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models;

/// <summary>
/// ytcfg 資料
/// </summary>
public class YTConfigData
{
    /// <summary>
    /// 初始頁面
    /// </summary>
    [JsonPropertyName("initPage")]
    public string? InitPage { get; set; }

    /// <summary>
    /// Continuation
    /// </summary>
    [JsonPropertyName("continuation")]
    public string? Continuation { get; set; }

    /// <summary>
    /// API 金鑰
    /// </summary>
    [JsonPropertyName("apiKey")]
    public string? APIKey { get; set; }

    /// <summary>
    /// ID_TOKEN
    /// </summary>
    [JsonPropertyName("ID_TOKEN")]
    public string? IDToken { get; set; }

    /// <summary>
    /// SESSION_INDEX
    /// </summary>
    [JsonPropertyName("SESSION_INDEX")]
    public string? SessionIndex { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT_CLIENT_NAME
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT_CLIENT_NAME")]
    public int InnertubeContextClientName { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT_CLIENT_VERSION
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT_CLIENT_VERSION")]
    public string? InnertubeContextClientVersion { get; set; }

    /// <summary>
    /// INNERTUBE_CLIENT_VERSION
    /// </summary>
    [JsonPropertyName("INNERTUBE_CLIENT_VERSION")]
    public string? InnertubeClientVersion { get; set; }

    /// <summary>
    /// DATASYNC_ID
    /// </summary>
    [JsonPropertyName("DATASYNC_ID")]
    public string? DataSyncID { get; set; }

    /// <summary>
    /// DELEGATED_SESSION_ID
    /// </summary>
    [JsonPropertyName("DELEGATED_SESSION_ID")]
    public string? DelegatedSessionID { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.browserName
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.browserName")]
    public string? BrowserName { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.browserVersion
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.browserVersion")]
    public string? BrowserVersion { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.clientFormFactor
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.clientFormFactor")]
    public string? ClientFormFactor { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.clientName
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.clientName")]
    public string? ClientName { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.clientVersion
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.clientVersion")]
    public string? ClientVersion { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.deviceMake
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.deviceMake")]
    public string? DeviceMake { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.deviceModel
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.deviceModel")]
    public string? DeviceModel { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.gl
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.gl")]
    public string? Gl { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.hl
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.hl")]
    public string? Hl { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.originalUrl
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.originalUrl")]
    public string? OriginalUrl { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.osName
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.osName")]
    public string? OsName { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.osVersion
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.osVersion")]
    public string? OsVersion { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.platform
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.platform")]
    public string? Platform { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.remoteHost
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.remoteHost")]
    public string? RemoteHost { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.userAgent
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.userAgent")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// INNERTUBE_CONTEXT.client.visitorData
    /// </summary>
    [JsonPropertyName("INNERTUBE_CONTEXT.client.visitorData")]
    public string? VisitorData { get; set; }

    /// <summary>
    /// timeZone
    /// </summary>
    [JsonPropertyName("timeZone")]
    public string? TimeZone { get; set; }

    /// <summary>
    /// 是否需要透過「重新載入」流程取得聊天室重播內容（見 GetReplayReloadContinuationAsync 的說明）。
    /// 為 true 時，GetJsonElementAsync 改打 get_live_chat_replay 端點，而不是一般輪詢用的 get_live_chat。
    /// 純粹是內部狀態，不對應任何 ytcfg／ytInitialData 欄位，不需要 JsonPropertyName。
    /// </summary>
    public bool IsReplayReload { get; set; }
}