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

## Phase Completion Protocol (CRITICAL)

**Before changing ANY phase status to ✅ COMPLETE:**

1. **Read the state file section** for the phase you're completing
2. **Find ALL ⏳ PENDING and [PENDING] fields** in that section
3. **Fill in every field** with actual content
4. **Verify no pending markers remain** in your section
5. **Commit the state file** with complete content
6. **Then change status** to ✅ COMPLETE

**Rule:** Status ✅ means "documentation complete", not "I finished thinking about it"

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

### Step 1: Multi-Model try-fix Exploration

Phase 4 uses a **multi-model approach** to maximize fix diversity. Each AI model brings different perspectives and may find solutions others miss.

**⚠️ SEQUENTIAL ONLY**: try-fix runs MUST execute one at a time. They modify the same files and use the same test device. Never run try-fix attempts in parallel.

#### Round 1: Run try-fix with Each Model

Run the `try-fix` skill **5 times sequentially**, once with each model:

| Order | Model | Invocation |
|-------|-------|------------|
| 1 | `claude-sonnet-4.5` | `task` agent with `model: "claude-sonnet-4.5"` |
| 2 | `claude-opus-4.5` | `task` agent with `model: "claude-opus-4.5"` |
| 3 | `gpt-5.2` | `task` agent with `model: "gpt-5.2"` |
| 4 | `gpt-5.2-codex` | `task` agent with `model: "gpt-5.2-codex"` |
| 5 | `gemini-3-pro-preview` | `task` agent with `model: "gemini-3-pro-preview"` |

**For each model**, invoke the try-fix skill:
```
Invoke the try-fix skill for PR #XXXXX:
- Platform: [android/ios]
- TestFilter: "IssueXXXXX"
- state_file: CustomAgentLogsTmp/PRState/pr-XXXXX.md

Generate ONE independent fix idea and test it empirically.
Do NOT look at the PR's fix - generate ideas independently.
```

**Wait for each to complete before starting the next.**

#### Round 2+: Cross-Pollination Loop

After Round 1 completes, share ALL results with ALL 5 models and ask for NEW ideas:

```
┌─────────────────────────────────────────────────────────────┐
│  Cross-Pollination Loop                                     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  LOOP until no new ideas:                                   │
│                                                             │
│    1. Compile summary of ALL try-fix attempts so far:       │
│       - Approach tried                                      │
│       - Pass/Fail result                                    │
│       - Key learnings (why it worked or failed)             │
│                                                             │
│    2. Share summary with ALL 5 models, ask each:            │
│       "Given these results, do you have any NEW fix ideas   │
│        that haven't been tried? If yes, describe briefly."  │
│                                                             │
│    3. For each model with a new idea:                       │
│       → Run try-fix with that model (SEQUENTIAL)            │
│       → Wait for completion before next                     │
│                                                             │
│    4. If ANY new ideas were tested → repeat loop            │
│       If NO new ideas from ANY model → exit loop            │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Exhaustion criteria**: The loop exits when ALL 5 models confirm they have no new ideas to try.

#### try-fix Invocation Details

Each `try-fix` invocation (via task agent):
- Reads state file to learn from prior attempts
- Reverts PR's fix to get broken baseline
- Proposes and implements ONE fix idea
- Runs tests to validate
- Records result with failure analysis
- Reverts changes (restores PR's fix)
- Updates state file with attempt results

See `.github/skills/try-fix/SKILL.md` for full details.

### What try-fix Does (Each Invocation)

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
- Commit the changes

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
- [ ] Cross-pollination completed: All 5 models confirmed "no new ideas"
- [ ] Fix Candidates table has numbered rows for each try-fix attempt
- [ ] Each row has: approach, test result, files changed, notes
- [ ] "Exhausted" field set to Yes (all models confirmed no new ideas)
- [ ] "Selected Fix" populated with reasoning
- [ ] No ⏳ PENDING markers remain in Fix section
- [ ] State file committed

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
- [ ] State file committed

---

## Common Mistakes in Post-Gate Phases

- ❌ **Looking at PR's fix before generating ideas** - Generate fix ideas independently first
- ❌ **Re-testing the PR's fix in try-fix** - Gate already validated it; try-fix tests YOUR ideas
- ❌ **Skipping models in Round 1** - All 5 models must run try-fix before cross-pollination
- ❌ **Running try-fix in parallel** - SEQUENTIAL ONLY - they modify same files and use same device
- ❌ **Stopping before cross-pollination** - Must share results and check for new ideas
- ❌ **Not analyzing why fixes failed** - Record the flawed reasoning to help future attempts
- ❌ **Selecting a failing fix** - Only select from passing candidates
- ❌ **Forgetting to revert between attempts** - Each try-fix must start from broken baseline, end with PR restored
- ❌ **Declaring exhaustion prematurely** - All 5 models must confirm "no new ideas"
- ❌ **Rushing the report** - Take time to write clear justification
