using GetCachable;
using Xunit;

namespace Rubujo.YouTube.Utility.Tests;

/// <summary>
/// 驗證 BetterCacheManager 的行為規格：同一個 key 併發呼叫只執行一次 callback（單飛）、
/// 不同 key 互不影響、forceRefresh 會重新執行、callback 失敗不快取失敗結果、
/// 滑動到期／絕對到期兩種多載都要涵蓋。
/// </summary>
public class BetterCacheManagerTests
{
    [Fact]
    public async Task GetCachableData_同一個key併發呼叫只執行一次callback()
    {
        string key = $"key-{Guid.NewGuid()}";
        int callCount = 0;

        Task<string> Callback()
        {
            Interlocked.Increment(ref callCount);

            return Task.FromResult("value");
        }

        Task<string>[] tasks =
        [
            BetterCacheManager.GetCachableData(key, Callback, 10),
            BetterCacheManager.GetCachableData(key, Callback, 10),
            BetterCacheManager.GetCachableData(key, Callback, 10),
        ];

        string[] results = await Task.WhenAll(tasks);

        Assert.Equal(1, callCount);
        Assert.All(results, n => Assert.Equal("value", n));
    }

    [Fact]
    public async Task GetCachableData_不同key互不影響各自執行callback()
    {
        string key1 = $"key-{Guid.NewGuid()}";
        string key2 = $"key-{Guid.NewGuid()}";

        string result1 = await BetterCacheManager.GetCachableData(key1, () => Task.FromResult("value1"), 10);
        string result2 = await BetterCacheManager.GetCachableData(key2, () => Task.FromResult("value2"), 10);

        Assert.Equal("value1", result1);
        Assert.Equal("value2", result2);
    }

    [Fact]
    public async Task GetCachableData_第二次呼叫命中快取不會重新執行callback()
    {
        string key = $"key-{Guid.NewGuid()}";
        int callCount = 0;

        Task<string> Callback()
        {
            Interlocked.Increment(ref callCount);

            return Task.FromResult("value");
        }

        await BetterCacheManager.GetCachableData(key, Callback, 10);
        await BetterCacheManager.GetCachableData(key, Callback, 10);

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetCachableData_forceRefresh為true時會重新執行callback()
    {
        string key = $"key-{Guid.NewGuid()}";
        int callCount = 0;

        Task<string> Callback()
        {
            Interlocked.Increment(ref callCount);

            return Task.FromResult("value");
        }

        await BetterCacheManager.GetCachableData(key, Callback, 10);
        await BetterCacheManager.GetCachableData(key, Callback, 10, forceRefresh: true);

        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task GetCachableData_callback失敗時不快取失敗結果_下次呼叫會重試()
    {
        string key = $"key-{Guid.NewGuid()}";
        int callCount = 0;

        Task<string> Callback()
        {
            int currentCall = Interlocked.Increment(ref callCount);

            if (currentCall == 1)
            {
                throw new InvalidOperationException("模擬第一次呼叫失敗。");
            }

            return Task.FromResult("value");
        }

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => BetterCacheManager.GetCachableData(key, Callback, 10));

        string result = await BetterCacheManager.GetCachableData(key, Callback, 10);

        Assert.Equal("value", result);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task GetCachableData_絕對到期多載能正確回傳結果並套用單飛()
    {
        string key = $"key-{Guid.NewGuid()}";
        int callCount = 0;
        DateTimeOffset absExpire = DateTimeOffset.UtcNow.AddMinutes(10);

        Task<string> Callback()
        {
            Interlocked.Increment(ref callCount);

            return Task.FromResult("value");
        }

        string result1 = await BetterCacheManager.GetCachableData(key, Callback, absExpire);
        string result2 = await BetterCacheManager.GetCachableData(key, Callback, absExpire);

        Assert.Equal("value", result1);
        Assert.Equal("value", result2);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetCachableData_絕對到期多載的forceRefresh也會重新執行callback()
    {
        string key = $"key-{Guid.NewGuid()}";
        int callCount = 0;
        DateTimeOffset absExpire = DateTimeOffset.UtcNow.AddMinutes(10);

        Task<string> Callback()
        {
            Interlocked.Increment(ref callCount);

            return Task.FromResult("value");
        }

        await BetterCacheManager.GetCachableData(key, Callback, absExpire);
        await BetterCacheManager.GetCachableData(key, Callback, absExpire, forceRefresh: true);

        Assert.Equal(2, callCount);
    }
}
