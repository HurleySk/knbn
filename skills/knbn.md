---
name: knbn
description: Register current work with the knbn kanban board. Usage: /knbn "Title of work item"
---

You are registering this Claude Code session's work with the knbn kanban board.

## Instructions

The user has invoked `/knbn` with a work item title. Register this session with the knbn aggregator.

1. Extract the title from the user's arguments. If no title was provided, ask: "What should I call this work item?"

2. Use the Bash tool to POST to the knbn aggregator:

```bash
curl -s -X POST http://localhost:9090/events \
  -H "Content-Type: application/json" \
  -d '{"action":"register","title":"THE_TITLE","session_id":"SESSION_ID","cwd":"CURRENT_WORKING_DIR"}'
```

Replace:
- `THE_TITLE` with the work item title
- `SESSION_ID` with a unique identifier for this session (use the current timestamp + random suffix if no session ID is available)
- `CURRENT_WORKING_DIR` with the current working directory

3. Parse the JSON response:
   - If `outcome` is `"created"`: respond "Created card: **THE_TITLE**"
   - If `outcome` is `"joined"`: respond "Joined existing card: **THE_TITLE**"
   - If the curl command fails (connection refused): respond "knbn board isn't running. Open the knbn tool window in Visual Studio to start it."

4. Done. Do not take any further action.
