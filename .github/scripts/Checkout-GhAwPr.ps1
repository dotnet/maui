<#
.SYNOPSIS
    Shared PR checkout and trusted-infra restore for gh-aw workflows.

.DESCRIPTION
    Checks out a PR branch and restores trusted agent infrastructure (skills,
    instructions) from the base branch. This gives the agent the PR's code
    changes with the latest skills and instructions from main.

    Currently used for workflow_dispatch triggers. For slash_command and
    issue_comment triggers, the gh-aw platform's checkout_pr_branch.cjs
    handles PR checkout automatically — but may overwrite trusted infra
    with fork-supplied files. Call this script after platform checkout to
    restore trusted .github/ from the base branch.

    SECURITY: Before checkout, the script verifies the PR author has
    write access (write, maintain, or admin) and rejects fork PRs.
    This prevents checkout of untrusted code in privileged contexts.

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

$RawJson = gh pr view $PrNumber --repo $env:GITHUB_REPOSITORY --json author,isCrossRepository --jq '{author: .author.login, isFork: .isCrossRepository}'
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to fetch PR #$PrNumber metadata"
    exit 1
}

try {
    $PrInfo = $RawJson | ConvertFrom-Json
} catch {
    Write-Host "❌ PR #$PrNumber returned malformed JSON: $RawJson"
    exit 1
}

if (-not $PrInfo -or -not $PrInfo.author) {
    Write-Host "❌ PR #$PrNumber returned empty or malformed metadata"
    exit 1
}

if ($PrInfo.isFork) {
    Write-Host "⏭️ PR #$PrNumber is from a fork — skipping. Fork PRs are evaluated in the sandboxed agent container via the platform's checkout_pr_branch.cjs."
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

# ── Restore agent infrastructure from base branch ────────────────────────────
# Replace skills and instructions with base branch versions to ensure the agent
# always uses trusted infrastructure from main. Uses git checkout to read files
# directly from the commit tree — works in shallow clones (no history traversal).
# Restore BEFORE deleting so a failure doesn't leave the workspace without infra.

git checkout $BaseSha -- .github/skills/ .github/instructions/ .github/copilot-instructions.md 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Restored agent infrastructure from base branch ($BaseSha)"
} else {
    Write-Host "❌ Failed to restore agent infrastructure from base branch — aborting to prevent running with untrusted infra"
    exit 1
}
