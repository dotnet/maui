---
name: release-readiness
description: Assesses ship-readiness for .NET MAUI release branches — Servicing Releases (SR) and Previews — and produces public-safe, copy-ready release handoffs or Loop-page drafts from the resulting evidence. Use for readiness verdicts, release blockers, Preview/SR status, release handoff pages, manual validation instructions, or "make the SR10/Preview N release page." Surveys CI and release delta, classifies regressions, and keeps Preview and servicing semantics distinct.
metadata:
  author: dotnet-maui
  version: "2.0"
compatibility: Requires `gh` CLI authenticated with `repo` + `read:org` scopes. Local net11 enrichment also uses `az` CLI with access to dnceng/internal; it fails open when unavailable and is always skipped in GitHub Actions. Preview installability uses NuGet v3 feeds; an optional short-lived Azure DevOps PAT with Packaging Read scope may be needed for an authenticated shipping feed. Run from a checkout of `dotnet/maui`.
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
- Scheduled and event-driven release tracking across all active majors

> **For per-PR regression risk** (deletions reverting prior bug-fix lines), use [`find-regression-risk`](../find-regression-risk/SKILL.md) instead — it answers a different question.

## Architecture

This skill has **four** PowerShell entry points, one Preview helper, and one workflow:

| Script | Branch type | Purpose |
|--------|-------------|---------|
| [`Find-ReleaseReadinessTrackers.ps1`](scripts/Find-ReleaseReadinessTrackers.ps1) | all | Detects active in-flight & candidate trackers (SR, Preview, and RC) across all active majors using a five-lane algorithm and the **tag-existence rule** ("a release is in flight unless its tag already exists"). Emits a single tracker JSON consumed by the workflow. |
| [`Get-ReleaseReadiness.ps1`](scripts/Get-ReleaseReadiness.ps1) | SR | Full readiness report for a single SR branch (in-flight, `-Candidate`, or `-Shipped`). `-Shipped` surveys the same branch with post-ship verdict, carry-forward, and hotfix-vs-next-SR guidance semantics. |
| [`Get-PreviewReadiness.ps1`](scripts/Get-PreviewReadiness.ps1) | Preview / RC | Full readiness report for a single prerelease branch (in-flight or candidate via `-Mode candidate -SurveyRef net<major>.0`). Preview reports include consumer-installability evidence; RC reports retain that check as `UNKNOWN` until RC workload-set package resolution is supported. |
| [`PreviewInstallability.ps1`](scripts/PreviewInstallability.ps1) | Preview helper | Resolves the workload-set package, validates branch-pin coherence, probes manifest and representative pack availability, extracts platform prerequisites, and emits an isolated NuGet configuration for local validation. |
| [`New-ReleaseHandoff.ps1`](scripts/New-ReleaseHandoff.ps1) | all | Projects existing readiness JSON plus separately verified public release evidence into copy-ready Markdown and normalized JSON. Missing facts remain `TBD`; it never selects builds or mutates release state. |
| [`release-readiness.yml`](../../workflows/release-readiness.yml) | both | Three-hourly daytime UTC schedule + event-driven refreshes + manual dispatch + PR validation. Non-PR triggers run `Find-Trackers -AllActiveMajors`, fan out a matrix job per tracker, and write idempotent `[Release Readiness]` issues; PR triggers validate outputs only. |

Shared support code lives in [`PublicReportSanitizer.ps1`](scripts/PublicReportSanitizer.ps1) for public Markdown/JSON redaction and [`TrackerIssueLifecycle.sh`](scripts/TrackerIssueLifecycle.sh) for tested issue-selection and race-compensation primitives.

### Tag-existence rule (canonical signal)

The trackers detector is grounded in **tag existence as the source of truth for "shipped vs in-flight"**. A release is in-flight if and only if its expected tag has NOT been published — branch existence, commit recency, and milestone state are all secondary signals.

- SR shipped tag pattern: `<major>.0.<patch>` (e.g. `10.0.71` shipped → SR7 retired, no longer produces a tracker)
- Preview shipped tag pattern: `<major>.0.0-preview.<N>.<date>[.<build>]` (e.g. `11.0.0-preview.5.26304.4` shipped → preview5 no longer produces a tracker)
- RC shipped tag pattern: `<major>.0.0-rc.<N>.<date>[.<build>]` (e.g. `11.0.0-rc.1.26425.128` shipped → RC1 no longer produces a tracker)

**Post-ship lifecycle (`shipped` mode).** Most shipped SRs are retired the moment their tag exists. The **one exception** is the *most-recently-shipped* SR (highest shipped patch), which keeps emitting as `mode='shipped'` so its tracker issue stays useful through post-ship follow-up — adding the new build to the GitHub issue version dropdown, publishing release notes, closing out the milestone. Shipped reports never retroactively return `Not Ready`: unresolved work is split into urgent hotfix-vs-next-SR follow-ups and structured carry-forward items. Each immutable shipped tag is **create-once**: if it was never tracked (for example, the tag appeared before a scheduled updater ran), the workflow creates it; once a human closes that exact tagged generation, scheduled runs do not resurrect it. An untagged hotfix is explicitly marked `hotfixInProgress=true`, forces a yellow follow-up verdict, and is likewise create-once. Hotfix closure is scoped to the live version **and branch commit**: the same generation stays closed, while new commits or a new version can create fresh evidence. Workflow decisions use the generated report markers, not detector-time hotfix fields, so tag/commit changes between detection and reporting cannot apply stale lifecycle state. Older shipped SRs remain retired.

Shipped commit inventory and fix ancestry are evaluated against the immutable stable tag and the prior SR cycle's latest stable tag, never against the mutable SR branch or current `main`. This keeps the full SR inventory visible when an SR publishes multiple hotfix tags. If either tag does not resolve locally, generation fails with an explicit fetch instruction rather than emitting a falsely empty/clean tracker. During the normal tag-before-GitHub-Release window, the local stable tag is already authoritative for immutable contents and the report labels its date as tagged-commit evidence until publication metadata appears. If the Releases API is unavailable, the same immutable local-tag bounds remain usable, but the report emits a publication-status-unknown warning instead of claiming the tag is published.

Both report generators dot-source [`PublicReportSanitizer.ps1`](scripts/PublicReportSanitizer.ps1), so public Markdown and JSON use one shared redaction implementation.
If the live SR branch has commits after the latest stable tag or has already bumped toward an untagged hotfix in the same SR patch decade, the detector keeps the latest shipped SR in `shipped` mode, anchors contents to the stable tag, and emits a WATCH follow-up. This catches the pre-bump window as soon as branch HEAD advances rather than waiting for `PatchVersion` to change.

## Quick Start

### One-shot portfolio report (matches a full scheduled fan-out)

```bash
# Detect every active in-flight + candidate tracker across all active majors
pwsh .github/skills/release-readiness/scripts/Find-ReleaseReadinessTrackers.ps1 \
  -AllActiveMajors \
  -OutputJson trackers.json

# Emits a JSON envelope with one tracker per active branch, each carrying:
#   branchType:    'sr' | 'preview' | 'rc'
#   branchName:    canonical proposed branch slug (always populated)
#   branchExists:  true if the branch is on origin, false for candidates
#   mode:          'in-flight' | 'candidate' | 'shipped'
#   hotfixInProgress: true only when the latest shipped SR branch is ahead of its stable tag
#   hotfixVersion/hotfixCommit: mutable hotfix generation used for close/recreate idempotency
#   surveyRef:     ref to actually survey (branch itself, or net<major>.0 for candidates)
#   canonicalKey:  stable join key (e.g. net10-sr8, net11-preview6, net11-rc1)
#   issueTitle:    title for the maintained tracker issue
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
  '-PublicSafe:$false' \
  -TrackerKey net11-preview6 \
  -OutputDir CustomAgentLogsTmp/release-readiness/preview6

# Preview candidate (branch not cut yet — survey net11.0 instead)
pwsh .github/skills/release-readiness/scripts/Get-PreviewReadiness.ps1 \
  -Branch release/11.0.1xx-preview6 \
  -Mode candidate \
  -SurveyRef net11.0 \
  '-PublicSafe:$false' \
  -TrackerKey net11-preview6 \
  -OutputDir CustomAgentLogsTmp/release-readiness/preview6-candidate
```

### Release Candidate

```bash
pwsh .github/skills/release-readiness/scripts/Get-PreviewReadiness.ps1 \
  -Branch release/11.0.1xx-rc1 \
  -Mode in-flight \
  '-PublicSafe:$false' \
  -TrackerKey net11-rc1 \
  -OutputDir CustomAgentLogsTmp/release-readiness/rc1
```

The unattended public survey does not know the release-owner-confirmed workload-set
version or private shipping source. It therefore keeps **Consumer installability**
`UNKNOWN` rather than guessing that the newest coherent package is the blessed one.
Complete the local gate below before declaring a Preview ready.

### Copy-ready release handoff / Loop draft

Generate the appropriate Preview or SR readiness JSON first. Then follow
[`references/release-handoff.md`](references/release-handoff.md) to gather
separately verified public build, test, assessment, rollback, and workload-set
evidence and render it:

```bash
pwsh .github/skills/release-readiness/scripts/New-ReleaseHandoff.ps1 \
  -ReadinessJson ./release-readiness.json \
  -EvidenceJson ./release-evidence.json \
  -OutputDir ./release-handoff
```

This is a deterministic projection of the readiness report, not a second survey.
It supports Preview and SR (including SR10) through one editorial renderer while
preserving their different readiness semantics. It does not read or write Loop
or SharePoint. Do not copy private source-page content into the evidence file;
unknown fields must remain `TBD`.

### Preview: local net11 official-build health

For net11 preview runs through this skill from a local checkout, invoke
`Get-PreviewReadiness.ps1 '-PublicSafe:$false'`. The script then automatically
queries the internal official `dotnet-maui` pipeline (Azure DevOps definition
`1095`, org `dnceng`, project `internal`) when the current Azure CLI identity has
access. No build ID is required. It independently checks:

1. `refs/heads/net11.0` — the inflight source/survey lane.
2. `refs/heads/release/11.0.1xx-previewN` — the evaluated release branch, when
   that branch exists.

Candidate mode still checks `net11.0`; it adds the prospective release ref only
after that ref exists. Identical refs are queried once. The local report includes
each branch's health classification, build ID and number, pipeline status/result,
source SHA, and internal build URL. A failed or canceled current build is `red`;
a partially successful build is `partial-success`; a build behind the newest
trigger-eligible commit is `stale`; a queued/running build is `in-progress`;
missing or malformed evidence is `unknown`.

Discovery examines a bounded five-build window. It prefers a build at exact branch
HEAD, then scans by queue time and skips only candidates proven stale before
accepting one proven current. Indeterminate candidates are buffered: disagreeing
possible outcomes remain `unknown`, while a later proven-current failure remains
`red` only when every buffered candidate is also a completed failure/cancellation.
A terminal window containing only same-branch failed/canceled indeterminate builds
also remains blocking as `failed-or-stale` because every candidate is either red
or stale. Its rendering preserves that uncertainty and requires restoring currency
evidence before choosing failure repair versus a current-HEAD rerun. This prevents
both false readiness upgrades and loss of certain blocking evidence.

The internal check is intentionally fail-open:

- `GITHUB_ACTIONS=true` skips it before any Azure command runs.
- Missing Azure CLI, expired login, or inaccessible dnceng/internal access yields
  `skipped` and does not downgrade the public-data verdict.
- Azure CLI and GitHub branch queries have bounded execution; a timeout yields
  `unknown` rather than hanging the local readiness run.
- Local `red`/`stale`/`failed-or-stale` maps to `BLOCKED`,
  `in-progress`/`partial-success` to `WATCH`, and `unknown` to `UNKNOWN`.
  For `failed-or-stale`, restore build-currency evidence first; then either repair
  the failed build if it is current or run the official build at current HEAD if
  it is stale.
- `-PublicSafe:$true` omits all internal IDs, SHAs, URLs, and branch rows. The
  public workflow uses this behavior and never receives internal credentials.

The script remains public-safe by default. This skill and the release-readiness
agent explicitly pass `'-PublicSafe:$false'` for enriched local net11 reports;
never reuse those artifacts in a public tracker issue. `-IncludeInternal`
remains an explicit compatibility override when a caller requests a sanitized
internal classification, and `-InternalBuildId` remains a diagnostic override
for the evaluated release branch.

### Preview: authoritative blessed-build source (.NET Release Tracker)

For **Previews**, this skill's public survey (CI health + regression classification on `net<major>.0` or the preview branch) tells you whether the code is *ready*, but it **cannot on its own name which staged build is the official, blessed preview** — that designation lives in the private **.NET Release Tracker** plugin. So when answering *"run release readiness … is net11 preview6 ready?"* / *"which build is the official preview6?"*, consult that authoritative source **in addition to** running `Get-PreviewReadiness.ps1`:

1. **Classify access first (deterministic gate — fetches no release data, always exits 0):**

   ```bash
   pwsh ./.github/skills/dependency-flow/scripts/Get-PreviewReleaseReadiness.ps1
   # -> RELEASE_TRACKER_STATUS=NO_ACCESS | ACCESS_ON_INACTIVE_ACCOUNT | AVAILABLE_NOT_ENABLED | AVAILABLE_ENABLED
   ```

2. **Branch on the token:**
   - `AVAILABLE_ENABLED` → invoke the **`dotnet-release-tracker`** skill for the blessed SDK/runtime + BAR id + stage, and present it as the authoritative official preview build. It is a **skill/plugin, not an MCP tool** — don't look for a `release-tracker` entry in the tool list and give up; run the skill (reload/restart the session if it's enabled but hasn't loaded yet). Combine it with this skill's CI/regression verdict for the full picture.
   - `AVAILABLE_NOT_ENABLED` → the caller has access but the plugin isn't enabled locally; offer the one-time user-scope opt-in, then re-run the gate.
   - `ACCESS_ON_INACTIVE_ACCOUNT` → access exists, but only under a logged-in **inactive** `gh` account (named in the gate's `inactiveAccount`); the plugin loads under the active identity, so advise `gh auth switch --user <account>` and re-run the gate — do **not** invoke the plugin or claim availability under the current identity.
   - `NO_ACCESS` → report from public data only. For the official-build line, fall back to the **latest build on the public `.NET 11.0.1xx SDK Preview N` channel** (public BAR/Maestro) and present it **labeled** as a display-only public-feed candidate — "source: public preview feed; may not be the final official build." Keep VMR validation **UNKNOWN**: do not compare/update the MAUI pin or render ✅ from that candidate. Don't name or hint at the private tracker tool (see dependency-flow's privacy guardrail).

The full tier table, the user-scope opt-in snippet, and the privacy guardrails live in dependency-flow's **"Preview release readiness (authoritative source + access tiers)"** section ([`../dependency-flow/SKILL.md`](../dependency-flow/SKILL.md)) — cross-reference it rather than duplicating it here.

> **Blessed ≠ green.** The release tracker names the *official* build; it does **not** substitute for the ship-readiness judgment. A build can be blessed while this skill still reports open `regressed-in-*` blockers — surface both.

**Don't maintain a standing "🏷️ Official (blessed) preview build" table in the tracker.** The deterministic CI body already owns the public build-pin handling: its **"🏷️ Preview N component build — branch pins + update paths"** section states the pins are explicitly *not* the blessed build, carries the drift-proof "verify locally" prompt, infers Android/macOS-iOS subscription health from the public PR trail, and identifies VMR as a local official-build reconciliation path. Because the blessed build number is embargoed (withheld from the public issue), a standing public table just renders "🔒 withheld" and duplicates that callout. So a local run with tracker access should **report the blessed SDK/runtime build in its conversational answer**, and only add a line to _Release Captain Notes_ when there's a **decision or exception worth persisting** — e.g. the blessed build differs from the branch pin, a promoted build was rejected, or an Android/macOS-iOS subscription is confirmed broken. Don't re-create the section the CI body already renders.

### Preview: consumer-installability gate

The branch being green is insufficient: a customer must be able to acquire the
exact SDK workload set, its component manifests, and representative Android,
Apple (including tvOS), Emscripten, MAUI, and runtime packs from a clean source
configuration.

Use the exact workload-set **CLI version** confirmed by the release owner. Do not
substitute the branch SDK version, and do not assume the newest coherent package
is blessed. Workload-set CLI and NuGet versions have different normalization:
`11.0.100-preview.6.26363.2` maps to
`11.100.0-preview.6.26363.2` for the NuGet package.

If all assets are public, the confirmed version is enough:

```bash
pwsh .github/skills/release-readiness/scripts/Get-PreviewReadiness.ps1 \
  -Branch release/11.0.1xx-preview6 \
  -Mode in-flight \
  -ConfirmedWorkloadSetVersion 11.0.100-preview.6.26363.2 \
  '-PublicSafe:$false' \
  -OutputDir CustomAgentLogsTmp/release-readiness/preview6-local
```

If an authenticated shipping feed is required:

1. Create a short-lived PAT at
   [`https://dev.azure.com/dnceng/_usersSettings/tokens`](https://dev.azure.com/dnceng/_usersSettings/tokens).
   Select the `dnceng` organization and grant only **Packaging > Read**. Use the
   shortest practical expiration. Never paste the PAT into a command argument,
   NuGet.Config, report, issue, PR, chat transcript, or repository file.
2. Put the credential in NuGet's standard environment variable. The suffix must
   exactly match the source name passed to `-AdditionalPackageSource`.

   ```bash
   read -s -p "dnceng Packaging Read PAT: " DNCENG_PACKAGING_PAT; echo
   export NuGetPackageSourceCredentials_internal_preview6="Username=release-readiness;Password=${DNCENG_PACKAGING_PAT};ValidAuthenticationTypes=Basic"
   unset DNCENG_PACKAGING_PAT
   ```

3. Run the local report with the source in `name=https://...` form:

   ```bash
   pwsh .github/skills/release-readiness/scripts/Get-PreviewReadiness.ps1 \
     -Branch release/11.0.1xx-preview6 \
     -Mode in-flight \
     -ConfirmedWorkloadSetVersion 11.0.100-preview.6.26363.2 \
     -AdditionalPackageSource 'internal_preview6=<shipping-feed-v3-index-url>' \
     '-PublicSafe:$false' \
     -OutputDir CustomAgentLogsTmp/release-readiness/preview6-local
   ```

4. Use the generated local-only `<clear />` NuGet configuration and install
   command from `preview-readiness.md`. Then remove the credential:

   ```bash
   unset NuGetPackageSourceCredentials_internal_preview6
   ```

`-PublicSafe $false` intentionally includes exact source URLs and installation
instructions, so keep that output local. The default public-safe report removes
the release-owner-confirmed workload-set version and any candidate version
learned from an authenticated/internal source, including versions repeated in
nested manifest and pack evidence. It also removes additional source names,
URLs, nested source metadata, credentials, and the generated NuGet
configuration. Unconfirmed candidates discovered entirely from public feeds
remain visible as diagnostic evidence.

The gate classifies evidence as follows:

| Installability | Readiness | Meaning |
|----------------|-----------|---------|
| `installable` | `READY` | Confirmed CLI version, branch pins, required manifests, and representative packs all agree and resolve. |
| `missing` | `BLOCKED` | A confirmed package or asset is absent from every accessible supplied source. |
| `mismatched` | `BLOCKED` | The workload set disagrees with the branch SDK, Android, Apple, or runtime pins, or with the target MAUI Preview train. |
| `unknown` | `UNKNOWN` | Version is unconfirmed, a source is inaccessible, or evidence could not be read. HTTP 401/403 is never treated as proof that a package is missing. |

The isolated source set is deliberate: `dotnet-workloads` owns the workload-set
package, `dotnet<major>-workloads` owns platform manifests/assets,
`dotnet<major>` owns MAUI and Apple manifests/assets,
`dotnet<major>-transport` owns runtime transport assets, and `dotnet-public`
plus NuGet.org provide shared dependencies. Do not inherit stale feeds from a
machine-wide NuGet.Config.

### Preview: is the branch actually plumbed? (subscription wiring + feed drift)

A preview can pass CI and even have a blessed build yet still not be *ship-wired* —
the branch is cut but nothing flows into it, or its promoted feed lags the branch.
The deterministic CI body already gives a **best-effort inferred** read of the
Android/macOS-iOS wiring from the public PR trail — the **Update path / flow signal**
column in its **"🏷️ Preview N component build — branch pins + update paths"** section
(🔄 open dep-flow PR / ✅ fresh merge ≤14d / ⚠️ stale >14d / ❌ none seen). Its VMR
row instead directs the captain to local official-build reconciliation. The
checks below are the **authoritative** confirmation a local run adds on top of that
inference (`darc`/BAR can see the subscription itself; CI can only see its PR
exhaust). Run them when the inferred signal is ⚠️/❌, or to confirm a ✅ before ship.
A complete *"is preview N ready?"* answer runs two public BAR/Maestro + git checks
and one access-tiered official-build check alongside the survey:

- **Subscriptions wired?** Confirm the `release/11.0.1xx-previewN` branch has its default-channel mapping **and** the baseline two subscriptions (android + macios on `.NET 11.0.1xx SDK Preview N`). There is intentionally **no dotnet/VMR subscription**: reconcile that pin locally against the official SDK/runtime build because the Maestro channel can differ from the release source of truth. Branch cut + default-channel present but either subscription missing = a start-of-preview flow gap → surface as an **FYI note** (not a ship blocker), naming the missing source repos.
- **Feed matches the branch?** Compare the latest build promoted to the `.NET 11.0.1xx SDK Preview N` channel (`maestro_latest_build`) against `origin/release/11.0.1xx-previewN` HEAD. Branch ahead of the promoted build = stale feed → flag it.
- **Component pins coherent?** Report which `dotnet/android`, `dotnet/macios`, and `dotnet/dotnet` (VMR) builds MAUI bundles (version + SHA from `eng/Version.Details.xml`). For Android/macOS-iOS, verify the pin belongs to the component's same-named `release/...-previewN` branch and carries the expected stage/band; the component branch being ahead of the pin is an FYI because subscriptions may advance it later. Validate the SDK/VMR pin against the **official SDK/runtime build** from the release source of truth, not `netN.0`, component branch tip, or the Maestro preview channel. Android's `-ci.main.NN` scheme is normal for net11 and is not itself an anomaly.

All three checks — the exact MCP/`darc`/git commands, the interpretation tables, the
remediation (combined-PR pattern), and live worked examples — live in dependency-flow's
**"Wiring checks: is Preview N actually plumbed?"** subsection (Checks A/B/C)
([`../dependency-flow/SKILL.md`](../dependency-flow/SKILL.md)); run them from there and
fold the results into the preview report (missing subs ⇒ an FYI note; stale feed ⇒ a
flagged concern; component pins ⇒ coherent ✅ or a flagged divergence) rather than
duplicating the mechanics here. Persist a line in _Release Captain Notes_ **only when
the authoritative check diverges from or refines the CI-inferred Flow signal** — e.g.
inferred ✅ but the sub points at the wrong channel, or inferred ⚠️/❌ confirmed as a
real gap with the missing source repos named. When the local check simply agrees with
the inferred signal, report it conversationally and leave the tracker to the CI body.

### Prerelease action ordering (newly cut Preview or RC branch)

When the preview branch exists but its plumbing is incomplete, present the remediation
as a dependency-ordered sequence. Do **not** sort unrelated `BLOCKED`/`WATCH` rows ahead
of these prerequisites:

1. Add the matching Preview/RC default-channel mapping for MAUI's outward build flow.
2. Add the baseline Android and macOS/iOS subscriptions. Wait for the
   `maestro-configuration` PR to merge into `production`, then verify BAR ingestion
   with `darc get-subscriptions --target-repo https://github.com/dotnet/maui --target-branch <preview-branch>`.
3. Reconcile MAUI's SDK/VMR pin **locally** with the official Preview/RC build and
   open the resulting focused component-bump PR. Do not add a VMR subscription.
4. Build branch HEAD and promote the resulting MAUI build to the matching Preview/RC channel.
5. Clear current-preview CI/device/UI failures and finish release validation.

Keep the report scoped to Preview N. Do **not** mention the `netN.0` Preview N+1
bump, `main → netN.0` PRs, the Preview N+1 milestone, or any other next-preview
work. Those belong only in the Preview N+1 candidate/in-flight readiness report.

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
| `-Shipped` | No | off | Post-ship mode for any explicitly requested tagged SR. Uses immutable stable-tag contents while keeping operational checks live; scheduled trackers normally emit only the most recently tagged SR. |
| `-ShippedTag` | No | latest local stable tag in the SR patch range | Explicit immutable shipped-anchor override for deterministic/manual runs. |
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
| `-IncludeInternal` | No | off | Release-captain only — augments the local survey with internal pipeline status when AzDO auth is available. |
| `-PublicSafe` | No | `$true` | Sanitizes private/internal coordinates from SR Markdown and JSON. Set false only for local artifacts that will not be posted publicly. |

### `Get-PreviewReadiness.ps1` (Preview / RC)

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `-Branch` | Yes | — | Preview or RC branch name (e.g. `release/11.0.1xx-preview6` or `release/11.0.1xx-rc1`). Required even for candidate runs — used to derive milestone, tracker key, and regression labels. |
| `-Mode` | No | `in-flight` | `in-flight` (survey the preview branch itself) or `candidate` (survey `-SurveyRef` instead — typically `net<major>.0`). |
| `-SurveyRef` | No | computed | Ref to actually survey. Defaults to `$Branch` for in-flight; `net<major>.0` for candidate. |
| `-Repository` | No | `dotnet/maui` | Repository in `owner/name` form. |
| `-TrackerKey` | No | derived | Canonical key (default: `net<major>-preview<N>` or `net<major>-rc<N>`) embedded for idempotent issue lookup. |
| `-OutputDir` | No | — | If set, writes `preview-readiness.{json,md}`. |
| `-OutputFormat` | No | `markdown` | `markdown`, `json`, or `both`. |
| `-IncludeInternal` | No | off | Compatibility override that requests sanitized internal classification even with `-PublicSafe`; enriched local skill/agent runs pass `'-PublicSafe:$false'`. GitHub Actions still skips. |
| `-InternalBuildId` | No | — | Diagnostic override for the evaluated release branch. Normal runs discover the latest definition-1095 build for each branch. |
| `-PublicSafe` | No | `$true` | Sanitizes private/internal coordinates from Preview Markdown and JSON, including internal IDs, SHAs, URLs, and branch rows. The local skill/agent explicitly sets `$false` for enriched net11 reports that will remain local. |
| `-ConfirmedWorkloadSetVersion` | No | — | Exact release-owner-confirmed workload-set CLI version. Required before Consumer installability can become `READY`. |
| `-AdditionalPackageSource` | No | — | Repeatable `name=https://...` authenticated dnceng Azure Artifacts source without user information, query parameters, or fragments. Credentials come from `NuGetPackageSourceCredentials_<name>`, never from the argument, and must explicitly select `ValidAuthenticationTypes=Basic`. |

## Outputs

| File | Producer | Purpose |
|------|----------|---------|
| `trackers.json` | Find-Trackers | List of active tracker descriptors with detection evidence (one envelope per major) |
| `release-readiness.{json,md}` | Get-ReleaseReadiness | Full SR readiness report |
| `sr-source-prs.txt` | Get-ReleaseReadiness | Flat newline-delimited source PR list; use `grep -qxF NNNNN file` for instant cherry-pick verification |
| `sr-commits.json` | Get-ReleaseReadiness | Raw SR-only commit metadata |
| `preview-readiness.{json,md}` | Get-PreviewReadiness | Full Preview/RC readiness report. Local non-public-safe net11 JSON includes `InternalOfficialBuilds`; public-safe JSON omits that property. |

## Tracker refresh workflow

`.github/workflows/release-readiness.yml` runs every three hours from **08:30–20:30 UTC daily**, plus targeted `issues`, `milestone`, and `push` refreshes, `workflow_dispatch`, and `pull_request` validation:

1. **`detect-trackers`** — runs `Find-Trackers -AllActiveMajors`, emits a matrix of tracker descriptors.
2. **`per-tracker-report`** — matrix-expanded job per tracker:
   - Dispatches to `Get-ReleaseReadiness.ps1` (SR) or `Get-PreviewReadiness.ps1` (Preview/RC) based on `branchType`.
   - Looks for an open tracker issue by the canonical marker `<!-- release-readiness-tracker: <key> -->`.
     - **Refresh path**: in shipped mode, prefer an open issue carrying the exact current shipped/hotfix generation marker; otherwise refresh the oldest labeled tracker in place so Release Captain Notes and subscriptions survive commit-to-commit generation changes. If the exact generation was intentionally closed, retire any stale generic opens and do not recreate it. For non-shipped trackers, adopt the oldest labeled tracker issue and close remaining duplicates.
     - **Create path**: open a new issue with the mandatory `area-infrastructure` ownership label (creation fails rather than producing a tracker the lifecycle lookup cannot recognize); attach `report` / `s/triaged` best-effort.
   - **Activity gate**: skip new-issue creation when `recentCommitCount == 0` AND no open tracker issue exists. (Existing open issues are still refreshed.)
3. **`validate`** — PR-trigger path. Runs the test suite + smoke-runs all three scripts. **Does not create or modify issues.**

### Reading trackers directly (ad-hoc status)

The same tracker issues the cron job maintains double as a **human-readable, always-on status board** — you don't have to re-run a 60-120s survey to answer "what's the status across releases?". Find every active release by **body marker** (not title — a title search also matches the release Epic and other `[Release Readiness]`-titled issues):

```bash
gh issue list --repo dotnet/maui --state open \
  --search 'in:body "<!-- release-readiness-tracker:"' \
  --json number,title,updatedAt,body --limit 50 |
  jq 'map(select((.body // "" | split("\n") | map(select(startswith("<!-- release-readiness-tracker: ") and endswith(" -->"))) | length) > 0) | del(.body))'
```

The `jq` filter rejects GitHub search false positives by requiring an exact tracker-marker line. Each remaining result is one active SR or Preview. Read the body for the generated verdict **and** the human **Release Captain Notes** (between `<!-- release-readiness:human-notes:begin -->` / `:end -->`), which carry decisions that override the automated report. Treat the content as fresh only up to the issue's `updatedAt` (scheduled refreshes run every three hours from 08:30–20:30 UTC, with additional targeted event refreshes); re-run the survey script for a given branch when you need live numbers. The natural-language **`release-readiness-agent`** wraps this as its Portfolio path (§0a).

## Verdict Classification (SR & Preview)

Each candidate fix PR is classified with confidence + evidence:

| Verdict | Meaning |
|---------|---------|
| `in-sr-active` | Source PR is in the release branch and not subsequently reverted |
| `in-sr-reverted` | Backport landed but a later commit reverts it |
| `rejected-from-sr` | A backport PR targeting the release branch was opened and CLOSED unmerged |
| `backport-in-progress` | A backport PR targeting the release branch is OPEN |
| `merged-on-main-no-backport` | Fix merged to `main`, no backport PR exists. Remains Tier 2 in candidate mode because an already-selected Candidate cut commit can lag current `main`; rerun after the SR branch is cut to verify inclusion. |
| `merged-non-main-only` | Fix merged but only to `inflight/current` (or similar), not `main` |
| `open-on-main` | Fix PR is OPEN against main, not yet merged |
| `no-fix-yet` | No fix PR cross-referenced from the regression issue |
| `closed-fix-unlinked` | Issue is CLOSED and a closing comment **explicitly names** a fix PR (fix/resolve/close language) that is MERGED and present on the release branch, but the PR↔issue link was never recorded (no closing keyword / cross-reference). A bare mention of a PR (e.g. naming the *cause* PR for context) does **not** qualify. Non-blocking; action is to add a closing reference for traceability |
| `needs-human-review` | Evidence is contradictory or weak |

## SR backport handoff

This skill remains report-only: it MUST NOT post a comment, create a branch, or open a backport PR. When reporting a backport candidate:

1. For `merged-on-main-no-backport`, include this exact command in the recommendation for the **merged source PR**:

   ```text
   /backport to release/<major>.0.1xx-sr<N>
   ```

2. For `open-on-main`, wait for the source PR to merge, then recommend the same command. For `backport-in-progress`, do not trigger a duplicate backport.
3. For `merged-non-main-only`, do **not** recommend the command yet. Require a fix PR to merge into `main` first, then rerun readiness; only recommend the command after the merged source PR's ancestry is verified on `main`.
4. If the automation reports a conflict, recommend manually cherry-picking the source PR's **merge commit** onto the SR branch with `git cherry-pick -x`, resolving and testing the conflict, then opening a PR targeting the SR branch.

Read [Gotcha #4](references/methodology.md#gotcha-4-source-to-sr-backport-workflow) for the generated PR shape and manual fallback commands.

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
| **`Versions.props servicing flip`** | Live-SR mode only | `BLOCKED` if the SR branch's `eng/Versions.props` is not flipped to servicing-release mode (`PreReleaseVersionLabel=servicing` + `StabilizePackageVersion=true`). After the last backport, create a focused SR PR that preserves `PatchVersion`, replaces `ci.main` and its inflight conditional with `servicing`, and enables stable versions; rerun final CI after it merges. |
| **`Bug template lists SR version`** | All SR runs | `CLEANUP` if `.github/ISSUE_TEMPLATE/bug-report.yml` on `main` is missing an entry for the SR being shipped (users can't file bugs against the version) — post-release housekeeping, not a ship blocker. |
| **`Main bumped to next SR cycle`** | All SR runs | `BLOCKED` if the next SR cycle's version hasn't been promoted on `main`. The next action emits the exact one-line `PatchVersion` edit and PR title while preserving the mainline SDK-band and prerelease settings. |
| **`BAR default-channel mapping`** | SR branches matching `release/X.Y.Zxx-srN` | `BLOCKED` if the SR branch is not wired to the `.NET <band> SDK` channel in BAR. `UNKNOWN` if `darc` isn't on PATH (report includes the exact verification command). |
| **`BAR build for SR HEAD`** | SR branches with the SR HEAD SHA resolved | `READY` if BAR has a published build for the SR HEAD commit. `WATCH` (not blocking — transient) if CI hasn't published one yet. `UNKNOWN` if `darc` isn't on PATH or the build lookup fails (report includes the exact verification command). |
| **`Ship Assessment validation feed`** | SR branches with the SR HEAD SHA resolved | `READY` surfaces the per-build `darc-pub-dotnet-maui-<sha8>` NuGet feed URL to paste into the DevDiv ship **Assessment**, once `darc get-asset` confirms the build's published `NugetFeed` location. `WATCH` if the build isn't promoted to a channel yet (no channel → no feed → the Assessment has no validation feed to link — the SR9 miss), or if a promoted build's `NugetFeed` location isn't confirmed by `darc get-asset` yet (don't link the guessed per-build endpoint before BAR publishes it — `--skip-assets-publishing` can leave it missing). `UNKNOWN` when `darc` is unavailable or the build lookup fails, so the feed can't be resolved. |
| **`Milestone for current cycle`** | SR + preview branches | `BLOCKED` if the current cycle's milestone (e.g. `.NET 10 SR8` or `.NET 11.0-preview6`) doesn't exist in the GitHub milestone list — fixed issues have nowhere to land. |
| **`Milestone for next cycle`** | SR + preview branches | `CLEANUP` if the next cycle's milestone isn't pre-created — open issues can't roll forward when current ships, but it doesn't block the current release. **The preview train is `preview1…preview7 → rc1 → rc2 → GA` — there is no `preview8`**, so the cycle after `preview7` is `.NET <major>.0-rc1` (see [Pre-release train cadence](#pre-release-train-cadence-no-preview8)). After `rc2` the check is skipped entirely (GA doesn't use this naming). |
| **`Stale open milestones`** | SR + preview branches | `CLEANUP` if any milestones in the same major + same cycle type (SR, or the preview/rc train) are past their `due_on` by >7 days and still open (already-shipped releases accumulating untriaged issues). The **current** and **next** cycle milestones are excluded here — the next-cycle (roll-forward) target is handled by `Next-cycle milestone past due` below so it isn't mislabeled as shipped debt. |
| **`Next-cycle milestone past due`** | SR + preview branches | `CLEANUP` if the next-cycle (roll-forward) milestone *exists* but is >7 days past its `due_on`. This is **not** already-shipped debt — it's the target open issues roll forward to after the current cycle ships — so it's surfaced distinctly rather than dropped. Lane-agnostic (an overdue `SR<n+1>` while surveying `SR<n>` triggers it exactly as a slipped `rc1` does). A slip usually means the schedule moved (bump `due_on`) or the cycle was skipped and the milestone abandoned (triage + close). |
| **`CI Failure Scanner signals`** | All SR runs | `WATCH` if fresh ci-scan issues are filed in the last 24h. |
| **`Known Build Errors`** | All SR runs | `WATCH` if open Known Build Error issues exist that may explain background CI noise. |

### Next-cycle main bump workflow

After `release/<major>.0.1xx-sr<N>` is cut, `main` must move to the next SR
cycle before SR<N> ships. Use a focused PR targeting `main`:

1. Change only `PatchVersion` in `eng/Versions.props` from the current SR
   patch to `(N+1)*10`.
2. Title the PR `Update PatchVersion from <old> to <new>`.
3. Keep `SdkBandVersion`, `PreReleaseVersionLabel=ci.main`, and
   `StabilizePackageVersion=false` unchanged.
4. Keep this PR separate from the release branch's servicing-flip PR.

For SR9 → SR10, the complete source change is:

```diff
-    <PatchVersion>90</PatchVersion>
+    <PatchVersion>100</PatchVersion>
```

This is the same one-file, one-line pattern used for SR8 in
[#35433](https://github.com/dotnet/maui/pull/35433) (`70` → `80`) and SR9 in
[#35879](https://github.com/dotnet/maui/pull/35879) (`80` → `90`). The skill
remains report-only: it must explain this PR precisely, never edit or push
`main` itself.

### Expected ship date

The header line **`Expected ship date`** is rendered from `Get-ExpectedShipDate`, which reads `PatchVersion` from the survey ref's `eng/Versions.props` and applies the .NET release cadence:

| PatchVersion | Cadence | Example |
|--------------|---------|---------|
| Multiple of 10 (`80`, `90`, `100`…) — also **previews** (patch=`0`) | 2nd Tuesday of the month | SR8 (`10.0.80`) → next 2nd Tuesday |
| Anything else (`81`, `82`, `91`…) | **ASAP** — no fixed cadence | SR8 hotfix `10.0.81` → as soon as ready |

Surfaced in JSON as `expectedShipDate.{cadence, date, daysFromNow, formattedLong, note, patchVersion}` so downstream automation doesn't redo the math.

### Pre-release train cadence (no `preview8`)

.NET ships a **single ordered pre-release train per major**:

```
preview1 → preview2 → … → preview7 → rc1 → rc2 → GA
```

**`preview7` is the FINAL preview. There is no `preview8`.** Verified against dotnet/maui's own tags and milestones:

| Major | Last preview | Then | Milestones |
|-------|--------------|------|------------|
| .NET 9 | `9.0.0-preview.7.24407.4` | `9.0.0-rc.1.24453.9` → `9.0.0-rc.2.24503.2` | — |
| .NET 10 | `10.0.0-preview.7.25406.3` | `10.0.0-rc.1.25424.2` → `10.0.0-rc.2.25504.7` | `.NET 10.0-preview7` → `.NET 10.0-rc1` → `.NET 10.0-rc2` |

Two consequences the report must get right:

1. **Roll-forward milestone.** The cycle after `preview7` is `.NET <major>.0-rc1`, *not* `.NET <major>.0-preview8`. `Get-PreviewTrainMilestoneTitle` in [`Get-ReleaseReadiness.ps1`](scripts/Get-ReleaseReadiness.ps1) owns this mapping (ordinals `1..7` → previews, `8` → rc1, `9` → rc2, `10+` → `$null`); `$script:FinalPreviewNumber` is the single constant to change if the cadence ever moves. (.NET 5 shipped 8 previews; the 7-preview cadence has held for every major since .NET 6.)
2. **`preview7` is the feature/API-lock gate.** Because no eighth preview exists, work that misses the `preview7` cut does **not** roll into "the next preview" — RC is normally API-locked and go-live-licensed, so in practice it slips to the **next major**. When reporting on a `preview7` branch or candidate, say so explicitly: public-API PRs still open at the `preview7` cut are a *decide-now* item, not a defer-later one.

> **RC is a first-class tracker lane.** `Find-ReleaseReadinessTrackers.ps1` discovers strict `release/<major>.0.<band>xx-rc<N>` branches and RC tags independently of Preview. `Get-PreviewReadiness.ps1` validates the current `.NET <major>.0-rc<N>` milestone, expected `rc/N` version metadata, the outward MAUI default-channel mapping, Android/macOS-iOS inbound subscriptions, and the no-VMR-subscription reconciliation model.

### Maestro / BAR check gating

The BAR checks shell out to `darc` (cached probe via `Get-Command darc`). When darc isn't installed (most CI environments), both checks emit `UNKNOWN` with the exact local-verification command embedded in the row's `Next action` — so the report **never silently skips** them. The release-readiness agent runs the same checks via the `maestro_*` MCP tools when the script reports `UNKNOWN`.

## Methodology

Eight critical gotchas this skill encodes — see [references/methodology.md](references/methodology.md) for the full discussion:

1. **Cherry-pick number swap**: SR backports get NEW PR numbers (e.g. main #35356 → SR7 #35428). Cannot naively grep source PR numbers; must walk SR-only commits and extract refs from commit bodies.

2. **Timeline cross-references**: `closedByPullRequestsReferences` returns empty for most MAUI issues. The skill walks `gh api repos/.../issues/N/timeline` filtering on `cross-referenced` events.

3. **Forward-flow / non-main merges**: A fix can merge into `inflight/current` only, not `main` (real example: PR #35609). The skill checks `git merge-base --is-ancestor $mergeCommit origin/main` before claiming a fix is "on main, just needs backport".

4. **Source-to-SR backport workflow**: Only a merged fix whose merge commit is on `main` is ready for the `/backport to release/<branch>` automation. Non-main fixes must flow to `main` first; an automated conflict requires a manual `git cherry-pick -x` of the source merge commit.

5. **Servicing flip workflow**: The release branch must produce stable packages before ship. All .NET 10 SRs set `PreReleaseVersionLabel=servicing` and `StabilizePackageVersion=true`; a focused flip PR is the normal pattern, while SR8 validly inherited those values during its catch-up merge.

6. **Next-cycle main bump workflow**: After an SR branch is cut, `main` advances through a separate one-line `PatchVersion` PR. The report emits the exact old/new XML and title while preserving `SdkBandVersion` and CI prerelease settings.

7. **Default-channel → per-build feed → ship Assessment**: An SR branch needs a BAR default-channel mapping so its build is promoted and generates the per-build `darc-pub-dotnet-maui-<sha8>` NuGet feed. The DevDiv ship **Assessment** must link that feed so CSI/customers can validate the exact candidate packages; without the mapping + promotion the feed never exists and the Assessment ships incomplete (the SR9 miss). The report derives and surfaces the exact feed URL once a promoted build exists.

8. **Local internal official-build evidence stays local**: net11 readiness needs the
   latest definition-1095 build for both `net11.0` and the evaluated release
   branch, but GitHub Actions cannot access dnceng/internal. The preview engine
   auto-queries both only in eligible local runs, compares each build SHA to
   the newest trigger-eligible commit, fails open on unavailable auth, and
   removes all internal identifiers from public-safe output.

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

**When the banner is ❌ (feed STALE):** the dogfood bits have stopped flowing because the nightly **official signed build** is failing — pipeline `dotnet-maui` (definition **1095**, org `dnceng` / project `internal`), defined by [`eng/pipelines/ci-official.yml`](../../../eng/pipelines/ci-official.yml) and scheduled daily on `inflight/current`. See the [`azdo-build-investigator`](../azdo-build-investigator/SKILL.md) skill's **Nightly / Official Signed Build (inflight dogfood feed)** section for the investigation recipe and the recurring `vs-workload.props` (`MSB4019`) failure in the `Pack Windows` → "Build Workloads, Sign & Publish" step.

## Integration

- **Custom agent**: `.github/agents/release-readiness-agent.agent.md` wraps this skill — handles regression-label confirmation, runs the script, then uses WorkIQ to add context for `rejected-from-sr` PRs.
- **WorkIQ**: NOT called from the PowerShell scripts (PowerShell can't invoke MCP tools). The agent enriches the script's JSON output with WorkIQ context where needed.
- **.NET Release Tracker (Preview blessed build)**: for Previews, the *authoritative* official build is designated in the private `dotnet-release-tracker` plugin, reached through dependency-flow's deterministic access gate (`Get-PreviewReleaseReadiness.ps1`). See [Preview: authoritative blessed-build source](#preview-authoritative-blessed-build-source-net-release-tracker) above; the tier table + opt-in live in `../dependency-flow/SKILL.md`.

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
- **Idempotent body hash** stability across re-runs — **SR trackers only** (the scheduled/event-driven workflow compares the embedded `<!-- release-readiness-hash: sha=... -->` marker against the live issue and skips the edit when the semantic content is unchanged, so re-runs don't churn the tracker). Preview trackers carry no hash marker and are refreshed on every scheduled run.
- **Nightly dogfood feed banner** (`NightlyFeed.ps1`) — offline unit coverage for the lane-label honest-labeling rule (`Format-NightlyFeedLaneLabel`), the `ci.inflight`-first / `ci.main`-false-green resolver, age→tier bucketing, the fail-open feed query (mocked `-Fetcher`), and the banner's fold into `Get-ReportSemanticHash` (tier change refreshes, same-tier day tick does not). All network-free via injected fixtures and explicit `-Now`.
- **Preview consumer installability** (`PreviewInstallability.ps1`) — offline fixtures cover CLI/NuGet version conversion, workload-set discovery with MSI exclusion, branch-pin coherence, source-role resolution, real manifest `alias-to` resolution for Android/Emscripten/runtime representative packs, platform prerequisites, isolated `<clear />` configuration, malformed/401/403=`UNKNOWN` semantics, verdict mapping, and public-output redaction.
