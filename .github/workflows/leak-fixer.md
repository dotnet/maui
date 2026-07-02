---
name: "Memory Leak Fixer"
description: |
  Turns one open `[leak-scan]` issue (filed by the Daily Memory Leak Hunter) into a
  validated, draft `[leak-fix]` pull request. For the selected issue the agent:

  1. Locates the leaking product code in the CURRENT source tree (the leak was found in
     the shipped NuGet, so it may already be fixed on this branch — that is handled).
  2. Writes a focused **regression unit test** in `Controls.Core.UnitTests` (managed,
     library-TFM, no workload/emulator) that asserts the transient is collected.
  3. Builds + runs that test and confirms it **FAILS** on the unpatched source — proving
     the test actually catches the leak. (If it already passes, the leak is already fixed
     on this branch: the agent opens NO PR and stops.)
  4. Implements the product fix (weak subscription / `WeakEventManager` / teardown on the
     missing path), rebuilds, and confirms the **same test now PASSES** with no regression
     in its neighbours.
  5. Opens ONE draft `[leak-fix]` PR (`Fixes #<issue>`) carrying the test + fix, with the
     before/after evidence in the body.

  Detection (the hunter) uses the shipped package and needs no source build; the FIXER
  must build MAUI **from source** to validate a product change, so it restores from the
  public dnceng Azure DevOps feeds (`dev.azure.com`) — hence the wider network allowlist.
  Scope is the managed `[leak-scan]` issues only; native/handler leaks are out of scope.

environment: gh-aw-agents

on:
  schedule: every 12h
  workflow_dispatch:
    inputs:
      issue_number:
        description: "Scope to ONE [leak-scan] or [leak-test-gap] issue number (blank = auto-pick the oldest open one without a fix PR)."
        required: false
        type: string
      dry_run:
        description: "Preview only: do the full work locally but emit NO PR (print the would-be PR to the run log)."
        required: false
        type: boolean
        default: false
  push:
    branches: [test/leak-fixer-live-run]

# Runs on dotnet/maui only. The org's Copilot setup (via the gh-aw-agents environment)
# supplies the engine token — no per-repo secret needed.
if: |
  github.repository == 'dotnet/maui'

permissions:
  contents: read
  issues: read
  pull-requests: read

engine:
  id: copilot
  model: claude-opus-4.8

concurrency:
  group: "leak-fixer"
  cancel-in-progress: false

timeout-minutes: 120

tools:
  github:
    toolsets: [pull_requests, repos, issues, search]
    min-integrity: approved
  edit:
  bash: ["dotnet", "git", "find", "ls", "cat", "grep", "head", "tail", "wc", "jq", "tee", "sed", "awk", "tr", "cut", "sort", "uniq", "xargs", "echo", "date", "mkdir", "test", "env", "basename", "dirname", "bash", "sh", "chmod", "curl"]

checkout:
  fetch-depth: 200

network:
  allowed:
    # `dotnet` ecosystem bundles the dnceng Azure DevOps feeds (pkgs.dev.azure.com),
    # nuget.org, and the dotnet CDN — required to restore Microsoft.DotNet.Arcade.Sdk and
    # build Controls.Core.UnitTests from source (a plain `dev.azure.com` host entry does
    # not match the `pkgs.` subdomain the NuGet feed lives on).
    - defaults
    - github
    - dotnet
    - dev.azure.com
    - "*.blob.core.windows.net"

safe-outputs:
  create-pull-request:
    # No fixed title-prefix: the agent writes the FULL title starting with the mode tag —
    # "[leak-fix] " when it fixes a runtime leak (a [leak-scan] issue) or "[leak-test] "
    # when it adds missing coverage (a [leak-test-gap] issue). At most ONE PR per run.
    draft: true
    max: 1
    # This workflow transports the fix as a patch relative to ONE base branch.
    base-branch: "main"
    allowed-base-branches:
      - "main"
    allowed-branches:
      - "leak-fix/**"
      - "leak-test/**"
    # Enforced allowlist of paths a run may touch: a runtime-leak fix is a managed product
    # change + a Core.UnitTests regression test (+ PublicAPI); a coverage-gap fix adds the
    # proposed memory-leak test (Core.UnitTests unit test and/or DeviceTests InlineData).
    allowed-files:
      - "src/Controls/src/**"
      - "src/Controls/tests/Core.UnitTests/**"
      - "src/Controls/tests/DeviceTests/**"
      - "src/Core/src/**"
      - "src/Essentials/src/**"
      - "**/PublicAPI.Unshipped.txt"
    labels: [agentic-workflows]
    allowed-labels: [agentic-workflows]
---

# Memory Leak Fixer — dotnet/maui

You take ONE open issue filed by the Daily Memory Leak Hunter and turn it into a single
**draft pull request**. There are two kinds of issue and two matching tracks:

- **`[leak-scan]` — a confirmed runtime leak → Track A.** You produce a `[leak-fix]` PR that
  contains BOTH a regression test that FAILS without the fix and the managed product fix that
  makes it PASS.
- **`[leak-test-gap]` — a missing memory-leak test → Track B.** You produce a `[leak-test]` PR
  that ADDS the proposed coverage (the issue body contains a ready-to-paste stub) so a future
  regression in that control/scenario is caught.

The PR base is always `main`. You build MAUI **from source** to validate (unlike the hunter,
which uses the shipped package). All scratch state goes under `/tmp/gh-aw/agent/` (each bash
call is a fresh subshell; persist what you need). The only write you may perform is the single
`create-pull-request` safe-output. Never push directly, never comment, never edit anything
outside the fix/test.

## Hard rules — non-negotiable

1. **Empirical validation, appropriate to the track.**
   - **Track A (`[leak-scan]`):** you MUST observe the new regression test **FAIL on the
     unpatched source** and then **PASS after your fix**, in this run, before emitting a PR. A
     fix without a demonstrated red→green transition is not allowed.
   - **Track B (`[leak-test-gap]`):** the added test must **build and — when it is a runner-
     runnable `Controls.Core.UnitTests` test — PASS on current `main`** (it is a regression
     *guard*; the control is not expected to leak today). If adding the test unexpectedly makes
     it **FAIL**, you have found a real live leak: switch to Track A for that control instead
     (write the fix too). Device-only tests (`src/Controls/tests/DeviceTests/**`) cannot run on
     this runner — validate that they **compile** and rely on `maui-pr-devicetests` to execute
     them; say so plainly in the PR.
2. **If a Track A leak is already fixed on `main`, open NO PR.** When your faithful regression
   test passes on the *unpatched* source, the leak no longer reproduces — record
   `skipped: already fixed on main (test green without fix)` and stop.
3. **Managed scope for product fixes.** Any *product* change (Track A) must live in managed
   cross-platform code (`src/Controls/src`, `src/Core/src`, `src/Essentials/src`). If a
   `[leak-scan]` leak can only be reproduced with a platform handler / native peer, it is out of
   scope — `skipped: requires device test (out of scope)`. Track B may add a device-test
   `[InlineData]` (that is coverage, not a product change) even for a platform-tested control.
4. **Never mute, skip, weaken, or delete any existing test**, and never add `[ActiveIssue]`,
   `Skip=`, `#if false`, timeout bumps, or retries. Reject your own attempt if the only change
   that makes things green is a mute.
5. **Fix the root cause the issue describes** (Track A) — typically: replace a plain
   `event += instanceMethod` with a weak subscription (`WeakNotifyCollectionChangedProxy` /
   `WeakNotifyPropertyChangedProxy` / `WeakEventManager` — all already used in the codebase), or
   add the missing `-=` / teardown on the unload path. Prefer the minimal, idiomatic change that
   matches how the surrounding code already hardens similar subscriptions.
6. **One issue, one draft PR per run.** Pick exactly one target. Honour the de-dup and
   attempt-cap gates (Step 3) so you never stack a second open PR on the same issue.
7. **AI attribution.** The PR body must clearly state it was generated by this workflow.

## Step 0 — Run mode (dispatch inputs)

- `issue_number` (optional): if set, target exactly that issue (either `[leak-scan]` or
  `[leak-test-gap]`). If blank (e.g. the scheduled run), auto-pick a target in Step 2.
- `dry_run` (optional, default false): if `"true"`, do the full local work but **emit NO PR** —
  print the would-be PR (base, branch, title, body, `git diff --stat`) to the run log instead.

```bash
mkdir -p /tmp/gh-aw/agent
echo "issue_number=${{ github.event.inputs.issue_number }}"
echo "dry_run=${{ github.event.inputs.dry_run }}"
```

## Step 1 — Orient

```bash
dotnet --version
git rev-parse --abbrev-ref HEAD
git log --oneline -1
```

The base branch is `main`. The product source you fix is whatever is on `main` now — NOT the
shipped package the issue's repro referenced. Always re-locate the cited APIs in the live tree
(line numbers will have drifted; the code may already be partly hardened).

## Step 2 — Select the target issue (either type) and its track

If `issue_number` was provided, use it — determine its track from the title tag
(`[leak-scan]` → Track A; `[leak-test-gap]` → Track B). Otherwise auto-pick: list this
scanner's open issues of **both** types (oldest first) and take the first that does NOT already
have an open fix PR (Step 3 confirms). **Prefer a `[leak-scan]` over a `[leak-test-gap]`** when
both are equally old — fixing a live leak outranks adding a guard.

```bash
# Open [leak-scan] (Track A) issues, oldest first.
gh issue list --repo "$GITHUB_REPOSITORY" --search '"[leak-scan]" in:title' \
  --state open --label agentic-workflows --limit 100 \
  --json number,title,createdAt,body \
  | jq 'sort_by(.createdAt)' > /tmp/gh-aw/agent/leakscan-issues.json
# Open [leak-test-gap] (Track B) issues, oldest first.
gh issue list --repo "$GITHUB_REPOSITORY" --search '"[leak-test-gap]" in:title' \
  --state open --label agentic-workflows --limit 100 \
  --json number,title,createdAt,body \
  | jq 'sort_by(.createdAt)' > /tmp/gh-aw/agent/leaktestgap-issues.json
echo "--- [leak-scan] (Track A) ---";     jq -r '.[] | "\(.number)\t\(.title)"' /tmp/gh-aw/agent/leakscan-issues.json
echo "--- [leak-test-gap] (Track B) ---"; jq -r '.[] | "\(.number)\t\(.title)"' /tmp/gh-aw/agent/leaktestgap-issues.json
```

(The Daily Memory Leak Hunter files both types with the `agentic-workflows` label.)

Read the chosen issue's body in full (`gh issue view <N> --json title,body`). Extract, for a
**Track A `[leak-scan]`**:

- the **rooting API** (e.g. `IndicatorView.ItemsSource`),
- the **retention path** `root -> … -> transient` with the cited file:line(s),
- the **suggested fix** shape, and
- any **non-default / disabling condition**.

…or, for a **Track B `[leak-test-gap]`**:

- the **control / scenario** that lacks coverage,
- the **proposed test** stub the issue provides (usually an `[InlineData(typeof(X))]` line on
  the `HandlerDoesNotLeak` theory in `src/Controls/tests/DeviceTests/Memory/MemoryTests.cs`,
  possibly plus a `handlers.AddHandler<…>()` registration), and
- which test file(s) it names.

## Step 3 — De-dup + attempt cap (live GitHub searches)

A fix PR carries `Fixes #<N>` (and `Refs: <owner>/<repo>#<N>`) in its body — that is the join
key, regardless of whether it is a `[leak-fix]` or a `[leak-test]` PR.

```bash
N=<issue-number>
# Open fix PR (either tag) already addressing this issue?
gh pr list --repo "$GITHUB_REPOSITORY" --state open --search 'in:title "[leak-fix]" OR "[leak-test]"' \
  --json number,title,body \
  | jq --arg n "$N" '[.[] | select((.body // "") | test("(Fixes|Refs)[^0-9]*#"+$n+"\\b"))]' \
  > /tmp/gh-aw/agent/open-fix-prs.json
jq 'length' /tmp/gh-aw/agent/open-fix-prs.json
# Closed-unmerged attempts for this issue (attempt cap = 3).
gh pr list --repo "$GITHUB_REPOSITORY" --state closed --search 'in:title "[leak-fix]" OR "[leak-test]"' \
  --json number,title,body,mergedAt \
  | jq --arg n "$N" '[.[] | select(((.body // "") | test("(Fixes|Refs)[^0-9]*#"+$n+"\\b")) and (.mergedAt == null))]' \
  > /tmp/gh-aw/agent/closed-fix-prs.json
jq 'length' /tmp/gh-aw/agent/closed-fix-prs.json
```

- If an **open** fix PR already refs this issue → `skipped: open fix PR exists` and stop (or, if
  `issue_number` was explicit, just stop).
- If **3+ closed-unmerged** attempts exist → `skipped: attempt cap reached (3)` and stop.
- An issue that is already CLOSED → `skipped: issue closed` (nothing to do).

If you auto-selected in Step 2 and the first candidate is gated out, move to the next oldest
(across both types, `[leak-scan]` first).

**Now branch by track:**
- Track A (`[leak-scan]`) → Steps 4–10.
- Track B (`[leak-test-gap]`) → **skip to Step 11**.

## Step 4 — Create the fix branch (Track A)

```bash
N=<issue-number>
git checkout -B "leak-fix/issue-${N}" "origin/main"
git rev-parse --abbrev-ref HEAD | tee /tmp/gh-aw/agent/branch.txt
test "$(cat /tmp/gh-aw/agent/branch.txt)" = "leak-fix/issue-${N}"
```

## Step 5 — Write the regression test (red)

Add ONE focused test to `Controls.Core.UnitTests` that reproduces the issue's retention path
using only managed APIs. Model it on the existing leak tests — they use a `WeakReference` plus
`await reference.WaitForCollect()` (see `src/Controls/tests/Core.UnitTests/WeakEventProxyTests.cs`,
`BindingUnitTests.cs`, `BindableLayoutTests.cs`, `Layouts/GridLayoutTests.cs`).

Shape (adapt to the issue):

```csharp
[Fact]
public async Task <Control><RootingApi>DoesNotLeak()
{
    // A long-lived/shared root, exactly as the issue describes (e.g. a ViewModel collection).
    var sharedRoot = new ObservableCollection<string> { "a", "b", "c" };

    WeakReference weakSubject;
    {
        var subject = new <Control>();
        subject.<RootingApi> = sharedRoot;     // installs the non-weak subscription
        weakSubject = new WeakReference(subject);
        // drop the only strong ref to `subject`; `sharedRoot` stays alive.
    }

    Assert.False(await weakSubject.WaitForCollect(), "<Control> should not be alive!");
    GC.KeepAlive(sharedRoot);
}
```

Place it in the most fitting existing test class (or a new `*MemoryTests.cs` file under
`src/Controls/tests/Core.UnitTests/`) and keep the namespace/usings consistent with its
neighbours. The assertion must be a genuine leak check — `WaitForCollect()` returns `true`
if the object is still alive after forced GC (i.e. the test FAILS while the leak exists).

## Step 6 — Build + run ONLY your test on the net TFM — confirm it FAILS (red)

Build and run **only your new test** on the .NET library TFM. Restrict to the `net` TFM with
the five platform-exclusion flags (no Android/iOS/etc. workload needed) and disable
`TreatWarningsAsErrors` (some test projects have pre-existing warning-as-error noise). The
project-reference graph **bootstraps the MAUI build tasks and builds `Core` → `Controls.Core`
→ the test project automatically** — you do NOT need to build `Microsoft.Maui.BuildTasks.slnf`
first:

```bash
FLAGS="-p:IncludeIosTargetFrameworks=false -p:IncludeAndroidTargetFrameworks=false -p:IncludeMacCatalystTargetFrameworks=false -p:IncludeWindowsTargetFrameworks=false -p:IncludeTizenTargetFrameworks=false -p:TreatWarningsAsErrors=false"

# First restore+build can take ~10-20 min (cold NuGet cache pulls Arcade SDK from the
# dnceng feed at pkgs.dev.azure.com — now allowlisted). Run ONLY the new test.
dotnet test src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj -c Debug $FLAGS \
  --filter "FullyQualifiedName~<YourTestName>" \
  --logger "console;verbosity=normal" 2>&1 | tee /tmp/gh-aw/agent/red.log | tail -50
```

Interpret:

- **Test FAILS** (the `WaitForCollect` assert trips: object still alive) → good, the test
  catches the leak. Proceed to Step 7.
- **Test PASSES on unpatched source** → the leak is already fixed on `main`. Per Hard Rule 2,
  open NO PR: record `skipped: already fixed on main (test green without fix)` and stop.
- **Restore 403 / feed unreachable** → should not happen now (`pkgs.dev.azure.com`,
  `*.blob.core.windows.net`, `*.nuget.org` are allowlisted). If it still does, record
  `skipped: build env cannot restore (feed blocked)` and stop — do NOT emit a PR.
- **Compile error from your test** (wrong API/usings) → fix the TEST and re-run. If the API
  you referenced is genuinely platform-only (not on the net TFM), the leak needs a device
  test: `skipped: requires device test (out of scope)`.

(Optional fallback only if the bootstrap genuinely fails: build the net-only build tasks once
with `dotnet build Microsoft.Maui.BuildTasks.slnf -c Debug $FLAGS` and re-run the test. Do not
spend many attempts here — the direct `dotnet test` above is the verified path.)

## Step 7 — Implement the product fix

Make the minimal, idiomatic managed change that breaks the strong-retention edge the issue
identifies — e.g. swap the plain `collection.CollectionChanged += OnCollectionChanged;` for a
`WeakNotifyCollectionChangedProxy` (the same hardening already used elsewhere in
`src/Controls/src/Core`), or add the missing unsubscribe/teardown on the unload path. Touch
only the cited product file(s). Do NOT change unrelated behaviour and do NOT alter the test.

If you add or change any public API, update the matching `PublicAPI.Unshipped.txt` (per-TFM).
A weak-proxy refactor usually changes no public surface.

## Step 8 — Rebuild + run the test — confirm it PASSES (green), no regressions

```bash
FLAGS="-p:IncludeIosTargetFrameworks=false -p:IncludeAndroidTargetFrameworks=false -p:IncludeMacCatalystTargetFrameworks=false -p:IncludeWindowsTargetFrameworks=false -p:IncludeTizenTargetFrameworks=false -p:TreatWarningsAsErrors=false"
# Your test must now pass.
dotnet test src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj -c Debug $FLAGS \
  --filter "FullyQualifiedName~<YourTestName>" \
  --logger "console;verbosity=normal" 2>&1 | tee /tmp/gh-aw/agent/green.log | tail -50
# Neighbours of the changed area must still pass (run the enclosing class).
dotnet test src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj -c Debug $FLAGS \
  --filter "FullyQualifiedName~<EnclosingTestClass>" \
  --logger "console;verbosity=normal" 2>&1 | tail -30
```

Require: `red.log` shows the test failing, `green.log` shows it passing, and the class run is
clean. If the fix does not turn the test green, or it breaks a neighbour, iterate on the fix
(not the test). If you cannot get a clean red→green without weakening anything, drop the
attempt: `skipped: could not validate a non-muting fix`.

## Step 9 — Format, then commit (injection-safe)

```bash
dotnet format src/Controls/src/Controls.Core.csproj --no-restore 2>&1 | tail -5 || true
git add -A
git --no-pager diff --stat "origin/main..HEAD" || true
git --no-pager diff --staged --stat
```

Commit with a heredoc + `git commit -F` so no issue-derived text is ever interpolated into a
shell command. Replace BOTH occurrences of the delimiter below with ONE fresh per-run random
token you generate now (≥16 hex chars, e.g. `GHAW_MSG_<16-random-hex>`); never emit the literal
placeholder, and keep the body to single lines:

```bash
cat > /tmp/gh-aw/agent/commitmsg.txt <<'GHAW_MSG_RANDOM_DELIMITER'
[leak-fix] Fix <rooting API> memory leak (Fixes #<N>)

Replace the non-weak subscription with a weak proxy / add teardown so the
transient is no longer rooted by the shared/long-lived value. Adds a
Controls.Core.UnitTests regression test that fails without this fix.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
GHAW_MSG_RANDOM_DELIMITER
git commit -F /tmp/gh-aw/agent/commitmsg.txt
git rev-list --count "origin/main..HEAD" | tee /tmp/gh-aw/agent/commitcount.txt
test "$(cat /tmp/gh-aw/agent/commitcount.txt)" -ge 1
```

If nothing was committed (count `0`) → `skipped: no commit produced` and stop.

## Step 10 — Emit the draft `[leak-fix]` PR (Track A)

> **Dry-run gate:** if `dry_run == "true"`, do NOT emit. Print `DRY RUN — would open PR`
> with base `main`, source branch `leak-fix/issue-<N>`, the title, the full body, and
> `git --no-pager diff --stat "origin/main..HEAD"`, then stop.

Emit exactly one `create-pull-request` safe-output. Do NOT set a `base` field (the
`base-branch: main` config pins it). Source `branch` MUST be `leak-fix/issue-<N>`. Title MUST
start with the literal tag **`[leak-fix] `** followed by e.g. `Fix <rooting API> memory leak
(Fixes #<N>)`.

The body MUST start with the required MAUI testing note (verbatim, no block-quote on the first
two lines as shown), then the details:

```markdown
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

> [!NOTE]
> 🤖 **AI-generated PR** — produced automatically by the **Memory Leak Fixer** agentic workflow from issue #<N>. The regression test below was observed to FAIL on unpatched `main` and PASS with this fix, on the runner. Please review carefully before merging.

Fixes #<N>
Refs: <owner>/<repo>#<N>
Target branch: main
Attempt: <K>/3

## The leak
<one-paragraph restatement of the rooting API + retention path, with current file:line.>

## The fix
<what you changed and why it breaks the retention edge — e.g. weak proxy / teardown.>

## Regression test
`<test class>.<TestName>` in `src/Controls/tests/Core.UnitTests/...`. It assigns a
long-lived collection/value to the control, drops the control, forces GC, and asserts the
control is collected.

| State | Result |
|---|---|
| Without fix (unpatched `main`) | ❌ test FAILS (control retained) |
| With fix | ✅ test PASSES (control collected) |

<paste the key red.log assertion line and the green.log pass line.>

## Scope
Managed cross-platform change → all platforms. No public API change (or: list the
PublicAPI.Unshipped.txt entries added).
```

Before emitting, re-read your body and confirm the `Target branch:` line says `main` and a
`Fixes #<N>` line is present. If not, drop the attempt: `skipped: PR self-check failed`.

If no PR was emitted this run (already-fixed, gated out, or validation failed), produce no
output — that is an acceptable outcome. Otherwise you have produced exactly one validated,
draft `[leak-fix]` PR.

# ===================== TRACK B — ADD MISSING COVERAGE (`[leak-test-gap]`) =====================

Reached from Step 3 when the selected issue is a `[leak-test-gap]`. The goal: ADD the memory-
leak test the issue proposes, so a future regression is caught. You are adding a **test only**
— no product change.

## Step 11 — Create the test branch (Track B)

```bash
N=<issue-number>
git checkout -B "leak-test/issue-${N}" "origin/main"
git rev-parse --abbrev-ref HEAD | tee /tmp/gh-aw/agent/branch.txt
test "$(cat /tmp/gh-aw/agent/branch.txt)" = "leak-test/issue-${N}"
```

## Step 12 — Add the proposed test

The `[leak-test-gap]` issue body contains a ready-to-paste stub. Apply it faithfully, choosing
the right home:

- **Control-coverage gap (the usual case)** — the issue proposes an `[InlineData(typeof(X))]`
  on the `HandlerDoesNotLeak` theory in `src/Controls/tests/DeviceTests/Memory/MemoryTests.cs`.
  Add exactly that line (keep the list ordering/style), AND add the matching
  `handlers.AddHandler<X, XHandler>();` in `SetupBuilder` **iff** it is not already registered
  (many controls resolve through an existing base registration such as
  `AddHandler<Layout, LayoutHandler>()` — do not add a duplicate). Verify the type is public,
  concrete, and parameterless-constructible.
- **Scenario gap** — the issue proposes a small `[Fact]`/`[Theory]`. Add it to the test file
  the issue names, matching the surrounding pattern.
- **Runner-runnable variant (preferred when it exists).** If the control's retention can be
  expressed as a purely-managed `Controls.Core.UnitTests` test (`WeakReference` +
  `WaitForCollect()`), add that too/instead — it is validated on this runner (Step 13) and is a
  strong regression guard. Model it on `WeakEventProxyTests.cs` / `BindableLayoutTests.cs`.

Never weaken/mute anything; never touch product code in Track B.

## Step 13 — Validate the added test (as far as the runner allows)

```bash
FLAGS="-p:IncludeIosTargetFrameworks=false -p:IncludeAndroidTargetFrameworks=false -p:IncludeMacCatalystTargetFrameworks=false -p:IncludeWindowsTargetFrameworks=false -p:IncludeTizenTargetFrameworks=false -p:TreatWarningsAsErrors=false"
```

- **If you added a `Controls.Core.UnitTests` test:** build + run it.
  ```bash
  dotnet test src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj -c Debug $FLAGS \
    --filter "FullyQualifiedName~<YourTestName>" --logger "console;verbosity=normal" \
    2>&1 | tee /tmp/gh-aw/agent/guard.log | tail -40
  ```
  It MUST build and **PASS on current `main`** — the control is a regression *guard*, not
  expected to leak today. **If it FAILS**, you have found a real live leak: abandon Track B and
  restart as **Track A** for this control (write the product fix + red→green, Steps 4–10), then
  emit a `[leak-fix]` PR instead.
- **If the proposed test is a `DeviceTests` `[InlineData]` (device-only):** it cannot run on
  this runner (needs a platform workload/emulator). Do NOT attempt a full device build. Instead
  validate **structurally**: the `[InlineData(typeof(X))]` line is added to the correct theory,
  `X` exists and is concrete/parameterless, and a handler registration is present. Note in the
  PR that the test executes on `maui-pr-devicetests`.

## Step 14 — Format, commit (injection-safe), and emit the draft `[leak-test]` PR

Format (`dotnet format …/Controls.Core.csproj --no-restore || true` when a `.cs` was touched),
then commit with the same heredoc + fresh-random-delimiter discipline as Step 9 (subject
`[leak-test] Add memory-leak coverage for <control> (Fixes #<N>)`), and confirm ≥1 commit on
`origin/main..HEAD` (else `skipped: no commit produced`).

> **Dry-run gate:** if `dry_run == "true"`, print `DRY RUN — would open PR` (base `main`,
> branch `leak-test/issue-<N>`, title, body, `git --no-pager diff --stat "origin/main..HEAD"`)
> and stop.

Emit exactly one `create-pull-request` safe-output. Do NOT set a `base` field. Source `branch`
MUST be `leak-test/issue-<N>`. Title MUST start with the literal tag **`[leak-test] `**. Body:

```markdown
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

> [!NOTE]
> 🤖 **AI-generated PR** — produced automatically by the **Memory Leak Fixer** agentic workflow from issue #<N> (coverage-gap track). It ADDS a memory-leak test; no product code changes. Please review before merging.

Fixes #<N>
Refs: <owner>/<repo>#<N>
Target branch: main

## The gap
<which control/scenario had no memory-leak test, per the issue.>

## The added test
<name + file. State whether it is a runner-run Controls.Core.UnitTests guard or a
DeviceTests HandlerDoesNotLeak InlineData (runs on maui-pr-devicetests).>

## Validation
| Test | Where it runs | Result |
|---|---|---|
| <name> | <Core.UnitTests on the runner / maui-pr-devicetests> | <✅ PASSES on main (guard) / compiles; runs on CI> |

<paste the guard.log pass line if you ran it.>

## Scope
Test-only change (no product code touched).
```

Confirm the `Target branch:` line says `main` and `Fixes #<N>` is present, else
`skipped: PR self-check failed`. If nothing was emitted, produce no output.
