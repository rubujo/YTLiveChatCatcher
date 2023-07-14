﻿using Microsoft.Data.Sqlite;
using NLog;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>
/// 網頁瀏覽器工具
/// <para>來源：https://stackoverflow.com/a/68703365</para>
/// <para>原作者：Flint Charles</para>
/// <para>原授權：CC BY-SA 4.0</para>
/// <para>CC BY-SA 4.0：https://creativecommons.org/licenses/by-sa/4.0/</para>
/// </summary>
public class WebBrowserUtil
{
    /// <summary>
    /// NLog 的 Logger
    /// </summary>
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 列舉：網頁瀏覽器類型
    /// </summary>
    public enum BrowserType
    {
        /// <summary>
        /// Brave
        /// </summary>
        Brave = 1,
        /// <summary>
        /// Brave Beta
        /// </summary>
        BraveBeta = 2,
        /// <summary>
        /// Brave Nightly
        /// </summary>
        BraveNightly = 3,
        /// <summary>
        /// Google Chrome
        /// </summary>
        GoogleChrome = 4,
        /// <summary>
        /// Google Chrome Beta
        /// </summary>
        GoogleChromeBeta = 5,
        /// <summary>
        /// Google Chrome Canary
        /// </summary>
        GoogleChromeCanary = 6,
        /// <summary>
        /// Chromium
        /// </summary>
        Chromium = 7,
        /// <summary>
        /// Microsoft Edge
        /// </summary>
        MicrosoftEdge = 8,
        /// <summary>
        /// Microsoft Edge Insider Beta
        /// </summary>
        MicrosoftEdgeInsiderBeta = 9,
        /// <summary>
        /// Microsoft Edge Insider Dev
        /// </summary>
        MicrosoftEdgeInsiderDev = 10,
        /// <summary>
        /// Microsoft Edge Insider Canary
        /// </summary>
        MicrosoftEdgeInsiderCanary = 11,
        /// <summary>
        /// Opera
        /// </summary>
        Opera = 12,
        /// <summary>
        /// Opera Beta
        /// </summary>
        OperaBeta = 13,
        /// <summary>
        /// Opera Developer
        /// </summary>
        OperaDeveloper = 14,
        /// <summary>
        /// Opera GX
        /// </summary>
        OperaGX = 15,
        /// <summary>
        /// Opera Crypto
        /// </summary>
        OperaCrypto = 16,
        /// <summary>
        /// Vivaldi
        /// <para>※各版本共用同一資料夾</para>
        /// </summary>
        Vivaldi = 17,
        /// <summary>
        /// Mozilla Firefox
        /// <para>※各版本共用上層資料夾</para>
        /// </summary>
        MozillaFirefox = 18
    };

    /// <summary>
    /// 取得 Cookies
    /// </summary>
    /// <param name="browserType">BrowserType</param>
    /// <param name="profileName">字串，設定檔名稱</param>
    /// <param name="hostKey">字串，主機鍵值</param>
    /// <returns>List&lt;Cookie&gt;</returns>
    public static List<CookieData> GetCookies(
        BrowserType browserType,
        string profileName,
        string hostKey)
    {
        bool isCustomProfilePath = false;

        // 判斷是否為自定義設定檔路徑。
        if (Path.IsPathRooted(profileName))
        {
            isCustomProfilePath = true;
        }

        List<CookieData> outputData = new();

        string cookieFilePath = string.Empty;

        if (browserType == BrowserType.MozillaFirefox)
        {
            cookieFilePath = isCustomProfilePath ?
                profileName :
                Path.Combine(
                    $@"C:\Users\{Environment.UserName}\AppData\Roaming\",
                    GetPartialPath(browserType));

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
                    $@"C:\Users\{Environment.UserName}\AppData\Local\{GetPartialPath(browserType)}\User Data",
                    $@"{profileName}\Network\Cookies");
        }

        _logger.Debug("Cookie 檔案的路徑：{Path}", cookieFilePath);

        if (File.Exists(cookieFilePath))
        {
            outputData = QuerySQLiteDB(browserType, cookieFilePath, hostKey);
        }

        return outputData;
    }

    /// <summary>
    /// 取得部分路徑
    /// </summary>
    /// <param name="browserType">BrowserType</param>
    /// <returns>字串</returns>
    public static string GetPartialPath(BrowserType browserType)
    {
        return browserType switch
        {
            BrowserType.Brave => @"BraveSoftware\Brave-Browser",
            BrowserType.BraveBeta => @"BraveSoftware\Brave-Browser-Beta",
            BrowserType.BraveNightly => @"BraveSoftware\Brave-Browser-Nightly",
            BrowserType.GoogleChrome => @"Google\Chrome",
            BrowserType.GoogleChromeBeta => @"Google\Chrome Beta",
            BrowserType.GoogleChromeCanary => @"Google\Chrome SxS",
            BrowserType.Chromium => @"Chromium",
            BrowserType.MicrosoftEdge => @"Microsoft\Edge",
            BrowserType.MicrosoftEdgeInsiderBeta => @"Microsoft\Edge Beta",
            BrowserType.MicrosoftEdgeInsiderDev => @"Microsoft\Edge Dev",
            BrowserType.MicrosoftEdgeInsiderCanary => @"Microsoft\Edge SxS",
            BrowserType.Opera => @"Opera Software\Opera Stable",
            BrowserType.OperaBeta => @"Opera Software\Opera Next",
            BrowserType.OperaDeveloper => @"Opera Software\Opera Developer",
            BrowserType.OperaGX => @"Opera Software\Opera GX Stable",
            BrowserType.OperaCrypto => @"Opera Software\Opera Crypto Stable",
            BrowserType.Vivaldi => "Vivaldi",
            BrowserType.MozillaFirefox => @"Mozilla\Firefox\Profiles",
            _ => @"Google\Chrome"
        };
    }

    /// <summary>
    /// 查詢 SQLite 資料庫
    /// </summary>
    /// <param name="browserType">BrowserType</param>
    /// <param name="cookieFilePath">字串，Cookie 檔案的位置</param>
    /// <param name="hostKey">字串，主機鍵值</param>
    /// <returns>List&lt;Cookie&gt;</returns>
    private static List<CookieData> QuerySQLiteDB(
        BrowserType browserType,
        string cookieFilePath,
        string hostKey)
    {
        List<CookieData> outputData = new();

        try
        {
            using SqliteConnection sqliteConnection = new($"Data Source={cookieFilePath}");
            using SqliteCommand sqliteCommand = sqliteConnection.CreateCommand();

            string rawTSQL = browserType switch
            {
                BrowserType.MozillaFirefox => "SELECT [name], [value], [host] FROM [moz_cookies]",
                _ => "SELECT [name], [encrypted_value], [host_key] FROM [cookies]"
            };
            string rawWhereClauseTSQL = browserType switch
            {
                BrowserType.MozillaFirefox => $" WHERE [host] = '{hostKey}'",
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
                byte[] key = browserType switch
                {
                    BrowserType.MozillaFirefox => Array.Empty<byte>(),
                    _ => AesGcm256.GetKey(browserType)
                };

                while (sqliteDataReader.Read())
                {
                    if (!outputData.Any(a => a.Name == sqliteDataReader.GetString(0)))
                    {
                        string value = string.Empty;

                        switch (browserType)
                        {
                            case BrowserType.MozillaFirefox:
                                value = sqliteDataReader.GetString(1);

                                break;
                            default:
                                byte[] encryptedData = GetBytes(sqliteDataReader, 1);
                                byte[] nonce, ciphertextTag;

                                AesGcm256.Prepare(encryptedData, out nonce, out ciphertextTag);

                                value = AesGcm256.Decrypt(ciphertextTag, key, nonce);

                                break;
                        }

                        outputData.Add(new CookieData()
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
    /// Cookie 資料類別
    /// </summary>
    public class CookieData
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
        /// <param name="browserType">BrowserType</param>
        /// <returns>字串</returns>
        public static byte[] GetKey(BrowserType browserType)
        {
            //string sR = string.Empty;
            //string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string path = $@"C:\Users\{Environment.UserName}\AppData\Local\{GetPartialPath(browserType)}\User Data\Local State";
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