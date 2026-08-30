# NOTICE

本專案原始碼原則上以本專案根目錄的 `LICENSE` 為準。

以下記錄一段授權沿革：本專案曾經包含改編自下列第三方來源的程式碼片段，並在對應的原始檔以「來源／原作者／原授權」的文件註解標示。這些來源分別授權於 CC BY-SA 3.0、CC BY-SA 4.0、CC BY-NC-SA 3.0 TW，其中 CC BY-NC-SA 3.0 TW 額外含有非商業（NC）限制。由於 ShareAlike 條款不允許把衍生作品降級成限制更少的授權，這些片段已於 2026/8 依各自的行為規格獨立重新實作（不參考原始程式碼的內部結構），公開簽章維持不變，改寫後的版本由對應的單元測試覆蓋。原始來源與作者列於下方，僅供歷史查證，不代表本專案目前的程式碼仍受這些授權拘束。

| 檔案 | 原始來源 | 原作者 | 原授權 |
|---|---|---|---|
| `YTJsonParser/Utils/BetterCacheManager.cs` | https://blog.darkthread.net/blog/cachable-data-object | 黑暗執行緒 | CC BY-NC-SA 3.0 TW |
| `YTJsonParser/Extensions/JsonElementExtension.cs` | https://stackoverflow.com/a/61561343 | dbc | CC BY-SA 4.0 |
| `YTJsonParser/Utils/YouTubeUrlUtil.cs`（`GetYouTubeVideoID`） | https://stackoverflow.com/a/15219045 | rvalvik | CC BY-SA 3.0 |
| `YTJsonParser/YTJsonParser.YouTubeAuth.cs`（`SetHttpRequestMessageHeader`） | https://stackoverflow.com/a/13287224 | Greg Beech | CC BY-SA 3.0 |
| `YTLiveChatCatcher/Common/CustomFunction.cs`（`RemoveInvalidFilePathCharacters`） | https://stackoverflow.com/a/8626562 | Gary Kindel | CC BY-SA 3.0 |
| `YTLiveChatCatcher/Common/DesignerBlocker.cs` | https://stackoverflow.com/a/68585095 | Regular Jo | CC BY-SA 4.0 |
| `YTLiveChatCatcher/Extensions/ImageExtension.cs` | https://stackoverflow.com/a/1668493 | JaredPar（編輯者：Kristian Frost） | CC BY-SA 3.0 |
| `YTLiveChatCatcher/Extensions/ListViewExtension.cs`（`GetSelectedListViewItems`／`GetListViewItems`） | https://stackoverflow.com/a/40205173 | Joe Savage | CC BY-SA 3.0 |
| `YTLiveChatCatcher/Program.cs`（`UpdateConfig`） | https://stackoverflow.com/a/23924277 | Grant | CC BY-SA 3.0 |
| `YTJsonParser/Extensions/StreamExtension.cs`（`GetImageFormat`） | https://gist.github.com/markcastle/3cc99c8e5756c7e27532900a5f8a2a93 | markcastle | 僅標示 Copyright 2017 Captive Reality Ltd，未標明可重新散布的授權條款 |
| `YTLiveChatCatcher/Common/CustomFunction.cs`（`OpenBrowser`） | https://github.com/dotnet/runtime/issues/17938（issuecomment-235502080／249383422） | mellinoe／brockallen | 未標明授權（GitHub Issue 留言，非該 repo 本身宣告授權的原始碼檔案） |
| `YTLiveChatCatcher/Extensions/ControlExtension.cs`（`InvokeIfRequired`） | https://dotblogs.com.tw/shinli/2015/04/16/151076 | Shin.Li | 未標明授權 |

以上兩項原本就沒有標註明確授權條款，風險評估上屬於極短、幾乎只有一種合理寫法（WinForms `Control.InvokeRequired` 判斷式、跨平台 `Process.Start` 開啟瀏覽器，兩者都是官方文件／社群公認的標準寫法），依 merger doctrine 本來風險就低，這裡一併重新表達純粹是求整個專案的一致性，不代表原本有實質侵權疑慮。

## 重寫方法的誠實揭露

這次重寫是由 AI 助理（Claude）依上述函式既有的公開簽章、doc comment、既有呼叫端用法寫出行為規格，再依規格與標準 .NET 慣用寫法獨立重新實作，並非嚴格意義下的正式淨室（clean room）流程——正式淨室要求「寫規格的人」與「依規格實作的人」之間完全沒有溝通管道，而這次是同一個 AI 助理先讀過原始碼、再依自訂規格重寫，這一點在業界（尤其是 AI 輔助開發的情境）目前仍沒有定論的判例可循。函式越短、越接近「只有一種合理寫法」（表達與概念合併原則，merger doctrine），重寫後被認定侵權的風險就越低；上表大多數項目屬於這一類。若要以本專案的 CC0 宣告為前提進行商業佈署或再散布，建議另外諮詢熟悉智慧財產權的專業人士做最終確認。
