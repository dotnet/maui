#!/usr/bin/env pwsh
<#
.SYNOPSIS
Collects the evidence the `/review run-ui-tests` agent uses to pick UI tests.

.DESCRIPTION
Runs from the trusted default-branch checkout before the agent starts. Writes
context.json with the PR identity, the changed files (with truncated patches), and
the UI test category inventory that maui-pr-uitests can run. PR code is never executed.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateRange(1, 2147483647)][int]$PrNumber,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$Repository = $(if ($env:GITHUB_REPOSITORY) { $env:GITHUB_REPOSITORY } else { 'dotnet/maui' }),
    [string]$PipelineYamlPath = "$PSScriptRoot/../../eng/pipelines/common/ui-tests.yml",
    [int]$MaxPatchCharsPerFile = 6000,
    [int]$MaxPatchCharsTotal = 200000,
    # Tell the requester on the PR when its target branch cannot run a selection.
    [switch]$PostUnsupportedComment,
    # 'ui' (/review run-ui-tests, maui-pr-uitests), 'device' (/review run-device-tests, maui-pr-devicetests)
    # or 'all' (/review run-tests: both, each suite's inventory and hints under `ui` and `device`).
    [ValidateSet('ui', 'device', 'all')][string]$Suite = 'ui'
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/RunTests.Common.ps1"
$combined = $Suite -eq 'all'
$requested = if ($combined) { @('ui', 'device') } else { @($Suite) }

function Invoke-GhJson {
    param([string[]]$Arguments)
    $raw = & gh api @Arguments
    if ($LASTEXITCODE -ne 0) { throw "gh api $($Arguments -join ' ') failed." }
    return ($raw -join "`n") | ConvertFrom-Json -Depth 50
}

$pr = Invoke-GhJson @("repos/$Repository/pulls/$PrNumber")
if ($pr.state -ne 'open') { throw "PR #$PrNumber is not open." }

# The dispatch commit carries the target branch's pipeline, or the default branch's when the target
# branch's cannot build run-tests/* dispatches (see Invoke-RunTests.ps1). Only if neither can would
# nothing run, so say so now instead of running both models for nothing.
function Test-RunTestsDispatchAt([string]$Ref) {
    if (-not $Ref) { return $false }
    $pipeline = & gh api -H 'Accept: application/vnd.github.raw' "repos/$Repository/contents/$($script:RunTestsPipelinePath)?ref=$([uri]::EscapeDataString($Ref))" 2>$null
    # An unreadable pipeline is not a reason to refuse; the dispatch re-checks it.
    return ($LASTEXITCODE -ne 0) -or (Test-RunTestsPipelineSupportsDispatch ($pipeline -join "`n"))
}
$suites = [System.Collections.Generic.List[string]]::new()
$notes = [System.Collections.Generic.List[string]]::new()
foreach ($candidate in $requested) {
    Set-RunTestsSuite $candidate -Combined:$combined
    if ((Test-RunTestsDispatchAt $pr.base.ref) -or (Test-RunTestsDispatchAt $pr.base.repo.default_branch)) { $suites.Add($candidate); continue }
    $notes.Add("$($script:RunTestsKind.Substring(0, 1).ToUpperInvariant() + $script:RunTestsKind.Substring(1)) can't run on PRs that target ``$($pr.base.ref)`` yet: neither that branch's nor the default branch's $($script:RunTestsPipelineName) pipeline builds $($script:RunTestsDispatchBranchPrefix)* dispatches. Use ``/azp run $($script:RunTestsPipelineName)`` for the full suite.")
}
if (-not $suites.Count) {
    $marker = if ($combined) { $script:RunTestsCombined.UnsupportedMarker } else { $script:RunTestsUnsupportedMarker }
    $message = "``/review $($script:RunTestsCommand)`` can't run on this PR. $($notes -join ' ')"
    Write-Host $message
    if ($PostUnsupportedComment) {
        $body = @{ body = "$marker`n$message" } | ConvertTo-Json
        $posted = $body | & gh api -X POST "repos/$Repository/issues/$PrNumber/comments" --input -
        if ($LASTEXITCODE -eq 0) { Hide-RunTestsOutdatedComments -Repository $Repository -PrNumber $PrNumber -KeepId (($posted -join "`n") | ConvertFrom-Json).id }
    }
    if ($env:GITHUB_OUTPUT) { "ready=false" | Add-Content -LiteralPath $env:GITHUB_OUTPUT }
    return
}

# --paginate concatenates pages as separate JSON arrays; --slurp merges them.
$pages = Invoke-GhJson @('--paginate', '--slurp', "repos/$Repository/pulls/$PrNumber/files?per_page=100")
$files = @($pages | ForEach-Object { $_ })

$budget = $MaxPatchCharsTotal
$changed = foreach ($file in $files) {
    $patch = [string]$file.PSObject.Properties['patch']?.Value
    $truncated = $false
    if ($patch.Length -gt $MaxPatchCharsPerFile) { $patch = $patch.Substring(0, $MaxPatchCharsPerFile); $truncated = $true }
    if ($patch.Length -gt $budget) { $patch = $patch.Substring(0, [Math]::Max(0, $budget)); $truncated = $true }
    $budget -= $patch.Length
    [ordered]@{
        path = $file.filename
        previousPath = $file.PSObject.Properties['previous_filename']?.Value
        status = $file.status
        additions = $file.additions
        deletions = $file.deletions
        patch = $patch
        patchTruncated = $truncated
    }
}

function Get-DeviceSuiteContext {
    # Device test categories at the PR head (a PR may add one) and in this trusted checkout.
    $deviceCategories = [ordered]@{}
    $deviceHints = [System.Collections.Generic.List[object]]::new()
    foreach ($project in $script:RunTestsDeviceProjects.Keys) {
        $source = $script:RunTestsDeviceProjects[$project].categories
        if (-not $source) { continue }
        $local = Join-Path "$PSScriptRoot/../.." $source
        $text = if (Test-Path -LiteralPath $local) { Get-Content -LiteralPath $local -Raw } else { '' }
        $head = & gh api -H 'Accept: application/vnd.github.raw' "repos/$Repository/contents/$($source)?ref=$($pr.head.sha)" 2>$null
        if ($LASTEXITCODE -eq 0) { $text += "`n" + ($head -join "`n") }
        $deviceCategories[$project] = @(Get-RunTestsDeviceCategories $text)
    }
    $deviceInventory = Get-RunTestsDeviceInventory $deviceCategories
    # Categories referenced by changed device tests ([Category(TestCategory.X)]): a strong hint.
    foreach ($entry in $changed) {
        if ($entry.path -notmatch '(?i)^src/(Controls|Core)/tests/DeviceTests/') { continue }
        $project = $Matches[1]
        $found = @([regex]::Matches([string]$entry.patch, 'TestCategory\.(\w+)') | ForEach-Object { "$project.$($_.Groups[1].Value)" } |
            Where-Object { $_ -cin $deviceInventory.categories } | Sort-Object -Unique)
        if ($found.Count) { $deviceHints.Add([ordered]@{ path = $entry.path; categories = $found }) }
    }
    return [ordered]@{
        testCategoryHints = @($deviceHints)
        inventory = [ordered]@{
            platforms = @(Get-RunTestsPlatformIds)
            categories = @($deviceInventory.categories)
            umbrellas = $deviceInventory.umbrellas
            projects = $deviceInventory.projects
        }
    }
}

function Get-UiSuiteContext {
    $inventory = Get-RunTestsInventory -PipelineYamlPath $PipelineYamlPath

    # Map UITestCategories constant names to their category values (e.g. LifeCycle -> Lifecycle).
    $categoryConstants = @{}
    $categoriesSource = "$PSScriptRoot/../../src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs"
    if (Test-Path -LiteralPath $categoriesSource) {
        foreach ($match in [regex]::Matches((Get-Content -LiteralPath $categoriesSource -Raw), 'const\s+string\s+(\w+)\s*=\s*"([^"]+)"')) {
            $categoryConstants[$match.Groups[1].Value] = $match.Groups[2].Value
        }
    }

    # Categories referenced by changed UI tests: a strong, cheap hint for the agent.
    $testCategoryHints = foreach ($entry in $changed) {
        if ($entry.path -notmatch '(?i)^src/Controls/tests/TestCases\.[^/]+\.Tests/') { continue }
        $found = @([regex]::Matches([string]$entry.patch, 'UITestCategories\.(\w+)') | ForEach-Object {
            $name = $_.Groups[1].Value
            if ($categoryConstants.ContainsKey($name)) { $categoryConstants[$name] } else { $name }
        } | Sort-Object -Unique)
        if ($found.Count) { [ordered]@{ path = $entry.path; categories = $found } }
    }

    # Categories enabled by the PR's own UI test files, per platform. Each changed test file is read
    # at the PR head and its #if guards are evaluated for every platform project that compiles it.
    $changedTestRunnable = [ordered]@{}
    foreach ($platform in Get-RunTestsPlatformIds) { $changedTestRunnable[$platform] = [System.Collections.Generic.List[string]]::new() }
    $projectPlatforms = @{ 'Shared' = @(Get-RunTestsPlatformIds); 'Android' = @('android'); 'iOS' = @('ios'); 'Mac' = @('maccatalyst'); 'WinUI' = @('windows') }
    $changedTestFiles = @($changed | Where-Object {
            $_.status -ne 'removed' -and $_.path -match '^src/Controls/tests/TestCases\.(Shared|Android|iOS|Mac|WinUI)\.Tests/.+\.cs$'
        })
    $maxChangedTestFiles = 100
    # When a changed test file cannot be inspected, the categories it enables are unknown. The trusted
    # side then keeps every AI-selected category instead of pruning to the categories known to be runnable.
    $changedTestScanComplete = ($changedTestFiles.Count -le $maxChangedTestFiles) -and ($files.Count -lt 3000)
    foreach ($file in @($changedTestFiles | Select-Object -First $maxChangedTestFiles)) {
        $null = $file.path -match '^src/Controls/tests/TestCases\.(\w+)\.Tests/'
        $encodedPath = (($file.path -split '/') | ForEach-Object { [uri]::EscapeDataString($_) }) -join '/'
        $raw = & gh api -H 'Accept: application/vnd.github.raw' "repos/$Repository/contents/$($encodedPath)?ref=$($pr.head.sha)" 2>$null
        if ($LASTEXITCODE -ne 0 -or -not $raw) { $changedTestScanComplete = $false; continue }
        $content = $raw -join "`n"
        foreach ($platform in $projectPlatforms[$Matches[1]]) {
            foreach ($category in Get-RunTestsFileCategories -Content $content -Symbols $script:RunTestsPlatformSymbols[$platform] -CategoryConstants $categoryConstants) {
                if (-not $changedTestRunnable[$platform].Contains($category)) { $changedTestRunnable[$platform].Add($category) }
            }
        }
    }

    return [ordered]@{
        testCategoryHints = @($testCategoryHints)
        # Categories the PR's own changed UI tests enable, per platform (evaluated at the PR head).
        changedTestRunnable = $changedTestRunnable
        # False when some changed UI test file could not be read; pruning is then skipped.
        changedTestScanComplete = $changedTestScanComplete
        inventory = [ordered]@{
            platforms = @(Get-RunTestsPlatformIds)
            categories = @(Get-RunTestsAllCategories $inventory)
            umbrellas = $inventory.umbrellas
            # Categories with at least one enabled test per platform on the base branch.
            runnable = (Get-RunTestsRunnableCategories -TestsRoot "$PSScriptRoot/../../src/Controls/tests")
        }
    }
}

$context = [ordered]@{
    suite = $Suite
    repository = $Repository
    prNumber = $PrNumber
    title = $pr.title
    body = $(if ($pr.body) { ([string]$pr.body).Substring(0, [Math]::Min(4000, ([string]$pr.body).Length)) } else { '' })
    labels = @($pr.labels | ForEach-Object { $_.name })
    headSha = $pr.head.sha
    baseRef = $pr.base.ref
    baseSha = $pr.base.sha
    changedFileCount = $files.Count
    changedFiles = @($changed)
}
$counts = foreach ($part in $suites) {
    $suiteContext = if ($part -eq 'device') { Get-DeviceSuiteContext } else { Get-UiSuiteContext }
    if ($combined) { $context[$part] = $suiteContext } else { foreach ($key in $suiteContext.Keys) { $context[$key] = $suiteContext[$key] } }
    "$($suiteContext.inventory.categories.Count) $($script:RunTestsSuites[$part].Kind) categories"
}
if ($combined) {
    # The suites that can run on this PR's target branch, and why any other cannot.
    $context.suites = @($suites)
    $context.notes = @($notes)
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$path = Join-Path $OutputDirectory 'context.json'
$context | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $path -Encoding utf8
Write-Host "Collected $($files.Count) changed files and $($counts -join ', ') for PR #$PrNumber at $($pr.head.sha)."
if ($env:GITHUB_OUTPUT) { "ready=true" | Add-Content -LiteralPath $env:GITHUB_OUTPUT }
