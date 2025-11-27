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

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}
