<!--
Canonical MAUI CI facts. Single source of truth for pipeline names/IDs, CI quirks,
deduplication, baseline comparison, and merge-readiness criteria.

Consumed by (keep these as references, do NOT re-copy the tables):
- .github/skills/azdo-build-investigator/SKILL.md
- .github/skills/review-test-failures/SKILL.md
- .github/skills/review-test-failures/scripts/Gather-TestFailureContext.ps1 (the deterministic gatherer)
- .github/workflows/ci-status-main.md
- .github/workflows/ci-status-net11.md
- .github/workflows/copilot-review-tests.md (via the review-test-failures skill)

When a fact changes (e.g. a pipeline definition ID), change it HERE only.
-->

# .NET MAUI CI Facts

Authoritative operational reference for dotnet/maui CI investigation. The
`review-test-failures` skill owns the current `/review tests` attribution rubric
and compact, pipeline-grouped comment. It compares all three pipelines with their latest five
completed runs on the PR's exact target branch (`pr.baseRefName`), then relates
failures to the PR diff. Previous PR-branch runs are not the baseline or required
coverage. Target sampling is independent of current PR build availability and is
not cut off at the PR build's queue time; missing or unreadable samples remain explicit.
If no usable current-PR results or build-failure diagnostics exist in any pipeline,
evaluation and target sampling are skipped, with a short `/azp run` request instead.

The deterministic merge-readiness fields documented below remain in the gatherer
for compatibility. They are not the current `/review tests` report policy: a
green base job alone is not causal proof, and missing evidence is reported explicitly.

## Pipelines

### Automated CI fixes

`ci-status-main` scans the three pipelines every twelve hours and creates fix
PRs against `main`, the .NET 11 servicing development branch. `ci-status-net11`
is manual-only for historical investigations after the net11-to-main promotion.
They do not file CI-scan tracking issues or failure/no-op diagnostic issues. Findings that
cannot be safely fixed remain in the run's evidence and analysis artifacts.
Manual dispatch defaults to staged (`dry_run: true`), including PR updates.

The scanners deduplicate by a stable failure fingerprint on the PR, watch the
same owned PR for up to ten attempts, and publish at most one PR operation per
run. A PR requires a real, validated source/test fix; reproduction-only and empty
hand-off PRs are not allowed. The trusted Azure DevOps validator reconciles open
CI-fix PR heads after a scanner completes, including PRs created with
`GITHUB_TOKEN` that do not emit follow-up workflow events.

`ci-status-fix` and `ci-status-fix-net11` are manual-only compatibility workflows
for older issue-linked PRs. They must not be scheduled alongside the direct
scanners. The legacy issue reconciler and locking workflow do not produce new
issues.

### Branch promotion and servicing

After the .NET 11 promotion, `main` owns .NET 11 servicing development. The
main-to-net11 and net11-to-RC2 merge routes are retired; a future servicing cut
must explicitly select its release branch rather than reuse the RC2 route.
Official nightly builds exclude the historical `net10.0` and `net11.0` branches.

For the staged .NET 12 transition described in `docs/ReleaseBranchWorkflow.md`,
literal `net11` preserves the pre-transition `main` history. Primary CI definitions
explicitly include `net11` and `candidate/*`; the official definition includes
`net11` for signed nightly, localization and API-scan routing. These changes must
be deployed before assuming that branch creation activates the new CI routes.
Do not treat the historical `net11.0` subscription target as the new branch.

Individual .NET11 fix PRs initially target `net11` for review. After approval,
a maintainer manually retargets the PR to the active versioned inflight branch
and merges it directly there; bots do not retarget or merge. Required CI must
validate the final base. `net11` receives selected release contents through the
separate candidate integration, not direct individual-fix merges. Milestone
assignment must honor the final versioned staging base instead of net11's patch.

Release-related tooling resolves the configured servicing branch independently
of feature `main`: milestone ownership and review baselines must recognize
literal `net11` and versioned candidate/inflight lanes. Credential-bearing
review provisioning refuses the new servicing/staging targets until GitHub
reports branch protection; the naming allowlist alone is not protection.
The release migration must deploy those protections separately rather than
bypass the refusal or grant extra pipeline permissions.

The daily **Forward servicing fixes** workflow prepares a draft merge PR from an
expendable automation head, not from the `net11` source ref itself. It activates
only when the feature branch has a later major and the fixes tip has CI-normalized
`ci.main` / non-stable package metadata. Candidates retain CI settings; their
servicing flip is a separate reviewed PR on automation/release before the
manual cut. The staging workflow prepares the normalized next-cycle net11-to-main PR too.
Source/target refs are read-only; automation never guesses conflict resolutions,
approves PRs, or auto-merges. Actual forward-PR CI remains a separate gate.

The **Release tracking PRs** workflow prepares the next candidate/inflight
branches and draft PR pair after a verified same-repository candidate merge into
net11. Its merged-PR trigger uses default-branch workflow code and explicitly
checks out trusted main, never PR code. A create-once release-lane receipt pins
inputs/outputs for retries. The deployed workflow stages the exact candidate merge,
proposes next-cycle CI on net11 and creates the next pair. Independently, publication
opens a servicing/stable PR targeting automation/release, before any real release.
The maintainer reviews and merges using Create a merge commit, then manually
creates the real release at that exact servicing merge. No post-cut servicing PR
is opened. CI restoration is prepared as a reviewed normalization
PR, never a direct protected net11 write. Until its pinned child is merged with
Create a merge commit, the coordinator reports waiting-for-normalization-approval,
preserves staging/current tracking SR, and withholds next-lane activation and
forward flow. Its same-repository merged-PR trigger also accepts the managed
automation/normalization route. It does not merge either reviewed PR, advance canonical
release/nightly state, or publish customer packages. Completed production
rollover prepares a draft canonical `currentSr` update after rechecking staging,
normalized fixes and replacement lanes; review/CI/merge still gate the actual
state change. Its `nightlyMode` is preserved, so the first versioned-nightly
activation still requires independently verified deployment. The signed-publication
workflow uses existing federation/internal access, waits for a reviewed servicing
merge, exact manual cut and mirror source, queues the existing signed build, then canonical
artifact preparation with pushPackages=false. Public release approvals remain.

Configured servicing progress is independent of manual release tags. Readiness
pins the first GA branch or preceding SR branch as its inventory baseline, keeps
the current/prior cycle visible, and honors human closure per source/baseline
snapshot. Tag creation, absence or movement cannot gate or advance cuts,
normalization, replacement lanes or signing preparation. Stable tag-push
milestone processing ignores the configured train; historical tag diagnostics
and explicit manual release actions remain separate.

.NET 10 SR13 publishes from `release/10.0.1xx-sr13`, independently of `main`.
Preserve its release commit and packages, and keep publication on the .NET 10 channel during the
.NET 11 transition. Do not cut over `inflight/current` or synchronize the internal
`main` mirror as part of SR13 publication; those are separate .NET 11 operations.

### Versioned inflight nightly migration

The release-branch proposal in `docs/ReleaseBranchWorkflow.md` adds support for
versioned `inflight/<band>-srN` sources without changing the existing
`ci.inflight` package stream or the internal `dotnet-maui` signing/publishing path.
Scheduled inflight runs select their eligible lane from
`eng/release-rollover.json` at a pinned public `main` commit before packing.
`nightlyMode: legacy` preserves `inflight/current`; `versioned` selects the state's
`releaseBand` and `currentSr + 1`. Unselected retained lanes skip packing.

This is a staged migration, not evidence that the new lanes are already building.
Deploy the selection and branding changes to the internal mirrored source
branches and verify signed artifacts/feed wiring before switching the canonical
mode. Creating the public refs or passing the GitHub sandbox is not sufficient.

### Engineering dependency updates

Maestro owns the `Microsoft.DotNet.*` dependencies, including the Arcade toolset
and XHarness. Update the toolset as a coordinated dependency change: keep
`global.json` and `eng/Versions.props` aligned with `eng/Version.Details.xml`,
including its source commit, and take the matching `eng/common` update and feed
changes when required. An individual NuGet version bump does not update the
tracked toolset or shared build infrastructure.

Dependabot excludes these packages to avoid partial toolset upgrades. It also
excludes `Magick.NET-Q8-AnyCPU` major version updates: version 15 changed screenshot
comparison behavior and removed Intel macOS support. A major upgrade requires
explicit validation of the screenshot baselines and supported runner
architectures; patch and minor updates remain enabled.

### Pipeline definitions

**Organization**: `dnceng-public` / project `public`
(`https://dev.azure.com/dnceng-public/public/_apis/build/...`).

| Pipeline Name | Definition ID | Purpose |
|---------------|---------------|---------|
| `maui-pr` | **302** | Main build + unit/integration validation — check this first |
| `maui-pr-devicetests` | **314** | Native `dotnet test` device tests (iOS, Android, Windows, MacCatalyst) |
| `maui-pr-uitests` | **313** | Appium-based UI tests |

**Investigation priority order**: `maui-pr` → `maui-pr-devicetests` → `maui-pr-uitests`.
Most failures are in `maui-pr`. Focus on the first failing pipeline before others.

> ⚠️ Older names like `maui-public` / `MAUI-public` / `MAUI-UITests-public` are
> **outdated**. The `ci-analysis` plugin reference doc still lists `maui-public` —
> ignore that and use the names above.

**When CI hasn't run:** Community PRs require a maintainer to trigger builds via
`/azp run maui-pr` (or `maui-pr-devicetests`, `maui-pr-uitests`). `maui-pr-devicetests`
and `maui-pr-uitests` may not run automatically depending on the changed files.

## AzDO data sources

- Primary access is **anonymous/public** REST: `builds`, `builds/{id}/timeline`,
  and `builds/{id}/logs/{logId}` under
  `https://dev.azure.com/dnceng-public/public/_apis/build/...`.
- `_apis/test/...` endpoints often redirect to sign-in anonymously. Treat them as
  **optional enrichment** only when an AzDO bearer token is available. Do not require
  them to reach a verdict.
- If a build returns **404** even with authenticated access, classify it as
  inaccessible/expired/insufficient data — do not assume unrelated or PR-caused.
- Helix work-item console output may live behind `helix.dot.net` and Azure Blob URLs.

## Setup-only retries

Shared provisioning initializes `DOTNET_CLI_HOME` under the agent's per-job
temporary directory before invoking .NET. It preserves an existing
`NUGET_PACKAGES`, or pins the original user-profile package cache when unset,
so NuGet restores and Arcade's MSBuild task imports use the same cache.
An unwritable CLI-state directory fails preparation rather than changing
permissions on the hosted agent's shared home directory.

Shared test preparation uses **at most five total attempts per setup operation**,
not five additional retries or five whole-job reruns. Azure task-level recovery
uses `retryCountOnTaskFailure: 4`, with the agent's backoff and attempt diagnostics.
Task and job timeouts can end recovery earlier; exhaustion remains a failed step.

- `eng/pipelines/common/provision.yml` covers .NET SDK/tool installation, JDK,
  Xcode selection/provisioning, Android SDK environment discovery and AVD creation.
  Its consumers include UI-test lanes (including Material3), device-test builds,
  unit/Helix preparation, integration tests and the review gate. Platform and skip
  conditions still determine which operations run.
- Android common packages, platform APIs and emulator images retry **per package**
  through `src/Provisioning/Provisioning.csproj` and
  `eng/scripts/Install-AndroidSdkPackage.ps1`. Only Azure Pipelines installs retry;
  local installs still attempt once. Each CI attempt has a fresh Android user
  cache and temporary directory. After a failed attempt, the installer discovers
  the SDK path and removes only the failed package directory and the SDK's `.temp`
  staging directory before retrying. Successful packages are not reinstalled.
  The enclosing Azure task does not retry, avoiding a five-by-five multiplier.
- Simulator-runtime installation keeps its existing CoreSimulator recovery, but
  counts the initial download toward the five-attempt cap and fails on exhaustion
  or an unusable runtime instead of continuing with a warning.
- Shared UI/device/template/integration setup also retries artifact downloads and
  separate .NET SDK/workload/tool installation. UI setup additionally retries Node
  and Appium installation. Windows Helix-device preparation and the legacy
  handler/template paths use the same task retry count.
- The Cake UI runner retries its explicitly separated preparation/build phases
  before invoking the test runner: iOS device preparation, Android APK installation,
  Windows test paths, and UI-test project builds on all four platforms. Local
  runs and non-UI Cake targets keep their existing behavior.

This is **not** a retry based on an absent TRX or a zero test count. Test commands,
NUnit fixture setup/teardown, result reconciliation, publication, and existing
test-execution retries are unchanged. Android emulator boot, independent review
gate preparation, and provisioning inside Helix/XHarness workers retain their
existing policies; shared agent setup does not wrap or resubmit those test runs.

## Enumerate EVERY failed leg — Build Analysis is NOT exhaustive

> 🚨 The single most common way to be **wrong** about "what's unique to this PR" is to
> answer from an incomplete failure list. Build a complete inventory first, then classify.

- **Build Analysis `unmatchedFailures` is not a complete failure list.** It curates
  test-like failures and recognized error strings; it routinely **omits whole failed build
  jobs** — crossgen2/ReadyToRun (R2R), NativeAOT/ILC, the linker, `pack`, and plain MSBuild
  `error` legs. Never treat it as the exhaustive set of what failed.
- **Enumerate every failed timeline record** (`result == failed`) and open the log of
  **each** one — including records whose structured `issues[]` array is empty
  (`issues == 0`). A failed record with zero structured issues is **not benign**: the real
  error is in the **log**, not the `issues[]` array. `Build <platform> (Debug/Release)` and
  `Build Microsoft.Maui.sln` legs are exactly where build breaks hide.
- **Not every failure is an xUnit `[FAIL]` test.** A broken build job has **no test name** —
  it shows up as `error <CODE>:` or `... : error : ...` lines (e.g. crossgen2
  `Failed to load assembly 'Microsoft.Maui'`). A test-name-only search will miss it entirely.
- Use `azdo_search_timeline` with `resultFilter=all` to get every record's result + logId,
  then open each failed leg's specific `logId`. A bare `azdo_search_log` without a logId
  only ranked-searches a subset of logs, so **0 matches there is inconclusive**, not "clean."
- Do not answer "nothing is unique to the PR" until you have confirmed you inspected
  **all** failed legs. Absence of evidence is not evidence — it is usually an unread log.

## MAUI-specific quirks

### Windows packaged device-test deployment

The Helix Windows runner installs the test MSIX and its x64 dependencies in one
`Add-AppxPackage` operation with `ForceApplicationShutdown`. On disposable workers,
Windows apps such as Web Experience can hold shared runtime dependencies open and
block deployment with `0x80073D02` before any tests start.
[Microsoft documents](https://learn.microsoft.com/powershell/module/appx/add-appxpackage#-forceapplicationshutdown)
that this switch shuts down processes associated with the package **or its
dependencies**; `ForceTargetApplicationShutdown` only covers the target package.
Keep this behavior scoped to the Helix runner, not local developer deployment.
Deployment errors still fail the work item and print the Appx deployment log.

### Android activity isolation

External-intent fixtures must remain scoped to the test package and an unhandled
action after component sanitization, rather than opening the resolver or system
settings. Assert that the original activity is `RESUMED` before and after
navigation. `HasWindowFocus` is not a lifecycle check: the notification shade can
take focus while that same activity remains resumed.

### XHarness exit-0 blind spot

Windows device categories also require complete, parseable xUnit output. The
Windows runner fails empty/malformed results and abnormal process exits, while
preserving valid category results for upload. WinUI `Application.Exit()` may
return `-1` normally; that code alone is not a crash. A successful build or a
partial merged XML does not prove every category executed.
The packaged launcher retains a native process handle before waiting: Windows
PowerShell's .NET Framework cannot read `ExitCode` afterward without that handle.
An unavailable exit code must not be treated as a successful exit.

XHarness (iOS/Android device tests in `maui-pr-devicetests`) **exits with code 0 even
when tests fail**. So the AzDO job shows ✅ "Succeeded", `ci-analysis` may report no
failures, but real failures are hidden inside the Helix work items.

**Detect hidden failures** via the Helix per-job **work-items** endpoint:

```
GET https://helix.dot.net/api/2019-06-17/jobs/{correlationId}/workitems
```

> ⚠️ The older `/aggregated` endpoint returns **HTTP 404 anonymously** (verified against
> live maui jobs) — it is unreachable from the gh-aw runner, so do not rely on it. The
> per-job `/workitems` endpoint **is** reachable anonymously and returns one entry per
> work item, each carrying an `ExitCode` (int) and `State` (`Finished`/`Running`/...).
> Job detail (`GET .../jobs/{correlationId}`, also anonymous) supplies `InitialWorkItemCount`
> and the job-level `Finished` timestamp.

A work item **failed** when it `Finished` with a non-zero `ExitCode`, even when the AzDO
build job is green. Always cross-check this for `maui-pr-devicetests` when a job is green
but device-test failures are suspected (or the PR carries `s/agent-gate-failed`). If Helix
work-item data is absent, state that device-test hidden failures could not be verified —
do not assume green = clean.

> 🔎 **Job correlation is from the build's own logs, not a Helix query.** The Helix job
> `Build` property is **blank** for maui, so jobs cannot be correlated to an AzDO build via
> Helix; a `Source`+time-window query over-discovers (it mixes the concurrent `maui-pr-uitests`
> and `maui-pr-devicetests` jobs of the *same* PR, and even other builds in the window). The
> gatherer instead scans **every** `Run DeviceTests` leg's log (green **and** failed) for its
> `Sent Helix Job ... /jobs/{id}/workitems` line — a green leg's hidden work-item failure
> would otherwise never be discovered.

**Deterministic enforcement in `/review tests`.** Because XHarness exit-0 makes a green
device-test check untrustworthy, the gatherer **force-inspects every device-test build**
(green or not) and only treats a green `maui-pr-devicetests` check as clean when it can
**positively confirm `Failed == 0`** — either every discovered Helix job's `/workitems`
set was read clean, or the authenticated test-API (when a token is present).
That confirmation requires a **complete, error-free read**: every discovered Helix job's
work-item set must be read without a thrown error (a job whose read fails may have carried the
hidden failures) **and** must be *complete* — a job is left **unverified** (which caps the
verdict) when any work item is not yet `Finished` or reports no `ExitCode`, when the job
itself never `Finished`, or when fewer work items were returned than the job's
`InitialWorkItemCount`. The test-API path **pages through every test run** via the
`x-ms-continuationtoken` header and refuses to confirm when the run set was truncated (a
retried device build publishes a new run per attempt, so a failing run can sit in an unread
page tail). When no positive confirmation is available — e.g. an unreadable/incomplete Helix
read, or when the anonymous AzDO test-results API redirects to sign-in —
the green device-test check is counted as `gate.deviceTestUnverified` and **hard-caps the
verdict ceiling at `Needs human investigation`**. A green device-test check is never a
false green. (SKIPPED device-test checks did not run, so they do not cap; RED ones are
ordinary failing checks.)

**Deep work-item read (Phase 2): turn opaque device-test failures into attributable detail.**
A failed Helix work item exposes its uploaded files **fully anonymously**, so the gatherer
deep-reads them instead of emitting one opaque "hidden failure" line:

```
GET https://helix.dot.net/api/2019-06-17/jobs/{correlationId}/workitems/{workItemName}
```

returns a `Files[]` array (each `{ FileName, Uri }`) plus a `ConsoleOutputUri`. Each file
`Uri` (`.../workitems/{name}/files/{file}?api-version=2019-06-17`) responds **HTTP 302 → an
Azure blob** whose SAS token is embedded in the redirect, so following the redirect needs **no
auth** (verified live). The useful files are:

- the aggregated `testResults.xml` when present; otherwise the per-category `TestResults-*.xml`
  files (xUnit v2 `<assemblies><assembly total/passed/failed/skipped/errors><test
  result="Pass|Fail|Skip">`), which the gatherer merges. Assembly/collection-level
  `<errors><error>` nodes (fixture/cleanup crashes that carry no `<test result="Fail">`) are also
  counted as named failures so a fixture crash can't vanish. The xUnit parse runs through an
  `XmlReader` with `DtdProcessing=Prohibit` + `XmlResolver=$null`, so a malicious/garbled inline
  `<!DOCTYPE>` (billion-laughs / XXE) safely fails the parse to NHI rather than expanding.
- `console.*.log` — carries the real failure reason (e.g. `[FAIL] Timeout waiting for
  HybridWebView test results after 480 seconds`, an unhandled exception, or a non-zero exit line).
- a `*.dmp` crash dump when the host crashed.

> ⚠️ **Content-type gotcha (decode bug, fixed).** Azure blob serves the `.xml` result files as
> `application/octet-stream`, so `Invoke-WebRequest`'s `.Content` is a **`byte[]`** — a plain
> `[string]` cast stringifies it as space-joined decimal byte values (`"60 63 120 …"`) and breaks
> XML parsing. Decode bytes as UTF-8 and strip any leading BOM (which would also make
> `XmlDocument.LoadXml` throw). `console.*.log` is served as `text/plain`, so its `.Content` is
> already a string — that asymmetry is why a console read can succeed while a TRX read silently
> returns nothing if bytes aren't handled.

**This NEVER relaxes the verdict cap.** A non-zero-ExitCode work item still hard-caps the verdict
at *Needs human investigation*; the deep read only makes it **intelligible**. Real `result="Fail"`
tests become NAMED records (`source = helix-trx`) that flow through the normal dedup / base-exact-
match / known-issue attribution — so a named device-test failure that also fails on base can
dismiss as pre-existing, exactly like any other test. But an **incomplete** run ALSO emits a
capping `helix-workitem-incomplete` record **in addition to** any named failures — because a run
that didn't finish can mask a PR-caused failure in tests that never ran, so even if every named
failure dismisses on base the work item stays NHI. A run is treated as incomplete when **any** of:
the console shows it was killed/hung/crashed; a `*.dmp` crash dump **or** a crash signal (`core
dumped` / `segfault` / `.dmp`) is present (a SIGSEGV can flush a partial result file then kill the
run — so a crash forces the cap *even when some tests were named*); no result file was readable;
the result files read were themselves incomplete (a per-category file failed to download/parse, the
category list overflowed the read cap, or the file **declared** more failures than we could
extract); or the work item exited non-zero with **zero** named failures. A device-test work item is
**never** a false green.

> 🔒 **Never-false-green coupling — keep these in sync (load-bearing).** The Phase-2
> incompleteness cap narrows `isIncomplete` to markers that *prove* a run didn't finish, and that
> narrowing is **coupled to the Windows runner's exact echo strings**:
> - `[FAIL] Timeout waiting for <category> test results after N seconds` — `eng/devices/run-windows-devicetests.cmd:503` (an unfinished category)
> - `All test processes may have crashed` — `run-windows-devicetests.cmd:430` (a total wipeout)
> - `Test execution completed with exit code: N` — `run-windows-devicetests.cmd:481` is **deliberately EXCLUDED** from the cap: the cmd echoes it **unconditionally** on every non-zero run (it only ever `exit /b 0|1`), so treating it as incompleteness over-caps *every* failed Windows work item and never lets a cleanly-named failure flow to base/known-issue attribution.
>
> The first two strings are the *independent* incompleteness markers in `Get-ConsoleFailureReason`'s
> `$incompleteRegex`. **If anyone rewords them in the cmd, mirror the change in
> `$incompleteRegex` (`Gather-TestFailureContext.ps1`)** — otherwise the cap silently weakens **with
> no test failing** (the coupling is enforced only by comments + this note, not by a shared
> constant). Also: any **new** call site that reads a `console.*.log` via `Invoke-HelixFileText` to
> judge completeness **must** pass `-Truncated ([ref]$flag)` and OR that flag into the incomplete
> cap — a console read without it reopens the head-only-`[FAIL]` truncation window (a >4 MB XHarness
> console keeps only the HEAD, where per-test `[FAIL]` lines live, and drops the TAIL where the
> timeout/crash marker prints).

### Container artifact binlogs

MAUI build artifacts are **Container** type, not `PipelineArtifact`:

- `az pipelines runs artifact download` does **not** work for binlogs.
- Artifact names look like `Windows_NT_Build Windows (Debug)_Attempt1` (not `binlog`).
- Download needs a Bearer token:
  `az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798`.
- Use the ADO File Container API:
  `/_apis/resources/Containers/{id}?api-version=5.0-preview&$format=OctetStream`.

If available, the `mcp-binlog-tool` / binlog MCP server can analyze downloaded
`.binlog` files. Optional — core investigation works via `gh` CLI and REST.

### APIScan analysis inputs

The official pipeline stages unsigned build outputs in `APIScanFiles` for
analysis, separately from the signed shipping packages. Keep both the
`APIScanFiles` artifact and its APIScan consumer job marked `isProduction: false`;
otherwise production-release validation reports missing signing certificates
on these analysis-only copies. This classification does not disable `APIScan@2`
or change signing and validation of the actual release artifacts.

## Test count deduplication

**Never sum raw failed counts across test runs.** MAUI UI/device tests repeat the
same test across:

- **Runtime variants**: CoreCLR and Mono
- **Platform versions**: e.g. iOS 18.5 and iOS latest, Android API 30 and API 36
- **Retry attempts**: each retried job publishes a new test run

A single failing test can appear in 4–8+ runs. Summing inflates counts dramatically.

**Deduplicate** by grouping on **normalized test name + OS platform** (`android`,
`ios`, `macos`, `windows`, or `unknown`). "DatePicker_Format_D on iOS" and
"DatePicker_Format_D on Android" are distinct failures. Collapse retries and runtime
variants (coreclr/mono) of the same test on the same OS into one. Report retry/run IDs
as supporting evidence under the same distinct failure.

## Baseline comparison (is it already red on the base branch?)

A failure that is **already failing on the base branch** (e.g. `main`) for the same
pipeline is almost certainly **not** caused by the PR.

- Compare each distinct PR failure (by normalized test name + OS platform) against
  failures from the **most recent base-branch build of the same pipeline definition**.
- If the same `(test, platform)` key fails on the baseline build, treat it as
  **pre-existing / likely unrelated** and subtract it from PR-caused — unless this PR
  directly changes that test. (The deterministic scope guard in the gatherer name-matches
  the failing test against PR-edited **test files** only; it does **not** automatically
  detect snapshot/baseline-image or platform-source edits. When this PR touches the
  snapshot/baseline or the platform code a still-red test exercises, do **not** subtract it
  — the same-reason match is not proof it is pre-existing.)
- Base-build *result* alone is weaker evidence than a per-test match: a red base build
  tells you the branch is unhealthy; a matching red **test** tells you this specific
  failure is not yours.
- **Also diff at the JOB/leg level, not just the test level.** Build-job breaks (crossgen,
  NativeAOT, `pack`, MSBuild errors) have **no test name**, so a test-only baseline diff
  structurally cannot see them. For each **failed leg** on the PR (by normalized job name,
  e.g. `Build macOS (Debug)`), compare the **same leg's result** on the most recent
  base-branch builds.
- **"Failed on the PR, green on the base branch for the same leg" is the STRONGEST
  PR-caused signal there is** — it outranks a test-name baseline match. If `Build macOS
  (Debug)` is red on the PR but was green on the base build, the break is the PR's, full
  stop, even if Build Analysis matched nothing.
- For **flow / dependency-update PRs** (`dotnet-maestro[bot]`): when a leg fails only on the
  PR, diff the **SDK/runtime version** between PR and base — compare the `.dotnet/sdk/<ver>`
  path in the PR's failing log against the base build's log. A bumped SDK/runtime
  (e.g. `preview.5` → `preview.6`) that introduces a crossgen/R2R or linker break is a
  PR-caused regression carried in by the dependency, not a pre-existing flake.
- If baseline data is missing or the base build is inaccessible, say so — do not assume
  a failure is pre-existing without evidence.

## Visual baseline failures

Messages like `Baseline snapshot not yet created`, missing snapshot paths, or snapshot
environment-version mismatches are strong **unrelated** evidence — unless the PR adds or
modifies that visual test or the affected snapshot/platform.

The gatherer can collect failed UI result IDs from the public
`vstmr.dev.azure.com/.../testresults/resultsbybuild` endpoint, then reads the public
result-detail and attachment APIs. The simplified workflow/local runner skips
visual enrichment and no longer publishes image panels. Earlier reports used the
asset-only `review-tests-assets-v2` branch (and previously `review-tests-assets`);
those branches and their commit-pinned image URLs remain intact. The legacy
publisher/merger scripts are retained for compatibility, not called by the current
`/review tests` workflow. Missing images do not establish a failure's cause.

## Platform mismatch

Platform mismatch is **supporting** evidence, not proof. An iOS-only test failing on a
Windows-only PR is likely unrelated when the message also points to missing iOS baseline
data — but it may still need investigation if the PR changes shared logic (e.g.
CarouselView) that runs on that platform.

## Gradle / Maven / CFSClean failures

**Error signatures** (these are build/feed issues, NOT test failures):

```
error XAGRDL0000: Could not resolve com.android.tools.build:gradle:8.11.1
  > Received status code 401: Unauthorized - No local versions of package
```
```
error XAGRDL0000: Could not GET '...pkgs.dev.azure.com/.../maven/v1/...'
  > Unauthorized - Please provide authentication to save package from upstream
```

**Fix:** run `./eng/ingest-maven-deps.sh` locally to pre-ingest packages into the feed.

**Do NOT:**
- Remove CFSClean from `ci-official.yml` — security compliance requirement.
- Upgrade Gradle past 8.x — `dotnet/android#10738`.
- Add `mavenCentral()` or `google()` back — use the Azure Artifacts feed.

## Common failure patterns

| Pattern | Where | Notes |
|---------|-------|-------|
| `error CS####` | `maui-pr` | C# compiler error — check file/line |
| `error XA####` | `maui-pr` | Android build error |
| `java_home -X` / `Root element is missing` / invalid JDK `jar` path | `maui-pr` Android builds and integration tests | Android SDK discovery can inspect broken system registrations or archive parent directories even with `JAVA_HOME` set. `ProvisionJdk` must also publish the validated JDK home as `JavaSdkDirectory` for downstream MSBuild processes. Do not suppress these diagnostics or allow-list the tooling warnings. |
| `error : ... Failed to load assembly` | `maui-pr` `Build <platform>` leg | **crossgen2 / ReadyToRun (R2R)** break — a failed **build job**, not a test, with no test name. Common after an SDK/runtime (`dotnet/dotnet`) bump on a flow PR. Job-level baseline diff: red on PR vs green on base ⇒ PR-caused. |
| `error IL####` / `ILC####` / NativeAOT publish fail | `maui-pr` AOT legs, `Run Integration Tests – AOT` | **NativeAOT / ILC** trim-analysis break. May be pre-existing (e.g. HybridWebView `IL2026`) — confirm with a job- AND test-level baseline diff before attributing to the PR. |
| `error NETSDK1144` | `maui-pr` TrimFull legs | Optimizing assemblies for size failed (often an ILLink warning promoted to error). Check whether the same leg is red on the base branch. |
| `XamlC` | `maui-pr` | XAML compiler — usually missing type or bad binding |
| `WMC0612` / XBF `0x09C4` / `Property Not Found` after adding native Windows `x:DataType` | `maui-pr` Windows build/pack | The native XBF generator rejects the annotations introduced by #38735 on classic `{Binding}` templates. Restore the prior metadata without replacing bindings or converters. XML/source-only checks cannot validate XBF compilation; require an actual Windows build. |
| `WMC9999` on overloaded table collections, `WMC0821` on open generic types, or invalid generated `XamlTypeInfo.g.cs` after adding native `[Bindable]` | `maui-pr` Windows build/pack | These were successive failures from #38735's new metadata in the Windows investigation on #39608. Restore the pre-cleanup native metadata baseline rather than changing public collection APIs, generic types, or generated code. Preserve existing runtime binding behavior and validate with the native Windows compiler. |
| Native exit `57005` / `0xDEAD`, with `CTitleBar::InitializeComposition` returning `E_OUTOFMEMORY` and growing composition/section handle counts | Windows device tests | Compare the native dump with [microsoft/microsoft-ui-xaml#12210](https://github.com/microsoft/microsoft-ui-xaml/issues/12210): extended title bars retain native resources after window closure, including on the pinned Windows App SDK 2.5.1. Fixture ownership corrections did not resolve the observed Controls crash. The exit code and aggregate handle counts do not exclude an in-repository owner leak: verify actual HWND destruction before attributing this solely to the SDK. Preserve real window closure and title-bar assertions; the reported reset workaround needs explicit approval and native validation, and a newer SDK report also describes reset-induced crashes. |
| Catalyst Core stops after `NativePresentationControlsInsetStabilization(style: PageSheet, ...)` | Native Core device tests | The last skipped Editor message was not the stalled test. `TEST_START:` tracing identified the native modal-presentation row. Present its owned controller from the real key window's root controller and await actual `ViewDidAppear`, with a bounded failure deadline. Dismiss only the owned modal; never hide, replace or dispose the borrowed runner window/controller. Preserve both PageSheet and FullScreen inset assertions and cover FullScreen followed by PageSheet explicitly. |
| `Add-AppxPackage` fails with `0x800B0109` after trusting a job certificate in `CurrentUser\TrustedPeople` | Packaged Windows device tests | [MSIX deployment guidance](https://learn.microsoft.com/windows/msix/desktop/managing-your-msix-deployment-troubleshooting#step-3-verify-prerequisites) requires a chain trusted by the local computer, not just the current user. Use an accepted signer or explicitly approved agent provisioning. Do not silently change machine trust, export private keys, or substitute unpackaged execution. |
| `error XAGRDL0000` / `401` / `No local versions` | `maui-pr` or official build | Gradle/Maven feed issue — see above |
| Apple workload install cannot find a `net10.0` SDK pack | `maui-pr` SDK provisioning | The .NET 11 Apple manifests also install .NET 10 compatibility packs. Resolve the exact missing version's feed with `darc get-asset`; the current .NET 11 feed alone may not contain it. Do not copy another branch's compatibility feed without checking its manifest versions. |
| `NETSDK1140` rejects the Apple target platform version | `maui-pr` build/pack | Align current Apple target-framework versions and SDK defaults in `Directory.Build.props` with the selected Apple SDK. net11.0 uses 26.5; the RC2 release branch uses 27.0. Preserve previous-.NET compatibility targets. |
| Xcode 27 rejects native iOS 12.x / mapped macOS 10.15 deployment targets | `maui-pr` native interop archive | On branches using Apple 27, align the native framework and managed iOS minimum to 15.0, already used by app templates. This maps to macOS 12.0 for Mac Catalyst. Do not raise minimums on Apple 26.5 branches solely to match RC2. |
| Apple `bgen.dll` exits 150 requesting a newer .NET runtime | `maui-pr` Apple binding generation | Keep bootstrap (`global.json`), workload provisioning (`MicrosoftNETSdkPackageVersion`), and SDK provenance pins aligned with a compatible host runtime. SDK `11.0.100-rc.2.26470.103` satisfies the Apple 27 generator's `26465.108` requirement and is already selected on net11.0. |
| Resizetizer raster test assertions pass but `Dispose` fails with a Windows sharing violation | Helix Windows unit tests | Keep temporary images under `BaseTest.DestinationDirectory` and reuse its bounded, logged cleanup retries from #38124. Dispose raster tools deterministically; preserve all image assertions and let exhausted cleanup retries fail the test. |
| `Svg.Animation` / `Svg.SceneGraph` `NU1603`, or `SKPathBuilder` `CS0246` | `maui-pr` restore/build | Release merges can preserve old `eng/Versions.props` pins while importing Skia 4 source and packaging changes. Restore the coordinated SkiaSharp/HarfBuzz/SVG dependency versions, not just the first missing package. |
| Required Xcode is missing / requested iOS runtime is unavailable | `maui-pr` macOS provisioning | Select an image or agents providing the branch's required Xcode. net11.0 retains Aces with Xcode 26.5; RC2 needs the MAUI pool's Xcode 27 RC agents. Do not fall back to an unrelated Xcode or purge runtimes to repair a version mismatch. The legacy Provisionator task is unavailable in `dnceng-public`; enabling it prevents pipeline startup. |
| `iOS 27.0 (universal) is not available for download` | `maui-pr` simulator provisioning | Request the `arm64` runtime on Apple Silicon agents; retain `universal` on Intel. Keep all downloads version-pinned. An unavailable download can also return exit code 70, so do not treat every exit 70 as a wedged CoreSimulator or delete installed runtimes. |
| Helix monitor cannot use `NetCore-Svc-Public` | `maui-pr` release branches | The monitor must use the pipeline's selected Helix submission pool, with its Linux image demand, rather than independently selecting an unauthorized pool from the branch name. |
| `XHarness timeout` | `maui-pr-devicetests` Helix logs | Test killed by infrastructure; may be transient |
| `No test result files found` | `maui-pr-devicetests` Helix logs | Tests never ran or app crashed on launch |
| UI test screenshot diff | `maui-pr-uitests` | Visual regression; check baseline images |
| Copilot PAT selection reports every model unavailable almost immediately | Skill evaluation | A failed CLI probe is not proof of missing model entitlement. Copilot CLI 1.0.105 rejects the removed `--no-remote` option before authentication or inference; `--no-remote-export` is still accepted. Verify parser compatibility before changing credentials or models. The evaluator runs its token selector from trusted `main`, not the PR's copy. |

### Native Windows resource evidence

The temporary cleanup probe in [device run 1671453](https://dev.azure.com/dnceng-public/cbb18261-c48f-4abb-8651-8cdcb5474649/_build/results?buildId=1671453),
log 937, reported `IsWindow=false` after every tracked window closure. Despite
that destruction, process handles rose from 1,304 to 19,052 and GDI objects from
144 to 15,834, while USER objects stayed between 39 and 56. Controls still exited
`57005` while `SingleShellSectionWithMultipleChildren` was active.

These observations rule out those HWNDs simply being left open. Together with
the earlier symbolized `CTitleBar::InitializeComposition` allocation failure,
they support investigating native title-bar resource retention, but do not
identify every remaining managed or native owner. The probe is not a fix and is
removed after capture. Do not substitute forced collection, raised quotas,
skips, or disabled title bars for a verified lifetime correction.

An independent repository-owned visual-root retention was subsequently reproduced
in [device run 1672255](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1672255),
log 553: `UnloadedWindowRootViewCanBeCollected` reached its collection assertion
after observing native `Loaded`, `Unloaded` and window `Closed`, but the
`WindowRootView` stayed alive. Its `UISettings.ColorValuesChanged` subscription
was added on every template application and never removed. Scope that subscription
to `Loaded`/`Unloaded`, refreshing current colors on reload rather than accumulating
template subscriptions. The unchanged native regression passed in
[device run 1672279](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1672279),
test run 46605876, result 100104; Windows Core completed with 2,082 passes and
111 existing skips. This proves release of that root, not resolution of every
native SDK resource leak or the full Controls crash.

Windows overlay teardown must clear the owned graphics view's `Drawable`
reference before removing that view, as well as unsubscribing borrowed frame
and platform-element events. A native graphics control retained by WinUI/Win2D
must not keep the removed overlay and its window alive through that drawable.
The native overlay regression retains the graphics view, checks that its
drawable is detached, and preserves the original overlay collection assertion.
Popping a Windows modal must likewise disconnect its full handler graph, not
only the modal's top-level handler; the FlyoutPage modal regression checks both
child-page handlers as well as the original collection assertions.

Native hosts default xUnit's `--long-running` threshold to 60 seconds and enable
`--xunit-diagnostics on`, unless those options were supplied explicitly. These
are liveness diagnostics, not test deadlines, retries or result evidence. Add
them inside the headless host: the native SDK proxy does not expose every xUnit
option. The native message consumer also logs `TEST_START:` for each in-progress
node, so a runtime or UI-thread stall does not depend on a diagnostic timer
continuing to run. These messages never supply result counters; genuine TRX and
the process exit code remain authoritative.

Windows fixtures must close their owned native window before fallback handler
and scope teardown, so `Destroying` sees the live handler and scoped services.
Windows window destruction must disconnect the page's handler graph before
disposing those services. In
[device run 1672454](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1672454),
log 692, a pending `StackNavigationManager.OnNavigated`/`Loaded` callback arranged
a still-connected page after its service provider had been disposed. The resulting
`ObjectDisposedException` terminated WinUI with `0xDEAD` after 65 passes; that
failure is distinct from the earlier native title-bar allocation failure.
Do not suppress the exception or replace native close with an arbitrary delay.

The Windows native host's existing unhandled-exception hooks attempt one
full-memory dump of their own process on a dedicated worker before termination.
The dump includes handle and thread information and is written inside the
unique result directory; a pending file is renamed only after a successful
write. Capture has a bounded diagnostic wait, reports failures explicitly, and
does not mark the original exception handled. This supplements MTP's crash
collector, which did not produce its expected dump for these WinUI terminations.
With `--diagnostic`, native test-start notifications also check the current
process's GDI-object count and request that same one-shot snapshot at 8,000
objects, before a hard termination can bypass unhandled-exception callbacks.
This only records evidence: it does not change resource quotas, force collection,
reset title bars or convert a failed process into a successful test run.
Diagnostic runs also request the same one-shot snapshot when MTP publishes a
failed/error test node. This captures transient retention at the failure instead
of waiting for a later resource-pressure threshold; the original failure and
genuine TRX remain authoritative.
Windows jobs analyze any resulting owned snapshots on Windows, retaining heap
statistics and managed root paths alongside the dump. Enumeration uses exact
method tables from heap statistics, not substring matches that can select
delegates or generated closure types instead of the requested objects. Root
analysis covers every enumerated window, visual root, overlay, FlyoutPage and
ContentPage, batching commands per type to reuse the root cache and requiring
one completed root search per address. Tooling is installed
only in the job-owned temporary directory when missing, and analysis failures
remain explicit. A separate targeted ownership/focus invocation runs after the
full Controls job, so an earlier suite crash cannot hide the attachment-lifetime,
modal/overlay collection and ScrollView reload/removal/disconnect assertions.
It does not replace or filter the full-suite invocation.
In [device run 1672521](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1672521),
log 1039, the live process snapshot was captured successfully at 8,002 GDI
objects as `native-process-4008.dmp`. The same run still failed normally, so
the diagnostic capture did not hide the subsequent native termination.

In [device run 1672577](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1672577),
the two genuine attachment-lifetime rows passed in test run 46615240. The full
Controls run still failed modal ContentPage collection and then terminated with
`0xDEAD` (331 passes, one failure and seven existing skips). Its pressure snapshot
`native-process-5552.dmp` exposed a global root through
`DeviceDisplayImplementation.MainDisplayInfoChanged` to a FlyoutPage, its pages
and an older MauiWinUIWindow. This does not establish that the HWND was destroyed,
or that every native-resource owner has been identified. FlyoutPage's display
listener must require both a logical window and a live handler, removing it on
handler teardown even while the logical Window reference remains. The managed
regression failed before this change and passes for both attachment orders,
handler replacement/reconnection and window removal/readdition. Native results
remain a separate requirement; this managed proof does not establish that the
remaining modal collection failure or native termination is resolved.

In [device run 1672626](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1672626),
the complete pressure snapshot contained no exact FlyoutPage or WindowOverlay
instances. Only two of 49 MauiWinUIWindow wrappers, two of 50 WindowRootViews and
two of 51 ContentPages had GC roots; their paths belonged to the runner/current
test window. This does not establish native HWND destruction or native-resource
release. The independent eight-row ownership/focus probe passed seven rows
(including ScrollView collection, overlay collection, reload focus, removal,
stale callback and both attachment-lifetime rows), but still failed modal
ContentPage collection. That transient failure needs its own failure-time
snapshot; the later pressure snapshot does not contain its retaining path.
In run 1672685, MTP-triggered snapshots still arrived after the modal's test
method had unwound: the isolated dump's two non-runner ContentPages and test
window wrapper had no GC roots. The original assertion still failed in genuine TRX.
The modal test therefore requests the same opt-in snapshot in its assertion
exception handler before rethrowing unchanged. This is diagnostic only; it
does not add a collection, drain, delay or accepted failure.
Run 1672756 captured from that handler before the test unwound. The non-runner
child ContentPage still had no GC root. The Windows collection helper now tests
the deferred-native-release explanation by awaiting a low-priority UI-queue
barrier after finalization, as the Apple helper already drains its native UI
release queue. There is no sleep, quota change or weakened assertion. The
focused probe also includes Shell page collection and both generic/non-generic
rooted-object contrasts; native results must establish whether this candidate
addresses the failure.
That candidate passed in
[device run 1672813](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1672813):
all eleven genuine ownership/focus rows passed, including the unchanged modal
and Shell collection assertions and both rooted-object contrasts. The full
Windows Controls process still terminated with `0xDEAD` while
`SettingTitleIconImageSourceDoesntCrash` was running. MTP recovered 339 rows
(332 passes, seven existing skips, no assertion failures), but preserved exit
code 7 and reported the host crash. Those incomplete results are not a passing
suite or proof that the remaining native resource owner is fixed.
The completed matrix had 35 successful jobs and six failed jobs: unpackaged
Windows Controls terminated natively, and all five packaged Windows suites
failed their signing/deployment prerequisite. All Android, iOS and Catalyst
jobs succeeded. A separate local full pinned Catalyst run still reported 81
fixture callback timeouts (1,520 passes and 42 existing skips); two affected
methods passed fresh isolation. Do not equate that isolated result or the green
CI lanes with resolution of the local full-run failures.

The local Catalyst trace showed native navigation continuing through all ten
push/pop operations while repeated readiness calls spent 50 ms waiting even
when both managed and native state were already satisfied. Those branches now
return immediately; transition waits, the 15-second fixture deadline and all
assertions remain unchanged. Two native regressions require synchronous task
completion only for already-satisfied loaded/unloaded states. The prescribed
verification script, with the helper as its sole fix file and immutable baseline
`832e96070e2346253ef3a7a6368d24ec41938ddc`, confirmed both regressions fail
without the change and all four VisualElement cases pass with it. An initial
attempt exposed a missing Label handler registration in the new test; that
setup failure was corrected before the successful red/green run. The subsequent
complete Release Catalyst run reported 1,600 passes, three failures and 42
existing skips: none of the earlier 81 timeout failures recurred. Two lifecycle
rows checked event counts before their observed events had arrived, and one
split-Flyout row read initial geometry before its detail pane finished layout.
The lifecycle test now waits for its own event counters before the unchanged
assertions and transitions; the split test awaits positive, non-overlapping
initial pane widths before its unchanged geometry assertions. All three rows
then passed together, with one pre-existing skip. The subsequent complete
Release CoreCLR Catalyst rerun passed all 1,603 runnable rows with 42 existing
skips, no failures and a successful native launcher exit. This is local
full-suite evidence for `9dc989040e`, not proof of Windows or Android success.

Device run 1672894 exposed Android Controls ViewPager crashes on both runtimes.
A local CoreCLR run of all 13 `ShellLazyStartupTests` reproduced the native
RecyclerView termination. The fixture's `FinishedDestroying` property returned
its loading task, its completion sources could run test code inline inside
native lifecycle/layout callbacks, and cleanup disconnected the window before
removing its owning fragment. The fixture now uses asynchronous continuations,
waits for the actual fragment destruction signal and lets that native callback
deliver window destruction before final handler cleanup. The same 13 native
rows then passed with no skips; no production animation suppression, fixed
delay or assertion change was needed. The complete local Release/CoreCLR
Android Controls suite subsequently passed 1,331 rows with 13 existing skips,
no failures and matching genuine host/native reports. Both Android runtimes
still require their own CI evidence.
The same run's Essentials Mono log identified reactivation of a disposed
`ActivityLifecycleContextListener`. Test-owned listener disposal now runs on
the UI thread, after owned recreation activities actually reach `OnDestroy`.
All nine lifecycle/recreation rows and the complete 424-row Essentials suite
passed locally on CoreCLR. The local Mono invocation's cross-target restore
also evaluated `net11.0-ios`, which the SDK rejects for Mono (`NETSDK1242`);
that invocation is not Mono execution evidence.

Run 1673020 completed 32 of 41 jobs successfully. Its additional Windows Core
termination was not attributed to title-bar pressure: log 928 records
`MauiWebView.LoadHtml` dereferencing a null `CoreWebView2` after initialization
reported `COMException (0x80010108)`. Navigation now captures and checks the
initialized native object for HTML loading, local-URL mapping and navigation
callbacks, logging explicitly if no native object remains. Local cross-host
build reached the Windows-only `MakePri.exe` prerequisite; it did not establish
native Windows correctness.
The guarded code subsequently passed the complete native Windows Core suite in
run 1673078, log 943: 2,082 passes, 111 existing skips, no failures and a
successful launcher exit. This resolves that concrete null-dereference
verification, not the distinct Windows Controls resource problem.

The iOS initial-inset assertion now waits for the real safe-area API value
before its unchanged final assertion; all three focused iOS 27 CoreCLR rows
passed. Two subsequent complete local General runs passed 1,289 rows with 14
existing skips. However, an earlier local run and both General jobs in run
1673020 terminated without genuine TRX. The local simulator log records
`SIGABRT` after Carousel tests; the last test and its complete ten-row class
both passed in fresh isolation. The full-pass repetitions do not establish an
abort fix or justify accepting missing results. Temporary app exception hooks
were removed; SDK launch logging was used for further diagnosis without
changing exception modes or native failure handling.
The same iOS General shard also passed under debugger-only native exception
capture (1,289 passes, 14 existing skips); no exception breakpoint fired.
The debugger passed the runtime's normal `SIGUSR1` suspension signal through
unchanged. This diagnostic run does not explain the earlier aborts.
Run 1673078's iOS CoreCLR Shell shard instead reported one real assertion
failure: the flyout-disposal test inspected header/footer native parents before
attachment. It now awaits those exact parent relationships before all unchanged
attachment and disposal assertions. The complete local Shell shard then passed
all 507 native rows without skips.
Two further native iOS fixture regressions demonstrated independent ownership
defects: logical window destruction preceded native content removal, and the
disconnect completion could resume callers before logical destruction.
Both failed against the original fixture and passed after moving Apple window
destruction into the native teardown owner and exposing its actual asynchronous
disconnect task. The task also propagates teardown exceptions to the awaiting
fixture instead of losing them through `async void`. This is direct lifecycle
ordering evidence, not proof of the earlier intermittent abort's cause.
The complete unfiltered local iOS Controls suite then passed 1,978 rows with
23 existing skips and no failures (2,001 total), including all eight inherited
rows for the two teardown regressions and the Shell attachment check. This
native CoreCLR witness produced genuine TRX and a successful launcher exit;
one complete pass still does not establish that the intermittent abort is gone.
The same native teardown change also passed the complete Catalyst Controls
suite: 1,611 passes, 42 existing skips, no failures and a successful launcher
exit (1,653 total, including the eight new inherited regression rows).
Run 1673173 exposed a separate Core presentation-test lifetime: the native
scroll view intentionally references its cross-platform layout weakly, while
the test's last strong use preceded native layout and its assertion. The test
now keeps that owner alive through native removal with `GC.KeepAlive`, without
changing production ownership or collecting garbage. Genuine CI TRX reported
2,613 passes, one zero-content-height assertion and 40 existing skips; the
complete local native iOS Core suite after the lifetime fix passed 2,614 rows
with the same 40 skips (2,654 total).
That run's Mono General simulator log also captured the previously missing abort
stack: `CustomDataTemplateSelectorSelector.OnSelectTemplate` unboxed a null item
from `ItemsViewController2.DetermineCellReuseId`, reached through the carousel
controller. This is a concrete managed exception crossing UIKit, not evidence
of an unexplained runtime-only abort. A real-detachment regression then passed
without looping and failed with looping: removing the native view discarded the
loop-index mapper while UIKit could still request cells. The mapper now remains
owned until controller disposal, without retaining new native objects or changing
template selectors. All 12 CarouselView rows passed on native iOS CoreCLR and
Catalyst. The complete native iOS Mono General shard also passed 1,299 rows with
14 existing skips (1,313 total), including the former crashing selector and both
new mapping rows. Switching local runtime flavors required a full rebuild;
the first incremental Mono attempt aborted at startup with a mismatched
`System.Private.CoreLib` AOT module and is not counted as test coverage.
The Windows fixture now captures each owned HWND before running its test and
requires the real `IsWindow` API to report its destruction after native closure,
before releasing the handler/window scopes. Successful checks log that exact
handle and process. This stricter native teardown check needs Windows CI
verification; it neither proves native title-bar/resource release nor treats
managed `Closed` or garbage collection as evidence of HWND destruction.

The shared NUnit/Appium MTP caller also has native execution evidence in
[UI run 1670623](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1670623):
123 genuine test runs contain 15,140 rows, with 14,852 passes, 267 skips and
21 failures. This establishes that the earlier invalid-argument startup failure
no longer prevents execution, not that the UI suite is green. Four jobs failed:
three Catalyst visual comparisons, one iOS title-view rotation assertion, and
17 Windows TimePicker rows led by an unsupported mouse-pointer action. The
Windows action source was corrected later in `f0775ea468`; that earlier run
does not validate the correction. These remaining failures are not classified
as pre-existing or infrastructure without corresponding baseline evidence.

In [device run 1672389](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1672389),
log 719, PageSheet's `PresentViewControllerAsync` task did not complete within
30 seconds even with an active application, foreground-active scene, key visible
window, attached presenter and no existing presentation or transition. The native
call itself returned, and the subsequent dismissal completed. Both the individual
PageSheet row and explicit FullScreen-then-PageSheet sequence failed; the suite
continued to a genuine TRX with 2,301 passes, two failures and 185 existing skips.
This rejects a missing active presenter or presentation order alone as the full
explanation. Waiting for the owned controller's native appearance event verifies
the lifecycle needed by the inset test without depending on that completion
callback. Local pinned-SDK results do not establish that the CI failure is fixed.
The actual native correction subsequently passed in
[device run 1672414](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1672414),
test run 46610588: 2,303 passes, no failures and 185 existing skips, including
both native styles and the explicit FullScreen-then-PageSheet sequence.

## Merge-readiness criteria

Retained for the interactive investigator and legacy deterministic gate consumers.
The current `/review tests` skill reports attribution and evidence coverage instead
of these merge-readiness verdicts. Assess **only CI/test health** — code review and
approval are separate, human-only decisions.

| Overall verdict | Use when |
|-----------------|----------|
| `Ready to merge` | No failing checks, OR every distinct failure is confidently `Likely unrelated` (infra, missing baselines, known flake) or matches a baseline failure on the base branch. |
| `Not ready` | At least one distinct failure is `Likely PR-caused` — references changed files/tests/APIs/platform, or appears only on a path/platform this PR changes and is not on the baseline. |
| `Needs human investigation` | Evidence is mixed: a failure overlaps the PR area/platform but no direct causal link is clear, or required checks are pending/absent. |
| `Insufficient data` | Build records, test results, or logs are missing/inaccessible/expired — not enough evidence to make a responsible claim. |
| `No failures found` | No failing, pending, or inconclusive checks and no extracted failures. |

Be conservative: do not declare `Ready to merge` while required checks are still
pending, and do not mark a failure unrelated just because it "looks flaky" — cite
concrete evidence (baseline match, infra message, known-issue link).

### Flaky vs PR-specific: the deterministic proofs

A failure may be called **not PR-specific** only with one of these concrete proofs — never
on appearance alone:

1. **Baseline match** — the same `test+platform` also fails on the most recent base-branch
   build (`alsoFailsOnBaseline = true`), **scoped to the same pipeline definition** (a
   failure in one pipeline is never dismissed by a same-named base failure that only occurred
   in a *different* pipeline). Pre-existing, not introduced by the PR. **One veto:**
   if the PR failure exact-matches a base failure by name but the two fail for *different
   reasons* (`baselineReasonConflict = true` — e.g. a PR-introduced
   `NullReferenceException` vs a base-branch `TimeoutException` in the same test), the
   dismissal is **refused** and the failure is forced to `indeterminate`, because the
   name-based dedup key is message-blind for test failures. The reason comparison **unwraps
   wrapper exceptions** (`AggregateException`/`TargetInvocationException`) to the inner cause —
   and when a wrapper carries **multiple** inner exceptions, collapses them into a **sorted
   compound token** so a PR-introduced inner cannot hide behind a base-matching first inner —
   and, when neither side yields a known reason, falls back to a **normalized message
   fingerprint** (PR text structurally absent from base ⇒ conflict; the fingerprint preserves
   identifier-internal digits and hashes any tail past 120 chars, so two breaks differing only
   by an identifier digit or a far-out suffix stay distinct). These fire only on data
   present on both sides — with one deliberate exception: a dismissible **test** failure with
   **no reason token and no message at all** (empty `errorMessage`) gives zero corroboration
   that it is the same failure as the name match, so it too is forced to `indeterminate`. A
   noisy/partially-present message still never inflates false reds.
2. **Job-level baseline match** — for a build break with no test name (crossgen/NativeAOT/
   linker/MSBuild), the same **leg** is also red on the base branch. Conversely,
   a leg that is **red on the PR but green across several recent base builds is PROOF the
   break is PR-caused** — this is the strongest signal and a test-only diff cannot produce
   it. **Sample a few base builds, not one:** MAUI's UI suite is intermittently red on the
   base branch, so a single green base build cannot tell a real regression from a flaky test
   that merely happened to pass its one sampled base run. The automated lane computes the diff
   over the **last few completed base builds of the PR's own base branch** (`main` for a
   `main` PR, `net11.0` for a net11-targeting PR — `RegressionBaseBuilds`, default 5) and only
   calls a leg `regressed-vs-base` when it was green on **at least `MinBaseGreenSamples`**
   (default 2) of them and red on **none** — a deterministic build-error leg
   (crossgen/NativeAOT/linker/MSBuild, which compiles or it doesn't) needs only one green base
   build. A leg red on **some** sampled base builds and green on others is `flaky-on-base`
   (never a regression); a leg green on base but on too few samples is
   `succeeded-on-base-unconfirmed` (indeterminate, not a confident regression). The automated
   lane **computes this in `Gather-TestFailureContext.ps1`** (per-failure `legBaselineResult` /
   `legRegressedVsBase` / `legAlsoFailsOnBase`, the `baseSampleCount` / `baseGreenCount` /
   `baseFailedCount` evidence, and a `deterministicAttribution` prior); the interactive
   investigator does the same comparison by hand from the timelines. **Note the asymmetry:** a
   leg being red on base (`legAlsoFailsOnBase`) is only **leg-level** evidence — the leg can
   fail on base at a *different* test, so it does **not** on its own prove *this* test is
   pre-existing. Only an **exact test+platform** base match (`alsoFailsOnBaseline`, item 1) is
   strong enough to dismiss; a leg-only match is treated as **indeterminate** (`Needs human
   investigation`), never dismissed.
3. **Known-issue match** — the failure message matches an open `Known Build Error` issue
   (the dotnet Build Analysis registry). Cite the issue number/link — but treat it as a
   **hint, not a dismissal**: a text match alone can shadow a real PR break with a broad
   matcher. The automated lane only dismisses a known-issue match (`deterministicAttribution
   = known-issue`) when the **exact same test+platform also failed on the base build**
   (i.e. the same condition as `pre-existing-on-base`, item 1) — leg-level corroboration
   (`legBaselineResult = failed-on-base` for the *leg*) is **too coarse** and no longer
   dismisses, because a broad known-issue regex could otherwise launder a PR-caused break in
   a *different* test that merely shares a red leg on base. An uncorroborated text match (no
   exact base match) stays `indeterminate` (`Needs human investigation`).
   The local gatherer preserves `ErrorMessage` arrays as ordered AND conditions:
   every literal must occur on a distinct subsequent line within one actual occurrence,
   following the [public BA array contract](https://github.com/dotnet/arcade/blob/5a63d054ea793bdbdb6aa9f4558558e3a8f41529/Documentation/Build%20Analysis/KnownIssues.md#fill-out-known-issues-with-a-list-of-errors).
   Each array's matched text includes that occurrence's name, message, and separately
   captured stack trace. Arrays cannot combine evidence from separate occurrences of a
   deduplicated test/platform. Scalar and regex rules retain their existing aggregated text.
   Case-insensitive matching, the 20,000-character cap per matched text, first-matching-rule
   selection, and attribution gates are unchanged. This local hint matcher does not
   establish full Build Analysis/Build Insights service parity.
4. **Retry recovery** — the failing leg was retried by CI and **passed** on a later
   attempt (the recovered leg does not surface as a failure at all). A leg that was retried
   and **still failed** (`retriedStillFailing = true`) is the opposite — **persistent**,
   so do not call it flaky.

If none of these hold, a failure on a path/platform the PR changes leans PR-caused. Note
that **not every failure is a test** — a red build leg with no extracted test name still
counts as a failure and must be classified, never silently dropped.

**Build Insights report intake:** The automated review gatherer also reads the
GitHub check reports for its captured PR head, accepting only the
`build-insights` / `Build Insights` publisher/name pair. It explicitly requests
all check runs, including earlier same-named reports, within its pagination cap.
`context.json` exposes their metadata and bounded raw
title, summary, and text in `buildInsights.reports`. These are untrusted,
supplementary leads, not a replacement for direct Azure/Helix evidence, the
local known-issue matcher, or baseline attribution. Collection `complete` does
not certify pipeline coverage; pending, missing, or truncated reports cannot
establish that a failure is absent. Collection errors remain explicit, and the
no-results evaluation shortcuts do not fetch reports. Supplementary collection
runs only after direct evidence, target-branch history, and baseline attribution,
using the remaining gather budget. An exhausted budget leaves an explicit report
collection limitation rather than delaying primary evidence.

The gatherer's **legacy deterministic verdict ceiling** (`gate.verdictCeiling`)
constrains legacy merge-readiness consumers, not the current skill's attribution
verdict. For those consumers, the overall verdict can never be more favorable than
what coverage allows. A green verdict
(`Ready to merge` / `No failures found`) is forbidden whenever a check is still pending, a
failing check could not be inspected, **or a failed build leg produced no extractable
failure** (`gate.unexplainedFailedLegs > 0` — the backstop that stops a crossgen/NativeAOT
build break from being silently counted as zero failures). The same green is also forbidden
whenever an **accessible** failing check produced **no** extractable failure **and no**
unexplained-leg record (`gate.unaccountedFailingChecks > 0` — the earned-green guard: a red
check whose log threw, had no log id, or fell past the per-build cap still pulls the ceiling
down to `Needs human investigation` instead of defaulting to green), whenever a failing check
**did not finish cleanly** (`gate.abortedFailingChecks > 0` — a `CANCELLED`/`TIMED_OUT`/
`STARTUP_FAILURE`/`STALE`/`ACTION_REQUIRED` conclusion, whose aborted legs can carry no
`error` issue and so never become unexplained legs; a PR-induced hang that got a job
cancelled must not be masked green by a dismissible sibling on the same build), whenever a
backing **build's own result is `canceled`** regardless of the GitHub check conclusion
(`gate.canceledBuildChecks > 0` — broader than the conclusion-based aborted guard: a build
canceled mid-flight after a leg already posted `FAILURE`/`SUCCESS` slips past that guard, so
it is capped on the build metadata directly), whenever a **green device-test check could not
be confirmed `Failed == 0`** (`gate.deviceTestUnverified > 0` — XHarness exits 0 even when
device tests fail, so a green `maui-pr-devicetests` check is trusted only when a fail count
was positively observed all-zero; absent that, it caps to `Needs human investigation`), or
when a failure can be
attributed **neither** way — not a clean regression vs base, not pre-existing on base, not a
known issue (`gate.unattributedFailures > 0`; e.g. the base leg was flaky, green on too few
base samples to confirm a regression (`succeeded-on-base-unconfirmed`), the base build was
missing/unreadable, or a device-test result fell outside the deterministic
build-error class). A `pre-existing-on-base` or exact-match `known-issue` dismissal is also
**refused** (downgraded to `indeterminate`) when the PR actually edits the failing test file
(`scopeGuardTripped` — the PR may have changed the test so it now fails for a new reason that
merely coincides with the base/known text) or when the PR and base failures of the same test
have a known **reason conflict** (`baselineReasonConflict`). It is likewise **capped at `Not ready`** whenever
a leg is red on the PR
but green across several recent base builds and red on none of them (`gate.legsRegressedVsBase > 0` — the
computed job-level regression; a device-test BUILD break counts here, only device-test TEST
results are excluded). A proven regression sets the ceiling to `Not ready` even when softer
`Needs human investigation` reasons are also present — a definitive PR-introduced break is a
more actionable headline than "go investigate", and `Not ready` is still non-green so this
never enables a false green (the NHI reasons remain listed in `ceilingReasons`). The
gatherer also extracts build-job errors (not just xUnit `[FAIL]` lines) via
`Get-BuildErrorsFromLog`, so crossgen/R2R/linker breaks **and fatal non-coded breaks**
(native crash/segfault/OOM, test-host crash, unhandled exception — not the ordinary
`exit code 1` test-runner rollup) flow into dedup, the (computed)
baseline diff, and the gate — and it does so **even on a leg that also has a test failure**
(a pre-existing flaky test must not hide a *new* `error CS####` build break or a new native
crash in the same leg),
suppressing only the generic `##[error]` rollup line when a real test failure is already
present. It also inspects **`partiallySucceeded`** timeline records on both the PR and base
side (not just `failed`), symmetric with the leg-result map, so a PR-caused break in a
partially-succeeded task cannot escape the gate while a dismissible sibling accounts for the
build. The interactive investigator should apply the same discipline by hand.

## Escalation

For deep Helix log analysis (recurring failures, machine-specific issues, comparing
passing vs. failing runs), escalate to the `helix-investigation` skill.
