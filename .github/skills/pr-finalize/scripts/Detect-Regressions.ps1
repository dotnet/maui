#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Detects potential regressions in a PR by checking if deleted lines were part of prior bug fixes.

.DESCRIPTION
    This script analyzes a PR's diff to find deleted lines from implementation files,
    then uses git blame to trace those lines back to their originating commits.
    It flags lines whose origin commit references an issue number (fixes #XXXX),
    as these are likely deliberate bug fixes that should not be removed without explanation.

.PARAMETER PRNumber
    The PR number to analyze. If not provided, attempts to detect from current branch.

.PARAMETER BaseBranch
    The base branch to diff against. Defaults to 'origin/main'.

.PARAMETER OutputPath
    Path for the output report. Defaults to CustomAgentLogsTmp/RegressionCheck/report.md

.EXAMPLE
    pwsh .github/skills/pr-finalize/scripts/Detect-Regressions.ps1 -PRNumber 33908
    pwsh .github/skills/pr-finalize/scripts/Detect-Regressions.ps1 -PRNumber 33908 -BaseBranch origin/net10.0
#>

param(
    [Parameter(Mandatory = $false)]
    [int]$PRNumber,

    [string]$BaseBranch = "origin/main",

    [string]$OutputPath = "CustomAgentLogsTmp/RegressionCheck/report.md"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# --- Helper Functions ---

function Write-Section {
    param([string]$Title, [string]$Content)
    Write-Host ""
    Write-Host "=== $Title ===" -ForegroundColor Cyan
    Write-Host $Content
}

function Get-IssueReferences {
    param([string]$CommitMessage)
    # Match explicit issue-fix keywords only. Do NOT match bare (#{N}) because GitHub
    # automatically appends the PR number to every squash-merged commit subject
    # (e.g. "Fix something (#32278)") — that pattern would flag every single deleted line.
    $patterns = @(
        'fixes?\s+#(\d+)',
        'closes?\s+#(\d+)',
        'resolves?\s+#(\d+)',
        'issue\s+#(\d+)',
        're:\s+#(\d+)'
    )
    $issues = @()
    foreach ($pattern in $patterns) {
        $matches = [regex]::Matches($CommitMessage, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        foreach ($match in $matches) {
            $issues += $match.Groups[1].Value
        }
    }
    return $issues | Select-Object -Unique
}

function Is-TrivialLine {
    param([string]$Line)
    # Skip blank lines, pure comment lines, opening/closing braces, using statements
    $trimmed = $Line.Trim()
    if ($trimmed -eq '' -or $trimmed -eq '{' -or $trimmed -eq '}' -or $trimmed -eq '};') { return $true }
    if ($trimmed.StartsWith('//') -or $trimmed.StartsWith('*') -or $trimmed.StartsWith('/*')) { return $true }
    if ($trimmed.StartsWith('using ') -or $trimmed.StartsWith('namespace ')) { return $true }
    # Match class declarations with any combination of modifiers (public, sealed, internal, static, abstract, partial, etc.)
    if ($trimmed -match '^\s*(public|private|protected|internal|static|sealed|abstract|partial|\s)*class\s') { return $true }
    return $false
}

function Is-TestFile {
    param([string]$FilePath)
    # Match only path SEGMENTS to avoid false positives on words containing "spec" or "test"
    # (e.g. "AspectExtensions.cs", "PlatformConfiguration/AndroidSpecific/Button.cs",
    #  "MeasureSpecFactory.cs" would all be incorrectly excluded by a plain substring match)
    return $FilePath -match '[\\/](tests?|specs?)[\\/]' -or
           $FilePath -match '[\\/](TestCases|DeviceTests|UnitTests|Xaml\.UnitTests)[\\/]' -or
           $FilePath -match '\.Tests\.' -or
           $FilePath -match '\.t\.cs$'
}

function Is-ImplementationFile {
    param([string]$FilePath)
    if (Is-TestFile $FilePath) { return $false }
    return $FilePath -match '\.(cs|xaml|swift|java|kt|m|cpp|h)$'
}

# --- Main Logic ---

$OutputDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

Write-Host "🔍 Starting Prior Fix Regression Check" -ForegroundColor Yellow

# Get the PR diff
$DiffContent = $null
if ($PRNumber -gt 0) {
    Write-Host "  Fetching diff for PR #$PRNumber..."
    try {
        $DiffContent = gh pr diff $PRNumber 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "gh pr diff failed: $DiffContent"
        }
    }
    catch {
        Write-Warning "Could not fetch PR diff via gh: $_. Falling back to git diff."
        $DiffContent = git diff "${BaseBranch}...HEAD" 2>&1
    }
}
else {
    Write-Host "  No PR number provided. Using git diff against $BaseBranch..."
    $DiffContent = git diff "${BaseBranch}...HEAD" 2>&1
}

if (-not $DiffContent) {
    Write-Warning "No diff content found. Exiting."
    exit 0
}

# Parse the diff to find deleted lines per file
$CurrentFile = $null
$FileDeletes = @{}  # file -> list of (lineContent, lineNumber)
$CurrentLineNumber = 0
$InHunk = $false

foreach ($line in $DiffContent -split "`n") {
    if ($line -match '^diff --git a/.+ b/(.+)$') {
        $CurrentFile = $Matches[1]
        $InHunk = $false
        continue
    }

    if ($line -match '^\+\+\+ b/(.+)$') {
        $CurrentFile = $Matches[1]
        continue
    }

    if ($line -match '^--- ') {
        continue
    }

    if ($line -match '^@@ -(\d+)') {
        $CurrentLineNumber = [int]$Matches[1]
        $InHunk = $true
        continue
    }

    if (-not $InHunk -or -not $CurrentFile) { continue }
    if (-not (Is-ImplementationFile $CurrentFile)) { continue }

    if ($line.StartsWith('-')) {
        $lineContent = $line.Substring(1)
        if (-not (Is-TrivialLine $lineContent)) {
            if (-not $FileDeletes.ContainsKey($CurrentFile)) {
                $FileDeletes[$CurrentFile] = @()
            }
            $FileDeletes[$CurrentFile] += [PSCustomObject]@{
                LineContent  = $lineContent
                LineNumber   = $CurrentLineNumber
            }
        }
        $CurrentLineNumber++
    }
    elseif ($line.StartsWith('+')) {
        # don't increment line number for additions
    }
    else {
        $CurrentLineNumber++
    }
}

if ($FileDeletes.Count -eq 0) {
    Write-Host "  No significant deleted lines found in implementation files." -ForegroundColor Green
    $Report = @"
### ✅ Prior Fix Regression Check: PASSED

No significant deleted lines were found in implementation files. No prior fix regressions detected.
"@
    $Report | Out-File -FilePath $OutputPath -Encoding utf8
    Write-Host ""
    Write-Host $Report
    exit 0
}

Write-Host "  Found deleted lines in $($FileDeletes.Count) implementation file(s). Checking git blame..."

# For each deleted line, check git blame on the base branch
$Regressions = @()
$Checked = 0

foreach ($file in $FileDeletes.Keys) {
    $deletes = $FileDeletes[$file]

    # Get git blame for this file on the base branch
    $blameOutput = $null
    try {
        # Use $BaseBranch as-is — git blame accepts remote tracking refs (e.g. origin/main)
        # directly, which works reliably in worktrees and CI where a local tracking branch
        # may not exist.
        $blameOutput = git blame "$BaseBranch" -- $file 2>&1
        if ($LASTEXITCODE -ne 0 -or ($blameOutput -join '') -match 'no such path|fatal:') {
            # File may have been renamed in the PR — git blame on the old path will fail.
            # Try to find the original path via rename detection.
            $renamedFrom = git log --follow --diff-filter=R --name-status "$BaseBranch..HEAD" -- $file 2>&1 |
                Where-Object { $_ -match '^R\d+\s' } |
                ForEach-Object { ($_ -split '\s+')[1] } |
                Select-Object -First 1
            if ($renamedFrom) {
                Write-Host "  '$file' appears renamed from '$renamedFrom'. Blaming original path."
                $blameOutput = git blame "$BaseBranch" -- $renamedFrom 2>&1
            }
            if ($LASTEXITCODE -ne 0 -or -not $blameOutput) {
                Write-Warning "  Could not run git blame for $file (or its rename source). Skipping."
                continue
            }
        }
    }
    catch {
        Write-Warning "  Could not run git blame for $file. Skipping."
        continue
    }

    $blameLines = $blameOutput -split "`n"

    foreach ($deleteInfo in $deletes) {
        $Checked++
        $searchContent = $deleteInfo.LineContent.Trim()
        if ($searchContent.Length -lt 5) { continue }  # too short to be meaningful

        # Find matching lines in blame output
        # Format: "<hash> (<author> <date> <time> <tz> <lineno>) <content>"
        $matchingBlameLines = $blameLines | Where-Object { $_ -match [regex]::Escape($searchContent.Substring(0, [Math]::Min(40, $searchContent.Length))) }

        foreach ($blameLine in $matchingBlameLines) {
            if ($blameLine -match '^(\^?[0-9a-f]{7,40})\s') {
                $commitHash = $Matches[1].TrimStart('^')
                if ($commitHash -match '^0+$') { continue }  # uncommitted

                # Get commit message
                $commitMsg = $null
                try {
                    $commitMsg = git log --format="%s%n%b" -1 $commitHash 2>&1 | Out-String
                }
                catch { continue }

                if (-not $commitMsg) { continue }

                # Check if commit references an issue
                $issueRefs = @(Get-IssueReferences $commitMsg)
                $prRefs = @([regex]::Matches($commitMsg, '\(#(\d+)\)') | ForEach-Object { $_.Groups[1].Value })

                # If the commit message itself has no issue refs but references a PR number,
                # check the PR body on GitHub — many PRs put "Fixes #XXXX" only in the
                # PR description, not in the commit message body.
                if ($issueRefs.Count -eq 0 -and $prRefs.Count -gt 0) {
                    foreach ($prNum in $prRefs) {
                        try {
                            $prBody = gh pr view $prNum --repo $env:GITHUB_REPOSITORY --json body --jq '.body' 2>&1
                            if ($LASTEXITCODE -eq 0 -and $prBody) {
                                $prIssueRefs = @(Get-IssueReferences ($prBody | Out-String))
                                if ($prIssueRefs.Count -gt 0) {
                                    $issueRefs = $prIssueRefs
                                    break
                                }
                                # Also check for standalone "- #XXXX" or "* #XXXX" under a Fixes/Issues section
                                $sectionRefs = @([regex]::Matches(($prBody | Out-String),
                                    '(?:^|\n)\s*[-*]\s*#(\d+)', [System.Text.RegularExpressions.RegexOptions]::Multiline) |
                                    ForEach-Object { $_.Groups[1].Value })
                                if ($sectionRefs.Count -gt 0) {
                                    $issueRefs = $sectionRefs
                                    break
                                }
                            }
                        }
                        catch {
                            # gh CLI may not be available (e.g. inside agent sandbox)
                            # Fall through — the agent prompt can do this check via MCP tools
                        }
                    }
                }

                if ($issueRefs.Count -gt 0) {
                    $commitSubject = (git log --format="%s" -1 $commitHash 2>&1) -join ""

                    $Regressions += [PSCustomObject]@{
                        File           = $file
                        DeletedLine    = $deleteInfo.LineContent
                        CommitHash     = $commitHash.Substring(0, 7)
                        CommitSubject  = $commitSubject
                        IssueRefs      = ($issueRefs | ForEach-Object { "#$_" }) -join ', '
                        PRRefs         = $prRefs -join ', '
                    }
                    break  # One match per deleted line is enough
                }
            }
        }
    }
}

Write-Host "  Checked $Checked deleted lines. Found $($Regressions.Count) potential regression(s)."

# Generate report
if ($Regressions.Count -eq 0) {
    $Report = @"
### ✅ Prior Fix Regression Check: PASSED

Checked $Checked deleted lines across $($FileDeletes.Count) implementation file(s). No lines were identified as reversions of prior bug fixes.
"@
}
else {
    $ReportLines = @("### 🔴 Prior Fix Regression Check: FAILED", "")
    $ReportLines += "**$($Regressions.Count) potential regression(s) detected.** The following deleted lines appear to have been added specifically to fix prior bugs:"
    $ReportLines += ""

    foreach ($reg in $Regressions) {
        $prInfo = if ($reg.PRRefs) { " (PR #$($reg.PRRefs))" } else { "" }
        $ReportLines += @"
---

**File:** ``$($reg.File)``

**Deleted line:**
``````diff
- $($reg.DeletedLine.Trim())
``````

**Origin:** Added in commit ``$($reg.CommitHash)``$prInfo — _"$($reg.CommitSubject)"_

**References issue(s):** $($reg.IssueRefs)

**⚠️ Risk:** This line was deliberately added to fix issue(s) $($reg.IssueRefs). Removing it may reintroduce that bug.

**Required action:** The PR author must confirm this removal is intentional AND explain how issue $($reg.IssueRefs) is still prevented by other means.

"@
    }

    $Report = $ReportLines -join "`n"
}

$Report | Out-File -FilePath $OutputPath -Encoding utf8
Write-Host ""
Write-Host $Report

if ($Regressions.Count -gt 0) {
    Write-Host ""
    Write-Host "⚠️ Regression check FAILED — $($Regressions.Count) potential regression(s) found. See $OutputPath" -ForegroundColor Red
    exit 1
}
else {
    Write-Host ""
    Write-Host "✅ Regression check PASSED" -ForegroundColor Green
    exit 0
}
