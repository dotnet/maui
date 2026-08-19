#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [string]$AgentOutputPath = $env:GH_AW_AGENT_OUTPUT,
    [string]$Repository = $env:GITHUB_REPOSITORY
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1') -Force

if ([string]::IsNullOrWhiteSpace($Repository)) {
    throw 'GITHUB_REPOSITORY is required.'
}

$agentOutput = Read-RegularJsonFile -Path $AgentOutputPath
$createItems = @($agentOutput.items | Where-Object { $_.type -eq 'create_issue' })
if ($createItems.Count -eq 0) {
    Write-Host 'No create_issue safe-output requested; final leak-hunter de-dup gate is not applicable.'
    return
}
if ($createItems.Count -gt 8) {
    throw "Expected at most eight create_issue items, found $($createItems.Count)."
}

$requestedItems = [System.Collections.Generic.List[object]]::new()
$requestedTitles = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal
)
$requestedApis = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal
)
foreach ($item in $createItems) {
    $title = [string]$item.title
    if (-not $title.StartsWith('[leak-scan] ', [StringComparison]::Ordinal)) {
        throw "Every create-issue title must start with the literal '[leak-scan] ' prefix."
    }
    $api = Get-CanonicalLeakApi -Title $title
    if ([string]::IsNullOrWhiteSpace($api)) {
        throw "Could not derive a canonical Type.Member from create-issue title '$title'."
    }
    if (-not $requestedTitles.Add($title)) {
        throw "Multiple create-issue outputs use the same exact title '$title'."
    }
    if (-not $requestedApis.Add($api)) {
        throw "Multiple create-issue outputs use the same canonical API '$api'. Emit at most one issue per canonical API in each safe-output batch."
    }
    $requestedItems.Add([pscustomobject]@{
            Api = $api
            Item = $item
        })
}

$openIssues = @(
    Invoke-LeakGhJson -Arguments @(
        'issue', 'list',
        '--repo', $Repository,
        '--search', '"[leak-scan]" in:title',
        '--state', 'open',
        '--label', 'agentic-workflows',
        '--limit', '1000',
        '--json', 'number,title,body,url'
    )
)
if ($openIssues.Count -ge 1000) {
    throw "Open [leak-scan] search returned $($openIssues.Count) rows at the GitHub Search API ceiling; refusing a potentially truncated final gate."
}

$merged = @(
    Invoke-LeakGhJson -Arguments @(
        'pr', 'list',
        '--repo', $Repository,
        '--state', 'merged',
        '--limit', '1000',
        '--search', '"[leak-fix]" in:title',
        '--json', 'number,title,body,baseRefName,mergedAt,url'
    )
)
if ($merged.Count -ge 1000) {
    throw "Merged [leak-fix] search returned $($merged.Count) rows at the GitHub Search API ceiling; refusing a potentially truncated final gate."
}

$authoritativeMerged = @(
    Select-LeakAuthoritativePullRequests `
        -PullRequests $merged `
        -Context 'Final merged leak-fix de-dup search'
)
$eligibleMerged = @($authoritativeMerged | Where-Object {
        $null -ne $_.mergedAt -and
        ([string]$_.title).StartsWith('[leak-fix] ', [StringComparison]::Ordinal)
    })
$mergedReverts = @(
    Get-RelevantMergedLeakReverts `
        -Repository $Repository `
        -TargetPullRequests $eligibleMerged
)
$effectivelyReverted = @(
    Get-EffectiveRevertedPullRequestNumbers `
        -Repository $Repository `
        -FixPullRequests $eligibleMerged `
        -MergedRevertPullRequests $mergedReverts
)
$reverted = [System.Collections.Generic.HashSet[int]]::new()
foreach ($number in $effectivelyReverted) {
    [void]$reverted.Add($number)
}
$eligibleMerged = @($eligibleMerged | Where-Object {
        -not $reverted.Contains([int]$_.number)
    })

# Validation is intentionally batch-atomic. Do not rewrite agent output at this trusted
# boundary: any stale item aborts before Process Safe Outputs, and distinct items retry next run.
foreach ($requested in $requestedItems) {
    $api = [string]$requested.Api
    $openApiMatches = @($openIssues | Where-Object {
            $issueTitle = [string]$_.title
            $issueTitle.StartsWith('[leak-scan] ', [StringComparison]::Ordinal) -and
            (Get-CanonicalExistingLeakApi -Title $issueTitle) -ceq $api
        })
    if ($openApiMatches.Count -gt 0) {
            throw "Final leak-hunter de-dup gate blocked issue creation for '$api': same-API open issue match $($openApiMatches.number -join ', '). The safe-output batch is rejected atomically; other items can retry on the next scheduled run."
    }

    $mergedApiMatches = @($eligibleMerged | Where-Object {
            (Get-CanonicalExistingLeakApi -Title ([string]$_.title)) -ceq $api
        })
    if ($mergedApiMatches.Count -gt 0) {
            throw "Final leak-hunter de-dup gate blocked issue creation for '$api': same-API merged fix match $($mergedApiMatches.number -join ', '). The safe-output batch is rejected atomically; other items can retry on the next scheduled run."
    }
}

$apis = @($requestedItems | ForEach-Object { $_.Api } | Sort-Object -Unique)
Write-Host "Final leak-hunter de-dup gate passed for APIs: $($apis -join ', ')."
