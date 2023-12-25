using System.Runtime.Caching;

namespace GetCachable;

/// <summary>
/// BetterCacheManager
/// <para>來源 1：https://blog.darkthread.net/blog/improved-getcachabledata/</para>
/// <para>來源 2：https://blog.darkthread.net/blog/cachable-data-object</para>
/// <para>原作者：黑暗執行緒</para>
/// <para>原授權：CC BY-NC-SA 3.0 TW</para>
/// <para>CC BY-NC-SA 3.0 TW：https://creativecommons.org/licenses/by-nc-sa/3.0/tw/</para>
/// </summary>
public class BetterCacheManager
{
    // 加入 Lock 機制限定同一 Key 同一時間只有一個 Callback 執行。
    const string AsyncLockPrefix = "$$CacheAsyncLock#";

    /// <summary>
    /// 取得每個 Key 專屬的鎖定對象
    /// </summary>
    /// <param name="key">Cache 保存號碼牌</param>
    /// <returns>object</returns>
    static object GetAsyncLock(string key)
    {
        ObjectCache objectCache = MemoryCache.Default;

        // 取得每個 Key 專屬的鎖定對象（object）。
        string asyncLockKey = AsyncLockPrefix + key;

        lock (objectCache)
        {
            if (objectCache[asyncLockKey] == null)
            {
                objectCache.Add(asyncLockKey,
                    new object(),
                    new CacheItemPolicy()
                    {
                        SlidingExpiration = new TimeSpan(0, 10, 0)
                    });
            }
        }

        return objectCache[asyncLockKey];
    }

    /// <summary>
    /// 取得可以被 Cache 的資料 (注意：非 Thread-Safe)
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <param name="key">Cache 保存號碼牌</param>
    /// <param name="callback">傳回查詢資料的函數</param>
    /// <param name="cacheMins">Cache 保持分鐘數</param>
    /// <param name="forceRefresh">是否清除 Cache，重新查詢</param>
    /// <returns>T</returns>
    public static T GetCachableData<T>(string key, Func<T> callback, int cacheMins, bool forceRefresh = false) where T : class
    {
        ObjectCache objectCache = MemoryCache.Default;

        string cacheKey = key;

        // 取得每個 Key 專屬的鎖定對象。
        lock (GetAsyncLock(key))
        {
            T? res = objectCache[cacheKey] as T;

            // 是否清除 Cache，強制重查。
            if (res != null && forceRefresh)
            {
                objectCache.Remove(cacheKey);

                res = null;
            }

            if (res == null)
            {
                res = callback();

                objectCache.Add(cacheKey, res,
                    new CacheItemPolicy()
                    {
                        SlidingExpiration = new TimeSpan(0, cacheMins, 0)
                    });
            }

            return res;
        }
    }

    /// <summary>
    /// 取得可以被 Cache 的資料 (注意：非 Thread-Safe)
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <param name="key">Cache 保存號碼牌</param>
    /// <param name="callback">傳回查詢資料的函數</param>
    /// <param name="absExpire">有效期限</param>
    /// <param name="forceRefresh">是否清除 Cache，重新查詢</param>
    /// <returns>T</returns>
    public static T GetCachableData<T>(string key, Func<T> callback, DateTimeOffset absExpire, bool forceRefresh = false) where T : class
    {
        ObjectCache objectCache = MemoryCache.Default;

        string cacheKey = key;

        // 取得每個 Key 專屬的鎖定對象。
        lock (GetAsyncLock(key))
        {
            T? res = objectCache[cacheKey] as T;

            // 是否清除 Cache，強制重查。
            if (res != null && forceRefresh)
            {
                objectCache.Remove(cacheKey);

                res = null;
            }

            if (res == null)
            {
                res = callback();

                objectCache.Add(cacheKey, res, new CacheItemPolicy()
                {
                    AbsoluteExpiration = absExpire
                });
            }

            return res;
        }
    }
}