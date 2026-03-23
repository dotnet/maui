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
  model: claude-sonnet-4

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
      if [ -n "$PR_NUMBER" ] && [ "$PR_NUMBER" != "0" ]; then
        echo "Checking out PR #$PR_NUMBER..."

        HEAD_OWNER=$(gh pr view "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --json headRepositoryOwner --jq '.headRepositoryOwner.login' 2>/dev/null || echo "")
        BASE_OWNER=$(echo "$GITHUB_REPOSITORY" | cut -d'/' -f1)
        if [ -z "$HEAD_OWNER" ]; then
          echo "⚠️ Could not determine PR owner. Skipping checkout for security."
          exit 0
        fi

        if [ "$HEAD_OWNER" != "$BASE_OWNER" ]; then
          # Fork PR: fetch individual changed files via GitHub API instead of full checkout.
          # Full `gh pr checkout` would place all fork-controlled files on disk where subsequent
          # steps could theoretically execute them. Fetching file contents via the API is a pure
          # data read — only the specific changed files are written, no git hooks or scripts run.
          echo "🔀 PR #$PR_NUMBER is from fork ($HEAD_OWNER). Fetching changed files via API..."

          HEAD_SHA=$(gh pr view "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --json headRefOid --jq '.headRefOid')
          gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --name-only > .pr-changed-files.txt

          DOWNLOAD_COUNT=0
          SKIP_COUNT=0
          while IFS= read -r FILE; do
            [ -z "$FILE" ] && continue
            mkdir -p "$(dirname "$FILE")"
            if gh api "repos/${GITHUB_REPOSITORY}/contents/${FILE}?ref=${HEAD_SHA}" \
                 --jq '.content' > /tmp/gh_content.b64 2>/dev/null; then
              base64 -d < /tmp/gh_content.b64 > "${FILE}"
              DOWNLOAD_COUNT=$((DOWNLOAD_COUNT + 1))
            else
              SKIP_COUNT=$((SKIP_COUNT + 1))
            fi
          done < .pr-changed-files.txt
          rm -f /tmp/gh_content.b64

          echo "✅ Downloaded $DOWNLOAD_COUNT file(s), $SKIP_COUNT skipped/deleted"
        else
          # Same-repo PR: standard checkout (safe — code is from the same repo)
          gh pr checkout "$PR_NUMBER" --repo "$GITHUB_REPOSITORY"
          echo "✅ Checked out PR #$PR_NUMBER"
          git log --oneline -1
        fi
      else
        echo "No PR number available, using default checkout"
      fi
---

# Evaluate PR Tests

Invoke the **evaluate-pr-tests** skill: read and follow `.github/skills/evaluate-pr-tests/SKILL.md`.

## Context

- **Repository**: ${{ github.repository }}
- **PR Number**: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}

The PR's changed files are available locally. For same-repo PRs, the full branch was checked out.
For fork PRs, individual changed files were fetched via the GitHub API and a `.pr-changed-files.txt`
file lists all changed paths.

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
