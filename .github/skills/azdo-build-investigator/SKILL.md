---
name: azdo-build-investigator
description: "Investigate CI failures for dotnet/maui PRs and the nightly/official signed build — build errors, Helix test logs, and binlog analysis. Use when asked about failing checks, CI status, test failures, 'why is CI red', 'build failed', 'what's failing on PR', 'is this PR ready to merge', Helix failures, device test failures, or 'nightly is broken', 'nightly build failing', 'inflight feed stale', 'dogfood feed stopped updating', 'official build failed'."
metadata:
  author: dotnet-maui
  version: "3.0"
---

# dotnet/maui CI Investigation Context

This skill provides MAUI-specific context for **interactive** CI investigation —
answering "why is CI red?" and "is this PR ready to merge?". Use it together with the
`ci-analysis` skill (loaded from the `dotnet-dnceng@dotnet-arcade-skills` plugin via
`.github/copilot/settings.json`).

> **First**: invoke the `ci-analysis` skill — it handles the core investigation workflow
> using `Get-CIStatus.ps1` and `gh` CLI (with MCP tools as optional enhancements if
> available). This skill provides MAUI-specific corrections and context on top of that.

For the **automated** path (a maintainer comments `/review tests` on a PR), the
`review-test-failures` skill does the same classification deterministically and posts a
merge-readiness comment. Both skills reason from the same shared facts below.

## MAUI CI facts (canonical)

All MAUI-specific facts live in **`.github/docs/maui-ci-facts.md`** — read it. It covers:

- **Pipeline names + definition IDs** (`maui-pr` 302, `maui-pr-devicetests` 314,
  `maui-pr-uitests` 313), org/project, and investigation priority order.
- **AzDO data sources** (anonymous public build/timeline/log REST APIs; `_apis/test` is
  optional enrichment).
- **XHarness exit-0 blind spot** and the Helix `aggregated` endpoint for hidden
  device-test failures.
- **Container artifact binlogs** (Bearer token + File Container API; `mcp-binlog-tool`).
- **Test count deduplication** (group by test name + OS platform).
- **Baseline comparison** (is the failure already red on the base branch?).
- **Visual baseline** and **platform mismatch** guidance.
- **Gradle / Maven / CFSClean** signatures and the `./eng/ingest-maven-deps.sh` fix.
- **Common failure patterns** table.
- **Merge-readiness criteria** — the shared definition of `Ready to merge` / `Not ready`
  / `Needs human investigation` / `Insufficient data` / `No failures found`.

Do not restate those facts from memory; change them in the canonical doc only.

## Script location

The `ci-analysis` skill and its `Get-CIStatus.ps1` script are loaded automatically from
the `dotnet/arcade-skills` plugin (configured in `.github/copilot/settings.json` via
`enabledPlugins`). The CLI caches scripts to
`~/.copilot/installed-plugins/dotnet-arcade-skills/`. No manual download is needed.

> ⚠️ The `ci-analysis` skill's reference doc lists `maui-public` as the MAUI pipeline —
> **this is outdated**. Use the pipeline names in `.github/docs/maui-ci-facts.md`.

## Using this skill

1. Invoke `ci-analysis` to gather build/test status for the PR.
2. Apply the MAUI corrections from `.github/docs/maui-ci-facts.md` — especially the
   correct pipeline names, the XHarness exit-0 cross-check on `maui-pr-devicetests`, and
   test-count deduplication.
3. When asked whether a PR is ready to merge, compare failures against the base branch
   and apply the merge-readiness criteria from the canonical doc.
4. **Escalation:** for deep Helix log analysis (recurring failures, machine-specific
   issues, comparing passing vs. failing runs), escalate to the `helix-investigation`
   skill.

**When CI hasn't run:** community PRs require a maintainer to trigger builds — see the
canonical doc.

## Nightly / Official Signed Build (inflight dogfood feed)

When asked about the **nightly build**, the **inflight feed**, or the **dogfood feed** being broken or stale — e.g. "nightly is broken please fix", "the dogfood feed stopped updating", or the `release-readiness` ❌ staleness banner fired — the pipeline to investigate is **NOT** `maui-pr`. It's the official signed build:

| | |
|---|---|
| **Pipeline** | `dotnet-maui` |
| **Definition ID** | **1095** |
| **Org / Project** | `dnceng` / `internal` — requires internal access (**not** the anonymous `dnceng-public` org the PR pipelines use) |
| **Defined by** | [`eng/pipelines/ci-official.yml`](../../../eng/pipelines/ci-official.yml) |
| **Schedule** | daily cron `0 5 * * *` (05:00 UTC) on `main`, `net*.0`, and `inflight/current` |
| **Feeds** | the `ci.inflight` dogfood NuGet stream — the official build on `inflight/current` produces the "shipping next" bits that `release-readiness` tracks. When this build is red, that feed goes stale and the release-readiness banner turns ❌. |

**Investigate** (the AzDO MCP tools accept this internal pipeline — pass `org: dnceng`, `project: internal`):
- `azdo_builds` with `definitionId: 1095`, `branch: refs/heads/inflight/current` → find the newest `failed` run (and confirm whether it's a one-off or a multi-day streak)
- `azdo_search_timeline` (filter `failed`) on that build → identifies the failing job/step + its `logId`
- `azdo_search_log` on that `logId` → the actual error

**Known failure surface:** the most common nightly break is the **`Pack Windows`** job → **"Build Workloads, Sign & Publish"** step ([`eng/pipelines/arcade/stage-pack.yml`](../../../eng/pipelines/arcade/stage-pack.yml), building `src/Workload/workloads.csproj`). Recurring signature:
```
src\Workload\workloads.csproj(30,3): error MSB4019: The imported project
"...\artifacts\packages\Release\Shipping\vs-workload.props" was not found
```
`vs-workload.props` is generated by the `_GenerateVSWorkloadProps` target in `src/Workload/Microsoft.NET.Sdk.Maui.Manifest/Microsoft.NET.Sdk.Maui.Manifest.csproj` (`AfterTargets="Build"`) during the **preceding** `Pack, Sign` step, then imported by `workloads.csproj`. If it's missing at import time, the manifest pack didn't stage it to `ArtifactsShippingPackagesDir`. Note the **macOS `Pack` job can succeed while `Pack Windows` fails** — a green macOS leg does NOT mean the dogfood feed is healthy.

**Root cause of the Jun 2026 `inflight/current` outage (binlog-proven, fixed by [#36089](https://github.com/dotnet/maui/pull/36089)):** the workload packs (`Microsoft.NET.Sdk.Maui.Manifest`, `Microsoft.Maui.Sdk`) target **netstandard** but are shipping packages, so they set `<IsPackable>true</IsPackable>` explicitly. PR #32203 ("Don't pack .NET Standard") added a *blanket* `<IsPackable Condition="$(TargetFramework.Contains('netstandard'))">false</IsPackable>` to `Directory.Build.targets` — which is auto-imported at the **end** of every project, so it overrode the packs' explicit `true` → `IsPackable=false`. With `IsPackable=false`, NuGet `Pack` no-ops; and because the **"Pack, Sign" step runs `-pack` *without* `-build`** (only the later workloads step uses `-build`), the `Build` target never ran → `_GenerateVSWorkloadProps` (`AfterTargets="Build"`) never ran → `vs-workload.props` was never written → the line-30 import threw `MSB4019`. The fix guards the blanket rule with `and '$(IsPackable)' == ''` so explicit opt-ins survive. **This regression was `inflight/current`-only** (#32203's commit `543b1ebeb7` is not yet on `main`/`net10.0`) — so when it forward-ports, it must carry the guard or the break reappears. The general lesson when triaging this failure: confirm `IsPackable` actually evaluates `true` for the workload packs in the `pack.binlog`, and that the pack step's `Build` (hence `_GenerateVSWorkloadProps`) actually ran.

> Distinguish from the **release** pipelines: `ci-official-release.yml` and `maui-release-internal.yml` are separate definitions used for shipping releases, not the nightly inflight dogfood build.
