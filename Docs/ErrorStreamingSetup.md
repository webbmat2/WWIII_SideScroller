# Error Streaming Setup (Unity 6000.2 + Cursor)

This integrates Unity Console errors into Cursor’s Problems panel and provides on‑device/runtime diagnostics.

## 1) Workspace
- Install recommended extensions (see `.vscode/extensions.json`).
- In Unity: Preferences → External Tools → External Script Editor = Open by file extension. Regenerate csprojs.
- In Cursor: Run Task → `Watch Unity Editor.log` (background). Keep it running while editing.

## 2) Cross‑Platform Logs
- macOS Editor.log: `~/Library/Logs/Unity/Editor/Editor.log`
- Windows Editor.log: `%LOCALAPPDATA%\Unity\Editor\Editor.log`
- Linux Editor.log: `~/.config/unity3d/Editor.log`
- Player.log paths are template placeholders in tasks.json; adjust CompanyName/ProductName if you need them.

## 3) Editor Log Viewer (inside Unity)
- Unity menu: WWIII → Diagnostics → Editor Log Viewer
- Updates ~1Hz; shows last ~10k characters of the log.

## 4) JSON Error Events (Editor)
- Written to `Logs/unity_error_events.jsonl` at project root.
- Each line is a JSON object `{ ts, type, msg, stack }` captured via `Application.logMessageReceivedThreaded`.
- Open file via WWIII → Diagnostics → Open Error Events (JSONL).

## 5) Tips
- Add `"problems.autoReveal": true` to `.vscode/settings.json` if you want Problems to pop on new errors.
- Use `WWIII/Addressables/Analyze` and `Build Player Content` for streaming build checks.
- For CI, add `UNITY_LICENSE` to repo secrets to run headless tests/builds.

## Troubleshooting
- No errors in Problems panel: ensure the Editor.log watcher task is running, and that a compile occurred.
- Windows: confirm PowerShell policy allows `Get-Content -Wait`; adjust shell accordingly if restricted.
- Too many messages: mute verbose logs or switch terminal panel to “shared” and “never reveal”.
