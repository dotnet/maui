# Local evidence contract

This is a manual Copilot handoff, not a job queue. The maintainer chooses the PR,
pins its merge-base and head, independently reviews the harness revision, authorizes
execution, and controls the evidence directories. No pipeline, workflow, registration,
remote build status, or publication is required.

## Entrypoints

Use PowerShell 7 and trusted scripts from the reviewed tooling checkout:

```powershell
& $NewRequests -SelectionPath $SelectionPath -PrMetadataPath $PrMetadataPath `
    -CurrentHeadSha $HeadSha -ResultsRoot $ResultsRoot -OutputPath $RequestPath `
    -Repository "dotnet/maui"

& $ValidateDevice -SelectionPath $SelectionPath -RequestPath $RequestPath `
    -ResultsRoot $ResultsRoot -SummaryPath $CompletedSummaryPaths `
    -Repository "dotnet/maui" -PullRequestNumber $PrNumber `
    -BaseCommitSha $MergeBaseSha -HeadCommitSha $HeadSha -CurrentHeadSha $CurrentHeadSha `
    -HarnessSha $HarnessSha -JsonOut $DeviceValidationPath
```

`$NewRequests` and `$ValidateDevice` are the absolute trusted paths to
`scripts\New-DevicePerformanceRequests.ps1` and
`scripts\Validate-DevicePerformanceEvidence.ps1`. All evidence/output path arguments
are absolute local filesystem paths. `ResultsRoot` must already exist and be a dedicated,
caller-owned trial directory. Requests and validation outputs must be outside that root
and must not overwrite their input files. UNC/network paths, symlinks, and reparse-point
components are rejected. Choose real host paths rather than linked aliases.

`PrMetadataPath` contains numeric `number` plus full, lowercase, 40-character
`mergeBaseOid`, `headRefOid`, and `harnessSha`. `Repository` defaults to `dotnet/maui`
on request generation only; the validator requires it explicitly. Repository names are
canonical lowercase `owner/name`. Preserve other resolved metadata (`state`,
`baseRefName`, and the pinned base tip) with the trial. Neither script resolves an
authorization identity or contacts GitHub. The caller verifies commit/archive identities
and supplies a freshly checked `CurrentHeadSha`; mismatches fail closed.

## Reviewed request array

`device-requests.json` is a JSON array, including `[]` for no supported local pairs.
Each `manual-local-ready` selection entry must match the trusted catalog's scenario,
driver, and complete lowercase platform list. Unsupported entries have no `localRun`
and produce no request; they remain coverage gaps.

Each deduplicated request contains exactly:

| Field | Value |
|---|---|
| `schemaVersion` | `1` (local request schema) |
| `executionMode` | `manual-local` |
| `requestKey` | `maui-perf-` plus the full lowercase SHA256 described below |
| `repository`, `pullRequestNumber` | Authorized repository and positive integer PR number |
| `baseCommitSha`, `headCommitSha`, `harnessSha` | Exact full pinned SHAs |
| `scenarioIds` | Sorted unique catalog IDs requiring this pair |
| `expectedScenario`, `platform` | Exact native result scenario and canonical platform |
| `expectedVariantRuns` | `2` (four records, ABBA) |
| `driver` | Trusted catalog's repository-relative local driver path |
| `resultDirectory` | Canonical `Join-Path $ResultsRoot $requestKey` |

The key hashes these UTF-8 values joined by a literal `|`, without whitespace or a
trailing separator:

```text
repository|pullRequestNumber|baseCommitSha|headCommitSha|harnessSha|expectedScenario|platform|2
```

Directory paths do not enter the key: using a fresh `ResultsRoot` makes a separate trial
of the same requested experiment. They are nevertheless checked exactly in the reviewed
request array and against every supplied summary path. Do not edit/relabel an old handoff
to claim a new PR, revision, platform, or relocated run. Select requests by exact key,
not a fuzzy scenario title. Generation creates neither result directories nor processes.

## Acquisition is a separate manual operation

First invoke `check-pr-performance` and follow its
`references/local-workflow.md`: export pinned product/harness snapshots, overlay only
the trusted measurement harness identically, build isolated variants, and record actual
app paths, SDKs, runtimes, and device/host selection. Keep credentials out of untrusted
builds/apps. Request JSON is not permission to execute a PR.

After reviewing one exact request, construct driver arguments from those verified facts:

```powershell
$run = @{
    BaseApp = $BaseApp
    HeadApp = $HeadApp
    Repository = $request.repository
    PullRequestNumber = $request.pullRequestNumber
    BaseCommitSha = $request.baseCommitSha
    HeadCommitSha = $request.headCommitSha
    HarnessSha = $request.harnessSha
    ExpectedScenario = $request.expectedScenario
    ExpectedVariantRuns = $request.expectedVariantRuns
    BaseRuntimeVariant = $BaseRuntime
    HeadRuntimeVariant = $HeadRuntime
    BaseSdkVersion = $BaseSdk
    HeadSdkVersion = $HeadSdk
    OutputDirectory = $request.resultDirectory
}
```

On Windows, invoke the trusted `eng\scripts\Run-WindowsDevicePerformanceComparison.ps1`
with that map. On Android/iOS/MacCatalyst, use
`eng\scripts\Run-DevicePerformanceComparison.ps1` and also supply `Platform`,
the explicitly selected `DeviceId` where applicable, and installed `XHarnessMode`
(`dotnet` or `global`). `MacCatalystResultFileRoot` is MacCatalyst-only.
`PullRequestAuthor` is optional report text, not authorization. Do not pass a build ID,
queue status, package identifier, or remote artifact location.

Use a **different** output directory for an authorized driver `-DryRun`, inspect that
plan, then use the request's fresh `resultDirectory` for the explicitly authorized real
run. A plan alone is never accepted as measurements. Do not silently overwrite/rerun an
existing trial. This acquisition recipe is not executed by the interpretation skill.

## Local admission and completeness

The caller supplies `RequestPath` and `ResultsRoot` independently of result JSON. The
validator regenerates the complete expected request set from selection and the supplied
identities. Missing, extra, duplicate, or altered requests are errors. Submit only the
explicit `comparison-summary.json` paths for completed requests; omit `SummaryPath` or
pass an empty array while everything is pending. No directory scan discovers new evidence.

Every submitted request directory must contain:

- `run-plan.json`: four ordered base-1, head-1, head-2, base-2 entries with exact commits.
  The cross-platform driver records `Number` and `RunDirectory`; Windows records
  `runOrdinal`. Every run directory must exist beneath the exact request directory.
- `results.json`: four complete native **schema-3** records. All repository, PR,
  variant/commit, harness, scenario/platform, ordinal, and expected-run identities must
  match. Warmups are nonnegative integers, correctness is an explicit Boolean, and all
  records have valid timestamps, matching full environments, equal workload counts,
  nonempty nonnegative finite measurements, matching per-run statistics, and complete
  numeric counters on both sides.
- `comparison-summary.json`: native **schema 3**, exact `expected` identities, one
  complete comparison with exact scenario/platform/base/head identity, provenance and
  HEAD correctness, baseline correctness context, statistics, counters, and verdict.
  Per-comparison property names may retain the core's PascalCase.
- `comparison-summary.md`: nonempty local human-readable output; the validator never
  treats its prose as evidence or executes anything found in it.

The trusted core `eng\scripts\Compare-DevicePerformanceResults.ps1` recomputes the
comparison from the raw records into a disposable validation directory. Scenario-specific
counter completeness and HEAD outcomes remain core-owned. The supplied summary's
expected identities, verdict, provenance, correctness, baseline failure count,
accessibility statuses, and complete comparison objects must match that recomputation.
Timing retains the deterministic 15% threshold and non-overlapping ranges. Native
verdicts are only `neutral`, `time-regression-advisory`, `time-improvement-advisory`, or
`inconclusive`; an inconclusive comparison cannot be admitted.

The trusted core must implement schema 3 plus per-comparison `BaseCorrectnessPassed`
and top-level `baseCorrectnessFailureCount`. If it does not, integrate the updated device
prerequisite before accepting native evidence. Do not shim schema versions, fabricate
remote metadata, weaken correctness checks, or rerun apps during interpretation.

Failed HEAD correctness blocks admission. A failed baseline Boolean does not: both sides
still require complete, valid metadata/data/counters, but baseline failures are preserved
as context. The native timing verdict is not rewritten. Downstream direct-path decisions
remain advisory when baseline behavior is not equivalent, even if native timing is neutral.

`device-validation.json` uses local envelope `schemaVersion: 2`,
`evidenceKind: manual-local-device`, and `nativeSchemaVersion: 3`. It records accepted
request keys, local paths, input checksums, measured statistics/counters, missing pairs,
errors, HEAD correctness, baseline failures, accessibility status, and coverage flags.
Checksums and `sealed` describe local validation, **not authentication, authorization,
binary provenance, or an execution attestation**. Caller-owned paths alone cannot prove
who ran an app; a PR author can fabricate JSON and hashes. The maintainer must protect the
acquisition process and evidence separately.

Valid partial/pending handoffs return exit 0 but never `deviceEvidenceComplete: true`.
Invalid submitted bundles return exit 2 with errors and no admission for those bundles.
Unsafe path/output arguments fail before writing a diagnostic artifact. No accepted
measurement, missing platform, unsupported scenario, or sampled/static path can become
whole-PR clean through completion flags alone.

## Interpretation and reporting

Freeze the evidence after acquisition/validation. Supply it read-only to `perf-analysis`,
with a separate narrative/report output directory. Do not execute commands from the run
plan, edit sources, change schemas, rebuild, revalidate by running an app, or auto-rerun
measurements. The caller separately runs `Resolve-PerfDecision.ps1`,
`New-PerformanceReport.ps1`, and `Validate-PerformanceReport.ps1` with the same selection
and local validation artifact. Managed benchmark summaries, report-policy/decision
metadata, and history keep their existing schemas; the native schema-3 migration does
not bump them.

The renderer exposes local request/summary identities and separates HEAD correctness from
baseline context. Missing/error rows remain visible, timing stays advisory, and
accessibility defaults to `not-assessed`. Optional history is a local file only. Nothing
in these entrypoints posts a comment, modifies a PR, pushes history, dispatches work, or
implicitly authorizes remote publication.
