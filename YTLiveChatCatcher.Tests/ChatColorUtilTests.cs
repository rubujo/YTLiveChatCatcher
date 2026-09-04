using System.Drawing;
using YTLiveChatCatcher.Common.Utils;
using Xunit;

namespace YTLiveChatCatcher.Tests;

public class ChatColorUtilTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("not-a-color")]
    [InlineData("NoForegroundColor")]
    [InlineData("NoBackgroundColor")]
    public void TryParse_沒有可套用的顏色時不拋例外(string? value)
    {
        bool parsed = ChatColorUtil.TryParse(value, out _);

        Assert.False(parsed);
    }

    [Theory]
    [InlineData("#112233", 17, 34, 51)]
    [InlineData("White", 255, 255, 255)]
    public void TryParse_有效HTML顏色(string value, int red, int green, int blue)
    {
        bool parsed = ChatColorUtil.TryParse(value, out Color color);

        Assert.True(parsed);
        Assert.Equal(red, color.R);
        Assert.Equal(green, color.G);
        Assert.Equal(blue, color.B);
    }
}
