#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Establishes a "broken" baseline by auto-detecting and reverting fix files to their merge-base state.

.DESCRIPTION
    This script provides reusable baseline logic for test verification workflows.
    It handles:
    - Finding the merge-base commit (supports fork workflows, PR metadata, etc.)
    - Auto-detecting fix files (non-test files that changed since merge-base)
    - Reverting fix files to create a "broken" state for testing
    - Restoring files back to their current state

    Used by verify-tests-fail.ps1 and try-fix skill.
    
    IMPORTANT: This script ONLY works with auto-detection. It will fail fast if it cannot
    detect fix files, which indicates you need to checkout the actual PR branch.

.PARAMETER BaseBranch
    Optional explicit base branch name. If not provided, auto-detects from PR metadata
    or finds the closest merge-base among common branch patterns.

.PARAMETER DryRun
    Report what would be done without making any changes.

.PARAMETER Restore
    Restore previously reverted files from HEAD.

.PARAMETER EditableFiles
    Explicit list of files to treat as the fix set, bypassing auto-detection.

.PARAMETER SnapshotOnly
    Record the scope without reverting anything. Requires a scope from
    -EditableFiles or the scope file. Used when the working tree is already in
    the broken state and there is no author fix to undo -- issue replication,
    where the defect is what HEAD ships. HEAD is then the restore point, so
    -Restore is unchanged: it still discards the agent's edits.

    Snapshot mode skips merge-base detection entirely, which also skips its
    network fetches.

.EXAMPLE
    # Establish baseline (revert fix files) - auto-detects what to revert
    $baseline = ./EstablishBrokenBaseline.ps1
    # Run tests...
    ./EstablishBrokenBaseline.ps1 -Restore

.EXAMPLE
    # Scope an already-broken tree without reverting (issue replication).
    $baseline = ./EstablishBrokenBaseline.ps1 -EditableFiles @('src/Core/src/Handler.cs') -SnapshotOnly
    # Agent edits only those files, then:
    ./EstablishBrokenBaseline.ps1 -Restore

.EXAMPLE
    # Dry run - see what would be reverted
    ./EstablishBrokenBaseline.ps1 -DryRun

.EXAMPLE
    # Explicit base branch (still auto-detects fix files)
    $baseline = ./EstablishBrokenBaseline.ps1 -BaseBranch main
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$BaseBranch,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$Restore,

    [Parameter(Mandatory = $false)]
    [string[]]$EditableFiles,

    [Parameter(Mandatory = $false)]
    [switch]$SnapshotOnly
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
    "*TestUtils*",
    "*DeviceTests.Runners*",
    "*DeviceTests.Shared*",
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

    # Get remote branches matching common base branch patterns.
    # inflight/* is included so this closest-base fallback can still pick the
    # current integration branch (inflight/current) when the primary PR-number
    # base resolution is unavailable — without it, this scan can only ever pick
    # main/net*.0/release and silently mis-bases inflight/current PRs on main
    # (gate build 14670709, #36274: 200+ file fix-set, broken without-fix build).
    $remoteBranches = git branch -r --format='%(refname:short)' 2>$null | Where-Object {
        $_ -match '^origin/(main|master|net\d+\.\d+|release/.*|inflight/.*)$'
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
# Get detected fix files from git diff (auto-detect only)
# ============================================================
function Get-FixFiles {
    param(
        [string]$MergeBase
    )

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

# A restore destroys the only copy of the agent's work. The reviewer never
# needed it back, but replicate's fix panel judges a candidate by the diff it
# left in the tree, and a candidate that obeys the skill's restoration rule
# leaves none - so obeying was being recorded as having done nothing at all.
# Every discard is therefore written down before it happens, and the panel
# recovers from it. Only the last one is kept: that is the state the candidate
# was in when it reported.
$DiscardedFile = Join-Path $RepoRoot ".github/.baseline-discarded.json"

function Save-DiscardedWork {
    param([string[]]$Files)

    $entries = @()
    foreach ($file in $Files) {
        $full = Join-Path $RepoRoot $file
        if (-not (Test-Path -LiteralPath $full)) { continue }
        try {
            $bytes = [System.IO.File]::ReadAllBytes($full)
        } catch {
            continue
        }
        $entries += [pscustomobject]@{
            Path = $file
            ContentBase64 = [Convert]::ToBase64String($bytes)
        }
    }

    if ($entries.Count -eq 0) {
        if (Test-Path $DiscardedFile) { Remove-Item $DiscardedFile -Force }
        return
    }

    $stateDir = Split-Path $DiscardedFile -Parent
    if (-not (Test-Path $stateDir)) {
        New-Item -ItemType Directory -Force -Path $stateDir | Out-Null
    }
    [pscustomobject]@{
        DiscardedAtUtc = [DateTimeOffset]::UtcNow.ToString('o')
        Files = $entries
    } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $DiscardedFile
}

# ============================================================
# Externally supplied scope (issue replication)
# ============================================================
# try-fix Step 2 runs this script with no arguments, so a caller that has no
# author diff cannot pass its scope on the command line. It seeds this
# environment variable instead and the skill's documented invocation keeps
# working untouched.
$script:BaselineScopeEnvVar = 'MAUI_BASELINE_SCOPE_FILE'

function Get-BaselineScopeFromEnvironment {
    $scopePath = [Environment]::GetEnvironmentVariable($script:BaselineScopeEnvVar)
    if ([string]::IsNullOrWhiteSpace($scopePath)) {
        return $null
    }

    if (-not (Test-Path -LiteralPath $scopePath -PathType Leaf)) {
        throw "$script:BaselineScopeEnvVar points at '$scopePath', which does not exist."
    }

    $raw = Get-Content -LiteralPath $scopePath -Raw
    try {
        $parsed = $raw | ConvertFrom-Json
    } catch {
        throw "$script:BaselineScopeEnvVar points at '$scopePath', which is not valid JSON: $($_.Exception.Message)"
    }

    $files = @($parsed.files | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($files.Count -eq 0) {
        throw "$script:BaselineScopeEnvVar points at '$scopePath', which names no files."
    }

    return $files
}

function Assert-BaselineScopeIsRestorable {
    param([string[]]$Files)

    # Restoring is 'git checkout HEAD -- <file>', so a path that HEAD does not
    # track cannot be restored. Refusing here is far better than discovering it
    # after an agent has already edited a tree we can no longer put back.
    $missing = @()
    foreach ($file in $Files) {
        $tracked = git ls-tree -r HEAD --name-only -- $file 2>$null
        if (-not $tracked) {
            $missing += $file
        }
    }

    if ($missing.Count -gt 0) {
        throw ("Baseline scope names $($missing.Count) path(s) that HEAD does not track, " +
            "so they could never be restored: $($missing -join ', ')")
    }
}

# ============================================================
# Main execution (only when run directly, not when dot-sourced)
# ============================================================

# Check if script is being dot-sourced (imported) vs run directly.
# Only '.' means dot-sourced. '&' is the call operator - the ordinary way to
# *invoke* a script - and treating it as an import made every such call a
# silent no-op: no output, no error, no state file, and an untouched
# $LASTEXITCODE. Replicate-Issue.ps1 established its fix-phase snapshot that
# way, so the scope was never once recorded, every fix candidate reported "No
# baseline state found", and each inherited the previous one's edits. The
# Pester suite invoked the script with `pwsh -File`, which runs the body, so
# it agreed with itself while production did nothing at all.
$script:IsBeingDotSourced = $MyInvocation.InvocationName -eq '.'

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

    Save-DiscardedWork -Files @($state.RevertedFiles)

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

# ============================================================
# AUTO-RESTORE: If a previous baseline is still active, restore it first
# ============================================================
# This prevents the Establish→fail→Establish loop that caused build #13539436
# to waste 3.7 hours. Instead of erroring on a dirty tree, we detect that a
# prior baseline was never restored and clean it up automatically.

$existingState = Get-BaselineState
if ($existingState) {
    Write-Host "⚠️  Previous baseline still active — auto-restoring before re-establishing..." -ForegroundColor Yellow

    foreach ($file in $existingState.RevertedFiles) {
        Write-Host "  Restoring: $file" -ForegroundColor Gray
        git checkout HEAD -- $file 2>&1 | Out-Null
    }

    Remove-BaselineState
    Write-Host "  Previous baseline restored." -ForegroundColor Green
}

# Resolve the scope before the cleanliness check, because snapshot mode
# judges cleanliness differently: see below.
$scopedFiles = $EditableFiles
$scopeCameFromEnvironment = $false
if (-not $scopedFiles -or $scopedFiles.Count -eq 0) {
    $scopedFiles = Get-BaselineScopeFromEnvironment
    $scopeCameFromEnvironment = $null -ne $scopedFiles
}

# A scope discovered from the environment always means snapshot mode: only a
# caller without an author diff seeds it. An explicit -EditableFiles keeps the
# usual revert semantics unless -SnapshotOnly says otherwise, so no existing
# caller changes behaviour.
$useSnapshotMode = $SnapshotOnly.IsPresent -or $scopeCameFromEnvironment

if ($SnapshotOnly.IsPresent -and (-not $scopedFiles -or $scopedFiles.Count -eq 0)) {
    throw "EstablishBrokenBaseline.ps1 failed: -SnapshotOnly requires a scope from -EditableFiles or $script:BaselineScopeEnvVar."
}

# ============================================================
# FAIL-FAST: Require clean working directory
# ============================================================
# After auto-restore above, the tree should be clean. If it's still dirty,
# something else is wrong (manual edits, uncommitted work, etc.).

$dirtyFiles = git status --porcelain --untracked-files=no 2>$null
if ($dirtyFiles -and $useSnapshotMode) {
    # Snapshot mode runs on a tree that is dirty by design: issue replication
    # has just authored the reproduction test into it, and that test is the
    # whole point. Refusing here is what stopped build 15069249 from recording
    # a scope at all, so the candidate got no allow-list, was blamed for a
    # pre-existing edit it never made, and -Restore left its fix in place.
    #
    # Dirt outside the scope is therefore expected and must be preserved. Dirt
    # inside the scope is not, because HEAD is the restore point and -Restore
    # would silently discard those changes.
    $scopedDirt = @($dirtyFiles -split "`n" | Where-Object {
        $entry = $_.Trim()
        if (-not $entry) { return $false }
        $path = ($entry -replace '^\S+\s+', '') -replace '^.*? -> ', ''
        $scopedFiles -contains $path.Trim('"')
    })

    if ($scopedDirt.Count -gt 0) {
        Write-Host "Snapshot mode cannot scope a file that already has uncommitted changes:" -ForegroundColor Red
        $scopedDirt | ForEach-Object { Write-Host "  $_" -ForegroundColor White }
        throw "EstablishBrokenBaseline.ps1 failed: scoped files have uncommitted changes, so HEAD is not their restore point."
    }

    Write-Host ("Snapshot mode: {0} file(s) already modified outside the scope; leaving them alone." -f
        @($dirtyFiles -split "`n" | Where-Object { $_.Trim() }).Count) -ForegroundColor Cyan
    $dirtyFiles = $null
}
if ($dirtyFiles) {
    Write-Host "" -ForegroundColor Red
    Write-Host "╔═══════════════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║  ERROR: DIRTY WORKING DIRECTORY - Cannot establish baseline       ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "The following files have uncommitted changes:" -ForegroundColor Yellow
    Write-Host ""
    $dirtyFiles -split "`n" | ForEach-Object { Write-Host "  $_" -ForegroundColor White }
    Write-Host ""
    Write-Host "This usually means a previous try-fix attempt did not restore properly." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To fix:" -ForegroundColor Cyan
    Write-Host "  1. Run 'git status' to review the changes" -ForegroundColor White
    Write-Host "  2. Either commit them: git add . && git commit -m 'Save changes'" -ForegroundColor White
    Write-Host "  3. Or discard them:   git checkout -- ." -ForegroundColor White
    Write-Host "  4. Then retry this script" -ForegroundColor White
    Write-Host ""
    
    throw "EstablishBrokenBaseline.ps1 failed: Working directory is not clean. Clean up before establishing baseline."
}

# ============================================================
# SNAPSHOT MODE: the tree is already broken, so scope it and stop
# ============================================================
# Issue replication has no author fix to undo -- the defect is what HEAD
# ships. There is nothing to revert, only a scope to record, so that try-fix
# confines itself to the files the expert phase implicated and restores them
# afterwards. HEAD is the restore point, which is what -Restore already uses.


if ($useSnapshotMode) {
    $scopedFiles = @($scopedFiles | Select-Object -Unique)
    Assert-BaselineScopeIsRestorable -Files $scopedFiles

    $headSha = (git rev-parse HEAD 2>$null)

    Write-Host "Snapshot mode: the tree is already in the broken state, nothing to revert." -ForegroundColor Cyan
    Write-Host "Editable scope ($($scopedFiles.Count)):" -ForegroundColor Cyan
    foreach ($file in $scopedFiles) {
        Write-Host "  [editable] $file" -ForegroundColor White
    }

    if ($DryRun) {
        Write-Host ""
        Write-Host "[DRY RUN] Would record $($scopedFiles.Count) editable file(s) without reverting" -ForegroundColor Cyan
        return @{
            Success       = $true
            DryRun        = $true
            SnapshotOnly  = $true
            MergeBase     = $headSha
            BaseBranch    = 'HEAD'
            WouldRevert   = @()
            EditableFiles = $scopedFiles
            NewFiles      = @()
        }
    }

    # RevertedFiles is the key try-fix reads to decide what it may edit and
    # what -Restore puts back. Writing the scope there is what lets the skill
    # run unmodified against a tree that was never reverted.
    Save-BaselineState @{
        MergeBase     = $headSha
        BaseBranch    = 'HEAD'
        Mode          = 'snapshot'
        RevertedFiles = $scopedFiles
        NewFiles      = @()
        Timestamp     = (Get-Date -Format "o")
    }

    Write-Host "Baseline scoped. $($scopedFiles.Count) file(s) editable, 0 reverted." -ForegroundColor Green
    Write-Host "Run with -Restore to discard edits." -ForegroundColor Cyan

    return @{
        Success       = $true
        SnapshotOnly  = $true
        MergeBase     = $headSha
        BaseBranch    = 'HEAD'
        RevertedFiles = $scopedFiles
        EditableFiles = $scopedFiles
        NewFiles      = @()
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
if ($scopedFiles -and $scopedFiles.Count -gt 0) {
    Write-Host "Using the explicitly supplied fix set, skipping auto-detection." -ForegroundColor Cyan
    $detectedFixFiles = @($scopedFiles | Select-Object -Unique)
} else {
    $detectedFixFiles = Get-FixFiles -MergeBase $MergeBase
}

if ($detectedFixFiles.Count -eq 0) {
    Write-Host "" -ForegroundColor Red
    Write-Host "ERROR: No fix files detected." -ForegroundColor Red
    Write-Host "" -ForegroundColor Red
    Write-Host "This means the script could not find any non-test files that changed between:" -ForegroundColor Yellow
    Write-Host "  - HEAD (current state)" -ForegroundColor White
    Write-Host "  - Merge-base: $(if ($MergeBase -and $MergeBase.Length -ge 8) { $MergeBase.Substring(0, 8) } else { $MergeBase })" -ForegroundColor White
    Write-Host "" -ForegroundColor Yellow
    Write-Host "Possible causes:" -ForegroundColor Yellow
    Write-Host "  1. You're on the wrong branch (not the actual PR branch)" -ForegroundColor White
    Write-Host "  2. Fix files were previously reverted and never restored" -ForegroundColor White
    Write-Host "  3. The PR only changes test files (no fix to test)" -ForegroundColor White
    Write-Host "" -ForegroundColor Yellow
    Write-Host "Required action:" -ForegroundColor Yellow
    Write-Host "  - Checkout the actual PR branch: gh pr checkout <PR#>" -ForegroundColor White
    Write-Host "  - Verify fix files exist in the PR changes" -ForegroundColor White
    Write-Host "" -ForegroundColor Red
    
    throw "EstablishBrokenBaseline.ps1 failed: No fix files detected. Cannot establish baseline without fix files."
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
