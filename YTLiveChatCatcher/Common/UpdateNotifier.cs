using Rubujo.YouTube.Utility.Extensions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YTLiveChatCatcher.Common;

/// <summary>
/// 更新通知器
/// </summary>
public class UpdateNotifier
{
    /// <summary>
    /// AppVersion.json 檔案的網址
    /// </summary>
    private static readonly string Url = "https://drive.google.com/uc?id=1cCrvyBTvtDKqK4rqq1wjizqnpTogRN70";

    /// <summary>
    /// 檢查版本
    /// </summary>
    /// <returns>Task&lt;CheckResult&gt;</returns>
    public static async Task<CheckResult> CheckVersion(HttpClient httpClient)
    {
        try
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = assembly.GetName();
            Version? localVersion = assemblyName.Version;

            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(Url);

            httpResponseMessage.EnsureSuccessStatusCode();

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return new CheckResult()
                {
                    IsException = true,
                    MessageText = $"發生錯誤，HTTP 狀態碼 {httpResponseMessage.StatusCode}"
                };
            }

            string textContent = await httpResponseMessage.Content.ReadAsStringAsync();

            List<AppData>? dataSet = JsonSerializer.Deserialize<List<AppData>>(textContent);

            if (dataSet == null)
            {
                return new CheckResult()
                {
                    IsException = true,
                    MessageText = "發生錯誤，無法解析 AppVersion.json 檔案。"
                };
            }

            AppData? appData = dataSet.FirstOrDefault(n => n.App == assemblyName.Name);

            if (appData == null)
            {
                return new CheckResult()
                {
                    IsException = true,
                    MessageText = $"發生錯誤，找不到 {assemblyName.Name} 應用程式的資料。"
                };
            };

            if (appData.AppVersion == null)
            {
                return new CheckResult()
                {
                    IsException = true,
                    MessageText = $"發生錯誤，無法解析 {assemblyName.Name} 應用程式的版本號資料。"
                };
            }

            Version? netVersion = Version.Parse(appData.AppVersion);

            CheckResult checkResult = new();

            int compareResult = netVersion.CompareTo(localVersion);

            if (compareResult > 0)
            {
                checkResult.HasNewVersion = true;
                checkResult.NetVersionIsOdler = false;
                checkResult.IsException = false;
                checkResult.MessageText = $"有新版本 v{appData.AppVersion} 可供下載。";
                checkResult.VersionText = appData.AppVersion;
                checkResult.Checksum = appData.Checksum;
                checkResult.DownloadUrl = appData.DownloadUrl;
            }
            else if (compareResult == 0)
            {
                checkResult.HasNewVersion = false;
                checkResult.NetVersionIsOdler = false;
                checkResult.IsException = false;
                checkResult.MessageText = $"您使用的是最新版本 v{localVersion}。";
                checkResult.VersionText = appData.AppVersion;
                checkResult.Checksum = appData.Checksum;
                checkResult.DownloadUrl = appData.DownloadUrl;
            }
            else
            {
                checkResult.HasNewVersion = false;
                checkResult.NetVersionIsOdler = true;
                checkResult.IsException = false;
                checkResult.MessageText = $"注意！網路的版本（v{netVersion}）比本機的版本（v{localVersion}）還要舊，請注意是否有降版的公告資訊。";
                checkResult.VersionText = appData.AppVersion;
                checkResult.Checksum = appData.Checksum;
                checkResult.DownloadUrl = appData.DownloadUrl;
            }

            return checkResult;
        }
        catch (Exception ex)
        {
            return new CheckResult()
            {
                IsException = true,
                MessageText = ex.GetExceptionMessage()
            };
        }
    }

    /// <summary>
    /// 檢查結果
    /// </summary>
    public class CheckResult
    {
        /// <summary>
        /// 訊息文字
        /// </summary>
        public string? MessageText { get; set; }

        /// <summary>
        /// 版本號文字
        /// </summary>
        public string? VersionText { get; set; }

        /// <summary>
        /// 下載網址
        /// </summary>
        public string? DownloadUrl { get; set; }

        /// <summary>
        /// 是否有新版本
        /// </summary>
        public bool HasNewVersion { get; set; }

        /// <summary>
        /// 是否為例外
        /// </summary>
        public bool IsException { get; set; }

        /// <summary>
        /// 網路版本是否比本基版本還要舊
        /// </summary>
        public bool NetVersionIsOdler { get; set; }

        /// <summary>
        /// 校驗碼
        /// </summary>
        public string? Checksum { get; set; }
    }

    /// <summary>
    /// 應用程式資料
    /// </summary>
    public class AppData
    {
        /// <summary>
        /// 應用程式
        /// </summary>
        [JsonPropertyName("app")]
        public string? App { get; set; }

        /// <summary>
        /// 應用程式名稱
        /// </summary>
        [JsonPropertyName("appName")]
        public string? AppName { get; set; }

        /// <summary>
        /// 應用程式版本號
        /// </summary>
        [JsonPropertyName("appVersion")]
        public string? AppVersion { get; set; }

        /// <summary>
        /// 建置日期
        /// </summary>
        [JsonPropertyName("buildDate")]
        public string? BuildDate { get; set; }

        /// <summary>
        /// 校驗碼
        /// </summary>
        [JsonPropertyName("checksum")]
        public string? Checksum { get; set; }

        /// <summary>
        /// 下載網址
        /// </summary>
        [JsonPropertyName("downloadUrl")]
        public string? DownloadUrl { get; set; }
    }
}