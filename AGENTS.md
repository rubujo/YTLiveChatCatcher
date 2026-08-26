# AGENTS.md

給所有 AI 編碼代理（Claude Code、Codex、Cursor、Copilot、Antigravity CLI 等）使用的專案規範。這份檔案是跨工具的共用事實來源；`CLAUDE.md` 僅是指向這裡的精簡入口。

這份文件只記錄「程式碼／git 歷史看不出來」的決策、限制與陷阱，不記錄變更日誌——已完成的修正、實測過程、調查細節請查 `git log`，不要疊加在這裡。新增內容前，先問自己：這是未來的代理需要知道才能做對決策的事，還是單純在記錄「我做了什麼」？只有前者才寫進來。

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

新增測試時：`YTJsonParser` 的解析方法幾乎都是 `private`，測試一律透過**公開 API**（`StreamLiveChatDataAsync`／`StreamCommunityPostsAsync`／`IsVideoStreamingAsync`）搭配 `FakeHttpMessageHandler` 進行，不要為了測試把方法改成 `internal` + `InternalsVisibleTo`——這樣測試才能在內部實作重構後仍然有效，同時也順便驗證了 URL 組成與 continuation 交換邏輯（這是最容易被 YouTube 改版打壞的部分）。新增 fixture 時，**用程式（例如 Python 的 `json.dumps`）產生保證合法的 JSON 字串再嵌入 HTML**，不要手動在一行內編織巢狀大括號／中括號，容易漏算括號讓 fixture 壞掉。

`YTLiveChatCatcher` 沒有自動化測試（`ListView`／`FMain` 高度耦合、需要 STA 訊息迴圈）。改動它之後至少要建置＋啟動應用程式確認無啟動期例外；牽涉即時串流／匯入匯出的改動，理想上要接真實直播手動操作驗證，做不到時要在回報裡明講「沒做到這一步」，不要含糊帶過。

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

## 直播／重播狀態判斷（`IsVideoStreamingAsync`）

抓 `/watch?v=` 頁面的 `ytInitialPlayerResponse`，以 `microformat.playerMicroformatRenderer.liveBroadcastDetails.isLiveNow` 判斷「是否目前正在直播」，`videoDetails.isLive` 作為備援。**注意**：`isLiveContent=true` 或聊天室是否有 "Top chat"/"Live chat" 篩選選單，都不能用來判斷「是否已結束」——長時間常態直播與已結束重播都可能同時成立，唯一可靠的欄位是 `liveBroadcastDetails.isLiveNow`／`videoDetails.isLive`。

`GetVideoTitleAsync` 讀取同一個 `ytInitialPlayerResponse.videoDetails.title`（JSON 字串值已解碼、無「 - YouTube」字尾問題），不要改回讀取 `<title>` 標籤的 `Element.InnerHtml`——`InnerHtml` 回傳重新序列化過的 HTML markup，`&`／`<`／`>` 等字元會維持 HTML 實體逸出，要用 `TextContent` 才是解碼後的文字。

**重播聊天室常態性關閉**：頻道可在「自訂管道」關閉「即時聊天室重播」，對直播影片做過剪輯（video editor 編輯過的影片一律沒有聊天室重播）的情況更是必然關閉，且不會通知觀眾（見 [YouTube 說明](https://support.google.com/youtube/answer/15268877)）。找重播測試樣本前，務必先用 curl 或瀏覽器直接檢查 `live_chat?is_popout=1` 回應內是否真的有 `liveChatRenderer`，不要假設熱門頻道會開放；重播視窗也可能在檢查完、還沒來得及測試前就被關閉。重播的輪詢遠快於即時直播的步調（回應間隔仍回報 1~10 秒，但每次回應內含的訊息量遠超過同樣間隔的即時直播），消費一整場重播不需要等待影片原始長度的時間。

## 輪詢頻率安全下限

`IntervalMs` 屬性（`YTJsonParser.PublicStatic.cs`）在未設定 `ForceIntervalMs` 時，取「YouTube 回應解析出的間隔值」與 `MinimumIntervalMs`（1000 毫秒）兩者中較大的值，避免因回應內容解析失敗等異常狀況導致間隔值意外停留在 0，對 YouTube 形成近乎無間隔的高頻輪詢。這個下限不套用在已明確設定的 `ForceIntervalMs` 上（視為刻意的選擇）。修改輪詢相關邏輯時，維持「正常情況下遵循 YouTube 建議間隔、異常情況下也不緊迫輪詢」這個原則。

## 已知的行為變化

`YTJsonParser` 的內部記錄只會寫進標準 `ILogger`（實際落地在 NLog 的 `Logs/log.txt` 檔案與主控台輸出），**不會**自動出現在應用程式的記錄文字框裡。如果要在 UI 文字框也看到，需要另外接一個自訂 NLog target 把 `YTJsonParser` 這個 logger 分類的訊息轉發過去，目前刻意沒有做這件事（「查看記錄檔」是預期的除錯方式）。

## WinForms 端消費 RendererData 的正確方式（`FMain.Methods.cs`）

`RendererData` 裡有幾種類型本質上是「以 `ID`（或其他欄位）關聯回既有訊息的更新／刪除事件」，不是獨立的新留言：`留言已被刪除`（`ID` = 目標訊息 ID）、`使用者已被封鎖`（`AuthorExternalChannelID` = 被封鎖使用者的頻道 ID）、`回覆數更新`（`ID` = `ReplyCountEntityKey`）、`投票結果更新`（`ID` = 建立投票時的 `liveChatPollId`），以及 `replaceChatItemAction` 產生的「同一個 `ID` 再次出現」情境。把這些當成全新留言加入 `ListView` 會在畫面上多出垃圾列、虛灌統計數字，Excel 匯出也會原封不動地把垃圾列匯出。

- `SharedItemsByMessageID`／`SharedItemsByReplyCountEntityKey`／`SharedItemsByAuthorChannelID`（`FMain.Variables.cs`）三個字典在建立新列時同步登記，讓上述事件能 O(1) 找到對應列（而不是線性掃描整個 `ListView`）。`BtnClear_Click` 清空聊天室時務必一併清空這三個字典（連同下方的累加式統計計數器），且要在呼叫 `UpdateSummaryInfo()` **之前**清空，順序寫反的話畫面會先短暫顯示清空前的舊數字。
- `留言已被刪除`／`使用者已被封鎖`：找到對應列後，在訊息內容前面加上文字標記（例如「〔已刪除〕」）並套用刪除線字型＋灰色，**保留原始列**（工具用途包含記錄／收益分析，刪除的留言本身也是有價值的資訊）。標記文字特意寫進訊息內容欄位本身而不是只靠字型樣式，因為 Excel 匯出不會轉存字型的刪除線樣式。
- `回覆數更新`／`投票結果更新`：找到對應列後就地更新欄位文字，不產生新列。
- 同一個 `ID` 再次出現（真重複資料，或 `replaceChatItemAction`）：就地更新既有列，不略過（避免 replace 的新內容被靜默丟棄）也不當成新列加入。
- `UpdateSummaryInfo` 用一組累加式計數器／集合（`FMain.Variables.cs`：`SharedChatCount`／.../`SharedTotalIncome`／`SharedMemberInRoomAuthors`／`SharedDistinctAuthors`）取代每批次對整個 `ListView` 重新掃描，只在 `RegisterNewListViewItemStats`（真正新增一列時呼叫一次；就地更新既有列不會呼叫，避免重複計算）裡更新，`UpdateSummaryInfo` 本身只讀取這些欄位組字串。**若未來新增一種會影響統計的訊息類型，記得同時更新 `RegisterNewListViewItemStats`**，否則新類型不會反映在統計數字裡——這是這個設計相對於「每次重新掃描」的取捨，正確性依賴人工同步維護兩處邏輯。
- `FMain.EPPlusUtil.cs` 的 `LoadXLSX`（匯入）讀取舊版（沒有新欄位）匯出的 *.xlsx 檔案時會安全地讀到空字串，不會出錯。

## 已知技術債

`EPPlus` 使用 Polyform Noncommercial 授權（`ExcelPackage.License.SetNonCommercialOrganization(...)`，`FMain.EPPlusUtil.cs`／`FMain.Methods.cs` 各呼叫一次，分別對應匯入／匯出兩個獨立進入點，非重複程式碼）。本專案為免費、非商業性質，符合此授權條款；商業用途需另外購買授權，更新版本前留意授權條款是否變動。
