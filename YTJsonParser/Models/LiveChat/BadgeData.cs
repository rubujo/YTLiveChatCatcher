using Microsoft.Maui.Graphics;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Utils;
using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models.LiveChat;

/// <summary>
/// 徽章資料
/// </summary>
public class BadgeData
{
    /// <summary>
    /// 工具提示
    /// </summary>
    [JsonPropertyName("tooltip")]
    public string? Tooltip { get; set; }

    /// <summary>
    /// 標籤
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// 圖示類型
    /// </summary>
    [JsonPropertyName("iconType")]
    public string? IconType { get; set; }

    /// <summary>
    /// 影像檔的網址
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

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
    /// <param name="cancellationToken">CancellationToken，預設值為 default</param>
    /// <returns>Task&lt;string&gt;，回傳的是錯誤訊息字串（成功時為空字串），不是影像資料本身</returns>
    public async Task<string> SetImage(HttpClient? httpClient, bool isFetchLargePicture, CancellationToken cancellationToken = default)
    {
        if (httpClient == null)
        {
            return "[BadgeData.SetImage()] 變數 \"httpClient\" 為 null！";
        }

        if (string.IsNullOrEmpty(Label) || string.IsNullOrEmpty(Url))
        {
            return "[BadgeData.SetImage()] 變數 \"Label\" 或 \"Url\" 為 null 或空白！";
        }

        (IImage image, string errorMessage) = await ImageDataUtil.DownloadOrPlaceholderAsync(
            httpClient: httpClient,
            cacheKey: Label,
            displayIdentifier: Label,
            url: Url,
            isFetchLargePicture: isFetchLargePicture,
            entityDisplayName: "會員徽章",
            cancellationToken: cancellationToken);

        Image = image;
        Format = image.AsStream().GetImageFormat().ToString();

        return errorMessage;
    }
}