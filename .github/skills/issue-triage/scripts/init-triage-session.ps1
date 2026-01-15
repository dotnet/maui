#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Initializes a triage session by loading current milestones, labels, and creating a session tracker.

.DESCRIPTION
    This script prepares for an issue triage session by:
    1. Querying all open milestones from dotnet/maui
    2. Loading common labels for quick reference
    3. Creating a session file to track triaged issues
    
    Run this at the start of a triage session to have current milestone/label data available.

.PARAMETER SessionName
    Optional name for the triage session (default: timestamp-based)

.PARAMETER OutputDir
    Directory to store session files (default: CustomAgentLogsTmp/Triage)

.EXAMPLE
    ./init-triage-session.ps1
    # Initializes a new triage session with defaults

.EXAMPLE
    ./init-triage-session.ps1 -SessionName "android-triage"
    # Creates a named session for Android issue triage
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$SessionName = "",

    [Parameter(Mandatory = $false)]
    [string]$OutputDir = "CustomAgentLogsTmp/Triage"
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           Initializing Triage Session                     ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Create output directory
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# Generate session name if not provided
if ($SessionName -eq "") {
    $SessionName = "triage-$(Get-Date -Format 'yyyy-MM-dd-HHmm')"
}

$sessionFile = Join-Path $OutputDir "$SessionName.json"

Write-Host ""
Write-Host "Session: $SessionName" -ForegroundColor Green
Write-Host "Output: $sessionFile" -ForegroundColor DarkGray

# Query open milestones
Write-Host ""
Write-Host "Fetching open milestones..." -ForegroundColor Cyan

$milestones = @()
try {
    $msResult = gh api repos/dotnet/maui/milestones --jq '.[] | {number, title, due_on, open_issues}' 2>&1
    $msLines = $msResult -split "`n" | Where-Object { $_ -match "^\{" }
    
    foreach ($line in $msLines) {
        $ms = $line | ConvertFrom-Json
        $milestones += [PSCustomObject]@{
            Number = $ms.number
            Title = $ms.title
            DueOn = $ms.due_on
            OpenIssues = $ms.open_issues
        }
    }
    
    # Sort by title for easy reference
    $milestones = $milestones | Sort-Object Title
    
    Write-Host "  Found $($milestones.Count) open milestones:" -ForegroundColor Green
    
    # Group milestones by type
    $srMilestones = $milestones | Where-Object { $_.Title -match "SR\d|Servicing" }
    $backlog = $milestones | Where-Object { $_.Title -eq "Backlog" }
    $otherMs = $milestones | Where-Object { $_.Title -notmatch "SR\d|Servicing" -and $_.Title -ne "Backlog" }
    
    Write-Host ""
    Write-Host "  Servicing Releases:" -ForegroundColor Yellow
    foreach ($ms in $srMilestones) {
        $dueInfo = ""
        if ($ms.DueOn -and $ms.DueOn -is [string] -and $ms.DueOn.Length -ge 10) {
            $dueInfo = " (due: $($ms.DueOn.Substring(0, 10)))"
        }
        Write-Host "    - $($ms.Title)$dueInfo [$($ms.OpenIssues) open]"
    }
    
    if ($backlog) {
        Write-Host ""
        Write-Host "  Backlog:" -ForegroundColor Yellow
        Write-Host "    - $($backlog.Title) [$($backlog.OpenIssues) open]"
    }
    
    if ($otherMs.Count -gt 0) {
        Write-Host ""
        Write-Host "  Other:" -ForegroundColor Yellow
        foreach ($ms in $otherMs | Select-Object -First 5) {
            Write-Host "    - $($ms.Title) [$($ms.OpenIssues) open]"
        }
        if ($otherMs.Count -gt 5) {
            Write-Host "    ... and $($otherMs.Count - 5) more"
        }
    }
}
catch {
    Write-Host "  Failed to fetch milestones: $_" -ForegroundColor Red
}

# Query common labels
Write-Host ""
Write-Host "Fetching labels..." -ForegroundColor Cyan

$labels = @{
    Platforms = @()
    Areas = @()
    Status = @()
    Priority = @()
    Regression = @()
    Other = @()
}

try {
    $labelResult = gh api repos/dotnet/maui/labels --paginate --jq '.[].name' 2>&1
    $allLabels = $labelResult -split "`n" | Where-Object { $_ -ne "" }
    
    foreach ($label in $allLabels) {
        if ($label -match "^platform/") {
            $labels.Platforms += $label
        }
        elseif ($label -match "^area-") {
            $labels.Areas += $label
        }
        elseif ($label -match "^s/") {
            $labels.Status += $label
        }
        elseif ($label -match "^p/") {
            $labels.Priority += $label
        }
        elseif ($label -match "regression|regressed") {
            $labels.Regression += $label
        }
    }
    
    Write-Host "  Platforms: $($labels.Platforms.Count) labels"
    Write-Host "  Areas: $($labels.Areas.Count) labels"
    Write-Host "  Status: $($labels.Status.Count) labels"
    Write-Host "  Priority: $($labels.Priority.Count) labels"
    Write-Host "  Regression: $($labels.Regression.Count) labels"
}
catch {
    Write-Host "  Failed to fetch labels: $_" -ForegroundColor Red
}

# Create session object
$session = [PSCustomObject]@{
    Name = $SessionName
    StartedAt = (Get-Date).ToString("o")
    Milestones = $milestones
    Labels = $labels
    TriagedIssues = @()
    Stats = @{
        Total = 0
        Backlog = 0
        Servicing = 0
        SR = 0
        Skipped = 0
    }
}

# Save session file
$session | ConvertTo-Json -Depth 10 | Out-File -FilePath $sessionFile -Encoding UTF8

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Session initialized! Quick reference:" -ForegroundColor Green
Write-Host ""
Write-Host "  Common Milestones:" -ForegroundColor Yellow
$srMilestones | ForEach-Object { Write-Host "    $($_.Title)" }
Write-Host "    Backlog"
Write-Host ""
Write-Host "  Priority Labels:" -ForegroundColor Yellow
$labels.Priority | ForEach-Object { Write-Host "    $_" }
Write-Host ""
Write-Host "  Regression Labels:" -ForegroundColor Yellow
$labels.Regression | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" }
Write-Host ""
Write-Host "  Session file: $sessionFile" -ForegroundColor DarkGray
Write-Host ""
Write-Host "Ready to triage! Run query-issues.ps1 to load issues." -ForegroundColor Green

# Return session for pipeline usage
return $session
