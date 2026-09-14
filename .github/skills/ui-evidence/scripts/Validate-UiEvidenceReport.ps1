#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$ReportPath,

    [Parameter(Mandatory = $true)]
    [string]$AgentSummaryPath,

    [Parameter(Mandatory = $false)]
    [string]$PolicyPath = (Join-Path $PSScriptRoot "..\references\report-policy.json")
)

$ErrorActionPreference = "Stop"
$report = Get-Content -LiteralPath $ReportPath -Raw -Encoding UTF8
$summary = Get-Content -LiteralPath $AgentSummaryPath -Raw -Encoding UTF8 | ConvertFrom-Json
$policy = Get-Content -LiteralPath $PolicyPath -Raw -Encoding UTF8 | ConvertFrom-Json

if ($report.Length -gt [int]$policy.maximumReportCharacters) {
    throw "UI evidence report exceeds the maximum length."
}
foreach ($section in @($policy.requiredSections)) {
    if (-not $report.Contains([string]$section, [StringComparison]::Ordinal)) {
        throw "UI evidence report is missing required section '$section'."
    }
}
if (-not $report.Contains([string]$policy.requiredFooter, [StringComparison]::Ordinal)) {
    throw "UI evidence report is missing the required workflow footer."
}
if (-not $report.Contains([string]$policy.requiredDisclaimer, [StringComparison]::Ordinal)) {
    throw "UI evidence report is missing the required advisory disclaimer."
}

$requiredVerdict = "**Empirical verdict:** ``$($summary.overallVerdict)``"
if (-not $report.Contains($requiredVerdict, [StringComparison]::Ordinal)) {
    throw "UI evidence report does not preserve the deterministic empirical verdict."
}
if (-not $report.Contains([string]$summary.measuredHeadSha, [StringComparison]::OrdinalIgnoreCase)) {
    throw "UI evidence report does not identify the measured head SHA."
}
if ($report -match '(?i)\b(safe to merge|approved for merge|no regressions?|clean verdict|empirically clean)\b') {
    throw "UI evidence report contains a prohibited clean or merge-safety claim."
}

Write-Host "UI evidence report validation passed."
