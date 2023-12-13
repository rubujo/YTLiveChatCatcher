using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Sets;

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

        return StringSet.NoAuthorBadges;
    }
}