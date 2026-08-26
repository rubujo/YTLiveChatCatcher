using System.Globalization;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility.Utils;

/// <summary>
/// 語言工具
/// </summary>
public class LangUtil
{
    /// <summary>
    /// 依 CultureInfo 判斷對應的 EnumSet.DisplayLanguage
    /// <para>比對順序：完整文化特性名稱（例如 "zh-TW"）→ 中文特例（依 Parent 鏈判斷屬於 zh-Hans 或 zh-Hant，
    /// 才能正確處理 zh-HK／zh-SG 這類本字典未直接收錄、但屬於同一書寫系統的地區）→
    /// 主要語言代碼（例如 "en"）→ 完全找不到對應語言時退回英文。</para>
    /// </summary>
    /// <param name="cultureInfo">CultureInfo，不指定時使用 CultureInfo.CurrentUICulture</param>
    /// <returns>EnumSet.DisplayLanguage</returns>
    public static EnumSet.DisplayLanguage GetDisplayLanguageFromCulture(
        CultureInfo? cultureInfo = null)
    {
        cultureInfo ??= CultureInfo.CurrentUICulture;

        Dictionary<EnumSet.DisplayLanguage, RegionData> dictRegion =
            DictionarySet.GetRegionDictionary();

        foreach (KeyValuePair<EnumSet.DisplayLanguage, RegionData> pair in dictRegion)
        {
            if (string.Equals(pair.Value.GetCultureInfo().Name, cultureInfo.Name, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Key;
            }
        }

        if (string.Equals(cultureInfo.TwoLetterISOLanguageName, "zh", StringComparison.OrdinalIgnoreCase))
        {
            for (CultureInfo? ancestor = cultureInfo; ancestor != null && ancestor != ancestor.Parent; ancestor = ancestor.Parent)
            {
                if (string.Equals(ancestor.Name, "zh-Hans", StringComparison.OrdinalIgnoreCase))
                {
                    return EnumSet.DisplayLanguage.Chinese_Simplified;
                }

                if (string.Equals(ancestor.Name, "zh-Hant", StringComparison.OrdinalIgnoreCase))
                {
                    return EnumSet.DisplayLanguage.Chinese_Traditional;
                }
            }
        }

        foreach (KeyValuePair<EnumSet.DisplayLanguage, RegionData> pair in dictRegion)
        {
            if (string.Equals(pair.Value.GetCultureInfo().TwoLetterISOLanguageName, cultureInfo.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Key;
            }
        }

        return EnumSet.DisplayLanguage.English;
    }

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