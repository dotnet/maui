<#
.SYNOPSIS
    Retrieves build errors and test failures from an Azure DevOps build.

.DESCRIPTION
    Queries the Azure DevOps build timeline to find failed jobs and tasks,
    then extracts build errors (MSBuild errors, compilation failures) and
    test failures with their details.

.PARAMETER BuildId
    The Azure DevOps build ID.

.PARAMETER Org
    The Azure DevOps organization. Defaults to 'dnceng-public'.

.PARAMETER Project
    The Azure DevOps project. Defaults to 'public'.

.PARAMETER TestsOnly
    If specified, only returns test results (no build errors).

.PARAMETER ErrorsOnly
    If specified, only returns build errors (no test results).

.PARAMETER JobFilter
    Optional filter to match job/task names (supports wildcards).

.EXAMPLE
    ./Get-BuildErrors.ps1 -BuildId 1240456

.EXAMPLE
    ./Get-BuildErrors.ps1 -BuildId 1240456 -ErrorsOnly

.EXAMPLE
    ./Get-BuildErrors.ps1 -BuildId 1240456 -TestsOnly -JobFilter "*SafeArea*"

.OUTPUTS
    Objects with Type (BuildError/TestFailure), Source, Message, and Details properties.
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
    [switch]$TestsOnly,

    [Parameter(Mandatory = $false)]
    [switch]$ErrorsOnly,

    [Parameter(Mandatory = $false)]
    [string]$JobFilter
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

$allResults = @()

# --- SECTION 1: Find Build Errors from Failed Tasks ---
if (-not $TestsOnly) {
    $failedTasks = $timeline.records | Where-Object { 
        $_.type -eq "Task" -and 
        $_.result -eq "failed" -and
        $_.log.url -and
        (-not $JobFilter -or $_.name -like $JobFilter)
    }

    foreach ($task in $failedTasks) {
        Write-Host "Analyzing failed task: $($task.name)" -ForegroundColor Red
        
        try {
            $log = Invoke-RestMethod -Uri $task.log.url -Method Get
            $lines = $log -split "`n"
            
            # Find MSBuild errors and ##[error] markers
            $errorLines = $lines | Where-Object { 
                $_ -match ": error [A-Z]+\d*:" -or      # MSBuild errors (CS1234, MT1234, etc.)
                $_ -match ": Error :" -or               # Xamarin.Shared.Sdk errors
                $_ -match "##\[error\]"                 # Azure DevOps error markers
            }
            
            foreach ($errorLine in $errorLines) {
                # Clean up the line
                $cleanLine = $errorLine -replace "^\d{4}-\d{2}-\d{2}T[\d:.]+Z\s*", ""
                $cleanLine = $cleanLine -replace "##\[error\]", ""
                
                # Skip generic "exited with code" errors - we want the actual error
                if ($cleanLine -match "exited with code") {
                    continue
                }
                
                $allResults += [PSCustomObject]@{
                    Type    = "BuildError"
                    Source  = $task.name
                    Message = $cleanLine.Trim()
                    Details = ""
                }
            }
        }
        catch {
            Write-Warning "Failed to fetch log for task $($task.name): $_"
        }
    }
}

# --- SECTION 2: Find Test Failures from Jobs ---
if (-not $ErrorsOnly) {
    $jobs = $timeline.records | Where-Object { 
        $_.type -eq "Job" -and 
        $_.log.url -and
        $_.state -eq "completed" -and
        $_.result -eq "failed" -and
        (-not $JobFilter -or $_.name -like $JobFilter)
    }

    foreach ($job in $jobs) {
        Write-Host "Analyzing job for test failures: $($job.name)" -ForegroundColor Yellow
        
        try {
            $logContent = Invoke-RestMethod -Uri $job.log.url -Method Get
            $lines = $logContent -split "`n"
            
            # Find test result lines: "Failed <TestName> [duration]"
            for ($i = 0; $i -lt $lines.Count; $i++) {
                if ($lines[$i] -match "^\d{4}-\d{2}-\d{2}.*\s+Failed\s+(\S+)\s+\[([^\]]+)\]") {
                    $testName = $matches[1]
                    $duration = $matches[2]
                    
                    $errorMessage = ""
                    $stackTrace = ""
                    
                    # Look ahead for error message and stack trace
                    for ($j = $i + 1; $j -lt $lines.Count; $j++) {
                        $line = $lines[$j]
                        $cleanLine = $line -replace "^\d{4}-\d{2}-\d{2}T[\d:.]+Z\s*", ""
                        
                        if ($cleanLine -match "^\s*Error Message:") {
                            for ($k = $j + 1; $k -lt [Math]::Min($j + 10, $lines.Count); $k++) {
                                $msgLine = $lines[$k] -replace "^\d{4}-\d{2}-\d{2}T[\d:.]+Z\s*", ""
                                if ($msgLine -match "^\s*Stack Trace:" -or [string]::IsNullOrWhiteSpace($msgLine)) {
                                    break
                                }
                                $errorMessage += $msgLine.Trim() + " "
                            }
                        }
                        
                        if ($cleanLine -match "^\s*Stack Trace:") {
                            for ($k = $j + 1; $k -lt [Math]::Min($j + 5, $lines.Count); $k++) {
                                $stLine = $lines[$k] -replace "^\d{4}-\d{2}-\d{2}T[\d:.]+Z\s*", ""
                                if ($stLine -match "at .+ in .+:line \d+") {
                                    $stackTrace = $stLine.Trim()
                                    break
                                }
                            }
                            break
                        }
                        
                        # Stop if we hit the next test
                        if ($cleanLine -match "^\s*(Passed|Failed|Skipped)\s+\S+\s+\[") {
                            break
                        }
                    }
                    
                    $allResults += [PSCustomObject]@{
                        Type    = "TestFailure"
                        Source  = $job.name
                        Message = $testName
                        Details = if ($errorMessage) { "$errorMessage`n$stackTrace".Trim() } else { $stackTrace }
                    }
                }
            }
        }
        catch {
            Write-Warning "Failed to fetch log for job $($job.name): $_"
        }
    }
}

# Remove duplicate errors (same message from same source)
$uniqueResults = $allResults | Group-Object -Property Type, Source, Message | ForEach-Object {
    $_.Group | Select-Object -First 1
}

# Summary
$buildErrors = ($uniqueResults | Where-Object { $_.Type -eq "BuildError" }).Count
$testFailures = ($uniqueResults | Where-Object { $_.Type -eq "TestFailure" }).Count

Write-Host "`nSummary: $buildErrors build error(s), $testFailures test failure(s)" -ForegroundColor Cyan

$uniqueResults
