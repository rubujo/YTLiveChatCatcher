using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Rubujo.YouTube.Utility.Extensions;
using YTLiveChatCatcher.Common.Utils;
using YTLiveChatCatcher.Extensions;

namespace YTLiveChatCatcher;

/// <summary>
/// 透過應用程式專屬的 WebView2 視窗登入 YouTube／Google 帳號，取得聊天室擷取用的 Cookie
/// <para>WebView2 使用自己專屬的 user data folder，不會讀取或碰觸使用者既有的 Edge／Chrome 瀏覽器資料。</para>
/// </summary>
public partial class FCookieLogin : Form
{
    private readonly FMain _FMain;

    /// <summary>
    /// 使用者確認後取得的 Cookie 字串（未確認則為 null）
    /// </summary>
    public string? ResultCookies { get; private set; }

    public FCookieLogin(FMain fmain)
    {
        InitializeComponent();

        Icon = Properties.Resources.app_icon;
        Text = $"登入 YouTube 帳號 - {fmain.Text}";

        _FMain = fmain;
    }

    private async void FCookieLogin_Load(object sender, EventArgs e)
    {
        try
        {
            CBRememberCookie.Checked = SecureCookieStore.Exists();

            UpdateStatus(SecureCookieStore.Exists() ?
                "已載入先前記住的登入資料，登入完成後按下「使用以上登入內容」以更新。" :
                "尚未登入，請於上方頁面登入您的 YouTube／Google 帳號。");

            // 使用應用程式自己專屬的 user data folder，
            // 刻意不指向使用者既有的 Edge／Chrome profile 路徑，避免碰到使用者日常瀏覽器的資料。
            string profileDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "YTLiveChatCatcher",
                "WebView2Profile");

            CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(userDataFolder: profileDirectory);

            await WebViewLogin.EnsureCoreWebView2Async(environment);

            WebViewLogin.CoreWebView2.Navigate(
                "https://accounts.google.com/ServiceLogin?service=youtube&continue=https://www.youtube.com/");
        }
        catch (WebView2RuntimeNotFoundException)
        {
            MessageBox.Show(
                "找不到 WebView2 執行階段（Runtime），請先安裝後再試一次。" + Environment.NewLine +
                "下載連結：https://developer.microsoft.com/microsoft-edge/webview2/",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            DialogResult = DialogResult.Cancel;

            Close();
        }
        catch (Exception ex)
        {
            _FMain.GetSharedLogger().LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void FCookieLogin_FormClosing(object sender, FormClosingEventArgs e)
    {
        // 確保 WebView2 底層資源被正確釋放，不影響下次開啟。
        WebViewLogin.Dispose();
    }

    private async void BtnConfirm_Click(object sender, EventArgs e)
    {
        try
        {
            string cookies;

            if (!string.IsNullOrWhiteSpace(TBManualCookie.Text))
            {
                cookies = TBManualCookie.Text.Trim();
            }
            else
            {
                if (WebViewLogin.CoreWebView2 == null)
                {
                    MessageBox.Show(
                        "瀏覽器元件尚未就緒，請稍後再試。",
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                IReadOnlyList<CoreWebView2Cookie> webViewCookies = await WebViewLogin.CoreWebView2.CookieManager
                    .GetCookiesAsync("https://www.youtube.com/");

                if (webViewCookies.Count == 0)
                {
                    MessageBox.Show(
                        "尚未偵測到登入的 Cookie，請先在上方完成登入，或改用下方手動貼上。",
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                cookies = string.Join(";", webViewCookies.Select(n => $"{n.Name}={n.Value}"));
            }

            if (CBRememberCookie.Checked)
            {
                SecureCookieStore.Save(cookies);
            }
            else
            {
                SecureCookieStore.Clear();
            }

            ResultCookies = cookies;

            DialogResult = DialogResult.OK;

            Close();
        }
        catch (Exception ex)
        {
            _FMain.GetSharedLogger().LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void BtnLogout_Click(object sender, EventArgs e)
    {
        try
        {
            WebViewLogin.CoreWebView2?.CookieManager.DeleteAllCookies();

            SecureCookieStore.Clear();

            TBManualCookie.InvokeIfRequired(TBManualCookie.Clear);

            CBRememberCookie.InvokeIfRequired(() => CBRememberCookie.Checked = false);

            ResultCookies = string.Empty;

            UpdateStatus("已清除登入資料。");

            MessageBox.Show(
                "已清除登入資料，包含本機記住的 Cookie。",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _FMain.GetSharedLogger().LogError("{ErrorMessage}", ex.GetExceptionMessage());

            MessageBox.Show(
                $"發生錯誤：{ex.GetExceptionMessage()}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;

        Close();
    }

    private void UpdateStatus(string message)
    {
        LStatus.InvokeIfRequired(() =>
        {
            LStatus.Text = message;
        });
    }
}
