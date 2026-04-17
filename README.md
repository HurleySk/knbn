# knbn

A Visual Studio Enterprise extension that provides a kanban board for tracking work across multiple Claude Code CLI instances.

## Setup

### 1. Install the Extension

Build and install the VSIX:

```bash
cd src/Knbn.Extension
dotnet build -c Release
```

Double-click the generated `.vsix` file in `bin/Release` to install.

### 2. Configure Claude Code Hook

Add a `SessionEnd` hook to your Claude Code user settings (`~/.claude/settings.json`):

```json
{
  "hooks": {
    "SessionEnd": [
      {
        "type": "command",
        "command": "curl -s -X POST http://localhost:9090/events -H \"Content-Type: application/json\" -d \"{\\\"event\\\":\\\"SessionEnd\\\",\\\"session_id\\\":\\\"$CLAUDE_SESSION_ID\\\"}\""
      }
    ]
  }
}
```

### 3. Install Claude Code Skills

Claude Code skills for knbn are maintained in [claude-knbn-skill](https://github.com/HurleySk/claude-knbn-skill) and distributed via the HurleySk marketplace. Install from the marketplace or add the plugin directly.

### 4. Open the Board

In Visual Studio: **View → knbn Board**

## Usage

In any Claude Code CLI session:

- `/knbn "Auth System"` — Register this session as working on "Auth System"
- `/knbn-join` — Smart-join an existing work item based on your context
- `/knbn-status "Done with JWT"` — Post a manual status update
- `/knbn-status` — Let the agent auto-generate a status update

## Architecture

```
Claude Code CLI instances → HTTP POST → localhost:9090 → VS Extension (WPF Kanban Board)
```

The extension embeds an HTTP server that receives events from Claude Code skills and hooks, maintains state in memory (persisted to `~/.knbn/events.jsonl`), and renders a kanban board with three columns: Active, Waiting, Done.
