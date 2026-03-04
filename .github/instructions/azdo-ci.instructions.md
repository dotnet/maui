---
applyTo: "**"
---

# Azure DevOps CI Investigation Guidelines

When investigating CI builds, test failures, pipeline status, or any Azure DevOps (ADO) data for this repository, **always prefer the `az` CLI** over raw REST API calls (`curl`, `Invoke-RestMethod`, etc.).

## Why `az` CLI?

The `az` CLI with the `azure-devops` extension provides:
- **Authenticated access** — handles tokens automatically, no manual header management
- **Richer data** — access to build logs, artifacts, test results, and timeline details that may be restricted for anonymous callers
- **Structured output** — native `--output json/table/tsv` formatting
- **Pagination** — automatic handling of large result sets
- **Rate limit resilience** — authenticated requests have much higher rate limits than anonymous

## Required: Check `az` CLI Availability First

**Before making ANY Azure DevOps API call**, check if `az` is installed and authenticated:

```bash
# Check if az is installed and logged in
az account show 2>/dev/null && az extension show --name azure-devops 2>/dev/null
```

### If `az` is NOT installed or NOT logged in

**STOP and tell the user:**

```
⚠️ The Azure CLI (az) is not installed/authenticated. CI investigation is much more
reliable with az CLI. To set up:

  1. Install:  brew install azure-cli        (macOS)
               winget install Microsoft.AzureCLI  (Windows)

  2. Login:    az login

  3. Extension: az extension add --name azure-devops

  4. Defaults: `az devops configure --defaults organization=https://dev.azure.com/dnceng-public project=public`

Once set up, I can provide much richer CI analysis.
```

Then proceed with anonymous access as a fallback, but **always mention the limitation**.

## Common `az` Commands for CI Investigation

### Get builds for a PR
```bash
# List builds triggered by a PR
az pipelines runs list --org https://dev.azure.com/dnceng-public --project public --query "[?triggerInfo.\"pr.number\"=='PR_NUMBER']" --output table

# Or use the pr-build-status skill scripts (which use gh CLI):
pwsh .github/skills/pr-build-status/scripts/Get-PrBuildIds.ps1 -PrNumber PR_NUMBER
```

### Get build details and timeline
```bash
# Build summary
az pipelines runs show --id BUILD_ID --org https://dev.azure.com/dnceng-public --project public

# Build timeline (Critical for finding WHICH step failed)
# This returns a large JSON. Filter to failed records to avoid token limits:
az devops invoke --area build --resource timeline --route-parameters buildId=BUILD_ID project=public --org https://dev.azure.com/dnceng-public --query "records[?result=='failed']" --output table
```

### Get build logs
```bash
# 1. Find the logId from the timeline (look for 'log' object in failed records)
# 2. Download/Show the log:
az devops invoke --area build --resource logs --route-parameters buildId=BUILD_ID logId=LOG_ID project=public --org https://dev.azure.com/dnceng-public --output json
```

### Get build artifacts (binlogs)
Artifacts often contain `.binlog` files which are critical for build failures.

```bash
# List artifacts
az pipelines runs artifact list --run-id BUILD_ID --org https://dev.azure.com/dnceng-public --project public --output table

# Download specific artifact (e.g., 'binlog')
az pipelines runs artifact download --run-id BUILD_ID --artifact-name "binlog" --path . --org https://dev.azure.com/dnceng-public --project public
```

### Get test results
```bash
# List test runs for a build
az devops invoke --area test --resource runs --query-parameters buildUri=vstfs:///Build/Build/BUILD_ID --org https://dev.azure.com/dnceng-public --project public --output json

# Get test results from a run
az devops invoke --area test --resource results --route-parameters runId=RUN_ID project=public --org https://dev.azure.com/dnceng-public --query-parameters "top=100" "outcomes=Failed" --output json
```

## Fallback: Anonymous REST API

Only use direct REST API calls if `az` CLI is unavailable:

```bash
# Anonymous (works for dnceng-public only, subject to rate limits)
curl -s "https://dev.azure.com/dnceng-public/public/_apis/build/builds/BUILD_ID?api-version=7.0"
```

**Always note in your response** that results may be limited without `az` authentication.

## Use Existing Skills When Appropriate

The repository has a `pr-build-status` skill with purpose-built scripts:
- `Get-PrBuildIds.ps1` — maps PR number to ADO build IDs (uses `gh` CLI)
- `Get-BuildInfo.ps1` — build status, stages, failed jobs
- `Get-BuildErrors.ps1` — build errors and test failures
- `Get-HelixLogs.ps1` — Helix console logs for device/integration test failures

**Use these scripts for structured queries.** Use `az` CLI directly for ad-hoc or deeper investigation.

## ADO Organization Reference

| Organization | Project | Access | Use |
|---|---|---|---|
| `dnceng-public` | `public` | Anonymous (with limits) | Open-source CI builds |
| `dnceng` | `internal` | **Auth required** | Internal builds (requires org membership) |

For `dnceng/internal` builds, `az` CLI authentication is **mandatory** — anonymous access will always fail.
