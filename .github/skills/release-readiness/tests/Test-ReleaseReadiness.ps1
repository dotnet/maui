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
    Write-Host "  Under the tag-existence rule we expect FOUR trackers:" -ForegroundColor DarkGray
    Write-Host "    - SR2 (patch=21, no tag 10.0.21)        - in-flight but inactive (workflow will skip — no recent commits)" -ForegroundColor DarkGray
    Write-Host "    - SR3 (patch=33, no tag 10.0.33)        - in-flight but inactive (workflow will skip — no recent commits)" -ForegroundColor DarkGray
    Write-Host "    - SR8 (patch=80, no tag 10.0.80)        - in-flight, active" -ForegroundColor DarkGray
    Write-Host "    - SR9 (candidate off main)              - active" -ForegroundColor DarkGray
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
            Assert-Eq -Label "tracker count is 4 (SR2+SR3+SR8+SR9 — SR7 shipped)" `
                      -Expected 4 -Actual $detected.trackers.Count
            # All trackers in single-major net10 mode must be SR-flavored. (Net10's
            # previews 1–7 all shipped + no in-flight preview branch -> no preview tracker.)
            foreach ($t in $detected.trackers) {
                Assert-Eq -Label "tracker '$($t.canonicalKey)' has branchType='sr'" `
                          -Expected 'sr' -Actual $t.branchType
            }

            $bySr = @{}
            foreach ($t in $detected.trackers) { $bySr[[int]$t.srNumber] = $t }

            # SR2 (in-flight, INACTIVE — workflow's activity gate prevents new issue)
            if ($bySr.ContainsKey(2)) {
                $sr2 = $bySr[2]
                Assert-Eq -Label "SR2 mode = in-flight (tag 10.0.21 absent)" `
                          -Expected 'in-flight' -Actual $sr2.mode
                Assert-Eq -Label "SR2 expectedTag = 10.0.21"     -Expected '10.0.21' -Actual $sr2.expectedTag
                Assert-Eq -Label "SR2 hasRecentActivity = false (workflow will skip new issue)" `
                          -Expected $false -Actual $sr2.hasRecentActivity
            } else {
                Write-Host "  ❌ SR2 tracker missing (tag rule should pick it up — patch=21, no tag)" -ForegroundColor Red; $script:failed++
            }

            # SR3 (in-flight, INACTIVE)
            if ($bySr.ContainsKey(3)) {
                $sr3 = $bySr[3]
                Assert-Eq -Label "SR3 mode = in-flight (tag 10.0.33 absent)" `
                          -Expected 'in-flight' -Actual $sr3.mode
                Assert-Eq -Label "SR3 expectedTag = 10.0.33"     -Expected '10.0.33' -Actual $sr3.expectedTag
                Assert-Eq -Label "SR3 hasRecentActivity = false" -Expected $false -Actual $sr3.hasRecentActivity
            } else {
                Write-Host "  ❌ SR3 tracker missing (tag rule should pick it up — patch=33, no tag)" -ForegroundColor Red; $script:failed++
            }

            # SR7 (shipped 2026-06-05 as 10.0.71 — Lane 1 should NOT emit a tracker)
            if ($bySr.ContainsKey(7)) {
                Write-Host "  ❌ SR7 tracker should NOT be present (tag 10.0.71 shipped 2026-06-05)" -ForegroundColor Red; $script:failed++
            } else {
                Assert-Eq -Label "SR7 tracker absent (shipped)" -Expected $true -Actual $true
            }

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
    #   - net10 -> 4 SR trackers (SR2, SR3, SR8, SR9), no preview tracker
    #     (SR7 shipped 2026-06-05; every net10 preview branch already shipped + net10.0 isn't in preview cycle)
    #   - net11 -> 0 SR trackers (pre-GA: no `11.0.0` tag), 1 preview tracker
    #     (preview6 candidate from net11.0)
    Write-Host "`n[E2E] Detection with -AllActiveMajors" -ForegroundColor Cyan
    Write-Host "  Expected:" -ForegroundColor DarkGray
    Write-Host "    - majors[].length = 2 (net10 + net11)" -ForegroundColor DarkGray
    Write-Host "    - net10 trackers: 4 SR (sr2/sr3/sr8/sr9), 0 preview (SR7 shipped 2026-06-05)" -ForegroundColor DarkGray
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
                Assert-Eq -Label "net10 tracker count is 4 (no preview lane, SR7 shipped)" -Expected 4 -Actual $net10.trackers.Count
                $srCount = @($net10.trackers | Where-Object branchType -eq 'sr').Count
                $previewCount = @($net10.trackers | Where-Object branchType -eq 'preview').Count
                Assert-Eq -Label "net10 has 4 SR trackers"      -Expected 4 -Actual $srCount
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

Write-Host "`n────────────────────────────────────────" -ForegroundColor Cyan
Write-Host "Passed: $script:passed   Failed: $script:failed" -ForegroundColor $(if ($script:failed -eq 0) { 'Green' } else { 'Red' })
exit $(if ($script:failed -eq 0) { 0 } else { 1 })
