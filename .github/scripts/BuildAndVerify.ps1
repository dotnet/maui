#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build and verify .NET MAUI projects to ensure everything compiles.

.DESCRIPTION
    This script builds and verifies MAUI projects before finalizing changes.
    It automatically detects the operating system and builds appropriate targets:
    - Linux: Builds Android targets only (net10.0-android)
    - macOS: Builds both iOS and Android targets
    - Windows: Builds all available targets
    
    The purpose is to catch compilation errors early before committing changes.

.PARAMETER Configuration
    Build configuration: "Debug" or "Release" (default: Debug)

.PARAMETER Projects
    Specific projects to build. If not specified, builds key MAUI projects.
    Valid values: "Core", "Controls", "Essentials", "HostApp", "All"

.EXAMPLE
    ./BuildAndVerify.ps1
    Builds key MAUI projects with Debug configuration for current platform

.EXAMPLE
    ./BuildAndVerify.ps1 -Configuration Release
    Builds with Release configuration

.EXAMPLE
    ./BuildAndVerify.ps1 -Projects "HostApp"
    Builds only the TestCases.HostApp project
#>

[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [ValidateSet("Core", "Controls", "Essentials", "HostApp", "All")]
    [string[]]$Projects = @("Core", "Controls", "HostApp")
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

$ProjectMap = @{
    "Core" = "src/Core/src/Core.csproj"
    "Controls" = "src/Controls/src/Core/Controls.Core.csproj"
    "Essentials" = "src/Essentials/src/Essentials.csproj"
    "HostApp" = "src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj"
}

# Determine which projects to build
if ($Projects -contains "All") {
    $ProjectsToBuild = $ProjectMap.Keys
} else {
    $ProjectsToBuild = $Projects
}

#endregion

#region Build Projects

$BuildSuccesses = 0
$BuildFailures = 0
$FailedBuilds = @()

foreach ($ProjectKey in $ProjectsToBuild) {
    if (-not $ProjectMap.ContainsKey($ProjectKey)) {
        Write-Error "Unknown project key: $ProjectKey"
        continue
    }

    $ProjectPath = Join-Path $RepoRoot $ProjectMap[$ProjectKey]
    
    if (-not (Test-Path $ProjectPath)) {
        Write-Error "Project not found: $ProjectPath"
        $BuildFailures++
        $FailedBuilds += @{ Project = $ProjectKey; Framework = "N/A"; Error = "Project file not found" }
        continue
    }

    Write-Step "Building project: $ProjectKey"
    Write-Info "Project path: $ProjectPath"

    # For HostApp, build for specific frameworks; for others, build all available
    if ($ProjectKey -eq "HostApp") {
        foreach ($Framework in $TargetFrameworks) {
            Write-Info "Building for framework: $Framework"
            
            try {
                # Build the project for the specific framework
                $buildOutput = dotnet build $ProjectPath `
                    -f $Framework `
                    -c $Configuration `
                    --nologo `
                    2>&1
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Success "✓ $ProjectKey ($Framework) built successfully"
                    $BuildSuccesses++
                } else {
                    Write-Error "✗ $ProjectKey ($Framework) build failed"
                    $BuildFailures++
                    $FailedBuilds += @{ 
                        Project = $ProjectKey
                        Framework = $Framework
                        Error = "Build failed with exit code $LASTEXITCODE"
                    }
                    
                    # Show last 20 lines of build output for context
                    Write-Host ""
                    Write-Host "Last 20 lines of build output:" -ForegroundColor Yellow
                    $buildOutput | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
                    Write-Host ""
                }
            } catch {
                Write-Error "✗ $ProjectKey ($Framework) build failed with exception"
                $BuildFailures++
                $FailedBuilds += @{ 
                    Project = $ProjectKey
                    Framework = $Framework
                    Error = $_.Exception.Message
                }
            }
        }
    } else {
        # Build without specifying framework - use all available
        Write-Info "Building all available frameworks"
        
        try {
            $buildOutput = dotnet build $ProjectPath `
                -c $Configuration `
                --nologo `
                2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Success "✓ $ProjectKey built successfully"
                $BuildSuccesses++
            } else {
                Write-Error "✗ $ProjectKey build failed"
                $BuildFailures++
                $FailedBuilds += @{ 
                    Project = $ProjectKey
                    Framework = "All"
                    Error = "Build failed with exit code $LASTEXITCODE"
                }
                
                # Show last 20 lines of build output for context
                Write-Host ""
                Write-Host "Last 20 lines of build output:" -ForegroundColor Yellow
                $buildOutput | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
                Write-Host ""
            }
        } catch {
            Write-Error "✗ $ProjectKey build failed with exception"
            $BuildFailures++
            $FailedBuilds += @{ 
                Project = $ProjectKey
                Framework = "All"
                Error = $_.Exception.Message
            }
        }
    }
}

#endregion

#region Summary

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║                    Build Summary                          ║" -ForegroundColor Magenta
Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Magenta
Write-Host "║  Operating System:  $OSName" -ForegroundColor Magenta
Write-Host "║  Configuration:     $Configuration" -ForegroundColor Magenta
Write-Host "║  Successful Builds: $BuildSuccesses" -ForegroundColor Green
Write-Host "║  Failed Builds:     $BuildFailures" -ForegroundColor $(if ($BuildFailures -gt 0) { "Red" } else { "Green" })
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
Write-Host ""

if ($BuildFailures -gt 0) {
    Write-Host "Failed builds:" -ForegroundColor Red
    foreach ($failed in $FailedBuilds) {
        Write-Host "  • $($failed.Project) ($($failed.Framework)): $($failed.Error)" -ForegroundColor Red
    }
    Write-Host ""
    exit 1
} else {
    Write-Success "All builds completed successfully! ✨"
    exit 0
}

#endregion
