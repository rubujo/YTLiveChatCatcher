namespace YTLiveChatCatcher.Common.Utils;

public static class ChatSearchUtil
{
    public readonly record struct SearchText(string Author, string Message, string Type);

    /// <summary>在 UI 執行緒讀取項目，背景工作僅使用不可變文字。</summary>
    public static SearchText[] Snapshot(IReadOnlyList<ListViewItem> source) =>
        source.Select(item => new SearchText(item.Text, item.SubItems[2].Text, item.SubItems[5].Text)).ToArray();

    public static List<int> FilterIndices(IReadOnlyList<SearchText> source, string keyword,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyword);
        List<int> result = [];
        for (int index = source.Count - 1; index >= 0; index--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SearchText text = source[index];
            if (Matches(text.Author, keyword) || Matches(text.Message, keyword) || Matches(text.Type, keyword))
                result.Add(index);
        }
        return result;
    }

    /// <summary>同步搜尋入口；呼叫端須確保來源未被其他執行緒修改。</summary>
    public static List<ListViewItem> Filter(IReadOnlyList<ListViewItem> source, string keyword) =>
        FilterIndices(Snapshot(source), keyword).Select(index => source[index]).ToList();

    private static bool Matches(string value, string keyword) =>
        value.Contains(keyword, StringComparison.CurrentCultureIgnoreCase);
}
