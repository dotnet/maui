#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Finds regression risks by cross-referencing a PR's changes against recently-merged bug-fix PRs.

.DESCRIPTION
    For each implementation file in the PR diff, this script:
    1. Finds recent PRs that modified the same file (last 6 months)
    2. Checks if those PRs were bug fixes (via PR labels or linked issue labels)
    3. Compares lines removed by the current PR against lines added by recent fix PRs
    4. Reports revert risks where fix lines are being undone

.PARAMETER PRNumber
    The PR number to analyze.

.PARAMETER FilePaths
    Array of implementation file paths from the PR diff. If not provided, auto-detected from the PR.

.PARAMETER MonthsBack
    How many months back to search for fix PRs. Default: 6.

.EXAMPLE
    pwsh Find-RegressionRisks.ps1 -PRNumber 33908
    pwsh Find-RegressionRisks.ps1 -PRNumber 33908 -FilePaths @("src/Core/src/Platform/iOS/MauiView.cs")
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [string[]]$FilePaths,

    [Parameter(Mandatory = $false)]
    [int]$MonthsBack = 6
)

$ErrorActionPreference = 'Continue'
$repo = "dotnet/maui"

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Regression Cross-Reference — PR #$PRNumber" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Step 1: Get implementation files from PR diff if not provided
if (-not $FilePaths -or $FilePaths.Count -eq 0) {
    Write-Host "📂 Auto-detecting implementation files from PR #$PRNumber..." -ForegroundColor Yellow
    $prFiles = gh pr diff $PRNumber --name-only 2>$null
    if (-not $prFiles) {
        Write-Host "❌ Could not get PR diff. Make sure the PR branch is available." -ForegroundColor Red
        exit 1
    }
    $FilePaths = $prFiles | Where-Object {
        $_ -match '\.(cs|xaml)$' -and
        $_ -notmatch '(Tests|TestCases|tests|snapshots|samples)/' -and
        $_ -notmatch '\.Designer\.cs$'
    }
    Write-Host "  Found $($FilePaths.Count) implementation file(s)" -ForegroundColor Gray
}

if ($FilePaths.Count -eq 0) {
    Write-Host "🟢 No implementation files to check. Exiting." -ForegroundColor Green
    exit 0
}

# Step 2: Get lines REMOVED by the current PR for each file
Write-Host ""
Write-Host "📝 Getting current PR diff..." -ForegroundColor Yellow
$prDiffRaw = gh pr diff $PRNumber 2>$null
if (-not $prDiffRaw) {
    Write-Host "❌ Could not get PR diff." -ForegroundColor Red
    exit 1
}
if ($prDiffRaw -is [array]) {
    $prDiffRaw = $prDiffRaw -join "`n"
}

$currentFile = $null
$removedLinesByFile = @{}
foreach ($line in ($prDiffRaw -split "`n")) {
    if ($line -match '^diff --git a/(.*) b/(.*)$') {
        $currentFile = $Matches[2]
    }
    elseif ($line -match '^-(.+)$' -and $line -notmatch '^---' -and $currentFile) {
        $trimmed = $Matches[1].Trim()
        if ($trimmed.Length -gt 3) {
            if (-not $removedLinesByFile.ContainsKey($currentFile)) {
                $removedLinesByFile[$currentFile] = @()
            }
            $removedLinesByFile[$currentFile] += $trimmed
        }
    }
}

# Step 3: For each file, find recent PRs that modified it
$sinceDate = (Get-Date).AddMonths(-$MonthsBack).ToString("yyyy-MM-dd")
$risks = @()
$checkedPRs = @{}

foreach ($filePath in $FilePaths) {
    Write-Host ""
    Write-Host "🔍 Checking: $filePath" -ForegroundColor Cyan

    $commits = git log --oneline --since="$sinceDate" --all -- $filePath 2>$null
    if (-not $commits) {
        Write-Host "  🟢 No recent commits found for this file." -ForegroundColor Green
        continue
    }

    $prNumbers = @()
    foreach ($commitLine in ($commits -split "`n")) {
        if ($commitLine -match '\(#(\d+)\)') {
            $foundPR = [int]$Matches[1]
            if ($foundPR -ne $PRNumber -and -not $checkedPRs.ContainsKey($foundPR)) {
                $prNumbers += $foundPR
                $checkedPRs[$foundPR] = $true
            }
        }
    }

    if ($prNumbers.Count -eq 0) {
        Write-Host "  🟢 No recent PRs found for this file." -ForegroundColor Green
        continue
    }

    Write-Host "  Found $($prNumbers.Count) recent PR(s): $($prNumbers -join ', ')" -ForegroundColor Gray

    # Step 4: Check each recent PR
    foreach ($recentPR in $prNumbers) {
        Write-Host "  📋 Checking PR #$recentPR..." -ForegroundColor Gray -NoNewline

        $labelsRaw = gh pr view $recentPR --repo $repo --json labels --jq '.labels[].name' 2>$null
        if (-not $labelsRaw) {
            Write-Host " (could not fetch)" -ForegroundColor DarkGray
            continue
        }
        $labels = $labelsRaw -split "`n"

        $isBugFix = $false
        $matchedLabels = @()
        foreach ($label in $labels) {
            if ($label -match '^(i/regression|t/bug|p/0|p/1)$') {
                $isBugFix = $true
                $matchedLabels += $label
            }
        }

        # Get PR body and extract linked issues
        $prBody = gh pr view $recentPR --repo $repo --json body --jq '.body' 2>$null
        if ($prBody -is [array]) {
            $prBody = $prBody -join "`n"
        }

        $fixedIssues = @()
        if ($prBody) {
            $issueMatches = [regex]::Matches($prBody, '(?:Fixes|Closes|Resolves)\s+(?:https://github\.com/dotnet/maui/issues/)?#?(\d+)')
            foreach ($m in $issueMatches) {
                $fixedIssues += "#$($m.Groups[1].Value)"
            }
            # Bare "- #XXXXX" lines (Copilot agent PRs)
            $normalizedBody = $prBody -replace "`r`n", "`n"
            $bareIssueMatches = [regex]::Matches($normalizedBody, '(?m)^\s*-\s+#(\d+)\s*$')
            foreach ($m in $bareIssueMatches) {
                $ref = "#$($m.Groups[1].Value)"
                if ($fixedIssues -notcontains $ref) {
                    $fixedIssues += $ref
                }
            }
            $bareUrlMatches = [regex]::Matches($normalizedBody, '(?m)^\s*-\s+https://github\.com/dotnet/maui/issues/(\d+)\s*$')
            foreach ($m in $bareUrlMatches) {
                $ref = "#$($m.Groups[1].Value)"
                if ($fixedIssues -notcontains $ref) {
                    $fixedIssues += $ref
                }
            }
        }

        # Check linked issue labels if PR itself isn't labeled
        if (-not $isBugFix -and $fixedIssues.Count -gt 0) {
            foreach ($issueRef in $fixedIssues) {
                $issueNum = $issueRef -replace '#', ''
                $issueLabelsRaw = gh issue view $issueNum --repo $repo --json labels --jq '.labels[].name' 2>$null
                if ($issueLabelsRaw) {
                    foreach ($il in ($issueLabelsRaw -split "`n")) {
                        if ($il -match '^(i/regression|t/bug|p/0|p/1)$') {
                            $isBugFix = $true
                            $matchedLabels += "$il (from $issueRef)"
                        }
                    }
                }
            }
        }

        if (-not $isBugFix) {
            Write-Host " not a bug fix, skipping." -ForegroundColor DarkGray
            continue
        }

        Write-Host " bug fix [$($matchedLabels -join ', ')]" -ForegroundColor Yellow

        $fixedIssueStr = if ($fixedIssues.Count -gt 0) { $fixedIssues -join ', ' } else { '(unknown)' }
        $prTitle = gh pr view $recentPR --repo $repo --json title --jq '.title' 2>$null
        if (-not $prTitle) { $prTitle = "(unknown)" }

        # Step 5: Compare diffs
        $recentPRDiff = gh pr diff $recentPR --repo $repo 2>$null
        if ($recentPRDiff -is [array]) {
            $recentPRDiff = $recentPRDiff -join "`n"
        }
        if (-not $recentPRDiff) {
            $risks += [PSCustomObject]@{
                File = $filePath; RecentPR = $recentPR; PRTitle = $prTitle
                FixedIssues = $fixedIssueStr; Labels = $matchedLabels -join ', '
                Risk = "OVERLAP"; Details = "Diff unavailable for revert check"; RevertedLine = ""
            }
            continue
        }

        $inTargetFile = $false
        $addedByFix = @()
        foreach ($diffLine in ($recentPRDiff -split "`n")) {
            if ($diffLine -match '^diff --git a/(.*) b/(.*)$') {
                $inTargetFile = ($Matches[2] -eq $filePath)
            }
            elseif ($inTargetFile -and $diffLine -match '^\+(.+)$' -and $diffLine -notmatch '^\+\+\+') {
                $trimmed = $Matches[1].Trim()
                if ($trimmed.Length -gt 3) {
                    $addedByFix += $trimmed
                }
            }
        }

        if ($addedByFix.Count -eq 0) {
            Write-Host "    No significant lines added by this fix PR for this file." -ForegroundColor DarkGray
            continue
        }

        $removedLines = $removedLinesByFile[$filePath]
        if (-not $removedLines -or $removedLines.Count -eq 0) {
            $risks += [PSCustomObject]@{
                File = $filePath; RecentPR = $recentPR; PRTitle = $prTitle
                FixedIssues = $fixedIssueStr; Labels = $matchedLabels -join ', '
                Risk = "OVERLAP"; Details = "Same file, different lines"; RevertedLine = ""
            }
            continue
        }

        $revertedLines = @()
        foreach ($fixLine in $addedByFix) {
            foreach ($removedLine in $removedLines) {
                if (($fixLine -replace '\s+', ' ') -eq ($removedLine -replace '\s+', ' ')) {
                    $revertedLines += $fixLine
                    break
                }
            }
        }

        if ($revertedLines.Count -gt 0) {
            Write-Host "    🔴 REVERT DETECTED! $($revertedLines.Count) line(s) from fix PR #$recentPR being removed" -ForegroundColor Red
            foreach ($rl in $revertedLines) {
                Write-Host "      - $rl" -ForegroundColor Red
            }
            $risks += [PSCustomObject]@{
                File = $filePath; RecentPR = $recentPR; PRTitle = $prTitle
                FixedIssues = $fixedIssueStr; Labels = $matchedLabels -join ', '
                Risk = "REVERT"
                Details = "Removes $($revertedLines.Count) line(s) from fix PR #$recentPR"
                RevertedLine = ($revertedLines | Select-Object -First 3) -join ' | '
            }
        }
        else {
            $risks += [PSCustomObject]@{
                File = $filePath; RecentPR = $recentPR; PRTitle = $prTitle
                FixedIssues = $fixedIssueStr; Labels = $matchedLabels -join ', '
                Risk = "OVERLAP"; Details = "Same file, different lines"; RevertedLine = ""
            }
        }
    }
}

# Summary
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Results" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$reverts = $risks | Where-Object { $_.Risk -eq "REVERT" }
$overlaps = $risks | Where-Object { $_.Risk -eq "OVERLAP" }

if ($reverts.Count -gt 0) {
    Write-Host "🔴 REVERT RISKS DETECTED: $($reverts.Count)" -ForegroundColor Red
    Write-Host ""
    foreach ($r in $reverts) {
        Write-Host "  File: $($r.File)" -ForegroundColor Red
        Write-Host "  Fix PR: #$($r.RecentPR) — $($r.PRTitle)" -ForegroundColor Red
        Write-Host "  Fixed Issues: $($r.FixedIssues)" -ForegroundColor Red
        Write-Host "  Reverted: $($r.RevertedLine)" -ForegroundColor Red
        Write-Host ""
    }
    $allFixedIssues = ($reverts | ForEach-Object { $_.FixedIssues }) | Select-Object -Unique | Sort-Object
    Write-Host "⚠️  Verify that issues $($allFixedIssues -join ', ') do not re-regress." -ForegroundColor Yellow
}
elseif ($overlaps.Count -gt 0) {
    Write-Host "🟡 OVERLAPS FOUND: $($overlaps.Count) (lower risk)" -ForegroundColor Yellow
    foreach ($o in $overlaps) {
        Write-Host "  File: $($o.File) — PR #$($o.RecentPR) ($($o.FixedIssues))" -ForegroundColor Yellow
    }
}
else {
    Write-Host "🟢 No regression risks detected." -ForegroundColor Green
}

# Structured output for agent consumption
Write-Host ""
Write-Host "--- STRUCTURED OUTPUT ---"
@{
    pr_number    = $PRNumber
    has_reverts  = ($reverts.Count -gt 0)
    has_overlaps = ($overlaps.Count -gt 0)
    risks        = @($risks | ForEach-Object {
        @{
            file = $_.File; recent_pr = $_.RecentPR; pr_title = $_.PRTitle
            fixed_issues = $_.FixedIssues; labels = $_.Labels
            risk = $_.Risk; details = $_.Details; reverted_line = $_.RevertedLine
        }
    })
} | ConvertTo-Json -Depth 4

exit 0
