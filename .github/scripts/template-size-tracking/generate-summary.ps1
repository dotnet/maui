#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates a GitHub Actions summary report for .NET MAUI template size tracking.

.PARAMETER MetricsPath
    Path to current metrics directory.

.PARAMETER ComparisonPath
    Path to comparison JSON file.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$MetricsPath,

    [Parameter(Mandatory=$false)]
    [string]$ComparisonPath = "comparison.json"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Generating Summary Report ===" -ForegroundColor Cyan

# Load comparisons
$comparisons = @()
if (Test-Path $ComparisonPath) {
    $comparisons = Get-Content $ComparisonPath -Raw | ConvertFrom-Json
}

# Load current metrics
$currentMetrics = @()
$currentFiles = Get-ChildItem -Path $MetricsPath -Filter "*.json" -Recurse

foreach ($file in $currentFiles) {
    $content = Get-Content $file.FullName -Raw | ConvertFrom-Json
    $currentMetrics += $content
}

# Group by .NET version
$groupedByDotNet = $currentMetrics | Group-Object -Property dotnetVersion

$totalSize = ($currentMetrics | Measure-Object -Property packageSize -Sum).Sum
$avgBuildTime = ($currentMetrics | Measure-Object -Property buildTimeSeconds -Average).Average

$summary = @"
# 📊 .NET MAUI Template Size Tracking - $(Get-Date -Format "yyyy-MM-dd")

## Overall Statistics

- **Total Configurations**: $($currentMetrics.Count)
- **Total Package Size**: $([math]::Round($totalSize / 1GB, 2)) GB
- **Average Build Time**: $([math]::Round($avgBuildTime, 2)) seconds
- **MAUI Templates Version**: $(if ($currentMetrics.Count -gt 0) { $currentMetrics[0].mauiVersion } else { 'unknown' })

"@

foreach ($dotnetGroup in $groupedByDotNet) {
    $dotnetVersion = $dotnetGroup.Name
    $summary += @"

## .NET $dotnetVersion

| Template | Platform | Size | Compressed | Files | Assemblies | Build Time | Change | 1 day | 2 days | 3 days | 4 days | 5 days | 1 week | 1 month |
|----------|----------|------|------------|-------|------------|------------|--------|-------|--------|--------|--------|--------|--------|---------|

"@

    foreach ($metric in $dotnetGroup.Group | Sort-Object template, platform) {
        $metricKey = if ($metric.PSObject.Properties["description"]) { $metric.description } else { $metric.platform }

        $comparison = $comparisons | Where-Object {
            $_.dotnetVersion -eq $metric.dotnetVersion -and
            $_.template -eq $metric.template -and
            $_.platform -eq $metricKey
        } | Select-Object -First 1

        $sizeMB = [math]::Round($metric.packageSize / 1MB, 1)
        $compressedMB = [math]::Round($metric.compressedSize / 1MB, 1)
        if ($metric.PSObject.Properties["buildTimeFormatted"] -and $metric.buildTimeFormatted) {
            $buildTime = $metric.buildTimeFormatted
        } else {
            $buildTime = "$([math]::Round($metric.buildTimeSeconds / 60, 1))m"
        }

        $changeIndicator = ""
        if ($comparison -and $comparison.status -ne "new") {
            $changePercent = [math]::Round($comparison.sizeChangePercent, 1)

            if ($changePercent -gt 1) {
                $changeIndicator = "+$changePercent% 🔴"
            } elseif ($changePercent -gt 0) {
                $changeIndicator = "+$changePercent% 🟠"
            } elseif ($changePercent -lt -1) {
                $changeIndicator = "$changePercent% 🟢"
            } elseif ($changePercent -lt 0) {
                $changeIndicator = "$changePercent% 🟠"
            } else {
                $changeIndicator = "—"
            }
        } elseif ($comparison -and $comparison.status -eq "new") {
            $changeIndicator = "NEW ✨"
        } else {
            $changeIndicator = "—"
        }

        # Historical columns
        $historicalColumns = @()
        $daysToShow = @(1, 2, 3, 4, 5, 7, 30)

        foreach ($daysAgo in $daysToShow) {
            $histData = $null
            $keyName = "days_$daysAgo"

            if ($comparison -and $comparison.PSObject.Properties["historical"] -and $comparison.historical.PSObject.Properties[$keyName]) {
                $histData = $comparison.historical.$keyName
            }

            if ($histData -and $histData.compressedSize -gt 0) {
                $histSizeMB = [math]::Round($histData.compressedSize / 1MB, 1)
                $histPercent = [math]::Round($histData.percentChange, 1)

                $percentStr = if ($histPercent -gt 1) {
                    "+$histPercent% 🔴"
                } elseif ($histPercent -gt 0) {
                    "+$histPercent% 🟠"
                } elseif ($histPercent -lt -1) {
                    "$histPercent% 🟢"
                } elseif ($histPercent -lt 0) {
                    "$histPercent% 🟠"
                } else {
                    "0%"
                }

                $historicalColumns += "$histSizeMB MB ($percentStr)"
            } else {
                $historicalColumns += "—"
            }
        }

        $summary += "| $($metric.template) | $metricKey | $sizeMB MB | $compressedMB MB | $($metric.fileCount) | $($metric.assemblyCount) | $buildTime | $changeIndicator | $($historicalColumns -join ' | ') |`n"
    }
}

# Trend analysis
if ($comparisons.Count -gt 0) {
    $increased = ($comparisons | Where-Object { $_.sizeChangePercent -gt 0 }).Count
    $decreased = ($comparisons | Where-Object { $_.sizeChangePercent -lt 0 }).Count
    $unchanged = ($comparisons | Where-Object { $_.sizeChangePercent -eq 0 }).Count
    $new = ($comparisons | Where-Object { $_.status -eq "new" }).Count

    $summary += @"

## Trend Analysis

- 📈 Increased: $increased
- 📉 Decreased: $decreased
- ➡️ Unchanged: $unchanged
- ✨ New: $new

"@

    $topIncreases = $comparisons |
        Where-Object { $_.sizeChangePercent -gt 0 } |
        Sort-Object -Property sizeChangePercent -Descending |
        Select-Object -First 5

    if ($topIncreases.Count -gt 0) {
        $summary += @"

### Top Size Increases

| Template | Platform | .NET | Change |
|----------|----------|------|--------|

"@

        foreach ($increase in $topIncreases) {
            $summary += "| $($increase.template) | $($increase.platform) | $($increase.dotnetVersion) | +$($increase.sizeChangePercent)% |`n"
        }
    }

    $topDecreases = $comparisons |
        Where-Object { $_.sizeChangePercent -lt 0 } |
        Sort-Object -Property sizeChangePercent |
        Select-Object -First 5

    if ($topDecreases.Count -gt 0) {
        $summary += @"

### Top Size Decreases

| Template | Platform | .NET | Change |
|----------|----------|------|--------|

"@

        foreach ($decrease in $topDecreases) {
            $summary += "| $($decrease.template) | $($decrease.platform) | $($decrease.dotnetVersion) | $($decrease.sizeChangePercent)% |`n"
        }
    }
}

$summary += @"

## Links

- 🔗 [Workflow Run]($($env:GITHUB_SERVER_URL)/$($env:GITHUB_REPOSITORY)/actions/runs/$($env:GITHUB_RUN_ID))
- 📚 [Repository]($($env:GITHUB_SERVER_URL)/$($env:GITHUB_REPOSITORY))

---
*Generated at $(Get-Date -Format "yyyy-MM-dd HH:mm:ss") UTC*
"@

# Write to GitHub Actions summary
$summaryFile = $env:GITHUB_STEP_SUMMARY
if ($summaryFile) {
    $summary | Out-File -FilePath $summaryFile -Encoding UTF8 -Append
    Write-Host "Summary written to GitHub Actions" -ForegroundColor Green
} else {
    Write-Host $summary
}

$summary | Out-File -FilePath "summary-report.md" -Encoding UTF8
Write-Host "Summary saved to summary-report.md" -ForegroundColor Green
