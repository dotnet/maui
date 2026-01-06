# PR Agent: Post-Gate Phases (4-7)

**⚠️ PREREQUISITE: Only read this file after 🚦 Gate shows `✅ PASSED` in your state file.**

If Gate is not passed, go back to `.github/agents/pr.md` and complete phases 1-3 first.

---

## Workflow Depends on Starting Point

**Starting from a PR (fix exists):**
- Phase 4: Research root cause independently
- Phase 5: Compare your approach vs PR's fix
- Phase 6: Regression testing
- Phase 7: Report with APPROVE/REQUEST CHANGES

**Starting from an Issue (no fix yet):**
- Phase 4: Research root cause, **implement fix**
- Phase 5: Skip comparison (no PR to compare)
- Phase 6: Verify fix with full test verification
- Phase 7: Create PR

---

## 🔍 ANALYSIS: Independent Analysis (Phase 4)

> **SCOPE**: Research root cause, design your own fix approach, understand the problem deeply.

**⚠️ Gate Check:** Verify 🚦 Gate is `✅ PASSED` in your state file before proceeding.

### Step 1: Review Pre-Flight Findings

Before analyzing code, review your `.github/agent-pr-session/pr-XXXXX.md`:
- What is the user-reported symptom? (from linked issue)
- What are the key disagreements? (from inline comments, if PR exists)
- What edge cases were mentioned? (from discussion)

### Step 2: Research the Root Cause

```bash
# Find relevant commits to the affected files
git log --oneline --all -20 -- path/to/affected/File.cs

# Look at the breaking commit (if regression)
git show COMMIT_SHA --stat

# Compare implementations
git show COMMIT_SHA:path/to/File.cs | head -100
```

### Step 3: Design Your Own Fix

Determine:
- What is the **minimal** fix?
- What are **alternative approaches**?
- What **edge cases** should be handled?

### Step 4: Implement Fix

**If starting from an Issue (no PR):**
Implement your fix now. This is the main deliverable.

```bash
# Implement the fix in the appropriate source files
# Then verify tests now PASS
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "IssueXXXXX"
```

**If starting from a PR:**
Optionally implement your alternative to compare approaches.

```bash
# Save PR's fix
git stash

# Implement your fix
# Run the same tests
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "IssueXXXXX"

# Restore PR's fix
git stash pop
```

### Complete 🔍 Analysis

**Update state file**:
1. Check off completed items in the checklist
2. Fill in **Root Cause** and **My Approach**
3. Change 🔍 Analysis status to `✅ COMPLETE`
4. Change ⚖️ Compare status to `▶️ IN PROGRESS` (or `⏭️ SKIPPED` if no PR)

---

## ⚖️ COMPARE: Compare Approaches (Phase 5)

> **SCOPE**: Compare PR's fix vs your alternative, recommend the better approach.

**⚠️ Gate Check:** Verify 🔍 Analysis is `✅ COMPLETE` before proceeding.

### If Starting from Issue (No PR)

**Skip this phase** - there's no PR fix to compare against.

Mark as `⏭️ SKIPPED` in state file and proceed to Phase 6 (Regression).

### If Starting from PR

Compare PR's Fix vs Your Alternative:

| Approach | Test Result | Lines Changed | Complexity | Recommendation |
|----------|-------------|---------------|------------|----------------|
| PR's fix | ✅/❌ | ? | Low/Med/High | |
| Your alternative | ✅/❌ | ? | Low/Med/High | |

### Assess Each Approach

For PR's fix:
- Is this the **minimal** fix?
- Are there **edge cases** that might break?
- Could this cause **regressions**?

For your alternative:
- Does it solve the same problem?
- Is it simpler or more robust?
- Any trade-offs?

### Complete ⚖️ Compare

**Update state file**:
1. Fill in comparison table with findings (or mark `⏭️ SKIPPED` if no PR)
2. Fill in **Recommendation** with your assessment
3. Change ⚖️ Compare status to `✅ COMPLETE` or `⏭️ SKIPPED`
4. Change 🔬 Regression status to `▶️ IN PROGRESS`

---

## 🔬 REGRESSION: Regression Testing (Phase 6)

> **SCOPE**: Verify edge cases, investigate disagreements, check for potential regressions.

**⚠️ Gate Check:** Verify ⚖️ Compare is `✅ COMPLETE` or `⏭️ SKIPPED` before proceeding.

### If Starting from Issue (No PR) - Verify Fix Works

Run the full verification to confirm your fix works (the script auto-detects mode when fix files are present):

```bash
# Commit your fix first
git add -A && git commit -m "Fix: Description of the fix"

# Run full verification - should FAIL without fix, PASS with fix
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform ios -TestFilter "IssueXXXXX"
```

### Step 1: Check Edge Cases from Pre-Flight

Go through each edge case identified during pre-flight (from `.github/agent-pr-session/pr-XXXXX.md`):

```markdown
### Edge Cases from Discussion
- [ ] [edge case 1] - Tested: [result]
- [ ] [edge case 2] - Tested: [result]
```

### Step 2: Investigate Disagreements (if PR exists)

For each disagreement between reviewers and author (from pre-flight):
1. Understand both positions
2. Test to determine who is correct
3. Document your finding in state file

### Step 3: Verify Author's Uncertain Areas (if PR exists)

If author expressed uncertainty (from pre-flight), investigate and provide guidance.

### Step 4: Check Code Paths

1. **Code paths affected by the fix**
   - What other scenarios use this code?
   - Are there conditional branches that might behave differently?

2. **Common regression patterns**

| Fix Pattern | Potential Regression |
|-------------|---------------------|
| `== ConstantValue` | Dynamic values won't match |
| Platform-specific fix | Other platforms affected? |

3. **Instrument code if needed** - Add `Debug.WriteLine` and grep device logs.

### Complete 🔬 Regression

**Update state file**:
1. Check off edge cases with results
2. Document disagreement findings
3. Change 🔬 Regression status to `✅ COMPLETE`
4. Change 📋 Report status to `▶️ IN PROGRESS`

---

## 📋 REPORT: Final Report (Phase 7)

> **SCOPE**: Write final recommendation with justification, or create PR if starting from issue.

**⚠️ Gate Check:** Verify ALL phases 1-6 are `✅ COMPLETE`, `✅ PASSED`, or `⏭️ SKIPPED` before proceeding.

### If Starting from Issue (No PR) - Create PR

1. **Ensure all changes are committed**:
   ```bash
   git add -A
   git commit -m "Fix #XXXXX: [Description of fix]"
   ```

2. **Create a feature branch** (if not already on one):
   ```bash
   git checkout -b fix/issue-XXXXX
   ```

3. **Push and create PR**:
   ```bash
   git push -u origin fix/issue-XXXXX
   gh pr create --title "Fix #XXXXX: [Title]" --body "Fixes #XXXXX

   ## Description
   [Brief description of the fix]

   ## Root Cause
   [What was causing the issue]

   ## Changes
   - [List of changes made]

   ## Testing
   - Added UI tests: Issue33356.cs
   - Tests verify [what the tests check]
   "
   ```

4. **Update state file** with PR link

### If Starting from PR - Write Review

Update the state file to its final format. The final structure should be:

1. **Header** with date, issue link, PR link - always visible
2. **Final Recommendation** - `✅ APPROVE` or `⚠️ REQUEST CHANGES`
3. **Phase status table** - all phases marked complete
4. **Collapsible sections** for each phase's details
5. **Justification** bullet points - always visible

### Complete 📋 Report

**Update state file**:
1. Change header status from `⏳ Status: IN PROGRESS` to `✅ Final Recommendation: APPROVE` or `✅ PR CREATED`
2. Update the status table to show all phases as `✅ PASSED`, `✅ COMPLETE`, or `⏭️ SKIPPED`
3. Fill in justification bullet points or PR link
4. Present final result to user

---

## State File: Post-Gate Sections

After Gate passes, add these sections to your state file if not already present:

```markdown
<details>
<summary><strong>🔍 Analysis</strong></summary>

**Status**: ▶️ IN PROGRESS

- [ ] Reviewed pre-flight findings
- [ ] Researched git history for root cause
- [ ] Formed independent opinion on fix approach

**Root Cause:** [PENDING]

**Alternative Approaches Considered:**
| Alternative | Location | Why NOT to use |
|-------------|----------|----------------|

**My Approach:** [PENDING]

</details>

<details>
<summary><strong>⚖️ Compare</strong></summary>

**Status**: ⏳ PENDING

| Approach | Test Result | Lines Changed | Complexity | Recommendation |
|----------|-------------|---------------|------------|----------------|
| PR's fix | | | | |
| My approach | | | | |

**Recommendation:** [PENDING]

</details>

<details>
<summary><strong>🔬 Regression</strong></summary>

**Status**: ⏳ PENDING

**Edge Cases Verified:**
- [ ] [Edge case 1]
- [ ] [Edge case 2]

**Disagreements Investigated:**
- [Findings]

**Potential Regressions:** [PENDING]

</details>
```

---

## Common Mistakes in Post-Gate Phases

- ❌ **Looking at PR diff before forming your own opinion** - Research the bug independently first
- ❌ **Skipping edge case verification** - Always check edge cases from pre-flight
- ❌ **Not documenting your alternative approach** - Even if PR's fix is better, document what you considered
- ❌ **Rushing the report** - Take time to write clear justification
