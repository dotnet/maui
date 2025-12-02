---
description: "Navigation hub for PR reviewer agent instructions"
---

# PR Reviewer Agent - Instruction Files

This directory contains all instruction files for the PR reviewer agent. Files are organized by when you need them during the review process.

---

## 🚀 Start Here (Read First)

| File | When | Time | Purpose |
|------|------|------|---------|
| **[quick-start.md](quick-start.md)** | **Start of every review** | 5 min | Fast-track guide with essential workflow |
| **[quick-ref.md](quick-ref.md)** | During work | As needed | Command cheat sheet and templates |

**New to PR reviewing?** Read quick-start.md first. It tells you everything you need to start working.

**Already familiar?** Use quick-ref.md for copy-paste commands and templates.

---

## 📖 Core Workflow (Reference During Work)

These files contain the complete workflow details. Reference them as needed:

| File | When to Read | What It Covers |
|------|--------------|----------------|
| [testing-guidelines.md](testing-guidelines.md) | When unclear about workflow | Fetch PR, checkpoints, test WITH/WITHOUT, time budgets, edge cases |
| [core-guidelines.md](core-guidelines.md) | For deep analysis guidance | Test vs review philosophy, deep code analysis, checkpoint rationale |

---

## 🤝 Shared Resources (Cross-Agent)

Resources shared with issue resolver agent:

| File | When to Read | What It Covers |
|------|--------------|----------------|
| [../shared/platform-workflows.md](../shared/platform-workflows.md) | Need complete iOS/Android workflows | UDID extraction, build, deploy, log monitoring |
| [../shared/error-handling-common.md](../shared/error-handling-common.md) | Build fails, app crashes | Common build/deploy error solutions |
| [../shared/fix-patterns.md](../shared/fix-patterns.md) | Understanding fixes | Null checks, platform code, lifecycle patterns |
| [../shared/checkpoints.md](../shared/checkpoints.md) | Creating checkpoints | Validation checkpoint templates |

## 🛠️ Specialized Techniques (Context-Specific)

Read these when working with specific controls or scenarios:

| File | When to Read | What It Covers |
|------|--------------|----------------|
| [collectionview-handler-detection.md](collectionview-handler-detection.md) | PR affects CollectionView/CarouselView | Handler detection algorithm, MauiProgram.cs configuration |
| [../safearea-testing.md](../safearea-testing.md) | PR modifies SafeArea code | Measure children not parents, gap calculations, instrumentation |
| [../uitests.instructions.md](../uitests.instructions.md) | PR adds/modifies UI tests | UI test file structure, Appium patterns |
| [../instrumentation.md](../instrumentation.md) | Need detailed patterns | Complete instrumentation guide with advanced techniques |

---

## 🚨 Problem Solving (When Things Go Wrong)

| File | When to Read | What It Covers |
|------|--------------|----------------|
| [error-handling.md](error-handling.md) | Build fails, tests fail, unexpected results | Common errors, recovery patterns, decision trees |
| [checkpoint-resume.md](checkpoint-resume.md) | Can't complete testing due to environment | Checkpoint format, resume process, delegation |

---

## 📝 Final Review (Before Posting)

| File | When to Read | What It Covers |
|------|--------------|----------------|
| [output-format.md](output-format.md) | Writing final review | Review structure, redundancy elimination, self-check |

**⚠️ CRITICAL**: Always read output-format.md before posting your review to eliminate redundancy.

---

## 📊 File Organization by Priority

### Priority 1: Start Every Review With These
1. **quick-start.md** - Your 5-minute primer
2. **quick-ref.md** - Commands and templates

### Priority 2: Reference During Work
3. **testing-guidelines.md** - Complete workflow
4. [DEPRECATED - file deleted] Test code creation
5. **error-handling.md** - When problems occur

### Priority 3: Context-Specific (Read If Applicable)
6. **collectionview-handler-detection.md** - CollectionView/CarouselView only
7. **safearea-testing.md** - SafeArea changes only
8. **uitests.instructions.md** - UI test files only

### Priority 4: Before Finishing
9. **output-format.md** - Eliminate redundancy, self-check

### Priority 5: Deep Dive (Optional)
10. **core-guidelines.md** - Philosophy and rationale
11. **instrumentation.md** - Advanced patterns
12. **checkpoint-resume.md** - Environment limitations

---

## 🎯 Quick Decision Trees

### Which Approach Should I Use?

```
User says: "Review PR" or "Test fix"
→ Use HostApp with UI tests ✅
  (Create test page in TestCases.HostApp)
  (Create NUnit test in TestCases.Shared.Tests)
  (Run BuildAndRunHostApp.ps1)

User says: "Write UI tests only" or "Debug UI test"
→ Delegate to uitest-coding-agent ✅
```

See: quick-start.md or testing-guidelines.md

### Do I Need to Configure Handlers?

```
PR changes files in:
├─ "Handlers/Items/" (NOT Items2) → Configure CollectionViewHandler
├─ "Handlers/Items2/" → Configure CollectionViewHandler2
└─ Other paths → No handler config needed
```

See: collectionview-handler-detection.md

### What Checkpoint Do I Need?

```
About to build?
→ MANDATORY: Show test code first (Checkpoint 1)

Done testing?
→ RECOMMENDED: Show raw data (Checkpoint 2)
```

See: testing-guidelines.md#mandatory-workflow-with-checkpoints

---

## � Quick Lookup by Scenario

### "I'm starting a new PR review"
1. Read [quick-start.md](quick-start.md) (5 min)
2. Fetch PR details from GitHub
3. Analyze code changes
4. Reference [quick-ref.md](quick-ref.md) for commands

### "I need to test a layout/UI change"
1. Check if SafeArea-related: [safearea-testing.md](../safearea-testing.md)
2. Check if CollectionView/CarouselView: [collectionview-handler-detection.md](collectionview-handler-detection.md)
3. Create HostApp UI tests: See [UITesting-Guide.md](../../../docs/UITesting-Guide.md)
4. Create test code with instrumentation
5. 🛑 Show Checkpoint 1 before building

### "I need to test WITH and WITHOUT PR changes"
1. Commands in [quick-ref.md](quick-ref.md#test-with-and-without-changes)
2. Baseline test (checkout main branch files)
3. PR test (restore PR files)
4. Compare results objectively

### "I'm hitting build/deployment errors"
1. Check [error-handling.md](error-handling.md) first
2. Also check [quick-ref.md](quick-ref.md#common-errors)
3. Check [../shared/error-handling-common.md](../shared/error-handling-common.md)
4. Use error-specific sections (iOS simulator, Android emulator, etc.)

### "I need to write or validate UI tests"
1. Follow [uitests.instructions.md](../uitests.instructions.md)
2. Use HostApp with UI tests
3. Templates in [quick-ref.md](quick-ref.md#ui-test-template)
4. Verify test FAILS without fix, PASSES with fix

### "I'm ready to post my review"
1. Read [output-format.md](output-format.md) (5 min)
2. Run self-check at end of output-format.md
3. Create `Review_Feedback_Issue_XXXXX.md` file
4. Include test evidence and measurements
5. Post review

### "I can't complete testing (environment issues)"
1. Read [checkpoint-resume.md](checkpoint-resume.md)
2. Use manual verification checkpoint template
3. Document what you attempted
4. Explain what manual verification is needed
5. DO NOT skip testing without checkpoint

---

## �📏 Typical Review Timeline

| PR Type | Time | Files to Read |
|---------|------|---------------|
| Simple | 30-45 min | quick-start + quick-ref |
| Medium | 1-2 hours | quick-start + testing-guidelines + quick-ref |
| Complex | 2-4 hours | All core files + specialized guides |

---

## 🔄 Review Workflow Summary

```
1. Read quick-start.md (5 min)
2. Fetch PR, analyze code (5-10 min)
3. Create test code using templates from quick-ref.md
4. 🛑 CHECKPOINT: Show test code, get approval
5. Build & deploy using commands from quick-ref.md
6. Test WITHOUT fix (baseline)
7. Test WITH fix
8. Compare results
9. Read output-format.md (5 min)
10. Write review, run self-check
11. Post review
```

---

## 🆘 Quick Help

**Lost?** Start with [quick-start.md](quick-start.md)

**Need commands?** Check [quick-ref.md](quick-ref.md)

**Hit an error?** See [error-handling.md](error-handling.md)

**Before posting review?** Read [output-format.md](output-format.md)

**Want to understand why?** Read [core-guidelines.md](core-guidelines.md)

---

## 📚 External References

Files outside this directory that you may need:

- **[../.github/copilot-instructions.md](../../copilot-instructions.md)** - General .NET MAUI coding standards
- **[../common-testing-patterns.md](../common-testing-patterns.md)** - Reusable command patterns
- **[../instrumentation.md](../instrumentation.md)** - Complete instrumentation guide
- **[../safearea-testing.md](../safearea-testing.md)** - SafeArea testing patterns
- **[../uitests.instructions.md](../uitests.instructions.md)** - UI test creation guide
- **[../../docs/UITesting-Guide.md](../../../docs/UITesting-Guide.md)** - UI testing reference

---

**Last Updated**: 2025-11-21
