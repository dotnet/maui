#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Assesses release readiness of a .NET MAUI Servicing Release (SR) branch.

.DESCRIPTION
    Produces a deterministic, evidence-backed answer to "Is release/X.Y.Zxx-srN
    ready to ship?" by:

      1. Computing what is NEW in the SR (commits + source PR refs + reverts)
      2. Querying open `regressed-in-*` issues, walking timelines, and classifying
         each candidate fix PR against SR contents
      3. Querying CI pipelines on the SR branch with freshness check

    All conclusions carry evidence (commit SHAs, PR numbers, ancestry checks)
    and confidence levels. See references/methodology.md for the algorithms
    and the three critical gotchas the skill encodes.

.PARAMETER SrBranch
    SR branch name (e.g. release/10.0.1xx-sr7). Required.

.PARAMETER RegressionLabels
    Comma-separated `regressed-in-*` label names. Required unless
    -InferRegressionLabels is set.

.PARAMETER InferRegressionLabels
    Auto-derive labels from the SR's version family. Agent should ALWAYS
    confirm the inferred labels with the user before using for automation.

.PARAMETER Repo
    Repository in owner/name form. Default: dotnet/maui.

.PARAMETER MainBranch
    Stable branch used for ancestry checks. Default: main.

.PARAMETER ExcludeBranches
    Comma-separated branches to exclude when computing SR-only commits.
    Default: origin/main. Do NOT add inflight/* refs — SR branches cut from
    main; comparing against inflight produces wrong "what's shipping" answers.

.PARAMETER Candidate
    Pre-flight / candidate mode. Use when the next SR branch doesn't exist
    yet but you want to know "what WOULD ship in SRn+1 if cut from main
    today?". With -Candidate, the script treats `origin/$MainBranch` as the
    SR-to-be and uses the named -SrBranch as the prior-SR exclude baseline.

.PARAMETER InheritFromPriorSr
    Only valid with -Candidate. Models the dotnet/maui release workflow where
    SRn+1 is cut from main AND then has SRn merged into it. The "what's
    shipping" set = (main commits since prior SR) ∪ (prior SR-only commits).
    Without this flag, candidate mode shows only main-since-priorSR.

.PARAMETER Phase
    Which phase to run: all (default), ci, commits, regressions, open-prs.

.PARAMETER OutputDir
    Directory for output files. If unset, prints to stdout.

.PARAMETER OutputFormat
    json, markdown, or both (default).

.PARAMETER MaxIssues
    Cap on regression issues to walk. Default: 100.

.PARAMETER NoFetch
    Skip `git fetch`. Use for re-runs with cached refs.

.EXAMPLE
    pwsh ./Get-ReleaseReadiness.ps1 -SrBranch release/10.0.1xx-sr7 `
        -RegressionLabels regressed-in-10.0.60,regressed-in-10.0.70 `
        -OutputDir /tmp/sr7

.EXAMPLE
    pwsh ./Get-ReleaseReadiness.ps1 -SrBranch release/10.0.1xx-sr7 -Phase commits

.EXAMPLE
    # Pre-flight: what would SR8 contain if cut from main today?
    pwsh ./Get-ReleaseReadiness.ps1 -SrBranch release/10.0.1xx-sr7 -Candidate `
        -RegressionLabels regressed-in-10.0.70,regressed-in-10.0.80 `
        -OutputDir /tmp/sr8-candidate

.EXAMPLE
    # Pre-flight SR8 modeling the SR7→SR8 merge workflow
    pwsh ./Get-ReleaseReadiness.ps1 -SrBranch release/10.0.1xx-sr7 -Candidate `
        -InheritFromPriorSr `
        -RegressionLabels regressed-in-10.0.70,regressed-in-10.0.80 `
        -OutputDir /tmp/sr8-candidate
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$SrBranch,
    [string]$RegressionLabels,
    [switch]$InferRegressionLabels,
    [string]$Repo = 'dotnet/maui',
    [string]$MainBranch = 'main',
    [string]$ExcludeBranches = 'origin/main',
    [ValidateSet('all', 'ci', 'commits', 'regressions', 'open-prs')]
    [string]$Phase = 'all',
    [string]$OutputDir,
    [ValidateSet('json', 'markdown', 'both')]
    [string]$OutputFormat = 'both',
    [int]$MaxIssues = 100,
    [switch]$NoFetch,
    # Candidate / pre-flight mode: survey what WOULD ship in the next SR if cut
    # from main today. Requires -SrBranch to be the prior SR (used as the
    # exclude baseline). Treats origin/main as the "SR-to-be".
    [switch]$Candidate,
    # When set in -Candidate mode, model the dotnet/maui workflow where, after
    # cutting SRn+1 from main, the prior SR (-SrBranch) is merged in. The
    # candidate's "what's shipping" set = main-since-priorSR ∪ priorSR-only commits.
    # Without this flag, candidate mode shows only main-since-priorSR.
    [switch]$InheritFromPriorSr
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# DETERMINISTIC RULE — SR branches in dotnet/maui ALWAYS cut from `main`.
# Refuse to operate on any `inflight/*` or `staging/*` ref — those are
# integration branches, not SR sources. This guard exists because conflating
# the two leads to wrong "what's shipping" conclusions.
$Script:ForbiddenSrPatterns = @(
    '^inflight/'   # inflight/current, inflight/candidate, inflight/ai — NOT SR sources
    '^staging/'    # any staging area
    '^backport/'   # in-progress backport branches
)

# Public AzDO MAUI pipelines on dnceng-public
$Script:PublicPipelines = @(
    @{ Name = 'maui-pr';              DefinitionId = 302; Org = 'dnceng-public'; Project = 'public' }
    @{ Name = 'maui-pr-devicetests';  DefinitionId = 314; Org = 'dnceng-public'; Project = 'public' }
    @{ Name = 'maui-pr-uitests';      DefinitionId = 313; Org = 'dnceng-public'; Project = 'public' }
)
# Internal signed build (best-effort — requires AzDO auth)
$Script:InternalPipelines = @(
    @{ Name = 'dotnet-maui';          DefinitionId = 1095; Org = 'dnceng'; Project = 'internal' }
)

$Script:Warnings = [System.Collections.Generic.List[string]]::new()

function Write-Warn([string]$msg) {
    $Script:Warnings.Add($msg) | Out-Null
    Write-Host "warn: $msg" -ForegroundColor Yellow
}

function Invoke-Git([string]$Cmd) {
    $argList = $Cmd -split ' ' | Where-Object { $_ -ne '' }
    $out = & git @argList 2>$null
    if ($LASTEXITCODE -ne 0) { return $null }
    return $out
}

function Invoke-Gh([string[]]$GhArgs) {
    $errFile = [System.IO.Path]::GetTempFileName()
    try {
        $out = & gh @GhArgs 2>$errFile
        $exitCode = $LASTEXITCODE
        if ($exitCode -ne 0) {
            $err = Get-Content $errFile -Raw -ErrorAction SilentlyContinue
            Write-Warn "gh $($GhArgs -join ' ') exited $exitCode : $err"
            return $null
        }
        return $out
    } finally {
        if (Test-Path $errFile) { Remove-Item $errFile -ErrorAction SilentlyContinue }
    }
}

# region ────────────────────── 1. CONTEXT RESOLUTION ──────────────────────

function Resolve-Context {
    param([string]$SrBranch, [string]$Repo, [string]$MainBranch,
          [string[]]$ExcludeBranches, [switch]$NoFetch, [switch]$Candidate,
          [switch]$InheritFromPriorSr)

    if ($InheritFromPriorSr -and -not $Candidate) {
        throw "-InheritFromPriorSr is only valid with -Candidate (it models the SR cut-then-merge workflow)."
    }

    # HARD VALIDATION — refuse inflight/staging refs as SR sources.
    # See $Script:ForbiddenSrPatterns at top of file for the rule rationale.
    foreach ($pat in $Script:ForbiddenSrPatterns) {
        if ($SrBranch -match $pat) {
            throw "REFUSED: '$SrBranch' is not a valid SR branch — SR branches in dotnet/maui cut from `main`, never from inflight/staging/backport refs. Use a `release/X.Y.Zxx-srN` branch, or pass -Candidate to pre-flight `main`."
        }
    }
    foreach ($eb in $ExcludeBranches) {
        $stripped = $eb -replace '^origin/', ''
        foreach ($pat in $Script:ForbiddenSrPatterns) {
            if ($stripped -match $pat) {
                Write-Warn "Exclude branch '$eb' is an inflight/staging ref — dropping. SR contents should only be compared against main or another SR branch."
                $ExcludeBranches = $ExcludeBranches | Where-Object { $_ -ne $eb }
            }
        }
    }

    if (-not $NoFetch) {
        Write-Host "Fetching latest refs..." -ForegroundColor Cyan
        & git fetch --all --quiet 2>$null | Out-Null
    }

    # Candidate mode: swap roles — main becomes the "SR-to-be", named SrBranch
    # becomes the exclude baseline (prior SR). This lets us answer "what would
    # SRn+1 contain if cut today?" without requiring the branch to exist yet.
    $mode = 'shipped'
    $effectiveSrRef = "origin/$SrBranch"
    $effectiveExcludes = $ExcludeBranches

    if ($Candidate) {
        $mode = 'candidate'
        $priorSrRef = "origin/$SrBranch"
        $priorSrSha = Invoke-Git "rev-parse $priorSrRef"
        if (-not $priorSrSha) {
            throw "Candidate mode requires -SrBranch to be the prior SR (used as exclude baseline). '$priorSrRef' not found."
        }
        $effectiveSrRef = "origin/$MainBranch"
        # Exclude prior SR from main, so we see only "new since last SR" commits
        $effectiveExcludes = @($priorSrRef)
        Write-Host "Candidate mode: surveying $effectiveSrRef vs prior SR $priorSrRef" -ForegroundColor Cyan
    }

    if ($Candidate -and $InheritFromPriorSr) {
        Write-Host "  -InheritFromPriorSr active: SR-to-be contents will be augmented with $priorSrRef-only commits" -ForegroundColor Cyan
    }

    $srHead = Invoke-Git "rev-parse $effectiveSrRef"
    if (-not $srHead) {
        throw "Branch '$effectiveSrRef' not found. Did you push it? (try without -NoFetch)"
    }
    $srSubject = Invoke-Git "log -1 --format=%s $effectiveSrRef"

    $mainHead = Invoke-Git "rev-parse origin/$MainBranch"
    if (-not $mainHead) { Write-Warn "Main branch 'origin/$MainBranch' not found" }

    # Validate exclude branches exist; drop missing with warning
    $validExcludes = @()
    foreach ($b in $effectiveExcludes) {
        $sha = Invoke-Git "rev-parse $b"
        if ($sha) {
            $validExcludes += $b
        } else {
            Write-Warn "Exclude branch '$b' not found, dropping"
        }
    }

    @{
        repo = $Repo
        srBranch = if ($Candidate) { $MainBranch } else { $SrBranch }
        srRef = $effectiveSrRef
        srHeadSha = $srHead
        srHeadSubject = $srSubject
        mainBranch = $MainBranch
        mainHeadSha = $mainHead
        excludeBranches = $validExcludes
        mode = $mode
        priorSrBranch = if ($Candidate) { $SrBranch } else { $null }
        priorSrRef = if ($Candidate) { "origin/$SrBranch" } else { $null }
        inheritFromPriorSr = [bool]($Candidate -and $InheritFromPriorSr)
        fetchedAt = (Get-Date).ToUniversalTime().ToString('o')
    }
}

# region ────────────────────── 2. SR COMMITS + SOURCE PR EXTRACTION ───────

# Internal scanner — extracts source PRs / backports / reverts from commits
# selected by an arbitrary `git log` rev-spec. Used by Get-SrCommits both for
# the primary scan and (optionally) for the inherited-from-prior-SR scan.
function Get-CommitsForRevSpec {
    param(
        [string]$RevSpec,           # e.g. "origin/main ^origin/release/10.0.1xx-sr7"
        [string]$OriginTag = 'primary'
    )

    $shaList = Invoke-Git "log --format=%H $RevSpec"
    if (-not $shaList) {
        return @{
            commits = @(); sourcePrs = @(); backportPrs = @();
            reverts = @(); fixedIssues = @()
        }
    }
    $shas = @($shaList)

    $commits = @()
    $allSourcePrs = New-Object 'System.Collections.Generic.HashSet[int]'
    $allBackportPrs = New-Object 'System.Collections.Generic.HashSet[int]'
    $reverts = @()
    $fixedIssues = New-Object 'System.Collections.Generic.HashSet[int]'

    foreach ($sha in $shas) {
        $raw = Invoke-Git "show --no-patch --format=%H%n%an%n%aI%n%s%n--BODY-START--%n%b $sha"
        if (-not $raw) { continue }
        $lines = @($raw)
        $cmtSha = $lines[0]
        $author = $lines[1]
        $authorDate = $lines[2]
        $subject = $lines[3]
        $bodyStartIdx = [Array]::IndexOf($lines, '--BODY-START--')
        $body = if ($bodyStartIdx -ge 0 -and $bodyStartIdx -lt $lines.Count - 1) {
            ($lines[($bodyStartIdx + 1)..($lines.Count - 1)] -join "`n")
        } else { '' }

        # Backport PR: last "(#NNNN)" in subject
        $backportPr = $null
        $subjMatches = [regex]::Matches($subject, '\(#(\d+)\)')
        if ($subjMatches.Count -gt 0) {
            $backportPr = [int]$subjMatches[$subjMatches.Count - 1].Groups[1].Value
            $allBackportPrs.Add($backportPr) | Out-Null
            $allSourcePrs.Add($backportPr) | Out-Null   # greedy: backport # also resolves
        }

        # Source PR strong signal: "Backport of #NNNN" / "cherry picked from PR #NNNN"
        $sourcePr = $null
        $sourceMatch = [regex]::Match($body, '(?im)(?:backport\s+of|cherry[-\s]picked\s+from(?:\s+PR)?)\s+#(\d+)')
        if ($sourceMatch.Success) {
            $sourcePr = [int]$sourceMatch.Groups[1].Value
            $allSourcePrs.Add($sourcePr) | Out-Null
        }

        # cherry-pick source SHA: "(cherry picked from commit <sha>)"
        $cherrySourceSha = $null
        $cherryShaMatch = [regex]::Match($body, '(?im)cherry\s+picked\s+from\s+commit\s+([0-9a-f]{7,40})')
        if ($cherryShaMatch.Success) { $cherrySourceSha = $cherryShaMatch.Groups[1].Value }

        # Fixed issues
        $issMatches = [regex]::Matches($body, '(?im)(?:fixes|closes|resolves)\s+(?:dotnet/maui#|#)(\d+)')
        $fixesList = @()
        foreach ($m in $issMatches) {
            $n = [int]$m.Groups[1].Value
            $fixesList += $n
            $fixedIssues.Add($n) | Out-Null
        }

        # Revert detection — matches "Revert ", "[Revert]", or "[branch-prefix] Revert ..."
        $isRevert = ($subject -match '(?i)^(?:\[[^\]]+\]\s+)?Revert\b') -or ($subject -match '\[Revert\]')
        $revertsCommit = $null
        $revertsPr = $null
        if ($isRevert) {
            $revM = [regex]::Match($body, '(?im)This reverts commit\s+([0-9a-f]{7,40})')
            if ($revM.Success) { $revertsCommit = $revM.Groups[1].Value }

            # Try to recover the original PR number from the reverted subject (it'll be at end as (#NNNN))
            # OR from explicit "Revert PR #NNNN" mentions
            $revPrM = [regex]::Match($subject, 'Revert\s+PR\s+#(\d+)|Revert.*\(#(\d+)\)')
            if ($revPrM.Success) {
                $revertsPr = if ($revPrM.Groups[1].Success) {
                    [int]$revPrM.Groups[1].Value
                } else {
                    [int]$revPrM.Groups[2].Value
                }
            }
            # If we have the reverted commit SHA, look up its subject and extract (#NNNN)
            if (-not $revertsPr -and $revertsCommit) {
                $revSubj = Invoke-Git "log -1 --format=%s $revertsCommit"
                if ($revSubj) {
                    $rsM = [regex]::Matches($revSubj, '\(#(\d+)\)')
                    if ($rsM.Count -gt 0) {
                        $revertsPr = [int]$rsM[$rsM.Count - 1].Groups[1].Value
                    }
                }
            }
            $reverts += @{
                revertCommit = $cmtSha
                revertsCommit = $revertsCommit
                revertsPr = $revertsPr
                revertBackportPr = $backportPr
                origin = $OriginTag
            }
        }

        $commits += @{
            sha = $cmtSha
            author = $author
            date = $authorDate
            subject = $subject
            isRevert = $isRevert
            backportPr = $backportPr
            sourcePr = $sourcePr
            cherrySourceSha = $cherrySourceSha
            fixedIssues = $fixesList
            origin = $OriginTag
        }
    }

    @{
        commits = $commits
        sourcePrs = @($allSourcePrs)
        backportPrs = @($allBackportPrs)
        reverts = $reverts
        fixedIssues = @($fixedIssues)
    }
}

function Get-SrCommits {
    param($Ctx)

    Write-Host "Computing SR-only commits..." -ForegroundColor Cyan
    $excludeArgs = $Ctx.excludeBranches | ForEach-Object { "^$_" }
    $primaryRevSpec = "$($Ctx.srRef) $($excludeArgs -join ' ')"
    $primary = Get-CommitsForRevSpec -RevSpec $primaryRevSpec -OriginTag 'primary'
    Write-Host "  Found $($primary.commits.Count) primary SR commits" -ForegroundColor Gray

    $inherited = $null
    if ($Ctx.inheritFromPriorSr -and $Ctx.priorSrRef) {
        # Inheritance set: commits on prior SR that are NOT yet on main.
        # When the SR-to-be (main today) has the prior SR merged in, these are
        # the additional shipping commits.
        Write-Host "Computing prior-SR-only commits ($($Ctx.priorSrRef) not in $($Ctx.srRef))..." -ForegroundColor Cyan
        $inheritRevSpec = "$($Ctx.priorSrRef) ^$($Ctx.srRef)"
        $inherited = Get-CommitsForRevSpec -RevSpec $inheritRevSpec -OriginTag 'inherited'
        Write-Host "  Found $($inherited.commits.Count) inherited-from-prior-SR commits" -ForegroundColor Gray
    }

    # Merge primary + inherited into a single SR-contents view.
    # We keep an `origin` tag on each item so the report can disambiguate.
    $mergedCommits = @($primary.commits)
    $mergedReverts = @($primary.reverts)
    $sourcePrSet = New-Object 'System.Collections.Generic.HashSet[int]'
    foreach ($n in $primary.sourcePrs) { $sourcePrSet.Add([int]$n) | Out-Null }
    $backportPrSet = New-Object 'System.Collections.Generic.HashSet[int]'
    foreach ($n in $primary.backportPrs) { $backportPrSet.Add([int]$n) | Out-Null }
    $fixedIssueSet = New-Object 'System.Collections.Generic.HashSet[int]'
    foreach ($n in $primary.fixedIssues) { $fixedIssueSet.Add([int]$n) | Out-Null }

    if ($inherited) {
        $mergedCommits += $inherited.commits
        $mergedReverts += $inherited.reverts
        foreach ($n in $inherited.sourcePrs) { $sourcePrSet.Add([int]$n) | Out-Null }
        foreach ($n in $inherited.backportPrs) { $backportPrSet.Add([int]$n) | Out-Null }
        foreach ($n in $inherited.fixedIssues) { $fixedIssueSet.Add([int]$n) | Out-Null }
    }

    $srcPrsSorted = @($sourcePrSet | Sort-Object)
    $result = @{
        commitCount = $mergedCommits.Count
        primaryCommitCount = $primary.commits.Count
        inheritedCommitCount = if ($inherited) { $inherited.commits.Count } else { 0 }
        commits = $mergedCommits
        sourcePrs = $srcPrsSorted
        sourcePrCount = $srcPrsSorted.Count
        primarySourcePrs = @($primary.sourcePrs | Sort-Object)
        inheritedSourcePrs = if ($inherited) { @($inherited.sourcePrs | Sort-Object) } else { @() }
        backportPrs = @($backportPrSet | Sort-Object)
        fixedIssues = @($fixedIssueSet | Sort-Object)
        reverts = $mergedReverts
    }
    return $result
}

# region ────────────────────── 3. CI STATUS ───────────────────────────────

function Get-PipelineLatestBuilds {
    param($Pipeline, [string]$SrBranch, [string]$SrHead)

    $org = $Pipeline.Org
    $project = $Pipeline.Project
    $defId = $Pipeline.DefinitionId
    $branchSpec = "refs/heads/$SrBranch"

    $url = "https://dev.azure.com/$org/$project/_apis/build/builds?definitions=$defId&branchName=$branchSpec&`$top=5&api-version=7.1"
    try {
        $resp = curl -s -L --max-time 30 "$url" 2>$null
        if (-not $resp) { return $null }
        $obj = $resp | ConvertFrom-Json -ErrorAction Stop
        if (-not $obj.value) { return $null }
        return $obj.value
    } catch {
        Write-Warn "Failed to query pipeline $($Pipeline.Name): $_"
        return $null
    }
}

function Get-CIStatus {
    param($Ctx)

    Write-Host "Querying CI pipelines..." -ForegroundColor Cyan
    $results = @()
    $allPipelines = $Script:PublicPipelines + $Script:InternalPipelines

    foreach ($p in $allPipelines) {
        $builds = Get-PipelineLatestBuilds -Pipeline $p -SrBranch $Ctx.srBranch -SrHead $Ctx.srHeadSha
        if (-not $builds) {
            $results += @{
                name = $p.Name; definitionId = $p.DefinitionId
                verdict = 'unknown'; latestBuild = $null
                url = "https://dev.azure.com/$($p.Org)/$($p.Project)/_build?definitionId=$($p.DefinitionId)&branchFilter=$($Ctx.srBranch)"
                note = 'Could not query (auth or outage)'
            }
            continue
        }

        $latest = $builds | Select-Object -First 1
        $sourceSha = $latest.sourceVersion
        $isAtOrAhead = $false
        if ($sourceSha -and $Ctx.srHeadSha) {
            # Is SR HEAD an ancestor of (or equal to) the build's source SHA?
            $check = Invoke-Git "merge-base --is-ancestor $($Ctx.srHeadSha) $sourceSha"
            $isAtOrAhead = ($LASTEXITCODE -eq 0)
            if (-not $isAtOrAhead -and $sourceSha -eq $Ctx.srHeadSha) { $isAtOrAhead = $true }
        }

        $verdict = if (-not $isAtOrAhead) {
            'stale'
        } elseif ($latest.result -eq 'succeeded') {
            'green'
        } elseif ($latest.result -eq 'partiallySucceeded') {
            'red-needs-review'
        } elseif ($latest.result -eq 'failed') {
            'red-needs-review'  # downstream agent classifies known-flakes vs new
        } else {
            'unknown'
        }

        $results += @{
            name = $p.Name; definitionId = $p.DefinitionId
            verdict = $verdict
            latestBuild = @{
                id = $latest.id
                buildNumber = $latest.buildNumber
                result = $latest.result
                status = $latest.status
                sourceSha = $sourceSha
                isAtOrAheadOfSrHead = $isAtOrAhead
                completedAt = $latest.finishTime
                url = $latest._links.web.href
            }
            recentBuilds = @($builds | Select-Object -First 5 | ForEach-Object {
                @{ id = $_.id; result = $_.result; sourceSha = $_.sourceVersion; completedAt = $_.finishTime }
            })
            url = "https://dev.azure.com/$($p.Org)/$($p.Project)/_build?definitionId=$($p.DefinitionId)&branchFilter=$($Ctx.srBranch)"
        }
    }

    # Overall verdict
    $overall = 'green'
    foreach ($r in $results) {
        if ($r.verdict -eq 'stale') { $overall = 'stale'; break }
        if ($r.verdict -like 'red-*') { $overall = 'red-needs-review' }
        if ($r.verdict -eq 'unknown' -and $overall -eq 'green') { $overall = 'partial-unknown' }
    }

    @{ overall = $overall; pipelines = $results }
}

# region ────────────────────── 4. REGRESSION LABEL INFERENCE ──────────────

function Get-RegressionLabelsAuto {
    param($Ctx)

    # Parse SR version from branch name: release/10.0.1xx-sr7 -> 10.0
    $branchMatch = [regex]::Match($Ctx.srBranch, '^release/(\d+)\.(\d+)\.\d+xx-sr(\d+)$')
    if (-not $branchMatch.Success) {
        return @{
            mode = 'inferred'; confidence = 'low'
            labels = @(); error = "Branch name doesn't match SR pattern; pass -RegressionLabels explicitly"
        }
    }
    $major = $branchMatch.Groups[1].Value
    $minor = $branchMatch.Groups[2].Value
    $srNum = [int]$branchMatch.Groups[3].Value

    # Query existing labels: regressed-in-{major}.{minor}.*
    $raw = Invoke-Gh @('api', "repos/$($Ctx.repo)/labels", '--paginate', '--jq',
                       ".[] | select(.name | test(`"^regressed-in-$major\\.$minor\\.\\d+$`")) | .name")
    if (-not $raw) {
        return @{ mode = 'inferred'; confidence = 'low'; labels = @();
                  error = "No regressed-in-$major.$minor.* labels found in repo" }
    }
    $allLabels = @($raw) | Sort-Object {
        # Sort by numeric patch
        [int]([regex]::Match($_, '\.(\d+)$').Groups[1].Value)
    } -Descending

    # Heuristic: take top 2 labels — covers the typical SR cycle that aggregates
    # two minor version's worth of fixes
    $picked = @($allLabels | Select-Object -First 2)

    @{
        mode = 'inferred'
        confidence = if ($picked.Count -eq 2) { 'medium' } else { 'low' }
        labels = $picked
        availableLabels = $allLabels
        note = "Inferred from SR$srNum on $major.$minor — VERIFY before treating as authoritative"
    }
}

# region ────────────────────── 5. REGRESSION CANDIDATE ANALYSIS ───────────

function Get-IssueTimelinePrs {
    param($Repo, $IssueNumber)
    $raw = Invoke-Gh @('api', "repos/$Repo/issues/$IssueNumber/timeline", '--paginate')
    if (-not $raw) { return @() }
    $events = $raw | ConvertFrom-Json -ErrorAction SilentlyContinue
    if (-not $events) { return @() }
    $prs = @()
    foreach ($e in $events) {
        # Use PSObject.Properties checks because strict mode forbids accessing
        # missing properties on PSCustomObject (timeline events have many shapes).
        if (-not $e.PSObject.Properties['event']) { continue }
        if ($e.event -ne 'cross-referenced') { continue }
        if (-not $e.PSObject.Properties['source']) { continue }
        $src = $e.source
        if (-not $src) { continue }
        if (-not $src.PSObject.Properties['type'] -or $src.type -ne 'issue') { continue }
        if (-not $src.PSObject.Properties['issue']) { continue }
        $iss = $src.issue
        if (-not $iss) { continue }
        # `pull_request` member only exists on issues that are actually PRs
        if (-not $iss.PSObject.Properties['pull_request']) { continue }
        if (-not $iss.pull_request) { continue }
        if (-not $iss.PSObject.Properties['number']) { continue }
        $prs += [int]$iss.number
    }
    return @($prs | Sort-Object -Unique)
}

function Get-PrEvidenceType {
    param($PrBody, $IssueNumber)
    if (-not $PrBody) { return 'none' }
    if ($PrBody -match "(?im)(?:fixes|closes|resolves)\s+(?:dotnet/maui#|#)$IssueNumber\b") {
        return 'closing-keyword'
    }
    if ($PrBody -match '(?im)(?:backport|cherry[-\s]picked)') {
        return 'explicit-backport'
    }
    if ($PrBody -match "#$IssueNumber\b") { return 'mentions-only' }
    return 'none'
}

function Get-PrInfo {
    param($Repo, $PrNumber)
    $json = Invoke-Gh @('pr', 'view', $PrNumber, '--repo', $Repo, '--json',
        'number,title,state,baseRefName,mergedAt,closedAt,body,mergeCommit,author,labels,isDraft')
    if (-not $json) { return $null }
    return ($json | ConvertFrom-Json -ErrorAction SilentlyContinue)
}

function Test-CommitOnBranch {
    param([string]$Sha, [string]$BranchRef)
    if (-not $Sha) { return $false }
    Invoke-Git "merge-base --is-ancestor $Sha $BranchRef" | Out-Null
    return ($LASTEXITCODE -eq 0)
}

function Get-BackportPrsForSr {
    param($Repo, $SrBranch, $SourcePrNumber)
    # Look for any PR targeting the SR branch that mentions the source PR
    $raw = Invoke-Gh @('pr', 'list', '--repo', $Repo, '--base', $SrBranch,
                       '--state', 'all', '--search', "$SourcePrNumber in:title,body",
                       '--json', 'number,title,state,mergedAt,closedAt', '--limit', '20')
    if (-not $raw) { return @() }
    $list = $raw | ConvertFrom-Json -ErrorAction SilentlyContinue
    return @($list)
}

function Classify-RegressionCandidate {
    param($Issue, $CandidatePrs, $Ctx, $SrContents)

    $sourcePrSet = @{}
    foreach ($n in $SrContents.sourcePrs) { $sourcePrSet[$n] = $true }
    $revertedPrSet = @{}
    foreach ($r in $SrContents.reverts) {
        if ($r.revertsPr) { $revertedPrSet[$r.revertsPr] = $true }
        if ($r.revertBackportPr) { $revertedPrSet[$r.revertBackportPr] = $true }
    }

    # Filter candidates to those with high evidence for this issue
    $strongPrs = @()
    $sawRevertCandidate = $false
    foreach ($prNum in $CandidatePrs) {
        $info = Get-PrInfo -Repo $Ctx.repo -PrNumber $prNum
        if (-not $info) { continue }
        $ev = Get-PrEvidenceType -PrBody $info.body -IssueNumber $Issue.number
        if ($ev -ne 'closing-keyword' -and $ev -ne 'explicit-backport') { continue }

        # Skip PRs that target SR branches (those are backport PRs themselves — examined separately)
        if ($info.baseRefName -like 'release/*') { continue }

        # Detect "Revert ..." titled PRs — these are NOT fixes, they're rollbacks.
        # When the only candidate PR is a revert, the issue is likely unfixed (or
        # in a revert-of-revert chain that needs manual verification).
        $isRevertPr = ($info.title -match '(?i)^(?:\[[^\]]+\]\s+)?Revert\b') -or ($info.title -match '\[Revert\]')
        if ($isRevertPr) { $sawRevertCandidate = $true; continue }

        $mergeSha = if ($info.mergeCommit) { $info.mergeCommit.oid } else { $null }
        $onMain = if ($mergeSha) { Test-CommitOnBranch -Sha $mergeSha -BranchRef "origin/$($Ctx.mainBranch)" } else { $false }

        # Look for backport PRs targeting SR
        $backports = Get-BackportPrsForSr -Repo $Ctx.repo -SrBranch $Ctx.srBranch -SourcePrNumber $prNum

        $strongPrs += @{
            number = [int]$info.number
            title = $info.title
            state = $info.state
            baseRef = $info.baseRefName
            mergeSha = $mergeSha
            mergedAt = $info.mergedAt
            evidenceType = $ev
            onMain = $onMain
            backports = @($backports | ForEach-Object {
                @{ number = $_.number; state = $_.state; mergedAt = $_.mergedAt; closedAt = $_.closedAt; title = $_.title }
            })
        }
    }

    if ($strongPrs.Count -eq 0) {
        if ($sawRevertCandidate) {
            return @{
                classification = 'needs-human-review'
                confidence = 'medium'
                evidence = @('All candidate fix PRs were Revert PRs — original fix may be missing or in a revert-of-revert chain. Manual verification required.')
                candidateFixPrs = @()
                recommendedAction = "Inspect the revert chain manually: original fix → revert → (possible) revert-of-revert. Look for the actual fix PR in `gh pr list --search 'fixes #$($Issue.number)'` excluding revert titles."
            }
        }
        return @{
            classification = 'no-fix-yet'
            confidence = 'high'
            evidence = @('no candidate PRs with closing-keyword or explicit-backport evidence')
            candidateFixPrs = @()
            recommendedAction = 'Investigate: no fix PR cross-referenced from issue'
        }
    }

    # Classify each strong PR; aggregate to issue-level verdict
    $perPrVerdicts = @()
    foreach ($pr in $strongPrs) {
        $verdict = $null
        $evidence = @()

        # In-SR (with revert check)
        if ($sourcePrSet.ContainsKey($pr.number)) {
            if ($revertedPrSet.ContainsKey($pr.number)) {
                $verdict = 'in-sr-reverted'
                $evidence += "PR #$($pr.number) source-PR in SR but reverted"
            } else {
                $verdict = 'in-sr-active'
                $evidence += "PR #$($pr.number) source-PR in SR contents (active)"
            }
        }
        else {
            # Look at backport PRs targeting SR
            $openBackport = $pr.backports | Where-Object { $_.state -eq 'OPEN' } | Select-Object -First 1
            $closedUnmergedBackport = $pr.backports | Where-Object { $_.state -eq 'CLOSED' -and -not $_.mergedAt } | Select-Object -First 1
            $mergedBackport = $pr.backports | Where-Object { $_.state -eq 'MERGED' } | Select-Object -First 1

            if ($mergedBackport) {
                # backport landed but PR # is different from what we tracked → check sourcePrSet for backport #
                if ($sourcePrSet.ContainsKey([int]$mergedBackport.number)) {
                    if ($revertedPrSet.ContainsKey([int]$mergedBackport.number)) {
                        $verdict = 'in-sr-reverted'
                        $evidence += "Backport PR #$($mergedBackport.number) in SR but reverted"
                    } else {
                        $verdict = 'in-sr-active'
                        $evidence += "Backport PR #$($mergedBackport.number) in SR (active)"
                    }
                } else {
                    $verdict = 'in-sr-active'
                    $evidence += "Backport PR #$($mergedBackport.number) marked merged (SR contents may need refresh)"
                }
            }
            elseif ($openBackport) {
                $verdict = 'backport-in-progress'
                $evidence += "Backport PR #$($openBackport.number) is OPEN against $($Ctx.srBranch)"
            }
            elseif ($closedUnmergedBackport) {
                $verdict = 'rejected-from-sr'
                $evidence += "Backport PR #$($closedUnmergedBackport.number) CLOSED unmerged — needs WorkIQ for context"
            }
            elseif ($pr.state -eq 'MERGED') {
                if ($pr.onMain) {
                    $verdict = 'merged-on-main-no-backport'
                    $evidence += "PR #$($pr.number) merged to main, no backport PR opened"
                } else {
                    $verdict = 'merged-non-main-only'
                    $evidence += "PR #$($pr.number) merged but NOT on main (likely inflight-only)"
                }
            }
            elseif ($pr.state -eq 'OPEN') {
                $verdict = 'open-on-main'
                $evidence += "PR #$($pr.number) is OPEN, base=$($pr.baseRef)"
            }
            else {
                $verdict = 'needs-human-review'
                $evidence += "PR #$($pr.number) in unexpected state: $($pr.state)"
            }
        }

        $perPrVerdicts += @{ pr = $pr; verdict = $verdict; evidence = $evidence }
    }

    # Pick the highest-priority verdict (in-sr-active > backport-in-progress > ... > no-fix-yet)
    $priority = @{
        'in-sr-active' = 1
        'in-sr-reverted' = 2
        'backport-in-progress' = 3
        'rejected-from-sr' = 4
        'merged-on-main-no-backport' = 5
        'merged-non-main-only' = 6
        'open-on-main' = 7
        'needs-human-review' = 8
        'no-fix-yet' = 9
    }
    $best = $perPrVerdicts | Sort-Object { $priority[$_.verdict] } | Select-Object -First 1

    $recAction = switch ($best.verdict) {
        'in-sr-active' { 'No action — fix is shipping' }
        'in-sr-reverted' { 'Investigate: backport landed and was reverted on SR' }
        'rejected-from-sr' { 'Check rejection rationale (WorkIQ) — was this intentional or stale?' }
        'backport-in-progress' { 'Track backport PR to completion' }
        'merged-on-main-no-backport' { 'Open a backport PR to SR' }
        'merged-non-main-only' { 'Flow fix to main first, then backport to SR' }
        'open-on-main' { 'Wait for main merge, then open backport' }
        'no-fix-yet' { 'No fix exists — investigate priority' }
        default { 'Manual review required' }
    }

    @{
        classification = $best.verdict
        confidence = 'high'
        evidence = $best.evidence
        candidateFixPrs = @($strongPrs | ForEach-Object { @{
            number = $_.number; title = $_.title; state = $_.state
            baseRef = $_.baseRef; evidenceType = $_.evidenceType
            onMain = $_.onMain; backports = $_.backports
        }})
        recommendedAction = $recAction
    }
}

function Get-RegressionCandidates {
    param($Ctx, $Labels, $SrContents, [int]$MaxIssues)

    Write-Host "Scanning regression issues for labels: $($Labels -join ', ')" -ForegroundColor Cyan
    $allIssues = @()
    $seen = @{}

    foreach ($label in $Labels) {
        $raw = Invoke-Gh @('issue', 'list', '--repo', $Ctx.repo, '--label', $label,
                           '--state', 'all', '--limit', $MaxIssues.ToString(),
                           '--json', 'number,title,state,labels,milestone,createdAt')
        if (-not $raw) { continue }
        $list = $raw | ConvertFrom-Json -ErrorAction SilentlyContinue
        foreach ($iss in $list) {
            if (-not $seen.ContainsKey($iss.number)) {
                $seen[$iss.number] = $true
                $allIssues += $iss
            }
        }
    }
    Write-Host "  Found $($allIssues.Count) unique regression issues" -ForegroundColor Gray

    $results = @()
    $i = 0
    foreach ($iss in $allIssues) {
        $i++
        Write-Host "  [$i/$($allIssues.Count)] Issue #$($iss.number)..." -ForegroundColor DarkGray
        $candidatePrs = Get-IssueTimelinePrs -Repo $Ctx.repo -IssueNumber $iss.number
        $classify = Classify-RegressionCandidate -Issue $iss -CandidatePrs $candidatePrs `
                        -Ctx $Ctx -SrContents $SrContents

        $results += @{
            issue = [int]$iss.number
            title = $iss.title
            state = $iss.state
            labels = @($iss.labels.name)
            milestone = if ($iss.milestone) { $iss.milestone.title } else { $null }
            createdAt = $iss.createdAt
            classification = $classify.classification
            confidence = $classify.confidence
            evidence = $classify.evidence
            candidateFixPrs = $classify.candidateFixPrs
            recommendedAction = $classify.recommendedAction
        }
    }
    return $results
}

# region ────────────────────── 6. OPEN SR-TARGETING PRs ───────────────────

function Get-OpenSrPrs {
    param($Ctx)
    Write-Host "Listing open PRs targeting $($Ctx.srBranch)..." -ForegroundColor Cyan
    $raw = Invoke-Gh @('pr', 'list', '--repo', $Ctx.repo, '--base', $Ctx.srBranch,
                       '--state', 'open', '--limit', '100',
                       '--json', 'number,title,author,isDraft,createdAt,updatedAt,labels,reviewDecision')
    if (-not $raw) { return @() }
    return @($raw | ConvertFrom-Json -ErrorAction SilentlyContinue)
}

# region ────────────────────── 7. MARKDOWN REPORT ─────────────────────────

function Format-MarkdownReport {
    param($Data)

    $ctx = $Data.metadata
    $srBranch = $ctx.srBranch
    $shortHead = if ($ctx.srHeadSha) { $ctx.srHeadSha.Substring(0, 8) } else { '?' }

    $sb = [System.Text.StringBuilder]::new()
    $mode = if ($ctx.ContainsKey('mode')) { $ctx['mode'] } else { 'shipped' }
    $inherits = ($ctx.ContainsKey('inheritFromPriorSr') -and $ctx['inheritFromPriorSr'])
    if ($mode -eq 'candidate') {
        if ($inherits) {
            [void]$sb.AppendLine("# Release Readiness — CANDIDATE for next SR (main + inherited from $($ctx.priorSrBranch))")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine("> 🛫 **Pre-flight mode (cut-then-merge).** Surveying ``$srBranch`` (== main) PLUS commits inherited from prior SR ``$($ctx.priorSrBranch)`` (the SR will be cut from main, then have the prior SR merged into it).")
        } else {
            [void]$sb.AppendLine("# Release Readiness — CANDIDATE for next SR (vs $($ctx.priorSrBranch))")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine("> 🛫 **Pre-flight mode.** Surveying ``$srBranch`` (== main) against prior SR ``$($ctx.priorSrBranch)``. Shows what WOULD ship if we cut the next SR today.")
        }
    } else {
        [void]$sb.AppendLine("# Release Readiness — $srBranch")
    }
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("**HEAD**: ``$shortHead`` — $($ctx.srHeadSubject)")
    [void]$sb.AppendLine("**Generated**: $($ctx.fetchedAt)")
    [void]$sb.AppendLine("**Regression labels**: $($ctx.regressionLabels -join ', ') _(mode: $($ctx.labelInferenceMode))_")
    [void]$sb.AppendLine()

    if ($Data.ContainsKey('warnings') -and $Data['warnings'].Count -gt 0) {
        [void]$sb.AppendLine("> ⚠️ **Warnings:**")
        foreach ($w in $Data['warnings']) { [void]$sb.AppendLine("> - $w") }
        [void]$sb.AppendLine()
    }

    # CI section
    if ($Data.ContainsKey('ci') -and $Data['ci']) {
        $ciData = $Data['ci']
        [void]$sb.AppendLine("## CI Status — overall: ``$($ciData.overall)``")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine('| Pipeline | Verdict | Latest result | At/ahead of SR HEAD? | Build |')
        [void]$sb.AppendLine('|---|---|---|---|---|')
        foreach ($p in $ciData.pipelines) {
            $lb = $p.latestBuild
            $verdict = $p.verdict
            $result = if ($lb) { $lb.result } else { '—' }
            $fresh = if ($lb) { if ($lb.isAtOrAheadOfSrHead) { '✅' } else { '❌ stale' } } else { '—' }
            $buildLink = if ($lb -and $lb.url) { "[$($lb.id)]($($lb.url))" } else { '—' }
            [void]$sb.AppendLine("| $($p.name) | ``$verdict`` | $result | $fresh | $buildLink |")
        }
        [void]$sb.AppendLine()
    }

    # SR contents section
    if ($Data.ContainsKey('srContents') -and $Data['srContents']) {
        $sc = $Data['srContents']
        [void]$sb.AppendLine("## What's New in SR — $($sc.commitCount) commits")
        [void]$sb.AppendLine()
        if ($inherits -and $sc.ContainsKey('inheritedCommitCount') -and $sc['inheritedCommitCount'] -gt 0) {
            [void]$sb.AppendLine("- **From main** (since prior SR): $($sc.primaryCommitCount) commits / $($sc.primarySourcePrs.Count) source PRs")
            [void]$sb.AppendLine("- **Inherited from $($ctx.priorSrBranch)** (will be merged in after cut): $($sc.inheritedCommitCount) commits / $($sc.inheritedSourcePrs.Count) source PRs")
            [void]$sb.AppendLine("- **Total source PRs** (deduplicated): **$($sc.sourcePrs.Count)** (see ``sr-source-prs.txt``)")
        } else {
            [void]$sb.AppendLine("- Source PRs included: **$($sc.sourcePrs.Count)** (see ``sr-source-prs.txt``)")
        }
        [void]$sb.AppendLine("- Reverts detected: **$($sc.reverts.Count)**")
        if ($sc.reverts.Count -gt 0) {
            [void]$sb.AppendLine()
            [void]$sb.AppendLine('### Reverts')
            [void]$sb.AppendLine('| Revert commit | Reverts PR | Reverts commit | On |')
            [void]$sb.AppendLine('|---|---|---|---|')
            foreach ($r in $sc.reverts) {
                $rs = if ($r.revertCommit) { $r.revertCommit.Substring(0, 8) } else { '?' }
                $rc = if ($r.revertsCommit) { $r.revertsCommit.Substring(0, 8) } else { '?' }
                $rp = if ($r.revertsPr) { "#$($r.revertsPr)" } else { '?' }
                $ro = if ($r.ContainsKey('origin')) { $r.origin } else { '?' }
                [void]$sb.AppendLine("| ``$rs`` | $rp | ``$rc`` | $ro |")
            }
        }
        [void]$sb.AppendLine()
    }

    # Open SR-targeting PRs
    if ($Data.ContainsKey('openSrPrs') -and $Data['openSrPrs'] -and $Data['openSrPrs'].Count -gt 0) {
        [void]$sb.AppendLine("## Open PRs Targeting $srBranch — $($Data['openSrPrs'].Count)")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine('| PR | Title | Author | Draft? | Review | Updated |')
        [void]$sb.AppendLine('|---|---|---|---|---|---|')
        foreach ($pr in $Data['openSrPrs']) {
            $title = if ($pr.title.Length -gt 60) { $pr.title.Substring(0, 60) + '...' } else { $pr.title }
            $draft = if ($pr.isDraft) { '✏️' } else { '' }
            $rev = if ($pr.reviewDecision) { $pr.reviewDecision } else { '—' }
            [void]$sb.AppendLine("| [#$($pr.number)](https://github.com/$($ctx.repo)/pull/$($pr.number)) | $title | $($pr.author.login) | $draft | $rev | $($pr.updatedAt) |")
        }
        [void]$sb.AppendLine()
    }

    # Regressions section
    if ($Data.ContainsKey('regressions') -and $Data['regressions']) {
        $regs = $Data['regressions']
        $summary = if ($Data.ContainsKey('summary')) { $Data['summary'] } else { @{} }

        [void]$sb.AppendLine("## Regression Candidates — $($regs.Count) issues scanned")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine('### Summary')
        [void]$sb.AppendLine('| Verdict | Count |')
        [void]$sb.AppendLine('|---|---|')
        foreach ($k in $summary.Keys | Sort-Object) {
            [void]$sb.AppendLine("| ``$k`` | $($summary[$k]) |")
        }
        [void]$sb.AppendLine()

        # Group by verdict, present actionables first
        $tierOrder = @('rejected-from-sr', 'backport-in-progress', 'merged-on-main-no-backport',
                       'merged-non-main-only', 'open-on-main', 'no-fix-yet', 'needs-human-review',
                       'in-sr-reverted', 'in-sr-active')
        foreach ($verdict in $tierOrder) {
            $items = @($regs | Where-Object { $_.classification -eq $verdict })
            if ($items.Count -eq 0) { continue }

            [void]$sb.AppendLine("### ``$verdict`` ($($items.Count))")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine('| Issue | Title | Fix PRs | Action |')
            [void]$sb.AppendLine('|---|---|---|---|')
            foreach ($it in $items) {
                $title = if ($it.title.Length -gt 50) { $it.title.Substring(0, 50) + '...' } else { $it.title }
                $prList = @($it.candidateFixPrs | ForEach-Object { "#$($_.number)" }) -join ', '
                if (-not $prList) { $prList = '—' }
                [void]$sb.AppendLine("| [#$($it.issue)](https://github.com/$($ctx.repo)/issues/$($it.issue)) | $title | $prList | $($it.recommendedAction) |")
            }
            [void]$sb.AppendLine()
        }
    }

    return $sb.ToString()
}

# region ────────────────────── 8. ORCHESTRATOR ────────────────────────────

function Invoke-Main {
    $excludes = $ExcludeBranches -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ }
    $ctx = Resolve-Context -SrBranch $SrBranch -Repo $Repo -MainBranch $MainBranch `
                           -ExcludeBranches $excludes -NoFetch:$NoFetch -Candidate:$Candidate `
                           -InheritFromPriorSr:$InheritFromPriorSr

    # Resolve regression labels
    $labelMode = 'explicit'
    $labelInfo = $null
    if ($RegressionLabels) {
        $labels = @($RegressionLabels -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    } elseif ($InferRegressionLabels) {
        $labelInfo = Get-RegressionLabelsAuto -Ctx $ctx
        $labels = @($labelInfo.labels)
        $labelMode = "inferred ($($labelInfo.confidence))"
        if ($labels.Count -eq 0) {
            Write-Warn "Label inference produced no labels: $($labelInfo.error)"
        } else {
            Write-Host "Inferred regression labels: $($labels -join ', ')" -ForegroundColor Yellow
            Write-Host "  Confidence: $($labelInfo.confidence) — agent should confirm with user" -ForegroundColor Yellow
        }
    } else {
        # No labels, no inference: regressions phase is skipped silently
        $labels = @()
    }

    $ctx['regressionLabels'] = $labels
    $ctx['labelInferenceMode'] = $labelMode

    $data = @{
        metadata = $ctx
        warnings = @()
    }

    if ($Phase -in 'all', 'commits', 'regressions') {
        $srContents = Get-SrCommits -Ctx $ctx
        $data['srContents'] = $srContents
    }

    if ($Phase -in 'all', 'ci') {
        $data['ci'] = Get-CIStatus -Ctx $ctx
    }

    if ($Phase -in 'all', 'open-prs') {
        $data['openSrPrs'] = Get-OpenSrPrs -Ctx $ctx
    }

    if ($Phase -in 'all', 'regressions') {
        if ($labels.Count -eq 0) {
            Write-Warn "No regression labels provided/inferred; skipping regressions phase. Pass -RegressionLabels or -InferRegressionLabels."
            $data['regressions'] = @()
        } else {
            $data['regressions'] = Get-RegressionCandidates -Ctx $ctx -Labels $labels `
                                       -SrContents $data['srContents'] -MaxIssues $MaxIssues

            # Summary buckets
            $summary = @{}
            foreach ($r in $data['regressions']) {
                $k = $r.classification
                if (-not $summary.ContainsKey($k)) { $summary[$k] = 0 }
                $summary[$k] += 1
            }
            $data['summary'] = $summary
        }
    }

    $data['warnings'] = @($Script:Warnings)

    # Output
    $jsonOut = $data | ConvertTo-Json -Depth 20 -Compress:$false
    $mdOut = Format-MarkdownReport -Data $data

    if ($OutputDir) {
        if (-not (Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir | Out-Null }
        if ($OutputFormat -in 'json', 'both') {
            Set-Content -Path (Join-Path $OutputDir 'release-readiness.json') -Value $jsonOut -Encoding UTF8
        }
        if ($OutputFormat -in 'markdown', 'both') {
            Set-Content -Path (Join-Path $OutputDir 'release-readiness.md') -Value $mdOut -Encoding UTF8
        }
        if ($data.ContainsKey('srContents')) {
            $srcPrs = $data['srContents'].sourcePrs -join "`n"
            Set-Content -Path (Join-Path $OutputDir 'sr-source-prs.txt') -Value $srcPrs -Encoding UTF8

            $commitsJson = $data['srContents'] | ConvertTo-Json -Depth 10
            Set-Content -Path (Join-Path $OutputDir 'sr-commits.json') -Value $commitsJson -Encoding UTF8
        }
        Write-Host "`nWrote outputs to: $OutputDir" -ForegroundColor Green
        Get-ChildItem $OutputDir | ForEach-Object { Write-Host "  $($_.Name) ($($_.Length) bytes)" }
    } else {
        if ($OutputFormat -in 'json', 'both') { Write-Output $jsonOut }
        if ($OutputFormat -in 'markdown', 'both') { Write-Output $mdOut }
    }
}

Invoke-Main
