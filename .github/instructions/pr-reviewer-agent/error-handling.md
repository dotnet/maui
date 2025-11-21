# Error Handling

## 🚫 Common Mistakes & How to Avoid Them

### Mistake #1: Building the Wrong App ⭐ MOST COMMON

**Symptom**: Agent tries to build `TestCases.HostApp` for PR validation

**Why it happens**:
- PR includes test files in HostApp directory
- Agent sees test files and assumes that's what to use
- Instructions not read before planning

**How to avoid**:
1. Read testing-guidelines.md FIRST
2. Remember: Sandbox = PR validation (default)
3. HostApp = writing/debugging UI tests only

**Cost if not avoided**: 15+ minutes wasted building

---

### Mistake #2: Planning Before Reading Instructions

**Symptom**: Agent creates detailed todo list immediately after user request

**Why it happens**:
- Eager to show organization and planning
- Assumes workflow without context
- Wants to appear proactive

**How to avoid**:
1. Resist urge to plan immediately
2. Read ALL instruction files first
3. Create plan based on instructions, not assumptions

**Cost if not avoided**: Plan has to be recreated, shows lack of process discipline

---

### Mistake #3: Not Checking Current Branch

**Symptom**: Planning to "fetch PR" or "checkout branch" when already on correct branch

**Why it happens**:
- Assumes starting from main branch
- Doesn't check current state before planning
- Copy-paste workflow without adaptation

**How to avoid**:
1. ALWAYS run `git branch --show-current` first
2. Adapt workflow to actual situation
3. Don't assume starting state

**Cost if not avoided**: Unnecessary git operations, potential branch confusion

---

### Mistake #4: Building Without User Validation

**Symptom**: Agent modifies Sandbox code and immediately starts building

**Why it happens**:
- Wants to show progress quickly
- Confident in test design
- Doesn't realize build cost

**How to avoid**:
1. Create test code first
2. Show it to user with explanation
3. Wait for approval before building
4. Remember: building takes 10-15 minutes

**Cost if not avoided**: Wasted build time if design is wrong

---

### Mistake #5: Only Testing WITH the Fix

**Symptom**: Agent only tests the PR branch, doesn't test baseline

**Why it happens**:
- Assumes fix must work if tests pass
- Skips comparative analysis
- Doesn't realize need to prove fix actually fixes

**How to avoid**:
1. Test WITHOUT fix first (baseline)
2. Document baseline behavior
3. Test WITH fix second
4. Compare baseline vs improved
5. Prove the fix actually fixes the issue

**Cost if not avoided**: Can't prove fix works, subjective review

---

### Mistake #6: Surface-Level Code Review

**Symptom**: Describing WHAT changed without explaining WHY

**Example bad review**:
- "This PR adds an event"
- "The code was modified"
- "Looks good to me"

**Why it happens**:
- Focused on diff, not root cause
- Not understanding platform behavior
- Quick review over thorough review

**How to avoid**:
1. Understand the root cause of the issue
2. Explain WHY each change was needed
3. Identify edge cases
4. Consider platform-specific implications

**Cost if not avoided**: Missed issues, shallow review, no real validation

---

## ✅ Checklist: Am I Doing This Right?

Before proceeding with each phase, check:

### ☑️ Initial Phase:
- [ ] Read ALL instruction files before creating plan
- [ ] Checked current branch state
- [ ] Fetched and understood PR details
- [ ] Created plan based on instructions, not assumptions

### ☑️ Planning Phase:
- [ ] Using Sandbox app (unless writing UI tests)
- [ ] Planned to test WITHOUT and WITH fix
- [ ] Included validation checkpoint before building
- [ ] Test design will provide measurable data

### ☑️ Implementation Phase:
- [ ] Created comprehensive test scenarios
- [ ] Added instrumentation for measurements
- [ ] Showed test code to user BEFORE building
- [ ] Got user approval to proceed

### ☑️ Testing Phase:
- [ ] Tested WITHOUT fix (baseline)
- [ ] Captured baseline data
- [ ] Tested WITH fix
- [ ] Captured improved data
- [ ] Compared baseline vs improved

### ☑️ Review Phase:
- [ ] Explained WHY fix works, not just WHAT changed
- [ ] Identified potential edge cases
- [ ] Provided objective data, not subjective opinion
- [ ] Made clear recommendation (approve/request changes)

---

## Handling Build Errors

If the build fails, follow this 3-step debugging process:

### Step 1: Check for Common Issues

**Common Build Failures**:

```bash
# Error: Build tasks not found
dotnet build ./Microsoft.Maui.BuildTasks.slnf

# Error: Dependency version conflicts
rm -rf bin/ obj/
dotnet restore Microsoft.Maui.slnx --force

# Error: Android SDK not found
export ANDROID_HOME=/path/to/android-sdk
# Or run: android  # Opens Android SDK Manager
```

### Step 2: Report the Error

**Template for Build Error Report**:

```markdown
## Build Error Encountered

**Command that failed**:
```bash
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-ios
```

**Error message**:
```
[Paste relevant error output here]
```

**What I've tried**:
1. [First attempt to fix]
2. [Second attempt to fix]

**Next steps**:
- [ ] Need help understanding this error
```

### Step 3: Decide Next Action

**Decision tree**:

1. **Error seems related to PR changes?**
   - 🔴 Flag this in review: "PR introduces build error on [platform]"
   - Show the error to user
   - Suggest fix if obvious

2. **Error seems environmental (missing SDK, wrong .NET version)?**
   - 🔧 Try to fix locally (install SDK, update .NET)
   - If can't fix, note in review: "Unable to test on [platform] due to [issue]"

3. **Error is unclear/unexpected?**
   - 📝 Document the error
   - Ask user for guidance

**What NOT to do:**
- ❌ Skip testing and assume it works
- ❌ Make up test results
- ❌ Silently ignore errors
- ❌ Spend 30+ minutes debugging environmental issues

## Handling Unexpected Test Results

Sometimes the test runs but results don't match expectations.

**Situations requiring pause:**

1. **Test shows no difference WITH and WITHOUT PR**
   - Might be testing wrong thing
   - Might need different edge case
   - **Action**: Use validation checkpoint to verify test setup

2. **Test shows behavior got WORSE with PR**
   - PR may introduce regression
   - May be expected tradeoff
   - **Action**: Flag in review, ask user if this is expected

3. **Measurements seem wrong (all zeros, negative values, extreme values)**
   - Layout may not have completed before measurement
   - May need longer delay or different measurement timing
   - **Action**: Add delay, remeasure, or ask for guidance

4. **Crash or exception during test**
   - PR may have introduced bug
   - May be pre-existing issue
   - **Action**: Test on baseline to determine if PR-related

**Template for Unexpected Result Report**:

```markdown
## Unexpected Test Result

**Test Scenario**: [What I tested]

**Expected**: [What should have happened]

**Actual**: [What actually happened]

**WITH PR Changes**:
```
[Console output or measurements]
```

**WITHOUT PR Changes**:
```
[Baseline console output or measurements]
```

**Possible explanations**:
1. [Hypothesis 1]
2. [Hypothesis 2]

**Need guidance on**:
- Is this expected behavior?
- Should I test differently?
- Is this a PR regression?
```

**Why this matters**:

❌ **Bad approach**: Silently ignore unexpected results and approve PR
```
Agent: "I tested it and everything works!" [but actually got confusing results]
```

✅ **Good approach**: Surface unexpected results for discussion
```
Agent: "I tested it but got unexpected results. Here's what I found: [data]. 
       This could mean [explanation]. Should I investigate further or is this expected?"
```

**Rationale**: Better to pause and clarify than to approve a PR with hidden issues.

## Handling Merge Conflicts

When fetching the PR results in merge conflicts:

**Template**:

```markdown
## Unable to Test: Merge Conflicts

**Issue**: PR #XXXXX has merge conflicts with current branch

**Conflicts in**:
```bash
# Output of git status
[List of conflicting files]
```

**Recommendation**:
- PR needs to be rebased on latest main/net10.0
- Unable to test until conflicts are resolved
- Author should run: `git rebase origin/main` (or origin/net10.0)

**Review Status**: ⏸️ **Paused pending conflict resolution**
```

**What NOT to do**:
- ❌ Try to manually resolve conflicts (not reviewer's job)
- ❌ Test on outdated base branch
- ❌ Approve PR with unresolved conflicts
