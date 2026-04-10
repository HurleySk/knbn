# knbn — Claude Code Kanban Board for Visual Studio Enterprise

## Overview

A Visual Studio Enterprise extension that provides a kanban board for tracking work across multiple Claude Code CLI instances. Users register work items via a Claude Code skill (`/knbn`), and the board aggregates and visualizes that work in a dockable tool window.

## Problem

When running multiple Claude Code CLI sessions in Visual Studio Enterprise terminals, there's no way to get a high-level view of what each instance is working on, what's active, what's waiting, and what's done. You have to check each terminal individually.

## Solution

Three components working together:

1. **Claude Code skills** — `/knbn` commands that let agents register and update work items
2. **Local aggregator service** — HTTP server embedded in the VS extension that receives events and maintains state
3. **VS Enterprise extension** — WPF tool window rendering a kanban board

## Architecture

```
┌─────────────────────┐     HTTP POST     ┌─────────────────────────┐     In-process     ┌──────────────────────┐
│  Claude Code CLI 1  │──────────────────▶│   Local Aggregator      │◀──────────────────▶│  VS Extension UI     │
│  (auth-service)     │                   │   localhost:9090        │                     │  (WPF Tool Window)   │
├─────────────────────┤                   │                         │                     │                      │
│  Claude Code CLI 2  │──────────────────▶│  - POST /events         │                     │  Kanban Board:       │
│  (frontend)         │                   │  - GET /cards           │                     │  Active | Waiting |  │
├─────────────────────┤                   │  - In-memory state      │                     │  Done                │
│  Claude Code CLI 3  │──────────────────▶│  - events.jsonl log     │                     │                      │
│  (api-gateway)      │                   └─────────────────────────┘                     └──────────────────────┘
└─────────────────────┘

Hook config: ~/.claude/settings.json (user-level, applies to all sessions)
```

The aggregator is embedded inside the VS extension process (Kestrel or HttpListener). One component to install.

## Skills

### `/knbn "Title"` — Register or Link Work

1. POSTs to aggregator: `{ action: "register", title, session_id, cwd }`
2. Aggregator checks for existing card with fuzzy title match
   - Match found → links session to existing card, responds "Joined 'Auth System'"
   - No match → creates new card, responds "Created 'Auth System'"
3. One-line confirmation, exits. Minimal context cost.

### `/knbn join` — Smart Self-Classification

1. GETs `/cards` from aggregator (list of active work items)
2. If no cards exist → asks user what to call this work
3. If cards exist → agent reads its own context (cwd, user prompt, files) and picks the best match
   - Confident match → "This looks like 'Auth System' work — joining." (user can override)
   - Uncertain → presents options, asks user to pick or name a new one
   - No reasonable match → asks user what to call this work
4. POSTs decision to aggregator

### `/knbn status "message"` — Manual Status Update

1. POSTs to aggregator: `{ action: "status", session_id, message }`
2. Updates the "Latest note" on the card
3. One-line confirmation, exits.

### `/knbn status` (no argument) — Agent Self-Reports

1. Agent summarizes what it's accomplished since its last status update in one sentence
2. POSTs the generated summary to the aggregator
3. One-line confirmation, exits.

## Data Model

### WorkItem

```
WorkItem {
  id: string               // auto-generated UUID
  title: string            // from /knbn invocation
  status: Active | Done
  created_at: datetime
  updated_at: datetime
  sessions: Session[]
  notes: Note[]
}
```

### Session

```
Session {
  session_id: string       // Claude Code session ID
  cwd: string              // working directory
  joined_at: datetime
  ended_at: datetime?
  active: boolean
}
```

### Note

```
Note {
  message: string
  session_id: string
  created_at: datetime
}
```

### Status Rules

- A card's data status is either **Active** or **Done**
- A card is **Active** if any of its sessions are still running
- A card moves to **Done** only when manually moved by the user on the board (no auto-done on session end to avoid false signals)
- A card with no session activity for 24h+ gets a visual "stale" indicator (dimmed)
- The **Waiting** column in the UI is not a data status — it's a derived view state. An Active card is shown in the Waiting column when all its sessions have ended but the user hasn't moved it to Done yet. This signals "no agent is actively working on this."

## Hook Configuration

A single `SessionEnd` hook in `~/.claude/settings.json`:

```json
{
  "hooks": {
    "SessionEnd": [
      {
        "type": "http",
        "url": "http://localhost:9090/events",
        "method": "POST"
      }
    ]
  }
}
```

This is the only hook needed. The skills handle registration and status updates directly via HTTP. The hook handles cleanup when a session ends.

## Kanban Board UI

### Layout

Three-column board in a VS Enterprise dockable tool window:

- **Active** (green) — work items with running sessions
- **Waiting** (yellow) — work items where all sessions are idle at a prompt
- **Done** (grey) — manually completed work items

### Card Contents

Each card displays:

- **Title** — from `/knbn` invocation
- **Project** — folder name derived from working directory
- **Session count** — number of Claude instances contributing
- **Latest note** — most recent `/knbn status` update
- **Last updated** — relative timestamp
- **Color-coded left border** — matches column color

### Top Bar

- Card count summary
- Settings access

### Technology

- VS Enterprise extension (VSIX, VS SDK)
- WPF/XAML for the tool window and kanban board
- C# for all logic
- Kestrel or HttpListener for the embedded HTTP server

## Aggregator API

### `POST /events`

Receives hook events and skill actions.

**Register:**
```json
{ "action": "register", "title": "Auth System", "session_id": "abc", "cwd": "/path" }
```

**Join:**
```json
{ "action": "join", "card_id": "uuid", "session_id": "abc", "cwd": "/path" }
```

**Status update:**
```json
{ "action": "status", "session_id": "abc", "message": "Completed JWT middleware" }
```

**Session end (from hook):**
```json
{ "event": "SessionEnd", "session_id": "abc" }
```

### `GET /cards`

Returns all work items with their sessions and notes. Used by the skill (`/knbn join`) and the UI.

```json
{
  "cards": [
    {
      "id": "uuid",
      "title": "Auth System",
      "status": "Active",
      "sessions": [...],
      "notes": [...],
      "created_at": "...",
      "updated_at": "..."
    }
  ]
}
```

## Persistence

- Events appended to `~/.knbn/events.jsonl` as they arrive
- Aggregator rehydrates from this file on VS startup
- Board state survives VS restarts

## Error Handling

- **Aggregator not running** — skill POST fails, skill tells user to open the knbn tool window in VS
- **Duplicate titles** — case-insensitive exact match on register ("Auth System" and "auth system" link to the same card; "Auth System v2" creates a new card)
- **Stale cards** — 24h+ no activity gets dimmed visual indicator, no auto-archive
- **VS restarts** — rehydrate from events.jsonl

## Scope

### In Scope

- VS Enterprise extension with WPF kanban board
- Embedded HTTP aggregator
- Three Claude Code skills (`/knbn`, `/knbn join`, `/knbn status`)
- One Claude Code hook (`SessionEnd`)
- Local-only operation (single machine)
- Event log persistence

### Out of Scope (future)

- Remote/multi-machine support
- Card drag-and-drop reordering
- Custom columns beyond Active/Waiting/Done
- Notifications or alerts
- Integration with issue trackers (Jira, Linear, etc.)
