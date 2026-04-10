---
name: knbn-join
description: Smart-join an existing knbn work item based on current context
---

You are joining an existing work item on the knbn kanban board by analyzing your current context.

## Instructions

1. Fetch active cards from the knbn aggregator:

```bash
curl -s http://localhost:9090/cards
```

If the curl command fails (connection refused), respond: "knbn board isn't running. Open the knbn tool window in Visual Studio to start it." and stop.

2. Parse the JSON response to get the list of cards.

3. **If no cards exist:**
   - Say: "No active work items on the board. Want me to create one? What should I call it?"
   - Wait for the user's response, then POST a register event (same as /knbn skill)

4. **If cards exist:**
   - Look at your current context: working directory, any recent user prompts, files you've been working with
   - Compare against the cards' titles and project directories
   - **Confident match** (same directory or clearly related title): Say "This looks like it relates to **[card title]** — joining." and POST:
     ```bash
     curl -s -X POST http://localhost:9090/events \
       -H "Content-Type: application/json" \
       -d '{"action":"join","card_id":"CARD_ID","session_id":"SESSION_ID","cwd":"CWD"}'
     ```
   - **Uncertain** (multiple possible matches): Present the options: "I see these active work items: [list with numbers]. Which one is this related to, or should I create a new one?"
   - **No reasonable match**: Say "None of the active work items seem related to what we're doing. What should I call this work?" Then POST a register event.

5. Done. Do not take any further action.
