namespace YTLiveChatCatcher.Common.Utils;

public static class ListSamplingUtil
{
    /// <summary>保留首尾並從整份資料等距取樣，避免畫面欄寬量測成本隨資料量線性增長。</summary>
    public static List<T> CreateEvenlySpaced<T>(IReadOnlyList<T> items, int maximumCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumCount, 2);

        if (items.Count <= maximumCount)
        {
            return [.. items];
        }

        List<T> sample = new(maximumCount);
        double step = (double)(items.Count - 1) / (maximumCount - 1);

        for (int index = 0; index < maximumCount; index++)
        {
            sample.Add(items[(int)Math.Round(index * step)]);
        }

        return sample;
    }
}
