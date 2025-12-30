---
description: "Guidance for GitHub Copilot when working on the .NET MAUI repository."
---

# GitHub Copilot Development Environment Instructions

This document provides specific guidance for GitHub Copilot when working on the .NET MAUI repository. It serves as context for understanding the project structure, development workflow, and best practices.

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



## Custom Agents

The repository includes specialized custom agents for specific tasks. These agents are available to GitHub Copilot and can be invoked for their respective specializations:

### Available Custom Agents

1. **issue-resolver** - Specialized agent for investigating and resolving community-reported .NET MAUI issues through hands-on testing and implementation
   - **Use when**: Working on bug fixes from GitHub issues
   - **Capabilities**: Issue reproduction, root cause analysis, fix implementation, testing
   - **Trigger phrases**: "fix issue #XXXXX", "resolve bug #XXXXX", "implement fix for #XXXXX"

2. **pr-reviewer** - Specialized agent for conducting thorough, constructive code reviews of .NET MAUI pull requests
   - **Use when**: User requests code review of a pull request
   - **Capabilities**: Code quality analysis, best practices validation, test coverage review
   - **Trigger phrases**: "review PR #XXXXX", "review pull request #XXXXX", "code review for PR #XXXXX", "review this PR"
   - **Do NOT use for**: Building/testing PR functionality (use Sandbox), asking about PR details (handle yourself)

3. **pr-reviewer-detailed** - Specialized agent for deep, independent PR reviews that challenge assumptions and propose alternative solutions
   - **Use when**: User wants thorough analysis with alternative fix proposals
   - **Capabilities**: Independent root cause analysis, alternative fix implementation, comparative testing
   - **Trigger phrases**: "deep review PR #XXXXX", "detailed review", "analyze and propose alternatives", "challenge this PR's approach"
   - **Skills used**: `assess-test-type`, `validate-ui-tests`, `validate-unit-tests`, `independent-fix-analysis`, `compare-fix-approaches`

4. **uitest-coding-agent** - Specialized agent for writing new UI tests for .NET MAUI with proper syntax, style, and conventions
   - **Use when**: Creating new UI tests or updating existing ones
   - **Capabilities**: UI test authoring, Appium WebDriver usage, NUnit test patterns
   - **Trigger phrases**: "write UI test for #XXXXX", "create UI tests", "add test coverage"

5. **sandbox-agent** - Specialized agent for working with the Sandbox app for testing, validation, and experimentation
   - **Use when**: User wants to manually test PR functionality or reproduce issues
   - **Capabilities**: Sandbox app setup, Appium-based manual testing, PR functional validation
   - **Trigger phrases**: "test this PR", "validate PR #XXXXX in Sandbox", "reproduce issue #XXXXX", "try out in Sandbox"
   - **Do NOT use for**: Code review (use pr-reviewer), writing automated tests (use uitest-coding-agent)

### Reusable Skills

Skills are modular capabilities that agents can invoke. Located in `.github/skills/`:

1. **assess-test-type** (`.github/skills/assess-test-type/SKILL.md`)
   - **Purpose**: Determines whether tests should be UI tests or unit tests
   - **Trigger phrases**: "should this be a UI test or unit test", "what type of test is appropriate"
   - **Used by**: `pr-reviewer-detailed` agent

2. **validate-ui-tests** (`.github/skills/validate-ui-tests/SKILL.md`)
   - **Purpose**: Validates UI tests correctly fail without fix and pass with fix
   - **Trigger phrases**: "validate the UI tests", "check if UI tests catch the regression"
   - **Used by**: `pr-reviewer-detailed` agent

3. **validate-unit-tests** (`.github/skills/validate-unit-tests/SKILL.md`)
   - **Purpose**: Validates unit tests correctly fail without fix and pass with fix
   - **Trigger phrases**: "validate the unit tests", "check if unit tests catch the regression"
   - **Used by**: `pr-reviewer-detailed` agent

4. **independent-fix-analysis** (`.github/skills/independent-fix-analysis/SKILL.md`)
   - **Purpose**: Analyze issue and propose fixes independently before comparing with PR
   - **Trigger phrases**: "analyze this issue independently", "propose your own fix"
   - **Used by**: `pr-reviewer-detailed` agent

5. **compare-fix-approaches** (`.github/skills/compare-fix-approaches/SKILL.md`)
   - **Purpose**: Compare multiple fix approaches by testing against same UI tests
   - **Trigger phrases**: "compare the approaches", "which fix is better"
   - **Used by**: `pr-reviewer-detailed` agent

6. **find-reviewable-pr** (`.github/skills/find-reviewable-pr/SKILL.md`)
   - **Purpose**: Find open PRs that are ready for review based on platform, recency, complexity, and project status
   - **Trigger phrases**: "find a PR to review", "find an easy Android PR", "what PRs are available for review"
   - **Used by**: Any agent or direct invocation

7. **issue-triage** (`.github/skills/issue-triage/SKILL.md`)
   - **Purpose**: Query and triage open issues that need milestones, labels, or investigation
   - **Trigger phrases**: "find issues to triage", "show me old Android issues", "what issues need attention"
   - **Script**: `.github/skills/issue-triage/scripts/QueryTriageIssues.ps1`
   - **Used by**: Any agent or direct invocation

### Using Custom Agents

**Delegation Policy**: When user request matches agent trigger phrases, **ALWAYS delegate to the appropriate agent immediately**. Do not ask for permission or explain alternatives unless the request is ambiguous.

**Examples of correct delegation**:
- User: "Review PR #12345" → Immediately invoke **pr-reviewer** agent
- User: "Deep review PR #12345" → Immediately invoke **pr-reviewer-detailed** agent
- User: "Test this PR" → Immediately invoke **sandbox-agent**
- User: "Fix issue #67890" → Immediately invoke **issue-resolver** agent
- User: "Write UI test for CollectionView" → Immediately invoke **uitest-coding-agent**

**When NOT to delegate**:
- User asks "What does PR #12345 do?" → Informational query, handle yourself
- User asks "How do I test PRs?" → Documentation query, handle yourself
- User has follow-up questions after agent completes → Continue the conversation yourself