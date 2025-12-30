#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates that unit tests correctly catch a regression by testing with and without the fix.

.DESCRIPTION
    This script automates the unit test validation workflow:
    1. Runs tests WITH the fix (should pass)
    2. Reverts the fix files to main branch
    3. Runs tests WITHOUT the fix (should fail)
    4. Restores the fix files
    5. Reports whether the tests correctly catch the regression

    This is the unit test equivalent of ValidateTestsCatchRegression.ps1.
    Use this for tests that don't require a device/simulator.

.PARAMETER TestProject
    Path to the test project (.csproj) to run

.PARAMETER TestFilter
    Test filter to pass to dotnet test (e.g., "FullyQualifiedName~MyTest")

.PARAMETER FixFiles
    Array of file paths (relative to repo root) that contain the fix.
    These files will be reverted to test that the tests catch the regression.

.PARAMETER BaseBranch
    Branch to revert files from (default: "main")

.PARAMETER OutputDir
    Directory to store validation results (default: "CustomAgentLogsTmp/TestValidation")

.EXAMPLE
    ./ValidateUnitTestsCatchRegression.ps1 `
        -TestProject "src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj" `
        -TestFilter "MyTestClass" `
        -FixFiles @("src/Controls/src/Core/SomeFile.cs")

.EXAMPLE
    ./ValidateUnitTestsCatchRegression.ps1 `
        -TestProject "src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj" `
        -TestFilter "Maui12345" `
        -FixFiles @("src/Controls/src/Xaml/SomeFile.cs")
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$TestProject,

    [Parameter(Mandatory = $true)]
    [string]$TestFilter,

    [Parameter(Mandatory = $true)]
    [string[]]$FixFiles,

    [Parameter(Mandatory = $false)]
    [string]$BaseBranch = "main",

    [Parameter(Mandatory = $false)]
    [string]$OutputDir = "CustomAgentLogsTmp/TestValidation"
)

$ErrorActionPreference = "Stop"
$RepoRoot = git rev-parse --show-toplevel

# Create output directory
$OutputPath = Join-Path $RepoRoot $OutputDir
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

$ValidationLog = Join-Path $OutputPath "validation-log.txt"
$WithFixLog = Join-Path $OutputPath "unittest-with-fix.log"
$WithoutFixLog = Join-Path $OutputPath "unittest-without-fix.log"

function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logLine = "[$timestamp] $Message"
    Write-Host $logLine
    Add-Content -Path $ValidationLog -Value $logLine
}

function Run-Tests {
    param(
        [string]$Project,
        [string]$Filter,
        [string]$LogFile
    )
    
    $projectPath = Join-Path $RepoRoot $Project
    
    Write-Log "Running: dotnet test `"$projectPath`" --filter `"$Filter`" --no-restore"
    
    $result = & dotnet test $projectPath --filter $Filter --no-restore 2>&1
    $result | Out-File -FilePath $LogFile -Encoding UTF8
    
    $exitCode = $LASTEXITCODE
    
    # Parse results
    $content = $result -join "`n"
    $passed = $exitCode -eq 0
    
    if ($content -match "Passed:\s*(\d+)") {
        $passCount = [int]$matches[1]
    }
    if ($content -match "Failed:\s*(\d+)") {
        $failCount = [int]$matches[1]
    }
    
    return @{
        Passed = $passed
        ExitCode = $exitCode
        PassCount = $passCount
        FailCount = $failCount
        Output = $content
    }
}

# Initialize log
"" | Set-Content $ValidationLog
Write-Log "=========================================="
Write-Log "Unit Test Validation Started"
Write-Log "=========================================="
Write-Log "TestProject: $TestProject"
Write-Log "TestFilter: $TestFilter"
Write-Log "FixFiles: $($FixFiles -join ', ')"
Write-Log "BaseBranch: $BaseBranch"
Write-Log ""

# Verify test project exists
$projectPath = Join-Path $RepoRoot $TestProject
if (-not (Test-Path $projectPath)) {
    Write-Log "ERROR: Test project not found: $TestProject"
    exit 1
}
Write-Log "✓ Test project found: $TestProject"

# Verify fix files exist
Write-Log ""
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
New-Item -ItemType Directory -Force -Path $backupDir | Out-Null

foreach ($file in $FixFiles) {
    $sourcePath = Join-Path $RepoRoot $file
    $destPath = Join-Path $backupDir (Split-Path $file -Leaf)
    Copy-Item $sourcePath $destPath -Force
    Write-Log "  Backed up: $file"
}

# Step 1: Build the test project first
Write-Log ""
Write-Log "=========================================="
Write-Log "Building test project..."
Write-Log "=========================================="

$buildResult = & dotnet build $projectPath --no-restore 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Log "ERROR: Build failed"
    Write-Log ($buildResult -join "`n")
    exit 1
}
Write-Log "✓ Build succeeded"

# Step 2: Run tests WITH fix
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 1: Running tests WITH fix (should PASS)"
Write-Log "=========================================="

$withFixResult = Run-Tests -Project $TestProject -Filter $TestFilter -LogFile $WithFixLog

if ($withFixResult.Passed) {
    Write-Log "✅ Tests PASSED with fix (expected)"
    Write-Log "   Passed: $($withFixResult.PassCount)"
} else {
    Write-Log "❌ Tests FAILED with fix (unexpected!)"
    Write-Log "   Failed: $($withFixResult.FailCount)"
    Write-Log "   The fix doesn't make the tests pass. Check the fix implementation."
    
    # Restore fix files before exiting
    foreach ($file in $FixFiles) {
        $sourcePath = Join-Path $backupDir (Split-Path $file -Leaf)
        $destPath = Join-Path $RepoRoot $file
        Copy-Item $sourcePath $destPath -Force
    }
    
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║              VALIDATION FAILED                            ║" -ForegroundColor Red
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
    Write-Host "║  Tests don't pass WITH the fix.                           ║" -ForegroundColor Red
    Write-Host "║  The fix implementation needs to be corrected.            ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    exit 1
}

# Step 3: Revert fix files to base branch
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 2: Reverting fix files to $BaseBranch"
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

# Rebuild after reverting
Write-Log ""
Write-Log "Rebuilding after revert..."
$buildResult = & dotnet build $projectPath --no-restore 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Log "ERROR: Build failed after revert"
    
    # Restore fix files before exiting
    foreach ($file in $FixFiles) {
        $sourcePath = Join-Path $backupDir (Split-Path $file -Leaf)
        $destPath = Join-Path $RepoRoot $file
        Copy-Item $sourcePath $destPath -Force
    }
    exit 1
}
Write-Log "✓ Build succeeded"

# Step 4: Run tests WITHOUT fix
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 3: Running tests WITHOUT fix (should FAIL)"
Write-Log "=========================================="

$withoutFixResult = Run-Tests -Project $TestProject -Filter $TestFilter -LogFile $WithoutFixLog

# Step 5: Restore fix files
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 4: Restoring fix files"
Write-Log "=========================================="

foreach ($file in $FixFiles) {
    $sourcePath = Join-Path $backupDir (Split-Path $file -Leaf)
    $destPath = Join-Path $RepoRoot $file
    Copy-Item $sourcePath $destPath -Force
    Write-Log "  Restored: $file"
}

# Rebuild after restoring
Write-Log ""
Write-Log "Rebuilding after restore..."
& dotnet build $projectPath --no-restore 2>&1 | Out-Null

# Step 6: Evaluate results
Write-Log ""
Write-Log "=========================================="
Write-Log "VALIDATION RESULTS"
Write-Log "=========================================="

$validationPassed = $false

if (-not $withoutFixResult.Passed) {
    Write-Log "✅ Tests FAILED without fix (expected - regression detected)"
    Write-Log "   Failed: $($withoutFixResult.FailCount)"
    $validationPassed = $true
} else {
    Write-Log "❌ Tests PASSED without fix (unexpected!)"
    Write-Log "   Passed: $($withoutFixResult.PassCount)"
    Write-Log "   The tests don't catch the regression."
    Write-Log "   Either the tests are wrong or the fix files don't contain the actual fix."
}

Write-Log ""
Write-Log "Summary:"
Write-Log "  - Tests WITH fix:    $(if ($withFixResult.Passed) { 'PASS ✅' } else { 'FAIL ❌' })"
Write-Log "  - Tests WITHOUT fix: $(if ($withoutFixResult.Passed) { 'PASS ❌ (should fail!)' } else { 'FAIL ✅ (expected)' })"

if ($validationPassed) {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║              VALIDATION PASSED ✅                         ║" -ForegroundColor Green
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Green
    Write-Host "║  Tests correctly catch the regression:                    ║" -ForegroundColor Green
    Write-Host "║  - PASS with fix                                          ║" -ForegroundColor Green
    Write-Host "║  - FAIL without fix                                       ║" -ForegroundColor Green
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
    exit 0
} else {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║              VALIDATION FAILED ❌                         ║" -ForegroundColor Red
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
    Write-Host "║  Tests do NOT catch the regression:                       ║" -ForegroundColor Red
    Write-Host "║  - Tests pass even without the fix                        ║" -ForegroundColor Red
    Write-Host "║                                                           ║" -ForegroundColor Red
    Write-Host "║  Possible causes:                                         ║" -ForegroundColor Red
    Write-Host "║  1. Wrong fix files specified                             ║" -ForegroundColor Red
    Write-Host "║  2. Tests don't actually test the fixed behavior          ║" -ForegroundColor Red
    Write-Host "║  3. The issue was already fixed in base branch            ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    exit 1
}
