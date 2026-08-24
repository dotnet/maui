#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Classifies performance-sensitive PR changes and selects trustworthy managed benchmarks.

.DESCRIPTION
    Maps changed product files into one of three evidence tiers:

      * managed-measured - a targeted BenchmarkDotNet suite is known to exercise the area;
      * device-required  - the code is platform/handler specific and needs a curated device scenario;
      * static-only      - no trustworthy empirical benchmark currently covers the file.

    A file is never marked measured merely because it lives under a broad directory such as
    src/Core/src/Handlers or src/Graphics/src. This prevents unrelated benchmarks from producing
    a false "no measurable impact" result.

.PARAMETER PrNumber
    Pull request number. Changed files are read using gh.

.PARAMETER BaseBranch
    Base ref used for local git-diff mode.

.PARAMETER ChangedFilesPath
    Optional newline-delimited changed-file fixture. Intended for deterministic tests.

.PARAMETER ScenarioRegistryPath
    Optional path to the platform scenario registry.

.PARAMETER OutputPath
    JSON output path, or '-' for stdout.
#>

param(
    [Parameter(Mandatory = $false)]
    [int]$PrNumber,

    [Parameter(Mandatory = $false)]
    [string]$BaseBranch = "origin/main",

    [Parameter(Mandatory = $false)]
    [string]$ChangedFilesPath,

    [Parameter(Mandatory = $false)]
    [string]$ScenarioRegistryPath = ([IO.Path]::Combine($PSScriptRoot, "..", "references", "platform-scenarios.json")),

    [Parameter(Mandatory = $false)]
    [string]$FamilyRegistryPath = ([IO.Path]::Combine($PSScriptRoot, "..", "references", "benchmark-families.json")),

    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "-"
)

$ErrorActionPreference = "Stop"

function Write-Info([string]$Message) {
    [Console]::Error.WriteLine($Message)
}

$Projects = @{
    Core     = "src/Core/tests/Benchmarks/Core.Benchmarks.csproj"
    Graphics = "src/Graphics/tests/Graphics.Benchmarks/Graphics.Benchmarks.csproj"
    Xaml     = "src/Controls/tests/Xaml.Benchmarks/Microsoft.Maui.Controls.Xaml.Benchmarks.csproj"
}

$CommonBenchmarkInputs = @(
    "Directory.Build.props",
    "Directory.Build.targets",
    "Directory.Build.Override.props",
    "Directory.Build.rsp",
    "Directory.Packages.props",
    "global.json",
    "NuGet.config",
    ".config/dotnet-tools.json",
    "eng/AndroidX.targets",
    "eng/BannedApis.targets",
    "eng/Build.props",
    "eng/CgManifest.targets",
    "eng/Environment.Build.props",
    "eng/ILRepack.targets",
    "eng/NuGetVersions.targets",
    "eng/Publishing.props",
    "eng/ReplaceText.targets",
    "eng/Signing.props",
    "eng/SourceLink.Build.props",
    "eng/Tools.props",
    "eng/Versions.props",
    "eng/Versions.targets",
    "src/Maui.InTree.props",
    "src/Maui.InTree.targets"
)

$SuiteWideBenchmarkInputs = @{
    Core = @(
        "src/Core/tests/Benchmarks/Core.Benchmarks.csproj",
        "src/Core/tests/Benchmarks/Program.cs",
        "src/Core/tests/Benchmarks/Registrar.cs",
        "src/Core/tests/Benchmarks/Stubs/"
    ) + $CommonBenchmarkInputs
    Graphics = @(
        "src/Graphics/tests/Graphics.Benchmarks/Graphics.Benchmarks.csproj",
        "src/Graphics/tests/Graphics.Benchmarks/Program.cs"
    ) + $CommonBenchmarkInputs
    Xaml = @(
        "src/Controls/tests/Xaml.Benchmarks/",
        "src/Controls/tests/Xaml.UnitTests/",
        "src/Controls/tests/Xaml.UnitTests.ExternalAssembly/",
        "src/Controls/tests/Xaml.UnitTests.InternalsHiddenAssembly/",
        "src/Controls/tests/Xaml.UnitTests.InternalsVisibleAssembly/",
        "src/Controls/tests/Maui25871Library/",
        "src/Controls/tests/Core.UnitTests/",
        "src/Core/tests/UnitTests/",
        "src/Controls/tests/XamlC.Tests.targets"
    ) + $CommonBenchmarkInputs
}

$BenchmarkInputs = @{
    Core = @(
        "src/Core/tests/Benchmarks/"
    ) + $CommonBenchmarkInputs
    Graphics = @(
        "src/Graphics/tests/Graphics.Benchmarks/"
    ) + $CommonBenchmarkInputs
    Xaml = @(
        "src/Controls/tests/Xaml.Benchmarks/",
        # The XAML benchmark references Controls.Xaml.UnitTests and instantiates its
        # Benchmark fixture, so changes there alter the workload as well.
        "src/Controls/tests/Xaml.UnitTests/",
        "src/Controls/tests/Xaml.UnitTests.ExternalAssembly/",
        "src/Controls/tests/Xaml.UnitTests.InternalsHiddenAssembly/",
        "src/Controls/tests/Xaml.UnitTests.InternalsVisibleAssembly/",
        "src/Controls/tests/Maui25871Library/",
        "src/Controls/tests/Core.UnitTests/",
        "src/Core/tests/UnitTests/",
        "src/Controls/tests/XamlC.Tests.targets"
    ) + $CommonBenchmarkInputs
}

# CoversFile=true means the benchmark directly targets the focused implementation file.
# CoversFile=false selects useful supplemental evidence but leaves the file static-only.
$ManagedRules = @(
    @{
        Area = "Layout frame alignment"
        Project = "Core"
        CoversFile = $true
        Pattern = '(^|/)src/Core/src/Layouts/LayoutExtensions\.cs$'
        Filters = @(
            '*LayoutExtensionsBenchmarker*'
        )
        TrustedBenchmarkFiles = @(
            'src/Core/tests/Benchmarks/Benchmarks/LayoutExtensionsBenchmarker.cs'
        )
    },
    @{
        Area = "Layout"
        Project = "Core"
        CoversFile = $true
        Pattern = '(^|/)(src/Controls/src/Core/Layout/(Layout|LayoutExtensions|VerticalStackLayout|StackLayoutManager|FlexLayout|FlexExtensions|Grid|GridExtensions)\.cs|src/Core/src/Layouts/(VerticalStackLayoutManager|FlexLayoutManager|GridLayoutManager)\.cs|src/Core/src/Primitives/GridLength\.cs|src/Controls/src/Core/(ColumnDefinition|ColumnDefinitionCollection|ColumnDefinitionCollectionTypeConverter|RowDefinition|RowDefinitionCollection|RowDefinitionCollectionTypeConverter|GridLengthTypeConverter)\.cs)$'
        Filters = @(
            '*LayoutBenchmarker*',
            '*FlexLayoutBenchmarker*',
            '*GridDefinitionBenchmarker*',
            '*GridLayoutManagerBenchMarker*'
        )
    },
    @{
        Area = "Binding"
        Project = "Core"
        CoversFile = $true
        Pattern = '(^|/)src/Controls/src/Core/(BindableObject|BindableProperty|Binding|BindingBase|BindingBase\.Create|BindingExpression|BindingExpressionHelper|TypedBinding|Setter|SettersExtensions)\.cs$'
        Filters = @(
            '*BindingBenchmarker*',
            '*TypedBindingBenchmarker*',
            '*SourceGeneratedBindingBenchmarker*',
            '*BindingComparisonBenchmarker*',
            '*BindableObjectBenchmarker*',
            '*SetterBenchmarker*'
        )
    },
    @{
        Area = "Generated binding"
        Project = "Core"
        CoversFile = $false
        Pattern = '(^|/)src/Controls/src/(BindingSourceGen|SourceGen)/.*Binding.*\.cs$'
        Filters = @('*SourceGeneratedBindingBenchmarker*', '*TypedBindingBenchmarker*')
    },
    @{
        Area = "Resources"
        Project = "Core"
        CoversFile = $true
        Pattern = '(^|/)src/Controls/src/Core/(ResourceDictionary|BindableObjectExtensions|AppThemeBinding)\.cs$'
        Filters = @('*ResourceDictionaryBenchmarker*', '*ParentResourceRefreshBenchmarker*')
    },
    @{
        Area = "Handler infrastructure"
        Project = "Core"
        CoversFile = $true
        Pattern = '(^|/)src/Core/src/(PropertyMapper|PropertyMapperExtensions)\.cs$|(^|/)src/Core/src/Hosting/(MauiApp|MauiAppBuilder|MauiHandlersCollectionExtensions)\.cs$|(^|/)src/Core/src/Hosting/Internal/(MauiHandlersFactory|MauiHandlersCollection)\.cs$'
        Filters = @(
            '*RegisterHandlersBenchmarker*',
            '*GetHandlersBenchmarker*',
            '*PropertyMapperBenchmarker*',
            '*PropertyMapperExtensionsBenchmarker*'
        )
    },
    @{
        Area = "Handler update dispatch"
        Project = "Core"
        CoversFile = $false
        Pattern = '(^|/)src/Core/src/Handlers/(IElementHandler\.cs|Element/ElementHandler(OfT)?\.cs|View/ViewHandler\.cs)$|(^|/)src/Controls/src/Core/VisualElement/VisualElement\.cs$'
        Filters = @(
            '*PropertyMapperBenchmarker*',
            '*ElementHandlerUpdateBenchmarks*'
        )
    },
    @{
        Area = "Visual diagnostics event dispatch"
        Project = "Core"
        CoversFile = $false
        Pattern = '(^|/)src/Core/src/VisualDiagnostics/VisualDiagnostics\.cs$'
        Filters = @('*VisualDiagnosticsBenchmarker*')
        TrustedBenchmarkFiles = @(
            'src/Core/tests/Benchmarks/Benchmarks/VisualDiagnosticsBenchmarker.cs'
        )
    },
    @{
        Area = "Shell"
        Project = "Core"
        CoversFile = $false
        Pattern = '(^|/)src/Controls/src/Core/Shell/|(^|/)Shell[A-Za-z]*\.cs$'
        Filters = @('*ShellBenchmarker*')
    },
    @{
        Area = "Visual tree"
        Project = "Core"
        CoversFile = $true
        Pattern = '(^|/)src/Core/src/Core/Extensions/VisualTreeElementExtensions\.cs$'
        Filters = @('*VisualTreeBenchmarker*')
    },
    @{
        Area = "Gestures"
        Project = "Core"
        CoversFile = $true
        Pattern = '(^|/)src/Controls/src/Core/GestureElement\.cs$'
        Filters = @('*GestureRecognizerBenchmarker*')
    },
    @{
        Area = "ImageSource"
        Project = "Core"
        CoversFile = $true
        Pattern = '(^|/)src/Controls/src/Core/(ImageSource|FileImageSource)\.cs$|(^|/)src/Controls/src/Core/Image/(Image|ImageSource)\.cs$'
        Filters = @('*ImageSourceBenchmarker*')
    },
    @{
        Area = "Hosting/DI"
        Project = "Core"
        CoversFile = $true
        Pattern = '(^|/)src/Core/src/Hosting/(MauiApp|MauiAppBuilder)\.cs$'
        Filters = @('*MauiServiceProviderBenchmarker*')
    },
    @{
        Area = "Graphics color"
        Project = "Graphics"
        CoversFile = $false
        Pattern = '(^|/)src/Graphics/src/Graphics/(Color|Colors|ColorUtils)\.cs$'
        Filters = @(
            '*ColorBenchmarker.Parse',
            '*ColorBenchmarker.ParseBlack'
        )
    },
    @{
        Area = "Graphics path parsing"
        Project = "Graphics"
        CoversFile = $true
        Pattern = '(^|/)src/Graphics/src/Graphics/(PathBuilder|PathF)\.cs$'
        Filters = @('*PathBenchmarker*')
    },
    @{
        Area = "XAML"
        Project = "Xaml"
        CoversFile = $false
        Pattern = '(^|/)src/Controls/src/Xaml/|(^|/)src/Controls/src/SourceGen/|\.xaml$'
        Filters = @('*')
    }
)

$ProductRoots = @(
    'src/BlazorWebView/src/',
    'src/Compatibility/Android.AppLinks/src/',
    'src/Compatibility/Core/src/',
    'src/Compatibility/Maps/src/',
    'src/Compatibility/Material/src/',
    'src/Controls/Foldable/src/',
    'src/Controls/Maps/src/',
    'src/Controls/src/',
    'src/Core/maps/src/',
    'src/Core/src/',
    'src/Essentials/src/',
    'src/Graphics/src/',
    'src/ProfiledAot/src/',
    'src/SingleProject/Resizetizer/src/',
    'src/Templates/src/'
)

$StaticPlatformPatterns = @(
    '(^|/)Resources/(values|drawable|mipmap|raw)/',
    '\.xml$'
)

$PlatformPattern = '(^|/)(Android|iOS|MacCatalyst|Windows|Tizen)(/|$)|\.(android|ios|maccatalyst|windows|tizen)\.(cs|xaml)$'
$CollectionViewHandlerPattern = '(^|/)Handlers/Items2?/'

function Get-ChangedFiles {
    if ($ChangedFilesPath) {
        if (-not (Test-Path $ChangedFilesPath)) {
            throw "Changed-files fixture not found: $ChangedFilesPath"
        }

        return @(Get-Content $ChangedFilesPath | Where-Object { $_ })
    }

    if ($PrNumber -gt 0) {
        $repo = $env:GITHUB_REPOSITORY
        $repoArg = if ($repo) { @('--repo', $repo) } else { @() }

        try {
            $output = & gh pr diff $PrNumber @repoArg --name-only 2>$null
            if ($LASTEXITCODE -eq 0 -and $output) {
                return @($output | Where-Object { $_ })
            }
        }
        catch {
        }

        if ($repo) {
            $output = & gh api "repos/$repo/pulls/$PrNumber/files" --paginate --jq '.[].filename' 2>$null
            if ($LASTEXITCODE -eq 0 -and $output) {
                return @($output | Where-Object { $_ })
            }
        }

        throw "Could not obtain changed files for PR #$PrNumber."
    }

    $output = & git diff --name-only "$BaseBranch...HEAD" 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $output) {
        $output = & git diff --name-only $BaseBranch 2>$null
        if ($LASTEXITCODE -ne 0) {
            throw "Could not obtain changed files from base ref '$BaseBranch'."
        }
    }

    return @($output | Where-Object { $_ })
}

function Read-ScenarioRegistry {
    if (-not (Test-Path $ScenarioRegistryPath)) {
        throw "Platform scenario registry not found: $ScenarioRegistryPath"
    }

    $registry = Get-Content $ScenarioRegistryPath -Raw | ConvertFrom-Json
    if (-not $registry.scenarios) {
        throw "Platform scenario registry contains no scenarios: $ScenarioRegistryPath"
    }

    return @($registry.scenarios)
}

function Read-FamilyRegistry {
    if (-not (Test-Path $FamilyRegistryPath)) {
        throw "Benchmark family registry not found: $FamilyRegistryPath"
    }

    $registry = Get-Content $FamilyRegistryPath -Raw | ConvertFrom-Json
    if ([int]$registry.schemaVersion -ne 1 -or -not $registry.families) {
        throw "Benchmark family registry is invalid: $FamilyRegistryPath"
    }

    return @($registry.families)
}

$changed = @(
    Get-ChangedFiles |
        ForEach-Object { $_ -replace '\\', '/' } |
        Sort-Object -Unique
)
Write-Info "Changed files: $($changed.Count)"

$productFiles = New-Object System.Collections.Generic.List[string]
foreach ($file in $changed) {
    if ($file -imatch '(^|/)PublicAPI\.(Shipped|Unshipped)\.txt$') {
        continue
    }

    foreach ($root in $ProductRoots) {
        if ($file -like "$root*") {
            $productFiles.Add($file)
            break
        }
    }
}

$scenarioDefinitions = @(Read-ScenarioRegistry)
$familyDefinitions = @(Read-FamilyRegistry)
$activeScenarioDefinitions = @(
    foreach ($scenario in $scenarioDefinitions) {
        $activationPatterns = @($scenario.activationPathPatterns | Where-Object { $_ })
        if ($activationPatterns.Count -eq 0 -or @(
            $changed | Where-Object {
                $candidate = $_
                @($activationPatterns | Where-Object { $candidate -imatch $_ }).Count -gt 0
            }
        ).Count -gt 0) {
            $scenario
        }
    }
)
$suiteMap = @{}
$scenarioMap = @{}
$managedFiles = New-Object System.Collections.Generic.HashSet[string]
$sampledFiles = New-Object System.Collections.Generic.HashSet[string]
$deviceFiles = New-Object System.Collections.Generic.HashSet[string]
$deviceSampledFiles = New-Object System.Collections.Generic.HashSet[string]
$staticOnlyFiles = New-Object System.Collections.Generic.HashSet[string]

foreach ($file in $productFiles) {
    $staticPlatformFile = @(
        $StaticPlatformPatterns | Where-Object { $file -imatch $_ }
    ).Count -gt 0
    if ($staticPlatformFile) {
        [void]$staticOnlyFiles.Add($file)
        continue
    }

    $requiresDevice = ($file -imatch $PlatformPattern) -or ($file -imatch $CollectionViewHandlerPattern)

    if ($requiresDevice) {
        $matchedScenario = $false
        foreach ($scenario in $activeScenarioDefinitions) {
            foreach ($pattern in @($scenario.pathPatterns)) {
                if ($file -imatch $pattern) {
                    $matchedScenario = $true
                    if ($scenario.automationStatus -eq "static-review-only") {
                        [void]$staticOnlyFiles.Add($file)
                        break
                    }

                    if ([string]$scenario.coverageMode -eq "sampled") {
                        [void]$staticOnlyFiles.Add($file)
                        [void]$deviceSampledFiles.Add($file)
                    }
                    else {
                        [void]$deviceFiles.Add($file)
                    }

                    if (-not $scenarioMap.ContainsKey($scenario.id)) {
                        $scenarioMap[$scenario.id] = @{
                            Definition = $scenario
                            Files = (New-Object System.Collections.Generic.HashSet[string])
                        }
                    }

                    [void]$scenarioMap[$scenario.id].Files.Add($file)
                    break
                }
            }

            if ($matchedScenario) {
                break
            }
        }

        if (-not $matchedScenario) {
            [void]$staticOnlyFiles.Add($file)
        }

        continue
    }

    $matchedManagedRule = $false
    $directlyCovered = $false
    foreach ($rule in $ManagedRules) {
        if ($file -imatch $rule.Pattern) {
            $matchedManagedRule = $true
            if ($rule.CoversFile) {
                $directlyCovered = $true
                [void]$managedFiles.Add($file)
            }
            else {
                [void]$sampledFiles.Add($file)
            }

            if (-not $suiteMap.ContainsKey($rule.Project)) {
                $suiteMap[$rule.Project] = @{
                    Filters = (New-Object System.Collections.Generic.HashSet[string])
                    Areas = (New-Object System.Collections.Generic.HashSet[string])
                    Files = (New-Object System.Collections.Generic.HashSet[string])
                    DirectFiles = (New-Object System.Collections.Generic.HashSet[string])
                    SampledFiles = (New-Object System.Collections.Generic.HashSet[string])
                    TrustedBenchmarkFiles = (New-Object System.Collections.Generic.HashSet[string])
                }
            }

            foreach ($filter in $rule.Filters) {
                [void]$suiteMap[$rule.Project].Filters.Add($filter)
            }
            foreach ($trustedBenchmarkFile in @(
                $rule.TrustedBenchmarkFiles |
                    Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) }
            )) {
                [void]$suiteMap[$rule.Project].TrustedBenchmarkFiles.Add($trustedBenchmarkFile)
            }
            [void]$suiteMap[$rule.Project].Areas.Add($rule.Area)
            [void]$suiteMap[$rule.Project].Files.Add($file)
            if ($rule.CoversFile) {
                [void]$suiteMap[$rule.Project].DirectFiles.Add($file)
            }
            else {
                [void]$suiteMap[$rule.Project].SampledFiles.Add($file)
            }
        }
    }

    if (-not $matchedManagedRule -or -not $directlyCovered) {
        [void]$staticOnlyFiles.Add($file)
    }

    if (-not $matchedManagedRule) {
        foreach ($family in $familyDefinitions) {
            if (@($family.pathPatterns | Where-Object { $file -imatch $_ }).Count -eq 0) {
                continue
            }

            if (-not $suiteMap.ContainsKey($family.project)) {
                $suiteMap[$family.project] = @{
                    Filters = (New-Object System.Collections.Generic.HashSet[string])
                    Areas = (New-Object System.Collections.Generic.HashSet[string])
                    Files = (New-Object System.Collections.Generic.HashSet[string])
                    DirectFiles = (New-Object System.Collections.Generic.HashSet[string])
                    SampledFiles = (New-Object System.Collections.Generic.HashSet[string])
                    TrustedBenchmarkFiles = (New-Object System.Collections.Generic.HashSet[string])
                }
            }

            foreach ($filter in @($family.filters)) {
                [void]$suiteMap[$family.project].Filters.Add($filter)
            }
            foreach ($trustedBenchmarkFile in @($family.trustedBenchmarkFiles | Where-Object { $_ })) {
                [void]$suiteMap[$family.project].TrustedBenchmarkFiles.Add($trustedBenchmarkFile)
            }
            [void]$suiteMap[$family.project].Areas.Add("Family: $($family.title)")
            [void]$suiteMap[$family.project].Files.Add($file)
            [void]$suiteMap[$family.project].SampledFiles.Add($file)
            [void]$sampledFiles.Add($file)
        }
    }
}

$changedBenchmarkFilesByProject = @{}
foreach ($project in $BenchmarkInputs.Keys) {
    $roots = @($BenchmarkInputs[$project])
    $changedBenchmarkFilesByProject[$project] = @(
        $changed | Where-Object {
            $candidate = $_
            @($roots | Where-Object { $candidate -like "$_*" }).Count -gt 0
        }
    )
}

$suites = @(
    foreach ($project in $suiteMap.Keys) {
        $changedInputs = @($changedBenchmarkFilesByProject[$project])
        $suiteWideChangedInputs = @(
            $changed | Where-Object {
                $candidate = $_
                @($SuiteWideBenchmarkInputs[$project] | Where-Object {
                    $candidate -like "$_*"
                }).Count -gt 0
            }
        )
        $filterInputChanges = @(
            foreach ($filter in @($suiteMap[$project].Filters | Sort-Object)) {
                $filterToken = $filter.Trim("*")
                $benchmarkClass = ($filterToken -split '\.')[0]
                $trustedFilterFile = @(
                    $suiteMap[$project].TrustedBenchmarkFiles | Where-Object {
                        [IO.Path]::GetFileNameWithoutExtension($_) -eq $benchmarkClass
                    }
                ).Count -gt 0
                $classInputs = if ($filter -eq "*") {
                    $changedInputs
                }
                elseif ($trustedFilterFile) {
                    @()
                }
                else {
                    @(
                        $changedInputs | Where-Object {
                            [IO.Path]::GetFileNameWithoutExtension($_) -eq $benchmarkClass
                        }
                    )
                }
                $filterChanges = @($suiteWideChangedInputs + $classInputs | Sort-Object -Unique)
                [PSCustomObject]@{
                    filter = $filter
                    benchmarkInputsChanged = $filterChanges.Count -gt 0
                    changedBenchmarkInputFiles = $filterChanges
                }
            }
        )
        $runnableFilters = @(
            $filterInputChanges |
                Where-Object { -not $_.benchmarkInputsChanged } |
                ForEach-Object { $_.filter }
        )
        $changedFilters = @(
            $filterInputChanges |
                Where-Object { $_.benchmarkInputsChanged } |
                ForEach-Object { $_.filter }
        )
        [PSCustomObject]@{
            project = $project
            csproj = $Projects[$project]
            filters = @($suiteMap[$project].Filters | Sort-Object)
            areas = @($suiteMap[$project].Areas | Sort-Object)
            matchedFiles = @($suiteMap[$project].Files | Sort-Object)
            directlyCoveredFiles = @($suiteMap[$project].DirectFiles | Sort-Object)
            sampledFiles = @($suiteMap[$project].SampledFiles | Sort-Object)
            benchmarkInputRoots = @($BenchmarkInputs[$project])
            benchmarkInputsChanged = ($filterInputChanges.Count -gt 0 -and $runnableFilters.Count -eq 0)
            runnableFilters = $runnableFilters
            changedFilters = $changedFilters
            filterInputChanges = $filterInputChanges
            trustedBenchmarkFiles = @($suiteMap[$project].TrustedBenchmarkFiles | Sort-Object)
            changedBenchmarkInputFiles = @($changedInputs)
            evidenceTier = if ($suiteMap[$project].SampledFiles.Count -gt 0) { "managed-mixed" } else { "managed-measured" }
        }
    }
) | Sort-Object project

$deviceScenarios = @(
    foreach ($id in $scenarioMap.Keys) {
        $definition = $scenarioMap[$id].Definition
        [PSCustomObject]@{
            id = $definition.id
            title = $definition.title
            platforms = @($definition.platforms)
            runner = $definition.runner
            automationStatus = $definition.automationStatus
            resultScenario = $definition.resultScenario
            pipeline = $definition.pipeline
            rationale = $definition.rationale
            setup = @($definition.setup)
            operations = @($definition.operations)
            metrics = @($definition.metrics)
            matchedFiles = @($scenarioMap[$id].Files | Sort-Object)
            coverageMode = if ($definition.coverageMode) { [string]$definition.coverageMode } else { "direct" }
            evidenceTier = if ([string]$definition.coverageMode -eq "sampled") { "device-sampled" } else { "device-required" }
        }
    }
) | Sort-Object id

$changedBenchmarkFiles = @(
    foreach ($project in $changedBenchmarkFilesByProject.Keys) {
        $changedBenchmarkFilesByProject[$project]
    }
) | Sort-Object -Unique

$changedBenchmarkFiles = @(
    $changedBenchmarkFiles | Where-Object { $_ }
)

$productCount = $productFiles.Count
$managedCount = $managedFiles.Count
$sampledCount = $sampledFiles.Count
$deviceCount = $deviceFiles.Count
$staticCount = $staticOnlyFiles.Count
$selectedBenchmarkInputsChanged = @(
    $suites | Where-Object { @($_.changedFilters).Count -gt 0 }
).Count -gt 0

$coverageStatus = if ($productCount -eq 0) {
    "none"
}
elseif ($managedCount -eq $productCount -and $selectedBenchmarkInputsChanged) {
    "managed-inputs-changed"
}
elseif ($managedCount -eq $productCount) {
    "managed-complete"
}
elseif ($managedCount -gt 0) {
    "mixed"
}
elseif ($deviceCount -gt 0 -and $staticCount -eq 0) {
    "device-required"
}
else {
    "static-only"
}

$result = [PSCustomObject]@{
    schemaVersion = 2
    prNumber = $PrNumber
    baseRef = if ($PrNumber -gt 0) { "pr-base" } else { $BaseBranch }
    changedFileCount = $changed.Count
    productFiles = @($productFiles)
    hasProductChanges = ($productCount -gt 0)
    relevant = ($productCount -gt 0)
    suites = @($suites)
    sampledProductFiles = @($sampledFiles | Sort-Object)
    deviceSampledProductFiles = @($deviceSampledFiles | Sort-Object)
    deviceScenarios = @($deviceScenarios)
    staticOnlyProductFiles = @($staticOnlyFiles | Sort-Object)
    unmappedProductFiles = @(
        @($deviceFiles | Sort-Object) + @($staticOnlyFiles | Sort-Object) |
            Sort-Object -Unique
    )
    requiresDeviceMeasurement = (@($deviceScenarios).Count -gt 0)
    benchmarkFilesChanged = ($changedBenchmarkFiles.Count -gt 0)
    changedBenchmarkFiles = @($changedBenchmarkFiles)
    benchmarkFamilies = @(
        $familyDefinitions | ForEach-Object {
            [PSCustomObject]@{
                id = $_.id
                title = $_.title
                project = $_.project
            }
        }
    )
    coverage = [PSCustomObject]@{
        status = $coverageStatus
        productFileCount = $productCount
        managedMeasuredFileCount = $managedCount
        managedSampledFileCount = $sampledCount
        deviceSampledFileCount = $deviceSampledFiles.Count
        deviceRequiredFileCount = $deviceCount
        staticOnlyFileCount = $staticCount
        managedCoverageComplete = ($productCount -gt 0 -and $managedCount -eq $productCount)
        benchmarkInputsChanged = $selectedBenchmarkInputsChanged
        canClaimWholePrClean = (
            $productCount -gt 0 -and
            $managedCount -eq $productCount -and
            -not $selectedBenchmarkInputsChanged
        )
    }
}

$json = $result | ConvertTo-Json -Depth 10
if ($OutputPath -eq "-") {
    Write-Output $json
}
else {
    $directory = Split-Path -Parent $OutputPath
    if ($directory -and -not (Test-Path $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    Set-Content -Path $OutputPath -Value $json -Encoding UTF8
    Write-Info "Wrote $OutputPath"
}

if ($productCount -eq 0) {
    Write-Info "No performance-sensitive product code changed."
    exit 3
}

Write-Info "Coverage: $coverageStatus (managed=$managedCount sampled=$sampledCount device=$deviceCount static=$staticCount)"
foreach ($suite in $suites) {
    Write-Info ("Managed suite: {0} [{1}] filters: {2}" -f $suite.project, ($suite.areas -join ", "), ($suite.filters -join " "))
}
foreach ($scenario in $deviceScenarios) {
    Write-Info ("Device scenario required: {0} ({1})" -f $scenario.id, ($scenario.platforms -join ", "))
}
foreach ($file in @($staticOnlyFiles | Sort-Object)) {
    Write-Info "Static-only: $file"
}

exit 0
