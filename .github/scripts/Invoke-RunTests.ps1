#!/usr/bin/env pwsh
<#
.SYNOPSIS
Trusted half of `/review run-ui-tests`: validates the AI selection, queues maui-pr-uitests,
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
    # The /review run-ui-tests run to credit in the comment and checks.
    [string]$WorkflowRunUrl,
    [string]$OutputDirectory = $(if ($env:RUNNER_TEMP) { Join-Path $env:RUNNER_TEMP 'run-tests' } else { Join-Path ([IO.Path]::GetTempPath()) 'run-tests' }),
    [string]$Repository = $(if ($env:GITHUB_REPOSITORY) { $env:GITHUB_REPOSITORY } else { 'dotnet/maui' }),
    [string]$Actor = $env:GITHUB_ACTOR,
    [string]$PipelineYamlPath = "$PSScriptRoot/../../eng/pipelines/common/ui-tests.yml",
    # Azure DevOps definition of the suite's pipeline (maui-pr-uitests 313, maui-pr-devicetests 314).
    [int]$DefinitionId,
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

function Select-ProposalFields {
    # Only the fields the agent may set; anything else in its output is ignored. The device_* fields
    # carry the device test half of a `/review run-tests` (both suites) proposal.
    param([System.Collections.IDictionary]$Item)
    $proposal = @{}
    foreach ($key in $script:RunTestsProposalKeys) {
        if ($Item.Contains($key)) { $proposal[$key] = $Item[$key] }
        if ($Item.Contains("device_$key")) { $proposal["device_$key"] = $Item["device_$key"] }
    }
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
# /review run-ui-tests (UI tests), /review run-device-tests (device tests) or /review run-tests
# (both, in one comment); the trusted context says which.
$combined = $context.PSObject.Properties['suite'] -and $context.suite -ceq 'all'
$suites = if ($combined) {
    @($context.suites | Where-Object { $_ -cin @('ui', 'device') } | Select-Object -Unique)
} elseif ($context.PSObject.Properties['suite'] -and $context.suite -ceq 'device') { @('device') } else { @('ui') }
if (-not $suites.Count) { throw 'The collected context names no test suite to run.' }
$definitionOverride = $DefinitionId
$proposals = Get-Proposals

function Get-SuiteContext([string]$Suite) {
    # A combined context keeps each suite's inventory and hints under `ui` / `device`.
    if (-not $combined) { return $context }
    $suiteContext = [ordered]@{}
    foreach ($property in $context.PSObject.Properties) { if ($property.Name -notin 'ui', 'device') { $suiteContext[$property.Name] = $property.Value } }
    foreach ($property in $context.$Suite.PSObject.Properties) { $suiteContext[$property.Name] = $property.Value }
    $suiteContext.suite = $Suite
    return [pscustomobject]$suiteContext
}

function Resolve-SuiteSelection([string]$Suite) {
    <#
    .SYNOPSIS
    Validates each model's proposal for one suite and returns { inventory; selection; selectionError }.
    #>
    $context = Get-SuiteContext $Suite
    $runnable = $null
    if ($Suite -eq 'device') {
        # Device test categories were read by the trusted context step (base checkout plus the PR head);
        # Get-RunTestsDeviceCategories keeps identifiers only.
        $deviceCategories = [ordered]@{}
        foreach ($project in $script:RunTestsDeviceProjects.Keys) {
            if (-not $script:RunTestsDeviceProjects[$project].categories) { continue }
            $names = @($context.inventory.projects.$project | Where-Object { $_ -is [string] -and $_.StartsWith("$project.") } | ForEach-Object { $_.Substring($project.Length + 1) })
            $deviceCategories[$project] = @(Get-RunTestsDeviceCategories (($names | ForEach-Object { "const string $_ = `"$_`";" }) -join "`n"))
        }
        $inventory = Get-RunTestsDeviceInventory $deviceCategories
    } else {
        $inventory = Get-RunTestsInventory -PipelineYamlPath $PipelineYamlPath
        # Which categories have enabled tests on each platform, from this trusted checkout, plus the
        # categories the PR's own changed tests enable (evaluated per platform at the PR head).
        $runnable = Get-RunTestsRunnableCategories -TestsRoot "$PSScriptRoot/../../src/Controls/tests"
    }
    if ($Suite -eq 'ui' -and $context.PSObject.Properties['changedTestRunnable'] -and $context.changedTestRunnable) {
        foreach ($platform in Get-RunTestsPlatformIds) {
            $extra = @($context.changedTestRunnable.$platform | Where-Object { $_ -is [string] -and $_ -cmatch '^[A-Za-z0-9_]{1,64}$' })
            if ($extra.Count) { $runnable[$platform] = @(@($runnable[$platform]) + $extra | Sort-Object -Unique) }
        }
    }
    if ($Suite -eq 'ui' -and $context.PSObject.Properties['changedTestScanComplete'] -and $context.changedTestScanComplete -eq $false) {
        # Some changed UI test files were not inspected, so an AI-selected category may be enabled only
        # by them. Keep every AI-selected category rather than pruning it as "no enabled tests".
        Write-Host 'Not every changed UI test file was inspected; keeping all AI-selected categories.'
        $runnable = $null
    }
    # Only AI-selected tests run: each model's proposal is validated on its own, then the union is taken.
    $resolved = [ordered]@{}
    foreach ($model in $proposals.Keys) {
        $proposal = Get-RunTestsSuiteProposal -Proposal $proposals[$model] -Suite $Suite -Combined:$combined
        $resolved[$model] = Resolve-RunTestsSelection -Proposal $proposal -Inventory $inventory -Runnable $runnable
    }
    $selection = Merge-RunTestsSelections -Selections $resolved -Inventory $inventory
    $selectionError = if ($proposals.Count -eq 0) {
        "No AI model produced a test selection, so nothing was queued. Comment ``/review $($script:RunTestsCommand)`` again, or use ``/azp run $($script:RunTestsPipelineName)`` for the full suite."
    }
    $selection | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $OutputDirectory $(if ($combined) { "selection-$Suite.json" } else { 'selection.json' })) -Encoding utf8
    return @{ inventory = $inventory; selection = $selection; selectionError = $selectionError }
}

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
            Write-Host "``$ref`` cannot build $($script:RunTestsDispatchBranchPrefix)* dispatches; trying the default branch's pipeline."
        }
        if (-not $sourceSha) {
            return @{ error = "Neither ``$($PullRequest.base.ref)`` nor the default branch can build ``/review $($script:RunTestsCommand)`` dispatches ($($script:RunTestsPipelinePath) does not build $($script:RunTestsDispatchBranchPrefix)* branches). Use ``/azp run $($script:RunTestsPipelineName)`` for the full suite." }
        }
    } elseif (-not (Test-RunTestsPipelineSupportsDispatch (Get-RunTestsPipelineAt $sourceSha))) {
        return @{ error = "The pipeline source $sourceSha cannot build $($script:RunTestsDispatchBranchPrefix)* dispatches." }
    }
    if ([string]$sourceSha -cnotmatch '^[0-9a-f]{40}$') { throw 'Could not resolve the trusted pipeline commit.' }
    $source = Invoke-GhJson @("repos/$Repository/git/commits/$sourceSha")
    $sourceRoot = Invoke-GhJson @("repos/$Repository/git/trees/$($source.tree.sha)")
    # PR code with the trusted pipeline, workflows (and for device tests the Helix project and
    # Windows runner that apply the selection). Taking .github from the parent also means the commit
    # changes no workflow, which GITHUB_TOKEN may not do.
    $entries = foreach ($path in $script:RunTestsTrustedPaths) {
        $parentTree = $source.tree.sha
        $segments = $path -split '/'
        $entry = $null
        foreach ($segment in $segments) {
            $entry = @((Invoke-GhJson @("repos/$Repository/git/trees/$parentTree")).tree | Where-Object { $_.path -ceq $segment })[0]
            if (-not $entry) { break }
            $parentTree = $entry.sha
        }
        if (-not $entry) { throw "The trusted commit has no $path." }
        @{ path = $path; mode = $entry.mode; type = $entry.type; sha = $entry.sha }
    }
    $overlay = @{ base_tree = $MergeCommit.tree.sha; tree = @($entries) } | ConvertTo-Json -Depth 5
    $tree = Invoke-GhJson @('-X', 'POST', "repos/$Repository/git/trees") -InputJson $overlay
    $yaml = New-RunTestsSuiteRequestYaml -Selection $Selection -Inventory $inventory -PrNumber $PrNumber -HeadSha $PullRequest.head.sha
    $request = @{ base_tree = $tree.sha; tree = @(@{ path = $script:RunTestsRequestPath; mode = '100644'; type = 'blob'; content = $yaml }) } | ConvertTo-Json -Depth 5
    $tree = Invoke-GhJson @('-X', 'POST', "repos/$Repository/git/trees") -InputJson $request

    $message = "/review $($script:RunTestsCommand) dispatch for #$PrNumber at $($PullRequest.head.sha.Substring(0, 8))`n`n" +
        "Test-merge $($MergeCommit.sha) with $($script:RunTestsTrustedPaths -join ', ') from $sourceSha.`nRequested in $runUrl"
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
            Write-Warning "Could not list $($script:RunTestsPipelineName) builds: $($_.Exception.Message)"
        }
        if ([DateTime]::UtcNow -ge $deadline) { return $null }
        Write-Host "Waiting for Azure Pipelines to start $($script:RunTestsPipelineName) for $Branch..."
        Start-Sleep -Seconds 15
    }
}

$verified = Get-VerifiedPullRequest
$pr = $verified.pr

function Invoke-SuiteRun {
    <#
    .SYNOPSIS
    Dispatches one suite's selection and publishes its checks. Returns what its comment shows.
    #>
    param([Parameter(Mandatory)][string]$Suite)
    Set-RunTestsSuite $Suite -Combined:$combined
    $DefinitionId = if ($definitionOverride) { $definitionOverride } else { $script:RunTestsDefinitionId }
    $resolvedSuite = Resolve-SuiteSelection $Suite
    $inventory = $resolvedSuite.inventory
    $selection = $resolvedSuite.selection
    $selectionError = $resolvedSuite.selectionError
    $queueError = $null
    if ($selectionError) {
        $queueError = $selectionError
    } elseif ($pr.state -ne 'open') {
        $queueError = 'The PR is no longer open.'
    } elseif ($pr.head.sha -cne $context.headSha) {
        $queueError = "The PR was updated while tests were being selected (selected for $($context.headSha.Substring(0, 8)), now $($pr.head.sha.Substring(0, 8))). Comment ``/review $($script:RunTestsCommand)`` again."
        # Report against the commit the selection was made for.
        $pr.head.sha = $context.headSha
    } elseif ($context.baseRef -and $pr.base.ref -cne $context.baseRef) {
        # Retargeting keeps the head SHA, but the models selected tests for the diff against the old base.
        $queueError = "The PR's target branch changed while tests were being selected (selected against ``$($context.baseRef)``, now ``$($pr.base.ref)``). Comment ``/review $($script:RunTestsCommand)`` again."
    } elseif ($selection.mode -ne 'none' -and -not $verified.merge -and $pr.mergeable -eq $false) {
        $queueError = "This PR has merge conflicts with ``$($pr.base.ref)``, so there is no test-merge commit to build. Resolve the conflicts, then comment ``/review $($script:RunTestsCommand)`` again."
    } elseif ($selection.mode -ne 'none' -and -not $verified.merge) {
        $queueError = "GitHub has no test-merge commit for this PR head (it may have merge conflicts, or GitHub is still computing it). Resolve conflicts or retry ``/review $($script:RunTestsCommand)``."
    }

    $build = $null
    $dispatch = $null
    if (-not $queueError -and $selection.mode -ne 'none') {
        New-RunTestsSuiteRequestYaml -Selection $selection -Inventory $inventory -PrNumber $PrNumber -HeadSha $pr.head.sha |
            Set-Content (Join-Path $OutputDirectory (Split-Path -Leaf $script:RunTestsRequestPath)) -Encoding utf8
        if ($DryRun) {
            Write-Host 'Dry run: dispatch request written; nothing pushed.'
        } elseif ($SkipQueue) {
            Write-Host "$($script:RunTestsPipelineName) dispatch is not available in this repository; publishing the selection only."
        } else {
            try {
                $dispatch = New-RunTestsDispatch -PullRequest $pr -MergeCommit $verified.mergeCommit -Selection $selection
                if ($dispatch.error) {
                    $queueError = $dispatch.error
                    $dispatch = $null
                } else {
                    $build = Wait-RunTestsDispatchBuild -Branch $dispatch.branch -Sha $dispatch.sha
                    if ($build) {
                        Write-Host "$($script:RunTestsPipelineName) build $($build.id) is building the dispatch commit: $($build._links.web.href)"
                    } else {
                        $queueError = "Azure Pipelines did not start $($script:RunTestsPipelineName) for the dispatch branch ``$($dispatch.branch)`` within $([int]($DispatchWaitSeconds / 60)) minutes. Comment ``/review $($script:RunTestsCommand)`` again, or use ``/azp run $($script:RunTestsPipelineName)`` for the full suite."
                        & gh api -X DELETE "repos/$Repository/git/refs/heads/$($dispatch.branch)" 2>$null | Out-Null
                        $dispatch = $null
                    }
                }
            } catch {
                Write-Warning "Dispatching $($script:RunTestsPipelineName) failed: $($_.Exception.Message)"
                $queueError = "Pushing the $($script:RunTestsPipelineName) dispatch commit failed; see the workflow log."
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
    $checks | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $OutputDirectory $(if ($combined) { "checks-$Suite.json" } else { 'checks.json' })) -Encoding utf8

    $platformText = if ($selection.platforms.Count) { ($selection.platforms | ForEach-Object { Get-RunTestsPlatformName $_ }) -join ', ' } else { 'none' }
    $buildUrl = if ($build) { [string]$build._links.web.href } else { $runUrl }
    $selectionSummary = @(
        "**Mode:** $($selection.mode)  ",
        "**Platforms:** $platformText  ",
        "**Categories:** $(if ($selection.mode -eq 'full') { 'all' } elseif ($selection.mode -eq 'none') { 'none' } else { Format-RunTestsCategoryList $selection.expandedCategories })",
        '',
        $selection.summary,
        '',
        $(if ($build) { "Azure DevOps run: [$($script:RunTestsPipelineName) #$($build.id)]($buildUrl)" } elseif ($queueError) { "Not queued: $queueError" } elseif ($SkipQueue) { 'Selection only: Azure DevOps dispatch is not available in this repository.' } else { '' })
    ) -join "`n"

    if (-not $DryRun -and -not $SkipChecks) {
        $sha = $pr.head.sha
        # A later run on the same commit may select something an earlier run skipped: retire those.
        $existing = Invoke-GhJson @('--paginate', '--slurp', "repos/$Repository/commits/$sha/check-runs?per_page=100&filter=latest")
        $existingRuns = @($existing | ForEach-Object { $_.check_runs } | Where-Object {
            $_.app.slug -eq 'github-actions' -and $_.name.StartsWith($script:RunTestsSkippedCheckPrefix) })
        $planned = @($checks | ForEach-Object { $_.name })
        foreach ($stale in $existingRuns | Where-Object { $_.name -notin $planned }) {
            $body = @{ conclusion = 'neutral'; output = @{ title = "Selected by a later /review $($script:RunTestsCommand)"; summary = "Superseded by [a later selection]($runUrl)." } } | ConvertTo-Json -Depth 5
            Invoke-GhJson @('-X', 'PATCH', "repos/$Repository/check-runs/$($stale.id)") -InputJson $body | Out-Null
        }

        $now = (Get-Date).ToUniversalTime().ToString('o')
        foreach ($check in $checks) {
            $summary = "Not selected by ``/review $($script:RunTestsCommand)`` for this commit.`n`n$selectionSummary"
            $body = [ordered]@{
                name = $check.name; head_sha = $sha; status = 'completed'; conclusion = 'skipped'
                started_at = $now; completed_at = $now; details_url = $buildUrl
                output = [ordered]@{ title = $check.title; summary = $summary }
            } | ConvertTo-Json -Depth 5
            Invoke-GhJson @('-X', 'POST', "repos/$Repository/check-runs") -InputJson $body | Out-Null
        }

        $selectionConclusion = if ($queueError) { 'failure' } else { 'success' }
        $title = if ($queueError) { "$($script:RunTestsKind) were not queued" } elseif ($selection.mode -eq 'none') { "No $($script:RunTestsKind) selected" } elseif ($selection.mode -eq 'full') { "Full $($script:RunTestsPipelineName) suite selected" } else { "$(Format-RunTestsCount $selection.expandedCategories.Count 'category' 'categories') on $platformText" }
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
                output = [ordered]@{ title = "$($script:RunTestsPipelineName) #$($build.id) is running"; summary = $selectionSummary }
            } | ConvertTo-Json -Depth 5
            $dispatch.checkRunId = (Invoke-GhJson @('-X', 'POST', "repos/$Repository/check-runs") -InputJson $body).id
        }
        Write-Host "Published $($checks.Count) skipped check runs and the selection check on $sha."
    }

    return @{ suite = $Suite; selection = $selection; inventory = $inventory; build = $build; dispatch = $dispatch; queueError = $queueError }
}

$runs = [ordered]@{}
foreach ($suite in $suites) { $runs[$suite] = Invoke-SuiteRun $suite }

if ($combined) {
    $modelNames = @($proposals.Keys | ForEach-Object { $script:RunTestsModels[$_] })
    $notes = @($context.PSObject.Properties['notes']?.Value | Where-Object { $_ -is [string] })
    $comment = New-RunTestsCombinedComment -Runs $runs -PullRequest $pr -WorkflowRunUrl $runUrl -ModelNames $modelNames `
        -Requester $Actor -Notes $notes -SelectionOnly:$SkipQueue
} else {
    $run = $runs[$suites[0]]
    $comment = New-RunTestsComment -Selection $run.selection -Inventory $run.inventory -PullRequest $pr -WorkflowRunUrl $runUrl `
        -Build $run.build -QueueError $run.queueError -Requester $Actor -Dispatch $run.dispatch -SelectionOnly:$SkipQueue
}
$commentPath = Join-Path $OutputDirectory 'comment.md'
$comment | Set-Content -LiteralPath $commentPath -Encoding utf8
if ($DryRun) {
    Write-Host "Dry run complete. Outputs are in $OutputDirectory."
    return
}
$posted = Invoke-GhJson @('-X', 'POST', "repos/$Repository/issues/$PrNumber/comments") -InputJson (@{ body = $comment } | ConvertTo-Json)
Write-Host "Posted the $($script:RunTestsCommand) comment on #$PrNumber."
Hide-RunTestsOutdatedComments -Repository $Repository -PrNumber $PrNumber -KeepId $posted.id
foreach ($run in $runs.Values) { if ($run.queueError) { Write-Host "::warning::$($run.suite): $($run.queueError)" } }
