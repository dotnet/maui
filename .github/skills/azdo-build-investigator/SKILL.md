---
name: azdo-build-investigator
description: "Investigate CI failures for dotnet/maui PRs — build errors, Helix test logs, and binlog analysis. Use when asked about failing checks, CI status, test failures, 'why is CI red', 'build failed', 'what's failing on PR', Helix failures, or device test failures."
metadata:
  author: dotnet-maui
  version: "2.0"
---

# dotnet/maui CI Investigation Context

This skill provides MAUI-specific context for CI investigation. Use it together with the `ci-analysis` skill (loaded from the `dotnet-dnceng@dotnet-arcade-skills` plugin via `.github/copilot/settings.json`).

> **First**: invoke the `ci-analysis` skill — it handles the core investigation workflow using `Get-CIStatus.ps1` and `gh` CLI (with MCP tools as optional enhancements if available). This skill provides MAUI-specific corrections and context on top of that.

## Script Location

The `ci-analysis` skill and its `Get-CIStatus.ps1` script are loaded automatically from the `dotnet/arcade-skills` plugin (configured in `.github/copilot/settings.json` via `enabledPlugins`). The CLI caches scripts to `~/.copilot/installed-plugins/dotnet-arcade-skills/`. No manual download is needed.

## MAUI CI Pipelines

> ⚠️ The `ci-analysis` skill's reference doc lists `maui-public` as the MAUI pipeline — **this is outdated**. The correct pipeline names are below.

| Pipeline Name | Definition ID | Purpose |
|---------------|---------------|---------|
| `maui-pr` | **302** | Main build — check this first |
| `maui-pr-devicetests` | **314** | Helix device tests (iOS, Android, Windows, MacCatalyst) |
| `maui-pr-uitests` | **313** | Appium-based UI tests |

**Organization**: `dnceng-public` / project `public`

**Investigation priority order**: `maui-pr` → `maui-pr-devicetests` → `maui-pr-uitests`

Most failures are in `maui-pr`. Device test failures appear in `maui-pr-devicetests`. Focus on the first failing pipeline before checking others.

## MAUI-Specific Quirks

### XHarness Exit-0 Blind Spot

XHarness (used for iOS/Android device tests in `maui-pr-devicetests`) **exits with code 0 even when tests fail**. This means:
- The ADO job shows ✅ "Succeeded"
- `ci-analysis` may report no failures
- But actual test failures are hidden inside the Helix work items

**How to detect hidden test failures**: Query the `ResultSummaryByBuild` Helix API endpoint:
```
GET https://helix.dot.net/api/2019-06-17/jobs/{correlationId}/aggregated
```
Look for `Failed` > 0 in the response even when the ADO build job shows green.

When `ci-analysis` reports a `maui-pr-devicetests` build as passing but the PR has a `s/agent-gate-failed` label or the user suspects device test failures, always cross-check Helix `ResultSummaryByBuild`.

### Container Artifact Binlogs

MAUI build artifacts are **Container type**, not `PipelineArtifact`. This means:
- `az pipelines runs artifact download` does **not** work for binlogs
- Artifact names are like `Windows_NT_Build Windows (Debug)_Attempt1` (not `binlog`)
- Download requires a Bearer token from `az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798`
- Use the ADO File Container API: `/_apis/resources/Containers/{id}?api-version=5.0-preview&$format=OctetStream`

If available, use the `mcp-binlog-tool` MCP server to analyze downloaded `.binlog` files. This is optional — the core investigation workflow works without it via `gh` CLI and REST APIs.

## Common MAUI Failure Patterns

| Pattern | Where | Notes |
|---------|-------|-------|
| `error CS####` | `maui-pr` | C# compiler error — check file/line |
| `error XA####` | `maui-pr` | Android build error |
| `XamlC` | `maui-pr` | XAML compiler — usually missing type or bad binding |
| `XHarness timeout` | `maui-pr-devicetests` Helix logs | Test killed by infrastructure; may be transient |
| `No test result files found` | `maui-pr-devicetests` Helix logs | Tests never ran or app crashed on launch |
| UI test screenshot diff | `maui-pr-uitests` | Visual regression; check baseline images |
