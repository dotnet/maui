#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build and verify .NET MAUI projects to ensure everything compiles.

.DESCRIPTION
    This script builds and verifies MAUI projects before finalizing changes.
    
    Default behavior:
    - Builds the TestCases.HostApp on all available TFMs (target frameworks)
    - Builds each UI test project for available platforms
    - Automatically detects the operating system and builds appropriate targets:
      * Linux: Android only
      * macOS: iOS, Android, and MacCatalyst
      * Windows: Windows and Android
    
    Optionally runs unit tests based on parameters.

.PARAMETER RunUnitTests
    Run unit tests after building (default: false)

.PARAMETER UnitTestFilter
    Filter for which unit tests to run (e.g., "FullyQualifiedName~Core")
    If not specified, runs all unit tests

.PARAMETER Configuration
    Build configuration: "Debug" or "Release" (default: Debug)

.EXAMPLE
    ./BuildAndVerify.ps1
    Builds HostApp and UI test projects for current platform

.EXAMPLE
    ./BuildAndVerify.ps1 -RunUnitTests
    Builds and runs all unit tests

.EXAMPLE
    ./BuildAndVerify.ps1 -RunUnitTests -UnitTestFilter "FullyQualifiedName~Core"
    Builds and runs only Core unit tests

.EXAMPLE
    ./BuildAndVerify.ps1 -Configuration Release
    Builds with Release configuration
#>

[CmdletBinding()]
param(
    [switch]$RunUnitTests,

    [string]$UnitTestFilter,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

# Script configuration
$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path "$PSScriptRoot/../.."

# Import shared utilities
. "$PSScriptRoot/shared/shared-utils.ps1"

# Detect operating system
$IsLinuxOS = $IsLinux -or ($PSVersionTable.PSVersion.Major -ge 6 -and $PSVersionTable.Platform -eq "Unix")
$IsMacOSDetected = $IsMacOS -or ($PSVersionTable.PSVersion.Major -ge 6 -and $PSVersionTable.OS -match "Darwin")
$IsWindowsOS = $IsWindows -or ($PSVersionTable.PSVersion.Major -lt 6) -or ($PSVersionTable.Platform -eq "Win32NT")

# Determine target frameworks based on OS
if ($IsLinuxOS) {
    $TargetFrameworks = @("net10.0-android")
    $OSName = "Linux"
} elseif ($IsMacOSDetected) {
    $TargetFrameworks = @("net10.0-android", "net10.0-ios", "net10.0-maccatalyst")
    $OSName = "macOS"
} elseif ($IsWindowsOS) {
    $TargetFrameworks = @("net10.0-android", "net10.0-windows10.0.19041.0")
    $OSName = "Windows"
} else {
    Write-Error "Unable to detect operating system"
    exit 1
}

# Banner
Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║     .NET MAUI Build and Verify Script                    ║
║     OS: $OSName
║     Configuration: $Configuration
║     Target Frameworks: $($TargetFrameworks -join ", ")
║     Run Unit Tests: $RunUnitTests
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Magenta

#region Validation

Write-Step "Validating prerequisites..."

# Check if dotnet is available
if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Error ".NET SDK not found. Please install .NET SDK and ensure 'dotnet' is in PATH."
    exit 1
}

$dotnetVersion = dotnet --version
Write-Info ".NET SDK version: $dotnetVersion"

Write-Success "Prerequisites validated"

#endregion

#region Define Projects

# HostApp project
$HostAppProject = "src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj"

# UI Test projects based on platform
$UITestProjects = @()
if ($IsLinuxOS) {
    $UITestProjects += "src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj"
} elseif ($IsMacOSDetected) {
    $UITestProjects += "src/Controls/tests/TestCases.iOS.Tests/Controls.TestCases.iOS.Tests.csproj"
    $UITestProjects += "src/Controls/tests/TestCases.Mac.Tests/Controls.TestCases.Mac.Tests.csproj"
    $UITestProjects += "src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj"
} elseif ($IsWindowsOS) {
    $UITestProjects += "src/Controls/tests/TestCases.WinUI.Tests/Controls.TestCases.WinUI.Tests.csproj"
    $UITestProjects += "src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj"
}

# Unit test projects (to run if requested)
$UnitTestProjects = @(
    "src/Core/tests/UnitTests/Core.UnitTests.csproj",
    "src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj",
    "src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj",
    "src/Essentials/test/UnitTests/Essentials.UnitTests.csproj"
)

#endregion

#region Build Projects

$BuildSuccesses = 0
$BuildFailures = 0
$FailedBuilds = @()

# Build HostApp for all available TFMs
Write-Step "Building TestCases.HostApp..."
$HostAppPath = Join-Path $RepoRoot $HostAppProject

if (-not (Test-Path $HostAppPath)) {
    Write-Error "HostApp project not found: $HostAppPath"
    $BuildFailures++
    $FailedBuilds += @{ Project = "HostApp"; Framework = "N/A"; Error = "Project file not found" }
} else {
    foreach ($Framework in $TargetFrameworks) {
        Write-Info "Building HostApp for framework: $Framework"
        
        try {
            $buildOutput = dotnet build $HostAppPath `
                -f $Framework `
                -c $Configuration `
                --nologo `
                2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Success "✓ HostApp ($Framework) built successfully"
                $BuildSuccesses++
            } else {
                Write-Error "✗ HostApp ($Framework) build failed"
                $BuildFailures++
                $FailedBuilds += @{ 
                    Project = "HostApp"
                    Framework = $Framework
                    Error = "Build failed with exit code $LASTEXITCODE"
                }
                
                Write-Host ""
                Write-Host "Last 20 lines of build output:" -ForegroundColor Yellow
                $buildOutput | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
                Write-Host ""
            }
        } catch {
            Write-Error "✗ HostApp ($Framework) build failed with exception"
            $BuildFailures++
            $FailedBuilds += @{ 
                Project = "HostApp"
                Framework = $Framework
                Error = $_.Exception.Message
            }
        }
    }
}

# Build UI Test projects
Write-Step "Building UI Test projects..."

foreach ($UITestProject in $UITestProjects) {
    $ProjectPath = Join-Path $RepoRoot $UITestProject
    $ProjectName = [System.IO.Path]::GetFileNameWithoutExtension($UITestProject)
    
    if (-not (Test-Path $ProjectPath)) {
        Write-Error "UI Test project not found: $ProjectPath"
        $BuildFailures++
        $FailedBuilds += @{ Project = $ProjectName; Framework = "N/A"; Error = "Project file not found" }
        continue
    }
    
    Write-Info "Building $ProjectName"
    
    try {
        $buildOutput = dotnet build $ProjectPath `
            -c $Configuration `
            --nologo `
            2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "✓ $ProjectName built successfully"
            $BuildSuccesses++
        } else {
            Write-Error "✗ $ProjectName build failed"
            $BuildFailures++
            $FailedBuilds += @{ 
                Project = $ProjectName
                Framework = "All"
                Error = "Build failed with exit code $LASTEXITCODE"
            }
            
            Write-Host ""
            Write-Host "Last 20 lines of build output:" -ForegroundColor Yellow
            $buildOutput | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
            Write-Host ""
        }
    } catch {
        Write-Error "✗ $ProjectName build failed with exception"
        $BuildFailures++
        $FailedBuilds += @{ 
            Project = $ProjectName
            Framework = "All"
            Error = $_.Exception.Message
        }
    }
}

#endregion

#region Run Unit Tests (Optional)

$TestSuccesses = 0
$TestFailures = 0
$FailedTests = @()

if ($RunUnitTests) {
    Write-Step "Running unit tests..."
    
    foreach ($UnitTestProject in $UnitTestProjects) {
        $ProjectPath = Join-Path $RepoRoot $UnitTestProject
        $ProjectName = [System.IO.Path]::GetFileNameWithoutExtension($UnitTestProject)
        
        if (-not (Test-Path $ProjectPath)) {
            Write-Info "Skipping $ProjectName (not found)"
            continue
        }
        
        Write-Info "Running tests in $ProjectName"
        
        try {
            $testArgs = @(
                "test"
                $ProjectPath
                "-c", $Configuration
                "--nologo"
            )
            
            if ($UnitTestFilter) {
                $testArgs += "--filter"
                $testArgs += $UnitTestFilter
            }
            
            $testOutput = & dotnet @testArgs 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Success "✓ $ProjectName tests passed"
                $TestSuccesses++
            } else {
                Write-Error "✗ $ProjectName tests failed"
                $TestFailures++
                $FailedTests += @{ 
                    Project = $ProjectName
                    Error = "Tests failed with exit code $LASTEXITCODE"
                }
                
                Write-Host ""
                Write-Host "Last 30 lines of test output:" -ForegroundColor Yellow
                $testOutput | Select-Object -Last 30 | ForEach-Object { Write-Host $_ }
                Write-Host ""
            }
        } catch {
            Write-Error "✗ $ProjectName tests failed with exception"
            $TestFailures++
            $FailedTests += @{ 
                Project = $ProjectName
                Error = $_.Exception.Message
            }
        }
    }
}

#endregion

#region Summary

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║                    Summary                                ║" -ForegroundColor Magenta
Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Magenta
Write-Host "║  Operating System:  $OSName" -ForegroundColor Magenta
Write-Host "║  Configuration:     $Configuration" -ForegroundColor Magenta
Write-Host "║  Successful Builds: $BuildSuccesses" -ForegroundColor Green
Write-Host "║  Failed Builds:     $BuildFailures" -ForegroundColor $(if ($BuildFailures -gt 0) { "Red" } else { "Green" })

if ($RunUnitTests) {
    Write-Host "║  Successful Tests:  $TestSuccesses" -ForegroundColor Green
    Write-Host "║  Failed Tests:      $TestFailures" -ForegroundColor $(if ($TestFailures -gt 0) { "Red" } else { "Green" })
}

Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
Write-Host ""

$HasFailures = $false

if ($BuildFailures -gt 0) {
    Write-Host "Failed builds:" -ForegroundColor Red
    foreach ($failed in $FailedBuilds) {
        Write-Host "  • $($failed.Project) ($($failed.Framework)): $($failed.Error)" -ForegroundColor Red
    }
    Write-Host ""
    $HasFailures = $true
}

if ($RunUnitTests -and $TestFailures -gt 0) {
    Write-Host "Failed tests:" -ForegroundColor Red
    foreach ($failed in $FailedTests) {
        Write-Host "  • $($failed.Project): $($failed.Error)" -ForegroundColor Red
    }
    Write-Host ""
    $HasFailures = $true
}

if ($HasFailures) {
    exit 1
} else {
    Write-Success "All builds and tests completed successfully! ✨"
    exit 0
}

#endregion
