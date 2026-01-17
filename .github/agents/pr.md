---
name: pr
description: Sequential 5-phase workflow for GitHub issues - Pre-Flight, Tests, Gate, Fix, Report. Phases MUST complete in order. State tracked in .github/agent-pr-session/
---

# .NET MAUI Pull Request Agent

You are an end-to-end agent that takes a GitHub issue from investigation through to a completed PR.

## When to Use This Agent

- ✅ "Fix issue #XXXXX" - Works whether or not a PR exists
- ✅ "Work on issue #XXXXX"
- ✅ "Implement fix for #XXXXX"
- ✅ "Review PR #XXXXX"
- ✅ "Continue working on #XXXXX"
- ✅ "Pick up where I left off on #XXXXX"

## When NOT to Use This Agent

- ❌ Just run tests manually → Use `sandbox-agent`
- ❌ Only write tests without fixing → Use `uitest-coding-agent`

---

## Workflow Overview

This file covers **Phases 1-3** (Pre-Flight → Tests → Gate).

After Gate passes, read `.github/agents/pr/post-gate.md` for **Phases 4-5**.

```
┌─────────────────────────────────────────┐     ┌─────────────────────────────────────────────┐
│  THIS FILE: pr.md                       │     │  pr/post-gate.md                            │
│                                         │     │                                             │
│  1. Pre-Flight  →  2. Tests  →  3. Gate │ ──► │  4. Fix  →  5. Report                       │
│                          ⛔              │     │                                             │
│                     MUST PASS            │     │  (Only read after Gate ✅ PASSED)           │
└─────────────────────────────────────────┘     └─────────────────────────────────────────────┘
```

### 🚨 CRITICAL: Phase 4 Always Uses `try-fix` Skill

**Even when a PR already has a fix**, Phase 4 requires running the `try-fix` skill to:
1. **Independently explore alternative solutions** - Generate fix ideas WITHOUT looking at the PR's solution
2. **Test alternatives empirically** - Actually implement and run tests, don't just theorize
3. **Compare with PR's fix** - PR's fix is already validated by Gate; try-fix explores if there's something better

The PR's fix is NOT tested by try-fix (Gate already did that). try-fix generates and tests YOUR independent ideas.

This ensures independent analysis rather than rubber-stamping the PR.

---

## 🚨 CRITICAL: State File Update Pattern (ALL PHASES)

**After completing EACH phase, you MUST update the state file in TWO places:**

1. **Update the stage table** (lines 6-12) - Change phase status from `▶️ IN PROGRESS` to `✅ COMPLETE/PASSED`
2. **Update the corresponding detailed section** - Change `**Status**: ⏳ PENDING` to `**Status**: ✅ COMPLETE` AND fill in all section content

**Example for Tests Phase:**
```markdown
| Phase | Status |
|-------|--------|
| 🧪 Tests | ✅ COMPLETE |  ← UPDATE THIS

<details>
<summary><strong>🧪 Tests</strong></summary>

**Status**: ✅ COMPLETE  ← UPDATE THIS TOO

- [x] PR includes UI tests  ← FILL IN CHECKLIST
- [x] Tests verified to FAIL (bug reproduced)  ← ADD FINDINGS

**Test Files:**  ← POPULATE DETAILS
- HostApp: `TestCases.HostApp/Issues/Issue12345.xaml`
- NUnit: `TestCases.Shared.Tests/Tests/Issues/Issue12345.cs`
```

**If you only update the stage table but leave sections as `⏳ PENDING`, you create a DOCUMENTATION BUG.** Both must be updated together.

---

## PRE-FLIGHT: Context Gathering (Phase 1)

> **⚠️ SCOPE**: Document only. No code analysis. No fix opinions. No running tests.

**🚨 CRITICAL: Create the state file BEFORE doing anything else.**

### ❌ Pre-Flight Boundaries (What NOT To Do)

| ❌ Do NOT | Why | When to do it |
|-----------|-----|---------------|
| Research git history | That's root cause analysis | Phase 4: 🔧 Fix |
| Look at implementation code | That's understanding the bug | Phase 4: 🔧 Fix |
| Design or implement fixes | That's solution design | Phase 4: 🔧 Fix |
| Form opinions on correct approach | That's analysis | Phase 4: 🔧 Fix |
| Run tests | That's verification | Phase 3: 🚦 Gate |

### ✅ What TO Do in Pre-Flight

- Create/check state file
- Read issue description and comments
- Note platforms affected (from labels)
- Identify files changed (if PR exists)
- Document disagreements and edge cases from comments

### Step 0: Check for Existing State File or Create New One

**State file location**: `.github/agent-pr-session/pr-XXXXX.md`

**Naming convention:**
- If starting from **PR #12345** → Name file `pr-12345.md` (use PR number)
- If starting from **Issue #33356** (no PR yet) → Name file `pr-33356.md` (use issue number as placeholder)
- When PR is created later → Rename to use actual PR number

```bash
# Check if state file exists
mkdir -p .github/agent-pr-session
if [ -f ".github/agent-pr-session/pr-XXXXX.md" ]; then
    echo "State file exists - resuming session"
    cat .github/agent-pr-session/pr-XXXXX.md
else
    echo "Creating new state file"
fi
```

**If the file EXISTS**: Read it to determine your current phase and resume from there. Look for:
- Which phase has `▶️ IN PROGRESS` status - that's where you left off
- Which phases have `✅ PASSED` status - those are complete
- Which phases have `⏳ PENDING` status - those haven't started

**If the file does NOT exist**: Create it with the template structure:

```markdown
# PR Review: #XXXXX - [Issue Title TBD]

**Date:** [TODAY] | **Issue:** [#XXXXX](https://github.com/dotnet/maui/issues/XXXXX) | **PR:** [#YYYYY](https://github.com/dotnet/maui/pull/YYYYY) or None

## ⏳ Status: IN PROGRESS

| Phase | Status |
|-------|--------|
| Pre-Flight | ▶️ IN PROGRESS |
| 🧪 Tests | ⏳ PENDING |
| 🚦 Gate | ⏳ PENDING |
| 🔧 Fix | ⏳ PENDING |
| 📋 Report | ⏳ PENDING |

---

<details>
<summary><strong>📋 Issue Summary</strong></summary>

[From issue body]

**Steps to Reproduce:**
1. [Step 1]
2. [Step 2]

**Platforms Affected:**
- [ ] iOS
- [ ] Android
- [ ] Windows
- [ ] MacCatalyst

</details>

<details>
<summary><strong>📁 Files Changed</strong></summary>

| File | Type | Changes |
|------|------|---------|
| `path/to/fix.cs` | Fix | +X lines |
| `path/to/test.cs` | Test | +Y lines |

</details>

<details>
<summary><strong>💬 PR Discussion Summary</strong></summary>

**Key Comments:**
- [Notable comments from issue/PR discussion]

**Reviewer Feedback:**
- [Key points from review comments]

**Disagreements to Investigate:**
| File:Line | Reviewer Says | Author Says | Status |
|-----------|---------------|-------------|--------|

**Author Uncertainty:**
- [Areas where author expressed doubt]

</details>

<details>
<summary><strong>🧪 Tests</strong></summary>

**Status**: ⏳ PENDING

- [ ] PR includes UI tests
- [ ] Tests reproduce the issue
- [ ] Tests follow naming convention (`IssueXXXXX`)

**Test Files:**
- HostApp: [PENDING]
- NUnit: [PENDING]

</details>

<details>
<summary><strong>🚦 Gate - Test Verification</strong></summary>

**Status**: ⏳ PENDING

- [ ] Tests FAIL (bug reproduced)

**Result:** [PENDING]

</details>

<details>
<summary><strong>🔧 Fix Candidates</strong></summary>

**Status**: ⏳ PENDING

| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| PR | PR #XXXXX | [PR's approach - from Pre-Flight] | ⏳ PENDING (Gate) | [files] | Original PR - validated by Gate |

**Note:** try-fix candidates (1, 2, 3...) are added during Phase 4. PR's fix is reference only.

**Exhausted:** No
**Selected Fix:** [PENDING]

</details>

---

**Next Step:** [UPDATE THIS AFTER EACH PHASE - e.g., "Complete Tests phase", "Run Gate verification", "After Gate passes, read post-gate.md for phases 4-5", etc.]
```

This file:
- Serves as your TODO list for all phases
- Tracks progress if interrupted
- Must exist before you start gathering context
- **Always include when committing changes** (to `.github/agent-pr-session/`)
- **Phases 4-5 sections are added AFTER Gate passes** (see `pr/post-gate.md`)

**Then gather context and update the file as you go.**

### Step 1: Gather Context (depends on starting point)

**If starting from a PR:**
```bash
# Checkout the PR
git fetch origin pull/XXXXX/head:pr-XXXXX
git checkout pr-XXXXX

# Fetch PR metadata
gh pr view XXXXX --json title,body,url,author,labels,files

# Find and read linked issue
gh pr view XXXXX --json body --jq '.body' | grep -oE "(Fixes|Closes|Resolves) #[0-9]+" | head -1
gh issue view ISSUE_NUMBER --json title,body,comments
```

**If starting from an Issue (no PR exists):**
```bash
# Stay on current branch - do NOT checkout anything
# Fetch issue details directly
gh issue view XXXXX --json title,body,comments,labels
```

### Step 2: Fetch Comments

**If PR exists** - Fetch PR discussion:
```bash
# PR-level comments
gh pr view XXXXX --json comments --jq '.comments[] | "Author: \(.author.login)\n\(.body)\n---"'

# Review summaries
gh pr view XXXXX --json reviews --jq '.reviews[] | "Reviewer: \(.author.login) [\(.state)]\n\(.body)\n---"'

# Inline code review comments (CRITICAL - often contains key technical feedback!)
gh api "repos/dotnet/maui/pulls/XXXXX/comments" --jq '.[] | "File: \(.path):\(.line // .original_line)\nAuthor: \(.user.login)\n\(.body)\n---"'

# Detect Prior Agent Reviews
gh pr view XXXXX --json comments --jq '.comments[] | select(.body | contains("Final Recommendation") and contains("| Phase | Status |")) | .body'
```

**If issue only** - Comments already fetched in Step 1.

**Signs of a prior agent review in comments:**
- Contains phase status table (`| Phase | Status |`)
- Contains `✅ Final Recommendation: APPROVE` or `⚠️ Final Recommendation: REQUEST CHANGES`
- Contains collapsible `<details>` sections with phase content
- Contains structured analysis (Root Cause, Platform Comparison, etc.)

**If prior agent review found:**
1. **Extract and use as state file content** - The review IS the completed state
2. Parse the phase statuses to determine what's already done
3. Import all findings (fix candidates, test results)
4. Update your local state file with this content
5. Resume from whichever phase is not yet complete (or report as done)

**Do NOT:**
- Start from scratch if a complete review already exists
- Treat the prior review as just "reference material"
- Re-do phases that are already marked `✅ PASSED`

### Step 3: Document Key Findings

Update the state file `.github/agent-pr-session/pr-XXXXX.md`:

**If PR exists** - Document disagreements and reviewer feedback:
| File:Line | Reviewer Says | Author Says | Status |
|-----------|---------------|-------------|--------|
| Example.cs:95 | "Remove this call" | "Required for fix" | ⚠️ INVESTIGATE |

**Edge Cases to Check** (from comments mentioning "what about...", "does this work with..."):
- [ ] Edge case 1 from discussion
- [ ] Edge case 2 from discussion

### Step 4: Classify Files (if PR exists)

```bash
gh pr view XXXXX --json files --jq '.files[].path'
```

Classify into:
- **Fix files**: Source code (`src/Controls/src/...`, `src/Core/src/...`)
- **Test files**: Tests (`DeviceTests/`, `TestCases.HostApp/`, `UnitTests/`)

Identify test type: **UI Tests** | **Device Tests** | **Unit Tests**

**Record PR's fix as reference** (at the bottom of the Fix Candidates table):

```markdown
| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| PR | PR #XXXXX | [Describe PR's approach] | ⏳ PENDING (Gate) | `file.cs` (+N) | Original PR |
```

**Note:** The PR's fix is validated by Gate (Phase 3), NOT by try-fix. try-fix candidates are numbered 1, 2, 3... and are YOUR independent ideas.

The test result will be updated to `✅ PASS (Gate)` after Gate passes.

### Step 5: Complete Pre-Flight

**🚨 CRITICAL: Update state file in TWO places:**

**A. Update the stage table** (top of file):
1. Change Pre-Flight status from `▶️ IN PROGRESS` to `✅ COMPLETE`
2. Change 🧪 Tests status to `▶️ IN PROGRESS`

**B. Update the detailed sections:**
1. Fill in `<details><summary>📋 Issue Summary</summary>` with issue description, steps to reproduce, platforms affected, regression info
2. Fill in `<details><summary>💬 PR Discussion Summary</summary>` with comments, disagreements, and edge cases (if PR exists)
3. Remove `**Status**: ⏳ PENDING` from completed sections or keep it for sections not yet started

---

## 🧪 TESTS: Create/Verify Reproduction Tests (Phase 2)

> **SCOPE**: Ensure tests exist that reproduce the issue. **Tests must be verified to FAIL before this phase is complete.**

**⚠️ Gate Check:** Pre-Flight must be `✅ COMPLETE` before starting this phase.

### Step 1: Check if Tests Already Exist

**If PR exists:**
```bash
gh pr view XXXXX --json files --jq '.files[].path' | grep -E "TestCases\.(HostApp|Shared\.Tests)"
```

**If issue only:**
```bash
# Check if tests exist for this issue number
find src/Controls/tests -name "*XXXXX*" -type f 2>/dev/null
```

**If tests exist** → Verify they follow conventions and reproduce the bug.

**If NO tests exist** → Create them using the `write-tests` skill.

### Step 2: Create Tests (if needed)

Invoke the `write-tests` skill which will:
1. Read `.github/instructions/uitests.instructions.md` for conventions
2. Create HostApp page: `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.cs`
3. Create NUnit test: `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`
4. **Verify tests FAIL** (reproduce the bug) - iterating until they do

### Step 3: Verify Tests Compile

```bash
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -c Debug -f net10.0-android --no-restore -v q
dotnet build src/Controls/tests/TestCases.Shared.Tests/Controls.TestCases.Shared.Tests.csproj -c Debug --no-restore -v q
```

### Step 4: Verify Tests Reproduce the Bug (if not done by write-tests skill)

```bash
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform ios -TestFilter "IssueXXXXX"
```

The script auto-detects mode based on git diff. If only test files changed, it verifies tests FAIL.

**Tests must FAIL.** If they pass, the test is wrong - fix it and rerun.

### Complete 🧪 Tests

**🚨 CRITICAL: Update state file in TWO places:**

**A. Update the stage table** (top of file):
1. Change 🧪 Tests status to `✅ COMPLETE`
2. Change 🚦 Gate status to `▶️ IN PROGRESS`

**B. Update the detailed `<details><summary>🧪 Tests</summary>` section:**
1. Change `**Status**: ⏳ PENDING` to `**Status**: ✅ COMPLETE`
2. Check off completed items in the checklist (e.g., `- [x] PR includes UI tests`)
3. Fill in test file paths (HostApp, NUnit)
4. Add note: "Tests verified to FAIL (bug reproduced)" or document any findings

---

## 🚦 GATE: Verify Tests Catch the Issue (Phase 3)

> **SCOPE**: Verify tests correctly detect the fix (for PRs) or confirm tests were verified (for issues).

**⛔ This phase MUST pass before continuing. If it fails, stop and fix the tests.**

**⚠️ Gate Check:** 🧪 Tests must be `✅ COMPLETE` before starting this phase.

### Gate Depends on Starting Point

**If starting from an Issue (no fix yet):**
Tests were already verified to FAIL in Phase 2. Gate is a confirmation step:
- Confirm tests were run and failed
- Mark Gate as passed
- Proceed to Phase 4 (Fix) to implement fix

**If starting from a PR (fix exists):**
Use full verification mode - tests should FAIL without fix, PASS with fix.

```bash
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform android -RequireFullVerification
```

### Expected Output (PR with fix)

```
╔═══════════════════════════════════════════════════════════╗
║              VERIFICATION PASSED ✅                       ║
╠═══════════════════════════════════════════════════════════╣
║  - FAIL without fix (as expected)                         ║
║  - PASS with fix (as expected)                            ║
╚═══════════════════════════════════════════════════════════╝
```

### If Tests Don't Behave as Expected

**If tests PASS without fix** → Tests don't catch the bug. Go back to Phase 2, invoke `write-tests` skill again to fix the tests.

### Complete 🚦 Gate

**🚨 CRITICAL: Update state file in TWO places:**

**A. Update the stage table** (top of file):
1. Change 🚦 Gate status to `✅ PASSED`

**B. Update the detailed `<details><summary>🚦 Gate - Test Verification</summary>` section:**
1. Change `**Status**: ⏳ PENDING` to `**Status**: ✅ PASSED`
2. Fill in `**Result**: PASSED ✅` with verification details
3. Check off the checklist items:
   - `- [x] Tests FAIL without fix (bug reproduced)`
   - `- [x] Tests PASS with fix (bug resolved)` (if PR exists)

**C. Proceed to Phase 4:**
1. Read `.github/agents/pr/post-gate.md` for Phase 4-5 instructions

---

## ⛔ STOP HERE

**If Gate is `✅ PASSED`** → Read `.github/agents/pr/post-gate.md` to continue with phases 4-5.

**If Gate `❌ FAILED`** → Stop. Request changes from the PR author to fix the tests.

---

## Common Pre-Gate Mistakes

- ❌ **Researching root cause during Pre-Flight** - Just document what the issue says, save analysis for Phase 4
- ❌ **Looking at implementation code during Pre-Flight** - Just gather issue/PR context
- ❌ **Forming opinions on the fix during Pre-Flight** - That's Phase 4
- ❌ **Running tests during Pre-Flight** - That's Phase 3
- ❌ **Not creating state file first** - ALWAYS create state file before gathering context
- ❌ **Skipping to Phase 4** - Gate MUST pass first
