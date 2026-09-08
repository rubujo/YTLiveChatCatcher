using Accessibility;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static YTLiveChatCatcher.UiaNative;
using Xunit;

namespace YTLiveChatCatcher.Tests;

public class AccessibleChatListViewTests
{
    [Fact]
    public async Task 有界UIA介面_大量虛擬列可讀取並選取且不列舉全部資料()
    {
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Thread thread = new(() =>
        {
            using Form form = new() { Opacity = 0, ShowInTaskbar = false };
            using AccessibleChatListView list = new()
            {
                Dock = DockStyle.Fill, View = View.Details, VirtualMode = true,
                VirtualListSize = 50_000, AccessibleName = "聊天室測試"
            };
            list.Columns.Add("作者", 100);
            list.Columns.Add("訊息", 180);
            int retrievals = 0;
            list.RetrieveVirtualItem += (_, e) =>
            {
                retrievals++;
                e.Item = new ListViewItem([$"作者 {e.ItemIndex}", $"內容 {e.ItemIndex}"]);
            };
            form.Controls.Add(list);
            form.Shown += (_, _) =>
            {
                try
                {
                    IAccessible accessible = list.AccessibilityObject;
                    retrievals = 0;
                    Assert.Equal("聊天室測試", accessible.get_accName(0));
                    ChatListAutomationProvider provider = list.AutomationProvider;
                    nint unknown = Marshal.GetIUnknownForObject(provider);
                    nint fragment = 0;
                    try
                    {
                        Guid fragmentId = typeof(IRawElementProviderFragment).GUID;
                        Assert.Equal(0, Marshal.QueryInterface(unknown, in fragmentId, out fragment));
                        Assert.NotEqual(0, fragment);
                    }
                    finally
                    {
                        if (fragment != 0) Marshal.Release(fragment);
                        Marshal.Release(unknown);
                    }
                    Assert.Equal(50_000, provider.Count);
                    int visibleCount = 0;
                    for (IRawElementProviderFragment? child = provider.Navigate(NavigateDirection.FirstChild); child != null;
                        child = child.Navigate(NavigateDirection.NextSibling))
                    {
                        Assert.NotNull(((IRawElementProviderSimple)child).GetPropertyValue(NamePropertyId));
                        Assert.False(child.get_BoundingRectangle().IsEmpty);
                        Assert.True(++visibleCount < 100);
                    }
                    Assert.True(visibleCount > 0);
                    Assert.True(retrievals < 1000, $"無障礙查詢造成 {retrievals} 次資料讀取。");
                    ChatListAutomationProvider.RowProvider last = provider.Row(49_999);
                    Assert.True(last.get_BoundingRectangle().IsEmpty);
                    Assert.Contains("內容 49999", last.Name);
                    last.Realize();
                    last.Select();
                    Assert.Contains(49_999, list.SelectedIndices.Cast<int>());
                    Assert.False(last.get_BoundingRectangle().IsEmpty);
                    list.VirtualListSize = 0;
                    Assert.Null(provider.Navigate(NavigateDirection.FirstChild));
                    list.VirtualListSize = 50_000;
                    Assert.Throws<InvalidOperationException>(() => last.Select());
                    completion.TrySetResult();
                }
                catch (Exception ex) { completion.TrySetException(ex); }
                finally { form.Close(); }
            };
            Application.Run(form);
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        await completion.Task.WaitAsync(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);
    }
}
