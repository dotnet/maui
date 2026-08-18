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
     the test actually catches the leak. (If it already passes on unpatched source, the agent
     opens NO PR — but a single green run is **not** proof the leak is fixed, so it does **not**
     close the scan issue. An immutable merged fix PR suppresses redundant rebuilds, but automatic
     issue closure is disabled — see Step 3 / gate (d).)
  4. Implements the product fix (weak subscription / `WeakEventManager` / teardown on the
     missing path), rebuilds, and confirms the **same test now PASSES** with no regression
     in its neighbours.
  5. Opens ONE draft `[leak-fix]` PR (`Fixes #<issue>`) carrying the test + fix, with the
     before/after evidence in the body.

  Detection (the hunter) uses the shipped package and needs no source build; the FIXER
  must build MAUI **from source** to validate a product change, so it restores from the
  public dnceng Azure DevOps feeds (`dev.azure.com`) — hence the wider network allowlist.
  Scope is the managed `[leak-scan]` issues only; native/handler leaks are out of scope.

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
  schedule: every 6h
  workflow_dispatch:
    inputs:
      issue_number:
        description: "Scope to ONE [leak-scan] issue number (blank = auto-pick the oldest open one without a fix PR)."
        required: false
        type: string
      dry_run:
        description: "Preview only: do the full work locally but emit NO PR (print the would-be PR to the run log)."
        required: false
        type: boolean
        default: false
  # Forces a no-op pre_activation job, required by the pat_pool import. See shared/pat_pool.README.md.
  permissions: {}

# Runs on dotnet/maui only. The engine token is supplied by the Copilot PAT pool
# (shared/pat_pool.md) via the copilot-pat-pool environment — no per-repo secret needed.
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
  group: "leak-fixer"
  cancel-in-progress: false

timeout-minutes: 120
max-ai-credits: -1
max-daily-ai-credits: -1

tools:
  github:
    toolsets: [pull_requests, repos, issues, search]
    min-integrity: approved
  edit:
  bash: ["dotnet", "git", "gh", "find", "ls", "cat", "grep", "head", "tail", "wc", "jq", "tee", "sed", "awk", "tr", "cut", "sort", "uniq", "xargs", "echo", "date", "mkdir", "test", "env", "basename", "dirname", "bash", "sh", "chmod", "curl"]

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
  # Job-enforced dry-run: when `dry_run` is true, gh-aw stages every task safe-output below
  # (only logs/records what WOULD have been emitted) regardless of what the agent does.
  # The per-step "Dry-run gate" prompt text further down is a courtesy for a clean run log, but
  # it is NOT what makes dry_run safe — an agent that emits a mutating safe-output anyway
  # anyway (ignoring the prompt) must still produce NO write. Making this an emitted-tool-call
  # decision inside the prompt (as it was before) leaves the actual guarantee entirely up to
  # model compliance; `staged` moves the task-output guarantee into the generated jobs, so a
  # manual preview run (`dry_run: true`) cannot open/push a PR or add a comment.
  # gh-aw's separate conclusion job may still report framework diagnostics when a run is
  # incomplete or fails. Keep this identical to the pattern already used by
  # ci-status-main.md / ci-status-net11.md.
  staged: ${{ github.event_name == 'workflow_dispatch' && inputs.dry_run == true }}
  create-pull-request:
    # No fixed title-prefix: the agent writes the FULL title starting with "[leak-fix] "
    # (it fixes a runtime leak from a [leak-scan] issue). At most ONE PR per run.
    draft: true
    max: 1
    # This workflow transports the fix as a patch relative to ONE base branch.
    base-branch: "main"
    allowed-base-branches:
      - "main"
    allowed-branches:
      - "leak-fix/**"
    protected-files: blocked
    # Enforced allowlist of paths a run may touch: a runtime-leak fix is a managed product
    # change + a Core.UnitTests regression test (+ PublicAPI).
    allowed-files:
      - "src/Controls/src/**"
      - "src/Controls/tests/Core.UnitTests/**"
      - "src/Controls/tests/DeviceTests/**"
      - "src/Core/src/**"
      - "src/Essentials/src/**"
      - "**/PublicAPI.Unshipped.txt"
    labels: [agentic-workflows, "perf/memory-leak 💦"]
    allowed-labels: [agentic-workflows, "perf/memory-leak 💦"]
  # Track C — respond to review feedback on this workflow's OWN open [leak-fix] PRs.
  # Scoped by required-title-prefix so it can only touch [leak-fix] PRs.
  push-to-pull-request-branch:
    target: "*"                     # agent supplies pull_request_number (its own leak PR)
    required-title-prefix: "[leak-fix]" # only [leak-fix] PRs (closing bracket keeps it exact)
    max: 1
    if-no-changes: "ignore"
    protected-files: blocked
    # Same managed allowlist as create-pull-request (a review fix touches the same surface).
    allowed-files:
      - "src/Controls/src/**"
      - "src/Controls/tests/Core.UnitTests/**"
      - "src/Controls/tests/DeviceTests/**"
      - "src/Core/src/**"
      - "src/Essentials/src/**"
      - "**/PublicAPI.Unshipped.txt"
  add-comment:
    target: "*"                     # agent supplies the leak PR number
    required-title-prefix: "[leak-fix]"  # Track C only comments on this workflow's own [leak-fix] PRs
    max: 1
  # Most 6h runs are idle (every open leak already has a [leak-fix] PR). Without this,
  # gh-aw's auto-injected default (noop: report-as-issue: true) files a "no action taken"
  # issue every idle run. Mirror the hunter, which already opts out.
  noop:
    report-as-issue: false
---

# Memory Leak Fixer — dotnet/maui

Each run you take **exactly one action**, choosing the highest-priority track:

- **Track C — respond to review feedback (HIGHEST PRIORITY, do this FIRST).** If one of *this
  workflow's own* open `[leak-fix]` PRs has a review that **requested changes**
  you haven't addressed yet, work on THAT PR: apply the changes (push a commit to its branch)
  and/or post a comment pushing back on any request that is wrong or doesn't make sense. Then
  stop — do not also do new work this run.
- **Track A — `[leak-scan]` runtime leak.** Otherwise, produce a `[leak-fix]` PR that contains
  BOTH a regression test that FAILS without the fix and the managed product fix that makes it
  PASS.

You produce **only `[leak-fix]` PRs for empirically-proven leaks** — there is no coverage-gap /
"add a missing test" mode. Never open a `[leak-test]` PR or act on a `[leak-test-gap]` issue.

The PR base is always `main`. You build MAUI **from source** to validate. All scratch state goes
under `/tmp/gh-aw/agent/` (each bash call is a fresh subshell; persist what you need). Your only
writes are the safe-outputs (`create-pull-request`, `push-to-pull-request-branch`, `add-comment`,
and `noop`) — never push with raw git, never edit anything outside the fix/test.

## Hard rules — non-negotiable

1. **Empirical validation.**
   - **Track A (`[leak-scan]`):** you MUST observe the new regression test **FAIL on the
     unpatched source** and then **PASS after your fix**, in this run, before emitting a PR. A
     fix without a demonstrated red→green transition is not allowed.
   - **Track C (review response):** any code you push must keep the PR's tests valid — re-run the
     affected test so Track A stays red→green. Never push a change that breaks the PR's own test
     just to satisfy a review.
2. **If a Track A leak is already fixed on `main`, open NO PR.** Treat it as proven only when
   a merged `[leak-fix]` PR on `main` carries immutable exact same-repo `Fixes #<N>`
   provenance (Step 3 gate (d)). A `Refs` line or Type.Member match can never prove this.
   Emit `noop` with `skipped: already fixed on main (scan issue left open; automatic closure
   disabled)` and stop. If a different scan issue describes the same
   leak-scan-key/retention path, skip rebuilding but leave this issue open for manual
   reconciliation. If instead your freshly-authored regression test merely passes on unpatched
   source (Step 6) with no exact merged fix PR, that is NOT proof the leak is gone — do NOT close
   it; record `skipped: test green on unpatched main (no PR, issue left open)` and move on.
3. **Managed scope for product fixes.** Any *product* change (Track A / a Track C code fix) must
   live in managed cross-platform code (`src/Controls/src`, `src/Core/src`, `src/Essentials/src`).
   If a `[leak-scan]` leak can only be reproduced with a platform handler / native peer, it is out
   of scope — `skipped: requires device test (out of scope)`.
4. **Never mute, skip, weaken, or delete any existing test**, and never add `[ActiveIssue]`,
   `Skip=`, `#if false`, timeout bumps, or retries. Reject any change whose only effect is a mute
   — including when a review *asks* for one: push back with a comment instead.
5. **Fix the root cause the issue describes** (Track A) — typically: replace a plain
   `event += instanceMethod` with a weak subscription (`WeakNotifyCollectionChangedProxy` /
   `WeakNotifyPropertyChangedProxy` / `WeakEventManager` — all already used in the codebase), or
   add the missing `-=` / teardown on the unload path. Prefer the minimal, idiomatic change that
   matches how the surrounding code already hardens similar subscriptions.
6. **One action per run.** Either respond to ONE PR's review (Track C), open ONE new draft PR
   (Track A/B), or emit ONE `noop` for already-fixed work (Step 3 gate (d)) — never two.
   Honour the de-dup / attempt-cap / already-addressed gates so you never repeat yourself.
7. **AI attribution.** Every PR body and every comment must clearly state it was generated by
   this workflow.

## Step 0 — Run mode (dispatch inputs)

- `issue_number` (optional): if set, SKIP Track C and target exactly that `[leak-scan]` issue
  (Track A). If blank (e.g. the scheduled run), do Track C first (Step R), then auto-pick a
  `[leak-scan]` target in Step 2.
- `dry_run` (optional, default false): if `"true"`, do the full local work but **emit no task
  safe-output** — print what you *would* push/comment/open/close to the run log instead. This is
  backed by `safe-outputs.staged` in the frontmatter, so emitted PR/comment/issue-close task
  outputs are staged on a `dry_run` run. The separate gh-aw conclusion job may still create a
  framework diagnostic issue if the run is incomplete or fails.

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

# ===================== TRACK C — RESPOND TO REVIEW FEEDBACK (do this FIRST) =====================

Skip this entire section if a `issue_number` input was provided (that's an explicit Track A
request). Otherwise, BEFORE looking for new work, see whether one of this workflow's own open
`[leak-fix]` PRs has a review requesting changes that you have not yet addressed.

## Step R1 — Find an open leak PR with UN-addressed requested changes

```bash
# This workflow's own open [leak-fix] PRs (its create-pull-request output labels them agentic-workflows).
# Track C pushes fixes via push-to-pull-request-branch, which can ONLY reach PR heads hosted on
# this same repo — scheduled runs have no credentials for FORK-hosted heads. So consider only
# same-repo (dotnet/maui-hosted) PRs here; skip fork-hosted [leak-fix] PRs entirely.
OWNER="${GITHUB_REPOSITORY%%/*}"
gh pr list --repo "$GITHUB_REPOSITORY" --state open \
  --search '"[leak-fix]" in:title label:agentic-workflows' --limit 200 \
  --json number,title,headRefName,headRepositoryOwner,url,updatedAt \
  | jq --arg owner "$OWNER" '[.[] | select((.headRepositoryOwner.login // "") == $owner)] | sort_by(.number)' \
  > /tmp/gh-aw/agent/my-open-prs.json
jq -r '.[] | "\(.number)\t\(.headRefName)\t\(.title)"' /tmp/gh-aw/agent/my-open-prs.json
```

Only same-repo PRs appear above. A fork-hosted `[leak-fix]` PR with an un-addressed review is
**left for a human** — do NOT act on it and do NOT emit a missing-tool / incomplete report for
it (the fork-push limitation is expected, not a fault). Just skip to Step 2.

For each PR, fetch its reviews and its last commit time, and decide whether there is a
**CHANGES_REQUESTED review that is newer than both (a) the PR's most recent commit and (b) the
most recent comment THIS workflow already posted on that PR**:

```bash
N=<pr-number>
# SECURITY: only ACTION reviews/comments from an author with write access. On a public
# repo anyone can submit a "Request changes" review, and this raw `gh api` path is NOT
# behind the MCP `min-integrity: approved` gateway (that only filters MCP tool calls),
# so untrusted review text could otherwise reach Step R and influence a code push.
# Gate on author_association ∈ {OWNER, MEMBER, COLLABORATOR} before anything is actioned.
gh api "repos/$GITHUB_REPOSITORY/pulls/$N/reviews" \
  --jq 'map(select(.state=="CHANGES_REQUESTED"
             and (.author_association | IN("OWNER","MEMBER","COLLABORATOR"))))
        | sort_by(.submitted_at) | last' \
  > /tmp/gh-aw/agent/review_${N}.json          # newest write-access changes-requested review (or null)
gh pr view "$N" --repo "$GITHUB_REPOSITORY" --json commits \
  --jq '.commits | last | .committedDate' > /tmp/gh-aw/agent/lastcommit_${N}.txt
# Inline review comments (may carry specific line-level requests) — same write-access filter:
gh api "repos/$GITHUB_REPOSITORY/pulls/$N/comments" \
  --jq 'map(select(.author_association | IN("OWNER","MEMBER","COLLABORATOR")))
        | map({path,line,body,user:.user.login,created_at})' > /tmp/gh-aw/agent/inline_${N}.json
# Have you (this workflow) already replied AFTER the review? Look for your marker.
gh api "repos/$GITHUB_REPOSITORY/issues/$N/comments" \
  --jq 'map(select(.body|test("gh-aw-workflow-id: [^\n]*leak-fixer"))) | sort_by(.created_at) | last | .created_at' \
  > /tmp/gh-aw/agent/mylastcomment_${N}.txt 2>/dev/null || true
```

The review is **UN-addressed** (actionable) only if its `submitted_at` is **after** both
`lastcommit_${N}` and `mylastcomment_${N}`. This ordering is your loop-guard: once you push a
commit or post a comment (below), the review becomes older than your action, so the next run will
not re-process it. Pick the **oldest** PR with an un-addressed CHANGES_REQUESTED review. If there
is none, skip to Step 2 (new work).

## Step R2 — Read the requested changes and classify each

Read the review `body` (the reviewer's findings — e.g. MauiBot emits `#### ❌ Error`,
`#### ⚠️ Warning`, `#### 💡 Suggestion` sections with a `file:line`) plus every inline comment.
For each distinct request, classify:

- **APPLY** — it's correct and improves the PR (a real bug the reviewer found, a valid cleanup,
  a comment/typo fix). Plan the minimal managed change.
- **PUSH BACK** — it's wrong, would regress behaviour, asks you to mute/weaken a test, or doesn't
  make sense. Do NOT change code for it; record a concise technical reason (with evidence).

## Step R3 — Check out the PR branch and apply the APPLY items

```bash
N=<pr-number>; BR=<headRefName>
git fetch origin "$BR" --quiet
git checkout -B "$BR" "origin/$BR"
```

Make the minimal, idiomatic managed changes for the APPLY items only (same non-muting rules as
Track A). Then **re-validate on the runner** so the PR stays valid — rebuild + run the PR's own
test with the net-only flags (Step 6/8 flags) and confirm it still holds (the fix test still
passes and would still fail without the fix). If an APPLY
change breaks the PR's test, it was wrong — revert that item and move it to PUSH BACK.

Commit with the injection-safe heredoc + fresh-random-delimiter discipline (see Step 9), subject
`[leak-fix] Address review feedback (refs #<issue>)`. If you applied nothing (everything was
PUSH BACK), make no commit.

## Step R4 — Emit the response (push and/or comment) — ONE PR, then STOP

- If you committed APPLY changes: emit ONE `push-to-pull-request-branch` targeting PR `N`
  (`pull_request_number: N`) with your commit.
- Emit ONE `add-comment` on PR `N` (`pull_request_number: N`) that:
  - is clearly **AI-generated** (name this workflow),
  - lists what you **applied** (per finding) and the re-validation result, and
  - for each **PUSH BACK** finding, states plainly why you did not change it (technical reason +
    evidence). Be courteous and specific; it is fine to disagree with a review when you are right.

> **Dry-run gate:** if `dry_run == "true"`, do NOT emit — print what you would push (diff --stat)
> and the comment body to the run log instead. (Enforced job-side too: `safe-outputs.staged`
> stages, and does not write, `push-to-pull-request-branch` / `add-comment` when `dry_run` is
> true, so this print-only behavior holds even if you emit anyway.)

Track C uses this run's single action. **Stop here — do not also do Track A/B.** If no PR needed
a response, fall through to Step 2.

## Step 2 — Select the target `[leak-scan]` issue

If `issue_number` was provided, use it (it must be a `[leak-scan]` issue → Track A). Otherwise
auto-pick: materialize this scanner's open `[leak-scan]` issues as an ordered candidate queue
(oldest first). Evaluate candidates one at a time through every Step 3 gate until one is
actionable. Do not make the oldest issue a terminal choice before those gates run: a proven
merged fix remains open by design and must not starve newer unfixed issues.

```bash
# Open [leak-scan] (Track A) issues, oldest first.
gh issue list --repo "$GITHUB_REPOSITORY" --search '"[leak-scan]" in:title' \
  --state open --label agentic-workflows --limit 100 \
  --json number,title,createdAt,body \
  | jq 'sort_by(.createdAt)' > /tmp/gh-aw/agent/leakscan-issues.json
echo "--- [leak-scan] (Track A) ---"; jq -r '.[] | "\(.number)\t\(.title)"' /tmp/gh-aw/agent/leakscan-issues.json
```

(The Daily Memory Leak Hunter files `[leak-scan]` issues with the `agentic-workflows` label.)

For the current queue candidate, read the issue's body in full
(`gh issue view <N> --json title,body`). Extract:

- the **rooting API** (e.g. `IndicatorView.ItemsSource`),
- the **retention path** `root -> … -> transient` with the cited file:line(s),
- the canonical `<!-- leak-scan-key: ... -->` marker when present (legacy issues may not have one),
- the **suggested fix** shape, and
- any **non-default / disabling condition**.

## Step 2.5 — Fetch workflow-owned `[leak-fix]` PR sets ONCE per run (cache, don't re-query)

Step 3's gates below all filter this SAME three-state `[leak-fix]` PR history with per-candidate
`jq` (keyed on `$N` / `$API`), but the raw open/closed/merged PR sets themselves do NOT depend on
the candidate. Fetch each state ONCE here and cache it. Do NOT ancestry-check every merged PR at
this stage: Step 3 first narrows the raw cache to the handful matching the exact scan issue or
rooting API, then calls the compare API only for those survivors. This preserves the main-ancestry
guard without spending up to 1000 sequential REST calls before the first candidate is evaluated.

```bash
# Open [leak-fix] PRs — gates (a) and (b) below both filter this SAME set (issue-ref match and
# same-rooting-API match respectively), so fetch it once with both fields instead of twice.
gh pr list --repo "$GITHUB_REPOSITORY" --state open --search '"[leak-fix]" in:title label:agentic-workflows' --limit 200 \
  --json number,title,body > /tmp/gh-aw/agent/open-leakfix-all.json
jq 'length' /tmp/gh-aw/agent/open-leakfix-all.json
# Closed-unmerged [leak-fix] PRs — feeds gate (c)'s attempt cap.
# --limit 1000 = the GitHub SEARCH API's hard ceiling (pagination cannot exceed it). The
# attempt cap is durable state (a genuinely un-fixable leak must stop being retried), but the
# search is global: as total closed [leak-fix] history grows past the cap, older attempts for
# THIS issue could fall off the set and let the 3-attempt cap under-count (allowing a 4th+
# rebuild). Fail-safe direction (over-processing, never data loss), but warn if we hit it.
gh pr list --repo "$GITHUB_REPOSITORY" --state closed --search '"[leak-fix]" in:title label:agentic-workflows' --limit 1000 \
  --json number,title,body,mergedAt > /tmp/gh-aw/agent/closed-leakfix-all.json
if [ "$(jq 'length' /tmp/gh-aw/agent/closed-leakfix-all.json)" -ge 1000 ]; then
  echo "WARNING: closed [leak-fix] set hit the 1000 search-API ceiling; older attempts may be TRUNCATED and the 3-attempt cap could under-count for some issues — switch to date-windowed enumeration."
fi
# MERGED [leak-fix] PRs — Step 3 first filters this raw set, then ancestry-checks only matches.
# --limit 1000 = the GitHub SEARCH API's hard result ceiling (pagination cannot exceed it). If
# the merged [leak-fix] set ever hits 1000, older fixes are TRUNCATED and this gate could miss a
# valid merged fix. Gate (d) stays fail-safe (a miss rebuilds rather than suppressing a real leak),
# but de-dup coverage would be incomplete, so warn.
gh pr list --repo "$GITHUB_REPOSITORY" --state merged --search '"[leak-fix]" in:title label:agentic-workflows' --limit 1000 \
  --json id,number,title,body,mergedAt,mergeCommit > /tmp/gh-aw/agent/merged-leakfix-raw.json
if [ "$(jq 'length' /tmp/gh-aw/agent/merged-leakfix-raw.json)" -ge 1000 ]; then
  echo "WARNING: merged [leak-fix] provenance hit the 1000 search-API ceiling; older merged fixes may be TRUNCATED. De-dup remains fail-safe (may rebuild) but coverage is incomplete — switch to date-windowed enumeration."
fi
```

Do this ONCE at the top of the run, before evaluating any candidate. If Step 2 gates out the
first candidate and moves to the next-oldest, do NOT re-run the fetches above — go straight to
Step 3 and re-apply its filters and targeted ancestry checks to the cached files for the new
`$N` / `$API`.

## Step 3 — De-dup + attempt cap (per-candidate filters against the Step 2.5 cache)

A fix PR carries `Fixes #<N>` in its body (where `<N>` is the `[leak-scan]` issue) — that is the
sole scan-issue join and the ONLY key allowed to prove an exact already-fixed match. An optional
`Refs: <owner>/<repo>#<UPSTREAM>` line points at a separate issue and can never prove that
`#<N>` was fixed. Duplicate scan issues may still describe the same leak, so API matching is retained only
as a cheap prefilter. Before skipping work for a different issue number, compare its
`leak-scan-key` or full retention path; the Type.Member alone is not a leak identity.

```bash
N=<issue-number>
gh issue view "$N" --repo "$GITHUB_REPOSITORY" --json number,title,body,state \
  > /tmp/gh-aw/agent/target-scan-issue.json
# The rooting Type.Member this issue is about (titles lead with it: "[leak-scan] Type.Member — ...").
# Use the same extraction as daily-leak-hunter.md (last Type.Member pair of the first identifier
# chain) so off-contract / fully-qualified titles key identically on both sides of the pipeline.
API=$(jq -r '.title // ""' /tmp/gh-aw/agent/target-scan-issue.json \
  | sed -E 's/^\[leak-scan\] *//' \
  | awk '{ if (match($0, /[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)+/)) { chain=substr($0,RSTART,RLENGTH); n=split(chain,seg,"."); print seg[n-1]"."seg[n] } }')
TARGET_LEAK_KEY=$(jq -L "$GITHUB_WORKSPACE/.github/scripts" -r '
  include "leak-workflow-provenance";
  leak_scan_key // empty' \
  /tmp/gh-aw/agent/target-scan-issue.json)
echo "target rooting API: $API"
echo "target leak key: ${TARGET_LEAK_KEY:-<legacy — compare retention path>}"
# Escape the owner/repo so it embeds literally in the jq test() calls below. The issue-ref match
# requires "#<N>" to be a BARE same-repo reference OR an explicit "<owner>/<repo>#<N>" qualifier —
# NEVER an arbitrary cross-repo "other/repo#<N>". Otherwise an unrelated upstream ref such as
# "Refs: dotnet/runtime#5000" would satisfy a search for maui #5000.
REPO_RE=$(printf '%s' "$GITHUB_REPOSITORY" | sed -E 's/[][(){}.^$*+?|\\]/\\&/g')
# (a) Open [leak-fix] PR already addressing THIS issue number? Filters the Step 2.5 cache — no
#     new gh pr list call for this (or any later) candidate.
jq -L "$GITHUB_WORKSPACE/.github/scripts" --arg n "$N" --arg repo "$REPO_RE" '
    include "leak-workflow-provenance";
    [.[] | select(leak_has_issue_reference($repo; $n))]' \
    /tmp/gh-aw/agent/open-leakfix-all.json \
  > /tmp/gh-aw/agent/open-fix-prs.json
jq 'length' /tmp/gh-aw/agent/open-fix-prs.json
# (b-prefilter) Open [leak-fix] PRs naming the same rooting Type.Member (any issue number).
#     [leak-fix] PR titles lead with "Fix <Type>.<Member> ...". Normalize each PR title with the
#     same last-dotted-pair extraction used for $API. This is only a cheap prefilter: Step (b)
#     below resolves each PR's exact Fixes scan issue and compares leak-scan-key / retention path
#     before deciding to skip. API equality alone is never enough.
jq -L "$GITHUB_WORKSPACE/.github/scripts" --arg apiplain "$API" --arg targetkey "$TARGET_LEAK_KEY" '
    include "leak-workflow-provenance";
    def titleapi: [scan("[A-Za-z_][A-Za-z0-9_]*(?:\\.[A-Za-z_][A-Za-z0-9_]*)+")] | if length>0 then (.[0]|split(".")|.[-2:]|join(".")) else null end;
    def bodykey: leak_scan_key;
    [.[] | select((((.title // "") | titleapi) == $apiplain) or (($targetkey != "") and (bodykey == $targetkey)))]' \
    /tmp/gh-aw/agent/open-leakfix-all.json \
  > /tmp/gh-aw/agent/same-api-open-candidates.json
# (c) Closed-unmerged attempts for this issue (attempt cap = 3). Filters the Step 2.5 cache.
jq -L "$GITHUB_WORKSPACE/.github/scripts" --arg n "$N" --arg repo "$REPO_RE" '
    include "leak-workflow-provenance";
    [.[] | select(leak_has_issue_reference($repo; $n) and (.mergedAt == null))]' \
    /tmp/gh-aw/agent/closed-leakfix-all.json \
  > /tmp/gh-aw/agent/closed-fix-prs.json
jq 'length' /tmp/gh-aw/agent/closed-fix-prs.json
# (d-prefilter) Narrow the raw merged cache BEFORE any compare API calls. Keep PRs that either
#     carry the exact same-repo Fixes #N provenance (the only destructive-close key) or name the
#     same rooting API (a non-destructive semantic de-dup candidate).
jq -L "$GITHUB_WORKSPACE/.github/scripts" --arg n "$N" --arg apiplain "$API" --arg targetkey "$TARGET_LEAK_KEY" --arg repo "$REPO_RE" \
    'include "leak-workflow-provenance";
     def titleapi: [scan("[A-Za-z_][A-Za-z0-9_]*(?:\\.[A-Za-z_][A-Za-z0-9_]*)+")] | if length>0 then (.[0]|split(".")|.[-2:]|join(".")) else null end;
     def bodykey: leak_scan_key;
     [.[] | select(leak_has_exact_fixes($repo; $n)
       or (((.title // "") | titleapi) == $apiplain)
       or (($targetkey != "") and (bodykey == $targetkey)))]' \
    /tmp/gh-aw/agent/merged-leakfix-raw.json \
  > /tmp/gh-aw/agent/merged-fix-candidates.json

# A merged PR is usable only when its current body is proven to be its merge-time body and its
# merge commit is actually contained in main. PullRequest.lastEditedAt is available through
# GraphQL (including userContentEdits history); gh pr list does not expose it. A post-merge edit
# or unavailable/malformed edit metadata fails closed for provenance, so a later `Fixes` line
# cannot suppress work. Compare only the verified candidate subset, not the entire history.
printf '' > /tmp/gh-aw/agent/merged-onmain-candidates.ndjson
jq -c '.[]' /tmp/gh-aw/agent/merged-fix-candidates.json | while IFS= read -r pr; do
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
  [ -z "$edit_meta" ] && continue
  provenance_guard=$(jq -L "$GITHUB_WORKSPACE/.github/scripts" -c '
    include "leak-workflow-provenance";
    .data.node | leak_merge_provenance_guard' <<<"$edit_meta" 2>/dev/null) || provenance_guard=""
  if [ -z "$provenance_guard" ] ||
     [ "$(jq -r '.verified' <<<"$provenance_guard")" != "true" ] ||
     [ "$(jq -r '.block_provenance' <<<"$provenance_guard")" = "true" ]; then
    echo "WARNING: merged PR provenance is mutable or unverified; ignoring PR #$(jq -r '.number' <<<"$pr")"
    continue
  fi
  ahead=$(gh api "repos/$GITHUB_REPOSITORY/compare/main...$sha" -q '.ahead_by' 2>/dev/null) || ahead=""
  if [ "$ahead" = "0" ]; then
    printf '%s\n' "$pr" >> /tmp/gh-aw/agent/merged-onmain-candidates.ndjson
  fi
done
if [ -s /tmp/gh-aw/agent/merged-onmain-candidates.ndjson ]; then
  jq -s '.' /tmp/gh-aw/agent/merged-onmain-candidates.ndjson > /tmp/gh-aw/agent/merged-onmain-candidates.json
else
  echo '[]' > /tmp/gh-aw/agent/merged-onmain-candidates.json
fi

# (d-exact) ONLY an exact immutable same-repo Fixes #N match may prove this selected scan issue is
# already fixed. A same Type.Member with a different issue number is not provenance for this
# retention path. This gate suppresses rebuilding only; automatic issue closure is disabled.
jq -L "$GITHUB_WORKSPACE/.github/scripts" --arg n "$N" --arg repo "$REPO_RE" \
    'include "leak-workflow-provenance";
     [.[] | select(leak_has_exact_fixes($repo; $n))]' \
    /tmp/gh-aw/agent/merged-onmain-candidates.json \
  > /tmp/gh-aw/agent/merged-exact-fix-prs.json
jq -r '.[] | "exact merged fix for scan issue: #\(.number) \(.title)"' \
  /tmp/gh-aw/agent/merged-exact-fix-prs.json

# Enrich same-API open/merged candidates with the scan issue named by their exact Fixes line.
# This makes the scan issue's marker/title/body — not the independently-authored PR title — the
# cross-workflow leak identity. Legacy scan issues without a marker remain available for semantic
# retention-path comparison. Entries without valid same-repo scan provenance are ignored.
enrich_scan_provenance () {
  input=$1
  output=$2
  printf '' > "$output"
  jq -c '.[]' "$input" | while IFS= read -r pr; do
    scan_n=$(jq -L "$GITHUB_WORKSPACE/.github/scripts" -r --arg repo "$REPO_RE" '
      include "leak-workflow-provenance";
      leak_first_exact_fixes_number($repo)' <<<"$pr")
    [ -z "$scan_n" ] && continue
    issue=$(gh issue view "$scan_n" --repo "$GITHUB_REPOSITORY" --json number,title,body,state 2>/dev/null) || issue=""
    [ -z "$issue" ] && continue
    key=$(jq -L "$GITHUB_WORKSPACE/.github/scripts" -r '
      include "leak-workflow-provenance";
      leak_scan_key // empty' <<<"$issue")
    jq -n --argjson pr "$pr" --argjson issue "$issue" --arg key "$key" \
      '{pull_request:$pr, scan_issue:$issue,
        leak_scan_key:($key | if . == "" then null else . end)}' >> "$output"
  done
}
enrich_scan_provenance /tmp/gh-aw/agent/same-api-open-candidates.json \
  /tmp/gh-aw/agent/same-api-open-fixes.ndjson
if [ -s /tmp/gh-aw/agent/same-api-open-fixes.ndjson ]; then
  jq -s '.' /tmp/gh-aw/agent/same-api-open-fixes.ndjson > /tmp/gh-aw/agent/same-api-open-fixes.json
else
  echo '[]' > /tmp/gh-aw/agent/same-api-open-fixes.json
fi
jq -L "$GITHUB_WORKSPACE/.github/scripts" --arg n "$N" --arg apiplain "$API" --arg targetkey "$TARGET_LEAK_KEY" --arg repo "$REPO_RE" \
    'include "leak-workflow-provenance";
     def titleapi: [scan("[A-Za-z_][A-Za-z0-9_]*(?:\\.[A-Za-z_][A-Za-z0-9_]*)+")] | if length>0 then (.[0]|split(".")|.[-2:]|join(".")) else null end;
     def bodykey: leak_scan_key;
     [.[] | select((leak_has_exact_fixes($repo; $n) | not)
       and ((((.title // "") | titleapi) == $apiplain)
         or (($targetkey != "") and (bodykey == $targetkey))))]' \
    /tmp/gh-aw/agent/merged-onmain-candidates.json \
  > /tmp/gh-aw/agent/same-api-merged-candidates.json
enrich_scan_provenance /tmp/gh-aw/agent/same-api-merged-candidates.json \
  /tmp/gh-aw/agent/same-api-merged-fixes.ndjson
if [ -s /tmp/gh-aw/agent/same-api-merged-fixes.ndjson ]; then
  jq -s '.' /tmp/gh-aw/agent/same-api-merged-fixes.ndjson > /tmp/gh-aw/agent/same-api-merged-fixes.json
else
  echo '[]' > /tmp/gh-aw/agent/same-api-merged-fixes.json
fi

```

**Gate disposition rule (prevents oldest-fixed starvation):**

- When `issue_number` was explicitly supplied, a no-work gate emits one informative `noop` and
  stops, because the caller requested that exact issue.
- During automatic selection, a no-work gate emits **no safe-output**, records the skip in the run
  log, and immediately continues with the next entry in `leakscan-issues.json`. Never stop the run
  merely because an auto-selected candidate is already fixed, already being fixed, duplicate,
  capped, or otherwise ineligible.

- If an **exact immutable** merged `[leak-fix]` PR in `merged-exact-fix-prs.json` carries this
  same-repo `Fixes #<N>` line, the scan issue's fix is already on `main`. Do NOT create a branch
  or rebuild. Record:
  `skipped: already fixed on main (scan issue left open; automatic closure disabled)`, including
  the merged PR number/title. If this candidate was auto-selected, continue to the next candidate
  without emitting `noop`; if `issue_number` was explicit, emit that message as one `noop` and
  stop.

  The workflow deliberately has no `close-issue` safe-output. In pinned gh-aw v0.85.4 the
  close handler re-fetches title/labels/state, then posts a comment and PATCHes the issue closed;
  it has no atomic timeline/version precondition. A maintainer can re-open between any check and
  that deferred PATCH. Leaving the issue open while suppressing the redundant rebuild is the only
  race-free way to preserve maintainer state with the available GitHub mutation APIs.
- If an **open** fix PR already refs this exact issue (a) → record
  `skipped: leak already being fixed`, then apply the gate disposition rule.
- For `same-api-open-fixes.json` and `same-api-merged-fixes.json`, API equality is only a
  prefilter. Skip without closing/rebuilding only when the referenced scan issue has the same
  non-empty `leak-scan-key` as `TARGET_LEAK_KEY`, or its title/body describes the same
  publisher/event/collection and the same `root -> ... -> transient` retention edge.
  If the Type.Member matches but the mechanism differs, continue normally. A same-leak merged fix
  under a different scan issue number is non-destructive: leave this issue open for manual
  reconciliation rather than claiming it was explicitly fixed.
- If **3+ closed-unmerged** attempts exist → record `skipped: attempt cap reached (3)`, then
  apply the gate disposition rule.
- An issue that is already CLOSED → record `skipped: issue closed`, then apply the gate
  disposition rule.

In automatic mode, keep advancing until a candidate reaches Track A or the ordered queue is
exhausted. If every candidate is gated out, emit one final `noop` summarizing the skip counts and
stop. This final queue-exhausted noop is the only safe-output produced by an all-skipped automatic
run.

Then proceed with **Track A** (`[leak-scan]`) → Steps 4–10.

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
- **Test PASSES on unpatched source** → your regression test does not reproduce the leak on
  `main`. Per Hard Rule 2, open NO PR. **Do NOT close the `[leak-scan]` issue here.** A single
  green result from a test *you just authored* is **not** proof the leak is fixed — the repro may
  simply have failed to recreate the retention path (wrong root, too little GC pressure, a typo'd
  assertion). Automatic issue closure is reserved for the **provenance-backed** path only —
  Step 3 gate (d), where a **merged `[leak-fix]` PR** for the same leak is the evidence. Here,
  just record `skipped: test green on unpatched main (no PR, issue left open)` and stop. If the
  leak really is fixed on `main`, gate (d) will close the scan issue once the merged fix is
  detected; if the test was a bad repro, leaving the issue open lets a later run try again — far
  safer than closing a still-live leak on weak evidence.
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
# A leak fix must ONLY touch managed src/** — never workflow/infra files. Defensively reset
# .github to origin/main and stage only src/** so nothing unrelated can enter the patch.
# (No-op when running on main; strips the test-branch's own workflow delta in pre-merge runs.)
git checkout origin/main -- .github 2>/dev/null || true
dotnet format src/Controls/src/Controls.Core.csproj --no-restore 2>&1 | tail -5 || true
git add -A -- src
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
> `git --no-pager diff --stat "origin/main..HEAD"`, then stop. (Enforced job-side too:
> `safe-outputs.staged` stages, and does not open, `create-pull-request` when `dry_run` is true.)

Emit exactly one `create-pull-request` safe-output. Do NOT set a `base` field (the
`base-branch: main` config pins it). Source `branch` MUST be `leak-fix/issue-<N>`. Title MUST
start with the literal tag **`[leak-fix] `** followed by e.g. `Fix <rooting API> memory leak
(Fixes #<N>)`, where **`<N>` is the `[leak-scan]` issue number you selected in Step 2** — not a
pre-existing upstream issue number. The `Fixes #<N>` join key MUST point at the `[leak-scan]`
issue so GitHub auto-closes it on merge; otherwise the scan issue is orphaned and the fixer
re-picks and rebuilds it every run.

The body MUST start with the required MAUI testing note (verbatim, no block-quote on the first
two lines as shown), then the details. Before constructing it, copy the selected scan issue's
exact `leak-scan-key` marker. For a legacy issue without one, derive
`Type.Member|short-mechanism-slug` from the issue's mechanism text, never from the PR title.
Include an optional `Refs: <owner>/<repo>#<UPSTREAM>` line only for a separate pre-existing
upstream issue; otherwise omit that line entirely. `Fixes #<N>` always names the selected
`[leak-scan]` issue and is the sole auto-close key.

```markdown
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

> [!NOTE]
> 🤖 **AI-generated PR** — produced automatically by the **Memory Leak Fixer** agentic workflow from issue #<N>. The regression test below was observed to FAIL on unpatched `main` and PASS with this fix, on the runner. Please review carefully before merging.

Fixes #<N>
<!-- leak-scan-key: <canonical-key-from-scan-issue> -->
Refs: <owner>/<repo>#<UPSTREAM>
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
`Fixes #<N>` line is present **whose `<N>` is the `[leak-scan]` issue number selected in Step 2**
(not an upstream issue), and that the body contains exactly one `leak-scan-key` marker copied from
or canonically derived from that scan issue. If not, drop the attempt:
`skipped: PR self-check failed`.

If no PR was emitted this run (already-fixed, gated out, or validation failed), produce no
output — that is an acceptable outcome. Otherwise you have produced exactly one validated,
draft `[leak-fix]` PR.