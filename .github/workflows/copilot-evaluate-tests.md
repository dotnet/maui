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

timeout-minutes: 15
---

# Evaluate PR Tests

Invoke the **evaluate-pr-tests** skill: read and follow `.github/skills/evaluate-pr-tests/SKILL.md`.

## Context

- **Repository**: ${{ github.repository }}
- **PR Number**: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}

## CRITICAL: Check out the PR branch first

Before any analysis, you MUST check out the PR's actual code:

```bash
gh pr checkout <PR_NUMBER>
```

This is essential — without it, git diff and file reads will analyze the wrong branch. This also handles fork PRs by adding the fork remote automatically.

## Running the skill

1. Check out the PR branch with `gh pr checkout <number>`
2. Use `gh pr view <number>` to fetch PR metadata (title, body, labels, base branch)
3. Run `Gather-TestContext.ps1 -PrNumber <number>` to gather automated context
4. Read the fix and test files, evaluate per SKILL.md criteria
5. Post results using `add_comment` with `item_number` set to the PR number

## Posting Results

Call `add_comment` with `item_number` set to the PR number. Wrap the report in a collapsible `<details>` block:

```markdown
## 🧪 PR Test Evaluation

**Overall Verdict:** [✅ Tests are adequate | ⚠️ Tests need improvement | ❌ Tests are insufficient]

[1-2 sentence summary]

<details>
<summary>📊 Expand Full Evaluation</summary>

[Full report from SKILL.md]

</details>
```
