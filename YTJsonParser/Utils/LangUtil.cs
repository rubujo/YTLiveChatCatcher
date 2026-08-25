using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility.Utils;

/// <summary>
/// 語言工具
/// </summary>
public class LangUtil
{
    /// <summary>
    /// 取得本地化字串
    /// <para>目前並非每種 EnumSet.DisplayLanguage 都有對應的本地化字典，找不到對應語言或對應鍵值時，
    /// 會依序退回英文、最後才退回原始鍵值本身——避免直接把內部鍵值常數洩漏到使用者看得到的 UI 文字上。</para>
    /// </summary>
    /// <param name="displayLanguage">EnumSet.DisplayLanguage，顯示語言</param>
    /// <param name="key">字串，鍵值</param>
    /// <returns>字串</returns>
    public static string GetLocalizeString(
        EnumSet.DisplayLanguage displayLanguage,
        string key)
    {
        Dictionary<EnumSet.DisplayLanguage, Dictionary<string, string>> dictLocalize =
            DictionarySet.GetLocalizeDictionary();

        if (dictLocalize.TryGetValue(displayLanguage, out Dictionary<string, string>? dict) &&
            dict.TryGetValue(key, out string? value))
        {
            return value;
        }

        if (displayLanguage != EnumSet.DisplayLanguage.English &&
            dictLocalize.TryGetValue(EnumSet.DisplayLanguage.English, out Dictionary<string, string>? englishDict) &&
            englishDict.TryGetValue(key, out string? englishValue))
        {
            return englishValue;
        }

        return key;
    }
}