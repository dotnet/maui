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
    # proposal.json files handed over by the agentic workflows, one per model
    # (one run_selected_ui_tests item each, plus the trusted `model` id).
    [string[]]$ProposalPath,
    # The /review run-tests run to credit in the comment and checks.
    [string]$WorkflowRunUrl,
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

$runUrl = if ($WorkflowRunUrl -cmatch '^https://github\.com/[\w.-]+/[\w.-]+/actions/runs/\d+$') { $WorkflowRunUrl } elseif ($env:GITHUB_RUN_ID) { "$($env:GITHUB_SERVER_URL ?? 'https://github.com')/$Repository/actions/runs/$($env:GITHUB_RUN_ID)" } else { "https://github.com/$Repository" }
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

$script:ProposalKeys = 'platforms', 'categories', 'full_run', 'summary', 'rationale'

function Select-ProposalFields {
    # Only the fields the agent may set; anything else in its output is ignored.
    param([System.Collections.IDictionary]$Item)
    $proposal = @{}
    foreach ($key in $script:ProposalKeys) { if ($Item.Contains($key)) { $proposal[$key] = $Item[$key] } }
    return $proposal
}

function Get-Proposals {
    # model id -> proposal, in a stable model order.
    $proposals = [ordered]@{}
    if ($ProposalJson) { $proposals['gpt-6.1-sol'] = Select-ProposalFields ($ProposalJson | ConvertFrom-Json -AsHashtable) }
    foreach ($path in @($ProposalPath | Where-Object { $_ })) {
        if (-not (Test-Path -LiteralPath $path)) { throw "Proposal file $path does not exist." }
        if ((Get-Item -LiteralPath $path).Length -gt 65536) { throw 'The proposal file is unexpectedly large.' }
        $item = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json -AsHashtable -Depth 5
        $model = [string]$item['model']
        if (-not $script:RunTestsModels.Contains($model)) { throw "Proposal $path names an unknown model '$model'." }
        $proposals[$model] = Select-ProposalFields $item
    }
    if ($AgentOutputPath -and (Test-Path -LiteralPath $AgentOutputPath)) {
        $output = Get-Content -LiteralPath $AgentOutputPath -Raw | ConvertFrom-Json -AsHashtable -Depth 20
        $items = @($output.items | Where-Object { $_.type -eq 'run_selected_ui_tests' })
        # The tool is documented as call-once; use the last call if the agent corrected itself.
        if ($items.Count) { $proposals['gpt-6.1-sol'] = Select-ProposalFields $items[-1] }
    }
    $ordered = [ordered]@{}
    foreach ($model in $script:RunTestsModels.Keys) { if ($proposals.Contains($model)) { $ordered[$model] = $proposals[$model] } }
    return $ordered
}

$context = Get-Content -LiteralPath $ContextPath -Raw | ConvertFrom-Json -Depth 30
if ([int]$context.prNumber -ne $PrNumber) { throw 'The collected context belongs to a different PR.' }
$inventory = Get-RunTestsInventory -PipelineYamlPath $PipelineYamlPath

$proposals = Get-Proposals
# Which categories have enabled tests on each platform, from this trusted checkout, plus the
# categories the PR's own changed tests enable (evaluated per platform at the PR head).
$runnable = Get-RunTestsRunnableCategories -TestsRoot "$PSScriptRoot/../../src/Controls/tests"
if ($context.PSObject.Properties['changedTestRunnable'] -and $context.changedTestRunnable) {
    foreach ($platform in Get-RunTestsPlatformIds) {
        $extra = @($context.changedTestRunnable.$platform | Where-Object { $_ -is [string] -and $_ -cmatch '^[A-Za-z0-9_]{1,64}$' })
        if ($extra.Count) { $runnable[$platform] = @(@($runnable[$platform]) + $extra | Sort-Object -Unique) }
    }
}
# Only AI-selected tests run: each model's proposal is validated on its own, then the union is taken.
$resolved = [ordered]@{}
foreach ($model in $proposals.Keys) {
    $resolved[$model] = Resolve-RunTestsSelection -Proposal $proposals[$model] -Inventory $inventory -Runnable $runnable
}
$selection = Merge-RunTestsSelections -Selections $resolved -Inventory $inventory
$selectionError = if ($proposals.Count -eq 0) {
    'No AI model produced a test selection, so nothing was queued. Comment `/review run-tests` again, or use `/azp run maui-pr-uitests` for the full suite.'
}
$selection | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $OutputDirectory 'selection.json') -Encoding utf8

# Re-check the caller and the PR: the agent may have taken minutes and the PR may have moved.
if (-not $DryRun -and $Actor) {
    $permission = Invoke-GhJson @("repos/$Repository/collaborators/$Actor/permission")
    if ($permission.permission -notin @('admin', 'maintain', 'write')) {
        throw "@$Actor no longer has write access; nothing was queued."
    }
}
function Get-VerifiedPullRequest {
    <#
    .SYNOPSIS
    Reads the PR and confirms GitHub's test-merge commit really merges the selected head.
    After a push GitHub recomputes the merge asynchronously; until then merge_commit_sha can
    still point at a merge of the previous head, so wait for it rather than trusting it.
    #>
    for ($attempt = 1; $attempt -le 8; $attempt++) {
        $current = Invoke-GhJson @("repos/$Repository/pulls/$PrNumber")
        if ($current.state -ne 'open' -or $current.head.sha -cne $context.headSha -or
            ($context.baseRef -and $current.base.ref -cne $context.baseRef)) { return @{ pr = $current; merge = $false } }
        if ($current.mergeable -eq $false) { return @{ pr = $current; merge = $false } }
        if ($current.merge_commit_sha) {
            $merge = Invoke-GhJson @("repos/$Repository/git/commits/$($current.merge_commit_sha)")
            if (@($merge.parents).Count -eq 2 -and $merge.parents[1].sha -ceq $current.head.sha) {
                return @{ pr = $current; merge = $true }
            }
        }
        Write-Host "Waiting for GitHub to compute the test-merge commit for $($context.headSha.Substring(0, 8)) (attempt $attempt)..."
        Start-Sleep -Seconds 10
    }
    return @{ pr = $current; merge = $false }
}

$verified = Get-VerifiedPullRequest
$pr = $verified.pr
$queueError = $null
if ($selectionError) {
    $queueError = $selectionError
} elseif ($pr.state -ne 'open') {
    $queueError = 'The PR is no longer open.'
} elseif ($pr.head.sha -cne $context.headSha) {
    $queueError = "The PR was updated while tests were being selected (selected for $($context.headSha.Substring(0, 8)), now $($pr.head.sha.Substring(0, 8))). Comment ``/review run-tests`` again."
    # Report against the commit the selection was made for.
    $pr.head.sha = $context.headSha
} elseif ($context.baseRef -and $pr.base.ref -cne $context.baseRef) {
    # Retargeting keeps the head SHA, but the models selected tests for the diff against the old base.
    $queueError = "The PR's target branch changed while tests were being selected (selected against ``$($context.baseRef)``, now ``$($pr.base.ref)``). Comment ``/review run-tests`` again."
} elseif ($selection.mode -ne 'none' -and -not $verified.merge) {
    $queueError = 'GitHub has no test-merge commit for this PR head (it may have merge conflicts, or GitHub is still computing it). Resolve conflicts or retry `/review run-tests`.'
}

$build = $null
if (-not $queueError -and $selection.mode -ne 'none') {
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
            if (-not (Test-RunTestsQueuedSelection -Build $build -Selection $selection) -or $build.sourceVersion -cne $pr.merge_commit_sha) {
                # Never leave an unexpected full run behind a "targeted" comment.
                Invoke-RestMethod -Method Patch -Uri "$base/$($build.id)?api-version=7.1" -Headers $headers `
                    -ContentType 'application/json' -Body '{"status":"cancelling"}' -TimeoutSec 30 | Out-Null
                $build = $null
                $queueError = 'Azure DevOps did not apply the selection to the verified test-merge commit; the run was cancelled.'
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
    $title = if ($queueError) { 'UI tests were not queued' } elseif ($selection.mode -eq 'none') { 'No UI tests selected' } elseif ($selection.mode -eq 'full') { 'Full UI test suite selected' } else { "$(Format-RunTestsCount $selection.expandedCategories.Count 'category' 'categories') on $platformText" }
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
