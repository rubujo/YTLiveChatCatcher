namespace LiveChatCatcher.Models;

/// <summary>
/// 作者徽章資料
/// </summary>
public class AuthorBadgesData
{
    /// <summary>
    /// 文字
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 列表：徽章資料
    /// </summary>
    public List<BadgeData>? Badges { get; set; }
}