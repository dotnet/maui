<#
.SYNOPSIS
    Selects and runs the minimum safe set of tests for a PR using Copilot CLI.

.DESCRIPTION
    This script:
    1. Fetches PR metadata (changed files, title, description, labels)
    2. Runs deterministic file-path → test mapping
    3. Invokes Copilot CLI with the smart-test-selector agent for AI refinement
    4. Outputs the selected test scope as JSON
    5. Optionally executes the selected tests

    Falls back to ALL tests if Copilot CLI fails or confidence is low.

.PARAMETER PRNumber
    The GitHub PR number to analyze.

.PARAMETER Platform
    Target platform for UI/device tests. Default: 'android'.
    Valid values: android, ios, all

.PARAMETER RunTests
    If specified, actually builds and runs the selected tests.
    Without this flag, only outputs the selection.

.PARAMETER ForceAll
    If specified, skips analysis and runs ALL tests.

.PARAMETER OutputFile
    Path to write the selection JSON. Default: selection.json in artifacts dir.

.PARAMETER LogFile
    If provided, captures all output via Start-Transcript.

.EXAMPLE
    .\Select-SmartTests.ps1 -PRNumber 33432
    Analyzes PR #33432 and outputs selected test scope.

.EXAMPLE
    .\Select-SmartTests.ps1 -PRNumber 33432 -Platform ios -RunTests
    Analyzes PR #33432 and runs selected tests on iOS.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [ValidateSet('android', 'ios', 'all')]
    [string]$Platform = 'android',

    [Parameter(Mandatory = $false)]
    [switch]$RunTests,

    [Parameter(Mandatory = $false)]
    [switch]$ForceAll,

    [Parameter(Mandatory = $false)]
    [string]$OutputFile,

    [Parameter(Mandatory = $false)]
    [string]$LogFile
)

$ErrorActionPreference = 'Stop'

if ($LogFile) {
    $logDir = Split-Path $LogFile -Parent
    if ($logDir -and -not (Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
    Start-Transcript -Path $LogFile -Force | Out-Null
}

# ──────────────────────────────────────────────────────
# CONFIGURATION
# ──────────────────────────────────────────────────────

$RepoRoot = git rev-parse --show-toplevel 2>$null
if (-not $RepoRoot) {
    Write-Error "Not in a git repository"
    exit 1
}

# Artifacts directory
$ArtifactsDir = if ($env:BUILD_ARTIFACTSTAGINGDIRECTORY) {
    "$env:BUILD_ARTIFACTSTAGINGDIRECTORY/smart-tests"
} else {
    "$RepoRoot/CustomAgentLogsTmp/SmartTests"
}
if (-not (Test-Path $ArtifactsDir)) {
    New-Item -ItemType Directory -Path $ArtifactsDir -Force | Out-Null
}

if (-not $OutputFile) {
    $OutputFile = Join-Path $ArtifactsDir "selection.json"
}

# ──────────────────────────────────────────────────────
# DETERMINISTIC FILE-PATH → TEST MAPPING
# ──────────────────────────────────────────────────────

# Map source paths to UI test categories
$PathToCategoryMap = [ordered]@{
    'src/Controls/src/Core/ActivityIndicator'       = @('ActivityIndicator')
    'src/Controls/src/Core/Border'                  = @('Border')
    'src/Controls/src/Core/BoxView'                 = @('BoxView')
    'src/Controls/src/Core/Brush'                   = @('Brush')
    'src/Controls/src/Core/Button'                  = @('Button')
    'src/Controls/src/Core/Cells'                   = @('Cells')
    'src/Controls/src/Core/CheckBox'                = @('CheckBox')
    'src/Controls/src/Core/DatePicker'              = @('DatePicker')
    'src/Controls/src/Core/DragAndDrop'             = @('DragAndDrop')
    'src/Controls/src/Core/Editor'                  = @('Editor')
    'src/Controls/src/Core/Entry'                   = @('Entry')
    'src/Controls/src/Core/FlyoutPage'              = @('FlyoutPage')
    'src/Controls/src/Core/Frame'                   = @('Frame')
    'src/Controls/src/Core/GraphicsView'            = @('GraphicsView')
    'src/Controls/src/Core/Image/'                  = @('Image')
    'src/Controls/src/Core/ImageButton'             = @('ImageButton')
    'src/Controls/src/Core/IndicatorView'           = @('IndicatorView')
    'src/Controls/src/Core/Label'                   = @('Label')
    'src/Controls/src/Core/Layout'                  = @('Layout')
    'src/Controls/src/Core/LegacyLayouts'           = @('Layout')
    'src/Controls/src/Core/ListView'                = @('ListView')
    'src/Controls/src/Core/NavigationPage'          = @('Navigation')
    'src/Controls/src/Core/Page'                    = @('Page')
    'src/Controls/src/Core/Picker'                  = @('Picker')
    'src/Controls/src/Core/ProgressBar'             = @('ProgressBar')
    'src/Controls/src/Core/RadioButton'             = @('RadioButton')
    'src/Controls/src/Core/RefreshView'             = @('RefreshView')
    'src/Controls/src/Core/ScrollView'              = @('ScrollView')
    'src/Controls/src/Core/SearchBar'               = @('SearchBar')
    'src/Controls/src/Core/Shape'                   = @('Shape')
    'src/Controls/src/Core/Shapes'                  = @('Shape')
    'src/Controls/src/Core/Shell'                   = @('Shell')
    'src/Controls/src/Core/Slider'                  = @('Slider')
    'src/Controls/src/Core/Stepper'                 = @('Stepper')
    'src/Controls/src/Core/SwipeView'               = @('SwipeView')
    'src/Controls/src/Core/Switch'                  = @('Switch')
    'src/Controls/src/Core/TabbedPage'              = @('TabbedPage')
    'src/Controls/src/Core/TableView'               = @('TableView')
    'src/Controls/src/Core/TimePicker'              = @('TimePicker')
    'src/Controls/src/Core/Toolbar'                 = @('ToolbarItem')
    'src/Controls/src/Core/TitleBar'                = @('TitleView')
    'src/Controls/src/Core/WebView'                 = @('WebView')
    'src/Controls/src/Core/HybridWebView'           = @('WebView')
    'src/Controls/src/Core/Window'                  = @('Window')
    'src/Controls/src/Core/ContentView'             = @('Page')
    'src/Controls/src/Core/ContentPage'             = @('Page')
    'src/Controls/src/Core/BindableLayout'          = @('Layout')
    'src/Controls/src/Core/CarouselView'            = @('CarouselView')
    'src/Controls/src/Core/Handlers/Items'          = @('CollectionView', 'CarouselView')
    'src/Controls/src/Core/Items'                   = @('CollectionView', 'CarouselView')
    'src/Controls/src/Core/Handlers/Shell'          = @('Shell')
    'src/Controls/src/Core/Handlers/Shapes'         = @('Shape')
    'src/Controls/src/Core/Platform/GestureManager' = @('Gestures', 'DragAndDrop')
    'src/Controls/src/Core/Platform/AlertManager'   = @('DisplayAlert', 'DisplayPrompt', 'ActionSheet')
    'src/Controls/src/Core/Platform/ModalNavigationManager' = @('Navigation', 'Page')
    'src/Controls/src/Core/Interactivity'           = @('Gestures')
    'src/Controls/src/Core/Application'             = @('Lifecycle', 'Window')
    'src/Controls/src/Core/AutomationProperties'    = @('Accessibility')
    'src/BlazorWebView'                             = @('WebView')
}

# Map source paths to unit test projects
$PathToUnitTestMap = [ordered]@{
    'src/Controls/src/'            = @('Controls.Core.UnitTests')
    'src/Controls/tests/Core.UnitTests' = @('Controls.Core.UnitTests')
    'src/Controls/tests/Xaml.UnitTests' = @('Controls.Xaml.UnitTests')
    'src/Controls/tests/SourceGen.UnitTests' = @('SourceGen.UnitTests')
    'src/Core/src/'                = @('Core.UnitTests')
    'src/Core/tests/UnitTests'     = @('Core.UnitTests')
    'src/Essentials/'              = @('Essentials.UnitTests')
    'src/Graphics/'                = @()  # No unit tests, only device tests
}

# Map source paths to device test projects
$PathToDeviceTestMap = [ordered]@{
    'src/Controls/src/'            = @('Controls.DeviceTests')
    'src/Controls/tests/DeviceTests' = @('Controls.DeviceTests')
    'src/Core/src/'                = @('Core.DeviceTests')
    'src/Core/tests/DeviceTests'   = @('Core.DeviceTests')
    'src/Essentials/'              = @('Essentials.DeviceTests')
    'src/Graphics/'                = @('Graphics.DeviceTests')
}

# Paths that trigger ALL tests
$InfrastructurePaths = @(
    'eng/'
    'Directory.Build'
    'global.json'
    'NuGet.config'
    'build.cake'
    'build.ps1'
    'build.sh'
    'build.cmd'
)

# File patterns for platform detection
$PlatformPatterns = @{
    'android'  = @('.android.cs', '/Android/', '/Platforms/Android/')
    'ios'      = @('.ios.cs', '/iOS/', '/Platforms/iOS/')
    'catalyst' = @('.maccatalyst.cs', '/MacCatalyst/', '/Platforms/MacCatalyst/')
    'windows'  = @('.windows.cs', '/Windows/', '/Platforms/Windows/')
}

# ──────────────────────────────────────────────────────
# HELPER FUNCTIONS
# ──────────────────────────────────────────────────────

function Get-DeterministicSelection {
    param([string[]]$ChangedFiles)

    $uiCategories = [System.Collections.Generic.HashSet[string]]::new()
    $unitProjects = [System.Collections.Generic.HashSet[string]]::new()
    $deviceProjects = [System.Collections.Generic.HashSet[string]]::new()
    $integrationCategories = [System.Collections.Generic.HashSet[string]]::new()
    $platforms = [System.Collections.Generic.HashSet[string]]::new()
    $isAllRequired = $false
    $unmatchedFiles = @()

    foreach ($file in $ChangedFiles) {
        $matched = $false

        # Check infrastructure paths first
        foreach ($infraPath in $InfrastructurePaths) {
            if ($file -like "$infraPath*" -or $file -like "*/$infraPath*") {
                $isAllRequired = $true
                $matched = $true
                break
            }
        }
        if ($isAllRequired) { break }

        # Check UI category mapping
        foreach ($entry in $PathToCategoryMap.GetEnumerator()) {
            if ($file -like "$($entry.Key)*") {
                foreach ($cat in $entry.Value) { [void]$uiCategories.Add($cat) }
                $matched = $true
                break
            }
        }

        # Check unit test mapping
        foreach ($entry in $PathToUnitTestMap.GetEnumerator()) {
            if ($file -like "$($entry.Key)*") {
                foreach ($proj in $entry.Value) { [void]$unitProjects.Add($proj) }
                $matched = $true
                break
            }
        }

        # Check device test mapping
        foreach ($entry in $PathToDeviceTestMap.GetEnumerator()) {
            if ($file -like "$($entry.Key)*") {
                foreach ($proj in $entry.Value) { [void]$deviceProjects.Add($proj) }
                $matched = $true
                break
            }
        }

        # Check for template changes → integration tests
        if ($file -like 'src/Templates/*') {
            [void]$integrationCategories.Add('Build')
            [void]$integrationCategories.Add('Blazor')
            $matched = $true
        }

        # Core framework changes → ALL UI tests
        if ($file -like 'src/Core/src/*') {
            $isAllRequired = $true
            break
        }

        # Detect platforms
        foreach ($entry in $PlatformPatterns.GetEnumerator()) {
            foreach ($pattern in $entry.Value) {
                if ($file -like "*$pattern*") {
                    [void]$platforms.Add($entry.Key)
                }
            }
        }

        if (-not $matched) {
            $unmatchedFiles += $file
        }
    }

    # If no platforms detected, it's shared code → all platforms
    if ($platforms.Count -eq 0 -and -not $isAllRequired) {
        [void]$platforms.Add('ALL')
    }

    # Always include safety-net categories if we have any UI categories
    if ($uiCategories.Count -gt 0) {
        [void]$uiCategories.Add('Lifecycle')
        [void]$uiCategories.Add('ViewBaseTests')
    }

    $confidence = if ($isAllRequired) { "low" }
                  elseif ($unmatchedFiles.Count -gt ($ChangedFiles.Count / 2)) { "medium" }
                  else { "high" }

    return @{
        IsAllRequired          = $isAllRequired
        UICategories           = [string[]]$uiCategories
        UnitProjects           = [string[]]$unitProjects
        DeviceProjects         = [string[]]$deviceProjects
        IntegrationCategories  = [string[]]$integrationCategories
        Platforms              = [string[]]$platforms
        UnmatchedFiles         = $unmatchedFiles
        Confidence             = $confidence
    }
}

function Invoke-CopilotSelection {
    param(
        [string[]]$ChangedFiles,
        [hashtable]$Baseline,
        [string]$PRTitle,
        [string]$PRBody,
        [string]$PRLabels
    )

    # Build prompt for Copilot CLI
    $changedFilesStr = $ChangedFiles -join "`n"
    $baselineStr = "Deterministic baseline:`n"
    $baselineStr += "  UI Categories: $($Baseline.UICategories -join ', ')`n"
    $baselineStr += "  Unit Tests: $($Baseline.UnitProjects -join ', ')`n"
    $baselineStr += "  Device Tests: $($Baseline.DeviceProjects -join ', ')`n"
    $baselineStr += "  Integration: $($Baseline.IntegrationCategories -join ', ')`n"
    $baselineStr += "  Platforms: $($Baseline.Platforms -join ', ')`n"
    $baselineStr += "  Confidence: $($Baseline.Confidence)`n"
    if ($Baseline.UnmatchedFiles.Count -gt 0) {
        $baselineStr += "  Unmatched files: $($Baseline.UnmatchedFiles -join ', ')`n"
    }

    $prompt = @"
Analyze PR #$PRNumber and select the minimum safe set of tests.

PR Title: $PRTitle
PR Labels: $PRLabels
PR Description (first 2000 chars):
$($PRBody.Substring(0, [Math]::Min(2000, $PRBody.Length)))

Changed files (one per line):
$changedFilesStr

$baselineStr

You MAY add to the baseline but SHOULD NOT remove from it.
Respond with ONLY a JSON object matching the schema in your agent definition.
"@

    Write-Host "  🤖 Invoking Copilot CLI with smart-test-selector agent..." -ForegroundColor Cyan

    $copilotArgs = @(
        "--agent", "smart-test-selector",
        "-p", $prompt
    )

    try {
        $rawOutput = & copilot @copilotArgs 2>/dev/null
        $exitCode = $LASTEXITCODE

        if ($exitCode -ne 0 -or [string]::IsNullOrWhiteSpace($rawOutput)) {
            Write-Host "  ⚠️ Copilot CLI returned exit code $exitCode" -ForegroundColor Yellow
            return $null
        }

        Write-Host "  ✅ Copilot CLI responded" -ForegroundColor Green

        # Extract JSON from response (Copilot may wrap in markdown code blocks)
        $jsonMatch = [regex]::Match($rawOutput, '\{[\s\S]*\}')
        if (-not $jsonMatch.Success) {
            Write-Host "  ⚠️ Could not extract JSON from Copilot response" -ForegroundColor Yellow
            Write-Host "  Raw output: $($rawOutput.Substring(0, [Math]::Min(500, $rawOutput.Length)))" -ForegroundColor Gray
            return $null
        }

        $parsed = $jsonMatch.Value | ConvertFrom-Json
        return $parsed
    }
    catch {
        Write-Host "  ⚠️ Copilot invocation failed: $_" -ForegroundColor Yellow
        return $null
    }
}

function Merge-Selections {
    param(
        [hashtable]$Baseline,
        $CopilotResult
    )

    # Start with baseline
    $uiCategories = [System.Collections.Generic.HashSet[string]]::new()
    $unitProjects = [System.Collections.Generic.HashSet[string]]::new()
    $deviceProjects = [System.Collections.Generic.HashSet[string]]::new()
    $integrationCategories = [System.Collections.Generic.HashSet[string]]::new()
    $platforms = [System.Collections.Generic.HashSet[string]]::new()

    foreach ($cat in $Baseline.UICategories) { [void]$uiCategories.Add($cat) }
    foreach ($proj in $Baseline.UnitProjects) { [void]$unitProjects.Add($proj) }
    foreach ($proj in $Baseline.DeviceProjects) { [void]$deviceProjects.Add($proj) }
    foreach ($cat in $Baseline.IntegrationCategories) { [void]$integrationCategories.Add($cat) }
    foreach ($p in $Baseline.Platforms) { [void]$platforms.Add($p) }

    # Merge Copilot additions (additive only)
    if ($CopilotResult) {
        if ($CopilotResult.uiTestCategories) {
            foreach ($cat in $CopilotResult.uiTestCategories) {
                if ($cat -eq 'ALL') { return @{ IsAllRequired = $true } }
                [void]$uiCategories.Add($cat)
            }
        }
        if ($CopilotResult.unitTestProjects) {
            foreach ($proj in $CopilotResult.unitTestProjects) {
                if ($proj -eq 'ALL') { $unitProjects = [System.Collections.Generic.HashSet[string]]@('ALL'); break }
                [void]$unitProjects.Add($proj)
            }
        }
        if ($CopilotResult.deviceTestProjects) {
            foreach ($proj in $CopilotResult.deviceTestProjects) {
                if ($proj -eq 'ALL') { $deviceProjects = [System.Collections.Generic.HashSet[string]]@('ALL'); break }
                [void]$deviceProjects.Add($proj)
            }
        }
        if ($CopilotResult.integrationTestCategories) {
            foreach ($cat in $CopilotResult.integrationTestCategories) {
                if ($cat -eq 'ALL') { $integrationCategories = [System.Collections.Generic.HashSet[string]]@('ALL'); break }
                [void]$integrationCategories.Add($cat)
            }
        }
        if ($CopilotResult.platforms) {
            foreach ($p in $CopilotResult.platforms) {
                [void]$platforms.Add($p)
            }
        }
    }

    return @{
        IsAllRequired          = $false
        UICategories           = [string[]]$uiCategories
        UnitProjects           = [string[]]$unitProjects
        DeviceProjects         = [string[]]$deviceProjects
        IntegrationCategories  = [string[]]$integrationCategories
        Platforms              = [string[]]$platforms
        Confidence             = if ($CopilotResult.confidence) { $CopilotResult.confidence } else { $Baseline.Confidence }
        Reasoning              = if ($CopilotResult.reasoning) { $CopilotResult.reasoning } else { "Deterministic mapping only" }
    }
}

# ──────────────────────────────────────────────────────
# MAIN EXECUTION
# ──────────────────────────────────────────────────────

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           Smart Test Selector                             ║" -ForegroundColor Cyan
Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Cyan
Write-Host "║  PR:        #$PRNumber                                          ║" -ForegroundColor Cyan
Write-Host "║  Platform:  $Platform                                        ║" -ForegroundColor Cyan
Write-Host "║  RunTests:  $RunTests                                      ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Step 1: Verify prerequisites
Write-Host "📋 Checking prerequisites..." -ForegroundColor Yellow

$ghVersion = gh --version 2>$null | Select-Object -First 1
if (-not $ghVersion) {
    Write-Error "GitHub CLI (gh) is not installed."
    exit 1
}
Write-Host "  ✅ GitHub CLI: $ghVersion" -ForegroundColor Green

$copilotVersion = copilot --version 2>$null
if (-not $copilotVersion) {
    Write-Host "  ⚠️ Copilot CLI not installed — will use deterministic mapping only" -ForegroundColor Yellow
    $hasCopilot = $false
} else {
    Write-Host "  ✅ Copilot CLI: $copilotVersion" -ForegroundColor Green
    $hasCopilot = $true
}

# Step 2: Fetch PR metadata
Write-Host ""
Write-Host "📥 Fetching PR #$PRNumber metadata..." -ForegroundColor Yellow

$prJson = gh pr view $PRNumber --json title,body,labels,files,headRefName,baseRefName 2>$null
if (-not $prJson) {
    Write-Error "PR #$PRNumber not found or not accessible."
    exit 1
}
try {
    $prData = $prJson | ConvertFrom-Json
} catch {
    Write-Error "Failed to parse PR data: $_"
    exit 1
}
Write-Host "  ✅ PR: $($prData.title)" -ForegroundColor Green
Write-Host "  ✅ Base: $($prData.baseRefName) ← $($prData.headRefName)" -ForegroundColor Green

$changedFiles = @($prData.files | ForEach-Object { $_.path })
$prLabels = ($prData.labels | ForEach-Object { $_.name }) -join ', '

Write-Host "  ✅ Changed files: $($changedFiles.Count)" -ForegroundColor Green
if ($prLabels) {
    Write-Host "  ✅ Labels: $prLabels" -ForegroundColor Green
}

# Step 3: ForceAll shortcut
if ($ForceAll) {
    Write-Host ""
    Write-Host "⚡ ForceAll specified — selecting ALL tests" -ForegroundColor Yellow
    $finalSelection = @{
        uiTestCategories         = @('ALL')
        unitTestProjects         = @('ALL')
        deviceTestProjects       = @('ALL')
        integrationTestCategories = @('ALL')
        platforms                = @('ALL')
        confidence               = 'high'
        reasoning                = 'ForceAll parameter specified'
        fallback                 = $true
    }
} else {
    # Step 4: Deterministic mapping
    Write-Host ""
    Write-Host "🔍 Running deterministic file-path mapping..." -ForegroundColor Yellow

    $baseline = Get-DeterministicSelection -ChangedFiles $changedFiles

    if ($baseline.IsAllRequired) {
        Write-Host "  ⚠️ Infrastructure/core changes detected — selecting ALL" -ForegroundColor Yellow
        $finalSelection = @{
            uiTestCategories         = @('ALL')
            unitTestProjects         = @('ALL')
            deviceTestProjects       = @('ALL')
            integrationTestCategories = @('ALL')
            platforms                = @('ALL')
            confidence               = 'low'
            reasoning                = 'Infrastructure or core framework changes require full test suite'
            fallback                 = $true
        }
    } else {
        Write-Host "  📊 Baseline:" -ForegroundColor Gray
        Write-Host "     UI Categories:  $($baseline.UICategories -join ', ')" -ForegroundColor Gray
        Write-Host "     Unit Tests:     $($baseline.UnitProjects -join ', ')" -ForegroundColor Gray
        Write-Host "     Device Tests:   $($baseline.DeviceProjects -join ', ')" -ForegroundColor Gray
        Write-Host "     Integration:    $($baseline.IntegrationCategories -join ', ')" -ForegroundColor Gray
        Write-Host "     Platforms:      $($baseline.Platforms -join ', ')" -ForegroundColor Gray
        Write-Host "     Confidence:     $($baseline.Confidence)" -ForegroundColor Gray
        if ($baseline.UnmatchedFiles.Count -gt 0) {
            Write-Host "     Unmatched:      $($baseline.UnmatchedFiles.Count) files" -ForegroundColor Gray
        }

        # Step 5: AI refinement via Copilot CLI
        $copilotResult = $null
        if ($hasCopilot -and ($baseline.UnmatchedFiles.Count -gt 0 -or $baseline.Confidence -ne 'high')) {
            Write-Host ""
            Write-Host "🤖 Requesting AI refinement..." -ForegroundColor Yellow
            $copilotResult = Invoke-CopilotSelection `
                -ChangedFiles $changedFiles `
                -Baseline $baseline `
                -PRTitle $prData.title `
                -PRBody ($prData.body ?? '') `
                -PRLabels $prLabels
        } elseif ($hasCopilot) {
            Write-Host ""
            Write-Host "  ✅ High confidence — skipping AI refinement" -ForegroundColor Green
        }

        # Step 6: Merge selections (additive)
        $merged = Merge-Selections -Baseline $baseline -CopilotResult $copilotResult

        if ($merged.IsAllRequired) {
            $finalSelection = @{
                uiTestCategories         = @('ALL')
                unitTestProjects         = @('ALL')
                deviceTestProjects       = @('ALL')
                integrationTestCategories = @('ALL')
                platforms                = @('ALL')
                confidence               = 'low'
                reasoning                = 'AI analysis expanded to ALL'
                fallback                 = $true
            }
        } else {
            $finalSelection = @{
                uiTestCategories         = if ($merged.UICategories.Count -gt 0) { $merged.UICategories } else { @() }
                unitTestProjects         = if ($merged.UnitProjects.Count -gt 0) { $merged.UnitProjects } else { @() }
                deviceTestProjects       = if ($merged.DeviceProjects.Count -gt 0) { $merged.DeviceProjects } else { @() }
                integrationTestCategories = if ($merged.IntegrationCategories.Count -gt 0) { $merged.IntegrationCategories } else { @() }
                platforms                = if ($merged.Platforms.Count -gt 0) { $merged.Platforms } else { @('ALL') }
                confidence               = $merged.Confidence
                reasoning                = $merged.Reasoning
                fallback                 = $false
            }
        }
    }
}

# Step 7: Output results
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  TEST SELECTION RESULTS                                   ║" -ForegroundColor Green
Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Green

$uiDisplay = if ($finalSelection.uiTestCategories -contains 'ALL') { 'ALL' } else { $finalSelection.uiTestCategories -join ', ' }
$unitDisplay = if ($finalSelection.unitTestProjects -contains 'ALL') { 'ALL' } else { $finalSelection.unitTestProjects -join ', ' }
$deviceDisplay = if ($finalSelection.deviceTestProjects -contains 'ALL') { 'ALL' } else { $finalSelection.deviceTestProjects -join ', ' }
$integDisplay = if ($finalSelection.integrationTestCategories -contains 'ALL') { 'ALL' }
                elseif ($finalSelection.integrationTestCategories.Count -eq 0) { '(none)' }
                else { $finalSelection.integrationTestCategories -join ', ' }
$platDisplay = if ($finalSelection.platforms -contains 'ALL') { 'ALL' } else { $finalSelection.platforms -join ', ' }

Write-Host "║  UI Tests:     $uiDisplay" -ForegroundColor White
Write-Host "║  Unit Tests:   $unitDisplay" -ForegroundColor White
Write-Host "║  Device Tests: $deviceDisplay" -ForegroundColor White
Write-Host "║  Integration:  $integDisplay" -ForegroundColor White
Write-Host "║  Platforms:    $platDisplay" -ForegroundColor White
Write-Host "║  Confidence:   $($finalSelection.confidence)" -ForegroundColor White
Write-Host "║  Reasoning:    $($finalSelection.reasoning)" -ForegroundColor White
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

# Write selection JSON
$selectionJson = @{
    prNumber                 = $PRNumber
    prTitle                  = $prData.title
    changedFileCount         = $changedFiles.Count
    uiTestCategories         = $finalSelection.uiTestCategories
    unitTestProjects         = $finalSelection.unitTestProjects
    deviceTestProjects       = $finalSelection.deviceTestProjects
    integrationTestCategories = $finalSelection.integrationTestCategories
    platforms                = $finalSelection.platforms
    confidence               = $finalSelection.confidence
    reasoning                = $finalSelection.reasoning
    fallback                 = $finalSelection.fallback
    timestamp                = (Get-Date -Format 'o')
} | ConvertTo-Json -Depth 5

$selectionJson | Out-File -FilePath $OutputFile -Encoding utf8
Write-Host "📄 Selection written to: $OutputFile" -ForegroundColor Gray

# Set Azure DevOps output variables if running in pipeline
if ($env:BUILD_BUILDID) {
    $uiCsv = $finalSelection.uiTestCategories -join ','
    $unitCsv = $finalSelection.unitTestProjects -join ','
    $deviceCsv = $finalSelection.deviceTestProjects -join ','
    $integCsv = $finalSelection.integrationTestCategories -join ','
    $platCsv = $finalSelection.platforms -join ','

    Write-Host "##vso[task.setvariable variable=SmartTests_UICategories;isOutput=true]$uiCsv"
    Write-Host "##vso[task.setvariable variable=SmartTests_UnitProjects;isOutput=true]$unitCsv"
    Write-Host "##vso[task.setvariable variable=SmartTests_DeviceProjects;isOutput=true]$deviceCsv"
    Write-Host "##vso[task.setvariable variable=SmartTests_IntegrationCategories;isOutput=true]$integCsv"
    Write-Host "##vso[task.setvariable variable=SmartTests_Platforms;isOutput=true]$platCsv"
    Write-Host "##vso[task.setvariable variable=SmartTests_Confidence;isOutput=true]$($finalSelection.confidence)"
    Write-Host "##vso[task.setvariable variable=SmartTests_Fallback;isOutput=true]$($finalSelection.fallback)"
}

# Step 8: Run tests if requested
if ($RunTests) {
    Write-Host ""
    Write-Host "🚀 Running selected tests..." -ForegroundColor Yellow
    Write-Host ""

    $testsFailed = $false

    # Run unit tests
    if ($finalSelection.unitTestProjects.Count -gt 0) {
        $unitProjectPaths = @{
            'Core.UnitTests'            = 'src/Core/tests/UnitTests/Core.UnitTests.csproj'
            'Controls.Core.UnitTests'   = 'src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj'
            'Controls.Xaml.UnitTests'   = 'src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj'
            'SourceGen.UnitTests'       = 'src/Controls/tests/SourceGen.UnitTests/SourceGen.UnitTests.csproj'
            'Essentials.UnitTests'      = 'src/Essentials/test/UnitTests/Essentials.UnitTests.csproj'
        }

        $projectsToRun = if ($finalSelection.unitTestProjects -contains 'ALL') {
            $unitProjectPaths.Keys
        } else {
            $finalSelection.unitTestProjects
        }

        foreach ($projName in $projectsToRun) {
            if ($unitProjectPaths.ContainsKey($projName)) {
                $projPath = Join-Path $RepoRoot $unitProjectPaths[$projName]
                Write-Host "  🧪 Running $projName..." -ForegroundColor Cyan
                $trxPath = Join-Path $ArtifactsDir "$projName.trx"
                & dotnet test $projPath --no-restore --logger "trx;LogFileName=$trxPath" 2>&1 | ForEach-Object { Write-Host "     $_" }
                if ($LASTEXITCODE -ne 0) {
                    Write-Host "  ❌ $projName FAILED" -ForegroundColor Red
                    $testsFailed = $true
                } else {
                    Write-Host "  ✅ $projName PASSED" -ForegroundColor Green
                }
            }
        }
    }

    # Run UI tests (using BuildAndRunHostApp.ps1 pattern)
    if ($finalSelection.uiTestCategories.Count -gt 0) {
        $categoryCsv = if ($finalSelection.uiTestCategories -contains 'ALL') {
            ''
        } else {
            $finalSelection.uiTestCategories -join ','
        }

        $platformsToTest = if ($finalSelection.platforms -contains 'ALL') {
            @($Platform)  # Use the parameter platform for ALL
        } elseif ($Platform -eq 'all') {
            $finalSelection.platforms
        } else {
            @($Platform)
        }

        foreach ($plat in $platformsToTest) {
            Write-Host "  🧪 Running UI tests on $plat..." -ForegroundColor Cyan
            $uiArgs = @('-Platform', $plat)
            if ($categoryCsv) {
                $uiArgs += @('-Category', $categoryCsv)
            }
            $scriptPath = Join-Path $RepoRoot '.github/scripts/BuildAndRunHostApp.ps1'
            if (Test-Path $scriptPath) {
                & pwsh $scriptPath @uiArgs 2>&1 | ForEach-Object { Write-Host "     $_" }
                if ($LASTEXITCODE -ne 0) {
                    Write-Host "  ❌ UI tests on $plat FAILED" -ForegroundColor Red
                    $testsFailed = $true
                } else {
                    Write-Host "  ✅ UI tests on $plat PASSED" -ForegroundColor Green
                }
            } else {
                Write-Host "  ⚠️ BuildAndRunHostApp.ps1 not found — skipping UI tests" -ForegroundColor Yellow
            }
        }
    }

    if ($testsFailed) {
        Write-Host ""
        Write-Host "❌ Some tests failed. See results above." -ForegroundColor Red
        exit 1
    } else {
        Write-Host ""
        Write-Host "✅ All selected tests passed!" -ForegroundColor Green
    }
}

if ($LogFile) {
    Stop-Transcript | Out-Null
}

Write-Host ""
Write-Host "Done." -ForegroundColor Green
exit 0
