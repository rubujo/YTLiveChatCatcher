using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models;

/// <summary>
/// ytcfg 的原始 JSON 結構（僅供反序列化使用，對應 YouTube 網頁內 ytcfg.set({...}) 的內容）
/// </summary>
internal class YtCfgDto
{
    [JsonPropertyName("INNERTUBE_API_KEY")]
    public string? InnertubeApiKey { get; set; }

    [JsonPropertyName("ID_TOKEN")]
    public string? IdToken { get; set; }

    [JsonPropertyName("SESSION_INDEX")]
    public string? SessionIndex { get; set; }

    [JsonPropertyName("INNERTUBE_CONTEXT_CLIENT_NAME")]
    public int InnertubeContextClientName { get; set; }

    [JsonPropertyName("INNERTUBE_CONTEXT_CLIENT_VERSION")]
    public string? InnertubeContextClientVersion { get; set; }

    [JsonPropertyName("INNERTUBE_CLIENT_VERSION")]
    public string? InnertubeClientVersion { get; set; }

    [JsonPropertyName("DATASYNC_ID")]
    public string? DataSyncId { get; set; }

    [JsonPropertyName("DELEGATED_SESSION_ID")]
    public string? DelegatedSessionId { get; set; }

    [JsonPropertyName("INNERTUBE_CONTEXT")]
    public YtCfgInnertubeContextDto? InnertubeContext { get; set; }
}

/// <summary>
/// ytcfg 的 INNERTUBE_CONTEXT 內容
/// </summary>
internal class YtCfgInnertubeContextDto
{
    [JsonPropertyName("client")]
    public YtCfgClientDto? Client { get; set; }
}

/// <summary>
/// ytcfg 的 INNERTUBE_CONTEXT.client 內容
/// </summary>
internal class YtCfgClientDto
{
    [JsonPropertyName("browserName")]
    public string? BrowserName { get; set; }

    [JsonPropertyName("browserVersion")]
    public string? BrowserVersion { get; set; }

    [JsonPropertyName("clientFormFactor")]
    public string? ClientFormFactor { get; set; }

    [JsonPropertyName("clientName")]
    public string? ClientName { get; set; }

    [JsonPropertyName("clientVersion")]
    public string? ClientVersion { get; set; }

    [JsonPropertyName("deviceMake")]
    public string? DeviceMake { get; set; }

    [JsonPropertyName("deviceModel")]
    public string? DeviceModel { get; set; }

    [JsonPropertyName("gl")]
    public string? Gl { get; set; }

    [JsonPropertyName("hl")]
    public string? Hl { get; set; }

    [JsonPropertyName("originalUrl")]
    public string? OriginalUrl { get; set; }

    [JsonPropertyName("osName")]
    public string? OsName { get; set; }

    [JsonPropertyName("osVersion")]
    public string? OsVersion { get; set; }

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("remoteHost")]
    public string? RemoteHost { get; set; }

    [JsonPropertyName("userAgent")]
    public string? UserAgent { get; set; }

    [JsonPropertyName("visitorData")]
    public string? VisitorData { get; set; }
}
