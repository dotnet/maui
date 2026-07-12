#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Prepare the input for the AI analysis of deep UI test failures.

.DESCRIPTION
    Runs in the UpdateAISummaryComment stage (which holds GH_TOKEN, NOT the
    Copilot token). It:
      1. Aggregates the deep-UI-test TRX artifacts by category.
      2. Extracts only the REGULAR failures — individual test bodies that ran
         and failed. Pure OneTimeSetUp/fixture failures and app-crash
         signatures are EXCLUDED here because the renderer already classifies
         those as infrastructure / ambiguous (they are not "the PR failed a
         test"); feeding them to the classifier would only add noise.
      3. When there is at least one regular failure, fetches the PR's changed
         files + a bounded unified diff via `gh` and writes a single
         self-contained data file the (token-less) Copilot step then reads.

    Security: this script only reads TRX text and calls `gh` for PR metadata —
    it never runs PR-controlled code and never invokes Copilot, so it is safe
    to hold GH_TOKEN (see ci-copilot-pipeline-security.instructions.md rule 1
    and the rule-4 exception for gh-metadata-only scripts). It intentionally
    writes all untrusted failure/diff text to a FILE and keeps its own stdout
    limited to counts + sanitized category names, so the `hasUIFailures`
    ##vso directive it emits can never be spoofed by a malicious test name
    (rule 6).

.PARAMETER ArtifactDir
    Root of the downloaded drop-deep-uitests artifact.

.PARAMETER PRNumber
    Pull request number (used for `gh pr diff`).

.PARAMETER OutputFile
    Path to write the analysis input markdown. Must live under
    $(Agent.TempDirectory) (rule 5). Written only when regular failures exist.

.PARAMETER MaxDiffLines
    Truncate the unified diff to this many lines (default 800) to bound prompt
    size. The complete changed-file list is always included regardless.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $ArtifactDir,
    [Parameter(Mandatory = $true)] [string] $PRNumber,
    [Parameter(Mandatory = $true)] [string] $OutputFile,
    [string] $Platform,
    [int] $MaxDiffLines = 800
)

$ErrorActionPreference = 'Continue'

if (-not (Test-Path $ArtifactDir)) {
    Write-Host "No artifact dir ($ArtifactDir) — nothing to analyze."
    exit 0
}

# Shared aggregation helpers (same ones the renderer uses, so "regular
# failure" here means exactly what $regularFailed means in the renderer).
. (Join-Path $PSScriptRoot 'Get-TrxResults.ps1')
. (Join-Path $PSScriptRoot 'Get-CategoryFromArtifactName.ps1')
. (Join-Path $PSScriptRoot 'Get-AggregatedTrxFromDirectory.ps1')

$byCat = Get-AggregatedTrxFromDirectory -RootDir $ArtifactDir
if (-not $byCat -or $byCat.Count -eq 0) {
    Write-Host "Aggregator returned no categories — no failures to analyze."
    exit 0
}

# Collect regular (non-setup, non-app-crash) failures per category.
$regular = [ordered]@{}
$regularCount = 0
foreach ($k in ($byCat.Keys | Sort-Object)) {
    $b = $byCat[$k]
    $isSetupFailure = ($b.ContainsKey('SetupFailure') -and [bool]$b.SetupFailure)
    if ($isSetupFailure) { continue }   # infra / ambiguous — renderer handles it
    $catFailed = @()
    foreach ($r in @($b.Results)) {
        if ($r.status -eq 'Failed') {
            $catFailed += [pscustomobject]@{
                Name  = [string]$r.name
                Error = [string]$r.error
                Stack = [string]$r.stack
            }
        }
    }
    if ($catFailed.Count -gt 0) {
        $regular[$k] = $catFailed
        $regularCount += $catFailed.Count
    }
}

if ($regularCount -eq 0) {
    Write-Host "No regular (non-setup) UI test failures — skipping AI failure analysis."
    exit 0
}

Write-Host "Found $regularCount regular UI test failure(s) across $($regular.Count) categor$(if ($regular.Count -eq 1) {'y'} else {'ies'}): $(( $regular.Keys) -join ', ')"

# ── Build the data file (untrusted failure text goes to the FILE only) ──
$sb = [System.Text.StringBuilder]::new()
[void]$sb.AppendLine("# Deep UI test failures — PR #$PRNumber")
[void]$sb.AppendLine()
if (-not [string]::IsNullOrWhiteSpace($Platform)) {
    [void]$sb.AppendLine("**Deep run platform: $Platform** — these tests executed on the $Platform agent, so only code compiled for $Platform can affect their results.")
    [void]$sb.AppendLine()
}
[void]$sb.AppendLine("$regularCount failing test(s) across $($regular.Count) categor$(if ($regular.Count -eq 1) {'y'} else {'ies'}).")
[void]$sb.AppendLine()
[void]$sb.AppendLine("## Failing tests")
foreach ($cat in $regular.Keys) {
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("### Category: $cat")
    foreach ($it in @($regular[$cat] | Select-Object -First 25)) {
        $err = if ($it.Error) { (($it.Error -split "`r?`n") | Select-Object -First 12) -join "`n" } else { '' }
        $stk = if ($it.Stack) { (($it.Stack -split "`r?`n") | Select-Object -First 8)  -join "`n" } else { '' }
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("- Test: $($it.Name)")
        $body = $err
        if ($stk) { $body = ($body + "`n" + $stk).Trim() }
        if ($body) {
            [void]$sb.AppendLine('  ```')
            foreach ($ln in ($body -split "`n")) { [void]$sb.AppendLine("  $ln") }
            [void]$sb.AppendLine('  ```')
        }
    }
    if (@($regular[$cat]).Count -gt 25) {
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("- _(+$((@($regular[$cat]).Count) - 25) more in this category — omitted)_")
    }
}

# ── PR context via gh (metadata only; this task legitimately holds GH_TOKEN) ──
[void]$sb.AppendLine()
[void]$sb.AppendLine("## PR changed files")
$changedFiles = @()
try {
    $changedFiles = @(& gh pr diff $PRNumber --name-only 2>$null | Where-Object { $_ })
} catch { }
if ($changedFiles.Count -gt 0) {
    foreach ($f in ($changedFiles | Select-Object -First 200)) { [void]$sb.AppendLine("- $f") }
    if ($changedFiles.Count -gt 200) { [void]$sb.AppendLine("- _(+$($changedFiles.Count - 200) more files)_") }
} else {
    [void]$sb.AppendLine("_(could not determine changed files)_")
}

[void]$sb.AppendLine()
[void]$sb.AppendLine("## PR unified diff (truncated to $MaxDiffLines lines)")
$diffLines = @()
try {
    $diffLines = @(& gh pr diff $PRNumber 2>$null)
} catch { }
if ($diffLines.Count -gt 0) {
    $truncated = $diffLines.Count -gt $MaxDiffLines
    $show = if ($truncated) { $diffLines[0..($MaxDiffLines - 1)] } else { $diffLines }
    [void]$sb.AppendLine('```diff')
    foreach ($ln in $show) { [void]$sb.AppendLine($ln) }
    [void]$sb.AppendLine('```')
    if ($truncated) { [void]$sb.AppendLine("_(diff truncated — $($diffLines.Count) total lines; full file list above)_") }
} else {
    [void]$sb.AppendLine("_(diff unavailable)_")
}

$outDir = Split-Path -Parent $OutputFile
if ($outDir -and -not (Test-Path $outDir)) { New-Item -ItemType Directory -Force -Path $outDir | Out-Null }
$sb.ToString() | Set-Content -Path $OutputFile -Encoding UTF8
Write-Host "Wrote analysis input ($((Get-Item $OutputFile).Length) bytes) to $OutputFile"

# Signal the (conditional) install + Copilot analysis tasks to run. This
# literal is fully controlled by us; no untrusted text reaches stdout above.
Write-Host "##vso[task.setvariable variable=hasUIFailures]true"
