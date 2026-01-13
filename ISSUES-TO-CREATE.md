# GitHub Issues Created for iOS 26 Skipped Tests

This document contains the 4 GitHub issues that need to be created to track re-enabling tests that are currently skipped on iOS/Catalyst.

**Note**: Due to authentication limitations in the automated environment, these issues need to be created manually. Use the information below to create each issue via the GitHub web interface or CLI.

---

## Issue 1: Enable Shell Flyout Header/Footer resize tests on iOS/Catalyst

**Labels**: `area-shell`, `platform/iOS 🍎`, `platform/macOS 🍏`, `t/bug`, `s/triaged`

**Body**:

The test `HeaderFooterShellFlyout.AFlyoutTests()` has header and footer resizing assertions that are currently skipped on iOS and Catalyst platforms due to the header height not updating correctly.

### Current State

The test runs on all platforms, but the resize verification (lines 68-110) only runs on Android and Windows using platform conditional compilation:

```csharp
// Lines 68-110 in HeaderFooterShellFlyout.cs
// These tests are ignored on iOS and Catalyst because the header height doesn't update correctly. 
// Refer to issue: https://github.com/dotnet/maui/issues/26397
#if ANDROID
    // ... resize tests ...
#elif WINDOWS
    // ... resize tests ...
#endif
```

### Test File
- `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/XFIssue/HeaderFooterShellFlyout.cs`
- Lines: 68-110

### Related Issues
- Original issue: #26397

### Expected Behavior

The Shell Flyout header and footer should dynamically resize when their content changes, and this behavior should be testable on iOS and Catalyst.

### Steps to Reproduce

1. Run `HeaderFooterShellFlyout` test on iOS or Catalyst
2. Tap "Resize Header/Footer" button
3. Observe that header/footer height changes are not reflected correctly

### Acceptance Criteria

- [ ] Identify root cause of header/footer resize issue on iOS/Catalyst
- [ ] Fix the underlying platform issue
- [ ] Add iOS/Catalyst test assertions to `HeaderFooterShellFlyout.cs`
- [ ] Verify tests pass on iOS and Catalyst platforms

---

## Issue 2: Re-enable Issue2954 test on iOS/Catalyst - TableView cell becomes empty after adding with context actions

**Labels**: `area-controls-tableview`, `platform/iOS 🍎`, `platform/macOS 🍏`, `t/bug`, `s/triaged`

**Body**:

The test for issue #2954 is currently completely disabled on iOS and Catalyst platforms. The issue is related to TableView cells becoming empty after adding a new cell with context actions.

### Current State

The entire test file is wrapped in a conditional compilation directive:

```csharp
#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
// ... entire test ...
#endif
```

This means the test never runs on iOS or Catalyst.

### Test File
- `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/XFIssue/Issue2954.cs`

### Related Issues
- Original issue logged: #26091

### Test Details

**Issue**: Cell becomes empty after adding a new one with context actions (TableView)

**Test scenario**:
1. Wait for "Cell2" element
2. Tap "Add new" button
3. Verify "Cell2" is still visible

### Expected Behavior

After adding a new TableView cell with context actions, existing cells should remain visible and not become empty.

### Acceptance Criteria

- [ ] Investigate the root cause of the cell visibility issue on iOS/Catalyst
- [ ] Fix the underlying platform issue
- [ ] Remove the `#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST` wrapper
- [ ] Verify test passes on iOS and Catalyst platforms

---

## Issue 3: Re-enable Issue2818 test on iOS/Catalyst - FlyoutPage RTL hamburger icon and flyout items not displaying

**Labels**: `area-controls-flyoutpage`, `platform/iOS 🍎`, `platform/macOS 🍏`, `t/bug`, `s/triaged`, `area-layout`

**Body**:

The test for issue #2818 is currently completely disabled on iOS, Catalyst, and Windows platforms. The issue is related to Right-to-Left (RTL) FlyoutPage behavior where the hamburger icon and flyout items don't display correctly.

### Current State

The entire test file is wrapped in a conditional compilation directive:

```csharp
#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS
// ... entire test ...
#endif
```

### Test File
- `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/XFIssue/Issue2818.cs`

### Related Issues
- iOS FlyoutPage RTL issue: #26726

### Test Details

**Issue**: Right-to-Left FlyoutPage in Xamarin.Forms Hamburger icon issue

**Test scenario**:
1. With flow direction set to RightToLeft on iOS
2. In landscape orientation
3. Flyout items should be displayed correctly
4. Root view should move and content should be visible

**Test steps**:
1. Tap "OpenRootView"
2. Wait for "CloseRootView"
3. Tap "CloseRootView"
4. Set orientation to landscape
5. Tap "OpenRootView" again
6. Verify "CloseRootView" position
7. Verify content visibility

### Expected Behavior

When using RTL flow direction on iOS in landscape orientation:
- Flyout items should be displayed correctly
- Root view should animate properly
- Content should be visible

### Acceptance Criteria

- [ ] Investigate the root cause of RTL FlyoutPage issue on iOS/Catalyst
- [ ] Fix the underlying platform issue (if not already fixed)
- [ ] Remove iOS/Catalyst from the conditional compilation
- [ ] Verify test passes on iOS and Catalyst platforms

---

## Issue 4: Re-enable Issue7993 test on iOS/Catalyst - CollectionView scroll position not reset when updating ItemsSource

**Labels**: `area-controls-collectionview`, `platform/iOS 🍎`, `platform/macOS 🍏`, `t/bug`, `s/triaged`

**Body**:

The test for issue #7993 is currently completely disabled on iOS and Catalyst platforms. The issue is related to CollectionView scroll position not being reset when the ItemsSource is updated.

### Current State

The entire test file is wrapped in a conditional compilation directive:

```csharp
#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS
// In MacCatalyst, the DragCoordinates is not supported. 
// On the iOS platform, scroll position is not reset while update the itemsource. 
// Issue: https://github.com/dotnet/maui/issues/26366
// ... entire test ...
#endif
```

### Test File
- `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/XFIssue/Issue7993.cs`

### Related Issues
- Original issue: #26366

### Test Details

The test has two known issues on iOS/Catalyst:
1. **Catalyst**: DragCoordinates is not supported
2. **iOS**: Scroll position is not reset when ItemsSource is updated

### Expected Behavior

When ItemsSource of a CollectionView is updated, the scroll position should reset to the top (or appropriate initial position).

### Acceptance Criteria

For iOS:
- [ ] Investigate why scroll position is not reset when ItemsSource changes
- [ ] Fix the scroll position reset behavior
- [ ] Remove iOS from conditional compilation or adapt test

For Catalyst:
- [ ] Either implement DragCoordinates support or
- [ ] Adapt test to work without DragCoordinates
- [ ] Remove Catalyst from conditional compilation

- [ ] Verify test passes on iOS and Catalyst platforms

---

## How to Create These Issues

### Option 1: GitHub Web Interface

1. Go to https://github.com/dotnet/maui/issues/new
2. Copy the title from each issue above
3. Copy the body content
4. Add the labels listed for each issue
5. Submit the issue

### Option 2: GitHub CLI

```bash
# Issue 1
gh issue create --repo dotnet/maui \
  --title "Enable Shell Flyout Header/Footer resize tests on iOS/Catalyst" \
  --label "area-shell,platform/iOS 🍎,platform/macOS 🍏,t/bug,s/triaged" \
  --body-file docs/ios26-issues/issue1-shell-flyout-resize.md

# Issue 2
gh issue create --repo dotnet/maui \
  --title "Re-enable Issue2954 test on iOS/Catalyst - TableView cell becomes empty after adding with context actions" \
  --label "area-controls-tableview,platform/iOS 🍎,platform/macOS 🍏,t/bug,s/triaged" \
  --body-file docs/ios26-issues/issue2-tableview-context-actions.md

# Issue 3
gh issue create --repo dotnet/maui \
  --title "Re-enable Issue2818 test on iOS/Catalyst - FlyoutPage RTL hamburger icon and flyout items not displaying" \
  --label "area-controls-flyoutpage,platform/iOS 🍎,platform/macOS 🍏,t/bug,s/triaged,area-layout" \
  --body-file docs/ios26-issues/issue3-flyoutpage-rtl.md

# Issue 4
gh issue create --repo dotnet/maui \
  --title "Re-enable Issue7993 test on iOS/Catalyst - CollectionView scroll position not reset when updating ItemsSource" \
  --label "area-controls-collectionview,platform/iOS 🍎,platform/macOS 🍏,t/bug,s/triaged" \
  --body-file docs/ios26-issues/issue4-collectionview-scroll.md
```

### Option 3: Automated Script

Run the provided script (requires authentication):
```bash
cd docs/ios26-issues
./create-ios26-issues.sh
```

---

## After Creating Issues

Once the issues are created, update this document with the issue numbers:

- [ ] Issue 1: #_____
- [ ] Issue 2: #_____
- [ ] Issue 3: #_____
- [ ] Issue 4: #_____

Then update the PR description to reference these issues.
