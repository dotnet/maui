---
name: pr-comment
description: Posts or updates automated progress comments on GitHub PRs. Use after completing any PR agent phase (pre-flight, tests, gate, fix, report). Triggers on 'post comment to PR', 'update PR progress', 'comment on PR with results', 'post pre-flight comment'. Creates single aggregated review comment with collapsible sections per commit.
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

## Supported Phases

| Phase | Description | When to Post | What This Enables Next |
|-------|-------------|--------------|------------------------|
| `pre-flight` | Context gathering complete | After documenting issue, files, and discussion | **Tests Phase**: Agent can now verify/create test files that reproduce the bug |
| `tests` | Test analysis complete | After identifying test files and coverage | **Gate Phase**: Agent can run tests to verify they catch the bug |
| `gate` | Test validation complete | After running tests and verifying bug reproduction | **Fix Phase**: Agent can explore alternative fixes (tests proven to catch bug) |
| `fix` | Solution comparison complete | After comparing PR fix with alternatives | **Report Phase**: Agent can finalize recommendation based on fix comparison |
| `report` | Final analysis complete | After generating comprehensive review | **PR Decision**: Maintainers can approve/merge or request changes based on full analysis |

## Usage

```bash
# Pipe content via stdin or use -Content parameter
cat CustomAgentLogsTmp/PRState/pr-12345.md | \
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

## Technical Details

- Comments identified by HTML marker `<!-- PR-AGENT-REVIEW -->`
- Existing comments are updated (not duplicated) when posting again
- Review sessions grouped by commit SHA
- Uses `gh api` for create/update operations
