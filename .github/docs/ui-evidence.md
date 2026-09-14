# AI-assisted UI evidence workflow

`/ui-evidence` is an optional, maintainer-triggered analysis workflow on top of
the [non-AI UI evidence measurement layer](../../docs/ui-evidence.md). The
measurement layer independently builds and runs trusted scenarios, computes
deterministic comparisons, and seals artifacts. It does not depend on this skill,
model credentials, or GitHub automation.

This follow-up adds the AI skill, bounded summary/report policy, manual GitHub
orchestration, and advisory PR comments. It adds no native scenarios or changes
to the measurement runner's verdicts.

## Architecture

1. Trusted workflow steps validate the AI report contracts, resolve an open PR
   targeting `main`, and pin its merge-base, head, and harness SHAs.
2. The non-AI selector and request scripts choose applicable scenario/platform
   pairs and seal their identities.
3. A separate safe-output job queues `eng/pipelines/ci-ui-evidence.yml` through
   GitHub OIDC and Azure DevOps, using the trusted harness revision.
4. The measurement pipeline builds both revisions and executes
   `base-1`, `head-1`, `head-2`, `base-2`, then validates and seals in a clean job.
5. The workflow downloads and validates sealed bundles and starts a trusted
   interpretation follow-up.
6. `Build-UiEvidenceAgentSummary.ps1` produces bounded normalized input. The
   model explains the deterministic result without changing its verdict.
7. A separate safe-output job validates the report and updates one bot-owned
   marker comment only while the PR is open, targets `main`, and retains the
   measured head. A stale write is rolled back.

The model does not receive PR text, changed filenames, screenshots, raw UI
strings, logs, or visual trees. It cannot modify source, labels, reviews,
branches, or pull requests. Appium remains the external native/visual oracle;
DevFlow is supporting structural evidence, not causal framework attribution.

## Repository setup

Land and configure the independent measurement pipeline first. For this optional
workflow, additionally configure:

- repository variable `MAUI_UI_EVIDENCE_PIPELINE_ID` with that pipeline's
  definition ID in `dnceng-public/public`;
- secrets `AZDO_TRIGGER_TENANT_ID` and `AZDO_TRIGGER_CLIENT_ID`;
- the `copilot-pat-pool` environment and usable `COPILOT_PAT_0` through
  `COPILOT_PAT_9` pool entries;
- the federated subject
  `repo:dotnet/maui:environment:copilot-pat-pool`, allowing the queue job to
  request an Azure DevOps token with the identity's approved queue permission.

See [OIDC setup](trigger-azdo-pipeline-setup.md#optional-ui-evidence-workflow).
This does not require the performance analyzer or its PRs. If the pipeline ID is
absent, evidence remains incomplete and the report is `inconclusive`, rather
than claiming a measurement ran.

## Usage

After deployment, a maintainer with write access comments on an open PR targeting
`main`:

```text
/ui-evidence
```

The initial workflow run selects and queues evidence. The follow-up interprets
validated artifacts and posts the advisory report.

For a suppressed-output run:

```powershell
gh aw run ui-evidence --ref main `
  -f pr_number=<number> `
  -f suppress_output=true
```

Suppressed-output mode neither queues measurement jobs nor posts comments. It
can inspect selection or existing follow-up evidence; it is not an end-to-end
device run.

## Local validation

The non-AI contracts and runner tests remain documented with the
[measurement layer](../../docs/ui-evidence.md#local-tooling). Validate this
follow-up separately:

```powershell
pwsh .github\skills\ui-evidence\tests\UiEvidenceSkill.Tests.ps1
gh aw compile ui-evidence
```

Use the repository-pinned `gh-aw` compiler version and commit
`.github/workflows/ui-evidence.md` with its generated
`.github/workflows/ui-evidence.lock.yml`. AI report tests run in the GitHub
workflow, not in the independent measurement pipeline.

## Report policy and limits

The empirical verdict is deterministic and immutable. The report must include
the measured head, coverage, each selected scenario/platform's evidence, and
limitations. `Validate-UiEvidenceReport.ps1` rejects missing sections, changed
verdicts, and prohibited merge-safety claims.

An Android-only path selects Android, not Windows. Unsupported platforms remain
unmapped. Mixed, sampled, or missing coverage cannot become a whole-PR
absence-of-change conclusion. Windows absence-of-change remains `inconclusive`
because the app and driver share a worker identity; this also makes a combined
Android/Windows result inconclusive when neither reports a positive change.

Only two initial-state smoke scenarios exist. Navigation, gestures, grouped-item
mutations, iOS, and Mac Catalyst are not comprehensively covered. A selected path
is not proof of the PR's exact behavior, and an observed change may be intentional.

`no-difference-observed` must never be described as clean, safe to merge, or no
regression. This result is advisory and is not a merge gate.
