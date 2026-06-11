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

.PARAMETER RepoUrl
    Base URL of the repository web UI. Used to linkify commit SHAs and PR
    numbers in the markdown report. Default: https://github.com/dotnet/maui.

.PARAMETER TrackerKey
    Canonical key used to identify the corresponding tracker issue (e.g.
    `net10-sr7`). When set, the markdown report includes a hidden HTML
    comment marker `<!-- release-readiness-tracker: net10-sr7 -->` and a
    visible "Tracker: …" line so a workflow can match a single tracker
    issue per SR. Optional; omit for ad-hoc local reports.

.PARAMETER MaxBodyBytes
    Hard cap on the rendered markdown body. When the report exceeds this,
    the script truncates and appends a single-line "[Report truncated. See
    artifacts at <link>.]" message. Default: 60000 (≈60KB, well under
    GitHub's 65,536-byte issue body limit).

.EXAMPLE
    pwsh ./Get-ReleaseReadiness.ps1 -SrBranch release/10.0.1xx-sr7 `
        -RegressionLabels regressed-in-10.0.60,regressed-in-10.0.70 `
        -OutputDir CustomAgentLogsTmp/release-readiness/sr7

.EXAMPLE
    pwsh ./Get-ReleaseReadiness.ps1 -SrBranch release/10.0.1xx-sr7 -Phase commits

.EXAMPLE
    # Pre-flight: what would SR8 contain if cut from main today?
    pwsh ./Get-ReleaseReadiness.ps1 -SrBranch release/10.0.1xx-sr7 -Candidate `
        -RegressionLabels regressed-in-10.0.70,regressed-in-10.0.80 `
        -OutputDir CustomAgentLogsTmp/release-readiness/sr8-candidate

.EXAMPLE
    # Pre-flight SR8 modeling the SR7→SR8 merge workflow
    pwsh ./Get-ReleaseReadiness.ps1 -SrBranch release/10.0.1xx-sr7 -Candidate `
        -InheritFromPriorSr `
        -RegressionLabels regressed-in-10.0.70,regressed-in-10.0.80 `
        -OutputDir CustomAgentLogsTmp/release-readiness/sr8-candidate
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
    # URL base for linkifying commit SHAs and PR numbers in the markdown
    # report. Defaults to the public dotnet/maui repo; override for forks.
    [string]$RepoUrl = 'https://github.com/dotnet/maui',
    # Canonical key for the tracker issue (e.g. net10-sr7). When set, the
    # markdown report embeds tracker + idempotency markers. Optional.
    [string]$TrackerKey,
    # Body-size cap (bytes) for markdown rendering. GitHub issue body limit
    # is 65,536 bytes; default 60,000 leaves headroom for marker comments.
    [int]$MaxBodyBytes = 60000,
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

function Get-FileFromRef {
    <#
    .SYNOPSIS
        Reads a file from the local repo at the given ref. Tries `git show` first (fast,
        offline); falls back to `gh api` if the local ref isn't available.
    #>
    param([string]$Path, [string]$Ref)
    $local = Invoke-Git "show ${Ref}:${Path}"
    if ($local) { return ($local -join "`n") }

    # Strip leading origin/ for gh api ref
    $apiRef = $Ref -replace '^origin/', ''
    $encodedRef = [System.Uri]::EscapeDataString($apiRef)
    $b64 = Invoke-Gh @('api', "repos/$($script:Repo)/contents/$Path`?ref=$encodedRef",
                       '--jq', '.content')
    if (-not $b64) { return $null }
    try {
        return [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String(($b64 -replace '\s', '')))
    } catch {
        return $null
    }
}

function Get-VersionsPropsState {
    <#
    .SYNOPSIS
        Parses eng/Versions.props at $Ref and returns the version-bump state.
    .DESCRIPTION
        Returns @{ Major; Minor; Patch; PreReleaseVersionLabel; PreReleaseVersionIteration;
                   StabilizePackageVersion; FullVersion } or $null if the file is
        unreadable. FullVersion is "<Major>.<Minor>.<Patch>" — the version that this
        branch's builds would emit.
    #>
    param([string]$Ref)
    $content = Get-FileFromRef -Path 'eng/Versions.props' -Ref $Ref
    if (-not $content) { return $null }

    function _Extract([string]$xml, [string]$tag) {
        if ($xml -match "<$tag(?:\s[^>]*)?>\s*([^<]*)\s*</$tag>") { return $Matches[1].Trim() }
        return $null
    }

    $major = _Extract $content 'MajorVersion'
    $minor = _Extract $content 'MinorVersion'
    $patch = _Extract $content 'PatchVersion'
    if (-not $major -or -not $minor -or $null -eq $patch) { return $null }

    @{
        Major                       = [int]$major
        Minor                       = [int]$minor
        Patch                       = [int]$patch
        PreReleaseVersionLabel      = (_Extract $content 'PreReleaseVersionLabel')
        PreReleaseVersionIteration  = (_Extract $content 'PreReleaseVersionIteration')
        StabilizePackageVersion     = (_Extract $content 'StabilizePackageVersion')
        FullVersion                 = "$major.$minor.$patch"
    }
}

function Get-BugTemplateVersions {
    <#
    .SYNOPSIS
        Reads the version-with-bug dropdown from .github/ISSUE_TEMPLATE/bug-report.yml
        at $Ref. See Get-PreviewReadiness for the matching helper.
    #>
    param([string]$Ref)

    $yaml = Get-FileFromRef -Path '.github/ISSUE_TEMPLATE/bug-report.yml' -Ref $Ref
    if ([string]::IsNullOrWhiteSpace($yaml)) { return @() }

    $lines = $yaml -split "`n"
    $inDropdown = $false
    $inOptions = $false
    $optionsIndent = -1
    $values = New-Object System.Collections.Generic.List[string]

    foreach ($rawLine in $lines) {
        $line = $rawLine.TrimEnd("`r")
        if (-not $inDropdown) {
            if ($line -match '^\s*id:\s*version-with-bug\s*$') { $inDropdown = $true }
            continue
        }
        if (-not $inOptions) {
            if ($line -match '^(\s*)options:\s*$') {
                $inOptions = $true
                $optionsIndent = $Matches[1].Length
            }
            if ($line -match '^\s*-\s*type:\s*') { break }
            continue
        }
        if ($line -match '^(\s*)-\s+(.+?)\s*$') {
            $indent = $Matches[1].Length
            if ($indent -gt $optionsIndent) {
                $value = $Matches[2].Trim().Trim("'").Trim('"')
                if (-not [string]::IsNullOrWhiteSpace($value)) { [void]$values.Add($value) }
                continue
            }
        }
        if ($line -match '^\s*$') { continue }
        if ($line -match '^(\s*)\S' -and $Matches[1].Length -le $optionsIndent) { break }
    }
    return @($values)
}

function New-ReadinessCheck {
    <#
    .SYNOPSIS
        Constructs a readiness-check record used by the Blocking summary at the top
        of the markdown report. Status: READY | WATCH | BLOCKED | UNKNOWN.
    #>
    param(
        [string]$Area,
        [ValidateSet('READY', 'WATCH', 'BLOCKED', 'UNKNOWN')][string]$Status,
        [string]$Details,
        [string]$NextAction
    )
    [PSCustomObject]@{
        Area       = $Area
        Status     = $Status
        Details    = $Details
        NextAction = $NextAction
    }
}

function Get-ReleaseShipChecks {
    <#
    .SYNOPSIS
        Runs the "ready to ship" checks for the SR/candidate report:
          - Versions.props bumped to match the SR cycle (Major.Minor.Patch in [N0..N9])
          - Bug template's version-with-bug dropdown contains the expected SR version

        In CANDIDATE mode the checks still run, but the messaging notes that
        the bumps + template updates happen AFTER the SR is cut, so a BLOCKED
        status in candidate mode is a soft heads-up rather than a hard blocker
        of the candidate itself.
    .OUTPUTS
        Array of check records (see New-ReadinessCheck).
    #>
    param($Ctx)

    $checks = @()
    $isCandidate = ($Ctx.mode -eq 'candidate')

    # Determine the SR number from the SR branch name. In shipped mode, srBranch
    # IS the release branch (release/X.Y.Zxx-srN). In candidate mode, srBranch is
    # main and the prior-SR name lives in priorSrBranch — we want NEXT SR (= prior + 1).
    $srBranchName = if ($isCandidate) { $Ctx.priorSrBranch } else { $Ctx.srBranch }
    $srMatch = [regex]::Match($srBranchName, '^release/(\d+)\.(\d+)\.\d+xx-sr(\d+)$')
    if (-not $srMatch.Success) {
        $checks += New-ReadinessCheck -Area 'Versions.props bump' -Status 'UNKNOWN' `
            -Details "Could not parse SR number from '$srBranchName'." `
            -NextAction "Verify the branch matches release/X.Y.Zxx-srN."
        return $checks
    }
    $major = [int]$srMatch.Groups[1].Value
    $minor = [int]$srMatch.Groups[2].Value
    $priorSr = [int]$srMatch.Groups[3].Value
    $targetSr = if ($isCandidate) { $priorSr + 1 } else { $priorSr }
    $expectedPatchPrefix = $targetSr * 10   # SR8 → 80, SR9 → 90, SR10 → 100

    # Which ref do we read Versions.props from?
    # Shipped mode: the SR branch itself.
    # Candidate mode: main (which would carry the bump once SR-prior cuts).
    $versionsRef = if ($isCandidate) { "origin/$($Ctx.mainBranch)" } else { $Ctx.srRef }
    $vp = Get-VersionsPropsState -Ref $versionsRef

    if (-not $vp) {
        $checks += New-ReadinessCheck -Area 'Versions.props bump' -Status 'UNKNOWN' `
            -Details "Could not read eng/Versions.props from ``$versionsRef``." `
            -NextAction "Inspect the file manually."
    } else {
        $patchInRange = ($vp.Patch -ge $expectedPatchPrefix -and $vp.Patch -lt ($expectedPatchPrefix + 10))
        $majorMinorMatch = ($vp.Major -eq $major -and $vp.Minor -eq $minor)
        $area = if ($isCandidate) { "Versions.props bump (main → SR$targetSr)" } else { "Versions.props bump (SR$targetSr)" }
        if ($majorMinorMatch -and $patchInRange) {
            $checks += New-ReadinessCheck -Area $area -Status 'READY' `
                -Details "``$versionsRef`` reports ``$($vp.FullVersion)`` — within expected SR$targetSr range [$expectedPatchPrefix..$($expectedPatchPrefix + 9)]." `
                -NextAction "No bump needed."
        } else {
            $candidateHint = if ($isCandidate) {
                " (Expected after SR$priorSr cut: bump main's PatchVersion from $($vp.Patch) to $expectedPatchPrefix.)"
            } else { "" }
            $checks += New-ReadinessCheck -Area $area -Status 'BLOCKED' `
                -Details "``$versionsRef`` reports ``$($vp.FullVersion)``; expected ``$major.$minor.[$expectedPatchPrefix..$($expectedPatchPrefix + 9)]`` for SR$targetSr.$candidateHint" `
                -NextAction "Bump eng/Versions.props (MajorVersion/MinorVersion/PatchVersion) before shipping SR$targetSr."
        }
    }

    # === Bug template version listing ===
    # Issue templates live on the default branch (main) — they're global per repo.
    $templateRef = "origin/$($Ctx.mainBranch)"
    $templateVersions = Get-BugTemplateVersions -Ref $templateRef
    # Acceptable: any entry matching $major.$minor.<patch-in-SR-range>, with or
    # without an "SR$targetSr" or similar suffix.
    $matchPattern = "^$major\.$minor\.(\d+)"
    $matchingEntries = @($templateVersions | Where-Object {
        if ($_ -match $matchPattern) {
            $p = [int]$Matches[1]
            return ($p -ge $expectedPatchPrefix -and $p -lt ($expectedPatchPrefix + 10))
        }
        return $false
    })

    $bugArea = "Bug template lists SR$targetSr version"
    if ($templateVersions.Count -eq 0) {
        $checks += New-ReadinessCheck -Area $bugArea -Status 'UNKNOWN' `
            -Details "Could not read .github/ISSUE_TEMPLATE/bug-report.yml from ``$templateRef`` or the version-with-bug dropdown is empty." `
            -NextAction "Inspect the bug template manually."
    } elseif ($matchingEntries.Count -gt 0) {
        $first = $matchingEntries[0]
        $checks += New-ReadinessCheck -Area $bugArea -Status 'READY' `
            -Details "Bug template lists ``$first`` (and $($matchingEntries.Count - 1) other SR$targetSr entries)." `
            -NextAction "No template update needed."
    } else {
        $sample = ($templateVersions | Select-Object -First 3) -join ', '
        $checks += New-ReadinessCheck -Area $bugArea -Status 'BLOCKED' `
            -Details "No entry matching ``$major.$minor.[$expectedPatchPrefix..$($expectedPatchPrefix + 9)]`` found in version-with-bug dropdown on ``$templateRef``. Top entries: $sample." `
            -NextAction "Add the SR$targetSr version (e.g. ``$major.$minor.$expectedPatchPrefix``) to .github/ISSUE_TEMPLATE/bug-report.yml before shipping."
    }

    return $checks
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
        $obj = Invoke-RestMethod -Uri $url -TimeoutSec 30 -ErrorAction Stop
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
            $isAtOrAhead = Test-CommitOnBranch -Sha $Ctx.srHeadSha -BranchRef $sourceSha
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
        'number,title,state,baseRefName,mergedAt,closedAt,body,mergeCommit,author,labels,isDraft,files')
    if (-not $json) { return $null }
    return ($json | ConvertFrom-Json -ErrorAction SilentlyContinue)
}

function Test-PrIsToolingOnly {
    <#
    .SYNOPSIS
        Returns $true when every file changed by the PR lives under .github/
        (or related tooling roots). Such PRs are agent/skill/workflow changes
        that mention regression issues for context but are NOT product fixes.

    .DESCRIPTION
        Guards against the self-reference false-positive: when an agent or
        workflow PR's body says "Fixes #NNNNN" (as documentation context),
        the regression classifier could otherwise mistake it for a real fix.

        Returns $false when:
          - $Files is null/empty (cannot make a decision -> leave alone)
          - ANY file is outside the tooling roots (real product change)
    #>
    param($Files)
    if (-not $Files) { return $false }
    $count = 0
    foreach ($f in $Files) {
        if (-not $f.path) { continue }
        $count++
        # Tooling roots — agent infrastructure, workflows, helper scripts,
        # docs. Product code (src/, tests/, etc.) is intentionally excluded.
        if ($f.path -notmatch '^(\.github/|eng/scripts/|docs/|README|CONTRIBUTING)') {
            return $false
        }
    }
    return ($count -gt 0)
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

        # False-positive guard: skip PRs whose entire change set lives in
        # tooling roots (.github/, docs/, eng/scripts/, etc). These are
        # agent/skill/workflow PRs that mention regression issue numbers in
        # their body for documentation purposes — they're not real fixes.
        if (Test-PrIsToolingOnly -Files $info.files) {
            Write-Verbose "  Skipping #$prNum — tooling-only PR (mentions #$($Issue.number) in body but changes only .github/, docs/, or eng/scripts/)"
            continue
        }

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
        $confidence = 'high'
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
                    $verdict = 'needs-human-review'
                    $confidence = 'low'
                    $evidence += "Backport PR #$($mergedBackport.number) is MERGED in GitHub but not found in SR git contents — re-run without -NoFetch or verify the merge target manually"
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
                    $confidence = 'medium'
                    $evidence += "PR #$($pr.number) merged to main, no backport PR opened"
                } else {
                    $verdict = 'merged-non-main-only'
                    $confidence = 'medium'
                    $evidence += "PR #$($pr.number) merged but NOT on main (likely inflight-only)"
                }
            }
            elseif ($pr.state -eq 'OPEN') {
                $verdict = 'open-on-main'
                $evidence += "PR #$($pr.number) is OPEN, base=$($pr.baseRef)"
            }
            else {
                $verdict = 'needs-human-review'
                $confidence = 'low'
                $evidence += "PR #$($pr.number) in unexpected state: $($pr.state)"
            }
        }

        $perPrVerdicts += @{ pr = $pr; verdict = $verdict; confidence = $confidence; evidence = $evidence }
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
        confidence = $best.confidence
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
                           '--json', 'number,title,state,stateReason,labels,milestone,createdAt,closedAt')
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

        # False-positive guard: issues closed as DUPLICATE are not regressions
        # against this SR — they were rolled up into a canonical issue. Skip
        # the expensive PR walk and flag them so the report can surface them
        # under an "informational" tier instead of "no fix yet".
        $isDuplicate = ($iss.state -eq 'CLOSED') -and ($iss.PSObject.Properties['stateReason']) -and ($iss.stateReason -eq 'DUPLICATE')
        if ($isDuplicate) {
            $results += @{
                issue             = [int]$iss.number
                title             = $iss.title
                state             = $iss.state
                stateReason       = $iss.stateReason
                labels            = @($iss.labels.name)
                milestone         = if ($iss.milestone) { $iss.milestone.title } else { $null }
                createdAt         = $iss.createdAt
                closedAt          = $iss.closedAt
                classification    = 'closed-as-duplicate'
                confidence        = 'high'
                evidence          = @("Issue closed with stateReason=DUPLICATE — rolled up into a canonical regression. Inspect the closing comment for the canonical issue reference.")
                candidateFixPrs   = @()
                recommendedAction = 'Confirm the canonical issue (visible in the close comment) is tracked separately. No action on this issue.'
            }
            continue
        }

        $candidatePrs = Get-IssueTimelinePrs -Repo $Ctx.repo -IssueNumber $iss.number
        $classify = Classify-RegressionCandidate -Issue $iss -CandidatePrs $candidatePrs `
                        -Ctx $Ctx -SrContents $SrContents

        $results += @{
            issue = [int]$iss.number
            title = $iss.title
            state = $iss.state
            stateReason = if ($iss.PSObject.Properties['stateReason']) { $iss.stateReason } else { $null }
            labels = @($iss.labels.name)
            milestone = if ($iss.milestone) { $iss.milestone.title } else { $null }
            createdAt = $iss.createdAt
            closedAt = if ($iss.PSObject.Properties['closedAt']) { $iss.closedAt } else { $null }
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

function Get-OpenIssuesByLabel {
    <#
    .SYNOPSIS
        Returns open issues labeled $Label. Includes createdAt so callers can
        compute freshness for ci-scan-style escalation. Returns an array.
    #>
    [OutputType([object[]])]
    param([string]$Label)

    $raw = Invoke-Gh @('issue', 'list', '--repo', $script:Repo, '--state', 'open',
                       '--limit', '100', '--label', $Label,
                       '--json', 'number,title,url,labels,createdAt,updatedAt')
    if (-not $raw) { return @() }
    $issues = $raw | ConvertFrom-Json -ErrorAction SilentlyContinue
    if (-not $issues) { return @() }
    return @($issues)
}

function Get-CiScanIssuesForSr {
    <#
    .SYNOPSIS
        Returns open `ci-scan` issues sorted newest-first. Not filtered by
        SR version — the scanner targets `main`, so every signal is a
        potential noise/blocker source for any in-flight SR.
    #>
    [OutputType([object[]])]
    param()

    $issues = Get-OpenIssuesByLabel -Label 'ci-scan'
    return @($issues | Sort-Object {
        $u = ConvertTo-Utc -Value $_.createdAt
        if ($u) { $u } else { [DateTime]::MinValue }
    } -Descending)
}

function Test-CiScanIsFresh {
    <#
    .SYNOPSIS
        Returns $true if the ci-scan issue was filed within the last $HoursThreshold
        hours (default 24). Used to escalate the ship-check to WATCH.
    #>
    param($Issue, [int]$HoursThreshold = 24)
    if (-not $Issue.PSObject.Properties['createdAt'] -or -not $Issue.createdAt) { return $false }
    $createdUtc = ConvertTo-Utc -Value $Issue.createdAt
    if (-not $createdUtc) { return $false }
    return ((Get-Date).ToUniversalTime() - $createdUtc).TotalHours -lt $HoursThreshold
}

function Get-CiSignalChecks {
    <#
    .SYNOPSIS
        Builds two readiness-check records:
          1. CI Failure Scanner signals (ci-scan label, escalates if any <24h)
          2. Known Build Errors (KBE label, WATCH if any open, READY otherwise)
        Returns @{ Checks = [array]; CiScanIssues = [array]; KbeIssues = [array] }.
    #>
    param()

    Write-Host "Querying ci-scan and Known Build Error issue lists..." -ForegroundColor Cyan
    $ciScan = @(Get-CiScanIssuesForSr)
    $kbe    = @(Get-OpenIssuesByLabel -Label 'Known Build Error')

    $checks = @()

    $fresh = @($ciScan | Where-Object { Test-CiScanIsFresh -Issue $_ -HoursThreshold 24 })
    if ($fresh.Count -gt 0) {
        $checks += New-ReadinessCheck `
            -Area 'CI Failure Scanner signals' `
            -Status 'WATCH' `
            -Details "$($fresh.Count) ci-scan issue(s) filed in the last 24h ($($ciScan.Count) total open). Likely affects this SR." `
            -NextAction 'Review the freshest ci-scan issues to confirm none block ship.'
    } elseif ($ciScan.Count -gt 0) {
        $checks += New-ReadinessCheck `
            -Area 'CI Failure Scanner signals' `
            -Status 'WATCH' `
            -Details "$($ciScan.Count) open ci-scan issue(s) (none filed in the last 24h)." `
            -NextAction 'Skim recent ci-scan issues for impact patterns; mark accepted-known if appropriate.'
    } else {
        $checks += New-ReadinessCheck `
            -Area 'CI Failure Scanner signals' `
            -Status 'READY' `
            -Details 'No open ci-scan issues — scanner has not flagged recurring CI failures.' `
            -NextAction 'Continue monitoring.'
    }

    if ($kbe.Count -gt 0) {
        $checks += New-ReadinessCheck `
            -Area 'Known Build Errors' `
            -Status 'WATCH' `
            -Details "$($kbe.Count) open Known Build Error issue(s). May explain background CI noise." `
            -NextAction 'Cross-check against any SR build failures to distinguish accepted-known vs new regressions.'
    } else {
        $checks += New-ReadinessCheck `
            -Area 'Known Build Errors' `
            -Status 'READY' `
            -Details 'No open Known Build Error issues found.' `
            -NextAction 'Continue monitoring.'
    }

    return @{
        Checks       = $checks
        CiScanIssues = $ciScan
        KbeIssues    = $kbe
    }
}

# region ────────────────────── 7. MARKDOWN REPORT ─────────────────────────

function Get-VerdictTier {
    <#
    .SYNOPSIS
        Maps a regression-issue classification to a deterministic readiness tier.

    .DESCRIPTION
        Tier 1 (🔴 blocking): classifications that PREVENT shipping the SR.
        Tier 2 (🟡 risk):     classifications that REQUIRE human review/decision.
        Tier 3 (🟢 informational): classifications that ARE NOT actionable.

        The mapping is intentionally simple and deterministic — no scoring,
        no judgement calls. If the rules need adjustment, edit this table.
    #>
    param([string]$Classification)
    switch ($Classification) {
        'in-sr-reverted'              { 1; break }
        'no-fix-yet'                  { 1; break }
        'rejected-from-sr'            { 2; break }
        'backport-in-progress'        { 2; break }
        'merged-on-main-no-backport'  { 2; break }
        'merged-non-main-only'        { 2; break }
        'open-on-main'                { 2; break }
        'needs-human-review'          { 2; break }
        'in-sr-active'                { 3; break }
        'closed-as-duplicate'         { 3; break }
        default                       { 2 }   # unknown → treat as risk
    }
}

function Get-OverallVerdict {
    <#
    .SYNOPSIS
        Computes a deterministic 🔴/🟡/🟢 overall verdict from a readiness report.

    .DESCRIPTION
        Rules (evaluated in order, first match wins):

          🔴 Not Ready when ANY of:
            - One or more regression classifications in Tier 1
              (in-sr-reverted, no-fix-yet for an OPEN regression issue)
          🟡 Conditionally Ready when ANY of:
            - One or more Tier 2 classifications
            - SR CI overall verdict is 'red-needs-review', 'stale',
              'partial-unknown', or 'unknown'  (NOT candidate)

          🟢 Ready otherwise.

        For candidate / pre-flight mode, CI staleness is non-blocking and
        downgraded to advisory (the SR branch doesn't exist yet — staleness
        of main's CI is normal cycle-time noise).

    .OUTPUTS
        Hashtable with fields:
          symbol   = 🔴 / 🟡 / 🟢
          tier     = 1 / 2 / 3
          label    = 'Not Ready' / 'Conditionally Ready' / 'Ready'
          reasons  = string[] explaining each contributing factor
    #>
    param($Data)

    $isCandidate = $false
    if ($Data.metadata.ContainsKey('mode') -and $Data.metadata['mode'] -eq 'candidate') {
        $isCandidate = $true
    }

    $reasons = New-Object System.Collections.Generic.List[string]
    $tier1 = $false
    $tier2 = $false

    # Regression classifications
    if ($Data.ContainsKey('regressions') -and $Data['regressions']) {
        $t1Counts = @{}
        $t2Counts = @{}
        foreach ($r in $Data['regressions']) {
            $tier = Get-VerdictTier -Classification $r.classification
            # `no-fix-yet` only blocks if the issue is still OPEN
            if ($r.classification -eq 'no-fix-yet' -and $r.state -ne 'OPEN') {
                $tier = 3
            }
            if ($tier -eq 1) {
                if (-not $t1Counts.ContainsKey($r.classification)) { $t1Counts[$r.classification] = 0 }
                $t1Counts[$r.classification]++
                $tier1 = $true
            } elseif ($tier -eq 2) {
                if (-not $t2Counts.ContainsKey($r.classification)) { $t2Counts[$r.classification] = 0 }
                $t2Counts[$r.classification]++
                $tier2 = $true
            }
        }
        foreach ($k in $t1Counts.Keys | Sort-Object) {
            $reasons.Add("[Tier 1] $($t1Counts[$k]) × ``$k``") | Out-Null
        }
        foreach ($k in $t2Counts.Keys | Sort-Object) {
            $reasons.Add("[Tier 2] $($t2Counts[$k]) × ``$k``") | Out-Null
        }
    }

    # CI status (skipped for candidate mode — main CI is naturally noisy)
    if (-not $isCandidate -and $Data.ContainsKey('ci') -and $Data['ci']) {
        switch ($Data['ci'].overall) {
            'red-needs-review' {
                $tier2 = $true
                $reasons.Add("[Tier 2] CI on SR branch: ``red-needs-review`` — investigate failures before judging") | Out-Null
            }
            'stale' {
                $tier2 = $true
                $reasons.Add("[Tier 2] CI on SR branch: ``stale`` — re-run before judging") | Out-Null
            }
            'partial-unknown' {
                $tier2 = $true
                $reasons.Add("[Tier 2] CI verdict ``partial-unknown`` — one or more pipeline queries failed") | Out-Null
            }
            'unknown' {
                $tier2 = $true
                $reasons.Add("[Tier 2] CI verdict ``unknown`` — could not query pipeline") | Out-Null
            }
        }
    } elseif ($isCandidate -and $Data.ContainsKey('ci') -and $Data['ci'] -and
              $Data['ci'].overall -in @('red-needs-review', 'stale', 'partial-unknown', 'unknown')) {
        $reasons.Add("[Advisory] Candidate mode — main CI is ``$($Data['ci'].overall)``. Re-evaluate after SR cut.") | Out-Null
    }

    # Ship-readiness checks (versions.props bumped, bug template updated).
    # Each BLOCKED check escalates the verdict to Tier 1 (Not Ready).
    if ($Data.ContainsKey('shipChecks') -and $Data['shipChecks']) {
        $blockedShipChecks = @($Data['shipChecks'] | Where-Object { $_.Status -eq 'BLOCKED' })
        foreach ($sc in $blockedShipChecks) {
            $tier1 = $true
            $reasons.Add("[Tier 1] Ship check BLOCKED: $($sc.Area)") | Out-Null
        }
    }

    if ($tier1) {
        return @{
            symbol = '🔴'
            tier = 1
            label = 'Not Ready'
            reasons = $reasons.ToArray()
        }
    }
    if ($tier2) {
        return @{
            symbol = '🟡'
            tier = 2
            label = 'Conditionally Ready'
            reasons = $reasons.ToArray()
        }
    }
    return @{
        symbol = '🟢'
        tier = 3
        label = 'Ready'
        reasons = if ($reasons.Count -gt 0) { $reasons.ToArray() } else { @('No blocking or risk-tier signals detected.') }
    }
}

function ConvertTo-LinkedSha {
    <#
    .SYNOPSIS Linkify a commit SHA in markdown using $RepoUrl.
    #>
    param([string]$Sha, [string]$RepoUrl)
    if (-not $Sha) { return '?' }
    $short = if ($Sha.Length -ge 8) { $Sha.Substring(0, 8) } else { $Sha }
    if (-not $RepoUrl) { return "``$short``" }
    return "[``$short``]($RepoUrl/commit/$Sha)"
}

function ConvertTo-LinkedPr {
    <#
    .SYNOPSIS Linkify a PR number in markdown using $RepoUrl.
    #>
    param($PrNumber, [string]$RepoUrl)
    if (-not $PrNumber) { return '—' }
    if (-not $RepoUrl) { return "#$PrNumber" }
    return "[#$PrNumber]($RepoUrl/pull/$PrNumber)"
}

function Format-CiScanIssueRows {
    <#
    .SYNOPSIS
        Builds the rows of the ci-scan section for the SR markdown report.
        Returns the table body as a single string (already terminated with newlines).
        Returns $null if there's nothing to render. Fresh issues (<24h) are
        flagged with 🆕.
    #>
    param([array]$Issues, [string]$RepoUrl, [int]$MaxRows = 15)
    if (-not $Issues -or $Issues.Count -eq 0) { return $null }

    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.AppendLine('| Issue | Title | Filed |')
    [void]$sb.AppendLine('|---|---|---|')
    $rows = $Issues | Select-Object -First $MaxRows
    foreach ($iss in $rows) {
        $marker = ''
        $ageDisplay = '—'
        if ($iss.PSObject.Properties['createdAt'] -and $iss.createdAt) {
            $createdUtc = ConvertTo-Utc -Value $iss.createdAt
            if ($createdUtc) {
                $hoursAgo = ((Get-Date).ToUniversalTime() - $createdUtc).TotalHours
                $ageDisplay = if ($hoursAgo -lt 24) { '{0:N0}h ago' -f $hoursAgo }
                              else                  { '{0:N0}d ago' -f ($hoursAgo / 24) }
                if ($hoursAgo -lt 24) { $marker = '🆕 ' }
            }
        }
        $issLink = "[#$($iss.number)]($RepoUrl/issues/$($iss.number))"
        $title = ($iss.title -replace '\|', '\|').Trim()
        [void]$sb.AppendLine("| $marker$issLink | $title | $ageDisplay |")
    }
    if ($Issues.Count -gt $MaxRows) {
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("_…and $($Issues.Count - $MaxRows) more. Full list: [open ci-scan issues]($RepoUrl/issues?q=is%3Aopen+is%3Aissue+label%3Aci-scan+sort%3Acreated-desc)._")
    }
    return $sb.ToString()
}

function ConvertTo-Utc {
    <#
    .SYNOPSIS
        Normalizes a value that may be a DateTime (Utc/Local/Unspecified) or a
        string into a UTC DateTime. Returns $null if conversion fails.
    .NOTES
        `ConvertFrom-Json` already parses ISO-8601 'Z' strings into DateTime
        with Kind=Utc. But `[DateTime]::Parse(...)` on a string produces
        Kind=Unspecified, which `.ToUniversalTime()` then misinterprets as
        Local — silently shifting the value by the host's UTC offset. Use
        this helper everywhere age/freshness is computed.
    #>
    param([object]$Value)

    if ($null -eq $Value) { return $null }

    if ($Value -is [DateTime]) {
        if ($Value.Kind -eq [DateTimeKind]::Utc) { return $Value }
        if ($Value.Kind -eq [DateTimeKind]::Local) { return $Value.ToUniversalTime() }
        # Unspecified — assume UTC (gh JSON normally returns 'Z' suffix)
        return [DateTime]::SpecifyKind($Value, [DateTimeKind]::Utc)
    }

    try {
        $dto = [DateTimeOffset]::Parse([string]$Value, [Globalization.CultureInfo]::InvariantCulture)
        return $dto.UtcDateTime
    } catch {
        return $null
    }
}

function Format-GitHubHandle {
    <#
    .SYNOPSIS Render a GitHub login as a code span so it does NOT trigger an @-mention notification.
    .DESCRIPTION
        GitHub treats `@username` in issue/PR bodies as a notification mention. To safely surface
        an author's handle in a report (without spamming them on every nightly run), wrap the
        login in backticks: `` `username` `` is rendered as a code span and is NOT interpreted as a mention.
        Handles bot/app refs (e.g. ``app/dotnet-maestro``) as well.
    .PARAMETER Login
        The raw GitHub login (with or without a leading ``@``). May be ``$null`` / empty.
    .PARAMETER Fallback
        Text to return when Login is null/empty. Defaults to ``unknown``.
    #>
    param(
        [Parameter(Mandatory = $false)][AllowNull()][AllowEmptyString()][string]$Login,
        [string]$Fallback = 'unknown'
    )
    if ([string]::IsNullOrWhiteSpace($Login)) { return $Fallback }
    $clean = $Login.TrimStart('@').Trim()
    if ([string]::IsNullOrWhiteSpace($clean)) { return $Fallback }
    return "``$clean``"
}

function Get-ReportSemanticHash {
    <#
    .SYNOPSIS
        Produces a stable SHA-256 hash of the report's semantic content.

    .DESCRIPTION
        The hash captures fields that change ONLY when the report's verdict
        or contents would meaningfully differ — used by the workflow to skip
        re-posting unchanged trackers (idempotency).

        DELIBERATELY EXCLUDED: fetchedAt timestamp, CI duration, "X minutes
        ago" relative times, and any other field that drifts on every run.
    #>
    param($Data, $Verdict)

    $semantic = @{
        verdict = $Verdict.symbol
        srHead = $Data.metadata.srHeadSha
        ciOverall = if ($Data.ContainsKey('ci') -and $Data['ci']) { $Data['ci'].overall } else { $null }
        srPrs = if ($Data.ContainsKey('srContents') -and $Data['srContents']) {
                    @($Data['srContents'].sourcePrs | Sort-Object) -join ','
                } else { '' }
        regressions = if ($Data.ContainsKey('regressions') -and $Data['regressions']) {
                          @($Data['regressions'] | Sort-Object issue | ForEach-Object {
                              "$($_.issue):$($_.classification)"
                          }) -join '|'
                      } else { '' }
        openSrPrs = if ($Data.ContainsKey('openSrPrs') -and $Data['openSrPrs']) {
                        @($Data['openSrPrs'] | Sort-Object number | ForEach-Object { $_.number }) -join ','
                    } else { '' }
        shipChecks = if ($Data.ContainsKey('shipChecks') -and $Data['shipChecks']) {
                         @($Data['shipChecks'] | Sort-Object Area | ForEach-Object {
                             "$($_.Area):$($_.Status)"
                         }) -join '|'
                     } else { '' }
    }

    $json = $semantic | ConvertTo-Json -Depth 5 -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hash = $sha.ComputeHash($bytes)
        return ([System.BitConverter]::ToString($hash) -replace '-', '').ToLowerInvariant()
    } finally {
        $sha.Dispose()
    }
}

function Format-MarkdownReport {
    param($Data, [string]$RepoUrl, [string]$TrackerKey, [int]$MaxBodyBytes = 60000)

    $ctx = $Data.metadata
    $srBranch = $ctx.srBranch
    $shortHead = if ($ctx.srHeadSha) { $ctx.srHeadSha.Substring(0, 8) } else { '?' }

    # Compute verdict + semantic hash (deterministic, used in markers)
    $verdict = Get-OverallVerdict -Data $Data
    $semanticHash = Get-ReportSemanticHash -Data $Data -Verdict $verdict

    $sb = [System.Text.StringBuilder]::new()

    # === HEADER + MARKERS ===
    # Markers go FIRST so a workflow scanning for them can short-circuit
    # without parsing the body.
    if ($TrackerKey) {
        [void]$sb.AppendLine("<!-- release-readiness-tracker: $TrackerKey -->")
    }
    [void]$sb.AppendLine("<!-- release-readiness-hash: sha=$semanticHash -->")

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

    # === VERDICT (always second, always visible) ===
    [void]$sb.AppendLine("## Verdict — $($verdict.symbol) **$($verdict.label)**")
    [void]$sb.AppendLine()
    foreach ($r in $verdict.reasons) {
        [void]$sb.AppendLine("- $r")
    }
    [void]$sb.AppendLine()

    # Tracker + provenance line (visible, complements the HTML comment marker)
    if ($TrackerKey) {
        [void]$sb.AppendLine("**Tracker:** ``$TrackerKey`` · mode=``$mode`` · branch=``$srBranch``")
    }
    $shaLinked = ConvertTo-LinkedSha -Sha $ctx.srHeadSha -RepoUrl $RepoUrl
    [void]$sb.AppendLine("**HEAD**: $shaLinked — $($ctx.srHeadSubject)")
    [void]$sb.AppendLine("**Generated**: $($ctx.fetchedAt)")
    [void]$sb.AppendLine("**Regression labels**: $($ctx.regressionLabels -join ', ') _(mode: $($ctx.labelInferenceMode))_")
    [void]$sb.AppendLine()

    if ($Data.ContainsKey('warnings') -and $Data['warnings'].Count -gt 0) {
        [void]$sb.AppendLine("> ⚠️ **Warnings:**")
        foreach ($w in $Data['warnings']) { [void]$sb.AppendLine("> - $w") }
        [void]$sb.AppendLine()
    }

    # === BLOCKING SUMMARY (hoisted to top, right under the verdict) ===
    # Surface every BLOCKED ship-check AND every Tier 1 regression so the
    # release captain sees what's preventing ship without scrolling past
    # CI tables, open-PR tables, and the full tier breakdown below.
    $blockingItems = New-Object System.Collections.Generic.List[hashtable]
    if ($Data.ContainsKey('shipChecks') -and $Data['shipChecks']) {
        foreach ($sc in $Data['shipChecks']) {
            if ($sc.Status -eq 'BLOCKED') {
                [void]$blockingItems.Add(@{
                    area = "🛠️ $($sc.Area)"
                    details = $sc.Details
                    action = $sc.NextAction
                })
            }
        }
    }
    if ($Data.ContainsKey('regressions') -and $Data['regressions']) {
        foreach ($r in $Data['regressions']) {
            $tier = Get-VerdictTier -Classification $r.classification
            if ($r.classification -eq 'no-fix-yet' -and $r.state -ne 'OPEN') { $tier = 3 }
            if ($tier -eq 1) {
                $issLink = "[#$($r.issue)]($RepoUrl/issues/$($r.issue))"
                [void]$blockingItems.Add(@{
                    area = "🐞 $issLink — $($r.classification)"
                    details = $r.title
                    action = $r.recommendedAction
                })
            }
        }
    }

    if ($blockingItems.Count -gt 0) {
        [void]$sb.AppendLine("## 🔴 Blocking — $($blockingItems.Count) item(s)")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine('| Area | Details | Next action |')
        [void]$sb.AppendLine('|---|---|---|')
        foreach ($b in $blockingItems) {
            $area = ($b.area -replace '\|', '\|').Trim()
            $details = ($b.details -replace '\|', '\|').Trim()
            $action = ($b.action -replace '\|', '\|').Trim()
            [void]$sb.AppendLine("| $area | $details | $action |")
        }
        [void]$sb.AppendLine()
    } else {
        [void]$sb.AppendLine("## 🟢 No blocking items")
        [void]$sb.AppendLine()
    }

    # === SHIP-READINESS CHECKS (full table — non-blocking + blocking) ===
    if ($Data.ContainsKey('shipChecks') -and $Data['shipChecks'] -and $Data['shipChecks'].Count -gt 0) {
        [void]$sb.AppendLine("## Ship-readiness checks")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine('| Check | Status | Details | Next action |')
        [void]$sb.AppendLine('|---|---|---|---|')
        foreach ($sc in $Data['shipChecks']) {
            $statusEmoji = switch ($sc.Status) {
                'READY'   { '🟢 READY' }
                'WATCH'   { '🟡 WATCH' }
                'BLOCKED' { '🔴 BLOCKED' }
                default   { "⚪ $($sc.Status)" }
            }
            $area = ($sc.Area -replace '\|', '\|').Trim()
            $details = ($sc.Details -replace '\|', '\|').Trim()
            $action = ($sc.NextAction -replace '\|', '\|').Trim()
            [void]$sb.AppendLine("| $area | $statusEmoji | $details | $action |")
        }
        [void]$sb.AppendLine()
    }

    # === HUMAN-EDITABLE SECTION ===
    # Wrapped in begin/end markers so a workflow can preserve manual edits
    # across re-runs (idempotency).
    [void]$sb.AppendLine("<!-- release-readiness:human-notes:begin -->")
    [void]$sb.AppendLine("## Release Captain Notes")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("_Add manual notes here. Anything between these begin/end markers is preserved across automated re-runs._")
    [void]$sb.AppendLine("<!-- release-readiness:human-notes:end -->")
    [void]$sb.AppendLine()

    # === CI section ===
    if ($Data.ContainsKey('ci') -and $Data['ci']) {
        $ciData = $Data['ci']
        [void]$sb.AppendLine("## CI Status — overall: ``$($ciData.overall)``")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine('| Pipeline | Verdict | Latest result | At/ahead of SR HEAD? | Build |')
        [void]$sb.AppendLine('|---|---|---|---|---|')
        foreach ($p in $ciData.pipelines) {
            $lb = $p.latestBuild
            $pverdict = $p.verdict
            $result = if ($lb) { $lb.result } else { '—' }
            $fresh = if ($lb) { if ($lb.isAtOrAheadOfSrHead) { '✅' } else { '❌ stale' } } else { '—' }
            $buildLink = if ($lb -and $lb.url) { "[$($lb.id)]($($lb.url))" } else { '—' }
            [void]$sb.AppendLine("| $($p.name) | ``$pverdict`` | $result | $fresh | $buildLink |")
        }
        [void]$sb.AppendLine()
    }

    # === Recent CI Failure Scanner signals (ci-scan label) ===
    if ($Data.ContainsKey('ciScanIssues') -and $Data['ciScanIssues']) {
        [void]$sb.AppendLine("## Recent CI Failure Scanner signals (``ci-scan``)")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("_Auto-filed by the CI Failure Scanner workflow (runs every 12h on ``main``). Fresh issues (<24h) are flagged 🆕._")
        [void]$sb.AppendLine()
        $rows = Format-CiScanIssueRows -Issues $Data['ciScanIssues'] -RepoUrl $RepoUrl
        if ($rows) {
            [void]$sb.Append($rows)
        } else {
            [void]$sb.AppendLine('_No open ci-scan issues._')
        }
        [void]$sb.AppendLine()
    }

    # === SR contents section ===
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
                $rs = ConvertTo-LinkedSha -Sha $r.revertCommit -RepoUrl $RepoUrl
                $rc = ConvertTo-LinkedSha -Sha $r.revertsCommit -RepoUrl $RepoUrl
                $rp = ConvertTo-LinkedPr -PrNumber $r.revertsPr -RepoUrl $RepoUrl
                $ro = if ($r.ContainsKey('origin')) { $r.origin } else { '?' }
                [void]$sb.AppendLine("| $rs | $rp | $rc | $ro |")
            }
        }
        [void]$sb.AppendLine()
    }

    # === Open SR-targeting PRs ===
    #
    # Two modes:
    #   - Live SR (mode == 'shipped'): show the full table — these are real
    #     backport PRs targeting the SR branch, which is a small, useful set.
    #   - Candidate (mode == 'candidate'): srBranch is main, so this query
    #     returns 100+ open PRs targeting main — far too noisy for a tracker
    #     issue. Instead, surface only the dotnet/maui "candidate PR" if one
    #     exists (e.g. "June 8th, Candidate" — the PR that promotes a specific
    #     main commit as the basis for cutting the next SR).
    if ($Data.ContainsKey('openSrPrs') -and $Data['openSrPrs'] -and $Data['openSrPrs'].Count -gt 0) {
        if ($mode -eq 'candidate') {
            # Find a PR whose title looks like a candidate-promotion PR.
            # Be conservative — require a word boundary so "CandidateView" doesn't match.
            $candidatePrs = @($Data['openSrPrs'] | Where-Object {
                $_.title -match '(?i)\bcandidate\b'
            })
            [void]$sb.AppendLine("## Candidate PR for next SR cut")
            [void]$sb.AppendLine()
            if ($candidatePrs.Count -eq 0) {
                [void]$sb.AppendLine("_No open PR titled `*Candidate*` found targeting ``$srBranch``. Open one when ready to promote a main commit as the SR cut point._")
            } else {
                foreach ($cp in $candidatePrs) {
                    $cpLink = ConvertTo-LinkedPr -PrNumber $cp.number -RepoUrl $RepoUrl
                    $cpTitle = if ($cp.title.Length -gt 80) { $cp.title.Substring(0, 80) + '...' } else { $cp.title }
                    [void]$sb.AppendLine("- $cpLink — $cpTitle (by $(Format-GitHubHandle $cp.author.login), updated $($cp.updatedAt))")
                }
                [void]$sb.AppendLine()
                [void]$sb.AppendLine("_Full list of $($Data['openSrPrs'].Count) open PRs targeting ``$srBranch`` omitted to reduce noise; see [the PR list]($RepoUrl/pulls?q=is%3Apr+is%3Aopen+base%3A$srBranch)._")
            }
            [void]$sb.AppendLine()
        } else {
            [void]$sb.AppendLine("## Open PRs Targeting $srBranch — $($Data['openSrPrs'].Count)")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine('| PR | Title | Author | Draft? | Review | Updated |')
            [void]$sb.AppendLine('|---|---|---|---|---|---|')
            foreach ($pr in $Data['openSrPrs']) {
                $title = if ($pr.title.Length -gt 60) { $pr.title.Substring(0, 60) + '...' } else { $pr.title }
                $draft = if ($pr.isDraft) { '✏️' } else { '' }
                $rev = if ($pr.reviewDecision) { $pr.reviewDecision } else { '—' }
                $prLink = ConvertTo-LinkedPr -PrNumber $pr.number -RepoUrl $RepoUrl
                [void]$sb.AppendLine("| $prLink | $title | $(Format-GitHubHandle $pr.author.login) | $draft | $rev | $($pr.updatedAt) |")
            }
            [void]$sb.AppendLine()
        }
    }

    # === Regressions section — organized into tiers ===
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

        # Three deterministic tiers. Order within a tier is alphabetical
        # over the classification name for stable diffs across runs.
        $tier1Classes = @('in-sr-reverted', 'no-fix-yet') | Sort-Object
        $tier2Classes = @('rejected-from-sr', 'backport-in-progress', 'merged-on-main-no-backport',
                          'merged-non-main-only', 'open-on-main', 'needs-human-review') | Sort-Object
        $tier3Classes = @('in-sr-active', 'closed-as-duplicate') | Sort-Object

        $emitTier = {
            param([string]$Header, [string[]]$Classes, [string]$EmptyLine)
            $any = $false
            foreach ($cls in $Classes) {
                $items = @($regs | Where-Object { $_.classification -eq $cls })
                # In Tier 1 we suppress no-fix-yet entries whose issue is CLOSED
                if ($cls -eq 'no-fix-yet') {
                    $items = @($items | Where-Object { $_.state -eq 'OPEN' })
                }
                if ($items.Count -eq 0) { continue }
                if (-not $any) {
                    [void]$sb.AppendLine("### $Header")
                    [void]$sb.AppendLine()
                    $any = $true
                }
                [void]$sb.AppendLine("#### ``$cls`` ($($items.Count))")
                [void]$sb.AppendLine()
                [void]$sb.AppendLine('| Issue | Title | Fix PRs | Action |')
                [void]$sb.AppendLine('|---|---|---|---|')
                # Stable sort: by issue number ascending
                foreach ($it in ($items | Sort-Object issue)) {
                    $title = if ($it.title.Length -gt 50) { $it.title.Substring(0, 50) + '...' } else { $it.title }
                    $prList = @($it.candidateFixPrs | ForEach-Object { ConvertTo-LinkedPr -PrNumber $_.number -RepoUrl $RepoUrl }) -join ', '
                    if (-not $prList) { $prList = '—' }
                    $issueLink = if ($RepoUrl) { "[#$($it.issue)]($RepoUrl/issues/$($it.issue))" } else { "#$($it.issue)" }
                    [void]$sb.AppendLine("| $issueLink | $title | $prList | $($it.recommendedAction) |")
                }
                [void]$sb.AppendLine()
            }
            if (-not $any -and $EmptyLine) {
                [void]$sb.AppendLine("### $Header")
                [void]$sb.AppendLine()
                [void]$sb.AppendLine($EmptyLine)
                [void]$sb.AppendLine()
            }
        }

        & $emitTier '🔴 Tier 1 — Blocking' $tier1Classes '_No blocking regressions._'
        & $emitTier '🟡 Tier 2 — Risk / Review' $tier2Classes '_No risk-tier regressions._'
        & $emitTier '🟢 Tier 3 — Informational' $tier3Classes $null
    }

    $body = $sb.ToString()

    # === SAFETY NET: defang any bare @-mentions ===
    # Primary defense is Format-GitHubHandle at emit time, but PR/issue
    # titles or commit messages can contain raw `@user` references that
    # would notify real users every time this report is filed. Wrap any
    # `@handle` in backticks so GitHub renders it as a code span (no mention).
    $body = [regex]::Replace(
        $body,
        '(^|[^a-zA-Z0-9/`])@([a-zA-Z0-9][a-zA-Z0-9_-]*(?:/[a-zA-Z0-9][a-zA-Z0-9_-]*)?)',
        '$1`$2`'
    )

    # === BODY-SIZE CAP ===
    # GitHub issue body limit is 65,536 bytes. Cap below that and append a
    # truncation message. We measure UTF-8 bytes, not character count.
    $bytes = [System.Text.Encoding]::UTF8.GetByteCount($body)
    if ($bytes -gt $MaxBodyBytes) {
        $truncateMsg = "`n`n> ⚠️ **Report truncated** ($bytes bytes exceeded cap of $MaxBodyBytes). See full data in workflow artifacts.`n"
        $tail = [System.Text.Encoding]::UTF8.GetByteCount($truncateMsg)
        $targetLen = $MaxBodyBytes - $tail
        if ($targetLen -lt 0) { $targetLen = $MaxBodyBytes }
        # Walk back to a safe character boundary
        $bodyBytes = [System.Text.Encoding]::UTF8.GetBytes($body)
        if ($targetLen -gt $bodyBytes.Length) { $targetLen = $bodyBytes.Length }
        $truncatedBytes = New-Object byte[] $targetLen
        [Array]::Copy($bodyBytes, 0, $truncatedBytes, 0, $targetLen)
        # UTF-8 boundary repair: trim incomplete continuation bytes from the tail
        while ($truncatedBytes.Length -gt 0 -and ($truncatedBytes[$truncatedBytes.Length - 1] -band 0xC0) -eq 0x80) {
            $newArr = New-Object byte[] ($truncatedBytes.Length - 1)
            [Array]::Copy($truncatedBytes, 0, $newArr, 0, $newArr.Length)
            $truncatedBytes = $newArr
        }
        $body = [System.Text.Encoding]::UTF8.GetString($truncatedBytes) + $truncateMsg
    }

    return $body
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

    # Run version + bug-template checks (cheap; included in all phases except 'ci'-only).
    # These surface the "is versions.props bumped?" and "is the bug template updated?"
    # questions as blocking items at the top of the report.
    if ($Phase -in 'all', 'commits', 'regressions', 'open-prs') {
        $data['shipChecks'] = Get-ReleaseShipChecks -Ctx $ctx
    }

    # CI scanner + KBE issue signals — merged into shipChecks so they appear in the
    # ship-readiness table AND can escalate the verdict (fresh ci-scan → WATCH; never
    # BLOCKED automatically because the scanner can be noisy).
    if ($Phase -in 'all', 'commits', 'regressions', 'open-prs') {
        $signalResult = Get-CiSignalChecks
        if (-not $data.ContainsKey('shipChecks') -or -not $data['shipChecks']) {
            $data['shipChecks'] = @()
        }
        $data['shipChecks'] = @($data['shipChecks']) + @($signalResult.Checks)
        $data['ciScanIssues'] = @($signalResult.CiScanIssues)
        $data['kbeIssues']    = @($signalResult.KbeIssues)
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

    # Compute deterministic verdict + semantic hash. Surfaced in JSON so
    # automation can consume it without re-parsing the markdown.
    $verdict = Get-OverallVerdict -Data $data
    $semanticHash = Get-ReportSemanticHash -Data $data -Verdict $verdict
    $data['verdict'] = @{
        symbol = $verdict.symbol
        tier = $verdict.tier
        label = $verdict.label
        reasons = $verdict.reasons
    }
    $data['semanticHash'] = $semanticHash
    if ($TrackerKey) {
        $data['trackerKey'] = $TrackerKey
    }

    # Output
    $jsonOut = $data | ConvertTo-Json -Depth 20 -Compress:$false
    $mdOut = Format-MarkdownReport -Data $data -RepoUrl $RepoUrl -TrackerKey $TrackerKey -MaxBodyBytes $MaxBodyBytes

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

# Skip orchestration when dot-sourced for unit tests. Tests do:
#   $env:GET_RELEASE_READINESS_TEST_MODE = '1'
#   . path/to/Get-ReleaseReadiness.ps1
# which makes Invoke-Main a no-op while still loading all functions.
if (-not $env:GET_RELEASE_READINESS_TEST_MODE) {
    Invoke-Main
}
