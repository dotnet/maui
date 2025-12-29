---
name: pr-build-status
description: "Retrieve Azure DevOps build information for GitHub Pull Requests, including build IDs, stage status, and failed jobs."
---

# PR Build Status Skill

Retrieve Azure DevOps build information for GitHub Pull Requests.

## Tools Required

This skill only needs `bash` to run PowerShell scripts. No file editing or other tools required.

## When to Use

- User asks about CI/CD status for a PR
- User asks about failed checks or builds
- User asks "what's failing on PR #XXXXX"
- User wants to see test results

## Scripts

All scripts are in `.github/skills/pr-build-status/`

### 1. Get Build IDs for a PR
```bash
pwsh .github/skills/pr-build-status/Get-PrBuildIds.ps1 -PrNumber <PR_NUMBER>
```

### 2. Get Build Status
```bash
pwsh .github/skills/pr-build-status/Get-BuildInfo.ps1 -BuildId <BUILD_ID>
# For failed jobs only:
pwsh .github/skills/pr-build-status/Get-BuildInfo.ps1 -BuildId <BUILD_ID> -FailedOnly
```

### 3. Get Test Results
```bash
pwsh .github/skills/pr-build-status/Get-TestResults.ps1 -BuildId <BUILD_ID> -FailedOnly
```

## Workflow

1. Get build IDs: `Get-PrBuildIds.ps1 -PrNumber XXXXX`
2. For each build, get status: `Get-BuildInfo.ps1 -BuildId YYYYY`
3. For UI test builds with failures, get test details: `Get-TestResults.ps1 -BuildId YYYYY -FailedOnly`

## Prerequisites

- `gh` (GitHub CLI) - authenticated
- `pwsh` (PowerShell 7+)
