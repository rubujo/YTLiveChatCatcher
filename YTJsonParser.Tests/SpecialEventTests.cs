using System.Text.Json;
using Rubujo.YouTube.Utility.Models.LiveChat;
using Rubujo.YouTube.Utility.Sets;
using Xunit;

namespace Rubujo.YouTube.Utility.Tests;

public class SpecialEventTests
{
    [Fact]
    public async Task 真實會員結構與現行刪除action可正確分類且不誤判自由文字()
    {
        string response = JsonSerializer.Serialize(new
        {
            continuationContents = new
            {
                liveChatContinuation = new
                {
                    actions = new object[]
                    {
                        new { addChatItemAction = new { item = new { liveChatMembershipItemRenderer = new
                        {
                            id = "milestone", headerPrimaryText = new { runs = new[]
                            { new { text = "Member for " }, new { text = "28" }, new { text = " months" } } },
                            headerSubtext = new { simpleText = "Sample tier" },
                            message = new { runs = new[] { new { text = "Sample message" } } }
                        } } } },
                        new { addChatItemAction = new { item = new { liveChatMembershipItemRenderer = new
                        {
                            id = "new-member", headerSubtext = new { simpleText = "Welcome to Member for fans" },
                            message = new { runs = new[] { new { text = "Upgraded membership to is only user text" } } }
                        } } } },
                        new { addChatItemAction = new { item = new { liveChatPaidMessageRenderer = new
                        {
                            id = "heart-control", purchaseAmountText = new { simpleText = "NT$75" },
                            message = new { runs = new[] { new { text = "Sample paid message" } } },
                            creatorHeartButton = new { creatorHeartViewModel = new
                            {
                                engagementStateKey = "sample-state", heartedAccessibilityLabel = "Remove heart",
                                unheartedAccessibilityLabel = "Heart"
                            } }
                        } } } },
                        new { markChatItemAsDeletedAction = new { targetItemId = "deleted-message" } },
                        new { markChatItemsByAuthorAsDeletedAction = new { externalChannelId = "restricted-author" } }
                    }
                }
            }
        });

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler()
            .When(HttpMethod.Get, "/live_chat?is_popout=1", ReadFixture("live_popout_active.html"))
            .When(HttpMethod.Post, "/youtubei/v1/live_chat/get_live_chat", response);
        using HttpClient client = new(handler);
        using YTJsonParser parser = new(new YTJsonParserOptions
        {
            HttpClient = client,
            DisplayLanguage = EnumSet.DisplayLanguage.English
        });

        List<RendererData> messages = [];
        await foreach (IReadOnlyList<RendererData> batch in parser.StreamLiveChatDataAsync(
            "TEST_VIDEO_ID", options: new() { ForceIntervalMs = 0 },
            cancellationToken: TestContext.Current.CancellationToken))
        {
            messages.AddRange(batch);
        }

        Assert.Contains(messages, item => item.ID == "milestone" && item.Type == "Member Milestone");
        Assert.Contains(messages, item => item.ID == "new-member" && item.Type == "Join Member");
        Assert.Single(messages, item => item.ID == "heart-control" && item.Type == "Super Chat");
        Assert.Contains(messages, item => item.ID == "deleted-message" && item.Type == "Message Deleted");
        Assert.Contains(messages, item => item.AuthorExternalChannelID == "restricted-author" &&
            item.Type == "Author Messages Removed");
    }

    private static string ReadFixture(string fileName) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName));
}
