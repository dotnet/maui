# Issue duplicate detector

[`issue-duplicate-detector`](../workflows/issue-duplicate-detector.md) checks new
and reopened issues in `dotnet/maui`. Maintainers can manually
check an existing open issue. It posts at most one advisory report per run and
never changes labels, closes issues, or asks reporters to close an issue.

## Existing automation and improvement

MAUI previously received `github-actions[bot]` comments explicitly described as
finding similar issues **based on the issue title**, with decimal similarity
scores. Examples: [issue #22884](https://github.com/dotnet/maui/issues/22884#issuecomment-2152048565)
and [issue #22849](https://github.com/dotnet/maui/issues/22849#issuecomment-2149701059).
The latter suggested different CollectionView symptoms across platforms.
Those similarity scores are not probabilities of a shared defect.

At implementation time, the current workflow definitions and Policy Service
configuration contained no dedicated general-purpose duplicate detector.
This workflow complements the area/platform-only `agentic-labeler` and the
maintainer-authorized `/issue triage` command; neither is broadened.
The triage policy still requires an authorized canonical duplicate disposition,
not an AI probability, before applying a duplicate label.

The new detector compares full reports and comments, reproduction conditions,
platforms, handler generations, version boundaries, and diagnostics. Historical
title-only suggestions can seed discovery but cannot justify a probability.
Closed reports are explicitly identified; a recurrence after a fix may instead
be a new regression.

## Report contract

Every suggested match contains an **integer duplicate-probability estimate**,
shown as a percentage, supporting evidence, differences/uncertainty, and linked
excerpts from both reports. These are **uncalibrated AI estimates**, not measured
statistical probabilities or confirmed duplicate decisions.

| Probability | Publication                                                        |
| ----------- | ------------------------------------------------------------------ |
| 85-100%     | Likely duplicate, requiring distinctive compatible evidence.       |
| 60-84%      | Possible duplicate, with explicit missing evidence or uncertainty. |
| 0-59%       | Not posted; similarity alone is insufficient.                      |

The schema and trusted publisher reject missing, fractional, out-of-range, or
below-threshold scores; self-matches, repeated candidates, pull requests,
unverified source excerpts, and stale evidence also fail validation.
The publisher renders the table and GitHub links itself, so free-form agent
prose cannot bypass the probability requirement.

Search is bounded to eight queries, 20 results per query, ten investigated
candidates, 30 issue-read calls, and five published matches. Context is limited
to 300 comments per issue and a 1 MiB prepared file. Missing required evidence
is an incomplete run, not proof that there are no duplicates.
An unchanged report is suppressed. Changed reports are posted as new comments;
existing bot and human comments are never edited, deleted, or minimized.
The visible report fingerprint survives gh-aw's content sanitization. Reports
are recognized by the bot author, trusted workflow markers, and exact fingerprint
even when the publisher prepends a caution. Recognized reports are excluded
from evidence and target hashes.
Every candidate's content hash, timestamp, state, and lock status are rechecked
after report construction, followed by the target's final eligibility/evidence
check. Validation and GitHub publication are not an atomic transaction; changes
after the final checks remain possible.
Closed, locked, or already-marked-duplicate targets are skipped. Issues with a
known bot author are skipped automatically but can be checked manually.
Deleted-account author identities are preserved as `null`, without dropping
their evidence or treating unknown authors as trusted workflow bots.

## Manual preview and publication

Run from the repository default branch after the workflow is merged:

```bash
gh aw run issue-duplicate-detector --ref main --raw-field issue_number=12345 --raw-field staged=true
```

Replace `12345` with the real issue number. Manual runs default to `staged=true`:
the same analysis and validation run, but the proposed report is shown in the
workflow summary rather than posted. To publish, explicitly pass
`--raw-field staged=false`. New/reopened issue events publish qualifying reports.
No-match runs intentionally produce no comment.

The workflow uses the existing `copilot-pat-pool` environment and GPT-6.1 Sol
configuration. No additional service, model provider, token, or secret is needed.
The workflow-local PAT selector follows `issue-triage`'s existing pattern to
keep the activation guard explicit with the pinned gh-aw v0.86.2 compiler.
Prepared evidence and validator code come from trusted default-branch
infrastructure; issue/reproduction content is never executed.
The GitHub MCP's repository guard restricts both searches and issue reads to
`dotnet/maui`; repository scope is enforced rather than left to the prompt.

## Editing and validation

Commit the source, trusted publisher and compiled lock file together:

```bash
node --check .github/scripts/IssueDuplicates.cjs
gh aw compile issue-duplicate-detector --strict --validate
actionlint -oneline -ignore 'unexpected key "queue" for "concurrency" section' .github/workflows/issue-duplicate-detector.lock.yml
git diff --check
```

The native actionlint v1.7.12 does not yet recognize GitHub's supported
[`concurrency.queue`](https://docs.github.com/en/actions/how-tos/write-workflows/choose-when-workflows-run/control-workflow-concurrency)
field, emitted by gh-aw's conclusion job. The narrow ignore above covers only
that compatibility warning; the compiler still validates the workflow schema.
The native linter also runs the existing ShellCheck without requiring Docker.
