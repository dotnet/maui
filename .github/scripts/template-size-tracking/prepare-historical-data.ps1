#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Prepares historical metrics data for comparison and trend analysis.

.PARAMETER MetricsPath
    Path to current metrics directory.

.PARAMETER HistoryDir
    Path to the historical metrics directory (restored from cache).

.PARAMETER RetentionDays
    Number of days of history to retain.
#>

param(
    [string]$MetricsPath = "metrics",
    [string]$HistoryDir = "metrics-history",
    [int]$RetentionDays = 35
)

$ErrorActionPreference = "Stop"

$today = (Get-Date).ToString("yyyy-MM-dd")

if (-not (Test-Path $HistoryDir)) {
    New-Item -ItemType Directory -Force -Path $HistoryDir | Out-Null
    Write-Host "Created new metrics history directory"
}

# Copy current metrics into today's history slot
$todayDir = Join-Path $HistoryDir $today
New-Item -ItemType Directory -Force -Path $todayDir | Out-Null

$metricsFiles = Get-ChildItem -Path $MetricsPath -Filter "*.json" -Recurse
foreach ($file in $metricsFiles) {
    Copy-Item $file.FullName -Destination $todayDir -Force
}
Write-Host "Saved $($metricsFiles.Count) metrics to $todayDir"

# Find yesterday's metrics for comparison
$yesterday = (Get-Date).AddDays(-1).ToString("yyyy-MM-dd")
$yesterdayDir = Join-Path $HistoryDir $yesterday

if (Test-Path $yesterdayDir) {
    Write-Host "Found previous metrics from $yesterday"
    Copy-Item $yesterdayDir -Destination "previous-metrics" -Recurse
} else {
    Write-Host "No previous metrics found for $yesterday"
    New-Item -ItemType Directory -Force -Path "previous-metrics" | Out-Null
}

# Prune history older than retention period
$cutoffDate = (Get-Date).AddDays(-$RetentionDays).ToString("yyyy-MM-dd")
$historyDirs = Get-ChildItem -Path $HistoryDir -Directory | Sort-Object Name

foreach ($dir in $historyDirs) {
    if ($dir.Name -lt $cutoffDate) {
        Write-Host "Pruning old metrics: $($dir.Name)"
        Remove-Item $dir.FullName -Recurse -Force
    }
}

$remaining = (Get-ChildItem -Path $HistoryDir -Directory).Count
Write-Host "History contains $remaining day(s) of metrics"
