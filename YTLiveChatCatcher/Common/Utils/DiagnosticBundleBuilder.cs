using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Rubujo.YouTube.Utility.Models.LiveChat;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>建立不含 Cookie、Token 與 continuation 的問題回報／結構漂移診斷包。</summary>
public static partial class DiagnosticBundleBuilder
{
    public static void Create(
        string destinationPath,
        CaptureSessionManifest? manifest,
        IEnumerable<RendererData> messages,
        string? logPath,
        IEnumerable<string>? sanitizedRawResponses = null)
    {
        using FileStream stream = File.Create(destinationPath);
        using ZipArchive archive = new(stream, ZipArchiveMode.Create);

        WriteEntry(archive, "environment.json", JsonSerializer.Serialize(new
        {
            appVersion = manifest?.AppVersion,
            osVersion = Environment.OSVersion.VersionString,
            framework = Environment.Version.ToString(),
            generatedAtUtc = DateTimeOffset.UtcNow
        }, new JsonSerializerOptions { WriteIndented = true }));

        if (manifest != null)
        {
            WriteEntry(archive, "session-manifest.json", JsonSerializer.Serialize(manifest with
            {
                LastContinuation = manifest.LastContinuation == null ? null : "[REDACTED]",
                FailureMessage = Redact(manifest.FailureMessage ?? string.Empty)
            }, new JsonSerializerOptions { WriteIndented = true }));
        }

        string fixture = string.Join('\n', messages.Select(message => Redact(JsonSerializer.Serialize(message))));
        WriteEntry(archive, "sanitized-structure-fixture.jsonl", fixture);

        if (sanitizedRawResponses != null)
        {
            int index = 1;

            foreach (string response in sanitizedRawResponses)
            {
                WriteEntry(archive, $"raw-responses/response-{index++:000}.json", Redact(response));
            }
        }

        if (!string.IsNullOrWhiteSpace(logPath) && File.Exists(logPath))
        {
            WriteEntry(archive, "log-sanitized.txt", Redact(File.ReadAllText(logPath)));
        }

        WriteEntry(archive, "README.txt", "此診斷包已自動遮蔽 Cookie、Authorization、Token、continuation 與常見 YouTube 身分欄位；傳送前仍請自行檢查內容。\r\n");
    }

    public static string Redact(string text)
    {
        string output = SensitiveHeaderRegex().Replace(text, "$1[REDACTED]");
        output = SensitiveJsonRegex().Replace(output, "$1\"[REDACTED]\"");
        return CookieRegex().Replace(output, "Cookie: [REDACTED]");
    }

    private static void WriteEntry(ZipArchive archive, string name, string content)
    {
        using StreamWriter writer = new(archive.CreateEntry(name, CompressionLevel.Optimal).Open(), new UTF8Encoding(false));
        writer.Write(content);
    }

    [GeneratedRegex("(?im)^(Authorization|X-Youtube-Identity-Token|Cookie)\\s*[:=]\\s*.*$")]
    private static partial Regex SensitiveHeaderRegex();

    [GeneratedRegex("(?i)(\\\"(?:continuation|token|id_token|visitorData|sessionIndex|cookie)\\\"\\s*:\\s*)\\\"[^\\\"]*\\\"")]
    private static partial Regex SensitiveJsonRegex();

    [GeneratedRegex("(?i)Cookie:\\s*[^\\r\\n]+")]
    private static partial Regex CookieRegex();
}
