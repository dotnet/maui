---
description: "Navigation hub for issue resolver instruction files"
---

# Issue Resolver Agent Instructions - Navigation Hub

This README helps you find the right guidance quickly. Files are organized by when you need them during the issue resolution workflow.

---

## 📁 File Organization

### Start Here (ALWAYS)
- **[quick-start.md](quick-start.md)** - 5-minute primer, read this first
- **[quick-ref.md](quick-ref.md)** - Command cheat sheet, keep open while working

### Core Workflow
- **[core-workflow.md](core-workflow.md)** - Complete workflow details, quality standards, common pitfalls
- **[reproduction.md](reproduction.md)** - HostApp test page setup, build/deploy patterns, verification
- **[solution-development.md](solution-development.md)** - Root cause analysis, fix implementation, testing
- **[pr-submission.md](pr-submission.md)** - PR requirements, description template, checklist

### Shared Resources (Cross-Agent)
- **[../shared/platform-workflows.md](../shared/platform-workflows.md)** - Complete iOS/Android/Mac workflows
- **[../shared/error-handling-common.md](../shared/error-handling-common.md)** - Common build/deploy error solutions
- **[../shared/fix-patterns.md](../shared/fix-patterns.md)** - Null checks, platform code, lifecycle patterns
- **[../shared/checkpoints.md](../shared/checkpoints.md)** - Validation checkpoint templates

### Support Files (Issue Resolver-Specific)
- **[error-handling.md](error-handling.md)** - Issue resolver-specific troubleshooting

---

## 🎯 Priority Reading Order

### Before Starting (5 minutes)
1. ✅ [quick-start.md](quick-start.md) - Essential workflow overview
2. ✅ [quick-ref.md](quick-ref.md) - Bookmark for commands

### During Work (Read as needed)
- **Reproducing issue?** → [reproduction.md](reproduction.md)
- **Investigating root cause?** → [solution-development.md](solution-development.md#root-cause-analysis)
- **Hit a build error?** → [error-handling.md](error-handling.md)
- **Creating PR?** → [pr-submission.md](pr-submission.md)

### Deep Dive (When you need details)
- **Want full context?** → [core-workflow.md](core-workflow.md)
- **Complex fix patterns?** → [solution-development.md](solution-development.md)

---

## 🔍 Quick Lookup by Scenario

### "I'm starting a new issue"
1. Read [quick-start.md](quick-start.md)
2. Fetch issue from GitHub
3. Follow workflow in quick-start
4. Reference [quick-ref.md](quick-ref.md) for commands

### "I need to reproduce the issue"
1. Open [quick-ref.md](quick-ref.md#reproduction-workflows)
2. Copy iOS or Android workflow
3. Create test page in TestCases.HostApp
4. If stuck: [reproduction.md](reproduction.md)

### "I'm investigating root cause"
1. Add instrumentation: [quick-ref.md](quick-ref.md#instrumentation-templates)
2. Analyze patterns: [solution-development.md](solution-development.md#root-cause-analysis)
3. Design fix: Use Checkpoint 2 template from [quick-ref.md](quick-ref.md#checkpoint-templates)

### "I'm implementing the fix"
1. Check patterns: [quick-ref.md](quick-ref.md#common-fix-patterns)
2. Platform-specific code: [solution-development.md](solution-development.md#platform-specific-considerations)
3. Test edge cases: [solution-development.md](solution-development.md#edge-case-testing)

### "I need to write UI tests"
1. Follow checklist: [quick-ref.md](quick-ref.md#ui-test-checklist)
2. Use templates from quick-ref
3. Verify test quality: Must fail without fix, pass with fix

### "I'm ready to submit PR"
1. Use template: [quick-ref.md](quick-ref.md#pr-description-template)
2. Complete checklist: [pr-submission.md](pr-submission.md#pr-checklist)
3. Format code: `dotnet format Microsoft.Maui.slnx --no-restore`

### "I hit an error"
1. Check [error-handling.md](error-handling.md)
2. Also check [quick-ref.md](quick-ref.md#common-errors--solutions)
3. If not found: Ask for help

---

## 📊 Decision Trees

### Which app should I use?

```
┌─────────────────────────────────────┐
│  What are you doing?                │
└─────────────────────────────────────┘
           │
           └─→ Issue resolution (reproduction AND UI tests)
               └─→ ✅ ALWAYS use TestCases.HostApp
                   - Create test page: src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml
                   - Write UI test: src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs
                   - Run with: BuildAndRunHostApp.ps1 -Platform [android|ios] -TestFilter "IssueXXXXX"
                   
⚠️  NEVER use Sandbox app for issue resolution
```

### When should I stop and ask for approval?

```
┌─────────────────────────────────────┐
│  Where are you in the workflow?     │
└─────────────────────────────────────┘
           │
           ├─→ Just reproduced issue?
           │   └─→ 🛑 STOP - Checkpoint 1 required
           │
           ├─→ Designed fix approach?
           │   └─→ 🛑 STOP - Checkpoint 2 required
           │
           ├─→ Implementing fix?
           │   └─→ ✅ Continue (already approved)
           │
           └─→ Ready to submit PR?
               └─→ ✅ Continue (run self-check first)
```

### How much time should this take?

```
┌─────────────────────────────────────┐
│  What type of issue?                │
└─────────────────────────────────────┘
           │
           ├─→ Simple (typo, obvious null check)?
           │   └─→ ⏱️ 1-2 hours
           │
           ├─→ Medium (bug fix, single file)?
           │   └─→ ⏱️ 3-6 hours
           │
           ├─→ Complex (multi-file, architecture)?
           │   └─→ ⏱️ 6-12 hours
           │
           └─→ Taking longer than expected?
               └─→ Use checkpoint, ask for help
```

---

## 🎓 Learning Path

### Day 1: Quick Start
- [x] Read [quick-start.md](quick-start.md)
- [x] Bookmark [quick-ref.md](quick-ref.md)
- [x] Try reproducing a simple issue
- [x] Practice using checkpoints

### Week 1: Core Skills
- [x] Read [core-workflow.md](core-workflow.md) fully
- [x] Study [reproduction.md](reproduction.md) patterns
- [x] Learn instrumentation from [quick-ref.md](quick-ref.md)
- [x] Review [error-handling.md](error-handling.md)

### Ongoing: Deep Expertise
- [x] Study [solution-development.md](solution-development.md) patterns
- [x] Master UI test creation
- [x] Learn platform-specific considerations
- [x] Build pattern recognition for common bugs

---

## 📋 Checklists by Phase

### Phase 1: Issue Analysis
- [ ] Read issue description completely
- [ ] Read ALL comments and linked issues
- [ ] Identify affected platforms
- [ ] Note any workarounds mentioned
- [ ] Create initial assessment for user

### Phase 2: Reproduction
- [ ] Create test page in TestCases.HostApp/Issues/IssueXXXXX.xaml
- [ ] Write UI test that reproduces the bug
- [ ] Add instrumentation to capture state
- [ ] Verify issue reproduces (test should fail)
- [ ] 🛑 Show Checkpoint 1 to user
- [ ] Wait for approval before investigating

### Phase 3: Investigation
- [ ] Analyze instrumentation output
- [ ] Identify root cause
- [ ] Design fix approach
- [ ] 🛑 Show Checkpoint 2 to user
- [ ] Wait for approval before implementing

### Phase 4: Implementation
- [ ] Implement fix
- [ ] Test on affected platforms
- [ ] Test edge cases (prioritized: HIGH → MEDIUM → LOW)
- [ ] Verify instrumentation shows correct behavior

### Phase 5: UI Tests
- [ ] Create HostApp test page (with AutomationIds)
- [ ] Create NUnit test
- [ ] Verify test FAILS without fix
- [ ] Verify test PASSES with fix
- [ ] Run test locally on at least one platform

### Phase 6: PR Submission
- [ ] Format code: `dotnet format`
- [ ] Update PublicAPI.Unshipped.txt if needed
- [ ] Create PR with template
- [ ] Include before/after evidence
- [ ] Link issue with "Fixes #XXXXX"
- [ ] Run self-check before posting

---

## ⚡ Quick Links

### Templates
- Checkpoint 1: [quick-ref.md#checkpoint-1](quick-ref.md#checkpoint-1-after-reproduction)
- Checkpoint 2: [quick-ref.md#checkpoint-2](quick-ref.md#checkpoint-2-before-implementation)
- PR description: [quick-ref.md#pr-description-template](quick-ref.md#pr-description-template)
- UI test code: [quick-ref.md#ui-test-checklist](quick-ref.md#ui-test-checklist)

### Commands
- iOS workflow: [quick-ref.md#complete-ios-reproduction-workflow](quick-ref.md#complete-ios-reproduction-workflow)
- Android workflow: [quick-ref.md#complete-android-reproduction-workflow](quick-ref.md#complete-android-reproduction-workflow)
- Instrumentation: [quick-ref.md#instrumentation-templates](quick-ref.md#instrumentation-templates)
- Fix patterns: [quick-ref.md#common-fix-patterns](quick-ref.md#common-fix-patterns)

### Troubleshooting
- Build errors: [quick-ref.md#build-errors](quick-ref.md#build-errors)
- Simulator errors: [quick-ref.md#ios-simulator-errors](quick-ref.md#ios-simulator-errors)
- Test failures: [quick-ref.md#test-failures](quick-ref.md#test-failures)
- Full guide: [error-handling.md](error-handling.md)

---

## 💡 Pro Tips

1. **Read once, reference often**: Quick-start first, then use quick-ref as your daily driver
2. **Checkpoints save hours**: Never skip them
3. **Copy-paste liberally**: All commands and templates are designed for it
4. **Test before PR**: UI tests must fail without fix, pass with fix
5. **Ask when stuck**: Better to ask after 30 minutes than waste 3 hours

---

## 📐 File Statistics

| File | Size | Primary Use | Read When |
|------|------|-------------|-----------|
| quick-start.md | ~150 lines | First-time setup | Every issue (first 5 min) |
| quick-ref.md | ~800 lines | Command reference | During active work |
| core-workflow.md | ~500 lines | Complete details | Deep understanding needed |
| reproduction.md | ~400 lines | Repro patterns | Setting up reproduction |
| solution-development.md | ~450 lines | Fix patterns | Implementing solution |
| pr-submission.md | ~200 lines | PR requirements | Creating PR |
| error-handling.md | ~300 lines | Troubleshooting | Hit an error |

**Total**: ~2,800 lines of guidance

**Efficient path**: Read ~950 lines (quick-start + quick-ref), reference others as needed

---

## 🚀 Get Started

**First time here?**
1. Open [quick-start.md](quick-start.md)
2. Read in 5 minutes
3. Bookmark [quick-ref.md](quick-ref.md)
4. Start your first issue

**Returning?**
- Jump straight to [quick-ref.md](quick-ref.md) for commands
- Check [error-handling.md](error-handling.md) if stuck
- Reference other files as workflow requires

---

## ✅ Success Metrics

You're ready when you can:
- [ ] Start an issue in under 5 minutes
- [ ] Find any command in under 30 seconds
- [ ] Know when to use checkpoints (2 mandatory points)
- [ ] Create UI tests without looking up syntax
- [ ] Troubleshoot common errors independently

**Achievement unlocked**: You're an efficient issue resolver! 🎉
