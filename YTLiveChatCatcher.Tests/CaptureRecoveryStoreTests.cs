using Rubujo.YouTube.Utility.Models.LiveChat;
using YTLiveChatCatcher.Common.Utils;
using Xunit;

namespace YTLiveChatCatcher.Tests;

// 只測試 SerializeBatchLine／ParseBatchLines 這組不依賴真實檔案系統的純邏輯，
// 刻意不測 AppendBatch／LoadBatches／Clear／Exists——那幾個方法固定寫死存取
// %LocalAppData%\YTLiveChatCatcher\recovery.jsonl，是使用者實際執行這個應用程式時
// 真正會用到的同一個檔案，測試如果直接操作它，可能會覆寫掉使用者自己真實的復原記錄。
public class CaptureRecoveryStoreTests
{
    [Fact]
    public void SerializeBatchLine與ParseBatchLines_可以正確往返還原資料()
    {
        List<RendererData> batch =
        [
            new RendererData { ID = "msg-1", AuthorName = "測試作者", MessageContent = "測試內容" },
            new RendererData { ID = "msg-2", AuthorName = "測試作者2", MessageContent = "測試內容2" },
        ];

        string line = CaptureRecoveryStore.SerializeBatchLine(batch);

        // JSON Lines 格式要求單行不能含有換行字元，否則會被誤判成多行。
        Assert.DoesNotContain('\n', line);
        Assert.DoesNotContain('\r', line);

        List<List<RendererData>> parsed = CaptureRecoveryStore.ParseBatchLines([line]);

        List<RendererData> parsedBatch = Assert.Single(parsed);
        Assert.Equal(2, parsedBatch.Count);
        Assert.Equal("msg-1", parsedBatch[0].ID);
        Assert.Equal("測試作者", parsedBatch[0].AuthorName);
        Assert.Equal("測試內容2", parsedBatch[1].MessageContent);
    }

    [Fact]
    public void ParseBatchLines_遇到毀損的單行時_略過該行但保留其它完整的批次()
    {
        List<RendererData> validBatch = [new RendererData { ID = "msg-1", AuthorName = "作者A" }];

        string validLine = CaptureRecoveryStore.SerializeBatchLine(validBatch);
        string corruptedLine = validLine[..(validLine.Length / 2)]; // 模擬寫到一半時當機，只寫了半行

        List<List<RendererData>> parsed = CaptureRecoveryStore.ParseBatchLines([validLine, corruptedLine]);

        List<RendererData> onlyBatch = Assert.Single(parsed);
        Assert.Equal("msg-1", onlyBatch[0].ID);
    }

    [Fact]
    public void ParseBatchLines_空白行與空字串會被略過()
    {
        List<List<RendererData>> parsed = CaptureRecoveryStore.ParseBatchLines(["", "   ", Environment.NewLine]);

        Assert.Empty(parsed);
    }

    [Fact]
    public void ParseBatchLines_沒有任何輸入時_回傳空清單()
    {
        List<List<RendererData>> parsed = CaptureRecoveryStore.ParseBatchLines([]);

        Assert.Empty(parsed);
    }
}
