#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Establishes a "broken" baseline by reverting fix files to their merge-base state.

.DESCRIPTION
    This script provides reusable baseline logic for test verification workflows.
    It handles:
    - Finding the merge-base commit (supports fork workflows, PR metadata, etc.)
    - Detecting fix files (non-test files that changed since merge-base)
    - Reverting fix files to create a "broken" state for testing
    - Restoring files back to their current state

    Used by verify-tests-fail.ps1 and try-fix skill.

.PARAMETER BaseBranch
    Optional explicit base branch name. If not provided, auto-detects from PR metadata
    or finds the closest merge-base among common branch patterns.

.PARAMETER FixFiles
    Optional array of explicit fix files. If not provided, auto-detects from git diff
    by excluding test directories.

.PARAMETER DryRun
    Report what would be done without making any changes.

.PARAMETER Restore
    Restore previously reverted files from HEAD.

.EXAMPLE
    # Establish baseline (revert fix files)
    $baseline = ./EstablishBrokenBaseline.ps1
    # Run tests...
    ./EstablishBrokenBaseline.ps1 -Restore

.EXAMPLE
    # Dry run - see what would be reverted
    ./EstablishBrokenBaseline.ps1 -DryRun

.EXAMPLE
    # Explicit base branch
    $baseline = ./EstablishBrokenBaseline.ps1 -BaseBranch main
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$BaseBranch,

    [Parameter(Mandatory = $false)]
    [string[]]$FixFiles,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$Restore
)

$ErrorActionPreference = "Stop"
$RepoRoot = git rev-parse --show-toplevel

# ============================================================
# Test path patterns to exclude when auto-detecting fix files
# ============================================================
$script:TestPathPatterns = @(
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

# ============================================================
# Function to check if a file should be excluded from fix files
# ============================================================
function Test-IsTestFile {
    param([string]$FilePath)

    foreach ($pattern in $script:TestPathPatterns) {
        if ($FilePath -like $pattern) {
            return $true
        }
    }
    return $false
}

# ============================================================
# Find the merge-base commit (where current branch diverged from base)
# This is more robust than tracking branch names/refs
# For fork workflows: fetches directly from the PR's target repo URL
# so it works even if the fork's main branch is out of sync
# ============================================================
function Find-MergeBase {
    param([string]$ExplicitBaseBranch)

    # 1. If explicit base branch provided, use it directly
    if ($ExplicitBaseBranch) {
        # Try with origin/ prefix first, then without
        foreach ($ref in @("origin/$ExplicitBaseBranch", $ExplicitBaseBranch)) {
            $mergeBase = git merge-base HEAD $ref 2>$null
            if ($mergeBase) {
                return @{ MergeBase = $mergeBase; BaseBranch = $ExplicitBaseBranch; Source = "explicit" }
            }
        }
    }

    # 2. Try to get PR metadata including the TARGET repository
    #    This is critical for fork workflows where origin points to the fork,
    #    not the upstream repo. We fetch directly from the target repo URL.
    #    The PR URL contains the target repo: https://github.com/OWNER/REPO/pull/123
    $prJson = gh pr view --json baseRefName,url 2>$null
    if ($prJson) {
        $prInfo = $prJson | ConvertFrom-Json
        $prBaseBranch = $prInfo.baseRefName
        $prUrl = $prInfo.url

        # Parse owner/repo from PR URL: https://github.com/OWNER/REPO/pull/123
        $targetOwner = $null
        $targetRepo = $null
        if ($prUrl -match "github\.com/([^/]+)/([^/]+)/pull/") {
            $targetOwner = $matches[1]
            $targetRepo = $matches[2]
        }

        if ($prBaseBranch -and $targetOwner -and $targetRepo) {
            # Construct the target repo URL and fetch directly from it
            # This works even if the developer hasn't set up an 'upstream' remote
            # and even if their fork's main is completely out of sync
            $targetUrl = "https://github.com/$targetOwner/$targetRepo.git"
            Write-Host "PR targets $targetOwner/$targetRepo - fetching $prBaseBranch from upstream..." -ForegroundColor Cyan
            git fetch $targetUrl $prBaseBranch 2>$null

            if ($LASTEXITCODE -eq 0) {
                # FETCH_HEAD now points to the target repo's base branch
                $mergeBase = git merge-base HEAD FETCH_HEAD 2>$null
                if ($mergeBase) {
                    return @{ MergeBase = $mergeBase; BaseBranch = $prBaseBranch; Source = "pr-target-repo"; TargetRepo = "$targetOwner/$targetRepo" }
                }
            }
        }

        # Fallback: try fetching from origin (works if origin IS the target repo)
        if ($prBaseBranch) {
            git fetch origin $prBaseBranch 2>$null
            foreach ($ref in @("origin/$prBaseBranch", $prBaseBranch)) {
                $mergeBase = git merge-base HEAD $ref 2>$null
                if ($mergeBase) {
                    return @{ MergeBase = $mergeBase; BaseBranch = $prBaseBranch; Source = "pr-metadata" }
                }
            }
        }
    }

    # 3. Fallback: Find closest merge-base among common base branch patterns
    #    The "correct" base is the one with fewest commits between merge-base and HEAD
    Write-Host "No PR detected, scanning remote branches for closest base..." -ForegroundColor Cyan

    # Fetch all remote refs to ensure we have latest
    git fetch origin 2>$null

    # Get remote branches matching common base branch patterns
    $remoteBranches = git branch -r --format='%(refname:short)' 2>$null | Where-Object {
        $_ -match '^origin/(main|master|net\d+\.\d+|release/.*)$'
    }

    $bestMatch = $null
    $shortestDistance = [int]::MaxValue

    foreach ($branch in $remoteBranches) {
        $mergeBase = git merge-base HEAD $branch 2>$null
        if ($mergeBase) {
            $distance = [int](git rev-list --count "$mergeBase..HEAD" 2>$null)
            if ($distance -lt $shortestDistance) {
                $shortestDistance = $distance
                $branchName = $branch -replace '^origin/', ''
                $bestMatch = @{ MergeBase = $mergeBase; BaseBranch = $branchName; Source = "closest-merge-base"; Distance = $distance }
            }
        }
    }

    return $bestMatch
}

# ============================================================
# Get detected fix files from git diff
# ============================================================
function Get-FixFiles {
    param(
        [string]$MergeBase,
        [string[]]$ExplicitFixFiles
    )

    # Override with explicitly provided fix files
    if ($ExplicitFixFiles -and $ExplicitFixFiles.Count -gt 0) {
        return $ExplicitFixFiles
    }

    # Auto-detect from git diff
    $DetectedFixFiles = @()
    $changedFiles = git diff $MergeBase HEAD --name-only 2>$null

    if ($changedFiles) {
        foreach ($file in $changedFiles) {
            if (-not (Test-IsTestFile $file)) {
                $DetectedFixFiles += $file
            }
        }
    }

    return $DetectedFixFiles
}

# ============================================================
# Categorize fix files into revertable vs new
# ============================================================
function Get-FileCategories {
    param(
        [string]$MergeBase,
        [string[]]$FixFiles
    )

    $RevertableFiles = @()
    $NewFiles = @()

    foreach ($file in $FixFiles) {
        # Check if file exists at merge-base commit
        $existsInBase = git ls-tree -r $MergeBase --name-only -- $file 2>$null

        if ($existsInBase) {
            $RevertableFiles += $file
        } else {
            $NewFiles += $file
        }
    }

    return @{
        RevertableFiles = $RevertableFiles
        NewFiles = $NewFiles
    }
}

# ============================================================
# Revert files to merge-base state
# ============================================================
function Invoke-RevertFiles {
    param(
        [string]$MergeBase,
        [string[]]$Files
    )

    foreach ($file in $Files) {
        git checkout $MergeBase -- $file 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to revert $file from $MergeBase"
        }
    }
}

# ============================================================
# Restore files from HEAD
# ============================================================
function Invoke-RestoreFiles {
    param([string[]]$Files)

    foreach ($file in $Files) {
        git checkout HEAD -- $file 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to restore $file from HEAD"
        }
    }
}

# ============================================================
# State file for tracking reverted files (for -Restore)
# ============================================================
$StateFile = Join-Path $RepoRoot ".github/.baseline-state.json"

function Save-BaselineState {
    param([hashtable]$State)

    $stateDir = Split-Path $StateFile -Parent
    if (-not (Test-Path $stateDir)) {
        New-Item -ItemType Directory -Force -Path $stateDir | Out-Null
    }

    $State | ConvertTo-Json -Depth 10 | Set-Content $StateFile
}

function Get-BaselineState {
    if (Test-Path $StateFile) {
        return Get-Content $StateFile -Raw | ConvertFrom-Json
    }
    return $null
}

function Remove-BaselineState {
    if (Test-Path $StateFile) {
        Remove-Item $StateFile -Force
    }
}

# ============================================================
# Main execution (only when run directly, not when dot-sourced)
# ============================================================

# Check if script is being dot-sourced (imported) vs run directly
# When dot-sourced, $MyInvocation.InvocationName is "." or "&"
$script:IsBeingDotSourced = $MyInvocation.InvocationName -eq '.' -or $MyInvocation.InvocationName -eq '&'

if ($script:IsBeingDotSourced) {
    # Script is being imported - just export the functions
    return
}

# Handle -Restore mode
if ($Restore) {
    $state = Get-BaselineState
    if (-not $state) {
        Write-Host "No baseline state found. Nothing to restore." -ForegroundColor Yellow
        return @{ Restored = $false; Message = "No baseline state found" }
    }

    Write-Host "Restoring $($state.RevertedFiles.Count) file(s) from HEAD..." -ForegroundColor Cyan

    foreach ($file in $state.RevertedFiles) {
        Write-Host "  Restoring: $file" -ForegroundColor Gray
        git checkout HEAD -- $file 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  WARNING: Failed to restore $file" -ForegroundColor Yellow
        }
    }

    Remove-BaselineState
    Write-Host "Baseline restored." -ForegroundColor Green

    return @{
        Restored = $true
        RestoredFiles = $state.RevertedFiles
    }
}

# Find merge-base
Write-Host "Detecting base branch and merge point..." -ForegroundColor Cyan

$baseInfo = Find-MergeBase -ExplicitBaseBranch $BaseBranch

if (-not $baseInfo) {
    Write-Host "ERROR: Could not find merge base" -ForegroundColor Red
    Write-Host "  Tried: PR metadata, common base branches (main, net*.0, release/*)" -ForegroundColor Yellow
    Write-Host "  Specify -BaseBranch explicitly to fix." -ForegroundColor Yellow
    return @{ Success = $false; Error = "Could not find merge base" }
}

$MergeBase = $baseInfo.MergeBase
$BaseBranchName = $baseInfo.BaseBranch

if ($baseInfo.TargetRepo) {
    Write-Host "PR target: $($baseInfo.TargetRepo) ($BaseBranchName branch)" -ForegroundColor Green
} else {
    Write-Host "Base branch: $BaseBranchName (via $($baseInfo.Source))" -ForegroundColor Green
}
Write-Host "Merge base: $($MergeBase.Substring(0, 8))" -ForegroundColor Green
if ($baseInfo.Distance) {
    Write-Host "  ($($baseInfo.Distance) commits ahead of $BaseBranchName)" -ForegroundColor Gray
}

# Get fix files
$detectedFixFiles = Get-FixFiles -MergeBase $MergeBase -ExplicitFixFiles $FixFiles

if ($detectedFixFiles.Count -eq 0) {
    Write-Host "No fix files detected." -ForegroundColor Yellow
    return @{
        Success = $true
        MergeBase = $MergeBase
        BaseBranch = $BaseBranchName
        RevertedFiles = @()
        NewFiles = @()
        NoFixFiles = $true
    }
}

Write-Host "Fix files ($($detectedFixFiles.Count)):" -ForegroundColor Cyan
foreach ($file in $detectedFixFiles) {
    Write-Host "  - $file" -ForegroundColor White
}

# Categorize files
$categories = Get-FileCategories -MergeBase $MergeBase -FixFiles $detectedFixFiles

Write-Host ""
Write-Host "File categories:" -ForegroundColor Cyan
foreach ($file in $categories.RevertableFiles) {
    Write-Host "  [revert] $file" -ForegroundColor White
}
foreach ($file in $categories.NewFiles) {
    Write-Host "  [new]    $file (skipping)" -ForegroundColor Gray
}

if ($categories.RevertableFiles.Count -eq 0) {
    Write-Host ""
    Write-Host "No revertable files found. All fix files are new." -ForegroundColor Yellow
    return @{
        Success = $true
        MergeBase = $MergeBase
        BaseBranch = $BaseBranchName
        RevertedFiles = @()
        NewFiles = $categories.NewFiles
        NoRevertableFiles = $true
    }
}

# Check for uncommitted changes on revertable files
$uncommittedFiles = @()
foreach ($file in $categories.RevertableFiles) {
    $status = git status --porcelain -- $file 2>$null
    if ($status) {
        $uncommittedFiles += $file
    }
}

if ($uncommittedFiles.Count -gt 0) {
    Write-Host ""
    Write-Host "ERROR: Uncommitted changes in fix files:" -ForegroundColor Red
    foreach ($file in $uncommittedFiles) {
        Write-Host "  - $file" -ForegroundColor Yellow
    }
    Write-Host "Commit changes before running this script." -ForegroundColor Yellow
    return @{ Success = $false; Error = "Uncommitted changes"; UncommittedFiles = $uncommittedFiles }
}

# DryRun mode
if ($DryRun) {
    Write-Host ""
    Write-Host "[DRY RUN] Would revert $($categories.RevertableFiles.Count) file(s) to merge-base" -ForegroundColor Cyan
    return @{
        Success = $true
        DryRun = $true
        MergeBase = $MergeBase
        BaseBranch = $BaseBranchName
        WouldRevert = $categories.RevertableFiles
        NewFiles = $categories.NewFiles
    }
}

# Revert files
Write-Host ""
Write-Host "Reverting $($categories.RevertableFiles.Count) file(s) to merge-base ($($MergeBase.Substring(0, 8)))..." -ForegroundColor Cyan

foreach ($file in $categories.RevertableFiles) {
    Write-Host "  Reverting: $file" -ForegroundColor Gray
    git checkout $MergeBase -- $file 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ERROR: Failed to revert $file" -ForegroundColor Red
        return @{ Success = $false; Error = "Failed to revert $file" }
    }
}

# Save state for -Restore
Save-BaselineState @{
    MergeBase = $MergeBase
    BaseBranch = $BaseBranchName
    RevertedFiles = $categories.RevertableFiles
    NewFiles = $categories.NewFiles
    Timestamp = (Get-Date -Format "o")
}

Write-Host "Baseline established. $($categories.RevertableFiles.Count) file(s) reverted." -ForegroundColor Green
Write-Host "Run with -Restore to restore files." -ForegroundColor Cyan

return @{
    Success = $true
    MergeBase = $MergeBase
    BaseBranch = $BaseBranchName
    RevertedFiles = $categories.RevertableFiles
    NewFiles = $categories.NewFiles
}
