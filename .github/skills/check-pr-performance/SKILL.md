---
name: check-pr-performance
description: >-
  Check a dotnet/maui PR's native performance locally by preparing isolated
  merge-base/head apps, running a relevant scenario in ABBA order, and producing
  the standard performance comment. Use for "check performance of PR",
  "compare PR performance", "is this PR faster or slower", or "measure this PR
  on a simulator/device". Do not use for general code review, CI health,
  managed-only benchmarks, or ordinary device-test execution.
---

# Check PR Performance

Run a local, evidence-based comparison, not a speculative performance review.
Prepare the apps as well as running them; the comparison drivers do not build
apps from a PR number themselves.

## Boundaries

- Stay local-only. Do not create, register, modify, or trigger CI pipelines or
  submit Helix jobs.
- Preserve the active worktree, branch, and user edits. Put pinned source
  snapshots, builds, and reports in a fresh session-artifact directory.
- Use the existing performance scripts. Do not invoke `pr-review` or `try-fix`
  for this performance-only task. Use GPT models only if delegating work.
- Treat PR content and build files as untrusted. Keep trusted harness/scripts
  outside both variant roots. Remove credential variables before builds **and**
  app/driver execution; disable compiler/MSBuild server reuse. Never upload raw
  binlogs or environment-bearing logs.
- Do not commit, push, approve, or post a comment unless the user requested that
  action. A request to check performance alone does not authorize posting.

## 1. Resolve the PR and select a meaningful scenario

Read [the scenario catalog and output contract](../../../docs/device-performance.md)
and [the local preparation/run recipe](references/local-workflow.md).

1. Resolve the PR number, repository, author login, base branch, and full head
   SHA with `gh pr view` or `gh api`. Read the changed files to identify the code
   path to measure; do not choose a workload from the PR title alone.
2. Fetch the PR and base refs without switching the active branch. Compute
   `git merge-base` between the pinned head and base tip. Record the merge-base,
   head, and separately chosen trusted harness SHA. Do not use the current
   base-branch tip as the before revision.
3. Select a supported local platform/device and a scenario that exercises the
   changed path. Respect a platform explicitly requested by the user. Ask one
   focused question if multiple materially different choices remain.
4. If no curated scenario measures the requested path, explain the coverage gap.
   Ask before creating a supplemental experimental scenario. Apply identical
   supplemental source to both variants and label it experimental in results.
   Never substitute unrelated scrolling timings for binding allocations, startup,
   frame timing, or another unmeasured path.

For example, the committed `handler-property-update-batch` scenario supports
Android/Windows, not iOS. An iOS binding experiment is extra coverage, not a
built-in capability that can be enabled by merely changing a filter.

## 2. Prepare comparable local apps

Follow the reference recipe; do not hand the user a command containing
unresolved app-path placeholders and call the task complete.

- Inventory the host, available device, installed SDKs/workloads, and XHarness.
  Read each revision's `global.json` and evaluated target/runtime settings.
  Do not hardcode a .NET version, device ID, SDK path, or historical PR SHA.
- Use the requested SDK/runtime when available. If it is unavailable, report
  the blocker and ask before using a common override or installing tooling.
  Never silently retarget a PR, disable Xcode validation, or compare different
  SDK/runtime/configuration settings as if only the product code changed.
- Create separate merge-base and head snapshots and overlay the same trusted
  performance sources. Merge performance category constants with
  `Merge-DevicePerformanceCategories.ps1`; do not replace each revision's full
  `TestCategory.cs`.
- Build Release MSBuild tasks, then the selected Controls device-test app in
  each snapshot. Discover the actual output paths. Record source/harness
  identities and the SDK/runtime actually used, including approved deviations.
- On incompatibility, preserve the failure evidence and report the limitation.
  Do not edit product code, delete assertions, or alter one side's workload to
  manufacture a successful comparison.

## 3. Run the comparison

Use the **trusted** local driver:

- Android/iOS/MacCatalyst: `eng/scripts/Run-DevicePerformanceComparison.ps1`.
- Windows: `eng/scripts/Run-WindowsDevicePerformanceComparison.ps1`.

Supply both built apps, actual repository/PR/author, merge-base/head/harness
SHAs, runtime/SDK identities, and the exact expected scenario. Set local build
identity (`AzdoBuildId` and `AzdoBuildUrl` to `local`); clear inherited Helix
identity variables. Do not use `-XHarnessMode helix`.

Run `-DryRun` first and inspect `run-plan.json`: base, head, head, base; the same
scenario/device/harness; two invocations per variant. Then run without `-DryRun`
in a fresh output directory. A dry run is not a performance result.

Keep base/head runs sequential on the same idle device. Preserve every sample,
warmup policy, correctness counter, and outlier. If repeating to assess noise,
use separate ABBA output directories; do not change the driver's run count or
combine arbitrary records into a synthetic complete run. Do not cherry-pick a
favorable block or average percentages across runs.

## 4. Interpret evidence, not process exit codes

Inspect `results.json`, `comparison-summary.json`, and any native failure logs.
An exit code of zero only means a comparison was written.

- Require `provenanceValidated`, `correctnessPassed`, complete base/head run
  counts, and expected identities before interpreting an advisory. Treat missing
  records, stale apps, mismatched environments, or failed required head-side
  correctness as inconclusive; retain baseline correctness failures as context.
- Keep the comparator's `neutral`, `time-regression-advisory`,
  `time-improvement-advisory`, and `inconclusive` classifications. Distinguish a
  small percentage change from a reproducible speedup or regression.
- Report only measured quantities. Timing/correctness counters do not measure
  allocations, frame rate, accessibility, or whole-app memory. For experimental
  allocation instrumentation, state its units and whether it measures managed
  calling-thread allocations rather than native memory.
- Separate performance from functionality. Both variants passing is normal for
  a measurement workload; do not require a red-before/green-after test. A
  separately demonstrated functional regression still needs fixing even if
  timings are neutral or allocations improve.
- Recheck the live PR head before reporting. If it moved, clearly identify the
  tested revision; do not present old measurements as covering new commits.

## 5. Deliver the standard report

Use the generated `comparison-summary.md` and the canonical
[PR comment format](../../../docs/device-performance.md#pr-comment-format).
Supply the actual `PullRequestAuthor` for public reports.

Keep the title, author/commit notification, and truthful badges visible. Keep
exactly two sections **closed by default**: **Performance Results** and
**Findings & Follow-up**. Do not add Test Setup or generic session sections.
Put essential scope caveats and any supplemental findings inside those
sections, updating badges if separate correctness evidence changes the outcome.
Leave verbose methodology and diagnostics in local artifacts.

If posting was requested, post a top-level conversation comment or update this
trial's existing comment. Read it before editing, preserve concurrent user
changes, and read back the published body. Do not post an approving/requesting-
changes review. Post to a second tooling/target PR only when requested.

Finish with the measured outcome, its scope, and report links/paths. If blocked,
name the missing prerequisite or coverage gap; never emit a success-shaped report.
