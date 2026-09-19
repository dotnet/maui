---
name: check-pr-ui-evidence
description: >-
  Manually build, run, and inspect local deterministic .NET MAUI PR UI evidence.
  USE FOR: an explicitly requested local before/after UI comparison using trusted
  Appium and DevFlow smoke scenarios, or inspection of its local evidence.
  DO NOT USE FOR: creating or queueing pipelines, GitHub workflow dispatch,
  automatic comments, merge approval, or Workbench Markdown-flow authoring.
compatibility: PowerShell 7, git, .NET with the selected platform workload, Appium, and an explicitly selected local test target.
---

# Local PR UI evidence

Run this skill manually from Copilot in the repository. The conversation selects
the work; deterministic scripts own all measurement and comparison results.
No pipeline registration, service identity, Copilot PAT pool, GitHub workflow, or
cloud queue permission is required.

For example:

> Use check-pr-ui-evidence to compare PR 38341 locally on a dedicated Android
> emulator. Keep every artifact local and report the findings here.

## Scope and authority

- Bind one repository, PR, full base/head/harness SHAs, selected scenario/platform,
  explicit device or desktop app, and a fresh local output directory.
- Read-only PR metadata retrieval is allowed when needed to resolve that request.
  Never push, post comments/reviews, dispatch workflows, queue builds, register
  pipelines, or change cloud permissions as part of this skill.
- Ask only for missing or ambiguous targets. Never pick the first device, reuse a
  busy emulator, or replace another app under investigation.
- Request/context preparation is inert. Do not treat a request JSON file as
  evidence that a build or UI run occurred.
- Do not bypass a restricted test-agent or Workbench approval boundary. This
  skill uses the repository's compiled measurement harness; saved Workbench flows
  belong to their own author/run workflow.
- Use a disposable local environment for untrusted PR code. Removing environment
  tokens is not a sandbox and cannot protect credentials stored elsewhere under
  the same workstation identity.

## Procedure

Follow [the local workflow](references/local-workflow.md). Do not invent an
alternative fixture, suppress a build failure, change an assertion, or substitute
the same binary for both sides.

1. Resolve exact source identities and inspect the existing scenario. For a
   historical or merged PR, establish the historical base explicitly; today's
   target-branch tip can make `merge-base` equal the already-merged head.
2. Run `eng/scripts/New-UiEvidenceContext.ps1`. It creates `selection.json`,
   `requests.json`, and a registry snapshot only; it performs no fetch, build,
   device action, or remote write.
3. Inspect `selectionStatus` and coverage. Exit `3` means no ready request. Do not
   manufacture a pass for unmapped, unsupported, or overflowed selection.
4. Choose the exact requested scenario/platform entry. Use two separate pinned
   checkouts with the same committed trusted harness. Validate the pinned
   DevFlow inputs before building.
5. Build both Release apps, or use explicitly identified previously built apps.
   Generate complete app manifests and prepare the paired payload with the
   existing trusted scripts.
6. Confirm target ownership and Appium device binding. Run one full
   `base-1`, `head-1`, `head-2`, `base-2` sequence with `-CaptureOnly`.
7. Inspect every `run-result.json`; a successful process exit only means results
   were written. Compare in a separate trusted process, seal, and validate the
   completed bundle before reporting it.
8. Retain incomplete attempts, logs, and exact identities. If a transient
   infrastructure problem justifies a user-authorized repeat, repeat the whole
   sequence into a new session directory, not selected samples.
9. Stop only processes and forwards created for this experiment. Leave unrelated
   devices, working trees, credentials, and global settings untouched.

## Evidence layout

Use a caller-owned directory outside both PR-controlled build checkouts:

```text
session\
  selection.json
  requests.json
  scenarios.json
  bundles\
    <requestKey>\
      request.json
      comparison-summary.json
      evidence-seal.json
      base-run1\
      head-run1\
      head-run2\
      base-run2\
```

The context helper embeds full keyed requests in `selection.json`, so manual
consumers do not need a GitHub precompute job. Keep raw logs and failed attempts
beside, not inside or overwriting, a validated final bundle.

## Report

Report the measured base/head/harness SHAs, actual target, scenario, coverage,
each run's status, deterministic verdict, and local evidence paths. Distinguish
build/tool failures from product symptoms. Say which planned requests did not
run and why.

`no-difference-observed` describes only the exercised scenario. It is not a clean
whole-PR result, proof of a specific fix, or merge approval. The current
CollectionView fixture does not mutate grouped items, and the layout fixture has
no FlexLayout. Never promote their broad path mappings into evidence of those
unexercised behaviors.

Windows absence-of-change remains inconclusive because the app and driver share
a worker identity. No iOS or Mac Catalyst scenario is available.

For a separately requested AI narrative, the optional `ui-evidence` skill can
consume the validated local context and bundles. It is not a prerequisite for
these measurements and must not reinterpret missing evidence as success.
