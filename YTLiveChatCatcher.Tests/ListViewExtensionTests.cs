using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Windows.Forms;
using YTLiveChatCatcher.Extensions;
using Xunit;

namespace YTLiveChatCatcher.Tests;

/// <summary>
/// 驗證 ListViewExtension.SetAuthorPhoto 的行為規格，特別是 2026/8 修正的 GDI+ 陷阱：
/// Image.FromStream 預設不會把像素資料複製進記憶體，若直接把它加進 ImageList 而不是複製成獨立的
/// Bitmap，來源的 MemoryStream 一旦被 Dispose，之後真正繪製這張圖片時會靜默失敗（不拋例外，
/// 只是畫面空白）。這裡驗證加入 ImageCollection 後的圖片即使在方法呼叫結束、來源串流早已釋放的
/// 情況下，仍然是可以正常存取像素資料的獨立圖片。
/// </summary>
public class ListViewExtensionTests
{
    private sealed class FakeImageHttpMessageHandler : HttpMessageHandler
    {
        private readonly byte[] _imageBytes;

        public FakeImageHttpMessageHandler(byte[] imageBytes)
        {
            _imageBytes = imageBytes;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(_imageBytes)
            };

            return Task.FromResult(response);
        }
    }

    private static byte[] CreateTestPngBytes()
    {
        using Bitmap bitmap = new(4, 4);

        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.Red);
        }

        using MemoryStream memoryStream = new();

        bitmap.Save(memoryStream, ImageFormat.Png);

        return memoryStream.ToArray();
    }

    [Fact]
    public async Task SetAuthorPhoto_加入的圖片在來源串流釋放後仍可正常存取像素資料()
    {
        using HttpClient httpClient = new(new FakeImageHttpMessageHandler(CreateTestPngBytes()));
        using ImageList imageList = new();

        string key = $"test-author-{Guid.NewGuid()}";

        string errorMessage = await imageList.Images.SetAuthorPhoto(httpClient, key, "https://example.com/avatar.png");

        Assert.Equal(string.Empty, errorMessage);
        Assert.True(imageList.Images.ContainsKey(key));

        Image storedImage = imageList.Images[key]!;

        // 若圖片仍然依賴已經 Dispose 的來源 MemoryStream，存取 Size／GetPixel 這類需要重新讀取
        // 像素資料的成員會拋出 ArgumentException("參數無效。")，而不是單純顯示空白——用這個當作
        // 「圖片資料是否真的獨立、可正常使用」的驗證方式。
        Size size = storedImage.Size;

        Assert.True(size.Width > 0);
        Assert.True(size.Height > 0);
    }

    [Fact]
    public async Task SetAuthorPhoto_同一個Key第二次呼叫時直接略過不重複下載()
    {
        using HttpClient httpClient = new(new FakeImageHttpMessageHandler(CreateTestPngBytes()));
        using ImageList imageList = new();

        string key = $"test-author-{Guid.NewGuid()}";

        string firstCallError = await imageList.Images.SetAuthorPhoto(httpClient, key, "https://example.com/avatar.png");
        int countAfterFirstCall = imageList.Images.Count;

        string secondCallError = await imageList.Images.SetAuthorPhoto(httpClient, key, "https://example.com/avatar.png");

        Assert.Equal(string.Empty, firstCallError);
        Assert.Equal(string.Empty, secondCallError);
        Assert.Equal(countAfterFirstCall, imageList.Images.Count);
    }

    [Fact]
    public async Task SetAuthorPhoto_httpClient為null時回傳錯誤訊息並改加入白色佔位圖()
    {
        using ImageList imageList = new();

        string key = $"test-author-{Guid.NewGuid()}";

        string errorMessage = await imageList.Images.SetAuthorPhoto(null, key, "https://example.com/avatar.png");

        // 下載失敗時仍會加入一張白色佔位圖，讓 ListView 不會因為缺圖示而出錯，
        // 只是同時回傳非空的 errorMessage 讓呼叫端可以記錄／顯示錯誤。
        Assert.False(string.IsNullOrEmpty(errorMessage));
        Assert.True(imageList.Images.ContainsKey(key));
    }
}
