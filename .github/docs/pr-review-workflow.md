# .NET MAUI test-failure review workflow

This guide explains `/review tests`, the automated test-failure review command used in dotnet/maui pull requests.

The legacy full-review command and automatic review reruns have been retired.
Standalone code-review, PR-review, test-verification, and PR-finalization skills remain available for local use.

It is intended for Microsoft maintainers and community contributors who want to understand when to request an automated review, what the automation does, and how to interpret the resulting comments.

## Quick command reference

| Command | Who can run it | What it does | Output |
| --- | --- | --- | --- |
| `/review tests` | Repository users with write, maintain, or admin access | Reviews current CI/test failures and classifies whether they are likely PR-caused, unrelated, or insufficiently evidenced. | Posts one `Tests Failure Analysis` comment and hides older reports. |

Only repository users with write access can trigger this command. Community contributors should ask a maintainer to run it for their PR.

## Choosing the right command

Use `/review tests` when the question is specifically about CI/test failures, for example:

- "Is this failure likely caused by the PR?"
- "Is CI red because of a known flaky test?"
- "Did this PR introduce a missing snapshot/baseline?"
- "Are these failures unrelated infrastructure or existing failures?"

Do not use `/review tests` as a substitute for a code review. It does not approve, request changes, apply labels, trigger reruns, or change the PR. It only posts evidence-based failure classification.

## `/review tests`: test-failure review

### Trigger

Comment `/review tests` on a pull request.

The trigger is implemented by `.github/workflows/copilot-review-tests.md`, compiled to `.github/workflows/copilot-review-tests.lock.yml`.

Because gh-aw slash commands match only the first command token, the workflow listens for `/review` and then neutrally skips unless the comment uses the canonical `/review tests` subcommand.

**Note**: The command comment is minimized (collapsed as "Resolved") after authorization to reduce conversation clutter.

### What it does

`/review tests` is comment-only. It does not:

- approve or request changes;
- apply labels;
- trigger CI reruns;
- change code.

The workflow uses one skill,
[`review-test-failures`](../skills/review-test-failures/SKILL.md), for both analysis
and comment formatting. The local runner uses that same skill. Both use GPT-6 Astra
(`gpt-6-astra`), as do the offline attribution evaluations and their judge.

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

## How to read the review comment

`/review tests` posts a `Test Failure Review` comment answering, "Why is CI red?"

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
2. Review the code directly or use the standalone review skills.
3. If CI is red or ambiguous, run `/review tests` to get a focused failure-causality report.
4. After new commits and CI results, run `/review tests` again when a refreshed report is needed.
5. Use human judgment for merge decisions. The report provides evidence, not final approval authority.

## Recommended workflow for community contributors

1. Open the PR with a clear description and linked issue when possible.
2. Wait for labels and CI to run.
3. If CI is red and you are unsure whether it is caused by your changes, ask a maintainer to run `/review tests`.
4. Expand the report's evidence sections and follow any requested actions.
5. Push fixes or reply with clarifying information, then request a fresh review from a maintainer.

## Safety and trust boundaries

The review automation analyzes untrusted PR code and untrusted comments. The workflows are designed so privileged writes happen through controlled steps and safe outputs.

Important safeguards:

- `/review tests` is comment-only and uses gh-aw safe outputs for PR comments.
- The command requires repository write-level permissions.
- Review comments should be treated as assistant-generated evidence, not as a substitute for human review.

## Troubleshooting

| Symptom | Likely cause | What to do |
| --- | --- | --- |
| `/review tests` does nothing | The commenter lacks write/maintain/admin access, the comment is not on a PR, or command syntax is incorrect. | Check permissions and use the exact `/review tests` command. |
| `/review tests` says `Insufficient data` | Build/log/Helix evidence was inaccessible or incomplete. | Re-run later, provide a build ID, or run locally with Azure CLI/AzDO auth. |
| Command comment is still visible | The commenter may lack authorization, or the command was malformed. | Check actor permissions and command syntax. Authorized commands are minimized after processing. |

## Related files

- `.github/workflows/copilot-review-tests.md` — gh-aw source for `/review tests`.
- `.github/skills/review-test-failures/SKILL.md` — classification rubric for test-failure reviews.
- `.github/scripts/Review-Tests.ps1` — local runner for `/review tests`.
