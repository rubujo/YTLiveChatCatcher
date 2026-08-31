using YTLiveChatCatcher.Common.Utils;
using Xunit;

namespace YTLiveChatCatcher.Tests;

// 只測試 GetCacheFilePath 這個不依賴真實檔案系統的純邏輯，刻意不測 TryRead／Write／
// PruneExpiredEntries——那幾個方法固定寫死存取 %LocalAppData%\YTLiveChatCatcher\AvatarCache\，
// 是使用者實際執行這個應用程式時真正會用到的同一個目錄，測試如果直接操作它，
// 可能會誤刪或覆寫掉使用者本機真實的頭像快取檔案（跟 CaptureRecoveryStoreTests 的考量一致）。
public class AvatarDiskCacheTests
{
    [Fact]
    public void GetCacheFilePath_同樣的網址每次都算出相同的路徑()
    {
        string url = "https://yt4.ggpht.com/example=s64-c-k-c0x00ffffff-no-rj";

        string path1 = AvatarDiskCache.GetCacheFilePath(url);
        string path2 = AvatarDiskCache.GetCacheFilePath(url);

        Assert.Equal(path1, path2);
    }

    [Fact]
    public void GetCacheFilePath_不同的網址算出不同的路徑()
    {
        string pathA = AvatarDiskCache.GetCacheFilePath("https://yt4.ggpht.com/authorA=s64");
        string pathB = AvatarDiskCache.GetCacheFilePath("https://yt4.ggpht.com/authorB=s64");

        Assert.NotEqual(pathA, pathB);
    }

    [Fact]
    public void GetCacheFilePath_回傳的路徑位於AvatarCache子目錄底下()
    {
        string path = AvatarDiskCache.GetCacheFilePath("https://yt4.ggpht.com/example=s64");

        Assert.Contains(Path.Combine("YTLiveChatCatcher", "AvatarCache"), path);
        Assert.EndsWith(".cache", path);
    }
}
