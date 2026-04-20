---
name: "Expert Code Review"
description: "Runs the expert-reviewer agent on pull requests on-demand via /expert-review."

on:
  slash_command:
    name: expert-review
    events: [pull_request_comment]
  workflow_dispatch:
    inputs:
      pr_number:
        description: 'PR number to review'
        required: true
        type: number
  roles: [admin, maintainer, write]

if: >-
  github.event_name == 'issue_comment' ||
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
    - dotnet

imports:
  - shared/review-shared.md

concurrency:
  group: "expert-review-${{ github.event.issue.number || github.event.pull_request.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: false

steps:
  - name: Checkout PR and restore agent infrastructure
    if: github.event_name == 'workflow_dispatch'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
    run: pwsh .github/scripts/Checkout-GhAwPr.ps1

timeout-minutes: 90
---

<!-- Orchestration instructions are in shared/review-shared.md -->
