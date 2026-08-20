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
$requestedApis = [System.Collections.Generic.List[string]]::new()
foreach ($item in $createItems) {
    $title = [string]$item.title
    if (-not (Test-LeakTitlePrefix -Title $title -Kind Scan)) {
        throw "Every create-issue title must start with '[leak-scan]' followed by a space or tab."
    }
    $api = Get-CanonicalLeakApi -Title $title
    if ([string]::IsNullOrWhiteSpace($api)) {
        throw "Could not derive an anchored API identity from create-issue title '$title'."
    }
    if (-not $requestedTitles.Add($title)) {
        throw "Multiple create-issue outputs use the same exact title '$title'."
    }
    $overlappingApi = @($requestedApis | Where-Object {
            Test-LeakApiIdentityMatch -Left $_ -Right $api
        })
    if ($overlappingApi.Count -gt 0) {
        throw "Multiple create-issue outputs use overlapping API identities '$($overlappingApi[0])' and '$api'. Emit at most one issue per exact or conservatively ambiguous API identity in each safe-output batch."
    }
    $requestedApis.Add($api)
    $requestedItems.Add([pscustomobject]@{
            Api = $api
            Item = $item
        })
}

$maximumConsistencyAttempts = 3
$consistentSnapshot = $null
$consistentIssueState = $null
$mergedReverts = $null

# Read every lifecycle state in one complete connection so a same-API OPEN -> MERGED
# transition cannot disappear between state-filtered queries. Relevant open issue state
# brackets the pull-request and revert analysis; any relevant change discards the whole
# attempt, while unrelated shared-label issue churn does not perturb the signature.
for ($attempt = 1; $attempt -le $maximumConsistencyAttempts; $attempt++) {
    $beforeIssues = @(
        Get-CompleteLeakIssues -Repository $Repository
    )
    $beforeIssueState = Get-LeakRelevantIssueConsistencyState `
        -Issues $beforeIssues `
        -RequestedApis ([string[]]@($requestedApis))

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
    $afterIssues = @(
        Get-CompleteLeakIssues -Repository $Repository
    )
    $afterIssueState = Get-LeakRelevantIssueConsistencyState `
        -Issues $afterIssues `
        -RequestedApis ([string[]]@($requestedApis))

    $beforeSignature = Get-LeakPullRequestConsistencySignature `
        -PullRequests $before `
        -Repository $Repository `
        -RelevantRevertTargetNumbers $relevantRevertTargetNumbers
    $afterSignature = Get-LeakPullRequestConsistencySignature `
        -PullRequests $after `
        -Repository $Repository `
        -RelevantRevertTargetNumbers $relevantRevertTargetNumbers
    if ($beforeSignature -ceq $afterSignature -and
        $beforeIssueState.Signature -ceq $afterIssueState.Signature) {
        $consistentSnapshot = $after
        $consistentIssueState = $afterIssueState
        $mergedReverts = $candidateMergedReverts
        break
    }

    if ($attempt -lt $maximumConsistencyAttempts) {
        Write-Warning "Final leak-hunter live pull-request or relevant issue state changed during consistency attempt $attempt of $maximumConsistencyAttempts; rebuilding the coherent snapshot and revert analysis."
    }
}

if ($null -eq $consistentSnapshot -or $null -eq $consistentIssueState) {
    throw "Final leak-hunter live pull-request or relevant issue state remained inconsistent after $maximumConsistencyAttempts bounded attempts."
}

$openIssueApis = @($consistentIssueState.Issues)
$merged = @($consistentSnapshot | Where-Object { $_.state -ceq 'MERGED' })
$open = @($consistentSnapshot | Where-Object { $_.state -ceq 'OPEN' })
$authoritativeMerged = @(
    Select-LeakAuthoritativePullRequests `
        -PullRequests $merged `
        -Context 'Final merged leak-fix de-dup search'
)
$eligibleMerged = @($authoritativeMerged | Where-Object {
        $null -ne $_.mergedAt -and
        (Test-LeakTitlePrefix -Title ([string]$_.title) -Kind Fix)
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
$authoritativeOpen = @(
    Select-LeakAuthoritativePullRequests `
        -PullRequests $open `
        -Context 'Final open leak-fix de-dup search'
)
$openFixApis = @(
    foreach ($pullRequest in $authoritativeOpen) {
        $number = [int]$pullRequest.number
        $identity = Get-LeakFixPullRequestTitleIdentity `
            -PullRequest $pullRequest `
            -Context "Open pull request #$number"
        if ($null -ne $identity) {
            [pscustomobject]@{
                Api = [string]$identity.Api
                Number = $number
            }
        }
    }
)

# Validation is intentionally batch-atomic. Do not rewrite agent output at this trusted
# boundary: any stale item aborts before Process Safe Outputs, and distinct items retry next run.
foreach ($requested in $requestedItems) {
    $api = [string]$requested.Api
    $openApiMatches = @($openIssueApis | Where-Object {
            Test-LeakApiIdentityMatch -Left ([string]$_.Api) -Right $api
        })
    if ($openApiMatches.Count -gt 0) {
        throw "Final leak-hunter de-dup gate blocked issue creation for '$api': same-API open issue match $($openApiMatches.Number -join ', '). The safe-output batch is rejected atomically; other items can retry on the next scheduled run."
    }

    $mergedApiMatches = @($eligibleMerged | Where-Object {
            $existingApi = Get-CanonicalExistingLeakApi `
                -Title ([string]$_.title)
            Test-LeakApiIdentityMatch -Left $existingApi -Right $api
        })
    if ($mergedApiMatches.Count -gt 0) {
        throw "Final leak-hunter de-dup gate blocked issue creation for '$api': same-API merged fix match $($mergedApiMatches.number -join ', '). The safe-output batch is rejected atomically; other items can retry on the next scheduled run."
    }

    $openFixApiMatches = @($openFixApis | Where-Object {
            Test-LeakApiIdentityMatch -Left ([string]$_.Api) -Right $api
        })
    if ($openFixApiMatches.Count -gt 0) {
        throw "Final leak-hunter de-dup gate blocked issue creation for '$api': same-API open fix match $($openFixApiMatches.Number -join ', '). The safe-output batch is rejected atomically; other items can retry on the next scheduled run."
    }
}

$apis = @($requestedItems | ForEach-Object { $_.Api } | Sort-Object)
Write-Host "Final leak-hunter de-dup gate passed for APIs: $($apis -join ', ')."
