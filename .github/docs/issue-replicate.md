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
```

`--platform android|ios` is optional only when exactly one supported `platform/android`
or `platform/ios` issue label exists. `--branch main|netN.0` defaults to `main`.
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

The sample is *built*, not driven through the reported interaction. A generated
unit/XAML/UI test runs against the pinned MAUI commit in a separate credential-free
job, with at most one feedback-driven revision. A passing test means only
`not-reproduced-on-tested-revision`. A test that fails at an assertion on two
runs, with the same failing test, assertion diagnostic and source signature,
is reported as a **verified failing test candidate**, not proof that the
author's scenario was exercised or the issue is confirmed. Only that outcome labels the complete generated candidate diff as verified failing.
Both drafting attempts import the same bounded author-build record and include
its validated target framework, outcome and diagnostic in the GPT prompt.
Tool-free drafting also runs when a completed, bounded author build record reports
a failure. That does not bypass the author-build prerequisite for native verification:
the unchanged sample's target and diagnostic remain in the report, and the draft
is explicitly marked unexecuted and unverified.
The diff is untrusted code: inspect its assertions, test scope, and provenance
before applying it. Unsupported or infrastructure-failed attempts do not
invalidate the issue. No pull request or production-code change is created.

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
distinguishes a test executed in either attempt from whether the fresh confirmation
completed its named body; setup, recording and missing/stale-result failures do not
claim a second execution. First-attempt assertion values remain visible. The score never claims
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
30-second deadline instead of exporting an automatically expired clip.
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
frontend command inside the ordinary 256 KiB tail. Its four-second deadline leaves
room for the existing 20-second recording request within the NUnit wait.
Recorded UI candidates must execute exactly one named test: one matching
`Start`/`Stop` pair, one acknowledged recording action and one TRX result whose
method matches those markers.
Multiple methods, repeated cases, missing markers or a mismatched TRX are
inconclusive; the clip is discarded instead of publishing an unrelated method's
video alongside an assertion. Unit/XAML verification retains its multi-test support.
The shared NUnit source must keep the inherited fixture lifecycle unchanged.
Validation conservatively rejects lifecycle/reset hook names, including escaped
identifiers and references in comments/literals, so candidates cannot enable
per-test fixture resets after the base `Start` marker. Additional NUnit setup/teardown
attributes are unsupported as well. Put issue-specific interaction
in the one test body instead. If the framework's own setup recovery reports a
successful session recreation, the controller restarts recording on that new
session before the body completes. These checks are not an isolation boundary.
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
Before adding or executing the candidate, the verifier uses the installed
XCUITest driver's WebDriverAgent library to resolve its derived-data directory
and build for that owned simulator and the pinned SDK version. The bundled
`build-wda` command does not resolve this directory before building; its default
Xcode output can differ from the directory used by the native runner. Preparation
uses the driver's own path resolution rather than a hardcoded cache path. This
separates a cold WebDriverAgent build from the unchanged pinned runner's
session-launch timeout. Preparation has a ten-minute process
deadline, emits a bounded diagnostic tail, and fails explicitly on a build error,
timeout, or incomplete output capture; none is a candidate assertion. It does not
change the pinned framework, Appium package versions, or launch capabilities.
Successful prebuilding alone is not evidence that WebDriverAgent starts or that
the test body executes.
Native iOS UI jobs also require FFmpeg with the `libx264` encoder before running
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
a fixed 64 MiB tail scan with a hard reader deadline. Omitted context is labeled;
these diagnostics are not assertion evidence and do not create artifacts.

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
or `ios-shell-navigation`.
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
   on the chosen MAUI branches. Xcode is selected from the pinned branch's
   declared iOS SDK; a missing matching installation fails rather than silently
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
and command exit status. NUnit may omit assertion framework frames from TRX:
comparison failures are also recognized by the NUnit executor's `Assert.That`,
`Expected:`, and `But was:` diagnostics, not by generic failure text.
Setup exceptions such as `Xunit.Sdk.TestClassException` do not count as assertions.
Prefix-colliding classes are excluded from the candidate's result set after
validating whole-run counters. UI runners must report their authoritative
`TRX_RESULT_FILE`; unsupported historical runners fail explicitly.
Unit candidates contain exactly one file in one project. The verifier snapshots
the bounded diff before execution, checks source hashes before and after each
attempt, and never exports mutated source or a patch assembled after generated code ran.
