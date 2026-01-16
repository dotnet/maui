<#
.SYNOPSIS
    Retrieves Helix console logs for failed work items from an Azure DevOps build.

.DESCRIPTION
    Parses Azure DevOps build logs to extract Helix job IDs, then queries the Helix API
    to get console logs for failed work items. This is essential for debugging device test
    failures that run on Helix infrastructure.

.PARAMETER BuildId
    The Azure DevOps build ID.

.PARAMETER Org
    The Azure DevOps organization. Defaults to 'dnceng-public'.

.PARAMETER Project
    The Azure DevOps project. Defaults to 'public'.

.PARAMETER Platform
    Optional filter for platform (e.g., 'Windows', 'iOS', 'Android', 'MacCatalyst').

.PARAMETER WorkItem
    Optional filter for specific work item name (supports wildcards).

.PARAMETER ShowConsoleLog
    If specified, displays the full console log content for each failed work item.

.PARAMETER TailLines
    Number of lines to show from the end of console logs. Default is 100.

.EXAMPLE
    ./Get-HelixLogs.ps1 -BuildId 1255952

.EXAMPLE
    ./Get-HelixLogs.ps1 -BuildId 1255952 -Platform Windows -ShowConsoleLog

.EXAMPLE
    ./Get-HelixLogs.ps1 -BuildId 1255952 -WorkItem "*Lifecycle*" -ShowConsoleLog -TailLines 200

.OUTPUTS
    Objects with JobId, WorkItem, Queue, ConsoleUrl, and optionally ConsoleLog properties.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$BuildId,

    [Parameter(Mandatory = $false)]
    [string]$Org = "dnceng-public",

    [Parameter(Mandatory = $false)]
    [string]$Project = "public",

    [Parameter(Mandatory = $false)]
    [string]$Platform,

    [Parameter(Mandatory = $false)]
    [string]$WorkItem,

    [Parameter(Mandatory = $false)]
    [switch]$ShowConsoleLog,

    [Parameter(Mandatory = $false)]
    [int]$TailLines = 100
)

$ErrorActionPreference = "Stop"

# Get build timeline
$timelineUrl = "https://dev.azure.com/$Org/$Project/_apis/build/builds/${BuildId}/timeline?api-version=7.0"

try {
    $timeline = Invoke-RestMethod -Uri $timelineUrl -Method Get -ContentType "application/json"
}
catch {
    Write-Error "Failed to query Azure DevOps timeline API: $_"
    exit 1
}

# Build platform filter pattern
$platformPattern = if ($Platform) {
    switch ($Platform.ToLower()) {
        "windows" { "*Windows*" }
        "ios"     { "*iOS*" }
        "android" { "*Android*" }
        "maccatalyst" { "*MacCatalyst*|*Catalyst*" }
        default   { "*$Platform*" }
    }
} else {
    "*"
}

# Find Helix-related tasks with logs (looking for DeviceTests tasks that submit to Helix)
$helixTasks = $timeline.records | Where-Object { 
    $_.name -like "*DeviceTests*" -and 
    $_.log.url -and
    ($_.result -eq "failed" -or $_.result -eq "succeeded") -and
    (-not $Platform -or $_.name -like $platformPattern)
}

if (-not $helixTasks) {
    Write-Host "No Helix-related tasks found in build $BuildId" -ForegroundColor Yellow
    exit 0
}

$allHelixJobs = @{}
$allResults = @()

foreach ($task in $helixTasks) {
    Write-Host "Scanning task: $($task.name) [$($task.result)]" -ForegroundColor $(if ($task.result -eq "failed") { "Red" } else { "Gray" })
    
    try {
        $logContent = Invoke-RestMethod -Uri $task.log.url -Method Get
        
        # Extract Helix job IDs from log (pattern: "jobs/{guid}/workitems")
        $jobMatches = [regex]::Matches($logContent, "jobs/([a-f0-9-]{36})/workitems")
        
        foreach ($match in $jobMatches) {
            $jobId = $match.Groups[1].Value
            if (-not $allHelixJobs.ContainsKey($jobId)) {
                $allHelixJobs[$jobId] = @{
                    Task = $task.name
                    Result = $task.result
                }
            }
        }
    }
    catch {
        Write-Warning "Failed to fetch log for task $($task.name): $_"
    }
}

if ($allHelixJobs.Count -eq 0) {
    Write-Host "No Helix job IDs found in build logs" -ForegroundColor Yellow
    exit 0
}

Write-Host "`nFound $($allHelixJobs.Count) Helix job(s)" -ForegroundColor Cyan

# Query each Helix job for work items
foreach ($jobId in $allHelixJobs.Keys) {
    $jobInfo = $allHelixJobs[$jobId]
    Write-Host "`n--- Helix Job: $jobId ---" -ForegroundColor Yellow
    Write-Host "From task: $($jobInfo.Task)" -ForegroundColor Gray
    
    try {
        # Get job details for queue info
        $jobDetailsUrl = "https://helix.dot.net/api/jobs/${jobId}/details?api-version=2019-06-17"
        $jobDetails = Invoke-RestMethod -Uri $jobDetailsUrl -Method Get
        $queue = $jobDetails.QueueId
        
        Write-Host "Queue: $queue" -ForegroundColor Gray
        
        # Get work items
        $workItemsUrl = "https://helix.dot.net/api/jobs/${jobId}/workitems?api-version=2019-06-17"
        $workItems = Invoke-RestMethod -Uri $workItemsUrl -Method Get
        
        foreach ($wi in $workItems) {
            # Skip the controller work item
            if ($wi.Name -eq "HelixController Work Queueing") {
                continue
            }
            
            # Apply work item filter if specified
            if ($WorkItem -and $wi.Name -notlike $WorkItem) {
                continue
            }
            
            $consoleUrl = "https://helix.dot.net/api/2019-06-17/jobs/${jobId}/workitems/$($wi.Name)/console"
            
            # Determine if this is a failure
            # Check: 1) ExitCode non-zero, 2) Parent task failed
            $isFailed = $false
            if ($wi.ExitCode -and $wi.ExitCode -ne 0) {
                $isFailed = $true
            }
            elseif ($jobInfo.Result -eq "failed") {
                $isFailed = $true
            }
            
            # For work items from failed tasks, always try to get console log to check for errors
            $shouldFetchLog = $ShowConsoleLog -and ($isFailed -or $jobInfo.Result -eq "failed")
            
            $consoleLogContent = $null
            if ($shouldFetchLog) {
                Write-Host "`nFetching console log for: $($wi.Name)" -ForegroundColor Cyan
                
                try {
                    $consoleLog = Invoke-RestMethod -Uri $consoleUrl -Method Get
                    
                    # Check if the log indicates a failure (even if ExitCode wasn't set)
                    if ($consoleLog -match "exited with (code )?1\]|ERROR:|FAILED|exception|crash") {
                        $isFailed = $true
                    }
                    
                    # Get tail lines
                    $lines = $consoleLog -split "`n"
                    if ($TailLines -gt 0 -and $lines.Count -gt $TailLines) {
                        $consoleLogContent = ($lines | Select-Object -Last $TailLines) -join "`n"
                        Write-Host "... (showing last $TailLines of $($lines.Count) lines)" -ForegroundColor Gray
                    }
                    else {
                        $consoleLogContent = $consoleLog
                    }
                    
                    Write-Host $consoleLogContent
                }
                catch {
                    Write-Warning "Failed to fetch console log: $_"
                }
            }
            
            $result = [PSCustomObject]@{
                JobId      = $jobId
                WorkItem   = $wi.Name
                State      = $wi.State
                ExitCode   = $wi.ExitCode
                Queue      = $queue
                IsFailed   = $isFailed
                ConsoleUrl = $consoleUrl
                ConsoleLog = $consoleLogContent
            }
            
            $allResults += $result
            
            # Print summary line
            $statusColor = if ($isFailed) { "Red" } else { "Green" }
            $statusSymbol = if ($isFailed) { "X" } else { "√" }
            Write-Host "  [$statusSymbol] $($wi.Name) (Exit: $($wi.ExitCode))" -ForegroundColor $statusColor
        }
    }
    catch {
        Write-Warning "Failed to query Helix job $jobId`: $_"
    }
}

# Summary
$failedCount = ($allResults | Where-Object { $_.IsFailed }).Count
$totalCount = $allResults.Count

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Summary: $failedCount failed / $totalCount total work items" -ForegroundColor $(if ($failedCount -gt 0) { "Red" } else { "Green" })
Write-Host "========================================" -ForegroundColor Cyan

# Output results
$allResults
