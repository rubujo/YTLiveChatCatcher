using System.Globalization;
using Rubujo.YouTube.Utility.Sets;
using Rubujo.YouTube.Utility.Utils;
using Xunit;

namespace Rubujo.YouTube.Utility.Tests;

public class LangUtilTests
{
    [Theory]
    [InlineData("zh-TW", EnumSet.DisplayLanguage.Chinese_Traditional)]
    [InlineData("zh-CN", EnumSet.DisplayLanguage.Chinese_Simplified)]
    [InlineData("en-US", EnumSet.DisplayLanguage.English)]
    [InlineData("ja-JP", EnumSet.DisplayLanguage.Japanese)]
    [InlineData("ko-KR", EnumSet.DisplayLanguage.Korean)]
    public void GetDisplayLanguageFromCulture_完整文化特性名稱有直接對應時_回傳對應語言(string cultureName, EnumSet.DisplayLanguage expected)
    {
        EnumSet.DisplayLanguage actual = LangUtil.GetDisplayLanguageFromCulture(new CultureInfo(cultureName));

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("zh-HK")]
    [InlineData("zh-MO")]
    public void GetDisplayLanguageFromCulture_字典未收錄的正體中文地區_依Parent鏈判斷為正體中文(string cultureName)
    {
        EnumSet.DisplayLanguage actual = LangUtil.GetDisplayLanguageFromCulture(new CultureInfo(cultureName));

        Assert.Equal(EnumSet.DisplayLanguage.Chinese_Traditional, actual);
    }

    [Fact]
    public void GetDisplayLanguageFromCulture_字典未收錄的簡體中文地區_依Parent鏈判斷為簡體中文()
    {
        EnumSet.DisplayLanguage actual = LangUtil.GetDisplayLanguageFromCulture(new CultureInfo("zh-SG"));

        Assert.Equal(EnumSet.DisplayLanguage.Chinese_Simplified, actual);
    }

    [Fact]
    public void GetDisplayLanguageFromCulture_完全找不到對應語言時_退回英文()
    {
        // "cy-GB"（威爾斯語）不在 DictionarySet 的字典內，且沒有 zh 特例可套用。
        EnumSet.DisplayLanguage actual = LangUtil.GetDisplayLanguageFromCulture(new CultureInfo("cy-GB"));

        Assert.Equal(EnumSet.DisplayLanguage.English, actual);
    }

    [Fact]
    public void GetDisplayLanguageFromCulture_不指定CultureInfo時_使用CurrentUICulture()
    {
        CultureInfo originalUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("ja-JP");

            EnumSet.DisplayLanguage actual = LangUtil.GetDisplayLanguageFromCulture();

            Assert.Equal(EnumSet.DisplayLanguage.Japanese, actual);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalUICulture;
        }
    }
}
