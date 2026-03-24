---
description: Evaluates test quality, coverage, and appropriateness on PRs that add or modify tests
on:
  pull_request:
    types: [opened, synchronize]
    paths:
      - 'src/**/tests/**'
      - 'src/**/test/**'
  issue_comment:
    types: [created]
  workflow_dispatch:
    inputs:
      pr_number:
        description: 'PR number to evaluate'
        required: true
        type: number

if: >-
  github.event_name == 'pull_request' ||
  github.event_name == 'workflow_dispatch' ||
  (github.event_name == 'issue_comment' &&
   github.event.issue.pull_request &&
   startsWith(github.event.comment.body, '/evaluate-tests'))

permissions:
  contents: read
  issues: read
  pull-requests: read

engine:
  id: copilot
  model: claude-sonnet-4.6

safe-outputs:
  add-comment:
    max: 1
    target: "*"
  noop:
  messages:
    footer: "> 🧪 *Test evaluation by [{workflow_name}]({run_url})*"
    run-started: "🔬 Evaluating tests on this PR… [{workflow_name}]({run_url})"
    run-success: "✅ Test evaluation complete! [{workflow_name}]({run_url})"
    run-failure: "❌ Test evaluation failed. [{workflow_name}]({run_url}) {status}"

tools:
  github:
    toolsets: [default]

network: defaults

concurrency:
  group: "evaluate-pr-tests-${{ github.event.issue.number || github.event.pull_request.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: true

timeout-minutes: 15

steps:
  - name: Gate — skip if no test source files in diff
    if: github.event_name == 'pull_request'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number }}
    run: |
      TEST_FILES=$(gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --name-only \
        | grep -E '\.(cs|xaml)$' \
        | grep -iE '(tests?/|TestCases|UnitTests|DeviceTests)' \
        || true)
      if [ -z "$TEST_FILES" ]; then
        echo "⏭️ No test source files (.cs/.xaml) found in PR diff. Skipping evaluation."
        exit 1
      fi
      echo "✅ Found test files to evaluate:"
      echo "$TEST_FILES" | head -20

  - name: Checkout PR branch
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}
    run: |
      # SECURITY NOTE: This step checks out PR code (including fork PRs) onto disk.
      # This is safe because NO subsequent steps execute workspace code — the gh-aw
      # platform copies the workspace into a sandboxed container with scrubbed
      # credentials before starting the agent. The classic "pwn-request" attack
      # requires checkout + execution; we only do checkout.
      #
      # ⚠️  DO NOT add steps after this that run scripts from the workspace
      #     (e.g., ./build.sh, pwsh ./script.ps1). That would create an actual
      #     fork code execution vulnerability. See:
      #     https://securitylab.github.com/resources/github-actions-preventing-pwn-requests/
      if [ -n "$PR_NUMBER" ] && [ "$PR_NUMBER" != "0" ]; then
        # Save base branch SHA to env — used by the restore step below.
        # Must be captured BEFORE checkout replaces HEAD.
        echo "BASE_SHA=$(git rev-parse HEAD)" >> "$GITHUB_ENV"

        # Block fork PRs via issue_comment trigger (/evaluate-tests).
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
            echo "❌ /evaluate-tests is not supported on fork PRs."
            echo ""
            echo "The gh-aw platform re-checks out the PR branch after our restore step,"
            echo "which would overwrite the trusted skill and instruction files the agent"
            echo "needs. See: https://github.com/github/gh-aw/issues/18481"
            echo ""
            echo "Workaround: use workflow_dispatch instead:"
            echo "  Actions tab → 'Evaluate PR Tests' → 'Run workflow' → enter PR number"
            echo ""
            echo "This achieves the same result and works for all PRs including forks."
            exit 1
          fi
        fi

        echo "Checking out PR #$PR_NUMBER..."
        gh pr checkout "$PR_NUMBER" --repo "$GITHUB_REPOSITORY"
        echo "✅ Checked out PR #$PR_NUMBER"
        git log --oneline -1
      else
        echo "No PR number available, using default checkout"
      fi

  - name: Restore agent infrastructure from base branch
    env:
      GH_TOKEN: ${{ github.token }}
    run: |
      # SECURITY: Restore trusted agent infrastructure files from the base branch
      # AND create an untracked backup that survives subsequent git checkouts.
      #
      # Problem: gh-aw inserts a platform "Checkout PR branch" step
      # (checkout_pr_branch.cjs) AFTER all user steps. This re-checks out the
      # PR branch, overwriting any files we restored here. We cannot insert
      # user steps after platform steps — this is a gh-aw limitation.
      #
      # Solution: Copy trusted files to .restore-backup/ (an untracked directory).
      # Untracked files are NOT affected by git checkout operations, so they
      # survive checkout_pr_branch.cjs. The agent prompt (baked from the base
      # branch lock.yml) instructs the agent to restore from this backup as
      # its very first action.
      #
      # Defense-in-depth layers:
      #   1. Fork PRs blocked by the check above (issue_comment path)
      #   2. pull_request fork PRs blocked by activation job's `if` condition
      #   3. Untracked backup survives checkout_pr_branch.cjs for ALL PR types
      #   4. Agent prompt from base branch (trusted) instructs restore
      if [ -n "$BASE_SHA" ]; then
        # Clean directories first to remove any fork-added files
        rm -rf .github/skills/ .github/instructions/
        # Restore from base branch
        git checkout "$BASE_SHA" -- \
          .github/skills/ \
          .github/instructions/ \
          .github/copilot-instructions.md \
          2>/dev/null
        if [ $? -eq 0 ]; then
          echo "✅ Restored agent infrastructure from base branch ($BASE_SHA)"
        else
          echo "⚠️ Failed to restore agent infrastructure from $BASE_SHA"
          exit 1
        fi

        # Create untracked backup that survives checkout_pr_branch.cjs.
        # git checkout only affects tracked files; untracked dirs are untouched.
        rm -rf .restore-backup
        mkdir -p .restore-backup/.github
        cp -r .github/skills/ .restore-backup/.github/skills/
        cp -r .github/instructions/ .restore-backup/.github/instructions/
        cp .github/copilot-instructions.md .restore-backup/.github/copilot-instructions.md
        # Also save the base SHA for the agent to verify
        echo "$BASE_SHA" > .restore-backup/.base-sha
        echo "✅ Created untracked backup in .restore-backup/"
      fi
---

# Evaluate PR Tests

## ⚠️ CRITICAL — Restore trusted agent infrastructure first

**Before doing ANYTHING else**, you MUST restore the trusted agent infrastructure files.
The gh-aw platform's `checkout_pr_branch.cjs` may have overwritten `.github/skills/`,
`.github/instructions/`, and `.github/copilot-instructions.md` with the PR branch version.
A backup of the trusted base-branch versions was saved to `.restore-backup/`.

Run these commands immediately as your **very first action**:

```bash
if [ -d ".restore-backup/.github" ]; then
  rm -rf .github/skills/ .github/instructions/
  cp -r .restore-backup/.github/skills/ .github/skills/
  cp -r .restore-backup/.github/instructions/ .github/instructions/
  cp .restore-backup/.github/copilot-instructions.md .github/copilot-instructions.md
  echo "✅ Restored trusted agent infrastructure from backup"
else
  echo "⚠️ No backup found at .restore-backup/ — using workspace files as-is"
fi
```

**Do NOT skip this step. Do NOT proceed to the evaluation until the restore is complete.**

## Task

Invoke the **evaluate-pr-tests** skill: read and follow `.github/skills/evaluate-pr-tests/SKILL.md`.

## Context

- **Repository**: ${{ github.repository }}
- **PR Number**: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}

The PR branch has been checked out for you. All files from the PR are available locally.

## Running the skill

1. Use `gh pr view <number>` to fetch PR metadata (title, body, labels, base branch). If `gh` CLI is unavailable, use the GitHub MCP tools instead.
2. Run `pwsh .github/skills/evaluate-pr-tests/scripts/Gather-TestContext.ps1` to gather automated context
3. Read the context report and the actual changed files, then evaluate per SKILL.md criteria
4. Post results using `add_comment` with `item_number` set to the PR number

## Posting Results

Call `add_comment` with `item_number` set to the PR number. Wrap the report in a collapsible `<details>` block:

```markdown
## 🧪 PR Test Evaluation

**Overall Verdict:** [✅ Tests are adequate | ⚠️ Tests need improvement | ❌ Tests are insufficient]

[1-2 sentence summary]

> 👍 / 👎 — Was this evaluation helpful? React to let us know!

<details>
<summary>📊 Expand Full Evaluation</summary>

[Full report from SKILL.md]

</details>
```
