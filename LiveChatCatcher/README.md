# LiveChatCatcher

## 一、簡介

原本為 `YouTube 聊天室捕手` 的核心程式碼，為了未來的開發以及再利用，因此將 `YouTube 聊天室捕手` 中與獲取 YouTube 聊天室的即時聊天資料有關的程式碼，剝離出來，並做成獨立的函式庫以供使用。

## 二、使用範例

```csharp
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Events;
using Rubujo.YouTube.Utility.Extensions;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Sets;
using System;
using System.Collections.Generic;
using System.Net.Http;

void Main()
{
	// 宣告影片的網址或是 ID 值。
	string videoUrlOrID = "{影片的網址或是 ID 值}";

	// 宣告 httpClient。
	HttpClient httpClient = new();

	// 設定使用者代理字串。
	string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " + 
		"AppleWebKit/537.36 (KHTML, like Gecko) " + 
		"Chrome/120.0.0.0 Safari/537.36";
		
	httpClient.DefaultRequestHeaders.UserAgent.Clear();
	httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);

	// 宣告 listMessage，用於暫存獲取到的即時聊天資料。
	List<RendererData> listMessage = [];

	// 宣告 liveChatCatcher。
	LiveChatCatcher liveChatCatcher = new();

	// 初始化 liveChatCatcher。
	//liveChatCatcher.Init(
	//	httpClient: httpClient,
	//	timeoutMs: 3000,
	//	isStreaming: false,
	//	isFetchLargePicture: true);
		
	liveChatCatcher.Init(httpClient: httpClient);

	// 設定目標影片是否為直播。
	// ※預設值為 false。
	//liveChatCatcher.IsStreaming(false);

	// 設定逾時毫秒值。（意即得等待多久，才會再抓取下一批資料）
	// 1. 在重播影片時可以自由設定。（配合 UseCookies() 使用時，此值請不要設太低，以免 YouTube 或是 Google 帳號被停權。）
	// 2. 在直播影片時，會直接使用獲取到的 timeoutMs 的值。（意即指不會理會使用此方法設定的值）
	// ※ 預設值為 3000。
	//liveChatCatcher.TimeoutMs(3000);

	// 設定是否使用 Cookies。
	// 1. "browserType" 為網頁瀏覽器的類型。
	// 2. "profileFolderName" 為設定檔資料夾名稱。
	// ※預設是不使用 Cookies。
	//liveChatCatcher.UseCookies(
	//	enable: true,
	//	browserType: WebBrowserUtil.BrowserType.GoogleChrome,
	//	profileFolderName: string.Empty);

	// 設定不使用 Cookies。
	//liveChatCatcher.UseCookies(enable: false);

	// 設定獲取大張圖片。
	// ※預設值為 true。
	//liveChatCatcher.FetchLargePicture(true);

	// 設定獲取小張圖片。
	//liveChatCatcher.FetchLargePicture(false);

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
				Console.WriteLine(listMessage.ToJson());
				
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
	
	// 手動停止獲取即時聊天資料。
	//liveChatCatcher.Stop();
}
```

## 三、注意事項

1. 本函式庫僅支援`正體中文`。
2. 本函式庫是以 `gl = "TW"`、`hl = "zh-TW"` 等語系參數來取得 YouTube 聊天室的即時聊天資料。
3. 本函式庫`僅支援部分類型`的即時聊天資料的獲取。
4. 取得 Cookies 的相關方法，`僅限於 Microsoft Windows 平臺可以使用。`