#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Generates a public-safe .NET 11 release-readiness report.

.DESCRIPTION
    Resolves a net11 release target and checks public release-readiness signals:
    branch/version setup, Maestro PRs, release PRs, release blockers, KBE issues,
    CI truth handoff status, Xcode readiness, and sanitized internal pipeline state.

    This script is deterministic by design. It does not approve, merge, rerun,
    promote, or mutate GitHub/Maestro/darc state.
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$Target = "auto",

    [Parameter(Mandatory = $false)]
    [ValidateSet("markdown", "json")]
    [string]$OutputFormat = "markdown",

    [Parameter(Mandatory = $false)]
    [string]$Repository = "dotnet/maui",

    [Parameter(Mandatory = $false)]
    [switch]$IncludeInternal,

    [Parameter(Mandatory = $false)]
    [string]$InternalBuildId,

    [Parameter(Mandatory = $false)]
    [bool]$PublicSafe = $true
)

$ErrorActionPreference = "Stop"

$StatusRank = @{
    "READY" = 0
    "WATCH" = 1
    "UNKNOWN" = 2
    "INSUFFICIENT_DATA" = 2
    "BLOCKED" = 3
}

function Invoke-GitHubWithRetry {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,

        [Parameter(Mandatory = $true)]
        [string]$Description,

        [Parameter(Mandatory = $false)]
        [int]$MaxRetries = 3
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
    param([string]$Branch)

    $encodedBranch = [System.Uri]::EscapeDataString($Branch)
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

    throw "Failed to check branch $Branch"
}

function Get-LatestPreviewNumber {
    try {
        $refs = Invoke-GitHubWithRetry -Arguments @(
            "api",
            "repos/$Repository/git/matching-refs/heads/release/11.0.1xx-preview",
            "--jq",
            ".[].ref"
        ) -Description "list net11 preview branches"

        $numbers = @()
        foreach ($line in ($refs -split "`n")) {
            if ($line -match "^refs/heads/release/11\.0\.1xx-preview(\d+)$") {
                $numbers += [int]$Matches[1]
            }
        }

        if ($numbers.Count -eq 0) {
            return $null
        }

        return ($numbers | Sort-Object -Descending | Select-Object -First 1)
    }
    catch {
        return $null
    }
}

function Resolve-Target {
    param([string]$InputTarget)

    $value = $InputTarget.Trim()
    $lower = $value.ToLowerInvariant()

    if ($lower -eq "auto" -or $lower -eq "latest") {
        $latestPreview = Get-LatestPreviewNumber
        if ($latestPreview) {
            $lower = "net11-preview$latestPreview"
        } else {
            $lower = "net11.0"
        }
    }

    if ($lower -in @("net11", "net11.0")) {
        return [PSCustomObject]@{
            CanonicalId = "net11"
            DisplayName = "net11.0"
            Branch = "net11.0"
            ExpectedIteration = $null
            ExpectedChannel = ".NET 11.0.1xx SDK"
            WorkloadReleaseChannel = ".NET 11 Workload Release"
            IsPreview = $false
        }
    }

    if ($lower -match "^net11-preview-?(\d+)$" -or $lower -match "^release/11\.0\.1xx-preview(\d+)$") {
        $preview = [int]$Matches[1]
        return [PSCustomObject]@{
            CanonicalId = "net11-preview$preview"
            DisplayName = "net11 preview $preview"
            Branch = "release/11.0.1xx-preview$preview"
            ExpectedIteration = $preview
            ExpectedChannel = ".NET 11.0.1xx SDK Preview $preview"
            WorkloadReleaseChannel = ".NET 11 Workload Release"
            IsPreview = $true
        }
    }

    throw "Unsupported target '$InputTarget'. Use auto, net11.0, net11-previewN, or release/11.0.1xx-previewN."
}

function Get-PreReleaseVersionIteration {
    param([string]$Branch)

    $versions = Get-ContentFromRepo -Path "eng/Versions.props" -Ref $Branch
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
    param([string]$Branch)

    $variables = Get-ContentFromRepo -Path "eng/pipelines/common/variables.yml" -Ref $Branch
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
        RequiredXcode = $required
        DeviceTestsRequiredXcode = $deviceRequired
    }
}

function Get-OpenPullRequests {
    param([string]$BaseBranch)

    if (-not (Test-BranchExists -Branch $BaseBranch)) {
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
    param([string]$Label)

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
        "number,title,url,labels,milestone,updatedAt"
    ) -Description "list issues with label '$Label'"

    return ConvertFrom-JsonOrEmptyArray $json
}

function Test-IssueReleaseRelevant {
    param(
        $Issue,
        $ResolvedTarget
    )

    $labels = @($Issue.labels | ForEach-Object { $_.name })
    $milestone = if ($Issue.milestone.title) { $Issue.milestone.title } else { "" }
    $haystack = "$($Issue.title) $milestone $($labels -join ' ')"

    if ($haystack -match "(?i)net\s*11|net11|11\.0|11\.0\.1xx|xcode") {
        return $true
    }

    if ($ResolvedTarget.IsPreview -and $haystack -match "(?i)preview\s*$($ResolvedTarget.ExpectedIteration)|preview$($ResolvedTarget.ExpectedIteration)") {
        return $true
    }

    return $false
}

function Get-ReleaseRelevantIssuesByLabel {
    param(
        [string[]]$Labels,
        $ResolvedTarget
    )

    $issues = @()
    foreach ($label in $Labels) {
        $issues += Get-IssuesByLabel -Label $label
    }

    $deduped = $issues |
        Sort-Object number -Unique |
        Where-Object { Test-IssueReleaseRelevant -Issue $_ -ResolvedTarget $ResolvedTarget }

    return @($deduped)
}

function Get-PRAction {
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

function New-Check {
    param(
        [string]$Area,
        [string]$Status,
        [string]$Details,
        [string]$NextAction
    )

    return [PSCustomObject]@{
        Area = $Area
        Status = $Status
        Details = $Details
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

function Escape-Markdown {
    param([string]$Value)

    if ($null -eq $Value) {
        return ""
    }

    return ($Value -replace "\|", "\|").Trim()
}

function Add-CheckTable {
    param(
        [System.Text.StringBuilder]$Builder,
        [array]$Checks
    )

    [void]$Builder.AppendLine("| Area | Status | Details | Next action |")
    [void]$Builder.AppendLine("|------|--------|---------|-------------|")
    foreach ($check in $Checks) {
        [void]$Builder.AppendLine("| $(Escape-Markdown $check.Area) | **$($check.Status)** | $(Escape-Markdown $check.Details) | $(Escape-Markdown $check.NextAction) |")
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
        $author = if ($pr.author.login) { "@$($pr.author.login)" } else { "unknown" }
        [void]$Builder.AppendLine("| [#$($pr.number)]($($pr.url)) | $(Escape-Markdown $pr.title) | $author | ``$($pr.baseRefName)`` | **$($action.Status)** | $($action.Age)d | $(Escape-Markdown $action.Action) |")
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
        $milestone = if ($issue.milestone.title) { $issue.milestone.title } else { "" }
        [void]$Builder.AppendLine("| [#$($issue.number)]($($issue.url)) | $(Escape-Markdown $issue.title) | $(Escape-Markdown $labels) | $(Escape-Markdown $milestone) |")
    }
    [void]$Builder.AppendLine("")
}

$resolvedTarget = Resolve-Target -InputTarget $Target
$checks = @()
$targetBranchExists = Test-BranchExists -Branch $resolvedTarget.Branch

if ($targetBranchExists) {
    $checks += New-Check -Area "Target branch" -Status "READY" -Details "``$($resolvedTarget.Branch)`` exists." -NextAction "Continue release-readiness checks."
} else {
    $checks += New-Check -Area "Target branch" -Status "BLOCKED" -Details "``$($resolvedTarget.Branch)`` does not exist." -NextAction "Create or select the correct release branch before declaring readiness."
}

$targetIteration = $null
$net11Iteration = $null
$xcodeRequirements = [PSCustomObject]@{ RequiredXcode = $null; DeviceTestsRequiredXcode = $null }

if ($targetBranchExists) {
    try {
        $targetIteration = Get-PreReleaseVersionIteration -Branch $resolvedTarget.Branch
        if ($resolvedTarget.IsPreview) {
            if ($targetIteration -eq [string]$resolvedTarget.ExpectedIteration) {
                $checks += New-Check -Area "Preview iteration" -Status "READY" -Details "``$($resolvedTarget.Branch)`` has PreReleaseVersionIteration=$targetIteration." -NextAction "No version-iteration action needed."
            } else {
                $displayValue = if ($targetIteration) { $targetIteration } else { "<empty>" }
                $checks += New-Check -Area "Preview iteration" -Status "BLOCKED" -Details "``$($resolvedTarget.Branch)`` has PreReleaseVersionIteration=$displayValue; expected $($resolvedTarget.ExpectedIteration)." -NextAction "Bump the release branch to match the preview number."
            }
        } else {
            $displayValue = if ($targetIteration) { $targetIteration } else { "<empty>" }
            $checks += New-Check -Area "Version iteration" -Status "WATCH" -Details "``$($resolvedTarget.Branch)`` PreReleaseVersionIteration is $displayValue." -NextAction "Confirm this matches the active release phase."
        }
    }
    catch {
        $checks += New-Check -Area "Preview iteration" -Status "UNKNOWN" -Details "Could not read version iteration from target branch." -NextAction "Run locally and inspect eng/Versions.props."
    }

    try {
        $xcodeRequirements = Get-XcodeRequirements -Branch $resolvedTarget.Branch
    }
    catch {
        $checks += New-Check -Area "Xcode variables" -Status "UNKNOWN" -Details "Could not read required Xcode variables." -NextAction "Inspect eng/pipelines/common/variables.yml on the target branch."
    }
}

if (Test-BranchExists -Branch "net11.0") {
    try {
        $net11Iteration = Get-PreReleaseVersionIteration -Branch "net11.0"
        $displayValue = if ($net11Iteration) { $net11Iteration } else { "<empty>" }
        if ($resolvedTarget.IsPreview -and $net11Iteration -and ([int]$net11Iteration -lt [int]$resolvedTarget.ExpectedIteration)) {
            $checks += New-Check -Area "net11.0 preview-next bump" -Status "BLOCKED" -Details "``net11.0`` PreReleaseVersionIteration is $displayValue; target preview is $($resolvedTarget.ExpectedIteration)." -NextAction "Confirm net11.0 is bumped for preview-next."
        } else {
            $checks += New-Check -Area "net11.0 preview-next bump" -Status "WATCH" -Details "``net11.0`` PreReleaseVersionIteration is $displayValue." -NextAction "Confirm this is correct for the next preview train."
        }
    }
    catch {
        $checks += New-Check -Area "net11.0 preview-next bump" -Status "UNKNOWN" -Details "Could not read net11.0 PreReleaseVersionIteration." -NextAction "Run locally and inspect eng/Versions.props on net11.0."
    }
} else {
    $checks += New-Check -Area "net11.0 branch" -Status "UNKNOWN" -Details "``net11.0`` branch was not found." -NextAction "Confirm branch state before release."
}

$targetPRs = @()
$net11PRs = @()
if ($targetBranchExists) {
    $targetPRs = Get-OpenPullRequests -BaseBranch $resolvedTarget.Branch
}
if ($resolvedTarget.Branch -ne "net11.0" -and (Test-BranchExists -Branch "net11.0")) {
    $net11PRs = Get-OpenPullRequests -BaseBranch "net11.0"
}

$allReleasePRs = @($targetPRs) + @($net11PRs)
$maestroPRs = @($allReleasePRs | Where-Object { $_.author.login -match "dotnet-maestro" })
$targetHumanPRs = @($targetPRs | Where-Object { $_.author.login -notmatch "dotnet-maestro" })
$inflightHumanPRs = @($net11PRs | Where-Object { $_.author.login -notmatch "dotnet-maestro" })

$blockedPRs = @()
$watchPRs = @()
foreach ($pr in $allReleasePRs) {
    $action = Get-PRAction -PR $pr
    if ($action.Status -eq "BLOCKED") {
        $blockedPRs += $pr
    } elseif ($action.Status -eq "WATCH") {
        $watchPRs += $pr
    }
}

if ($maestroPRs.Count -eq 0) {
    $checks += New-Check -Area "Maestro PRs" -Status "READY" -Details "No open Maestro PRs target ``$($resolvedTarget.Branch)`` or ``net11.0``." -NextAction "Continue monitoring for new dependency-flow PRs."
} elseif (($maestroPRs | Where-Object { (Get-PRAction -PR $_).Status -eq "BLOCKED" }).Count -gt 0) {
    $checks += New-Check -Area "Maestro PRs" -Status "BLOCKED" -Details "$($maestroPRs.Count) open Maestro PR(s), including blocked/conflicted PRs." -NextAction "Resolve blocked Maestro PRs before release."
} else {
    $checks += New-Check -Area "Maestro PRs" -Status "WATCH" -Details "$($maestroPRs.Count) open Maestro PR(s) need review/merge triage." -NextAction "Review dependency PRs and merge expected updates."
}

if ($targetHumanPRs.Count -eq 0) {
    $checks += New-Check -Area "Release branch PRs" -Status "READY" -Details "No non-Maestro open PRs target ``$($resolvedTarget.Branch)``." -NextAction "No direct release-branch PR action from this check."
} elseif (($targetHumanPRs | Where-Object { (Get-PRAction -PR $_).Status -eq "BLOCKED" }).Count -gt 0) {
    $checks += New-Check -Area "Release branch PRs" -Status "BLOCKED" -Details "$($targetHumanPRs.Count) non-Maestro PR(s) target the release branch, including blocked PRs." -NextAction "Resolve blocked release-branch PRs before release."
} else {
    $checks += New-Check -Area "Release branch PRs" -Status "WATCH" -Details "$($targetHumanPRs.Count) non-Maestro PR(s) target the release branch." -NextAction "Confirm which PRs must merge for the release."
}

if ($inflightHumanPRs.Count -eq 0) {
    $checks += New-Check -Area "net11.0 inflight branch health" -Status "READY" -Details "No non-Maestro inflight PRs are open on ``net11.0``." -NextAction "Continue monitoring inflight branch health."
} elseif (($inflightHumanPRs | Where-Object { (Get-PRAction -PR $_).Status -eq "BLOCKED" }).Count -gt 0) {
    $checks += New-Check -Area "net11.0 inflight branch health" -Status "WATCH" -Details "$($inflightHumanPRs.Count) non-Maestro PR(s) are open on ``net11.0``, including blocked PRs." -NextAction "Track as preview-next/inflight work; do not treat every inflight PR as a direct blocker for this release branch."
} else {
    $checks += New-Check -Area "net11.0 inflight branch health" -Status "WATCH" -Details "$($inflightHumanPRs.Count) non-Maestro PR(s) are open on ``net11.0``." -NextAction "Review inflight queue for preview-next readiness."
}

$priorityIssues = Get-ReleaseRelevantIssuesByLabel -Labels @("p/0", "p/1") -ResolvedTarget $resolvedTarget
$kbeIssues = Get-ReleaseRelevantIssuesByLabel -Labels @("Known Build Error") -ResolvedTarget $resolvedTarget

if ($priorityIssues.Count -gt 0) {
    $checks += New-Check -Area "Priority blockers" -Status "BLOCKED" -Details "$($priorityIssues.Count) open P/0 or P/1 issue(s) look release-relevant." -NextAction "Triage whether each blocks this release target."
} else {
    $checks += New-Check -Area "Priority blockers" -Status "READY" -Details "No open release-relevant P/0 or P/1 issues found by public search." -NextAction "Confirm with release owners."
}

if ($kbeIssues.Count -gt 0) {
    $checks += New-Check -Area "Known Build Errors" -Status "WATCH" -Details "$($kbeIssues.Count) open release-relevant KBE issue(s) found." -NextAction "Use #35052 CI truth to decide accepted-known vs release-blocking."
} else {
    $checks += New-Check -Area "Known Build Errors" -Status "READY" -Details "No release-relevant open KBE issues found by public search." -NextAction "Continue monitoring."
}

$checks += New-Check -Area "CI truth" -Status "INSUFFICIENT_DATA" -Details "#35052 structured CI evidence is not wired into this script yet." -NextAction "Do not infer release readiness from GitHub checks alone; consume #35052 output when available."

$requiredXcode = if ($xcodeRequirements.RequiredXcode) { $xcodeRequirements.RequiredXcode } else { "unknown" }
$deviceXcode = if ($xcodeRequirements.DeviceTestsRequiredXcode) { $xcodeRequirements.DeviceTestsRequiredXcode } else { "unknown" }
$checks += New-Check -Area "Xcode / ICM" -Status "UNKNOWN" -Details "REQUIRED_XCODE=$requiredXcode; DEVICETESTS_REQUIRED_XCODE=$deviceXcode." -NextAction "Verify hosted Mac pool support and file/update ICM immediately when public Xcode availability requires it."

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
        }
        catch {
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

$report = [PSCustomObject]@{
    GeneratedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    Repository = $Repository
    Target = $resolvedTarget
    OverallStatus = $overallStatus
    Checks = $checks
    MaestroPullRequests = $maestroPRs
    ReleasePullRequests = $targetHumanPRs
    InflightPullRequests = $inflightHumanPRs
    PriorityIssues = $priorityIssues
    KnownBuildErrorIssues = $kbeIssues
}

if ($OutputFormat -eq "json") {
    $report | ConvertTo-Json -Depth 20
    exit 0
}

$md = [System.Text.StringBuilder]::new()
[void]$md.AppendLine("<!-- NET11_RELEASE_READY_BEGIN -->")
[void]$md.AppendLine("# Release READY — $($resolvedTarget.CanonicalId) — $((Get-Date).ToString("yyyy-MM-dd"))")
[void]$md.AppendLine("")
[void]$md.AppendLine("**Overall status:** **$overallStatus**")
[void]$md.AppendLine("")
[void]$md.AppendLine("Generated at $($report.GeneratedAt) for ``$Repository``.")
[void]$md.AppendLine("")
[void]$md.AppendLine("## Target")
[void]$md.AppendLine("")
[void]$md.AppendLine("| Field | Value |")
[void]$md.AppendLine("|-------|-------|")
[void]$md.AppendLine("| Canonical target | ``$($resolvedTarget.CanonicalId)`` |")
[void]$md.AppendLine("| Branch | ``$($resolvedTarget.Branch)`` |")
[void]$md.AppendLine("| Expected SDK channel | ``$($resolvedTarget.ExpectedChannel)`` |")
[void]$md.AppendLine("| Workload release channel | ``$($resolvedTarget.WorkloadReleaseChannel)`` |")
if ($resolvedTarget.ExpectedIteration) {
    [void]$md.AppendLine("| Expected PreReleaseVersionIteration | ``$($resolvedTarget.ExpectedIteration)`` |")
}
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

[void]$md.AppendLine("## net11.0 inflight PRs")
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
foreach ($check in ($checks | Where-Object { $_.Status -ne "READY" })) {
    [void]$md.AppendLine("- **$($check.Area)**: $($check.NextAction)")
}
if (($checks | Where-Object { $_.Status -ne "READY" }).Count -eq 0) {
    [void]$md.AppendLine("- No non-ready actions found by this public checklist.")
}
[void]$md.AppendLine("")

[void]$md.AppendLine("## Public/internal data boundary")
[void]$md.AppendLine("")
[void]$md.AppendLine("This public report intentionally omits internal logs, artifacts, private URLs, raw error text, secret names, account identifiers, and detailed dnceng/internal failure payloads. Use the local script with appropriate internal access for deeper validation.")
[void]$md.AppendLine("")
[void]$md.AppendLine("<!-- NET11_RELEASE_READY_END -->")

$md.ToString()
