using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using System.Text;

namespace YTLiveChatCatcher;

internal static class Program
{
    private static IServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    internal static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        UpdateConfig();
        ConfigureServices();

        Application.Run((FMain)ServiceProvider?.GetService(typeof(FMain))!);
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
                FileTarget logFile = new("logFile")
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
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, logConsole);
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, logFile);

                // Apply config.      
                LogManager.Configuration = config;

                configure.AddNLog(config);
            })
            .AddSingleton<FMain>();

        ServiceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// 更新設定
    /// <para>來源：https://stackoverflow.com/a/23924277</para>
    /// <para>原作者：Grant</para>
    /// <para>原授權：CC BY-SA 3.0</para>
    /// <para>CC BY-SA 3.0：https://creativecommons.org/licenses/by-sa/3.0/</para>
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