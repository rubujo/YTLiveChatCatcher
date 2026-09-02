using System.Globalization;
using System.Text;
using System.Text.Json;
using Rubujo.YouTube.Utility.Models.LiveChat;

namespace YTLiveChatCatcher.Common.Utils;

public sealed record ChatFilterOptions(
    DateTimeOffset? StartTime = null,
    DateTimeOffset? EndTime = null,
    string? MessageType = null,
    string? Author = null,
    decimal? MinimumAmount = null,
    decimal? MaximumAmount = null);

public sealed record ChatAnalytics(
    int MessageCount,
    IReadOnlyDictionary<string, int> MessagesByMinute,
    IReadOnlyDictionary<string, int> ActiveAuthors,
    IReadOnlyDictionary<string, decimal> AmountsByCurrency,
    IReadOnlyList<RendererData> PaidTimeline);

/// <summary>聊天室資料的無損匯出、篩選與分析。</summary>
public static class ChatDataTools
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static IReadOnlyList<RendererData> Filter(
        IEnumerable<RendererData> messages,
        ChatFilterOptions options)
    {
        return messages.Where(message =>
        {
            DateTimeOffset? timestamp = ParseTimestamp(message.TimestampUsec);

            if (options.StartTime.HasValue && (!timestamp.HasValue || timestamp < options.StartTime))
            {
                return false;
            }

            if (options.EndTime.HasValue && (!timestamp.HasValue || timestamp > options.EndTime))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(options.MessageType) &&
                !string.Equals(message.Type, options.MessageType, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(options.Author) &&
                !(message.AuthorName?.Contains(options.Author, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                return false;
            }

            if (options.MinimumAmount.HasValue || options.MaximumAmount.HasValue)
            {
                if (string.IsNullOrWhiteSpace(message.PurchaseAmountText) ||
                    !ChatStatsCalculator.TryParsePurchaseAmount(message.PurchaseAmountText, out _, out decimal amount))
                {
                    return false;
                }

                if (options.MinimumAmount.HasValue && amount < options.MinimumAmount.Value ||
                    options.MaximumAmount.HasValue && amount > options.MaximumAmount.Value)
                {
                    return false;
                }
            }

            return true;
        }).ToList();
    }

    public static void ExportJsonLines(string path, IEnumerable<RendererData> messages)
    {
        using StreamWriter writer = new(path, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        foreach (RendererData message in messages)
        {
            writer.WriteLine(JsonSerializer.Serialize(message, JsonOptions));
        }
    }

    public static void ExportCsv(string path, IEnumerable<RendererData> messages)
    {
        using StreamWriter writer = new(path, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        writer.WriteLine("id,type,timestampUsec,timestampText,authorName,authorExternalChannelID,authorBadges,messageContent,purchaseAmountText,foregroundColor,backgroundColor,headerBackgroundColor,leaderboardRank,replyCountEntityKey,replyCount,stickersJson,emojisJson,badgesJson");

        foreach (RendererData message in messages)
        {
            string[] values =
            [
                message.ID ?? string.Empty,
                message.Type ?? string.Empty,
                message.TimestampUsec ?? string.Empty,
                message.TimestampText ?? string.Empty,
                message.AuthorName ?? string.Empty,
                message.AuthorExternalChannelID ?? string.Empty,
                message.AuthorBadges ?? string.Empty,
                message.MessageContent ?? string.Empty,
                message.PurchaseAmountText ?? string.Empty,
                message.ForegroundColor ?? string.Empty,
                message.BackgroundColor ?? string.Empty,
                message.HeaderBackgroundColor ?? string.Empty,
                message.LeaderboardRank ?? string.Empty,
                message.ReplyCountEntityKey ?? string.Empty,
                message.ReplyCount ?? string.Empty,
                JsonSerializer.Serialize(message.Stickers, JsonOptions),
                JsonSerializer.Serialize(message.Emojis, JsonOptions),
                JsonSerializer.Serialize(message.Badges, JsonOptions)
            ];

            writer.WriteLine(string.Join(',', values.Select(EscapeCsv)));
        }
    }

    public static ChatAnalytics Analyze(IEnumerable<RendererData> messages)
    {
        List<RendererData> data = messages.ToList();
        Dictionary<string, int> density = [];
        Dictionary<string, int> authors = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, decimal> currencies = new(StringComparer.Ordinal);
        List<RendererData> paid = [];

        foreach (RendererData message in data)
        {
            DateTimeOffset? timestamp = ParseTimestamp(message.TimestampUsec);

            if (timestamp.HasValue)
            {
                string minute = timestamp.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                density[minute] = density.GetValueOrDefault(minute) + 1;
            }

            if (!string.IsNullOrWhiteSpace(message.AuthorName))
            {
                authors[message.AuthorName] = authors.GetValueOrDefault(message.AuthorName) + 1;
            }

            if (!string.IsNullOrWhiteSpace(message.PurchaseAmountText) &&
                ChatStatsCalculator.TryParsePurchaseAmount(message.PurchaseAmountText, out string currency, out decimal amount))
            {
                paid.Add(message);
                currencies[currency] = currencies.GetValueOrDefault(currency) + amount;
            }
        }

        return new ChatAnalytics(
            data.Count,
            density.OrderBy(item => item.Key).ToDictionary(),
            authors.OrderByDescending(item => item.Value).ThenBy(item => item.Key).ToDictionary(),
            currencies,
            paid.OrderBy(item => ParseTimestamp(item.TimestampUsec)).ToList());
    }

    public static DateTimeOffset? ParseTimestamp(string? timestampUsec)
    {
        return long.TryParse(timestampUsec, NumberStyles.Integer, CultureInfo.InvariantCulture, out long microseconds) ?
            DateTimeOffset.FromUnixTimeMilliseconds(microseconds / 1000) :
            null;
    }

    private static string EscapeCsv(string value) =>
        value.IndexOfAny([',', '"', '\r', '\n']) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}
