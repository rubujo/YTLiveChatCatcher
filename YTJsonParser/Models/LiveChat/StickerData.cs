using Microsoft.Maui.Graphics;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Utils;
using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models.LiveChat;

/// <summary>
/// Sticker 資料
/// </summary>
public class StickerData
{
    /// <summary>
    /// Sticker 的 ID 值
    /// </summary>
    [JsonPropertyName("id")]
    public string? ID { get; set; }

    /// <summary>
    /// 影像檔的網址
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 文字
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 標籤
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// 影像檔的格式
    /// </summary>
    [JsonIgnore]
    public string? Format { get; set; }

    /// <summary>
    /// 影像
    /// </summary>
    [JsonIgnore]
    public IImage? Image { get; set; }

    /// <summary>
    /// 設定影像
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="isFetchLargePicture">布林值，是否獲取大張圖片</param>
    /// <returns>Task&lt;string&gt;</returns>
    public async Task<string> SetImage(HttpClient? httpClient, bool isFetchLargePicture)
    {
        if (httpClient == null)
        {
            return "[StickerData.SetImage()] 變數 \"httpClient\" 為 null！";
        }

        if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(Url))
        {
            return "[StickerData.SetImage()] 變數 \"ID\" 或 \"Url\" 為 null 或空白！";
        }

        (IImage image, string errorMessage) = await ImageDataUtil.DownloadOrPlaceholderAsync(
            httpClient: httpClient,
            cacheKey: ID,
            displayIdentifier: Label,
            url: Url,
            isFetchLargePicture: isFetchLargePicture,
            entityDisplayName: "超級貼圖");

        Image = image;
        Format = image.AsStream().GetImageFormat().ToString();

        return errorMessage;
    }
}