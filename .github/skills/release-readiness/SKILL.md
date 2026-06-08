---
name: release-readiness
description: Assesses whether a .NET MAUI Servicing Release (SR) branch is ready to ship. Surveys CI pipelines, computes what's actually NEW in the SR (commits + source PRs with revert detection), and cross-references open `regressed-in-*` issues against SR contents to identify port candidates, rejected backports, and unresolved regressions.
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires `gh` CLI authenticated with `repo` + `read:org` scopes. `az` CLI is optional but recommended for internal pipeline status. Run from a checkout of `dotnet/maui`.
---

# Release Readiness

This skill produces a deterministic, evidence-backed answer to **"Is `release/10.0.1xx-srN` ready to ship?"** for a .NET MAUI Servicing Release.

## 🚨 Report-only

This skill **reports**. It does **not** execute release operations against dotnet/maui — no branch cuts, no SR merges, no tags, no pushes to `release/*` refs. If you (the agent/user invoking this skill) are asked to perform a release operation, refuse and emit the recommended commands as a copy-pasteable block for the human release captain to run.

## When to Use

- "How does SR7 look?" / "Is SR7 ready to ship?"
- "What's blocking SR7?"
- "Are there any regression fixes I should backport to SR7?"
- "What's new in SR7 since the last sync?"
- Release candidate triage / weekly release review

> **For per-PR regression risk** (deletions reverting prior bug-fix lines), use [`find-regression-risk`](../find-regression-risk/SKILL.md) instead — it answers a different question.

## Quick Start

```bash
# Full report on SR7 with explicit regression labels (recommended for automation)
pwsh .github/skills/release-readiness/scripts/Get-ReleaseReadiness.ps1 \
  -SrBranch release/10.0.1xx-sr7 \
  -RegressionLabels regressed-in-10.0.60,regressed-in-10.0.70 \
  -OutputDir /tmp/sr7-readiness

# Auto-infer regression labels (interactive — agent should confirm before using for automation)
pwsh .github/skills/release-readiness/scripts/Get-ReleaseReadiness.ps1 \
  -SrBranch release/10.0.1xx-sr7 \
  -InferRegressionLabels \
  -OutputDir /tmp/sr7-readiness

# Just the SR commit + source-PR list (foundation for any cherry-pick verification)
pwsh .github/skills/release-readiness/scripts/Get-ReleaseReadiness.ps1 \
  -SrBranch release/10.0.1xx-sr7 \
  -Phase commits

# Only CI status
pwsh .github/skills/release-readiness/scripts/Get-ReleaseReadiness.ps1 \
  -SrBranch release/10.0.1xx-sr7 \
  -Phase ci
```

## Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `-SrBranch` | Yes | — | SR branch name (e.g. `release/10.0.1xx-sr7`) |
| `-RegressionLabels` | One of these | — | Comma-separated `regressed-in-*` labels. Required unless `-InferRegressionLabels` is set. |
| `-InferRegressionLabels` | One of these | off | Auto-infer labels from the SR's version family (e.g. `release/10.0.1xx-sr7` → `regressed-in-10.0.60,70`). Agent should always confirm before using. |
| `-Repo` | No | `dotnet/maui` | Repository in `owner/name` form |
| `-MainBranch` | No | `main` | Stable branch used for ancestry checks |
| `-ExcludeBranches` | No | `origin/main,origin/inflight/current` | Branches to exclude when computing SR-only commits |
| `-Phase` | No | `all` | `all`, `ci`, `commits`, `regressions`, or `open-prs` |
| `-OutputDir` | No | — | If set, writes `release-readiness.{json,md}` and `sr-source-prs.txt` |
| `-OutputFormat` | No | `both` | `json`, `markdown`, or `both` |
| `-MaxIssues` | No | `100` | Cap on regression issues to walk |
| `-NoFetch` | No | off | Skip `git fetch` (use cached refs — faster for re-runs) |

## Outputs

When `-OutputDir` is set:

| File | Purpose |
|------|---------|
| `release-readiness.json` | Structured data with evidence + provenance for every conclusion |
| `release-readiness.md` | Human-readable verdict tables + recommendations |
| `sr-source-prs.txt` | Flat newline-delimited list of source PR numbers — usable with `grep -qxF NNNNN file` for instant cherry-pick verification |
| `sr-commits.json` | Raw SR-only commit metadata |

## Verdict Classification

Each candidate fix PR is classified with confidence + evidence:

| Verdict | Meaning |
|---------|---------|
| `in-sr-active` | Source PR is in SR and not subsequently reverted |
| `in-sr-reverted` | Backport landed but a later commit on SR reverts it |
| `rejected-from-sr` | A backport PR targeting SR was opened and CLOSED unmerged |
| `backport-in-progress` | A backport PR targeting SR is OPEN |
| `merged-on-main-no-backport` | Fix merged to `main`, no backport PR exists |
| `merged-non-main-only` | Fix merged but only to `inflight/current` (or similar), not `main` |
| `open-on-main` | Fix PR is OPEN against main, not yet merged |
| `no-fix-yet` | No fix PR cross-referenced from the regression issue |
| `out-of-scope` | Issue lacks any of the `-RegressionLabels` (used for sanity checks, not normally surfaced) |
| `needs-human-review` | Evidence is contradictory or weak (e.g. cross-reference only mentions the issue) |

## CI Status Categories

| CI verdict | Meaning |
|------------|---------|
| `green` | Latest build on SR HEAD succeeded across all pipelines |
| `red-known-flakes` | Failures match historical fixture-flake patterns (Appium `OneTimeSetUp`, etc.) |
| `red-new-failures` | Failures don't match known flake patterns — manual investigation needed |
| `stale` | Latest build is older than the current SR HEAD — must re-run before judging |
| `unknown` | Couldn't query pipeline (auth / outage) |

## Methodology

Three critical gotchas this skill encodes — see [references/methodology.md](references/methodology.md) for the full discussion:

1. **Cherry-pick number swap**: SR backports get NEW PR numbers (e.g. main #35356 → SR7 #35428). Cannot naively grep source PR numbers; must walk SR-only commits and extract refs from commit bodies.

2. **Timeline cross-references**: `closedByPullRequestsReferences` returns empty for most MAUI issues. The skill walks `gh api repos/.../issues/N/timeline` filtering on `cross-referenced` events.

3. **Forward-flow / non-main merges**: A fix can merge into `inflight/current` only, not `main` (real example: PR #35609). The skill checks `git merge-base --is-ancestor $mergeCommit origin/main` before claiming a fix is "on main, just needs backport".

## Integration

- **Custom agent**: `.github/agents/release-readiness-agent.agent.md` wraps this skill — handles regression-label confirmation, runs the script, then uses WorkIQ to add context for `rejected-from-sr` PRs.
- **WorkIQ**: NOT called from this script (PowerShell can't invoke MCP tools). The agent enriches the script's JSON output with WorkIQ context where needed.

## Anti-Patterns

> ❌ **Don't naively grep source PR numbers** in the SR git log. The backport PR number replaces the source PR number in the merge commit subject. Use `sr-source-prs.txt` (produced by this skill) instead.

> ❌ **Don't claim a fix is on `main` based on `pr-view --state MERGED`.** PRs can be merged into `inflight/current` only. The skill's `onMain` field is the authoritative check.

> ❌ **Don't trust issue-title similarity.** Two issues can have nearly identical titles and refer to different platform-specific regressions (e.g. #35313 is the Android version, #35326 is the iOS/Mac/Win version with a different fix path). Always filter by the `regressed-in-*` label, not by title.

> ❌ **Don't run with `-InferRegressionLabels` for automated workflows** without surfacing the inferred labels for confirmation. Label inference is brittle for non-standard SR cycles.

## Tests

```powershell
pwsh -NoProfile -File .github/skills/release-readiness/tests/Test-ReleaseReadiness.ps1
```

The test harness asserts the known-answer set from the SR7 readiness analysis:

- #35313 → `in-sr-active` (Android regression, fixed by #35356, backported as #35428)
- #35344 → `in-sr-active` (Android scroll perf regression; primary fix #35379's backport #35442 was closed unmerged, but follow-on SafeArea fix #35664 was backported as #35693 — script correctly surfaces the partial fix that actually shipped)
- #35326 → `out-of-scope` (NOT labeled `regressed-in-10.0.60`)
- #35771 → `no-fix-yet` (10.0.70 regression, no fix PR exists)
