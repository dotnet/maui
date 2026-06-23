<!--
Canonical MAUI CI facts. Single source of truth for pipeline names/IDs, CI quirks,
deduplication, baseline comparison, and merge-readiness criteria.

Consumed by (keep these as references, do NOT re-copy the tables):
- .github/skills/azdo-build-investigator/SKILL.md
- .github/skills/review-test-failures/SKILL.md
- .github/workflows/ci-status-main.md
- .github/workflows/ci-status-net11.md
- .github/workflows/copilot-review-tests.md (via the review-test-failures skill)

When a fact changes (e.g. a pipeline definition ID), change it HERE only.
-->

# .NET MAUI CI Facts

Authoritative reference for dotnet/maui CI investigation, test-failure
classification, and merge-readiness assessment. Both the interactive
`azdo-build-investigator` skill and the automated `/review tests`
(`review-test-failures`) workflow reason from these same facts.

## Pipelines

**Organization**: `dnceng-public` / project `public`
(`https://dev.azure.com/dnceng-public/public/_apis/build/...`).

| Pipeline Name | Definition ID | Purpose |
|---------------|---------------|---------|
| `maui-pr` | **302** | Main build + unit/integration validation — check this first |
| `maui-pr-devicetests` | **314** | Helix device tests (iOS, Android, Windows, MacCatalyst) |
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

### XHarness exit-0 blind spot

XHarness (iOS/Android device tests in `maui-pr-devicetests`) **exits with code 0 even
when tests fail**. So the AzDO job shows ✅ "Succeeded", `ci-analysis` may report no
failures, but real failures are hidden inside the Helix work items.

**Detect hidden failures** via the Helix aggregated endpoint:

```
GET https://helix.dot.net/api/2019-06-17/jobs/{correlationId}/aggregated
```

Look for `Failed` > 0 even when the AzDO build job is green. Always cross-check this
for `maui-pr-devicetests` when a job is green but device-test failures are suspected
(or the PR carries `s/agent-gate-failed`). If Helix aggregate data is absent, state
that device-test hidden failures could not be verified — do not assume green = clean.

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
  directly changes that test, its snapshot/baseline, or the platform code it exercises.
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
| `error : ... Failed to load assembly` | `maui-pr` `Build <platform>` leg | **crossgen2 / ReadyToRun (R2R)** break — a failed **build job**, not a test, with no test name. Common after an SDK/runtime (`dotnet/dotnet`) bump on a flow PR. Job-level baseline diff: red on PR vs green on base ⇒ PR-caused. |
| `error IL####` / `ILC####` / NativeAOT publish fail | `maui-pr` AOT legs, `Run Integration Tests – AOT` | **NativeAOT / ILC** trim-analysis break. May be pre-existing (e.g. HybridWebView `IL2026`) — confirm with a job- AND test-level baseline diff before attributing to the PR. |
| `error NETSDK1144` | `maui-pr` TrimFull legs | Optimizing assemblies for size failed (often an ILLink warning promoted to error). Check whether the same leg is red on the base branch. |
| `XamlC` | `maui-pr` | XAML compiler — usually missing type or bad binding |
| `error XAGRDL0000` / `401` / `No local versions` | `maui-pr` or official build | Gradle/Maven feed issue — see above |
| `XHarness timeout` | `maui-pr-devicetests` Helix logs | Test killed by infrastructure; may be transient |
| `No test result files found` | `maui-pr-devicetests` Helix logs | Tests never ran or app crashed on launch |
| UI test screenshot diff | `maui-pr-uitests` | Visual regression; check baseline images |

## Merge-readiness criteria

Used by both the interactive investigator (answering "is this PR ready to merge?") and
the automated `/review tests` overall verdict. Assess **only CI/test health** — code
review and approval are separate, human-only decisions.

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
   build (`alsoFailsOnBaseline = true`). Pre-existing, not introduced by the PR.
2. **Job-level baseline match** — for a build break with no test name (crossgen/NativeAOT/
   linker/MSBuild), the same **leg** is also red on the most recent base build. Conversely,
   a leg that is **red on the PR but green on base is PROOF the break is PR-caused** — this
   is the strongest signal and a test-only diff cannot produce it.
3. **Known-issue match** — the failure message matches an open `Known Build Error` issue
   (the dotnet Build Analysis registry). Cite the issue number/link.
4. **Retry recovery** — the failing leg was retried by CI and **passed** on a later
   attempt (the recovered leg does not surface as a failure at all). A leg that was retried
   and **still failed** (`retriedStillFailing = true`) is the opposite — **persistent**,
   so do not call it flaky.

If none of these hold, a failure on a path/platform the PR changes leans PR-caused. Note
that **not every failure is a test** — a red build leg with no extracted test name still
counts as a failure and must be classified, never silently dropped.

The automated `/review tests` lane additionally computes a **deterministic verdict
ceiling** in `Gather-TestFailureContext.ps1` (`gate.verdictCeiling`): the overall verdict
can never be more favorable than what coverage allows. A green verdict
(`Ready to merge` / `No failures found`) is forbidden whenever a check is still pending, a
failing check could not be inspected, **or a failed build leg produced no extractable
failure** (`gate.unexplainedFailedLegs > 0` — the backstop that stops a crossgen/NativeAOT
build break from being silently counted as zero failures). The gatherer also extracts
build-job errors (not just xUnit `[FAIL]` lines) via `Get-BuildErrorsFromLog`, so crossgen/
R2R/linker breaks flow into dedup, the baseline diff, and the gate. The interactive
investigator should apply the same discipline by hand.

## Escalation

For deep Helix log analysis (recurring failures, machine-specific issues, comparing
passing vs. failing runs), escalate to the `helix-investigation` skill.
