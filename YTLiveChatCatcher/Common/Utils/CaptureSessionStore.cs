using System.Text.Json;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>
/// 一次聊天室擷取工作的可持久化狀態，用於辨識資料完整性與嘗試斷點續傳。
/// </summary>
public sealed record CaptureSessionManifest
{
    public required string SessionId { get; init; }
    public required string VideoId { get; init; }
    public string? VideoTitle { get; set; }
    public required string AppVersion { get; init; }
    public required DateTimeOffset StartedAtUtc { get; init; }
    public DateTimeOffset? EndedAtUtc { get; set; }
    public string? LastContinuation { get; set; }
    public bool IsReplay { get; set; }
    public long MessageCount { get; set; }
    public CaptureSessionEndReason EndReason { get; set; } = CaptureSessionEndReason.Running;
    public bool IsDataComplete { get; set; }
    public string? FailureMessage { get; set; }
}

/// <summary>
/// 擷取 session 的結束原因
/// </summary>
public enum CaptureSessionEndReason
{
    Running,
    Completed,
    UserStopped,
    Cancelled,
    Failed
}

/// <summary>
/// 原子寫入／讀取目前擷取 session manifest。
/// </summary>
public static class CaptureSessionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "YTLiveChatCatcher",
        "capture-session.json");

    public static void Save(CaptureSessionManifest manifest)
    {
        string? directory = Path.GetDirectoryName(FilePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string temporaryPath = FilePath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(manifest, JsonOptions));
        File.Move(temporaryPath, FilePath, overwrite: true);
    }

    public static CaptureSessionManifest? Load()
    {
        if (!File.Exists(FilePath))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CaptureSessionManifest>(File.ReadAllText(FilePath), JsonOptions);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return null;
        }
    }

    public static void Clear()
    {
        File.Delete(FilePath);

        string temporaryPath = FilePath + ".tmp";

        if (File.Exists(temporaryPath))
        {
            File.Delete(temporaryPath);
        }
    }
}
