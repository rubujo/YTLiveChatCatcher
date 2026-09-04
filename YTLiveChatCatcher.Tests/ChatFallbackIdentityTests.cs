using YTLiveChatCatcher.Common.Utils;
using Xunit;

namespace YTLiveChatCatcher.Tests;

public class ChatFallbackIdentityTests
{
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
