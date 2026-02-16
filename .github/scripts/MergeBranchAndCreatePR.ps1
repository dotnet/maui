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

function Write-SuccessMessage {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-WarningMessage {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-ErrorMessage {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║          Branch Merge and PR Creation Script              ║" -ForegroundColor Magenta
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

if ($DryRun) {
    Write-WarningMessage "DRY RUN MODE - No changes will be made"
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
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to fetch from $Remote"
        }
    }
    Write-SuccessMessage "Fetched latest"

    Write-Host "`n" + ("=" * 60) -ForegroundColor DarkGray
    Write-Step "Processing: $SourceBranch → $TargetBranch"
    
    $mergeBranch = "merge/$SourceBranch-to-$TargetBranch"
    $result.MergeBranch = $mergeBranch

    # Check if there's already an open PR for this merge
    Write-Step "Checking for existing open PR..."
    try {
        $existingPR = gh pr list --head $mergeBranch --base $TargetBranch --state open --json number,url --jq '.[0]' 2>&1 | Out-String
        if ($LASTEXITCODE -ne 0) {
            throw "gh pr list failed with exit code $LASTEXITCODE.`nOutput:`n$existingPR"
        }
        $existingPR = $existingPR.Trim()
    }
    catch {
        throw "Failed to check for existing PRs: $_"
    }

    if ($existingPR -and $existingPR -ne "null" -and $existingPR -ne "") {
        try {
            $prData = $existingPR | ConvertFrom-Json
        }
        catch {
            throw "Failed to parse PR data from 'gh pr list': $($_.Exception.Message). Raw output: $existingPR"
        }
        Write-ErrorMessage "An open PR already exists for this merge - previous week's PR was not merged!"
        Write-ErrorMessage "PR #$($prData.number): $($prData.url)"
        Write-ErrorMessage "Please merge or close the existing PR before running this workflow again."
        $result.Status = "Failed"
        $result.PRUrl = $prData.url
        $result.Error = "PR already exists - previous week's merge was not completed"
    }
    else {
        # Check if merge branch already exists on remote (but no PR - just need to create PR)
        try {
            $existingBranch = git ls-remote --heads $Remote $mergeBranch 2>&1 | Out-String
            if ($LASTEXITCODE -ne 0) {
                throw "git ls-remote failed with exit code $LASTEXITCODE. Output:`n$existingBranch"
            }
            $branchExists = -not [string]::IsNullOrWhiteSpace($existingBranch)
        }
        catch {
            throw "Failed to check for existing branch '$mergeBranch' on remote '$Remote': $_"
        }

        if ($branchExists) {
            Write-WarningMessage "Merge branch '$mergeBranch' exists on $Remote but no open PR found"
            Write-Step "Creating PR for existing branch..."
        }
        else {
            Write-SuccessMessage "No existing merge branch found"
        }

        try {
            if (-not $branchExists) {
                # Checkout target branch
                Write-Step "Checking out $TargetBranch..."
                if (-not $DryRun) {
                    git checkout "$Remote/$TargetBranch" -B $TargetBranch 2>&1 | Out-Null
                    if ($LASTEXITCODE -ne 0) {
                        throw "Failed to checkout $TargetBranch from $Remote"
                    }
                }
                Write-SuccessMessage "Checked out $TargetBranch"

                # Create merge branch
                Write-Step "Creating merge branch: $mergeBranch"
                if (-not $DryRun) {
                    git checkout -b $mergeBranch
                    if ($LASTEXITCODE -ne 0) {
                        throw "Failed to create merge branch $mergeBranch"
                    }
                }
                Write-SuccessMessage "Created $mergeBranch"

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
                Write-SuccessMessage "Merged $SourceBranch"

                # Push branch
                Write-Step "Pushing $mergeBranch to $Remote..."
                if (-not $DryRun) {
                    git push -u $Remote $mergeBranch 2>&1 | Out-Null
                    if ($LASTEXITCODE -ne 0) {
                        throw "Failed to push $mergeBranch to $Remote"
                    }
                }
                Write-SuccessMessage "Pushed $mergeBranch"
            }

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
                    Write-SuccessMessage "Created PR: $prUrl"
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
            Write-ErrorMessage $_.Exception.Message
            
            # Try to clean up
            if (-not $DryRun) {
                Write-Step "Attempting cleanup..."
                $mergeAbort = git merge --abort 2>&1
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "  Aborted merge" -ForegroundColor Gray
                }
                git checkout $originalBranch 2>&1 | Out-Null
                if ($LASTEXITCODE -ne 0) {
                    Write-WarningMessage "Failed to return to original branch: $originalBranch"
                }
            }
        }
    }
}
finally {
    # Return to original branch
    Write-Step "Returning to original branch: $originalBranch"
    if (-not $DryRun) {
        git checkout $originalBranch 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-WarningMessage "Failed to return to original branch $originalBranch"
        }
    }
}

# Print summary
Write-Host "`n" + ("=" * 60) -ForegroundColor Magenta
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║                        SUMMARY                            ║" -ForegroundColor Magenta
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$statusIcon = switch ($result.Status) {
    "Success" { "✅" }
    default { "❌" }
}
$statusColor = switch ($result.Status) {
    "Success" { "Green" }
    default { "Red" }
}
Write-Host "`n$statusIcon $($result.TargetBranch)" -ForegroundColor $statusColor
Write-Host "   Branch: $($result.MergeBranch)" -ForegroundColor Gray
if ($result.PRUrl) {
    Write-Host "   PR: $($result.PRUrl)" -ForegroundColor Cyan
}
if ($result.Error) {
    Write-Host "   Error: $($result.Error)" -ForegroundColor Red
}

# Exit with appropriate code
if ($result.Status -eq "Failed") {
    Write-Host "`n❌ Merge failed" -ForegroundColor Red
    exit 1
}
else {
    Write-Host "`n🎉 Merge completed successfully!" -ForegroundColor Green
    exit 0
}
