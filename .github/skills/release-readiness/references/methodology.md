# Release Readiness — Methodology

This document captures the deterministic algorithms used by the SR and Preview readiness engines, including **the three gotchas** discovered through real SR analysis and the Preview **consumer-installability gate**.

## Gotcha #1: Cherry-Pick Number Swap

### The trap

It's tempting to "verify a fix is in SR" by grepping for the source PR number in the SR branch's git log:

```bash
git log origin/release/10.0.1xx-sr7 --grep="35356"  # ❌ WRONG
```

This **misses** the most common case: the fix was backported. MAUI's backport workflow produces a NEW PR (e.g. `#35428`) whose merge commit on SR has the subject:

```
[release/10.0.1xx-sr7] [Android] Fix CollectionView ScrollTo(0) IsGrouped (#35428)
```

The source PR number (#35356) only appears in the *body* of the backport PR (typically as `Backport of #35356`).

### The fix

Walk SR-only commits and extract ALL `#NNNN` references from BOTH subject AND body:

```bash
git log --format='%H' origin/release/10.0.1xx-sr7 \
  ^origin/inflight/current ^origin/main
```

For each commit, parse:
- Subject `(#NNNN)` suffix → backport PR number
- Body `Backport of #NNNN` / `Cherry-picked from #NNNN` / `from PR #NNNN` → source PR number
- Body `Fixes #NNNN` / `Closes #NNNN` → fixed issue number
- Body `cherry picked from commit <sha>` → original SHA on main

The skill emits a deliberately **greedy** `sourcePrs` list that includes both backport and source PR numbers. A lookup `grep -qxF $prNum sr-source-prs.txt` then succeeds for either form.

### Confidence ladder

| Signal | Confidence |
|--------|-----------|
| `cherry picked from commit <sha>` in body | **high** — git apply pedigree, traceable to source commit |
| `Backport of #NNNN` / `[release/...] ... (#NNNN)` subject + body match | **high** |
| Bare `#NNNN` mention in commit body | **medium** — may be unrelated issue ref |
| Subject contains `Revert` | **revert** — handled separately |

## Gotcha #2: Empty `closedByPullRequestsReferences`

### The trap

GitHub's GraphQL `closedByPullRequestsReferences` field returns empty for most MAUI issues, even when a PR clearly "Fixes #N" in its body. The link only gets populated by a specific merge-time event flow that often doesn't fire.

```bash
gh issue view 35313 --json closedByPullRequestsReferences   # ❌ often empty
```

### The fix

Use the issue *timeline* API and filter `cross-referenced` events:

```bash
gh api repos/dotnet/maui/issues/35313/timeline --paginate \
  | jq '.[] | select(.event=="cross-referenced"
                     and .source.type=="issue"
                     and .source.issue.pull_request != null)
            | {pr: .source.issue.number,
               title: .source.issue.title,
               state: .source.issue.state}'
```

### Evidence weighting

A cross-reference alone is **insufficient** — anyone can mention an issue. Weight cross-referenced PRs by the strength of their link to the issue:

| Evidence type | Strength | Detection |
|--------------|----------|-----------|
| `closing-keyword` | **high** | PR body or commit message contains `Fixes #N`, `Closes #N`, `Resolves #N` |
| `explicit-backport` | **high** | PR title prefixed `[release/...]` AND body mentions source PR |
| `linked-via-comment` | **medium** | Issue comment links to PR (often added by maintainer) |
| `mentions-only` | **low** | PR body mentions issue without closing keyword |

Only `high` evidence produces an automatic classification; `medium`/`low` falls into `needs-human-review`.

## Gotcha #3: Forward-Flow / Non-Main Merges

### The trap

A PR shows `state: MERGED` and a maintainer might assume the fix is on `main` and just needs a backport. But MAUI uses multiple long-lived branches:

- `main` — current stable / shipped line
- `inflight/current` — next iteration (post-SR)
- `release/10.0.1xx-srN` — current SR

A PR can merge into `inflight/current` ONLY, bypassing `main` entirely (real example: PR #35609 merged on 2026-06-01, base = `inflight/current`).

```bash
gh pr view 35609 --json baseRefName,mergedAt,mergeCommit
# baseRefName: "inflight/current"   ← NOT main!
```

### The fix

Don't trust `state: MERGED`. Resolve the merge commit and check ancestry against `main`:

```bash
mergeSha=$(gh pr view $pr --json mergeCommit | jq -r .mergeCommit.oid)
git merge-base --is-ancestor "$mergeSha" origin/main && echo "on main" || echo "NOT on main"
git merge-base --is-ancestor "$mergeSha" origin/inflight/current && echo "on inflight"
git merge-base --is-ancestor "$mergeSha" origin/release/10.0.1xx-sr7 && echo "on SR"
```

The skill records `onMain`, `onInflight`, `onSr` independently. A PR can be merged-and-on-main, merged-but-only-on-inflight, or merged-and-on-SR (via direct merge or backport).

## Revert Detection

A fix can land on SR and then be **reverted** later in the same SR window — e.g. PR #35744 was backported to SR7 then reverted via a `[Revert]` commit. A naive "is the PR in SR?" check would falsely report "in SR" while the user effectively ships without the fix.

### Algorithm

For each SR-only commit, detect revert intent:

```
isRevert = subject.startsWith("Revert ")
        || subject.contains("[Revert]")
        || body.contains("This reverts commit <sha>")
```

For each revert commit, extract:
- The `revertsCommit` SHA from `This reverts commit <sha>.`
- The `revertsPr` number from an explicit `Revert PR #NNNN`, or the `(#NNNN)` **inside the quoted original title** (`Revert "Original title (#1234)" (#5678)` → `1234`, never the revert's own trailing `(#5678)`); the reverted commit's SHA subject is the authoritative override when available

Then build a `reverts` map: `{sourcePr → revertCommit}`. A PR classified as `in-sr` becomes `in-sr-reverted` if its source PR appears as a key in `reverts`.

### Ordering matters

Verify the revert happened **after** the original landing on SR:

```
git log --topo-order origin/release/10.0.1xx-sr7
```

A revert from SR's `git log` ordered before the fix would actually mean "the fix never landed."

## Regression Label Inference

### When `-InferRegressionLabels` is set

The skill must derive which `regressed-in-X.Y.Z` labels matter for a given SR:

1. List all existing labels matching `^regressed-in-(\d+)\.(\d+)\.(\d+)$`
2. Filter to the major.minor family implied by `$SrBranch` (e.g. `release/10.0.1xx-sr7` → 10.0.\*)
3. Sort descending by patch version
4. Take the top N labels whose patch < the SR's patch
   - Heuristic: SR N is built from minor versions released since SR (N-1). For 10.0 family, each SR roughly covers 2 minor version bumps → take top 2 labels.
5. Emit `labelInferenceMode: inferred` + `confidence: medium` so callers know to confirm

**Why this is brittle**: SR cycles can skip patches, repeat patches (hotfix), or be triggered by a single late-cycle regression. The agent **must** show inferred labels to the user before treating them as authoritative.

## Classification Matrix (Full)

| Verdict | Detection rules (in order, first match wins) |
|---------|----------------------------------------------|
| `in-sr-reverted` | Source PR's commit on SR is reverted by a later revert commit |
| `in-sr-active` | Source PR number ∈ SR `sourcePrs` AND not reverted |
| `rejected-from-sr` | A backport PR targeting `$SrBranch` exists, state=CLOSED, merged=false |
| `backport-in-progress` | A backport PR targeting `$SrBranch` exists, state=OPEN |
| `merged-non-main-only` | Fix PR state=MERGED, `onMain=false`, `onInflight=true` |
| `merged-on-main-no-backport` | Fix PR state=MERGED, `onMain=true`, no backport PR to `$SrBranch` exists |
| `open-on-main` | Fix PR state=OPEN, base=main |
| `no-fix-yet` | No cross-referenced PR with high-confidence evidence found |
| `closed-fix-unlinked` | `no-fix-yet` would apply, BUT the issue is CLOSED and a closing comment **explicitly names** a fix PR (fix/resolve/close language) that is MERGED and whose commit is on `$SrBranch` (verified by SHA-ancestry OR the `(#<num>)` squash-subject token). A bare PR mention (the cause-PR blame pattern) is rejected. Surfaces a missing PR↔issue link rather than a false "no fix" alarm. Non-blocking (Tier 3) |
| `needs-human-review` | Only weak evidence; OR multiple candidate PRs with conflicting verdicts |

## Preview Consumer Installability

A green Preview branch or promoted SDK build does not prove that a customer can
install the exact workload set from a clean machine. The gate in
`PreviewInstallability.ps1` evaluates package identity, branch pins, source
availability, and platform prerequisites before contributing to the Preview
verdict.

### Inputs and Trust Boundaries

The evaluator uses:

1. Branch pins for the VMR/runtime SDK, Android, and Apple. The VMR pin also
   anchors Mono toolchain and Emscripten; MAUI is checked against the target
   Preview train because the MAUI branch does not statically pin its own build
   version.
2. Public NuGet v3 sources derived from the SDK major version.
3. An optional release-owner-confirmed workload-set CLI version.
4. Optional additional HTTPS package sources in `name=https://...` form.

Only a release owner can identify the blessed workload-set version. Discovery
may find several coherent candidates, but "newest coherent" does not mean
"approved for release." An unconfirmed version therefore cannot produce
`READY`.

Additional sources are accepted only for dnceng Azure Artifacts HTTPS
endpoints. NuGet.org is already part of the fixed public source set and cannot
be supplied under an arbitrary credential-bearing name. User information
embedded in a URL is rejected. Credentials come from NuGet's
`NuGetPackageSourceCredentials_<name>` environment variable, where `<name>`
exactly matches the source name. The value must contain non-empty `Username`
and `Password` fields and `ValidAuthenticationTypes=Basic`.

### Workload-Set Version Normalization

The workload-set CLI version and NuGet package version differ:

```text
CLI:   <major>.0.<feature-band>-preview.<N>.<build>
NuGet: <major>.<feature-band>.0-preview.<N>.<build>
```

For example:

```text
11.0.100-preview.6.26363.2
    -> 11.100.0-preview.6.26363.2
```

The package ID is derived from the SDK feature band and Preview number:

```text
Microsoft.NET.Workloads.<major>.0.<feature-band>-preview.<N>
```

MSI packages are excluded from discovery because they are installer artifacts,
not workload-set metadata packages.

### Source Roles and Isolation

The evaluator builds a clean source list instead of inheriting machine-wide
NuGet configuration:

| Source role | Expected contents |
|-------------|-------------------|
| `dotnet-workloads` | Workload-set metadata package |
| `dotnet<major>-workloads` | Android and other platform manifest assets |
| `dotnet<major>` | MAUI and Apple manifests and packs |
| `dotnet<major>-transport` | Runtime transport packs |
| `dotnet-public` | Shared dotnet dependencies |
| NuGet.org | Public ecosystem dependencies |
| Additional authenticated source | Release-specific assets not yet present on public feeds |

The generated local NuGet configuration starts with `<clear />`. This prevents
an old major-version or stale shipping feed from making an installation appear
to work accidentally.

### Evaluation Algorithm

1. Derive the SDK major, feature band, Preview number, package ID, CLI version,
   and normalized NuGet version.
2. If no version is confirmed, discover prerelease candidates from accessible
   sources as diagnostic evidence and return `unknown`.
3. Download and extract the exact confirmed workload-set package.
4. Compare its Android, Apple, runtime/VMR, and Emscripten versions with the
   branch pins, and verify that its MAUI manifest targets the expected Preview
   train.
5. Probe every required manifest package.
6. Probe representative Android, Apple, MAUI, and runtime transport packs.
7. Extract the JDK, Android SDK, Xcode, Apple SDK, and Windows App SDK
   prerequisites when present.
8. Emit an isolated local NuGet configuration and exact install command only
   when public-safe mode is disabled.

Package probes distinguish absence from unavailable evidence:

- A not-found response from all accessible sources can establish `missing`.
- HTTP 401/403, an inaccessible authenticated source, timeout, malformed
  metadata, or another read failure establishes only `unknown`.
- A package found on one supplied source is available even if another source is
  inaccessible.

### State and Verdict Mapping

| Gate state | Readiness check | Rule |
|------------|-----------------|------|
| `installable` | `READY` | The version is confirmed, pins agree, and all required manifests and representative packs resolve. |
| `missing` | `BLOCKED` | A confirmed package or asset is absent from every accessible supplied source. |
| `mismatched` | `BLOCKED` | Workload-set component versions disagree with branch pins or the target MAUI Preview train. |
| `unknown` | `UNKNOWN` | Confirmation or trustworthy package evidence is unavailable. |

`BLOCKED` prevents readiness; `UNKNOWN` prevents an unconditional `READY`.

### Public-Output Safety

Local release-captain output may contain the exact source list, generated NuGet
configuration, resolved-source diagnostics, and install command. Public-safe
serialization recursively removes:

- additional source names and URLs
- private source metadata, including nested resolved-source objects
- generated NuGet configuration paths and content
- authentication state and credential details
- local installation commands that reference private sources

The public report retains the state, safe package identities and versions, pin
comparison, prerequisite summary, and remediation category needed to explain
the readiness result.

## CI Freshness

A passing CI build is only meaningful if it ran **at or after** the current SR HEAD. The skill records:

```json
"latestBuild": {
  "sourceSha": "...",
  "isAtOrAheadOfSrHead": true|false,
  "completedAt": "..."
}
```

If `isAtOrAheadOfSrHead=false`, the pipeline verdict is `stale` regardless of result. The user must re-run before judging.

## Why no WorkIQ in the script

WorkIQ is an MCP tool only available to the agent, not to PowerShell scripts. The script's job is to identify **which** PRs need WorkIQ context (e.g. all `rejected-from-sr` PRs); the agent enriches the JSON output by calling WorkIQ and adding `workIqContext` fields.

This keeps the script reproducible (any user can run it deterministically) and concentrates judgment work where the LLM can apply it.
