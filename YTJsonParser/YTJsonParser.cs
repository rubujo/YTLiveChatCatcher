using System.Text.Json;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Models.Community;
using Rubujo.YouTube.Utility.Models.LiveChat;
using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// YTJsonParser
/// </summary>
public partial class YTJsonParser
{
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="httpClient">HttpClient，預設值為 null</param>
    public void Init(HttpClient? httpClient = null)
    {
        SharedTask = null;
        SharedCancellationTokenSource = null;
        SharedHttpClient = httpClient;
        SharedCookies = string.Empty;
        SharedIsStreaming = false;
        SharedIsFetchLargePicture = true;
        SharedFetchWholeCommunityPosts = true;
        SharedDisplayLanguage = EnumSet.DisplayLanguage.Chinese_Traditional;
        SharedLiveChatType = EnumSet.LiveChatType.All;
        SharedCustomLiveChatType = string.Empty;
        SharedIntervalMs = 0;
        SharedForceIntervalMs = -1;

        // 當傳入的 httpClient 為 null 時，則自動建立 HttpClient。
        SharedHttpClient ??= CreateHttpClient();
    }

    /// <summary>
    /// 開始獲取即時聊天資料
    /// </summary>
    /// <param name="videoUrlOrID">字串，YouTube 影片網址或是 ID 值</param>
    public void StartFetchLiveChatData(string videoUrlOrID)
    {
        if (SharedTask != null && !SharedTask.IsCompleted)
        {
            RaiseOnLogOutput(
                EnumSet.LogType.Warn,
                "[YTJsonParser.StartFetchLiveChatData()] Task 正在執行中。");

            return;
        }

        if (SharedHttpClient == null)
        {
            RaiseOnRunningStatusUpdate(EnumSet.RunningStatus.ErrorOccured);

            RaiseOnLogOutput(
                EnumSet.LogType.Error,
                "[YTJsonParser.StartFetchLiveChatData()] 發生錯誤，變數 \"SharedHttpClient\" 是 null！");

            return;
        }

        SharedCancellationTokenSource = new CancellationTokenSource();

        // 開始 Task。
        SharedTask = Task.Run(() =>
        {
            string videoID = GetYouTubeVideoID(videoUrl: videoUrlOrID);

            SharedIsStreaming = IsVideoStreaming(videoID: videoID);

            FetchLiveChatData(videoID: videoID);
        },
        SharedCancellationTokenSource.Token);

        SharedTask?.ContinueWith(
            task => TaskCompleted(task, null),
            CancellationToken.None);
    }

    /// <summary>
    /// 開始獲取社群貼文資料
    /// </summary>
    /// <param name="channelUrlOrID">字串，YouTube 頻道網址或是 ID 值</param>
    public void StartFetchCommunityPosts(string channelUrlOrID)
    {
        if (SharedTask != null && !SharedTask.IsCompleted)
        {
            RaiseOnLogOutput(
                EnumSet.LogType.Warn,
                "[YTJsonParser.StartFetchCommunityPosts()] Task 正在執行中。");

            return;
        }

        if (SharedHttpClient == null)
        {
            RaiseOnRunningStatusUpdate(EnumSet.RunningStatus.ErrorOccured);

            RaiseOnLogOutput(
                EnumSet.LogType.Error,
                "[YTJsonParser.StartFetchCommunityPosts()] 發生錯誤，變數 \"SharedHttpClient\" 是 null！");

            return;
        }

        SharedCancellationTokenSource = new CancellationTokenSource();

        // 開始 Task。
        SharedTask = Task.Run(async () =>
        {
            string channelID = await GetYouTubeChannelID(channelUrl: channelUrlOrID);

            FetchCommunityPosts(channelID: channelID);
        },
        SharedCancellationTokenSource.Token);

        SharedTask?.ContinueWith(
            task => TaskCompleted(task, null),
            CancellationToken.None);
    }

    /// <summary>
    /// 獲取即時聊天資料
    /// </summary>
    /// <param name="videoID">字串，YouTube 影片的 ID 值</param>
    private void FetchLiveChatData(string videoID)
    {
        RaiseOnRunningStatusUpdate(EnumSet.RunningStatus.Running);

        InitialData initialData = GetYTConfigData(videoID, EnumSet.DataType.LiveChat);

        YTConfigData? ytConfigData = initialData.YTConfigData;

        if (ytConfigData == null)
        {
            RaiseOnRunningStatusUpdate(EnumSet.RunningStatus.ErrorOccured);
            RaiseOnLogOutput(
                EnumSet.LogType.Error,
                "[YTJsonParser.FetchLiveChatData()] 發生錯誤，變數 \"ytConfigData\" 是 null！");

            return;
        }

        // 處理直播中頁面內的影片聊天室的內容。
        if (initialData.Messages != null &&
            initialData.Messages.Count > 0)
        {
            RaiseOnFecthLiveChatData(initialData.Messages);
        }

        // 持續取得即時聊天資料。
        while (SharedCancellationTokenSource?.IsCancellationRequested == false)
        {
            JsonElement jsonElement = GetJsonElement(ytConfigData, EnumSet.DataType.LiveChat);

            // 判斷是否有取得有效的內容。
            if (string.IsNullOrEmpty(jsonElement.ToString()))
            {
                break;
            }

            // 0：continuation、1：timeoutMs 或 timeUntilLastMessageMsec。
            string[] continuationData = ParseContinuation(jsonElement);

            // 更新 continuation。
            ytConfigData.Continuation = continuationData[0];

            if (int.TryParse(continuationData[1], out int timeoutMs))
            {
                RaiseOnLogOutput(
                    EnumSet.LogType.Info,
                    $"接收到的間隔毫秒值：{timeoutMs}");

                // 更新間隔值。
                IntervalMs(timeoutMs);
            }

            List<RendererData> messages = ParseActions(jsonElement);

            if (messages.Count > 0)
            {
                RaiseOnFecthLiveChatData(messages);
            }

            RaiseOnLogOutput(
                EnumSet.LogType.Info,
                $"於 {IntervalMs() / 1000} 秒後，獲取下一批次的即時聊天資料。");

            SpinWait.SpinUntil(() => false, IntervalMs());
        }
    }

    /// <summary>
    /// 獲取社群貼文資料
    /// </summary>
    /// <param name="channelID">字串，YouTube 頻道的 ID 值</param>
    private void FetchCommunityPosts(string channelID)
    {
        RaiseOnRunningStatusUpdate(EnumSet.RunningStatus.Running);

        InitialData initialData = GetYTConfigData(channelID, EnumSet.DataType.Community);

        YTConfigData? ytConfigData = initialData.YTConfigData;

        if (ytConfigData == null)
        {
            RaiseOnRunningStatusUpdate(EnumSet.RunningStatus.ErrorOccured);
            RaiseOnLogOutput(
                EnumSet.LogType.Error,
                "[YTJsonParser.FetchCommunityPosts()] 發生錯誤，變數 \"ytConfigData\" 是 null！");

            return;
        }

        // 處理初始的社群貼文資料。
        if (initialData.Posts != null &&
            initialData.Posts.Count > 0)
        {
            RaiseOnFecthCommunityPosts(initialData.Posts);
        }

        if (SharedCancellationTokenSource?.IsCancellationRequested != false)
        {
            return;
        }

        // 判斷是否要獲取全部的社群貼文。
        if (SharedFetchWholeCommunityPosts)
        {
            // 持續取得社群資料。
            while (SharedCancellationTokenSource?.IsCancellationRequested == false &&
                !string.IsNullOrEmpty(ytConfigData?.Continuation))
            {
                List<PostData> posts = GetEarlierPosts(ytConfigData: ytConfigData);

                if (posts.Count > 0)
                {
                    RaiseOnFecthCommunityPosts(posts);
                }

                RaiseOnLogOutput(
                    EnumSet.LogType.Info,
                    $"於 {IntervalMs() / 1000} 秒後，獲取下一批次的社群貼文資料。");

                SpinWait.SpinUntil(() => false, IntervalMs());
            }
        }
    }

    /// <summary>
    /// 已完成任務
    /// </summary>
    /// <param name="task">Task</param>
    /// <param name="obj">object</param>
    private void TaskCompleted(Task task, object? obj)
    {
        if (task.IsFaulted)
        {
            RaiseOnRunningStatusUpdate(EnumSet.RunningStatus.ErrorOccured);

            RaiseOnLogOutput(EnumSet.LogType.Error, task.Exception.GetExceptionMessage());
        }

        RaiseOnRunningStatusUpdate(EnumSet.RunningStatus.Stopped);

        // 清除 SharedCancellationTokenSource。
        SharedCancellationTokenSource = null;
        // 清除 SharedTask。
        SharedTask = null;
    }
}