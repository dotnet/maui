---
# ###############################################################
# Select a PAT from the pool and override COPILOT_GITHUB_TOKEN.
# Run agentic jobs in an isolated `copilot-pat-pool` environment.
#
# When org-level billing is available, this will be removed.
# See `shared/pat_pool.README.md` for more information.
# ###############################################################
imports:
  - uses: shared/pat_pool.md
    with:
      environment: copilot-pat-pool

environment: copilot-pat-pool

on:
  schedule:
    - cron: "0 * * * *"
  workflow_dispatch:
    inputs:
      dry_run:
        description: "Preview reactions, label removal, and AzDO trigger without applying side effects"
        required: false
        type: boolean
        default: true
      max_prs:
        description: "Maximum queued PRs to inspect"
        required: false
        type: number
        default: 5
  steps:
    - name: Checkout repository scripts
      uses: actions/checkout@v7.0.1
      with:
        persist-credentials: false
    - name: Build rerun candidate context
      id: rerun_context
      shell: pwsh
      env:
        GH_TOKEN: ${{ github.token }}
        MAX_PRS: ${{ inputs.max_prs || '5' }}
        REPO_OWNER: ${{ github.repository_owner }}
        REPO_NAME: ${{ github.event.repository.name }}
      run: |
        $max = 5
        if ($env:MAX_PRS -match '^\d+$') {
          $max = [Math]::Max(1, [Math]::Min(20, [int]$env:MAX_PRS))
        }
        $output = "CustomAgentLogsTmp/RerunScanner/candidates.json"
        .github/scripts/Query-RerunReadyPRs.ps1 `
          -Owner $env:REPO_OWNER `
          -Repo $env:REPO_NAME `
          -MaxPRs $max `
          -OutputPath $output | Out-Null
        $json = Get-Content -Raw -LiteralPath $output
        $delimiter = "EOF_$([Guid]::NewGuid().ToString('N'))"
        "candidates<<$delimiter" >> $env:GITHUB_OUTPUT
        $json >> $env:GITHUB_OUTPUT
        $delimiter >> $env:GITHUB_OUTPUT
    - name: Upload rerun candidate context
      uses: actions/upload-artifact@v7.0.1
      with:
        name: rerun-candidates
        path: CustomAgentLogsTmp/RerunScanner/candidates.json
        if-no-files-found: error
        retention-days: 1

permissions:
  contents: read
  issues: read
  pull-requests: read

jobs:
  pre-activation:
    outputs:
      rerun_candidates: ${{ steps.rerun_context.outputs.candidates }}

concurrency:
  # Serialize scheduled and manual scanner runs so each queued PR is evaluated
  # against the latest label/head/lock state before any safe-output job can trigger.
  group: "gh-aw-${{ github.workflow }}"
  cancel-in-progress: false

model: gpt-5.6-sol
engine:
  id: copilot
  env:
    COPILOT_GITHUB_TOKEN: ${{ case(needs.pat_pool.outputs.pat_number == '0', secrets.COPILOT_PAT_0, needs.pat_pool.outputs.pat_number == '1', secrets.COPILOT_PAT_1, needs.pat_pool.outputs.pat_number == '2', secrets.COPILOT_PAT_2, needs.pat_pool.outputs.pat_number == '3', secrets.COPILOT_PAT_3, needs.pat_pool.outputs.pat_number == '4', secrets.COPILOT_PAT_4, needs.pat_pool.outputs.pat_number == '5', secrets.COPILOT_PAT_5, needs.pat_pool.outputs.pat_number == '6', secrets.COPILOT_PAT_6, needs.pat_pool.outputs.pat_number == '7', secrets.COPILOT_PAT_7, needs.pat_pool.outputs.pat_number == '8', secrets.COPILOT_PAT_8, needs.pat_pool.outputs.pat_number == '9', secrets.COPILOT_PAT_9, 'NO COPILOT PAT AVAILABLE') }}
safe-outputs:
  # gh-aw compiles this safe-output job into the `trigger_rerun_review` tool
  # called by the agent below. The hyphenated job key is converted to the
  # underscored tool name in the generated lock workflow.
  jobs:
    trigger-rerun-review:
      description: "Apply validated rerun scanner decisions. Call EXACTLY ONCE per run, passing a `decisions` JSON array with one entry per candidate PR."
      runs-on: ubuntu-latest
      output: "Rerun scanner decisions processed."
      permissions:
        # actions:write is required to dispatch the review-trigger.yml workflow.
        # No id-token here: OIDC + the AzDO trigger now live entirely in
        # review-trigger.yml (the same workflow a maintainer `/review` runs).
        contents: read
        issues: write
        pull-requests: write
        actions: write
      env:
        GH_TOKEN: ${{ github.token }}
        DRY_RUN: ${{ github.event_name == 'workflow_dispatch' && inputs.dry_run == true }}
        REPO_OWNER: ${{ github.repository_owner }}
        REPO_NAME: maui
        RERUN_ACTIONS_PATH: ${{ runner.temp }}/rerun-actions.json
      inputs:
        # A custom safe-output job is capped at one invocation per run by gh-aw,
        # which previously dropped every decision after the first and limited the
        # scanner to a single PR per run. Batching all decisions into one array
        # field lets a single invocation carry every candidate's decision.
        decisions:
          description: "JSON array of decision objects, one per candidate PR. Each object: pr_number (string), decision ('trigger'|'skip'), rerun_comment_id ('0' unless a current-cycle source is proven), expected_head_sha (string), reason (short string), and optional platform and pipeline_ref strings."
          required: true
          type: string
      steps:
        - name: Checkout repository scripts
          uses: actions/checkout@v7.0.1
          with:
            ref: main
            persist-credentials: false
        - name: Download rerun candidate context
          uses: actions/download-artifact@v8.0.1
          with:
            name: rerun-candidates
            path: ${{ runner.temp }}/rerun-candidates
        - name: Validate rerun scanner decisions
          shell: pwsh
          env:
            RERUN_CANDIDATES_PATH: ${{ runner.temp }}/rerun-candidates/candidates.json
          run: |
            .github/scripts/Invoke-RerunReviewTrigger.ps1 -DefaultPipelineRef 'main'
        - name: Dispatch review-trigger.yml for validated decisions
          # Runs even when the Validate step exits non-zero (it does so to flag a
          # PR whose decision failed validation, e.g. a stale head SHA). The valid
          # actions are already in actions.json and MUST still be dispatched — a
          # single bad decision must not discard the whole batch. No-ops when
          # actions.json is absent and still fails via core.setFailed on a real
          # dispatch error.
          if: ${{ !cancelled() }}
          # The `gh` CLI returns a spurious HTTP 404 for repos/.../pulls/N in this
          # gh-aw safe-output job context, so all GitHub writes go through octokit
          # here. We dispatch the same review-trigger.yml workflow a maintainer
          # `/review` runs; it owns PR validation, the s/agent-review-in-progress
          # lock, platform inference, OIDC, and the AzDO pipeline trigger.
          uses: actions/github-script@v9.0.0
          with:
            github-token: ${{ secrets.GITHUB_TOKEN }}
            script: |
              const fs = require('fs');
              const readyLabel = 's/agent-ready-for-rerun';
              const declinedLabel = {
                name: 's/agent-rerun-declined',
                description: 'AI rerun scanner declined the current PR state; new author activity is required',
                color: 'D4C5F9',
              };
              const dryRun = process.env.DRY_RUN === 'true';
              const actionsPath = process.env.RERUN_ACTIONS_PATH;
              const { owner, repo } = context.repo;

              if (!actionsPath || !fs.existsSync(actionsPath)) {
                core.info('No rerun actions file found; nothing to dispatch.');
                return;
              }

              let actions;
              try {
                actions = JSON.parse(fs.readFileSync(actionsPath, 'utf8') || '[]');
              } catch (e) {
                core.setFailed(`Failed to parse rerun actions file: ${e.message}`);
                return;
              }
              if (!Array.isArray(actions)) { actions = actions ? [actions] : []; }

              let hadFailure = false;

              async function react(commentId, content) {
                if (!commentId || commentId <= 0) { return; }
                if (dryRun) { core.info(`[dry-run] Would react '${content}' to comment ${commentId}`); return; }
                try {
                  await github.rest.reactions.createForIssueComment({ owner, repo, comment_id: commentId, content });
                  core.info(`Reacted '${content}' to comment ${commentId}`);
                } catch (e) {
                  core.warning(`Failed to react '${content}' to comment ${commentId}: ${e.message}`);
                }
              }

              async function removeReadyLabel(prNumber) {
                if (dryRun) { core.info(`[dry-run] Would remove ${readyLabel} from PR #${prNumber}`); return; }
                try {
                  await github.rest.issues.removeLabel({ owner, repo, issue_number: prNumber, name: readyLabel });
                  core.info(`Removed ${readyLabel} from PR #${prNumber}`);
                } catch (e) {
                  if (e.status === 404) { core.info(`${readyLabel} already absent on PR #${prNumber}`); }
                  else { throw new Error(`Failed to remove ${readyLabel} from PR #${prNumber}: ${e.message}`); }
                }
              }

              async function syncDeclinedLabel(existing) {
                if (existing.data.description !== declinedLabel.description || existing.data.color.toUpperCase() !== declinedLabel.color) {
                  await github.rest.issues.updateLabel({
                    owner, repo,
                    name: declinedLabel.name,
                    new_name: declinedLabel.name,
                    description: declinedLabel.description,
                    color: declinedLabel.color,
                  });
                }
              }

              async function ensureDeclinedLabel() {
                try {
                  const existing = await github.rest.issues.getLabel({ owner, repo, name: declinedLabel.name });
                  await syncDeclinedLabel(existing);
                } catch (e) {
                  if (e.status !== 404) { throw e; }
                  try {
                    await github.rest.issues.createLabel({ owner, repo, ...declinedLabel });
                  } catch (createError) {
                    if (createError.status !== 422) { throw createError; }
                    const existing = await github.rest.issues.getLabel({ owner, repo, name: declinedLabel.name });
                    await syncDeclinedLabel(existing);
                    core.info(`${declinedLabel.name} was created concurrently; using the existing label.`);
                  }
                }
              }

              async function getCurrentLabelNames(prNumber) {
                const issue = await github.rest.issues.get({ owner, repo, issue_number: prNumber });
                return new Set(issue.data.labels.map(
                  label => typeof label === 'string' ? label : label.name,
                ));
              }

              async function markDeclined(prNumber, headSha, activityCheckpoint, activityKey) {
                if (!Number.isSafeInteger(activityCheckpoint) || activityCheckpoint <= 0) {
                  throw new Error(`Missing valid scan-time activity checkpoint for PR #${prNumber}`);
                }
                if (!/^[0-9a-f]{64}$/.test(activityKey)) {
                  throw new Error(`Missing valid activity key for PR #${prNumber}`);
                }
                if (dryRun) { core.info(`[dry-run] Would record ${declinedLabel.name} for PR #${prNumber} at ${headSha}, checkpoint=${activityCheckpoint}, activity=${activityKey}`); return true; }
                let currentLabels = await getCurrentLabelNames(prNumber);
                if (!currentLabels.has(readyLabel)) {
                  core.info(`Skip: ${readyLabel} was consumed before decline recording for PR #${prNumber}.`);
                  return false;
                }
                if (currentLabels.has('s/agent-review-in-progress')) {
                  core.info(`Skip: PR #${prNumber} acquired s/agent-review-in-progress before decline recording.`);
                  return false;
                }
                await ensureDeclinedLabel();

                const cycleMarker = `<!-- agent-rerun-declined-cycle:${headSha}:${activityKey} -->`;
                const markerText = `<!-- agent-rerun-declined:${headSha}:${activityCheckpoint} -->\n${cycleMarker}`;
                const maxMarkerPages = 5;
                let commentsBefore = null;
                let existingMarker = null;
                let markerHistoryTruncated = false;
                for (let markerPage = 0; markerPage < maxMarkerPages && !existingMarker; markerPage += 1) {
                  const existing = await github.graphql(
                    `query($owner:String!,$repo:String!,$number:Int!,$before:String){
                      repository(owner:$owner,name:$repo){
                        pullRequest(number:$number){
                          comments(last:100,before:$before){
                            nodes{id body}
                            pageInfo{hasPreviousPage startCursor}
                          }
                        }
                      }
                    }`,
                    { owner, repo, number: prNumber, before: commentsBefore },
                  );
                  const comments = existing.repository.pullRequest.comments;
                  // The cycle key stays stable across a retry but changes when new
                  // same-head activity enters the deterministic candidate context.
                  existingMarker = comments.nodes.find(
                    comment => (comment.body || '').includes(cycleMarker),
                  );
                  if (!comments.pageInfo.hasPreviousPage) { break; }
                  if (markerPage + 1 >= maxMarkerPages) {
                    markerHistoryTruncated = true;
                    break;
                  }
                  commentsBefore = comments.pageInfo.startCursor;
                  if (!commentsBefore) { break; }
                }
                if (markerHistoryTruncated) {
                  core.info(`Decline marker was not found in the latest ${maxMarkerPages * 100} comments for PR #${prNumber}; a duplicate checkpoint is safer than an unbounded history walk.`);
                }

                // Re-read labels after the bounded marker lookup. A maintainer review can
                // consume the ready label and acquire the lock while this action is running.
                currentLabels = await getCurrentLabelNames(prNumber);
                if (!currentLabels.has(readyLabel)) {
                  core.info(`Skip: ${readyLabel} was consumed while decline history was checked for PR #${prNumber}.`);
                  return false;
                }
                if (currentLabels.has('s/agent-review-in-progress')) {
                  core.info(`Skip: PR #${prNumber} acquired s/agent-review-in-progress while decline history was checked.`);
                  return false;
                }
                const alreadyLabelled = currentLabels.has(declinedLabel.name);

                if (alreadyLabelled && existingMarker) {
                  core.info(`${declinedLabel.name} already records unchanged head ${headSha} for PR #${prNumber}`);
                  return true;
                } else if (existingMarker) {
                  core.info(`Decline marker for PR #${prNumber} at ${headSha} already exists; restoring its label only.`);
                } else {
                  const marker = await github.rest.issues.createComment({
                    owner, repo,
                    issue_number: prNumber,
                    body: `${markerText}\nAutomated rerun scanner checkpoint for declined head \`${headSha.slice(0, 12)}\`.`,
                  });
                  try {
                    await github.graphql(
                      'mutation($id:ID!){minimizeComment(input:{subjectId:$id,classifier:RESOLVED}){minimizedComment{isMinimized}}}',
                      { id: marker.data.node_id },
                    );
                  } catch (e) {
                    core.warning(`Recorded decline head for PR #${prNumber}, but could not minimize marker comment: ${e.message}`);
                  }
                }
                await github.rest.issues.addLabels({
                  owner, repo,
                  issue_number: prNumber,
                  labels: [declinedLabel.name],
                });
                core.info(`Applied ${declinedLabel.name} to PR #${prNumber}`);
                return true;
              }

              async function clearDeclined(prNumber) {
                if (dryRun) { core.info(`[dry-run] Would remove ${declinedLabel.name} from PR #${prNumber}`); return; }
                try {
                  await github.rest.issues.removeLabel({ owner, repo, issue_number: prNumber, name: declinedLabel.name });
                  core.info(`Removed ${declinedLabel.name} from PR #${prNumber}`);
                } catch (e) {
                  if (e.status === 404) {
                    core.info(`${declinedLabel.name} already absent on PR #${prNumber}`);
                  } else {
                    core.warning(`Could not remove advisory label ${declinedLabel.name} from PR #${prNumber}; continuing dispatch: ${e.message}`);
                  }
                }
              }

              for (const a of actions) {
                const prNumber = a.prNumber;
                try {
                  if (a.decision === 'trigger') {
                    if (dryRun) {
                      core.info(`[dry-run] Would dispatch review-trigger.yml for PR #${prNumber} (platform=${a.platform}, pipeline_ref=${a.pipelineRef})`);
                    } else {
                      // A previous semantic skip must not suppress recovery if this
                      // dispatch or the downstream review fails before posting a summary.
                      await clearDeclined(prNumber);
                      await github.rest.actions.createWorkflowDispatch({
                        owner, repo,
                        workflow_id: 'review-trigger.yml',
                        ref: 'main',
                        inputs: {
                          pr_number: String(prNumber),
                          platform: a.platform || '',
                          pipeline_ref: a.pipelineRef || 'main',
                        },
                      });
                      core.info(`Dispatched review-trigger.yml for PR #${prNumber} (platform=${a.platform}, pipeline_ref=${a.pipelineRef})`);
                    }
                    // review-trigger.yml owns the s/agent-review-in-progress lock and
                    // removes s/agent-ready-for-rerun when it locks+triggers — same as
                    // a maintainer /review — so the scanner does not remove it here.
                    // Use 🚀 (not 👍): this acknowledges the dispatch was *queued*, not that an
                    // AzDO run is confirmed/complete — review-trigger.yml owns the final outcome,
                    // and the s/agent-ready-for-rerun label persists so the next scan self-heals
                    // if the downstream dispatch no-ops.
                    await react(a.rerunCommentId, 'rocket');
                  } else {
                    // skip: the scanner consumes the queue label itself (review-trigger.yml
                    // is not involved). The candidate head SHA is a SCAN-TIME snapshot, so
                    // guard against a TOCTOU race: only react/remove when the PR's LIVE head
                    // still matches it. If the head advanced after the scan, the
                    // agent's skip is stale — leave the
                    // queue label so the next scan re-evaluates the new head instead of
                    // silently dropping the request, and don't 👎 the superseded comment.
                    // octokit is used here (the gh CLI 404s in this job), so the live read works.
                    let liveHeadSha = null;
                    let headReadFailed = false;
                    try {
                      const livePr = await github.rest.pulls.get({ owner, repo, pull_number: prNumber });
                      liveHeadSha = livePr.data.head.sha;
                    } catch (e) {
                      headReadFailed = true;
                      core.warning(`Skip: could not read live head for PR #${prNumber} (${e.message}); leaving ${readyLabel} in place for the next scan.`);
                    }
                    if (headReadFailed) {
                      // Conservative: never remove the label when we can't confirm the head.
                    } else if (a.headSha && liveHeadSha !== a.headSha) {
                      core.info(`Skip: PR #${prNumber} head advanced ${a.headSha} -> ${liveHeadSha} since the scan; leaving ${readyLabel} for re-evaluation.`);
                    } else {
                      // Persist the scan-time activity checkpoint before consuming
                      // the queue label. Activity that arrives after collection but
                      // before this action remains newer than the decline checkpoint.
                      const declineRecorded = await markDeclined(prNumber, liveHeadSha, a.activityCheckpoint, a.activityKey);
                      if (!declineRecorded) { continue; }
                      await react(a.rerunCommentId, '-1');
                      await removeReadyLabel(prNumber);
                    }
                  }
                } catch (e) {
                  core.error(`Failed to process PR #${prNumber}: ${e.message}`);
                  hadFailure = true;
                }
              }

              if (hadFailure) {
                core.setFailed('One or more rerun decisions failed to dispatch.');
              }

---

# Rerun Review Scanner

You are scanning queued .NET MAUI PRs that already have the label `s/agent-ready-for-rerun`. The daily PR Review Queue workflow applies this label autonomously when it detects genuinely new PR-author activity since the last AI review. The `/review rerun` comment command is intentionally unsupported; maintainers can use the ordinary `/review` command for a direct review request.

## Concurrency, locking, and duplicate prevention

The workflow-level concurrency group serializes scanner runs, including scheduled
and manual dispatches. Before applying any side effects, the
`trigger_rerun_review` safe-output job validates every decision against the
deterministic candidate set (`candidates.json`): the PR must be a recorded
candidate and its `expected_head_sha` must match the candidate `headSha`
(anti-stale / anti-hallucination). It performs NO live PR reads itself — the
`gh` CLI returns a spurious HTTP 404 for `repos/.../pulls/N` in this gh-aw
safe-output job context, so all GitHub writes go through octokit.

For a validated `trigger`, the safe-output job dispatches the **same
`review-trigger.yml` workflow that a maintainer `/review` comment runs** (via
`workflow_dispatch`). That workflow owns everything downstream: it re-validates
that the PR is open, applies the `s/agent-review-in-progress` lock (clearing a
stale one), removes `s/agent-ready-for-rerun`, infers the platform, performs the
OIDC exchange, and triggers the AzDO `maui-copilot` pipeline (which removes the
lock in its final cleanup stage). `review-trigger.yml` also has a per-PR
concurrency group and refuses to start when the in-progress lock is already
present, so a dispatched rerun can never double-trigger a review that is already
running. For a `skip`, the safe-output job applies `s/agent-rerun-declined`, reacts
`-1`, and removes the queue label itself.

Because `review-trigger.yml` consumes `s/agent-ready-for-rerun` when it
locks+triggers, a queued PR is removed from the candidate set after its first
successful trigger and is not re-picked by a later scan. Duplicates are
prevented by scanner serialization, candidate-set head-SHA revalidation,
`review-trigger.yml`'s per-PR concurrency group, and the persistent in-progress
lock — without a global concurrency group that could cancel unrelated maintainer
`/review` requests.

### Rerun volume (no separate scanner rate limit — by design)

An earlier version of this safe-output job enforced a per-PR cap (3 rerun
triggers / 24h with a 60-minute cooldown). That cap was **intentionally removed**
so the scanner path behaves exactly like a maintainer `/review`, which has no
such limit. Volume is instead bounded structurally:

1. A PR only becomes a candidate when `Resolve-RerunEligibility.ps1` finds
   genuinely *new* author activity (a new commit, a new non-command comment, or a
   head commit SHA that differs from the last reviewed SHA) since the last AI Summary /
   rerun checkpoint. The identical PR state cannot be re-queued.
2. Re-entry requires genuinely new activity each cycle. The PR Review Queue
   workflow applies the queue label only when the deterministic gate
   (`Resolve-AutonomousRerunEligibility`) finds
   new activity since the last AI Summary. When a review **completes** (the `trigger`
   path) it posts a fresh AI Summary that advances the checkpoint, so the identical
   PR state cannot re-qualify and autonomous re-entry cannot loop.

   > **Skip-path checkpoint (anti-flap):** the `trigger` path advances the checkpoint by
   > posting a fresh AI Summary, but a `skip` decision does not. Before consuming
   > `s/agent-ready-for-rerun`, the safe-output job applies the explicit
   > `s/agent-rerun-declined` marker and writes a minimized bot comment containing the
   > exact head SHA, scan-time checkpoint, and a hash of the deterministic activity
   > context that was declined. A retry of the same activity cycle reuses that marker
   > even when its new scan-time checkpoint differs; genuinely new same-head activity
   > changes the hash and records a fresh checkpoint. `Query-AutoRerunCandidates.ps1`
   > passes the declined SHA and comment timestamp to
   > `Resolve-AutonomousRerunEligibility`. Any different current head always
   > re-qualifies even if a push raced the marker write. Trigger-path and manual
   > removals of the ready label are not decline checkpoints. Every successful review
   > entrypoint clears the visible decline label.
3. The per-PR in-progress lock prevents overlapping reviews of the same PR.

This is an accepted, documented cost trade-off: it matches manual `/review`
exactly. If a hard ceiling is ever needed, the `s/agent-review-in-progress`
label history is still available to the github-script step and a lightweight
per-PR throttle can be reintroduced there.

The deterministic scanner found these fixed-schema candidates. PR-authored titles,
comments, command bodies, commit messages, and other prose are intentionally excluded
from this payload and must not be fetched or inspected:

```json
${{ needs.pre_activation.outputs.rerun_candidates }}
```

For each candidate in `candidates`:

1. Use only the fixed fields in the payload. Do not fetch or inspect PR titles, bodies,
   comments, command text, commit messages, diffs, or AI Summary content.
2. Each candidate already passed the trusted ready-label and deterministic new-activity
   gates. Use `activity.headChanged`, `activity.newAuthorCommentCount`, and
   `activity.newCommitCount` to make the decision without interpreting prose.
3. Choose exactly one decision per candidate:
   - `trigger`: the PR is not a draft and at least one fixed activity signal is non-zero/true.
   - `skip`: the PR is a draft or every fixed activity signal is zero/false.

Then call the `trigger_rerun_review` safe-output tool **exactly once for the whole run**, passing a single `decisions` argument: a JSON array string containing one object per candidate. This tool is generated from `safe-outputs.jobs.trigger-rerun-review` above. Do NOT call the tool more than once — a custom safe-output job runs once per scan, so additional calls are dropped.

Each object in the `decisions` array must use:

- `pr_number`: the candidate `prNumber`.
- `decision`: `trigger` or `skip`.
- `rerun_comment_id`: use the candidate `rerunCommentId`, currently `"0"`. Autonomous queue cycles have no command comment, so historical comments are never reused as reaction targets. A missing id is **not** a reason to skip; the dispatch does not require a comment.
- `expected_head_sha`: the candidate `headSha`.
- `platform`: the candidate `platform`.
- `pipeline_ref`: the candidate `pipelineRef`.
- `reason`: one short sentence.

Example: `decisions = "[{\"pr_number\":\"123\",\"decision\":\"trigger\",\"rerun_comment_id\":\"0\",\"expected_head_sha\":\"abc123\",\"platform\":\"android\",\"pipeline_ref\":\"main\",\"reason\":\"New commit addresses review feedback.\"}]"`

Do not call any other write tool. Do not create comments, labels, issues, or pull requests directly. The safe-output job will handle reactions, queue-label removal, and dispatching `review-trigger.yml` deterministically.
