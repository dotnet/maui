<#
.SYNOPSIS
    Retrieves detailed status information for an Azure DevOps build.

.DESCRIPTION
    Queries the Azure DevOps build timeline API and returns comprehensive
    information about the build including all stages, their status, and
    any failed or canceled jobs.

.PARAMETER BuildId
    The Azure DevOps build ID.

.PARAMETER Org
    The Azure DevOps organization. Defaults to 'dnceng-public'.

.PARAMETER Project
    The Azure DevOps project. Defaults to 'public'.

.PARAMETER FailedOnly
    If specified, only returns failed or canceled stages and jobs.

.EXAMPLE
    ./Get-BuildInfo.ps1 -BuildId 1240455

.EXAMPLE
    ./Get-BuildInfo.ps1 -BuildId 1240455 -FailedOnly

.EXAMPLE
    ./Get-BuildInfo.ps1 -BuildId 1240455 -Org "dnceng-public" -Project "public"

.OUTPUTS
    Object with BuildId, Status, Result, Stages, and FailedJobs properties.
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
    [switch]$FailedOnly
)

$ErrorActionPreference = "Stop"

# Get build info
$buildUrl = "https://dev.azure.com/$Org/$Project/_apis/build/builds/$BuildId`?api-version=7.0"
$timelineUrl = "https://dev.azure.com/$Org/$Project/_apis/build/builds/$BuildId/timeline?api-version=7.0"

try {
    $build = Invoke-RestMethod -Uri $buildUrl -Method Get -ContentType "application/json"
    $timeline = Invoke-RestMethod -Uri $timelineUrl -Method Get -ContentType "application/json"
}
catch {
    Write-Error "Failed to query Azure DevOps API: $_"
    exit 1
}

# Extract stages
$stages = $timeline.records | Where-Object { $_.type -eq "Stage" } | ForEach-Object {
    [PSCustomObject]@{
        Name   = $_.name
        State  = $_.state
        Result = $_.result
    }
} | Sort-Object -Property { $_.State -eq "completed" }, { $_.State -eq "inProgress" }

# Extract failed/canceled jobs
$failedJobs = $timeline.records | 
    Where-Object { 
        ($_.type -eq "Stage" -or $_.type -eq "Job") -and 
        ($_.result -eq "failed" -or $_.result -eq "canceled") 
    } | 
    ForEach-Object {
        [PSCustomObject]@{
            Name   = $_.name
            Type   = $_.type
            Result = $_.result
        }
    } | Sort-Object -Property Type, Name

if ($FailedOnly) {
    $failedJobs
}
else {
    [PSCustomObject]@{
        BuildId    = $BuildId
        BuildNumber = $build.buildNumber
        Status     = $build.status
        Result     = $build.result
        Pipeline   = $build.definition.name
        StartTime  = $build.startTime
        FinishTime = $build.finishTime
        Stages     = $stages
        FailedJobs = $failedJobs
        Link       = "https://dev.azure.com/$Org/$Project/_build/results?buildId=$BuildId"
    }
}
