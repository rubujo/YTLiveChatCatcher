# YTJsonParser

## 一、簡介

原本為 `YouTube 聊天室捕手` 應用程式的核心程式碼，後續為了便利再利用，所以獨立成函式庫。

1. 本函式庫的程式碼、註解、文件一律使用`正體中文`；取得資料時使用的顯示語言（`DisplayLanguage`）預設依執行環境的 `CultureInfo.CurrentUICulture` 自動判斷，也可以在建構時透過 `YTJsonParserOptions.DisplayLanguage` 明確指定。
2. 本函式庫`僅支援部分類型`的 YouTube 社群貼文、即時聊天資料的獲取。
3. `沒有人可以保證您使用本函式庫，不會違反 YouTube 或是 Google 的服務條款，相關的風險請您自行負責，否則請勿使用本函式庫。`

## 二、注意事項

1. `YTJsonParser` 的設定（`HttpClient`、顯示語言等）在**建構時**透過 `YTJsonParserOptions` 傳入、建立後不可變，不需要（也沒有）額外呼叫 `Init()` 之類的初始化方法。
2. 若要帶入 Cookie（例如會員限定內容），本函式庫**不提供**任何直接讀取／解密瀏覽器 Cookie 資料庫的方法——請透過官方支援的介面（例如專屬登入視窗＋ `CoreWebView2CookieManager`，或使用者手動貼上）取得 Cookie 字串後，指派給 `Cookies` 屬性。
3. `StreamLiveChatDataAsync`／`StreamCommunityPostsAsync` 是 `IAsyncEnumerable`，取消由呼叫端自己持有的 `CancellationTokenSource` 負責，函式庫不會替您保管背景工作或計時器。

## 三、使用範例

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Models.Community;
using Rubujo.YouTube.Utility.Models.LiveChat;
using Rubujo.YouTube.Utility.Sets;
using Rubujo.YouTube.Utility.Utils;

async Task Main()
{
	// 宣告 YouTube 影片、頻道的網址或是 ID 值。
	string urlOrIDOfChannelOrVideo = "{YouTube 影片、頻道的網址或是 ID 值}";

	#region 設定本地化字串（選用）

	// 判斷語言是否已存在。
	if (!DictionarySet.GetLocalizeDictionary().ContainsKey(EnumSet.DisplayLanguage.English))
	{
		// 若不存在則新增。
		DictionarySet.GetLocalizeDictionary().Add(
			EnumSet.DisplayLanguage.English,
			new Dictionary<string, string>()
			{
				{ KeySet.ChatGeneral, "General" },
				{ KeySet.ChatSuperChat, "Super Chat" },
				{ KeySet.ChatSuperSticker, "Super Sticker" },
				{ KeySet.ChatJoinMember, "Join Member" },
				{ KeySet.ChatMemberUpgrade, "Member Upgrade" },
				{ KeySet.ChatMemberMilestone, "Member Milestone" },
				{ KeySet.ChatMemberGift, "Member Gift" },
				{ KeySet.ChatReceivedMemberGift, "Received Member Gift" },
				{ KeySet.ChatRedirect, "Redirect" },
				{ KeySet.ChatPinned, "Pinned" },
				// 使用 Contains() 判斷。
				{ KeySet.MemberUpgrade, "Upgraded membership to" },
				{ KeySet.MemberMilestone, "Member for" }
			}
		);
	}

	#endregion

	// 建立 YTJsonParser 實例。
	// ※不指定 HttpClient 時，會自動建立一個並由本實例負責釋放（Dispose 時一併釋放）。
	// ※不指定 DisplayLanguage 時，會依 CultureInfo.CurrentUICulture 自動判斷顯示語言。
	// ※若要帶入 Cookie（例如會員限定內容），請透過官方支援的介面（例如專屬登入視窗＋
	//   CoreWebView2CookieManager，或使用者手動貼上）取得 Cookie 字串後再指派給 Cookies。
	using YTJsonParser ytJsonParser = new(
		new YTJsonParserOptions
		{
			DisplayLanguage = EnumSet.DisplayLanguage.Chinese_Traditional,
			FetchLargePicture = true,
			//Cookies = "{取得的 Cookie 字串}"
		});

	// 用來取消串流的 CancellationTokenSource，例如「於 5 秒後停止獲取即時聊天資料」。
	using CancellationTokenSource cancellationTokenSource = new();
	cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));

	#region 範例一：獲取即時聊天資料

	List<RendererData> listMessage = [];

	try
	{
		await foreach (IReadOnlyList<RendererData> batch in ytJsonParser.StreamLiveChatDataAsync(
			urlOrIDOfChannelOrVideo,
			options: new LiveChatStreamOptions
			{
				LiveChatType = EnumSet.LiveChatType.All,
				// 設定自定義即時聊天類型。
				// 當有設定此屬性時，會自動忽略 LiveChatType 的值。
				// ※請依照顯示語言填入對應的語言字串。
				//CustomLiveChatType = "重播熱門聊天室訊息",
			},
			cancellationToken: cancellationTokenSource.Token))
		{
			// 依據您的需求處理獲取到的即時聊天資料。
			listMessage.AddRange(batch);
		}
	}
	catch (OperationCanceledException)
	{
		// 正常取消，不需特別處理。
	}

	if (listMessage.Count > 0)
	{
		Console.WriteLine($"資料筆數: {listMessage.Count}");
		Console.WriteLine(listMessage.ToJsonString());
	}

	#endregion

	#region 範例二：獲取社群貼文資料

	List<PostData> listPost = [];

	try
	{
		await foreach (IReadOnlyList<PostData> batch in ytJsonParser.StreamCommunityPostsAsync(
			urlOrIDOfChannelOrVideo,
			options: new CommunityPostStreamOptions { FetchWholeCommunityPosts = true },
			cancellationToken: cancellationTokenSource.Token))
		{
			listPost.AddRange(batch);
		}
	}
	catch (OperationCanceledException)
	{
		// 正常取消，不需特別處理。
	}

	if (listPost.Count > 0)
	{
		#region 後處理資料

		foreach (PostData postData in listPost)
		{
			await postData.SetDataUri(ytJsonParser);

			if (postData.Attachments != null)
			{
				foreach (AttachmentData attachmentData in postData.Attachments)
				{
					await attachmentData.SetDataUri(ytJsonParser);
				}
			}
		}

		#endregion

		Console.WriteLine($"資料筆數: {listPost.Count}");
		Console.WriteLine(listPost.ToJsonString());
	}

	#endregion
}
```

需要記錄除錯資訊時，建構子可傳入 `ILogger<YTJsonParser>`（例如 `Microsoft.Extensions.Logging` 的 `ILoggerFactory.CreateLogger<YTJsonParser>()`），未指定時預設不記錄（`NullLogger`）。
