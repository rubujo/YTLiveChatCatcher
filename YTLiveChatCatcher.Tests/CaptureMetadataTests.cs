using System.Text.Json;
using OfficeOpenXml;
using Rubujo.YouTube.Utility.Models.LiveChat;
using Xunit;
using YTLiveChatCatcher.Common.Utils;

namespace YTLiveChatCatcher.Tests;

public class CaptureMetadataTests
{
    [Fact]
    public void 金額篩選要求幣別並將裸美元符號依臺灣語系正規化()
    {
        RendererData[] messages = [new() { ID = "tw", PurchaseAmountText = "$100" },
            new() { ID = "us", PurchaseAmountText = "US$100" }, new() { ID = "text" }];
        Assert.Throws<FormatException>(() => ChatDataTools.Filter(messages, new(MinimumAmount: 50)));
        Assert.Equal("tw", Assert.Single(ChatDataTools.Filter(messages, new(MinimumAmount: 50, Currency: "NT$"))).ID);
        Assert.Equal("us", Assert.Single(ChatDataTools.Filter(messages, new(Currency: "US$"))).ID);
        Assert.Throws<FormatException>(() => ChatDataTools.Filter(messages, new(MinimumAmount: 200, MaximumAmount: 50, Currency: "NT$")));
    }

    [Fact]
    public void 連續失敗合併區間且恢復後保留可能缺漏()
    {
        DateTimeOffset start = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        CaptureSessionManifest manifest = NewSession(start);
        CaptureSessionTimeline.ResponseReceived(manifest, start);
        CaptureSessionTimeline.Interrupt(manifest, start.AddMinutes(1), "NetworkFailure");
        CaptureSessionTimeline.Interrupt(manifest, start.AddMinutes(2), "NetworkFailure");
        CaptureSessionTimeline.ResponseReceived(manifest, start.AddMinutes(3));
        CaptureInterruption gap = Assert.Single(manifest.Interruptions);
        Assert.Equal(start, gap.FromUtc);
        Assert.Equal(start.AddMinutes(3), gap.ResumedAtUtc);
        Assert.False(manifest.IsDataComplete);
        CaptureSessionTimeline.Interrupt(manifest, start.AddMinutes(4), "UserStopped");
        Assert.Equal(2, manifest.Interruptions.Length);
    }

    [Fact]
    public void 匯出說明不帶權杖與例外且快照不隨原始session改變()
    {
        CaptureSessionManifest session = NewSession(DateTimeOffset.UtcNow);
        session.LastContinuation = "SECRET_TOKEN";
        session.FailureMessage = "SECRET_EXCEPTION";
        CaptureSessionTimeline.Interrupt(session, DateTimeOffset.UtcNow, "Failed");
        ChatExportMetadata metadata = ChatExportMetadata.Create(session, 2, new(Currency: "NT$"));
        CaptureSessionTimeline.ResponseReceived(session, DateTimeOffset.UtcNow);
        Assert.Null(Assert.Single(metadata.Interruptions).ResumedAtUtc);
        Assert.DoesNotContain("SECRET", metadata.ToJson());
        Assert.Equal("NT$", metadata.Filters?.Currency);
    }

    [Fact]
    public void 舊session缺少中斷欄位仍可讀取()
    {
        string json = JsonSerializer.Serialize(new { SessionId = "old", VideoId = "test", AppVersion = "2.0.7", StartedAtUtc = DateTimeOffset.UtcNow });
        CaptureSessionManifest session = JsonSerializer.Deserialize<CaptureSessionManifest>(json)!;
        Assert.Empty(session.Interruptions);
        Assert.Null(session.LastResponseAtUtc);
    }

    [Fact]
    public void 三種匯出均可攜帶相同說明且不改變資料筆數()
    {
        string directory = Path.Combine(Path.GetTempPath(), "ytlc-metadata-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            RendererData[] messages = [new() { ID = "1", MessageContent = "hello" }];
            ChatExportMetadata metadata = ChatExportMetadata.Create(NewSession(DateTimeOffset.UtcNow), 1);
            string jsonl = Path.Combine(directory, "chat.jsonl"), csv = Path.Combine(directory, "chat.csv");
            ChatDataTools.ExportJsonLines(jsonl, messages);
            ChatDataTools.ExportCsv(csv, messages);
            metadata.WriteSidecar(jsonl);
            metadata.WriteSidecar(csv);
            Assert.Equal(File.ReadAllText(jsonl + ".metadata.json"), File.ReadAllText(csv + ".metadata.json"));
            Assert.Single(ChatDataTools.ImportJsonLines(jsonl));
            Assert.Single(ChatDataTools.ImportCsv(csv));
            ExcelPackage.License.SetNonCommercialOrganization("Tests");
            using ExcelPackage package = new();
            metadata.WriteWorksheet(package.Workbook);
            using MemoryStream stream = new();
            package.SaveAs(stream);
            stream.Position = 0;
            using ExcelPackage loaded = new(stream);
            Assert.Equal("test", loaded.Workbook.Worksheets["擷取資訊"].Cells[1, 2].Text);
            Assert.Equal("1", loaded.Workbook.Worksheets["擷取資訊"].Cells[9, 2].Text);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static CaptureSessionManifest NewSession(DateTimeOffset start) => new()
    { SessionId = "test", VideoId = "test", AppVersion = "2.0.7", StartedAtUtc = start };
}
