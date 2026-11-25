#!/usr/bin/env pwsh
<#
.SYNOPSIS
Trusted half of `/review run-tests`: validates the AI selection, queues maui-pr-uitests,
publishes "skipped" check runs and posts the PR comment.

.DESCRIPTION
Runs in an isolated job (never in the agent's sandbox). The agent's output is only a proposal:
platforms and categories are validated against the inventory from the trusted checkout, the
caller's permission and the PR head are re-checked, and only then is maui-pr-uitests started.

GitHub Actions has no way to queue an Azure DevOps build: Microsoft Entra refuses the federated
identity (AADSTS53003, Conditional Access) and Azure Pipelines ignores `/azp run` from bots. So
the run is dispatched through the pipeline's CI trigger instead: a commit pushed to a
run-tests/* branch, made of the PR's test-merge tree with eng/pipelines and .github replaced by the
target branch's trusted copies, plus eng/pipelines/common/run-tests-request.yml carrying the
validated selection. maui-pr-uitests builds it with only the selected tests and without the
MAUI variable group, like a pull-request build from a fork (see eng/pipelines/ci-uitests.yml).

That build reports its checks on the dispatch commit, so a "maui-pr-uitests (selected run)"
check on the PR head follows it (Update-RunTestsResults.ps1 completes it), and anything not
selected is published as a check run with conclusion "skipped".

.PARAMETER PipelineSource
For trials only: build with the pipeline and workflows of this commit instead of the target
branch tip (for example a branch that adds dispatch support before it is merged).
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
    [int]$DefinitionId = 313,
    # Names the dispatch branch; unique per request.
    [string]$DispatchRunId = $(if ($env:GITHUB_RUN_ID) { $env:GITHUB_RUN_ID } else { [string][DateTimeOffset]::UtcNow.ToUnixTimeSeconds() }),
    # How long to wait for Azure Pipelines to start the build of the dispatch branch.
    [int]$DispatchWaitSeconds = 600,
    [string]$PipelineSource,
    # Do not queue, create checks or comment; write the planned outputs to OutputDirectory.
    [switch]$DryRun,
    # Queue and comment but do not create check runs (the Checks API needs a GitHub App token).
    [switch]$SkipChecks,
    # Do not dispatch maui-pr-uitests (e.g. a fork of the repository); still publish checks and comment.
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
    if ($ProposalJson) { $proposals['gpt-6.2-sol'] = Select-ProposalFields ($ProposalJson | ConvertFrom-Json -AsHashtable) }
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
        if ($items.Count) { $proposals['gpt-6.2-sol'] = Select-ProposalFields $items[-1] }
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
if ($context.PSObject.Properties['changedTestScanComplete'] -and $context.changedTestScanComplete -eq $false) {
    # Some changed UI test files were not inspected, so an AI-selected category may be enabled only
    # by them. Keep every AI-selected category rather than pruning it as "no enabled tests".
    Write-Host 'Not every changed UI test file was inspected; keeping all AI-selected categories.'
    $runnable = $null
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
function Get-RunTestsPipelineAt([string]$Ref) {
    $raw = & gh api -H 'Accept: application/vnd.github.raw' "repos/$Repository/contents/$($script:RunTestsPipelinePath)?ref=$Ref" 2>$null
    if ($LASTEXITCODE -ne 0) { return '' }
    return ($raw -join "`n")
}

function Get-VerifiedPullRequest {
    <#
    .SYNOPSIS
    Reads the PR and confirms GitHub's test-merge commit really merges the selected head.
    After a push GitHub recomputes the merge asynchronously; until then merge_commit_sha can
    still point at a merge of the previous head, so wait for it rather than trusting it.
    #>
    $attempt = 0
    while ($true) {
        $attempt++
        $current = Invoke-GhJson @("repos/$Repository/pulls/$PrNumber")
        $mergeCommit = $null
        if ($current.state -eq 'open' -and $current.merge_commit_sha) {
            $mergeCommit = Invoke-GhJson @("repos/$Repository/git/commits/$($current.merge_commit_sha)")
        }
        $state = Get-RunTestsMergeState -PullRequest $current -MergeCommit $mergeCommit -HeadSha $context.headSha -BaseRef $context.baseRef
        if ($state -ne 'wait') { return @{ pr = $current; merge = ($state -eq 'ready'); mergeCommit = $mergeCommit } }
        # A missing or outdated merge of the head itself normally settles within a minute or two.
        if ($attempt -ge 12) { return @{ pr = $current; merge = $false; mergeCommit = $null } }
        Write-Host "Waiting for GitHub to compute the test-merge commit for $($context.headSha.Substring(0, 8)) (attempt $attempt)..."
        Start-Sleep -Seconds 15
    }
}

function New-RunTestsDispatch {
    <#
    .SYNOPSIS
    Pushes the dispatch commit for this request and returns { sha; branch } (or { error }).
    #>
    param([Parameter(Mandatory)]$PullRequest, [Parameter(Mandatory)]$MergeCommit, [Parameter(Mandatory)]$Selection)
    # The trusted half: pipeline and workflows of the target branch, or of the default branch when the
    # target branch's pipeline cannot take dispatches (servicing branches cut before run-tests support).
    # The PR's own code still comes from its test-merge commit.
    $sourceSha = $PipelineSource
    if (-not $sourceSha) {
        foreach ($ref in @($PullRequest.base.ref, $PullRequest.base.repo.default_branch) | Where-Object { $_ } | Select-Object -Unique) {
            $sha = (Invoke-GhJson @("repos/$Repository/git/ref/heads/$ref")).object.sha
            if (Test-RunTestsPipelineSupportsDispatch (Get-RunTestsPipelineAt $sha)) { $sourceSha = $sha; break }
            Write-Host "``$ref`` cannot build run-tests/* dispatches; trying the default branch's pipeline."
        }
        if (-not $sourceSha) {
            return @{ error = "Neither ``$($PullRequest.base.ref)`` nor the default branch can build ``/review run-tests`` dispatches (eng/pipelines/ci-uitests.yml does not build run-tests/* branches). Use ``/azp run maui-pr-uitests`` for the full suite." }
        }
    } elseif (-not (Test-RunTestsPipelineSupportsDispatch (Get-RunTestsPipelineAt $sourceSha))) {
        return @{ error = "The pipeline source $sourceSha cannot build run-tests/* dispatches." }
    }
    if ([string]$sourceSha -cnotmatch '^[0-9a-f]{40}$') { throw 'Could not resolve the trusted pipeline commit.' }
    $source = Invoke-GhJson @("repos/$Repository/git/commits/$sourceSha")
    $sourceRoot = Invoke-GhJson @("repos/$Repository/git/trees/$($source.tree.sha)")
    $githubTree = @($sourceRoot.tree | Where-Object { $_.path -ceq '.github' -and $_.type -eq 'tree' })[0].sha
    $engTree = @($sourceRoot.tree | Where-Object { $_.path -ceq 'eng' -and $_.type -eq 'tree' })[0].sha
    $pipelinesTree = @((Invoke-GhJson @("repos/$Repository/git/trees/$engTree")).tree | Where-Object { $_.path -ceq 'pipelines' -and $_.type -eq 'tree' })[0].sha
    if (-not $githubTree -or -not $pipelinesTree) { throw 'The trusted commit has no .github or eng/pipelines directory.' }

    # PR code with the trusted pipeline and workflows. Taking .github from the parent also means the
    # commit changes no workflow, which GITHUB_TOKEN may not do.
    $overlay = @{ base_tree = $MergeCommit.tree.sha; tree = @(
            @{ path = '.github'; mode = '040000'; type = 'tree'; sha = $githubTree },
            @{ path = 'eng/pipelines'; mode = '040000'; type = 'tree'; sha = $pipelinesTree }) } | ConvertTo-Json -Depth 5
    $tree = Invoke-GhJson @('-X', 'POST', "repos/$Repository/git/trees") -InputJson $overlay
    $yaml = New-RunTestsRequestYaml -Selection $Selection -PrNumber $PrNumber -HeadSha $PullRequest.head.sha
    $request = @{ base_tree = $tree.sha; tree = @(@{ path = $script:RunTestsRequestPath; mode = '100644'; type = 'blob'; content = $yaml }) } | ConvertTo-Json -Depth 5
    $tree = Invoke-GhJson @('-X', 'POST', "repos/$Repository/git/trees") -InputJson $request

    $message = "/review run-tests dispatch for #$PrNumber at $($PullRequest.head.sha.Substring(0, 8))`n`n" +
        "Test-merge $($MergeCommit.sha) with eng/pipelines and .github from $sourceSha.`nRequested in $runUrl"
    $commit = Invoke-GhJson @('-X', 'POST', "repos/$Repository/git/commits") -InputJson (@{ message = $message; tree = $tree.sha; parents = @($sourceSha) } | ConvertTo-Json)
    $branch = Get-RunTestsDispatchBranch -PrNumber $PrNumber -RunId $DispatchRunId
    Invoke-GhJson @('-X', 'POST', "repos/$Repository/git/refs") -InputJson (@{ ref = "refs/heads/$branch"; sha = $commit.sha } | ConvertTo-Json) | Out-Null
    Write-Host "Pushed dispatch commit $($commit.sha) to $branch."
    return @{ sha = $commit.sha; branch = $branch }
}

function Wait-RunTestsDispatchBuild {
    <#
    .SYNOPSIS
    The maui-pr-uitests build Azure Pipelines started for the dispatch commit (public API, no credentials).
    #>
    param([Parameter(Mandatory)][string]$Branch, [Parameter(Mandatory)][string]$Sha)
    $deadline = [DateTime]::UtcNow.AddSeconds($DispatchWaitSeconds)
    $uri = "https://dev.azure.com/dnceng-public/public/_apis/build/builds?definitions=$DefinitionId&branchName=$([uri]::EscapeDataString("refs/heads/$Branch"))&api-version=7.1"
    while ($true) {
        try {
            $found = @((Invoke-RestMethod -Uri $uri -TimeoutSec 30).value | Where-Object { $_.sourceVersion -ceq $Sha })
            if ($found.Count) { return $found[0] }
        } catch {
            Write-Warning "Could not list maui-pr-uitests builds: $($_.Exception.Message)"
        }
        if ([DateTime]::UtcNow -ge $deadline) { return $null }
        Write-Host "Waiting for Azure Pipelines to start maui-pr-uitests for $Branch..."
        Start-Sleep -Seconds 15
    }
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
$dispatch = $null
if (-not $queueError -and $selection.mode -ne 'none') {
    New-RunTestsRequestYaml -Selection $selection -PrNumber $PrNumber -HeadSha $pr.head.sha |
        Set-Content (Join-Path $OutputDirectory 'run-tests-request.yml') -Encoding utf8
    if ($DryRun) {
        Write-Host 'Dry run: dispatch request written; nothing pushed.'
    } elseif ($SkipQueue) {
        Write-Host 'maui-pr-uitests dispatch is not available in this repository; publishing the selection only.'
    } else {
        try {
            $dispatch = New-RunTestsDispatch -PullRequest $pr -MergeCommit $verified.mergeCommit -Selection $selection
            if ($dispatch.error) {
                $queueError = $dispatch.error
                $dispatch = $null
            } else {
                $build = Wait-RunTestsDispatchBuild -Branch $dispatch.branch -Sha $dispatch.sha
                if ($build) {
                    Write-Host "maui-pr-uitests build $($build.id) is building the dispatch commit: $($build._links.web.href)"
                } else {
                    $queueError = "Azure Pipelines did not start maui-pr-uitests for the dispatch branch ``$($dispatch.branch)`` within $([int]($DispatchWaitSeconds / 60)) minutes. Comment ``/review run-tests`` again, or use ``/azp run maui-pr-uitests`` for the full suite."
                    & gh api -X DELETE "repos/$Repository/git/refs/heads/$($dispatch.branch)" 2>$null | Out-Null
                    $dispatch = $null
                }
            }
        } catch {
            Write-Warning "Dispatching maui-pr-uitests failed: $($_.Exception.Message)"
            $queueError = 'Pushing the maui-pr-uitests dispatch commit failed; see the workflow log.'
            $build = $null
            $dispatch = $null
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
    if ($build -and $dispatch) {
        # maui-pr-uitests reports its own checks on the dispatch commit; this one follows it on the PR.
        $body = [ordered]@{
            name = $script:RunTestsRunCheckName; head_sha = $sha; status = 'in_progress'
            started_at = $now; details_url = $buildUrl; external_id = [string]$build.id
            output = [ordered]@{ title = "maui-pr-uitests #$($build.id) is running"; summary = $selectionSummary }
        } | ConvertTo-Json -Depth 5
        $dispatch.checkRunId = (Invoke-GhJson @('-X', 'POST', "repos/$Repository/check-runs") -InputJson $body).id
    }
    Write-Host "Published $($checks.Count) skipped check runs and the selection check on $sha."
}

$comment = New-RunTestsComment -Selection $selection -Inventory $inventory -PullRequest $pr -WorkflowRunUrl $runUrl `
    -Build $build -QueueError $queueError -Requester $Actor -Dispatch $dispatch -SelectionOnly:$SkipQueue
$commentPath = Join-Path $OutputDirectory 'comment.md'
$comment | Set-Content -LiteralPath $commentPath -Encoding utf8
if ($DryRun) {
    Write-Host "Dry run complete. Outputs are in $OutputDirectory."
    return
}
$posted = Invoke-GhJson @('-X', 'POST', "repos/$Repository/issues/$PrNumber/comments") -InputJson (@{ body = $comment } | ConvertTo-Json)
Write-Host "Posted the run-tests comment on #$PrNumber."
Hide-RunTestsOutdatedComments -Repository $Repository -PrNumber $PrNumber -KeepId $posted.id
if ($queueError) { Write-Host "::warning::$queueError" }
