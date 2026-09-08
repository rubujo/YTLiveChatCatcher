using System.Runtime.InteropServices;
using static YTLiveChatCatcher.UiaNative;

namespace YTLiveChatCatcher;

// UIA 透過 COM 跨執行緒呼叫，StandardOleMarshalObject 會切回建立 provider 的 UI STA。
[ComVisible(true), ClassInterface(ClassInterfaceType.None)]
internal sealed class ChatListAutomationProvider(AccessibleChatListView list) : StandardOleMarshalObject,
    IRawElementProviderSimple, IRawElementProviderFragmentRoot, IRawElementProviderFragment,
    ISelectionProvider, IItemContainerProvider
{
    private readonly AccessibleChatListView owner = list;
    internal int Count => owner.IsDisposed || !owner.IsHandleCreated ? 0 : owner.VirtualListSize;
    internal int First => Count == 0 ? 0 : owner.TopItem?.Index ?? 0;
    internal int Last
    {
        get
        {
            if (Count == 0) return -1;
            Rectangle bounds = owner.GetItemRect(First);
            int height = Math.Max(1, bounds.Height);
            return Math.Min(Count - 1, First + Math.Max(0, owner.ClientSize.Height - bounds.Top - 1) / height);
        }
    }

    internal RowProvider Row(int index) => new(this, index);
    internal void NotifyViewportChanged() => owner.NotifyAccessibilityViewportChanged();

    public ProviderOptions get_ProviderOptions() => ProviderOptions.ServerSideProvider | ProviderOptions.UseComThreading;
    public object? GetPatternProvider(int patternId) =>
        patternId is SelectionPatternId or ItemContainerPatternId ? this : null;
    public object? GetPropertyValue(int propertyId) => propertyId switch
    {
        ControlTypePropertyId => ListControlTypeId,
        NamePropertyId => owner.AccessibleName ?? "聊天室內容",
        AutomationIdPropertyId => owner.Name,
        IsControlElementPropertyId or IsContentElementPropertyId => true,
        IsEnabledPropertyId => owner.Enabled,
        IsKeyboardFocusablePropertyId => true,
        HasKeyboardFocusPropertyId => owner.Focused,
        HelpTextPropertyId => $"虛擬聊天室清單，共 {Count} 筆；方向鍵、Home、End 或捲動可瀏覽全部留言。",
        _ => null
    };
    public IRawElementProviderSimple? get_HostRawElementProvider()
    {
        _ = UiaHostProviderFromHwnd(owner.Handle, out IRawElementProviderSimple? provider);
        return provider;
    }

    public UiaRect get_BoundingRectangle()
    {
        if (owner.IsDisposed || !owner.IsHandleCreated) return default;
        Rectangle bounds = owner.RectangleToScreen(owner.ClientRectangle);
        return new(bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }
    public IRawElementProviderFragmentRoot get_FragmentRoot() => this;
    public IRawElementProviderFragment? Navigate(NavigateDirection direction) => Count == 0 ? null : direction switch
    {
        NavigateDirection.FirstChild => Row(First),
        NavigateDirection.LastChild => Row(Last),
        _ => null
    };
    public int[]? GetRuntimeId() => null;
    public IRawElementProviderSimple[]? GetEmbeddedFragmentRoots() => null;
    public void SetFocus() => owner.Focus();
    public IRawElementProviderFragment? GetFocus() => owner.FocusedItem is { } item ? Row(item.Index) : null;
    public IRawElementProviderFragment? ElementProviderFromPoint(double x, double y)
    {
        UiaRect bounds = get_BoundingRectangle();
        if (x < bounds.Left || y < bounds.Top || x >= bounds.Left + bounds.Width || y >= bounds.Top + bounds.Height)
            return null;
        Point point = owner.PointToClient(new((int)x, (int)y));
        return owner.GetItemAt(point.X, point.Y) is { } item ? Row(item.Index) : this;
    }

    public IRawElementProviderSimple[] GetSelection() => owner.SelectedIndices.Cast<int>()
        .Select(index => (IRawElementProviderSimple)Row(index)).ToArray();
    public bool get_CanSelectMultiple() => owner.MultiSelect;
    public bool get_IsSelectionRequired() => false;

    public IRawElementProviderSimple? FindItemByProperty(IRawElementProviderSimple? startAfter, int propertyId, object? value)
    {
        int start = startAfter is RowProvider row && row.Root == this ? row.Index + 1 : 0;
        if (startAfter != null && (startAfter is not RowProvider prior || prior.Root != this))
            throw new ArgumentException("項目不屬於此清單。");
        if (propertyId is not 0 and not NamePropertyId)
            throw new ArgumentException("僅支援依順序或名稱取得虛擬項目。");

        for (int index = start; index < Count; index++)
        {
            RowProvider candidate = Row(index);
            if (propertyId == 0 || candidate.Name == value as string) return candidate;
        }
        return null;
    }

    [ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    internal sealed class RowProvider(ChatListAutomationProvider root, int index) : StandardOleMarshalObject,
        IRawElementProviderSimple, IRawElementProviderFragment, ISelectionItemProvider,
        IVirtualizedItemProvider, IScrollItemProvider
    {
        private readonly int generation = root.owner.DataGeneration;
        internal ChatListAutomationProvider Root => root;
        internal int Index => index;
        private bool Valid => generation == root.owner.DataGeneration && index >= 0 && index < root.Count;
        private void Check() { if (!Valid) throw new InvalidOperationException("此 UIA 資料列已失效。"); }
        internal string Name
        {
            get
            {
                Check();
                ListViewItem item = root.owner.ReadVirtualItem(index);
                return string.Join("；", root.owner.Columns.Cast<ColumnHeader>()
                    .Select((column, columnIndex) => (column, columnIndex))
                    .Where(entry => entry.column.Width > 0 && entry.columnIndex < item.SubItems.Count)
                    .Select(entry => $"{entry.column.Text}：{item.SubItems[entry.columnIndex].Text}"));
            }
        }

        public ProviderOptions get_ProviderOptions() => root.get_ProviderOptions();
        public object? GetPatternProvider(int patternId) =>
            patternId is SelectionItemPatternId or VirtualizedItemPatternId or ScrollItemPatternId ? this : null;
        public object? GetPropertyValue(int propertyId) => propertyId switch
        {
            NamePropertyId => Name,
            ControlTypePropertyId => ListItemControlTypeId,
            AutomationIdPropertyId => $"Row{index}",
            IsControlElementPropertyId or IsContentElementPropertyId or IsKeyboardFocusablePropertyId => true,
            IsEnabledPropertyId => Valid && root.owner.Enabled,
            IsOffscreenPropertyId => get_BoundingRectangle().IsEmpty,
            HasKeyboardFocusPropertyId => root.owner.Focused && root.owner.FocusedItem?.Index == index,
            PositionInSetPropertyId => index + 1,
            SizeOfSetPropertyId => root.Count,
            _ => null
        };
        public IRawElementProviderSimple? get_HostRawElementProvider() => null;

        public UiaRect get_BoundingRectangle()
        {
            if (!Valid || index < root.First || index > root.Last) return default;
            Rectangle bounds = root.owner.RectangleToScreen(root.owner.GetItemRect(index));
            bounds.Intersect(root.owner.RectangleToScreen(root.owner.ClientRectangle));
            return bounds.IsEmpty ? default : new(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }
        public IRawElementProviderFragmentRoot get_FragmentRoot() => root;
        public IRawElementProviderFragment? Navigate(NavigateDirection direction) => direction switch
        {
            NavigateDirection.Parent => root,
            NavigateDirection.NextSibling when Valid && index >= root.First && index < root.Last => root.Row(index + 1),
            NavigateDirection.PreviousSibling when Valid && index > root.First && index <= root.Last => root.Row(index - 1),
            _ => null
        };
        public int[] GetRuntimeId() => [AppendRuntimeId, generation, index];
        public IRawElementProviderSimple[]? GetEmbeddedFragmentRoots() => null;
        public void SetFocus()
        {
            Realize();
            root.owner.Focus();
            Rectangle bounds = root.owner.GetItemRect(index);
            if (root.owner.GetItemAt(bounds.Left + 1, bounds.Top + 1) is { } item) item.Focused = true;
        }

        public void Select() { Check(); root.owner.SelectedIndices.Clear(); root.owner.SelectedIndices.Add(index); }
        public void AddToSelection() { Check(); if (!root.owner.MultiSelect) Select(); else root.owner.SelectedIndices.Add(index); }
        public void RemoveFromSelection() { Check(); root.owner.SelectedIndices.Remove(index); }
        public bool get_IsSelected() => Valid && root.owner.SelectedIndices.Contains(index);
        public IRawElementProviderSimple get_SelectionContainer() => root;
        public void Realize() { Check(); root.owner.EnsureVisible(index); root.NotifyViewportChanged(); }
        public void ScrollIntoView() => Realize();
    }
}
