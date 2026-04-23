#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts UI test results summary on a PR after maui-pr-uitests pipeline completes.

.DESCRIPTION
    Maintains ONE comment per PR identified by <!-- UI Test Categories -->.
    Fetches test run results from AzDO, groups failures by error pattern,
    classifies them as snapshot/timeout/crash/assertion, and provides
    actionable information for maintainers.

    Each invocation adds an expandable session keyed by the PR HEAD SHA.
    - Same SHA  -> replaces that session in-place.
    - New SHA   -> prepends a new session (latest first; older collapsed).

.PARAMETER PRNumber
    The pull request number (required).

.PARAMETER BuildId
    The Azure DevOps build ID to summarise (required).

.PARAMETER Repo
    Repo in owner/name form. Defaults to dotnet/maui.

.PARAMETER AzdoOrg
    Azure DevOps org. Defaults to dnceng-public.

.PARAMETER AzdoProject
    Azure DevOps project. Defaults to public.

.PARAMETER DryRun
    Print the comment instead of posting.

.EXAMPLE
    ./post-uitest-categories-comment.ps1 -PRNumber 35015 -BuildId 1386834

.EXAMPLE
    ./post-uitest-categories-comment.ps1 -PRNumber 35015 -BuildId 1386834 -DryRun
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$PRNumber,

    [Parameter(Mandatory = $true)]
    [int]$BuildId,

    [Parameter(Mandatory = $false)]
    [string]$Repo = "dotnet/maui",

    [Parameter(Mandatory = $false)]
    [string]$AzdoOrg = "dnceng-public",

    [Parameter(Mandatory = $false)]
    [string]$AzdoProject = "public",

    [Parameter(Mandatory = $false)]
    [string]$OutputFile,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
$MARKER = "<!-- UI Test Categories -->"
$BuildUrl = "https://dev.azure.com/$AzdoOrg/$AzdoProject/_build/results?buildId=$BuildId"
$ApiBase = "https://dev.azure.com/$AzdoOrg/$AzdoProject/_apis/build/builds/$BuildId"

# ============================================================================
# HELPER: classify a test failure
# ============================================================================

function Get-FailureType {
    param([string]$ErrorMsg)
    if ([string]::IsNullOrWhiteSpace($ErrorMsg)) { return "unknown" }
    if ($ErrorMsg -match 'Snapshot different|VisualTestFailedException|difference\)') { return "snapshot" }
    if ($ErrorMsg -match 'Timeout|timed out|TimeoutException') { return "timeout" }
    if ($ErrorMsg -match 'NullReferenceException|ObjectDisposedException|crashed|SIGABRT|SIGSEGV') { return "crash" }
    if ($ErrorMsg -match 'Assert\.|Expected|actual|Is\.EqualTo|Is\.Not') { return "assertion" }
    return "other"
}

function Get-FailureIcon {
    param([string]$Type)
    switch ($Type) {
        "snapshot"  { return "🖼️" }
        "timeout"   { return "⏱️" }
        "crash"     { return "💥" }
        "assertion" { return "❌" }
        default     { return "❓" }
    }
}

function Format-RunName {
    param([string]$Name)
    # _ios_ui_tests_mono_controls_latest -> iOS Mono (latest)
    # _android_ui_tests_controls_30 -> Android (API 30)
    # _winui_ui_tests_controls -> WinUI
    # _mac_ui_tests_controls -> macOS
    # Material3_M3_android_ui_tests_controls_36 -> Material3 Android (API 36)
    # CarouselView_CARV1_ios_ui_tests_mono_controls_latest -> CarouselView iOS Mono (latest)
    $n = $Name -replace '^_', ''
    # Extract prefix (CarouselView, CollectionView, Material3, etc.)
    $prefix = ''
    if ($n -match '^(CarouselView|CollectionView|Material3)[_]') {
        $prefix = $Matches[1] + ' '
        $n = $n -replace '^[^_]+_', ''
    }
    # Strip intermediate noise
    $n = $n -replace 'CARV1_', '' -replace 'CV1_', '' -replace 'M3_', ''
    # Parse platform
    $platform = ''
    if ($n -match 'ios_ui_tests_mono') { $platform = 'iOS Mono' }
    elseif ($n -match 'android_ui_tests') { $platform = 'Android' }
    elseif ($n -match 'winui_ui_tests') { $platform = 'WinUI' }
    elseif ($n -match 'mac_ui_tests') { $platform = 'macOS' }
    else { return "$prefix$Name" }
    # Parse version suffix
    $version = ''
    if ($n -match '_(\d+_\d+)$') { $version = $Matches[1] -replace '_', '.' }
    elseif ($n -match '_(\d+)$') { $version = "API $($Matches[1])" }
    elseif ($n -match '_(latest)$') { $version = $Matches[1] }
    if ($version) { return "$prefix$platform ($version)" }
    return "$prefix$platform"
}

# ============================================================================
# FETCH BUILD + TIMELINE
# ============================================================================

Write-Host "Fetching build $BuildId..." -ForegroundColor Cyan
$build = Invoke-RestMethod -Uri "$ApiBase`?api-version=7.1"
$timeline = Invoke-RestMethod -Uri "$ApiBase/timeline?api-version=7.1"

# ----------------------------------------------------------------------------
# Detected categories (from "Check if category should run" logs)
# ----------------------------------------------------------------------------

$checkRecords = @($timeline.records |
    Where-Object { $_.name -eq "Check if category should run" -and $_.log -and $_.log.id })

$detectedCategories = $null
$filterEngaged = $false
$ranCount = 0
$totalCount = 0

if ($checkRecords.Count -gt 0) {
    foreach ($rec in $checkRecords) {
        $log = Invoke-RestMethod -Uri "$ApiBase/logs/$($rec.log.id)?api-version=7.1"
        if ($log -match "Detected Categories:\s*'([^']*)'\s*\(filter engaged:\s*(True|False)") {
            $val = $Matches[1]
            $eng = $Matches[2] -eq "True"
            if (-not $val.StartsWith('$(')) {
                $detectedCategories = $val
                $filterEngaged = $eng
                break
            }
        }
    }
    # Count ran vs skipped
    foreach ($rec in $checkRecords) {
        $totalCount++
        $log = Invoke-RestMethod -Uri "$ApiBase/logs/$($rec.log.id)?api-version=7.1"
        if ($log -match "Should run tests:\s*True") { $ranCount++ }
    }
}

if ([string]::IsNullOrWhiteSpace($detectedCategories)) {
    $detectedCategories = "all (full matrix)"
}
$noneDetected = $detectedCategories -eq 'NONE'
$skippedCount = $totalCount - $ranCount

Write-Host "Detected categories: $detectedCategories" -ForegroundColor Green
Write-Host "Filter engaged: $filterEngaged" -ForegroundColor Green

# ============================================================================
# FETCH TEST RESULTS (requires auth — try az CLI first, then AZURE_DEVOPS_EXT_PAT)
# ============================================================================

$allRuns = @()
$testApiHeaders = $null

# Try az CLI token first (works locally)
try {
    $azToken = (az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv 2>$null)
    if (-not [string]::IsNullOrWhiteSpace($azToken)) {
        $testApiHeaders = @{ Authorization = "Bearer $azToken" }
        Write-Host "Using az CLI token for test API" -ForegroundColor DarkGray
    }
} catch { }

# Fall back to AZURE_DEVOPS_EXT_PAT (available in CI)
if (-not $testApiHeaders) {
    $azdoPat = $env:AZURE_DEVOPS_EXT_PAT
    if (-not [string]::IsNullOrWhiteSpace($azdoPat) -and -not $azdoPat.StartsWith('$(') -and $azdoPat.Length -ge 40) {
        $base64 = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$azdoPat"))
        $testApiHeaders = @{ Authorization = "Basic $base64" }
        Write-Host "Using AZURE_DEVOPS_EXT_PAT for test API" -ForegroundColor DarkGray
    }
}

if ($testApiHeaders) {
    Write-Host "Fetching test results..." -ForegroundColor Cyan
    $buildUri = "vstfs:///Build/Build/$BuildId"
    try {
        $runsResp = Invoke-RestMethod -Headers $testApiHeaders -Uri "https://dev.azure.com/$AzdoOrg/$AzdoProject/_apis/test/runs?buildUri=$buildUri&api-version=7.1"
        foreach ($run in $runsResp.value) {
            if ($run.totalTests -eq 0) { continue }
            $failedTests = @()
            if ($run.passedTests -lt $run.totalTests) {
                try {
                    $failedResp = Invoke-RestMethod -Headers $testApiHeaders `
                        -Uri "https://dev.azure.com/$AzdoOrg/$AzdoProject/_apis/test/Runs/$($run.id)/results?outcomes=Failed&`$top=200&api-version=7.1"
                    $failedTests = @($failedResp.value)
                } catch { }
            }
            $allRuns += [pscustomobject]@{
                Name        = $run.name
                Total       = $run.totalTests
                Passed      = $run.passedTests
                FailedCount = $failedTests.Count
                FailedTests = $failedTests
            }
        }
    } catch {
        Write-Host "⚠️ Could not fetch test results: $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠️ No az token — test details unavailable (public org requires auth for test API)" -ForegroundColor Yellow
}

# ============================================================================
# CLASSIFY AND GROUP FAILURES
# ============================================================================

$totalTests = ($allRuns | Measure-Object -Property Total -Sum).Sum
$totalPassed = ($allRuns | Measure-Object -Property Passed -Sum).Sum
$totalFailed = ($allRuns | Measure-Object -Property FailedCount -Sum).Sum

$failedRuns = @($allRuns | Where-Object { $_.FailedCount -gt 0 })
$passedRuns = @($allRuns | Where-Object { $_.FailedCount -eq 0 -and $_.Total -gt 0 })

# Build a flat list of all failures with classification
$allFailures = @()
foreach ($run in $failedRuns) {
    # Parse platform from run name (e.g., _ios_ui_tests_mono_controls_latest -> iOS Mono latest)
    $platform = $run.Name -replace '^_', '' -replace '_ui_tests_', ' ' -replace '_controls_', ' ' -replace '_', ' '
    foreach ($t in $run.FailedTests) {
        $errRaw = if ($t.errorMessage) { $t.errorMessage } else { '' }
        $errOneLine = ($errRaw -replace '\r?\n', ' ' -replace '\s+', ' ').Trim()
        $ftype = Get-FailureType -ErrorMsg $errOneLine

        # Extract key detail based on type
        $detail = switch ($ftype) {
            "snapshot" {
                if ($errOneLine -match '(\d+\.\d+)%\s*difference') { "$($Matches[1])% diff" }
                else { "snapshot mismatch" }
            }
            "timeout" { "timed out" }
            "crash" {
                if ($errOneLine -match '(NullReferenceException|ObjectDisposedException|SIGABRT|SIGSEGV)') { $Matches[1] }
                else { "crash" }
            }
            "assertion" {
                if ($errOneLine.Length -gt 120) { $errOneLine.Substring(0, 120) + "..." }
                else { $errOneLine }
            }
            default {
                if ($errOneLine.Length -gt 120) { $errOneLine.Substring(0, 120) + "..." }
                else { $errOneLine }
            }
        }

        $allFailures += [pscustomobject]@{
            TestName = $t.testCase.name
            RunName  = $run.Name
            Platform = $platform
            Type     = $ftype
            Detail   = $detail
            Error    = if ($errOneLine.Length -gt 300) { $errOneLine.Substring(0, 300) + "..." } else { $errOneLine }
        }
    }
}

# Group by failure type
$failuresByType = $allFailures | Group-Object -Property Type | Sort-Object Count -Descending

Write-Host "Total: $totalTests tests, $totalPassed passed, $totalFailed failed across $($allRuns.Count) runs" -ForegroundColor $(if ($totalFailed -gt 0) { "Yellow" } else { "Green" })

# ============================================================================
# STAGE RESULTS (only failed/issues stages)
# ============================================================================

$stages = @($timeline.records | Where-Object { $_.type -eq "Stage" } | Sort-Object name -Unique)
$failedStages = @($stages | Where-Object { $_.result -eq "failed" -or $_.result -eq "canceled" })
$passedStages = @($stages | Where-Object { $_.result -eq "succeeded" -or $_.result -eq "succeededWithIssues" })

# ============================================================================
# FETCH PR METADATA
# ============================================================================

try {
    $commitJson = gh api "repos/$Repo/pulls/$PRNumber/commits" --jq '.[-1] | {message: .commit.message, sha: .sha}' 2>$null | ConvertFrom-Json
} catch {
    Write-Host "⚠️ Could not fetch commits: $_" -ForegroundColor Yellow
    $commitJson = $null
}
$commitTitle = if ($commitJson) { ($commitJson.message -split "`n")[0] } else { "Unknown" }
$commitTitle = $commitTitle -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;'
$commitSha7  = if ($commitJson) { $commitJson.sha.Substring(0, 7) } else { "unknown" }
$commitFull  = if ($commitJson) { $commitJson.sha } else { "" }
$commitUrl   = if ($commitJson) { "https://github.com/$Repo/commit/$commitFull" } else { "#" }

try { $prAuthor = gh api "repos/$Repo/pulls/$PRNumber" --jq '.user.login' 2>$null } catch { $prAuthor = $null }

$timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd HH:mm UTC")

# ============================================================================
# BUILD COMMENT
# ============================================================================

$buildBadge = switch ($build.result) {
    "succeeded"           { "✅ passed" }
    "succeededWithIssues" { "⚠️ passed with issues" }
    "failed"              { "❌ failed" }
    "canceled"            { "🚫 canceled" }
    default               { "🔄 $($build.status)" }
}

$passRate = if ($totalTests -gt 0) { [math]::Round(($totalPassed / $totalTests) * 100, 1) } else { 0 }

# Header line with key stats
if ($noneDetected -and $totalTests -eq 0) {
    $headerLine = "**$buildBadge** | No UI test categories detected"
} else {
    $headerLine = "**$buildBadge** | $totalPassed/$totalTests passed ($passRate%)"
    if ($totalFailed -gt 0) { $headerLine += " | **$totalFailed failed**" }
}

# Filter info line
$filterLine = if ($noneDetected) {
    "⏭️ No UI test categories detected — skipped $skippedCount of $totalCount matrix cells"
} elseif ($filterEngaged) {
    "🎯 **Detected categories:** ``$detectedCategories`` — ran $ranCount of $totalCount matrix cells (skipped $skippedCount)"
} else {
    "📦 **Full matrix** — all $totalCount matrix cells ran"
}

# --- Failed tests section (the main content) ---
$failedSection = ""
if ($totalFailed -gt 0) {
    $parts = @()

    # Platform summary table
    $parts += "### Results by Platform"
    $parts += ""
    $parts += "| Platform | Passed | Failed | Total |"
    $parts += "|---|---|---|---|"
    foreach ($run in ($allRuns | Sort-Object Name)) {
        $displayName = Format-RunName -Name $run.Name
        $icon = if ($run.FailedCount -eq 0) { "✅" } else { "❌" }
        $parts += "| $icon $displayName | $($run.Passed) | $($run.FailedCount) | $($run.Total) |"
    }
    $parts += ""

    # Failure breakdown by type
    $parts += "### Failures ($totalFailed)"
    $parts += ""
    $typeSummary = @($failuresByType | ForEach-Object {
        $icon = Get-FailureIcon -Type $_.Name
        "$icon **$($_.Name)**: $($_.Count)"
    })
    $parts += ($typeSummary -join " | ")
    $parts += ""

    # Per-run breakdown — cap at 15 runs and 25 tests per run to stay under GitHub comment limit
    $maxRuns = 15
    $maxTestsPerRun = 25
    $sortedFailedRuns = @($failedRuns | Sort-Object FailedCount -Descending)
    $shownRuns = @($sortedFailedRuns | Select-Object -First $maxRuns)
    $hiddenRunCount = $sortedFailedRuns.Count - $shownRuns.Count

    foreach ($run in $shownRuns) {
        $displayName = Format-RunName -Name $run.Name
        $passedPct = if ($run.Total -gt 0) { [math]::Round(($run.Passed / $run.Total) * 100, 0) } else { 0 }

        $parts += "<details>"
        $parts += "<summary>❌ <strong>$displayName</strong> — $($run.FailedCount) failed, $($run.Passed)/$($run.Total) passed ($passedPct%)</summary>"
        $parts += "<br>"
        $parts += ""
        $parts += "| | Test | Detail |"
        $parts += "|---|---|---|"

        $runFailures = @($allFailures | Where-Object { $_.RunName -eq $run.Name } | Sort-Object Type, TestName)
        $shownTests = @($runFailures | Select-Object -First $maxTestsPerRun)
        $hiddenTestCount = $runFailures.Count - $shownTests.Count
        foreach ($f in $shownTests) {
            $icon = Get-FailureIcon -Type $f.Type
            $name = $f.TestName -replace '\|', '\|'
            $det = $f.Detail -replace '\|', '\|' -replace '`', "'"
            if ($det.Length -gt 150) { $det = $det.Substring(0, 150) + "..." }
            $parts += "| $icon | ``$name`` | $det |"
        }
        if ($hiddenTestCount -gt 0) {
            $parts += "| | _...and $hiddenTestCount more_ | |"
        }
        $parts += ""
        $parts += "</details>"
        $parts += ""
    }
    if ($hiddenRunCount -gt 0) {
        $parts += "_...and $hiddenRunCount more runs with failures (see [build]($BuildUrl))_"
        $parts += ""
    }

    $failedSection = $parts -join "`n"
} elseif (-not $noneDetected -and $totalTests -gt 0) {
    $parts = @()
    $parts += "### Results by Platform"
    $parts += ""
    $parts += "| Platform | Passed | Total |"
    $parts += "|---|---|---|"
    foreach ($run in ($allRuns | Sort-Object Name)) {
        $displayName = Format-RunName -Name $run.Name
        $parts += "| ✅ $displayName | $($run.Passed) | $($run.Total) |"
    }
    $parts += ""
    $failedSection = $parts -join "`n"
}

# --- Failed stages (compact) ---
$stageSection = ""
if ($failedStages.Count -gt 0) {
    $stageLines = @()
    $stageLines += "<details>"
    $stageLines += "<summary>🔴 <strong>Failed stages ($($failedStages.Count))</strong> of $($stages.Count) total</summary>"
    $stageLines += "<br>"
    $stageLines += ""
    foreach ($s in $failedStages) {
        $stageLines += "- ❌ $($s.name)"
    }
    $stageLines += ""
    $stageLines += "</details>"
    $stageSection = $stageLines -join "`n"
}

$sessionStart = "<!-- SESSION:$commitSha7 START -->"
$sessionEnd   = "<!-- SESSION:$commitSha7 END -->"

$sessionBody = @"
$sessionStart
<details open>
<summary>🧪 <a href="$commitUrl"><code>$commitSha7</code></a> · $commitTitle · <em>$timestamp</em></summary>
<br>

[Build #$BuildId]($BuildUrl) | $headerLine

$filterLine

$failedSection

$stageSection

</details>
$sessionEnd
"@

# ============================================================================
# OUTPUT: either write to file (-OutputFile) or post as PR comment
# ============================================================================

if ($OutputFile) {
    # Write content for embedding in AI summary (no session markers, no comment wrapper)
    $contentBody = @"
[Build #$BuildId]($BuildUrl) | $headerLine

$filterLine

$failedSection

$stageSection
"@
    $contentBody = $contentBody -replace "`n{4,}", "`n`n`n"
    $outputDir = Split-Path $OutputFile -Parent
    if ($outputDir -and -not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
    }
    $contentBody | Set-Content -Path $OutputFile -Encoding UTF8
    Write-Host "✅ Written to $OutputFile" -ForegroundColor Green
    return
}

# ============================================================================
# POST AS SEPARATE PR COMMENT (standalone mode)
# ============================================================================

function Merge-Sessions {
    param([string]$ExistingBody, [string]$NewSession, [string]$Sha7)

    $pattern = '(?s)<!-- SESSION:([a-f0-9]+) START -->.*?<!-- SESSION:\1 END -->'
    $matches = [regex]::Matches($ExistingBody, $pattern)

    $sessions = [ordered]@{}
    foreach ($m in $matches) { $sessions[$m.Groups[1].Value] = $m.Value }
    $sessions[$Sha7] = $NewSession

    $orderedKeys = @($Sha7) + @($sessions.Keys | Where-Object { $_ -ne $Sha7 })
    $blocks = @()
    $first = $true
    foreach ($k in $orderedKeys) {
        $b = $sessions[$k]
        # Ensure <br> after every </summary> (normalize old sessions)
        $b = $b -replace '</summary>\s*(?!<br>)', "</summary>`n<br>"
        if ($first) {
            $b = $b -replace '<details(?:\s+open)?>', '<details open>'
            $first = $false
        } else {
            $b = $b -replace '<details\s+open>', '<details>'
        }
        $blocks += $b
    }
    return ($blocks -join "`n`n---`n`n")
}

Write-Host "Looking for existing comment on $Repo#$PRNumber..." -ForegroundColor Cyan
$existingId = $null
$existingBody = $null

$existingRaw = gh api "repos/$Repo/issues/$PRNumber/comments" --paginate 2>$null
if ($existingRaw) {
    try {
        $all = $existingRaw | ConvertFrom-Json
        $existing = @($all | Where-Object { $_.body -and $_.body.Contains($MARKER) }) | Select-Object -Last 1
        if ($existing) {
            $existingId = $existing.id
            $existingBody = $existing.body
            Write-Host "Found existing comment (ID: $existingId)" -ForegroundColor Green
        }
    } catch {
        Write-Host "⚠️ Could not parse comments: $_" -ForegroundColor Yellow
    }
}

if ($existingBody) {
    $merged = Merge-Sessions -ExistingBody $existingBody -NewSession $sessionBody -Sha7 $commitSha7
    $commentBody = @"
$MARKER

## 🧪 UI Test Results

$merged
"@
} else {
    $commentBody = @"
$MARKER

## 🧪 UI Test Results

$sessionBody
"@
}

$commentBody = $commentBody -replace "`n{4,}", "`n`n`n"

if ($DryRun) {
    Write-Host ""
    Write-Host "=== COMMENT PREVIEW ===" -ForegroundColor Cyan
    Write-Host $commentBody
    Write-Host "=== END PREVIEW ===" -ForegroundColor Cyan
    exit 0
}

$tempFile = [System.IO.Path]::GetTempFileName()
try {
    @{ body = $commentBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8
    if ($existingId) {
        Write-Host "Updating comment $existingId..." -ForegroundColor Yellow
        gh api --method PATCH "repos/$Repo/issues/comments/$existingId" --input $tempFile | Out-Null
        Write-Host "✅ Updated" -ForegroundColor Green
        Write-Output "COMMENT_ID=$existingId"
    } else {
        Write-Host "Creating new comment..." -ForegroundColor Yellow
        $resp = gh api --method POST "repos/$Repo/issues/$PRNumber/comments" --input $tempFile | ConvertFrom-Json
        Write-Host "✅ Posted (ID: $($resp.id))" -ForegroundColor Green
        Write-Output "COMMENT_ID=$($resp.id)"
    }
} finally {
    Remove-Item $tempFile -ErrorAction SilentlyContinue
}
