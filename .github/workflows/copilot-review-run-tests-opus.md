---
description: "/review run-tests, second model: Claude Opus 5.5 picks UI tests from the same context as GPT-6.2 Sol; the union of both selections runs in maui-pr-uitests."

imports:
  - shared/threat-detection.md
  - uses: shared/pat_pool.md
    with:
      environment: copilot-pat-pool
  - shared/run-tests-selection.md

environment: copilot-pat-pool

# The agent works from the collected context and GitHub read tools; it never needs PR code on disk.
checkout: false

on:
  # Dispatched only by copilot-review-run-tests (GPT-6.2 Sol) once its selection is handed over.
  workflow_dispatch:
    inputs:
      pr_number:
        description: PR the selection belongs to
        type: string
        required: true
      source_run_id:
        description: The copilot-review-run-tests run whose context this model selects from
        type: string
        required: true
  # The dispatching token is the run-tests workflow's own; the source run is verified below.
  roles: all
  status-comment: false
  permissions:
    actions: read
    contents: read
  steps:
    - name: Verify the source run and download its context
      id: context
      env:
        GH_TOKEN: ${{ github.token }}
        PR_NUMBER: ${{ inputs.pr_number }}
        SOURCE_RUN_ID: ${{ inputs.source_run_id }}
        DEFAULT_BRANCH: ${{ github.event.repository.default_branch }}
      shell: pwsh
      run: |
        if ($env:GITHUB_REF -ne "refs/heads/$env:DEFAULT_BRANCH") { throw 'Runs only from the default branch.' }
        if ($env:PR_NUMBER -notmatch '^[1-9][0-9]{0,9}$' -or $env:SOURCE_RUN_ID -notmatch '^[1-9][0-9]{0,19}$') {
          throw 'pr_number and source_run_id must be positive integers.'
        }
        $run = gh api "repos/$env:GITHUB_REPOSITORY/actions/runs/$env:SOURCE_RUN_ID" | ConvertFrom-Json
        if ($LASTEXITCODE -ne 0) { throw 'Could not read the source run.' }
        if ($run.path -ne '.github/workflows/copilot-review-run-tests.lock.yml' -or
            $run.head_branch -ne $env:DEFAULT_BRANCH -or
            $run.repository.full_name -ne $env:GITHUB_REPOSITORY -or
            $run.event -notin @('issue_comment', 'workflow_dispatch')) {
          throw "Run $env:SOURCE_RUN_ID is not a /review run-tests run on $env:DEFAULT_BRANCH."
        }
        if (([DateTimeOffset]::UtcNow - [DateTimeOffset]::Parse($run.created_at)).TotalHours -gt 3) {
          throw 'The source run is too old.'
        }
        $directory = "$env:RUNNER_TEMP/run-tests"
        gh run download $env:SOURCE_RUN_ID --repo $env:GITHUB_REPOSITORY --name "run-tests-context-$env:SOURCE_RUN_ID" --dir $directory
        if ($LASTEXITCODE -ne 0) { throw 'Could not download the collected context.' }
        $context = Get-Content "$directory/context.json" -Raw | ConvertFrom-Json -Depth 30
        if ([string]$context.prNumber -ne $env:PR_NUMBER) { throw 'The context belongs to a different PR.' }
        "ready=true" >> $env:GITHUB_OUTPUT
    - name: Upload context for the agent
      if: steps.context.outputs.ready == 'true'
      uses: actions/upload-artifact@043fb46d1a93c77aae656e7c1c64a875d1fc6a0a # v7.0.1
      with:
        name: run-tests-context-${{ github.run_id }}
        path: ${{ runner.temp }}/run-tests/context.json
        retention-days: 7
        if-no-files-found: error

if: needs.pre_activation.outputs.context_ready == 'true'

permissions:
  contents: read
  pull-requests: read

model: claude-opus-5.5
engine:
  id: copilot
  env:
    COPILOT_GITHUB_TOKEN: ${{ case(needs.pat_pool.outputs.pat_number == '0', secrets.COPILOT_PAT_0, needs.pat_pool.outputs.pat_number == '1', secrets.COPILOT_PAT_1, needs.pat_pool.outputs.pat_number == '2', secrets.COPILOT_PAT_2, needs.pat_pool.outputs.pat_number == '3', secrets.COPILOT_PAT_3, needs.pat_pool.outputs.pat_number == '4', secrets.COPILOT_PAT_4, needs.pat_pool.outputs.pat_number == '5', secrets.COPILOT_PAT_5, needs.pat_pool.outputs.pat_number == '6', secrets.COPILOT_PAT_6, needs.pat_pool.outputs.pat_number == '7', secrets.COPILOT_PAT_7, needs.pat_pool.outputs.pat_number == '8', secrets.COPILOT_PAT_8, needs.pat_pool.outputs.pat_number == '9', secrets.COPILOT_PAT_9, 'NO COPILOT PAT AVAILABLE') }}

jobs:
  pre-activation:
    outputs:
      context_ready: ${{ steps.context.outputs.ready }}
  # If this model fails, hand over GPT-6.2 Sol's selection on its own. An agent can emit its
  # proposal and then exit non-zero; only hand over without one when nothing was dispatched.
  run-tests-fallback:
    needs: [pre_activation, activation, agent, run_selected_ui_tests]
    if: ${{ !cancelled() && needs.pre_activation.outputs.context_ready == 'true' && needs.agent.result == 'failure' && needs.run_selected_ui_tests.result != 'success' }}
    runs-on: ubuntu-latest
    timeout-minutes: 5
    permissions:
      actions: write
    steps:
      - name: Hand over to the dispatch workflow without this model's proposal
        env:
          GH_TOKEN: ${{ github.token }}
          PR_NUMBER: ${{ inputs.pr_number }}
          SOURCE_RUN_ID: ${{ inputs.source_run_id }}
          DEFAULT_BRANCH: ${{ github.event.repository.default_branch }}
        run: |
          gh workflow run review-run-tests-dispatch.yml --repo "$GITHUB_REPOSITORY" --ref "$DEFAULT_BRANCH" \
            -f pr_number="$PR_NUMBER" -f source_run_id="$SOURCE_RUN_ID" -f second_run_id="$GITHUB_RUN_ID"

safe-outputs:
  report-failure-as-issue: false
  report-failed-jobs: false
  missing-tool:
    create-issue: false
  noop:
    report-as-issue: false
  jobs:
    run-selected-ui-tests:
      description: >-
        Propose the platforms and UI test categories maui-pr-uitests should run for the triggering PR.
        Call exactly once. Trusted code validates the selection, combines it with the other model's and queues the run.
      runs-on: ubuntu-latest
      # Inherits the copilot-pat-pool environment; it only hands both proposals to
      # review-run-tests-dispatch.yml, which has no environment and queues Azure DevOps.
      permissions:
        actions: write
      inputs:
        platforms:
          description: 'JSON array string of platforms to run: any of "android", "ios", "maccatalyst", "windows". Use "[]" when no UI test is affected.'
          type: string
          required: true
        categories:
          description: 'JSON array string of UI test categories from the inventory. "[]" means every category on the selected platforms.'
          type: string
          required: true
        full_run:
          description: 'true to run the complete suite on every platform (ignores platforms and categories)'
          type: boolean
        summary:
          description: One sentence describing what the PR changes and what will be tested.
          type: string
          required: true
        rationale:
          description: Short markdown bullet list mapping the changed areas to the chosen platforms and categories, and why the rest is not needed.
          type: string
          required: true
      steps:
        - name: Extract the proposal
          shell: pwsh
          env:
            SOURCE_RUN_ID: ${{ github.event.inputs.source_run_id }}
          run: |
            $output = Get-Content -LiteralPath $env:GH_AW_AGENT_OUTPUT -Raw | ConvertFrom-Json -Depth 20
            $items = @($output.items | Where-Object { $_.type -eq 'run_selected_ui_tests' })
            if ($items.Count -eq 0) { throw 'The agent produced no run_selected_ui_tests proposal.' }
            # Keep only the declared inputs; the dispatch workflow validates every value again.
            $item = $items[-1]
            $proposal = [ordered]@{}
            foreach ($key in 'platforms', 'categories', 'full_run', 'summary', 'rationale') {
              if ($item.PSObject.Properties[$key]) { $proposal[$key] = $item.$key }
            }
            $proposal['model'] = 'claude-opus-5.5'
            $proposal['sourceRunId'] = $env:SOURCE_RUN_ID
            New-Item -ItemType Directory -Force "$env:RUNNER_TEMP/run-tests-proposal" | Out-Null
            $proposal | ConvertTo-Json -Depth 5 | Set-Content "$env:RUNNER_TEMP/run-tests-proposal/proposal.json" -Encoding utf8
        - uses: actions/upload-artifact@043fb46d1a93c77aae656e7c1c64a875d1fc6a0a # v7.0.1
          with:
            name: run-tests-proposal-${{ github.run_id }}
            path: ${{ runner.temp }}/run-tests-proposal/proposal.json
            retention-days: 7
            if-no-files-found: error
        - name: Hand over both selections to the dispatch workflow
          env:
            GH_TOKEN: ${{ github.token }}
            PR_NUMBER: ${{ github.event.inputs.pr_number }}
            SOURCE_RUN_ID: ${{ github.event.inputs.source_run_id }}
            DEFAULT_BRANCH: ${{ github.event.repository.default_branch }}
          run: |
            gh workflow run review-run-tests-dispatch.yml --repo "$GITHUB_REPOSITORY" --ref "$DEFAULT_BRANCH" \
              -f pr_number="$PR_NUMBER" -f source_run_id="$SOURCE_RUN_ID" -f second_run_id="$GITHUB_RUN_ID"

tools:
  github:
    toolsets: [pull_requests, repos]

network:
  allowed: [defaults, github]

concurrency:
  group: review-run-tests-opus-${{ inputs.pr_number || github.run_id }}
  cancel-in-progress: false

timeout-minutes: 20

steps:
  - uses: actions/download-artifact@3e5f45b2cfb9172054b4087a40e8e0b5a5461e7c # v8.0.1
    with:
      name: run-tests-context-${{ github.run_id }}
      path: /tmp/gh-aw/agent/run-tests
---

Follow the shared `/review run-tests` selection instructions.
