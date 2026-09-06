using YTLiveChatCatcher.Common.Utils;
using Xunit;

namespace YTLiveChatCatcher.Tests;

public class ChatFallbackIdentityTests
{
    [Fact]
    public void 不同內容與金額不互相去重且缺少時間時保留()
    {
        var first = ChatFallbackIdentity.Create("channel", "作者", "123", "留言", "第一則", "NT$10");
        var second = ChatFallbackIdentity.Create("channel", "作者", "123", "留言", "第二則", "NT$10");
        var amount = ChatFallbackIdentity.Create("channel", "作者", "123", "留言", "第一則", "NT$20");
        HashSet<ChatFallbackIdentity> seen = [];
        Assert.True(first.CanDeduplicate);
        Assert.True(seen.Add(first));
        Assert.False(seen.Add(first));
        Assert.True(seen.Add(second));
        Assert.True(seen.Add(amount));
        Assert.False(ChatFallbackIdentity.Create("channel", "作者", "", "留言", "內容").CanDeduplicate);
        Assert.False(ChatFallbackIdentity.Create("channel", "作者", "123", "留言").CanDeduplicate);
    }
    [Fact]
    public void Create_優先使用頻道ID避免同名作者互相去重()
    {
        ChatFallbackIdentity first = ChatFallbackIdentity.Create("channel-a", "同名", "123", "留言");
        ChatFallbackIdentity second = ChatFallbackIdentity.Create("channel-b", "同名", "123", "留言");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Create_沒有頻道ID時使用作者名稱且保留訊息類型()
    {
        ChatFallbackIdentity chat = ChatFallbackIdentity.Create(null, "作者", "123", "留言");
        ChatFallbackIdentity purchase = ChatFallbackIdentity.Create(null, "作者", "123", "超級留言");

        Assert.Equal("作者", chat.AuthorKey);
        Assert.NotEqual(chat, purchase);
    }
}
