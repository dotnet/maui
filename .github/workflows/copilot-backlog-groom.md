---
description: Assesses issue quality and health for backlog grooming
on:
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
---

# Assess Issue Quality

Invoke the **backlog-groom** skill: read and follow `.github/skills/backlog-groom/SKILL.md`.

## Context

- **Repository**: ${{ github.repository }}
- **Issue Number**: ${{ github.event.issue.number || inputs.issue_number }}

This workflow assesses the quality and health of GitHub issues — it does NOT operate on pull requests. It is triggered by:
- `/assess` or `/groom` comments on issues
- Manual dispatch with an issue number

## Pre-flight check

Before starting, verify the skill file exists:

```bash
test -f .github/skills/backlog-groom/SKILL.md
```

If the file is **missing**, post a comment using `add_comment`:

```markdown
## 📋 Issue Health Assessment

❌ **Cannot assess**: the backlog-groom skill (`.github/skills/backlog-groom/SKILL.md`) is missing.
```

Then stop — do not proceed with the assessment.

## Running the assessment

1. Determine the issue number from context:
   - `issue_comment`: use `${{ github.event.issue.number }}`
   - `workflow_dispatch`: use `${{ inputs.issue_number }}`

2. Run the assessment script for summary output (goes to stderr/display):
   ```bash
   pwsh .github/skills/backlog-groom/scripts/assess-issue.ps1 -IssueNumber <ISSUE_NUMBER> -OutputFormat summary
   ```

3. Run again for structured JSON data (goes to stdout):
   ```bash
   pwsh .github/skills/backlog-groom/scripts/assess-issue.ps1 -IssueNumber <ISSUE_NUMBER> -OutputFormat json
   ```

4. Parse the JSON to extract all quality factors, flags, and recommended actions.

## Posting Results

Call `add_comment` with `item_number` set to the **issue number**. Format the report as follows:

```markdown
## 📋 Issue Health Assessment

**Health Grade:** [A|B|C|D|F] ([score]/100) · **Repro Quality:** [good|fair|weak|missing] (score: [N])

### Quality Factors

| Factor | Status | Details |
|--------|--------|---------|
| Repro steps/link | ✅ or ❌ | [brief detail] |
| Expected vs actual | ✅ or ❌ | |
| Platform/OS version | ✅ or ❌ | |
| SDK/MAUI version | ✅ or ❌ | |
| Regression info | ✅ or ❌ | |
| Stack trace/logs | ✅ or ❌ | |
| Frequency | ✅ or ❌ | |
| Workaround | ✅ or ❌ | |

### Flags

[List each flag with its emoji from the script output]

### Template Completeness

[If using bug-report template: list which fields are complete vs empty/missing]

### Recommended Actions

[List each recommended action from the assessment, or "✅ No actions needed — this issue looks healthy!" if none]

> 👍 / 👎 — Was this assessment helpful? React to let us know!
```

**Important:**
- Always include ALL 8 quality factors in the table, even if the issue doesn't need all of them
- If the issue has a health grade of A or B with no flags, keep the report brief
- If the issue has grade D or F, emphasize what's missing and how the reporter can improve it
