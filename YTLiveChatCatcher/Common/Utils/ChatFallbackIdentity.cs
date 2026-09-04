namespace YTLiveChatCatcher.Common.Utils;

/// <summary>供缺少 YouTube 訊息 ID 的資料進行跨批次去重。</summary>
public readonly record struct ChatFallbackIdentity(
    string AuthorKey,
    string TimestampUsec,
    string Type)
{
    public static ChatFallbackIdentity Create(
        string? authorChannelId,
        string authorName,
        string timestampUsec,
        string type) =>
        new(
            !string.IsNullOrEmpty(authorChannelId) ? authorChannelId : authorName,
            timestampUsec,
            type);
}
