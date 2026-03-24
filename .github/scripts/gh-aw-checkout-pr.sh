#!/usr/bin/env bash
# gh-aw-checkout-pr.sh — Shared PR checkout for gh-aw (GitHub Agentic Workflows)
#
# Checks out a PR branch and restores trusted agent infrastructure (skills,
# instructions) from the base branch. Handles fork PRs safely.
#
# REQUIRED environment variables (set by the calling workflow step):
#   GH_TOKEN          — GitHub token for API access
#   PR_NUMBER         — PR number to check out
#   GITHUB_REPOSITORY — owner/repo (set by GitHub Actions)
#   GITHUB_EVENT_NAME — trigger type (set by GitHub Actions)
#   GITHUB_ENV        — path to env file (set by GitHub Actions)
#
# OPTIONAL environment variables:
#   WORKFLOW_NAME     — human-readable workflow name for error messages
#                       (e.g., "Evaluate PR Tests"). Defaults to "this workflow".
#
# SECURITY NOTE: This script checks out PR code (including fork PRs) onto disk.
# This is safe because NO subsequent user steps execute workspace code — the
# gh-aw platform copies the workspace into a sandboxed container with scrubbed
# credentials before starting the agent. The classic "pwn-request" attack
# requires checkout + execution; we only do checkout.
#
# ⚠️  DO NOT add steps after this that run scripts from the workspace
#     (e.g., ./build.sh, pwsh ./script.ps1). That would create an actual
#     fork code execution vulnerability. See:
#     https://securitylab.github.com/resources/github-actions-preventing-pwn-requests/

set -euo pipefail

WORKFLOW_DISPLAY_NAME="${WORKFLOW_NAME:-this workflow}"

# ── Validate inputs ──────────────────────────────────────────────────────────

if [ -z "${PR_NUMBER:-}" ] || [ "$PR_NUMBER" = "0" ]; then
  echo "No PR number available, using default checkout"
  exit 0
fi

# ── Save base branch SHA ─────────────────────────────────────────────────────
# Must be captured BEFORE checkout replaces HEAD.
# Written to GITHUB_ENV so the restore step (below) can read it.

BASE_SHA=$(git rev-parse HEAD)
# Exported for potential use by downstream platform steps (e.g., checkout_pr_branch.cjs)
echo "BASE_SHA=$BASE_SHA" >> "$GITHUB_ENV"

# ── Fork guard for issue_comment triggers ────────────────────────────────────
# The gh-aw platform inserts checkout_pr_branch.cjs AFTER all user steps,
# which re-checks out the fork branch and overwrites our restored skill files.
# Until gh-aw supports post-platform user steps, fork PRs must use
# workflow_dispatch instead. See: https://github.com/github/gh-aw/issues/18481

if [ "$GITHUB_EVENT_NAME" = "issue_comment" ]; then
  HEAD_REPO_ID=$(gh pr view "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" \
    --json headRepository --jq '.headRepository.id' 2>/dev/null) \
    || { echo "❌ Failed to query PR repository info (API error). Blocking for safety."; exit 1; }
  BASE_REPO_ID=$(gh pr view "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" \
    --json baseRepository --jq '.baseRepository.id' 2>/dev/null) \
    || { echo "❌ Failed to query PR repository info (API error). Blocking for safety."; exit 1; }

  if [ "$HEAD_REPO_ID" != "$BASE_REPO_ID" ]; then
    echo "❌ Slash commands are not supported on fork PRs for $WORKFLOW_DISPLAY_NAME."
    echo ""
    echo "The gh-aw platform re-checks out the PR branch after our restore step,"
    echo "which would overwrite the trusted skill and instruction files the agent"
    echo "needs. See: https://github.com/github/gh-aw/issues/18481"
    echo ""
    echo "Workaround: use workflow_dispatch instead:"
    echo "  Actions tab → '$WORKFLOW_DISPLAY_NAME' → 'Run workflow' → enter PR number"
    echo ""
    echo "This achieves the same result and works for all PRs including forks."
    exit 1
  fi
fi

# ── Checkout PR branch ──────────────────────────────────────────────────────

echo "Checking out PR #$PR_NUMBER..."
gh pr checkout "$PR_NUMBER" --repo "$GITHUB_REPOSITORY"
echo "✅ Checked out PR #$PR_NUMBER"
git log --oneline -1

# ── Restore agent infrastructure from base branch ────────────────────────────
# For fork PRs (workflow_dispatch path), the PR branch won't have
# .github/skills/ or .github/instructions/. Restore from base branch.
# For same-repo PRs, this replaces files with base branch copies.
# rm -rf first to prevent fork-added files from surviving the restore.

rm -rf .github/skills/ .github/instructions/
if git checkout "$BASE_SHA" -- \
  .github/skills/ \
  .github/instructions/ \
  .github/copilot-instructions.md \
  2>/dev/null; then
  echo "✅ Restored agent infrastructure from base branch ($BASE_SHA)"
else
  echo "❌ Failed to restore agent infrastructure from $BASE_SHA"
  exit 1
fi
