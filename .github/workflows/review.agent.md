---
name: "Expert Code Review"
description: "Runs expert code review on pull requests on-demand via /review."

on:
  slash_command:
    name: review
    events: [pull_request_comment]
  roles: [admin, maintainer, write]

# slash_command compiles to issue_comment; only proceed for PR comments.
if: >-
  github.event_name == 'issue_comment' && github.event.issue.pull_request

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

timeout-minutes: 90
---

<!-- Orchestration instructions are in shared/review-shared.md -->
