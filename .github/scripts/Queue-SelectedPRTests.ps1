#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ContextDirectory,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$AgentOutputPath,
    [ValidateRange(1, 2147483647)][int]$PrNumber,
    [string]$Actor = $env:GITHUB_ACTOR,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. "$PSScriptRoot/Queue-CiFixAzdoValidation.ps1" -LibraryOnly -DispatcherBudgetSeconds 420 -DryRun:$DryRun
$script:selectionSummaryPosted = $false
$script:selectionAuthorized = $false
trap {
    if (-not $DryRun -and $script:selectionAuthorized -and -not $script:selectionSummaryPosted -and $env:GITHUB_REPOSITORY -ceq 'dotnet/maui') {
        $notice = "<!-- review-run-tests-failed -->`nSelected-test dispatch could not complete. Evidence may be stale, authorization may have changed, or an API request failed. Inspect [the workflow logs](https://github.com/dotnet/maui/actions/runs/$($env:GITHUB_RUN_ID)) before retrying; do not assume any unreported run was queued or passed."
        & gh api -X POST "repos/dotnet/maui/issues/$PrNumber/comments" -f "body=$notice" | Out-Null
        if ($LASTEXITCODE -ne 0) { Write-Warning 'The dispatch failure notice could not be published.' }
    }
    throw
}

function Get-SelectedPr {
    $raw = & gh api "repos/dotnet/maui/pulls/$PrNumber"
    if ($LASTEXITCODE -ne 0) { throw 'Could not revalidate PR identity.' }
    return ($raw -join "`n") | ConvertFrom-Json
}

function Assert-SelectedIdentity {
    param($Selection)
    $current = Get-SelectedPr
    if ($current.state -ne 'open' -or $current.head.sha -cne $Selection.headSha -or
        $current.base.sha -cne $Selection.baseSha -or $current.merge_commit_sha -cne $Selection.mergeSha) {
        throw 'PR head, base, or test-merge changed; request /review run-tests again.'
    }
    $merge = (& gh api "repos/dotnet/maui/git/commits/$($Selection.mergeSha)" | Out-String) | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0 -or $merge.parents.Count -ne 2 -or
        $merge.parents[0].sha -cne $Selection.baseSha -or $merge.parents[1].sha -cne $Selection.headSha) {
        throw 'The selected revision is not the verified PR test-merge.'
    }
    return $current
}

function Find-SelectedRun {
    param([int]$DefinitionId, [string]$Fingerprint, [string]$MergeSha, [string]$Token)
    $branch = [uri]::EscapeDataString("refs/pull/$PrNumber/merge")
    $response = Invoke-WithHttpRetry -OperationName 'Find equivalent selected run' -Operation {
        param($timeoutSeconds)
        Invoke-RestMethod -Uri "https://dev.azure.com/dnceng-public/public/_apis/build/builds?definitions=$DefinitionId&branchName=$branch&queryOrder=queueTimeDescending&`$top=50&api-version=7.1" `
            -Headers @{ Authorization = "Bearer $Token" } -TimeoutSec $timeoutSeconds
    }
    foreach ($build in $response.value) {
        $key = Get-ObjectPropertyValue $build.triggerInfo 'review.selection'
        if ($build.sourceVersion -ceq $MergeSha -and $key -ceq $Fingerprint -and
            ($build.status -in @('notStarted', 'inProgress') -or $build.result -eq 'succeeded')) { return $build }
    }
    return $null
}

if (-not $DryRun) {
    if ($env:GITHUB_REPOSITORY -cne 'dotnet/maui' -or $env:GITHUB_REF -cne 'refs/heads/main') {
        throw 'Credentialed dispatch must run from trusted dotnet/maui main.'
    }
    $permission = (& gh api "repos/dotnet/maui/collaborators/$Actor/permission" | Out-String) | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0 -or $permission.permission -notin @('write', 'maintain', 'admin')) {
        throw 'The caller is no longer authorized to queue tests.'
    }
    $script:selectionAuthorized = $true
}
& "$PSScriptRoot/Select-PRTests.ps1" -Stage Validate -ContextDirectory $ContextDirectory `
    -OutputDirectory $OutputDirectory -AgentOutputPath $AgentOutputPath
$selection = Get-Content "$OutputDirectory/selection.json" -Raw | ConvertFrom-Json
if ($selection.prNumber -ne $PrNumber) { throw 'Selection does not belong to the triggering PR.' }
$pr = Assert-SelectedIdentity $selection
$context = [pscustomobject]@{
    PullRequestNumber = $PrNumber; PullRequestId = $pr.id; HeadRef = $pr.head.ref
    BaseRef = $pr.base.ref; HeadSha = $pr.head.sha; MergeSha = $selection.mergeSha
    Title = $pr.title; Draft = $pr.draft
}
$pipelines = @([pscustomobject]@{ id = 302; name = 'maui-pr'; parameters = [ordered]@{} })
if ($selection.runUi) {
    $pipelines += [pscustomobject]@{
        id = 313; name = 'maui-pr-uitests'
        parameters = [ordered]@{
            selectedPlatforms = @($selection.platforms | ForEach-Object { if ($_ -eq 'maccatalyst') { 'catalyst' } else { $_ } })
            selectedCategories = @($selection.uiCategories)
        }
    }
}
if ($selection.runDevice) {
    $pipelines += [pscustomobject]@{
        id = 314; name = 'maui-pr-devicetests'
        parameters = [ordered]@{
            selectedPlatforms = @($selection.platforms)
            selectedProjects = @($selection.deviceProjects)
            selectedCategories = @($selection.deviceCategories)
        }
    }
}
$requests = @()
foreach ($pipeline in $pipelines) {
    $request = New-AzdoQueueRequest -DefinitionId $pipeline.id -Context $context
    $prParameters = $request.parameters | ConvertFrom-Json -AsHashtable
    $prParameters['system.pullRequest.isFork'] = "$($pr.head.repo.full_name -cne 'dotnet/maui')"
    $prParameters['system.pullRequest.sourceRepositoryUri'] = $pr.head.repo.clone_url
    $request.parameters = $prParameters | ConvertTo-Json -Compress
    $request.triggerInfo['pr.isFork'] = "$($pr.head.repo.full_name -cne 'dotnet/maui')"
    $request.templateParameters = $pipeline.parameters
    $fingerprintInput = "$($selection.mergeSha):$($pipeline.id):" + ($pipeline.parameters | ConvertTo-Json -Compress -Depth 8)
    $fingerprint = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($fingerprintInput))).ToLowerInvariant()
    $request.triggerInfo['review.selection'] = $fingerprint
    $requests += @{ pipeline = $pipeline.name; id = $pipeline.id; fingerprint = $fingerprint; request = $request }
}
$requests | ConvertTo-Json -Depth 20 | Set-Content "$OutputDirectory/queue-requests.json" -Encoding utf8
if ($DryRun) {
    Write-Output "Dry run: validated selections and $($requests.Count) queue payloads; nothing queued or posted."
    return
}
$token = Get-AzdoToken
$results = @()
foreach ($entry in $requests) {
    try {
        [void](Assert-SelectedIdentity $selection)
        $build = Find-SelectedRun $entry.id $entry.fingerprint $selection.mergeSha $token
        $reused = $null -ne $build
        if (-not $build) {
            # Queue POST is not blindly retried: an uncertain response can already have created a run.
            try {
                $build = Invoke-RestMethod -Method Post -Uri 'https://dev.azure.com/dnceng-public/public/_apis/build/builds?api-version=7.1' `
                    -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json' `
                    -Body ($entry.request | ConvertTo-Json -Depth 20 -Compress) `
                    -TimeoutSec (Get-DispatcherHttpTimeoutSeconds -OperationName "Queue $($entry.pipeline)")
            } catch {
                $queueError = $_
                $build = Find-SelectedRun $entry.id $entry.fingerprint $selection.mergeSha $token
                if (-not $build) { throw $queueError }
            }
        }
        if ($build.sourceVersion -cne $selection.mergeSha -or -not $build._links.web.href) {
            throw 'Azure DevOps did not confirm the requested merge revision and run URL.'
        }
        $results += @{ pipeline = $entry.pipeline; status = $(if ($reused) { 'reused' } else { 'queued' }); url = $build._links.web.href }
    } catch {
        $results += @{ pipeline = $entry.pipeline; status = 'failed'; detail = 'Dispatch failed; see the workflow log for the API diagnostic.' }
        Write-Warning (Get-AzdoQueueFailureMessage -ErrorRecord $_)
    }
}
$token = $null
$results | ConvertTo-Json -Depth 8 | Set-Content "$OutputDirectory/queue-results.json" -Encoding utf8
$lines = @(
    '<!-- review-run-tests -->',
    '### Selected CI tests',
    '',
    "Head: ``$($selection.headSha)`` · Base: ``$($selection.baseSha)`` · Test merge: ``$($selection.mergeSha)``",
    '',
    "Platforms: $($selection.platforms -join ', ').",
    "UI: $(if ($selection.runUi) { if ($selection.uiCategories.Count) { $selection.uiCategories -join ', ' } else { 'all categories on selected platforms' } } else { 'not selected' }).",
    "Device: $(if ($selection.runDevice) { ($selection.deviceProjects -join ', ') + ' / ' + $(if ($selection.deviceCategories.Count) { $selection.deviceCategories -join ', ' } else { 'all categories' }) } else { 'not selected' }).",
    'maui-pr build/unit/integration validation is unchanged.',
    'Applicable runtime/handler variants and uncategorized device tests are retained; unsupported category runners keep full selected-project coverage.',
    ''
)
foreach ($result in $results) {
    if ($result.status -eq 'failed') { $lines += "- $($result.pipeline): **dispatch failed**; inspect this workflow's logs." }
    else { $lines += "- [$($result.pipeline)]($($result.url)): $($result.status)." }
}
$lines += @('', 'Selected runs are not full-suite certification; queued does not mean passed.')
if ($selection.reasons.Count) {
    $lines += @('', 'Coverage notes:')
    foreach ($reason in $selection.reasons) {
        $safeReason = ([string]$reason -replace '[\r\n<>`@]', ' ' -replace '##(?=\[|vso\[)', '## ')
        $lines += "- ``$safeReason``"
    }
}
$lines += @('', "Generated by [review run-tests](https://github.com/dotnet/maui/actions/runs/$($env:GITHUB_RUN_ID)).")
$bodyPath = Join-Path $OutputDirectory 'report.md'
$lines -join "`n" | Set-Content $bodyPath -Encoding utf8
& gh api -X POST "repos/dotnet/maui/issues/$PrNumber/comments" -f "body=$(Get-Content $bodyPath -Raw)" | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Runs were processed but the summary could not be published.' }
$script:selectionSummaryPosted = $true
if (@($results | Where-Object { $_.status -eq 'failed' }).Count) { throw 'One or more pipelines failed to dispatch; partial results were reported.' }
