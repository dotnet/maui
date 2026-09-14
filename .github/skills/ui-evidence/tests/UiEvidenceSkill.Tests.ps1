#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$skillRoot = Split-Path -Parent $PSScriptRoot
$repoRoot = [IO.Path]::GetFullPath((Join-Path $skillRoot "..\..\.."))
$coreScripts = Join-Path $repoRoot "eng\scripts"
. (Join-Path $coreScripts "UiEvidence.Common.ps1")
$summaryScript = Join-Path $skillRoot "scripts\Build-UiEvidenceAgentSummary.ps1"
$reportValidator = Join-Path $skillRoot "scripts\Validate-UiEvidenceReport.ps1"
$bundleValidator = Join-Path $coreScripts "Validate-UiEvidenceBundle.ps1"
$sealer = Join-Path $coreScripts "Seal-UiEvidence.ps1"
$registryPath = Join-Path $repoRoot "eng\ui-evidence\scenarios.json"
$policy = Read-UiEvidenceJson (Join-Path $skillRoot "references\report-policy.json")
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-ui-evidence-skill-tests-" + [Guid]::NewGuid().ToString("N"))
$script:assertions = 0

function Assert-Equal($Expected, $Actual, [string]$Message) {
    $script:assertions++
    if ($Expected -cne $Actual) {
        throw "$Message. Expected '$Expected', actual '$Actual'."
    }
}

function Assert-True([bool]$Condition, [string]$Message) {
    $script:assertions++
    if (-not $Condition) {
        throw $Message
    }
}

function Invoke-TestProcess([string]$Script, [string[]]$Arguments) {
    $output = & pwsh -NoProfile -NonInteractive -File $Script @Arguments 2>&1 | Out-String
    $exitCode = $LASTEXITCODE
    return [PSCustomObject]@{
        ExitCode = $exitCode
        Output = $output
        Message = (($output -replace '\x1b\[[0-9;]*m', '') -replace '\s+', ' ')
    }
}

function Copy-JsonValue($Value) {
    return ConvertTo-Json -InputObject $Value -Depth 32 | ConvertFrom-Json
}

function New-TestSelection([string]$Name, [string[]]$Files, [string]$Registry = $registryPath) {
    $changedPath = Join-Path $testRoot "$Name-changed.txt"
    $path = Join-Path $testRoot "$Name-selection.json"
    $Files | Set-Content -LiteralPath $changedPath -Encoding UTF8
    $process = Invoke-TestProcess (Join-Path $coreScripts "Select-UiEvidenceScenarios.ps1") @(
        "-ChangedFilesPath", $changedPath,
        "-BaseCommitSha", ("1" * 40), "-HeadCommitSha", ("2" * 40),
        "-HarnessSha", ("3" * 40), "-PullRequestNumber", "42",
        "-RegistryPath", $Registry, "-OutputPath", $path
    )
    Assert-True ($process.ExitCode -in @(0, 3)) "Core selection failed: $($process.Output)"
    $selection = Read-UiEvidenceJson $path
    if ($process.ExitCode -eq 0) {
        $requestsPath = Join-Path $testRoot "$Name-requests.json"
        $requestsProcess = Invoke-TestProcess (Join-Path $coreScripts "New-UiEvidenceRequests.ps1") @(
            "-SelectionPath", $path, "-OutputPath", $requestsPath
        )
        Assert-Equal 0 $requestsProcess.ExitCode "Core request generation failed: $($requestsProcess.Output)"
        $selection.requests = @(Read-UiEvidenceJson $requestsPath)
        Write-UiEvidenceJson $selection $path
    }
    return $selection
}

function New-TestBundle([string]$Root, $Request, [string]$Verdict) {
    $bundle = Join-Path $Root $Request.requestKey
    New-Item -ItemType Directory -Force -Path $bundle | Out-Null
    Write-UiEvidenceJson $Request (Join-Path $bundle "request.json")
    Copy-Item -LiteralPath $registryPath -Destination (Join-Path $bundle "scenarios.json")
    $environment = [ordered]@{
        targetPlatform = $Request.platform
        platformVersion = if ($Request.platform -eq "android") { "35" } else { "10.0" }
        deviceModel = "fixture-device"
        displaySize = "1080x2400"
        displayDensity = "420"
        orientation = "portrait"
        hostOperatingSystem = "fixture-host"
        runtimeVersion = "10.0"
        machineName = "private-machine-not-for-model"
        deviceId = "private-device-not-for-model"
        appiumUrl = "http://127.0.0.1:4723/private-not-for-model"
    }
    $sequence = 0
    foreach ($item in @(
        @{ Directory = "base-run1"; Variant = "base"; Ordinal = 1; Commit = $Request.baseCommitSha },
        @{ Directory = "head-run1"; Variant = "head"; Ordinal = 1; Commit = $Request.headCommitSha },
        @{ Directory = "head-run2"; Variant = "head"; Ordinal = 2; Commit = $Request.headCommitSha },
        @{ Directory = "base-run2"; Variant = "base"; Ordinal = 2; Commit = $Request.baseCommitSha }
    )) {
        $sequence++
        $runRoot = Join-Path $bundle $item.Directory
        $status = if ($Verdict -eq "head-functional-failure-advisory" -and $item.Variant -eq "head") {
            "scenario-failed"
        }
        else {
            "passed"
        }
        Write-UiEvidenceJson ([ordered]@{
            schemaVersion = 1
            requestKey = $Request.requestKey
            scenarioId = $Request.scenarioId
            platform = $Request.platform
            variant = $item.Variant
            variantRunOrdinal = $item.Ordinal
            sequenceOrdinal = $sequence
            commitSha = $item.Commit
            harnessSha = $Request.harnessSha
            status = $status
            startedAtUtc = "2026-01-01T00:00:00Z"
            finishedAtUtc = "2026-01-01T00:00:01Z"
            appArtifactSha256 = ("4" * 64)
            environment = $environment
            assertions = @(@{ id = "UiEvidenceReady"; status = $status })
            checkpoints = @()
            devFlow = @{ status = "not-requested"; layoutFindingKeys = @(); layoutStable = $false }
            errorCodes = @()
        }) (Join-Path $runRoot "run-result.json")
        "Synthetic raw capture; raw-ui-text-not-for-model." |
            Set-Content -LiteralPath (Join-Path $runRoot "capture.txt") -Encoding UTF8
    }
    $checkpointStatus = if ($Verdict -eq "visual-change-advisory") { "change-detected" } else { "no-difference-observed" }
    $difference = if ($Verdict -eq "visual-change-advisory") { 0.2 } else { 0 }
    Write-UiEvidenceJson ([ordered]@{
        schemaVersion = 1
        request = $Request
        provenanceValidated = $true
        environmentComparable = $true
        evidenceComplete = $Verdict -ne "head-functional-failure-advisory"
        environment = $environment
        trustLevel = if ($Request.platform -eq "android") { "isolated-emulator" } else { "co-resident-advisory" }
        verdict = $Verdict
        checkpointComparisons = @(@{
            checkpointId = "initial"
            status = $checkpointStatus
            baseIntraDifference = 0
            headIntraDifference = 0
            minimumCrossDifference = $difference
            maximumCrossDifference = $difference
            diffPath = "raw-diff-path-not-for-model.png"
        })
        newLayoutFindingKeys = @(if ($Verdict -eq "layout-change-advisory") { "overflow:UiEvidencePrimaryButton" })
        errors = @()
        warnings = @(if ($Request.platform -eq "windows" -and $Verdict -eq "inconclusive") {
            "windows-co-resident-no-difference-untrusted"
        })
    }) (Join-Path $bundle "comparison-summary.json")
    & $sealer -Root $bundle -RequestManifestPath (Join-Path $bundle "request.json") 6> $null
    return $bundle
}

function Copy-TestBundle([string]$Name) {
    $root = Join-Path $testRoot "$Name\bundles"
    New-Item -ItemType Directory -Force -Path $root | Out-Null
    $bundle = Join-Path $root $androidRequest.requestKey
    Copy-Item -LiteralPath $validBundle -Destination $bundle -Recurse
    return [PSCustomObject]@{ Root = $root; Bundle = $bundle }
}

function Seal-TestCopy([string]$Bundle) {
    & $sealer -Root $Bundle -RequestManifestPath (Join-Path $validBundle "request.json") 6> $null
}

function Invoke-SummaryCase([string]$Name, $Selection, [string]$Root, [string]$Failure = "") {
    $selectionPath = Join-Path $testRoot "$Name-input.json"
    $outputPath = Join-Path $testRoot "$Name-agent-summary.json"
    Write-UiEvidenceJson $Selection $selectionPath
    $process = Invoke-TestProcess $summaryScript @(
        "-SelectionPath", $selectionPath, "-BundlesRoot", $Root, "-OutputPath", $outputPath
    )
    if ($Failure) {
        Assert-True ($process.ExitCode -ne 0) "$Name must fail, not produce an empirical summary."
        Assert-True ($process.Message.Contains($Failure)) "$Name failed for the wrong reason: $($process.Output)"
        Assert-True (-not (Test-Path -LiteralPath $outputPath)) "$Name must not write a success-shaped output."
        return
    }
    Assert-Equal 0 $process.ExitCode "$Name should succeed: $($process.Output)"
    Assert-True (Test-Path -LiteralPath $outputPath -PathType Leaf) "$Name must write a local summary."
    return [PSCustomObject]@{ Path = $outputPath; Value = Read-UiEvidenceJson $outputPath }
}

function New-TestReport($Summary) {
    $rows = @(
        foreach ($result in $Summary.results) {
            $note = if ($result.bundleValidated) {
                "$($result.trustLevel); $($result.environment.deviceModel); see recorded run and checkpoint results."
            }
            else {
                "Missing sealed bundle; no measurement completed."
            }
            "| ``$($result.requestKey)`` | ``$($result.scenarioId)`` | ``$($result.platform)`` | ``$($result.verdict)`` | $note |"
        }
    ) -join "`n"
    return @"
## UI evidence analysis

**Empirical verdict:** ``$($Summary.overallVerdict)``

Head under review: ``$($Summary.measuredHeadSha)``

### Coverage

$($Summary.coverage.status); direct $($Summary.coverage.directFileCount), sampled $($Summary.coverage.sampledFileCount), unmapped $($Summary.coverage.unmappedFileCount).
Selection: $($Summary.selectionStatus). Zero evidence rows means no selected runs, not empirical coverage.

### Evidence

| Request | Scenario | Platform | Verdict | Evidence and limits |
| --- | --- | --- | --- | --- |
$rows

### Limitations

Only the listed completed scenarios have measurements. Local seals are not authentication.
This result is advisory and is not a merge gate.

$($policy.requiredFooter)
"@
}

function Invoke-ReportCase([string]$Name, [string]$Text, [string]$SummaryPath, [string]$Failure = "") {
    $path = Join-Path $testRoot "$Name-report.md"
    $Text | Set-Content -LiteralPath $path -Encoding UTF8
    $process = Invoke-TestProcess $reportValidator @("-ReportPath", $path, "-AgentSummaryPath", $SummaryPath)
    if ($Failure) {
        Assert-True ($process.ExitCode -ne 0) "$Name report must fail."
        Assert-True ($process.Message.Contains($Failure)) "$Name report failed for the wrong reason: $($process.Output)"
    }
    else {
        Assert-Equal 0 $process.ExitCode "$Name report should be locally valid: $($process.Output)"
        Assert-True ($process.Output.Contains("Nothing was posted.")) "Report validation must not imply publication."
    }
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null
try {
    $skill = Get-Content -LiteralPath (Join-Path $skillRoot "SKILL.md") -Raw
    Assert-True ($skill -match '\A---\r?\nname: ui-evidence\r?\n') "The skill must be frontmatter-discoverable."
    Assert-True ($skill -match '(?m)^description: .+\r?$') "The skill must declare its local purpose."
    Assert-True ($skill -match '(?m)^disable-model-invocation: true\r?$') "Interpretation must remain explicitly manual."
    Assert-Equal "ui-evidence" $policy.skillName "Policy skill identity"
    Assert-Equal "manual-local" $policy.invocation "Policy invocation"
    Assert-Equal "> Local advisory interpretation by the manually invoked **ui-evidence** skill." $policy.requiredFooter "Honest local footer"
    Assert-Equal "head-functional-failure-advisory,visual-change-advisory,layout-change-advisory,inconclusive,no-difference-observed,not-applicable" ($policy.verdictPrecedence -join ",") "Core verdict precedence must not change"
    foreach ($workflow in @("ui-evidence.md", "ui-evidence.lock.yml")) {
        Assert-True (-not (Test-Path -LiteralPath (Join-Path $repoRoot ".github\workflows\$workflow"))) "No replacement or compiled UI evidence workflow is required."
    }
    $entrypoints = $skill + (Get-Content -LiteralPath $summaryScript -Raw) + (Get-Content -LiteralPath $reportValidator -Raw)
    Assert-True ($entrypoints -notmatch 'run_ui_evidence|post_ui_evidence_report|precompute-status\.json|BuildManifestPath|buildUrl|safe-outputs|gh aw|workflow_dispatch|suppress_output|evidence_run_id') "Local entrypoints must not retain publication or Azure enrichment machinery."

    $selection = New-TestSelection "android" @("src/Controls/src/Core/Handlers/Items/Android/SpacingItemDecoration.cs")
    Assert-Equal "ready" $selection.selectionStatus "Android selection"
    Assert-Equal 1 $selection.requests.Count "Single selected Android request"
    $androidRequest = $selection.requests[0]
    $bundlesRoot = Join-Path $testRoot "valid\bundles"
    $validBundle = New-TestBundle $bundlesRoot $androidRequest "no-difference-observed"
    $initialSealHash = Get-UiEvidenceSha256 (Join-Path $validBundle "evidence-seal.json")
    $coreValidation = Invoke-TestProcess $bundleValidator @(
        "-Root", $validBundle, "-ExpectedRequestKey", $androidRequest.requestKey,
        "-ExpectedHeadCommitSha", $selection.headCommitSha
    )
    Assert-Equal 0 $coreValidation.ExitCode "Synthetic fixture must have a genuine valid core seal: $($coreValidation.Output)"
    Assert-True (($coreValidation.Output | ConvertFrom-Json).valid) "Core validator must really accept the fixture."
    $valid = Invoke-SummaryCase "valid" $selection $bundlesRoot
    Assert-Equal "no-difference-observed" $valid.Value.overallVerdict "Direct stable Android evidence"
    Assert-Equal $true $valid.Value.results[0].bundleValidated "Present bundle must be validated"
    Assert-Equal 4 $valid.Value.results[0].runs.Count "All four run identities are retained"
    Assert-Equal $selection.registrySha256 $valid.Value.registrySha256 "Selected registry identity"
    Assert-True ($valid.Value.results -is [array]) "One result must remain a JSON array."
    $normalizedJson = Get-Content -LiteralPath $valid.Path -Raw
    Assert-True ($normalizedJson -notmatch 'private-.*-not-for-model|raw-ui-text-not-for-model|raw-diff-path-not-for-model|appiumUrl|buildUrl') "Raw/private fields and build URLs must not enter model input."
    Assert-True (($valid.Value.limitations -join " ").Contains("not authenticity of third-party artifacts")) "A valid seal must not be described as third-party authentication."

    $missing = Invoke-SummaryCase "missing" $selection (Join-Path $testRoot "absent-bundles")
    Assert-Equal "inconclusive" $missing.Value.overallVerdict "Entire missing bundle root"
    Assert-Equal 1 $missing.Value.results.Count "Missing requested bundle must not be dropped"
    Assert-Equal $androidRequest.requestKey $missing.Value.results[0].requestKey "Missing request identity"
    Assert-Equal "missing-sealed-bundle" $missing.Value.results[0].errorCodes[0] "Missing bundle diagnostic"
    Assert-Equal $false $missing.Value.results[0].evidenceComplete "Missing bundle is not evidence"
    Assert-Equal $false $missing.Value.results[0].bundleValidated "Missing bundle is not validated"
    $fileRoot = Join-Path $testRoot "not-a-bundle-root.txt"
    "not a directory" | Set-Content -LiteralPath $fileRoot
    Invoke-SummaryCase "file-root" $selection $fileRoot "BundlesRoot must be a local directory"

    $pairSelection = New-TestSelection "pair" @("src/Controls/src/Core/CollectionView.cs")
    Assert-Equal 2 $pairSelection.requests.Count "Paired platform selection"
    $windowsRequest = @($pairSelection.requests | Where-Object { $_.platform -eq "windows" })[0]
    $pairMissing = Invoke-SummaryCase "pair-missing" $pairSelection $bundlesRoot
    Assert-Equal "inconclusive" $pairMissing.Value.overallVerdict "Missing platform outranks observed absence"
    Assert-Equal 2 $pairMissing.Value.results.Count "All selected platforms remain in the summary"
    $visualRoot = Join-Path $testRoot "visual\bundles"
    $null = New-TestBundle $visualRoot $androidRequest "visual-change-advisory"
    $visualMissing = Invoke-SummaryCase "visual-missing" $pairSelection $visualRoot
    Assert-Equal "visual-change-advisory" $visualMissing.Value.overallVerdict "Positive advisory retains precedence over missing evidence"
    Assert-Equal "inconclusive" $visualMissing.Value.results[1].verdict "Positive advisory must not hide a missing platform"

    $partialSelection = New-TestSelection "partial" @(
        "src/Controls/src/Core/Handlers/Items/Android/SpacingItemDecoration.cs",
        "src/Controls/src/Core/UnknownVisualFeature.cs"
    )
    $partial = Invoke-SummaryCase "partial" $partialSelection $bundlesRoot
    Assert-Equal "inconclusive" $partial.Value.overallVerdict "Partial coverage suppresses overall absence"
    Assert-Equal "no-difference-observed" $partial.Value.results[0].verdict "Aggregation must not rewrite a measured scenario verdict"
    Assert-Equal 1 $partial.Value.coverage.unmappedFileCount "Unmapped coverage stays explicit"

    foreach ($pair in @(
        @("head-functional-failure-advisory", "visual-change-advisory"),
        @("visual-change-advisory", "layout-change-advisory"),
        @("layout-change-advisory", "inconclusive")
    )) {
        $name = "precedence-$($pair[0])"
        $root = Join-Path $testRoot "$name\bundles"
        $null = New-TestBundle $root $androidRequest $pair[0]
        $null = New-TestBundle $root $windowsRequest $pair[1]
        $result = Invoke-SummaryCase $name $pairSelection $root
        Assert-Equal $pair[0] $result.Value.overallVerdict "Deterministic advisory precedence"
    }
    $windowsRoot = Join-Path $testRoot "windows\bundles"
    $null = New-TestBundle $windowsRoot $androidRequest "no-difference-observed"
    $null = New-TestBundle $windowsRoot $windowsRequest "inconclusive"
    $windows = Invoke-SummaryCase "windows" $pairSelection $windowsRoot
    Assert-Equal "inconclusive" $windows.Value.overallVerdict "Co-resident Windows absence cannot become a clean combined result"
    Assert-Equal "windows-co-resident-no-difference-untrusted" $windows.Value.results[1].warningCodes[0] "Core warnings must remain visible"
    $sampledSelection = New-TestSelection "sampled" @("src/Core/src/Handlers/Layout/LayoutHandler.Android.cs")
    Assert-Equal "sampled-only" $sampledSelection.coverage.status "Sampled fixture coverage"
    $sampledRoot = Join-Path $testRoot "sampled\bundles"
    $null = New-TestBundle $sampledRoot $sampledSelection.requests[0] "inconclusive"
    $sampled = Invoke-SummaryCase "sampled" $sampledSelection $sampledRoot
    Assert-Equal "inconclusive" $sampled.Value.overallVerdict "Sampled scenario remains inconclusive"

    $unmappedSelection = New-TestSelection "unmapped" @("src/Controls/src/Core/UnknownVisualFeature.cs")
    $unmapped = Invoke-SummaryCase "unmapped" $unmappedSelection (Join-Path $testRoot "no-runs")
    Assert-Equal "no-trusted-scenario" $unmapped.Value.selectionStatus "Unknown UI source has no trusted mapping"
    Assert-Equal "inconclusive" $unmapped.Value.overallVerdict "Unknown UI source without runs is not not-applicable"
    Assert-Equal 0 $unmapped.Value.results.Count "Do not invent runs for unknown UI source"
    $unmappedSelection.requests = @()
    $unmappedArray = Invoke-SummaryCase "unmapped-array" $unmappedSelection (Join-Path $testRoot "no-runs")
    Assert-Equal "inconclusive" $unmappedArray.Value.overallVerdict "Manual context with an explicit empty request array"
    $unsupportedSelection = New-TestSelection "unsupported" @("src/Controls/src/Core/Handlers/Items/iOS/ItemsViewController.cs")
    $unsupported = Invoke-SummaryCase "unsupported" $unsupportedSelection (Join-Path $testRoot "no-runs")
    Assert-Equal "inconclusive" $unsupported.Value.overallVerdict "Unsupported platform is not measured on another platform"
    $noUiSelection = New-TestSelection "no-ui" @("docs/example.md")
    $noUi = Invoke-SummaryCase "no-ui" $noUiSelection (Join-Path $testRoot "no-runs")
    Assert-Equal "not-applicable" $noUi.Value.overallVerdict "No UI-relevant changes can be not-applicable"
    Assert-Equal 0 $noUi.Value.results.Count "No UI-relevant changes must not imply measured runs"
    $smallRegistryPath = Join-Path $testRoot "small-registry.json"
    $smallRegistry = Read-UiEvidenceJson $registryPath
    $smallRegistry.limits.maxScenarioPlatformPairs = 1
    Write-UiEvidenceJson $smallRegistry $smallRegistryPath
    $overflowSelection = New-TestSelection "overflow" @("src/Controls/src/Core/CollectionView.cs") $smallRegistryPath
    $overflow = Invoke-SummaryCase "overflow" $overflowSelection (Join-Path $testRoot "no-runs")
    Assert-Equal "selection-overflow" $overflow.Value.selectionStatus "Bounded selection status"
    Assert-Equal "inconclusive" $overflow.Value.overallVerdict "Overflow without runs is inconclusive"

    $modified = Copy-TestBundle "modified-comparison"
    "{ not valid JSON; must fail the seal before parsing" |
        Set-Content -LiteralPath (Join-Path $modified.Bundle "comparison-summary.json")
    Invoke-SummaryCase "modified-comparison" $selection $modified.Root "Sealed UI evidence bundle validation failed"
    $modifiedRun = Copy-TestBundle "modified-capture"
    Add-Content -LiteralPath (Join-Path $modifiedRun.Bundle "head-run1\capture.txt") -Value "changed capture"
    Invoke-SummaryCase "modified-capture" $selection $modifiedRun.Root "Sealed UI evidence bundle validation failed"
    $unsealed = Copy-TestBundle "unsealed-file"
    "unexpected" | Set-Content -LiteralPath (Join-Path $unsealed.Bundle "extra.txt")
    Invoke-SummaryCase "unsealed-file" $selection $unsealed.Root "Sealed UI evidence bundle validation failed"
    foreach ($file in @("evidence-seal.json", "request.json", "comparison-summary.json", "base-run1\run-result.json")) {
        $name = "missing-file-" + ($file -replace '[\\.]', '-')
        $copy = Copy-TestBundle $name
        Remove-Item -LiteralPath (Join-Path $copy.Bundle $file)
        Invoke-SummaryCase $name $selection $copy.Root "Sealed UI evidence bundle validation failed"
    }
    $incomplete = Copy-TestBundle "resealed-incomplete"
    Remove-Item -LiteralPath (Join-Path $incomplete.Bundle "base-run1\run-result.json")
    Seal-TestCopy $incomplete.Bundle
    Invoke-SummaryCase "resealed-incomplete" $selection $incomplete.Root "missing required sealed file"
    $emptyRoot = Join-Path $testRoot "empty\bundles"
    New-Item -ItemType Directory -Force -Path (Join-Path $emptyRoot $androidRequest.requestKey) | Out-Null
    Invoke-SummaryCase "empty" $selection $emptyRoot "Sealed UI evidence bundle validation failed"

    foreach ($field in @("repository", "pullRequestNumber", "baseCommitSha", "headCommitSha", "harnessSha", "registrySha256")) {
        $wrong = Copy-JsonValue $selection
        $wrong.$field = switch ($field) {
            "repository" { "dotnet/other" }
            "pullRequestNumber" { 43 }
            "registrySha256" { "a" * 64 }
            default { "a" * 40 }
        }
        Invoke-SummaryCase "wrong-selected-$field" $wrong $bundlesRoot "Selected request identity field '$field'"
    }
    foreach ($badKey in @("..\outside", "maui-ui-000000000000000000000000")) {
        $wrong = Copy-JsonValue $selection
        $wrong.requests[0].requestKey = $badKey
        $failure = if ($badKey.StartsWith("..")) { "invalid platform, coverage, or request key" } else { "request key is inconsistent or duplicated" }
        Invoke-SummaryCase ("wrong-key-" + $badKey.Length) $wrong $bundlesRoot $failure
    }
    $duplicate = Copy-JsonValue $selection
    $duplicate.requests = @($duplicate.requests[0], $duplicate.requests[0])
    Invoke-SummaryCase "duplicate-request" $duplicate $bundlesRoot "request key is inconsistent or duplicated"
    $badCoverage = Copy-JsonValue $selection
    $badCoverage.coverage.status = "none"
    Invoke-SummaryCase "wrong-coverage" $badCoverage $bundlesRoot "selection status, requests, and coverage are inconsistent"
    $badOrder = Copy-JsonValue $selection
    $badOrder.requests[0].expectedRunOrder = @("base-1", "base-2", "head-1", "head-2")
    Invoke-SummaryCase "wrong-order" $badOrder $bundlesRoot "core four-run contract"
    $badOrder.requests[0].expectedRunOrder = @("base-1,head-1,head-2,base-2")
    Invoke-SummaryCase "joined-order" $badOrder $bundlesRoot "core four-run contract"
    $badReady = Copy-JsonValue $selection
    $badReady.requests = $null
    Invoke-SummaryCase "ready-without-requests" $badReady $bundlesRoot "selection status or request array is invalid"
    $badRepository = Copy-JsonValue $noUiSelection
    $badRepository.repository = "not a repository"
    Invoke-SummaryCase "invalid-repository" $badRepository $bundlesRoot "selection provenance is invalid"

    foreach ($field in @("requestKey", "repository", "pullRequestNumber", "baseCommitSha", "headCommitSha", "harnessSha", "scenarioId", "platform")) {
        $copy = Copy-TestBundle "wrong-seal-$field"
        $path = Join-Path $copy.Bundle "evidence-seal.json"
        $seal = Read-UiEvidenceJson $path
        $seal.$field = switch ($field) {
            "requestKey" { "maui-ui-000000000000000000000000" }
            "repository" { "dotnet/other" }
            "pullRequestNumber" { 43 }
            "scenarioId" { "layout-controls-smoke" }
            "platform" { "windows" }
            default { "a" * 40 }
        }
        Write-UiEvidenceJson $seal $path
        $failure = if ($field -in @("requestKey", "headCommitSha")) { "Sealed UI evidence bundle validation failed" } else { "Evidence seal identity field '$field'" }
        Invoke-SummaryCase "wrong-seal-$field" $selection $copy.Root $failure
    }
    foreach ($document in @("request.json", "comparison-summary.json")) {
        foreach ($field in @("requestKey", "baseCommitSha", "headCommitSha", "harnessSha", "registrySha256", "scenarioId", "platform", "coverage")) {
            $name = "wrong-$document-$field"
            $copy = Copy-TestBundle $name
            $path = Join-Path $copy.Bundle $document
            $value = Read-UiEvidenceJson $path
            $request = if ($document -eq "request.json") { $value } else { $value.request }
            $request.$field = switch ($field) {
                "requestKey" { "maui-ui-000000000000000000000000" }
                "registrySha256" { "a" * 64 }
                "scenarioId" { "layout-controls-smoke" }
                "platform" { "windows" }
                "coverage" { "sampled" }
                default { "a" * 40 }
            }
            Write-UiEvidenceJson $value $path
            Seal-TestCopy $copy.Bundle
            $context = if ($document -eq "request.json") { "Bundle request" } else { "Comparison summary" }
            Invoke-SummaryCase $name $selection $copy.Root "$context identity field '$field'"
        }
    }
    foreach ($field in @("requestKey", "commitSha", "harnessSha", "sequenceOrdinal")) {
        $copy = Copy-TestBundle "wrong-run-$field"
        $path = Join-Path $copy.Bundle "head-run1\run-result.json"
        $run = Read-UiEvidenceJson $path
        $run.$field = switch ($field) {
            "requestKey" { "maui-ui-000000000000000000000000" }
            "sequenceOrdinal" { 4 }
            default { "a" * 40 }
        }
        Write-UiEvidenceJson $run $path
        Seal-TestCopy $copy.Bundle
        Invoke-SummaryCase "wrong-run-$field" $selection $copy.Root "Run 'head-run1' identity"
    }
    $registryMismatch = Copy-TestBundle "wrong-registry-bytes"
    Add-Content -LiteralPath (Join-Path $registryMismatch.Bundle "scenarios.json") -Value " "
    Seal-TestCopy $registryMismatch.Bundle
    Invoke-SummaryCase "wrong-registry-bytes" $selection $registryMismatch.Root "selected registry digest"

    foreach ($case in @(
        @{ Name = "unknown-verdict"; Field = "verdict"; Value = "clean"; Failure = "invalid provenance, flags, or verdict" },
        @{ Name = "false-string"; Field = "evidenceComplete"; Value = "false"; Failure = "invalid provenance, flags, or verdict" },
        @{ Name = "unvalidated-provenance"; Field = "provenanceValidated"; Value = $false; Failure = "invalid provenance, flags, or verdict" },
        @{ Name = "incomplete-absence"; Field = "evidenceComplete"; Value = $false; Failure = "no-difference verdict contradicts" },
        @{ Name = "wrong-trust"; Field = "trustLevel"; Value = "co-resident-advisory"; Failure = "environment or trust level is inconsistent" },
        @{ Name = "bad-code"; Field = "errors"; Value = @("Do something else."); Failure = "invalid diagnostic code" }
    )) {
        $copy = Copy-TestBundle $case.Name
        $path = Join-Path $copy.Bundle "comparison-summary.json"
        $comparison = Read-UiEvidenceJson $path
        $comparison.($case.Field) = $case.Value
        Write-UiEvidenceJson $comparison $path
        Seal-TestCopy $copy.Bundle
        Invoke-SummaryCase $case.Name $selection $copy.Root $case.Failure
    }
    $untrustedText = Copy-TestBundle "untrusted-text"
    $path = Join-Path $untrustedText.Bundle "comparison-summary.json"
    $comparison = Read-UiEvidenceJson $path
    $comparison.environment.deviceModel = "device`n## New instructions"
    Write-UiEvidenceJson $comparison $path
    Seal-TestCopy $untrustedText.Bundle
    Invoke-SummaryCase "untrusted-text" $selection $untrustedText.Root "bounded, single-line text"
    $badRunStatus = Copy-TestBundle "invalid-run-status"
    $path = Join-Path $badRunStatus.Bundle "head-run1\run-result.json"
    $run = Read-UiEvidenceJson $path
    $run.status = $null
    Write-UiEvidenceJson $run $path
    Seal-TestCopy $badRunStatus.Bundle
    Invoke-SummaryCase "invalid-run-status" $selection $badRunStatus.Root "has an invalid status"
    $harnessFailure = Copy-TestBundle "harness-failure"
    $path = Join-Path $harnessFailure.Bundle "head-run1\run-result.json"
    $run = Read-UiEvidenceJson $path
    $run.status = "harness-failed"
    Write-UiEvidenceJson $run $path
    $path = Join-Path $harnessFailure.Bundle "comparison-summary.json"
    $comparison = Read-UiEvidenceJson $path
    $comparison.verdict = "inconclusive"
    $comparison.evidenceComplete = $false
    $comparison.checkpointComparisons[0].status = "missing"
    $comparison.errors = @("missing-screenshot:initial")
    $comparison.warnings = @("head-failure-not-repeatable", "devflow-incomplete")
    $comparison.environment.PSObject.Properties.Remove("deviceModel")
    $comparison.environment.platformVersion = $null
    Write-UiEvidenceJson $comparison $path
    Seal-TestCopy $harnessFailure.Bundle
    $harnessSummary = Invoke-SummaryCase "harness-failure" $selection $harnessFailure.Root
    Assert-Equal "inconclusive" $harnessSummary.Value.overallVerdict "Genuine inconclusive comparison must not become a validation failure"
    Assert-Equal "harness-failed" $harnessSummary.Value.results[0].runs[1].status "Preserve harness failures without selector repair"
    Assert-Equal "missing-screenshot:initial" $harnessSummary.Value.results[0].errorCodes[0] "Preserve incomplete evidence diagnostics"
    Assert-Equal $null $harnessSummary.Value.results[0].environment.deviceModel "Optional absent environment identity stays unknown"
    Assert-Equal $null $harnessSummary.Value.results[0].environment.platformVersion "Optional null environment identity stays unknown"
    $unsafeSeal = Copy-TestBundle "unsafe-seal-path"
    $path = Join-Path $unsafeSeal.Bundle "evidence-seal.json"
    $seal = Read-UiEvidenceJson $path
    $seal.files[0].relativePath = "../outside.json"
    Write-UiEvidenceJson $seal $path
    Invoke-SummaryCase "unsafe-seal-path" $selection $unsafeSeal.Root "Sealed UI evidence bundle validation failed"

    $linked = Copy-TestBundle "linked-run"
    $linkPath = Join-Path $linked.Bundle "base-run1"
    Remove-Item -LiteralPath $linkPath -Recurse -Force
    $linkType = if ($IsWindows) { "Junction" } else { "SymbolicLink" }
    New-Item -ItemType $linkType -Path $linkPath -Target (Join-Path $validBundle "base-run1") | Out-Null
    try {
        Invoke-SummaryCase "linked-run" $selection $linked.Root "bundles cannot contain reparse points"
    }
    finally {
        Remove-Item -LiteralPath $linkPath -Force
    }
    $overwritePath = Join-Path $validBundle "comparison-summary.json"
    $inputPath = Join-Path $testRoot "overwrite-selection.json"
    Write-UiEvidenceJson $selection $inputPath
    $beforeOverwrite = Get-UiEvidenceSha256 $overwritePath
    $overwrite = Invoke-TestProcess $summaryScript @(
        "-SelectionPath", $inputPath, "-BundlesRoot", $bundlesRoot, "-OutputPath", $overwritePath
    )
    Assert-True ($overwrite.ExitCode -ne 0 -and $overwrite.Message.Contains("must not overwrite")) "Output path must not overwrite a sealed input."
    Assert-Equal $beforeOverwrite (Get-UiEvidenceSha256 $overwritePath) "Refused output must leave input unchanged"

    $report = New-TestReport $valid.Value
    $summaryHash = Get-UiEvidenceSha256 $valid.Path
    Invoke-ReportCase "valid" $report $valid.Path
    Invoke-ReportCase "missing" (New-TestReport $missing.Value) $missing.Path
    Invoke-ReportCase "unmapped" (New-TestReport $unmapped.Value) $unmapped.Path
    Invoke-ReportCase "no-ui" (New-TestReport $noUi.Value) $noUi.Path
    $mixedReport = New-TestReport $visualMissing.Value
    Invoke-ReportCase "mixed" $mixedReport $visualMissing.Path
    Invoke-ReportCase "changed-overall-verdict" ($report.Replace(
        "**Empirical verdict:** ``no-difference-observed``",
        "**Empirical verdict:** ``visual-change-advisory``"
    )) $valid.Path "does not preserve the deterministic empirical verdict"
    Invoke-ReportCase "duplicate-verdict" ($report.Replace(
        "### Coverage", "**Empirical verdict:** ``visual-change-advisory```n`n### Coverage"
    )) $valid.Path "does not preserve the deterministic empirical verdict"
    Invoke-ReportCase "changed-row-verdict" ($report.Replace(
        "| ``no-difference-observed`` |", "| ``visual-change-advisory`` |"
    )) $valid.Path "does not preserve the scenario identity and verdict"
    Invoke-ReportCase "changed-row-identity" ($report.Replace(
        "| ``collectionview-smoke`` |", "| ``layout-controls-smoke`` |"
    )) $valid.Path "does not preserve the scenario identity and verdict"
    $missingRowPattern = '(?m)^\| `' + [regex]::Escape($windowsRequest.requestKey) + '` \|[^\r\n]*\r?\n'
    Invoke-ReportCase "omitted-missing-row" ($mixedReport -replace $missingRowPattern, '') $visualMissing.Path "one evidence row for every selected request"
    Invoke-ReportCase "missing-section" ($report.Replace("### Coverage", "Coverage")) $valid.Path "missing required section"
    Invoke-ReportCase "missing-head" ($report.Replace($selection.headCommitSha, ("a" * 40))) $valid.Path "does not identify the measured head SHA"
    Invoke-ReportCase "old-footer" ($report.Replace(
        $policy.requiredFooter, "> Automated analysis by the **ui-evidence** agentic workflow."
    )) $valid.Path "required manual skill footer"
    Invoke-ReportCase "missing-disclaimer" ($report.Replace($policy.requiredDisclaimer, "")) $valid.Path "required advisory disclaimer"
    Invoke-ReportCase "oversized" (("x" * 20001) + $report) $valid.Path "exceeds the maximum length"
    $claimIndex = 0
    foreach ($claim in @(
        "This PR is safe to merge.",
        "Approved for merge.",
        "No regressions.",
        "No UI regressions.",
        "No-regression.",
        "This is a no-regression result.",
        "This pull request is regression-free.",
        "This PR is clean.",
        "The entire pull request looks clean.",
        "Whole PR: clean.",
        "This **PR** is **clean**.",
        "This PR is <strong>safe</strong> to merge.",
        "This PR is s&#97;fe to merge.",
        "The PR is merge-ready.",
        "Ready to merge.",
        "Merge this PR.",
        "An empirically clean result."
    )) {
        $claimIndex++
        $claimedReport = $report.Replace($policy.requiredDisclaimer, "$claim`n`n$($policy.requiredDisclaimer)")
        Invoke-ReportCase "prohibited-claim-$claimIndex" $claimedReport $valid.Path "prohibited clean or merge-safety claim"
    }
    Assert-Equal $summaryHash (Get-UiEvidenceSha256 $valid.Path) "Report checks must never modify the deterministic input"
    Assert-Equal $initialSealHash (Get-UiEvidenceSha256 (Join-Path $validBundle "evidence-seal.json")) "All fixture mutation tests must leave the original sealed bundle unchanged"
    $finalValidation = Invoke-TestProcess $bundleValidator @(
        "-Root", $validBundle, "-ExpectedRequestKey", $androidRequest.requestKey,
        "-ExpectedHeadCommitSha", $selection.headCommitSha
    )
    Assert-Equal 0 $finalValidation.ExitCode "Original bundle must still pass the core validator: $($finalValidation.Output)"
    Write-Host "All UI evidence skill tests passed ($script:assertions assertions; local synthetic fixtures, no devices or network)."
}
finally {
    if (Test-Path -LiteralPath $testRoot) {
        Remove-Item -LiteralPath $testRoot -Recurse -Force
    }
}
