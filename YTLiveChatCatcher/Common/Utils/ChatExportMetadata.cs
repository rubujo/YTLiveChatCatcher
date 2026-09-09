using System.Text.Json;
using OfficeOpenXml;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>可攜出的擷取說明；刻意不包含 continuation、Cookie 或例外原文。</summary>
public sealed record ChatExportMetadata
{
    public int SchemaVersion { get; init; } = 1;
    public DateTimeOffset ExportedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public string? VideoId { get; init; }
    public string? VideoTitle { get; init; }
    public string? AppVersion { get; init; }
    public DateTimeOffset? StartedAtUtc { get; init; }
    public DateTimeOffset? EndedAtUtc { get; init; }
    public bool? IsDataComplete { get; init; }
    public bool HasUnsupportedContent { get; init; }
    public string? EndReason { get; init; }
    public int ExportedMessageCount { get; init; }
    public string Scope { get; init; } = "目前資料快照";
    public ChatFilterOptions? Filters { get; init; }
    public CaptureInterruption[] Interruptions { get; init; } = [];
    public string DataSemantics { get; init; } = "JSONL 保存 RendererData 模型，不包含所有 YouTube 原始欄位。中斷區間僅表示可能缺漏，無法推算漏訊息數；時間使用 UTC。";

    public static ChatExportMetadata Create(CaptureSessionManifest? session, int count,
        ChatFilterOptions? filters = null, string scope = "目前資料快照") => new()
    {
        VideoId = session?.VideoId,
        VideoTitle = session?.VideoTitle,
        AppVersion = session?.AppVersion,
        StartedAtUtc = session?.StartedAtUtc,
        EndedAtUtc = session?.EndedAtUtc,
        IsDataComplete = session?.IsDataComplete,
        HasUnsupportedContent = session?.HasUnsupportedContent ?? false,
        EndReason = session?.EndReason.ToString(),
        ExportedMessageCount = count,
        Scope = scope,
        Filters = filters,
        Interruptions = session?.Interruptions.ToArray() ?? []
    };

    public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

    public void WriteSidecar(string dataPath) => File.WriteAllText(dataPath + ".metadata.json", ToJson());

    public void WriteWorksheet(ExcelWorkbook workbook)
    {
        ExcelWorksheet sheet = workbook.Worksheets.Add("擷取資訊");
        (string Name, object? Value)[] rows =
        [
            ("影片 ID", VideoId), ("影片標題", VideoTitle), ("擷取版本", AppVersion),
            ("開始時間 UTC", StartedAtUtc?.ToString("O")), ("結束時間 UTC", EndedAtUtc?.ToString("O")),
            ("匯出時間 UTC", ExportedAtUtc.ToString("O")),
            ("資料完整性", IsDataComplete is null ? "未知（缺少來源資訊）" : IsDataComplete.Value ? "擷取正常結束；不保證涵蓋整場直播" : "可能不完整"),
            ("結束原因", EndReason), ("匯出筆數", ExportedMessageCount), ("匯出範圍", Scope),
            ("篩選條件", JsonSerializer.Serialize(Filters)), ("資料語意", DataSemantics),
            ("遇到未支援內容", HasUnsupportedContent ? "是，資料可能未完整解析" : "未記錄（不保證所有事件皆支援）")
        ];
        int row = 1;
        foreach (var item in rows)
        {
            sheet.Cells[row, 1].Value = item.Name;
            sheet.Cells[row++, 2].Value = item.Value;
        }
        sheet.Cells[++row, 1].Value = "可能缺漏起點 UTC";
        sheet.Cells[row, 2].Value = "收到恢復回應 UTC";
        sheet.Cells[row++, 3].Value = "原因（未估計漏訊息數）";
        foreach (CaptureInterruption gap in Interruptions)
        {
            sheet.Cells[row, 1].Value = gap.FromUtc?.ToString("O") ?? "未知";
            sheet.Cells[row, 2].Value = gap.ResumedAtUtc?.ToString("O") ?? "尚未恢復";
            sheet.Cells[row++, 3].Value = gap.Reason;
        }
        sheet.Column(1).Width = 32;
        sheet.Column(2).Width = 65;
        sheet.Column(3).Width = 38;
        sheet.Cells[sheet.Dimension.Address].Style.WrapText = true;
        sheet.DefaultRowHeight = 45;
    }
}
