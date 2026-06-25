---
name: azdo-build-investigator
description: "Investigate CI failures for dotnet/maui PRs — build errors, Helix test logs, and binlog analysis. Use when asked about failing checks, CI status, test failures, 'why is CI red', 'build failed', 'what's failing on PR', 'is this PR ready to merge', Helix failures, or device test failures."
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
