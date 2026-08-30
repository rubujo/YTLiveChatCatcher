using Rubujo.YouTube.Utility.Utils;
using Xunit;

namespace Rubujo.YouTube.Utility.Tests;

/// <summary>
/// 驗證 YouTubeUrlUtil.GetYouTubeVideoID 的行為規格：從常見的 YouTube 影片網址形式取出純影片 ID，
/// 結尾若帶有分享／追蹤參數要一併移除；辨識不出來時把原字串整個回傳（既有 fallback 行為）。
/// </summary>
public class YouTubeUrlUtilVideoIdTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("http://www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    // 注意：m.youtube.com（行動版子網域）現行實作不支援，維持原有行為，不在這次範圍內擴充。
    public void GetYouTubeVideoID_watch網址取出影片ID(string url, string expected)
    {
        Assert.Equal(expected, YouTubeUrlUtil.GetYouTubeVideoID(url));
    }

    [Theory]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("http://youtu.be/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    public void GetYouTubeVideoID_短網址取出影片ID(string url, string expected)
    {
        Assert.Equal(expected, YouTubeUrlUtil.GetYouTubeVideoID(url));
    }

    [Theory]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/v/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/e/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    public void GetYouTubeVideoID_內嵌格式網址取出影片ID(string url, string expected)
    {
        Assert.Equal(expected, YouTubeUrlUtil.GetYouTubeVideoID(url));
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PLxxxxxx", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=42s", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&si=abcdefg123", "dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?si=abcdefg123", "dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?t=10", "dQw4w9WgXcQ")]
    public void GetYouTubeVideoID_結尾追蹤參數要一併移除(string url, string expected)
    {
        Assert.Equal(expected, YouTubeUrlUtil.GetYouTubeVideoID(url));
    }

    [Fact]
    public void GetYouTubeVideoID_已經是純ID時原樣回傳()
    {
        Assert.Equal("dQw4w9WgXcQ", YouTubeUrlUtil.GetYouTubeVideoID("dQw4w9WgXcQ"));
    }

    [Fact]
    public void GetYouTubeVideoID_無法辨識時原字串整個回傳()
    {
        string input = "not a youtube url at all";

        Assert.Equal(input, YouTubeUrlUtil.GetYouTubeVideoID(input));
    }

    [Fact]
    public void GetYouTubeVideoID_空字串時回傳空字串()
    {
        Assert.Equal(string.Empty, YouTubeUrlUtil.GetYouTubeVideoID(string.Empty));
    }
}
