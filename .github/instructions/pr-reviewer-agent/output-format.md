# Output Format and Review Structure

## Review Output Format

**CRITICAL**: All reviews MUST be saved to a file named `Review_Feedback_Issue_XXXXX.md` (replace XXXXX with the actual issue number).

Structure your review in this exact format:

```markdown
# Review Feedback: PR #XXXXX - [PR Title]

## Recommendation
✅ **Approve** - Ready to merge
⚠️ **Request Changes** - Issues must be fixed
💬 **Comment** - Feedback but not blocking
⏸️ **Paused** - Cannot complete review (conflicts, environment issues, etc.)

**Required changes** (if any):
1. [First required change]
2. [Second required change]

**Recommended changes** (if any):
1. [First suggested improvement]
2. [Second suggested improvement]

---

<details>
<summary><b>📋 For full PR Review from agent, expand here</b></summary>

## Summary
[2-3 sentence overview of what the PR does and your assessment]

---

## Code Review
[Your analysis of the code changes - see core-guidelines.md for details]

---

## Test Coverage Review
[Analysis of tests added/modified in the PR]

### Issues Found in Tests (if any)
[Specific test issues with file locations and line numbers]

---

## Testing
[Results from your manual testing with the Sandbox app]

### Manual Testing (if applicable)
[Your testing results]

### Recommended Testing Steps (if checkpoint created)
[Commands for others to test]

---

## Security Review
[Security assessment - or "✅ No security concerns" if none found]

---

## Breaking Changes
[Breaking change analysis - or "✅ No breaking changes" if none found]

---

## Documentation
[Documentation review - or "✅ Adequate" if satisfactory]

---

## Issues to Address

### Must Fix Before Merge
[Critical issues that block approval]

### Should Fix (Recommended)
[Important improvements that should be made]

### Optional Improvements
[Nice-to-have suggestions]

---

## Approval Checklist
- [ ] Code solves the stated problem correctly
- [ ] Minimal, focused changes
- [ ] No breaking changes
- [ ] Appropriate test coverage exists
- [ ] No security concerns
- [ ] Follows .NET MAUI conventions
[Add specific items relevant to this PR]

---

## Review Metadata
- **Reviewer**: @copilot (PR Review Agent)
- **Review Date**: [YYYY-MM-DD]
- **PR Number**: #XXXXX
- **Issue Number**: #XXXXX
- **Platforms Tested**: [List or "None"]
- **Test Approach**: [Brief description]

</details>
```

### Format Requirements

1. **Top section (always visible)**:
   - Title with PR number and title
   - Clear recommendation
   - Action items separated into "Required" and "Recommended"
   - Keep this section concise - reader should understand next steps immediately

2. **Collapsible section**:
   - Contains all detailed analysis
   - Wrapped in `<details>` tag with descriptive summary
   - Organized into clearly separated sections with `---` dividers
   - Full review context for those who want deep dive

3. **File naming**:
   - Always `Review_Feedback_Issue_XXXXX.md` where XXXXX is the issue number
   - Save in repository root unless user specifies different location

## Eliminating Redundancy: Key Principles

**CRITICAL**: Before posting your review, eliminate redundancy to respect the reader's time.

### Core Rules

1. **State each fact once** - Don't repeat information in multiple sections
2. **Consolidate related items** - Group similar issues/findings together  
3. **Remove obvious statements** - If it's self-evident, don't state it
4. **Be concise** - Shorter is better when meaning is preserved

---

## Key Examples

### Example 1: Remove Repeated Test Details

❌ **BAD - Repeating information**:
```markdown
## Code Review
The PR modifies `TextViewExtensions.cs` to fix RTL padding.

## Testing
I tested RTL padding. The fix correctly reverses padding in RTL mode.

## Issues Found
None - the padding values are now correct in RTL mode.
```

✅ **GOOD - Information stated once**:
```markdown
## Code Review
Modifies `TextViewExtensions.cs` to fix RTL padding by swapping left/right values.

## Testing
**WITHOUT PR**: Left=0, Right=10 ❌  
**WITH PR**: Left=10, Right=0 ✅

## Issues Found
None
```

---

### Example 2: Consolidate Platform Information

❌ **BAD - Scattered platform mentions**:
```markdown
## Code Review
The PR modifies iOS and Android code...

## Testing
I tested on iOS...

## Issues Found
The iOS fix looks good...
The Android code also looks correct...
```

✅ **GOOD - Platform info consolidated**:
```markdown
## Code Review
**Platforms affected**: iOS, Android

Modifies `TextViewExtensions.cs` (Android) and `MauiLabel.cs` (iOS) to fix RTL padding.

## Testing
Verified on iOS (Android not tested due to emulator unavailability).

## Issues Found
None
```

---

### Example 3: Avoid Redundant Conclusions

❌ **BAD - Saying "it works" multiple times**:
```markdown
## Code Review
The code looks correct and should fix the issue.

## Testing
Test results show the fix works correctly.

## Issues Found
None - everything works as expected.

## Recommendation
✅ Approve - The fix is working correctly.
```

✅ **GOOD - State conclusion once**:
```markdown
## Code Review
Code correctly implements RTL padding swap.

## Testing
Verified: padding values reversed correctly in RTL mode.

## Issues Found
None

## Recommendation
✅ Approve
```

---

### Example 4: Group Related Findings

❌ **BAD - Fragmented issues**:
```markdown
## Issues Found

**Issue 1**: No test coverage for FlowDirection.MatchParent
**Issue 2**: Edge case not tested - what about nested RTL containers?
**Issue 3**: Should also test with Margin property
```

✅ **GOOD - Grouped by theme**:
```markdown
## Issues Found

**Missing test coverage**:
- `FlowDirection.MatchParent` (edge case)
- Nested RTL containers (complex scenario)
- Interaction with Margin property
```

---

## Brevity Guidelines

**Remember**: Make every word count. Shorter is better when meaning is preserved.

| Instead of... | Write... |
|---------------|----------|
| "The RTL padding values are now correctly reversed in RTL mode as they should be" | "RTL padding correctly reversed" |
| "I tested this on iOS and it works correctly" | "Verified on iOS" |
| "I noticed that there's a missing null check on line 47 which could cause issues" | "Missing null check (line 47)" |
| "The code looks correct and should fix the issue mentioned in the PR" | "Code correctly fixes the issue" |

---

## Self-Check Before Posting

**MANDATORY: Run this checklist before posting your review:**

### Redundancy Check
- [ ] No information repeated in multiple sections
- [ ] Platform details consolidated (not scattered)
- [ ] Conclusion stated once (not in every section)
- [ ] Related issues grouped together
- [ ] No obvious/redundant statements

### Completeness Check
- [ ] All required sections present
- [ ] Testing includes both WITH and WITHOUT PR results
- [ ] Issues are specific (not vague)
- [ ] Recommendation is clear

### Quality Check
- [ ] Code review explains WHY, not just WHAT
- [ ] Test results show actual measurements
- [ ] Review is concise and respects reader's time
- [ ] No speculation - only tested observations

**If any item is unchecked, revise before posting.**

---

**Goal**: Clear, concise review that respects the reader's time.
