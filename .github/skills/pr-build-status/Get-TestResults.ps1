<#
.SYNOPSIS
    Retrieves UI test results from an Azure DevOps build.

.DESCRIPTION
    Queries the Azure DevOps build timeline to find test jobs, then
    fetches the logs and extracts test results including passed, failed,
    and skipped tests with their details.

.PARAMETER BuildId
    The Azure DevOps build ID.

.PARAMETER Org
    The Azure DevOps organization. Defaults to 'dnceng-public'.

.PARAMETER Project
    The Azure DevOps project. Defaults to 'public'.

.PARAMETER FailedOnly
    If specified, only returns failed tests.

.PARAMETER JobFilter
    Optional filter to match job names (supports wildcards).

.EXAMPLE
    ./Get-TestResults.ps1 -BuildId 1240456

.EXAMPLE
    ./Get-TestResults.ps1 -BuildId 1240456 -FailedOnly

.EXAMPLE
    ./Get-TestResults.ps1 -BuildId 1240456 -JobFilter "*SafeArea*"

.OUTPUTS
    Array of objects with JobName, TestName, Result, Duration, ErrorMessage, and StackTrace properties.
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
    [switch]$FailedOnly,

    [Parameter(Mandatory = $false)]
    [string]$JobFilter
)

$ErrorActionPreference = "Stop"

# Get build timeline to find jobs with logs
$timelineUrl = "https://dev.azure.com/$Org/$Project/_apis/build/builds/$BuildId/timeline?api-version=7.0"

try {
    $timeline = Invoke-RestMethod -Uri $timelineUrl -Method Get -ContentType "application/json"
}
catch {
    Write-Error "Failed to query Azure DevOps timeline API: $_"
    exit 1
}

# Find jobs with log URLs (filter to failed jobs if FailedOnly, otherwise get completed jobs)
$jobs = $timeline.records | Where-Object { 
    $_.type -eq "Job" -and 
    $_.log.url -and
    $_.state -eq "completed" -and
    (-not $FailedOnly -or $_.result -eq "failed") -and
    (-not $JobFilter -or $_.name -like $JobFilter)
} | ForEach-Object {
    [PSCustomObject]@{
        Name   = $_.name
        Result = $_.result
        LogUrl = $_.log.url
    }
}

if (-not $jobs) {
    Write-Host "No matching jobs found in build $BuildId"
    return @()
}

$allTests = @()

foreach ($job in $jobs) {
    Write-Host "Analyzing job: $($job.Name) [$($job.Result)]" -ForegroundColor Yellow
    
    try {
        # Fetch the log content
        $logContent = Invoke-RestMethod -Uri $job.LogUrl -Method Get
        
        # Split into lines for processing
        $lines = $logContent -split "`n"
        
        # Find all test result lines: "Passed|Failed|Skipped <TestName> [duration]"
        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match "^\d{4}-\d{2}-\d{2}.*\s+(Passed|Failed|Skipped)\s+(\S+)\s+\[([^\]]+)\]") {
                $result = $matches[1]
                $testName = $matches[2]
                $duration = $matches[3]
                
                # Skip if we only want failed and this passed/skipped
                if ($FailedOnly -and $result -ne "Failed") {
                    continue
                }
                
                $errorMessage = ""
                $stackTrace = ""
                
                # For failed tests, look ahead for error message and stack trace
                if ($result -eq "Failed") {
                    for ($j = $i + 1; $j -lt [Math]::Min($i + 25, $lines.Count); $j++) {
                        $line = $lines[$j]
                        $cleanLine = $line -replace "^\d{4}-\d{2}-\d{2}T[\d:.]+Z\s*", ""
                        
                        if ($cleanLine -match "^\s*Error Message:") {
                            # Collect error message lines
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
                }
                
                $allTests += [PSCustomObject]@{
                    JobName      = $job.Name
                    TestName     = $testName
                    Result       = $result
                    Duration     = $duration
                    ErrorMessage = $errorMessage.Trim()
                    StackTrace   = $stackTrace
                }
            }
        }
    }
    catch {
        Write-Warning "Failed to fetch log for job $($job.Name): $_"
    }
}

# Remove duplicates (tests may be retried) - keep the last occurrence
$uniqueTests = $allTests | Group-Object -Property JobName, TestName | ForEach-Object {
    $_.Group | Select-Object -Last 1
}

# Summary
$passed = ($uniqueTests | Where-Object { $_.Result -eq "Passed" }).Count
$failed = ($uniqueTests | Where-Object { $_.Result -eq "Failed" }).Count
$skipped = ($uniqueTests | Where-Object { $_.Result -eq "Skipped" }).Count

Write-Host "`nTest Summary: $passed passed, $failed failed, $skipped skipped" -ForegroundColor Cyan

$uniqueTests
