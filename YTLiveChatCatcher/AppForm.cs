namespace YTLiveChatCatcher;

/// <summary>統一套用應用程式識別圖示的視窗基底類別。</summary>
public abstract class AppForm : Form
{
    protected AppForm()
    {
        Icon = Properties.Resources.app_icon;
    }
}
