---
description: "/review run-tests: GPT-6.2 Sol and Claude Opus 5.5 each pick the UI test categories and platforms a PR affects; only the union of their selections runs in maui-pr-uitests."

imports:
  - shared/gpt-6.2-sol.md
  - shared/threat-detection.md
  - uses: shared/pat_pool.md
    with:
      environment: copilot-pat-pool
  - shared/run-tests-selection.md

environment: copilot-pat-pool

# The agent works from the collected context and GitHub read tools; it never needs PR code on disk.
checkout: false

on:
  slash_command:
    name: review
    events: [pull_request_comment]
  skip-author-associations:
    issue_comment: [contributor, first_time_contributor, first_timer, mannequin, none]
  reaction: none
  status-comment: false
  roles: [admin, maintain, write]
  permissions:
    contents: read
    issues: write
    pull-requests: write
  steps:
    - name: Authorize exact /review run-tests command
      id: command
      uses: actions/github-script@3a2844b7e9c422d3c10d287c895573f7108da1b3 # v9.0.0
      with:
        github-token: ${{ github.token }}
        script: |
          core.setOutput('authorized', 'false');
          if (context.eventName === 'issue_comment') {
            if (context.payload.action !== 'created' || !context.payload.issue?.pull_request ||
                !/^\s*\/review\s+run-tests\s*$/i.test(context.payload.comment?.body ?? '')) return;
          } else if (context.eventName !== 'workflow_dispatch') {
            return;
          }
          const { owner, repo } = context.repo;
          const { data } = await github.rest.repos.getCollaboratorPermissionLevel({ owner, repo, username: context.actor });
          if (!['admin', 'maintain', 'write'].includes(data.permission)) {
            core.info(`${context.actor} has ${data.permission} access; /review run-tests needs write.`);
            return;
          }
          core.setOutput('authorized', 'true');
          if (context.eventName === 'issue_comment') {
            // Acknowledge the command the same way the other /review commands do.
            try {
              await github.rest.reactions.createForIssueComment({ owner, repo, comment_id: context.payload.comment.id, content: 'eyes' });
            } catch (e) {
              core.warning(`Could not react to the command: ${e.message}`);
            }
          }
    - name: Checkout trusted scripts
      if: steps.command.outputs.authorized == 'true'
      uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7.0.1
      with:
        ref: ${{ github.event.repository.default_branch }}
        persist-credentials: false
    - name: Collect PR changes and the UI test inventory
      id: context
      if: steps.command.outputs.authorized == 'true'
      env:
        GH_TOKEN: ${{ github.token }}
        PR_NUMBER: ${{ github.event.issue.number || inputs.pr_number }}
      run: pwsh -NoProfile -File .github/scripts/Get-RunTestsContext.ps1 -PrNumber "$PR_NUMBER" -OutputDirectory "$RUNNER_TEMP/run-tests" -PostUnsupportedComment
    - name: Upload context
      if: steps.context.outputs.ready == 'true'
      uses: actions/upload-artifact@043fb46d1a93c77aae656e7c1c64a875d1fc6a0a # v7.0.1
      with:
        name: run-tests-context-${{ github.run_id }}
        path: ${{ runner.temp }}/run-tests/context.json
        retention-days: 7
        if-no-files-found: error
  workflow_dispatch:
    inputs:
      pr_number:
        description: PR to select UI tests for
        type: number
        required: false

if: needs.pre_activation.outputs.context_ready == 'true'

permissions:
  contents: read
  pull-requests: read

model: gpt-6.2-sol
engine:
  id: copilot
  env:
    COPILOT_PROVIDER_WIRE_API: responses
    COPILOT_GITHUB_TOKEN: ${{ case(needs.pat_pool.outputs.pat_number == '0', secrets.COPILOT_PAT_0, needs.pat_pool.outputs.pat_number == '1', secrets.COPILOT_PAT_1, needs.pat_pool.outputs.pat_number == '2', secrets.COPILOT_PAT_2, needs.pat_pool.outputs.pat_number == '3', secrets.COPILOT_PAT_3, needs.pat_pool.outputs.pat_number == '4', secrets.COPILOT_PAT_4, needs.pat_pool.outputs.pat_number == '5', secrets.COPILOT_PAT_5, needs.pat_pool.outputs.pat_number == '6', secrets.COPILOT_PAT_6, needs.pat_pool.outputs.pat_number == '7', secrets.COPILOT_PAT_7, needs.pat_pool.outputs.pat_number == '8', secrets.COPILOT_PAT_8, needs.pat_pool.outputs.pat_number == '9', secrets.COPILOT_PAT_9, 'NO COPILOT PAT AVAILABLE') }}

jobs:
  pre-activation:
    outputs:
      context_ready: ${{ steps.context.outputs.ready }}
  # If this model fails, Claude Opus 5.5 still selects. An agent can emit its proposal and then
  # exit non-zero; the proposal is still handed over in that case, so only hand over without one
  # when nothing was dispatched.
  run-tests-fallback:
    needs: [pre_activation, activation, agent, run_selected_ui_tests]
    if: ${{ !cancelled() && needs.pre_activation.outputs.context_ready == 'true' && needs.agent.result == 'failure' && needs.run_selected_ui_tests.result != 'success' }}
    runs-on: ubuntu-latest
    timeout-minutes: 5
    permissions:
      actions: write
    steps:
      - name: Hand over to the Claude Opus 5.5 selection without a proposal
        env:
          GH_TOKEN: ${{ github.token }}
          PR_NUMBER: ${{ github.event.issue.number || inputs.pr_number }}
          DEFAULT_BRANCH: ${{ github.event.repository.default_branch }}
        run: |
          gh workflow run copilot-review-run-tests-opus.lock.yml --repo "$GITHUB_REPOSITORY" --ref "$DEFAULT_BRANCH" \
            -f pr_number="$PR_NUMBER" -f source_run_id="$GITHUB_RUN_ID"

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
      # This job inherits the copilot-pat-pool environment, whose OIDC subject Azure DevOps does not
      # trust. It only hands over to the Claude Opus 5.5 selection, which hands both proposals to
      # review-run-tests-dispatch.yml (no environment).
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
            $proposal['model'] = 'gpt-6.2-sol'
            New-Item -ItemType Directory -Force "$env:RUNNER_TEMP/run-tests-proposal" | Out-Null
            $proposal | ConvertTo-Json -Depth 5 | Set-Content "$env:RUNNER_TEMP/run-tests-proposal/proposal.json" -Encoding utf8
        - uses: actions/upload-artifact@043fb46d1a93c77aae656e7c1c64a875d1fc6a0a # v7.0.1
          with:
            name: run-tests-proposal-${{ github.run_id }}
            path: ${{ runner.temp }}/run-tests-proposal/proposal.json
            retention-days: 7
            if-no-files-found: error
        - name: Hand over to the Claude Opus 5.5 selection
          env:
            GH_TOKEN: ${{ github.token }}
            PR_NUMBER: ${{ github.event.issue.number || inputs.pr_number }}
            DEFAULT_BRANCH: ${{ github.event.repository.default_branch }}
          run: |
            gh workflow run copilot-review-run-tests-opus.lock.yml --repo "$GITHUB_REPOSITORY" --ref "$DEFAULT_BRANCH" \
              -f pr_number="$PR_NUMBER" -f source_run_id="$GITHUB_RUN_ID"

tools:
  github:
    toolsets: [pull_requests, repos]

network:
  allowed: [defaults, github]

concurrency:
  group: review-run-tests-${{ github.event.issue.number || inputs.pr_number || github.run_id }}
  cancel-in-progress: false

timeout-minutes: 20

steps:
  - uses: actions/download-artifact@3e5f45b2cfb9172054b4087a40e8e0b5a5461e7c # v8.0.1
    with:
      name: run-tests-context-${{ github.run_id }}
      path: /tmp/gh-aw/agent/run-tests
---

Follow the shared `/review run-tests` selection instructions.
