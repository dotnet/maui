# iOS 26 Test Re-enablement Tracking Issues

This directory contains GitHub issue templates for tracking tests that are currently skipped or disabled on iOS 26/macOS 26 platforms.

## Overview

During iOS 26/macOS 26 testing, several UI tests were identified as skipped or disabled on iOS/Catalyst platforms. These issue templates help track the work needed to re-enable these tests.

## Issue Templates

| File | Issue Title | Area | Status |
|------|-------------|------|--------|
| `issue1-shell-flyout-resize.md` | Enable Shell Flyout Header/Footer resize tests on iOS/Catalyst | Shell | Ready to create |
| `issue2-tableview-context-actions.md` | Re-enable Issue2954 test on iOS/Catalyst - TableView cell becomes empty | TableView | Ready to create |
| `issue3-flyoutpage-rtl.md` | Re-enable Issue2818 test on iOS/Catalyst - FlyoutPage RTL hamburger icon | FlyoutPage | Ready to create |
| `issue4-collectionview-scroll.md` | Re-enable Issue7993 test on iOS/Catalyst - CollectionView scroll position | CollectionView | Ready to create |

## Creating Issues

### Automated (Recommended)

Use the provided script to create all issues at once:

```bash
cd docs/ios26-issues
./create-ios26-issues.sh
```

The script will:
- Create all 4 issues in the dotnet/maui repository
- Apply appropriate labels
- Return the issue numbers for reference

**Requirements:**
- GitHub CLI (`gh`) installed and authenticated
- Write access to dotnet/maui repository

### Manual

To create issues manually via GitHub web interface:

1. Go to https://github.com/dotnet/maui/issues/new
2. Copy the content from the appropriate markdown file
3. Set the title as specified in the template
4. Add the recommended labels
5. Submit the issue

## Labels Guide

Recommended labels for each issue:

- **Issue 1**: `area-shell`, `platform/iOS 🍎`, `platform/macOS 🍏`, `t/bug`, `s/triaged`
- **Issue 2**: `area-controls-tableview`, `platform/iOS 🍎`, `platform/macOS 🍏`, `t/bug`, `s/triaged`
- **Issue 3**: `area-controls-flyoutpage`, `platform/iOS 🍎`, `platform/macOS 🍏`, `t/bug`, `s/triaged`, `area-layout`
- **Issue 4**: `area-controls-collectionview`, `platform/iOS 🍎`, `platform/macOS 🍏`, `t/bug`, `s/triaged`

Optional additional labels:
- `version/iOS-26` - If specifically related to iOS 26 behavior changes
- `p/1` or `p/2` - Priority (to be determined by triage)
- `i/regression` - If the test was working before iOS 26

## Related Documentation

- [iOS26-SkippedTests-Analysis.md](../../iOS26-SkippedTests-Analysis.md) - Comprehensive analysis of all iOS 26 skipped tests
- [iOS26-TrackingIssues-Template.md](../../iOS26-TrackingIssues-Template.md) - Detailed templates with full context

## Test Files Referenced

| Test File | Location | Issue |
|-----------|----------|-------|
| `HeaderFooterShellFlyout.cs` | `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/XFIssue/` | Lines 68-110 skipped |
| `Issue2954.cs` | `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/XFIssue/` | Entire file disabled |
| `Issue2818.cs` | `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/XFIssue/` | Entire file disabled |
| `Issue7993.cs` | `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/XFIssue/` | Entire file disabled |

## Next Steps After Creating Issues

1. **Triage**: Review and adjust priority/milestone for each issue
2. **Investigation**: Determine root cause of each test failure
3. **Fix**: Implement platform fixes where needed
4. **Re-enable**: Remove conditional compilation directives
5. **Verify**: Confirm tests pass on iOS/Catalyst in CI

## Additional Tests to Review

30+ additional tests are currently disabled on iOS/Catalyst. See the main analysis document for a complete list. These should be reviewed to determine if they're affected by iOS 26 changes.

## Contact

For questions or issues with these templates, please reference:
- Main analysis: [iOS26-SkippedTests-Analysis.md](../../iOS26-SkippedTests-Analysis.md)
- GitHub issue: [Link to issue that created this PR]
