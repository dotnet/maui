---
name: net11-release-readiness
description: Checks whether a .NET 11 MAUI preview/release target is ready to release. Use when asked "is net11 preview ready", "is net11-preview6 ready to release", "release ready for net11", or similar release-readiness questions. Runs a deterministic checklist covering branch setup, version iteration, Maestro PRs, release PRs, CI truth handoff, Xcode/ICM readiness, and sanitized dnceng/internal status.
metadata:
  author: dotnet-maui
  version: "0.1"
compatibility: Requires GitHub CLI (gh) authenticated with access to dotnet/maui. Optional internal validation requires maintainer access to dnceng/internal and local Azure DevOps tooling.
---

# Net11 Release Readiness

Use this skill when a maintainer asks whether a .NET 11 release target is ready, for example:

- "Is net11-preview6 ready to release?"
- "Is release/11.0.1xx-preview6 ready?"
- "Check net11 release readiness."
- "What is blocking the next net11 preview?"

## What this checks

The deterministic script checks:

1. **Target resolution** - maps aliases such as `net11-preview6` to `release/11.0.1xx-preview6`.
2. **Branch existence** - verifies the target branch exists.
3. **Preview version iteration** - checks the release branch has the expected `PreReleaseVersionIteration` value and reports the `net11.0` preview-next value.
4. **Maestro/dependency PRs** - lists open `dotnet-maestro` PRs targeting the release branch or `net11.0`, including review/merge/conflict state.
5. **Release branch PRs** - lists non-Maestro open PRs targeting the release branch or `net11.0`.
6. **Release blockers and KBEs** - surfaces open P/0, P/1, and Known Build Error issues with net11/release relevance.
7. **CI truth handoff** - reports `INSUFFICIENT_DATA` until #35052 provides structured CI evidence; do not infer readiness from GitHub checks alone.
8. **Xcode / ICM readiness** - reports required Xcode values and reminds maintainers to verify hosted image readiness / ICM status.
9. **dnceng/internal release pipeline status** - public output is sanitized. Detailed internal diagnostics must stay local/internal.

## Local usage

```bash
pwsh .github/skills/net11-release-readiness/scripts/Get-Net11ReleaseReadiness.ps1 -Target net11-preview6
```

Common targets:

```bash
pwsh .github/skills/net11-release-readiness/scripts/Get-Net11ReleaseReadiness.ps1 -Target auto
pwsh .github/skills/net11-release-readiness/scripts/Get-Net11ReleaseReadiness.ps1 -Target net11.0
pwsh .github/skills/net11-release-readiness/scripts/Get-Net11ReleaseReadiness.ps1 -Target release/11.0.1xx-preview6
```

For local-only internal validation, keep public output sanitized:

```bash
pwsh .github/skills/net11-release-readiness/scripts/Get-Net11ReleaseReadiness.ps1 \
  -Target net11-preview6 \
  -IncludeInternal \
  -InternalBuildId <build-id>
```

## Security boundary

The script is designed to produce a public-safe markdown block between:

```text
<!-- NET11_RELEASE_READY_BEGIN -->
...
<!-- NET11_RELEASE_READY_END -->
```

Only content inside that block should be posted to public GitHub issues. Do not include internal logs, artifacts, private URLs, raw errors, secret names, account identifiers, or detailed dnceng/internal failure payloads in public output.

## Ownership boundaries

- #35052 owns CI evidence extraction and failure classification.
- This skill consumes that output when available and otherwise reports CI as `INSUFFICIENT_DATA`.
- This skill does not approve, merge, rerun, promote, or mutate Maestro/darc state.
