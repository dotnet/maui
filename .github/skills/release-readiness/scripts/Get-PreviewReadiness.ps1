#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Generates a public-safe .NET MAUI preview release-readiness report
    for a specific net<major>.0-previewN branch.

.DESCRIPTION
    This is the "preview lane" companion to Get-ReleaseReadiness.ps1 (SR lane).

    Given a preview branch (e.g. `release/11.0.1xx-preview6`), checks the
    public release-readiness signals that don't require internal access:
        - Target branch exists with the right PreReleaseVersionIteration
        - net<major>.0 inflight branch is bumped for the NEXT preview train
        - Maestro / dependency-flow PRs
        - Release-branch human PRs
        - net<major>.0 inflight PRs (preview-next watch)
        - Priority release blockers (p/0, p/1) tagged release-relevant
        - Known Build Error issues tagged release-relevant
        - Xcode requirement variables (from eng/pipelines/common/variables.yml)
        - CI truth (placeholder — not wired to #35052 yet)
        - Internal release pipelines (READY/UNKNOWN classification — sanitized)

    Deterministic by design — does NOT approve, merge, rerun, promote, or
    mutate GitHub / Maestro / darc state.

    Output:
        - Markdown report fenced by parameterized tracker markers
            <!-- release-readiness-tracker: <TrackerKey> -->
        - JSON dump of the checks + collected PRs/issues when -OutputDir
          is supplied

.PARAMETER Branch
    Required. Preview branch name in the form:
        release/<major>.0.1xx-preview<N>
    e.g. release/11.0.1xx-preview6

.PARAMETER Mode
    'in-flight' (default) — the branch already exists, survey it directly.
    'candidate' — the branch hasn't been cut yet, survey `-SurveyRef` (the
    source branch the preview will be cut from) and treat the missing
    target branch as informational, not blocking.

.PARAMETER SurveyRef
    Branch to survey for PRs / version checks. Defaults to `-Branch`.
    For 'candidate' mode, the workflow should pass net<major>.0 (the
    upstream inflight branch the preview will be cut from).

.PARAMETER Repository
    GitHub repo to query (default dotnet/maui).

.PARAMETER OutputDir
    If supplied, writes preview-readiness.{json,md} into this directory.
    If omitted, the markdown body is written to stdout.

.PARAMETER TrackerKey
    Canonical tracker slug (e.g. "net11-preview6"). Embedded in the
    `<!-- release-readiness-tracker: $TrackerKey -->` marker so the
    workflow can idempotently match and update a single tracker issue.
    If omitted, derived from the parsed branch (net<major>-preview<N>).

.PARAMETER OutputFormat
    "markdown" (default, also written when OutputDir is set), "json"
    (stdout only), or "both" (write both files when OutputDir is set; the
    markdown body is also returned to stdout).

.PARAMETER IncludeInternal
    When set, attempts to query internal dnceng Azure DevOps via `az` CLI
    for the supplied -InternalBuildId. Only relevant for local runs by
    release captains with internal access.

.PARAMETER InternalBuildId
    Internal AzDO build ID used when -IncludeInternal is set.

.PARAMETER PublicSafe
    When true (default), any non-READY internal status is sanitized to
    omit raw error/log payloads before being included in the report.

.NOTES
    Faithfully ports the logic from the prior
    `.github/skills/net11-release-readiness/scripts/Get-Net11ReleaseReadiness.ps1`
    script (PR #35754) into the unified release-readiness skill, dropping
    the `Resolve-Target` indirection in favour of explicit `-Branch` input
    from the Find-ReleaseReadinessTrackers driver.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Branch,

    [Parameter(Mandatory = $false)]
    [ValidateSet("in-flight", "candidate")]
    [string]$Mode = "in-flight",

    [Parameter(Mandatory = $false)]
    [string]$SurveyRef,

    [Parameter(Mandatory = $false)]
    [string]$Repository = "dotnet/maui",

    [Parameter(Mandatory = $false)]
    [string]$OutputDir,

    [Parameter(Mandatory = $false)]
    [string]$TrackerKey,

    [Parameter(Mandatory = $false)]
    [ValidateSet("markdown", "json", "both")]
    [string]$OutputFormat = "markdown",

    [Parameter(Mandatory = $false)]
    [switch]$IncludeInternal,

    [Parameter(Mandatory = $false)]
    [string]$InternalBuildId,

    [Parameter(Mandatory = $false)]
    [bool]$PublicSafe = $true,

    [Parameter(Mandatory = $false)]
    [int]$MaxBodyBytes = 60000
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# Shared nightly-feed freshness helpers (Get-NightlyFeedFreshness / Format-NightlyFeedBanner).
# Defensive load: the banner is auxiliary signal, not part of the verdict, so a missing
# helper degrades to "no banner" rather than crashing the unattended preview tracker job.
# Loaded above the dot-source guard so the pure renderer is reachable from the test harness.
$Script:NightlyFeedHelperLoaded = $false
$nightlyFeedHelperPath = Join-Path $PSScriptRoot 'NightlyFeed.ps1'
if (Test-Path $nightlyFeedHelperPath) {
    . $nightlyFeedHelperPath
    $Script:NightlyFeedHelperLoaded = $true
} else {
    Write-Warning "NightlyFeed.ps1 helper not found at $nightlyFeedHelperPath — nightly-feed banner disabled." -WarningAction Continue
}

# ===================================================================
# BRANCH PARSING
# ===================================================================
# Preview branch contract: release/<major>.0.1xx-preview<N>
# (Find-Trackers emits exactly this format for branchType='preview'.)
if ($Branch -notmatch '^release/(\d+)\.0\.1xx-preview(\d+)$') {
    throw "Branch '$Branch' does not match expected preview format 'release/<major>.0.1xx-preview<N>'."
}
$majorVersion = [int]$Matches[1]
$previewNumber = [int]$Matches[2]
$mainBranch = "net$majorVersion.0"

# In candidate mode, the preview branch hasn't been cut yet — survey the
# source instead (caller passes net<major>.0 via -SurveyRef). In in-flight
# mode, the source IS the branch itself.
if ([string]::IsNullOrWhiteSpace($SurveyRef)) {
    $SurveyRef = if ($Mode -eq 'candidate') { $mainBranch } else { $Branch }
}

# Human-readable label for the daily-flow merge-up CHAIN that feeds the survey
# ref. The preview lane is a two-hop chain: main → net<N>.0 → previewN. An
# in-flight preview surveys previewN, so BOTH hops feed it (main → net<N>.0 →
# previewN). A candidate's survey ref IS net<N>.0, so the chain it sees is the
# single main → net<N>.0 hop. Used for the merge-up check Area, the high-priority
# blurb, and the carve-out list (which must stay string-equal to the check Area).
$mergeUpChainLabel = if ($Mode -eq 'candidate') { "main → $SurveyRef" } else { "main → $mainBranch → $SurveyRef" }

# Canonical tracker key. Default matches Find-Trackers' New-PreviewTracker.
if ([string]::IsNullOrWhiteSpace($TrackerKey)) {
    $TrackerKey = "net$majorVersion-preview$previewNumber"
}

# ===================================================================
# STATUS RANKING (worst-wins)
# ===================================================================
$StatusRank = @{
    "READY"             = 0
    "CLEANUP"           = 1
    "WATCH"             = 1
    "UNKNOWN"           = 2
    "INSUFFICIENT_DATA" = 2
    "BLOCKED"           = 3
}

# ===================================================================
# HELPERS
# ===================================================================

function Invoke-GitHubWithRetry {
    <#
    .SYNOPSIS
        Calls `gh` with bounded exponential backoff on transient errors.
    .DESCRIPTION
        Retries on 502/503/504/timeout/stream-error/CANCEL/Bad-Gateway up
        to MaxRetries (default 3) with 2^N * 2-second backoff.
        Throws on persistent failure — caller must wrap if soft-fail is
        wanted.
    #>
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Description,
        [Parameter(Mandatory = $false)][int]$MaxRetries = 3
    )

    $retryCount = 0
    $baseDelay = 2

    while ($retryCount -lt $MaxRetries) {
        $global:LASTEXITCODE = 0
        $output = & gh @Arguments 2>&1
        $exitCode = $LASTEXITCODE
        $text = $output -join "`n"

        if ($exitCode -eq 0) {
            return $text
        }

        $retryCount++
        if ($text -match "502|503|504|timeout|stream error|CANCEL|Bad Gateway" -and $retryCount -lt $MaxRetries) {
            Start-Sleep -Seconds ($baseDelay * [Math]::Pow(2, $retryCount - 1))
            continue
        }

        throw "Failed to $Description"
    }

    throw "Failed to $Description after $MaxRetries attempts"
}

function ConvertFrom-JsonOrEmptyArray {
    param([string]$Json)
    if ([string]::IsNullOrWhiteSpace($Json)) {
        return @()
    }
    $parsed = $Json | ConvertFrom-Json
    if ($null -eq $parsed) {
        return @()
    }
    return @($parsed)
}

function Get-ContentFromRepo {
    <#
    .SYNOPSIS
        Reads a file from the repo at a specific ref via gh api.
    #>
    param(
        [string]$Path,
        [string]$Ref
    )

    $encodedRef = [System.Uri]::EscapeDataString($Ref)
    $json = Invoke-GitHubWithRetry -Arguments @(
        "api",
        "repos/$Repository/contents/$Path`?ref=$encodedRef"
    ) -Description "fetch $Path from $Ref"

    $content = $json | ConvertFrom-Json
    if (-not $content.content) {
        throw "Content response for $Path at $Ref did not include content"
    }

    $bytes = [Convert]::FromBase64String(($content.content -replace "\s", ""))
    return [Text.Encoding]::UTF8.GetString($bytes)
}

function Test-BranchExists {
    param([string]$BranchName)

    $encodedBranch = [System.Uri]::EscapeDataString($BranchName)
    $global:LASTEXITCODE = 0
    $output = & gh api "repos/$Repository/branches/$encodedBranch" --jq ".name" 2>&1
    $exitCode = $LASTEXITCODE
    $text = $output -join "`n"

    if ($exitCode -eq 0) {
        return $true
    }

    if ($text -match '"status"\s*:\s*"404"|"message"\s*:\s*"Branch not found"|HTTP 404') {
        return $false
    }

    throw "Failed to check branch $BranchName"
}

function Get-PreReleaseVersionIteration {
    <#
    .SYNOPSIS
        Reads <PreReleaseVersionIteration> from eng/Versions.props at $Branch.
    .NOTES
        Returns the raw string (or $null if empty/missing). Cross-checked
        as `[string] -eq` against the expected preview number, so do not
        normalise to [int] here.
    #>
    param([string]$BranchName)

    $versions = Get-ContentFromRepo -Path "eng/Versions.props" -Ref $BranchName
    if ($versions -match "<PreReleaseVersionIteration>\s*([^<]*)\s*</PreReleaseVersionIteration>") {
        $value = $Matches[1].Trim()
        if ([string]::IsNullOrWhiteSpace($value)) {
            return $null
        }
        return $value
    }

    return $null
}

function Get-XcodeRequirements {
    <#
    .SYNOPSIS
        Reads REQUIRED_XCODE and DEVICETESTS_REQUIRED_XCODE from
        eng/pipelines/common/variables.yml at $Branch.
    #>
    param([string]$BranchName)

    $variables = Get-ContentFromRepo -Path "eng/pipelines/common/variables.yml" -Ref $BranchName
    $required = $null
    $deviceRequired = $null
    $currentName = $null

    foreach ($line in ($variables -split "`n")) {
        if ($line -match "^\s*-\s+name:\s+(.+?)\s*$") {
            $currentName = $Matches[1].Trim()
            continue
        }

        if ($line -match "^\s*REQUIRED_XCODE\s*:\s+(.+?)\s*$") {
            $required = $Matches[1].Trim().Trim("'").Trim('"')
            continue
        }

        if ($line -match "^\s*DEVICETESTS_REQUIRED_XCODE\s*:\s+(.+?)\s*$") {
            $deviceRequired = $Matches[1].Trim().Trim("'").Trim('"')
            continue
        }

        if ($line -match "^\s*value:\s+(.+?)\s*$") {
            $value = $Matches[1].Trim().Trim("'").Trim('"')
            if ($currentName -eq "REQUIRED_XCODE") {
                $required = $value
            } elseif ($currentName -eq "DEVICETESTS_REQUIRED_XCODE") {
                $deviceRequired = $value
            }
        }
    }

    return [PSCustomObject]@{
        RequiredXcode            = $required
        DeviceTestsRequiredXcode = $deviceRequired
    }
}

function Get-BranchComponentPins {
    <#
    .SYNOPSIS
        Reads the dotnet/dotnet (VMR), dotnet/android and dotnet/macios anchor
        pins (version + commit SHA) from eng/Version.Details.xml at $Ref.
    .DESCRIPTION
        Public git source — reachable with the repo-scoped GITHUB_TOKEN the
        GitHub Action runs under (no Maestro/BAR access required). These are the
        components CURRENTLY BUNDLED on the branch, NOT a confirmed "blessed"
        build. The authoritative blessed designation comes from the internal
        .NET Release Tracker (private dotnet/release), which CI cannot reach.

        Returns a PSCustomObject with Vmr/Android/Macios members (each with
        Name/Version/Sha), or $null if the file can't be read or parsed.
    .NOTES
        eng/Version.Details.xml `<Dependency>` elements carry Name/Version as
        XML *attributes* and Uri/Sha as child *elements*. On an XmlElement,
        `$dep.Name` resolves to the element tag ("Dependency") — NOT the Name
        attribute — so attributes are read with GetAttribute() and children via
        the `$dep['Uri']` indexer.
    #>
    param(
        [string]$Ref,
        [int]$Major
    )

    $xmlText = $null
    try {
        $xmlText = Get-ContentFromRepo -Path "eng/Version.Details.xml" -Ref $Ref
    } catch {
        Write-Warning "Could not read eng/Version.Details.xml at ${Ref}: $($_.Exception.Message)"
        return $null
    }

    $xml = $null
    try {
        $xml = [xml]$xmlText
    } catch {
        Write-Warning "Could not parse eng/Version.Details.xml at ${Ref}: $($_.Exception.Message)"
        return $null
    }

    $deps = @($xml.SelectNodes("//Dependency"))
    if ($deps.Count -eq 0) { return $null }

    # Pick the anchor dependency for a repo: match by <Uri>, then prefer the
    # most representative Name (falling back to the first dep for that repo).
    $selectPin = {
        param($UriMatch, $PreferredNamePatterns)
        $cands = @($deps | Where-Object {
            $u = ''
            if ($_['Uri']) { $u = "$($_['Uri'].InnerText)".Trim().TrimEnd('/') }
            $u -match $UriMatch
        })
        if ($cands.Count -eq 0) { return $null }
        $pick = $null
        foreach ($pat in $PreferredNamePatterns) {
            $pick = $cands | Where-Object { "$($_.GetAttribute('Name'))" -match $pat } | Select-Object -First 1
            if ($pick) { break }
        }
        if (-not $pick) { $pick = $cands[0] }
        $sha = ''
        if ($pick['Sha']) { $sha = "$($pick['Sha'].InnerText)".Trim() }
        [PSCustomObject]@{
            Name    = $pick.GetAttribute('Name')
            Version = $pick.GetAttribute('Version')
            Sha     = $sha
        }
    }

    $vmr     = & $selectPin 'github\.com/dotnet/dotnet(?![\w-])'  @('^Microsoft\.NET\.Sdk$')
    $android = & $selectPin 'github\.com/dotnet/android(?![\w-])' @('^Microsoft\.Android\.Sdk\.Windows$', '^Microsoft\.Android\.Sdk')
    $macios  = & $selectPin 'github\.com/dotnet/macios(?![\w-])'  @("^Microsoft\.macOS\.Sdk\.net$Major\.0", '^Microsoft\.macOS\.Sdk', '^Microsoft\.iOS\.Sdk')

    if (-not ($vmr -or $android -or $macios)) { return $null }

    return [PSCustomObject]@{
        Vmr     = $vmr
        Android = $android
        Macios  = $macios
    }
}

function Get-BugTemplateVersions {
    <#
    .SYNOPSIS
        Reads the `version-with-bug` dropdown options from .github/ISSUE_TEMPLATE/bug-report.yml at $Branch.
    .DESCRIPTION
        Returns an array of dropdown option strings (without leading `- ` markers).
        Used to verify the bug template has been updated to include the version
        we're about to ship — releasing a version that's missing from the template
        means users can't file bug reports against it (they'd have to pick
        "Unknown/Other"). Returns @() if the file is missing or the dropdown isn't found.
    .NOTES
        The file is a GitHub issue-form YAML. The relevant block looks like:
            - type: dropdown
              id: version-with-bug
              attributes:
                label: Version with bug
                options:
                  - 11.0.0-preview.4
                  - 10.0.70
                  ...
        We do a lightweight scan rather than parsing YAML to keep the dependency surface small.
    #>
    param([string]$BranchName)

    try {
        $yaml = Get-ContentFromRepo -Path ".github/ISSUE_TEMPLATE/bug-report.yml" -Ref $BranchName
    } catch {
        return @()
    }
    if ([string]::IsNullOrWhiteSpace($yaml)) { return @() }

    $lines = $yaml -split "`n"
    $inVersionDropdown = $false
    $inOptions = $false
    $optionsIndent = -1
    $values = New-Object System.Collections.Generic.List[string]

    foreach ($rawLine in $lines) {
        $line = $rawLine.TrimEnd("`r")

        # Detect entry into the `version-with-bug` dropdown's options block.
        if (-not $inVersionDropdown) {
            if ($line -match '^\s*id:\s*version-with-bug\s*$') {
                $inVersionDropdown = $true
            }
            continue
        }

        # Once inside the dropdown, look for `options:` and capture its child indent.
        if (-not $inOptions) {
            if ($line -match '^(\s*)options:\s*$') {
                $inOptions = $true
                $optionsIndent = $Matches[1].Length
            }
            # Bail out if we hit the next top-level block before finding options.
            if ($line -match '^\s*-\s*type:\s*') { break }
            continue
        }

        # We're inside the options list. Capture `- value` rows.
        if ($line -match '^(\s*)-\s+(.+?)\s*$') {
            $indent = $Matches[1].Length
            if ($indent -gt $optionsIndent) {
                $value = $Matches[2].Trim()
                # Strip surrounding quotes if any
                $value = $value.Trim("'").Trim('"')
                if (-not [string]::IsNullOrWhiteSpace($value)) {
                    [void]$values.Add($value)
                }
                continue
            }
        }

        # Empty or differently-indented line ends the options block.
        if ($line -match '^\s*$') { continue }
        if ($line -match '^(\s*)\S' -and $Matches[1].Length -le $optionsIndent) {
            break
        }
    }

    return @($values)
}

function Get-OpenPullRequests {
    param([string]$BaseBranch)

    if (-not (Test-BranchExists -BranchName $BaseBranch)) {
        return @()
    }

    $json = Invoke-GitHubWithRetry -Arguments @(
        "pr",
        "list",
        "--repo",
        $Repository,
        "--state",
        "open",
        "--base",
        $BaseBranch,
        "--limit",
        "100",
        "--json",
        "number,title,author,url,createdAt,updatedAt,isDraft,reviewDecision,mergeStateStatus,labels,headRefName,baseRefName"
    ) -Description "list open PRs for $BaseBranch"

    return ConvertFrom-JsonOrEmptyArray $json
}

function Get-MergedPullRequests {
    <#
    .SYNOPSIS
        Fetches recently MERGED PRs into $BaseBranch (for dependency-flow /
        subscription-health inference). Public data — readable in CI with the
        repo-scoped GITHUB_TOKEN.
    .NOTES
        `mergedAt` is requested so callers can date each merge; an open PR (from
        Get-OpenPullRequests) has no `mergedAt`, which is how Get-ComponentFlowSignal
        tells "open (flowing now)" from "merged N days ago". Newest-first by default.
    #>
    param([string]$BaseBranch)

    if (-not (Test-BranchExists -BranchName $BaseBranch)) {
        return @()
    }

    $json = Invoke-GitHubWithRetry -Arguments @(
        "pr",
        "list",
        "--repo",
        $Repository,
        "--state",
        "merged",
        "--base",
        $BaseBranch,
        "--limit",
        "100",
        "--json",
        "number,title,author,url,createdAt,updatedAt,mergedAt,labels,headRefName,baseRefName"
    ) -Description "list merged PRs for $BaseBranch"

    return ConvertFrom-JsonOrEmptyArray $json
}

function Get-IssuesByLabel {
    param(
        [string]$Label,
        [switch]$IncludeBody
    )

    $fields = "number,title,url,labels,milestone,createdAt,updatedAt"
    if ($IncludeBody) { $fields += ",body" }

    $json = Invoke-GitHubWithRetry -Arguments @(
        "issue",
        "list",
        "--repo",
        $Repository,
        "--state",
        "open",
        "--limit",
        "100",
        "--label",
        $Label,
        "--json",
        $fields
    ) -Description "list issues with label '$Label'"

    return ConvertFrom-JsonOrEmptyArray $json
}

function Get-AllMilestones {
    <#
    .SYNOPSIS
        Fetches all milestones (open + closed) for the repo via `gh api`.
        Returns a Success/Data envelope so callers can distinguish
        "API call failed" from "no milestones exist".
    .NOTES
        Query parameters MUST be embedded in the URL — passing them via `-f`
        switches `gh api` to POST mode (form body), which the milestones
        endpoint rejects with HTTP 422. A successful milestones query always
        returns at least `[]`; empty/failed output is surfaced as
        Success=$false rather than masked as "zero milestones" (which would
        let the milestone-existence check silently pass on a gh outage).
        Kept in sync with the identically-named helper in Get-ReleaseReadiness.ps1.
    #>
    try {
        $raw = Invoke-GitHubWithRetry -Arguments @(
            'api', "repos/$Repository/milestones?state=all&per_page=100", '--paginate'
        ) -Description "list milestones for $Repository"
        if ([string]::IsNullOrWhiteSpace($raw)) {
            return [PSCustomObject]@{ Success = $false; Data = @() }
        }
        $parsed = $raw | ConvertFrom-Json
        return [PSCustomObject]@{ Success = $true; Data = @($parsed) }
    } catch {
        return [PSCustomObject]@{ Success = $false; Data = @() }
    }
}

function Test-PreviewMilestoneExists {
    <#
    .SYNOPSIS
        Checks whether the GitHub milestone for THIS preview
        (e.g. ".NET 11.0-preview7") exists in the repo.
    .DESCRIPTION
        A preview release-readiness tracker and its milestone are coupled: if
        a tracker exists, the milestone must exist right away — not deferred to
        cut time — so fixed issues have a milestone to land on and the
        release-notes generator has something to query. A missing milestone is
        therefore a real ship-readiness gap (BLOCKED), mirroring the SR lane's
        current-cycle rule in Get-ReleaseReadiness.ps1 (which likewise treats a
        candidate tracker's OWN milestone as blocking).

        Accepts the modern ".NET <major>.0-preview<N>" title and the legacy
        "<major>.0-preview<N>" form (case-insensitive), so a valid milestone
        under older naming isn't mis-flagged as missing.

        Returns @{ QueryFailed=[bool]; Exists=[bool]; ExpectedTitle=[string];
                   MatchedTitle=[string] }. QueryFailed=$true (gh outage) must
        be surfaced as UNKNOWN by the caller, never as a false BLOCK.
    #>
    param(
        [int]$Major,
        [int]$Preview
    )

    $expected = ".NET $Major.0-preview$Preview"
    $ms = Get-AllMilestones
    if (-not $ms.Success) {
        return @{ QueryFailed = $true; Exists = $false; ExpectedTitle = $expected; MatchedTitle = $null }
    }

    $acceptable = @(
        ".net $Major.0-preview$Preview",
        "$Major.0-preview$Preview"
    )
    $match = $null
    foreach ($m in $ms.Data) {
        $t = "$($m.title)".Trim().ToLowerInvariant()
        if ($acceptable -contains $t) { $match = $m; break }
    }

    if ($match) {
        return @{ QueryFailed = $false; Exists = $true; ExpectedTitle = $expected; MatchedTitle = $match.title }
    }
    return @{ QueryFailed = $false; Exists = $false; ExpectedTitle = $expected; MatchedTitle = $null }
}

function Get-CiScanLabelForBranch {
    <#
    .SYNOPSIS
        Maps a branch/ref name to the single `ci-scan*` label its scanner
        workflow writes. Returns $null when no scanner runs against the ref.
    .DESCRIPTION
        The CI Failure Scanner has one workflow per scanned branch
        (.github/workflows/ci-status-main.md → 'main' → 'ci-scan';
         .github/workflows/ci-status-net11.md → 'net11.0' → 'ci-scan-net11').
        Label name fully encodes the branch — no need to crack open the
        issue body to figure out where it came from.

        Mapping:
          main                                  → ci-scan
          netN.0                                → ci-scan-netN
          release/N.0.<patch>xx-previewM        → ci-scan-netN   (upstream)
          release/N.0.<patch>xx-srM             → $null          (no scanner)
          anything else                         → $null          (no scanner)

        Preview branches return the parent net<N>.0 label so an in-flight
        preview readiness check still surfaces signals from the branch
        the preview was cut from — the per-branch ci-status-*.md workflow
        runs against net<N>.0, not the preview branch.

        Add a case here when a new ci-status-*.md workflow is introduced.
        Must be kept in sync with the matching helper in
        scripts/Get-ReleaseReadiness.ps1.
    #>
    param([string]$Branch)

    if ([string]::IsNullOrWhiteSpace($Branch)) { return $null }
    if ($Branch -eq 'main') { return 'ci-scan' }
    if ($Branch -match '^net(\d+)\.0$') { return "ci-scan-net$($Matches[1])" }
    if ($Branch -match '^release/(\d+)\.0\.\d+xx-preview\d+$') {
        return "ci-scan-net$($Matches[1])"
    }
    return $null
}

function Get-CiScanIssues {
    <#
    .SYNOPSIS
        Returns open ci-scan issues for the scanner attached to $Branch.
        Returns @{ Matched=[array]; FilteredOut=int; Total=int;
                   QueryFailed=[bool]; ScannerLabel=[string]|$null }.
    .DESCRIPTION
        Uses Get-CiScanLabelForBranch to resolve the single relevant label
        (e.g. net11.0 → ci-scan-net11) and queries only that one — no more
        cross-branch dedup or body marker parsing. When the branch has no
        scanner, ScannerLabel is $null and Matched is empty.

        QueryFailed flips $true if the underlying `gh issue list` call
        throws after retries. Callers must treat that case as "no signal"
        rather than "no issues" to avoid emitting a false-green READY on
        tool failure.
    #>
    param([string]$Branch)

    $label = Get-CiScanLabelForBranch -Branch $Branch
    if (-not $label) {
        return @{
            Matched      = @()
            FilteredOut  = 0
            Total        = 0
            QueryFailed  = $false
            ScannerLabel = $null
        }
    }

    try {
        $batch = Get-IssuesByLabel -Label $label -IncludeBody
    } catch {
        return @{
            Matched      = @()
            FilteredOut  = 0
            Total        = 0
            QueryFailed  = $true
            ScannerLabel = $label
        }
    }

    $sorted = @($batch | Sort-Object {
        $u = ConvertTo-UtcDateTime -Value $_.createdAt
        if ($u) { $u } else { [DateTime]::MinValue }
    } -Descending)

    return @{
        Matched      = $sorted
        FilteredOut  = 0
        Total        = $sorted.Count
        QueryFailed  = $false
        ScannerLabel = $label
    }
}

function Test-IssueHasForeignMajor {
    <#
    .SYNOPSIS
        Returns $true if the text explicitly names a .NET major version that
        differs from $Major (e.g. a `regressed-in-10-*` label or a `.NET 10`
        milestone when surveying major 11).
    .NOTES
        The bare "previewN" phrase is major-ambiguous — every major has a
        previewN. Regression labels encode the major (`regressed-in-10-preview7`
        is a .NET 10 label), so without this guard a .NET 10 preview7 issue
        leaks onto the .NET 11 preview7 tracker. This detector lets the
        relevance check reject a previewN match when a *different* major is
        explicitly named.

        A foreign major is only recognized when the digits are anchored to an
        explicit .NET token — `net`, `.net`, or `regressed-in-` — so unrelated
        OS/tool versions that pepper MAUI issue titles (`Android 15.0`,
        `iOS 18.0`, `macOS 14.0`, `VS 17.0`) are NOT mistaken for .NET majors
        and cannot silently drop a genuine, still-untriaged p/0 report. The
        6..99 bound is defense-in-depth against a stray build number sneaking
        in behind an anchor.
    #>
    param(
        [string]$Haystack,
        [int]$Major
    )

    if ([string]::IsNullOrWhiteSpace($Haystack)) { return $false }

    foreach ($m in [regex]::Matches($Haystack, "(?i)(?:net\s*|\.net\s*|regressed-in-)(\d+)")) {
        $val = 0
        if ([int]::TryParse($m.Groups[1].Value, [ref]$val) -and $val -ge 6 -and $val -le 99 -and $val -ne $Major) {
            return $true
        }
    }

    return $false
}

function Test-IssueReleaseRelevant {
    <#
    .SYNOPSIS
        Returns $true if a labelled issue is plausibly relevant to the
        active major / preview number based on its title, milestone, or
        labels.
    .NOTES
        Uses a wide net on purpose — false negatives are worse than false
        positives for release-readiness triage. The one exception is a
        *cross-major* collision: the bare "previewN" phrase matches every
        major's previewN, so it is only honored when the issue carries no
        contradicting foreign-major signal (see Test-IssueHasForeignMajor).
    #>
    param(
        $Issue,
        [int]$Major,
        [int]$Preview
    )

    $labels = @($Issue.labels | ForEach-Object { $_.name })
    $milestone = if ($Issue.milestone -and $Issue.milestone.title) { $Issue.milestone.title } else { "" }
    $haystack = "$($Issue.title) $milestone $($labels -join ' ')"

    $majorRx = "(?i)net\s*$Major|net$Major|$Major\.0|$Major\.0\.1xx|xcode"
    if ($haystack -match $majorRx) {
        return $true
    }

    if ($haystack -match "(?i)preview\s*$Preview|preview$Preview") {
        # "previewN" alone is major-ambiguous; reject it when the issue
        # explicitly names a different major (e.g. regressed-in-10-preview7
        # surveyed against major 11).
        if (-not (Test-IssueHasForeignMajor -Haystack $haystack -Major $Major)) {
            return $true
        }
    }

    return $false
}

function Get-ReleaseRelevantIssuesByLabel {
    param(
        [string[]]$Labels,
        [int]$Major,
        [int]$Preview
    )

    $issues = @()
    foreach ($label in $Labels) {
        $issues += Get-IssuesByLabel -Label $label
    }

    $deduped = $issues |
        Sort-Object number -Unique |
        Where-Object { Test-IssueReleaseRelevant -Issue $_ -Major $Major -Preview $Preview }

    # PowerShell unwraps single-element arrays on function return, so a
    # naked `return @($deduped)` with a $null/empty pipeline result yields
    # $null at the call site (then `.Count` blows up under StrictMode).
    # The leading comma forces a single-element outer array containing our
    # real array, which PowerShell unwraps to the inner array — preserving
    # the array type even when empty.
    if ($null -eq $deduped) { return ,@() }
    return ,@($deduped)
}

function Test-IssueIsFresh {
    <#
    .SYNOPSIS
        Returns $true if the issue was created within the last $HoursThreshold
        hours. Used to escalate ci-scan checks to WATCH when scanner activity
        is recent.
    #>
    param($Issue, [int]$HoursThreshold = 24)

    if (-not $Issue.PSObject.Properties['createdAt'] -or -not $Issue.createdAt) { return $false }
    $createdUtc = ConvertTo-UtcDateTime -Value $Issue.createdAt
    if (-not $createdUtc) { return $false }
    return ((Get-Date).ToUniversalTime() - $createdUtc).TotalHours -lt $HoursThreshold
}

function ConvertTo-UtcDateTime {
    <#
    .SYNOPSIS
        Normalizes a value that may be a DateTime (Utc/Local/Unspecified) or a
        string into a UTC DateTime. Returns $null if conversion fails.
    .NOTES
        ConvertFrom-Json parses ISO-8601 'Z' strings into DateTime with Kind=Utc,
        but [DateTime]::Parse on a string returns Kind=Unspecified, which
        .ToUniversalTime() then misinterprets as Local — silently shifting by
        the host's UTC offset. Use this helper everywhere age is computed.
    #>
    param([object]$Value)

    if ($null -eq $Value) { return $null }

    if ($Value -is [DateTime]) {
        if ($Value.Kind -eq [DateTimeKind]::Utc)   { return $Value }
        if ($Value.Kind -eq [DateTimeKind]::Local) { return $Value.ToUniversalTime() }
        return [DateTime]::SpecifyKind($Value, [DateTimeKind]::Utc)
    }

    try {
        $dto = [DateTimeOffset]::Parse([string]$Value, [Globalization.CultureInfo]::InvariantCulture)
        return $dto.UtcDateTime
    } catch {
        return $null
    }
}

function Get-PRAction {
    <#
    .SYNOPSIS
        Maps PR state to a {Status, Action, Age} verdict.
    #>
    param($PR)

    $labels = @($PR.labels | ForEach-Object { $_.name })
    $ageDays = [Math]::Round(((Get-Date) - [DateTime]::Parse($PR.createdAt, [Globalization.CultureInfo]::InvariantCulture)).TotalDays)

    if ($PR.isDraft) {
        return [PSCustomObject]@{ Status = "WATCH"; Action = "Draft PR; wait until ready for review."; Age = $ageDays }
    }
    if ($labels -contains "do-not-merge") {
        return [PSCustomObject]@{ Status = "BLOCKED"; Action = "do-not-merge label present; resolve blocker before release."; Age = $ageDays }
    }
    if ($PR.mergeStateStatus -eq "DIRTY") {
        return [PSCustomObject]@{ Status = "BLOCKED"; Action = "Resolve merge conflicts."; Age = $ageDays }
    }
    if ($PR.reviewDecision -eq "APPROVED") {
        return [PSCustomObject]@{ Status = "WATCH"; Action = "Approved; verify release owner is ready to merge when CI/release gates allow."; Age = $ageDays }
    }
    if ($PR.reviewDecision -eq "CHANGES_REQUESTED") {
        return [PSCustomObject]@{ Status = "BLOCKED"; Action = "Changes requested; author/release owner follow-up required."; Age = $ageDays }
    }
    return [PSCustomObject]@{ Status = "WATCH"; Action = "Needs review or triage."; Age = $ageDays }
}

function Test-IsP0Pr {
    <#
    .SYNOPSIS
        True when a PR object carries the release-blocking 'p/0' label.
    .DESCRIPTION
        Mirrors the issue-side p/0 detection so a p/0-labelled PR targeting the
        release branch is surfaced as a blocker (not buried in the generic PR
        WATCH count). StrictMode-safe: a PR with a missing or null `labels`
        property yields an empty array (-> $false) instead of throwing. Accepts
        both PSCustomObject (the production `gh ... --json` shape) and
        IDictionary/hashtable (the shape test mocks commonly use), mirroring the
        dual-shape handling in Get-ReleaseReadiness.ps1.
    #>
    param($PR)

    if (-not $PR) { return $false }
    $labels = if ($PR -is [System.Collections.IDictionary]) {
        if ($PR.Contains('labels')) { $PR['labels'] } else { $null }
    } elseif ($PR.PSObject.Properties['labels']) {
        $PR.labels
    } else {
        $null
    }
    if (-not $labels) { return $false }
    return (@($labels | ForEach-Object { $_.name }) -contains 'p/0')
}

function Test-IsDependencyFlowPr {
    <#
    .SYNOPSIS
        True when a PR is a dependency-flow / component-bump PR — either an
        automated darc PR (author dotnet-maestro) OR a human-authored component
        bump that rolls upstream builds into a branch.
    .DESCRIPTION
        The dependency-flow bucket was originally author-only (`dotnet-maestro`).
        But release owners also open MANUAL component-bump PRs — e.g.
        "[release/11.0.1xx-preview6] Bump dotnet/dotnet (BAR 321614), dotnet/android
        (BAR 321622) and dotnet/macios (BAR 321780)" (head `update-321614`, authored
        by a human) — which land the blessed component set into the release branch
        and are just as release-critical as a darc PR. Author-only detection buried
        those in the generic "Release branch PRs" list instead of surfacing them in
        the hoisted 🔴 High-priority items. Detect them by title / head-ref too.

        Signals (ANY one is sufficient):
          - author login matches `dotnet-maestro` (the automated darc flow);
          - title has the word "Bump" plus a dotnet dependency repo
            (dotnet/dotnet|android|macios|runtime|sdk|emsdk|wasm) or a "BAR <id>";
          - head ref matches `update-<digits>` (the manual bump-PR branch scheme,
            e.g. `update-321614`).

        Deliberately NARROW so it doesn't swallow unrelated human PRs: a generic
        "Fix Y" or a "[automated] Merge branch" merge-up PR matches none of the
        signals. StrictMode-safe: tolerates a missing author/title/headRefName.
        Accepts both PSCustomObject (the gh --json shape) and IDictionary
        (the shape test mocks commonly use), mirroring Test-IsP0Pr.
    #>
    param($PR)

    if (-not $PR) { return $false }

    # --- author login (dual-shape, StrictMode-safe) ---
    $author = if ($PR -is [System.Collections.IDictionary]) {
        if ($PR.Contains('author')) { $PR['author'] } else { $null }
    } elseif ($PR.PSObject.Properties['author']) {
        $PR.author
    } else { $null }
    if ($author) {
        $login = if ($author -is [System.Collections.IDictionary]) {
            if ($author.Contains('login')) { $author['login'] } else { $null }
        } elseif ($author.PSObject.Properties['login']) {
            $author.login
        } else { $null }
        if ($login -and $login -match 'dotnet-maestro') { return $true }
    }

    # --- title (component / BAR bump) ---
    $title = if ($PR -is [System.Collections.IDictionary]) {
        if ($PR.Contains('title')) { $PR['title'] } else { $null }
    } elseif ($PR.PSObject.Properties['title']) {
        $PR.title
    } else { $null }
    if ($title -and $title -match '(?i)\bBump\b.*(dotnet/(dotnet|android|macios|runtime|sdk|emsdk|wasm)|\bBAR\s*\d+)') { return $true }

    # --- head ref (manual bump-branch scheme `update-<barId>`) ---
    $head = if ($PR -is [System.Collections.IDictionary]) {
        if ($PR.Contains('headRefName')) { $PR['headRefName'] } else { $null }
    } elseif ($PR.PSObject.Properties['headRefName']) {
        $PR.headRefName
    } else { $null }
    if ($head -and $head -match '(?i)^update-\d+$') { return $true }

    return $false
}

function Test-IsSdkBumpPr {
    <#
    .SYNOPSIS
        True when a PR bumps the .NET SDK/VMR (dotnet/dotnet or dotnet/sdk).
    .DESCRIPTION
        A narrow subset of Test-IsDependencyFlowPr: specifically the VMR/SDK bump,
        whose landed pin advances the branch's SDK version. That new pin is only a
        *candidate* until the blessed build is confirmed locally (the Action can't
        reach the .NET Release Tracker), so callers add a "verify blessed locally"
        emphasis for these. Matches on TITLE only ("Bump dotnet/dotnet …" /
        "Bump dotnet/sdk …"); android / macios / runtime bumps are intentionally
        NOT flagged as SDK bumps. The repo segment is bounded by a negative
        look-ahead `(?![\w-])` (not a bare `\b`) so a hyphenated sibling such as
        `dotnet/dotnet-optimization` is NOT misclassified as an SDK bump — `\b`
        sits between `t` and `-` and would have matched it. StrictMode-safe,
        dual-shape (PSCustomObject / IDictionary), mirroring Test-IsDependencyFlowPr.
    #>
    param($PR)

    if (-not $PR) { return $false }

    $title = if ($PR -is [System.Collections.IDictionary]) {
        if ($PR.Contains('title')) { $PR['title'] } else { $null }
    } elseif ($PR.PSObject.Properties['title']) {
        $PR.title
    } else { $null }

    return [bool]($title -and $title -match '(?i)\bBump\b.*dotnet/(dotnet|sdk)(?![\w-])')
}

function Get-ComponentFlowSignal {
    <#
    .SYNOPSIS
        Best-effort inference of one component's dependency-flow (subscription)
        health from PUBLIC PR history — no Maestro/darc access required.
    .DESCRIPTION
        The GitHub Action cannot read Maestro subscription config (that lives in
        the AzDO-internal maestro-configuration repo). But every *working*
        subscription leaves a public trail: dependency-flow PRs into the branch
        — darc "Update dependencies from dotnet/<repo> …" or human "Bump
        dotnet/<repo> …". This scans that trail (open + merged dep-flow PRs) for
        a single source repo and classifies:
          * open    — an unmerged dep-flow PR for this repo is open right now
                      (flow is actively happening) → healthy
          * fresh   — newest dep-flow PR merged within $StaleDays → healthy
          * stale   — newest dep-flow PR merged, but older than $StaleDays. This
                      is the silent-decay case: the subscription exists but has
                      stopped producing PRs ("it's been a while… 🤨") → suspicious
          * missing — NO dep-flow PR for this repo into this branch at all: the
                      subscription may never have been wired (or the branch is
                      brand new) → suspicious
        This is an INFERENCE from public signal, NOT the subscription itself; the
        authoritative wiring is confirmed locally (darc get-subscriptions).
    .NOTES
        Feed ONLY open + merged dep-flow PRs (never closed-unmerged): a fed PR
        with no `mergedAt` is treated as open. Repo match uses a trailing
        negative lookahead so 'dotnet/dotnet' does not collide with
        'dotnet/dotnet-optimization'. StrictMode-safe, dual-shape.
    #>
    param(
        [string]$Repo,
        [array]$DepFlowPRs,
        [datetime]$Now = ([DateTime]::UtcNow),
        [int]$StaleDays = 14
    )

    $none = [PSCustomObject]@{ Repo = $Repo; Status = 'missing'; Number = $null; Url = $null; AgeDays = $null }

    if (-not $DepFlowPRs -or $DepFlowPRs.Count -eq 0) { return $none }

    # dual-shape field reader (StrictMode-safe)
    $field = {
        param($obj, $name)
        if ($null -eq $obj) { return $null }
        if ($obj -is [System.Collections.IDictionary]) {
            if ($obj.Contains($name)) { return $obj[$name] } else { return $null }
        } elseif ($obj.PSObject.Properties[$name]) { return $obj.$name } else { return $null }
    }

    $pattern = "(?i)$([regex]::Escape($Repo))(?![\w-])"

    $hits = @()
    foreach ($pr in $DepFlowPRs) {
        if (-not $pr) { continue }
        $title = & $field $pr 'title'
        if ($title -and ($title -match $pattern)) { $hits += $pr }
    }
    if ($hits.Count -eq 0) { return $none }

    # Prefer an OPEN dep-flow PR (flow is happening right now) — no mergedAt.
    $openHits = @($hits | Where-Object { -not (& $field $_ 'mergedAt') })
    if ($openHits.Count -gt 0) {
        $pick = $openHits |
            Sort-Object -Descending { ConvertTo-UtcDateTime (& $field $_ 'createdAt') } |
            Select-Object -First 1
        return [PSCustomObject]@{
            Repo    = $Repo
            Status  = 'open'
            Number  = (& $field $pick 'number')
            Url     = (& $field $pick 'url')
            AgeDays = $null
        }
    }

    # Otherwise the newest MERGED dep-flow PR decides fresh vs stale.
    $pick = $hits |
        Sort-Object -Descending { ConvertTo-UtcDateTime (& $field $_ 'mergedAt') } |
        Select-Object -First 1
    $mergedUtc = ConvertTo-UtcDateTime (& $field $pick 'mergedAt')
    $ageDays = if ($mergedUtc) { [Math]::Round(($Now - $mergedUtc).TotalDays) } else { $null }
    $status = if ($null -ne $ageDays -and $ageDays -gt $StaleDays) { 'stale' } else { 'fresh' }

    return [PSCustomObject]@{
        Repo    = $Repo
        Status  = $status
        Number  = (& $field $pick 'number')
        Url     = (& $field $pick 'url')
        AgeDays = $ageDays
    }
}

function Format-FlowSignalCell {
    <#
    .SYNOPSIS
        Render the Flow-signal table cell for one component from a
        Get-ComponentFlowSignal result. Pure/string-only so it is unit-testable.
    .DESCRIPTION
        The 'missing' status normally renders as "❌ none seen — sub may be
        missing" (we looked at the public PR trail and found nothing → the sub may
        never have been wired). But when the merged-PR history could NOT be fetched
        (transient gh failure caught upstream), 'missing' is NOT a trustworthy
        "none seen" — we simply couldn't look. In that case -HistoryUnavailable
        renders an honest "couldn't check" cell instead of a false absence claim.
    #>
    param(
        [Parameter(Mandatory)] $Flow,
        [switch]$HistoryUnavailable
    )
    $ageTxt = ''
    if ($null -ne $Flow.AgeDays) {
        $ageTxt = if ($Flow.AgeDays -le 0) { 'today' } elseif ($Flow.AgeDays -eq 1) { '1 day ago' } else { "$($Flow.AgeDays) days ago" }
    }
    switch ($Flow.Status) {
        'open'  { "🔄 [#$($Flow.Number)]($($Flow.Url)) open — flowing" }
        'fresh' { "✅ [#$($Flow.Number)]($($Flow.Url)) merged $ageTxt" }
        'stale' { "⚠️ stale — newest [#$($Flow.Number)]($($Flow.Url)) merged $ageTxt" }
        default {
            if ($HistoryUnavailable) { "— _merged-PR history unavailable (transient); re-run to confirm sub health_" }
            else { "❌ none seen — sub may be missing" }
        }
    }
}

function Get-UpstreamDriftSignal {
    <#
    .SYNOPSIS
        Best-effort detection of whether an upstream component repo's same-named
        branch has advanced past the commit maui currently pins — a public,
        CI-reachable "the source moved, you may want to pull it in" FYI.
    .DESCRIPTION
        maui pins a specific SHA of each component (dotnet/dotnet, dotnet/android,
        dotnet/macios) in eng/Version.Details.xml. Those repos are PUBLIC, so the
        GitHub Action can — with only GITHUB_TOKEN — ask GitHub's *compare* API
        whether that repo's branch has newer commits than our pin. This is a hard
        git fact, complementary to Get-ComponentFlowSignal (which infers whether a
        subscription is *producing PRs into maui*): a sub can be healthy and quiet
        while upstream just landed a fix we haven't pulled — only THIS check sees it.

        Branch is DERIVED, never hardcoded: we look up $BranchName (the survey ref,
        e.g. release/11.0.1xx-preview6) on the component repo via git/matching-refs.
        All three components mirror maui's branch naming today; if a future one
        doesn't, we degrade to 'unknown' rather than false-alarm.

        Compares base=our pin, head=branch tip. GitHub returns ahead_by (commits the
        branch has beyond our pin) and behind_by (commits our pin has the branch
        lacks). Classifies:
          * current   — ahead_by 0 & behind_by 0: our pin IS the branch tip.
          * ahead     — ahead_by N & behind_by 0: N newer upstream commits we don't
                        bundle yet. FYI only — maui pins BLESSED builds deliberately,
                        so these may be post-preview churn or not-yet-blessed work.
          * diverged  — behind_by > 0: our pin isn't a clean ancestor of the branch
                        tip (build tag / different line) — can't cleanly count "ahead".
          * unknown   — no pin SHA, no same-named upstream branch, or an API error.
    .NOTES
        Soft-fail: any lookup/parse failure returns 'unknown' with a Reason and
        never throws (must not break report generation). ~2 gh calls per component.
        dotnet/dotnet (VMR) moves constantly and flows on manual trigger, so a large
        ahead_by there is expected churn, rarely actionable — the caller labels it.
    #>
    param(
        [string]$Repo,        # e.g. 'dotnet/macios'
        [string]$Sha,         # our pinned SHA (base of the compare)
        [string]$BranchName,  # survey ref, e.g. 'release/11.0.1xx-preview6'
        [scriptblock]$Fetcher # optional seam: (ApiPath) -> parsed JSON. Defaults to live gh.
    )

    # Default fetcher hits the public GitHub REST API via the retrying gh wrapper
    # and returns parsed JSON. Tests inject a mock that returns fixtures instead.
    if (-not $Fetcher) {
        $Fetcher = {
            param($ApiPath)
            $json = Invoke-GitHubWithRetry -Arguments @("api", $ApiPath) -Description "gh api $ApiPath" -MaxRetries 2
            if ([string]::IsNullOrWhiteSpace($json)) { return $null }
            return ($json | ConvertFrom-Json)
        }
    }

    $result = [PSCustomObject]@{
        Repo     = $Repo
        Status   = 'unknown'
        AheadBy  = $null
        BehindBy = $null
        Branch   = $BranchName
        Url      = $null
        Reason   = $null
    }

    if ([string]::IsNullOrWhiteSpace($Sha)) {
        $result.Reason = 'no pin SHA'
        return $result
    }
    if ([string]::IsNullOrWhiteSpace($BranchName)) {
        $result.Reason = 'no branch to compare'
        return $result
    }

    # 1) Confirm the component repo actually has a branch of this name (derive,
    #    don't assume). matching-refs is an exact-PREFIX match, so require an exact
    #    full-ref hit — otherwise 'release/11.0.1xx-preview6' would also match a
    #    hypothetical 'release/11.0.1xx-preview60'.
    try {
        $refs = & $Fetcher "repos/$Repo/git/matching-refs/heads/$BranchName"
        $branchFound = @($refs | Where-Object { $_.ref -eq "refs/heads/$BranchName" }).Count -gt 0
    } catch {
        $result.Reason = 'branch lookup failed'
        return $result
    }
    if (-not $branchFound) {
        $result.Reason = 'no same-named upstream branch'
        return $result
    }

    # 2) Compare our pin (base) against the branch tip (head). The '/' in the
    #    branch name is passed through literally — GitHub's compare endpoint
    #    accepts slashed branch names as the head ref.
    try {
        $cmp = & $Fetcher "repos/$Repo/compare/$Sha...$BranchName"
    } catch {
        $result.Reason = 'compare failed'
        return $result
    }

    if ($null -eq $cmp -or
        -not $cmp.PSObject.Properties['ahead_by'] -or $null -eq $cmp.ahead_by -or
        -not $cmp.PSObject.Properties['behind_by'] -or $null -eq $cmp.behind_by) {
        $result.Reason = 'compare returned no counts'
        return $result
    }

    $aheadBy  = [int]$cmp.ahead_by
    $behindBy = [int]$cmp.behind_by
    $result.AheadBy  = $aheadBy
    $result.BehindBy = $behindBy
    $result.Url      = "https://github.com/$Repo/compare/$Sha...$BranchName"

    if ($behindBy -gt 0) {
        $result.Status = 'diverged'
    } elseif ($aheadBy -gt 0) {
        $result.Status = 'ahead'
    } else {
        $result.Status = 'current'
    }
    $result.Reason = $null
    return $result
}

function Get-CategorizedPullRequests {
    <#
    .SYNOPSIS
        Splits the open release/inflight PRs into mutually-exclusive buckets:
        P/0, Maestro (dependency-flow), merge-up, generic-human (target), and
        inflight-human.
    .DESCRIPTION
        Single source of truth for PR categorization precedence, shared by the
        engine driver and its unit tests so the tests exercise the REAL filter
        expressions (not a re-implementation). Precedence, highest first:

          1. P/0  — any survey-ref PR carrying the 'p/0' label, REGARDLESS of
                    author or merge-up status. P/0 is the strongest release
                    signal: a p/0-labelled Maestro or merge-up PR escalates to
                    the P/0 blocker category (trips its dedicated BLOCKED check +
                    renders once as a 🔥 P/0 PR row) and is never silently
                    downgraded to a 📦 Maestro / merge-up row.
          2. Maestro  — non-P/0 PRs authored by dotnet-maestro on the TARGET
                    branch only (the survey ref). net<major>.0 (inflight) Maestro
                    bumps are intentionally NOT reported by a branched preview
                    tracker — they belong to the inflight branch's own readiness.
          3. Merge-up — non-P/0, non-Maestro target PRs that are automated
                    main → survey-ref merges (head `merge/<x>-to-<y>` or title
                    "[automated] Merge branch ...").
          4. Generic-human (target) — the remaining survey-ref PRs.
          5. Inflight-human — non-Maestro PRs on the inflight (net<major>.0) branch.

        Buckets are mutually exclusive by PR number. Inflight PRs never escalate
        to P/0 (only survey-ref PRs block), and Maestro is scoped to $TargetPRs
        only — a branched preview reports dependency-flow bumps against its own
        branch, so net<major>.0 (inflight) Maestro PRs are intentionally dropped
        from this tracker (only non-Maestro inflight PRs surface, in the
        Inflight-human bucket). $p0PrNumbers is computed from $TargetPRs only, and
        PR numbers are globally unique. StrictMode-safe on two fronts: (1) inputs
        are normalized to drop $null elements up front, so an AutomationNull list
        (what
        Get-OpenPullRequests returns for a zero-PR branch) can't seed a `@($null)`
        whose null element would throw "property 'author' cannot be found"; and
        (2) for genuine PR objects every property accessed is guaranteed present by
        Get-OpenPullRequests' --json projection, with `-and` short-circuits keeping
        a null author from dereferencing `.login`.
    .OUTPUTS
        PSCustomObject with arrays: P0Prs, MaestroPRs, MergeUpPRs, TargetHumanPRs,
        InflightHumanPRs.
    #>
    param(
        [array]$TargetPRs = @(),
        [array]$InflightPRs = @()
    )

    # Normalize inputs: the driver assigns these from Get-OpenPullRequests, which
    # returns AutomationNull for a branch with zero open PRs (an empty `gh pr list`
    # result collapses through `return @()`). When AutomationNull is bound to an
    # [array] parameter the parameter becomes $null (NOT the `= @()` default — the
    # default only applies when the argument is omitted), and `@($null)` then yields
    # a single-element array whose lone element is $null. Iterating that under
    # `Set-StrictMode -Version Latest` and dereferencing `$_.author` throws
    # "The property 'author' cannot be found on this object". Stripping nulls here
    # makes the function robust to null / AutomationNull / @($null) inputs — the
    # realistic trigger is an in-flight run against a freshly-cut release branch
    # that exists but has no PRs yet while the inflight (net<major>.0) branch does.
    $TargetPRs   = @($TargetPRs   | Where-Object { $null -ne $_ })
    $InflightPRs = @($InflightPRs | Where-Object { $null -ne $_ })

    # 1. P/0 first (highest precedence), from survey-ref PRs only.
    $p0Prs = @($TargetPRs | Where-Object { Test-IsP0Pr $_ })
    $p0PrNumbers = @($p0Prs | ForEach-Object { $_.number })

    # 2. Dependency-flow (non-P/0), TARGET branch only. Once a preview is branched,
    #    this tracker must only surface dependency-flow bumps against its OWN branch
    #    (the survey ref). Detection is NOT author-only: it covers both automated
    #    darc PRs (author dotnet-maestro) AND human-authored component-bump PRs
    #    (e.g. a "[release/...] Bump dotnet/dotnet (BAR ...)" PR on head `update-<id>`
    #    that lands the blessed component set) via Test-IsDependencyFlowPr. Bumps
    #    targeting net<major>.0 (the inflight branch) belong to the inflight branch's
    #    own readiness, not this preview tracker — so they are intentionally NOT
    #    reported here. (In candidate mode the survey ref IS net<major>.0 and
    #    $InflightPRs is empty, so target-only is correct there too.)
    $maestroPRs = @($TargetPRs | Where-Object { (Test-IsDependencyFlowPr $_) -and ($p0PrNumbers -notcontains $_.number) })

    # Non-P/0, non-dependency-flow humans, split by scope. Both slots keep a
    #    Raw stage so the two-hop merge-up filter below can strip hoisted merge-up
    #    PRs from target AND inflight. Detection uses Test-IsDependencyFlowPr so
    #    human-authored component bumps (not just author dotnet-maestro) are
    #    excluded from the human buckets and routed to the dependency-flow bucket.
    $targetHumanPRsRaw   = @($TargetPRs   | Where-Object { -not (Test-IsDependencyFlowPr $_) -and ($p0PrNumbers -notcontains $_.number) })
    $inflightHumanPRsRaw = @($InflightPRs | Where-Object { -not (Test-IsDependencyFlowPr $_) })

    # 3. Merge-up: non-P/0, non-Maestro PRs from BOTH hops of the daily-flow chain
    #    that feeds the survey ref. The preview lane chains main → net<N>.0 →
    #    previewN, so a stuck merge-up at EITHER hop starves the release of
    #    upstream fixes:
    #      - net<N>.0 → previewN  (base = survey ref)      — from target PRs
    #      - main → net<N>.0      (base = inflight branch)  — from inflight PRs
    #    Both are hoisted to high priority rather than the second hop being buried
    #    as generic inflight-queue noise (the #36085 scenario). In candidate mode
    #    the inflight set is empty, so only the single base=net<N>.0 hop is seen —
    #    no double counting across modes.
    #    MAUI convention:
    #      - head ref like `merge/main-to-net11.0` or `merge/preview4-to-net11.0`
    #      - title like "[automated] Merge branch 'main' => 'net11.0'"
    $allHumanPRs = @($targetHumanPRsRaw) + @($inflightHumanPRsRaw)
    $mergeUpPRs = @($allHumanPRs | Where-Object {
        ($_.headRefName -and $_.headRefName -match '^merge/.+-to-') -or
        ($_.title -and $_.title -match '^\[automated\] Merge branch')
    })
    $mergeUpPrNumbers = @($mergeUpPRs | ForEach-Object { $_.number })

    # 4. Generic-human (target) and inflight-human = the remainders, with merge-up
    #    PRs removed from BOTH so a hoisted merge-up is never also listed below.
    $targetHumanPRs   = @($targetHumanPRsRaw   | Where-Object { $mergeUpPrNumbers -notcontains $_.number })
    $inflightHumanPRs = @($inflightHumanPRsRaw | Where-Object { $mergeUpPrNumbers -notcontains $_.number })

    return [PSCustomObject]@{
        P0Prs            = $p0Prs
        MaestroPRs       = $maestroPRs
        MergeUpPRs       = $mergeUpPRs
        TargetHumanPRs   = $targetHumanPRs
        InflightHumanPRs = $inflightHumanPRs
    }
}

function New-Check {
    param(
        [string]$Area,
        [string]$Status,
        [string]$Details,
        [string]$NextAction
    )

    return [PSCustomObject]@{
        Area       = $Area
        Status     = $Status
        Details    = $Details
        NextAction = $NextAction
    }
}

function Get-OverallStatus {
    param([array]$Checks)

    $worst = "READY"
    foreach ($check in $Checks) {
        if ($StatusRank[$check.Status] -gt $StatusRank[$worst]) {
            $worst = $check.Status
        }
    }
    return $worst
}

function Format-MarkdownCell {
    param([string]$Value)
    if ($null -eq $Value) {
        return ""
    }
    # Escape `<`/`>` so user-controlled cell content (issue/PR titles) cannot
    # inject an HTML comment. A title like `<!-- release-readiness-hash: sha=... -->`
    # would otherwise render verbatim ABOVE the human-notes block, where the
    # workflow's hash-extraction (`sed '/begin/q' | grep ...`) would capture it as
    # the semantic hash — freezing the Preview tracker (which emits no hash of its
    # own) via OLD_HASH==NEW_HASH. Escaping also fixes legitimate titles such as
    # `List<T>` that GitHub markdown would otherwise swallow as an HTML tag. The
    # engine's own markers are emitted via AppendLine, not through this formatter,
    # so escaping cells never disturbs them.
    # Collapse embedded newlines first: a malformed upstream title can contain a
    # literal CR/LF (observed: ci-scan issue #35957), which would otherwise split
    # the markdown table row across physical lines and break the rendered table.
    # Escape each pipe AND double only the backslash run immediately preceding it:
    # a title may legally contain a literal `\|`, and escaping only the pipe would
    # yield `\\|` — which GFM renders as a literal `\` plus an ACTIVE column delimiter
    # (table breakout). Doubling the pipe-adjacent run makes `\|` -> `\\\|`, a literal
    # `\|`. Scoping the doubling to `(\\*)\|` (rather than every backslash) preserves a
    # title's other backslash escapes (e.g. `\[link\](url)` is not de-escaped into an
    # active link). No-pipe-adjacent-backslash titles are unaffected (`a | b` -> `a \| b`).
    $v = $Value -replace "[\r\n]+", " "
    $v = [regex]::Replace($v, '(\\*)\|', { param($m) ($m.Groups[1].Value * 2) + '\|' })
    return ($v -replace "<", "&lt;" -replace ">", "&gt;").Trim()
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

function Add-CheckTable {
    param(
        [System.Text.StringBuilder]$Builder,
        [array]$Checks
    )

    [void]$Builder.AppendLine("| Area | Status | Details | Next action |")
    [void]$Builder.AppendLine("|------|--------|---------|-------------|")
    foreach ($check in $Checks) {
        [void]$Builder.AppendLine("| $(Format-MarkdownCell $check.Area) | **$($check.Status)** | $(Format-MarkdownCell $check.Details) | $(Format-MarkdownCell $check.NextAction) |")
    }
    [void]$Builder.AppendLine("")
}

function Add-PRTable {
    param(
        [System.Text.StringBuilder]$Builder,
        [array]$PRs,
        [int]$MaxRows = 100
    )

    if ($PRs.Count -eq 0) {
        [void]$Builder.AppendLine("_None found._")
        [void]$Builder.AppendLine("")
        return
    }

    [void]$Builder.AppendLine("| PR | Title | Author | Base | State | Age | Next action |")
    [void]$Builder.AppendLine("|----|-------|--------|------|-------|-----|-------------|")
    $rows = @($PRs | Select-Object -First $MaxRows)
    foreach ($pr in $rows) {
        $action = Get-PRAction -PR $pr
        $author = Format-GitHubHandle -Login $pr.author.login
        [void]$Builder.AppendLine("| [#$($pr.number)]($($pr.url)) | $(Format-MarkdownCell $pr.title) | $author | ``$($pr.baseRefName)`` | **$($action.Status)** | $($action.Age)d | $(Format-MarkdownCell $action.Action) |")
    }
    if ($PRs.Count -gt $MaxRows) {
        [void]$Builder.AppendLine("")
        [void]$Builder.AppendLine("_Showing $MaxRows of $($PRs.Count) PRs._")
    }
    [void]$Builder.AppendLine("")
}

function Add-IssueTable {
    param(
        [System.Text.StringBuilder]$Builder,
        [array]$Issues
    )

    if ($Issues.Count -eq 0) {
        [void]$Builder.AppendLine("_None found._")
        [void]$Builder.AppendLine("")
        return
    }

    [void]$Builder.AppendLine("| Issue | Title | Labels | Milestone |")
    [void]$Builder.AppendLine("|-------|-------|--------|-----------|")
    foreach ($issue in $Issues) {
        $labels = (@($issue.labels | ForEach-Object { $_.name }) -join ", ")
        $milestone = if ($issue.milestone -and $issue.milestone.title) { $issue.milestone.title } else { "" }
        [void]$Builder.AppendLine("| [#$($issue.number)]($($issue.url)) | $(Format-MarkdownCell $issue.title) | $(Format-MarkdownCell $labels) | $(Format-MarkdownCell $milestone) |")
    }
    [void]$Builder.AppendLine("")
}

function Add-CiScanTable {
    <#
    .SYNOPSIS
        Renders open ci-scan issues with creation age. Fresh issues (<24h)
        are visually flagged with 🆕 so release captains can spot recent
        scanner activity at a glance. Sorted newest-first; capped at $MaxRows.
    #>
    param(
        [System.Text.StringBuilder]$Builder,
        [array]$Issues,
        [int]$MaxRows = 15
    )

    if ($Issues.Count -eq 0) {
        [void]$Builder.AppendLine("_No open ``ci-scan`` issues — scanner has not flagged recurring CI failures recently._")
        [void]$Builder.AppendLine("")
        return
    }

    [void]$Builder.AppendLine("| Issue | Title | Filed |")
    [void]$Builder.AppendLine("|-------|-------|-------|")
    $rows = $Issues | Select-Object -First $MaxRows
    foreach ($issue in $rows) {
        $marker = ""
        $ageDisplay = "—"
        if ($issue.PSObject.Properties['createdAt'] -and $issue.createdAt) {
            $createdUtc = ConvertTo-UtcDateTime -Value $issue.createdAt
            if ($createdUtc) {
                $hoursAgo = ((Get-Date).ToUniversalTime() - $createdUtc).TotalHours
                $ageDisplay = if ($hoursAgo -lt 24) {
                    "{0:N0}h ago" -f $hoursAgo
                } else {
                    "{0:N0}d ago" -f ($hoursAgo / 24)
                }
                if ($hoursAgo -lt 24) { $marker = "🆕 " }
            }
        }
        [void]$Builder.AppendLine("| $marker[#$($issue.number)]($($issue.url)) | $(Format-MarkdownCell $issue.title) | $ageDisplay |")
    }
    if ($Issues.Count -gt $MaxRows) {
        [void]$Builder.AppendLine("")
        [void]$Builder.AppendLine("_…and $($Issues.Count - $MaxRows) more. Full list: [open ci-scan issues](https://github.com/$Repository/issues?q=is%3Aopen+is%3Aissue+label%3Aci-scan+sort%3Acreated-desc)._")
    }
    [void]$Builder.AppendLine("")
}

# ===================================================================
# MAIN — gather checks
# ===================================================================

# Guard: skip the main driver when dot-sourced so tests can load the helper
# functions (e.g. Test-IsP0Pr) without invoking the full report flow, which
# requires git + gh + network. Mirrors Find-ReleaseReadinessTrackers.ps1.
# `InvocationName -eq '.'` alone reliably detects dot-sourcing; matching
# `$MyInvocation.Line` against a leading dot is avoided because that text can be
# the whole command line and would wrongly skip a later `&`/`-File` call.
if ($MyInvocation.InvocationName -eq '.') { return }

$checks = @()

# --- Target branch existence ---
$targetBranchExists = Test-BranchExists -BranchName $Branch
if ($Mode -eq 'candidate') {
    if ($targetBranchExists) {
        # Branch already exists; if Find-Trackers ran today it would have
        # classified this as in-flight. Inform the operator but don't fail.
        $checks += New-Check -Area "Target branch" -Status "WATCH" -Details "``$Branch`` already exists — preview was cut. Re-run Find-Trackers to switch this tracker to in-flight mode." -NextAction "Re-run Find-ReleaseReadinessTrackers and update the workflow input."
    } else {
        $checks += New-Check -Area "Target branch (candidate)" -Status "READY" -Details "``$Branch`` does not exist yet — surveying source ``$SurveyRef`` (candidate mode)." -NextAction "Cut ``$Branch`` from ``$SurveyRef`` when ready."
    }
} else {
    if ($targetBranchExists) {
        $checks += New-Check -Area "Target branch" -Status "READY" -Details "``$Branch`` exists." -NextAction "Continue release-readiness checks."
    } else {
        $checks += New-Check -Area "Target branch" -Status "BLOCKED" -Details "``$Branch`` does not exist." -NextAction "Create or select the correct release branch before declaring readiness."
    }
}

# --- Iteration check ---
# In-flight: surveyRef == Branch, so we check that the branch itself declares
# PreReleaseVersionIteration == previewNumber.
# Candidate: surveyRef == net<major>.0, so we check that the source branch
# is bumped to match THIS preview (the one about to be cut).
$surveyIteration = $null
$xcodeRequirements = [PSCustomObject]@{ RequiredXcode = $null; DeviceTestsRequiredXcode = $null }
$surveyExists = if ($SurveyRef -eq $Branch) { $targetBranchExists } else { Test-BranchExists -BranchName $SurveyRef }
if ($surveyExists) {
    try {
        $surveyIteration = Get-PreReleaseVersionIteration -BranchName $SurveyRef
        $iterArea = if ($Mode -eq 'candidate') { "$SurveyRef preview iteration (candidate source)" } else { "Preview iteration" }
        if ($surveyIteration -eq [string]$previewNumber) {
            $checks += New-Check -Area $iterArea -Status "READY" -Details "``$SurveyRef`` has PreReleaseVersionIteration=$surveyIteration." -NextAction "No version-iteration action needed."
        } else {
            $displayValue = if ($surveyIteration) { $surveyIteration } else { "<empty>" }
            $checks += New-Check -Area $iterArea -Status "BLOCKED" -Details "``$SurveyRef`` has PreReleaseVersionIteration=$displayValue; expected $previewNumber." -NextAction "Bump ``$SurveyRef`` to match the preview number before cutting."
        }
    } catch {
        $checks += New-Check -Area "Preview iteration" -Status "UNKNOWN" -Details "Could not read version iteration from ``$SurveyRef``." -NextAction "Run locally and inspect eng/Versions.props."
    }

    try {
        $xcodeRequirements = Get-XcodeRequirements -BranchName $SurveyRef
    } catch {
        $checks += New-Check -Area "Xcode variables" -Status "UNKNOWN" -Details "Could not read required Xcode variables from ``$SurveyRef``." -NextAction "Inspect eng/pipelines/common/variables.yml on ``$SurveyRef``."
    }

    # --- Bug template version listing check ---
    # Releasing a preview that's not in .github/ISSUE_TEMPLATE/bug-report.yml's
    # `version-with-bug` dropdown means users can't file targeted bug reports
    # against it. Read the template from main (issue templates are global per repo)
    # and verify the dropdown contains an entry matching this preview.
    try {
        $expectedVersion = "$majorVersion.0.0-preview.$previewNumber"
        $templateBranch = if ($mainBranch) { $mainBranch } else { 'main' }
        $templateVersions = Get-BugTemplateVersions -BranchName $templateBranch
        if ($templateVersions.Count -eq 0) {
            $checks += New-Check -Area "Bug template versions" -Status "UNKNOWN" -Details "Could not read .github/ISSUE_TEMPLATE/bug-report.yml from ``$templateBranch`` or its version-with-bug dropdown is empty." -NextAction "Inspect the bug template manually."
        } elseif ($templateVersions -contains $expectedVersion) {
            $checks += New-Check -Area "Bug template versions" -Status "READY" -Details "``$expectedVersion`` listed in bug-report.yml on ``$templateBranch``." -NextAction "No action needed."
        } else {
            $sample = ($templateVersions | Select-Object -First 3) -join ', '
            $checks += New-Check -Area "Bug template versions" -Status "CLEANUP" -Details "``$expectedVersion`` NOT in .github/ISSUE_TEMPLATE/bug-report.yml version-with-bug dropdown on ``$templateBranch``. Top entries: $sample." -NextAction "Add ``$expectedVersion`` to the dropdown (PR against ``$templateBranch``). Not release-blocking — this is post-release cleanup so users can file bugs against the right version."
        }
    } catch {
        $checks += New-Check -Area "Bug template versions" -Status "UNKNOWN" -Details "Failed to evaluate bug template: $($_.Exception.Message)" -NextAction "Inspect .github/ISSUE_TEMPLATE/bug-report.yml manually."
    }
}

# --- Preview milestone existence check ---
# Policy: a preview release-readiness tracker and its GitHub milestone are
# coupled. If this tracker exists, the ".NET <major>.0-preview<N>" milestone
# must exist RIGHT AWAY — not deferred to cut time — otherwise fixed issues have
# no milestone to land on and the release-notes generator has nothing to query.
# Missing => BLOCKED (a real ship-readiness gap), mirroring the SR lane's
# current-cycle rule in Get-ReleaseReadiness.ps1, which likewise treats a
# candidate tracker's OWN milestone as blocking. Repo-global, so it runs
# independent of branch/survey state (candidate and in-flight alike).
try {
    $msCheck = Test-PreviewMilestoneExists -Major $majorVersion -Preview $previewNumber
    $msArea = "Milestone for preview$previewNumber ($($msCheck.ExpectedTitle))"
    if ($msCheck.QueryFailed) {
        $checks += New-Check -Area $msArea -Status "UNKNOWN" `
            -Details "Could not query milestones from the GitHub API for ``$Repository`` (gh exited non-zero after retries). Treating as unknown so a gh outage does not silently pass — or falsely block — the milestone check." `
            -NextAction "Verify ``gh auth status`` and rerun, or check manually: ``gh api repos/$Repository/milestones``."
    } elseif ($msCheck.Exists) {
        $checks += New-Check -Area $msArea -Status "READY" `
            -Details "Milestone ``$($msCheck.MatchedTitle)`` exists — fixed preview$previewNumber issues have a milestone to land on." `
            -NextAction "No action needed."
    } else {
        $checks += New-Check -Area $msArea -Status "BLOCKED" `
            -Details "No milestone matching ``$($msCheck.ExpectedTitle)`` exists in ``$Repository``. A preview$previewNumber release-readiness tracker exists, so its milestone must exist right away — without it, fixed issues have nowhere to land and the release-notes generator has nothing to query." `
            -NextAction "Create it now: ``gh api repos/$Repository/milestones -f title=""$($msCheck.ExpectedTitle)"" -f state=open``"
    }
} catch {
    $checks += New-Check -Area "Milestone for preview$previewNumber" -Status "UNKNOWN" `
        -Details "Failed to evaluate the preview milestone: $($_.Exception.Message)" `
        -NextAction "Check milestones manually: ``gh api repos/$Repository/milestones``."
}

# --- Inflight branch (net<major>.0) bump check ---
# In-flight mode: surveyRef == Branch, so net<major>.0 should be on N+1 (next preview).
# Candidate mode: surveyRef == net<major>.0 already (and we just checked it
# declares iteration N above), so net<major>.0 IS the source for this preview
# and the bump-to-N+1 conversation comes AFTER this preview ships.
$inflightIteration = $null
$inflightExists = Test-BranchExists -BranchName $mainBranch
if ($Mode -eq 'in-flight') {
    if ($inflightExists) {
        try {
            $inflightIteration = Get-PreReleaseVersionIteration -BranchName $mainBranch
            $displayValue = if ($inflightIteration) { $inflightIteration } else { "<empty>" }
            if ($inflightIteration -and ([int]$inflightIteration -le $previewNumber)) {
                $checks += New-Check -Area "$mainBranch preview-next bump" -Status "BLOCKED" -Details "``$mainBranch`` PreReleaseVersionIteration is $displayValue; target preview is $previewNumber." -NextAction "Confirm ``$mainBranch`` is bumped for preview-next."
            } else {
                $checks += New-Check -Area "$mainBranch preview-next bump" -Status "WATCH" -Details "``$mainBranch`` PreReleaseVersionIteration is $displayValue." -NextAction "Confirm this is correct for the next preview train."
            }
        } catch {
            $checks += New-Check -Area "$mainBranch preview-next bump" -Status "UNKNOWN" -Details "Could not read $mainBranch PreReleaseVersionIteration." -NextAction "Run locally and inspect eng/Versions.props on $mainBranch."
        }
    } else {
        $checks += New-Check -Area "$mainBranch branch" -Status "UNKNOWN" -Details "``$mainBranch`` branch was not found." -NextAction "Confirm branch state before release."
    }
}

# --- Open PRs ---
# "Target PRs" = PRs against the survey ref (the branch we're actually
# reporting readiness on; same as $Branch in in-flight mode, $mainBranch
# in candidate mode).
# "Inflight PRs" = PRs against net<major>.0, ONLY surfaced when
# surveyRef != mainBranch (otherwise these are the same set).
$targetPRs = @()
$inflightPRs = @()
if ($surveyExists) {
    $targetPRs = Get-OpenPullRequests -BaseBranch $SurveyRef
}
if ($SurveyRef -ne $mainBranch -and $inflightExists) {
    $inflightPRs = Get-OpenPullRequests -BaseBranch $mainBranch
}

# Categorize PRs into mutually-exclusive blocker buckets with P/0 as the highest
# precedence (a p/0-labelled Maestro or merge-up PR escalates to the P/0 category
# rather than being downgraded to a 📦 Maestro / merge-up row). The carve-out
# precedence logic lives in Get-CategorizedPullRequests so the unit tests drive
# the same code the engine runs (see Test-ReleaseReadiness.ps1 precedence block).
$prBuckets        = Get-CategorizedPullRequests -TargetPRs $targetPRs -InflightPRs $inflightPRs
$p0Prs            = $prBuckets.P0Prs
$maestroPRs       = $prBuckets.MaestroPRs
$mergeUpPRs       = $prBuckets.MergeUpPRs
$targetHumanPRs   = $prBuckets.TargetHumanPRs
$inflightHumanPRs = $prBuckets.InflightHumanPRs

if ($maestroPRs.Count -eq 0) {
    # Maestro is scoped to the survey ref (target) only. When the preview is
    # branched ($SurveyRef differs from the inflight $mainBranch), don't claim
    # anything about net<major>.0 Maestro PRs — they exist but are intentionally
    # tracked by the inflight branch's own readiness, not this preview tracker.
    $maestroReadyDetails = if ($SurveyRef -ne $mainBranch) {
        "No open Maestro PRs target ``$SurveyRef``. (Dependency-flow PRs targeting the inflight ``$mainBranch`` branch are tracked by ``$mainBranch``'s own readiness, not this branched preview.)"
    } else {
        "No open Maestro PRs target ``$SurveyRef``."
    }
    $checks += New-Check -Area "Maestro PRs" -Status "READY" -Details $maestroReadyDetails -NextAction "Continue monitoring for new dependency-flow PRs."
} elseif (@($maestroPRs | Where-Object { (Get-PRAction -PR $_).Status -eq "BLOCKED" }).Count -gt 0) {
    $checks += New-Check -Area "Maestro PRs" -Status "BLOCKED" -Details "$($maestroPRs.Count) open dependency-flow PR(s) (darc + manual component bumps), including blocked/conflicted PRs." -NextAction "Resolve blocked dependency-flow PRs before release."
} else {
    $checks += New-Check -Area "Maestro PRs" -Status "WATCH" -Details "$($maestroPRs.Count) open dependency-flow PR(s) (darc + manual component bumps) need review/merge triage." -NextAction "Review dependency PRs and merge expected updates."
}

if ($targetHumanPRs.Count -eq 0) {
    $checks += New-Check -Area "Release branch PRs" -Status "READY" -Details "No non-Maestro open PRs target ``$SurveyRef``." -NextAction "No direct release-branch PR action from this check."
} else {
    # Generic open PRs are NOT release blockers — only P/0 issues block the
    # release (and those have a dedicated check above + hoisted section).
    # PRs with merge conflicts or do-not-merge labels are normal queue
    # noise: the captain decides per-PR if any specific one MUST merge.
    $blockedCount = @($targetHumanPRs | Where-Object { (Get-PRAction -PR $_).Status -eq "BLOCKED" }).Count
    $blockedNote = if ($blockedCount -gt 0) { " ($blockedCount with merge conflicts / do-not-merge label)" } else { "" }
    $checks += New-Check -Area "Release branch PRs" -Status "WATCH" -Details "$($targetHumanPRs.Count) non-Maestro PR(s) target ``$SurveyRef``$blockedNote. Not auto-blocking — only P/0 issues and P/0-labelled PRs block shipment." -NextAction "Confirm which PRs (if any) must merge for the release; the rest can ride normal queue cadence."
}

# P/0-labelled PRs targeting the release branch are blockers (parallel to P/0
# issues). They are itemized in the hoisted "🔴 High-priority items" section.
# By design this is label-only and does NOT filter drafts: a `p/0` label
# deliberately placed on a release-targeting PR is an explicit "must ship"
# signal regardless of draft state, so a draft p/0 PR intentionally trips
# BLOCKED here. Surfacing "a release-critical change isn't ready yet" is the
# useful behavior; the per-row 🔥 entry still shows "Draft PR; wait until
# ready" via Get-PRAction, so the draft state is not lost.
if ($p0Prs.Count -gt 0) {
    $checks += New-Check -Area "P/0 release-branch PRs" -Status "BLOCKED" -Details "$($p0Prs.Count) open P/0-labelled PR(s) target ``$SurveyRef``. See 🔴 High-priority items at top." -NextAction "Land or de-prioritize each P/0 PR before shipping."
} else {
    $checks += New-Check -Area "P/0 release-branch PRs" -Status "READY" -Details "No open P/0-labelled PRs target ``$SurveyRef``." -NextAction "No action required."
}

# Inflight watch only matters when survey != inflight (otherwise it
# duplicates the target check).
if ($SurveyRef -ne $mainBranch) {
    if ($inflightHumanPRs.Count -eq 0) {
        $checks += New-Check -Area "$mainBranch inflight branch health" -Status "READY" -Details "No non-Maestro inflight PRs are open on ``$mainBranch``." -NextAction "Continue monitoring inflight branch health."
    } elseif (@($inflightHumanPRs | Where-Object { (Get-PRAction -PR $_).Status -eq "BLOCKED" }).Count -gt 0) {
        $checks += New-Check -Area "$mainBranch inflight branch health" -Status "WATCH" -Details "$($inflightHumanPRs.Count) non-Maestro PR(s) are open on ``$mainBranch``, including blocked PRs." -NextAction "Track as preview-next/inflight work; do not treat every inflight PR as a direct blocker for this release branch."
    } else {
        $checks += New-Check -Area "$mainBranch inflight branch health" -Status "WATCH" -Details "$($inflightHumanPRs.Count) non-Maestro PR(s) are open on ``$mainBranch``." -NextAction "Review inflight queue for preview-next readiness."
    }
}

# --- Release-relevant issues ---
$priorityIssues = Get-ReleaseRelevantIssuesByLabel -Labels @("p/0", "p/1") -Major $majorVersion -Preview $previewNumber
$kbeIssues = Get-ReleaseRelevantIssuesByLabel -Labels @("Known Build Error") -Major $majorVersion -Preview $previewNumber

# Carve out P/0 issues separately — these are surfaced in the hoisted
# "🔴 High-priority items" section at the top of the report so the release
# captain sees them before any other content. P/1 issues still flow through
# the regular "Priority blockers" check below.
$p0Issues = @($priorityIssues | Where-Object {
    @($_.labels | ForEach-Object { $_.name }) -contains 'p/0'
})

if ($p0Issues.Count -gt 0) {
    $checks += New-Check -Area "P/0 priority blockers" -Status "BLOCKED" -Details "$($p0Issues.Count) open P/0 issue(s) look release-relevant. See 🔴 High-priority items at top." -NextAction "Resolve or downgrade each P/0 before shipping."
} else {
    $checks += New-Check -Area "P/0 priority blockers" -Status "READY" -Details "No open release-relevant P/0 issues found." -NextAction "Confirm with release owners."
}

$p1Issues = @($priorityIssues | Where-Object {
    -not (@($_.labels | ForEach-Object { $_.name }) -contains 'p/0')
})
if ($p1Issues.Count -gt 0) {
    $checks += New-Check -Area "P/1 priority blockers" -Status "WATCH" -Details "$($p1Issues.Count) open P/1 issue(s) look release-relevant." -NextAction "Triage whether each blocks this release target."
} else {
    $checks += New-Check -Area "P/1 priority blockers" -Status "READY" -Details "No open release-relevant P/1 issues found by public search." -NextAction "No action required."
}

if ($mergeUpPRs.Count -gt 0) {
    $checks += New-Check -Area "Merge-up PRs ($mergeUpChainLabel)" -Status "BLOCKED" -Details "$($mergeUpPRs.Count) open merge-up PR(s) in the ``$mergeUpChainLabel`` daily-flow chain. See 🔴 High-priority items at top. A stuck merge-up at any hop accumulates conflicts and starves the release of upstream fixes." -NextAction "Resolve and merge each before shipping."
} else {
    $checks += New-Check -Area "Merge-up PRs ($mergeUpChainLabel)" -Status "READY" -Details "No open merge-up PRs in the ``$mergeUpChainLabel`` daily-flow chain." -NextAction "Continue monitoring."
}

if ($kbeIssues.Count -gt 0) {
    $checks += New-Check -Area "Known Build Errors" -Status "WATCH" -Details "$($kbeIssues.Count) open release-relevant KBE issue(s) found." -NextAction "Use #35052 CI truth to decide accepted-known vs release-blocking."
} else {
    $checks += New-Check -Area "Known Build Errors" -Status "READY" -Details "No release-relevant open KBE issues found by public search." -NextAction "Continue monitoring."
}

# --- ci-scan signals (auto-filed by CI Failure Scanner every 12h) ---
# Filtered to issues whose body marker `**Branch**: <name>` matches the
# survey ref — repo-wide scanner signals from other branches (e.g. main
# failures when we're surveying net11.0) are excluded as not relevant.
# Fresh issues (created in last 24h) escalate to WATCH so release captains
# notice that the scanner just found something on this branch.
# Branch-scoped (was: dedup-and-filter; now: one label lookup via
# Get-CiScanLabelForBranch). For in-flight previews the parent net<N>.0
# scanner is queried; for SR-style refs there is no scanner and we surface
# that fact explicitly instead of a misleading "no signals". gh failures
# escalate to WATCH so a missing query doesn't silently READY the verdict.
$ciScanResult = Get-CiScanIssues -Branch $SurveyRef
$ciScanIssues = @($ciScanResult.Matched)
$ciScanFilteredOut = $ciScanResult.FilteredOut
$ciScanQueryFailed = [bool]$ciScanResult.QueryFailed
$ciScanLabel = $ciScanResult.ScannerLabel
$freshCiScan = @($ciScanIssues | Where-Object { Test-IssueIsFresh -Issue $_ -HoursThreshold 24 })

if ($ciScanQueryFailed) {
    $checks += New-Check -Area "CI Failure Scanner signals" -Status "WATCH" `
        -Details "Could not query ci-scan issues (label ``$ciScanLabel`` — gh exited non-zero after retries). Treating as missing signal so the verdict reflects unknown state." `
        -NextAction "Verify ``gh auth status`` and rerun. If gh is unavailable, triage ci-scan manually."
} elseif (-not $ciScanLabel) {
    $checks += New-Check -Area "CI Failure Scanner signals" -Status "READY" `
        -Details "No per-branch CI Failure Scanner is configured for ``$SurveyRef``. Add a case to Get-CiScanLabelForBranch if a scanner is added later." `
        -NextAction "No action — this branch is not continuously scanned."
} elseif ($freshCiScan.Count -gt 0) {
    $detail = "$($freshCiScan.Count) ci-scan issue(s) on ``$SurveyRef`` (label ``$ciScanLabel``) filed in the last 24h ($($ciScanIssues.Count) total open). Likely affects this release."
    $checks += New-Check -Area "CI Failure Scanner signals" -Status "WATCH" -Details $detail -NextAction "Review the freshest ci-scan issues; decide whether any affect ship-readiness."
} elseif ($ciScanIssues.Count -gt 0) {
    $checks += New-Check -Area "CI Failure Scanner signals" -Status "WATCH" -Details "$($ciScanIssues.Count) open ci-scan issue(s) on ``$SurveyRef`` (label ``$ciScanLabel``, none filed in the last 24h)." -NextAction "Review recent ci-scan issues for ship-impact patterns."
} else {
    $checks += New-Check -Area "CI Failure Scanner signals" -Status "READY" -Details "No open ci-scan issues on ``$SurveyRef`` (label ``$ciScanLabel``) — scanner has not flagged recurring CI failures." -NextAction "Continue monitoring."
}

# --- CI truth (placeholder; #35052 wiring not yet done) ---
$checks += New-Check -Area "CI truth" -Status "INSUFFICIENT_DATA" -Details "#35052 structured CI evidence is not wired into this script yet." -NextAction "Do not infer release readiness from GitHub checks alone; consume #35052 output when available."

# --- Xcode ICM ---
$requiredXcode = if ($xcodeRequirements.RequiredXcode) { $xcodeRequirements.RequiredXcode } else { "unknown" }
$deviceXcode = if ($xcodeRequirements.DeviceTestsRequiredXcode) { $xcodeRequirements.DeviceTestsRequiredXcode } else { "unknown" }
$checks += New-Check -Area "Xcode / ICM" -Status "UNKNOWN" -Details "REQUIRED_XCODE=$requiredXcode; DEVICETESTS_REQUIRED_XCODE=$deviceXcode." -NextAction "Verify hosted Mac pool support and file/update ICM immediately when public Xcode availability requires it."

# --- Internal release pipelines (sanitized) ---
$internalStatus = "UNKNOWN"
$internalDetails = "Internal dnceng pipeline details are not queried in public workflow mode."
$internalAction = "Run this script locally with internal access, then publish only sanitized status."

if ($IncludeInternal) {
    if ([string]::IsNullOrWhiteSpace($InternalBuildId)) {
        $internalStatus = "UNKNOWN"
        $internalDetails = "Internal validation requested, but no InternalBuildId was provided."
        $internalAction = "Run with -InternalBuildId <build-id> or extend the local adapter for the target internal pipeline."
    } elseif (Get-Command az -ErrorAction SilentlyContinue) {
        try {
            $azArgs = @(
                "pipelines", "build", "show",
                "--id", $InternalBuildId,
                "--org", "https://dev.azure.com/dnceng",
                "--project", "internal",
                "--query", "{status:status,result:result}",
                "-o", "json"
            )
            $azOutput = & az @azArgs 2>$null
            if ($LASTEXITCODE -eq 0 -and $azOutput) {
                $internal = $azOutput | ConvertFrom-Json
                if ($internal.status -eq "completed" -and $internal.result -eq "succeeded") {
                    $internalStatus = "READY"
                    $internalDetails = "Local internal validation found a completed/succeeded internal build."
                    $internalAction = "Keep detailed diagnostics internal; public issue may report READY."
                } elseif ($internal.result) {
                    $internalStatus = "BLOCKED"
                    $internalDetails = "Local internal validation found an internal build that did not succeed."
                    $internalAction = "Release owner should inspect internal pipeline details ASAP."
                } else {
                    $internalStatus = "WATCH"
                    $internalDetails = "Local internal validation found an internal build still in progress."
                    $internalAction = "Wait for completion or inspect internally if stale."
                }
            } else {
                $internalStatus = "UNKNOWN"
                $internalDetails = "Internal build query did not return usable status."
                $internalAction = "Inspect internal Azure DevOps directly."
            }
        } catch {
            $internalStatus = "UNKNOWN"
            $internalDetails = "Internal validation failed locally."
            $internalAction = "Inspect internal Azure DevOps directly; do not publish raw error details."
        }
    } else {
        $internalStatus = "UNKNOWN"
        $internalDetails = "Azure CLI is not available for local internal validation."
        $internalAction = "Install/configure Azure CLI or inspect internal Azure DevOps directly."
    }
}

if ($PublicSafe -and $internalStatus -ne "READY") {
    $internalDetails = "Internal release pipeline status is $internalStatus."
    $internalAction = "Release owner should inspect dnceng/internal pipeline details ASAP."
}

$checks += New-Check -Area "Internal release pipelines" -Status $internalStatus -Details $internalDetails -NextAction $internalAction

$overallStatus = Get-OverallStatus -Checks $checks

# ===================================================================
# REPORT ASSEMBLY
# ===================================================================
$generatedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
$report = [PSCustomObject]@{
    GeneratedAt           = $generatedAt
    Repository            = $Repository
    Branch                = $Branch
    Mode                  = $Mode
    SurveyRef             = $SurveyRef
    BranchType            = "preview"
    MajorVersion          = $majorVersion
    PreviewNumber         = $previewNumber
    InflightBranch        = $mainBranch
    TrackerKey            = $TrackerKey
    OverallStatus         = $overallStatus
    Checks                = $checks
    XcodeRequirements     = $xcodeRequirements
    MaestroPullRequests   = $maestroPRs
    ReleasePullRequests   = $targetHumanPRs
    P0PullRequests        = $p0Prs
    MergeUpPullRequests   = $mergeUpPRs
    InflightPullRequests  = $inflightHumanPRs
    PriorityIssues        = $priorityIssues
    KnownBuildErrorIssues = $kbeIssues
    CiScanIssues          = $ciScanIssues
    NightlyFeed           = $null
}

# Nightly dogfood feed freshness (preview lane). Tracks the inflight/current dogfood stream
# (ci.inflight builds) on the dotnet<major> feed; falls back to this preview's preview.N
# version band when the feed has no inflight builds yet (the common case while a major is
# still in preview — its newest bits ARE the preview.N builds). Fail-open: any gap (helper
# unloaded, version unreadable, network error) degrades to "no banner".
$nightlyFeedBanner = $null
if ($Script:NightlyFeedHelperLoaded -and
    (Get-Command Resolve-NightlyDogfoodFreshness -ErrorAction SilentlyContinue) -and
    (Get-Command Format-NightlyFeedBanner -ErrorAction SilentlyContinue)) {
    try {
        $nfFeed = "dotnet$majorVersion"
        $nfFeedUrl = "https://dev.azure.com/dnceng/public/_artifacts/feed/$nfFeed"
        $nfIteration = Get-PreReleaseVersionIteration -BranchName $SurveyRef
        if ([string]::IsNullOrWhiteSpace($nfIteration)) { $nfIteration = "$previewNumber" }
        $nfBand = "$majorVersion.0.0-preview.$nfIteration"
        $nfBandPrefix = '^' + [regex]::Escape("$nfBand.")

        $nfFresh = Resolve-NightlyDogfoodFreshness -Feed $nfFeed -BandPrefixRegex $nfBandPrefix
        if ($null -eq $nfFresh) { $nfFresh = @{ unknown = $true } }

        $nfBuildType = [string](Get-NightlyFeedProp $nfFresh 'buildType')
        $nfLaneLabel = Format-NightlyFeedLaneLabel -Feed $nfFeed -FeedUrl $nfFeedUrl -BuildType $nfBuildType -BandNote "``$nfBand`` (preview.$nfIteration)"
        $nfFresh['laneLabel'] = $nfLaneLabel
        $nfFresh['feedUrl'] = $nfFeedUrl
        $nfFresh['versionPrefix'] = $nfBandPrefix

        $report.NightlyFeed = $nfFresh
        $nightlyFeedBanner = Format-NightlyFeedBanner -Freshness $nfFresh -Now ([DateTime]::UtcNow)
    } catch {
        # -WarningAction Continue: keep this fail-open even under an ambient
        # $WarningPreference='Stop', where a bare Write-Warning would be promoted to a
        # terminating error inside the catch and escape, crashing the unattended job.
        Write-Warning "Nightly-feed freshness check failed (non-fatal): $($_.Exception.Message)" -WarningAction Continue
    }
}

$md = [System.Text.StringBuilder]::new()
[void]$md.AppendLine("<!-- release-readiness-tracker: $TrackerKey -->")
[void]$md.AppendLine("<!-- release-readiness-flavor: preview -->")
[void]$md.AppendLine("<!-- release-readiness-mode: $Mode -->")
if ($Mode -eq 'candidate') {
    [void]$md.AppendLine("# Release Readiness — .NET $majorVersion.0 preview $previewNumber (CANDIDATE from $SurveyRef) — $((Get-Date).ToString("yyyy-MM-dd"))")
} else {
    [void]$md.AppendLine("# Release Readiness — .NET $majorVersion.0 preview $previewNumber — $((Get-Date).ToString("yyyy-MM-dd"))")
}
[void]$md.AppendLine("")
[void]$md.AppendLine("**Overall status:** **$overallStatus**")
[void]$md.AppendLine("")
if ($nightlyFeedBanner) {
    [void]$md.AppendLine($nightlyFeedBanner)
    [void]$md.AppendLine("")
}

# === HIGH-PRIORITY ITEMS (hoisted to the very top) ===
# Four categories the release captain must see BEFORE anything else:
#   1. P/0 priority blockers — open issues labeled p/0 (release-blocking severity).
#   2. P/0 release-branch PRs — open PRs labeled p/0 targeting the survey ref.
#      A p/0 PR is an explicitly release-blocking change that must land (or be
#      de-prioritized) before shipping.
#   3. Maestro dependency-flow PRs — open Maestro PRs against the survey ref.
#      A stuck Maestro PR blocks all upstream dependency flow into this branch.
#   4. Merge-up PRs (upstream → survey ref) — daily-flow sync PRs whose head ref
#      matches `merge/...-to-...` or title starts with "[automated] Merge branch".
#      A stuck merge-up PR accumulates conflicts and starves the release branch
#      of new fixes from upstream (main on the SR lane, net11.0 on the preview lane).
# Each item is itemized (one row per issue/PR) so the captain can see exactly
# what's outstanding without drilling into the per-category PR tables below.
$highPriorityRows = New-Object System.Collections.Generic.List[hashtable]
foreach ($iss in $p0Issues) {
    [void]$highPriorityRows.Add(@{
        kind = '🔥 P/0 issue'
        link = "[#$($iss.number)]($($iss.url))"
        title = $iss.title
        actor = if ($iss.milestone -and $iss.milestone.title) { $iss.milestone.title } else { '' }
        nextAction = 'Resolve or downgrade before shipping.'
    })
}
foreach ($pr in $p0Prs) {
    $action = Get-PRAction -PR $pr
    [void]$highPriorityRows.Add(@{
        kind = '🔥 P/0 PR'
        link = "[#$($pr.number)]($($pr.url))"
        title = $pr.title
        actor = "base ``$($pr.baseRefName)``, $($action.Age)d old"
        nextAction = $action.Action
    })
}
foreach ($pr in $maestroPRs) {
    $action = Get-PRAction -PR $pr
    # A dependency-flow PR that bumps the VMR/SDK (dotnet/dotnet or dotnet/sdk)
    # advances the branch's SDK pin — but the landed pin is only a *candidate*
    # until the blessed build is confirmed locally (the Action can't reach the
    # .NET Release Tracker). Flag it so the captain runs the local check.
    $isSdkBump = Test-IsSdkBumpPr $pr
    $paNote = if ($isSdkBump) { " ⚠️ bumps the SDK/VMR — confirm the blessed build locally (see 🏷️ component build) before treating the new pin as final." } else { "" }
    [void]$highPriorityRows.Add(@{
        kind = '📦 Dependency-flow PR'
        link = "[#$($pr.number)]($($pr.url))"
        title = $pr.title
        actor = "base ``$($pr.baseRefName)``, $($action.Age)d old"
        nextAction = "$($action.Action)$paNote"
    })
}
foreach ($pr in $mergeUpPRs) {
    $action = Get-PRAction -PR $pr
    # Name the specific hop from the PR's base: a survey-ref-based merge-up is the
    # net<N>.0 → previewN hop; an inflight-based one (base = net<N>.0) is the
    # main → net<N>.0 hop. In candidate mode the survey ref IS net<N>.0, so a
    # base=net<N>.0 PR is the main → net<N>.0 hop (guarded by the candidate check).
    $leg = if ($Mode -ne 'candidate' -and $pr.baseRefName -eq $SurveyRef) {
        "$mainBranch → $SurveyRef"
    } elseif ($pr.baseRefName -eq $mainBranch) {
        "main → $mainBranch"
    } else {
        "merge-up"
    }
    [void]$highPriorityRows.Add(@{
        kind = "🔀 Merge-up PR ($leg)"
        link = "[#$($pr.number)]($($pr.url))"
        title = $pr.title
        actor = "base ``$($pr.baseRefName)``, $($action.Age)d old"
        nextAction = $action.Action
    })
}

if ($highPriorityRows.Count -gt 0) {
    [void]$md.AppendLine("## 🔴 High-priority items — $($highPriorityRows.Count) item(s)")
    [void]$md.AppendLine("")
    [void]$md.AppendLine("_P/0 issues, P/0 PRs, dependency-flow PRs (darc + manual component bumps), and merge-up PRs in the ``$mergeUpChainLabel`` daily-flow chain. Resolve these before treating the release as ready._")
    [void]$md.AppendLine("")
    [void]$md.AppendLine("| Kind | Item | Title | Context | Next action |")
    [void]$md.AppendLine("|------|------|-------|---------|-------------|")
    foreach ($row in $highPriorityRows) {
        [void]$md.AppendLine("| $(Format-MarkdownCell $row.kind) | $($row.link) | $(Format-MarkdownCell $row.title) | $(Format-MarkdownCell $row.actor) | $(Format-MarkdownCell $row.nextAction) |")
    }
    [void]$md.AppendLine("")
}

# Human-editable section, preserved across re-runs by workflow body merge.
# Positioned directly under the "🔴 High-priority items" section so the
# agent-orchestrated, release-critical content that gets spliced in here (the
# official/blessed build, subscription wiring, component-pin coherence) renders
# near the TOP of the tracker — right below the blockers — while still living
# inside the preserved human-notes markers so it survives automated re-runs.
#
# Built as a reusable block (like the SR engine) so the body-size cap below can
# strip it, truncate the remaining content, then re-append it — guaranteeing the
# begin/end markers always survive truncation regardless of section order. The
# "🔴 High-priority items" section above is itemized and uncapped, so a naive
# byte-prefix cut could otherwise drop these markers.
$notesSb = [System.Text.StringBuilder]::new()
[void]$notesSb.AppendLine("<!-- release-readiness:human-notes:begin -->")
[void]$notesSb.AppendLine("## Release Captain Notes")
[void]$notesSb.AppendLine("")
[void]$notesSb.AppendLine("_Add manual notes here. Anything between these begin/end markers is preserved across automated re-runs._")
[void]$notesSb.AppendLine("<!-- release-readiness:human-notes:end -->")
$notesBlockText = $notesSb.ToString()
[void]$md.Append($notesBlockText)
[void]$md.AppendLine("")

# === Action-owned component build (git-pin sourced) — blessed & subs are LOCAL-only ===
# The GitHub Action runs with only contents:read + GITHUB_TOKEN. It CANNOT reach:
#   * the internal .NET Release Tracker (which names the authoritative "blessed"
#     preview build), nor
#   * Maestro/BAR or the AzDO-internal maestro-configuration repo (which hold the
#     android/macios/dotnet preview *subscription* wiring).
# So the Action reports only what PUBLIC git can prove: the component builds
# CURRENTLY BUNDLED on the branch (eng/Version.Details.xml). These are branch pins,
# NOT a confirmed blessed build. Confirming the blessed build AND verifying the
# preview subscriptions are both LOCAL tasks — the callout below points the captain
# at the exact local prompt to run. Rendered OUTSIDE the human-notes markers so it
# self-refreshes on every automated re-run.
$componentPins = Get-BranchComponentPins -Ref $SurveyRef -Major $majorVersion
if ($componentPins) {
    # --- Inferred subscription health (public PR trail) ---
    # We can't read Maestro subscription config from CI, but a *working* sub
    # leaves a public trail of dependency-flow PRs into the branch. Gather that
    # trail (open $targetPRs + recently merged) so each component row can show
    # whether flow is alive, silently stalled, or apparently never wired.
    $flowStaleDays   = 14
    $flowNow         = [DateTime]::UtcNow
    # Best-effort: the flow signal is inferred from the public dep-flow PR trail.
    # Get-MergedPullRequests re-throws non-retryable gh failures (e.g. a 403
    # secondary-rate-limit or a 500), so a transient outage here must degrade the
    # signal to open-PR-only inference, NEVER abort the whole readiness report
    # (every other data section in this driver degrades rather than throws).
    $mergedPRsForFlow = @()
    $mergedHistoryUnavailable = $false
    try {
        $mergedPRsForFlow = @(Get-MergedPullRequests -BaseBranch $SurveyRef)
    } catch {
        $mergedHistoryUnavailable = $true
        Write-Warning "Flow-signal merged-PR fetch failed (non-fatal): $($_.Exception.Message)" -WarningAction Continue
    }
    $allPRsForFlow   = @($targetPRs) + @($mergedPRsForFlow)
    $depFlowPRs      = @($allPRsForFlow | Where-Object { Test-IsDependencyFlowPr $_ })

    [void]$md.AppendLine("## 🏷️ Preview $previewNumber component build — branch pins + inferred sub health")
    [void]$md.AppendLine("")
    [void]$md.AppendLine("> [!IMPORTANT]")
    [void]$md.AppendLine("> **Branch pins** below are the component builds **currently bundled** on ``$SurveyRef`` (``eng/Version.Details.xml`` — public git). **This is NOT the confirmed official/blessed build** — that designation lives in the internal **.NET Release Tracker**, which this Action can't reach (``contents:read`` + ``GITHUB_TOKEN`` only). The **Flow signal** column *infers* each component's subscription health from the **public dependency-flow PR trail** (the Action can't read Maestro subscription config directly). It surfaces both a **never-wired** sub (❌ none seen) and a **silently stalled** one (⚠️ stale — the `"we stopped getting Maestro PRs a month ago`" decay case). The blessed build AND the authoritative subscription wiring are both confirmed **locally** — run the release-readiness skill:")
    [void]$md.AppendLine(">")
    [void]$md.AppendLine("> The **Upstream** column is a hard git fact (not an inference): it compares our pinned SHA against the tip of each component's same-named branch (``$SurveyRef`` — derived, never hardcoded) via the public compare API. ⬆️ *N ahead* means the source moved past what we bundle — an FYI you *may* want to pull in, **not** a required bump (maui pins blessed builds deliberately, so those commits may be post-preview churn or not yet blessed).")
    [void]$md.AppendLine(">")
    [void]$md.AppendLine("> ``````")
    [void]$md.AppendLine("> Run release readiness for ${Branch}: report the blessed/official preview build (SDK + runtime) from the .NET Release Tracker, and verify the dotnet/android + dotnet/macios preview subscriptions are wired to the .NET $majorVersion.0.1xx SDK Preview $previewNumber channel.")
    [void]$md.AppendLine("> ``````")
    [void]$md.AppendLine("")
    [void]$md.AppendLine("| Component | Branch pin (bundled) | Commit | Flow signal (inferred sub health) | Upstream (vs our pin) |")
    [void]$md.AppendLine("|-----------|----------------------|--------|-----------------------------------|-----------------------|")

    $pinRows = @(
        @{ Label = 'dotnet/dotnet (VMR / SDK)'; Repo = 'dotnet/dotnet'; Pin = $componentPins.Vmr;     Noisy = $true  }
        @{ Label = 'dotnet/android';            Repo = 'dotnet/android'; Pin = $componentPins.Android; Noisy = $false }
        @{ Label = 'dotnet/macios';             Repo = 'dotnet/macios';  Pin = $componentPins.Macios;  Noisy = $false }
    )
    foreach ($r in $pinRows) {
        $flow = Get-ComponentFlowSignal -Repo $r.Repo -DepFlowPRs $depFlowPRs -Now $flowNow -StaleDays $flowStaleDays
        $flowCell = Format-FlowSignalCell -Flow $flow -HistoryUnavailable:$mergedHistoryUnavailable
        $pin = $r.Pin
        if (-not $pin) {
            [void]$md.AppendLine("| $(Format-MarkdownCell $r.Label) | _not pinned_ | — | $flowCell | — |")
            continue
        }

        # Upstream drift (public compare API) — has the component's same-named
        # branch advanced past the SHA we pin? Needs the pin SHA; soft-fails to '—'.
        $driftCell = '—'
        if (-not [string]::IsNullOrWhiteSpace($pin.Sha)) {
            $drift = Get-UpstreamDriftSignal -Repo $r.Repo -Sha $pin.Sha -BranchName $SurveyRef
            $driftCell = switch ($drift.Status) {
                'current'  { '✅ current' }
                'ahead'    {
                    $n = $drift.AheadBy
                    $unit = if ($n -eq 1) { 'commit' } else { 'commits' }
                    if ($r.Noisy) { "⬆️ [$n $unit ahead]($($drift.Url)) — VMR churn (expected)" }
                    else          { "⬆️ [$n $unit ahead]($($drift.Url)) — FYI" }
                }
                'diverged' { "⚠️ [diverged]($($drift.Url)) — compare manually" }
                default    { if ($drift.Reason) { "— _$($drift.Reason)_" } else { '—' } }
            }
        }

        $commitCell = '—'
        if (-not [string]::IsNullOrWhiteSpace($pin.Sha)) {
            $shortSha = $pin.Sha.Substring(0, [Math]::Min(7, $pin.Sha.Length))
            $commitCell = "[``$shortSha``](https://github.com/$($r.Repo)/commit/$($pin.Sha))"
        }
        [void]$md.AppendLine("| $(Format-MarkdownCell $r.Label) | ``$($pin.Version)`` | $commitCell | $flowCell | $driftCell |")
    }
    [void]$md.AppendLine("")
    [void]$md.AppendLine("_Flow-signal legend: 🔄 open dep-flow PR (flowing now) · ✅ merged ≤ $flowStaleDays d · ⚠️ newest merge > $flowStaleDays d (sub may have stalled) · ❌ no dep-flow PR into ``$SurveyRef`` seen (sub may be missing — or the branch is brand new). Inferred from the **public** PR trail; confirm authoritative wiring locally with ``darc get-subscriptions --target-repo https://github.com/dotnet/maui --target-branch $SurveyRef``. dotnet/dotnet (VMR) typically flows on manual trigger, so a quiet VMR is less alarming than a quiet android/macios._")
    [void]$md.AppendLine("")
    [void]$md.AppendLine("_Upstream legend: ✅ current = our pin **is** the tip of the component's ``$SurveyRef`` branch · ⬆️ N ahead = that branch has N newer commit(s) than we bundle (**FYI** — may be post-preview churn or not-yet-blessed work; not necessarily a bump you need) · ⚠️ diverged = our pin isn't a clean ancestor of the branch tip (build tag / different line) — compare manually · — = couldn't determine (no same-named upstream branch, or the compare API was unavailable). Branch is **derived** from the survey ref, never hardcoded. dotnet/dotnet (VMR) advances constantly on manual trigger, so 'N ahead' there is expected churn and rarely actionable._")

    # If a manual/darc component-bump PR is open against the branch, it names the
    # pending target BAR builds in its title — surface it so readers see the pins
    # above will advance once it merges. Reuses the already-computed dependency-flow set.
    $bumpPr = @($maestroPRs | Where-Object { $_.title -match 'Bump\s+dotnet/dotnet' }) | Select-Object -First 1
    if ($bumpPr) {
        $bars = @([regex]::Matches($bumpPr.title, 'BAR\s+(\d+)') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique)
        $barText = if ($bars.Count -gt 0) { " (target BAR $([string]::Join('/', $bars)))" } else { "" }
        [void]$md.AppendLine("⏳ **Pending component bump:** [#$($bumpPr.number)]($($bumpPr.url))$barText will advance these branch pins — see 🔴 High-priority items above.")
        [void]$md.AppendLine("")
    }
}


# === BLOCKING SUMMARY (hoisted to top) ===
# Surface aggregate BLOCKED checks (e.g. CI red, versions.props not bumped).
# The high-priority categories above already enumerate individual items,
# so exclude them here to avoid duplicate rows under two separate headings.
# Every Area whose items are hoisted into 🔴 High-priority items must be listed
# here (dependency-flow PRs are hoisted as '📦 Dependency-flow PR' rows, so they belong too).
$highPriorityCheckAreas = @(
    'P/0 priority blockers',
    'P/0 release-branch PRs',
    'Maestro PRs',
    "Merge-up PRs ($mergeUpChainLabel)"
)
$blockingChecks = @($checks | Where-Object {
    $_.Status -eq 'BLOCKED' -and -not ($highPriorityCheckAreas -contains $_.Area)
})
if ($blockingChecks.Count -gt 0) {
    [void]$md.AppendLine("## 🔴 Blocking — $($blockingChecks.Count) item(s)")
    [void]$md.AppendLine("")
    [void]$md.AppendLine("| Area | Details | Next action |")
    [void]$md.AppendLine("|------|---------|-------------|")
    foreach ($bc in $blockingChecks) {
        [void]$md.AppendLine("| $(Format-MarkdownCell $bc.Area) | $(Format-MarkdownCell $bc.Details) | $(Format-MarkdownCell $bc.NextAction) |")
    }
    [void]$md.AppendLine("")
} elseif ($highPriorityRows.Count -eq 0) {
    [void]$md.AppendLine("## 🟢 No blocking items")
    [void]$md.AppendLine("")
}

# === CLEANUP FOLLOW-UPS (post-release housekeeping) ===
# Items that are NOT release-blocking but are real follow-ups the release
# captain should track (e.g. bug template version dropdown not yet updated
# — that's a post-release cleanup, not a ship blocker).
$cleanupChecks = @($checks | Where-Object { $_.Status -eq 'CLEANUP' })
if ($cleanupChecks.Count -gt 0) {
    [void]$md.AppendLine("## 🧹 Cleanup follow-ups — $($cleanupChecks.Count) item(s)")
    [void]$md.AppendLine("")
    [void]$md.AppendLine("_Not release-blocking — these are post-ship housekeeping items to track separately._")
    [void]$md.AppendLine("")
    [void]$md.AppendLine("| Area | Details | Next action |")
    [void]$md.AppendLine("|------|---------|-------------|")
    foreach ($cc in $cleanupChecks) {
        [void]$md.AppendLine("| $(Format-MarkdownCell $cc.Area) | $(Format-MarkdownCell $cc.Details) | $(Format-MarkdownCell $cc.NextAction) |")
    }
    [void]$md.AppendLine("")
}

# === Recent CI Failure Scanner signals (hoisted near the top so signals
#     specific to this release branch are surfaced before the deeper
#     readiness checklist / PR tables) ===
[void]$md.AppendLine("## Recent CI Failure Scanner signals (``ci-scan``)")
[void]$md.AppendLine("")
$ciScanBlurb = "_Filtered to issues whose ``**Branch**: <name>`` body marker matches ``$SurveyRef`` (auto-filed by the CI Failure Scanner workflow every 12h). Fresh issues (<24h) are flagged 🆕._"
if ($ciScanFilteredOut -gt 0) {
    $ciScanBlurb += " _$ciScanFilteredOut other-branch issue(s) were excluded as not relevant to this release._"
}
[void]$md.AppendLine($ciScanBlurb)
[void]$md.AppendLine("")
if ($ciScanIssues.Count -eq 0) {
    [void]$md.AppendLine("_No ci-scan issues target ``$SurveyRef``._")
    [void]$md.AppendLine("")
} else {
    Add-CiScanTable -Builder $md -Issues $ciScanIssues
}

[void]$md.AppendLine("Generated at $generatedAt for ``$Repository``.")
[void]$md.AppendLine("")
# Report freshness banner — DERIVED-AT-RENDER note of how long ago this report was generated,
# with a ⏳ "may be stale" flag past the threshold. Pure presentation only. (The preview engine
# emits no semantic hash and refreshes every run, so there is no no-op to protect here; the
# banner still gives a viewer an at-a-glance staleness cue.) Fail-open if the helper is absent.
if ($generatedAt -and (Get-Command Format-ReportFreshnessBanner -ErrorAction SilentlyContinue)) {
    $previewFreshnessBanner = Format-ReportFreshnessBanner -GeneratedAt $generatedAt -Now ([DateTime]::UtcNow)
    if ($previewFreshnessBanner) {
        [void]$md.AppendLine($previewFreshnessBanner)
        [void]$md.AppendLine("")
    }
}
[void]$md.AppendLine("**Tracker:** ``$TrackerKey`` · mode=``$Mode`` · branch=``$Branch`` · survey=``$SurveyRef``")
[void]$md.AppendLine("")
if ($Mode -eq 'candidate') {
    [void]$md.AppendLine("> 🛫 **Pre-flight (candidate) mode.** Branch ``$Branch`` has not been cut yet. This report surveys ``$SurveyRef`` and shows what WOULD ship if the preview were cut today.")
    [void]$md.AppendLine("")
}
[void]$md.AppendLine("## Target")
[void]$md.AppendLine("")
[void]$md.AppendLine("| Field | Value |")
[void]$md.AppendLine("|-------|-------|")
[void]$md.AppendLine("| Branch | ``$Branch`` |")
[void]$md.AppendLine("| Inflight branch | ``$mainBranch`` |")
[void]$md.AppendLine("| Expected SDK channel | ``.NET $majorVersion.0.1xx SDK Preview $previewNumber`` |")
[void]$md.AppendLine("| Workload release channel | ``.NET $majorVersion Workload Release`` |")
[void]$md.AppendLine("| Expected PreReleaseVersionIteration | ``$previewNumber`` |")
[void]$md.AppendLine("")

[void]$md.AppendLine("## Readiness checklist")
[void]$md.AppendLine("")
Add-CheckTable -Builder $md -Checks $checks

[void]$md.AppendLine("## Maestro / dependency-flow PRs")
[void]$md.AppendLine("")
# Highlight any open SDK/VMR bump: its landed pin is only a candidate until the
# blessed build is confirmed locally (the Action can't reach the .NET Release Tracker).
$sdkBumpPrs = @($maestroPRs | Where-Object { Test-IsSdkBumpPr $_ })
if ($sdkBumpPrs.Count -gt 0) {
    $sdkBumpLinks = ($sdkBumpPrs | ForEach-Object { "[#$($_.number)]($($_.url))" }) -join ', '
    [void]$md.AppendLine("> [!WARNING]")
    [void]$md.AppendLine("> $($sdkBumpPrs.Count) open PR(s) bump the **SDK/VMR** ($sdkBumpLinks). The new SDK pin they land is only a **candidate** — the official/blessed build is designated in the internal .NET Release Tracker, which this Action can't reach. **Confirm the blessed SDK build locally** (see 🏷️ Preview $previewNumber component build above) before treating the bumped pin as final.")
    [void]$md.AppendLine("")
}
Add-PRTable -Builder $md -PRs $maestroPRs

[void]$md.AppendLine("## Release branch PRs")
[void]$md.AppendLine("")
Add-PRTable -Builder $md -PRs $targetHumanPRs

[void]$md.AppendLine("## $mainBranch inflight PRs")
[void]$md.AppendLine("")
Add-PRTable -Builder $md -PRs $inflightHumanPRs -MaxRows 30

[void]$md.AppendLine("## Priority release blockers")
[void]$md.AppendLine("")
Add-IssueTable -Builder $md -Issues $priorityIssues

[void]$md.AppendLine("## Known Build Error watch list")
[void]$md.AppendLine("")
Add-IssueTable -Builder $md -Issues $kbeIssues

[void]$md.AppendLine("## Maintainer next actions")
[void]$md.AppendLine("")
$nonReady = @($checks | Where-Object { $_.Status -ne "READY" })
if ($nonReady.Count -eq 0) {
    [void]$md.AppendLine("- No non-ready actions found by this public checklist.")
} else {
    foreach ($check in $nonReady) {
        [void]$md.AppendLine("- **$($check.Area)**: $($check.NextAction)")
    }
}
[void]$md.AppendLine("")

[void]$md.AppendLine("## Public/internal data boundary")
[void]$md.AppendLine("")
[void]$md.AppendLine("This public report intentionally omits internal logs, artifacts, private URLs, raw error text, secret names, account identifiers, and detailed dnceng/internal failure payloads. Use the local script with appropriate internal access for deeper validation.")
[void]$md.AppendLine("")

$markdownBody = $md.ToString()

# ===================================================================
# SAFETY NET: defang any remaining bare @-mentions in the final body.
# Primary defense is Format-GitHubHandle at emit time, but PR/issue
# titles or commit messages can contain raw `@user` references that
# would notify real users every time this report is filed. Wrap any
# `@handle` in backticks so GitHub renders it as a code span (no mention).
# ===================================================================
$markdownBody = [regex]::Replace(
    $markdownBody,
    '(^|[^a-zA-Z0-9/`])@([a-zA-Z0-9][a-zA-Z0-9_-]*(?:/[a-zA-Z0-9][a-zA-Z0-9_-]*)?)',
    '$1`$2`'
)

# ===================================================================
# BODY-SIZE SAFETY CAP
# ===================================================================
# GitHub rejects an issue body over 65,536 bytes; the daily refresh would then
# fail `gh issue edit` and the tracker would silently stop updating. The body
# has unbounded sections BOTH above the human-notes block (the itemized,
# uncapped "🔴 High-priority items" table) AND below it (the Maestro / release /
# inflight PR tables, rendered with Add-PRTable's default 100-row cap). A plain
# byte-prefix cut could therefore drop the notes begin/end markers — and a
# markerless fresh body makes the workflow skip the edit (freezing the tracker)
# or, worse, overwrite live Release Captain Notes. So we mirror the SR engine:
# strip the notes placeholder, truncate only the remaining content (reserving
# room for the notes block + message), boundary-repair, then RE-APPEND the notes
# block. This guarantees exactly one clean begin/end pair always survives for the
# workflow splice, independent of section order. The placeholder carries no human
# data (real notes live on the issue and are spliced in by the workflow), so
# removing and re-adding it is lossless. The tracker markers sit at the very top,
# well inside the reserved prefix, so they survive too.
$bodyBytes = [System.Text.Encoding]::UTF8.GetByteCount($markdownBody)
if ($bodyBytes -gt $MaxBodyBytes) {
    $truncateMsg = "`n`n> ⚠️ **Report truncated** ($bodyBytes bytes exceeded cap of $MaxBodyBytes). See full data in workflow artifacts.`n"
    $tail = [System.Text.Encoding]::UTF8.GetByteCount($truncateMsg)
    $notesTail = "`n" + $notesBlockText
    $notesReserve = [System.Text.Encoding]::UTF8.GetByteCount($notesTail)
    $bodyNoNotes = $markdownBody.Replace($notesBlockText, '')
    $targetLen = $MaxBodyBytes - $tail - $notesReserve
    if ($targetLen -lt 0) { $targetLen = 0 }
    $allBytes = [System.Text.Encoding]::UTF8.GetBytes($bodyNoNotes)
    if ($targetLen -gt $allBytes.Length) { $targetLen = $allBytes.Length }
    $truncatedBytes = New-Object byte[] $targetLen
    [Array]::Copy($allBytes, 0, $truncatedBytes, 0, $targetLen)
    # UTF-8 boundary repair: drop a trailing INCOMPLETE multibyte sequence so
    # GetString() doesn't emit a U+FFFD (which re-encodes to 3 bytes and could
    # push the body back over the cap). Walk back over continuation bytes
    # (10xxxxxx) to the lead byte, infer the sequence length, and cut at the
    # lead only when the full sequence doesn't fit.
    if ($truncatedBytes.Length -gt 0) {
        $i = $truncatedBytes.Length - 1
        while ($i -ge 0 -and ($truncatedBytes[$i] -band 0xC0) -eq 0x80) { $i-- }
        if ($i -ge 0) {
            $lead = $truncatedBytes[$i]
            $seqLen = if (($lead -band 0x80) -eq 0x00) { 1 }
                      elseif (($lead -band 0xE0) -eq 0xC0) { 2 }
                      elseif (($lead -band 0xF0) -eq 0xE0) { 3 }
                      elseif (($lead -band 0xF8) -eq 0xF0) { 4 }
                      else { 1 }
            if (($i + $seqLen) -gt $truncatedBytes.Length) {
                $newArr = New-Object byte[] $i
                [Array]::Copy($truncatedBytes, 0, $newArr, 0, $i)
                $truncatedBytes = $newArr
            }
        }
    }
    $markdownBody = [System.Text.Encoding]::UTF8.GetString($truncatedBytes) + $notesTail + $truncateMsg
}

# ===================================================================
# OUTPUT
# ===================================================================
if ($OutputDir) {
    if (-not (Test-Path $OutputDir)) {
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    }
    $jsonPath = Join-Path $OutputDir "preview-readiness.json"
    $mdPath = Join-Path $OutputDir "preview-readiness.md"

    $report | ConvertTo-Json -Depth 20 | Out-File -FilePath $jsonPath -Encoding utf8
    $markdownBody | Out-File -FilePath $mdPath -Encoding utf8

    Write-Host "Wrote $jsonPath"
    Write-Host "Wrote $mdPath"
}

switch ($OutputFormat) {
    "json" {
        $report | ConvertTo-Json -Depth 20
    }
    "both" {
        if (-not $OutputDir) {
            $report | ConvertTo-Json -Depth 20
        }
        $markdownBody
    }
    default {
        # "markdown" — if -OutputDir was given, the file is already on
        # disk; still write the body to stdout so dispatchers can capture
        # it inline.
        $markdownBody
    }
}
