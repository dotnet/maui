---
name: pr-build-status
description: "Retrieve Azure DevOps build information for GitHub Pull Requests, including build IDs, stage status, and failed jobs."
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires GitHub CLI (gh) authenticated with access to dotnet/maui repository.
---

# PR Build Status Skill

Retrieve Azure DevOps build information for GitHub Pull Requests.

## Tools Required

This skill uses `bash` together with `pwsh` (PowerShell 7+) to run the PowerShell scripts. No file editing or other tools are required.

## When to Use

- User asks about CI/CD status for a PR
- User asks about failed checks or builds
- User asks "what's failing on PR #XXXXX"
- User wants to see test results

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

## Workflow

1. Get build IDs: `scripts/Get-PrBuildIds.ps1 -PrNumber XXXXX`
2. For each build, get status: `scripts/Get-BuildInfo.ps1 -BuildId YYYYY`
3. For failed builds, get error details: `scripts/Get-BuildErrors.ps1 -BuildId YYYYY`

## Prerequisites

- `gh` (GitHub CLI) - authenticated
- `pwsh` (PowerShell 7+)
