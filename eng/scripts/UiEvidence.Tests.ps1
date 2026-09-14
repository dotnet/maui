#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$selector = Join-Path $PSScriptRoot "Select-UiEvidenceScenarios.ps1"
$requestBuilder = Join-Path $PSScriptRoot "New-UiEvidenceRequests.ps1"
$requestManifestBuilder = Join-Path $PSScriptRoot "New-UiEvidenceRequestManifest.ps1"
$payloadBuilder = Join-Path $PSScriptRoot "Prepare-UiEvidencePayload.ps1"
$sealer = Join-Path $PSScriptRoot "Seal-UiEvidence.ps1"
$validator = Join-Path $PSScriptRoot "Validate-UiEvidenceBundle.ps1"
$registry = Join-Path $repoRoot "eng\ui-evidence\scenarios.json"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-ui-evidence-tests-" + [Guid]::NewGuid().ToString("N"))
$baseSha = "1111111111111111111111111111111111111111"
$headSha = "2222222222222222222222222222222222222222"
$harnessSha = "3333333333333333333333333333333333333333"

function Assert-Equal($Expected, $Actual, [string]$Message) {
    if ($Expected -ne $Actual) {
        throw "$Message. Expected '$Expected', actual '$Actual'."
    }
}

function Assert-True([bool]$Condition, [string]$Message) {
    if (-not $Condition) {
        throw $Message
    }
}

function Invoke-ScriptProcess(
    [string]$Script,
    [string[]]$Arguments,
    [switch]$Quiet
) {
    if ($Quiet) {
        & pwsh -NoProfile -File $Script @Arguments *> $null
    }
    else {
        & pwsh -NoProfile -File $Script @Arguments | Out-Host
    }
    return $LASTEXITCODE
}

function Write-ChangedFiles([string]$Name, [string[]]$Files) {
    $path = Join-Path $testRoot "$Name.txt"
    $Files | Set-Content -LiteralPath $path -Encoding UTF8
    return $path
}

function Invoke-Selection([string]$Name, [string[]]$Files) {
    $changedPath = Write-ChangedFiles $Name $Files
    $outputPath = Join-Path $testRoot "$Name-selection.json"
    $exitCode = Invoke-ScriptProcess $selector @(
        "-ChangedFilesPath", $changedPath,
        "-BaseCommitSha", $baseSha,
        "-HeadCommitSha", $headSha,
        "-HarnessSha", $harnessSha,
        "-PullRequestNumber", "42",
        "-RegistryPath", $registry,
        "-OutputPath", $outputPath
    )
    return [PSCustomObject]@{
        ExitCode = $exitCode
        Result = Get-Content -LiteralPath $outputPath -Raw | ConvertFrom-Json
        Path = $outputPath
    }
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null

try {
    $pipelinePath = Join-Path $repoRoot "eng\pipelines\ci-ui-evidence.yml"
    $pipeline = Get-Content -LiteralPath $pipelinePath -Raw
    Assert-True ($pipeline -match '(?m)^trigger: none\r?$') "UI evidence pipeline must remain manually triggered"
    Assert-True ($pipeline -match '(?m)^pr: none\r?$') "UI evidence pipeline must not run automatically on PRs"
    foreach ($path in @(
        $pipelinePath,
        (Join-Path $repoRoot "eng\pipelines\common\ui-evidence-build-job.yml"),
        (Join-Path $repoRoot "eng\pipelines\common\ui-evidence-run-job.yml")
    )) {
        $content = Get-Content -LiteralPath $path -Raw
        Assert-True ($content -notmatch '\.github[/\\]skills[/\\]ui-evidence') "Measurement pipelines must not depend on the optional AI skill: $path"
        Assert-True ($content -notmatch '\bgh aw\b') "Measurement pipelines must not invoke an agentic workflow: $path"
    }

    $layout = Invoke-Selection "layout" @(
        "src/Controls/src/Core/Layout/Grid.cs"
    )
    Assert-Equal 0 $layout.ExitCode "Layout selection should succeed"
    Assert-Equal "ready" $layout.Result.selectionStatus "Layout selection status"
    Assert-Equal "complete" $layout.Result.coverage.status "Layout coverage"
    Assert-Equal 2 @($layout.Result.requests).Count "Layout platform request count"
    Assert-Equal "layout-controls-smoke" $layout.Result.requests[0].scenarioId "Layout scenario"

    $mixedCoverage = Invoke-Selection "mixed-coverage" @(
        "src/Controls/src/Core/Layout/Grid.cs",
        "src/Core/src/Handlers/Layout/LayoutHandler.cs"
    )
    Assert-Equal 0 $mixedCoverage.ExitCode "Mixed coverage selection should produce requests"
    Assert-Equal "partial" $mixedCoverage.Result.coverage.status "Sampled files must prevent complete coverage"
    Assert-Equal "sampled" $mixedCoverage.Result.requests[0].coverage "Least-complete request coverage must win"

    $collection = Invoke-Selection "collection" @(
        "src/Controls/src/Core/Handlers/Items/CollectionViewHandler.Android.cs"
    )
    Assert-Equal 0 $collection.ExitCode "Collection selection should succeed"
    Assert-Equal "collectionview-smoke" $collection.Result.requests[0].scenarioId "Collection scenario"

    $androidCollection = Invoke-Selection "collection-android" @(
        "src/Controls/src/Core/Handlers/Items/Android/SpacingItemDecoration.cs"
    )
    Assert-Equal 0 $androidCollection.ExitCode "Android CollectionView selection should succeed"
    Assert-True ($androidCollection.Result.requests -is [array]) "Single selection request must remain a JSON array"
    Assert-Equal 1 @($androidCollection.Result.requests).Count "Android-specific request count"
    Assert-Equal "android" $androidCollection.Result.requests[0].platform "Android-specific request platform"
    Assert-Equal "complete" $androidCollection.Result.coverage.status "Android-specific coverage"

    $androidRequestsPath = Join-Path $testRoot "collection-android-requests.json"
    $androidRequestExit = Invoke-ScriptProcess $requestBuilder @(
        "-SelectionPath", $androidCollection.Path,
        "-OutputPath", $androidRequestsPath
    )
    Assert-Equal 0 $androidRequestExit "Android request generation should succeed"
    $androidRequestsJson = Get-Content -LiteralPath $androidRequestsPath -Raw
    Assert-True ($androidRequestsJson.TrimStart().StartsWith("[")) "Single generated request must remain a JSON array"
    $androidRequests = $androidRequestsJson | ConvertFrom-Json
    Assert-Equal 1 @($androidRequests).Count "Android generated request count"

    $windowsCollection = Invoke-Selection "collection-windows" @(
        "src/Controls/src/Core/Handlers/Items/CollectionViewHandler.Windows.cs"
    )
    Assert-Equal 0 $windowsCollection.ExitCode "Windows CollectionView selection should succeed"
    Assert-Equal 1 @($windowsCollection.Result.requests).Count "Windows-specific request count"
    Assert-Equal "windows" $windowsCollection.Result.requests[0].platform "Windows-specific request platform"

    $unsupportedCollection = Invoke-Selection "collection-ios" @(
        "src/Controls/src/Core/Handlers/Items/iOS/ItemsViewController.cs"
    )
    Assert-Equal 3 $unsupportedCollection.ExitCode "Unsupported platform selection should be incomplete"
    Assert-Equal "no-trusted-scenario" $unsupportedCollection.Result.selectionStatus "Unsupported platform selection status"
    Assert-Equal 1 $unsupportedCollection.Result.coverage.unmappedFileCount "Unsupported platform file count"

    $unmapped = Invoke-Selection "unmapped" @(
        "src/Controls/src/Core/UnknownVisualFeature.cs"
    )
    Assert-Equal 3 $unmapped.ExitCode "Unmapped selection should be incomplete"
    Assert-Equal "no-trusted-scenario" $unmapped.Result.selectionStatus "Unmapped selection status"
    Assert-Equal "partial" $unmapped.Result.coverage.status "Unmapped coverage"

    $docs = Invoke-Selection "docs" @(
        "docs/some-file.md"
    )
    Assert-Equal 3 $docs.ExitCode "Non-product selection should be a no-op"
    Assert-Equal "no-ui-relevant-changes" $docs.Result.selectionStatus "Non-product status"

    $requestsPath = Join-Path $testRoot "requests.json"
    $requestExit = Invoke-ScriptProcess $requestBuilder @(
        "-SelectionPath", $layout.Path,
        "-OutputPath", $requestsPath
    )
    Assert-Equal 0 $requestExit "Request generation should succeed"
    $requests = Get-Content -LiteralPath $requestsPath -Raw | ConvertFrom-Json
    Assert-Equal 2 @($requests).Count "Request generation count"
    Assert-True ([string]$requests[0].requestKey -match '^maui-ui-[0-9a-f]{24}$') "Request key format"
    Assert-Equal "base-1" $requests[0].expectedRunOrder[0] "Run order start"
    Assert-Equal "base-2" $requests[0].expectedRunOrder[3] "Run order end"

    $repeatRequestsPath = Join-Path $testRoot "requests-repeat.json"
    $repeatExit = Invoke-ScriptProcess $requestBuilder @(
        "-SelectionPath", $layout.Path,
        "-OutputPath", $repeatRequestsPath
    )
    Assert-Equal 0 $repeatExit "Repeated request generation should succeed"
    $repeatRequests = Get-Content -LiteralPath $repeatRequestsPath -Raw | ConvertFrom-Json
    Assert-Equal $requests[0].requestKey $repeatRequests[0].requestKey "Request key should be stable"

    $requestManifestPath = Join-Path $testRoot "request-manifest.json"
    $requestManifestExit = Invoke-ScriptProcess $requestManifestBuilder @(
        "-RequestKey", ([string]$requests[0].requestKey),
        "-PullRequestNumber", "42",
        "-BaseCommitSha", $baseSha,
        "-HeadCommitSha", $headSha,
        "-HarnessSha", $harnessSha,
        "-RegistrySha256", ([string]$layout.Result.registrySha256),
        "-ScenarioId", ([string]$requests[0].scenarioId),
        "-Platform", ([string]$requests[0].platform),
        "-Coverage", ([string]$requests[0].coverage),
        "-OutputPath", $requestManifestPath
    )
    Assert-Equal 0 $requestManifestExit "Request manifest generation should succeed"
    $requestManifest = Get-Content -LiteralPath $requestManifestPath -Raw | ConvertFrom-Json
    Assert-Equal $requests[0].requestKey $requestManifest.requestKey "Request manifest key"

    $invalidRequestPath = Join-Path $testRoot "invalid-request.json"
    $invalidRequestExit = Invoke-ScriptProcess $requestManifestBuilder @(
        "-RequestKey", "maui-ui-000000000000000000000000",
        "-PullRequestNumber", "42",
        "-BaseCommitSha", $baseSha,
        "-HeadCommitSha", $headSha,
        "-HarnessSha", $harnessSha,
        "-RegistrySha256", ([string]$layout.Result.registrySha256),
        "-ScenarioId", ([string]$requests[0].scenarioId),
        "-Platform", ([string]$requests[0].platform),
        "-Coverage", ([string]$requests[0].coverage),
        "-OutputPath", $invalidRequestPath
    ) -Quiet
    Assert-True ($invalidRequestExit -ne 0) "Invalid request identity should fail"

    $baseArtifacts = Join-Path $testRoot "payload-base"
    $headArtifacts = Join-Path $testRoot "payload-head"
    $baseAppRoot = Join-Path $baseArtifacts "app"
    $headAppRoot = Join-Path $headArtifacts "app"
    New-Item -ItemType Directory -Force -Path $baseAppRoot, $headAppRoot | Out-Null
    "base-app" | Set-Content -LiteralPath (Join-Path $baseAppRoot "app.exe")
    "head-app" | Set-Content -LiteralPath (Join-Path $headAppRoot "app.exe")
    foreach ($variant in @(
        @{ Name = "base"; Root = $baseArtifacts; App = (Join-Path $baseAppRoot "app.exe"); Commit = $baseSha },
        @{ Name = "head"; Root = $headArtifacts; App = (Join-Path $headAppRoot "app.exe"); Commit = $headSha }
    )) {
        $appHash = (Get-FileHash -LiteralPath $variant.App -Algorithm SHA256).Hash.ToLowerInvariant()
        $appLength = (Get-Item -LiteralPath $variant.App).Length
        $appInventory = "app.exe:${appLength}:$appHash"
        $appDirectoryHash = [Convert]::ToHexString(
            [Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($appInventory))
        ).ToLowerInvariant()
        @{
            schemaVersion = 1
            requestKey = $requestManifest.requestKey
            variant = $variant.Name
            commitSha = $variant.Commit
            harnessSha = $harnessSha
            scenarioId = $requestManifest.scenarioId
            platform = $requestManifest.platform
            appRootRelativePath = "app"
            appRelativePath = "app/app.exe"
            appSha256 = $appHash
            appDirectorySha256 = $appDirectoryHash
            appFiles = @(@{
                relativePath = "app.exe"
                sizeBytes = $appLength
                sha256 = $appHash
            })
            devFlowCommit = ("4" * 40)
            devFlowPackageVersion = "0.1.0-ui.test"
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $variant.Root "ui-evidence-build-metadata.json")
    }
    $fakeFeed = Join-Path $testRoot "fake-feed"
    New-Item -ItemType Directory -Force -Path $fakeFeed | Out-Null
    @{
        schemaVersion = 1
        commit = ("4" * 40)
        packageVersion = "0.1.0-ui.test"
    } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $fakeFeed "devflow-manifest.json")
    "package" | Set-Content -LiteralPath (Join-Path $fakeFeed "driver.nupkg")
    $payloadPath = Join-Path $testRoot "payload"
    $payloadExit = Invoke-ScriptProcess $payloadBuilder @(
        "-BaseArtifacts", $baseArtifacts,
        "-HeadArtifacts", $headArtifacts,
        "-RequestPath", $requestManifestPath,
        "-RegistryPath", $registry,
        "-DevFlowFeed", $fakeFeed,
        "-OutputDirectory", $payloadPath
    )
    Assert-Equal 0 $payloadExit "Payload preparation should succeed"
    $payloadManifest = Get-Content -LiteralPath (Join-Path $payloadPath "payload-manifest.json") -Raw | ConvertFrom-Json
    Assert-Equal "base/app/app.exe" $payloadManifest.baseAppRelativePath "Payload base app path"
    Assert-Equal "head/app/app.exe" $payloadManifest.headAppRelativePath "Payload head app path"

    $bundleRoot = Join-Path $testRoot "bundle"
    New-Item -ItemType Directory -Force -Path $bundleRoot | Out-Null
    $requestPath = Join-Path $bundleRoot "request.json"
    $requests[0] | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $requestPath -Encoding UTF8
    @{
        schemaVersion = 1
        verdict = "no-difference-observed"
    } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $bundleRoot "comparison-summary.json") -Encoding UTF8

    $sealExit = Invoke-ScriptProcess $sealer @(
        "-Root", $bundleRoot,
        "-RequestManifestPath", $requestPath
    )
    Assert-Equal 0 $sealExit "Bundle sealing should succeed"
    Assert-True (Test-Path (Join-Path $bundleRoot "evidence-seal.json")) "Seal should exist"

    $validationPath = Join-Path $testRoot "validation.json"
    $validateExit = Invoke-ScriptProcess $validator @(
        "-Root", $bundleRoot,
        "-ExpectedRequestKey", ([string]$requests[0].requestKey),
        "-ExpectedHeadCommitSha", $headSha,
        "-OutputPath", $validationPath
    )
    Assert-Equal 0 $validateExit "Sealed bundle should validate"
    $validation = Get-Content -LiteralPath $validationPath -Raw | ConvertFrom-Json
    Assert-Equal $true $validation.valid "Validation result"

    Add-Content -LiteralPath (Join-Path $bundleRoot "comparison-summary.json") -Value "tampered"
    $tamperedPath = Join-Path $testRoot "tampered-validation.json"
    $tamperedExit = Invoke-ScriptProcess $validator @(
        "-Root", $bundleRoot,
        "-OutputPath", $tamperedPath
    ) -Quiet
    Assert-Equal 2 $tamperedExit "Tampered bundle should fail validation"
    $tampered = Get-Content -LiteralPath $tamperedPath -Raw | ConvertFrom-Json
    Assert-Equal $false $tampered.valid "Tampered validation result"

    Write-Host "All UI evidence contract tests passed."
}
finally {
    Remove-Item -LiteralPath $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
