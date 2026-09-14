#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$SelectionPath,

    [Parameter(Mandatory = $true)]
    [string]$BundlesRoot,

    [Parameter(Mandatory = $false)]
    [string]$BuildManifestPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

function Read-Json([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "JSON file does not exist: $Path"
    }
    return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Write-Json([object]$Value, [string]$Path) {
    $directory = Split-Path -Parent $Path
    if ($directory) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }
    $Value | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $Path -Encoding UTF8
}

$selection = Read-Json $SelectionPath
$buildManifest = if ($BuildManifestPath -and (Test-Path -LiteralPath $BuildManifestPath)) {
    Read-Json $BuildManifestPath
}
else {
    @()
}
$buildsByRequest = @{}
foreach ($build in @($buildManifest)) {
    $buildsByRequest[[string]$build.requestKey] = $build
}

$results = @()
foreach ($request in @($selection.requests | Sort-Object scenarioId, platform)) {
    $bundle = Join-Path $BundlesRoot ([string]$request.requestKey)
    $summaryPath = Join-Path $bundle "comparison-summary.json"
    $build = $buildsByRequest[[string]$request.requestKey]
    if (-not (Test-Path -LiteralPath $summaryPath -PathType Leaf)) {
        $results += [PSCustomObject][ordered]@{
            requestKey = [string]$request.requestKey
            scenarioId = [string]$request.scenarioId
            platform = [string]$request.platform
            coverage = [string]$request.coverage
            verdict = "inconclusive"
            evidenceComplete = $false
            environmentComparable = $false
            checkpointComparisons = @()
            newLayoutFindingKeys = @()
            buildUrl = if ($null -ne $build) { [string]$build.buildUrl } else { $null }
            errorCodes = @("missing-sealed-bundle")
        }
        continue
    }

    $summary = Read-Json $summaryPath
    if ([string]$summary.request.requestKey -ne [string]$request.requestKey -or
        [string]$summary.request.headCommitSha -ne [string]$selection.headCommitSha) {
        throw "Comparison summary identity does not match request '$($request.requestKey)'."
    }

    $safeCheckpoints = @(
        $summary.checkpointComparisons |
            ForEach-Object {
                [PSCustomObject][ordered]@{
                    checkpointId = [string]$_.checkpointId
                    status = [string]$_.status
                    baseIntraDifference = [double]$_.baseIntraDifference
                    headIntraDifference = [double]$_.headIntraDifference
                    minimumCrossDifference = [double]$_.minimumCrossDifference
                    maximumCrossDifference = [double]$_.maximumCrossDifference
                }
            }
    )
    $results += [PSCustomObject][ordered]@{
        requestKey = [string]$request.requestKey
        scenarioId = [string]$request.scenarioId
        platform = [string]$request.platform
        coverage = [string]$request.coverage
        verdict = [string]$summary.verdict
        trustLevel = [string]$summary.trustLevel
        evidenceComplete = [bool]$summary.evidenceComplete
        environmentComparable = [bool]$summary.environmentComparable
        environment = [PSCustomObject][ordered]@{
            targetPlatform = [string]$summary.environment.targetPlatform
            platformVersion = [string]$summary.environment.platformVersion
            deviceModel = [string]$summary.environment.deviceModel
            displaySize = [string]$summary.environment.displaySize
            displayDensity = [string]$summary.environment.displayDensity
            orientation = [string]$summary.environment.orientation
        }
        checkpointComparisons = $safeCheckpoints
        newLayoutFindingKeys = @($summary.newLayoutFindingKeys | ForEach-Object { [string]$_ })
        buildUrl = if ($null -ne $build) { [string]$build.buildUrl } else { $null }
        errorCodes = @($summary.errors | ForEach-Object {
            $value = [string]$_
            if ($value -match '^[a-z0-9:-]{1,100}$') { $value } else { "invalid-error-code" }
        })
    }
}

$precedence = @(
    "head-functional-failure-advisory",
    "visual-change-advisory",
    "layout-change-advisory",
    "inconclusive",
    "no-difference-observed",
    "not-applicable"
)
$overall = "not-applicable"
foreach ($candidate in $precedence) {
    if (@($results | Where-Object { $_.verdict -eq $candidate }).Count -gt 0) {
        $overall = $candidate
        break
    }
}
if ([string]$selection.coverage.status -ne "complete" -and $overall -eq "no-difference-observed") {
    $overall = "inconclusive"
}

$agentSummary = [PSCustomObject][ordered]@{
    schemaVersion = 1
    repository = [string]$selection.repository
    pullRequestNumber = [int]$selection.pullRequestNumber
    measuredBaseSha = [string]$selection.baseCommitSha
    measuredHeadSha = [string]$selection.headCommitSha
    harnessSha = [string]$selection.harnessSha
    overallVerdict = $overall
    coverage = [PSCustomObject][ordered]@{
        status = [string]$selection.coverage.status
        uiRelevantFileCount = [int]$selection.coverage.uiRelevantFileCount
        directFileCount = [int]$selection.coverage.directFileCount
        sampledFileCount = [int]$selection.coverage.sampledFileCount
        unmappedFileCount = [int]$selection.coverage.unmappedFileCount
    }
    results = $results
    limitations = @(
        "The result is advisory and is not a merge gate.",
        "No-difference-observed is not a clean or safe verdict.",
        "DevFlow layout and source locations identify symptoms, not framework causality.",
        "Windows runs are co-resident with the app under test, so absence of change remains inconclusive.",
        "Only trusted selected scenarios and the listed platforms were exercised."
    )
}
Write-Json $agentSummary $OutputPath
