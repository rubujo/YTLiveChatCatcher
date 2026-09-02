using System.Text.Json;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>收益估算設定；比例只是粗略估算，不代表 YouTube 實際結算。</summary>
public static class RevenueEstimateSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "YTLiveChatCatcher",
        "revenue-settings.json");

    public static decimal LoadRate()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                decimal rate = JsonSerializer.Deserialize<RevenueSettings>(File.ReadAllText(FilePath))?.Rate ?? 0.70m;
                return Math.Clamp(rate, 0m, 1m);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
        }

        return 0.70m;
    }

    public static void SaveRate(decimal rate)
    {
        string? directory = Path.GetDirectoryName(FilePath);
        Directory.CreateDirectory(directory!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(new RevenueSettings(Math.Clamp(rate, 0m, 1m))));
    }

    private sealed record RevenueSettings(decimal Rate);
}
