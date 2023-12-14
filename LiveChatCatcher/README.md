# LiveChatCatcher

## 一、範例程式碼

```csharp
using System;
using System.Collections.Generic;
using Rubujo.YouTube.Utility;
using Rubujo.YouTube.Utility.Events;
using Rubujo.YouTube.Utility.Models;
using Rubujo.YouTube.Utility.Sets;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

void Main()
{
	// 宣告影片的 ID 值。
	string videoID = "UbbOUr_pxQk";
	
	// 宣告 httpClient。
	HttpClient httpClient = new();
	
	// 設定使用者代理字串。
	httpClient.DefaultRequestHeaders.UserAgent.Clear();
	httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
	
	// 宣告 listMessage，用於暫存獲取到的即時聊天資料。
	List<RendererData> listMessage = [];
	
	// 宣告 liveChatCatcher。
	LiveChatCatcher liveChatCatcher = new();

	// 初始化 liveChatCatcher。
	liveChatCatcher.Init(httpClient: httpClient);
	
	// 設定目標影片是否為直播。
	liveChatCatcher.IsStreaming(false);
	
	// 設定逾時毫秒值。（意即得等待多久，才會再抓取下一批資料）
	// 於重播影片時可以自由設定，但建議不要設太低，以免被停權，尤其是配和 UseCookies() 使用時。
	// 在直播影片時，會跟隨獲取到的 timeoutMs 的值。
	liveChatCatcher.TimeoutMs(3000);
	
	// 設定是否使用 Cookies。
	/*
	liveChatCatcher.UseCookies(
		enable: true,
		browserType: WebBrowserUtil.BrowserType.GoogleChrome,
		profileFolderName: string.Empty);
	*/
	
	// 設定不使用 Cookies。
	liveChatCatcher.UseCookies(enable: false);
	
	// 設定獲取大張圖片。
	liveChatCatcher.FetchLargePicture(true);

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
				TextEncoderSettings textEncoderSettings = new();

				textEncoderSettings.AllowRanges(UnicodeRanges.All);
				
				JsonSerializerOptions jsonSerializerOptions = new()
				{
					WriteIndented = true,
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					Encoder = JavaScriptEncoder.Create(textEncoderSettings)
				};

				string jsonContent = JsonSerializer.Serialize(listMessage, jsonSerializerOptions);
				
				Console.WriteLine(jsonContent);
				
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
				//Console.WriteLine(e.Message);

				break;
			default:
				break;
		}
	};
	
	// 開始獲取即時聊天資料。
	liveChatCatcher.Start(videoID);
	
	// 手動停止獲取即時聊天資料。
	//liveChatCatcher.Stop();
}
```

## 二、注意事項

1. 本函式庫僅支援`正體中文`。
2. 本函式庫是以 `gl = "TW"`、`hl = "zh-TW"` 等語系參數來取得 YouTube 直播聊天室的內容。