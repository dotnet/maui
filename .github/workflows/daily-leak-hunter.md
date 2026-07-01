---
name: "Daily Memory Leak Hunter"
description: |
  Periodic (every 12h) AI-driven, TWO-MODE memory-leak workflow for MAUI's managed code.
  Both modes run on a standard GitHub runner — no platform workload, no emulator, no
  simulator, no MAUI source build.

  PASS A — Runtime leak hunt. Scans the shared/managed surface (src/Core/src,
  src/Controls/src/Core, src/Essentials/src) for the leak signature — a long-lived/static
  root that holds a strong reference to a transient with no teardown — writes a
  control/leaky/mitigation xUnit repro (referencing the SHIPPED Microsoft.Maui.Controls
  package) that measures retention with WeakReference + a forced GC, runs it, and ONLY if
  the leak is empirically confirmed files a `[leak-scan]` issue with the metrics.

  PASS B — Leak-test coverage-gap scan (only when Pass A files nothing). Statically
  compares MAUI's concrete control surface against its memory-leak TEST coverage
  (src/Controls/tests/DeviceTests/Memory/MemoryTests.cs InlineData lists + per-control
  `*DoesNotLeak` device tests + the Appium MemoryTests). When a shipping control or
  high-value scenario has NO memory-leak test, it files a `[leak-test-gap]` issue
  proposing the precise missing test (with a ready-to-paste stub following the existing
  pattern). This keeps every run productive once the runtime surface is hardened, and
  proactively closes the holes through which future leaks slip in untested.

  Platform-specific (native peer / GREF / NSObject cycle) leaks are OUT OF SCOPE for Pass
  A — those need device tests, which run on the AzDO/Helix pipeline, not gh-aw. Pass A
  flags only what a unit test can prove on a standard runner; Pass B may legitimately
  point at device-test gaps (the scan is static; the proposed test runs later on Helix).

environment: gh-aw-agents

on:
  schedule: every 12h
  workflow_dispatch:

if: |
  github.repository == 'dotnet/maui'

permissions:
  contents: read
  issues: read

engine:
  id: copilot
  model: claude-opus-4.8

concurrency:
  group: "daily-leak-hunter"
  cancel-in-progress: false

timeout-minutes: 90
max-ai-credits: -1

tools:
  github:
    toolsets: [issues, search]
  edit:
  bash: ["dotnet", "git", "find", "ls", "cat", "grep", "head", "tail", "wc", "jq", "tee", "sed", "awk", "tr", "cut", "sort", "uniq", "xargs", "echo", "date", "mkdir", "test", "env", "basename", "dirname", "bash", "sh", "chmod", "curl"]

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
    # "[leak-scan] " for a confirmed runtime leak (Pass A) or "[leak-test-gap] " for a
    # missing-memory-leak-test proposal (Pass B). At most ONE issue per run.
    labels: [agentic-workflows]
    allowed-labels: [agentic-workflows]
    max: 1
  noop:
    report-as-issue: false
---

# Daily Memory Leak Hunter — dotnet/maui

You run a **two-pass** memory-leak workflow per run and file **at most ONE** issue.
Neither pass needs an emulator/simulator or a MAUI source build.

- **Pass A — runtime leak hunt.** Find ONE *new* cross-platform **managed** leak, prove it
  with a `dotnet test` against the shipped `Microsoft.Maui.Controls` package, and file a
  **`[leak-scan]`** issue **only if the test confirms it**.
- **Pass B — leak-test coverage-gap scan.** Run this **only if Pass A produced no issue**
  (no new confirmable leak — the expected case now that the managed surface is hardened).
  Statically find ONE shipping control / high-value scenario that has **no memory-leak
  test**, and file a **`[leak-test-gap]`** issue proposing the precise missing test.

So every run files exactly one of: a confirmed `[leak-scan]` leak, a `[leak-test-gap]`
coverage proposal, or (rarely) nothing. **Pass B is what keeps runs productive** — there is
always another untested control, and closing these gaps stops future leaks from shipping
untested.

All intermediate state goes under `/tmp/gh-aw/agent/` (each bash call is a fresh subshell;
persist anything you need). The only write you may perform is the single `create-issue`
safe-output. Never push, never open a PR, never comment, never edit product or test code in
the repo (Pass B only *proposes* a test in the issue body — it does not commit it).

## Hard rules — non-negotiable

1. **File at most ONE issue per run.** Prefer a Pass A `[leak-scan]` when a runtime leak is
   empirically confirmed; otherwise a Pass B `[leak-test-gap]`. Never file both.
2. **Pass A: only file on EMPIRICAL confirmation.** If your unit test does not show the
   Leaky scenario retaining while Control AND Mitigation release, file nothing in Pass A and
   fall through to Pass B. A false positive is far worse than a quiet Pass A.
3. **Pass A is managed cross-platform code only.** Restrict the hunt to `src/Core/src`,
   `src/Controls/src/Core`, and `src/Essentials/src`. Do NOT chase platform handler /
   renderer / native-peer leaks (`*.Android.cs`, `*.iOS.cs`, `Platform/**`) — those need
   device tests and are out of scope for Pass A.
4. **Pass A: skip weak-proxied code.** If the suspect uses `WeakEventManager`,
   `ConditionalWeakTable`, `WeakReference`, or any `Weak*Proxy`, it does not leak — move on.
5. **Per-mode de-dup against THIS SCANNER's own OPEN issues.** Before filing, fetch this
   workflow's open issues. For Pass A skip a leak already covered by an OPEN `[leak-scan]`
   issue (same rooting API / retention path); for Pass B skip a control/scenario already
   covered by an OPEN `[leak-test-gap]` issue. Do NOT suppress a Pass A candidate because
   AdamEssenmacher (or anyone else) has a repro/issue for it — duplicating those is fine. A
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

## Step 2 — Fetch this scanner's own OPEN issues (per-mode de-dup)

The only de-dup that matters is not posting a second OPEN copy of something THIS workflow
already filed — separately for each mode. You do **not** care about AdamEssenmacher's repro
branches or anyone else's issues for Pass A leaks — duplicating those is explicitly fine.

Fetch this scanner's own open issues (both modes) on the repo the workflow runs in:

```
gh issue list --repo "$GITHUB_REPOSITORY" --search '"[leak-scan]" in:title' \
  --state open --label memory-leak --limit 100 --json number,title,body \
  > /tmp/gh-aw/agent/my-open-leakscan.json
gh issue list --repo "$GITHUB_REPOSITORY" --search '"[leak-test-gap]" in:title' \
  --state open --label memory-leak --limit 100 --json number,title,body \
  > /tmp/gh-aw/agent/my-open-testgap.json
```

- A **Pass A** candidate is OUT only if an open `[leak-scan]` issue already covers the same
  rooting API / retention path.
- A **Pass B** candidate is OUT only if an open `[leak-test-gap]` issue already proposes a
  test for the same control / scenario.

A candidate whose only prior issue from this scanner is CLOSED may be re-filed.

# ===================== PASS A — RUNTIME LEAK HUNT =====================

## Step 3 — Scan for the leak signature

### Step 3.0 — Pick this run's focus (rotate, so runs find DIFFERENT leaks)

To avoid fixating on the same area every run, compute a rotating focus index and scan that
area first/hardest this run:

```
# Rotate by run number so successive runs (and dispatches) explore different areas.
FOCUS=$(( ${GITHUB_RUN_NUMBER:-1} % 8 ))
echo "focus index: $FOCUS"
```

| Index | Focus this run (all PURELY MANAGED — testable on plain `net10.0`) |
|------:|----------------|
| 0 | `static event` / static delegate fields in `src/Controls/src/Core` and `src/Core/src` |
| 1 | `static` mutable collections (`Dictionary`/`List`/`HashSet`/`ConcurrentDictionary`) holding transients (exclude `ConditionalWeakTable` + type/registry caches) |
| 2 | Shared `ResourceDictionary` / `MergedDictionaries` / resources-changed-listener plumbing |
| 3 | Binding / `DynamicResource` / `AppThemeBinding` plumbing |
| 4 | Shared bindable values (a collection or `Element` used as a resource) the element subscribes to via `CollectionChanged` / `PropertyChanged` without a weak proxy |
| 5 | Animation / `IAnimationManager` / tweener / ticker plumbing |
| 6 | `AttachedCollection` / triggers / behaviors / VisualStateManager retention |
| 7 | Shapes / Geometry / Brush change-notification (`StrokeDashArray`, `GradientStops`, Path geometry, …) |

Treat the table as a *starting point*, not a cage — if the focus area is exhausted (its leaks
are already open `[leak-scan]` issues), move to the next index. Over many runs the rotation
covers the whole managed surface. (Essentials sensors / `Connectivity` / `Battery` /
`DeviceDisplay` are intentionally absent — they need a device and are out of scope for this
no-emulator workflow.)

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
`root -> ... -> transient` with file:line citations, then cross-check Step 2. Pick the ONE
strongest candidate that is not already an open `[leak-scan]` issue (Step 2). If there is no
convincing candidate, stop and
create nothing (a quiet run is a success — the surface is heavily hardened, so most runs find
nothing; the value is catching NEW leaks as code lands).

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

`/tmp/gh-aw/agent/leakprobe/LeakTest.cs`: a single `[Fact]` that
1. allocates N (e.g. 30) subjects, each owning a `new byte[1024*1024]` payload tracked by a `WeakReference`;
2. runs Control / Leaky / Mitigation;
3. forces a full GC (`for (i in 0..6) { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }`);
4. asserts `Assert.Equal(N, leakyAlive)`, `Assert.Equal(0, controlAlive)`, `Assert.Equal(0, mitigationAlive)`.

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

- If it **passes**, the leak is confirmed (Leaky retains; Control + Mitigation release).
- If it **fails to build** (the API isn't in the shipped package) or the assertions don't hold,
  your hypothesis is wrong — iterate once on a different candidate, otherwise stop and create
  nothing.

## Step 6 — File the issue (Pass A — only on a confirmed leak)

If (and only if) Step 5 confirmed the leak, emit exactly one `create-issue` safe-output and
**stop** (do not run Pass B). Title MUST start with the literal tag **`[leak-scan] `**
followed by a precise one-liner naming the rooting API. Body (markdown):

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

If **no** leak was confirmed this run, file nothing in Pass A and **continue to Pass B**.

# ===================== PASS B — LEAK-TEST COVERAGE-GAP SCAN =====================

Run Pass B **only if Pass A filed nothing.** Pass B is purely static — it READS repo source
and files a proposal; it never builds, runs tests, or commits code. The goal: find ONE
shipping control (or a high-value, currently-untested leak scenario) that has **no
memory-leak test**, and propose the specific test that would cover it.

## Step 7 — Build the memory-leak TEST coverage map

Read the repo's existing memory-leak tests and extract the set of types/scenarios already
covered. The authoritative sources (read all that exist):

```
# (1) Central device tests — the InlineData lists are the main coverage signal.
sed -n '1,800p' src/Controls/tests/DeviceTests/Memory/MemoryTests.cs
# (2) Per-control device tests with a *DoesNotLeak / memory assertion.
grep -rlnE "DoesNotLeak|AssertionExtensions.*[Ll]eak|new WeakReference" \
  src/Controls/tests/DeviceTests/Elements
# (3) Appium-side memory tests (controls that were moved out of device tests).
sed -n '1,200p' src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/MemoryTests.cs
```

From these, list every control/type that has memory-leak coverage today (e.g. the
`HandlerDoesNotLeak` `[InlineData(typeof(X))]` set, `PagesDoNotLeak`, the cells theory,
`BindableLayout`, `Window`, plus per-control files like Border/Label/ScrollView/
NavigationPage/TabbedPage/FlyoutPage/CollectionView/CarouselView/Slider/TitleBar, and the
Appium DatePicker/WebView/Image). Note commented-out / `ActiveIssue`-linked entries (e.g.
`CollectionView2`) — those are KNOWN gaps and are valid targets.

## Step 8 — Enumerate the concrete control surface and diff

Enumerate the **public, concrete, parameterless-constructible** controls that ship in
`Microsoft.Maui.Controls` — i.e. real views/layouts/pages a user instantiates:

```
# Public concrete View/Layout/Page/Cell subtypes (skip abstract/internal/<T> generic bases).
grep -rnE "public (sealed )?class [A-Za-z0-9_]+ *: *(View|Layout|TemplatedView|ContentView|Page|Cell|VisualElement|StackBase|Shell|GraphicsView|StackLayout|Compatibility\.)" \
  src/Controls/src/Core | grep -vE "abstract|internal|<" | sort -u
```

Cross-reference with `src/Controls/src/Core/Layout` (FlexLayout, AbsoluteLayout, StackLayout,
VerticalStackLayout, HorizontalStackLayout, Grid, …) and the Shell family.

**The gap set = concrete shipping controls that appear NOWHERE in the Step 7 coverage map**
(and are not already an open `[leak-test-gap]` issue from Step 2). Known examples at time of
writing (verify against the live tree — some may have gained coverage): `FlexLayout`,
`AbsoluteLayout`, `StackLayout`, `Shell`, `MenuBar`, plus any commented-out InlineData such
as `CollectionView2` (CV2 handler). Prefer a control that is **widely used** and whose leak
would be **high-impact** (layouts and Shell rank high; obscure cells rank low).

Pick the ONE strongest gap. If — and only if — you genuinely find NO uncovered concrete
control and NO commented-out coverage entry, file nothing (a fully-covered surface is a valid
quiet outcome, but it is rare; look thoroughly before giving up).

## Step 9 — File the coverage-gap issue (Pass B)

Emit exactly one `create-issue` safe-output. Title MUST start with the literal tag
**`[leak-test-gap] `** followed by a precise one-liner, e.g.
`[leak-test-gap] FlexLayout has no memory-leak (HandlerDoesNotLeak) test`. Body (markdown):

- A clear **AI-generated** banner naming this workflow (Pass B / coverage-gap scan).
- **The gap**: which control/scenario, and proof it is untested — cite the coverage sources
  you checked (file paths) and show it is absent from the `HandlerDoesNotLeak` InlineData
  list and from every per-control memory test. If it's a commented-out InlineData, link the
  tracking issue in the comment.
- **Why it matters**: how the control is used, and a plausible retention path that such a
  test would catch (cite product file:line if you can — e.g. a non-weak subscription).
- **Proposed test** — a ready-to-paste change that follows the EXISTING pattern. Usually
  that's a single new `[InlineData(typeof(TheControl))]` line on the `HandlerDoesNotLeak`
  theory in `src/Controls/tests/DeviceTests/Memory/MemoryTests.cs` (plus, if the control
  needs a handler registration, the matching `handlers.AddHandler<...>()` line in
  `SetupBuilder`). For a scenario gap (not a whole control), paste a small new `[Fact]` test
  body modeled on the nearby tests. Make it copy-paste-ready.
- **Where it runs**: note that this test executes on the AzDO/Helix **device-test** pipeline
  (`maui-pr-devicetests`), not on gh-aw — Pass B's scan is static; the proposed test is what
  runs on a device later.
- A short **scope note**: is this a true coverage hole, or is the control intentionally
  excluded (if so, say why and don't file).

If Pass B also finds nothing to file, produce no output. That is acceptable but should be
rare — there is almost always another untested control.
