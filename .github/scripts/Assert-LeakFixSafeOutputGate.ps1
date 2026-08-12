#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [string]$AgentOutputPath = $env:GH_AW_AGENT_OUTPUT,
    [string]$StateDirectory = $env:LEAK_DEDUP_STATE_DIR,
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
if ([string]::IsNullOrWhiteSpace($StateDirectory)) {
    throw 'LEAK_DEDUP_STATE_DIR is required.'
}

$agentOutput = Read-RegularJsonFile -Path $AgentOutputPath
$createItems = @($agentOutput.items | Where-Object { $_.type -eq 'create_pull_request' })
if ($createItems.Count -eq 0) {
    Write-Host 'No create_pull_request safe-output requested; final leak de-dup gate is not applicable.'
    return
}
if ($createItems.Count -ne 1) {
    throw "Expected exactly one create_pull_request item, found $($createItems.Count)."
}

$item = $createItems[0]
$api = Get-CanonicalLeakApi -Title ([string]$item.title)
if ([string]::IsNullOrWhiteSpace($api)) {
    throw "Could not derive a canonical Type.Member from create-pull-request title '$($item.title)'."
}

$fixMatches = [regex]::Matches(([string]$item.body), '(?m)^[ \t]*Fixes #(?<number>[1-9][0-9]*)\b')
$repo = [regex]::Escape($Repository)
$refsMatches = [regex]::Matches(
    ([string]$item.body),
    "(?m)^[ \t]*Refs:[ \t]*$repo#(?<number>[1-9][0-9]*)\b"
)
if ($fixMatches.Count -ne 1 -or $refsMatches.Count -ne 1) {
    throw 'The PR body must contain exactly one canonical Fixes line and one exact-repository Refs line.'
}
$issueNumber = [int]$fixMatches[0].Groups['number'].Value
if ([int]$refsMatches[0].Groups['number'].Value -ne $issueNumber) {
    throw 'The PR body Fixes and Refs lines identify different issues.'
}

$statePath = Join-Path $StateDirectory 'dedup-state.json'
$state = Read-RegularJsonFile -Path $statePath
$approved = @(
    Assert-LeakDedupState `
        -State $state `
        -IssueNumber $issueNumber `
        -Api $api `
        -Repository $Repository
)

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

$open = @(
    Invoke-GhJson -Arguments @(
        'pr', 'list',
        '--repo', $Repository,
        '--state', 'open',
        '--limit', '1000',
        '--search', '"[leak-fix]" in:title',
        '--json', 'number,title,body,baseRefName,mergedAt,url'
    )
)
if ($open.Count -ge 1000) {
    throw "Open [leak-fix] search returned $($open.Count) rows at the GitHub Search API ceiling; refusing a potentially truncated final gate."
}

$result = Get-LeakFixFinalDedupResult `
    -IssueNumber $issueNumber `
    -Api $api `
    -Repository $Repository `
    -MergedPullRequests $merged `
    -OpenPullRequests $open `
    -MergedRevertPullRequests $mergedReverts `
    -ApprovedDifferentMechanismPullRequests $approved

if ($result.Blocked) {
    throw "Final leak-fix de-dup gate blocked PR creation: $($result.Reason)."
}

Write-Host "Final leak-fix de-dup gate passed for issue #$issueNumber ($api): $($result.Reason)."
