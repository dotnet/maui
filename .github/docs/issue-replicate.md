# `/issue replicate` (public issue repro)

Repository writers, maintainers, and administrators can comment on an **open issue**:

```text
/issue replicate --platform android --branch main
```

`--platform android|ios` is optional only when exactly one supported `platform/android`
or `platform/ios` issue label exists. `--branch main|netN.0` defaults to `main`.
Pull requests and commands from users without current repository write access do
not queue a run. The trigger replies with a link to the public Azure build; a
second comment reports the result. A fresh command comment starts a fresh run.

The issue author must provide exactly one repro in the issue body or an
author-written comment: a public `https://github.com/owner/repo` URL,
`https://github.com/owner/repo/tree/ref` (one URL-encoded ref segment), or a
GitHub-hosted ZIP issue attachment (linked as `[repro.zip](...)`). The latest
author comment containing a supported link takes precedence. The repository is
pinned to the explicit ref or default-branch commit; an attachment is hashed. Archives are
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
Tool-free drafting also runs when a completed, bounded author build record reports
a failure. That does not bypass the author-build prerequisite for native verification:
the unchanged sample's target and diagnostic remain in the report, and the draft
is explicitly marked unexecuted and unverified.
The diff is untrusted code: inspect its assertions, test scope, and provenance
before applying it. Unsupported or infrastructure-failed attempts do not
invalidate the issue. No pull request or production-code change is created.

The result is posted **under the originating issue**, with a concise outcome and
the same expandable-section style as `/review tests`: **Reproduction evidence**,
**Generated test candidate**, and **Follow-up**. A verified failing candidate's
hash-checked diff is embedded in the comment so it can be reviewed without
downloading anything. Oversized diffs are split across bounded continuation
comments linked from the main report, with the full patch hash and ordered code
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
repository, the immutable repository revision when applicable, and the issue
comment associated with the run. Repro links are validated against the supported
GitHub source formats before publication.
Results are updated idempotently per run, using only the authenticated publisher's
own comments; third-party markers cannot redirect or suppress publication.
Multi-comment publication creates an explicitly incomplete main report before
posting parts and finalizes it only when every part succeeds. A failed attempt
can be retried without duplicating or presenting incomplete fragments as a complete patch.
Publication failures also produce an
expandable follow-up notice, without claiming a verified outcome.

For a read-only preview, pass `-OutputPath` to `IssueReplicate.Post.ps1`; this
validates the same result data and writes the comment without calling GitHub.
Authorized fork canaries can use `-GitHubRunId` and `-GitHubRepository` instead
of `-BuildId` to link their actual GitHub Actions evidence, without pretending
they ran in Azure. Normal production publication still uses the isolated Azure
Post job and its separately scoped issue-comment token.

### Comments-only output

Neither the production pipeline nor fork canaries upload artifacts, even for
cross-job transfer. Comments are the only published reproduction output; normal
execution logs remain available on the run page. Local sample archives, SDKs,
build outputs, TRX files, and patches exist only in disposable job workspaces.

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
candidate hash are compared. When the first attempt observed no assertion, the
second job forwards its completed result and bounded UTF-8 feedback without
rerunning it. An inconclusive result must retain that feedback for the one
allowed GPT revision; missing, empty or invalid feedback fails explicitly.
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

**Production activation is blocked by the unresolved same-user evidence boundary.**
Before enabling the trigger or registering an active production pipeline,
isolate untrusted execution from the trusted collector with an enforced OS
security boundary. Generated code and its descendants must not be able to write
trusted result/output locations, and the collector must receive runner-owned
results through a channel unavailable to that code. Validate this with real
native execution on both supported platforms, including detached-writer attempts;
the current fresh-job split does not establish this boundary. Do not treat the
steps below as permission to deploy before that requirement is implemented.

Intake and the always-running posting job independently require the
`dnceng-public` collection and `System.TeamProject` exactly equal to `public`.
Both fail closed before intake or publication, including fallback notices, when
either predefined environment value is missing or belongs to another project.

1. Merge the trusted scripts, trigger, and pipeline YAML to `main`. Create a
   **separate public Azure pipeline** in `dnceng-public/public` with
   `eng/pipelines/ci-issue-replicate.yml` as its YAML path. This is not `/review`:
   `/review` queues DevDiv/DevDiv pipeline 27723. Set the GitHub Actions
   repository variable `ISSUE_REPLICATE_PIPELINE_ID` to the new public ID.
2. Give the GitHub OIDC identity Basic access in `dnceng-public` and explicitly
   allow **Queue builds** on the new pipeline (see
   [OIDC setup](trigger-azdo-pipeline-setup.md)). Protect the pipeline, its
   `main`-branch definition, and its variables from arbitrary run edits.
3. Provision three **separate protected secret variables** on this public
   definition: `ISSUE_REPRO_READ_TOKEN` (GitHub issue/repo read),
   `ISSUE_REPRO_COPILOT_TOKEN` (Copilot CLI GPT access), and
   `ISSUE_REPRO_COMMENT_TOKEN` (issue-comment-only publication). Scope them to
   their respective intake, generator, and posting tasks; do not import a
   shared MAUI secret group. Review whether your token policy permits public
   issue/repository reads, Copilot access, and issue-comment posting.
4. Confirm `ubuntu-22.04` and `macOS-26` are **fresh Microsoft-hosted
   agents**, with Android KVM, Appium, appropriate Xcode/simulator, and workloads
   on the chosen MAUI branches. Xcode is selected from the pinned branch's
   declared iOS SDK; a missing matching installation fails rather than silently
   using an incompatible version. Do not enable iOS on a persistent/shared macOS
   agent. Run an authorized test issue through the pipeline before announcing
   availability; YAML parsing alone does not validate Azure template expansion
   or image capabilities.
5. For scheduled missed-webhook recovery, set
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
queueing again. Incomplete history or multiple matching runs fail explicitly.
Recovery's transient `eyes` reaction is cleared after dispatch and is never a
terminal latch; only the bot's successful build marker or `rocket` acknowledgement
completes recovery.

The GitHub gate and Azure intake/posting check out trusted `main` without
persisting Git credentials. Sample and test jobs use `checkout: none`, receive
bounded job data, and never receive the Copilot or issue-posting tokens. The
generator uses GPT with tools disabled and never executes sample or generated
code. A public pipeline alone is **not** a security boundary: do not grant
execution jobs privileged access or reuse the trusted job agents for them.

Execution checkouts are shallow, without partial-clone extensions that the pinned
SourceLink tooling does not support. Before restoring tools, native jobs use the
pinned checkout's Arcade bootstrap to install the exact `global.json` SDK from
public feeds; the shared provisioner does not replace it with a hard-coded SDK.
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
