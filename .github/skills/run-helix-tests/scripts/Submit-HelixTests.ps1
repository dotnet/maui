<#
.SYNOPSIS
    Submit .NET MAUI unit tests to Helix infrastructure.

.DESCRIPTION
    This script submits unit tests to Helix queues for distributed testing.
    It sets up required environment variables and invokes the helix.proj MSBuild project.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release

.PARAMETER Queue
    Specific Helix queue to target. If not specified, uses default queues from helix.proj.

.PARAMETER DryRun
    If specified, shows what would be submitted without actually submitting.

.EXAMPLE
    ./Submit-HelixTests.ps1
    
.EXAMPLE
    ./Submit-HelixTests.ps1 -Configuration Debug
    
.EXAMPLE
    ./Submit-HelixTests.ps1 -Queue "Windows.10.Amd64.Open"
#>

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [string]$Queue,
    
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

# Find repository root
$repoRoot = git rev-parse --show-toplevel 2>$null
if (-not $repoRoot) {
    $repoRoot = (Get-Item $PSScriptRoot).Parent.Parent.Parent.Parent.FullName
}
$repoRoot = $repoRoot -replace '/', '\'

Write-Host "Repository root: $repoRoot" -ForegroundColor Cyan

# Check for local SDK
$dotnetPath = Join-Path $repoRoot ".dotnet"
$dotnetExe = if ($IsWindows -or $env:OS -match "Windows") {
    Join-Path $dotnetPath "dotnet.exe"
} else {
    Join-Path $dotnetPath "dotnet"
}

if (-not (Test-Path $dotnetExe)) {
    Write-Host "ERROR: Local SDK not found at $dotnetPath" -ForegroundColor Red
    Write-Host "Run 'dotnet cake --target=dotnet' to provision the local SDK first." -ForegroundColor Yellow
    exit 1
}

Write-Host "Using SDK: $dotnetExe" -ForegroundColor Cyan

# Set required environment variables
$env:BUILD_SOURCEBRANCH = "main"
$env:BUILD_REPOSITORY_NAME = "maui"
$env:SYSTEM_TEAMPROJECT = "public"
$env:BUILD_REASON = "test"
$env:DOTNET_ROOT = $dotnetPath

Write-Host ""
Write-Host "Environment Variables:" -ForegroundColor Cyan
Write-Host "  BUILD_SOURCEBRANCH: $env:BUILD_SOURCEBRANCH"
Write-Host "  BUILD_REPOSITORY_NAME: $env:BUILD_REPOSITORY_NAME"
Write-Host "  SYSTEM_TEAMPROJECT: $env:SYSTEM_TEAMPROJECT"
Write-Host "  BUILD_REASON: $env:BUILD_REASON"
Write-Host ""

# Build command
$helixProj = Join-Path $repoRoot "eng\helix.proj"
$args = @(
    "build"
    $helixProj
    "/restore"
    "/t:Test"
    "/p:Configuration=$Configuration"
)

if ($Queue) {
    $args += "/p:HelixTargetQueues=$Queue"
    Write-Host "Target Queue: $Queue" -ForegroundColor Cyan
}

# Add binlog for diagnostics
$binlogPath = Join-Path $repoRoot "artifacts\log\helix-submit.binlog"
$args += "/bl:$binlogPath"

if ($DryRun) {
    Write-Host ""
    Write-Host "DRY RUN - Would execute:" -ForegroundColor Yellow
    Write-Host "$dotnetExe $($args -join ' ')" -ForegroundColor Gray
    exit 0
}

Write-Host ""
Write-Host "Submitting tests to Helix..." -ForegroundColor Green
Write-Host "Command: $dotnetExe $($args -join ' ')" -ForegroundColor Gray
Write-Host ""

# Execute
Push-Location $repoRoot
try {
    & $dotnetExe @args
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -eq 0) {
        Write-Host ""
        Write-Host "✅ Tests submitted successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Monitor results at: https://helix.dot.net" -ForegroundColor Cyan
        Write-Host "Binlog saved to: $binlogPath" -ForegroundColor Gray
    } else {
        Write-Host ""
        Write-Host "❌ Submission failed with exit code: $exitCode" -ForegroundColor Red
        Write-Host "Check binlog for details: $binlogPath" -ForegroundColor Yellow
    }
    
    exit $exitCode
}
finally {
    Pop-Location
}
