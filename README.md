# BetterCompactAntigravity

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14-239120)](https://learn.microsoft.com/dotnet/csharp/)
[![Platform](https://img.shields.io/badge/platform-Windows-0078D6)](#usage)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

Exports the user messages and Antigravity replies from one Antigravity conversation database to Markdown. Tool steps, task activity, generation steps and context summaries are counted in the header instead of copied into the transcript.

The message text comes from Antigravity's indexed conversation content and is written without paraphrasing or shortening. The database is opened read-only. The Markdown file is the only output written by the conversion.

## Usage

```text
BetterCompactAntigravity <conversation-id|conversation.db> [-o <output.md|folder>]
```

Pass an Antigravity conversation identifier or the path to its `.db` file. Use `-o` to name the Markdown file or its destination folder. With no output path, the file is written under `UserData\Conversations` beside the program.

```powershell
BetterCompactAntigravity 4eaec55a-fbe5-467d-90bf-f04a7d467ff4 -o C:\E\Codex\temp\BetterCompactAntigravity
```

Antigravity stores conversation databases under `%USERPROFILE%\.gemini\antigravity\conversations` and their titles and searchable message text in `conversation_summaries.db`.

## Requirements

- Windows 10 or Windows 11 with the .NET 10 runtime installed.
- An Antigravity profile with the conversation database and summary database available locally.
- The application uses the Windows `winsqlite3.dll` system library; it does not install NuGet packages or modify Antigravity databases.

## Building

```powershell
dotnet build BetterCompactAntigravity.csproj
dotnet publish BetterCompactAntigravity.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish\FrameworkDependent
```

The framework-dependent executable requires the .NET 10 runtime. A self-contained build can be made locally with `--self-contained true`; it is not included in the GitHub release.

## Skill

`skills/BetterCompactAntigravity/SKILL.md` is the Antigravity skill. Copy that directory under `.agents\skills` in the Antigravity skill root to install it.

## Licence

MIT. See [LICENSE](LICENSE).
