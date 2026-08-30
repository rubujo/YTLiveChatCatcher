using YTLiveChatCatcher.Common;
using Xunit;

namespace YTLiveChatCatcher.Tests;

/// <summary>
/// 驗證 CustomFunction.RemoveInvalidFilePathCharacters 的行為規格：把檔名裡任何檔案名稱或路徑
/// 無效字元換成指定的替代字元，其餘字元保持不變。
/// </summary>
public class CustomFunctionTests
{
    [Fact]
    public void RemoveInvalidFilePathCharacters_沒有無效字元時原樣回傳()
    {
        string result = CustomFunction.RemoveInvalidFilePathCharacters("正常檔名123", "_");

        Assert.Equal("正常檔名123", result);
    }

    [Fact]
    public void RemoveInvalidFilePathCharacters_冒號換成替代字元()
    {
        string result = CustomFunction.RemoveInvalidFilePathCharacters("2026:08:30", "_");

        Assert.Equal("2026_08_30", result);
    }

    [Theory]
    [InlineData("a<b>c", "a_b_c")]
    [InlineData("a:b*c", "a_b_c")]
    [InlineData("a?b\"c", "a_b_c")]
    [InlineData("a|b\\c", "a_b_c")]
    public void RemoveInvalidFilePathCharacters_多種無效字元都會被換成替代字元(string input, string expected)
    {
        Assert.Equal(expected, CustomFunction.RemoveInvalidFilePathCharacters(input, "_"));
    }

    [Fact]
    public void RemoveInvalidFilePathCharacters_空字串時回傳空字串()
    {
        Assert.Equal(string.Empty, CustomFunction.RemoveInvalidFilePathCharacters(string.Empty, "_"));
    }

    [Fact]
    public void RemoveInvalidFilePathCharacters_全部都是無效字元時全部被換成替代字元()
    {
        string result = CustomFunction.RemoveInvalidFilePathCharacters("<>:\"|", "_");

        Assert.Equal("_____", result);
    }

    [Fact]
    public void RemoveInvalidFilePathCharacters_Unicode字元不受影響()
    {
        string result = CustomFunction.RemoveInvalidFilePathCharacters("測試:檔名", "_");

        Assert.Equal("測試_檔名", result);
    }

    [Fact]
    public void RemoveInvalidFilePathCharacters_替代字元可以是多字元字串()
    {
        string result = CustomFunction.RemoveInvalidFilePathCharacters("a:b", "[X]");

        Assert.Equal("a[X]b", result);
    }
}
