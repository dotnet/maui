<#
.SYNOPSIS
    Shared PR checkout for gh-aw (GitHub Agentic Workflows).

.DESCRIPTION
    Checks out a PR branch and restores trusted agent infrastructure (skills,
    instructions) from the base branch. Handles fork PRs safely.

    SECURITY NOTE: This script checks out PR code (including fork PRs) onto disk.
    This is safe because NO subsequent user steps execute workspace code — the
    gh-aw platform copies the workspace into a sandboxed container with scrubbed
    credentials before starting the agent. The classic "pwn-request" attack
    requires checkout + execution; we only do checkout.

    DO NOT add steps after this that run scripts from the workspace
    (e.g., ./build.sh, pwsh ./script.ps1). That would create an actual
    fork code execution vulnerability. See:
    https://securitylab.github.com/resources/github-actions-preventing-pwn-requests/

.NOTES
    Required environment variables (set by the calling workflow step):
      GH_TOKEN          - GitHub token for API access
      PR_NUMBER         - PR number to check out
      GITHUB_REPOSITORY - owner/repo (set by GitHub Actions)
      GITHUB_EVENT_NAME - trigger type (set by GitHub Actions)
      GITHUB_ENV        - path to env file (set by GitHub Actions)

    Optional environment variables:
      WORKFLOW_NAME     - human-readable workflow name for error messages
                          (e.g., "Evaluate PR Tests"). Defaults to "this workflow".
#>

$ErrorActionPreference = 'Stop'

$WorkflowDisplayName = if ($env:WORKFLOW_NAME) { $env:WORKFLOW_NAME } else { "this workflow" }

# ── Validate inputs ──────────────────────────────────────────────────────────

if (-not $env:PR_NUMBER -or $env:PR_NUMBER -eq '0') {
    Write-Host "No PR number available, using default checkout"
    exit 0
}

$PrNumber = $env:PR_NUMBER

# ── Save base branch SHA ─────────────────────────────────────────────────────
# Must be captured BEFORE checkout replaces HEAD.
# Exported for potential use by downstream platform steps (e.g., checkout_pr_branch.cjs)

$BaseSha = git rev-parse HEAD
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to get current HEAD SHA"
    exit 1
}
Add-Content -Path $env:GITHUB_ENV -Value "BASE_SHA=$BaseSha"

# ── Fork guard for issue_comment triggers ────────────────────────────────────
# The gh-aw platform inserts checkout_pr_branch.cjs AFTER all user steps,
# which re-checks out the fork branch and overwrites our restored skill files.
# Until gh-aw supports post-platform user steps, fork PRs must use
# workflow_dispatch instead. See: https://github.com/github/gh-aw/issues/18481

if ($env:GITHUB_EVENT_NAME -eq 'issue_comment') {
    $HeadRepoId = gh pr view $PrNumber --repo $env:GITHUB_REPOSITORY `
        --json headRepository --jq '.headRepository.id' 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $HeadRepoId) {
        Write-Host "❌ Failed to query PR repository info (API error). Blocking for safety."
        exit 1
    }

    $BaseRepoId = gh pr view $PrNumber --repo $env:GITHUB_REPOSITORY `
        --json baseRepository --jq '.baseRepository.id' 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $BaseRepoId) {
        Write-Host "❌ Failed to query PR repository info (API error). Blocking for safety."
        exit 1
    }

    if ($HeadRepoId -ne $BaseRepoId) {
        Write-Host "❌ Slash commands are not supported on fork PRs for $WorkflowDisplayName."
        Write-Host ""
        Write-Host "The gh-aw platform re-checks out the PR branch after our restore step,"
        Write-Host "which would overwrite the trusted skill and instruction files the agent"
        Write-Host "needs. See: https://github.com/github/gh-aw/issues/18481"
        Write-Host ""
        Write-Host "Workaround: use workflow_dispatch instead:"
        Write-Host "  Actions tab -> '$WorkflowDisplayName' -> 'Run workflow' -> enter PR number"
        Write-Host ""
        Write-Host "This achieves the same result and works for all PRs including forks."
        exit 1
    }
}

# ── Checkout PR branch ──────────────────────────────────────────────────────

Write-Host "Checking out PR #$PrNumber..."
gh pr checkout $PrNumber --repo $env:GITHUB_REPOSITORY
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to checkout PR #$PrNumber"
    exit 1
}
Write-Host "✅ Checked out PR #$PrNumber"
git log --oneline -1

# ── Restore agent infrastructure from base branch ────────────────────────────
# For fork PRs (workflow_dispatch path), the PR branch won't have
# .github/skills/ or .github/instructions/. Restore from base branch.
# For same-repo PRs, this replaces files with base branch copies.
# rm -rf first to prevent fork-added files from surviving the restore.

if (Test-Path '.github/skills/') { Remove-Item -Recurse -Force '.github/skills/' }
if (Test-Path '.github/instructions/') { Remove-Item -Recurse -Force '.github/instructions/' }

git checkout $BaseSha -- .github/skills/ .github/instructions/ .github/copilot-instructions.md 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Restored agent infrastructure from base branch ($BaseSha)"
} else {
    Write-Host "❌ Failed to restore agent infrastructure from $BaseSha"
    exit 1
}
