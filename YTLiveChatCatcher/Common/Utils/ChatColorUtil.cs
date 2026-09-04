using System.Drawing;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>聊天室顏色字串的安全轉換工具。</summary>
public static class ChatColorUtil
{
    /// <summary>
    /// 嘗試解析聊天室資料中的 HTML 色碼；空值、透明色 0 與無效內容都視為沒有樣式覆寫。
    /// </summary>
    public static bool TryParse(string? value, out Color color)
    {
        color = default;

        if (string.IsNullOrWhiteSpace(value) ||
            value == "0" ||
            value == Rubujo.YouTube.Utility.Sets.KeySet.NoForegroundColor ||
            value == Rubujo.YouTube.Utility.Sets.KeySet.NoBackgroundColor)
        {
            return false;
        }

        try
        {
            color = ColorTranslator.FromHtml(value);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
