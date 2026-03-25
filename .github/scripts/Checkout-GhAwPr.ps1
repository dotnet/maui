<#
.SYNOPSIS
    Shared PR checkout for gh-aw (GitHub Agentic Workflows).

.DESCRIPTION
    Checks out a PR branch and restores trusted agent infrastructure (skills,
    instructions) from the base branch. Works for both same-repo and fork PRs.

    This script is only invoked for workflow_dispatch triggers. For pull_request
    and issue_comment, the gh-aw platform's checkout_pr_branch.cjs handles PR
    checkout automatically (it runs as a platform step after all user steps).
    workflow_dispatch skips the platform checkout entirely, so this script is
    the only thing that gets the PR code onto disk.

    SECURITY NOTE: This script checks out PR code onto disk. This is safe
    because NO subsequent user steps execute workspace code — the gh-aw
    platform copies the workspace into a sandboxed container with scrubbed
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
      GITHUB_ENV        - path to env file (set by GitHub Actions)
#>

$ErrorActionPreference = 'Stop'

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
# This script only runs for workflow_dispatch (other triggers use the platform's
# checkout_pr_branch.cjs instead). For workflow_dispatch the platform checkout is
# skipped, so this restore IS the final workspace state.
# rm -rf first to prevent fork-added files from surviving the restore.

if (Test-Path '.github/skills/') { Remove-Item -Recurse -Force '.github/skills/' }
if (Test-Path '.github/instructions/') { Remove-Item -Recurse -Force '.github/instructions/' }

git checkout $BaseSha -- .github/skills/ .github/instructions/ .github/copilot-instructions.md 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Restored agent infrastructure from base branch ($BaseSha)"
} else {
    Write-Host "⚠️ Could not restore agent infrastructure from base branch — files may come from the PR branch"
}
