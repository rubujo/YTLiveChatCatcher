using System.Threading.Channels;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>固定消費者與有限等待容量；保留建立時的同步內容供 UI 工作使用。</summary>
public sealed class BoundedWorkQueue : IAsyncDisposable
{
    private readonly Channel<Func<CancellationToken, Task>> _channel;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly Task _completion;
    private Task? _stopTask;

    public BoundedWorkQueue(int concurrency, int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(concurrency, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        _channel = Channel.CreateBounded<Func<CancellationToken, Task>>(capacity);
        _completion = Task.WhenAll(Enumerable.Range(0, concurrency).Select(_ => ConsumeAsync()));
    }

    /// <summary>容量不足時拒絕非必要工作，呼叫端不能為拒絕的工作另建無界等待。</summary>
    public bool TryEnqueue(Func<CancellationToken, Task> work) =>
        _channel.Writer.TryWrite(work);

    public Task StopAsync() => _stopTask ??= StopCoreAsync();

    private async Task StopCoreAsync()
    {
        _channel.Writer.TryComplete();
        _cancellation.Cancel();
        try
        {
            await _completion;
        }
        finally
        {
            while (_channel.Reader.TryRead(out _)) { }
            _cancellation.Dispose();
        }
    }

    private async Task ConsumeAsync()
    {
        try
        {
            await foreach (Func<CancellationToken, Task> work in _channel.Reader.ReadAllAsync(_cancellation.Token))
            {
                _cancellation.Token.ThrowIfCancellationRequested();
                await work(_cancellation.Token);
            }
        }
        catch (OperationCanceledException) when (_cancellation.IsCancellationRequested) { }
    }

    public ValueTask DisposeAsync() => new(StopAsync());
}
