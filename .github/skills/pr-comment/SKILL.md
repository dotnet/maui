---
name: pr-comment
description: Posts progress comments to GitHub PRs during review phases. Self-contained comments with collapsible details. Use when a PR agent phase completes.
metadata:
  author: dotnet-maui
  version: "3.0"
compatibility: Requires GitHub CLI (gh) authenticated with access to dotnet/maui repository.
---

# PR Comment Skill

This skill posts automated progress comments to GitHub Pull Requests during the PR review workflow. Comments are **self-contained** with collapsible Review Session details, providing rich context to maintainers and contributors.

**✨ Key Features**:
- **Single Aggregated Comment**: One comment for the entire review with all phases
- **Duplicate Prevention**: Checks if review comment already exists before posting  
- **Self-Contained**: All content stored in comments - no external dependencies
- **Review Session Support**: Tracks multiple review sessions with expandable details and commit links
- **Simple Interface**: Just pass content - script handles everything else

## When to Use

- After completing Pre-Flight phase (context gathering)
- After completing Tests phase (test verification)
- After completing Gate phase (test validation)
- After completing Fix phase (solution comparison)
- After completing Report phase (final analysis)

**Trigger phrases:**
- "Post Pre-Flight comment to PR #XXXXX"
- "Comment on PR #XXXXX with Pre-Flight results"
- "Update PR #XXXXX with progress"

**🚨 CRITICAL**: Always post phase comments after completing each phase. The PR agent workflow mandates this.

## Supported Phases

| Phase | Description | When to Post | What This Enables Next |
|-------|-------------|--------------|------------------------|
| `pre-flight` | Context gathering complete | After documenting issue, files, and discussion | **Tests Phase**: Agent can now verify/create test files that reproduce the bug |
| `tests` | Test analysis complete | After identifying test files and coverage | **Gate Phase**: Agent can run tests to verify they catch the bug |
| `gate` | Test validation complete | After running tests and verifying bug reproduction | **Fix Phase**: Agent can explore alternative fixes (tests proven to catch bug) |
| `fix` | Solution comparison complete | After comparing PR fix with alternatives | **Report Phase**: Agent can finalize recommendation based on fix comparison |
| `report` | Final analysis complete | After generating comprehensive review | **PR Decision**: Maintainers can approve/merge or request changes based on full analysis |

## Usage

### Understanding the Phase Progression

Each phase completion unlocks the next phase in the workflow:

```
🔍 Pre-Flight
   ↓
   📤 Post comment → Context documented, test requirements identified
   ↓
🧪 Tests
   ↓
   📤 Post comment → Tests exist and reproduce the bug
   ↓
🚦 Gate
   ↓
   📤 Post comment → Tests verified to catch the fix
   ↓
🔧 Fix
   ↓
   📤 Post comment → Alternative fixes explored and compared
   ↓
📋 Report
   ↓
   📤 Post comment → Final recommendation ready
   ↓
✅ PR Decision (Approve/Merge or Request Changes)
```

**Why post after each phase?**
- **Transparency**: Maintainers see progress in real-time
- **Accountability**: Each phase result is documented
- **Collaboration**: Contributors can provide input at any stage
- **History**: Multiple review sessions are tracked on the same PR
- **Async workflow**: Reviewers can pick up where previous session left off

### Post a Phase Completion Comment

```bash
# Pipe content via stdin or use -Content parameter
cat .github/agent-pr-session/pr-12345.md | \
  pwsh .github/skills/pr-comment/scripts/post-pr-comment.ps1 -PRNumber 12345
```

### Parameters

| Parameter | Required | Description | Example |
|-----------|----------|-------------|---------|
| `PRNumber` | Yes | Pull request number | `12345` |
| `Content` | No | Full state file content (can be piped via stdin) | Content from state file |
| `DryRun` | No | Print comment instead of posting | `-DryRun` |
| `SkipValidation` | No | Skip validation checks (not recommended) | `-SkipValidation` |

## Comment Format

Comments are formatted with:
- **Phase badge** (🔍 Pre-Flight, 🧪 Tests, 🚦 Gate, 🔧 Fix, 📋 Report)
- **Status indicator** (✅ Completed, ⚠️ Issues Found)
- **Expandable review sessions** (each session is a collapsible section)
- **What's Next** (what phase happens next)

### Review Session Tracking

When the same PR is reviewed multiple times (e.g., after new commits), the script **updates the single aggregated review comment** and adds a new expandable section for each commit-based review session. This keeps PR comments organized and prevents duplication.

**Example Comment Structure:**

```markdown
## 🤖 PR Agent Review — ✅ APPROVE

<details>
<summary>📊 Expand Full Review</summary>

### Review Sessions

<details>
<summary>📝 Session: Fix CollectionView null reference - abc123d</summary>

#### 🔍 Pre-Flight: Context Gathering
✅ Analyzed issue #33356...

#### 🧪 Tests: Verification
✅ Found existing test coverage...

</details>

<details>
<summary>📝 Session: Update after feedback - def456e</summary>

#### 🔍 Pre-Flight: Context Gathering
✅ Re-analyzed with latest changes...

</details>

</details>
```

### Example Comment (Single Session)

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

## Workflow Integration

The PR agent should call this skill after completing each phase:

```
Phase 1: Pre-Flight
  ├─ Gather context
  ├─ Update state file
  └─ 📤 POST COMMENT (pr-comment skill)

Phase 2: Tests  
  ├─ Analyze tests
  ├─ Update state file
  └─ 📤 POST COMMENT (pr-comment skill)

[... and so on for remaining phases]
```

### Technical Details

- A single aggregated PR review comment is identified by the HTML comment marker `<!-- PR-AGENT-REVIEW -->`
- The script checks for an existing comment containing the review marker before posting
- If an existing comment is found, the single aggregated review comment is **updated** to add a new commit-based review session
- Review sessions are grouped by commit and labeled using the commit title and short SHA
- Comments use collapsible `<details>` sections for each commit-based review session
- Updates preserve all previous review sessions
- Uses GitHub's markdown rendering for formatted output
- API calls use `gh api` for editing existing comments
- Comments are posted using GitHub CLI (`gh pr comment`)
- State file is parsed to extract all phase-specific information
- Comments are idempotent - posting again updates the aggregated comment
