---
description: "5-minute quick start guide for issue resolver agents"
---

# Issue Resolver Quick Start (5 Minutes)

This is your fast-track guide to starting issue resolution. Read this first, reference other files as needed.

---

## ⚡ Pre-Flight Checklist (30 seconds)

```bash
# 1. Check where you are
git branch --show-current

# 2. Get the issue number from user's request
# User should say: "Fix issue #12345" or similar
```

**Output**: You know your starting branch and the issue number to work on.

---

## 📖 Essential Reading (3 minutes)

### Core Workflow (2 minutes)

**Standard workflow**:
```
1. Analyze issue → Read issue + ALL comments
2. Reproduce → Create test in Sandbox app
3. 🛑 CHECKPOINT 1: Show reproduction before investigating
4. Investigate → Root cause analysis
5. 🛑 CHECKPOINT 2: Show fix design before implementing
6. Implement → Write the fix
7. Test → Verify fix + edge cases
8. Write UI tests → TestCases.HostApp + Shared.Tests
9. Submit PR → [Issue-Resolver] Fix #XXXXX
```

**Key principle**: Reproduce first, understand deeply, fix correctly, test thoroughly.

### App Selection (30 seconds)

**For reproduction**: ✅ Use Sandbox app (`src/Controls/samples/Controls.Sample.Sandbox/`)
**For UI tests**: ✅ Use TestCases.HostApp (`src/Controls/tests/TestCases.HostApp/`)

### Mandatory Checkpoints (30 seconds)

**🛑 CHECKPOINT 1 - After Reproduction (MANDATORY)**
- Show: Your reproduction setup and results
- User validates: You've reproduced the correct issue

**🛑 CHECKPOINT 2 - Before Implementation (MANDATORY)**  
- Show: Root cause analysis + proposed fix design
- User validates: Approach is correct before you code

**Why**: Saves hours if you're on wrong track.

---

## 🚀 Start Working (90 seconds)

### Step 1: Fetch Issue Details (30 seconds)

```bash
# Navigate to GitHub issue
ISSUE_NUM=12345  # Replace with actual number
echo "Fetching: https://github.com/dotnet/maui/issues/$ISSUE_NUM"
```

**Read thoroughly**:
- Issue description
- ALL comments (additional details, workarounds, platform info)
- Linked issues/PRs
- Screenshots/code samples

### Step 2: Create Initial Assessment (30 seconds)

Post this to user:

```markdown
## Initial Assessment - Issue #XXXXX

**Issue Summary**: [Brief description of reported problem]

**Affected Platforms**: [iOS/Android/Windows/Mac/All]

**Reproduction Plan**:
- Using Sandbox app
- Will test: [scenario description]
- Platforms to test: [list]

**Next Step**: Creating reproduction in Sandbox app, will show before investigating.

Any concerns about this approach?
```

**⚠️ WAIT for user response before continuing.**

### Step 3: Reference Quick Commands (30 seconds)

See [quick-ref.md](quick-ref.md) for:
- Complete iOS/Android reproduction workflows
- Test code templates
- Instrumentation patterns
- PR description template

---

## 🛑 Mandatory Checkpoints

### Checkpoint 1: After Reproduction (MANDATORY)

After creating reproduction test case, **STOP and show user**:

```markdown
## 🛑 Checkpoint 1: Issue Reproduced

**Platform**: iOS 18.0 (iPhone 15 Pro Simulator)

**Reproduction Steps**:
1. [Exact steps you followed]
2. [...]

**Observed Behavior** (the bug):
```
[Console output or screenshots showing the issue]
```

**Expected Behavior**:
[What should happen instead]

**Evidence**: Issue confirmed, matches reporter's description.

Should I proceed with root cause investigation?
```

**Do NOT investigate without approval.**

### Checkpoint 2: Before Implementation (MANDATORY)

After root cause analysis, **STOP and show user**:

```markdown
## 🛑 Checkpoint 2: Fix Design

**Root Cause**: [Technical explanation of WHY bug exists]

**Files affected**:
- `src/Core/src/Platform/iOS/SomeHandler.cs` - Line 123

**Proposed Solution**:
[High-level explanation of the fix approach]

**Why this approach**:
[Addresses root cause, minimal impact, follows patterns]

**Alternative considered**: [Other approach and why rejected]

**Risks**: [Potential issues and mitigations]

**Edge cases to test**:
1. [Edge case 1]
2. [Edge case 2]

Should I proceed with implementation?
```

**Do NOT implement without approval.**

---

## 📋 Quick Reference Links

**During reproduction**:
- [Quick commands](quick-ref.md#reproduction-workflows)
- [Instrumentation patterns](quick-ref.md#instrumentation-templates)
- [Common Testing Patterns](../common-testing-patterns.md)

**During investigation**:
- [Root cause patterns](solution-development.md#root-cause-analysis)
- [Error handling](error-handling.md)

**During implementation**:
- [Fix patterns](quick-ref.md#common-fix-patterns)
- [Platform-specific code](solution-development.md#platform-specific-considerations)

**Before PR**:
- [UI test requirements](quick-ref.md#ui-test-checklist)
- [PR template](quick-ref.md#pr-description-template)
- [Self-check](pr-submission.md#pr-checklist)

---

## ❌ Top 5 Mistakes to Avoid

1. ❌ **Skipping reproduction** → Implementing fix without confirming issue exists
2. ❌ **No checkpoints** → Wasting hours on wrong approach
3. ❌ **Fixing symptoms** → Not understanding root cause
4. ❌ **Missing UI tests** → Every fix needs automated tests
5. ❌ **Incomplete PR description** → No before/after evidence

---

## ⏱️ Time Budgets

| Issue Type | Expected Time |
|------------|---------------|
| Simple (typo, obvious null check) | 1-2 hours |
| Medium (bug fix, single file) | 3-6 hours |
| Complex (multi-file, architecture) | 6-12 hours |

**Exceeding these times?** Use checkpoints, ask for help.

---

## 📚 When to Read Other Guides

**Full workflow details**: [core-workflow.md](core-workflow.md)  
**Reproduction help**: [reproduction.md](reproduction.md)  
**Implementation patterns**: [solution-development.md](solution-development.md)  
**PR submission**: [pr-submission.md](pr-submission.md)  
**Hit errors**: [error-handling.md](error-handling.md)

---

## ✅ Ready to Start

You now know:
- ✅ Standard workflow with mandatory checkpoints
- ✅ Which apps to use (Sandbox for repro, HostApp for tests)
- ✅ Where to find commands and templates
- ✅ Time expectations and when to ask for help

**Next action**: Fetch the issue from GitHub and create initial assessment.

**Remember**: Show reproduction AND fix design before implementing. Always.
