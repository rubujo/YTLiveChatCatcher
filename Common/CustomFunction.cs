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
using System.Text.RegularExpressions;

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
    /// <para>來源：https://stackoverflow.com/a/8626562</para>
    /// <para>原作者：Gary Kindel</para>
    /// <para>原授權：CC BY-SA 3.0</para>
    /// <para>CC BY-SA 3.0：https://creativecommons.org/licenses/by-sa/3.0/</para>
    /// </summary>
    /// <param name="filename">字串，檔案名稱</param>
    /// <param name="replaceChar">字串，替換無效字元的字元</param>
    /// <returns>字串</returns>
    public static string RemoveInvalidFilePathCharacters(string filename, string replaceChar)
    {
        string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

        Regex regex = new(string.Format("[{0}]", Regex.Escape(regexSearch)));

        return regex.Replace(filename, replaceChar);
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
    /// <para>來源 1：https://github.com/dotnet/runtime/issues/17938#issuecomment-235502080</para>
    /// <para>原作者：mellinoe</para>
    /// <para>來源 2：https://github.com/dotnet/runtime/issues/17938#issuecomment-249383422</para>
    /// <para>原作者：brockallen</para>
    /// </summary>
    /// <param name="url">字串，網址</param>
    public static void OpenBrowser(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
            url = url.Replace("&", "^&");

            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
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