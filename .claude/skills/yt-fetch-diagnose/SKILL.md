---
name: yt-fetch-diagnose
description: 診斷 YouTube 直播／重播聊天室 JSON 擷取機制是否仍可用。當使用者反應「抓不到聊天室內容」、「聊天室資料是空的」、或要求「確認擷取機制是否仍正常」時使用。不綁定特定 AI 工具，任何能執行 shell 指令的 Agent 都可以照著本文件的步驟操作。
---

# 診斷 YouTube 聊天室擷取機制

YTJsonParser 呼叫的是 YouTube 網頁版背後、未公開且無版本保證的 InnerTube API。YouTube 曾在 2025/10 做過一次破壞性改版（詳見 [AGENTS.md](../../../AGENTS.md)），因此若擷取功能忽然失效，第一步永遠是**直接對 YouTube 發請求，比對真實 JSON 結構跟程式碼裡假設的路徑是否還吻合**，而不是憑空猜測或改寫成另一種假設。

## 步驟

### 1. 準備測試影片

找一支目前正在直播、且聊天室未關閉的影片 ID（可用 `curl` 打 YouTube 搜尋頁篩選 `EgJAAQ%3D%3D`，即「直播中」篩選條件），以及一支已結束、開放聊天重播的影片 ID（搜尋詞加上「已直播」/「streamed live」較容易找到）。

```bash
curl -s -A "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0 Safari/537.36" \
  "https://www.youtube.com/results?search_query=live&sp=EgJAAQ%253D%253D" -o search.html
grep -o '"videoId":"[a-zA-Z0-9_-]\{11\}"' search.html | head -5
```

### 2. 打統一擷取端點，確認 `contents.liveChatRenderer` 結構還在

目前（2026/8）驗證有效的做法：**不分直播／重播，一律先打同一個 popout 端點**（但重播影片幾乎必定要接著走步驟 2.5 的 fallback，見下方說明）：

```bash
curl -s -A "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0 Safari/537.36" \
  "https://www.youtube.com/live_chat?is_popout=1&v={videoID}" -o livechat.html
```

用 Python（或任何 JSON 工具）解析 `window["ytInitialData"] = {...};`，檢查：

- `contents.liveChatRenderer` 是否存在。**若是 `contents.messageRenderer`（文字通常是「這部直播影片的聊天室已停用。」），不要直接認定該影片聊天室被關閉、換一支影片測試**——2026/8 已證實過至少一種情境（例如聊天室在直播期間曾被限制過的「初配信」）這其實是假象，真正的聊天室還在，只是網頁版把它做成「需要重新載入」的狀態。先照下方步驟 2.5 檢查 `/watch` 頁面，兩邊都沒有 continuation 才代表真的關閉。
- `contents.liveChatRenderer.continuations[0].invalidationContinuationData.continuation` 是否存在（直播／重播輪詢用的權杖）。

### 2.5　popout 頁面顯示「已停用」時，檢查 `/watch` 頁面是否其實有「重新載入」路徑

```bash
curl -s -A "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0 Safari/537.36" \
  -b "CONSENT=YES+1" \
  "https://www.youtube.com/watch?v={videoID}" -o watch.html
```

用 Python 解析 `var ytInitialData = {...};`（注意跟 popout 頁面的 `window["ytInitialData"] = ` 包裝不同），檢查
`contents.twoColumnWatchNextResults.conversationBar.liveChatRenderer` 是否存在，若存在再檢查
`continuations[0].reloadContinuationData.continuation` 是否存在——這個 continuation 型別跟一般輪詢不同，且必須改打 `get_live_chat_replay`（見步驟 3）。目前的實作（`YTJsonParser.Core.cs` 的 `GetReplayReloadContinuationAsync`）已經會自動做這個 fallback，這裡的手動 curl 驗證主要用於：YouTube 又改版、fallback 也失效時，重新確認目前真正有效的欄位路徑。
- `contents.liveChatRenderer.header.liveChatHeaderRenderer.viewSelector.sortFilterSubMenuRenderer.subMenuItems` 內每個項目的 `title` 字串（例如 "Top chat"／"Live chat"）——**這組標題文字本身在 2025/10 前後就已經變過**（原本是 "Top chat replay"／"Live chat replay"），若之後又改了，`YTJsonParser.ParseLiveChatJson.cs` 內 `ParseSubMenuItemsContinuation` 用 `LiveChatStreamOptions.CustomLiveChatType` 比對標題字串的邏輯就需要跟著更新，或改回索引比對。
- `contents.liveChatRenderer.actions` 內是否有 `addChatItemAction`／`removeChatItemAction`／`removeChatItemByAuthorAction`／`showLiveChatActionPanelAction`（投票）等已知的 action 種類；若出現陌生的 key，代表 YouTube 又新增了聊天室元素種類，需要在 `YTJsonParser.ParseLiveChatJson.cs` 的 `ParseRenderer`／`ParseNonMessageAction` 補上對應處理（不要直接忽略，先印出該 JSON 片段確認內容再決定怎麼解析）。

### 3. 確認輪詢端點

用步驟 2 拿到的 continuation，POST 到：

```
https://www.youtube.com/youtubei/v1/live_chat/get_live_chat?key={INNERTUBE_API_KEY}
```

（`INNERTUBE_API_KEY` 從同一個 HTML 內的 `ytcfg.set({...})` 取得）body 格式：

```json
{ "context": { "client": { /* 從 ytcfg 的 INNERTUBE_CONTEXT.client 複製 */ } }, "continuation": "..." }
```

**注意**：`get_live_chat_replay` 並非「一律回傳 400」——2026/8 曾一度下過這個結論，但那是拿 `invalidationContinuationData`（一般輪詢用、通常來自 popout 頁面）的權杖去打這個端點才會 400。這個端點真正吃的是步驟 2.5 的 `reloadContinuationData`，或它自己回應內下一頁的 `liveChatReplayContinuationData`——用對權杖時，這個端點運作正常。**判斷規則**：popout 頁面直接給出 continuation → 用 `get_live_chat`（實測目前只有「正在直播中」的影片會走到這條）；popout 頁面顯示「已停用」但 `/watch` 頁面有 `reloadContinuationData` → 改用 `get_live_chat_replay`（實測抽測 35 支涵蓋不同類型的已結束重播影片，**全部**都是這個情況，這是重播的常態路徑，不是少數例外）。兩條路徑都拿不到 continuation，才代表該影片真的沒有開放聊天室重播。如果連這套判斷規則都開始不準，代表 YouTube 又改版了，需要重新走一次本文件的流程，找出目前真正有效的端點/欄位。

### 4. 用實際程式碼跑一次端對端驗證

比起單獨用 curl 兜資料，更可靠的方式是寫一支暫時性主控台專案直接引用 `YTJsonParser.csproj`，建立 `YTJsonParser` 實例、傳入一個會把 Debug 等級也印出來的 `ILogger`（篩選訊息中含有「尚未支援的內容」或「略過不處理的內容」的紀錄，藉此發現真正遺漏的元素種類）、以 `await foreach` 消費 `StreamLiveChatDataAsync(videoID)`，實際觀察是否收到訊息、`CancellationTokenSource.Cancel()` 後迴圈是否能在數百毫秒內結束。範例架構：

```csharp
using Microsoft.Extensions.Logging;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder
    .AddConsole()
    .SetMinimumLevel(LogLevel.Debug));

using YTJsonParser ytJsonParser = new(logger: loggerFactory.CreateLogger<YTJsonParser>());

using CancellationTokenSource cancellationTokenSource = new();
cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(15));

try
{
    await foreach (IReadOnlyList<RendererData> batch in ytJsonParser.StreamLiveChatDataAsync(
        videoID,
        cancellationToken: cancellationTokenSource.Token))
    {
        Console.WriteLine($"+{batch.Count}");
    }
}
catch (OperationCanceledException)
{
    // 正常取消，不需特別處理。
}
```

測試完畢後刪除暫時性專案，不要留在版本庫內。

## 修好之後

在 `YTJsonParser.Core.cs`／`YTJsonParser.ParseLiveChatJson.cs` 對應方法的註解補上修正日期與簡述（沿用既有風格，例如「2026/8 更新：……」），並更新 `AGENTS.md` 內「YouTube 端點會不定期變動」段落的端點/日期資訊，讓下一次診斷有最新的基準點可以比對。
