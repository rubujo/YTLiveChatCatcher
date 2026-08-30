using System.Runtime.Versioning;

namespace YTLiveChatCatcher.Extensions;

/// <summary>
/// 控制項的擴充方法
/// </summary>
public static class ControlExtension
{
    /// <summary>
    /// 若目前不在 UI 執行緒上，透過 Control.Invoke 轉送到 UI 執行緒執行；已經在 UI 執行緒上則直接呼叫。
    /// </summary>
    /// <param name="control">Control</param>
    /// <param name="action">MethodInvoker</param>
    [SupportedOSPlatform("windows")]
    public static void InvokeIfRequired(this Control control, MethodInvoker action)
    {
        // 在非當前執行緒內，使用委派。
        if (control.InvokeRequired)
        {
            control.Invoke(action);
        }
        else
        {
            action();
        }
    }

    /// <summary>
    /// 非同步委派更新 UI（.NET 9+ Control.InvokeAsync）
    /// <para>跟 InvokeIfRequired 的差異：Control.Invoke 會阻塞呼叫端執行緒直到 UI 執行緒處理完委派，
    /// Control.InvokeAsync 只是把委派排入 UI 執行緒的訊息佇列後就立即返回，呼叫端改用 await 等待，
    /// 不會佔用執行緒集區的執行緒。適合用在背景執行緒（例如 Task.Run 內）頻繁或會處理較多資料的 UI 更新；
    /// 由 UI 執行緒直接呼叫的事件處理常式，或每次只更新單一控制項的低頻呼叫，維持用 InvokeIfRequired 即可。</para>
    /// </summary>
    /// <param name="control">Control</param>
    /// <param name="action">Action</param>
    /// <param name="cancellationToken">CancellationToken，預設值為 default</param>
    /// <returns>Task</returns>
    [SupportedOSPlatform("windows")]
    public static Task InvokeAsyncIfRequired(this Control control, Action action, CancellationToken cancellationToken = default)
    {
        if (control.InvokeRequired)
        {
            return control.InvokeAsync(action, cancellationToken);
        }

        action();

        return Task.CompletedTask;
    }
}