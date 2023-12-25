using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility.Utils;

/// <summary>
/// 語言工具
/// </summary>
public class LangUtil
{
    /// <summary>
    /// 取得本地化字串
    /// </summary>
    /// <param name="displayLanguage">EnumSet.DisplayLanguage，顯示語言</param>
    /// <param name="key">字串，鍵值</param>
    /// <returns><字串/returns>
    public static string GetLocalizeString(
        EnumSet.DisplayLanguage displayLanguage,
        string key)
    {
        bool hasValue = DictionarySet.GetLocalizeDictionary()
            .TryGetValue(
                displayLanguage,
                out Dictionary<string, string>? dictLocalize);

        return hasValue ? dictLocalize?.GetValueOrDefault(key) ?? key : key;
    }
}