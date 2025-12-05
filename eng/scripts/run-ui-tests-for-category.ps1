#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Runs UI tests for a specific category from the command line

.DESCRIPTION
    This script allows running UI tests for a specific category or set of categories
    from the command line, useful for local development and debugging.

.PARAMETER Category
    Single category to test (e.g., "Button", "Label", "Entry")

.PARAMETER CategoryGroup
    Category group to test (e.g., "Button,Label,Entry")

.PARAMETER Platform
    Platform to test: android, ios, windows, catalyst (default: android)

.PARAMETER Configuration
    Build configuration (default: Release)

.PARAMETER PrNumber
    Optional PR number to analyze for intelligent test selection

.PARAMETER ListCategories
    List all available test categories

.EXAMPLE
    ./run-ui-tests-for-category.ps1 -Category Button -Platform android

.EXAMPLE
    ./run-ui-tests-for-category.ps1 -CategoryGroup "Button,Label,Entry" -Platform ios

.EXAMPLE
    ./run-ui-tests-for-category.ps1 -PrNumber 12345
    # Analyzes PR and runs only affected categories

.EXAMPLE
    ./run-ui-tests-for-category.ps1 -ListCategories
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$Category,
    
    [Parameter(Mandatory=$false)]
    [string]$CategoryGroup,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("android", "ios", "windows", "catalyst")]
    [string]$Platform = "android",
    
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [string]$PrNumber,
    
    [Parameter(Mandatory=$false)]
    [switch]$ListCategories
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path "$PSScriptRoot/../.."

# List available categories
if ($ListCategories) {
    Write-Host "=== Available UI Test Categories ===" -ForegroundColor Cyan
    Write-Host ""
    
    $categoriesFile = "$RepoRoot/src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs"
    if (Test-Path $categoriesFile) {
        $content = Get-Content $categoriesFile
        $categories = $content | Select-String 'public const string (\w+) = "(\w+)"' -AllMatches | 
            ForEach-Object { $_.Matches } | 
            ForEach-Object { $_.Groups[1].Value } | 
            Sort-Object
        
        Write-Host "Individual Categories:" -ForegroundColor Yellow
        $categories | ForEach-Object { Write-Host "  - $_" -ForegroundColor White }
        
        Write-Host ""
        Write-Host "Common Category Groups (from pipeline):" -ForegroundColor Yellow
        $groups = @(
            "Accessibility,ActionSheet,ActivityIndicator,Animation,AppLinks",
            "Border,BoxView,Brush,Button",
            "CarouselView",
            "CollectionView",
            "Entry",
            "Label,Layout,Lifecycle,ListView",
            "Navigation",
            "SafeAreaEdges",
            "Shell"
        )
        $groups | ForEach-Object { Write-Host "  - $_" -ForegroundColor White }
    }
    
    exit 0
}

Write-Host "=== UI Test Category Runner ===" -ForegroundColor Cyan

# If PR number provided, analyze and determine categories
if ($PrNumber) {
    Write-Host "Analyzing PR #$PrNumber for intelligent test selection..." -ForegroundColor Yellow
    
    $analysisScript = "$RepoRoot/eng/scripts/analyze-pr-changes.ps1"
    & $analysisScript -PrNumber $PrNumber -OutputFile "$RepoRoot/test-categories.txt"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to analyze PR #$PrNumber"
        exit 1
    }
    
    # Read the categories
    $categories = Get-Content "$RepoRoot/test-categories.txt" | Where-Object { $_.Trim() -ne "" }
    
    if ($categories.Count -eq 0) {
        Write-Host "No UI tests needed for this PR (documentation only?)" -ForegroundColor Green
        exit 0
    }
    
    Write-Host ""
    Write-Host "Categories to test:" -ForegroundColor Cyan
    $categories | ForEach-Object { Write-Host "  - $_" -ForegroundColor Green }
    Write-Host ""
    
    # Run tests for each category group
    foreach ($cat in $categories) {
        Write-Host "=== Running tests for: $cat ===" -ForegroundColor Yellow
        & $MyInvocation.MyCommand.Path -CategoryGroup $cat -Platform $Platform -Configuration $Configuration
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Tests failed for category group: $cat"
            exit 1
        }
    }
    
    Write-Host ""
    Write-Host "✅ All category tests passed!" -ForegroundColor Green
    exit 0
}

# Determine test filter
$testFilter = ""
if ($Category) {
    $testFilter = "TestCategory=$Category"
    Write-Host "Running tests for category: $Category" -ForegroundColor Yellow
}
elseif ($CategoryGroup) {
    $categories = $CategoryGroup.Split(",") | ForEach-Object { $_.Trim() }
    $testFilter = ($categories | ForEach-Object { "TestCategory=$_" }) -join "|"
    Write-Host "Running tests for category group: $CategoryGroup" -ForegroundColor Yellow
}
else {
    Write-Error "Must specify -Category, -CategoryGroup, or -PrNumber"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  Run Button tests:          ./run-ui-tests-for-category.ps1 -Category Button"
    Write-Host "  Run multiple categories:   ./run-ui-tests-for-category.ps1 -CategoryGroup 'Button,Label'"
    Write-Host "  Analyze PR and run:        ./run-ui-tests-for-category.ps1 -PrNumber 12345"
    Write-Host "  List all categories:       ./run-ui-tests-for-category.ps1 -ListCategories"
    exit 1
}

Write-Host "Platform: $Platform" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "Test Filter: $testFilter" -ForegroundColor Cyan
Write-Host ""

# Determine paths based on platform
$testProjectPath = ""
$deviceArg = ""
$apiVersion = ""

switch ($Platform) {
    "android" {
        $testProjectPath = "$RepoRoot/src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj"
        $deviceArg = "android-emulator-64_30"
        $apiVersion = "30"
    }
    "ios" {
        $testProjectPath = "$RepoRoot/src/Controls/tests/TestCases.iOS.Tests/Controls.TestCases.iOS.Tests.csproj"
        $deviceArg = "ios-simulator-64"
        $apiVersion = "18.4"
    }
    "windows" {
        $testProjectPath = "$RepoRoot/src/Controls/tests/TestCases.WinUI.Tests/Controls.TestCases.WinUI.Tests.csproj"
        $deviceArg = "windows10"
        $apiVersion = "10.0.19041.0"
    }
    "catalyst" {
        $testProjectPath = "$RepoRoot/src/Controls/tests/TestCases.Mac.Tests/Controls.TestCases.Mac.Tests.csproj"
        $deviceArg = "mac"
        $apiVersion = "15.3"
    }
}

if (-not (Test-Path $testProjectPath)) {
    Write-Error "Test project not found: $testProjectPath"
    exit 1
}

# Check if HostApp is built
Write-Host "Checking if HostApp is built..." -ForegroundColor Yellow
$appPath = "$RepoRoot/src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj"

$needsBuild = $false
switch ($Platform) {
    "android" {
        $outputPath = "$RepoRoot/artifacts/bin/Controls.TestCases.HostApp/$Configuration/net10.0-android"
        if (-not (Test-Path $outputPath)) { $needsBuild = $true }
    }
    "ios" {
        $outputPath = "$RepoRoot/artifacts/bin/Controls.TestCases.HostApp/$Configuration/net10.0-ios"
        if (-not (Test-Path $outputPath)) { $needsBuild = $true }
    }
    "windows" {
        $outputPath = "$RepoRoot/artifacts/bin/Controls.TestCases.HostApp/$Configuration/net10.0-windows10.0.19041.0"
        if (-not (Test-Path $outputPath)) { $needsBuild = $true }
    }
    "catalyst" {
        $outputPath = "$RepoRoot/artifacts/bin/Controls.TestCases.HostApp/$Configuration/net10.0-maccatalyst"
        if (-not (Test-Path $outputPath)) { $needsBuild = $true }
    }
}

if ($needsBuild) {
    Write-Host "HostApp not built for $Platform. Building..." -ForegroundColor Yellow
    
    $targetFramework = switch ($Platform) {
        "android" { "net10.0-android" }
        "ios" { "net10.0-ios" }
        "windows" { "net10.0-windows10.0.19041.0" }
        "catalyst" { "net10.0-maccatalyst" }
    }
    
    Push-Location $RepoRoot
    try {
        & ./build.ps1 --target=dotnet --configuration=$Configuration
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to install .NET"
            exit 1
        }
        
        $dotnetPath = "./bin/dotnet/dotnet"
        if ($IsWindows) {
            $dotnetPath = ".\bin\dotnet\dotnet.exe"
        }
        
        Write-Host "Building HostApp for $targetFramework..." -ForegroundColor Yellow
        & $dotnetPath build $appPath -f $targetFramework -c $Configuration
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to build HostApp"
            exit 1
        }
    }
    finally {
        Pop-Location
    }
}

# Run the tests using the cake script
Write-Host ""
Write-Host "=== Running Tests ===" -ForegroundColor Cyan
Write-Host ""

Push-Location $RepoRoot
try {
    $cakeScript = switch ($Platform) {
        "android" { "eng/devices/android.cake" }
        "ios" { "eng/devices/ios.cake" }
        "windows" { "eng/devices/windows.cake" }
        "catalyst" { "eng/devices/catalyst.cake" }
    }
    
    $resultsDir = "$RepoRoot/test-results"
    $binlogDir = "$RepoRoot/artifacts/log"
    
    # Create directories if they don't exist
    New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null
    New-Item -ItemType Directory -Force -Path $binlogDir | Out-Null
    
    $cakeArgs = @(
        "--target=uitest",
        "--project=`"$testProjectPath`"",
        "--appproject=`"$appPath`"",
        "--device=`"$deviceArg`"",
        "--apiversion=`"$apiVersion`"",
        "--configuration=`"$Configuration`"",
        "--results=`"$resultsDir`"",
        "--binlog=`"$binlogDir`"",
        "--test-filter=`"$testFilter`"",
        "--verbosity=diagnostic"
    )
    
    Write-Host "Executing: ./build.ps1 -Script $cakeScript $($cakeArgs -join ' ')" -ForegroundColor Gray
    
    & ./build.ps1 -Script $cakeScript @cakeArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Tests failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "✅ Tests completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Test results: $resultsDir" -ForegroundColor Cyan
Write-Host "Build logs: $binlogDir" -ForegroundColor Cyan

exit 0
