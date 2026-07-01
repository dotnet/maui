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
  workflow_dispatch:
    inputs:
      issue_number:
        description: "Scope to ONE [leak-scan] issue number (blank = pick the oldest open one without a [leak-fix] PR)."
        required: false
        type: string
      dry_run:
        description: "Preview only: do the full test+fix locally but emit NO PR (print the would-be PR to the run log)."
        required: false
        type: boolean
        default: false

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
    title-prefix: "[leak-fix] "
    draft: true
    max: 1
    # This workflow transports the fix as a patch relative to ONE base branch.
    base-branch: "main"
    allowed-base-branches:
      - "main"
    allowed-branches:
      - "leak-fix/**"
    # Enforced allowlist of paths the fix may touch. A leak fix is a product change in
    # the managed core plus the new regression test (+ PublicAPI if the surface changes).
    allowed-files:
      - "src/Controls/src/**"
      - "src/Controls/tests/Core.UnitTests/**"
      - "src/Core/src/**"
      - "src/Essentials/src/**"
      - "**/PublicAPI.Unshipped.txt"
    labels: [agentic-workflows]
    allowed-labels: [agentic-workflows]
---

# Memory Leak Fixer — dotnet/maui

You take ONE open `[leak-scan]` issue (filed by the Daily Memory Leak Hunter) and produce a
single **draft `[leak-fix]` pull request** that contains BOTH a regression test that fails
without the fix and the product fix that makes it pass. The PR base is always `main`.

You build MAUI **from source** to validate the fix (unlike the hunter, which uses the shipped
package). All scratch state goes under `/tmp/gh-aw/agent/` (each bash call is a fresh
subshell; persist what you need). The only write you may perform is the single
`create-pull-request` safe-output. Never push directly, never comment, never edit anything
outside the fix + its test.

## Hard rules — non-negotiable

1. **Empirical TDD, both directions.** You MUST observe the new test **FAIL on the unpatched
   source** and then **PASS after your fix**, in this run, before emitting a PR. A fix without
   a demonstrated red→green transition is not allowed.
2. **If the leak is already fixed on `main`, open NO PR.** When your faithful regression test
   passes on the *unpatched* source, the leak no longer reproduces on this branch — record
   `skipped: already fixed on main (test green without fix)` and stop. This is a correct,
   expected outcome (the leaks were found in the shipped 10.0.0 package; some are already
   fixed in `main`).
3. **Managed scope only.** The fix must live in managed cross-platform code (`src/Controls/src`,
   `src/Core/src`, `src/Essentials/src`) and the test in `Controls.Core.UnitTests` (library
   TFM, no workload/emulator). If the issue's leak can only be reproduced with a platform
   handler / native peer, it is out of scope — `skipped: requires device test (out of scope)`.
4. **Never mute, skip, weaken, or delete any existing test**, and never add `[ActiveIssue]`,
   `Skip=`, `#if false`, timeout bumps, or retries. The diff must be a *real* product fix plus
   a *new* test. Reject your own attempt if the only change that makes things green is a mute.
5. **Fix the root cause the issue describes** — typically: replace a plain
   `event += instanceMethod` with a weak subscription (`WeakNotifyCollectionChangedProxy` /
   `WeakNotifyPropertyChangedProxy` / `WeakEventManager` — all already used in the codebase),
   or add the missing `-=` / teardown on the unload path. Prefer the minimal, idiomatic change
   that matches how the surrounding code already hardens similar subscriptions.
6. **One issue, one draft PR per run.** Pick exactly one target. Honour the de-dup and
   attempt-cap gates (Step 3) so you never stack a second open PR on the same issue.
7. **AI attribution.** The PR body must clearly state it was generated by this workflow.

## Step 0 — Run mode (dispatch inputs)

- `issue_number` (optional): if set, target exactly that `[leak-scan]` issue. If blank, pick a
  target in Step 2.
- `dry_run` (optional, default false): if `"true"`, do the full local test+fix+validate but
  **emit NO PR** — print the would-be PR (base, branch, title, body, `git diff --stat`) to the
  run log instead.

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
shipped 10.0.0 the issue's repro referenced. Always re-locate the cited APIs in the live tree
(line numbers will have drifted; the code may already be partly hardened).

## Step 2 — Select the target `[leak-scan]` issue

If `issue_number` was provided, use it. Otherwise list this scanner's open `[leak-scan]`
issues (oldest first) and pick the first that does NOT already have an open `[leak-fix]` PR
(Step 3 confirms):

```bash
gh issue list --repo "$GITHUB_REPOSITORY" --search '"[leak-scan]" in:title' \
  --state open --label memory-leak --limit 100 \
  --json number,title,createdAt,body \
  | jq 'sort_by(.createdAt)' > /tmp/gh-aw/agent/leakscan-issues.json
jq -r '.[] | "\(.number)\t\(.title)"' /tmp/gh-aw/agent/leakscan-issues.json
```

Read the chosen issue's body in full (`gh issue view <N> --json title,body`). Extract:

- the **rooting API** (e.g. `IndicatorView.ItemsSource`),
- the **retention path** `root -> … -> transient` with the cited file:line(s),
- the **suggested fix** shape, and
- any **non-default / disabling condition**.

## Step 3 — De-dup + attempt cap (live GitHub searches)

A `[leak-fix]` PR carries `Fixes #<N>` (and `Refs: <owner>/<repo>#<N>`) in its body — that is
the join key.

```bash
N=<issue-number>
# Open [leak-fix] PR already addressing this issue?
gh pr list --repo "$GITHUB_REPOSITORY" --state open --search '"[leak-fix]" in:title' \
  --json number,title,body \
  | jq --arg n "$N" '[.[] | select((.body // "") | test("(Fixes|Refs)[^0-9]*#"+$n+"\\b"))]' \
  > /tmp/gh-aw/agent/open-fix-prs.json
jq 'length' /tmp/gh-aw/agent/open-fix-prs.json
# Closed-unmerged [leak-fix] attempts for this issue (attempt cap = 3).
gh pr list --repo "$GITHUB_REPOSITORY" --state closed --search '"[leak-fix]" in:title' \
  --json number,title,body,mergedAt \
  | jq --arg n "$N" '[.[] | select(((.body // "") | test("(Fixes|Refs)[^0-9]*#"+$n+"\\b")) and (.mergedAt == null))]' \
  > /tmp/gh-aw/agent/closed-fix-prs.json
jq 'length' /tmp/gh-aw/agent/closed-fix-prs.json
```

- If an **open** `[leak-fix]` PR already refs this issue → `skipped: open [leak-fix] PR exists`
  and stop (or, if `issue_number` was explicit, pick nothing else — just stop).
- If **3+ closed-unmerged** attempts exist → `skipped: attempt cap reached (3)` and stop.
- A `[leak-scan]` issue that is already CLOSED → `skipped: issue closed` (nothing to fix).

If you auto-selected in Step 2 and the first candidate is gated out, move to the next oldest.

## Step 4 — Create the fix branch

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

## Step 10 — Emit the draft `[leak-fix]` PR

> **Dry-run gate:** if `dry_run == "true"`, do NOT emit. Print `DRY RUN — would open PR`
> with base `main`, source branch `leak-fix/issue-<N>`, the title, the full body, and
> `git --no-pager diff --stat "origin/main..HEAD"`, then stop.

Emit exactly one `create-pull-request` safe-output. Do NOT set a `base` field (the
`base-branch: main` config pins it). Source `branch` MUST be `leak-fix/issue-<N>`. Title:
`Fix <rooting API> memory leak (Fixes #<N>)` (the `[leak-fix] ` prefix is added automatically).

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
