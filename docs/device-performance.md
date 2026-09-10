# Device performance measurements

The device-performance tools compare a selected native scenario between a PR's merge-base
and head. They build or accept two device-test apps, run them on one test host in
**base, head, head, base** order, and produce JSON and Markdown comparisons.

There is no AI analysis, managed benchmark selection, automatic PR trigger, or comment
posting in this measurement path. A maintainer selects the scenario and platforms and
authorizes execution through the existing pipeline mechanism or the local drivers.

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

## Manual pipeline execution

Register `eng/pipelines/ci-device-performance.yml` with the existing authorized Azure DevOps
pipeline mechanism. It declares `trigger: none` and `pr: none`.

Supply:

- `prNumber`: the approved PR.
- `pullRequestAuthor`: the PR author's GitHub login, without `@`, for the report's author mention.
- `baseCommitSha`: its full merge-base SHA, not the current base-branch tip.
- `headCommitSha`: the exact head SHA to measure.
- `platform`: `android`, `ios`, `maccatalyst`, or `windows`.
- `expectedScenario`: a supported scenario/platform combination from the table above.

The pipeline snapshots its trusted performance harness and overlays it into separate
base/head builds. It merges only the required performance category constants into each
revision's own `TestCategory.cs`, preserving older or revision-specific categories.
Conflicting performance category values fail rather than silently changing the workload.
It records build provenance, packages both applications, submits the
paired payload to Helix, and publishes `device_performance_results_<platform>`.
Required build pools, workloads, and Helix access must already be available.

Both applications must use the same scenario/harness and comparable SDK/runtime settings.
The pipeline revision is the harness identity; it is distinct from the measured product
head and must be reviewed as trusted code. Never expose privileged credentials to
PR-controlled builds.

On Apple hosts, Helix execution requires a logged-in, non-root console user. The wrapper
resolves that user's UID/home and uses non-interactive `sudo` and `launchctl` with a clean,
explicitly allowlisted environment. It does not write an environment file.

## Local execution

Build the base and head apps separately in Release with the same trusted performance
harness. Use XHarness on Android/iOS/MacCatalyst, or an unpackaged Windows device-test
publish directory with all dependencies. Keep output in a fresh directory per comparison.

- Android/iOS/MacCatalyst: `eng/scripts/Run-DevicePerformanceComparison.ps1`.
- Windows: `eng/scripts/Run-WindowsDevicePerformanceComparison.ps1`.

Both drivers accept app paths, exact base/head/harness SHAs, repository/PR identity,
build identity, runtime/SDK identity, the expected scenario, and an output directory.
For local runs, use explicitly local build identity rather than claiming an AzDO or
Helix run. `-DryRun` produces a run plan without executing either app; app paths must
still exist.

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
    AzdoBuildId = "local"
    AzdoBuildUrl = "local"
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
drivers and the Helix path use the same deterministic renderer. Supply `-PullRequestAuthor`
to either driver or the comparator (pipeline parameter: `pullRequestAuthor`) so the
notification mentions the author. For backwards-compatible offline use, omitting the
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

The pipeline runs `Compare-DevicePerformanceResults.Tests.ps1` before packaging to guard
the layout, author handling, and neutral/advisory/inconclusive result states. Generating
this Markdown does not authorize or perform comment posting.
