#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [string]$AgentOutputPath = $env:GH_AW_AGENT_OUTPUT,
    [string]$StateDirectory = $env:LEAK_DEDUP_STATE_DIR,
    [string]$Repository = $env:GITHUB_REPOSITORY
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1') -Force

function Get-LeakFixConsistencySignature {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$PullRequests,
        [Parameter(Mandatory = $true)][string]$Repository,
        [AllowEmptyCollection()][int[]]$RelevantRevertTargetNumbers = @()
    )

    $relevantRevertTargets = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($number in $RelevantRevertTargetNumbers) {
        if ($number -le 0) {
            throw "Consistency signature received invalid revert target PR #$number."
        }
        [void]$relevantRevertTargets.Add($number)
    }

    # Preserve immutable identity for every merged PR, but include editable title/body
    # only when the record can affect leak-fix or relevant revert de-duplication.
    $relevant = @(
        $PullRequests |
            Where-Object {
                [string]$_.state -ceq 'MERGED' -or
                (Test-LeakTitlePrefix -Title ([string]$_.title) -Kind Fix)
            } |
            Sort-Object number |
            ForEach-Object {
                $includeMutableMetadata =
                    Test-LeakTitlePrefix -Title ([string]$_.title) -Kind Fix
                if (-not $includeMutableMetadata -and
                    [string]$_.state -ceq 'MERGED' -and
                    $relevantRevertTargets.Count -gt 0) {
                    foreach ($targetNumber in @(
                            Get-LeakRevertTargets `
                                -Body ([string]$_.body) `
                                -Repository $Repository
                        )) {
                        if ($relevantRevertTargets.Contains($targetNumber)) {
                            $includeMutableMetadata = $true
                            break
                        }
                    }
                }

                $record = [ordered]@{
                    number = [int]$_.number
                    baseRefName = [string]$_.baseRefName
                    state = [string]$_.state
                    merged = [bool]$_.merged
                    mergedAt = if ($null -eq $_.mergedAt) {
                        $null
                    } else {
                        [string]$_.mergedAt
                    }
                    mergeCommitOid = if ($null -eq $_.mergeCommitOid) {
                        $null
                    } else {
                        [string]$_.mergeCommitOid
                    }
                    url = [string]$_.url
                }
                if ($includeMutableMetadata) {
                    $record.title = [string]$_.title
                    $record.body = if ($null -eq $_.body) {
                        $null
                    } else {
                        [string]$_.body
                    }
                }
                $record
            }
    )

    return ConvertTo-Json -InputObject $relevant -Depth 4 -Compress
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

$maximumConsistencyAttempts = 3
$consistentSnapshot = $null
$mergedReverts = $null

# Read every lifecycle state in one complete connection so an OPEN -> MERGED or
# OPEN -> CLOSED transition cannot disappear between state-filtered queries.
# Revert analysis is bracketed by an identical final read; any relevant change
# discards the whole attempt and rebuilds from live data.
for ($attempt = 1; $attempt -le $maximumConsistencyAttempts; $attempt++) {
    $before = @(
        Get-CompleteLeakPullRequests `
            -Repository $Repository `
            -States @('OPEN', 'CLOSED', 'MERGED')
    )
    $beforeMerged = @($before | Where-Object { $_.state -ceq 'MERGED' })
    $authoritativeMerged = @(
        Select-LeakAuthoritativePullRequests `
            -PullRequests $beforeMerged `
            -Context 'Final merged leak-fix de-dup search'
    )
    $eligibleMerged = @($authoritativeMerged | Where-Object {
            $null -ne $_.mergedAt -and
            (Test-LeakTitlePrefix -Title ([string]$_.title) -Kind Fix)
        })
    $candidateMergedReverts = @(
        Get-RelevantMergedLeakReverts `
            -Repository $Repository `
            -TargetPullRequests $eligibleMerged `
            -MergedPullRequests $beforeMerged
    )
    $relevantRevertTargetNumbers = @(
        @($eligibleMerged | ForEach-Object { [int]$_.number }) +
        @($candidateMergedReverts | ForEach-Object { [int]$_.number }) |
            Sort-Object -Unique
    )

    $after = @(
        Get-CompleteLeakPullRequests `
            -Repository $Repository `
            -States @('OPEN', 'CLOSED', 'MERGED')
    )
    $beforeSignature = Get-LeakFixConsistencySignature -PullRequests $before `
        -Repository $Repository `
        -RelevantRevertTargetNumbers $relevantRevertTargetNumbers
    $afterSignature = Get-LeakFixConsistencySignature -PullRequests $after `
        -Repository $Repository `
        -RelevantRevertTargetNumbers $relevantRevertTargetNumbers
    if ($beforeSignature -ceq $afterSignature) {
        $consistentSnapshot = $after
        $mergedReverts = $candidateMergedReverts
        break
    }

    if ($attempt -lt $maximumConsistencyAttempts) {
        Write-Warning "Final leak-fix live pull-request state changed during consistency attempt $attempt of $maximumConsistencyAttempts; rebuilding the snapshot and revert analysis."
    }
}

if ($null -eq $consistentSnapshot) {
    throw "Final leak-fix live pull-request state remained inconsistent after $maximumConsistencyAttempts bounded attempts."
}

$merged = @($consistentSnapshot | Where-Object { $_.state -ceq 'MERGED' })
$open = @($consistentSnapshot | Where-Object { $_.state -ceq 'OPEN' })
$closed = @($consistentSnapshot | Where-Object { $_.state -ceq 'CLOSED' })
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

# Keep this exact issue fetch last so its type, repository, OPEN state, canonical
# title, and scanner-owned labels are as fresh as possible before PR mutation.
$liveIssue = Get-ValidatedLeakScanIssue `
    -Repository $Repository `
    -IssueNumber $issueNumber `
    -Api $api

Write-Host "Final leak-fix de-dup gate passed for issue #$issueNumber ($api): $($result.Reason). Live issue $($liveIssue.url) validated."
