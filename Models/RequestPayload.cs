using System.Text.Json.Serialization;

namespace YTLiveChatCatcher.Models;

public class RequestPayload
{
    [JsonPropertyName("context")]
    public Context? Context { get; set; }

    [JsonPropertyName("continuation")]
    public string? Continuation { get; set; }

    [JsonPropertyName("currentPlayerState")]
    public CurrentPlayerState? CurrentPlayerState { get; set; }
}

public class Context
{
    [JsonPropertyName("client")]
    public Client? Client { get; set; }

    [JsonPropertyName("user")]
    public User? User { get; set; }

    [JsonPropertyName("request")]
    public Request? Request { get; set; }

    [JsonPropertyName("clickTracking")]
    public ClickTracking? ClickTracking { get; set; }

    [JsonPropertyName("adSignalsInfo")]
    public AdSignalsInfo? AdSignalsInfo { get; set; }
}

public class Client
{
    [JsonPropertyName("hl")]
    public string? Hl { get; set; }

    [JsonPropertyName("gl")]
    public string? Gl { get; set; }

    [JsonPropertyName("remoteHost")]
    public string? RemoteHost { get; set; }

    [JsonPropertyName("deviceMake")]
    public string? DeviceMake { get; set; }

    [JsonPropertyName("deviceModel")]
    public string? DeviceModel { get; set; }

    [JsonPropertyName("visitorData")]
    public string? VisitorData { get; set; }

    [JsonPropertyName("userAgent")]
    public string? UserAgent { get; set; }

    [JsonPropertyName("clientName")]
    public string? ClientName { get; set; }

    [JsonPropertyName("clientVersion")]
    public string? ClientVersion { get; set; }

    [JsonPropertyName("osName")]
    public string? OsName { get; set; }

    [JsonPropertyName("osVersion")]
    public string? OsVersion { get; set; }

    [JsonPropertyName("originalUrl")]
    public string? OriginalUrl { get; set; }

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("clientFormFactor")]
    public string? ClientFormFactor { get; set; }

    [JsonPropertyName("configInfo")]
    public ConfigInfo? ConfigInfo { get; set; }

    [JsonPropertyName("timeZone")]
    public string? TimeZone { get; set; }

    [JsonPropertyName("browserName")]
    public string? BrowserName { get; set; }

    [JsonPropertyName("browserVersion")]
    public string? BrowserVersion { get; set; }

    [JsonPropertyName("acceptHeader")]
    public string? AcceptHeader { get; set; }

    [JsonPropertyName("deviceExperimentId")]
    public string? DeviceExperimentId { get; set; }

    [JsonPropertyName("screenWidthPoints")]
    public int ScreenWidthPoints { get; set; }

    [JsonPropertyName("screenHeightPoints")]
    public int ScreenHeightPoints { get; set; }

    [JsonPropertyName("screenPixelDensity")]
    public int ScreenPixelDensity { get; set; }

    [JsonPropertyName("screenDensityFloat")]
    public int ScreenDensityFloat { get; set; }

    [JsonPropertyName("utcOffsetMinutes")]
    public int UtcOffsetMinutes { get; set; }

    [JsonPropertyName("userInterfaceTheme")]
    public string? UserInterfaceTheme { get; set; }

    [JsonPropertyName("connectionType")]
    public string? ConnectionType { get; set; }

    [JsonPropertyName("memoryTotalKbytes")]
    public string? MemoryTotalKbytes { get; set; }

    [JsonPropertyName("mainAppWebInfo")]
    public MainAppWebInfo? MainAppWebInfo { get; set; }
}

public class ConfigInfo
{
    [JsonPropertyName("appInstallData")]
    public string? AppInstallData { get; set; }
}

public class MainAppWebInfo
{
    [JsonPropertyName("graftUrl")]
    public string? GraftUrl { get; set; }

    [JsonPropertyName("webDisplayMode")]
    public string? WebDisplayMode { get; set; }

    [JsonPropertyName("isWebNativeShareAvailable")]
    public bool? IsWebNativeShareAvailable { get; set; }
}

public class User
{
    [JsonPropertyName("lockedSafetyMode")]
    public bool LockedSafetyMode { get; set; }
}

public class Request
{
    [JsonPropertyName("useSsl")]
    public bool UseSsl { get; set; }

    [JsonPropertyName("internalExperimentFlags")]
    public List<object>? InternalExperimentFlags { get; set; }

    [JsonPropertyName("consistencyTokenJars")]
    public List<object>? ConsistencyTokenJars { get; set; }
}

public class ClickTracking
{
    [JsonPropertyName("clickTrackingParams")]
    public string? ClickTrackingParams { get; set; }
}

public class AdSignalsInfo
{
    [JsonPropertyName("params")]
    public List<Param>? Params { get; set; }
}

public class Param
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

public class CurrentPlayerState
{
    [JsonPropertyName("playerOffsetMs")]
    public string? PlayerOffsetMs { get; set; }
}