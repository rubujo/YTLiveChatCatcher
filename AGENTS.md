# AGENTS.md

給所有 AI 編碼代理（Claude Code、Codex、Cursor、Copilot、Antigravity CLI 等）使用的專案規範。這份檔案是跨工具的共用事實來源；`CLAUDE.md` 僅是指向這裡的精簡入口。

這份文件只記錄「程式碼／git 歷史看不出來」的決策、限制與陷阱，不記錄變更日誌——已完成的修正、實測過程、調查細節請查 `git log`，不要疊加在這裡。新增內容前，先問自己：這是未來的代理需要知道才能做對決策的事，還是單純在記錄「我做了什麼」？只有前者才寫進來。

## 專案是什麼

「YouTube 聊天室捕手」：模擬 YouTube 直播聊天室網頁版的行為，取得直播中或重播影片的聊天室內容，並提供收益分析、報表匯出等功能。詳見 [README.md](README.md)。

三個專案：

- `YTJsonParser/`：核心函式庫。負責向 YouTube 發送請求、解析回傳的 JSON（ytcfg、ytInitialData、innertube API 回應），對外提供強型別的 `RendererData`／`PostData`／`YTConfigData` 等模型，以及 `StreamLiveChatDataAsync`／`StreamCommunityPostsAsync` 兩個 `IAsyncEnumerable` 拉取式串流方法。不依賴任何 UI 框架、不依賴 DI 容器，理論上可被任何 .NET 應用程式引用。
- `YTLiveChatCatcher/`：WinForms 桌面應用程式，使用 `YTJsonParser` 的單一實例（`FMain.Variables.cs` 內的 `SharedYTJsonParser`，於 `InitLiveChatCather` 建立）驅動 UI，透過 `Task.Run` + `await foreach` 消費串流。
- `YTJsonParser.Tests/`：xUnit v3（`xunit.v3` 套件，非舊版 `xunit`）測試專案，用 `FakeHttpMessageHandler` 依 URL 比對回傳 `Fixtures/` 內的真實 JSON 樣本，完全不連線即可驗證解析邏輯。
- `YTLiveChatCatcher.Tests/`：同樣 xUnit v3，測試 `YTLiveChatCatcher/Common/Utils/ChatStatsCalculator.cs`——WinForms 端跟 `Control`／`Form` 完全脫鉤的純計算邏輯（金額解析、訊息類型分類），見下方「WinForms 端的純計算邏輯」。

## 建置、執行與測試

```bash
dotnet build YTLiveChatCatcher.slnx
dotnet run --project YTLiveChatCatcher
dotnet test YTJsonParser.Tests/YTJsonParser.Tests.csproj
dotnet test YTLiveChatCatcher.Tests/YTLiveChatCatcher.Tests.csproj
```

`dotnet test` 需要 repo 根目錄的 `global.json`（`{"test":{"runner":"Microsoft.Testing.Platform"}}`）才能在 .NET 10 SDK 下運作——xunit.v3 是 Microsoft.Testing.Platform 原生專案，.NET 10 的 `dotnet test` 預設仍嘗試走舊版 VSTest 橋接、會直接失敗，不要移除這個 `global.json`。也可以直接 `dotnet run --project YTJsonParser.Tests`（或 `YTLiveChatCatcher.Tests`）以主控台方式執行（xunit.v3 內建的 in-process runner）。

新增測試時：`YTJsonParser` 的解析方法幾乎都是 `private`，測試一律透過**公開 API**（`StreamLiveChatDataAsync`／`StreamCommunityPostsAsync`／`IsVideoStreamingAsync`）搭配 `FakeHttpMessageHandler` 進行，不要為了測試把方法改成 `internal` + `InternalsVisibleTo`——這樣測試才能在內部實作重構後仍然有效，同時也順便驗證了 URL 組成與 continuation 交換邏輯（這是最容易被 YouTube 改版打壞的部分）。新增 fixture 時，**用程式（例如 Python 的 `json.dumps`）產生保證合法的 JSON 字串再嵌入 HTML**，不要手動在一行內編織巢狀大括號／中括號，容易漏算括號讓 fixture 壞掉。

`YTLiveChatCatcher` 本身（`ListView`／`FMain`）沒有自動化測試——高度耦合、需要 STA 訊息迴圈，這部分改動後至少要建置＋啟動應用程式確認無啟動期例外；牽涉即時串流／匯入匯出的改動，理想上要接真實直播手動操作驗證，做不到時要在回報裡明講「沒做到這一步」，不要含糊帶過。但**其中不依賴 `Control`／`Form` 的純計算邏輯要抽到獨立類別、由 `YTLiveChatCatcher.Tests` 覆蓋**，不要因為「反正 WinForms 沒辦法測」就連可以測的部分也一起放棄——`ChatStatsCalculator` 就是這樣抽出來的，且抽出來的過程直接消滅了一個真實 bug 的重演可能（見下方）。

## 目標框架與語言版本

- .NET 10.0 LTS（`net10.0-windows7.0`），C# 14（隨 SDK 預設）。
- `Nullable` 與 `ImplicitUsings` 皆為 `enable`。
- 檔案範圍命名空間（`namespace Foo;`，不用大括號包整個檔案）。
- `YTJsonParser` 類別用多個 partial class 檔案依職責切分（`YTJsonParser.Core.cs`、`YTJsonParser.Common.cs`、`YTJsonParser.ParseLiveChatJson.cs`、`YTJsonParser.PublicStatic.cs` 等）——新增方法時找對應職責的檔案放，不要都塞進 `YTJsonParser.cs`。

## 核心程式庫的架構

- **建構子**：`new YTJsonParser(YTJsonParserOptions? options, ILogger<YTJsonParser>? logger)`。`YTJsonParserOptions` 是建構後不可變的 `record`（`HttpClient`、`DisplayLanguage`、`FetchLargePicture`）。唯一例外是 `Cookies`——使用者常在同一個 session 內動態切換／重新登入，所以刻意設計成可變屬性（`ytJsonParser.Cookies = "..."`），不併入不可變的 options，理由寫在 `YTJsonParser.PublicStatic.cs` 的 `Cookies` 屬性註解上。
- **`IAsyncEnumerable<T>` 串流**：`StreamLiveChatDataAsync`／`StreamCommunityPostsAsync` 回傳 `IAsyncEnumerable<IReadOnlyList<T>>`（每次列舉＝一次輪詢取得的一批資料，不是逐則訊息——YouTube 本來就是批次回應，逐則 yield 只會讓 WinForms 端被迫逐則 `Invoke`）。取消完全交給呼叫端自己的 `CancellationTokenSource`，函式庫不保管 `Task`／`CancellationTokenSource`／`Stop()`。`LiveChatStreamOptions`／`CommunityPostStreamOptions` 是每次呼叫的區域參數，不是共用可變狀態——同一個 `YTJsonParser` 實例可以同時執行多個獨立串流，彼此不互相干擾。
- **`ILogger`**：建構子接受 `ILogger<YTJsonParser>`（預設 `NullLogger`）。內部記錄呼叫集中在 `LogMessages.cs`（`[LoggerMessage]` source-generated partial method），大部分呼叫點用通用範本（`LogMessages.Error/Warning/Debug/Trace`），少數語意明確的事件有專屬方法。新增記錄呼叫時比照這個分法，不要退回手動組字串的 `_logger.LogXxx($"...")`。
- **輪詢間隔回報**：`StreamLiveChatDataAsync` 有一個 `IProgress<int>? intervalProgress` 參數，每次輪詢間隔更新時呼叫 `.Report(intervalMs)`——WinForms 端用它更新「下次擷取還要等幾秒」文字框。
- **純工具函式**：`GetYouTubeChannelID`／`GetYouTubeVideoID`／`GetYouTubeChannelUrl`（`Utils/YouTubeUrlUtil.cs`）都是不依賴 instance 狀態的 `static` 方法，呼叫時不需要（也不應該透過）`YTJsonParser` 實例。
- **`ConfigureAwait(false)`**：函式庫內部所有 `await` 呼叫都要加，避免不必要地跳回呼叫端的 `SynchronizationContext`（對 WinForms 這種有 UI 執行緒的消費端尤其重要）。

## 顯示語言（`YTJsonParserOptions.DisplayLanguage`）

型別是 `EnumSet.DisplayLanguage?`，不指定時建構子呼叫 `LangUtil.GetDisplayLanguageFromCulture()` 依 `CultureInfo.CurrentUICulture` 自動判斷（完整文化特性名稱 → 中文特例走 `CultureInfo.Parent` 鏈判斷 zh-Hans／zh-Hant → 主要語言代碼 → 找不到退回英文，細節見該方法的 XML 文件註解）。`YTLiveChatCatcher` 明確指定 `Chinese_Traditional`，不受自動判斷影響；其他消費端若沒有明確指定，行為會依執行環境而變。測試見 `YTJsonParser.Tests/LangUtilTests.cs`。

**`DisplayLanguage` 換算出的 `hl`／`gl`（`RegionData`）不是只影響本函式庫自己標籤要顯示的文字，也會實際改變 YouTube 回應內容本身的格式**——這點容易被忽略，因為大部分欄位（訊息內文、時間標記）看起來只是「換一種語言顯示」，但金額這類欄位的格式差異足以造成統計錯誤，見上面「顯示語言」文件註解與下面「WinForms 端的純計算邏輯」段落的 `TryParsePurchaseAmount` 說明。新增任何會解析 YouTube 回應文字內容（不是純結構／ID／數值欄位）的邏輯時，先假設格式會隨 `hl`／`gl` 改變，需要時直接對同一筆真實資料換不同 `hl` 重新請求驗證，不要只用單一語系測試過就當作通用。已針對整個核心程式庫掃過一輪同類風險（`grep` 所有 `int/double/long.Parse`／`.Contains(GetLocalizeString(...))` 呼叫點）：數字解析全部只用在結構層級的欄位（`timestampUsec`、輪詢間隔毫秒值），不是使用者看得到的顯示文字，沒有這個問題；`ParseSubMenuItemsContinuation` 的預設路徑用陣列索引而非文字比對，語系無關；唯一另一個「比對 YouTube 渲染文字」的地方是 `SetRendererData` 內判斷 `liveChatMembershipItemRenderer` 是加入／升級／里程碑的關鍵字比對（`KeySet.MemberUpgrade`／`KeySet.MemberMilestone`，已有 5 種語言的 `DictionarySet` 翻譯）——這個機制本身就是設計成語系感知的，不是像金額那樣完全沒考慮語系；但這次審查用兩場真實直播、累計約 7 分鐘輪詢只捕捉到「加入會員」事件（9 筆，皆正確落入預設分類），沒能捕捉到真正的升級或里程碑事件，因此**目前用的關鍵字文字本身是否仍與 YouTube 現況相符，尚未被這次驗證直接證實**，之後若要繼續確認，需要找真的會出現升級／里程碑事件的直播（例如創作者自己測試觸發）。

## 取得會員限定內容用的 Cookie

`YTJsonParser` 本身**不提供**任何讀取或解密瀏覽器（Chrome／Edge／Firefox）Cookie 資料庫的方法。原因：

1. **技術上已失效**：Chrome 127+（2024/7 起）與新版 Edge 預設啟用 App-Bound Encryption，一般使用者權限的 DPAPI 解不出正確金鑰。
2. **與竊資軟體技術特徵重疊**：直接解密另一個應用程式私有憑證資料庫，即使無惡意仍容易被防毒軟體誤判、被上架平台拒絕。

`YTLiveChatCatcher` 改用官方支援的介面，兩條路徑互為備援：

- **`FCookieLogin`（主要）**：應用程式專屬的 `Microsoft.Web.WebView2` 登入視窗，使用**自己專屬的 user data folder**（`%LocalAppData%\YTLiveChatCatcher\WebView2Profile`，刻意不指向使用者既有的 Edge／Chrome profile）。登入後透過官方 API `CoreWebView2CookieManager.GetCookiesAsync(...)` 取得 Cookie，不受 App-Bound Encryption 影響，也不會碰到使用者日常瀏覽器的資料。
- **手動貼上（備援）**：`FCookieLogin` 提供文字欄位讓使用者自行貼上 Cookie 字串，WebView2 不可用時仍有路可走。

儲存：預設只存在記憶體（`SharedYTJsonParser.Cookies`），關閉程式即遺失；勾選「記住我」才會透過 `Common/Utils/SecureCookieStore.cs` 以 **DPAPI（`CurrentUser` scope）加密**寫入 `%LocalAppData%\YTLiveChatCatcher\cookie.dat`（加密自己要存的東西，跟上面「解密別人已加密的資料」方向相反，只有同一台機器、同一個 Windows 使用者才能還原）。「登出／清除已儲存資料」會同時清掉 WebView2 profile 的 Cookie 與這個加密檔案。

## 程式風格

- 註解與 XML 文件註解一律使用繁體中文。
- 只在「為什麼」不明顯時才寫註解（隱藏限制、繞過某個特定 bug 的原因），不要寫「這段程式碼在做什麼」這種顯而易見的說明。
- 解析 YouTube 原始 JSON（`*Renderer`／`*ViewModel`）時，**刻意使用 `JsonElement.Get(...)` 防禦式走訪**（見 `Extensions/JsonElementExtension.cs`），不要為這一層改寫成強型別 DTO 直接反序列化——原因見下方「YouTube 端點沒有官方文件」。穩定、簡單的設定資料（例如 ytcfg 的 `INNERTUBE_API_KEY`，見 `Models/YtCfgDto.cs`）可以、也應該用強型別 DTO + `JsonElement.Deserialize<T>()`。對外公開的模型（`RendererData`、`YTConfigData`、`PostData` 等）永遠是強型別、有 `[JsonPropertyName]` 標註的 C# 類別。
- 解析 `ytcfg.set({...})`／`ytInitialPlayerResponse` 這類內嵌在 HTML `<script>` 裡的 JSON 時，用 `ExtractBalancedJsonObject`（括號配對），不要用字串裁切（`Replace` + `LastIndexOf`／裁切最後一個 `;`）——這類物件常含巢狀大型字串（例如 SVG），字面上剛好出現裁切用的分隔符會直接截斷錯誤。
- WinForms 端跨執行緒更新 UI：背景執行緒（`Task.Run` 內）且會頻繁或處理較多資料的更新用 `ControlExtension.InvokeAsyncIfRequired`（`Control.InvokeAsync`，非阻塞，`await` 等待完成但不佔用執行緒集區執行緒）；由 UI 執行緒直接呼叫的事件處理常式、或低頻／單一控制項的小更新，維持用 `InvokeIfRequired`（`Control.Invoke`，阻塞版本，程式碼更單純）即可，不必為了一致性把所有呼叫點都改成非同步鏈。清理／還原 UI 狀態這類「不論成功或取消都要執行」的收尾動作，呼叫 `InvokeAsyncIfRequired` 時不要帶入可能已取消的 `CancellationToken`。
- **不要用 `Task.ContinueWith(...)` 做「不論成功失敗都要收尾」的邏輯**——預設的 `ContinueWith` 不論前面的 `Task` 是成功、失敗還是取消都會執行，且回傳的新 `Task` 只反映 `ContinueWith` 委派本身的結果，不會反映前面 `Task` 的例外；如果外層有 `await`，前面 `Task` 拋出的例外會被整個吞掉，外層的 `catch` 永遠攔不到，畫面上可能還顯示成功訊息。這個 bug 曾經同時出現在 `FMain.cs`（匯出／匯入）跟 `FSearch.cs`（匯出）四個地方，其中兩處匯入流程甚至完全沒有 `await`，是純粹的 fire-and-forget。一律改用 `try { await X(); } finally { 收尾邏輯 }`，讓例外能正確傳到外層的 `catch`。

## YouTube 端點沒有官方文件，會不定期變動

`YTJsonParser` 呼叫的是 YouTube 網頁版背後的 InnerTube 內部 API（未公開、無版本保證），過去曾發生過破壞性改版讓 continuation 擷取邏輯完全失效。目前統一使用 `GET /live_chat?is_popout=1&v={videoID}`（直播、重播皆同一端點）取得 `contents.liveChatRenderer`，並以 `POST /youtubei/v1/live_chat/get_live_chat` 輪詢（`get_live_chat_replay` 端點目前回傳 400，勿使用）。

**如果聊天室擷取功能又不動了**：先用 `.claude/skills/yt-fetch-diagnose/SKILL.md` 描述的步驟，直接對 YouTube 發請求比對目前的 JSON 結構跟程式碼裡假設的路徑是否還吻合，而不是憑空猜測或改用其他假設。

## 參考第三方專案的原則（Clean Room）

理解 InnerTube 協議格式時，可以參考 chat-downloader、yt-dlp、其他語言的類似專案的公開文件／原始碼**去理解資料格式與端點行為**，但實作本身一律依據自己實際發送請求觀察到的 JSON 結果重新撰寫，不得複製其他專案的程式碼進來。既有程式碼中標註「參考：」某網址，代表該處的解析邏輯或欄位命名依循該資料格式撰寫，不代表程式碼是複製的。

## 已確認支援的 LiveChat 元素種類

一般留言、超級留言、超級貼圖、加入／升級／里程碑會員、贈送會員（`liveChatSponsorshipsGiftPurchaseAnnouncementRenderer`）、接收會員贈送、新版個別小禮物（`giftMessageViewModel`）、置頂／導向橫幅（`addBannerToLiveChatCommand` -> `bannerRenderer`）、跑馬燈（ticker，會取出內嵌的完整原始 Renderer）、捐款／購買／版主／自動版主訊息、創作者投票（`showLiveChatActionPanelAction` -> `pollRenderer`）與投票的即時得票率更新（見下方「即時更新機制」）、留言刪除（`removeChatItemAction`）、使用者被封鎖（`removeChatItemByAuthorAction`）、留言被取代／修改（`replaceChatItemAction`）、超級留言／貼圖的排行榜徽章（`leaderboardBadge`，掛在訊息上而非獨立 action，對應 `RendererData.LeaderboardRank`）、超級留言的回覆討論串人數更新（見下方）。

判斷「是否已結束」一律用 `IsVideoStreamingAsync` 或直接查 `liveBroadcastDetails.isLiveNow`，不要只憑「有 Top chat／Live chat 篩選選單」判斷——長時間常態直播與已結束重播都可能同時有這個選單。

`ParseNonMessageAction`（action 層級）與 `GetBackstageAttachment`（社群貼文附件層級）遇到不認識的類型時會記錄 Trace 診斷（`"... -> 尚未支援的 action/附件類型"`），純診斷用途、不影響既有解析行為。新增支援前，先靠這個記錄收集到真實樣本，不要憑空實作。

**刻意不實作／排除的項目**：

- `creatorHeartViewModel`（超級留言的「已被創作者比愛心」狀態）：走 `frameworkUpdates.entityBatchUpdate` 的 `engagementToolbarStateEntityPayload`，該酬載只有一個不透明 `key` 欄位，沒有任何布林值或狀態欄位，**確認無法解析**，不是尚未找到。
- `pointsButton` / `liveViewerLeaderboardChatEntryPointViewModel`（"Top fans" 排行榜進入點）：聊天室**標頭**的靜態導覽按鈕，不是逐則訊息的 action，跟 `leaderboardBadge`（掛在個別訊息上）是兩回事。
- `emojiFountainDataEntity`：直播間共用的「表情雨」環境特效資料，不屬於任何一則訊息。
- **暫時 timeout vs 永久封鎖的區分**：`removeChatItemByAuthorAction` 只有 `externalChannelId`。官方 `banType`（`PERMANENT`／`TEMPORARY`）只存在於需要 OAuth 的官方 Data API v3（`liveChatBans`），InnerTube 這層很可能本來就不帶這個資訊；`markChatItemsByAuthorAsDeletedAction`／`markChatItemAsDeletedAction` 這兩個 chat-downloader 有處理但本函式庫未實作的舊版 action，尚未觀察到真實樣本，若出現會被上面的診斷記錄捕捉。

## 即時更新機制（`frameworkUpdates.entityBatchUpdate` 與 `updateLiveChatPollAction`）

超級留言／貼圖的「回覆討論串」人數（`liveChatPaidMessageRenderer.replyButton.pdgReplyButtonViewModel`）不是內嵌在訊息 JSON 裡的靜態數字：訊息只帶一個不透明的 `replyCountEntityKey`（對應 `RendererData.ReplyCountEntityKey`），實際數字要等某一批 `get_live_chat` 回應**頂層**（`continuationContents` 同層級，不在 `actions` 陣列裡）的 `frameworkUpdates.entityBatchUpdate.mutations` 出現對應 `entityKey` 的 `payload.replyCountEntity.replyCountNumber`。`ParseFrameworkUpdates`（`YTJsonParser.ParseLiveChatJson.cs`）把每筆突變包成一筆獨立的 `RendererData`（`Type` 為「回覆數更新」、`ID` 借用來存放 `entityKey`、`ReplyCount` 存放新數字）附加進同一批輸出；呼叫端要自行記住每則付費訊息的 `ReplyCountEntityKey`，之後對照 `ID` 更新回原本那則訊息（跟 `removeChatItemAction`／`replaceChatItemAction`「借用 `ID` 做關聯」同一套模式）。

投票建立時（`ParsePollRenderer`）只有問題與選項文字，沒有票數；`updateLiveChatPollAction`（**不是**上面的 `frameworkUpdates` 機制，是獨立的 action）才即時推送 `voteRatio`／`votePercentage.simpleText`，`liveChatPollId` 與建立時相同可用來對照。

## 社群貼文

**請求網址一律用 `/posts`，不要用 `/community`**：`/community` 對部分頻道會回傳「沒有貼文」的空狀態（`contents.messageRenderer`），即使該頻道實際有大量貼文——這是一個靜默失敗，沒有任何錯誤訊息，容易誤判為「這個頻道沒有社群貼文」。`/posts` 是 YouTube 官方文件也已採用的新網址。

`GetCommunityTab` 優先找 `tabRenderer.selected == true` 的分頁（YouTube 現在只在 `tabs` 陣列回傳單一已選取分頁，不再塞進所有分頁，也不穩定提供可比對的 `webCommandMetadata.url`），找不到才退回舊版網址比對，最後才退回「若整份回應只有一個分頁就直接視為社群分頁」。

- **測驗貼文（`quizRenderer`）**：`backstageAttachment.quizRenderer` 底下 `choices[]` 每個選項多了 `isCorrect`（布林值），沒有 `numVotes`／`votePercentage`（作答前不透露票數分布）；問題文字沿用貼文既有的 `contentText`，不在 `quizRenderer` 內。解析結果沿用既有的 `PollData`／`ChoiceData` 模型（`ChoiceData.IsCorrect`、`AttachmentData.IsQuiz`）。
- **轉發貼文（`sharedPostRenderer`）**：YouTube 的「在 YouTube 上轉發」功能，**不是**附件類型，而是 `backstagePostThreadRenderer.post` 底下**取代** `backstagePostRenderer` 的另一種可能值（互斥，只會有一個）。`post.sharedPostRenderer` 帶有 `displayName`（轉發者）、`content`（轉發附加文字）、`publishedTimeText`，並巢狀包一份完整的 `originalPost.backstagePostRenderer`（被轉發的原始貼文，結構與一般貼文相同）。`GetBackstagePostRenderer` 在 `post.backstagePostRenderer` 不存在時退回 `post.sharedPostRenderer.originalPost.backstagePostRenderer`，讓 `PostData` 的 `AuthorText`／`ContentTexts`／`Attachments` 永遠代表「貼文本身的內容」；轉發中繼資料另外存在 `PostData.IsRepost`／`RepostedByAuthorText`／`RepostCaptionTexts`。

**`GetEarlierPostsAsync` 續傳失敗時務必清空 `ytConfigData.Continuation`，不能讓它停留在舊值**：這是實測抓到的真實卡死 bug——當續傳請求失敗（`GetJsonElementAsync` 重試耗盡後放棄）或回應結構不符預期（`onResponseReceivedEndpoints` 不存在）時，`GetEarlierPostsAsync` 原本直接 `return` 空清單，完全沒呼叫 `SetContinuation`，導致 `Continuation` 停在上一次的（已經沒用的）token 上。`StreamCommunityPostsAsync` 外層 while 迴圈只看 `Continuation` 是否還有值決定要不要繼續，於是變成每隔 `MinimumIntervalMs`（1 秒）就重送同一個 token，永遠不會結束、也不會拋出任何錯誤——WinForms 端的「匯出社群貼文」會卡在某個進度、進度條繼續轉但永遠不會完成，跟畫面凍結的表現一模一樣，但其實背景執行緒還在正常運作（只是在做無意義的重複請求）。修法是在 `!onResponseReceivedEndpointsArray.HasValue` 分支呼叫 `SetContinuation(arrayEnumerator: null, ytConfigData: ytConfigData)`，比對 `StreamLiveChatDataAsync` 對應位置本來就有的安全閥（`if (string.IsNullOrEmpty(jsonElement.ToString())) { break; }`）——社群貼文這條路徑當初漏做了同樣的事。`CommunityPostStreamingTests.cs` 有專門的迴歸測試（模擬續傳回應缺少 `onResponseReceivedEndpoints`，斷言只送出一次續傳請求就正常結束，並用逾時保護測試本身），新增／修改這一段續傳邏輯時務必保留這個測試。

## 直播／重播狀態判斷（`IsVideoStreamingAsync`）

抓 `/watch?v=` 頁面的 `ytInitialPlayerResponse`，以 `microformat.playerMicroformatRenderer.liveBroadcastDetails.isLiveNow` 判斷「是否目前正在直播」，`videoDetails.isLive` 作為備援。**注意**：`isLiveContent=true` 或聊天室是否有 "Top chat"/"Live chat" 篩選選單，都不能用來判斷「是否已結束」——長時間常態直播與已結束重播都可能同時成立，唯一可靠的欄位是 `liveBroadcastDetails.isLiveNow`／`videoDetails.isLive`。

`GetVideoTitleAsync` 讀取同一個 `ytInitialPlayerResponse.videoDetails.title`（JSON 字串值已解碼、無「 - YouTube」字尾問題），不要改回讀取 `<title>` 標籤的 `Element.InnerHtml`——`InnerHtml` 回傳重新序列化過的 HTML markup，`&`／`<`／`>` 等字元會維持 HTML 實體逸出，要用 `TextContent` 才是解碼後的文字。

**重播聊天室常態性關閉**：頻道可在「自訂管道」關閉「即時聊天室重播」，對直播影片做過剪輯（video editor 編輯過的影片一律沒有聊天室重播）的情況更是必然關閉，且不會通知觀眾（見 [YouTube 說明](https://support.google.com/youtube/answer/15268877)）。找重播測試樣本前，務必先用 curl 或瀏覽器直接檢查 `live_chat?is_popout=1` 回應內是否真的有 `liveChatRenderer`，不要假設熱門頻道會開放；重播視窗也可能在檢查完、還沒來得及測試前就被關閉。重播的輪詢遠快於即時直播的步調（回應間隔仍回報 1~10 秒，但每次回應內含的訊息量遠超過同樣間隔的即時直播），消費一整場重播不需要等待影片原始長度的時間。

## 輪詢頻率安全下限

`IntervalMs` 屬性（`YTJsonParser.PublicStatic.cs`）在未設定 `ForceIntervalMs` 時，取「YouTube 回應解析出的間隔值」與 `MinimumIntervalMs`（1000 毫秒）兩者中較大的值，避免因回應內容解析失敗等異常狀況導致間隔值意外停留在 0，對 YouTube 形成近乎無間隔的高頻輪詢。這個下限不套用在已明確設定的 `ForceIntervalMs` 上（視為刻意的選擇）。修改輪詢相關邏輯時，維持「正常情況下遵循 YouTube 建議間隔、異常情況下也不緊迫輪詢」這個原則。

## 輪詢的重試機制（`GetJsonElementAsync`，`YTJsonParser.Core.cs`）

HTTP 429（限速）與暫時性網路例外（`SendAsync` 拋出的非取消性質例外，例如 Wi-Fi 瞬斷、DNS 短暫失敗）共用同一組固定的嘗試次數預算（`maxAttempts`），在同一個 `for` 迴圈內處理，因為每次迭代本來就會重新建構一個新的 `HttpRequestMessage`（`HttpRequestMessage` 送出後不能重複使用，這是兩者能共用同一個迴圈的前提）。429 依伺服器回應的 `Retry-After` 等待，網路例外用遞增間隔（`5 * attempt` 秒，上限 20 秒）。重試次數用盡才放棄這次輪詢、記錄錯誤，讓串流照原本邏輯自然結束，刻意不做無限重試／指數退避，也不做跨輪詢週期的斷點續傳。測試見 `YTJsonParser.Tests/NetworkRetryTests.cs`（用 `FakeHttpMessageHandler.WhenSequence` 模擬「先失敗、重試後成功」）。

## 已知的行為變化

`YTJsonParser` 的內部記錄只會寫進標準 `ILogger`（實際落地在 NLog 的 `Logs/log.txt` 檔案與主控台輸出），**不會**自動出現在應用程式的記錄文字框裡，**唯一例外是** `LogMessages.UnsupportedContentEncountered`（EventId 15：解析時遇到尚未支援的內容/類型）——`YTLiveChatCatcher` 的 `InitLiveChatCather` 把這個特定事件透過 `DiagnosticForwardingLogger`（`Common/Utils/DiagnosticForwardingLogger.cs`，依 EventId 攔截，不是比對訊息字串）轉送到 `WriteLog`，讓正在盯著畫面的使用者能即時知道「這批資料可能沒有被完整解析」，而不是事後才想到要去查記錄檔。**這個事件刻意用 `LogLevel.Debug`，不是 `LogLevel.Trace`**——`YTLiveChatCatcher` 的 NLog 規則最低層級是 Debug，Trace 會被直接濾掉、連寫進 `Logs/log.txt` 都不會，這正是這次發現的問題：新增診斷用的 Trace 記錄，若消費端的最低記錄層級高於 Trace，等於完全沒有作用。新增其他值得結構化、需要被特別攔截的事件時，比照這個做法用專屬的 `[LoggerMessage]` 方法（固定 EventId），不要用訊息字串比對去猜測記錄內容。除此之外的其他內部記錄，維持只寫進記錄檔，不轉送到 UI（避免洗版）。

## WinForms 端消費 RendererData 的正確方式（`FMain.Methods.cs`）

`RendererData` 裡有幾種類型本質上是「以 `ID`（或其他欄位）關聯回既有訊息的更新／刪除事件」，不是獨立的新留言：`留言已被刪除`（`ID` = 目標訊息 ID）、`使用者已被封鎖`（`AuthorExternalChannelID` = 被封鎖使用者的頻道 ID）、`回覆數更新`（`ID` = `ReplyCountEntityKey`）、`投票結果更新`（`ID` = 建立投票時的 `liveChatPollId`），以及 `replaceChatItemAction` 產生的「同一個 `ID` 再次出現」情境。把這些當成全新留言加入 `ListView` 會在畫面上多出垃圾列、虛灌統計數字，Excel 匯出也會原封不動地把垃圾列匯出。

- `SharedItemsByMessageID`／`SharedItemsByReplyCountEntityKey`／`SharedItemsByAuthorChannelID`（`FMain.Variables.cs`）三個字典在建立新列時同步登記，讓上述事件能 O(1) 找到對應列（而不是線性掃描整個 `ListView`）。`BtnClear_Click` 清空聊天室時務必一併清空這三個字典（連同下方的累加式統計計數器），且要在呼叫 `UpdateSummaryInfo()` **之前**清空，順序寫反的話畫面會先短暫顯示清空前的舊數字。
- `留言已被刪除`／`使用者已被封鎖`：找到對應列後，在訊息內容前面加上文字標記（例如「〔已刪除〕」）並套用刪除線字型＋灰色，**保留原始列**（工具用途包含記錄／收益分析，刪除的留言本身也是有價值的資訊）。標記文字特意寫進訊息內容欄位本身而不是只靠字型樣式，因為 Excel 匯出不會轉存字型的刪除線樣式。
- `回覆數更新`／`投票結果更新`：找到對應列後就地更新欄位文字，不產生新列。
- 同一個 `ID` 再次出現（真重複資料，或 `replaceChatItemAction`）：就地更新既有列，不略過（避免 replace 的新內容被靜默丟棄）也不當成新列加入。
- `UpdateSummaryInfo` 用一組累加式計數器／集合（`FMain.Variables.cs`：`SharedChatCount`／.../`SharedIncomeByCurrency`／`SharedMemberInRoomAuthors`／`SharedDistinctAuthors`）取代每批次對整個 `ListView` 重新掃描，只在 `RegisterNewListViewItemStats`（真正新增一列時呼叫一次；就地更新既有列不會呼叫，避免重複計算）裡更新，`UpdateSummaryInfo` 本身只讀取這些欄位組字串。**若未來新增一種全新的計數器類型（例如「XX 事件人數」），記得同時更新 `RegisterNewListViewItemStats`**——但「這則訊息算不算聊天留言」這個分類本身已經抽到 `ChatStatsCalculator.Classify`，不受這條限制，見下一段。
- `FMain.EPPlusUtil.cs` 的 `LoadXLSX`（匯入）讀取舊版（沒有新欄位）匯出的 *.xlsx 檔案時會安全地讀到空字串，不會出錯。
- **`DoProcessMessages` 本身不可以再用 `Task.Run` 把實際插入 `ListView` 的動作丟到背景執行緒。** `DoProcessMessages` 是透過 `TBUserAgent.InvokeAsyncIfRequired(() => DoProcessMessages(batch), cancellationToken)` 呼叫的（`FMain.cs` 的擷取迴圈），呼叫當下就已經在 UI 執行緒上。曾經在方法內部又包一層 `Task.Run(async () => { await LVLiveChatList.InvokeAsyncIfRequired(...); ... })`，導致 `DoProcessMessages` 在那批資料真正插入畫面**之前**就先 return——外層迴圈以為這批已處理完成，繼續去抓下一批，但執行緒集區不保證先排的工作先跑，下一批的 `Task.Run` 可能比這一批先執行完，畫面上（以及匯出檔案裡）的訊息順序因此會跟實際收到的順序不一致。已改成直接同步呼叫（拿掉那層 `Task.Run`），讓外層的 `await` 真正等到這批資料完整插入畫面才算完成。用一支重現同樣結構、跑 200 批次的驗證程式證實過：修正前的寫法真的會亂序（例如第 4 批比第 0 批先出現），修正後不會。

## WinForms 端的純計算邏輯（`ChatStatsCalculator`）

`RegisterNewListViewItemStats` 曾經一次犯過兩個真實的統計正確性錯誤：金額加總只認裸 `"$"` 開頭（導致主要受眾使用的 `"NT$100"` 格式被完全忽略）、Excel 匯出的「留言數量」公式用列舉法列出三種類型（導致捐款／版主訊息／投票建立這類同樣算「留言」的類型被漏算，跟畫面即時數字對不起來）。兩次都不是被測試抓到的——因為判斷邏輯整包寫在跟 `Form`／`Control` 綁死的方法裡，沒有測試能碰。

修法：把不依賴 WinForms 的判斷邏輯抽到 `YTLiveChatCatcher/Common/Utils/ChatStatsCalculator.cs`（純 `static` 類別，只依賴 `YTJsonParser`／BCL，沒有任何 `System.Windows.Forms` 參照），由 `YTLiveChatCatcher.Tests` 覆蓋：

- `TryParsePurchaseAmount`：用 `[GeneratedRegex]` 正確拆解貨幣符號與金額（`"NT$100"` → `"NT$"`／`100`），不假設一律是新臺幣或美金；**裸 `"$"`（沒有任何字首）正規化成 `"NT$"`**——直接對同一筆真實新臺幣超級留言分別用 `hl=zh-TW` 與 `hl=en` 發請求驗證過，前者回傳裸 `"$15.00"`，後者回傳 `"NT$15.00"`：貨幣符號的字首格式取決於發送請求時的 `hl`／`gl`，不是只取決於實際交易貨幣。本應用程式固定用 `hl=zh-TW`（`DisplayLanguage.Chinese_Traditional`），裸 `"$"` 在這裡幾乎必然是新臺幣，不正規化的話同一種貨幣會被拆成兩個統計項目。若 `DisplayLanguage` 未來改成非正體中文，這個正規化規則需要一併重新檢視。
- `Classify`：依訊息類型／徽章判斷這則訊息該如何影響各項統計，回傳 `MessageStatsClassification`。`RegisterNewListViewItemStats` 只負責依這個分類結果做欄位遞增／集合新增，不再自己判斷。
- `ChatMessageExclusionKeys`：「留言數量」統計要排除的訊息類型清單，`ChatStatsCalculator.Classify` 的 `CountsAsChatMessage` 與 `DoExportTask` 組 Excel「留言數量」公式時**共用同一份**（後者用 `.Select(SharedYTJsonParser.GetLocalizeString)` 動態組出排除清單），不是兩處各自維護——這是消滅前述第二個 bug 的根本做法：兩處不可能再各自漂移，而不是靠人工記得同步。

新增會影響「這則訊息算不算留言」這個分類的邏輯時，改 `ChatStatsCalculator.Classify` 跟 `ChatMessageExclusionKeys`，不要繞回 `FMain.Methods.cs` 裡另外寫一份判斷。

## 社群貼文匯出（WinForms 端，`FMain.CommunityPostsExport.cs`）

一次性匯出功能：輸入頻道 ID、按下「匯出社群貼文」，直接呼叫 `SharedYTJsonParser.StreamCommunityPostsAsync`（`FetchWholeCommunityPosts = true`）把整個頻道的社群貼文全部拉完再存成 Excel，沒有 ListView 預覽、沒有即時更新（跟聊天室擷取是完全獨立的路徑，共用的只有 `RunLongTask`／`TerminateLongTask` 這組互斥控制與匯出用的具名樣式／`IMAGE()` 公式慣例）。

- `CommunityPostExportUtil`（`Common/Utils/CommunityPostExportUtil.cs`）：`FlattenRuns`（把 `List<RunsData>` 串成純文字，`ContentTexts`／`RepostCaptionTexts` 都用這個）與 `SummarizeAttachmentTypes`（組出「圖片 x2、影片、測驗」這種摘要文字）是抽出來的純邏輯，由 `YTLiveChatCatcher.Tests` 覆蓋。
- 產出 4 個分頁（`StringSet.SheetName7`～`SheetName10`）：「社群貼文」主分頁（每篇一列，含頭像 `IMAGE()`／內容／轉發資訊／附件摘要／超連結／隱藏的貼文 ID 技術欄位）、「貼文圖片」「貼文影片」「投票與測驗」則是攤平附件（`Attachments`）後依 `IsVideo`／`IsPoll` 分流，都用隱藏的「貼文 ID」欄位對照回主分頁；沒有對應附件類型的貼文時，該分頁直接不建立（不是建一個空分頁）。測驗貼文（`IsQuiz = true`）跟一般投票共用同一個分頁，靠「是否為測驗」欄位區分。
- 跟 `DoExportTask`（聊天記錄匯出）一樣，用 `try { await ...; } finally { TerminateLongTask(...); }` 收尾，**不要**改回 `.ContinueWith(...)`——這個對話早先修過的 bug 模式（見上方「程式風格」段落），預設 `ContinueWith` 會吞掉例外，讓使用者看不到真正的失敗原因。
- 目前沒有取消 UI（`BtnExportCommunityPosts_Click` 傳 `CancellationToken.None`）——這個操作沒有像聊天室擷取一樣的「停止」按鈕，超出這次的範圍；未來若要加，不要直接借用 `SharedFetchCancellationTokenSource`（那個欄位語意是「取消即時聊天擷取」，混用會讓兩個完全獨立的操作互相影響取消時機）。

## 當機復原（`CaptureRecoveryStore`）

擷取聊天室是一個可能持續數小時的過程，畫面上的資料只存在記憶體裡，完全依賴使用者自己記得手動匯出。`CaptureRecoveryStore`（`Common/Utils/CaptureRecoveryStore.cs`）在每次 `StartFetchLiveChatData` 收到新批次時，就先把原始資料附加寫進 `%LocalAppData%\YTLiveChatCatcher\recovery.jsonl`（JSON Lines，一行一批次，附加寫入成本不隨累積資料量變貴），再交給 `DoProcessMessages` 處理成 `ListView` 項目——即使處理過程本身出問題，這批已收到的原始資料也已經安全落地。

- `FMain_Load` 呼叫 `CheckCaptureRecovery`：偵測到非空的復原記錄時詢問使用者是否載入；選是就把每個批次重新餵給 `DoProcessMessages`（跟即時擷取走同一條處理路徑，包含 ID 關聯／去重／統計邏輯），選否則清除記錄檔。
- 記錄檔**只在**成功完整匯出 `LVLiveChatList`（不是搜尋結果的篩選子集）或使用者主動按「清空聊天室」時才清除——單純按「停止擷取」不會清除，因為停止不代表使用者已經拿到資料的安全備份，「忘記匯出就關掉程式」正是這個機制要保護的情境之一。
- `SerializeBatchLine`／`ParseBatchLines` 是抽出來的純邏輯（不碰真實檔案系統），供 `YTLiveChatCatcher.Tests` 覆蓋；`AppendBatch`／`LoadBatches`／`Clear`／`Exists` 這幾個會動到真實檔案的方法刻意沒有測試覆蓋，因為它們固定寫死存取使用者實際執行這個應用程式時真正會用到的同一個檔案路徑，測試直接操作有覆寫掉使用者真實復原記錄的風險。

## 已知技術債

`EPPlus` 使用 Polyform Noncommercial 授權（`ExcelPackage.License.SetNonCommercialOrganization(...)`，`FMain.EPPlusUtil.cs`／`FMain.Methods.cs` 各呼叫一次，分別對應匯入／匯出兩個獨立進入點，非重複程式碼）。本專案為免費、非商業性質，符合此授權條款；商業用途需另外購買授權，更新版本前留意授權條款是否變動。

`Application.SetColorMode(SystemColorMode.System)`（`Program.cs`）讓應用程式跟隨 Windows 設定自動切換深／淺色模式，僅 Windows 11 以上有效（Windows 10 自動退回淺色），且不是所有控制項都會跟著變（`MessageBox` 固定淺色，是 WinForms 目前的已知限制，不是這個專案沒做完整）。**`ListView` 無法內嵌顯示自訂表情符號／超級貼圖的圖片**——`YTJsonParser` 把這類內容轉成文字佔位符（例如 `:emoji_label:`）存進訊息內容，圖片只會出現在 Excel 匯出的「自定義表情符號」「超級貼圖」分頁，因為標準 `ListView`（沒有用 OwnerDraw 自繪）一列只能掛一張圖示（這裡拿去顯示作者頭像了），沒辦法在文字欄位裡文字＋圖片混排；要做到需要自繪 ListView、換控制項，或加一個獨立的縮圖預覽面板，目前刻意沒有做。
