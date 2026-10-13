# `/issue triage`

Post exactly `/issue triage` as a new comment on an **open issue** in
`dotnet/maui`. The caller must currently have write, maintain or admin permission.
Inherited repository write access from the
[`dotnet/maui-external-partners` team](https://github.com/orgs/dotnet/teams/maui-external-partners)
satisfies this requirement; no public team membership, admin grant or new token
is required. See the [command access inventory](pr-review-workflow.md#command-access-and-other-entrypoints)
for the other repository-owned commands and their target restrictions.
The command proposes and applies evidence-backed manual issue labels and retains
its explanation only in Actions artifacts, without posting a public triage report.
Separate Policy Service replies triggered by feedback labels are unchanged.
After successful validation and native safe-output processing, the exact
authorized triggering slash-command comment collapses as **resolved**, including
successful no-change and withheld-only results. A separate trusted completion job
rechecks the open issue, unchanged human command and caller's current write access.
It also requires every trusted validated addition to be present and every removal
to be absent in the freshly fetched issue's exact canonical label names, not just
successful native operation counts. This is a point-in-time postcondition check:
malformed or unmet label state detected in that fetched snapshot keeps the command
visible without changing any labels. The subsequent command checks and separate
minimization mutation are not atomic with the label read; a concurrent label change
after that snapshot can still occur before the command collapses.
Failed, cancelled, deferred, skipped, warning, partial or incomplete outcomes leave
the command visible. Manual dispatches (including staged runs), edited/replayed
commands, bot comments and unrelated comments are never minimized. A minimization
API error is reported in Actions without rolling back successful labels or posting
a report; explanations remain artifact-only.
It does not reproduce bugs, run samples, change milestones or
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
Decisions require a clause-opening directive/first-person action, a directly
stated label decision, or a current-target affirmative canonical predicate.
A bare declarative canonical disposition at a clause opening is also eligible;
an "Expected behavior:" field heading is not a disposition. Phrase
proximity is insufficient: "The request to add p/1 remains open" is not an
instruction, and "The reported result differs from the expected behavior" is
not a not-a-bug disposition. Unrecognized wording remains withheld.
Direct action statements must finish their declarative clause after the exact
label, optionally with "now" or "immediately". "Apply p/1 later" and
"Remove p/1 tomorrow" are not current decisions; a comma/colon continuation or
other unrecognized tail cannot be discarded to turn them into one. Put a reason
in a separate sentence rather than relying on an incomplete action prefix.
The same completeness rule applies to first-person actions and supersession;
existing directly stated label decisions and directed-veto rules are unchanged.
Clause-opening "I can confirm" or "We can confirm" is affirmative only when it
directly governs a recognized exact-label action or canonical disposition.
"I can confirm: Apply p/1." is eligible; "I can confirm the previous comment
says 'Apply p/1'." is not. Other modal wording such as "We can apply p/1",
negation, uncertainty, conditions and questions remain withheld; embedded or
reported confirmation wording does not receive this exception.
Decision-only filtering excludes natural quoted commands/dispositions and
unmatched quotation tails from actions and vetoes. Quoting only the exact label
preserves a directive outside those quotes. Original paragraph-wide question,
conditional and uncertainty checks remain, including raw qualifiers in a bound
veto. The same quote filtering feeds canonical-target derivation and both
supersession scans; factual validation rules are unchanged.
Unsuccessful actions such as "I couldn't apply p/1", "we were unable to apply
p/1" and "we failed to remove p/1" cannot authorize either action or supersede
an older affirmative decision. The final polarity gate uses the shared
negative-outcome vocabulary for ordinary and canonical dispositions.
"Neither" and "nor" likewise withhold decisions before canonical acceptance:
"This is neither duplicate of #N nor expected behavior" approves neither label.
The positive "not a bug" disposition remains recognized.
Exact label token boundaries apply to approvals and superseding decisions:
`Apply partner/syncfusion` does not also approve `partner`.
All label decisions and both actions share non-question and affirmative-polarity
checks with their supersession scan. "Do not remove p/1" and "Should we remove
p/1?" cannot revoke "Apply p/1"; a directed "Do not apply p/1" can veto it.
Indirect "we discussed whether to apply p/1" inquiries are not decisions either.
Conditional/timing-contingent decision paragraphs cannot authorize additions,
removals or superseding reversals. "Apply p/1 if the regression is confirmed" is
not a current priority commitment, nor is "Remove p/1 once validation is complete"
a completed revocation. Ambiguous mixed paragraphs are withheld; cite a separate
unconditional decision rather than inferring that later evidence activated a
previous condition.
Directed prohibitions retain the existing conservative veto on older authority:
"Do not apply p/1 until validation is complete" still blocks an older approval.
The prohibition must begin an imperative sentence or clause, optionally with
"please". Markdown unordered/ordered list prefixes do not hide that opening,
including list items following other prose on a new line.
Embedded hypothetical negations such as "If we do not apply p/1"
are not directed vetoes. Tentative prohibitions such as "Maybe do not apply p/1"
remain withheld; the directed-veto exception never bypasses uncertainty.
For a recognized directed veto, uncertainty is checked in its label-bound
imperative clause, including trailing qualifiers, rather than borrowing it from
a separate explanatory clause. "Do not apply p/1; I am not sure this is a
regression" still vetoes an older approval; "Do not apply p/1, maybe" does not.
This scope is veto-only: affirmative decisions and factual evidence retain
their paragraph-wide uncertainty checks.
The pre-label gap and post-label capture share their punctuation and contrastive
boundaries: "but", "however", "instead" and "rather than". "Do not apply p/1, but
maybe apply p/2" retains the p/1 veto without approving p/2. A comma alone still
retains a trailing qualifier such as "Do not apply p/1, maybe".
Explanatory negation such as "this is not ready" or "we cannot commit" does not
cancel a recognized directed veto. Questions remain non-decisions.
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
Each prepared source includes its unmodified `body` and a citation-safe `prose`
projection computed by the trusted `Get-Prose` routine. Filtering blanks forbidden
spans without changing string length or line breaks. Choose contiguous verbatim
quotes present in both `source.body` and `source.prose` with exact ordinal matching;
prefer short complete prose sentences that support the label. Never strip markup
or rewrite a quote, or cite across blanked code, quoted text or link-metadata spans.
If eligible evidence is unavailable, withhold the label instead of emitting
malformed evidence. The projection is a citation aid only, not proof of authority
or semantic support; provenance, policy and whole-paragraph contradiction checks
remain mandatory. Final validation recomputes prose from the freshly fetched
original body and checks exact containment in both strings, never trusting an
agent-supplied projection.
Automatic area correction requires a fresh unconditional maintainer explanation
of this reported issue's cause naming the exact replacement area. The removal
and that specific addition must cite the same source and exact quotation.
The cause paragraph is checked against the full comment's issue-reference
context. A foreign-target explanation cannot correct this issue; references in
another paragraph require the cause paragraph to explicitly identify this report.
Negated, tentative, unrelated or mixed explanations and an independently cited
area addition cannot justify deleting an existing area. If that narrow gate
cannot establish the correction, preserve it or cite a current explicit
maintainer removal. Unrelated secondary areas remain protected.
Cause-only correction requires exactly one area in the freshly fetched current
labels. Multiple current areas require an explicit current maintainer removal
decision; counting proposed effective labels would allow removals to authorize
themselves and is not permitted.
Identified result comments from this workflow's publisher are excluded from
evidence sources and related-reference intake. They remain in `resultComments`
for context hashing and retry reconciliation only. Generated report reasons
cannot become fresh facts after the original evidence is edited or removed.
Suggestions and tentative candidates are not approvals.
Tentative validation such as "should be reproducible on Android" is not an
observed reproduction and cannot support confirmation labels. Confirmation uses
the shared tentative-evidence gate, including "I think I reproduced this issue"
and "apparently reproduced", for additions and confirmation-based removals.
Explicit "unsure", "not sure" and "not certain" wording also withholds
confirmation and maintainer approvals, including negative copula contractions
and bounded certainty modifiers. "I am not sure I reproduced this issue"
is not an observed confirmation. Quoting an affirmative-looking fragment cannot
hide the enclosing uncertainty; the existing paragraph-level gate is unchanged.
The shared enclosing-claim denial guard rejects "I cannot say I reproduced this
issue" and "There is no evidence that this behavior worked in MAUI X and fails in
MAUI Y". These are neither reproduction nor demonstrated regression boundaries.
The same guard covers technical assessments, failed-workaround transitions and
superseding evidence. Denied claim spans stop at sentence/paragraph or contrastive
clause boundaries; masking them cannot erase separate observed positive results
that veto a mixed-result no-repro assessment.
Conditional outcomes such as "If the issue is reproduced on Android, collect
logs" cannot supply observed confirmation, even when the citation omits the
conditional prefix. The shared positive-evidence predicate applies to confirmation
additions, removal transitions and contrary positive results. It distinguishes
contingent outcomes from factual reproduction scenarios such as "I reproduced
the issue when the keyboard was visible".
Generic "confirmed"/"verified" wording directly governs the reported target,
optionally with bounded observation modifiers, and ends there or continues with
a recognized test environment. "MacCatalyst" and "Mac Catalyst" name the same
environment at both the target-bound observation and authorized-source gates.
Verifying the version in this issue or this
issue's title/state is metadata checking, not observed reproduction. The generic
matcher cannot borrow its target across the verified property's description.
Ambiguous generic continuations are withheld; the explicit target-bound
reproduction alternatives remain unchanged. This shared distinction also covers
confirmation-based removals, simulator evidence and positive no-repro counterevidence.
Negative-evidence vetoes must also
concern the current report: failed reproduction of a foreign issue cannot invalidate current-issue
confirmation, simulator reproduction or technical assessments. The full comment
is still scanned for current-report negative outcomes, including repeated
paragraphs; metadata-only foreign references remain scope constraints.
Subject-negated outcomes such as "no one reproduced this issue" and
"nobody verified this issue" share negative-evidence handling and cannot
become positive validation. Only those negative spans are masked for positive
matching, so separate positive outcomes still block mixed-result no-repro claims.
Technical-state labels require their specific affirmative review, reproduction
outcome or version recommendation, not merely an authorized comment author.
Maintainer directives, technical assessments and failed-workaround observations
are target-scoped. A paragraph about a foreign issue cannot authorize this
issue's labels; foreign-reference context elsewhere in the comment requires an
explicitly current-report supporting paragraph. Mixed-reference paragraphs are
withheld rather than guessing which report an action concerns. A fetched
canonical duplicate reference is permitted only as the bound "duplicate of"
relationship, not as another action target.
Deleted Markdown/HTML text is excluded from evidence, including strikethrough
and nested or unclosed deletion markup. Adjective hedges such as "possible
duplicate" and "looks like expected behavior" cannot supply definitive
dispositions.
Technical-assessment questions are rejected. No-repro is withheld when the cited
comment reports resource access, build/download, authentication/network failure or
timeout that prevented testing. A historical setup failure is not itself no-repro.
An emulator/simulator or test runner/host that could not start, boot or connect
is also a setup blocker, not an observed product outcome. Product app startup
wording alone does not acquire this environment-specific blocker.
The outcome phrase "run into" is not an environment operation failure;
it does not itself prove no-repro or bypass any other validation guard.
Noun-first failures such as "Setup failed" and contextual failures such as
"The workaround failed during setup" are blocked setup evidence, not observed
product or workaround outcomes. Operation/failure binding is checked in both
directions; unrelated successful setup wording does not establish a blocker.
It can be discharged only by an explicit resolution followed by a completed
target-specific test and unsuccessful outcome in the same supporting paragraph.
That resolution can name the repaired environment; a negated repair does not
discharge the blocker.
Every blocker must qualify; unresolved blockers elsewhere still withhold no-repro.
Not-regression cannot use a negated same-behavior comparison.
Factual "could not" and "couldn't" reproduction/replication failures share the
same narrow normalization before tentative classification, including straight
and curly apostrophes. The original outcome, target, resource-failure,
conditional, uncertainty and mixed-result checks remain required.
No-repro uses the observation-oriented conditional gate, so completed
post-update or post-rebuild test context is not mistaken for a pending decision.
Explicit lack of attempts, untested samples and pending prerequisites are not
unsuccessful completed tests. Current-report qualifiers anywhere in the cited
comment veto no-repro, including inability "until" a prerequisite is fulfilled
or waiting for a sample. This does not restore a blanket timing-word veto.
Questions remain withheld, but auxiliary/subject wording must begin a clause:
declarative "I have this issue" does not turn a factual reproduction into a question.
Indirect "we discussed whether this issue was reproduced" is not validation.
Factual "this issue was reproduced whether or not X is enabled" remains eligible;
an inquiry about whether or not an outcome occurred is not an observation.
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
bug` field, considering higher MAUI versions in the author's prose. Bare values
in explicitly MAUI-scoped headings, fields and table columns/rows participate;
explicit OS, SDK and tool versions do not. An unqualified higher version makes
the baseline ambiguous and withholds the request rather than being silently
ignored as unrelated.
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
or questions. Categorical "expected behavior" is not a tentative expectation;
qualifiers such as "maybe expected behavior" still withhold that disposition.
Duplicate decisions identify exactly one canonical target via
"duplicate of #N" or a full same-repository issue/PR URL and cite that exact
fetched related source; an unrelated fetched reference is not sufficient.
Newer opposite maintainer decisions or label events supersede earlier approvals;
an old approval cannot silently undo a later manual removal.
Compounded decisions such as "Approve removing LABEL" or "Decline removal of LABEL"
cannot authorize either action. Their label-bound action is not the outer approval
verb; they conservatively veto stale support until an unambiguous decision is cited.
Postposed rejection, denial, cancellation, withdrawal, revocation or "ruled out"
bound directly to an exact label or its special disposition also cannot
authorize an action. "The request to apply p/1 was rejected" is not an approval;
"Duplicate of #N was ruled out" is not an affirmative duplicate disposition.
Directly bound "false", "incorrect", "inaccurate", "untrue" and "wrong"
assessments use the same rejection gate. "Duplicate of #N is false" and
"Expected behavior is incorrect" cannot authorize their canonical labels.
A qualified "Duplicate of #N was revoked" also vetoes an older duplicate
addition without repeating the label. The supersession exception is bound to
the exact fetched canonical target in the original cited decision paragraph,
not another related source elsewhere in its evidence. Both other paragraphs
in that maintainer comment and later maintainer comments are checked.
A positive canonical restatement is not a removal; this veto does not
authorize an actual removal. Ordinary explicit label-removal rules are unchanged.
The same directly bound veto covers coordinated review predicates such as "was
reviewed and rejected" and "was considered and then declined", with bounded
disposition modifiers. An embedded "apply" verb cannot authorize the rejected
request. The predicate does not cross arbitrary intervening prose or borrow
another subject's rejection.
Apply the same veto before accepting ordinary actions, canonical duplicates or
expected-behavior explanations, including in the supersession scan. Do not
borrow a rejection from a later unrelated clause.
Equal-second contrary evidence is also superseding because GitHub timestamps
cannot reliably establish its order. A fresh affirmative comment must follow
the contrary evidence, not merely share its timestamp.
The same rule applies to confirmation labels: cite a newer positive confirmation
or a later explicit maintainer re-add rather than reversing a newer removal.
The confirmation gate and later-contrary-evidence veto share unsuccessful
reproduce/confirm/verify/validate detection. The veto withholds confirmation;
it does not turn an unsuccessful validation or infrastructure failure into
`s/no-repro`.
Denied regression history does not retract an independent current reproduction.
Reproduction confirmation uses reproduction-specific claim denials; regression
confirmation and first-bad decisions also retain the broader history-denial veto.
A later denial of a WebView2-regression assessment uses the broader veto too;
it supersedes the assessment without retracting independent reproduction.
Confirmation-based removal transitions use the same freshness gate as additions,
even when s/verified is already present. Later contrary validation or a newer
maintainer removal cannot be bypassed to clear pending information/reproduction
labels. Affirmative transition support also keeps its creation time when compared
with the latest request assignment; cosmetic edits cannot revive older support.
Negation must grammatically modify a validation verb or the reported-outcome
predicate, with bounded intervening auxiliaries/adverbs; an unrelated
"not a duplicate" clause cannot negate a following successful verification.
Confirming that this issue is fixed/resolved or no longer occurs/fails does not
confirm reproduction. Such outcomes veto stale confirmation without independently
authorizing no-repro or blocking a legitimate completed try-latest response.
The full cited comment is checked for contrary
validation, not just the selected paragraph. Mixed validation/retraction comments
are conservatively withheld until a fresh unambiguous confirmation is supplied.
Positive reproduction must concern the reported issue/behavior, not merely
running the sample. Postposed negation of that outcome also vetoes confirmation
and participates in the later-contrary-evidence scan.
Explicit target wording such as "this issue" or "the reported behavior" is
required. A bare issue/bug/problem mention or reproduction of another, different
or unrelated outcome cannot establish confirmation. The cited paragraph is
checked, so an affirmative-looking quote cannot hide such a qualifier.
When the cited comment references a foreign issue/PR, its validation must bind
the outcome explicitly to the current report, using current-issue wording or
the current issue's exact reference. "That issue" cannot carry confirmation
from the referenced report. This shared target gate also applies to simulator
transitions and contrary positive outcomes; reference metadata is a conservative
scope check, not affirmative authority.
Not-regression assessments must also be unconditional and non-tentative:
"This is probably not a regression" cannot supply a definitive disposition or
remove potential-regression through that transition.
They must describe the current/reported issue or its same behavior on older
explicitly named .NET/MAUI versions or releases. Older devices/OS versions or
unqualified "earlier" wording cannot establish prior framework behavior.
A foreign subject such as "the other issue" cannot supply the
disposition or removal transition. The full cited comment is checked for
foreign-outcome qualifiers, so selecting another paragraph cannot hide them.
The same unconditional, non-tentative requirement applies to completed-review
and no-repro assessments. No-repro requires an unsuccessful outcome for the
reported issue, not "Do not reproduce this issue" or a hypothetical attempt.
Factual "I could not reproduce this issue" remains eligible, subject to the
existing authority, infrastructure and freshness checks.
Factual "failed to reproduce/replicate" and "fails to reproduce/replicate" are
also unsuccessful outcomes, not a bare imperative to avoid reproduction.
Mixed unsuccessful/successful validation cannot establish no-repro, including
an initial failure followed by reproduction in the same paragraph. Positive
outcomes elsewhere in the same cited comment also veto the assessment; the
negative-outcome gate cannot hide that counterevidence.
Regression confirmation additionally requires explicit working behavior tied
to an earlier named .NET/MAUI version and failing/reproducing behavior tied to a
later named version. Lists of tested versions do not establish their outcomes.
Explicit fails/failed outcomes need not also say reproduced/confirmed/verified.
"Fails to reproduce/replicate" describes unsuccessful validation, not a failing
reported behavior, and cannot confirm a regression.
Negated failures such as "no longer fails", "doesn't fail" or "can't reproduce"
cannot supply a failing framework version or first-bad boundary.
A directly continued "worked in MAUI X and fails starting in MAUI Y" statement
can retain its subject; the version/outcome, scope and uncertainty guards remain.
Version/outcome binding cannot cross an "and/or" clause into a separate subject's
outcome; explicit repeated-subject clauses keep their own working/failing versions.
The same regression confirmation supersedes older no-repro/not-regression
assessments and supports suspected-to-confirmed transitions.
WebView2-regression assessment requires an affirmative assertion concerning this
issue, not merely both keywords in a reproduction comment. An explicit current
negative WebView2 classification also supersedes older support; quote selection
cannot hide it elsewhere in the cited comment.
"This issue is a regression caused by WebView2" and "due to WebView2" are causal
classifications with the same current-target and affirmative/negative safeguards.
The first-bad citation must include both results and the explicit boundary
matching that label, with no earlier contradictory failing outcome; merely
asserting "regressed from" does not establish earlier passing behavior.
Neither "regressed from" nor bare "from" establishes the first bad version;
outcome/version binding cannot bridge a "regressed from" baseline as failure.
The full enclosing cited paragraph is checked for that boundary; a selected
quote cannot hide an earlier failing version.
The same exact-first-bad-version confirmations supply the creation-time
freshness and later contrary-validation/removal checks. A newer generic
regression confirmation or one for another boundary cannot lend recency to
older exact-version evidence. The existing strictly newer positive-confirmation
or explicit later maintainer re-add alternative is unchanged.
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
Failed-workaround transitions require an unconditional, non-tentative observed
failure in one enclosing paragraph and in the selected quote. Questions,
conditional advice and mixed working/failing outcomes are not such observations.
Workaround failures use the observation-oriented conditional gate: completed
post-update or post-rebuild context is not mistaken for a deferred decision.
A timing condition directly bound to a workaround-failure outcome is still
hypothetical; failure advice cannot authorize removal.
Intervening prospective confirmation does not establish an observation:
"Once we confirm the workaround does not work" remains deferred evidence.
A trailing "until", "before", "pending" or waiting prerequisite bound to the
failure is not unconditional efficacy evidence either. These prerequisites are
distinct from completed "after updating" or "after rebuilding" context.
Bind the trailing condition within the failure predicate, not across an unrelated
aside or follow-up clause. A categorical failure followed by
"before I forget, the repro logs are attached" or a dash-separated
"pending a proper fix, I reverted the change" is still observed failure.
A comma can introduce a bound "until" prerequisite; it does not bind arbitrary
"before" or "pending" discourse back to the efficacy outcome.
Deferred confirmation/testing or application/setup steps immediately introduced
by a comma, semicolon, colon, opening parenthesis or dash still qualify as
unfinished prerequisites: "The workaround does not work: pending validation"
and "The workaround does not work (pending validation)" are not observed
efficacy failures. Keep the prerequisite and its step within the same bounded
clause; do not borrow that step from a later aside or follow-up clause.
Failure to reproduce, replicate or trigger the issue with a workaround is not
failure of the workaround. Such a paragraph cannot support failure-based
removal, while observed failure to work or fix the issue remains eligible.
Setup, build/download/install, network and infrastructure failures cannot supply
workaround-efficacy evidence. A bound "failed to ..." must concern working,
fixing, resolving or helping, not a different attempted operation; inspect the
whole cited paragraph rather than borrowing a later failure word.
Positive continued clauses remain visible even when they omit the workaround
subject: "The workaround failed initially, but now works" is mixed evidence,
not authority to remove the label.
Bind success to the workaround subject or a directly inherited/pronominal
continuation. Unrelated "Working with the author", "Fixed the sample link" or
"Helped the author collect logs" sentences are activities, not workaround efficacy.
"Suggested workaround" names the attempted workaround, not a tentative failure;
uncertainty about the outcome still withholds the transition.
Device-to-simulator transitions require affirmative, unconditional, non-tentative
reproduction of the reported issue on a simulator in both the quote and enclosing
paragraph. Questions and unsuccessful attempts are not contradictory evidence;
a device reproduction followed by "but not on the simulator" is not sufficient.
Contrary validation elsewhere in the cited comment also vetoes this transition.
Workaround/device-only contradiction evidence must be strictly later than the
latest assignment when that event is available. An equal-second observation
cannot authorize removal because its order relative to the assignment is unknown.
Root-cause area corrections need a maintainer comment created after the latest
assignment of the removed area, or a current explicit removal decision.
Cosmetic edits cannot revive an older correction after a newer assignment.
The `needs-area-label` placeholder can be cleared using issue/comment evidence
from an area addition validated in the same proposal. Current report content can
predate the initial placeholder event; that transition does not require a newer
validator comment. Existing area membership alone is not sufficient.
The removal must cite at least one exact source/quote pair from that validated
addition; a separate unrelated citation cannot stand in for its authority.
Priority changes cannot leave conflicting priorities. Changes to status states
cannot leave verified/needs-verification, verified/no-repro, suspected/confirmed regression or
not-regression/regression states active together. No-repro also conflicts with `i/regression`,
`blazor-webview2-regression` or any `regressed-in-*` label. These families
also supersede older no-repro assessments. A first-bad-boundary change
must leave at most one `regressed-in-*` label active, not accumulate differing
boundaries. Every necessary removal still
requires its own permitted authority or transition; otherwise withhold the
change. Unrelated pre-existing conflicts are preserved, not silently cleaned up.
The verified/pending-verification pair is incompatible for changes in either
direction. A later verification request cannot be cleared by stale confirmation;
without independently valid removal evidence, withhold the conflicting change.
Preserve unrelated manual labels, Policy Service staleness tags,
release/automation outcomes, legacy names and unknown labels.
Uncertain decisions are withheld with an explanation.

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
or `dotnet/maui#N` reference is unambiguous. Intake and validation use the same
reference grammar; other repositories' qualified references are not fetched
as dotnet/maui issues. A qualified canonical duplicate still needs the existing
explicit maintainer disposition and exact fetched related source.
The same color-aware classification is shared with validation scope and
positive-outcome matching; numeric colors are not foreign-issue references.
Explicit issue/PR wording, repository-qualified references and full URLs still
constrain the target.
Nonexistent/inaccessible referenced items produce a warning and no related
evidence instead of aborting otherwise valid triage. A missing canonical source
still cannot authorize a duplicate disposition. Authentication, rate-limit and
server failures retain the normal retry/failure behavior rather than being skipped.
Markdown handling uses the parser already bundled with PowerShell's
`ConvertFrom-Markdown`; no additional package or runtime is installed.
Issues, comments and related reports can retain a null author after account
deletion. Intake emits a warning and records an empty, non-authoritative login
instead of failing strict-mode property access. Such comments cannot acquire
maintainer/validator authority; a source command without an author is rejected.

One GPT-6.1 Sol/Copilot analysis reads prepared evidence and the declared local
skill. Shell and GitHub tools are disabled. It has read-only repository
permissions and does not receive the publication job's write token.
It declares final intents through the exposed safeoutputs MCP tools, not shell
CLI/schema probes. Structured triage data belongs to the comment tool, which
remains declared solely as internal typed evidence transport. Trusted validation
always strips this intent before native publication, including withheld-only results.
The pinned runtime automatically attaches an `aw_` plus eight-alphanumeric
comment temporary ID; the validator accepts only that known transport shape and
discards it before publication. Numeric target, evidence and incomplete-output
checks remain mandatory; a mixed missing-tool/label proposal is still rejected.
Known automation accounts are excluded from both maintainer and validator
authority even when GitHub reports their account type as `User`, including the
write collaborator `vs-mobiletools-engineering-service2`. The explicit denylist
also guards the registered-validator path; account type or write access alone
cannot establish human evidence. Their label events remain chronology facts,
not approvals.

The separate native threat detector is instructed to perform its full analysis
without delegation and emit one final verdict with all three boolean flags and a
`reasons` array, including `[]` when empty, matching the existing regression-trace
workflow's format contract. In gh-aw v0.86.2, a result without `reasons` and a
second result with `reasons: []` count as conflicting raw verdicts even if all
flags agree; the parser checks for conflicts before defaulting optional reasons.
These instructions reduce that observed format failure, not guarantee model
compliance. Native conflicting/malformed verdict handling and genuine-threat
checks are unchanged. WTD3 still cancels label writes on a detection warning.
Inspect the detection log, native safe-output counters and retained report:
green job conclusions alone do not prove labels were applied. A blocked run
leaves the command visible and does not post a triage status/report comment.

The separate safe-output job checks out the exact trusted revision, imports only
bounded regular JSON outside the checkout, binds it to the preparation job's
independent context hash, and re-fetches/rechecks context and authority. It rejects
stale context, fabricated quotes, wrong targets, unsupported labels, inconsistent
intents and unsafe transitions, then renders its own artifact-only report.
The source prose projections are computed before hashing each prepared/current
snapshot, so the context hash and freshness checks cover this derived data too.
Including both body and prose increases context size, potentially approaching
twice the source-text portion; the existing 1 MiB JSON limit and all collection
bounds remain unchanged. Oversized or incomplete evidence still aborts rather
than truncating sources or salvaging partial proposals.
Withheld labels must be unique and cannot overlap the proposed label delta.
After evidence validation and report rendering, the final gate re-fetches the
complete bounded snapshot and requires its hash to match the validated context
before writing the report or releasing native intents. This clears the permission
cache, rechecks the original command actor and any rerun actor, verifies the source
command and open issue again, and rejects intervening evidence, label, reference,
authority or catalog changes. A stale proposal requires a fresh invocation.
Only built-in gh-aw add/remove-label handlers receive validated write intents.
Safe-output publication explicitly uses the built-in `secrets.GITHUB_TOKEN`,
consistently applying labels as `github-actions[bot]`; it does not select a configured
`GH_AW_GITHUB_TOKEN` publisher. Retry-marker recognition uses that same identity.
Their coarse family allowlists meet the compiler's 50-entry bound; the narrower
machine policy and exact live-name checks are mandatory before those handlers.

**Known permission limitation:** gh-aw v0.86.2's generic `remove-labels` handler
adds `pull-requests: write` to publication jobs and has no issue-only permission
switch. Exact issue-type/target checks constrain reachability but do not narrow
that credential. This PR does not upgrade repository-wide compiler tooling or
introduce a custom write handler; review this remaining least-privilege limitation
before apply-mode deployment.

Each run permits at most 20 total label changes, ten additions, ten removals and
one internal structured comment, with zero public triage comment writes.
The pinned add-label handler independently rejects arrays
larger than ten labels before applying its configurable limit; increasing
`max` cannot override that ceiling. The workflow, skill and trusted policy share
the ten-addition cap, so oversized proposals fail validation rather than reaching
that handler. Per-issue concurrency does not cancel an in-progress publication.
The workflow-level `queue: max` retains up to 100 pending runs per issue rather
than allowing an ordinary or edited comment to replace the single pending
triage command before authorization. Job-level queues do not protect this slot.
The queue remains bounded: additional runs are canceled when it is full, and
dispatch-order processing is not guaranteed. Inspect the Actions run before
assuming a command was processed; use a fresh invocation for a canceled command.
A genuine no-change result uses `noop` and leaves the issue untouched. Missing
required evidence is incomplete, not a successful review; inspect the Actions
result rather than interpreting the absence of a comment as success.
A validated withheld-only proposal retains its report and decisions, then emits
one native `noop` with a nonempty `message`, rather than an empty intent array or
a public report. Existing pure no-ops and incomplete/failure semantics are unchanged.
Label APIs are **not an atomic transaction**: inspect the Actions result
for actual application, particularly after a partial failure. The explanation
describes a validated requested delta, not an unconditional delivery claim.
The final snapshot check detects changes during validation; it cannot lock GitHub
state across the subsequent artifact upload or native handler/API calls.
Job reruns are deliberately unsupported: the authorization step rejects them
before exposing the Copilot pool, and the trusted validator rejects publication
job reruns too. The pool and the compiled agent, detection and safe-output job
conditions explicitly require attempt one, including partial failed-job reruns
that reuse successful dependency outputs. Top-level workflow `if` gates
activation, not each downstream job; additive built-in job conditions supply
the downstream guards without replacing compiler checks.
The pinned compiler emits immutable,
fixed-name artifacts, so changing only this workflow's custom artifact names
would not make reruns safe.
Use a fresh `/issue triage` comment or manual dispatch to gather current state
and assess remaining deltas. Each rendered decision carries a canonical
fingerprint of its action, label, reason, exact evidence and request.
Invocation identifiers and decision fingerprints are visible inline-code lines,
not HTML comments removed by the pinned publisher's sanitizer. Intake recognizes
these reports only from `github-actions[bot]` and never treats them as evidence.
Legacy intact HTML identifiers remain readable. Legacy sanitized reports with
the publisher-injected `Issue Triage` header and validated-proposal heading are
also excluded from evidence, but missing invocation or decision identifiers
cannot establish exact retry coverage.
Legacy report-marker checks remain defensive: a report for the same invocation
must cover all decisions exactly; changed decisions or multiple reports fail
visibly. Reports are never published by this workflow now, regardless of whether
a prior report exists. This is not a supported job-rerun recovery mechanism.
The validated `report.md`, `decision.json` and `validation.json` are retained
before any label handler runs. `validation.json` records zero public comment writes
and the exact requested additions/removals, not proof of completed label writes.
Retrieve the report artifact for the historical explanation.
A fresh invocation proposes only remaining changes and may legitimately use
`noop`; it does not reconstruct or publish a historical comment.
Inspect the original run and retained report for actual partial
publication, rather than treating the new no-op as proof of prior delivery.

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
The retained report describes the validated requested delta, not completed
writes. Check the run's staged mode and handler outcomes before claiming delivery.
Omitting `issue_number` is rejected, even though gh-aw requires the dispatch
input itself to be declared non-required for slash-command compatibility.
The compiler-added `aw_context` input must be empty. Authorization rejects
caller workspace context before trusted checkout, intake or PAT selection, and
both trusted script stages independently enforce the same dispatch constraint.
It cannot select an unrelated PR checkout for issue analysis.
Use `staged=false` only for intentional application; `/issue triage` comments
are apply-mode commands. New workflows are not dispatchable until recognized
on the default branch, so local compilation is not end-to-end hosted validation.

After source changes, regenerate only this workflow. Shared import frontmatter
changes (including `shared/pat_pool.md`) also require recompiling dependent locks
against the final merged imports, even when this workflow's source is unchanged.
Otherwise activation rejects the stale lock with `E009 CONFIG_HASH_MISMATCH`
before inference or publication.

```bash
gh aw compile issue-triage --strict --no-check-update
```

Commit source and lock together. Parse PowerShell and JSON and lint the generated
workflow with an actionlint version supporting its compiler-generated concurrency
fields. No pipeline snapshot/Pester/regex tests are part of this feature.
With actionlint 1.7.12, the compiler-generated `queue: max` field is not yet
recognized. Other diagnostics can still be checked without editing the lock:

```bash
actionlint -ignore '^unexpected key "queue" for "concurrency" section\. expected one of "cancel-in-progress", "group"$' \
  .github/workflows/issue-triage.lock.yml
```

### Real hosted fork canaries

Two GPT-6.1 Sol dispatches on an isolated `kubaflo/maui` feature branch completed
successfully; their original model outputs also passed this trusted validator
locally against freshly re-fetched upstream evidence and authority.

| Upstream issue | Hosted run | Validated proposed delta |
| --- | --- | --- |
| [#38925](https://github.com/dotnet/maui/issues/38925) | [37004775466](https://github.com/kubaflo/maui/actions/runs/37004775466) | Add `perf/general`, `has-workaround`, `version/android-16`. |
| [#37440](https://github.com/dotnet/maui/issues/37440) | [37004775679](https://github.com/kubaflo/maui/actions/runs/37004775679) | Add `material3`, `version/android-14`, `potential-regression`; remove `has-workaround`. |

The runs exercised actual source quotations, conflicting reproduction/version
evidence, an ineffective workaround, preservation of existing areas and
withholding unsupported priority/confirmed-regression decisions. Initial hosted
proposals exposed unnecessary blocked shell probes and automatic comment-ID
metadata; they were rejected rather than sanitized into success. Native MCP
guidance and narrow transport-ID handling were corrected before both reruns
passed. No pipeline test files or synthetic evidence were introduced.

This was **split validation**, not production end-to-end authorization. Both
fork credentials returned HTTP 403 on upstream collaborator-permission reads.
The existing local CLI login therefore performed real Gather and final Validate
GETs; its credential was never copied into Actions. Bounded compressed dispatch
data was independently hashed, and downloaded hosted context matched the
prepared bytes. The fork used its existing Copilot secret only for inference,
and literal staged handlers made no label/comment writes. Both issue states and
the fork's default branch were unchanged. Production default-branch/slash-command
gates, PAT-pool selection and apply-mode delivery remain unexercised.
