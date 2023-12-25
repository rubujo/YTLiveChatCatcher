using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models;

/// <summary>
/// RequestPayloadData
/// </summary>
public class RequestPayloadData
{
    /// <summary>
    /// context
    /// </summary>
    [JsonPropertyName("context")]
    public Context? Context { get; set; }

    /// <summary>
    /// continuation
    /// </summary>
    [JsonPropertyName("continuation")]
    public string? Continuation { get; set; }

    /// <summary>
    /// currentPlayerState
    /// </summary>
    [JsonPropertyName("currentPlayerState")]
    public CurrentPlayerState? CurrentPlayerState { get; set; }
}

/// <summary>
/// context
/// </summary>
public class Context
{
    /// <summary>
    /// client
    /// </summary>
    [JsonPropertyName("client")]
    public Client? Client { get; set; }

    /// <summary>
    /// user
    /// </summary>
    [JsonPropertyName("user")]
    public User? User { get; set; }

    /// <summary>
    /// request
    /// </summary>
    [JsonPropertyName("request")]
    public Request? Request { get; set; }

    /// <summary>
    /// clickTracking
    /// </summary>
    [JsonPropertyName("clickTracking")]
    public ClickTracking? ClickTracking { get; set; }

    /// <summary>
    /// adSignalsInfo
    /// </summary>
    [JsonPropertyName("adSignalsInfo")]
    public AdSignalsInfo? AdSignalsInfo { get; set; }
}

/// <summary>
/// client
/// </summary>
public class Client
{
    /// <summary>
    /// hl
    /// </summary>
    [JsonPropertyName("hl")]
    public string? Hl { get; set; }

    /// <summary>
    /// gl
    /// </summary>
    [JsonPropertyName("gl")]
    public string? Gl { get; set; }

    /// <summary>
    /// remoteHost
    /// </summary>
    [JsonPropertyName("remoteHost")]
    public string? RemoteHost { get; set; }

    /// <summary>
    /// deviceMake
    /// </summary>
    [JsonPropertyName("deviceMake")]
    public string? DeviceMake { get; set; }

    /// <summary>
    /// deviceModel
    /// </summary>
    [JsonPropertyName("deviceModel")]
    public string? DeviceModel { get; set; }

    /// <summary>
    /// visitorData
    /// </summary>
    [JsonPropertyName("visitorData")]
    public string? VisitorData { get; set; }

    /// <summary>
    /// userAgent
    /// </summary>
    [JsonPropertyName("userAgent")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// clientName
    /// </summary>
    [JsonPropertyName("clientName")]
    public string? ClientName { get; set; }

    /// <summary>
    /// clientVersion
    /// </summary>
    [JsonPropertyName("clientVersion")]
    public string? ClientVersion { get; set; }

    /// <summary>
    /// osName
    /// </summary>
    [JsonPropertyName("osName")]
    public string? OsName { get; set; }

    /// <summary>
    /// osVersion
    /// </summary>
    [JsonPropertyName("osVersion")]
    public string? OsVersion { get; set; }

    /// <summary>
    /// originalUrl
    /// </summary>
    [JsonPropertyName("originalUrl")]
    public string? OriginalUrl { get; set; }

    /// <summary>
    /// platform
    /// </summary>
    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    /// <summary>
    /// clientFormFactor
    /// </summary>
    [JsonPropertyName("clientFormFactor")]
    public string? ClientFormFactor { get; set; }

    /// <summary>
    /// configInfo
    /// </summary>
    [JsonPropertyName("configInfo")]
    public ConfigInfo? ConfigInfo { get; set; }

    /// <summary>
    /// timeZone
    /// </summary>
    [JsonPropertyName("timeZone")]
    public string? TimeZone { get; set; }

    /// <summary>
    /// browserName
    /// </summary>
    [JsonPropertyName("browserName")]
    public string? BrowserName { get; set; }

    /// <summary>
    /// browserVersion
    /// </summary>
    [JsonPropertyName("browserVersion")]
    public string? BrowserVersion { get; set; }

    /// <summary>
    /// acceptHeader
    /// </summary>
    [JsonPropertyName("acceptHeader")]
    public string? AcceptHeader { get; set; }

    /// <summary>
    /// deviceExperimentId
    /// </summary>
    [JsonPropertyName("deviceExperimentId")]
    public string? DeviceExperimentId { get; set; }

    /// <summary>
    /// screenWidthPoints
    /// </summary>
    [JsonPropertyName("screenWidthPoints")]
    public int ScreenWidthPoints { get; set; }

    /// <summary>
    /// screenHeightPoints
    /// </summary>
    [JsonPropertyName("screenHeightPoints")]
    public int ScreenHeightPoints { get; set; }

    /// <summary>
    /// screenPixelDensity
    /// </summary>
    [JsonPropertyName("screenPixelDensity")]
    public int ScreenPixelDensity { get; set; }

    /// <summary>
    /// screenDensityFloat
    /// </summary>
    [JsonPropertyName("screenDensityFloat")]
    public int ScreenDensityFloat { get; set; }

    /// <summary>
    /// utcOffsetMinutes
    /// </summary>
    [JsonPropertyName("utcOffsetMinutes")]
    public int UtcOffsetMinutes { get; set; }

    /// <summary>
    /// userInterfaceTheme
    /// </summary>
    [JsonPropertyName("userInterfaceTheme")]
    public string? UserInterfaceTheme { get; set; }

    /// <summary>
    /// connectionType
    /// </summary>
    [JsonPropertyName("connectionType")]
    public string? ConnectionType { get; set; }

    /// <summary>
    /// memoryTotalKbytes
    /// </summary>
    [JsonPropertyName("memoryTotalKbytes")]
    public string? MemoryTotalKbytes { get; set; }

    /// <summary>
    /// mainAppWebInfo
    /// </summary>
    [JsonPropertyName("mainAppWebInfo")]
    public MainAppWebInfo? MainAppWebInfo { get; set; }
}

/// <summary>
/// configInfo
/// </summary>
public class ConfigInfo
{
    /// <summary>
    /// appInstallData
    /// </summary>
    [JsonPropertyName("appInstallData")]
    public string? AppInstallData { get; set; }
}

/// <summary>
/// mainAppWebInfo
/// </summary>
public class MainAppWebInfo
{
    /// <summary>
    /// graftUrl
    /// </summary>
    [JsonPropertyName("graftUrl")]
    public string? GraftUrl { get; set; }

    /// <summary>
    /// webDisplayMode
    /// </summary>
    [JsonPropertyName("webDisplayMode")]
    public string? WebDisplayMode { get; set; }

    /// <summary>
    /// isWebNativeShareAvailable
    /// </summary>
    [JsonPropertyName("isWebNativeShareAvailable")]
    public bool? IsWebNativeShareAvailable { get; set; }
}

/// <summary>
/// user
/// </summary>
public class User
{
    /// <summary>
    /// lockedSafetyMode
    /// </summary>
    [JsonPropertyName("lockedSafetyMode")]
    public bool LockedSafetyMode { get; set; }
}

/// <summary>
/// request
/// </summary>
public class Request
{
    /// <summary>
    /// useSsl
    /// </summary>
    [JsonPropertyName("useSsl")]
    public bool UseSsl { get; set; }

    /// <summary>
    /// internalExperimentFlags
    /// </summary>
    [JsonPropertyName("internalExperimentFlags")]
    public List<object>? InternalExperimentFlags { get; set; }

    /// <summary>
    /// consistencyTokenJars
    /// </summary>
    [JsonPropertyName("consistencyTokenJars")]
    public List<object>? ConsistencyTokenJars { get; set; }
}

/// <summary>
/// clickTracking
/// </summary>
public class ClickTracking
{
    /// <summary>
    /// clickTrackingParams
    /// </summary>
    [JsonPropertyName("clickTrackingParams")]
    public string? ClickTrackingParams { get; set; }
}

/// <summary>
/// adSignalsInfo
/// </summary>
public class AdSignalsInfo
{
    /// <summary>
    /// params
    /// </summary>
    [JsonPropertyName("params")]
    public List<Param>? Params { get; set; }
}

/// <summary>
/// param
/// </summary>
public class Param
{
    /// <summary>
    /// key
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    /// <summary>
    /// value
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

/// <summary>
/// currentPlayerState
/// </summary>
public class CurrentPlayerState
{
    /// <summary>
    /// playerOffsetMs
    /// </summary>
    [JsonPropertyName("playerOffsetMs")]
    public string? PlayerOffsetMs { get; set; }
}