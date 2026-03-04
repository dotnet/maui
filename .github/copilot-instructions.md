---
description: "Guidance for GitHub Copilot when working on the .NET MAUI repository."
---

# GitHub Copilot Development Environment Instructions

## Code Review Instructions

When performing a code review on PRs that change functional code, run the pr-finalize skill to verify that the PR title and description accurately match the actual implementation.

## Repository Overview

**.NET MAUI** is a cross-platform framework (Android, iOS, macOS, Windows) built with C# and XAML.

### Key Technologies

- **.NET SDK** — version always defined in `global.json`; each branch (`main`, `net10.0`, `net11.0`) maps to its .NET version
- **Cake build system** (`dotnet cake`); MSBuild build tasks (`Microsoft.Maui.BuildTasks.slnf` must be built first)
- **Testing**: xUnit (unit tests), NUnit (UI tests, `TestCases.Shared.Tests`), Appium WebDriver (UI automation)

## Development Environment Setup

Assumes: tools restored (`dotnet tool restore`), build tasks compiled, correct SDK installed.

- **Android**: OpenJDK 17 + Android SDK (`android` command after tool restore)
- **iOS/macOS**: Xcode (current stable)
- **Windows**: Windows SDK

### Azure DevOps CI Access

- **Azure CLI (`az`)** — preferred over `curl`/`Invoke-RestMethod` for CI queries
  - Install: `brew install azure-cli` (macOS) / `winget install Microsoft.AzureCLI` (Windows)
  - Setup: `az extension add --name azure-devops` (`az login` optional — `dnceng-public` is publicly accessible)
  - Defaults: `az devops configure --defaults organization=https://dev.azure.com/dnceng-public project=public`
  - For structured CI queries, use the `azdo-build-investigator` skill scripts in `.github/skills/azdo-build-investigator/`

## Project Structure

- `src/Core/` — core framework; `src/Controls/` — UI controls; `src/Essentials/` — platform APIs
- `src/TestUtils/` — test utilities; `docs/` — docs; `eng/` — build tooling; `.github/` — workflows

### Platform-Specific Code

Platform folders: `Android/`, `iOS/`, `MacCatalyst/`, `Windows/`

File extensions:
- `.windows.cs` — Windows only; `.android.cs` — Android only
- `.ios.cs` — iOS **and** MacCatalyst; `.maccatalyst.cs` — MacCatalyst only (not iOS)

⚠️ Both `.ios.cs` and `.maccatalyst.cs` compile for MacCatalyst — no precedence or exclusion between them.

### Sample Projects

- `src/Controls/samples/Maui.Controls.Sample` — full gallery
- `src/Controls/samples/Maui.Controls.Sample.Sandbox` — empty sandbox for testing/reproduction
- `src/Essentials/samples/Essentials.Sample` — Essentials API demos
- `src/BlazorWebView/samples/` — BlazorWebView samples

## Development Workflow

### Testing

- `src/Core/tests/UnitTests/Core.UnitTests.csproj`
- `src/Essentials/test/UnitTests/Essentials.UnitTests.csproj`
- `src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj`
- `src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj`

Find all: `find . -name "*.UnitTests.csproj"`

### CI Pipelines (Azure DevOps)

| Pipeline | Name | Purpose |
|----------|------|---------|
| Overall CI | `maui-pr` | Full PR validation build |
| Device Tests | `maui-pr-devicetests` | Helix-based device tests |
| UI Tests | `maui-pr-uitests` | Appium-based UI tests |

⚠️ Old names (`MAUI-UITests-public`, `MAUI-public`) are outdated — do not use.

### Code Formatting

Always format code before committing:

```bash
dotnet format Microsoft.Maui.sln --no-restore --exclude Templates/src --exclude-diagnostics CA1822
```

## Contribution Guidelines

### Handling Existing PRs for Assigned Issues

**🚨 Always develop your own solution first, then compare with existing PRs.**

1. Develop your solution independently without looking at existing PRs
2. Search for existing PRs addressing the same issue
3. Compare approaches; choose the better solution
4. In PR description, explain your choice vs. alternatives
5. Enhance whichever solution you use (tests, quality, error handling, docs)

### Auto-Generated Files (Never Commit)

- `cgmanifest.json` — generated during CI builds
- `templatestrings.json` — auto-generated localization

Always reset these files before committing.

### PublicAPI.Unshipped.txt File Management

- Never disable analyzers to bypass issues
- Always add correct API entries; use `dotnet format analyzers` if needed
- If incorrect: revert all changes, then re-add only necessary entries

### Branching
- `main` — bug fixes without API changes
- `net10.0` — new features and API changes

### Git Workflow (Copilot CLI Rules)

**🚨 Critical rules:**

1. **Never commit directly to `main`** — always use a feature branch
2. **When amending a PR, check out its branch directly** (`gh pr checkout 12345`) — do NOT create a new branch off it; CI only runs on the original PR branch
3. **No rebase, squash, or force-push** unless explicitly requested

**Before pushing:** Always stop and ask the user first — unless their instructions explicitly include pushing.

### Documentation
- Update XML documentation for public APIs
- Update `docs/` when relevant

### Opening PRs

All PRs must include this at the top of the description (without surrounding block quotes):

```
<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!
```

## Custom Agents and Skills

### Skills vs Agents

| Aspect | Skills | Agents |
|--------|--------|--------|
| Invoke | `/skill-name` or direct request | Delegate to agent |
| Output | Analysis, recommendations | Actions applied |
| Interaction | Interactive | Autonomous workflow |

### Available Custom Agents

1. **pr** — PR review/fix workflow. Triggers: "review PR #XXXXX", "work on PR #XXXXX", "fix issue #XXXXX", "continue PR #XXXXX". Do NOT use for manual testing → use `sandbox-agent`.
2. **write-tests-agent** — Writes UI/XAML tests. Triggers: "write tests for #XXXXX", "create tests", "add test coverage".
3. **sandbox-agent** — Manual testing in Sandbox app. Triggers: "test this PR", "validate PR #XXXXX in Sandbox", "reproduce issue #XXXXX". Do NOT use for code review → use `pr` agent.
4. **learn-from-pr** — Extracts lessons and applies repo improvements. Triggers: "learn from PR #XXXXX and apply improvements", "update skills based on PR". For analysis only (no changes) → use `/learn-from-pr` skill instead.

### Reusable Skills (`.github/skills/`)

| Skill | Purpose | Trigger phrases |
|-------|---------|----------------|
| **issue-triage** | Triage open issues needing milestones/labels | "find issues to triage", "what issues need attention" |
| **find-reviewable-pr** | Find PRs needing review | "find PRs to review", "find partner PRs" |
| **pr-finalize** | Verify PR title/description match implementation; code review before merge. ⚠️ NEVER `--approve`/`--request-changes` | "finalize PR", "check PR description", "review commit message" |
| **learn-from-pr** | Analyze completed PR for repo improvements (analysis only, no changes) | "what can we learn from PR #XXXXX?" |
| **write-ui-tests** | Create UI tests that reproduce a bug | "write UI tests for #XXXXX", "add UI test coverage" |
| **write-xaml-tests** | Create XAML unit tests (parsing, XamlC, source gen) | "write XAML tests for #XXXXX", "test XamlC behavior" |
| **verify-tests-fail-without-fix** | Verify tests catch the bug before fix | After creating tests, before PR is complete |
| **azdo-build-investigator** | Investigate CI failures: build errors, Helix logs, binlog analysis | "check build for PR", "why did PR build fail", "get build status" |
| **run-integration-tests** | Run integration tests locally. **ALWAYS use instead of `dotnet test`** | "run integration tests", "run macOSTemplates tests", "run RunOniOS tests" |
| **try-fix** *(internal)* | One independent fix attempt; used by pr agent Phase 3. Max 5 attempts/session. | — |

### Delegation Policy

When a request matches agent trigger phrases, **immediately delegate — no permission needed.**

- "Review PR #12345" → **pr** agent
- "Test this PR" → **sandbox-agent**
- "Fix issue #67890" (no PR exists) → suggest `/delegate` command
- "Write tests for #12345" → **write-tests-agent**
- Informational queries ("What does PR #12345 do?") → handle yourself