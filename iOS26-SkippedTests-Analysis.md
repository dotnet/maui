# iOS 26 Skipped Tests and Code Analysis

This document catalogs tests that are currently skipped or disabled due to iOS 26/macOS 26 related issues, as well as code sections that have iOS 26-specific implementations.

## Summary

- **Tests with explicit iOS 26 references**: 2 test files
- **Tests currently disabled on iOS/Catalyst**: 30+ test files
- **Code locations with iOS 26 version checks**: 8 locations
- **Tests referencing iOS-related GitHub issues**: 4+ tests

## 1. Tests with Explicit iOS 26/iPadOS 26 References

### Issue32722 - NavigationPage.TitleView does not expand with host window in iPadOS 26+
- **File**: `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue32722.cs`
- **Status**: Currently wrapped in `#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST`
- **Reason**: Test only works on iOS/Android (SetOrientation not supported on Windows/Catalyst)
- **Issue**: Tests TitleView expansion during rotation on iPadOS 26+
- **HostApp**: `src/Controls/tests/TestCases.HostApp/Issues/Issue32722.xaml[.cs]`
- **Related PR/Issue**: https://github.com/dotnet/maui/issues/32722
- **Action Needed**: ✅ Test exists and runs on iOS/Android - verify it's actually running in CI

### Issue32899 - Dotnet bot image is not showing up when using iOS 26 and macOS 26.1
- **File**: `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue32899.cs`
- **Status**: Active test (no compile-time skip)
- **Issue**: TitleView ImageButton visibility on iOS 26/macOS 26.1
- **HostApp**: `src/Controls/tests/TestCases.HostApp/Issues/Issue32899.xaml[.cs]`
- **Related PR/Issue**: https://github.com/dotnet/maui/issues/32899
- **Action Needed**: ✅ Test exists and should be running - verify results in CI

## 2. Tests with Partial iOS/Catalyst Skips (Inline Platform Conditionals)

### HeaderFooterShellFlyout - Shell Flyout Header/Footer resizing
- **File**: `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/XFIssue/HeaderFooterShellFlyout.cs`
- **Status**: Test runs on all platforms but has platform-specific assertions
- **Lines 68-110**: Header/footer resize tests are **skipped on iOS and Catalyst** using `#if ANDROID` / `#elif WINDOWS`
- **Issue**: Header height doesn't update correctly on iOS/Catalyst
- **Related Issue**: https://github.com/dotnet/maui/issues/26397
- **Action Needed**: 🔴 **CREATE ISSUE** to track enabling these assertions on iOS/Catalyst

## 3. Tests Completely Disabled on iOS/Catalyst

### Issue2954 - Cell becomes empty after adding a new one with context actions (TableView)
- **File**: `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/XFIssue/Issue2954.cs`
- **Status**: Wrapped in `#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST`
- **Related Issue**: https://github.com/dotnet/maui/issues/26091
- **Action Needed**: 🔴 **CREATE ISSUE** to track re-enabling on iOS/Catalyst

### Issue2818 - Right-to-Left FlyoutPage Hamburger icon issue
- **File**: `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/XFIssue/Issue2818.cs`
- **Status**: Wrapped in `#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS`
- **Issue**: FlyoutPage RTL not working as expected on iOS
- **Related Issue**: https://github.com/dotnet/maui/issues/26726
- **Action Needed**: 🔴 **CREATE ISSUE** to track re-enabling on iOS/Catalyst

### Issue7993 - CollectionView scroll position issue
- **File**: `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/XFIssue/Issue7993.cs`
- **Status**: Wrapped in `#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS`
- **Issue**: Scroll position is not reset while updating the itemsource on iOS
- **Related Issue**: https://github.com/dotnet/maui/issues/26366
- **Action Needed**: 🔴 **CREATE ISSUE** to track re-enabling on iOS/Catalyst

## 4. Tests Disabled on iOS (Not Necessarily iOS 26 Specific)

The following tests are currently disabled on iOS/Catalyst. While they may not be directly iOS 26-related, they should be reviewed to determine if iOS 26 changes affected them:

1. **CollectionViewUITests.CollectionViewItemsUpdatingScrollMode.cs** - Picker control issues on iOS/macOS (Issue #28024)
2. **Issue10025.cs** - Select items traces preserved (Issue #26187)
3. **Issue13537.cs** - TBD
4. **Issue15173.cs** - TBD
5. **Issue16910.cs** - Flaky test on Mac and Windows (Issue #28368)
6. **Issue18158.cs** - TBD
7. **Issue18623.cs** - App.EnterText not working with iOS password (Issue #18981)
8. **Issue18896.cs** - Related issues #18811, #15994
9. **Issue19509.cs** - Issue #28806
10. **Issue22987.cs** - TBD
11. **Issue25598.cs** - TBD
12. **Issue27474.cs** - Issue #27999
13. **Issue27803.cs** - Issue #20904
14. **Issue28343.cs** - App.ScrollUp does nothing on Catalyst (Issue #31216)
15. **Issue28622.cs** - CollectionView2 ItemsLayout change exception (Issue #28678)
16. **Issue28945.cs** - TBD
17. **Issue29261.cs** - TBD
18. **Issue29492.cs** - TBD
19. **Issue29633.cs** - Highlighting not working on Mac Catalyst (Issue #27519)
20. **Issue29824.cs** - TBD
21. **Issue30690.cs** - TBD
22. **Issue31361.cs** - TBD
23. **Issue5354.cs** - TBD
24. **Issue8945DatePicker.cs** - TBD
25. **Issue8945Picker.cs** - TBD
26. **Issue8945TimePicker.cs** - TBD
27. **Bugzilla31333.cs** - Editor cursor issue with Appium
28. **Bugzilla46363.cs** - TBD
29. **Bugzilla46363_2.cs** - TBD
30. **Bugzilla58875.cs** - Context actions menu items not accessible

**Action Needed**: 🟡 Review each to determine iOS 26 relevance

## 5. Code Locations with iOS 26 Version Checks

The following locations in the codebase have runtime checks for iOS 26:

### NavigationRenderer.cs (4 locations)
- **File**: `src/Controls/src/Core/Compatibility/Handlers/NavigationPage/iOS/NavigationRenderer.cs`
- **Lines**: 1621, 1728, 2183, 2287
- **Purpose**: iOS 26+ requires autoresizing masks instead of constraints for TitleView layout
- **Comment**: "iOS 26+ requires autoresizing masks and explicit frame sizing to prevent TitleView from covering content"

### ShellPageRendererTracker.cs (3 locations)
- **File**: `src/Controls/src/Core/Compatibility/Handlers/Shell/iOS/ShellPageRendererTracker.cs`
- **Lines**: 287, 338, 723
- **Purpose**: iOS 26+ layout behavior workarounds for TitleView in Shell
- **Comment**: "iOS 26+ and MacCatalyst 26+ require autoresizing masks instead of constraints"

### ShellSectionRenderer.cs (1 location)
- **File**: `src/Controls/src/Core/Compatibility/Handlers/Shell/iOS/ShellSectionRenderer.cs`
- **Line**: 832
- **Purpose**: iOS 26+ ViewController checks

### TabbedRenderer.cs (1 location - iPadOS 26.1)
- **File**: `src/Controls/src/Core/Compatibility/Handlers/TabbedPage/iOS/TabbedRenderer.cs`
- **Line**: 135
- **Purpose**: Guard against ViewDidLayoutSubviews being called during construction on iPadOS 26.1+
- **Comment**: "On iPadOS 26.1+, ViewDidLayoutSubviews can be called during UITabBarController construction in narrow viewports (< 667 points)"

**Action Needed**: ✅ These are implemented workarounds, not tests to track

## 6. Recommended Actions

### High Priority - Create Tracking Issues

1. **HeaderFooterShellFlyout resize tests on iOS/Catalyst**
   - Title: "Enable Shell Flyout Header/Footer resize tests on iOS/Catalyst"
   - Related: Issue #26397
   - Current state: Tests are skipped using platform conditionals
   - Test file: `HeaderFooterShellFlyout.cs` lines 68-110

2. **Issue2954 - TableView context actions**
   - Title: "Re-enable Issue2954 test on iOS/Catalyst - TableView cell empty after adding with context actions"
   - Related: Issue #26091
   - Current state: Entire test file disabled on iOS/Catalyst

3. **Issue2818 - FlyoutPage RTL**
   - Title: "Re-enable Issue2818 test on iOS - FlyoutPage RTL hamburger icon"
   - Related: Issue #26726
   - Current state: Entire test file disabled on iOS/Catalyst/Windows

4. **Issue7993 - CollectionView scroll position**
   - Title: "Re-enable Issue7993 test on iOS/Catalyst - CollectionView scroll position reset"
   - Related: Issue #26366
   - Current state: Entire test file disabled on iOS/Catalyst

### Medium Priority - Verify Test Execution

1. **Issue32722** - Verify this test is actually running in CI for iOS/Android
2. **Issue32899** - Verify this test is actually running in CI

### Low Priority - Review and Categorize

Review the 30+ tests disabled on iOS/Catalyst to determine:
- Which are iOS 26 specific
- Which are general iOS issues
- Which can be re-enabled
- Which need new tracking issues

## 7. Summary Statistics

- ✅ **2 tests** explicitly test iOS 26 functionality (Issue32722, Issue32899)
- 🔴 **4 tests** need tracking issues for iOS/Catalyst re-enablement
- 🟡 **30+ tests** need review for iOS 26 relevance
- ✅ **8 code locations** with iOS 26 runtime checks (already implemented)

## Notes

- Many iOS test failures are due to Appium/automation limitations (Context Actions, Picker controls)
- Some tests are disabled due to platform API limitations (SetOrientation on Catalyst/Windows)
- iOS 26 introduced layout behavior changes requiring autoresizing masks instead of constraints
- Focus should be on tests that were working before iOS 26 and broke after
