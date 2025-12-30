---
name: compare-fix-approaches
description: Compares your independently developed fix against the PR's fix by testing both against the same UI tests. Use after independent-fix-analysis to validate which approach is better.
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires iOS simulator or Android emulator, PowerShell, and BuildAndRunHostApp.ps1 script.
---

# Compare Fix Approaches

This skill compares multiple fix approaches by testing them against the same UI tests and evaluating their trade-offs.

## When to Use

- "Test your fix against the PR's fix"
- "Compare the approaches"
- "Which fix is better?"
- After completing `independent-fix-analysis` skill

## Prerequisites

- You have already completed the `independent-fix-analysis` skill
- You have at least one alternative fix implemented
- The PR's UI tests exist and are ready to run

## Dependencies

This skill uses the shared infrastructure script:
- `.github/scripts/BuildAndRunHostApp.ps1` - Test runner for UI tests across platforms

## Instructions

### Step 1: Establish Test Baseline

First, verify tests fail without any fix:

```bash
# Revert to main (no fix)
git checkout main -- path/to/affected/files

# Run tests - should FAIL
lsof -i :4723 | grep LISTEN | awk '{print $2}' | xargs kill -9 2>/dev/null
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "IssueXXXXX" 2>&1 | tee CustomAgentLogsTmp/UITests/baseline-no-fix.log
```

Record the failure reason for comparison.

### Step 2: Test PR's Fix

```bash
# Checkout PR's fix
git checkout pr-XXXXX -- path/to/affected/files

# Run tests - should PASS
lsof -i :4723 | grep LISTEN | awk '{print $2}' | xargs kill -9 2>/dev/null
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "IssueXXXXX" 2>&1 | tee CustomAgentLogsTmp/UITests/test-pr-fix.log

# Measure PR's fix size
echo "=== PR Fix Size ===" && git diff main --stat -- path/to/affected/files
```

### Step 3: Test Your Alternative Fix(es)

For each alternative:

```bash
# Revert to main
git checkout main -- path/to/affected/files

# Apply your fix
# [Make your edits]

# Run tests
lsof -i :4723 | grep LISTEN | awk '{print $2}' | xargs kill -9 2>/dev/null
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "IssueXXXXX" 2>&1 | tee CustomAgentLogsTmp/UITests/test-alt-N.log

# Measure your fix size
echo "=== Alternative N Size ===" && git diff main --stat -- path/to/affected/files
```

### Step 4: Compare Results

Create a comparison table:

| Approach | Test Result | Lines Changed | Files Changed | Complexity |
|----------|-------------|---------------|---------------|------------|
| No fix (baseline) | ❌ FAIL | 0 | 0 | N/A |
| PR's fix | ? | ? | ? | ? |
| Alternative 1 | ? | ? | ? | ? |
| Alternative 2 | ? | ? | ? | ? |

### Step 5: Evaluate Trade-offs

For each passing approach, evaluate:

1. **Code Simplicity**
   - Is the code easy to understand?
   - Are there redundant changes?
   - Does it follow DRY principles?

2. **Performance**
   - Does it add event handlers that fire frequently?
   - Does it iterate collections unnecessarily?
   - Are there potential memory leaks?

3. **Maintainability**
   - How many files are touched?
   - Does it introduce new abstractions?
   - Could future changes break it easily?

4. **Edge Cases**
   - Does it handle null values?
   - What about empty collections?
   - Platform-specific behavior?

## Output Format

```markdown
## Fix Comparison Results

### Test Results
| Approach | Result | Lines | Complexity |
|----------|--------|-------|------------|
| No fix | ❌ FAIL | 0 | N/A |
| PR's fix | ✅ PASS | 93 | High |
| Alternative 1 | ✅ PASS | 44 | Medium |
| Alternative 2 | ✅ PASS | 14 | Low |

### Recommendation

**Preferred approach**: [Alternative X / PR's fix]

**Reasoning**:
1. [Reason 1 - e.g., "85% less code"]
2. [Reason 2 - e.g., "Uses existing infrastructure"]
3. [Reason 3 - e.g., "Easier to maintain"]

### Concerns with PR's Approach (if any)
1. [Concern 1]
2. [Concern 2]

### Suggested Action
- ✅ Approve as-is
- ⚠️ Request changes: [Describe what to change]
- 💬 Comment: [Suggest improvement but don't block]
```

## Decision Framework

Use this framework to choose the best approach:

```
If all approaches pass tests:
  ├── Choose the one with fewest lines (simpler is better)
  ├── If lines are similar, choose better maintainability
  └── If still tied, prefer the one using existing patterns

If only some approaches pass:
  ├── Investigate why others fail
  ├── Consider if failing approaches miss edge cases
  └── The passing approach may be more complete

If your approach is significantly simpler AND passes:
  └── Recommend it as an alternative in review
```
