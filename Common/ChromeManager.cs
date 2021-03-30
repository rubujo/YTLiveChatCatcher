using Microsoft.Data.Sqlite;
using NLog;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace YTLiveChatCatcher.Common;

/// <summary>
/// ChromeManager
/// <para>參考：https://stackoverflow.com/a/68703365 </para>
/// </summary>
public class ChromeManager
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 網頁瀏覽器
    /// </summary>
    public enum Browser
    {
        /// <summary>
        /// Google Chrome
        /// </summary>
        GoogleChrome = 1,
        /// <summary>
        /// Microsoft Edge
        /// </summary>
        MicrosoftEdge = 2
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
        if (string.IsNullOrEmpty(profileName))
        {
            profileName = "Default";
        }

        // 2022-05-17 Cookies 已經移至 Network 資料夾內。
        string ChromeCookiePath = @"C:\Users\" + Environment.UserName +
            @$"\AppData\Local\{GetPartialPath(browser)}\User Data\{profileName}\Network\Cookies";

        List<Cookie> data = new();

        if (File.Exists(ChromeCookiePath))
        {
            try
            {
                using SqliteConnection sqliteConnection = new($"Data Source={ChromeCookiePath}");
                using SqliteCommand sqliteCommand = sqliteConnection.CreateCommand();

                string sql = "SELECT [name], [encrypted_value], [host_key] FROM [cookies]";

                if (!string.IsNullOrEmpty(hostKey))
                {
                    sql += $" WHERE [host_key] = '{hostKey}'";
                    //sql += $" WHERE [host_key] LIKE '%{hostKey}%'";
                }

                sqliteCommand.CommandText = sql;

                byte[] key = AesGcm256.GetKey(browser);

                sqliteConnection.Open();

                using (SqliteDataReader sqliteDataReader = sqliteCommand.ExecuteReader())
                {
                    while (sqliteDataReader.Read())
                    {
                        if (!data.Any(a => a.Name == sqliteDataReader.GetString(0)))
                        {
                            byte[] encryptedData = GetBytes(sqliteDataReader, 1);

                            AesGcm256.Prepare(encryptedData, out byte[] nonce, out byte[] ciphertextTag);

                            string value = AesGcm256.Decrypt(ciphertextTag, key, nonce);

                            data.Add(new Cookie()
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
        }

        return data;
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
            Browser.GoogleChrome => @"Google\Chrome",
            Browser.MicrosoftEdge => @"Microsoft\Edge",
            _ => @"Google\Chrome"
        };
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
        public string? Name { get; set; }

        public string? HostKey { get; set; }

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

            string path = @"C:\Users\" + Environment.UserName +
                @$"\AppData\Local\{GetPartialPath(browser)}\User Data\Local State";

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