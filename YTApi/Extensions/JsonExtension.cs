using System.Text.Json;

namespace YTApi.Extensions;

/// <summary>
/// JsonElement 的擴充方法
/// <para>來源：https://stackoverflow.com/a/61561343 </para>
/// </summary>
public static partial class JsonExtension
{
    /// <summary>
    /// 取得指定名稱的 JsonElement
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <param name="name">字串，名稱</param>
    /// <returns>JsonElement?</returns>
    public static JsonElement? Get(this JsonElement element, string name) =>
        element.ValueKind != JsonValueKind.Null &&
        element.ValueKind != JsonValueKind.Undefined &&
        element.TryGetProperty(name, out var value)
            ? value : null;

    /// <summary>
    /// 取得指定索引值的 JsonElement
    /// </summary>
    /// <param name="element">JsonElement</param>
    /// <param name="index">數值，索引值</param>
    /// <returns>JsonElement?</returns>
    public static JsonElement? Get(this JsonElement element, int index)
    {
        if (element.ValueKind == JsonValueKind.Null ||
            element.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        var value = element.EnumerateArray().ElementAtOrDefault(index);

        return value.ValueKind != JsonValueKind.Undefined ? value : null;
    }
}