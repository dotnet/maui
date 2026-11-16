#!/usr/bin/env pwsh
<#
.SYNOPSIS
Trusted half of `/review run-tests`: validates the AI selection, queues maui-pr-uitests,
publishes "skipped" check runs and posts the PR comment.

.DESCRIPTION
Runs in an isolated safe-output job (never in the agent's sandbox). The agent's output
is only a proposal: platforms and categories are validated against the inventory from
the trusted checkout, the caller's permission and the PR head are re-checked, and only
then is the Azure DevOps build queued.

maui-pr-uitests is queued through the Build API as a pull-request build
(reason=pullRequest + triggerInfo.pr.sourceSha), so Azure Pipelines reports the run and
each job as native checks on the PR head commit. Anything not selected is published as a
GitHub check run with conclusion "skipped", so the checks list shows both what ran and
what was skipped.

.PARAMETER AzdoToken
Optional bearer token for local testing. In the workflow the token comes from GitHub OIDC
federated with the Azure DevOps service identity (see .github/docs/trigger-azdo-pipeline-setup.md).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateRange(1, 2147483647)][int]$PrNumber,
    [Parameter(Mandatory)][string]$ContextPath,
    [string]$AgentOutputPath,
    [string]$ProposalJson,
    [string]$OutputDirectory = $(if ($env:RUNNER_TEMP) { Join-Path $env:RUNNER_TEMP 'run-tests' } else { Join-Path ([IO.Path]::GetTempPath()) 'run-tests' }),
    [string]$Repository = $(if ($env:GITHUB_REPOSITORY) { $env:GITHUB_REPOSITORY } else { 'dotnet/maui' }),
    [string]$Actor = $env:GITHUB_ACTOR,
    [string]$PipelineYamlPath = "$PSScriptRoot/../../eng/pipelines/common/ui-tests.yml",
    [string]$AzdoToken = $env:RUN_TESTS_AZDO_TOKEN,
    [int]$DefinitionId = 313,
    # Do not queue, create checks or comment; write the planned outputs to OutputDirectory.
    [switch]$DryRun,
    # Queue and comment but do not create check runs (the Checks API needs a GitHub App token).
    [switch]$SkipChecks,
    # Do not queue Azure DevOps (e.g. a fork without the federated identity); still publish checks and comment.
    [switch]$SkipQueue
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/RunTests.Common.ps1"

$runUrl = if ($env:GITHUB_RUN_ID) { "$($env:GITHUB_SERVER_URL ?? 'https://github.com')/$Repository/actions/runs/$($env:GITHUB_RUN_ID)" } else { "https://github.com/$Repository" }
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

function Invoke-GhJson {
    param([string[]]$Arguments, [string]$InputJson)
    if ($InputJson) {
        $raw = $InputJson | & gh api @Arguments --input -
    } else {
        $raw = & gh api @Arguments
    }
    if ($LASTEXITCODE -ne 0) { throw "gh api $($Arguments -join ' ') failed." }
    if (-not $raw) { return $null }
    return ($raw -join "`n") | ConvertFrom-Json -Depth 50
}

function Get-Proposal {
    if ($ProposalJson) { return $ProposalJson | ConvertFrom-Json -AsHashtable }
    if (-not $AgentOutputPath -or -not (Test-Path -LiteralPath $AgentOutputPath)) {
        return $null
    }
    $output = Get-Content -LiteralPath $AgentOutputPath -Raw | ConvertFrom-Json -AsHashtable -Depth 20
    $items = @($output.items | Where-Object { $_.type -eq 'run_selected_ui_tests' })
    if ($items.Count -eq 0) { return $null }
    # The tool is documented as call-once; use the last call if the agent corrected itself.
    return $items[-1]
}

$context = Get-Content -LiteralPath $ContextPath -Raw | ConvertFrom-Json -Depth 30
if ([int]$context.prNumber -ne $PrNumber) { throw 'The collected context belongs to a different PR.' }
$inventory = Get-RunTestsInventory -PipelineYamlPath $PipelineYamlPath

$proposal = Get-Proposal
if (-not $proposal) {
    # The agent failed or produced nothing usable: fall back to the full suite rather than guessing.
    $proposal = @{
        full_run = $true; platforms = '[]'; categories = '[]'
        summary = 'The AI selection was unavailable, so the full UI test suite was selected.'
        rationale = 'No valid `run_selected_ui_tests` proposal was produced by the agent.'
    }
}
$selection = Resolve-RunTestsSelection -Proposal $proposal -Inventory $inventory
$selection | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $OutputDirectory 'selection.json') -Encoding utf8

# Re-check the caller and the PR: the agent may have taken minutes and the PR may have moved.
if (-not $DryRun -and $Actor) {
    $permission = Invoke-GhJson @("repos/$Repository/collaborators/$Actor/permission")
    if ($permission.permission -notin @('admin', 'maintain', 'write')) {
        throw "@$Actor no longer has write access; nothing was queued."
    }
}
$pr = Invoke-GhJson @("repos/$Repository/pulls/$PrNumber")
$queueError = $null
if ($pr.state -ne 'open') {
    $queueError = 'The PR is no longer open.'
} elseif ($pr.head.sha -cne $context.headSha) {
    $queueError = "The PR was updated while tests were being selected (selected for $($context.headSha.Substring(0, 8)), now $($pr.head.sha.Substring(0, 8))). Comment ``/review run-tests`` again."
    # Report against the commit the selection was made for.
    $pr.head.sha = $context.headSha
}

$build = $null
if (-not $queueError -and $selection.mode -ne 'none') {
    if ($pr.mergeable -eq $false -or -not $pr.merge_commit_sha) {
        $queueError = 'GitHub has no test-merge commit for this PR (it may have merge conflicts). Rebase or resolve conflicts, then retry.'
    } else {
        $request = New-RunTestsQueueRequest -PullRequest $pr -Selection $selection -DefinitionId $DefinitionId
        $request | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $OutputDirectory 'queue-request.json') -Encoding utf8
        if ($DryRun) {
            Write-Host 'Dry run: queue request written; nothing queued.'
        } elseif ($SkipQueue) {
            Write-Host 'Azure DevOps dispatch is not available in this repository; publishing the selection only.'
        } else {
            try {
                if (-not $AzdoToken) { $AzdoToken = Get-RunTestsAzdoToken }
                $headers = @{ Authorization = "Bearer $AzdoToken" }
                $base = 'https://dev.azure.com/dnceng-public/public/_apis/build/builds'
                $build = Invoke-RestMethod -Method Post -Uri "$($base)?api-version=7.1" -Headers $headers `
                    -ContentType 'application/json' -Body ($request | ConvertTo-Json -Depth 10 -Compress) -TimeoutSec 60
                if (-not (Test-RunTestsQueuedSelection -Build $build -Selection $selection)) {
                    # Never leave an unexpected full run behind a "targeted" comment.
                    Invoke-RestMethod -Method Patch -Uri "$base/$($build.id)?api-version=7.1" -Headers $headers `
                        -ContentType 'application/json' -Body '{"status":"cancelling"}' -TimeoutSec 30 | Out-Null
                    $build = $null
                    $queueError = 'Azure DevOps ignored the selection parameters (is the pipeline YAML on the PR merge ref up to date?); the run was cancelled.'
                } else {
                    Write-Host "Queued maui-pr-uitests build $($build.id): $($build._links.web.href)"
                }
            } catch {
                Write-Warning "Queueing maui-pr-uitests failed: $($_.Exception.Message)"
                $queueError = 'The Azure DevOps request failed; see the workflow log.'
                $build = $null
            } finally {
                $AzdoToken = $null
            }
        }
    }
}

# Check runs: everything that will not run is listed as Skipped, plus one summary check.
$checks = @()
if (-not $queueError) {
    $checks = New-RunTestsCheckPlan -Selection $selection -Inventory $inventory
}
$checks | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $OutputDirectory 'checks.json') -Encoding utf8

$platformText = if ($selection.platforms.Count) { ($selection.platforms | ForEach-Object { Get-RunTestsPlatformName $_ }) -join ', ' } else { 'none' }
$buildUrl = if ($build) { [string]$build._links.web.href } else { $runUrl }
$selectionSummary = @(
    "**Mode:** $($selection.mode)  ",
    "**Platforms:** $platformText  ",
    "**Categories:** $(if ($selection.mode -eq 'full') { 'all' } elseif ($selection.mode -eq 'none') { 'none' } else { Format-RunTestsCategoryList $selection.expandedCategories })",
    '',
    $selection.summary,
    '',
    $(if ($build) { "Azure DevOps run: [maui-pr-uitests #$($build.id)]($buildUrl)" } elseif ($queueError) { "Not queued: $queueError" } elseif ($SkipQueue) { 'Selection only: Azure DevOps dispatch is not available in this repository.' } else { '' })
) -join "`n"

if (-not $DryRun -and -not $SkipChecks) {
    $sha = $pr.head.sha
    # A later run on the same commit may select something an earlier run skipped: retire those.
    $existing = Invoke-GhJson @('--paginate', '--slurp', "repos/$Repository/commits/$sha/check-runs?per_page=100&filter=latest")
    $existingRuns = @($existing | ForEach-Object { $_.check_runs } | Where-Object {
        $_.app.slug -eq 'github-actions' -and $_.name.StartsWith($script:RunTestsSkippedCheckPrefix) })
    $planned = @($checks | ForEach-Object { $_.name })
    foreach ($stale in $existingRuns | Where-Object { $_.name -notin $planned }) {
        $body = @{ conclusion = 'neutral'; output = @{ title = 'Selected by a later /review run-tests'; summary = "Superseded by [a later selection]($runUrl)." } } | ConvertTo-Json -Depth 5
        Invoke-GhJson @('-X', 'PATCH', "repos/$Repository/check-runs/$($stale.id)") -InputJson $body | Out-Null
    }

    $now = (Get-Date).ToUniversalTime().ToString('o')
    foreach ($check in $checks) {
        $summary = "Not selected by ``/review run-tests`` for this commit.`n`n$selectionSummary"
        $body = [ordered]@{
            name = $check.name; head_sha = $sha; status = 'completed'; conclusion = 'skipped'
            started_at = $now; completed_at = $now; details_url = $buildUrl
            output = [ordered]@{ title = $check.title; summary = $summary }
        } | ConvertTo-Json -Depth 5
        Invoke-GhJson @('-X', 'POST', "repos/$Repository/check-runs") -InputJson $body | Out-Null
    }

    $selectionConclusion = if ($queueError) { 'failure' } else { 'success' }
    $title = if ($queueError) { 'UI tests were not queued' } elseif ($selection.mode -eq 'none') { 'No UI tests selected' } elseif ($selection.mode -eq 'full') { 'Full UI test suite selected' } else { "$($selection.expandedCategories.Count) categories on $platformText" }
    $body = [ordered]@{
        name = $script:RunTestsSelectionCheckName; head_sha = $sha; status = 'completed'; conclusion = $selectionConclusion
        started_at = $now; completed_at = $now; details_url = $buildUrl
        output = [ordered]@{ title = $title; summary = $selectionSummary; text = $selection.rationale }
    } | ConvertTo-Json -Depth 5
    Invoke-GhJson @('-X', 'POST', "repos/$Repository/check-runs") -InputJson $body | Out-Null
    Write-Host "Published $($checks.Count) skipped check runs and the selection check on $sha."
}

$comment = New-RunTestsComment -Selection $selection -Inventory $inventory -PullRequest $pr -WorkflowRunUrl $runUrl `
    -Build $build -QueueError $queueError -Requester $Actor -SelectionOnly:$SkipQueue
$commentPath = Join-Path $OutputDirectory 'comment.md'
$comment | Set-Content -LiteralPath $commentPath -Encoding utf8
if ($DryRun) {
    Write-Host "Dry run complete. Outputs are in $OutputDirectory."
    return
}
Invoke-GhJson @('-X', 'POST', "repos/$Repository/issues/$PrNumber/comments") -InputJson (@{ body = $comment } | ConvertTo-Json) | Out-Null
Write-Host "Posted the run-tests comment on #$PrNumber."
if ($queueError) { Write-Host "::warning::$queueError" }
