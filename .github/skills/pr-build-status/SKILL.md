---
name: pr-build-status
description: "Retrieve Azure DevOps build information for GitHub Pull Requests, including build IDs, stage status, failed jobs, and Helix console logs for any Helix-based test failures."
metadata:
  author: dotnet-maui
  version: "1.1"
compatibility: Requires GitHub CLI (gh) authenticated with access to dotnet/maui repository.
---

# PR Build Status Skill

Retrieve Azure DevOps build information for GitHub Pull Requests, including Helix test logs.

## Tools Required

This skill uses `bash` together with `pwsh` (PowerShell 7+) to run the PowerShell scripts. No file editing or other tools are required.

## When to Use

- User asks about CI/CD status for a PR
- User asks about failed checks or builds
- User asks "what's failing on PR #XXXXX"
- User wants to see test results
- **User asks about Helix failures (device tests, integration tests, etc.)**
- **User needs to debug why tests are failing on Helix infrastructure**

## Scripts

All scripts are in `.github/skills/pr-build-status/scripts/`

### 1. Get Build IDs for a PR
```bash
pwsh .github/skills/pr-build-status/scripts/Get-PrBuildIds.ps1 -PrNumber <PR_NUMBER>
```

### 2. Get Build Status
```bash
pwsh .github/skills/pr-build-status/scripts/Get-BuildInfo.ps1 -BuildId <BUILD_ID>
# For failed jobs only:
pwsh .github/skills/pr-build-status/scripts/Get-BuildInfo.ps1 -BuildId <BUILD_ID> -FailedOnly
```

### 3. Get Build Errors and Test Failures
```bash
# Get all errors (build errors + test failures)
pwsh .github/skills/pr-build-status/scripts/Get-BuildErrors.ps1 -BuildId <BUILD_ID>

# Get only build/compilation errors
pwsh .github/skills/pr-build-status/scripts/Get-BuildErrors.ps1 -BuildId <BUILD_ID> -ErrorsOnly

# Get only test failures
pwsh .github/skills/pr-build-status/scripts/Get-BuildErrors.ps1 -BuildId <BUILD_ID> -TestsOnly
```

### 4. Get Helix Console Logs
```bash
# List all Helix work items and their status
pwsh .github/skills/pr-build-status/scripts/Get-HelixLogs.ps1 -BuildId <BUILD_ID>

# Filter by platform
pwsh .github/skills/pr-build-status/scripts/Get-HelixLogs.ps1 -BuildId <BUILD_ID> -Platform Windows

# Show console log content for failed work items
pwsh .github/skills/pr-build-status/scripts/Get-HelixLogs.ps1 -BuildId <BUILD_ID> -ShowConsoleLog

# Filter by work item name and show more log lines
pwsh .github/skills/pr-build-status/scripts/Get-HelixLogs.ps1 -BuildId <BUILD_ID> -WorkItem "*Lifecycle*" -ShowConsoleLog -TailLines 200
```

## Workflow

### Standard Build Failures
1. Get build IDs: `Get-PrBuildIds.ps1 -PrNumber XXXXX`
2. For each build, get status: `Get-BuildInfo.ps1 -BuildId YYYYY -FailedOnly`
3. For failed builds, get errors: `Get-BuildErrors.ps1 -BuildId YYYYY`

### Helix Test Failures
1. Get build IDs: `Get-PrBuildIds.ps1 -PrNumber XXXXX`
2. Find the build with Helix jobs (e.g., `maui-pr-devicetests`, `maui-integration-tests`)
3. Get Helix logs: `Get-HelixLogs.ps1 -BuildId YYYYY -ShowConsoleLog`
4. For specific platform: `Get-HelixLogs.ps1 -BuildId YYYYY -Platform Windows -ShowConsoleLog`

## Understanding Helix Logs

Helix is the .NET engineering infrastructure that runs tests across multiple platforms and device types. Tests that run on Helix include:
- **Device tests** - Run on real devices/emulators (iOS, Android, Windows, MacCatalyst)
- **Integration tests** - Run on various OS configurations
- **Other distributed tests** - Any test scenario that requires Helix infrastructure

When Helix tests fail:

1. **Build stage** - Compiles and packages the test app/harness
2. **Helix submission** - Sends the work items to Helix queues
3. **Work item execution** - Helix runs the tests on target machines/devices
4. **Console log** - Contains stdout/stderr from the test execution

The `Get-HelixLogs.ps1` script retrieves the console logs which show:
- Test execution output
- Any crashes or errors
- Infrastructure issues (timeouts, installation failures, etc.)

## Common Helix Failure Patterns

| Pattern in Console Log | Meaning |
|------------------------|---------|
| "XHarness timeout" | Test took too long, killed by infrastructure |
| "No test result files found" | Tests never ran or process crashed |
| "error MT..." or "error BL..." | Build/linking error (check build logs instead) |
| Exit code non-zero | Test failures or infrastructure issues |

## Prerequisites

- `gh` (GitHub CLI) - authenticated
- `pwsh` (PowerShell 7+)
