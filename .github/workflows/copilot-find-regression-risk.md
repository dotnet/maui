---
description: Detects regression risks by cross-referencing PR changes against recently-merged bug-fix PRs
on:
  pull_request:
    types: [opened, synchronize, reopened, ready_for_review]
  issue_comment:
    types: [created]
  workflow_dispatch:
    inputs:
      pr_number:
        description: 'PR number to check for regression risks'
        required: true
        type: number

if: >-
  (github.event_name == 'pull_request' && github.event.pull_request.draft == false) ||
  github.event_name == 'workflow_dispatch' ||
  (github.event_name == 'issue_comment' &&
   github.event.issue.pull_request &&
   startsWith(github.event.comment.body, '/check-regression'))

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
    footer: "> 🔍 *Regression risk analysis by [{workflow_name}]({run_url})*"
    run-started: "🔍 Checking for regression risks on this PR… [{workflow_name}]({run_url})"
    run-success: "✅ Regression risk analysis complete! [{workflow_name}]({run_url})"
    run-failure: "❌ Regression risk analysis failed. [{workflow_name}]({run_url}) {status}"

tools:
  github:
    toolsets: [default]

network: defaults

concurrency:
  group: "find-regression-risk-${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: true

timeout-minutes: 15

steps:
  - name: Gate — skip if no implementation files in diff
    if: github.event_name == 'pull_request'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number }}
    run: |
      IMPL_FILES=$(gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --name-only \
        | grep -E '\.(cs|xaml)$' \
        | grep -vE '(Tests|TestCases|tests|snapshots|samples)/' \
        || true)
      if [ -z "$IMPL_FILES" ]; then
        echo "⏭️ No implementation files found in PR diff. Skipping regression risk check."
        exit 1
      fi
      echo "✅ Found implementation files to check:"
      echo "$IMPL_FILES" | head -20

  - name: Checkout PR and restore agent infrastructure
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}
    run: pwsh .github/scripts/Checkout-GhAwPr.ps1
---

# Find Regression Risk

Invoke the **find-regression-risk** skill: read and follow `.github/skills/find-regression-risk/SKILL.md`.

## Context

- **Repository**: ${{ github.repository }}
- **PR Number**: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}

The PR branch has been checked out for you. All files from the PR are available locally.

## Pre-flight check

Before starting, verify the skill file exists:

```bash
test -f .github/skills/find-regression-risk/SKILL.md
```

If the file is **missing**, post a comment using `add_comment`:

```markdown
## 🔍 Regression Risk Analysis

❌ **Cannot analyze**: this PR's branch does not include the find-regression-risk skill (`.github/skills/find-regression-risk/SKILL.md` is missing).

**Fix**: rebase your fork on the latest `main` branch, or use the **workflow_dispatch** trigger (Actions tab → "Find Regression Risk" → "Run workflow" → enter PR number).
```

Then stop — do not proceed.

## Running the skill

1. Run the regression risk script:
   ```bash
   pwsh .github/skills/find-regression-risk/scripts/Find-RegressionRisks.ps1 -PRNumber <PR_NUMBER>
   ```
2. Read the structured output and the console summary
3. Post results using `add_comment` with `item_number` set to the PR number

## Posting Results

Call `add_comment` with `item_number` set to the PR number. Format the report based on the risk level:

### If 🟢 Clean (no risks):

```markdown
## 🔍 Regression Risk Analysis

🟢 **No regression risks detected.** No recent bug-fix PRs were affected by the changes in this PR.

> 👍 / 👎 — Was this analysis helpful? React to let us know!
```

### If 🟡 Overlaps found:

```markdown
## 🔍 Regression Risk Analysis

🟡 **Overlaps found** — This PR modifies files that were recently changed by bug-fix PRs, but does not revert their fix lines.

> 👍 / 👎 — Was this analysis helpful? React to let us know!

<details>
<summary>📊 Expand Full Analysis</summary>

| File | Recent Fix PR | Fixed Issue | Risk |
|------|--------------|-------------|------|
| `{file}` | #{recent_pr} — {title} | {issues} | 🟡 Overlap |

</details>
```

### If 🔴 Revert risks detected:

```markdown
## 🔍 Regression Risk Analysis

🔴 **Revert risks detected!** This PR removes code that was specifically added by recent bug-fix PRs. The previously-fixed bugs may re-appear.

**⚠️ Action required:** Verify that the issues listed below do not re-regress before merging.

> 👍 / 👎 — Was this analysis helpful? React to let us know!

<details>
<summary>📊 Expand Full Analysis</summary>

| File | Recent Fix PR | Fixed Issue | Risk | Reverted Line |
|------|--------------|-------------|------|---------------|
| `{file}` | #{recent_pr} — {title} | {issues} | 🔴 REVERT | `{line}` |

### Recommendations

- Verify issue(s) {issues} do not re-regress with these changes
- Consider adding tests that cover the previously-fixed scenario
- If the revert is intentional, document why the old fix is no longer needed

</details>
```
