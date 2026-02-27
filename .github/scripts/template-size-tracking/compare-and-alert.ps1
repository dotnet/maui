#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Compares current metrics with previous runs and generates alerts.

.PARAMETER CurrentMetricsPath
    Path to current metrics directory.

.PARAMETER PreviousMetricsPath
    Path to previous metrics directory.

.PARAMETER HistoricalMetricsPath
    Path to historical metrics directory with dated subdirectories.

.PARAMETER AlertThreshold
    Percentage threshold for alerts (default: 10).

.PARAMETER FailureThreshold
    Percentage threshold for critical failures (default: 20).
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$CurrentMetricsPath,

    [Parameter(Mandatory=$true)]
    [string]$PreviousMetricsPath,

    [Parameter(Mandatory=$false)]
    [string]$HistoricalMetricsPath = "metrics-history",

    [Parameter(Mandatory=$false)]
    [decimal]$AlertThreshold = 10,

    [Parameter(Mandatory=$false)]
    [decimal]$FailureThreshold = 20
)

$ErrorActionPreference = "Stop"

Write-Host "=== Comparing Metrics ===" -ForegroundColor Cyan
Write-Host "Alert Threshold: $AlertThreshold%"
Write-Host "Failure Threshold: $FailureThreshold%"

# Load current metrics
$currentMetrics = @()
$currentFiles = Get-ChildItem -Path $CurrentMetricsPath -Filter "*.json" -Recurse

foreach ($file in $currentFiles) {
    $content = Get-Content $file.FullName -Raw | ConvertFrom-Json
    $currentMetrics += $content
}

Write-Host "Loaded $($currentMetrics.Count) current metrics"

# Load previous metrics
$previousMetrics = @()
if (Test-Path $PreviousMetricsPath) {
    $previousFiles = Get-ChildItem -Path $PreviousMetricsPath -Filter "*.json" -Recurse

    foreach ($file in $previousFiles) {
        try {
            $content = Get-Content $file.FullName -Raw | ConvertFrom-Json
            $previousMetrics += $content
        } catch {
            Write-Warning "Could not parse previous metric file: $($file.Name)"
        }
    }

    Write-Host "Loaded $($previousMetrics.Count) previous metrics"
} else {
    Write-Host "No previous metrics found - this appears to be the first run"
}

# Load historical metrics
$historicalMetrics = @{}
$daysToLoad = @(1, 2, 3, 4, 5, 7, 30)

foreach ($daysAgo in $daysToLoad) {
    $targetDate = (Get-Date).AddDays(-$daysAgo).ToString("yyyy-MM-dd")
    $historicalPath = Join-Path $HistoricalMetricsPath $targetDate

    if (Test-Path $historicalPath) {
        $historicalFiles = Get-ChildItem -Path $historicalPath -Filter "*.json" -Recurse
        $metrics = @()

        foreach ($file in $historicalFiles) {
            try {
                $content = Get-Content $file.FullName -Raw | ConvertFrom-Json
                $metrics += $content
            } catch {
                Write-Warning "Could not parse historical metric file: $($file.Name)"
            }
        }

        $historicalMetrics[$daysAgo] = $metrics
        Write-Host "Loaded $($metrics.Count) metrics from $daysAgo day(s) ago ($targetDate)"
    } else {
        Write-Host "No historical metrics found for $daysAgo day(s) ago ($targetDate)"
        $historicalMetrics[$daysAgo] = @()
    }
}

# Compare metrics
$comparisons = @()
$alerts = @()
$criticalAlerts = @()

foreach ($current in $currentMetrics) {
    $currentKey = if ($current.PSObject.Properties["description"]) { $current.description } else { $current.platform }

    # Find matching previous metric
    $previous = $previousMetrics | Where-Object {
        $previousKey = if ($_.PSObject.Properties["description"]) { $_.description } else { $_.platform }
        $_.dotnetVersion -eq $current.dotnetVersion -and
        $_.template -eq $current.template -and
        $previousKey -eq $currentKey
    } | Select-Object -First 1

    $comparison = @{
        dotnetVersion = $current.dotnetVersion
        template = $current.template
        platform = $currentKey
        currentSize = $current.packageSize
        currentCompressedSize = $current.compressedSize
        currentBuildTime = $current.buildTimeSeconds
        previousSize = if ($previous) { $previous.packageSize } else { 0 }
        previousBuildTime = if ($previous) { $previous.buildTimeSeconds } else { 0 }
        sizeChange = 0
        sizeChangePercent = 0
        buildTimeChange = 0
        buildTimeChangePercent = 0
        status = "new"
        historical = @{}
    }

    # Historical data
    foreach ($daysAgo in $daysToLoad) {
        $historical = $historicalMetrics[$daysAgo] | Where-Object {
            $historicalKey = if ($_.PSObject.Properties["description"]) { $_.description } else { $_.platform }
            $_.dotnetVersion -eq $current.dotnetVersion -and
            $_.template -eq $current.template -and
            $historicalKey -eq $currentKey
        } | Select-Object -First 1

        if ($historical) {
            $compressedSize = if ($historical.PSObject.Properties["compressedSize"]) { $historical.compressedSize } else { 0 }
            $percentChange = 0

            if ($compressedSize -gt 0 -and $current.compressedSize -gt 0) {
                $percentChange = [math]::Round((($current.compressedSize - $compressedSize) / $compressedSize) * 100, 2)
            }

            $comparison.historical["days_$daysAgo"] = @{
                compressedSize = $compressedSize
                percentChange = $percentChange
            }
        } else {
            $comparison.historical["days_$daysAgo"] = @{
                compressedSize = 0
                percentChange = 0
            }
        }
    }

    if ($previous) {
        if ($previous.packageSize -gt 0) {
            $comparison.sizeChange = $current.packageSize - $previous.packageSize
            $comparison.sizeChangePercent = [math]::Round(($comparison.sizeChange / $previous.packageSize) * 100, 2)
        }

        if ($previous.buildTimeSeconds -gt 0) {
            $comparison.buildTimeChange = $current.buildTimeSeconds - $previous.buildTimeSeconds
            $comparison.buildTimeChangePercent = [math]::Round(($comparison.buildTimeChange / $previous.buildTimeSeconds) * 100, 2)
        }

        if ($comparison.sizeChangePercent -ge $FailureThreshold) {
            $comparison.status = "critical"
            $criticalAlerts += $comparison
        } elseif ($comparison.sizeChangePercent -ge $AlertThreshold) {
            $comparison.status = "alert"
            $alerts += $comparison
        } elseif ($comparison.sizeChangePercent -gt 0) {
            $comparison.status = "increased"
        } elseif ($comparison.sizeChangePercent -lt 0) {
            $comparison.status = "decreased"
        } else {
            $comparison.status = "unchanged"
        }
    }

    $comparisons += $comparison
}

# Save comparison results
$comparisons | ConvertTo-Json -Depth 10 | Out-File -FilePath "comparison.json" -Encoding UTF8

# Generate alert report if needed
if ($alerts.Count -gt 0 -or $criticalAlerts.Count -gt 0) {
    Write-Host "`n⚠️  ALERTS DETECTED!" -ForegroundColor Red

    $alertReport = @"
# ⚠️ .NET MAUI Template Size Alert - $(Get-Date -Format "yyyy-MM-dd")

Significant size increases detected in .NET MAUI templates.

"@

    if ($criticalAlerts.Count -gt 0) {
        $alertReport += @"

## 🔴 Critical Increases (>$FailureThreshold%)

| Template | Platform | .NET | Previous | Current | Change |
|----------|----------|------|----------|---------|--------|

"@

        foreach ($alert in $criticalAlerts) {
            $prevMB = [math]::Round($alert.previousSize / 1MB, 2)
            $currMB = [math]::Round($alert.currentSize / 1MB, 2)
            $change = "+$($alert.sizeChangePercent)%"
            $alertReport += "| $($alert.template) | $($alert.platform) | $($alert.dotnetVersion) | $prevMB MB | $currMB MB | $change 🔴 |`n"
        }
    }

    if ($alerts.Count -gt 0) {
        $alertReport += @"

## 🟡 Notable Increases (>$AlertThreshold%)

| Template | Platform | .NET | Previous | Current | Change |
|----------|----------|------|----------|---------|--------|

"@

        foreach ($alert in $alerts) {
            $prevMB = [math]::Round($alert.previousSize / 1MB, 2)
            $currMB = [math]::Round($alert.currentSize / 1MB, 2)
            $change = "+$($alert.sizeChangePercent)%"
            $alertReport += "| $($alert.template) | $($alert.platform) | $($alert.dotnetVersion) | $prevMB MB | $currMB MB | $change 🟡 |`n"
        }
    }

    $alertReport += @"

## Details

- **Workflow Run**: [View Details]($($env:GITHUB_SERVER_URL)/$($env:GITHUB_REPOSITORY)/actions/runs/$($env:GITHUB_RUN_ID))
- **Alert Threshold**: $AlertThreshold%
- **Failure Threshold**: $FailureThreshold%

---
*This issue was automatically generated by the daily template size tracking workflow.*
"@

    $alertReport | Out-File -FilePath "alert-report.md" -Encoding UTF8

    Add-Content -Path $env:GITHUB_OUTPUT -Value "alert=true"

    if ($criticalAlerts.Count -gt 0) {
        Add-Content -Path $env:GITHUB_OUTPUT -Value "critical=true"
        Write-Host "Critical alerts: $($criticalAlerts.Count)" -ForegroundColor Red
    } else {
        Add-Content -Path $env:GITHUB_OUTPUT -Value "critical=false"
    }
} else {
    Write-Host "`n✓ No significant size increases detected" -ForegroundColor Green
    Add-Content -Path $env:GITHUB_OUTPUT -Value "alert=false"
    Add-Content -Path $env:GITHUB_OUTPUT -Value "critical=false"
}

# Summary
Write-Host "`n=== Comparison Summary ===" -ForegroundColor Cyan
Write-Host "Total comparisons: $($comparisons.Count)"
Write-Host "New entries: $(($comparisons | Where-Object { $_.status -eq 'new' }).Count)"
Write-Host "Decreased: $(($comparisons | Where-Object { $_.status -eq 'decreased' }).Count)"
Write-Host "Unchanged: $(($comparisons | Where-Object { $_.status -eq 'unchanged' }).Count)"
Write-Host "Increased: $(($comparisons | Where-Object { $_.status -eq 'increased' }).Count)"
Write-Host "Alerts: $($alerts.Count)" -ForegroundColor Yellow
Write-Host "Critical: $($criticalAlerts.Count)" -ForegroundColor Red
