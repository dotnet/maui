---
description: "How to detect and prevent regression bugs caused by PRs that inadvertently remove lines added by prior bug fix PRs. Apply during code reviews of any PR that removes lines from implementation files."
applyTo: "**/*.cs,**/*.xaml,**/*.swift,**/*.java,**/*.kt"
---

# Regression Detection: Reverted Fix Lines

## The Problem

A critical class of regression bugs occurs when a PR fixes issue A but silently removes a line that was added to fix issue B. The reviewer approves the PR thinking it only fixes A, unaware that B has been reintroduced.

### Real Example from dotnet/maui

**PR #33908** fixed issue #33604 (CollectionView not respecting SafeAreaEdges). To fix this, the PR removed `|| parent is IMauiRecyclerView` from `FindListenerForView()` in `MauiWindowInsetListener.cs`.

**What the reviewer didn't know:** That exact line was added by **PR #32278** to fix **issue #32436** (increasing gap at bottom while scrolling in RecyclerView). Removing it caused bug #32436 to reappear as **regression #34634**, shipping in 10.0.60.

**Why it was missed:** The removed line looked intentional — PR #33908 explained that the `IMauiRecyclerView` check was preventing CollectionView items from receiving window inset listeners. The explanation was correct for the NEW issue. But neither the author nor the reviewer knew about the OLD issue the line was protecting against.

## Detection Method

When reviewing any PR that **deletes lines** from implementation files, run the prior fix regression check:

### Step 1: Identify Significant Deleted Lines

Look at the PR diff for deleted lines (`-` prefix) in non-test `.cs`, `.xaml`, `.swift`, `.java`, or `.kt` files.

Skip trivial deletions:
- Blank lines
- Pure comments (`//`, `/* */`, `/** */`)
- Opening/closing braces alone (`{`, `}`)
- `using` statements and `namespace` declarations

**Pay extra attention to:**
- Condition guards: `if (x is SomeType)`, `|| condition`, `&& condition`
- Type checks: `parent is IMauiRecyclerView`, `view is IScrollView`
- Null checks added after null reference exceptions
- Try/catch blocks for specific exception types
- Special-case handling for edge conditions

### Step 2: Check Git Blame on Deleted Lines

```bash
# Check who added a specific line and why
git blame main -- src/path/to/file.cs | grep -F "deleted line content"

# Get the full commit message for that hash
git log --format="%s%n%b" -1 <commit-hash>

# Or use the script:
pwsh .github/skills/pr-finalize/scripts/Detect-Regressions.ps1 -PRNumber XXXXX
```

### Step 3: Check for Issue References

In the commit message, look for:
- `fixes #XXXX`, `closes #XXXX`, `resolves #XXXX`
- `issue #XXXX`, `re: #XXXX`
- PR references like `(#XXXX)` in the subject line

### Step 4: Flag if Origin is a Bug Fix

**Flag as 🔴 Critical** if:
- The deleted line's originating commit references an issue number, AND
- The PR being reviewed does NOT mention that the original issue is still addressed by other means

## What to Output

### When a Regression is Detected

````markdown
### 🔴 Prior Fix Regression Detected

**File:** `src/Core/src/Platform/Android/MauiWindowInsetListener.cs`

**Removed line:**
```diff
- (parent is AppBarLayout || parent is MauiScrollView || parent is IMauiRecyclerView)
+ (parent is AppBarLayout || parent is MauiScrollView)
```

**Origin:** Added in commit `0b30658` (PR #32278):
> "[Android] Refactor WindowInsetListener to per-view registry with MauiWindowInsetListener"

**References issue:** #32436 (increasing gap at bottom while scrolling)

**Risk:** Removing this guard will cause RecyclerView/CollectionView scroll to reintroduce the bottom gap (issue #32436).

**Action required:** PR author must confirm this removal is intentional AND explain how issue #32436 is prevented by other means in this PR.
````

### When Check Passes

````markdown
### ✅ Prior Fix Regression Check: PASSED

No deleted lines were identified as reversions of prior bug fixes.
````

## Guidance for PR Authors

When your PR removes lines, explicitly document WHY in the PR description:

```markdown
### Removed Lines (with justification)

- Removed `|| parent is IMauiRecyclerView` from `FindListenerForView()`
  - **Original purpose:** Added in PR #32278 to fix issue #32436 (bottom gap in RecyclerView)
  - **Why safe to remove now:** This PR adds a more targeted fix in `CollectionViewHandler.cs` that
    handles the RecyclerView inset case directly, making the blanket exclusion in `FindListenerForView()` 
    unnecessary. Verified that issue #32436 does not reproduce with this PR.
```

## High-Risk Patterns in dotnet/maui

These code patterns in MAUI are frequently the targets of "reverted fix" regressions:

### Android Window Insets (`MauiWindowInsetListener.cs`)

Type guards in `FindListenerForView()` that exclude specific view types from inset handling:
```csharp
// Each of these guards was added to fix a specific bug — do NOT remove without understanding why
if (view is not MaterialToolbar &&
    (parent is AppBarLayout || parent is MauiScrollView || parent is IMauiRecyclerView))
{
    return null;
}
```

### iOS Safe Area (`MauiView.cs`, `SafeAreaExtensions.cs`)

Checks that prevent double-application of safe area padding:
```csharp
// Guards preventing double-padding — each added to fix a specific issue
if (IsParentHandlingSafeArea(view, edge)) return;
if (EqualsAtPixelLevel(current, insets)) return;
```

### Android RecyclerView Padding (`CollectionViewHandler.Android.cs`)

Padding resets that prevent cumulative padding bugs:
```csharp
// Reset before applying — added to fix issue #XXXXX where padding accumulated on scroll
view.SetPadding(0, 0, 0, 0);
```

## Integration with Skills

This check is built into:
- **`pr-finalize/SKILL.md`** — Phase 2, Step 1 (runs before all other code review)
- **`pr-finalize/scripts/Detect-Regressions.ps1`** — Automated detection script

When the `pr-finalize` skill is invoked, the Prior Fix Regression Check runs automatically as the first step of Phase 2.
