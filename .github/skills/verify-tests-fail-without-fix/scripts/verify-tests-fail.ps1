#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Verifies that UI tests fail when the PR's fix is reverted and pass with the fix.

.DESCRIPTION
    This script verifies that tests actually catch the issue by:
    1. Auto-detecting fix files from git diff (excludes test paths)
    2. Reverting the fix files to main branch
    3. Running tests WITHOUT the fix (should FAIL)
    4. Restoring the fix files
    5. Running tests WITH the fix (should PASS)
    6. Reporting whether tests correctly detect the issue

    Fix files are auto-detected by finding all changed files that are NOT in test directories.

.PARAMETER Platform
    Target platform: "android" or "ios"

.PARAMETER TestFilter
    Test filter to pass to dotnet test (e.g., "FullyQualifiedName~Issue12345").
    If not provided, auto-detects from test files in the PR (looks for IssueXXXXX pattern).

.PARAMETER FixFiles
    (Optional) Array of file paths to revert. If not provided, auto-detects from git diff
    by excluding test directories.

.PARAMETER BaseBranch
    Branch to revert files from (default: "main")

.PARAMETER OutputDir
    Directory to store results (default: "CustomAgentLogsTmp/TestValidation")

.EXAMPLE
    # Auto-detect everything - simplest usage
    ./verify-tests-fail.ps1 -Platform android

.EXAMPLE
    # Specify test filter, auto-detect fix files
    ./verify-tests-fail.ps1 -Platform android -TestFilter "Issue32030"

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
    [string]$BaseBranch = "main",

    [Parameter(Mandatory = $false)]
    [string]$OutputDir = "CustomAgentLogsTmp/TestValidation"
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

# Auto-detect fix files from git diff if not provided
if (-not $FixFiles -or $FixFiles.Count -eq 0) {
    Write-Host "🔍 Auto-detecting fix files from git diff..." -ForegroundColor Cyan
    
    # Get tracked changes (modified files)
    $trackedChanges = git diff $BaseBranch --name-only 2>$null
    if ($LASTEXITCODE -ne 0) {
        $trackedChanges = git diff "origin/$BaseBranch" --name-only 2>$null
    }
    
    # Get untracked files (new files not yet committed)
    $untrackedFiles = git ls-files --others --exclude-standard 2>$null
    
    # Combine both
    $allChangedFiles = @()
    if ($trackedChanges) { $allChangedFiles += $trackedChanges }
    if ($untrackedFiles) { $allChangedFiles += $untrackedFiles }
    
    $FixFiles = @()
    foreach ($file in $allChangedFiles) {
        if (-not (Test-IsTestFile $file)) {
            $FixFiles += $file
        }
    }
    
    if ($FixFiles.Count -eq 0) {
        Write-Host "❌ No fix files detected. All changed files appear to be test files." -ForegroundColor Red
        Write-Host "   Changed files:" -ForegroundColor Yellow
        foreach ($file in $allChangedFiles) {
            Write-Host "     - $file" -ForegroundColor Yellow
        }
        exit 1
    }
    
    Write-Host "✅ Auto-detected $($FixFiles.Count) fix file(s):" -ForegroundColor Green
    foreach ($file in $FixFiles) {
        Write-Host "   - $file" -ForegroundColor White
    }
}

# Auto-detect test filter from test files if not provided
if (-not $TestFilter) {
    Write-Host "🔍 Auto-detecting test filter from changed test files..." -ForegroundColor Cyan
    
    # Get tracked changes
    $trackedChanges = git diff $BaseBranch --name-only 2>$null
    if ($LASTEXITCODE -ne 0) {
        $trackedChanges = git diff "origin/$BaseBranch" --name-only 2>$null
    }
    
    # Get untracked files
    $untrackedFiles = git ls-files --others --exclude-standard 2>$null
    
    # Combine both
    $allChangedFiles = @()
    if ($trackedChanges) { $allChangedFiles += $trackedChanges }
    if ($untrackedFiles) { $allChangedFiles += $untrackedFiles }
    
    # Find test files (files in test directories that are .cs files)
    $testFiles = @()
    foreach ($file in $allChangedFiles) {
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
    # For NUnit tests in TestCases.Shared.Tests, the class name is what we filter on
    $testClassNames = @()
    foreach ($file in $testFiles) {
        # Only process Shared.Tests files (the actual test classes, not HostApp UI pages)
        if ($file -match "TestCases\.Shared\.Tests.*\.cs$") {
            $fullPath = Join-Path $RepoRoot $file
            if (Test-Path $fullPath) {
                # Extract class name from file content
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
    
    # Fallback: use file names without extension if class extraction failed
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
    
    # Build test filter - combine with OR (|) for multiple classes
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
    Write-Log "  ✓ $file"
}

# Store current state of fix files
Write-Log ""
Write-Log "Storing current state of fix files..."
$backupDir = Join-Path $OutputPath "fix-backup"

# Clean backup directory to avoid stale files from previous runs
if (Test-Path $backupDir) {
    Remove-Item -Path $backupDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $backupDir | Out-Null

foreach ($file in $FixFiles) {
    $sourcePath = Join-Path $RepoRoot $file
    # Preserve directory structure to avoid name collisions
    $destPath = Join-Path $backupDir $file
    $destDir = Split-Path $destPath -Parent
    if (-not (Test-Path $destDir)) {
        New-Item -ItemType Directory -Force -Path $destDir | Out-Null
    }
    Copy-Item $sourcePath $destPath -Force
    Write-Log "  Backed up: $file"
}

# Step 1: Revert fix files to base branch
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 1: Reverting fix files to $BaseBranch"
Write-Log "=========================================="

foreach ($file in $FixFiles) {
    Write-Log "  Reverting: $file"
    git checkout $BaseBranch -- $file 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Log "  Warning: Could not revert from $BaseBranch, trying origin/$BaseBranch"
        git checkout "origin/$BaseBranch" -- $file 2>&1 | Out-Null
    }
}

Write-Log "  Fix files reverted to $BaseBranch state"

# Step 2: Run tests WITHOUT fix
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 2: Running tests WITHOUT fix (should FAIL)"
Write-Log "=========================================="

# Use shared BuildAndRunHostApp.ps1 infrastructure with -Rebuild to ensure clean builds
$buildScript = Join-Path $RepoRoot ".github/scripts/BuildAndRunHostApp.ps1"
& $buildScript -Platform $Platform -TestFilter $TestFilter -Rebuild 2>&1 | Tee-Object -FilePath $WithoutFixLog

$withoutFixResult = Get-TestResult -LogFile (Join-Path $RepoRoot "CustomAgentLogsTmp/UITests/test-output.log")

# Step 3: Restore fix files
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 3: Restoring fix files"
Write-Log "=========================================="

foreach ($file in $FixFiles) {
    # Use full path structure matching backup
    $sourcePath = Join-Path $backupDir $file
    $destPath = Join-Path $RepoRoot $file
    Copy-Item $sourcePath $destPath -Force
    Write-Log "  Restored: $file"
}

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
