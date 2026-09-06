---
name: yt-fetch-diagnose
description: 診斷 YTLiveChatCatcher 的 YouTube 直播／重播擷取為空、停滯或結構漂移，依當次回應核對端點、continuation 與解析結果。
---

# YouTube 擷取診斷

依 [專案規範](../../../AGENTS.md) 的資料保留與協定限制找出原因。使用者只要求診斷時交付證據；要求修復時繼續實作與驗證。例行選擇自行處理，不因這份 Skill 另加核准流程。

## 診斷依據

- 先看指定影片、log 與 fixture，決定離線重現或即時請求。網路故障不靠猜測改解析器。
- 即時驗證先確認影片目前狀態；舊 ID 不保證有效。沒有 continuation 可能是登入、同意頁或請求失敗，不直接宣稱聊天室關閉。
- 直播檢查 popout 的 contents.liveChatRenderer；重播檢查 /watch 的 contents.twoColumnWatchNextResults.conversationBar.liveChatRenderer。內嵌 JSON 用括號配對解析。
- 依 continuation 來源選 get_live_chat 或 get_live_chat_replay，不混用 token。重播檢查 replayChatItemAction.actions 與 liveChatReplayContinuationData。
- 核對 ytcfg client context、語系、HTTP 狀態及 action 類型。選單文字不能證明直播狀態；未知結構先保存遮蔽樣本再決定支援。
- 命令符合當下 shell；Windows 使用 curl.exe 避免別名差異，搜尋優先 rg。請求有逾時及取消，不無限抓取。
- 保留原有 Cookie／復原記錄，沒有必要不使用登入狀態。只保存已遮蔽 fixture，不輸出 Cookie、授權標頭或 continuation。

## 修復與驗證

透過公開 StreamLiveChatDataAsync + FakeHttpMessageHandler 重現缺陷，fixture 用 JSON serializer 產生。修改對應 partial 並驗證直播／重播分流，不複製第三方程式碼。

必要時以隔離暫時主控台或既有測試觀察批次、停止與取消，不接觸使用者真實復原檔。GUI 以畫面、程序／視窗識別和實際操作交叉驗證；UIA 失敗不代表功能壞掉。

回報原因、命令、結果及真實直播／重播是否實測。無可靠即時樣本時交付離線結果與精確缺口。只更新會影響未來決策的規範，修正日期故事留在 git。

設計依據：OpenAI [Astra 指引](https://developers.openai.com/api/docs/guides/latest-model#prompting-best-practices) 與 [Skill 文件](https://learn.chatgpt.com/docs/build-skills)，核對日期 2026-09-06。
