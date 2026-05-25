<#
.SYNOPSIS
    Get console log for a Helix work item.

.DESCRIPTION
    Retrieves the console output from a Helix work item execution.
    Useful for debugging test failures.

.PARAMETER JobId
    The Helix job ID (GUID format).

.PARAMETER WorkItem
    The work item name (e.g., "Microsoft.Maui.Controls.Xaml.UnitTests.dll").
    Supports wildcards.

.PARAMETER FailedOnly
    If specified, only shows logs for failed work items.

.PARAMETER TailLines
    Number of lines to show from the end of the log. Default: 100

.EXAMPLE
    ./Get-HelixWorkItemLog.ps1 -JobId "12345678-..." -WorkItem "Microsoft.Maui.Controls.Xaml.UnitTests.dll"
    
.EXAMPLE
    ./Get-HelixWorkItemLog.ps1 -JobId "12345678-..." -FailedOnly
    
.EXAMPLE
    ./Get-HelixWorkItemLog.ps1 -JobId "12345678-..." -WorkItem "*Xaml*" -TailLines 200
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$JobId,
    
    [string]$WorkItem,
    
    [switch]$FailedOnly,
    
    [int]$TailLines = 100
)

$ErrorActionPreference = "Stop"

$baseUrl = "https://helix.dot.net/api/2019-06-17"

function Get-WorkItems {
    param([string]$JobId)
    
    $url = "$baseUrl/jobs/$JobId/workitems"
    try {
        $response = Invoke-RestMethod -Uri $url -Method Get
        return $response
    }
    catch {
        Write-Host "Error fetching work items: $_" -ForegroundColor Red
        return @()
    }
}

function Get-ConsoleLog {
    param(
        [string]$JobId,
        [string]$WorkItemName
    )
    
    $url = "$baseUrl/jobs/$JobId/workitems/$WorkItemName/console"
    try {
        $response = Invoke-RestMethod -Uri $url -Method Get
        return $response
    }
    catch {
        Write-Host "Error fetching console log: $_" -ForegroundColor Red
        return $null
    }
}

function Show-WorkItemLog {
    param(
        [string]$JobId,
        $WorkItem,
        [int]$TailLines
    )
    
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Work Item: $($WorkItem.Name)" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  State:     $($WorkItem.State)" -ForegroundColor $(if ($WorkItem.State -eq "Finished") { if ($WorkItem.ExitCode -eq 0) { "Green" } else { "Red" } } else { "Yellow" })
    Write-Host "  Exit Code: $($WorkItem.ExitCode)" -ForegroundColor $(if ($WorkItem.ExitCode -eq 0) { "Green" } else { "Red" })
    Write-Host "  Machine:   $($WorkItem.MachineName)" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "  Console Log (last $TailLines lines):" -ForegroundColor White
    Write-Host "  ─────────────────────────────────────────────────────────" -ForegroundColor Gray
    
    $log = Get-ConsoleLog -JobId $JobId -WorkItemName $WorkItem.Name
    if ($log) {
        # Split by lines and take last N
        $lines = $log -split "`n"
        $startIndex = [Math]::Max(0, $lines.Count - $TailLines)
        $relevantLines = $lines[$startIndex..($lines.Count - 1)]
        
        foreach ($line in $relevantLines) {
            # Color code based on content
            $color = "White"
            if ($line -match "FAIL|ERROR|Exception|❌") { $color = "Red" }
            elseif ($line -match "PASS|✅|succeeded") { $color = "Green" }
            elseif ($line -match "SKIP|⚠️|warning") { $color = "Yellow" }
            elseif ($line -match "^\s*at ") { $color = "DarkGray" }  # Stack trace
            
            Write-Host "  $line" -ForegroundColor $color
        }
    } else {
        Write-Host "  (No console log available)" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "  Full log: $baseUrl/jobs/$JobId/workitems/$($WorkItem.Name)/console" -ForegroundColor Gray
    Write-Host ""
}

# Main execution
Write-Host "Fetching Helix work items..." -ForegroundColor Cyan

$workItems = Get-WorkItems -JobId $JobId
if ($workItems.Count -eq 0) {
    Write-Host "No work items found for job $JobId" -ForegroundColor Yellow
    exit 0
}

# Filter work items
$filtered = $workItems

if ($FailedOnly) {
    $filtered = $filtered | Where-Object { $_.State -eq "Finished" -and $_.ExitCode -ne 0 }
    if ($filtered.Count -eq 0) {
        Write-Host "✅ No failed work items found!" -ForegroundColor Green
        exit 0
    }
}

if ($WorkItem) {
    $filtered = $filtered | Where-Object { $_.Name -like $WorkItem }
    if ($filtered.Count -eq 0) {
        Write-Host "No work items matching '$WorkItem' found" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Available work items:" -ForegroundColor Cyan
        foreach ($item in $workItems) {
            Write-Host "  - $($item.Name)" -ForegroundColor Gray
        }
        exit 1
    }
}

# Show logs for each filtered work item
foreach ($item in $filtered) {
    Show-WorkItemLog -JobId $JobId -WorkItem $item -TailLines $TailLines
}

# Summary
$failedCount = ($filtered | Where-Object { $_.ExitCode -ne 0 }).Count
if ($failedCount -gt 0) {
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host "  Summary: $failedCount failed work item(s)" -ForegroundColor Red
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Red
    exit 1
}
