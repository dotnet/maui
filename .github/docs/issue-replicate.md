# `/issue replicate` (public issue repro)

GitHub comment/manual processing and scheduled recovery are disabled by default
and require the repository variable `ISSUE_REPLICATE_ENABLED=true`. Merging the
workflows or configuring a pipeline ID alone does not enable GitHub
acknowledgements or dispatch. This is not an Azure resource gate: someone with
Queue builds permission can call Azure directly while the GitHub variable is
unset. Keep the separate production definition's server-side `queueStatus`
disabled until the external activation/revision checks below protect every
credential-bearing resource. Leave the GitHub variable unset until all deployment
boundaries are enforced and validated. The credential-free manual canary on the
existing `maui-pr-uitests` definition is independent; do not disable that definition.

Repository writers, maintainers, and administrators can comment on an **open issue**:

```text
/issue replicate --platform android --branch main
/issue replicate --platform android --branch main --android-api 35
```

`--platform android|ios` is optional only when exactly one supported `platform/android`
or `platform/ios` issue label exists. `--branch main|netN.0` defaults to `main`.
`--android-api 30|35|36` selects the hosted Android runtime and defaults to 30
only for Android. It is rejected for iOS and does not change the author's target
framework, packages or MAUI revision. GitHub dispatch, Azure intake and publication
bind that selection to the current authorized command and immutable snapshot.
The owned emulator must report the same API before a UI test can execute;
independent confirmation compares the observed API again. Reports distinguish
requested and observed runtimes. Historical snapshots without this field remain
readable but do not acquire retroactive runtime evidence.
`--source <supported-GitHub-URL>` may explicitly select one of multiple links in
the latest author repro text. Intake rejects a selection absent from that text;
without a selection, ambiguity still fails closed. The immutable snapshot keeps
the alternate author links, and both intake and publication reauthorize the exact
selected URL against the current command.
Pull requests and commands from users without current repository write access do
not queue a run. The trigger posts an unassessed status acknowledgement without
operational run/log links; a second comment reports the result. A fresh command
comment starts a fresh run.

Issue comments missing either command marker (`/issue` and `replicate`) are
filtered before allocating an authorization runner. This is only a coarse
prefilter: the trusted parser still requires the command at the start after
trimming whitespace, with whitespace allowed between its words. Manual dispatch
and recovery still re-fetch and authorize the referenced command.

Azure intake independently re-fetches the open issue and command comment,
re-parses the command with the same GitHub-gate helpers, binds the queued platform
and target branch to it, and checks the comment author's current write permission.
It also requires the infrastructure checkout to match GitHub's current `main` SHA,
not merely the requested Azure repository version. On this pipeline revision,
direct Azure queue access does not bypass these gates. The always-running publisher
repeats these checks on its fresh trusted agent before importing results or entering
the publication/fallback handler. Deleted commands, edits that invalidate the command
or change its requested parameters, revoked permissions, closed issues, or unavailable
authorization metadata fail closed without publishing. If `main` advances during a
run, publication from the older infrastructure revision also fails closed.

Those checks authorize the command and revision, not production activation, and
run after Azure imports the read group. They cannot prevent credential release
on a direct queue. Activation must be enforced outside the selected YAML before
any dedicated variable group is released, as described below.

The issue author must provide exactly one repro in the issue body or an
author-written comment: a public `https://github.com/owner/repo` URL,
`https://github.com/owner/repo/tree/ref` (one URL-encoded ref segment), or a
GitHub-hosted ZIP issue attachment (linked as `[repro.zip](...)`). The latest
author comment containing a supported link takes precedence. The repository is
pinned to the explicit ref or default-branch commit; an attachment is hashed.
Repeated links to the same repository and decoded ref count as one source:
repository identity is case-insensitive, but refs are case-sensitive. Links to
case-distinct refs are rejected as multiple repros. Archives are
limited to 10 MiB compressed, 40 MiB expanded, 512 safe entries, and one
platform-targeting `.csproj`. A repro with multiple project files, external
downloads, non-GitHub attachments, or an inaccessible dependency may be
inconclusive. **Do not include credentials or private data in a public repro,
issue comment, or generated candidate.**

The isolated author SDK honors the nearest archived `global.json` SDK version,
or the pinned target branch SDK when the author did not specify one. An SDK
incompatible with the unchanged author TFM blocks provisioning instead of
retargeting the sample. Choose the matching `netN.0` branch explicitly. The build
runs from the author project directory so its SDK declaration is actually used.
ZIP metadata such as `__MACOSX` and AppleDouble files is not treated as source.
On iOS, the isolated SDK's actual native pack selects Xcode from its installed
`targets/Microsoft.iOS.Sdk.Versions.props` `RecommendedXcodeVersion` (or the
older pack's `_RecommendedXcodeVersion`), not from
the iOS SDK minor version. Those versions can differ: an iOS 26.5 pack may
require Xcode 26.6 while still requiring the exact iOS 26.5 simulator runtime.
The framework verifier likewise installs its pinned primary native pack before
reading that pack's requirement. Its checkout-free Azure job loads runtime
helpers from the immutable `$(Pipeline.Workspace)/IssueTools` bundle, not from
`Build.SourcesDirectory` or the downloaded framework tree.
Each selector records the pack identity and
requirement metadata hash. Missing or ambiguous metadata or a missing matching
Xcode installation fails explicitly. The author toolchain stays independent
from the framework verifier's toolchain; a newer runtime or
different framework is not silently substituted.
Author provisioning selects that Xcode and clears the command cache before the
first CoreSimulator query. Runtime inventory has a two-minute cold-start
deadline; failures retain bounded output plus exit/timeout/capture status.
Author and framework provisioning reuse a uniquely available exact runtime
instead of downloading another build of the same version. Missing runtimes may
be installed once, followed by at most three minutes of fresh registration
checks. Bounded inventory summaries retain the exact match count, build,
availability and architectures; multiple exact available matches are an
explicit blocker, not a reason to download or select an arbitrary copy.
The producer projects only those runtime identity fields before transferring
JSON to PowerShell: large per-runtime device catalogs are not needed for
admission. The normalized response retains its 64-KiB limit; diagnostic
availability messages are capped without truncating version or identity.

The sample is *built*, not driven through the reported interaction. A generated
unit/XAML/UI test runs against the pinned MAUI commit in a separate credential-free
job, with at most one feedback-driven revision. A passing test means only
`not-reproduced-on-tested-revision`. A test that fails at an assertion on two
runs, with the same failing test, assertion diagnostic and source signature,
is reported as a **verified failing test candidate**, not proof that the
author's scenario was exercised or the issue is confirmed. Only that outcome labels the complete generated candidate diff as verified failing.
Keep invariant assertion explanations stable, and log incidental timing or
successful-control measurements separately. The reviewed WebView-scroll probe
retains its positive label control and the unchanged `WebViewOffset > 20`
assertion; both measured offsets are logged without inserting the variable
label offset into the assertion identity. Expected/actual assertion values and
the strict cross-run identity comparison are not normalized or weakened.
Both drafting attempts import the same bounded author-build record, including
its validated target framework, outcome and diagnostic in the GPT prompt.
The unchanged eight-file, 8,000-byte/character-per-file source budget prioritizes project
declarations, app bootstrap, files named by its reachable constructed/registered types and
custom renderer/handler implementations over archive order, unrelated platforms
and resources. Up to eight traversal levels follow literal C# construction/registration
and prefixed XAML element or `DataTemplate` markup-extension type references,
including Shell content pages and nested navigation pages. A paired
XAML code-behind containing only its constructor's `InitializeComponent()` call
has lower priority than the actual page markup and interaction code.
References are read as bounded source text, never executed or parsed as live XAML.
A custom native registration must remain
part of the candidate's actual execution path, not be replaced with a default
handler or page-level styling. Missing native binding remains a prerequisite
blocker. Nullable text observations require a real non-null value before use,
without suppressing compiler diagnostics or substituting expected text.
Verifier feedback retains the latest bounded compiler/native log tail rather
than only early build output.
Rendering/crash candidates must preserve the author's child order and render
surface, including the navigation launcher; newly inserted diagnostic children
or an opaque status overlay are not an unchanged-scenario confirmation.
An `OnAppearing` counter alone does not establish completed native return paint
or post-return responsiveness.
Hosted recording/toolchain failures do not justify rewriting that scenario.
Generated UI tests must use existing category members; NavigationPage tests use
`UITestCategories.Navigation`, not `UITestCategories.NavigationPage`.
Native-navigation scenarios retain a real navigation host before observing its
bar or rotating the device. Unset-width scenarios retain the zero/non-accessible
baseline instead of demanding a positive initial element or substituting its
requested width. Bounds alone do not establish rendered BoxView fill: the unset
case can report width 100 while its colored bar remains absent. Generated tests
must inspect an external native screenshot, derive pixel regions and scale from
observed geometry, and require the initially nonzero reference to actually paint
before comparing the unset case's painted extent. Missing observation or a failed
reference remains a prerequisite blocker; instrumentation must not force a redraw
or insert diagnostic children into the affected layout.
Boolean text waits must succeed before a required interaction. A failed load
or readiness transition is an ordinary prerequisite exception, not an issue
assertion. Crash scenarios after WebView navigation retain the actual
`WebNavigationResult` and require successful navigation before the reported pop.
The Android screenshot helper's unsupported image/API/density assertion is a
prerequisite failure, even if a candidate catches the helper's wrapper exception
and NUnit retains a body-bound assertion. It cannot confirm an issue.
Owned Android API inspection uses the trusted replication tools' bounded process
reader, not a timeout helper imported from the target framework checkout. Its
fixed emulator-serial arguments, twenty-second deadline, sixteen-byte response
bound and owned-process cleanup remain required on historical branches. Crash-buffer
capture and recording conversion use that same trusted reader with their existing
twenty-/ninety-second deadlines and explicit output bounds, without increasing
recording size or changing full-duration checks. Both redirected streams drain
concurrently into fixed allocations; stderr beyond 8 KiB is an explicit failure.
Run and standalone verification preload the reader before untrusted execution;
post-body recording conversion calls that in-memory function without reloading
a mutable script file. This does not establish the missing OS evidence boundary.
If the UI runner exits nonzero without reporting a TRX, verification exports an
explicitly inconclusive result without qualified execution and bounded runner/compiler feedback
for the permitted revision. This does not qualify an assertion, recording or
verified patch. A successful runner without its authoritative result, multiple
reported results, and changed tracked source still fail closed.
Bounded build diagnostics retain up to three error lines, including native
tool errors without a compiler code (such as `actool error :`), so a missing
simulator runtime is not reduced to an empty build-blocker record.
Author build output is drained but forwarding/storage stops at 3,000 lines or
2 MiB, with an 8,192-character per-line bound and an explicit truncation warning.
The first three bounded error lines are retained independently of that log
budget, so late compiler diagnostics still reach the author-build record.
Bounded verification feedback retains ordinary exception messages, the generated
test's source frame, and the verifier's incomplete-result diagnostic. Inconclusive
reports without an observed assertion show that feedback in a closed
**Verification blocker** section without qualifying a timeout or setup failure
as an issue assertion.
The iOS prerequisite checks a responsive native WebDriverAgent status, not
just a successful build. Preflight failures are exported as bounded unqualified
feedback. The UI subprocess has a 45-minute wall deadline and a 10-minute
no-output deadline, with exact-process-tree termination and bounded pipe draining,
so session setup cannot consume the full outer job without exporting a blocker.
Inconclusive native runs retain a bounded filtered Appium tail. On an explicitly
owned Android emulator, the app-scoped crash buffer keeps process/thread,
signal and backtrace diagnostics; those diagnostics remain unqualified and do
not turn a teardown error into an assertion or verified patch.
Post-body native trees, PNG screenshots and marker-bound captured MP4 bytes are retained
as bounded, hash-labelled **diagnostic-only** chunks in the native job log, even
when the result is an ordinary exception. Trees may be truncated and STOP-side
snapshots/footage may include teardown. These logs remain retrievable if posting
is blocked, but do not qualify execution, an assertion, a patch or published
reproduction media; the existing qualified-recording checks are unchanged.
No snapshot request is inserted before or during the reported interaction.
This diagnostic logging is enabled by CI output providers, not by default for
standalone verifier calls. Raw clips retain the 512-KiB native limit in the log.
Android capture uses 400x712 video at 100,000 bits/second to leave headroom
for variable-rate output within that unchanged raw bound; this does not change
the native app viewport. Hosted clip readability still needs verification.
For GitHub's unchanged 256-KiB job-output limit, oversized qualified clips are
two-pass encoded without trimming their duration, at up to 480 pixels wide and
8 fps; duration, width, file shape and final byte size are checked. Encoder
failure remains an explicit media blocker, not successful publication.
Tool-free drafting also runs when a completed, bounded author build record reports
a failure. That does not bypass the author-build prerequisite for native verification:
the unchanged sample's target and diagnostic remain in the report, and the draft
is explicitly marked unexecuted and unverified.
The diff is untrusted code: inspect its assertions, test scope, and provenance
before applying it. Unsupported or infrastructure-failed attempts do not
invalidate the issue. No pull request or production-code change is created.

Every report starts with an **Issue reproduction** heading, explicit
**AI-generated issue replication attempt** attribution, and a compact
**Reproduction / Test quality / Evidence confidence** verdict. Detailed narrative,
code, videos, diagnostics, caveats and provenance are initially hidden in closed
`<details>` sections. Flat-square **Scope: Issue replication** and **Commit** badges
are in the closed **Report provenance** section and use the
same layout as test-failure analysis comments. The commit links to the validated,
pinned MAUI target revision, not the workflow revision or the author's sample.
When no validated intake snapshot is available, the commit badge is omitted
rather than inventing a revision. Pending publication notices and generated-test
continuation comments use the same attribution and badges.

Every final comment uses an expandable **Reproduction analysis** section with
short emoji-labelled **Reproduction**, **Test**, and **Evidence** verdicts.
A check mark identifies a repeatable generated failure,
a yellow indicator identifies a passing or repeatably failing candidate whose
match to the original issue still needs review, and warning/neutral indicators
identify incomplete or unavailable evidence. There is no technical boolean table.
The percentage is a
deterministic evidence score, not a calibrated probability that the issue is real:
**0%** means no failing reproduction evidence (unassessed, blocked, unsupported,
or a passing candidate), **25%** means an executed but unconfirmed assertion
failure, and **75%** means two matching independent assertion failures. A first
assertion followed by a passing or inconclusive fresh confirmation retains **25%**;
its result stays inconclusive and exports no verified failing patch. The report
explicitly acknowledges an executed test even when verification or independent
confirmation is incomplete; it does not describe that case as having no outcome.
Recording failures remain explicit alongside the incomplete verification. The report
distinguishes a test executed in either attempt from whether the fresh confirmation
completed its named body; setup, recording and missing/stale-result failures do not
claim a second execution. First-attempt assertion values remain available inside the closed analysis. The score never claims
certainty: the original author interaction and a bug-specific causal
control have not been verified. A repeatably failing generated scenario does not
automatically establish that its test catches the reported issue. Passing and
blocked cases explicitly state that they do not invalidate the issue. Available
expected/actual assertion text is included directly; no run or execution-log links
are posted, including in pending, queue-failure and incomplete-publication notices.

The result is posted **under the originating issue**, using the same expandable
layout as the regression-trace reports: **Reproduction analysis** contains the
outcome and nested **Verified failing test patch** or **Unverified test draft**
and **Native recording** sections when available. A separate expandable
**Follow-up** section contains run/deployment context and refresh instructions.
Build blockers are expandable rather than part of a technical evidence table. A verified failing candidate's
hash-checked diff is embedded in the comment so it can be reviewed without
downloading anything. Every patch includes a complete readable fenced diff.
Publication keeps **one current completed report per issue**, across run-specific
markers and recognized historical reports, from the authenticated publisher,
`kubaflo`, or the established MAUI bot accounts. Other discussion is untouched.
An independent validation report must explicitly start with
`<!-- issue-replicate-independent-report -->` and end with
`<!-- issue-replicate-report-complete -->` to participate; generic validation
headings do not grant cleanup authority.

The publisher verifies the new owned report's exact body and visible state before
minimizing older recognized reports as **OUTDATED**. POST and PATCH send a JSON
envelope through native `gh api --input -`, preserving the body exactly: the
PowerShell pipeline's terminal newline follows the JSON document rather than
becoming part of the comment. It never deletes their text,
attachments or continuation links. Among visible completed reports, the newest
comment ID wins; overlapping publishers may only hide older IDs, never a newer
replacement. Each minimization rechecks the unchanged old body and visible
replacement, and completion verifies there is exactly one current report.
Pending recording/patch checkpoints do not supersede the last completed report.
Legacy reports without the completion footer are checked for incomplete media or
candidate publication warnings throughout their body, including closed sections.
Hidden reports or continuations are not resurrected by retries. Failed reads,
writes or minimization are explicit publication failures, leaving the replacement
and any unprocessed historical evidence intact. Authorized local publishers of a
read-only canary preview must invoke `Complete-IssueReplicateReportPublication`
from the trusted Core script after posting their owned main report; preview itself
still performs no GitHub mutation. The reconciliation helper also supports
`-WhatIf` for a read-only live publication-state audit; that mode does not claim
that minimization succeeded or that the issue already has a single current report.
Hashes and exact-byte recovery data are retained only in non-rendered HTML
metadata; no checksums, base64 blocks or decode instructions clutter the rendered
report. Internal source, result, candidate, patch and video integrity checks are
unchanged. The hidden `issue-replicate-patch-data` marker contains the full patch
SHA-256 and exact UTF-8 base64 data, including for small patches; use that metadata
from the raw comment if exact-byte recovery is needed, since rendered diff previews
may normalize line endings. When the combined diff and hidden metadata exceed the inline
budget, diffs are split across bounded continuation
comments linked from the main report, with hidden full-patch binding and ordered code
blocks; no code is truncated. Passing and inconclusive results can include the
complete generated draft, clearly distinguished from a verified failing candidate.
The publisher validates its path, platform envelope, bounds and candidate hash;
when native evidence exists, that hash must match the actual verified input.
Unsupported or missing drafts omit the code section rather than offering an empty
“review code” section. A draft may need corrections before it compiles or runs.
OS-setting scenarios must use the real setting change, not injected notifications
or controller trait overrides. The generator requires an observed initial state,
separate confirmation that the setting changed, component-scoped native observations,
and restoration of modified settings. Generation guidance is not execution evidence.
Evidence includes direct links to the author's original repro ZIP or public
repository at the immutable revision when applicable. Repro links are validated against the supported
GitHub source formats before publication.
Results are updated idempotently per run, using only the authenticated publisher's
own comments; third-party markers cannot redirect or suppress publication.
Multi-comment publication creates an explicitly incomplete main report before
posting parts and finalizes it only when every part succeeds. A failed attempt
can be retried without duplicating or presenting incomplete fragments as a complete patch.
Publication failures also produce an
expandable follow-up notice, without claiming a verified outcome.

UI verification also records one bounded test span on the selected native
attempt: at most 30 seconds, no audio, and at most 512 KiB of MP4 data. Recording
starts through a trusted assembly-level NUnit action after one-time fixture
setup. The action blocks before per-test setup and the body until the controller
acknowledges that Appium started recording. Merely reacting asynchronously to
the `Start` log marker can miss a fast interaction while the recorder starts.
The action is imported only into the Android/iOS test project from immutable
tools, without changing the candidate or tracked framework files. Its fresh
acknowledgement nonce and operation ID prevent stale replies; missing or failed
acknowledgements stop the body after a bounded 25-second wait. The controller
stops at the named `Stop` marker and rejects spans exceeding its conservative
30-second deadline instead of exporting an automatically expired clip. The deadline
is checked when that marker arrives, before the bounded recording retrieval;
retrieval latency does not count as continued test execution.
An Appium acknowledgement confirms the recording request, not that the encoder
captured the first frame or initiating interaction. Before publishing a manual
canary, decode its actual frames and inspect the baseline, interaction and result.
A decodable clip showing only the final state is not interaction evidence.
The trusted controller uses the live Appium session without enabling session discovery.
It selects the frontend session from Appium's incoming HTTP command or top-level
session-creation log entries, not the distinct UiAutomator2/WDA backend session
UUIDs in proxy requests. Missing frontend evidence fails recording explicitly.
Selection reuses the bounded Appium reader: it scans at most the last 64 MiB and
keeps the last 80 non-syslog rows, so an iOS system-log flood cannot hide a recent
frontend command inside the ordinary 256 KiB tail. It reads backwards, stopping
once those rows and an omitted earlier row are found, instead of always scanning
the entire window. Its four-second deadline leaves room for the existing
20-second recording request within the NUnit wait. A deadline failure still
blocks recording and the body; reduced read cost is not proof of native success.
Recorded UI candidates must execute exactly one named test: one matching
`Start`/`Stop` pair, one acknowledged recording action and one TRX result whose
method matches those markers.
Multiple methods, repeated cases, missing markers or a mismatched TRX are
inconclusive; the clip is discarded instead of publishing an unrelated method's
video alongside an assertion. Unit/XAML verification retains its multi-test support.
All candidate kinds must use assertion APIs rather than assertion-exception
implementation types. Validation conservatively rejects references to the same
NUnit/xUnit/MSTest exception names recognized by the TRX classifier, after XAML
entity decoding and Unicode escape, formatting-character and verbatim-identifier
normalization. This also
rejects type-alias declarations, namespace-aliased constructors, static factories,
target-typed construction and catches, including references in comments/literals.
Ordinary setup/protocol exceptions and genuine assertion API calls remain supported.
This syntactic guard does not authenticate arbitrary generated code or replace the
still-required OS isolation and inaccessible evidence channel.
The shared NUnit source must keep the inherited fixture lifecycle unchanged.
Validation conservatively rejects lifecycle/reset hook names, including escaped
identifiers and references in comments/literals, so candidates cannot enable
per-test fixture resets after the base `Start` marker. Additional NUnit setup/teardown
attributes are unsupported as well. Put issue-specific interaction
in the one test body instead. If the framework's own setup recovery reports a
successful session recreation, the controller restarts recording on that new
session before the body completes. These checks are not an isolation boundary.
Diagnostic snapshots additionally retain bounded `SearchBar` and
`GoToTestButton` native attributes from the already-bounded source response,
before truncating the raw tree. An initial source-only query runs after releasing
the recording acknowledgement, so diagnostics do not extend its 25-second body
gate. These unqualified observations expose stale gallery input without expanding
the tree budget or changing navigation; they do not prove pre-interaction state.
Generation rejection keeps the guard intact and logs the exact matched lifecycle
token plus a bounded, SHA256-labelled, base64 proposal diagnostic. Rejected source
is not a candidate envelope and is never admitted to native execution or publication.
UI drafting uses the required `ShardedTestCategory` attribute for CollectionView,
preserving its umbrella and CI-shard registration rather than suppressing MAUI0003.
For iOS-specific configuration imports, drafting qualifies the actual Controls
navigation types to avoid the identically named platform-configuration classes.
The recording is visual context for the generated candidate, not proof of the
author's exact app interaction or tamper-proof evidence. Unit/XAML candidates
do not record a UI video. Recorder or upload failures remain explicit in the
report and fail publication rather than claiming a playable attachment exists.
The isolated publisher uploads the validated bytes as a GitHub media attachment,
then includes its player URL inside the expandable **Native recording** section.
The publisher records a bot-owned pending report before uploading and checkpoints
the returned attachment URL immediately afterward, before finalizing the report
or publishing patch continuations. Retries reuse that matching run/video receipt;
partial candidate reports retain it. If an upload may have started but its URL
was not persisted, retries fail closed without uploading again or overwriting the
pending report. An operator must reconcile the public upload receipt from the
trusted publisher log; GitHub upload and comment writes are not one atomic operation.
The clip is retained as a GitHub user attachment, independently of Azure/GitHub
run-artifact retention. This workflow does not automatically delete uploaded clips.

For a read-only preview, pass `-OutputPath` to `IssueReplicate.Post.ps1`; this
validates the same result data and writes the comment without calling GitHub.
An available recording is saved beside the preview as `.recording.mp4`; previewing
does not upload the recording or publish anything.
Authorized fork canaries can use `-GitHubRunId` and `-GitHubRepository` instead
of `-BuildId` to identify their provider in internal reconciliation markers,
without posting operational run links or pretending they ran in Azure.
`-GitHubRepository` is required with `-GitHubRunId`; publication never silently
defaults to a fork or infers the run repository from the current checkout.
Normal production publication still uses the isolated Azure
Post job and its separately scoped issue-comment token.

### Comments-only output

Neither the production pipeline nor fork canaries upload artifacts, even for
cross-job transfer. Comments are the only published reproduction output; normal
execution logs remain available on the run page. Local sample archives, SDKs,
build outputs, TRX files, and patches exist only in disposable job workspaces.
An inline GitHub video attachment is comment media, not an Actions/Azure pipeline
artifact. Recording bytes use eight fixed, independently bounded base64 outputs;
the selected clip's byte count and SHA-256 are checked at forwarding and publication.
Only the selected native attempt/candidate's clip enters the next job. Payload
variables are injected once, avoiding duplicate full-size environment aliases.
The optional GitHub output provider rejects recordings above 256 KiB to leave
room for the text envelope within GitHub's UTF-16 job-output budget.

Jobs exchange bounded gzip/base64 data through named job outputs. The transport
accepts only a fixed set of data filenames, checks per-file sizes and SHA-256,
limits decompression, and cannot overwrite existing files. It never transfers
scripts, extracted samples, compiled apps, or installed tools. Each isolated job
fetches trusted scripts from the immutable public pipeline commit. Jobs needing
the sample re-download its pinned public source and require the original ZIP hash.
If the source changes or disappears, the attempt is explicitly inconclusive.

The trusted execution wrapper loads the validator, executor and bounded exporter
into its parent process before launching an author build or generated test.
It exports in that same process, including bounded failed-build diagnostics;
no later task re-reads executable scripts from the mutable native job workspace.
Completed result, immutable patch and captured-feedback bytes remain in that
parent's memory through validation and bounded export; it does not reopen
sample/result/patch files after child execution.
Captured recording bytes likewise remain in parent memory until bounded export.
Both draft and native-verification patches snapshot the original candidate bytes
before execution through the same isolated Git diff helper. File-based diff capture
and the empty diff tree preserve CRLF, Unicode and missing final newlines without
PowerShell line splitting or repository text-attribute normalization.
This protects executable re-entry and finalized-record export, not evidence
authenticity: author/generated code still runs as the same OS user and can write
the TRX and CI job-output locations.
Fresh agents, source hashing and a preloaded exporter do not prevent that code or
its detached processes from replacing evidence. Current native reports are
observations requiring review, not tamper-proof verification.

The isolated posting job imports only the snapshot, bounded sample build record,
candidate draft and verified result when available, validates
the existing issue/revision/patch contracts, and publishes the expandable report.
Selection follows the available verification payload: result 2 uses candidate 2;
if verification 2 exported nothing, a completed result 1 retains candidate 1 even
when generation 2 succeeded. With no verification result, the latest available
candidate remains an unverified draft. Missing matching candidates or malformed
selected payloads fail explicitly; they do not fall back to a different pair or
turn a failed revision into an independent confirmation.
The full diff and original repro links are preserved in comments without artifact
retention or storage charges. Fork publication requires a separately configured
issue-comment credential: the fork's built-in token cannot post to `dotnet/maui`.
Never repurpose the Copilot credential or expose a posting credential to native
execution. An unconfigured publisher fails explicitly rather than pretending a
comment was posted.
If the unchanged author sample cannot build, the report preserves its exact target
framework and a bounded build diagnostic as untrusted log text. It explicitly
states that no generated test ran and makes no verified-failure or assertion claim.
An available draft is published separately as unexecuted and unverified, not as a
verified failing candidate or an assertion claim.

iOS UI execution uses a hosted simulator, not a physical iPhone. Reports labeled
`repro:device-only` still need physical-device validation; a simulator pass or an
unsupported generated candidate cannot rule out the reported behavior.
Each native iOS verifier creates a fresh iPhone 11 Pro on the exact major/minor
runtime declared by the pinned iOS SDK and passes its UDID explicitly to the
unchanged pinned UI runner. Missing or unavailable matching runtimes fail
explicitly; visual-test preferences must not silently select a newer runtime
than the matching Xcode. The owned simulator is deleted after verification.
The verifier passes the pinned runner's existing `HEADLESS=true` setting only
while running against its owned iOS simulator, then restores the prior value.
This avoids Appium shutting down a ready background simulator to reopen it with
a visible Simulator window. Native screenshots, recording and assertions remain required.
Before adding or executing the candidate, the verifier uses the installed
XCUITest driver's WebDriverAgent library to resolve its derived-data directory
and build for that owned simulator and the pinned SDK version. The bundled
`build-wda` command does not resolve this directory before building; its default
Xcode output can differ from the directory used by the native runner. Preparation
uses the driver's own path resolution rather than a hardcoded cache path.
The uniquely owned simulator first has a ten-minute cold-boot deadline, with a
bounded boot-status diagnostic tail; it must actually become ready.
Preparation separates a cold WebDriverAgent build from the unchanged pinned runner's
session-launch timeout. The cold preflight launch may wait within the existing
ten-minute process deadline instead of interrupting WebDriverAgent after 50 seconds;
it must still return a responsive native iOS status. Preparation has a ten-minute process
deadline, emits a bounded diagnostic tail, and fails explicitly on a build error,
timeout, or incomplete output capture; none is a candidate assertion. It does not
change the pinned framework, Appium package versions, or native test launch capabilities.
Successful prebuilding alone is not evidence that WebDriverAgent starts or that
the test body executes.
Native iOS and Android UI jobs also require FFmpeg, ffprobe and the `libx264` encoder before running
the candidate. A missing binary is installed on that disposable hosted agent;
installation, version and encoder checks fail explicitly. Recorder HTTP failures
retain the bounded Appium error message rather than only its status code. Error
bodies are limited to 16 KiB and published diagnostics to 1,000 characters.
A fresh, matching named TRX body and Start/Stop pair establish test execution
independently of video availability. If recording fails, the result honestly
reports whether the body executed but remains inconclusive. Startup rejection
now prevents the body, while a later recorder failure does not erase a completed
body's evidence. Neither case exports assertion routing, a verified patch,
video evidence or a successful strict canary packet.
Appium context retains the last 80 non-system-log rows in at most 128 KiB from
a backwards scan bounded to the last 64 MiB with a hard reader deadline.
The scan stops after finding the retained rows and an omitted earlier row;
syslog-only spans still require scanning within the fixed window.
Omitted context is labeled; these diagnostics are not assertion evidence and
do not create artifacts.

Generated UI files are limited to the Android or iOS platform actually verified.
The entire shared NUnit file must use the corresponding exclusion-symbol `#if`
guard. The paired multi-targeted HostApp file must independently use the exact
whole-file `#if ANDROID` or `#if IOS` guard, including all using directives and
declarations. Neither file may contain other preprocessor directives; generation
and both native verification attempts enforce the same contract without rewriting
candidate code.
`PlatformAffected` on the HostApp page is metadata, not a test-discovery filter.
Unit and XAML candidates retain their existing scope. A reviewed UI candidate can
be broadened later only after establishing its applicability on other platforms;
the workflow does not claim that coverage.

The complete generator prompt, including issue text, sample snippets and revision
feedback, is capped at 95,000 UTF-16 characters and 120,000 UTF-8 bytes before
invoking Copilot. The byte budget leaves headroom below the hosted Linux
per-argument limit for multibyte text. Oversized prompts fail explicitly before
model invocation; text is not silently truncated to fit.

Repeated assertions run in separate fresh hosted jobs, each independently
provisioning and checking out the same pinned source and candidate. First-attempt
build outputs, ignored files, SDK directories and simulator state are never
transferred to the second job. Only the bounded first-attempt identities and
candidate hash are compared. The first attempt exports an explicit boolean
assertion routing state. Only an observed assertion schedules the second native
job; passing, unsupported and non-assertion inconclusive outcomes skip that job.
A credential-free Linux forwarding job validates the completed record against
the same snapshot, successful sample build, candidate hash and attempt number,
then forwards its result, bounded UTF-8 feedback and any hash-matching confirmed
patch. It does not check out MAUI, install SDKs/workloads, build tasks, provision
Appium or boot a device. It rejects missing routing state or missing required
confirmation rather than accepting the first observed assertion as confirmed.
This routing applies to both the initial and revised candidate. An inconclusive
result must retain that feedback for the one allowed GPT revision; missing,
empty or invalid feedback fails explicitly. Captured native output can contain
blank lines without preventing the completed record from being exported.
Feedback retains fixture setup/teardown diagnostics, which remain inconclusive
rather than being classified as test-body assertions.
Selected TRX failures that do not qualify as issue assertions retain their bounded,
sanitized error message instead of being mislabeled as missing recording markers.
Native verification preserves any separate recording failure alongside that error,
and its feedback retains the combined diagnostic. These outcomes still discard
the clip, skip assertion confirmation and export no verified failing patch.
Every assertion identity requires a matching non-constructor candidate stack
frame and the fully qualified test method recorded in its TRX definition
(including async state-machine frames). NUnit fixture display arguments such
as `(Android)` are removed only for the NUnit adapter when binding that class;
the namespace and method still have to match. Helper-only or different-namespace
frames are inconclusive; test names and assertion text alone cannot establish a
matching repeated failure. Lifecycle/constructor callers outside the recorded
body, including inherited `InitializeAsync`, `DisposeAsync` and `Dispose`,
invalidate the evidence even if they invoke that test method directly.
Ordinary assertions in helpers, including disposal invoked by the actual body,
remain eligible and retain the assertion-source identity.
Candidate call chains containing instance or static constructors are also
inconclusive, including assertions in helpers called during fixture initialization.
Tracked framework and candidate files are hashed
before execution and rechecked afterward; changes invalidate the attempt.
NUnit setup/teardown assertion diagnostics and xUnit fixture errors are
inconclusive, not verified test-body assertion evidence.
Constant-only NUnit and xUnit assertions are rejected, but generated code still
requires human fidelity review: an executed failure is not proof of the original bug.

## Deployment and isolation

### Manual Azure canary before deployment

The existing `maui-pr-uitests` definition can run a credential-free canary from
the PR branch without changing its registered YAML path. Manually select the
branch and exact commit, set `IssueReplicateCanary=true`, and choose
`IssueReplicateCase=all`, `android-carousel`, `android-scrollview`, `ios-refresh`,
`ios-shell-navigation`, `android-webview-scroll`, `ios-modal-singleton`, or
`ios-collection-shrink`.
The default is false: ordinary UI validation, pools, parameters and secret imports
are unchanged. Canary mode excludes the shared MAUI variable group and all normal
UI stages. It requires a manual run in `dnceng-public/public`.

Canaries replay reviewed public snapshots and UI candidates stored in
`.github/issue-replicate-canary`. Their framework commit,
author archive hash and issue association remain explicit; this is not fresh
intake or generation. The files are platform-scoped before execution, not rewritten
by the verifier. Repository repros also require a full immutable source commit.
The Shell navigation case uses issue #37360's author-owned repository and a newly
reviewed iOS candidate. It runs a normal push/pop control before dismissing a modal
and pushing a detail page inside `Shell.Navigated`, then observes the real root
handler and root pop notifications. Its assertion is not proof of the original
app's visual blank-page outcome.
The WebView case uses issue #38452's author-owned repository. It checks real native
HTML loading and absence of vertical overflow, proves a normal label gesture can
scroll the parent, then measures the parent scroll offset after dragging on the
short WebView. The overflowing WebView remains in the layout but its independent
scrolling is not asserted. Missing, nonnumeric or nonfinite parent scroll-position
diagnostics are setup failures, not evidence of the WebView bug.
Before the WebView gesture, the same finite numeric telemetry must confirm a
parent offset within 0.5 of zero; a substring such as `0.0` also matches `100.0`
and cannot establish that the reset completed.
The Android WebView is located through its unique short HTML content and outer
native WebView ancestor; Chromium's accessibility subtree omits its automation ID.
The candidate still requires that native rectangle to fit fully inside the parent
viewport before initiating the gesture, with no change to the recording limit.
An unverified execution result can include a body that entered but failed setup;
the Report gate does not treat that as a completed, verified candidate execution.
The singleton modal case uses issue #38361's author-owned repository. It preserves
the reused modal page/navigation wrapper and manual disconnect policy, compares a
fresh-instance control, and observes native content elements and geometry after
interactive dismissal and reopening through displayed native accessibility
elements with positive bounds, not pixel comparison. DI is isolated to the
candidate's route factory; the author application's bootstrap is not replayed.
The modal candidate reads native content, instance markers, navigation bars,
window geometry and returned-home controls from one accessibility snapshot per
state instead of repeated element-property requests. Actions use the observed
native bounds; waits for snapshot reads and gestures share the 26-second body
budget. Timing out a wait does not cancel an already issued Appium request.
The readiness retry window limits scheduling another snapshot, not accepting an
already completed, ready snapshot that is still within the body budget. A read
that crosses that window logs its readiness and elapsed body time; a late read
cannot bypass the overall budget or turn an incomplete control into an assertion.
The two fresh-instance controls, singleton reuse check, reopened-content
observation and 30-second recording limit are unchanged.
The CollectionView case uses issue #38276's original public attachment. It
preserves the capped Grid inside a VerticalStackLayout and compares rendered
height for one item, ten items, and a replacement with one item; it does not force
a layout invalidation. Each state first qualifies status, native geometry and
visible item contents from one native accessibility snapshot. Stabilization then
reads the bound native collection and status elements, including visible item
queries scoped to that collection, instead of rebuilding the entire hierarchy
for every sample. It still requires three matching geometry observations, the
same item/source-state checks, and the unchanged observation/recording budgets.
These reviewed candidates are not executed evidence until
their corresponding native jobs run.
The ScrollView candidate polls both rendered padding insets for up
to five seconds after the status changes; the synchronous status text alone is
not a layout-completion signal. They share the production sample job, native
verifier, conditional fresh-agent confirmation, bounded transport and Linux forwarder. Microsoft-hosted
agents avoid assuming shared pools are disposable. A missing matching SDK/Xcode,
an unexecuted candidate or unmatched confirmation fails the canary explicitly.
The report job requires an available, hash-checked native clip and renders the
production comment preview into normal execution logs without uploading artifacts
or posting issue comments. Actual publication can be tested separately with an
authorized local publisher importing the selected bounded outputs; it is labeled
`-NativeCanary` and is not automatic Azure publication. Never attach a credential
group to this branch-selectable canary to bypass the unresolved production resource
checks. Actual canary observations still require review and are not tamper-proof
evidence.
After successful strict validation, the canary Report log also exposes a bounded
`CANARY_PUBLICATION_` data packet. Azure suppresses ordinary output-variable
logging, so this explicit normal-log packet is needed for the external publisher.
`IssueReplicate.CanaryData.ps1` reuses the existing text and media transports,
binds import to the selected build/infrastructure SHA, rejects missing/duplicate
or oversized fields, and preserves the exact patch and recording bytes.
It transfers data only, never executable files; no artifact upload is introduced.
Pass the import's `RecordingBytes` directly to `IssueReplicate.Post.ps1 -VideoBytes`
in the trusted publisher process. This avoids putting the full recording back in
the local environment, where macOS has a much smaller aggregate process-argument
limit than hosted Linux. The supplied bytes must still match the result's size
and SHA-256 before preview or upload.
The publisher reads one compact JSON record per matching owned report across
all comment pages with `gh api --paginate --jq`. Do not combine `--slurp` with
`--jq`; the CLI rejects that combination before media can be reconciled.

Hosted Ubuntu sample and verification jobs reclaim named unused preinstalled
.NET, Swift, Haskell, Go, Boost and CodeQL directories after installing the exact
bootstrap SDK outside those directories. Android, Java, Node and PowerShell
remain available. The jobs log actual disk capacity without requiring more free
space than the hosted pool guarantees. Workload installation and author builds
must actually complete; disk exhaustion is an explicit environment failure, not
evidence that a generated test ran or that the reported issue is invalid.
Android verification pins a single `ANDROID_AVD_HOME` for AVD creation, emulator
launch and the test runner, checks that the created AVD exists there, and limits
its data partition to 2 GiB. Emulator startup is bounded; a failed startup prints
the last 120 diagnostic lines in the task log without uploading an artifact.
The Android screen stays awake and unlocked while the HostApp builds, and window
animations are disabled before UI execution. The existing CI `hide_error_dialogs`
setting is applied and checked at boot, before the lengthy HostApp compilation:
applying it only in the pinned runner's pre-test warmup does not remove a SystemUI
ANR dialog already covering the app. This prevents system error overlays, not test
failures; crashes, setup failures and missing execution still cannot qualify as
assertion evidence. Native verification prints a bounded,
sanitized Appium log tail and the latest existing UI hierarchy text for startup
diagnosis, using the pinned runner's actual log directory. The hierarchy's start
and end are limited to 16,000 characters including the truncation marker.
The Appium reader reads at most its last 128 KiB, then prints at most 80 sanitized
lines. Both log and hierarchy readers use Python 3's nonblocking, no-follow Unix
open and check the opened handle with `fstat` before reading. FIFOs, device files
and symlinks cannot pass as regular files. Each reader has a hard 20-second process
deadline and fixed output allocation; the optional task also has a two-minute
deadline. The helper is re-fetched into parent memory from the exact public
pipeline revision, not executed from a post-test mutable tools file. Long startup
logs do not suppress bounded context; the entire Appium file is never loaded.
Unsafe files, stalled readers or oversized hierarchies fail visibly without
changing an already-completed verification record. These
diagnostics are not assertion evidence and are never uploaded as artifacts.

This mode does **not** enable production authorization, OIDC dispatch, credentialed
GPT generation, the feedback-driven GPT revision, recovery or automatic publication.
Those paths retain their production guards and deployment requirements below.

**Production activation is blocked by the unresolved same-user evidence boundary.**
Before enabling the trigger or registering an active production pipeline,
isolate untrusted execution from the trusted collector with an enforced OS
security boundary. Generated code and its descendants must not be able to write
trusted result/output locations, and the collector must receive runner-owned
results through a channel unavailable to that code. Validate this with real
native execution on both supported platforms, including detached-writer attempts;
the current fresh-job split does not establish this boundary. Do not treat the
steps below as permission to deploy before that requirement is implemented.

**Historical YAML replay also requires an external enforced boundary.** New
guards do not retroactively protect older pipeline YAML. Before releasing any
credentials, a mandatory server-side protected-resource check outside the
selected YAML must compare the run's `self` repository ref and exact version with
GitHub's current `dotnet/maui` `main` commit and reject historical or unavailable
metadata. It must also independently read the live repository activation variable
and reject missing, false or unavailable activation before releasing a resource.
Neither a queue-time parameter/variable nor a condition inside selected YAML is
an acceptable substitute. Apply both checks to every dedicated credential-bearing variable group;
deny unapproved pipeline/group edits and queue-time variable overrides. Do not
place these secrets on the pipeline definition or in root variables: older YAML
can inherit those without requesting a checked resource. Verify direct API
queues against old revisions are rejected before any credentialed work. Also
verify a direct queue of current `main` with a valid maintainer command cannot
release credentials while activation is unset or false. These external checks
have not been configured or exercised; no separate production definition or
credential groups have been provisioned. Production remains blocked. Keep the
separate definition's server-side `queueStatus` disabled during configuration,
and whenever activation is turned off; do not rely on GitHub workflow conditions.

Intake and the always-running posting job independently require the
`dnceng-public` collection and `System.TeamProject` exactly equal to `public`.
Both fail closed before intake or publication, including fallback notices, when
either predefined environment value is missing or belongs to another project.

1. Keep `ISSUE_REPLICATE_ENABLED` unset while merging the trusted scripts, trigger,
   and pipeline YAML to `main` and configuring resources. Create a
   **separate public Azure pipeline** in `dnceng-public/public` with
   `eng/pipelines/ci-issue-replicate.yml` as its YAML path and server-side
   `queueStatus: disabled`. Verify direct API queues are rejected before granting
   access or attaching credentials. This is not the existing canary definition
   313 and is not `/review`:
   `/review` queues DevDiv/DevDiv pipeline 27723. Set the GitHub Actions
   repository variable `ISSUE_REPLICATE_PIPELINE_ID` to the new public ID.
2. Give the GitHub OIDC identity Basic access in `dnceng-public` and explicitly
   allow **Queue builds** on the new pipeline (see
   [OIDC setup](trigger-azdo-pipeline-setup.md)). Protect the pipeline, its
   `main`-branch definition, and its variables from arbitrary run edits.
3. Provision three **separate protected secret variable groups** with the
   mandatory external activation and revision checks above: `issue-replicate-read` containing
   `ISSUE_REPRO_READ_TOKEN` (GitHub issue/repo and collaborator-permission read),
   `issue-replicate-copilot` containing `ISSUE_REPRO_COPILOT_TOKEN` (Copilot CLI GPT
   access), and `issue-replicate-comment` containing `ISSUE_REPRO_COMMENT_TOKEN`
   (issue-comment-only publication). The YAML imports them only in their
   respective intake, generator, and posting jobs; the read group is also
   required by the publisher's independent authorization check. Sample/native
   test jobs import none. Keep task-level token mapping; do not add definition-level
   copies or a shared MAUI secret group. Review whether your token policy permits public
   issue/repository reads, Copilot access, and issue-comment posting.
4. Confirm `ubuntu-22.04` and `macOS-26` are **fresh Microsoft-hosted
   agents**, with Android KVM, Appium, appropriate Xcode/simulator, and workloads
   on the chosen MAUI branches. Xcode is selected from the installed pinned
   iOS SDK pack's declared requirement; a missing matching installation fails rather than silently
   using an incompatible version. Do not enable iOS on a persistent/shared macOS
   agent. Validate native execution with the credential-free canary while
   production stays disabled. Validate old-revision and activation-off direct
   queues against the external resource checks before enabling production;
   YAML parsing alone does not validate Azure resource enforcement or images.
5. Only after the evidence boundary, both external resource checks and the
   negative/native validations pass, set `ISSUE_REPLICATE_ENABLED=true` and enable
   the separate production definition. Run an authorized production test issue
   before announcing availability. If that validation fails, disable the
   definition and unset the activation variable again.
   Leaving it unset or setting it to `false` disables GitHub trigger/recovery;
   the external checks must independently deny direct Azure credential release.
   Disable the separate definition as well when pausing production.
   No activation variable is configured by this PR or its manual Azure canary.
   For scheduled missed-webhook recovery, also set
   `ISSUE_REPLICATE_RECOVERY_NOT_BEFORE` to the activation time in UTC
   (`yyyy-MM-ddTHH:mm:ssZ`). The recovery workflow scans recent comments older
   than 35 minutes, rechecks current write permission, and dispatches the same
   trusted workflow. Leave the variable unset to disable recovery; use
   `ISSUE_REPLICATE_RECOVERY_DISABLED=true` to pause it. This avoids replaying
   historical comments on first deployment.

Durable command acknowledgements are trusted only from `github-actions[bot]`.
Fresh commands reserve one comment of headroom in the three-page intake budget;
retries reuse their existing acknowledgement. A pending acknowledgement is a
35-minute lease, not a terminal success marker. Expired reservations are reconciled
against bounded Azure run history and the exact issue/comment/platform/target-ref parameters before
queueing again. The GitHub gate binds its current `main` checkout SHA to dispatch;
dispatch rechecks it before requesting an explicit `self.version` and validates
the returned run's ref and version. Reconciliation accepts only that exact
repository version. A matching command with an older/different infrastructure
version fails explicitly instead of being adopted or silently queued again.
Incomplete history or multiple matching runs also fail explicitly.
Recovery's transient `eyes` reaction is cleared after dispatch and is never a
terminal latch; only the bot's successful build marker or `rocket` acknowledgement
completes recovery.

The GitHub gate and Azure intake/posting check out trusted `main` without
persisting Git credentials. Sample and test jobs use `checkout: none`, receive
bounded job data, and never receive the Copilot or issue-posting tokens. The
generator uses GPT with tools disabled and never executes sample or generated
code. A public pipeline alone is **not** a security boundary: do not grant
execution jobs privileged access or reuse the trusted job agents for them.

Both generation attempts install Copilot CLI **1.0.91**, the version observed in
the successful tool-free generation run `37140665018`. The trusted tools checkout
carries `.github/issue-replicate-cli/package.json` and its npm lockfile. `npm ci`
uses exact locked versions and integrity hashes, including transitive and optional
platform packages; install scripts are disabled. Only the subsequent generator
task receives the Copilot credential. Do not install `latest` or use an unlocked
global install. Update the manifest and lockfile together, deliberately review
the dependency changes and revalidate generation before deploying a version bump.

Execution checkouts are shallow, without partial-clone extensions that the pinned
SourceLink tooling does not support. Git transfer integrity checks are enabled.
A failed clone, fetch or checkout is discarded and retried at most three times in
fresh owned temporary directories, never in a corrupt or existing destination.
Only a checkout matching the pinned SHA with no local credential header is
atomically moved into the execution directory; exhausted retries remain fatal.
Before restoring tools, native jobs use the
pinned checkout's Arcade bootstrap to install the exact `global.json` SDK from
public feeds in a separate job-temporary bootstrap directory. Native provisioning
replaces the checkout's `.dotnet` directory, so the bootstrap SDK running Cake and
MSBuild must not live there. The shared provisioner does not replace the pinned
version with a hard-coded SDK.
Sample restore uses the pinned MAUI
`NuGet.config` public feeds plus nuget.org, so servicing SDK dependencies remain
available even though the author's sample is extracted outside the MAUI checkout.
The sample job installs a separate copy of the exact public SDK distribution from
the pinned `global.json`, retaining its stock MAUI manifests and installing the
selected platform's public MAUI workload without manifest updates. Repository-native
provisioning alone removes those manifests and cannot supply template samples'
`$(MauiVersion)` defaults. No package version is injected into the author's project.
Stable SDKs use the installer's normal public feeds; prerelease SDKs use the public
CI feed. Both historical `bin` and current `temp` installer layouts are supported.
Restore is restricted to its selected, existing platform TFM, including an explicit
platform-version suffix, rather than requiring
unrequested Android or MacCatalyst workloads. The sample's bundled public MAUI
version may differ from the issue's reported version; it is not the framework under
test. Verification retains its own source-pinned SDK and native workloads.
Verification builds `Microsoft.Maui.BuildTasks.slnf` before compiling candidate
tests or the HostApp. Feedback prioritizes compiler/assertion diagnostics over
trailing device logs so the single revision can address the actual failure.
Generation guidance preserves the repro's relevant layout and content size and
requires observing transient gesture behavior rather than only its settled position.
Initial-state assertions must observe live control or binding state, not a label
hardcoded to the expected value; diagnostic labels track real changes, and the
test must verify that a guarded issue interaction actually ran.
These instructions do not prove candidate adequacy: inspect whether the assertion
isolates the reported bug instead of ordinary scrolling or overscroll.
TRX verification accepts NUnit's parameterized fixture names (for example
`Issue37323(Android)`) while still requiring matching test IDs, execution counts,
and command exit status. TRX-only assertion evidence requires a recognized exception
type at the start of the runner's message or an assertion API frame in its separate
`StackTrace` field, not assertion-looking text embedded in an ordinary exception.
Messages identifying ordinary exceptions remain inconclusive even when they
include assertion frames. NUnit filters its framework frames and omits the
exception type for normal constraint failures. The verifier supplies run settings
through MSBuild's
`VSTestSetting` to enable the pinned adapter's supported `NUnit.TestOutputXml`
output, without modifying the source-pinned UI runner. Existing run settings are
not overwritten, and the environment is restored afterward. The verifier requires
the adapter's fresh, bounded assembly-named XML result and
binds its structured failed assertions to the TRX adapter, exact test/fixture name,
start/end timestamps, failure message, source stack and whole-run counters, rejecting
error labels and lifecycle sites. Each structured failed assertion's filtered stack
must terminate at the candidate body: NUnit 4 can omit a setup site even when setup
calls that body. Async dispatch frames in the aggregate failure stack are retained.
`Assert.That`, `Expected:`, and `But was:` text alone remains
ambiguous and inconclusive, with its diagnostic preserved; neither repetition nor
an unbound, missing or stale XML result upgrades that evidence.
Setup exceptions such as `Xunit.Sdk.TestClassException` do not count as assertions.
Prefix-colliding classes are excluded from the candidate's result set after
validating whole-run counters. Every failed result must belong to the intended
candidate class; an unrelated or undiscovered failure makes the run inconclusive,
even when the candidate also fails an assertion. Unrelated passing results remain
excluded from candidate identities. UI runners must report their authoritative
`TRX_RESULT_FILE`; unsupported historical runners fail explicitly.
Unit candidates contain exactly one file in one project. The verifier snapshots
the bounded diff before execution, checks source hashes before and after each
attempt, and never exports mutated source or a patch assembled after generated code ran.
