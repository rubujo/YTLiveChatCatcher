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
    /// WebView2 專屬 user data folder 的完整路徑，在 FCookieLogin_Load 建立 WebView2 環境時設定，
    /// 供 BtnLogout_Click／FCookieLogin_FormClosing 需要清除整個 profile 目錄時使用。
    /// </summary>
    private string? _webView2ProfileDirectory;

    /// <summary>
    /// 使用者是否在這次開啟視窗期間按過「登出／清除已儲存資料」，設為 true 時會在
    /// FCookieLogin_FormClosing（WebViewLogin.Dispose() 之後，檔案控制代碼才會釋放）
    /// 嘗試清除整個 WebView2 profile 目錄，不只清 Cookie。
    /// </summary>
    private bool _pendingProfileCleanup;

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
            _webView2ProfileDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "YTLiveChatCatcher",
                "WebView2Profile");

            CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(userDataFolder: _webView2ProfileDirectory);

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

        // 2026/9 修正：BtnLogout_Click 原本只呼叫 CookieManager.DeleteAllCookies()，
        // %LocalAppData%\YTLiveChatCatcher\WebView2Profile 目錄本身（cache、localStorage、
        // IndexedDB 等，其中可能還留有 Google 登入相關的其他狀態）從未被刪除，且每次登入都會
        // 持續累積，跟使用者直覺認知的「登出＝清乾淨」有落差。這裡等 WebViewLogin.Dispose()
        // 釋放檔案控制代價之後才嘗試刪除，是 best-effort：檔案系統層面的清理失敗（例如系統
        // 尚未完全釋放某個鎖）不應該讓關閉視窗這個動作本身失敗，靜默記錄即可。
        if (_pendingProfileCleanup && !string.IsNullOrEmpty(_webView2ProfileDirectory))
        {
            try
            {
                if (Directory.Exists(_webView2ProfileDirectory))
                {
                    Directory.Delete(_webView2ProfileDirectory, recursive: true);
                }
            }
            catch (Exception ex)
            {
                _FMain.GetSharedLogger().LogError("{ErrorMessage}", ex.GetExceptionMessage());
            }
        }
    }

    private async void BtnConfirm_Click(object sender, EventArgs e)
    {
        try
        {
            string cookies;

            if (!string.IsNullOrWhiteSpace(TBManualCookie.Text))
            {
                // 2026/9 修正：手動貼上的 Cookie 字串原本完全沒有格式驗證，只要含有換行字元
                // （從瀏覽器開發人員工具或筆記軟體複製時很常見）就會在下游組 HTTP 標頭時
                // （YTJsonParser.YouTubeAuth.cs 的 SetHttpRequestMessageHeader）讓 .NET 的
                // HttpHeaders.Add 直接拋出 FormatException，而該例外的 Message 會把整段 Cookie
                // 原文回顯出來——這條例外路徑不在程式原本設計好的 [REDACTED] 標頭遮蔽機制保護
                // 範圍內，會讓 Cookie 明碼外洩進 Logs/log.txt 跟畫面上的記錄框。與其讓這個問題在
                // 下游才被動出現，這裡先移除貼上內容中的所有控制字元（換行、Tab、NUL 等），
                // 一般合法的 Cookie 內容不會用到控制字元，清理後不影響正常使用情境。
                string sanitizedCookies = new(
                    [.. TBManualCookie.Text.Trim().Where(c => !char.IsControl(c))]);

                if (string.IsNullOrEmpty(sanitizedCookies))
                {
                    MessageBox.Show(
                        "貼上的 Cookie 字串格式無效，請重新從瀏覽器複製。",
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                cookies = sanitizedCookies;
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

            // 標記讓 FCookieLogin_FormClosing 在 WebViewLogin.Dispose() 釋放檔案控制代碼之後，
            // 一併清除整個 WebView2 profile 目錄，不只清 Cookie。
            _pendingProfileCleanup = true;

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
