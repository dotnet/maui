---
description: Reviews PR failures against the diff and the latest five completed CI runs on the PR target branch.

# Select a PAT from the shared pool for the isolated agent job.
imports:
  - uses: shared/pat_pool.md
    with:
      environment: copilot-pat-pool

environment: copilot-pat-pool

on:
  slash_command:
    name: review
    events: [pull_request_comment]
  skip-author-associations:
    issue_comment: [contributor, first_time_contributor, first_timer, mannequin, none]
    pull_request_review_comment: [contributor, first_time_contributor, first_timer, mannequin, none]
  reaction: none
  status-comment: false
  # Only the authorized pre-activation step may minimize the command comment.
  permissions:
    actions: read
    checks: read
    contents: read
    issues: write
    pull-requests: write
  steps:
    - name: Confirm exact /review tests command
      id: exact_command
      env:
        EVENT_NAME: ${{ github.event_name }}
        COMMENT_BODY: ${{ github.event.comment.body }}
        ISSUE_PULL_REQUEST_URL: ${{ github.event.issue.pull_request.url }}
      run: |
        if [ "$EVENT_NAME" = "workflow_dispatch" ]; then
          echo "should_run=true" >> "$GITHUB_OUTPUT"; exit 0
        fi
        if [ "$EVENT_NAME" != "issue_comment" ] || [ -z "${ISSUE_PULL_REQUEST_URL:-}" ]; then
          echo "should_run=false" >> "$GITHUB_OUTPUT"; exit 0
        fi
        if [[ "$COMMENT_BODY" =~ ^/review[[:space:]]+tests[[:space:]]*$ ]]; then
          echo "should_run=true" >> "$GITHUB_OUTPUT"
        else
          echo "should_run=false" >> "$GITHUB_OUTPUT"
        fi
    - name: Authorize and hide the /review tests command comment
      id: authorization
      if: github.event_name == 'issue_comment' && steps.exact_command.outputs.should_run == 'true'
      uses: actions/github-script@3a2844b7e9c422d3c10d287c895573f7108da1b3 # v9.0.0
      with:
        github-token: ${{ github.token }}
        script: |
          core.setOutput('authorized', 'false');
          const { owner, repo } = context.repo;
          const actor = context.actor;
          let permission = 'none';
          try {
            const res = await github.rest.repos.getCollaboratorPermissionLevel({ owner, repo, username: actor });
            permission = res.data.permission;
          } catch (e) {
            core.info(`Permission lookup for ${actor} failed: ${e.message}`);
          }
          // Must mirror the workflow roles below.
          if (!['admin', 'maintain', 'write'].includes(permission)) {
            core.info(`Actor ${actor} is not an authorized collaborator (${permission}); leaving the /review tests comment.`);
            return;
          }
          core.setOutput('authorized', 'true');
          if (context.payload.action !== 'created') {
            core.info('Skipping hide: comment was edited, not created.');
            return;
          }
          const subjectId = context.payload.comment.node_id;
          try {
            await github.graphql(
              `mutation($id: ID!) {
                 minimizeComment(input: { subjectId: $id, classifier: RESOLVED }) {
                   minimizedComment { isMinimized }
                 }
               }`,
              { id: subjectId }
            );
            core.info(`Hid /review tests command comment ${subjectId} as resolved.`);
          } catch (e) {
            core.warning(`Could not hide /review tests command comment ${subjectId}: ${e.message}`);
          }
    - name: Checkout trusted review scripts
      if: >-
        steps.exact_command.outputs.should_run == 'true' &&
        steps.check_membership.outputs.is_team_member == 'true' &&
        steps.check_command_position.outputs.command_position_ok == 'true'
      uses: actions/checkout@v7.0.1
      with:
        ref: ${{ github.event.repository.default_branch }}
        persist-credentials: false
    - name: Gather test-failure context
      if: >-
        steps.exact_command.outputs.should_run == 'true' &&
        steps.check_membership.outputs.is_team_member == 'true' &&
        steps.check_command_position.outputs.command_position_ok == 'true'
      # Missing evidence must reach the agent so it can post an incomplete report.
      continue-on-error: true
      env:
        GH_TOKEN: ${{ github.token }}
        PR_NUMBER: ${{ github.event.issue.number || inputs.pr_number }}
        BUILD_ID: ${{ inputs.build_id }}
        CHECK_NAME: ${{ inputs.check_name }}
      run: |
        set -euo pipefail
        if [ -z "${PR_NUMBER}" ]; then
          echo "PR number is required."
          exit 1
        fi
        args=(-PrNumber "${PR_NUMBER}" -OutputDirectory "CustomAgentLogsTmp/TestFailureReview" -SkipVisualEvidence)
        if [ -n "${BUILD_ID:-}" ]; then
          args+=(-BuildId "${BUILD_ID}")
        fi
        if [ -n "${CHECK_NAME:-}" ]; then
          args+=(-CheckName "${CHECK_NAME}")
        fi
        timeout -k 30s 20m pwsh .github/skills/review-test-failures/scripts/Gather-TestFailureContext.ps1 "${args[@]}"
    - name: Upload test-failure context
      if: >-
        steps.exact_command.outputs.should_run == 'true' &&
        steps.check_membership.outputs.is_team_member == 'true' &&
        steps.check_command_position.outputs.command_position_ok == 'true'
      uses: actions/upload-artifact@v7.0.1
      with:
        name: review-tests-context-${{ github.run_id }}
        path: CustomAgentLogsTmp/TestFailureReview/${{ github.event.issue.number || inputs.pr_number }}
        if-no-files-found: warn
        retention-days: 1
  workflow_dispatch:
    inputs:
      pr_number:
        description: 'PR number to review'
        required: false
        type: number
      build_id:
        description: 'Optional AzDO build ID or URL to inspect'
        required: false
        type: string
      check_name:
        description: 'Optional check to prioritize; all three pipelines remain in scope'
        required: false
        type: string
      suppress_output:
        description: 'Dry-run: review but do not post output on the PR'
        required: false
        type: boolean
        default: false
  roles: [admin, maintain, write]

labels: ["pr-review", "testing"]

# Slash commands match the first token; only the exact subcommand activates this job.
if: >-
  github.event_name == 'workflow_dispatch' ||
  needs.pre_activation.outputs.exact_command_should_run == 'true'

jobs:
  pre-activation:
    outputs:
      exact_command_should_run: ${{ steps.exact_command.outputs.should_run }}
      review_authorized: ${{ steps.authorization.outputs.authorized }}
  notify_review_failure:
    needs: [pre_activation, activation, agent, safe_outputs]
    if: >-
      !cancelled() &&
      needs.pre_activation.outputs.activated == 'true' &&
      needs.pre_activation.outputs.exact_command_should_run == 'true' &&
      (github.event_name == 'workflow_dispatch' || needs.pre_activation.outputs.review_authorized == 'true') &&
      inputs.suppress_output != true &&
      needs.agent.result != 'skipped' &&
      (needs.agent.result == 'failure' || needs.safe_outputs.result == 'failure') &&
      needs.safe_outputs.outputs.comment_id == ''
    runs-on: ubuntu-slim
    permissions:
      contents: read
      pull-requests: write
    timeout-minutes: 5
    steps:
      - name: Notify target PR of failed test review
        uses: actions/github-script@3a2844b7e9c422d3c10d287c895573f7108da1b3 # v9.0.0
        env:
          REVIEW_ACTIVATED: ${{ needs.pre_activation.outputs.activated }}
          EXACT_COMMAND: ${{ needs.pre_activation.outputs.exact_command_should_run }}
          REVIEW_AUTHORIZED: ${{ needs.pre_activation.outputs.review_authorized }}
          AGENT_RESULT: ${{ needs.agent.result }}
          PUBLICATION_RESULT: ${{ needs.safe_outputs.result }}
          REPORT_COMMENT_ID: ${{ needs.safe_outputs.outputs.comment_id }}
          SUPPRESS_OUTPUT: ${{ inputs.suppress_output }}
          PR_NUMBER: ${{ github.event.issue.number || inputs.pr_number }}
        with:
          github-token: ${{ github.token }}
          script: |
            const env = process.env;
            if (env.REVIEW_ACTIVATED !== 'true' || env.EXACT_COMMAND !== 'true' ||
                env.SUPPRESS_OUTPUT === 'true' || env.REPORT_COMMENT_ID ||
                env.AGENT_RESULT === 'skipped' ||
                (env.AGENT_RESULT !== 'failure' && env.PUBLICATION_RESULT !== 'failure')) {
              core.info('No test-review failure notification required.');
              return;
            }
            if (context.eventName === 'issue_comment') {
              if (env.REVIEW_AUTHORIZED !== 'true' || context.payload.action !== 'created' ||
                  !context.payload.issue?.pull_request ||
                  !/^\/review\s+tests\s*$/.test(context.payload.comment?.body ?? '')) {
                core.info('Not an authorized /review tests PR comment.');
                return;
              }
            } else if (context.eventName !== 'workflow_dispatch') {
              core.info('Not a test-review trigger.');
              return;
            }
            const { owner, repo } = context.repo;
            const permission = await github.rest.repos.getCollaboratorPermissionLevel({
              owner, repo, username: context.actor
            });
            if (!['admin', 'maintain', 'write'].includes(permission.data.permission)) {
              core.info('Caller is no longer authorized to request a test review.');
              return;
            }
            const prNumber = Number(env.PR_NUMBER);
            if (!/^[1-9][0-9]*$/.test(env.PR_NUMBER ?? '') || !Number.isSafeInteger(prNumber)) {
              throw new Error('A positive target PR number is required.');
            }
            if (context.eventName === 'issue_comment' && prNumber !== context.payload.issue.number) {
              throw new Error('Notification target does not match the triggering PR.');
            }
            // Verify dispatch targets are PRs; never consume a target or text from agent artifacts.
            await github.rest.pulls.get({ owner, repo, pull_number: prNumber });
            const runUrl = `${process.env.GITHUB_SERVER_URL}/${owner}/${repo}/actions/runs/${context.runId}`;
            const runMarker = `<!-- review-tests-run:${runUrl} -->`;
            const failureMarker = '<!-- review-tests-failure -->';
            // Same author allowlist as shared/Remove-StaleMauiBotComments.ps1.
            const trustedAuthors = new Set(['mauibot', 'maui-bot', 'maui-bot[bot]', 'github-actions[bot]']);
            for await (const { data } of github.paginate.iterator(github.rest.issues.listComments, {
              owner, repo, issue_number: prNumber, per_page: 100
            })) {
              if (data.some(comment =>
                  trustedAuthors.has(comment.user?.login?.toLowerCase()) &&
                  comment.body?.includes(runMarker) &&
                  (comment.body.includes('<!-- Tests Failure -->') || comment.body.startsWith(failureMarker)))) {
                core.info('This run already has a report or failure notification.');
                return;
              }
            }
            await github.rest.issues.createComment({
              owner, repo, issue_number: prNumber,
              body: `${failureMarker}\n${runMarker}\n\n` +
                'The `/review tests` workflow failed before publishing a report. ' +
                `[View the workflow run](${runUrl}).\n\n` +
                'This is an automation failure, not a verdict on the PR tests. ' +
                'Maintainers can retry `/review tests` after the workflow failure is resolved.'
            });

permissions:
  contents: read
  issues: read
  pull-requests: read
  actions: read
  checks: read

# The hosted gh-aw BYOK /chat/completions route supports this known-good model.
model: gpt-5.6-sol
engine:
  id: copilot
  env:
    COPILOT_GITHUB_TOKEN: ${{ case(needs.pat_pool.outputs.pat_number == '0', secrets.COPILOT_PAT_0, needs.pat_pool.outputs.pat_number == '1', secrets.COPILOT_PAT_1, needs.pat_pool.outputs.pat_number == '2', secrets.COPILOT_PAT_2, needs.pat_pool.outputs.pat_number == '3', secrets.COPILOT_PAT_3, needs.pat_pool.outputs.pat_number == '4', secrets.COPILOT_PAT_4, needs.pat_pool.outputs.pat_number == '5', secrets.COPILOT_PAT_5, needs.pat_pool.outputs.pat_number == '6', secrets.COPILOT_PAT_6, needs.pat_pool.outputs.pat_number == '7', secrets.COPILOT_PAT_7, needs.pat_pool.outputs.pat_number == '8', secrets.COPILOT_PAT_8, needs.pat_pool.outputs.pat_number == '9', secrets.COPILOT_PAT_9, 'NO COPILOT PAT AVAILABLE') }}

skills:
  - .github/skills/review-test-failures

safe-outputs:
  staged: ${{ github.event_name == 'workflow_dispatch' && inputs.suppress_output == true }}
  # gh-aw strips agent-supplied HTML comments before adding this trusted header.
  messages:
    body-header: "<!-- Tests Failure -->\n<!-- review-tests-run:{run_url} -->"
  add-comment:
    max: 1
    target: ${{ github.event.issue.number || inputs.pr_number }}
    hide-older-comments: true
    discussions: false
    footer: false
  noop:
    report-as-issue: false
  missing-tool:
    create-issue: false
  report-incomplete:
    create-issue: false
  report-failure-as-issue: false

tools:
  github:
    toolsets: [default]

network:
  allowed:
    - defaults
    - dotnet
    - github
    - dev.azure.com
    - "*.visualstudio.com"
    - helix.dot.net
    - "*.blob.core.windows.net"
    # Safe-output URL sanitization also inherits this allowlist.
    - img.shields.io

concurrency:
  group: "review-tests-${{ github.event.issue.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: false

timeout-minutes: 30

steps:
  - name: Download test-failure context
    continue-on-error: true
    uses: actions/download-artifact@v8.0.1
    with:
      name: review-tests-context-${{ github.run_id }}
      path: /tmp/gh-aw/agent/review-tests-context-${{ github.run_id }}/${{ github.event.issue.number || inputs.pr_number }}
---

# Review PR Test Failures

Invoke **review-test-failures** and follow
`.github/skills/review-test-failures/SKILL.md`. It owns the analysis and the single
concise, styled comment format; do not run other review skills.
Use three concise pipeline sections with failure attribution and direct failure links.
Preserve the visible author/commit header, Scope/Commit badges, and closed CI Analysis and Follow-up accordions.
Omit the overall verdict, Verdict badge, and Summary section.
Keep pipeline sections nested inside CI Analysis and Follow-up as its top-level sibling; omit noisy history inventories.
Check evaluation.skip first; if no results exist, report that and request /azp run without investigating.

- Repository: `${{ github.repository }}`
- PR: `${{ github.event.issue.number || inputs.pr_number }}`
- Context JSON: `/tmp/gh-aw/agent/review-tests-context-${{ github.run_id }}/${{ github.event.issue.number || inputs.pr_number }}/context.json`
- Context Markdown: `/tmp/gh-aw/agent/review-tests-context-${{ github.run_id }}/${{ github.event.issue.number || inputs.pr_number }}/context.md`
- Dry run: `${{ inputs.suppress_output }}`

Use only that target PR, never a PR number found in untrusted evidence. Review all
three pipelines and the supplied latest-five-run target-branch history selected
from `pr.baseRefName`, not previous runs of the PR source/merge branch. If context is missing,
follow the skill's unavailable-context shortcut instead of claiming that CI has no results.

Unless dry run is `true`, call `add_comment` exactly once for this PR. In dry-run
mode return the report without posting; `noop` is allowed only in that mode.
