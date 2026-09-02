using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using Rubujo.YouTube.Utility.Models.LiveChat;

namespace YTLiveChatCatcher.Common.Utils;

/// <summary>
/// 擷取過程的當機復原記錄（JSON Lines，一行一批次）。
/// <para>目的：擷取聊天室是一個可能持續數小時的過程，畫面上的資料只存在記憶體裡，使用者必須自己記得
/// 手動匯出——應用程式當機、非預期關閉，或單純忘記匯出就關閉，都會讓那幾個小時累積的資料整批消失。
/// 這個類別在每次收到新批次資料時就直接附加寫入本機檔案，下次啟動時若偵測到這個檔案還存在，
/// 代表上次沒有正常結束（見 <see cref="FMain.FMain_Load"/>），可以提示使用者是否要載入回來。</para>
/// <para>用 JSON Lines 而不是每次重新寫一份完整快照，是因為附加寫入的成本不會隨著已擷取的資料量增加
/// 而變貴，適合這種「頻繁、少量」的寫入模式；即使中途當機導致最後一行沒寫完整，<see cref="LoadBatches"/>
/// 也只會略過那一行毀損的資料，不影響前面已經完整寫入的批次。</para>
/// </summary>
public static class CaptureRecoveryStore
{
    private const string EncryptedLinePrefix = "dpapi:";

    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "YTLiveChatCatcher",
        "recovery.jsonl");

    /// <summary>
    /// 是否存在非空的復原記錄
    /// </summary>
    /// <returns>布林值</returns>
    public static bool Exists()
    {
        return File.Exists(FilePath) && new FileInfo(FilePath).Length > 0;
    }

    /// <summary>
    /// 附加寫入一批新資料
    /// </summary>
    /// <param name="batch">IReadOnlyList&lt;RendererData&gt;</param>
    public static void AppendBatch(IReadOnlyList<RendererData> batch)
    {
        string? directory = Path.GetDirectoryName(FilePath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string serializedBatch = SerializeBatchLine(batch);
        string protectedLine = ProtectLine(serializedBatch);

        File.AppendAllText(FilePath, protectedLine + Environment.NewLine, Encoding.UTF8);
    }

    /// <summary>
    /// 讀取所有已記錄的批次
    /// </summary>
    /// <returns>List&lt;List&lt;RendererData&gt;&gt;</returns>
    public static List<List<RendererData>> LoadBatches()
    {
        if (!File.Exists(FilePath))
        {
            return [];
        }

        // 逐行讀取，避免長時間直播累積的大型復原檔在載入時，同時保留完整 string[] 與
        // 反序列化後的所有 RendererData，造成不必要的記憶體尖峰。
        return ParseStoredBatchLines(File.ReadLines(FilePath, Encoding.UTF8));
    }

    /// <summary>
    /// 把一批資料序列化成單行 JSON（不含換行字元），供 <see cref="AppendBatch"/> 使用，
    /// 抽成獨立方法純粹是為了不依賴真實檔案系統就能被單元測試覆蓋。
    /// </summary>
    /// <param name="batch">IReadOnlyList&lt;RendererData&gt;</param>
    /// <returns>字串</returns>
    public static string SerializeBatchLine(IReadOnlyList<RendererData> batch) => JsonSerializer.Serialize(batch);

    /// <summary>
    /// 解析多行 JSON Lines 內容，單行毀損（例如寫到一半時當機）時略過該行，
    /// 不影響其它已完整寫入的批次；抽成獨立方法純粹是為了不依賴真實檔案系統就能被單元測試覆蓋。
    /// </summary>
    /// <param name="lines">IEnumerable&lt;string&gt;</param>
    /// <returns>List&lt;List&lt;RendererData&gt;&gt;</returns>
    public static List<List<RendererData>> ParseBatchLines(IEnumerable<string> lines)
    {
        List<List<RendererData>> batches = [];

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                List<RendererData>? batch = JsonSerializer.Deserialize<List<RendererData>>(line);

                if (batch != null)
                {
                    batches.Add(batch);
                }
            }
            catch (JsonException)
            {
                // 單行毀損時略過，不影響其它已完整寫入的批次。
            }
        }

        return batches;
    }

    /// <summary>
    /// 解析磁碟上的復原記錄。新版內容以 DPAPI 加密；沒有前綴的舊版純文字 JSONL 仍可載入，
    /// 避免升級後讓使用者原本尚未匯出的復原資料失效。
    /// </summary>
    /// <param name="lines">IEnumerable&lt;string&gt;</param>
    /// <returns>List&lt;List&lt;RendererData&gt;&gt;</returns>
    private static List<List<RendererData>> ParseStoredBatchLines(IEnumerable<string> lines)
    {
        IEnumerable<string> unprotectedLines = lines.Select(TryUnprotectLine);

        return ParseBatchLines(unprotectedLines);
    }

    /// <summary>
    /// 使用 Windows DPAPI（CurrentUser）保護一行復原資料
    /// </summary>
    /// <param name="line">字串，JSON 資料</param>
    /// <returns>字串，含格式前綴的 Base64 密文</returns>
    private static string ProtectLine(string line)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(line);

        try
        {
            byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);

            return EncryptedLinePrefix + Convert.ToBase64String(encryptedBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plainBytes);
        }
    }

    /// <summary>
    /// 解密一行新版復原資料；舊版沒有加密前綴時原樣回傳，毀損或無法解密時回傳空字串並略過。
    /// </summary>
    /// <param name="line">字串，磁碟上的一行資料</param>
    /// <returns>字串，JSON 資料</returns>
    private static string TryUnprotectLine(string line)
    {
        if (!line.StartsWith(EncryptedLinePrefix, StringComparison.Ordinal))
        {
            return line;
        }

        try
        {
            byte[] encryptedBytes = Convert.FromBase64String(line[EncryptedLinePrefix.Length..]);
            byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);

            try
            {
                return Encoding.UTF8.GetString(plainBytes);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(plainBytes);
            }
        }
        catch (Exception ex) when (ex is FormatException or CryptographicException)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 清除復原記錄
    /// </summary>
    public static void Clear()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}
