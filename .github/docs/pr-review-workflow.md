# .NET MAUI automated PR and issue investigations

This guide explains the automated investigation commands used in dotnet/maui:

- `/review`
- `/review tests`
- `/review expert-review`
- `/evaluate-tests`
- `/issue triage` (on open issues)
- `/issue trace-regression` (on issues)

It is intended for Microsoft maintainers and community contributors who want to understand when to request an automated review, what the automation does, and how to interpret the resulting comments.

## Quick command reference

| Command | Who can run it | What it does | Output |
| --- | --- | --- | --- |
| `/review` | Repository users with write, maintain, or admin access | Queues the existing full MAUI Copilot PR review pipeline. | Updates the PR with an `AI Summary` comment. |
| `/review <platform>` | Repository users with write, maintain, or admin access | Runs the full review on `android`, `ios`, `catalyst`, or `windows`. | Updates the PR with an `AI Summary` comment. |
| `/review tests` | Repository users with write, maintain, or admin access | Reviews current CI/test failures and classifies whether they are likely PR-caused, unrelated, or insufficiently evidenced. | Posts one `Tests Failure Analysis` comment and hides older reports. |
| `/review expert-review` | Repository users with write, maintain, or admin access | Runs only the read-only MAUI expert reviewer on GitHub, with independent GPT dimension passes. | Posts one consolidated COMMENT review with inline findings and an `Expert Review` summary. |
| `/evaluate-tests` | Repository users with write, maintain, or admin access | Evaluates test quality and coverage on an open PR with test source changes. | Posts one test evaluation report. |
| `/issue triage` | Repository users with write, maintain, or admin access | Applies evidence-backed manual labels to an open issue. | Labels only; explanation retained in Actions artifacts, not a public report. |
| `/issue trace-regression` | Repository users with write, maintain, or admin access | Traces an issue's reported regression through release boundaries and source history to candidate introducing commits or PRs. | Posts one expandable `Regression Trace` comment on the issue and hides older reports. |

Only repository users with write access can trigger these commands. Community contributors should ask a maintainer to run the relevant command for their PR or issue.

## Command access and other entrypoints

The [`dotnet/maui-external-partners` team](https://github.com/orgs/dotnet/teams/maui-external-partners)
already has **write access to dotnet/maui**. Inherited team access satisfies the
current repository-permission gates; the team's organization-wide default role
is not its repository grant. These commands do not require public membership or
a separate team-roster allowlist. Authorization still depends on the caller's
current effective repository permission, not the team name alone.

| Entrypoint | Permission gates and non-permission restrictions |
| --- | --- |
| [`/issue triage`](../workflows/issue-triage.md) | Native roles and pre-activation accept write/maintain/admin. Trusted `IssueTriage.ps1` rechecks that permission during Gather, Validate and Complete. Requires a new exact human command on an open issue, trusted default-branch infrastructure and attempt one; freshness, evidence authority and exact label postconditions remain mandatory. |
| [`/issue trace-regression`](../workflows/issue-trace-regression.md) | Native roles, collector checks before/after gathering and command completion accept write/maintain/admin. Requires a new exact human issue comment; publication validates the triggering issue and completion requires the actual report receipt. |
| [`/evaluate-tests`](../workflows/copilot-evaluate-tests.md) | Native roles accept write/maintain/admin. The compiled association prefilter retains OWNER/MEMBER/COLLABORATOR; collaborator access includes repository write access, not only public organization membership. Requires a PR comment and an open PR with test source changes. |
| [`/review tests`](../workflows/copilot-review-tests.md) | Native roles, exact-command authorization/minimization and failure-notification reauthorization accept write/maintain/admin. Association filters retain OWNER/MEMBER/COLLABORATOR. Requires the exact subcommand on a PR; reporting uses the triggering target. |
| [`/review expert-review`](../workflows/copilot-review-expert.md) | Native roles, exact human-command intake, collector, publisher and completion recheck current write/maintain/admin. Requires an open PR, default-branch infrastructure and a fresh run. PAT-pool access follows successful frozen-context collection; completion verifies the actual review receipt. |
| [`/review [platform] [options]`](../workflows/review-trigger.yml) | The normal trigger, recovery source-comment recheck and shared review-option helper accept current write/maintain/admin. Recovery also rechecks the original author before dispatch. `/review tests` and `/review expert-review` are separate workflows; unsupported legacy commands are ignored. |
| [`/evaluate-skills`](../workflows/skill-validation.yml) | The slash gate accepts write/maintain/admin on PR comments. Static-only mode can disable live evaluation independently of caller access; trusted-source and evaluation-content validation still apply. |
| [`/rebase`](../workflows/rebase.yml) | The commenter must have write/maintain/admin and post exactly `/rebase` on an open PR. Conflicts or unavailable maintainer edits can prevent branch updates after authorization. |
| [`/backport [to] <branch>`](../workflows/backport.yml) | Delegates to `dotnet/arcade`'s `backport-base.yml@main`; its current caller gate accepts write/admin, so partner write access qualifies. Requires a merged source PR and an existing target branch; upstream behavior can change independently. |

Post commands as new comments on the target type above, not inside an issue
description, quotation or code fence. Issue commands are not PR commands and
vice versa. Effective write access makes a caller eligible; it does not bypass
command matching, target state, infrastructure, validation or safety checks.

For `/issue triage`, absence of a public report is intentional. A successful
validated no-change/withheld-only result uses `noop`; a blocked, warning or
incomplete run keeps the command visible. Inspect Actions artifacts, detector
logs and native handler outcomes before claiming delivery. See
[`/issue triage`](issue-triage.md) for the artifact-only contract and
non-atomic completion postconditions. A triggered or green workflow is not
itself proof of applied labels.

External `/azp` commands are owned by the Azure Pipelines GitHub integration.
Its trust/service-connection permissions are separate from these repository
workflows; repository YAML or a GitHub team grant alone does not configure
Azure authorization.

## Choosing the right command

Use `/review` for the existing complete automated PR review, including expert
code review, alternative fixes, and the selected deep UI categories.

The full `/review` pipeline includes test verification alongside code review,
alternative-fix analysis, and selected deep UI tests. Test verification is not
available as a separate `/review gate` command.

Use `/review expert-review` for the experimental GitHub-hosted expert reviewer
alone: code/history review and inline findings, without the AzDO test gate,
fix candidates, UI tests, CI attribution, review labels or merge verdict.

`/review expert-reviewer` is an equivalent expert-only alias.
The expert lane uses the trusted exact-command matcher on newly created PR
comments, rather than the native slash-command prefilter, and accepts leading
whitespace and case-insensitive command text.

Use `/review tests` when the question is specifically about CI/test failures, for example:

- "Is this failure likely caused by the PR?"
- "Is CI red because of a known flaky test?"
- "Did this PR introduce a missing snapshot/baseline?"
- "Are these failures unrelated infrastructure or existing failures?"

Do not use `/review tests` as a substitute for a code review. It does not approve, request changes, apply labels, trigger reruns, or change the PR. It only posts evidence-based failure classification.

Use `/issue trace-regression` on an **issue** to investigate which change introduced
the reported behavior. This is different from the full PR review's regression-risk
check, which detects removal of earlier fixes; the test-verification phase does not run that check.

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
Conflicting duplicate version headings are ambiguous rather than silently selecting
the first value. Repeated answers identifying the same precise version are
consistent, including `v` prefixes, GA/SR qualifiers and a trailing parenthetical
note that contains no other version. The raw issue body and first answer remain
available; generic or conflicting answers are not normalized into a release.
Comment pages whose counts change during collection record an evidence gap.
The collector also revalidates the issue's comment count and update marker after
pagination; concurrent changes or a failed revalidation make the comment evidence incomplete.
The agent narrows that history to affected code and verifies candidate diffs,
platform applicability and shipped ancestry. API failures, missing versions,
divergent branches and truncated history remain explicit evidence gaps.

Optional context extensions provide a cheap boundary preflight, a diagnostic
inventory and a trusted bounded source/history snapshot. Conflicting headings or
two unmapped versions without a usable supplemental source pair select
`boundary-only`: the agent reads existing
corrections/diagnostics and reports the missing exact installed versions without
loading the full investigation skill or doing source/history searches. The
trusted mode is exposed as a fixed workflow output, not inferred from issue
instructions. The same native detection, report validation/publication and
receipt/minimization guards still run. One exact boundary permits static leads,
not regression attribution. Generic Preview/RC workload-set metadata is supplemental and does
not verify the application's installed MAUI packages.
Recoverable Preview/RC shorthand remains eligible for `metadata-resolution`
(one 30-release list page and two matching published releases across collector
and agent), rather than being
discarded because its shorthand has no exact tag. Precise version values already
provided in prose/tables are inventoried as supplemental, unverified leads;
they never silently replace ambiguous form fields.
An additive `investigation` record keeps a selected source pair and comparison
separate from `boundaries.reportedGood`/`reportedBad` and the original comparison.
An explicit good/bad pair from one human comment or issue text can select exact
release snapshots, with at most four supplemental version lookups.
Sources are considered in reverse frozen encounter order, not lexicographic
permalink order, so later corrections take precedence across comment-ID lengths.
Outcome cells or prose sentences must contain one version and fit 1,024 characters;
fenced code and SDK/OS version labels are excluded. The twenty-observation cap
retains early and late observations. Each selection retains author, outcome excerpt and permalink and
does not establish equivalent-environment runtime confirmation.
Statements such as "failed to reproduce" or "cannot reproduce" remain
unclassified outcomes; inability to reproduce is not a failing version boundary
or proof that a version works.
Present- and past-tense negations are handled consistently: "did not work" is
failing, while "did not reproduce" is a human-reported non-reproduction outcome,
not a failing boundary or verified runtime proof.
An explicit human "from/since version" failing observation can instead pair with
a resolved working form answer; both origins remain visible and the observed
failure is not claimed to be the first bad release.
Preview/RC release metadata can supply a `mapped-source` lead from the published
MAUI workload row, keeping the workload-set tag, package version and source commit
distinct. It cannot resolve the application's installed package or justify a
verified regression boundary.
Mapping eligibility, shorthand matching and preflight use the same canonical
version normalizer, including accepted parenthetical annotations, while retaining
the raw reported answer for provenance.
`investigation.releaseMetadata` records whether the list was requested and how
many release reads were attempted so inference cannot repeat an already spent
metadata budget.

The trusted collector reads only `dotnet/maui`, at exact resolved release SHAs
or explicitly marked published-release source commits,
using symptom-selected **fixed paths**, not agent-supplied endpoints. Its budget
is six paths, 64 KiB per complete source file, ten commits per path and six commit diffs
with 8,000-character patches. Every source/history record includes revision/path
and API provenance. For the large iOS `NavigationRenderer`, fixed trusted
symbols permit at most 256 KiB decoded input and retain at most 16 KiB/240 lines
of excerpts with original blob, size and line ranges. Such records explicitly
have `complete: false`, `representation: symbol-excerpts` and `content: null`;
omitted code cannot establish absence. Diff selection prefers commits in the
visible bad-side comparison history and round-robins across paths. A divergent
comparison remains non-linear even when it helps prioritize a source lead.
Older history is excluded only for a complete forward comparison. Capped lists,
renames not followed, missing/oversized files and unavailable patches remain
explicit. This deliberately limited read-only
snapshot restores useful leads without changing the MCP DIFC
`min-integrity: approved` policy, visibility rules or shell allowlist. Original
hosted failures included integrity-filtered history and separate shell permission
denials, not a demonstrated content-exclusion denial. No denied resource is
retried through another surface. Organization content exclusions must be honored,
but their enforcement on pre-activation REST bytes subsequently supplied through
the frozen artifact has not been verified. That integration requires
platform-owner evidence or an authorized non-production policy test; the hosted
public-repository acceptance runs do not establish it.

Title-matched groups precede incidental body/comment matches, and reported
platforms filter native paths before applying the six-path cap. Fixed groups
include Resizetizer versions/packaging, iOS NavigationPage TitleView,
item safe-area/wrapper handling, Windows CollectionView selection/AOT,
Android/iOS/Windows SwipeView and SourceGen resource generation. An explicit
HybridWebView name selects its shared and applicable native handlers
and excludes the legacy WebView group.
`EvaluateJavaScriptAsync` is shared by both controls and does not independently
select either group; method-only reports retain the unmatched-path gap. Legacy WebView
selection prioritizes Android's native view when Android is reported, followed
by applicable platform and shared handlers; source presence at
the exact release remains explicit, not proof that each platform is affected.

Inline logs/code and author corrections are analyzed before requesting more
evidence. Every report mode, including boundary-only without the skill, excludes
credentials, signed URLs, internal links and unrelated personal data and escapes
dynamic HTML/badge components. These are prompt constraints, not proof of
deterministic redaction.
Inventory uses the issue body and human (`authorType: User`) comments,
not bot reports. Prior regression/AI-summary/replication reports are also excluded even
when posted by an account GitHub identifies as a User; comments retain an
`isPriorReport` marker so the first agent read can omit them.
Public GitHub attachment links retain their originating
comment; the twenty-mention cap retains the earliest ten and latest ten so a
later correction can reach image selection. An inventory alone is **not
analysis**. Trailing sentence periods are removed from inventoried attachment
URLs; static capture still requires the exact canonical UUID URL.
Supplemental version tokens exclude sentence punctuation and do not
accept partial four-component stable versions. For canonical
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

Comment `/review` on a pull request to keep the existing full review process.
The same platform and branch options remain supported, for example
`/review ios` or `/review --platform android --branch main`.

The Azure pipeline runs Setup, test verification, Copilot review and alternative-fix analysis, fresh-agent full-review
publication, selected Deep UI tests, final AI Summary updates, lock cleanup,
and token-usage aggregation. Local full-review tooling remains available too.

`/review tests` remains an independent command. `/review gate`, `/review rerun`
and the retired `/review performance` are unsupported and ignored, including by
missed-command recovery. Full-review requests use the same permission checks,
per-PR concurrency, and in-progress lock.

Review commands do not post routine status or "already running" skip comments.
When a full review is already running, the trigger records the skip in
the Actions step summary and acknowledges and collapses the command comment
without posting a reply. `/review tests` also disables routine status comments.
Result reports and actionable failure notices remain enabled.

### Test verification within `/review`

The full review includes test verification against the prepared PR snapshot.
It checks existing tests before and after the fix, reports coverage limitations
and classifies setup/build/environment blockers as inconclusive. This phase is
part of the full `/review` pipeline and is no longer available as a separate
command or report.

## `/review expert-review`: standalone expert review

Post exactly `/review expert-review` as a new standalone human comment on an
open PR. Edited comments, issues, quoted/fenced commands, extra arguments,
other subcommands and unauthorized users do not activate this lane. It never
queues AzDO, including through full-review missed-command recovery. Per-PR
concurrency uses the maximum pending queue so unrelated comments cannot
replace an authorized pending request.

The gh-aw source is
[`copilot-review-expert.md`](../workflows/copilot-review-expert.md), paired with
its generated `.lock.yml`. The hosted adapter
[`ghaw-expert-reviewer.agent.md`](../agents/ghaw-expert-reviewer.agent.md) reuses
the canonical [`maui-expert-reviewer.md`](../agents/maui-expert-reviewer.md);
the 32-dimension rulebook is not duplicated. The primary expert, its fresh
single-level dimension workers, and the separately configured threat detector
all use **`gpt-6.1-sol`**. The primary review sandbox's inference-proxy
allowlist rejects other model IDs, including worker requests. The shared
[`gpt-6.1-sol.md`](../workflows/shared/gpt-6.1-sol.md) import supplies pricing
for the pinned runtime. The pinned
gh-aw version does not propagate that allowlist to the separate detector:
its model is explicitly pinned and delegation is forbidden by its prompt.
Standard Copilot proxy routing does not substitute unavailable models.
There is no Auto/model-alias fallback or non-GPT worker.
The existing `copilot-pat-pool` supplies Copilot authentication; no new
secret, Azure pipeline or service connection is needed.

The four canonical waves are preserved: code-first briefing and platform/
dimension routing, independent passes in batches of at most two, candidate
validation against surrounding source/callers/tests/blame, and required
findings-file output. Path routing takes the union of matching rows; Logic
and Correctness, Regression Prevention and Complexity Reduction always run.
The trusted collector freezes the expected dimension set from the union of
canonical path rules and conservative conditional triggers; publication rejects
a model-selected subset. The mapping is tied to the canonical routing section's
hash, so rulebook routing changes require an explicit mapping update.
Only test source is inspected: no builds, analyzers, tests, native devices,
repro applications or PR-controlled scripts execute. The workflow does not
invoke the full `pr-review` or `code-review` orchestration, produce a candidate
patch, change source/labels, approve or request changes.

Trusted collection freezes the infrastructure, PR head, base and merge-base,
full diff, changed paths, linked issue context, and all three feedback surfaces:
review bodies, inline comments and issue comments. Narrative stays separate
until the independent briefing. Full feedback is retained, with an explicit
bounded-collection failure instead of silent truncation. PR source is a
separate read-only snapshot; its `.github/` cannot replace trusted rules.
Implicit PR checkout is disabled, result directories must be fresh, and agent
shell tools cannot execute git aliases. Trusted collection supplies inert pinned
log/blame evidence for up to 50 changed files within a 12 MB content budget;
missing history is recorded explicitly rather than fetched by the agent.
The existing regression scanner uses its six-month/20-PR-per-file bounds with
a frozen current diff and changed-path list, pinned base and strict evidence
mode. Failed retrieval is a coverage gap, not CLEAN.

Each independent result is `dimensions/{id}.json` with exactly `dimension`
(integer), `complete` (boolean, not `completed`), and `findings` (the canonical
array, including `[]`). Both the primary reviewer and trusted publisher
validate this contract; worker text is not evidence of completion. Linked
issue collection is bounded to 20 referenced issues, with an explicit gap
when that bound is exceeded.
Finding headers use only `critical`, `major`, `moderate`, or `minor`; aliases
such as `medium` are invalid, including in discarded worker candidates.
Every saved worker candidate also has a retained/rejected disposition and
reason in `review-state.json`; retained entries identify their canonical
finding. Prior feedback cannot silently remove still-valid canonical findings.

A fresh native safe-output job imports bounded regular result files, checks
the exact schema and persisted worker completion, reauthorizes the requester,
and rechecks the open PR and frozen head/base. It stages up to 15 ordinary
findings plus the canonical committed-credential and archive/path-escape
exceptions (up to 17 total), preserving the dimension-31 severity cap.
Credential values must never appear in findings. Only exact RIGHT-side anchors
become inline comments. Findings without a valid anchor stay in the publication
artifact with an explicit Actions warning; they are not moved to an unrelated
line or published in a top-level comment.
Inside the isolated safe-output job, the trusted publisher sanitizes text with
the pinned runtime and submits **one atomic COMMENT review** pinned to the
reviewed commit. Permission and head/base are rechecked immediately before that
non-retried write. This deliberately avoids the pinned generic review buffer's
invalid-anchor fallback: a rejected anchor fails publication instead of moving
finding text into the receipt. Deduplication requires the exact sanitized body,
RIGHT side, path, line and reviewed commit, not a substring match.
Its compact receipt says **Expert Review - N findings** and links the
reviewed commit, including when there are zero findings. The count includes
already-reported findings; those are not duplicated inline. Behavioral Change
Disclosure findings (dimension 32: existing apps would look or behave
differently without opting in) are also counted as breaking changes in the
heading, and a warning callout links each location in the reviewed commit; the
publisher matches the dimension's label and never copies finding text into the
receipt. The Azure DevOps lane's
`post-inline-review.ps1` adds the same count and links when no
`review-summary.md` is supplied. Findings without
valid anchors remain in artifacts, with a short receipt note instead of their
details. Incomplete source assessments are identified explicitly, never
presented as completed zero-finding reviews.
Publication uses the repository-scoped Actions token and appears as
`github-actions[bot]`; it does not require a MauiBot publishing credential.
Completion verifies the review ID returned by this run's write, its exact body
and commit, and the exact count and membership of its inline comments. Manual
dispatch and combined commands perform the same receipt checks; only command
acknowledgement differs. Publication plans share a 4 MB UTF-8 bound through
submission and completion, including long or non-ASCII finding text.

There is no detailed expert summary, merge recommendation, author ping, badge,
dimension table, follow-up or separate automation-status comment. Failures and
context limitations stay in Actions logs and artifacts. This lane
does not edit, collapse or replace existing summary comments from earlier runs
or other review lanes.
Per-run AI-credit budgets are disabled for both the expert and independent
detector, and the daily AI-credit guard is disabled. Cost accounting is not a
prerequisite for starting or publishing a review; there is no custom accounting
job or required daily usage cache. Native usage reporting remains diagnostic.
The expert's configurable turn and consecutive-cache-miss caps use
`2147483647` because pinned gh-aw v0.86.2 does not support disabling them.
The generated detector retains the runtime's built-in turn/cache-miss caps;
its engine-level turn setting is not propagated by this compiler.
Provider-enforced quotas, runtime hard caps and job timeouts still apply.
The review always covers the entire merge-base-to-head PR diff across all
commits and changed files, not only the latest commit or changes since a prior
review. The pinned SHA identifies the frozen snapshot and inline anchors,
not a single-commit review scope. Prior feedback deduplicates comments without
reducing that scope.
Inline comments retain the canonical severity/dimension finding body; execution
artifacts retain run provenance. They describe concrete problems at that line,
not workflow capability or merge readiness. No separate issue comment is posted.
GitHub evidence reads retry only recognized network failures or HTTP 502/503/504,
at most three attempts with bounded backoff. Writes and authorization/rate-limit
failures are not retried. Errors retain the CLI exit code and HTTP status or a
fixed transport classification, never raw response bodies or stderr that could
expose credentials or PR-controlled content.
Hosted dimension workers run in non-overlapping batches of at most two to
reduce model throttling. A worker that explicitly hits model HTTP 429/502/503/504
or a network timeout may be retried once sequentially after 60 seconds; no model
fallback or fabricated completion is allowed. Source/history assessment and
JSON validation define completion. Lack of device/build/test execution does not
make a dimension incomplete; runtime certification remains outside this lane.
Workers assess test adequacy from source and can still report concrete missing
coverage. Built-in read/search tools avoid requiring interactive shell approval
for source and JSON validation.
Discussion comparison uses authors, full text, review states, timestamps and
inline anchors. A meaningful change is recorded in the publication artifact;
findings still assess the frozen code and discussion available at review start.
Head/base freshness remains a hard publication boundary; stale code never
receives new inline findings. Incidental metadata does not add a caveat.
For strict pinned history scans, an unavailable local historical merge object is
checked against the exact base SHA using GitHub's comparison API. A divergent
other-branch fix is excluded only after ancestry is established; failed or unknown
comparisons remain explicit context limitations, never a CLEAN history result.
Validated inline findings can be published while missing workers remain
explicit in the compact receipt and logs. Stale snapshots produce a silent no-op; closed
PRs, detection or schema failures stop publication and surface in Actions.
Source review does not establish test success or merge approval.

The command stays visible until all expected inline comments are independently
found on the correct PR, bot identity and pinned commit, and the exact compact
review receipt has been posted, including for zero findings. Incomplete/stale/failed runs
leave it visible. The completion job then rechecks the command and requester
before minimizing only that command.
For the combined command, only the existing Gate trigger acknowledges/minimizes
the request. Scheduled missed-command recovery can recover its Gate half, but
does not dispatch a second expert review. The standalone expert workflow still
has no scheduled recovery.

Manual dispatch requires the default branch and a positive integer
`pr_number`; the shared slash-command trigger prevents marking that input
required in the form, so trusted intake enforces it:

```bash
gh workflow run copilot-review-expert.lock.yml --repo dotnet/maui \
  --ref main -f pr_number=12345
```

**Manual dispatch publishes comments too.** This lane has no output-suppression
or staged-only mode. Context, findings, dimension results, completion state
and the validated publication plan are retained for seven days. Use a fresh
dispatch or command, not **Re-run failed jobs**, to avoid mixing same-named
artifacts from different attempts. There is no scheduled recovery for this
standalone lane.

Compile with the repository-pinned gh-aw **v0.86.2**:

```bash
gh aw compile copilot-review-expert --strict --actionlint --no-check-update
```

The compiler's `--actionlint` runner requires Docker. Without Docker, compile
without that flag and run the installed `actionlint` on the generated lock.
Older linters may not recognize GitHub's supported `queue: max`; exclude only
that exact schema diagnostic, not other errors. The ordinary AzDO trigger's
existing empty-choice/shellcheck diagnostics are unchanged by routing this
subcommand.

The workflow and trusted scripts must land on the default branch before this
new slash command can run. Registration is not an end-to-end canary: dispatch
against an approved PR target and verify routed independent GPT passes,
validated findings, explicit gaps and an actual published review/comment
receipt. Artifacts or a publication preview alone do not prove publication.
Do not treat the hosted lane as operational before that GitHub canary succeeds.

## `/review tests`: test-failure review

### Trigger

Comment `/review tests` on a pull request.

The trigger is implemented by `.github/workflows/copilot-review-tests.md`, compiled to `.github/workflows/copilot-review-tests.lock.yml`.

Because gh-aw slash commands match only the first command token, the workflow
listens for `/review` and neutrally skips unless the comment uses the canonical
`/review tests` subcommand. The full/gate trigger excludes `/review tests`,
so the two workflows do not both run.

**Note**: The command comment is minimized (collapsed as
"Resolved") after authorization and processing to reduce conversation clutter.

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
uses GPT-6.1 Sol (`gpt-6.1-sol`) through the Responses
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

### Test verification

The test-verification gate is included in the full `/review` run and its
results appear in the `AI Summary` comment. It is not a separate command or
standalone PR comment.

### Test Failure Review

`/review tests` posts a separate failure-attribution comment. It answers "Why is
CI red?" rather than checking the gate's without-fix/with-fix test outcomes.

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

### Expert Review

`/review expert-review` posts a consolidated PR review, not the full pipeline's
`AI Summary` or a test-failure report. Read its reviewed commit, completed
dimension count, breaking-change warning, inline findings, body-only findings
and evidence gaps. An
empty incomplete review is not a clean result. Request a fresh command after
new commits; history/source review is never a test pass or human approval.

## Recommended workflow for maintainers

1. Make sure the PR has appropriate `area-*` and `platform/*` labels. The agentic labeler normally handles this on PR open/reopen.
2. Run `/review` for the complete review.
3. Read the `AI Summary` and inspect the evidence, including test-verification results.
4. If CI is red or ambiguous, run `/review tests` to get a focused failure-causality report.
5. If the author changes the fix or tests, repeat the relevant command for a fresh snapshot.
6. Use human judgment for merge decisions. These workflows provide evidence and recommendations, not final approval authority.

## Recommended workflow for community contributors

1. Open the PR with a clear description and linked issue when possible.
2. Wait for labels and CI to run.
3. Ask a maintainer to run `/review` for a full review.
4. If CI is red and you are unsure whether it is caused by your changes, ask a maintainer to run `/review tests`.
5. When an automated comment is posted, read the summary first, then expand evidence sections for details.
6. Push fixes or tests, then ask a maintainer to repeat the relevant review command.

## Safety and trust boundaries

The review automation analyzes untrusted PR code and untrusted comments. The workflows are designed so privileged writes happen through controlled steps and safe outputs.

Important safeguards:

- `/review` requires repository write-level permissions and queues a trusted Azure pipeline.
- `/review tests` is comment-only and uses gh-aw safe outputs for PR comments.
- `/review expert-review` inspects frozen read-only source and publishes only
  through a fresh validated native COMMENT-review job with GPT-only detection.
- The full review pipeline keeps PR-controlled code separated from trusted scripts where possible.
- The gate uses trusted runner copies, a credential-free test task, and a fresh hosted publisher that imports only bounded diagnostics.
- Review comments should be treated as assistant-generated evidence, not as a substitute for human review.

## Troubleshooting

| Symptom | Likely cause | What to do |
| --- | --- | --- |
| `/review` does nothing | The commenter lacks write/maintain/admin access, the PR is closed, or the webhook was delayed. | Authorized commands should be recovered within about 35 minutes. Check GitHub Status if Actions is degraded. |
| `/review tests` says `Insufficient data` | Build/log/Helix evidence was inaccessible or incomplete. | Re-run later, provide a build ID, or run locally with Azure CLI/AzDO auth. |
| `/review expert-review` is stale, incomplete or failed | The PR changed, a worker/evidence source was unavailable, or trusted publication rejected the output. | Inspect the linked run and artifacts; request a fresh command after resolving the gap. Do not rerun old jobs. |
| `/review expert-review` does not start | The new workflow is not registered on the default branch, the command/target is invalid, or permission is insufficient. | Check default-branch rollout and exact command syntax; the standalone lane has no scheduled recovery. |
| Test-verification results are outdated | The PR head changed after Setup captured its snapshot. | Run `/review` again for a fresh snapshot. |
| Command comment is still visible | The commenter may lack authorization, or the command was malformed. | Check actor permissions and command syntax. Authorized commands are minimized after processing. |

## Related files

- `.github/workflows/review-trigger.yml` — GitHub comment trigger for `/review`.
- `.github/workflows/review-trigger-recovery.yml` — scheduled fallback for missed full-review webhooks.
- `.github/scripts/Recover-MissedReviewCommands.ps1` — deterministic recovery and duplicate-prevention logic.
- `.github/scripts/shared/ReviewCommandHelpers.ps1` — command parsing and authorization helpers for manual review recovery.
- `eng/pipelines/ci-copilot.yml` — full-review Azure pipeline, including test verification.
- `.github/scripts/Review-PR.ps1` — orchestrates full-review phases, including test verification.
- `.github/scripts/post-ai-summary-comment.ps1` — AI Summary comment formatter.
- `.github/workflows/copilot-review-expert.md` — standalone gh-aw expert-review source.
- `.github/agents/ghaw-expert-reviewer.agent.md` — trusted hosted adapter.
- `.github/agents/maui-expert-reviewer.md` — canonical expert dimensions and review contract.
- `.github/scripts/Review-Expert.ps1` — frozen context, validated publication and receipt checks.
- `.github/workflows/copilot-review-tests.md` — gh-aw source for `/review tests`.
- `.github/skills/review-test-failures/SKILL.md` — classification rubric for test-failure reviews.
- `.github/scripts/Review-Tests.ps1` — local runner for `/review tests`.
- `.github/docs/trigger-azdo-pipeline-setup.md` — OIDC setup for triggering AzDO pipelines from GitHub Actions.
