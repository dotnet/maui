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
if (-not (Test-LeakTitlePrefix -Title $title -Kind Fix)) {
    throw "The create-pull-request title must start with '[leak-fix]' followed by a space or tab."
}

$api = Get-CanonicalLeakApi -Title $title
if ([string]::IsNullOrWhiteSpace($api)) {
    throw "Could not derive a canonical Type.Member from create-pull-request title '$title'."
}

$issueNumber = Get-LeakFixProvenanceIssueNumber `
    -Body ([string]$item.body) `
    -Repository $Repository

$statePath = Join-Path $StateDirectory 'dedup-state.json'
$state = Read-RegularJsonFile -Path $statePath
Assert-LeakDedupState `
    -State $state `
    -IssueNumber $issueNumber `
    -Api $api `
    -Repository $Repository

$merged = @(
    Get-CompleteLeakPullRequests `
        -Repository $Repository `
        -State MERGED
)

$authoritativeMerged = @(
    Select-LeakAuthoritativePullRequests `
        -PullRequests $merged `
        -Context 'Final merged leak-fix de-dup search'
)
$eligibleMerged = @($authoritativeMerged | Where-Object {
        $null -ne $_.mergedAt -and
        (Test-LeakTitlePrefix -Title ([string]$_.title) -Kind Fix)
    })
$mergedReverts = @(
    Get-RelevantMergedLeakReverts `
        -Repository $Repository `
        -TargetPullRequests $eligibleMerged `
        -MergedPullRequests $merged
)

$open = @(
    Get-CompleteLeakPullRequests `
        -Repository $Repository `
        -State OPEN
)

$closed = @(
    Get-CompleteLeakPullRequests `
        -Repository $Repository `
        -State CLOSED
)
$authoritativeClosed = @(
    Select-LeakAuthoritativePullRequests `
        -PullRequests $closed `
        -Context 'Final closed leak-fix attempt-cap search'
)
# The cap is one aggregate budget across both authoritative lanes. A fix merged or attempted
# in main or inflight/current represents the same canonical leak work; release lanes do not.
$closedAttempts = @($authoritativeClosed | Where-Object {
        $referencesIssue = Test-LeakPrReferencesIssue `
            -Body ([string]$_.body) `
            -IssueNumber $issueNumber `
            -Repository $Repository
        $null -eq $_.mergedAt -and
        (Test-LeakTitlePrefix -Title ([string]$_.title) -Kind Fix) -and
        ($referencesIssue -or
            (Get-CanonicalExistingLeakApi -Title ([string]$_.title)) -ceq $api)
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
