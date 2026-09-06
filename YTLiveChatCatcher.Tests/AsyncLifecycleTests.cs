using System.Windows.Forms;
using Microsoft.Extensions.Logging.Abstractions;
using Rubujo.YouTube.Utility;
using YTLiveChatCatcher.Common.Utils;
using Xunit;
using System.Reflection;

namespace YTLiveChatCatcher.Tests;

public class AsyncLifecycleTests
{
    [Fact]
    public async Task Queue_限制並行與等待容量並取消未開始工作()
    {
        await using BoundedWorkQueue queue = new(2, 3);
        int running = 0, started = 0;
        TaskCompletionSource bothStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task Work(CancellationToken token)
        {
            Interlocked.Increment(ref started);
            if (Interlocked.Increment(ref running) == 2) bothStarted.TrySetResult();
            return Task.Delay(Timeout.Infinite, token);
        }
        Assert.True(queue.TryEnqueue(Work));
        Assert.True(queue.TryEnqueue(Work));
        await bothStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        for (int i = 0; i < 3; i++) Assert.True(queue.TryEnqueue(Work));
        Assert.False(queue.TryEnqueue(Work));
        await queue.StopAsync().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(2, started);
        Assert.False(queue.TryEnqueue(Work));
    }

    [Fact]
    public Task Close_等待需要UI內容的背景工作後才關閉() => RunStaAsync(async () =>
    {
        using TestMain main = new();
        main.Show();
        CancellationTokenSource cancellation = new();
        SetField(main, "SharedFetchCancellationTokenSource", cancellation);
        bool finished = false;
        async Task FinishOnUiAsync()
        {
            await Task.Delay(50, TestContext.Current.CancellationToken);
            finished = true;
        }
        SetField(main, "SharedFetchTask", FinishOnUiAsync());
        TaskCompletionSource closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        main.FormClosed += (_, _) => closed.TrySetResult();
        main.Close();
        Assert.False(main.IsDisposed);
        await closed.Task.WaitAsync(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);
        Assert.True(finished);
        Assert.True(cancellation.IsCancellationRequested);
        cancellation.Dispose();
    });

    [Fact]
    public Task AutoFit_最後一批沒有後續資料仍會補做且樣本有界() => RunStaAsync(async () =>
    {
        using TestMain main = new();
        main.Show();
        ListView list = main.Controls.OfType<ListView>().Single(x => x.Name == "LVLiveChatList");
        FMain.InitListView(list);
        SetField(main, "SharedLastAutoFitUtc", DateTime.UtcNow);
        ListViewItem item = new("作者");
        item.SubItems.AddRange(Enumerable.Repeat("測試長文字測試長文字測試長文字", 16).ToArray());
        MethodInfo autoFit = typeof(FMain).GetMethod("AutoFitLiveChatColumnsThrottled", BindingFlags.NonPublic | BindingFlags.Instance)!;
        for (int i = 0; i < 10; i++) autoFit.Invoke(main, [Enumerable.Repeat(item, 128).ToList(), false]);
        List<ListViewItem> pending = (List<ListViewItem>)Field("SharedPendingAutoFitItems").GetValue(main)!;
        Assert.InRange(pending.Count, 1, 512);
        TaskCompletionSource flushed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using System.Windows.Forms.Timer timer = new() { Interval = 20 };
        timer.Tick += (_, _) => { if (pending.Count == 0) flushed.TrySetResult(); };
        timer.Start();
        await flushed.Task.WaitAsync(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);
        Assert.Empty(pending);
    });

    private static FieldInfo Field(string name) =>
        typeof(FMain).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static void SetField(FMain main, string name, object value) => Field(name).SetValue(main, value);

    private sealed class Factory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }

    private sealed class TestMain() : FMain(new Factory(), NullLogger<FMain>.Instance, NullLogger<YTJsonParser>.Instance)
    {
        // 不執行正式 Load，避免接觸使用者 Cookie、復原記錄與網路。
        protected override void OnLoad(EventArgs e) { Opacity = 0; ShowInTaskbar = false; }
    }

    private static async Task RunStaAsync(Func<Task> test)
    {
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Thread thread = new(() =>
        {
            using Form host = new() { Opacity = 0, ShowInTaskbar = false };
            host.Shown += async (_, _) =>
            {
                try { await test(); completion.TrySetResult(); }
                catch (Exception error) { completion.TrySetException(error); }
                finally { host.Close(); }
            };
            Application.Run(host);
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        await completion.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
    }
}
