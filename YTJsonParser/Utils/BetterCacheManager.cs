using Microsoft.Extensions.Caching.Memory;

namespace GetCachable;

/// <summary>
/// BetterCacheManager
/// <para>參考：https://blog.darkthread.net/blog/cachable-data-object</para>
/// <para>原作者：黑暗執行緒</para>
/// <para>原授權：CC BY-NC-SA 3.0 TW</para>
/// <para>CC BY-NC-SA 3.0 TW：https://creativecommons.org/licenses/by-nc-sa/3.0/tw/</para>
/// </summary>
public static class BetterCacheManager
{
    private static readonly MemoryCache Cache = new(new MemoryCacheOptions());

    /// <summary>
    /// 取得可以被 Cache 的資料
    /// <para>同一個 Key 同一時間僅會執行一次 callback（透過 Lazy&lt;Task&lt;T&gt;&gt; 達成，執行失敗時不快取結果）</para>
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <param name="key">Cache 保存號碼牌</param>
    /// <param name="callback">傳回查詢資料的非同步函數</param>
    /// <param name="cacheMins">Cache 保持分鐘數</param>
    /// <param name="forceRefresh">是否清除 Cache，重新查詢</param>
    /// <returns>Task&lt;T&gt;</returns>
    public static async Task<T> GetCachableData<T>(
        string key,
        Func<Task<T>> callback,
        int cacheMins,
        bool forceRefresh = false) where T : class
    {
        if (forceRefresh)
        {
            Cache.Remove(key);
        }

        Lazy<Task<T>> lazyTask = Cache.GetOrCreate(key, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(cacheMins);

            return new Lazy<Task<T>>(callback, LazyThreadSafetyMode.ExecutionAndPublication);
        })!;

        try
        {
            return await lazyTask.Value;
        }
        catch
        {
            // 執行失敗時，移除快取，避免永久快取住失敗的結果。
            Cache.Remove(key);

            throw;
        }
    }

    /// <summary>
    /// 取得可以被 Cache 的資料
    /// <para>同一個 Key 同一時間僅會執行一次 callback（透過 Lazy&lt;Task&lt;T&gt;&gt; 達成，執行失敗時不快取結果）</para>
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <param name="key">Cache 保存號碼牌</param>
    /// <param name="callback">傳回查詢資料的非同步函數</param>
    /// <param name="absExpire">有效期限</param>
    /// <param name="forceRefresh">是否清除 Cache，重新查詢</param>
    /// <returns>Task&lt;T&gt;</returns>
    public static async Task<T> GetCachableData<T>(
        string key,
        Func<Task<T>> callback,
        DateTimeOffset absExpire,
        bool forceRefresh = false) where T : class
    {
        if (forceRefresh)
        {
            Cache.Remove(key);
        }

        Lazy<Task<T>> lazyTask = Cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpiration = absExpire;

            return new Lazy<Task<T>>(callback, LazyThreadSafetyMode.ExecutionAndPublication);
        })!;

        try
        {
            return await lazyTask.Value;
        }
        catch
        {
            // 執行失敗時，移除快取，避免永久快取住失敗的結果。
            Cache.Remove(key);

            throw;
        }
    }
}
