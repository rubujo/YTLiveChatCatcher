using NLog;
using OfficeOpenXml.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Speech.Synthesis;
using System.Text;

namespace YTLiveChatCatcher.Common;

/// <summary>
/// 自定義功能
/// </summary>
public class CustomFunction
{
    /// <summary>
    /// NLog 的 Logger
    /// </summary>
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 取得應用程式的版本號
    /// </summary>
    /// <returns>字串</returns>
    public static string GetAppVersion()
    {
        Version? version = Assembly.GetExecutingAssembly().GetName().Version;

        return version != null ? $"v{version}" : string.Empty;
    }

    /// <summary>
    /// 使用 SpeechSynthesizer 說話
    /// </summary>
    /// <param name="value">字串</param>
    [SupportedOSPlatform("windows7.0")]
    public static void SpeechText(string value)
    {
        if (OperatingSystem.IsWindows())
        {
            if (Properties.Settings.Default.EnableTTS)
            {
                Task.Run(() =>
                {
                    CultureInfo cultureInfo = new("zh-TW", false);

                    SpeechSynthesizer speechSynthesizer = new();

                    InstalledVoice? installedVoice = speechSynthesizer
                        .GetInstalledVoices()
                        .FirstOrDefault(n => n.VoiceInfo.Culture.DisplayName == cultureInfo.DisplayName);

                    if (installedVoice != null)
                    {
                        speechSynthesizer.SelectVoice(installedVoice.VoiceInfo.Name);
                    }

                    speechSynthesizer.Speak(value);
                });
            }
        }
    }

    /// <summary>
    /// 移除檔案路徑中的無效字元
    /// </summary>
    /// <param name="filename">字串，檔案名稱</param>
    /// <param name="replaceChar">字串，替換無效字元的字元</param>
    /// <returns>字串</returns>
    public static string RemoveInvalidFilePathCharacters(string filename, string replaceChar)
    {
        HashSet<char> invalidChars = [.. Path.GetInvalidFileNameChars(), .. Path.GetInvalidPathChars()];

        StringBuilder builder = new(filename.Length);

        foreach (char c in filename)
        {
            if (invalidChars.Contains(c))
            {
                builder.Append(replaceChar);
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// 取得隨機間隔值（毫秒）
    /// </summary>
    /// <returns>數值，3000 ~ 10000</returns>
    public static int GetRandomInterval()
    {
        return RandomNumberGenerator.GetInt32(3, 10) * 1000;
    }

    /// <summary>
    /// 開啟網頁瀏覽器
    /// </summary>
    /// <param name="url">字串，網址</param>
    public static void OpenBrowser(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // 2026/9 修正：原本透過 cmd /c start {url} 開瀏覽器，只跳脫了 &，其餘 cmd.exe 特殊字元
            // （例如 |、"）沒有處理，且 url 沒有加上引號——只要 url 內容帶有這類字元就可能被解讀成
            // 額外的指令。這裡的 url 不保證一定是使用者自己輸入的（也可能來自檢查更新抓回來的遠端
            // JSON 內的下載網址，或聊天訊息解析出的頻道 ID），不能假設一定安全。改用
            // ProcessStartInfo(url) { UseShellExecute = true } 直接呼叫 Windows Shell 開啟預設瀏覽器，
            // 完全不經過 cmd.exe，從根本上排除這個注入面。
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
        else
        {
            _logger.Debug("不支援的作業系統。");
        }
    }

    /// <summary>
    /// 取得對應的 ePictureType
    /// </summary>
    /// <param name="value">字串，ImageFormat 的 .ToString()</param>
    /// <returns>ePictureType</returns>
    public static ePictureType GetEPictureType(string value)
    {
        return value switch
        {
            nameof(ImageFormat.Jpeg) => ePictureType.Jpg,
            nameof(ImageFormat.Png) => ePictureType.Png,
            nameof(ImageFormat.Bmp) => ePictureType.Bmp,
            nameof(ImageFormat.Gif) => ePictureType.Gif,
            nameof(ImageFormat.Emf) => ePictureType.Emf,
            nameof(ImageFormat.Icon) => ePictureType.Ico,
            nameof(ImageFormat.Tiff) => ePictureType.Tif,
            nameof(ImageFormat.MemoryBmp) => ePictureType.Bmp,
            nameof(ImageFormat.Wmf) => ePictureType.Wmf,
            _ => ePictureType.Png,
        };
    }
}