#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [string]$AgentOutputPath = $env:GH_AW_AGENT_OUTPUT,
    [string]$Repository = $env:GITHUB_REPOSITORY
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1') -Force

function Read-RegularJsonFile {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required JSON file is missing: $Path"
    }
    $item = Get-Item -LiteralPath $Path -Force
    if ($item.LinkType) {
        throw "Refusing symbolic-link JSON file: $Path"
    }
    $raw = Get-Content -LiteralPath $Path -Raw
    if ([string]::IsNullOrWhiteSpace($raw) -or $raw.Length -gt 1MB) {
        throw "JSON file is empty or too large: $Path"
    }
    try {
        return $raw | ConvertFrom-Json
    } catch {
        throw "Invalid JSON in '$Path': $($_.Exception.Message)"
    }
}

function Assert-SameApiDisclosure {
    param(
        [Parameter(Mandatory = $true)][string]$Body,
        [Parameter(Mandatory = $true)][ValidateSet('issue', 'pull-request')][string]$Kind,
        [Parameter(Mandatory = $true)][int]$Number,
        [Parameter(Mandatory = $true)][string]$Repository
    )

    $repo = [regex]::Escape($Repository)
    $label = if ($Kind -eq 'issue') {
        'Same-API issue comparison'
    } else {
        'Same-API comparison'
    }
    $labelPattern = [regex]::Escape($label)
    $pattern = "(?m)^[ `t]*${labelPattern}:[ `t]*$repo#$Number[ `t]*\|[ `t]*Different mechanism:[ `t]*(?<basis>[^|`r`n]{12,500}?)[ `t]*$"
    $matches = [regex]::Matches($Body, $pattern)
    if ($matches.Count -ne 1) {
        throw "Final leak-hunter de-dup gate requires exactly one structured $Kind override for same-API $Repository#${Number}: '$label`: $Repository#$Number | Different mechanism: <specific comparison basis>'."
    }
}

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

$mergedReverts = @(
    Invoke-LeakGhJson -Arguments @(
        'pr', 'list',
        '--repo', $Repository,
        '--state', 'merged',
        '--limit', '1000',
        '--search', '"Revert" in:title',
        '--json', 'number,title,body,baseRefName,mergedAt'
    )
)
if ($mergedReverts.Count -ge 1000) {
    throw "Merged Revert search returned $($mergedReverts.Count) rows at the GitHub Search API ceiling; refusing a potentially truncated final gate."
}

$eligibleMerged = @($merged | Where-Object {
        $null -ne $_.mergedAt -and
        ([string]$_.title).StartsWith('[leak-fix] ', [StringComparison]::Ordinal) -and
        [string]$_.baseRefName -in @('main', 'inflight/current')
    })
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

foreach ($requested in $requestedItems) {
    $api = [string]$requested.Api
    $body = [string]$requested.Item.body
    $openApiMatches = @($openIssues | Where-Object {
            $issueTitle = [string]$_.title
            $issueTitle.StartsWith('[leak-scan] ', [StringComparison]::Ordinal) -and
            (Get-CanonicalLeakApi -Title $issueTitle) -ceq $api
        })
    foreach ($match in $openApiMatches) {
        Assert-SameApiDisclosure `
            -Body $body `
            -Kind issue `
            -Number ([int]$match.number) `
            -Repository $Repository
    }

    $mergedApiMatches = @($eligibleMerged | Where-Object {
            (Get-CanonicalLeakApi -Title ([string]$_.title)) -ceq $api
        })
    foreach ($match in $mergedApiMatches) {
        Assert-SameApiDisclosure `
            -Body $body `
            -Kind pull-request `
            -Number ([int]$match.number) `
            -Repository $Repository
    }
}

$apis = @($requestedItems | ForEach-Object { $_.Api } | Sort-Object -Unique)
Write-Host "Final leak-hunter de-dup gate passed for APIs: $($apis -join ', ')."
