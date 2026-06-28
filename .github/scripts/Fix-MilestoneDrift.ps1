#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Detects and fixes milestone drift for dotnet/maui releases.

.DESCRIPTION
    When PRs merge to inflight/current, they may get milestoned prematurely.
    The actual release they ship in depends on which Candidate PR carries them
    to main and which SR branch cut includes that commit. This script detects
    and reports (or optionally fixes) milestone mismatches.

    Modes:
    1. Single PR:   -PrNumber 33818 [-Tag 10.0.50]
    2. Single tag:  -Tag 10.0.50 [-PreviousTag 10.0.41]

    Safety: PRs merged before a cutoff date are always skipped. The cutoff
    defaults to 2026-01-01 (when this automation went live) and is configurable
    via -MergedAfter, so an older release can be processed deliberately.

.PARAMETER PrNumber
    Analyze and fix a single PR (and its linked issues).

.PARAMETER Tag
    Release tag to analyze (e.g., "10.0.50"). For single-PR mode, auto-detected if omitted.

.PARAMETER PreviousTag
    Previous release tag. Auto-detected if omitted.

.PARAMETER RepoPath
    Path to a dotnet/maui git checkout with full tag history.

.PARAMETER Output
    Output JSON file path.

.PARAMETER MergedAfter
    Cutoff date: PRs merged strictly before this date are skipped (never
    milestoned or closed). Defaults to 2026-01-01 (when this automation went
    live) so the bulk -Apply / -CloseFixedIssues path can't reach back and
    rewrite milestones for PRs that predate it. Override to deliberately process
    an older release — e.g. -MergedAfter '2025-01-01' to close linked issues for
    a historical SR. Accepts any parseable date (e.g. '2025-01-01' or
    '2025-06-01T00:00:00Z'); no-timezone values are treated as UTC.

.PARAMETER Apply
    Actually apply milestone fixes. Without this flag, only a dry-run report is produced.

.PARAMETER CreateIssue
    Create a GitHub issue in dotnet/maui with the milestone drift report.

.PARAMETER CloseFixedIssues
    Close issues that are referenced as fixed by the analyzed PR(s). Use this for
    PRs merged to a non-default branch (e.g. net11.0, release/*) because GitHub
    only auto-closes linked issues for PRs merged to the default branch (main).
    Honors -Apply (dry-run when -Apply is not set).

.EXAMPLE
    ./Fix-MilestoneDrift.ps1 -PrNumber 33818 -RepoPath ~/Projects/maui -Verbose
    ./Fix-MilestoneDrift.ps1 -PrNumber 33818 -Apply
    ./Fix-MilestoneDrift.ps1 -Tag 10.0.50 -RepoPath ~/Projects/maui
    # Process a historical SR (closing linked issues) by lowering the cutoff:
    ./Fix-MilestoneDrift.ps1 -Tag 9.0.90 -MergedAfter '2024-01-01' -Apply -CloseFixedIssues
#>

[CmdletBinding()]
param(
    [int]$PrNumber,
    [string]$Tag,
    [string]$PreviousTag,
    [string]$RepoPath = ".",
    [string]$Output,
    [string]$MergedAfter,
    [switch]$Apply,
    [switch]$CreateIssue,
    [switch]$CloseFixedIssues
)

# Resolve the "merged after" safety cutoff. PRs merged strictly before this
# date are always skipped, so the bulk -Apply / -CloseFixedIssues path can never
# reach back and rewrite milestones for PRs that predate this automation.
# Defaults to 2026-01-01 (go-live); override via -MergedAfter to deliberately
# process an older release. Pure + side-effect-free so it can be unit tested.
function Resolve-MergedAfterCutoff {
    param([string]$Value)
    if ([string]::IsNullOrWhiteSpace($Value)) {
        return [datetime]::new(2026, 1, 1, 0, 0, 0, [System.DateTimeKind]::Utc)
    }
    try {
        return [datetime]::Parse(
            $Value,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [System.Globalization.DateTimeStyles]::AssumeUniversal -bor [System.Globalization.DateTimeStyles]::AdjustToUniversal)
    } catch {
        throw "Invalid -MergedAfter value '$Value'. Expected a date such as '2025-01-01' or '2025-06-01T00:00:00Z'."
    }
}

$script:MergedAfterCutoff = Resolve-MergedAfterCutoff $MergedAfter

# Only enable StrictMode during normal execution — not when dot-sourced for testing,
# since StrictMode leaks into the caller scope and can break Pester or other scripts.
if ($MyInvocation.InvocationName -ne '.') {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Stop"
}

# Import shared MAUI release versioning helpers. Pulls in:
#   Get-CurrentMajorVersion, Get-MainBranchForVersion, Get-VersionFromGitRef,
#   ConvertTo-Milestone, ConvertBranchToMilestone, Get-TagSortKey, Find-PreviousTag
# Module uses Set-StrictMode internally; importing it does not leak strict mode
# to this script's caller (unlike dot-sourcing), which is why the conditional
# StrictMode dance above is still needed for Pester compatibility.
Import-Module (Join-Path $PSScriptRoot 'shared/MauiReleaseVersioning.psm1') -Force

#region ── Milestone mapping helpers ──────────────────────────────────────

function Get-PatchVersion([string]$ReleaseTag) {
    if ($ReleaseTag -match '^(\d+)\.0\.(\d+)$') { return [int]$Matches[2] }
    return 0
}

function Test-IsReleaseTag([string]$ReleaseTag, [int]$Major) {
    # Matches stable tags (10.0.50) and preview/RC tags (11.0.0-preview.3.26203.7)
    return ($ReleaseTag -match "^$Major\.0\.")
}

function Test-MilestoneMatch([string]$Actual, [string]$Expected) {
    <# Handles ".NET 10.0 SR4" vs ".NET 10 SR4" and ".NET 10.0 GA" vs ".NET 10 GA" normalization.
       Sub-patches like ".NET 10 SR4.1" are distinct milestones and do NOT match ".NET 10 SR4". #>
    if ([string]::IsNullOrEmpty($Actual)) { return $false }
    if ($Actual -eq $Expected) { return $true }

    # Normalize: ".NET 10.0 SRx" → ".NET 10 SRx" and ".NET 10.0 GA" → ".NET 10 GA"
    $normActual   = $Actual   -replace '\.NET (\d+)\.0 (SR|GA)', '.NET $1 $2'
    $normExpected = $Expected -replace '\.NET (\d+)\.0 (SR|GA)', '.NET $1 $2'
    if ($normActual -eq $normExpected) { return $true }

    return $false
}

function Find-MatchingMilestone([string]$Expected, [hashtable]$AllMilestones) {
    <# Returns @{Title; Number} or $null #>
    if ($AllMilestones.ContainsKey($Expected)) {
        return @{ Title = $Expected; Number = $AllMilestones[$Expected] }
    }
    # Normalized search (handles ".NET 10.0 SRx" ↔ ".NET 10 SRx" and ".NET 10.0 GA" ↔ ".NET 10 GA")
    $normExpected = $Expected -replace '\.NET (\d+)\.0 (SR|GA)', '.NET $1 $2'
    foreach ($key in $AllMilestones.Keys) {
        $normKey = $key -replace '\.NET (\d+)\.0 (SR|GA)', '.NET $1 $2'
        if ($normKey -eq $normExpected) {
            return @{ Title = $key; Number = $AllMilestones[$key] }
        }
    }
    return $null
}

function Test-PrBelongsToVersion([string]$BaseRef, [string]$MainBranch, [int]$Major) {
    <# Checks if a PR's base branch is compatible with the version being analyzed.
       Prevents merge-up commits from causing incorrect milestoning.
       E.g. when main (.NET 10) merges into net11.0, the .NET 10 PRs should not
       be milestoned as .NET 11. #>
    if ([string]::IsNullOrEmpty($BaseRef)) { return $true }

    # PR targets the main branch for this version — always allowed
    if ($BaseRef -eq $MainBranch) { return $true }

    # Allow inflight/darc branches only if they feed into this version's main branch
    if ($BaseRef -match '^(inflight|darc)/') {
        # inflight/* and darc/* branches feed into 'main', so only allow when MainBranch is 'main'
        return ($MainBranch -eq 'main')
    }

    # Release branches: only allow if they match this major version
    if ($BaseRef -match '^release/(\d+)\.') {
        return ([int]$Matches[1] -eq $Major)
    }

    # Reject PRs explicitly targeting a different .NET version branch (net11.0, net12.0, etc.)
    if ($BaseRef -match '^net(\d+)\.\d+$') {
        return ([int]$Matches[1] -eq $Major)
    }

    # Reject 'main' when we're analyzing a non-main version (e.g. .NET 11 on net11.0)
    # PRs targeting 'main' belong to the .NET version that lives on main, not this one.
    if ($BaseRef -eq 'main' -and $MainBranch -ne 'main') {
        return $false
    }

    # Unknown branches (feature/*, etc.) — allow
    return $true
}

#endregion

#region ── Git helpers ────────────────────────────────────────────────────

function Get-AllTags([string]$Repo) {
    $output = git -C $Repo --no-pager tag -l 2>&1
    if ($LASTEXITCODE -ne 0) { throw "git tag failed: $output" }
    return ($output -split "`n" | Where-Object { $_ })
}

function Test-CommitInTag([string]$CommitSha, [string]$Tag, [string]$Repo) {
    <# Returns $true if $CommitSha is an ancestor of (i.e. contained in) $Tag.
       git merge-base --is-ancestor exits 0 (ancestor), 1 (NOT ancestor), or >1
       for a real failure (bad/unknown object, shallow clone missing the object,
       tag not fetched). Only 0 and 1 are valid answers; a higher exit code is a
       git error, NOT proof of non-containment, so we throw rather than silently
       reporting $false — otherwise a transient failure on the earliest containing
       tag would skip it and mislabel the milestone. Callers that prefer precision
       over hard-failing catch this and fall back to the coarser branch milestone.
       Extracted into its own function so it can be mocked in unit tests. #>
    $output = git -C $Repo merge-base --is-ancestor $CommitSha $Tag 2>&1
    if ($LASTEXITCODE -gt 1) {
        throw "git merge-base --is-ancestor failed (exit $LASTEXITCODE) for '$CommitSha' in '$Tag': $output"
    }
    return ($LASTEXITCODE -eq 0)
}

function Get-OnBranchShaFromLog([string[]]$LogLines, [int]$PrNum) {
    <# Selects the on-branch commit SHA for a squash-merged/cherry-picked PR from
       `git log --format='%H%x1f%s'` output (full SHA, US 0x1f separator, subject).

       Only a commit whose SUBJECT ENDS with the squash-merge token "(#$PrNum)" is
       accepted. This is the crux of the fallback's correctness: GitHub squash and
       cherry-pick subjects place the PR token at the END ("Some title (#$PrNum)"),
       so matching the trailing token rejects two whole classes of false positives
       that a raw --grep over the full message would let through:
         * body-only mentions  — "Workaround until (#$PrNum) lands" (token in body,
           not the subject)  → subject doesn't end with the token → ignored.
         * quoted reverts      — 'Revert "Fix X (#$PrNum)" (#OTHER)' when searching
           for #$PrNum         → trailing token is (#OTHER), not (#$PrNum) → ignored.
       Input is expected oldest-first (git --reverse), so the FIRST genuine match is
       the original introduction, not a later re-mention. #>
    foreach ($line in $LogLines) {
        if ($line -match "^([0-9a-f]{40})\x1f.*\(#$PrNum\)\s*$") {
            return $Matches[1]
        }
    }
    return $null
}

function Get-PrNumbersBetweenTags([string]$TagFrom, [string]$TagTo, [string]$Repo) {
    $output = git -C $Repo --no-pager log --oneline "$TagFrom..$TagTo" 2>&1
    if ($LASTEXITCODE -ne 0) { throw "git log failed: $output" }
    $prs = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($line in ($output -split "`n")) {
        foreach ($m in [regex]::Matches($line, '\(#(\d+)\)')) {
            [void]$prs.Add([int]$m.Groups[1].Value)
        }
    }
    return ($prs | Sort-Object)
}

function Get-PrNumbersReachableFromTag([string]$TagName, [string]$Repo) {
    <# Returns PR numbers reachable from a tag (all commits up to and including the tag). #>
    $output = git -C $Repo --no-pager log --oneline $TagName 2>&1
    if ($LASTEXITCODE -ne 0) { throw "git log failed: $output" }
    $prs = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($line in ($output -split "`n")) {
        foreach ($m in [regex]::Matches($line, '\(#(\d+)\)')) {
            [void]$prs.Add([int]$m.Groups[1].Value)
        }
    }
    return ($prs | Sort-Object)
}

function Find-TagContainingPr([int]$PrNum, [string]$Repo, [int]$Major) {
    <# Searches tag ranges to find which release contains this PR.
       Handles GA (first tag) by searching all reachable commits. #>
    $allTags = Get-AllTags $Repo
    $srTags = $allTags | Where-Object { Test-IsReleaseTag $_ $Major } |
              Sort-Object { Get-TagSortKey $_ }

    for ($i = 0; $i -lt $srTags.Count; $i++) {
        $current = $srTags[$i]

        if ($i -eq 0) {
            # First tag (e.g. GA) — search all commits reachable from it
            $prsInRange = Get-PrNumbersReachableFromTag $current $Repo
        } else {
            $prev = $srTags[$i - 1]
            $prsInRange = Get-PrNumbersBetweenTags $prev $current $Repo
        }

        if ($PrNum -in $prsInRange) {
            $previousTag = if ($i -gt 0) { $srTags[$i - 1] } else { $null }
            return @{ Tag = $current; PreviousTag = $previousTag }
        }
    }
    return $null
}

function Get-RefinedReleaseMilestone([string]$BranchMilestone, [string]$CommitSha, [string]$Repo) {
    <# Refines an SR branch milestone (e.g. ".NET 10 SR7") to the sub-patch the
       commit actually shipped in (e.g. ".NET 10 SR7.1").

       Why: an SR release branch (release/X.0.Yxx-srN) produces MULTIPLE servicing
       drops over its lifetime — 10.0.70 (SR7), 10.0.71 (SR7.1), 10.0.72 (SR7.2), …
       ConvertBranchToMilestone only knows the branch name, so it always returns the
       BASE SR. A revert/hotfix that lands after the base SR shipped actually goes
       out in a later sub-patch, and milestoning it as the base SR is wrong (it can
       even DOWNGRADE an already-correct SR7.1 issue back to SR7).

       Rule (earliest release wins): a commit on the SR branch ships in the EARLIEST
       SR-family tag (X.0.{sr}{sub}) that contains it. If no family tag contains it
       yet (the drop hasn't been tagged), the base SR (and any earlier sub-patches)
       are already shipped, so the commit goes out in the NEXT sub-patch after the
       latest shipped family tag — provided that next sub-patch is still within THIS
       SR family (the SR7 branch only ever ships 10.0.70..10.0.79).

       Non-SR milestones (preview/rc/GA) have no sub-patches and are returned as-is.
       The major version is taken from the milestone string itself (authoritative). #>
    if ([string]::IsNullOrWhiteSpace($BranchMilestone)) { return $BranchMilestone }
    if ([string]::IsNullOrWhiteSpace($CommitSha)) { return $BranchMilestone }
    if ($BranchMilestone -notmatch '^\.NET (\d+) SR(\d+)$') { return $BranchMilestone }
    $msMajor = [int]$Matches[1]
    $sr = [int]$Matches[2]

    # SR-family tags: X.0.{sr*10 .. sr*10+9} (e.g. SR7 → 10.0.70..10.0.79), ascending.
    # The whole tag scan is guarded: Test-CommitInTag throws on a real git error
    # (vs a clean "not an ancestor"), and rather than risk an incorrect sub-patch we
    # fall back to the coarser base SR milestone — never wrong, just less precise.
    try {
        $low = $sr * 10
        $high = $low + 9
        $familyTags = @(Get-AllTags $Repo | Where-Object {
            ($_ -match "^$msMajor\.0\.(\d+)$") -and ([int]$Matches[1] -ge $low) -and ([int]$Matches[1] -le $high)
        } | Sort-Object { Get-TagSortKey $_ })

        # Earliest family tag that contains the commit = the drop it shipped in.
        foreach ($tag in $familyTags) {
            if (Test-CommitInTag $CommitSha $tag $Repo) {
                $refined = ConvertTo-Milestone $tag
                if ($refined) { return $refined }
            }
        }

        # Not in any shipped family tag yet. If earlier drops already shipped, this
        # commit goes out in the next sub-patch after the latest one — but only while
        # that next sub-patch still belongs to THIS SR family (patch <= $high). Once the
        # family is exhausted (latest is X.0.{sr}9) the next patch would roll into the
        # NEXT SR, which this branch never ships, so fall back to the base SR rather than
        # silently crossing the family boundary (e.g. SR7 → SR8).
        if ($familyTags.Count -gt 0 -and $familyTags[-1] -match "^$msMajor\.0\.(\d+)$") {
            $nextPatch = [int]$Matches[1] + 1
            if ($nextPatch -le $high) {
                $refined = ConvertTo-Milestone "$msMajor.0.$nextPatch"
                if ($refined) { return $refined }
            }
        }
    }
    catch {
        Write-Warning "Get-RefinedReleaseMilestone: git error refining '$BranchMilestone' for '$CommitSha' — falling back to base milestone. $_"
        return $BranchMilestone
    }

    # No (further) family tags apply — still the base SR.
    return $BranchMilestone
}

function Find-ReleaseBranchForCommit([string]$CommitSha, [string]$Repo, [int]$Major, [int]$PrNum = 0) {
    <# Finds the earliest release branch containing a PR.
       First checks git ancestry (commit SHA). If that fails (rebase/cherry-pick
       changed the SHA), falls back to searching commit messages for the PR number.
       Checks in chronological order: previews → RCs → GA → SRs.
       The matched branch's milestone is refined to the sub-patch the commit shipped
       in (see Get-RefinedReleaseMilestone).
       Returns @{ Branch; Milestone } or $null. #>

    # Fetch all release branches for this major version
    $remoteBranches = git -C $Repo --no-pager ls-remote --heads origin "refs/heads/release/$Major.0.*" 2>&1
    if ($LASTEXITCODE -ne 0 -or -not $remoteBranches) { return $null }

    $branches = ($remoteBranches -split "`n") | ForEach-Object {
        if ($_ -match 'refs/heads/(.+)$') { $Matches[1] }
    } | Where-Object { $_ }

    # Sort by chronological release order within a major version:
    # preview1, preview2, ..., rc1, rc2, GA, sr1, sr2, ...
    # A commit in both preview7 and GA should be milestoned preview7
    # (that's where it first shipped).
    $sorted = $branches | Sort-Object {
        if ($_ -match '-preview(\d+)$') { return 100  + [int]$Matches[1] }
        if ($_ -match '-rc(\d+)$')      { return 200  + [int]$Matches[1] }
        if ($_ -match '-sr(\d+)$')      { return 1000 + [int]$Matches[1] }
        return 500  # GA (no suffix) — after previews/RCs, before SRs
    }

    # Check each branch: first by ancestry, then by PR number in commit messages.
    # The commit message approach handles rebases and cherry-picks where the SHA changes
    # but the PR number "(#NNNNN)" is preserved in the squash-merge message.
    foreach ($branch in $sorted) {
        $null = git -C $Repo fetch origin $branch --quiet 2>&1

        # Try ancestry first (fastest, exact match)
        $null = git -C $Repo merge-base --is-ancestor $CommitSha "origin/$branch" 2>&1
        if ($LASTEXITCODE -eq 0) {
            $milestone = ConvertBranchToMilestone $branch
            if ($milestone) {
                $milestone = Get-RefinedReleaseMilestone $milestone $CommitSha $Repo
                return @{ Branch = $branch; Milestone = $milestone }
            }
        }

        # Fall back to commit message search (handles rebase/cherry-pick).
        # Emit "%H<US>%s" (SHA, 0x1f separator, subject) and accept ONLY commits whose
        # SUBJECT ends with the squash token "(#$PrNum)" — see Get-OnBranchShaFromLog.
        # --grep is a coarse pre-filter over the whole message; the trailing-subject
        # check is what makes this precise, rejecting body-only mentions and quoted
        # reverts that would otherwise refine to the wrong sub-patch. --reverse yields
        # oldest-first so the original introduction wins, not a later re-mention.
        # stderr is discarded and only a 40-hex SHA is returned, so a git warning can
        # never be misread as the SHA.
        if ($PrNum -gt 0) {
            $grepResult = git -C $Repo --no-pager log "origin/$branch" --format='%H%x1f%s' --grep="(#$PrNum)" --reverse 2>$null
            $branchSha = Get-OnBranchShaFromLog @($grepResult) $PrNum
            if ($branchSha) {
                $milestone = ConvertBranchToMilestone $branch
                if ($milestone) {
                    $milestone = Get-RefinedReleaseMilestone $milestone $branchSha $Repo
                    Write-Verbose "  PR #$PrNum found via commit message on $branch (rebased/cherry-picked)"
                    return @{ Branch = $branch; Milestone = $milestone }
                }
            }
        }
    }
    return $null
}

#endregion

function Invoke-GhApi([string]$Endpoint) {
    $result = gh api $Endpoint 2>&1
    if ($LASTEXITCODE -ne 0) { throw "gh api $Endpoint failed: $result" }
    return ($result | ConvertFrom-Json)
}

function Get-AllMilestones {
    $milestones = @{}; $page = 1
    while ($true) {
        $data = Invoke-GhApi "repos/dotnet/maui/milestones?state=all&per_page=100&page=$page"
        foreach ($ms in $data) { $milestones[$ms.title] = $ms.number }
        if ($data.Count -lt 100) { break }
        $page++
    }
    return $milestones
}

function Get-PrInfo([int]$PrNum) {
    try {
        $pr = Invoke-GhApi "repos/dotnet/maui/pulls/$PrNum"
        # Safety: skip PRs merged before cutoff date
        if ($pr.merged_at) {
            # ConvertFrom-Json may return a DateTime or a string depending on PS version
            $mergedAt = if ($pr.merged_at -is [datetime]) {
                $pr.merged_at.ToUniversalTime()
            } else {
                [datetime]::Parse($pr.merged_at, [System.Globalization.CultureInfo]::InvariantCulture,
                    [System.Globalization.DateTimeStyles]::AdjustToUniversal)
            }
            if ($mergedAt -lt $script:MergedAfterCutoff) {
                Write-Warning "PR #$PrNum merged $($pr.merged_at) — before cutoff ($($script:MergedAfterCutoff.ToString('yyyy-MM-dd'))). Skipping."
                # Return a distinct sentinel (not $null) so callers can tell an
                # intentional pre-cutoff skip apart from a real fetch failure and
                # avoid mis-reporting the skip as an error / failing the whole run.
                return @{ SkippedPreCutoff = $true; Number = $PrNum }
            }
        }
        return @{
            Number         = $PrNum
            Title          = $pr.title
            Milestone      = if ($pr.milestone) { $pr.milestone.title } else { $null }
            MsNumber       = if ($pr.milestone) { $pr.milestone.number } else { $null }
            Url            = $pr.html_url
            Body           = if ($pr.body) { $pr.body } else { "" }
            BaseRef        = if ($pr.base -and $pr.base.ref) { $pr.base.ref } else { $null }
            MergeCommitSha = if ($pr.merge_commit_sha) { $pr.merge_commit_sha } else { $null }
        }
    } catch {
        Write-Warning "Failed to fetch PR #$PrNum`: $_"
        return $null
    }
}

function Get-IssueInfo([int]$IssueNumber) {
    try {
        $issue = Invoke-GhApi "repos/dotnet/maui/issues/$IssueNumber"
        if ($issue.PSObject.Properties['pull_request'] -and $issue.pull_request) { return $null }
        return @{
            Number    = $IssueNumber
            Title     = $issue.title
            State     = $issue.state
            Milestone = if ($issue.milestone) { $issue.milestone.title } else { $null }
            MsNumber  = if ($issue.milestone) { $issue.milestone.number } else { $null }
            Url       = $issue.html_url
        }
    } catch {
        Write-Warning "Failed to fetch issue #$IssueNumber`: $_"
        return $null
    }
}

function Get-LinkedIssues([string]$Body, [string]$Title) {
    $text = "$Title`n$Body"
    $issues = [System.Collections.Generic.HashSet[int]]::new()
    # Match `Fixes #N` or `Fixes dotnet/maui#N`. The owner/repo prefix is restricted to
    # `dotnet/maui` (case-insensitive) because we are collecting linked issues for THIS
    # repo only — accepting any `[A-Za-z0-9_\-./]+` would silently treat a cross-repo
    # reference like `Fixes dotnet/runtime#1234` as if it linked dotnet/maui#1234,
    # clobbering an unrelated MAUI issue's milestone. The literal-prefix form mirrors
    # GitHub's own auto-close behavior (which only auto-closes for the same-repo or
    # explicitly-named-cross-repo form).
    foreach ($m in [regex]::Matches($text, '(?:fix(?:es|ed)?|close[sd]?|resolve[sd]?)\s+(?:dotnet/maui)?#(\d+)', 'IgnoreCase')) {
        [void]$issues.Add([int]$m.Groups[1].Value)
    }
    # Match URLs only when preceded by a fixing keyword (mirrors GitHub auto-close behavior).
    # Bare URLs like "See https://github.com/.../issues/123" are informational, not fixing references.
    foreach ($m in [regex]::Matches($text, '(?:fix(?:es|ed)?|close[sd]?|resolve[sd]?)\s+https?://github\.com/dotnet/maui/issues/(\d+)', 'IgnoreCase')) {
        [void]$issues.Add([int]$m.Groups[1].Value)
    }
    return ($issues | Sort-Object)
}

function Invoke-GhCli {
    # Thin shim around the `gh` CLI so Pester tests can Mock it without spawning a real
    # process. Returns the combined stdout/stderr text; callers can check $LASTEXITCODE.
    [CmdletBinding()]
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)
    return (& gh @Arguments 2>&1)
}

function Close-LinkedIssue {
    <#
    .SYNOPSIS
        Closes a GitHub issue that was fixed by a PR merged to a non-default branch.
    .DESCRIPTION
        GitHub only auto-closes "fixes #N" linked issues when the PR is merged to
        the default branch. For PRs merged to net11.0, release/*, etc. the issue
        stays open. This helper closes the issue and leaves a breadcrumb comment.

        - If the issue is already closed: no-op (logs and returns).
        - If -Apply is not set on the script: dry-run (logs intent, no gh call).
        - gh failures are warned, not thrown — one bad issue shouldn't fail the
          whole milestone-drift run.
    .PARAMETER IssueNumber
        Issue to close.
    .PARAMETER PrNumber
        PR that fixed it (referenced in the comment).
    .PARAMETER BaseRef
        Branch the PR was merged to (referenced in the comment).
    .PARAMETER Apply
        When false, dry-run only.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][int]$IssueNumber,
        [Parameter(Mandatory)][int]$PrNumber,
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string]$BaseRef,
        [Parameter(Mandatory)][bool]$Apply
    )

    # Check current state. Don't double-close.
    $stateJson = Invoke-GhCli 'issue' 'view' "$IssueNumber" '--repo' 'dotnet/maui' '--json' 'state,number,title'
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Close-LinkedIssue: failed to fetch issue #$IssueNumber state: $stateJson"
        return
    }

    try {
        $issue = $stateJson | ConvertFrom-Json
    } catch {
        Write-Warning "Close-LinkedIssue: failed to parse gh response for issue #$IssueNumber`: $_"
        return
    }

    # `gh issue view --json state` returns "OPEN" / "CLOSED" (uppercase). Match case-insensitively
    # so we're robust if gh ever changes casing.
    if ($issue.state -and $issue.state.ToString().ToUpperInvariant() -eq 'CLOSED') {
        Write-Host "  ℹ️  issue #$IssueNumber already closed — no action needed"
        return
    }

    $baseLabel = if ($BaseRef) { $BaseRef } else { '(unknown branch)' }
    $comment = "Closed by #$PrNumber (merged to ``$baseLabel``). GitHub only auto-closes for PRs merged to the default branch; this issue was fixed by a PR merged to a non-default branch."

    if (-not $Apply) {
        Write-Host "  [dry-run] would close issue #$IssueNumber as completed (merged to $baseLabel via PR #$PrNumber)"
        return
    }

    $closeResult = Invoke-GhCli 'issue' 'close' "$IssueNumber" '--repo' 'dotnet/maui' '--reason' 'completed' '--comment' $comment
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Close-LinkedIssue: failed to close issue #$IssueNumber via gh: $closeResult"
        return
    }
    Write-Host "  ✅ closed issue #$IssueNumber as completed (merged to $baseLabel via PR #$PrNumber)"
}

function Set-ItemMilestone([int]$ItemNumber, [int]$MilestoneNumber) {
    $body = @{ milestone = $MilestoneNumber } | ConvertTo-Json
    $result = $body | gh api "repos/dotnet/maui/issues/$ItemNumber" -X PATCH --input - 2>&1
    if ($LASTEXITCODE -ne 0) { throw "Failed to set milestone on #$ItemNumber`: $result" }
}

# Cache of milestone-validity lookups so we don't re-query gh for the same (issue, milestone) pair.
$script:milestoneValidationCache = @{}

# Caches used by the commit-in-tag validation. Populated lazily.
$script:milestoneTagsCache = @{}
$script:tagPrCache = @{}

# Script-scoped context set by Invoke-AnalyzeRelease so validators can resolve
# milestone → tag mappings without re-fetching.
$script:validationRepoPath = $null
$script:validationAllTags = $null
$script:validationMajor = 0

function Initialize-MilestoneValidationContext {
    <# Reset and seed per-run validation context. Call from Invoke-AnalyzeRelease. #>
    param(
        [Parameter(Mandatory)][string]$RepoPath,
        [Parameter(Mandatory)][string[]]$AllTags,
        [Parameter(Mandatory)][int]$Major
    )
    $script:milestoneValidationCache = @{}
    $script:milestoneTagsCache = @{}
    $script:tagPrCache = @{}
    $script:validationRepoPath = $RepoPath
    $script:validationAllTags = $AllTags
    $script:validationMajor = $Major
}

function Get-TagsForMilestone {
    <#
    .SYNOPSIS
        Returns release tags whose ConvertTo-Milestone result matches the given milestone name.
    .DESCRIPTION
        e.g. ".NET 10 SR6" → @("10.0.60") (and 10.0.61 only if it maps back to SR6, which it
        doesn't — 10.0.61 maps to ".NET 10 SR6.1"). Empty array if the milestone is not a
        comparable release (Backlog/Planning) or no tag matches.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string]$Milestone
    )

    if ([string]::IsNullOrWhiteSpace($Milestone)) { return @() }
    if (-not $script:validationAllTags -or $script:validationMajor -le 0) { return @() }

    if ($script:milestoneTagsCache.ContainsKey($Milestone)) {
        return @($script:milestoneTagsCache[$Milestone])
    }

    # Cross-major scenarios are the WHOLE POINT of the KEEP guard: a net11.0 PR may have
    # an issue currently milestoned `.NET 10 SR6` (because a prior fix shipped in 10.0.60).
    # We must filter tags by THIS milestone's major (10), not by $script:validationMajor
    # (set to the target major — 11 — by Initialize-MilestoneValidationContext).
    # Pre-fix, the `Test-IsReleaseTag $tag 11` filter dropped every 10.x tag, returning
    # @() → Test-PrShippedInMilestone false → validator false → KEEP guard clobbered SR6.
    $msMajor = if ($Milestone -match '\.NET (\d+)') { [int]$Matches[1] } else { 0 }
    if ($msMajor -le 0) {
        # Non-comparable milestone — can't derive a major, fall back to validation context.
        $msMajor = $script:validationMajor
    }

    $matchingTags = [System.Collections.Generic.List[string]]::new()
    foreach ($tag in $script:validationAllTags) {
        if (-not (Test-IsReleaseTag $tag $msMajor)) { continue }
        $tagMs = ConvertTo-Milestone $tag
        if ([string]::IsNullOrWhiteSpace($tagMs)) { continue }
        if (Test-MilestoneMatch $tagMs $Milestone) {
            [void]$matchingTags.Add($tag)
        }
    }

    $script:milestoneTagsCache[$Milestone] = $matchingTags.ToArray()
    return @($script:milestoneTagsCache[$Milestone])
}

function Get-PrsInTag {
    <# Returns (cached) the set of PR numbers reachable from a tag.
       Returns $null on git failure so the caller can propagate "uncertain" rather than
       silently treat a transient git error as "no PRs in this tag" (which would cause
       Test-PrShippedInMilestone to return $false → Test-MilestoneValidForPr false →
       KEEP guard clobbers a valid earlier milestone). #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Tag
    )
    if (-not $script:validationRepoPath) { return $null }
    if ($script:tagPrCache.ContainsKey($Tag)) {
        # Same unary-comma protection as the fresh-read return below — otherwise a
        # cached 1-element HashSet gets unwrapped to Int32 (no .Contains) and a
        # cached 0-element HashSet gets unwrapped to $null (mistaken for git failure).
        return ,$script:tagPrCache[$Tag]
    }
    try {
        $prs = Get-PrNumbersReachableFromTag $Tag $script:validationRepoPath
    } catch {
        Write-Warning "Get-PrsInTag: failed to read PRs reachable from ${Tag}: $_"
        # Do NOT cache the uncertain result — let a later call retry.
        return $null
    }
    # Store as HashSet for O(1) Contains lookups.
    $set = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($p in $prs) { [void]$set.Add([int]$p) }
    $script:tagPrCache[$Tag] = $set
    # Use unary comma so PowerShell doesn't unwrap the HashSet (otherwise the caller
    # receives an Int32 / Object[] depending on size and `.Contains()` fails).
    return ,$set
}

function Test-PrShippedInMilestone {
    <#
    .SYNOPSIS
        Returns $true iff a PR's merge commit is reachable from a tag that maps to $Milestone.
    .DESCRIPTION
        Used by earliest-release-wins validation. A PR is considered to have "shipped in
        milestone X" only when its commit is actually present in at least one release tag
        that ConvertTo-Milestone maps to X. This rejects:
          - PRs that were milestoned for X but never made it (still on `inflight/*`)
          - Issues whose linking PR was cherry-picked to a later branch but never landed in X

        Returns:
          - $true  — PR's commit is in at least one tag mapping to $Milestone.
          - $false — definitively NOT in any matching tag (all git reads succeeded).
          - $null  — UNCERTAIN: at least one Get-PrsInTag call failed. Caller must skip
                     the item to avoid clobbering a possibly-valid earlier milestone.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][int]$PrNumber,
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string]$Milestone
    )

    $tags = @(Get-TagsForMilestone -Milestone $Milestone)
    if ($tags.Count -eq 0) { return $false }

    $anyFailure = $false
    foreach ($tag in $tags) {
        $prs = Get-PrsInTag -Tag $tag
        if ($null -eq $prs) {
            # Git read for THIS tag failed — but we can still return $true if a SUCCEEDING
            # tag read in the same milestone contains the PR. So note the failure and keep
            # scanning; only convert to $null below if no tag confirmed the PR.
            $anyFailure = $true
            continue
        }
        if ($prs.Contains([int]$PrNumber)) { return $true }
    }
    if ($anyFailure) { return $null }
    return $false
}

function Test-MilestoneValidForIssue {
    <#
    .SYNOPSIS
        Checks whether an issue's current milestone is "valid" — i.e., a real fix PR
        for this issue shipped under that milestone — so we shouldn't override it.
    .DESCRIPTION
        Searches all merged PRs that reference the issue (by `#N` in title/body OR by
        `issues/N` URL form in body), filters to ones with an actual Fixes/Closes/Resolves
        verb for this exact issue number, and returns $true if any of them has its merge
        commit reachable from a tag matching $Milestone.

        Used to avoid clobbering an earlier-release milestone (e.g., .NET 10 SR6)
        when a follow-up fix later shipped in net11.0 — see GitHub PR #35858
        follow-up for context.

        Returns:
          - $true  — at least one linking fix PR shipped in $Milestone
          - $false — milestone is non-comparable, or we successfully queried and no
                     linking PR was found / shipped in this milestone
          - $null  — UNCERTAIN: gh search or parse failed. Caller must treat this as
                     "do not touch this item" (failing open here would silently destroy
                     legitimate earlier-release milestones on transient gh failures).
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][int]$IssueNumber,
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string]$Milestone
    )

    if ([string]::IsNullOrWhiteSpace($Milestone)) { return $false }

    $cacheKey = "$IssueNumber|$Milestone"
    if ($script:milestoneValidationCache.ContainsKey($cacheKey)) {
        # Cache stores $true / $false / $null verbatim — propagate exactly.
        return $script:milestoneValidationCache[$cacheKey]
    }

    # Single OR-query covers both linking forms in one Search API call:
    #   - `#N in:title,body` — `Fixes #N` whether the token appears in body or title.
    #   - `issues/N in:body` — URL form (`Fixes https://github.com/.../issues/N`) where
    #     the body never contains the literal `#N` token.
    # The GitHub Search API supports `OR` between query terms. Combining keeps us under
    # the 30-req/min Search rate limit on large tag-range walks (~100 issues = ~100 calls
    # instead of ~200) and avoids the "discard positive evidence on partial failure"
    # trap from running two separate queries.
    # If the search fails we return $null (uncertain) — better to leave the milestone
    # alone than to silently overwrite a valid earlier one based on a partial result.
    $query = "#$IssueNumber in:title,body OR issues/$IssueNumber in:body"
    # --limit 100: hardened against the (rare but real) case of a heavily-referenced issue
    # where the actual fixing PR gets sorted past position 50 by gh's relevance ranking.
    # Pre-100 the validator would silently return $false → clobber a possibly-valid earlier
    # milestone. 100 is the gh search per-page max; below pagination concerns kick in.
    $json = Invoke-GhCli 'search' 'prs' $query `
        '--repo' 'dotnet/maui' '--merged' '--limit' '100' `
        '--json' 'number,title,body,url'
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Test-MilestoneValidForIssue: gh search failed for issue #$IssueNumber (query: '$query'): $json"
        # Do NOT cache $null — a transient gh failure would otherwise permanently disable
        # KEEP validation for this (issue, milestone) pair for the entire run. Letting the
        # next call retry is safe because the check is read-only.
        return $null
    }

    try {
        $prs = $json | ConvertFrom-Json
    } catch {
        Write-Warning "Test-MilestoneValidForIssue: failed to parse gh search response for issue #$IssueNumber (query: '$query'): $_"
        return $null
    }

    $prHashes = [System.Collections.Generic.Dictionary[int, object]]::new()
    foreach ($pr in @($prs)) {
        if ($null -eq $pr -or $null -eq $pr.number) { continue }
        $n = [int]$pr.number
        if (-not $prHashes.ContainsKey($n)) { $prHashes[$n] = $pr }
    }

    $sawUncertain = $false
    foreach ($pr in $prHashes.Values) {
        # Re-use the same regex Get-LinkedIssues uses, but pinned to THIS issue number,
        # so a casual `#34490` mention doesn't count — we want only real Fixes/Closes/Resolves.
        # Prefix restricted to `dotnet/maui` so a `Fixes dotnet/runtime#NNNN` mention
        # in an unrelated discussion doesn't get treated as if it linked dotnet/maui#NNNN.
        # Also accept the URL form `Fixes https://github.com/.../issues/N`.
        $body = if ($pr.body) { [string]$pr.body } else { '' }
        $title = if ($pr.title) { [string]$pr.title } else { '' }
        $combined = "$title`n$body"
        $hashMatch = $combined -match "(?i)(?:fix(?:es|ed)?|close[sd]?|resolve[sd]?)\s+(?:dotnet/maui)?#$IssueNumber\b"
        $urlMatch  = $combined -match "(?i)(?:fix(?:es|ed)?|close[sd]?|resolve[sd]?)\s+https?://github\.com/dotnet/maui/issues/$IssueNumber\b"
        if (-not ($hashMatch -or $urlMatch)) {
            continue
        }

        # The linking PR must actually have its commit in a tag that maps to $Milestone.
        # Trusting the PR's own milestone field isn't enough — that field can be stale
        # (e.g. a hotfix PR milestoned `.NET 10 SR7` but only present in tag 10.0.71
        # which maps to `.NET 10 SR7.1`).
        # Test-PrShippedInMilestone is tristate: $null = git read failed for some tag,
        # we don't know definitively. Don't conclude $false from that — there may be
        # ANOTHER linking PR in the list that confirmably ships in the milestone.
        $shipped = Test-PrShippedInMilestone -PrNumber ([int]$pr.number) -Milestone $Milestone
        if ($shipped) {
            $script:milestoneValidationCache[$cacheKey] = $true
            return $true
        }
        if ($null -eq $shipped) { $sawUncertain = $true }
    }

    if ($sawUncertain) {
        # No definitive $true, but at least one linking PR couldn't be checked due to a
        # git failure. Don't cache and don't return $false — the caller should skip the
        # item to preserve the current milestone.
        Write-Warning "Test-MilestoneValidForIssue: at least one linking PR's tag membership could not be determined for issue #$IssueNumber + milestone '$Milestone'; returning uncertain"
        return $null
    }

    $script:milestoneValidationCache[$cacheKey] = $false
    return $false
}

function Test-MilestoneValidForPr {
    <#
    .SYNOPSIS
        Returns $true iff the PR's commit is reachable from a tag that maps to $Milestone
        — meaning the PR genuinely shipped in that release and we shouldn't override the
        milestone to a later one.
    .DESCRIPTION
        Mirrors the issue-side check, but for PRs themselves. The motivating case: a PR
        merged to SR6's release branch was later cherry-picked to SR8 via main flow. Its
        commit therefore appears in BOTH `10.0.60` and `10.0.80`. An SR8 audit would
        otherwise stomp the (correct) `.NET 10 SR6` milestone.

        Returns:
          - $true  — PR's commit is in a tag mapping to $Milestone.
          - $false — milestone is null/empty/non-comparable, or no matching tag contains the PR.
          - $null  — UNCERTAIN: at least one git read failed. Caller must skip to avoid
                     clobbering a possibly-valid earlier milestone.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][int]$PrNumber,
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string]$Milestone
    )
    if ([string]::IsNullOrWhiteSpace($Milestone)) { return $false }
    return Test-PrShippedInMilestone -PrNumber $PrNumber -Milestone $Milestone
}

#endregion

#region ── Correction helpers ─────────────────────────────────────────────

function Test-AndRecordCorrection(
    [string]$ItemType,
    [int]$ItemNumber,
    [string]$ItemTitle,
    [string]$ItemUrl,
    [string]$CurrentMilestone,
    [string]$ExpectedMs,
    [hashtable]$ResolvedMs,
    [int]$RelatedPr,
    [hashtable]$Report
) {
    if (Test-MilestoneMatch $CurrentMilestone $ExpectedMs) {
        # Dedup: don't count the same item as correct twice (e.g. issue linked from multiple PRs)
        $key = "$ItemType`:$ItemNumber"
        if (-not $Report.ContainsKey('_checkedItems')) { $Report._checkedItems = [System.Collections.Generic.HashSet[string]]::new() }
        if ($Report._checkedItems.Add($key)) {
            $Report.AlreadyCorrect++
        }
        Write-Verbose "  ✅ $ItemType #$ItemNumber`: $CurrentMilestone (correct)"
        return
    }

    # Skip if a correction for this item was already recorded (e.g. same issue linked from multiple PRs)
    $existing = $Report.Corrections | Where-Object { $_.ItemType -eq $ItemType -and $_.Number -eq $ItemNumber }
    if ($existing) {
        Write-Verbose "  ⏭️  $ItemType #$ItemNumber`: already queued for correction (via PR #$($existing.RelatedPr))"
        return
    }

    # Earlier-milestone validation: if the current milestone is an earlier real release
    # than the target AND we can prove the fix shipped in that release, KEEP the earlier
    # milestone (earliest release wins).
    #   - For ISSUES: confirm a fix-linking PR's commit is reachable from a tag mapping
    #     to the current milestone.
    #   - For PRS: confirm the PR's own commit is reachable from such a tag (catches the
    #     cherry-pick scenario where the same PR ships in multiple release branches).
    # Both validators are tristate. `$null` means a transient gh/git failure made the
    # answer indeterminate; we MUST skip the item to avoid silently clobbering a
    # possibly-valid earlier milestone.
    if (-not [string]::IsNullOrWhiteSpace($CurrentMilestone)) {
        $cmp = Compare-MauiMilestone $CurrentMilestone $ExpectedMs
        if ($null -ne $cmp -and $cmp -lt 0) {
            $isValidEarlier = $false
            if ($ItemType -eq 'issue') {
                $isValidEarlier = Test-MilestoneValidForIssue -IssueNumber $ItemNumber -Milestone $CurrentMilestone
            } elseif ($ItemType -eq 'pr') {
                $isValidEarlier = Test-MilestoneValidForPr -PrNumber $ItemNumber -Milestone $CurrentMilestone
            }

            if ($null -eq $isValidEarlier) {
                # Validator could not determine (transient failure). Do NOT queue a correction
                # — overwriting a possibly-valid earlier milestone on a transient failure is the
                # exact data-loss case the earliest-release-wins guard exists to prevent.
                Write-Warning "  ⚠️  $ItemType #$ItemNumber`: milestone validation was inconclusive (likely transient gh failure); skipping to preserve current milestone '$CurrentMilestone'"
                return
            }

            if ($isValidEarlier) {
                # Dedup: the same issue/PR can be reached multiple times in tag-mode walks
                # (e.g. issue linked from multiple PRs in the same range, or a cherry-picked
                # PR matching multiple linking-PR scans). Without this guard, $Report.Kept
                # accumulates duplicate rows that pollute the JSON report and the rolled-up
                # GitHub issue table.
                $keepKey = "$ItemType`:$ItemNumber"
                if (-not $Report.ContainsKey('_keptItems')) { $Report._keptItems = [System.Collections.Generic.HashSet[string]]::new() }
                if (-not $Report._keptItems.Add($keepKey)) {
                    Write-Verbose "  ⏭️  $ItemType #$ItemNumber`: already in KEEP set (via earlier linking PR)"
                    return
                }
                if (-not $Report.ContainsKey('Kept')) { $Report.Kept = [System.Collections.ArrayList]::new() }
                $kept = @{
                    ItemType   = $ItemType
                    Number     = $ItemNumber
                    Title      = $ItemTitle
                    Url        = $ItemUrl
                    Current    = $CurrentMilestone
                    Expected   = $ExpectedMs
                }
                if ($RelatedPr -gt 0) { $kept.RelatedPr = $RelatedPr }
                [void]$Report.Kept.Add($kept)
                Write-Host "  [KEEP] $ItemType #$ItemNumber`: keeping earlier valid milestone '$CurrentMilestone' (target was '$ExpectedMs')"
                return
            }
        }
    }

    $correction = @{
        ItemType   = $ItemType
        Number     = $ItemNumber
        Title      = $ItemTitle
        Url        = $ItemUrl
        Current    = $CurrentMilestone
        Expected   = $ExpectedMs
        Resolved   = $ResolvedMs.Title
        ResolvedNo = $ResolvedMs.Number
    }
    if ($RelatedPr -gt 0) { $correction.RelatedPr = $RelatedPr }
    [void]$Report.Corrections.Add($correction)

    $current = if ($CurrentMilestone) { $CurrentMilestone } else { "(none)" }
    Write-Verbose "  ⚠️  $ItemType #$ItemNumber`: $current → $($ResolvedMs.Title)"
}

#endregion

#region ── Analysis ───────────────────────────────────────────────────────

function Invoke-AnalyzeSinglePr([int]$PrNum, [string]$ReleaseTag, [string]$Repo) {
    Write-Host "`n$('═' * 70)"
    Write-Host "  Single-PR mode: #$PrNum"
    Write-Host "$('═' * 70)`n"

    $expectedMs = $null
    $preLabel = $null
    $preIter = 0
    $versionInfo = $null
    $allTags = $null  # declared so the validation-context seeding below can probe it under StrictMode

    # Fetch PR info first — we need merge_commit_sha for version detection
    $pr = Get-PrInfo $PrNum
    if ($pr -is [hashtable] -and $pr.ContainsKey('SkippedPreCutoff')) {
        throw "PR #$PrNum was merged before the -MergedAfter cutoff ($($script:MergedAfterCutoff.ToString('yyyy-MM-dd'))). Lower -MergedAfter to process it."
    }
    if (-not $pr) { throw "Could not fetch PR #$PrNum" }

    if ($ReleaseTag) {
        # Explicit tag: validate it exists and optionally check PR is in range
        $allTags = Get-AllTags $Repo
        if ($ReleaseTag -notin $allTags) {
            throw "Tag '$ReleaseTag' not found in repo. Available SR tags: $(($allTags | Where-Object { $_ -match '^\d+\.0\.\d+$' }) -join ', ')"
        }
        $prev = Find-PreviousTag $ReleaseTag $allTags
        if ($prev) {
            $prsInRange = Get-PrNumbersBetweenTags $prev $ReleaseTag $Repo
        } else {
            $prsInRange = Get-PrNumbersReachableFromTag $ReleaseTag $Repo
        }
        if ($PrNum -notin $prsInRange) {
            Write-Warning "PR #$PrNum is not in the commit range for tag $ReleaseTag. The milestone may be set incorrectly."
        }
        Write-Host "  Using explicit tag: $ReleaseTag"
        # For preview/RC tags, read Versions.props at the tag to get pre-release info
        $versionInfo = Get-VersionFromGitRef $ReleaseTag $Repo
        if ($versionInfo) {
            $preLabel = $versionInfo.PreLabel
            $preIter = if ($versionInfo.PreIter) { $versionInfo.PreIter } else { 0 }
        }
    } elseif ($pr.MergeCommitSha) {
        # Step 1: Check release branches first — find the earliest release containing this commit.
        # This is the most accurate signal for which release the PR actually ships in.
        $versionInfo = Get-VersionFromGitRef $pr.MergeCommitSha $Repo
        $detectedMajor = if ($versionInfo -and $versionInfo.Tag -match '^(\d+)\.') { [int]$Matches[1] } else { Get-CurrentMajorVersion $Repo }

        $releaseBranch = Find-ReleaseBranchForCommit $pr.MergeCommitSha $Repo $detectedMajor $PrNum
        if ($releaseBranch) {
            $expectedMs = $releaseBranch.Milestone
            if ($versionInfo) { $ReleaseTag = $versionInfo.Tag }
            Write-Host "  Found in release branch: $($releaseBranch.Branch) → $expectedMs"
        } else {
            # Step 2: Fall back to Versions.props on the development branch HEAD.
            # All branches ultimately feed into the main development branch for their
            # .NET version (main for .NET 10, net11.0 for .NET 11). Read from there
            # to get the current target version, regardless of which staging branch
            # the PR was merged to (inflight/current, inflight/candidate, darc/*, etc.).
            $mainBranch = Get-MainBranchForVersion $detectedMajor $Repo
            $devBranch = "origin/$mainBranch"
            $branchVersionInfo = Get-VersionFromGitRef $devBranch $Repo
            if ($branchVersionInfo) {
                $versionInfo = $branchVersionInfo
                $ReleaseTag = $branchVersionInfo.Tag
                $preLabel = $branchVersionInfo.PreLabel
                $preIter = if ($branchVersionInfo.PreIter) { $branchVersionInfo.PreIter } else { 0 }
                $preDisplay = if ($branchVersionInfo.PreLabel) { " ($($branchVersionInfo.PreLabel)$($branchVersionInfo.PreIter))" } else { "" }
                Write-Host "  Version from Versions.props on $devBranch`: $ReleaseTag$preDisplay"
            } elseif ($versionInfo) {
                # Branch read failed; fall back to merge commit's version info
                Write-Verbose "  Could not read from $devBranch — using merge commit Versions.props"
                $ReleaseTag = $versionInfo.Tag
                $preLabel = $versionInfo.PreLabel
                $preIter = if ($versionInfo.PreIter) { $versionInfo.PreIter } else { 0 }
            }
        }
    }

    # Determine expected milestone if not already set by release branch detection
    if (-not $expectedMs) {
        # Fallback: try to find in existing tag ranges
        if (-not $ReleaseTag) {
            Write-Host "Auto-detecting release tag for PR #$PrNum..."
            $fallbackMajor = Get-CurrentMajorVersion $Repo
            $found = Find-TagContainingPr $PrNum $Repo $fallbackMajor
            if (-not $found) { throw "PR #$PrNum not found in any release tag range for .NET $fallbackMajor" }
            $ReleaseTag = $found.Tag
            $prevDisplay = if ($found.PreviousTag) { $found.PreviousTag } else { "(root)" }
            Write-Host "  Found in: $prevDisplay..$ReleaseTag"
        }

        # Use the clean version tag (e.g. "11.0.0") for milestone mapping, not the full tag
        $cleanTag = if ($versionInfo) { $versionInfo.Tag } else { $ReleaseTag }
        $expectedMs = ConvertTo-Milestone $cleanTag $preLabel $preIter

        # If ConvertTo-Milestone failed (e.g. $cleanTag is a preview tag string),
        # read Versions.props at the tag to get clean version + pre-release info
        if (-not $expectedMs -and $ReleaseTag) {
            $tagVersionInfo = Get-VersionFromGitRef $ReleaseTag $Repo
            if ($tagVersionInfo) {
                $preLabel = $tagVersionInfo.PreLabel
                $preIter = if ($tagVersionInfo.PreIter) { $tagVersionInfo.PreIter } else { 0 }
                $expectedMs = ConvertTo-Milestone $tagVersionInfo.Tag $preLabel $preIter
            }
        }
    }
    if (-not $expectedMs) { throw "Cannot determine milestone for PR #$PrNum" }

    # Derive MajorVersion from the milestone name, tag, or Versions.props
    $Major = if ($expectedMs -match '\.NET (\d+)') {
        [int]$Matches[1]
    } elseif ($ReleaseTag -and $ReleaseTag -match '^(\d+)\.') {
        [int]$Matches[1]
    } else {
        Get-CurrentMajorVersion $Repo
    }

    # Detect which branch owns this version's tags
    $Branch = Get-MainBranchForVersion $Major $Repo
    Write-Host "  Main branch for .NET $Major`: $Branch"
    Write-Host "  Expected milestone: $expectedMs"

    # Seed validation context so the earliest-release-wins guard (Test-MilestoneValidForIssue /
    # Test-MilestoneValidForPr) works in single-PR mode too — without this, $script:validationAllTags
    # stays $null, Get-TagsForMilestone returns @(), Test-PrShippedInMilestone short-circuits to $false,
    # and the KEEP branch in Test-AndRecordCorrection is silently dead code. Reuse $allTags from the
    # explicit-tag path above when present to avoid an extra git call.
    if (-not $allTags) { $allTags = Get-AllTags $Repo }
    Initialize-MilestoneValidationContext -RepoPath $Repo -AllTags ([string[]]$allTags) -Major $Major

    Write-Host "Fetching GitHub milestones..."
    $allMilestones = Get-AllMilestones
    $match = Find-MatchingMilestone $expectedMs $allMilestones
    if (-not $match) {
        # Milestone may not have been created yet (common during normal development).
        # Warn and return empty report — prevents red CI on every auto-triggered merge.
        Write-Warning "No GitHub milestone found matching `"$expectedMs`". The milestone may not have been created yet. Skipping."
        return @{
            Tag               = $ReleaseTag
            ExpectedMilestone = $expectedMs
            TotalPrs          = 1
            PrsChecked        = 0
            IssuesChecked     = 0
            AlreadyCorrect    = 0
            Corrections       = [System.Collections.ArrayList]::new()
            Errors            = [System.Collections.ArrayList]::new()
        }
    }
    Write-Host "  Resolved to: `"$($match.Title)`" (#$($match.Number))`n"

    $report = @{
        Tag               = $ReleaseTag
        ExpectedMilestone = $expectedMs
        ResolvedMilestone = $match.Title
        ResolvedMsNumber  = $match.Number
        TotalPrs          = 1
        PrsChecked        = 0
        IssuesChecked     = 0
        AlreadyCorrect    = 0
        Corrections       = [System.Collections.ArrayList]::new()
        Errors            = [System.Collections.ArrayList]::new()
    }

    # Safety: skip PRs targeting a different .NET version branch
    if (-not (Test-PrBelongsToVersion $pr.BaseRef $Branch $Major)) {
        Write-Warning "PR #$PrNum targets '$($pr.BaseRef)' which is not for .NET $Major (MainBranch: $Branch). Skipping."
        return $report
    }
    $report.PrsChecked++

    Test-AndRecordCorrection "pr" $PrNum $pr.Title $pr.Url $pr.Milestone $expectedMs $match 0 $report

    # Check linked issues
    $linked = Get-LinkedIssues $pr.Body $pr.Title
    foreach ($issueNum in $linked) {
        $issue = Get-IssueInfo $issueNum
        if (-not $issue) { continue }
        $report.IssuesChecked++
        Test-AndRecordCorrection "issue" $issueNum $issue.Title $issue.Url $issue.Milestone $expectedMs $match $PrNum $report
        if ($script:CloseFixedIssues) {
            Close-LinkedIssue -IssueNumber $issueNum -PrNumber $PrNum -BaseRef $pr.BaseRef -Apply ([bool]$script:Apply)
        }
    }

    return $report
}

function Invoke-AnalyzeRelease([string]$ReleaseTag, [string]$PrevTag, [string]$Repo) {
    # Try direct tag-to-milestone mapping first (works for stable tags like 10.0.50)
    $expectedMs = ConvertTo-Milestone $ReleaseTag

    # If that fails, read Versions.props at the tag (works for preview/RC tags like 11.0.0-preview.3.26203.7)
    if (-not $expectedMs) {
        $versionInfo = Get-VersionFromGitRef $ReleaseTag $Repo
        if ($versionInfo) {
            $preLabel = $versionInfo.PreLabel
            $preIter = if ($versionInfo.PreIter) { $versionInfo.PreIter } else { 0 }
            $expectedMs = ConvertTo-Milestone $versionInfo.Tag $preLabel $preIter
        }
    }
    if (-not $expectedMs) { throw "Cannot determine milestone for tag $ReleaseTag" }

    # Derive MajorVersion from the tag
    $Major = if ($ReleaseTag -match '^(\d+)\.') { [int]$Matches[1] } else { Get-CurrentMajorVersion $Repo }

    $allTags = Get-AllTags $Repo
    if ($ReleaseTag -notin $allTags) { throw "Tag $ReleaseTag not found in repo" }

    # Seed validation context so Test-MilestoneValid* helpers can resolve milestone → tag.
    Initialize-MilestoneValidationContext -RepoPath $Repo -AllTags ([string[]]$allTags) -Major $Major

    if (-not $PrevTag) {
        $PrevTag = Find-PreviousTag $ReleaseTag $allTags
        # No previous tag means this is the first release (e.g. GA).
        # We'll use Get-PrNumbersReachableFromTag instead of a range.
    }

    # Detect which branch owns this version's tags
    $Branch = Get-MainBranchForVersion $Major $Repo

    $prevDisplay = if ($PrevTag) { $PrevTag } else { "(root)" }

    Write-Host "`n$('═' * 70)"
    Write-Host "  Release: $ReleaseTag"
    Write-Host "  Previous: $prevDisplay"
    Write-Host "  Main branch: $Branch"
    Write-Host "  Expected milestone: $expectedMs"
    Write-Host "$('═' * 70)`n"

    Write-Host "Fetching GitHub milestones..."
    $allMilestones = Get-AllMilestones
    $match = Find-MatchingMilestone $expectedMs $allMilestones
    if (-not $match) {
        throw "No GitHub milestone found matching `"$expectedMs`". Available: $($allMilestones.Keys -join ', ')"
    }
    Write-Host "  Resolved to: `"$($match.Title)`" (#$($match.Number))`n"

    if ($PrevTag) {
        Write-Host "Finding PRs between $PrevTag..$ReleaseTag..."
        $prNumbers = Get-PrNumbersBetweenTags $PrevTag $ReleaseTag $Repo
    } else {
        Write-Host "Finding all PRs reachable from $ReleaseTag (first tag)..."
        $prNumbers = Get-PrNumbersReachableFromTag $ReleaseTag $Repo
    }
    Write-Host "  Found $($prNumbers.Count) PRs`n"

    $report = @{
        Tag               = $ReleaseTag
        PreviousTag       = $PrevTag
        ExpectedMilestone = $expectedMs
        ResolvedMilestone = $match.Title
        ResolvedMsNumber  = $match.Number
        TotalPrs          = $prNumbers.Count
        PrsChecked        = 0
        PrsSkippedWrongBranch = 0
        PrsSkippedPreCutoff = 0
        IssuesChecked     = 0
        AlreadyCorrect    = 0
        Corrections       = [System.Collections.ArrayList]::new()
        Errors            = [System.Collections.ArrayList]::new()
    }

    for ($i = 0; $i -lt $prNumbers.Count; $i++) {
        $prNum = $prNumbers[$i]
        Write-Verbose "  [$($i+1)/$($prNumbers.Count)] PR #$prNum..."

        $pr = Get-PrInfo $prNum
        if ($pr -is [hashtable] -and $pr.ContainsKey('SkippedPreCutoff')) {
            # Intentional pre-cutoff skip (see Get-PrInfo) — not a fetch failure.
            # Count it separately so an all-pre-cutoff cohort exits cleanly instead
            # of being reported as "0 PRs checked, N errors".
            $report.PrsSkippedPreCutoff++
            continue
        }
        if (-not $pr) {
            [void]$report.Errors.Add("Failed to fetch PR #$prNum")
            continue
        }

        # Safety: skip PRs targeting a different .NET version branch
        if (-not (Test-PrBelongsToVersion $pr.BaseRef $Branch $Major)) {
            Write-Verbose "  ⏭️  PR #$prNum targets '$($pr.BaseRef)' — not for .NET $Major, skipping"
            $report.PrsSkippedWrongBranch++
            continue
        }
        $report.PrsChecked++

        Test-AndRecordCorrection "pr" $prNum $pr.Title $pr.Url $pr.Milestone $expectedMs $match 0 $report

        $linked = Get-LinkedIssues $pr.Body $pr.Title
        foreach ($issueNum in $linked) {
            $issue = Get-IssueInfo $issueNum
            if (-not $issue) { continue }
            $report.IssuesChecked++
            Test-AndRecordCorrection "issue" $issueNum $issue.Title $issue.Url $issue.Milestone $expectedMs $match $prNum $report
            if ($script:CloseFixedIssues) {
                Close-LinkedIssue -IssueNumber $issueNum -PrNumber $prNum -BaseRef $pr.BaseRef -Apply ([bool]$script:Apply)
            }
        }
    }

    return $report
}

#endregion

#region ── Output ─────────────────────────────────────────────────────────

function Write-Report([hashtable]$Report) {
    Write-Host "`n$('═' * 70)"
    Write-Host "  MILESTONE DRIFT REPORT: $($Report.Tag)"
    Write-Host "$('═' * 70)"
    if ($Report.ContainsKey('PreviousTag') -and $Report.PreviousTag) { Write-Host "  Range: $($Report.PreviousTag)..$($Report.Tag)" }
    Write-Host "  Expected milestone: $($Report.ExpectedMilestone)"
    Write-Host "  Resolved milestone: $($Report.ResolvedMilestone)"
    Write-Host "  PRs in range: $($Report.TotalPrs)"
    Write-Host "  PRs checked: $($Report.PrsChecked)"
    if ($Report.ContainsKey('PrsSkippedWrongBranch') -and $Report.PrsSkippedWrongBranch -gt 0) {
        Write-Host "  PRs skipped (wrong branch): $($Report.PrsSkippedWrongBranch)"
    }
    if ($Report.ContainsKey('PrsSkippedPreCutoff') -and $Report.PrsSkippedPreCutoff -gt 0) {
        Write-Host "  PRs skipped (merged before cutoff): $($Report.PrsSkippedPreCutoff)"
    }
    Write-Host "  Issues checked: $($Report.IssuesChecked)"
    Write-Host "  Already correct: $($Report.AlreadyCorrect)"
    $keptCount = if ($Report.ContainsKey('Kept')) { $Report.Kept.Count } else { 0 }
    if ($keptCount -gt 0) {
        Write-Host "  Kept earlier milestone: $keptCount"
    }
    Write-Host "  Corrections needed: $($Report.Corrections.Count)"
    if ($Report.Errors.Count -gt 0) { Write-Host "  Errors: $($Report.Errors.Count)" }
    Write-Host ""

    if ($keptCount -gt 0) {
        foreach ($k in $Report.Kept) {
            $via = if ($k.ContainsKey('RelatedPr') -and $k.RelatedPr) { " (via PR #$($k.RelatedPr))" } else { "" }
            Write-Host "  [KEEP] $($k.ItemType) #$($k.Number)$via`: keeping '$($k.Current)' (target was '$($k.Expected)')"
        }
        Write-Host ""
    }

    if ($Report.Corrections.Count -eq 0) {
        if ($Report.Errors.Count -gt 0 -and $Report.PrsChecked -eq 0) {
            Write-Host "  ❌ No PRs were successfully checked — all $($Report.Errors.Count) failed.`n"
        } else {
            Write-Host "  ✅ All milestones are correct!`n"
        }
        return
    }

    foreach ($c in $Report.Corrections) {
        $current = if ($c.Current) { $c.Current } else { "(none)" }
        $action = if ($c.Current) { "CHANGE" } else { "SET" }
        $via = if ($c.ContainsKey('RelatedPr') -and $c.RelatedPr) { " (via PR #$($c.RelatedPr))" } else { "" }
        Write-Host "  [$action] $($c.ItemType) #$($c.Number)$via`: $current → $($c.Resolved)"
    }
    Write-Host ""
}

function Save-ReportJson([hashtable]$Report, [string]$Path) {
    # NB: `if (...) { @() } else { @() }` collapses an empty `@()` to $null in a PowerShell
    # assignment under StrictMode, which trips the later .Count call. Use a typed cast
    # ([object[]]) so we always end up with an array, even when there are zero kept items.
    [object[]]$keptItems = @()
    if ($Report.ContainsKey('Kept')) { $keptItems = @($Report.Kept) }
    $data = @{
        tag                = $Report.Tag
        previous_tag       = if ($Report.ContainsKey('PreviousTag')) { $Report.PreviousTag } else { $null }
        expected_milestone = $Report.ExpectedMilestone
        resolved_milestone = $Report.ResolvedMilestone
        summary            = @{
            total_prs_in_range = $Report.TotalPrs
            prs_checked        = $Report.PrsChecked
            prs_skipped_wrong_branch = if ($Report.ContainsKey('PrsSkippedWrongBranch')) { $Report.PrsSkippedWrongBranch } else { 0 }
            issues_checked     = $Report.IssuesChecked
            already_correct    = $Report.AlreadyCorrect
            kept_earlier       = $keptItems.Count
            corrections_needed = $Report.Corrections.Count
            errors             = $Report.Errors.Count
        }
        corrections        = @($Report.Corrections)
        kept               = $keptItems
        errors             = @($Report.Errors)
    }
    $data | ConvertTo-Json -Depth 5 | Set-Content -Path $Path -Encoding utf8
    Write-Host "  📄 Report saved to: $Path"
}

function Invoke-ApplyCorrections([hashtable]$Report, [bool]$DoApply) {
    $failCount = 0
    foreach ($c in $Report.Corrections) {
        $current = if ($c.Current) { $c.Current } else { "(none)" }
        if ($DoApply) {
            try {
                Set-ItemMilestone $c.Number $c.ResolvedNo
                Write-Host "  ✅ Updated $($c.ItemType) #$($c.Number): $current → $($c.Resolved)"
            } catch {
                $failCount++
                Write-Host "  ❌ Failed $($c.ItemType) #$($c.Number): $_"
            }
        } else {
            Write-Host "  [DRY-RUN] Would set $($c.ItemType) #$($c.Number) milestone: $current → $($c.Resolved)"
        }
    }
    if ($failCount -gt 0) {
        throw "$failCount milestone update(s) failed. Check output above for details."
    }
}

function New-GitHubIssue([hashtable]$Report, [bool]$WasApplied) {
    $tag = $Report.Tag
    $status = if ($WasApplied) { "Applied" } else { "Dry-Run" }
    $title = "Milestone drift report: $tag ($status)"

    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.AppendLine("## Milestone Drift Report: ``$tag``")
    [void]$sb.AppendLine()
    if ($Report.ContainsKey('PreviousTag') -and $Report.PreviousTag) {
        [void]$sb.AppendLine("**Range:** ``$($Report.PreviousTag)..$tag``")
    }
    [void]$sb.AppendLine("**Expected milestone:** $($Report.ExpectedMilestone)")
    [void]$sb.AppendLine("**Resolved milestone:** $($Report.ResolvedMilestone)")
    [void]$sb.AppendLine("**Status:** $status")
    [void]$sb.AppendLine()

    # Summary table
    [void]$sb.AppendLine("### Summary")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("| Metric | Count |")
    [void]$sb.AppendLine("|--------|-------|")
    [void]$sb.AppendLine("| PRs in range | $($Report.TotalPrs) |")
    [void]$sb.AppendLine("| PRs checked | $($Report.PrsChecked) |")
    [void]$sb.AppendLine("| Issues checked | $($Report.IssuesChecked) |")
    [void]$sb.AppendLine("| Already correct | $($Report.AlreadyCorrect) |")
    if ($Report.ContainsKey('Kept') -and $Report.Kept.Count -gt 0) {
        [void]$sb.AppendLine("| Kept earlier milestone | $($Report.Kept.Count) |")
    }
    [void]$sb.AppendLine("| Corrections needed | $($Report.Corrections.Count) |")
    if ($Report.Errors.Count -gt 0) {
        [void]$sb.AppendLine("| Errors | $($Report.Errors.Count) |")
    }
    [void]$sb.AppendLine()

    if ($Report.ContainsKey('Kept') -and $Report.Kept.Count -gt 0) {
        [void]$sb.AppendLine("### Kept (earlier milestone validated)")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("| Type | Item | Via PR | Kept | Target |")
        [void]$sb.AppendLine("|------|------|--------|------|--------|")
        foreach ($k in $Report.Kept) {
            $via = if ($k.ContainsKey('RelatedPr') -and $k.RelatedPr) { "#$($k.RelatedPr)" } else { "—" }
            [void]$sb.AppendLine("| $($k.ItemType) | #$($k.Number) | $via | $($k.Current) | $($k.Expected) |")
        }
        [void]$sb.AppendLine()
    }

    if ($Report.Corrections.Count -eq 0) {
        [void]$sb.AppendLine("✅ All milestones are correct!")
    } else {
        # Corrections table
        [void]$sb.AppendLine("### Corrections")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("| Action | Type | Item | Via PR | Current | Expected |")
        [void]$sb.AppendLine("|--------|------|------|--------|---------|----------|")
        foreach ($c in $Report.Corrections) {
            $current = if ($c.Current) { $c.Current } else { "_(none)_" }
            $action = if ($c.Current) { "CHANGE" } else { "SET" }
            $via = if ($c.ContainsKey('RelatedPr') -and $c.RelatedPr) { "#$($c.RelatedPr)" } else { "—" }
            [void]$sb.AppendLine("| $action | $($c.ItemType) | #$($c.Number) | $via | $current | $($c.Resolved) |")
        }
    }

    $body = $sb.ToString()

    # Write body to a temp file to avoid shell quoting issues
    $tempFile = [System.IO.Path]::GetTempFileName()
    try {
        $body | Set-Content -Path $tempFile -Encoding utf8 -NoNewline
        $result = gh issue create --repo dotnet/maui --title $title --body-file $tempFile --label "area/infrastructure 🏗️" 2>&1
        if ($LASTEXITCODE -ne 0) { throw "Failed to create issue: $result" }
        Write-Host "  📋 Issue created: $result"
    } finally {
        Remove-Item $tempFile -ErrorAction SilentlyContinue
    }
}

#endregion

#region ── Main ───────────────────────────────────────────────────────────

# Guard: skip main execution when dot-sourced for testing
if ($MyInvocation.InvocationName -eq '.' -or $MyInvocation.Line -match '^\.\s') { return }

if ($Apply) {
    Write-Host "⚠️  --Apply mode: Will modify GitHub milestones!"
}

if ($CloseFixedIssues) {
    $applyHint = if ($Apply) { '' } else { ' (dry-run — pass -Apply to actually close)' }
    Write-Host "ℹ️  --CloseFixedIssues mode: will close issues linked as fixed by the PR(s)$applyHint"
}

if ($PrNumber -gt 0) {
    $report = Invoke-AnalyzeSinglePr $PrNumber $Tag $RepoPath
    Write-Report $report
    if ($Output) { Save-ReportJson $report $Output }
    Invoke-ApplyCorrections $report $Apply.IsPresent
    if ($CreateIssue -and $report.Corrections.Count -gt 0) { New-GitHubIssue $report $Apply.IsPresent }
}
elseif ($Tag) {
    $report = Invoke-AnalyzeRelease $Tag $PreviousTag $RepoPath
    Write-Report $report
    $outPath = if ($Output) { $Output } else { "milestone-drift-$($Tag -replace '\.','_').json" }
    Save-ReportJson $report $outPath
    Invoke-ApplyCorrections $report $Apply.IsPresent
    if ($CreateIssue -and $report.Corrections.Count -gt 0) { New-GitHubIssue $report $Apply.IsPresent }
    if ($report.Errors.Count -gt 0 -and $report.PrsChecked -eq 0) {
        throw "Release analysis failed: $($report.Errors.Count) errors, 0 PRs checked."
    }
}
else {
    Write-Host "Error: -PrNumber or -Tag is required." -ForegroundColor Red
    Get-Help $MyInvocation.MyCommand.Path -Detailed
    exit 1
}

#endregion
