---
description: Detects potential regressions in PRs by checking if deleted lines were added by prior bug fixes
on:
  pull_request:
    types: [opened, synchronize, reopened, ready_for_review]
    forks: ["*"]
  issue_comment:
    types: [created]
  workflow_dispatch:
    inputs:
      pr_number:
        description: 'PR number to check for regressions'
        required: true
        type: number

if: >-
  (github.event_name == 'pull_request' && github.event.pull_request.draft == false) ||
  github.event_name == 'workflow_dispatch' ||
  (github.event_name == 'issue_comment' &&
   github.event.issue.pull_request &&
   startsWith(github.event.comment.body, '/check-regressions'))

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
    footer: "> 🔍 *Regression check by [{workflow_name}]({run_url})*"
    run-started: "🔍 Checking for prior fix regressions on this PR… [{workflow_name}]({run_url})"
    run-success: "✅ Regression check complete! [{workflow_name}]({run_url})"
    run-failure: "❌ Regression check failed. [{workflow_name}]({run_url}) {status}"

tools:
  github:
    toolsets: [default]

network: defaults

concurrency:
  group: "regression-check-${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: true

timeout-minutes: 15

steps:
  - name: Gate — skip if no implementation source files in diff
    if: github.event_name == 'pull_request'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number }}
    run: |
      IMPL_FILES=$(gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --name-only \
        | grep -E '\.(cs|xaml)$' \
        | grep -v -iE '(tests?/|TestCases|UnitTests|DeviceTests|\.Tests\.)' \
        || true)
      if [ -z "$IMPL_FILES" ]; then
        echo "⏭️ No implementation source files (.cs/.xaml) changed in PR diff. Skipping regression check."
        exit 1
      fi
      echo "✅ Found implementation files to check for regressions:"
      echo "$IMPL_FILES" | head -20

  - name: Checkout PR and restore agent infrastructure
    if: github.event_name == 'workflow_dispatch'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
    run: pwsh .github/scripts/Checkout-GhAwPr.ps1
---

# Prior Fix Regression Check

Detect whether this PR inadvertently removes lines that were added by prior bug fixes.

## Context

- **Repository**: ${{ github.repository }}
- **PR Number**: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}

The PR branch has been checked out for you. All files from the PR are available locally.

## Pre-flight check

Before starting, verify the regression detection script exists:

```bash
test -f .github/skills/pr-finalize/scripts/Detect-Regressions.ps1
```

If the file is **missing**, the fork PR branch is likely not rebased on the latest `main`. Post a comment using `add_comment`:

```markdown
## 🔍 Prior Fix Regression Check

❌ **Cannot check for regressions**: this PR's branch does not include the regression detection script (`.github/skills/pr-finalize/scripts/Detect-Regressions.ps1` is missing).

**Fix**: rebase your fork on the latest `main` branch, or use the **workflow_dispatch** trigger (Actions tab → "Regression Check" → "Run workflow" → enter PR number) which handles this automatically.
```

Then stop — do not proceed with the check.

## Running the regression check

1. Use `gh pr view <number>` to fetch PR metadata (title, body, changed files). If `gh` CLI is unavailable, use the GitHub MCP tools instead.
2. Run the regression detection script:
   ```bash
   pwsh .github/skills/pr-finalize/scripts/Detect-Regressions.ps1 -PRNumber <number>
   ```
3. Read the report at `CustomAgentLogsTmp/RegressionCheck/report.md`
4. Analyze the results: if regressions were found, review each one to assess severity and add context about why the removal is risky
5. Post results using `add_comment` with `item_number` set to the PR number

## Analyzing results

When interpreting the script output:

### If script exits 0 (no regressions found)
Report a clean result — no prior fix lines were removed.

### If script exits 1 (regressions found)
For each flagged regression:
1. Look up the referenced issue number to understand the original bug
2. Read the PR description to see if the author already addressed the removal
3. Check if the PR introduces alternative protection for the original bug
4. Classify severity:
   - **🔴 Critical**: Removed line was a guard/check fixing a specific bug, PR doesn't explain removal
   - **🟡 Warning**: Removed line references an issue, but PR description explains why removal is safe
   - **✅ Acknowledged**: Author explicitly documented the removal with justification

## Posting Results

Call `add_comment` with `item_number` set to the PR number. Use this format:

### When regressions are found:

```markdown
## 🔍 Prior Fix Regression Check

**Result:** 🔴 **Potential regression(s) detected**

[1-2 sentence summary of what was found]

> ⚠️ This PR removes lines that were added to fix prior bugs. Please review the findings below and confirm each removal is intentional.

<details>
<summary>📊 Expand Full Report</summary>

[Full regression report from the script output, enhanced with issue context]

### Required Actions

For each flagged item, the PR author should:
1. Confirm the removal is intentional
2. Explain how the original bug is still prevented
3. Add a comment in the PR description under "Removed Lines (with justification)"

</details>
```

### When no regressions are found:

```markdown
## 🔍 Prior Fix Regression Check

**Result:** ✅ **No prior fix regressions detected**

Checked deleted lines across implementation files. No lines were identified as reversions of prior bug fixes.

> 👍 / 👎 — Was this check helpful? React to let us know!
```
