#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Records a triaged issue to the current session.

.DESCRIPTION
    This script adds a triaged issue to the session tracker, recording:
    - Issue number
    - Action taken (milestone, labels added)
    - Timestamp
    
    Use this after triaging each issue to maintain session history.

.PARAMETER IssueNumber
    The GitHub issue number that was triaged

.PARAMETER Milestone
    The milestone that was set (optional)

.PARAMETER LabelsAdded
    Labels that were added (comma-separated, optional)

.PARAMETER LabelsRemoved
    Labels that were removed (comma-separated, optional)

.PARAMETER Action
    The action taken: "milestoned", "labeled", "skipped", "closed" (default: milestoned)

.PARAMETER SessionFile
    Path to the session file (default: most recent session in CustomAgentLogsTmp/Triage)

.EXAMPLE
    ./record-triage.ps1 -IssueNumber 33272 -Milestone "Backlog"
    # Records that issue 33272 was set to Backlog

.EXAMPLE
    ./record-triage.ps1 -IssueNumber 33264 -Milestone ".NET 10.0 SR3" -LabelsAdded "i/regression"
    # Records milestone and label changes
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$IssueNumber,

    [Parameter(Mandatory = $false)]
    [string]$Milestone = "",

    [Parameter(Mandatory = $false)]
    [string]$LabelsAdded = "",

    [Parameter(Mandatory = $false)]
    [string]$LabelsRemoved = "",

    [Parameter(Mandatory = $false)]
    [ValidateSet("milestoned", "labeled", "skipped", "closed")]
    [string]$Action = "milestoned",

    [Parameter(Mandatory = $false)]
    [string]$SessionFile = ""
)

$ErrorActionPreference = "Stop"

$outputDir = "CustomAgentLogsTmp/Triage"

# Find session file if not provided
if ($SessionFile -eq "") {
    if (Test-Path $outputDir) {
        $latestSession = Get-ChildItem -Path $outputDir -Filter "*.json" | 
            Sort-Object LastWriteTime -Descending | 
            Select-Object -First 1
        
        if ($latestSession) {
            $SessionFile = $latestSession.FullName
        }
    }
}

if ($SessionFile -eq "" -or -not (Test-Path $SessionFile)) {
    Write-Host "No active session found. Run init-triage-session.ps1 first." -ForegroundColor Yellow
    exit 1
}

# Load session
$session = Get-Content $SessionFile | ConvertFrom-Json

# Create triage record
$record = [PSCustomObject]@{
    IssueNumber = $IssueNumber
    Action = $Action
    Milestone = $Milestone
    LabelsAdded = if ($LabelsAdded) { $LabelsAdded -split "," | ForEach-Object { $_.Trim() } } else { @() }
    LabelsRemoved = if ($LabelsRemoved) { $LabelsRemoved -split "," | ForEach-Object { $_.Trim() } } else { @() }
    TriagedAt = (Get-Date).ToString("o")
}

# Add to session
$session.TriagedIssues += $record
$session.Stats.Total++

# Update stats
switch -Regex ($Milestone) {
    "Backlog" { $session.Stats.Backlog++ }
    "Servicing" { $session.Stats.Servicing++ }
    "SR\d" { $session.Stats.SR++ }
}
if ($Action -eq "skipped") { $session.Stats.Skipped++ }

# Save session
$session | ConvertTo-Json -Depth 10 | Out-File -FilePath $SessionFile -Encoding UTF8

Write-Host "✓ Recorded: #$IssueNumber → $Action" -ForegroundColor Green
if ($Milestone) { Write-Host "  Milestone: $Milestone" -ForegroundColor DarkGray }
if ($LabelsAdded) { Write-Host "  Labels+: $LabelsAdded" -ForegroundColor DarkGray }
if ($LabelsRemoved) { Write-Host "  Labels-: $LabelsRemoved" -ForegroundColor DarkGray }
Write-Host "  Session total: $($session.Stats.Total) issues triaged" -ForegroundColor DarkGray
