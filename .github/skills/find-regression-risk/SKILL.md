---
name: find-regression-risk
description: "Detects if a PR modifies or reverts code from a recent bug-fix PR, which could re-introduce a previously-fixed bug. Use when reviewing any PR, before approving, or when asked 'check for regression risks', 'could this revert a fix', 'regression cross-reference'."
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires git, PowerShell, and gh CLI.
---

# Find Regression Risk

Detects if a PR's changes could re-introduce previously-fixed bugs by cross-referencing the diff against recently-merged bug-fix PRs.

## Why This Exists

PRs that fix one bug can silently re-introduce another if they modify or revert lines from a prior fix.

**Real-world example:** PR #33908 removed `|| parent is IMauiRecyclerView` from `MauiWindowInsetListener.cs` — a line that PR #32278 had specifically added to fix issue #32436 (increasing gap at bottom while scrolling). The agent approved PR #33908 without detecting this revert, and the exact same bug reappeared as issue #34634.

## When to Use

- ✅ Before approving any PR that modifies platform code
- ✅ When asked "check for regression risks" or "could this revert a fix"
- ✅ As part of the `pr-review` skill's Pre-Flight phase
- ✅ When a PR touches high-churn files (safe area, CollectionView handlers, inset listeners)

## Workflow

### Step 1: Run the Script

```bash
# Auto-detect implementation files from PR diff
pwsh .github/skills/find-regression-risk/scripts/Find-RegressionRisks.ps1 -PRNumber XXXXX

# Or specify files manually
pwsh .github/skills/find-regression-risk/scripts/Find-RegressionRisks.ps1 \
  -PRNumber XXXXX \
  -FilePaths @("src/Core/src/Platform/iOS/MauiView.cs","src/Core/src/Platform/Android/MauiWindowInsetListener.cs")
```

### Step 2: Interpret Results

| Risk Level | Meaning | Action |
|------------|---------|--------|
| 🔴 **REVERT DETECTED** | PR removes/modifies lines from a recent bug-fix PR | **MUST** verify the previously-fixed bug doesn't reappear before approving |
| 🟡 **OVERLAP** | PR modifies the same file as a recent bug-fix but different lines | Note as lower-risk concern |
| 🟢 **CLEAN** | No recent bug-fix PRs touched these files | No action needed |

### Step 3: Act on Findings

**If 🔴 REVERT DETECTED:**

1. Identify the at-risk issues — the script reports which issues were fixed by the reverted code
2. Verify the fix still holds — run the test from the original fix PR, or manually verify
3. Document in PR review — note the revert risk and the verification result
4. **Do NOT approve** until the risk is mitigated

## What the Script Does

For each implementation file in the PR diff:

1. **Finds recent PRs** — queries `git log` for the last 6 months of changes to that file
2. **Identifies bug fixes** — checks PR labels AND linked issue labels for `i/regression`, `t/bug`, `p/0`, `p/1`
3. **Compares diffs** — checks if lines ADDED by a fix PR are being REMOVED by the current PR
4. **Reports risks** — structured output with file, PR, issue, and reverted line details

## Output Format

Post results using the following format:

```markdown
## 🔍 Regression Risk Analysis

**PR:** #XXXXX
**Risk Level:** [🟢 Clean | 🟡 Overlaps Found | 🔴 Revert Risks Detected]

[Summary sentence]

> 👍 / 👎 — Was this analysis helpful? React to let us know!

<details>
<summary>📊 Expand Full Analysis</summary>

### Risks Found

| File | Recent Fix PR | Fixed Issue | Risk | Details |
|------|--------------|-------------|------|---------|
| ... | ... | ... | ... | ... |

### Recommendations

- ...

</details>
```
