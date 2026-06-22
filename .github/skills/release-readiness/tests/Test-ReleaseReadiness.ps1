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
        # Hard-fail immediately: a bare `return` here exits at script scope and
        # bypasses the terminal `exit $(... $script:failed ...)`, so a crashed
        # script-under-test could still exit 0 (CI green). exit 1 is unambiguous.
        exit 1
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

# ─────────── Tracker detection algorithm (Find-ReleaseReadinessTrackers.ps1) ───────────

Write-Host "`n[Unit] Tracker detection regex contracts" -ForegroundColor Cyan

$detectScriptPath = Join-Path $PSScriptRoot '..' 'scripts' 'Find-ReleaseReadinessTrackers.ps1'
if (-not (Test-Path $detectScriptPath)) {
    Write-Host "  ❌ Find-ReleaseReadinessTrackers.ps1 missing at $detectScriptPath" -ForegroundColor Red
    $script:failed++
} else {
    # Dot-source to expose the strict regex constants (guarded against main execution)
    . $detectScriptPath

    $branchRegex = $Global:FindReleaseReadinessTrackers_StrictSrBranchRegex
    $tagRegex    = $Global:FindReleaseReadinessTrackers_StrictStableTagRegex

    # Branch acceptance — these MUST match
    foreach ($case in @(
        @{ Name = 'release/10.0.1xx-sr1';   Major = 10; Sr = 1 }
        @{ Name = 'release/10.0.1xx-sr7';   Major = 10; Sr = 7 }
        @{ Name = 'release/10.0.1xx-sr10';  Major = 10; Sr = 10 }
        @{ Name = 'release/9.0.1xx-sr9';    Major = 9;  Sr = 9 }
        @{ Name = 'release/11.0.2xx-sr1';   Major = 11; Sr = 1 }
    )) {
        $m = [regex]::Match($case.Name, $branchRegex)
        Assert-Eq -Label "strict regex accepts $($case.Name)" -Expected $true -Actual $m.Success
        if ($m.Success) {
            Assert-Eq -Label "  -> extracts major=$($case.Major)" -Expected $case.Major -Actual ([int]$m.Groups[1].Value)
            Assert-Eq -Label "  -> extracts sr=$($case.Sr)"        -Expected $case.Sr    -Actual ([int]$m.Groups[2].Value)
        }
    }

    # Branch rejection — these MUST NOT match (false positives the reviewers flagged)
    foreach ($name in @(
        'release/10.0.1xx-sr8-backup'    # backup suffix
        'release/10.0.1xx-sr10-test'     # test suffix
        'release/10.0.1xx-sr-next'       # non-numeric
        'release/10.0.1xx-sr8-old'       # old suffix
        'release/10.0.1xx-sr8-hotfix'    # hotfix suffix
        'release/10.0.1xx-srN'           # placeholder
        'release/10.0.1xx-sr8 '          # trailing whitespace
        'release/10.0.1xx-SR8'           # case-mismatch (regex is case-sensitive)
        'release/10.0.1xx'               # GA, not SR
        'release/10.0.1xx-preview7'      # preview
        'release/10.0.1xx-rc1'           # rc
        'inflight/current'               # integration ref
        'main'                           # not a release branch
        'feature/sr8'                    # not a release branch
    )) {
        $m = [regex]::Match($name, $branchRegex)
        Assert-Eq -Label "strict regex rejects $name" -Expected $false -Actual $m.Success
    }

    # sr08 (leading zero) is debatable - .NET tooling normalizes to sr8. The current
    # strict regex DOES accept "sr08" because \d+ doesn't forbid leading zeros. We
    # consider this acceptable: lane 1 will fetch the branch, classify it normally,
    # and the canonical key would be "net10-sr8" once parsed as [int]. If you need
    # to forbid the leading zero, tighten to `-sr([1-9]\d*)`.
    $sr08 = [regex]::Match('release/10.0.1xx-sr08', $branchRegex)
    Assert-Eq -Label "regex tolerates 'sr08' leading zero (parsed as int 8)" `
              -Expected 8 -Actual $(if ($sr08.Success) { [int]$sr08.Groups[2].Value } else { -1 })

    # Stable tag acceptance — these MUST match
    foreach ($case in @(
        @{ Name = '10.0.0';   Major = 10; Patch = 0 }
        @{ Name = '10.0.70';  Major = 10; Patch = 70 }
        @{ Name = '10.0.71';  Major = 10; Patch = 71 }
        @{ Name = '10.0.100'; Major = 10; Patch = 100 }
    )) {
        $m = [regex]::Match($case.Name, $tagRegex)
        Assert-Eq -Label "stable-tag regex accepts $($case.Name)" -Expected $true -Actual $m.Success
    }

    # Stable tag rejection — prerelease tags MUST be ignored when computing highest shipped
    foreach ($name in @(
        '11.0.0-preview.1.26107'
        '11.0.0-rc.1.25424.2'
        '10.0.71-rtm.123'
        '10.0.71-servicing'
        '10.0'              # missing patch
        '10.0.71.0'         # extra segment
    )) {
        $m = [regex]::Match($name, $tagRegex)
        Assert-Eq -Label "stable-tag regex rejects $name (prerelease/malformed)" -Expected $false -Actual $m.Success
    }

    # Regression label inference — exercise the helper
    Write-Host "`n[Unit] Tracker regression-label inference" -ForegroundColor Cyan
    foreach ($case in @(
        @{ Major = 10; Sr = 7;  Expected = @('regressed-in-10.0.60', 'regressed-in-10.0.70') }
        @{ Major = 10; Sr = 8;  Expected = @('regressed-in-10.0.70', 'regressed-in-10.0.80') }
        @{ Major = 10; Sr = 9;  Expected = @('regressed-in-10.0.80', 'regressed-in-10.0.90') }
        @{ Major = 10; Sr = 10; Expected = @('regressed-in-10.0.90', 'regressed-in-10.0.100') }
        @{ Major = 10; Sr = 1;  Expected = @('regressed-in-10.0.0',  'regressed-in-10.0.10') }
        @{ Major = 11; Sr = 1;  Expected = @('regressed-in-11.0.0',  'regressed-in-11.0.10') }
    )) {
        $actual = (New-RegressionLabelList -Major $case.Major -SrNumber $case.Sr) -join ','
        $expected = $case.Expected -join ','
        Assert-Eq -Label "regression labels for major=$($case.Major) sr=$($case.Sr)" `
                  -Expected $expected -Actual $actual
    }

    # ─────────── In-flight tag-existence check ───────────
    # The authoritative ship signal is the existence of the stable tag
    # `<major>.0.<patch>` (created when release notes publish). These tests
    # exercise the two helpers backing that rule.

    Write-Host "`n[Unit] Get-ShippedPatchSet" -ForegroundColor Cyan

    # Builds a HashSet[int] from a tag list, dropping prereleases and noise.
    $live10Tags = @(
        '10.0.0', '10.0.1', '10.0.10', '10.0.11', '10.0.20',
        '10.0.30', '10.0.31', '10.0.40', '10.0.41',
        '10.0.50', '10.0.51', '10.0.60', '10.0.70'
    )
    $set = Get-ShippedPatchSet -StableTags $live10Tags
    Assert-Eq -Label "set is HashSet[int]" `
              -Expected $true `
              -Actual ($set -is [System.Collections.Generic.HashSet[int]])
    Assert-Eq -Label "set count = 13 distinct shipped patches" -Expected 13 -Actual $set.Count
    Assert-Eq -Label "set contains shipped patch 70"  -Expected $true  -Actual $set.Contains(70)
    Assert-Eq -Label "set contains GA patch 0"        -Expected $true  -Actual $set.Contains(0)
    Assert-Eq -Label "set does NOT contain 71"        -Expected $false -Actual $set.Contains(71)
    Assert-Eq -Label "set does NOT contain 80"        -Expected $false -Actual $set.Contains(80)
    Assert-Eq -Label "set does NOT contain 90"        -Expected $false -Actual $set.Contains(90)

    # Prereleases must NOT count as shipped.
    $mixed = @('10.0.70', '10.0.71-rtm.123', '10.0.71-servicing', '11.0.0-preview.1.26107')
    $mixedSet = Get-ShippedPatchSet -StableTags $mixed
    Assert-Eq -Label "prerelease tags ignored: only stable 10.0.70 counts" -Expected 1 -Actual $mixedSet.Count
    Assert-Eq -Label "prerelease '10.0.71-rtm.123' does NOT mark 71 shipped" -Expected $false -Actual $mixedSet.Contains(71)

    # Edge cases.
    $emptySet = Get-ShippedPatchSet -StableTags @()
    Assert-Eq -Label "empty input -> empty set" -Expected 0 -Actual $emptySet.Count
    $nullSet = Get-ShippedPatchSet -StableTags $null
    Assert-Eq -Label "null input -> empty set"  -Expected 0 -Actual $nullSet.Count

    # Duplicate tags collapse (HashSet semantics).
    $dupSet = Get-ShippedPatchSet -StableTags @('10.0.70', '10.0.70', '10.0.71')
    Assert-Eq -Label "duplicate tags collapse"  -Expected 2 -Actual $dupSet.Count

    Write-Host "`n[Unit] Test-IsBranchInFlight" -ForegroundColor Cyan

    # The current live state: SR7 (patch 71) and SR8 (patch 80) are in-flight,
    # SR6 (patch 60) is already shipped.
    Assert-Eq -Label "SR6 patch 60 — tag 10.0.60 exists -> shipped"     -Expected $false -Actual (Test-IsBranchInFlight -BranchPatch 60 -ShippedPatches $set)
    Assert-Eq -Label "SR7 patch 71 — tag 10.0.71 missing -> in-flight"  -Expected $true  -Actual (Test-IsBranchInFlight -BranchPatch 71 -ShippedPatches $set)
    Assert-Eq -Label "SR8 patch 80 — tag 10.0.80 missing -> in-flight"  -Expected $true  -Actual (Test-IsBranchInFlight -BranchPatch 80 -ShippedPatches $set)

    # A second-patch ship in an SR family (10.0.31 → SR3 already shipped twice).
    Assert-Eq -Label "patch 31 — tag 10.0.31 exists -> shipped"         -Expected $false -Actual (Test-IsBranchInFlight -BranchPatch 31 -ShippedPatches $set)
    Assert-Eq -Label "patch 32 — tag 10.0.32 missing -> in-flight (hypothetical SR3 hotfix branch)" `
              -Expected $true  -Actual (Test-IsBranchInFlight -BranchPatch 32 -ShippedPatches $set)

    # Out-of-order ship scenario: tag for SR8 (80) exists but not for SR7 (71).
    # New tag-based rule must still mark SR7 in-flight; the old highest-shipped
    # comparison would have wrongly classified it as shipped.
    $outOfOrder = Get-ShippedPatchSet -StableTags @('10.0.0', '10.0.60', '10.0.80')
    Assert-Eq -Label "out-of-order: SR7 patch 71 still in-flight even though 80 shipped" `
              -Expected $true  -Actual (Test-IsBranchInFlight -BranchPatch 71 -ShippedPatches $outOfOrder)
    Assert-Eq -Label "out-of-order: SR8 patch 80 correctly shipped"     -Expected $false -Actual (Test-IsBranchInFlight -BranchPatch 80 -ShippedPatches $outOfOrder)

    # Hotfix branch resetting PatchVersion below highest known patch.
    # Example: SR2 branch bumped back to patch 22 to prepare a security
    # release after SR7 already shipped. Tag 10.0.22 doesn't exist → in-flight.
    $hotfix = Get-ShippedPatchSet -StableTags @('10.0.0', '10.0.20', '10.0.70')
    Assert-Eq -Label "hotfix: SR2 patch 22 still in-flight when latest shipped is 70" `
              -Expected $true  -Actual (Test-IsBranchInFlight -BranchPatch 22 -ShippedPatches $hotfix)
    Assert-Eq -Label "hotfix: SR2 patch 20 is the already-shipped baseline"  -Expected $false -Actual (Test-IsBranchInFlight -BranchPatch 20 -ShippedPatches $hotfix)

    # Empty ship set: every branch must be in-flight.
    Assert-Eq -Label "no shipped tags yet: patch 0 (GA) in-flight"  -Expected $true -Actual (Test-IsBranchInFlight -BranchPatch 0  -ShippedPatches $emptySet)
    Assert-Eq -Label "no shipped tags yet: patch 11 in-flight"      -Expected $true -Actual (Test-IsBranchInFlight -BranchPatch 11 -ShippedPatches $emptySet)

    # ─────────── Test-IsStaleSrBranch (Lane 1 staleness guard) ───────────
    # Secondary disambiguator that runs AFTER Test-IsBranchInFlight returns true.
    # Drops tag-absent SR branches that sit below the shipped watermark AND are
    # idle — e.g. SR2 (patch 21) / SR3 (patch 33) lingering long after SR7
    # (patch 71) shipped — so they don't spin up no-op workflow matrix jobs.
    Write-Host "`n[Unit] Test-IsStaleSrBranch (Lane 1 staleness guard)" -ForegroundColor Cyan

    # The reported case: stale below-watermark branches with no recent commits.
    Assert-Eq -Label "SR2 patch 21 < 71, idle -> stale (skip)" `
              -Expected $true  -Actual (Test-IsStaleSrBranch -BranchPatch 21 -HighestShippedPatch 71 -RecentActivityCount 0)
    Assert-Eq -Label "SR3 patch 33 < 71, idle -> stale (skip)" `
              -Expected $true  -Actual (Test-IsStaleSrBranch -BranchPatch 33 -HighestShippedPatch 71 -RecentActivityCount 0)

    # A freshly-cut live SR sits at/above the watermark — never stale, even idle.
    Assert-Eq -Label "SR8 patch 80 > 71, idle -> NOT stale (above watermark)" `
              -Expected $false -Actual (Test-IsStaleSrBranch -BranchPatch 80 -HighestShippedPatch 71 -RecentActivityCount 0)
    Assert-Eq -Label "patch 71 == 71, idle -> NOT stale (equal, not strictly below)" `
              -Expected $false -Actual (Test-IsStaleSrBranch -BranchPatch 71 -HighestShippedPatch 71 -RecentActivityCount 0)

    # The hotfix scenario tag-existence protects: a reset branch BELOW the
    # watermark but with recent commits is genuinely in-flight, NOT stale.
    Assert-Eq -Label "security-hotfix patch 22 < 71 but active -> NOT stale" `
              -Expected $false -Actual (Test-IsStaleSrBranch -BranchPatch 22 -HighestShippedPatch 71 -RecentActivityCount 3)
    Assert-Eq -Label "below-watermark patch 21 with 1 recent commit -> NOT stale" `
              -Expected $false -Actual (Test-IsStaleSrBranch -BranchPatch 21 -HighestShippedPatch 71 -RecentActivityCount 1)

    # No shipped tags yet (highest = 0): nothing is below the watermark, so the
    # guard never fires — every in-flight branch is preserved.
    Assert-Eq -Label "no shipped tags (highest 0): patch 11 idle -> NOT stale" `
              -Expected $false -Actual (Test-IsStaleSrBranch -BranchPatch 11 -HighestShippedPatch 0 -RecentActivityCount 0)

    # ─────────── Preview-tag regex contract ───────────
    Write-Host "`n[Unit] Preview tag regex (<major>.0.0-preview.<N>.<date>[.<build>])" -ForegroundColor Cyan
    $previewTagCases = @(
        @{ Tag = '11.0.0-preview.5.26304.4';  Match = $true;  Major = 11; PreviewN = 5 }   # GA preview with build
        @{ Tag = '11.0.0-preview.1.26107';    Match = $true;  Major = 11; PreviewN = 1 }   # GA preview without build suffix
        @{ Tag = '10.0.0-preview.7.25406.3';  Match = $true;  Major = 10; PreviewN = 7 }   # net10 preview7
        @{ Tag = '11.0.0-preview.10.26999';   Match = $true;  Major = 11; PreviewN = 10 }  # double-digit preview
        @{ Tag = '10.0.70';                   Match = $false }                              # stable tag should NOT match
        @{ Tag = '11.0.0-rc.1.26404.4';       Match = $false }                              # rc, not preview
        @{ Tag = '11.0.0-preview.5';          Match = $false }                              # missing date
        @{ Tag = '11.0.0-preview.5.26304x';   Match = $false }                              # garbage suffix
    )
    foreach ($case in $previewTagCases) {
        $m = [regex]::Match($case.Tag, $Script:StrictPreviewTagRegex)
        Assert-Eq -Label "tag '$($case.Tag)' match=$($case.Match)" -Expected $case.Match -Actual $m.Success
        if ($case.Match) {
            Assert-Eq -Label "  -> major=$($case.Major)"       -Expected $case.Major    -Actual ([int]$m.Groups[1].Value)
            Assert-Eq -Label "  -> previewN=$($case.PreviewN)" -Expected $case.PreviewN -Actual ([int]$m.Groups[2].Value)
        }
    }

    # ─────────── Preview-branch regex contract ───────────
    Write-Host "`n[Unit] Preview branch regex (release/<major>.0.<patchband>xx-preview<N>)" -ForegroundColor Cyan
    $previewBranchCases = @(
        @{ Branch = 'release/11.0.1xx-preview6';   Match = $true;  Major = 11; PreviewN = 6 }
        @{ Branch = 'release/10.0.1xx-preview7';   Match = $true;  Major = 10; PreviewN = 7 }
        @{ Branch = 'release/11.0.1xx-preview10';  Match = $true;  Major = 11; PreviewN = 10 }
        @{ Branch = 'release/10.0.1xx-sr7';        Match = $false }                                # SR branch must NOT match preview
        @{ Branch = 'release/11.0.1xx-preview6.1'; Match = $false }                                # no dotted suffix
        @{ Branch = 'release/11.0.1xx-previewa';   Match = $false }                                # preview number must be digits
        @{ Branch = 'release/11.0.1xx-preview6/x'; Match = $false }                                # no trailing path
        @{ Branch = 'release/11.0.0-preview6';     Match = $false }                                # missing patch band
    )
    foreach ($case in $previewBranchCases) {
        $m = [regex]::Match($case.Branch, $Script:StrictPreviewBranchRegex)
        Assert-Eq -Label "branch '$($case.Branch)' match=$($case.Match)" -Expected $case.Match -Actual $m.Success
        if ($case.Match) {
            Assert-Eq -Label "  -> major=$($case.Major)"       -Expected $case.Major    -Actual ([int]$m.Groups[1].Value)
            Assert-Eq -Label "  -> previewN=$($case.PreviewN)" -Expected $case.PreviewN -Actual ([int]$m.Groups[2].Value)
        }
    }

    # ─────────── Preview regression label inference ───────────
    Write-Host "`n[Unit] Preview regression-label inference" -ForegroundColor Cyan
    foreach ($case in @(
        @{ Major = 11; Preview = 1; Expected = @('regressed-in-11.0.0-preview1') }                              # preview1 has only its own label
        @{ Major = 11; Preview = 6; Expected = @('regressed-in-11.0.0-preview5', 'regressed-in-11.0.0-preview6') }
        @{ Major = 12; Preview = 3; Expected = @('regressed-in-12.0.0-preview2', 'regressed-in-12.0.0-preview3') }
    )) {
        $actual = (New-PreviewRegressionLabelList -Major $case.Major -PreviewNumber $case.Preview) -join ','
        $expected = $case.Expected -join ','
        Assert-Eq -Label "preview labels for major=$($case.Major) preview=$($case.Preview)" `
                  -Expected $expected -Actual $actual
    }

    # ─────────── Get-ShippedPreviewSet ───────────
    Write-Host "`n[Unit] Get-ShippedPreviewSet" -ForegroundColor Cyan
    $live11Previews = @(
        '11.0.0-preview.1.26107',
        '11.0.0-preview.2.26152.10',
        '11.0.0-preview.3.26203.7',
        '11.0.0-preview.4.26230.3',
        '11.0.0-preview.5.26304.4'
    )
    $previewSet = Get-ShippedPreviewSet -PreviewTags $live11Previews
    Assert-Eq -Label "preview set is HashSet[int]" `
              -Expected $true `
              -Actual ($previewSet -is [System.Collections.Generic.HashSet[int]])
    Assert-Eq -Label "preview set count = 5"                   -Expected 5     -Actual $previewSet.Count
    Assert-Eq -Label "preview set contains 5"                  -Expected $true -Actual $previewSet.Contains(5)
    Assert-Eq -Label "preview set does NOT contain 6"          -Expected $false -Actual $previewSet.Contains(6)

    # Multiple tags for the same preview N collapse (preview3 had 3 ship-day candidates in practice).
    $multiTag = @('11.0.0-preview.5.26301.1', '11.0.0-preview.5.26304.4', '11.0.0-preview.6.26350.0')
    $multiSet = Get-ShippedPreviewSet -PreviewTags $multiTag
    Assert-Eq -Label "multiple tags for same preview N collapse" -Expected 2 -Actual $multiSet.Count
    Assert-Eq -Label "multi: contains 5"                          -Expected $true -Actual $multiSet.Contains(5)
    Assert-Eq -Label "multi: contains 6"                          -Expected $true -Actual $multiSet.Contains(6)

    # Stable tags must not pollute preview set.
    $stableMix = @('10.0.70', '11.0.0', '11.0.0-preview.5.26304.4')
    $mixSet = Get-ShippedPreviewSet -PreviewTags $stableMix
    Assert-Eq -Label "stable tags ignored by preview set" -Expected 1     -Actual $mixSet.Count
    Assert-Eq -Label "preview set only contains 5"         -Expected $true -Actual $mixSet.Contains(5)

    # Empty/null inputs.
    $emptyPreviewSet = Get-ShippedPreviewSet -PreviewTags @()
    Assert-Eq -Label "empty preview input -> empty set" -Expected 0 -Actual $emptyPreviewSet.Count
    $nullPreviewSet = Get-ShippedPreviewSet -PreviewTags $null
    Assert-Eq -Label "null preview input -> empty set"  -Expected 0 -Actual $nullPreviewSet.Count

    # ─────────── Test-IsPreviewBranchInFlight ───────────
    Write-Host "`n[Unit] Test-IsPreviewBranchInFlight" -ForegroundColor Cyan
    # Live state: net11 has previews 1–5 shipped; preview6 is the in-flight candidate.
    Assert-Eq -Label "preview1 (tag exists) -> NOT in-flight" -Expected $false -Actual (Test-IsPreviewBranchInFlight -PreviewNumber 1 -ShippedPreviews $previewSet)
    Assert-Eq -Label "preview5 (tag exists) -> NOT in-flight" -Expected $false -Actual (Test-IsPreviewBranchInFlight -PreviewNumber 5 -ShippedPreviews $previewSet)
    Assert-Eq -Label "preview6 (no tag)     -> in-flight"     -Expected $true  -Actual (Test-IsPreviewBranchInFlight -PreviewNumber 6 -ShippedPreviews $previewSet)
    Assert-Eq -Label "preview7 (no tag)     -> in-flight"     -Expected $true  -Actual (Test-IsPreviewBranchInFlight -PreviewNumber 7 -ShippedPreviews $previewSet)

    # Empty shipped set: every preview is in-flight.
    Assert-Eq -Label "no shipped previews: preview1 in-flight"  -Expected $true -Actual (Test-IsPreviewBranchInFlight -PreviewNumber 1 -ShippedPreviews $emptyPreviewSet)
    Assert-Eq -Label "no shipped previews: preview20 in-flight" -Expected $true -Actual (Test-IsPreviewBranchInFlight -PreviewNumber 20 -ShippedPreviews $emptyPreviewSet)
}

# ─────────── E2E: Run detection against this repo and validate trackers ───────────

if (-not $SkipE2E) {
    Write-Host "`n[E2E] Detection against live repo" -ForegroundColor Cyan
    Write-Host "  Under the tag-existence rule + Lane 1 staleness guard we expect TWO trackers:" -ForegroundColor DarkGray
    Write-Host "    - SR8 (patch=80, no tag 10.0.80)        - in-flight, active" -ForegroundColor DarkGray
    Write-Host "    - SR9 (candidate off main)              - active" -ForegroundColor DarkGray
    Write-Host "    DROPPED by the staleness guard (idle + below the shipped watermark 71):" -ForegroundColor DarkGray
    Write-Host "    - SR2 (patch=21, no tag 10.0.21)        - tag-absent but stale -> no matrix job" -ForegroundColor DarkGray
    Write-Host "    - SR3 (patch=33, no tag 10.0.33)        - tag-absent but stale -> no matrix job" -ForegroundColor DarkGray
    Write-Host "    NOTE: SR7 shipped 2026-06-05 (tag 10.0.71); no longer produces a tracker." -ForegroundColor DarkGray

    $detectOut = Join-Path ([System.IO.Path]::GetTempPath()) "rr-detect-$(Get-Date -Format 'HHmmss').json"
    try {
        & pwsh -NoProfile -File $detectScriptPath -NoFetch -OutputJson $detectOut 2>&1 | Out-Null
        if (-not (Test-Path $detectOut)) {
            Write-Host "  ❌ detection JSON not created" -ForegroundColor Red
            $script:failed++
        } else {
            $detected = Get-Content $detectOut -Raw | ConvertFrom-Json

            Assert-Eq -Label "majorVersion is 10"             -Expected 10 -Actual $detected.majorVersion
            Assert-Eq -Label "mainBranch is 'main'"            -Expected 'main' -Actual $detected.mainBranch
            Assert-Eq -Label "highestShippedTag is '10.0.71'"  -Expected '10.0.71' -Actual $detected.highestShippedTag
            Assert-Eq -Label "highestShippedPreviewTag carries net10's last preview" `
                      -Expected '10.0.0-preview.7.25406.3' -Actual $detected.highestShippedPreviewTag
            Assert-Eq -Label "tracker count is 2 (SR8+SR9 — SR7 shipped; SR2/SR3 dropped as stale)" `
                      -Expected 2 -Actual $detected.trackers.Count
            # All trackers in single-major net10 mode must be SR-flavored. (Net10's
            # previews 1–7 all shipped + no in-flight preview branch -> no preview tracker.)
            foreach ($t in $detected.trackers) {
                Assert-Eq -Label "tracker '$($t.canonicalKey)' has branchType='sr'" `
                          -Expected 'sr' -Actual $t.branchType
            }

            $bySr = @{}
            foreach ($t in $detected.trackers) { $bySr[[int]$t.srNumber] = $t }

            # SR2 (tag-absent but STALE — Lane 1 staleness guard drops it so the
            # workflow matrix never spins up a no-op job for it).
            Assert-Eq -Label "SR2 tracker absent (stale: patch 21 < 71, no recent activity)" `
                      -Expected $false -Actual ($bySr.ContainsKey(2))

            # SR3 (tag-absent but STALE — dropped by the staleness guard)
            Assert-Eq -Label "SR3 tracker absent (stale: patch 33 < 71, no recent activity)" `
                      -Expected $false -Actual ($bySr.ContainsKey(3))

            # SR7 (shipped 2026-06-05 as 10.0.71 — Lane 1 should NOT emit a tracker)
            Assert-Eq -Label "SR7 tracker absent (shipped)" `
                      -Expected $false -Actual ($bySr.ContainsKey(7))

            # SR8 (in-flight, ACTIVE)
            if ($bySr.ContainsKey(8)) {
                $sr8 = $bySr[8]
                Assert-Eq -Label "SR8 mode = in-flight"         -Expected 'in-flight' -Actual $sr8.mode
                Assert-Eq -Label "SR8 canonicalKey"             -Expected 'net10-sr8' -Actual $sr8.canonicalKey
                Assert-Eq -Label "SR8 branchName"               -Expected 'release/10.0.1xx-sr8' -Actual $sr8.branchName
                Assert-Eq -Label "SR8 branchExists = true"      -Expected $true -Actual $sr8.branchExists
                Assert-Eq -Label "SR8 expectedTag = 10.0.80"    -Expected '10.0.80' -Actual $sr8.expectedTag
                Assert-Eq -Label "SR8 hasRecentActivity = true" -Expected $true -Actual $sr8.hasRecentActivity
                Assert-Eq -Label "SR8 regression labels"        `
                          -Expected 'regressed-in-10.0.70,regressed-in-10.0.80' `
                          -Actual ($sr8.regressionLabels -join ',')
            } else {
                Write-Host "  ❌ SR8 tracker missing" -ForegroundColor Red; $script:failed++
            }

            # SR9 (candidate from main, ACTIVE)
            if ($bySr.ContainsKey(9)) {
                $sr9 = $bySr[9]
                Assert-Eq -Label "SR9 mode = candidate"         -Expected 'candidate' -Actual $sr9.mode
                Assert-Eq -Label "SR9 canonicalKey"             -Expected 'net10-sr9' -Actual $sr9.canonicalKey
                Assert-Eq -Label "SR9 branchName = canonical proposed slug" `
                          -Expected 'release/10.0.1xx-sr9' -Actual $sr9.branchName
                Assert-Eq -Label "SR9 branchExists = false (not cut yet)" `
                          -Expected $false -Actual $sr9.branchExists
                Assert-Eq -Label "SR9 surveyRef = main"         -Expected 'main' -Actual $sr9.surveyRef
                Assert-Eq -Label "SR9 priorSrBranch = SR8 branch" `
                          -Expected 'release/10.0.1xx-sr8' -Actual $sr9.priorSrBranch
                Assert-Eq -Label "SR9 expectedPatch = 90"       -Expected 90 -Actual $sr9.expectedPatch
                Assert-Eq -Label "SR9 hasRecentActivity = true" -Expected $true -Actual $sr9.hasRecentActivity
                Assert-Eq -Label "SR9 regression labels"        `
                          -Expected 'regressed-in-10.0.80,regressed-in-10.0.90' `
                          -Actual ($sr9.regressionLabels -join ',')
            } else {
                Write-Host "  ❌ SR9 tracker missing" -ForegroundColor Red; $script:failed++
            }

            # Active SRs (the ones the workflow will actually post) all have activity.
            # SR7 shipped 2026-06-05 (no longer in the tracker set); only SR8 + SR9 are active.
            foreach ($srNum in @(8, 9)) {
                if ($bySr.ContainsKey($srNum)) {
                    Assert-Eq -Label "SR$srNum hasRecentActivity == true (active SR)" `
                              -Expected $true -Actual $bySr[$srNum].hasRecentActivity
                }
            }
        }
    } finally {
        if (Test-Path $detectOut) { Remove-Item -Force $detectOut }
    }

    # ──────────── E2E: -AllActiveMajors multi-major envelope ────────────
    # In the unified post-consolidation shape, one invocation must surface every
    # active major (main's + any net<N>.0 ≥ main). Expected current state:
    #   - net10 -> 2 SR trackers (SR8, SR9), no preview tracker
    #     (SR7 shipped 2026-06-05; SR2/SR3 dropped by the Lane 1 staleness guard;
    #      every net10 preview branch already shipped + net10.0 isn't in preview cycle)
    #   - net11 -> 0 SR trackers (pre-GA: no `11.0.0` tag), 1 preview tracker
    #     (preview6 candidate from net11.0)
    Write-Host "`n[E2E] Detection with -AllActiveMajors" -ForegroundColor Cyan
    Write-Host "  Expected:" -ForegroundColor DarkGray
    Write-Host "    - majors[].length = 2 (net10 + net11)" -ForegroundColor DarkGray
    Write-Host "    - net10 trackers: 2 SR (sr8/sr9), 0 preview (SR7 shipped 2026-06-05; SR2/SR3 stale-dropped)" -ForegroundColor DarkGray
    Write-Host "    - net11 trackers: 0 SR (pre-GA), 1 preview (preview6 candidate from net11.0)" -ForegroundColor DarkGray

    $multiOut = Join-Path ([System.IO.Path]::GetTempPath()) "rr-detect-allmajors-$(Get-Date -Format 'HHmmss').json"
    try {
        & pwsh -NoProfile -File $detectScriptPath -NoFetch -AllActiveMajors -OutputJson $multiOut 2>&1 | Out-Null
        if (-not (Test-Path $multiOut)) {
            Write-Host "  ❌ allmajors detection JSON not created" -ForegroundColor Red; $script:failed++
        } else {
            $multi = Get-Content $multiOut -Raw | ConvertFrom-Json
            Assert-Eq -Label "AllActiveMajors envelope has no top-level trackers" `
                      -Expected $false -Actual ($multi.PSObject.Properties.Name -contains 'trackers')
            Assert-Eq -Label "AllActiveMajors envelope has top-level majors[]" `
                      -Expected $true  -Actual ($multi.PSObject.Properties.Name -contains 'majors')
            Assert-Eq -Label "majors[] contains exactly 2 entries (net10 + net11)" `
                      -Expected 2      -Actual $multi.majors.Count

            $byMajor = @{}
            foreach ($m in $multi.majors) { $byMajor[[int]$m.majorVersion] = $m }

            # net10 — same as single-major run, all SR trackers.
            if ($byMajor.ContainsKey(10)) {
                $net10 = $byMajor[10]
                Assert-Eq -Label "net10 mainBranch is 'main'"               -Expected 'main' -Actual $net10.mainBranch
                Assert-Eq -Label "net10 highestShippedTag is '10.0.71'"      -Expected '10.0.71' -Actual $net10.highestShippedTag
                Assert-Eq -Label "net10 tracker count is 2 (no preview lane, SR7 shipped, SR2/SR3 stale-dropped)" -Expected 2 -Actual $net10.trackers.Count
                $srCount = @($net10.trackers | Where-Object branchType -eq 'sr').Count
                $previewCount = @($net10.trackers | Where-Object branchType -eq 'preview').Count
                Assert-Eq -Label "net10 has 2 SR trackers"      -Expected 2 -Actual $srCount
                Assert-Eq -Label "net10 has 0 preview trackers" -Expected 0 -Actual $previewCount
            } else {
                Write-Host "  ❌ majors[] missing net10 entry" -ForegroundColor Red; $script:failed++
            }

            # net11 — pre-GA: no SR trackers; expects preview6 candidate from net11.0.
            if ($byMajor.ContainsKey(11)) {
                $net11 = $byMajor[11]
                Assert-Eq -Label "net11 mainBranch is 'net11.0'"          -Expected 'net11.0' -Actual $net11.mainBranch
                Assert-Eq -Label "net11 highestShippedTag is null (pre-GA)" -Expected $true -Actual ([string]::IsNullOrEmpty($net11.highestShippedTag))
                Assert-Eq -Label "net11 highestShippedPreviewTag carries preview5 tag" `
                          -Expected '11.0.0-preview.5.26304.4' -Actual $net11.highestShippedPreviewTag
                Assert-Eq -Label "net11 tracker count is 1 (preview6 only)" -Expected 1 -Actual $net11.trackers.Count
                $previewTrackers = @($net11.trackers | Where-Object branchType -eq 'preview')
                Assert-Eq -Label "net11 has 1 preview tracker"            -Expected 1 -Actual $previewTrackers.Count
                $srTrackers = @($net11.trackers | Where-Object branchType -eq 'sr')
                Assert-Eq -Label "net11 has 0 SR trackers (pre-GA -> Lane 2 skipped)" -Expected 0 -Actual $srTrackers.Count

                $preview6 = $previewTrackers[0]
                Assert-Eq -Label "preview6 canonicalKey"              -Expected 'net11-preview6'       -Actual $preview6.canonicalKey
                Assert-Eq -Label "preview6 mode = candidate"          -Expected 'candidate'           -Actual $preview6.mode
                Assert-Eq -Label "preview6 surveyRef = net11.0"       -Expected 'net11.0'             -Actual $preview6.surveyRef
                Assert-Eq -Label "preview6 expectedTagPrefix"         -Expected '11.0.0-preview.6.'   -Actual $preview6.expectedTagPrefix
                Assert-Eq -Label "preview6 previewNumber = 6"         -Expected 6                      -Actual $preview6.previewNumber
                Assert-Eq -Label "preview6 milestone name"            -Expected '.NET 11.0-preview6'  -Actual $preview6.milestoneName
                Assert-Eq -Label "preview6 issue title format"        `
                          -Expected '[Release Readiness] .NET 11.0 preview6 — candidate from net11.0' `
                          -Actual $preview6.issueTitle
                Assert-Eq -Label "preview6 branchName = canonical proposed slug" `
                          -Expected 'release/11.0.1xx-preview6' -Actual $preview6.branchName
                Assert-Eq -Label "preview6 branchExists = false (no branch yet)" `
                          -Expected $false -Actual $preview6.branchExists
                Assert-Eq -Label "preview6 hasRecentActivity = true (active preview cycle)" `
                          -Expected $true -Actual $preview6.hasRecentActivity
                Assert-Eq -Label "preview6 regressionLabels carries previewN-1 + previewN" `
                          -Expected 'regressed-in-11.0.0-preview5,regressed-in-11.0.0-preview6' `
                          -Actual ($preview6.regressionLabels -join ',')
            } else {
                Write-Host "  ❌ majors[] missing net11 entry" -ForegroundColor Red; $script:failed++
            }
        }
    } finally {
        if (Test-Path $multiOut) { Remove-Item -Force $multiOut }
    }

    # Fail-closed: bad repo path should exit non-zero
    Write-Host "`n[E2E] Detection fails closed on invalid repo" -ForegroundColor Cyan
    $badRepoOut = Join-Path ([System.IO.Path]::GetTempPath()) "rr-detect-badrepo-$(Get-Date -Format 'HHmmss').json"
    $badRepoPath = Join-Path ([System.IO.Path]::GetTempPath()) "rr-detect-non-git-$(Get-Date -Format 'HHmmss')"
    try {
        New-Item -ItemType Directory -Path $badRepoPath -Force | Out-Null
        & pwsh -NoProfile -File $detectScriptPath -NoFetch -Repo $badRepoPath -OutputJson $badRepoOut 2>&1 | Out-Null
        $exit = $LASTEXITCODE
        $jsonCreated = Test-Path $badRepoOut
        Assert-Eq -Label "exits non-zero on non-git path"           -Expected $true -Actual ($exit -ne 0)
        Assert-Eq -Label "does not write JSON on failure (fail-closed)" -Expected $false -Actual $jsonCreated
    } finally {
        if (Test-Path $badRepoOut) { Remove-Item -Force $badRepoOut }
        if (Test-Path $badRepoPath) { Remove-Item -Recurse -Force $badRepoPath }
    }
}

# ─────────── Unit tests for Get-ReleaseReadiness internals ───────────
# Dot-source the script in test mode so we can call individual functions
# without invoking the full orchestrator (which requires git + gh + network).
# The TEST_MODE env var short-circuits Invoke-Main at the bottom of the script.
$env:GET_RELEASE_READINESS_TEST_MODE = '1'
try {
    $rrScript = Join-Path $PSScriptRoot '..' 'scripts' 'Get-ReleaseReadiness.ps1'
    # Dot-source needs to satisfy [Parameter(Mandatory)] for $SrBranch; pass a dummy.
    . $rrScript -SrBranch 'release/10.0.1xx-sr1'
} finally {
    Remove-Item -Path Env:GET_RELEASE_READINESS_TEST_MODE -ErrorAction SilentlyContinue
}

# ───── Get-RevertedPrFromSubject (revert false-green guard) ─────
Write-Host "`n[Unit] Get-RevertedPrFromSubject (revert classification)" -ForegroundColor Cyan

# The reverted-PR must be the ORIGINAL fix, NOT the revert's own trailing (#N).
# GitHub's revert subject is  Revert "Title (#1234)" (#5678)  — 1234 is the
# reverted fix, 5678 is the revert PR. A greedy pattern previously captured 5678,
# which skipped the SHA-lookup fallback and flipped a reverted regression fix to
# in-sr-active ("ready to ship") instead of in-sr-reverted.
Assert-Eq -Label "Reverted-PR from quoted title returns inner #, not trailing revert #" `
    -Expected 1234 -Actual (Get-RevertedPrFromSubject -Subject 'Revert "Some fix (#1234)" (#5678)')
Assert-Eq -Label "Reverted-PR from branch-prefixed quoted revert" `
    -Expected 35313 -Actual (Get-RevertedPrFromSubject -Subject '[release/10.0.1xx-sr8] Revert "Fix CollectionView (#35313)" (#35804)')
Assert-Eq -Label "Reverted-PR from explicit 'Revert PR #NNNN'" `
    -Expected 35428 -Actual (Get-RevertedPrFromSubject -Subject 'Revert PR #35428 - broke iOS')
Assert-Eq -Label "Revert subject with no inner (#N) yields null (no false reverted-PR)" `
    -Expected $null -Actual (Get-RevertedPrFromSubject -Subject 'Revert "Fix some thing" (#35744)')
Assert-Eq -Label "Non-revert subject yields null" `
    -Expected $null -Actual (Get-RevertedPrFromSubject -Subject '[Android] Fix layout pass (#35900)')
# Internal quotes in the original title must not truncate the match. The old
# [^"]* pattern stopped at the first inner quote and returned null.
Assert-Eq -Label "Reverted-PR from quoted title containing internal quotes" `
    -Expected 1234 -Actual (Get-RevertedPrFromSubject -Subject 'Revert "Fix "weird" bug (#1234)" (#5678)')
# Case-insensitive: a hand-typed lowercase 'revert "..."' subject must resolve.
Assert-Eq -Label "Reverted-PR from lowercase 'revert' subject" `
    -Expected 4321 -Actual (Get-RevertedPrFromSubject -Subject 'revert "fix thing (#4321)" (#8765)')

# ───── Test-PrIsToolingOnly (false-positive guard #1) ─────
Write-Host "`n[Unit] Test-PrIsToolingOnly (FP guard)" -ForegroundColor Cyan

# Self-reference case: a workflow/skill PR that mentions a regression issue
$toolingOnlyFiles = @(
    @{ path = '.github/workflows/foo.yml'; additions = 10; deletions = 0 }
    @{ path = '.github/skills/release-readiness/SKILL.md'; additions = 5; deletions = 0 }
    @{ path = 'docs/release-readiness.md'; additions = 3; deletions = 0 }
    @{ path = 'eng/scripts/helper.ps1'; additions = 20; deletions = 0 }
    @{ path = 'README.md'; additions = 1; deletions = 0 }
)
Assert-Eq -Label "tooling-only PR (workflows + docs + scripts)" -Expected $true `
    -Actual (Test-PrIsToolingOnly -Files $toolingOnlyFiles)

# Real fix: at least one product file
$realFixFiles = @(
    @{ path = 'src/Controls/src/Core/Button.cs'; additions = 20; deletions = 5 }
    @{ path = '.github/workflows/foo.yml'; additions = 2; deletions = 0 }   # mixed
)
Assert-Eq -Label "real fix PR (src/ + .github/ mixed) is NOT tooling-only" -Expected $false `
    -Actual (Test-PrIsToolingOnly -Files $realFixFiles)

# Pure src changes
$srcOnlyFiles = @(
    @{ path = 'src/Core/src/Layouts/StackLayout.cs'; additions = 50; deletions = 10 }
    @{ path = 'src/Core/tests/UnitTests/StackLayoutTests.cs'; additions = 25; deletions = 0 }
)
Assert-Eq -Label "src-only PR is NOT tooling-only" -Expected $false `
    -Actual (Test-PrIsToolingOnly -Files $srcOnlyFiles)

# Empty/null files: indeterminate → return false (don't accidentally skip)
Assert-Eq -Label "null file list returns false (cannot decide → leave alone)" -Expected $false `
    -Actual (Test-PrIsToolingOnly -Files $null)
Assert-Eq -Label "empty file list returns false (cannot decide → leave alone)" -Expected $false `
    -Actual (Test-PrIsToolingOnly -Files @())

# Edge: file with null path is ignored (count of valid files = 0 → false)
$weirdFiles = @( @{ path = $null; additions = 1 }, @{ path = ''; additions = 1 } )
Assert-Eq -Label "all-null-path files returns false (no real files counted)" -Expected $false `
    -Actual (Test-PrIsToolingOnly -Files $weirdFiles)

# Edge: src/.../docs/foo.md should NOT match the docs/ prefix rule
$srcUnderDocsFiles = @(
    @{ path = 'src/Controls/docs/api-stability.md'; additions = 5 }
)
Assert-Eq -Label "src/.../docs/ does NOT match top-level docs/ rule" -Expected $false `
    -Actual (Test-PrIsToolingOnly -Files $srcUnderDocsFiles)

# Edge: eng/something-not-scripts is NOT in the tooling list
$engNotScriptsFiles = @(
    @{ path = 'eng/cake/Build.cake'; additions = 5 }
)
Assert-Eq -Label "eng/cake/ is NOT classified as tooling (only eng/scripts/ is)" -Expected $false `
    -Actual (Test-PrIsToolingOnly -Files $engNotScriptsFiles)

# ───── Classify-RegressionCandidate (contradictory evidence guard) ─────
Write-Host "`n[Unit] Classify-RegressionCandidate (contradictory merged backport)" -ForegroundColor Cyan

function Get-PrInfo {
    param($Repo, $PrNumber)
    return [pscustomobject]@{
        number = $PrNumber
        title = 'Fix regression'
        state = 'MERGED'
        baseRefName = 'main'
        mergedAt = '2026-01-01T00:00:00Z'
        closedAt = '2026-01-01T00:00:00Z'
        body = 'Fixes #35000'
        mergeCommit = [pscustomobject]@{ oid = 'abc1234def5678' }
        files = @([pscustomobject]@{ path = 'src/Core/src/Layouts/Layout.cs'; additions = 1; deletions = 0 })
    }
}

function Get-BackportPrsForSr {
    param($Repo, $SrBranch, $SourcePrNumber)
    return @([pscustomobject]@{
        number = 36000
        title = 'Backport fix regression'
        state = 'MERGED'
        mergedAt = '2026-01-02T00:00:00Z'
        closedAt = '2026-01-02T00:00:00Z'
    })
}

function Test-CommitOnBranch {
    param([string]$Sha, [string]$BranchRef)
    return $true
}

$classification = Classify-RegressionCandidate `
    -Issue @{ number = 35000 } `
    -CandidatePrs @(35001) `
    -Ctx @{ repo = 'dotnet/maui'; srBranch = 'release/10.0.1xx-sr7'; mainBranch = 'main' } `
    -SrContents @{ sourcePrs = @(); reverts = @() }

Assert-Eq -Label "merged backport absent from SR sourcePrSet requires review" `
    -Expected 'needs-human-review' -Actual $classification.classification
Assert-Eq -Label "contradictory merged backport evidence is low confidence" `
    -Expected 'low' -Actual $classification.confidence
Assert-Eq -Label "contradictory evidence explains missing SR git contents" `
    -Expected $true -Actual (($classification.evidence -join "`n") -match 'not found in SR git contents')

# ───── Bug regression: issue fixed by SR-direct PR (closing keyword on SR commit) ─────
# Real-world case: issue #35756 (TabbedPage modal) was fixed by PR #35768 opened
# directly against release/10.0.1xx-sr7. A later PR #35803 opened against main
# also closes the same issue (forward-flow). The classifier MUST recognize the
# SR commit's closing keyword and classify 'in-sr-active', not 'open-on-main'.
Write-Host "`n[Unit] Classify-RegressionCandidate (issue fixed by SR-direct PR)" -ForegroundColor Cyan

# Mock: PR #35803 is an OPEN PR against main (the forward-flow companion). The
# classifier would normally pick it up via timeline cross-references and report
# 'open-on-main'. With the fix, the SR-direct fix in srContents.fixedIssues
# takes precedence.
function Get-PrInfo {
    param($Repo, $PrNumber)
    return [pscustomobject]@{
        number      = $PrNumber
        title       = 'Fix OnNavigatedTo not firing after PopModalAsync'
        state       = 'OPEN'
        baseRefName = 'main'
        mergedAt    = $null
        closedAt    = $null
        body        = 'Fixes #35756'
        mergeCommit = $null
        files       = @([pscustomobject]@{ path = 'src/Controls/src/Core/Page.cs'; additions = 5; deletions = 1 })
    }
}
function Get-BackportPrsForSr { param($Repo, $SrBranch, $SourcePrNumber) return @() }
function Test-CommitOnBranch { param([string]$Sha, [string]$BranchRef) return $false }

$srContentsWithDirectFix = @{
    sourcePrs    = @(35768)
    backportPrs  = @()
    reverts      = @()
    fixedIssues  = @(35756)
    commits      = @(
        @{
            sha             = 'ddf238c74fb10bc42b1722495117e216cd43d772'
            author          = 'praveenkumarkarunanithi'
            date            = '2026-06-05T17:17:07+05:30'
            subject         = 'Fix OnNavigatedTo not firing after PopModalAsync (#35768)'
            isRevert        = $false
            backportPr      = 35768
            sourcePr        = $null
            cherrySourceSha = $null
            fixedIssues     = @(35756)
            origin          = 'primary'
        }
    )
}

$cls = Classify-RegressionCandidate `
    -Issue @{ number = 35756 } `
    -CandidatePrs @(35803) `
    -Ctx @{ repo = 'dotnet/maui'; srBranch = 'release/10.0.1xx-sr8'; mainBranch = 'main' } `
    -SrContents $srContentsWithDirectFix

Assert-Eq -Label "SR-direct fix (closing keyword on SR commit) → in-sr-active not open-on-main" `
    -Expected 'in-sr-active' -Actual $cls.classification
Assert-Eq -Label "SR-direct fix → high confidence" `
    -Expected 'high' -Actual $cls.confidence
Assert-Eq -Label "SR-direct fix → evidence cites the SR fix PR (#35768)" `
    -Expected $true -Actual (($cls.evidence -join "`n") -match '#35768')
Assert-Eq -Label "SR-direct fix → candidateFixPrs surfaces the SR PR (not the open main PR)" `
    -Expected 35768 -Actual ([int]$cls.candidateFixPrs[0].number)
Assert-Eq -Label "SR-direct fix → recommendedAction says no action" `
    -Expected $true -Actual ($cls.recommendedAction -match 'No action')

# Edge: SR-direct fix that was REVERTED on SR should classify as in-sr-reverted
$srContentsWithRevertedFix = @{
    sourcePrs    = @(35768)
    backportPrs  = @()
    reverts      = @(@{ revertsPr = $null; revertBackportPr = 35768 })
    fixedIssues  = @(35756)
    commits      = @(
        @{ backportPr = 35768; sourcePr = $null; fixedIssues = @(35756); isRevert = $false }
    )
}
$clsRev = Classify-RegressionCandidate `
    -Issue @{ number = 35756 } `
    -CandidatePrs @(35803) `
    -Ctx @{ repo = 'dotnet/maui'; srBranch = 'release/10.0.1xx-sr8'; mainBranch = 'main' } `
    -SrContents $srContentsWithRevertedFix

Assert-Eq -Label "SR-direct fix REVERTED → classified as in-sr-reverted" `
    -Expected 'in-sr-reverted' -Actual $clsRev.classification

# Edge: backward compat — partial SrContents shape (no .commits field) shouldn't throw
$cls2 = Classify-RegressionCandidate `
    -Issue @{ number = 99999 } `
    -CandidatePrs @() `
    -Ctx @{ repo = 'dotnet/maui'; srBranch = 'release/10.0.1xx-sr8'; mainBranch = 'main' } `
    -SrContents @{ sourcePrs = @(); reverts = @() }
Assert-Eq -Label "Partial SrContents (no commits/fixedIssues) does not throw" `
    -Expected 'no-fix-yet' -Actual $cls2.classification

# ───── Get-VerdictTier (deterministic tier table) ─────
Write-Host "`n[Unit] Get-VerdictTier (deterministic tier table)" -ForegroundColor Cyan

foreach ($case in @(
    @{ Cls = 'in-sr-reverted';             Tier = 1 }
    @{ Cls = 'no-fix-yet';                 Tier = 1 }
    @{ Cls = 'rejected-from-sr';           Tier = 2 }
    @{ Cls = 'backport-in-progress';       Tier = 2 }
    @{ Cls = 'merged-on-main-no-backport'; Tier = 2 }
    @{ Cls = 'merged-non-main-only';       Tier = 2 }
    @{ Cls = 'open-on-main';               Tier = 2 }
    @{ Cls = 'needs-human-review';         Tier = 2 }
    @{ Cls = 'in-sr-active';               Tier = 3 }
    @{ Cls = 'closed-as-duplicate';        Tier = 3 }
    @{ Cls = 'out-of-scope-future-sr';     Tier = 3 }
    @{ Cls = 'something-unknown';          Tier = 2 }   # safe-default: risk
)) {
    Assert-Eq -Label "Get-VerdictTier '$($case.Cls)' = $($case.Tier)" `
        -Expected $case.Tier `
        -Actual (Get-VerdictTier -Classification $case.Cls)
}

# ───── Get-OverallVerdict (the readiness gate) ─────
Write-Host "`n[Unit] Get-OverallVerdict (readiness gate)" -ForegroundColor Cyan

# Green: nothing bad
$dataGreen = @{
    metadata = @{ mode = 'shipped' }
    regressions = @(
        @{ classification = 'in-sr-active'; state = 'CLOSED' }
        @{ classification = 'closed-as-duplicate'; state = 'CLOSED' }
    )
    ci = @{ overall = 'green' }
}
$v = Get-OverallVerdict -Data $dataGreen
Assert-Eq -Label "all clean → 🟢 Ready" -Expected '🟢' -Actual $v.symbol
Assert-Eq -Label "all clean → tier 3"   -Expected 3    -Actual $v.tier

# Yellow: a backport in progress
$dataYellow = @{
    metadata = @{ mode = 'shipped' }
    regressions = @(
        @{ classification = 'in-sr-active'; state = 'CLOSED' }
        @{ classification = 'backport-in-progress'; state = 'OPEN' }
    )
    ci = @{ overall = 'green' }
}
$v = Get-OverallVerdict -Data $dataYellow
Assert-Eq -Label "backport-in-progress → 🟡 Conditionally Ready" -Expected '🟡' -Actual $v.symbol

# Yellow: red-needs-review CI
$dataYellowCi = @{
    metadata = @{ mode = 'shipped' }
    regressions = @(@{ classification = 'in-sr-active'; state = 'CLOSED' })
    ci = @{ overall = 'red-needs-review' }
}
$v = Get-OverallVerdict -Data $dataYellowCi
Assert-Eq -Label "red-needs-review (shipped) → 🟡" -Expected '🟡' -Actual $v.symbol

# Yellow: partial-unknown CI
$dataPartialUnknownCi = @{
    metadata = @{ mode = 'shipped' }
    regressions = @(@{ classification = 'in-sr-active'; state = 'CLOSED' })
    ci = @{ overall = 'partial-unknown' }
}
$v = Get-OverallVerdict -Data $dataPartialUnknownCi
Assert-Eq -Label "partial-unknown (shipped) → 🟡" -Expected '🟡' -Actual $v.symbol

# Red: open no-fix-yet
$dataRedRegr = @{
    metadata = @{ mode = 'shipped' }
    regressions = @(
        @{ classification = 'in-sr-active'; state = 'CLOSED' }
        @{ classification = 'no-fix-yet'; state = 'OPEN' }
    )
    ci = @{ overall = 'green' }
}
$v = Get-OverallVerdict -Data $dataRedRegr
Assert-Eq -Label "OPEN no-fix-yet → 🔴" -Expected '🔴' -Actual $v.symbol

# CLOSED no-fix-yet must NOT block (the issue was triaged away)
$dataClosedNoFix = @{
    metadata = @{ mode = 'shipped' }
    regressions = @(
        @{ classification = 'no-fix-yet'; state = 'CLOSED' }
    )
    ci = @{ overall = 'green' }
}
$v = Get-OverallVerdict -Data $dataClosedNoFix
Assert-Eq -Label "CLOSED no-fix-yet does NOT block → 🟢" -Expected '🟢' -Actual $v.symbol

# In-sr-reverted always blocks
$dataReverted = @{
    metadata = @{ mode = 'shipped' }
    regressions = @(@{ classification = 'in-sr-reverted'; state = 'CLOSED' })
    ci = @{ overall = 'green' }
}
$v = Get-OverallVerdict -Data $dataReverted
Assert-Eq -Label "in-sr-reverted → 🔴" -Expected '🔴' -Actual $v.symbol

# Candidate mode downgrades CI noise to advisory
$dataCandidateCi = @{
    metadata = @{ mode = 'candidate' }
    regressions = @()
    ci = @{ overall = 'red-needs-review' }
}
$v = Get-OverallVerdict -Data $dataCandidateCi
Assert-Eq -Label "candidate + red-needs-review does NOT block → 🟢" -Expected '🟢' -Actual $v.symbol

# Unknown CI in candidate mode is advisory only
$dataCandidateUnknown = @{
    metadata = @{ mode = 'candidate' }
    regressions = @()
    ci = @{ overall = 'partial-unknown' }
}
$v = Get-OverallVerdict -Data $dataCandidateUnknown
Assert-Eq -Label "candidate + partial-unknown does NOT block → 🟢" -Expected '🟢' -Actual $v.symbol

# ───── ConvertTo-LinkedSha / ConvertTo-LinkedPr ─────
Write-Host "`n[Unit] Markdown linkification helpers" -ForegroundColor Cyan

$rurl = 'https://github.com/dotnet/maui'
Assert-Eq -Label "ConvertTo-LinkedSha full SHA → markdown link with 8-char display" `
    -Expected '[`23accba7`](https://github.com/dotnet/maui/commit/23accba79e0f12345678)' `
    -Actual (ConvertTo-LinkedSha -Sha '23accba79e0f12345678' -RepoUrl $rurl)

Assert-Eq -Label "ConvertTo-LinkedSha short SHA renders as-is in display" `
    -Expected '[`abc1234`](https://github.com/dotnet/maui/commit/abc1234)' `
    -Actual (ConvertTo-LinkedSha -Sha 'abc1234' -RepoUrl $rurl)

Assert-Eq -Label "ConvertTo-LinkedSha empty SHA returns '?'" -Expected '?' `
    -Actual (ConvertTo-LinkedSha -Sha '' -RepoUrl $rurl)

Assert-Eq -Label "ConvertTo-LinkedSha no RepoUrl falls back to code-fence" `
    -Expected '`abc1234`' `
    -Actual (ConvertTo-LinkedSha -Sha 'abc1234' -RepoUrl '')

Assert-Eq -Label "ConvertTo-LinkedPr 35807 → markdown link" `
    -Expected '[#35807](https://github.com/dotnet/maui/pull/35807)' `
    -Actual (ConvertTo-LinkedPr -PrNumber 35807 -RepoUrl $rurl)

Assert-Eq -Label "ConvertTo-LinkedPr null → em-dash" -Expected '—' `
    -Actual (ConvertTo-LinkedPr -PrNumber $null -RepoUrl $rurl)

Assert-Eq -Label "ConvertTo-LinkedPr no RepoUrl falls back to '#NNN'" -Expected '#35807' `
    -Actual (ConvertTo-LinkedPr -PrNumber 35807 -RepoUrl '')

# ───── Get-ReportSemanticHash (idempotency hash) ─────
Write-Host "`n[Unit] Get-ReportSemanticHash (idempotency)" -ForegroundColor Cyan

$dataA = @{
    metadata = @{ srHeadSha = 'aaaaaaaa1111'; fetchedAt = '2025-01-01T00:00:00Z' }
    ci = @{ overall = 'green' }
    srContents = @{ sourcePrs = @(35001, 35002, 35003) }
    regressions = @(
        @{ issue = 35001; classification = 'in-sr-active' }
        @{ issue = 35002; classification = 'backport-in-progress' }
    )
    openSrPrs = @( @{ number = 35100 } )
}
$verdictA = @{ symbol = '🟡' }
$hashA = Get-ReportSemanticHash -Data $dataA -Verdict $verdictA
Assert-Eq -Label "Hash is 64-char SHA-256 hex"   -Expected 64   -Actual $hashA.Length
Assert-Eq -Label "Hash is lowercase hex chars"   -Expected $true `
    -Actual ($hashA -match '^[0-9a-f]{64}$')

# fetchedAt change → SAME hash (intentionally excluded)
$dataB = @{
    metadata = @{ srHeadSha = 'aaaaaaaa1111'; fetchedAt = '2099-12-31T23:59:59Z' }   # different
    ci = @{ overall = 'green' }
    srContents = @{ sourcePrs = @(35001, 35002, 35003) }
    regressions = @(
        @{ issue = 35001; classification = 'in-sr-active' }
        @{ issue = 35002; classification = 'backport-in-progress' }
    )
    openSrPrs = @( @{ number = 35100 } )
}
$hashB = Get-ReportSemanticHash -Data $dataB -Verdict $verdictA
Assert-Eq -Label "Hash invariant to fetchedAt change" -Expected $hashA -Actual $hashB

# srHeadSha change → DIFFERENT hash
$dataC = $dataA.Clone()
$dataC['metadata'] = @{ srHeadSha = 'bbbbbbbb2222'; fetchedAt = '2025-01-01T00:00:00Z' }
$hashC = Get-ReportSemanticHash -Data $dataC -Verdict $verdictA
Assert-Eq -Label "Hash changes when srHeadSha changes" -Expected $false -Actual ($hashA -eq $hashC)

# Regression classification change → DIFFERENT hash
$dataD = $dataA.Clone()
$dataD['regressions'] = @(
    @{ issue = 35001; classification = 'in-sr-reverted' }   # different!
    @{ issue = 35002; classification = 'backport-in-progress' }
)
$hashD = Get-ReportSemanticHash -Data $dataD -Verdict $verdictA
Assert-Eq -Label "Hash changes when classification changes" -Expected $false -Actual ($hashA -eq $hashD)

# Source PR set change → DIFFERENT hash
$dataE = $dataA.Clone()
$dataE['srContents'] = @{ sourcePrs = @(35001, 35002, 35003, 35004) }
$hashE = Get-ReportSemanticHash -Data $dataE -Verdict $verdictA
Assert-Eq -Label "Hash changes when srContents.sourcePrs changes" -Expected $false -Actual ($hashA -eq $hashE)

# Verdict change → DIFFERENT hash
$verdictRed = @{ symbol = '🔴' }
$hashF = Get-ReportSemanticHash -Data $dataA -Verdict $verdictRed
Assert-Eq -Label "Hash changes when verdict.symbol changes" -Expected $false -Actual ($hashA -eq $hashF)

# Same input → SAME hash (determinism)
$hashAgain = Get-ReportSemanticHash -Data $dataA -Verdict $verdictA
Assert-Eq -Label "Hash is deterministic across runs" -Expected $hashA -Actual $hashAgain

# Order independence: source PRs in different order → SAME hash
$dataReorder = $dataA.Clone()
$dataReorder['srContents'] = @{ sourcePrs = @(35003, 35001, 35002) }   # reordered
$hashReorder = Get-ReportSemanticHash -Data $dataReorder -Verdict $verdictA
Assert-Eq -Label "Hash invariant to source-PR order" -Expected $hashA -Actual $hashReorder

# Cross-process stability (regression guard for the unordered-hashtable shuffle).
# .NET Core randomizes String.GetHashCode() per process, so a plain [hashtable]
# would serialize its keys in a DIFFERENT order each process -> a DIFFERENT hash,
# silently defeating the workflow's idempotent no-op (it compares a hash written
# by an earlier process against one computed now). The function must use an
# [ordered] dictionary so JSON key order — and the hash — is stable across
# processes. Same-process re-computation (above) can't catch this because the
# hash seed is fixed within one process; we must compute in fresh child processes.
Write-Host "`n[Unit] Get-ReportSemanticHash cross-process stability" -ForegroundColor Cyan
$childHashScript = @'
$env:GET_RELEASE_READINESS_TEST_MODE = "1"
. (Join-Path $args[0] "Get-ReleaseReadiness.ps1") -SrBranch "release/10.0.1xx-sr1" | Out-Null
$data = @{
    metadata    = @{ srHeadSha = "aaaaaaaa1111"; fetchedAt = "2025-01-01T00:00:00Z" }
    ci          = @{ overall = "green" }
    srContents  = @{ sourcePrs = @(35001, 35002, 35003) }
    regressions = @(
        @{ issue = 35001; classification = "in-sr-active" }
        @{ issue = 35002; classification = "backport-in-progress" }
    )
    openSrPrs   = @( @{ number = 35100 } )
    shipChecks  = @( @{ Area = "CI"; Status = "GREEN" }, @{ Area = "Milestones"; Status = "WATCH" } )
}
Write-Output (Get-ReportSemanticHash -Data $data -Verdict @{ symbol = "YELLOW" })
'@
$childScriptPath = Join-Path ([System.IO.Path]::GetTempPath()) "rr-hash-child-$([guid]::NewGuid().ToString('N')).ps1"
Set-Content -LiteralPath $childScriptPath -Value $childHashScript -Encoding UTF8
$rrScriptsDir = Join-Path $PSScriptRoot '..' 'scripts'
try {
    $childHash1 = (& pwsh -NoProfile -File $childScriptPath $rrScriptsDir 2>$null | Select-Object -Last 1)
    $childHash2 = (& pwsh -NoProfile -File $childScriptPath $rrScriptsDir 2>$null | Select-Object -Last 1)
    Assert-Eq -Label "Hash is a 64-char SHA-256 hex (child process)" `
        -Expected $true -Actual ($childHash1 -match '^[0-9a-f]{64}$')
    Assert-Eq -Label "Hash is stable across separate processes (ordered keys)" `
        -Expected $childHash1 -Actual $childHash2
} finally {
    Remove-Item -LiteralPath $childScriptPath -ErrorAction SilentlyContinue
}

# ───── Format-MarkdownReport: tracker markers + linkification + body cap ─────
Write-Host "`n[Unit] Format-MarkdownReport (markers, linkification, cap)" -ForegroundColor Cyan

$mdData = @{
    metadata = @{
        srBranch = 'release/10.0.1xx-sr7'
        srHeadSha = 'aaaaaaaa1111bbbbbbbb2222cccccccc'
        srHeadSubject = 'Test commit'
        fetchedAt = '2025-01-01T00:00:00Z'
        regressionLabels = @('regressed-in-10.0.60', 'regressed-in-10.0.70')
        labelInferenceMode = 'explicit'
        repo = 'dotnet/maui'
    }
    warnings = @()
    ci = @{
        overall = 'green'
        pipelines = @(
            @{ name = 'maui-pr'; verdict = 'green'; latestBuild = @{ result = 'succeeded'; isAtOrAheadOfSrHead = $true; id = '12345'; url = 'https://example/12345' } }
        )
    }
    srContents = @{ commitCount = 5; sourcePrs = @(35001, 35002); reverts = @() }
    regressions = @(
        @{ issue = 35001; title = 'Bug A'; state = 'CLOSED'; classification = 'in-sr-active';
           candidateFixPrs = @( @{ number = 35100 } ); recommendedAction = 'No action' }
        @{ issue = 35002; title = 'Bug B'; state = 'OPEN'; classification = 'backport-in-progress';
           candidateFixPrs = @( @{ number = 35200 } ); recommendedAction = 'Track backport' }
    )
    summary = @{ 'in-sr-active' = 1; 'backport-in-progress' = 1 }
    openSrPrs = @()
}

$md = Format-MarkdownReport -Data $mdData -RepoUrl 'https://github.com/dotnet/maui' `
                            -TrackerKey 'net10-sr7' -MaxBodyBytes 60000

# Tracker marker (hidden)
Assert-Eq -Label "Body contains tracker marker comment" -Expected $true `
    -Actual ($md -match '<!-- release-readiness-tracker: net10-sr7 -->')

# Semantic hash marker (hidden)
Assert-Eq -Label "Body contains semantic-hash marker comment" -Expected $true `
    -Actual ($md -match '<!-- release-readiness-hash: sha=[0-9a-f]{64} -->')

# Visible tracker line
Assert-Eq -Label "Body contains visible Tracker: line" -Expected $true `
    -Actual ($md -match '\*\*Tracker:\*\* `net10-sr7`')

# Verdict appears
Assert-Eq -Label "Body shows 🟡 verdict (backport-in-progress)" -Expected $true `
    -Actual ($md -match 'Verdict — 🟡 \*\*Conditionally Ready\*\*')

# Tier sections
Assert-Eq -Label "Body has 🔴 Tier 1 section" -Expected $true `
    -Actual ($md -match '🔴 Tier 1')
Assert-Eq -Label "Body has 🟡 Tier 2 section" -Expected $true `
    -Actual ($md -match '🟡 Tier 2')
Assert-Eq -Label "Body has 🟢 Tier 3 section" -Expected $true `
    -Actual ($md -match '🟢 Tier 3')

# Linkified PR (issue 35001's fix #35100)
Assert-Eq -Label "Body linkifies fix PRs (#35100)" -Expected $true `
    -Actual ($md -match '\[#35100\]\(https://github\.com/dotnet/maui/pull/35100\)')

# Linkified issue
Assert-Eq -Label "Body linkifies issues (#35001)" -Expected $true `
    -Actual ($md -match '\[#35001\]\(https://github\.com/dotnet/maui/issues/35001\)')

# Linkified SHA
Assert-Eq -Label "Body linkifies HEAD SHA" -Expected $true `
    -Actual ($md -match '\[`aaaaaaaa`\]\(https://github\.com/dotnet/maui/commit/aaaaaaaa1111')

# Human-editable section markers
Assert-Eq -Label "Body has human-notes:begin marker" -Expected $true `
    -Actual ($md -match '<!-- release-readiness:human-notes:begin -->')
Assert-Eq -Label "Body has human-notes:end marker" -Expected $true `
    -Actual ($md -match '<!-- release-readiness:human-notes:end -->')

# Without TrackerKey: no tracker marker, no visible Tracker line
$mdNoTracker = Format-MarkdownReport -Data $mdData -RepoUrl 'https://github.com/dotnet/maui' `
                                     -MaxBodyBytes 60000
Assert-Eq -Label "Without -TrackerKey: no tracker marker"   -Expected $false `
    -Actual ($mdNoTracker -match 'release-readiness-tracker:')
Assert-Eq -Label "Without -TrackerKey: no visible Tracker line" -Expected $false `
    -Actual ($mdNoTracker -match '\*\*Tracker:\*\*')
# Hash marker still present (it's not gated by TrackerKey)
Assert-Eq -Label "Without -TrackerKey: hash marker still present" -Expected $true `
    -Actual ($mdNoTracker -match '<!-- release-readiness-hash:')

# Body cap: with very low cap, must truncate
$mdCapped = Format-MarkdownReport -Data $mdData -RepoUrl 'https://github.com/dotnet/maui' `
                                  -TrackerKey 'net10-sr7' -MaxBodyBytes 500
$cappedBytes = [System.Text.Encoding]::UTF8.GetByteCount($mdCapped)
Assert-Eq -Label "Cap respected (within ±200 bytes for truncation msg)" -Expected $true `
    -Actual ($cappedBytes -le 700 -and $cappedBytes -ge 200)
Assert-Eq -Label "Truncation message appears in capped body" -Expected $true `
    -Actual ($mdCapped -match 'Report truncated')
# Notes markers MUST survive truncation — the workflow splices live Release
# Captain Notes into them; a truncated body that lost them would let the daily
# refresh overwrite real notes. Assert exactly one clean begin+end pair remains.
$cappedBegin = ([regex]::Matches($mdCapped, '(?m)^<!-- release-readiness:human-notes:begin -->$')).Count
$cappedEnd   = ([regex]::Matches($mdCapped, '(?m)^<!-- release-readiness:human-notes:end -->$')).Count
Assert-Eq -Label "Truncated body retains exactly one notes:begin marker" -Expected 1 -Actual $cappedBegin
Assert-Eq -Label "Truncated body retains exactly one notes:end marker"   -Expected 1 -Actual $cappedEnd
# Hash marker (top of body) also survives truncation, so the SR no-op still works.
Assert-Eq -Label "Truncated body retains the semantic-hash marker" -Expected $true `
    -Actual ($mdCapped -match '<!-- release-readiness-hash: sha=[0-9a-f]{64} -->')

# ───── UTF-8 boundary repair: truncation must never split a multibyte char ─────
# Regression for the boundary-repair fix. A naive "trim trailing continuation
# bytes" cut leaves an orphan multibyte LEAD byte (and even strips a COMPLETE
# trailing char down to its lead), which GetString() renders as U+FFFD. That
# replacement char then re-encodes to 3 bytes, pushing the body back over the
# cap. Stuff the HEAD subject (rendered near the top of the body) with 4-byte
# chars, sweep caps so the cut lands inside that run at every byte phase, and
# assert no replacement char ever appears and the cap is never exceeded.
Write-Host "`n[Unit] UTF-8 boundary repair on truncation" -ForegroundColor Cyan
$origSubject = $mdData.metadata.srHeadSubject
$mdData.metadata.srHeadSubject = ([string][char]::ConvertFromUtf32(0x1F30D)) * 250  # globe x250
$replacementChar = [char]0xFFFD
$boundaryBad = 0
$capBusted = 0
foreach ($cap in 700..790) {
    $swept = Format-MarkdownReport -Data $mdData -RepoUrl 'https://github.com/dotnet/maui' `
                                   -TrackerKey 'net10-sr7' -MaxBodyBytes $cap
    if ($swept.Contains($replacementChar)) { $boundaryBad++ }
    if ([System.Text.Encoding]::UTF8.GetByteCount($swept) -gt $cap) { $capBusted++ }
}
$mdData.metadata.srHeadSubject = $origSubject
Assert-Eq -Label "No U+FFFD across cap sweep (multibyte boundary)" -Expected 0 -Actual $boundaryBad
Assert-Eq -Label "Cap never exceeded across multibyte sweep" -Expected 0 -Actual $capBusted

# ───── Verdict idempotency: same input → same hash → tracker survives re-runs ─────
Write-Host "`n[Unit] Verdict + hash idempotency (workflow re-run)" -ForegroundColor Cyan

$md1 = Format-MarkdownReport -Data $mdData -RepoUrl 'https://github.com/dotnet/maui' `
                             -TrackerKey 'net10-sr7' -MaxBodyBytes 60000
$md2 = Format-MarkdownReport -Data $mdData -RepoUrl 'https://github.com/dotnet/maui' `
                             -TrackerKey 'net10-sr7' -MaxBodyBytes 60000
$hash1 = if ($md1 -match '<!-- release-readiness-hash: sha=([0-9a-f]{64}) -->') { $Matches[1] } else { $null }
$hash2 = if ($md2 -match '<!-- release-readiness-hash: sha=([0-9a-f]{64}) -->') { $Matches[1] } else { $null }
Assert-Eq -Label "Re-running with same data produces same semantic hash" `
    -Expected $hash1 -Actual $hash2

# Change just the fetchedAt timestamp → hash stays the same
$mdDataNewTime = @{} + $mdData
$mdDataNewTime['metadata'] = @{} + $mdData.metadata
$mdDataNewTime['metadata']['fetchedAt'] = '2099-01-01T00:00:00Z'
$md3 = Format-MarkdownReport -Data $mdDataNewTime -RepoUrl 'https://github.com/dotnet/maui' `
                             -TrackerKey 'net10-sr7' -MaxBodyBytes 60000
$hash3 = if ($md3 -match '<!-- release-readiness-hash: sha=([0-9a-f]{64}) -->') { $Matches[1] } else { $null }
Assert-Eq -Label "Hash stable across only-timestamp re-runs (idempotent posts)" `
    -Expected $hash1 -Actual $hash3

# ───── @-mention defang: tracker issues must never tag real users ─────
Write-Host "`n[Unit] @-mention defang (no real-user tagging in tracker issues)" -ForegroundColor Cyan

# Format-GitHubHandle helper — exercises the at-emit-time defense
Assert-Eq -Label "Format-GitHubHandle: regular login wrapped in backticks" `
    -Expected '`jfversluis`' -Actual (Format-GitHubHandle -Login 'jfversluis')
Assert-Eq -Label "Format-GitHubHandle: bot/app ref preserved + wrapped" `
    -Expected '`app/dotnet-maestro`' -Actual (Format-GitHubHandle -Login 'app/dotnet-maestro')
Assert-Eq -Label "Format-GitHubHandle: strips leading @ before wrapping" `
    -Expected '`mattleibow`' -Actual (Format-GitHubHandle -Login '@mattleibow')
Assert-Eq -Label "Format-GitHubHandle: empty login → fallback" `
    -Expected 'unknown' -Actual (Format-GitHubHandle -Login '')
Assert-Eq -Label "Format-GitHubHandle: null login → fallback" `
    -Expected 'unknown' -Actual (Format-GitHubHandle -Login $null)
Assert-Eq -Label "Format-GitHubHandle: custom fallback honored" `
    -Expected 'n/a' -Actual (Format-GitHubHandle -Login '' -Fallback 'n/a')

# Safety-net regex: even if a PR title or commit subject contains `@user`,
# the final rendered body must defang it. Inject a hostile title via openSrPrs.
$mdDataWithAt = @{} + $mdData
$mdDataWithAt['openSrPrs'] = @(
    @{
        number = 99001
        title = '[BUG] CC @maintainer please review @another/user soon'
        author = @{ login = 'jfversluis' }
        isDraft = $false
        reviewDecision = 'APPROVED'
        updatedAt = '2025-01-01T00:00:00Z'
    }
)
$mdWithAt = Format-MarkdownReport -Data $mdDataWithAt -RepoUrl 'https://github.com/dotnet/maui' `
                                  -TrackerKey 'net10-sr7' -MaxBodyBytes 60000

# Find any bare @-mentions that survived (i.e. @-followed-by-username NOT inside backticks)
$bareMentionPattern = '(^|[^a-zA-Z0-9/`])@([a-zA-Z0-9][a-zA-Z0-9_-]*(?:/[a-zA-Z0-9][a-zA-Z0-9_-]*)?)'
$bareMatches = [regex]::Matches($mdWithAt, $bareMentionPattern)
Assert-Eq -Label "Safety net: zero bare @-mentions in rendered body even with hostile title" `
    -Expected 0 -Actual $bareMatches.Count

# Specific assertions: every hostile mention got backticked
Assert-Eq -Label "Hostile PR title: @maintainer defanged to `maintainer`" -Expected $true `
    -Actual ($mdWithAt -match '`maintainer`')
Assert-Eq -Label "Hostile PR title: @another/user defanged to `another/user`" -Expected $true `
    -Actual ($mdWithAt -match '`another/user`')
Assert-Eq -Label "Author column also defanged (no bare @jfversluis)" -Expected $true `
    -Actual ($mdWithAt -match '`jfversluis`')

# ───── Candidate-mode open-PR collapse: avoid noisy main-PR dump ─────
Write-Host "`n[Unit] Candidate-mode open-PR collapse (link to candidate PR only)" -ForegroundColor Cyan

# Shipped-mode (live SR) baseline: full table renders, all rows present.
$mdDataShipped = @{} + $mdData
$mdDataShipped['metadata'] = @{} + $mdData.metadata
$mdDataShipped['metadata']['mode'] = 'shipped'
$mdDataShipped['openSrPrs'] = @(
    @{ number = 1001; title = 'Backport: fix A'; author = @{ login = 'alice' }; isDraft = $false; reviewDecision = 'APPROVED'; updatedAt = '2026-06-01T00:00:00Z' }
    @{ number = 1002; title = 'Backport: fix B'; author = @{ login = 'bob' };   isDraft = $false; reviewDecision = 'REVIEW_REQUIRED'; updatedAt = '2026-06-02T00:00:00Z' }
)
$mdShipped = Format-MarkdownReport -Data $mdDataShipped -RepoUrl 'https://github.com/dotnet/maui' `
                                   -TrackerKey 'net10-sr7' -MaxBodyBytes 60000
Assert-Eq -Label "Shipped mode: full 'Open PRs Targeting' header still emitted" -Expected $true `
    -Actual ($mdShipped -match 'Open PRs Targeting release/10.0.1xx-sr7 — 2')
Assert-Eq -Label "Shipped mode: full table renders both rows" -Expected $true `
    -Actual (($mdShipped -match '\| \[#1001\]') -and ($mdShipped -match '\| \[#1002\]'))
Assert-Eq -Label "Shipped mode: NO 'Candidate PR for next SR cut' heading" -Expected $false `
    -Actual ($mdShipped -match 'Candidate PR for next SR cut')

# Candidate mode with NO candidate PR: emit explanatory note, suppress full table.
$mdDataCandNone = @{} + $mdData
$mdDataCandNone['metadata'] = @{} + $mdData.metadata
$mdDataCandNone['metadata']['mode'] = 'candidate'
$mdDataCandNone['metadata']['priorSrBranch'] = 'release/10.0.1xx-sr7'
$mdDataCandNone['metadata']['srBranch'] = 'main'
$mdDataCandNone['openSrPrs'] = @(
    @{ number = 2001; title = 'Random WIP fix';     author = @{ login = 'alice' }; isDraft = $false; reviewDecision = 'REVIEW_REQUIRED'; updatedAt = '2026-06-01T00:00:00Z' }
    @{ number = 2002; title = 'Bump dependencies';  author = @{ login = 'bob' };   isDraft = $false; reviewDecision = 'APPROVED'; updatedAt = '2026-06-02T00:00:00Z' }
)
$mdCandNone = Format-MarkdownReport -Data $mdDataCandNone -RepoUrl 'https://github.com/dotnet/maui' `
                                    -TrackerKey 'net10-sr8' -MaxBodyBytes 60000
Assert-Eq -Label "Candidate (no candidate PR): heading is 'Candidate PR for next SR cut'" -Expected $true `
    -Actual ($mdCandNone -match 'Candidate PR for next SR cut')
Assert-Eq -Label "Candidate (no candidate PR): explanatory note rendered" -Expected $true `
    -Actual ($mdCandNone -match 'No open PR titled')
Assert-Eq -Label "Candidate (no candidate PR): noisy PR rows NOT rendered" -Expected $false `
    -Actual (($mdCandNone -match '\| \[#2001\]') -or ($mdCandNone -match '\| \[#2002\]'))
Assert-Eq -Label "Candidate (no candidate PR): old 'Open PRs Targeting' header NOT emitted" -Expected $false `
    -Actual ($mdCandNone -match 'Open PRs Targeting main')

# Candidate mode WITH a candidate PR: emit single link + omit full table.
$mdDataCandFound = @{} + $mdData
$mdDataCandFound['metadata'] = @{} + $mdData.metadata
$mdDataCandFound['metadata']['mode'] = 'candidate'
$mdDataCandFound['metadata']['priorSrBranch'] = 'release/10.0.1xx-sr8'
$mdDataCandFound['metadata']['srBranch'] = 'main'
$mdDataCandFound['openSrPrs'] = @(
    @{ number = 3001; title = 'Random WIP fix';        author = @{ login = 'alice' }; isDraft = $false; reviewDecision = 'REVIEW_REQUIRED'; updatedAt = '2026-06-01T00:00:00Z' }
    @{ number = 3002; title = 'June 8th, Candidate';   author = @{ login = 'PureWeen' }; isDraft = $false; reviewDecision = 'REVIEW_REQUIRED'; updatedAt = '2026-06-08T00:00:00Z' }
    @{ number = 3003; title = 'Unrelated noise';       author = @{ login = 'bob' };   isDraft = $false; reviewDecision = 'APPROVED'; updatedAt = '2026-06-02T00:00:00Z' }
)
$mdCandFound = Format-MarkdownReport -Data $mdDataCandFound -RepoUrl 'https://github.com/dotnet/maui' `
                                     -TrackerKey 'net10-sr9' -MaxBodyBytes 60000
Assert-Eq -Label "Candidate (found): heading is 'Candidate PR for next SR cut'" -Expected $true `
    -Actual ($mdCandFound -match 'Candidate PR for next SR cut')
Assert-Eq -Label "Candidate (found): linked the actual candidate PR (#3002)" -Expected $true `
    -Actual ($mdCandFound -match '\[#3002\]\(https://github.com/dotnet/maui/pull/3002\)')
Assert-Eq -Label "Candidate (found): author defanged in link line" -Expected $true `
    -Actual ($mdCandFound -match '`PureWeen`')
Assert-Eq -Label "Candidate (found): unrelated PRs (#3001, #3003) NOT listed" -Expected $false `
    -Actual (($mdCandFound -match '\| \[#3001\]') -or ($mdCandFound -match '\| \[#3003\]'))
Assert-Eq -Label "Candidate (found): pointer to full PR list rendered" -Expected $true `
    -Actual ($mdCandFound -match 'is%3Apr\+is%3Aopen\+base%3Amain')

# ───── Ship-readiness checks: blocking summary + table ─────
Write-Host "`n[Unit] Ship-readiness checks (versions.props + bug template)" -ForegroundColor Cyan

# Baseline: no shipChecks key → empty blocking summary, no table
$mdNoShipChecks = Format-MarkdownReport -Data $mdData -RepoUrl 'https://github.com/dotnet/maui' `
                                        -TrackerKey 'net10-sr7' -MaxBodyBytes 60000
Assert-Eq -Label "No shipChecks key: still emits '🟢 No blocking items' (only Tier 1 regressions matter)" -Expected $true `
    -Actual ($mdNoShipChecks -match '🟢 No blocking items')
Assert-Eq -Label "No shipChecks key: no Ship-readiness checks table" -Expected $false `
    -Actual ($mdNoShipChecks -match 'Ship-readiness checks')

# Single BLOCKED ship check
$mdDataBlocked = @{} + $mdData
$mdDataBlocked['shipChecks'] = @(
    [PSCustomObject]@{
        Area       = 'versions.props PatchVersion'
        Status     = 'BLOCKED'
        Details    = "Current PatchVersion 80 is below expected range [90..99] for SR9"
        NextAction = "Bump <PatchVersion> in eng/Versions.props on main from 80 to 90"
    },
    [PSCustomObject]@{
        Area       = 'Bug-report template version dropdown'
        Status     = 'READY'
        Details    = "Found 10.0.71 in version-with-bug dropdown"
        NextAction = 'None'
    }
)
$mdBlocked = Format-MarkdownReport -Data $mdDataBlocked -RepoUrl 'https://github.com/dotnet/maui' `
                                   -TrackerKey 'net10-sr9' -MaxBodyBytes 60000
Assert-Eq -Label "BLOCKED ship check: blocking summary header reflects count" -Expected $true `
    -Actual ($mdBlocked -match '🔴 Blocking — \d+ item')
Assert-Eq -Label "BLOCKED ship check: blocking summary mentions versions.props area" -Expected $true `
    -Actual ($mdBlocked -match '🛠️ versions.props PatchVersion')
Assert-Eq -Label "BLOCKED ship check: blocking summary contains the next-action text" -Expected $true `
    -Actual ($mdBlocked -match 'Bump <PatchVersion>')
Assert-Eq -Label "BLOCKED ship check: full Ship-readiness checks table emitted" -Expected $true `
    -Actual ($mdBlocked -match 'Ship-readiness checks')
Assert-Eq -Label "BLOCKED ship check: table shows READY entry for bug template (transparency)" -Expected $true `
    -Actual ($mdBlocked -match 'Bug-report template[^|]*\|\s*🟢 READY')
# Extract just the blocking-summary section (from its heading to the next ## heading)
# and assert it does NOT mention the READY check.
$blockingSection = if ($mdBlocked -match '(?s)## 🔴 Blocking[^\n]*\n(.*?)\n## ') { $Matches[1] } else { '' }
Assert-Eq -Label "READY ship check: NOT listed in blocking summary section" -Expected $false `
    -Actual ($blockingSection -match 'Bug-report template')

# Only READY ship checks → 🟢 No blocking items (when no Tier 1 regressions)
$mdDataReady = @{} + $mdData
$mdDataReady['shipChecks'] = @(
    [PSCustomObject]@{
        Area = 'versions.props'; Status = 'READY';
        Details = 'PatchVersion=71 in expected range [70..79] for SR7';
        NextAction = 'None'
    }
)
$mdReady = Format-MarkdownReport -Data $mdDataReady -RepoUrl 'https://github.com/dotnet/maui' `
                                 -TrackerKey 'net10-sr7' -MaxBodyBytes 60000
Assert-Eq -Label "All ship checks READY (no Tier 1 regressions): '🟢 No blocking items'" -Expected $true `
    -Actual ($mdReady -match '🟢 No blocking items')

# Hash includes shipChecks state (changing a ship check status flips the hash)
$h1 = if ($mdReady -match '<!-- release-readiness-hash: sha=([0-9a-f]{64}) -->') { $Matches[1] } else { $null }
$h2 = if ($mdBlocked -match '<!-- release-readiness-hash: sha=([0-9a-f]{64}) -->') { $Matches[1] } else { $null }
Assert-Eq -Label "Hash changes when a ship check flips from READY → BLOCKED" -Expected $true `
    -Actual ($h1 -and $h2 -and $h1 -ne $h2)

# Get-OverallVerdict: BLOCKED ship check forces Not Ready
Write-Host "`n[Unit] Get-OverallVerdict — BLOCKED ship checks force Not Ready" -ForegroundColor Cyan

$verdictData = @{
    metadata = @{ mode = 'shipped' }
    regressions = @()
    ci = @{ overall = 'green' }
    shipChecks = @(
        [PSCustomObject]@{ Area = 'versions.props'; Status = 'BLOCKED'; Details = 'patch not bumped'; NextAction = 'bump it' }
    )
}
$verdict = Get-OverallVerdict -Data $verdictData
Assert-Eq -Label "Verdict tier 1 when shipChecks contains BLOCKED entry" -Expected 1 -Actual $verdict.tier
Assert-Eq -Label "Verdict label is 'Not Ready'" -Expected 'Not Ready' -Actual $verdict.label
Assert-Eq -Label "Verdict reasons list mentions BLOCKED ship-check area" -Expected $true `
    -Actual ([bool](@($verdict.reasons) -match 'Ship check BLOCKED: versions\.props'))

# WATCH or READY ship checks must not escalate
$verdictDataReady = @{
    metadata = @{ mode = 'shipped' }
    regressions = @()
    ci = @{ overall = 'green' }
    shipChecks = @(
        [PSCustomObject]@{ Area = 'versions.props'; Status = 'READY'; Details = 'OK'; NextAction = 'None' }
    )
}
$verdictReadyResult = Get-OverallVerdict -Data $verdictDataReady
Assert-Eq -Label "READY-only ship checks: verdict stays at tier 3 (Ready)" -Expected 3 -Actual $verdictReadyResult.tier

# CLEANUP ship checks must surface in the report but MUST NOT escalate the verdict.
# This locks the contract: CLEANUP = "housekeeping that needs doing, but doesn't
# prevent shipping". Used for stale-milestone backlog, missing bug-template entry, etc.
$verdictDataCleanup = @{
    metadata = @{ mode = 'shipped' }
    regressions = @()
    ci = @{ overall = 'green' }
    shipChecks = @(
        [PSCustomObject]@{ Area = 'Stale open milestones (2)'; Status = 'CLEANUP'; Details = 'SR6+SR7 still open'; NextAction = 'triage' }
        [PSCustomObject]@{ Area = 'Bug template lists SR8 version'; Status = 'CLEANUP'; Details = 'missing 10.0.80 entry'; NextAction = 'add entry' }
    )
}
$verdictCleanupResult = Get-OverallVerdict -Data $verdictDataCleanup
Assert-Eq -Label "CLEANUP-only ship checks: verdict stays at tier 3 (Ready)" -Expected 3 -Actual $verdictCleanupResult.tier
Assert-Eq -Label "CLEANUP-only ship checks: no Tier 1 reason about BLOCKED ship check" -Expected $false `
    -Actual ([bool](@($verdictCleanupResult.reasons) -match 'Ship check BLOCKED'))

# Markdown rendering: CLEANUP renders a separate '🧹 Cleanup follow-ups' section
# and stays out of the '🔴 Blocking' table.
$mdDataCleanup = @{} + $mdData
$mdDataCleanup['shipChecks'] = @(
    [PSCustomObject]@{ Area = 'Stale open milestones (2)'; Status = 'CLEANUP'; Details = 'SR6+SR7 open'; NextAction = 'triage' }
)
$mdCleanup = Format-MarkdownReport -Data $mdDataCleanup -RepoUrl 'https://github.com/dotnet/maui'
Assert-Eq -Label "CLEANUP renders dedicated '🧹 Cleanup follow-ups' section" -Expected $true `
    -Actual ($mdCleanup -match '## 🧹 Cleanup follow-ups')
Assert-Eq -Label "CLEANUP does NOT appear in '🔴 Blocking' table" -Expected $false `
    -Actual ($mdCleanup -match '🔴 Blocking[\s\S]*Stale open milestones')
Assert-Eq -Label "CLEANUP renders '🧹 CLEANUP' badge in full ship-checks table" -Expected $true `
    -Actual ($mdCleanup -match '🧹 CLEANUP')

# ───── Open Fix PRs Inbound — hoisted regression-fix watchlist ─────
Write-Host "`n[Unit] Open Fix PRs Inbound (hoisted regression-fix watchlist)" -ForegroundColor Cyan

# Two open-on-main + one backport-in-progress = 3 rows; one in-sr-active filtered out
$mdDataInbound = @{} + $mdData
$mdDataInbound['metadata'] = @{} + $mdData.metadata
$mdDataInbound['metadata']['srBranch'] = 'release/10.0.1xx-sr8'
$mdDataInbound['regressions'] = @(
    @{ issue = 9001; title = 'Open-on-main regression 1'; state = 'OPEN'
       classification = 'open-on-main'; confidence = 'high'; evidence = @()
       candidateFixPrs = @(
           @{ number = 4001; title = 'Fix 9001'; state = 'OPEN'; baseRef = 'main'; onMain = $false; backports = @() }
       )
       recommendedAction = 'Wait for main merge, then open backport' }
    @{ issue = 9002; title = 'Open-on-main regression 2 with very long title that should be truncated when rendered to keep the column readable'
       state = 'OPEN'
       classification = 'open-on-main'; confidence = 'high'; evidence = @()
       candidateFixPrs = @(
           @{ number = 4002; title = 'Fix 9002'; state = 'OPEN'; baseRef = 'main'; onMain = $false; backports = @() }
       )
       recommendedAction = 'Wait for main merge, then open backport' }
    @{ issue = 9003; title = 'Backport-in-progress regression'; state = 'OPEN'
       classification = 'backport-in-progress'; confidence = 'high'; evidence = @()
       candidateFixPrs = @(
           @{ number = 4003; title = 'Fix 9003'; state = 'MERGED'; baseRef = 'main'; onMain = $true
              backports = @(
                  @{ number = 4099; state = 'OPEN'; title = 'Backport: fix 9003' }
              ) }
       )
       recommendedAction = 'Track backport PR to completion' }
    @{ issue = 9004; title = 'Already shipped regression'; state = 'CLOSED'
       classification = 'in-sr-active'; confidence = 'high'; evidence = @()
       candidateFixPrs = @()
       recommendedAction = 'No action — fix is shipping' }
)
$mdInbound = Format-MarkdownReport -Data $mdDataInbound -RepoUrl 'https://github.com/dotnet/maui' `
                                   -TrackerKey 'net10-sr8' -MaxBodyBytes 60000

Assert-Eq -Label "Open Fix PRs Inbound: section header emitted with count 3" -Expected $true `
    -Actual ($mdInbound -match '## 📥 Open Fix PRs Inbound — 3 PR\(s\)')
# Extract just the inbound section so we can check what's inside it
# (other PR/issue numbers like #4003, #9004 legitimately appear in the lower
# regression breakdown tables — they're just not allowed in the Inbound row set).
$inboundSection = if ($mdInbound -match '(?s)## 📥 Open Fix PRs Inbound[^\n]*\n(.*?)\n## ') { $Matches[1] } else { '' }
Assert-Eq -Label "Open Fix PRs Inbound: links open-on-main PR #4001" -Expected $true `
    -Actual ($inboundSection -match '\[#4001\]\(https://github.com/dotnet/maui/pull/4001\)')
Assert-Eq -Label "Open Fix PRs Inbound: links open-on-main PR #4002" -Expected $true `
    -Actual ($inboundSection -match '\[#4002\]\(https://github.com/dotnet/maui/pull/4002\)')
Assert-Eq -Label "Open Fix PRs Inbound: links backport-in-progress PR #4099 (not source #4003)" -Expected $true `
    -Actual (($inboundSection -match '\[#4099\]') -and -not ($inboundSection -match '\[#4003\]'))
Assert-Eq -Label "Open Fix PRs Inbound: in-sr-active regression (#9004) NOT listed in Inbound rows" -Expected $false `
    -Actual ($inboundSection -match '#9004')
Assert-Eq -Label "Open Fix PRs Inbound: status column distinguishes main vs SR" -Expected $true `
    -Actual (($inboundSection -match '🔵 OPEN — awaiting main merge') -and ($inboundSection -match '🟡 backport OPEN on SR'))
Assert-Eq -Label "Open Fix PRs Inbound: long titles truncated at 70 chars" -Expected $true `
    -Actual ($inboundSection -match 'Open-on-main regression 2[^|]*\.\.\.')

# Section is appended ABOVE Ship-readiness checks (just under Blocking)
$inboundIdx = $mdInbound.IndexOf('## 📥 Open Fix PRs Inbound')
$shipChecksIdx = $mdInbound.IndexOf('## Ship-readiness checks')
$blockingIdx = if ($mdInbound -match '(?m)^## (?:🔴 Blocking|🟢 No blocking)') { $mdInbound.IndexOf($Matches[0]) } else { -1 }
Assert-Eq -Label "Open Fix PRs Inbound: appears AFTER Blocking section" -Expected $true `
    -Actual ($blockingIdx -ge 0 -and $inboundIdx -gt $blockingIdx)
Assert-Eq -Label "Open Fix PRs Inbound: appears BEFORE Ship-readiness checks" -Expected $true `
    -Actual ($shipChecksIdx -lt 0 -or $inboundIdx -lt $shipChecksIdx)

# Empty case: no regressions in flight → no section
$mdDataNoInbound = @{} + $mdData
$mdDataNoInbound['regressions'] = @(
    @{ issue = 9005; title = 'no-fix-yet'; state = 'OPEN'; classification = 'no-fix-yet'
       confidence = 'high'; evidence = @(); candidateFixPrs = @(); recommendedAction = 'investigate' }
)
$mdNoInbound = Format-MarkdownReport -Data $mdDataNoInbound -RepoUrl 'https://github.com/dotnet/maui' `
                                     -TrackerKey 'net10-sr8' -MaxBodyBytes 60000
Assert-Eq -Label "Open Fix PRs Inbound: no section when no open fix PRs in flight" -Expected $false `
    -Actual ($mdNoInbound -match 'Open Fix PRs Inbound')

# ───── Get-ReleaseShipChecks: 'Main bumped to next SR cycle' check ─────
# Verifies that when surveying an in-flight SR, the script ALSO blocks if
# main hasn't bumped its PatchVersion past the SR being shipped. (Convention:
# right after release/X.Y.Zxx-srN is cut, main bumps to (N+1)*10 so any PR
# merging during SR$N stabilization correctly targets the NEXT SR cycle.)
Write-Host "`n[Unit] Get-ReleaseShipChecks — 'Main bumped to next SR cycle'" -ForegroundColor Cyan

function Build-VersionsPropsXml {
    param(
        [int]$Major,
        [int]$Minor,
        [int]$Patch,
        # Optional servicing-flip fields. When $null, the element is omitted
        # (mirrors a freshly-cut SR branch that hasn't been flipped yet).
        [string]$PreReleaseVersionLabel,
        [string]$StabilizePackageVersion
    )
    $labelLine = if ($PreReleaseVersionLabel) {
        "    <PreReleaseVersionLabel>$PreReleaseVersionLabel</PreReleaseVersionLabel>`n"
    } else { "" }
    $stabilizeLine = if ($StabilizePackageVersion) {
        "    <StabilizePackageVersion Condition=`"'`$(StabilizePackageVersion)' == ''`">$StabilizePackageVersion</StabilizePackageVersion>`n"
    } else { "" }
    @"
<Project>
  <PropertyGroup>
    <MajorVersion>$Major</MajorVersion>
    <MinorVersion>$Minor</MinorVersion>
    <PatchVersion>$Patch</PatchVersion>
$labelLine$stabilizeLine  </PropertyGroup>
</Project>
"@
}

# Tiny bug-report.yml that always satisfies the version-with-bug dropdown check
# (we're focused on the new main-bumped check, not the template check).
$bugYamlAllowsAll = @'
- type: dropdown
  id: version-with-bug
  attributes:
    options:
      - "10.0.80 (SR8)"
      - "10.0.90 (SR9)"
'@

function Invoke-ShipChecksWithMockedVersions {
    param(
        [hashtable]$SrVersion,    # @{Major;Minor;Patch [;PreReleaseVersionLabel;StabilizePackageVersion]} for the SR branch
        [hashtable]$MainVersion,  # @{Major;Minor;Patch [;PreReleaseVersionLabel;StabilizePackageVersion]} for main
        [string]$SrBranch = 'release/10.0.1xx-sr8',
        [string]$MainBranch = 'main',
        [switch]$Candidate
    )
    # Wrap Get-FileFromRef so the script's existing Get-VersionsPropsState /
    # Get-BugTemplateVersions read from these in-memory blobs.
    $srRef   = "origin/$SrBranch"
    $mainRef = "origin/$MainBranch"
    $srXml   = Build-VersionsPropsXml @SrVersion
    $mainXml = if ($MainVersion) { Build-VersionsPropsXml @MainVersion } else { $null }

    $script:_origGetFile = Get-Command Get-FileFromRef -CommandType Function
    function global:Get-FileFromRef {
        param([string]$Path, [string]$Ref)
        if ($Path -eq 'eng/Versions.props') {
            if ($Ref -eq $script:_mockSrRef)   { return $script:_mockSrXml }
            if ($Ref -eq $script:_mockMainRef) { return $script:_mockMainXml }
            return $null
        }
        if ($Path -eq '.github/ISSUE_TEMPLATE/bug-report.yml') {
            return $script:_mockBugYaml
        }
        return $null
    }
    $script:_mockSrRef    = $srRef
    $script:_mockMainRef  = $mainRef
    $script:_mockSrXml    = $srXml
    $script:_mockMainXml  = $mainXml
    $script:_mockBugYaml  = $bugYamlAllowsAll

    try {
        $ctx = @{
            srBranch   = if ($Candidate) { $MainBranch } else { $SrBranch }
            srRef      = if ($Candidate) { "origin/$MainBranch" } else { "origin/$SrBranch" }
            mainBranch = $MainBranch
            mode       = if ($Candidate) { 'candidate' } else { 'in-flight' }
            priorSrBranch = if ($Candidate) { $SrBranch } else { $null }
        }
        return Get-ReleaseShipChecks -Ctx $ctx
    } finally {
        Remove-Item function:global:Get-FileFromRef -ErrorAction SilentlyContinue
    }
}

# Helper: scoped check lookup
function Get-CheckByAreaPrefix {
    param($Checks, [string]$Prefix)
    @($Checks | Where-Object { $_.Area.StartsWith($Prefix) }) | Select-Object -First 1
}

# Scenario 1: SR8 in-flight, main STILL at same cycle (10.0.80) — BLOCKED
$checks1 = Invoke-ShipChecksWithMockedVersions `
    -SrVersion @{ Major=10; Minor=0; Patch=80 } `
    -MainVersion @{ Major=10; Minor=0; Patch=80 } `
    -SrBranch 'release/10.0.1xx-sr8'

$mainBumpCheck = Get-CheckByAreaPrefix -Checks $checks1 -Prefix 'Main bumped to SR9 cycle'
Assert-Eq -Label "Main-not-bumped: emits 'Main bumped to SR9 cycle' check" -Expected $true `
    -Actual ($null -ne $mainBumpCheck)
Assert-Eq -Label "Main-not-bumped (main=80, SR8=80): status BLOCKED" -Expected 'BLOCKED' -Actual $mainBumpCheck.Status
Assert-Eq -Label "Main-not-bumped: details mention same cycle" -Expected $true `
    -Actual ([bool]($mainBumpCheck.Details -match 'same cycle'))
Assert-Eq -Label "Main-not-bumped: next action points to 90" -Expected $true `
    -Actual ([bool]($mainBumpCheck.NextAction -match '\b90\b'))

# Scenario 2: SR8 in-flight, main already bumped to 10.0.90 — READY
$checks2 = Invoke-ShipChecksWithMockedVersions `
    -SrVersion @{ Major=10; Minor=0; Patch=80 } `
    -MainVersion @{ Major=10; Minor=0; Patch=90 } `
    -SrBranch 'release/10.0.1xx-sr8'

$mainBumpCheck2 = Get-CheckByAreaPrefix -Checks $checks2 -Prefix 'Main bumped to SR9 cycle'
Assert-Eq -Label "Main-bumped-to-90: status READY"  -Expected 'READY' -Actual $mainBumpCheck2.Status
Assert-Eq -Label "Main-bumped-to-90: details show 90 satisfied" -Expected $true `
    -Actual ([bool]($mainBumpCheck2.Details -match 'at or past'))

# Scenario 3: SR8 in-flight, main past the major train (11.0.x) — READY
$checks3 = Invoke-ShipChecksWithMockedVersions `
    -SrVersion @{ Major=10; Minor=0; Patch=80 } `
    -MainVersion @{ Major=11; Minor=0; Patch=10 } `
    -SrBranch 'release/10.0.1xx-sr8'

$mainBumpCheck3 = Get-CheckByAreaPrefix -Checks $checks3 -Prefix 'Main bumped to SR9 cycle'
Assert-Eq -Label "Main-past-major (11.0): status READY"  -Expected 'READY' -Actual $mainBumpCheck3.Status
Assert-Eq -Label "Main-past-major: details mention moved past train" -Expected $true `
    -Actual ([bool]($mainBumpCheck3.Details -match 'moved past'))

# Scenario 4: SR8 in-flight, main bumped multiple cycles ahead (10.0.110 for hypothetical SR11) — READY
$checks4 = Invoke-ShipChecksWithMockedVersions `
    -SrVersion @{ Major=10; Minor=0; Patch=80 } `
    -MainVersion @{ Major=10; Minor=0; Patch=110 } `
    -SrBranch 'release/10.0.1xx-sr8'

$mainBumpCheck4 = Get-CheckByAreaPrefix -Checks $checks4 -Prefix 'Main bumped to SR9 cycle'
Assert-Eq -Label "Main-way-ahead (patch=110): status READY"  -Expected 'READY' -Actual $mainBumpCheck4.Status

# Scenario 5: Candidate mode → the new check is SKIPPED (no double-counting with the
# existing 'Versions.props bump (main → SRn)' check that already targets main)
$checks5 = Invoke-ShipChecksWithMockedVersions `
    -SrVersion @{ Major=10; Minor=0; Patch=80 } `
    -MainVersion @{ Major=10; Minor=0; Patch=80 } `
    -SrBranch 'release/10.0.1xx-sr8' `
    -Candidate

$mainBumpCheck5 = Get-CheckByAreaPrefix -Checks $checks5 -Prefix 'Main bumped to'
Assert-Eq -Label "Candidate mode: 'Main bumped to' check NOT emitted (avoids redundancy)" -Expected $true `
    -Actual ($null -eq $mainBumpCheck5)

# Scenario 6: SR-branch check still works (existing behavior — guard against regressions)
$srBranchCheck = Get-CheckByAreaPrefix -Checks $checks1 -Prefix 'Versions.props bump (SR8)'
Assert-Eq -Label "Existing SR-branch check still emitted alongside new main-bump check" -Expected $true `
    -Actual ($null -ne $srBranchCheck)
Assert-Eq -Label "Existing SR-branch check stays READY when SR is at 80" -Expected 'READY' -Actual $srBranchCheck.Status

# ───── Get-ReleaseShipChecks: 'Servicing-release flip' check ─────
# When an SR branch is cut from main, eng/Versions.props MUST be flipped to
# servicing-release mode (PreReleaseVersionLabel=servicing, StabilizePackageVersion=true).
# Without it, the SR builds prerelease packages and never ships as stable —
# CI stays green so nothing else catches it.
Write-Host "`n[Unit] Get-ReleaseShipChecks — 'Servicing-release flip'" -ForegroundColor Cyan

# Scenario A: SR8 fully flipped — READY
$flipChecksA = Invoke-ShipChecksWithMockedVersions `
    -SrVersion @{ Major=10; Minor=0; Patch=80; PreReleaseVersionLabel='servicing'; StabilizePackageVersion='true' } `
    -MainVersion @{ Major=10; Minor=0; Patch=90; PreReleaseVersionLabel='ci.main'; StabilizePackageVersion='false' } `
    -SrBranch 'release/10.0.1xx-sr8'
$flipCheckA = Get-CheckByAreaPrefix -Checks $flipChecksA -Prefix 'Versions.props servicing flip (SR8)'
Assert-Eq -Label "Flip-applied: emits 'Versions.props servicing flip (SR8)' check" -Expected $true `
    -Actual ($null -ne $flipCheckA)
Assert-Eq -Label "Flip-applied (servicing + true): status READY" -Expected 'READY' -Actual $flipCheckA.Status

# Scenario B: SR8 with label still ci.main — BLOCKED
$flipChecksB = Invoke-ShipChecksWithMockedVersions `
    -SrVersion @{ Major=10; Minor=0; Patch=80; PreReleaseVersionLabel='ci.main'; StabilizePackageVersion='true' } `
    -MainVersion @{ Major=10; Minor=0; Patch=90 } `
    -SrBranch 'release/10.0.1xx-sr8'
$flipCheckB = Get-CheckByAreaPrefix -Checks $flipChecksB -Prefix 'Versions.props servicing flip (SR8)'
Assert-Eq -Label "Flip-missing-label (ci.main): status BLOCKED" -Expected 'BLOCKED' -Actual $flipCheckB.Status
Assert-Eq -Label "Flip-missing-label: details mention PreReleaseVersionLabel" -Expected $true `
    -Actual ([bool]($flipCheckB.Details -match 'PreReleaseVersionLabel'))
Assert-Eq -Label "Flip-missing-label: details mention actual ci.main value" -Expected $true `
    -Actual ([bool]($flipCheckB.Details -match 'ci\.main'))
Assert-Eq -Label "Flip-missing-label: details do NOT flag StabilizePackageVersion" -Expected $true `
    -Actual (-not ($flipCheckB.Details -match 'StabilizePackageVersion'))

# Scenario C: SR8 with StabilizePackageVersion=false — BLOCKED
$flipChecksC = Invoke-ShipChecksWithMockedVersions `
    -SrVersion @{ Major=10; Minor=0; Patch=80; PreReleaseVersionLabel='servicing'; StabilizePackageVersion='false' } `
    -MainVersion @{ Major=10; Minor=0; Patch=90 } `
    -SrBranch 'release/10.0.1xx-sr8'
$flipCheckC = Get-CheckByAreaPrefix -Checks $flipChecksC -Prefix 'Versions.props servicing flip (SR8)'
Assert-Eq -Label "Flip-missing-stabilize (false): status BLOCKED" -Expected 'BLOCKED' -Actual $flipCheckC.Status
Assert-Eq -Label "Flip-missing-stabilize: details mention StabilizePackageVersion" -Expected $true `
    -Actual ([bool]($flipCheckC.Details -match 'StabilizePackageVersion'))
Assert-Eq -Label "Flip-missing-stabilize: details do NOT flag PreReleaseVersionLabel" -Expected $true `
    -Actual (-not ($flipCheckC.Details -match 'PreReleaseVersionLabel'))

# Scenario D: SR8 with BOTH missing entirely (fresh branch cut, never flipped) — BLOCKED with both flagged
$flipChecksD = Invoke-ShipChecksWithMockedVersions `
    -SrVersion @{ Major=10; Minor=0; Patch=80 } `
    -MainVersion @{ Major=10; Minor=0; Patch=90 } `
    -SrBranch 'release/10.0.1xx-sr8'
$flipCheckD = Get-CheckByAreaPrefix -Checks $flipChecksD -Prefix 'Versions.props servicing flip (SR8)'
Assert-Eq -Label "Flip-never-applied: status BLOCKED" -Expected 'BLOCKED' -Actual $flipCheckD.Status
Assert-Eq -Label "Flip-never-applied: details flag PreReleaseVersionLabel" -Expected $true `
    -Actual ([bool]($flipCheckD.Details -match 'PreReleaseVersionLabel'))
Assert-Eq -Label "Flip-never-applied: details flag StabilizePackageVersion" -Expected $true `
    -Actual ([bool]($flipCheckD.Details -match 'StabilizePackageVersion'))
Assert-Eq -Label "Flip-never-applied: details mark unset values" -Expected $true `
    -Actual ([bool]($flipCheckD.Details -match '<unset>'))
Assert-Eq -Label "Flip-never-applied: next action references the prior SR's diff" -Expected $true `
    -Actual ([bool]($flipCheckD.NextAction -match 'release/10\.0\.1xx-sr7'))

# Scenario E: Candidate mode → flip check SKIPPED (main is supposed to be ci.main/false)
$flipChecksE = Invoke-ShipChecksWithMockedVersions `
    -SrVersion @{ Major=10; Minor=0; Patch=80; PreReleaseVersionLabel='ci.main'; StabilizePackageVersion='false' } `
    -MainVersion @{ Major=10; Minor=0; Patch=80; PreReleaseVersionLabel='ci.main'; StabilizePackageVersion='false' } `
    -SrBranch 'release/10.0.1xx-sr8' `
    -Candidate
$flipCheckE = Get-CheckByAreaPrefix -Checks $flipChecksE -Prefix 'Versions.props servicing flip'
Assert-Eq -Label "Candidate mode: servicing-flip check NOT emitted" -Expected $true `
    -Actual ($null -eq $flipCheckE)

# ───── ci-scan freshness + rendering ─────
Write-Host "`n[Unit] Format-CiScanIssueRows + freshness" -ForegroundColor Cyan

$nowUtc = (Get-Date).ToUniversalTime()
$ciScanIssues = @(
    [PSCustomObject]@{ number = 35864; url = 'https://github.com/dotnet/maui/issues/35864'; title = 'Recurring CarouselView timeout';
                       createdAt = $nowUtc.AddHours(-6).ToString('o') }
    [PSCustomObject]@{ number = 35854; url = 'https://github.com/dotnet/maui/issues/35854'; title = 'Env instability CV Android';
                       createdAt = $nowUtc.AddDays(-3).ToString('o') }
    [PSCustomObject]@{ number = 35738; url = 'https://github.com/dotnet/maui/issues/35738'; title = 'Flaky iOS RootViewSize test';
                       createdAt = $nowUtc.AddDays(-10).ToString('o') }
)
$rows = Format-CiScanIssueRows -Issues $ciScanIssues -RepoUrl 'https://github.com/dotnet/maui'
Assert-Eq -Label "Fresh issue (<24h) gets 🆕 marker" -Expected $true `
    -Actual ($rows -match '🆕\s*\[#35864\]')
Assert-Eq -Label "Older issue (>24h) does NOT get 🆕 marker" -Expected $false `
    -Actual ($rows -match '🆕\s*\[#35854\]')
Assert-Eq -Label "Age column shows '6h ago' for ~6-hour-old issue" -Expected $true `
    -Actual ($rows -match '6h ago')
Assert-Eq -Label "Age column shows 'Nd ago' for older issues" -Expected $true `
    -Actual ($rows -match '\d+d ago')
Assert-Eq -Label "Format-CiScanIssueRows returns null for empty input" -Expected $true `
    -Actual ($null -eq (Format-CiScanIssueRows -Issues @() -RepoUrl 'https://github.com/dotnet/maui'))

# Truncation behavior: > MaxRows
$manyIssues = 1..20 | ForEach-Object {
    [PSCustomObject]@{ number = 40000 + $_; url = "https://github.com/dotnet/maui/issues/$(40000+$_)";
                       title = "Auto-filed $_"; createdAt = $nowUtc.AddDays(-$_).ToString('o') }
}
$rowsCapped = Format-CiScanIssueRows -Issues $manyIssues -RepoUrl 'https://github.com/dotnet/maui' -MaxRows 5
Assert-Eq -Label "Cap respected (MaxRows=5 shows 5 issue rows)" -Expected 5 `
    -Actual ([regex]::Matches($rowsCapped, '\| \[#400').Count)
Assert-Eq -Label "Cap explanation rendered with '…and N more' note" -Expected $true `
    -Actual ($rowsCapped -match '…and 15 more')
Assert-Eq -Label "Cap explanation links to filtered issue list" -Expected $true `
    -Actual ($rowsCapped -match 'label%3Aci-scan')

# Markdown includes ci-scan section when ciScanIssues are present
Write-Host "`n[Unit] SR markdown includes 'Recent CI Failure Scanner signals' section" -ForegroundColor Cyan

$mdDataWithCiScan = @{} + $mdData
$mdDataWithCiScan['ciScanIssues'] = $ciScanIssues
$mdWithCiScan = Format-MarkdownReport -Data $mdDataWithCiScan -RepoUrl 'https://github.com/dotnet/maui' `
                                      -TrackerKey 'net10-sr7' -MaxBodyBytes 60000
Assert-Eq -Label "ci-scan section header rendered when issues present" -Expected $true `
    -Actual ($mdWithCiScan -match 'Recent CI Failure Scanner signals')
Assert-Eq -Label "ci-scan section explanatory note rendered" -Expected $true `
    -Actual ($mdWithCiScan -match 'auto-filed by the CI Failure Scanner workflow')
Assert-Eq -Label "ci-scan section links to a fresh issue" -Expected $true `
    -Actual ($mdWithCiScan -match '🆕\s*\[#35864\]')

# Branch-filter: when filtered, blurb mentions the survey branch
Assert-Eq -Label "ci-scan blurb mentions survey branch" -Expected $true `
    -Actual ($mdWithCiScan -match 'matches `release/10\.0\.1xx-sr7`')

# Branch-filter: when ciScanFilteredOut > 0, blurb surfaces excluded count
$mdDataWithFiltered = @{} + $mdDataWithCiScan
$mdDataWithFiltered['ciScanFilteredOut'] = 7
$mdWithFiltered = Format-MarkdownReport -Data $mdDataWithFiltered -RepoUrl 'https://github.com/dotnet/maui' `
                                        -TrackerKey 'net10-sr7' -MaxBodyBytes 60000
Assert-Eq -Label "ci-scan blurb surfaces excluded-count" -Expected $true `
    -Actual ($mdWithFiltered -match '7 other-branch issue\(s\) were excluded')

# Branch-filter: empty matched list still renders section header (with no-issues note)
$mdDataEmptyCiScan = @{} + $mdData
$mdDataEmptyCiScan['ciScanIssues'] = @()
$mdDataEmptyCiScan['ciScanFilteredOut'] = 5
$mdEmptyCiScan = Format-MarkdownReport -Data $mdDataEmptyCiScan -RepoUrl 'https://github.com/dotnet/maui' `
                                       -TrackerKey 'net10-sr7' -MaxBodyBytes 60000
Assert-Eq -Label "ci-scan empty list: section still renders" -Expected $true `
    -Actual ($mdEmptyCiScan -match 'Recent CI Failure Scanner signals')
Assert-Eq -Label "ci-scan empty list: shows no-issues note for branch" -Expected $true `
    -Actual ($mdEmptyCiScan -match 'No ci-scan issues target')

# Without ciScanIssues key → no ci-scan section
$mdNoCiScan = Format-MarkdownReport -Data $mdData -RepoUrl 'https://github.com/dotnet/maui' `
                                    -TrackerKey 'net10-sr7' -MaxBodyBytes 60000
Assert-Eq -Label "No ciScanIssues key: section NOT rendered" -Expected $false `
    -Actual ($mdNoCiScan -match 'Recent CI Failure Scanner signals')

# Get-CiScanLabelForBranch: deterministic branch → label mapping
# Replaces both Get-CiScanIssueBranch (body-marker parser, deleted) and
# Get-CiScanLabels (label-list filter, deleted). Same convention: the
# label name fully encodes the source branch (`ci-scan` = main,
# `ci-scan-net11` = net11.0, `ci-scan-net12` = net12.0, etc.).
# Preview branches are mapped to their parent net<N>.0 so an in-flight
# preview readiness still surfaces signals from the branch the preview
# was cut from.
Write-Host "`n[Unit] Get-CiScanLabelForBranch returns canonical label per branch" -ForegroundColor Cyan

Assert-Eq -Label "'main' → 'ci-scan'" -Expected 'ci-scan' `
    -Actual (Get-CiScanLabelForBranch -Branch 'main')
Assert-Eq -Label "'net11.0' → 'ci-scan-net11'" -Expected 'ci-scan-net11' `
    -Actual (Get-CiScanLabelForBranch -Branch 'net11.0')
Assert-Eq -Label "'net12.0' → 'ci-scan-net12' (future-proof)" -Expected 'ci-scan-net12' `
    -Actual (Get-CiScanLabelForBranch -Branch 'net12.0')
Assert-Eq -Label "preview branch → parent net<N>.0 label" -Expected 'ci-scan-net11' `
    -Actual (Get-CiScanLabelForBranch -Branch 'release/11.0.1xx-preview6')
Assert-Eq -Label "SR branch → null (no scanner configured)" -Expected $null `
    -Actual (Get-CiScanLabelForBranch -Branch 'release/10.0.1xx-sr8')
Assert-Eq -Label "empty branch → null" -Expected $null `
    -Actual (Get-CiScanLabelForBranch -Branch '')
Assert-Eq -Label "garbage branch → null" -Expected $null `
    -Actual (Get-CiScanLabelForBranch -Branch 'feature/foo')


# ───── Get-CandidatePrChecks computes nextSr label from priorSrBranch ─────
# The check label uses 'SR9' (next SR) not 'SR8' (prior SR / branch passed
# to -SrBranch in candidate mode). Lock the regex that extracts the SR
# number from the prior SR branch name and increments it.
Write-Host "`n[Unit] nextSr label derivation from priorSrBranch" -ForegroundColor Cyan

function Get-NextSrLabel {
    param([string]$PriorSrBranch)
    if ($PriorSrBranch -and $PriorSrBranch -match 'sr(\d+)$') {
        return "SR$([int]$Matches[1] + 1)"
    }
    return $null
}

Assert-Eq -Label "release/10.0.1xx-sr8 → SR9" -Expected 'SR9' `
    -Actual (Get-NextSrLabel 'release/10.0.1xx-sr8')
Assert-Eq -Label "release/9.0.2xx-sr5 → SR6" -Expected 'SR6' `
    -Actual (Get-NextSrLabel 'release/9.0.2xx-sr5')
Assert-Eq -Label "release/10.0.1xx-sr10 → SR11 (two-digit)" -Expected 'SR11' `
    -Actual (Get-NextSrLabel 'release/10.0.1xx-sr10')
Assert-Eq -Label "main → null (not an SR branch)" -Expected $null `
    -Actual (Get-NextSrLabel 'main')
Assert-Eq -Label "empty → null" -Expected $null `
    -Actual (Get-NextSrLabel '')


# ───── Regression test: ConvertTo-Utc handles both string + DateTime inputs ─────
# ConvertFrom-Json already returns DateTime (Kind=Utc) for ISO-8601 'Z' strings.
# A naive [DateTime]::Parse(...) re-converts to Kind=Unspecified, which then
# ToUniversalTime() misinterprets as Local, silently shifting age by the host's
# UTC offset (e.g. PDT-shifted age becomes negative). Lock the contract.
Write-Host "`n[Unit] ConvertTo-Utc handles DateTime + string input identically" -ForegroundColor Cyan

# String input
$strUtc = ConvertTo-Utc -Value '2026-06-11T01:53:28Z'
Assert-Eq -Label "String 'Z' input → Kind=Utc" -Expected ([DateTimeKind]::Utc) -Actual $strUtc.Kind
Assert-Eq -Label "String 'Z' input → correct hour" -Expected 1 -Actual $strUtc.Hour

# DateTime input (already Utc — what ConvertFrom-Json produces)
$dtUtc = [DateTime]::SpecifyKind('2026-06-11T01:53:28', [DateTimeKind]::Utc)
$out = ConvertTo-Utc -Value $dtUtc
Assert-Eq -Label "DateTime (Utc) input → preserved" -Expected $dtUtc.Hour -Actual $out.Hour
Assert-Eq -Label "DateTime (Utc) input → Kind stays Utc" -Expected ([DateTimeKind]::Utc) -Actual $out.Kind

# DateTime input (Unspecified — assume UTC, don't apply local offset)
$dtUnspec = [DateTime]::SpecifyKind('2026-06-11T01:53:28', [DateTimeKind]::Unspecified)
$out2 = ConvertTo-Utc -Value $dtUnspec
Assert-Eq -Label "DateTime (Unspecified) input → assumed UTC (no offset shift)" -Expected 1 -Actual $out2.Hour

# Null / bad input
Assert-Eq -Label "Null input returns null" -Expected $true -Actual ($null -eq (ConvertTo-Utc -Value $null))
Assert-Eq -Label "Garbage string returns null" -Expected $true -Actual ($null -eq (ConvertTo-Utc -Value 'not-a-date'))

# End-to-end: Format-CiScanIssueRows with a DateTime (Utc) field — must produce
# the SAME age as the equivalent string. This is the exact bug we just hit.
$twoHoursAgo = (Get-Date).ToUniversalTime().AddHours(-2)
$twoHoursAgoUtc = [DateTime]::SpecifyKind($twoHoursAgo, [DateTimeKind]::Utc)
$issueWithDtField = @(
    [PSCustomObject]@{ number = 99999; url = 'https://github.com/dotnet/maui/issues/99999';
                       title = 'Bug repro'; createdAt = $twoHoursAgoUtc }
)
$rowsDt = Format-CiScanIssueRows -Issues $issueWithDtField -RepoUrl 'https://github.com/dotnet/maui'
Assert-Eq -Label "DateTime createdAt (Utc): age positive (no '-Nh ago' bug)" -Expected $false `
    -Actual ($rowsDt -match '-\d+h ago')
Assert-Eq -Label "DateTime createdAt (Utc): rendered as 2h or 3h ago, not negative" -Expected $true `
    -Actual ($rowsDt -match '[23]h ago')

# ───── Get-AzdoProp: safe AzDO API property access under StrictMode ─────
# Real-world regression: SR8 had an in-progress build (status=inProgress, no
# 'result' field) and Set-StrictMode -Version Latest threw on $latest.result.
# Tests below lock the contract that Get-AzdoProp tolerates missing properties.
Write-Host "`n[Unit] Get-AzdoProp safe AzDO property access" -ForegroundColor Cyan

$completedBuild = [PSCustomObject]@{ id = 1; result = 'succeeded'; status = 'completed'; sourceVersion = 'sha1'; finishTime = '2026-06-11T10:00:00Z' }
$inProgressBuild = [PSCustomObject]@{ id = 2; status = 'inProgress'; sourceVersion = 'sha2' }   # NO 'result', NO 'finishTime'

Assert-Eq -Label "Get-AzdoProp returns value for present property" -Expected 'succeeded' -Actual (Get-AzdoProp $completedBuild 'result')
Assert-Eq -Label "Get-AzdoProp returns null for missing property (no throw under StrictMode)" -Expected $true -Actual ($null -eq (Get-AzdoProp $inProgressBuild 'result'))
Assert-Eq -Label "Get-AzdoProp returns null for missing 'finishTime'" -Expected $true -Actual ($null -eq (Get-AzdoProp $inProgressBuild 'finishTime'))
Assert-Eq -Label "Get-AzdoProp returns null when input is null" -Expected $true -Actual ($null -eq (Get-AzdoProp $null 'anything'))
Assert-Eq -Label "Get-AzdoProp returns status field on in-progress build" -Expected 'inProgress' -Actual (Get-AzdoProp $inProgressBuild 'status')
# Nested access (used for $latest._links.web.href) — multi-level missing must also be safe
$noLinksBuild = [PSCustomObject]@{ id = 3; status = 'inProgress' }
$innerLinks = Get-AzdoProp $noLinksBuild '_links'
Assert-Eq -Label "Get-AzdoProp nested: null base → null result" -Expected $true -Actual ($null -eq $innerLinks)
# Hashtable input (the API response is sometimes constructed as a hashtable in tests)
$hashLike = [PSCustomObject]@{ value = @('a','b') }
$hashVal = Get-AzdoProp $hashLike 'value'
Assert-Eq -Label "Get-AzdoProp returns array value when 'value' present" -Expected '2' -Actual "$($hashVal.Count)"

# ──────────────────────────────────────────────────────────────────────────
# Get-MaestroOperationalChecks — BAR / darc default-channel & build lookups
# ──────────────────────────────────────────────────────────────────────────
Write-Host "`n[Unit] Get-MaestroOperationalChecks — BAR default-channel + per-commit build" -ForegroundColor Cyan

function Invoke-MaestroChecksWithMocks {
    <#
        Test harness for Get-MaestroOperationalChecks.
        Mocks Test-DarcAvailable + Invoke-DarcJson so we exercise the real check
        logic without needing darc, BAR auth, or network access.

        Parameters:
          -DarcAvailable          $true|$false — controls Test-DarcAvailable response
          -DefaultChannelsAuthFail switch — when set, mock returns Success=$false
          -DefaultChannelsResponse  array of mock mappings (used when not auth-failing).
                                    Empty array = darc returned no mappings.
          -BuildAuthFail           switch — when set, mock returns Success=$false
          -BuildResponse           array of mock builds; empty = no builds for HEAD
          -SrBranch / -SrHeadSha / -Mode / -SkipChecks — passed through to ctx
    #>
    param(
        [bool]$DarcAvailable = $true,
        [switch]$DefaultChannelsAuthFail,
        $DefaultChannelsResponse = @(),
        [switch]$BuildAuthFail,
        $BuildResponse = @(),
        [string]$SrBranch = 'release/10.0.1xx-sr8',
        [string]$SrHeadSha = 'a11840bfdeadbeefcafebabe1234567890abcdef',
        [string]$Mode = 'in-flight',
        [switch]$SkipChecks
    )
    $script:_mockDarcAvail = $DarcAvailable
    $script:_mockDCAuthFail = [bool]$DefaultChannelsAuthFail
    $script:_mockDC = @($DefaultChannelsResponse)
    $script:_mockBuildAuthFail = [bool]$BuildAuthFail
    $script:_mockBuilds = @($BuildResponse)

    function global:Test-DarcAvailable { return $script:_mockDarcAvail }
    function global:Invoke-DarcJson {
        param([string[]]$DarcArgs)
        if ($DarcArgs[0] -eq 'get-default-channels') {
            if ($script:_mockDCAuthFail) {
                return [PSCustomObject]@{ Success = $false; Data = @() }
            }
            return [PSCustomObject]@{ Success = $true; Data = @($script:_mockDC) }
        }
        if ($DarcArgs[0] -eq 'get-build') {
            if ($script:_mockBuildAuthFail) {
                return [PSCustomObject]@{ Success = $false; Data = @() }
            }
            return [PSCustomObject]@{ Success = $true; Data = @($script:_mockBuilds) }
        }
        return [PSCustomObject]@{ Success = $false; Data = @() }
    }

    try {
        $ctx = @{
            repo       = 'dotnet/maui'
            srBranch   = $SrBranch
            srRef      = "origin/$SrBranch"
            srHeadSha  = $SrHeadSha
            mode       = $Mode
            mainBranch = 'main'
        }
        return Get-MaestroOperationalChecks -Ctx $ctx -SkipChecks:$SkipChecks
    } finally {
        Remove-Item function:global:Test-DarcAvailable -ErrorAction SilentlyContinue
        Remove-Item function:global:Invoke-DarcJson -ErrorAction SilentlyContinue
    }
}

# Helper: find a check whose Area STARTS WITH a prefix (the SR HEAD short SHA
# varies per test fixture, so we can't match the full Area string).
function Get-MaestroCheckByPrefix {
    param($Checks, [string]$Prefix)
    @($Checks | Where-Object { $_.Area.StartsWith($Prefix) }) | Select-Object -First 1
}

# Fixture: realistic get-default-channels response (subset, includes SR7 + SR8
# absent, mirroring the real-world SR8-not-wired state we discovered).
$mockChannelsWithSr7 = @(
    [PSCustomObject]@{ id = 6945; repository = 'https://github.com/dotnet/maui'; branch = 'release/10.0.1xx-sr7'; enabled = $true; channel = [PSCustomObject]@{ id = 5174; name = '.NET 10.0.1xx SDK'; classification = 'product' } }
    [PSCustomObject]@{ id = 6604; repository = 'https://github.com/dotnet/maui'; branch = 'main';                  enabled = $true; channel = [PSCustomObject]@{ id = 5174; name = '.NET 10.0.1xx SDK'; classification = 'product' } }
)
$mockChannelsWithSr8 = $mockChannelsWithSr7 + @(
    [PSCustomObject]@{ id = 7100; repository = 'https://github.com/dotnet/maui'; branch = 'release/10.0.1xx-sr8'; enabled = $true; channel = [PSCustomObject]@{ id = 5174; name = '.NET 10.0.1xx SDK'; classification = 'product' } }
)
$mockChannelsSr8Disabled = $mockChannelsWithSr7 + @(
    [PSCustomObject]@{ id = 7100; repository = 'https://github.com/dotnet/maui'; branch = 'release/10.0.1xx-sr8'; enabled = $false; channel = [PSCustomObject]@{ id = 5174; name = '.NET 10.0.1xx SDK'; classification = 'product' } }
)
$mockBuildForHead = @(
    [PSCustomObject]@{
        id = 318278; repository = 'https://github.com/dotnet/maui'; branch = 'release/10.0.1xx-sr8'
        commit = 'a11840bfdeadbeefcafebabe1234567890abcdef'; buildNumber = '20260610.5'
        dateProduced = '6/11/2026 1:53 AM'; buildLink = 'https://dev.azure.com/dnceng/internal/_build/results?buildId=2997620'
        azdoBuildId = 2997620; released = $false; channels = @('.NET 10.0.1xx SDK')
    }
)

# ── Scenario 1: darc unavailable (CI) — both checks UNKNOWN with hints ──
$s1 = Invoke-MaestroChecksWithMocks -DarcAvailable $false
Assert-Eq -Label "darc-unavailable: emits exactly 2 checks" -Expected 2 -Actual @($s1).Count
$s1Map = Get-MaestroCheckByPrefix -Checks $s1 -Prefix 'BAR default-channel'
Assert-Eq -Label "darc-unavailable: mapping check is UNKNOWN" -Expected 'UNKNOWN' -Actual $s1Map.Status
Assert-Eq -Label "darc-unavailable: mapping NextAction mentions add-default-channel" -Expected $true `
    -Actual ($s1Map.NextAction -match 'add-default-channel')
$s1Build = Get-MaestroCheckByPrefix -Checks $s1 -Prefix 'BAR build for SR HEAD'
Assert-Eq -Label "darc-unavailable: build check is UNKNOWN" -Expected 'UNKNOWN' -Actual $s1Build.Status

# ── Scenario 2: SR branch present in BAR mappings + build for HEAD → 2x READY ──
$s2 = Invoke-MaestroChecksWithMocks -DefaultChannelsResponse $mockChannelsWithSr8 -BuildResponse $mockBuildForHead
$s2Map = Get-MaestroCheckByPrefix -Checks $s2 -Prefix 'BAR default-channel'
Assert-Eq -Label "sr-mapped + build-present: mapping is READY" -Expected 'READY' -Actual $s2Map.Status
Assert-Eq -Label "sr-mapped + build-present: mapping details name the channel" -Expected $true `
    -Actual ($s2Map.Details -match '\.NET 10\.0\.1xx SDK')
$s2Build = Get-MaestroCheckByPrefix -Checks $s2 -Prefix 'BAR build for SR HEAD'
Assert-Eq -Label "sr-mapped + build-present: build check is READY" -Expected 'READY' -Actual $s2Build.Status
Assert-Eq -Label "sr-mapped + build-present: build details show build number" -Expected $true `
    -Actual ($s2Build.Details -match '20260610\.5')

# ── Scenario 3: SR branch MISSING from BAR (the SR8 real-world bug) → BLOCKED ──
$s3 = Invoke-MaestroChecksWithMocks -DefaultChannelsResponse $mockChannelsWithSr7 -BuildResponse @()
$s3Map = Get-MaestroCheckByPrefix -Checks $s3 -Prefix 'BAR default-channel'
Assert-Eq -Label "sr-not-mapped: mapping is BLOCKED" -Expected 'BLOCKED' -Actual $s3Map.Status
Assert-Eq -Label "sr-not-mapped: mapping details mention 'NO default-channel mapping'" -Expected $true `
    -Actual ($s3Map.Details -match 'NO default-channel mapping')
Assert-Eq -Label "sr-not-mapped: mapping NextAction has the exact darc add-default-channel command" -Expected $true `
    -Actual ($s3Map.NextAction -match 'darc add-default-channel.*--channel ".NET 10\.0\.1xx SDK"')

# ── Scenario 4: SR mapping exists but disabled → still BLOCKED (treated as missing) ──
$s4 = Invoke-MaestroChecksWithMocks -DefaultChannelsResponse $mockChannelsSr8Disabled
$s4Map = Get-MaestroCheckByPrefix -Checks $s4 -Prefix 'BAR default-channel'
Assert-Eq -Label "sr-mapped-but-disabled: still BLOCKED" -Expected 'BLOCKED' -Actual $s4Map.Status

# ── Scenario 5: get-default-channels returns null (auth failure) → UNKNOWN ──
$s5 = Invoke-MaestroChecksWithMocks -DefaultChannelsAuthFail
$s5Map = Get-MaestroCheckByPrefix -Checks $s5 -Prefix 'BAR default-channel'
Assert-Eq -Label "darc-call-failed: mapping is UNKNOWN with auth-issue hint" -Expected 'UNKNOWN' -Actual $s5Map.Status
Assert-Eq -Label "darc-call-failed: mapping details mention auth/network" -Expected $true `
    -Actual ($s5Map.Details -match 'auth')

# ── Scenario 6: mapping OK but no build for HEAD → WATCH (CI in flight) ──
$s6 = Invoke-MaestroChecksWithMocks -DefaultChannelsResponse $mockChannelsWithSr8 -BuildResponse @()
$s6Build = Get-MaestroCheckByPrefix -Checks $s6 -Prefix 'BAR build for SR HEAD'
Assert-Eq -Label "no-build-for-head: build check is WATCH (not BLOCKED — transient)" -Expected 'WATCH' -Actual $s6Build.Status

# ── Scenario 7: candidate mode → no checks emitted (SR doesn't exist yet) ──
$s7 = Invoke-MaestroChecksWithMocks -Mode 'candidate' -DefaultChannelsResponse $mockChannelsWithSr8
Assert-Eq -Label "candidate-mode: emits 0 checks" -Expected 0 -Actual @($s7).Count

# ── Scenario 8: -SkipChecks switch → no checks emitted ──
$s8 = Invoke-MaestroChecksWithMocks -SkipChecks -DefaultChannelsResponse $mockChannelsWithSr8
Assert-Eq -Label "skip-checks: emits 0 checks" -Expected 0 -Actual @($s8).Count

# ── Scenario 9: non-SR branch shape → no checks (don't guess channel name) ──
$s9 = Invoke-MaestroChecksWithMocks -SrBranch 'release/11.0.1xx-preview5' -DefaultChannelsResponse $mockChannelsWithSr8
Assert-Eq -Label "preview-branch (not -srN): emits 0 checks (channel inference doesn't apply)" -Expected 0 -Actual @($s9).Count

# ── Scenario 10: SR HEAD SHA absent from ctx → only mapping check, no build check ──
$s10 = Invoke-MaestroChecksWithMocks -SrHeadSha '' -DefaultChannelsResponse $mockChannelsWithSr8
$s10Build = Get-MaestroCheckByPrefix -Checks $s10 -Prefix 'BAR build for SR HEAD'
Assert-Eq -Label "no-head-sha: build check is absent (only mapping emitted)" -Expected $true -Actual ($null -eq $s10Build)
Assert-Eq -Label "no-head-sha: still emits exactly 1 check (the mapping)" -Expected 1 -Actual @($s10).Count

# ── Scenario 11: get-build returns null (auth failure) → build check UNKNOWN ──
$s11 = Invoke-MaestroChecksWithMocks -DefaultChannelsResponse $mockChannelsWithSr8 -BuildAuthFail
$s11Build = Get-MaestroCheckByPrefix -Checks $s11 -Prefix 'BAR build for SR HEAD'
Assert-Eq -Label "build-call-failed: build check is UNKNOWN" -Expected 'UNKNOWN' -Actual $s11Build.Status

# ── Scenario 12: multiple builds for HEAD → picks highest BAR id ──
$multipleBuilds = @(
    [PSCustomObject]@{ id = 318100; buildNumber = '20260609.1'; buildLink = 'https://example/1'; channels = @('.NET 10.0.1xx SDK') }
    [PSCustomObject]@{ id = 318278; buildNumber = '20260610.5'; buildLink = 'https://example/2'; channels = @('.NET 10.0.1xx SDK') }
    [PSCustomObject]@{ id = 318200; buildNumber = '20260609.7'; buildLink = 'https://example/3'; channels = @('.NET 10.0.1xx SDK') }
)
$s12 = Invoke-MaestroChecksWithMocks -DefaultChannelsResponse $mockChannelsWithSr8 -BuildResponse $multipleBuilds
$s12Build = Get-MaestroCheckByPrefix -Checks $s12 -Prefix 'BAR build for SR HEAD'
Assert-Eq -Label "multiple-builds: details report highest-id build (20260610.5)" -Expected $true `
    -Actual ($s12Build.Details -match '20260610\.5')

# ── Scenario 13: SR7 branch (real, currently mapped) → READY (sanity) ──
$s13 = Invoke-MaestroChecksWithMocks -SrBranch 'release/10.0.1xx-sr7' -DefaultChannelsResponse $mockChannelsWithSr7 -BuildResponse $mockBuildForHead
$s13Map = Get-MaestroCheckByPrefix -Checks $s13 -Prefix 'BAR default-channel'
Assert-Eq -Label "sr7-already-mapped: READY" -Expected 'READY' -Actual $s13Map.Status

# =========================================================================
# Get-MilestoneHygieneChecks — current/next milestone existence + stale detection
# =========================================================================
Write-Host "`n[Unit] Get-MilestoneHygieneChecks — current/next milestone existence + stale detection" -ForegroundColor Cyan

# Mock harness — overrides Get-AllMilestones globally with a fixture, exercises
# the real Get-MilestoneHygieneChecks logic, then restores. Mirrors the
# Maestro mock pattern so any test scaffolding learning here transfers.
function Invoke-MilestoneChecksWithMocks {
    param(
        [switch]$ApiFail,
        $MilestonesResponse = @(),
        [string]$SrBranch = 'release/10.0.1xx-sr8',
        [string]$PriorSrBranch,
        [string]$Mode = 'in-flight',
        [switch]$SkipChecks
    )
    $script:_mockMsApiFail = [bool]$ApiFail
    $script:_mockMsData = @($MilestonesResponse)

    function global:Get-AllMilestones {
        param([string]$Repo)
        if ($script:_mockMsApiFail) {
            return [PSCustomObject]@{ Success = $false; Data = @() }
        }
        return [PSCustomObject]@{ Success = $true; Data = @($script:_mockMsData) }
    }

    try {
        $ctx = @{
            repo          = 'dotnet/maui'
            srBranch      = if ($Mode -eq 'candidate') { 'main' } else { $SrBranch }
            priorSrBranch = if ($Mode -eq 'candidate') { $PriorSrBranch } else { $null }
            mode          = $Mode
        }
        return Get-MilestoneHygieneChecks -Ctx $ctx -SkipChecks:$SkipChecks
    } finally {
        Remove-Item function:global:Get-AllMilestones -ErrorAction SilentlyContinue
    }
}

function Get-MilestoneCheckByPrefix {
    param($Checks, [string]$Prefix)
    if (-not $Checks) { return $null }
    return @($Checks) | Where-Object { $_.Area -like "$Prefix*" } | Select-Object -First 1
}

# Helper to build mock milestone objects with the shape returned by gh API
function New-MockMilestone {
    param(
        [string]$Title,
        [string]$State = 'open',
        [int]$Number = 100,
        [int]$OpenIssues = 0,
        $DueOn = $null  # ISO-8601 string; null = no due date
    )
    [PSCustomObject]@{
        title       = $Title
        state       = $State
        number      = $Number
        open_issues = $OpenIssues
        due_on      = $DueOn
    }
}

# === Common fixtures ===
# Past dates relative to now so the test stays valid as time passes
$daysAgo30  = (Get-Date).ToUniversalTime().AddDays(-30).ToString('o')
$daysAgo60  = (Get-Date).ToUniversalTime().AddDays(-60).ToString('o')
$daysAgo3   = (Get-Date).ToUniversalTime().AddDays(-3).ToString('o')   # within grace
$daysAgo10  = (Get-Date).ToUniversalTime().AddDays(-10).ToString('o')  # past grace
$daysAhead30 = (Get-Date).ToUniversalTime().AddDays(30).ToString('o')

$mockMsAllPresent = @(
    (New-MockMilestone -Title '.NET 10 SR8' -Number 117 -OpenIssues 50 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 10 SR9' -Number 118 -DueOn $daysAhead30)
    (New-MockMilestone -Title 'Backlog')  # no due date — always excluded
    (New-MockMilestone -Title '.NET 11 Planning')  # planning excluded
)

# ── Scenario M1: Current + next milestone exist, nothing stale → 0 checks ──
$m1 = Invoke-MilestoneChecksWithMocks -MilestonesResponse $mockMsAllPresent
Assert-Eq -Label "M1: all present, no stale → 0 checks emitted" -Expected 0 -Actual @($m1).Count

# ── Scenario M2: SR8 milestone missing → BLOCKED current ──
$m2Data = @(
    (New-MockMilestone -Title '.NET 10 SR9' -Number 118 -DueOn $daysAhead30)
)
$m2 = Invoke-MilestoneChecksWithMocks -MilestonesResponse $m2Data
$m2Curr = Get-MilestoneCheckByPrefix -Checks $m2 -Prefix 'Milestone for current cycle'
Assert-Eq -Label "M2: current missing → BLOCKED check emitted" -Expected 'BLOCKED' -Actual $m2Curr.Status
Assert-Eq -Label "M2: current missing → details name the exact missing title" -Expected $true `
    -Actual ($m2Curr.Details -match '\.NET 10 SR8')
Assert-Eq -Label "M2: current missing → action has gh api create command" -Expected $true `
    -Actual ($m2Curr.NextAction -match 'gh api repos/dotnet/maui/milestones')

# ── Scenario M3: SR9 milestone missing → CLEANUP next ──
# Per Finding #5 follow-up: missing roll-forward milestone is housekeeping,
# not a ship blocker. The current cycle (SR8) can still ship while the
# next milestone (SR9) is created later.
$m3Data = @(
    (New-MockMilestone -Title '.NET 10 SR8' -Number 117 -OpenIssues 50 -DueOn $daysAhead30)
)
$m3 = Invoke-MilestoneChecksWithMocks -MilestonesResponse $m3Data
$m3Next = Get-MilestoneCheckByPrefix -Checks $m3 -Prefix 'Milestone for next cycle'
Assert-Eq -Label "M3: next missing → CLEANUP check emitted (not ship-blocker)" -Expected 'CLEANUP' -Actual $m3Next.Status
Assert-Eq -Label "M3: next missing → action proposes creating SR9" -Expected $true `
    -Actual ($m3Next.NextAction -match '\.NET 10 SR9')

# ── Scenario M4: Legacy ".NET 10.0 SR8" naming also satisfies current check ──
$m4Data = @(
    (New-MockMilestone -Title '.NET 10.0 SR8' -Number 117 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 10 SR9' -Number 118 -DueOn $daysAhead30)
)
$m4 = Invoke-MilestoneChecksWithMocks -MilestonesResponse $m4Data
$m4Curr = Get-MilestoneCheckByPrefix -Checks $m4 -Prefix 'Milestone for current cycle'
Assert-Eq -Label "M4: legacy 'X.0 SRn' title satisfies current check" -Expected $true -Actual ($null -eq $m4Curr)

# ── Scenario M5: Stale .NET 10 milestone past 7-day grace → BLOCKED ──
$m5Data = @(
    (New-MockMilestone -Title '.NET 10 SR8' -Number 117 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 10 SR9' -Number 118 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 10 SR6' -Number 115 -OpenIssues 76 -DueOn $daysAgo60)
    (New-MockMilestone -Title '.NET 10 SR7' -Number 116 -OpenIssues 63 -DueOn $daysAgo30)
)
$m5 = Invoke-MilestoneChecksWithMocks -MilestonesResponse $m5Data
$m5Stale = Get-MilestoneCheckByPrefix -Checks $m5 -Prefix 'Stale open milestones'
Assert-Eq -Label "M5: stale SR6+SR7 → CLEANUP check emitted (housekeeping, not blocking)" -Expected 'CLEANUP' -Actual $m5Stale.Status
Assert-Eq -Label "M5: stale count reflected in area" -Expected $true -Actual ($m5Stale.Area -match '\(2\)')
Assert-Eq -Label "M5: details mention SR6 by title" -Expected $true -Actual ($m5Stale.Details -match 'SR6')
Assert-Eq -Label "M5: details mention SR7 by title" -Expected $true -Actual ($m5Stale.Details -match 'SR7')

# ── Scenario M6: Past-due within 7-day grace → NOT flagged ──
$m6Data = @(
    (New-MockMilestone -Title '.NET 10 SR8' -Number 117 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 10 SR9' -Number 118 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 10 SR7' -Number 116 -OpenIssues 5 -DueOn $daysAgo3)  # within grace
)
$m6 = Invoke-MilestoneChecksWithMocks -MilestonesResponse $m6Data
$m6Stale = Get-MilestoneCheckByPrefix -Checks $m6 -Prefix 'Stale open milestones'
Assert-Eq -Label "M6: within 7-day grace → no stale check" -Expected $true -Actual ($null -eq $m6Stale)

# ── Scenario M7: Closed milestone past due → NOT flagged ──
$m7Data = @(
    (New-MockMilestone -Title '.NET 10 SR8' -Number 117 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 10 SR9' -Number 118 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 10 SR6' -Number 115 -State 'closed' -DueOn $daysAgo60)
)
$m7 = Invoke-MilestoneChecksWithMocks -MilestonesResponse $m7Data
$m7Stale = Get-MilestoneCheckByPrefix -Checks $m7 -Prefix 'Stale open milestones'
Assert-Eq -Label "M7: closed milestone never flagged stale" -Expected $true -Actual ($null -eq $m7Stale)

# ── Scenario M8: Backlog with no due_on → NOT flagged ──
$m8Data = @(
    (New-MockMilestone -Title '.NET 10 SR8' -Number 117 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 10 SR9' -Number 118 -DueOn $daysAhead30)
    (New-MockMilestone -Title 'Backlog' -Number 1 -OpenIssues 3000)  # no due
)
$m8 = Invoke-MilestoneChecksWithMocks -MilestonesResponse $m8Data
Assert-Eq -Label "M8: Backlog never flagged stale" -Expected 0 -Actual @($m8).Count

# ── Scenario M9: Cross-major staleness → NOT flagged (cycle isolation) ──
# Surveying SR8 of .NET 10; stale .NET 9 SR9 should NOT flag (different major).
$m9Data = @(
    (New-MockMilestone -Title '.NET 10 SR8' -Number 117 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 10 SR9' -Number 118 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 9 SR9' -Number 50 -OpenIssues 10 -DueOn $daysAgo60)
)
$m9 = Invoke-MilestoneChecksWithMocks -MilestonesResponse $m9Data
$m9Stale = Get-MilestoneCheckByPrefix -Checks $m9 -Prefix 'Stale open milestones'
Assert-Eq -Label "M9: .NET 9 stale milestones don't flag when surveying .NET 10 SR" -Expected $true -Actual ($null -eq $m9Stale)

# ── Scenario M10: Cross-cycle staleness → NOT flagged (SR/preview isolation) ──
# Surveying SR8 of .NET 10; stale .NET 10.0-preview1 should NOT flag (preview vs SR).
$m10Data = @(
    (New-MockMilestone -Title '.NET 10 SR8' -Number 117 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 10 SR9' -Number 118 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 10.0-preview1' -Number 40 -OpenIssues 5 -DueOn $daysAgo60)
)
$m10 = Invoke-MilestoneChecksWithMocks -MilestonesResponse $m10Data
$m10Stale = Get-MilestoneCheckByPrefix -Checks $m10 -Prefix 'Stale open milestones'
Assert-Eq -Label "M10: preview milestones don't flag when surveying an SR cycle" -Expected $true -Actual ($null -eq $m10Stale)

# ── Scenario M11: Preview branch surveys preview milestones ──
$m11Data = @(
    (New-MockMilestone -Title '.NET 11.0-preview5' -Number 200 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 11.0-preview6' -Number 201 -DueOn $daysAhead30)
)
$m11 = Invoke-MilestoneChecksWithMocks -SrBranch 'release/11.0.1xx-preview5' -MilestonesResponse $m11Data
Assert-Eq -Label "M11: preview branch all-present → 0 checks" -Expected 0 -Actual @($m11).Count

# ── Scenario M12: Preview branch missing next-preview → CLEANUP ──
# Per Finding #5 follow-up: missing roll-forward (preview6) milestone is
# cleanup, not a ship blocker for preview5.
$m12Data = @(
    (New-MockMilestone -Title '.NET 11.0-preview5' -Number 200 -DueOn $daysAhead30)
)
$m12 = Invoke-MilestoneChecksWithMocks -SrBranch 'release/11.0.1xx-preview5' -MilestonesResponse $m12Data
$m12Next = Get-MilestoneCheckByPrefix -Checks $m12 -Prefix 'Milestone for next cycle'
Assert-Eq -Label "M12: preview6 missing → CLEANUP next-cycle check (not ship-blocker)" -Expected 'CLEANUP' -Actual $m12Next.Status
Assert-Eq -Label "M12: details name preview6 by exact title" -Expected $true `
    -Actual ($m12Next.Area -match '\.NET 11\.0-preview6')

# ── Scenario M13: Candidate mode for SR (priorSr = SR7 → candidate is SR8) ──
$m13Data = @(
    (New-MockMilestone -Title '.NET 10 SR8' -Number 117 -DueOn $daysAhead30)
    (New-MockMilestone -Title '.NET 10 SR9' -Number 118 -DueOn $daysAhead30)
)
$m13 = Invoke-MilestoneChecksWithMocks -Mode 'candidate' -PriorSrBranch 'release/10.0.1xx-sr7' -MilestonesResponse $m13Data
Assert-Eq -Label "M13: candidate-mode SR (prior=SR7) accepts SR8/SR9 → 0 checks" -Expected 0 -Actual @($m13).Count

# ── Scenario M14: -SkipChecks → 0 checks even with missing milestones ──
$m14 = Invoke-MilestoneChecksWithMocks -SkipChecks -MilestonesResponse @()
Assert-Eq -Label "M14: SkipChecks emits 0 checks" -Expected 0 -Actual @($m14).Count

# ── Scenario M15: Non-SR / non-preview branch → 0 checks (silent skip) ──
$m15 = Invoke-MilestoneChecksWithMocks -SrBranch 'release/10.0.1xx-rc1' -MilestonesResponse @()
Assert-Eq -Label "M15: RC branch shape → 0 checks (can't infer milestone name)" -Expected 0 -Actual @($m15).Count

# ── Scenario M16: API failure → UNKNOWN check (gh auth gap) ──
$m16 = Invoke-MilestoneChecksWithMocks -ApiFail
$m16Unk = Get-MilestoneCheckByPrefix -Checks $m16 -Prefix 'Milestone hygiene'
Assert-Eq -Label "M16: API fail → UNKNOWN status" -Expected 'UNKNOWN' -Actual $m16Unk.Status
Assert-Eq -Label "M16: API fail action mentions gh auth status" -Expected $true `
    -Actual ($m16Unk.NextAction -match 'gh auth status')

# ───── Get-ExpectedShipDate: deterministic 2nd-Tuesday math + hotfix cadence ─────
# .NET releases ship on the 2nd Tuesday of every month for x0 patches (80, 90, 100…)
# and previews. Hotfix patches (81, 82…) ship ASAP — no cadence.
Write-Host "`n[Unit] Get-ExpectedShipDate (2nd Tuesday + hotfix)" -ForegroundColor Cyan

# 2nd Tuesday calendar for sanity (verified independently):
#   June 2026:  2nd Tue = June 9
#   July 2026:  2nd Tue = July 14
#   Aug 2026:   2nd Tue = Aug 11
#   May 2026:   2nd Tue = May 12
#   Feb 2026:   2nd Tue = Feb 10 (no leap-week issue)

# Scenario T1: x0 patch + BEFORE this month's 2nd Tuesday → use this month
$t1 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-01') -PatchVersion 80
Assert-Eq -Label "T1: 06-01 + patch=80 → cadence second-tuesday" -Expected 'second-tuesday' -Actual $t1.Cadence
Assert-Eq -Label "T1: 06-01 → June 9 2026"  -Expected '2026-06-09' -Actual $t1.Date.ToString('yyyy-MM-dd')
Assert-Eq -Label "T1: days from 06-01 = 8"   -Expected 8           -Actual $t1.DaysFromNow

# Scenario T2: x0 patch + AFTER this month's 2nd Tuesday → roll to next month
$t2 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-11') -PatchVersion 80
Assert-Eq -Label "T2: 06-11 (past June 9) → July 14 2026" -Expected '2026-07-14' -Actual $t2.Date.ToString('yyyy-MM-dd')
Assert-Eq -Label "T2: days from 06-11 = 33"                -Expected 33           -Actual $t2.DaysFromNow

# Scenario T3: today IS the 2nd Tuesday → return today (DaysFromNow = 0)
$t3 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-09') -PatchVersion 80
Assert-Eq -Label "T3: 06-09 IS June's 2nd Tue → 06-09 returned" -Expected '2026-06-09' -Actual $t3.Date.ToString('yyyy-MM-dd')
Assert-Eq -Label "T3: days from shipping day = 0"               -Expected 0           -Actual $t3.DaysFromNow

# Scenario T4: month starts on a Tuesday → first Tue is day 1, second Tue is day 8
# Sept 2026 starts on a Tuesday (Sept 1 = Tue).
$t4 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-09-01') -PatchVersion 90
Assert-Eq -Label "T4: 09-01 (month starts on Tue) → Sept 8" -Expected '2026-09-08' -Actual $t4.Date.ToString('yyyy-MM-dd')

# Scenario T5: month rollover crossing year boundary — December past 2nd Tue → January
$t5 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-12-15') -PatchVersion 100
Assert-Eq -Label "T5: 12-15 (past Dec 8) → Jan 12 2027" -Expected '2027-01-12' -Actual $t5.Date.ToString('yyyy-MM-dd')

# Scenario T6: formatted string includes day-of-week + month name
$t6 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-11') -PatchVersion 80
Assert-Eq -Label "T6: FormattedLong contains 'Tuesday'" -Expected $true -Actual ([bool]($t6.FormattedLong -match '^Tuesday'))
Assert-Eq -Label "T6: FormattedLong contains 'July'"    -Expected $true -Actual ([bool]($t6.FormattedLong -match 'July'))
Assert-Eq -Label "T6: FormattedLong contains '14'"      -Expected $true -Actual ([bool]($t6.FormattedLong -match '\b14\b'))
Assert-Eq -Label "T6: FormattedLong contains '2026'"    -Expected $true -Actual ([bool]($t6.FormattedLong -match '2026'))

# Scenario T7: month starts on Wednesday (e.g. Jul 2026: Jul 1 = Wed) — first Tue = Jul 7, second Tue = Jul 14
$t7 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-07-01') -PatchVersion 80
Assert-Eq -Label "T7: 07-01 (month starts on Wed) → Jul 14" -Expected '2026-07-14' -Actual $t7.Date.ToString('yyyy-MM-dd')

# Scenario T8: time-of-day portion shouldn't affect the result
$t8 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-09T23:59:00Z') -PatchVersion 80
Assert-Eq -Label "T8: time-of-day stripped → 06-09 still recognized as shipping day" -Expected 0 -Actual $t8.DaysFromNow

# Scenario T9: patch=$null (caller doesn't know) → defaults to 2nd-Tuesday cadence
$t9 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-11')
Assert-Eq -Label "T9: patch=$null → cadence second-tuesday (back-compat)" -Expected 'second-tuesday' -Actual $t9.Cadence
Assert-Eq -Label "T9: patch=$null → still produces a date"                -Expected '2026-07-14' -Actual $t9.Date.ToString('yyyy-MM-dd')

# Scenario T10: hotfix patch (81) → ASAP, NO cadence
$t10 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-11') -PatchVersion 81
Assert-Eq -Label "T10: patch=81 → cadence asap-hotfix"     -Expected 'asap-hotfix' -Actual $t10.Cadence
Assert-Eq -Label "T10: patch=81 → Date is null"            -Expected $true         -Actual ($null -eq $t10.Date)
Assert-Eq -Label "T10: patch=81 → DaysFromNow is null"     -Expected $true         -Actual ($null -eq $t10.DaysFromNow)
Assert-Eq -Label "T10: patch=81 → FormattedLong mentions ASAP" -Expected $true     -Actual ([bool]($t10.FormattedLong -match 'ASAP'))
Assert-Eq -Label "T10: patch=81 → Note mentions hotfix"    -Expected $true         -Actual ([bool]($t10.Note -match 'hotfix'))
Assert-Eq -Label "T10: patch=81 → Note quotes the patch"   -Expected $true         -Actual ([bool]($t10.Note -match '\b81\b'))

# Scenario T11: hotfix mid-range (85) → still ASAP
$t11 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-11') -PatchVersion 85
Assert-Eq -Label "T11: patch=85 → cadence asap-hotfix" -Expected 'asap-hotfix' -Actual $t11.Cadence

# Scenario T12: another decade boundary — patch=91 (SR9 hotfix) → ASAP
$t12 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-11') -PatchVersion 91
Assert-Eq -Label "T12: patch=91 → cadence asap-hotfix" -Expected 'asap-hotfix' -Actual $t12.Cadence

# Scenario T13: preview/major-zero patch (0) → 2nd-Tuesday (0 % 10 == 0)
$t13 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-11') -PatchVersion 0
Assert-Eq -Label "T13: patch=0 (preview) → cadence second-tuesday" -Expected 'second-tuesday' -Actual $t13.Cadence

# Scenario T14: patch=100 (triple digit, % 10 == 0) → 2nd-Tuesday
$t14 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-11') -PatchVersion 100
Assert-Eq -Label "T14: patch=100 → cadence second-tuesday" -Expected 'second-tuesday' -Actual $t14.Cadence

# ───── Get-ExpectedShipDate with MainBumpDate anchoring ─────
# The real bug: without an anchor, the fallback rolls forward when the SR's
# month passes — so SR8 (patch=80, expected June 9) wrongly slid into July 14
# (SR9's window) once June 9 passed. MainBumpDate fixes that.

# T15: SR8 — main bumped 70→80 on 2026-05-13 → SR8 ships 2nd Tue of June (06-09).
# Today = 2026-06-01 (BEFORE June 9) → date = June 9, days = 8, not missed.
$t15 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-01') -PatchVersion 80 -MainBumpDate ([DateTime]'2026-05-13')
Assert-Eq -Label "T15: bump 05-13 + today 06-01 → 2026-06-09"    -Expected '2026-06-09'      -Actual $t15.Date.ToString('yyyy-MM-dd')
Assert-Eq -Label "T15: days from 06-01 = 8"                       -Expected 8                 -Actual $t15.DaysFromNow
Assert-Eq -Label "T15: not missed"                                -Expected $false            -Actual $t15.MissedWindow
Assert-Eq -Label "T15: anchorSource = main-bump"                  -Expected 'main-bump'       -Actual $t15.AnchorSource
Assert-Eq -Label "T15: cadence = second-tuesday"                  -Expected 'second-tuesday'  -Actual $t15.Cadence

# T16: SR8 — main bumped 70→80 on 2026-05-13. Today = 2026-06-11 (AFTER June 9).
# WITHOUT anchor, function would say July 14 (SR9 territory). WITH anchor,
# we get the correct June 9 date but flagged as missed.
$t16 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-11') -PatchVersion 80 -MainBumpDate ([DateTime]'2026-05-13')
Assert-Eq -Label "T16: bump 05-13 + today 06-11 → 2026-06-09 (still anchored)" -Expected '2026-06-09' -Actual $t16.Date.ToString('yyyy-MM-dd')
Assert-Eq -Label "T16: missedWindow = true"                       -Expected $true             -Actual $t16.MissedWindow
Assert-Eq -Label "T16: days from 06-11 = -2"                      -Expected -2                -Actual $t16.DaysFromNow
Assert-Eq -Label "T16: cadence = second-tuesday-missed"           -Expected 'second-tuesday-missed' -Actual $t16.Cadence

# T17: SR9 — main bumped 80→90 on 2026-06-15 → SR9 ships 2nd Tue of July (07-14).
# Today = 2026-06-11 → before bump, so this is more theoretical, but if you call
# with bump date 06-15 you get July 14.
$t17 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-11') -PatchVersion 90 -MainBumpDate ([DateTime]'2026-06-15')
Assert-Eq -Label "T17: bump 06-15 → 2026-07-14"                   -Expected '2026-07-14'      -Actual $t17.Date.ToString('yyyy-MM-dd')
Assert-Eq -Label "T17: anchorSource = main-bump"                  -Expected 'main-bump'       -Actual $t17.AnchorSource

# T18: anchor wins over fallback even when both would give same answer.
$t18 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-01') -PatchVersion 80
Assert-Eq -Label "T18: no MainBumpDate → fallback (current-month anchor)" -Expected 'fallback-current-month' -Actual $t18.AnchorSource

# T19: hotfix patch ignores MainBumpDate (cadence wins).
$t19 = Get-ExpectedShipDate -ReferenceDate ([DateTime]'2026-06-11') -PatchVersion 81 -MainBumpDate ([DateTime]'2026-05-13')
Assert-Eq -Label "T19: patch=81 + MainBumpDate → asap-hotfix"     -Expected 'asap-hotfix'     -Actual $t19.Cadence
Assert-Eq -Label "T19: missedWindow = false for hotfix"           -Expected $false            -Actual $t19.MissedWindow

# =========================================================================
# Test-IsP0Pr — preview engine p/0 PR blocker classification
# =========================================================================
# Regression guard for the gap where p/0-labelled PRs targeting a preview
# release branch were NOT surfaced as blockers (only p/0 *issues* were).
# gh issue list --label p/0 never returns PRs, so p/0 PRs (e.g. #34758,
# #35626 against net11.0) rendered as generic "Needs review or triage"
# WATCH rows. Test-IsP0Pr is the predicate that carves them out for hoisting.
Write-Host "`n[Unit] Test-IsP0Pr — p/0 PR blocker classification" -ForegroundColor Cyan

# Dot-source the preview engine to access its helpers without running the
# main driver (the InvocationName guard returns on dot-source). A valid
# -Branch is required to satisfy the mandatory parameter + branch parse.
$prevScript = Join-Path $PSScriptRoot '..' 'scripts' 'Get-PreviewReadiness.ps1'
. $prevScript -Branch 'release/11.0.1xx-preview6'

$p0Pr        = [PSCustomObject]@{ number = 34758; labels = @([PSCustomObject]@{ name = 'p/0' }, [PSCustomObject]@{ name = 'area-xaml' }) }
$nonP0Pr     = [PSCustomObject]@{ number = 99999; labels = @([PSCustomObject]@{ name = 'area-xaml' }, [PSCustomObject]@{ name = 'p/1' }) }
$missingLbls = [PSCustomObject]@{ number = 12345 }            # no labels property at all
$nullLbls    = [PSCustomObject]@{ number = 22222; labels = $null }
$emptyLbls   = [PSCustomObject]@{ number = 33333; labels = @() }
$hashLbls    = [PSCustomObject]@{ number = 44444; labels = @(@{ name = 'p/0' }) }   # hashtable-shaped labels
$hashPrP0    = @{ number = 55555; labels = @(@{ name = 'p/0' }, @{ name = 'area-xaml' }) }  # whole PR is a hashtable (test-mock shape)
$hashPrNonP0 = @{ number = 66666; labels = @(@{ name = 'p/1' }) }                            # hashtable PR, no p/0
$hashPrNoLbl = @{ number = 77777 }                                                          # hashtable PR, no labels key

Assert-Eq -Label "p/0-labelled PR → blocker"                  -Expected $true  -Actual (Test-IsP0Pr $p0Pr)
Assert-Eq -Label "non-p/0 PR (has p/1) → not a blocker"       -Expected $false -Actual (Test-IsP0Pr $nonP0Pr)
Assert-Eq -Label "PR missing labels property → false (StrictMode-safe)" -Expected $false -Actual (Test-IsP0Pr $missingLbls)
Assert-Eq -Label "PR with null labels → false"                -Expected $false -Actual (Test-IsP0Pr $nullLbls)
Assert-Eq -Label "PR with empty labels → false"               -Expected $false -Actual (Test-IsP0Pr $emptyLbls)
Assert-Eq -Label "hashtable-shaped labels still matched"      -Expected $true  -Actual (Test-IsP0Pr $hashLbls)
Assert-Eq -Label "null PR → false (no throw)"                 -Expected $false -Actual (Test-IsP0Pr $null)
# Whole-PR-as-hashtable (IDictionary) shape: common in test mocks; must not
# silently return $false (a hashtable's PSObject.Properties has no 'labels').
Assert-Eq -Label "hashtable PR with p/0 → blocker (IDictionary path)"  -Expected $true  -Actual (Test-IsP0Pr $hashPrP0)
Assert-Eq -Label "hashtable PR without p/0 → not a blocker"            -Expected $false -Actual (Test-IsP0Pr $hashPrNonP0)
Assert-Eq -Label "hashtable PR missing labels key → false (no throw)"  -Expected $false -Actual (Test-IsP0Pr $hashPrNoLbl)

# Carve-out semantics: the p/0 subset is selected, and the generic (WATCH)
# bucket has them removed — exactly what the engine does before hoisting.
$mixedPrs = @($p0Pr, $nonP0Pr, $hashLbls, $emptyLbls)
$p0Subset = @($mixedPrs | Where-Object { Test-IsP0Pr $_ })
$p0Nums   = @($p0Subset | ForEach-Object { $_.number })
$generic  = @($mixedPrs | Where-Object { $p0Nums -notcontains $_.number })
Assert-Eq -Label "carve-out: 2 of 4 PRs are p/0"              -Expected 2     -Actual $p0Subset.Count
Assert-Eq -Label "carve-out: p/0 subset contains #34758"      -Expected $true -Actual ($p0Nums -contains 34758)
Assert-Eq -Label "carve-out: p/0 subset contains #44444"      -Expected $true -Actual ($p0Nums -contains 44444)
Assert-Eq -Label "carve-out: generic bucket excludes p/0 PRs" -Expected 2     -Actual $generic.Count
Assert-Eq -Label "carve-out: generic bucket keeps #99999"     -Expected $true -Actual (@($generic | ForEach-Object { $_.number }) -contains 99999)

# Precedence: P/0 takes priority over author-type (Maestro) AND merge-up
# categorization. A p/0-labelled Maestro or merge-up PR must be carved into the
# P/0 blocker set FIRST (so it trips the dedicated BLOCKED check + 🔥 P/0 PR row)
# and excluded from the Maestro / merge-up / generic buckets — never silently
# downgraded to a 📦 Maestro / merge-up row. This drives the REAL engine carve-out
# (Get-CategorizedPullRequests) rather than a re-implementation, so a regression
# in the engine's own filter expressions is caught here.
$maestroLogin = [PSCustomObject]@{ login = 'dotnet-maestro[bot]' }
$humanLogin   = [PSCustomObject]@{ login = 'someDev' }
$p0Lbl        = @([PSCustomObject]@{ name = 'p/0' })
$plainLbl     = @([PSCustomObject]@{ name = 'area-xaml' })

$prHumanP0   = [PSCustomObject]@{ number = 1; author = $humanLogin;   labels = $p0Lbl;    headRefName = 'fix/x';                  title = 'Fix X' }
$prMaestroP0 = [PSCustomObject]@{ number = 2; author = $maestroLogin; labels = $p0Lbl;    headRefName = 'darc-net11.0-abc';       title = 'Update dependencies' }
$prMergeP0   = [PSCustomObject]@{ number = 3; author = $humanLogin;   labels = $p0Lbl;    headRefName = 'merge/main-to-net11.0';  title = "[automated] Merge branch 'main' => 'net11.0'" }
$prMaestro   = [PSCustomObject]@{ number = 4; author = $maestroLogin; labels = $plainLbl; headRefName = 'darc-net11.0-def';       title = 'Update dependencies' }
$prHuman     = [PSCustomObject]@{ number = 5; author = $humanLogin;   labels = $plainLbl; headRefName = 'fix/y';                  title = 'Fix Y' }
$prMergeUp   = [PSCustomObject]@{ number = 6; author = $humanLogin;   labels = $plainLbl; headRefName = 'merge/main-to-net11.0';  title = "[automated] Merge branch 'main' => 'net11.0'" }
# Inflight (net<major>.0) PRs: a Maestro one (must still bucket as Maestro) and a
# p/0-labelled one (must NOT escalate — only survey-ref PRs block).
$prInflightMaestro = [PSCustomObject]@{ number = 7; author = $maestroLogin; labels = $plainLbl; headRefName = 'darc-main-xyz'; title = 'Update dependencies' }
$prInflightP0      = [PSCustomObject]@{ number = 8; author = $humanLogin;   labels = $p0Lbl;    headRefName = 'fix/z';          title = 'Fix Z' }

$targetSet   = @($prHumanP0, $prMaestroP0, $prMergeP0, $prMaestro, $prHuman, $prMergeUp)
$inflightSet = @($prInflightMaestro, $prInflightP0)

$buckets = Get-CategorizedPullRequests -TargetPRs $targetSet -InflightPRs $inflightSet
$bP0      = @($buckets.P0Prs            | ForEach-Object { $_.number })
$bMaestro = @($buckets.MaestroPRs       | ForEach-Object { $_.number })
$bMergeUp = @($buckets.MergeUpPRs       | ForEach-Object { $_.number })
$bHuman   = @($buckets.TargetHumanPRs   | ForEach-Object { $_.number })
$bInflight = @($buckets.InflightHumanPRs | ForEach-Object { $_.number })

Assert-Eq -Label "precedence: 3 p/0 PRs carved (human+maestro+merge-up)"   -Expected 3     -Actual $buckets.P0Prs.Count
Assert-Eq -Label "precedence: p/0 set includes the Maestro p/0 (#2)"       -Expected $true -Actual ($bP0 -contains 2)
Assert-Eq -Label "precedence: p/0 set includes the merge-up p/0 (#3)"      -Expected $true -Actual ($bP0 -contains 3)
Assert-Eq -Label "precedence: p/0 set EXCLUDES inflight p/0 (#8 never blocks)" -Expected $false -Actual ($bP0 -contains 8)
Assert-Eq -Label "precedence: Maestro bucket excludes the p/0 Maestro (#2)" -Expected $false -Actual ($bMaestro -contains 2)
Assert-Eq -Label "precedence: Maestro bucket = plain target + inflight Maestro" -Expected 2  -Actual $buckets.MaestroPRs.Count
Assert-Eq -Label "precedence: Maestro bucket keeps the plain target Maestro (#4)" -Expected $true -Actual ($bMaestro -contains 4)
Assert-Eq -Label "precedence: Maestro bucket keeps the inflight Maestro (#7)" -Expected $true -Actual ($bMaestro -contains 7)
Assert-Eq -Label "precedence: merge-up bucket excludes the p/0 merge-up (#3)" -Expected $false -Actual ($bMergeUp -contains 3)
Assert-Eq -Label "precedence: merge-up bucket = only the plain merge-up (#6)" -Expected 1   -Actual $buckets.MergeUpPRs.Count
Assert-Eq -Label "precedence: generic human = only the plain human (#5)"   -Expected 1     -Actual $buckets.TargetHumanPRs.Count
Assert-Eq -Label "precedence: generic human keeps #5"                      -Expected $true -Actual ($bHuman -contains 5)
Assert-Eq -Label "precedence: inflight-human = the inflight p/0 human (#8)" -Expected $true -Actual ($bInflight -contains 8)
Assert-Eq -Label "precedence: inflight-human excludes inflight Maestro (#7)" -Expected $false -Actual ($bInflight -contains 7)

# Empty-input safety: no PRs at all yields five empty buckets, no throw.
$emptyBuckets = Get-CategorizedPullRequests -TargetPRs @() -InflightPRs @()
Assert-Eq -Label "precedence: empty input → 0 p/0"      -Expected 0 -Actual $emptyBuckets.P0Prs.Count
Assert-Eq -Label "precedence: empty input → 0 Maestro"  -Expected 0 -Actual $emptyBuckets.MaestroPRs.Count
Assert-Eq -Label "precedence: empty input → 0 merge-up" -Expected 0 -Actual $emptyBuckets.MergeUpPRs.Count
Assert-Eq -Label "precedence: empty input → 0 human"    -Expected 0 -Actual $emptyBuckets.TargetHumanPRs.Count
Assert-Eq -Label "precedence: empty input → 0 inflight" -Expected 0 -Actual $emptyBuckets.InflightHumanPRs.Count

# AutomationNull-input safety (regression for the zero-PR-branch crash).
# The driver assigns $targetPRs/$inflightPRs from Get-OpenPullRequests, which
# returns AutomationNull (NOT a literal @()) for a branch with no open PRs — an
# empty `gh pr list` result collapses through `return @()`. AutomationNull bound
# to an [array] param becomes $null, and @($null) seeds a single null element
# whose `$_.author` dereference throws under StrictMode. Reproduce that EXACT
# value via ConvertFrom-JsonOrEmptyArray '[]' (the real collapse path), not a
# literal @() — the literal does not reproduce the bug.
$nullFromGh    = ConvertFrom-JsonOrEmptyArray '[]'   # AutomationNull, exactly like Get-OpenPullRequests on a 0-PR branch
$maestroPrMock = [PSCustomObject]@{ number = 9001; title = 'Bump deps'; author = [PSCustomObject]@{ login = 'dotnet-maestro' }; headRefName = 'darc-x'; labels = @(); url = 'u'; isDraft = $false }

# (a) The reachable in-flight shape: AutomationNull target (existing branch, 0 PRs)
#     + non-empty inflight Maestro list. Must not throw; Maestro PR still counted.
$nullTargetThrew = $false
$nullTargetBuckets = $null
try { $nullTargetBuckets = Get-CategorizedPullRequests -TargetPRs $nullFromGh -InflightPRs @($maestroPrMock) }
catch { $nullTargetThrew = $true }
Assert-Eq -Label "AutomationNull target + inflight Maestro → no throw" -Expected $false -Actual $nullTargetThrew
Assert-Eq -Label "AutomationNull target → 0 target-human"             -Expected 0     -Actual $nullTargetBuckets.TargetHumanPRs.Count
Assert-Eq -Label "AutomationNull target → inflight Maestro counted"   -Expected 1     -Actual $nullTargetBuckets.MaestroPRs.Count

# (b) Both inputs AutomationNull → five empty buckets, no throw.
$bothNullThrew = $false
$bothNullBuckets = $null
try { $bothNullBuckets = Get-CategorizedPullRequests -TargetPRs (ConvertFrom-JsonOrEmptyArray '[]') -InflightPRs (ConvertFrom-JsonOrEmptyArray '[]') }
catch { $bothNullThrew = $true }
Assert-Eq -Label "AutomationNull both → no throw"      -Expected $false -Actual $bothNullThrew
Assert-Eq -Label "AutomationNull both → 0 p/0"         -Expected 0      -Actual $bothNullBuckets.P0Prs.Count
Assert-Eq -Label "AutomationNull both → 0 Maestro"     -Expected 0      -Actual $bothNullBuckets.MaestroPRs.Count
Assert-Eq -Label "AutomationNull both → 0 inflight"    -Expected 0      -Actual $bothNullBuckets.InflightHumanPRs.Count

# (c) Explicit $null and an array carrying a $null element are both normalized.
$explicitNullThrew = $false
try { $null = Get-CategorizedPullRequests -TargetPRs $null -InflightPRs @($null, $maestroPrMock) }
catch { $explicitNullThrew = $true }
Assert-Eq -Label "explicit null target + @(null, maestro) inflight → no throw" -Expected $false -Actual $explicitNullThrew

Write-Host "`n────────────────────────────────────────" -ForegroundColor Cyan
Write-Host "Passed: $script:passed   Failed: $script:failed" -ForegroundColor $(if ($script:failed -eq 0) { 'Green' } else { 'Red' })
exit $(if ($script:failed -eq 0) { 0 } else { 1 })
