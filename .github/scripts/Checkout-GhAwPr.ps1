<#
.SYNOPSIS
    Shared PR checkout for gh-aw (GitHub Agentic Workflows).

.DESCRIPTION
    Checks out a PR branch and restores trusted agent infrastructure (skills,
    instructions) from the base branch. For issue_comment triggers, fork PRs
    are rejected (fail-closed) because the platform's checkout_pr_branch.cjs
    overwrites restored files after user steps. Fork PRs should use
    workflow_dispatch instead.

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
      GITHUB_EVENT_NAME - trigger type (set by GitHub Actions)
#>

$ErrorActionPreference = 'Stop'

# ── Validate inputs ──────────────────────────────────────────────────────────

if (-not $env:PR_NUMBER -or $env:PR_NUMBER -eq '0') {
    Write-Host "No PR number available, using default checkout"
    exit 0
}

$PrNumber = $env:PR_NUMBER

# ── Fork guard (issue_comment only) ─────────────────────────────────────────
# For issue_comment triggers, platform's checkout_pr_branch.cjs runs AFTER user
# steps and re-checks out the fork branch, overwriting any restored skill/instruction
# files. A fork could include a crafted SKILL.md that alters agent behavior.
# Fail closed: if we can't verify origin, exit 1 (not 0).
# Fork PRs can still be evaluated via workflow_dispatch (where platform checkout is skipped).

if ($env:GITHUB_EVENT_NAME -eq 'issue_comment') {
    $isFork = gh pr view $PrNumber --repo $env:GITHUB_REPOSITORY --json isCrossRepository --jq '.isCrossRepository' 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Could not verify PR origin — failing closed"
        exit 1
    }
    if ($isFork -eq 'true') {
        Write-Host "::notice::Fork PR detected — /evaluate-tests via issue_comment is not supported for fork PRs. Use workflow_dispatch with pr_number=$PrNumber instead."
        exit 1
    }
}

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
# Best-effort restore of skill/instruction files from the base branch.
# - workflow_dispatch: platform checkout is skipped, so this IS the final state
# - issue_comment (same-repo): platform's checkout_pr_branch.cjs runs after and
#   overwrites, but files already match (same repo). Fork PRs are blocked above.
# - pull_request (same-repo): files already exist, this is a no-op
# rm -rf first to prevent fork-added files from surviving the restore.

if (Test-Path '.github/skills/') { Remove-Item -Recurse -Force '.github/skills/' }
if (Test-Path '.github/instructions/') { Remove-Item -Recurse -Force '.github/instructions/' }

git checkout $BaseSha -- .github/skills/ .github/instructions/ .github/copilot-instructions.md 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Restored agent infrastructure from base branch ($BaseSha)"
} else {
    Write-Host "⚠️ Could not restore agent infrastructure from base branch — files may come from the PR branch"
}
