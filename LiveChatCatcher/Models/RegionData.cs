using System.Globalization;
using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models;

/// <summary>
/// 區域資料
/// </summary>
public class RegionData
{
    /// <summary>
    /// Google 的國家參數值
    /// </summary>
    [JsonPropertyName("gl")]
    public string? Gl { get; set; } = "US";

    /// <summary>
    /// Google 的語言參數值
    /// </summary>
    [JsonPropertyName("hl")]
    public string? Hl { get; set; } = "en";

    /// <summary>
    /// 時區
    /// </summary>
    [JsonPropertyName("timeZone")]
    public string? TimeZone { get; set; } = "America/Los_Angeles";

    /// <summary>
    /// 接受語言（連線請求標頭）
    /// </summary>
    [JsonPropertyName("acceptLanguage")]
    public string? AcceptLanguage { get; set; } = "en-US;q=0.9,en-GB;q=0.8,en;q=0.7";

    /// <summary>
    /// 取得 CultureInfo
    /// </summary>
    /// <returns>CultureInfo</returns>
    public CultureInfo GetCultureInfo()
    {
        return new CultureInfo($"{Hl}{(string.IsNullOrEmpty(this?.Gl) ? string.Empty : $"-{this?.Gl}")}");
    }
}