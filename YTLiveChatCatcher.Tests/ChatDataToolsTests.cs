using System.IO.Compression;
using Rubujo.YouTube.Utility.Models.LiveChat;
using Xunit;
using YTLiveChatCatcher.Common.Utils;

namespace YTLiveChatCatcher.Tests;

public class ChatDataToolsTests
{
    private static readonly RendererData[] Messages =
    [
        new()
        {
            ID = "1", Type = "一般留言", AuthorName = "Alice", MessageContent = "a,b\n\"c\"",
            TimestampUsec = "1756872000000000"
        },
        new()
        {
            ID = "2", Type = "超級留言", AuthorName = "Bob", PurchaseAmountText = "NT$100",
            TimestampUsec = "1756872060000000"
        }
    ];

    [Fact]
    public void Filter_可組合類型作者與金額條件()
    {
        IReadOnlyList<RendererData> result = ChatDataTools.Filter(Messages,
            new ChatFilterOptions(MessageType: "超級留言", Author: "bo", MinimumAmount: 50, MaximumAmount: 150));

        RendererData message = Assert.Single(result);
        Assert.Equal("2", message.ID);
    }

    [Fact]
    public void Analyze_產生密度活躍作者付費時間軸與幣別分布()
    {
        ChatAnalytics result = ChatDataTools.Analyze(Messages);

        Assert.Equal(2, result.MessageCount);
        Assert.Equal(2, result.MessagesByMinute.Count);
        Assert.Equal(1, result.ActiveAuthors["Alice"]);
        Assert.Equal(100m, result.AmountsByCurrency["NT$"]);
        Assert.Single(result.PaidTimeline);
    }

    [Fact]
    public void ExportJsonLines與Csv_保留完整資料並正確逸出()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"ytlc-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            string jsonl = Path.Combine(directory, "data.jsonl");
            string csv = Path.Combine(directory, "data.csv");
            ChatDataTools.ExportJsonLines(jsonl, Messages);
            ChatDataTools.ExportCsv(csv, Messages);

            Assert.Equal(2, File.ReadLines(jsonl).Count());
            Assert.Contains("\"messageContent\":\"a,b\\n\\u0022c\\u0022\"", File.ReadAllText(jsonl));
            Assert.Contains("\"a,b", File.ReadAllText(csv));
            Assert.Contains("\"\"c\"\"", File.ReadAllText(csv));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void DiagnosticBundle_遮蔽敏感欄位並包含必要檔案()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"ytlc-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            string log = Path.Combine(directory, "log.txt");
            string zip = Path.Combine(directory, "bundle.zip");
            File.WriteAllText(log, "Authorization: secret\nCookie: SID=secret\n{\"continuation\":\"token-value\"}");
            DiagnosticBundleBuilder.Create(zip, null, Messages, log);

            using ZipArchive archive = ZipFile.OpenRead(zip);
            Assert.NotNull(archive.GetEntry("environment.json"));
            Assert.NotNull(archive.GetEntry("sanitized-structure-fixture.jsonl"));
            ZipArchiveEntry logEntry = Assert.IsType<ZipArchiveEntry>(archive.GetEntry("log-sanitized.txt"));
            using StreamReader reader = new(logEntry.Open());
            string content = reader.ReadToEnd();
            Assert.DoesNotContain("secret", content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("token-value", content, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
