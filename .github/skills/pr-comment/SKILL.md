---
name: pr-comment
description: Posts or updates automated progress comments on GitHub PRs. Use after completing any PR agent phase (pre-flight, tests, gate, fix, report). Triggers on 'post comment to PR', 'update PR progress', 'comment on PR with results', 'post pre-flight comment'. Creates single aggregated review comment with collapsible sections per commit.
metadata:
  author: dotnet-maui
  version: "5.0"
compatibility: Requires GitHub CLI (gh) authenticated with access to dotnet/maui repository.
---

# PR Comment Skill

This skill posts automated progress comments to GitHub Pull Requests during the PR review workflow. Comments are **self-contained** with collapsible Review Session details, providing rich context to maintainers and contributors.

**⚠️ Self-Contained Rule**: All content in PR comments must be self-contained. Never reference local files like `CustomAgentLogsTmp/` - GitHub users cannot access your local filesystem.

**✨ Key Features**:
- **Single Unified Comment**: ONE comment per PR/Issue containing ALL sections (PR Review, Try-Fix, Write-Tests, Verify-Tests)
- **Section-Based Updates**: Each script updates only its section, preserving others
- **Duplicate Prevention**: Finds existing `<!-- AI Summary -->` comment and updates it
- **File-Based DryRun Preview**: Use `-DryRun` to preview changes in a local file before posting
- **Auto-Loading State Files**: Automatically finds and loads state files from `CustomAgentLogsTmp/PRState/`
- **Simple Interface**: Just provide PR number - script handles everything else

## Comment Architecture

### Unified AI Summary Comment

Most scripts post to the **same single comment** identified by `<!-- AI Summary -->`. Each script updates its own section:

```markdown
<!-- AI Summary -->

## 🤖 AI Summary

<!-- SECTION:PR-REVIEW -->
... PR review phases ...
<!-- /SECTION:PR-REVIEW -->

<!-- SECTION:TRY-FIX -->
... try-fix attempts ...
<!-- /SECTION:TRY-FIX -->

<!-- SECTION:WRITE-TESTS -->
... write-tests attempts ...
<!-- /SECTION:WRITE-TESTS -->

<!-- SECTION:VERIFY-TESTS -->
... test verification results ...
<!-- /SECTION:VERIFY-TESTS -->
```

**Behavior:**
- First script to run creates the comment
- Subsequent scripts find the existing comment and update/add their section
- Sections are independent - updating one preserves others

### Separate PR Finalization Comment

The `post-pr-finalize-comment.ps1` script posts a **separate comment** identified by `<!-- PR-FINALIZE-COMMENT -->`. This is intentional because:
- Finalization reviews can happen multiple times (after each commit)
- Each review is numbered (Review 1, Review 2, etc.)
- Keeps finalization reviews distinct from automated analysis

## Section Scripts

### AI Summary Sections (Unified Comment)

| Section | Script | Location |
|---------|--------|----------|
| `PR-REVIEW` | `post-pr-comment.ps1` | `.github/skills/pr-comment/scripts/` |
| `TRY-FIX` | `post-try-fix-comment.ps1` | `.github/skills/pr-comment/scripts/` |
| `WRITE-TESTS` | `post-write-tests-comment.ps1` | `.github/skills/pr-comment/scripts/` |
| `VERIFY-TESTS` | `post-verify-tests-comment.ps1` | `.github/skills/pr-comment/scripts/` |

### Separate Comments

| Comment | Script | Marker |
|---------|--------|--------|
| PR Finalization | `post-pr-finalize-comment.ps1` | `<!-- PR-FINALIZE-COMMENT -->` |

## Supported Phases

| Phase | Description | When to Post | What This Enables Next |
|-------|-------------|--------------|------------------------|
| `pre-flight` | Context gathering complete | After documenting issue, files, and discussion | **Tests Phase**: Agent can now verify/create test files that reproduce the bug |
| `tests` | Test analysis complete | After identifying test files and coverage | **Gate Phase**: Agent can run tests to verify they catch the bug |
| `gate` | Test validation complete | After running tests and verifying bug reproduction | **Fix Phase**: Agent can explore alternative fixes (tests proven to catch bug) |
| `fix` | Solution comparison complete | After comparing PR fix with alternatives | **Report Phase**: Agent can finalize recommendation based on fix comparison |
| `report` | Final analysis complete | After generating comprehensive review | **PR Decision**: Maintainers can approve/merge or request changes based on full analysis |

## Usage

### Simplest: Just provide PR number

```bash
# Auto-loads CustomAgentLogsTmp/PRState/pr-27246.md
pwsh .github/skills/pr-comment/scripts/post-pr-comment.ps1 -PRNumber 27246
```

### With explicit state file path

```bash
# PRNumber auto-extracted from filename (pr-27246.md → 27246)
pwsh .github/skills/pr-comment/scripts/post-pr-comment.ps1 -StateFile CustomAgentLogsTmp/PRState/pr-27246.md
```

### Legacy: Provide content directly

```bash
pwsh .github/skills/pr-comment/scripts/post-pr-comment.ps1 -PRNumber 12345 -Content "$(cat CustomAgentLogsTmp/PRState/pr-12345.md)"
```

### Parameters

| Parameter | Required | Description | Example |
|-----------|----------|-------------|---------|
| `PRNumber` | No* | Pull request number | `12345` |
| `StateFile` | No* | Path to state file (PRNumber auto-extracted from `pr-XXXXX.md` naming) | `CustomAgentLogsTmp/PRState/pr-27246.md` |
| `Content` | No* | Full state file content (legacy, can be piped via stdin) | Content from state file |
| `DryRun` | No | Preview changes in local file instead of posting to GitHub | `-DryRun` |
| `PreviewFile` | No | Path to local preview file for DryRun mode (default: `CustomAgentLogsTmp/PRState/{PRNumber}/pr-comment-preview.md`) | `-PreviewFile ./preview.md` |
| `SkipValidation` | No | Skip validation checks (not recommended) | `-SkipValidation` |

*At least one of PRNumber, StateFile, or Content is required. The script will:
- If `-PRNumber` provided: Auto-load `CustomAgentLogsTmp/PRState/pr-{PRNumber}.md`
- If `-StateFile` provided: Load the file and extract PRNumber from `pr-XXXXX.md` filename
- If `-Content` provided: Use content directly (legacy, requires `-PRNumber`)

## DryRun Preview Workflow

Use `-DryRun` to preview the combined comment before posting to GitHub. Each script updates the same preview file, mirroring how the actual GitHub comment is updated.

```bash
# Step 1: Run verify-tests script (creates preview file)
pwsh .github/skills/pr-comment/scripts/post-verify-tests-comment.ps1 -PRNumber 32891 -DryRun

# Step 2: Run try-fix script (updates same preview file)
pwsh .github/skills/pr-comment/scripts/post-try-fix-comment.ps1 -IssueNumber 32891 -DryRun

# Step 3: Review the combined preview
open CustomAgentLogsTmp/PRState/32891/pr-comment-preview.md

# Step 4: Post for real (remove -DryRun)
pwsh .github/skills/pr-comment/scripts/post-verify-tests-comment.ps1 -PRNumber 32891
pwsh .github/skills/pr-comment/scripts/post-try-fix-comment.ps1 -IssueNumber 32891
```

**Key behavior:** The preview file exactly matches what will be posted to GitHub. Multiple scripts accumulate their sections in the same file.

### Section Ordering

Sections appear in the unified comment in this order (based on which scripts run first):
1. **VERIFY-TESTS** - Test verification results
2. **TRY-FIX** - Alternative fix exploration attempts
3. **WRITE-TESTS** - Test writing attempts
4. **PR-REVIEW** - PR review phases

Each section is wrapped with markers like `<!-- SECTION:TRY-FIX -->` and `<!-- /SECTION:TRY-FIX -->`.

### Cleanup

To reset the preview file for a fresh start:
```bash
rm CustomAgentLogsTmp/PRState/{PRNumber}/pr-comment-preview.md
```

### Prerequisites

Scripts require GitHub CLI authentication:
```bash
gh auth status  # Verify authentication before running
```

## Comment Format

Comments are formatted with:
- **Phase badge** (🔍 Pre-Flight, 🧪 Tests, 🚦 Gate, 🔧 Fix, 📋 Report)
- **Status indicator** (✅ Completed, ⚠️ Issues Found)
- **Expandable review sessions** (each session is a collapsible section)
- **What's Next** (what phase happens next)

### Review Session Tracking

When the same PR is reviewed multiple times (e.g., after new commits), the script **updates the single aggregated review comment** and adds a new expandable section for each commit-based review session.

### Example Output

```markdown
## 🔍 Pre-Flight: Context Gathering Complete

✅ **Status**: Phase completed successfully

### Summary
- **Issue**: #33356 - CollectionView crash on iOS
- **Platforms Affected**: iOS, MacCatalyst
- **Files Changed**: 2 implementation files, 1 test file
- **Discussion**: 3 key reviewer comments identified

### Key Findings
- Crash occurs when scrolling rapidly with large datasets
- Existing PR adds null check in ItemsViewController
- Test coverage includes iOS device test

### Next Steps
→ **Phase 2: Tests** - Analyzing test files and coverage

---
*Posted by PR Agent @ 2026-01-17 14:23:45 UTC*
```

## Script Files

- [`post-pr-comment.ps1`](scripts/post-pr-comment.ps1) - Posts or updates the aggregated PR agent review comment
- [`post-try-fix-comment.ps1`](scripts/post-try-fix-comment.ps1) - Posts or updates try-fix attempts comment

## Try-Fix Comment Script

The `post-try-fix-comment.ps1` script updates the `<!-- SECTION:TRY-FIX -->` section of the unified AI Summary comment. It aggregates all try-fix attempts into collapsible sections. Works for both issues and PRs (GitHub treats PR comments as issue comments).

**✨ Auto-Loading from `CustomAgentLogsTmp`**: The script automatically discovers and aggregates ALL attempt directories from `CustomAgentLogsTmp/PRState/{IssueNumber}/try-fix/`.

### Usage

#### Simplest: Provide attempt directory

```powershell
# All parameters auto-loaded from directory structure
pwsh .github/skills/pr-comment/scripts/post-try-fix-comment.ps1 `
    -TryFixDir CustomAgentLogsTmp/PRState/27246/try-fix/attempt-1
```

#### Or just provide issue number

```powershell
# Auto-discovers and posts latest attempt from CustomAgentLogsTmp/PRState/27246/try-fix/
pwsh .github/skills/pr-comment/scripts/post-try-fix-comment.ps1 -IssueNumber 27246
```

#### Legacy: Manual parameters

```powershell
pwsh .github/skills/pr-comment/scripts/post-try-fix-comment.ps1 `
    -IssueNumber 19806 `
    -AttemptNumber 1 `
    -Approach "LayoutExtensions Width Constraint" `
    -RootCause "ComputeFrame only constrains width for Fill alignment" `
    -FilesChanged "| File | Changes |`n|------|---------|`n| LayoutExtensions.cs | +17/-3 |" `
    -Status "Compiles" `
    -CodeSnippet "else if (!hasExplicitWidth) { ... }" `
    -Analysis "Core project compiles successfully"
```

### Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `TryFixDir` | No* | Path to try-fix attempt directory (auto-loads all parameters) |
| `IssueNumber` | No* | Issue or PR number to post comment on |
| `AttemptNumber` | No* | Attempt number (1, 2, 3, etc.) - auto-detected from TryFixDir |
| `Approach` | No* | Brief description of fix approach |
| `RootCause` | No | Description of root cause identified |
| `FilesChanged` | No* | Markdown table of files changed - auto-generated from diff |
| `Status` | No* | "Compiles", "Pass", or "Fail" - loaded from result.txt |
| `CodeSnippet` | No | Code snippet showing the fix - loaded from fix.diff |
| `Analysis` | No | Analysis of why it worked/failed - loaded from analysis.md |
| `DryRun` | No | Print comment instead of posting |

*When using `-TryFixDir`, all marked parameters are auto-loaded from files in the directory.

### Expected Directory Structure

```
CustomAgentLogsTmp/PRState/{IssueNumber}/try-fix/
├── attempt-1/
│   ├── approach.md      # Brief description of the approach (required)
│   ├── result.txt       # "Pass", "Fail", or "Compiles" (required)
│   ├── fix.diff         # Git diff of the fix (optional)
│   └── analysis.md      # Detailed analysis (optional)
├── attempt-2/
│   └── ...
└── attempt-3/
    └── ...
```

### Comment Format

```markdown
## 🔧 Try-Fix Attempts for Issue #XXXXX

<!-- TRY-FIX-COMMENT -->

<details>
<summary>📊 <strong>Expand Full Details</strong></summary>

**Issue:** [#XXXXX](link)

---

<details>
<summary><strong>🔧 Attempt #1: Approach Name</strong> ✅ Status</summary>
... attempt details ...
</details>

---

*This fix was developed independently.*

</details>
```

### Key Behaviors

- First attempt creates new comment with `<!-- TRY-FIX-COMMENT -->` marker
- Subsequent attempts **edit the same comment** (no new comments)
- Outer wrapper shows "📊 Expand Full Details" - keeps PR page clean
- Each attempt is a nested collapsible section inside the wrapper

---

## Verify-Tests Comment Script

The `post-verify-tests-comment.ps1` script updates the `<!-- SECTION:VERIFY-TESTS -->` section of the unified AI Summary comment. It documents test verification results (whether tests fail without fix and pass with fix).

**✨ Auto-Loading from `CustomAgentLogsTmp`**: The script automatically loads verification results from `CustomAgentLogsTmp/PRState/{PRNumber}/verify-tests-fail/verification-report.md`.

### Usage

#### Simplest: Provide PR number

```powershell
# Auto-loads from CustomAgentLogsTmp/PRState/{PRNumber}/verify-tests-fail/
pwsh .github/skills/pr-comment/scripts/post-verify-tests-comment.ps1 -PRNumber 32891
```

#### With explicit report file

```powershell
pwsh .github/skills/pr-comment/scripts/post-verify-tests-comment.ps1 `
    -PRNumber 32891 `
    -ReportFile CustomAgentLogsTmp/PRState/32891/verify-tests-fail/verification-report.md
```

### Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `PRNumber` | Yes | Pull request number |
| `ReportFile` | No | Path to verification report (auto-discovered if not provided) |
| `Status` | No | "Passed" or "Failed" - auto-detected from report |
| `Platform` | No | Platform tested (ios, android, etc.) - auto-detected from report |
| `Mode` | No | "FailureOnly" or "FullVerification" - auto-detected from report |
| `DryRun` | No | Preview changes in local file instead of posting |
| `PreviewFile` | No | Path to local preview file for DryRun mode |

### Expected Directory Structure

```
CustomAgentLogsTmp/PRState/{PRNumber}/verify-tests-fail/
├── verification-report.md   # Full verification report (required)
├── verification-log.txt     # Detailed log (optional)
├── test-without-fix.log     # Test output without fix (optional)
└── test-with-fix.log        # Test output with fix (optional)
```

---

## Write-Tests Comment Script

The `post-write-tests-comment.ps1` script updates the `<!-- SECTION:WRITE-TESTS -->` section of the unified AI Summary comment. It documents test writing attempts for an issue.

**✨ Auto-Loading from `CustomAgentLogsTmp`**: The script can automatically load test details from the write-tests output directory structure.

### Usage

#### Simplest: Provide test directory

```powershell
# All parameters auto-loaded from directory structure
pwsh .github/skills/pr-comment/scripts/post-write-tests-comment.ps1 `
    -TestDir CustomAgentLogsTmp/PRState/27246/write-tests/attempt-1
```

#### Or just provide issue number

```powershell
# Auto-discovers and posts latest attempt from CustomAgentLogsTmp/PRState/27246/write-tests/
pwsh .github/skills/pr-comment/scripts/post-write-tests-comment.ps1 -IssueNumber 27246
```

#### Legacy: Manual parameters

```powershell
pwsh .github/skills/pr-comment/scripts/post-write-tests-comment.ps1 `
    -IssueNumber 33331 `
    -AttemptNumber 1 `
    -TestDescription "Verifies Picker.IsOpen property changes correctly" `
    -HostAppFile "src/Controls/tests/TestCases.HostApp/Issues/Issue33331.cs" `
    -TestFile "src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue33331.cs" `
    -TestMethod "PickerIsOpenPropertyChanges" `
    -Category "Picker" `
    -VerificationStatus "Verified"
```

### Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `TestDir` | No* | Path to write-tests attempt directory (auto-loads all parameters) |
| `IssueNumber` | No* | Issue or PR number to post comment on |
| `AttemptNumber` | No* | Attempt number (1, 2, 3, etc.) - auto-detected from TestDir |
| `TestDescription` | No* | Brief description of what the test verifies |
| `HostAppFile` | No* | Path to the HostApp test page file |
| `TestFile` | No* | Path to the NUnit test file |
| `TestMethod` | No* | Name of the test method |
| `Category` | No* | UITestCategories category used |
| `VerificationStatus` | No* | "Verified", "Failed", or "Unverified" - loaded from result.txt |
| `Platforms` | No | Platforms the test runs on (default: "All") |
| `Notes` | No | Additional notes - loaded from notes.md |
| `DryRun` | No | Print comment instead of posting |

*When using `-TestDir`, all marked parameters are auto-loaded from files in the directory.

### Expected Directory Structure

```
CustomAgentLogsTmp/PRState/{IssueNumber}/write-tests/
├── attempt-1/
│   ├── description.md   # Brief test description (required)
│   ├── test-info.json   # {HostAppFile, TestFile, TestMethod, Category} (required)
│   ├── result.txt       # "Verified", "Pass", "Failed", or "Unverified" (required)
│   └── notes.md         # Additional notes (optional)
├── attempt-2/
│   └── ...
└── attempt-3/
    └── ...
```

### test-info.json Format

```json
{
    "HostAppFile": "src/Controls/tests/TestCases.HostApp/Issues/Issue27246.cs",
    "TestFile": "src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue27246.cs",
    "TestMethod": "ScrollToFirstItemWithHeader",
    "Category": "CollectionView"
}
```

---

## Technical Details

- Comments identified by HTML marker `<!-- AI Summary -->`
- Existing comments are updated (not duplicated) when posting again
- Review sessions grouped by commit SHA
- Uses `gh api` for create/update operations
