using System.Text.Json;
using Rubujo.YouTube.Utility.Extensions;
using Xunit;

namespace Rubujo.YouTube.Utility.Tests;

/// <summary>
/// 驗證 JsonElementExtension 的行為規格：對 JsonElement 做「安全導覽」，
/// 型別不符或找不到目標時一律回傳 null，絕不拋例外。
/// </summary>
public class JsonElementExtensionTests
{
    private static JsonElement Parse(string json) => JsonSerializer.Deserialize<JsonElement>(json);

    [Fact]
    public void Get_字串鍵值_物件內存在該屬性時回傳其值()
    {
        JsonElement element = Parse("""{"foo":"bar"}""");

        JsonElement? result = element.Get("foo");

        Assert.NotNull(result);
        Assert.Equal("bar", result!.Value.GetString());
    }

    [Fact]
    public void Get_字串鍵值_物件內不存在該屬性時回傳null()
    {
        JsonElement element = Parse("""{"foo":"bar"}""");

        JsonElement? result = element.Get("missing");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("\"just a string\"")]
    [InlineData("[1,2,3]")]
    [InlineData("123")]
    [InlineData("true")]
    [InlineData("null")]
    public void Get_字串鍵值_非物件型別一律回傳null不拋例外(string json)
    {
        JsonElement element = Parse(json);

        JsonElement? result = element.Get("anything");

        Assert.Null(result);
    }

    [Fact]
    public void Get_索引值_陣列內索引存在時回傳該元素()
    {
        JsonElement element = Parse("[10,20,30]");

        JsonElement? result = element.Get(1);

        Assert.NotNull(result);
        Assert.Equal(20, result!.Value.GetInt32());
    }

    [Fact]
    public void Get_索引值_超出陣列範圍時回傳null()
    {
        JsonElement element = Parse("[10,20,30]");

        JsonElement? result = element.Get(99);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("""{"foo":"bar"}""")]
    [InlineData("\"just a string\"")]
    [InlineData("123")]
    [InlineData("null")]
    public void Get_索引值_非陣列型別一律回傳null不拋例外(string json)
    {
        JsonElement element = Parse(json);

        JsonElement? result = element.Get(0);

        Assert.Null(result);
    }

    [Fact]
    public void ToArrayEnumerator_陣列型別時回傳可列舉的ArrayEnumerator()
    {
        JsonElement element = Parse("[1,2,3]");

        JsonElement.ArrayEnumerator? enumerator = element.ToArrayEnumerator();

        Assert.NotNull(enumerator);
        Assert.Equal(3, enumerator!.Value.Count());
    }

    [Theory]
    [InlineData("""{"foo":"bar"}""")]
    [InlineData("\"just a string\"")]
    [InlineData("123")]
    [InlineData("null")]
    public void ToArrayEnumerator_非陣列型別時回傳null(string json)
    {
        JsonElement element = Parse(json);

        JsonElement.ArrayEnumerator? enumerator = element.ToArrayEnumerator();

        Assert.Null(enumerator);
    }

    [Fact]
    public void Get_ArrayEnumerator索引值_索引存在時回傳該元素()
    {
        JsonElement element = Parse("[100,200,300]");
        JsonElement.ArrayEnumerator enumerator = element.EnumerateArray();

        JsonElement? result = enumerator.Get(2);

        Assert.NotNull(result);
        Assert.Equal(300, result!.Value.GetInt32());
    }

    [Fact]
    public void Get_ArrayEnumerator索引值_超出範圍時回傳null()
    {
        JsonElement element = Parse("[100,200,300]");
        JsonElement.ArrayEnumerator enumerator = element.EnumerateArray();

        JsonElement? result = enumerator.Get(99);

        Assert.Null(result);
    }

    [Fact]
    public void Get_可串接多層導覽取得巢狀值()
    {
        JsonElement element = Parse("""{"a":{"b":[{"c":"深層值"}]}}""");

        JsonElement? result = element.Get("a")?.Get("b")?.Get(0)?.Get("c");

        Assert.NotNull(result);
        Assert.Equal("深層值", result!.Value.GetString());
    }

    [Fact]
    public void Get_串接導覽中任一層型別不符時整條鏈回傳null()
    {
        JsonElement element = Parse("""{"a":"不是物件"}""");

        JsonElement? result = element.Get("a")?.Get("b");

        Assert.Null(result);
    }
}
