<#
.SYNOPSIS
    Runs a PR review using Copilot CLI with skill-based prompts.

.DESCRIPTION
    Orchestrates a PR review by invoking scripts and Copilot CLI:
    
    Step 0: Branch setup        - Create review branch from main, merge PR squashed
    Step 1: Gate                - Run test verification directly (verify-tests-fail.ps1)
    Step 2: pr-review skill     - 3-phase review (Pre-Flight, Try-Fix, Report)
    Step 3: Post AI Summary     - Directly runs posting scripts
    Step 4: Apply labels        - Apply agent labels based on review results

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
#  STEP 0: Branch Setup (Create Review Branch & Cherry-Pick PR)
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
Write-Host "║  STEP 0: BRANCH SETUP                                     ║" -ForegroundColor Yellow
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

    # Auto-detect CI environment — in CI, always use current branch
    $isCI = $env:CI -or $env:TF_BUILD -or $env:GITHUB_ACTIONS -or $env:BUILD_BUILDID
    if ($isCI -and -not $UseCurrentBranch) {
        Write-Host "  🤖 CI environment detected — using current branch instead of main" -ForegroundColor Cyan
        $UseCurrentBranch = $true
    }

    # Capture original branch so error paths can restore it (not `git checkout -` which is unreliable)
    $originalBranch = git branch --show-current 2>$null
    if (-not $originalBranch) { $originalBranch = git rev-parse HEAD 2>$null }

    if (-not $UseCurrentBranch) {
        # Default: checkout main first
        Write-Host "  📌 Checking out main branch..." -ForegroundColor Cyan
        git checkout main 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) { Write-Error "Failed to checkout main"; exit 1 }
        $pullOutput = git pull origin main --ff-only 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ⚠️ git pull failed (non-fatal, continuing with local main): $pullOutput" -ForegroundColor Yellow
        }
        $baseSha = git rev-parse --short HEAD 2>$null
        Write-Host "  📌 Review base: main @ $baseSha" -ForegroundColor Cyan
    } else {
        $currentBranch = git branch --show-current 2>$null
        if (-not $currentBranch) { $currentBranch = "(detached HEAD)" }
        Write-Host "  📌 Using current branch: $currentBranch" -ForegroundColor Cyan
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
    & copilot -p $Prompt --allow-all --output-format json 2>&1 | ForEach-Object {
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
                    # Final stats
                    $usage = $event.data.usage
                    if ($usage) {
                        $elapsed = $stopwatch.Elapsed.ToString("mm\:ss")
                        $apiMs = if ($usage.totalApiDurationMs) { [math]::Round($usage.totalApiDurationMs / 1000, 1) } else { "?" }
                        $changes = $usage.codeChanges
                        $filesChanged = if ($changes -and $changes.filesModified) { $changes.filesModified.Count } else { 0 }
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
#  STEP 1: Gate - Test Before and After Fix (script, no copilot agent)
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
Write-Host "║  STEP 1: GATE — TEST VERIFICATION                         ║" -ForegroundColor Yellow
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Yellow

$gateOutputDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate"
New-Item -ItemType Directory -Force -Path $gateOutputDir | Out-Null

# Detect tests in PR
Write-Host "  🔍 Detecting tests in PR #$PRNumber..." -ForegroundColor Cyan
$detectScript = Join-Path $PSScriptRoot "shared/Detect-TestsInDiff.ps1"
& pwsh -NoProfile -Command "& '$detectScript' -PRNumber $PRNumber" 2>&1 | ForEach-Object { Write-Host "    $_" }

# Determine platform for gate
$gatePlatform = if ($Platform) { $Platform } else { "android" }
Write-Host "  🧪 Running gate on platform: $gatePlatform" -ForegroundColor Cyan

$verifyScript = Join-Path $PSScriptRoot "../skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1"
$gateOutput = & pwsh -NoProfile -File "$verifyScript" -Platform $gatePlatform -PRNumber $PRNumber -RequireFullVerification 2>&1
$gateExitCode = $LASTEXITCODE
$gateOutput | ForEach-Object { Write-Host "    $_" }

# Exit code: 0 = passed, 1 = verification failed, 2 = no tests detected
$gateResult = switch ($gateExitCode) {
    0 { "PASSED" }
    2 { "SKIPPED" }
    default { "FAILED" }
}
$gateColor = switch ($gateResult) { "PASSED" { "Green" } "SKIPPED" { "Yellow" } default { "Red" } }
Write-Host "  📁 Gate result: $gateResult" -ForegroundColor $gateColor

# Copy the verification report to gate/content.md if it exists
$verificationReport = Join-Path $gateOutputDir "verify-tests-fail/verification-report.md"
if (Test-Path $verificationReport) {
    Copy-Item $verificationReport (Join-Path $gateOutputDir "content.md") -Force
} elseif (-not (Test-Path (Join-Path $gateOutputDir "content.md"))) {
    # Create gate content based on result
    if ($gateResult -eq "SKIPPED") {
        $skipContent = @"
### Gate Result: ⚠️ SKIPPED

No tests were detected in this PR.

**Recommendation:** Add tests to verify the fix using the ``write-tests-agent``:

``````
@copilot write tests for this PR
``````

The agent will analyze the issue, determine the appropriate test type (UI test, device test, unit test, or XAML test), and create tests that verify the fix.
"@
        $skipContent | Set-Content (Join-Path $gateOutputDir "content.md")
    } else {
        "### Gate Result: $(if ($gateExitCode -eq 0) { '✅ PASSED' } else { '❌ FAILED' })`n`n**Platform:** $gatePlatform" |
            Set-Content (Join-Path $gateOutputDir "content.md")
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
#  STEP 2: PR Review (3-phase skill: Pre-Flight, Try-Fix, Report)
# ═════════════════════════════════════════════════════════════════════════════

$gateStatusForPrompt = switch ($gateResult) {
    "PASSED" { "Gate ✅ PASSED — tests FAIL without fix, PASS with fix." }
    "SKIPPED" { "Gate ⚠️ SKIPPED — no tests detected in this PR. Consider suggesting the author add tests." }
    default { "Gate ❌ FAILED — tests did NOT behave as expected." }
}

$step2Prompt = @"
First, use the code-review skill for dimensional code analysis with the maui-expert-reviewer agent — this writes inline-findings.json.
Then, use the pr-review skill to review PR #$PRNumber. The pr-review Report phase should account for any code quality issues found by the expert reviewer.

$platformInstruction
$autonomousRules

**Gate result (already completed in a prior step):** $gateStatusForPrompt
Do NOT re-run gate verification. The gate phase is handled separately.

📁 Write phase output to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/{phase}/content.md``
📁 Also write inline review findings to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/inline-findings.json``
"@

Invoke-CopilotStep -StepName "STEP 2: PR REVIEW" -Prompt $step2Prompt | Out-Null

# Restore review branch — the Copilot agent may have switched branches (e.g. via gh pr checkout)
git checkout $reviewBranch 2>$null | Out-Null

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 3: Post AI Summary Comment (direct script invocation)
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  STEP 3: POST AI SUMMARY                                  ║" -ForegroundColor Magenta
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

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
        } else {
            Write-Host "  ✅ PR review summary posted" -ForegroundColor Green
        }
    } catch {
        Write-Host "  ⚠️ PR review summary posting failed (non-fatal): $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ⚠️ post-ai-summary-comment.ps1 not found — skipping review summary" -ForegroundColor Yellow
}

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

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 4: Apply Labels
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Blue
Write-Host "║  STEP 4: APPLY LABELS                                     ║" -ForegroundColor Blue
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
