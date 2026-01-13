# Quick Start: Creating iOS 26 Test Tracking Issues

This guide helps you quickly create GitHub issues for the 4 high-priority tests that are currently skipped on iOS 26/macOS 26.

## TL;DR - One Command

```bash
cd docs/ios26-issues
./create-ios26-issues.sh
```

This will create all 4 issues automatically with the correct labels and content.

## The 4 Issues to Create

### Issue 1: Shell Flyout Header/Footer Resize Tests
- **What**: Test assertions for header/footer resizing are skipped on iOS/Catalyst
- **File**: `HeaderFooterShellFlyout.cs` (lines 68-110)
- **Related**: Issue #26397

### Issue 2: TableView Cell with Context Actions
- **What**: Entire test disabled - cell becomes empty after adding with context actions
- **File**: `Issue2954.cs` (entire file wrapped in `#if TEST_FAILS_ON_IOS`)
- **Related**: Issue #26091

### Issue 3: FlyoutPage RTL Hamburger Icon
- **What**: Entire test disabled - RTL FlyoutPage hamburger icon not displaying
- **File**: `Issue2818.cs` (entire file wrapped in `#if TEST_FAILS_ON_IOS`)
- **Related**: Issue #26726

### Issue 4: CollectionView Scroll Position Reset
- **What**: Entire test disabled - scroll position not reset when ItemsSource updates
- **File**: `Issue7993.cs` (entire file wrapped in `#if TEST_FAILS_ON_IOS`)
- **Related**: Issue #26366

## Option 1: Automated Creation (Recommended)

**Requirements:**
- GitHub CLI (`gh`) installed
- Authenticated with access to dotnet/maui

**Steps:**
```bash
# Navigate to the issue templates directory
cd docs/ios26-issues

# Run the script
./create-ios26-issues.sh

# The script will:
# - Create all 4 issues
# - Apply correct labels
# - Return issue numbers for reference
```

**Example output:**
```
Creating issue: Enable Shell Flyout Header/Footer resize tests on iOS/Catalyst
✅ Created: https://github.com/dotnet/maui/issues/33401

Creating issue: Re-enable Issue2954 test on iOS/Catalyst...
✅ Created: https://github.com/dotnet/maui/issues/33402

Creating issue: Re-enable Issue2818 test on iOS/Catalyst...
✅ Created: https://github.com/dotnet/maui/issues/33403

Creating issue: Re-enable Issue7993 test on iOS/Catalyst...
✅ Created: https://github.com/dotnet/maui/issues/33404

✅ All issues created successfully!
```

## Option 2: Manual Creation

If you prefer to create issues manually or don't have GitHub CLI:

1. Go to https://github.com/dotnet/maui/issues/new
2. Open `docs/ios26-issues/issue1-shell-flyout-resize.md`
3. Copy the title from the script or template:
   - **Title**: "Enable Shell Flyout Header/Footer resize tests on iOS/Catalyst"
4. Copy the entire content of the markdown file into the issue description
5. Add labels: `area-shell`, `platform/iOS 🍎`, `platform/macOS 🍏`, `t/bug`, `s/triaged`
6. Click "Submit new issue"
7. Repeat for issues 2-4

## After Creating Issues

### Recommended Labels to Add

Consider adding these additional labels during triage:
- `version/iOS-26` - Mark as iOS 26 specific
- `p/1` or `p/2` - Set priority
- `i/regression` - If test was working before iOS 26

### Next Steps

1. **Triage**: Review and set priority/milestone for each issue
2. **Investigate**: Determine root cause of each test failure
3. **Fix**: Implement platform fixes where needed
4. **Re-enable**: Remove conditional compilation directives
5. **Verify**: Confirm tests pass on iOS/Catalyst in CI

### Link Issues to PRs

When someone works on fixing these tests, they can reference the issue number in their PR:
```
Fixes #33401
```

## Need More Context?

- **Comprehensive analysis**: See `iOS26-SkippedTests-Analysis.md`
- **Detailed templates**: See `iOS26-TrackingIssues-Template.md`
- **Implementation summary**: See `IMPLEMENTATION-SUMMARY.md`
- **Template documentation**: See `docs/ios26-issues/README.md`

## Troubleshooting

### "gh: command not found"

Install GitHub CLI:
- **macOS**: `brew install gh`
- **Linux**: See https://cli.github.com/manual/installation
- **Windows**: `winget install --id GitHub.cli`

Then authenticate: `gh auth login`

### "You don't have permission to create issues"

Make sure you're authenticated with an account that has write access to dotnet/maui:
```bash
gh auth status
gh auth login
```

### Script fails to create issues

Check that:
1. You're authenticated: `gh auth status`
2. Issue template files exist in `docs/ios26-issues/`
3. You have network connectivity
4. The repository is dotnet/maui (check `REPO` variable in script)

### Want to create only specific issues

Edit the script `create-ios26-issues.sh` and comment out the issues you don't want to create.

## Summary

**Fastest path**: Run `./docs/ios26-issues/create-ios26-issues.sh` and you're done! ✅

This creates 4 tracking issues so that the skipped iOS 26 tests can be investigated and re-enabled.
