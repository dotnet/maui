<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

## Summary

This PR exposes new pipeline parameters that let `maui-pr-uitests` and `maui-pr-devicetests` be triggered manually for a targeted subset of test categories on any PR branch. The goal is to enable fast, focused regression checks against an in-flight PR without needing to run the full UI/device test matrix or merge the PR first.

## Why

The intended workflow pairs this PR with the companion PR (#34856), which detects which UI/device test categories a given PR is likely to affect and surfaces those category names in the AI Summary comment posted on the PR. A reviewer (or automation) can then copy those category values directly into the parameters added here and queue a targeted run of `maui-pr-uitests` / `maui-pr-devicetests` against the PR branch — getting fast signal on the relevant subset of tests without paying the cost of the full matrix.

## What changed

- **`eng/pipelines/ci-uitests.yml`** — Adds two pipeline parameters:
  - `uiTestCategories` (string, comma-separated, e.g. `Button,Label,Shell`). When set, overrides `categoryGroupsToTest` with a single matrix entry that runs only those categories. When empty (default), the full matrix runs as before.
  - `prNumber` (string). When set, the build checks out `refs/pull/<N>/head` before building, so any PR can be tested without merging.
- **`eng/pipelines/ci-device-tests.yml`** — Adds two pipeline parameters:
  - `deviceTestCategories` (string, semicolon-separated, e.g. `Button;Label;Shell`). Forwarded to `helix_xharness.proj` as `/p:DeviceTestCategoryFilter=...` via `extraHelixArguments`.
  - `prNumber` (string, same behavior as `ci-uitests.yml`).
- **`eng/pipelines/common/ui-tests.yml`** — Adds the `prNumber` parameter and a PR-checkout step before the Mono and CoreCLR build legs.
- **`eng/pipelines/arcade/stage-device-tests.yml`** — Wires the existing `extraHelixArguments` parameter through to all `HelixProjectArguments` invocations (iOS, MacCatalyst, Android, Android CoreCLR, Windows). The parameter was previously declared but never actually plumbed.
- **`eng/helix_xharness.proj`** — Adds the `DeviceTestCategoryFilter` MSBuild property. When set:
  - Disables iOS category splitting (so all categories run in a single filtered work item, not split per-heavy-category).
  - Applies the filter via `--set-env=TestFilter=Category=...` on Apple work items and `--arg TestFilter=...` on Android.

## New parameter quick-reference

| Pipeline | Parameter | Format | Example |
| --- | --- | --- | --- |
| maui-pr-uitests | `uiTestCategories` | comma-separated | `Button,Label,Shell` |
| maui-pr-devicetests | `deviceTestCategories` | semicolon-separated | `Button;Label;Shell` |
| both | `prNumber` | integer | `34699` |

## Companion PR

This PR is the pipeline-side half of a two-PR change. The companion is dotnet/maui#34856, which detects the affected UI/device test categories for a given PR and prints them in the AI Summary comment. The values printed by `Detect-UITestCategories.ps1` plug directly into the `uiTestCategories` / `deviceTestCategories` / `prNumber` parameters added here.

## Inspiration / prior art

Inspired by dotnet/maui#33176 ("Add PR category detection for UI tests"). We deliberately kept scope smaller here — that PR also explored skipping the provisioning/build legs entirely when no matching categories exist for a PR. That optimization is a reasonable follow-up once the manual-trigger workflow proves itself, but it is intentionally out of scope for this PR.
