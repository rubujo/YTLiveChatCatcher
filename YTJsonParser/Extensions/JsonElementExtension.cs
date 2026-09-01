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

    // 2026/9 新增：上面的 Get(...) 只防禦「導覽」（屬性/索引不存在、或容器型別不對時回傳 null），
    // 但呼叫端最終要「取值」時（.GetString()／.GetInt64()／.GetBoolean()／.GetInt32() 等 System.Text.Json
    // 內建方法），型別不符一樣會直接拋例外，等於防禦鏈在最後一步斷開。YouTube 的欄位型別偶爾會漂移
    // （例如原本是數字的顏色值變成字串），這裡補上同一套「型別不符就回傳 null」的安全取值版本，
    // 讓呼叫端可以用 ?? 或 null 合併運算子處理，不會讓一則訊息的欄位型別異常炸穿整批解析。

    /// <summary>
    /// 安全取得字串值：ValueKind 不是 String 時回傳 null，不拋例外。
    /// </summary>
    /// <param name="jsonElement">JsonElement?</param>
    /// <returns>string?</returns>
    public static string? GetStringSafely(this JsonElement? jsonElement)
    {
        return jsonElement?.ValueKind == JsonValueKind.String ? jsonElement.Value.GetString() : null;
    }

    /// <summary>
    /// 安全取得 Int64 值：型別不是有效數字時回傳 null，不拋例外。
    /// </summary>
    /// <param name="jsonElement">JsonElement?</param>
    /// <returns>long?</returns>
    public static long? GetInt64Safely(this JsonElement? jsonElement)
    {
        return jsonElement?.ValueKind == JsonValueKind.Number && jsonElement.Value.TryGetInt64(out long value) ?
            value :
            null;
    }

    /// <summary>
    /// 安全取得 Int32 值：型別不是有效數字時回傳 null，不拋例外。
    /// </summary>
    /// <param name="jsonElement">JsonElement?</param>
    /// <returns>int?</returns>
    public static int? GetInt32Safely(this JsonElement? jsonElement)
    {
        return jsonElement?.ValueKind == JsonValueKind.Number && jsonElement.Value.TryGetInt32(out int value) ?
            value :
            null;
    }

    /// <summary>
    /// 安全取得布林值：ValueKind 不是 True／False 時回傳 null，不拋例外。
    /// </summary>
    /// <param name="jsonElement">JsonElement?</param>
    /// <returns>bool?</returns>
    public static bool? GetBooleanSafely(this JsonElement? jsonElement)
    {
        return jsonElement?.ValueKind is JsonValueKind.True or JsonValueKind.False ?
            jsonElement.Value.GetBoolean() :
            null;
    }
}
