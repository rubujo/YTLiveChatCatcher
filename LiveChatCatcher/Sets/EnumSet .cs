namespace Rubujo.YouTube.Utility.Sets;

/// <summary>
/// 列舉組
/// </summary>
public class EnumSet
{
    /// <summary>
    /// 即時聊天類型
    /// </summary>
    public enum LiveChatType
    {
        /// <summary>
        /// 熱門
        /// </summary>
        TopHot = 0,
        /// <summary>
        /// 全部
        /// </summary>
        All = 1
    }

    /// <summary>
    /// 紀錄類型
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// 資訊
        /// </summary>
        Info,
        /// <summary>
        /// 警告
        /// </summary>
        Warn,
        /// <summary>
        /// 錯誤
        /// </summary>
        Error,
        /// <summary>
        /// 除錯
        /// </summary>
        Debug
    }

    /// <summary>
    /// 執行狀態
    /// </summary>
    public enum RunningStatus
    {
        /// <summary>
        /// 執行中
        /// </summary>
        Running,
        /// <summary>
        /// 發生錯誤
        /// </summary>
        ErrorOccured,
        /// <summary>
        /// 已停止
        /// </summary>
        Stopped
    }

    /// <summary>
    /// 顯示語言（hl）
    /// <para>gl: https://developers.google.com/custom-search/docs/xml_results_appendices#country-codes</para>
    /// <para>hl: https://developers.google.com/custom-search/docs/xml_results_appendices#supported-interface-languages</para>
    /// <para>https://support.google.com/business/answer/6270107?hl=zh-Hant</para>
    /// </summary>
    public enum DisplayLanguage
    {
        /// <summary>
        /// 南非語
        /// </summary>
        Afrikaans,
        /// <summary>
        /// 阿爾巴尼亞語
        /// </summary>
        Albanian,
        /// <summary>
        /// 阿拉伯語
        /// </summary>
        Arabic,
        /// <summary>
        /// 亞塞拜然語
        /// </summary>
        Azerbaijani,
        /// <summary>
        /// 巴斯克語
        /// </summary>
        Basque,
        /// <summary>
        /// 白俄羅斯語
        /// </summary>
        Belarusian,
        /// <summary>
        /// 孟加拉語
        /// </summary>
        Bengali,
        /// <summary>
        /// 波士尼亞語
        /// </summary>
        Bosnian,
        /// <summary>
        /// 保加利亞語
        /// </summary>
        Bulgarian,
        /// <summary>
        /// 加泰隆尼亞語
        /// </summary>
        Catalan,
        /// <summary>
        /// 簡體中文
        /// </summary>
        Chinese_Simplified,
        /// <summary>
        /// 正體中文
        /// </summary>
        Chinese_Traditional,
        /// <summary>
        /// 克羅埃西亞語
        /// </summary>
        Croatian,
        /// <summary>
        /// 捷克語
        /// </summary>
        Czech,
        /// <summary>
        /// 丹麥語
        /// </summary>
        Danish,
        /// <summary>
        /// 荷蘭語
        /// </summary>
        Dutch,
        /// <summary>
        /// 英文
        /// </summary>
        English,
        /// <summary>
        /// 愛沙尼亞語
        /// </summary>
        Estonian,
        /// <summary>
        /// 芬蘭語
        /// </summary>
        Finnish,
        /// <summary>
        /// 法語
        /// </summary>
        French,
        /// <summary>
        /// 喬治亞語
        /// </summary>
        Georgian,
        /// <summary>
        /// 德語
        /// </summary>
        German,
        /// <summary>
        /// 希臘語
        /// </summary>
        Greek,
        /// <summary>
        /// 古吉拉特語
        /// </summary>
        Gujarati,
        /// <summary>
        /// 希伯來語
        /// </summary>
        Hebrew,
        /// <summary>
        /// 印地語
        /// </summary>
        Hindi,
        /// <summary>
        /// 匈牙利語
        /// </summary>
        Hungarian,
        /// <summary>
        /// 冰島語
        /// </summary>
        Icelandic,
        /// <summary>
        /// 印尼語
        /// </summary>
        Indonesian,
        /// <summary>
        /// 義大利語
        /// </summary>
        Italian,
        /// <summary>
        /// 日語
        /// </summary>
        Japanese,
        /// <summary>
        /// 卡納達語
        /// </summary>
        Kannada,
        /// <summary>
        /// 韓語
        /// </summary>
        Korean,
        /// <summary>
        /// 拉脫維亞語
        /// </summary>
        Latvian,
        /// <summary>
        /// 立陶宛語
        /// </summary>
        Lithuanian,
        /// <summary>
        /// 馬其頓語
        /// </summary>
        Macedonian,
        /// <summary>
        /// 馬來語
        /// </summary>
        Malay,
        /// <summary>
        /// 馬拉雅拉姆語
        /// </summary>
        Malayam,
        /// <summary>
        /// 馬拉地語
        /// </summary>
        Marathi,
        /// <summary>
        /// 尼泊爾語
        /// </summary>
        Nepali,
        /// <summary>
        /// 挪威語
        /// </summary>
        Norwegian,
        /// <summary>
        /// 波斯語
        /// </summary>
        Persian,
        /// <summary>
        /// 波蘭語
        /// </summary>
        Polish,
        /// <summary>
        /// 葡萄牙語（巴西）
        /// </summary>
        Portuguese_Brazil,
        /// <summary>
        /// 葡萄牙語（葡萄牙）
        /// </summary>
        Portuguese_Portugal,
        /// <summary>
        /// 旁遮普語
        /// </summary>
        Punjabi,
        /// <summary>
        /// 羅馬尼亞語
        /// </summary>
        Romanian,
        /// <summary>
        /// 俄語
        /// </summary>
        Russian,
        /// <summary>
        /// 塞爾維亞語
        /// </summary>
        Serbian,
        /// <summary>
        /// 僧伽羅語
        /// </summary>
        Sinhalese,
        /// <summary>
        /// 斯洛伐克語
        /// </summary>
        Slovak,
        /// <summary>
        /// 斯洛維尼亞語
        /// </summary>
        Slovenian,
        /// <summary>
        /// 西班牙語
        /// </summary>
        Spanish,
        /// <summary>
        /// 斯瓦希里語
        /// </summary>
        Swahili,
        /// <summary>
        /// 瑞典語
        /// </summary>
        Swedish,
        /// <summary>
        /// 他加祿語
        /// </summary>
        Tagalog,
        /// <summary>
        /// 泰米爾語
        /// </summary>
        Tamil,
        /// <summary>
        /// 泰盧固語
        /// </summary>
        Telugu,
        /// <summary>
        /// 泰語
        /// </summary>
        Thai,
        /// <summary>
        /// 土耳其語
        /// </summary>
        Turkish,
        /// <summary>
        /// /烏克蘭語
        /// </summary>
        Ukrainian,
        /// <summary>
        /// 烏爾都語
        /// </summary>
        Urdu,
        /// <summary>
        /// 烏茲別克語
        /// </summary>
        Uzbek,
        /// <summary>
        /// 越南語
        /// </summary>
        Vietnamese,
        /// <summary>
        /// 祖魯語
        /// </summary>
        Zulu
    }
}