#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Verifies that UI tests catch the bug by running tests without and with the fix.

.DESCRIPTION
    This script verifies that tests actually catch the issue by:
    1. Reverting fix files to base branch
    2. Running tests WITHOUT fix (should FAIL)
    3. Restoring fix files
    4. Running tests WITH fix (should PASS)
    
    Fix files are auto-detected from the git diff (non-test files that changed).

.PARAMETER Platform
    Target platform: "android" or "ios"

.PARAMETER TestFilter
    Test filter to pass to dotnet test (e.g., "FullyQualifiedName~Issue12345").
    If not provided, auto-detects from test files in the git diff.

.PARAMETER FixFiles
    (Optional) Array of file paths to revert. If not provided, auto-detects from git diff
    by excluding test directories.

.PARAMETER BaseBranch
    Branch to revert files from. Auto-detected from PR if not specified.

.PARAMETER OutputDir
    Directory to store results (default: "CustomAgentLogsTmp/TestValidation")

.PARAMETER RequireFullVerification
    If set, the script will fail if it cannot run full verification mode
    (i.e., if no fix files are detected). Used by the skill to ensure proper validation.

.EXAMPLE
    # Auto-detect everything - simplest usage
    ./verify-tests-fail.ps1 -Platform android

.EXAMPLE
    # Specify test filter, auto-detect mode and fix files
    ./verify-tests-fail.ps1 -Platform android -TestFilter "Issue32030"

.EXAMPLE
    # Require full verification (fail if no fix files detected)
    ./verify-tests-fail.ps1 -Platform android -RequireFullVerification

.EXAMPLE
    # Specify everything explicitly
    ./verify-tests-fail.ps1 -Platform ios -TestFilter "Issue12345" `
        -FixFiles @("src/Controls/src/Core/SomeFile.cs")
#>

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("android", "ios")]
    [string]$Platform,

    [Parameter(Mandatory = $false)]
    [string]$TestFilter,

    [Parameter(Mandatory = $false)]
    [string[]]$FixFiles,

    [Parameter(Mandatory = $false)]
    [string]$BaseBranch,

    [Parameter(Mandatory = $false)]
    [string]$OutputDir = "CustomAgentLogsTmp/TestValidation",

    [Parameter(Mandatory = $false)]
    [switch]$RequireFullVerification
)

$ErrorActionPreference = "Stop"
$RepoRoot = git rev-parse --show-toplevel

# Test path patterns to exclude when auto-detecting fix files
$TestPathPatterns = @(
    "*/tests/*",
    "*/test/*",
    "*.Tests/*",
    "*.UnitTests/*",
    "*TestCases*",
    "*snapshots*",
    "*.png",
    "*.jpg",
    ".github/*",
    "*.md",
    "pr-*-review.md"
)

# Function to check if a file should be excluded from fix files
function Test-IsTestFile {
    param([string]$FilePath)
    
    foreach ($pattern in $TestPathPatterns) {
        if ($FilePath -like $pattern) {
            return $true
        }
    }
    return $false
}

# ============================================================
# AUTO-DETECT MODE: Check if there are fix files to revert
# ============================================================

# Try to detect base branch
$BaseBranchDetected = $BaseBranch
if (-not $BaseBranchDetected) {
    $currentBranch = git rev-parse --abbrev-ref HEAD 2>$null
    
    # Try gh pr view without --repo first (works for PRs from forks to upstream)
    $BaseBranchDetected = gh pr view --json baseRefName --jq '.baseRefName' 2>$null
    
    # If that fails, try with the origin remote's repo
    if (-not $BaseBranchDetected) {
        $remoteUrl = git remote get-url origin 2>$null
        $repo = $null
        if ($remoteUrl -match "github\.com[:/]([^/]+/[^/]+?)(\.git)?$") {
            $repo = $matches[1]
        }
        
        if ($repo) {
            $BaseBranchDetected = gh pr view $currentBranch --repo $repo --json baseRefName --jq '.baseRefName' 2>$null
        }
    }
}

# Fetch the base branch from origin to ensure we have the latest
# This ensures accurate diff detection even if local branch is stale
if ($BaseBranchDetected) {
    Write-Host "ℹ️  Fetching latest $BaseBranchDetected from origin..." -ForegroundColor Cyan
    git fetch origin "${BaseBranchDetected}:${BaseBranchDetected}" 2>$null
    if ($LASTEXITCODE -ne 0) {
        # If direct fetch fails (e.g., currently on that branch), just fetch the ref
        git fetch origin $BaseBranchDetected 2>$null
    }
}

# Check for fix files (non-test files that changed)
$DetectedFixFiles = @()
if ($BaseBranchDetected) {
    $changedFiles = git diff $BaseBranchDetected HEAD --name-only 2>$null
    
    if ($changedFiles) {
        foreach ($file in $changedFiles) {
            if (-not (Test-IsTestFile $file)) {
                $DetectedFixFiles += $file
            }
        }
    }
}

# Also check explicitly provided fix files
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
    Write-Host "║  - Base branch detection failed                           ║" -ForegroundColor Red
    Write-Host "║  - No non-test files changed in this PR                   ║" -ForegroundColor Red
    Write-Host "║  - Git fetch failed to update local branch                ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "Debug info:" -ForegroundColor Yellow
    Write-Host "  Base branch detected: '$BaseBranchDetected'" -ForegroundColor Yellow
    Write-Host "  Current branch: $(git rev-parse --abbrev-ref HEAD)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To fix, try one of:" -ForegroundColor Cyan
    Write-Host "  1. Specify fix files explicitly: -FixFiles @('path/to/fix.cs')" -ForegroundColor White
    Write-Host "  2. Specify base branch: -BaseBranch 'main'" -ForegroundColor White
    Write-Host "  3. Ensure you're on a PR branch with non-test file changes" -ForegroundColor White
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
    Write-Host "║  Use this mode when creating tests before writing a fix. ║" -ForegroundColor Cyan
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
    
    # Auto-detect test filter if not provided
    if (-not $TestFilter) {
        Write-Host "🔍 Auto-detecting test filter from changed test files..." -ForegroundColor Cyan
        
        $changedFiles = @()
        if ($BaseBranchDetected) {
            $changedFiles = git diff $BaseBranchDetected HEAD --name-only 2>$null
            if ($LASTEXITCODE -ne 0) {
                $changedFiles = git diff "origin/$BaseBranchDetected" HEAD --name-only 2>$null
            }
        }
        
        # If no base branch, use git status to find unstaged/staged files
        if (-not $changedFiles -or $changedFiles.Count -eq 0) {
            $changedFiles = git diff --name-only 2>$null
            if ($changedFiles -and $changedFiles.Count -gt 0) {
                $changedFiles = @($changedFiles)
            } else {
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
            Write-Host "❌ Could not auto-detect test filter. No test files found in changed files." -ForegroundColor Red
            Write-Host "   Looking for files matching: TestCases.(Shared.Tests|HostApp)/*.cs" -ForegroundColor Yellow
            Write-Host "   Please provide -TestFilter parameter explicitly." -ForegroundColor Yellow
            exit 1
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
            Write-Host "❌ Could not extract test class names from changed files." -ForegroundColor Red
            Write-Host "   Please provide -TestFilter parameter explicitly." -ForegroundColor Yellow
            exit 1
        }
        
        if ($testClassNames.Count -eq 1) {
            $TestFilter = $testClassNames[0]
        } else {
            $TestFilter = $testClassNames -join "|"
        }
        
        Write-Host "✅ Auto-detected $($testClassNames.Count) test class(es):" -ForegroundColor Green
        foreach ($name in $testClassNames) {
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
    "" | Add-Content $ValidationLog
    
    Write-Host "🧪 Running tests (expecting them to FAIL)..." -ForegroundColor Cyan
    Write-Host ""
    
    # Use shared BuildAndRunHostApp.ps1 infrastructure with -Rebuild to ensure clean builds
    $buildScript = Join-Path $RepoRoot ".github/scripts/BuildAndRunHostApp.ps1"
    & $buildScript -Platform $Platform -TestFilter $TestFilter -Rebuild 2>&1 | Tee-Object -FilePath $TestLog
    
    # Parse test results
    $testOutputLog = Join-Path $RepoRoot "CustomAgentLogsTmp/UITests/test-output.log"
    $testResult = @{ Passed = $false; Error = $null }
    
    if (Test-Path $testOutputLog) {
        $content = Get-Content $testOutputLog -Raw
        if ($content -match "Failed:\s*(\d+)") {
            $testResult.Passed = $false
            $testResult.FailCount = [int]$matches[1]
        } elseif ($content -match "Passed:\s*(\d+)") {
            $testResult.Passed = $true
            $testResult.PassCount = [int]$matches[1]
        } else {
            $testResult.Error = "Could not parse test results"
        }
    } else {
        $testResult.Error = "Test output log not found: $testOutputLog"
    }
    
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
        Write-Host "║  This proves the tests correctly reproduce the bug.      ║" -ForegroundColor Green
        Write-Host "║  You can now proceed to write the fix.                   ║" -ForegroundColor Green
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
        Write-Host "║  This means your tests don't actually reproduce the bug. ║" -ForegroundColor Red
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

$BaseBranch = $BaseBranchDetected
$FixFiles = $DetectedFixFiles

Write-Host "✅ Base branch: $BaseBranch" -ForegroundColor Green
Write-Host "✅ Fix files ($($FixFiles.Count)):" -ForegroundColor Green
foreach ($file in $FixFiles) {
    Write-Host "   - $file" -ForegroundColor White
}

# Auto-detect test filter from test files if not provided
if (-not $TestFilter) {
    Write-Host "🔍 Auto-detecting test filter from changed test files..." -ForegroundColor Cyan
    
    $changedFiles = git diff $BaseBranch HEAD --name-only 2>$null
    if ($LASTEXITCODE -ne 0) {
        $changedFiles = git diff "origin/$BaseBranch" HEAD --name-only 2>$null
    }
    
    # Find test files (files in test directories that are .cs files)
    $testFiles = @()
    foreach ($file in $changedFiles) {
        if ($file -match "TestCases\.(Shared\.Tests|HostApp).*\.cs$" -and $file -notmatch "^_") {
            $testFiles += $file
        }
    }
    
    if ($testFiles.Count -eq 0) {
        Write-Host "❌ Could not auto-detect test filter. No test files found in changed files." -ForegroundColor Red
        Write-Host "   Looking for files matching: TestCases.(Shared.Tests|HostApp)/*.cs" -ForegroundColor Yellow
        Write-Host "   Please provide -TestFilter parameter explicitly." -ForegroundColor Yellow
        exit 1
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
        Write-Host "❌ Could not extract test class names from changed files." -ForegroundColor Red
        Write-Host "   Please provide -TestFilter parameter explicitly." -ForegroundColor Yellow
        exit 1
    }
    
    if ($testClassNames.Count -eq 1) {
        $TestFilter = $testClassNames[0]
    } else {
        $TestFilter = $testClassNames -join "|"
    }
    
    Write-Host "✅ Auto-detected $($testClassNames.Count) test class(es):" -ForegroundColor Green
    foreach ($name in $testClassNames) {
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

function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logLine = "[$timestamp] $Message"
    Write-Host $logLine
    Add-Content -Path $ValidationLog -Value $logLine
}

function Get-TestResult {
    param([string]$LogFile)

    if (Test-Path $LogFile) {
        $content = Get-Content $LogFile -Raw
        if ($content -match "Failed:\s*(\d+)") {
            return @{ Passed = $false; FailCount = [int]$matches[1] }
        }
        if ($content -match "Passed:\s*(\d+)") {
            return @{ Passed = $true; PassCount = [int]$matches[1] }
        }
    }
    return @{ Passed = $false; Error = "Could not parse test results" }
}

# Initialize log
"" | Set-Content $ValidationLog
Write-Log "=========================================="
Write-Log "Verify Tests Fail Without Fix"
Write-Log "=========================================="
Write-Log "Platform: $Platform"
Write-Log "TestFilter: $TestFilter"
Write-Log "FixFiles: $($FixFiles -join ', ')"
Write-Log "BaseBranch: $BaseBranch"
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

# Determine which files exist in the base branch (can be reverted)
Write-Log ""
Write-Log "Checking which fix files exist in $BaseBranch..."
$RevertableFiles = @()
$NewFiles = @()

foreach ($file in $FixFiles) {
    # Check if file exists in base branch
    $existsInBase = git ls-tree -r $BaseBranch --name-only -- $file 2>$null
    if (-not $existsInBase) {
        $existsInBase = git ls-tree -r "origin/$BaseBranch" --name-only -- $file 2>$null
    }
    
    if ($existsInBase) {
        $RevertableFiles += $file
        Write-Log "  ✓ $file (exists in $BaseBranch - will revert)"
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

# Step 1: Revert fix files to base branch
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 1: Reverting fix files to $BaseBranch"
Write-Log "=========================================="

foreach ($file in $RevertableFiles) {
    Write-Log "  Reverting: $file"
    git checkout $BaseBranch -- $file 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Log "  Warning: Could not revert from $BaseBranch, trying origin/$BaseBranch"
        git checkout "origin/$BaseBranch" -- $file 2>&1 | Out-Null
    }
}

Write-Log "  ✓ $($RevertableFiles.Count) fix file(s) reverted to $BaseBranch state"

# Step 2: Run tests WITHOUT fix
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 2: Running tests WITHOUT fix (should FAIL)"
Write-Log "=========================================="

# Use shared BuildAndRunHostApp.ps1 infrastructure with -Rebuild to ensure clean builds
$buildScript = Join-Path $RepoRoot ".github/scripts/BuildAndRunHostApp.ps1"
& $buildScript -Platform $Platform -TestFilter $TestFilter -Rebuild 2>&1 | Tee-Object -FilePath $WithoutFixLog

$withoutFixResult = Get-TestResult -LogFile (Join-Path $RepoRoot "CustomAgentLogsTmp/UITests/test-output.log")

# Step 3: Restore fix files from current branch HEAD
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 3: Restoring fix files from HEAD"
Write-Log "=========================================="

foreach ($file in $RevertableFiles) {
    Write-Log "  Restoring: $file"
    git checkout HEAD -- $file 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Log "  ERROR: Failed to restore $file from HEAD"
        exit 1
    }
}

Write-Log "  ✓ $($RevertableFiles.Count) fix file(s) restored from HEAD"

# Step 4: Run tests WITH fix
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 4: Running tests WITH fix (should PASS)"
Write-Log "=========================================="

& $buildScript -Platform $Platform -TestFilter $TestFilter -Rebuild 2>&1 | Tee-Object -FilePath $WithFixLog

$withFixResult = Get-TestResult -LogFile (Join-Path $RepoRoot "CustomAgentLogsTmp/UITests/test-output.log")

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

if ($verificationPassed) {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║              VERIFICATION PASSED ✅                       ║" -ForegroundColor Green
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Green
    Write-Host "║  Tests correctly detect the issue:                        ║" -ForegroundColor Green
    Write-Host "║  - FAIL without fix (as expected)                         ║" -ForegroundColor Green
    Write-Host "║  - PASS with fix (as expected)                            ║" -ForegroundColor Green
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
    Write-Host "║  4. Build caching - try clean rebuild                     ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    exit 1
}
