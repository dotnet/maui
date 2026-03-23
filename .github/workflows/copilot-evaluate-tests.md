---
description: Evaluates test quality, coverage, and appropriateness on PRs that add or modify tests
on:
  pull_request:
    types: [opened, synchronize]
    paths:
      - 'src/Controls/tests/**'
      - 'src/Core/tests/**'
      - 'src/Essentials/test/**'
      - '.github/workflows/copilot-evaluate-tests.md'
      - '.github/skills/evaluate-pr-tests/**'
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
  - name: Checkout PR branch
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}
    run: |
      if [ -n "$PR_NUMBER" ] && [ "$PR_NUMBER" != "0" ]; then
        echo "Checking out PR #$PR_NUMBER..."

        # Guard: block fork PRs on workflow_dispatch to prevent fork code execution
        HEAD_OWNER=$(gh pr view "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --json headRepositoryOwner --jq '.headRepositoryOwner.login' 2>/dev/null || echo "")
        BASE_OWNER=$(echo "$GITHUB_REPOSITORY" | cut -d'/' -f1)
        if [ -n "$HEAD_OWNER" ] && [ "$HEAD_OWNER" != "$BASE_OWNER" ]; then
          echo "⚠️ PR #$PR_NUMBER is from fork ($HEAD_OWNER). Skipping checkout for security."
          echo "The agent will use GitHub API to read PR data instead."
          exit 0
        fi

        gh pr checkout "$PR_NUMBER" --repo "$GITHUB_REPOSITORY"
        echo "✅ Checked out PR #$PR_NUMBER"
        git log --oneline -1
      else
        echo "No PR number available, using default checkout"
      fi
---

# Evaluate PR Tests

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
