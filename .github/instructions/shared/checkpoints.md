---
description: "Checkpoint templates for PR reviewer and issue resolver workflows"
---

# Checkpoint Templates

This document provides checkpoint templates used by both PR reviewer and issue resolver agents. Checkpoints ensure quality gates and user validation before expensive operations.

---

## Table of Contents

- [PR Reviewer Checkpoints](#pr-reviewer-checkpoints)
- [Issue Resolver Checkpoints](#issue-resolver-checkpoints)
- [Shared Checkpoint Principles](#shared-checkpoint-principles)

---

## PR Reviewer Checkpoints

### Checkpoint: Before Building (MANDATORY)

**When**: After creating test code, before starting build

**Purpose**: Validate test approach and get approval for 10-15 minute build

**Template**:

```markdown
## 🛑 Validation Checkpoint - Before Building

**Test code created** (Sandbox app modified):

### XAML
```xml
[Show relevant snippet of test UI you created]

Example:
<ContentView x:Name="TestElement" 
             FlowDirection="RightToLeft"
             Padding="20">
    <Label Text="Test Content" x:Name="ContentLabel"/>
</ContentView>
```

### Code-Behind
```csharp
[Show instrumentation and measurement code]

Example:
private void OnLoaded(object sender, EventArgs e)
{
    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
    {
        Console.WriteLine($"=== TEST OUTPUT: {TestContext} ===");
        Console.WriteLine($"Element Padding: {TestElement.Padding}");
        Console.WriteLine($"Content Position: {ContentLabel.X}");
        Console.WriteLine("=== END TEST OUTPUT ===");
    });
}
```

### What I'm Measuring
[Clear explanation of what data you'll capture]

Example:
- Content position relative to parent
- Padding values in RTL vs LTR
- Whether content is on correct side

### Expected Results

**WITHOUT PR** (baseline):
[What you expect to see in current code]

Example:
- RTL padding: Left=20, Right=0 (WRONG - should be reversed)
- Content starts at X=0 (on left side, should be right)

**WITH PR** (improved):
[How PR should change the behavior]

Example:
- RTL padding: Left=0, Right=20 (CORRECT)
- Content starts at X=(width-20) (on right side as expected)

### Build Time
⏱️ ~10-15 minutes per platform

### Approval Request
Should I proceed with building and testing?
```

**Wait for user response before building**

---

### Checkpoint: After Testing (RECOMMENDED)

**When**: After capturing test data, before writing review

**Purpose**: Show raw data to user for validation

**Template**:

```markdown
## 📊 Test Results - Raw Data

### Test Environment
- Platform: [iOS 18.0 / Android 14 / etc.]
- Device: [iPhone Xs Simulator / Pixel 9 Emulator]
- App: [Sandbox / HostApp]

### WITHOUT PR Changes (Baseline)
```
[Paste console output or measurements]

Example:
=== TEST OUTPUT: OnLoaded ===
Element Padding: Left=20, Top=0, Right=0, Bottom=0
Content Position: X=0, Y=0
FlowDirection: RightToLeft
=== END TEST OUTPUT ===
```

### WITH PR Changes (Fixed)
```
[Paste console output or measurements]

Example:
=== TEST OUTPUT: OnLoaded ===
Element Padding: Left=0, Top=0, Right=20, Bottom=0
Content Position: X=355, Y=0
FlowDirection: RightToLeft
=== END TEST OUTPUT ===
```

### Analysis
[Brief interpretation of the data]

Example:
✅ Padding correctly reversed in RTL mode
✅ Content positioned on right side as expected
✅ PR successfully fixes the reported issue

Ready to write final review based on this data?
```

---

## Issue Resolver Checkpoints

### Checkpoint 1: After Reproduction (MANDATORY)

**When**: After successfully reproducing the issue, before investigating fix

**Purpose**: Validate reproduction and get approval to proceed with fix

**Template**:

```markdown
## 🛑 Checkpoint 1: Reproduction Confirmed

### Issue Reproduced
✅ Successfully reproduced the reported behavior

### Platform Tested
- [iOS 18.0 Simulator / Android 14 Emulator / etc.]

### Reproduction Evidence
```
[Console output or screenshots showing the bug]

Example:
=== REPRO: Button Click ===
ERROR: NullReferenceException in UpdateLayout
Stack: at CollectionView.ArrangeChildren() line 123
Handler: null (should not be null here)
=== END REPRO ===
```

### Root Cause Hypothesis
[Initial analysis of why this is happening]

Example:
The Handler is being accessed before it's connected to the platform view.
This happens when FlowDirection changes during initialization, triggering
layout before OnHandlerChanged completes.

### Proposed Investigation
[What you'll examine to confirm root cause]

Example:
1. Add instrumentation to OnHandlerChanged to track timing
2. Check FlowDirection property change handler order
3. Verify Handler initialization sequence on affected platforms

### Time Estimate
⏱️ [Simple/Medium/Complex] - [1-2hr / 3-6hr / 6-12hr]

### Approval Request
Should I proceed with investigating the fix?
```

**Wait for user response before continuing**

---

### Checkpoint 2: Before Implementation (MANDATORY)

**When**: After identifying root cause, before implementing fix

**Purpose**: Validate fix approach and get approval for implementation

**Template**:

```markdown
## 🛑 Checkpoint 2: Fix Approach Validation

### Root Cause Confirmed
[Detailed explanation of why the bug occurs]

Example:
The issue occurs because:
1. FlowDirection property changes during control initialization
2. This triggers UpdateLayout() before Handler is connected
3. UpdateLayout accesses Handler.PlatformView without null check
4. Result: NullReferenceException on iOS and Android

Evidence:
```
[Instrumentation output confirming root cause]

Example:
[LIFECYCLE] FlowDirection changed to RightToLeft
[LIFECYCLE] UpdateLayout called
[ERROR] Handler is null in UpdateLayout
[LIFECYCLE] OnHandlerChanged called (too late)
```
```

### Proposed Solution
[Detailed fix approach]

Example:
**Option 1** (Recommended): Add null check in UpdateLayout
- Check if Handler exists before accessing PlatformView
- If null, defer layout update until Handler connects
- Platform-specific: Apply to iOS and Android handlers

**Option 2** (Alternative): Defer property change handlers
- Move FlowDirection handler registration until after Handler connects
- More invasive change, affects control lifecycle

**Recommendation**: Option 1 is safer and more targeted

### Implementation Plan
[Step-by-step what you'll change]

Example:
1. Add null check in CollectionView.UpdateLayout()
2. Store pending layout flag if Handler is null
3. Apply pending layout in OnHandlerChanged
4. Add same pattern to iOS and Android handlers
5. Test on both platforms to confirm fix

### Files to Modify
- `src/Controls/src/Core/CollectionView.cs` - Add null check
- `src/Core/src/Handlers/Items/CollectionViewHandler.ios.cs` - iOS handler
- `src/Core/src/Handlers/Items/CollectionViewHandler.android.cs` - Android handler

### Edge Cases to Test
🔴 HIGH: Handler is null when property changes
🔴 HIGH: Multiple property changes before Handler connects
🟡 MEDIUM: Handler disconnects while layout pending
⚫ LOW: Rapid property changes

### Time Estimate
⏱️ [2-3 hours] for implementation + testing

### Approval Request
Does this approach look correct? Should I proceed with implementation?
```

**Wait for user response before implementing**

---

## Shared Checkpoint Principles

### When Checkpoints Are Required

**MANDATORY Checkpoints**:
- Before builds (expensive operation, 10-15 minutes)
- Before major direction changes
- Before implementing fixes (validate approach)
- After reproduction (confirm understanding)

**OPTIONAL Checkpoints**:
- After testing (show raw data)
- When uncertain about approach
- When multiple solutions exist

---

### Checkpoint Best Practices

**DO**:
- ✅ Show actual code/data (not descriptions)
- ✅ Explain reasoning clearly
- ✅ Provide time estimates
- ✅ Ask specific approval questions
- ✅ Wait for response before proceeding

**DON'T**:
- ❌ Ask generic "should I continue?" without details
- ❌ Show todo lists as checkpoints
- ❌ Proceed without explicit approval
- ❌ Skip mandatory checkpoints
- ❌ Bundle multiple checkpoints together

---

### Checkpoint Response Handling

**If user approves**:
- Proceed with planned work
- Reference checkpoint in future updates
- Example: "As approved in Checkpoint 2, implementing fix..."

**If user requests changes**:
- Adjust approach based on feedback
- Show updated plan if major changes
- Confirm new direction before proceeding

**If user asks questions**:
- Answer thoroughly
- Update checkpoint with additional details
- Re-confirm approval after clarification

---

### Time Budget Estimates

Include realistic time estimates in checkpoints:

| Complexity | Build Time | Investigation | Implementation | Testing |
|------------|------------|---------------|----------------|---------|
| **Simple** | 10-15 min | 15-30 min | 30 min - 1 hr | 30 min |
| **Medium** | 10-15 min | 1-2 hours | 2-3 hours | 1 hour |
| **Complex** | 10-15 min | 2-4 hours | 3-6 hours | 2 hours |

---

## Example Complete Checkpoint Flow

### PR Reviewer Flow

```
1. Analyze PR → Create test code
2. 🛑 Checkpoint: Show test code (MANDATORY)
3. [User approves]
4. Build and test
5. 🛑 Checkpoint: Show raw data (OPTIONAL)
6. [User approves]
7. Write review
```

### Issue Resolver Flow

```
1. Reproduce issue
2. 🛑 Checkpoint 1: Show reproduction (MANDATORY)
3. [User approves]
4. Investigate root cause
5. 🛑 Checkpoint 2: Validate fix approach (MANDATORY)
6. [User approves]
7. Implement fix
8. Test and create PR
```

---

## Related Documentation

- [PR Reviewer Testing Guidelines](../pr-reviewer-agent/testing-guidelines.md) - PR review workflow
- [Issue Resolver Core Workflow](../issue-resolver-agent/core-workflow.md) - Issue resolution workflow
- [Quick References](../pr-reviewer-agent/quick-ref.md) - Templates and commands

---

**Last Updated**: November 2025
