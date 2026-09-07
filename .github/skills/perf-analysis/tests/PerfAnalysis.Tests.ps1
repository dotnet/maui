#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"

$skillRoot = Split-Path -Parent $PSScriptRoot
$selector = [IO.Path]::Combine($skillRoot, "scripts", "Select-Benchmarks.ps1")
$comparator = [IO.Path]::Combine($skillRoot, "scripts", "Compare-BenchmarkResults.ps1")
$scenarioRegistry = [IO.Path]::Combine($skillRoot, "references", "platform-scenarios.json")
$familyRegistry = [IO.Path]::Combine($skillRoot, "references", "benchmark-families.json")
$recommendationPolicyPath = [IO.Path]::Combine($skillRoot, "references", "recommendation-policy.json")
$benchmarkRunner = [IO.Path]::Combine($skillRoot, "scripts", "Invoke-PerfBenchmarks.ps1")
$repositoryRoot = [IO.Path]::GetFullPath([IO.Path]::Combine($skillRoot, "..", "..", ".."))
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-perf-tests-" + [Guid]::NewGuid().ToString("N"))

function Assert-True([bool]$Condition, [string]$Message) {
    if (-not $Condition) {
        throw "ASSERT TRUE FAILED: $Message"
    }
}

function Assert-Equal($Expected, $Actual, [string]$Message) {
    if ($Expected -ne $Actual) {
        throw "ASSERT EQUAL FAILED: $Message. Expected '$Expected', actual '$Actual'."
    }
}

function Write-ChangedFiles([string]$Name, [string[]]$Files) {
    $path = Join-Path $testRoot "$Name.txt"
    $Files | Set-Content -Path $path -Encoding UTF8
    return $path
}

function Invoke-SelectorFixture([string]$Name, [string[]]$Files) {
    $changedFilesPath = Write-ChangedFiles $Name $Files
    $outputPath = Join-Path $testRoot "$Name.json"

    & $selector `
        -ChangedFilesPath $changedFilesPath `
        -ScenarioRegistryPath $scenarioRegistry `
        -OutputPath $outputPath

    Assert-Equal 0 $LASTEXITCODE "Selector fixture '$Name' should be relevant"
    return Get-Content $outputPath -Raw | ConvertFrom-Json
}

function Write-BenchmarkReport(
    [string]$Path,
    [string]$FullName,
    [double]$Allocated,
    [double]$Mean = 100,
    [bool]$IncludeMemory = $true,
    [bool]$IncludeStatistics = $true
) {
    $directory = Split-Path -Parent $Path
    New-Item -ItemType Directory -Force -Path $directory | Out-Null

    $benchmark = @{
        FullName = $FullName
        Parameters = ""
    }
    if ($IncludeStatistics) {
        $benchmark.Statistics = @{
            Mean = $Mean
            StandardError = 1
        }
    }
    if ($IncludeMemory) {
        $benchmark.Memory = @{
            BytesAllocatedPerOperation = $Allocated
        }
    }

    $document = @{ Benchmarks = @($benchmark) }

    $document | ConvertTo-Json -Depth 8 | Set-Content -Path $Path -Encoding UTF8
}

function Invoke-ComparatorFixture(
    [string]$Name,
    [string]$ManifestStatus = "complete"
) {
    $root = Join-Path $testRoot $Name
    $summaryPath = Join-Path $root "summary.json"
    $markdownPath = Join-Path $root "table.md"
    $manifestPath = Join-Path $root "manifest.json"

    @{ status = $ManifestStatus } |
        ConvertTo-Json |
        Set-Content -Path $manifestPath -Encoding UTF8

    & $comparator `
        -BaseDir (Join-Path $root "base") `
        -HeadDir (Join-Path $root "head") `
        -RunManifestPath $manifestPath `
        -MarkdownOut $markdownPath `
        -JsonOut $summaryPath

    Assert-Equal 0 $LASTEXITCODE "Comparator fixture '$Name' should run"
    return Get-Content $summaryPath -Raw | ConvertFrom-Json
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null

try {
    $recommendationPolicy = Get-Content $recommendationPolicyPath -Raw | ConvertFrom-Json
    $benchmarkFamilies = Get-Content $familyRegistry -Raw | ConvertFrom-Json
    Assert-Equal 1 $benchmarkFamilies.schemaVersion "Benchmark family schema"
    Assert-True (@($benchmarkFamilies.families).Count -ge 8) "Benchmark family catalog breadth"
    Assert-Equal 2 $recommendationPolicy.schemaVersion "Recommendation policy schema"
    Assert-Equal 3 $recommendationPolicy.limits.maxRecommendations "Recommendation limit"
    Assert-Equal 7 (@($recommendationPolicy.nextActions).Count) "Next action count"
    Assert-Equal 7 (@($recommendationPolicy.reportVerdicts).Count) "Report verdict count"
    foreach ($requiredVerdict in @(
        "blocker",
        "advisory",
        "improvement",
        "clean",
        "no-blocker-incomplete",
        "device-required",
        "inconclusive"
    )) {
        Assert-True (@($recommendationPolicy.reportVerdicts.id) -contains $requiredVerdict) "Missing verdict '$requiredVerdict'"
    }
    $nextActionIds = @($recommendationPolicy.nextActions | ForEach-Object { $_.id })
    foreach ($requiredAction in @(
        "no_concerns",
        "no_perf_action_needed",
        "accept_tradeoff",
        "accept_with_followup",
        "optimize_before_merge",
        "run_more_measurements",
        "needs_human_discussion"
    )) {
        Assert-True ($nextActionIds -contains $requiredAction) "Missing next action '$requiredAction'"
    }
    Assert-True (
        @($recommendationPolicy.hardGates | Where-Object { $_ -match "Never recommend automatic issue closure" }).Count -eq 1
    ) "No-auto-close gate missing"
    Assert-True (
        @($recommendationPolicy.tradeoffAssessmentGates."likely-not-worth-it" | Where-Object { $_ -match "tested lower-cost" }).Count -eq 1
    ) "Likely-not-worth-it alternative gate missing"
    Assert-True (
        @($recommendationPolicy.hardGates | Where-Object { $_ -match "advisory-only evidence" }).Count -eq 1
    ) "Advisory evidence assessment gate missing"
    $acceptTradeoff = $recommendationPolicy.nextActions | Where-Object { $_.id -eq "accept_tradeoff" }
    Assert-Equal 1 (@($acceptTradeoff.allowedAssessments).Count) "Accept tradeoff assessment count"
    Assert-Equal "likely-worth-it" $acceptTradeoff.allowedAssessments[0] "Accept tradeoff assessment gate"
    Assert-True (
        @($acceptTradeoff.requires | Where-Object { $_ -match "non-advisory" }).Count -eq 1
    ) "Accept tradeoff non-advisory gate missing"
    $runMoreMeasurements = $recommendationPolicy.nextActions | Where-Object { $_.id -eq "run_more_measurements" }
    Assert-Equal "unclear" $runMoreMeasurements.allowedAssessments[0] "Run-more assessment gate"
    Assert-True $recommendationPolicy.decisionRules.confirmedRegressionTakesPrecedenceOverCoverageGaps "Regression precedence missing"
    Assert-True $recommendationPolicy.decisionRules.acceptanceRequiresConfirmedMeasuredCost "Measured acceptance gate missing"
    Assert-Equal "needs_human_discussion" $recommendationPolicy.decisionRules.unsupportedMissingEvidenceAction "Unsupported evidence action"
    Assert-True (-not $recommendationPolicy.decisionRules.externalEvidenceCanValidateWorkaround) "External workaround evidence must not validate"
    Assert-True (
        @($recommendationPolicy.costAttributions) -contains "deliberate"
    ) "Deliberate cost attribution missing"
    Assert-True (
        @($recommendationPolicy.workaroundStatuses) -contains "plausible-unverified"
    ) "Unverified workaround status missing"
    Assert-True (
        @($recommendationPolicy.workaroundStatuses) -notcontains "validated"
    ) "Validated workarounds must not be advertised without a trusted execution path"

    $invalidBaseFailed = $false
    try {
        & $selector `
            -BaseBranch "refs/heads/perf-analysis-base-does-not-exist" `
            -OutputPath (Join-Path $testRoot "invalid-base.json")
    }
    catch {
        $invalidBaseFailed = $_.Exception.Message -like "*Could not obtain changed files from base ref*"
    }
    Assert-True $invalidBaseFailed "An invalid local base ref must fail closed"

    $benchmarkRunnerSource = Get-Content $benchmarkRunner -Raw
    Assert-True ($benchmarkRunnerSource -match "Assert-SafeArtifactTree") "Isolated artifact type validation missing"
    Assert-True ($benchmarkRunnerSource -match "Assert-RealDirectoryPath") "Artifact root link validation missing"
    Assert-True ($benchmarkRunnerSource -match "Ensure-DotNetSdk") "Pinned SDK provisioning missing"
    Assert-True ($benchmarkRunnerSource -match '"--cli", \$context\.DotNet') "BenchmarkDotNet pinned CLI forwarding missing"
    Assert-True ($benchmarkRunnerSource -match "Directory\.Build\.Override\.props") "Nested benchmark build override missing"
    Assert-True ($benchmarkRunnerSource -match "Microsoft\.Maui\.BuildTasks\.slnf") "XAML build-task prerequisite missing"
    Assert-True ($benchmarkRunnerSource -match "Install-TrustedBenchmarkFiles") "Trusted benchmark overlay missing"
    Assert-True (
        $benchmarkRunnerSource -match 'Initialize-ExecutionContexts\r?\n\s+Install-TrustedBenchmarkFiles'
    ) "Trusted overlays must be installed into final execution contexts"
    Assert-True ($benchmarkRunnerSource -match "\*-report-full\*\.json") "Benchmark artifact allowlist missing"
    Assert-True (
        $benchmarkRunnerSource -notmatch 'Copy-Item\s+\(Join-Path\s+\$Source\s+"\*"\)'
    ) "Isolated artifacts must not use recursive wildcard copying"

    $tokens = $null
    $parseErrors = $null
    $runnerAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $benchmarkRunner,
        [ref]$tokens,
        [ref]$parseErrors)
    Assert-Equal 0 @($parseErrors).Count "Benchmark runner should parse"
    $realDirectoryFunction = $runnerAst.Find({
        param($node)
        $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $node.Name -eq "Assert-RealDirectoryPath"
    }, $true)
    Assert-True ($null -ne $realDirectoryFunction) "Real-directory validator function missing"
    $sdkVersionFunction = $runnerAst.Find({
        param($node)
        $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $node.Name -eq "Get-RequiredSdkVersion"
    }, $true)
    Assert-True ($null -ne $sdkVersionFunction) "SDK version resolver function missing"

    foreach ($trustedBenchmark in @(
        "src/Core/tests/Benchmarks/Benchmarks/LayoutExtensionsBenchmarker.cs",
        "src/Core/tests/Benchmarks/Benchmarks/VisualDiagnosticsBenchmarker.cs"
    )) {
        $productBenchmark = Join-Path (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $skillRoot))) $trustedBenchmark
        $overlayBenchmark = Join-Path $skillRoot "benchmark-overlays\$trustedBenchmark"
        Assert-True (Test-Path $overlayBenchmark) "Trusted overlay missing for $trustedBenchmark"
        Assert-Equal (
            (Get-FileHash $productBenchmark -Algorithm SHA256).Hash
        ) (
            (Get-FileHash $overlayBenchmark -Algorithm SHA256).Hash
        ) "Trusted overlay must match the checked-in benchmark source for $trustedBenchmark"
    }

    & {
        Invoke-Expression $sdkVersionFunction.Extent.Text
        $sdkFixture = Join-Path $testRoot "sdk-version"
        New-Item -ItemType Directory -Force $sdkFixture | Out-Null
        @{ tools = @{ dotnet = "11.0.100-preview.6.26325.125" } } |
            ConvertTo-Json -Depth 4 |
            Set-Content (Join-Path $sdkFixture "global.json") -Encoding UTF8
        $context = [PSCustomObject]@{ Work = $sdkFixture }
        Assert-Equal "11.0.100-preview.6.26325.125" (Get-RequiredSdkVersion $context) "Repository tools.dotnet SDK resolution"
    }

    & {
        Invoke-Expression $realDirectoryFunction.Extent.Text
        $script:linkedComponent = $null
        function Invoke-Native {
            param([string]$FilePath, [object[]]$Arguments)
            $component = [string]$Arguments[-1]
            $isLinkCheck = $Arguments -contains "-L"
            return [PSCustomObject]@{
                ExitCode = if ($isLinkCheck -and $component -eq $script:linkedComponent) { 1 } else { 0 }
                Output = @()
            }
        }

        $trustedRoot = [IO.Path]::Combine($testRoot, "trusted-root")
        $artifactPath = [IO.Path]::Combine($trustedRoot, "home", "artifacts", "run1")
        Assert-RealDirectoryPath -Root $trustedRoot -Path $artifactPath

        $outsideRejected = $false
        try {
            Assert-RealDirectoryPath -Root $trustedRoot -Path (Join-Path $testRoot "outside")
        } catch {
            $outsideRejected = $true
        }
        Assert-True $outsideRejected "Artifact paths outside the trusted root must fail"

        $script:linkedComponent = [IO.Path]::Combine($trustedRoot, "home")
        $linkRejected = $false
        try {
            Assert-RealDirectoryPath -Root $trustedRoot -Path $artifactPath
        } catch {
            $linkRejected = $true
        }
        Assert-True $linkRejected "Symbolic-link path components must fail"
    }

    foreach ($removedWorkflow in @("perf-check.md", "perf-check.lock.yml", "perf-history.yml")) {
        Assert-True (-not (Test-Path ([IO.Path]::Combine($repositoryRoot, ".github", "workflows", $removedWorkflow)))) "Standalone triggering must remain outside the analyzer"
    }
    $skillSource = Get-Content (Join-Path $skillRoot "SKILL.md") -Raw
    Assert-True ($skillSource -match '(?m)^name: perf-analysis\r?$') "Reusable skill discovery metadata missing"
    Assert-True ($skillSource -notmatch 'post_perf_report|run_device_performance|MAUI_DEVICE_PERFORMANCE_PIPELINE_ID') "Skill must not depend on removed workflow actions"

    $layoutExtensions = Invoke-SelectorFixture "layout-extensions" @(
        "src/Core/src/Layouts/LayoutExtensions.cs"
    )
    Assert-Equal "managed-complete" $layoutExtensions.coverage.status "LayoutExtensions should map to a direct managed benchmark"
    Assert-Equal 1 @($layoutExtensions.suites).Count "LayoutExtensions should select one benchmark project"
    Assert-Equal "Core" $layoutExtensions.suites[0].project "LayoutExtensions should select Core benchmarks"
    Assert-Equal 1 @($layoutExtensions.suites[0].filters).Count "LayoutExtensions should select only its direct benchmark"
    Assert-Equal "*LayoutExtensionsBenchmarker*" $layoutExtensions.suites[0].filters[0] "LayoutExtensions benchmark filter"
    Assert-Equal 1 $layoutExtensions.coverage.managedMeasuredFileCount "LayoutExtensions measured file count"
    Assert-Equal 0 $layoutExtensions.coverage.staticOnlyFileCount "LayoutExtensions should not remain static-only"

    $graphicsColor = Invoke-SelectorFixture "graphics-color" @(
        "src/Graphics/src/Graphics/Color.cs"
    )
    Assert-Equal "static-only" $graphicsColor.coverage.status "Graphics Color should remain partial while named-color coverage is excluded"
    Assert-Equal 2 @($graphicsColor.suites[0].filters).Count "Graphics Color stable benchmark count"
    Assert-True (@($graphicsColor.suites[0].filters) -contains "*ColorBenchmarker.Parse") "Color Parse benchmark missing"
    Assert-True (@($graphicsColor.suites[0].filters) -contains "*ColorBenchmarker.ParseBlack") "Color ParseBlack benchmark missing"
    Assert-Equal 1 $graphicsColor.coverage.managedSampledFileCount "Graphics Color sampled count"
    Assert-Equal 1 $graphicsColor.coverage.staticOnlyFileCount "Graphics Color static review count"
    Assert-True (-not $graphicsColor.coverage.canClaimWholePrClean) "Graphics Color cannot claim clean coverage from parser benchmarks"

    $platform = Invoke-SelectorFixture "platform" @(
        "src/Controls/src/Core/Handlers/Items/Android/MauiRecyclerView.cs",
        "src/Controls/src/Core/Handlers/Items2/iOS/LayoutFactory2.cs"
    )
    Assert-Equal "device-required" $platform.coverage.status "CollectionView platform files require devices"
    Assert-Equal 0 @($platform.suites).Count "Platform files must not map to managed suites"
    Assert-True $platform.requiresDeviceMeasurement "Platform selection should require device measurement"
    $scenarioIds = @($platform.deviceScenarios | ForEach-Object { $_.id })
    Assert-True ($scenarioIds -contains "collectionview-items-update-android") "Android CollectionView scenario missing"
    Assert-True ($scenarioIds -contains "collectionview-scroll-ios") "iOS CollectionView scenario missing"
    $androidScenario = $platform.deviceScenarios | Where-Object { $_.id -eq "collectionview-items-update-android" }
    Assert-Equal "manual-device-ci-ready" $androidScenario.automationStatus "Android pipeline status"
    Assert-Equal "eng/pipelines/ci-device-performance.yml" $androidScenario.pipeline.path "Android pipeline path"
    Assert-Equal "android" $androidScenario.pipeline.platforms[0] "Android canonical pipeline platform"
    $iosScenario = $platform.deviceScenarios | Where-Object { $_.id -eq "collectionview-scroll-ios" }
    Assert-Equal 2 (@($iosScenario.pipeline.platforms).Count) "Apple pipeline platform count"
    Assert-Equal "ios" $iosScenario.pipeline.platforms[0] "iOS canonical pipeline platform"
    Assert-Equal "maccatalyst" $iosScenario.pipeline.platforms[1] "MacCatalyst canonical pipeline platform"

    $uncoveredItemsViewLayout = Invoke-SelectorFixture "items-view-layout" @(
        "src/Controls/src/Core/Handlers/Items/iOS/ItemsViewLayout.cs"
    )
    Assert-Equal "collectionview-items-update-ios" $uncoveredItemsViewLayout.deviceScenarios[0].id "ItemsViewLayout should use its dedicated update scenario"
    Assert-Equal "manual-device-ci-ready" $uncoveredItemsViewLayout.deviceScenarios[0].automationStatus "ItemsViewLayout update scenario status"
    Assert-Equal "collectionview-keepitemsinview-update" $uncoveredItemsViewLayout.deviceScenarios[0].resultScenario "ItemsViewLayout result scenario"

    $windowsPlatform = Invoke-SelectorFixture "windows-platform" @(
        "src/Controls/src/Core/Handlers/Items/Windows/ItemsViewHandler.cs"
    )
    $windowsScenario = $windowsPlatform.deviceScenarios[0]
    Assert-Equal "required-not-yet-automated" $windowsScenario.automationStatus "Windows scenario remains unsupported"
    Assert-True ($null -eq $windowsScenario.pipeline) "Unsupported scenarios must not expose a pipeline handoff"

    $windowsCarousel = Invoke-SelectorFixture "windows-carousel" @(
        "src/Controls/src/Core/Handlers/Items/CarouselViewHandler.Windows.cs",
        "src/Controls/src/Core/Platform/Windows/CollectionView/LoopableCollectionView.cs"
    )
    Assert-Equal 1 @($windowsCarousel.deviceScenarios).Count "Windows CarouselView should select one dedicated scenario"
    Assert-Equal "carouselview-wheel-snap-windows" $windowsCarousel.deviceScenarios[0].id "Windows CarouselView scenario"
    Assert-Equal "manual-device-ci-ready" $windowsCarousel.deviceScenarios[0].automationStatus "Windows CarouselView automation status"
    Assert-Equal "windows" $windowsCarousel.deviceScenarios[0].pipeline.platforms[0] "Windows pipeline platform"

    $carouselPlatform = Invoke-SelectorFixture "carousel-platform" @(
        "src/Controls/src/Core/Handlers/Items/Android/MauiCarouselRecyclerView.cs",
        "src/Controls/src/Core/Handlers/Items/iOS/MauiCollectionView.cs",
        "src/Controls/src/Core/Handlers/Items2/CarouselViewHandler2.iOS.cs",
        "src/Controls/src/Core/PublicAPI/net-ios/PublicAPI.Unshipped.txt"
    )
    $carouselScenarioIds = @($carouselPlatform.deviceScenarios | ForEach-Object { $_.id })
    Assert-Equal 1 $carouselScenarioIds.Count "CarouselView should select only its dedicated scenario"
    Assert-Equal "carouselview-swipe-disabled" $carouselScenarioIds[0] "CarouselView scenario selection"
    Assert-Equal "manual-device-ci-ready" $carouselPlatform.deviceScenarios[0].automationStatus "CarouselView pipeline status"
    Assert-Equal 3 @($carouselPlatform.deviceScenarios[0].pipeline.platforms).Count "CarouselView platform count"
    Assert-Equal "carouselview-swipe-disabled" $carouselPlatform.deviceScenarios[0].resultScenario "CarouselView result scenario"
    Assert-Equal 0 $carouselPlatform.coverage.staticOnlyFileCount "PublicAPI files must not create performance coverage gaps"
    Assert-Equal 3 $carouselPlatform.coverage.deviceRequiredFileCount "CarouselView device file count"

    $sharedMauiCollectionView = Invoke-SelectorFixture "shared-maui-collection-view" @(
        "src/Controls/src/Core/Handlers/Items/iOS/MauiCollectionView.cs"
    )
    Assert-Equal "collectionview-handler-device" $sharedMauiCollectionView.deviceScenarios[0].id "CollectionView fallback should remain device-required"
    Assert-Equal "required-not-yet-automated" $sharedMauiCollectionView.deviceScenarios[0].automationStatus "CollectionView fallback scenario status"

    $platformResource = Invoke-SelectorFixture "platform-resource" @(
        "src/Controls/src/Core/Platform/Android/Resources/values/styles.xml"
    )
    Assert-Equal "static-only" $platformResource.coverage.status "Platform resources should not require performance device runs"
    Assert-Equal 0 @($platformResource.deviceScenarios).Count "Platform resources must not select device scenarios"

    $genericPlatform = Invoke-SelectorFixture "generic-platform" @(
        "src/Core/src/Handlers/ScrollView/ScrollViewHandler.Windows.cs"
    )
    Assert-Equal "static-only" $genericPlatform.coverage.status "Unmatched generic platform paths should be static-review-only"
    Assert-Equal 0 @($genericPlatform.deviceScenarios).Count "Generic platform fallback must not invent an unsupported device scenario"

    $handlerPropertyUpdate = Invoke-SelectorFixture "handler-property-update" @(
        "src/Core/src/Handlers/Label/LabelHandler.Android.cs"
    )
    Assert-Equal "handler-property-update-android" $handlerPropertyUpdate.deviceScenarios[0].id "Common Android handler family scenario"
    Assert-Equal "manual-device-ci-ready" $handlerPropertyUpdate.deviceScenarios[0].automationStatus "Common handler scenario automation"
    Assert-Equal "static-only" $handlerPropertyUpdate.coverage.status "Sampled device family must not claim direct coverage"

    $controlHandler = Invoke-SelectorFixture "control-handler" @(
        "src/Core/src/Handlers/Button/ButtonHandler.cs"
    )
    Assert-Equal "static-only" $controlHandler.coverage.status "A control handler is not covered by registrar benchmarks"
    Assert-Equal 1 @($controlHandler.suites).Count "Broad handler family should provide supplemental evidence"
    Assert-Equal 1 $controlHandler.coverage.managedSampledFileCount "Handler family sampled count"

    $gestureFamily = Invoke-SelectorFixture "gesture-family" @(
        "src/Controls/src/Core/TapGestureRecognizer.cs"
    )
    Assert-Equal "static-only" $gestureFamily.coverage.status "Gesture family evidence remains sampled"
    Assert-True (@($gestureFamily.suites[0].filters) -contains "*GestureRecognizerBenchmarker*") "Gesture family benchmark filter"

    $handlerDispatch = Invoke-SelectorFixture "handler-dispatch" @(
        "src/Core/src/Handlers/Element/ElementHandler.cs"
    )
    Assert-Equal "static-only" $handlerDispatch.coverage.status "Handler dispatch benchmarks are supplemental until every mapped path is exercised"
    Assert-Equal 1 $handlerDispatch.coverage.managedSampledFileCount "ElementHandler supplemental benchmark count"
    Assert-True (@($handlerDispatch.suites[0].filters) -contains "*PropertyMapperBenchmarker*") "Existing update dispatch benchmark missing"
    Assert-True (@($handlerDispatch.suites[0].filters) -contains "*ElementHandlerUpdateBenchmarks*") "Batching benchmark mapping missing"

    $visualDiagnostics = Invoke-SelectorFixture "visual-diagnostics" @(
        "src/Core/src/VisualDiagnostics/VisualDiagnostics.cs"
    )
    Assert-Equal "static-only" $visualDiagnostics.coverage.status "VisualDiagnostics event dispatch covers only one responsibility in the file"
    Assert-Equal 1 $visualDiagnostics.coverage.managedSampledFileCount "VisualDiagnostics supplemental benchmark count"
    Assert-Equal "*VisualDiagnosticsBenchmarker*" $visualDiagnostics.suites[0].filters[0] "VisualDiagnostics benchmark filter"

    $converter = Invoke-SelectorFixture "converter" @(
        "src/Controls/src/Core/FlowDirectionConverter.cs"
    )
    Assert-Equal "static-only" $converter.coverage.status "Generic converters are not covered by TypeConversionBenchmarker"
    Assert-Equal 0 @($converter.suites).Count "Generic converters must not map to a narrow conversion benchmark"

    $multiBinding = Invoke-SelectorFixture "multi-binding" @(
        "src/Controls/src/Core/MultiBinding.cs"
    )
    Assert-Equal "static-only" $multiBinding.coverage.status "MultiBinding is not exercised by the selected binding benchmarks"
    Assert-Equal 1 @($multiBinding.suites).Count "MultiBinding should receive binding-family sampled evidence"
    Assert-Equal 1 $multiBinding.coverage.managedSampledFileCount "MultiBinding sampled count"

    $blazor = Invoke-SelectorFixture "blazor-root" @(
        "src/BlazorWebView/src/MauiBlazorWebView.cs"
    )
    Assert-True $blazor.relevant "Shipping BlazorWebView source must be performance-relevant"
    Assert-Equal "static-only" $blazor.coverage.status "Unbenchmarked shipping roots remain static-only"

    $mapper = Invoke-SelectorFixture "mapper" @(
        "src/Core/src/PropertyMapper.cs"
    )
    Assert-Equal "managed-complete" $mapper.coverage.status "PropertyMapper should map to managed benchmarks"
    Assert-Equal 1 @($mapper.suites).Count "PropertyMapper should select one project"
    Assert-Equal "Core" $mapper.suites[0].project "PropertyMapper should select Core benchmarks"

    $mixedRoots = Invoke-SelectorFixture "mixed-roots" @(
        "src/Core/src/PropertyMapper.cs",
        "src/BlazorWebView/src/MauiBlazorWebView.cs"
    )
    Assert-Equal "mixed" $mixedRoots.coverage.status "Mixed benchmarked and unbenchmarked shipping roots must remain partial"
    Assert-True (-not $mixedRoots.coverage.canClaimWholePrClean) "Mixed-root PR cannot claim whole-PR clean coverage"

    $commonInputs = Invoke-SelectorFixture "common-inputs" @(
        "src/Core/src/PropertyMapper.cs",
        "eng/Versions.targets"
    )
    Assert-True $commonInputs.suites[0].benchmarkInputsChanged "Shared build inputs must invalidate benchmark comparison"
    Assert-True (-not $commonInputs.coverage.canClaimWholePrClean) "Changed shared build inputs cannot claim clean coverage"

    $registrarInput = Invoke-SelectorFixture "registrar-input" @(
        "src/Core/src/PropertyMapper.cs",
        "src/Core/tests/Benchmarks/Registrar.cs"
    )
    Assert-True $registrarInput.suites[0].benchmarkInputsChanged "Shared Registrar harness changes must invalidate the Core suite"

    $partialInputs = Invoke-SelectorFixture "partial-inputs" @(
        "src/Core/src/PropertyMapper.cs",
        "src/Controls/src/Core/BindableObject.cs",
        "src/Core/tests/Benchmarks/Benchmarks/BindingBenchmarker.cs"
    )
    Assert-True (-not $partialInputs.suites[0].benchmarkInputsChanged) "One changed benchmark class must not invalidate unrelated filters"
    Assert-True (@($partialInputs.suites[0].changedFilters) -contains "*BindingBenchmarker*") "Changed benchmark filter should be identified"
    Assert-True (@($partialInputs.suites[0].runnableFilters) -contains "*PropertyMapperBenchmarker*") "Unaffected benchmark filters should remain runnable"
    Assert-Equal "managed-inputs-changed" $partialInputs.coverage.status "Partial benchmark-input changes must remain visible as an evidence gap"

    $xamlInputs = Invoke-SelectorFixture "xaml-inputs" @(
        "src/Controls/src/Xaml/XamlLoader.cs",
        "src/Controls/tests/Xaml.UnitTests/Benchmark.xaml"
    )
    Assert-Equal 1 @($xamlInputs.suites).Count "XAML product change should select XAML benchmarks"
    Assert-True $xamlInputs.suites[0].benchmarkInputsChanged "XAML unit-test fixture changes alter the benchmark workload"

    $productXaml = Invoke-SelectorFixture "product-xaml" @(
        "src/Templates/src/templates/maui-mobile/MainPage.xaml"
    )
    Assert-Equal 1 @($productXaml.suites).Count "Product XAML should select supplemental XAML benchmarks"
    Assert-Equal "Xaml" $productXaml.suites[0].project "Product XAML benchmark project"

    $neutralRoot = Join-Path $testRoot "neutral"
    foreach ($side in @("base", "head")) {
        Write-BenchmarkReport ([IO.Path]::Combine($neutralRoot, $side, "run1", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100 100
        Write-BenchmarkReport ([IO.Path]::Combine($neutralRoot, $side, "run2", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100 100
    }
    $neutral = Invoke-ComparatorFixture "neutral"
    Assert-Equal "neutral" $neutral.verdict "Complete equal benchmark data should be neutral"
    Assert-True $neutral.canClaimClean "Complete neutral benchmark data should support a clean claim"

    $allocationImprovementRoot = Join-Path $testRoot "allocation-improvement"
    Write-BenchmarkReport ([IO.Path]::Combine($allocationImprovementRoot, "base", "run1", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 200 100
    Write-BenchmarkReport ([IO.Path]::Combine($allocationImprovementRoot, "base", "run2", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 210 100
    Write-BenchmarkReport ([IO.Path]::Combine($allocationImprovementRoot, "head", "run1", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100 100
    Write-BenchmarkReport ([IO.Path]::Combine($allocationImprovementRoot, "head", "run2", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 110 100
    $allocationImprovement = Invoke-ComparatorFixture "allocation-improvement"
    Assert-Equal "improvement" $allocationImprovement.verdict "Confirmed allocation reduction should be a measured improvement"

    $timeImprovementRoot = Join-Path $testRoot "time-improvement"
    Write-BenchmarkReport ([IO.Path]::Combine($timeImprovementRoot, "base", "run1", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100 120
    Write-BenchmarkReport ([IO.Path]::Combine($timeImprovementRoot, "base", "run2", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100 125
    Write-BenchmarkReport ([IO.Path]::Combine($timeImprovementRoot, "head", "run1", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100 80
    Write-BenchmarkReport ([IO.Path]::Combine($timeImprovementRoot, "head", "run2", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100 85
    $timeImprovement = Invoke-ComparatorFixture "time-improvement"
    Assert-Equal "time-improvement-advisory" $timeImprovement.verdict "Timing-only improvement must remain advisory"

    $rangeRoot = Join-Path $testRoot "allocation-range"
    Write-BenchmarkReport ([IO.Path]::Combine($rangeRoot, "base", "run1", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100
    Write-BenchmarkReport ([IO.Path]::Combine($rangeRoot, "base", "run2", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 140
    Write-BenchmarkReport ([IO.Path]::Combine($rangeRoot, "head", "run1", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 150
    Write-BenchmarkReport ([IO.Path]::Combine($rangeRoot, "head", "run2", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 190
    $range = Invoke-ComparatorFixture "allocation-range"
    Assert-Equal "alloc-regression" $range.verdict "Non-overlapping allocation ranges should regress"
    Assert-Equal 10 $range.allocRegressions[0].confirmedDeltaBytes "Only the proven range gap should be reported"
    Assert-True $range.allocRegressions[0].confirmed "Repeated allocation evidence should be confirmed"
    Assert-True (-not $range.canClaimClean) "A confirmed regression cannot claim a clean result"

    $disjointRoot = Join-Path $testRoot "disjoint"
    Write-BenchmarkReport ([IO.Path]::Combine($disjointRoot, "base", "run1", "base-report-full.json")) "Microsoft.Maui.Benchmarks.BaseOnly" 100
    Write-BenchmarkReport ([IO.Path]::Combine($disjointRoot, "base", "run2", "base-report-full.json")) "Microsoft.Maui.Benchmarks.BaseOnly" 100
    Write-BenchmarkReport ([IO.Path]::Combine($disjointRoot, "head", "run1", "head-report-full.json")) "Microsoft.Maui.Benchmarks.HeadOnly" 100
    Write-BenchmarkReport ([IO.Path]::Combine($disjointRoot, "head", "run2", "head-report-full.json")) "Microsoft.Maui.Benchmarks.HeadOnly" 100
    $disjoint = Invoke-ComparatorFixture "disjoint"
    Assert-Equal "inconclusive" $disjoint.verdict "Disjoint benchmark sets must not be neutral"
    Assert-True (-not $disjoint.canClaimClean) "Disjoint benchmark sets cannot claim clean"
    Assert-Equal 0 $disjoint.commonCount "Disjoint fixture should have no common benchmarks"

    $incompleteRoot = Join-Path $testRoot "incomplete"
    foreach ($side in @("base", "head")) {
        Write-BenchmarkReport ([IO.Path]::Combine($incompleteRoot, $side, "run1", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100
        Write-BenchmarkReport ([IO.Path]::Combine($incompleteRoot, $side, "run2", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100
    }
    $incomplete = Invoke-ComparatorFixture "incomplete" "incomplete"
    Assert-Equal "inconclusive" $incomplete.verdict "Incomplete runner manifest must fail closed"
    Assert-True (-not $incomplete.executionComplete) "Incomplete manifest should be reflected in summary"
    Assert-True (-not $incomplete.canClaimClean) "Incomplete execution cannot claim clean"

    $partialRoot = Join-Path $testRoot "partial-coverage"
    foreach ($side in @("base", "head")) {
        Write-BenchmarkReport ([IO.Path]::Combine($partialRoot, $side, "run1", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100
        Write-BenchmarkReport ([IO.Path]::Combine($partialRoot, $side, "run2", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100
    }
    @{
        status = "incomplete"
        runsPerSide = 2
        suites = @(
            @{ project = "Core"; complete = $true; skipReason = $null },
            @{ project = "Xaml"; complete = $false; skipReason = "benchmark inputs changed" }
        )
    } | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $partialRoot "manifest.json") -Encoding UTF8
    & $comparator `
        -BaseDir (Join-Path $partialRoot "base") `
        -HeadDir (Join-Path $partialRoot "head") `
        -RunManifestPath (Join-Path $partialRoot "manifest.json") `
        -MarkdownOut (Join-Path $partialRoot "table.md") `
        -JsonOut (Join-Path $partialRoot "summary.json")
    $partial = Get-Content (Join-Path $partialRoot "summary.json") -Raw | ConvertFrom-Json
    Assert-True $partial.executionComplete "Planned benchmark-input gaps must not masquerade as execution failures"
    Assert-True (-not $partial.coverageComplete) "Planned benchmark-input gaps must still prevent clean coverage"
    Assert-Equal "inconclusive" $partial.verdict "Partial empirical comparison remains inconclusive"

    $missingDataRoot = Join-Path $testRoot "missing-data"
    Write-BenchmarkReport ([IO.Path]::Combine($missingDataRoot, "base", "run1", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100
    Write-BenchmarkReport ([IO.Path]::Combine($missingDataRoot, "base", "run2", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100 -IncludeMemory $false
    Write-BenchmarkReport ([IO.Path]::Combine($missingDataRoot, "head", "run1", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100
    Write-BenchmarkReport ([IO.Path]::Combine($missingDataRoot, "head", "run2", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100
    $missingData = Invoke-ComparatorFixture "missing-data"
    Assert-Equal "inconclusive" $missingData.verdict "Missing allocation data must fail closed"
    Assert-True (-not $missingData.benchmarkDataComplete) "Missing allocation data should be listed as incomplete"
    Assert-True (-not $missingData.canClaimClean) "Missing allocation data cannot claim clean"

    $invalidManifestRoot = Join-Path $testRoot "invalid-manifest"
    foreach ($side in @("base", "head")) {
        Write-BenchmarkReport ([IO.Path]::Combine($invalidManifestRoot, $side, "run1", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100
        Write-BenchmarkReport ([IO.Path]::Combine($invalidManifestRoot, $side, "run2", "sample-report-full.json")) "Microsoft.Maui.Benchmarks.Sample.Run" 100
    }
    "{" | Set-Content (Join-Path $invalidManifestRoot "manifest.json") -Encoding UTF8
    & $comparator `
        -BaseDir (Join-Path $invalidManifestRoot "base") `
        -HeadDir (Join-Path $invalidManifestRoot "head") `
        -RunManifestPath (Join-Path $invalidManifestRoot "manifest.json") `
        -MarkdownOut (Join-Path $invalidManifestRoot "table.md") `
        -JsonOut (Join-Path $invalidManifestRoot "summary.json")
    Assert-Equal 0 $LASTEXITCODE "Malformed manifest should produce fail-closed evidence"
    $invalidManifest = Get-Content (Join-Path $invalidManifestRoot "summary.json") -Raw | ConvertFrom-Json
    Assert-Equal "invalid" $invalidManifest.executionStatus "Malformed manifest status"
    Assert-Equal "inconclusive" $invalidManifest.verdict "Malformed manifest must be inconclusive"

    $noManifestSummaryPath = Join-Path $incompleteRoot "summary-no-manifest.json"
    & $comparator `
        -BaseDir (Join-Path $incompleteRoot "base") `
        -HeadDir (Join-Path $incompleteRoot "head") `
        -MarkdownOut (Join-Path $incompleteRoot "table-no-manifest.md") `
        -JsonOut $noManifestSummaryPath
    $noManifest = Get-Content $noManifestSummaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $noManifest.verdict "A runner manifest is required for a clean verdict"
    Assert-True (-not $noManifest.executionComplete) "Missing runner manifest must be incomplete"

    Write-Output "All perf-analysis tests passed."
}
finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
