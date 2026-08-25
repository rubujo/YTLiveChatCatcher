# CLAUDE.md

專案規範請見 [AGENTS.md](AGENTS.md)（跨 Agent 通用，Claude Code 也直接遵循該檔案內容）。

Claude Code 專屬事項：

- 本專案有一個 Skill：`yt-fetch-diagnose`（`.claude/skills/yt-fetch-diagnose/SKILL.md`），用於診斷 YouTube 聊天室 JSON 擷取機制是否仍可用。當使用者反應「抓不到聊天室內容」或要求「確認擷取機制是否仍正常」時，請呼叫此 Skill。
