# 即時聊天捕手

## 一、簡介

原本為 `YouTube 聊天室捕手` 的核心程式碼，為了未來的開發以及再利用，因此將 `YouTube 聊天室捕手` 中與獲取 YouTube 聊天室的即時聊天資料有關的程式碼，剝離出來，並做成獨立的函式庫以供使用。

## 二、使用範例

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Events;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Sets;
using Rubujo.YouTube.Utility.Utils;

void Main()
{
	// 宣告影片的網址或是 ID 值。
	string videoUrlOrID = "{影片的網址或是 ID 值}";

	// 宣告 listMessage，用於暫存獲取到的即時聊天資料。
	List<RendererData> listMessage = [];

	// 宣告 liveChatCatcher。
	LiveChatCatcher liveChatCatcher = new();

	// 初始化 liveChatCatcher。
	liveChatCatcher.Init();

	// 設定強制間隔毫秒值。（意即得等待多久，才會再抓取下一批資料）
	// 將此值設為 -1 即可改為使用 IntervalMs 的值。
	// ※配合 UseCookies() 使用時，此值請不要設太低，以免 YouTube 或是 Google 帳號被停權。
	LiveChatCatcher.ForceIntervalMs(-1);

	// 設定是否使用 Cookie。
	// 1. "browserType" 為網頁瀏覽器的類型。
	// 2. "profileFolderName" 為設定檔資料夾名稱。
	// ※預設是不使用 Cookie。
	liveChatCatcher.UseCookie(
		enable: false,
		browserType: WebBrowserUtil.BrowserType.GoogleChrome,
		profileFolderName: string.Empty);

	// 設定獲取大張圖片。
	// ※預設值為 true。
	LiveChatCatcher.FetchLargePicture(true);

	// 設定顯示語系。
	//※預設值為 EnumSet.DisplayLanguage.Chinese_Traditional。
	LiveChatCatcher.DisplayLanguage(EnumSet.DisplayLanguage.Tamil);

	// 設定即時聊天類型。
	LiveChatCatcher.LiveChatType(EnumSet.LiveChatType.All);

	// 設定自定義即時聊天類型。
	// 當使用此方法時，會自動忽略使用 liveChatCatcher.CustomLiveChatType() 方法的設定值。
	//LiveChatCatcher.CustomLiveChatType("重播熱門聊天室訊息");
	//LiveChatCatcher.CustomLiveChatType("聊天重播");

	// 獲取即時聊天資料事件。
	liveChatCatcher.OnFecthLiveChat += (object? sender, FecthLiveChatArgs e) =>
	{
		// 依據您的需求處理獲取到的即時聊天資料。
		listMessage.AddRange(e.Data);
	};

	// 執行狀態更新事件。
	liveChatCatcher.OnRunningStatusUpdate += (object? sender, RunningStatusArgs e) =>
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
				
				// 依據您的需求處理獲取到的即時聊天資料，此處是輸出成 JSON 格式的資料。
				Console.WriteLine($"資料筆數: {listMessage.Count}");
				Console.WriteLine(listMessage.ToJsonString());
				
				break;
			case EnumSet.RunningStatus.ErrorOccured:
				Console.WriteLine(runningStatus.ToString());

				break;
		}
	};

	// 紀錄輸出事件。
	liveChatCatcher.OnLogOutput += (object? sender, LogOutputArgs e) =>
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

	// 開始獲取即時聊天資料。
	liveChatCatcher.Start(videoUrlOrID);
	
	// 於 5 秒後停止獲取即時聊天資料。
	Task.Delay(5000).ContinueWith(task => liveChatCatcher.Stop());
```

## 三、注意事項

1. 本函式庫使用`正體中文`，預設是以`正體中文`的語系參數來取得 YouTube 影片或直播的即時聊天資料。
2. 本函式庫`僅支援部分類型`的即時聊天資料的獲取。
3. 取得 Cookie 的相關方法，`僅限於 Microsoft Windows 平臺可以使用。`
   - `※使用相關方法時，請先確認目標的網頁瀏覽器是處於關閉的狀態，否則有可能會無法成功的取得 Cookie 資料。`
4. 本函式庫的`靜態`方法，請在呼叫 `Init()` 方法後再呼叫使用。