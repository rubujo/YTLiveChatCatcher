using Rubujo.YouTube.Utility.Models.Community;

namespace Rubujo.YouTube.Utility.Events;

/// <summary>
/// 社群貼文獲取事件參數
/// </summary>
/// <param name="dataSet">List&lt;RendererData&gt;</param>
public class FecthCommunityPostsArgs(List<PostData> dataSet) : EventArgs
{
    /// <summary>
    /// List&lt;PostData&gt;
    /// </summary>
    public List<PostData> Data { get; set; } = dataSet;
}