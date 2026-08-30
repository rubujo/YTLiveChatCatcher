using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using System.Text;

namespace YTLiveChatCatcher;

internal static class Program
{
    /// <summary>
    /// ServiceProvider
    /// </summary>
    private static ServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    internal static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        // 跟隨 Windows 系統設定自動切換深／淺色模式（.NET 10 起穩定 API，不再是實驗性功能）。
        // 僅 Windows 11 以上有效，Windows 10 會自動退回淺色；不會在應用程式執行期間跟著系統設定即時切換
        // （系統設定變更後需要重啟應用程式），也不是所有控制項都會跟著變（例如 MessageBox 固定是淺色）。
        Application.SetColorMode(SystemColorMode.System);

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // 讓 UI 執行緒（含 async void 事件處理常式、SynchronizationContext.Post 回呼）
        // 未處理的例外，統一導向 Application.ThreadException，而不是讓整個程序直接終止。
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += OnThreadException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        UpdateConfig();
        ConfigureServices();

        Application.Run((FMain)ServiceProvider?.GetService(typeof(FMain))!);
    }

    /// <summary>
    /// 處理 UI 執行緒（含訊息迴圈內的 async void 回呼）未攔截的例外
    /// </summary>
    private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
    {
        LogManager.GetCurrentClassLogger().Error(e.Exception, "未處理的 UI 執行緒例外。");

        MessageBox.Show(
            $"發生未預期的錯誤：{e.Exception.Message}",
            "YTLiveChatCatcher",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    /// <summary>
    /// 處理非 UI 執行緒（例如背景 Task 內未被攔截）的致命例外，至少記錄下來以利事後排查
    /// </summary>
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogManager.GetCurrentClassLogger().Fatal(ex, "未處理的非 UI 執行緒例外。");
        }
    }

    /// <summary>
    /// 設定服務
    /// <para>來源：https://docs.microsoft.com/zh-tw/archive/msdn-magazine/2019/may/net-core-3-0-create-a-centralized-pull-request-hub-with-winforms-in-net-core-3-0 </para>
    /// </summary>
    private static void ConfigureServices()
    {
        ServiceCollection services = new();

        services.AddHttpClient()
            .AddLogging(configure =>
            {
                LoggingConfiguration config = new();

                // Targets where to log to: File and Console.
                ConcurrentFileTarget logFile = new("logFile")
                {
                    FileName = Path.Combine(AppContext.BaseDirectory, @"Logs\log.txt"),
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                    ArchiveAboveSize = 8 * 1024 * 1024,
                    MaxArchiveFiles = 10,
                    MaxArchiveDays = 7,
                    LineEnding = LineEndingMode.CRLF,
                    Encoding = Encoding.UTF8,
                    WriteBom = false,
                    CreateDirs = true,
                    AutoFlush = true,
                    ConcurrentWrites = true,
                    EnableArchiveFileCompression = true
                };

                ConsoleTarget logConsole = new("logConsole");

                // Rules for mapping loggers to targets.          
                config.AddRule(minLevel: LogLevel.Debug, maxLevel: LogLevel.Fatal, target: logConsole);
                config.AddRule(minLevel: LogLevel.Debug, maxLevel: LogLevel.Fatal, target: logFile);

                // Apply config.      
                LogManager.Configuration = config;

                configure.AddNLog(config);
            })
            .AddSingleton<FMain>();

        ServiceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// 更新設定：偵測到新版本時，把使用者先前版本的 user.config 設定值搬移到新版本，
    /// 並清空這個一次性的搬移旗標，避免下次啟動重複執行。
    /// </summary>
    private static void UpdateConfig()
    {
        if (Properties.Settings.Default.UpdateSettings)
        {
            Properties.Settings.Default.Upgrade();
            Properties.Settings.Default.Reload();
            Properties.Settings.Default.UpdateSettings = false;
            Properties.Settings.Default.Save();
        }
    }
}