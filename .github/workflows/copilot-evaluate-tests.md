---
description: Evaluates test quality, coverage, and appropriateness on PRs that add or modify tests
on:
  pull_request_target:
    types: [opened, synchronize, reopened]
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
      suppress_output:
        description: 'Dry-run — evaluate but do not post output on the PR'
        required: false
        type: boolean
        default: false
  bots:
    - "copilot-swe-agent[bot]"

if: >-
  ((!github.event.repository.fork) || github.event_name == 'workflow_dispatch') &&
  (
    (github.event_name == 'pull_request_target' && github.event.pull_request.draft == false) ||
    github.event_name == 'workflow_dispatch' ||
    (github.event_name == 'issue_comment' &&
     github.event.issue.pull_request &&
     startsWith(github.event.comment.body, '/evaluate-tests'))
  )

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
    report-as-issue: false
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
  group: "evaluate-pr-tests-${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: true

timeout-minutes: 15

steps:
  - name: Gate — skip if no test source files in diff
    if: github.event_name == 'pull_request_target'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number }}
    run: |
      # Try gh pr diff first; fall back to REST API for large PRs (300+ files)
      TEST_FILES=$(gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --name-only 2>/dev/null \
        | grep -E '\.(cs|xaml)$' \
        | grep -iE '(tests?/|TestCases|UnitTests|DeviceTests)' \
        || true)
      if [ -z "$TEST_FILES" ]; then
        # gh pr diff fails with HTTP 406 for PRs with 300+ files; use paginated files API
        TEST_FILES=$(gh api "repos/$GITHUB_REPOSITORY/pulls/$PR_NUMBER/files" --paginate --jq '.[].filename' 2>/dev/null \
          | grep -E '\.(cs|xaml)$' \
          | grep -iE '(tests?/|TestCases|UnitTests|DeviceTests)' \
          || true)
      fi
      if [ -z "$TEST_FILES" ]; then
        echo "⏭️ No test source files (.cs/.xaml) found in PR diff. Skipping evaluation."
        exit 1
      fi
      echo "✅ Found test files to evaluate:"
      echo "$TEST_FILES" | head -20

  # Only needed for workflow_dispatch — for pull_request_target and issue_comment,
  # the gh-aw platform's checkout_pr_branch.cjs handles PR checkout automatically.
  # workflow_dispatch skips the platform checkout entirely, so we must do it here.
  # The script gates on PR author having write access before checkout.
  - name: Checkout PR and restore agent infrastructure
    if: github.event_name == 'workflow_dispatch'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
    run: pwsh .github/scripts/Checkout-GhAwPr.ps1
---

# Evaluate PR Tests

Invoke the **evaluate-pr-tests** skill: read and follow `.github/skills/evaluate-pr-tests/SKILL.md`.

## Context

- **Repository**: ${{ github.repository }}
- **PR Number**: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}

The PR branch has been checked out for you. All files from the PR are available locally.

## Pre-flight check

Before starting, verify the skill file exists:

```bash
test -f .github/skills/evaluate-pr-tests/SKILL.md
```

If the file is **missing**, the fork PR branch is likely not rebased on the latest `main`. Post a comment using `add_comment`:

```markdown
## 🧪 PR Test Evaluation

❌ **Cannot evaluate**: this PR's branch does not include the evaluate-pr-tests skill (`.github/skills/evaluate-pr-tests/SKILL.md` is missing).

**Fix**: rebase your fork on the latest `main` branch and push again. The evaluation will trigger automatically once the skill file is available.
```

Then stop — do not proceed with the evaluation.

## Dry-run mode

When triggered via `workflow_dispatch`, the `suppress_output` input controls behavior.
- If `${{ inputs.suppress_output }}` == **true**, perform the full evaluation but **do not** post output on the PR. Write the evaluation to the workflow log only. This is useful for testing the skill without spamming the PR.
- If **false** (default), post the output as normal.

## When no action is needed

If there is nothing to evaluate (PR has no test files, PR is a docs-only change, etc.), you **must** call the `noop` tool with a message explaining why:

```json
{"noop": {"message": "No action needed: [brief explanation, e.g. 'PR contains no test files']"}}
```

Do not post a comment and do not silently exit — always use `noop` so the workflow run shows a clear reason.

## Running the skill

1. Use `gh pr view <number>` to fetch PR metadata (title, body, labels, base branch). If `gh` CLI is unavailable, use the GitHub MCP tools instead.
2. Run `pwsh .github/skills/evaluate-pr-tests/scripts/Gather-TestContext.ps1` to gather automated context
3. Read the context report and the actual changed files, then evaluate per SKILL.md criteria
4. Post results using `add_comment` with `item_number` set to the PR number

## Posting Results

If dry-run mode is active (`suppress_output` is true), log the evaluation report to stdout and stop — do **not** call `add_comment`.

Otherwise, call `add_comment` with `item_number` set to the PR number. Wrap the report in a collapsible `<details>` block:

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
