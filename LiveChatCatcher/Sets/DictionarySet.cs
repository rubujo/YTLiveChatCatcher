using Rubujo.YouTube.Utility.Models;

namespace Rubujo.YouTube.Utility.Sets;

/// <summary>
/// 字典組
/// </summary>
public class DictionarySet
{
    /// <summary>
    /// 字典：區域
    /// <para>來源：https://support.gooGle.com/business/answer/6270107?Hl=zh-Hant</para>
    /// <para>tz database 來源：https://en.wikipedia.org/wiki/List_of_tz_database_time_zones</para>
    /// </summary>
    private static readonly Dictionary<EnumSet.DisplayLanguage, RegionData> DictRegion = new()
    {
        {
            EnumSet.DisplayLanguage.Afrikaans,
            new RegionData()
            {
                Gl = "ZA",
                Hl = "af",
                TimeZone = "Africa/Johannesburg",
                AcceptLanguage = "af-ZA,af;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Albanian,
            new RegionData()
            {
                Gl = "AL",
                Hl = "sq",
                TimeZone = "Europe/Tirane",
                AcceptLanguage = "sq-AL,sq;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Arabic,
            new RegionData()
            {
                Gl = "SA",
                Hl = "ar",
                TimeZone = "Asia/Riyadh",
                AcceptLanguage = "ar-SA,ar;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Azerbaijani,
            new RegionData()
            {
                Gl = "AZ",
                Hl = "az",
                TimeZone = "Asia/Baku",
                AcceptLanguage = "az-AZ,az;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Basque,
            new RegionData()
            {
                Gl = "ES",
                Hl = "eu",
                TimeZone = "Africa/Ceuta",
                AcceptLanguage = "eu-ES,eu;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Belarusian,
            new RegionData()
            {
                Gl = "BY",
                Hl = "be",
                TimeZone = "Europe/Minsk",
                AcceptLanguage = "be-BY,be;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Bengali,
            new RegionData()
            {
                Gl = "BD",
                Hl = "bn",
                TimeZone = "Asia/Dhaka",
                AcceptLanguage = "bn-BD,bn;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Bosnian,
            new RegionData()
            {
                Gl = "BA",
                Hl = "bs",
                TimeZone = "Europe/Belgrade",
                AcceptLanguage = "bs-BA,bs;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Bulgarian,
            new RegionData() {
                Gl = "BG",
                Hl = "bg",
                TimeZone = "Europe/Sofia",
                AcceptLanguage = "bg-BG,bg;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Catalan,
            new RegionData()
            {
                Gl = "AD",
                Hl = "ca",
                TimeZone = "Europe/Andorra",
                AcceptLanguage = "ca=AD,ca;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Chinese_Simplified,
            new RegionData()
            {
                Gl = "CN",
                Hl = "zh-CN",
                TimeZone = "Asia/Shanghai",
                AcceptLanguage = "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7,zh-HK;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Chinese_Traditional,
            new RegionData()
            {
                Gl = "TW",
                Hl = "zh-TW",
                TimeZone = "Asia/Taipei",
                AcceptLanguage = "zh-TW,zh;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Croatian,
            new RegionData()
            {
                Gl = "HR",
                Hl = "hr",
                TimeZone = "Europe/Belgrade",
                AcceptLanguage = "hr-HR,hr;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Czech,
            new RegionData()
            {
                Gl = "CZ",
                Hl = "cs",
                TimeZone = "Europe/Prague",
                AcceptLanguage = "cs-CZ,cs;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Danish,
            new RegionData()
            {
                Gl = "DK",
                Hl = "da",
                TimeZone = "Europe/Berlin",
                AcceptLanguage = "da-DK,da;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Dutch,
            new RegionData()
            {
                Gl = "NL",
                Hl = "nl",
                TimeZone = "Europe/Brussels",
                AcceptLanguage = "nl-NL,nl;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.English,
            new RegionData()
            {
                Gl = "US",
                Hl = "en",
                TimeZone = "America/Los_Angeles",
                AcceptLanguage = "en-US;q=0.9,en-GB;q=0.8,en;q=0.7"
            }
        },
        {
            EnumSet.DisplayLanguage.Estonian,
            new RegionData()
            {
                Gl = "EE",
                Hl = "et",
                TimeZone = "Europe/Tallinn",
                AcceptLanguage = "et-EE,et;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Finnish,
            new RegionData()
            {
                Gl = "FI",
                Hl = "fi",
                TimeZone = "Europe/Helsinki",
                AcceptLanguage = "fi-FI,fi;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.French,
            new RegionData()
            {
                Gl = "FR",
                Hl = "fr",
                TimeZone = "Europe/Paris",
                AcceptLanguage = "fr-FR,fr;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Georgian,
            new RegionData()
            {
                Gl = "GE",
                Hl = "ka",
                TimeZone = "Asia/Tbilisi",
                AcceptLanguage = "ka-GE,ka;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.German,
            new RegionData()
            {
                Gl = "DE",
                Hl = "de",
                TimeZone = "Europe/Berlin",
                AcceptLanguage = "de-DE,de;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Greek,
            new RegionData()
            {
                Gl = "GR",
                Hl = "el",
                TimeZone = "Europe/Athens",
                AcceptLanguage = "el-GR,el;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Gujarati,
            new RegionData()
            {
                Gl = "IN",
                Hl = "gu",
                TimeZone = "Asia/Kolkata",
                AcceptLanguage = "gu-IN,gu;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Hebrew,
            new RegionData()
            {
                Gl = "IL",
                Hl = "iw",
                TimeZone = "Asia/Jerusalem",
                AcceptLanguage = "iw-IL,iw;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Hindi,
            new RegionData()
            {
                Gl = "IN",
                Hl = "hi",
                TimeZone = "Asia/Kolkata",
                AcceptLanguage = "hi-IN,hi;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Hungarian,
            new RegionData()
            {
                Gl = "HU",
                Hl = "hu",
                TimeZone = "Europe/Budapest",
                AcceptLanguage = "hu-HU,hu;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Icelandic,
            new RegionData()
            {
                Gl = "IS",
                Hl = "is",
                TimeZone = "Africa/Abidjan",
                AcceptLanguage = "is-IS,is;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Indonesian,
            new RegionData()
            {
                Gl = "ID",
                Hl = "id",
                TimeZone = "Asia/Jakarta",
                AcceptLanguage = "id-ID,id;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Italian,
            new RegionData()
            {
                Gl = "IT",
                Hl = "it",
                TimeZone = "Europe/Rome",
                AcceptLanguage = "it-IT,it;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Japanese,
            new RegionData()
            {
                Gl = "JP",
                Hl = "ja",
                TimeZone = "Asia/Tokyo",
                AcceptLanguage = "ja-JP,ja;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Kannada,
            new RegionData()
            {
                Gl = "IN",
                Hl = "kn",
                TimeZone = "Asia/Kolkata",
                AcceptLanguage = "kn-IN,kn;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Korean,
            new RegionData()
            {
                Gl = "KR",
                Hl = "ko",
                TimeZone = "Asia/Seoul",
                AcceptLanguage = "ko-KR,ko;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Latvian,
            new RegionData()
            {
                Gl = "LV",
                Hl = "lv",
                TimeZone = "Europe/Riga",
                AcceptLanguage = "lv-LV,lv;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Lithuanian,
            new RegionData()
            {
                Gl = "LT",
                Hl = "lt",
                TimeZone = "Europe/Vilnius",
                AcceptLanguage = "lt-LT,lt;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Macedonian,
            new RegionData()
            {
                Gl = "MK",
                Hl = "mk",
                TimeZone = "Europe/Belgrade",
                AcceptLanguage = "mk-MK,mk;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Malay,
            new RegionData()
            {
                Gl = "MY",
                Hl = "ms",
                TimeZone = "Asia/Singapore",
                AcceptLanguage = "ms-MY,ms;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Malayam,
            new RegionData()
            {
                Gl = "IN",
                Hl = "ml",
                TimeZone = "Asia/Kolkata",
                AcceptLanguage = "ml-IN,ml;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Marathi,
            new RegionData()
            {
                Gl = "IN",
                Hl = "mr",
                TimeZone = "Asia/Kolkata",
                AcceptLanguage = "mr-IN,mr;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Nepali,
            new RegionData()
            {
                Gl = "NP",
                Hl = "ne",
                TimeZone = "Asia/Kathmandu",
                AcceptLanguage = "ne-NP,ne;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Norwegian,
            new RegionData()
            {
                Gl = "NO",
                Hl = "no",
                TimeZone = "Europe/Berlin",
                AcceptLanguage = "no-NO,no;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Persian,
            new RegionData()
            {
                Gl = "IR",
                Hl = "fa",
                TimeZone = "Asia/Tehran",
                AcceptLanguage = "fa-IR,fa;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Polish,
            new RegionData()
            {
                Gl = "PL",
                Hl = "pl",
                TimeZone = "Europe/Warsaw",
                AcceptLanguage = "pl-/pl,pl;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Portuguese_Brazil,
            new RegionData()
            {
                Gl = "BR",
                Hl = "pt-BR",
                TimeZone = "America/Sao_Paulo",
                AcceptLanguage = "pt-BR,pt;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Portuguese_Portugal,
            new RegionData()
            {
                Gl = "PT",
                Hl = "pt-PT",
                TimeZone = "Atlantic/Madeira",
                AcceptLanguage = "pt-PT,pt;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Punjabi,
            new RegionData()
            {
                Gl = "IN",
                Hl = "pa",
                TimeZone = "Asia/Kolkata",
                AcceptLanguage = "pa-IN,pa;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Romanian,
            new RegionData()
            {
                Gl = "RO",
                Hl = "ro",
                TimeZone = "Europe/Bucharest",
                AcceptLanguage = "ro-RO,ro;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Russian,
            new RegionData()
            {
                Gl = "RU",
                Hl = "ru",
                TimeZone = "Europe/Moscow",
                AcceptLanguage = "ru-RU,ru;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Serbian,
            new RegionData()
            {
                Gl = "RS",
                Hl = "sr",
                TimeZone = "Europe/Belgrade",
                AcceptLanguage = "sr-RS,sr;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Sinhalese,
            new RegionData()
            {
                Gl = "LK",
                Hl = "si",
                TimeZone = "Asia/Colombo",
                AcceptLanguage = "si-LK,si;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Slovak,
            new RegionData()
            {
                Gl = "SK",
                Hl = "sk",
                TimeZone = "Europe/Prague",
                AcceptLanguage = "sk-SK,sk;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Slovenian,
            new RegionData()
            {
                Gl = "SI",
                Hl = "sl",
                TimeZone = "Europe/Belgrade",
                AcceptLanguage = "sl-SI,sl;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Spanish,
            new RegionData()
            {
                Gl = "ES",
                Hl = "es",
                TimeZone = "Europe/Madrid",
                AcceptLanguage = "es-ES,es;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Swahili,
            new RegionData()
            {
                Gl = "KE",
                Hl = "sw",
                TimeZone = "Africa/Nairobi",
                AcceptLanguage = "sw-KE,sw;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Swedish,
            new RegionData()
            {
                Gl = "SE",
                Hl = "sv",
                TimeZone = "Europe/Berlin",
                AcceptLanguage = "sv-SE.sv;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Tagalog,
            new RegionData()
            {
                Gl = "PH",
                Hl = "tl",
                TimeZone = "Asia/Manila",
                AcceptLanguage = "tl-PH,tl;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Tamil,
            new RegionData()
            {
                Gl = "IN",
                Hl = "ta",
                TimeZone = "Asia/Kolkata",
                AcceptLanguage = "ta-IN,ta;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Telugu,
            new RegionData()
            {
                Gl = "IN",
                Hl = "te",
                TimeZone = "Asia/Kolkata",
                AcceptLanguage = "te-IN,te;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Thai,
            new RegionData()
            {
                Gl = "TH",
                Hl = "th",
                TimeZone = "Asia/Bangkok",
                AcceptLanguage = "th-TH,th;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Turkish,
            new RegionData()
            {
                Gl = "TR",
                Hl = "tr",
                TimeZone = "Europe/Istanbul",
                AcceptLanguage = "tr-TR,tr;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Ukrainian,
            new RegionData()
            {
                Gl = "UA",
                Hl = "uk",
                TimeZone = "Europe/Kyiv",
                AcceptLanguage = "uk-UA,uk;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Urdu,
            new RegionData()
            {
                Gl = "PK",
                Hl = "ur",
                TimeZone = "Asia/Karachi",
                AcceptLanguage = "ur-PK,ur;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Uzbek,
            new RegionData()
            {
                Gl = "UZ",
                Hl = "uz",
                TimeZone = "Asia/Samarkand",
                AcceptLanguage = "uz-UZ,uz;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Vietnamese,
            new RegionData()
            {
                Gl = "VN",
                Hl = "vi",
                TimeZone = "Asia/Ho_Chi_Minh",
                AcceptLanguage = "vi-VN,vi;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        },
        {
            EnumSet.DisplayLanguage.Zulu,
            new RegionData()
            {
                Gl = "ZA",
                Hl = "zu",
                TimeZone = "Africa/Johannesburg",
                AcceptLanguage = "zu-ZA,zu;q=0.9,en-US;q=0.8,en-GB;q=0.7,en;q=0.6"
            }
        }
    };

    /// <summary>
    /// 字典：本地化
    /// </summary>
    private static readonly Dictionary<EnumSet.DisplayLanguage, Dictionary<string, string>> DictLocalize = new()
    {
        // 2023/12/20 請自行補充其它語系的本地化字串，
        // 詳細請參考 README.md 內的範例程式碼。
        {
            EnumSet.DisplayLanguage.English,
            new Dictionary<string, string>()
            {
                { KeySet.ChatGeneral, "General" },
                { KeySet.ChatSuperChat, "Super Chat" },
                { KeySet.ChatSuperSticker, "Super Sticker" },
                { KeySet.ChatJoinMember, "Join Member" },
                { KeySet.ChatMemberUpgrade, "Member Upgrade" },
                { KeySet.ChatMemberMilestone, "Member Milestone" },
                { KeySet.ChatMemberGift, "Member Gift" },
                { KeySet.ChatReceivedMemberGift, "Received Member Gift" },
                { KeySet.ChatRedirect, "Redirect" },
                { KeySet.ChatPinned, "Pinned" },
                // 使用 Contains() 判斷關鍵字詞。
                { KeySet.MemberUpgrade, "Upgraded membership to" },
                { KeySet.MemberMilestone, "Member for" }
            }
        },
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
                // 使用 Contains() 判斷關鍵字詞。
                { KeySet.MemberUpgrade, "頻道會員等級已升級至" },
                { KeySet.MemberMilestone, "已加入會員" }
            }
        },
        {
            EnumSet.DisplayLanguage.Chinese_Simplified,
            new Dictionary<string, string>()
            {
                { KeySet.ChatGeneral, "一般" },
                { KeySet.ChatSuperChat, "超级留言" },
                { KeySet.ChatSuperSticker, "超级贴图" },
                { KeySet.ChatJoinMember, "加入会员" },
                { KeySet.ChatMemberUpgrade, "会员升级" },
                { KeySet.ChatMemberMilestone, "会员里程碑" },
                { KeySet.ChatMemberGift, "赠送会员" },
                { KeySet.ChatReceivedMemberGift, "接收会员赠送" },
                { KeySet.ChatRedirect, "重新导向" },
                { KeySet.ChatPinned, "置顶留言" },
                // 使用 Contains() 判斷關鍵字詞。
                { KeySet.MemberUpgrade, "会员级别已升至" },
                { KeySet.MemberMilestone, "会龄" }
            }
        },
        {
            EnumSet.DisplayLanguage.Japanese,
            new Dictionary<string, string>()
            {
                { KeySet.ChatGeneral, "一般" },
                { KeySet.ChatSuperChat, "スーパーチャット" },
                { KeySet.ChatSuperSticker, "スーパーステッカー" },
                { KeySet.ChatJoinMember, "メンバー登録" },
                { KeySet.ChatMemberUpgrade, "会員アップグレード" },
                { KeySet.ChatMemberMilestone, "会員マイルストーン" },
                { KeySet.ChatMemberGift, "会員ギフト" },
                { KeySet.ChatReceivedMemberGift, "会員プレゼントを受け取る" },
                { KeySet.ChatRedirect, "リダイレクト" },
                { KeySet.ChatPinned, "ピン留め" },
                // 使用 Contains() 判斷關鍵字詞。
                { KeySet.MemberUpgrade, "にアップグレードされました" },
                { KeySet.MemberMilestone, "メンバー歴" }
            }
        },
        {
            EnumSet.DisplayLanguage.Korean,
            new Dictionary<string, string>()
            {
                { KeySet.ChatGeneral, "일반" },
                { KeySet.ChatSuperChat, "슈퍼 채팅" },
                { KeySet.ChatSuperSticker, "슈퍼 스티커" },
                { KeySet.ChatJoinMember, "회원 가입" },
                { KeySet.ChatMemberUpgrade, "회원 업그레이드" },
                { KeySet.ChatMemberMilestone, "회원 마일스톤" },
                { KeySet.ChatMemberGift, "회원 선물" },
                { KeySet.ChatReceivedMemberGift, "회원 선물 받기" },
                { KeySet.ChatRedirect, "리디렉션" },
                { KeySet.ChatPinned, "고정" },
                // 使用 Contains() 判斷關鍵字詞。
                { KeySet.MemberUpgrade, "멤버십을" },
                { KeySet.MemberMilestone, "회원 가입 기간" }
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