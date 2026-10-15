---
name: BetterCompactAntigravity
description: Use when the user asks to export an Antigravity conversation to a faithful, compact Markdown transcript, or invokes BetterCompactAntigravity.
---

# BetterCompactAntigravity

Export the selected Antigravity conversation as Markdown while retaining the original user messages and Antigravity replies. Tool calls and task activity stay out of the body; the header reports their counts. Message text is copied from Antigravity's searchable conversation records without summarizing or shortening it.

## Steps

1. Identify the exact target conversation. Use the conversation identifier from the user's request or current task context. If the user names a prior chat, search Antigravity's `conversation_summaries.db` for its title and use the matching `conversation_id`; do not choose a chat only because it is the newest.
2. Choose the output folder. For a requested Markdown file in the current workspace, create a topic-named subfolder under `C:\E\Codex\temp\` and use it as the destination.
3. Run the installed program with the conversation identifier and destination folder:

   ```powershell
   & 'C:\E\kp\aaBenimProgramlarim\BetterCompactAntigravity\BetterCompactAntigravity.exe' '<conversation-id>' --output '<destination-folder>'
   ```

4. Read the console report. It gives the output path and counts of retained messages, excluded Antigravity steps, and input/output sizes. Report the counts without changing them.
5. Give the full Markdown path. If the user asked to open the result and the host offers a file-open tool, open that exact file.

## Input and output

- Antigravity conversations are stored in `%USERPROFILE%\.gemini\antigravity\conversations\<conversation-id>.db`.
- Conversation titles and searchable user/assistant message bodies are stored in `%USERPROFILE%\.gemini\antigravity\conversation_summaries.db`.
- The program opens both databases read-only. It writes the Markdown output to the folder passed after `--output`.
- The generated file is named `<conversation-id>.compact.md`.
- Never select a different conversation, modify the Antigravity databases, or replace the message text with a summary.
