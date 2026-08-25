# AGENTS.md

給所有 AI 編碼代理（Claude Code、Codex、Cursor、Copilot、Antigravity CLI 等）使用的專案規範。這份檔案是跨工具的共用事實來源；`CLAUDE.md` 僅是指向這裡的精簡入口。

## 專案是什麼

「YouTube 聊天室捕手」：模擬 YouTube 直播聊天室網頁版的行為，取得直播中或重播影片的聊天室內容，並提供收益分析、報表匯出等功能。詳見 [README.md](README.md)。

三個專案：

- `YTJsonParser/`：核心函式庫。負責向 YouTube 發送請求、解析回傳的 JSON（ytcfg、ytInitialData、innertube API 回應），對外提供強型別的 `RendererData`／`PostData`／`YTConfigData` 等模型，以及 `StreamLiveChatDataAsync`／`StreamCommunityPostsAsync` 兩個 `IAsyncEnumerable` 拉取式串流方法。不依賴任何 UI 框架、不依賴 DI 容器，理論上可被任何 .NET 應用程式引用。
- `YTLiveChatCatcher/`：WinForms 桌面應用程式，使用 `YTJsonParser` 的單一實例（`FMain.Variables.cs` 內的 `SharedYTJsonParser`，於 `InitLiveChatCather` 建立）驅動 UI，透過 `Task.Run` + `await foreach` 消費串流。
- `YTJsonParser.Tests/`：xUnit v3（`xunit.v3` 套件，非舊版 `xunit`）測試專案，用 `FakeHttpMessageHandler` 依 URL 比對回傳 `Fixtures/` 內的真實 JSON 樣本，完全不連線即可驗證解析邏輯。

## 建置、執行與測試

```bash
dotnet build YTLiveChatCatcher.sln
dotnet run --project YTLiveChatCatcher
dotnet test YTJsonParser.Tests/YTJsonParser.Tests.csproj
```

`dotnet test` 需要 repo 根目錄的 `global.json`（`{"test":{"runner":"Microsoft.Testing.Platform"}}`）才能在 .NET 10 SDK 下運作——xunit.v3 是 Microsoft.Testing.Platform 原生專案，.NET 10 的 `dotnet test` 預設仍嘗試走舊版 VSTest 橋接、會直接失敗，不要移除這個 `global.json`。也可以直接 `dotnet run --project YTJsonParser.Tests` 以主控台方式執行（xunit.v3 內建的 in-process runner）。

新增測試時：`YTJsonParser` 的解析方法幾乎都是 `private`，測試一律透過**公開 API**（`StreamLiveChatDataAsync`／`StreamCommunityPostsAsync`／`IsVideoStreamingAsync`）搭配 `FakeHttpMessageHandler` 進行，不要為了測試把方法改成 `internal` + `InternalsVisibleTo`——這樣測試才能在內部實作重構後仍然有效，同時也順便驗證了 URL 組成與 continuation 交換邏輯（這是最容易被 YouTube 改版打壞的部分）。新增 fixture 時，**用程式（例如 Python 的 `json.dumps`）產生保證合法的 JSON 字串再嵌入 HTML**，不要手動在一行內編織巢狀大括號／中括號——這次工作階段就因為手動編織漏算括號而讓其中一個 fixture 壞掉過。

## 目標框架與語言版本

- .NET 10.0 LTS（`net10.0-windows7.0`），C# 14（隨 SDK 預設）。
- `Nullable` 與 `ImplicitUsings` 皆為 `enable`。
- 檔案範圍命名空間（`namespace Foo;`，不用大括號包整個檔案）。
- `YTJsonParser` 類別用多個 partial class 檔案依職責切分（`YTJsonParser.Core.cs`、`YTJsonParser.Common.cs`、`YTJsonParser.ParseLiveChatJson.cs`、`YTJsonParser.PublicStatic.cs` 等）——新增方法時找對應職責的檔案放，不要都塞進 `YTJsonParser.cs`。

## 核心程式庫的架構（2026/8 現代化重構後）

- **建構子取代 `Init()`**：`new YTJsonParser(YTJsonParserOptions? options, ILogger<YTJsonParser>? logger)`。`YTJsonParserOptions`（`YTJsonParserOptions.cs`）是建構後不可變的 `record`（HttpClient、DisplayLanguage、FetchLargePicture）。唯一例外是 `Cookies`——因為使用者常在同一個 session 內動態切換／重新登入，所以刻意設計成可變屬性（`ytJsonParser.Cookies = "..."`），不併入不可變的 options，理由寫在 `YTJsonParser.PublicStatic.cs` 的 `Cookies` 屬性註解上。
- **`IAsyncEnumerable<T>` 取代事件**：`StreamLiveChatDataAsync`／`StreamCommunityPostsAsync` 回傳 `IAsyncEnumerable<IReadOnlyList<T>>`（每次列舉＝一次輪詢取得的一批資料，不是逐則訊息——YouTube 本來就是批次回應，逐則 yield 只會讓 WinForms 端被迫逐則 `Invoke`）。取消完全交給呼叫端自己的 `CancellationTokenSource`，函式庫不再保管 `Task`／`CancellationTokenSource`／`Stop()`。`LiveChatStreamOptions`／`CommunityPostStreamOptions` 是每次呼叫的區域參數（`LiveChatType`、`CustomLiveChatType`、`ForceIntervalMs` 等），不是共用可變狀態——這讓同一個 `YTJsonParser` 實例可以同時執行多個獨立的串流，彼此不會互相干擾。
- **`ILogger` 取代自製 log 事件**：建構子接受 `ILogger<YTJsonParser>`（預設 `NullLogger`）。內部記錄呼叫集中在 `LogMessages.cs`（`[LoggerMessage]` source-generated partial method），大部分呼叫點用通用範本（`LogMessages.Error/Warning/Debug/Trace`），少數語意明確的事件（例如 `YtConfigDataIsNull`、`SubMenuCustomTitle`）有專屬方法。新增記錄呼叫時比照這個分法，不要退回手動組字串的 `_logger.LogXxx($"...")`。
- **輪詢間隔的回報**：`StreamLiveChatDataAsync` 有一個 `IProgress<int>? intervalProgress` 參數，每次輪詢間隔更新時會呼叫 `.Report(intervalMs)`——這是 WinForms 端用來更新「下次擷取還要等幾秒」文字框的方式，取代了舊版「解析 log 訊息字串抓數字」的做法。
- **純工具函式已拆出**：`GetYouTubeChannelID`／`GetYouTubeVideoID`／`GetYouTubeChannelUrl`（`Utils/YouTubeUrlUtil.cs`）都是不依賴 instance 狀態的 `static` 方法，呼叫時不需要（也不應該透過）`YTJsonParser` 實例。

## 取得會員限定內容用的 Cookie（2026/8 重構後）

`YTJsonParser` 本身**不提供**任何讀取或解密瀏覽器（Chrome／Edge／Firefox）Cookie 資料庫的方法——舊版的 `Utils/WebBrowserUtil.cs`／`Utils/YouTubeCookieUtil.cs` 已完全移除。原因：

1. **已經技術上失效**：Chrome 127+（2024/7 起）與新版 Edge 預設啟用 App-Bound Encryption，一般使用者權限的 DPAPI 已解不出正確的金鑰，舊版做法對主流瀏覽器基本上已經拿不到資料。
2. **與竊資軟體技術特徵重疊**：直接解密另一個應用程式私有儲存的憑證資料庫，即使程式碼本身沒有惡意，仍容易被防毒軟體誤判、被使用者誤解、被上架平台拒絕。

`YTLiveChatCatcher`（WinForms 端）改用官方支援的介面取得 Cookie，兩條路徑互為備援：

- **`FCookieLogin`（主要）**：應用程式專屬的 `Microsoft.Web.WebView2` 登入視窗，使用**自己專屬的 user data folder**（`%LocalAppData%\YTLiveChatCatcher\WebView2Profile`，刻意不指向使用者既有的 Edge／Chrome profile 路徑）。使用者在裡面直接登入 Google／YouTube 帳號後，透過官方 API `CoreWebView2CookieManager.GetCookiesAsync("https://www.youtube.com/")` 取得該 WebView2 profile 的 Cookie——這是瀏覽器自己把資料交給宿主程式，不受 App-Bound Encryption 影響，也不會碰到使用者日常瀏覽器的資料。
- **手動貼上（備援）**：`FCookieLogin` 也提供一個文字欄位，讓使用者自行貼上從瀏覽器複製的 Cookie 字串，在 WebView2 不可用（例如企業政策擋掉、Runtime 未安裝）時仍有路可走。

儲存：預設 Cookie 只存在記憶體（`SharedYTJsonParser.Cookies`），關閉程式即遺失；使用者可在 `FCookieLogin` 勾選「記住我」，才會透過 `Common/Utils/SecureCookieStore.cs` 以 **DPAPI（`CurrentUser` scope）加密後**寫入本機檔案（`%LocalAppData%\YTLiveChatCatcher\cookie.dat`）——注意這裡的 DPAPI 用途是「加密自己這個程式要存的東西」，跟舊版「解密別的應用程式已加密的資料」方向相反，只有同一台機器、同一個 Windows 使用者才能還原。`FCookieLogin` 的「登出／清除已儲存資料」會同時清掉 WebView2 profile 內的 Cookie 與這個加密檔案。

## 程式風格

- 註解與 XML 文件註解一律使用繁體中文。
- 只在「為什麼」不明顯時才寫註解（隱藏限制、繞過某個特定 bug 的原因），不要寫「這段程式碼在做什麼」這種顯而易見的說明。
- 解析 YouTube 回傳的原始 JSON（ytInitialData／innertube 回應內的 `*Renderer`／`*ViewModel`）時，**刻意使用 `JsonElement.Get(...)` 防禦式走訪**（見 `Extensions/JsonElementExtension.cs`），不要為這一層改寫成強型別 DTO 直接反序列化——原因見下方「YouTube 端點會不定期變動」。
- 相對地，穩定、簡單的設定資料（例如 ytcfg 的 `INNERTUBE_API_KEY` 等欄位，見 `Models/YtCfgDto.cs`）可以、也應該用強型別 DTO + `JsonElement.Deserialize<T>()`。
- 對外公開的模型（`RendererData`、`YTConfigData`、`PostData` 等）永遠是強型別、有 `[JsonPropertyName]` 標註的 C# 類別。

## ⚠️ YouTube 端點會不定期變動，且沒有官方文件

`YTJsonParser` 呼叫的是 YouTube 網頁版背後的 InnerTube 內部 API（未公開、無版本保證），**YouTube 曾在 2025/10 做過一次破壞性改版**，讓當時的 continuation 擷取邏輯完全失效。2026/8 已重新驗證並修正為統一使用 `GET /live_chat?is_popout=1&v={videoID}`（直播、重播皆同一端點）取得 `contents.liveChatRenderer`，並統一以 `POST /youtubei/v1/live_chat/get_live_chat`輪詢（`get_live_chat_replay` 端點目前已回傳 400，勿使用）。

**如果聊天室擷取功能又不動了**：先用 `.claude/skills/yt-fetch-diagnose/SKILL.md` 描述的步驟，直接對 YouTube 發請求比對目前的 JSON 結構跟程式碼裡假設的路徑是否還吻合，而不是憑空猜測或改用其他假設。

## 參考第三方專案的原則（Clean Room）

理解 InnerTube 協議格式時，可以參考 chat-downloader、yt-dlp、其他語言的類似專案的公開文件／原始碼**去理解資料格式與端點行為**，但實作本身一律依據自己實際發送請求觀察到的 JSON 結果重新撰寫，不得複製其他專案的程式碼進來。既有程式碼中的 XML 註解會標註「參考：」某網址，代表該處的解析邏輯或欄位命名是依循該資料格式撰寫的說明，不代表程式碼是複製的。

## 已確認支援的 LiveChat 元素種類（2026/8 實測）

`YTJsonParser.ParseLiveChatJson.cs` 目前會解析：一般留言、超級留言、超級貼圖、加入／升級／里程碑會員、贈送會員（`liveChatSponsorshipsGiftPurchaseAnnouncementRenderer`）、接收會員贈送、新版個別小禮物（`giftMessageViewModel`，ViewModel 結構）、置頂／導向橫幅、跑馬燈（ticker，會取出內嵌的完整原始 Renderer）、捐款／購買／版主／自動版主訊息、創作者投票（`showLiveChatActionPanelAction` -> `pollRenderer`）、留言刪除（`removeChatItemAction`）、使用者被封鎖（`removeChatItemByAuthorAction`）、留言被取代／修改（`replaceChatItemAction`，例如超級留言淡出後改為較小樣式）。已於多支真實直播（VTuber、遊戲台、新聞台）上實測驗證。**注意**：判斷「是否已結束」請一律用 `IsVideoStreamingAsync` 或直接查 `liveBroadcastDetails.isLiveNow`，不要只憑「有 Top chat／Live chat 篩選選單」判斷（長時間常態直播與已結束重播都可能同時有這個選單）。「重播」情境目前驗證覆蓋範圍見下方「已知技術債」。

尚未實作、且**尚未在真實資料中觀察到具體內容**，僅在頁面內的型別註冊清單看到名稱、須等實際觀察到再補上解析的項目：

- `creatorHeartViewModel`：疑似「創作者已 Heart 該留言」的裝飾標記（可能附加在既有訊息上，而非獨立的 action），尚未在實測中捕捉到實際觸發的 JSON 內容。
- `pointsButton` / `liveViewerLeaderboardChatEntryPointViewModel`（"Top fans" 觀眾排行榜進入點）：這是聊天室**標頭**的靜態導覽按鈕（點擊會開啟 `ENGAGEMENT_PANEL_SURFACE_LIVE_CHAT` 面板），不是逐則訊息的 action，本身沒有「訊息內容」可解析，不在 `ParseActions`／`ParseRenderer` 的處理範圍內。

## 社群貼文分頁尋找機制（`GetCommunityTab`，2026/8 修正）

實測發現 YouTube 對 `/channel/{id}/community` 的回應，現在只會在 `tabs` 陣列內回傳**單一、已經選取**的分頁，不再像過去一樣把所有分頁都塞進同一份回應，也不再穩定提供可比對 `"/community"` 的 `tabRenderer.endpoint.commandMetadata.webCommandMetadata.url`（該欄位常常整個不存在）。舊版靠網址比對的 `GetCommunityTab` 因此完全失效——對任何 2026 年的頻道都會回傳 `null`，導致社群貼文擷取從頭到尾拿不到任何資料。已修正為優先找 `tabRenderer.selected == true` 的分頁，找不到才退回舊版網址比對，最後才退回「若整份回應只有一個分頁就直接視為社群分頁」。已用 10 個真實頻道驗證修正後可正常運作（image／video／poll 三種附件皆有實測樣本，皆已支援）。

## 直播／重播狀態判斷（`IsVideoStreamingAsync`）

`IsVideoStreamingAsync`（`YTJsonParser.Public.cs`）改為抓取 `/watch?v=` 頁面的 `ytInitialPlayerResponse`，
以 `microformat.playerMicroformatRenderer.liveBroadcastDetails.isLiveNow` 判斷「是否目前正在直播」，
並以 `videoDetails.isLive` 作為備援。已實測驗證四種情境：目前直播中、已結束的直播（`isLiveNow=false` 且多出
`endTimestamp`）、一般非直播來源影片（`isLiveContent=false`）、長時間 24/7 常態直播（`isLiveNow` 持續為
`true`）。**注意**：光看 `isLiveContent=true` 或聊天室是否有 "Top chat"/"Live chat" 篩選選單，都無法用來判斷
「是否已結束」——這兩者在長時間常態直播與已結束重播上都可能同時成立，唯一可靠的欄位是
`liveBroadcastDetails.isLiveNow`／`videoDetails.isLive`。

`ytInitialPlayerResponse` 內可能含有巢狀大型字串（例如 SVG 圖示），單純裁切最後一個 `;` 並不可靠；
`IsVideoStreamingAsync` 因此改用 `ExtractBalancedJsonObject`（括號配對）取出完整 JSON 物件。若之後要在
其他地方解析 `ytInitialPlayerResponse`，建議沿用這個做法而非字串裁切。

## 輪詢頻率安全下限

`IntervalMs` 屬性（`YTJsonParser.PublicStatic.cs`）在未設定 `ForceIntervalMs` 時，會取「YouTube 回應解析出的間隔值」與 `MinimumIntervalMs`（1000 毫秒）兩者中較大的值，避免因回應內容解析失敗等異常狀況導致間隔值意外停留在 0，對 YouTube 形成近乎無間隔的高頻輪詢。這個下限不會套用在您已明確設定的 `ForceIntervalMs` 上（視為刻意的選擇）。修改輪詢相關邏輯時，請維持「正常情況下遵循 YouTube 回應建議的間隔、異常情況下也不會緊迫輪詢」這個原則，不要為了效能把這個安全下限拿掉。

## 已知的行為變化（2026/8 重構後）

`YTJsonParser` 的內部記錄（原本會透過 `OnLogOutput` 事件轉發到 WinForms 應用程式自己的 `TBLog` 文字框）現在只會寫進標準 `ILogger`（實際落地在 NLog 的 `Logs/log.txt` 檔案與主控台輸出），**不會**再自動出現在應用程式的記錄文字框裡。如果之後想在 UI 文字框也看到，需要另外接一個自訂 NLog target 把 `YTJsonParser` 這個 logger 分類的訊息轉發過去，這次重構刻意沒有做這件事（改用標準記錄架構後，「查看記錄檔」才是預期的除錯方式）。

## 已知技術債（非本次任務範圍，供後續參考）

- `EPPlus` 使用 Polyform Noncommercial 授權（`ExcelPackage.License.SetNonCommercialOrganization(...)`，`FMain.EPPlusUtil.cs`／`FMain.Methods.cs` 各呼叫一次，分別對應匯入／匯出兩個獨立進入點，非重複程式碼）。2026/8 已確認目前為最新穩定版（8.7.0），且本專案為免費、非商業性質，符合此授權條款；商業用途需另外購買授權，之後更新版本前請留意授權條款是否變動。
- LiveChat 的「重播」情境：2026/8 已實際測試 8 支不同頻道（Hololive 官方 VTuber、獨立 VTuber Ironmouse、西方遊戲實況主 Asmongold、台灣實況主館長）已確認結束的直播（`isLiveNow=false` 且有 `endTimestamp`），**全部**的重播聊天室（`GET /live_chat?is_popout=1&v={videoID}`）都回傳關閉訊息（`contents.messageRenderer`，無 `liveChatRenderer`／`liveChatReplayContinuationData`）。依 YouTube 官方說明（[Learn about Live Chat](https://support.google.com/youtube/answer/15268877)），原因是**創作者端設定**：頻道可在「自訂管道」關閉「即時聊天室重播」、對直播影片做過剪輯（video editor 編輯過的影片一律沒有聊天室重播）、或事後手動刪除／設為會員限定，且不會通知觀眾。這代表 LiveChat 解析邏輯目前依然沒有一支「確認已結束、且重播聊天室仍開放」的真實影片做過完整的訊息批次驗證——不是本專案程式碼的問題，而是「熱門頻道普遍會關閉或剪輯」讓合適的測試樣本很難找。之後若要驗證，優先找**未剪輯的個人／小型頻道**且結束不久的直播，且驗證前務必先用 curl 或瀏覽器直接檢查 `live_chat?is_popout=1` 回應內是否真的有 `liveChatRenderer`，不要直接假設熱門頻道會開放。
