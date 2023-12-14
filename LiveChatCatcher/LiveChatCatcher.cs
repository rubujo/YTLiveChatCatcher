using System.Diagnostics.CodeAnalysis;
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
    /// <param name="httpClient">HttpClient</param>
    /// <param name="timeoutMs">數值，逾時的毫秒值，預設值 3000</param>
    /// <param name="isStreaming">布林值，是否為直播，預設值為 false</param>
    /// <param name="isFetchLargePicture">布林值，是否獲取大張圖片，預設值為 true</param>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public void Init(
        HttpClient httpClient,
        int timeoutMs = 3000,
        bool isStreaming = false,
        bool isFetchLargePicture = true)
    {
        SharedHttpClient = httpClient;
        SharedTimeoutMs = timeoutMs;
        SharedIsStreaming = isStreaming;
        SharedIsFetchLargePicture = isFetchLargePicture;
        SharedCookies = string.Empty;
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

        // 開始 Task。
        SharedCancellationTokenSource = new CancellationTokenSource();

        SharedTask = Task.Run(() =>
            FetchLiveChatData(GetYouTubeVideoID(videoUrlOrID)),
            SharedCancellationTokenSource.Token);

        SharedTask.ContinueWith((task) =>
            TaskCompleted(task, null),
            SharedCancellationTokenSource.Token);
    }

    /// <summary>
    /// 停止
    /// </summary>
    [SuppressMessage("Performance", "CA1822:將成員標記為靜態", Justification = "<暫止>")]
    public void Stop()
    {
        SharedCancellationTokenSource?.Cancel();
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

            // 0：continuation、1：timeoutMs。
            string[] continuationData = ParseContinuation(jsonElement);

            // 更換 Continuation。
            ytConfigData.Continuation = continuationData[0];

            if (int.TryParse(continuationData[1], out int timeoutMs))
            {
                // 更新 SharedTimeoutMs。
                SharedTimeoutMs = timeoutMs;

                RaiseOnLogOutput(
                    EnumSet.LogType.Info,
                    $"接收到的 timeoutMs：{SharedTimeoutMs}");
            }

            List<RendererData> messages = ParseActions(jsonElement);

            if (messages.Count > 0)
            {
                RaiseOnFecthLiveChat(messages);
            }

            RaiseOnLogOutput(
                EnumSet.LogType.Info,
                $"於 {SharedTimeoutMs / 1000} 秒後，獲取下一批次的即時聊天資料。");

            SpinWait.SpinUntil(() => false, SharedTimeoutMs);
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

            RaiseOnLogOutput(
                EnumSet.LogType.Error,
                task.Exception.GetExceptionMessage());

            return;
        }

        RaiseOnRunningStatusUpdate(EnumSet.RunningStatus.Stopped);
    }
}