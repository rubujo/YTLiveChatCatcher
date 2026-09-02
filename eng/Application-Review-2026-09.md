# YTLiveChatCatcher 應用程式審查與競品比較（2026-09）

## 結論摘要

YTLiveChatCatcher 的差異化不是「單純下載聊天室」，而是 Windows 圖形介面、即時收益統計、Excel 報表、社群貼文匯出，以及不讀取日常瀏覽器資料庫的會員內容登入流程。核心解析器對 YouTube 結構漂移已有防禦式取值、有限重試、直播／重播雙路徑、未知 action 診斷與 fixture 測試，基礎比一般一次性下載腳本完整。

最大殘餘風險仍是 InnerTube 未公開且隨時可能改版；可攜格式、擷取斷點續傳、診斷封裝、分析工具與 CI 已完成。真正可點擊所有 WinForms 流程的端到端測試仍受 GitHub hosted runner 沒有互動式桌面的限制，目前採「純邏輯測試＋程序啟動 smoke test＋必要時實機驗證」三層策略。

## 本輪已實作

| 項目 | 原始風險 | 實作 |
|---|---|---|
| 防止多開 | 多個程序可能同時寫入 `recovery.jsonl`、設定、Cookie 與頭像快取 | 使用命名 Mutex；第二個實例顯示訊息後退出 |
| 復原資料隱私 | 會員限定聊天室也會以純文字 JSONL 自動落地 | 新資料逐批使用 DPAPI CurrentUser 加密；仍可讀取舊版純文字記錄 |
| 復原寫入可用性 | 磁碟滿、檔案鎖定或 DPAPI 失敗會中止整場擷取 | 寫入失敗只警告一次，擷取繼續，並要求使用者儘快手動匯出 |
| 大型復原檔載入 | `ReadAllLines` 先建立完整字串陣列，再建立完整物件圖 | 改為 `ReadLines` 逐行解析，降低尖峰記憶體 |
| 敏感標頭記錄 | 通用 `HttpClient` 標頭記錄沒有統一遮蔽身分憑證 | 遮蔽 Cookie、Authorization、X-Youtube-Identity-Token |
| Cookie 明文緩衝區 | DPAPI 加密後，額外建立的 UTF-8 明文陣列等待 GC | `finally` 立即清零可變明文緩衝區 |
| 金額精度 | 使用 `double` 解析與累加貨幣，可能出現二進位浮點誤差 | 全鏈路改用 `decimal`，含 70% 試算與單元測試資料 |
| DI 資源生命週期 | 根 `ServiceProvider` 在程序結束時沒有釋放 | `using` 管理根容器與其中服務 |
| 持續整合 | 沒有 CI，還原、Release 建置與測試完全依賴本機 | 新增 Windows/.NET 10 GitHub Actions：還原、建置、兩組測試、相依漏洞稽核 |
| 擷取中斷 | 只能載入已保存資料，不能接續網路擷取 | session manifest、continuation checkpoint、結束原因、完整性標示與事件去重 |
| 資料可攜性 | 只有 XLSX | 無損 JSONL、通用 CSV、組合篩選與分析工具 |
| 問題回報 | 使用者需自行挑選 log，可能外洩憑證 | 一鍵產生自動遮蔽的 ZIP 診斷包與結構 fixture |
| 收益語意 | 固定 70%，容易被誤解為實際結算 | 比例可設定，UI 明確標示為粗略估算 |

## 風險盤點

### 高風險

1. **InnerTube 結構漂移**：直播與重播都依賴未公開端點。現有 fixture、未知 action 診斷與 `/watch` replay fallback 能縮短修復時間，但不能消除改版風險。
2. **缺乏真正的 UI 自動化測試**：純邏輯已有 xUnit 覆蓋，但開始／停止、匯入／匯出、搜尋視窗、Cookie 對話框與大型 VirtualMode ListView 仍仰賴實機。
3. **續傳權杖仍可能過期**：已具備 checkpoint 與去重，但 InnerTube continuation 沒有有效期保證，因此 manifest 會保守標示完整性。

### 中風險

1. **資料可攜性**：已補 JSONL／CSV；XLSX 繼續作為人類報表格式。
2. **診斷封裝**：已提供自動遮蔽診斷包，但傳送前仍需人工複核。
3. **官方 API 與 InnerTube 沒有雙引擎策略**：官方 `liveChatMessages.streamList` 僅適合直播且需要 API/OAuth，但可作為直播的低延遲、強型別備援；重播仍需 InnerTube。
4. **發佈流程不完整**：本輪補了 CI，但尚未有可重現的簽章、打包、雜湊、SBOM 與 release artifact 工作流程。
5. **單語系產品 UI**：解析層支援多種區域設定，但 WinForms UI 與統計假設固定為繁體中文／臺灣情境。

### 低風險或已接受限制

1. EPPlus 採 Polyform Noncommercial；README 已明確揭露，若要商業發佈必須換套件或購買授權。
2. DPAPI 只能保護靜態檔案，無法防止同一 Windows 使用者權限下已入侵的程序讀取執行中記憶體。
3. 70% 收益僅是粗略試算，不包含稅務、地區差異、退款與實際 YouTube 結算規則；不應標示為會計數字。

## 競品比較

| 能力 | YTLiveChatCatcher | chat-downloader 系列 | yt-dlp | HyperChat / LiveTL | YouTube Data API |
|---|---|---|---|---|---|
| 主要定位 | Windows 擷取、分析、Excel 報表 | 跨平台 CLI／Python 擷取 | 媒體下載器附帶聊天室原始資料 | 瀏覽器內即時聊天體驗／翻譯 | 官方直播訊息 API |
| 直播 | 是 | 是 | 是 | 是 | 是 |
| 重播聊天室 | 是，含 `/watch` fallback | 是 | 是，視為 `live_chat` 字幕 | 以觀看體驗為主 | 否，活動結束後不可取 |
| 會員內容 | 專屬 WebView2 登入＋手動 Cookie | Cookie／請求 profile | Cookie | 瀏覽器既有登入 | OAuth／權限模型 |
| 輸出 | XLSX | JSONL／文字，可限制時間與訊息類型 | 原始 live-chat JSON／字幕工作流 | 非主要功能 | JSON API |
| 收益分析 | 有，逐幣別 | 無內建 GUI 分析 | 無 | 無 | 提供結構化金額欄位，分析需自行做 |
| 社群貼文 | 有 | 非核心 | 可抓部分網站中繼資料，但不是同型報表 | 無 | 另需其他 API，能力不同 |
| 多平台 | 僅 YouTube | YouTube／Twitch／Kick（依分支） | 多網站 | YouTube／瀏覽器 | YouTube |
| 低延遲推送 | 輪詢 | 輪詢／平台機制 | 下載流程 | 貼近瀏覽器即時體驗 | `streamList` 伺服器串流 |
| 使用門檻 | 低，GUI | 中高，CLI | 中高，CLI | 低，瀏覽器擴充 | 高，Cloud 專案、配額與 OAuth |

參考來源：

- chat-downloader：<https://github.com/xenova/chat-downloader>；活躍分支文件：<https://github.com/75ohmantenna/chat-downloader-fork/blob/master/docs/cli-usage.md>
- yt-dlp：<https://github.com/yt-dlp/yt-dlp>
- HyperChat / LiveTL：<https://github.com/LiveTL/LiveTL>
- YouTube `liveChatMessages`：<https://developers.google.com/youtube/v3/live/docs/liveChatMessages>
- YouTube `liveChatMessages.list`／`streamList` 建議：<https://developers.google.com/youtube/v3/live/docs/liveChatMessages/list>

## 功能缺口與建議順序

### 已完成的 P0：可靠交付與資料不遺失

1. **CI 實際跑綠並設為分支保護必要檢查**（工作流程已加入，尚待 GitHub 首次執行）。
2. **結構漂移 fixture 收集工具**：一鍵輸出已遮蔽的 ytcfg、初始頁與單批 InnerTube 回應，讓使用者可安全附在 issue。
3. **擷取 session manifest**：保存影片 ID、標題、開始／停止時間、程式版本、語系、最後 continuation 與訊息數；明確區分「完整結束」「使用者停止」「網路失敗」「結構不支援」。
4. **斷點續傳與去重**：以 session manifest＋訊息 ID 繼續擷取；無法保證無缺口時必須在報表標示資料不完整。
5. **WinForms smoke test harness**：至少覆蓋啟動、開始／停止、XLSX 往返、搜尋、大量資料與 Cookie 輸入邊界。

### 已完成的 P1：資料可攜與分析能力

1. **JSONL 與 CSV 匯出**：JSONL 作為無損原始格式，CSV 作為一般分析交換格式；XLSX 保留為人類報表。
2. **時間範圍、訊息類型、作者與金額篩選**：下載前／匯出時都可套用，對齊 chat-downloader 的 CLI 能力。
3. **分析儀表板**：每分鐘訊息密度、付費事件時間軸、新增會員／贈送會員、活躍作者、幣別分布。
4. **一鍵診斷包**：包含版本、OS、錯誤摘要、已遮蔽 log、未知 renderer/action 名稱，不包含 Cookie 或 token。
5. **明確的收益語意**：把「70%」標示為估算，可設定比例；報表保留原始顯示金額與幣別，不進行隱含換匯。

### P2：產品擴張

1. **同步重播檢視器**：依影片時間軸重播聊天、跳轉熱點，補足 YT-Live-Chat-Replayer 類產品能力。
2. **官方 API 直播引擎（選配）**：支援 `streamList`，用於需要官方強型別事件與低延遲的直播；InnerTube 保留給重播與免 Cloud 設定情境。
3. **CLI／無頭模式**：排程、伺服器長時間執行與批次處理，並讓核心函式庫更容易被其他程式整合。
4. **多場直播／頻道監看與排程**：自動偵測開台、開始擷取、結束後匯出。
5. **多平台連接器**：若產品方向要成為通用直播分析工具，再考慮 Twitch／Kick；否則維持 YouTube 深度會更有差異化。
6. **翻譯、overlay、Webhook／OBS 整合**：屬於觀看與營運體驗，不應優先於資料完整性。

## 驗證限制

GitHub Windows runner 已完成 Release 還原、建置、兩個測試專案、WinForms 程序啟動 smoke test 與相依漏洞稽核，NuGet Cache 也以相同提交重跑確認 `exact hit: true`。Hosted runner 沒有可靠的互動式桌面，因此「實際點擊開始／停止、檔案對話框、Cookie 登入視窗」仍不能冒充已自動實測；這些流程需在發佈前以實機 smoke checklist 補驗。
