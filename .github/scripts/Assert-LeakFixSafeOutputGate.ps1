#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [string]$AgentOutputPath = $env:GH_AW_AGENT_OUTPUT,
    [string]$StateDirectory = $env:LEAK_DEDUP_STATE_DIR,
    [string]$Repository = $env:GITHUB_REPOSITORY
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1') -Force

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
$title = [string]$item.title
if (-not $title.StartsWith('[leak-fix] ', [StringComparison]::Ordinal)) {
    throw "The create-pull-request title must start with the literal '[leak-fix] ' prefix."
}

$api = Get-CanonicalLeakApi -Title $title
if ([string]::IsNullOrWhiteSpace($api)) {
    throw "Could not derive a canonical Type.Member from create-pull-request title '$title'."
}

$fixMatches = [regex]::Matches(([string]$item.body), '(?m)^[ \t]*Fixes #(?<number>[1-9][0-9]*)\b')
if ($fixMatches.Count -ne 1) {
    throw 'The PR body must contain exactly one canonical Fixes line.'
}

$issueNumber = [int]$fixMatches[0].Groups['number'].Value
$repo = [regex]::Escape($Repository)
$issue = [regex]::Escape([string]$issueNumber)
$targetRefsMatches = [regex]::Matches(
    ([string]$item.body),
    "(?m)^[ \t]*Refs:[ \t]*$repo#$issue\b"
)
if ($targetRefsMatches.Count -ne 1) {
    throw "The PR body must contain exactly one exact-repository Refs line for issue #$issueNumber."
}

$statePath = Join-Path $StateDirectory 'dedup-state.json'
$state = Read-RegularJsonFile -Path $statePath
Assert-LeakDedupState `
    -State $state `
    -IssueNumber $issueNumber `
    -Api $api `
    -Repository $Repository

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

$eligibleMerged = @($merged | Where-Object {
        $null -ne $_.mergedAt -and
        ([string]$_.title).StartsWith('[leak-fix] ', [StringComparison]::Ordinal) -and
        [string]$_.baseRefName -in @('main', 'inflight/current')
    })
$mergedReverts = @(
    Get-RelevantMergedLeakReverts `
        -Repository $Repository `
        -TargetPullRequests $eligibleMerged
)

$open = @(
    Invoke-LeakGhJson -Arguments @(
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

$closed = @(
    Invoke-LeakGhJson -Arguments @(
        'pr', 'list',
        '--repo', $Repository,
        '--state', 'closed',
        '--limit', '1000',
        '--search', '"[leak-fix]" in:title',
        '--json', 'number,title,body,mergedAt'
    )
)
if ($closed.Count -ge 1000) {
    throw "Closed [leak-fix] search returned $($closed.Count) rows at the GitHub Search API ceiling; refusing a potentially truncated final gate."
}
$closedAttempts = @($closed | Where-Object {
        $referencesIssue = Test-LeakPrReferencesIssue `
            -Body ([string]$_.body) `
            -IssueNumber $issueNumber `
            -Repository $Repository
        $null -eq $_.mergedAt -and
        ([string]$_.title).StartsWith('[leak-fix] ', [StringComparison]::Ordinal) -and
        ($referencesIssue -or
            (Get-CanonicalLeakApi -Title ([string]$_.title)) -ceq $api)
    } | Sort-Object number -Unique)
if ($closedAttempts.Count -ge 3) {
    throw "Final leak-fix attempt-cap gate blocked PR creation: $($closedAttempts.Count) closed-unmerged attempts already reference issue #$issueNumber or canonical API '$api'."
}

$result = Get-LeakFixFinalDedupResult `
    -IssueNumber $issueNumber `
    -Api $api `
    -Repository $Repository `
    -MergedPullRequests $merged `
    -OpenPullRequests $open `
    -MergedRevertPullRequests $mergedReverts

if ($result.Blocked) {
    throw "Final leak-fix de-dup gate blocked PR creation: $($result.Reason)."
}

Write-Host "Final leak-fix de-dup gate passed for issue #$issueNumber ($api): $($result.Reason)."
