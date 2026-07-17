# Release Readiness — Methodology

This document captures the algorithms used by `Get-ReleaseReadiness.ps1` and **the seven gotchas** discovered through real SR analysis that the algorithms exist to prevent.

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

## Gotcha #4: Source-to-SR Backport Workflow

### The trap

Do not treat a merged PR or an open SR fix branch as a backport candidate. A backport starts from a **merged source PR whose merge commit is on `main`**. Triggering the automation before that point is skipped; triggering it for `merged-non-main-only` bypasses the required mainline fix.

### The automated path

Confirm all of these first:

1. Source PR state is `MERGED`.
2. Its merge commit is an ancestor of `origin/main`.
3. No OPEN or MERGED backport PR already targets the SR branch.

Post this exact comment on the merged source PR:

```text
/backport to release/<major>.0.1xx-sr<N>
```

`.github/workflows/backport.yml` delegates this operation to Arcade. A successful run creates a branch named `backport/pr-<source-pr>-to-release/<major>.0.1xx-sr<N>` and an SR-targeted PR whose body identifies `Backport of #<source-pr>`. Its cherry-pick commit preserves source pedigree with `(cherry picked from commit <source-merge-sha>)`.

### Automation conflict fallback

If the workflow cannot apply the change cleanly, use the source PR's **merge commit** — not its head-branch tip — to prepare a manual backport:

```bash
git fetch origin
git switch -c backport/pr-<source-pr>-to-release/<major>.0.1xx-sr<N> \
  origin/release/<major>.0.1xx-sr<N>
git cherry-pick -x <source-merge-sha>
# Resolve any conflicts, then test the resolved behavior.
git push -u origin HEAD
```

Open the resulting PR against the SR branch, keep the generated branch/title convention, and state `Backport of #<source-pr>` in its body. After it merges, re-run readiness so the regression is classified from the SR contents rather than the source PR's state.

## Gotcha #5: Servicing Version Flip

### The trap

A release branch cut from `main` normally retains CI versioning. Green CI does
not prove the branch will produce stable packages: it must use
`PreReleaseVersionLabel=servicing` and `StabilizePackageVersion=true`.

Every .NET 10 SR1–SR8 finished with both values. SR1–SR7 used an explicit
flip PR; SR8 inherited the same diff through its catch-up merge from SR7:

| SR | Transition |
|----|------------|
| SR1–SR7 | Dedicated servicing flip PRs: #32433, #33057, #33520, #34001, #34371, #34942, #35520 |
| SR8 | Catch-up merge #35810 preserved PatchVersion 80 and inherited the flip |

### The workflow

After the last required content backport, open a focused PR **targeting the
SR branch**. Keep `PatchVersion` unchanged and make only this versioning
transition:

```diff
- <PreReleaseVersionLabel>ci.main</PreReleaseVersionLabel>
- <PreReleaseVersionLabel Condition="'$(BUILD_SOURCEBRANCH)' == 'refs/heads/inflight/current'">ci.inflight</PreReleaseVersionLabel>
+ <PreReleaseVersionLabel>servicing</PreReleaseVersionLabel>
...
- <StabilizePackageVersion Condition="'$(StabilizePackageVersion)' == ''">false</StabilizePackageVersion>
+ <StabilizePackageVersion Condition="'$(StabilizePackageVersion)' == ''">true</StabilizePackageVersion>
```

Keep the `main` version bump in its own `main` PR. After the servicing PR
merges, rerun final CI at the new SR HEAD. The report is advisory only: it
must recommend this PR workflow, never edit a release branch directly.

## Gotcha #6: Next-Cycle Main PatchVersion Bump

### The trap

Cutting an SR branch does not automatically move `main` to the next release
cycle. If both refs keep the same `PatchVersion`, new work merged to `main`
continues to identify itself as part of the SR that is about to ship.

Do not solve this by copying the release branch's servicing settings back to
`main`. The two transitions are independent:

- The SR branch keeps its current `PatchVersion` and switches to
  `servicing`/stable package production.
- `main` keeps `ci.main`/prerelease package production and advances only its
  `PatchVersion`.

### Historical pattern

The .NET 10 transitions were focused, one-file, one-line PRs:

| Preparing | PR | `PatchVersion` |
|-----------|----|----------------|
| SR7 | #34943 | `60` → `70` |
| SR8 | #35433 | `70` → `80` |
| SR9 | #35879 | `80` → `90` |

`SdkBandVersion` remained `10.0.100`, and the mainline prerelease settings
were unchanged in each PR.

### The workflow

After `release/<major>.0.1xx-sr<N>` is cut and before SR<N> ships:

1. Open a focused PR targeting `main`.
2. Change only `eng/Versions.props`:

   ```diff
   - <PatchVersion><current></PatchVersion>
   + <PatchVersion><(N+1)*10></PatchVersion>
   ```

3. Title it `Update PatchVersion from <current> to <(N+1)*10>`.
4. Keep `SdkBandVersion`, `PreReleaseVersionLabel=ci.main`, and
   `StabilizePackageVersion=false` unchanged.
5. Keep the PR separate from the SR servicing-flip PR and merge it before
   shipping the current SR.

For SR9 → SR10, that means changing only
`<PatchVersion>90</PatchVersion>` to
`<PatchVersion>100</PatchVersion>` with title
`Update PatchVersion from 90 to 100`.

The readiness report must emit these exact instructions when the check is
blocked. It remains report-only and must not edit or push `main`.

## Gotcha #7: Default-Channel → Per-Build Feed → Ship Assessment

### The trap

The DevDiv ship **Assessment** (an Azure DevOps "Assessment" work item) must
link the **per-build NuGet feed** so CSI and customers can validate the exact
candidate packages before the release ships. That feed —
`https://pkgs.dev.azure.com/dnceng/public/_packaging/darc-pub-dotnet-maui-<sha8>/nuget/v3/index.json`
(`<sha8>` = the build's commit short SHA) — is **only generated when the build
is promoted to a channel**. Promotion requires a **BAR default-channel
mapping** for the SR branch (Gotcha checks emit this as `BAR default-channel
mapping`).

If the branch has no default channel, the build is never promoted, no per-build
feed is generated, and the Assessment gets created **without** the validation
feed — exactly what happened for SR9: the assessment shipped incomplete and CSI
had no feed to validate against. CI stays green throughout, so nothing else
catches it.

### Real example (SR9)

- SR9 branch `release/10.0.1xx-sr9` was cut **without** a default channel.
- Build `20260710.6` (BAR **322419** / AzDO **3019432**) was **not promoted** →
  no per-build feed → the Assessment was created without a feed link.
- Fixed by `darc add-default-channel --repo https://github.com/dotnet/maui
  --branch release/10.0.1xx-sr9 --channel ".NET 10.0.1xx SDK"` (maestro-config
  PR) and re-promoting the build. `darc get-asset --name
  Microsoft.Maui.Controls --build 322419` then showed channel `.NET 10.0.1xx
  SDK` and feed `darc-pub-dotnet-maui-8e2547a4`.
- SR8 for reference used feed `darc-pub-dotnet-maui-bf615689` — same
  `darc-pub-dotnet-maui-<sha8>` pattern.

### The workflow

1. Ensure the SR branch has a BAR default-channel mapping (the
   `BAR default-channel mapping` check). Without it, escalate to release
   engineering: `darc add-default-channel --channel ".NET <band> SDK" --branch
   release/<major>.0.1xx-sr<N> --repo https://github.com/dotnet/maui`.
2. Ensure the SR HEAD build is **promoted** to that channel. The
   `Ship Assessment validation feed` check reports `READY` (with the derived
   feed URL) once the build carries a channel, or `WATCH` while it is
   unpromoted.
3. Confirm the feed location with `darc get-asset --name
   Microsoft.Maui.Controls --build <BAR id>` (prints the `NugetFeed` location).
4. **Paste that feed URL into the DevDiv ship Assessment.**

The report derives and surfaces the feed URL but remains report-only — it does
not create channels, promote builds, or edit the Assessment.

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
