<#
.SYNOPSIS
    Get status of a Helix job.

.DESCRIPTION
    Queries the Helix API to get the status of a submitted job and its work items.

.PARAMETER JobId
    The Helix job ID (GUID format).

.PARAMETER Wait
    If specified, polls until the job completes.

.PARAMETER PollInterval
    Seconds between polls when using -Wait. Default: 30

.EXAMPLE
    ./Get-HelixJobStatus.ps1 -JobId "12345678-1234-1234-1234-123456789abc"
    
.EXAMPLE
    ./Get-HelixJobStatus.ps1 -JobId "12345678-1234-1234-1234-123456789abc" -Wait
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$JobId,
    
    [switch]$Wait,
    
    [int]$PollInterval = 30
)

$ErrorActionPreference = "Stop"

$baseUrl = "https://helix.dot.net/api/2019-06-17"

function Get-JobDetails {
    param([string]$JobId)
    
    $url = "$baseUrl/jobs/$JobId/details"
    try {
        $response = Invoke-RestMethod -Uri $url -Method Get
        return $response
    }
    catch {
        Write-Host "Error fetching job details: $_" -ForegroundColor Red
        return $null
    }
}

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

function Show-JobStatus {
    param($Details, $WorkItems)
    
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Helix Job: $($Details.JobId)" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  Source:    $($Details.Source)" -ForegroundColor Gray
    Write-Host "  Type:      $($Details.Type)" -ForegroundColor Gray
    Write-Host "  Build:     $($Details.Build)" -ForegroundColor Gray
    Write-Host "  Queue:     $($Details.QueueId)" -ForegroundColor Gray
    Write-Host ""
    
    # Count work item states
    $pending = ($WorkItems | Where-Object { $_.State -eq "Waiting" -or $_.State -eq "Running" }).Count
    $passed = ($WorkItems | Where-Object { $_.State -eq "Finished" -and $_.ExitCode -eq 0 }).Count
    $failed = ($WorkItems | Where-Object { $_.State -eq "Finished" -and $_.ExitCode -ne 0 }).Count
    $total = $WorkItems.Count
    
    Write-Host "  Work Items: $total total" -ForegroundColor White
    if ($pending -gt 0) { Write-Host "    ⏳ Pending/Running: $pending" -ForegroundColor Yellow }
    if ($passed -gt 0) { Write-Host "    ✅ Passed: $passed" -ForegroundColor Green }
    if ($failed -gt 0) { Write-Host "    ❌ Failed: $failed" -ForegroundColor Red }
    Write-Host ""
    
    # Show work item details
    Write-Host "  Work Item Details:" -ForegroundColor White
    Write-Host "  ─────────────────────────────────────────────────────────" -ForegroundColor Gray
    
    foreach ($item in $WorkItems | Sort-Object Name) {
        $statusIcon = switch ($item.State) {
            "Waiting" { "⏳" }
            "Running" { "🔄" }
            "Finished" { if ($item.ExitCode -eq 0) { "✅" } else { "❌" } }
            default { "❓" }
        }
        
        $statusColor = switch ($item.State) {
            "Waiting" { "Yellow" }
            "Running" { "Cyan" }
            "Finished" { if ($item.ExitCode -eq 0) { "Green" } else { "Red" } }
            default { "Gray" }
        }
        
        Write-Host "  $statusIcon $($item.Name)" -ForegroundColor $statusColor
        if ($item.State -eq "Finished" -and $item.ExitCode -ne 0) {
            Write-Host "       Exit Code: $($item.ExitCode)" -ForegroundColor Red
            Write-Host "       Console: $baseUrl/jobs/$JobId/workitems/$($item.Name)/console" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
    Write-Host "  Portal: https://helix.dot.net/api/2019-06-17/jobs/$JobId" -ForegroundColor Gray
    Write-Host ""
    
    return @{
        Pending = $pending
        Passed = $passed
        Failed = $failed
        Total = $total
    }
}

# Main execution
Write-Host "Fetching Helix job status..." -ForegroundColor Cyan

do {
    $details = Get-JobDetails -JobId $JobId
    if (-not $details) {
        Write-Host "Could not fetch job details. Is the JobId correct?" -ForegroundColor Red
        exit 1
    }
    
    $workItems = Get-WorkItems -JobId $JobId
    $status = Show-JobStatus -Details $details -WorkItems $workItems
    
    if ($Wait -and $status.Pending -gt 0) {
        Write-Host "Waiting $PollInterval seconds before next poll..." -ForegroundColor Gray
        Start-Sleep -Seconds $PollInterval
        Clear-Host
    }
} while ($Wait -and $status.Pending -gt 0)

# Final summary
if ($status.Failed -gt 0) {
    Write-Host "❌ Job completed with $($status.Failed) failed work item(s)" -ForegroundColor Red
    exit 1
} elseif ($status.Pending -eq 0) {
    Write-Host "✅ Job completed successfully!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "⏳ Job still in progress..." -ForegroundColor Yellow
    exit 0
}
