using System.Text.Json;

namespace YTApi.Extensions;

/// <summary>
/// JsonElement 的擴充方法
/// <para>來源：https://stackoverflow.com/a/61561343 </para>
/// </summary>
public static partial class JsonElementExtension
{
    /// <summary>
    /// 取得指定屬性名稱的 JsonElement
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="propertyName">字串，屬性名稱</param>
    /// <returns>JsonElement?</returns>
    public static JsonElement? Get(this JsonElement jsonElement, string propertyName) =>
        jsonElement.ValueKind != JsonValueKind.Null &&
        jsonElement.ValueKind != JsonValueKind.Undefined &&
        jsonElement.TryGetProperty(propertyName, out JsonElement value)
            ? value : null;

    /// <summary>
    /// 取得指定索引值的 JsonElement
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="index">數值，索引值</param>
    /// <returns>JsonElement?</returns>
    public static JsonElement? Get(this JsonElement jsonElement, int index)
    {
        JsonElement? value;

        if (jsonElement.ValueKind == JsonValueKind.Null ||
            jsonElement.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        try
        {
            value = jsonElement.EnumerateArray().ElementAtOrDefault(index);
        }
        catch (Exception)
        {
            value = null;
        }

        return value?.ValueKind != JsonValueKind.Undefined ? value : null;
    }

    /// <summary>
    /// 將 JsonElement 轉換成 JsonElement.ArrayEnumerator
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="index">數值，索引值</param>
    /// <returns>JsonElement.ArrayEnumerator</returns>
    public static JsonElement.ArrayEnumerator? ToArrayEnumerator(this JsonElement jsonElement)
    {
        JsonElement.ArrayEnumerator? value;

        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        try
        {
            value = jsonElement.EnumerateArray();
        }
        catch (Exception)
        {
            value = null;
        }

        return value;
    }

    /// <summary>
    /// 取得 JsonElement.ArrayEnumerator 內指定索引的 JsonElement
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="index">數值，索引值</param>
    /// <returns>JsonElement.ArrayEnumerator</returns>
    public static JsonElement? Get(this JsonElement.ArrayEnumerator arrayEnumerator, int index)
    {
        JsonElement? value;

        try
        {
            value = arrayEnumerator.ElementAtOrDefault(index);
        }
        catch (Exception)
        {
            value = null;
        }

        return value;
    }
}