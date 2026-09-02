using Rubujo.YouTube.Utility.Models.LiveChat;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>
/// 避免 continuation 重送重疊批次時，把同一事件重複寫入復原檔與畫面。
/// </summary>
public sealed class CaptureMessageDeduplicator
{
    private readonly HashSet<string> _seenKeys = new(StringComparer.Ordinal);

    public IReadOnlyList<RendererData> FilterNew(IReadOnlyList<RendererData> messages)
    {
        List<RendererData> output = [];

        foreach (RendererData message in messages)
        {
            string? key = CreateKey(message);

            if (key == null || _seenKeys.Add(key))
            {
                output.Add(message);
            }
        }

        return output;
    }

    public void Clear() => _seenKeys.Clear();

    private static string? CreateKey(RendererData message)
    {
        if (string.IsNullOrEmpty(message.ID))
        {
            return null;
        }

        return string.Join('\u001f',
            message.ID,
            message.Type,
            message.TimestampUsec,
            message.AuthorExternalChannelID,
            message.MessageContent,
            message.PurchaseAmountText,
            message.ReplyCount);
    }
}
