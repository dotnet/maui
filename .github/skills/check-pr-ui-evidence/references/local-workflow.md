# Manual local workflow

These steps replace any hosted orchestration. Execute them only for the bound
user request, keeping all outputs local. Read `docs/ui-evidence.md` for schemas,
known compatibility limits, and verdict interpretation.

## 1. Bind inputs and prepare an inert context

Work from a clean trusted harness checkout, not a PR-controlled checkout. Obtain
the requested PR's repository, full head SHA and historical merge-base through
read-only metadata/git operations. Inspect changed code before executing it.
Fetch only required refs into unique local refs; do not overwrite branches or
switch the trusted checkout to the PR.

Use an explicit historical base for merged PRs; do not infer it from today's
branch tip. Confirm the diff is the intended PR diff.

The following PowerShell variables must be bound before running the examples:

```powershell
$repo = '<absolute trusted repository path>'
$work = '<fresh local experiment directory outside the PR checkouts>'
$pr = 38341
$baseSha = '<full historical merge-base SHA>'
$headSha = '<full PR head SHA>'
$harnessSha = '<full trusted harness commit SHA>'
$platform = 'android'
$scenario = 'collectionview-smoke'
$deviceId = '<explicit owned emulator serial>'
$session = Join-Path $work 'session'
$ErrorActionPreference = 'Stop'
```

Keep the changed-file input outside the new context directory. Reject paths
containing line breaks rather than changing their meaning:

```powershell
New-Item -ItemType Directory -Path $work | Out-Null
$paths = @((& git -C $repo diff --name-only --no-renames -z $baseSha $headSha) -split "`0" |
  Where-Object { $_ -ne '' })
if ($LASTEXITCODE -ne 0) { throw 'Could not read the pinned diff.' }
if (@($paths | Where-Object { $_ -match "[`r`n]" }).Count -ne 0) {
  throw 'Changed paths containing line breaks are unsupported.'
}
$changedFiles = Join-Path $work 'changed-files.txt'
$paths | Set-Content -LiteralPath $changedFiles -Encoding UTF8
pwsh -NoProfile -File "$repo\eng\scripts\New-UiEvidenceContext.ps1" `
  -ChangedFilesPath $changedFiles -BaseCommitSha $baseSha -HeadCommitSha $headSha `
  -HarnessSha $harnessSha -PullRequestNumber $pr -OutputDirectory $session
if ($LASTEXITCODE -notin @(0, 3)) { throw 'Context preparation failed.' }
```

Stop if `selectionStatus` is not `ready`. Read the coverage counts and explain
unmapped or unsupported work. Otherwise select the explicitly requested
scenario/platform, not the first entry:

```powershell
$requests = @(Get-Content (Join-Path $session 'requests.json') -Raw | ConvertFrom-Json)
$matches = @($requests | Where-Object {
  $_.scenarioId -eq $scenario -and $_.platform -eq $platform
})
if ($matches.Count -ne 1) { throw 'The requested scenario/platform is not uniquely selected.' }
$request = $matches[0]
$requestPath = Join-Path $work 'request.json'
pwsh -NoProfile -File "$repo\eng\scripts\New-UiEvidenceRequestManifest.ps1" `
  -RequestKey $request.requestKey -PullRequestNumber $pr `
  -BaseCommitSha $baseSha -HeadCommitSha $headSha -HarnessSha $harnessSha `
  -RegistrySha256 $request.registrySha256 -ScenarioId $scenario `
  -Platform $platform -Coverage $request.coverage -OutputPath $requestPath
if ($LASTEXITCODE -ne 0) { throw 'Request identity validation failed.' }
```

## 2. Check local prerequisites and compatibility

Inspect `global.json`, project TFMs, installed SDK/workloads, Appium and driver
versions, the explicit device, and free local ports. Do not install dependencies
speculatively; diagnose missing-tool failures and request any required human
permission first. Do not change registry or machine-wide settings.

For Android, confirm `adb -s <serial> emu avd name` identifies the intended
dedicated emulator. A running device used by another investigation is not a
fallback target. Bind the build process's `AndroidSdkDirectory` and
`JavaSdkDirectory` to the verified installed SDK/JDK when stale discovery settings
would select another location. On Windows an excessively long inherited PATH
can break `cmd.exe` children; use a bounded process-local tool PATH if required.

Provision the Driver package and pristine source adapter with
`Provision-UiEvidenceDevFlow.ps1`, supplying fresh output/source/work directories
and the explicit `DotNetPath`. Validate them with
`Validate-UiEvidenceDevFlowFeed.ps1` and
`Validate-UiEvidenceDevFlowSource.ps1` against `eng/ui-evidence/devflow.json`.
Reuse a local pinned source checkout through `-SourceRoot` only when its exact
commit is verified. Do not disable validation to reuse a modified adapter.

Known stop conditions from local QA:

- The adapter currently pins Microsoft.Extensions to `10.0.0`. Product revisions
  requiring a newer version can fail `NU1109`; this is a harness compatibility
  failure, not a product regression.
- The trusted HostApp entrypoint includes main-specific issue registrations.
  Older revisions may lack those types and fail `CS1061`/`CS0246`. Do not copy
  unrelated issue code or edit the measured product to fabricate a successful
  baseline.
- Broad registry paths can overstate direct coverage. A FlexLayout change does
  not become exercised merely because a Grid/stack smoke page was selected.

Report these conditions explicitly. A new harness or scenario is a separate,
reviewable change and needs a new recorded harness identity before a rerun.

## 3. Build pinned variants with the same trusted harness

Create new detached worktrees at the exact base and head. Normal `git worktree
add --detach <new-path> <sha>` materializes the index and source. Do not use an
empty `--no-checkout` directory as if it were a complete build checkout.

Snapshot only these paths with `git archive` at `$harnessSha`, then extract that
same archive over both new worktrees:

- `src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj`
- `src/Controls/tests/TestCases.HostApp/MauiProgram.cs`
- `src/Controls/tests/TestCases.HostApp/UiEvidence/`

Record the archive hash. Verify that only this trusted overlay differs from the
measured product commits. Keep the trusted scripts and output context outside the
PR-controlled checkouts. Copy the validated pristine DevFlow source into a
separate build-local source directory for each variant; its generated build
outputs must not contaminate the reusable pristine source.

Run PR-controlled builds in a separate local process. Immediately before
launching `dotnet`, clear `GH_TOKEN`, `GITHUB_TOKEN`, `COPILOT_GITHUB_TOKEN`,
`AZDO_TOKEN`, `SYSTEM_ACCESSTOKEN`, and `DOTNET_TOKEN` in that process. Never dump
the full environment to a file. Keep any read-only metadata credentials in the
separate trusted caller. This is hygiene, not a security sandbox.

From each variant checkout invoke the trusted script by absolute path:

```powershell
pwsh -NoProfile -File "$repo\eng\scripts\Invoke-UiEvidenceBuild.ps1" `
  -Platform $platform -ScenarioId $scenario -Configuration Release `
  -DevFlowFeed '<validated feed path>' `
  -DevFlowSourceRoot '<this variant pristine source copy>' `
  -LogDirectory '<fresh build log directory>'
if ($LASTEXITCODE -ne 0) { throw 'Variant build failed; do not proceed to capture.' }
```

Never substitute another revision's app. For prebuilt apps, verify their exact
source/harness identity and complete artifact provenance before proceeding.

## 4. Package and build the external runner

For each variant, call `New-UiEvidenceBuildMetadata.ps1` with its `ArtifactRoot`,
the bound `RequestPath`, `Variant`, `DevFlowManifestPath`, and an `OutputPath`
named `ui-evidence-build-metadata.json` under that artifact root. It must find
exactly one requested app and records hashes for the complete app directory,
including hidden files and directories. Linked/reparse-point paths are rejected.

Call `Prepare-UiEvidencePayload.ps1` with `BaseArtifacts`, `HeadArtifacts`,
`RequestPath`, the context's `scenarios.json` as `RegistryPath`, `DevFlowFeed`,
and a fresh `OutputDirectory`. Inspect `payload-manifest.json`.

Build the trusted external runner from the trusted checkout:

```powershell
dotnet build "$repo\src\Controls\tests\UiEvidence.Runner\Controls.UiEvidence.Runner.csproj" `
  -c Release -p:MauiUiEvidenceDevFlowEnabled=true `
  -p:MauiUiEvidenceDevFlowVersion='<version in validated devflow-manifest.json>' `
  -p:RestoreAdditionalProjectSources='<validated feed path>'
if ($LASTEXITCODE -ne 0) { throw 'Trusted runner build failed.' }
```

Resolve exactly one built `Controls.UiEvidence.Runner.dll`, excluding reference
assemblies. Do not silently choose the first matching configuration.

## 5. Execute one bounded local sequence

Start a dedicated localhost Appium server and retain its process identity and
log. `Start-UiEvidenceAppium.ps1` refuses an occupied port and requires fresh state
and log files. For Android, `DeviceId` is mandatory: the runner uses that same
serial for adb and the Appium `udid` capability. Assign an unused UiAutomator2
system port when another investigation is running. Do not reuse an unverified
server. DevFlow uses port `9223`; its adb forward uses `--no-rebind` and is removed
only after this run successfully created it.

Windows execution refuses to start while a same-name TestCases.HostApp is
running; it does not terminate existing processes. Do not kill another process
to get past this guard.

Check server readiness and exact target identity. Use new output/log directories;
packaging, capture, and comparison reject existing directories rather than
deleting them. Outputs must also be outside their input directories.
Then run:

```powershell
pwsh -NoProfile -File "$repo\eng\scripts\Invoke-UiEvidenceRuns.ps1" `
  -PayloadRoot '<paired payload path>' -RunnerPath '<trusted runner DLL>' `
  -OutputDirectory '<fresh raw evidence directory>' `
  -LogDirectory '<fresh run log directory>' -AppiumUrl '<owned localhost URL>' `
  -DeviceId $deviceId -Headless -DevFlow -CaptureOnly
if ($LASTEXITCODE -ne 0) { throw 'Capture did not complete; inspect retained logs.' }
```

Omit Android-only `DeviceId` and `Headless` arguments for Windows. Inspect the four
run records and their assertions, screenshot counts, and DevFlow status. Do not
translate "captured four runs" into four passing runs.

## 6. Compare, validate, report, and stop owned resources

From a separate trusted process invoke `Complete-UiEvidenceComparison.ps1` with
`RawEvidenceRoot`, `PayloadRoot`, `RunnerPath`, and a fresh `OutputDirectory` at
`$session\bundles\<requestKey>`. Then invoke `Validate-UiEvidenceBundle.ps1` with
that `Root`, `ExpectedRequestKey`, `ExpectedHeadCommitSha`, and an `OutputPath`
outside the sealed bundle. Check both exit codes and the returned validation.

Report `comparison-summary.json` without changing its verdict. Keep all planned
requests in `selection.json`, including platforms or scenarios that did not run;
missing bundles remain explicitly incomplete for later manual interpretation.

A hash-consistent local bundle is not an attestation of a third-party artifact.
Workstation execution does not provide the isolation of a disposable build
service. Do not claim otherwise.

Retain failed attempts separately. Only after a diagnosed transient failure may
the caller request a complete fresh sequence; never replace individual failing
samples or overwrite the first attempt. Finish by stopping only owned process
IDs and device forwards. Do not post results or queue a follow-up automatically.

`Stop-UiEvidenceAppium.ps1` requires the version-2 state written by the start
helper and verifies both PID and process start time before stopping the owned
process tree. Legacy or mismatched state cannot authorize termination.
