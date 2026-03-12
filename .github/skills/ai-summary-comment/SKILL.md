---
name: ai-summary-comment
description: Posts or updates automated progress comments on GitHub PRs. Use after completing any agent phase (pre-flight, gate, try-fix, report). Triggers on 'post comment to PR', 'update PR progress', 'comment on PR with results', 'post pre-flight comment'. Creates single aggregated review comment with collapsible sections per commit.
metadata:
  author: dotnet-maui
  version: "5.0"
compatibility: Requires GitHub CLI (gh) authenticated with access to dotnet/maui repository.
---

# PR Comment Skill

This skill posts automated progress comments to GitHub Pull Requests during the PR review workflow. Comments are **self-contained** with collapsible Review Session details, providing rich context to maintainers and contributors.

**⚠️ Self-Contained Rule**: All content in PR comments must be self-contained. Never reference local files like `CustomAgentLogsTmp/` - GitHub users cannot access your local filesystem.

**✨ Key Features**:
- **Single Unified Comment**: ONE comment per PR/Issue containing ALL sections (PR Review, PR Finalize)
- **Section-Based Updates**: Each script updates only its section, preserving others
- **Duplicate Prevention**: Finds existing `<!-- AI Summary -->` comment and updates it
- **File-Based DryRun Preview**: Use `-DryRun` to preview changes in a local file before posting
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

<!-- SECTION:PR-FINALIZE -->
... PR finalization results ...
<!-- /SECTION:PR-FINALIZE -->
```

**Behavior:**
- First script to run creates the comment
- Subsequent scripts find the existing comment and update/add their section
- Sections are independent - updating one preserves others

### PR Finalization Section

The `post-pr-finalize-comment.ps1` script injects a `<!-- SECTION:PR-FINALIZE -->` section into the **unified AI Summary comment** by default. This section contains three collapsible parts:
- **Title**: Shows the current vs recommended PR title
- **Description**: Shows description assessment, missing elements, and **recommended description**
- **Code Review**: Shows code review findings (critical issues, suggestions, positive observations)

Use `-Standalone` to post as a separate comment instead (legacy behavior).

**⚠️ Important Requirements for PR Finalize Comments:**
- When `TitleStatus` is `NeedsUpdate`, **always provide** `-RecommendedTitle`
- When `DescriptionStatus` is `NeedsUpdate` or `NeedsRewrite`, **always provide** `-RecommendedDescription` with the full suggested description text
- The script will warn if these are missing but won't fail

## Section Scripts

### AI Summary Sections (Unified Comment)

| Section | Script | Location |
|---------|--------|----------|
| `PR-REVIEW` | `post-ai-summary-comment.ps1` | `.github/skills/ai-summary-comment/scripts/` |
| `PR-FINALIZE` | `post-pr-finalize-comment.ps1` | `.github/skills/ai-summary-comment/scripts/` |

## Supported Phases

| Phase | Description | When to Post | What This Enables Next |
|-------|-------------|--------------|------------------------|
| `pre-flight` | Context gathering complete | After documenting issue, files, and discussion | **Tests Phase**: Agent can now verify/create test files that reproduce the bug |
| `tests` | Test analysis complete | After identifying test files and coverage | **Validate Phase**: Agent can run tests to verify they catch the bug |
| `validate` | Test validation complete | After running tests and verifying bug reproduction | **Fix Phase**: Agent can explore alternative fixes (tests proven to catch bug) |
| `fix` | Solution comparison complete | After comparing PR fix with alternatives | **Report Phase**: Agent can finalize recommendation based on fix comparison |
| `report` | Final analysis complete | After generating comprehensive review | **PR Decision**: Maintainers can approve/merge or request changes based on full analysis |

## Usage

### Simplest: Just provide PR number (auto-loads from phase files)

```bash
# Auto-loads from CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/*/content.md
pwsh .github/skills/ai-summary-comment/scripts/post-ai-summary-comment.ps1 -PRNumber 27246
```

### Parameters

| Parameter | Required | Description | Example |
|-----------|----------|-------------|---------|
| `PRNumber` | Yes | Pull request number | `12345` |
| `DryRun` | No | Preview changes in local file instead of posting to GitHub | `-DryRun` |
| `PreviewFile` | No | Path to local preview file for DryRun mode (default: `CustomAgentLogsTmp/PRState/{PRNumber}/ai-summary-comment-preview.md`) | `-PreviewFile ./preview.md` |
| `SkipValidation` | No | Skip validation checks (not recommended) | `-SkipValidation` |

## DryRun Preview Workflow

Use `-DryRun` to preview the combined comment before posting to GitHub. Each script updates the same preview file, mirroring how the actual GitHub comment is updated.

```bash
# Step 1: Preview AI summary
pwsh .github/skills/ai-summary-comment/scripts/post-ai-summary-comment.ps1 -PRNumber 32891 -DryRun

# Step 2: Review the preview
open CustomAgentLogsTmp/PRState/32891/ai-summary-comment-preview.md

# Step 3: Post for real (remove -DryRun)
pwsh .github/skills/ai-summary-comment/scripts/post-ai-summary-comment.ps1 -PRNumber 32891
```

**Key behavior:** The preview file exactly matches what will be posted to GitHub. Multiple scripts accumulate their sections in the same file.

### Section Ordering

Sections appear in the unified comment in this order:
1. **PR-REVIEW** - PR review phases
2. **PR-FINALIZE** - PR finalization results

Each section is wrapped with markers like `<!-- SECTION:PR-REVIEW -->` and `<!-- /SECTION:PR-REVIEW -->`.

### Cleanup

To reset the preview file for a fresh start:
```bash
rm CustomAgentLogsTmp/PRState/{PRNumber}/ai-summary-comment-preview.md
```

### Prerequisites

Scripts require GitHub CLI authentication:
```bash
gh auth status  # Verify authentication before running
```

## Comment Format

Comments are formatted with:
- **Phase badge** (🔍 Pre-Flight, 🚦 Validate, 🔧 Fix, 📋 Report)
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
→ **Phase 3: Validate** - Verifying fix works

---
*Posted by Agent @ 2026-01-17 14:23:45 UTC*
```

## Script Files

- [`post-ai-summary-comment.ps1`](scripts/post-ai-summary-comment.ps1) - Posts or updates the aggregated agent review comment
- [`post-pr-finalize-comment.ps1`](scripts/post-pr-finalize-comment.ps1) - Posts or updates PR finalization section

---

## PR Finalize Comment Script

The `post-pr-finalize-comment.ps1` script injects a PR finalization section into the **unified AI Summary comment** by default. It provides structured feedback on the PR title, description, and code review findings. Use `-Standalone` to post as a separate comment instead.

### Usage

#### Simplest: Just provide PR number (auto-loads from summary file)

```powershell
# Auto-loads from CustomAgentLogsTmp/PRState/{PRNumber}/pr-finalize/pr-finalize-summary.md
pwsh .github/skills/ai-summary-comment/scripts/post-pr-finalize-comment.ps1 -PRNumber 33892
```

#### Full manual parameters (recommended for best results)

```powershell
pwsh .github/skills/ai-summary-comment/scripts/post-pr-finalize-comment.ps1 `
    -PRNumber 33892 `
    -TitleStatus "NeedsUpdate" `
    -CurrentTitle "Fix 32650 Image Orientation" `
    -RecommendedTitle "[iOS][Android] MediaPicker: Fix image orientation when RotateImage=true" `
    -TitleIssues "- Missing platform tags
- Doesn't describe the behavior fix" `
    -DescriptionStatus "NeedsUpdate" `
    -DescriptionAssessment "The current description is minimal and missing:
- ❌ Missing NOTE block for testing artifacts
- ❌ No root cause analysis
- ❌ No technical details" `
    -MissingElements "Add the NOTE block, root cause, and technical details." `
    -RecommendedDescription "> [!NOTE]
> Are you waiting for this PR? Test it: [Testing PR Builds](link)

### Root Cause
...description...

### Description of Change
...details..." `
    -CodeReviewStatus "IssuesFound" `
    -CodeReviewFindings "### 🔴 Critical Issues
**1. Broken indentation**
- File: \`src/file.cs\`
- Problem: Inconsistent tabs/spaces

### 🟡 Suggestions
1. Consider disposing Matrix object

### ✅ Looks Good
- Proper cleanup in finally block"
```

### Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `PRNumber` | Yes* | Pull request number |
| `SummaryFile` | No | Path to pr-finalize-summary.md (auto-discovered) |
| `TitleStatus` | No* | `Good` or `NeedsUpdate` |
| `CurrentTitle` | No* | Current PR title (fetched from GitHub if not provided) |
| `RecommendedTitle` | No | **Required if TitleStatus is NeedsUpdate** |
| `TitleIssues` | No | List of issues with current title |
| `DescriptionStatus` | No* | `Excellent`, `Good`, `NeedsUpdate`, or `NeedsRewrite` |
| `DescriptionAssessment` | Yes | Assessment of description quality |
| `MissingElements` | No | What's missing from the description |
| `RecommendedDescription` | No | **Required if DescriptionStatus is NeedsUpdate/NeedsRewrite** |
| `CodeReviewStatus` | No | `Passed`, `IssuesFound`, or `Skipped` |
| `CodeReviewFindings` | No | Markdown content for code review section |
| `DryRun` | No | Preview instead of posting |

*At least PRNumber or SummaryFile required. Script auto-detects values when possible.

### ⚠️ Common Mistakes to Avoid

1. **Missing RecommendedTitle when TitleStatus is NeedsUpdate**
   - The script will warn but still post - always provide a recommended title

2. **Missing RecommendedDescription when DescriptionStatus is NeedsUpdate**
   - Users need to see what the description SHOULD look like

3. **Code review findings not starting with proper headers**
   - Always structure with `### 🔴 Critical Issues`, `### 🟡 Suggestions`, `### ✅ Looks Good`

4. **Auto-parsing from summary file getting confused**
   - When in doubt, provide explicit parameters instead of relying on auto-parsing

### Expected Directory Structure

The agent writes phase output files that comment scripts auto-load:

```
CustomAgentLogsTmp/PRState/{PRNumber}/
├── PRAgent/
│   ├── pre-flight/
│   │   └── content.md              # Phase 1 output (auto-loaded by post-ai-summary-comment.ps1)
│   ├── validate/
│   │   ├── content.md              # Phase 2 output (auto-loaded by post-ai-summary-comment.ps1)
│   │   └── verify-tests-fail/     # Script output from verify-tests-fail.ps1
│   │       ├── verification-report.md
│   │       ├── verification-log.txt
│   │       ├── test-without-fix.log
│   │       └── test-with-fix.log
│   ├── try-fix/
│   │   ├── content.md              # Phase 3 summary (auto-loaded by post-ai-summary-comment.ps1)
│   │   ├── attempt-1/             # Individual attempt outputs
│   │   │   ├── approach.md
│   │   │   ├── result.txt
│   │   │   ├── fix.diff
│   │   │   └── analysis.md
│   │   └── attempt-2/
│   │       └── ...
│   └── report/
│       └── content.md              # Phase 4 output (auto-loaded by post-ai-summary-comment.ps1)
├── pr-finalize/
│   ├── pr-finalize-summary.md
│   ├── recommended-description.md
│   └── code-review.md
└── copilot-logs/
    ├── process-*.log
    └── session-*.md
```

### Auto-Loading Behavior

When `post-ai-summary-comment.ps1` is called, it auto-discovers phase files:
1. Checks `CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/*/content.md`
2. Loads all available phase content files
3. Builds the comment structure from the loaded files
4. Posts/updates the unified AI Summary comment

This eliminates the need to pass large content strings as parameters.

---

## Technical Details

- Comments identified by HTML marker `<!-- AI Summary -->`
- Existing comments are updated (not duplicated) when posting again
- Review sessions grouped by commit SHA
- Uses `gh api` for create/update operations
