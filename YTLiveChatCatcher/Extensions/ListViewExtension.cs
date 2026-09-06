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
        string imageUrl,
        CancellationToken cancellationToken = default)
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

        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            if (httpClient == null) throw new InvalidOperationException("HttpClient 尚未初始化。");
            byte[]? bytes = await AvatarDiskCache.TryReadAsync(imageUrl);
            cancellationToken.ThrowIfCancellationRequested();
            bool downloaded = bytes == null;
            bytes ??= await httpClient.GetByteArrayAsync(imageUrl, cancellationToken);
            using MemoryStream stream = new(bytes);
            using Image decoded = Image.FromStream(stream);
            if (downloaded) await AvatarDiskCache.WriteAsync(imageUrl, bytes);
            cancellationToken.ThrowIfCancellationRequested();
            // await 期間其他工作可能已加入圖片；容量與取消都要在寫入前再次確認。
            if (!imageCollection.ContainsKey(key) && imageCollection.Count < MaxImageListEntries)
            {
                imageCollection.Add(key, new Bitmap(decoded));
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            errorMessage = $"無法載入頭像：{ex.GetExceptionMessage()}";
            cancellationToken.ThrowIfCancellationRequested();
            if (!imageCollection.ContainsKey(key) && imageCollection.Count < MaxImageListEntries)
            {
                Bitmap placeholder = new(32, 32);
                using (Graphics graphics = Graphics.FromImage(placeholder)) graphics.Clear(Color.White);
                imageCollection.Add(key, placeholder);
            }
        }

        return errorMessage;
    }
}
