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

function Test-IssueReleaseRelevant {
    <#
    .SYNOPSIS
        Returns $true if a labelled issue is plausibly relevant to the
        active major / preview number based on its title, milestone, or
        labels.
    .NOTES
        Uses a wide net on purpose — false negatives are worse than false
        positives for release-readiness triage.
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
        return $true
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
          2. Maestro  — non-P/0 PRs authored by dotnet-maestro (target OR inflight).
          3. Merge-up — non-P/0, non-Maestro target PRs that are automated
                    main → survey-ref merges (head `merge/<x>-to-<y>` or title
                    "[automated] Merge branch ...").
          4. Generic-human (target) — the remaining survey-ref PRs.
          5. Inflight-human — non-Maestro PRs on the inflight (net<major>.0) branch.

        Buckets are mutually exclusive by PR number. Inflight PRs never escalate
        to P/0 (only survey-ref PRs block), matching the engine's release scope:
        $p0PrNumbers is computed from $TargetPRs only, and PR numbers are globally
        unique, so excluding them from the target+inflight Maestro set cannot drop
        an inflight PR. StrictMode-safe on two fronts: (1) inputs are normalized to
        drop $null elements up front, so an AutomationNull list (what
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

    $allReleasePRs = @($TargetPRs) + @($InflightPRs)

    # 1. P/0 first (highest precedence), from survey-ref PRs only.
    $p0Prs = @($TargetPRs | Where-Object { Test-IsP0Pr $_ })
    $p0PrNumbers = @($p0Prs | ForEach-Object { $_.number })

    # 2. Maestro (non-P/0), across target + inflight.
    $maestroPRs = @($allReleasePRs | Where-Object { $_.author -and $_.author.login -match "dotnet-maestro" -and ($p0PrNumbers -notcontains $_.number) })

    # Non-P/0, non-Maestro humans, split by scope.
    $targetHumanPRsRaw = @($TargetPRs | Where-Object { -not ($_.author -and $_.author.login -match "dotnet-maestro") -and ($p0PrNumbers -notcontains $_.number) })
    $inflightHumanPRs = @($InflightPRs | Where-Object { -not ($_.author -and $_.author.login -match "dotnet-maestro") })

    # 3. Merge-up: non-P/0, non-Maestro target PRs. MAUI convention:
    #   - head ref like `merge/main-to-net11.0` or `merge/preview4-to-net11.0`
    #   - title like "[automated] Merge branch 'main' => 'net11.0'"
    $mergeUpPRs = @($targetHumanPRsRaw | Where-Object {
        ($_.headRefName -and $_.headRefName -match '^merge/.+-to-') -or
        ($_.title -and $_.title -match '^\[automated\] Merge branch')
    })
    $mergeUpPrNumbers = @($mergeUpPRs | ForEach-Object { $_.number })

    # 4. Generic-human (target) = the remainder, counted/listed once.
    $targetHumanPRs = @($targetHumanPRsRaw | Where-Object { $mergeUpPrNumbers -notcontains $_.number })

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
if ($MyInvocation.InvocationName -eq '.' -or $MyInvocation.Line -match '^\.\s') { return }

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
    $checks += New-Check -Area "Maestro PRs" -Status "READY" -Details "No open Maestro PRs target ``$SurveyRef`` or ``$mainBranch``." -NextAction "Continue monitoring for new dependency-flow PRs."
} elseif (@($maestroPRs | Where-Object { (Get-PRAction -PR $_).Status -eq "BLOCKED" }).Count -gt 0) {
    $checks += New-Check -Area "Maestro PRs" -Status "BLOCKED" -Details "$($maestroPRs.Count) open Maestro PR(s), including blocked/conflicted PRs." -NextAction "Resolve blocked Maestro PRs before release."
} else {
    $checks += New-Check -Area "Maestro PRs" -Status "WATCH" -Details "$($maestroPRs.Count) open Maestro PR(s) need review/merge triage." -NextAction "Review dependency PRs and merge expected updates."
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
    $checks += New-Check -Area "Merge-up PRs (main → $SurveyRef)" -Status "BLOCKED" -Details "$($mergeUpPRs.Count) open merge-up PR(s). See 🔴 High-priority items at top. Stuck merge-up PRs block daily flow and accumulate conflicts." -NextAction "Resolve and merge each before shipping."
} else {
    $checks += New-Check -Area "Merge-up PRs (main → $SurveyRef)" -Status "READY" -Details "No open merge-up PRs from ``main`` → ``$SurveyRef``." -NextAction "Continue monitoring."
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
#   4. Merge-up PRs (main → survey ref) — daily-flow sync PRs whose head ref
#      matches `merge/...-to-...` or title starts with "[automated] Merge branch".
#      A stuck merge-up PR accumulates conflicts and starves the release branch
#      of new fixes from main.
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
    [void]$highPriorityRows.Add(@{
        kind = '📦 Maestro PR'
        link = "[#$($pr.number)]($($pr.url))"
        title = $pr.title
        actor = "base ``$($pr.baseRefName)``, $($action.Age)d old"
        nextAction = $action.Action
    })
}
foreach ($pr in $mergeUpPRs) {
    $action = Get-PRAction -PR $pr
    [void]$highPriorityRows.Add(@{
        kind = "🔀 Merge-up PR (main → $SurveyRef)"
        link = "[#$($pr.number)]($($pr.url))"
        title = $pr.title
        actor = "base ``$($pr.baseRefName)``, $($action.Age)d old"
        nextAction = $action.Action
    })
}

if ($highPriorityRows.Count -gt 0) {
    [void]$md.AppendLine("## 🔴 High-priority items — $($highPriorityRows.Count) item(s)")
    [void]$md.AppendLine("")
    [void]$md.AppendLine("_P/0 issues, P/0 PRs, Maestro PRs, and ``main`` → ``$SurveyRef`` merge-up PRs. Resolve these before treating the release as ready._")
    [void]$md.AppendLine("")
    [void]$md.AppendLine("| Kind | Item | Title | Context | Next action |")
    [void]$md.AppendLine("|------|------|-------|---------|-------------|")
    foreach ($row in $highPriorityRows) {
        [void]$md.AppendLine("| $(Format-MarkdownCell $row.kind) | $($row.link) | $(Format-MarkdownCell $row.title) | $(Format-MarkdownCell $row.actor) | $(Format-MarkdownCell $row.nextAction) |")
    }
    [void]$md.AppendLine("")
}

# === BLOCKING SUMMARY (hoisted to top) ===
# Surface aggregate BLOCKED checks (e.g. CI red, versions.props not bumped).
# The high-priority categories above already enumerate individual items,
# so exclude them here to avoid duplicate rows under two separate headings.
# Every Area whose items are hoisted into 🔴 High-priority items must be listed
# here (Maestro PRs are hoisted as '📦 Maestro PR' rows, so they belong too).
$highPriorityCheckAreas = @(
    'P/0 priority blockers',
    'P/0 release-branch PRs',
    'Maestro PRs',
    "Merge-up PRs (main → $SurveyRef)"
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

# Human-editable section, preserved across re-runs by workflow body merge.
# Built as a reusable block (like the SR engine) so the body-size cap below can
# strip it, truncate the remaining content, then re-append it — guaranteeing the
# begin/end markers always survive truncation regardless of section order. The
# "🔴 High-priority items" table above this block is itemized and uncapped, so a
# naive byte-prefix cut could otherwise drop these markers.
$notesSb = [System.Text.StringBuilder]::new()
[void]$notesSb.AppendLine("<!-- release-readiness:human-notes:begin -->")
[void]$notesSb.AppendLine("## Release Captain Notes")
[void]$notesSb.AppendLine("")
[void]$notesSb.AppendLine("_Add manual notes here. Anything between these begin/end markers is preserved across automated re-runs._")
[void]$notesSb.AppendLine("<!-- release-readiness:human-notes:end -->")
$notesBlockText = $notesSb.ToString()
[void]$md.Append($notesBlockText)
[void]$md.AppendLine("")

[void]$md.AppendLine("## Readiness checklist")
[void]$md.AppendLine("")
Add-CheckTable -Builder $md -Checks $checks

[void]$md.AppendLine("## Maestro / dependency-flow PRs")
[void]$md.AppendLine("")
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
