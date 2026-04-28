---
name: "Expert Code Review"
description: "Runs expert code review on pull requests on-demand via /review."

on:
  slash_command:
    name: review
    events: [pull_request_comment]
  workflow_dispatch:
    inputs:
      pr_number:
        description: 'PR number to review'
        required: true
        type: number
  status-comment: true
  roles: [admin, maintainer, write]
  bots:
    - "copilot-swe-agent[bot]"

concurrency:
  group: "expert-review-${{ github.event.issue.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: false

# slash_command compiles to issue_comment; workflow_dispatch is always allowed.
if: >-
  (github.event_name == 'issue_comment' && github.event.issue.pull_request) ||
  github.event_name == 'workflow_dispatch'

permissions:
  contents: read
  pull-requests: read

engine:
  id: copilot
  model: claude-opus-4.6

network:
  allowed:
    - defaults

imports:
  - shared/review-shared.md

timeout-minutes: 90
---

<!-- Orchestration instructions are in shared/review-shared.md -->
