using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility.Extensions;

/// <summary>
/// 列舉的擴充方法
/// </summary>
public static class EnumExtension
{
    /// <summary>
    /// 轉換成數值
    /// </summary>
    /// <param name="liveChatType">EnumSet.LiveChatType</param>
    /// <returns>數值</returns>
    public static int ToInt32(this EnumSet.LiveChatType liveChatType)
    {
        return Convert.ToInt32(liveChatType);
    }
}