---
name: pr
description: Sequential 4-phase workflow for GitHub issues - Pre-Flight, Gate, Fix, Report. Phases MUST complete in order.
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
- ❌ Only write tests without fixing → Use `write-tests-agent`

---

## Workflow Overview

This file covers **Phases 1-2** (Pre-Flight → Gate).

After Gate passes, read `.github/agents/pr/post-gate.md` for **Phases 3-4**.

```
┌─────────────────────────────────────────┐     ┌─────────────────────────────────────────────┐
│  THIS FILE: pr.md                       │     │  pr/post-gate.md                            │
│                                         │     │                                             │
│  1. Pre-Flight  →  2. Gate              │ ──► │  3. Fix  →  4. Report                       │
│                       ⛔                 │     │                                             │
│                  MUST PASS              │     │  (Only read after Gate ✅ PASSED)           │
└─────────────────────────────────────────┘     └─────────────────────────────────────────────┘
```

---

## 🚨 Critical Rules

**Read `.github/agents/pr/SHARED-RULES.md` for complete details on:**
- Phase Completion Protocol (fill ALL pending fields before marking complete)
- Follow Templates EXACTLY (no `open` attributes, no "improvements")
- No Direct Git Commands (use `gh pr diff/view`, let scripts handle files)
- Use Skills' Scripts (don't bypass with manual commands)
- Stop on Environment Blockers (retry once, then skip and continue autonomously)
- Multi-Model Configuration (5 models for Phase 4)
- Platform Selection (must be affected AND available on host)

**Key points:**
- ❌ Never run `git checkout`, `git switch`, `git stash`, `git reset` - agent is always on correct branch
- ❌ Never stop and ask user - use best judgment to skip blocked phases and continue
- ❌ Never mark phase ✅ with [PENDING] fields remaining

Phase 3 uses a 5-model exploration workflow. See `post-gate.md` for detailed instructions after Gate passes.

---

## PRE-FLIGHT: Context Gathering (Phase 1)

> **⚠️ SCOPE**: Document only. No code analysis. No fix opinions. No running tests.

### ❌ Pre-Flight Boundaries (What NOT To Do)

| ❌ Do NOT | Why | When to do it |
|-----------|-----|---------------|
| Research git history | That's root cause analysis | Phase 3: 🔧 Fix |
| Look at implementation code | That's understanding the bug | Phase 3: 🔧 Fix |
| Design or implement fixes | That's solution design | Phase 3: 🔧 Fix |
| Form opinions on correct approach | That's analysis | Phase 3: 🔧 Fix |
| Run tests | That's verification | Phase 2: 🚦 Gate |

### ✅ What TO Do in Pre-Flight

- Read issue description and comments
- Note platforms affected (from labels)
- Identify files changed (if PR exists)
- Document disagreements and edge cases from comments

### Step 1: Gather Context (depends on starting point)

**If starting from a PR:**
```bash
# Fetch PR metadata (agent is already on correct branch)
gh pr view XXXXX --json title,body,url,author,labels,files

# Find and read linked issue
gh pr view XXXXX --json body --jq '.body' | grep -oE "(Fixes|Closes|Resolves) #[0-9]+" | head -1
gh issue view ISSUE_NUMBER --json title,body,comments
```

**If starting from an Issue (no PR exists):**
```bash
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
1. Parse the phase statuses to determine what's already done
2. Import all findings (fix candidates, test results)
3. Resume from whichever phase is not yet complete (or report as done)

**Do NOT:**
- Start from scratch if a complete review already exists
- Treat the prior review as just "reference material"
- Re-do phases that are already marked `✅ PASSED`

### Step 3: Document Key Findings

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

Verify the following before proceeding:
- [ ] Issue summary captured
- [ ] Platform information noted
- [ ] Files changed identified (if PR exists)
- [ ] PR discussion summarized (if PR exists)
- [ ] **Write phase output to `CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/pre-flight/content.md`** (see SHARED-RULES.md "Phase Output Artifacts")

---

## 🚦 GATE: Verify Tests Catch the Issue (Phase 2)

> **SCOPE**: Verify tests exist and correctly detect the fix (for PRs) or reproduce the bug (for issues).

**⛔ This phase MUST pass before continuing. If it fails, stop and fix the tests.**

**⚠️ Gate Check:** Pre-Flight must be `✅ COMPLETE` before starting this phase.

### Step 1: Check if Tests Exist

**If PR exists:**
```bash
gh pr view XXXXX --json files --jq '.files[].path' | grep -E "TestCases\.(HostApp|Shared\.Tests)"
```

**If issue only:**
```bash
# Check if tests exist for this issue number
find src/Controls/tests -name "*XXXXX*" -type f 2>/dev/null
```

**If tests exist** → Proceed to verification.

**If NO tests exist** → Let the user know that tests are missing. They can use the `write-tests-agent` to help create them.

### Step 2: Select Platform

**🚨 CRITICAL: Choose a platform that is BOTH affected by the bug AND available on the current host.**

**Identify affected platforms** from Pre-Flight:
- Check the platforms affected from Pre-Flight context
- Check issue labels (e.g., `platform/iOS`, `platform/Android`)
- Check which platform-specific files the PR modifies

**Match to available platforms on current host:**

| Host OS | Available Platforms |
|---------|---------------------|
| Windows | Android, Windows |
| macOS | Android, iOS, MacCatalyst |

**Select the best match:**
1. Pick a platform that IS affected by the bug
2. That IS available on the current host
3. Prefer the platform most directly impacted by the PR's code changes

**Example decisions:**
- Bug affects iOS/Windows/MacCatalyst, host is Windows → Test on **Windows**
- Bug affects iOS only, host is Windows → **STOP** - cannot test (ask user)
- Bug affects Android only → Test on **Android** (works on any host)
- Bug affects all platforms → Pick based on host (Windows on Windows, iOS on macOS)

**⚠️ Do NOT test on a platform that isn't affected by the bug** - the test will pass regardless of whether the fix works.

### Step 3: Run Verification

**🚨 MUST invoke as a task agent** to prevent command substitution:

```markdown
Invoke the `task` agent with agent_type: "task" and this prompt:

"Invoke the verify-tests-fail-without-fix skill for this PR:
- Platform: [selected platform from Platform Selection above]
- TestFilter: 'IssueXXXXX'
- RequireFullVerification: true

Report back: Did tests FAIL without fix? Did tests PASS with fix? Final status?"
```

**Why task agent?** Running inline allows substituting commands and fabricating results. Task agent runs in isolation and reports exactly what happened.

See `.github/skills/verify-tests-fail-without-fix/SKILL.md` for full skill documentation.

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

**If tests PASS without fix** → Tests don't catch the bug. Let the user know the tests need to be fixed. They can use the `write-tests-agent` for help.

### Complete 🚦 Gate

Verify the following before proceeding:
- [ ] Test result documented (PASSED ✅ or FAILED ❌)
- [ ] Test behavior documented
- [ ] Platform tested noted
- [ ] **Write phase output to `CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/gate/content.md`** (see SHARED-RULES.md "Phase Output Artifacts")

---

## ⛔ STOP HERE

**If Gate is `✅ PASSED`** → Read `.github/agents/pr/post-gate.md` to continue with phases 3-4.

**If Gate `❌ FAILED`** → Stop. Request changes from the PR author to fix the tests.

---

## Common Pre-Gate Mistakes

- ❌ **Researching root cause during Pre-Flight** - Just document what the issue says, save analysis for Phase 3
- ❌ **Looking at implementation code during Pre-Flight** - Just gather issue/PR context
- ❌ **Forming opinions on the fix during Pre-Flight** - That's Phase 3
- ❌ **Running tests during Pre-Flight** - That's Phase 2 (Gate)
- ❌ **Skipping to Phase 3** - Gate MUST pass first

## Common Gate Mistakes

- ❌ **Running Gate verification inline** - Use task agent to prevent command substitution
- ❌ **Using `BuildAndRunHostApp.ps1` for Gate** - That only runs ONE direction; the skill does TWO runs
- ❌ **Using manual `dotnet test` commands** - Doesn't revert/restore fix files automatically
- ❌ **Claiming "fails both ways" from a single test run** - That's fabrication; you need the script's TWO runs
- ❌ **Not waiting for task agent completion** - Script takes 5-10+ minutes; wait for task to return

**🚨 The verify-tests-fail.ps1 script does TWO test runs automatically:**
1. Reverts fix → runs tests (should FAIL)
2. Restores fix → runs tests (should PASS)

Never run Gate inline. Always invoke as task agent.
