---
description: Evaluates test quality, coverage, and appropriateness on PRs that add or modify tests
on:
  # pull_request_target is intentionally disabled — we don't want auto-runs on PR create/update.
  # pull_request_target:
  #   types: [opened, synchronize, reopened]
  #   paths:
  #     - 'src/**/tests/**'
  #     - 'src/**/test/**'
  slash_command:
    name: evaluate-tests
    events: [pull_request_comment]
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

labels: ["pr-review", "testing"]

# Trigger filtering: slash_command compiles to issue_comment (platform handles
# command matching). workflow_dispatch is always allowed.
if: >-
  github.event_name == 'issue_comment' ||
  github.event_name == 'workflow_dispatch'

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
    hide-older-comments: true
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
  group: "evaluate-pr-tests-${{ github.event.issue.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: true

timeout-minutes: 20

steps:
  - name: Gate — skip if no test source files in diff
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.issue.number || inputs.pr_number }}
    run: |
      # Verify this is an open PR
      if ! STATE=$(gh pr view "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --json state --jq .state 2>&1); then
        echo "❌ Failed to fetch PR #$PR_NUMBER state: $STATE"
        exit 1
      fi
      if [ "$STATE" != "OPEN" ]; then
        echo "⏭️ PR #$PR_NUMBER is $STATE — skipping evaluation."
        exit 1
      fi
      # Try gh pr diff first; fall back to REST API only on command failure
      if DIFF_OUTPUT=$(gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --name-only 2>/dev/null); then
        TEST_FILES=$(echo "$DIFF_OUTPUT" \
          | grep -E '\.(cs|xaml)$' \
          | grep -iE '(tests?/|TestCases|UnitTests|DeviceTests)' \
          || true)
      else
        # gh pr diff fails with HTTP 406 for PRs with 300+ files; use paginated files API
        if ! API_FILES=$(gh api "repos/$GITHUB_REPOSITORY/pulls/$PR_NUMBER/files" --paginate --jq '.[].filename' 2>&1); then
          echo "❌ gh pr diff failed and REST API fallback also failed: $API_FILES"
          exit 1
        fi
        TEST_FILES=$(echo "$API_FILES" \
          | grep -E '\.(cs|xaml)$' \
          | grep -iE '(tests?/|TestCases|UnitTests|DeviceTests)' \
          || true)
      fi
      if [ -z "$TEST_FILES" ]; then
        echo "⏭️ No test source files (.cs/.xaml) found in PR diff. Nothing to evaluate."
        exit 1
      fi
      echo "✅ Found test files to evaluate:"
      echo "$TEST_FILES" | head -20

  # For slash_command triggers, the gh-aw platform's checkout_pr_branch.cjs runs
  # AFTER all user steps and overlays the PR branch onto the workspace. This means
  # fork PRs can supply their own .github/skills/ and .github/instructions/.
  # We cannot restore trusted infra here because the platform checkout runs later.
  # Mitigation: agent is sandboxed (no credentials), max 1 comment via safe-outputs,
  # and the agent prompt includes a pre-flight check that catches missing SKILL.md.
  # See: .github/instructions/gh-aw-workflows.instructions.md "The issue_comment + Fork Problem"

  # For workflow_dispatch, the platform skips checkout entirely — this step is the
  # only thing that gets the PR code onto disk and restores trusted infra from main.
  - name: Checkout PR and restore agent infrastructure
    if: github.event_name == 'workflow_dispatch'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
    run: |
      set -euo pipefail
      # workflow_dispatch is already write-gated — no fork/permission checks needed.
      gh pr checkout "$PR_NUMBER"
      # Restore trusted .github/ from base branch (defense-in-depth)
      BASE_SHA=$(gh pr view "$PR_NUMBER" --json baseRefOid --jq '.baseRefOid')
      git checkout "$BASE_SHA" -- .github/ .agents/ 2>&1 \
        && echo "✅ Restored .github/ and .agents/ from base ($BASE_SHA)" \
        || { echo "❌ Could not restore trusted infra from base"; exit 1; }
---

# Evaluate PR Tests

Invoke the **evaluate-pr-tests** skill: read and follow `.github/skills/evaluate-pr-tests/SKILL.md`.

## Context

- **Repository**: ${{ github.repository }}
- **PR Number**: ${{ github.event.issue.number || inputs.pr_number }}

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
2. Run `pwsh .github/skills/evaluate-pr-tests/scripts/Gather-TestContext.ps1 -PrNumber <number>` to gather automated context (use the PR number from the Context section above)
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
