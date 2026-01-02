#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Verifies that UI tests fail when the PR's fix is reverted and pass with the fix.

.DESCRIPTION
    This script verifies that tests actually catch the issue by:
    1. Auto-detecting the PR base branch from GitHub
    2. Auto-detecting fix files from git diff (excludes test paths)
    3. Reverting the fix files to the base branch
    4. Running tests WITHOUT the fix (should FAIL)
    5. Restoring the fix files
    6. Running tests WITH the fix (should PASS)
    7. Reporting whether tests correctly detect the issue

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
    Branch to revert files from. Auto-detected from PR if not specified.

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
    [string]$BaseBranch,

    [Parameter(Mandatory = $false)]
    [string]$OutputDir = "CustomAgentLogsTmp/TestValidation"
)

$ErrorActionPreference = "Stop"
$RepoRoot = git rev-parse --show-toplevel

# Auto-detect base branch if not provided
if (-not $BaseBranch) {
    Write-Host "🔍 Auto-detecting base branch from PR..." -ForegroundColor Cyan
    
    # Get the remote repo for the current branch
    $currentBranch = git rev-parse --abbrev-ref HEAD 2>$null
    $remote = git config "branch.$currentBranch.remote" 2>$null
    if (-not $remote) { $remote = "origin" }
    
    # Get the repo owner/name from the remote URL
    $remoteUrl = git remote get-url $remote 2>$null
    $repo = $null
    if ($remoteUrl -match "github\.com[:/]([^/]+/[^/]+?)(\.git)?$") {
        $repo = $matches[1]
    }
    
    # Get base branch from GitHub PR using gh CLI
    # When using --repo, we must also provide the branch name as an argument
    if ($repo) {
        $BaseBranch = gh pr view $currentBranch --repo $repo --json baseRefName --jq '.baseRefName' 2>$null
    } else {
        $BaseBranch = gh pr view --json baseRefName --jq '.baseRefName' 2>$null
    }
    
    if ($BaseBranch) {
        Write-Host "✅ Auto-detected base branch: $BaseBranch" -ForegroundColor Green
    } else {
        Write-Host "❌ Could not detect base branch." -ForegroundColor Red
        Write-Host "   Make sure:" -ForegroundColor Yellow
        Write-Host "   - You're on a branch with an open PR" -ForegroundColor Yellow
        Write-Host "   - gh CLI is installed and authenticated" -ForegroundColor Yellow
        Write-Host "   Or specify -BaseBranch manually." -ForegroundColor Yellow
        exit 1
    }
}

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
    
    # Get committed files changed between current branch HEAD and base branch (the PR diff)
    # Using HEAD ensures we only get committed changes, not uncommitted working tree changes
    $changedFiles = git diff $BaseBranch HEAD --name-only 2>$null
    if ($LASTEXITCODE -ne 0) {
        $changedFiles = git diff "origin/$BaseBranch" HEAD --name-only 2>$null
    }
    
    if (-not $changedFiles) {
        Write-Host "❌ No committed changes detected between HEAD and $BaseBranch." -ForegroundColor Red
        Write-Host "   Make sure your changes are committed." -ForegroundColor Yellow
        exit 1
    }
    
    $FixFiles = @()
    foreach ($file in $changedFiles) {
        if (-not (Test-IsTestFile $file)) {
            $FixFiles += $file
        }
    }
    
    if ($FixFiles.Count -eq 0) {
        Write-Host "❌ No fix files detected. All changed files appear to be test files." -ForegroundColor Red
        Write-Host "   Changed files:" -ForegroundColor Yellow
        foreach ($file in $changedFiles) {
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
    
    # Get committed files changed between current branch HEAD and base branch (the PR diff)
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
