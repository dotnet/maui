---
description: "Guidance for GitHub Copilot when working on the .NET MAUI repository."
---

# GitHub Copilot Development Environment Instructions

This document provides specific guidance for GitHub Copilot when working on the .NET MAUI repository. It serves as context for understanding the project structure, development workflow, and best practices.

## Code Review Instructions

When performing a code review on PRs that change functional code, run the pr-finalize skill to verify that the PR title and description accurately match the actual implementation. This ensures proper documentation and helps maintain high-quality commit messages.

## Repository Overview

**.NET MAUI** is a cross-platform framework for creating mobile and desktop applications with C# and XAML. This repository contains the core framework code that enables development for Android, iOS, iPadOS, macOS, and Windows from a single shared codebase.

### Key Technologies

- **.NET SDK** - Version is **ALWAYS** defined in `global.json` at repository root
  - **main branch**: Latest stable .NET version
  - **net10.0 branch**: .NET 10 SDK
  - **Feature branches**: Each feature branch (e.g., `net11.0`, `net12.0`) correlates to its respective .NET version
- **Cake build system** for compilation and packaging (`dotnet cake`)
- **MSBuild** with custom build tasks (must build `Microsoft.Maui.BuildTasks.slnf` first)
- **Testing frameworks**:
  - **xUnit** - Unit tests (`*.UnitTests.csproj`)
  - **NUnit** - UI tests (`TestCases.Shared.Tests`)
  - **Appium WebDriver** - UI test automation

## Development Environment Setup

This guidance assumes:
- Repository is already cloned and tools are restored (`dotnet tool restore` completed)
- Build tasks are compiled (`Microsoft.Maui.BuildTasks.slnf` built successfully)
- Correct .NET SDK version installed (verify with `dotnet --version` against `global.json`)

### Platform-Specific Requirements

- **Android**: OpenJDK 17 + Android SDK (install via `android` command after `dotnet tool restore`)
- **iOS/macOS**: Xcode (current stable version)
- **Windows**: Windows SDK

## Project Structure

### Important Directories
- `src/Core/` - Core MAUI framework code
- `src/Controls/` - UI controls and components
- `src/Essentials/` - Platform APIs and essentials
- `src/TestUtils/` - Testing utilities and infrastructure
- `docs/` - Development documentation
- `eng/` - Build engineering and tooling
- `.github/` - GitHub workflows and configuration

### Platform-Specific Code Organization
- **Android** specific code is inside folders labeled `Android`
- **iOS** specific code is inside folders labeled `iOS`
- **MacCatalyst** specific code is inside folders named `MacCatalyst`
- **Windows** specific code is inside folders named `Windows`

### Platform-Specific File Extensions

Platform-specific files use naming conventions to control compilation:

**File extension patterns**:
- `.windows.cs` - Windows TFM only
- `.android.cs` - Android TFM only
- `.ios.cs` - iOS and MacCatalyst TFMs (both)
- `.maccatalyst.cs` - MacCatalyst TFM only (does NOT compile for iOS)

**Important**: Both `.ios.cs` and `.maccatalyst.cs` files compile for MacCatalyst. There is no precedence mechanism that excludes one when the other exists.

**Example**: If you have both `CollectionView.ios.cs` and `CollectionView.maccatalyst.cs`, both will compile for MacCatalyst builds. The `.maccatalyst.cs` file won't compile for iOS, but the `.ios.cs` file will compile for both iOS and MacCatalyst.

### Sample Projects

- `src/Controls/samples/Maui.Controls.Sample` - Full gallery sample with all controls and features
- `src/Controls/samples/Maui.Controls.Sample.Sandbox` - Empty project for testing/reproduction
- `src/Essentials/samples/Essentials.Sample` - Essentials API demonstrations (non-UI MAUI APIs)
- `src/BlazorWebView/samples/` - BlazorWebView sample applications

## Development Workflow

### Testing

Major test projects:
- **Core**: `src/Core/tests/UnitTests/Core.UnitTests.csproj`
- **Essentials**: `src/Essentials/test/UnitTests/Essentials.UnitTests.csproj`
- **Controls**: `src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj`
- **XAML**: `src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj`

Find all tests: `find . -name "*.UnitTests.csproj"`

### Code Formatting

Always format code before committing:

```bash
dotnet format Microsoft.Maui.sln --no-restore --exclude Templates/src --exclude-diagnostics CA1822
```

## Contribution Guidelines

### Handling Existing PRs for Assigned Issues

**🚨 CRITICAL REQUIREMENT: Always develop your own solution first, then compare with existing PRs.**

1. **Develop your own solution first** - Analyze the issue independently and design your approach without looking at existing PRs
2. **Search for existing PRs** - After developing your solution, search for open PRs addressing the same issue
3. **Compare and evaluate** - Examine existing PR approaches and decide which solution better addresses the issue
4. **Document your decision** - In your PR description, compare your solution to existing PRs and explain why you chose your approach, including concerns with alternatives
5. **Improve either solution** - Whether using your solution or an existing one, enhance with better tests, code quality, error handling, or documentation

### Auto-Generated Files (Never Commit)

These files are auto-generated and must NOT be committed:
- `cgmanifest.json` - Generated during CI builds
- `templatestrings.json` - Auto-generated localization

**For AI agents:** Always reset changes to these files before committing.

### PublicAPI.Unshipped.txt File Management

When working with public API changes:
- **Never disable analyzers** to bypass PublicAPI.Unshipped.txt issues
- **Always add correct API entries** to PublicAPI.Unshipped.txt files
- **Use `dotnet format analyzers`** if having trouble
- **If files are incorrect**: Revert all changes, then add only the necessary new API entries

### Branching
- `main` - For bug fixes without API changes
- `net10.0` - For new features and API changes

### Git Workflow (Copilot CLI Rules)

**🚨 CRITICAL Git Rules for Copilot CLI:**

1. **NEVER commit directly to `main`** - Always create a feature branch for your work. Direct commits to `main` are strictly prohibited.

2. **Do NOT rebase, squash, or force-push** unless explicitly requested by the user. These operations rewrite git history and can cause problems for other contributors. Default behavior should be regular commits and pushes.

3. **When amending an existing PR, do NOT automatically push** - After making changes to an existing PR branch, ask the user before pushing. This allows the user to review the changes locally first. Exception: If the user's instructions explicitly include pushing, proceed without asking.

**Safe Git Workflow:**
```bash
# Create a feature branch (NEVER work directly on main)
git checkout -b feature/issue-12345

# Make commits normally
git add .
git commit -m "Fix: Description of the change"

# Push to remote (for new branches)
git push -u origin feature/issue-12345

# For subsequent pushes on the same branch
git push
```

**When asked to update an existing PR:**
1. Make the requested changes
2. Stage and commit the changes
3. **STOP and ask the user** before pushing: "Changes are committed locally. Would you like me to push these changes to the PR?"

### Documentation
- Update XML documentation for public APIs
- Follow existing code documentation patterns
- Update relevant docs in `docs/` folder when needed

### Opening PRs

All PRs are required to have this at the top of the description:

```
<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!
```

Always put that at the top, without the block quotes. Without it, users will NOT be able to try the PR and your work will have been in vain!



## Custom Agents and Skills

The repository includes specialized custom agents and reusable skills for specific tasks.

### Skills vs Agents

| Aspect | Skills | Agents |
|--------|--------|--------|
| **Invoke** | `/skill-name` or direct request | Delegate to agent |
| **Output** | Analysis, recommendations | Actions, changes applied |
| **Interaction** | Interactive discussion | Autonomous workflow |
| **Example** | `/learn-from-pr` → recommendations | learn-from-pr agent → applies changes |

### Available Custom Agents

1. **pr** - Sequential 4-phase workflow for reviewing and working on PRs
   - **Use when**: A PR already exists and needs review or work, OR an issue needs a fix
   - **Capabilities**: PR review, test verification, fix exploration, alternative comparison
   - **Trigger phrases**: "review PR #XXXXX", "work on PR #XXXXX", "fix issue #XXXXX", "continue PR #XXXXX"
   - **Do NOT use for**: Just running tests manually → Use `sandbox-agent`

2. **write-tests-agent** - Agent for writing tests. Determines test type (UI vs XAML) and invokes the appropriate skill (`write-ui-tests`, `write-xaml-tests`)
   - **Use when**: Creating new tests for issues or PRs
   - **Capabilities**: Test type determination (UI and XAML), skill invocation, test verification
   - **Trigger phrases**: "write tests for #XXXXX", "create tests", "add test coverage"

3. **sandbox-agent** - Specialized agent for working with the Sandbox app for testing, validation, and experimentation
   - **Use when**: User wants to manually test PR functionality or reproduce issues
   - **Capabilities**: Sandbox app setup, Appium-based manual testing, PR functional validation
   - **Trigger phrases**: "test this PR", "validate PR #XXXXX in Sandbox", "reproduce issue #XXXXX", "try out in Sandbox"
   - **Do NOT use for**: Code review (use pr agent), writing automated tests (use write-tests-agent)

4. **learn-from-pr** - Extracts lessons from PRs and applies improvements to the repository
   - **Use when**: After complex PR, want to improve instruction files/skills based on lessons learned
   - **Capabilities**: Analyzes PR, identifies failure modes, applies improvements to instruction files, skills, code comments
   - **Trigger phrases**: "learn from PR #XXXXX and apply improvements", "improve repo based on what we learned", "update skills based on PR"
   - **Output**: Applied changes to instruction files, skills, architecture docs, code comments
   - **Do NOT use for**: Analysis only without applying changes → Use `/learn-from-pr` skill instead

### Reusable Skills

Skills are modular capabilities that can be invoked directly or used by agents. Located in `.github/skills/`:

#### User-Facing Skills

1. **issue-triage** (`.github/skills/issue-triage/SKILL.md`)
   - **Purpose**: Query and triage open issues that need milestones, labels, or investigation
   - **Trigger phrases**: "find issues to triage", "show me old Android issues", "what issues need attention"
   - **Scripts**: `init-triage-session.ps1`, `query-issues.ps1`, `record-triage.ps1`

2. **find-reviewable-pr** (`.github/skills/find-reviewable-pr/SKILL.md`)
   - **Purpose**: Finds open PRs in dotnet/maui and dotnet/docs-maui that need review
   - **Trigger phrases**: "find PRs to review", "show milestoned PRs", "find partner PRs"
   - **Scripts**: `query-reviewable-prs.ps1`
   - **Categories**: P/0, milestoned, partner, community, recent, docs-maui

3. **pr-finalize** (`.github/skills/pr-finalize/SKILL.md`)
   - **Purpose**: Verifies PR title and description match actual implementation, AND performs code review for best practices before merge.
   - **Trigger phrases**: "finalize PR #XXXXX", "check PR description for #XXXXX", "review commit message"
   - **Used by**: Before merging any PR, when description may be stale
   - **Note**: Works on any PR
   - **🚨 CRITICAL**: NEVER use `--approve` or `--request-changes` - only post comments. Approval is a human decision.

4. **learn-from-pr** (`.github/skills/learn-from-pr/SKILL.md`)
   - **Purpose**: Analyzes completed PR to identify repository improvements (analysis only, no changes applied)
   - **Trigger phrases**: "what can we learn from PR #XXXXX?", "how can we improve agents based on PR #XXXXX?"
   - **Used by**: After complex PRs, when agent struggled to find solution
   - **Output**: Prioritized recommendations for instruction files, skills, code comments
   - **Note**: For applying changes automatically, use the learn-from-pr agent instead

5. **write-ui-tests** (`.github/skills/write-ui-tests/SKILL.md`)
   - **Purpose**: Creates UI tests for GitHub issues and verifies they reproduce the bug
   - **Trigger phrases**: "write UI tests for #XXXXX", "create UI test for issue", "add UI test coverage"
   - **Output**: Test files that fail without fix, pass with fix

6. **write-xaml-tests** (`.github/skills/write-xaml-tests/SKILL.md`)
   - **Purpose**: Creates XAML unit tests for XAML parsing, compilation, and source generation
   - **Trigger phrases**: "write XAML tests for #XXXXX", "test XamlC behavior", "reproduce XAML parsing bug"
   - **Output**: Test files for Controls.Xaml.UnitTests

7. **verify-tests-fail-without-fix** (`.github/skills/verify-tests-fail-without-fix/SKILL.md`)
   - **Purpose**: Verifies UI tests catch the bug before fix and pass with fix
   - **Two modes**: Verify failure only (test creation) or full verification (test + fix)
   - **Used by**: After creating tests, before considering PR complete

8. **pr-build-status** (`.github/skills/pr-build-status/SKILL.md`)
   - **Purpose**: Retrieves Azure DevOps build information for PRs (build IDs, stage status, failed jobs)
   - **Trigger phrases**: "check build for PR #XXXXX", "why did PR build fail", "get build status"
   - **Used by**: When investigating CI failures

8. **run-integration-tests** (`.github/skills/run-integration-tests/SKILL.md`)
   - **Purpose**: Build, pack, and run .NET MAUI integration tests locally
   - **Trigger phrases**: "run integration tests", "test templates locally", "run macOSTemplates tests", "run RunOniOS tests"
   - **Categories**: Build, WindowsTemplates, macOSTemplates, Blazor, MultiProject, Samples, AOT, RunOnAndroid, RunOniOS
   - **Note**: **ALWAYS use this skill** instead of manual `dotnet test` commands for integration tests

#### Internal Skills (Used by Agents)

9. **try-fix** (`.github/skills/try-fix/SKILL.md`)
   - **Purpose**: Proposes ONE independent fix approach, applies it, tests, records result with failure analysis, then reverts
   - **Used by**: pr agent Phase 3 (Fix phase) - rarely invoked directly by users
   - **Behavior**: Reads prior attempts to learn from failures. Max 5 attempts per session.
   - **Output**: Reports attempt results and failure analysis

### Agent Workflow Labels

Labels with `s/agent-*` prefix track agent workflow outcomes for metrics. Applied by `Review-PR.ps1` Phase 4.

**Outcome Labels** (mutually exclusive — one per PR):

| Label | Description |
|-------|-------------|
| `s/agent-approved` | AI agent recommends approval |
| `s/agent-changes-requested` | AI agent recommends changes |
| `s/agent-review-incomplete` | AI agent could not complete all phases |

**Signal Labels** (additive):

| Label | Description |
|-------|-------------|
| `s/agent-gate-passed` | AI verified tests catch the bug |
| `s/agent-gate-failed` | AI could not verify tests catch the bug |
| `s/agent-fix-optimal` | AI confirms PR fix is the best among candidates |

**Manual Labels** (applied by maintainers):

| Label | Description |
|-------|-------------|
| `s/agent-fix-implemented` | PR author implemented the agent's suggested fix |

**Base Label**: `s/agent-reviewed` — always applied on completed agent runs.

**Helper module**: `.github/scripts/helpers/Update-AgentLabels.ps1`

### Agent Workflow Labels

Labels with `s/agent-*` prefix track agent workflow outcomes for metrics. Applied by `Review-PR.ps1` Phase 4.

**Outcome Labels** (mutually exclusive — one per PR):

| Label | Description |
|-------|-------------|
| `s/agent-approved` | AI agent recommends approval |
| `s/agent-changes-requested` | AI agent recommends changes |
| `s/agent-review-incomplete` | AI agent could not complete all phases |

**Signal Labels** (additive):

| Label | Description |
|-------|-------------|
| `s/agent-gate-passed` | AI verified tests catch the bug |
| `s/agent-gate-failed` | AI could not verify tests catch the bug |
| `s/agent-fix-optimal` | AI confirms PR fix is the best among candidates |

**Manual Labels** (applied by maintainers):

| Label | Description |
|-------|-------------|
| `s/agent-fix-implemented` | PR author implemented the agent's suggested fix |

**Base Label**: `s/agent-reviewed` — always applied on completed agent runs.

**Helper module**: `.github/scripts/helpers/Update-AgentLabels.ps1`

### Using Custom Agents

**Delegation Policy**: When user request matches agent trigger phrases, **ALWAYS delegate to the appropriate agent immediately**. Do not ask for permission or explain alternatives unless the request is ambiguous.

**Examples of correct delegation**:
- User: "Review PR #12345" → Immediately invoke **pr** agent
- User: "Test this PR" → Immediately invoke **sandbox-agent**
- User: "Fix issue #67890" (no PR exists) → Suggest using `/delegate` command
- User: "Write tests for issue #12345" → Immediately invoke **write-tests-agent**

**When NOT to delegate**:
- User asks "What does PR #12345 do?" → Informational query, handle yourself
- User asks "How do I test PRs?" → Documentation query, handle yourself
- User has follow-up questions after agent completes → Continue the conversation yourself