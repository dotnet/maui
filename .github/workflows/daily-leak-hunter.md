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
  bash: ["dotnet", "git", "gh", "find", "ls", "cat", "grep", "head", "tail", "wc", "jq", "tee", "sed", "awk", "tr", "cut", "sort", "uniq", "xargs", "echo", "date", "mkdir", "test", "env", "basename", "dirname", "bash", "sh", "chmod", "curl", "pwsh"]

checkout:
  fetch-depth: 50

# gh-aw computes the built-in safe_outputs permissions from mutation handlers, but the trusted
# final gate also performs read-only PR metadata queries. Add only that missing read scope to
# the generated job; the compiler merges it with contents:read + issues:write.
jobs:
  safe_outputs:
    permissions:
      pull-requests: read

# Deterministic pre-pass (runs BEFORE the agent/MCP gateway starts, same job/runner, so its
# /tmp/gh-aw writes are visible to the agent's later bash calls — /tmp/gh-aw is bind-mounted
# read-write into the agent's sandbox container). This is a GENUINE job-enforced gate: `set -e`
# means a `gh` failure here fails this GH Actions step (and therefore the whole job) BEFORE the
# agent ever starts — unlike the equivalent fetch previously run as an in-prompt bash tool call,
# where a nonzero exit is only reported to the agent as a tool error and does not by itself stop
# the agent from continuing and still emitting a `create-issue` safe-output.
pre-agent-steps:
  - name: Fetch leak de-dup context (fail-closed)
    shell: bash
    env:
      GH_TOKEN: ${{ github.token }}
      GITHUB_REPOSITORY: ${{ github.repository }}
    run: |
      set -euo pipefail
      mkdir -p /tmp/gh-aw/agent

      # This workflow's own open [leak-scan] issues (filed with the agentic-workflows label).
      # The shared GraphQL helper follows every cursor and verifies stable totalCount/pageInfo.
      # Count, duplicate, or cursor churn rebuilds the whole issue snapshot once from scratch;
      # persistent churn and malformed, incomplete, or over-budget pagination fail closed.
      pwsh .github/scripts/Get-CompleteLeakIssues.ps1 \
        -Repository "$GITHUB_REPOSITORY" \
        -OutputPath /tmp/gh-aw/agent/my-open-leakscan.json
      jq -r '.[].title | gsub("[\r\n]+";" ")' /tmp/gh-aw/agent/my-open-leakscan.json \
        | while IFS= read -r TITLE; do
            pwsh .github/scripts/Get-CanonicalLeakApi.ps1 -Title "$TITLE" -ExistingTitle
          done \
        | sort -u \
        > /tmp/gh-aw/agent/already-filed-apis.txt
      echo "already-filed rooting APIs:"
      cat /tmp/gh-aw/agent/already-filed-apis.txt

      # Exact [leak-fix] PRs already MERGED to main/inflight/current.
      # Fetch complete non-Search history once per authoritative base. Pagination is bounded by
      # an explicit query budget rather than a result cutoff; reaching the budget is a visible
      # fail-closed error, never a silently truncated history.
      pwsh .github/scripts/Get-CompleteLeakPullRequests.ps1 \
        -Repository "$GITHUB_REPOSITORY" \
        -State MERGED \
        -OutputPath /tmp/gh-aw/agent/merged-leak-fix-prs-raw.json
      jq '[.[] |
          select(.mergedAt != null) |
          select(.title | test("^\\[leak-fix\\][ \\t]+")) |
          select(.baseRefName == "main" or .baseRefName == "inflight/current")]' \
        /tmp/gh-aw/agent/merged-leak-fix-prs-raw.json \
        > /tmp/gh-aw/agent/merged-leak-fix-prs.json

      jq -r '.[] | [.number, .title, .baseRefName, .url] | @tsv' \
        /tmp/gh-aw/agent/merged-leak-fix-prs.json \
        | while IFS=$'\t' read -r PR TITLE BASE URL; do
            API=$(pwsh .github/scripts/Get-CanonicalLeakApi.ps1 -Title "$TITLE" -ExistingTitle)
            if test -n "$API"; then
              printf '%s\t%s\t%s\t%s\t%s\n' "$API" "$PR" "$BASE" "$URL" "$TITLE"
            fi
          done \
        | sort -u \
        > /tmp/gh-aw/agent/already-merged-fix-apis.tsv
      cut -f1 /tmp/gh-aw/agent/already-merged-fix-apis.tsv | sort -u \
        > /tmp/gh-aw/agent/already-merged-fix-apis.txt
      echo "already-merged fix APIs:"
      cat /tmp/gh-aw/agent/already-merged-fix-apis.tsv

      # A merged [leak-fix] PR is not permanent proof the fix is still active — it may since
      # have been reverted (e.g. it broke something else), in which case the shipped package
      # will still reproduce the ORIGINAL leak and skipping the API forever would be wrong.
      # Repository-local "Reverts #<N>" body references and revert-shaped titles narrow the
      # candidate set but are editable and never establish revert state themselves. This keeps
      # ordinary unrelated merged-PR growth outside the traversal budget. Resolve only
      # independently verified links recursively: any active same-branch direct reverter keeps
      # its target reverted. Reverting a reverter can deactivate it, but
      # independent sibling reverts never cancel each other.
      # Reuse the same complete merged-PR history, including each original merge/squash commit
      # OID. Those editable hints identify candidates only. The helper batches
      # candidate PR commit-history queries and accepts an edge only when the union of the
      # merge/squash message and complete branch commit messages contains one unambiguous full
      # `This reverts commit <40-hex-target-OID>.` proof. Wrong, abbreviated, duplicate,
      # ambiguous, cross-branch, truncated, or over-budget evidence leaves the original fix
      # active or fails closed. Both merged-history and commit pagination use stable
      # totalCount/pageInfo validation and rebuild the whole snapshot once on count, duplicate,
      # cursor, or cross-base retarget churn. Persistent churn fails closed.
      # Native requests use bounded transient retries, capped server-directed rate-limit delays,
      # and 1000-query safety budgets per snapshot attempt; commit verification is batched 10 PRs
      # at a time, pages 100 commits, and caps aggregate records at 20000. Local traversal keeps
      # the 1000-discovery and 2000-PR aggregate bounds.
      pwsh .github/scripts/Get-RelevantMergedLeakReverts.ps1 \
        -Repository "$GITHUB_REPOSITORY" \
        -MergedFixTsvPath /tmp/gh-aw/agent/already-merged-fix-apis.tsv \
        -MergedPullRequestsJsonPath /tmp/gh-aw/agent/merged-leak-fix-prs-raw.json \
        -OutputPath /tmp/gh-aw/agent/merged-revert-prs.json
      # Resolve the EFFECTIVE state recursively, not just one hop. An immutably verified merged
      # revert edge toggles its target only while that revert itself remains active on the SAME
      # base branch; every edge in a revert-of-revert chain must carry its own exact proof.
      # A revert is active only when none of its own same-branch direct reverters is active;
      # any active direct reverter keeps its target reverted. Servicing-branch reverts cannot
      # alter main/inflight.
      pwsh .github/scripts/Get-EffectiveRevertedLeakFixes.ps1 \
        -Repository "$GITHUB_REPOSITORY" \
        -MergedFixTsvPath /tmp/gh-aw/agent/already-merged-fix-apis.tsv \
        -MergedRevertsJsonPath /tmp/gh-aw/agent/merged-revert-prs.json \
        -OutputPath /tmp/gh-aw/agent/reverted-fix-pr-numbers.txt
      if test -s /tmp/gh-aw/agent/reverted-fix-pr-numbers.txt; then
        echo "excluding effectively-reverted merged-fix PRs from the permanent-proof set:"
        cat /tmp/gh-aw/agent/reverted-fix-pr-numbers.txt
        awk -F '\t' 'NR==FNR{rev[$1]=1; next} !($2 in rev)' \
          /tmp/gh-aw/agent/reverted-fix-pr-numbers.txt /tmp/gh-aw/agent/already-merged-fix-apis.tsv \
          > /tmp/gh-aw/agent/already-merged-fix-apis.filtered.tsv
        mv /tmp/gh-aw/agent/already-merged-fix-apis.filtered.tsv /tmp/gh-aw/agent/already-merged-fix-apis.tsv
        cut -f1 /tmp/gh-aw/agent/already-merged-fix-apis.tsv | sort -u \
          > /tmp/gh-aw/agent/already-merged-fix-apis.txt
      fi

      # Authoritative open [leak-fix] PRs remain duplicate work even when their original
      # scanner issue was closed independently. A valid anchored title identity is sufficient
      # for conservative API de-dup; malformed or missing editable body provenance must not
      # erase it. Strict Fixes/Refs provenance remains required when creating a new fix PR.
      pwsh .github/scripts/Get-CompleteLeakPullRequests.ps1 \
        -Repository "$GITHUB_REPOSITORY" \
        -State OPEN \
        -OutputPath /tmp/gh-aw/agent/open-leak-fix-prs-raw.json
      # shellcheck disable=SC2016
      pwsh -NoLogo -NoProfile -Command '
        $ErrorActionPreference = "Stop"
        Import-Module .github/scripts/LeakWorkflowDedup.psm1 -Force
        $open = @(Read-RegularJsonFile `
            -Path "/tmp/gh-aw/agent/open-leak-fix-prs-raw.json" `
            -MaximumBytes 128MB)
        $authoritative = @(Select-LeakAuthoritativePullRequests `
            -PullRequests $open `
            -Context "Hunter pre-agent open leak-fix de-dup search")
        $identified = @(
          foreach ($pullRequest in $authoritative) {
            $number = [int]$pullRequest.number
            $identity = Get-LeakFixPullRequestTitleIdentity `
              -PullRequest $pullRequest `
              -Context "Open pull request #$number"
            if ($null -ne $identity) {
              [pscustomobject]@{
                api = [string]$identity.Api
                number = $number
                baseRefName = [string]$pullRequest.baseRefName
                url = [string]$pullRequest.url
                title = [string]$pullRequest.title
              }
            }
          }
        )
        ConvertTo-Json -InputObject @($identified) -Depth 5
      ' > /tmp/gh-aw/agent/already-open-fix-prs.json
      jq -r '.[] | [.api, .number, .baseRefName, .url, .title] | @tsv' \
        /tmp/gh-aw/agent/already-open-fix-prs.json \
        | sort -u \
        > /tmp/gh-aw/agent/already-open-fix-apis.tsv
      cut -f1 /tmp/gh-aw/agent/already-open-fix-apis.tsv | sort -u \
        > /tmp/gh-aw/agent/already-open-fix-apis.txt
      echo "already-open title-identified fix APIs:"
      cat /tmp/gh-aw/agent/already-open-fix-apis.tsv

network:
  allowed:
    - defaults
    - github
    - dotnet
    - "*.blob.core.windows.net"

safe-outputs:
  # The pre-agent snapshot keeps the agent from wasting work on known leaks, but a fix can
  # merge or open during the up-to-90-minute hunt. Re-fetch authoritative live metadata in
  # the generated safe-output job immediately before Process Safe Outputs. The gate brackets
  # effective-revert evaluation with matching bounded all-state snapshots, removes each late
  # same-API issue, merged fix, or title-identified open fix, and atomically rewrites the agent
  # output so otherwise-distinct items proceed. It blocks without mutation only when every
  # create-issue item became stale. It never accepts agent-authored mechanism overrides.
  # Restore it from the read-only default branch rather than executing
  # workflow-dispatch-selected code with the write-capable job token.
  steps:
    - name: Checkout trusted leak-hunter de-dup gate
      if: ${{ contains(needs.agent.outputs.output_types, 'create_issue') }}
      uses: actions/checkout@v7.0.1
      with:
        ref: ${{ github.event.repository.default_branch }}
        path: trusted-leak-hunter
        sparse-checkout: .github/scripts
        persist-credentials: false
    - name: Protect trusted leak-hunter de-dup gate
      if: ${{ contains(needs.agent.outputs.output_types, 'create_issue') }}
      shell: bash
      run: |
        set -euo pipefail
        TRUSTED_DIR="$GITHUB_WORKSPACE/trusted-leak-hunter/.github/scripts"
        test -s "$TRUSTED_DIR/Assert-LeakHunterSafeOutputGate.ps1"
        test -s "$TRUSTED_DIR/LeakWorkflowDedup.psm1"
        chmod -R a-w "$TRUSTED_DIR"
    - name: Enforce final leak-hunter de-dup gate
      if: ${{ contains(needs.agent.outputs.output_types, 'create_issue') }}
      shell: pwsh
      env:
        GH_TOKEN: ${{ github.token }}
        GH_AW_AGENT_OUTPUT: /tmp/gh-aw/agent_output.json
      run: '& (Join-Path $env:GITHUB_WORKSPACE "trusted-leak-hunter/.github/scripts/Assert-LeakHunterSafeOutputGate.ps1")'
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
5. **De-dup against open scanner issues, merged fixes, AND title-identified open fixes.** Before
   testing or filing, skip a leak already covered by this workflow's open `[leak-scan]` issue
   (same rooting API / retention path), by an exact `[leak-fix]` PR for the same API/retention
   path already merged to `main` or `inflight/current`, or by an OPEN `[leak-fix]` PR on either
   authoritative lane whose anchored title identifies the same API. Body provenance is not
   needed for this conservative same-API block. Do NOT suppress a candidate merely because
   AdamEssenmacher (or anyone else) has a repro/issue for it — duplicating those is fine. A
   candidate whose only prior scanner issue is CLOSED may be re-filed only when no equivalent
   supported-branch merged or open fix exists.
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

## Step 2 — Fetch open scanner issues plus merged/open fixes (de-dup)

Three de-dup sources matter:

1. this workflow's own open `[leak-scan]` issues; and
2. exact `[leak-fix]` PRs already merged to `main` or `inflight/current`; and
3. title-identified `[leak-fix]` PRs still OPEN on `main` or `inflight/current`.

You do **not** care about AdamEssenmacher's repro branches or anyone else's issues by
themselves — duplicating those is explicitly fine. A merged generated fix is different: the
shipped package may still reproduce the old leak even though the fix has already landed in the
active source flow, so filing it again would only create another redundant fix PR.

**This de-dup context was already gathered for you, before you started, by a deterministic
`pre-agent-steps` job step** (not a bash tool call you invoke) — see the workflow frontmatter.
That step runs on the plain runner (not through your sandbox) with `set -euo pipefail`, so a
`gh` failure or malformed, incomplete, repeated-cursor, or over-budget GraphQL pagination fails
the GitHub Actions step itself — and therefore the whole job, before you are ever started —
rather than merely returning an error you could choose to route around. Its output already
sits under `/tmp/gh-aw/agent/` (that path is shared with your sandbox), so you only need to
READ it:

```bash
echo "already-filed rooting APIs:"; cat /tmp/gh-aw/agent/already-filed-apis.txt
echo "already-merged fix APIs:"; cat /tmp/gh-aw/agent/already-merged-fix-apis.tsv
echo "already-open title-identified fix APIs:"; cat /tmp/gh-aw/agent/already-open-fix-apis.tsv
```

- `already-filed-apis.txt` — the rooting API identity of every currently-open `[leak-scan]`
  issue this workflow filed (one per line).
- `already-merged-fix-apis.tsv` / `.txt` — `API identity <TAB> PR# <TAB> baseRefName <TAB> URL
  <TAB> title` for every `[leak-fix]` PR already merged to `main` or `inflight/current` (the
  `.txt` is just the first column, deduplicated). Effective revert state is resolved
  recursively and per base branch. Editable repository-local `Reverts #<N>` body references
  only identify candidates; every edge also requires exactly one full target merge/squash
  commit OID in an immutable `This reverts commit <40-hex-OID>.` commit-message line from a
  complete merged candidate history. Any active same-branch verified direct reverter excludes
  its target. Reverting a reverter can deactivate that reverter only when that next edge is
  independently verified; sibling reverts never cancel each other. A servicing-branch revert
  cannot toggle a main/inflight fix. Missing, ambiguous, malformed, truncated, or over-budget
  evidence retains the original fix as active or blocks the run.
- `already-open-fix-apis.tsv` / `.txt` — the same identity and PR metadata for every
  authoritative OPEN `[leak-fix]` PR with a valid anchored title-derived API identity. Missing
  or malformed editable body provenance cannot erase that conservative same-API identity.
  Strict visible `Fixes #N` / `Refs: dotnet/maui#N` provenance remains mandatory when a new
  fix PR reaches its mutation boundary, and invalid provenance cannot establish a strict
  issue-provenance identity.

- A candidate is **OUT** if an open `[leak-scan]` issue, active merged `[leak-fix]` PR, or
  title-identified authoritative open `[leak-fix]` PR covers the same API identity. Use all three
  identity files as authoritative match signals and skip every match. **Check this for EVERY
  candidate before its test.**
- Extract each candidate with the same anchored title convention: the API must be the first
  dotted identifier chain immediately after `[leak-scan] ` (or after `[leak-fix] Fix `).
  Preserve every qualified identifier exactly, including `Microsoft.Maui.*`; never strip its
  namespace. Exact identifiers compare ordinal/case-sensitively. A legacy unqualified
  `Type.Member` conservatively overlaps any qualified identifier ending in that exact trailing
  pair, while two different qualified identifiers remain distinct even when their trailing
  pairs collide. Use `pwsh .github/scripts/Test-LeakApiIdentity.ps1 -Left "$API" -Right
  "$EXISTING_API"` for every comparison; do not use raw equality, substring matching, or
  trailing-pair normalization.
- For a merged-fix match, print the matching row(s) from
  `already-merged-fix-apis.tsv` and record
  `skipped: API identity already fixed via #<PR> to <baseRefName>`.
- For an open issue match, skip the candidate.
- Re-filing the same API identity under different wording/number is the primary failure mode.
- Immediately before issue mutation, a trusted safe-output step independently re-fetches open
  scanner issues plus a coherent all-state PR snapshot, extracts authoritative open-fix title
  identities independently of editable body provenance, and brackets branch-scoped
  effective-revert evaluation with a matching snapshot.
  A late same-API issue, merged fix, or open fix removes only the stale `create-issue` item.
  The gate atomically rewrites the bounded agent output so otherwise-distinct items proceed,
  and it blocks without mutation only when every requested issue became stale. Do not treat
  the pre-agent snapshot as the final authority.

A candidate whose only prior scanner issue is CLOSED may be re-filed when no active merged fix
or title-identified authoritative open fix covers the same API identity.

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
distinct candidate** across all focus areas that is not already an open `[leak-scan]` issue and
does not already have a supported-branch merged `[leak-fix]` PR — build a candidate list (aim
for several). Rank them strongest-first, then confirm as many as you can in Step 4/5. If —
after a genuine sweep — there is no convincing candidate at all, stop and create nothing (a
quiet run is fine — there is no coverage-gap fallback).

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

For **every** leak Step 5 confirmed whose API identity has not already been selected this run,
emit a `create-issue` safe-output (up to the 8 cap) — one issue per distinct leak, with at most
one output per exact or conservatively ambiguous rooting API identity in the current batch. If
multiple confirmed retention mechanisms share one identity, emit the strongest report and defer
the others to a later run rather than producing same-API siblings together. De-dup each
selected leak against open `[leak-scan]` issues, supported-branch merged `[leak-fix]` PRs,
title-identified authoritative open `[leak-fix]` PRs, AND the other issues you're filing this run.
Any exact or short/full ambiguous match blocks output because the trusted gate has no
independent evidence that an agent-authored mechanism comparison is correct. A trusted final
gate repeats the live de-dup immediately before mutation. It removes any late same-API matches
from the up-to-eight-item batch while preserving otherwise-distinct reports; if every item
became stale, it blocks the batch without mutation.
Each title MUST be of the form **`[leak-scan] <API identity> — <short mechanism>`** — it MUST
lead with the anchored API identity immediately after the tag. Use the most qualified source
identifier available and preserve it exactly (for example,
`[leak-scan] Microsoft.Maui.Controls.SwipeItemView.Command — non-weak
ICommand.CanExecuteChanged retains the control`). Never strip a `Microsoft.Maui.*` namespace.
Legacy short `Type.Member` identities remain supported but conservatively block every qualified
identity with that trailing pair. Keep the leading identity stable run-to-run.
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