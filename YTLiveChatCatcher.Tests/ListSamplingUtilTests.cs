using YTLiveChatCatcher.Common.Utils;
using Xunit;

namespace YTLiveChatCatcher.Tests;

public class ListSamplingUtilTests
{
    [Fact]
    public void CreateEvenlySpaced_大量資料限制數量並保留首尾()
    {
        int[] source = Enumerable.Range(0, 10_000).ToArray();

        List<int> result = ListSamplingUtil.CreateEvenlySpaced(source, 512);

        Assert.Equal(512, result.Count);
        Assert.Equal(0, result[0]);
        Assert.Equal(9_999, result[^1]);
        Assert.Equal(result.Count, result.Distinct().Count());
    }

    [Fact]
    public void CreateEvenlySpaced_資料未超限時完整保留()
    {
        List<string> result = ListSamplingUtil.CreateEvenlySpaced(["a", "b", "c"], 4);

        Assert.Equal(["a", "b", "c"], result);
    }
}
