# .NET MAUI automated PR and issue investigations

This guide explains the automated investigation commands used in dotnet/maui:

- `/review`
- `/review tests`
- `/review performance`
- `/issue trace-regression` (on issues)

It is intended for Microsoft maintainers and community contributors who want to understand when to request an automated review, what the automation does, and how to interpret the resulting comments.

## Quick command reference

| Command | Who can run it | What it does | Output |
| --- | --- | --- | --- |
| `/review` | Repository users with write, maintain, or admin access | Queues the full MAUI Copilot PR review pipeline. | Updates the PR with an `AI Summary` comment. |
| `/review <platform>` | Repository users with write, maintain, or admin access | Queues the full review pipeline for a specific platform: `android`, `ios`, `catalyst`, or `windows`. | Updates the PR with an `AI Summary` comment. |
| `/review tests` | Repository users with write, maintain, or admin access | Reviews current CI/test failures and classifies whether they are likely PR-caused, unrelated, or insufficiently evidenced. | Posts one `Tests Failure Analysis` comment and hides older reports. |
| `/review performance` | Repository users with write, maintain, or admin access | Runs selected managed benchmarks against pinned merge-base/head commits and reviews performance coverage. | Posts one validated performance report and hides older performance reports. |
| `/issue trace-regression` | Repository users with write, maintain, or admin access | Traces an issue's reported regression through release boundaries and source history to candidate introducing commits or PRs. | Posts one expandable `Regression Trace` comment on the issue and hides older reports. |

Only repository users with write access can trigger these commands. Community contributors should ask a maintainer to run the relevant command for their PR or issue.

## Choosing the right command

Use `/review` when you want the complete automated PR review. This is the normal entry point for maintainers reviewing a PR.

Use `/review tests` when the question is specifically about CI/test failures, for example:

- "Is this failure likely caused by the PR?"
- "Is CI red because of a known flaky test?"
- "Did this PR introduce a missing snapshot/baseline?"
- "Are these failures unrelated infrastructure or existing failures?"

Do not use `/review tests` as a substitute for a code review. It does not approve, request changes, apply labels, trigger reruns, or change the PR. It only posts evidence-based failure classification.

Use `/issue trace-regression` on an **issue** to investigate which change introduced
the reported behavior. This is different from the full PR review's regression-risk
check, which detects removal of earlier fixes.

## `/issue trace-regression`: trace an introducing change

Post exactly `/issue trace-regression` as a standalone issue comment. PR comments,
edited comments, bots, extra arguments and other `/issue` subcommands are ignored.
The workflow checks the comment author's current `write`, `maintain`, or `admin`
permission before collecting evidence and again before activation. The command
stays visible until the safe-output job has actually published a report; a separate
trusted completion job fetches the published comment, requires its exact triggering
issue URL, and rechecks permission before minimizing it, including when
the non-cancelled agent job failed after producing the published output. Cancellation
or a missing publication receipt skips completion. Downstream failures therefore
cannot make an unanswered command appear resolved. Private organization
membership does not exclude an otherwise authorized commenter. The credential-bearing PAT-pool job runs only
after the exact command and these permission checks succeed. Per-issue concurrency
uses the maximum pending queue, so unrelated comments or edits cannot replace
an already pending authorized request.

The gh-aw workflow `.github/workflows/issue-trace-regression.md` uses **GPT-6.1 Sol**,
the existing `copilot-pat-pool`, and the dedicated
`.github/skills/trace-regression/SKILL.md`. It requires no new secret. Changes to
the workflow source must include its regenerated `.lock.yml`. The pinned gh-aw
runtime requires `COPILOT_PROVIDER_WIRE_API: responses` and the
[`shared/gpt-6.1-sol.md`](../workflows/shared/gpt-6.1-sol.md) pricing import for
this model's wire protocol and AI-credit accounting.

The trusted collector freezes the issue, up to 100 latest comments, reported
working/failing versions, exact release-tag SHAs, and a bounded release comparison.
The collected context is mounted read-only in the agent sandbox. A failed context
download stops the agent before inference; no report is published without that
snapshot. Only headings outside backtick or tilde fences are parsed as form fields.
Duplicate version headings are ambiguous rather than silently selecting the first value;
comment pages whose counts change during collection record an evidence gap.
The collector also revalidates the issue's comment count and update marker after
pagination; concurrent changes or a failed revalidation make the comment evidence incomplete.
The agent narrows that history to affected code and verifies candidate diffs,
platform applicability and shipped ancestry. API failures, missing versions,
divergent branches and truncated history remain explicit evidence gaps.

Optional context extensions provide a cheap boundary preflight, a diagnostic
inventory and a trusted bounded source/history snapshot. Ambiguous headings or
two unmapped reported versions select `boundary-only`: the agent reads existing
corrections/diagnostics and reports the missing exact installed versions without
loading the full investigation skill or doing source/history searches. The
trusted mode is exposed as a fixed workflow output, not inferred from issue
instructions. The same native detection, report validation/publication and
receipt/minimization guards still run. One exact boundary permits static leads,
not regression attribution. Generic Preview/RC workload-set metadata is supplemental and does
not verify the application's installed MAUI packages.
Recoverable Preview/RC shorthand remains eligible for `metadata-resolution`
(one 30-release list page and two matching published releases), rather than being
discarded because its shorthand has no exact tag. Precise version values already
provided in prose/tables are inventoried as supplemental, unverified leads;
they never silently replace ambiguous form fields.

The trusted collector reads only `dotnet/maui`, at exact resolved release SHAs,
using symptom-selected **fixed paths**, not agent-supplied endpoints. Its budget
is six paths, 64 KiB per source file, ten commits per path and six commit diffs
with 8,000-character patches. Every source/history record includes revision/path
and API provenance; capped lists, renames not followed, missing/oversized files
and unavailable patches remain explicit. This deliberately limited read-only
snapshot restores useful leads without changing the MCP DIFC
`min-integrity: approved` policy, visibility rules or shell allowlist. Original
hosted failures included integrity-filtered history and separate shell permission
denials, not a demonstrated content-exclusion denial. No denied resource is
retried through another surface; organization content exclusions still apply.

Inline logs/code and author corrections are analyzed before requesting more
evidence. Public GitHub attachment links are inventoried with their originating
comment; an inventory alone is **not analysis**. For canonical
`github.com/user-attachments/assets/<UUID>` images, the trusted collector can
capture at most two PNGs (early evidence and latest correction), with no
credentials/cookies, a 20-second transfer deadline, 512 KiB per image, 4 million
pixels and at most two HTTPS redirects to GitHub's dedicated user-asset CDN.
Byte signature, content type and dimensions are checked; signed redirect URLs
are never retained. Image hashes/provenance and capture gaps accompany the
read-only snapshot. The agent uses its native image viewer and distinguishes
actually read screenshots from unavailable/inventoried evidence. Archives,
repros and dumps are never downloaded/executed. If the viewer or image capture
is unavailable, request only the missing discriminating text, not the existing
attachment again. A first-chance debugger exception screenshot is not proof of
an uncaught crash, runtime reproduction or a new introduction.

The report distinguishes **Confirmed introduction**, **Likely introduction**,
**Candidate**, and **Insufficient evidence**. Confirmation requires verifiable,
linked same-environment parent/candidate runtime evidence; source inspection or a
prior AI summary alone cannot establish it. A reported failing release is not
automatically the first bad release.

The command is **report-only**: it does not run reproduction code, builds, tests
or bisects, change labels, or push a fix. When confirmation needs execution, it
identifies the exact comparison to perform. Only the separate safe-output job
posts the report, restricted to the triggering issue with issue-only write access.
A trusted pre-publication step checks the bounded regular output file and rejects
cross-issue target aliases, repository overrides, and existing-comment edits before
the native handler runs. A report intent must have a nonempty text body, and multiple
report intents are rejected instead of allowing the native maximum to select one.
No-report outcomes remain valid and cannot minimize the command without a verified
publication receipt. This is required because the pinned handler prioritizes
an explicit `item_number` over `target: triggering`; configuration alone is not
the publication boundary.
Both agent-failure and custom failed-job issue reporters are disabled, so a failed
completion job cannot open an untargeted repository issue.

The comment follows `/review tests` styling: author/issue header, Scope/Range
badges, closed **Regression Analysis** and **Follow-up** accordions, and linked
candidate evidence. **Verdict** and **Evidence** (including degraded coverage)
remain visible above the accordions. A green run is not proof of healthy tooling:
the pinned v0.86.2 detector compares raw result strings before normalizing JSON,
so identical flags with omitted `reasons` versus `reasons: []` can cause a parser
warning. The declarative detector prompt requests one complete final object and
no delegation; it is prevention, not a parser fix or guarantee. Native verified
tooling warnings remain visible and authoritative; a parser failure is not itself
a detected threat. Detection and publication guards are unchanged.
Rerun the command after supplying missing version or
reproduction details; older reports are collapsed automatically.

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

The workflow intentionally does not handle `/review tests` or `/review performance`;
those subcommands belong to their focused gh-aw workflows. The full-review
missed-command recovery and rerun option parser also exclude both subcommands.

GitHub Actions webhook deliveries can occasionally be delayed or dropped during an Actions incident. A deterministic scheduled fallback (`.github/workflows/review-trigger-recovery.yml`) polls recent commands, waits 25 minutes so both bounded trigger jobs have time to finish, rechecks the commenter's current repository permission, and dispatches the same trusted review workflow. The default-branch commit used by the first scheduled run is a permanent lower bound, preventing already-handled commands from being replayed when the fallback is introduced. Processed commands are marked so a delayed webhook cannot trigger a duplicate review.

**Note**: Command comments are minimized (collapsed as "Resolved") after authorization to reduce conversation clutter while preserving the comment history. Unauthorized or malformed command comments remain fully visible.

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

## `/review performance`: performance review

Comment exactly `/review performance` on an open PR. Only newly created comments
activate the command; editing a comment does not start another measurement run.
Unauthorized comments, issues, other subcommands, closed PRs, and changes with no
product files do not start measurements. Authorized command comments are minimized
after the pinned context is ready. Runs for the same PR are serialized.

The gh-aw source is `.github/workflows/copilot-review-performance.md`; commit its
generated `.lock.yml` whenever it changes. Compile with the repository's pinned
gh-aw **v0.86.2**:

```bash
gh aw compile copilot-review-performance --strict --actionlint
```

The trusted orchestration script, `.github/scripts/Review-Performance.ps1`, uses
the existing `perf-analysis` selector, runner, comparator, policy, renderer, and
validator. The job boundaries follow the issue-replication isolation pattern:

1. **Intake:** authorize the caller, pin exact merge-base/head/harness commits,
   and select coverage from the full changed-file list without executing PR code.
2. **Measurements:** run managed Release benchmarks in ABBA order on a disposable
   hosted Linux runner. Base/head use separate unprivileged users and an explicit
   environment allowlist. No Copilot PAT or posting token is passed to measurements.
3. **Evidence and interpretation:** a fresh job imports bounded structured evidence
   and resolves the decision baseline. The GPT agent reads that bundle and the
   pinned diff; it writes narrative only, with no measurement or publishing authority.
4. **Publication:** a fresh gh-aw safe-output job downloads independent evidence,
   recomputes the decision, renders and validates the narrative, and rechecks
   authorization and live base/head identities. Only this job can publish the report.

The comment keeps **Performance Review Summary**, the actual PR author and pinned
commit notification, and truthful Scope/Result/Commit badges visible. Everything
else is inside exactly two closed sibling sections: **Performance Results**
(verdict, coverage, and any benchmark table) and **Findings & Follow-up**
(static findings, recommendations, and next action). Static-only and failed or
incomplete measurement reports use the same layout without implying measured
coverage. The trusted validator rejects flat, expanded, or extra sections and
checks the visible author, commit, and badges against the pinned evidence.

Native scenarios **do not run in the hosted command**. Device-required, sampled,
static-only, missing, and failed measurements remain explicit coverage gaps.
Native device tooling is outside this workflow's scope. Shared-host timing is
advisory; no whole-PR clean verdict may be inferred from a passing managed subset.
The command never approves, changes code or labels, queues the full review pipeline,
or automatically starts native follow-ups.

Manual dispatch requires the default branch and a positive `pr_number`:

```bash
gh workflow run copilot-review-performance.lock.yml --repo dotnet/maui \
  --ref main -f pr_number=12345 -f suppress_output=true
```

`suppress_output=true` still performs measurements and validates the report, but
does not post a comment. Evidence, rendered reports, and separate SDK-install,
build, and benchmark diagnostic artifacts are retained for seven days. Diagnostic
logs are not admitted as measurement evidence. Failed automation posts a run link
rather than a performance verdict; stale evidence is never published as a current report.
Retry with a fresh dispatch or `/review performance` command, not **Re-run failed
jobs**: repeated attempts can leave same-named gh-aw agent artifacts in one run,
allowing a downstream job to download an earlier failed attempt's empty output.

No new Azure pipeline, service connection, or secret is needed. The workflow uses
the existing `copilot-pat-pool` environment for GPT interpretation and read-only
agent permissions. The workflow, skill, and trusted scripts must land on the
default branch before slash commands can run. A default-branch dry-run canary is
required before treating the hosted path as operational. There is no scheduled
recovery for this command; request it again after resolving an automation failure.

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

The workflow uses one skill,
[`review-test-failures`](../skills/review-test-failures/SKILL.md), for both analysis
and comment formatting. The local runner uses that same skill. The hosted workflow,
like the other gh-aw workflows, uses GPT-6.1 Sol (`gpt-6.1-sol`) through the Responses
API. The local runner and offline evaluations keep their independently configured
models.

The pinned gh-aw **v0.86.2** runtime does not yet include this model's pricing or
wire-protocol metadata. Each workflow explicitly sets
`COPILOT_PROVIDER_WIRE_API: responses` and imports
[`shared/gpt-6.1-sol.md`](../workflows/shared/gpt-6.1-sol.md) for model-specific
pricing, preserving AI-credit accounting without a fallback to another model.
The per-token prices come from the
[OpenAI model reference](https://developers.openai.com/api/docs/models/gpt-6.1-sol).

It gathers evidence from:

- GitHub PR metadata, the actual code diff, changed files, and check rollup;
- Azure DevOps build metadata, timelines, and build logs;
- Helix references when available for device tests;
- optional authenticated AzDO data when `AZDO_TOKEN` or local Azure CLI auth is available;
- the latest five completed runs on the PR's target branch for each pipeline.

It always covers `maui-pr`, `maui-pr-devicetests`, and `maui-pr-uitests`. For each,
it compares the current run with the latest five completed runs of the same
pipeline definition on `refs/heads/<pr.baseRefName>` (for example,
`refs/heads/net11.0` for a PR targeting `net11.0`), never the PR source/merge ref.
These are the latest runs at review time, not only runs predating the PR build.
When there is current evidence to evaluate, it queries all three target-branch
windows even if one current PR build is missing or unreadable. Missing or
unreadable historical samples are disclosed when they affect attribution.
An optional build or check input
prioritizes evidence; it does not remove the other pipelines from the report.

Previous PR runs are not required and their absence is not a coverage gap.
The skill connects failure diagnostics to changed code and compares matching
reasons/configurations across all five target samples to distinguish existing defects.
Green device jobs require actual test-result confirmation.

Then it posts a `Test Failure Review` comment that classifies failures as:

- **Likely PR-caused**
- **Likely unrelated**
- **Needs human investigation**
- **Insufficient data**

The workflow posts exactly one concise comment using the original styled layout:
a visible author/commit header, Scope/Commit badges, and two closed
top-level sibling accordions, **CI Analysis** and **Follow-up**. CI Analysis
contains three nested sections: **maui-pr**,
**maui-pr-devicetests**, and **maui-pr-uitests**. Each failure gets its attribution,
a direct test-result/log/Helix link, and one short reason. Passing pipelines get
one line; missing evidence is mentioned only in its pipeline. Sampling inventories
and repeated coverage summaries stay out of the comment; detailed evidence stays
in the context artifact. There is no overall verdict, Verdict badge, or Summary
section; attribution is reported per failure rather than obscured by an aggregate
`Inconclusive` label. Follow-up contains an action only when needed and the
`/review tests` refresh instruction.

If no current-PR results exist, gathering stops before diff, known-issue,
and target-history analysis. It still posts a short **Evaluation skipped** report
asking for `/azp run` (or to wait if CI is already running). The local runner uses
that deterministic report, with the same badges and accordions, without invoking
Copilot. If just one pipeline lacks
results, the others are evaluated and the report requests `/azp run <pipeline>`
for the missing one. Actual build/restore/linker or host failures remain actionable
even when they prevent tests from starting; zero failed tests is not zero results.
Existing runs whose evidence was omitted, inaccessible, or not verified are
reported as **Insufficient data**, with a run link and `/review tests` refresh
guidance, not `No results available` or an unnecessary `/azp run`. Check discovery
deduplicates explicitly by name and URL so all three pipelines survive the
collector's ordered-dictionary representation.

This is failure attribution, not merge approval. The skill does not use the
legacy gatherer's deterministic merge-readiness verdict as a causal conclusion.
The workflow no longer publishes visual asset branches or appends image panels.

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

The local runner preserves the same styled, pipeline-grouped report; it does
not wrap or replace the skill's report. It retains the canonical
`<!-- Tests Failure -->` marker and adds a separate hidden local-ownership marker
so subsequent local runs update the local comment, not a workflow-owned report.
`-DryRun` prevents posting even when `-PostComment` is also supplied.
The report process has scoped access to the run's artifact directory, including
when `-OutputDirectory` is outside the checkout. Its default tools can only inspect
the frozen evidence (file readers and `jq`); the runner saves the final response
instead of granting model write access or reusing an earlier report. A missing or
unreadable context is an access problem, not proof that CI has no results.

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

The full `/review` pipeline posts an `AI Summary` comment. It may include:

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

The header gives the author and analyzed commit, with two badges showing the
`CI failures` scope and short SHA. Expand **CI Analysis** for the
three pipeline sections with related/unrelated/uncertain failures and direct
evidence links. Expand its sibling **Follow-up** for actions and refresh.
Failure labels use &#x1F534; for **Likely PR-caused**, &#x1F7E2; for
**Likely unrelated**, and &#x1F7E1; for **Needs human investigation**.
Green indicates attribution, not a passing test; yellow indicates unresolved causality.
Missing results produce `/azp run` guidance, not a long evaluation.
This is not merge approval; incomplete evidence remains explicit in its pipeline.
The canonical layout lives in the skill rather than a separate caller template.

## Recommended workflow for maintainers

1. Make sure the PR has appropriate `area-*` and `platform/*` labels. The agentic labeler normally handles this on PR open/reopen.
2. Run `/review` when a PR is ready for automated review.
3. Read the `AI Summary` comment and check whether the review found actionable issues.
4. If CI is red or ambiguous, run `/review tests` to get a focused failure-causality report.
5. If the author pushes fixes or adds material context, run `/review` for a fresh review.
6. Use human judgment for merge decisions. These workflows provide evidence and recommendations, not final approval authority.

## Recommended workflow for community contributors

1. Open the PR with a clear description and linked issue when possible.
2. Wait for labels and CI to run.
3. If you need an automated review, ask a maintainer to run `/review`.
4. If CI is red and you are unsure whether it is caused by your changes, ask a maintainer to run `/review tests`.
5. When an automated comment is posted, read the summary first, then expand evidence sections for details.
6. Push fixes or reply with clarifying information, then ask a maintainer to run `/review` for a fresh review.

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
| `/review` does nothing | The commenter does not have write/maintain/admin access, the comment is not on a PR, or GitHub Actions delayed the webhook. | Authorized commands should be recovered automatically within about 35 minutes. Check GitHub Status if Actions is degraded. |
| `/review` used the wrong platform | Platform labels were missing or ambiguous. | Re-run with an explicit platform, for example `/review ios`. |
| `/review tests` says `Insufficient data` | Build/log/Helix evidence was inaccessible or incomplete. | Re-run later, provide a build ID, or run locally with Azure CLI/AzDO auth. |
| The AI Summary looks stale | New commits or author comments landed after the last review. | Ask a maintainer to run `/review` for a fresh review. |
| There are multiple old AI Summary comments | Each comment holds only the latest session (keyed to its HEAD commit); previous review comments are minimized and hidden as outdated. | Expand the **Review Sessions** section in the newest comment — it reflects the current HEAD commit. |
| Command comment is still visible | The commenter may lack authorization, or the command was malformed. | Check actor permissions and command syntax. Authorized commands are minimized after processing. |

## Related files

- `.github/workflows/review-trigger.yml` — GitHub comment trigger for `/review`.
- `.github/workflows/review-trigger-recovery.yml` — scheduled fallback for missed `/review` webhooks.
- `.github/scripts/Recover-MissedReviewCommands.ps1` — deterministic recovery and duplicate-prevention logic.
- `.github/scripts/shared/ReviewCommandHelpers.ps1` — command parsing and authorization helpers for manual review recovery.
- `eng/pipelines/ci-copilot.yml` — Azure DevOps PR review pipeline.
- `.github/scripts/Review-PR.ps1` — local script orchestrating full PR review phases.
- `.github/scripts/post-ai-summary-comment.ps1` — AI Summary comment formatter.
- `.github/workflows/copilot-review-tests.md` — gh-aw source for `/review tests`.
- `.github/skills/review-test-failures/SKILL.md` — classification rubric for test-failure reviews.
- `.github/scripts/Review-Tests.ps1` — local runner for `/review tests`.
- `.github/docs/trigger-azdo-pipeline-setup.md` — OIDC setup for triggering AzDO pipelines from GitHub Actions.
