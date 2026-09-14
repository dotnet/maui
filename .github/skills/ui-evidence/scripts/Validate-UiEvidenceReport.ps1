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
Set-StrictMode -Version Latest
$report = Get-Content -LiteralPath $ReportPath -Raw -Encoding UTF8
$summary = Get-Content -LiteralPath $AgentSummaryPath -Raw -Encoding UTF8 | ConvertFrom-Json
$policy = Get-Content -LiteralPath $PolicyPath -Raw -Encoding UTF8 | ConvertFrom-Json

if ($summary.schemaVersion -ne 1 -or $summary.overallVerdict -cnotin $policy.verdictPrecedence -or
    $summary.measuredHeadSha -cnotmatch '^[0-9a-f]{40}$' -or $summary.results -isnot [array]) {
    throw "UI evidence agent summary has an invalid report contract."
}
if ($report.Length -gt [int]$policy.maximumReportCharacters) {
    throw "UI evidence report exceeds the maximum length."
}
foreach ($section in @($policy.requiredSections)) {
    if (-not $report.Contains([string]$section, [StringComparison]::Ordinal)) {
        throw "UI evidence report is missing required section '$section'."
    }
}
if (-not $report.TrimEnd().EndsWith([string]$policy.requiredFooter, [StringComparison]::Ordinal)) {
    throw "UI evidence report must end with the required manual skill footer."
}
if (-not $report.Contains([string]$policy.requiredDisclaimer, [StringComparison]::Ordinal)) {
    throw "UI evidence report is missing the required advisory disclaimer."
}

$verdicts = [regex]::Matches($report, '(?m)^\*\*Empirical verdict:\*\* `([a-z-]+)`[ \t]*\r?$')
if ($verdicts.Count -ne 1 -or $verdicts[0].Groups[1].Value -cne $summary.overallVerdict -or
    [regex]::Matches($report, [regex]::Escape("**Empirical verdict:**")).Count -ne 1) {
    throw "UI evidence report does not preserve the deterministic empirical verdict."
}
if (-not $report.Contains([string]$summary.measuredHeadSha, [StringComparison]::OrdinalIgnoreCase)) {
    throw "UI evidence report does not identify the measured head SHA."
}
$rows = [regex]::Matches($report, '(?m)^[ \t]*\|[ \t]*`(?<key>maui-ui-[0-9a-f]{24})`[ \t]*\|[^\r\n]*')
if ($rows.Count -ne $summary.results.Count) {
    throw "UI evidence report must include one evidence row for every selected request, including missing bundles."
}
foreach ($result in $summary.results) {
    $matchingRows = @($rows | Where-Object { $_.Groups["key"].Value -ceq $result.requestKey })
    $cells = @($result.requestKey, $result.scenarioId, $result.platform, $result.verdict) | ForEach-Object {
        '[ \t]*`' + [regex]::Escape([string]$_) + '`[ \t]*\|'
    }
    if ($matchingRows.Count -ne 1 -or $matchingRows[0].Value -cnotmatch ('^[ \t]*\|' + ($cells -join ""))) {
        throw "UI evidence report does not preserve the scenario identity and verdict for '$($result.requestKey)'."
    }
}

$claimText = [Net.WebUtility]::HtmlDecode($report) -replace '<[^>]*>', '' -replace '[`*_]', ''
foreach ($pattern in @(
    '\b(safe[\s-]+(?:to|for)[\s-]+merg(?:e|ing)|approved[\s-]+for[\s-]+merge|ready[\s-]+(?:to|for)[\s-]+merge|merge[\s-]+(?:ready|safe|approved))\b',
    '\b(?:no|zero)[\s-]+(?:(?:ui|visual|functional|layout)[\s-]+)?regressions?\b',
    '\bregression[\s-]+free\b',
    '\b(clean verdict|empirically clean)\b',
    '\b(?:pr|pull request)[\s:-]+(?:(?:is|looks|seems|appears)[\s-]+)?(?:(?:completely|entirely)[\s-]+)?(?:clean|safe)\b',
    '\bclean[\s-]+(?:(?:whole|entire)[\s-]+)?(?:pr|pull request)\b',
    '\b(?:merge|approve)[\s-]+(?:this|the)[\s-]+(?:pr|pull request)\b'
)) {
    if ($claimText -match "(?i)$pattern") {
        throw "UI evidence report contains a prohibited clean or merge-safety claim."
    }
}

Write-Host "Local UI evidence report validation passed. Nothing was posted."
