---
name: independent-fix-analysis
description: Analyzes an issue independently to develop your own fix approach before comparing with a PR. Use for deep technical reviews where you want to validate the PR author's approach.
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires access to git history, GitHub API, and ability to read/analyze C# code.
---

# Independent Fix Analysis

This skill guides you through analyzing an issue and developing your own fix approach BEFORE looking at how the PR solved it. This ensures you form an independent opinion and can provide meaningful review feedback.

## When to Use

- "Deep review this PR"
- "Analyze this issue and propose your own fix"
- "What do you think the fix should be?"
- "Come up with alternative approaches"
- When you want to challenge/validate a PR's approach

## Dependencies

This skill references the shared infrastructure script:
- `.github/scripts/BuildAndRunHostApp.ps1` - Test runner for UI tests (optional, only if testing your fix)

## Instructions

### Step 1: Understand the Issue (NOT the PR)

Read the linked issue, not the PR diff:

```bash
# Get the issue number from PR description
gh pr view XXXXX --json body | jq -r '.body' | grep -oE "#[0-9]+"

# Read the issue
gh issue view ISSUE_NUMBER
```

Key questions:
- What is the user-reported symptom?
- What version did it work in?
- What version did it break in?
- Is there a reproduction?

### Step 2: Identify the Root Cause

Research the code history:

```bash
# Find when the relevant file changed
git log --oneline --all -20 -- path/to/File.cs

# Look at the breaking commit
git show COMMIT_SHA --stat

# Compare old vs new implementation
git show COMMIT_SHA:path/to/File.cs | head -100
```

Key questions:
- What changed that caused the regression?
- Why did the old approach work?
- Why does the new approach fail?

### Step 3: Design Your Own Fix

Before looking at the PR's approach, determine:

1. **What is the minimal fix?**
   - What's the smallest change that addresses the root cause?

2. **What are alternative approaches?**
   - List 2-3 different ways to solve this
   - Consider trade-offs (complexity, performance, maintainability)

3. **What edge cases should be handled?**
   - Null values, empty collections
   - Race conditions, timing issues
   - Platform-specific behavior

### Step 3b: Regression Analysis (CRITICAL)

**Actively search for scenarios the fix might break:**

1. **Analyze the fix pattern for risks:**

   | Fix Pattern | Regression Risk | Test Scenario |
   |-------------|-----------------|---------------|
   | `== ConstantValue` | Dynamic values won't match | Test with DataTemplateSelector |
   | `!= ConstantValue` | May include invalid values | Test boundary conditions |
   | Performance path change | Other scenarios lose optimization | Instrument and compare |

2. **Check for dynamic values in the codebase:**
   ```bash
   # Find where related IDs/values come from
   grep -rn "template.Id\|\.Id\|GetItemViewType" src/
   
   # Check if values can be dynamic (generated at runtime)
   grep -rn "Interlocked.Increment\|idCounter\|_id = " src/
   ```

3. **Ask these questions:**
   - Does the fix use a **hardcoded constant**? If so, are there cases where the value is **dynamic** (like template IDs)?
   - Does the fix check for a **specific value**? What about **other valid values** that should also match?
   - Does the fix affect a **performance optimization**? Will other scenarios lose that optimization?
   - Is the fix **platform-specific**? What happens on other platforms?

4. **List regression scenarios to test:**
   - For each identified risk, describe a test scenario
   - These will become regression tests in Phase 3

Document your analysis:

```markdown
## My Independent Analysis

### Root Cause
[Explain what's broken and why]

### Proposed Fix Approach
[Describe your preferred solution]

### Alternative Approaches
1. [Alternative 1]: [Pros/Cons]
2. [Alternative 2]: [Pros/Cons]

### Edge Cases to Consider
1. [Edge case 1]
2. [Edge case 2]

### Potential Regressions to Test
1. **[Scenario]**: [Why this might regress] - Test with [specific setup]
2. **[Scenario]**: [Why this might regress] - Test with [specific setup]
```

### Step 4: Implement Your Fix (Optional)

If directed to exhaust alternatives:

```bash
# Revert to baseline (no fix)
git checkout main -- path/to/affected/files

# Implement your fix
# Edit the files...

# Test your fix
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "IssueXXXXX"
```

### Step 5: Document Lines of Code

For each approach, measure the change size:

```bash
# Your fix
git diff main --stat -- path/to/files

# PR's fix (after checking out PR branch)
git diff main..pr-XXXXX --stat -- path/to/files
```

## Output Format

```markdown
## Independent Analysis Complete

### Root Cause
[1-2 paragraph explanation]

### My Proposed Fix
**Approach**: [Name/description]
**Lines changed**: XX lines
**Complexity**: Low/Medium/High

### Alternative Approaches Considered
| Approach | Lines | Complexity | Pros | Cons |
|----------|-------|------------|------|------|
| [Name] | XX | Low | [Pro] | [Con] |

### Edge Cases Identified
1. [Edge case]: [How it should be handled]

### Potential Regressions Identified
| Scenario | Risk Level | How to Test |
|----------|------------|-------------|
| DataTemplateSelector + MeasureFirstItem | Medium | Create test with DataTemplateSelector, verify code path |
| [Other scenario] | [Level] | [Test approach] |

### Ready for Comparison
I have formed an independent opinion on how this should be fixed.
Proceed to compare with PR's approach?
```

## Key Principles

1. **Don't look at the PR diff first** - Form your own opinion
2. **Understand the history** - What changed and when
3. **Consider alternatives** - There's rarely only one solution
4. **Measure complexity** - Simpler is usually better
5. **Think about edge cases** - What could break?
6. **Actively search for regressions** - A fix that "works" may break other scenarios
   - Check for hardcoded constants vs dynamic values
   - Look for `== ConstantValue` patterns that miss dynamic IDs
   - Consider performance optimizations that other code paths depend on
   - Test with DataTemplateSelector, grouped CollectionView, multiple platforms

## Real-World Example: PR #27847

The PR fixed grouped CollectionView by checking `ItemViewType == TemplatedItem` (42).

**Regression found**: When using DataTemplateSelector, ItemViewType is the template's unique ID (101+), not 42. This caused the MeasureFirstItem optimization to be bypassed.

**Better fix**: Check `!= Header && != Footer && != GroupHeader && != GroupFooter` instead of `== TemplatedItem`.

**Lesson**: Always check if a positive match (`==`) might miss valid dynamic values.
