using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Rubujo.YouTube.Utility.Utils;
using Xunit;

namespace Rubujo.YouTube.Utility.Tests;

/// <summary>
/// 驗證 YouTubeUrlUtil.GetOgUrlContent（ParseYouTubeChannelID 內部用來找 og:url meta 標籤的邏輯）。
/// </summary>
public class YouTubeUrlUtilTests
{
    private static IDocument ParseHtml(string html)
    {
        IConfiguration configuration = Configuration.Default;
        IBrowsingContext browsingContext = BrowsingContext.New(configuration);
        HtmlParser parser = new(new HtmlParserOptions(), browsingContext);

        return parser.ParseDocument(html);
    }

    [Fact]
    public void GetOgUrlContent_og_url位於head的直接子節點時能正確取得()
    {
        string html = """
            <!DOCTYPE html>
            <html>
            <head>
            <meta property="og:url" content="https://www.youtube.com/channel/UCTEST1" />
            </head>
            <body></body>
            </html>
            """;

        string result = YouTubeUrlUtil.GetOgUrlContent(ParseHtml(html));

        Assert.Equal("https://www.youtube.com/channel/UCTEST1", result);
    }

    [Fact]
    public void GetOgUrlContent_og_url位於body底下時仍能正確取得()
    {
        // 對應實測發現的真實回歸問題：YouTube 對部分頻道（例如 @handle 網址）的頁面，現在會把
        // og:url 這類 SEO meta 標籤放在 <body> 底下而非 <head> 的直接子節點。原本用
        // document.Head.Children.FirstOrDefault(...) 只找 <head> 的直接子節點，遇到這種頁面會找不到，
        // 讓「自動取得頻道 ID」整個功能看起來像壞掉（輸入框內容原封不動）。
        string html = """
            <!DOCTYPE html>
            <html>
            <head>
            <meta charset="utf-8" />
            </head>
            <body>
            <meta property="og:url" content="https://www.youtube.com/channel/UCTEST2" />
            </body>
            </html>
            """;

        string result = YouTubeUrlUtil.GetOgUrlContent(ParseHtml(html));

        Assert.Equal("https://www.youtube.com/channel/UCTEST2", result);
    }

    [Fact]
    public void GetOgUrlContent_找不到og_url時回傳空字串()
    {
        string html = """
            <!DOCTYPE html>
            <html>
            <head></head>
            <body></body>
            </html>
            """;

        string result = YouTubeUrlUtil.GetOgUrlContent(ParseHtml(html));

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetOgUrlContent_document為null時回傳空字串()
    {
        string result = YouTubeUrlUtil.GetOgUrlContent(null);

        Assert.Equal(string.Empty, result);
    }
}
