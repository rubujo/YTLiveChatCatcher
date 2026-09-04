using System.Diagnostics;
using System.Windows.Forms;
using YTLiveChatCatcher.Common.Utils;
using Xunit;

namespace YTLiveChatCatcher.Tests;

public class ChatSearchUtilTests
{
    [Fact]
    public void Filter_搜尋三個欄位並保留原始參照與反向順序()
    {
        ListViewItem author = CreateItem("Alice", "內容", "一般留言");
        ListViewItem message = CreateItem("Bob", "HELLO world", "一般留言");
        ListViewItem type = CreateItem("Carol", "內容", "Hello 類型");
        ListViewItem unrelated = CreateItem("Dave", "內容", "一般留言");

        List<ListViewItem> result = ChatSearchUtil.Filter(
            [author, message, type, unrelated],
            "hello");

        Assert.Equal(2, result.Count);
        Assert.Same(type, result[0]);
        Assert.Same(message, result[1]);
    }

    [Fact]
    public void Filter_五萬筆資料維持線性處理且不複製項目()
    {
        List<ListViewItem> source = Enumerable.Range(0, 50_000)
            .Select(index => CreateItem($"作者 {index}", $"目標 {index}", "一般留言"))
            .ToList();
        Stopwatch stopwatch = Stopwatch.StartNew();

        List<ListViewItem> result = ChatSearchUtil.Filter(source, "目標");

        stopwatch.Stop();
        Assert.Equal(source.Count, result.Count);
        Assert.Same(source[^1], result[0]);
        Assert.Same(source[0], result[^1]);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(10), $"搜尋耗時 {stopwatch.Elapsed}");
    }

    private static ListViewItem CreateItem(string author, string message, string type)
    {
        ListViewItem item = new(author);
        item.SubItems.AddRange(["徽章", message, "金額", "時間", type]);
        return item;
    }
}
