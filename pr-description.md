<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

## Description

Split iOS device tests by category in Helix to work around an unknown issue that causes tests to take over an hour when run together, vs under 15 minutes when split.

## Problem

When running all Controls.DeviceTests categories together on iOS in CI, the test run takes over an hour. When the same tests are split by category, they complete in under 15 minutes total. This issue only reproduces in CI - we haven't been able to reproduce it locally. Ideally we'd find and fix the root cause, but until then this workaround keeps CI times reasonable.

## Changes

### Helix Configuration (`eng/helix_xharness.proj`)
- Add `ControlsTestCategoriesToSkipForRestOfTests` property defining heavy categories to run individually
- Split Controls.DeviceTests into 4 work items for iOS:
  - 3 heavy categories run separately: `CollectionView`, `Shell`, `HybridWebView`
  - 1 "Other" work item runs all remaining categories
- Core.DeviceTests runs as a single work item (no splitting)
- MacCatalyst and Android unchanged

### Test Infrastructure (`src/Core/tests/DeviceTests.Shared/DeviceTestSharedHelpers.cs`)
- Add support for `TestFilter=SkipCategories=X,Y,Z` environment variable
- Allows skipping multiple categories via comma or semicolon-separated list
- Existing `TestFilter=Category=X` support unchanged

### Documentation (`.github/instructions/helix-device-tests.instructions.md`)
- Add Copilot instructions for Helix device test configuration
- Document category splitting, local execution, and troubleshooting

## Result

iOS device tests now run as ~8 parallel work items instead of 5, with the slowest categories isolated. This brings iOS device test time from 1+ hour down to ~15 minutes.
