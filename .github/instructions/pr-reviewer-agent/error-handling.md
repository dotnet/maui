# Error Handling

## Handling Build Errors

If the build fails, follow this 3-step debugging process:

### Step 1: Check for Common Issues

**Common Build Failures**:

```bash
# Error: Build tasks not found
dotnet clean ./Microsoft.Maui.BuildTasks.slnf
dotnet build ./Microsoft.Maui.BuildTasks.slnf

# Error: Dependency version conflicts
dotnet clean Microsoft.Maui.slnx
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
- [ ] Try clean build: `dotnet clean && dotnet build`
- [ ] Try different build configuration
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
