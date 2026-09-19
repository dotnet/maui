#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$SelectionPath,

    [Parameter(Mandatory = $true)]
    [string]$BundlesRoot,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"
$coreScripts = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\..\..\..\eng\scripts"))
. (Join-Path $coreScripts "UiEvidence.Common.ps1")

function Get-LocalPath([string]$Path) {
    $provider = $null
    $drive = $null
    $fullPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath(
        $Path, [ref]$provider, [ref]$drive)
    if ($provider.Name -ne "FileSystem" -or $fullPath -match '^(\\\\|//)' -or
        ($null -ne $drive -and ($drive.Root -match '^(\\\\|//)' -or $drive.DisplayRoot -match '^(\\\\|//)'))) {
        throw "UI evidence inputs and outputs must use local filesystem paths."
    }
    for ($ancestor = $fullPath; $ancestor; $ancestor = [IO.Path]::GetDirectoryName($ancestor)) {
        if (Test-Path -LiteralPath $ancestor) {
            $item = Get-Item -LiteralPath $ancestor -Force
            if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "UI evidence paths cannot contain reparse points: $ancestor"
            }
        }
    }
    return $fullPath
}

function Assert-BundleTree([string]$Root) {
    $directories = [Collections.Generic.Stack[string]]::new()
    $directories.Push($Root)
    while ($directories.Count -gt 0) {
        foreach ($item in @(Get-ChildItem -LiteralPath $directories.Pop() -Force)) {
            if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "UI evidence bundles cannot contain reparse points."
            }
            if ($item.PSIsContainer) {
                $directories.Push($item.FullName)
            }
        }
    }
}

function Read-SummaryJson([string]$Path) {
    if ((Get-Item -LiteralPath $Path -Force).Length -gt 1048576) {
        throw "UI evidence summary input exceeds the 1 MiB limit: $Path"
    }
    return Read-UiEvidenceJson $Path
}

function Assert-MatchingFields($Actual, $Expected, [string[]]$Fields, [string]$Context) {
    foreach ($field in $Fields) {
        $actualProperty = $Actual.PSObject.Properties[$field]
        $expectedProperty = $Expected.PSObject.Properties[$field]
        if ($null -eq $actualProperty -or $null -eq $expectedProperty -or
            $null -eq $actualProperty.Value -or
            [string]$actualProperty.Value -cne [string]$expectedProperty.Value) {
            throw "$Context identity field '$field' does not match the selected request."
        }
    }
}

function Assert-RunContract($Request) {
    if ($Request.expectedVariantRuns -ne 2 -or
        $Request.expectedRunOrder -isnot [array] -or
        $Request.expectedRunOrder.Count -ne 4 -or
        ($Request.expectedRunOrder -join ",") -cne "base-1,head-1,head-2,base-2") {
        throw "UI evidence requests must retain the core four-run contract."
    }
}

function Get-BoundedText($Value, [string]$Name, [int]$MaximumLength = 160, [switch]$AllowNull) {
    if ($null -eq $Value -and $AllowNull) {
        return $null
    }
    if ($Value -isnot [string] -or $Value.Length -gt $MaximumLength -or
        $Value -match '[\p{Cc}\p{Cf}]') {
        throw "UI evidence field '$Name' must be bounded, single-line text."
    }
    return $Value
}

function Get-EnvironmentText($Environment, [string]$Name) {
    $property = $Environment.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }
    return Get-BoundedText $property.Value "environment.$Name" -AllowNull
}

function Get-ErrorCodes($Values) {
    if ($Values -isnot [array] -or $Values.Count -gt 256) {
        throw "UI evidence diagnostic codes must be a bounded array."
    }
    foreach ($value in $Values) {
        if ($value -isnot [string] -or $value -cnotmatch '^[a-z0-9:-]{1,100}$') {
            throw "UI evidence contains an invalid diagnostic code."
        }
        $value
    }
}

$SelectionPath = Get-LocalPath $SelectionPath
$BundlesRoot = Get-LocalPath $BundlesRoot
$OutputPath = Get-LocalPath $OutputPath
if ((Test-Path -LiteralPath $BundlesRoot) -and -not (Test-Path -LiteralPath $BundlesRoot -PathType Container)) {
    throw "BundlesRoot must be a local directory, not a file."
}
$bundlePrefix = $BundlesRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
$selectionDirectory = Split-Path -Parent $SelectionPath
if ($OutputPath -eq $BundlesRoot -or
    $OutputPath.StartsWith($bundlePrefix, [StringComparison]::OrdinalIgnoreCase) -or
    $OutputPath -in @(
        $SelectionPath,
        (Join-Path $selectionDirectory "requests.json"),
        (Join-Path $selectionDirectory "scenarios.json")
    )) {
    throw "OutputPath must not overwrite local context or sealed bundle inputs."
}

$selection = Read-SummaryJson $SelectionPath
if ($selection.schemaVersion -ne 1) {
    throw "Unsupported UI evidence selection version."
}
foreach ($field in @("baseCommitSha", "headCommitSha", "harnessSha")) {
    Assert-UiEvidenceSha ([string]$selection.$field) $field
}
if ($selection.registrySha256 -cnotmatch '^[0-9a-f]{64}$' -or
    $selection.repository -cnotmatch '^[A-Za-z0-9_.-]{1,100}/[A-Za-z0-9_.-]{1,100}$' -or
    ($selection.pullRequestNumber -isnot [long] -and $selection.pullRequestNumber -isnot [int]) -or
    $selection.pullRequestNumber -lt 1 -or $selection.pullRequestNumber -gt [int]::MaxValue) {
    throw "UI evidence selection provenance is invalid."
}
# The core selector can serialize its empty, non-ready request list as null.
if ($null -eq $selection.requests -and $selection.selectionStatus -cne "ready") {
    $selection.requests = @()
}
if ($selection.requests -isnot [array] -or $selection.requests.Count -gt 64 -or
    $selection.selectionStatus -cnotin @(
        "ready", "no-ui-relevant-changes", "no-trusted-scenario", "selection-overflow")) {
    throw "UI evidence selection status or request array is invalid."
}
foreach ($field in @("uiRelevantFileCount", "directFileCount", "sampledFileCount", "unmappedFileCount")) {
    $count = $selection.coverage.$field
    if (($count -isnot [long] -and $count -isnot [int]) -or $count -lt 0 -or $count -gt [int]::MaxValue) {
        throw "UI evidence coverage field '$field' must be a nonnegative integer."
    }
}
$coverage = $selection.coverage
$expectedCoverageStatus = if ($coverage.uiRelevantFileCount -eq 0) {
    "none"
}
elseif ($coverage.unmappedFileCount -gt 0 -or
    ($coverage.sampledFileCount -gt 0 -and $coverage.directFileCount -gt 0)) {
    "partial"
}
elseif ($coverage.sampledFileCount -gt 0) {
    "sampled-only"
}
else {
    "complete"
}
if ($coverage.uiRelevantFileCount -ne
    ($coverage.directFileCount + $coverage.sampledFileCount + $coverage.unmappedFileCount) -or
    $coverage.status -cne $expectedCoverageStatus -or
    (($selection.selectionStatus -eq "no-ui-relevant-changes") -ne ($coverage.uiRelevantFileCount -eq 0)) -or
    (($selection.selectionStatus -eq "ready") -ne ($selection.requests.Count -gt 0))) {
    throw "UI evidence selection status, requests, and coverage are inconsistent."
}

$identityFields = @(
    "schemaVersion", "repository", "pullRequestNumber", "baseCommitSha",
    "headCommitSha", "harnessSha", "registrySha256"
)
$requestFields = $identityFields + @("requestKey", "scenarioId", "platform", "coverage", "expectedVariantRuns")
$keys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($request in $selection.requests) {
    Assert-MatchingFields $request $selection $identityFields "Selected request"
    Assert-UiEvidenceId ([string]$request.scenarioId) "Scenario id"
    if ($request.platform -cnotin @("android", "windows") -or
        $request.coverage -cnotin @("direct", "sampled") -or
        $request.requestKey -cnotmatch '^maui-ui-[0-9a-f]{24}$') {
        throw "Selected UI evidence request has an invalid platform, coverage, or request key."
    }
    Assert-RunContract $request
    $expectedKey = Get-UiEvidenceRequestKey `
        -Repository $request.repository -PullRequestNumber $request.pullRequestNumber `
        -BaseCommitSha $request.baseCommitSha -HeadCommitSha $request.headCommitSha `
        -HarnessSha $request.harnessSha -RegistrySha256 $request.registrySha256 `
        -ScenarioId $request.scenarioId -Platform $request.platform
    if ($request.requestKey -cne $expectedKey -or -not $keys.Add($request.requestKey)) {
        throw "Selected UI evidence request key is inconsistent or duplicated."
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
$results = @()
foreach ($request in @($selection.requests | Sort-Object scenarioId, platform)) {
    $bundle = Get-LocalPath (Join-Path $BundlesRoot $request.requestKey)
    if (-not (Test-Path -LiteralPath $bundle)) {
        $results += [PSCustomObject][ordered]@{
            requestKey = [string]$request.requestKey
            scenarioId = [string]$request.scenarioId
            platform = [string]$request.platform
            coverage = [string]$request.coverage
            verdict = "inconclusive"
            bundleValidated = $false
            trustLevel = $null
            evidenceComplete = $false
            environmentComparable = $false
            environment = $null
            runs = @()
            checkpointComparisons = @()
            newLayoutFindingKeys = @()
            errorCodes = @("missing-sealed-bundle")
            warningCodes = @()
        }
        continue
    }

    if (-not (Test-Path -LiteralPath $bundle -PathType Container)) {
        throw "Selected UI evidence bundle is not a directory: $($request.requestKey)"
    }
    Assert-BundleTree $bundle
    $validationJson = & (Join-Path $coreScripts "Validate-UiEvidenceBundle.ps1") `
        -Root $bundle -ExpectedRequestKey $request.requestKey -ExpectedHeadCommitSha $selection.headCommitSha
    if ($LASTEXITCODE -ne 0) {
        throw "Sealed UI evidence bundle validation failed for '$($request.requestKey)'."
    }
    $validation = ($validationJson -join "`n") | ConvertFrom-Json
    if ($validation.valid -isnot [bool] -or -not $validation.valid) {
        throw "The core validator did not accept bundle '$($request.requestKey)'."
    }

    $seal = Read-UiEvidenceJson (Join-Path $bundle "evidence-seal.json")
    $sealFields = @($requestFields | Where-Object { $_ -notin @("registrySha256", "coverage", "expectedVariantRuns") })
    Assert-MatchingFields $seal $request $sealFields "Evidence seal"
    if ($seal.PSObject.Properties.Name -contains "registrySha256") {
        Assert-MatchingFields $seal $request @("registrySha256") "Evidence seal"
    }
    $requiredFiles = @("request.json", "comparison-summary.json") + @(
        "base-run1", "head-run1", "head-run2", "base-run2" | ForEach-Object { "$_/run-result.json" }
    )
    foreach ($relativePath in $requiredFiles) {
        if (@($seal.files.relativePath) -cnotcontains $relativePath) {
            throw "UI evidence bundle is missing required sealed file '$relativePath'."
        }
    }
    $bundleRequest = Read-SummaryJson (Join-Path $bundle "request.json")
    Assert-MatchingFields $bundleRequest $request $requestFields "Bundle request"
    Assert-RunContract $bundleRequest
    $registryPath = Join-Path $bundle "scenarios.json"
    if (Test-Path -LiteralPath $registryPath) {
        if (@($seal.files.relativePath) -cnotcontains "scenarios.json" -or
            (Get-UiEvidenceSha256 $registryPath) -cne $request.registrySha256) {
            throw "Bundled scenario registry does not match the selected registry digest."
        }
    }

    $safeRuns = @()
    $sequence = 0
    foreach ($runName in @("base-run1", "head-run1", "head-run2", "base-run2")) {
        $sequence++
        $variant, $ordinal = $runName.Split("-run")
        $run = Read-SummaryJson (Join-Path $bundle "$runName\run-result.json")
        Assert-MatchingFields $run $request @("schemaVersion", "requestKey", "scenarioId", "platform", "harnessSha") "Run '$runName'"
        $commit = if ($variant -eq "base") { $request.baseCommitSha } else { $request.headCommitSha }
        if ($run.variant -cne $variant -or $run.variantRunOrdinal -ne [int]$ordinal -or
            $run.sequenceOrdinal -ne $sequence -or $run.commitSha -cne $commit) {
            throw "Run '$runName' identity does not match the selected request order or commit."
        }
        if ($run.status -cnotin @("passed", "scenario-failed", "harness-failed", "timed-out")) {
            throw "UI evidence run '$runName' has an invalid status."
        }
        $safeRuns += [PSCustomObject][ordered]@{
            variant = $variant
            variantRunOrdinal = [int]$ordinal
            status = Get-BoundedText $run.status "run.status" 64
        }
    }

    $summary = Read-SummaryJson (Join-Path $bundle "comparison-summary.json")
    Assert-MatchingFields $summary.request $request $requestFields "Comparison summary"
    Assert-RunContract $summary.request
    if ($summary.schemaVersion -ne 1 -or $summary.provenanceValidated -isnot [bool] -or
        -not $summary.provenanceValidated -or $summary.evidenceComplete -isnot [bool] -or
        $summary.environmentComparable -isnot [bool] -or
        $summary.verdict -cnotin $precedence[0..4]) {
        throw "UI evidence comparison has invalid provenance, flags, or verdict."
    }
    $expectedTrust = if ($request.platform -eq "android") { "isolated-emulator" } else { "co-resident-advisory" }
    if ($summary.trustLevel -cne $expectedTrust -or $summary.environment.targetPlatform -cne $request.platform -or
        ($summary.verdict -ne "inconclusive" -and -not $summary.environmentComparable)) {
        throw "UI evidence comparison environment or trust level is inconsistent."
    }
    if ($summary.checkpointComparisons -isnot [array] -or $summary.checkpointComparisons.Count -gt 256 -or
        $summary.newLayoutFindingKeys -isnot [array] -or $summary.newLayoutFindingKeys.Count -gt 256) {
        throw "UI evidence comparison collections must be bounded arrays."
    }

    $safeCheckpoints = @(
        foreach ($checkpoint in $summary.checkpointComparisons) {
            Assert-UiEvidenceId ([string]$checkpoint.checkpointId) "Checkpoint id"
            if ($checkpoint.status -cnotin @("missing", "unstable", "change-detected", "no-difference-observed")) {
                throw "UI evidence checkpoint status is invalid."
            }
            foreach ($field in @("baseIntraDifference", "headIntraDifference", "minimumCrossDifference", "maximumCrossDifference")) {
                $value = $checkpoint.$field
                if (($value -isnot [long] -and $value -isnot [int] -and $value -isnot [double]) -or
                    -not [double]::IsFinite([double]$value) -or $value -lt 0 -or $value -gt 1) {
                    throw "UI evidence checkpoint '$field' must be a finite difference between zero and one."
                }
            }
            [PSCustomObject][ordered]@{
                checkpointId = [string]$checkpoint.checkpointId
                status = [string]$checkpoint.status
                baseIntraDifference = [double]$checkpoint.baseIntraDifference
                headIntraDifference = [double]$checkpoint.headIntraDifference
                minimumCrossDifference = [double]$checkpoint.minimumCrossDifference
                maximumCrossDifference = [double]$checkpoint.maximumCrossDifference
            }
        }
    )
    $errorCodes = @(Get-ErrorCodes $summary.errors)
    $warningCodes = @(Get-ErrorCodes $summary.warnings)
    if ($summary.verdict -eq "no-difference-observed" -and
        (-not $summary.evidenceComplete -or $request.coverage -ne "direct" -or
        $request.platform -ne "android" -or $errorCodes.Count -gt 0 -or
        $safeCheckpoints.Count -eq 0 -or $summary.newLayoutFindingKeys.Count -gt 0 -or
        @($safeCheckpoints | Where-Object { $_.status -ne "no-difference-observed" }).Count -gt 0 -or
        @($safeRuns | Where-Object { $_.status -ne "passed" }).Count -gt 0)) {
        throw "UI evidence no-difference verdict contradicts its evidence or coverage."
    }
    $results += [PSCustomObject][ordered]@{
        requestKey = [string]$request.requestKey
        scenarioId = [string]$request.scenarioId
        platform = [string]$request.platform
        coverage = [string]$request.coverage
        verdict = [string]$summary.verdict
        bundleValidated = $true
        trustLevel = [string]$summary.trustLevel
        evidenceComplete = [bool]$summary.evidenceComplete
        environmentComparable = [bool]$summary.environmentComparable
        environment = [PSCustomObject][ordered]@{
            targetPlatform = [string]$summary.environment.targetPlatform
            platformVersion = Get-EnvironmentText $summary.environment "platformVersion"
            deviceModel = Get-EnvironmentText $summary.environment "deviceModel"
            displaySize = Get-EnvironmentText $summary.environment "displaySize"
            displayDensity = Get-EnvironmentText $summary.environment "displayDensity"
            orientation = Get-EnvironmentText $summary.environment "orientation"
        }
        runs = $safeRuns
        checkpointComparisons = $safeCheckpoints
        newLayoutFindingKeys = @($summary.newLayoutFindingKeys | ForEach-Object {
            Get-BoundedText $_ "newLayoutFindingKeys" 512
        })
        errorCodes = $errorCodes
        warningCodes = $warningCodes
    }
}

$overall = if ($selection.selectionStatus -eq "no-ui-relevant-changes") { "not-applicable" } else { "inconclusive" }
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
    registrySha256 = [string]$selection.registrySha256
    selectionStatus = [string]$selection.selectionStatus
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
        "This result is advisory and is not a merge gate.",
        "No-difference-observed applies only to the measured scenarios, not the whole PR.",
        "A self-consistent seal checks local integrity and identity, not authenticity of third-party artifacts.",
        "Use independently trusted local context and captures; local execution does not establish isolated provenance.",
        "DevFlow layout and source locations identify symptoms, not framework causality.",
        "Windows runs are co-resident with the app under test, so absence of change remains inconclusive.",
        "Missing bundles were not measured; selection and coverage labels do not prove that changed behavior executed.",
        "Harness compatibility and smoke-scenario coverage limit interpretation; selectors are not repaired by this skill."
    )
}
Write-UiEvidenceJson $agentSummary $OutputPath
