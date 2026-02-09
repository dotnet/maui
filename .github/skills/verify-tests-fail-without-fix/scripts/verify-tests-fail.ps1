#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Verifies that UI tests catch the bug. Supports two modes: verify failure only or full verification.

.DESCRIPTION
    This script verifies that tests actually catch the issue. It supports two modes:
    
    VERIFY FAILURE ONLY MODE (no fix files detected):
    - Runs tests to verify they FAIL (proving they catch the bug)
    - Used when creating tests before writing a fix
    
    FULL VERIFICATION MODE (fix files detected):
    1. Reverting fix files to base branch
    2. Running tests WITHOUT fix (should FAIL)
    3. Restoring fix files
    4. Running tests WITH fix (should PASS)
    
    The script auto-detects which mode to use based on whether fix files are present.
    Fix files and test filters are auto-detected from the git diff (non-test files that changed).

.PARAMETER Platform
    Target platform: "android", "ios", "catalyst" (MacCatalyst), or "windows"

.PARAMETER TestFilter
    Test filter to pass to dotnet test (e.g., "FullyQualifiedName~Issue12345").
    If not provided, auto-detects from test files in the git diff.

.PARAMETER FixFiles
    (Optional) Array of file paths to revert. If not provided, auto-detects from git diff
    by excluding test directories. If no fix files are found, runs in verify failure only mode.

.PARAMETER BaseBranch
    Branch to revert files from. Auto-detected from PR if not specified.

.PARAMETER OutputDir
    Directory to store results (default: "CustomAgentLogsTmp/TestValidation")

.PARAMETER RequireFullVerification
    If set, the script will fail if it cannot run full verification mode
    (i.e., if no fix files are detected). Without this flag, the script will
    automatically run in verify failure only mode when no fix files are found.

.EXAMPLE
    # Verify failure only mode - tests should fail (test creation workflow)
    ./verify-tests-fail.ps1 -Platform android

.EXAMPLE
    # Full verification mode - require fix files to be present
    ./verify-tests-fail.ps1 -Platform android -RequireFullVerification

.EXAMPLE
    # Specify test filter explicitly (works in both modes)
    ./verify-tests-fail.ps1 -Platform android -TestFilter "Issue32030"

.EXAMPLE
    # Specify everything explicitly
    ./verify-tests-fail.ps1 -Platform ios -TestFilter "Issue12345" `
        -FixFiles @("src/Controls/src/Core/SomeFile.cs")
#>

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("android", "ios", "catalyst", "maccatalyst", "windows")]
    [string]$Platform,

    [Parameter(Mandatory = $false)]
    [string]$TestFilter,

    [Parameter(Mandatory = $false)]
    [string[]]$FixFiles,

    [Parameter(Mandatory = $false)]
    [string]$BaseBranch,

    [Parameter(Mandatory = $false)]
    [string]$PRNumber,

    [Parameter(Mandatory = $false)]
    [string]$DeviceUdid,

    [Parameter(Mandatory = $false)]
    [switch]$RequireFullVerification
)

$ErrorActionPreference = "Stop"
$RepoRoot = git rev-parse --show-toplevel

# Normalize platform name (accept both "catalyst" and "maccatalyst")
if ($Platform -eq "maccatalyst") {
    $Platform = "catalyst"
}

# ============================================================
# Detect PR number if not provided
# ============================================================
if (-not $PRNumber) {
    # Try to get PR number from branch name (e.g., pr-27847)
    $currentBranch = git branch --show-current 2>$null
    if ($currentBranch -match "^pr-(\d+)") {
        $PRNumber = $matches[1]
        Write-Host "✅ Auto-detected PR #$PRNumber from branch name" -ForegroundColor Green
    } else {
        $foundPR = $false
        # Try gh cli - first try 'gh pr view' for current branch
        try {
            $prInfo = gh pr view --json number 2>$null | ConvertFrom-Json
            if ($prInfo -and $prInfo.number) {
                $PRNumber = $prInfo.number
                $foundPR = $true
                Write-Host "✅ Auto-detected PR #$PRNumber from gh cli (pr view)" -ForegroundColor Green
            }
        } catch {
            # gh pr view failed, will try fallback
        }
        
        # Fallback: search for PRs with this branch as head (works across forks)
        if (-not $foundPR) {
            try {
                $prList = gh pr list --head $currentBranch --json number --limit 1 2>$null | ConvertFrom-Json
                if ($prList -and $prList.Count -gt 0 -and $prList[0].number) {
                    $PRNumber = $prList[0].number
                    $foundPR = $true
                    Write-Host "✅ Auto-detected PR #$PRNumber from gh cli (pr list --head)" -ForegroundColor Green
                }
            } catch {
                # gh pr list also failed
            }
        }
        
        if (-not $foundPR) {
            Write-Error "Could not auto-detect PR number. Please provide -PRNumber parameter."
            exit 1
        }
    }
}

# Set output directory based on PR number
$OutputDir = "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate/verify-tests-fail"
Write-Host "📁 Output directory: $OutputDir" -ForegroundColor Cyan

# ============================================================
# Import shared baseline script for merge-base and file detection
# ============================================================
$BaselineScript = Join-Path $RepoRoot ".github/scripts/EstablishBrokenBaseline.ps1"

# Import Test-IsTestFile and Find-MergeBase from shared script
. $BaselineScript


# ============================================================
# Auto-detect test filter from changed files
# ============================================================
function Get-AutoDetectedTestFilter {
    param([string]$MergeBase)

    $changedFiles = @()
    if ($MergeBase) {
        $changedFiles = git diff $MergeBase HEAD --name-only 2>$null
    }

    # If no merge-base, use git status to find unstaged/staged files
    if (-not $changedFiles -or $changedFiles.Count -eq 0) {
        $changedFiles = git diff --name-only 2>$null
        if (-not $changedFiles -or $changedFiles.Count -eq 0) {
            $changedFiles = git diff --cached --name-only 2>$null
        }
    }

    # Find test files (files in test directories that are .cs files)
    $testFiles = @()
    foreach ($file in $changedFiles) {
        if ($file -match "TestCases\.(Shared\.Tests|HostApp).*\.cs$" -and $file -notmatch "^_") {
            $testFiles += $file
        }
    }

    if ($testFiles.Count -eq 0) {
        return $null
    }

    # Extract class names from test files
    $testClassNames = @()
    foreach ($file in $testFiles) {
        if ($file -match "TestCases\.Shared\.Tests.*\.cs$") {
            $fullPath = Join-Path $RepoRoot $file
            if (Test-Path $fullPath) {
                $content = Get-Content $fullPath -Raw
                if ($content -match "public\s+(partial\s+)?class\s+(\w+)") {
                    $className = $matches[2]
                    if ($className -notmatch "^_" -and $testClassNames -notcontains $className) {
                        $testClassNames += $className
                    }
                }
            }
        }
    }

    # Fallback: use file names without extension
    if ($testClassNames.Count -eq 0) {
        foreach ($file in $testFiles) {
            $fileName = [System.IO.Path]::GetFileNameWithoutExtension($file)
            if ($fileName -notmatch "^_" -and $testClassNames -notcontains $fileName) {
                $testClassNames += $fileName
            }
        }
    }

    if ($testClassNames.Count -eq 0) {
        return $null
    }

    return @{
        Filter = if ($testClassNames.Count -eq 1) { $testClassNames[0] } else { $testClassNames -join "|" }
        ClassNames = $testClassNames
    }
}

# ============================================================
# Parse test results from log file
# ============================================================
function Get-TestResultFromLog {
    param([string]$LogFile)

    if (-not (Test-Path $LogFile)) {
        return @{ Passed = $false; Error = "Test output log not found: $LogFile" }
    }

    $content = Get-Content $LogFile -Raw

    # Check for failures first - but only if count > 0
    if ($content -match "Failed:\s*(\d+)") {
        $failCount = [int]$matches[1]
        if ($failCount -gt 0) {
            return @{ Passed = $false; FailCount = $failCount }
        }
    }

    # Check for passes
    if ($content -match "Passed:\s*(\d+)") {
        $passCount = [int]$matches[1]
        if ($passCount -gt 0) {
            return @{ Passed = $true; PassCount = $passCount }
        }
    }

    return @{ Passed = $false; Error = "Could not parse test results" }
}

# ============================================================
# AUTO-DETECT MODE: Find merge-base and fix files
# ============================================================

Write-Host ""
Write-Host "🔍 Detecting base branch and merge point..." -ForegroundColor Cyan

$baseInfo = Find-MergeBase -ExplicitBaseBranch $BaseBranch

if (-not $baseInfo) {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║         ERROR: COULD NOT FIND MERGE BASE                  ║" -ForegroundColor Red
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
    Write-Host "║  Could not determine where this branch diverged from.     ║" -ForegroundColor Red
    Write-Host "║                                                           ║" -ForegroundColor Red
    Write-Host "║  Tried:                                                   ║" -ForegroundColor Red
    Write-Host "║  - PR metadata (gh pr view)                               ║" -ForegroundColor Red
    Write-Host "║  - Common base branches (main, net*.0, release/*)         ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "To fix, specify -BaseBranch explicitly:" -ForegroundColor Cyan
    Write-Host "  ./verify-tests-fail.ps1 -Platform android -BaseBranch main" -ForegroundColor White
    exit 1
}

$MergeBase = $baseInfo.MergeBase
$BaseBranchName = $baseInfo.BaseBranch

if ($baseInfo.TargetRepo) {
    Write-Host "✅ PR target: $($baseInfo.TargetRepo) ($BaseBranchName branch)" -ForegroundColor Green
} else {
    Write-Host "✅ Base branch: $BaseBranchName (via $($baseInfo.Source))" -ForegroundColor Green
}
Write-Host "✅ Merge base commit: $($MergeBase.Substring(0, 8))" -ForegroundColor Green
if ($baseInfo.Distance) {
    Write-Host "   ($($baseInfo.Distance) commits ahead of $BaseBranchName)" -ForegroundColor Gray
}

# Check for fix files (non-test files that changed since merge-base)
$DetectedFixFiles = @()
$changedFiles = git diff $MergeBase HEAD --name-only 2>$null

if ($changedFiles) {
    foreach ($file in $changedFiles) {
        if (-not (Test-IsTestFile $file)) {
            $DetectedFixFiles += $file
        }
    }
}

# Override with explicitly provided fix files
if ($FixFiles -and $FixFiles.Count -gt 0) {
    $DetectedFixFiles = $FixFiles
}

# Error if no fix files detected and RequireFullVerification is set
if ($DetectedFixFiles.Count -eq 0 -and $RequireFullVerification) {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║         ERROR: NO FIX FILES DETECTED                      ║" -ForegroundColor Red
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
    Write-Host "║  Full verification mode required but no fix files found.  ║" -ForegroundColor Red
    Write-Host "║                                                           ║" -ForegroundColor Red
    Write-Host "║  Possible causes:                                         ║" -ForegroundColor Red
    Write-Host "║  - No non-test files changed since merge-base             ║" -ForegroundColor Red
    Write-Host "║  - All changes are in test directories                    ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "Debug info:" -ForegroundColor Yellow
    Write-Host "  Merge base: $MergeBase" -ForegroundColor Yellow
    Write-Host "  Base branch: $BaseBranchName" -ForegroundColor Yellow
    Write-Host "  Current branch: $(git rev-parse --abbrev-ref HEAD)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To fix, try one of:" -ForegroundColor Cyan
    Write-Host "  1. Specify fix files explicitly: -FixFiles @('path/to/fix.cs')" -ForegroundColor White
    Write-Host "  2. Remove -RequireFullVerification to run in failure-only mode" -ForegroundColor White
    exit 1
}

# If no fix files and not requiring full verification, run in "verify failure only" mode
if ($DetectedFixFiles.Count -eq 0) {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║         VERIFY FAILURE ONLY MODE                          ║" -ForegroundColor Cyan
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Cyan
    Write-Host "║  No fix files detected - will only verify:                ║" -ForegroundColor Cyan
    Write-Host "║  1. Tests FAIL (proving they catch the bug)               ║" -ForegroundColor Cyan
    Write-Host "║                                                           ║" -ForegroundColor Cyan
    Write-Host "║  Use this mode when creating tests before writing a fix.  ║" -ForegroundColor Cyan
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""

    # Auto-detect test filter if not provided
    if (-not $TestFilter) {
        Write-Host "🔍 Auto-detecting test filter from changed test files..." -ForegroundColor Cyan
        $filterResult = Get-AutoDetectedTestFilter -MergeBase $MergeBase

        if (-not $filterResult) {
            Write-Host "❌ Could not auto-detect test filter. No test files found in changed files." -ForegroundColor Red
            Write-Host "   Looking for files matching: TestCases.(Shared.Tests|HostApp)/*.cs" -ForegroundColor Yellow
            Write-Host "   Please provide -TestFilter parameter explicitly." -ForegroundColor Yellow
            exit 1
        }

        $TestFilter = $filterResult.Filter
        Write-Host "✅ Auto-detected $($filterResult.ClassNames.Count) test class(es):" -ForegroundColor Green
        foreach ($name in $filterResult.ClassNames) {
            Write-Host "   - $name" -ForegroundColor White
        }
        Write-Host "   Filter: $TestFilter" -ForegroundColor Cyan
    }

    # Create output directory
    $OutputPath = Join-Path $RepoRoot $OutputDir
    New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

    $ValidationLog = Join-Path $OutputPath "verification-log.txt"
    $TestLog = Join-Path $OutputPath "test-failure-verification.log"

    # Initialize log
    "" | Set-Content $ValidationLog
    "=========================================" | Add-Content $ValidationLog
    "Verify Tests Fail (Failure Only Mode)" | Add-Content $ValidationLog
    "=========================================" | Add-Content $ValidationLog
    "Platform: $Platform" | Add-Content $ValidationLog
    "TestFilter: $TestFilter" | Add-Content $ValidationLog
    "MergeBase: $MergeBase" | Add-Content $ValidationLog
    "" | Add-Content $ValidationLog

    Write-Host "🧪 Running tests (expecting them to FAIL)..." -ForegroundColor Cyan
    Write-Host ""

    # Use shared BuildAndRunHostApp.ps1 infrastructure with -Rebuild to ensure clean builds
    $buildScript = Join-Path $RepoRoot ".github/scripts/BuildAndRunHostApp.ps1"
    $buildParams = @{
        Platform = $Platform
        TestFilter = $TestFilter
        Rebuild = $true
    }
    if ($DeviceUdid) {
        $buildParams.DeviceUdid = $DeviceUdid
    }
    & $buildScript @buildParams 2>&1 | Tee-Object -FilePath $TestLog

    # Parse test results using shared function
    $testOutputLog = Join-Path $RepoRoot "CustomAgentLogsTmp/UITests/test-output.log"
    $testResult = Get-TestResultFromLog -LogFile $testOutputLog

    # Evaluate results
    Write-Host ""
    Write-Host "=========================================="
    Write-Host "VERIFICATION RESULTS"
    Write-Host "=========================================="
    Write-Host ""

    if ($testResult.Error) {
        Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
        Write-Host "║              ERROR PARSING TEST RESULTS                   ║" -ForegroundColor Red
        Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
        Write-Host "║  $($testResult.Error.PadRight(57)) ║" -ForegroundColor Red
        Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
        exit 1
    }

    if (-not $testResult.Passed) {
        # Tests FAILED - this is what we want!
        Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
        Write-Host "║              VERIFICATION PASSED ✅                       ║" -ForegroundColor Green
        Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Green
        Write-Host "║  Tests FAILED as expected!                                ║" -ForegroundColor Green
        Write-Host "║                                                           ║" -ForegroundColor Green
        Write-Host "║  This proves the tests correctly reproduce the bug.       ║" -ForegroundColor Green
        Write-Host "║  You can now proceed to write the fix.                    ║" -ForegroundColor Green
        Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
        Write-Host ""
        Write-Host "Failed tests: $($testResult.FailCount)" -ForegroundColor Yellow
        exit 0
    } else {
        # Tests PASSED - this is bad!
        Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
        Write-Host "║              VERIFICATION FAILED ❌                       ║" -ForegroundColor Red
        Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
        Write-Host "║  Tests PASSED but they should FAIL!                       ║" -ForegroundColor Red
        Write-Host "║                                                           ║" -ForegroundColor Red
        Write-Host "║  This means your tests don't actually reproduce the bug.  ║" -ForegroundColor Red
        Write-Host "║                                                           ║" -ForegroundColor Red
        Write-Host "║  Possible causes:                                         ║" -ForegroundColor Red
        Write-Host "║  1. Test scenario doesn't match the issue description     ║" -ForegroundColor Red
        Write-Host "║  2. Test assertions are wrong or too lenient              ║" -ForegroundColor Red
        Write-Host "║  3. The bug was already fixed in this branch              ║" -ForegroundColor Red
        Write-Host "║  4. The bug only happens in specific conditions           ║" -ForegroundColor Red
        Write-Host "║                                                           ║" -ForegroundColor Red
        Write-Host "║  Go back and revise your tests!                           ║" -ForegroundColor Red
        Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
        Write-Host ""
        Write-Host "Passed tests: $($testResult.PassCount)" -ForegroundColor Yellow
        exit 1
    }
}

# ============================================================
# FULL VERIFICATION MODE (fix files detected)
# ============================================================

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║         FULL VERIFICATION MODE                            ║" -ForegroundColor Cyan
Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Cyan
Write-Host "║  Fix files detected - will verify:                        ║" -ForegroundColor Cyan
Write-Host "║  1. Tests FAIL without fix                                ║" -ForegroundColor Cyan
Write-Host "║  2. Tests PASS with fix                                   ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$FixFiles = $DetectedFixFiles

Write-Host "✅ Fix files ($($FixFiles.Count)):" -ForegroundColor Green
foreach ($file in $FixFiles) {
    Write-Host "   - $file" -ForegroundColor White
}

# Auto-detect test filter from test files if not provided
if (-not $TestFilter) {
    Write-Host "🔍 Auto-detecting test filter from changed test files..." -ForegroundColor Cyan
    $filterResult = Get-AutoDetectedTestFilter -MergeBase $MergeBase

    if (-not $filterResult) {
        Write-Host "❌ Could not auto-detect test filter. No test files found in changed files." -ForegroundColor Red
        Write-Host "   Looking for files matching: TestCases.(Shared.Tests|HostApp)/*.cs" -ForegroundColor Yellow
        Write-Host "   Please provide -TestFilter parameter explicitly." -ForegroundColor Yellow
        exit 1
    }

    $TestFilter = $filterResult.Filter
    Write-Host "✅ Auto-detected $($filterResult.ClassNames.Count) test class(es):" -ForegroundColor Green
    foreach ($name in $filterResult.ClassNames) {
        Write-Host "   - $name" -ForegroundColor White
    }
    Write-Host "   Filter: $TestFilter" -ForegroundColor Cyan
}

# Create output directory
$OutputPath = Join-Path $RepoRoot $OutputDir
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

$ValidationLog = Join-Path $OutputPath "verification-log.txt"
$WithoutFixLog = Join-Path $OutputPath "test-without-fix.log"
$WithFixLog = Join-Path $OutputPath "test-with-fix.log"
$MarkdownReport = Join-Path $OutputPath "verification-report.md"

function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logLine = "[$timestamp] $Message"
    Write-Host $logLine
    Add-Content -Path $ValidationLog -Value $logLine
}

function Write-MarkdownReport {
    param(
        [bool]$VerificationPassed,
        [bool]$FailedWithoutFix,
        [bool]$PassedWithFix,
        [hashtable]$WithoutFixResult,
        [hashtable]$WithFixResult
    )
    
    $reportDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $status = if ($VerificationPassed) { "✅ PASSED" } else { "❌ FAILED" }
    $statusSymbol = if ($VerificationPassed) { "✅" } else { "❌" }
    
    $markdown = @"
### Test Verification Report

**Date:** $reportDate | **Platform:** $($Platform.ToUpper()) | **Status:** $status

#### Summary

| Check | Expected | Actual | Result |
|-------|----------|--------|--------|
| Tests WITHOUT fix | FAIL | $(if ($FailedWithoutFix) { "FAIL" } else { "PASS" }) | $(if ($FailedWithoutFix) { "✅" } else { "❌" }) |
| Tests WITH fix | PASS | $(if ($PassedWithFix) { "PASS" } else { "FAIL" }) | $(if ($PassedWithFix) { "✅" } else { "❌" }) |

#### $statusSymbol Final Verdict

$(if ($VerificationPassed) {
    @"
**VERIFICATION PASSED** ✅

The tests correctly detect the issue:
- ✅ Tests **FAIL** without the fix (as expected - bug is present)
- ✅ Tests **PASS** with the fix (as expected - bug is fixed)

**Conclusion:** The tests properly validate the fix and catch the bug when it's present.
"@
} else {
    @"
**VERIFICATION FAILED** ❌

$(if (-not $FailedWithoutFix) {
    "❌ **Tests PASSED without fix** (should have failed)`n   - The tests don't actually detect the bug`n   - Tests may not be testing the right behavior`n"
})$(if (-not $PassedWithFix) {
    "❌ **Tests FAILED with fix** (should have passed)`n   - The fix doesn't resolve the issue`n   - Tests may be broken or testing something else`n"
})
**Possible causes:**
1. Wrong fix files specified
2. Tests don't actually test the fixed behavior  
3. The issue was already fixed in base branch
4. Build caching - try clean rebuild
5. Test needs different setup or conditions
"@
})

---

#### Configuration

**Platform:** $Platform
**Test Filter:** $TestFilter
**Base Branch:** $BaseBranchName
**Merge Base:** $(if ($MergeBase -and $MergeBase.Length -ge 8) { $MergeBase.Substring(0, 8) } else { $MergeBase })

### Fix Files

$(($RevertableFiles | ForEach-Object { "- ``$_``" }) -join "`n")

$(if ($NewFiles.Count -gt 0) {
@"

### New Files (Not Reverted)

$(($NewFiles | ForEach-Object { "- ``$_``" }) -join "`n")
"@
})

---

#### Test Results Details

### Test Run 1: WITHOUT Fix

**Expected:** Tests should FAIL (bug is present)  
**Actual:** Tests $(if ($FailedWithoutFix) { "FAILED" } else { "PASSED" }) $(if ($FailedWithoutFix) { "✅" } else { "❌" })

**Test Summary:**
- Total: $($WithoutFixResult.Total)
- Passed: $($WithoutFixResult.Passed)
- Failed: $($WithoutFixResult.Failed)
- Skipped: $($WithoutFixResult.Skipped)

$(if ($WithoutFixResult.FailureReason) {
    "**Failure Reason:** ``$($WithoutFixResult.FailureReason)``"
})

<details>
<summary>View full test output (without fix)</summary>

``````
$(Get-Content $WithoutFixLog -Raw)
``````

</details>

---

### Test Run 2: WITH Fix

**Expected:** Tests should PASS (bug is fixed)  
**Actual:** Tests $(if ($PassedWithFix) { "PASSED" } else { "FAILED" }) $(if ($PassedWithFix) { "✅" } else { "❌" })

**Test Summary:**
- Total: $($WithFixResult.Total)
- Passed: $($WithFixResult.Passed)
- Failed: $($WithFixResult.Failed)
- Skipped: $($WithFixResult.Skipped)

$(if ($WithFixResult.FailureReason) {
    "**Failure Reason:** ``$($WithFixResult.FailureReason)``"
})

<details>
<summary>View full test output (with fix)</summary>

``````
$(Get-Content $WithFixLog -Raw)
``````

</details>

---

#### Logs

- Full verification log: ``$ValidationLog``
- Test output without fix: ``$WithoutFixLog``
- Test output with fix: ``$WithFixLog``
- UI test logs: ``CustomAgentLogsTmp/UITests/``
"@

    $markdown | Set-Content -Path $MarkdownReport -Encoding UTF8
    Write-Host ""
    Write-Host "📄 Markdown report saved to: $MarkdownReport" -ForegroundColor Cyan
}

# Reuse the Get-TestResultFromLog function defined earlier

# Initialize log
"" | Set-Content $ValidationLog
Write-Log "=========================================="
Write-Log "Verify Tests Fail Without Fix"
Write-Log "=========================================="
Write-Log "Platform: $Platform"
Write-Log "TestFilter: $TestFilter"
Write-Log "FixFiles: $($FixFiles -join ', ')"
Write-Log "BaseBranch: $BaseBranchName"
Write-Log "MergeBase: $MergeBase"
Write-Log ""

# Verify fix files exist
Write-Log "Verifying fix files exist..."
foreach ($file in $FixFiles) {
    $fullPath = Join-Path $RepoRoot $file
    if (-not (Test-Path $fullPath)) {
        Write-Log "ERROR: Fix file not found: $file"
        exit 1
    }
    Write-Log "  ✓ $file exists"
}

# Determine which files exist at the merge-base (can be reverted)
Write-Log ""
Write-Log "Checking which fix files exist at merge-base ($($MergeBase.Substring(0, 8)))..."
$RevertableFiles = @()
$NewFiles = @()

foreach ($file in $FixFiles) {
    # Check if file exists at merge-base commit
    $existsInBase = git ls-tree -r $MergeBase --name-only -- $file 2>$null

    if ($existsInBase) {
        $RevertableFiles += $file
        Write-Log "  ✓ $file (exists at merge-base - will revert)"
    } else {
        $NewFiles += $file
        Write-Log "  ○ $file (new file - skipping revert)"
    }
}

if ($RevertableFiles.Count -eq 0) {
    Write-Host "❌ No revertable fix files found. All fix files are new." -ForegroundColor Red
    Write-Host "   Cannot verify test behavior without files to revert." -ForegroundColor Yellow
    exit 1
}

# Check for uncommitted changes ONLY on files we will revert
Write-Log ""
Write-Log "Checking for uncommitted changes on revertable files..."
$uncommittedFiles = @()
foreach ($file in $RevertableFiles) {
    # Check if file has uncommitted changes (staged or unstaged)
    $status = git status --porcelain -- $file 2>$null
    if ($status) {
        $uncommittedFiles += $file
    }
}

if ($uncommittedFiles.Count -gt 0) {
    Write-Host "" -ForegroundColor Red
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║  ERROR: Uncommitted changes detected in fix files         ║" -ForegroundColor Red
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
    Write-Host "║  This script requires revertable fix files to be          ║" -ForegroundColor Red
    Write-Host "║  committed so they can be restored via git checkout HEAD. ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "Uncommitted files:" -ForegroundColor Yellow
    foreach ($file in $uncommittedFiles) {
        Write-Host "  - $file" -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host "Run 'git add <files> && git commit' to commit your changes." -ForegroundColor Cyan
    exit 1
}

Write-Log "  ✓ All revertable fix files are committed"

# Step 1: Revert fix files to merge-base state
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 1: Reverting fix files to merge-base ($($MergeBase.Substring(0, 8)))"
Write-Log "=========================================="

foreach ($file in $RevertableFiles) {
    Write-Log "  Reverting: $file"
    $gitOutput = git checkout $MergeBase -- $file 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Log "  ERROR: Failed to revert $file from $MergeBase"
        Write-Log "  Git output: $gitOutput"
        exit 1
    }
}

Write-Log "  ✓ $($RevertableFiles.Count) fix file(s) reverted to merge-base state"

# Step 2: Run tests WITHOUT fix
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 2: Running tests WITHOUT fix (should FAIL)"
Write-Log "=========================================="

# Use shared BuildAndRunHostApp.ps1 infrastructure with -Rebuild to ensure clean builds
$buildScript = Join-Path $RepoRoot ".github/scripts/BuildAndRunHostApp.ps1"
$buildParams = @{
    Platform = $Platform
    TestFilter = $TestFilter
    Rebuild = $true
}
if ($DeviceUdid) {
    $buildParams.DeviceUdid = $DeviceUdid
}
& $buildScript @buildParams 2>&1 | Tee-Object -FilePath $WithoutFixLog

$withoutFixResult = Get-TestResultFromLog -LogFile (Join-Path $RepoRoot "CustomAgentLogsTmp/UITests/test-output.log")

# Step 3: Restore fix files from current branch HEAD
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 3: Restoring fix files from HEAD"
Write-Log "=========================================="

foreach ($file in $RevertableFiles) {
    Write-Log "  Restoring: $file"
    $gitOutput = git checkout HEAD -- $file 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Log "  ERROR: Failed to restore $file from HEAD"
        Write-Log "  Git output: $gitOutput"
        exit 1
    }
}

Write-Log "  ✓ $($RevertableFiles.Count) fix file(s) restored from HEAD"

# Step 4: Run tests WITH fix
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 4: Running tests WITH fix (should PASS)"
Write-Log "=========================================="

$buildParams = @{
    Platform = $Platform
    TestFilter = $TestFilter
    Rebuild = $true
}
if ($DeviceUdid) {
    $buildParams.DeviceUdid = $DeviceUdid
}
& $buildScript @buildParams 2>&1 | Tee-Object -FilePath $WithFixLog

$withFixResult = Get-TestResultFromLog -LogFile (Join-Path $RepoRoot "CustomAgentLogsTmp/UITests/test-output.log")

# Step 5: Evaluate results
Write-Log ""
Write-Log "=========================================="
Write-Log "VERIFICATION RESULTS"
Write-Log "=========================================="

$verificationPassed = $false
$failedWithoutFix = -not $withoutFixResult.Passed
$passedWithFix = $withFixResult.Passed

if ($failedWithoutFix) {
    Write-Log "✅ Tests FAILED without fix (expected - issue detected)"
} else {
    Write-Log "❌ Tests PASSED without fix (unexpected!)"
    Write-Log "   The tests don't detect the issue."
}

if ($passedWithFix) {
    Write-Log "✅ Tests PASSED with fix (expected - fix works)"
} else {
    Write-Log "❌ Tests FAILED with fix (unexpected!)"
    Write-Log "   The fix doesn't resolve the issue, or there's another problem."
}

$verificationPassed = $failedWithoutFix -and $passedWithFix

Write-Log ""
Write-Log "Summary:"
Write-Log "  - Tests WITHOUT fix: $(if ($failedWithoutFix) { 'FAIL ✅ (expected)' } else { 'PASS ❌ (should fail!)' })"
Write-Log "  - Tests WITH fix: $(if ($passedWithFix) { 'PASS ✅ (expected)' } else { 'FAIL ❌ (should pass!)' })"

# Generate markdown report
Write-MarkdownReport `
    -VerificationPassed $verificationPassed `
    -FailedWithoutFix $failedWithoutFix `
    -PassedWithFix $passedWithFix `
    -WithoutFixResult $withoutFixResult `
    -WithFixResult $withFixResult

if ($verificationPassed) {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║              VERIFICATION PASSED ✅                       ║" -ForegroundColor Green
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Green
    Write-Host "║  Tests correctly detect the issue:                        ║" -ForegroundColor Green
    Write-Host "║  - FAIL without fix (as expected)                         ║" -ForegroundColor Green
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
    exit 0
} else {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║              VERIFICATION FAILED ❌                       ║" -ForegroundColor Red
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
    if (-not $failedWithoutFix) {
        Write-Host "║  Tests PASSED without fix (should fail)                   ║" -ForegroundColor Red
        Write-Host "║  - Tests don't actually detect the bug                    ║" -ForegroundColor Red
    }
    if (-not $passedWithFix) {
        Write-Host "║  Tests FAILED with fix (should pass)                      ║" -ForegroundColor Red
        Write-Host "║  - Fix doesn't resolve the issue or test is broken        ║" -ForegroundColor Red
    }
    Write-Host "║                                                           ║" -ForegroundColor Red
    Write-Host "║  Possible causes:                                         ║" -ForegroundColor Red
    Write-Host "║  1. Wrong fix files specified                             ║" -ForegroundColor Red
    Write-Host "║  2. Tests don't actually test the fixed behavior          ║" -ForegroundColor Red
    Write-Host "║  3. The issue was already fixed in base branch            ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    exit 1
}
