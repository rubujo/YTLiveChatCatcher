﻿using System.Text.Json.Serialization;

namespace Rubujo.YouTube.Utility.Models.LiveChat;

/// <summary>
/// 作者徽章資料
/// </summary>
public class AuthorBadgesData
{
    /// <summary>
    /// 文字
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 列表：徽章資料
    /// </summary>
    [JsonPropertyName("badges")]
    public List<BadgeData>? Badges { get; set; }
}