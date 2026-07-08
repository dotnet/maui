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

engine:
  id: copilot
  model: claude-opus-4.8
  env:
    COPILOT_GITHUB_TOKEN: |
      ${{ case(
        needs.pat_pool.outputs.pat_number == '0', secrets.COPILOT_PAT_0,
        needs.pat_pool.outputs.pat_number == '1', secrets.COPILOT_PAT_1,
        needs.pat_pool.outputs.pat_number == '2', secrets.COPILOT_PAT_2,
        needs.pat_pool.outputs.pat_number == '3', secrets.COPILOT_PAT_3,
        needs.pat_pool.outputs.pat_number == '4', secrets.COPILOT_PAT_4,
        needs.pat_pool.outputs.pat_number == '5', secrets.COPILOT_PAT_5,
        needs.pat_pool.outputs.pat_number == '6', secrets.COPILOT_PAT_6,
        needs.pat_pool.outputs.pat_number == '7', secrets.COPILOT_PAT_7,
        needs.pat_pool.outputs.pat_number == '8', secrets.COPILOT_PAT_8,
        needs.pat_pool.outputs.pat_number == '9', secrets.COPILOT_PAT_9,
        'NO COPILOT PAT AVAILABLE')
      }}

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
5. **De-dup against THIS SCANNER's own OPEN issues.** Before filing, fetch this workflow's open
   `[leak-scan]` issues and skip a leak already covered by one (same rooting API / retention
   path). Do NOT suppress a candidate because AdamEssenmacher (or anyone else) has a repro/issue
   for it — duplicating those is fine. A
   candidate whose only prior issue from this scanner is CLOSED may be re-filed.
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
label) and extract the **rooting API** each one already covers:

```
gh issue list --repo "$GITHUB_REPOSITORY" --search '"[leak-scan]" in:title' \
  --state open --label agentic-workflows --limit 200 --json number,title,body \
  > /tmp/gh-aw/agent/my-open-leakscan.json
# The rooting API is the "Type.Member" the title names. Titles SHOULD lead with it (Step 6),
# but real runs have produced off-contract titles like "Shell BackButtonBehavior.Command …"
# (#36345) vs "BackButtonBehavior.Command: …" (#36354). A prefix-only cut keys those on
# "Shell" vs "BackButtonBehavior.Command" and re-files a duplicate. Extract the LAST dotted
# Type.Member pair of the first identifier chain: for a fully-qualified title like
# "Microsoft.Maui.Controls.Picker.ItemsSource" this yields "Picker.ItemsSource" (not the
# namespace head "Microsoft.Maui", which would over-collapse distinct leaks to one key).
jq -r '.[].title' /tmp/gh-aw/agent/my-open-leakscan.json \
  | sed -E 's/^\[leak-scan\] *//' \
  | awk '{ if (match($0, /[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)+/)) { chain=substr($0,RSTART,RLENGTH); n=split(chain,seg,"."); print seg[n-1]"."seg[n] } else print }' \
  | sort -u \
  > /tmp/gh-aw/agent/already-filed-apis.txt
echo "already-filed rooting APIs:"; cat /tmp/gh-aw/agent/already-filed-apis.txt
```

- A candidate is **OUT** if its rooting `Type.Member` (e.g. `SwipeItemView.Command`,
  `Picker.ItemsSource`) is already in `already-filed-apis.txt`, OR an open `[leak-scan]` issue
  otherwise covers the same rooting API / retention path. **Check this for EVERY candidate
  before you write its test** — re-filing a leak this scanner already has open (even with
  different title wording) is the #1 failure mode, so be strict about matching the `Type.Member`.

A candidate whose only prior issue from this scanner is CLOSED may be re-filed.

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
issue per distinct leak. De-dup each against open `[leak-scan]` issues AND against the other
issues you're filing this run (no two issues for the same rooting API). Each title MUST be of the
form **`[leak-scan] <Type>.<Member> — <short mechanism>`** — it MUST **lead with the canonical
rooting `Type.Member`** immediately after the tag (e.g. `[leak-scan] SwipeItemView.Command — non-weak
ICommand.CanExecuteChanged retains the control`). De-dup (Step 2) matches on that leading
`Type.Member`, so keep it stable and canonical — do not reword it run-to-run.
Body (markdown):

- A clear **AI-generated** banner naming this workflow.
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
