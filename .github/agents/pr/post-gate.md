# PR Agent: Post-Gate Phases (3-4)

**⚠️ PREREQUISITE: Only read this file after 🚦 Gate shows `✅ PASSED` in your state file.**

If Gate is not passed, go back to `.github/agents/pr.md` and complete phases 1-2 first.

---

## Workflow Overview

| Phase | Name | What Happens |
|-------|------|--------------|
| 3 | **Fix** | Invoke `try-fix` skill repeatedly to explore independent alternatives, then compare with PR's fix |
| 4 | **Report** | Deliver result (approve PR, request changes, or create new PR) |

---

## 🚨 Critical Rules

**All rules from `.github/agents/pr/SHARED-RULES.md` apply here**, including:
- Phase Completion Protocol (fill ALL pending fields before marking complete)
- Stop on Environment Blockers (STOP and ask user, don't continue)
- Multi-Model Configuration (5 models, SEQUENTIAL only)

If try-fix cannot run due to environment issues, **STOP and ask the user**. Do NOT mark attempts as "BLOCKED" and continue.

### 🚨 CRITICAL: Stop on Environment Blockers (Applies to Phase 4)

The same "Stop on Environment Blockers" rule from `pr.md` applies here. If try-fix cannot run due to:
- Missing Appium drivers
- Device/emulator not available
- WinAppDriver not installed
- Platform tools missing

**STOP and ask the user** before continuing. Do NOT mark try-fix attempts as "BLOCKED" and continue. Either fix the environment issue or get explicit user permission to skip.

---

## 🔧 FIX: Explore and Select Fix (Phase 3)

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

### Step 1: Multi-Model try-fix Exploration

Phase 4 uses a **multi-model approach** to maximize fix diversity. Each AI model brings different perspectives and may find solutions others miss.

**⚠️ SEQUENTIAL ONLY**: try-fix runs MUST execute one at a time. They modify the same files and use the same test device. Never run try-fix attempts in parallel.

#### Round 1: Run try-fix with Each Model

Run the `try-fix` skill **5 times sequentially**, once with each model (see `SHARED-RULES.md` for model list).

**For each model**, invoke the try-fix skill:
```
Invoke the try-fix skill for PR #XXXXX:
- problem: [Description of the bug from issue/PR - what's broken and expected behavior]
- platform: [Use platform selected in Gate phase - must be affected by the bug AND available on host]
- test_command: pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform [same platform] -TestFilter "IssueXXXXX"
- target_files:
  - src/[area]/[likely-affected-file-1].cs
  - src/[area]/[likely-affected-file-2].cs
- state_file: CustomAgentLogsTmp/PRState/pr-XXXXX.md

Generate ONE independent fix idea. Review the PR's fix first to ensure your approach is DIFFERENT.
```

**Wait for each to complete before starting the next.**

**🧹 MANDATORY: Clean up between attempts.** After each try-fix completes (pass or fail), run these commands before starting the next attempt:

```bash
# 1. Restore any baseline state from the previous attempt (safe no-op if none exists)
pwsh .github/scripts/EstablishBrokenBaseline.ps1 -Restore

# 2. Restore all tracked files to HEAD (the merged PR state)
# This catches any files the previous attempt modified but didn't restore
git checkout HEAD -- .
```

**Why this is required:** Each try-fix attempt modifies source files. If an attempt fails mid-way (build error, timeout, model error), it may not run its own cleanup step. Without explicit cleanup, the next attempt starts with a dirty working tree, which can cause missing files, corrupt state, or misleading test results. Use `HEAD` (not just `-- .`) to also restore deleted files.

#### Round 2+: Cross-Pollination Loop (MANDATORY)

After Round 1, invoke EACH of the 5 models to ask for new ideas. **No shortcuts allowed.**

**❌ WRONG**: Using `explore`/`glob`, declaring exhaustion without invoking each model
**✅ CORRECT**: Invoke EACH model via task agent and ask explicitly

**Steps (repeat until all 5 say "NO NEW IDEAS", max 3 rounds):**

1. **Compile bounded summary** (max 3-4 bullets per attempt):
   - Attempt #, approach (1 line), result (✅/❌), key learning (1 line)

2. **Invoke each model via task agent:**
   ```
   agent_type: "task", model: "[model-name]"
   prompt: "Review PR #XXXXX fix attempts:
     - Attempt 1: [approach] - ✅/❌
     - Attempt 2: [approach] - ✅/❌
     Do you have any NEW fix ideas? Reply: 'NEW IDEA: [desc]' or 'NO NEW IDEAS'"
   ```

3. **Record each model's response** in state file Cross-Pollination table

4. **For each new idea**: Run try-fix with that model (SEQUENTIAL, wait for completion)

5. **Exit when**: ALL 5 models say "NO NEW IDEAS" in the same round

#### try-fix Behavior

Each `try-fix` invocation (run via task agent with specific model):
1. Reads state file to learn from prior failed attempts
2. Reverts PR's fix to get a broken baseline
3. Proposes ONE new independent fix idea
4. Implements and tests it
5. Records result (with failure analysis if it failed)
6. **Updates state file** (appends row to Fix Candidates table)
7. Reverts all changes (restores PR's fix)

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
- **Multiple passing alternatives** → Select simplest/most robust

### Step 4: Apply Selected Fix (if different from PR)

**If PR's fix was selected:**
- No action needed - PR's changes are already in place

**If a try-fix alternative was selected:**
- Re-implement the fix (you documented the approach in the table)
- Apply the changes to files (do not commit - user handles git)

### Complete 🔧 Fix

**🚨 MANDATORY: Update state file**

**Update state file**:
1. Verify Fix Candidates table is complete with all attempts
2. Verify failure analyses are documented for failed attempts
3. Verify Selected Fix is documented with reasoning
4. Change 🔧 Fix status to `✅ COMPLETE`
5. Change 📋 Report status to `▶️ IN PROGRESS`

**Before marking ✅ COMPLETE, verify state file contains:**
- [ ] Round 1 completed: All 5 models ran try-fix
- [ ] **Cross-pollination table exists** with responses from ALL 5 models:
  ```
  | Model | Round 2 Response |
  |-------|------------------|
  | claude-sonnet-4.5 | NO NEW IDEAS |
  | claude-opus-4.6 | NO NEW IDEAS |
  | gpt-5.2 | NO NEW IDEAS |
  | gpt-5.2-codex | NO NEW IDEAS |
  | gemini-3-pro-preview | NO NEW IDEAS |
  ```
- [ ] Fix Candidates table has numbered rows for each try-fix attempt
- [ ] Each row has: approach, test result, files changed, notes
- [ ] "Exhausted" field set to Yes (all models confirmed no new ideas)
- [ ] "Selected Fix" populated with reasoning
- [ ] Root cause analysis documented for the selected fix (to be surfaced in 📋 Report phase "### Root Cause" section)
- [ ] No ⏳ PENDING markers remain in Fix section
- [ ] State file saved

**🚨 If cross-pollination table is missing, you skipped Round 2. Go back and invoke each model.**

---

## 📋 REPORT: Final Report (Phase 4)

> **SCOPE**: Deliver the final result - either a PR review or a new PR.

**⚠️ Gate Check:** Verify ALL phases 1-3 are `✅ COMPLETE` or `✅ PASSED` before proceeding.

### Finalize Title and Description

**Invoke the `pr-finalize` skill** to ensure the PR title and description:
- Accurately reflect the actual implementation
- Provide context for future agents (root cause, key insight, what to avoid)
- Follow the repository's PR template structure

See `.github/skills/pr-finalize/SKILL.md` for details.

If creating a new PR (from issue), use the skill's output template to write the PR body.
If reviewing an existing PR, check if title/description need updates and include in review.

### If Starting from Issue (No PR) - Create PR

1. **⛔ STOP: Ask user to commit and create PR**:
   
   Present a summary to the user and wait for them to handle git operations:
   > "I've implemented the fix for issue #XXXXX. Here's what needs to be committed:
   > - **Selected fix**: Candidate #N - [approach]
   > - **Files changed**: [list files]
   > - **Tests added**: [list test files]
   > - **Other candidates considered**: [brief summary]
   > 
   > Please commit these changes and create a PR when ready.
   > Suggested PR title: `[Platform] Brief description of behavior fix`
   > 
   > Use the pr-finalize skill output for the PR body."
   
   **Do NOT run git commands. User handles commit/push/PR creation.**

2. **Update state file** with PR link once user provides it

### If Starting from PR - Write Review

Determine your recommendation based on the Fix phase:

**If PR's fix was selected:**
- Recommend: `✅ APPROVE`
- Justification: PR's approach is correct/optimal

**If an alternative fix was selected:**
- Recommend: `⚠️ REQUEST CHANGES`
- Justification: Suggest the better approach from try-fix Candidate #N
- **Tell user:** "I've applied the alternative fix locally. Please review the changes and commit/push to update the PR."

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

**🚨 MANDATORY: Update state file**

**Update state file**:
1. Change header status to final recommendation
2. Update all phases to `✅ COMPLETE` or `✅ PASSED`
3. Present final result to user

**Before marking ✅ COMPLETE, verify state file contains:**
- [ ] Final recommendation (APPROVE/REQUEST_CHANGES/COMMENT)
- [ ] Summary of findings
- [ ] Key technical insights documented
- [ ] Overall status changed to final recommendation
- [ ] State file saved

---

## Common Mistakes in Post-Gate Phases

- ❌ **Looking at PR's fix before generating ideas** - Generate fix ideas independently first
- ❌ **Re-testing the PR's fix in try-fix** - Gate already validated it; try-fix tests YOUR ideas
- ❌ **Skipping models in Round 1** - All 5 models must run try-fix before cross-pollination
- ❌ **Running try-fix in parallel** - SEQUENTIAL ONLY - they modify same files and use same device
- ❌ **Using explore/glob instead of invoking models** - Cross-pollination requires ACTUAL task agent invocations with each model, not code searches
- ❌ **Assuming "comprehensive coverage" = exhausted** - Only exhausted when all 5 models explicitly say "NO NEW IDEAS"
- ❌ **Not recording cross-pollination responses** - State file must have table showing each model's Round 2 response
- ❌ **Not analyzing why fixes failed** - Record the flawed reasoning to help future attempts
- ❌ **Selecting a failing fix** - Only select from passing candidates
- ❌ **Forgetting to revert between attempts** - Each try-fix must start from broken baseline, end with PR restored
- ❌ **Declaring exhaustion prematurely** - All 5 models must confirm "no new ideas" via actual invocation
- ❌ **Rushing the report** - Take time to write clear justification
- ❌ **Skipping cleanup between attempts** - ALWAYS run `-Restore` + `git checkout HEAD -- .` between try-fix attempts (see Step 1)

---

## Common Errors and Recovery

### skill(try-fix) fails with "ENOENT: no such file or directory"

**Symptom:** `skill(try-fix) Failed to read skill file: Error: ENOENT: no such file or directory, open '.../.github/skills/try-fix/SKILL.md'`

**Root cause:** A previous try-fix attempt failed mid-way and left the working tree in a dirty state. Files may have been modified or deleted by `EstablishBrokenBaseline.ps1` without being restored.

**Fix:** Run cleanup before retrying:
```bash
pwsh .github/scripts/EstablishBrokenBaseline.ps1 -Restore
git checkout HEAD -- .
```

Then retry the try-fix attempt. The skill file should now be accessible.

**Prevention:** Always run the cleanup commands between try-fix attempts (see Step 1).

### try-fix attempt starts with dirty working tree

**Symptom:** `git status` shows modified files before the attempt starts, or the build fails with unexpected errors from files the attempt didn't touch.

**Root cause:** Previous attempt didn't restore its changes (crashed, timed out, or model didn't follow Step 8 restore instructions).

**Fix:** Same as above — run `-Restore` + `git checkout HEAD -- .` to reset to the merged PR state.

### Build errors unrelated to the fix being attempted

**Symptom:** Build fails with errors in files the try-fix attempt didn't modify (e.g., XAML parse errors, unrelated compilation failures).

**Root cause:** Often caused by dirty working tree from a previous attempt. Can also be transient environment issues.

**Fix:**
1. Run cleanup: `pwsh .github/scripts/EstablishBrokenBaseline.ps1 -Restore && git checkout HEAD -- .`
2. Retry the attempt
3. If it fails again with the same unrelated error, skip this attempt and continue with the next model
