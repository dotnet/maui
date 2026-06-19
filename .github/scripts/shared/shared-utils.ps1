#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Shared utility functions for MAUI test scripts

.DESCRIPTION
    Common functions used across BuildAndRunHostApp.ps1 and BuildAndRunSandbox.ps1
#>

# Color output functions
function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "🔹 $Message" -ForegroundColor Cyan
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Gray
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Warn {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Get-MauiTfmVersion {
    <#
    .SYNOPSIS
        Returns the repo's MAUI .NET TFM version (e.g. "10.0" or "11.0").
    .DESCRIPTION
        Reads <_MauiDotNetVersionMajor>/<_MauiDotNetVersionMinor> from Directory.Build.props so
        app/test TargetFrameworks follow the checked-out branch instead of being hardcoded
        (e.g. a net11.0 PR builds net11.0-android, not net10.0-android). Searches the supplied
        RepoRoot, then $env:BUILD_SOURCESDIRECTORY (the AzDO working tree), then the current
        directory — so it still resolves when the caller runs from a trusted-copy location
        outside the working tree. Falls back to "10.0".
    #>
    param([string]$RepoRoot)

    $candidates = @()
    if ($RepoRoot) { $candidates += $RepoRoot }
    if ($env:BUILD_SOURCESDIRECTORY) { $candidates += $env:BUILD_SOURCESDIRECTORY }
    $candidates += (Get-Location).Path

    foreach ($root in $candidates) {
        if (-not $root) { continue }
        $propsPath = Join-Path $root 'Directory.Build.props'
        if (Test-Path $propsPath) {
            $content = Get-Content $propsPath -Raw
            if ($content -match '<_MauiDotNetVersionMajor[^>]*>\s*(\d+)\s*<') {
                $major = $Matches[1]
                $minor = if ($content -match '<_MauiDotNetVersionMinor[^>]*>\s*(\d+)\s*<') { $Matches[1] } else { '0' }
                return "$major.$minor"
            }
        }
    }

    # Secondary source: global.json's SDK band, so a Directory.Build.props parse-miss on a
    # net11+ branch doesn't silently build net10. Only major.minor is used.
    # NOTE: global.json's `tools.dotnet` is the build-SDK *band* — a proxy for the MAUI TFM,
    # which move in lockstep in this repo. The regex keys on that `tools.dotnet` convention
    # (not `sdk.version`) and the trailing `.` after the minor; revisit if the pin format changes.
    foreach ($root in $candidates) {
        if (-not $root) { continue }
        $gjPath = Join-Path $root 'global.json'
        if (Test-Path $gjPath) {
            $gj = Get-Content $gjPath -Raw
            if ($gj -match '"dotnet"\s*:\s*"(\d+)\.(\d+)\.') {
                Write-Warn "Get-MauiTfmVersion: Directory.Build.props parse failed; using global.json SDK band ($($Matches[1]).$($Matches[2]))"
                return "$($Matches[1]).$($Matches[2])"
            }
        }
    }

    Write-Warn "Could not find <_MauiDotNetVersionMajor> in Directory.Build.props or global.json — falling back to '10.0'"
    return '10.0'
}
