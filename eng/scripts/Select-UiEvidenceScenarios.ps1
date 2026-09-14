#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$ChangedFilesPath,

    [Parameter(Mandatory = $true)]
    [string]$BaseCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$HeadCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$HarnessSha,

    [Parameter(Mandatory = $false)]
    [string]$Repository = "dotnet/maui",

    [Parameter(Mandatory = $true)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$PullRequestNumber,

    [Parameter(Mandatory = $false)]
    [string]$RegistryPath = (Join-Path $PSScriptRoot "..\ui-evidence\scenarios.json"),

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")

function Get-UiEvidencePathPlatformScope([string]$Path) {
    $platformMarkers = @{
        android = "(^|/)(Android)(/|$)|\.Android\.[^/]+$"
        windows = "(^|/)(Windows)(/|$)|\.Windows\.[^/]+$"
        unsupported = "(^|/)(iOS|MacCatalyst|Tizen|Standard)(/|$)|\.(iOS|MacCatalyst|Tizen|Standard)\.[^/]+$"
    }

    $supportedPlatforms = @(
        foreach ($platform in @("android", "windows")) {
            if ($Path -match $platformMarkers[$platform]) {
                $platform
            }
        }
    )

    [PSCustomObject]@{
        IsPlatformSpecific = $supportedPlatforms.Count -gt 0 -or $Path -match $platformMarkers.unsupported
        SupportedPlatforms = $supportedPlatforms
    }
}

Assert-UiEvidenceSha $BaseCommitSha "BaseCommitSha"
Assert-UiEvidenceSha $HeadCommitSha "HeadCommitSha"
Assert-UiEvidenceSha $HarnessSha "HarnessSha"

if (-not (Test-Path -LiteralPath $ChangedFilesPath -PathType Leaf)) {
    throw "Changed-files input does not exist: $ChangedFilesPath"
}

$registry = Read-UiEvidenceJson $RegistryPath
if ($registry.schemaVersion -ne 1) {
    throw "Unsupported UI evidence scenario registry version '$($registry.schemaVersion)'."
}

$scenarioIds = @{}
foreach ($scenario in @($registry.scenarios)) {
    Assert-UiEvidenceId ([string]$scenario.id) "Scenario id"
    if ($scenarioIds.ContainsKey([string]$scenario.id)) {
        throw "Duplicate UI evidence scenario id '$($scenario.id)'."
    }

    $scenarioIds[[string]$scenario.id] = $scenario
    foreach ($platform in @($scenario.platforms)) {
        if ($platform -notin @("android", "windows")) {
            throw "Scenario '$($scenario.id)' contains unsupported platform '$platform'."
        }
    }
}

$productRoots = @($registry.productRoots | ForEach-Object {
    (Normalize-UiEvidenceRepositoryPath ([string]$_)).TrimEnd('/') + '/'
})
$changedFiles = @(
    Get-Content -LiteralPath $ChangedFilesPath -Encoding UTF8 |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        ForEach-Object { Normalize-UiEvidenceRepositoryPath $_ }
)

$fileResults = @()
$requestMap = @{}

foreach ($path in $changedFiles) {
    $pathPlatformScope = Get-UiEvidencePathPlatformScope $path
    $isProductFile = @($productRoots | Where-Object {
        $path.StartsWith($_, [StringComparison]::Ordinal)
    }).Count -gt 0

    if (-not $isProductFile) {
        $fileResults += [PSCustomObject][ordered]@{
            path = $path
            classification = "not-ui-relevant"
            affectedPlatforms = @()
            selectedScenarioIds = @()
        }
        continue
    }

    $ruleMatches = @()
    foreach ($scenario in @($registry.scenarios)) {
        foreach ($rule in @($scenario.pathRules)) {
            if ($path -match [string]$rule.pattern) {
                $rulePlatforms = if ($rule.PSObject.Properties.Name -contains "platforms") {
                    @($rule.platforms)
                }
                else {
                    @($scenario.platforms)
                }
                if ($pathPlatformScope.IsPlatformSpecific) {
                    $rulePlatforms = @($rulePlatforms | Where-Object {
                        $_ -in $pathPlatformScope.SupportedPlatforms
                    })
                }
                if ($rulePlatforms.Count -eq 0) {
                    continue
                }

                $ruleMatches += [PSCustomObject]@{
                    Scenario = $scenario
                    Coverage = [string]$rule.coverage
                    Platforms = @($rulePlatforms)
                }
            }
        }
    }

    if ($ruleMatches.Count -eq 0) {
        $fileResults += [PSCustomObject][ordered]@{
            path = $path
            classification = "unmapped"
            affectedPlatforms = @()
            selectedScenarioIds = @()
        }
        continue
    }

    $classification = if (@($ruleMatches | Where-Object { $_.Coverage -eq "direct" }).Count -gt 0) {
        "direct"
    }
    else {
        "sampled"
    }
    $platforms = @($ruleMatches | ForEach-Object { $_.Platforms } | Sort-Object -Unique)
    $selectedIds = @($ruleMatches | ForEach-Object { $_.Scenario.id } | Sort-Object -Unique)

    foreach ($match in $ruleMatches) {
        foreach ($platform in $match.Platforms) {
            $requestKey = "$($match.Scenario.id)|$platform"
            if (-not $requestMap.ContainsKey($requestKey)) {
                $requestMap[$requestKey] = [PSCustomObject][ordered]@{
                    scenarioId = [string]$match.Scenario.id
                    platform = [string]$platform
                    coverage = [string]$match.Coverage
                }
            }
            elseif ($match.Coverage -eq "sampled") {
                $requestMap[$requestKey].coverage = "sampled"
            }
        }
    }

    $fileResults += [PSCustomObject][ordered]@{
        path = $path
        classification = $classification
        affectedPlatforms = $platforms
        selectedScenarioIds = $selectedIds
    }
}

$requests = @($requestMap.Values | Sort-Object scenarioId, platform)
$uiRelevant = @($fileResults | Where-Object { $_.classification -ne "not-ui-relevant" })
$directCount = @($fileResults | Where-Object { $_.classification -eq "direct" }).Count
$sampledCount = @($fileResults | Where-Object { $_.classification -eq "sampled" }).Count
$unmappedCount = @($fileResults | Where-Object { $_.classification -eq "unmapped" }).Count
$maxRequests = [int]$registry.limits.maxScenarioPlatformPairs

$selectionStatus = if ($uiRelevant.Count -eq 0) {
    "no-ui-relevant-changes"
}
elseif ($requests.Count -eq 0) {
    "no-trusted-scenario"
}
elseif ($requests.Count -gt $maxRequests) {
    "selection-overflow"
}
else {
    "ready"
}

$coverageStatus = if ($uiRelevant.Count -eq 0) {
    "none"
}
elseif ($unmappedCount -gt 0) {
    "partial"
}
elseif ($sampledCount -gt 0) {
    if ($directCount -gt 0) { "partial" } else { "sampled-only" }
}
elseif ($directCount -gt 0) {
    "complete"
}
else {
    "none"
}

[object[]]$selectedRequests = if ($selectionStatus -eq "ready") {
    @($requests)
}
else {
    @()
}

$selection = [PSCustomObject][ordered]@{
    schemaVersion = 1
    repository = $Repository
    pullRequestNumber = $PullRequestNumber
    baseCommitSha = $BaseCommitSha.ToLowerInvariant()
    headCommitSha = $HeadCommitSha.ToLowerInvariant()
    harnessSha = $HarnessSha.ToLowerInvariant()
    registrySha256 = Get-UiEvidenceSha256 $RegistryPath
    selectionStatus = $selectionStatus
    requests = $selectedRequests
    coverage = [PSCustomObject][ordered]@{
        status = $coverageStatus
        uiRelevantFileCount = $uiRelevant.Count
        directFileCount = $directCount
        sampledFileCount = $sampledCount
        unmappedFileCount = $unmappedCount
    }
}

Write-UiEvidenceJson $selection $OutputPath

if ($selectionStatus -eq "ready") {
    exit 0
}

exit 3
