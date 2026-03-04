---
name: pr-build-status
description: "Investigate CI failures for dotnet/maui PRs — build errors, Helix test logs, and binlog analysis. Use when asked about failing checks, CI status, test failures, 'why is CI red', 'build failed', 'what's failing on PR', Helix failures, or device test failures."
metadata:
  author: dotnet-maui
  version: "1.2"
compatibility: Requires GitHub CLI (gh) authenticated with access to dotnet/maui repository.
---

# PR Build Status Skill

Investigate CI failures for dotnet/maui PRs — build errors, Helix test logs, and binlog analysis.

## Tools Required

This skill uses `bash` together with `pwsh` (PowerShell 7+) to run the PowerShell scripts. No file editing or other tools are required.

**If `gh` or `pwsh` is missing: stop immediately and tell the user to install the missing tool. Do NOT attempt to install it yourself.**

- `gh`: https://cli.github.com/
- `pwsh`: https://aka.ms/install-powershell

Optional for binlog analysis (MSBuild failures):
- `binlogtool`: `dotnet tool install -g binlogtool` (https://www.nuget.org/packages/binlogtool)

## When to Use

- User asks about CI/CD status for a PR
- User asks about failed checks or builds
- User asks "what's failing on PR #XXXXX" / "why is CI red" / "build failed"
- User wants to see test results
- **User asks about Helix failures (device tests, integration tests, etc.)**
- **User needs to debug why tests are failing on Helix infrastructure**
- **Text logs say "Build FAILED" with no detail — use binlog analysis**

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

> **Focus on the first error chronologically — later errors usually cascade from the root cause.**

### Standard Build Failures
1. Get build IDs: `Get-PrBuildIds.ps1 -PrNumber XXXXX`
   - If output shows ⚠️ with no build IDs, CI was not triggered — read the diagnostic message
2. For each build, get status: `Get-BuildInfo.ps1 -BuildId YYYYY -FailedOnly`
3. For failed builds, get errors: `Get-BuildErrors.ps1 -BuildId YYYYY`
4. If errors say "Build FAILED" with no detail, check for binlog artifacts (see below)

### Helix Test Failures
1. Get build IDs: `Get-PrBuildIds.ps1 -PrNumber XXXXX`
2. Find the build with Helix jobs (e.g., `maui-pr-devicetests`, `maui-integration-tests`)
3. Get Helix logs: `Get-HelixLogs.ps1 -BuildId YYYYY -ShowConsoleLog`
4. For specific platform: `Get-HelixLogs.ps1 -BuildId YYYYY -Platform Windows -ShowConsoleLog`

### Binlog Analysis (MSBuild/XamlC/NuGet failures)
When text logs are inconclusive, `.binlog` artifacts contain the full MSBuild structured log.

**Requires `binlogtool`** (`dotnet tool install -g binlogtool`). If not installed, tell the user and stop.

```bash
# Download the binlog artifact
az pipelines runs artifact download --run-id BUILD_ID --artifact-name "binlog" --path /tmp/maui-binlog \
  --org https://dev.azure.com/dnceng-public --project public

# Search for errors (broad first, then narrow)
binlogtool search "/tmp/maui-binlog/*.binlog" "error"
binlogtool search "/tmp/maui-binlog/*.binlog" "error CS"    # C# compiler
binlogtool search "/tmp/maui-binlog/*.binlog" "error NU"    # NuGet
binlogtool search "/tmp/maui-binlog/*.binlog" "XamlC"       # XAML compiler
binlogtool search "/tmp/maui-binlog/*.binlog" "XA"          # Android build errors

# Clean up
Remove-Item -Recurse -Force /tmp/maui-binlog
```

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
