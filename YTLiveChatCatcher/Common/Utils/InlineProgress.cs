namespace YTLiveChatCatcher.Common.Utils;

/// <summary>
/// 在呼叫 <see cref="IProgress{T}.Report"/> 的執行緒上同步執行回呼，避免 <see cref="Progress{T}"/>
/// 捕捉 WinForms SynchronizationContext，或把連續的 session manifest 寫入改成無序的背景回呼。
/// </summary>
public sealed class InlineProgress<T>(Action<T> handler) : IProgress<T>
{
    public void Report(T value) => handler(value);
}
