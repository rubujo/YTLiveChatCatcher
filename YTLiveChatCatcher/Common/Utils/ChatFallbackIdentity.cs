namespace YTLiveChatCatcher.Common.Utils;

/// <summary>供缺少 YouTube 訊息 ID 的資料進行跨批次去重。</summary>
public readonly record struct ChatFallbackIdentity(
    string AuthorKey,
    string TimestampUsec,
    string Type,
    string Content,
    string Amount)
{
    public static ChatFallbackIdentity Create(
        string? authorChannelId,
        string authorName,
        string timestampUsec,
        string type,
        string content = "",
        string amount = "") =>
        new(
            !string.IsNullOrEmpty(authorChannelId) ? authorChannelId : authorName,
            timestampUsec,
            type,
            content,
            amount);

    /// <summary>缺少可靠時間或內容時保留資料，不以空值判定兩則訊息相同。</summary>
    public bool CanDeduplicate =>
        !string.IsNullOrWhiteSpace(AuthorKey) &&
        !string.IsNullOrWhiteSpace(Type) &&
        long.TryParse(TimestampUsec, out long timestamp) && timestamp > 0 &&
        (!string.IsNullOrEmpty(Content) || !string.IsNullOrEmpty(Amount));
}
