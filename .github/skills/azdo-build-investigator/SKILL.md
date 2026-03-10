---
name: azdo-build-investigator
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
- `az` (Azure CLI): `brew install azure-cli` / `winget install Microsoft.AzureCLI`, then `az extension add --name azure-devops`
- `binlogtool`: `dotnet tool install -g binlogtool` (https://www.nuget.org/packages/binlogtool)

## Scripts

All scripts are in `.github/skills/azdo-build-investigator/scripts/`

### 1. Get Build IDs for a PR
```bash
pwsh .github/skills/azdo-build-investigator/scripts/Get-PrBuildIds.ps1 -PrNumber <PR_NUMBER>
```

### 2. Get Build Status
```bash
pwsh .github/skills/azdo-build-investigator/scripts/Get-BuildInfo.ps1 -BuildId <BUILD_ID>
# For failed jobs only:
pwsh .github/skills/azdo-build-investigator/scripts/Get-BuildInfo.ps1 -BuildId <BUILD_ID> -FailedOnly
```

### 3. Get Build Errors and Test Failures
```bash
# Get all errors (build errors + test failures)
pwsh .github/skills/azdo-build-investigator/scripts/Get-BuildErrors.ps1 -BuildId <BUILD_ID>

# Get only build/compilation errors
pwsh .github/skills/azdo-build-investigator/scripts/Get-BuildErrors.ps1 -BuildId <BUILD_ID> -ErrorsOnly

# Get only test failures
pwsh .github/skills/azdo-build-investigator/scripts/Get-BuildErrors.ps1 -BuildId <BUILD_ID> -TestsOnly
```

### 4. Get Helix Console Logs
```bash
# List all Helix work items and their status
pwsh .github/skills/azdo-build-investigator/scripts/Get-HelixLogs.ps1 -BuildId <BUILD_ID>

# Filter by platform
pwsh .github/skills/azdo-build-investigator/scripts/Get-HelixLogs.ps1 -BuildId <BUILD_ID> -Platform Windows

# Show console log content for failed work items
pwsh .github/skills/azdo-build-investigator/scripts/Get-HelixLogs.ps1 -BuildId <BUILD_ID> -ShowConsoleLog

# Filter by work item name and show more log lines
pwsh .github/skills/azdo-build-investigator/scripts/Get-HelixLogs.ps1 -BuildId <BUILD_ID> -WorkItem "*Lifecycle*" -ShowConsoleLog -TailLines 200
```

## Workflow

> **Focus on the first error chronologically — later errors usually cascade from the root cause.**

**Multiple pipelines**: PRs trigger multiple builds. Investigate in priority order:
1. **`maui-pr`** (main build) — check first, most failures here
2. **`maui-pr-devicetests`** — if device test failures
3. **`maui-pr-uitests`** — if UI test failures

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

**When to use binlog analysis**:
- ✅ `Get-BuildErrors` returns generic "Build FAILED" with no error messages
- ✅ Errors mention MSBuild, XamlC, or NuGet restore issues
- ✅ Error says "See binlog for details"
- ❌ Helix test failures (use `Get-HelixLogs` instead)
- ❌ Clear error messages already visible in build logs

`.binlog` files are MSBuild structured logs inside Container-type build artifacts (e.g., `Windows_NT_Build Windows (Debug)_Attempt1`).

**Requires**:
- `az` CLI logged in (`az login`) — needed to get Bearer token for Container artifact API
- `binlogtool` — `dotnet tool install -g binlogtool` (https://www.nuget.org/packages/binlogtool)

If either is missing, tell the user and stop.

**Step 1: List available artifacts** (no auth needed):
```bash
az pipelines runs artifact list --run-id BUILD_ID \
  --org https://dev.azure.com/dnceng-public --project public --detect false \
  --query "[].{name:name, type:resource.type}" -o table
```

Build log artifacts are Container-type and named like `Windows_NT_Build Windows (Debug)_Attempt1` or `Darwin_Build macOS (Debug)_Attempt1`.

**Step 2: Download binlogs from a Container artifact**:
```bash
# Download all binlogs from the build (requires az login)
pwsh .github/skills/azdo-build-investigator/scripts/Get-BuildBinlogs.ps1 -BuildId BUILD_ID

# Download from a specific artifact
pwsh .github/skills/azdo-build-investigator/scripts/Get-BuildBinlogs.ps1 -BuildId BUILD_ID -ArtifactName "*Windows*Build*"

# Custom output directory
pwsh .github/skills/azdo-build-investigator/scripts/Get-BuildBinlogs.ps1 -BuildId BUILD_ID -OutputDir /tmp/mybinlogs
```

**Step 3: Analyze with binlogtool**:
```bash
# Search for errors (broad first, then narrow)
binlogtool search "/tmp/maui-binlogs/*.binlog" "error"
binlogtool search "/tmp/maui-binlogs/*.binlog" "error CS"    # C# compiler
binlogtool search "/tmp/maui-binlogs/*.binlog" "error NU"    # NuGet
binlogtool search "/tmp/maui-binlogs/*.binlog" "XamlC"       # XAML compiler
binlogtool search "/tmp/maui-binlogs/*.binlog" "XA"          # Android build errors

# Reconstruct full text log (useful for context around an error)
binlogtool reconstruct "/tmp/maui-binlogs/*.binlog" > /tmp/maui-build.log

# Detect double-write errors
binlogtool doublewrites "/tmp/maui-binlogs/*.binlog"

# Clean up when done
rm -rf /tmp/maui-binlogs
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

## Common Build Error Patterns

| Pattern | Area | Notes |
|---------|------|-------|
| `error CS####` | C# compiler | Root cause; check file/line reference |
| `error NU1###` | NuGet restore | NU1301 = feed unreachable; NU11## = resolution failure |
| `XamlC` | XAML compiler | MAUI-specific; usually missing type or invalid binding |
| `##[error]` | ADO infrastructure | Pipeline-level error, not a build error |
| `System.TimeoutException` | Test infra | Infrastructure timeout; may be transient |
| `error MT####` | iOS/Mac linker | Linking failure; check build logs |
| `error BL####` | Build logic | MSBuild task failure |

## Common Helix Failure Patterns

| Pattern in Console Log | Meaning |
|------------------------|---------|
| "XHarness timeout" | Test took too long, killed by infrastructure |
| "No test result files found" | Tests never ran or process crashed |
| "error MT..." or "error BL..." | Build/linking error (check build logs instead) |
| Exit code non-zero | Test failures or infrastructure issues |
