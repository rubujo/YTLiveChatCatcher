using System.Text.Json;

namespace Rubujo.YouTube.Utility.Extensions;

/// <summary>
/// JsonElement 的擴充方法：對 JsonElement 做「安全導覽」，型別不符或找不到目標時一律回傳 null，
/// 讓呼叫端可以用 <c>?.</c> 串接多層存取，不需要每一層都自己檢查 ValueKind 或包 try/catch。
/// </summary>
public static class JsonElementExtension
{
    /// <summary>
    /// 取得指定屬性名稱的 JsonElement
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="propertyName">字串，屬性名稱</param>
    /// <returns>JsonElement?</returns>
    public static JsonElement? Get(this JsonElement jsonElement, string propertyName)
    {
        // TryGetProperty 只有在 ValueKind 為 Object 時才合法，其餘型別（String、Array、數值等）呼叫會直接拋例外——
        // YouTube 偶爾會把某個欄位的型別從物件改成字串／陣列，這裡必須明確擋下，不能只擋 Null／Undefined。
        if (jsonElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return jsonElement.TryGetProperty(propertyName, out JsonElement value) ? value : null;
    }

    /// <summary>
    /// 取得指定索引值的 JsonElement
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <param name="index">數值，索引值</param>
    /// <returns>JsonElement?</returns>
    public static JsonElement? Get(this JsonElement jsonElement, int index)
    {
        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        return jsonElement.ToArrayEnumerator()?.Get(index);
    }

    /// <summary>
    /// 將 JsonElement 轉換成 JsonElement.ArrayEnumerator
    /// </summary>
    /// <param name="jsonElement">JsonElement</param>
    /// <returns>JsonElement.ArrayEnumerator?</returns>
    public static JsonElement.ArrayEnumerator? ToArrayEnumerator(this JsonElement jsonElement)
    {
        return jsonElement.ValueKind == JsonValueKind.Array ? jsonElement.EnumerateArray() : null;
    }

    /// <summary>
    /// 取得 JsonElement.ArrayEnumerator 內指定索引的 JsonElement
    /// </summary>
    /// <param name="arrayEnumerator">JsonElement.ArrayEnumerator</param>
    /// <param name="index">數值，索引值</param>
    /// <returns>JsonElement?</returns>
    public static JsonElement? Get(this JsonElement.ArrayEnumerator arrayEnumerator, int index)
    {
        int currentIndex = 0;

        foreach (JsonElement element in arrayEnumerator)
        {
            if (currentIndex == index)
            {
                return element;
            }

            currentIndex++;
        }

        return null;
    }
}
