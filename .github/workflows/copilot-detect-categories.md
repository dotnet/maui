---
description: Detects UI test categories in PR diffs and posts a comment listing which test categories should run on CI pipelines
on:
  pull_request:
    types: [opened, synchronize, reopened, ready_for_review]
  issue_comment:
    types: [created]
  workflow_dispatch:
    inputs:
      pr_number:
        description: 'PR number to detect categories for'
        required: true
        type: number

if: >-
  (github.event_name == 'pull_request' && github.event.pull_request.draft == false) ||
  github.event_name == 'workflow_dispatch' ||
  (github.event_name == 'issue_comment' &&
   github.event.issue.pull_request &&
   startsWith(github.event.comment.body, '/detect-categories'))

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
    footer: "> 🏷️ *Category detection by [{workflow_name}]({run_url})*"
    run-started: "🔍 Detecting UI test categories… [{workflow_name}]({run_url})"
    run-success: "✅ Category detection complete! [{workflow_name}]({run_url})"
    run-failure: "❌ Category detection failed. [{workflow_name}]({run_url}) {status}"

tools:
  github:
    toolsets: [default]

network: defaults

concurrency:
  group: "detect-categories-${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: true

timeout-minutes: 10

steps:
  - name: Gate — skip if no UI test files in diff
    if: github.event_name == 'pull_request'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number }}
    run: |
      TEST_FILES=$(gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --name-only \
        | grep -E '\.(cs|xaml)$' \
        | grep -iE '(TestCases\.Shared\.Tests|TestCases\.HostApp|UITest|Xaml\.UnitTests)' \
        || true)
      if [ -z "$TEST_FILES" ]; then
        echo "⏭️ No UI test files found in PR diff. Skipping category detection."
        exit 1
      fi
      echo "✅ Found UI test files to analyze:"
      echo "$TEST_FILES" | head -20

  - name: Detect UI test categories from PR diff
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}
    run: |
      echo "Detecting UI test categories for PR #$PR_NUMBER..."

      DIFF=$(gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" 2>/dev/null || true)
      if [ -z "$DIFF" ]; then
        echo "NO_DIFF=true" > "$GITHUB_WORKSPACE/category-results.txt"
        echo "⚠️ Could not fetch PR diff"
        exit 0
      fi

      # Extract Category attributes from added lines (+ prefix in diff)
      CATEGORIES=$(echo "$DIFF" \
        | grep -E '^\+.*\[Category\(' \
        | grep -oE 'UITestCategories\.[A-Za-z0-9_]+' \
        | sed 's/UITestCategories\.//' \
        | sort -u \
        || true)

      # Also check for nameof(UITestCategories.X) pattern
      NAMEOF_CATEGORIES=$(echo "$DIFF" \
        | grep -E '^\+.*\[Category\(' \
        | grep -oE 'nameof\(UITestCategories\.[A-Za-z0-9_]+\)' \
        | sed 's/nameof(UITestCategories\.//;s/)//' \
        | sort -u \
        || true)

      # Also check for quoted string categories like [Category("Button")]
      QUOTED_CATEGORIES=$(echo "$DIFF" \
        | grep -E '^\+.*\[Category\(' \
        | grep -oE '\[Category\("[A-Za-z0-9_]+"\)' \
        | sed 's/\[Category("//;s/")//' \
        | sort -u \
        || true)

      # Merge all categories
      ALL_CATEGORIES=$(echo -e "${CATEGORIES}\n${NAMEOF_CATEGORIES}\n${QUOTED_CATEGORIES}" \
        | grep -v '^$' \
        | sort -u \
        || true)

      # Get list of changed test files
      CHANGED_FILES=$(gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --name-only 2>/dev/null \
        | grep -E '\.(cs|xaml)$' \
        | grep -iE '(TestCases\.Shared\.Tests|TestCases\.HostApp|UITest|Xaml\.UnitTests)' \
        || true)

      {
        echo "PR_NUMBER=$PR_NUMBER"
        if [ -z "$ALL_CATEGORIES" ]; then
          echo "NO_CATEGORIES=true"
        else
          echo "CATEGORIES=$ALL_CATEGORIES"
        fi
        echo "---CHANGED_FILES---"
        echo "$CHANGED_FILES"
      } > "$GITHUB_WORKSPACE/category-results.txt"

      if [ -n "$ALL_CATEGORIES" ]; then
        echo "✅ Detected categories:"
        echo "$ALL_CATEGORIES"
      else
        echo "ℹ️ No new Category attributes detected in diff"
      fi

  - name: Checkout PR and restore agent infrastructure
    if: github.event_name == 'workflow_dispatch'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
    run: pwsh .github/scripts/Checkout-GhAwPr.ps1
---

# Detect UI Test Categories

You are a CI assistant that detects UI test categories from PR diffs and posts a summary comment.

## Context

- **Repository**: ${{ github.repository }}
- **PR Number**: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}

## Instructions

1. Read the detection results file at `category-results.txt` in the workspace root.

2. Parse the file:
   - `PR_NUMBER=<number>` — the PR being analyzed
   - `CATEGORIES=<newline-separated list>` — detected UI test category names (if any)
   - `NO_CATEGORIES=true` — no categories were detected
   - `NO_DIFF=true` — could not fetch the diff
   - Lines after `---CHANGED_FILES---` — list of changed test file paths

3. Post a comment using `add_comment` with `item_number` set to the PR number.

## Comment Format

### When categories ARE detected

Post this comment:

```markdown
## 🏷️ UI Test Categories Detected

This PR adds or modifies UI tests in the following categories:

| Category | Pipeline Filter |
|----------|----------------|
| {CategoryName} | `Category={CategoryName}` |
| ... | ... |

**To run only these categories on CI**, use the following filter:

```
{comma-separated category list}
```

<details>
<summary>📁 Changed test files ({count})</summary>

{list of changed test file paths}

</details>

> ℹ️ Only categories from **newly added** `[Category(UITestCategories.X)]` attributes are detected.
```

### When NO categories are detected

Post this comment:

```markdown
## 🏷️ UI Test Categories Detected

No new `[Category(UITestCategories.X)]` attributes were found in the PR diff.

**All UI test categories will run** with default pipeline behavior.

<details>
<summary>📁 Changed test files ({count})</summary>

{list of changed test file paths, or "No test files changed" if empty}

</details>
```

### When diff could not be fetched

Post this comment:

```markdown
## 🏷️ UI Test Categories Detected

⚠️ Could not fetch the PR diff. Category detection was skipped.

**All UI test categories will run** with default pipeline behavior.
```
