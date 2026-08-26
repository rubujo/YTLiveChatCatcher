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
dotnet build YTLiveChatCatcher.slnx
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

**`GetYTConfigDataAsync`（`YTJsonParser.Core.cs`）擷取 ytcfg 的方式（2026/8 修正）**：LiveChat 情境已改用 `ExtractBalancedJsonObject`（括號配對，跟 `IsVideoStreamingAsync`／`GetVideoTitleAsync` 解析 `ytInitialPlayerResponse` 用的是同一個方法）取出 `ytcfg.set({...})` 的完整物件，取代舊版 `Replace("ytcfg.set(", "")` + `LastIndexOf("});")` 的字串裁切——舊版只要 ytcfg 內任何欄位值剛好含有字面上的 `"});"` 就會截斷錯誤，是同一類「巢狀大型字串打斷天真字串裁切」的問題。已對真實直播（東森新聞）與真實頻道（冬蜜 DonBee 社群貼文）驗證修正後兩條路徑皆正常運作。Community 情境的正則表達式擷取（`RegexYtCfgCommunity`）未受影響，維持原樣。

## 參考第三方專案的原則（Clean Room）

理解 InnerTube 協議格式時，可以參考 chat-downloader、yt-dlp、其他語言的類似專案的公開文件／原始碼**去理解資料格式與端點行為**，但實作本身一律依據自己實際發送請求觀察到的 JSON 結果重新撰寫，不得複製其他專案的程式碼進來。既有程式碼中的 XML 註解會標註「參考：」某網址，代表該處的解析邏輯或欄位命名是依循該資料格式撰寫的說明，不代表程式碼是複製的。

## 已確認支援的 LiveChat 元素種類（2026/8 實測）

`YTJsonParser.ParseLiveChatJson.cs` 目前會解析：一般留言、超級留言、超級貼圖、加入／升級／里程碑會員、贈送會員（`liveChatSponsorshipsGiftPurchaseAnnouncementRenderer`）、接收會員贈送、新版個別小禮物（`giftMessageViewModel`，ViewModel 結構）、置頂／導向橫幅（`addBannerToLiveChatCommand` -> `bannerRenderer`，2026/8 已對真實直播驗證）、跑馬燈（ticker，會取出內嵌的完整原始 Renderer）、捐款／購買／版主／自動版主訊息、創作者投票（`showLiveChatActionPanelAction` -> `pollRenderer`）與投票的即時得票率更新（`updateLiveChatPollAction`，見下方說明）、留言刪除（`removeChatItemAction`）、使用者被封鎖（`removeChatItemByAuthorAction`）、留言被取代／修改（`replaceChatItemAction`，例如超級留言淡出後改為較小樣式）、超級留言／貼圖的排行榜徽章（`leaderboardBadge`，2026/8 新發現，掛在訊息上而非獨立 action，解析出的名次文字對應 `RendererData.LeaderboardRank`）、超級留言的回覆討論串人數更新（見下方「回覆數更新機制」）。已於多支真實直播（VTuber、遊戲台、新聞台、Hololive-EN、歐美遊戲／新聞頻道）上實測驗證。**注意**：判斷「是否已結束」請一律用 `IsVideoStreamingAsync` 或直接查 `liveBroadcastDetails.isLiveNow`，不要只憑「有 Top chat／Live chat 篩選選單」判斷（長時間常態直播與已結束重播都可能同時有這個選單）。「重播」情境目前驗證覆蓋範圍見下方「已知技術債」。

`updateLiveChatPollAction`（2026/8 新增）：投票建立時（`ParsePollRenderer`）只有問題與選項文字，沒有任何票數；YouTube 是透過這個獨立的 action（注意**不是**上面「回覆數更新」用的 `frameworkUpdates` 通用實體更新機制）即時推送 `voteRatio`／`votePercentage.simpleText`，`liveChatPollId` 與建立時相同可用來對照。已對一場真實直播（美國兒童內容頻道，928→959 票、YES 91%／NO 9%）連續輪詢驗證過端到端正確。

**action／附件層級目前沒有「未知類型」的診斷記錄，這是本次發現的一個架構性問題（2026/8 已補上）**：`ParseRenderer`（訊息內容層級）本來就有 `else` 分支記錄「尚未支援的內容」，但 action 層級（`ParseNonMessageAction`）與社群貼文附件層級（`GetBackstageAttachment`）過去完全沒有這層防護——遇到不認識的 action 或附件類型會**靜默丟棄、不留任何記錄**，代表過去所有測試「沒有發現新的未支援類型」這個結論，其實只涵蓋 `ParseRenderer` 這一層。已補上對應的 Trace 記錄（`"ParseNonMessageAction -> 尚未支援的 action 類型"`／`"GetBackstageAttachment -> 尚未支援的附件類型"`），純粹是診斷用途、不影響既有解析行為。實際測試中已用這個新記錄機制抓到一個真實但尚未確認結構的新 action：`updateOrAddInteractivityWidgetAction`（真實直播中只出現過 1 次，重新嘗試擷取完整樣本未再次命中，結構未知，之後若要支援請先用新的診斷記錄收集到真實樣本再實作）。

**使用者被封鎖（暫時 timeout vs 永久封鎖）的區分能力**：`removeChatItemByAuthorAction` 目前只讀取 `externalChannelId`，程式碼裡沒有解析任何「封鎖類型」或「持續時間」欄位。查證官方 YouTube Data API v3 文件，`banType`（`PERMANENT`／`TEMPORARY`）只存在於需要 OAuth 授權的 `liveChatBans`／`liveChatMessages.snippet.userBannedDetails` 這組**官方**端點，跟本函式庫使用的 InnerTube 非官方端點是完全不同的資料來源；交叉參考 chat-downloader 專案對同一批 action 類型（`removeChatItemAction`／`removeChatItemByAuthorAction`／`markChatItemsByAuthorAsDeletedAction`／`markChatItemAsDeletedAction`）的獨立分析，也確認其實作**沒有**解析任何 banType／duration 欄位。本次會話累計在 9 個真實直播（涵蓋新聞台、政論節目、新人 VTuber 首播、大型內容頻道等多種觀眾與版主行為模式）上連續輪詢共約 45 分鐘（含一次對已知有活躍板主刪言的東森新聞頻道特別延長到 8 分鐘的觀察），只捕捉到訊息刪除（`removeChatItemAction`）與跨頻道導流橫幅（`addBannerToLiveChatCommand` -> `liveChatBannerRedirectRenderer`，`bannerType: "LIVE_CHAT_BANNER_TYPE_CROSS_CHANNEL_REDIRECT"`，已確認是既有 `ChatRedirect` 類型會正確處理的情境），**完全沒有捕捉到任何一次 `removeChatItemByAuthorAction`（使用者封鎖）事件**，因此依然**未能親自取得真實的封鎖 action 原始 JSON 做最終確認**。這個結果本身有其意義：封鎖事件在真實直播裡的發生頻率遠低於單純刪言，用隨機時間點連續輪詢的方式很難統計上巧遇；依據前述官方 API／chat-downloader 交叉證據，暫時／永久封鎖的區分很可能在 InnerTube 這一層本來就不存在，不是本函式庫遺漏解析，但這個結論目前仍建立在間接證據上。另外，`markChatItemsByAuthorAsDeletedAction`／`markChatItemAsDeletedAction` 這兩個 chat-downloader 有處理、但本函式庫完全沒有實作的舊版 action 名稱，這次也沒有觀察到——若之後真的出現，會被上面新增的診斷記錄捕捉到。**若之後要繼續追查，比起隨機輪詢，更有效的做法是找一個自己能控制、可以主動觸發封鎖／timeout 動作的測試直播（例如自己開一場測試直播、用另一個帳號發言後自行封鎖），直接觀察 action 的完整原始 JSON，而不是被動等待真實直播剛好發生。**

尚未實作的項目：

- `creatorHeartViewModel`：2026/8 已在真實資料中觀察到，結構掛在 `liveChatPaidMessageRenderer.creatorHeartButton.creatorHeartViewModel` 底下（如先前猜測，附加在既有超級留言訊息上而非獨立 action）。但目前只確認得到「這則訊息具備愛心按鈕」的靜態定義（`heartedIcon`／`unheartedIcon`／`heartedHoverText` 等），抓不到「這則訊息現在是否已被創作者比愛心」這個動態狀態。**2026/8 後續更新：已確認根本原因並排除實作可能性**——這個狀態走的是跟下方「回覆數更新機制」相同的 `frameworkUpdates.entityBatchUpdate` 實體更新架構，對應的實體酬載是 `engagementToolbarStateEntityPayload`；直接對一場有 Super Chat 活動的真實直播（Hololive-EN）連續輪詢驗證過，這個酬載**只有一個不透明的 `key` 欄位，沒有任何布林值或狀態欄位**，因此無論怎麼解析都無法判斷「已被愛心」或「未被愛心」，**確認無法實作**，不是尚未找到而是資料本身就不包含這個資訊。
- `pointsButton` / `liveViewerLeaderboardChatEntryPointViewModel`（"Top fans" 觀眾排行榜進入點）：這是聊天室**標頭**的靜態導覽按鈕（點擊會開啟 `ENGAGEMENT_PANEL_SURFACE_LIVE_CHAT` 面板），不是逐則訊息的 action，本身沒有「訊息內容」可解析，不在 `ParseActions`／`ParseRenderer` 的處理範圍內（跟上面 `leaderboardBadge` 這個掛在個別訊息上的排行榜徽章是兩回事，不要混淆）。它在 `frameworkUpdates.entityBatchUpdate` 底下對應的實體酬載是 `liveViewerLeaderboardChatEntryPointStateEntity`（只有一個列舉狀態，例如 `LIVE_VIEWER_LEADERBOARD_CHAT_ENTRY_POINT_STATE_POINTS_AVAILABLE`），同樣是標頭層級的狀態、不屬於任何一則訊息，維持排除。
- `emojiFountainDataEntity`：2026/8 新發現，`frameworkUpdates.entityBatchUpdate` 底下的另一種實體酬載，是直播間共用的「表情雨」環境特效資料（`reactionBuckets` 陣列記錄近期各時間窗的表情反應強度），不屬於任何一則訊息、也不是逐則訊息的 action，性質上更接近直播環境資訊而非聊天室訊息，刻意不處理。

## 回覆數更新機制（`ParseFrameworkUpdates`，2026/8 新增）

2026/8 實測發現 YouTube 為超級留言／超級貼圖新增了「回覆討論串」功能（`liveChatPaidMessageRenderer.replyButton.pdgReplyButtonViewModel`，點擊會開啟 `ENGAGEMENT_PANEL_SURFACE_LIVE_CHAT` 面板、`tag: "PAreply_thread"`）。這個回覆數**不是**內嵌在訊息 JSON 裡的靜態數字，而是 YouTube 一套通用的「實體」（entity）即時更新機制：訊息只帶一個不透明的 `replyCountEntityKey`（對應 `RendererData.ReplyCountEntityKey`），實際數字要等到某一批 `get_live_chat` 回應的**頂層**（`continuationContents`的同層級，不在 `actions` 陣列裡）出現 `frameworkUpdates.entityBatchUpdate.mutations`，裡面某筆 `entityKey` 等於這個 key、`payload.replyCountEntity.replyCountNumber` 才是目前的數字。

`ParseActions` 因此在處理完 `actions` 陣列後，額外呼叫 `ParseFrameworkUpdates` 解析同一份頂層回應的 `frameworkUpdates`，把每筆 `replyCountEntity` 突變包成一筆獨立的 `RendererData`（`Type` 為本地化過的「回覆數更新」、`ID` 借用來存放 `entityKey`、`ReplyCount` 存放新數字，其餘欄位為空）附加進同一批輸出——呼叫端要自行記住每則付費訊息的 `ReplyCountEntityKey`，日後看到 `ID` 相符的「回覆數更新」項目時，自行把 `ReplyCount` 更新回原本那則訊息上（作法上跟既有的 `removeChatItemAction`／`replaceChatItemAction`「借用 `ID` 做關聯」是同一套模式，不是新發明的機制）。已對真實有 Super Chat 活動的直播（Hololive-EN）連續輪詢驗證過，能正確抓到會隨時間變化的回覆數（例如同一個 `entityKey` 從初始的 `"0"` 到之後的 `"36"`）。

同一個實體更新機制底下還有另外兩種酬載類型（`engagementToolbarStateEntityPayload`、`liveViewerLeaderboardChatEntryPointStateEntity`、`emojiFountainDataEntity`），皆已實測並確認不屬於「可解析的逐則訊息資料」，見上方「尚未實作的項目」。

## 社群貼文分頁尋找機制（`GetCommunityTab`，2026/8 修正）

實測發現 YouTube 對社群分頁的回應，現在只會在 `tabs` 陣列內回傳**單一、已經選取**的分頁，不再像過去一樣把所有分頁都塞進同一份回應，也不再穩定提供可比對舊網址格式的 `tabRenderer.endpoint.commandMetadata.webCommandMetadata.url`（該欄位常常整個不存在）。舊版靠網址比對的 `GetCommunityTab` 因此完全失效——對任何 2026 年的頻道都會回傳 `null`，導致社群貼文擷取從頭到尾拿不到任何資料。已修正為優先找 `tabRenderer.selected == true` 的分頁，找不到才退回舊版網址比對，最後才退回「若整份回應只有一個分頁就直接視為社群分頁」。已用 10 個真實頻道驗證修正後可正常運作（image／video／poll 三種附件皆有實測樣本，皆已支援）。

**請求網址：`/community` → `/posts`（2026/8 修正，重要）**：`GetYTConfigDataAsync`（`YTJsonParser.Core.cs`）原本用 `/channel/{id}/community` 抓社群分頁。實測對 5 個頻道抽測比較 `/community` 與 `/posts` 兩種網址，發現**其中 2 個頻道（Kurzgesagt、米妃Tobi）直接請求 `/community` 會回傳「沒有貼文」的空狀態訊息（`contents.messageRenderer`），即使該頻道其實有大量貼文**（改用 `/posts` 各自正常抓到 174／10+ 篇）；另外 3 個頻道兩種網址皆可正常運作（`/community` 甚至會回傳稍多一點的初始項目，但差異僅止於分頁大小，不影響完整擷取的正確性）。已全面改用 `/posts`，這是 YouTube 官方文件也已採用的新網址（`/community` 是舊名稱）。**這是一個先前完全沒被注意到的靜默失敗**：受影響的頻道用舊版程式碼會直接回傳 0 篇貼文、沒有任何錯誤訊息，容易被誤判為「這個頻道沒有社群貼文」。

## 社群貼文類型（2026/8 新增支援）

- **測驗貼文（`quizRenderer`）**：YouTube 於 2025 年起逐步開放的社群貼文測驗功能，`backstageAttachment.quizRenderer` 底下的 `choices[]` 每個選項多了 `isCorrect`（布林值），且沒有 `numVotes`／`votePercentage`（實測確認：測驗選項在作答前不會透露即時票數分布）；問題文字本身沿用貼文既有的 `contentText`（跟一般文字貼文同一個欄位），不在 `quizRenderer` 內。解析結果沿用既有的 `PollData`／`ChoiceData` 模型（新增 `ChoiceData.IsCorrect`、`AttachmentData.IsQuiz`），因為兩者資料形狀相同，沒有另外建立 QuizData 模型的必要。已對 3 個真實頻道（Trivia Quiz Channel、Veritasium、Buzzfeed Quiz）驗證存在，並對 Trivia Quiz Channel 完整驗證解析正確（2 篇真實測驗貼文，選項與正確答案皆吻合原始 JSON）。
- **轉發貼文（`sharedPostRenderer`）**：YouTube 的「在 YouTube 上轉發」（Repost on YouTube）功能。**注意**：這不是 `backstageAttachment` 底下的附件類型，而是 `backstagePostThreadRenderer.post` 底下**取代** `backstagePostRenderer` 的另一種可能值（兩者互斥，只會有一個）。結構：`post.sharedPostRenderer` 本身帶有 `displayName`（轉發者）、`content`（轉發時附加的文字，可為空）、`publishedTimeText`（轉發時間）、自己的 `postId`，並巢狀包一份完整的 `originalPost.backstagePostRenderer`（被轉發的原始貼文，結構與一般貼文完全相同，可直接沿用既有的作者／內容／附件解析函式）。`GetBackstagePostRenderer` 已修正為在 `post.backstagePostRenderer`不存在時，退回 `post.sharedPostRenderer.originalPost.backstagePostRenderer`，讓 `PostData` 的 `AuthorText`／`ContentTexts`／`Attachments` 等欄位語意上永遠代表「貼文本身的內容」；轉發中繼資料另外存在新增的 `PostData.IsRepost`／`RepostedByAuthorText`／`RepostCaptionTexts`。已對真實跨頻道轉發（Kurzgesagt 主頻道轉發子頻道「Nightshift – Kurzgesagt After Dark」的 3 篇貼文）完整驗證：`RepostedByAuthorText`（轉發者）與 `AuthorText`（原始作者）正確地是兩個不同的頻道名稱，`RepostCaptionTexts`（轉發附加文字）與 `ContentTexts`（原始內容）也正確地是兩段不同文字，附件（圖片）也正確從原始貼文解析出來——這組真實樣本剛好是跨頻道轉發而非自我轉發，完整驗證了轉發者／原始作者的欄位對應關係沒有猜錯。

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

`GetVideoTitleAsync`（2026/8 修正）：舊版直接讀取 `/watch` 頁面 HTML 的 `<title>` 標籤（`Element.InnerHtml`），
有兩個問題：(1) `<title>` 內容固定帶有「 - YouTube」字尾；(2) `Element.InnerHtml` 回傳的是重新序列化過的
HTML markup，字面上的 `&`／`<`／`>` 等字元會維持 HTML 實體逸出（例如 `&amp;`），不是解碼後的原始文字——已用
throwaway 測試（AngleSharp `HtmlParser` 解析含 `&amp;`／`&lt;`／`&gt;` 的 `<title>`，`InnerHtml` 回傳的仍是逸出後的字串，`TextContent` 才是解碼後的正確文字）證實這是真實問題，只要標題含有這類符號就會回傳錯誤字串。已改為比照 `IsVideoStreamingAsync` 解析同一頁面的 `ytInitialPlayerResponse.videoDetails.title`（JSON 字串值本來就是解碼後的文字，也沒有「 - YouTube」字尾問題）。

## 輪詢頻率安全下限

`IntervalMs` 屬性（`YTJsonParser.PublicStatic.cs`）在未設定 `ForceIntervalMs` 時，會取「YouTube 回應解析出的間隔值」與 `MinimumIntervalMs`（1000 毫秒）兩者中較大的值，避免因回應內容解析失敗等異常狀況導致間隔值意外停留在 0，對 YouTube 形成近乎無間隔的高頻輪詢。這個下限不會套用在您已明確設定的 `ForceIntervalMs` 上（視為刻意的選擇）。修改輪詢相關邏輯時，請維持「正常情況下遵循 YouTube 回應建議的間隔、異常情況下也不會緊迫輪詢」這個原則，不要為了效能把這個安全下限拿掉。

## 已知的行為變化（2026/8 重構後）

`YTJsonParser` 的內部記錄（原本會透過 `OnLogOutput` 事件轉發到 WinForms 應用程式自己的 `TBLog` 文字框）現在只會寫進標準 `ILogger`（實際落地在 NLog 的 `Logs/log.txt` 檔案與主控台輸出），**不會**再自動出現在應用程式的記錄文字框裡。如果之後想在 UI 文字框也看到，需要另外接一個自訂 NLog target 把 `YTJsonParser` 這個 logger 分類的訊息轉發過去，這次重構刻意沒有做這件事（改用標準記錄架構後，「查看記錄檔」才是預期的除錯方式）。

## WinForms 端消費 RendererData 的正確方式（`FMain.Methods.cs`，2026/8 修正）

`YTJsonParser` 的 `RendererData` 裡有幾種類型本質上是「以 `ID`（或其它欄位）關聯回既有訊息的更新／刪除事件」，不是獨立的新留言：`留言已被刪除`（`ID` = 目標訊息 ID）、`使用者已被封鎖`（`AuthorExternalChannelID` = 被封鎖使用者的頻道 ID）、`回覆數更新`（`ID` = `ReplyCountEntityKey`，需要對照原始付費訊息自己的 `ReplyCountEntityKey` 欄位）、`投票結果更新`（`ID` = 建立投票時的同一個 `liveChatPollId`），以及 `replaceChatItemAction` 產生的「同一個 `ID` 再次出現」情境（例如超級留言／貼圖淡出後改為較小樣式）。

2026/8 之前，`DoProcessMessages` 完全沒有處理這個關聯語意，把上述每一種都當成全新留言加入 `ListView`——實際效果是畫面上會多出顯示原始 ID／頻道 ID 字串或近乎全空白的垃圾列，且會虛灌「留言數量」／「留言人數」統計，Excel 匯出（直接鏡射 `ListView` 內容）也會原封不動地把這些垃圾列匯出。已修正為：

- 新增 `SharedItemsByMessageID`／`SharedItemsByReplyCountEntityKey`／`SharedItemsByAuthorChannelID` 三個字典（`FMain.Variables.cs`），在建立新列時同步登記，讓上述事件能 O(1) 找到對應列（而不是線性掃描整個 `ListView`），`BtnClear_Click` 清空聊天室時務必一併清空這三個字典。
- `留言已被刪除`／`使用者已被封鎖`：找到對應列後，在訊息內容前面加上文字標記（例如「〔已刪除〕」）並套用刪除線字型＋灰色，**保留原始列**供封存／匯出使用（不是直接移除該列——這個工具的用途包含記錄／收益分析，刪除的留言本身也是有價值的資訊）。標記文字特意寫進訊息內容欄位本身而不是只靠字型樣式，因為 Excel 匯出目前不會轉存字型的刪除線樣式。
- `回覆數更新`／`投票結果更新`：找到對應列後就地更新回覆數／得票結果欄位文字，不產生新列。
- 同一個 `ID` 再次出現時（真重複資料，或 `replaceChatItemAction`）：就地更新既有列的訊息內容／金額／顏色等欄位，而不是略過（避免 replace 的新內容被靜默丟棄）或加入看似重複的新列。
- 新增 `RendererData.LeaderboardRank`／`ReplyCount`／`HeaderBackgroundColor`／`ReplyCountEntityKey` 對應的 `ListView` 欄位（先前這幾個函式庫已經提供的欄位完全沒有接到 UI 上）；`HeaderBackgroundColor` 套用在作者名稱／徽章／金額／時間這幾個「標頭」欄位的背景色，呈現跟真實 YouTube 超級留言一樣的標頭／內文雙色設計。
- `UpdateSummaryInfo`／Excel 匯出的內容分頁／時間熱點分頁，都補上這四種類型的排除條件（正常情況下這幾種事件現在不會再變成獨立列，這裡的排除純粹是防禦性處理，避免舊版匯出檔案匯入後殘留資料虛灌統計）。
- `FMain.EPPlusUtil.cs` 的 `LoadXLSX`（匯入）與 Excel 匯出的 `widthSet` 欄寬陣列都同步補上新欄位，`LoadXLSX` 讀取舊版（沒有這幾欄）匯出的 *.xlsx 檔案時會安全地讀到空字串，不會出錯。

這次修正只用建置＋既有的 `YTJsonParser.Tests`（不受影響，仍 8/8 通過）＋啟動應用程式確認無啟動期例外做驗證；`YTLiveChatCatcher` 專案本身沒有自動化測試（`ListView`／`FMain` 高度耦合、需要 STA 訊息迴圈），完整的即時串流＋刪除／封鎖／回覆數／投票更新情境的視覺驗證，需要實際連上真實直播手動操作，這次沒有做到這一步。

### `UpdateSummaryInfo` 效能修正（2026/8，累加式計數器取代每批次重新掃描）

舊版 `UpdateSummaryInfo` 每次收到新批次都會對整個 `LVLiveChatList.Items`（可能累積上千則訊息）用 LINQ `Where(...).Count()` 重新掃描約 10 次（留言數量、超級留言／貼圖數量、五種會員事件數量、會員人數、留言人數、收益加總），長時間直播下來是隨訊息數量成長的 O(n²)。已改為：

- 新增一組累加式計數器／集合（`FMain.Variables.cs`：`SharedChatCount`／`SharedSuperChatCount`／...／`SharedTotalIncome`／`SharedMemberInRoomAuthors`／`SharedDistinctAuthors`），只在 `RegisterNewListViewItemStats`（`FMain.Methods.cs`）裡更新，條件逐一對照舊版 `UpdateSummaryInfo` 的篩選邏輯改寫，確保計算結果一致。
- `RegisterNewListViewItemStats` 只在真正新增一列時呼叫一次（`DoProcessMessages`／`LoadXLSX` 各一處）；就地更新既有列（刪除／封鎖標記、`ApplyExistingListViewItemUpdate`）不會呼叫，避免同一則訊息被重複計算。
- `UpdateSummaryInfo` 本身改成純粹讀取這些欄位組字串，不再掃描 `ListView`，方法本身變成 O(1)。
- `BtnClear_Click` 清空聊天室時，務必在呼叫 `UpdateSummaryInfo()` **之前**把這些計數器／集合歸零／清空，順序寫反的話畫面會先短暫顯示清空前的舊數字。
- 若未來又要新增一種會影響統計的訊息類型，記得同時更新 `RegisterNewListViewItemStats`，否則新類型不會反映在統計數字裡（這是這個設計相對於「每次重新掃描」的取捨：正確性依賴人工同步維護兩處邏輯，而不是單一事實來源）。

## 已知技術債（非本次任務範圍，供後續參考）

- `EPPlus` 使用 Polyform Noncommercial 授權（`ExcelPackage.License.SetNonCommercialOrganization(...)`，`FMain.EPPlusUtil.cs`／`FMain.Methods.cs` 各呼叫一次，分別對應匯入／匯出兩個獨立進入點，非重複程式碼）。2026/8 已確認目前為最新穩定版（8.7.0），且本專案為免費、非商業性質，符合此授權條款；商業用途需另外購買授權，之後更新版本前請留意授權條款是否變動。
- LiveChat 的「重播」情境：2026/8 已實際測試 8 支不同頻道（Hololive 官方 VTuber、獨立 VTuber Ironmouse、西方遊戲實況主 Asmongold、台灣實況主館長）已確認結束的直播（`isLiveNow=false` 且有 `endTimestamp`），**全部**的重播聊天室（`GET /live_chat?is_popout=1&v={videoID}`）都回傳關閉訊息（`contents.messageRenderer`，無 `liveChatRenderer`／`liveChatReplayContinuationData`）。依 YouTube 官方說明（[Learn about Live Chat](https://support.google.com/youtube/answer/15268877)），原因是**創作者端設定**：頻道可在「自訂管道」關閉「即時聊天室重播」、對直播影片做過剪輯（video editor 編輯過的影片一律沒有聊天室重播）、或事後手動刪除／設為會員限定，且不會通知觀眾。之後若要驗證，優先找**未剪輯的個人／小型頻道**且結束不久的直播，且驗證前務必先用 curl 或瀏覽器直接檢查 `live_chat?is_popout=1` 回應內是否真的有 `liveChatRenderer`，不要直接假設熱門頻道會開放。
  - **2026/8 後續更新：已找到並完整驗證過一次。** 從約 280 支「已結束、搜尋結果偏個人／小型頻道」的候選影片中，只有 2 支（約 0.7%）重播聊天室仍開放——比例非常低，印證上面「熱門頻道普遍關閉」的觀察，個人／小型頻道也大多關閉。其中一支（約 4.7 小時的個人頻道長時間直播）成功用 `StreamLiveChatDataAsync`（不設任何人工截斷時間）完整跑到 `continuation` 自然耗盡結束，2 個批次、16 則訊息、耗時僅 41 秒——**證實重播的輪詢並非按原始直播的即時步調傳送，而是遠快於即時**（回應間隔仍會回報 1~10 秒，但每次回應內含的訊息量遠超過即時直播同樣間隔會有的量），因此消費一整場重播在實務上是可行的，不需要等待影片原始長度的時間。另一支候選影片則是在「curl 驗證聊天室開放」與「實際跑測試」中間的十幾分鐘內被關閉（`contents.messageRenderer` 回傳「這部直播影片的聊天室已停用」）——**這不是程式碼的 bug**，`ParseStreamingContinuation` 找不到 continuation 而記錄 `LogMessages.Error` 是正確、預期的行為；但這是一個活生生的例子，證實「重播聊天室視窗可能在你檢查完之後、還沒來得及測試前就被關閉」是真實會發生的情況，不只是理論上的風險，日後找測試樣本要有這個心理準備（愈快驗證愈好，不要間隔太久才實際測試）。
