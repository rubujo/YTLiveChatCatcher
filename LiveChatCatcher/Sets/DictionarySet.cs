using Org.BouncyCastle.Utilities;
using Rubujo.YouTube.Utility.Models;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

namespace Rubujo.YouTube.Utility.Sets;

public class DictionarySet
{

    public static Dictionary<string, RegionData> DictRegion = new()
    {
        { "123", new RegionData() { GL = "af", HL = "af", TimeZone = "" } },
    };


    /// <summary>
    /// 區域資料
    /// </summary>
    public class RegionData
    {
        /// <summary>
        /// Google 的國家參數值
        /// </summary>
        public string? GL { get; set; }

        /// <summary>
        /// Google 的語言參數值
        /// </summary>
        public string? HL { get; set; }

        /// <summary>
        /// 時區
        /// </summary>
        public string? TimeZone { get; set; }
    }
}