#!/usr/bin/env pwsh

<#
.SYNOPSIS
Sorts all PublicAPI.Unshipped.txt files in the MAUI repository.

.DESCRIPTION
This script finds all PublicAPI.Unshipped.txt files in the repository and sorts their content.
It preserves special metadata prefixes (*REMOVED* and ~) while sorting based on the actual API signatures.

.PARAMETER DryRun
If specified, shows what would be changed without modifying files.

.EXAMPLE
.\sort-public-api-unshipped.ps1
Sorts all PublicAPI.Unshipped.txt files in place.

.EXAMPLE
.\sort-public-api-unshipped.ps1 -DryRun
Shows what would be changed without modifying files.
#>

param(
    [switch]$DryRun = $false
)

# Function to extract the sort key from a line by removing metadata prefixes
function Get-SortKey {
    param([string]$Line)
    
    # Skip empty lines and comments
    if ([string]::IsNullOrWhiteSpace($Line) -or $Line.StartsWith('#')) {
        return $Line
    }
    
    $sortKey = $Line
    
    # Remove *REMOVED* prefix if present
    if ($sortKey.StartsWith('*REMOVED*')) {
        $sortKey = $sortKey.Substring('*REMOVED*'.Length)
    }
    
    # Remove ~ prefix if present
    if ($sortKey.StartsWith('~')) {
        $sortKey = $sortKey.Substring(1)
    }
    
    return $sortKey
}

# Function to sort lines while preserving metadata
function Invoke-PublicApiSort {
    param([string[]]$Lines)
    
    if ($Lines.Count -eq 0) {
        return $Lines
    }
    
    # Separate header comments from API lines
    $headerLines = @()
    $apiLines = @()
    $inHeader = $true
    
    foreach ($line in $Lines) {
        if ($inHeader -and ($line.StartsWith('#') -or [string]::IsNullOrWhiteSpace($line))) {
            $headerLines += $line
        } else {
            $inHeader = $false
            if (-not [string]::IsNullOrWhiteSpace($line)) {
                $apiLines += $line
            }
        }
    }
    
    # Sort API lines using the sort key
    $sortedApiLines = $apiLines | Sort-Object -Property { Get-SortKey $_ }
    
    # Combine header and sorted API lines
    $result = @()
    $result += $headerLines
    $result += $sortedApiLines
    
    return $result
}

# Find the repository root (assuming script is in eng/scripts)
$scriptPath = $PSScriptRoot
$repoRoot = Split-Path -Path (Split-Path -Path $scriptPath -Parent) -Parent

# Change to repository root for searching
Push-Location -Path $repoRoot

try {
    # Find all PublicAPI.Unshipped.txt files
    Write-Host "Searching for PublicAPI.Unshipped.txt files..." -ForegroundColor Green
    $publicApiFiles = Get-ChildItem -Path "." -Recurse -Filter "PublicAPI.Unshipped.txt" | Where-Object { $_.FullName -like "*PublicAPI*" }

    if ($publicApiFiles.Count -eq 0) {
        Write-Host "No PublicAPI.Unshipped.txt files found." -ForegroundColor Yellow
        exit 0
    }

    Write-Host "Found $($publicApiFiles.Count) PublicAPI.Unshipped.txt files." -ForegroundColor Green

    $changedFiles = 0
    $totalFiles = $publicApiFiles.Count

    foreach ($file in $publicApiFiles) {
        Write-Progress -Activity "Processing PublicAPI.Unshipped.txt files" -Status "Processing $($file.Name)" -PercentComplete (($changedFiles / $totalFiles) * 100)
        
        $relativePath = $file.FullName.Replace($repoRoot, "").TrimStart('\', '/')
        
        try {
            # Read the file content
            $content = Get-Content -Path $file.FullName -Raw
            
            # Handle empty files
            if ([string]::IsNullOrWhiteSpace($content)) {
                Write-Host "  Skipping empty file: $relativePath" -ForegroundColor Gray
                continue
            }
            
            # Split into lines and sort
            $lines = $content -split "`r?`n"
            $sortedLines = Invoke-PublicApiSort -Lines $lines
            
            # Join lines back to content
            $sortedContent = $sortedLines -join "`n"
            
            # Ensure file ends with a newline
            if (-not $sortedContent.EndsWith("`n")) {
                $sortedContent += "`n"
            }
            
            # Compare with original content
            if ($content.Replace("`r`n", "`n") -ne $sortedContent) {
                $changedFiles++
                
                if ($DryRun) {
                    Write-Host "  Would sort: $relativePath" -ForegroundColor Yellow
                } else {
                    # Write sorted content back to file
                    Set-Content -Path $file.FullName -Value $sortedContent -NoNewline
                    Write-Host "  Sorted: $relativePath" -ForegroundColor Green
                }
            } else {
                Write-Host "  Already sorted: $relativePath" -ForegroundColor Gray
            }
        }
        catch {
            Write-Host "  Error processing $relativePath`: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    Write-Progress -Activity "Processing PublicAPI.Unshipped.txt files" -Completed

    if ($DryRun) {
        Write-Host "`nDry run completed. $changedFiles files would be changed out of $totalFiles total files." -ForegroundColor Yellow
    } else {
        Write-Host "`nCompleted! $changedFiles files were sorted out of $totalFiles total files." -ForegroundColor Green
    }

    if ($changedFiles -gt 0) {
        Write-Host "`nNote: Files have been sorted while preserving *REMOVED* and ~ prefixes." -ForegroundColor Cyan
    }
}
finally {
    # Always return to original location
    Pop-Location
}
