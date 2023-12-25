using GetCachable;
using Rubujo.YouTube.Utility.Extensions;

namespace YTLiveChatCatcher.Extensions;

/// <summary>
/// ListView 的擴充方法
/// <para>來源：https://stackoverflow.com/a/40205173</para>
/// <para>原作者：Joe Savage</para>
/// <para>原授權：CC BY-SA 3.0</para>
/// <para>CC BY-SA 3.0：https://creativecommons.org/licenses/by-sa/3.0/</para>
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
        return listView.SelectedItems.OfType<ListViewItem>();
    }

    /// <summary>
    ///  取得 ListViewItem
    /// </summary>
    /// <param name="listView">ListView</param>
    /// <returns>IEnumerable&lt;ListViewItem&gt;</returns>
    public static IEnumerable<ListViewItem> GetListViewItems(this ListView listView)
    {
        return listView.Items.OfType<ListViewItem>();
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

        imageCollection.Add(key, image);

        return errorMessage;
    }
}