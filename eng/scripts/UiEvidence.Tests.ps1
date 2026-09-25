#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$selector = Join-Path $PSScriptRoot "Select-UiEvidenceScenarios.ps1"
$requestBuilder = Join-Path $PSScriptRoot "New-UiEvidenceRequests.ps1"
$contextBuilder = Join-Path $PSScriptRoot "New-UiEvidenceContext.ps1"
$requestManifestBuilder = Join-Path $PSScriptRoot "New-UiEvidenceRequestManifest.ps1"
$payloadBuilder = Join-Path $PSScriptRoot "Prepare-UiEvidencePayload.ps1"
$sealer = Join-Path $PSScriptRoot "Seal-UiEvidence.ps1"
$validator = Join-Path $PSScriptRoot "Validate-UiEvidenceBundle.ps1"
$metadataBuilder = Join-Path $PSScriptRoot "New-UiEvidenceBuildMetadata.ps1"
$runBuilder = Join-Path $PSScriptRoot "Invoke-UiEvidenceRuns.ps1"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")
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

function Assert-ScriptFailure([string]$Script, [string[]]$Arguments, [string]$MessagePattern) {
    $text = (& pwsh -NoProfile -File $Script @Arguments 2>&1 | Out-String)
    Assert-True ($LASTEXITCODE -ne 0) "Expected failure from $Script"
    Assert-True ($text -match $MessagePattern) "Expected '$MessagePattern' from $Script, received: $text"
}

function New-HiddenFile([string]$Path) {
    "hidden evidence" | Set-Content -LiteralPath $Path
    if ($IsWindows) {
        [IO.File]::SetAttributes($Path, [IO.FileAttributes]::Hidden)
    }
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
    $skillPath = Join-Path $repoRoot ".github\skills\check-pr-ui-evidence\SKILL.md"
    $localWorkflowPath = Join-Path $repoRoot ".github\skills\check-pr-ui-evidence\references\local-workflow.md"
    Assert-True (Test-Path -LiteralPath $skillPath -PathType Leaf) "The manual measurement skill must exist"
    Assert-True (Test-Path -LiteralPath $localWorkflowPath -PathType Leaf) "The local workflow reference must exist"
    $skill = Get-Content -LiteralPath $skillPath -Raw
    Assert-True ($skill -match '(?m)^name: check-pr-ui-evidence\r?$') "The measurement skill must be discoverable"
    foreach ($path in @(
        (Join-Path $repoRoot "eng\pipelines\ci-ui-evidence.yml"),
        (Join-Path $repoRoot "eng\pipelines\common\ui-evidence-build-job.yml"),
        (Join-Path $repoRoot "eng\pipelines\common\ui-evidence-run-job.yml"),
        (Join-Path $repoRoot ".github\workflows\ui-evidence.md"),
        (Join-Path $repoRoot ".github\workflows\ui-evidence.lock.yml")
    )) {
        Assert-True (-not (Test-Path -LiteralPath $path)) "Manual UI evidence must not introduce a pipeline or workflow: $path"
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
    Assert-True ($docs.Result.requests -is [array]) "Empty selection requests must remain a JSON array"
    Assert-Equal 0 $docs.Result.requests.Count "Non-product selection must have no requests"

    foreach ($case in @(
        @{ Name = "layout"; Count = 2; ExitCode = 0 },
        @{ Name = "collection-android"; Count = 1; ExitCode = 0 },
        @{ Name = "docs"; Count = 0; ExitCode = 3 },
        @{ Name = "unmapped"; Count = 0; ExitCode = 3 }
    )) {
        $contextRoot = Join-Path $testRoot "$($case.Name)-context"
        $arguments = @(
            "-ChangedFilesPath", (Join-Path $testRoot "$($case.Name).txt"),
            "-BaseCommitSha", $baseSha,
            "-HeadCommitSha", $headSha,
            "-HarnessSha", $harnessSha,
            "-PullRequestNumber", "42",
            "-RegistryPath", $registry,
            "-OutputDirectory", $contextRoot
        )
        $contextExit = Invoke-ScriptProcess $contextBuilder $arguments
        Assert-Equal $case.ExitCode $contextExit "Local context exit code for $($case.Name)"
        $context = Get-Content -LiteralPath (Join-Path $contextRoot "selection.json") -Raw | ConvertFrom-Json
        Assert-True ($context.requests -is [array]) "Local context requests must always be an array"
        Assert-Equal $case.Count $context.requests.Count "Local context request count for $($case.Name)"
        $contextRequestsJson = Get-Content -LiteralPath (Join-Path $contextRoot "requests.json") -Raw
        Assert-True ($contextRequestsJson.TrimStart().StartsWith("[")) "Local requests file must always be a JSON array"
        $contextRequests = @($contextRequestsJson | ConvertFrom-Json)
        Assert-Equal $case.Count $contextRequests.Count "Standalone request count for $($case.Name)"
        Assert-Equal (Get-FileHash -LiteralPath $registry).Hash `
            (Get-FileHash -LiteralPath (Join-Path $contextRoot "scenarios.json")).Hash "Local registry snapshot"
        if ($case.Count -gt 0) {
            Assert-Equal $contextRequests[0].requestKey $context.requests[0].requestKey "Context must contain keyed requests"
            Assert-Equal $harnessSha $context.requests[0].harnessSha "Context must retain full request identity"
            Assert-Equal $headSha $context.requests[0].headCommitSha "Context must retain measured head"
        }
        $beforeHash = (Get-FileHash -LiteralPath (Join-Path $contextRoot "selection.json")).Hash
        $reuseExit = Invoke-ScriptProcess $contextBuilder $arguments -Quiet
        Assert-True ($reuseExit -ne 0) "Local context must not overwrite an existing evidence session"
        Assert-Equal $beforeHash (Get-FileHash -LiteralPath (Join-Path $contextRoot "selection.json")).Hash "Existing evidence must remain unchanged"
    }

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

    $baseMetadataFile = Join-Path $baseArtifacts "ui-evidence-build-metadata.json"
    $originalMetadata = Get-Content -LiteralPath $baseMetadataFile -Raw
    $payloadArguments = @(
        "-BaseArtifacts", $baseArtifacts, "-HeadArtifacts", $headArtifacts,
        "-RequestPath", $requestManifestPath, "-RegistryPath", $registry, "-DevFlowFeed", $fakeFeed
    )
    $outsideRoot = Join-Path $testRoot "outside"
    New-Item -ItemType Directory -Path $outsideRoot | Out-Null
    Copy-Item -LiteralPath (Join-Path $baseAppRoot "app.exe") -Destination (Join-Path $outsideRoot "app.exe")
    foreach ($case in @(
        @{ Name = "parent-root"; Root = "../outside"; App = "../outside/app.exe" },
        @{ Name = "absolute-root"; Root = $outsideRoot; App = (Join-Path $outsideRoot "app.exe") },
        @{ Name = "outside-app"; Root = "app"; App = "ui-evidence-build-metadata.json" }
    )) {
        $metadata = $originalMetadata | ConvertFrom-Json
        $metadata.appRootRelativePath = $case.Root
        $metadata.appRelativePath = $case.App
        Write-UiEvidenceJson $metadata $baseMetadataFile
        Assert-ScriptFailure $payloadBuilder ($payloadArguments + @(
            "-OutputDirectory", (Join-Path $testRoot $case.Name)
        )) "safe relative path|unsafe character"
    }
    Set-Content -LiteralPath $baseMetadataFile -Value $originalMetadata

    $hiddenAppFile = Join-Path $baseAppRoot ".injected"
    New-HiddenFile $hiddenAppFile
    Assert-ScriptFailure $payloadBuilder ($payloadArguments + @(
        "-OutputDirectory", (Join-Path $testRoot "hidden-payload")
    )) "unsealed file"
    Remove-Item -LiteralPath $hiddenAppFile -Force

    $link = Join-Path $baseAppRoot "linked"
    $linkType = if ($IsWindows) { "Junction" } else { "SymbolicLink" }
    New-Item -ItemType $linkType -Path $link -Target $outsideRoot | Out-Null
    try {
        Assert-ScriptFailure $payloadBuilder ($payloadArguments + @(
            "-OutputDirectory", (Join-Path $testRoot "linked-payload")
        )) "reparse point"
    }
    finally {
        [IO.Directory]::Delete($link)
    }

    $metadataArtifacts = Join-Path $testRoot "metadata-artifacts"
    $metadataApp = Join-Path $metadataArtifacts "win-x64"
    $hiddenDirectory = Join-Path $metadataApp ".assets"
    New-Item -ItemType Directory -Force -Path $hiddenDirectory | Out-Null
    "app" | Set-Content -LiteralPath (Join-Path $metadataApp "Controls.TestCases.HostApp.exe")
    New-HiddenFile (Join-Path $metadataApp ".hidden")
    New-HiddenFile (Join-Path $hiddenDirectory ".nested")
    if ($IsWindows) { [IO.File]::SetAttributes($hiddenDirectory, [IO.FileAttributes]::Hidden) }
    $windowsRequest = @($requests | Where-Object platform -eq "windows")[0]
    $windowsRequestPath = Join-Path $testRoot "windows-request.json"
    Write-UiEvidenceJson $windowsRequest $windowsRequestPath
    $metadataPath = Join-Path $testRoot "generated-metadata.json"
    Assert-Equal 0 (Invoke-ScriptProcess $metadataBuilder @(
        "-ArtifactRoot", $metadataArtifacts, "-RequestPath", $windowsRequestPath, "-Variant", "base",
        "-DevFlowManifestPath", (Join-Path $fakeFeed "devflow-manifest.json"), "-OutputPath", $metadataPath
    )) "Build metadata with hidden files must succeed"
    $generatedMetadata = Read-UiEvidenceJson $metadataPath
    Assert-Equal 3 @($generatedMetadata.appFiles).Count "App inventory must include hidden files and hidden directories"

    $sentinel = Join-Path $payloadPath "preserve.txt"
    "existing session" | Set-Content -LiteralPath $sentinel
    Assert-ScriptFailure $payloadBuilder ($payloadArguments + @("-OutputDirectory", $payloadPath)) "must be fresh"
    Assert-True (Test-Path -LiteralPath $sentinel) "Payload preparation must not delete an existing session"
    $runArguments = @(
        "-PayloadRoot", $payloadPath, "-RunnerPath", (Join-Path $testRoot "not-a-runner.dll"),
        "-DeviceId", "ui-evidence-test-target"
    )
    Assert-ScriptFailure $runBuilder ($runArguments + @("-OutputDirectory", $payloadPath)) "must be fresh"
    Assert-True (Test-Path -LiteralPath $sentinel) "Capture must not delete an existing session"
    Assert-ScriptFailure (Join-Path $PSScriptRoot "Complete-UiEvidenceComparison.ps1") @(
        "-RawEvidenceRoot", $payloadPath, "-PayloadRoot", $payloadPath,
        "-RunnerPath", (Join-Path $testRoot "not-a-runner.dll"), "-OutputDirectory", $payloadPath
    ) "must be fresh"
    Assert-True (Test-Path -LiteralPath $sentinel) "Comparison must not delete an existing session"
    Assert-ScriptFailure $payloadBuilder ($payloadArguments + @(
        "-OutputDirectory", (Join-Path $baseArtifacts "nested-output")
    )) "outside its input"
    Assert-ScriptFailure $runBuilder @(
        "-PayloadRoot", $payloadPath, "-RunnerPath", (Join-Path $testRoot "not-a-runner.dll"),
        "-OutputDirectory", (Join-Path $testRoot "missing-device")
    ) "requires an explicit DeviceId"

    $hiddenPayloadFile = Join-Path $payloadPath "base\app\.injected"
    New-HiddenFile $hiddenPayloadFile
    Assert-ScriptFailure $runBuilder ($runArguments + @(
        "-OutputDirectory", (Join-Path $testRoot "hidden-run"), "-LogDirectory", (Join-Path $testRoot "hidden-run-logs")
    )) "unsealed file"
    Remove-Item -LiteralPath $hiddenPayloadFile -Force

    $payloadManifestPath = Join-Path $payloadPath "payload-manifest.json"
    $originalPayload = Get-Content -LiteralPath $payloadManifestPath -Raw
    $payloadManifest.baseAppRelativePath = "../outside/app.exe"
    Write-UiEvidenceJson $payloadManifest $payloadManifestPath
    Assert-ScriptFailure $runBuilder ($runArguments + @(
        "-OutputDirectory", (Join-Path $testRoot "escaping-run"), "-LogDirectory", (Join-Path $testRoot "escaping-run-logs")
    )) "safe relative path"
    Set-Content -LiteralPath $payloadManifestPath -Value $originalPayload

    $payloadRegistry = Join-Path $payloadPath "scenarios.json"
    $originalRegistry = [IO.File]::ReadAllBytes($payloadRegistry)
    Add-Content -LiteralPath $payloadRegistry -Value " "
    Assert-ScriptFailure $runBuilder ($runArguments + @(
        "-OutputDirectory", (Join-Path $testRoot "changed-registry-run")
    )) "registry hash"
    [IO.File]::WriteAllBytes($payloadRegistry, $originalRegistry)

    $devFlowSource = Join-Path $testRoot "devflow-source"
    $sourceEntries = @()
    foreach ($relative in @(
        "src/DevFlow/Microsoft.Maui.DevFlow.Agent.Core/Microsoft.Maui.DevFlow.Agent.Core.csproj",
        "src/DevFlow/Microsoft.Maui.DevFlow.Agent.Core/DevFlowAgentService.cs",
        "src/DevFlow/Microsoft.Maui.DevFlow.Agent.Core/LayoutDiagnostics/VisualTreeWalker.LayoutDiagnostics.cs",
        ".hidden-source"
    )) {
        $path = Join-Path $devFlowSource $relative
        New-Item -ItemType Directory -Force -Path (Split-Path $path -Parent) | Out-Null
        New-HiddenFile $path
        $sourceEntries += @{ relativePath = $relative; sizeBytes = (Get-Item -LiteralPath $path -Force).Length; sha256 = Get-UiEvidenceSha256 $path }
    }
    Write-UiEvidenceJson @{
        schemaVersion = 1; commit = $harnessSha
        compatibility = @{ dependencyBaseline = "dotnet-maui"; windowsNativeUiAutomationDisabled = $true; sourceMauiProjectReferencesEnabled = $true }
        files = $sourceEntries
    } (Join-Path $devFlowSource "devflow-source-manifest.json")
    $sourceValidator = Join-Path $PSScriptRoot "Validate-UiEvidenceDevFlowSource.ps1"
    Assert-Equal 0 (Invoke-ScriptProcess $sourceValidator @(
        "-SourceDirectory", $devFlowSource, "-ExpectedCommit", $harnessSha
    )) "Hidden source files must validate when inventoried"
    New-HiddenFile (Join-Path $devFlowSource ".injected")
    Assert-ScriptFailure $sourceValidator @(
        "-SourceDirectory", $devFlowSource, "-ExpectedCommit", $harnessSha
    ) "unsealed file"

    $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
    $listener.Start()
    try {
        $port = $listener.LocalEndpoint.Port
        Assert-ScriptFailure (Join-Path $PSScriptRoot "Start-UiEvidenceAppium.ps1") @(
            "-Port", "$port", "-StatePath", (Join-Path $testRoot "appium-state.json"),
            "-LogPath", (Join-Path $testRoot "appium.log")
        ) "port $port is unavailable"
    }
    finally {
        $listener.Stop()
    }

    $owned = Start-Process pwsh -ArgumentList "-NoProfile", "-NonInteractive", "-Command", "Start-Sleep -Seconds 60" -PassThru
    try {
        $statePath = Join-Path $testRoot "owned-process.json"
        $state = @{
            schemaVersion = 2; started = $true; processId = $owned.Id
            processStartedAtUtc = $owned.StartTime.ToUniversalTime().AddMinutes(1).ToString("O")
        }
        Write-UiEvidenceJson $state $statePath
        Assert-ScriptFailure (Join-Path $PSScriptRoot "Stop-UiEvidenceAppium.ps1") @(
            "-StatePath", $statePath
        ) "identity changed"
        Assert-True (-not $owned.HasExited) "A mismatched process identity must not be terminated"
        $state.processStartedAtUtc = $owned.StartTime.ToUniversalTime().ToString("O")
        Write-UiEvidenceJson $state $statePath
        Assert-Equal 0 (Invoke-ScriptProcess (Join-Path $PSScriptRoot "Stop-UiEvidenceAppium.ps1") @(
            "-StatePath", $statePath
        )) "The exact owned process must be stopped"
        Assert-True ($owned.WaitForExit(10000)) "Owned process cleanup must finish"
    }
    finally {
        if (-not $owned.HasExited) { $owned.Kill($true); $owned.WaitForExit() }
        $owned.Dispose()
    }

    $bundleRoot = Join-Path $testRoot "bundle"
    New-Item -ItemType Directory -Force -Path $bundleRoot | Out-Null
    $requestPath = Join-Path $bundleRoot "request.json"
    $requests[0] | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $requestPath -Encoding UTF8
    @{
        schemaVersion = 1
        verdict = "no-difference-observed"
    } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $bundleRoot "comparison-summary.json") -Encoding UTF8
    $hiddenBundleFile = Join-Path $bundleRoot ".captured"
    New-HiddenFile $hiddenBundleFile

    $sealExit = Invoke-ScriptProcess $sealer @(
        "-Root", $bundleRoot,
        "-RequestManifestPath", $requestPath
    )
    Assert-Equal 0 $sealExit "Bundle sealing should succeed"
    Assert-True (Test-Path (Join-Path $bundleRoot "evidence-seal.json")) "Seal should exist"
    $seal = Read-UiEvidenceJson (Join-Path $bundleRoot "evidence-seal.json")
    Assert-True ($seal.files.relativePath -contains ".captured") "Seal must inventory hidden evidence"

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

    $injectedBundleFile = Join-Path $bundleRoot ".injected"
    New-HiddenFile $injectedBundleFile
    Assert-ScriptFailure $validator @("-Root", $bundleRoot) "unsealed file"
    Remove-Item -LiteralPath $injectedBundleFile -Force
    Add-Content -LiteralPath $hiddenBundleFile -Value "tampered" -Force
    Assert-ScriptFailure $validator @("-Root", $bundleRoot) "size changed|hash changed"
    "hidden evidence" | Set-Content -LiteralPath $hiddenBundleFile -Force

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
