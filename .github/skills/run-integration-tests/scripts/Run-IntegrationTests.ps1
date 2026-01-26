<#
.SYNOPSIS
    Build, pack, and run .NET MAUI integration tests locally.

.DESCRIPTION
    This script automates the full workflow for running integration tests:
    1. Build and pack the MAUI product
    2. Install the generated MAUI workloads locally
    3. Extract and set MAUI_PACKAGE_VERSION
    4. Run integration tests with specified filter

.PARAMETER Category
    Test category to run (WindowsTemplates, Samples, Build, Blazor, MultiProject, etc.)

.PARAMETER TestFilter
    Custom NUnit test filter expression. Overrides -Category if specified.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Debug

.PARAMETER SkipBuild
    Skip the build and pack step (useful if already built)

.PARAMETER ResultsDirectory
    Directory for test results. Default: artifacts/integration-tests

.EXAMPLE
    .\Run-IntegrationTests.ps1 -Category "WindowsTemplates"
    
.EXAMPLE
    .\Run-IntegrationTests.ps1 -Category "Samples" -Configuration "Release"

.EXAMPLE
    .\Run-IntegrationTests.ps1 -TestFilter "FullyQualifiedName~BuildSample" -SkipBuild
#>

param(
    [Parameter()]
    [ValidateSet("Build", "WindowsTemplates", "macOSTemplates", "Blazor", "MultiProject", "Samples", "AOT", "RunOnAndroid", "RunOniOS")]
    [string]$Category,

    [Parameter()]
    [string]$TestFilter,

    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [Parameter()]
    [switch]$SkipBuild,

    [Parameter()]
    [switch]$SkipInstall,

    [Parameter()]
    [switch]$SkipXcodeVersionCheck,

    [Parameter()]
    [string]$ResultsDirectory = "artifacts/integration-tests"
)

$ErrorActionPreference = "Stop"
$RepoRoot = (Get-Item $PSScriptRoot).Parent.Parent.Parent.Parent.FullName
$RunningOnWindows = $IsWindows -or $env:OS -eq 'Windows_NT'
$RunningOnMacOS = $IsMacOS -or ((Get-Command uname -ErrorAction SilentlyContinue) -and ((uname) -eq 'Darwin'))

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       .NET MAUI Integration Tests Runner                  ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "Repository Root: $RepoRoot"
Write-Host "Platform:        $(if ($RunningOnWindows) { 'Windows' } elseif ($RunningOnMacOS) { 'macOS' } else { 'Linux' })"
Write-Host "Configuration:   $Configuration"
Write-Host "Category:        $(if ($Category) { $Category } else { 'Custom filter' })"
Write-Host "Skip Build:      $SkipBuild"
Write-Host ""

Push-Location $RepoRoot

try {
    # ═══════════════════════════════════════════════════════════════════
    # Pre-flight: Check for processes using .dotnet folder (Windows only)
    # ═══════════════════════════════════════════════════════════════════
    $dotnetFolder = Join-Path $RepoRoot ".dotnet"
    
    if ($RunningOnWindows) {
        $lockingProcesses = Get-Process | Where-Object { $_.Path -like "$dotnetFolder\*" } -ErrorAction SilentlyContinue
        
        if ($lockingProcesses) {
            Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Red
            Write-Host "⚠️  WARNING: Processes are using the .dotnet folder!" -ForegroundColor Red
            Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Red
            Write-Host ""
            Write-Host "The following processes have locks on files in .dotnet:" -ForegroundColor Yellow
            foreach ($proc in $lockingProcesses) {
                Write-Host "  - PID: $($proc.Id) | Name: $($proc.ProcessName) | Path: $($proc.Path)" -ForegroundColor Gray
            }
            Write-Host ""
            Write-Host "To fix this, run:" -ForegroundColor Cyan
            Write-Host '  Get-Process | Where-Object { $_.Path -like "*\.dotnet\*" } | ForEach-Object { Stop-Process -Id $_.Id -Force }' -ForegroundColor White
            Write-Host ""
            throw "Cannot proceed with locked .dotnet folder. Kill the processes above and retry."
        }
        
        Write-Host "✅ No processes locking .dotnet folder" -ForegroundColor Green
    }
    else {
        Write-Host "✅ Skipping process lock check (not Windows)" -ForegroundColor Green
    }
    Write-Host ""

    # ═══════════════════════════════════════════════════════════════════
    # Step 1: Build and Pack
    # ═══════════════════════════════════════════════════════════════════
    if (-not $SkipBuild) {
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
        Write-Host "Step 1: Building and Packing MAUI..." -ForegroundColor Yellow
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
        
        if ($RunningOnWindows) {
            $buildCmd = Join-Path $RepoRoot 'build.cmd'
            $buildArgs = @("-restore", "-pack", "-configuration", $Configuration)
            Write-Host "Running: $buildCmd $($buildArgs -join ' ')" -ForegroundColor Gray
            & $buildCmd @buildArgs
        }
        else {
            $buildCmd = Join-Path $RepoRoot 'build.sh'
            $buildArgs = @("-restore", "-pack", "-configuration", $Configuration)
            Write-Host "Running: $buildCmd $($buildArgs -join ' ')" -ForegroundColor Gray
            & bash $buildCmd @buildArgs
        }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Build and pack failed with exit code $LASTEXITCODE"
        }
        
        Write-Host "✅ Build and pack completed successfully" -ForegroundColor Green
        Write-Host ""
    }
    else {
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
        Write-Host "Step 1: Skipping build (SkipBuild specified)" -ForegroundColor Yellow
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
        Write-Host ""
    }

    # ═══════════════════════════════════════════════════════════════════
    # Step 2: Install Workloads
    # ═══════════════════════════════════════════════════════════════════
    $dotnetPath = if ($RunningOnWindows) { 
        Join-Path $RepoRoot ".dotnet\dotnet.exe" 
    } else { 
        Join-Path $RepoRoot ".dotnet/dotnet" 
    }
    
    if (-not (Test-Path $dotnetPath)) {
        throw "Local .dotnet SDK not found at: $dotnetPath. Run build first or provision with: ./build.sh --target=dotnet && dotnet cake --target=dotnet-local-workloads"
    }

    if (-not $SkipInstall) {
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
        Write-Host "Step 2: Installing MAUI Workloads..." -ForegroundColor Yellow
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
        
        $installArgs = @(
            "build",
            ".\src\DotNet\DotNet.csproj",
            "-t:Install",
            "-c", $Configuration
        )
        
        Write-Host "Running: $dotnetPath $($installArgs -join ' ')" -ForegroundColor Gray
        
        & $dotnetPath @installArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "Workload installation failed with exit code $LASTEXITCODE"
        }
        
        Write-Host "✅ Workload installation completed successfully" -ForegroundColor Green
        Write-Host ""
    }
    else {
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
        Write-Host "Step 2: Skipping workload install (SkipInstall specified)" -ForegroundColor Yellow
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
        Write-Host ""
    }

    # ═══════════════════════════════════════════════════════════════════
    # Step 3: Extract MAUI Package Version
    # ═══════════════════════════════════════════════════════════════════
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "Step 3: Extracting MAUI Package Version..." -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
    
    $packsPath = Join-Path $RepoRoot ".dotnet/packs/Microsoft.Maui.Sdk"
    
    if (-not (Test-Path $packsPath)) {
        throw "Microsoft.Maui.Sdk packs not found at: $packsPath"
    }
    
    # Get SDK version from global.json to match the correct MAUI version
    $globalJsonPath = Join-Path $RepoRoot "global.json"
    $globalJson = Get-Content $globalJsonPath | ConvertFrom-Json
    $sdkVersion = $globalJson.tools.dotnet
    $sdkMajorVersion = $sdkVersion.Split('.')[0]
    
    Write-Host "SDK Version: $sdkVersion (major: $sdkMajorVersion)" -ForegroundColor Gray
    
    # Find MAUI SDK version matching the current SDK major version
    $versionFolder = Get-ChildItem -Path $packsPath -Directory | 
        Where-Object { $_.Name -match "^$sdkMajorVersion\." } |
        Sort-Object { [Version]($_.Name -replace '-.*$', '') } -Descending |
        Select-Object -First 1
    
    if (-not $versionFolder) {
        # Fallback to any version if no match found
        Write-Host "No version matching SDK $sdkMajorVersion.x found, falling back to latest..." -ForegroundColor Yellow
        $versionFolder = Get-ChildItem -Path $packsPath -Directory | Sort-Object Name -Descending | Select-Object -First 1
    }
    
    if (-not $versionFolder) {
        throw "No version folders found in: $packsPath"
    }
    
    $mauiPackageVersion = $versionFolder.Name
    $env:MAUI_PACKAGE_VERSION = $mauiPackageVersion
    
    # Set Xcode version skip if requested
    if ($SkipXcodeVersionCheck) {
        $env:SKIP_XCODE_VERSION_CHECK = "true"
        Write-Host "SKIP_XCODE_VERSION_CHECK: true" -ForegroundColor Yellow
    }
    
    Write-Host "MAUI_PACKAGE_VERSION: $mauiPackageVersion" -ForegroundColor Green
    Write-Host "✅ Environment variable set" -ForegroundColor Green
    Write-Host ""

    # ═══════════════════════════════════════════════════════════════════
    # Step 4: Run Integration Tests
    # ═══════════════════════════════════════════════════════════════════
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "Step 4: Running Integration Tests..." -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
    
    # Create results directory
    $resultsPath = Join-Path $RepoRoot $ResultsDirectory
    if (-not (Test-Path $resultsPath)) {
        New-Item -ItemType Directory -Path $resultsPath -Force | Out-Null
    }
    
    # Build test filter
    $effectiveFilter = if ($TestFilter) {
        $TestFilter
    }
    elseif ($Category) {
        "Category=$Category"
    }
    else {
        throw "Either -Category or -TestFilter must be specified"
    }
    
    $testProjectPath = Join-Path $RepoRoot "src/TestUtils/src/Microsoft.Maui.IntegrationTests/Microsoft.Maui.IntegrationTests.csproj"
    
    $testArgs = @(
        "test",
        $testProjectPath,
        "--configuration", $Configuration,
        "--filter", $effectiveFilter,
        "--logger", "trx",
        "--results-directory", $resultsPath
    )
    
    Write-Host "Test Filter: $effectiveFilter" -ForegroundColor Cyan
    Write-Host "Results Directory: $resultsPath" -ForegroundColor Cyan
    Write-Host "Running: $dotnetPath $($testArgs -join ' ')" -ForegroundColor Gray
    Write-Host ""
    
    & $dotnetPath @testArgs
    
    $testExitCode = $LASTEXITCODE
    
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "                    Test Summary                           " -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "Configuration:        $Configuration"
    Write-Host "Category/Filter:      $effectiveFilter"
    Write-Host "MAUI_PACKAGE_VERSION: $mauiPackageVersion"
    Write-Host "Results Directory:    $resultsPath"
    
    if ($testExitCode -eq 0) {
        Write-Host "Result:               ✅ PASSED" -ForegroundColor Green
    }
    else {
        Write-Host "Result:               ❌ FAILED (Exit Code: $testExitCode)" -ForegroundColor Red
    }
    
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    
    # List TRX files generated
    $trxFiles = Get-ChildItem -Path $resultsPath -Filter "*.trx" -ErrorAction SilentlyContinue
    if ($trxFiles) {
        Write-Host ""
        Write-Host "Generated TRX Files:" -ForegroundColor Yellow
        foreach ($trx in $trxFiles) {
            Write-Host "  - $($trx.FullName)" -ForegroundColor Gray
        }
    }
    
    exit $testExitCode
}
catch {
    Write-Host ""
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}
