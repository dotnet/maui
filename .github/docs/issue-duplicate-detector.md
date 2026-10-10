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
The publisher renders the comment and GitHub links itself, so free-form agent
prose cannot bypass the probability requirement. Reports follow the
[`/review tests` visual layout](../skills/review-test-failures/SKILL.md):
an issue header and two blue Scope/Issue badges, then closed sibling
**Duplicate Analysis** and **Follow-up** sections with icons, spacing, and
horizontal rules. Every probability remains visible in the compact summary
without expanding anything. Duplicate Analysis nests one closed section per
candidate, including its percentage/assessment, state, matching evidence,
differences/uncertainty, and linked source excerpts. Follow-up contains the
maintainer action, closed-report caveat, refresh instructions, and workflow link.

The network allowlist includes `img.shields.io` alongside `defaults` because
gh-aw also uses it to sanitize published URLs. This allows the two badge images
without disabling URL filtering or broadening the GitHub tool permissions.

Issue discovery uses the trusted, read-only
[`IssueDuplicateSearch.cjs`](../scripts/IssueDuplicateSearch.cjs) MCP service.
The pinned GitHub MCP versions force semantic search, whose shared installation
quota rate-limited all five overlapping fork trials. The replacement makes
ordinary REST issue searches with literal keyword/phrase inputs supplied by
GPT, not title-only similarity. It fixes `repo:dotnet/maui is:issue`, searches
open and closed reports, and returns only unscored candidate metadata. Full
reports and chronology still come from the repository-scoped GitHub `issue_read`.

The search service enforces eight logical queries, 20 results per query, one
page, and at most 16 search HTTP attempts including retries for the entire
agent job. It serializes requests, spaces them by at least seven seconds,
honors both `Retry-After` and exhausted `x-ratelimit-reset` headers, and uses
bounded exponential backoff for secondary limits without reset headers.
There are at most three attempts per query, a 15-second HTTP timeout,
120 seconds per wait, and 180 seconds of cumulative waiting. Missing evidence,
bad inputs, exhausted budgets, incomplete API results and other HTTP errors
fail explicitly; they never become empty search results. Runs share a
workflow-wide `queue: max` concurrency group rather than per-issue groups,
so different issues do not compete for quota or displace pending checks.
GitHub permits at most 100 pending runs in that queue.

The service runs outside the agent sandbox using the existing pinned gh-aw
MCP transport, with no new dependencies or secrets. Its only GitHub credential
is this read-only job's built-in `GITHUB_TOKEN`; it never rotates credentials to
evade throttling. A fresh local authentication key is separate from that token.
Keywords cannot inject qualifiers, change repositories, select an endpoint, or
invoke commands. Public repository metadata and every returned issue's
repository/URL are checked. Agent tools still grant neither shell nor file writes.

The native GitHub gateway separately enforces 30 `issue_read` calls per MCP
session. These limits do not include public repository checks or the trusted
collector/publisher's API reads. Ten investigated candidates remain an **agent
instruction**, not an investigation counter. The trusted collector/publisher
enforces five published matches, 300 comments per issue, and 1 MiB regular JSON
files. The agent job also has a 15-minute timeout. A trusted post-agent step
collects the service's run/source/hash-bound status, retains its diagnostics,
and stops its exact process. The separate publisher rejects missing, failed,
unfinished or empty discovery for both comments and completed no-match outcomes.
It also requires the authoritative threat-detector conclusion to be `success`
or an intentional `warning`; a successful detector job cannot mask a missing
or failed verdict.
An unchanged report is suppressed. Changed reports are posted as new comments;
existing bot and human comments are never edited, deleted, or minimized.
The fingerprint covers the trusted rendered summary, assessments, excerpts,
and static follow-up, not the evidence-freshness hash or per-run workflow link.
An unrelated target comment therefore cannot defeat suppression when the rendered report is unchanged;
the separate evidence hashes still require fresh target and candidate snapshots.
The visible report fingerprint survives gh-aw's content sanitization. Reports
are recognized by the bot author, trusted workflow markers, and exact fingerprint
even when the publisher prepends a caution. Recognized reports are excluded
from evidence and target hashes.
Safe-output publication is pinned to the built-in `GITHUB_TOKEN`, keeping
comment authors consistent with the strict `github-actions[bot]` provenance
check instead of accepting a configurable publisher identity.
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
the same analysis and validation run, but the trusted validator writes the
same expandable report, including its visible probabilities, to the safe-output
job's **Validated duplicate report preview** summary rather than posting them.
This preview appears only after all validation and final freshness checks,
and precedes gh-aw's publication sanitization, cautions, and provenance wrappers.
It does not by itself verify which URLs survive publication. Invalid,
stale, no-match, and identical-report-suppressed outputs produce no report preview.
To publish, explicitly pass
`--raw-field staged=false`. New/reopened issue events publish qualifying reports.
No-match runs intentionally produce no comment.
The run-scoped evidence artifact is overwritten when the collector reruns, so
full reruns do not collide with immutable uploads. Failed-job-only reruns can
still download the completed collector's artifact; final freshness checks remain
required before publication.

The workflow uses the existing `copilot-pat-pool` environment and GPT-6.1 Sol
configuration. No additional external service, model provider or repository secret is needed.
The workflow-local PAT selector follows `issue-triage`'s existing pattern to
keep the activation guard explicit with the pinned gh-aw v0.86.2 compiler.
Prepared evidence and validator code come from trusted default-branch
infrastructure; issue/reproduction content is never executed.
The GitHub MCP's repository guard restricts issue reads to `dotnet/maui`;
the trusted discovery service separately enforces that same public repository.
Repository scope is enforced rather than left to the prompt.
The gateway's public-repository scope override is disabled so it cannot broaden
that explicit scope to all public repositories.
Exact repository scope conservatively carries the `private:dotnet/maui` secrecy
label even for public reports. The workflow declares
`private-to-public-flows: [safeoutputs, duplicate-search]` for the publication
server and the fixed public-repository read-only search service only; their
write-sinks still accept only `private:dotnet/maui`. The compiler omits sink
visibility only for these two named servers, not for unrelated sinks,
and does not emit a blanket `allow` or wildcard exemption.
The trusted collector and validator check live repository metadata and reject
anything other than public `dotnet/maui`, including a final check before a report
is approved for publication. The compiled Copilot command grants neither shell
nor filesystem-write tools.

## Editing and validation

Stock gh-aw v0.86.2 rewrites structured GitHub `allowed` entries to tool names
during default-tool normalization, dropping their `max-calls` metadata, and
grants filesystem-write permission even when `edit: false` is declared.
[`CompileIssueDuplicateDetector.sh`](../scripts/CompileIssueDuplicateDetector.sh)
builds an isolated compiler from the immutable
[`8b600a3beee3591b7add4c79e595d9481a470cc4` correction](https://github.com/kubaflo/gh-aw/commit/8b600a3beee3591b7add4c79e595d9481a470cc4)
on top of v0.86.2. The correction preserves both gateway call limits and
Copilot's explicit tool permissions, honors disabled editing, and wires the
scoped safe-output exemption consistently in strict validation and JSON/TOML
configuration. It does not install or replace the user's `gh aw` extension.
Compiler metadata identifies the patched build.
Only the official MCP gateway is upgraded, to
[`v0.4.30`](https://github.com/github/gh-aw-mcpg/releases/tag/v0.4.30), whose
[`sink-visibility correction`](https://github.com/github/gh-aw-mcpg/commit/ece856095ca6445fc4f41884512478b1d4e2851c)
honors the safe-output exemption. Its immutable image digest is
`sha256:ab5a436a1490438db473e4e3d4c973cb1d75e3cb233fb08b73d31b42d7d18fba`.
All other runtime action/container pins and engine versions remain unchanged.
The custom compiler version is not recognized by the runtime's official-release
checker, so an explicit pre-agent step runs the same compatibility and revocation
check against the v0.86.2 base before inference.

Go 1.26.5 or later is required for this build-only workaround. The helper caches
the pinned source and compiler under `${XDG_CACHE_HOME:-$HOME/.cache}/maui/gh-aw`,
checks the source revision and cleanliness before every build, and removes
publication/inference tokens from both the build and compiler execution using
the same command-scoped environment wrapper. The build
sets `GOWORK=off`, so parent- or source-local `go.work` files cannot override the
pinned module's dependency selection. This isolates Go workspace resolution,
not every aspect of the build environment. Replace this helper with a fixed
official compiler after verifying that it preserves the counter policy, explicit
Copilot allowlist, disabled editing and scoped safe-output policy; do not
regenerate with stock v0.86.2 or hand-edit the generated lock.

Commit the source, compilation helper, trusted publisher and compiled lock file together:

```bash
node --check .github/scripts/IssueDuplicates.cjs
node --check .github/scripts/IssueDuplicateSearch.cjs
bash .github/scripts/CompileIssueDuplicateDetector.sh
shellcheck .github/scripts/CompileIssueDuplicateDetector.sh
actionlint -oneline -ignore 'unexpected key "queue" for "concurrency" section' .github/workflows/issue-duplicate-detector.lock.yml
git diff --check
```

The native actionlint v1.7.12 does not yet recognize GitHub's supported
[`concurrency.queue`](https://docs.github.com/en/actions/how-tos/write-workflows/choose-when-workflows-run/control-workflow-concurrency)
field, emitted by gh-aw's conclusion job. The narrow ignore above covers only
that compatibility warning; the compiler still validates the workflow schema.
The native linter also runs the existing ShellCheck without requiring Docker.
