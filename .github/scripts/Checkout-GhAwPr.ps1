<#
.SYNOPSIS
    Shared PR checkout for gh-aw (GitHub Agentic Workflows).

.DESCRIPTION
    Checks out a PR branch and merges the base branch to produce the same
    combined state as a pull_request merge commit. This gives the agent the
    PR's code changes plus anything new on main (skills, instructions, etc.).

    This script is only invoked for workflow_dispatch triggers. For
    pull_request_target and issue_comment, the gh-aw platform's
    checkout_pr_branch.cjs handles PR checkout automatically.
    workflow_dispatch skips the platform checkout entirely, so this script
    is the only thing that gets the PR code onto disk.

    SECURITY: Before checkout, the script verifies the PR is not from a
    fork and that the author has write access (write, maintain, or admin).
    Fork PRs are evaluated via pull_request_target instead (where the
    platform handles checkout safely inside a sandboxed container).

    DO NOT add steps after this that run scripts from the workspace
    (e.g., ./build.sh, pwsh ./script.ps1). That would create a code
    execution vulnerability. See:
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

# ── Verify PR is same-repo and author has write access ───────────────────────

$PrInfo = gh pr view $PrNumber --repo $env:GITHUB_REPOSITORY --json author,isCrossRepository --jq '{author: .author.login, isFork: .isCrossRepository}' | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to fetch PR #$PrNumber metadata"
    exit 1
}

if ($PrInfo.isFork) {
    Write-Host "⏭️ PR #$PrNumber is from a fork. workflow_dispatch does not check out fork PRs."
    Write-Host "   Fork PRs are evaluated automatically via pull_request_target."
    exit 1
}

$Permission = gh api "repos/$($env:GITHUB_REPOSITORY)/collaborators/$($PrInfo.author)/permission" --jq '.permission'
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to check permissions for '$($PrInfo.author)'"
    exit 1
}

$AllowedRoles = @('admin', 'write', 'maintain')
if ($Permission -notin $AllowedRoles) {
    Write-Host "⏭️ PR author '$($PrInfo.author)' has '$Permission' access. workflow_dispatch only processes PRs from authors with write access."
    exit 1
}
Write-Host "✅ PR #$PrNumber by '$($PrInfo.author)' ($Permission access, same-repo)"

# ── Save base branch SHA ─────────────────────────────────────────────────────

$BaseSha = git rev-parse HEAD
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to get current HEAD SHA"
    exit 1
}
Add-Content -Path $env:GITHUB_ENV -Value "BASE_SHA=$BaseSha"
Write-Host "Base branch SHA: $BaseSha"

# ── Checkout PR branch ──────────────────────────────────────────────────────

Write-Host "Checking out PR #$PrNumber..."
gh pr checkout $PrNumber --repo $env:GITHUB_REPOSITORY
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to checkout PR #$PrNumber"
    exit 1
}
Write-Host "✅ Checked out PR #$PrNumber"
git log --oneline -1

# ── Merge base branch into PR branch ────────────────────────────────────────
# Produces the same combined state as a pull_request merge commit:
# PR's changes + anything new on main. If the PR modifies a skill, the PR's
# version wins. If it doesn't, main's version is used. This lets contributors
# iterate on skills via workflow_dispatch while keeping everything else current.

Write-Host "Merging base branch ($BaseSha) into PR branch..."
git merge $BaseSha --no-edit 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️ Merge conflict — PR may have conflicts with the base branch."
    Write-Host "   The agent will run with the PR branch as-is (without base branch updates)."
    git merge --abort 2>&1
} else {
    Write-Host "✅ Merged base branch into PR — workspace has PR changes + latest main"
}
