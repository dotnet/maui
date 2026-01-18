---
name: pr-comment
description: Posts progress comments to GitHub PRs during review phases. Mirrors state file structure and prevents duplicates. Use when a PR agent phase completes.
metadata:
  author: dotnet-maui
  version: "2.0"
compatibility: Requires GitHub CLI (gh) authenticated with access to dotnet/maui repository.
---

# PR Comment Skill

This skill posts automated progress comments to GitHub Pull Requests during the PR review workflow. Comments **mirror the state file structure** with collapsible details sections, providing rich context to maintainers and contributors.

**✨ Key Features**:
- **Auto-Ordering**: Automatically ensures all previous phase comments exist
- **Duplicate Prevention**: Checks if phase comment already exists before posting  
- **State File Independence**: Comments are self-contained - state file only needed at POST time, not READ time
- **Review Session Support**: Tracks multiple review sessions with expandable details and commit links
- **Direct Content Mode**: Can post content without state file (via `-Content` parameter)

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

### Post a Phase Completion Comment (Using State File)

```bash
pwsh .github/skills/pr-comment/scripts/post-pr-comment.ps1 \
  -PRNumber 12345 \
  -Phase pre-flight \
  -StateFile .github/agent-pr-session/pr-12345.md
```

### Post Custom Content (Without State File)

```bash
pwsh .github/skills/pr-comment/scripts/post-pr-comment.ps1 \
  -PRNumber 12345 \
  -Phase fix \
  -Content "Alternative fix tested and validated..."
```

### Parameters

| Parameter | Required | Description | Example |
|-----------|----------|-------------|---------|
| `PRNumber` | Yes | Pull request number | `12345` |
| `Phase` | Yes | Phase name: `pre-flight`, `tests`, `gate`, `fix`, or `report` | `pre-flight` |
| `StateFile` | No* | Path to PR session state file | `.github/agent-pr-session/pr-12345.md` |
| `Content` | No* | Direct content to post (alternative to StateFile) | `"Custom content..."` |
| `DryRun` | No | Print comment instead of posting | |

**\* Either `StateFile` or `Content` must be provided**
| `PRNumber` | ✅ | Pull request number | `12345` |
| `Phase` | ✅ | Phase name | `pre-flight`, `tests`, `gate`, `fix`, `report` |
| `StateFile` | ✅ | Path to PR session state file | `.github/agent-pr-session/pr-12345.md` |
| `AdditionalNotes` | ❌ | Extra information to include | `Found 3 test files` |

## Comment Format

Comments are formatted with:
- **Phase badge** (🔍 Pre-Flight, 🧪 Tests, 🚦 Gate, 🔧 Fix, 📋 Report)
- **Status indicator** (✅ Completed, ⚠️ Issues Found)
- **Expandable review sessions** (each session is a collapsible section)
- **What's Next** (what phase happens next)

### Auto-Ordering Feature

**The script automatically enforces correct phase ordering:**

```
Example: You try to post Gate comment but Tests comment doesn't exist
Result: Script posts Tests comment first, then Gate comment

Example: You try to post Report but Pre-Flight, Tests, and Fix are missing
Result: Script posts Pre-Flight → Tests → Gate → Fix → Report in order
```

This ensures PR comments ALWAYS appear in chronological phase order, even if you accidentally skip posting a phase comment.

### Multiple Review Sessions

When the same PR is reviewed multiple times (e.g., after new commits), the script **edits the existing phase comment** and adds a new expandable section for each review session. This keeps PR comments organized and prevents duplication.


✅ **Status**: Phase completed successfully

<details>
<summary><strong>📝 Review Session #1</strong> - 2026-01-17 14:23:45 UTC</summary>

### Summary
- **Issue**: #33356 - CollectionView crash on iOS
- **Platforms Affected**: iOS, MacCatalyst
- **Files Changed**: 2 files

### What Was Done
✓ Analyzed issue description and reproduction steps  
✓ Reviewed PR discussion and reviewer feedback  
✓ Documented files changed and code modifications  
✓ Identified platforms affected and scope of changes  

</details>
</details>

### Next Steps
→ **Phase 2: Tests** - Analyzing test files and coverage
```

**Second Review (comment is updated):**
```markdown
## 🔍 Pre-Flight: Context Gathering

✅ **Status**: Phase completed successfully

<details>
<summary><strong>📝 Review Session #1</strong> - 2026-01-17 14:23:45 UTC</summary>
...
</details>

<details>
<summary><strong>📝 Review Session #2</strong> - 2026-01-18 09:15:30 UTC</summary>

### Summary
- **Issue**: #33356 - CollectionView crash on iOS
- **Platforms Affected**: iOS, MacCatalyst
...
</details>

### Next Steps
→ **Phase 2: Tests** - Analyzing test files and coverage
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

- [`post-pr-comment.ps1`](scripts/post-pr-comment.ps1) - Posts phase completion comment
- [`format-comment.ps1`](scripts/format-comment.ps1) - Formats comment markdown from state file

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
``Each phase has a unique HTML comment marker (e.g., `<!-- PR-AGENT-PHASE: pre-flight -->`)
- The script checks for existing comments with the phase marker before posting
- If an existing comment is found, it's **edited** (not replaced) to add a new review session
- Review sessions are numbered sequentially (#1, #2, #3, etc.)
- Comments use collapsible `<details>` sections for each review session
- Updates preserve all previous review sessions
- Uses GitHub's markdown rendering for formatted output
- API calls use `gh api` for editing existing comments
- Comments are posted using GitHub CLI (`gh pr comment`)
- State file is parsed to extract phase-specific information
- Each phase has a unique comment format template
- Comments are idempotent - posting again updates the previous comment
- Uses GitHub's markdown rendering for formatted output
