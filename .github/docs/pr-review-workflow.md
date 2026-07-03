# .NET MAUI automated PR review workflow

This guide explains the automated review commands used in dotnet/maui pull requests:

- `/review`
- `/review rerun`
- `/review tests`

It is intended for Microsoft maintainers and community contributors who want to understand when to request an automated review, what the automation does, and how to interpret the resulting comments.

## Quick command reference

| Command | Who can run it | What it does | Output |
| --- | --- | --- | --- |
| `/review` | Repository users with write, maintain, or admin access | Queues the full MAUI Copilot PR review pipeline. | Updates the PR with an `AI Summary` comment. |
| `/review <platform>` | Repository users with write, maintain, or admin access | Queues the full review pipeline for a specific platform: `android`, `ios`, `catalyst`, or `windows`. | Updates the PR with an `AI Summary` comment. |
| `/review rerun` | Repository users with write, maintain, or admin access; or non-first-time PR authors on their own PR | Requests a fresh full review after comments, commits, or CI context changed. Applies the `s/agent-ready-for-rerun` label; the hourly scanner then triggers the review pipeline. | Adds or replaces a review session in the `AI Summary` comment. |
| `/review tests` | Repository users with write, maintain, or admin access | Reviews current CI/test failures and classifies whether they are likely PR-caused, unrelated, or insufficiently evidenced. | Adds or updates a `Test Failure Review` comment. |

Community contributors can trigger `/review rerun` on their own PR after their first contribution. For `/review` and `/review tests`, only repository users with write access can trigger these commands. If you are a first-time contributor, ask a maintainer to run the relevant command for your PR.

## Choosing the right command

Use `/review` when you want the complete automated PR review. This is the normal entry point for maintainers reviewing a PR.

Use `/review rerun` when the PR already has an AI review but something changed enough that the previous review may be stale. Typical reasons:

- the author pushed new commits;
- the author replied to review feedback;
- CI or test results changed;
- a maintainer wants a deterministic fresh review session without manually interpreting older output.

Use `/review tests` when the question is specifically about CI/test failures, for example:

- "Is this failure likely caused by the PR?"
- "Is CI red because of a known flaky test?"
- "Did this PR introduce a missing snapshot/baseline?"
- "Are these failures unrelated infrastructure or existing failures?"

Do not use `/review tests` as a substitute for a code review. It does not approve, request changes, apply labels, trigger reruns, or change the PR. It only posts evidence-based failure classification.

## `/review`: full PR review

### Trigger

Comment `/review` on a pull request.

Optional platform argument:

```text
/review android
/review ios
/review catalyst
/review windows
```

You can also use explicit flags:

```text
/review --platform ios
/review --branch main
```

The trigger is implemented by `.github/workflows/review-trigger.yml`. It:

1. checks that the comment is on a pull request;
2. verifies the actor has `write`, `maintain`, or `admin` repository permission;
3. parses the platform and optional pipeline branch;
4. infers the platform from `platform/*` labels when no platform was supplied;
5. queues the DevDiv `maui-copilot` Azure DevOps pipeline;
6. minimizes (collapses) the command comment as resolved once authorized.

The workflow intentionally does not handle `/review tests`; that subcommand is reserved for the test-failure review workflow.

**Note**: Command comments are minimized (collapsed as "Resolved") after authorization to reduce conversation clutter while preserving the comment history for the automated rerun scanner. Unauthorized or malformed command comments remain fully visible.

### Platform inference

If you do not specify a platform, the trigger looks at PR labels:

- `platform/iOS` -> `ios`
- `platform/macOS` -> `catalyst`
- `platform/android` -> `android`
- `platform/windows` -> `windows`

If labels are inconclusive, it defaults to Android. If that is wrong for the PR, use an explicit platform argument.

### What the review pipeline does

The Azure DevOps review pipeline is defined in `eng/pipelines/ci-copilot.yml`. At a high level it has three stages:

1. **ReviewPR**: checks out the PR, prepares the target platform, runs the Copilot PR review script, and publishes the initial review artifacts.
2. **RunDeepUITests**: runs detected UI test categories on the correct platform pool when the review identifies relevant UI tests.
3. **UpdateAISummaryComment**: updates the PR's `AI Summary` comment with review results and deep UI test results.

The PR review script is `.github/scripts/Review-PR.ps1`. It orchestrates the core review phases:

1. branch setup and PR merge for review;
2. UI category detection;
3. regression cross-reference;
4. gate verification;
5. candidate review and fix exploration;
6. AI summary posting;
7. review labels.

The generated PR comment is a single session-based `AI Summary` comment. New runs replace the review and hide older sessions, keyed by the reviewed commit.

## `/review rerun`: fresh full review

Comment `/review rerun` when you want a new full review session after the PR changed.

Operationally, `/review rerun` applies the `s/agent-ready-for-rerun` label to your PR. The hourly `rerun-review-scanner` workflow checks for eligible PRs with this label and triggers the review pipeline. This means your review may be delayed up to ~1 hour, or may not run if the PR is ineligible.

If you need an immediate review, use `/review` instead (write access required).

### Eligibility requirements

The `/review rerun` workflow checks for **PR author activity** since the latest AI Summary or rerun checkpoint:

- New commits (head SHA changed)
- New non-command comments from the PR author

**Important**: Reviewer or maintainer reminder comments do NOT satisfy the rerun eligibility check. Only author activity triggers a rerun.

When eligible, the workflow applies the `s/agent-ready-for-rerun` label. An automated hourly scanner processes queued reruns and triggers them when appropriate.

### Automated rerun scanner

The repository includes an hourly gh-aw workflow (`.github/workflows/rerun-review-scanner.md`) that:

1. Queries PRs labeled `s/agent-ready-for-rerun`
2. Uses AI to decide `trigger` or `skip` for each PR
3. Triggers approved reruns via Azure DevOps
4. Cleans up queue labels and posts reactions

This ensures reruns are processed automatically without manual intervention.

### When to use `/review rerun`

Use it when:

- a previous AI summary is stale;
- the author pushed a fix after review feedback;
- the previous run analyzed the wrong commit or incomplete context;
- a maintainer wants to replace an older session with a fresh one for the current PR head.

Avoid using it repeatedly without new commits or author comments. It consumes CI and agent capacity, and repeated identical runs are unlikely to add useful information.

## `/review tests`: test-failure review

### Trigger

Comment `/review tests` on a pull request.

The trigger is implemented by `.github/workflows/copilot-review-tests.md`, compiled to `.github/workflows/copilot-review-tests.lock.yml`.

Because gh-aw slash commands match only the first command token, the workflow listens for `/review` and then neutrally skips unless the comment uses the canonical `/review tests` subcommand. The regular `/review` trigger excludes `/review tests` so the two workflows do not both run.

**Note**: Like `/review`, the command comment is minimized (collapsed as "Resolved") after authorization to reduce conversation clutter.

### What it does

`/review tests` is comment-only. It does not:

- approve or request changes;
- apply labels;
- trigger CI reruns;
- change code;
- start the full PR review pipeline.

It gathers evidence from:

- GitHub PR metadata, labels, changed files, and check rollup;
- Azure DevOps build metadata, timelines, and build logs;
- Helix references when available for device tests;
- optional authenticated AzDO data when `AZDO_TOKEN` or local Azure CLI auth is available;
- PR scope, including changed platforms, areas, and test files.

Then it posts a `Test Failure Review` comment that classifies failures as:

- **Likely PR-caused**
- **Likely unrelated**
- **Needs human investigation**
- **Insufficient data**

The comment includes status badges, a short summary, a per-failure table, recommended action, and collapsible evidence details.

### Local usage

Maintainers can run the same flow locally:

```powershell
pwsh .github/scripts/Review-Tests.ps1 -PRNumber 29800 -BuildId 1443464
```

By default this writes local artifacts only:

```text
CustomAgentLogsTmp/TestFailureReview/<PRNumber>/context.json
CustomAgentLogsTmp/TestFailureReview/<PRNumber>/context.md
CustomAgentLogsTmp/TestFailureReview/<PRNumber>/report.md
CustomAgentLogsTmp/TestFailureReview/<PRNumber>/comment.md
```

To post the generated comment:

```powershell
pwsh .github/scripts/Review-Tests.ps1 -PRNumber 29800 -BuildId 1443464 -PostComment
```

To gather evidence without invoking Copilot:

```powershell
pwsh .github/scripts/Review-Tests.ps1 -PRNumber 29800 -BuildId 1443464 -GatherOnly
```

Local runs can use Azure CLI to acquire an Azure DevOps bearer token. If available, the gatherer records that in the generated context. If builds are still inaccessible after authenticated access, the report should say so and classify affected checks as `Insufficient data`.

### Interpreting `Insufficient data`

`Insufficient data` means the workflow saw a failing check but did not have enough reliable evidence to attribute it.

Common causes:

- AzDO build records returned 404 or expired;
- logs were inaccessible;
- the build is still running;
- authenticated AzDO test APIs were unavailable;
- device-test failures may be hidden in Helix and no Helix data was available.

Do not treat `Insufficient data` as "unrelated." It means a human or a rerun with better data is needed.

## How to read the review comments

### AI Summary

The full `/review` and `/review rerun` pipeline posts an `AI Summary` comment. It may include:

- gate status;
- UI test results;
- regression cross-reference;
- pre-flight context;
- code review findings;
- fix/candidate analysis;
- final recommendation.

The review sessions are collapsed by default — expand the **Review Sessions** section to read the latest session, which is keyed to the current HEAD commit. Previous review comments are minimized and hidden as outdated.

### Test Failure Review

`/review tests` posts a separate `Test Failure Review` comment. This comment is intentionally separate from the `AI Summary` so readers can quickly answer, "Why is CI red?" without reading the full review.

The top-level title is always:

```markdown
## Tests Failure Analysis
```

The verdict details and "Test Failure Review" label live in badges and in the expanded review session.

## Recommended workflow for maintainers

1. Make sure the PR has appropriate `area-*` and `platform/*` labels. The agentic labeler normally handles this on PR open/reopen.
2. Run `/review` when a PR is ready for automated review.
3. Read the `AI Summary` comment and check whether the review found actionable issues.
4. If CI is red or ambiguous, run `/review tests` to get a focused failure-causality report.
5. If the author pushes fixes or comments materially change the context, run `/review rerun`.
6. Use human judgment for merge decisions. These workflows provide evidence and recommendations, not final approval authority.

## Recommended workflow for community contributors

1. Open the PR with a clear description and linked issue when possible.
2. Wait for labels and CI to run.
3. If you need an automated review, ask a maintainer to run `/review`.
4. If CI is red and you are unsure whether it is caused by your changes, ask a maintainer to run `/review tests`.
5. When an automated comment is posted, read the summary first, then expand evidence sections for details.
6. Push fixes or reply with clarifying information, then ask a maintainer whether `/review rerun` is useful.

## Safety and trust boundaries

The review automation analyzes untrusted PR code and untrusted comments. The workflows are designed so privileged writes happen through controlled steps and safe outputs.

Important safeguards:

- `/review` requires repository write-level permissions and queues a trusted AzDO pipeline.
- `/review tests` is comment-only and uses gh-aw safe outputs for PR comments.
- The full review pipeline keeps PR-controlled code separated from trusted scripts where possible.
- Review comments should be treated as assistant-generated evidence, not as a substitute for human review.

## Troubleshooting

| Symptom | Likely cause | What to do |
| --- | --- | --- |
| `/review` does nothing | The commenter does not have write/maintain/admin access, or the comment is not on a PR. | Ask a maintainer to run the command on the PR. |
| `/review` used the wrong platform | Platform labels were missing or ambiguous. | Re-run with an explicit platform, for example `/review ios`. |
| `/review tests` says `Insufficient data` | Build/log/Helix evidence was inaccessible or incomplete. | Re-run later, provide a build ID, or run locally with Azure CLI/AzDO auth. |
| The AI Summary looks stale | New commits or comments landed after the last review. | Run `/review rerun`. |
| There are multiple old AI Summary comments | Each comment holds only the latest session (keyed to its HEAD commit); previous review comments are minimized and hidden as outdated. | Expand the **Review Sessions** section in the newest comment — it reflects the current HEAD commit. |
| `/review rerun` didn't trigger | Only PR author activity (commits or non-command comments) satisfies eligibility. Reviewer comments don't trigger reruns. | Wait for author activity or use `/review` to force a new review. |
| Command comment is still visible | The commenter may lack authorization, or the command was malformed. | Check actor permissions and command syntax. Authorized commands are minimized after processing. |

## Related files

- `.github/workflows/review-trigger.yml` — GitHub comment trigger for `/review`.
- `eng/pipelines/ci-copilot.yml` — Azure DevOps PR review pipeline.
- `.github/scripts/Review-PR.ps1` — local script orchestrating full PR review phases.
- `.github/scripts/post-ai-summary-comment.ps1` — AI Summary comment formatter.
- `.github/workflows/copilot-review-tests.md` — gh-aw source for `/review tests`.
- `.github/workflows/rerun-review-scanner.md` — gh-aw hourly scanner for queued `/review rerun` requests.
- `.github/scripts/Resolve-RerunEligibility.ps1` — determines if a PR is eligible for rerun based on author activity.
- `.github/scripts/Query-RerunReadyPRs.ps1` — queries PRs labeled `s/agent-ready-for-rerun`.
- `.github/skills/review-test-failures/SKILL.md` — classification rubric for test-failure reviews.
- `.github/scripts/Review-Tests.ps1` — local runner for `/review tests`.
- `.github/docs/trigger-azdo-pipeline-setup.md` — OIDC setup for triggering AzDO pipelines from GitHub Actions.
