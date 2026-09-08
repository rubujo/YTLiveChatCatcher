using System.Runtime.InteropServices;

namespace YTLiveChatCatcher;

/// <summary>Windows SDK UIAutomationCore.h 中本控制項需要的最小 COM 定義。</summary>
public static class UiaNative
{
    internal const int AppendRuntimeId = 3;
    internal const int NamePropertyId = 30005;
    internal const int ControlTypePropertyId = 30003;
    internal const int HasKeyboardFocusPropertyId = 30008;
    internal const int IsKeyboardFocusablePropertyId = 30009;
    internal const int IsEnabledPropertyId = 30010;
    internal const int AutomationIdPropertyId = 30011;
    internal const int HelpTextPropertyId = 30013;
    internal const int IsControlElementPropertyId = 30016;
    internal const int IsContentElementPropertyId = 30017;
    internal const int IsOffscreenPropertyId = 30022;
    internal const int PositionInSetPropertyId = 30152;
    internal const int SizeOfSetPropertyId = 30153;
    internal const int ListControlTypeId = 50008;
    internal const int ListItemControlTypeId = 50007;
    internal const int SelectionPatternId = 10001;
    internal const int SelectionItemPatternId = 10010;
    internal const int ScrollItemPatternId = 10017;
    internal const int ItemContainerPatternId = 10019;
    internal const int VirtualizedItemPatternId = 10020;

    [Flags]
    public enum ProviderOptions
    {
        ServerSideProvider = 1,
        UseComThreading = 32
    }

    public enum NavigateDirection
    {
        Parent,
        NextSibling,
        PreviousSibling,
        FirstChild,
        LastChild
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UiaRect(double left, double top, double width, double height)
    {
        public double Left = left;
        public double Top = top;
        public double Width = width;
        public double Height = height;
        internal bool IsEmpty => Width <= 0 || Height <= 0;
    }

    [ComVisible(true), Guid("d6dd68d1-86fd-4332-8666-9abedea2d24c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IRawElementProviderSimple
    {
        ProviderOptions get_ProviderOptions();
        [return: MarshalAs(UnmanagedType.IUnknown)] object? GetPatternProvider(int patternId);
        object? GetPropertyValue(int propertyId);
        IRawElementProviderSimple? get_HostRawElementProvider();
    }

    [ComVisible(true), Guid("f7063da8-8359-439c-9297-bbc5299a7d87"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IRawElementProviderFragment
    {
        IRawElementProviderFragment? Navigate(NavigateDirection direction);
        int[]? GetRuntimeId();
        UiaRect get_BoundingRectangle();
        IRawElementProviderSimple[]? GetEmbeddedFragmentRoots();
        void SetFocus();
        IRawElementProviderFragmentRoot get_FragmentRoot();
    }

    [ComVisible(true), Guid("620ce2a5-ab8f-40a9-86cb-de3c75599b58"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IRawElementProviderFragmentRoot
    {
        IRawElementProviderFragment? ElementProviderFromPoint(double x, double y);
        IRawElementProviderFragment? GetFocus();
    }

    [ComVisible(true), Guid("fb8b03af-3bdf-48d4-bd36-1a65793be168"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISelectionProvider
    {
        IRawElementProviderSimple[] GetSelection();
        [return: MarshalAs(UnmanagedType.I2)] bool get_CanSelectMultiple();
        [return: MarshalAs(UnmanagedType.I2)] bool get_IsSelectionRequired();
    }

    [ComVisible(true), Guid("2acad808-b2d4-452d-a407-91ff1ad167b2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISelectionItemProvider
    {
        void Select();
        void AddToSelection();
        void RemoveFromSelection();
        [return: MarshalAs(UnmanagedType.I2)] bool get_IsSelected();
        IRawElementProviderSimple get_SelectionContainer();
    }

    [ComVisible(true), Guid("e747770b-39ce-4382-ab30-d8fb3f336f24"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IItemContainerProvider
    {
        IRawElementProviderSimple? FindItemByProperty(
            IRawElementProviderSimple? startAfter,
            int propertyId,
            object? value);
    }

    [ComVisible(true), Guid("cb98b665-2d35-4fac-ad35-f3c60d0c0b8b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IVirtualizedItemProvider { void Realize(); }

    [ComVisible(true), Guid("2360c714-4bf1-4b26-ba65-9b21316127eb"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IScrollItemProvider { void ScrollIntoView(); }

    [DllImport("UIAutomationCore.dll")]
    internal static extern int UiaHostProviderFromHwnd(nint hwnd,
        [MarshalAs(UnmanagedType.Interface)] out IRawElementProviderSimple? provider);
}
