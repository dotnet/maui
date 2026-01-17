# PR Agent: Post-Gate Phases (4-5)

**⚠️ PREREQUISITE: Only read this file after 🚦 Gate shows `✅ PASSED` in your state file.**

If Gate is not passed, go back to `.github/agents/pr.md` and complete phases 1-3 first.

---

## Workflow Overview

| Phase | Name | What Happens |
|-------|------|--------------|
| 4 | **Fix** | Invoke `try-fix` skill repeatedly to explore independent alternatives, then compare with PR's fix |
| 5 | **Report** | Deliver result (approve PR, request changes, or create new PR) |

---

## 🔧 FIX: Explore and Select Fix (Phase 4)

> **SCOPE**: Explore independent fix alternatives using `try-fix` skill, compare with PR's fix, select the best approach.

**⚠️ Gate Check:** Verify 🚦 Gate is `✅ PASSED` in your state file before proceeding.

### 🚨 CRITICAL: try-fix is Independent of PR's Fix

**The PR's fix has already been validated by Gate (tests FAIL without it, PASS with it).**

The purpose of Phase 4 is NOT to re-test the PR's fix, but to:
1. **Generate independent fix ideas** - What would YOU do to fix this bug?
2. **Test those ideas empirically** - Actually implement and run tests
3. **Compare with PR's fix** - Is there a simpler/better alternative?
4. **Learn from failures** - Record WHY failed attempts didn't work

**Do NOT let the PR's fix influence your thinking.** Generate ideas as if you hadn't seen the PR.

### Step 0: Verify State File and Change Status

**🚨 BEFORE starting try-fix loop:**
1. Read state file `.github/agent-pr-session/pr-XXXXX.md`
2. Verify 🚦 Gate is `✅ PASSED`
3. Change 🔧 Fix status from `⏳ PENDING` to `▶️ IN PROGRESS`
4. **COMMIT the state file**: `git add .github/agent-pr-session/pr-XXXXX.md && git commit -m "Starting Fix phase"`

### Step 1: Agent Orchestrates try-fix Loop

Invoke the `try-fix` skill repeatedly. The skill handles one fix attempt per invocation.

```
┌─────────────────────────────────────────────────────────────┐
│  Agent orchestration loop                                   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  attempts = 0                                               │
│  max_attempts = 5                                           │
│                                                             │
│  while (attempts < max_attempts):                           │
│      result = invoke try-fix skill                          │
│      attempts++                                             │
│                                                             │
│      if result.exhausted:                                   │
│          break  # try-fix has no more ideas                 │
│                                                             │
│      # result.passed indicates if this attempt worked       │
│      # Continue loop to explore more alternatives           │
│                                                             │
│  # After loop: compare all try-fix results vs PR's fix      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Stop the loop when:**
- `try-fix` returns `exhausted=true` (no more ideas)
- 5 try-fix attempts have been made
- User requests to stop

### What try-fix Does (Each Invocation)

Each `try-fix` invocation:
1. Reads state file to learn from prior failed attempts
2. Reverts PR's fix to get a broken baseline
3. Proposes ONE new independent fix idea
4. Implements and tests it
5. Records result (with failure analysis if it failed)
6. Reverts all changes (restores PR's fix)
7. Returns `{passed: bool, exhausted: bool}`

See `.github/skills/try-fix/SKILL.md` for full details.

### Step 2: Compare Results

After the loop, review the **Fix Candidates** table:

```markdown
| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| 1 | try-fix | Fix in TabbedPageManager | ❌ FAIL | 1 file | Why failed: Too late in lifecycle |
| 2 | try-fix | RequestApplyInsets only | ❌ FAIL | 1 file | Why failed: Trigger insufficient |
| 3 | try-fix | Reset + RequestApplyInsets | ✅ PASS | 2 files | Works! |
| PR | PR #33359 | [PR's approach] | ✅ PASS (Gate) | 2 files | Original PR |
```

**Compare passing candidates:**
- PR's fix (known to pass from Gate)
- Any try-fix attempts that passed

### Step 3: Select Best Fix

**Selection criteria (in order of priority):**
1. **Must pass tests** - Only consider candidates with ✅ PASS
2. **Simplest solution** - Fewer files, fewer lines, lower complexity
3. **Most robust** - Handles edge cases, less likely to regress
4. **Matches codebase style** - Consistent with existing patterns

Update the state file:

```markdown
**Exhausted:** Yes (or No if stopped early)
**Selected Fix:** PR's fix - [Reason] OR #N - [Reason why alternative is better]
```

**Possible outcomes:**
- **PR's fix is best** → Approve the PR
- **try-fix found a simpler/better alternative** → Request changes with suggestion
- **try-fix found same solution independently** → Strong validation, approve PR
- **All try-fix attempts failed** → PR's fix is the only working solution, approve PR

### Step 4: Apply Selected Fix (if different from PR)

**If PR's fix was selected:**
- No action needed - PR's changes are already in place

**If a try-fix alternative was selected:**
- Re-implement the fix (you documented the approach in the table)
- Commit the changes

### Complete 🔧 Fix

**🚨 CRITICAL: Update state file in TWO places:**

**A. Update the stage table** (top of file):
1. Change 🔧 Fix status to `✅ COMPLETE`
2. Change 📋 Report status to `▶️ IN PROGRESS`

**B. Update the detailed `<details><summary>🔧 Fix Candidates</summary>` section:**
1. Change `**Status**: ⏳ PENDING` to `**Status**: ✅ COMPLETE`
2. Verify Fix Candidates table is complete with all try-fix attempts (numbered 1, 2, 3...)
3. Verify failure analyses are documented for failed attempts (why they didn't work)
4. Verify **Selected Fix** is documented with reasoning (PR's fix OR try-fix alternative)
5. Update `**Exhausted:**` field (Yes/No - did you explore enough alternatives?)

**C. Commit the state file:**
```bash
git add .github/agent-pr-session/pr-XXXXX.md
git commit -m "Fix phase complete - selected fix: [summary]"
```

**⛔ DO NOT proceed to Phase 5 (Report) until state file is updated and committed.**

---

## 📋 REPORT: Final Report (Phase 5)

> **SCOPE**: Deliver the final result - either a PR review or a new PR.

**⚠️ Gate Check:** Verify ALL phases 1-4 are `✅ COMPLETE` or `✅ PASSED` before proceeding.

### Finalize Title and Description

**Invoke the `pr-finalize` skill** to ensure the PR title and description:
- Accurately reflect the actual implementation
- Provide context for future agents (root cause, key insight, what to avoid)
- Follow the repository's PR template structure

See `.github/skills/pr-finalize/SKILL.md` for details.

If creating a new PR (from issue), use the skill's output template to write the PR body.
If reviewing an existing PR, check if title/description need updates and include in review.

### If Starting from Issue (No PR) - Create PR

1. **Ensure selected fix is applied and committed**:
   ```bash
   git add -A
   git commit -m "Fix #XXXXX: [Description of fix]"
   ```

2. **Create a feature branch** (if not already on one):
   ```bash
   git checkout -b fix/issue-XXXXX
   ```

3. **⛔ STOP: Ask user for confirmation before creating PR**:
   
   Present a summary to the user and wait for explicit approval:
   > "I'm ready to create a PR for issue #XXXXX. Here's what will be included:
   > - **Branch**: fix/issue-XXXXX
   > - **Selected fix**: Candidate #N - [approach]
   > - **Files changed**: [list files]
   > - **Tests added**: [list test files]
   > - **Other candidates considered**: [brief summary]
   > 
   > Would you like me to push and create the PR?"
   
   **Do NOT proceed until user confirms.**

4. **Push and create PR** (after user confirmation):

   ```bash
   git push -u origin fix/issue-XXXXX
   gh pr create --title "[Platform] Brief description of behavior fix" --body "<pr-finalize skill output>"
   ```
   
   Use the `pr-finalize` skill output as the `--body` argument.

5. **Update state file** with PR link

### If Starting from PR - Write Review

Determine your recommendation based on the Fix phase:

**If PR's fix was selected:**
- Recommend: `✅ APPROVE`
- Justification: PR's approach is correct/optimal

**If an alternative fix was selected:**
- Recommend: `⚠️ REQUEST CHANGES`
- Justification: Suggest the better approach from try-fix Candidate #N

**If PR's fix failed tests:**
- Recommend: `⚠️ REQUEST CHANGES`
- Justification: Fix doesn't work, suggest alternatives

**Check title/description accuracy:**
- Run the `pr-finalize` skill to verify title and description match implementation
- If discrepancies found, include suggested updates in review comments

### Final State File Format

Update the state file header:

```markdown
## ✅ Final Recommendation: APPROVE
```
or
```markdown
## ⚠️ Final Recommendation: REQUEST CHANGES
```

Update all phase statuses to complete.

### Complete 📋 Report

**Update state file**:
1. Change header status to final recommendation
2. Update all phases to `✅ COMPLETE` or `✅ PASSED`
3. Present final result to user
4. Update the "Next Steps" section documenting follow-up actions (e.g., wait for PR merge, monitor CI, etc.)
5. **COMMIT the state file**: `git add .github/agent-pr-session/pr-XXXXX.md && git commit -m "PR review complete - [APPROVE/REQUEST CHANGES]"`
---

## Common Mistakes in Post-Gate Phases

- ❌ **Looking at PR's fix before generating ideas** - Generate fix ideas independently first
- ❌ **Re-testing the PR's fix in try-fix** - Gate already validated it; try-fix tests YOUR ideas
- ❌ **Skipping the try-fix loop** - Always explore at least one independent alternative
- ❌ **Not analyzing why fixes failed** - Record the flawed reasoning to help future attempts
- ❌ **Selecting a failing fix** - Only select from passing candidates
- ❌ **Forgetting to revert between attempts** - Each try-fix must start from broken baseline, end with PR restored
- ❌ **Rushing the report** - Take time to write clear justification
