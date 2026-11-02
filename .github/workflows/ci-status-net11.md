---
name: "CI Failure Scanner (net11.0)"
description: |
  Scan net11.0 CI failures and create or advance one validated draft fix PR.
  Findings and blocked investigations stay in run artifacts, never tracking issues.

imports:
  - shared/gpt-6.1-sol.md
  - shared/threat-detection.md
  - uses: shared/ci-scan-fix.md
    with:
      branch: net11.0
      title-prefix: "[ci-fix-net11]"
  - uses: shared/pat_pool.md
    with:
      environment: copilot-pat-pool

environment: copilot-pat-pool

permissions:
  contents: read
  issues: read
  pull-requests: read
  checks: read
  statuses: read

checkout:
  fetch-depth: 200

timeout-minutes: 90
max-ai-credits: -1
max-daily-ai-credits: -1

on:
  schedule: every 12h
  workflow_dispatch:
    inputs:
      dry_run:
        description: "Analyze and validate without creating or changing PRs"
        required: false
        type: boolean
        default: true
  permissions: {}

if: github.repository == 'dotnet/maui' && github.ref == 'refs/heads/main'

model: gpt-6.1-sol
engine:
  id: copilot
  env:
    COPILOT_PROVIDER_WIRE_API: responses
    COPILOT_GITHUB_TOKEN: ${{ case(needs.pat_pool.outputs.pat_number == '0', secrets.COPILOT_PAT_0, needs.pat_pool.outputs.pat_number == '1', secrets.COPILOT_PAT_1, needs.pat_pool.outputs.pat_number == '2', secrets.COPILOT_PAT_2, needs.pat_pool.outputs.pat_number == '3', secrets.COPILOT_PAT_3, needs.pat_pool.outputs.pat_number == '4', secrets.COPILOT_PAT_4, needs.pat_pool.outputs.pat_number == '5', secrets.COPILOT_PAT_5, needs.pat_pool.outputs.pat_number == '6', secrets.COPILOT_PAT_6, needs.pat_pool.outputs.pat_number == '7', secrets.COPILOT_PAT_7, needs.pat_pool.outputs.pat_number == '8', secrets.COPILOT_PAT_8, needs.pat_pool.outputs.pat_number == '9', secrets.COPILOT_PAT_9, 'NO COPILOT PAT AVAILABLE') }}
    DEFAULT_BRANCH: net11.0
    CI_FIX_TOOLS_DIR: ${{ runner.temp }}/gh-aw/ci-fix-tools

concurrency:
  group: ci-failure-scan-net11
  cancel-in-progress: false

safe-outputs:
  report-failure-as-issue: false
  report-failed-jobs: false
  missing-tool:
    create-issue: false
  missing-data:
    create-issue: false
  report-incomplete:
    create-issue: false

---

# Net11-branch CI fixes

Follow the shared CI scan-and-fix protocol. Inspect only `net11.0` builds and
target every fix PR at `net11.0`. Never create or reopen an issue.
