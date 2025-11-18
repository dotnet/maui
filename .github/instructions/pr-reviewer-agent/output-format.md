# Output Format and Review Structure

## Review Output Format

Structure your review in this order:

```markdown
## Summary
[2-3 sentence overview of what the PR does and your assessment]

## Code Review
[Your analysis of the code changes - see core-guidelines.md for details]

## Testing
[Results from your manual testing with the Sandbox app]

## Issues Found
[Any problems, concerns, or questions - or "None" if everything looks good]

## Recommendation
✅ **Approve** - Ready to merge
⚠️ **Request Changes** - Issues must be fixed
💬 **Comment** - Feedback but not blocking
⏸️ **Paused** - Cannot complete review (conflicts, environment issues, etc.)
```

## Final Review Step: Eliminate Redundancy

**CRITICAL FINAL STEP**: Before posting your review, eliminate redundancy using this 6-step self-review process:

### Step 1: Remove Repeated Test Details

❌ **BAD - Repeating test details**:
```markdown
## Code Review
The PR modifies `TextViewExtensions.cs` to fix RTL padding.

## Testing
I tested the RTL padding by creating a test with `FlowDirection.RightToLeft`.

**Test Setup**:
- Modified `MainPage.xaml` to add Label with `FlowDirection.RightToLeft`
- Added Console.WriteLine for padding values
- Built and deployed to iOS

**Results WITHOUT PR**:
Left padding: 0, Right padding: 10 ❌ (should be reversed)

**Results WITH PR**:
Left padding: 10, Right padding: 0 ✅

The fix correctly reverses padding in RTL mode.

## Issues Found
None - the padding values are now correct in RTL mode.
```

✅ **GOOD - Information presented once**:
```markdown
## Code Review
The PR modifies `TextViewExtensions.cs` to fix RTL padding by swapping left/right padding values when `FlowDirection.RightToLeft` is detected.

## Testing

**WITHOUT PR**: Left=0, Right=10 ❌ (padding on wrong side)
**WITH PR**: Left=10, Right=0 ✅ (padding correctly reversed)

Fix verified on iOS.

## Issues Found
None
```

### Step 2: Consolidate Platform-Specific Information

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

The PR modifies `TextViewExtensions.cs` (Android) and `MauiLabel.cs` (iOS) to fix RTL padding.

## Testing
Verified on iOS (Android not tested due to emulator unavailability).

## Issues Found
None
```

### Step 3: Avoid Redundant Conclusions

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

### Step 4: Merge Related Findings

❌ **BAD - Fragmented related issues**:
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

### Step 5: Remove Obvious Statements

❌ **BAD - Stating the obvious**:
```markdown
## Summary
This PR fixes a bug with RTL padding.

## Code Review
The PR modifies code to fix RTL padding issues.

## Testing
I tested the RTL padding fix to see if it works.
```

✅ **GOOD - Only necessary information**:
```markdown
## Summary
PR correctly implements RTL padding swap for Label on iOS/Android.

## Testing
Verified padding values reverse correctly in RTL mode.
```

### Step 6: Self-Review Checklist

Before posting, verify:

- [ ] No information repeated in multiple sections
- [ ] Platform-specific details consolidated
- [ ] Conclusion stated once (not in every section)
- [ ] Related issues grouped together
- [ ] Obvious/redundant statements removed
- [ ] Review reads smoothly without repetition

**The goal**: Clear, concise review that respects the reader's time.

## Examples to Avoid

### Example 1: Extreme Redundancy

❌ **This review repeats the same information 5 times**:

```markdown
## Summary
This PR fixes RTL padding for Labels. The padding was on the wrong side in RTL mode.

## Code Review
The code changes fix the RTL padding issue by swapping left and right padding when in RTL mode.

## Testing
I tested the RTL padding fix. Before the PR, padding was on the wrong side in RTL mode. 
After the PR, padding is now on the correct side in RTL mode.

**Test Results**:
- WITHOUT PR: Padding on wrong side in RTL ❌
- WITH PR: Padding on correct side in RTL ✅

The RTL padding is now working correctly.

## Issues Found
None - the RTL padding issue is fixed.

## Recommendation
✅ Approve - The RTL padding fix works correctly
```

✅ **Same review without redundancy**:

```markdown
## Summary
PR fixes RTL padding for Labels by swapping left/right values when `FlowDirection.RightToLeft`.

## Code Review
Correctly implements padding swap in `TextViewExtensions.cs` (Android) and `MauiLabel.cs` (iOS).

## Testing
**WITHOUT PR**: Left=0, Right=10 ❌  
**WITH PR**: Left=10, Right=0 ✅

Verified on iOS.

## Issues Found
None

## Recommendation
✅ Approve
```

### Example 2: Scattered Information

❌ **Information about testing scattered everywhere**:

```markdown
## Code Review
I'll need to test this on iOS...

## Testing
I tested on iOS as mentioned above...

## Issues Found
As noted in testing, I couldn't test Android...

## Recommendation
Approve, though I couldn't test Android as I mentioned earlier...
```

✅ **Information organized by section**:

```markdown
## Code Review
[Code analysis]

## Testing
Tested on iOS. Android not tested (no emulator available).

## Issues Found
None

## Recommendation
✅ Approve
```

## Consolidation Strategies

**Strategy 1: Combine Test Setup and Results**

Instead of:
```markdown
**Test Setup**: Created Label with FlowDirection.RightToLeft
**Test Execution**: Built, deployed, and ran app
**Test Results**: Padding values correct
```

Write:
```markdown
Verified with Label in RTL mode: padding values correctly reversed.
```

**Strategy 2: Use Bullet Points for Multiple Items**

Instead of:
```markdown
Issue 1: Missing null check
Issue 2: No test for edge case
Issue 3: Documentation needed
```

Write:
```markdown
**Issues**:
- Missing null check on line 47
- No test for `FlowDirection.MatchParent` edge case
- Public API needs XML documentation
```

**Strategy 3: Combine Related Code Comments**

Instead of:
```markdown
Line 23: Good null check
Line 24: Good error handling
Line 25: Good logging
```

Write:
```markdown
Lines 23-25: Proper error handling with null check and logging.
```

## Final Note on Brevity

**Remember**: Your review is likely one of many the PR author will read. Make every word count.

- ✅ "RTL padding correctly reversed"
- ❌ "The RTL padding values are now correctly reversed in RTL mode as they should be"

- ✅ "Verified on iOS"
- ❌ "I tested this on iOS and it works correctly"

- ✅ "Missing null check (line 47)"
- ❌ "I noticed that there's a missing null check on line 47 which could cause issues"

**Shorter is better** as long as meaning is preserved.
