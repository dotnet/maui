#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Merges a source branch into a target branch and creates a PR.

.DESCRIPTION
    This script automates the branch merge workflow:
    1. Fetches latest from the specified remote
    2. Creates a new merge branch from the target branch (NEVER pushes to target directly)
    3. Merges source branch into the merge branch
    4. Pushes the merge branch and creates a PR with p/0 label

.PARAMETER Remote
    Required. The git remote to operate against. Example: "origin", "upstream"

.PARAMETER TargetBranch
    Required. The branch to merge into. Example: "net10.0"

.PARAMETER SourceBranch
    The source branch to merge from. Defaults to "main"

.PARAMETER DryRun
    If specified, shows what would happen without making changes

.EXAMPLE
    ./MergeBranchAndCreatePR.ps1 -Remote "origin" -TargetBranch "net10.0"
    # Merges main into net10.0, creates a PR

.EXAMPLE
    ./MergeBranchAndCreatePR.ps1 -Remote "upstream" -TargetBranch "net11.0" -SourceBranch "net10.0"
    # Merges net10.0 into net11.0 using upstream remote

.EXAMPLE
    ./MergeBranchAndCreatePR.ps1 -Remote "origin" -TargetBranch "net10.0" -DryRun
    # Shows what would happen without making changes
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Remote,
    [Parameter(Mandatory=$true)]
    [string]$TargetBranch,
    [string]$SourceBranch = "main",
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "`n📋 $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

# Get current date for branch naming
$dateStamp = Get-Date -Format "yyyyMMdd"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║          Branch Merge and PR Creation Script              ║" -ForegroundColor Magenta
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

if ($DryRun) {
    Write-Warning "DRY RUN MODE - No changes will be made"
}

Write-Host "`nConfiguration:" -ForegroundColor White
Write-Host "  Remote:         $Remote" -ForegroundColor Gray
Write-Host "  Source Branch:  $SourceBranch" -ForegroundColor Gray
Write-Host "  Target Branch:  $TargetBranch" -ForegroundColor Gray

# Store original branch to return to later
$originalBranch = git rev-parse --abbrev-ref HEAD
Write-Host "  Current Branch: $originalBranch" -ForegroundColor Gray

# Result tracking
$result = @{
    TargetBranch = $TargetBranch
    MergeBranch = $null
    Status = "Pending"
    PRUrl = $null
    Error = $null
}

try {
    # Fetch latest
    Write-Step "Fetching latest from $Remote..."
    if (-not $DryRun) {
        git fetch $Remote --prune
    }
    Write-Success "Fetched latest"

    Write-Host "`n" + ("=" * 60) -ForegroundColor DarkGray
    Write-Step "Processing: $SourceBranch → $TargetBranch"
    
    $mergeBranch = "merge/$SourceBranch-to-$TargetBranch-$dateStamp"
    $result.MergeBranch = $mergeBranch

    # Check if merge branch already exists on remote
    Write-Step "Checking if merge branch already exists..."
    $existingBranch = git ls-remote --heads $Remote $mergeBranch 2>&1
    if ($existingBranch) {
        Write-Warning "Merge branch '$mergeBranch' already exists on $Remote"
        Write-Warning "Skipping - a merge for today is already in progress"
        $result.Status = "Skipped"
        $result.Error = "Branch already exists"
    }
    else {
        Write-Success "No existing merge branch found"

    try {
        # Checkout target branch
        Write-Step "Checking out $TargetBranch..."
        if (-not $DryRun) {
            git checkout "$Remote/$TargetBranch" -B $TargetBranch 2>&1 | Out-Null
        }
        Write-Success "Checked out $TargetBranch"

        # Create merge branch
        Write-Step "Creating merge branch: $mergeBranch"
        if (-not $DryRun) {
            git checkout -b $mergeBranch
        }
        Write-Success "Created $mergeBranch"

        # Merge source branch
        Write-Step "Merging $SourceBranch into $mergeBranch..."
        if (-not $DryRun) {
            $mergeOutput = git merge "$Remote/$SourceBranch" --no-edit 2>&1
            if ($LASTEXITCODE -ne 0) {
                # Check for merge conflicts
                $conflictFiles = git diff --name-only --diff-filter=U
                if ($conflictFiles) {
                    throw "Merge conflicts detected in:`n$conflictFiles`nPlease resolve manually."
                }
                throw "Merge failed: $mergeOutput"
            }
        }
        Write-Success "Merged $SourceBranch"

        # Push branch
        Write-Step "Pushing $mergeBranch to $Remote..."
        if (-not $DryRun) {
            git push -u $Remote $mergeBranch 2>&1 | Out-Null
        }
        Write-Success "Pushed $mergeBranch"

        # Create PR with p/0 label
        Write-Step "Creating Pull Request with p/0 label..."
        $prTitle = "[$TargetBranch] Merge $SourceBranch to $TargetBranch"
        $prBody = @"
<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

## Description

Weekly merge of ``$SourceBranch`` into ``$TargetBranch``.

This PR brings the latest changes from ``$SourceBranch`` branch into the ``$TargetBranch`` branch.

## Checklist
- [ ] Verify CI passes
- [ ] Review any merge conflicts (if applicable)
"@

        if (-not $DryRun) {
            $prUrl = gh pr create `
                --base $TargetBranch `
                --head $mergeBranch `
                --title $prTitle `
                --body $prBody `
                --label "p/0" `
                2>&1

            if ($LASTEXITCODE -eq 0) {
                $result.PRUrl = $prUrl
                Write-Success "Created PR: $prUrl"
            } else {
                throw "Failed to create PR: $prUrl"
            }
        } else {
            Write-Host "  Would create PR: $prTitle" -ForegroundColor Gray
            Write-Host "  Would add label: p/0" -ForegroundColor Gray
            $result.PRUrl = "(dry-run)"
        }

        $result.Status = "Success"
    }
    catch {
        $result.Status = "Failed"
        $result.Error = $_.Exception.Message
        Write-Error $_.Exception.Message
        
        # Try to clean up
        if (-not $DryRun) {
            git merge --abort 2>&1 | Out-Null
            git checkout $originalBranch 2>&1 | Out-Null
        }
    }
    }  # End of else block for "branch doesn't exist"
}
finally {
    # Return to original branch
    Write-Step "Returning to original branch: $originalBranch"
    if (-not $DryRun) {
        git checkout $originalBranch 2>&1 | Out-Null
    }
}

# Print summary
Write-Host "`n" + ("=" * 60) -ForegroundColor Magenta
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║                        SUMMARY                            ║" -ForegroundColor Magenta
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$statusIcon = switch ($result.Status) {
    "Success" { "✅" }
    "Skipped" { "⏭️" }
    default { "❌" }
}
$statusColor = switch ($result.Status) {
    "Success" { "Green" }
    "Skipped" { "Yellow" }
    default { "Red" }
}
Write-Host "`n$statusIcon $($result.TargetBranch)" -ForegroundColor $statusColor
Write-Host "   Branch: $($result.MergeBranch)" -ForegroundColor Gray
if ($result.PRUrl) {
    Write-Host "   PR: $($result.PRUrl)" -ForegroundColor Cyan
}
if ($result.Error) {
    Write-Host "   Error: $($result.Error)" -ForegroundColor Yellow
}

# Exit with appropriate code
switch ($result.Status) {
    "Failed" {
        Write-Host "`n⚠️  Merge failed" -ForegroundColor Yellow
        exit 1
    }
    "Skipped" {
        Write-Host "`n⏭️  Merge skipped - branch already exists" -ForegroundColor Yellow
        exit 0
    }
    default {
        Write-Host "`n🎉 Merge completed successfully!" -ForegroundColor Green
        exit 0
    }
}
