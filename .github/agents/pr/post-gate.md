# PR Agent: Post-Gate Phases (4-5)

**⚠️ PREREQUISITE: Only read this file after 🚦 Gate shows `✅ PASSED` in your state file.**

If Gate is not passed, go back to `.github/agents/pr.md` and complete phases 1-3 first.

---

## Workflow Overview

| Phase | Name | What Happens |
|-------|------|--------------|
| 4 | **Fix** | Explore fix candidates using `try-fix` skill, select best one |
| 5 | **Report** | Deliver result (approve PR, request changes, or create new PR) |

---

## 🔧 FIX: Explore and Select Fix (Phase 4)

> **SCOPE**: Explore alternative fixes using `try-fix` skill, select the best approach.

**⚠️ Gate Check:** Verify 🚦 Gate is `✅ PASSED` in your state file before proceeding.

### Step 1: Loop - Call try-fix Skill

Invoke the `try-fix` skill repeatedly to explore alternative fixes:

```
┌─────────────────────────────────────────────────────────────┐
│  try-fix loop (max 5 total candidates)                      │
├─────────────────────────────────────────────────────────────┤
│  1. Invoke try-fix skill                                    │
│  2. Skill reads state file, sees prior attempts             │
│  3. Skill proposes NEW approach, tests it, records result   │
│  4. Skill reverts changes, returns to agent                 │
│  5. Check: exhausted=true OR 5 candidates reached?          │
│     YES → Exit loop                                         │
│     NO  → Go to step 1                                      │
└─────────────────────────────────────────────────────────────┘
```

**Stop the loop when:**
- `try-fix` returns `exhausted=true` (no more ideas)
- 5 candidates have been recorded in the table
- User requests to stop

### Step 2: Select Best Fix

Review the **Fix Candidates** table and select the best approach:

**Selection criteria (in order of priority):**
1. **Must pass tests** - Only consider candidates with ✅ PASS
2. **Simplest solution** - Fewer lines changed, lower complexity
3. **Most robust** - Handles edge cases, less likely to regress
4. **Matches codebase style** - Consistent with existing patterns

Update the state file:

```markdown
**Exhausted:** Yes
**Selected Fix:** #N - [Reason for selection]
```

### Step 3: Apply Selected Fix

Apply the selected fix to the working tree (it was reverted after testing):

**If selected fix is the PR's fix (#1):**
- No action needed - PR's changes are already in place

**If selected fix is an alternative (#2+):**
- Re-implement the fix (you documented the approach in the table)
- Or: if you saved a patch, apply it

### Complete 🔧 Fix

**Update state file**:
1. Verify Fix Candidates table is complete
2. Verify Selected Fix is documented
3. Change 🔧 Fix status to `✅ COMPLETE`
4. Change 📋 Report status to `▶️ IN PROGRESS`

---

## 📋 REPORT: Final Report (Phase 5)

> **SCOPE**: Deliver the final result - either a PR review or a new PR.

**⚠️ Gate Check:** Verify ALL phases 1-4 are `✅ COMPLETE` or `✅ PASSED` before proceeding.

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
   gh pr create --title "Fix #XXXXX: [Title]" --body "Fixes #XXXXX

   ## Description
   [Brief description of the fix]

   ## Root Cause
   [What was causing the issue]

   ## Solution
   [Selected approach and why]

   ## Other Approaches Considered
   [Brief summary of alternatives tried]

   ## Testing
   - Added UI tests: IssueXXXXX.cs
   - Tests verify [what the tests check]
   "
   ```

5. **Update state file** with PR link

### If Starting from PR - Write Review

Determine your recommendation based on the Fix phase:

**If PR's fix (Candidate #1) was selected:**
- Recommend: `✅ APPROVE`
- Justification: PR's approach is correct/optimal

**If an alternative fix was selected:**
- Recommend: `⚠️ REQUEST CHANGES`
- Justification: Suggest the better approach from Candidate #N

**If PR's fix failed tests:**
- Recommend: `⚠️ REQUEST CHANGES`
- Justification: Fix doesn't work, suggest alternatives

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

---

## Common Mistakes in Post-Gate Phases

- ❌ **Skipping the try-fix loop** - Always explore at least one alternative
- ❌ **Selecting a failing fix** - Only select from passing candidates
- ❌ **Forgetting to revert between attempts** - Each try-fix must start clean
- ❌ **Rushing the report** - Take time to write clear justification
