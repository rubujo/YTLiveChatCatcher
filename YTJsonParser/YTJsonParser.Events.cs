using Rubujo.YouTube.Utility.Events;
using Rubujo.YouTube.Utility.Models.Community;
using Rubujo.YouTube.Utility.Models.LiveChat;
using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// YTJsonParser 的事件
/// </summary>
public partial class YTJsonParser
{
    /// <summary>
    /// 紀錄輸出事件
    /// </summary>
    public event EventHandler<LogOutputArgs>? OnLogOutput;

    /// <summary>
    /// 即時聊天資料獲取事件
    /// </summary>
    public event EventHandler<FecthLiveChatDataArgs>? OnFecthLiveChatData;

    /// <summary>
    /// 社群貼文獲取事件
    /// </summary>
    public event EventHandler<FecthCommunityPostsArgs>? OnFecthCommunityPosts;

    /// <summary>
    /// 執行狀態更新事件
    /// </summary>
    public event EventHandler<RunningStatusArgs>? OnRunningStatusUpdate;

    /// <summary>
    /// 引發紀錄輸出事件
    /// </summary>
    /// <param name="logType">EnumSet.LogType，紀錄類型</param>
    /// <param name="message">字串，訊息</param>
    private void RaiseOnLogOutput(EnumSet.LogType logType, string message) =>
        OnLogOutput?.Invoke(this, new LogOutputArgs(logType, message));

    /// <summary>
    /// 引發即時聊天資料獲取事件
    /// </summary>
    /// <param name="dataSet">List&lt;RendererData&gt;</param>
    private void RaiseOnFecthLiveChatData(List<RendererData> dataSet) =>
        OnFecthLiveChatData?.Invoke(this, new FecthLiveChatDataArgs(dataSet));


    /// <summary>
    /// 引發社群貼文獲取事件
    /// </summary>
    /// <param name="dataSet">List&lt;PostData&gt;</param>
    private void RaiseOnFecthCommunityPosts(List<PostData> dataSet) =>
        OnFecthCommunityPosts?.Invoke(this, new FecthCommunityPostsArgs(dataSet));

    /// <summary>
    /// 引發執行狀態更新事件
    /// </summary>
    /// <param name="runningStatus">RunningStatusArgs，執行狀態</param>
    private void RaiseOnRunningStatusUpdate(EnumSet.RunningStatus runningStatus) =>
        OnRunningStatusUpdate?.Invoke(this, new RunningStatusArgs(runningStatus));
}