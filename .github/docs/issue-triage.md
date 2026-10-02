# `/issue triage`

Post exactly `/issue triage` as a new comment on an **open issue** in
`dotnet/maui`. The caller must currently have write, maintain or admin permission.
The command proposes and applies evidence-backed manual issue labels and posts
one explanation. It does not reproduce bugs, run samples, change milestones or
assignees, close/reopen issues, modify product code or create pull requests.

This is a standalone **GitHub Agentic Workflow**, not an Azure pipeline.
It complements, rather than broadens, the automatic `agentic-labeler`: the
automatic opening-event labeler still applies only area/platform labels.
The interactive `issue-triage` skill retains its human-approved milestone flow.

## Where existing labels come from

The repository has hundreds of live labels, not one interchangeable taxonomy.
Issue forms assign initial type/proposal labels. The opening-event agent assigns
area/platform labels. Maintainers and registered partner validators add the
remaining triage metadata. Policy Service owns feedback/staleness transitions
and identity rules; release/CI/PR-review automations have separate bookkeeping.

An issue-opening event under a human account can be form-assigned labeling.
Likewise, an event under a maintainer account can be automation evaluation, not a
product decision. Live label existence and current issue membership alone are
insufficient evidence for this command.

## Label rules

The [full-triage skill](../skills/issue-triage-labels/SKILL.md) and its
[machine-readable policy](../skills/issue-triage-labels/references/label-policy.json)
are authoritative. Label names are rediscovered, fully paginated, on each run.
Use canonical exact names, including emoji; never create labels or normalize a
typo into a new label.
Prepared `eligibleLabels` governs additions; `removableLabels` lists eligible
current labels, including removal-only `s/needs-attention`/`needs-area-label`.

| Family | Required evidence |
| --- | --- |
| `area-*`, `platform/*` | The actual subsystem and explicitly affected platforms, not incidental mentions. Preserve justified secondary areas. Mac Catalyst normally uses `platform/macos`. Explicit Linux/Tizen reports may be classified by this manual policy without implying Microsoft support or changing the automatic labeler's Tizen exclusion. |
| `t/*`, question/task, feature/layout/handler/testing facets | The actual subject and scenario: bug, enhancement, docs, accessibility, native embedding, Material3, CollectionView generation, XAML source generation, migration, etc. Established exact labels are eligible; unfamiliar and automation-only tags are preserved. |
| `s/verified`, `i/regression` | Existing positive, scenario-specific reproduction/validation by a currently authorized maintainer or a validator named in the existing Syncfusion identity policy. A sample link, build success, plausible diagnosis, or this classifier's completion is not verification. Later contrary validation blocks stale confirmation. |
| `potential-regression`, `regressed-in-*` | Distinguish reported suspicion from confirmation. A first-bad-version label needs demonstrated version-boundary evidence, not just a failing SDK in logs. OS changes and Xamarin.Forms differences are not automatically MAUI regressions. |
| Review/investigation/no-repro states | Evidence of the corresponding review or validation. No-repro is not an inaccessible sample, timeout or infrastructure failure. The command cannot manufacture empirical results. |
| `s/needs-info`, `s/needs-repro`, `s/try-latest-version` | Specific missing information, an inadequate reproduction, or an authorized assessment supporting a newer version, with a concrete author request. Adequate inline code/attachments can suffice without a repository URL. |
| `p/0` through `p/3` | Explicit existing maintainer decisions. Priority is product/release planning, not an automatic severity score: p/0 is highest release-targeted priority, p/1 important scheduled work, p/2 important unscheduled work, p/3 nice-to-have. |
| Proposal acceptance, backport approval, release/fixed-in, contributor suitability and planning labels | Explicit existing maintainer authority; do not invent roadmap acceptance, release guarantees, approval or ownership from upvotes or impact. |
| `partner`, `partner/*`, `external` | Actual documented involvement/routing and maintainer authority, not guessed identity. Partner collaboration need not imply that the reporter belongs to that partner. The existing identity automation is unchanged. |
| `perf/*`, `version/*`, workaround/device-only | Relevant measurements, retained objects for a leak, explicit affected OS/device versions, or a usable workaround/device restriction. A crash alone is not a memory leak; later simulator reproduction or a disputed workaround must be considered. |
| Duplicate/not-a-bug | An authorized disposition. A duplicate also cites the fetched canonical issue; similarity is insufficient. Not-a-bug needs a technical explanation. Neither label closes the issue. |

Read-only registered validators may supply reproduction evidence but **cannot**
authorize priority, roadmap, backport or ownership commitments. Business decisions
must explicitly name the exact label and an affirmative action in unquoted
maintainer prose, for example `Apply p/1` or `Remove backport/approved`.
Exact label token boundaries apply to approvals and superseding decisions:
`Apply partner/syncfusion` does not also approve `partner`.
All label decisions and both actions share non-question and affirmative-polarity
checks with their supersession scan. "Do not remove p/1" and "Should we remove
p/1?" cannot revoke "Apply p/1"; a directed "Do not apply p/1" can veto it.
Conditional/timing-contingent decision paragraphs cannot authorize additions,
removals or superseding reversals. "Apply p/1 if the regression is confirmed" is
not a current priority commitment, nor is "Remove p/1 once validation is complete"
a completed revocation. Ambiguous mixed paragraphs are withheld; cite a separate
unconditional decision rather than inferring that later evidence activated a
previous condition.
Directed prohibitions retain the existing conservative veto on older authority:
"Do not apply p/1 until validation is complete" still blocks an older approval.
It does not authorize a new removal or prove completion of the condition.
The entire cited comment is also checked for a qualifying opposite decision or
directed veto, not just the selected paragraph. "Apply p/1" and "Remove p/1" in
the same comment authorize neither action, including when creation and update
timestamps are equal. The command does not infer which paragraph was intended
to win; require a fresh unambiguous maintainer comment. The same exact-label,
polarity, conditional and Markdown rules govern the opposite-decision check.
Markdown parsing excludes quoted requests (including lazy continuations),
indented/fenced/inline code and HTML quote/code containers from decision prose.
Link destinations, titles, reference definitions and image metadata cannot
authorize decisions; link text remains visible prose. Reference intake separately
retains link targets so canonical same-repository issue links are still fetched.
Every evidence quote must survive the same prose filter, including content labels
and narrow removals. Code/samples can inform analysis but cannot serve as
authority quotations.
Marker-bearing result comments from this workflow's publisher are excluded from
evidence sources and related-reference intake. They remain in `resultComments`
for context hashing and retry reconciliation only. Generated report reasons
cannot become fresh facts after the original evidence is edited or removed.
Suggestions and tentative candidates are not approvals.
Tentative validation such as "should be reproducible on Android" is not an
observed reproduction and cannot support confirmation labels.
Conditional outcomes such as "If the issue is reproduced on Android, collect
logs" cannot supply observed confirmation, even when the citation omits the
conditional prefix. The shared positive-evidence predicate applies to confirmation
additions, removal transitions and contrary positive results. It distinguishes
contingent outcomes from factual reproduction scenarios such as "I reproduced
the issue when the keyboard was visible"; existing conservative negative-evidence
vetoes are unchanged.
Technical-state labels require their specific affirmative review, reproduction
outcome or version recommendation, not merely an authorized comment author.
Technical-assessment questions are rejected. No-repro is withheld when the cited
comment reports resource access, build/download, authentication/network failure or
timeout; not-regression cannot use a negated same-behavior comparison.
All technical assessments share chronology checks for additions and removal
transitions. Later label removals, including automated Policy Service removals,
contrary state labels, authorized contrary outcomes/retractions and explicit
maintainer revocations supersede old support. A later non-maintainer issue-author
reply ends a try-latest feedback wait, matching existing Policy Service behavior.
Old recommendations cannot restart that wait; re-adding needs a fresh qualifying
assessment. Contrary edits use last-modified time, while affirmative support keeps
its original creation time. Mixed assessment/retraction comments are withheld.
A later i/regression, potential-regression, blazor-webview2-regression or
regressed-in-* assignment supersedes an older not-regression assessment.
The shared pattern check includes the entire first-bad-version family and
applies both to adding not-regression and to removing a suspected-regression
label through that assessment. Re-adding needs fresh qualifying evidence.
Try-latest recommendations must bind the instruction to one concrete MAUI version
and quote that version. The target must be present in the prepared published
release window and strictly newer than the report's unambiguous `Version with
bug` field, considering any higher named MAUI version in the author's prose.
An absent/ambiguous baseline, downgrade/equal version, multiple targets or an
unestablished publication cannot activate the feedback/closure policy.
Version comparison includes numeric core, preview/RC/stable phase and iteration;
build variants within the same preview/RC iteration are not treated as upgrades.
Short preview/RC names and exact package builds are accepted only when backed by
the release's framework heading or same-family Controls package links. A bare
release tag without either a recognized framework heading or a matching Controls
package link cannot establish the framework version. Workload-set tags are not
substituted for framework package versions.
An authoritative "expected behavior/by design" explanation or
"duplicate of #..." disposition is also recognized for its respective label.
Expected-behavior and duplicate dispositions must be affirmative, not tentative
or questions. Duplicate decisions identify exactly one canonical target via
"duplicate of #N" or a full same-repository issue/PR URL and cite that exact
fetched related source; an unrelated fetched reference is not sufficient.
Newer opposite maintainer decisions or label events supersede earlier approvals;
an old approval cannot silently undo a later manual removal.
The same rule applies to confirmation labels: cite a newer positive confirmation
or a later explicit maintainer re-add rather than reversing a newer removal.
The confirmation gate and later-contrary-evidence veto share unsuccessful
reproduce/confirm/verify/validate detection. The veto withholds confirmation;
it does not turn an unsuccessful validation or infrastructure failure into
`s/no-repro`.
Negation must grammatically modify a validation verb, with bounded intervening
auxiliaries/adverbs; an unrelated "not a duplicate" clause cannot negate a
following successful verification. The full cited comment is checked for contrary
validation, not just the selected paragraph. Mixed validation/retraction comments
are conservatively withheld until a fresh unambiguous confirmation is supplied.
Positive reproduction must concern the reported issue/behavior, not merely
running the sample. Postposed negation of that outcome also vetoes confirmation
and participates in the later-contrary-evidence scan.
Explicit target wording such as "this issue" or "the reported behavior" is
required. A bare issue/bug/problem mention or reproduction of another, different
or unrelated outcome cannot establish confirmation. The cited paragraph is
checked, so an affirmative-looking quote cannot hide such a qualifier.
Not-regression assessments must also be unconditional and non-tentative:
"This is probably not a regression" cannot supply a definitive disposition or
remove potential-regression through that transition.
Regression confirmation additionally requires explicit working behavior tied
to an earlier named .NET/MAUI version and failing/reproducing behavior tied to a
later named version. Lists of tested versions do not establish their outcomes.
The first-bad citation must include both results and the explicit boundary
matching that label, with no earlier contradictory failing outcome; merely
asserting "regressed from" does not establish earlier passing behavior.
Preview and RC ordering is recognized; generic build success is insufficient.
Unknown or ambiguous outcome/version bindings are conservatively withheld.
Generic reproduction on a platform/OS version is not proof of regression.
This gate also applies to suspected-to-confirmed transitions and first-bad-version
labels, whose exact version citation must come from authorized regression evidence.
Contrary comment edits use their last-modified time; immutable label events use
their creation time. Affirmative support retains its original creation time so a
cosmetic edit cannot revive an old approval or confirmation after a later
revocation. Supply a fresh affirmative comment when that chronology is ambiguous.

Corrections are deltas, never whole-label replacement. Allow explicit maintainer
removals and narrowly supported pending-to-validated, suspected-to-confirmed,
root-cause area, disputed-workaround and device-to-simulator transitions. At most
one dominant area is removed and two independently supported areas added.
Root-cause area corrections need a maintainer comment created after the latest
assignment of the removed area, or a current explicit removal decision.
Cosmetic edits cannot revive an older correction after a newer assignment.
The `needs-area-label` placeholder can be cleared using issue/comment evidence
from an area addition validated in the same proposal. Current report content can
predate the initial placeholder event; that transition does not require a newer
validator comment. Existing area membership alone is not sufficient.
Priority changes cannot leave conflicting priorities. Preserve unrelated manual
labels, Policy Service staleness tags, release/automation outcomes, legacy names
and unknown labels. Uncertain decisions are withheld with an explanation.

**Feedback side effects:** needs-info/repro/try-latest-version participate in
Policy Service replies, staleness reminders and potential automatic closure.
Adding these labels is not merely visual categorization. The command requires an
actionable request and explains those effects in its result.

## Evidence behind the policy

The rules were derived from process documents, issue forms, Policy Service,
the live label catalog and chronological human/bot label events. Representative
cases illustrate why content, confirmation and product decisions are separate:

| Issue | Observed distinction |
| --- | --- |
| [#37281](https://github.com/dotnet/maui/issues/37281) | A presumed CollectionView regression was traced to shadows/drawing; area correction followed root-cause investigation. |
| [#38925](https://github.com/dotnet/maui/issues/38925) | A partner validator tested 10.0.90, 10.0.100 and 10.0.110 and reproduced the Android issue from 10.0.100. This is authorized reproduction, but the tested-version list alone does not explicitly establish a working 10.0.90 outcome for the stricter regression gate. |
| [#39017](https://github.com/dotnet/maui/issues/39017) | Verification across versions and later Syncfusion collaboration are separate facts; the reporter need not be a partner. |
| [#35965](https://github.com/dotnet/maui/issues/35965) | Unsuccessful reproduction and a concrete sample request preceded feedback/staleness handling. |
| [#37275](https://github.com/dotnet/maui/issues/37275) | Verified observed behavior was subsequently explained as not-a-bug: those labels are not necessarily contradictory. |
| [#27667](https://github.com/dotnet/maui/issues/27667), [#29588](https://github.com/dotnet/maui/issues/29588) | Priority was added in release/servicing planning, not mechanically derived from initial content. |
| [#36233](https://github.com/dotnet/maui/issues/36233) | A reported regression with failed reproduction remained a potential regression. |
| [#35401](https://github.com/dotnet/maui/issues/35401), [#37440](https://github.com/dotnet/maui/issues/37440) | Later simulator reproduction and a disputed workaround can contradict existing facet labels. |
| [#35498](https://github.com/dotnet/maui/issues/35498) | Memory-leak classification had retained-object/measurement evidence, not merely a crash. |
| [#35448](https://github.com/dotnet/maui/issues/35448) | Maintainer-attributed label events were explicitly automation evaluation; copying them would reproduce bad triage. |

## Runtime and safety

The workflow runs trusted default-branch code, checks current caller permission
before analysis and publication, and verifies the exact source-comment identity.
PR comments, edited/quoted/fenced commands, other `/issue` subcommands, closed
issues, other repositories and non-default infrastructure refs cannot publish.
An unauthorized/incomplete pre-activation prevents PAT pool selection and GPT.

Trusted preparation captures the complete bounded chronology, label events,
all label definitions and same-repository linked references. Bounds are 200
comments, 600 events, 1,000 labels, eight related references, 60 authority lookups
and a 1 MiB context file. Exceeding them fails visibly rather than discarding later
contradictory evidence. Related issue/PR titles and bodies are fetched; their code,
archives and nested discussions are not executed or recursively expanded.
Trusted intake also reads a fixed 20-record window of official GitHub releases
and retains published, non-draft framework version metadata in
`publishedMauiReleases`, not in authority evidence. The window is explicitly
bounded, not a complete release/support-lifecycle catalog; unlisted targets are
withheld. It is re-fetched and hashed at publication like the other context.
Request/API failures propagate rather than inventing a latest release.
References are extracted outside Markdown code with a full numeric token boundary.
Three/four/six/eight-digit color-shaped shorthand, including `#3333` and `#333333`,
requires explicit issue/PR wording;
ordinary `Fixes`, `Closes`, `Resolves`, `See` and related-reference wording also
qualifies, including three-digit issue numbers. A full same-repository issue URL
is unambiguous.
Nonexistent/inaccessible referenced items produce a warning and no related
evidence instead of aborting otherwise valid triage. A missing canonical source
still cannot authorize a duplicate disposition. Authentication, rate-limit and
server failures retain the normal retry/failure behavior rather than being skipped.
Markdown handling uses the parser already bundled with PowerShell's
`ConvertFrom-Markdown`; no additional package or runtime is installed.

One GPT-6.1 Sol/Copilot analysis reads prepared evidence and the declared local
skill. Shell and GitHub tools are disabled. It has read-only repository
permissions and does not receive the publication job's write token.
Known automation accounts are excluded from both maintainer and validator
authority even when GitHub reports their account type as `User`, including the
write collaborator `vs-mobiletools-engineering-service2`. The explicit denylist
also guards the registered-validator path; account type or write access alone
cannot establish human evidence. Their label events remain chronology facts,
not approvals.

The separate safe-output job checks out the exact trusted revision, imports only
bounded regular JSON outside the checkout, binds it to the preparation job's
independent context hash, and re-fetches/rechecks context and authority. It rejects
stale context, fabricated quotes, wrong targets, unsupported labels, inconsistent
intents and unsafe transitions, then renders its own comment.
Withheld labels must be unique and cannot overlap the proposed label delta.
The final authorization clears the permission cache and rechecks both the
original command actor and any rerun actor, rather than reusing intake permissions.
Only built-in gh-aw add/remove/comment handlers perform writes.
Safe-output publication explicitly uses the built-in `secrets.GITHUB_TOKEN`,
consistently posting as `github-actions[bot]`; it does not select a configured
`GH_AW_GITHUB_TOKEN` publisher. Retry-marker recognition uses that same identity.
Their coarse family allowlists meet the compiler's 50-entry bound; the narrower
machine policy and exact live-name checks are mandatory before those handlers.

**Known permission limitation:** gh-aw v0.86.2's generic `remove-labels` handler
adds `pull-requests: write` to publication jobs and has no issue-only permission
switch. Exact issue-type/target checks constrain reachability but do not narrow
that credential. This PR does not upgrade repository-wide compiler tooling or
introduce a custom write handler; review this remaining least-privilege limitation
before apply-mode deployment.

Each run permits at most 20 total label changes, ten removals and one result
comment. Per-issue concurrency does not cancel an in-progress publication.
A genuine no-change result uses `noop` and leaves the issue untouched. Missing
required evidence is incomplete, not a successful review; inspect the Actions
result rather than interpreting the absence of a comment as success.
Label/comment APIs are **not an atomic transaction**: inspect the Actions result
for actual application, particularly after a partial failure. The explanation
describes a validated requested delta, not an unconditional delivery claim.
Rerun all jobs to gather current state and reconcile remaining deltas. Each
rendered decision carries a canonical fingerprint of its action, label,
reason, exact evidence and request. Retrying the same invocation suppresses a
duplicate comment only when every remaining decision exactly matches one in its
original report. Decision/evidence ordering and changing context hashes do not
prevent an otherwise identical partial retry.
A changed/new decision or a legacy report without matching fingerprints fails
before publication and requires a fresh command or manual dispatch, rather than
applying a different delta under an old explanation. Multiple reports for one
invocation also fail visibly. The original report remains a historical requested
plan; the workflow result records the actual remaining application.
Rerunning only a failed publication job after partial writes intentionally fails
the stale-context check.

## Deployment and staged operation

Merge the source, compiled lock, policy, script and skill to the default branch.
The workflow uses the existing `copilot-pat-pool` environment and `COPILOT_PAT_0`
through `COPILOT_PAT_9` configuration. No new Azure pipeline, OIDC registration,
external gateway, schedule or write PAT is needed.

The complete pool job is conditionally redeclared because gh-aw v0.86.2 replaces
imported job definitions instead of merging an added `if`. This preserves the
existing environment/token/output contract while withholding pool secrets until
trusted intake succeeds. Other workflows and the shared pool are unchanged.

Start with manual dispatch on an authorized designated canary:

```bash
gh workflow run issue-triage.lock.yml --repo dotnet/maui --ref main \
  -f issue_number=ISSUE_NUMBER -f staged=true
```

Manual dispatch defaults to staged mode. It performs normal proposal validation
and retains `issue-triage-context-*` and `issue-triage-report-*` artifacts for
seven days, but built-in handlers suppress label and comment writes.
Omitting `issue_number` is rejected, even though gh-aw requires the dispatch
input itself to be declared non-required for slash-command compatibility.
Use `staged=false` only for intentional application; `/issue triage` comments
are apply-mode commands. New workflows are not dispatchable until recognized
on the default branch, so local compilation is not end-to-end hosted validation.

After source changes, regenerate only this workflow:

```bash
gh aw compile issue-triage --strict --no-check-update
```

Commit source and lock together. Parse PowerShell and JSON and lint the generated
workflow with an actionlint version supporting its compiler-generated concurrency
fields. No pipeline snapshot/Pester/regex tests are part of this feature.
With actionlint 1.7.12, the compiler-generated `queue: max` field is not yet
recognized. Other diagnostics can still be checked without editing the lock:

```bash
actionlint -ignore 'unexpected key "queue" for "concurrency" section' \
  .github/workflows/issue-triage.lock.yml
```
