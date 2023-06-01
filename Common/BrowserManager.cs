using Microsoft.Data.Sqlite;
using NLog;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace YTLiveChatCatcher.Common;

/// <summary>
/// BrowserManager
/// <para>參考：https://stackoverflow.com/a/68703365 </para>
/// </summary>
public class BrowserManager
{
    /// <summary>
    /// NLog 的 Logger
    /// </summary>
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 網頁瀏覽器
    /// </summary>
    public enum Browser
    {
        /// <summary>
        /// Brave
        /// </summary>
        Brave = 1,
        /// <summary>
        /// Google Chrome
        /// </summary>
        GoogleChrome = 2,
        /// <summary>
        /// Chromium
        /// </summary>
        Chromium = 3,
        /// <summary>
        /// Microsoft Edge
        /// </summary>
        MicrosoftEdge = 4,
        /// <summary>
        /// Opera
        /// </summary>
        Opera = 5,
        /// <summary>
        /// Opera GX
        /// </summary>
        OperaGX = 6,
        /// <summary>
        /// Vivaldi
        /// </summary>
        Vivaldi = 7,
        /// <summary>
        /// Mozilla Firefox
        /// </summary>
        MozillaFirefox = 8
    };

    /// <summary>
    /// 取得 Cookies
    /// </summary>
    /// <param name="browser">Browser</param>
    /// <param name="profileName">字串，設定檔名稱</param>
    /// <param name="hostKey">字串，主機鍵值</param>
    /// <returns>List&lt;Cookie&gt;</returns>
    public static List<Cookie> GetCookies(
        Browser browser,
        string profileName,
        string hostKey)
    {
        bool isCustomProfilePath = false;

        // 判斷是否為自定義設定檔路徑。
        if (Path.IsPathRooted(profileName))
        {
            isCustomProfilePath = true;
        }

        List<Cookie> outputData = new();

        string cookieFilePath = string.Empty;

        if (browser == Browser.MozillaFirefox)
        {
            cookieFilePath = isCustomProfilePath ?
                profileName :
                Path.Combine(
                    $@"C:\Users\{Environment.UserName}\AppData\Roaming\",
                    GetPartialPath(browser));

            DirectoryInfo directoryInfo = new(cookieFilePath);

            if (directoryInfo.Exists)
            {
                string portableProfilePath = Path.Combine(cookieFilePath, "cookies.sqlite");

                if (File.Exists(portableProfilePath))
                {
                    cookieFilePath = portableProfilePath;
                }
                else
                {
                    DirectoryInfo[] diDirectories = directoryInfo.GetDirectories();
                    DirectoryInfo? diTargetDirectory = diDirectories.FirstOrDefault(n => n.Name == profileName);

                    // 當 diTargetDirectory 為 null 時，則取 diTargetDirectory 第一個資料夾。
                    diTargetDirectory ??= diDirectories.FirstOrDefault();

                    // 理論上 diTargetDirectory 不應該為 null。
                    cookieFilePath = Path.Combine(
                        cookieFilePath,
                        $@"{diTargetDirectory?.Name}\cookies.sqlite");
                }
            }
        }
        else
        {
            if (string.IsNullOrEmpty(profileName))
            {
                profileName = "Default";
            }

            cookieFilePath = isCustomProfilePath ?
                Path.Combine(profileName, @"Network\Cookies") :
                Path.Combine(
                    $@"C:\Users\{Environment.UserName}\AppData\Local\{GetPartialPath(browser)}\User Data",
                    $@"{profileName}\Network\Cookies");
        }

        _logger.Debug("Cookie 檔案的路徑：{Path}", cookieFilePath);

        if (File.Exists(cookieFilePath))
        {
            outputData = QuerySQLiteDB(browser, cookieFilePath, hostKey);
        }

        return outputData;
    }

    /// <summary>
    /// 取得部分路徑
    /// </summary>
    /// <param name="browser">Browser</param>
    /// <returns>字串</returns>
    public static string GetPartialPath(Browser browser)
    {
        return browser switch
        {
            Browser.Brave => @"BraveSoftware\Brave-Browser",
            Browser.GoogleChrome => @"Google\Chrome",
            Browser.Chromium => @"Chromium",
            Browser.MicrosoftEdge => @"Microsoft\Edge",
            Browser.Opera => @"Opera Software\Opera Stable",
            Browser.OperaGX => @"Opera Software\Opera GX Stable",
            Browser.Vivaldi => "Vivaldi",
            Browser.MozillaFirefox => @"Mozilla\Firefox\Profiles",
            _ => @"Google\Chrome"
        };
    }

    /// <summary>
    /// 查詢 SQLite 資料庫
    /// </summary>
    /// <param name="browser">Browser</param>
    /// <param name="cookieFilePath">字串，Cookie 檔案的位置</param>
    /// <param name="hostKey">字串，主機鍵值</param>
    /// <returns>List&lt;Cookie&gt;</returns>
    private static List<Cookie> QuerySQLiteDB(
        Browser browser,
        string cookieFilePath,
        string hostKey)
    {
        List<Cookie> outputData = new();

        try
        {
            using SqliteConnection sqliteConnection = new($"Data Source={cookieFilePath}");
            using SqliteCommand sqliteCommand = sqliteConnection.CreateCommand();

            string rawTSQL = browser switch
            {
                Browser.MozillaFirefox => "SELECT [name], [value], [host] FROM [moz_cookies]",
                _ => "SELECT [name], [encrypted_value], [host_key] FROM [cookies]"
            };
            string rawWhereClauseTSQL = browser switch
            {
                Browser.MozillaFirefox => $" WHERE [host] = '{hostKey}'",
                //Browser.MozillaFirefox =>  $" WHERE [host] = LIKE '%{hostKey}%'",
                _ => $" WHERE [host_key] = '{hostKey}'"
                //_ => $" WHERE [host_key] LIKE '%{hostKey}%'"
            };

            if (!string.IsNullOrEmpty(hostKey))
            {
                rawTSQL += rawWhereClauseTSQL;
            }

            sqliteCommand.CommandText = rawTSQL;

            sqliteConnection.Open();

            using (SqliteDataReader sqliteDataReader = sqliteCommand.ExecuteReader())
            {
                byte[] key = browser switch
                {
                    Browser.MozillaFirefox => Array.Empty<byte>(),
                    _ => AesGcm256.GetKey(browser)
                };

                while (sqliteDataReader.Read())
                {
                    if (!outputData.Any(a => a.Name == sqliteDataReader.GetString(0)))
                    {
                        string value = string.Empty;

                        switch (browser)
                        {
                            case Browser.MozillaFirefox:
                                value = sqliteDataReader.GetString(1);

                                break;
                            default:
                                byte[] encryptedData = GetBytes(sqliteDataReader, 1);
                                byte[] nonce, ciphertextTag;

                                AesGcm256.Prepare(encryptedData, out nonce, out ciphertextTag);

                                value = AesGcm256.Decrypt(ciphertextTag, key, nonce);

                                break;
                        }

                        outputData.Add(new Cookie()
                        {
                            HostKey = sqliteDataReader.GetString(2),
                            Name = sqliteDataReader.GetString(0),
                            Value = value
                        });
                    }
                }
            }

            sqliteConnection.Close();
        }
        catch (Exception ex)
        {
            _logger.Debug(ex.ToString());
        }

        return outputData;
    }

    /// <summary>
    /// 取得 byte[]
    /// </summary>
    /// <param name="sqliteDataReader">SqliteDataReader</param>
    /// <param name="columnIndex">數值</param>
    /// <returns>byte[]</returns>
    private static byte[] GetBytes(SqliteDataReader sqliteDataReader, int columnIndex)
    {
        const int CHUNK_SIZE = 2 * 1024;

        byte[] buffer = new byte[CHUNK_SIZE];

        long bytesRead;
        long fieldOffset = 0;

        using MemoryStream memoryStream = new();

        while ((bytesRead = sqliteDataReader.GetBytes(columnIndex, fieldOffset, buffer, 0, buffer.Length)) > 0)
        {
            memoryStream.Write(buffer, 0, (int)bytesRead);

            fieldOffset += bytesRead;
        }

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Cookie 類別
    /// </summary>
    public class Cookie
    {
        /// <summary>
        /// 名稱
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 主機鍵值
        /// </summary>
        public string? HostKey { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string? Value { get; set; }
    }

    /// <summary>
    /// AesGcm256
    /// </summary>
    public class AesGcm256
    {
        /// <summary>
        /// 取得金鑰
        /// </summary>
        /// <param name="browser">Browser</param>
        /// <returns>字串</returns>
        public static byte[] GetKey(Browser browser)
        {
            //string sR = string.Empty;
            //string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string path = $@"C:\Users\{Environment.UserName}\AppData\Local\{GetPartialPath(browser)}\User Data\Local State";
            string v = File.ReadAllText(path);

            JsonElement json = JsonSerializer.Deserialize<JsonElement>(v);

            string key = json.GetProperty("os_crypt")
                .GetProperty("encrypted_key")
                .GetString() ?? string.Empty;

            byte[] src = Convert.FromBase64String(key!);
            byte[] encryptedKey = src.Skip(5).ToArray();
            byte[] decryptedKey = ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.CurrentUser);

            return decryptedKey;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="encryptedBytes">byte[]</param>
        /// <param name="key">byte[]</param>
        /// <param name="iv">byte[]</param>
        /// <returns>字串</returns>
        public static string Decrypt(byte[] encryptedBytes, byte[] key, byte[] iv)
        {
            string sR = string.Empty;

            try
            {
                GcmBlockCipher cipher = new(new AesEngine());
                AeadParameters parameters = new(new KeyParameter(key), 128, iv, null);

                cipher.Init(false, parameters);

                byte[] plainBytes = new byte[cipher.GetOutputSize(encryptedBytes.Length)];

                int retLen = cipher.ProcessBytes(encryptedBytes, 0, encryptedBytes.Length, plainBytes, 0);

                cipher.DoFinal(plainBytes, retLen);

                sR = Encoding.UTF8.GetString(plainBytes).TrimEnd("\r\n\0".ToCharArray());
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.ToString());
            }

            return sR;
        }

        /// <summary>
        /// 準備
        /// </summary>
        /// <param name="encryptedData">byte[]</param>
        /// <param name="nonce">byte[]</param>
        /// <param name="ciphertextTag">byte[]</param>
        public static void Prepare(byte[] encryptedData, out byte[] nonce, out byte[] ciphertextTag)
        {
            nonce = new byte[12];
            ciphertextTag = new byte[encryptedData.Length - 3 - nonce.Length];

            Array.Copy(encryptedData, 3, nonce, 0, nonce.Length);
            Array.Copy(encryptedData, 3 + nonce.Length, ciphertextTag, 0, ciphertextTag.Length);
        }
    }
}