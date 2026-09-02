using Rubujo.YouTube.Utility.Models.LiveChat;
using Xunit;
using YTLiveChatCatcher.Common.Utils;

namespace YTLiveChatCatcher.Tests;

public class CaptureMessageDeduplicatorTests
{
    [Fact]
    public void FilterNew_相同事件只保留一次但同ID不同事件仍保留()
    {
        CaptureMessageDeduplicator deduplicator = new();
        RendererData original = new() { ID = "message-1", Type = "一般留言", MessageContent = "內容" };
        RendererData deleted = new() { ID = "message-1", Type = "留言已被刪除" };

        Assert.Single(deduplicator.FilterNew([original]));
        Assert.Empty(deduplicator.FilterNew([original]));
        Assert.Single(deduplicator.FilterNew([deleted]));
    }

    [Fact]
    public void FilterNew_沒有ID的診斷事件不會被誤刪()
    {
        CaptureMessageDeduplicator deduplicator = new();
        RendererData diagnostic = new() { Type = "診斷", MessageContent = "內容" };

        Assert.Equal(2, deduplicator.FilterNew([diagnostic, diagnostic]).Count);
    }
}
