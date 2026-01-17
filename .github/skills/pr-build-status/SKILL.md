---
name: pr-build-status
description: "Retrieve Azure DevOps build information for GitHub Pull Requests, including build IDs, stage status, failed jobs, and Helix console logs for device test failures."
metadata:
  author: dotnet-maui
  version: "1.1"
compatibility: Requires GitHub CLI (gh) authenticated with access to dotnet/maui repository.
---

# PR Build Status Skill

Retrieve Azure DevOps build information for GitHub Pull Requests, including Helix device test logs.

## Tools Required

This skill uses `bash` together with `pwsh` (PowerShell 7+) to run the PowerShell scripts. No file editing or other tools are required.

## When to Use

- User asks about CI/CD status for a PR
- User asks about failed checks or builds
- User asks "what's failing on PR #XXXXX"
- User wants to see test results
- **User asks about Helix failures or device test failures**
- **User needs to debug why device tests are failing on iOS/Android/Windows**

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

### 4. Get Helix Console Logs (Device Tests)
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

### Device Test / Helix Failures
1. Get build IDs: `Get-PrBuildIds.ps1 -PrNumber XXXXX`
2. Find the `maui-pr-devicetests` build
3. Get Helix logs: `Get-HelixLogs.ps1 -BuildId YYYYY -ShowConsoleLog`
4. For specific platform: `Get-HelixLogs.ps1 -BuildId YYYYY -Platform Windows -ShowConsoleLog`

## Understanding Helix Logs

Helix is the infrastructure that runs device tests on real devices/emulators. When device tests fail:

1. **Build stage** - Compiles and packages the test app
2. **Helix submission** - Sends the package to Helix queues
3. **Work item execution** - Helix runs the tests on target devices
4. **Console log** - Contains stdout/stderr from the test execution

The `Get-HelixLogs.ps1` script retrieves the console logs which show:
- App installation status
- App launch status
- Test execution output
- Any crashes or errors

## Common Helix Failure Patterns

| Pattern in Console Log | Meaning |
|------------------------|---------|
| "Package installed successfully" then silence | App installed but crashed on startup |
| "Category file not found" | App started but UI didn't load properly |
| "No test result files found" | Tests never ran or app crashed |
| "XHarness timeout" | Test took too long, killed by infrastructure |
| "error MT..." or "error BL..." | Build/linking error (check build logs instead) |

## Prerequisites

- `gh` (GitHub CLI) - authenticated
- `pwsh` (PowerShell 7+)
