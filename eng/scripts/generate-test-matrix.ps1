#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Generates Azure DevOps test matrix from analysis results

.DESCRIPTION
    This script reads the test category analysis output and generates
    a dynamic test matrix for Azure DevOps pipelines.

.PARAMETER AnalysisFile
    Path to the test category analysis file (default: test-categories.txt)

.PARAMETER OutputFormat
    Output format: json, yaml, or azdo (default: azdo)

.EXAMPLE
    ./generate-test-matrix.ps1 -AnalysisFile test-categories.txt -OutputFormat json
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$AnalysisFile = "test-categories.txt",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("json", "yaml", "azdo")]
    [string]$OutputFormat = "azdo"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Test Matrix Generator ===" -ForegroundColor Cyan

# Check if analysis file exists
if (-not (Test-Path $AnalysisFile)) {
    Write-Error "Analysis file not found: $AnalysisFile"
    exit 1
}

# Read category groups
$categoryGroups = Get-Content $AnalysisFile | Where-Object { $_.Trim() -ne "" }

if ($categoryGroups.Count -eq 0) {
    Write-Host "No test categories to run" -ForegroundColor Yellow
    
    if ($OutputFormat -eq "azdo") {
        Write-Host "##vso[task.setVariable variable=TestMatrixJson;isOutput=true]{}"
        Write-Host "##vso[task.setVariable variable=HasTestsToRun;isOutput=true]false"
    }
    
    exit 0
}

Write-Host "Generating test matrix for $($categoryGroups.Count) category groups" -ForegroundColor Green

# Build matrix
$matrix = @{}
foreach ($group in $categoryGroups) {
    $key = $group
    $matrix[$key] = @{
        CATEGORYGROUP = $group
    }
}

# Output based on format
switch ($OutputFormat) {
    "json" {
        $matrix | ConvertTo-Json -Depth 10
    }
    
    "yaml" {
        Write-Host "strategy:" -ForegroundColor White
        Write-Host "  matrix:" -ForegroundColor White
        foreach ($key in $matrix.Keys) {
            Write-Host "    $($key):" -ForegroundColor White
            Write-Host "      CATEGORYGROUP: $($matrix[$key].CATEGORYGROUP)" -ForegroundColor White
        }
    }
    
    "azdo" {
        # Output for Azure DevOps
        $matrixJson = $matrix | ConvertTo-Json -Compress -Depth 10
        Write-Host "##vso[task.setVariable variable=TestMatrixJson;isOutput=true]$matrixJson"
        Write-Host "##vso[task.setVariable variable=HasTestsToRun;isOutput=true]true"
        Write-Host "##vso[task.setVariable variable=TestCategoryCount;isOutput=true]$($categoryGroups.Count)"
        
        Write-Host "Test Matrix:" -ForegroundColor Cyan
        foreach ($key in $matrix.Keys) {
            Write-Host "  - $key" -ForegroundColor Green
        }
    }
}

Write-Host "`n=== Matrix Generation Complete ===" -ForegroundColor Cyan
exit 0
