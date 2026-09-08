using System.Runtime.InteropServices;

namespace YTLiveChatCatcher;

/// <summary>UIA 僅展開可見列，避免對大量虛擬資料逐列計算邊界。</summary>
public class AccessibleChatListView : ListView
{
    private ChatListAutomationProvider? provider;
    internal int DataGeneration { get; private set; }
    internal ChatListAutomationProvider AutomationProvider => provider ??= new(this);

    protected override AccessibleObject CreateAccessibilityInstance() => new ControlAccessibleObject(this);

    internal ListViewItem ReadVirtualItem(int index)
    {
        RetrieveVirtualItemEventArgs args = new(index);
        OnRetrieveVirtualItem(args);
        return args.Item ?? throw new InvalidOperationException("虛擬清單未提供資料列。");
    }

    internal void NotifyAccessibilityViewportChanged() =>
        AccessibilityNotifyClients(AccessibleEvents.Reorder, -1);

    protected override void OnHandleDestroyed(EventArgs e)
    {
        DataGeneration++;
        provider = null;
        base.OnHandleDestroyed(e);
    }

    protected override void WndProc(ref Message m)
    {
        // 只接管 UIA root；MSAA 與其他原生無障礙請求仍交給 WinForms。
        if (m.Msg == 0x003D && unchecked((int)(long)m.LParam) == -25 && VirtualMode)
        {
            m.Result = UiaReturnRawElementProvider(Handle, m.WParam, m.LParam, AutomationProvider);
            return;
        }

        if (m.Msg == 0x102F) DataGeneration++;
        base.WndProc(ref m);
        if (m.Msg is 0x0115 or 0x020A or 0x0100 or 0x102F)
            NotifyAccessibilityViewportChanged();
    }

    [DllImport("UIAutomationCore.dll")]
    private static extern nint UiaReturnRawElementProvider(nint hwnd, nint wParam, nint lParam,
        [MarshalAs(UnmanagedType.Interface)] UiaNative.IRawElementProviderSimple provider);
}
