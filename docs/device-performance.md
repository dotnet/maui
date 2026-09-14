# Device performance measurements

The device-performance tools compare a selected native scenario between a PR's merge-base
and head. The local drivers accept two device-test apps, run them on one test host in
**base, head, head, base** order, and produce JSON and Markdown comparisons.

Execution is local-only, through scripts or the `check-pr-performance` skill.
There is no remote submission, automatic PR trigger, managed benchmark selection, or
automatic comment posting. The skill prepares the apps and runs the same local scripts;
the scripts themselves do not invoke an AI model.

## Available scenarios

| `expectedScenario` | Platforms | Recorded correctness |
|---|---|---|
| `collectionview-keepitemsinview-update` | Android, iOS, MacCatalyst | First visible item is preserved through collection updates |
| `collectionview-grouped-scrollto-makevisible` | iOS, MacCatalyst | Target visibility and final-position consistency |
| `carouselview-swipe-disabled` | Android, iOS, MacCatalyst | Disabled touch interception on Android; embedded scroll-state reapplication on Apple |
| `carouselview-wheel-snap-windows` | Windows | Centering tolerance and Position/CurrentItem consistency after native offset changes |
| `handler-property-update-batch` | Android, Windows | Completed update batches and matching native control values |

These are finite, curated scenarios, not coverage of every possible code path. A result
applies only to the scenario, platform, revisions, and environment in its metadata.
Unmeasured paths remain unmeasured.

Performance categories are excluded from normal device-test runs. The comparison drivers
explicitly select the required category; Windows also opts in during category discovery.

## Local execution

For Copilot-assisted execution, ask **"Check performance of PR #12345 on iOS."**
The [check-pr-performance skill](../.github/skills/check-pr-performance/SKILL.md)
selects a relevant supported scenario, prepares isolated merge-base/head builds,
runs the local comparison, and generates the standard report. Add
**"post the results on the PR"** only when you want Copilot to publish the comment.

Build the base and head apps separately in Release with the same trusted performance
harness. Use XHarness on Android/iOS/MacCatalyst, or an unpackaged Windows device-test
publish directory with all dependencies. Follow the
[local preparation recipe](../.github/skills/check-pr-performance/references/local-workflow.md)
for pinned source snapshots, credential isolation, category overlays, and build commands.
The trusted harness is distinct from the measured product head. Never expose privileged
credentials to PR-controlled builds or apps, and do not run them as root.
Keep output in a fresh directory per comparison.

- Android/iOS/MacCatalyst: `eng/scripts/Run-DevicePerformanceComparison.ps1`.
- Windows: `eng/scripts/Run-WindowsDevicePerformanceComparison.ps1`.

Both drivers accept app paths, exact base/head/harness SHAs, repository/PR identity,
runtime/SDK identity, the expected scenario, and an output directory.
Use `-XHarnessMode dotnet` for `dotnet xharness` or `-XHarnessMode global` for an
`xharness` executable on your PATH; Windows invokes the local app directly.
`-DryRun` produces a run plan without executing either app; app paths must still exist.

Example Android plan (replace paths and identities with the actual local inputs):

```powershell
$run = @{
    Platform = "android"
    BaseApp = "C:\perf\base\com.microsoft.maui.controls.devicetests-Signed.apk"
    HeadApp = "C:\perf\head\com.microsoft.maui.controls.devicetests-Signed.apk"
    BaseCommitSha = "<MERGE_BASE_SHA>"
    HeadCommitSha = "<HEAD_SHA>"
    HarnessSha = "<TRUSTED_HARNESS_SHA>"
    Repository = "dotnet/maui"
    PullRequestNumber = 12345
    PullRequestAuthor = "<AUTHOR_LOGIN>"
    ExpectedScenario = "collectionview-keepitemsinview-update"
    BaseRuntimeVariant = "mono"
    HeadRuntimeVariant = "mono"
    BaseSdkVersion = "<SDK_VERSION>"
    HeadSdkVersion = "<SDK_VERSION>"
    DeviceId = "emulator-5554"
    OutputDirectory = "C:\perf\comparison"
}
.\eng\scripts\Run-DevicePerformanceComparison.ps1 @run -DryRun
```

Remove `-DryRun` only when intentionally executing the measurement. Use an appropriate
test host; local results are machine-specific.

## Outputs and interpretation

| File | Contents |
|---|---|
| `run-plan.json` | ABBA order and execution inputs |
| `results.json` | Parsed `MAUI_PERF_RESULT` records, including measurements, counters, and provenance |
| `comparison-summary.json` | Expected identities, provenance/correctness status, comparisons, and result classification |
| `comparison-summary.md` | Compact, comment-ready results with collapsed findings and follow-up |

The parser also reassembles chunked Android log records. Comparisons require two complete
runs per side, matching identities and environment metadata, and the required correctness
counters. A buggy baseline can provide context, but required head-side correctness must
pass.

Result records and comparison summaries use schema version 3. Rebuild both apps with
the same current harness before running these scripts; older records are rejected rather
than compared under a different provenance contract. Repository, PR, product/harness SHAs,
run ordinals, device, OS, architecture, runtime, and SDK identity remain recorded.

These scenarios are measurement workloads, not regression tests for a particular product
fix. Passing on both revisions is expected; assess the recorded correctness and comparison
results rather than requiring a failing test before the change. Warmups are excluded from
timing samples but still execute the scenario operation. For example,
`updatesPreservingFirstVisibleItem` includes both warmups and measured updates, so a correct
run with two warmups and ten measurements reports 12.

Timing changes remain advisory: the comparator flags non-overlapping repeated ranges with
at least a 15% median change by default. Its classifications are `neutral`,
`time-regression-advisory`, `time-improvement-advisory`, or `inconclusive`.
Inspect `provenanceValidated`, `correctnessPassed`, and `verdict`; exit code zero means
the comparison was written, not that the scenario was correct or regression-free.

Latency and correctness counters do not imply allocation, frame timing, or accessibility
coverage. Accessibility defaults to `not-assessed`. These results inform a human decision;
they do not approve a PR or prove whole-PR performance.

## PR comment format

Use the generated `comparison-summary.md` when sharing performance results. Both local
drivers use the same deterministic renderer. Supply `-PullRequestAuthor` to either
driver or the comparator so the notification mentions the author. For offline use, omitting the
author produces a linked PR notice instead; it never guesses an author or queries GitHub.
When preparing a public comment, obtain the actual PR author's login and supply it.

Keep this layout for generated and manually supplemented performance comments:

- Visible: **Performance Review Summary**, an author/commit notification, and truthful status badges.
- Collapsed by default: **Performance Results** and **Findings & Follow-up**, exactly two sections.
- No **Test Setup**, **Review Sessions**, or additional expanded narrative.

Keep the result table and actionable findings concise. Preserve essential scope caveats,
especially experimental scenarios, simulator-only runs, and managed versus native allocations.
Do not turn a neutral timing result into a correctness or merge approval. Detailed ranges,
counters, environment metadata, and provenance remain in `comparison-summary.json`.

Run `pwsh -NoProfile -File eng/scripts/Compare-DevicePerformanceResults.Tests.ps1`
locally to check the layout, author handling, and neutral/advisory/inconclusive result
states. Generating this Markdown does not authorize or perform comment posting.
