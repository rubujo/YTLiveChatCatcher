namespace YTLiveChatCatcher.Common.Utils;

public static class ChatSearchUtil
{
    /// <summary>依作者、訊息及類型篩選，並以最新資料優先回傳原始項目參照。</summary>
    public static List<ListViewItem> Filter(IReadOnlyList<ListViewItem> source, string keyword)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyword);

        List<ListViewItem> result = [];

        for (int index = source.Count - 1; index >= 0; index--)
        {
            ListViewItem item = source[index];

            if (Matches(item.SubItems[0].Text, keyword) ||
                Matches(item.SubItems[2].Text, keyword) ||
                Matches(item.SubItems[5].Text, keyword))
            {
                result.Add(item);
            }
        }

        return result;
    }

    private static bool Matches(string value, string keyword) =>
        value.Contains(keyword, StringComparison.CurrentCultureIgnoreCase);
}
