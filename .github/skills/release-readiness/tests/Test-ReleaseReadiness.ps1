#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Smoke tests for Get-ReleaseReadiness.ps1.

.DESCRIPTION
    Tests two flavors:
      (a) Parser/regex unit tests with fake commit-message fixtures (no network)
      (b) End-to-end smoke against SR7 known-answer set (requires git + gh)

    Run with -SkipE2E to skip the network-dependent integration test.
#>
[CmdletBinding()]
param(
    [switch]$SkipE2E
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$script:passed = 0
$script:failed = 0

function Assert-Eq {
    param([string]$Label, $Expected, $Actual)
    if ($Expected -ceq $Actual -or
        ((@($Expected) -join ',') -eq (@($Actual) -join ','))) {
        Write-Host "  ✅ $Label" -ForegroundColor Green
        $script:passed++
    } else {
        Write-Host "  ❌ $Label" -ForegroundColor Red
        Write-Host "     expected: $Expected" -ForegroundColor DarkRed
        Write-Host "     actual  : $Actual" -ForegroundColor DarkRed
        $script:failed++
    }
}

# ─────────── Parser/regex unit tests (no network) ───────────

Write-Host "`n[Unit] Commit message parsing" -ForegroundColor Cyan

# Test 1: Backport with "(#NNNN)" subject suffix and "Backport of #NNNN" body
$bodyA = @"
Backport of #35356

This is the backport of the Android CollectionView fix.
Fixes #35313

(cherry picked from commit deadbeef1234)
"@
$subjA = '[release/10.0.1xx-sr7] [Android] Fix CollectionView ScrollTo(0) IsGrouped (#35428)'
$subjMatch = [regex]::Matches($subjA, '\(#(\d+)\)')
Assert-Eq -Label "Subject extracts backport PR #" -Expected '35428' -Actual $subjMatch[$subjMatch.Count - 1].Groups[1].Value

$sourceMatch = [regex]::Match($bodyA, '(?im)(?:backport\s+of|cherry[-\s]picked\s+from(?:\s+PR)?)\s+#(\d+)')
Assert-Eq -Label "Body extracts source PR via 'Backport of #'" -Expected '35356' -Actual $sourceMatch.Groups[1].Value

$cherrySha = [regex]::Match($bodyA, '(?im)cherry\s+picked\s+from\s+commit\s+([0-9a-f]{7,40})')
Assert-Eq -Label "Body extracts cherry-pick source SHA" -Expected 'deadbeef1234' -Actual $cherrySha.Groups[1].Value

$issMatches = [regex]::Matches($bodyA, '(?im)(?:fixes|closes|resolves)\s+(?:dotnet/maui#|#)(\d+)')
Assert-Eq -Label "Body extracts 'Fixes #' issues" -Expected '35313' -Actual $issMatches[0].Groups[1].Value

# Test 2: Revert commit detection
$subjRevert = '[release/10.0.1xx-sr7] Revert - Fix Changing Shell.NavBarIsVisible does not update (#35703)'
$bodyRevert = @"
This reverts commit abc1234def5678.
"@
$isRevert = ($subjRevert -match '(?i)^(?:\[[^\]]+\]\s+)?Revert\b') -or ($subjRevert -match '\[Revert\]')
Assert-Eq -Label "Detect Revert after [branch-prefix]" -Expected $true -Actual $isRevert

$revertedSha = [regex]::Match($bodyRevert, '(?im)This reverts commit\s+([0-9a-f]{7,40})')
Assert-Eq -Label "Extract reverted commit SHA" -Expected 'abc1234def5678' -Actual $revertedSha.Groups[1].Value

# Test 3: Plain "Revert " prefix
$subjRevertPlain = 'Revert "Fix some thing" (#35744)'
$isRevertPlain = ($subjRevertPlain -match '(?i)^(?:\[[^\]]+\]\s+)?Revert\b') -or ($subjRevertPlain -match '\[Revert\]')
Assert-Eq -Label "Detect 'Revert ' prefix" -Expected $true -Actual $isRevertPlain

# Test 3b: Bracketed [Revert] prefix
$subjBracketRevert = '[Revert] - [Windows] Fix WebView blank rendering (#35744)'
$isBracketRevert = ($subjBracketRevert -match '(?i)^(?:\[[^\]]+\]\s+)?Revert\b') -or ($subjBracketRevert -match '\[Revert\]')
Assert-Eq -Label "Detect '[Revert]' bracket form" -Expected $true -Actual $isBracketRevert

# Test 4: Non-fix PR body should not match closing-keyword
$nonFix = 'Adds a helper method. Mentions #12345 in passing.'
$closingMatch = $nonFix -match "(?im)(?:fixes|closes|resolves)\s+(?:dotnet/maui#|#)12345\b"
Assert-Eq -Label "Plain mention does not trigger closing-keyword" -Expected $false -Actual $closingMatch

# Test 5: Closing keyword case-insensitive + with "Fixes dotnet/maui#NNNN"
$crossRepoFix = 'Fixes dotnet/maui#9999'
$crFixMatch = $crossRepoFix -match "(?im)(?:fixes|closes|resolves)\s+(?:dotnet/maui#|#)9999\b"
Assert-Eq -Label "Cross-repo 'Fixes dotnet/maui#NNNN' matches" -Expected $true -Actual $crFixMatch

# Test 6: SR branch name parsing for label inference
$branchTest = 'release/10.0.1xx-sr7'
$brMatch = [regex]::Match($branchTest, '^release/(\d+)\.(\d+)\.\d+xx-sr(\d+)$')
Assert-Eq -Label "SR branch parses major.minor.sr#" -Expected '10,0,7' `
    -Actual "$($brMatch.Groups[1].Value),$($brMatch.Groups[2].Value),$($brMatch.Groups[3].Value)"

$badBranch = 'release/main-sr1-preview'
$badMatch = [regex]::Match($badBranch, '^release/(\d+)\.(\d+)\.\d+xx-sr(\d+)$')
Assert-Eq -Label "Non-standard branch name does NOT match" -Expected $false -Actual $badMatch.Success

# ─────────── SR-source validation rules (no network) ───────────

Write-Host "`n[Unit] SR-source branch validation rules" -ForegroundColor Cyan

# These patterns mirror $Script:ForbiddenSrPatterns in the script. If the
# script's rule list changes, this test list must be updated to match.
$forbidden = @('^inflight/', '^staging/', '^backport/')

foreach ($case in @(
    @{ Branch = 'inflight/current';     ShouldMatch = $true  ; Label = 'inflight/current is forbidden' }
    @{ Branch = 'inflight/candidate';   ShouldMatch = $true  ; Label = 'inflight/candidate is forbidden' }
    @{ Branch = 'staging/foo';          ShouldMatch = $true  ; Label = 'staging/* is forbidden' }
    @{ Branch = 'backport/pr-31149';    ShouldMatch = $true  ; Label = 'backport/* is forbidden' }
    @{ Branch = 'release/10.0.1xx-sr7'; ShouldMatch = $false ; Label = 'release/*-sr* is allowed' }
    @{ Branch = 'main';                 ShouldMatch = $false ; Label = 'main is allowed' }
)) {
    $hit = $false
    foreach ($p in $forbidden) {
        if ($case.Branch -match $p) { $hit = $true; break }
    }
    Assert-Eq -Label $case.Label -Expected $case.ShouldMatch -Actual $hit
}

# ─────────── E2E smoke test against SR7 ───────────

if (-not $SkipE2E) {
    Write-Host "`n[E2E] Smoke test against SR7 known-answer set" -ForegroundColor Cyan

    $scriptPath = Join-Path $PSScriptRoot '..' 'scripts' 'Get-ReleaseReadiness.ps1'
    $outDir = Join-Path ([System.IO.Path]::GetTempPath()) "release-readiness-test-$(Get-Date -Format 'yyyyMMddHHmmss')"

    # Test the SR commits + source PR phase only (fast: ~10s)
    Write-Host "  Running: -Phase commits..." -ForegroundColor Gray
    try {
        & pwsh -NoProfile -File $scriptPath `
            -SrBranch 'release/10.0.1xx-sr7' `
            -Phase commits `
            -OutputDir $outDir `
            -NoFetch 2>&1 | Out-Null
    } catch {
        Write-Host "  ❌ E2E script invocation failed: $_" -ForegroundColor Red
        $script:failed++
        return
    }

    $srcPrsFile = Join-Path $outDir 'sr-source-prs.txt'
    if (-not (Test-Path $srcPrsFile)) {
        Write-Host "  ❌ sr-source-prs.txt was not created" -ForegroundColor Red
        $script:failed++
    } else {
        $srcPrs = Get-Content $srcPrsFile
        # Expected: backport PR 35428 (Android #35313 fix backport) MUST be in the list
        $has35428 = $srcPrs -contains '35428'
        Assert-Eq -Label "SR7 source-PRs contains #35428 (Android #35313 backport)" `
                  -Expected $true -Actual $has35428

        # Expected: #35609 (iOS/Mac #35326 fix) was NOT backported — must NOT appear
        $has35609 = $srcPrs -contains '35609'
        Assert-Eq -Label "SR7 source-PRs does NOT contain #35609 (#35326 fix, not backported)" `
                  -Expected $false -Actual $has35609

        # Expected: count is in the right ballpark (we measured 54 manually)
        Write-Host "  Source PR count: $($srcPrs.Count) (expected ~50-60)" -ForegroundColor Gray
        Assert-Eq -Label "SR7 source-PR count in expected range" `
                  -Expected $true -Actual ($srcPrs.Count -ge 40 -and $srcPrs.Count -le 100)
    }

    # Cleanup
    if (Test-Path $outDir) { Remove-Item -Recurse -Force $outDir }

    # ─────────── -InheritFromPriorSr E2E: SR8-candidate-style ───────────
    Write-Host "`n[E2E] Candidate mode with -InheritFromPriorSr (SR8-style)" -ForegroundColor Cyan

    # Negative: -InheritFromPriorSr without -Candidate must throw
    $bogusOut = Join-Path ([System.IO.Path]::GetTempPath()) "rr-test-bogus-$(Get-Date -Format 'HHmmss')"
    $stderr = & pwsh -NoProfile -File $scriptPath `
        -SrBranch 'release/10.0.1xx-sr7' `
        -InheritFromPriorSr `
        -Phase commits `
        -OutputDir $bogusOut `
        -NoFetch 2>&1
    $threw = ($LASTEXITCODE -ne 0) -or ($stderr -match 'only valid with -Candidate')
    Assert-Eq -Label "-InheritFromPriorSr without -Candidate is rejected" `
              -Expected $true -Actual $threw
    if (Test-Path $bogusOut) { Remove-Item -Recurse -Force $bogusOut }

    # Positive: candidate mode + inheritance must produce a non-empty union
    $candOut = Join-Path ([System.IO.Path]::GetTempPath()) "rr-test-cand-$(Get-Date -Format 'HHmmss')"
    & pwsh -NoProfile -File $scriptPath `
        -SrBranch 'release/10.0.1xx-sr7' `
        -Candidate -InheritFromPriorSr `
        -Phase commits `
        -OutputDir $candOut `
        -NoFetch 2>&1 | Out-Null
    $candJson = Join-Path $candOut 'release-readiness.json'
    if (-not (Test-Path $candJson)) {
        Write-Host "  ❌ candidate JSON not created" -ForegroundColor Red
        $script:failed++
    } else {
        $cand = Get-Content $candJson -Raw | ConvertFrom-Json
        $sc = $cand.srContents
        # Inherited count must be > 0 (SR7 has commits not on main)
        Assert-Eq -Label "Inherited commit count > 0 when -InheritFromPriorSr is set" `
                  -Expected $true -Actual ($sc.inheritedCommitCount -gt 0)
        # Total source PRs must be >= primary alone
        Assert-Eq -Label "Total sourcePrs >= primarySourcePrs (union grows)" `
                  -Expected $true -Actual ($sc.sourcePrs.Count -ge $sc.primarySourcePrs.Count)
        # Metadata flag is persisted
        Assert-Eq -Label "metadata.inheritFromPriorSr is true" `
                  -Expected $true -Actual $cand.metadata.inheritFromPriorSr
        # The well-known SR7 backport (#35428) must appear in the union (it's in SR7-only)
        $hasInherited = $sc.sourcePrs -contains 35428
        Assert-Eq -Label "Union sourcePrs contains SR7-only backport #35428" `
                  -Expected $true -Actual $hasInherited
    }
    if (Test-Path $candOut) { Remove-Item -Recurse -Force $candOut }
}

Write-Host "`n────────────────────────────────────────" -ForegroundColor Cyan
Write-Host "Passed: $script:passed   Failed: $script:failed" -ForegroundColor $(if ($script:failed -eq 0) { 'Green' } else { 'Red' })
exit $(if ($script:failed -eq 0) { 0 } else { 1 })
