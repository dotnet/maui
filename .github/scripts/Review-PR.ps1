<#
.SYNOPSIS
    Runs a PR review using Copilot CLI with skill-based prompts.

.DESCRIPTION
    Orchestrates a PR review by invoking scripts and Copilot CLI:
    
    Step 1: Branch setup           - Create review branch from main, merge PR squashed
    Step 2: Detect UI categories   - Run eng/scripts/detect-ui-test-categories.ps1 (info only)
    Step 3: Run detected UI tests  - Execute BuildAndRunHostApp.ps1 per detected category (informational)
    Step 4: Regression cross-ref   - Run Find-RegressionRisks.ps1 + run any tests from prior fix PRs
    Step 5: Gate                   - Run test verification directly (verify-tests-fail.ps1)
    Step 6: Multi-candidate review - Pre-Flight, then PARALLEL (expert-reviewer eval of PR + Try-Fix×4),
                                     then Report compares all candidates and writes winner.json
    Step 7: Post AI Summary        - Directly runs posting scripts
    Step 8: Apply labels           - Apply agent labels based on review results

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
    [switch]$UseCurrentBranch,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [string]$LogFile
)

$ErrorActionPreference = 'Stop'

if ($LogFile) {
    $logDir = Split-Path $LogFile -Parent
    if ($logDir -and -not (Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
    Start-Transcript -Path $LogFile -Force | Out-Null
}

$RepoRoot = git rev-parse --show-toplevel 2>$null
if (-not $RepoRoot) { Write-Error "Not in a git repository"; exit 1 }

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

# ─── Prerequisites ────────────────────────────────────────────────────────────
Write-Host "📋 Checking prerequisites..." -ForegroundColor Yellow

$ghVersion = gh --version 2>$null | Select-Object -First 1
if (-not $ghVersion) { Write-Error "GitHub CLI (gh) not installed"; exit 1 }
Write-Host "  ✅ GitHub CLI: $ghVersion" -ForegroundColor Green

$copilotCmd = Get-Command copilot -ErrorAction SilentlyContinue
if (-not $copilotCmd) { Write-Error "Copilot CLI not installed"; exit 1 }
$copilotVersion = (& copilot --version 2>&1 | Out-String).Trim()
if (-not $copilotVersion) { $copilotVersion = $copilotCmd.Source }
Write-Host "  ✅ Copilot CLI: $copilotVersion" -ForegroundColor Green

$prInfo = gh pr view $PRNumber --json title,state 2>$null | ConvertFrom-Json
if (-not $prInfo) { Write-Error "PR #$PRNumber not found"; exit 1 }
Write-Host "  ✅ PR: $($prInfo.title)" -ForegroundColor Green

# ─── Shared prompt rules ─────────────────────────────────────────────────────
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

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 1: Branch Setup (Create Review Branch & Cherry-Pick PR)
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
Write-Host "║  STEP 1: BRANCH SETUP                                     ║" -ForegroundColor Yellow
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Yellow

$reviewBranch = "pr-review-$PRNumber"

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
        # feature/regression-check) which may differ from the PR's target
        # branch (main, net10.0, net11.0). Squash-merging the PR onto the
        # wrong base causes spurious conflicts. Resolve the PR's actual
        # target branch and use it as the merge base.
        $prTarget = gh pr view $PRNumber --json baseRefName --jq '.baseRefName' 2>$null
        if (-not $prTarget) { $prTarget = 'main' }
        Write-Host "  🤖 CI environment detected — using PR target branch '$prTarget' as merge base" -ForegroundColor Cyan
        git fetch origin "$prTarget" 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ⚠️ Failed to fetch '$prTarget', falling back to current branch" -ForegroundColor Yellow
        } else {
            git checkout "origin/$prTarget" 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) {
                Write-Host "  ⚠️ Failed to checkout 'origin/$prTarget', falling back to current branch" -ForegroundColor Yellow
            } else {
                $baseSha = git rev-parse --short HEAD 2>$null
                Write-Host "  📌 Review base: $prTarget @ $baseSha" -ForegroundColor Cyan
            }
        }
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
    } else {
        Write-Host "  ❌ Squash-merge had conflicts." -ForegroundColor Red
        git merge --abort 2>$null
        git reset --hard HEAD 2>$null

        # Clean up branches
        git checkout $originalBranch 2>$null
        git branch -D $reviewBranch 2>$null
        git branch -D $tempBranch 2>$null

        # Post a comment on the PR about merge conflicts
        $conflictBody = "⚠️ **Merge Conflict Detected** — This PR has merge conflicts with its target branch. Please rebase onto the target branch and resolve the conflicts."
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

# ─── Helper: Parse `dotnet test --logger "console;verbosity=detailed"` ──────
# Extracts per-test results (Passed/Failed/Skipped) plus failure messages and
# stack traces from raw stdout. Used by STEP 3 so the AI summary comment shows
# WHICH tests failed and WHY, not just an aggregate exit code.
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
# any console-scrape ambiguity. STEP 3 prefers TRX when available because
# parsing console output is fragile when many tests run, lines wrap, or
# multi-line ErrorRecords get glued together by PowerShell stream merging.
# Get-TrxResults: dot-source from shared file (single source of truth)
. "$PSScriptRoot/shared/Get-TrxResults.ps1"

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

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $toolCount = 0
    $turnCount = 0
    $currentIntent = ""
    $modelName = ""
    $failedTools = @()

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
    # Model is overridable via $env:COPILOT_REVIEW_MODEL so contributors without internal-model access
    # can run this script (e.g., with 'claude-opus-4.6' or 'claude-sonnet-4.6').
    $copilotModel = if ($env:COPILOT_REVIEW_MODEL) { $env:COPILOT_REVIEW_MODEL } else { 'claude-opus-4.7-1m-internal' }
    & copilot -p $Prompt --allow-all --output-format json --model $copilotModel 2>&1 | ForEach-Object {
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
                        $currentIntent = $args_.intent ?? $currentIntent
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
                    $displayName = $toolName -replace '^github-mcp-server-', 'gh/'

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
                        Write-Host "  $detail" -ForegroundColor DarkGray
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
                        Write-Host "  │  💬 " -ForegroundColor DarkGray -NoNewline
                        Write-Host $preview -ForegroundColor White
                    }
                }
                'result' {
                    # Final stats — note: 'result' is a top-level event with no 'data' wrapper.
                    $usage = $event.usage
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
            # Non-JSON line (e.g. stats) — pass through as-is
            if ($line.Trim()) {
                Write-Host "  $line" -ForegroundColor DarkGray
            }
        }
    }
    $exitCode = $LASTEXITCODE
    $stopwatch.Stop()

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

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  STEP 2: DETECT UI TEST CATEGORIES                       ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

$uitestCategories = ""

$detectScript = Join-Path $RepoRoot "eng/scripts/detect-ui-test-categories.ps1"
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
        # can read them via $(stageDependencies.ReviewPR.CopilotReview.outputs['RunReview.detectedCategories']).
        # `isOutput=true` is required for cross-stage consumption; the
        # variable name is namespaced under the step's `name:` property
        # in ci-copilot.yml (currently `RunReview`) by AzDO.
        # Local invocations (no $env:TF_BUILD) won't have an AzDO variable
        # store but the marker is harmless — gets ignored.
        $catsForOutput = if ([string]::IsNullOrWhiteSpace($uitestCategories)) { 'NONE' } else { $uitestCategories }
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
#  STEP 3: RUN DETECTED UI TEST CATEGORIES (script, no copilot agent)
# ═════════════════════════════════════════════════════════════════════════════
# Runs the UI test categories that Step 2 detected. Skipped when:
#   - $uitestCategories is 'NONE'        (no UI-relevant changes)
#   - $uitestCategories is empty/blank    (run-all matrix — too expensive locally)
# Results are appended to the existing uitests/content.md so they show up in
# the same collapsible section of the AI summary comment.

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  STEP 3: RUN DETECTED UI TESTS                            ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

$uitestRunResult = "SKIPPED"
$uitestRunnerScript = Join-Path $PSScriptRoot "BuildAndRunHostApp.ps1"

if ($uitestCategories -eq 'NONE') {
    Write-Host "  ⏭️  Skipped — detection returned NONE (no UI-relevant changes)" -ForegroundColor DarkGray
} elseif ([string]::IsNullOrWhiteSpace($uitestCategories)) {
    Write-Host "  ⏭️  Skipped — detection returned the run-all matrix (too expensive to run all categories locally)" -ForegroundColor DarkGray
} elseif (-not (Test-Path $uitestRunnerScript)) {
    Write-Host "  ⚠️ BuildAndRunHostApp.ps1 not found — cannot run UI tests" -ForegroundColor Yellow
} else {
    # Mirror the regression-test platform fallback so a $Platform-less invocation
    # still has a concrete target instead of silently picking nothing.
    $uitestPlatform = if ($Platform) { $Platform } else { "android" }

    $categoryList = @($uitestCategories -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    Write-Host "  🧪 Running $($categoryList.Count) detected UI category(ies) on '$uitestPlatform'…" -ForegroundColor Cyan

    $uitestRunOutputDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/uitests"
    New-Item -ItemType Directory -Force -Path $uitestRunOutputDir | Out-Null

    $uitestPassed = 0
    $uitestFailed = 0
    $uitestSkipped = 0
    $uitestDetails = @()

    foreach ($cat in $categoryList) {
        Write-Host ""
        Write-Host "  📋 [$cat] Invoke-UITestWithRetry -Platform $uitestPlatform -Category $cat" -ForegroundColor Cyan

        # Delegate to the shared deploy+retry script so STEP 3 uses the
        # SAME pre-boot + retry-on-env-error + device-reboot pipeline as
        # the Gate (verify-tests-fail.ps1's Invoke-TestRun +
        # Invoke-TestRunWithRetry). When the Android emulator/iOS sim
        # rejects an install ("ADB0010 Broken pipe", XHarness exit 83,
        # AppiumServerHasNotBeenStartedLocally, …) the helper retries up
        # to 3 times with adb reboot / simctl boot recovery between
        # attempts. Without this, a single transient install failure was
        # turning into "119 OneTimeSetUp timeouts" in the AI summary.
        $catLogPath = Join-Path $uitestRunOutputDir ("$cat-output.log")
        $catStart = Get-Date
        $sharedRunner = Join-Path $PSScriptRoot "shared/Invoke-UITestWithRetry.ps1"
        $runResult = $null
        $testOutput = @()
        $testExitCode = -1
        $envErrHit = $null
        try {
            $runResult = & $sharedRunner `
                -Platform $uitestPlatform `
                -Category $cat `
                -RepoRoot $RepoRoot `
                -LogFile $catLogPath
            if ($runResult) {
                $testOutput   = $runResult.Output
                $testExitCode = $runResult.ExitCode
                $envErrHit    = $runResult.EnvErrorHit
                Write-Host "    Attempts: $($runResult.Attempts) · Exit: $testExitCode · EnvError: $envErrHit" -ForegroundColor Gray
                $testOutput | Select-Object -Last 20 | ForEach-Object { Write-Host "    $_" }
            }
        } catch {
            Write-Host "    ⚠️ Shared runner threw: $_" -ForegroundColor Yellow
            $testExitCode = -1
        }
        $catDuration = [math]::Round(((Get-Date) - $catStart).TotalSeconds, 1)

        # Parse per-test results. We prefer the TRX file written by
        # `dotnet test --logger trx` (mirrors CI pipeline 313's
        # `RunTestWithLocalDotNet`) — it's authoritative because it captures
        # every test's outcome, duration, error and stack regardless of
        # how the console output got wrapped or interleaved. We only fall
        # back to scraping the captured stdout via Get-DotNetTestResults
        # when the TRX is missing (build/deploy crashed before tests ran,
        # or an older BuildAndRunHostApp.ps1 ran without --logger trx).
        $perTestResults = @()
        $trxAggregate   = $null
        $trxPath        = if ($runResult) { [string]$runResult.TrxResultFile } else { $null }
        if ($trxPath -and (Test-Path $trxPath)) {
            try {
                $trxAggregate = Get-TrxResults -TrxPath $trxPath
                if ($trxAggregate) {
                    $perTestResults = @($trxAggregate.Results)
                    Write-Host "    📄 TRX parsed: total=$($trxAggregate.Total) passed=$($trxAggregate.Passed) failed=$($trxAggregate.Failed) skipped=$($trxAggregate.Skipped)" -ForegroundColor Cyan
                }
            } catch {
                Write-Host "    ⚠️ Failed to parse TRX $trxPath : $_" -ForegroundColor Yellow
            }
        }
        if (-not $trxAggregate) {
            try {
                $perTestResults = @(Get-DotNetTestResults -Lines $testOutput)
            } catch {
                Write-Host "    ⚠️ Failed to parse per-test results: $_" -ForegroundColor Yellow
            }
        }
        $catFailedTests = @($perTestResults | Where-Object { $_.status -eq 'Failed' })
        $catPassedTests = @($perTestResults | Where-Object { $_.status -eq 'Passed' })
        # Authoritative aggregate counts: TRX > per-test array. (When the TRX
        # is present its <Counters total="N" .../> attribute beats counting
        # array items because VSTest may report retries/skips that aren't in
        # individual <UnitTestResult> nodes.)
        if ($trxAggregate) {
            $catTotalCount  = [int]$trxAggregate.Total
            $catPassedCount = [int]$trxAggregate.Passed
            $catFailedCount = [int]$trxAggregate.Failed
        } else {
            $catTotalCount  = $perTestResults.Count
            $catPassedCount = $catPassedTests.Count
            $catFailedCount = $catFailedTests.Count
        }

        if ($testExitCode -eq 0) {
            Write-Host "    ✅ PASSED ($catDuration s, $catPassedCount test(s))" -ForegroundColor Green
            $uitestPassed++
            $uitestDetails += @{
                category     = $cat
                result       = 'PASSED'
                duration_s   = $catDuration
                tests_total  = $catTotalCount
                tests_passed = $catPassedCount
                tests_failed = 0
                passed_tests = @($catPassedTests | ForEach-Object { @{ name = $_.name; duration = $_.duration } })
                failed_tests = @()
            }
        } elseif ($testExitCode -eq -1) {
            Write-Host "    ⏭️ SKIPPED" -ForegroundColor DarkGray
            $uitestSkipped++
            $uitestDetails += @{
                category     = $cat
                result       = 'SKIPPED'
                duration_s   = $catDuration
                reason       = 'Runner threw an exception'
                tests_total  = 0
                tests_passed = 0
                tests_failed = 0
                passed_tests = @()
                failed_tests = @()
            }
        } else {
            Write-Host "    ❌ FAILED (exit code: $testExitCode, $catDuration s, $catFailedCount failed test(s))" -ForegroundColor Red
            foreach ($ft in $catFailedTests) {
                Write-Host "       • $($ft.name)" -ForegroundColor Red
            }
            $uitestFailed++
            # When per-test parsing found no failures (e.g. build/deploy
            # crashed before tests ran), capture the last 30 lines of the
            # category's stdout so the AI summary can show the actual error
            # (CS0246, RS0016, missing dependency, etc.) instead of just
            # "exit code 1".
            $buildTail = $null
            if ($catFailedCount -eq 0) {
                try {
                    $tail = @($testOutput | ForEach-Object { "$_" } | Select-Object -Last 30)
                    $buildTail = ($tail -join "`n").Trim()
                } catch { $buildTail = $null }
            }
            # Detect infrastructure-level failure: when ALL failures share a
            # OneTimeSetUp timeout AND the build log shows the HostApp couldn't
            # be installed/launched (ADB install failure, broken pipe, no
            # device, etc.), this is a CI infra problem — not real test
            # regressions. Reviewers shouldn't be alarmed by "119 failed tests"
            # when the app never even started.
            #
            # If $envErrHit was set above, use that — the retry loop already
            # detected an env error and exhausted retries.
            $infraSignals = @(
                'InstallFailedException',
                'Failure calling service package',
                'ADB0010',
                'Broken pipe',
                'no devices/emulators found',
                'device offline',
                'Could not connect to device',
                'Failed to launch the application',
                'cmd: Failure'
            )
            $infraReason = $envErrHit
            if (-not $infraReason -and $catFailedTests.Count -gt 0) {
                # Two equally-strong infra-failure indicators:
                #   (a) every failure is `OneTimeSetUp:` — driver couldn't
                #       reach the runner UI button.
                #   (b) the build itself failed (`Build FAILED`) and there
                #       are zero passes — NUnit then "fails" every test in
                #       the assembly because the HostApp APK never got
                #       installed.
                $logText = ($testOutput | ForEach-Object { "$_" }) -join "`n"
                $allOneTimeSetup = @($catFailedTests | Where-Object {
                    ($_.error -as [string]) -match '^OneTimeSetUp:'
                }).Count -eq $catFailedTests.Count
                $buildFailedNoPasses = ($catPassedCount -eq 0) -and ($logText -match '(?m)^Build FAILED\.\s*$')
                if ($allOneTimeSetup -or $buildFailedNoPasses) {
                    foreach ($sig in $infraSignals) {
                        if ($logText -match [regex]::Escape($sig)) {
                            $infraReason = $sig
                            break
                        }
                    }
                }
            }
            $uitestDetails += @{
                category     = $cat
                result       = 'FAILED'
                duration_s   = $catDuration
                exit_code    = $testExitCode
                tests_total  = $catTotalCount
                tests_passed = $catPassedCount
                tests_failed = $catFailedCount
                build_tail   = $buildTail
                infra_failure = $infraReason
                trx_path     = $trxPath
                passed_tests = @($catPassedTests | ForEach-Object { @{ name = $_.name; duration = $_.duration } })
                failed_tests = @($catFailedTests | ForEach-Object {
                    @{
                        name     = $_.name
                        duration = $_.duration
                        error    = $_.error
                        stack    = $_.stack
                    }
                })
            }
        }
    }

    if ($uitestFailed -gt 0) {
        $uitestRunResult = "FAILED"
        Write-Host ""
        Write-Host "  🔴 UI test result: $uitestPassed passed, $uitestFailed FAILED, $uitestSkipped skipped" -ForegroundColor Red
    } elseif ($uitestPassed -gt 0) {
        $uitestRunResult = "PASSED"
        Write-Host ""
        Write-Host "  ✅ UI test result: $uitestPassed passed, $uitestSkipped skipped" -ForegroundColor Green
    } else {
        $uitestRunResult = "SKIPPED"
        Write-Host ""
        Write-Host "  ⏭️  All UI categories skipped ($uitestSkipped total)" -ForegroundColor DarkGray
    }

    # Append a results table to the existing uitests/content.md so the same
    # collapsible "UI Tests — Category Detection" section in the AI summary
    # comment now contains both the detected list and the run results.
    $uitestContentFile = Join-Path $uitestRunOutputDir "content.md"
    $appendMd = New-Object System.Text.StringBuilder
    [void]$appendMd.AppendLine()
    [void]$appendMd.AppendLine("### 🧪 UI Test Execution Results")
    [void]$appendMd.AppendLine()
    $resultIcon = switch ($uitestRunResult) { "PASSED" { "✅" }; "FAILED" { "❌" }; default { "⏭️" } }
    [void]$appendMd.AppendLine("$resultIcon **$uitestRunResult** — $uitestPassed passed, $uitestFailed failed, $uitestSkipped skipped (platform: ``$uitestPlatform``)")
    [void]$appendMd.AppendLine()
    if ($uitestDetails.Count -gt 0) {
        [void]$appendMd.AppendLine("| Category | Result | Tests | Duration | Notes |")
        [void]$appendMd.AppendLine("|---|---|---|---|---|")
        foreach ($d in $uitestDetails) {
            $icon = switch ($d.result) { "PASSED" { "✅" }; "FAILED" { "❌" }; default { "⏭️" } }
            # Tests column: e.g. "1/1 ✓" on pass, "0/1 (1 ❌)" on fail. When the
            # category itself failed but no per-test failures were parsed (e.g.
            # build/deploy crashed before tests ran), don't claim a green ✓ —
            # show "build/deploy failed" so reviewers aren't misled.
            $tCount = if ($null -ne $d.tests_total) { [int]$d.tests_total } else { 0 }
            $tPass  = if ($null -ne $d.tests_passed) { [int]$d.tests_passed } else { 0 }
            $tFail  = if ($null -ne $d.tests_failed) { [int]$d.tests_failed } else { 0 }
            $testsCol = if ($d.infra_failure) {
                            "🛠️ infra failure ($tFail bogus failures)"
                        }
                        elseif ($d.result -eq 'FAILED' -and $tFail -eq 0) {
                            if ($tCount -eq 0) { "build/deploy failed" }
                            else { "$tPass/$tCount — build/deploy failed before per-test results" }
                        }
                        elseif ($tCount -eq 0) { "—" }
                        elseif ($tFail -gt 0) { "$tPass/$tCount ($tFail ❌)" }
                        else { "$tPass/$tCount ✓" }
            $notes = if ($d.infra_failure) { "infra: $($d.infra_failure)" }
                     elseif ($d.exit_code) { "exit code $($d.exit_code)" }
                     elseif ($d.reason)    { $d.reason }
                     else                  { "" }
            [void]$appendMd.AppendLine("| ``$($d.category)`` | $icon $($d.result) | $testsCol | $($d.duration_s)s | $notes |")
        }
    }
    [void]$appendMd.AppendLine()

    # Per-failed-category breakdown: collapsible block with each failed test's
    # name, error message, and first stack frame so a reviewer can diagnose
    # without downloading the full build artifact. When a category failed but
    # produced no per-test failures (build/deploy crashed), surface the last
    # 30 lines of stdout so the AI summary still pinpoints the cause.
    $failedCats = @($uitestDetails | Where-Object { $_.result -eq 'FAILED' -and (($_.failed_tests -and $_.failed_tests.Count -gt 0) -or $_.build_tail) })
    $infraCats = @($failedCats | Where-Object { $_.infra_failure })
    if ($infraCats.Count -gt 0) {
        [void]$appendMd.AppendLine("> ⚠️ **Infrastructure failure detected** — for $($infraCats.Count) categor$(if ($infraCats.Count -eq 1) { 'y' } else { 'ies' }) below, the HostApp couldn't be installed or launched on the device (build/deploy failed). NUnit then reports every test in the assembly as failed. **These are NOT real test regressions** — the test runner never started. Look for ``$($infraCats[0].infra_failure)`` in the build log.")
        [void]$appendMd.AppendLine()
    }
    if ($failedCats.Count -gt 0) {
        [void]$appendMd.AppendLine("#### Failed test details")
        [void]$appendMd.AppendLine()
        foreach ($d in $failedCats) {
            $hasFailedTests = $d.failed_tests -and $d.failed_tests.Count -gt 0
            $headSummary = if ($d.infra_failure) {
                "🛠️ <code>$($d.category)</code> — infra failure ($($d.failed_tests.Count) bogus failures, app never installed)"
            } elseif ($hasFailedTests) {
                "❌ <code>$($d.category)</code> — $($d.failed_tests.Count) failed test$(if ($d.failed_tests.Count -ne 1) { 's' })"
            } else {
                "❌ <code>$($d.category)</code> — build/deploy failed (no per-test results)"
            }
            [void]$appendMd.AppendLine("<details><summary>$headSummary</summary>")
            [void]$appendMd.AppendLine()
            if ($hasFailedTests) {
                # GitHub's comment body limit is 65,536 chars; large categories
                # can have 100+ failures with multi-KB error messages each.
                # Group by error message to dedup the common "OneTimeSetUp:
                # Timed out…" cases (one root cause, N tests). Show full
                # detail for the first 5 unique errors, then a compact list.
                # @() wrap is required: Group-Object on a single unique key
                # returns ONE GroupInfo (not an array), and `.Count` on a
                # GroupInfo returns the size of the group, not the number of
                # groups — without @() the foreach below would iterate the
                # group's members instead of the groups themselves.
                $byErr = @($d.failed_tests | Group-Object -Property {
                    if ($_.error) { ($_.error -as [string]).Substring(0, [Math]::Min(200, ([string]$_.error).Length)) } else { '<no error>' }
                } | Sort-Object Count -Descending)

                $shownGroups = 0
                foreach ($g in $byErr) {
                    if ($shownGroups -ge 5) {
                        $remaining = ($byErr | Select-Object -Skip 5 | Measure-Object -Property Count -Sum).Sum
                        [void]$appendMd.AppendLine("…and $remaining more failure(s) with other error signatures (see CopilotLogs artifact for full detail).")
                        [void]$appendMd.AppendLine()
                        break
                    }
                    $shownGroups++

                    $first = $g.Group[0]
                    $count = $g.Count
                    if ($count -gt 1) {
                        $sampleNames = ($g.Group | Select-Object -First 3 | ForEach-Object { "``$($_.name)``" }) -join ', '
                        $more = if ($count -gt 3) { ", … (+$($count - 3) more)" } else { '' }
                        [void]$appendMd.AppendLine("**$count tests failed with the same error** — e.g. $sampleNames$more")
                    } else {
                        [void]$appendMd.AppendLine("**``$($first.name)``** *(took $($first.duration))*")
                    }
                    [void]$appendMd.AppendLine()

                    $errBody = if ($first.error) {
                        $e = [string]$first.error
                        if ($e.Length -gt 1500) { $e.Substring(0, 1500) + "`n…(truncated)" } else { $e }
                    } else { "_(no error message captured)_" }
                    [void]$appendMd.AppendLine('```')
                    [void]$appendMd.AppendLine($errBody)
                    [void]$appendMd.AppendLine('```')
                    if ($first.stack) {
                        $firstFrame = ($first.stack -split "`n" | Where-Object { $_.Trim() } | Select-Object -First 1)
                        if ($firstFrame) {
                            [void]$appendMd.AppendLine("> at $($firstFrame.Trim().TrimStart('a','t',' '))")
                            [void]$appendMd.AppendLine()
                        }
                    }
                }

                # Always print a compact name-only list of every failed test
                # so reviewers know exactly which tests need to be re-run,
                # even if their error matched a deduped group above.
                if ($d.failed_tests.Count -gt 1) {
                    [void]$appendMd.AppendLine("<details><summary>All $($d.failed_tests.Count) failed test names</summary>")
                    [void]$appendMd.AppendLine()
                    foreach ($ft in $d.failed_tests) {
                        [void]$appendMd.AppendLine("- ``$($ft.name)``")
                    }
                    [void]$appendMd.AppendLine()
                    [void]$appendMd.AppendLine("</details>")
                    [void]$appendMd.AppendLine()
                }
            }
            if ($d.build_tail) {
                $tail = [string]$d.build_tail
                if ($tail.Length -gt 3000) { $tail = $tail.Substring($tail.Length - 3000) }
                [void]$appendMd.AppendLine("Last 30 lines of build/test stdout:")
                [void]$appendMd.AppendLine()
                [void]$appendMd.AppendLine('```')
                [void]$appendMd.AppendLine($tail)
                [void]$appendMd.AppendLine('```')
            }
            [void]$appendMd.AppendLine()
            [void]$appendMd.AppendLine("</details>")
            [void]$appendMd.AppendLine()
        }
    }

    # Per-passed-category mini-summary: only emitted if there were ANY passed
    # tests, so empty/skipped runs stay quiet.
    $passedCats = @($uitestDetails | Where-Object { $_.passed_tests -and $_.passed_tests.Count -gt 0 -and $_.result -eq 'PASSED' })
    if ($passedCats.Count -gt 0) {
        [void]$appendMd.AppendLine("<details><summary>Show $(($passedCats | Measure-Object -Property tests_passed -Sum).Sum) passed test name(s)</summary>")
        [void]$appendMd.AppendLine()
        foreach ($d in $passedCats) {
            [void]$appendMd.AppendLine("**``$($d.category)``**")
            [void]$appendMd.AppendLine()
            foreach ($pt in $d.passed_tests) {
                [void]$appendMd.AppendLine("- ``$($pt.name)`` *($($pt.duration))*")
            }
            [void]$appendMd.AppendLine()
        }
        [void]$appendMd.AppendLine("</details>")
        [void]$appendMd.AppendLine()
    }
    [void]$appendMd.AppendLine("_Failures here are informational only — they do not block the gate or affect try-fix candidate scoring._")
    Add-Content $uitestContentFile $appendMd.ToString() -Encoding UTF8

    # JSON summary for downstream consumers / debugging.
    @{
        result   = $uitestRunResult
        platform = $uitestPlatform
        passed   = $uitestPassed
        failed   = $uitestFailed
        skipped  = $uitestSkipped
        details  = $uitestDetails
    } | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $uitestRunOutputDir "test-results.json") -Encoding UTF8

    # result.txt — one-line traceability marker (PASSED / FAILED / SKIPPED).
    $uitestRunResult | Set-Content (Join-Path $uitestRunOutputDir "result.txt") -Encoding UTF8
}

# Restore the review branch in case BuildAndRunHostApp.ps1 (or any of its
# child invocations) detached HEAD or switched branches.
git checkout $reviewBranch 2>$null | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ⚠️ Failed to restore review branch '$reviewBranch' after Step 3 — subsequent steps may run against the wrong tree" -ForegroundColor Red
}

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 4: REGRESSION CROSS-REFERENCE (script, no copilot agent)
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  STEP 4: REGRESSION CROSS-REFERENCE                      ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

$regressionOutputDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/regression-check"
$regressionScript = Join-Path $PSScriptRoot "Find-RegressionRisks.ps1"
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

# --- Regression Test Execution (part of STEP 4) ---
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
        $uiTestRunner = Join-Path $RepoRoot ".github/scripts/BuildAndRunHostApp.ps1"
        $deviceTestRunner = Join-Path $RepoRoot ".github/skills/run-device-tests/scripts/Run-DeviceTests.ps1"

        foreach ($t in $regressionTests) {
            Write-Host ""
            Write-Host "  📋 [$($t.Type)] $($t.TestName) (from fix PR #$($t.FixPR))" -ForegroundColor Cyan

            try {
                switch ($t.Type) {
                    'UITest' {
                        if (Test-Path $uiTestRunner) {
                            Write-Host "    🖥️ Running UI test via BuildAndRunHostApp.ps1 -Platform $regrPlatform -TestFilter `"$($t.Filter)`"" -ForegroundColor Cyan
                            $testOutput = & $uiTestRunner -Platform $regrPlatform -TestFilter $t.Filter 2>&1
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
                            $testOutput = & $deviceTestRunner -Project $dtProject -Platform $regrPlatform -TestFilter $t.Filter 2>&1
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
                            $testOutput = dotnet test $resolvedProj --filter $t.Filter --logger "console;verbosity=minimal" 2>&1
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
#  STEP 5: Gate - Test Before and After Fix (script, no copilot agent)
# ═════════════════════════════════════════════════════════════════════════════

# TEMP: Skip Gate (STEP 5) + Try-Fix (STEP 6) for fast iteration on the
# inline-stages architecture. Both phases are expensive (build the whole
# repo, run agents on multiple candidates) and we just need STEPs 1-4 +
# STEP 7 (post comment) to validate that detectedCategories /
# aiSummaryCommentId output variables flow through to the new
# RunDeepUITests + UpdateAISummaryComment stages. Flip $skipGateAndTryFix
# back to $false (or delete the wrapper) once the new pipeline stages
# are validated end-to-end.
$skipGateAndTryFix = $false
if (-not $skipGateAndTryFix) {

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
Write-Host "║  STEP 5: GATE — TEST VERIFICATION                         ║" -ForegroundColor Yellow
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Yellow

$gateOutputDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate"
New-Item -ItemType Directory -Force -Path $gateOutputDir | Out-Null

# Detect tests in PR
Write-Host "  🔍 Detecting tests in PR #$PRNumber..." -ForegroundColor Cyan
$testDetectScript = Join-Path $PSScriptRoot "shared/Detect-TestsInDiff.ps1"
if (Test-Path $testDetectScript) {
    $testDetectScript = (Resolve-Path $testDetectScript).Path
    & pwsh -NoProfile -File $testDetectScript -PRNumber $PRNumber 2>&1 | ForEach-Object { Write-Host "    $_" }
} else {
    Write-Host "    ⚠️ Detect-TestsInDiff.ps1 not found at $testDetectScript" -ForegroundColor Yellow
}

# Determine platform for gate
$gatePlatform = if ($Platform) { $Platform } else { "android" }
Write-Host "  🧪 Running gate on platform: $gatePlatform" -ForegroundColor Cyan

$verifyScript = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "../skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1"))
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
    # Clear previous attempt's report so a crash mid-run doesn't leak its classification into this one.
    Remove-Item $gateContentFile -Force -ErrorAction SilentlyContinue
    # Note: -RequireFullVerification is intentionally OMITTED. The verify script
    # auto-detects fix files from the diff; if there are none (e.g., a test-only
    # PR like a regression repro), it falls back to "verify failure only" mode
    # and reports whether the new tests fail without any fix. Passing the flag
    # would force the script to error out for those PRs.
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
}

} # end else (verify script exists)

# Exit code: 0 = passed, 1 = verification failed, 2 = no tests detected
$gateResult = switch ($gateExitCode) {
    0 { "PASSED" }
    2 { "SKIPPED" }
    default { "FAILED" }
}
$gateColor = switch ($gateResult) { "PASSED" { "Green" } "SKIPPED" { "Yellow" } default { "Red" } }
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
        $resultIcon = switch ($gateResult) { "PASSED" { "✅" } "SKIPPED" { "⚠️" } default { "❌" } }
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
        $resultIcon = switch ($gateResult) { "PASSED" { "✅" } default { "❌" } }
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

# Post gate result as a separate PR comment
$postGateScript = Join-Path $PSScriptRoot "post-gate-comment.ps1"
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

# Restore review branch
git checkout $reviewBranch 2>$null | Out-Null

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 6: PR Review (3-phase skill: Pre-Flight, Try-Fix, Report)
# ═════════════════════════════════════════════════════════════════════════════

$gateStatusForPrompt = switch ($gateResult) {
    "PASSED" { "Gate ✅ PASSED — tests FAIL without fix, PASS with fix." }
    "SKIPPED" { "Gate ⚠️ SKIPPED — no tests detected in this PR. Consider suggesting the author add tests." }
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

$step2Prompt = @"
Run a multi-candidate PR review for PR #$PRNumber using the following flow.

## Phase 1 — Pre-Flight (context only)
Use the pr-review skill's pre-flight phase to gather context. Do NOT modify code.
Write summary to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pre-flight/content.md``.

## Phase 2 — Candidate generation (run BOTH branches; do not skip either)
Generate the following candidates. Each candidate is an alternative diff against the PR's base branch. Do this work in isolated worktrees / scratch copies so artifacts do NOT clobber each other.

### Branch A — Expert reviewer evaluation of the current PR fix (in sandbox)
Use the code-review skill with the maui-expert-reviewer agent to evaluate the PR's existing fix. Apply the reviewer's actionable feedback in a sandbox copy and treat the result as a candidate named ``pr-plus-reviewer``.
- Always also write the raw inline findings to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/inline-findings.json`` (these are file:line findings against the PR's diff and feed the inline-comment posting step).
- Write candidate output to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/expert-pr-eval/content.md``.

### Branch B — Try-Fix ×4 (ALWAYS runs — do NOT skip)
Use the pr-review skill's try-fix phase to generate FOUR independent candidate fixes (``try-fix-1`` through ``try-fix-4``). Each candidate must load domain knowledge from a different maui-expert-reviewer dimension so the candidates are diverse.
- 🚨 You MUST generate all four candidates. Do not short-circuit even if Pre-Flight or the expert eval suggests the PR is already correct.
- Write each candidate's output to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/try-fix-{N}/content.md`` (N = 1..4).
- Aggregate try-fix narrative for the AI summary comment to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/try-fix/content.md``.
$regressionTestInstruction

## Phase 3 — Report
The expert reviewer evaluates ALL candidates against each other:
- ``pr`` (the raw PR fix as submitted)
- ``pr-plus-reviewer`` (PR fix + reviewer feedback applied in sandbox)
- ``try-fix-1``..``try-fix-4``
Pick the single winning candidate. **Candidates that failed regression tests MUST be ranked lower than candidates that passed them.**
Write the comparative analysis to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/report/content.md``.

## Phase 4 — Winner manifest (REQUIRED)
Write ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/winner.json`` with this exact schema:
``````json
{
  "schemaVersion": 1,
  "winner": "pr" | "pr-plus-reviewer" | "try-fix-1" | "try-fix-2" | "try-fix-3" | "try-fix-4",
  "isPRFix": true | false,
  "summary": "1-3 sentence rationale for why this candidate won",
  "candidateDiff": "<unified diff against PR base — REQUIRED iff isPRFix is false; OMIT or empty string when isPRFix is true>"
}
``````
Rules:
- ``isPRFix`` MUST be ``true`` when ``winner`` is ``pr`` or ``pr-plus-reviewer``.
- ``isPRFix`` MUST be ``false`` when ``winner`` is any ``try-fix-*``.
- When ``isPRFix`` is ``false``, ``candidateDiff`` MUST be a non-empty unified diff. Truncate at 55 KB if larger and end with a ``... [truncated]`` marker line.

$platformInstruction
$autonomousRules

**Gate result (already completed in a prior step):** $gateStatusForPrompt
Do NOT re-run gate verification. The gate phase is handled separately.
⚠️ Do NOT create or overwrite ``gate/content.md`` — it is already generated by the gate script with detailed test output.
"@

Invoke-CopilotStep -StepName "STEP 6: PR REVIEW" -Prompt $step2Prompt | Out-Null

# Restore review branch — the Copilot agent may have switched branches (e.g. via gh pr checkout)
git checkout $reviewBranch 2>$null | Out-Null

# ─── Tier 3 refresh: feed AI categories back into category detection ───
# Step 2 ran detection without the AI tier (-AiCategories was empty).
# Pre-flight (Step 6) wrote `ai-categories.md`; re-run detection now so the
# unified comment reflects all three tiers before Step 7 posts.
$aiCategoriesFile = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/uitests/ai-categories.md"
if ((Test-Path $detectScript) -and (Test-Path $aiCategoriesFile)) {
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

            $uitestOutputDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/uitests"
            $uitestContentFile = Join-Path $uitestOutputDir "content.md"

            # Preserve any STEP 3 results table that was appended earlier so
            # the post-comment phase keeps the actual run output (categories +
            # execution table) instead of just the refreshed category list.
            $preservedExecution = ""
            if (Test-Path $uitestContentFile) {
                $existing = Get-Content $uitestContentFile -Raw
                $marker = '### 🧪 UI Test Execution Results'
                $idx = $existing.IndexOf($marker)
                if ($idx -ge 0) {
                    $preservedExecution = $existing.Substring($idx)
                }
            }

            if ($refreshedCategories -eq 'NONE') {
                "No UI test categories needed for this PR (no UI-relevant changes)." | Set-Content $uitestContentFile -Encoding UTF8
            } elseif ([string]::IsNullOrWhiteSpace($refreshedCategories)) {
                "Full UI test matrix will run (no specific categories detected from PR changes)." | Set-Content $uitestContentFile -Encoding UTF8
            } else {
                "**Detected UI test categories:** ``$refreshedCategories``" | Set-Content $uitestContentFile -Encoding UTF8
            }

            if (-not [string]::IsNullOrWhiteSpace($preservedExecution)) {
                Add-Content $uitestContentFile "`n$preservedExecution" -Encoding UTF8
            }
        }
    } catch {
        Write-Host "  ⚠️ AI-tier category refresh failed (non-fatal, keeping Step 2 result): $_" -ForegroundColor Yellow
    }
}

}  # END TEMP SKIP wrapper for STEP 5 (Gate) + STEP 6 (Try-Fix) — see $skipGateAndTryFix above

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 7: Post AI Summary Comment (direct script invocation)
#  When DEFER_COMMENT_TO_STAGE3=true, skip posting here — Stage 3
#  (UpdateAISummaryComment) will post the full comment after deep tests.
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  STEP 7: POST AI SUMMARY                                  ║" -ForegroundColor Magenta
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

if ($env:DEFER_COMMENT_TO_STAGE3 -eq 'true') {
    Write-Host "  ⏭️ Deferred to Stage 3 (DEFER_COMMENT_TO_STAGE3=true)" -ForegroundColor Gray
    Write-Host "  ℹ️  Content files saved in CopilotLogs artifact" -ForegroundColor Gray
    # Still emit a dummy output var so Stage 3 condition works
    Write-Host "##vso[task.setvariable variable=aiSummaryCommentId;isOutput=true]DEFERRED"
} else {

$summaryScriptsDir = Join-Path $RepoRoot ".github/scripts"

# Post PR review phases (pre-flight, try-fix, report)
$aiSummaryCommentId = $null
$reviewScript = Join-Path $summaryScriptsDir "post-ai-summary-comment.ps1"
if (Test-Path $reviewScript) {
    try {
        Write-Host "  📝 Posting PR review summary..." -ForegroundColor Cyan
        if ($DryRun) {
            $reviewOutput = & $reviewScript -PRNumber $PRNumber -DryRun
        } else {
            $reviewOutput = & $reviewScript -PRNumber $PRNumber
        }
        # Capture comment ID from script output (format: COMMENT_ID=<id>)
        $idLine = $reviewOutput | Where-Object { $_ -match '^COMMENT_ID=' } | Select-Object -Last 1
        if ($idLine -match '^COMMENT_ID=(\d+)$') {
            $aiSummaryCommentId = $Matches[1]
            Write-Host "  ✅ PR review summary posted (comment ID: $aiSummaryCommentId)" -ForegroundColor Green

            # Persist comment ID + PR number to a known location and emit
            # as an output variable so the downstream UpdateAISummaryComment
            # stage in ci-copilot.yml can rewrite the STEP 3 section once
            # the deep UI tests finish on the platform-pool agents.
            $commentIdFile = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/ai-summary-comment-id.txt"
            New-Item -ItemType Directory -Force -Path (Split-Path -Parent $commentIdFile) | Out-Null
            $aiSummaryCommentId | Set-Content $commentIdFile -Encoding UTF8
            Write-Host "##vso[task.setvariable variable=aiSummaryCommentId;isOutput=true]$aiSummaryCommentId"
        } else {
            Write-Host "  ✅ PR review summary posted" -ForegroundColor Green
        }
    } catch {
        Write-Host "  ⚠️ PR review summary posting failed (non-fatal): $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ⚠️ post-ai-summary-comment.ps1 not found — skipping review summary" -ForegroundColor Yellow
}

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
    # Non-PR candidate won — submit a REQUEST_CHANGES review with the candidate diff in the body
    Write-Host "  📝 Non-PR candidate won — submitting REQUEST_CHANGES review with candidate diff..." -ForegroundColor Cyan

    $maxDiffBytes = 55KB
    $diff = [string]$winner.candidateDiff
    $truncated = $false
    # Truncate by binary-searching the largest character count whose UTF-8
    # encoding fits within the byte budget (reserving room for the marker line).
    # O(log n) and much cheaper than the previous O(n²) trim-512-and-recount loop.
    $marker = "`n... [truncated]"
    $markerBytes = [System.Text.Encoding]::UTF8.GetByteCount($marker)
    $budget = $maxDiffBytes - $markerBytes
    if ([System.Text.Encoding]::UTF8.GetByteCount($diff) -gt $maxDiffBytes) {
        $lo = 0
        $hi = $diff.Length
        while ($lo -lt $hi) {
            $mid = [int](($lo + $hi + 1) / 2)
            $bytes = [System.Text.Encoding]::UTF8.GetByteCount($diff.Substring(0, $mid))
            if ($bytes -le $budget) { $lo = $mid } else { $hi = $mid - 1 }
        }
        $diff = $diff.Substring(0, $lo) + $marker
        $truncated = $true
    }

    # Compute an outer code fence longer than any backtick run inside the diff
    # (minimum 4) so the diff content cannot accidentally close the fence and
    # leak into the surrounding markdown. Preserves the diff text exactly.
    $maxBacktickRun = 0
    foreach ($m in [regex]::Matches($diff, '`+')) {
        if ($m.Length -gt $maxBacktickRun) { $maxBacktickRun = $m.Length }
    }
    $fenceLen = [Math]::Max(4, $maxBacktickRun + 1)
    $fence = '`' * $fenceLen

    $rationale = if ($winner.summary) { [string]$winner.summary } else { "Automated review identified a stronger candidate fix." }
    $reviewBody = @"
🤖 **Automated review — alternative fix proposed**

The expert-reviewer evaluation compared the PR fix against $($winner.winner -replace 'try-fix-','#') automatically generated candidates and selected ``$($winner.winner)`` as the strongest fix.

**Why:** $rationale

Please consider applying the candidate diff below (or use it as guidance). Once you push an update, this workflow will re-trigger and re-evaluate.

<details><summary>Candidate diff (``$($winner.winner)``)</summary>

${fence}diff
$diff
$fence

</details>
$( if ($truncated) { "`n_The diff was truncated to fit GitHub's review body limit._" } )
"@

    if ($DryRun) {
        Write-Host "  [DryRun] Would POST review state=REQUEST_CHANGES with body length $($reviewBody.Length)" -ForegroundColor Yellow
    } else {
        try {
            $bodyJson = @{ body = $reviewBody; event = 'REQUEST_CHANGES' } | ConvertTo-Json -Compress -Depth 5
            $tmp = New-TemporaryFile
            Set-Content -LiteralPath $tmp -Value $bodyJson -Encoding utf8 -NoNewline
            $resp = & gh api -X POST "repos/dotnet/maui/pulls/$PRNumber/reviews" --input $tmp 2>&1
            Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  ✅ REQUEST_CHANGES review submitted" -ForegroundColor Green
            } else {
                Write-Host "  ⚠️ Failed to submit REQUEST_CHANGES review (non-fatal): $resp" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "  ⚠️ REQUEST_CHANGES submission threw (non-fatal): $_" -ForegroundColor Yellow
        }
    }
    Write-Host "  ⏭️ Skipping inline findings (winner is not the PR fix)" -ForegroundColor Gray
}

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 8: Apply Labels
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Blue
Write-Host "║  STEP 8: APPLY LABELS                                     ║" -ForegroundColor Blue
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Blue

$labelHelperPath = Join-Path $RepoRoot ".github/scripts/shared/Update-AgentLabels.ps1"
if (Test-Path $labelHelperPath) {
    try {
        . $labelHelperPath
        Apply-AgentLabels -PRNumber $PRNumber -RepoRoot $RepoRoot
        Write-Host "  ✅ Labels applied" -ForegroundColor Green
    } catch {
        Write-Host "  ⚠️ Label application failed (non-fatal): $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ⚠️ Label helper not found — skipping" -ForegroundColor Yellow
}

} # END DEFER_COMMENT_TO_STAGE3 else block

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
