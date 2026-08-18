---
name: "Daily Memory Leak Hunter"
description: |
  Periodic (every 12h) AI-driven memory-leak workflow for MAUI's managed code. Runs on a
  standard GitHub runner — no platform workload, no emulator, no simulator, no MAUI source
  build.

  Runtime leak hunt. Scans the shared/managed surface (src/Core/src, src/Controls/src/Core,
  src/Essentials/src) for the leak signature — a long-lived/static root that holds a strong
  reference to a transient with no teardown — writes a control/leaky/mitigation xUnit repro
  (referencing the SHIPPED Microsoft.Maui.Controls package) that measures retention with
  WeakReference + a forced GC, runs it, and ONLY if the leak is EMPIRICALLY CONFIRMED by the
  test files a `[leak-scan]` issue with the metrics. It files ONLY proven leaks — no
  coverage-gap / "missing test" proposals. If a run confirms nothing, it files nothing.

  Platform-specific (native peer / GREF / NSObject cycle) leaks are OUT OF SCOPE — those need
  device tests, which run on the AzDO/Helix pipeline, not gh-aw. This workflow flags only what
  a unit test can prove on a standard runner.

# ###############################################################
# Select a PAT from the pool and override COPILOT_GITHUB_TOKEN.
# Run agentic jobs in an isolated `copilot-pat-pool` environment.
#
# When org-level billing is available, this will be removed.
# See `shared/pat_pool.README.md` for more information.
# ###############################################################
imports:
  - uses: shared/pat_pool.md
    with:
      environment: copilot-pat-pool
environment: copilot-pat-pool

on:
  schedule: every 12h
  workflow_dispatch:
  # Forces a no-op pre_activation job, required by the pat_pool import. See shared/pat_pool.README.md.
  permissions: {}

if: |
  github.repository == 'dotnet/maui'

permissions:
  contents: read
  issues: read
  pull-requests: read

model: gpt-5.6-sol
engine:
  id: copilot
  env:
    COPILOT_GITHUB_TOKEN: ${{ case(needs.pat_pool.outputs.pat_number == '0', secrets.COPILOT_PAT_0, needs.pat_pool.outputs.pat_number == '1', secrets.COPILOT_PAT_1, needs.pat_pool.outputs.pat_number == '2', secrets.COPILOT_PAT_2, needs.pat_pool.outputs.pat_number == '3', secrets.COPILOT_PAT_3, needs.pat_pool.outputs.pat_number == '4', secrets.COPILOT_PAT_4, needs.pat_pool.outputs.pat_number == '5', secrets.COPILOT_PAT_5, needs.pat_pool.outputs.pat_number == '6', secrets.COPILOT_PAT_6, needs.pat_pool.outputs.pat_number == '7', secrets.COPILOT_PAT_7, needs.pat_pool.outputs.pat_number == '8', secrets.COPILOT_PAT_8, needs.pat_pool.outputs.pat_number == '9', secrets.COPILOT_PAT_9, 'NO COPILOT PAT AVAILABLE') }}

concurrency:
  group: "daily-leak-hunter"
  cancel-in-progress: false

timeout-minutes: 90
max-ai-credits: -1
max-daily-ai-credits: -1

tools:
  github:
    toolsets: [issues, search]
  edit:
  bash: ["dotnet", "git", "gh", "find", "ls", "cat", "grep", "head", "tail", "wc", "jq", "tee", "sed", "awk", "tr", "cut", "sort", "uniq", "xargs", "echo", "date", "mkdir", "test", "env", "basename", "dirname", "bash", "sh", "chmod", "curl"]

checkout:
  fetch-depth: 50

network:
  allowed:
    - defaults
    - github
    - dotnet
    - "*.blob.core.windows.net"

safe-outputs:
  create-issue:
    # No auto title-prefix: the agent writes the FULL title, starting with the mode tag —
    # The agent writes the FULL title, starting with the tag "[leak-scan] ". Up to `max`
    # issues per run — one per DISTINCT empirically-confirmed leak (sweep all focus areas).
    # ONLY proven leaks — never a coverage-gap / "missing test" proposal.
    labels: [agentic-workflows, "perf/memory-leak 💦"]
    allowed-labels: [agentic-workflows, "perf/memory-leak 💦"]
    max: 8
  noop:
    report-as-issue: false
---

# Daily Memory Leak Hunter — dotnet/maui

You run a **runtime memory-leak hunt** per run. It does not need an emulator/simulator or a
MAUI source build.

- **Find as many *new* cross-platform managed leaks as you can this run** — **sweep every focus
  area**, not just one — prove **each** with a `dotnet test` against the shipped
  `Microsoft.Maui.Controls` package, and file a **`[leak-scan]`** issue for **each DISTINCT
  empirically-confirmed leak** (up to the 8-issue cap). Filing several strong, test-proven
  leaks in one run is the goal — do not stop after the first.

You file **ONLY empirically-proven leaks**. There is **no** coverage-gap / "missing test" mode —
never file a `[leak-test-gap]` or any "this control has no test" issue. If a run confirms no new
leak, it files **nothing** (a quiet run is a perfectly good outcome).

All intermediate state goes under `/tmp/gh-aw/agent/` (each bash call is a fresh subshell;
persist anything you need). The only write you may perform is the `create-issue` safe-output.
Never push, never open a PR, never comment, never edit product or test code in the repo.

## Hard rules — non-negotiable

1. **File one issue per DISTINCT confirmed leak, up to 8 per run.** Sweep all focus areas and
   file a `[leak-scan]` for every runtime leak you empirically confirm this run. If you confirm
   **none**, file nothing — there is no fallback mode.
2. **Only file on EMPIRICAL confirmation.** If your unit test does not show the Leaky scenario
   retaining while Control AND Mitigation release, file nothing. A false positive is far worse
   than a quiet run.
3. **Managed cross-platform code only.** Restrict the hunt to `src/Core/src`,
   `src/Controls/src/Core`, and `src/Essentials/src`. Do NOT chase platform handler /
   renderer / native-peer leaks (`*.Android.cs`, `*.iOS.cs`, `Platform/**`) — those need
   device tests and are out of scope.
4. **Skip weak-proxied code.** If the suspect uses `WeakEventManager`,
   `ConditionalWeakTable`, `WeakReference`, or any `Weak*Proxy`, it does not leak — move on.
5. **De-dup against THIS SCANNER's own OPEN issues AND leaks already fixed on `main`.** Before
   filing, fetch this workflow's open `[leak-scan]` issues and skip a leak already covered by one
   (same rooting API / retention path). ALSO skip the same retention path when Step 2 proves a
   workflow-owned `[leak-fix]` PR landed on `main` but is not yet contained in the pinned shipped
   package. A Type.Member match alone is insufficient: a later regression or second mechanism on
   the same property remains eligible. A close *reason* alone is not proof of a fix. Such
   unshipped fixes still reproduce against the package, so the empirical test alone cannot tell;
   the issue-linked provenance is the guard. Do NOT suppress a candidate because
   AdamEssenmacher (or anyone else) has a repro/issue for it — duplicating those is fine. A
   candidate whose only prior issue from this scanner is CLOSED with **no merged `[leak-fix]` PR**
   (any close reason) may be re-filed if it still reproduces.
6. **Never weaken or disable anything, and never commit code.** You only READ repo source
   and (Pass A) ADD a throwaway test under `/tmp`. Never edit product code, never
   `[ActiveIssue]`/skip/mute existing tests, never push.
7. **AI attribution.** Every issue body must clearly state it was generated by this workflow.

## Step 1 — Environment

1. Confirm the SDK is present: `dotnet --version` (the runner already has the .NET SDK).
2. **Do NOT build MAUI from source.** The MAUI build SDK (`Microsoft.DotNet.Arcade.Sdk`) lives
   on Azure DevOps feeds this runner cannot reach (NuGet returns 403). Instead you verify every
   candidate with a **standalone** test that references the **shipped `Microsoft.Maui.Controls`
   NuGet package** from nuget.org (Step 4) — no source build, no workload, no emulator.

## Step 2 — Fetch this scanner's own OPEN issues (de-dup)

The only de-dup that matters is not posting a second OPEN copy of a leak THIS workflow already
filed. You do **not** care about AdamEssenmacher's repro branches or anyone else's issues —
duplicating those is explicitly fine.

Fetch this scanner's own open `[leak-scan]` issues (they are filed with the `agentic-workflows`
label) and retain their full leak identity:

```
gh issue list --repo "$GITHUB_REPOSITORY" --search '"[leak-scan]" in:title' \
  --state open --label agentic-workflows --limit 200 --json number,title,body \
  > /tmp/gh-aw/agent/my-open-leakscan.json
# The rooting API is the LAST dotted Type.Member pair of the first identifier chain. Preserve the
# issue title/body and canonical marker too: API equality is only a prefilter, because two
# different retention paths can share one property.
jq '
  def titleapi:
    [(.title // "") | scan("[A-Za-z_][A-Za-z0-9_]*(?:\\.[A-Za-z_][A-Za-z0-9_]*)+")]
    | if length > 0 then (.[0] | split(".") | .[-2:] | join(".")) else null end;
  def leakkey:
    [(.body // "") | capture("(?i)<!-- *leak-scan-key: *(?<key>[^>]+?) *-->").key]
    | if length > 0 then .[0] else null end;
  [.[] | {scan_issue:., rooting_api:titleapi, leak_scan_key:leakkey}]
' /tmp/gh-aw/agent/my-open-leakscan.json > /tmp/gh-aw/agent/open-leakscan-provenance.json
jq -r '.[] | "open scan #\(.scan_issue.number): API \(.rooting_api // "<off-contract>"), key \(.leak_scan_key // "<legacy>")"' \
  /tmp/gh-aw/agent/open-leakscan-provenance.json

# ── ALSO skip the SAME leak while its merged fix is on main but NOT in the shipped package ───
# Step 5 tests the shipped package pinned below. A fix newer than that package still reproduces
# there, so provenance must suppress it temporarily. Suppression MUST end once the pinned release
# contains the fix, and MUST distinguish two retention paths rooted at the same Type.Member.
#
# The durable cross-workflow join is the exact same-repo `Fixes #<scan-issue>` line copied into
# every [leak-fix] PR body. Resolve that issue and retain its full title/body; never infer leak
# identity from the independently-authored PR title. New scan issues also carry a canonical
# `leak-scan-key` marker, while legacy issues without the marker remain comparable from their
# original title/body. A Type.Member match alone is only a candidate for semantic comparison —
# it is NEVER sufficient to suppress a different retention mechanism.
SHIPPED_MAUI_VERSION=10.0.0
printf '%s\n' "$SHIPPED_MAUI_VERSION" > /tmp/gh-aw/agent/shipped-maui-version.txt
REPO_RE=$(printf '%s' "$GITHUB_REPOSITORY" | sed -E 's/[][(){}.^$*+?|\\]/\\&/g')
gh pr list --repo "$GITHUB_REPOSITORY" --state merged --search '"[leak-fix]" in:title label:agentic-workflows base:main' \
  --limit 1000 --json id,number,title,body,mergedAt,mergeCommit > /tmp/gh-aw/agent/merged-leakfix-raw.json
if [ "$(jq 'length' /tmp/gh-aw/agent/merged-leakfix-raw.json)" -ge 1000 ]; then
  echo "WARNING: merged [leak-fix] provenance hit the 1000 search-API ceiling; older unshipped fixes may be TRUNCATED and re-filed once — switch to date-windowed enumeration."
fi

# A tag timestamp does not prove ancestry: a release-branch tag can be newer than an unrelated
# main merge while excluding it. Retain every merged PR with current exact Fixes provenance until
# GraphQL edit verification and both compare APIs establish containment.
jq -L "$GITHUB_WORKSPACE/.github/scripts" --arg repo "$REPO_RE" '
  include "leak-workflow-provenance";
  [.[] | select((leak_exact_fixes_numbers($repo) | length) > 0)]' \
  /tmp/gh-aw/agent/merged-leakfix-raw.json \
  > /tmp/gh-aw/agent/merged-leakfix-unshipped-candidates.json

printf '' > /tmp/gh-aw/agent/unshipped-main-fixes.ndjson
printf '' > /tmp/gh-aw/agent/merged-leakfix-compare-failures.txt
printf '' > /tmp/gh-aw/agent/merged-leakfix-provenance-failures.txt
COMPARE_CANDIDATE_COUNT=$(jq 'length' /tmp/gh-aw/agent/merged-leakfix-unshipped-candidates.json)
jq -c '.[]' /tmp/gh-aw/agent/merged-leakfix-unshipped-candidates.json | while IFS= read -r pr; do
  node_id=$(jq -r '.id // empty' <<<"$pr")
  sha=$(jq -r '.mergeCommit.oid // empty' <<<"$pr")
  [ -z "$node_id" ] && continue
  [ -z "$sha" ] && continue
  edit_meta=$(gh api graphql -f id="$node_id" -f query='
    query($id: ID!) {
      node(id: $id) {
        ... on PullRequest {
          mergedAt
          lastEditedAt
        }
      }
    }' 2>/dev/null) || edit_meta=""
  provenance_guard=$(jq -L "$GITHUB_WORKSPACE/.github/scripts" -c '
    include "leak-workflow-provenance";
    .data.node | leak_merge_provenance_guard' <<<"$edit_meta" 2>/dev/null) || provenance_guard=""
  if [ -z "$provenance_guard" ] ||
     [ "$(jq -r '.verified' <<<"$provenance_guard")" != "true" ] ||
     [ "$(jq -r '.block_provenance' <<<"$provenance_guard")" = "true" ]; then
    printf '%s\n' "$(jq -r '.number' <<<"$pr")" >> /tmp/gh-aw/agent/merged-leakfix-provenance-failures.txt
    continue
  fi
  # Only workflow PRs with an exact SAME-REPO Fixes reference have scan provenance. A Refs line
  # points at a separate upstream issue and an arbitrary cross-repo #N must never become a join.
  scan_n=$(jq -L "$GITHUB_WORKSPACE/.github/scripts" -r --arg repo "$REPO_RE" '
    include "leak-workflow-provenance";
    leak_first_exact_fixes_number($repo)' <<<"$pr")
  [ -z "$scan_n" ] && continue

  # The merge commit must be contained in main, but not in the shipped release tag. If either
  # compare cannot be verified, fail open (do not suppress): a bounded duplicate is safer than
  # permanently hiding a real leak.
  on_main=$(gh api "repos/$GITHUB_REPOSITORY/compare/main...$sha" -q '.ahead_by' 2>/dev/null) || on_main=""
  if [ -z "$on_main" ]; then
    printf '%s main\n' "$sha" >> /tmp/gh-aw/agent/merged-leakfix-compare-failures.txt
    continue
  fi
  [ "$on_main" != "0" ] && continue

  in_shipped=$(gh api "repos/$GITHUB_REPOSITORY/compare/$SHIPPED_MAUI_VERSION...$sha" -q '.ahead_by' 2>/dev/null) || in_shipped=""
  if [ -z "$in_shipped" ]; then
    printf '%s shipped\n' "$sha" >> /tmp/gh-aw/agent/merged-leakfix-compare-failures.txt
    continue
  fi
  [ "$in_shipped" = "0" ] && continue

  issue=$(gh issue view "$scan_n" --repo "$GITHUB_REPOSITORY" --json number,title,body,state 2>/dev/null) || issue=""
  [ -z "$issue" ] && continue
  api=$(jq -r '.title // ""' <<<"$issue" \
    | sed -E 's/^\[leak-scan\] *//' \
    | awk '{ if (match($0, /[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)+/)) { chain=substr($0,RSTART,RLENGTH); n=split(chain,seg,"."); print seg[n-1]"."seg[n] } }')
  leak_key=$(jq -r '(.body // "") | capture("(?i)<!-- *leak-scan-key: *(?<key>[^>]+?) *-->").key // empty' <<<"$issue")
  jq -n --argjson pr "$pr" --argjson issue "$issue" --arg api "$api" --arg key "$leak_key" \
    '{pull_request:{number:$pr.number,title:$pr.title,mergedAt:$pr.mergedAt,mergeCommit:$pr.mergeCommit},
      scan_issue:$issue, rooting_api:$api,
      leak_scan_key:($key | if . == "" then null else . end)}' \
    >> /tmp/gh-aw/agent/unshipped-main-fixes.ndjson
done
PROVENANCE_FAILURE_COUNT=$(wc -l < /tmp/gh-aw/agent/merged-leakfix-provenance-failures.txt | tr -d ' ')
if [ "$PROVENANCE_FAILURE_COUNT" -gt 0 ]; then
  echo "WARNING: merge-time body provenance was mutable or unverified for $PROVENANCE_FAILURE_COUNT merged fixes — those fixes cannot suppress scans."
fi
COMPARE_FAILURE_COUNT=$(wc -l < /tmp/gh-aw/agent/merged-leakfix-compare-failures.txt | tr -d ' ')
if [ "$COMPARE_FAILURE_COUNT" -gt 0 ]; then
  echo "WARNING: ancestry compare failed for $COMPARE_FAILURE_COUNT/$COMPARE_CANDIDATE_COUNT prefiltered merged fixes (tag \"$SHIPPED_MAUI_VERSION\" resolvable?) — unshipped-fix suppression is degraded."
fi
if [ -s /tmp/gh-aw/agent/unshipped-main-fixes.ndjson ]; then
  jq -s '.' /tmp/gh-aw/agent/unshipped-main-fixes.ndjson > /tmp/gh-aw/agent/unshipped-main-fixes.json
else
  echo '[]' > /tmp/gh-aw/agent/unshipped-main-fixes.json
fi
jq -r '.[] | "unshipped main fix: PR #\(.pull_request.number), scan #\(.scan_issue.number), API \(.rooting_api // "<off-contract>"), key \(.leak_scan_key // "<legacy>")"' \
  /tmp/gh-aw/agent/unshipped-main-fixes.json
```

- A candidate is **OUT** when an entry in `open-leakscan-provenance.json` covers the same leak.
  Narrow by rooting `Type.Member`; a matching non-empty `leak-scan-key` is sufficient. Otherwise
  compare the full title/body and confirm the same publisher/event/collection and retention path.
  A same-API issue with a different mechanism does not block the candidate. **Check this for
  EVERY candidate before you write its test** — re-filing the same retention path is the #1
  failure mode, but over-collapsing distinct paths loses real regressions.
- A candidate is **ALSO OUT** only when an entry in `unshipped-main-fixes.json` describes the
  **same leak**, not merely the same API. First narrow by `rooting_api`; then require the same
  non-empty `leak-scan-key`, or compare the referenced scan issue's title/body and confirm the
  same publisher/event/collection and the same
  `root -> ... -> transient` retention edge. If the Type.Member matches but the retention
  mechanism differs, the candidate remains eligible. Off-contract records are never matched by an
  independently slugified PR title; use their referenced scan issue title/body directly.
  These records contain only fixes that are on `main` and absent from the pinned shipped release,
  so suppression expires automatically when `SHIPPED_MAUI_VERSION` advances to a tag containing
  the fix.

A candidate whose only prior `[leak-scan]` issue was **closed with no merged `[leak-fix]` PR**
(closed as not planned — wontfix / invalid / duplicate — OR closed as completed by a maintainer
without a landed fix) may be re-filed if it still reproduces: a close *reason* is not proof the
leak is gone. A candidate matching an entry in `unshipped-main-fixes.json` must **not** be
re-filed while that exact fix is absent from the pinned package.

# ===================== RUNTIME LEAK HUNT =====================

## Step 3 — Scan for the leak signature

### Step 3.0 — Sweep ALL focus areas this run (fan-out)

Do **not** limit the run to one area. Work through **every** focus area below and collect a
candidate list from each — the more areas you cover, the more distinct leaks you file. Start at
a rotating index just to vary ordering across runs, then continue through all of them:

```
# Rotate the STARTING point so successive runs vary order; but cover ALL areas.
START=$(( ${GITHUB_RUN_NUMBER:-1} % 8 ))
echo "start focus index: $START (then sweep all 8)"
```

Each row lists **catalog-proven signatures** — real MAUI leak shapes that have been confirmed
before. Hunt for NEW instances of these shapes (different control/API, same mechanism):

| Index | Focus (all PURELY MANAGED — testable on plain `net10.0`) — proven signatures to hunt |
|------:|----------------|
| 0 | `static event` / static delegate fields in `src/Controls/src/Core` + `src/Core/src` (e.g. `AppActions.OnAppAction`-style static publishers never unsubscribed) |
| 1 | `static` mutable collections (`Dictionary`/`List`/`HashSet`/`ConcurrentDictionary`) holding transients (static route tables, request-tables, `AnimationExtensions.s_animations`-style caches; exclude `ConditionalWeakTable` + type/registry caches) |
| 2 | Shared `ResourceDictionary` retained via `MergedDictionaries` **or** direct `VisualElement.Resources` assignment — strong `ValuesChanged` subscription from a page-local dict with no unload teardown |
| 3 | Binding / `DynamicResource` / `AppThemeBinding` plumbing (shared source strongly rooting the target) |
| 4 | **Shared publisher → strong subscription with no weak proxy** — the #1 catalog pattern. A control subscribes to an *external/shared/long-lived* collection or element via `CollectionChanged` / `PropertyChanged` and never unsubscribes: `Picker.ItemsSource`, `TableView`/`TableRoot` section collection, `SelectableItemsView.SelectedItems`, `CarouselView` item source, etc. |
| 5 | **Shared `ICommand` → `CanExecuteChanged`** roots the control (strong subscription, no teardown): `ListView.RefreshCommand`, `SwipeItemView.Command`, `BackButtonBehavior.Command`, `MenuItem.Command`, toolbar/refresh commands. |
| 6 | Animation / `IAnimationManager` / tweener / ticker plumbing (tickers/animators not stopped/disposed on teardown; `AnimatableKey`/animation-cache retention) |
| 7 | `AttachedCollection` / triggers / behaviors / **VisualStateManager**: state triggers (`CompareStateTrigger`, `AdaptiveTrigger`, `StateTriggerBase` subclasses) that subscribe to a shared managed source and stay subscribed when VSGroups are replaced (test the trigger's own subscribe/unsubscribe bookkeeping — do NOT use platform display sources like `DeviceDisplay`, which throw `NotImplementedInReferenceAssembly` on plain `net`); `Shapes`/`Geometry`/`Brush` change-notification (`StrokeDashArray`, `GradientBrush.GradientStops`, `Path` geometry). |

Cover every row. Within a row, look for MULTIPLE instances (e.g. area 5 alone spans several
controls that each take an `ICommand`). If a row is fully hardened or already covered by open
`[leak-scan]` issues, note it and move on. Over the sweep you should surface several candidates.

### Step 3.1 — Hunt

Look for a **long-lived / static / singleton / shared root that holds a STRONG reference to a
transient object (page / view / view-model / handler) with no teardown**, e.g.:

- A `static` event (plain delegate, NOT `WeakEventManager`) whose subscribers are never
  removed (`grep -rn "static event" src/Core/src src/Controls/src/Core src/Essentials/src`).
- A `static` mutable collection (`Dictionary`/`List`/`HashSet`/`ConcurrentDictionary`) that
  is `Add`ed to but whose removal is conditional on an event/callback that may not fire
  (`grep -rnE "static (readonly )?(Concurrent)?Dictionary|static .*List<" ...`). Exclude
  `ConditionalWeakTable` and type/registry caches (they hold types, not transients).
- An instance subscribing to an event on `Application.Current` / a singleton / a *shared*
  bindable value (e.g. a collection or `Element` set as a resource) where the cleanup runs
  only on a path that navigation-away doesn't trigger.

For each candidate, write down the precise retention path
`root -> ... -> transient` with file:line citations, then cross-check Step 2. **Collect EVERY
distinct candidate** across all focus areas that is not already an open `[leak-scan]` issue —
build a candidate list (aim for several). Rank them strongest-first, then confirm as many as
you can in Step 4/5. If — after a genuine sweep — there is no convincing candidate at all, stop
and create nothing (a quiet run is fine — there is no coverage-gap fallback).

## Step 4 — Write a standalone control/leaky/mitigation test (shipped package)

Create a self-contained xUnit project **outside the repo**, under
`/tmp/gh-aw/agent/leakprobe/`, that references the **shipped** `Microsoft.Maui.Controls`
package (so restore uses nuget.org, NOT the repo's Azure DevOps feeds):

`/tmp/gh-aw/agent/leakprobe/leakprobe.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Maui.Controls" Version="10.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>
</Project>
```

(Use a `10.0.x` version that restores; `10.0.0` is known to work.)

`/tmp/gh-aw/agent/leakprobe/LeakTest.cs`: **one `[Fact]` per candidate** (name them clearly,
e.g. `Picker_ItemsSource_Leaks`, `ListView_RefreshCommand_Leaks`), each of which:
1. allocates N (e.g. 30) subjects, each owning a `new byte[1024*1024]` payload tracked by a `WeakReference`;
2. runs Control / Leaky / Mitigation;
3. forces a full GC (`for (i in 0..6) { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }`);
4. asserts `Assert.Equal(N, leakyAlive)`, `Assert.Equal(0, controlAlive)`, `Assert.Equal(0, mitigationAlive)`.

Put ALL candidates in this ONE project (one restore, one `dotnet test` run confirms them all).
Each `[Fact]` that PASSES is one empirically-confirmed leak you will file in Step 6; each that
FAILS to build or assert is a wrong hypothesis — drop just that candidate and keep the rest.

**CRITICAL — pick a PURELY-MANAGED candidate that is testable on plain `net10.0`.** The leak's
subscribe/teardown path must use only managed types that work without a platform — e.g.
`AnimationManager`/`Ticker` (`Microsoft.Maui.Animations`), static collections/events in
`Microsoft.Maui.Controls`, bindings, `ResourceDictionary`, triggers, `AttachedCollection`.
**AVOID** any candidate whose path calls platform Essentials/handlers — `Accelerometer`,
`Connectivity`, `Battery`, `DeviceDisplay`, sensors, or anything under `Platform/**` — they
throw `NotImplementedInReferenceAssembly` on plain `net` and need a device (out of scope here).
If your focus area only yields platform-dependent candidates, switch to a managed-only area.

## Step 5 — Run it (no emulator, no source build)

```
cd /tmp/gh-aw/agent/leakprobe && dotnet test --logger "console;verbosity=normal"
```

This restores `Microsoft.Maui.Controls` from nuget.org and runs on the runner — no workload,
no MAUI source build, no emulator.

- For **each `[Fact]`**: if it **passes**, that leak is confirmed (Leaky retains; Control +
  Mitigation release) → file it in Step 6. If it **fails to build** (API not in the shipped
  package) or its assertions don't hold, that candidate's hypothesis is wrong — drop it and
  keep the others. Confirming several in one run is expected and good.

## Step 6 — File the issues (Pass A — one per confirmed leak)

For **every** leak Step 5 confirmed, emit a `create-issue` safe-output (up to the 8 cap) — one
issue per distinct leak. De-dup each against open `[leak-scan]` issues, against
`unshipped-main-fixes.json` (Step 2 — never re-file the same retention path while its fix is on
`main` but absent from the pinned package), AND against the other issues you're filing this run
(no two issues for the same retention path). Each title MUST be
of the form **`[leak-scan] <Type>.<Member> — <short mechanism>`** — it MUST **lead with the
canonical rooting `Type.Member`** immediately after the tag (e.g. `[leak-scan] SwipeItemView.Command — non-weak
ICommand.CanExecuteChanged retains the control`). Keep both the API and mechanism stable and
canonical — do not reword them run-to-run.
Body (markdown):

- A clear **AI-generated** banner naming this workflow.
- A hidden canonical marker immediately after the banner:
  `<!-- leak-scan-key: <Type.Member>|<short-mechanism-slug> -->`, where the slug is the lower-case
  `<short mechanism>` with each non-alphanumeric run replaced by `-` and leading/trailing `-`
  removed. This marker is copied unchanged into the eventual `[leak-fix]` PR and is the durable
  identity for this exact retention path; the Type.Member alone is not unique.
- **Description** of the leak and why it retains.
- **Retention path** `root -> ... -> transient` with file:line citations.
- **Repro**: paste the standalone `leakprobe.csproj` + `LeakTest.cs` (it restores the shipped
  `Microsoft.Maui.Controls` package and runs on plain `net10.0` — no device needed) and the
  `dotnet test` command.
- **Observed results** table (Control / Mitigation / Leaky alive counts + retained MB).
- **Affected platforms** (managed code → all) and the **disabling/non-default condition**
  if any.
- **Suggested fix** (e.g. use `WeakEventManager`, or clear the collection on the missing
  path) and a short, **honest scope note** (is it a clear framework bug, or a usage footgun
  the framework could harden?).

If **no** leak was confirmed this run, file nothing — a quiet run is a perfectly good outcome.
There is no coverage-gap fallback.