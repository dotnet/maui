#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Detects regression risks by cross-referencing a PR's deletions against lines added by recent bug-fix PRs.

.DESCRIPTION
    Purely mechanical (no AI / LLM). For each implementation file in the PR diff:
      1. Collects lines REMOVED by the PR being reviewed.
      2. Uses `git log` to find PRs that touched the same file in the last N months.
      3. Filters those to bug-fix PRs (label match: i/regression, t/bug, p/0, p/1; or
         linked-issue label match).
      4. Pulls each fix PR's diff and collects lines it ADDED to that same file.
      5. Compares (whitespace-insensitive). If a removed line equals a line a fix PR
         added → 🔴 REVERT. Same file but no line match → 🟡 OVERLAP. Otherwise → 🟢 CLEAN.

    Outputs (when -OutputDir is provided):
      - content.md       Markdown summary suitable for the wall-of-text PR review.
      - risks.json       Structured findings for downstream agents.
      - result.txt       One token: CLEAN | OVERLAP | REVERT (used by Review-PR.ps1
                         for branching).
      - inline-findings.json (only when -WriteInlineFindings is set and reverts found)

.PARAMETER PRNumber
    The PR number being analyzed.

.PARAMETER Repo
    Repository in `owner/name` form. Defaults to dotnet/maui.

.PARAMETER FilePaths
    Optional list of files to analyze. If omitted, auto-detected from `gh pr diff`.

.PARAMETER MonthsBack
    How many months of history to scan for fix PRs. Default 6.

.PARAMETER MaxRecentPRsPerFile
    Cap on how many recent PRs to inspect per file (rate-limit guard). Default 20.

.PARAMETER OutputDir
    Directory to write content.md, risks.json, result.txt. If omitted, only console output.

.PARAMETER WriteInlineFindings
    When set, append entries to inline-findings.json at the file:line where reverted code
    was deleted. Off by default until accuracy is validated.

.EXAMPLE
    pwsh .github/scripts/Find-RegressionRisks.ps1 -PRNumber 33908

.EXAMPLE
    pwsh .github/scripts/Find-RegressionRisks.ps1 -PRNumber 33908 `
        -OutputDir "CustomAgentLogsTmp/PRState/33908/PRAgent/regression-check"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [string]$Repo = "dotnet/maui",

    [Parameter(Mandatory = $false)]
    [string[]]$FilePaths,

    [Parameter(Mandatory = $false)]
    [int]$MonthsBack = 6,

    [Parameter(Mandatory = $false)]
    [int]$MaxRecentPRsPerFile = 20,

    [Parameter(Mandatory = $false)]
    [string]$BaseBranch = 'main',

    [Parameter(Mandatory = $false)]
    [string]$OutputDir,

    [Parameter(Mandatory = $false)]
    [switch]$WriteInlineFindings
)

$ErrorActionPreference = 'Continue'

# ─── Helpers ──────────────────────────────────────────────────────────────────

function Write-Banner {
    param([string]$Title)
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
}

function ConvertTo-NormalizedLine {
    # Whitespace-insensitive comparison key. Collapses runs of whitespace to a single space
    # so an indent change alone won't trigger a false REVERT.
    param([string]$Line)
    return ($Line -replace '\s+', ' ').Trim()
}

function Test-IsImplementationFile {
    param([string]$Path)
    if ($Path -notmatch '\.(cs|xaml)$') { return $false }
    if ($Path -match '(?i)(Tests|TestCases|tests|snapshots|samples)/') { return $false }
    if ($Path -match '\.Designer\.cs$') { return $false }
    if ($Path -match '\.g\.cs$') { return $false }
    return $true
}

function Test-IsTestFile {
    param([string]$Path)
    if ($Path -notmatch '\.cs$') { return $false }
    if ($Path -match '(?i)(Tests|TestCases)/') { return $true }
    return $false
}

function Get-PRDiffText {
    param(
        [int]$Number,
        [string]$Repo
    )
    $raw = gh pr diff $Number --repo $Repo 2>$null
    if (-not $raw) { return $null }
    if ($raw -is [array]) { $raw = $raw -join "`n" }
    return $raw
}

function Get-DiffLinesByFile {
    <#
    Parses a unified diff. Returns a hashtable:
      { filePath -> [PSCustomObject]@{ Sign = '+' | '-'; Text = '...'; Line = <new-side line number for +, old-side for -> } }
    Line numbers are tracked from hunk headers so we can post inline findings.
    #>
    param(
        [string]$DiffText
    )
    $byFile = @{}
    $currentFile = $null
    $newLineCursor = 0
    $oldLineCursor = 0

    foreach ($rawLine in ($DiffText -split "`n")) {
        # Strip trailing CR (Windows-style line endings can survive in diff output)
        $line = $rawLine.TrimEnd("`r")

        if ($line -match '^diff --git a/(.*) b/(.*)$') {
            $currentFile = $Matches[2]
            if (-not $byFile.ContainsKey($currentFile)) {
                $byFile[$currentFile] = [System.Collections.Generic.List[object]]::new()
            }
            continue
        }
        if (-not $currentFile) { continue }

        if ($line -match '^@@ -(\d+)(?:,\d+)? \+(\d+)(?:,\d+)? @@') {
            $oldLineCursor = [int]$Matches[1]
            $newLineCursor = [int]$Matches[2]
            continue
        }

        # Skip diff metadata lines
        if ($line -match '^(---|\+\+\+|index |new file|deleted file|similarity|rename|Binary)') { continue }

        # "\ No newline at end of file" marker — explicitly skip without advancing cursors
        if ($line -match '^\\ No newline at end of file') { continue }

        if ($line.Length -eq 0) {
            # Empty diff line outside a hunk — ignore (cursors only matter inside hunks)
            continue
        }

        $sign = $line.Substring(0, 1)
        $text = if ($line.Length -gt 1) { $line.Substring(1) } else { '' }

        switch ($sign) {
            '+' {
                $byFile[$currentFile].Add([PSCustomObject]@{
                    Sign = '+'; Text = $text; Line = $newLineCursor
                })
                $newLineCursor++
            }
            '-' {
                $byFile[$currentFile].Add([PSCustomObject]@{
                    Sign = '-'; Text = $text; Line = $oldLineCursor
                })
                $oldLineCursor++
            }
            ' ' {
                $oldLineCursor++
                $newLineCursor++
            }
            default {
                # Unknown line — don't advance cursors
            }
        }
    }
    return $byFile
}

function Test-IsTrivialLine {
    # Filters out lines that produce meaningless matches (control-flow keywords alone,
    # punctuation, single-token braces). A line must contain a substantive identifier
    # or expression to be a useful match key.
    param([string]$NormalizedText)

    if ([string]::IsNullOrWhiteSpace($NormalizedText)) { return $true }
    if ($NormalizedText.Length -le 4) { return $true }

    # Punctuation/brace-only lines
    if ($NormalizedText -match '^[\s\{\}\(\)\[\];,:]+$') { return $true }

    # Pure control-flow / scope keywords with optional terminator
    if ($NormalizedText -match '^(return|break|continue|throw|else|try|finally|do|true|false|null);?\s*$') { return $true }

    # `using xyz;` and `namespace xyz` are very common — not interesting unless they
    # appear next to surrounding context which we don't compare here. Skip.
    if ($NormalizedText -match '^(using|namespace)\s+[\w\.]+;?\s*$') { return $true }

    # Comment-only lines
    if ($NormalizedText -match '^(//|/\*|\*|#)') { return $true }

    return $false
}

function Test-IsBugFixLabel {
    param([string]$Label)
    # Only definitive bug-fix labels. p/0 and p/1 are priority labels that also
    # apply to enhancements — they're used as secondary signal in Get-PRMetadataIfBugFix
    # (AND-ed with linked-issue bug labels) but not as standalone classifiers.
    return $Label -match '^(i/regression|t/bug)$'
}

function Get-LinkedIssueNumbers {
    param([string]$PRBody)
    if (-not $PRBody) { return @() }
    if ($PRBody -is [array]) { $PRBody = $PRBody -join "`n" }
    $normalized = $PRBody -replace "`r`n", "`n"
    $set = New-Object 'System.Collections.Generic.HashSet[int]'

    $patterns = @(
        '(?i)(?:Fix(?:es|ed)?|Close[sd]?|Resolve[sd]?)\s+(?:https://github\.com/dotnet/maui/issues/)?#?(\d+)',
        '(?m)^\s*-\s+#(\d+)\s*$',
        '(?m)^\s*-\s+https://github\.com/dotnet/maui/issues/(\d+)\s*$'
    )
    foreach ($pat in $patterns) {
        foreach ($m in [regex]::Matches($normalized, $pat)) {
            [void]$set.Add([int]$m.Groups[1].Value)
        }
    }
    return @($set)
}

function Get-PRMetadataIfBugFix {
    param([int]$Number, [string]$Repo)

    # Single gh call for labels + title + body + merge commit (was 3 separate calls before).
    $json = gh pr view $Number --repo $Repo --json labels,title,body,mergeCommit 2>$null
    if (-not $json) { return $null }
    if ($json -is [array]) { $json = $json -join "`n" }

    try {
        $data = $json | ConvertFrom-Json
    } catch {
        return $null
    }

    $labelNames = @()
    if ($data.labels) {
        $labelNames = @($data.labels | ForEach-Object { $_.name } | Where-Object { $_ })
    }

    $matched = @($labelNames | Where-Object { Test-IsBugFixLabel $_ })
    $title = if ($data.title) { $data.title } else { '(unknown)' }
    $linkedIssues = Get-LinkedIssueNumbers $data.body

    # Secondary signal: high-priority labels (p/0, p/1) combined with
    # linked-issue bug labels suggest a bug-fix even when the PR itself
    # lacks t/bug or i/regression.
    $hasPriorityLabel = @($labelNames | Where-Object { $_ -match '^(p/0|p/1)$' }).Count -gt 0

    # Fall back to linked-issue labels (the PR itself may not be labeled even though
    # it fixes a bug — common for fork PRs where labels weren't applied at merge).
    if ($matched.Count -eq 0 -and $linkedIssues.Count -gt 0) {
        foreach ($issueNum in $linkedIssues) {
            $issueLabelsRaw = gh issue view $issueNum --repo $Repo --json labels --jq '.labels[].name' 2>$null
            if (-not $issueLabelsRaw) { continue }
            foreach ($il in ($issueLabelsRaw -split "`n")) {
                if (Test-IsBugFixLabel $il) {
                    $matched += "$il (from #$issueNum)"
                }
            }
        }
    }

    # p/0 and p/1 only count as bug-fix signals when combined with a
    # definitive bug label from the PR or its linked issues.
    if ($matched.Count -gt 0 -and $hasPriorityLabel) {
        $matched += @($labelNames | Where-Object { $_ -match '^(p/0|p/1)$' })
    }

    if ($matched.Count -eq 0) { return $null }

    $mergeOid = $null
    if ($data.mergeCommit -and $data.mergeCommit.oid) {
        $mergeOid = $data.mergeCommit.oid
    }

    return [PSCustomObject]@{
        Number       = $Number
        Title        = $title
        Labels       = $matched
        LinkedIssues = $linkedIssues
        MergeCommit  = $mergeOid
    }
}

# ─── Main ─────────────────────────────────────────────────────────────────────

# Validate gh authentication before making any API calls.
# Silent auth failures would cause every PR lookup to return empty,
# producing a false CLEAN result for risky PRs.
$authCheck = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ GitHub CLI not authenticated. Cannot reliably analyze regression risks." -ForegroundColor Red
    Write-Host "   Run 'gh auth login' or set GH_TOKEN. Auth output:" -ForegroundColor Red
    Write-Host "   $authCheck" -ForegroundColor Gray
    exit 2
}

Write-Banner "Regression Cross-Reference — PR #$PRNumber"

# Resolve files
if (-not $FilePaths -or $FilePaths.Count -eq 0) {
    Write-Host "📂 Auto-detecting implementation files from PR #$PRNumber…" -ForegroundColor Yellow
    $prFiles = gh pr diff $PRNumber --repo $Repo --name-only 2>$null
    if (-not $prFiles) {
        Write-Host "❌ Could not get PR diff. Make sure gh is authenticated." -ForegroundColor Red
        exit 2
    }
    $FilePaths = @($prFiles | Where-Object { Test-IsImplementationFile $_ })
    Write-Host "  Found $($FilePaths.Count) implementation file(s)" -ForegroundColor Gray
}

if ($FilePaths.Count -eq 0) {
    Write-Host "🟢 No implementation files to check." -ForegroundColor Green
    if ($OutputDir) {
        New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
        "🟢 No implementation files modified — skipping regression cross-reference." |
            Set-Content (Join-Path $OutputDir "content.md") -Encoding UTF8
        '{ "pr_number": ' + $PRNumber + ', "result": "CLEAN", "risks": [] }' |
            Set-Content (Join-Path $OutputDir "risks.json") -Encoding UTF8
        "CLEAN" | Set-Content (Join-Path $OutputDir "result.txt") -Encoding UTF8
    }
    exit 0
}

# Step 1: PR diff (lines removed)
Write-Host ""
Write-Host "📝 Reading current PR diff…" -ForegroundColor Yellow
$prDiff = Get-PRDiffText -Number $PRNumber -Repo $Repo
if (-not $prDiff) {
    Write-Host "❌ Empty PR diff." -ForegroundColor Red
    exit 2
}
$prDiffByFile = Get-DiffLinesByFile -DiffText $prDiff

# Per-file: removed lines (non-trivial) AND added lines (for move-suppression).
$removedByFile = @{}
$addedNormByFile = @{}
foreach ($file in $prDiffByFile.Keys) {
    $removed = @($prDiffByFile[$file] | Where-Object {
        $_.Sign -eq '-' -and -not (Test-IsTrivialLine (ConvertTo-NormalizedLine $_.Text))
    })
    if ($removed.Count -gt 0) {
        $removedByFile[$file] = $removed
    }

    $added = $prDiffByFile[$file] | Where-Object { $_.Sign -eq '+' } |
        ForEach-Object { ConvertTo-NormalizedLine $_.Text }
    $addedSet = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($a in $added) { [void]$addedSet.Add($a) }
    $addedNormByFile[$file] = $addedSet
}

# Resolve the base ref for git log scope. Try local refs first; if neither exists, fall
# back to --all (with a warning) so the script still produces useful output.
$gitLogRef = $null
foreach ($candidate in @($BaseBranch, "origin/$BaseBranch", "upstream/$BaseBranch")) {
    git rev-parse --verify --quiet $candidate 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) {
        $gitLogRef = $candidate
        break
    }
}
if (-not $gitLogRef) {
    Write-Host "  ⚠️  Base ref '$BaseBranch' not found locally — falling back to --all (may include unrelated history)." -ForegroundColor Yellow
}

# Resolve the PR's base branch so we can verify that fix PRs were actually merged
# into it. A fix merged to inflight/current won't be reachable from main.
$prBaseRef = $null
$prBaseJson = gh pr view $PRNumber --repo $Repo --json baseRefName --jq '.baseRefName' 2>$null
if ($prBaseJson) {
    foreach ($candidate in @($prBaseJson, "origin/$prBaseJson", "upstream/$prBaseJson")) {
        git rev-parse --verify --quiet $candidate 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) {
            $prBaseRef = $candidate
            break
        }
    }
}
if ($prBaseRef) {
    Write-Host "  📌 PR targets '$prBaseJson' — verifying fix PRs are reachable from $prBaseRef" -ForegroundColor Gray
} else {
    Write-Host "  ⚠️ Could not resolve PR base branch — skipping ancestry verification" -ForegroundColor Yellow
}

# Steps 2-5: per file
$risks = New-Object System.Collections.Generic.List[object]
$inspectedPRs = @{}
$fixDiffCache = @{}
$ghCallCount = 0

foreach ($filePath in $FilePaths) {
    Write-Host ""
    Write-Host "🔍 $filePath" -ForegroundColor Cyan

    # Step 2: recent PRs touching this file
    $sinceDate = (Get-Date).AddMonths(-$MonthsBack).ToString("yyyy-MM-dd")
    if ($gitLogRef) {
        # `--follow` traces through renames so we don't lose history when a file moves.
        # `--follow` is single-file only, which matches our per-file loop.
        $commitLog = git log --oneline --follow --since="$sinceDate" $gitLogRef -- $filePath 2>$null
    } else {
        $commitLog = git log --oneline --follow --since="$sinceDate" --all -- $filePath 2>$null
    }
    if (-not $commitLog) {
        Write-Host "  🟢 No recent commits." -ForegroundColor Green
        continue
    }

    $recentPRs = New-Object 'System.Collections.Generic.List[int]'
    $seen = New-Object 'System.Collections.Generic.HashSet[int]'
    foreach ($line in ($commitLog -split "`n")) {
        if ($line -match '\(#(\d+)\)') {
            $n = [int]$Matches[1]
            if ($n -ne $PRNumber -and $seen.Add($n)) {
                $recentPRs.Add($n)
                if ($recentPRs.Count -ge $MaxRecentPRsPerFile) { break }
            }
        }
    }

    if ($recentPRs.Count -eq 0) {
        Write-Host "  🟢 No recent PRs reference this file." -ForegroundColor Green
        continue
    }

    Write-Host "  Found $($recentPRs.Count) recent PR(s)" -ForegroundColor Gray

    # Step 3: filter to bug-fix PRs
    foreach ($recentPR in $recentPRs) {
        Write-Host "  📋 #$recentPR…" -ForegroundColor Gray -NoNewline

        if ($inspectedPRs.ContainsKey($recentPR)) {
            $meta = $inspectedPRs[$recentPR]
        } else {
            $meta = Get-PRMetadataIfBugFix -Number $recentPR -Repo $Repo
            $inspectedPRs[$recentPR] = $meta
            # Single combined `gh pr view --json labels,title,body` + up to one `gh issue
            # view` per linked issue. Average ≈ 1-3 calls per fix-PR candidate.
            $ghCallCount += 1 + ($(if ($meta -and $meta.LinkedIssues) { @($meta.LinkedIssues).Count } else { 0 }))
            if ($ghCallCount -gt 100) {
                Write-Host " (rate-limit guard: $ghCallCount gh calls so far)" -ForegroundColor DarkYellow
            }
        }
        if (-not $meta) {
            Write-Host " not a bug-fix" -ForegroundColor DarkGray
            continue
        }
        Write-Host " bug-fix [$($meta.Labels -join ', ')]" -ForegroundColor Yellow

        # Verify fix PR was actually merged into the PR's base branch. A fix merged
        # to inflight/current (or another branch) won't be in a PR targeting main.
        if ($prBaseRef -and $meta.MergeCommit) {
            git merge-base --is-ancestor $meta.MergeCommit $prBaseRef 2>$null
            if ($LASTEXITCODE -ne 0) {
                Write-Host "    ⏭️ fix not in PR's base branch (merged to different branch)" -ForegroundColor DarkGray
                continue
            }
        }

        # Step 4: parsed fix-PR diff (cache the *parsed* output, not just raw text).
        if ($fixDiffCache.ContainsKey($recentPR)) {
            $fixByFile = $fixDiffCache[$recentPR]
        } else {
            $fixDiff = Get-PRDiffText -Number $recentPR -Repo $Repo
            $ghCallCount++
            $fixByFile = if ($fixDiff) { Get-DiffLinesByFile -DiffText $fixDiff } else { @{} }
            $fixDiffCache[$recentPR] = $fixByFile
        }
        if ($fixByFile.Count -eq 0) {
            # Fix PR diff unavailable — record only if we actually deleted something here.
            if ($removedByFile.ContainsKey($filePath)) {
                $risks.Add([PSCustomObject]@{
                    File          = $filePath
                    RecentPR      = $recentPR
                    PRTitle       = $meta.Title
                    FixedIssues   = ($meta.LinkedIssues | ForEach-Object { "#$_" }) -join ', '
                    Labels        = $meta.Labels -join ', '
                    Risk          = 'OVERLAP'
                    Details       = 'Fix PR diff unavailable'
                    RevertedLines = @()
                })
            }
            continue
        }

        if (-not $fixByFile.ContainsKey($filePath)) {
            continue
        }

        $addedByFix = @($fixByFile[$filePath] |
            Where-Object { $_.Sign -eq '+' -and -not (Test-IsTrivialLine (ConvertTo-NormalizedLine $_.Text)) } |
            ForEach-Object { ConvertTo-NormalizedLine $_.Text }) | Select-Object -Unique
        if ($addedByFix.Count -eq 0) { continue }

        $removedHere = $removedByFile[$filePath]
        # OVERLAP only matters when the current PR actually deleted something from this
        # file. Otherwise, "same file, different lines" isn't regression evidence.
        if (-not $removedHere) {
            continue
        }

        # Step 5: compare. Suppress matches the current PR also re-added (move/refactor).
        $addedSet = New-Object 'System.Collections.Generic.HashSet[string]'
        foreach ($n in $addedByFix) { [void]$addedSet.Add($n) }
        $currentAddedSet = $addedNormByFile[$filePath]

        $reverted = New-Object System.Collections.Generic.List[object]
        $seenLines = New-Object 'System.Collections.Generic.HashSet[string]'
        foreach ($r in $removedHere) {
            $key = ConvertTo-NormalizedLine $r.Text
            if (-not $addedSet.Contains($key)) { continue }
            if ($currentAddedSet -and $currentAddedSet.Contains($key)) { continue }   # moved within PR
            if (-not $seenLines.Add($key)) { continue }   # dedup repeats
            $reverted.Add([PSCustomObject]@{ Text = $r.Text; Line = $r.Line })
        }

        # Pre-compute values outside [PSCustomObject]@{} to avoid PowerShell evaluation
        # context issues (observed "Argument types do not match" when $reverted.Count is
        # evaluated inside a hashtable literal passed to List[object].Add()).
        $issueLinks   = ($meta.LinkedIssues | ForEach-Object { "#$_" }) -join ', '
        $labelJoined  = $meta.Labels -join ', '
        $revertCount  = $reverted.Count
        $revertedArr  = $reverted.ToArray()

        if ($revertCount -gt 0) {
            Write-Host "    🔴 REVERT — $revertCount line(s) from #$recentPR being removed" -ForegroundColor Red
            foreach ($rl in $reverted) { Write-Host "      - $($rl.Text.Trim())" -ForegroundColor Red }
            $riskEntry = [PSCustomObject]@{
                File          = $filePath
                RecentPR      = $recentPR
                PRTitle       = $meta.Title
                FixedIssues   = $issueLinks
                Labels        = $labelJoined
                Risk          = 'REVERT'
                Details       = "Removes $revertCount line(s) added by fix PR #$recentPR"
                RevertedLines = $revertedArr
            }
            $risks.Add($riskEntry)
        } else {
            $riskEntry = [PSCustomObject]@{
                File          = $filePath
                RecentPR      = $recentPR
                PRTitle       = $meta.Title
                FixedIssues   = $issueLinks
                Labels        = $labelJoined
                Risk          = 'OVERLAP'
                Details       = 'Same file, different lines'
                RevertedLines = @()
            }
            $risks.Add($riskEntry)
        }
    }
}

# ─── Extract test files from fix PRs that triggered REVERT ─────────────────────
# For each REVERT, find test files the fix PR added/modified and classify them
# via Detect-TestsInDiff.ps1 (if available). This enables downstream test execution.

$detectTestsScript = Join-Path $PSScriptRoot "shared/Detect-TestsInDiff.ps1"
$hasTestDetector = Test-Path $detectTestsScript

$fixPRsWithTests = @{}   # fixPR -> array of test metadata

if ($hasTestDetector) {
    # Extract tests for ALL risk entries (REVERT and OVERLAP) for maximum confidence
    $allFixPRs = @($risks | Select-Object -ExpandProperty RecentPR -Unique)

    foreach ($fixPR in $allFixPRs) {
        if ($fixPRsWithTests.ContainsKey($fixPR)) { continue }

        # Get all file paths from the fix PR diff (already cached)
        $fixFiles = @()
        if ($fixDiffCache.ContainsKey($fixPR)) {
            $fixFiles = @($fixDiffCache[$fixPR].Keys | Where-Object { Test-IsTestFile $_ })
        }

        if ($fixFiles.Count -eq 0) {
            Write-Host "  [info] Fix PR #$fixPR`: no test files in diff" -ForegroundColor DarkGray
            $fixPRsWithTests[$fixPR] = @()
            continue
        }

        Write-Host "  🧪 Fix PR #$fixPR`: detecting tests from $($fixFiles.Count) test file(s)…" -ForegroundColor Cyan
        try {
            $detected = & $detectTestsScript -ChangedFiles $fixFiles 2>&1
            # Filter out Write-Host output — only keep returned objects
            $testEntries = @($detected | Where-Object { $_ -is [hashtable] -or ($_ -is [PSCustomObject]) })
            if ($testEntries.Count -gt 0) {
                Write-Host "    Found $($testEntries.Count) test(s)" -ForegroundColor Green
                $fixPRsWithTests[$fixPR] = $testEntries
            } else {
                Write-Host "    No classifiable tests found" -ForegroundColor DarkGray
                $fixPRsWithTests[$fixPR] = @()
            }
        } catch {
            Write-Host "    ⚠️ Test detection failed: $_" -ForegroundColor Yellow
            $fixPRsWithTests[$fixPR] = @()
        }
    }
} else {
    Write-Host "  ℹ️ Detect-TestsInDiff.ps1 not found — skipping test extraction" -ForegroundColor DarkGray
}

# Attach test metadata to ALL risk entries (REVERT and OVERLAP)
foreach ($r in $risks) {
    $r | Add-Member -NotePropertyName TestsFromFixPR -NotePropertyValue @() -Force
    if ($fixPRsWithTests.ContainsKey($r.RecentPR)) {
        $r.TestsFromFixPR = $fixPRsWithTests[$r.RecentPR]
    }
}

Write-Banner "Results"

$reverts  = @($risks | Where-Object { $_.Risk -eq 'REVERT' })
$overlaps = @($risks | Where-Object { $_.Risk -eq 'OVERLAP' })
$result   = if ($reverts.Count -gt 0) { 'REVERT' }
            elseif ($overlaps.Count -gt 0) { 'OVERLAP' }
            else { 'CLEAN' }

switch ($result) {
    'REVERT' {
        Write-Host "🔴 REVERT RISKS: $($reverts.Count)" -ForegroundColor Red
        foreach ($r in $reverts) {
            Write-Host ""
            Write-Host "  File:         $($r.File)" -ForegroundColor Red
            Write-Host "  Fix PR:       #$($r.RecentPR) — $($r.PRTitle)" -ForegroundColor Red
            Write-Host "  Fixed:        $($r.FixedIssues)" -ForegroundColor Red
            Write-Host "  Reverted:     $((@($r.RevertedLines) | Select-Object -First 3 | ForEach-Object { $_.Text.Trim() }) -join ' | ')" -ForegroundColor Red
        }
        $allIssues = @($reverts | ForEach-Object { $_.FixedIssues -split ',\s*' } |
            Where-Object { $_ } | Select-Object -Unique | Sort-Object)
        if ($allIssues.Count -gt 0) {
            Write-Host ""
            Write-Host "⚠️  Verify that issues $($allIssues -join ', ') do not re-regress." -ForegroundColor Yellow
        }
    }
    'OVERLAP' {
        Write-Host "🟡 OVERLAPS: $($overlaps.Count) (lower risk — same files, different lines)" -ForegroundColor Yellow
        foreach ($o in $overlaps) {
            Write-Host "  $($o.File) — fix PR #$($o.RecentPR) ($($o.FixedIssues))" -ForegroundColor Yellow
        }
    }
    'CLEAN' {
        Write-Host "🟢 No regression risks detected." -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "(gh API calls: $ghCallCount; PRs inspected: $($inspectedPRs.Count))" -ForegroundColor DarkGray

# ─── Output files ─────────────────────────────────────────────────────────────

if ($OutputDir) {
    New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

    # result.txt
    $result | Set-Content (Join-Path $OutputDir 'result.txt') -Encoding UTF8

    # risks.json — structured output for agent consumption
    $jsonRisks = @($risks | ForEach-Object {
        $entry = @{
            file           = $_.File
            recent_pr      = $_.RecentPR
            pr_title       = $_.PRTitle
            fixed_issues   = $_.FixedIssues
            labels         = $_.Labels
            risk           = $_.Risk
            details        = $_.Details
            reverted_lines = @(@($_.RevertedLines) | ForEach-Object { @{ text = $_.Text; line = $_.Line } })
        }
        # Include test metadata for all risk entries (REVERT and OVERLAP)
        if ($_.TestsFromFixPR -and $_.TestsFromFixPR.Count -gt 0) {
            $entry['regression_tests'] = @($_.TestsFromFixPR | ForEach-Object {
                @{
                    type         = $_.Type
                    test_name    = $_.TestName
                    filter       = $_.Filter
                    project_path = $_.ProjectPath
                    project      = $_.Project
                    runner       = $_.Runner
                    files        = @($_.Files)
                }
            })
        } else {
            $entry['regression_tests'] = @()
        }
        $entry
    })
    $payload = @{
        pr_number    = $PRNumber
        result       = $result
        revert_count = $reverts.Count
        overlap_count= $overlaps.Count
        risks        = $jsonRisks
    } | ConvertTo-Json -Depth 6
    $payload | Set-Content (Join-Path $OutputDir 'risks.json') -Encoding UTF8

    # content.md — markdown summary for the wall-of-text PR review
    $md = New-Object System.Text.StringBuilder
    [void]$md.AppendLine("## 🔍 Regression Cross-Reference")
    [void]$md.AppendLine()
    switch ($result) {
        'REVERT' {
            [void]$md.AppendLine("🔴 **Revert risks detected** — this PR removes $($reverts.Count) line(s) previously added by labeled bug-fix PRs.")
            [void]$md.AppendLine()
            [void]$md.AppendLine("| File | Fix PR | Fixed issue(s) | Risk | Reverted line |")
            [void]$md.AppendLine("|---|---|---|---|---|")
            foreach ($r in $reverts) {
                $sample = @($r.RevertedLines) | Select-Object -First 1 | ForEach-Object { $_.Text.Trim() }
                $sampleEsc = ($sample -replace '\|', '\|')
                [void]$md.AppendLine("| ``$($r.File)`` | #$($r.RecentPR) | $($r.FixedIssues) | 🔴 REVERT | ``$sampleEsc`` |")
            }
            $allIssues = @($reverts | ForEach-Object { $_.FixedIssues -split ',\s*' } |
                Where-Object { $_ } | Select-Object -Unique | Sort-Object)
            if ($allIssues.Count -gt 0) {
                [void]$md.AppendLine()
                [void]$md.AppendLine("**Action required:** Verify that issues $($allIssues -join ', ') do not re-regress before merging.")
            }

            # List regression tests that should be run
            $allRegressionTests = @($reverts | Where-Object { $_.TestsFromFixPR.Count -gt 0 } |
                ForEach-Object { $pr = $_.RecentPR; $_.TestsFromFixPR | ForEach-Object {
                    [PSCustomObject]@{ FixPR = $pr; Type = $_.Type; TestName = $_.TestName; Filter = $_.Filter; Runner = $_.Runner }
                }})
            if ($allRegressionTests.Count -gt 0) {
                [void]$md.AppendLine()
                [void]$md.AppendLine("### 🧪 Regression Tests to Verify")
                [void]$md.AppendLine()
                [void]$md.AppendLine("These tests were added by the fix PRs being reverted. They must still pass:")
                [void]$md.AppendLine()
                [void]$md.AppendLine("| Fix PR | Type | Test | Filter |")
                [void]$md.AppendLine("|---|---|---|---|")
                foreach ($t in $allRegressionTests) {
                    [void]$md.AppendLine("| #$($t.FixPR) | $($t.Type) | $($t.TestName) | ``$($t.Filter)`` |")
                }
            }
        }
        'OVERLAP' {
            [void]$md.AppendLine("🟡 **Overlaps with prior bug-fix PRs** — same files modified, but no exact line revert detected.")
            [void]$md.AppendLine()
            [void]$md.AppendLine("| File | Fix PR | Fixed issue(s) |")
            [void]$md.AppendLine("|---|---|---|")
            foreach ($o in $overlaps) {
                [void]$md.AppendLine("| ``$($o.File)`` | #$($o.RecentPR) | $($o.FixedIssues) |")
            }

            # List regression tests from overlapping fix PRs
            $overlapTests = @($overlaps | Where-Object { $_.TestsFromFixPR.Count -gt 0 } |
                ForEach-Object { $pr = $_.RecentPR; $_.TestsFromFixPR | ForEach-Object {
                    [PSCustomObject]@{ FixPR = $pr; Type = $_.Type; TestName = $_.TestName; Filter = $_.Filter; Runner = $_.Runner }
                }})
            if ($overlapTests.Count -gt 0) {
                [void]$md.AppendLine()
                [void]$md.AppendLine("### 🧪 Regression Tests to Verify")
                [void]$md.AppendLine()
                [void]$md.AppendLine("These tests were added by the overlapping fix PRs. Running them to verify no side-effect regressions:")
                [void]$md.AppendLine()
                [void]$md.AppendLine("| Fix PR | Type | Test | Filter |")
                [void]$md.AppendLine("|---|---|---|---|")
                foreach ($t in $overlapTests) {
                    [void]$md.AppendLine("| #$($t.FixPR) | $($t.Type) | $($t.TestName) | ``$($t.Filter)`` |")
                }
            }
        }
        'CLEAN' {
            [void]$md.AppendLine("🟢 No regression risks detected. No labeled bug-fix PRs in the last $MonthsBack months touched the modified files.")
        }
    }
    $md.ToString() | Set-Content (Join-Path $OutputDir 'content.md') -Encoding UTF8

    # inline-findings.json — optional, only if reverts found
    if ($WriteInlineFindings -and $reverts.Count -gt 0) {
        $inlinePath = Join-Path $OutputDir 'inline-findings.json'
        $inline = @()
        foreach ($r in $reverts) {
            foreach ($rl in @($r.RevertedLines)) {
                $prUrl = "https://github.com/$Repo/pull/$($r.RecentPR)"
                $body = "🔴 **Regression risk** — this line was added by [#$($r.RecentPR)]($prUrl) to fix $($r.FixedIssues). Removing it may re-introduce the original bug. Please confirm this removal is intentional and that the previously-fixed issue is covered by another mechanism."
                $inline += @{
                    path = $r.File
                    line = $rl.Line
                    body = $body
                    side = 'LEFT'
                }
            }
        }
        ($inline | ConvertTo-Json -Depth 4) | Set-Content $inlinePath -Encoding UTF8
        Write-Host ""
        Write-Host "📝 Wrote $($inline.Count) inline finding(s) to $inlinePath" -ForegroundColor DarkGray
    }

    Write-Host ""
    Write-Host "📁 Outputs written to: $OutputDir" -ForegroundColor DarkGray
}

exit 0
