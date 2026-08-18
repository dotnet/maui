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

function Invoke-GhJson {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)

    $output = & gh @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "'gh $($Arguments -join ' ')' failed with exit code $LASTEXITCODE`: $output"
    }
    $raw = ($output -join [Environment]::NewLine)
    if ([string]::IsNullOrWhiteSpace($raw)) {
        throw "'gh $($Arguments -join ' ')' returned an empty response."
    }
    try {
        return $raw | ConvertFrom-Json
    } catch {
        throw "'gh $($Arguments -join ' ')' returned invalid JSON: $($_.Exception.Message)"
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

$requestedApis = [System.Collections.Generic.Dictionary[string, string]]::new(
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
    if ($requestedApis.ContainsKey($api)) {
        throw "Multiple create-issue outputs target the same canonical API '$api'."
    }
    $requestedApis.Add($api, $title)
}

$openIssues = @(
    Invoke-GhJson -Arguments @(
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
    Invoke-GhJson -Arguments @(
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
    Invoke-GhJson -Arguments @(
        'pr', 'list',
        '--repo', $Repository,
        '--state', 'merged',
        '--limit', '1000',
        '--search', '"Revert" in:title',
        '--json', 'number,title,body,mergedAt'
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
        -FixPullRequestNumbers @($eligibleMerged | ForEach-Object { [int]$_.number }) `
        -MergedRevertPullRequests $mergedReverts
)
$reverted = [System.Collections.Generic.HashSet[int]]::new()
foreach ($number in $effectivelyReverted) {
    [void]$reverted.Add($number)
}
$eligibleMerged = @($eligibleMerged | Where-Object {
        -not $reverted.Contains([int]$_.number)
    })

$openApiMatches = @($openIssues | Where-Object {
        $title = [string]$_.title
        $api = Get-CanonicalLeakApi -Title $title
        $title.StartsWith('[leak-scan] ', [StringComparison]::Ordinal) -and
        -not [string]::IsNullOrWhiteSpace($api) -and
        $requestedApis.ContainsKey($api)
    })
$mergedApiMatches = @($eligibleMerged | Where-Object {
        $api = Get-CanonicalLeakApi -Title ([string]$_.title)
        -not [string]::IsNullOrWhiteSpace($api) -and
        $requestedApis.ContainsKey($api)
    })

if ($openApiMatches.Count -gt 0 -or $mergedApiMatches.Count -gt 0) {
    $reasons = @()
    if ($openApiMatches.Count -gt 0) {
        $reasons += "open [leak-scan] issue match: $($openApiMatches.number -join ', ')"
    }
    if ($mergedApiMatches.Count -gt 0) {
        $reasons += "active merged [leak-fix] PR match: $($mergedApiMatches.number -join ', ')"
    }
    throw "Final leak-hunter de-dup gate blocked issue creation: $($reasons -join '; ')."
}

Write-Host "Final leak-hunter de-dup gate passed for APIs: $($requestedApis.Keys -join ', ')."
