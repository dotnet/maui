#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Tests for EstablishBrokenBaseline.ps1

.DESCRIPTION
    Validates that the EstablishBrokenBaseline.ps1 script works correctly.
    Run these tests to verify the baseline logic after making changes.

.EXAMPLE
    ./Test-EstablishBrokenBaseline.ps1
#>

param(
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"
$RepoRoot = git rev-parse --show-toplevel
$ScriptPath = Join-Path $RepoRoot ".github/scripts/EstablishBrokenBaseline.ps1"
$StateFile = Join-Path $RepoRoot ".github/.baseline-state.json"

# Test tracking
$script:TestsPassed = 0
$script:TestsFailed = 0
$script:TestsSkipped = 0

function Write-TestResult {
    param(
        [string]$TestName,
        [bool]$Passed,
        [string]$Message = ""
    )

    if ($Passed) {
        Write-Host "  [PASS] $TestName" -ForegroundColor Green
        $script:TestsPassed++
    } else {
        Write-Host "  [FAIL] $TestName" -ForegroundColor Red
        if ($Message) {
            Write-Host "         $Message" -ForegroundColor Yellow
        }
        $script:TestsFailed++
    }
}

function Write-TestSkipped {
    param([string]$TestName, [string]$Reason)
    Write-Host "  [SKIP] $TestName - $Reason" -ForegroundColor Yellow
    $script:TestsSkipped++
}

function Test-Section {
    param([string]$Name)
    Write-Host ""
    Write-Host "=== $Name ===" -ForegroundColor Cyan
}

# ============================================================
# Cleanup function
# ============================================================
function Invoke-Cleanup {
    # Restore any changes and clean up state file
    if (Test-Path $StateFile) {
        Remove-Item $StateFile -Force -ErrorAction SilentlyContinue
    }
    git checkout -- . 2>$null
}

# ============================================================
# Test: Script exists
# ============================================================
Test-Section "Script Existence"

Write-TestResult "EstablishBrokenBaseline.ps1 exists" (Test-Path $ScriptPath)

# ============================================================
# Test: Dot-source import works
# ============================================================
Test-Section "Dot-Source Import"

try {
    . $ScriptPath
    Write-TestResult "Script can be dot-sourced without errors" $true

    # Verify functions are available
    $functionsAvailable = (Get-Command -Name Find-MergeBase -ErrorAction SilentlyContinue) -and
                          (Get-Command -Name Test-IsTestFile -ErrorAction SilentlyContinue) -and
                          (Get-Command -Name Get-FixFiles -ErrorAction SilentlyContinue) -and
                          (Get-Command -Name Get-FileCategories -ErrorAction SilentlyContinue)
    Write-TestResult "Required functions are exported" $functionsAvailable
} catch {
    Write-TestResult "Script can be dot-sourced without errors" $false $_.Exception.Message
}

# ============================================================
# Test: Test-IsTestFile function
# ============================================================
Test-Section "Test-IsTestFile Function"

$testCases = @(
    # Test files (should return $true)
    @{ Path = "src/Controls/tests/TestCases.HostApp/Issues/Issue20772.cs"; Expected = $true; Desc = "TestCases.HostApp file" },
    @{ Path = "src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue20772.cs"; Expected = $true; Desc = "TestCases.Shared.Tests file" },
    @{ Path = "src/Core/tests/UnitTests/SomeTest.cs"; Expected = $true; Desc = "tests directory file" },
    @{ Path = ".github/scripts/SomeScript.ps1"; Expected = $true; Desc = ".github directory file" },
    @{ Path = "README.md"; Expected = $true; Desc = "Markdown file" },
    @{ Path = "docs/screenshot.png"; Expected = $true; Desc = "PNG image" },
    @{ Path = "snapshots/baseline.png"; Expected = $true; Desc = "Snapshots directory" },

    # Non-test files (should return $false)
    @{ Path = "src/Controls/src/Core/Button.cs"; Expected = $false; Desc = "Core source file" },
    @{ Path = "src/Controls/src/Core/Platform/Android/InnerGestureListener.cs"; Expected = $false; Desc = "Platform source file" },
    @{ Path = "src/Core/src/Handlers/ButtonHandler.cs"; Expected = $false; Desc = "Handler file" },
    @{ Path = "src/Essentials/src/Accelerometer/Accelerometer.cs"; Expected = $false; Desc = "Essentials file" }
)

foreach ($tc in $testCases) {
    $result = Test-IsTestFile $tc.Path
    Write-TestResult "$($tc.Desc): $($tc.Path)" ($result -eq $tc.Expected) "Expected: $($tc.Expected), Got: $result"
}

# ============================================================
# Test: Find-MergeBase function
# ============================================================
Test-Section "Find-MergeBase Function"

try {
    $baseInfo = Find-MergeBase

    Write-TestResult "Find-MergeBase returns result" ($null -ne $baseInfo)

    if ($baseInfo) {
        Write-TestResult "Result has MergeBase property" ($null -ne $baseInfo.MergeBase)
        Write-TestResult "MergeBase is valid git commit" ($baseInfo.MergeBase -match '^[a-f0-9]{40}$')
        Write-TestResult "Result has BaseBranch property" ($null -ne $baseInfo.BaseBranch)
        Write-TestResult "Result has Source property" ($null -ne $baseInfo.Source)
    }
} catch {
    Write-TestResult "Find-MergeBase executes without error" $false $_.Exception.Message
}

# ============================================================
# Test: Get-FixFiles function
# ============================================================
Test-Section "Get-FixFiles Function"

try {
    $baseInfo = Find-MergeBase
    if ($baseInfo) {
        $fixFiles = Get-FixFiles -MergeBase $baseInfo.MergeBase -ExplicitFixFiles @()

        Write-TestResult "Get-FixFiles returns array" ($fixFiles -is [array] -or $fixFiles -is [string[]] -or $null -eq $fixFiles)

        if ($fixFiles -and $fixFiles.Count -gt 0) {
            # Verify none are test files
            $hasTestFile = $false
            foreach ($file in $fixFiles) {
                if (Test-IsTestFile $file) {
                    $hasTestFile = $true
                    break
                }
            }
            Write-TestResult "No test files in fix files list" (-not $hasTestFile)
        } else {
            Write-TestSkipped "Fix files validation" "No fix files detected (this may be expected)"
        }
    }
} catch {
    Write-TestResult "Get-FixFiles executes without error" $false $_.Exception.Message
}

# ============================================================
# Test: Get-FileCategories function
# ============================================================
Test-Section "Get-FileCategories Function"

try {
    $baseInfo = Find-MergeBase
    if ($baseInfo) {
        $fixFiles = Get-FixFiles -MergeBase $baseInfo.MergeBase -ExplicitFixFiles @()

        if ($fixFiles -and $fixFiles.Count -gt 0) {
            $categories = Get-FileCategories -MergeBase $baseInfo.MergeBase -FixFiles $fixFiles

            Write-TestResult "Get-FileCategories returns result" ($null -ne $categories)
            Write-TestResult "Result has RevertableFiles property" ($null -ne $categories.RevertableFiles)
            Write-TestResult "Result has NewFiles property" ($null -ne $categories.NewFiles)

            # Total should equal input
            $totalCategorized = $categories.RevertableFiles.Count + $categories.NewFiles.Count
            Write-TestResult "All files categorized" ($totalCategorized -eq $fixFiles.Count)
        } else {
            Write-TestSkipped "File categorization validation" "No fix files to categorize"
        }
    }
} catch {
    Write-TestResult "Get-FileCategories executes without error" $false $_.Exception.Message
}

# ============================================================
# Test: Script DryRun mode (via subprocess)
# ============================================================
Test-Section "Script DryRun Mode"

try {
    # Capture current git status
    $beforeStatus = git status --porcelain 2>$null

    # Run DryRun and capture output
    $output = pwsh -NoProfile -File $ScriptPath -DryRun 2>&1
    $exitCode = $LASTEXITCODE

    # Check git status unchanged
    $afterStatus = git status --porcelain 2>$null

    Write-TestResult "DryRun doesn't modify files" ($beforeStatus -eq $afterStatus)
    Write-TestResult "DryRun outputs expected text" (($output | Out-String) -match "DRY RUN")

    # Verify no state file created
    Write-TestResult "DryRun doesn't create state file" (-not (Test-Path $StateFile))
} catch {
    Write-TestResult "DryRun mode works" $false $_.Exception.Message
}

# ============================================================
# Test: Full establish/restore cycle (via subprocess)
# ============================================================
Test-Section "Establish/Restore Cycle (Script Invocation)"

# Only run this if we have fix files to work with
try {
    $baseInfo = Find-MergeBase
    $fixFiles = Get-FixFiles -MergeBase $baseInfo.MergeBase -ExplicitFixFiles @()
    $categories = Get-FileCategories -MergeBase $baseInfo.MergeBase -FixFiles $fixFiles

    if ($categories.RevertableFiles.Count -gt 0) {
        # Ensure working directory is clean before testing (checkout files to HEAD)
        foreach ($file in $categories.RevertableFiles) {
            git checkout HEAD -- $file 2>$null
        }

        # Save original state (should be empty now)
        $originalStatus = git status --porcelain 2>$null

        # Establish baseline
        $establishOutput = pwsh -NoProfile -File $ScriptPath 2>&1
        $establishExitCode = $LASTEXITCODE

        $establishOutputStr = $establishOutput | Out-String
        Write-TestResult "Establish script executes" ($establishExitCode -eq 0 -or $establishOutputStr -match "Baseline established")
        Write-TestResult "State file created" (Test-Path $StateFile)

        # Check files were reverted
        $afterEstablish = git status --porcelain 2>$null
        $statusChanged = (($afterEstablish | Out-String).Trim()) -ne (($originalStatus | Out-String).Trim())
        Write-TestResult "Files modified after establish" $statusChanged

        # Verify state file has expected structure
        if (Test-Path $StateFile) {
            $stateContent = Get-Content $StateFile -Raw | ConvertFrom-Json
            Write-TestResult "State file has MergeBase" ($null -ne $stateContent.MergeBase)
            Write-TestResult "State file has RevertedFiles" ($null -ne $stateContent.RevertedFiles)
        }

        # Restore
        $restoreOutput = pwsh -NoProfile -File $ScriptPath -Restore 2>&1
        $restoreOutputStr = $restoreOutput | Out-String

        Write-TestResult "Restore outputs expected text" ($restoreOutputStr -match "Baseline restored" -or $restoreOutputStr -match "Restoring")
        Write-TestResult "State file removed after restore" (-not (Test-Path $StateFile))

        # Check clean state
        $afterRestore = git status --porcelain 2>$null
        $statusRestored = (($afterRestore | Out-String).Trim()) -eq (($originalStatus | Out-String).Trim())
        Write-TestResult "Clean state after restore" $statusRestored

    } else {
        Write-TestSkipped "Establish/Restore cycle" "No revertable files available"
    }
} catch {
    Write-TestResult "Establish/Restore cycle works" $false $_.Exception.Message
    # Cleanup on error
    Invoke-Cleanup
}

# ============================================================
# Test: Restore with no state
# ============================================================
Test-Section "Restore With No State"

try {
    # Ensure no state file
    if (Test-Path $StateFile) {
        Remove-Item $StateFile -Force
    }

    $output = pwsh -NoProfile -File $ScriptPath -Restore 2>&1
    $outputStr = $output | Out-String

    Write-TestResult "Restore without state completes" ($outputStr -match "No baseline state found" -or $outputStr -match "Nothing to restore")
} catch {
    Write-TestResult "Restore without state handles gracefully" $false $_.Exception.Message
}

# ============================================================
# Test: Explicit FixFiles parameter
# ============================================================
Test-Section "Explicit FixFiles Parameter"

try {
    # Pass explicit files
    $testFile = "src/Controls/src/Core/Platform/Android/InnerGestureListener.cs"
    $output = pwsh -NoProfile -File $ScriptPath -DryRun -FixFiles $testFile 2>&1
    $outputStr = $output | Out-String

    Write-TestResult "Explicit FixFiles accepted" ($outputStr -match "DRY RUN" -or $outputStr -match "revert")
    Write-TestResult "Explicit file appears in output" ($outputStr -match "InnerGestureListener")
} catch {
    Write-TestResult "Explicit FixFiles works" $false $_.Exception.Message
}

# ============================================================
# Test: Explicit BaseBranch parameter
# ============================================================
Test-Section "Explicit BaseBranch Parameter"

try {
    $output = pwsh -NoProfile -File $ScriptPath -DryRun -BaseBranch "main" 2>&1
    $outputStr = $output | Out-String

    Write-TestResult "Explicit BaseBranch accepted" ($outputStr -match "main" -or $outputStr -match "DRY RUN")
} catch {
    Write-TestResult "Explicit BaseBranch works" $false $_.Exception.Message
}

# ============================================================
# Test: State file functions
# ============================================================
Test-Section "State File Functions"

try {
    # Test Save-BaselineState
    $testState = @{
        MergeBase = "abc123"
        BaseBranch = "test"
        RevertedFiles = @("file1.cs", "file2.cs")
        NewFiles = @()
        Timestamp = (Get-Date -Format "o")
    }

    Save-BaselineState $testState
    Write-TestResult "Save-BaselineState creates file" (Test-Path $StateFile)

    # Test Get-BaselineState
    $loadedState = Get-BaselineState
    Write-TestResult "Get-BaselineState returns data" ($null -ne $loadedState)
    Write-TestResult "Loaded state has correct MergeBase" ($loadedState.MergeBase -eq "abc123")
    Write-TestResult "Loaded state has correct RevertedFiles count" ($loadedState.RevertedFiles.Count -eq 2)

    # Test Remove-BaselineState
    Remove-BaselineState
    Write-TestResult "Remove-BaselineState removes file" (-not (Test-Path $StateFile))

} catch {
    Write-TestResult "State file functions work" $false $_.Exception.Message
} finally {
    # Cleanup
    if (Test-Path $StateFile) {
        Remove-Item $StateFile -Force -ErrorAction SilentlyContinue
    }
}

# ============================================================
# Summary
# ============================================================
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "TEST SUMMARY" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Passed:  $script:TestsPassed" -ForegroundColor Green
Write-Host "Failed:  $script:TestsFailed" -ForegroundColor $(if ($script:TestsFailed -gt 0) { "Red" } else { "Green" })
Write-Host "Skipped: $script:TestsSkipped" -ForegroundColor Yellow
Write-Host ""

if ($script:TestsFailed -gt 0) {
    Write-Host "TESTS FAILED" -ForegroundColor Red
    exit 1
} else {
    Write-Host "ALL TESTS PASSED" -ForegroundColor Green
    exit 0
}
