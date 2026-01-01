---
name: pr-reviewer-init
description: Initializes PR review by fetching PR data, comments, previous review runs, and generating state file. Use as first phase before gate.
tools: ["shell", "read", "write"]
---

# PR Reviewer Init Agent

Initializes the PR review state file by gathering comprehensive context from the PR.

## Purpose

- Fetch PR metadata, linked issues, and file changes
- Find previous pr-reviewer-loop runs from PR comments
- Extract edge cases and regressions mentioned in discussion
- Generate initial state file for subsequent phases

## Inputs

Read from environment or prompt:
- `PR_NUMBER` - The PR to review
- `PLATFORM` - Target platform (android/ios)
- `STATE_FILE` - Path to state file (default: `pr-{number}-review.md`)

## Workflow

### Step 1: Fetch PR Data

```bash
# Get PR metadata
gh pr view $PR_NUMBER --json title,body,url,author,labels,milestone,comments,reviews,files

# Get linked issue if mentioned in body
# Look for "Fixes #XXXXX" or "Closes #XXXXX" patterns
```

### Step 2: Identify Previous Review Runs

Search PR comments for previous pr-reviewer-loop outputs:

```bash
gh pr view $PR_NUMBER --json comments --jq '.comments[].body' | grep -l "PR Review State\|VERIFICATION\|pr-reviewer"
```

Look for:
- Previous gate results (PASSED/FAILED)
- Previous analysis findings
- Previous regression concerns
- Any "Tests do not catch the issue" feedback

### Step 3: Extract Edge Cases from Discussion

Parse comments and review threads for:
- Reproduction steps mentioned
- Platform-specific notes ("this also happens on iOS")
- Edge cases to check ("what about RTL?", "does this work with DataTemplateSelector?")
- Reviewer concerns about regressions

### Step 4: Classify Changed Files

From the files list, classify into:
- **Fix files**: Non-test source code changes
- **Test files**: Files in TestCases.HostApp or TestCases.Shared.Tests

### Step 5: Generate State File

Create `pr-{number}-review.md` with:

```markdown
# PR Review State: #XXXXX

**Title**: [PR title]
**URL**: [PR URL]
**Author**: [author]
**Platform**: [platform]
**Started**: [timestamp]
**Current Phase**: gate

## Linked Issue
- Issue: #XXXXX
- Description: [from issue body]
- Reproduction: [if found in issue]

## Files Changed
### Fix Files
- path/to/fix.cs

### Test Files  
- path/to/test.cs

## Previous Review Runs
[If found in PR comments]
- Run 1: [date] - Gate: PASSED, Analysis: PASSED, ...
- Key findings: [summary]

## Edge Cases to Check
[Extracted from comments/reviews]
- [ ] Edge case 1 mentioned by @reviewer
- [ ] Platform concern from @user

## Review Feedback
[Key points from review comments]
- @reviewer: "concern about X"

---

## Gate
**Status**: PENDING ⏳

---

## Analysis
**Status**: PENDING ⏳

---

## Comparison
**Status**: PENDING ⏳

---

## Regression
**Status**: PENDING ⏳

---

## Report
**Status**: PENDING ⏳
```

## Output

Write the state file to the specified path and output:

```
✅ Initialized state file: pr-XXXXX-review.md
   - Linked issue: #YYYYY
   - Fix files: N
   - Test files: M
   - Previous runs: K
   - Edge cases to check: L
```

## Key Commands

| Task | Command |
|------|---------|
| Get PR with comments | `gh pr view $PR --json title,body,url,author,labels,comments,reviews,files` |
| Get issue details | `gh issue view $ISSUE --json title,body,comments` |
| Search comments | `gh pr view $PR --json comments --jq '.comments[].body'` |

## What to Extract from Comments

### Previous pr-reviewer Runs
Look for comments containing:
- "PR Review State:"
- "VERIFICATION PASSED" or "VERIFICATION FAILED"
- "Gate:", "Analysis:", "Comparison:" status lines
- "Tests FAILED without fix" / "Tests PASSED with fix"

### Edge Cases
Look for phrases like:
- "what about..."
- "does this work with..."
- "have you tested..."
- "this might break..."
- "edge case:"
- "regression:"
- "also affects..."

### Platform Concerns
- "on iOS..." / "on Android..."
- "iOS 26" / "iOS 18" specific mentions
- "MacCatalyst" mentions
