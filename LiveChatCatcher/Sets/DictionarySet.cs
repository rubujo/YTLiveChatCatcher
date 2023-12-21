using Rubujo.YouTube.Utility.Models;

namespace Rubujo.YouTube.Utility.Sets;

/// <summary>
/// 字典組
/// </summary>
public class DictionarySet
{
    /// <summary>
    /// 字典：區域
    /// </summary>
    private static readonly Dictionary<EnumSet.DisplayLanguage, RegionData> DictRegion = new()
    {
        // TODO: 2023/12/19 待完成字典：區域，需要刪除人造語言以及死語言。

        // 來源資料：
        // http://www.lingoes.net/en/translator/langcode.htm
        // https://support.google.com/business/answer/6270107?hl=zh-Hant
        // https://en.wikipedia.org/wiki/List_of_tz_database_time_zones#DUBLIN
        // https://github.com/lau/tzdata/blob/master/test/tzdata_fixtures/europe_shortened
        // https://en.wikipedia.org/wiki/List_of_tz_database_time_zones#DUBLIN
        // https://github.com/lau/tzdata/blob/master/test/tzdata_fixtures/europe_shortened
        { EnumSet.DisplayLanguage.Afrikaans, new RegionData() { Gl = "ZA", Hl = "af", TimeZone = "Africa/Johannesburg", AcceptLanguage = "af-ZA,af;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Albanian, new RegionData() { Gl = "AL", Hl = "sq", TimeZone = "Europe/Tirane", AcceptLanguage = "sq-AL,sq;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Amharic, new RegionData() { Gl = "ET", Hl = "sm", TimeZone = "Africa/Nairobi", AcceptLanguage = "sm-ET,sm;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Arabic, new RegionData() { Gl = "SA", Hl = "ar", TimeZone = "Asia/Riyadh", AcceptLanguage = "ar-SA,ar;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Azerbaijani, new RegionData() { Gl = "AZ", Hl = "az", TimeZone = "Asia/Baku", AcceptLanguage = "az-AZ,az;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Basque, new RegionData() { Gl = "ES", Hl = "eu", TimeZone = "Africa/Ceuta", AcceptLanguage = "eu-ES,eu;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Belarusian, new RegionData() { Gl = "BY", Hl = "be", TimeZone = "Europe/Minsk", AcceptLanguage = "be-BY,be;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Bengali, new RegionData() { Gl = "BD", Hl = "bn", TimeZone = "Asia/Dhaka", AcceptLanguage = "bn-BD,bn;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Bihari, new RegionData() { Gl = "IN", Hl = "bh", TimeZone = "Asia/Kolkata", AcceptLanguage = "bn-IN,bh;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Bosnian, new RegionData() { Gl = "BA", Hl = "bs", TimeZone = "Europe/Belgrade", AcceptLanguage = "bs-BA,bs;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Bulgarian, new RegionData() { Gl = "BG", Hl = "bg", TimeZone = "Europe/Sofia", AcceptLanguage = "bg-BG,bg;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Catalan, new RegionData() { Gl = "AD", Hl = "ca", TimeZone = "Europe/Andorra", AcceptLanguage = "ca=AD,ca;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Chinese_Simplified, new RegionData() { Gl = "CN", Hl = "zh-CN", TimeZone = "Asia/Shanghai", AcceptLanguage = "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7,zh-HK;q=0.6" } },
        { EnumSet.DisplayLanguage.Chinese_Traditional, new RegionData() { Gl = "TW", Hl = "zh-TW", TimeZone = "Asia/Taipei", AcceptLanguage = "zh-TW,zh;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Croatian, new RegionData() { Gl = "HR", Hl = "hr", TimeZone = "Europe/Belgrade", AcceptLanguage = "hr-HR,hr;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Czech, new RegionData() { Gl = "CZ", Hl = "cs", TimeZone = "Europe/Prague", AcceptLanguage = "cs-CZ,cs;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Danish, new RegionData() { Gl = "DK", Hl = "da", TimeZone = "Europe/Berlin", AcceptLanguage = "da-DK,da;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Dutch, new RegionData() { Gl = "NL", Hl = "nl", TimeZone = "Europe/Brussels", AcceptLanguage = "nl-NL,nl;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.English, new RegionData() { Gl = "US", Hl = "en", TimeZone = "America/Los_Angeles", AcceptLanguage = "en-US;q=0.9,en-GB;q=0.8,en;q=0.7" } },
        { EnumSet.DisplayLanguage.Esperanto, new RegionData() { Gl = "US", Hl = "eo", TimeZone = "America/Los_Angeles", AcceptLanguage = "eo-US,eo;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Estonian, new RegionData() { Gl = "EE", Hl = "et", TimeZone = "Europe/Tallinn", AcceptLanguage = "et-EE,et;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Faroese, new RegionData() { Gl = "FO", Hl = "fo", TimeZone = "Atlantic/Faroe", AcceptLanguage = "fo-FO,fo;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Finnish, new RegionData() { Gl = "FI", Hl = "fi", TimeZone = "Europe/Helsinki", AcceptLanguage = "fi-FI,fi;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.French, new RegionData() { Gl = "FR", Hl = "fr", TimeZone = "Europe/Paris", AcceptLanguage = "fr-FR,fr;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Frisian, new RegionData() { Gl = "NL", Hl = "fy", TimeZone = "Europe/Brussels", AcceptLanguage = "fy-NL,fy;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Galician, new RegionData() { Gl = "ES", Hl = "gl", TimeZone = "Europe/Madrid", AcceptLanguage = "gl-ES,gl;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Georgian, new RegionData() { Gl = "GE", Hl = "ka", TimeZone = "Asia/Tbilisi", AcceptLanguage = "ka-GE,ka;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.German, new RegionData() { Gl = "DE", Hl = "de", TimeZone = "Europe/Berlin", AcceptLanguage = "de-DE,de;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Greek, new RegionData() { Gl = "GR", Hl = "el", TimeZone = "Europe/Athens", AcceptLanguage = "el-GR,el;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Gujarati, new RegionData() { Gl = "IN", Hl = "gu", TimeZone = "Asia/Kolkata", AcceptLanguage = "gu-IN,gu;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Hebrew, new RegionData() { Gl = "IL", Hl = "iw", TimeZone = "Asia/Jerusalem", AcceptLanguage = "iw-IL,iw;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Hindi, new RegionData() { Gl = "IN", Hl = "hi", TimeZone = "Asia/Kolkata", AcceptLanguage = "hi-IN,hi;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Hungarian, new RegionData() { Gl = "HU", Hl = "hu", TimeZone = "Europe/Budapest", AcceptLanguage = "hu-HU,hu;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Icelandic, new RegionData() { Gl = "IS", Hl = "is", TimeZone = "Africa/Abidjan", AcceptLanguage = "is-IS,is;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Indonesian, new RegionData() { Gl = "ID", Hl = "id", TimeZone = "Asia/Jakarta", AcceptLanguage = "id-ID,id;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Interlingua, new RegionData() { Gl = "US", Hl = "ia", TimeZone = "America/Los_Angeles", AcceptLanguage = "ia;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Irish, new RegionData() { Gl = "IE", Hl = "ga", TimeZone = "Europe/Dublin", AcceptLanguage = "ga-IE,ga;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Italian, new RegionData() { Gl = "IT", Hl = "it", TimeZone = "Europe/Rome", AcceptLanguage = "it-IT,it;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Japanese, new RegionData() { Gl = "JP", Hl = "ja", TimeZone = "Asia/Tokyo", AcceptLanguage = "ja-JP,ja;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Javanese, new RegionData() { Gl = "ID", Hl = "jw", TimeZone = "Asia/Jakarta", AcceptLanguage = "jw-ID,jw;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Kannada, new RegionData() { Gl = "IN", Hl = "kn", TimeZone = "Asia/Kolkata", AcceptLanguage = "kn-IN,kn;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Korean, new RegionData() { Gl = "KR", Hl = "ko", TimeZone = "Asia/Seoul", AcceptLanguage = "ko-KR,ko;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Latin, new RegionData() { Gl = "US", Hl = "la", TimeZone = "America/Los_Angeles", AcceptLanguage = "la;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Latvian, new RegionData() { Gl = "", Hl = "lv", TimeZone = "", AcceptLanguage = "lv;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Lithuanian, new RegionData() { Gl = "", Hl = "lt", TimeZone = "", AcceptLanguage = "lt;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Macedonian, new RegionData() { Gl = "", Hl = "mk", TimeZone = "", AcceptLanguage = "mk;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Malay, new RegionData() { Gl = "", Hl = "ms", TimeZone = "", AcceptLanguage = "ms;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Malayam, new RegionData() { Gl = "", Hl = "ml", TimeZone = "", AcceptLanguage = "ml;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Maltese, new RegionData() { Gl = "", Hl = "mt", TimeZone = "", AcceptLanguage = "mt;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Marathi, new RegionData() { Gl = "", Hl = "mr", TimeZone = "", AcceptLanguage = "mr;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Nepali, new RegionData() { Gl = "", Hl = "ne", TimeZone = "", AcceptLanguage = "ne;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Norwegian, new RegionData() { Gl = "", Hl = "no", TimeZone = "", AcceptLanguage = "no;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Norwegian_Nynorsk, new RegionData() { Gl = "", Hl = "nn", TimeZone = "", AcceptLanguage = "nn;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Occitan, new RegionData() { Gl = "", Hl = "oc", TimeZone = "", AcceptLanguage = "oc;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Persian, new RegionData() { Gl = "", Hl = "fa", TimeZone = "", AcceptLanguage = "fa;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Polish, new RegionData() { Gl = "", Hl = "pl", TimeZone = "", AcceptLanguage = "pl;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Portuguese_Brazil, new RegionData() { Gl = "BR", Hl = "pt-BR", TimeZone = "America/Sao_Paulo", AcceptLanguage = "pt-BR,pt;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Portuguese_Portugal, new RegionData() { Gl = "PT", Hl = "pt-PT", TimeZone = "Atlantic/Madeira", AcceptLanguage = "pt-PT,pt;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Punjabi, new RegionData() { Gl = "", Hl = "pa", TimeZone = "", AcceptLanguage = "pa;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Romanian, new RegionData() { Gl = "", Hl = "ro", TimeZone = "", AcceptLanguage = "ro;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Russian, new RegionData() { Gl = "", Hl = "ru", TimeZone = "", AcceptLanguage = "ru;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Scots_Gaelic, new RegionData() { Gl = "", Hl = "gd", TimeZone = "", AcceptLanguage = "gd;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Serbian, new RegionData() { Gl = "", Hl = "sr", TimeZone = "", AcceptLanguage = "sr;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Sinhalese, new RegionData() { Gl = "", Hl = "si", TimeZone = "", AcceptLanguage = "si;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Slovak, new RegionData() { Gl = "", Hl = "sk", TimeZone = "", AcceptLanguage = "sk;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Slovenian, new RegionData() { Gl = "", Hl = "sl", TimeZone = "", AcceptLanguage = "sl;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Spanish, new RegionData() { Gl = "", Hl = "es", TimeZone = "", AcceptLanguage = "es;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Sudanese, new RegionData() { Gl = "", Hl = "su", TimeZone = "", AcceptLanguage = "su;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Swahili, new RegionData() { Gl = "", Hl = "sw", TimeZone = "", AcceptLanguage = "sw;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Swedish, new RegionData() { Gl = "", Hl = "sv", TimeZone = "", AcceptLanguage = "sv;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Tagalog, new RegionData() { Gl = "", Hl = "tl", TimeZone = "", AcceptLanguage = "tl;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Tamil, new RegionData() { Gl = "", Hl = "ta", TimeZone = "", AcceptLanguage = "ta;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Telugu, new RegionData() { Gl = "", Hl = "te", TimeZone = "", AcceptLanguage = "te;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Thai, new RegionData() { Gl = "", Hl = "th", TimeZone = "", AcceptLanguage = "th;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Tigrinya, new RegionData() { Gl = "", Hl = "ti", TimeZone = "", AcceptLanguage = "ti;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Turkish, new RegionData() { Gl = "", Hl = "tr", TimeZone = "", AcceptLanguage = "tr;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Ukrainian, new RegionData() { Gl = "", Hl = "uk", TimeZone = "", AcceptLanguage = "uk;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Urdu, new RegionData() { Gl = "", Hl = "ur", TimeZone = "", AcceptLanguage = "ur;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Uzbek, new RegionData() { Gl = "", Hl = "uz", TimeZone = "", AcceptLanguage = "uz;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Vietnamese, new RegionData() { Gl = "", Hl = "vi", TimeZone = "", AcceptLanguage = "vi;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Welsh, new RegionData() { Gl = "", Hl = "cy", TimeZone = "", AcceptLanguage = "cy;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Xhosa, new RegionData() { Gl = "", Hl = "xh", TimeZone = "", AcceptLanguage = "xh;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } },
        { EnumSet.DisplayLanguage.Zulu, new RegionData() { Gl = "", Hl = "zu", TimeZone = "", AcceptLanguage = "zu;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6" } }
    };

    /// <summary>
    /// 字典：本地化
    /// </summary>
    private static readonly Dictionary<EnumSet.DisplayLanguage, Dictionary<string, string>> DictLocalize = new()
    {
        // 2023/12/20 請自行補充其它語系的本地化字串，
        // 詳細請參考 README.md 內的範例程式碼。。
        {
            EnumSet.DisplayLanguage.Chinese_Traditional,
            new Dictionary<string, string>()
            {
                { KeySet.ChatGeneral, "一般" },
                { KeySet.ChatSuperChat, "超級留言" },
                { KeySet.ChatSuperSticker, "超級貼圖" },
                { KeySet.ChatJoinMember, "加入會員" },
                { KeySet.ChatMemberUpgrade, "會員升級" },
                { KeySet.ChatMemberMilestone, "會員里程碑" },
                { KeySet.ChatMemberGift, "贈送會員" },
                { KeySet.ChatReceivedMemberGift, "接收會員贈送" },
                { KeySet.ChatRedirect, "重新導向" },
                { KeySet.ChatPinned, "置頂留言" },
                { KeySet.MemberUpgrade, "頻道會員等級已升級至" },
                { KeySet.MemberMilestone, "已加入會員" }
            }
        }
    };

    /// <summary>
    /// 取得字典：區域
    /// </summary>
    /// <returns>Dictionary&lt;EnumSet.DisplayLanguage, RegionData&gt;</returns>
    public static Dictionary<EnumSet.DisplayLanguage, RegionData> GetRegionDictionary()
    {
        return DictRegion;
    }

    /// <summary>
    /// 字典：本地化
    /// </summary>
    /// <returns>Dictionary&lt;EnumSet.DisplayLanguage, Dictionary&lt;string, string&gt;&gt;</returns>
    public static Dictionary<EnumSet.DisplayLanguage, Dictionary<string, string>> GetLocalizeDictionary()
    {
        return DictLocalize;
    }
}