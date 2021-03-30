using AngleSharp.Dom;
using AngleSharp;
using Microsoft.Extensions.Logging;
using NLog;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using OfficeOpenXml.Drawing;
using System.Drawing.Imaging;

namespace YTLiveChatCatcher.Common;

/// <summary>
/// 自定義功能
/// </summary>
public class CustomFunction
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

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
    /// <para>來源：https://stackoverflow.com/a/8626562 </para>
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
    /// 取得 HttpClient
    /// </summary>
    /// <param name="httpClientFactory">IHttpClientFactory</param>
    /// <param name="logger">Microsoft.Extensions.Logging.ILogger</param>
    /// <param name="userAgent">字串，使用者代理字串</param>
    /// <returns>HttpClient</returns>
    public static HttpClient GetHttpClient(
        IHttpClientFactory httpClientFactory,
        Microsoft.Extensions.Logging.ILogger logger,
        string userAgent = "")
    {
        HttpClient outputHttpClient = httpClientFactory.CreateClient();

        if (!string.IsNullOrEmpty(userAgent))
        {
            bool result = SetUserAgent(outputHttpClient, userAgent);

            if (result)
            {
                logger.LogInformation("本次連線使用的使用者代理字串：{UserAgent}", userAgent);
            }

        }

        return outputHttpClient;
    }

    /// <summary>
    /// 為 HttpClient 設定指定的使用者代理字串
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="userAgent">字串，使用者代理字串</param>
    /// <returns>布林值</returns>
    public static bool SetUserAgent(HttpClient httpClient, string userAgent)
    {
        bool result = httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);

        if (result)
        {
            httpClient.DefaultRequestHeaders.UserAgent.Clear();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        }

        return result;
    }

    /// <summary>
    /// 檢查使用者代理字串
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="userAgent">字串，使用者代理字串</param>
    /// <returns>布林值</returns>
    public static bool CheckUserAgent(HttpClient httpClient, string userAgent)
    {
        return httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
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
    /// <para>參考 1：https://github.com/dotnet/runtime/issues/17938#issuecomment-235502080 </para>
    /// <para>參考 2：https://github.com/dotnet/runtime/issues/17938#issuecomment-249383422 </para>
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
    /// 從 YouTube 頻道自定義網址取得頻道 ID
    /// </summary>
    /// <param name="ytChannelCustomUrl">字串，YouTube 頻道自定義網址</param>
    /// <returns>字串</returns>
    public static async Task<string?> GetYtChannelIdByYtChannelCustomUrl(string ytChannelCustomUrl)
    {
        string? ytChannelId = string.Empty;

        IConfiguration configuration = Configuration.Default.WithDefaultLoader();
        IBrowsingContext browsingContext = BrowsingContext.New(configuration);
        IDocument document = await browsingContext.OpenAsync(ytChannelCustomUrl);
        IElement? element = document?.Body?.Children
            .FirstOrDefault(n => n.LocalName == "meta" &&
                n.GetAttribute("property") == "og:url");

        if (element != null)
        {
            ytChannelId = element.GetAttribute("content");
        }

        if (!string.IsNullOrEmpty(ytChannelId))
        {
            ytChannelId = ytChannelId.Replace("https://www.youtube.com/channel/", string.Empty);
        }

        return ytChannelId;
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