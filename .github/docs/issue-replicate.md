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
author-written comment: a public `https://github.com/owner/repo` URL or a
GitHub-hosted ZIP issue attachment (linked as `[repro.zip](...)`). The latest
author comment containing a supported link takes precedence. The repository is
pinned to its default-branch commit; an attachment is hashed. Archives are
limited to 10 MiB compressed, 40 MiB expanded, 512 safe entries, and one
platform-targeting `.csproj`. A repro with multiple project files, external
downloads, non-GitHub attachments, or an inaccessible dependency may be
inconclusive. **Do not include credentials or private data in a public repro,
issue comment, or generated artifact.**

The sample is *built*, not driven through the reported interaction. A generated
unit/XAML/UI test runs against the pinned MAUI commit in a separate credential-free
job, with at most one feedback-driven revision. A passing test means only
`not-reproduced-on-tested-revision`. A test that fails at an assertion on two
runs is reported as a **verified failing test candidate**, not proof that the
author's scenario was exercised or the issue is confirmed. The `Verified1` or
`Verified2` public build artifact contains a `test.patch` only for that outcome.
The patch is untrusted code: inspect its assertions, test scope, and provenance
before applying it. Unsupported or infrastructure-failed attempts do not
invalidate the issue. No pull request or production-code change is created.

The result is posted **under the originating issue**, with a concise outcome and
the same expandable-section style as `/review tests`: **Reproduction evidence**,
**Generated test candidate**, and **Follow-up**. A verified failing candidate's
hash-checked diff is embedded in the comment so it can be reviewed without
downloading artifacts. Oversized diffs are explicitly linked in full rather than
truncated; passing, unsupported, and inconclusive results do not publish a patch.
Results are updated idempotently per run. Publication failures also produce an
expandable follow-up notice, without claiming a verified outcome.

For a read-only preview, pass `-OutputPath` to `IssueReplicate.Post.ps1`; this
validates the same artifacts and writes the comment without calling GitHub.
Authorized fork canaries can use `-GitHubRunId` and `-GitHubRepository` instead
of `-BuildId` to link their actual GitHub Actions evidence, without pretending
they ran in Azure. Normal production publication still uses the isolated Azure
Post job and its separately scoped issue-comment token.

iOS UI execution uses a hosted simulator, not a physical iPhone. Reports labeled
`repro:device-only` still need physical-device validation; a simulator pass or an
unsupported generated candidate cannot rule out the reported behavior.

## Deployment and isolation

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
   artifact metadata and issue comment posting.
4. Confirm `ubuntu-22.04` and `macOS-15-arm64` are **fresh Microsoft-hosted
   agents**, with Android KVM, Appium, appropriate Xcode/simulator, and workloads
   on the chosen MAUI branches. Do not enable iOS on a persistent/shared macOS
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

The GitHub gate and Azure intake/posting check out trusted `main` without
persisting Git credentials. Sample and test jobs use `checkout: none`, receive
bounded artifacts, and never receive the Copilot or issue-posting tokens. The
generator uses GPT with tools disabled and never executes sample or generated
code. A public pipeline alone is **not** a security boundary: do not grant
execution jobs privileged access or reuse the trusted job agents for them.

Execution checkouts are shallow, without partial-clone extensions that the pinned
SourceLink tooling does not support. Sample restore uses the pinned MAUI
`NuGet.config` public feeds plus nuget.org, so servicing SDK dependencies remain
available even though the author's sample is extracted outside the MAUI checkout.
The sample job installs a separate copy of the exact public SDK distribution from
the pinned `global.json`, retaining its stock MAUI manifests and installing the
selected platform's public MAUI workload without manifest updates. Repository-native
provisioning alone removes those manifests and cannot supply template samples'
`$(MauiVersion)` defaults. No package version is injected into the author's project.
Restore is restricted to its selected, existing platform TFM rather than requiring
unrequested Android or MacCatalyst workloads. The sample's bundled public MAUI
version may differ from the issue's reported version; it is not the framework under
test. Verification retains its own source-pinned SDK and native workloads.
Verification builds `Microsoft.Maui.BuildTasks.slnf` before compiling candidate
tests or the HostApp. Feedback prioritizes compiler/assertion diagnostics over
trailing device logs so the single revision can address the actual failure.
Generation guidance preserves the repro's relevant layout and content size and
requires observing transient gesture behavior rather than only its settled position.
These instructions do not prove candidate adequacy: inspect whether the assertion
isolates the reported bug instead of ordinary scrolling or overscroll.
TRX verification accepts NUnit's parameterized fixture names (for example
`Issue37323(Android)`) while still requiring matching test IDs, execution counts,
and command exit status. NUnit may omit assertion framework frames from TRX:
comparison failures are also recognized by the NUnit executor's `Assert.That`,
`Expected:`, and `But was:` diagnostics, not by generic failure text.
