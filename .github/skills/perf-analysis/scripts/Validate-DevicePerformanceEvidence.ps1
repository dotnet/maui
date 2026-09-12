#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Seals complete device-performance summaries for one exact PR revision.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$SelectionPath,

    [Parameter(Mandatory = $true)]
    [string[]]$SummaryPath,

    [Parameter(Mandatory = $false)]
    [string]$BuildManifestPath,

    [Parameter(Mandatory = $true)]
    [string]$Repository,

    [Parameter(Mandatory = $true)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$PullRequestNumber,

    [Parameter(Mandatory = $true)]
    [string]$BaseCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$HeadCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$CurrentHeadSha,

    [Parameter(Mandatory = $true)]
    [string]$HarnessSha,

    [Parameter(Mandatory = $true)]
    [string]$JsonOut
)

$ErrorActionPreference = "Stop"
$errors = New-Object System.Collections.Generic.List[string]
$accepted = New-Object System.Collections.Generic.List[object]
$seenPairs = New-Object System.Collections.Generic.HashSet[string]

function Add-Error([string]$message) {
    $errors.Add($message)
}

function Get-PropertyValue($object, [string]$propertyPath) {
    $value = $object
    foreach ($segment in $propertyPath.Split(".")) {
        if ($null -eq $value) {
            return $null
        }

        $property = $value.PSObject.Properties[$segment]
        if ($null -eq $property) {
            return $null
        }
        $value = $property.Value
    }
    return $value
}

if (-not (Test-Path $SelectionPath)) {
    throw "Selection evidence does not exist: $SelectionPath"
}

$selection = Get-Content $SelectionPath -Raw | ConvertFrom-Json
$buildManifest = if ($BuildManifestPath) {
    if (-not (Test-Path $BuildManifestPath)) {
        throw "Build manifest does not exist: $BuildManifestPath"
    }
    @(Get-Content $BuildManifestPath -Raw | ConvertFrom-Json)
} else {
    @()
}
$supportedScenarios = @(
    $selection.deviceScenarios |
        Where-Object {
            $_.automationStatus -eq "manual-device-ci-ready" -and
            -not [string]::IsNullOrWhiteSpace([string]$_.resultScenario)
        }
)
$unsupportedScenarios = @(
    $selection.deviceScenarios |
        Where-Object { $_.automationStatus -eq "required-not-yet-automated" }
)

$required = @(
    foreach ($scenario in $supportedScenarios) {
        foreach ($platform in @($scenario.pipeline.platforms)) {
            [PSCustomObject]@{
                scenarioId = [string]$scenario.id
                resultScenario = [string]$scenario.resultScenario
                platform = ([string]$platform).ToLowerInvariant()
            }
        }
    }
)

if ($required.Count -eq 0) {
    Add-Error "Selection contains no supported device-performance scenario."
}

if ($HeadCommitSha -ne $CurrentHeadSha) {
    Add-Error "Requested head '$HeadCommitSha' is stale; the current PR head is '$CurrentHeadSha'."
}

foreach ($path in $SummaryPath) {
    if (-not (Test-Path $path)) {
        Add-Error "Device comparison summary does not exist: $path"
        continue
    }

    try {
        $summary = Get-Content $path -Raw | ConvertFrom-Json
    } catch {
        Add-Error "Device comparison summary '$path' is invalid JSON."
        continue
    }

    $expectedValues = [ordered]@{
        "schemaVersion" = "2"
        "expected.repository" = $Repository
        "expected.pullRequestNumber" = "$PullRequestNumber"
        "expected.baseCommitSha" = $BaseCommitSha
        "expected.headCommitSha" = $HeadCommitSha
        "expected.harnessSha" = $HarnessSha
        "expected.variantRuns" = "2"
    }
    foreach ($entry in $expectedValues.GetEnumerator()) {
        $actual = [string](Get-PropertyValue $summary $entry.Key)
        if ($actual -ne [string]$entry.Value) {
            Add-Error "Summary '$path' has $($entry.Key) '$actual'; expected '$($entry.Value)'."
        }
    }

    $scenario = [string](Get-PropertyValue $summary "expected.scenario")
    $platform = ([string](Get-PropertyValue $summary "expected.platform")).ToLowerInvariant()
    $pair = "$scenario|$platform"
    $requirement = @(
        $required |
            Where-Object { $_.resultScenario -eq $scenario -and $_.platform -eq $platform }
    )

    if ($requirement.Count -ne 1) {
        Add-Error "Summary '$path' does not match exactly one requested scenario/platform pair."
    } elseif (-not $seenPairs.Add($pair)) {
        Add-Error "Duplicate device summary for '$pair'."
    }

    $comparison = @($summary.comparisons)
    if ($summary.provenanceValidated -ne $true -or
        $summary.correctnessPassed -ne $true -or
        $summary.verdict -eq "inconclusive" -or
        $comparison.Count -ne 1 -or
        $comparison[0].Complete -ne $true)
    {
        Add-Error "Summary '$path' is incomplete or failed provenance/correctness validation."
    }

    if ($buildManifest.Count -gt 0) {
        $manifestEntry = @(
            $buildManifest |
                Where-Object {
                    $_.request.expectedScenario -eq $scenario -and
                    $_.request.platform -eq $platform
                }
        )
        if ($manifestEntry.Count -ne 1) {
            Add-Error "Summary '$path' does not match exactly one queued build."
        } elseif ([string]$comparison[0].Build.azdoBuildId -ne [string]$manifestEntry[0].buildId) {
            Add-Error "Summary '$path' build ID does not match the queued build."
        } elseif ($manifestEntry[0].status -ne "completed" -or $manifestEntry[0].result -ne "succeeded") {
            Add-Error "Queued build '$($manifestEntry[0].buildId)' did not complete successfully."
        }
    }

    $accessibilityStatuses = @($summary.accessibilityStatuses | Sort-Object -Unique)
    if ("failed" -in $accessibilityStatuses) {
        Add-Error "Summary '$path' reports an accessibility failure."
    }

    if ($requirement.Count -eq 1) {
        $accepted.Add([PSCustomObject]@{
            scenarioId = $requirement[0].scenarioId
            resultScenario = $scenario
            platform = $platform
            verdict = [string]$summary.verdict
            accessibilityStatuses = $accessibilityStatuses
            azdoBuildId = [string]$comparison[0].Build.azdoBuildId
            azdoBuildUrl = [string]$comparison[0].Build.azdoBuildUrl
        })
    }
}

$missing = @(
    $required |
        Where-Object { -not $seenPairs.Contains("$($_.resultScenario)|$($_.platform)") }
)
$unsupportedMeasurements = @(
    foreach ($scenario in $unsupportedScenarios) {
        [PSCustomObject]@{
            scenarioId = [string]$scenario.id
            resultScenario = $null
            platform = @($scenario.platforms) -join ","
            reason = "No trusted automated device scenario is available."
        }
    }
)
$missing = @($missing) + @($unsupportedMeasurements)
$sealed = $errors.Count -eq 0
$allAffectedPlatformsCovered = $required.Count -gt 0 -and $missing.Count -eq 0 -and $unsupportedScenarios.Count -eq 0
$allAccessibilityStatuses = @($accepted.accessibilityStatuses | ForEach-Object { $_ } | Sort-Object -Unique)
$accessibilityStatus = if ("failed" -in $allAccessibilityStatuses) {
    "failed"
} elseif ($allAccessibilityStatuses.Count -gt 0 -and "not-assessed" -notin $allAccessibilityStatuses) {
    "passed"
} else {
    "not-assessed"
}

$result = [PSCustomObject]@{
    schemaVersion = 1
    sealed = $sealed
    deviceEvidenceComplete = $sealed -and $allAffectedPlatformsCovered
    repository = $Repository
    pullRequestNumber = $PullRequestNumber
    baseCommitSha = $BaseCommitSha
    headCommitSha = $HeadCommitSha
    harnessSha = $HarnessSha
    correctnessPassed = $sealed -and $accepted.Count -gt 0
    accessibilityStatus = $accessibilityStatus
    allAffectedPlatformsCovered = $allAffectedPlatformsCovered
    requiredMeasurements = $required
    acceptedMeasurements = @($accepted | ForEach-Object { $_ })
    missingMeasurements = $missing
    errors = @($errors | ForEach-Object { $_ })
}

$directory = Split-Path -Parent $JsonOut
if ($directory -and -not (Test-Path $directory)) {
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
}
$result | ConvertTo-Json -Depth 12 | Set-Content $JsonOut -Encoding UTF8

if (-not $sealed) {
    foreach ($validationError in $errors) {
        [Console]::Error.WriteLine("ERROR: $validationError")
    }
    exit 2
}

Write-Host "Device evidence sealed (complete=$allAffectedPlatformsCovered)."
exit 0
