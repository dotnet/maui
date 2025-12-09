# PR Submission Guidelines

**Quick Links**:
- PR description template (copy-paste): [quick-ref.md#pr-description-template](quick-ref.md#pr-description-template)
- Self-check before submitting: See "Pre-Submission Self-Check" section below

## Pre-Submission Self-Check (MANDATORY)

**Before creating your PR, verify ALL items:**

✅ **Completeness Check**:
- [ ] Issue reproduced and documented
- [ ] Root cause identified and explained
- [ ] Fix implemented and tested
- [ ] Edge cases tested (🔴 HIGH priority at minimum)
- [ ] UI tests created (HostApp page + NUnit test)
- [ ] UI tests verified (fails without fix, passes with fix)
- [ ] Code formatted (`dotnet format Microsoft.Maui.sln --no-restore`)
- [ ] No breaking changes (or documented if unavoidable)

✅ **Quality Check**:
- [ ] Fix addresses root cause (not just symptoms)
- [ ] Minimal changes (no unnecessary refactoring)
- [ ] Follows existing patterns
- [ ] Platform-specific code properly organized (.ios.cs, .android.cs, etc.)
- [ ] No commented-out code or debug logging left behind
- [ ] Null safety and error handling considered

✅ **Documentation Check**:
- [ ] PR description complete (uses template from quick-ref.md)
- [ ] Before/after evidence included
- [ ] Edge cases documented with priority levels
- [ ] PublicAPI.Unshipped.txt updated if any public APIs changed
- [ ] Breaking changes noted (if any)

**If ANY item fails**: Address it before creating the PR.

**If all items pass**: Proceed with PR creation below.

---

## Creating the Pull Request

### PR Title Format

**Required format**: `[Issue-Resolver] Fix #XXXXX - <Brief Description>`

**Examples:**
- `[Issue-Resolver] Fix #12345 - CollectionView RTL padding incorrect on iOS`
- `[Issue-Resolver] Fix #67890 - Label text truncation with SafeArea enabled`
- `[Issue-Resolver] Fix #11111 - Entry cursor position wrong in RTL mode`

**The `[Issue-Resolver]` prefix:**
- **MANDATORY**: All PRs from the issue-resolver agent MUST use this prefix
- Identifies PRs created by the issue-resolver agent
- Helps maintainers track agent-generated contributions
- Distinguishes from community PRs and PR reviewer agent PRs (`[PR-Reviewer]`)
- Allows filtering and statistics on agent-generated fixes

### PR Description Template

**CRITICAL**: Use collapsible format to keep PR description concise while providing complete details.

**Complete template to use:**

```markdown
Fixes #XXXXX

<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

## Summary

[Brief 2-3 sentence description of what the issue was and what this PR fixes]

Example: 
"CollectionView was not applying correct padding in RTL mode on iOS. This PR fixes the issue by configuring the UICollectionViewCompositionalLayout to respect the FlowDirection property."

**Quick verification:**
- ✅ Tested on [Platform(s)] - Issue resolved
- ✅ Edge cases tested (list key ones briefly)
- ✅ UI tests added and passing

<details>
<summary><b>📋 Click to expand full PR details</b></summary>

## Root Cause

[Technical explanation of WHY the bug existed]

Example:
"The `UpdateFlowDirection` method in `CollectionViewHandler.iOS.cs` only set the `SemanticContentAttribute` on the UICollectionView itself, but UICollectionViewCompositionalLayout requires explicit configuration on the layout object to properly handle RTL scenarios. The layout continued using LTR direction regardless of the view's semantic attribute."

---

## Solution

[Explanation of HOW your fix resolves the root cause]

Example:
"Added a new extension method `UpdateLayoutDirection()` that configures the UICollectionViewCompositionalLayout's scroll direction based on FlowDirection. This method is called whenever FlowDirection changes, ensuring the layout properly mirrors content in RTL mode."

**Changes made:**
- Modified `CollectionViewHandler.iOS.cs` to update layout configuration
- Added `UpdateLayoutDirection()` extension method
- Updated `MapFlowDirection()` to configure both view and layout

---

## Testing

### Reproduction Verified

**Platform**: iOS 18.0 (iPhone 15 Pro Simulator)
**Build**: Debug, net10.0-ios

**Before fix:**
```
=== STATE CAPTURE: AfterTrigger ===
Control Bounds: {X=0 Y=0 Width=393 Height=600}
Left Padding: 0px ❌ (Expected: 16px)
Right Padding: 16px ❌ (Expected: 0px)
```

**After fix:**
```
=== STATE CAPTURE: AfterTrigger ===
Control Bounds: {X=0 Y=0 Width=393 Height=600}
Left Padding: 16px ✅
Right Padding: 0px ✅
```

### Edge Cases Tested

| Scenario | Result |
|----------|--------|
| Empty ItemsSource | ✅ No crash, handles correctly |
| Null ItemsSource | ✅ Handles gracefully |
| Rapid FlowDirection toggling (10x) | ✅ No flicker or incorrect state |
| FlowDirection.MatchParent | ✅ Inherits correctly from parent |
| With headers and footers | ✅ Padding applies to all sections |
| While scrolling | ✅ Updates correctly without interruption |
| Nested in ScrollView | ✅ Both controls handle RTL correctly |

### Platforms Tested

- ✅ iOS 18.0 (Simulator)
- ✅ Android 14.0 (Emulator) - Verify no regression
- ⏭️ Windows - Not affected by this iOS-specific issue
- ⏭️ MacCatalyst - Same code path as iOS, assumed working

### Related Scenarios Verified

Verified these related scenarios still work correctly:
- ✅ FlowDirection on ListView
- ✅ FlowDirection on Grid layouts
- ✅ Nested RTL controls
- ✅ CollectionView with different ItemsLayout types

---

## Test Coverage

**UI Tests Added:**
- ✅ Test page: `src/Controls/tests/TestCases.HostApp/Issues/Issue12345.xaml`
- ✅ NUnit test: `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue12345.cs`

**Test validates:**
- CollectionView displays with correct RTL padding
- Padding values are swapped correctly (left/right)
- Screenshot verification confirms visual correctness

---

## Breaking Changes

**None** - This fix is purely a bug fix with no API changes or behavior changes to correct functionality.

[OR if there are breaking changes:]

**⚠️ Breaking Changes:**
- [List any breaking changes]
- [Explain why they are necessary]
- [Provide migration guidance]

---

## Additional Notes

[Any other context for reviewers]

Examples:
- "This fix only affects iOS. Android already handles this correctly."
- "Considered alternative approach X, but rejected because Y."
- "This may need backporting to release/X.X branch."

---

## PR Metadata
- **Submitted by**: @copilot (Issue Resolver Agent)
- **Issue**: #XXXXX
- **Platforms Fixed**: iOS
- **Breaking Changes**: None

</details>
```

### Format Requirements

1. **Top section (always visible)**:
   - Title showing issue number
   - Required testing note for users
   - Brief summary (2-3 sentences)
   - Quick verification checkboxes
   - Keep this section concise - reviewers should understand the fix immediately

2. **Collapsible section**:
   - Contains all detailed information
   - Wrapped in `<details>` tag with descriptive summary
   - Organized into clearly separated sections with `---` dividers
   - Full context for reviewers who want to deep dive

3. **Required elements** (inside or outside collapsible):
   - `Fixes #XXXXX` link at the top
   - User testing note (always at top, outside collapsible)
   - Root cause explanation
   - Solution approach
   - Testing results (before/after)
   - Edge cases tested
   - UI test locations

### Why Use Collapsible Format

**Benefits**:
- **Concise first impression**: Reviewers see summary and can quickly assess
- **Complete details available**: All information preserved for thorough review
- **Better GitHub UI**: Long PRs don't dominate the screen
- **Faster triage**: Maintainers can quickly scan multiple PRs
- **Professional appearance**: Shows organization and consideration for reviewers

**When to expand details**:
- Initial code review
- Questions about edge cases
- Understanding root cause
- Verifying test coverage

### Writing UI Tests

**REQUIRED for every fix**: Create automated UI tests to prevent regressions.

#### Complete UI Test Creation Guide

See **[UI Tests Instructions](../uitests.instructions.md)** for comprehensive guidance on creating UI tests.

#### Quick Checklist for Issue Tests

Every issue fix requires tests in **TWO locations**:

**1. Test Page (HostApp)**  
Create `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml`:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.Controls.Sample.Issues.IssueXXXXX"
             Title="Issue XXXXX">
    <StackLayout>
        <!-- Reproduce the issue scenario -->
        <Button x:Name="TestButton" 
                AutomationId="TestButton"
                Text="Click Me" />
        <Label x:Name="ResultLabel"
               AutomationId="ResultLabel" />
    </StackLayout>
</ContentPage>
```

**Key requirements**:
- Use `x:Name` for elements you'll reference in code
- Use `AutomationId` for elements tests will interact with
- Reproduce the issue scenario, not just any scenario

**2. Test Page Code-Behind (HostApp)**  
Create `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml.cs`:
```csharp
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, XXXXX, "Brief description", PlatformAffected.All)]
public partial class IssueXXXXX : ContentPage
{
    public IssueXXXXX()
    {
        InitializeComponent();
    }
}
```

**Critical**: Include `[Issue]` attribute with tracker, number, description, and affected platforms.

**3. NUnit Test (Shared Tests)**  
Create `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`:
```csharp
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class IssueXXXXX : _IssuesUITest
{
    public override string Issue => "Brief description of what's being tested";

    public IssueXXXXX(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.YourCategory)]  // ONE category only
    public void IssueXXXXXTestName()
    {
        App.WaitForElement("TestButton");
        App.Tap("TestButton");
        
        var result = App.FindElement("ResultLabel").GetText();
        Assert.That(result, Is.EqualTo("Expected Value"));
        
        // Optional: Visual verification
        VerifyScreenshot();
    }
}
```

**Critical**:
- Inherit from `_IssuesUITest`
- Use only ONE `[Category]` attribute
- Test the fix, not just that the page loads

#### Running Your Tests Locally

**Before committing**, verify your tests work using BuildAndRunHostApp.ps1:

```bash
# Android
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "IssueXXXXX"

# iOS
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "IssueXXXXX"
```

**All logs saved to**: `CustomAgentLogsTmp/UITests/` (check these if tests fail)

See [UI Tests Instructions](../uitests.instructions.md) for:
- Complete test creation workflow
- Platform-specific considerations
- Troubleshooting test failures
- Screenshot verification patterns
- Running tests in CI/CD

#### Common UI Test Mistakes

❌ **Don't**:
- Use multiple `[Category]` attributes (pick ONE)
- Test only that the page loads (test the actual fix)
- Forget `AutomationId` on interactive elements
- Skip local testing before committing

✅ **Do**:
- Test the specific behavior that was broken
- Include edge cases in your tests
- Use descriptive test method names
- Verify tests pass locally on at least one platform

### Code Formatting

**Before committing, format the code:**

```bash
# Format the entire solution
dotnet format Microsoft.Maui.sln --no-restore --exclude Templates/src --exclude-diagnostics CA1822

# Verify no formatting issues
dotnet format Microsoft.Maui.sln --verify-no-changes --no-restore --exclude Templates/src
```

### Creating the PR

**Step-by-step process:**

1. **Commit your changes:**
   ```bash
   git add .
   git commit -m "[Issue-Resolver] Fix #12345 - CollectionView RTL padding"
   ```

2. **Push to your fork:**
   ```bash
   git push origin fix-issue-12345
   ```

3. **Open PR on GitHub:**
   - Navigate to https://github.com/dotnet/maui
   - Click "Pull requests" → "New pull request"
   - Select your branch
   - Use the title format: `[Issue-Resolver] Fix #12345 - CollectionView RTL padding`
   - Fill in the complete PR description template

4. **Add required note at top of description:**
   ```markdown
   <!-- Please let the below note in for people that find this PR -->
   > [!NOTE]
   > Are you waiting for the changes in this PR to be merged?
   > It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!
   ```

5. **Link the issue:**
   - Use `Fixes #12345` in PR description
   - GitHub will automatically link and close the issue when PR merges

6. **Request review:**
   - Tag appropriate maintainers if known
   - Wait for CI checks to pass
   - Respond to review feedback promptly

## PR Checklist

**Before submitting, verify:**

- [ ] PR title follows format: `[Issue-Resolver] Fix #XXXXX - <Description>`
- [ ] PR description is complete with all required sections
- [ ] Required note added at top of PR description
- [ ] Issue is linked with `Fixes #XXXXX`
- [ ] Root cause explained clearly
- [ ] Solution approach documented
- [ ] Before/after test results included
- [ ] Edge cases documented
- [ ] All affected platforms tested
- [ ] UI tests created (TestCases.HostApp + Shared.Tests)
- [ ] Code formatted with `dotnet format`
- [ ] No breaking changes (or justified and documented)
- [ ] PublicAPI.Unshipped.txt updated if public APIs changed
- [ ] XML documentation added for new public APIs
- [ ] No auto-generated files committed (cgmanifest.json, templatestrings.json)

## After PR Submission

**Monitor your PR:**

1. **Watch for CI failures:**
   - Build errors → Fix and push update
   - Test failures → Investigate and fix
   - Format issues → Run `dotnet format` and push

2. **Respond to review comments:**
   - Address feedback promptly
   - Explain your reasoning when needed
   - Make requested changes
   - Re-request review after updates

3. **Update based on feedback:**
   - Make code changes as requested
   - Add additional tests if needed
   - Update documentation
   - Push updates to the same branch

4. **Communicate with issue reporter:**
   - Let them know PR is submitted
   - Ask them to test the PR build when available
   - Thank them for reporting the issue

## Common PR Mistakes to Avoid

❌ **Incomplete PR description**
- Missing root cause explanation
- No before/after test results
- Missing edge case documentation

❌ **Missing tests**
- No UI tests added
- Only tested happy path
- Didn't test on all affected platforms

❌ **Too many changes**
- Unrelated refactoring included
- Multiple issues fixed in one PR
- Changed code formatting throughout unrelated files

❌ **Not linking the issue**
- Forgot `Fixes #XXXXX` in description
- Issue not automatically closed when merged

❌ **Breaking changes without justification**
- Changed public API behavior without explanation
- No migration guidance provided

❌ **Ignoring CI failures**
- Submitted PR with known build errors
- Didn't check test results

## Example of Excellent PR

**Title**: `[Issue-Resolver] Fix #12345 - CollectionView RTL padding incorrect on iOS`

**Description**: Complete, with all sections filled in, root cause explained, solution documented, comprehensive testing results, UI tests added

**Changes**: Minimal, focused only on fixing the specific issue

**Tests**: Both HostApp page and NUnit test added, edge cases covered

**Documentation**: XML docs added for any new public methods

**Result**: Reviewers can quickly understand the issue, verify the fix is correct, and approve with confidence

This is what every issue-resolver PR should look like.
