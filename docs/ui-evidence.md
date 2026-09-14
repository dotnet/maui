# UI evidence measurements

This is a manually invoked, non-AI measurement layer for comparing a pull
request's merge-base and head. It builds the same trusted HostApp scenario on
Android or Windows, collects Appium and DevFlow evidence in base-head-head-base
order, and publishes a deterministic comparison with a hashed evidence bundle.

It does not invoke a model, register a GitHub trigger, post a PR comment, or make a
merge decision. AI interpretation and GitHub orchestration are an optional
follow-up, not prerequisites for building, running, or comparing evidence.

## Scenarios and coverage

The trusted registry is `eng/ui-evidence/scenarios.json`.

| Scenario | Evidence | Platforms |
|---|---|---|
| `layout-controls-smoke` | Initial Grid, stack, Border, Label, Button, Entry, and ScrollView layout | Android, Windows |
| `collectionview-smoke` | Initial CollectionView realization, sizing, and viewport layout | Android, Windows |

`Select-UiEvidenceScenarios.ps1` classifies changed product paths as direct,
sampled, or unmapped. Platform-specific paths select only the matching supported
target; iOS, Mac Catalyst, and other unsupported implementations do not claim
Android or Windows coverage. Non-product changes do not select scenarios.

These are curated smoke scenarios, not proof that every changed branch or
interaction executed. For example, initial CollectionView layout does not test
every grouped-item mutation. Inspect the scenario before interpreting its
path-based coverage classification.

## Manual pipeline

Register `eng/pipelines/ci-ui-evidence.yml` through the existing approved Azure
DevOps mechanism in `dnceng-public/public`. Both `trigger` and `pr` are `none`.
This layer requires the configured build/run pools, SDKs, Appium drivers, and
Windows desktop or Android emulator prerequisites. It requires no Copilot token,
GitHub slash command, or dedicated OIDC queue bridge.

The pipeline:

1. Validates deterministic contracts and provisions the pinned DevFlow adapter.
2. Snapshots the trusted HostApp harness from the pipeline source revision.
3. Builds exact merge-base and head revisions separately, using that same harness.
4. Packages the apps and records SHA-256 manifests of their complete directories.
5. Runs `base-1`, `head-1`, `head-2`, `base-2` on one worker.
6. Publishes raw screenshots, assertions, tree/layout evidence, and run identities.
7. In a separate clean job, validates provenance, compares repeats, and seals the
   final `ui_evidence_sealed` artifact.

PR-controlled builds and apps receive no GitHub or Azure DevOps write
credentials. The clean comparison job does not execute either app.

### Prepare a request

In a trusted checkout, resolve the PR's full head SHA and merge-base. Fetch the
PR into a separate ref rather than switching the trusted checkout to PR code.
Write the paths from `git diff --name-only <merge-base> <head>` to a UTF-8 file.
Then run, using PowerShell 7:

```powershell
$baseSha = '<40-character merge-base SHA>'
$headSha = '<40-character PR head SHA>'
$harnessSha = git rev-parse HEAD

pwsh eng\scripts\Select-UiEvidenceScenarios.ps1 `
  -ChangedFilesPath artifacts\ui-evidence\changed-files.txt `
  -BaseCommitSha $baseSha `
  -HeadCommitSha $headSha `
  -HarnessSha $harnessSha `
  -PullRequestNumber 12345 `
  -OutputPath artifacts\ui-evidence\selection.json

pwsh eng\scripts\New-UiEvidenceRequests.ps1 `
  -SelectionPath artifacts\ui-evidence\selection.json `
  -OutputPath artifacts\ui-evidence\requests.json
```

Selection exits `0` when requests are ready and `3` for an explicit no-op or
incomplete selection. Inspect `selectionStatus` and `coverage` before proceeding.
`requests.json` is always an array when generated for a ready selection.

Queue one manual pipeline run for each request, with these parameters:

| Pipeline parameter | Request property |
|---|---|
| `prNumber` | `pullRequestNumber` |
| `requestKey` | `requestKey` |
| `baseCommitSha` | `baseCommitSha` |
| `headCommitSha` | `headCommitSha` |
| `registrySha256` | `registrySha256` |
| `scenarioId` | `scenarioId` |
| `platform` | `platform` |
| `coverage` | `coverage` |

Use the request's `harnessSha` as the pipeline source version. A different
pipeline revision or registry changes the request identity and is rejected.

## Local tooling

Run the contracts and runner tests without the optional AI layer:

```powershell
pwsh eng\scripts\UiEvidence.Tests.ps1
dotnet test src\Controls\tests\UiEvidence.Runner.Tests\Controls.UiEvidence.Runner.Tests.csproj
```

Provision the pinned DevFlow Driver and source adapter:

```powershell
pwsh eng\scripts\Provision-UiEvidenceDevFlow.ps1 `
  -OutputDirectory artifacts\ui-evidence\devflow-feed `
  -SourceOutputDirectory artifacts\ui-evidence\devflow-source `
  -WorkingDirectory "$env:TEMP\ui-evidence-devflow" `
  -DotNetPath .dotnet\dotnet.exe

pwsh eng\scripts\Validate-UiEvidenceDevFlowFeed.ps1 `
  -FeedDirectory artifacts\ui-evidence\devflow-feed `
  -ExpectedCommit 27bc75807b10b54805e30f29af6f20df2e62d631

pwsh eng\scripts\Validate-UiEvidenceDevFlowSource.ps1 `
  -SourceDirectory artifacts\ui-evidence\devflow-source `
  -ExpectedCommit 27bc75807b10b54805e30f29af6f20df2e62d631
```

`Invoke-UiEvidenceBuild.ps1` builds one variant in its current checkout. For an
actual comparison, use separate pinned base/head checkouts with the same trusted
HostApp overlay, as in `ui-evidence-build-job.yml`; building the current branch
twice is not a PR comparison. The evidence HostApp is disabled by default and is
enabled only with `EnableMauiUiEvidence=true`.

For already-built apps, use `New-UiEvidenceBuildMetadata.ps1` and
`Prepare-UiEvidencePayload.ps1` to create the paired payload. Build
`Controls.UiEvidence.Runner.csproj` with `MauiUiEvidenceDevFlowEnabled=true`,
the provisioned `MauiUiEvidenceDevFlowVersion`, and the feed supplied through
`RestoreAdditionalProjectSources`. Then start Appium and invoke:

```powershell
pwsh eng\scripts\Invoke-UiEvidenceRuns.ps1 `
  -PayloadRoot artifacts\ui-evidence\payload `
  -RunnerPath artifacts\bin\Controls.UiEvidence.Runner\Release\net10.0\Controls.UiEvidence.Runner.dll `
  -OutputDirectory artifacts\ui-evidence\local-result `
  -LogDirectory artifacts\ui-evidence\local-logs `
  -DeviceId '<explicit Android device id>' `
  -DevFlow `
  -CaptureOnly
```

Omit `DeviceId` for Windows. Select the intended target explicitly; do not reuse
an unrelated running app. Output and log directories are replaced by the runner,
so use dedicated fresh paths containing no files that need to be retained.

Use `Complete-UiEvidenceComparison.ps1` in a clean comparison environment with
`RawEvidenceRoot`, `PayloadRoot`, `RunnerPath`, and a fresh `OutputDirectory`.
It replaces request/registry data with trusted payload copies, compares, and
seals. `Validate-UiEvidenceBundle.ps1` checks the resulting bundle hashes and
optional expected request key/head SHA. Local execution beside PR code does not
reproduce the isolation of the separate pipeline comparison job.

## DevFlow compatibility boundary

The in-app service is compiled from
`dotnet/maui-labs@27bc75807b10b54805e30f29af6f20df2e62d631` against the exact
MAUI source under test, rather than a separately packaged MAUI version. The
adapter aligns supporting package versions, disables Windows cross-process UI
Automation and XAML source-map generation, and retains managed tree and layout
diagnostics.

Appium is the independent native/visual oracle. DevFlow supplies bounded,
redacted structural evidence; symptom locations are not causal framework source
attribution.

## Results and limits

The sealed bundle contains `comparison-summary.json`, request and payload
identities, the four run results, screenshot evidence, and DevFlow tree/layout
evidence. Results are advisory:

| Verdict | Meaning |
|---|---|
| `head-functional-failure-advisory` | Both head runs repeat a functional failure while base passes |
| `visual-change-advisory` | Repeated screenshots differ beyond the observed noise and scenario threshold |
| `layout-change-advisory` | Stable new DevFlow layout findings appear on head |
| `inconclusive` | Evidence, coverage, stability, environment, or isolation is insufficient |
| `no-difference-observed` | Complete, direct, stable evidence observed no repeatable difference |
| `not-applicable` | No applicable evidence was measured |

Missing evidence, sampled coverage, environment mismatches, and unstable repeats
cannot produce an absence-of-change conclusion. Windows apps share the worker
identity with the driver, so Windows absence-of-change is always downgraded to
`inconclusive`; positive changes can still be reported. Android runs in an
emulator, but even a direct Android result covers only the selected scenario.

There are currently no iOS/Mac Catalyst scenarios, multi-page journeys, or
automatic causal attribution. A successful script exit means the artifact was
written, not that the PR has no regression. This result is advisory and is not a
merge gate.
