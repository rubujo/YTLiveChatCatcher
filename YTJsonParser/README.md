# YTJsonParser

## 一、簡介

原本為 `YouTube 聊天室捕手` 應用程式的核心程式碼，後續為了便利再利用，所以獨立成函式庫。

1. 本函式庫使用`正體中文`，預設是以`正體中文`的顯示語言參數來取得 YouTube 社群貼文、影片或直播影片的即時聊天資料。
2. 本函式庫`僅支援部分類型`的 YouTube 社群貼文、即時聊天資料的獲取。
3. `沒有人可以保證您使用本函式庫，不會違反 YouTube 或是 Google 的服務條款，相關的風險請您自行負責，否則請勿使用本函式庫。`

## 二、注意事項

1. 本函式庫的`公開靜態`方法，請在呼叫 `Init()` 方法後再呼叫使用。
2. 取得 Cookie 的相關方法，`僅限於` Microsoft Windows 平臺可以使用。
   - ※使用相關方法時，請先確認目標的網頁瀏覽器是否處於`關閉`的狀態，否則會有可能無法成功的取得該網頁瀏覽器的 Cookie 資料。

## 三、使用範例

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Events;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Models.Community;
using Rubujo.YouTube.Utility.Models.LiveChat;
using Rubujo.YouTube.Utility.Sets;
using Rubujo.YouTube.Utility.Utils;

#region 宣告變數

// 宣告 listMessage，用於暫存獲取到的即時聊天資料。
List<RendererData> listMessage = [];

// 宣告 listPost，用於暫存獲取到的社群貼文資料。
List<PostData> listPost = [];

#endregion

void Main()
{
	// 宣告 YouTube 影片、頻道的網址或是 ID 值。
	string urlOrIDOfChannelOrVideo = "{YouTube 影片、頻道的網址或是 ID 值}";
	
	// 宣告 isFetchLiveChatData，用於決定是否要獲取即時聊天資料。
	// 當值為 false 時，則為獲取社群貼文資料。
	bool isFetchLiveChatData = true;

	// 宣告 ytJsonParser。
	YTJsonParser ytJsonParser = new();

	#region 設定 YTJsonParser

	// 初始化 ytJsonParser。
	// ※在使用 YTJsonParser 的公開靜態方法前，需先呼叫此方法。
	ytJsonParser.Init();

	// 設定強制間隔毫秒值。（意即得等待多久，才會再抓取下一批資料）
	// ※配合 UseCookies() 使用時，此值請不要設太低，以免 YouTube 或是 Google 帳號被停權。
	// ※將此值設為 -1 即可改為使用 IntervalMs 的值。
	if (isFetchLiveChatData)
	{
		YTJsonParser.ForceIntervalMs(-1);
	}
	else
	{
		// ※在獲取社群貼文資料時，IntervalMs 的值會是 0，需手動設定期望的強制間隔毫秒值。
		YTJsonParser.ForceIntervalMs(1000);
	}

	// 設定是否使用 Cookie。
	// 1. "browserType" 為網頁瀏覽器的類型。
	// 2. "profileFolderName" 為設定檔資料夾名稱。
	// ※預設是不使用 Cookie。
	//ytJsonParser.UseCookie(
	//	enable: false,
	//	browserType: WebBrowserUtil.BrowserType.GoogleChrome,
	//	profileFolderName: string.Empty);

	// 設定獲取大張圖片。
	// ※預設值為 true。
	//YTJsonParser.FetchLargePicture(true);

	// 設定顯示語言。
	//※預設值為 EnumSet.DisplayLanguage.Chinese_Traditional。
	YTJsonParser.DisplayLanguage(EnumSet.DisplayLanguage.Chinese_Traditional);

	#region 設定本地化字串

	// 判斷語言是否已存在。
	if (DictionarySet.GetLocalizeDictionary().ContainsKey(EnumSet.DisplayLanguage.English))
	{
		// 若已存在則移除。
		DictionarySet.GetLocalizeDictionary().Remove(EnumSet.DisplayLanguage.English);
	}

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

	#region 更新現有的本地化字串

	//bool hasValue = DictionarySet.GetLocalizeDictionary()
	//	.TryGetValue(
	//		EnumSet.DisplayLanguage.English,
	//		out Dictionary<string, string>? value);
	//
	//if (hasValue && value != null)
	//{
	//	value[KeySet.ChatGeneral] = "{新的值}";
	//	value[KeySet.ChatSuperChat] = "{新的值}";
	//	value[KeySet.ChatSuperSticker] = "{新的值}";
	//	value[KeySet.ChatJoinMember] = "{新的值}";
	//	value[KeySet.ChatMemberUpgrade] = "{新的值}";
	//	value[KeySet.ChatMemberMilestone] = "{新的值}";
	//	value[KeySet.ChatMemberGift] = "{新的值}";
	//	value[KeySet.ChatReceivedMemberGift] = "{新的值}";
	//	value[KeySet.ChatRedirect] = "{新的值}";
	//	value[KeySet.ChatPinned] = "{新的值}";
	//	// 使用 Contains() 判斷。
	//	value[KeySet.MemberUpgrade] = "{新的值}";
	//	value[KeySet.MemberMilestone] = "{新的值}";
	//}

	#endregion

	#endregion

	if (isFetchLiveChatData)
	{
		// 設定要獲取的即時聊天類型。
		// ※預設值為 EnumSet.LiveChatType.All。
		YTJsonParser.LiveChatType(EnumSet.LiveChatType.All);

		// 設定自定義即時聊天類型。
		// 當使用此方法時，會自動忽略使用 YTJsonParser.LiveChatType() 方法所設定的值。
		// ※請依照顯示語言填入對應的語言字串。
		//YTJsonParser.CustomLiveChatType("重播熱門聊天室訊息");
		//YTJsonParser.CustomLiveChatType("聊天重播");
	}
	else
	{
		// 設定是否要獲取全部的社群貼文。
		// ※預設值為 true。
		YTJsonParser.FetchWholeCommunityPosts(true);
	}

	#endregion

	#region 事件

	// 獲取即時聊天資料事件。
	ytJsonParser.OnFecthLiveChatData += (object? sender, FecthLiveChatDataArgs e) =>
	{
		// 依據您的需求處理獲取到的即時聊天資料。
		listMessage.AddRange(e.Data);
	};

	ytJsonParser.OnFecthCommunityPosts += (object? sender, FecthCommunityPostsArgs e) =>
	{
		// 依據您的需求處理獲取到的社群貼文資料。
		listPost.AddRange(e.Data);
	};

	// 執行狀態更新事件。
	ytJsonParser.OnRunningStatusUpdate += (object? sender, RunningStatusArgs e) =>
	{
		EnumSet.RunningStatus runningStatus = e.RunningStatus;

		switch (runningStatus)
		{
			default:
			case EnumSet.RunningStatus.Running:
				Console.WriteLine(runningStatus.ToString());

				break;
			case EnumSet.RunningStatus.Stopped:
				Console.WriteLine(runningStatus.ToString());

				// 依據您的需求處理獲取到的資料，此處是輸出成 JSON 格式的資料。
				if (isFetchLiveChatData)
				{
					if (listMessage.Count > 0)
					{
						Console.WriteLine($"資料筆數: {listMessage.Count}");
						Console.WriteLine(listMessage.ToJsonString());
					}
				}
				else
				{
					if (listPost.Count > 0)
					{
						#region 後處理資料
						
						foreach (PostData postData in listPost)
						{
							postData.SetDataUri();
							
							if(postData.Attachments != null)
							{
								foreach (AttachmentData attachmentData in postData.Attachments)
								{
									attachmentData.SetDataUri();
								}
							}
						}
						
						#endregion
	
						Console.WriteLine($"資料筆數: {listPost.Count}");
						Console.WriteLine(listPost.ToJsonString());
					}
				}

				break;
			case EnumSet.RunningStatus.ErrorOccured:
				Console.WriteLine(runningStatus.ToString());

				break;
		}
	};

	// 紀錄輸出事件。
	ytJsonParser.OnLogOutput += (object? sender, LogOutputArgs e) =>
	{
		EnumSet.LogType logType = e.LogType;

		switch (logType)
		{
			case EnumSet.LogType.Info:
				Console.WriteLine(e.Message);

				break;
			case EnumSet.LogType.Warn:
				Console.WriteLine(e.Message);

				break;
			case EnumSet.LogType.Error:
				Console.WriteLine(e.Message);

				break;
			case EnumSet.LogType.Debug:
				// ※此處不輸出除錯資訊。
				//Console.WriteLine(e.Message);

				break;
			default:
				break;
		}
	};

	#endregion
	
	// 獲取資料。
	if (isFetchLiveChatData)
	{
		// 開始獲取即時聊天資料。
		ytJsonParser.StartFetchLiveChatData(urlOrIDOfChannelOrVideo);
	}
	else
	{
		// 開始獲取社群貼文資料。
		ytJsonParser.StartFetchCommunityPosts(urlOrIDOfChannelOrVideo);
	}

	// 於 5 秒後停止獲取即時聊天資料。
	Task.Delay(5000).ContinueWith(task => YTJsonParser.Stop());
}
```
