---
description: Assesses issue quality and health for backlog grooming
on:
  pull_request:
    types: [opened, synchronize, reopened, ready_for_review]
  issue_comment:
    types: [created]
  workflow_dispatch:
    inputs:
      issue_number:
        description: 'Issue number to assess'
        required: true
        type: number

if: >-
  github.event_name == 'workflow_dispatch' ||
  (github.event_name == 'pull_request' && github.event.pull_request.draft == false) ||
  (github.event_name == 'issue_comment' &&
   !github.event.issue.pull_request &&
   (startsWith(github.event.comment.body, '/assess') || startsWith(github.event.comment.body, '/groom')))

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
    footer: "> 📋 *Issue assessment by [{workflow_name}]({run_url})*"
    run-started: "📋 Assessing issue quality… [{workflow_name}]({run_url})"
    run-success: "✅ Issue assessment complete! [{workflow_name}]({run_url})"
    run-failure: "❌ Issue assessment failed. [{workflow_name}]({run_url}) {status}"

tools:
  github:
    toolsets: [default]

network: defaults

concurrency:
  group: "backlog-groom-${{ github.event.issue.number || inputs.issue_number || github.run_id }}"
  cancel-in-progress: true

timeout-minutes: 10

steps:
  - name: Gate — skip on pull_request (this workflow is for issues only)
    if: github.event_name == 'pull_request'
    run: |
      echo "⏭️ This workflow assesses issues, not PRs. Use /assess or workflow_dispatch on an issue."
      exit 1

  - name: Gate — verify issue is open
    if: github.event_name == 'workflow_dispatch'
    env:
      GH_TOKEN: ${{ github.token }}
      ISSUE_NUMBER: ${{ inputs.issue_number }}
    run: |
      STATE=$(gh issue view "$ISSUE_NUMBER" --repo "$GITHUB_REPOSITORY" --json state -q '.state')
      if [ "$STATE" != "OPEN" ]; then
        echo "⏭️ Issue #$ISSUE_NUMBER is $STATE. Skipping assessment."
        exit 1
      fi
      echo "✅ Issue #$ISSUE_NUMBER is open"

  # Fetch issue data and run assessment OUTSIDE the sandbox.
  # The gh-aw integrity filter blocks the sandboxed agent from reading
  # community-submitted issues via MCP tools. By fetching here (with
  # GITHUB_TOKEN) and saving to a file, the agent can read the results.
  - name: Run issue assessment
    env:
      GH_TOKEN: ${{ github.token }}
      ISSUE_NUMBER: ${{ github.event.issue.number || inputs.issue_number }}
    run: |
      echo "📋 Assessing issue #$ISSUE_NUMBER..."

      # Run assessment and save JSON output
      pwsh .github/skills/backlog-groom/scripts/assess-issue.ps1 \
        -IssueNumber "$ISSUE_NUMBER" \
        -OutputFormat json \
        > /tmp/assessment.json 2>/tmp/assessment-stderr.log

      # Also run summary for logs
      pwsh .github/skills/backlog-groom/scripts/assess-issue.ps1 \
        -IssueNumber "$ISSUE_NUMBER" \
        -OutputFormat summary \
        2>&1 || true

      # Copy to workspace so agent can read it
      cp /tmp/assessment.json "$GITHUB_WORKSPACE/assessment.json"
      echo "✅ Assessment saved to assessment.json"
      echo "--- JSON preview ---"
      head -30 "$GITHUB_WORKSPACE/assessment.json"
---

# Assess Issue Quality

## Context

- **Repository**: ${{ github.repository }}
- **Issue Number**: ${{ github.event.issue.number || inputs.issue_number }}

This workflow assesses the quality and health of GitHub issues.

## Pre-flight check

Verify the assessment data file exists:

```bash
test -f assessment.json
```

If the file is **missing**, post a comment using `add_comment` with `item_number` set to the issue number:

```markdown
## 📋 Issue Health Assessment

❌ **Cannot assess**: the assessment script failed to produce results. Check the workflow logs.
```

Then stop.

## Reading the assessment

The assessment has already been run by the pre-agent step. Read the JSON file:

```bash
cat assessment.json
```

**IMPORTANT: Do NOT use GitHub MCP tools (`issue_read`, `search_issues`, etc.) to read issue data.** The integrity filter blocks reading community-submitted issues. All data you need is in `assessment.json`.

Parse the JSON to extract: HealthGrade, HealthScore, ReproQuality, ReproScore, all `Has*` boolean fields, Flags, Actions, TemplateCompleteness, TemplateFieldsEmpty, TemplateFieldsMissing, and other fields.

## Posting Results

Call `add_comment` with `item_number` set to the **issue number**. Format the report as follows:

```markdown
## 📋 Issue Health Assessment

**Health Grade:** [A|B|C|D|F] ([score]/100) · **Repro Quality:** [good|fair|weak|missing] (score: [N])

### Quality Factors

| Factor | Status | Details |
|--------|--------|---------|
| Repro steps/link | ✅ or ❌ | [brief detail from ReproDetails] |
| Expected vs actual | ✅ or ❌ | |
| Platform/OS version | ✅ or ❌ | |
| SDK/MAUI version | ✅ or ❌ | |
| Regression info | ✅ or ❌ | |
| Stack trace/logs | ✅ or ❌ | |
| Frequency | ✅ or ❌ | |
| Workaround | ✅ or ❌ | |

### Flags

[List each flag with its emoji: ✅ possibly-fixed, 💀 very-stale, 😴 stale, 🔍 needs-repro, 📝 weak-repro, 📋 incomplete-template, 🏷️ no-platform/no-area, 📌 no-milestone. Or "✅ No flags — this issue looks healthy!" if empty]

### Template Completeness

[If TemplateCompleteness is "incomplete": list which fields are empty/missing. If "complete": say all fields filled. If "no-template": say issue doesn't use the bug-report template]

### Recommended Actions

[List each action from the Actions array, or "✅ No actions needed" if empty]

> 👍 / 👎 — Was this assessment helpful? React to let us know!
```

**Important:**
- Always include ALL 8 quality factors in the table
- Map JSON booleans to ✅/❌: HasReproLink or ReproScore>=5 → Repro steps, HasExpectedActual → Expected vs actual, HasPlatformVersionDetails → Platform/OS, HasDotNetVersion → SDK/MAUI, HasRegressionInfo → Regression, HasStackTrace → Stack trace, HasFrequencyInfo → Frequency, HasWorkaroundInfo → Workaround
- If health grade is A or B with no flags, keep the report brief
- If grade D or F, emphasize what's missing and how to improve
