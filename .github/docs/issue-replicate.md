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
