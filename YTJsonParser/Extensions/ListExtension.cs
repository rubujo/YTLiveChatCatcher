using Rubujo.YouTube.Utility.Models.LiveChat;
using Rubujo.YouTube.Utility.Sets;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Rubujo.YouTube.Utility.Extensions;

/// <summary>
/// List 的擴充方法
/// </summary>
public static class ListExtension
{
    /// <summary>
    /// 取得徽章名稱
    /// </summary>
    /// <param name="list">List&lt;BadgeData&gt;</param>
    /// <returns>字串</returns>
    public static string GetBadgeName(this List<BadgeData> list)
    {
        string?[] array = list.Select(n => n.Label).ToArray();

        if (array != null && array.Length > 0)
        {
            return string.Join("、", array);
        }

        return KeySet.NoAuthorBadges;
    }

    /// <summary>
    /// 轉換成 JSON 字串
    /// </summary>
    /// <param name="list">List&lt;T&gt;</param>
    /// <returns>字串</returns>
    public static string ToJsonString<T>(this List<T> list)
    {
        return JsonSerializer.Serialize(list, GetJsonSerializerOptions());
    }

    /// <summary>
    /// 取得 JsonSerializerOptions
    /// </summary>
    /// <returns>JsonSerializerOptions</returns>
    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        TextEncoderSettings textEncoderSettings = new();

        textEncoderSettings.AllowRanges(UnicodeRanges.All);

        return new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.Create(textEncoderSettings)
        };
    }
}