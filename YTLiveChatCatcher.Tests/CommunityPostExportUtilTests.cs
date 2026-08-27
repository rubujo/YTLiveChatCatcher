using Rubujo.YouTube.Utility.Models.Community;
using Rubujo.YouTube.Utility.Models.LiveChat;
using YTLiveChatCatcher.Common.Utils;
using Xunit;

namespace YTLiveChatCatcher.Tests;

public class CommunityPostExportUtilTests
{
    [Fact]
    public void FlattenRuns_null時回傳空字串()
    {
        Assert.Equal(string.Empty, CommunityPostExportUtil.FlattenRuns(null));
    }

    [Fact]
    public void FlattenRuns_空清單時回傳空字串()
    {
        Assert.Equal(string.Empty, CommunityPostExportUtil.FlattenRuns([]));
    }

    [Fact]
    public void FlattenRuns_多段文字依序串接不加分隔符()
    {
        List<RunsData> runs =
        [
            new RunsData { Text = "今天" },
            new RunsData { Text = "天氣" },
            new RunsData { Text = "真好" },
        ];

        Assert.Equal("今天天氣真好", CommunityPostExportUtil.FlattenRuns(runs));
    }

    [Fact]
    public void SummarizeAttachmentTypes_null時回傳空字串()
    {
        Assert.Equal(string.Empty, CommunityPostExportUtil.SummarizeAttachmentTypes(null));
    }

    [Fact]
    public void SummarizeAttachmentTypes_空清單時回傳空字串()
    {
        Assert.Equal(string.Empty, CommunityPostExportUtil.SummarizeAttachmentTypes([]));
    }

    [Fact]
    public void SummarizeAttachmentTypes_單張圖片不顯示數量()
    {
        List<AttachmentData> attachments = [new AttachmentData()];

        Assert.Equal("圖片", CommunityPostExportUtil.SummarizeAttachmentTypes(attachments));
    }

    [Fact]
    public void SummarizeAttachmentTypes_多張圖片顯示數量()
    {
        List<AttachmentData> attachments = [new AttachmentData(), new AttachmentData(), new AttachmentData()];

        Assert.Equal("圖片 x3", CommunityPostExportUtil.SummarizeAttachmentTypes(attachments));
    }

    [Fact]
    public void SummarizeAttachmentTypes_影片附件()
    {
        List<AttachmentData> attachments = [new AttachmentData { IsVideo = true }];

        Assert.Equal("影片", CommunityPostExportUtil.SummarizeAttachmentTypes(attachments));
    }

    [Fact]
    public void SummarizeAttachmentTypes_一般投票不算測驗()
    {
        List<AttachmentData> attachments = [new AttachmentData { IsPoll = true }];

        Assert.Equal("投票", CommunityPostExportUtil.SummarizeAttachmentTypes(attachments));
    }

    [Fact]
    public void SummarizeAttachmentTypes_測驗貼文顯示測驗不顯示投票()
    {
        List<AttachmentData> attachments = [new AttachmentData { IsPoll = true, IsQuiz = true }];

        Assert.Equal("測驗", CommunityPostExportUtil.SummarizeAttachmentTypes(attachments));
    }

    [Fact]
    public void SummarizeAttachmentTypes_混合類型依圖片影片投票測驗順序串接()
    {
        List<AttachmentData> attachments =
        [
            new AttachmentData(),
            new AttachmentData(),
            new AttachmentData { IsVideo = true },
            new AttachmentData { IsPoll = true, IsQuiz = true },
        ];

        Assert.Equal("圖片 x2、影片、測驗", CommunityPostExportUtil.SummarizeAttachmentTypes(attachments));
    }
}
