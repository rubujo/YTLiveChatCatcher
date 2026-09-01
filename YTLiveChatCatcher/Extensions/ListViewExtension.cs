using GetCachable;
using Rubujo.YouTube.Utility.Extensions;
using YTLiveChatCatcher.Common.Utils;

namespace YTLiveChatCatcher.Extensions;

/// <summary>
/// ListView 的擴充方法
/// </summary>
public static class ListViewExtension
{
    /// <summary>
    /// 單一 ImageList 允許累積的頭像圖示上限。
    /// <para>2026/9 新增：只有使用者手動按「清除」才會釋放 SmallImageList.Images（見
    /// FMain.cs 的 BtnClear_Click，單純按「停止」不會清空，這是刻意設計，讓當機復原情境下
    /// 重新載入時舊頭像還在）。超長時間、超熱門直播的不重複留言者數量在極端情況下可能逼近或
    /// 超過 Windows 每個處理程序的 GDI 控制代碼配額（預設 10,000），這裡設一個遠低於配額的
    /// 軟上限，超過後不再新增頭像圖示（不影響訊息本身正常擷取，該作者只是這次沒有專屬頭像
    /// 圖示），避免耗盡 GDI 資源拖累或搞壞其他視窗繪製。使用者可以隨時按「清除」重置這個上限。</para>
    /// </summary>
    private const int MaxImageListEntries = 5000;

    /// <summary>
    /// 取得選擇的 ListViewItem
    /// </summary>
    /// <param name="listView">ListView</param>
    /// <returns>IEnumerable&lt;ListViewItem&gt;</returns>
    public static IEnumerable<ListViewItem> GetSelectedListViewItems(this ListView listView)
    {
        foreach (int index in listView.SelectedIndices)
        {
            yield return listView.Items[index];
        }
    }

    /// <summary>
    /// 設定作者相片
    /// </summary>
    /// <param name="imageCollection">ImageList.ImageCollection</param>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="key"字串，鍵值</param>
    /// <param name="imageUrl">字串，相片檔案的網址</param>
    /// <returns>Task&lt;string&gt;</returns>
    public static async Task<string> SetAuthorPhoto(
        this ImageList.ImageCollection imageCollection,
        HttpClient? httpClient,
        string key,
        string imageUrl)
    {
        string errorMessage = string.Empty;

        // 當 key 已存在於 imageCollection 時，忽略不處理。
        if (imageCollection.ContainsKey(key))
        {
            return string.Empty;
        }

        // 已達軟上限，不再新增頭像圖示（見 MaxImageListEntries 的說明），直接跳過、不觸發下載。
        if (imageCollection.Count >= MaxImageListEntries)
        {
            return string.Empty;
        }

        // 以 key 為鍵值，將 Image 暫存 10 分鐘。
        Image image = await BetterCacheManager.GetCachableData(key, async () =>
        {
            try
            {
                if (httpClient == null)
                {
                    throw new Exception("變數 \"httpClient\" 是 null！");
                }

                // 2026/8 新增：先查落地快取（跨應用程式重啟／跨不同場次直播都能沿用），
                // 沒命中才真的發送網路請求，下載完成後順手寫回落地快取供下次使用。
                // 2026/9 修正：改用非同步版本（TryReadAsync／WriteAsync），見 AvatarDiskCache.cs
                // 的方法註解——避免快取命中時整段磁碟 I/O 在 UI 執行緒上同步跑完。
                byte[]? bytes = await AvatarDiskCache.TryReadAsync(imageUrl);
                bool isFreshlyDownloaded = bytes == null;

                if (bytes == null)
                {
                    bytes = await httpClient.GetByteArrayAsync(imageUrl);
                }

                using MemoryStream memoryStream = new(bytes);
                using Image loadedImage = Image.FromStream(memoryStream);

                // 2026/8 修正：先前在下載完成當下就立刻寫入落地快取，沒有先驗證 bytes 真的是可以
                // 解碼的圖片——如果 CDN 一時回應了 200 但內容不是有效圖片（例如暫時性錯誤頁面），
                // Image.FromStream 這裡會拋例外，但無效的內容早已寫進磁碟快取，之後每次都直接
                // TryRead 命中同一份壞資料，要卡到 30 天過期或手動清除才會重新嘗試下載。
                // 改成等 Image.FromStream 成功解碼、確認是有效圖片之後才寫入快取，
                // 且只在這次是剛下載（不是從快取讀到）時才需要寫，避免對同一份已存在的快取檔案
                // 做無意義的重複寫入。
                if (isFreshlyDownloaded)
                {
                    await AvatarDiskCache.WriteAsync(imageUrl, bytes);
                }

                // 2026/8 修正：Image.FromStream 預設不會把像素資料複製進記憶體，而是延遲讀取來源
                // 串流；原本直接把 loadedImage 回傳出去，上面的 using 會在這個方法返回時把
                // memoryStream Dispose 掉，之後 ImageList 真正要繪製這張圖片（發生在更後面、
                // 非同步完成後的畫面重繪階段）時，讀取的其實是已經釋放的串流——這是 GDI+ 層級的
                // 靜默失敗（不會拋出可攔截的例外），實際症狀是圖片「成功」加入 ImageList、
                // Images.Count 也正確累加，畫面上卻永遠是空白。改成 new Bitmap(loadedImage) 建立
                // 一份不依賴來源串流的獨立複本，才能安全地在 memoryStream 釋放後繼續使用。
                return new Bitmap(loadedImage);
            }
            catch (Exception ex)
            {
                errorMessage = $"發生錯誤：{ex.GetExceptionMessage()}{Environment.NewLine}" +
                    $"無法下載「{key}」的頭像。{Environment.NewLine}" +
                    $"頭像的網址：{imageUrl}";

                // 建立一個 64x64 的白色 Bitmap。
                Bitmap bitmap = new(64, 64);

                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.FromKnownColor(KnownColor.White));
                }

                return bitmap;
            }
        }, 10);

        // 因為多筆訊息可能各自觸發下載（download 期間會讓出執行緒給其他佇列中的呼叫），
        // 開頭的 ContainsKey 檢查與這裡的 Add 之間並非原子操作，
        // 所以寫入前要再檢查一次，避免對同一個 key 重複 Add 而拋出例外。
        if (!imageCollection.ContainsKey(key))
        {
            imageCollection.Add(key, image);
        }

        return errorMessage;
    }
}