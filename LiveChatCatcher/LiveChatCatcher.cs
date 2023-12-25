using System.Text.Json;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Sets;

namespace Rubujo.YouTube.Utility;

/// <summary>
/// LiveChatCatcher
/// </summary>
public partial class LiveChatCatcher
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
        SharedDisplayLanguage = EnumSet.DisplayLanguage.Chinese_Traditional;
        SharedLiveChatType = EnumSet.LiveChatType.All;
        SharedCustomLiveChatType = string.Empty;
        SharedIntervalMs = 0;
        SharedForceIntervalMs = -1;

        // 當傳入的 httpClient 為 null 時，則自動建立 HttpClient。
        SharedHttpClient ??= CreateHttpClient();
    }

    /// <summary>
    /// 開始
    /// </summary>
    /// <param name="videoUrlOrID">字串，YouTube 影片網址或是 ID 值</param>
    public void Start(string videoUrlOrID)
    {
        if (SharedTask != null && !SharedTask.IsCompleted)
        {
            RaiseOnLogOutput(
                EnumSet.LogType.Warn,
                "[LiveChatCatcher.Start()] Task 正在執行中。");

            return;
        }

        if (SharedHttpClient == null)
        {
            RaiseOnRunningStatusUpdate(EnumSet.RunningStatus.ErrorOccured);

            RaiseOnLogOutput(
                EnumSet.LogType.Error,
                "[LiveChatCatcher.Start()] 發生錯誤，變數 \"SharedHttpClient\" 是 null！");

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
    /// 獲取即時聊天資料
    /// </summary>
    /// <param name="videoID">字串，YouTube 影片的 ID 值</param>
    private void FetchLiveChatData(string videoID)
    {
        RaiseOnRunningStatusUpdate(EnumSet.RunningStatus.Running);

        InitialData initialData = GetYTConfigData(videoID);

        YTConfigData? ytConfigData = initialData.YTConfigData;

        if (ytConfigData == null)
        {
            RaiseOnRunningStatusUpdate(EnumSet.RunningStatus.ErrorOccured);
            RaiseOnLogOutput(
                EnumSet.LogType.Error,
                "[LiveChatCatcher.FetchLiveChatData()] 發生錯誤，變數 \"ytConfigData\" 是 null！");

            return;
        }

        // 處理直播中頁面內的影片聊天室的內容。
        if (initialData.Messages != null &&
            initialData.Messages.Count > 0)
        {
            RaiseOnFecthLiveChat(initialData.Messages);
        }

        // 持續取得即時聊天資料。
        while (SharedCancellationTokenSource?.IsCancellationRequested == false)
        {
            JsonElement jsonElement = GetJsonElement(ytConfigData);

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
                RaiseOnFecthLiveChat(messages);
            }

            RaiseOnLogOutput(
                EnumSet.LogType.Info,
                $"於 {IntervalMs() / 1000} 秒後，獲取下一批次的即時聊天資料。");

            SpinWait.SpinUntil(() => false, IntervalMs());
        }
    }

    /// <summary>
    /// 已完成獲取 Live chat 資料
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