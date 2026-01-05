# PR Agent: Post-Gate Phases (4-7)

**⚠️ PREREQUISITE: Only read this file after 🚦 Gate shows `✅ PASSED` in your state file.**

If Gate is not passed, go back to `.github/agents/pr.md` and complete phases 1-3 first.

---

## 🔍 ANALYSIS: Independent Analysis (Phase 4)

> **SCOPE**: Research root cause, design your own fix approach, understand the problem deeply.

**⚠️ Gate Check:** Verify 🚦 Gate is `✅ PASSED` in your state file before proceeding.

### Step 1: Review Pre-Flight Findings

Before analyzing code, review your `.github/agent-pr-session/pr-XXXXX.md`:
- What is the user-reported symptom? (from linked issue)
- What are the key disagreements? (from inline comments)
- What edge cases were mentioned? (from discussion)

### Step 2: Research the Root Cause

```bash
# Find relevant commits to the affected files
git log --oneline --all -20 -- path/to/affected/File.cs

# Look at the breaking commit
git show COMMIT_SHA --stat

# Compare implementations
git show COMMIT_SHA:path/to/File.cs | head -100
```

### Step 3: Design Your Own Fix

Before looking at PR's diff, determine:
- What is the **minimal** fix?
- What are **alternative approaches**?
- What **edge cases** should be handled?

### Step 4: Implement and Test Your Alternative (Optional)

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
4. Change ⚖️ Compare status to `▶️ IN PROGRESS`

---

## ⚖️ COMPARE: Compare Approaches (Phase 5)

> **SCOPE**: Compare PR's fix vs your alternative, recommend the better approach.

**⚠️ Gate Check:** Verify 🔍 Analysis is `✅ COMPLETE` before proceeding.

### Compare PR's Fix vs Your Alternative

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
1. Fill in comparison table with findings
2. Fill in **Recommendation** with your assessment
3. Change ⚖️ Compare status to `✅ COMPLETE`
4. Change 🔬 Regression status to `▶️ IN PROGRESS`

---

## 🔬 REGRESSION: Regression Testing (Phase 6)

> **SCOPE**: Verify edge cases, investigate disagreements, check for potential regressions.

**⚠️ Gate Check:** Verify ⚖️ Compare is `✅ COMPLETE` before proceeding.

### Step 1: Check Edge Cases from Pre-Flight

Go through each edge case identified during pre-flight (from `.github/agent-pr-session/pr-XXXXX.md`):

```markdown
### Edge Cases from Discussion
- [ ] [edge case 1] - Tested: [result]
- [ ] [edge case 2] - Tested: [result]
```

### Step 2: Investigate Disagreements

For each disagreement between reviewers and author (from pre-flight):
1. Understand both positions
2. Test to determine who is correct
3. Document your finding in state file

### Step 3: Verify Author's Uncertain Areas

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

> **SCOPE**: Write final recommendation with justification.

**⚠️ Gate Check:** Verify ALL phases 1-6 are `✅ COMPLETE` or `✅ PASSED` before proceeding.

### Write Final Report

Update the state file to its final format. The final structure should be:

1. **Header** with date, issue link, PR link - always visible
2. **Final Recommendation** - `✅ APPROVE` or `⚠️ REQUEST CHANGES`
3. **Phase status table** - all phases marked complete
4. **Collapsible sections** for each phase's details
5. **Justification** bullet points - always visible

### Complete 📋 Report

**Update state file**:
1. Change header status from `⏳ Status: IN PROGRESS` to `✅ Final Recommendation: APPROVE` or `⚠️ Final Recommendation: REQUEST CHANGES`
2. Update the status table to show all phases as `✅ PASSED` or `✅ COMPLETE`
3. Fill in justification bullet points
4. Review is complete - present final recommendation to user

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
