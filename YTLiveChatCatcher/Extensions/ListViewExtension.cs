using GetCachable;
using Rubujo.YouTube.Utility.Extensions;

namespace YTLiveChatCatcher.Extensions;

/// <summary>
/// ListView 的擴充方法
/// </summary>
public static class ListViewExtension
{
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

        // 以 key 為鍵值，將 Image 暫存 10 分鐘。
        Image image = await BetterCacheManager.GetCachableData(key, async () =>
        {
            try
            {
                if (httpClient == null)
                {
                    throw new Exception("變數 \"httpClient\" 是 null！");
                }

                byte[] bytes = await httpClient.GetByteArrayAsync(imageUrl);

                using MemoryStream memoryStream = new(bytes);

                return Image.FromStream(memoryStream);
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