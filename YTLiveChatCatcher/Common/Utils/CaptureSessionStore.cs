using System.Security.Cryptography;
using System.Text;
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
    public DateTimeOffset? LastResponseAtUtc { get; set; }
    public bool HasUnsupportedContent { get; set; }
    public CaptureInterruption[] Interruptions { get; set; } = [];
}

/// <summary>觀察到的擷取中斷區間；不推算漏訊息數，也不保證續傳已補齊。</summary>
public sealed record CaptureInterruption(DateTimeOffset? FromUtc, DateTimeOffset? ResumedAtUtc, string Reason);

public static class CaptureSessionTimeline
{
    public static void Interrupt(CaptureSessionManifest manifest, DateTimeOffset now, string reason)
    {
        if (manifest.Interruptions.LastOrDefault() is { ResumedAtUtc: null }) return;
        manifest.Interruptions = [.. manifest.Interruptions, new(manifest.LastResponseAtUtc ?? now, null, reason)];
        manifest.IsDataComplete = false;
    }

    public static void ResponseReceived(CaptureSessionManifest manifest, DateTimeOffset now)
    {
        if (manifest.Interruptions.LastOrDefault() is { ResumedAtUtc: null } interruption)
        {
            CaptureInterruption[] updated = [.. manifest.Interruptions];
            updated[^1] = interruption with { ResumedAtUtc = now };
            manifest.Interruptions = updated;
        }
        manifest.LastResponseAtUtc = now;
    }
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
    private const string EncryptedContentPrefix = "dpapi:";

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
        byte[] plainBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(manifest, JsonOptions));

        try
        {
            byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            File.WriteAllText(
                temporaryPath,
                EncryptedContentPrefix + Convert.ToBase64String(encryptedBytes),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            File.Move(temporaryPath, FilePath, overwrite: true);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plainBytes);
        }
    }

    public static CaptureSessionManifest? Load()
    {
        if (!File.Exists(FilePath))
        {
            return null;
        }

        try
        {
            string storedContent = File.ReadAllText(FilePath, Encoding.UTF8);
            bool isLegacyPlainText = !storedContent.StartsWith(EncryptedContentPrefix, StringComparison.Ordinal);
            string json = isLegacyPlainText ? storedContent : Unprotect(storedContent);
            CaptureSessionManifest? manifest = JsonSerializer.Deserialize<CaptureSessionManifest>(json, JsonOptions);

            // 舊版 manifest 是純文字 JSON；成功讀取後立即改存成 DPAPI 格式，避免敏感的
            // continuation 在升級後仍長期以明文留在磁碟上。
            if (isLegacyPlainText && manifest != null)
            {
                Save(manifest);
            }

            return manifest;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or FormatException or CryptographicException)
        {
            return null;
        }
    }

    private static string Unprotect(string storedContent)
    {
        byte[] encryptedBytes = Convert.FromBase64String(storedContent[EncryptedContentPrefix.Length..]);
        byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);

        try
        {
            return Encoding.UTF8.GetString(plainBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plainBytes);
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
