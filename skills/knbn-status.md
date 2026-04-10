---
name: knbn-status
description: Update the knbn board with a status message. Usage: /knbn-status "message" or /knbn-status (auto-generates)
---

You are updating the knbn kanban board with a status message for this session's work item.

## Instructions

1. **If the user provided a message** (e.g., `/knbn-status "Finished auth middleware"`):
   - Use that message as-is.

2. **If no message was provided** (just `/knbn-status`):
   - Review what you've accomplished in this conversation since your last status update (or since the start if no prior updates).
   - Summarize it in one concise sentence. Focus on what was completed or what milestone was reached, not what you're about to do.
   - Examples: "Implemented JWT validation and added route guards" or "Fixed race condition in session cleanup"

3. POST the status update:

```bash
curl -s -X POST http://localhost:9090/events \
  -H "Content-Type: application/json" \
  -d '{"action":"status","session_id":"SESSION_ID","message":"THE_MESSAGE"}'
```

Replace `SESSION_ID` and `THE_MESSAGE` appropriately.

4. Parse the response:
   - If `outcome` is `"updated"`: respond "Status updated: **THE_MESSAGE**"
   - If `outcome` is `"not_found"`: respond "This session isn't linked to a work item. Use `/knbn` or `/knbn-join` first."
   - If curl fails: respond "knbn board isn't running. Open the knbn tool window in Visual Studio to start it."

5. Done. Do not take any further action.
