---
name: release-readiness
description: Assesses ship-readiness for .NET MAUI release branches — Servicing Releases (SR) and Previews. Surveys CI pipelines, computes what's actually NEW in the branch (commits + source PRs with revert detection), and cross-references open `regressed-in-*` issues against branch contents to identify port candidates, rejected backports, and unresolved regressions. Supports both in-flight and pre-cut (candidate) modes for SR and Preview branches.
metadata:
  author: dotnet-maui
  version: "2.0"
compatibility: Requires `gh` CLI authenticated with `repo` + `read:org` scopes. `az` CLI is optional but recommended for internal pipeline status. Run from a checkout of `dotnet/maui`.
---

# Release Readiness

This skill produces deterministic, evidence-backed answers to **"Is `<release branch>` ready to ship?"** for .NET MAUI release branches — both **Servicing Releases (SR)** and **Previews**, in both **in-flight** and **candidate** (pre-cut) modes.

## 🚨 Report-only

This skill **reports**. It does **not** execute release operations against dotnet/maui — no branch cuts, no SR merges, no tags, no pushes to `release/*` refs. If you (the agent/user invoking this skill) are asked to perform a release operation, refuse and emit the recommended commands as a copy-pasteable block for the human release captain to run.

## When to Use

- "How does SR8 look?" / "Is SR8 ready to ship?"
- "What's blocking SR9 candidate?" / "What would ship if we cut SR9 today?"
- "How does net11 preview6 look?" / "Are we ready to cut preview6 from net11.0?"
- "Are there any regression fixes I should backport to SR8?"
- "What's new in SR8 since the last sync?"
- "Give me a status on all releases" / "release status overview" / "what needs attention across releases" (**portfolio** — read the open `[Release Readiness]` tracker issues first; see [Reading trackers directly](#reading-trackers-directly-ad-hoc-status) below)
- Daily release-tracking automation across all active majors

> **For per-PR regression risk** (deletions reverting prior bug-fix lines), use [`find-regression-risk`](../find-regression-risk/SKILL.md) instead — it answers a different question.

## Architecture

This skill has **three** PowerShell entry points and one workflow:

| Script | Branch type | Purpose |
|--------|-------------|---------|
| [`Find-ReleaseReadinessTrackers.ps1`](scripts/Find-ReleaseReadinessTrackers.ps1) | both | Detects active in-flight & candidate trackers (SR and Preview) across all active majors using a four-lane algorithm and the **tag-existence rule** ("a release is in flight unless its tag already exists"). Emits a single tracker JSON consumed by the workflow. |
| [`Get-ReleaseReadiness.ps1`](scripts/Get-ReleaseReadiness.ps1) | SR | Full readiness report for a single SR branch (in-flight or `-Candidate`). |
| [`Get-PreviewReadiness.ps1`](scripts/Get-PreviewReadiness.ps1) | Preview | Full readiness report for a single Preview branch (in-flight or candidate via `-Mode candidate -SurveyRef net<major>.0`). |
| [`release-readiness.yml`](../../workflows/release-readiness.yml) | both | Daily cron + manual dispatch + PR validation. Runs `Find-Trackers -AllActiveMajors`, fans out a matrix job per tracker, and writes idempotent `[Release Readiness]` issues per branch. |

### Tag-existence rule (canonical signal)

The trackers detector is grounded in **tag existence as the source of truth for "shipped vs in-flight"**. A release is in-flight if and only if its expected tag has NOT been published — branch existence, commit recency, and milestone state are all secondary signals.

- SR shipped tag pattern: `<major>.0.<patch>` (e.g. `10.0.71` shipped → SR7 no longer produces a tracker)
- Preview shipped tag pattern: `<major>.0.0-preview.<N>.<date>[.<build>]` (e.g. `11.0.0-preview.5.26304.4` shipped → preview5 no longer produces a tracker)

## Quick Start

### One-shot daily report (matches what the workflow runs)

```bash
# Detect every active in-flight + candidate tracker across all active majors
pwsh .github/skills/release-readiness/scripts/Find-ReleaseReadinessTrackers.ps1 \
  -AllActiveMajors \
  -OutputJson trackers.json

# Emits a JSON envelope with one tracker per active branch, each carrying:
#   branchType:    'sr' | 'preview'
#   branchName:    canonical proposed branch slug (always populated)
#   branchExists:  true if the branch is on origin, false for candidates
#   mode:          'in-flight' | 'candidate'
#   surveyRef:     ref to actually survey (branch itself, or net<major>.0 for candidates)
#   canonicalKey:  stable join key (e.g. net10-sr8, net11-preview6)
#   issueTitle:    title for the daily tracker issue
#   regressionLabels: list of regressed-in-* labels relevant to this branch
```

### SR (Servicing Release)

```bash
# In-flight SR
pwsh .github/skills/release-readiness/scripts/Get-ReleaseReadiness.ps1 \
  -SrBranch release/10.0.1xx-sr8 \
  -RegressionLabels regressed-in-10.0.70,regressed-in-10.0.80 \
  -TrackerKey net10-sr8 \
  -OutputDir CustomAgentLogsTmp/release-readiness/sr8

# SR candidate (no branch yet — survey main; pass the PRIOR SR as -SrBranch)
pwsh .github/skills/release-readiness/scripts/Get-ReleaseReadiness.ps1 \
  -SrBranch release/10.0.1xx-sr8 \
  -Candidate \
  -RegressionLabels regressed-in-10.0.80,regressed-in-10.0.90 \
  -TrackerKey net10-sr9 \
  -OutputDir CustomAgentLogsTmp/release-readiness/sr9-candidate
```

### Preview

```bash
# In-flight preview
pwsh .github/skills/release-readiness/scripts/Get-PreviewReadiness.ps1 \
  -Branch release/11.0.1xx-preview6 \
  -Mode in-flight \
  -TrackerKey net11-preview6 \
  -OutputDir CustomAgentLogsTmp/release-readiness/preview6

# Preview candidate (branch not cut yet — survey net11.0 instead)
pwsh .github/skills/release-readiness/scripts/Get-PreviewReadiness.ps1 \
  -Branch release/11.0.1xx-preview6 \
  -Mode candidate \
  -SurveyRef net11.0 \
  -TrackerKey net11-preview6 \
  -OutputDir CustomAgentLogsTmp/release-readiness/preview6-candidate
```

## Parameters

### `Find-ReleaseReadinessTrackers.ps1`

| Parameter | Default | Description |
|-----------|---------|-------------|
| `-MajorVersion` | 0 (auto from `eng/Versions.props`) | Single major to scan. |
| `-AllActiveMajors` | off | Scan every active major (current + lower in-flight). Mutually exclusive with `-MajorVersion`. |
| `-Repo` | cwd | Path to a checkout of dotnet/maui. |
| `-ActivityWindowDays` | 7 | Recent-commit window used to compute `recentCommitCount`. |
| `-NoFetch` | off | Skip `git fetch` (faster re-runs). |
| `-OutputJson` | — | File to write the tracker envelope JSON. |
| `-MaxBranches` | 50 | Safety cap on how many SR/preview branches to enumerate per major. |

### `Get-ReleaseReadiness.ps1` (SR)

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `-SrBranch` | Yes | — | SR branch name (e.g. `release/10.0.1xx-sr8`). In `-Candidate` mode, pass the **prior** SR — it's the exclude baseline for "what's new". |
| `-Candidate` | No | off | Pre-flight mode — survey `main` (with `-SrBranch` as the prior-SR baseline) to show what WOULD ship in the next SR. |
| `-InheritFromPriorSr` | No | off | In `-Candidate` mode, model the workflow where the prior SR is merged into the new branch after cut. Candidate's "what's shipping" set = main-since-priorSR ∪ priorSR-only commits. |
| `-RegressionLabels` | One of these | — | Comma-separated `regressed-in-*` labels. |
| `-InferRegressionLabels` | One of these | off | Auto-infer from `-SrBranch`. Agent should confirm before relying on this for automation. |
| `-Repo` | No | `dotnet/maui` | Repository in `owner/name` form. |
| `-MainBranch` | No | `main` | Stable branch used for ancestry checks. |
| `-ExcludeBranches` | No | `origin/main` | Branches to exclude from SR-only commit computation. |
| `-Phase` | No | `all` | `all`, `ci`, `commits`, `regressions`, or `open-prs`. |
| `-TrackerKey` | No | — | Canonical key (e.g. `net10-sr8`) embedded in the markdown body for idempotent issue lookup. |
| `-OutputDir` | No | — | If set, writes `release-readiness.{json,md}` and `sr-source-prs.txt`. |
| `-OutputFormat` | No | `both` | `json`, `markdown`, or `both`. |
| `-MaxIssues` | No | `100` | Cap on regression issues to walk. |
| `-NoFetch` | No | off | Skip `git fetch`. |
| `-SkipMaestroChecks` | No | off | Skip BAR/darc operational checks (default-channel mapping + per-HEAD build lookup). Auto-skipped silently if `darc` isn't on PATH; this switch forces the skip even when darc IS available. |
| `-SkipMilestoneChecks` | No | off | Skip GitHub-milestone hygiene checks (current/next milestone existence + stale-open detection). |

### `Get-PreviewReadiness.ps1` (Preview)

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `-Branch` | Yes | — | Preview branch name (e.g. `release/11.0.1xx-preview6`). Required even for candidate runs — used to derive milestone, tracker key, and regression labels. |
| `-Mode` | No | `in-flight` | `in-flight` (survey the preview branch itself) or `candidate` (survey `-SurveyRef` instead — typically `net<major>.0`). |
| `-SurveyRef` | No | computed | Ref to actually survey. Defaults to `$Branch` for in-flight; `net<major>.0` for candidate. |
| `-Repository` | No | `dotnet/maui` | Repository in `owner/name` form. |
| `-TrackerKey` | No | derived | Canonical key (default: `net<major>-preview<N>`) embedded for idempotent issue lookup. |
| `-OutputDir` | No | — | If set, writes `preview-readiness.{json,md}`. |
| `-OutputFormat` | No | `markdown` | `markdown`, `json`, or `both`. |
| `-IncludeInternal`, `-InternalBuildId` | No | — | Release-captain only — augments report with internal pipeline status when AzDO auth is available. |
| `-PublicSafe` | No | `$true` | Sanitizes non-READY internal status from public output. |

## Outputs

| File | Producer | Purpose |
|------|----------|---------|
| `trackers.json` | Find-Trackers | List of active tracker descriptors with detection evidence (one envelope per major) |
| `release-readiness.{json,md}` | Get-ReleaseReadiness | Full SR readiness report |
| `sr-source-prs.txt` | Get-ReleaseReadiness | Flat newline-delimited source PR list; use `grep -qxF NNNNN file` for instant cherry-pick verification |
| `sr-commits.json` | Get-ReleaseReadiness | Raw SR-only commit metadata |
| `preview-readiness.{json,md}` | Get-PreviewReadiness | Full Preview readiness report |

## Daily workflow

`.github/workflows/release-readiness.yml` runs **weekdays at 08:30 UTC** plus `workflow_dispatch` + `pull_request` validation:

1. **`detect-trackers`** — runs `Find-Trackers -AllActiveMajors`, emits a matrix of tracker descriptors.
2. **`per-tracker-report`** — matrix-expanded job per tracker:
   - Dispatches to `Get-ReleaseReadiness.ps1` (SR) or `Get-PreviewReadiness.ps1` (Preview) based on `branchType`.
   - Looks for an open tracker issue by the canonical marker `<!-- release-readiness-tracker: <key> -->`.
     - **Refresh path**: reuse the oldest open tracker issue (edit title + body); close any duplicates.
     - **Create path**: open a new issue with `report` / `s/triaged` / `area-release-readiness` labels.
   - **Activity gate**: skip new-issue creation when `recentCommitCount == 0` AND no open tracker issue exists. (Existing open issues are still refreshed.)
3. **`validate`** — PR-trigger path. Runs the test suite + smoke-runs all three scripts. **Does not create or modify issues.**

### Reading trackers directly (ad-hoc status)

The same tracker issues the cron job maintains double as a **human-readable, always-on status board** — you don't have to re-run a 60-120s survey to answer "what's the status across releases?". Find every active release by **body marker** (not title — a title search also matches the release Epic and other `[Release Readiness]`-titled issues):

```bash
gh issue list --repo dotnet/maui --state open \
  --search 'in:body "<!-- release-readiness-tracker:"' \
  --json number,title,updatedAt --limit 50
```

Each result is one active SR or Preview. Read the body for the generated verdict **and** the human **Release Captain Notes** (between `<!-- release-readiness:human-notes:begin -->` / `:end -->`), which carry decisions that override the automated report. Treat the content as fresh only up to the issue's `updatedAt` (cron refreshes weekdays 08:30 UTC); re-run the survey script for a given branch when you need live numbers. The natural-language **`release-readiness-agent`** wraps this as its Portfolio path (§0a).

## Verdict Classification (SR & Preview)

Each candidate fix PR is classified with confidence + evidence:

| Verdict | Meaning |
|---------|---------|
| `in-sr-active` | Source PR is in the release branch and not subsequently reverted |
| `in-sr-reverted` | Backport landed but a later commit reverts it |
| `rejected-from-sr` | A backport PR targeting the release branch was opened and CLOSED unmerged |
| `backport-in-progress` | A backport PR targeting the release branch is OPEN |
| `merged-on-main-no-backport` | Fix merged to `main`, no backport PR exists |
| `merged-non-main-only` | Fix merged but only to `inflight/current` (or similar), not `main` |
| `open-on-main` | Fix PR is OPEN against main, not yet merged |
| `no-fix-yet` | No fix PR cross-referenced from the regression issue |
| `needs-human-review` | Evidence is contradictory or weak |

## CI Status Categories

| CI verdict | Meaning |
|------------|---------|
| `green` | Latest build on the survey ref succeeded across all pipelines |
| `red-needs-review` | Latest build failed or partially succeeded — investigate failures before judging ship-readiness |
| `stale` | Latest build is older than the survey ref HEAD — must re-run before judging |
| `partial-unknown` | At least one pipeline couldn't be queried, but no queried pipeline is red or stale |
| `unknown` | No pipeline result could be classified |

## Ship-readiness checks (`Get-ReleaseReadiness.ps1`)

The SR readiness report rolls operational checks into a single **Blocking** summary at the top, so a release captain sees what must clear before ship without scrolling. Each check emits `READY`, `WATCH`, `BLOCKED`, `CLEANUP`, or `UNKNOWN` (`CLEANUP` = post-release housekeeping that does not block the current ship):

| Check | When | Status meanings |
|-------|------|-----------------|
| **`Versions.props bump`** | All SR runs | `BLOCKED` if `eng/Versions.props` on `main` hasn't been bumped past the current SR cycle (next SR has nowhere to flow). |
| **`Versions.props servicing flip`** | Live-SR mode only | `BLOCKED` if the SR branch's `eng/Versions.props` is not flipped to servicing-release mode (`PreReleaseVersionLabel=servicing` + `StabilizePackageVersion=true`). Without it the branch builds prerelease packages and never ships as stable — CI stays green so nothing else catches it. |
| **`Bug template lists SR version`** | All SR runs | `CLEANUP` if `.github/ISSUE_TEMPLATE/bug-report.yml` on `main` is missing an entry for the SR being shipped (users can't file bugs against the version) — post-release housekeeping, not a ship blocker. |
| **`Main bumped to next SR cycle`** | All SR runs | `BLOCKED` if the next SR cycle's version hasn't been promoted on `main`. |
| **`BAR default-channel mapping`** | SR branches matching `release/X.Y.Zxx-srN` | `BLOCKED` if the SR branch is not wired to the `.NET <band> SDK` channel in BAR. `UNKNOWN` if `darc` isn't on PATH (report includes the exact verification command). |
| **`BAR build for SR HEAD`** | When darc is available + SR HEAD SHA known | `READY` if BAR has a published build for the SR HEAD commit. `WATCH` (not blocking — transient) if CI hasn't published one yet. |
| **`Milestone for current cycle`** | SR + preview branches | `BLOCKED` if the current cycle's milestone (e.g. `.NET 10 SR8` or `.NET 11.0-preview6`) doesn't exist in the GitHub milestone list — fixed issues have nowhere to land. |
| **`Milestone for next cycle`** | SR + preview branches | `CLEANUP` if the next cycle's milestone isn't pre-created — open issues can't roll forward when current ships, but it doesn't block the current release. |
| **`Stale open milestones`** | SR + preview branches | `CLEANUP` if any milestones in the same major + same cycle type (SR or preview) are past their `due_on` by >7 days and still open (already-shipped releases accumulating untriaged issues). |
| **`CI Failure Scanner signals`** | All SR runs | `WATCH` if fresh ci-scan issues are filed in the last 24h. |
| **`Known Build Errors`** | All SR runs | `WATCH` if open Known Build Error issues exist that may explain background CI noise. |

### Expected ship date

The header line **`Expected ship date`** is rendered from `Get-ExpectedShipDate`, which reads `PatchVersion` from the survey ref's `eng/Versions.props` and applies the .NET release cadence:

| PatchVersion | Cadence | Example |
|--------------|---------|---------|
| Multiple of 10 (`80`, `90`, `100`…) — also **previews** (patch=`0`) | 2nd Tuesday of the month | SR8 (`10.0.80`) → next 2nd Tuesday |
| Anything else (`81`, `82`, `91`…) | **ASAP** — no fixed cadence | SR8 hotfix `10.0.81` → as soon as ready |

Surfaced in JSON as `expectedShipDate.{cadence, date, daysFromNow, formattedLong, note, patchVersion}` so downstream automation doesn't redo the math.

### Maestro / BAR check gating

The BAR checks shell out to `darc` (cached probe via `Get-Command darc`). When darc isn't installed (most CI environments), both checks emit `UNKNOWN` with the exact local-verification command embedded in the row's `Next action` — so the report **never silently skips** them. The release-readiness agent runs the same checks via the `maestro_*` MCP tools when the script reports `UNKNOWN`.

## Methodology

Three critical gotchas this skill encodes — see [references/methodology.md](references/methodology.md) for the full discussion:

1. **Cherry-pick number swap**: SR backports get NEW PR numbers (e.g. main #35356 → SR7 #35428). Cannot naively grep source PR numbers; must walk SR-only commits and extract refs from commit bodies.

2. **Timeline cross-references**: `closedByPullRequestsReferences` returns empty for most MAUI issues. The skill walks `gh api repos/.../issues/N/timeline` filtering on `cross-referenced` events.

3. **Forward-flow / non-main merges**: A fix can merge into `inflight/current` only, not `main` (real example: PR #35609). The skill checks `git merge-base --is-ancestor $mergeCommit origin/main` before claiming a fix is "on main, just needs backport".

## Shared module

This skill depends on `.github/scripts/shared/MauiReleaseVersioning.psm1` for canonical milestone/version parsing (e.g. `Get-CurrentMajorVersion`, `ConvertBranchToMilestone`, `Get-MilestoneSortKey`, `Compare-MauiMilestone`). The module is also consumed by `Fix-MilestoneDrift.ps1` to keep milestone classification consistent across all release-related automation.

### `scripts/NightlyFeed.ps1` (nightly dogfood feed staleness banner)

Both engines dot-source [`scripts/NightlyFeed.ps1`](scripts/NightlyFeed.ps1) to surface a one-line **nightly dogfood feed freshness banner** at the top of each tracker (just under **Generated**). The point of the banner is to make it obvious when the dogfood bits people are told to test have stopped flowing — e.g. when the `ci.inflight` pipeline is red, the feed goes stale and the banner turns ❌ so consumers don't waste time validating against builds that never updated.

Key functions (all PURE except the one network call, which is **fail-open** — any feed error returns `$null` / renders a muted "freshness unknown" note and never breaks tracker generation):

| Function | Purpose |
|----------|---------|
| `Get-NightlyFeedFreshness` | Queries an Azure Artifacts NuGet feed (`dotnet10`, `dotnet11`, …) for the newest **published** build matching a version-prefix regex; returns version + publish date. Injectable `-Fetcher` for offline tests. |
| `Resolve-NightlyDogfoodFreshness` | Picks the stream that matters: **`ci.inflight` first** (the "shipping next" dogfood bits), the lane band only as a fallback. Conservatively returns `matched=$false` when *only* `ci.main` exists, so a daily main build never paints a false green. |
| `Format-NightlyFeedLaneLabel` | PURE builder for the `` [`feed`](url) · <typeNote> `` lane label. Centralizes the honest-labeling rule (`inflight`→`ci.inflight`; `band`→caller-formatted band note; unknown→`ci.inflight`) so the SR and Preview lanes can't drift. |
| `Get-NightlyFeedTier` / `Format-NightlyFeedBanner` | Bucket age into ✅ ≤2d · ⚠️ 3–6d · ❌ ≥7d and render the markdown banner. Both take an explicit `-Now`, so they're deterministic and unit-testable offline. |

Determinism / idempotency: the engine captures **one** `UtcNow` per run (`$Data['nightlyFeedNow']`) and reuses it for both the rendered banner and the semantic-hash tier, so a quiet SR tracker still refreshes when the feed crosses a tier boundary, but a same-tier day-count tick does **not** churn the issue. The freshness band is folded into `Get-ReportSemanticHash` (tier|version only — the raw timestamp is never hashed).

## Integration

- **Custom agent**: `.github/agents/release-readiness-agent.agent.md` wraps this skill — handles regression-label confirmation, runs the script, then uses WorkIQ to add context for `rejected-from-sr` PRs.
- **WorkIQ**: NOT called from the PowerShell scripts (PowerShell can't invoke MCP tools). The agent enriches the script's JSON output with WorkIQ context where needed.

## Anti-Patterns

> ❌ **Don't naively grep source PR numbers** in the SR git log. The backport PR number replaces the source PR number in the merge commit subject. Use `sr-source-prs.txt` (produced by this skill) instead.

> ❌ **Don't claim a fix is on `main` based on `pr-view --state MERGED`.** PRs can be merged into `inflight/current` only. The skill's `onMain` field is the authoritative check.

> ❌ **Don't trust issue-title similarity.** Two issues can have nearly identical titles and refer to different platform-specific regressions (e.g. #35313 is the Android version, #35326 is the iOS/Mac/Win version with a different fix path). Always filter by the `regressed-in-*` label, not by title.

> ❌ **Don't run with `-InferRegressionLabels` for automated workflows** without surfacing the inferred labels for confirmation. Label inference is brittle for non-standard SR cycles.

> ❌ **Don't infer "in-flight" from branch existence alone.** The detector uses the **tag-existence rule** — a release is in-flight if and only if its expected tag has not been published. Branches can linger after their release ships (and SR branches don't exist yet for SR candidates).

## Tests

```powershell
pwsh -NoProfile -File .github/skills/release-readiness/tests/Test-ReleaseReadiness.ps1
```

The harness covers:

- **Lane 1–4 detection** (shipped patch set, SR-from-main candidate, in-flight SR branches, preview lane) against the live `dotnet/maui` clone
- **Tracker emission** for SR2/SR3 (inactive), SR8 (active in-flight), SR9 (active candidate), and net11 preview6 (active candidate)
- **`-AllActiveMajors`** end-to-end across net10 + net11 with the expected tracker counts
- **`Get-ReleaseReadiness`** verdict classification using known-answer data from the SR7 readiness analysis (e.g. #35313 → `in-sr-active`, #35344 → `in-sr-active` via the SafeArea follow-on fix, #35771 → `no-fix-yet`)
- **Idempotent body hash** stability across re-runs — **SR trackers only** (the daily workflow compares the embedded `<!-- release-readiness-hash: sha=... -->` marker against the live issue and skips the edit when the semantic content is unchanged, so re-runs don't churn the tracker). Preview trackers carry no hash marker and are refreshed on every scheduled run.
- **Nightly dogfood feed banner** (`NightlyFeed.ps1`) — offline unit coverage for the lane-label honest-labeling rule (`Format-NightlyFeedLaneLabel`), the `ci.inflight`-first / `ci.main`-false-green resolver, age→tier bucketing, the fail-open feed query (mocked `-Fetcher`), and the banner's fold into `Get-ReportSemanticHash` (tier change refreshes, same-tier day tick does not). All network-free via injected fixtures and explicit `-Now`.
