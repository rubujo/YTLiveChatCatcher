using Microsoft.Extensions.Caching.Memory;

namespace GetCachable;

/// <summary>
/// BetterCacheManager
/// <para>提供「單飛」（single-flight）語意的非同步快取：同一個 key 在同一時間只會有一次 callback
/// 真正在執行，併發呼叫共用同一個進行中的 Task，避免同一份資料被重複計算／重複下載。</para>
/// </summary>
public static class BetterCacheManager
{
    private static readonly MemoryCache Cache = new(new MemoryCacheOptions());

    /// <summary>
    /// 取得可以被 Cache 的資料（滑動到期）
    /// <para>同一個 Key 同一時間僅會執行一次 callback（透過 Lazy&lt;Task&lt;T&gt;&gt; 達成，執行失敗時不快取結果）</para>
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <param name="key">Cache 保存號碼牌</param>
    /// <param name="callback">傳回查詢資料的非同步函數</param>
    /// <param name="cacheMins">Cache 保持分鐘數（滑動到期：每次命中都會重新計時）</param>
    /// <param name="forceRefresh">是否清除 Cache，重新查詢</param>
    /// <returns>Task&lt;T&gt;</returns>
    public static Task<T> GetCachableData<T>(
        string key,
        Func<Task<T>> callback,
        int cacheMins,
        bool forceRefresh = false) where T : class
    {
        return GetOrCreateAsync(key, callback, forceRefresh, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(cacheMins);
        });
    }

    /// <summary>
    /// 取得可以被 Cache 的資料（絕對到期）
    /// <para>同一個 Key 同一時間僅會執行一次 callback（透過 Lazy&lt;Task&lt;T&gt;&gt; 達成，執行失敗時不快取結果）</para>
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <param name="key">Cache 保存號碼牌</param>
    /// <param name="callback">傳回查詢資料的非同步函數</param>
    /// <param name="absExpire">有效期限（絕對到期：不論命中與否都在這個時間點過期）</param>
    /// <param name="forceRefresh">是否清除 Cache，重新查詢</param>
    /// <returns>Task&lt;T&gt;</returns>
    public static Task<T> GetCachableData<T>(
        string key,
        Func<Task<T>> callback,
        DateTimeOffset absExpire,
        bool forceRefresh = false) where T : class
    {
        return GetOrCreateAsync(key, callback, forceRefresh, entry =>
        {
            entry.AbsoluteExpiration = absExpire;
        });
    }

    /// <summary>
    /// 兩個多載共用的核心邏輯：單飛取值＋失敗不快取。
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <param name="key">Cache 保存號碼牌</param>
    /// <param name="callback">傳回查詢資料的非同步函數</param>
    /// <param name="forceRefresh">是否清除 Cache，重新查詢</param>
    /// <param name="configureEntry">設定快取項目的到期方式（滑動或絕對）</param>
    /// <returns>Task&lt;T&gt;</returns>
    private static async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> callback,
        bool forceRefresh,
        Action<ICacheEntry> configureEntry) where T : class
    {
        if (forceRefresh)
        {
            Cache.Remove(key);
        }

        Lazy<Task<T>> lazyTask = Cache.GetOrCreate(key, entry =>
        {
            configureEntry(entry);

            return new Lazy<Task<T>>(callback, LazyThreadSafetyMode.ExecutionAndPublication);
        })!;

        try
        {
            return await lazyTask.Value;
        }
        catch
        {
            // callback 失敗時把這筆快取移除，避免下次呼叫直接拿到快取住的失敗結果，
            // 導致短暫性錯誤（例如網路瞬斷）要等到快取自然過期才會重新嘗試。
            Cache.Remove(key);

            throw;
        }
    }
}
